# Clinic Management System (مجمع عيادات)

A full ASP.NET Core MVC application (.NET 10) for managing a multi-clinic complex: appointment booking, patient
follow-up, medical records, prescriptions, and doctor scheduling — with a plain **HTML-only** frontend (no Razor
Class Libraries / component frameworks), a custom CSS design system, ASP.NET Core Identity, a Generic Repository,
a full **Admin Area**, and a full **Customer (Patient) Area**.

---

## 1. System Description — User Stories

### 👤 As a Patient (Customer)
1. As a visitor, I want to **register an account** so that I can book appointments.
2. As a new patient, I want to **complete my medical profile** (gender, DOB, blood type, allergies, emergency
   contact) after registering, so the clinic has the information it needs.
3. As a patient, I want to **browse clinics** and see their specialization, address and doctors.
4. As a patient, I want to **search/browse doctors** by name, specialization, or clinic.
5. As a patient, I want to **view a doctor's weekly availability** and **book an open time slot**.
6. As a patient, I want the system to **prevent double-booking** — a slot that's already taken shouldn't be
   offered to me.
7. As a patient, I want to **view my upcoming and past appointments**.
8. As a patient, I want to **cancel an upcoming appointment** if I can no longer make it.
9. As a patient, I want to **view my medical records** (diagnosis, vitals, doctor's notes) after each visit.
10. As a patient, I want to **view my prescriptions** (medicine, dosage, frequency, duration) linked to each record.
11. As a patient, I want to **edit my account info** (name, phone, address, photo) and **change my password**.

### 🛠️ As an Admin / Super Admin
12. As an admin, I want a **dashboard** with key stats (users, doctors, patients, clinics, appointments) and
    **charts** (appointments over time, doctors per clinic, users by role, appointment status breakdown).
13. As an admin, I want to **manage Clinics** (create/edit/delete, upload a photo, mark active/inactive).
14. As an admin, I want to **manage Doctors** (assign to a clinic, set consultation fee, upload a photo, link
    to a login account).
15. As an admin, I want to **manage Doctor Schedules** (weekly recurring availability per doctor, with slot length).
16. As an admin, I want to **manage Patient profiles** (create on behalf of a user, edit medical info).
17. As an admin, I want to **manage all Appointments** (create/edit/cancel/filter by status).
18. As an admin, I want to **create Medical Records** against a completed appointment (this auto-marks the
    appointment "Completed").
19. As an admin, I want to **attach Prescriptions** to a medical record.
20. As an admin, I want full **User Management**: create users, assign one or more roles, edit user info,
    and **lock/unlock** accounts.

---

## 2. Entity Relationship Diagram (ERD)

```
┌──────────────────┐         ┌──────────────────┐         ┌──────────────────┐
│   ApplicationUser │1       *│      Doctor       │*       1│      Clinic       │
│───────────────────│◄────────│───────────────────│────────►│───────────────────│
│ Id (PK)           │ (opt.)  │ Id (PK)           │         │ Id (PK)           │
│ FullName          │         │ FullName          │         │ Name              │
│ Email             │         │ Specialization    │         │ Specialization    │
│ Address           │         │ PhoneNumber       │         │ Address           │
│ ImageUrl          │         │ ConsultationFee   │         │ PhoneNumber       │
│ ...(Identity)     │         │ ClinicId (FK)     │         │ ImageUrl          │
└─────────┬─────────┘         │ ApplicationUserId │         │ IsActive          │
          │1                  └─────────┬─────────┘         └───────────────────┘
          │                             │1
          │                             │*
          │                   ┌─────────▼─────────┐
          │                   │  DoctorSchedule    │
          │                   │────────────────────│
          │                   │ Id (PK)            │
          │                   │ DoctorId (FK)      │
          │                   │ DayOfWeek (enum)   │
          │                   │ StartTime/EndTime  │
          │                   │ SlotDurationMinutes│
          │                   └────────────────────┘
          │1
          │
┌─────────▼─────────┐         ┌──────────────────┐         ┌───────────────────┐
│      Patient       │1      *│   Appointment     │*      1│      Doctor        │
│────────────────────│◄────────│──────────────────│────────►│  (see above)      │
│ Id (PK)            │         │ Id (PK)          │         └───────────────────┘
│ ApplicationUserId  │         │ DoctorId (FK)    │
│ FullName           │         │ PatientId (FK)   │
│ Gender / DOB / Age │         │ AppointmentDate  │
│ BloodType          │         │ Status (enum)    │
│ Allergies          │         │ Notes            │
│ ChronicDiseases    │         └────────┬─────────┘
│ EmergencyContact.. │                  │1
└─────────┬──────────┘                  │0..1
          │1                  ┌─────────▼─────────┐
          │                   │   MedicalRecord    │
          │*                  │────────────────────│
┌─────────▼──────────┐        │ Id (PK)            │
│    MedicalFile      │*     1│ AppointmentId (FK) │
│─────────────────────│◄───────│ PatientId (FK)    │
│ Id (PK)             │        │ Diagnosis         │
│ PatientId (FK)      │        │ Symptoms          │
│ MedicalRecordId(FK) │        │ TreatmentPlan     │
│ FileName / FileUrl  │        │ Weight/Height/BP  │
│ FileType            │        └─────────┬──────────┘
└─────────────────────┘                  │1
                                          │*
                                ┌─────────▼─────────┐
                                │    Prescription     │
                                │──────────────────────│
                                │ Id (PK)              │
                                │ MedicalRecordId (FK) │
                                │ MedicineName         │
                                │ Dosage / Frequency   │
                                │ DurationDays         │
                                │ StartDate / EndDate  │
                                └──────────────────────┘
```

**Relationships summary**
| Relationship | Type |
|---|---|
| Clinic → Doctor | 1‑to‑many |
| Doctor → DoctorSchedule | 1‑to‑many |
| Doctor → Appointment | 1‑to‑many |
| Patient → Appointment | 1‑to‑many |
| Appointment → MedicalRecord | 1‑to‑1 (optional) |
| Patient → MedicalRecord | 1‑to‑many |
| MedicalRecord → Prescription | 1‑to‑many |
| Patient / MedicalRecord → MedicalFile | 1‑to‑many |
| ApplicationUser → Patient | 1‑to‑1 |
| ApplicationUser → Doctor | 1‑to‑1 (optional, for doctor logins) |
| ApplicationUser ↔ IdentityRole | many‑to‑many (SuperAdmin, Admin, Doctor, Patient) |

---

## 3. Models
All models live in `/Models`: `ApplicationUser`, `Clinic`, `Doctor`, `DoctorSchedule`, `Patient`, `Appointment`
(+ `AppointmentStatus` enum), `MedicalRecord`, `Prescription`, `MedicalFile`.

## 4. DbContext
`Data/ApplicationDbContext.cs` extends `IdentityDbContext<ApplicationUser>` and exposes a `DbSet<T>` for every
entity above, with `OnModelCreating` configuring delete behaviors (mostly `Restrict` to protect historical data)
and decimal precision.

## 5 & 6. Migration / Update-Database
**This sandbox has no .NET SDK installed**, so I could not run `dotnet ef` here. Everything (models, DbContext,
connection string) is ready to go — run these two commands yourself from the project folder:

```bash
# Package Manager Console (Visual Studio)
Add-Migration InitialCreate
Update-Database

# OR .NET CLI
dotnet ef migrations add InitialCreate
dotnet ef database update
```

> Update `appsettings.json` → `ConnectionStrings:DefaultConnection` first if your SQL Server instance name differs
> from the default `Server=.`.

## 7. Generic Repository
`Repositories/IRepository.cs` + `Repository.cs` — a single generic repository (`GetAllAsync`, `GetOneAsync`,
`AddAsync`, `Update`, `Delete`, `SaveAsync`) used by every controller via `IRepository<T>` (registered as an open
generic in `Program.cs`, so it works for every entity automatically).

## 8. ASP.NET Core Identity
- `ApplicationUser : IdentityUser` with extra fields (`FullName`, `Address`, `ImageUrl`).
- Roles: `SuperAdmin`, `Admin`, `Doctor`, `Patient` (seeded automatically on first run).
- Views: `Areas/Identity/Views/Account/{Login, Register, AccessDenied}.cshtml` and
  `Areas/Identity/Views/Profile/Index.cshtml` (account info + photo + change password) — all plain HTML/CSS,
  no client component libraries.

---

## Admin Area (`/Admin`)
- **Dashboard** (`Areas/Admin/Views/Home/Index.cshtml`): stat cards + Chart.js **line** (appointments/6mo),
  **pie** (users by role), **bar** (doctors per clinic), **doughnut** (appointment status).
- **Full CRUD** for: Clinics, Doctors, Doctor Schedules, Patients, Appointments, Medical Records, Prescriptions.
- **User Management** (`UserController`): create user + assign roles, edit info, **ManageRoles**, **lock/unlock**,
  delete (blocked for SuperAdmin accounts).

## Customer Area (`/Customer`)
- **Dashboard**: next appointment, quick stats, quick actions.
- **Clinics/Doctors**: browse + search, doctor profile with weekly availability.
- **Book Appointment**: generates real bookable slots from the doctor's weekly schedule for the next 14 days,
  excludes already-booked slots, and re-validates on submit (anti double-booking).
- **My Appointments**: Upcoming / Past tabs, **Cancel** (blocked for past/completed appointments).
- **Medical Records**: read-only accordion view with nested prescriptions.
- **Medical Profile**: "complete profile" flow triggered automatically on first login after registration.

---

## Demo Login
| Role | Username | Password |
|---|---|---|
| Super Admin | `superadmin` | `Admin@123` |

Seed data also includes 2 clinics and 3 doctors with Sun–Thu 9am–5pm schedules, so you can log in as
`superadmin`, create a test Patient user from **Admin → Users → Create**, then log in as that patient and
walk the full booking cycle immediately.

## Recommended full walkthrough ("امشي cycle")
1. Login as `superadmin` → see Dashboard with charts.
2. Admin → Users → Create a user with role **Patient** (auto email-confirmed).
3. Logout → Login as the new patient → you're redirected to **Complete Medical Profile**.
4. Fill it in → land on the Patient Dashboard.
5. Go to **Doctors** → pick one of the 3 seeded doctors → **Book Appointment** → pick a slot.
6. Go to **My Appointments** → see it under Upcoming → try **Cancel**.
7. Book another appointment, then log back in as `superadmin` → **Admin → Medical Records → New Record** →
   pick that appointment → fill diagnosis (this auto-marks the appointment Completed).
8. Add a **Prescription** to that record.
9. Log back in as the patient → **Medical Records** → see the diagnosis + prescription.
