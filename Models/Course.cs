using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

// Course = Khóa học. Mỗi đối tượng Course tương ứng một dòng trong bảng Courses.
public class Course
{
    // Khóa chính của khóa học.
    public int Id { get; set; }

    // Mã khóa học bắt buộc có dạng CR01, CR02... và tối đa 20 ký tự.
    [Required, StringLength(20)]
    [RegularExpression(@"^CR\d{2,}$", ErrorMessage = "Mã khóa học phải có dạng CR01.")]
    [Display(Name = "Mã khóa học")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(150)]
    [Display(Name = "Tên khóa học")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Display(Name = "Trình độ")]
    public string Level { get; set; } = string.Empty;

    // Học phí phải nằm từ 0 đến 1 tỷ; decimal phù hợp với dữ liệu tiền tệ.
    [Range(0, 1_000_000_000)]
    [DataType(DataType.Currency)]
    [DisplayFormat(DataFormatString = "{0:N0} đ")]
    [Display(Name = "Học phí")]
    public decimal Tuition { get; set; }

    [Required, StringLength(80)]
    [Display(Name = "Thời lượng")]
    public string Duration { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    [Display(Name = "Mô tả")]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Ảnh khóa học")]
    public string ImageUrl { get; set; } = string.Empty;

    // Một khóa học có thể mở nhiều lớp khác nhau về lịch học, giáo viên và phòng học.
    public ICollection<CourseClass> CourseClasses { get; set; } = new List<CourseClass>();
    // Một khóa học có thể xuất hiện trong nhiều phiếu đăng ký của học viên.
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<CourseLecture> CourseLectures { get; set; } = new List<CourseLecture>();
    public ICollection<SavedCourse> SavedCourses { get; set; } = new List<SavedCourse>();
}
