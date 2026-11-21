using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Admin.Controllers
{
    [Area(CD.AdminArea)]
    [Authorize(Roles = CD.SuperAdmin + "," + CD.Admin)]
    public class DoctorScheduleController : Controller
    {
        private readonly IRepository<DoctorSchedule> _repo;
        private readonly IRepository<Doctor> _doctorRepo;

        public DoctorScheduleController(IRepository<DoctorSchedule> repo, IRepository<Doctor> doctorRepo)
        {
            _repo = repo;
            _doctorRepo = doctorRepo;
        }

        private async Task LoadDoctorsAsync()
            => ViewBag.Doctors = (await _doctorRepo.GetAllAsync(tracked: false)).OrderBy(d => d.FullName).ToList();

        public async Task<IActionResult> Index()
        {
            ViewData["Page"] = "Schedules";
            var schedules = await _repo.GetAllAsync(tracked: false, includes: [s => s.Doctor]);
            return View(schedules.OrderBy(s => s.Doctor.FullName).ThenBy(s => s.DayOfWeek));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Page"] = "Schedules";
            await LoadDoctorsAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorScheduleVM vm)
        {
            ViewData["Page"] = "Schedules";

            if (vm.EndTime <= vm.StartTime)
                ModelState.AddModelError("EndTime", "End time must be after start time.");

            if (!ModelState.IsValid) { await LoadDoctorsAsync(); return View(vm); }

            var schedule = new DoctorSchedule
            {
                DoctorId = vm.DoctorId, DayOfWeek = vm.DayOfWeek,
                StartTime = vm.StartTime, EndTime = vm.EndTime, SlotDurationMinutes = vm.SlotDurationMinutes
            };

            await _repo.AddAsync(schedule);
            await _repo.SaveAsync();

            TempData["Success"] = "Schedule added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Page"] = "Schedules";
            var schedule = await _repo.GetOneAsync(s => s.Id == id);
            if (schedule == null) return NotFound();

            await LoadDoctorsAsync();
            ViewBag.ScheduleId = schedule.Id;

            return View(new DoctorScheduleVM
            {
                DoctorId = schedule.DoctorId, DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime, EndTime = schedule.EndTime, SlotDurationMinutes = schedule.SlotDurationMinutes
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DoctorScheduleVM vm)
        {
            ViewData["Page"] = "Schedules";
            var schedule = await _repo.GetOneAsync(s => s.Id == id);
            if (schedule == null) return NotFound();

            if (vm.EndTime <= vm.StartTime)
                ModelState.AddModelError("EndTime", "End time must be after start time.");

            if (!ModelState.IsValid) { await LoadDoctorsAsync(); ViewBag.ScheduleId = id; return View(vm); }

            schedule.DoctorId = vm.DoctorId; schedule.DayOfWeek = vm.DayOfWeek;
            schedule.StartTime = vm.StartTime; schedule.EndTime = vm.EndTime; schedule.SlotDurationMinutes = vm.SlotDurationMinutes;

            _repo.Update(schedule);
            await _repo.SaveAsync();

            TempData["Success"] = "Schedule updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _repo.GetOneAsync(s => s.Id == id);
            if (schedule == null) return NotFound();

            _repo.Delete(schedule);
            await _repo.SaveAsync();

            TempData["Success"] = "Schedule deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
