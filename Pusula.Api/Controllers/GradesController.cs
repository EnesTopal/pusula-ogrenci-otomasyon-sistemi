using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pusula.Api.Data;
using Pusula.Api.DTOs;
using Pusula.Api.Domain;

namespace Pusula.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class GradesController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		public GradesController(ApplicationDbContext db) { _db = db; }

		[HttpPost]
		[Authorize(Roles = "Teacher")]
		public async Task<ActionResult<GradeDto>> Create([FromBody] CreateGradeRequest request)
		{
			var grade = new Grade { Id = Guid.NewGuid(), StudentId = Guid.Parse(request.StudentId), CourseId = Guid.Parse(request.CourseId), Value = request.Value, GivenAt = DateTime.UtcNow };
			_db.Grades.Add(grade);
			await _db.SaveChangesAsync();
			return Ok(new GradeDto(grade.Id.ToString(), grade.StudentId.ToString(), grade.CourseId.ToString(), grade.Value, grade.GivenAt));
		}

		[HttpGet("by-student/{studentId}")]
		[Authorize(Roles = "Student,Teacher,Admin")]
		public async Task<IEnumerable<GradeDto>> GetByStudent(Guid studentId)
		{
			if (User.IsInRole("Student"))
    		{	
        		var userId = User.FindFirst("sub")?.Value
            		?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        		if (!Guid.TryParse(userId, out var userGuid) || userGuid != studentId)
        		{
            		Response.StatusCode = StatusCodes.Status403Forbidden; 
            		return Enumerable.Empty<GradeDto>();
        		}
    		}
			
			var list = await _db.Grades.Where(g => g.StudentId == studentId).ToListAsync();
			return list.Select(g => new GradeDto(g.Id.ToString(), g.StudentId.ToString(), g.CourseId.ToString(), g.Value, g.GivenAt));
		}
	}
}


