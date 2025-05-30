using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models.Appointments;
using ProyectoFinal.Models.Departments;
using ProyectoFinal.Models.Districts;
using ProyectoFinal.Models.Doctors;
using ProyectoFinal.Models.Institutions;
using ProyectoFinal.Models.Municipalities;
using ProyectoFinal.Models.Patients;
using ProyectoFinal.Models.Permissions;
using ProyectoFinal.Models.Specialties;
using ProyectoFinal.Models.Users;
using System.Linq.Expressions;

namespace ProyectoFinal.Models.Base;

public partial class DbCitasMedicasContext : DbContext
{
    public DbCitasMedicasContext()
    {
    }

    public DbCitasMedicasContext(DbContextOptions<DbCitasMedicasContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<Institution> Institutions { get; set; }

    public virtual DbSet<Municipality> Municipalities { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Specialty> Specialties { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserLoginHistory> UserLoginHistories { get; set; }

    public virtual DbSet<UserPermission> UserPermissions { get; set; }

    public virtual DbSet<UserToken> UserTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Name=DefaultConnection");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCC2C434A351");

            entity.HasIndex(e => e.AppointmentDate, "IDX_Appointments_AppointmentDate");

            entity.HasIndex(e => e.AppointmentId, "IDX_Appointments_AppointmentId");

            entity.HasIndex(e => e.DoctorId, "IDX_Appointments_DoctorId");

            entity.HasIndex(e => e.InstitutionId, "IDX_Appointments_InstitutionId");

            entity.Property(e => e.AppointmentDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.DeletedBy).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasMaxLength(50);

            entity.Property(e => e.Status)
                .HasDefaultValue(AppointmentStatus.Scheduled)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Appointments_Doctors");

            entity.HasOne(d => d.Institution).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.InstitutionId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Appointments_Institutions");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Appointments_Patients");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentCode).HasName("PK__Departme__6EA8896CECBB558C");

            entity.Property(e => e.DepartmentCode).HasMaxLength(2);
            entity.Property(e => e.DepartmentName).HasMaxLength(50);
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.DistrictCode).HasName("PK__District__3D4E86AA957D26B2");

            entity.Property(e => e.DistrictCode).HasMaxLength(4);
            entity.Property(e => e.DistrictName).HasMaxLength(50);
            entity.Property(e => e.MunicipalityCode).HasMaxLength(4);

            entity.HasOne(d => d.MunicipalityCodeNavigation).WithMany(p => p.Districts)
                .HasForeignKey(d => d.MunicipalityCode)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK__Districts__Munic__3C69FB99");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorId).HasName("PK__Doctors__2DC00EBF76B447A7");

            entity.HasIndex(e => e.Dui, "IDX_Doctors_DUI");

            entity.HasIndex(e => e.DoctorId, "IDX_Doctors_DoctorId");

            entity.HasIndex(e => e.InstitutionId, "IDX_Doctors_InstitutionId");

            entity.HasIndex(e => e.SpecialtyId, "IDX_Doctors_SpecialtyId");

            entity.HasIndex(e => e.Email, "UQ__Doctors__A9D10534ECE39BC7").IsUnique();

            entity.HasIndex(e => e.Dui, "UQ__Doctors__C03671B9CE3A8576").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.DeletedBy).HasMaxLength(50);
            entity.Property(e => e.Dui)
                .HasMaxLength(9)
                .HasColumnName("DUI");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.MiddleName).HasMaxLength(50);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(8);
            entity.Property(e => e.SecondLastName).HasMaxLength(50);

            entity.HasOne(d => d.Institution).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.InstitutionId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK__Doctors__Institu__6383C8BA");

            entity.HasOne(d => d.Specialty).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.SpecialtyId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK__Doctors__Special__628FA481");
        });

        modelBuilder.Entity<Institution>(entity =>
        {
            entity.HasKey(e => e.InstitutionId).HasName("PK__Institut__8DF6B6AD35E2768A");

            entity.HasIndex(e => e.DistrictCode, "IDX_Institutions_DistrictCode");

            entity.HasIndex(e => e.InstitutionId, "IDX_Institutions_InstitutionId");

            entity.HasIndex(e => e.Name, "IDX_Institutions_Name");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.DeletedBy).HasMaxLength(50);
            entity.Property(e => e.DistrictCode).HasMaxLength(4);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(8);

            entity.HasOne(d => d.District).WithMany(p => p.Institutions)
                .HasForeignKey(d => d.DistrictCode)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK__Instituti__Distr__5BE2A6F2");
        });

        modelBuilder.Entity<Municipality>(entity =>
        {
            entity.HasKey(e => e.MunicipalityCode).HasName("PK__Municipa__582D8400C9624328");

            entity.Property(e => e.MunicipalityCode).HasMaxLength(4);
            entity.Property(e => e.DepartmentCode).HasMaxLength(2);
            entity.Property(e => e.MunicipalityName).HasMaxLength(50);

            entity.HasOne(d => d.DepartmentCodeNavigation).WithMany(p => p.Municipalities)
                .HasForeignKey(d => d.DepartmentCode)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK__Municipal__Depar__398D8EEE");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__Patients__970EC366DA5BA251");

            entity.HasIndex(e => e.Dui, "IDX_Patients_DUI");

            entity.HasIndex(e => e.PatientId, "IDX_Patients_PatientId");

            entity.HasIndex(e => e.Dui, "UQ__Patients__C03671B96FD1186B").IsUnique();

            entity.HasIndex(e => e.UserId, "IDX_Patient_UserId");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.DeletedBy).HasMaxLength(50);
            entity.Property(e => e.Dui)
                .HasMaxLength(9)
                .HasColumnName("DUI");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.MiddleName).HasMaxLength(50);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(8);
            entity.Property(e => e.SecondLastName).HasMaxLength(50);

            entity.HasOne(e => e.User).WithMany(e => e.Patients)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK__Patients__User__666");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissi__EFA6FB2FA762D8F1");

            entity.HasIndex(e => e.PermissionId, "IDX_Permissions_PermissionId");

            entity.HasIndex(e => e.PermissionName, "UQ__Permissi__0FFDA357060CF395").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PermissionName).HasMaxLength(50);
        });

        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.HasKey(e => e.SpecialtyId).HasName("PK__Specialt__D768F6A881DCA376");

            entity.HasIndex(e => e.SpecialtyId, "IDX_Specialties_SpecialtyId");

            entity.HasIndex(e => e.Name, "IDX_Specialties_SpecialtyName");

            entity.HasIndex(e => e.Name, "UQ__Specialt__7DCA5748215E07C4").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.DeletedBy).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CE147ED69");

            entity.HasIndex(e => e.UserId, "IDX_Users_UserId");

            entity.HasIndex(e => e.Username, "IDX_Users_Username");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E48305B4B7").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534C83A046E").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.DeletedBy).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<UserLoginHistory>(entity =>
        {
            entity.HasKey(e => e.LoginId).HasName("PK__UserLogi__4DDA28189AD2E5C5");

            entity.ToTable("UserLoginHistory");

            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.LoginDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.UserLoginHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserLoginHistory_Users");
        });

        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.PermissionId });

            entity.Property(e => e.GrantedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.GrantedBy).HasMaxLength(50);

            entity.HasOne(d => d.Permission).WithMany(p => p.UserPermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_UserPermissions_Permissions");

            entity.HasOne(d => d.User).WithMany(p => p.UserPermissions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_UserPermissions_Users");
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__UserToke__658FEEEABDD8CD70");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Expiration).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.UserTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserTokens_Users");
        });


        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsActive));
                var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(true)), parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
