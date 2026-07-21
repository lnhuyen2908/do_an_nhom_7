using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models
{
    public class CourseClass
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã lớp học.")]
        [RegularExpression(@"^LH\d{2,}$", ErrorMessage = "Mã lớp học phải có dạng LH01.")]
        public string Code { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn khóa học.")]
        public int CourseId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn giáo viên.")]
        public int TeacherId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập phòng học.")]
        public string Room { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lịch học.")]
        public string StudyTime { get; set; } = string.Empty;

        public DateTime StartDate { get; set; } = DateTime.Today;

        [Range(1, 500, ErrorMessage = "Sĩ số phải từ 1 đến 500.")]
        public int Capacity { get; set; } = 20;

        public Course? Course { get; set; }
        public Teacher? Teacher { get; set; }
        public List<Enrollment> Enrollments { get; set; } = new();
        public List<Score> Scores { get; set; } = new();
        public List<AttendanceRecord> AttendanceRecords { get; set; } = new();
    }
}
