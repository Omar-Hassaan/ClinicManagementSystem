namespace ClinicManagementSystem.Models
{
    public class MedicalFile
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;  // e.g. PDF, Image, Lab, Xray
        public string? Description { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // FK
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public int? MedicalRecordId { get; set; }
        public MedicalRecord? MedicalRecord { get; set; }
    }
}
