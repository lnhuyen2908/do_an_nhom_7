using Microsoft.EntityFrameworkCore;
using web_do_an1.Models;

namespace web_do_an1.Data;

public class EnglishCenterDbContext : DbContext
{
    public EnglishCenterDbContext(DbContextOptions<EnglishCenterDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseClass> CourseClasses => Set<CourseClass>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<CourseLecture> CourseLectures => Set<CourseLecture>();
    public DbSet<SavedCourse> SavedCourses => Set<SavedCourse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<UserAccount>().HasIndex(x => x.UserName).IsUnique();
        modelBuilder.Entity<UserAccount>().HasIndex(x => x.StudentId).IsUnique().HasFilter("[StudentId] IS NOT NULL");
        modelBuilder.Entity<UserAccount>().HasIndex(x => x.TeacherId).IsUnique().HasFilter("[TeacherId] IS NOT NULL");
        modelBuilder.Entity<Student>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Student>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Teacher>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Teacher>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Course>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<CourseClass>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Enrollment>()
            .HasIndex(x => new { x.StudentId, x.CourseId })
            .IsUnique()
            .HasFilter("[Status] <> 'Cancelled'");
        modelBuilder.Entity<Payment>().HasIndex(x => x.EnrollmentId).IsUnique();
        modelBuilder.Entity<Score>().HasIndex(x => new { x.StudentId, x.CourseClassId }).IsUnique();
        modelBuilder.Entity<AttendanceRecord>()
            .HasIndex(x => new { x.StudentId, x.CourseClassId, x.StudyDate })
            .IsUnique();
        modelBuilder.Entity<SavedCourse>().HasIndex(x => new { x.StudentId, x.CourseId }).IsUnique();

        modelBuilder.Entity<Course>().Property(x => x.Tuition).HasPrecision(18, 2);
        modelBuilder.Entity<Payment>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<Payment>().Property(x => x.PaidAmount).HasPrecision(18, 2);
        modelBuilder.Entity<PaymentTransaction>().Property(x => x.Amount).HasPrecision(18, 2);

        modelBuilder.Entity<Enrollment>().Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Payment>().Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Payment>().Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<PaymentTransaction>().Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(30);

        modelBuilder.Entity<UserAccount>()
            .HasOne(x => x.Role)
            .WithMany(x => x.UserAccounts)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserAccount>()
            .HasOne(x => x.Student)
            .WithOne(x => x.UserAccount)
            .HasForeignKey<UserAccount>(x => x.StudentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<UserAccount>()
            .HasOne(x => x.Teacher)
            .WithOne(x => x.UserAccount)
            .HasForeignKey<UserAccount>(x => x.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CourseClass>()
            .HasOne(x => x.Course)
            .WithMany(x => x.CourseClasses)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CourseClass>()
            .HasOne(x => x.Teacher)
            .WithMany(x => x.CourseClasses)
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(x => x.Student)
            .WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(x => x.Course)
            .WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(x => x.CourseClass)
            .WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.CourseClassId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Payment>()
            .HasOne(x => x.Student)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(x => x.Enrollment)
            .WithOne(x => x.Payment)
            .HasForeignKey<Payment>(x => x.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PaymentTransaction>()
            .HasOne(x => x.Payment)
            .WithMany(x => x.PaymentTransactions)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PaymentTransaction>()
            .HasOne(x => x.Student)
            .WithMany(x => x.PaymentTransactions)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Score>()
            .HasOne(x => x.Student)
            .WithMany(x => x.Scores)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Score>()
            .HasOne(x => x.CourseClass)
            .WithMany(x => x.Scores)
            .HasForeignKey(x => x.CourseClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(x => x.Student)
            .WithMany(x => x.AttendanceRecords)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(x => x.CourseClass)
            .WithMany(x => x.AttendanceRecords)
            .HasForeignKey(x => x.CourseClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CourseLecture>()
            .HasOne(x => x.Course)
            .WithMany(x => x.CourseLectures)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CourseLecture>()
            .HasOne(x => x.Teacher)
            .WithMany(x => x.CourseLectures)
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SavedCourse>()
            .HasOne(x => x.Student)
            .WithMany(x => x.SavedCourses)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SavedCourse>()
            .HasOne(x => x.Course)
            .WithMany(x => x.SavedCourses)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
