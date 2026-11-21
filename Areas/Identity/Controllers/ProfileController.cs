using ClinicManagementSystem.ViewModels.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Identity.Controllers
{
    [Area(CD.IdentityArea)]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var vm = new ProfileVM
            {
                FullName    = user.FullName,
                Email       = user.Email ?? "",
                UserName    = user.UserName ?? "",
                Address     = user.Address,
                PhoneNumber = user.PhoneNumber,
                ImageUrl    = user.ImageUrl
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileVM vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(vm.FullName))
            {
                TempData["Error"] = "Full name is required.";
                return RedirectToAction(nameof(Index));
            }

            user.FullName    = vm.FullName;
            user.Address     = vm.Address;
            user.PhoneNumber = vm.PhoneNumber;

            if (vm.Photo != null)
            {
                var folder = Path.Combine(_env.WebRootPath, "images", "Users");
                Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.Photo.FileName)}";
                var path = Path.Combine(folder, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await vm.Photo.CopyToAsync(stream);

                user.ImageUrl = $"/images/Users/{fileName}";
            }

            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid || vm.NewPassword != vm.ConfirmPassword)
            {
                TempData["Error"] = "Please check your password fields.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Password changed successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
