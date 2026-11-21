using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.ViewModels.Admin
{
    public class UserListVM
    {
        public string Id            { get; set; } = string.Empty;
        public string FullName      { get; set; } = string.Empty;
        public string UserName      { get; set; } = string.Empty;
        public string Email         { get; set; } = string.Empty;
        public string? PhoneNumber  { get; set; }
        public List<string> Roles   { get; set; } = new();
        public bool IsLocked        { get; set; }
        public bool EmailConfirmed  { get; set; }
        public DateTime CreatedAt   { get; set; }
    }

    public class UserCreateVM
    {
        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Address { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public List<string> SelectedRoles { get; set; } = new();
    }

    public class UserEditVM
    {
        public string Id { get; set; } = string.Empty;

        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Address { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.Password), MinLength(6)]
        [Display(Name = "New Password (leave blank to keep)")]
        public string? NewPassword { get; set; }
    }

    public class AssignRolesVM
    {
        public string UserId        { get; set; } = string.Empty;
        public string UserName      { get; set; } = string.Empty;
        public List<string> AllRoles     { get; set; } = new();
        public List<string> SelectedRoles { get; set; } = new();
    }
}
