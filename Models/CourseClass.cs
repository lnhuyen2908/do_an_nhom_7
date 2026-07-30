using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public class CourseClass
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    [RegularExpression(@"^CL\d{2,}$", ErrorMessage = "Mã lớp phải có dạng CL01.")]
    [Display(Name = "Mã lớp")]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Khóa học")]
    public int CourseId { get; set; }

    [Display(Name = "Giáo viên")]
    public int TeacherId { get; set; }

    [Required, StringLength(80)]
    [Display(Name = "Phòng học")]
    public string Room { get; set; } = string.Empty;

    [Required, StringLength(150)]
    [Display(Name = "Lịch học")]
    public string Schedule { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Ngày bắt đầu")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Range(1, 500)]
    [Display(Name = "Sĩ số tối đa")]
    public int Capacity { get; set; } = 20;

    [Display(Name = "Khóa học")]
    public Course Course { get; set; } = null!;
    [Display(Name = "Giáo viên")]
    public Teacher Teacher { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Score> Scores { get; set; } = new List<Score>();
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
