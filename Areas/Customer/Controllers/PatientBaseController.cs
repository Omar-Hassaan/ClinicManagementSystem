using ClinicManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace ClinicManagementSystem.Areas.Customer.Controllers
{
    // Shared base for every controller in the Customer (Patient) area.
    // Centralizes role restriction + resolving the Patient row for the
    // currently logged-in ApplicationUser.
    [Area(CD.CustomerArea)]
    [Authorize(Roles = CD.Patient)]
    public abstract class PatientBaseController : Controller
    {
        protected readonly UserManager<ApplicationUser> UserManager;
        protected readonly IRepository<Patient> PatientRepository;

        protected PatientBaseController(UserManager<ApplicationUser> userManager, IRepository<Patient> patientRepository)
        {
            UserManager = userManager;
            PatientRepository = patientRepository;
        }

        protected async Task<Patient?> GetCurrentPatientAsync(params Expression<Func<Patient, object>>[] includes)
        {
            var userId = UserManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return null;

            return await PatientRepository.GetOneAsync(p => p.ApplicationUserId == userId, includes: includes);
        }
    }
}
