using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public class CourseLecture
{
    public int Id { get; set; }

    [Display(Name = "Khóa học")]
    public int CourseId { get; set; }

    [Display(Name = "Giáo viên")]
    public int TeacherId { get; set; }

    [Required, StringLength(200)]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [StringLength(255)]
    [Display(Name = "Tên tệp")]
    public string FileName { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Đường dẫn tệp")]
    public string FileUrl { get; set; } = string.Empty;

    [StringLength(500)]
    [Url]
    [Display(Name = "Link YouTube")]
    public string YouTubeUrl { get; set; } = string.Empty;

    [Display(Name = "Ngày tải lên")]
    public DateTime UploadedAt { get; set; } = DateTime.Now;

    [Display(Name = "Khóa học")]
    public Course Course { get; set; } = null!;
    [Display(Name = "Giáo viên")]
    public Teacher Teacher { get; set; } = null!;
}
