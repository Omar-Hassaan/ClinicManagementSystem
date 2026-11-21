using ClinicManagementSystem.Repositories;
using ClinicManagementSystem.ViewModels.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Areas.Identity.Controllers
{
    [Area(CD.IdentityArea)]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser>  _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRepository<Patient>          _patientRepo;

        public AccountController(
            UserManager<ApplicationUser>  userManager,
            SignInManager<ApplicationUser> signInManager,
            IRepository<Patient>          patientRepo)
        {
            _userManager  = userManager;
            _signInManager = signInManager;
            _patientRepo  = patientRepo;
        }

        // ── Register ─────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (await _userManager.FindByEmailAsync(vm.Email) != null)
            {
                ModelState.AddModelError("Email", "Email already registered."); return View(vm);
            }

            if (await _userManager.FindByNameAsync(vm.Username) != null)
            {
                ModelState.AddModelError("Username", "Username already taken."); return View(vm);
            }

            var user = new ApplicationUser
            {
                FullName       = vm.FullName,
                UserName       = vm.Username,
                Email          = vm.Email,
                Address        = vm.Address,
                PhoneNumber    = vm.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, vm.Password);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            await _userManager.AddToRoleAsync(user, CD.Patient);

            TempData["Success"] = "Account created! Please log in.";
            return RedirectToAction(nameof(Login));
        }

        // ── Login ─────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.EmailOrUsername)
                    ?? await _userManager.FindByNameAsync(vm.EmailOrUsername);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid credentials."); return View(vm);
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError("", "Account is locked. Contact the administrator.");
                return View(vm);
            }

            var result = await _signInManager.PasswordSignInAsync(user, vm.Password, vm.RememberMe, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid credentials."); return View(vm);
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains(CD.SuperAdmin) || roles.Contains(CD.Admin))
                return RedirectToAction("Index", "Home", new { area = CD.AdminArea });

            if (roles.Contains(CD.Patient))
                return RedirectToAction("Index", "Home", new { area = CD.CustomerArea });

            return RedirectToAction("Index", "Profile", new { area = CD.IdentityArea });
        }

        // ── Logout ────────────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        // ── Access Denied ─────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}
