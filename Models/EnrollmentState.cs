using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

// Enum = kiểu liệt kê. EnrollmentState giới hạn trạng thái đăng ký chỉ có ba giá trị hợp lệ.
public enum EnrollmentState
{
    [Display(Name = "Chờ duyệt")]
    Pending, // Phiếu vừa được học viên gửi và đang chờ nhân viên xử lý.

    [Display(Name = "Đã duyệt")]
    Approved, // Nhân viên đã chấp nhận và xếp học viên vào lớp.

    [Display(Name = "Đã hủy")]
    Cancelled // Đăng ký không còn hiệu lực; học viên có thể đăng ký lại khóa học.
}
