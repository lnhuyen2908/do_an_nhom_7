using System.ComponentModel.DataAnnotations.Schema;

namespace web_do_an1.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }

        [ForeignKey(nameof(Course))]
        public int CourseId { get; set; }

        [ForeignKey(nameof(AssignedClass))]
        public int? ClassId { get; set; }
        public string Status { get; set; } = "ChoDuyet";
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
        public Student? Student { get; set; }
        public Course? Course { get; set; }
        public CourseClass? AssignedClass { get; set; }
        public Payment? Payment { get; set; }
    }
}
