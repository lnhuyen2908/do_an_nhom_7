using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public class Teacher
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    [RegularExpression(@"^TC\d{2,}$", ErrorMessage = "Mã giáo viên phải có dạng TC01.")]
    [Display(Name = "Mã giáo viên")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(150)]
    [Display(Name = "Chuyên môn")]
    public string Specialty { get; set; } = string.Empty;

    public UserAccount? UserAccount { get; set; }
    public ICollection<CourseClass> CourseClasses { get; set; } = new List<CourseClass>();
    public ICollection<CourseLecture> CourseLectures { get; set; } = new List<CourseLecture>();
}
