using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pusula.Api.Domain;
using Pusula.Api.DTOs;

namespace Pusula.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ProfilesController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		public ProfilesController(UserManager<ApplicationUser> userManager) { _userManager = userManager; }

		[HttpGet("me")]
		[Authorize]
		public async Task<ActionResult<UserDto>> Me()
		{
			var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
			if (userId == null) return Unauthorized();
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null) return NotFound();
			var roles = await _userManager.GetRolesAsync(user);
			return new UserDto(user.Id.ToString(), user.Email ?? string.Empty, user.FullName ?? string.Empty, roles.FirstOrDefault() ?? "Student");
		}
	}
}


