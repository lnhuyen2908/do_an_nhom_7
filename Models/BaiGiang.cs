using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models
{
    public class CourseLecture
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int TeacherId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài giảng.")]
        public string Title { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        public Course? Course { get; set; }
        public Teacher? Teacher { get; set; }
    }
}
