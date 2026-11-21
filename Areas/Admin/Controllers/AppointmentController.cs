using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Admin.Controllers
{
    [Area(CD.AdminArea)]
    [Authorize(Roles = CD.SuperAdmin + "," + CD.Admin)]
    public class AppointmentController : Controller
    {
        private readonly IRepository<Appointment> _repo;
        private readonly IRepository<Doctor> _doctorRepo;
        private readonly IRepository<Patient> _patientRepo;

        public AppointmentController(IRepository<Appointment> repo, IRepository<Doctor> doctorRepo, IRepository<Patient> patientRepo)
        {
            _repo = repo;
            _doctorRepo = doctorRepo;
            _patientRepo = patientRepo;
        }

        private async Task LoadDropdownsAsync()
        {
            ViewBag.Doctors = (await _doctorRepo.GetAllAsync(tracked: false)).OrderBy(d => d.FullName).ToList();
            ViewBag.Patients = (await _patientRepo.GetAllAsync(tracked: false)).OrderBy(p => p.FullName).ToList();
        }

        public async Task<IActionResult> Index(string? status)
        {
            ViewData["Page"] = "Appointments";

            var appointments = await _repo.GetAllAsync(tracked: false, includes: [a => a.Doctor, a => a.Patient]);
            var list = appointments.OrderByDescending(a => a.AppointmentDate).AsEnumerable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AppointmentStatus>(status, out var st))
                list = list.Where(a => a.Status == st);

            ViewBag.StatusFilter = status;
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Page"] = "Appointments";
            await LoadDropdownsAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentVM vm)
        {
            ViewData["Page"] = "Appointments";
            if (!ModelState.IsValid) { await LoadDropdownsAsync(); return View(vm); }

            var appointment = new Appointment
            {
                DoctorId = vm.DoctorId, PatientId = vm.PatientId,
                AppointmentDate = vm.AppointmentDate, Status = vm.Status, Notes = vm.Notes
            };

            await _repo.AddAsync(appointment);
            await _repo.SaveAsync();

            TempData["Success"] = "Appointment created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Page"] = "Appointments";
            var appt = await _repo.GetOneAsync(a => a.Id == id);
            if (appt == null) return NotFound();

            await LoadDropdownsAsync();
            ViewBag.AppointmentId = appt.Id;

            return View(new AppointmentVM
            {
                DoctorId = appt.DoctorId, PatientId = appt.PatientId,
                AppointmentDate = appt.AppointmentDate, Status = appt.Status, Notes = appt.Notes
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentVM vm)
        {
            ViewData["Page"] = "Appointments";
            var appt = await _repo.GetOneAsync(a => a.Id == id);
            if (appt == null) return NotFound();

            if (!ModelState.IsValid) { await LoadDropdownsAsync(); ViewBag.AppointmentId = id; return View(vm); }

            appt.DoctorId = vm.DoctorId; appt.PatientId = vm.PatientId;
            appt.AppointmentDate = vm.AppointmentDate; appt.Status = vm.Status; appt.Notes = vm.Notes;

            _repo.Update(appt);
            await _repo.SaveAsync();

            TempData["Success"] = "Appointment updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Page"] = "Appointments";
            var appt = await _repo.GetOneAsync(a => a.Id == id, tracked: false,
                includes: [a => a.Doctor, a => a.Patient, a => a.MedicalRecord!]);
            if (appt == null) return NotFound();
            return View(appt);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var appt = await _repo.GetOneAsync(a => a.Id == id, includes: [a => a.MedicalRecord!]);
            if (appt == null) return NotFound();

            if (appt.MedicalRecord != null)
            {
                TempData["Error"] = "Cannot delete an appointment that has a medical record attached.";
                return RedirectToAction(nameof(Index));
            }

            _repo.Delete(appt);
            await _repo.SaveAsync();

            TempData["Success"] = "Appointment deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
