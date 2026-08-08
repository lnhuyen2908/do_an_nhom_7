using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_do_an1.Models;

// UserAccount = Tài khoản người dùng, dùng để lưu thông tin đăng nhập và phân quyền.
public class UserAccount
{
    // Khóa chính của tài khoản.
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    // Tên đăng nhập bắt buộc, dài từ 3 đến 50 ký tự.
    [Required, StringLength(50, MinimumLength = 3)]
    [Display(Name = "Tên đăng nhập")]
    public string UserName { get; set; } = string.Empty;

    // DataType.Password giúp ô nhập trên giao diện che nội dung mật khẩu.
    [Display(Name = "Mật khẩu")]
    [DataType(DataType.Password)]
    // Hiện tại dự án lưu mật khẩu trực tiếp; hệ thống thực tế cần băm mật khẩu trước khi lưu.
    public string Password { get; set; } = string.Empty;

    [EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(20)]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    // RoleId là khóa ngoại trỏ đến bảng Roles để xác định quyền Admin, Staff, Teacher hoặc Student.
    [Display(Name = "Vai trò")]
    public int RoleId { get; set; }

    // StudentId là khóa ngoại có thể rỗng; chỉ tài khoản học viên mới cần liên kết tới Student.
    [Display(Name = "Học viên liên kết")]
    public int? StudentId { get; set; }

    [Display(Name = "Giáo viên liên kết")]
    public int? TeacherId { get; set; }

    [Display(Name = "Đang hoạt động")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Ngày tạo")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Các navigation property giúp Entity Framework truy cập đối tượng liên quan từ khóa ngoại phía trên.
    public Role Role { get; set; } = null!;
    public Student? Student { get; set; }
    public Teacher? Teacher { get; set; }
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
