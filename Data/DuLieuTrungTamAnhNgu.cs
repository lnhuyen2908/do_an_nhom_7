using Microsoft.EntityFrameworkCore;

using web_do_an1.Models;

namespace web_do_an1.Data
{
    public class EnglishCenterDbContext : DbContext
    {
        public EnglishCenterDbContext(DbContextOptions<EnglishCenterDbContext> options) : base(options)
        {

        }

        public DbSet<RoleItem> Roles => Set<RoleItem>();
        public DbSet<UserAccount> Users => Set<UserAccount>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<CourseClass> Classes => Set<CourseClass>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        public DbSet<Score> Scores => Set<Score>();
        public DbSet<AttendanceRecord> Attendance => Set<AttendanceRecord>();
        public DbSet<CourseLecture> Lectures => Set<CourseLecture>();
        public DbSet<SavedCourse> SavedCourses => Set<SavedCourse>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RoleItem>().ToTable("VaiTro");
            modelBuilder.Entity<UserAccount>().ToTable("TaiKhoan");
            modelBuilder.Entity<Student>().ToTable("HocVien");
            modelBuilder.Entity<Teacher>().ToTable("GiaoVien");
            modelBuilder.Entity<Course>().ToTable("KhoaHoc");
            modelBuilder.Entity<CourseClass>().ToTable("LopHoc");
            modelBuilder.Entity<Enrollment>().ToTable("DangKy");
            modelBuilder.Entity<Payment>().ToTable("HocPhi");
            modelBuilder.Entity<PaymentTransaction>().ToTable("LichSuThanhToan");
            modelBuilder.Entity<Score>().ToTable("DiemSo");
            modelBuilder.Entity<AttendanceRecord>().ToTable("DiemDanh");
            modelBuilder.Entity<CourseLecture>().ToTable("BaiGiang");
            modelBuilder.Entity<SavedCourse>().ToTable("KhoaHocDaLuu");
            modelBuilder.Entity<Course>().Property(x => x.Tuition).HasPrecision(18, 2);
            modelBuilder.Entity<Payment>().Property(x => x.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<Payment>().Property(x => x.PaidAmount).HasPrecision(18, 2);
            modelBuilder.Entity<PaymentTransaction>().Property(x => x.Amount).HasPrecision(18, 2);

            modelBuilder.Entity<UserAccount>().HasIndex(x => x.UserName).IsUnique();
            modelBuilder.Entity<Course>().HasIndex(x => x.Code).IsUnique();
            modelBuilder.Entity<Student>().HasIndex(x => x.Code).IsUnique();
            modelBuilder.Entity<Teacher>().HasIndex(x => x.Code).IsUnique();
            modelBuilder.Entity<RoleItem>().HasAlternateKey(x => x.Name);
            modelBuilder.Entity<Payment>().HasIndex(x => x.EnrollmentId).IsUnique();
            modelBuilder.Entity<Score>().HasIndex(x => new { x.StudentId, x.ClassId }).IsUnique();
            modelBuilder.Entity<SavedCourse>().HasIndex(x => new { x.StudentId, x.CourseId }).IsUnique();

            modelBuilder.Entity<UserAccount>()
                .HasOne(x => x.RoleItem)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.Role)
                .HasPrincipalKey(x => x.Name)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CourseClass>()
                .HasOne(x => x.Course)
                .WithMany(x => x.Classes)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CourseClass>()
                .HasOne(x => x.Teacher)
                .WithMany(x => x.Classes)
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
                .HasOne(x => x.AssignedClass)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.ClassId)
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
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Score>()
                .HasOne(x => x.Student)
                .WithMany(x => x.Scores)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Score>()
                .HasOne(x => x.CourseClass)
                .WithMany(x => x.Scores)
                .HasForeignKey(x => x.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(x => x.Student)
                .WithMany(x => x.AttendanceRecords)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(x => x.CourseClass)
                .WithMany(x => x.AttendanceRecords)
                .HasForeignKey(x => x.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentTransaction>()
                .HasOne(x => x.Payment)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentTransaction>()
                .HasOne(x => x.Student)
                .WithMany(x => x.PaymentTransactions)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CourseLecture>()
                .HasOne(x => x.Course)
                .WithMany(x => x.Lectures)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CourseLecture>()
                .HasOne(x => x.Teacher)
                .WithMany(x => x.Lectures)
                .HasForeignKey(x => x.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SavedCourse>()
                .HasOne(x => x.Student)
                .WithMany(x => x.SavedCourses)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SavedCourse>()
                .HasOne(x => x.Course)
                .WithMany(x => x.SavedCourses)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoleItem>().HasData(
                new RoleItem { Id = 1, Name = "Admin", DisplayName = "Quản trị viên" },
                new RoleItem { Id = 2, Name = "Teacher", DisplayName = "Giáo viên" },
                new RoleItem { Id = 3, Name = "Student", DisplayName = "Học viên" },
                new RoleItem { Id = 4, Name = "Staff", DisplayName = "Nhân viên đào tạo" });

            modelBuilder.Entity<Teacher>().HasData(EnglishCenterStore.TeacherProfiles
                .Select(x => new Teacher
                {
                    Id = x.Id,
                    Code = x.Code,
                    FullName = x.FullName,
                    Email = x.Email,
                    Phone = x.Phone,
                    Specialty = x.Specialty
                })
                .ToArray());

            var seededStudents = EnglishCenterStore.StudentNames
                .Select((fullName, index) => new Student
                {
                    Id = index + 1,
                    Code = $"HV{index + 1:00}",
                    FullName = fullName,
                    Email = $"hv{index + 1:00}@englishcenter.vn",
                    Phone = $"0912{index + 1:000000}",
                    DateOfBirth = new DateTime(2004, (index % 12) + 1, (index % 24) + 1),
                    Address = "TP.HCM"
                })
                .ToArray();

            modelBuilder.Entity<Student>().HasData(seededStudents);

            modelBuilder.Entity<Course>().HasData(
                new Course { Id = 1, Code = "KH01", Name = "English Basic", Level = "A1", Tuition = 1500000, Duration = "8 tuần", Description = "Khóa học nền tảng phát âm, từ vựng và ngữ pháp cơ bản.", ImageUrl = "/images/courses/general.jpg" },
                new Course { Id = 2, Code = "KH02", Name = "Giao tiếp cơ bản", Level = "A2", Tuition = 2000000, Duration = "10 tuần", Description = "Rèn luyện phản xạ nghe nói trong tình huống hằng ngày.", ImageUrl = "/images/courses/communication.jpg" },
                new Course { Id = 3, Code = "KH03", Name = "IELTS Foundation", Level = "B1", Tuition = 3500000, Duration = "12 tuần", Description = "Làm quen IELTS Listening, Reading, Writing và Speaking.", ImageUrl = "/images/courses/ielts.jpg" },
                new Course { Id = 4, Code = "KH04", Name = "TOEIC 450+", Level = "TOEIC", Tuition = 2800000, Duration = "10 tuần", Description = "Hệ thống kiến thức trọng tâm cho mục tiêu TOEIC 450+.", ImageUrl = "/images/courses/toeic.jpg" },
                new Course { Id = 5, Code = "KH05", Name = "IELTS Level 5.5", Level = "IELTS", Tuition = 4200000, Duration = "12 tuần", Description = "Lộ trình tăng tốc cho học viên cần đạt IELTS 5.5 với chiến lược làm bài theo từng kỹ năng.", ImageUrl = "/images/courses/ielts.jpg" },
                new Course { Id = 6, Code = "KH06", Name = "IELTS Level 6.5", Level = "IELTS", Tuition = 5200000, Duration = "14 tuần", Description = "Khóa học phát triển tư duy học thuật và kỹ năng xử lý đề IELTS mục tiêu 6.5.", ImageUrl = "/images/courses/ielts.jpg" },
                new Course { Id = 7, Code = "KH07", Name = "IELTS Level 7.5", Level = "IELTS", Tuition = 6800000, Duration = "16 tuần", Description = "Luyện đề nâng cao, tối ưu band điểm Writing và Speaking cho mục tiêu IELTS 7.5.", ImageUrl = "/images/courses/ielts.jpg" },
                new Course { Id = 8, Code = "KH08", Name = "TOEIC 650+", Level = "TOEIC", Tuition = 3600000, Duration = "12 tuần", Description = "Củng cố ngữ pháp, từ vựng và chiến thuật làm bài cho mục tiêu TOEIC 650+.", ImageUrl = "/images/courses/toeic.jpg" },
                new Course { Id = 9, Code = "KH09", Name = "TOEIC 750+", Level = "TOEIC", Tuition = 4600000, Duration = "12 tuần", Description = "Khóa học giải đề chuyên sâu, tăng tốc độ nghe đọc và xử lý bẫy đáp án.", ImageUrl = "/images/courses/toeic.jpg" },
                new Course { Id = 10, Code = "KH10", Name = "Business English", Level = "B1", Tuition = 3900000, Duration = "10 tuần", Description = "Tiếng Anh công việc: email, họp, thuyết trình và giao tiếp với đối tác.", ImageUrl = "/images/courses/toeic.jpg" },
                new Course { Id = 11, Code = "KH11", Name = "English for Kids Starter", Level = "Kids", Tuition = 2400000, Duration = "8 tuần", Description = "Lớp tiếng Anh trẻ em với hoạt động nghe nói, từ vựng và phản xạ ngôn ngữ tự nhiên.", ImageUrl = "/images/courses/kids.jpg" },
                new Course { Id = 12, Code = "KH12", Name = "English for Kids Movers", Level = "Kids", Tuition = 2700000, Duration = "8 tuần", Description = "Mở rộng từ vựng và cấu trúc giao tiếp cho học viên nhỏ tuổi đã có nền tảng.", ImageUrl = "/images/courses/kids.jpg" },
                new Course { Id = 13, Code = "KH13", Name = "English for Teens", Level = "A2", Tuition = 3100000, Duration = "10 tuần", Description = "Tiếng Anh thiếu niên theo chủ đề học tập, đời sống và thuyết trình ngắn.", ImageUrl = "/images/courses/kids.jpg" },
                new Course { Id = 14, Code = "KH14", Name = "Pronunciation Mastery", Level = "A2", Tuition = 2200000, Duration = "6 tuần", Description = "Sửa âm, trọng âm, nối âm và ngữ điệu để nói tiếng Anh rõ ràng hơn.", ImageUrl = "/images/courses/communication.jpg" },
                new Course { Id = 15, Code = "KH15", Name = "Grammar Foundation", Level = "A1", Tuition = 1800000, Duration = "6 tuần", Description = "Hệ thống ngữ pháp nền tảng cho người mất gốc hoặc cần ôn lại từ đầu.", ImageUrl = "/images/courses/general.jpg" },
                new Course { Id = 16, Code = "KH16", Name = "Academic Writing", Level = "B2", Tuition = 4300000, Duration = "10 tuần", Description = "Rèn cấu trúc bài viết học thuật, lập luận, liên kết ý và sửa lỗi diễn đạt.", ImageUrl = "/images/courses/general.jpg" },
                new Course { Id = 17, Code = "KH17", Name = "Speaking Club", Level = "A2", Tuition = 1600000, Duration = "4 tuần", Description = "Thực hành nói theo chủ đề với giáo viên, tăng phản xạ và sự tự tin.", ImageUrl = "/images/courses/communication.jpg" },
                new Course { Id = 18, Code = "KH18", Name = "Listening Booster", Level = "B1", Tuition = 2600000, Duration = "6 tuần", Description = "Luyện nghe ý chính, chi tiết và ghi chú nhanh qua nhiều giọng nói.", ImageUrl = "/images/courses/general.jpg" },
                new Course { Id = 19, Code = "KH19", Name = "Reading Comprehension", Level = "B1", Tuition = 2600000, Duration = "6 tuần", Description = "Tăng tốc đọc hiểu, scanning, skimming và xử lý câu hỏi từ vựng.", ImageUrl = "/images/courses/general.jpg" },
                new Course { Id = 20, Code = "KH20", Name = "English for Travel", Level = "A2", Tuition = 2100000, Duration = "5 tuần", Description = "Tiếng Anh du lịch cho sân bay, khách sạn, nhà hàng và hỏi đường.", ImageUrl = "/images/courses/general.jpg" });

            modelBuilder.Entity<CourseClass>().HasData(
                new CourseClass { Id = 1, Code = "LH01", CourseId = 1, TeacherId = 1, Room = "P101", StudyTime = "Thứ 2-4-6, 18:00-19:30", StartDate = new DateTime(2026, 7, 16), Capacity = 24 },
                new CourseClass { Id = 2, Code = "LH02", CourseId = 3, TeacherId = 3, Room = "P203", StudyTime = "Thứ 3-5, 19:00-21:00", StartDate = new DateTime(2026, 7, 19), Capacity = 20 },
                new CourseClass { Id = 3, Code = "LH03", CourseId = 2, TeacherId = 2, Room = "P102", StudyTime = "Thứ 7-CN, 08:00-10:00", StartDate = new DateTime(2026, 7, 23), Capacity = 22 },
                new CourseClass { Id = 4, Code = "LH04", CourseId = 4, TeacherId = 4, Room = "P204", StudyTime = "Thứ 2-4, 18:00-19:30", StartDate = new DateTime(2026, 7, 24), Capacity = 24 },
                new CourseClass { Id = 5, Code = "LH05", CourseId = 5, TeacherId = 5, Room = "P205", StudyTime = "Thứ 3-5, 19:00-20:30", StartDate = new DateTime(2026, 7, 25), Capacity = 20 },
                new CourseClass { Id = 6, Code = "LH06", CourseId = 6, TeacherId = 6, Room = "Online Zoom 01", StudyTime = "Thứ 2-CN, 18:00-19:30, 20:00-21:30", StartDate = new DateTime(2026, 7, 26), Capacity = 30 },
                new CourseClass { Id = 7, Code = "LH07", CourseId = 7, TeacherId = 7, Room = "P301", StudyTime = "Thứ 7-CN, 09:00-10:30, 14:00-15:30", StartDate = new DateTime(2026, 7, 27), Capacity = 18 },
                new CourseClass { Id = 8, Code = "LH08", CourseId = 8, TeacherId = 8, Room = "P103", StudyTime = "Thứ 2-4-6, 18:00-19:30", StartDate = new DateTime(2026, 7, 28), Capacity = 28 },
                new CourseClass { Id = 9, Code = "LH09", CourseId = 9, TeacherId = 9, Room = "Online Zoom 02", StudyTime = "Thứ 3-5, 20:00-21:30", StartDate = new DateTime(2026, 7, 29), Capacity = 30 },
                new CourseClass { Id = 10, Code = "LH10", CourseId = 10, TeacherId = 10, Room = "P401", StudyTime = "Thứ 2-4, 19:30-21:00", StartDate = new DateTime(2026, 8, 1), Capacity = 22 },
                new CourseClass { Id = 11, Code = "LH11", CourseId = 11, TeacherId = 11, Room = "PKids 01", StudyTime = "Thứ 7-CN, 08:00-09:30", StartDate = new DateTime(2026, 8, 2), Capacity = 16 },
                new CourseClass { Id = 12, Code = "LH12", CourseId = 12, TeacherId = 12, Room = "PKids 02", StudyTime = "Thứ 7-CN, 09:45-11:15", StartDate = new DateTime(2026, 8, 2), Capacity = 16 },
                new CourseClass { Id = 13, Code = "LH13", CourseId = 13, TeacherId = 13, Room = "P201", StudyTime = "Thứ 3-5, 17:30-19:00", StartDate = new DateTime(2026, 8, 3), Capacity = 24 },
                new CourseClass { Id = 14, Code = "LH14", CourseId = 14, TeacherId = 14, Room = "P202", StudyTime = "Thứ 2-4, 18:00-19:30", StartDate = new DateTime(2026, 8, 4), Capacity = 20 },
                new CourseClass { Id = 15, Code = "LH15", CourseId = 15, TeacherId = 15, Room = "Online Zoom 03", StudyTime = "Thứ 2-4-6, 20:00-21:00", StartDate = new DateTime(2026, 8, 5), Capacity = 32 });

            var seededUsers = new List<UserAccount>
            {
                new UserAccount { Id = 1, FullName = "Administrator", UserName = "admin", Password = "admin123", Role = "Admin", LinkedId = 0, Email = "admin@englishcenter.vn", Phone = string.Empty },
                new UserAccount { Id = 22, FullName = "Nhân viên đào tạo", UserName = "nvdt", Password = "123456", Role = "Staff", LinkedId = 0, Email = "daotao@englishcenter.vn", Phone = "0909000001" }
            };

            seededUsers.AddRange(EnglishCenterStore.TeacherProfiles.Select(teacher => new UserAccount
            {
                Id = teacher.Id + 1,
                FullName = teacher.FullName,
                UserName = teacher.Code.ToLowerInvariant(),
                Password = "123456",
                Role = "Teacher",
                LinkedId = teacher.Id,
                Email = teacher.Email,
                Phone = teacher.Phone
            }));

            seededUsers.AddRange(seededStudents.Select(student => new UserAccount
            {
                Id = student.Id + 22,
                FullName = student.FullName,
                UserName = student.Code.ToLowerInvariant(),
                Password = "123456",
                Role = "Student",
                LinkedId = student.Id,
                Email = student.Email,
                Phone = student.Phone
            }));

            modelBuilder.Entity<UserAccount>().HasData(seededUsers);

            modelBuilder.Entity<Enrollment>().HasData(
                new Enrollment { Id = 1, StudentId = 1, CourseId = 1, ClassId = 1, Status = "DaDuyet", RegisteredAt = new DateTime(2026, 7, 1) },
                new Enrollment { Id = 2, StudentId = 2, CourseId = 3, ClassId = 2, Status = "DaDuyet", RegisteredAt = new DateTime(2026, 7, 5) },
                new Enrollment { Id = 3, StudentId = 3, CourseId = 2, ClassId = null, Status = "ChoDuyet", RegisteredAt = new DateTime(2026, 7, 8) });

            modelBuilder.Entity<Payment>().HasData(
                new Payment { Id = 1, StudentId = 1, EnrollmentId = 1, Amount = 1500000, PaidAmount = 1500000, Status = "DaDong", PaymentMethod = "Cash", PaidDate = new DateTime(2026, 7, 2) },
                new Payment { Id = 2, StudentId = 2, EnrollmentId = 2, Amount = 3500000, PaidAmount = 1500000, Status = "DongMotPhan", PaymentMethod = "Transfer", PaidDate = new DateTime(2026, 7, 7) },
                new Payment { Id = 3, StudentId = 3, EnrollmentId = 3, Amount = 2000000, PaidAmount = 0, Status = "ChuaDong", PaymentMethod = "Cash", PaidDate = null });

            modelBuilder.Entity<Score>().HasData(
                new Score { Id = 1, StudentId = 1, ClassId = 1, Midterm = 7.5, Final = 8.0, Comment = "Tiếp thu tốt, cần nói tự tin hơn." },
                new Score { Id = 2, StudentId = 2, ClassId = 2, Midterm = 6.5, Final = 7.0, Comment = "Tiến bộ ổn định." });

            modelBuilder.Entity<AttendanceRecord>().HasData(
                new AttendanceRecord { Id = 1, StudentId = 1, ClassId = 1, StudyDate = new DateTime(2026, 7, 7), IsPresent = true, Note = string.Empty },
                new AttendanceRecord { Id = 2, StudentId = 2, ClassId = 2, StudyDate = new DateTime(2026, 7, 8), IsPresent = true, Note = string.Empty });
        }
    }
}



