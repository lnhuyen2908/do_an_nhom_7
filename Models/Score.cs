using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_do_an1.Models;

public class Score
{
    public int Id { get; set; }

    [Display(Name = "Học viên")]
    public int StudentId { get; set; }

    [Display(Name = "Lớp học")]
    public int CourseClassId { get; set; }

    [Range(0, 10)]
    [Display(Name = "Điểm giữa kỳ")]
    public double MidtermScore { get; set; }

    [Range(0, 10)]
    [Display(Name = "Điểm cuối kỳ")]
    public double FinalScore { get; set; }

    [StringLength(500)]
    [Display(Name = "Nhận xét")]
    public string Comment { get; set; } = string.Empty;

    [NotMapped]
    [Display(Name = "Điểm trung bình")]
    public double AverageScore => Math.Round((MidtermScore + FinalScore) / 2, 2);

    [NotMapped]
    [Display(Name = "Kết quả")]
    public string Result => AverageScore >= 5 ? "Đạt" : "Chưa đạt";

    [Display(Name = "Học viên")]
    public Student Student { get; set; } = null!;
    [Display(Name = "Lớp học")]
    public CourseClass CourseClass { get; set; } = null!;
}
