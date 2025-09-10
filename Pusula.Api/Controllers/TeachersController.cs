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
	}
}


