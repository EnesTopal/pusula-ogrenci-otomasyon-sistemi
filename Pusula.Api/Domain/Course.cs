namespace Pusula.Api.Domain
{
	public enum CourseStatus
	{
		NotStarted = 0,
		InProgress = 1,
		Completed = 2
	}

	public class Course
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public Guid TeacherId { get; set; }
		public Teacher Teacher { get; set; } = null!;
		public CourseStatus Status { get; set; }
		public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
		public ICollection<TeacherComment> Comments { get; set; } = new List<TeacherComment>();
	}
}


