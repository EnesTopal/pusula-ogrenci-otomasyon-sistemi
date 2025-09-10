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
			var a = new Pusula.Api.Domain.Absence { Id = Guid.NewGuid(), StudentId = Guid.Parse(request.StudentId), CourseId = Guid.Parse(request.CourseId), Date = request.Date, Reason = request.Reason };
			_db.Absences.Add(a);
			await _db.SaveChangesAsync();
			return Ok(new AbsenceDto(a.Id.ToString(), a.StudentId.ToString(), a.CourseId.ToString(), a.Date, a.Reason));
		}

		[HttpGet("by-student/{studentId}")]
		[Authorize(Roles = "Student,Teacher,Admin")]
		public async Task<IEnumerable<AbsenceDto>> GetByStudent(Guid studentId)
		{
			var list = await _db.Absences.Where(x => x.StudentId == studentId).ToListAsync();
			return list.Select(a => new AbsenceDto(a.Id.ToString(), a.StudentId.ToString(), a.CourseId.ToString(), a.Date, a.Reason));
		}
	}
}


