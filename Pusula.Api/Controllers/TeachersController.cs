using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pusula.Api.Data;
using Pusula.Api.Domain;
using Pusula.Api.DTOs;

namespace Pusula.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class TeachersController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;

		public TeachersController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
		{
			_db = db;
			_userManager = userManager;
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<IEnumerable<TeacherDto>> GetAll()
		{
			var list = await _db.Teachers.Include(t => t.User).ToListAsync();
			return list.Select(t => new TeacherDto(t.Id.ToString(), t.UserId.ToString(), t.User.Email ?? string.Empty, t.User.FullName ?? string.Empty, t.HireDate));
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<TeacherDto>> Create([FromBody] CreateTeacherRequest request)
		{
			Console.WriteLine("Create Teacher {0}, {1}, {2}", request.FullName, request.Email, request.Password);
			var user = new ApplicationUser { UserName = request.Email, Email = request.Email, FullName = request.FullName };
			var result = await _userManager.CreateAsync(user, request.Password);
			if (!result.Succeeded) return BadRequest(result.Errors);
			await _userManager.AddToRoleAsync(user, "Teacher");
			var teacher = new Teacher { Id = Guid.NewGuid(), UserId = user.Id, HireDate = DateTime.UtcNow };
			_db.Teachers.Add(teacher);
			await _db.SaveChangesAsync();
			return CreatedAtAction(nameof(GetById), new { id = teacher.Id }, new TeacherDto(teacher.Id.ToString(), teacher.UserId.ToString(), user.Email ?? string.Empty, user.FullName ?? string.Empty, teacher.HireDate));
		}

		[HttpGet("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<TeacherDto>> GetById(Guid id)
		{
			var t = await _db.Teachers.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
			if (t == null) return NotFound();
			return new TeacherDto(t.Id.ToString(), t.UserId.ToString(), t.User.Email ?? string.Empty, t.User.FullName ?? string.Empty, t.HireDate);
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeacherRequest request)
		{
			var t = await _db.Teachers.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
			if (t == null) return NotFound();
			t.User.FullName = request.FullName;
			await _db.SaveChangesAsync();
			return NoContent();
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var t = await _db.Teachers.FirstOrDefaultAsync(x => x.Id == id);
			if (t == null) return NotFound();
			_db.Teachers.Remove(t);
			await _db.SaveChangesAsync();
			return NoContent();
		}

	[HttpGet("my-courses")]
	[Authorize(Roles = "Teacher")]
	public async Task<IEnumerable<CourseDto>> GetMyCourses()
	{
		var userId = User.FindFirst("sub")?.Value
			?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
		if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<CourseDto>();

		// First find the teacher by UserId
		var teacher = await _db.Teachers
			.Include(t => t.User)
			.FirstOrDefaultAsync(t => t.UserId.ToString() == userId);
		
		if (teacher == null) return Enumerable.Empty<CourseDto>();

		var courses = await _db.Courses
			.Where(c => c.TeacherId == teacher.Id)
			.ToListAsync();
		
		return courses.Select(c => new CourseDto(
			c.Id.ToString(),
			c.Name,
			c.Description,
			(CourseStatusDto)c.Status,
			c.TeacherId.ToString(),
			teacher.User.FullName ?? string.Empty
		));
	}

	[HttpGet("my-courses/{courseId}/students")]
	[Authorize(Roles = "Teacher")]
	public async Task<IEnumerable<StudentDto>> GetStudentsInMyCourse(Guid courseId)
	{
		Console.WriteLine($"GetStudentsInMyCourse called with courseId: {courseId}");
		
		var userId = User.FindFirst("sub")?.Value
			?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
		if (string.IsNullOrEmpty(userId)) 
		{
			Console.WriteLine("No userId found");
			return Enumerable.Empty<StudentDto>();
		}

		// First find the teacher by UserId
		var teacher = await _db.Teachers
			.FirstOrDefaultAsync(t => t.UserId.ToString() == userId);
		
		if (teacher == null) 
		{
			Console.WriteLine("Teacher not found");
			return Enumerable.Empty<StudentDto>();
		}

		// Verify the teacher owns this course
		var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == courseId && c.TeacherId == teacher.Id);
		if (course == null) 
		{
			Console.WriteLine($"Course not found or teacher doesn't own it. CourseId: {courseId}, TeacherId: {teacher.Id}");
			return Enumerable.Empty<StudentDto>();
		}

		var enrollments = await _db.Enrollments
			.Include(e => e.Student)
			.ThenInclude(s => s.User)
			.Where(e => e.CourseId == courseId)
			.ToListAsync();

		Console.WriteLine($"Found {enrollments.Count} enrollments for course {courseId}");

		var result = enrollments.Select(e => new StudentDto(
			e.Student.Id.ToString(),
			e.Student.UserId.ToString(),
			e.Student.User.Email ?? string.Empty,
			e.Student.User.FullName ?? string.Empty,
			e.Student.EnrollmentDate
		)).ToList();

		Console.WriteLine($"Returning {result.Count} students");
		return result;
	}
	}
}


