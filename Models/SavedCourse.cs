using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public class SavedCourse
{
    public int Id { get; set; }

    [Display(Name = "Học viên")]
    public int StudentId { get; set; }

    [Display(Name = "Khóa học")]
    public int CourseId { get; set; }

    [Display(Name = "Ngày lưu")]
    public DateTime SavedAt { get; set; } = DateTime.Now;

    [Display(Name = "Học viên")]
    public Student Student { get; set; } = null!;
    [Display(Name = "Khóa học")]
    public Course Course { get; set; } = null!;
}
