using Microsoft.EntityFrameworkCore;
using web_do_an1.Models;

namespace web_do_an1.Data;

public class DatabaseSeeder
{
    private const string DefaultPassword = "123456";
    private readonly EnglishCenterDbContext _context;

    public DatabaseSeeder(EnglishCenterDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedPeopleAsync();
        await SeedCoursesAsync();
        await SeedClassesAsync();
        await SeedAccountsAsync();
        await SeedLearningDataAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roleData = new[]
        {
            ("Admin", "Quản trị viên"),
            ("Staff", "Nhân viên đào tạo"),
            ("Teacher", "Giáo viên"),
            ("Student", "Học viên")
        };

        foreach (var (name, displayName) in roleData)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Name == name);
            if (role is null)
            {
                _context.Roles.Add(new Role { Name = name, DisplayName = displayName });
            }
            else
            {
                role.DisplayName = displayName;
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedPeopleAsync()
    {
        var teachers = new[]
        {
            ("TC01", "Nguyễn Minh Anh", "IELTS"),
            ("TC02", "Trần Hoàng Nam", "Giao tiếp"),
            ("TC03", "Lê Thùy Dương", "TOEIC"),
            ("TC04", "Phạm Quốc Bảo", "Ngữ pháp"),
            ("TC05", "Võ Thanh Hằng", "Tiếng Anh thiếu nhi"),
            ("TC06", "Đỗ Hải Đăng", "Phát âm"),
            ("TC07", "Bùi Ngọc Mai", "IELTS Writing"),
            ("TC08", "Hoàng Tuấn Kiệt", "Business English"),
            ("TC09", "Đặng Khánh Linh", "TOEIC"),
            ("TC10", "Ngô Đức Anh", "Tiếng Anh học thuật")
        };

        for (var index = 0; index < teachers.Length; index++)
        {
            var (code, fullName, specialty) = teachers[index];
            var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.Code == code);
            teacher ??= new Teacher { Code = code };
            teacher.FullName = fullName;
            teacher.Email = $"gv{index + 1:00}@englishcenter.vn";
            teacher.Phone = $"0901{index + 1:000000}";
            teacher.Specialty = specialty;

            if (teacher.Id == 0)
            {
                _context.Teachers.Add(teacher);
            }
        }

        var studentNames = new[]
        {
            "Lê Thu Hà", "Phạm Gia Huy", "Nguyễn Hoàng Anh", "Trần Minh Châu",
            "Võ Đức Long", "Đặng Khánh Vy", "Bùi Anh Khoa", "Đỗ Ngọc Trâm",
            "Hoàng Minh Quân", "Ngô Thanh Tâm", "Phan Tuấn Anh", "Lý Hải Yến",
            "Trương Gia Bảo", "Mai Thảo Nhi", "Đinh Quốc Việt", "Vũ Hà My",
            "Nguyễn Đức Minh", "Trần Ngọc Ánh", "Lê Thành Đạt", "Phạm Kim Ngân"
        };

        for (var index = 0; index < studentNames.Length; index++)
        {
            var code = $"ST{index + 1:00}";
            var student = await _context.Students.FirstOrDefaultAsync(x => x.Code == code);
            student ??= new Student { Code = code };
            student.FullName = studentNames[index];
            student.Email = $"st{index + 1:00}@englishcenter.vn";
            student.Phone = $"0912{index + 1:000000}";
            student.DateOfBirth = new DateTime(2002 + index % 5, index % 12 + 1, index % 24 + 1);
            student.Address = index % 2 == 0 ? "Thành phố Hồ Chí Minh" : "Bình Dương";

            if (student.Id == 0)
            {
                _context.Students.Add(student);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedCoursesAsync()
    {
        var courses = new[]
        {
            new Course { Code = "CR01", Name = "English Communication", Level = "A2", Tuition = 2_500_000, Duration = "10 tuần", Description = "Rèn luyện phản xạ nghe nói trong các tình huống giao tiếp hằng ngày.", ImageUrl = "/images/courses/communication.jpg" },
            new Course { Code = "CR02", Name = "IELTS Foundation", Level = "B1", Tuition = 3_800_000, Duration = "12 tuần", Description = "Xây dựng nền tảng cho bốn kỹ năng IELTS.", ImageUrl = "/images/courses/ielts.jpg" },
            new Course { Code = "CR03", Name = "TOEIC 650+", Level = "Intermediate", Tuition = 3_200_000, Duration = "10 tuần", Description = "Củng cố từ vựng, ngữ pháp và chiến thuật làm bài TOEIC.", ImageUrl = "/images/courses/toeic.jpg" },
            new Course { Code = "CR04", Name = "English for Kids", Level = "Kids", Tuition = 2_800_000, Duration = "8 tuần", Description = "Khóa học sinh động giúp trẻ xây dựng nền tảng tiếng Anh tự nhiên.", ImageUrl = "/images/courses/kids.jpg" },
            new Course { Code = "CR05", Name = "General English", Level = "A1", Tuition = 2_200_000, Duration = "8 tuần", Description = "Củng cố phát âm, từ vựng và ngữ pháp cho người mới bắt đầu.", ImageUrl = "/images/courses/general.jpg" },
            new Course { Code = "CR06", Name = "IELTS 6.5 Intensive", Level = "B2", Tuition = 5_200_000, Duration = "14 tuần", Description = "Lộ trình tăng tốc bốn kỹ năng hướng đến IELTS 6.5.", ImageUrl = "/images/courses/ielts.jpg" },
            new Course { Code = "CR07", Name = "Business English", Level = "B1", Tuition = 3_900_000, Duration = "10 tuần", Description = "Tiếng Anh công sở, email, họp và thuyết trình chuyên nghiệp.", ImageUrl = "/images/courses/communication.jpg" },
            new Course { Code = "CR08", Name = "Pronunciation Mastery", Level = "A2", Tuition = 2_400_000, Duration = "6 tuần", Description = "Sửa âm, trọng âm và ngữ điệu để giao tiếp rõ ràng hơn.", ImageUrl = "/images/courses/general.jpg" }
        };

        foreach (var source in courses)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(x => x.Code == source.Code);
            if (course is null)
            {
                _context.Courses.Add(source);
                continue;
            }

            course.Name = source.Name;
            course.Level = source.Level;
            course.Tuition = source.Tuition;
            course.Duration = source.Duration;
            course.Description = source.Description;
            course.ImageUrl = source.ImageUrl;
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedClassesAsync()
    {
        var courses = await _context.Courses.OrderBy(x => x.Code).Take(8).ToListAsync();
        var teachers = await _context.Teachers.OrderBy(x => x.Code).Take(10).ToListAsync();
        if (courses.Count < 8 || teachers.Count < 8)
        {
            return;
        }

        for (var index = 0; index < 8; index++)
        {
            var code = $"CL{index + 1:00}";
            var courseClass = await _context.CourseClasses.FirstOrDefaultAsync(x => x.Code == code);
            courseClass ??= new CourseClass { Code = code };
            courseClass.CourseId = courses[index].Id;
            courseClass.TeacherId = teachers[index].Id;
            courseClass.Room = index % 3 == 2 ? $"Online {index + 1:00}" : $"P{index + 1:00}1";
            courseClass.Schedule = index % 2 == 0
                ? "Thứ 2-4-6, 18:00-19:30"
                : "Thứ 3-5, 19:00-20:30";
            courseClass.StartDate = DateTime.Today.AddDays(7 + index * 3);
            courseClass.Capacity = 20 + index;

            if (courseClass.Id == 0)
            {
                _context.CourseClasses.Add(courseClass);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedAccountsAsync()
    {
        var roles = await _context.Roles.ToDictionaryAsync(x => x.Name);

        await UpsertAccountAsync(
            "admin",
            "Administrator",
            "admin@englishcenter.vn",
            "0909000001",
            roles["Admin"].Id);

        await UpsertAccountAsync(
            "nvdt",
            "Nhân viên đào tạo",
            "nvdt@englishcenter.vn",
            "0909000002",
            roles["Staff"].Id);

        var teachers = await _context.Teachers.OrderBy(x => x.Code).Take(10).ToListAsync();
        for (var index = 0; index < teachers.Count; index++)
        {
            var teacher = teachers[index];
            await UpsertAccountAsync(
                $"gv{index + 1:00}",
                teacher.FullName,
                teacher.Email,
                teacher.Phone,
                roles["Teacher"].Id,
                teacherId: teacher.Id);
        }

        var students = await _context.Students.OrderBy(x => x.Code).Take(20).ToListAsync();
        for (var index = 0; index < students.Count; index++)
        {
            var student = students[index];
            await UpsertAccountAsync(
                $"st{index + 1:00}",
                student.FullName,
                student.Email,
                student.Phone,
                roles["Student"].Id,
                studentId: student.Id);
        }

        await _context.SaveChangesAsync();
    }

    private async Task UpsertAccountAsync(
        string userName,
        string fullName,
        string email,
        string phone,
        int roleId,
        int? studentId = null,
        int? teacherId = null)
    {
        var account = studentId.HasValue
            ? await _context.UserAccounts.FirstOrDefaultAsync(x => x.StudentId == studentId)
            : teacherId.HasValue
                ? await _context.UserAccounts.FirstOrDefaultAsync(x => x.TeacherId == teacherId)
                : await _context.UserAccounts.FirstOrDefaultAsync(x => x.UserName == userName);

        account ??= new UserAccount();
        account.FullName = fullName;
        account.UserName = userName;
        account.Password = DefaultPassword;
        account.Email = email;
        account.Phone = phone;
        account.RoleId = roleId;
        account.StudentId = studentId;
        account.TeacherId = teacherId;
        account.IsActive = true;

        if (account.Id == 0)
        {
            account.CreatedAt = DateTime.Now;
            _context.UserAccounts.Add(account);
        }
    }

    private async Task SeedLearningDataAsync()
    {
        var students = await _context.Students.OrderBy(x => x.Code).Take(12).ToListAsync();
        var classes = await _context.CourseClasses
            .Include(x => x.Course).Include(x => x.Teacher)
            .OrderBy(x => x.Code).Take(8).ToListAsync();
        if (students.Count < 12 || classes.Count < 4)
        {
            return;
        }

        for (var index = 0; index < students.Count; index++)
        {
            var student = students[index];
            var courseClass = classes[index % 4];
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(x =>
                x.StudentId == student.Id && x.CourseId == courseClass.CourseId);
            if (enrollment is null)
            {
                enrollment = new Enrollment
                {
                    StudentId = student.Id,
                    CourseId = courseClass.CourseId,
                    CourseClassId = index < 10 ? courseClass.Id : null,
                    Status = index < 10 ? EnrollmentState.Approved : EnrollmentState.Pending,
                    RegisteredAt = DateTime.Now.AddDays(-(index + 2))
                };
                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
            }

            if (!await _context.Payments.AnyAsync(x => x.EnrollmentId == enrollment.Id))
            {
                var paidAmount = (index % 3) switch
                {
                    0 => courseClass.Course.Tuition,
                    1 => courseClass.Course.Tuition / 2,
                    _ => 0
                };
                _context.Payments.Add(new Payment
                {
                    StudentId = student.Id,
                    EnrollmentId = enrollment.Id,
                    Amount = courseClass.Course.Tuition,
                    PaidAmount = paidAmount,
                    Status = paidAmount <= 0
                        ? PaymentState.Unpaid
                        : paidAmount >= courseClass.Course.Tuition
                            ? PaymentState.Paid
                            : PaymentState.PartiallyPaid,
                    PaymentMethod = index % 2 == 0
                        ? PaymentMethod.BankTransfer
                        : PaymentMethod.Cash,
                    PaidDate = paidAmount > 0 ? DateTime.Today.AddDays(-index) : null
                });
            }

            if (enrollment.Status == EnrollmentState.Approved
                && enrollment.CourseClassId.HasValue
                && index < 8
                && !await _context.Scores.AnyAsync(x =>
                    x.StudentId == student.Id
                    && x.CourseClassId == enrollment.CourseClassId.Value))
            {
                _context.Scores.Add(new Score
                {
                    StudentId = student.Id,
                    CourseClassId = enrollment.CourseClassId.Value,
                    MidtermScore = 6.5 + index % 4 * 0.5,
                    FinalScore = 7 + index % 3 * 0.5,
                    Comment = index % 2 == 0
                        ? "Tiến bộ tốt, cần duy trì luyện tập."
                        : "Chủ động hơn trong phần luyện nói."
                });
            }

            if (enrollment.Status == EnrollmentState.Approved
                && enrollment.CourseClassId.HasValue
                && !await _context.AttendanceRecords.AnyAsync(x =>
                    x.StudentId == student.Id
                    && x.CourseClassId == enrollment.CourseClassId.Value
                    && x.StudyDate == DateTime.Today.AddDays(-1)))
            {
                _context.AttendanceRecords.Add(new AttendanceRecord
                {
                    StudentId = student.Id,
                    CourseClassId = enrollment.CourseClassId.Value,
                    StudyDate = DateTime.Today.AddDays(-1),
                    IsPresent = index % 5 != 0,
                    Note = index % 5 == 0 ? "Học viên xin phép nghỉ." : string.Empty
                });
            }
        }

        await _context.SaveChangesAsync();

        foreach (var courseClass in classes.Take(4))
        {
            if (await _context.CourseLectures.AnyAsync(x =>
                x.CourseId == courseClass.CourseId && x.TeacherId == courseClass.TeacherId))
            {
                continue;
            }

            _context.CourseLectures.Add(new CourseLecture
            {
                CourseId = courseClass.CourseId,
                TeacherId = courseClass.TeacherId,
                Title = $"Tài liệu nhập môn {courseClass.Course.Name}",
                FileName = "tai-lieu-nhap-mon.pdf",
                FileUrl = "https://example.com/tai-lieu-hoc-tap",
                UploadedAt = DateTime.Now.AddDays(-3)
            });
        }

        await _context.SaveChangesAsync();
    }
}
