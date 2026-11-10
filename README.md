# 📋 AnimalCare Veterinary Clinic (DB-First ASP.NET Core MVC)

AnimalCare Veterinary Clinic is an ASP.NET Core MVC web application built with Entity Framework Core (DB-First). It manages animals, owners, veterinarians, appointments, schedules, medical histories, and notifications for a veterinary clinic, with role-based authentication for Administrators, Receptionists, and Veterinarians. The application was originally developed against SQL Server and has since been migrated to PostgreSQL.

## 📦 Technologies

- **Language:** C#
- **Framework:** ASP.NET Core MVC (.NET 7)
- **ORM:** Entity Framework Core 7 (DB-First)
- **Database:** PostgreSQL (migrated from SQL Server)
- **Authentication:** Cookie-based authentication with role-based authorization
- **Views:** Razor Views (`.cshtml`)
- **State Management:** Scoped services (`AnimalCareContext`, `NotificationService`)
- **Data Access Patterns:** Eager loading with `Include` / `ThenInclude`, view-mapped entities (`DailyAgenda`, `MonthlyReport`)
- **Database Objects:** Tables, views, indexes, foreign keys, check constraints, and statement-level triggers

## ⚙️ Features

- **Authentication:** Login, logout, and access-denied pages for unauthenticated or unauthorized users.
- **Role-Based Authorization:** Three roles — Administrator, Receptionist, and Veterinarian — each with different page access and field-level editing permissions.
- **Animal Management:** Create, read, update, and delete animals; veterinarians may only edit an animal's weight.
- **Owner Management:** Full CRUD for pet owners, including phone, email, and address validation.
- **Veterinarian Management:** Admin-only CRUD for veterinarian profiles.
- **Scheduling:** Administrators and Receptionists manage vet schedules; veterinarians see only their own schedules with a live "Busy Now" indicator.
- **Appointments:** Grouped into *Today*, *Upcoming*, and *Past* views; role-scoped so veterinarians see only their own appointments.
- **Available Time Slots:** AJAX endpoint that returns free 30-minute slots for a given vet and date, excluding already-booked and canceled appointments.
- **Medical Histories:** Vets record observations, diagnosis, and treatment when marking an appointment as *Completed*.
- **Notifications:** Automatically created when appointments are booked, updated, cancelled, or completed.
- **Daily Agenda:** View-backed daily list filtered per role and date.
- **Monthly Report:** View-backed statistics (appointments, cancellations, active vets, occupancy rate) with month/year filtering and Excel-compatible export.
- **User Accounts & Roles:** Admin-only CRUD; creating a Veterinarian user auto-creates a linked `Veterinarian` row and a default schedule.
- **Database Triggers:** Prevent double-booking of a vet's time and automatically flip a schedule's status to `Busy`.

## 🧑‍🍳 The Process

1. **Database Design:** Designed a normalized schema (Role, UserAccount, Owner, Veterinarian, Animal, Schedule, Appointment, MedicalHistory, Notification) with foreign keys, cascade deletes, and check constraints on status fields.
2. **DB-First Scaffolding:** Generated the EF Core context and entity models from the existing SQL Server database using `dotnet ef dbcontext scaffold`, keeping the schema authoritative.
3. **Authentication:** Implemented cookie-based authentication with a custom `AuthController` that validates credentials against `UserAccount` and issues claims for user ID and role.
4. **Role-Based Access Control:** Applied `[Authorize(Roles = "...")]` on every controller and used `User.IsInRole(...)` inside `Edit` actions to restrict field editing.
5. **Appointment Workflow:** Built the create/edit/delete flow, added time-slot selection driven by AJAX calls to `GetAvailableTimes`, and validated that appointments are not in the past or more than a year away.
6. **View-Backed Reporting:** Created SQL views `DailyAgenda` and `MonthlyReport` and mapped them as keyless EF entities, then built controllers to display and filter them.
7. **Triggers & Notifications:** Implemented database triggers to enforce time-slot uniqueness and update schedule status; created a `NotificationService` to record booking/cancellation/completion notifications.
8. **Migration to PostgreSQL:** Switched the provider from `Microsoft.EntityFrameworkCore.SqlServer` to `Npgsql.EntityFrameworkCore.PostgreSQL`, rewrote the T-SQL script in PostgreSQL syntax (`SERIAL`, `TIMESTAMP`, `BOOLEAN`, statement-level triggers with `REFERENCING NEW TABLE`), and updated the context's `OnModelCreating` to use lowercase identifiers and PostgreSQL-native types.

## 🧠 What I Learnt

- **Entity Framework Core (DB-First):** Understood how to keep the database as the source of truth and use scaffolding to generate models and context, versus code-first migrations.
- **Role-Based Authorization:** Learned to enforce permissions both at the controller level (`[Authorize(Roles = ...)]`) and inside action methods (branching on `User.IsInRole`).
- **Eager vs. Lazy Loading:** Used `Include` / `ThenInclude` extensively to avoid N+1 queries when rendering related entities in views.
- **Database Triggers:** Implemented business rules (time-slot uniqueness, schedule status) at the database layer rather than in C#, and learned how PostgreSQL's statement-level triggers with transition tables differ from SQL Server's row-level triggers with `inserted`.
- **Cross-Database Migration:** Gained hands-on experience translating a nontrivial T-SQL schema (identities, `NVARCHAR`, `BIT`, `DATETIME`, `ISNULL`, `RAISERROR`, `inserted` pseudo-table) into PostgreSQL equivalents (`SERIAL`, `VARCHAR`, `BOOLEAN`, `TIMESTAMP`, `COALESCE`, `RAISE EXCEPTION`, `REFERENCING NEW TABLE`).
- **Provider-Specific EF Core Behaviour:** Discovered that SQL Server is case-insensitive by default while PostgreSQL is case-sensitive, which required both a lowercase-identifier configuration in `OnModelCreating` and defensive `.ToLower()` comparisons in the login flow.
- **View-Mapped Entities:** Learned how to map SQL views to keyless EF entities (`HasNoKey().ToView(...)`) and query them like normal tables for reporting.
- **Cookie Authentication:** Implemented custom claims-based cookie sign-in, sliding expiration, and role claims without ASP.NET Core Identity.
- **AJAX in MVC:** Built a JSON endpoint used by a client-side calendar to filter available appointment slots dynamically.

## 🚥 Running the Project

1. **Prerequisites:** Install [.NET 7 SDK](https://dotnet.microsoft.com/download) and [PostgreSQL](https://www.postgresql.org/download/) (14 or newer).
2. **Clone the repository** and open the solution in Visual Studio 2022 / Rider / VS Code.
3. **Create the database:**
   ```bash
   psql -U postgres -c "DROP DATABASE IF EXISTS animalcaredb;"
   psql -U postgres -c "CREATE DATABASE animalcaredb;"
   psql -U postgres -d animalcaredb -f animalcare_pg.sql
   ```
4. **Update the connection string** in `appsettings.json` under `ConnectionStrings:MyConnectionString` with your PostgreSQL host, user, and password.
5. **Restore & run:**
   ```bash
   cd AnimalCareVeterinaryClinicDBFirst
   dotnet restore
   dotnet build
   dotnet run
   ```
6. **Log in** at `https://localhost:64862/Auth/Login` using one of the seed accounts:
   - `admin1` / `Admin@123` (Administrator)
   - `vet1` / `Vet@123` (Veterinarian)
   - `receptionist1` / `Receptionist@123` (Receptionist)
7. **Optional:** If the `vet1` account shows empty appointments/schedules (because it has no matching row in `veterinarian`), run:
   ```bash
   psql -U postgres -d animalcaredb -c \
     "UPDATE useraccount SET userfirstname='Emily', userlastname='Johnson' WHERE userlogin='vet1';"
   ```

## 🎛 Design Decision: DB-First vs. Code-First, and SQL Server vs. PostgreSQL

The application was originally developed against **SQL Server** using a **DB-First** workflow — the schema was authored first as a `.sql` script and the EF Core models were scaffolded from it. This made the database the authoritative source of truth, which was appropriate given the heavy use of views, check constraints, and triggers.

**Why PostgreSQL:** The project was migrated to PostgreSQL to avoid licensing and platform friction on macOS development machines, where SQL Server runs poorly (or not at all) without Docker. The migration preserved:

1. **Provider swap:** `Microsoft.EntityFrameworkCore.SqlServer` → `Npgsql.EntityFrameworkCore.PostgreSQL`.
2. **Schema translation:** `IDENTITY` → `SERIAL`, `NVARCHAR` → `VARCHAR`, `BIT` → `BOOLEAN`, `DATETIME` → `TIMESTAMP WITHOUT TIME ZONE`, `ISNULL` → `COALESCE`.
3. **Trigger rewrites:** SQL Server's `AFTER INSERT` with `inserted` became PostgreSQL statement-level triggers using `REFERENCING NEW TABLE AS new_rows` and PL/pgSQL functions.
4. **Case sensitivity:** Because PostgreSQL folds unquoted identifiers to lowercase while SQL Server is case-insensitive, the `OnModelCreating` method was updated to lower-case all table, column, and constraint names, and the login flow compares usernames with `.ToLower()`.

The trade-off accepted was a small amount of provider-specific code in the context in exchange for a fully cross-platform development experience. The DB-first approach was retained so the schema could continue to be hand-tuned (views, triggers, constraints) without fighting EF Core migrations.

## 🍿 Video Demonstration
https://github.com/user-attachments/assets/a948c60f-637d-4a17-9f6a-10a99556aeea
