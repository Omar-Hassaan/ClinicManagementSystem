namespace ClinicManagementSystem.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string? Symptoms { get; set; }
        public string? TreatmentPlan { get; set; }
        public string? DoctorNotes { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public string? BloodPressure { get; set; }
        public decimal? Temperature { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // FK
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        // Navigation
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<MedicalFile> MedicalFiles { get; set; } = new List<MedicalFile>();
    }
}
