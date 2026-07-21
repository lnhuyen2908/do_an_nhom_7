using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã khóa học.")]
        [RegularExpression(@"^KH\d{2,}$", ErrorMessage = "Mã khóa học phải có dạng KH01.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên khóa học.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập trình độ khóa học.")]
        public string Level { get; set; } = string.Empty;

        [Range(1, 1000000000, ErrorMessage = "Học phí phải lớn hơn 0.")]
        public decimal Tuition { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời lượng khóa học.")]
        public string Duration { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả khóa học.")]
        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;
        public List<CourseClass> Classes { get; set; } = new();
        public List<Enrollment> Enrollments { get; set; } = new();
        public List<CourseLecture> Lectures { get; set; } = new();
        public List<SavedCourse> SavedCourses { get; set; } = new();
    }
}
