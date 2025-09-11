using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pusula.Api.Data;
using Pusula.Api.Domain;
using Pusula.Api.DTOs;

namespace Pusula.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CoursesController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		public CoursesController(ApplicationDbContext db) { _db = db; }

		[HttpGet]
		[Authorize]
		public async Task<IEnumerable<CourseDto>> GetAll()
		{
			var list = await _db.Courses.Include(c => c.Teacher).ThenInclude(t => t.User).ToListAsync();
			return list.Select(c => new CourseDto(c.Id.ToString(), c.Name, c.Description, (CourseStatusDto)c.Status, c.TeacherId.ToString(), c.Teacher.User.FullName ?? string.Empty));
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<CourseDto>> Create([FromBody] CreateCourseRequest request)
		{
			var teacherId = Guid.Parse(request.TeacherId);
			if (!await _db.Teachers.AnyAsync(t => t.Id == teacherId)) return BadRequest("Teacher not found");
			var c = new Course { Id = Guid.NewGuid(), Name = request.Name, Description = request.Description, TeacherId = teacherId, Status = CourseStatus.NotStarted };
			_db.Courses.Add(c);
			await _db.SaveChangesAsync();
			var teacher = await _db.Teachers.Include(t => t.User).FirstAsync(t => t.Id == teacherId);
			return CreatedAtAction(nameof(GetById), new { id = c.Id }, new CourseDto(c.Id.ToString(), c.Name, c.Description, (CourseStatusDto)c.Status, c.TeacherId.ToString(), teacher.User.FullName ?? string.Empty));
		}

	[HttpGet("mine")]
	[Authorize(Roles = "Teacher")]
	public async Task<ActionResult<IEnumerable<CourseDto>>> GetMine()
	{
		var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
		if (userId == null) return Unauthorized();

		var guid = Guid.Parse(userId);
		var teacher = await _db.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.UserId == guid);
		if (teacher == null) return NotFound();

		var list = await _db.Courses
			.Where(c => c.TeacherId == teacher.Id)
			.Include(c => c.Teacher)
			.ThenInclude(t => t.User)
			.ToListAsync();

		return Ok(list.Select(c => new CourseDto(
			c.Id.ToString(),
			c.Name,
			c.Description,
			(CourseStatusDto)c.Status,
			c.TeacherId.ToString(),
			c.Teacher.User.FullName ?? string.Empty
		)));
	}

		[HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<CourseDto>> GetById(Guid id)
		{
			var c = await _db.Courses.Include(x => x.Teacher).ThenInclude(t => t.User).FirstOrDefaultAsync(x => x.Id == id);
			if (c == null) return NotFound();
			return new CourseDto(c.Id.ToString(), c.Name, c.Description, (CourseStatusDto)c.Status, c.TeacherId.ToString(), c.Teacher.User.FullName ?? string.Empty);
		}

		[HttpPatch("{id}/status")]
		[Authorize(Roles = "Teacher")]
		public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateCourseStatusRequest request)
		{
			var c = await _db.Courses.FirstOrDefaultAsync(x => x.Id == id);
			if (c == null) return NotFound();
			c.Status = (CourseStatus)request.Status;
			await _db.SaveChangesAsync();
			return NoContent();
		}

	[HttpPost("{id}/enrollments")]
	[Authorize(Roles = "Teacher")]
	public async Task<IActionResult> Enroll(Guid id, [FromBody] EnrollmentRequest request)
	{
		var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id);
		if (course == null) return NotFound();
		
		// Verify the teacher owns this course
		var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
		if (string.IsNullOrEmpty(userId)) return Unauthorized();
		
		var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.UserId.ToString() == userId);
		if (teacher == null || course.TeacherId != teacher.Id) return Forbid();
		
		var studentId = Guid.Parse(request.StudentId);
		if (!await _db.Students.AnyAsync(s => s.Id == studentId)) return BadRequest("Student not found");
		if (await _db.Enrollments.AnyAsync(e => e.CourseId == id && e.StudentId == studentId)) return Conflict("Already enrolled");
		_db.Enrollments.Add(new Enrollment { Id = Guid.NewGuid(), CourseId = id, StudentId = studentId, EnrolledAt = DateTime.UtcNow });
		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpDelete("{id}/enrollments/{studentId}")]
	[Authorize(Roles = "Teacher")]
	public async Task<IActionResult> Unenroll(Guid id, Guid studentId)
	{
		// Verify the teacher owns this course
		var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
		if (string.IsNullOrEmpty(userId)) return Unauthorized();
		
		var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.UserId.ToString() == userId);
		if (teacher == null) return Forbid();
		
		var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id);
		if (course == null || course.TeacherId != teacher.Id) return Forbid();
		
		var e = await _db.Enrollments.FirstOrDefaultAsync(e => e.CourseId == id && e.StudentId == studentId);
		if (e == null) return NotFound();
		_db.Enrollments.Remove(e);
		await _db.SaveChangesAsync();
		return NoContent();
	}

		[HttpPost("{id}/comments")]
		[Authorize(Roles = "Teacher")]
		public async Task<ActionResult<TeacherCommentDto>> Comment(Guid id, [FromBody] CreateTeacherCommentRequest request)
		{
			if (!await _db.Courses.AnyAsync(c => c.Id == id)) return NotFound();
			var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
    		if (string.IsNullOrEmpty(userId)) return Unauthorized();

    		var teacherId = await _db.Courses
        		.Where(c => c.Id == id)
        		.Select(c => c.TeacherId)
        		.FirstAsync();

    		if (teacherId.ToString() != userId)
        		return Forbid();
			var comment = new TeacherComment { Id = Guid.NewGuid(), CourseId = id, StudentId = Guid.Parse(request.StudentId), TeacherId = teacherId, Comment = request.Comment, CreatedAt = DateTime.UtcNow };
			_db.TeacherComments.Add(comment);
			await _db.SaveChangesAsync();
			return Ok(new TeacherCommentDto(comment.Id.ToString(), comment.TeacherId.ToString(), comment.StudentId.ToString(), comment.CourseId.ToString(), comment.Comment, comment.CreatedAt));
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin,Teacher")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id);
			if (course == null) return NotFound();

			// If user is Teacher, verify they own this course
			if (User.IsInRole("Teacher"))
			{
				var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
				if (string.IsNullOrEmpty(userId)) return Unauthorized();
				
				var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.UserId.ToString() == userId);
				if (teacher == null || course.TeacherId != teacher.Id) return Forbid();
			}

			// Delete grades for this course
			var grades = await _db.Grades.Where(g => g.CourseId == id).ToListAsync();
			_db.Grades.RemoveRange(grades);

			// Delete absences for this course
			var absences = await _db.Absences.Where(a => a.CourseId == id).ToListAsync();
			_db.Absences.RemoveRange(absences);

			// Delete teacher comments for this course
			var comments = await _db.TeacherComments.Where(tc => tc.CourseId == id).ToListAsync();
			_db.TeacherComments.RemoveRange(comments);

			// Delete enrollments for this course
			var enrollments = await _db.Enrollments.Where(e => e.CourseId == id).ToListAsync();
			_db.Enrollments.RemoveRange(enrollments);

			// Finally delete the course
			_db.Courses.Remove(course);

			await _db.SaveChangesAsync();
			return NoContent();
		}
	}
}


