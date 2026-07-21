namespace web_do_an1.Models
{
    public static class EnglishCenterStore
    {
        public const int DefaultPageSize = 12;

        public const string EnrollmentPending = "ChoDuyet";
        public const string EnrollmentApproved = "DaDuyet";
        public const string EnrollmentCanceled = "DaHuy";

        public const string PaymentUnpaid = "ChuaDong";
        public const string PaymentPartial = "DongMotPhan";
        public const string PaymentPaid = "DaDong";

        public const string PaymentMethodCash = "Cash";
        public const string PaymentMethodTransfer = "Transfer";

        public const string RoleAdmin = "Admin";
        public const string RoleStaff = "Staff";
        public const string RoleTeacher = "Teacher";
        public const string RoleStudent = "Student";

        public sealed record TeacherProfile(int Id, string Code, string FullName, string Email, string Phone, string Specialty);

        public static readonly TeacherProfile[] TeacherProfiles =
        {
            new(1, "GV01", "Nguyễn Minh Anh", "minhanh@englishcenter.vn", "0901000001", "IELTS"),
            new(2, "GV02", "Trần Quốc Bảo", "quocbao@englishcenter.vn", "0901000002", "Giao tiếp"),
            new(3, "GV03", "Lê Thu Hà", "thuha@englishcenter.vn", "0901000003", "Ngữ pháp"),
            new(4, "GV04", "Phạm Đức Long", "duclong@englishcenter.vn", "0901000004", "TOEIC"),
            new(5, "GV05", "Hoàng Mai Phương", "maiphuong@englishcenter.vn", "0901000005", "IELTS Writing"),
            new(6, "GV06", "Võ Thanh Sơn", "thanhson@englishcenter.vn", "0901000006", "IELTS Speaking"),
            new(7, "GV07", "Đặng Ngọc Lan", "ngoclan@englishcenter.vn", "0901000007", "IELTS Advanced"),
            new(8, "GV08", "Bùi Quang Hưng", "quanghung@englishcenter.vn", "0901000008", "TOEIC Listening"),
            new(9, "GV09", "Đỗ Khánh Vy", "khanhvy@englishcenter.vn", "0901000009", "TOEIC Reading"),
            new(10, "GV10", "Hồ Anh Tuấn", "anhtuan@englishcenter.vn", "0901000010", "Business English"),
            new(11, "GV11", "Nguyễn Bảo Trâm", "baotram@englishcenter.vn", "0901000011", "Kids Starter"),
            new(12, "GV12", "Trần Gia Hân", "giahan@englishcenter.vn", "0901000012", "Kids Movers"),
            new(13, "GV13", "Lê Nhật Minh", "nhatminh@englishcenter.vn", "0901000013", "Teen English"),
            new(14, "GV14", "Phạm Hồng Nhung", "hongnhung@englishcenter.vn", "0901000014", "Pronunciation"),
            new(15, "GV15", "Hoàng Minh Quân", "minhquan@englishcenter.vn", "0901000015", "Grammar Foundation"),
            new(16, "GV16", "Võ Thùy Linh", "thuylinh@englishcenter.vn", "0901000016", "Academic Writing"),
            new(17, "GV17", "Đặng Hải Nam", "hainam@englishcenter.vn", "0901000017", "Speaking Club"),
            new(18, "GV18", "Bùi Phương Anh", "phuonganh@englishcenter.vn", "0901000018", "Listening"),
            new(19, "GV19", "Đỗ Việt Hoàng", "viethoang@englishcenter.vn", "0901000019", "Reading"),
            new(20, "GV20", "Hồ Ngọc Diệp", "ngocdiep@englishcenter.vn", "0901000020", "English for Travel")
        };

        public static readonly string[] StudentNames =
        {
            "Nguyễn Hoàng Anh",
            "Trần Minh Châu",
            "Lê Gia Bảo",
            "Phạm Khánh Linh",
            "Hoàng Đức Anh",
            "Võ Ngọc Mai",
            "Đặng Quang Huy",
            "Bùi Thu Trang",
            "Đỗ Minh Khang",
            "Hồ Phương Thảo",
            "Nguyễn Tuấn Kiệt",
            "Trần Hải Yến",
            "Lê Nhật Nam",
            "Phạm Bảo Ngọc",
            "Hoàng Anh Thư",
            "Võ Quốc Khánh",
            "Đặng Thanh Hà",
            "Bùi Gia Hân",
            "Đỗ Thành Đạt",
            "Hồ Minh Thư",
            "Nguyễn Đức Minh",
            "Trần Ngọc Ánh",
            "Lê Hoài Phương",
            "Phạm Công Thành",
            "Hoàng Thùy Dương",
            "Võ Minh Triết",
            "Đặng Mai Anh",
            "Bùi Quốc Việt",
            "Đỗ Khánh Vy",
            "Hồ Gia Khiêm",
            "Nguyễn Phương Nhi",
            "Trần Đình Phúc",
            "Lê Ngọc Hân",
            "Phạm Trung Kiên",
            "Hoàng Bích Ngọc",
            "Võ Anh Khoa",
            "Đặng Thu Uyên",
            "Bùi Minh Quân",
            "Đỗ Hải Anh",
            "Hồ Thanh Trúc",
            "Nguyễn Quỳnh Như",
            "Trần Gia Huy",
            "Lê Mỹ Linh",
            "Phạm Quốc Bảo",
            "Hoàng Ngọc Diệp"
        };

        public static readonly string[] CourseImageOptions =
        {
            "/images/courses/general.jpg",
            "/images/courses/communication.jpg",
            "/images/courses/ielts.jpg",
            "/images/courses/toeic.jpg",
            "/images/courses/kids.jpg"
        };

        public static string CourseImage(Course course)
        {
            if (!string.IsNullOrWhiteSpace(course.ImageUrl))
            {
                return course.ImageUrl;
            }

            var text = $"{course.Name} {course.Level}".ToLowerInvariant();
            if (text.Contains("ielts")) return "/images/courses/ielts.jpg";
            if (text.Contains("toeic")) return "/images/courses/toeic.jpg";
            if (text.Contains("kids") || text.Contains("teens")) return "/images/courses/kids.jpg";
            if (text.Contains("giao") || text.Contains("speaking") || text.Contains("pronunciation")) return "/images/courses/communication.jpg";
            return "/images/courses/general.jpg";
        }

        public static string StatusText(string status)
        {
            return status switch
            {
                EnrollmentPending => "Chờ duyệt",
                EnrollmentApproved => "Đã duyệt",
                EnrollmentCanceled => "Đã hủy",
                PaymentUnpaid => "Chưa đóng",
                PaymentPartial => "Đóng một phần",
                PaymentPaid => "Đã đóng",
                _ => status
            };
        }

        public static string RoleText(string role)
        {
            return role switch
            {
                "Admin" => "Quản trị viên",
                "Staff" => "Nhân viên đào tạo",
                "Teacher" => "Giáo viên",
                "Student" => "Học viên",
                _ => role
            };
        }

        public static string PaymentMethodText(string method)
        {
            return method switch
            {
                PaymentMethodTransfer => "Chuyển khoản",
                PaymentMethodCash => "Tiền mặt",
                _ => "Chưa chọn"
            };
        }

        public static bool IsValidPaymentMethod(string method)
        {
            return method == PaymentMethodCash || method == PaymentMethodTransfer;
        }

        public static string PaymentStatus(decimal paidAmount, decimal amount)
        {
            if (paidAmount <= 0)
            {
                return PaymentUnpaid;
            }

            return paidAmount < amount ? PaymentPartial : PaymentPaid;
        }

        public static int TotalPages(int totalItems, int pageSize = DefaultPageSize)
        {
            return Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        }

        public static int NormalizePage(int page, int totalItems, int pageSize = DefaultPageSize)
        {
            return Math.Clamp(page, 1, TotalPages(totalItems, pageSize));
        }
    }

    public static class PasswordUtility
    {
        public static string Hash(UserAccount user, string password)
        {
            return password;
        }

        public static bool Verify(UserAccount user, string password, out bool shouldUpgrade)
        {
            shouldUpgrade = false;
            return user.Password == password;
        }

        public static int UsePlainTextDemoPasswords(web_do_an1.Data.EnglishCenterDbContext db)
        {
            var changed = 0;

            foreach (var user in db.Users)
            {
                var password = PlainTextDemoPassword(user);
                if (!string.IsNullOrEmpty(password) && user.Password != password)
                {
                    user.Password = password;
                    changed++;
                }
            }

            if (changed > 0)
            {
                db.SaveChanges();
            }

            return changed;
        }

        private static string? PlainTextDemoPassword(UserAccount user)
        {
            if (user.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return "admin123";
            }

            if (user.Password.StartsWith("AQAAAA")
                || user.UserName.Equals("nvdt", StringComparison.OrdinalIgnoreCase)
                || IsDemoCode(user.UserName, "gv")
                || IsDemoCode(user.UserName, "hv"))
            {
                return "123456";
            }

            return null;
        }

        private static bool IsDemoCode(string userName, string prefix)
        {
            return userName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && userName.Length > prefix.Length
                && userName[prefix.Length..].All(char.IsDigit);
        }
    }

    public static class LectureFileStorage
    {
        public const long MaxFileSize = 10 * 1024 * 1024;

        private static readonly IReadOnlyDictionary<string, string> ContentTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = "application/pdf",
                [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation"
            };

        public static bool IsAllowedExtension(string extension)
        {
            return ContentTypes.ContainsKey(extension);
        }

        public static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return ContentTypes.GetValueOrDefault(extension, "application/octet-stream");
        }

        public static string GetPrivateDirectory(string contentRootPath)
        {
            return Path.Combine(contentRootPath, "App_Data", "Lectures");
        }

        public static string GetStoredFileName(string fileReference)
        {
            return Path.GetFileName(fileReference.Replace('\\', '/'));
        }

        public static string? ResolveExistingPath(string contentRootPath, string fileReference)
        {
            var storedName = GetStoredFileName(fileReference);
            if (string.IsNullOrWhiteSpace(storedName))
            {
                return null;
            }

            var privatePath = SafePath(GetPrivateDirectory(contentRootPath), storedName);
            if (privatePath != null && File.Exists(privatePath))
            {
                return privatePath;
            }

            var legacyPath = SafePath(Path.Combine(contentRootPath, "wwwroot", "uploads", "lectures"), storedName);
            return legacyPath != null && File.Exists(legacyPath) ? legacyPath : null;
        }

        public static string CreatePrivatePath(string contentRootPath, string storedName)
        {
            var directory = GetPrivateDirectory(contentRootPath);
            Directory.CreateDirectory(directory);
            return SafePath(directory, storedName)
                ?? throw new InvalidOperationException("Tên file bài giảng không hợp lệ.");
        }

        public static void DeleteIfExists(string contentRootPath, string fileReference)
        {
            var path = ResolveExistingPath(contentRootPath, fileReference);
            if (path != null)
            {
                File.Delete(path);
            }
        }

        public static void MigratePublicFiles(string contentRootPath)
        {
            var publicDirectory = Path.Combine(contentRootPath, "wwwroot", "uploads", "lectures");
            if (!Directory.Exists(publicDirectory))
            {
                return;
            }

            var privateDirectory = GetPrivateDirectory(contentRootPath);
            Directory.CreateDirectory(privateDirectory);
            foreach (var sourcePath in Directory.EnumerateFiles(publicDirectory))
            {
                var destinationPath = SafePath(privateDirectory, Path.GetFileName(sourcePath));
                if (destinationPath == null)
                {
                    continue;
                }

                if (File.Exists(destinationPath))
                {
                    if (new FileInfo(sourcePath).Length == new FileInfo(destinationPath).Length)
                    {
                        File.Delete(sourcePath);
                    }
                    continue;
                }

                File.Move(sourcePath, destinationPath);
            }
        }

        private static string? SafePath(string directory, string fileName)
        {
            var root = Path.GetFullPath(directory);
            var candidate = Path.GetFullPath(Path.Combine(root, Path.GetFileName(fileName)));
            return candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                ? candidate
                : null;
        }
    }

    public static class SampleDataUtility
    {
        private static readonly string[] StudyTimes =
        {
            "Thứ 2-4-6, 18:00-19:30",
            "Thứ 3-5, 19:00-21:00",
            "Thứ 7-CN, 08:00-10:00",
            "Thứ 2-4, 19:30-21:00",
            "Thứ 7-CN, 14:00-15:30"
        };

        public static void NormalizeTeachersAndClasses(web_do_an1.Data.EnglishCenterDbContext db)
        {
            var teacherProfiles = EnglishCenterStore.TeacherProfiles;
            var allowedTeacherCodes = teacherProfiles.Select(x => x.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var profile in teacherProfiles)
            {
                var teacher = db.Teachers.FirstOrDefault(x => x.Code == profile.Code);
                if (teacher == null)
                {
                    teacher = new Teacher { Code = profile.Code };
                    db.Teachers.Add(teacher);
                }

                teacher.FullName = profile.FullName;
                teacher.Email = profile.Email;
                teacher.Phone = profile.Phone;
                teacher.Specialty = profile.Specialty;
            }

            db.SaveChanges();

            var teachersByCode = db.Teachers
                .Where(x => allowedTeacherCodes.Contains(x.Code))
                .ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

            var teachersByCourseId = teacherProfiles
                .Where(x => teachersByCode.ContainsKey(x.Code))
                .ToDictionary(x => x.Id, x => teachersByCode[x.Code]);

            var extraTeachers = db.Teachers
                .Where(x => !allowedTeacherCodes.Contains(x.Code))
                .ToList();
            var extraTeacherIds = extraTeachers.Select(x => x.Id).ToHashSet();

            foreach (var courseClass in db.Classes.Where(x => extraTeacherIds.Contains(x.TeacherId)).ToList())
            {
                courseClass.TeacherId = teachersByCourseId.TryGetValue(courseClass.CourseId, out var teacher)
                    ? teacher.Id
                    : teachersByCourseId[1].Id;
            }

            foreach (var lecture in db.Lectures.Where(x => extraTeacherIds.Contains(x.TeacherId)).ToList())
            {
                lecture.TeacherId = teachersByCourseId.TryGetValue(lecture.CourseId, out var teacher)
                    ? teacher.Id
                    : teachersByCourseId[1].Id;
            }

            var extraTeacherUsers = db.Users
                .Where(x => x.Role == EnglishCenterStore.RoleTeacher)
                .AsEnumerable()
                .Where(x => extraTeacherIds.Contains(x.LinkedId) || !allowedTeacherCodes.Contains(x.UserName.ToUpperInvariant()))
                .ToList();
            db.Users.RemoveRange(extraTeacherUsers);
            db.Teachers.RemoveRange(extraTeachers);
            db.SaveChanges();

            SyncTeacherAccounts(db, teacherProfiles, teachersByCode);
            SyncClasses(db, teachersByCourseId);

            db.SaveChanges();
        }

        private static void SyncTeacherAccounts(web_do_an1.Data.EnglishCenterDbContext db, IEnumerable<EnglishCenterStore.TeacherProfile> profiles, Dictionary<string, Teacher> teachersByCode)
        {
            foreach (var profile in profiles)
            {
                var teacher = teachersByCode[profile.Code];
                var userName = profile.Code.ToLowerInvariant();
                var user = db.Users.FirstOrDefault(x => x.Role == EnglishCenterStore.RoleTeacher && x.LinkedId == teacher.Id)
                    ?? db.Users.FirstOrDefault(x => x.UserName == userName);

                if (user == null)
                {
                    user = new UserAccount
                    {
                        UserName = userName,
                        Role = EnglishCenterStore.RoleTeacher,
                        LinkedId = teacher.Id
                    };
                    user.Password = "123456";
                    db.Users.Add(user);
                }

                user.FullName = teacher.FullName;
                user.UserName = userName;
                user.Role = EnglishCenterStore.RoleTeacher;
                user.LinkedId = teacher.Id;
                user.Email = teacher.Email;
                user.Phone = teacher.Phone;
            }
        }

        private static void SyncClasses(web_do_an1.Data.EnglishCenterDbContext db, Dictionary<int, Teacher> teachersByCourseId)
        {
            var courses = db.Courses.OrderBy(x => x.Id).Take(20).ToList();
            var nextClassNumber = db.Classes
                .AsEnumerable()
                .Select(x => int.TryParse(x.Code.Replace("LH", string.Empty), out var number) ? number : 0)
                .DefaultIfEmpty(0)
                .Max() + 1;

            foreach (var course in courses)
            {
                if (!teachersByCourseId.TryGetValue(course.Id, out var teacher))
                {
                    continue;
                }

                var courseClasses = db.Classes.Where(x => x.CourseId == course.Id).ToList();
                if (courseClasses.Count == 0)
                {
                    var code = $"LH{course.Id:00}";
                    if (db.Classes.Any(x => x.Code == code))
                    {
                        code = $"LH{nextClassNumber++:00}";
                    }

                    courseClasses.Add(new CourseClass
                    {
                        Code = code,
                        CourseId = course.Id,
                        Room = course.Level == "Kids" ? "PKids 01" : $"P{100 + course.Id}",
                        StudyTime = StudyTimes[(course.Id - 1) % StudyTimes.Length],
                        StartDate = new DateTime(2026, 7, 16).AddDays(course.Id - 1),
                        Capacity = course.Level == "Kids" ? 16 : 24
                    });
                    db.Classes.Add(courseClasses[0]);
                }

                foreach (var courseClass in courseClasses)
                {
                    courseClass.TeacherId = teacher.Id;
                }
            }
        }
    }
}
