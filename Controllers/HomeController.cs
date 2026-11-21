using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains(CD.SuperAdmin) || roles.Contains(CD.Admin))
                        return RedirectToAction("Index", "Home", new { area = CD.AdminArea });

                    if (roles.Contains(CD.Patient))
                        return RedirectToAction("Index", "Home", new { area = CD.CustomerArea });
                }
            }

            return RedirectToAction("Login", "Account", new { area = CD.IdentityArea });
        }

        public IActionResult Error() => View();
    }
}
