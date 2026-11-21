using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Admin.Controllers
{
    [Area(CD.AdminArea)]
    [Authorize(Roles = CD.SuperAdmin + "," + CD.Admin)]
    public class MedicalRecordController : Controller
    {
        private readonly IRepository<MedicalRecord> _repo;
        private readonly IRepository<Appointment> _appointmentRepo;

        public MedicalRecordController(IRepository<MedicalRecord> repo, IRepository<Appointment> appointmentRepo)
        {
            _repo = repo;
            _appointmentRepo = appointmentRepo;
        }

        private async Task LoadAppointmentsAsync()
        {
            var appts = await _appointmentRepo.GetAllAsync(tracked: false, includes: [a => a.Doctor, a => a.Patient, a => a.MedicalRecord!]);
            // Only appointments that don't already have a record can be picked for a NEW record
            ViewBag.Appointments = appts.Where(a => a.MedicalRecord == null).OrderByDescending(a => a.AppointmentDate).ToList();
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Page"] = "MedicalRecords";
            var records = await _repo.GetAllAsync(tracked: false, includes: [r => r.Patient, r => r.Appointment, r => r.Appointment.Doctor]);
            return View(records.OrderByDescending(r => r.CreatedAt));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Page"] = "MedicalRecords";
            await LoadAppointmentsAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicalRecordVM vm)
        {
            ViewData["Page"] = "MedicalRecords";
            if (!ModelState.IsValid) { await LoadAppointmentsAsync(); return View(vm); }

            var appointment = await _appointmentRepo.GetOneAsync(a => a.Id == vm.AppointmentId);
            if (appointment == null) return NotFound();

            var record = new MedicalRecord
            {
                AppointmentId = vm.AppointmentId, PatientId = appointment.PatientId,
                Diagnosis = vm.Diagnosis, Symptoms = vm.Symptoms, TreatmentPlan = vm.TreatmentPlan,
                DoctorNotes = vm.DoctorNotes, Weight = vm.Weight, Height = vm.Height,
                BloodPressure = vm.BloodPressure, Temperature = vm.Temperature
            };

            await _repo.AddAsync(record);

            // Mark the appointment as completed once a record is attached
            appointment.Status = AppointmentStatus.Completed;
            _appointmentRepo.Update(appointment);

            await _repo.SaveAsync();

            TempData["Success"] = "Medical record created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Page"] = "MedicalRecords";
            var record = await _repo.GetOneAsync(r => r.Id == id, includes: [r => r.Patient, r => r.Appointment, r => r.Appointment.Doctor]);
            if (record == null) return NotFound();

            ViewBag.RecordId = record.Id;
            ViewBag.AppointmentLabel = $"{record.Patient.FullName} with Dr. {record.Appointment.Doctor.FullName} — {record.Appointment.AppointmentDate:g}";

            return View(new MedicalRecordVM
            {
                AppointmentId = record.AppointmentId, PatientId = record.PatientId,
                Diagnosis = record.Diagnosis, Symptoms = record.Symptoms, TreatmentPlan = record.TreatmentPlan,
                DoctorNotes = record.DoctorNotes, Weight = record.Weight, Height = record.Height,
                BloodPressure = record.BloodPressure, Temperature = record.Temperature
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicalRecordVM vm)
        {
            ViewData["Page"] = "MedicalRecords";
            var record = await _repo.GetOneAsync(r => r.Id == id, includes: [r => r.Patient, r => r.Appointment, r => r.Appointment.Doctor]);
            if (record == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.RecordId = id;
                ViewBag.AppointmentLabel = $"{record.Patient.FullName} with Dr. {record.Appointment.Doctor.FullName} — {record.Appointment.AppointmentDate:g}";
                return View(vm);
            }

            record.Diagnosis = vm.Diagnosis; record.Symptoms = vm.Symptoms; record.TreatmentPlan = vm.TreatmentPlan;
            record.DoctorNotes = vm.DoctorNotes; record.Weight = vm.Weight; record.Height = vm.Height;
            record.BloodPressure = vm.BloodPressure; record.Temperature = vm.Temperature;

            _repo.Update(record);
            await _repo.SaveAsync();

            TempData["Success"] = "Medical record updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Page"] = "MedicalRecords";
            var record = await _repo.GetOneAsync(r => r.Id == id, tracked: false,
                includes: [r => r.Patient, r => r.Appointment, r => r.Appointment.Doctor, r => r.Prescriptions]);
            if (record == null) return NotFound();
            return View(record);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _repo.GetOneAsync(r => r.Id == id, includes: [r => r.Prescriptions]);
            if (record == null) return NotFound();

            if (record.Prescriptions.Any())
            {
                TempData["Error"] = "Cannot delete a record that still has prescriptions attached.";
                return RedirectToAction(nameof(Index));
            }

            _repo.Delete(record);
            await _repo.SaveAsync();

            TempData["Success"] = "Medical record deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
