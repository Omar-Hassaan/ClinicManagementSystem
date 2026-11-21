namespace ClinicManagementSystem.ViewModels.Admin
{
    public class DashboardVM
    {
        // Stat Cards
        public int TotalUsers         { get; set; }
        public int TotalDoctors       { get; set; }
        public int TotalPatients      { get; set; }
        public int TotalClinics       { get; set; }
        public int TotalAppointments  { get; set; }
        public int PendingAppointments    { get; set; }
        public int CompletedAppointments  { get; set; }
        public int CancelledAppointments  { get; set; }
        public int TotalMedicalRecords    { get; set; }
        public int TotalPrescriptions     { get; set; }

        // Line Chart — appointments last 6 months
        public List<string> MonthLabels              { get; set; } = new();
        public List<int>    MonthlyAppointments      { get; set; } = new();

        // Bar Chart — doctors per clinic
        public List<string> ClinicLabels             { get; set; } = new();
        public List<int>    DoctorsPerClinic         { get; set; } = new();

        // Pie Chart — users by role
        public List<string> RoleLabels               { get; set; } = new();
        public List<int>    RoleCounts               { get; set; } = new();

        // Pie Chart 2 — appointment status distribution
        public List<string> StatusLabels             { get; set; } = new();
        public List<int>    StatusCounts             { get; set; } = new();
    }
}
