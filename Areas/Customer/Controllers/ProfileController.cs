using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.ViewModels.Customer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Customer.Controllers
{
    public class ProfileController : PatientBaseController
    {
        private readonly IRepository<Patient> _patientRepo;

        public ProfileController(UserManager<ApplicationUser> userManager, IRepository<Patient> patientRepo)
            : base(userManager, patientRepo)
        {
            _patientRepo = patientRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Complete()
        {
            var existing = await GetCurrentPatientAsync();
            if (existing != null) return RedirectToAction("Index", "Home");

            return View(new CompleteProfileVM());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(CompleteProfileVM vm)
        {
            var existing = await GetCurrentPatientAsync();
            if (existing != null) return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid) return View(vm);

            var user = await UserManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var patient = new Patient
            {
                ApplicationUserId = user.Id, FullName = user.FullName,
                Gender = vm.Gender, DateOfBirth = vm.DateOfBirth, Age = vm.Age,
                BloodType = vm.BloodType, Allergies = vm.Allergies, ChronicDiseases = vm.ChronicDiseases,
                EmergencyContactName = vm.EmergencyContactName, EmergencyContactPhone = vm.EmergencyContactPhone
            };

            await _patientRepo.AddAsync(patient);
            await _patientRepo.SaveAsync();

            TempData["Success"] = "Welcome! Your medical profile has been created.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var patient = await GetCurrentPatientAsync(p => p.ApplicationUser);
            if (patient == null) return RedirectToAction(nameof(Complete));

            return View(patient);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null) return RedirectToAction(nameof(Complete));

            return View(new CompleteProfileVM
            {
                Gender = patient.Gender, DateOfBirth = patient.DateOfBirth, Age = patient.Age,
                BloodType = patient.BloodType, Allergies = patient.Allergies, ChronicDiseases = patient.ChronicDiseases,
                EmergencyContactName = patient.EmergencyContactName, EmergencyContactPhone = patient.EmergencyContactPhone
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CompleteProfileVM vm)
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null) return RedirectToAction(nameof(Complete));

            if (!ModelState.IsValid) return View(vm);

            patient.Gender = vm.Gender; patient.DateOfBirth = vm.DateOfBirth; patient.Age = vm.Age;
            patient.BloodType = vm.BloodType; patient.Allergies = vm.Allergies; patient.ChronicDiseases = vm.ChronicDiseases;
            patient.EmergencyContactName = vm.EmergencyContactName; patient.EmergencyContactPhone = vm.EmergencyContactPhone;

            _patientRepo.Update(patient);
            await _patientRepo.SaveAsync();

            TempData["Success"] = "Medical profile updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
