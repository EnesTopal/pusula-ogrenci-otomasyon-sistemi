namespace Pusula.Api.Domain
{
	public class Grade
	{
		public Guid Id { get; set; }
		public Guid StudentId { get; set; }
		public Student Student { get; set; } = null!;
		public Guid CourseId { get; set; }
		public Course Course { get; set; } = null!;
		public decimal Value { get; set; }
		public DateTime GivenAt { get; set; }
	}
}


