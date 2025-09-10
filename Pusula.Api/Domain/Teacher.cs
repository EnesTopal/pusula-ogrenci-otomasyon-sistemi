namespace Pusula.Api.Domain
{
	public class Teacher
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public ApplicationUser User { get; set; } = null!;
		public DateTime HireDate { get; set; }
		public ICollection<Course> Courses { get; set; } = new List<Course>();
	}
}


