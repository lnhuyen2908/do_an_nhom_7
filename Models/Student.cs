using System.ComponentModel.DataAnnotations;

namespace web_do_an1.Models;

public class Student : IValidatableObject
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    [RegularExpression(@"^ST\d{2,}$", ErrorMessage = "Mã học viên phải có dạng ST01.")]
    [Display(Name = "Mã học viên")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Ngày sinh")]
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-18);

    [Required, StringLength(250)]
    [Display(Name = "Địa chỉ")]
    public string Address { get; set; } = string.Empty;

    public UserAccount? UserAccount { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    public ICollection<Score> Scores { get; set; } = new List<Score>();
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public ICollection<SavedCourse> SavedCourses { get; set; } = new List<SavedCourse>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfBirth.Date > DateTime.Today.AddYears(-5) ||
            DateOfBirth.Date < DateTime.Today.AddYears(-100))
        {
            yield return new ValidationResult(
                "Học viên phải từ 5 đến 100 tuổi.",
                new[] { nameof(DateOfBirth) });
        }
    }
}
