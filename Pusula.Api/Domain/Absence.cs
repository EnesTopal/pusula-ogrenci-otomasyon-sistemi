namespace Pusula.Api.Domain
{
	public class Absence
	{
		public Guid Id { get; set; }
		public Guid StudentId { get; set; }
		public Student Student { get; set; } = null!;
		public Guid CourseId { get; set; }
		public Course Course { get; set; } = null!;
		public DateTime Date { get; set; }
		public string? Reason { get; set; }
	}
}


