using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_do_an1.Models;

public class UserAccount
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(50, MinimumLength = 3)]
    [Display(Name = "Tên đăng nhập")]
    public string UserName { get; set; } = string.Empty;

    [Display(Name = "Mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(20)]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Vai trò")]
    public int RoleId { get; set; }

    [Display(Name = "Học viên liên kết")]
    public int? StudentId { get; set; }

    [Display(Name = "Giáo viên liên kết")]
    public int? TeacherId { get; set; }

    [Display(Name = "Đang hoạt động")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Ngày tạo")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Role Role { get; set; } = null!;
    public Student? Student { get; set; }
    public Teacher? Teacher { get; set; }
}
