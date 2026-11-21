using ClinicManagementSystem.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Customer.Controllers
{
    public class ClinicController : PatientBaseController
    {
        private readonly IRepository<Clinic> _clinicRepo;
        private readonly IRepository<Doctor> _doctorRepo;

        public ClinicController(UserManager<ApplicationUser> userManager, IRepository<Patient> patientRepo,
            IRepository<Clinic> clinicRepo, IRepository<Doctor> doctorRepo)
            : base(userManager, patientRepo)
        {
            _clinicRepo = clinicRepo;
            _doctorRepo = doctorRepo;
        }

        public async Task<IActionResult> Index(string? search)
        {
            ViewData["Page"] = "Clinics";

            var clinics = await _clinicRepo.GetAllAsync(
                c => c.IsActive && (string.IsNullOrEmpty(search) || c.Name.Contains(search) || c.Specialization.Contains(search)),
                tracked: false);

            ViewBag.Search = search;
            return View(clinics.OrderBy(c => c.Name));
        }

        public async Task<IActionResult> Details(int id)
        {
            ViewData["Page"] = "Clinics";

            var clinic = await _clinicRepo.GetOneAsync(c => c.Id == id, tracked: false);
            if (clinic == null) return NotFound();

            var doctors = await _doctorRepo.GetAllAsync(d => d.ClinicId == id && d.IsActive, tracked: false, includes: [d => d.Clinic]);
            ViewBag.Doctors = doctors.ToList();

            return View(clinic);
        }
    }
}
