using ClinicManagementSystem.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Customer.Controllers
{
    // Read-only: patients can view their own records and prescriptions, never edit.
    public class MedicalRecordController : PatientBaseController
    {
        private readonly IRepository<MedicalRecord> _recordRepo;

        public MedicalRecordController(UserManager<ApplicationUser> userManager, IRepository<Patient> patientRepo,
            IRepository<MedicalRecord> recordRepo)
            : base(userManager, patientRepo)
        {
            _recordRepo = recordRepo;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Page"] = "Records";

            var patient = await GetCurrentPatientAsync();
            if (patient == null)
            {
                TempData["Warning"] = "Please complete your medical profile first.";
                return RedirectToAction("Complete", "Profile");
            }

            var records = await _recordRepo.GetAllAsync(r => r.PatientId == patient.Id, tracked: false,
                includes: [r => r.Appointment, r => r.Appointment.Doctor, r => r.Prescriptions]);

            return View(records.OrderByDescending(r => r.CreatedAt));
        }
    }
}
