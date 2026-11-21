using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.ViewModels.Customer
{
    public class BookAppointmentVM
    {
        [Required] public int DoctorId        { get; set; }
        [Required] public DateTime SlotDateTime { get; set; }
        public string? Notes                  { get; set; }
    }

    public class CompleteProfileVM
    {
        [Required] public string Gender    { get; set; } = string.Empty;
        [Required] public DateOnly DateOfBirth { get; set; }
        [Required, Range(0,130)] public int Age { get; set; }
        public string? BloodType           { get; set; }
        public string? Allergies           { get; set; }
        public string? ChronicDiseases     { get; set; }
        public string? EmergencyContactName  { get; set; }
        [Phone] public string? EmergencyContactPhone { get; set; }
    }
}
