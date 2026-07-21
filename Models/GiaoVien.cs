using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models
{
    public class Teacher
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã giáo viên.")]
        [RegularExpression(@"^GV\d{2,}$", ErrorMessage = "Mã giáo viên phải có dạng GV01.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên giáo viên.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email giáo viên.")]
        [EmailAddress(ErrorMessage = "Email giáo viên chưa đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại giáo viên.")]
        [RegularExpression(@"^\d{9,11}$", ErrorMessage = "Số điện thoại phải gồm 9 đến 11 chữ số.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập chuyên môn của giáo viên.")]
        public string Specialty { get; set; } = string.Empty;

        public List<CourseClass> Classes { get; set; } = new();
        public List<CourseLecture> Lectures { get; set; } = new();
    }
}
