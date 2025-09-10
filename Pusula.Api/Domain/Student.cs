namespace Pusula.Api.Domain
{
	public class Student
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public ApplicationUser User { get; set; } = null!;
		public DateTime EnrollmentDate { get; set; }
		public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
		public ICollection<Grade> Grades { get; set; } = new List<Grade>();
		public ICollection<Absence> Absences { get; set; } = new List<Absence>();
	}
}


