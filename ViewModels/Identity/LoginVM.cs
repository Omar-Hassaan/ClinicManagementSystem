using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.ViewModels.Identity
{
    public class LoginVM
    {
        [Required]
        public string EmailOrUsername { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
