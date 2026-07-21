namespace web_do_an1.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int? ClassId { get; set; }
        public string Status { get; set; } = "ChoDuyet";
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
        public Student? Student { get; set; }
        public Course? Course { get; set; }
        public CourseClass? AssignedClass { get; set; }
        public Payment? Payment { get; set; }
    }
}
