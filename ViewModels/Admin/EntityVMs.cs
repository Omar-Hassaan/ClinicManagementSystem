using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.ViewModels.Admin
{
    // ── Clinic ────────────────────────────────────────────────────────────────
    public class ClinicVM
    {
        [Required] public string Name           { get; set; } = string.Empty;
        [Required] public string Specialization { get; set; } = string.Empty;
        [Required] public string Address        { get; set; } = string.Empty;
        [Required, Phone] public string PhoneNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public IFormFile? Image { get; set; }
    }

    // ── Doctor ────────────────────────────────────────────────────────────────
    public class DoctorVM
    {
        [Required, Display(Name = "Full Name")] public string FullName { get; set; } = string.Empty;
        [Required] public string Specialization  { get; set; } = string.Empty;
        [Required, Phone] public string PhoneNumber { get; set; } = string.Empty;
        [EmailAddress] public string? Email      { get; set; }
        public string? Bio                       { get; set; }
        [Required] public decimal ConsultationFee { get; set; }
        [Required] public int ClinicId           { get; set; }
        public string? ApplicationUserId         { get; set; }
        public bool IsActive { get; set; } = true;
        public IFormFile? Image { get; set; }
    }

    // ── DoctorSchedule ────────────────────────────────────────────────────────
    public class DoctorScheduleVM
    {
        [Required] public int DoctorId              { get; set; }
        [Required] public DayOfWeek DayOfWeek       { get; set; }
        [Required] public TimeOnly StartTime        { get; set; }
        [Required] public TimeOnly EndTime          { get; set; }
        [Range(10, 120)] public int SlotDurationMinutes { get; set; } = 30;
    }

    // ── Patient ───────────────────────────────────────────────────────────────
    public class PatientVM
    {
        public string? ApplicationUserId   { get; set; }
        [Required] public string Gender    { get; set; } = string.Empty;
        [Required] public DateOnly DateOfBirth { get; set; }
        [Required, Range(0,130)] public int Age { get; set; }
        public string? BloodType           { get; set; }
        public string? Allergies           { get; set; }
        public string? ChronicDiseases     { get; set; }
        public string? EmergencyContactName  { get; set; }
        [Phone] public string? EmergencyContactPhone { get; set; }
    }

    // ── Appointment ───────────────────────────────────────────────────────────
    public class AppointmentVM
    {
        [Required] public int DoctorId    { get; set; }
        [Required] public int PatientId   { get; set; }
        [Required] public DateTime AppointmentDate { get; set; }
        public AppointmentStatus Status   { get; set; } = AppointmentStatus.Pending;
        public string? Notes              { get; set; }
    }

    // ── MedicalRecord ─────────────────────────────────────────────────────────
    public class MedicalRecordVM
    {
        [Required] public int AppointmentId  { get; set; }
        [Required] public int PatientId      { get; set; }
        [Required] public string Diagnosis   { get; set; } = string.Empty;
        public string? Symptoms              { get; set; }
        public string? TreatmentPlan        { get; set; }
        public string? DoctorNotes          { get; set; }
        public decimal? Weight               { get; set; }
        public decimal? Height               { get; set; }
        public string? BloodPressure        { get; set; }
        public decimal? Temperature          { get; set; }
    }

    // ── Prescription ──────────────────────────────────────────────────────────
    public class PrescriptionVM
    {
        [Required] public int MedicalRecordId        { get; set; }
        [Required] public string MedicineName        { get; set; } = string.Empty;
        [Required] public string Dosage              { get; set; } = string.Empty;
        [Required] public string Frequency           { get; set; } = string.Empty;
        [Required, Range(1,365)] public int DurationDays { get; set; }
        public string? Instructions                  { get; set; }
        [Required] public DateTime StartDate         { get; set; } = DateTime.Today;
    }
}
