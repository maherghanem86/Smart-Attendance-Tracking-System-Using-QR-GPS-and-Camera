using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SmartAttendance.API.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AbsenceExcuse> AbsenceExcuses { get; set; }

    public virtual DbSet<AcademicProfile> AcademicProfiles { get; set; }

    public virtual DbSet<AttendanceLog> AttendanceLogs { get; set; }

    public virtual DbSet<AttendanceSession> AttendanceSessions { get; set; }

    public virtual DbSet<AuditTrail> AuditTrails { get; set; }

    public virtual DbSet<Building> Buildings { get; set; }

    public virtual DbSet<Campus> Campuses { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Faculty> Faculties { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<SecurityAlert> SecurityAlerts { get; set; }

    public virtual DbSet<StudentProfile> StudentProfiles { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    public virtual DbSet<User> Users { get; set; }
        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AbsenceExcuse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AbsenceE__3214EC07BD1BDB5B");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Session).WithMany(p => p.AbsenceExcuses)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("FK__AbsenceEx__Sessi__6383C8BA");

            entity.HasOne(d => d.Student).WithMany(p => p.AbsenceExcuses)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__AbsenceEx__Stude__628FA481");
        });

        modelBuilder.Entity<AcademicProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Academic__1788CC4C7CE36973");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Specialization).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.User).WithOne(p => p.AcademicProfile)
                .HasForeignKey<AcademicProfile>(d => d.UserId)
                .HasConstraintName("FK__AcademicP__UserI__3D5E1FD2");
        });

        modelBuilder.Entity<AttendanceLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attendan__3214EC0711956CD7");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CheckInTime).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Present");

            entity.HasOne(d => d.Session).WithMany(p => p.AttendanceLogs)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("FK__Attendanc__Sessi__5CD6CB2B");

            entity.HasOne(d => d.Student).WithMany(p => p.AttendanceLogs)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Attendanc__Stude__5BE2A6F2");
        });

        modelBuilder.Entity<AttendanceSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attendan__3214EC07A15D059F");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DynamicQrcode).HasColumnName("DynamicQRCode");
            entity.Property(e => e.IsActive).HasDefaultValue(false);
            entity.Property(e => e.OtpBackup)
                .HasMaxLength(10)
                .HasColumnName("OTP_Backup");
            entity.Property(e => e.SessionDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Schedule).WithMany(p => p.AttendanceSessions)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Attendanc__Sched__5629CD9C");
        });

        modelBuilder.Entity<AuditTrail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditTra__3214EC072E8FB93E");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ActionType).HasMaxLength(100);
            entity.Property(e => e.TableName).HasMaxLength(100);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(sysdatetimeoffset())");

            entity.HasOne(d => d.ActionByNavigation).WithMany(p => p.AuditTrails)
                .HasForeignKey(d => d.ActionBy)
                .HasConstraintName("FK__AuditTrai__Actio__68487DD7");
        });

        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Building__3214EC07E884DCC4");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Campus).WithMany(p => p.Buildings)
                .HasForeignKey(d => d.CampusId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Buildings__Campu__1BFD2C07");
        });

        modelBuilder.Entity<Campus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Campuses__3214EC0768609E65");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Courses__3214EC07C20F0108");

            entity.HasIndex(e => e.CourseCode, "UQ__Courses__FC00E0007CF95F9D").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CourseCode).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Dept).WithMany(p => p.Courses)
                .HasForeignKey(d => d.DeptId)
                .HasConstraintName("FK__Courses__DeptId__4222D4EF");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departme__3214EC07457EF9D9");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DeptHead).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Faculty).WithMany(p => p.Departments)
                .HasForeignKey(d => d.FacultyId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Departmen__Facul__15502E78");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Enrollme__3214EC075D95F16B");

            entity.HasIndex(e => new { e.StudentId, e.SectionId }, "UQ__Enrollme__0ACBDB1F80F4372D").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.EnrollmentDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Section).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Enrollmen__Secti__4CA06362");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Enrollmen__Stude__4BAC3F29");
        });

        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Facultie__3214EC07A93D8BFA");

            entity.HasIndex(e => e.Code, "UQ__Facultie__A25C5AA7156BAF54").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.DeanName).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC0724503568");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Notificat__UserI__73BA3083");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Permissi__3214EC07BAB92D53");

            entity.HasIndex(e => e.PermissionName, "UQ__Permissi__0FFDA35741A35FCF").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.PermissionName).HasMaxLength(100);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07D4652C17");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160444D8555").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RoleName).HasMaxLength(50);

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("FK__RolePermi__Permi__2B3F6F97"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK__RolePermi__RoleI__2A4B4B5E"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId").HasName("PK__RolePerm__6400A1A8EB6871A5");
                        j.ToTable("RolePermissions");
                    });
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Rooms__3214EC07C488F3BF");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.RoomNumber).HasMaxLength(50);
            entity.Property(e => e.RoomType).HasMaxLength(50);

            entity.HasOne(d => d.Building).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.BuildingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Rooms__BuildingI__1FCDBCEB");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Schedule__3214EC07DD314652");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Room).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__Schedules__RoomI__52593CB8");

            entity.HasOne(d => d.Section).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Schedules__Secti__5165187F");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sections__3214EC07951507A3");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Semester).HasMaxLength(50);

            entity.HasOne(d => d.Course).WithMany(p => p.Sections)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Sections__Course__45F365D3");

            entity.HasOne(d => d.Instructor).WithMany(p => p.Sections)
                .HasForeignKey(d => d.InstructorId)
                .HasConstraintName("FK__Sections__Instru__46E78A0C");
        });

        modelBuilder.Entity<SecurityAlert>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Security__3214EC07D60C2566");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DetectedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.Severity).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.SecurityAlerts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__SecurityA__UserI__6D0D32F4");
        });

        modelBuilder.Entity<StudentProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__StudentP__1788CC4CD614B388");

            entity.HasIndex(e => e.UniversityId, "UQ__StudentP__9F19E19D68C29B22").IsUnique();

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.UniversityId)
                .HasMaxLength(50)
                .HasColumnName("UniversityID");

            entity.HasOne(d => d.Major).WithMany(p => p.StudentProfiles)
                .HasForeignKey(d => d.MajorId)
                .HasConstraintName("FK__StudentPr__Major__3A81B327");

            entity.HasOne(d => d.User).WithOne(p => p.StudentProfile)
                .HasForeignKey<StudentProfile>(d => d.UserId)
                .HasConstraintName("FK__StudentPr__UserI__398D8EEE");
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("PK__SystemSe__C41E02889D1F8AC0");

            entity.Property(e => e.Key).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC076C8D83D9");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E436B97D47").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105346979C583").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Username).HasMaxLength(100);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK__UserRoles__RoleI__35BCFE0A"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK__UserRoles__UserI__34C8D9D1"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__UserRole__AF2760AD6E353B97");
                        j.ToTable("UserRoles");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
