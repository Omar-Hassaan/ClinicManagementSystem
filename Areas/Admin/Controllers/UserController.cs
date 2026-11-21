using ClinicManagementSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Admin.Controllers
{
    [Area(CD.AdminArea)]
    [Authorize(Roles = CD.SuperAdmin + "," + CD.Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole>    _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Page"] = "Users";

            var users = _userManager.Users.OrderBy(u => u.FullName).ToList();
            var list = new List<UserListVM>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                list.Add(new UserListVM
                {
                    Id             = u.Id,
                    FullName       = u.FullName,
                    UserName       = u.UserName ?? "",
                    Email          = u.Email ?? "",
                    PhoneNumber    = u.PhoneNumber,
                    Roles          = roles.ToList(),
                    IsLocked       = u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow,
                    EmailConfirmed = u.EmailConfirmed,
                    CreatedAt      = u.CreatedAt
                });
            }

            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Page"] = "Users";
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateVM vm)
        {
            ViewData["Page"] = "Users";

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(vm);
            }

            if (await _userManager.FindByEmailAsync(vm.Email) != null)
            {
                ModelState.AddModelError("Email", "Email already registered.");
                ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(vm);
            }

            var user = new ApplicationUser
            {
                FullName       = vm.FullName,
                UserName       = vm.UserName,
                Email          = vm.Email,
                Address        = vm.Address,
                PhoneNumber    = vm.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, vm.Password);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(vm);
            }

            if (vm.SelectedRoles.Any())
                await _userManager.AddToRolesAsync(user, vm.SelectedRoles);

            TempData["Success"] = "User created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            ViewData["Page"] = "Users";

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(new UserEditVM
            {
                Id          = user.Id,
                FullName    = user.FullName,
                Email       = user.Email ?? "",
                Address     = user.Address,
                PhoneNumber = user.PhoneNumber
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserEditVM vm)
        {
            ViewData["Page"] = "Users";
            if (id != vm.Id) return NotFound();

            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!string.Equals(user.Email, vm.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userManager.FindByEmailAsync(vm.Email);
                if (existing != null && existing.Id != user.Id)
                {
                    ModelState.AddModelError("Email", "Email already in use.");
                    return View(vm);
                }
                await _userManager.SetEmailAsync(user, vm.Email);
            }

            user.FullName    = vm.FullName;
            user.Address     = vm.Address;
            user.PhoneNumber = vm.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var pwResult = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);
                if (!pwResult.Succeeded)
                {
                    foreach (var e in pwResult.Errors) ModelState.AddModelError("", e.Description);
                    return View(vm);
                }
            }

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ManageRoles(string id)
        {
            ViewData["Page"] = "Users";

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            return View(new AssignRolesVM
            {
                UserId        = user.Id,
                UserName      = user.UserName ?? "",
                AllRoles      = _roleManager.Roles.Select(r => r.Name!).ToList(),
                SelectedRoles = userRoles.ToList()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(AssignRolesVM vm)
        {
            var user = await _userManager.FindByIdAsync(vm.UserId);
            if (user == null) return NotFound();

            var current = await _userManager.GetRolesAsync(user);
            var selected = vm.SelectedRoles ?? new List<string>();

            var toRemove = current.Except(selected).ToList();
            var toAdd    = selected.Except(current).ToList();

            if (toRemove.Any()) await _userManager.RemoveFromRolesAsync(user, toRemove);
            if (toAdd.Any())    await _userManager.AddToRolesAsync(user, toAdd);

            TempData["Success"] = "Roles updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            bool isLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow;

            if (isLocked)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);
                TempData["Success"] = $"\"{user.UserName}\" unlocked.";
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                TempData["Success"] = $"\"{user.UserName}\" locked.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(CD.SuperAdmin))
            {
                TempData["Error"] = "Cannot delete a Super Admin account.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
