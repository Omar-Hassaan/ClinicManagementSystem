using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Admin.Controllers
{
    [Area(CD.AdminArea)]
    [Authorize(Roles = CD.SuperAdmin + "," + CD.Admin)]
    public class ClinicController : Controller
    {
        private readonly IRepository<Clinic> _repo;
        private readonly IWebHostEnvironment _env;

        public ClinicController(IRepository<Clinic> repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Page"] = "Clinics";
            var clinics = await _repo.GetAllAsync(tracked: false);
            return View(clinics.OrderBy(c => c.Name));
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Page"] = "Clinics";
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClinicVM vm)
        {
            ViewData["Page"] = "Clinics";
            if (!ModelState.IsValid) return View(vm);

            var clinic = new Clinic
            {
                Name = vm.Name,
                Specialization = vm.Specialization,
                Address = vm.Address,
                PhoneNumber = vm.PhoneNumber,
                Description = vm.Description,
                IsActive = vm.IsActive
            };

            if (vm.Image != null)
                clinic.ImageUrl = await SaveImageAsync(vm.Image);

            await _repo.AddAsync(clinic);
            await _repo.SaveAsync();

            TempData["Success"] = "Clinic created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Page"] = "Clinics";
            var clinic = await _repo.GetOneAsync(c => c.Id == id);
            if (clinic == null) return NotFound();

            ViewBag.CurrentImage = clinic.ImageUrl;
            ViewBag.ClinicId = clinic.Id;

            return View(new ClinicVM
            {
                Name = clinic.Name, Specialization = clinic.Specialization, Address = clinic.Address,
                PhoneNumber = clinic.PhoneNumber, Description = clinic.Description, IsActive = clinic.IsActive
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClinicVM vm)
        {
            ViewData["Page"] = "Clinics";
            var clinic = await _repo.GetOneAsync(c => c.Id == id);
            if (clinic == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.CurrentImage = clinic.ImageUrl;
                ViewBag.ClinicId = clinic.Id;
                return View(vm);
            }

            clinic.Name = vm.Name;
            clinic.Specialization = vm.Specialization;
            clinic.Address = vm.Address;
            clinic.PhoneNumber = vm.PhoneNumber;
            clinic.Description = vm.Description;
            clinic.IsActive = vm.IsActive;

            if (vm.Image != null)
                clinic.ImageUrl = await SaveImageAsync(vm.Image);

            _repo.Update(clinic);
            await _repo.SaveAsync();

            TempData["Success"] = "Clinic updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Page"] = "Clinics";
            var clinic = await _repo.GetOneAsync(c => c.Id == id, tracked: false, includes: [c => c.Doctors]);
            if (clinic == null) return NotFound();
            return View(clinic);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var clinic = await _repo.GetOneAsync(c => c.Id == id, includes: [c => c.Doctors]);
            if (clinic == null) return NotFound();

            if (clinic.Doctors.Any())
            {
                TempData["Error"] = "Cannot delete a clinic that still has doctors assigned to it.";
                return RedirectToAction(nameof(Index));
            }

            _repo.Delete(clinic);
            await _repo.SaveAsync();

            TempData["Success"] = "Clinic deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var folder = Path.Combine(_env.WebRootPath, CD.ClinicImagesPath);
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{CD.ClinicImagesPath}/{fileName}";
        }
    }
}
