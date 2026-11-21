using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.ViewModels.Customer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Customer.Controllers
{
    public class AppointmentController : PatientBaseController
    {
        private readonly IRepository<Appointment> _appointmentRepo;
        private readonly IRepository<Doctor> _doctorRepo;
        private readonly IRepository<DoctorSchedule> _scheduleRepo;

        private const int DaysAhead = 14; // how far ahead we generate bookable slots

        public AppointmentController(UserManager<ApplicationUser> userManager, IRepository<Patient> patientRepo,
            IRepository<Appointment> appointmentRepo, IRepository<Doctor> doctorRepo, IRepository<DoctorSchedule> scheduleRepo)
            : base(userManager, patientRepo)
        {
            _appointmentRepo = appointmentRepo;
            _doctorRepo = doctorRepo;
            _scheduleRepo = scheduleRepo;
        }

        // Builds bookable slots for the next `DaysAhead` days based on the
        // doctor's weekly recurring schedule, excluding slots already taken.
        private async Task<List<DateTime>> GetAvailableSlotsAsync(int doctorId)
        {
            var schedules = (await _scheduleRepo.GetAllAsync(s => s.DoctorId == doctorId && s.IsActive, tracked: false)).ToList();
            if (!schedules.Any()) return new List<DateTime>();

            var existing = await _appointmentRepo.GetAllAsync(
                a => a.DoctorId == doctorId && a.AppointmentDate >= DateTime.Now && a.Status != AppointmentStatus.Cancelled,
                tracked: false);

            var booked = existing.Select(a => a.AppointmentDate).ToHashSet();
            var slots = new List<DateTime>();

            for (int dayOffset = 0; dayOffset <= DaysAhead; dayOffset++)
            {
                var date = DateTime.Today.AddDays(dayOffset);
                var daySchedules = schedules.Where(s => s.DayOfWeek == date.DayOfWeek);

                foreach (var schedule in daySchedules)
                {
                    var slotStart = date.Add(schedule.StartTime.ToTimeSpan());
                    var slotEnd = date.Add(schedule.EndTime.ToTimeSpan());
                    var length = TimeSpan.FromMinutes(schedule.SlotDurationMinutes);

                    for (var slot = slotStart; slot + length <= slotEnd; slot = slot.Add(length))
                    {
                        if (slot > DateTime.Now && !booked.Contains(slot))
                            slots.Add(slot);
                    }
                }
            }

            return slots.OrderBy(s => s).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> Book(int doctorId)
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null)
            {
                TempData["Warning"] = "Please complete your medical profile before booking.";
                return RedirectToAction("Complete", "Profile");
            }

            var doctor = await _doctorRepo.GetOneAsync(d => d.Id == doctorId, tracked: false, includes: [d => d.Clinic]);
            if (doctor == null) return NotFound();

            ViewBag.Doctor = doctor;
            ViewBag.AvailableSlots = await GetAvailableSlotsAsync(doctorId);

            return View(new BookAppointmentVM { DoctorId = doctorId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookAppointmentVM vm)
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null)
            {
                TempData["Warning"] = "Please complete your medical profile before booking.";
                return RedirectToAction("Complete", "Profile");
            }

            var doctor = await _doctorRepo.GetOneAsync(d => d.Id == vm.DoctorId, tracked: false, includes: [d => d.Clinic]);
            if (doctor == null) return NotFound();

            // Re-validate on the server (prevents double booking / stale form submissions)
            var availableSlots = await GetAvailableSlotsAsync(vm.DoctorId);

            if (!ModelState.IsValid || !availableSlots.Contains(vm.SlotDateTime))
            {
                ModelState.AddModelError("", "This time slot is no longer available. Please choose another one.");
                ViewBag.Doctor = doctor;
                ViewBag.AvailableSlots = availableSlots;
                return View(vm);
            }

            var appointment = new Appointment
            {
                DoctorId = vm.DoctorId, PatientId = patient.Id,
                AppointmentDate = vm.SlotDateTime, Status = AppointmentStatus.Pending, Notes = vm.Notes
            };

            await _appointmentRepo.AddAsync(appointment);
            await _appointmentRepo.SaveAsync();

            TempData["Success"] = $"Your appointment with Dr. {doctor.FullName} on {vm.SlotDateTime:f} has been booked.";
            return RedirectToAction(nameof(Index));
        }

        // My Appointments
        public async Task<IActionResult> Index()
        {
            ViewData["Page"] = "Appointments";

            var patient = await GetCurrentPatientAsync();
            if (patient == null)
            {
                TempData["Warning"] = "Please complete your medical profile first.";
                return RedirectToAction("Complete", "Profile");
            }

            var appointments = await _appointmentRepo.GetAllAsync(a => a.PatientId == patient.Id, tracked: false,
                includes: [a => a.Doctor, a => a.Doctor.Clinic]);

            var ordered = appointments.OrderByDescending(a => a.AppointmentDate).ToList();

            ViewBag.Upcoming = ordered.Where(a => a.AppointmentDate >= DateTime.Now && a.Status != AppointmentStatus.Cancelled)
                                      .OrderBy(a => a.AppointmentDate).ToList();
            ViewBag.Past = ordered.Where(a => a.AppointmentDate < DateTime.Now || a.Status == AppointmentStatus.Cancelled).ToList();

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null) return RedirectToAction("Complete", "Profile");

            var appointment = await _appointmentRepo.GetOneAsync(a => a.Id == id);
            if (appointment == null || appointment.PatientId != patient.Id) return NotFound();

            if (appointment.AppointmentDate <= DateTime.Now)
            {
                TempData["Error"] = "Past appointments can't be cancelled.";
                return RedirectToAction(nameof(Index));
            }

            if (appointment.Status == AppointmentStatus.Completed)
            {
                TempData["Error"] = "Completed appointments can't be cancelled.";
                return RedirectToAction(nameof(Index));
            }

            appointment.Status = AppointmentStatus.Cancelled;
            _appointmentRepo.Update(appointment);
            await _appointmentRepo.SaveAsync();

            TempData["Success"] = "Your appointment has been cancelled.";
            return RedirectToAction(nameof(Index));
        }
    }
}
