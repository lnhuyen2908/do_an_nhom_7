using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_do_an1.Models;

// CourseClass = Lớp học được mở cho một khóa học cụ thể.
// Ví dụ Course là IELTS Foundation, CourseClass có thể là lớp CL01 học tối thứ 2-4.
public class CourseClass
{
    // Khóa chính của lớp học.
    public int Id { get; set; }

    [Required, StringLength(20)]
    [RegularExpression(@"^CL\d{2,}$", ErrorMessage = "Mã lớp phải có dạng CL01.")]
    [Display(Name = "Mã lớp")]
    public string Code { get; set; } = string.Empty;

    // CourseId là khóa ngoại cho biết lớp này thuộc khóa học nào.
    [Display(Name = "Khóa học")]
    public int CourseId { get; set; }

    // TeacherId là khóa ngoại cho biết giáo viên nào phụ trách lớp.
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

    [DataType(DataType.Date)]
    [Display(Name = "Ngày kết thúc")]
    public DateTime? EndDate { get; set; }

    [Display(Name = "Trạng thái lớp")]
    public CourseClassStatus Status { get; set; } = CourseClassStatus.Open;

    // Capacity = sức chứa tối đa; chức năng đăng ký dùng số này để kiểm tra lớp đã đầy chưa.
    [Range(1, 500)]
    [Display(Name = "Sĩ số tối đa")]
    public int Capacity { get; set; } = 20;

    // Navigation property cho phép Include lấy chi tiết Course và Teacher từ khóa ngoại.
    [Display(Name = "Khóa học")]
    public Course Course { get; set; } = null!;
    [Display(Name = "Giáo viên")]
    public Teacher Teacher { get; set; } = null!;
    // Danh sách các phiếu đăng ký liên quan đến lớp này.
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Score> Scores { get; set; } = new List<Score>();
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();

    [NotMapped]
    public CourseClassStatus EffectiveStatus
    {
        get
        {
            var today = DateTime.Today;
            if (Status is CourseClassStatus.Locked or CourseClassStatus.Closed)
            {
                return Status;
            }

            if (EndDate.HasValue && EndDate.Value.Date < today)
            {
                return CourseClassStatus.Closed;
            }

            return StartDate.Date > today ? CourseClassStatus.Upcoming : CourseClassStatus.Open;
        }
    }

    [NotMapped]
    public bool CanRegister => EffectiveStatus is CourseClassStatus.Upcoming or CourseClassStatus.Open;
}
