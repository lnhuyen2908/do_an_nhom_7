using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

// Enrollment = Phiếu đăng ký khóa học, nối một học viên với một khóa học và lớp học cụ thể.
public class Enrollment
{
    // Khóa chính của phiếu đăng ký.
    public int Id { get; set; }

    // StudentId = khóa ngoại cho biết học viên nào đăng ký.
    [Display(Name = "Học viên")]
    public int StudentId { get; set; }

    // CourseId = khóa ngoại cho biết học viên đăng ký khóa học nào.
    [Display(Name = "Khóa học")]
    public int CourseId { get; set; }

    // CourseClassId = lớp được chọn; int? cho phép để trống khi đăng ký chưa được xếp lớp.
    [Display(Name = "Lớp học")]
    public int? CourseClassId { get; set; }

    // Đăng ký mới mặc định ở trạng thái Pending = Chờ duyệt.
    [Display(Name = "Trạng thái")]
    public EnrollmentState Status { get; set; } = EnrollmentState.Pending;

    // Tự ghi nhận thời điểm tạo phiếu đăng ký.
    [Display(Name = "Ngày đăng ký")]
    public DateTime RegisteredAt { get; set; } = DateTime.Now;

    // Các navigation property chứa dữ liệu chi tiết của học viên, khóa học, lớp học và học phí liên quan.
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public CourseClass? CourseClass { get; set; }
    public Payment? Payment { get; set; }
}
