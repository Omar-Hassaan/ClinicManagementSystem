using ClinicManagementSystem.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Customer.Controllers
{
    public class HomeController : PatientBaseController
    {
        private readonly IRepository<Appointment> _appointmentRepo;
        private readonly IRepository<MedicalRecord> _recordRepo;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            IRepository<Patient> patientRepo,
            IRepository<Appointment> appointmentRepo,
            IRepository<MedicalRecord> recordRepo)
            : base(userManager, patientRepo)
        {
            _appointmentRepo = appointmentRepo;
            _recordRepo = recordRepo;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Page"] = "Home";

            var patient = await GetCurrentPatientAsync(p => p.ApplicationUser);
            if (patient == null) return RedirectToAction("Complete", "Profile");

            var appointments = await _appointmentRepo.GetAllAsync(a => a.PatientId == patient.Id, tracked: false,
                includes: [a => a.Doctor, a => a.Doctor.Clinic]);

            var upcoming = appointments.Where(a => a.AppointmentDate >= DateTime.Now
                                                 && a.Status != AppointmentStatus.Cancelled)
                                        .OrderBy(a => a.AppointmentDate).ToList();

            var recordsCount = (await _recordRepo.GetAllAsync(r => r.PatientId == patient.Id, tracked: false)).Count();

            ViewBag.Patient = patient;
            ViewBag.TotalAppointments = appointments.Count();
            ViewBag.UpcomingCount = upcoming.Count;
            ViewBag.NextAppointment = upcoming.FirstOrDefault();
            ViewBag.RecordsCount = recordsCount;

            return View();
        }
    }
}
