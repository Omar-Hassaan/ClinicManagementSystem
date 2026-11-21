using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.ViewModels.Identity
{
    public class ProfileVM
    {
        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        public string Email    { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Address { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? ImageUrl { get; set; }
        public IFormFile? Photo { get; set; }
    }

    public class ChangePasswordVM
    {
        [Required, DataType(DataType.Password), Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(6), Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
