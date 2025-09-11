using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pusula.Api.Data;
using Pusula.Api.DTOs;

namespace Pusula.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AbsencesController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		public AbsencesController(ApplicationDbContext db) { _db = db; }

		[HttpPost]
		[Authorize(Roles = "Teacher")]
		public async Task<ActionResult<AbsenceDto>> Create([FromBody] CreateAbsenceRequest request)
		{
			var courseId = Guid.Parse(request.CourseId);
    		var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
    		if (course == null) return NotFound();

		var userId = User.FindFirst("sub")?.Value
		    ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
		if (string.IsNullOrEmpty(userId)) return Unauthorized();
		
		// Find the teacher by UserId to get the Teacher.Id
		var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.UserId.ToString() == userId);
		if (teacher == null || course.TeacherId != teacher.Id)
			return Forbid();

			var a = new Pusula.Api.Domain.Absence { Id = Guid.NewGuid(), StudentId = Guid.Parse(request.StudentId), CourseId = Guid.Parse(request.CourseId), Date = request.Date.Kind == DateTimeKind.Utc ? request.Date : request.Date.ToUniversalTime(), Reason = request.Reason };
			_db.Absences.Add(a);
			await _db.SaveChangesAsync();
			return Ok(new AbsenceDto(a.Id.ToString(), a.StudentId.ToString(), a.CourseId.ToString(), a.Date, a.Reason));
		}

		[HttpGet("by-student/{studentId}")]
		[Authorize(Roles = "Student,Teacher,Admin")]
		public async Task<IEnumerable<AbsenceDto>> GetByStudent(Guid studentId)
		{
			if (User.IsInRole("Student"))
    		{
        		var userId = User.FindFirst("sub")?.Value
            		?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        		if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var currentUserId))
        		{
            		Response.StatusCode = StatusCodes.Status403Forbidden;
            		return Enumerable.Empty<AbsenceDto>();
        		}
        		
        		// Check if the student is accessing their own data
        		var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == currentUserId);
        		if (student == null || student.Id != studentId)
        		{
            		Response.StatusCode = StatusCodes.Status403Forbidden;
            		return Enumerable.Empty<AbsenceDto>();
        		}
    		}
			var list = await _db.Absences.Where(x => x.StudentId == studentId).ToListAsync();
			return list.Select(a => new AbsenceDto(a.Id.ToString(), a.StudentId.ToString(), a.CourseId.ToString(), a.Date, a.Reason));
		}

		[HttpGet("student/{studentId}")]
		[Authorize(Roles = "Teacher,Admin")]
		public async Task<IEnumerable<AbsenceWithCourseDto>> GetAbsencesWithCourseForStudent(Guid studentId)
		{
			var absences = await _db.Absences
				.Include(a => a.Course)
				.Where(a => a.StudentId == studentId)
				.OrderByDescending(a => a.Date)
				.Select(a => new AbsenceWithCourseDto(
					a.Id.ToString(),
					a.StudentId.ToString(),
					a.CourseId.ToString(),
					a.Course.Name,
					a.Date,
					a.Reason
				))
				.ToListAsync();

			return absences;
		}
	}
}


