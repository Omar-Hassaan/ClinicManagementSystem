using ClinicManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<MedicalFile> MedicalFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Appointment -> Doctor (restrict delete to avoid cascade conflict)
            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment -> Patient
            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // MedicalRecord -> Appointment (1-to-1)
            builder.Entity<MedicalRecord>()
                .HasOne(m => m.Appointment)
                .WithOne(a => a.MedicalRecord)
                .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // MedicalRecord -> Patient
            builder.Entity<MedicalRecord>()
                .HasOne(m => m.Patient)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // MedicalFile -> MedicalRecord (optional)
            builder.Entity<MedicalFile>()
                .HasOne(f => f.MedicalRecord)
                .WithMany(m => m.MedicalFiles)
                .HasForeignKey(f => f.MedicalRecordId)
                .OnDelete(DeleteBehavior.SetNull);

            // Decimals precision
            builder.Entity<Doctor>().Property(d => d.ConsultationFee).HasColumnType("decimal(10,2)");
            builder.Entity<MedicalRecord>().Property(m => m.Weight).HasColumnType("decimal(5,2)");
            builder.Entity<MedicalRecord>().Property(m => m.Height).HasColumnType("decimal(5,2)");
            builder.Entity<MedicalRecord>().Property(m => m.Temperature).HasColumnType("decimal(4,1)");
        }
    }
}
