using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Admin.Controllers
{
    [Area(CD.AdminArea)]
    [Authorize(Roles = CD.SuperAdmin + "," + CD.Admin)]
    public class PrescriptionController : Controller
    {
        private readonly IRepository<Prescription> _repo;
        private readonly IRepository<MedicalRecord> _recordRepo;

        public PrescriptionController(IRepository<Prescription> repo, IRepository<MedicalRecord> recordRepo)
        {
            _repo = repo;
            _recordRepo = recordRepo;
        }

        private async Task LoadRecordsAsync()
        {
            var records = await _recordRepo.GetAllAsync(tracked: false, includes: [r => r.Patient]);
            ViewBag.Records = records.OrderByDescending(r => r.CreatedAt).ToList();
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Page"] = "Prescriptions";
            var prescriptions = await _repo.GetAllAsync(tracked: false, includes: [p => p.MedicalRecord, p => p.MedicalRecord.Patient]);
            return View(prescriptions.OrderByDescending(p => p.StartDate));
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? recordId)
        {
            ViewData["Page"] = "Prescriptions";
            await LoadRecordsAsync();
            return View(new PrescriptionVM { MedicalRecordId = recordId ?? 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PrescriptionVM vm)
        {
            ViewData["Page"] = "Prescriptions";
            if (!ModelState.IsValid) { await LoadRecordsAsync(); return View(vm); }

            var prescription = new Prescription
            {
                MedicalRecordId = vm.MedicalRecordId, MedicineName = vm.MedicineName, Dosage = vm.Dosage,
                Frequency = vm.Frequency, DurationDays = vm.DurationDays, Instructions = vm.Instructions,
                StartDate = vm.StartDate, EndDate = vm.StartDate.AddDays(vm.DurationDays)
            };

            await _repo.AddAsync(prescription);
            await _repo.SaveAsync();

            TempData["Success"] = "Prescription added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Page"] = "Prescriptions";
            var p = await _repo.GetOneAsync(x => x.Id == id, includes: [x => x.MedicalRecord, x => x.MedicalRecord.Patient]);
            if (p == null) return NotFound();

            await LoadRecordsAsync();
            ViewBag.PrescriptionId = p.Id;

            return View(new PrescriptionVM
            {
                MedicalRecordId = p.MedicalRecordId, MedicineName = p.MedicineName, Dosage = p.Dosage,
                Frequency = p.Frequency, DurationDays = p.DurationDays, Instructions = p.Instructions, StartDate = p.StartDate
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PrescriptionVM vm)
        {
            ViewData["Page"] = "Prescriptions";
            var p = await _repo.GetOneAsync(x => x.Id == id);
            if (p == null) return NotFound();

            if (!ModelState.IsValid) { await LoadRecordsAsync(); ViewBag.PrescriptionId = id; return View(vm); }

            p.MedicalRecordId = vm.MedicalRecordId; p.MedicineName = vm.MedicineName; p.Dosage = vm.Dosage;
            p.Frequency = vm.Frequency; p.DurationDays = vm.DurationDays; p.Instructions = vm.Instructions;
            p.StartDate = vm.StartDate; p.EndDate = vm.StartDate.AddDays(vm.DurationDays);

            _repo.Update(p);
            await _repo.SaveAsync();

            TempData["Success"] = "Prescription updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _repo.GetOneAsync(x => x.Id == id);
            if (p == null) return NotFound();

            _repo.Delete(p);
            await _repo.SaveAsync();

            TempData["Success"] = "Prescription deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
