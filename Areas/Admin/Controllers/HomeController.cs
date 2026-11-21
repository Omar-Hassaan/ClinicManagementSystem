using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Areas.Admin.Controllers
{
    [Area(CD.AdminArea)]
    [Authorize(Roles = CD.SuperAdmin + "," + CD.Admin)]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole>    _roleManager;
        private readonly IRepository<Clinic>          _clinicRepo;
        private readonly IRepository<Doctor>          _doctorRepo;
        private readonly IRepository<Patient>         _patientRepo;
        private readonly IRepository<Appointment>     _appointmentRepo;
        private readonly IRepository<MedicalRecord>   _medicalRecordRepo;
        private readonly IRepository<Prescription>    _prescriptionRepo;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole>    roleManager,
            IRepository<Clinic>          clinicRepo,
            IRepository<Doctor>          doctorRepo,
            IRepository<Patient>         patientRepo,
            IRepository<Appointment>     appointmentRepo,
            IRepository<MedicalRecord>   medicalRecordRepo,
            IRepository<Prescription>    prescriptionRepo)
        {
            _userManager       = userManager;
            _roleManager       = roleManager;
            _clinicRepo        = clinicRepo;
            _doctorRepo        = doctorRepo;
            _patientRepo       = patientRepo;
            _appointmentRepo   = appointmentRepo;
            _medicalRecordRepo = medicalRecordRepo;
            _prescriptionRepo  = prescriptionRepo;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Page"] = "Dashboard";

            var clinics       = (await _clinicRepo.GetAllAsync(tracked: false)).ToList();
            var doctors       = (await _doctorRepo.GetAllAsync(tracked: false)).ToList();
            var patients      = (await _patientRepo.GetAllAsync(tracked: false)).ToList();
            var appointments  = (await _appointmentRepo.GetAllAsync(tracked: false)).ToList();
            var records       = (await _medicalRecordRepo.GetAllAsync(tracked: false)).ToList();
            var prescriptions = (await _prescriptionRepo.GetAllAsync(tracked: false)).ToList();

            // Roles pie chart — materialize the roles query FIRST (ToListAsync)
            // before issuing further queries on the same DbContext, otherwise
            // EF Core throws "a second operation was started on this context
            // before a previous operation completed" because the live
            // IQueryable keeps a reader open while looping.
            var allRoles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();

            var roleLabels = new List<string>();
            var roleCounts = new List<int>();

            foreach (var role in allRoles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                roleLabels.Add(role.Name!);
                roleCounts.Add(usersInRole.Count);
            }

            // Line chart — appointments over the last 6 months
            var now = DateTime.Now;
            var monthLabels = new List<string>();
            var monthCounts = new List<int>();

            for (int i = 5; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                monthLabels.Add(month.ToString("MMM yyyy"));
                monthCounts.Add(appointments.Count(a => a.AppointmentDate.Year == month.Year && a.AppointmentDate.Month == month.Month));
            }

            // Bar chart — doctors per clinic
            var clinicLabels = clinics.Select(c => c.Name).ToList();
            var doctorsPerClinic = clinics.Select(c => doctors.Count(d => d.ClinicId == c.Id)).ToList();

            // Status pie chart
            var statusLabels = Enum.GetNames(typeof(AppointmentStatus)).ToList();
            var statusCounts = Enum.GetValues<AppointmentStatus>()
                .Select(s => appointments.Count(a => a.Status == s)).ToList();

            var vm = new DashboardVM
            {
                TotalUsers        = _userManager.Users.Count(),
                TotalDoctors      = doctors.Count,
                TotalPatients     = patients.Count,
                TotalClinics      = clinics.Count,
                TotalAppointments = appointments.Count,
                PendingAppointments   = appointments.Count(a => a.Status == AppointmentStatus.Pending),
                CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                TotalMedicalRecords= records.Count,
                TotalPrescriptions = prescriptions.Count,

                MonthLabels        = monthLabels,
                MonthlyAppointments= monthCounts,
                ClinicLabels       = clinicLabels,
                DoctorsPerClinic   = doctorsPerClinic,
                RoleLabels         = roleLabels,
                RoleCounts         = roleCounts,
                StatusLabels       = statusLabels,
                StatusCounts       = statusCounts
            };

            return View(vm);
        }
    }
}
