using Microsoft.AspNetCore.Identity;

namespace Pusula.Api.Domain
{
	public class ApplicationUser : IdentityUser<Guid>
	{
		public string? FullName { get; set; }
		public Student? StudentProfile { get; set; }
		public Teacher? TeacherProfile { get; set; }
	}
}


