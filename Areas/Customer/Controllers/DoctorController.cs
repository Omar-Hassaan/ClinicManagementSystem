using ClinicManagementSystem.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Customer.Controllers
{
    public class DoctorController : PatientBaseController
    {
        private readonly IRepository<Doctor> _doctorRepo;
        private readonly IRepository<Clinic> _clinicRepo;
        private readonly IRepository<DoctorSchedule> _scheduleRepo;

        public DoctorController(UserManager<ApplicationUser> userManager, IRepository<Patient> patientRepo,
            IRepository<Doctor> doctorRepo, IRepository<Clinic> clinicRepo, IRepository<DoctorSchedule> scheduleRepo)
            : base(userManager, patientRepo)
        {
            _doctorRepo = doctorRepo;
            _clinicRepo = clinicRepo;
            _scheduleRepo = scheduleRepo;
        }

        public async Task<IActionResult> Index(string? search, int? clinicId)
        {
            ViewData["Page"] = "Doctors";

            var doctors = await _doctorRepo.GetAllAsync(
                d => d.IsActive
                     && (string.IsNullOrEmpty(search) || d.FullName.Contains(search) || d.Specialization.Contains(search))
                     && (!clinicId.HasValue || d.ClinicId == clinicId.Value),
                tracked: false, includes: [d => d.Clinic]);

            ViewBag.Search = search;
            ViewBag.ClinicId = clinicId;
            ViewBag.Clinics = (await _clinicRepo.GetAllAsync(tracked: false)).OrderBy(c => c.Name).ToList();

            return View(doctors.OrderBy(d => d.FullName));
        }

        public async Task<IActionResult> Details(int id)
        {
            ViewData["Page"] = "Doctors";

            var doctor = await _doctorRepo.GetOneAsync(d => d.Id == id, tracked: false, includes: [d => d.Clinic]);
            if (doctor == null) return NotFound();

            var today = DateTime.Today.DayOfWeek;
            var schedules = await _scheduleRepo.GetAllAsync(s => s.DoctorId == id && s.IsActive, tracked: false);

            ViewBag.Schedules = schedules.OrderBy(s => s.DayOfWeek).ToList();
            return View(doctor);
        }
    }
}
