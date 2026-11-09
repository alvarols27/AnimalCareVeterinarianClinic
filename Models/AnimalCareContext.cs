using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AnimalCareVeterinaryClinicDBFirst.Models;

public partial class AnimalCareContext : DbContext
{
    public AnimalCareContext() { }

    public AnimalCareContext(DbContextOptions<AnimalCareContext> options)
        : base(options) { }

    public virtual DbSet<Animal> Animals { get; set; }
    public virtual DbSet<Appointment> Appointments { get; set; }
    public virtual DbSet<DailyAgendum> DailyAgenda { get; set; }
    public virtual DbSet<MedicalHistory> MedicalHistories { get; set; }
    public virtual DbSet<MonthlyReport> MonthlyReports { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<Owner> Owners { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Schedule> Schedules { get; set; }
    public virtual DbSet<UserAccount> UserAccounts { get; set; }
    public virtual DbSet<Veterinarian> Veterinarians { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // =====================================================================
        // PG-MIGRATION (Fix B): force every table, column, key, FK, and index
        // name to lowercase so EF Core emits unquoted identifiers that match
        // the lowercase schema from animalcare_pg.sql.
        // (PostgreSQL folds unquoted identifiers to lowercase, so this makes
        // EF Core's SQL match the actual table/column names in the DB.)
        // =====================================================================
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (tableName != null)
                entity.SetTableName(tableName.ToLowerInvariant());

            var viewName = entity.GetViewName();
            if (viewName != null)
                entity.SetViewName(viewName.ToLowerInvariant());

            foreach (var prop in entity.GetProperties())
            {
                var colName = prop.GetColumnName();
                if (!string.IsNullOrEmpty(colName))
                    prop.SetColumnName(colName.ToLowerInvariant());
            }

            foreach (var key in entity.GetKeys())
            {
                var name = key.GetName();
                if (!string.IsNullOrEmpty(name))
                    key.SetName(name.ToLowerInvariant());
            }

            foreach (var fk in entity.GetForeignKeys())
            {
                var name = fk.GetConstraintName();
                if (!string.IsNullOrEmpty(name))
                    fk.SetConstraintName(name.ToLowerInvariant());
            }

            foreach (var idx in entity.GetIndexes())
            {
                var name = idx.GetDatabaseName();
                if (!string.IsNullOrEmpty(name))
                    idx.SetDatabaseName(name.ToLowerInvariant());
            }
        }

        // Configure Animal entity
        modelBuilder.Entity<Animal>(entity =>
        {
            entity.HasKey(e => e.AnimalId).HasName("pk_animal");
            entity.ToTable("animal");
            entity.HasIndex(e => e.OwnerId, "ix_animal_ownerid");

            entity.Property(e => e.AnimalName).HasMaxLength(50);
            entity.Property(e => e.AnimalWeight).HasColumnType("numeric(5, 2)");
            entity.Property(e => e.BirthDate).HasColumnType("date");
            entity.Property(e => e.Breed).HasMaxLength(50);
            entity.Property(e => e.Sex)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Species).HasMaxLength(50);

            entity.HasOne(d => d.Owner).WithMany(p => p.Animals)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("fk_animal_owner");
        });

        // Configure Appointment entity
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("pk_appointment");

            // PG-MIGRATION (7h): SQL-Server-specific HasTrigger(...) block removed.
            // Triggers live inside the PostgreSQL script.
            entity.ToTable("appointment");

            entity.HasIndex(e => e.AnimalId, "ix_appointment_animalid");
            entity.HasIndex(e => e.StartDateTime, "ix_appointment_startdt");
            entity.HasIndex(e => e.VetId, "ix_appointment_vetid");

            entity.Property(e => e.AppointmentStatus).HasMaxLength(15);
            entity.Property(e => e.EndDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Purpose).HasMaxLength(255);
            entity.Property(e => e.StartDateTime).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Animal).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.AnimalId)
                .HasConstraintName("fk_appointment_animal");

            entity.HasOne(d => d.Vet).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.VetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_appointment_vet");
        });

        // Configure DailyAgendum view entity
        modelBuilder.Entity<DailyAgendum>(entity =>
        {
            entity.HasNoKey().ToView("dailyagenda");

            entity.Property(e => e.AnimalName).HasMaxLength(50);
            entity.Property(e => e.AppointmentStatus).HasMaxLength(15);
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.OwnerName).HasMaxLength(101);
            entity.Property(e => e.VeterinarianName).HasMaxLength(101);
        });

        // Configure MedicalHistory entity
        modelBuilder.Entity<MedicalHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("pk_medicalhistory");
            entity.ToTable("medicalhistory");

            entity.HasIndex(e => e.AnimalId, "ix_medicalhistory_animalid");
            entity.HasIndex(e => e.VetId, "ix_medicalhistory_vetid");

            entity.Property(e => e.ConsultationDate).HasColumnType("date");

            entity.HasOne(d => d.Animal).WithMany(p => p.MedicalHistories)
                .HasForeignKey(d => d.AnimalId)
                .HasConstraintName("fk_medicalhistory_animal");

            entity.HasOne(d => d.Vet).WithMany(p => p.MedicalHistories)
                .HasForeignKey(d => d.VetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_medicalhistory_vet");
        });

        // Configure MonthlyReport view entity
        modelBuilder.Entity<MonthlyReport>(entity =>
        {
            entity.HasNoKey().ToView("monthlyreport");
            entity.Property(e => e.OccupancyRate).HasColumnType("numeric(5, 2)");
        });

        // Configure Notification entity
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("pk_notification");
            entity.ToTable("notification");

            entity.HasIndex(e => e.AppointmentId, "ix_notification_appt");

            entity.Property(e => e.Channel).HasMaxLength(10);
            entity.Property(e => e.NotificationStatus).HasMaxLength(10);
            entity.Property(e => e.NotificationType).HasMaxLength(20);
            entity.Property(e => e.SendDate).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("fk_notification_appointment");
        });

        // Configure Owner entity
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.OwnerId).HasName("pk_owner");
            entity.ToTable("owner");

            entity.Property(e => e.OwnerAddress).HasMaxLength(255);
            entity.Property(e => e.OwnerEmail).HasMaxLength(100);
            entity.Property(e => e.OwnerFirstName).HasMaxLength(50);
            entity.Property(e => e.OwnerLastName).HasMaxLength(50);
            entity.Property(e => e.OwnerPhone).HasMaxLength(20);
        });

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("pk_role");
            entity.ToTable("role");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        // Configure Schedule entity
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("pk_schedule");
            entity.ToTable("schedule");

            entity.HasIndex(e => e.VetId, "ix_schedule_vetid");

            entity.Property(e => e.ScheduleDate).HasColumnType("date");
            entity.Property(e => e.ScheduleStatus).HasMaxLength(10);

            entity.HasOne(d => d.Vet).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.VetId)
                .HasConstraintName("fk_schedule_vet");
        });

        // Configure UserAccount entity
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("pk_useraccount");
            entity.ToTable("useraccount");

            entity.HasIndex(e => e.RoleId, "ix_useraccount_roleid");
            entity.HasIndex(e => e.UserLogin, "ix_useraccount_userlogin").IsUnique();

            // PostgreSQL boolean default (was ((1)) on SQL Server)
            entity.Property(e => e.UserActive).HasDefaultValueSql("true");
            entity.Property(e => e.UserFirstName).HasMaxLength(50);
            entity.Property(e => e.UserLastName).HasMaxLength(50);
            entity.Property(e => e.UserLogin).HasMaxLength(100);
            entity.Property(e => e.UserPassword).HasMaxLength(255);

            entity.HasOne(d => d.Role).WithMany(p => p.UserAccounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_useraccount_role");
        });

        // Configure Veterinarian entity
        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasKey(e => e.VetId).HasName("pk_veterinarian");
            entity.ToTable("veterinarian");

            entity.Property(e => e.VetEmail).HasMaxLength(100);
            entity.Property(e => e.VetFirstName).HasMaxLength(50);
            entity.Property(e => e.VetLastName).HasMaxLength(50);
            entity.Property(e => e.VetPhone).HasMaxLength(20);
            entity.Property(e => e.VetSpecialty).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}