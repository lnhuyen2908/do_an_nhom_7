using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models
{
    public class Student : IValidatableObject
    {
        public int Id { get; set; }

        [RegularExpression(@"^$|^HV\d{2,}$", ErrorMessage = "Mã học viên phải có dạng HV01.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên học viên.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email học viên.")]
        [EmailAddress(ErrorMessage = "Email học viên chưa đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại học viên.")]
        [RegularExpression(@"^\d{9,11}$", ErrorMessage = "Số điện thoại phải gồm 9 đến 11 chữ số.")]
        public string Phone { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-18);

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ học viên.")]
        public string Address { get; set; } = string.Empty;

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
        public ICollection<Score> Scores { get; set; } = new List<Score>();
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public ICollection<SavedCourse> SavedCourses { get; set; } = new List<SavedCourse>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var today = DateTime.Today;
            if (DateOfBirth.Date > today.AddYears(-5) || DateOfBirth.Date < today.AddYears(-100))
            {
                yield return new ValidationResult(
                    "Ngày sinh không hợp lệ. Học viên phải từ 5 đến 100 tuổi.",
                    new[] { nameof(DateOfBirth) });
            }
        }
    }
}
