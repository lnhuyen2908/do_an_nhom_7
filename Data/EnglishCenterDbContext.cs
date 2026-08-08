using Microsoft.EntityFrameworkCore;
using web_do_an1.Models;

namespace web_do_an1.Data;

// DbContext là cầu nối giữa các class C# và các bảng trong SQL Server.
public class EnglishCenterDbContext : DbContext
{
    // Nhận cấu hình kết nối database từ Program.cs rồi chuyển cho DbContext của Entity Framework.
    public EnglishCenterDbContext(DbContextOptions<EnglishCenterDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>(); // Khai báo bảng Vai trò.
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>(); // Khai báo bảng Tài khoản người dùng.
    public DbSet<Student> Students => Set<Student>(); // Khai báo bảng Học viên.
    public DbSet<Teacher> Teachers => Set<Teacher>(); // Khai báo bảng Giáo viên.
    public DbSet<Course> Courses => Set<Course>(); // Khai báo bảng Khóa học.
    public DbSet<CourseClass> CourseClasses => Set<CourseClass>(); // Khai báo bảng Lớp học.
    public DbSet<Enrollment> Enrollments => Set<Enrollment>(); // Khai báo bảng Đăng ký khóa học.
    public DbSet<Payment> Payments => Set<Payment>(); // Khai báo bảng Học phí.
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>(); // Bảng Giao dịch thanh toán.
    public DbSet<Score> Scores => Set<Score>(); // Bảng Điểm số.
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>(); // Bảng Điểm danh.
    public DbSet<CourseLecture> CourseLectures => Set<CourseLecture>(); // Bảng Bài giảng.
    public DbSet<SavedCourse> SavedCourses => Set<SavedCourse>(); // Bảng Khóa học đã lưu.
    public DbSet<Notification> Notifications => Set<Notification>(); // Bảng Thông báo.

    // OnModelCreating chạy khi Entity Framework xây dựng mô hình database.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Giữ lại cấu hình mặc định từ DbContext cha trước khi thêm cấu hình riêng.
        base.OnModelCreating(modelBuilder);

        // HasIndex tạo chỉ mục; IsUnique không cho phép dữ liệu trùng nhau.
        modelBuilder.Entity<Role>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<UserAccount>().HasIndex(x => x.UserName).IsUnique();
        // Mỗi học viên chỉ được nối với tối đa một tài khoản; bỏ qua các tài khoản không có StudentId.
        modelBuilder.Entity<UserAccount>().HasIndex(x => x.StudentId).IsUnique().HasFilter("[StudentId] IS NOT NULL");
        modelBuilder.Entity<UserAccount>().HasIndex(x => x.TeacherId).IsUnique().HasFilter("[TeacherId] IS NOT NULL");
        modelBuilder.Entity<Student>().HasIndex(x => x.Code).IsUnique(); // Mã học viên không được trùng.
        modelBuilder.Entity<Student>().HasIndex(x => x.Email).IsUnique(); // Email học viên không được trùng.
        modelBuilder.Entity<Teacher>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Teacher>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Course>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<CourseClass>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<CourseClass>().HasIndex(x => x.StartDate);
        modelBuilder.Entity<Enrollment>()
            // Tạo chỉ mục kết hợp từ mã học viên và mã khóa học.
            .HasIndex(x => new { x.StudentId, x.CourseId })
            // Không cho một học viên có hai đăng ký còn hiệu lực cho cùng một khóa học.
            .IsUnique()
            // Đăng ký đã hủy không bị tính, vì vậy học viên có thể đăng ký lại.
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

        // Lưu enum trạng thái đăng ký thành chữ như Pending, Approved, Cancelled thay vì số.
        modelBuilder.Entity<Enrollment>().Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Payment>().Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Payment>().Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<PaymentTransaction>().Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(30);

        modelBuilder.Entity<UserAccount>()
            // Một tài khoản thuộc về một vai trò.
            .HasOne(x => x.Role)
            // Một vai trò có thể được dùng bởi nhiều tài khoản.
            .WithMany(x => x.UserAccounts)
            // RoleId trong UserAccount là khóa ngoại.
            .HasForeignKey(x => x.RoleId)
            // Không cho xóa vai trò nếu vẫn còn tài khoản đang sử dụng.
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserAccount>()
            // Một tài khoản học viên liên kết với một Student.
            .HasOne(x => x.Student)
            // Một Student cũng chỉ có một UserAccount.
            .WithOne(x => x.UserAccount)
            // StudentId nằm trong bảng UserAccounts và là khóa ngoại.
            .HasForeignKey<UserAccount>(x => x.StudentId)
            // Nếu xóa Student thì StudentId trong tài khoản được đặt thành null.
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
            // Một phiếu đăng ký thuộc về một học viên.
            .HasOne(x => x.Student)
            // Một học viên có thể có nhiều phiếu đăng ký.
            .WithMany(x => x.Enrollments)
            // StudentId là khóa ngoại nối hai bảng.
            .HasForeignKey(x => x.StudentId)
            // Không cho xóa học viên khi vẫn còn dữ liệu đăng ký liên quan.
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            // Một phiếu đăng ký thuộc về một khóa học.
            .HasOne(x => x.Course)
            // Một khóa học có thể có nhiều học viên đăng ký.
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
            // Một khoản học phí thuộc về một phiếu đăng ký.
            .HasOne(x => x.Enrollment)
            // Một phiếu đăng ký chỉ có một khoản học phí.
            .WithOne(x => x.Payment)
            .HasForeignKey<Payment>(x => x.EnrollmentId)
            // Khi xóa phiếu đăng ký thì khoản học phí liên quan cũng bị xóa.
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

        modelBuilder.Entity<Notification>().HasIndex(x => new { x.UserAccountId, x.IsRead, x.CreatedAt });

        modelBuilder.Entity<CourseClass>().Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<PaymentTransaction>().Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

        modelBuilder.Entity<Notification>()
            .HasOne(x => x.UserAccount)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.UserAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
