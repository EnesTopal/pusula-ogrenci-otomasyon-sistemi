namespace Pusula.Api.Domain
{
	public class TeacherComment
	{
		public Guid Id { get; set; }
		public Guid TeacherId { get; set; }
		public Teacher Teacher { get; set; } = null!;
		public Guid StudentId { get; set; }
		public Student Student { get; set; } = null!;
		public Guid CourseId { get; set; }
		public Course Course { get; set; } = null!;
		public string Comment { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
	}
}


