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
	public class StudentsController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;

		public StudentsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
		{
			_db = db;
			_userManager = userManager;
		}

		[HttpGet]
		[Authorize(Roles = "Admin,Teacher")]
		public async Task<IEnumerable<StudentDto>> GetAll()
		{
			var list = await _db.Students.Include(s => s.User).ToListAsync();
			return list.Select(s => new StudentDto(s.Id.ToString(), s.UserId.ToString(), s.User.Email ?? string.Empty, s.User.FullName ?? string.Empty, s.EnrollmentDate));
		}

		
		[HttpGet("me")]
		[Authorize(Roles = "Student")]
		public async Task<ActionResult<StudentDto>> GetMe()
		{
			var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
			if (userId == null) return Unauthorized();
			var guid = Guid.Parse(userId);
			var s = await _db.Students.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == guid);
			if (s == null) return NotFound();
			return new StudentDto(s.Id.ToString(), s.UserId.ToString(), s.User.Email ?? string.Empty, s.User.FullName ?? string.Empty, s.EnrollmentDate);
		}

		[HttpPost]
		[Authorize(Roles = "Admin,Teacher")]
		public async Task<ActionResult<StudentDto>> Create([FromBody] CreateStudentRequest request)
		{
			var user = new ApplicationUser { UserName = request.Email, Email = request.Email, FullName = request.FullName };
			var result = await _userManager.CreateAsync(user, request.Password);
			if (!result.Succeeded) return BadRequest(result.Errors);
			await _userManager.AddToRoleAsync(user, "Student");
			var student = new Student { Id = Guid.NewGuid(), UserId = user.Id, EnrollmentDate = DateTime.UtcNow };
			_db.Students.Add(student);
			await _db.SaveChangesAsync();
			return CreatedAtAction(nameof(GetById), new { id = student.Id }, new StudentDto(student.Id.ToString(), student.UserId.ToString(), user.Email ?? string.Empty, user.FullName ?? string.Empty, student.EnrollmentDate));
		}

		[HttpGet("{id}")]
		[Authorize(Roles = "Admin,Teacher")]
		public async Task<ActionResult<StudentDto>> GetById(Guid id)
		{
			var s = await _db.Students.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
			if (s == null) return NotFound();
			return new StudentDto(s.Id.ToString(), s.UserId.ToString(), s.User.Email ?? string.Empty, s.User.FullName ?? string.Empty, s.EnrollmentDate);
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "Admin,Teacher")]
		public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentRequest request)
		{
			var s = await _db.Students.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
			if (s == null) return NotFound();
			s.User.FullName = request.FullName;
			await _db.SaveChangesAsync();
			return NoContent();
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var s = await _db.Students.FirstOrDefaultAsync(x => x.Id == id);
			if (s == null) return NotFound();
			_db.Students.Remove(s);
			await _db.SaveChangesAsync();
			return NoContent();
		}
	}
}


