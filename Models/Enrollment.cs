using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public class Enrollment
{
    public int Id { get; set; }

    [Display(Name = "Học viên")]
    public int StudentId { get; set; }

    [Display(Name = "Khóa học")]
    public int CourseId { get; set; }

    [Display(Name = "Lớp học")]
    public int? CourseClassId { get; set; }

    [Display(Name = "Trạng thái")]
    public EnrollmentState Status { get; set; } = EnrollmentState.Pending;

    [Display(Name = "Ngày đăng ký")]
    public DateTime RegisteredAt { get; set; } = DateTime.Now;

    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public CourseClass? CourseClass { get; set; }
    public Payment? Payment { get; set; }
}
