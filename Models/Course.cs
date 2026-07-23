using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public class Course
{
    public int Id { get; set; }

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

    [Range(0, 1_000_000_000)]
    [DataType(DataType.Currency)]
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

    public ICollection<CourseClass> CourseClasses { get; set; } = new List<CourseClass>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<CourseLecture> CourseLectures { get; set; } = new List<CourseLecture>();
    public ICollection<SavedCourse> SavedCourses { get; set; } = new List<SavedCourse>();
}
