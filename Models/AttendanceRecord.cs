using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public class AttendanceRecord
{
    public int Id { get; set; }

    [Display(Name = "Học viên")]
    public int StudentId { get; set; }

    [Display(Name = "Lớp học")]
    public int CourseClassId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ngày học")]
    public DateTime StudyDate { get; set; } = DateTime.Today;

    [Display(Name = "Có mặt")]
    public bool IsPresent { get; set; }

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string Note { get; set; } = string.Empty;

    [Display(Name = "Học viên")]
    public Student Student { get; set; } = null!;
    [Display(Name = "Lớp học")]
    public CourseClass CourseClass { get; set; } = null!;
}
