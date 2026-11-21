using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Admin.Controllers
{
    [Area(CD.AdminArea)]
    [Authorize(Roles = CD.SuperAdmin + "," + CD.Admin)]
    public class DoctorController : Controller
    {
        private readonly IRepository<Doctor> _repo;
        private readonly IRepository<Clinic> _clinicRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public DoctorController(IRepository<Doctor> repo, IRepository<Clinic> clinicRepo,
            UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _repo = repo;
            _clinicRepo = clinicRepo;
            _userManager = userManager;
            _env = env;
        }

        private async Task LoadDropdownsAsync()
        {
            ViewBag.Clinics = (await _clinicRepo.GetAllAsync(tracked: false)).OrderBy(c => c.Name).ToList();
            ViewBag.Users = (await _userManager.GetUsersInRoleAsync(CD.Doctor)).ToList();
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Page"] = "Doctors";
            var doctors = await _repo.GetAllAsync(tracked: false, includes: [d => d.Clinic]);
            return View(doctors.OrderBy(d => d.FullName));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Page"] = "Doctors";
            await LoadDropdownsAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorVM vm)
        {
            ViewData["Page"] = "Doctors";
            if (!ModelState.IsValid) { await LoadDropdownsAsync(); return View(vm); }

            var doctor = new Doctor
            {
                FullName = vm.FullName, Specialization = vm.Specialization, PhoneNumber = vm.PhoneNumber,
                Email = vm.Email, Bio = vm.Bio, ConsultationFee = vm.ConsultationFee,
                ClinicId = vm.ClinicId, ApplicationUserId = vm.ApplicationUserId, IsActive = vm.IsActive
            };

            if (vm.Image != null)
                doctor.ImageUrl = await SaveImageAsync(vm.Image);

            await _repo.AddAsync(doctor);
            await _repo.SaveAsync();

            TempData["Success"] = "Doctor added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Page"] = "Doctors";
            var doctor = await _repo.GetOneAsync(d => d.Id == id);
            if (doctor == null) return NotFound();

            await LoadDropdownsAsync();
            ViewBag.CurrentImage = doctor.ImageUrl;
            ViewBag.DoctorId = doctor.Id;

            return View(new DoctorVM
            {
                FullName = doctor.FullName, Specialization = doctor.Specialization, PhoneNumber = doctor.PhoneNumber,
                Email = doctor.Email, Bio = doctor.Bio, ConsultationFee = doctor.ConsultationFee,
                ClinicId = doctor.ClinicId, ApplicationUserId = doctor.ApplicationUserId, IsActive = doctor.IsActive
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DoctorVM vm)
        {
            ViewData["Page"] = "Doctors";
            var doctor = await _repo.GetOneAsync(d => d.Id == id);
            if (doctor == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                ViewBag.CurrentImage = doctor.ImageUrl;
                ViewBag.DoctorId = doctor.Id;
                return View(vm);
            }

            doctor.FullName = vm.FullName; doctor.Specialization = vm.Specialization; doctor.PhoneNumber = vm.PhoneNumber;
            doctor.Email = vm.Email; doctor.Bio = vm.Bio; doctor.ConsultationFee = vm.ConsultationFee;
            doctor.ClinicId = vm.ClinicId; doctor.ApplicationUserId = vm.ApplicationUserId; doctor.IsActive = vm.IsActive;

            if (vm.Image != null)
                doctor.ImageUrl = await SaveImageAsync(vm.Image);

            _repo.Update(doctor);
            await _repo.SaveAsync();

            TempData["Success"] = "Doctor updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Page"] = "Doctors";
            var doctor = await _repo.GetOneAsync(d => d.Id == id, tracked: false, includes: [d => d.Clinic, d => d.Schedules]);
            if (doctor == null) return NotFound();
            return View(doctor);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _repo.GetOneAsync(d => d.Id == id, includes: [d => d.Appointments]);
            if (doctor == null) return NotFound();

            if (doctor.Appointments.Any())
            {
                TempData["Error"] = "Cannot delete a doctor who has appointment history.";
                return RedirectToAction(nameof(Index));
            }

            _repo.Delete(doctor);
            await _repo.SaveAsync();

            TempData["Success"] = "Doctor deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var folder = Path.Combine(_env.WebRootPath, CD.DoctorImagesPath);
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(folder, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/{CD.DoctorImagesPath}/{fileName}";
        }
    }
}
