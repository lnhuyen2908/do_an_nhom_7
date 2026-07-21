using web_do_an1.Models;

namespace web_do_an1.Services
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

        public static readonly (int Id, string Code, string FullName, string Email, string Phone, string Specialty)[] TeacherProfiles =
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
}
