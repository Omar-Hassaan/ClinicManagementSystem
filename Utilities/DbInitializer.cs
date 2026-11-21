using ClinicManagementSystem.Data;
using ClinicManagementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagementSystem.Utilities
{
    public interface IDbInitializer
    {
        Task InitializeAsync();
    }

    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole>   _roleManager;
        private readonly ApplicationDbContext        _context;

        public DbInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context     = context;
        }

        public async Task InitializeAsync()
        {
            // Seed Roles
            string[] roles = [CD.SuperAdmin, CD.Admin, CD.Doctor, CD.Patient];

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed SuperAdmin
            const string adminEmail = "superadmin@clinic.com";

            if (await _userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    FullName       = "Super Admin",
                    UserName       = "superadmin",
                    Email          = adminEmail,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(admin, "Admin@123");

                if (result.Succeeded)
                    await _userManager.AddToRoleAsync(admin, CD.SuperAdmin);
            }

            // Seed demo Clinics + Doctors + Schedules (only if none exist yet)
            if (!_context.Clinics.Any())
            {
                var clinic1 = new Clinic
                {
                    Name = "Sunrise Medical Complex", Specialization = "General & Family Medicine",
                    Address = "12 Nile Corniche, Cairo", PhoneNumber = "02-1234-5678",
                    Description = "A multi-specialty clinic complex serving the community for over 15 years.",
                    IsActive = true
                };

                var clinic2 = new Clinic
                {
                    Name = "Smile Dental Center", Specialization = "Dentistry",
                    Address = "45 Tahrir St, Giza", PhoneNumber = "02-9876-5432",
                    Description = "Modern dental care with the latest technology.",
                    IsActive = true
                };

                _context.Clinics.AddRange(clinic1, clinic2);
                await _context.SaveChangesAsync();

                var doctor1 = new Doctor
                {
                    FullName = "Ahmed Hassan", Specialization = "General Practitioner",
                    PhoneNumber = "010-1111-2222", Email = "ahmed.hassan@clinic.com",
                    Bio = "Over 10 years of experience in family medicine.",
                    ConsultationFee = 300, ClinicId = clinic1.Id, IsActive = true
                };

                var doctor2 = new Doctor
                {
                    FullName = "Mona Farid", Specialization = "Pediatrics",
                    PhoneNumber = "010-3333-4444", Email = "mona.farid@clinic.com",
                    Bio = "Specialized in child healthcare and vaccinations.",
                    ConsultationFee = 350, ClinicId = clinic1.Id, IsActive = true
                };

                var doctor3 = new Doctor
                {
                    FullName = "Karim Adel", Specialization = "Dentist",
                    PhoneNumber = "010-5555-6666", Email = "karim.adel@clinic.com",
                    Bio = "Expert in cosmetic and restorative dentistry.",
                    ConsultationFee = 400, ClinicId = clinic2.Id, IsActive = true
                };

                _context.Doctors.AddRange(doctor1, doctor2, doctor3);
                await _context.SaveChangesAsync();

                DayOfWeek[] weekdays = [DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday];

                foreach (var doctor in new[] { doctor1, doctor2, doctor3 })
                {
                    foreach (var day in weekdays)
                    {
                        _context.DoctorSchedules.Add(new DoctorSchedule
                        {
                            DoctorId = doctor.Id, DayOfWeek = day,
                            StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0),
                            SlotDurationMinutes = 30, IsActive = true
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}

