using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Admin.Controllers
{
    [Area(CD.AdminArea)]
    [Authorize(Roles = CD.SuperAdmin + "," + CD.Admin)]
    public class PatientController : Controller
    {
        private readonly IRepository<Patient> _repo;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientController(IRepository<Patient> repo, UserManager<ApplicationUser> userManager)
        {
            _repo = repo;
            _userManager = userManager;
        }

        private async Task<List<ApplicationUser>> GetAvailableUsersAsync()
        {
            var patientUsers = await _userManager.GetUsersInRoleAsync(CD.Patient);
            var existing = await _repo.GetAllAsync(tracked: false);
            var used = existing.Select(p => p.ApplicationUserId).ToHashSet();
            return patientUsers.Where(u => !used.Contains(u.Id)).ToList();
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Page"] = "Patients";
            var patients = await _repo.GetAllAsync(tracked: false, includes: [p => p.ApplicationUser]);
            return View(patients.OrderBy(p => p.FullName));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Page"] = "Patients";
            ViewBag.Users = await GetAvailableUsersAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientVM vm)
        {
            ViewData["Page"] = "Patients";

            if (string.IsNullOrEmpty(vm.ApplicationUserId))
                ModelState.AddModelError("ApplicationUserId", "Please select a user.");

            if (!ModelState.IsValid) { ViewBag.Users = await GetAvailableUsersAsync(); return View(vm); }

            var user = await _userManager.FindByIdAsync(vm.ApplicationUserId!);
            if (user == null) return NotFound();

            var patient = new Patient
            {
                ApplicationUserId = vm.ApplicationUserId!,
                FullName = user.FullName,
                Gender = vm.Gender, DateOfBirth = vm.DateOfBirth, Age = vm.Age,
                BloodType = vm.BloodType, Allergies = vm.Allergies, ChronicDiseases = vm.ChronicDiseases,
                EmergencyContactName = vm.EmergencyContactName, EmergencyContactPhone = vm.EmergencyContactPhone
            };

            await _repo.AddAsync(patient);
            await _repo.SaveAsync();

            TempData["Success"] = "Patient profile created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Page"] = "Patients";
            var patient = await _repo.GetOneAsync(p => p.Id == id, includes: [p => p.ApplicationUser]);
            if (patient == null) return NotFound();

            ViewBag.UserName = $"{patient.ApplicationUser.FullName} ({patient.ApplicationUser.Email})";
            ViewBag.PatientId = patient.Id;

            return View(new PatientVM
            {
                Gender = patient.Gender, DateOfBirth = patient.DateOfBirth, Age = patient.Age,
                BloodType = patient.BloodType, Allergies = patient.Allergies, ChronicDiseases = patient.ChronicDiseases,
                EmergencyContactName = patient.EmergencyContactName, EmergencyContactPhone = patient.EmergencyContactPhone
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PatientVM vm)
        {
            ViewData["Page"] = "Patients";
            var patient = await _repo.GetOneAsync(p => p.Id == id, includes: [p => p.ApplicationUser]);
            if (patient == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.UserName = $"{patient.ApplicationUser.FullName} ({patient.ApplicationUser.Email})";
                ViewBag.PatientId = id;
                return View(vm);
            }

            patient.Gender = vm.Gender; patient.DateOfBirth = vm.DateOfBirth; patient.Age = vm.Age;
            patient.BloodType = vm.BloodType; patient.Allergies = vm.Allergies; patient.ChronicDiseases = vm.ChronicDiseases;
            patient.EmergencyContactName = vm.EmergencyContactName; patient.EmergencyContactPhone = vm.EmergencyContactPhone;

            _repo.Update(patient);
            await _repo.SaveAsync();

            TempData["Success"] = "Patient updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Page"] = "Patients";
            var patient = await _repo.GetOneAsync(p => p.Id == id, tracked: false,
                includes: [p => p.ApplicationUser, p => p.Appointments, p => p.MedicalRecords]);
            if (patient == null) return NotFound();
            return View(patient);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _repo.GetOneAsync(p => p.Id == id, includes: [p => p.Appointments]);
            if (patient == null) return NotFound();

            if (patient.Appointments.Any())
            {
                TempData["Error"] = "Cannot delete a patient who has appointment history.";
                return RedirectToAction(nameof(Index));
            }

            _repo.Delete(patient);
            await _repo.SaveAsync();

            TempData["Success"] = "Patient deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
