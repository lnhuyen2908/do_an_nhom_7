using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace web_do_an1.Models
{
    public class UserAccount
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [MinLength(3, ErrorMessage = "Tên đăng nhập phải có ít nhất 3 ký tự.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        [ForeignKey(nameof(RoleItem))]
        public string Role { get; set; } = "Student";

        public int LinkedId { get; set; }

        [EmailAddress(ErrorMessage = "Email chưa đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [RegularExpression(@"^$|^\d{9,11}$", ErrorMessage = "Số điện thoại phải gồm 9 đến 11 chữ số.")]
        public string Phone { get; set; } = string.Empty;

        public RoleItem? RoleItem { get; set; }
    }
}
