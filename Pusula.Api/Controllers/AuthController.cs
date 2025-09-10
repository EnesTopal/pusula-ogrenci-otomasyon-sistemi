using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Pusula.Api.Domain;

namespace Pusula.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly RoleManager<ApplicationRole> _roleManager;
		private readonly IConfiguration _configuration;

		public AuthController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			RoleManager<ApplicationRole> roleManager,
			IConfiguration configuration)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
			_configuration = configuration;
		}

		public record RegisterRequest(string Email, string Password, string FullName, string Role);
		public record LoginRequest(string Email, string Password);
		public record AuthResponse(string Token, string UserId, string Email, string Role, string FullName);

		[HttpPost("register")]
		[AllowAnonymous]
		public async Task<IActionResult> Register([FromBody] RegisterRequest request)
		{
			if (!new[] { "Admin", "Teacher", "Student" }.Contains(request.Role))
			{
				return BadRequest("Invalid role.");
			}

			var user = new ApplicationUser
			{
				UserName = request.FullName,
				Email = request.Email,
				FullName = request.FullName
			};
			var result = await _userManager.CreateAsync(user, request.Password);
			if (!result.Succeeded) return BadRequest(result.Errors);

			if (!await _roleManager.RoleExistsAsync(request.Role))
			{
				await _roleManager.CreateAsync(new ApplicationRole { Name = request.Role });
			}
			await _userManager.AddToRoleAsync(user, request.Role);

			return Ok(new { Message = "Registered" });
		}

		[HttpPost("login")]
		[AllowAnonymous]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			var user = await _userManager.FindByEmailAsync(request.Email);
			if (user == null) return Unauthorized();

			var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
			if (!result.Succeeded) return Unauthorized();

			var roles = await _userManager.GetRolesAsync(user);
			var role = roles.FirstOrDefault() ?? "Student";

			var token = GenerateJwtToken(user, role);
			return Ok(new AuthResponse(token, user.Id.ToString(), user.Email!, role, user.FullName ?? string.Empty));
		}

		private string GenerateJwtToken(ApplicationUser user, string role)
		{
			var jwtSection = _configuration.GetSection("Jwt");
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var claims = new List<Claim>
			{
				new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
				new(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
				new(ClaimTypes.Email, user.Email ?? string.Empty),
				new(ClaimTypes.Role, role)
			};
			var token = new JwtSecurityToken(
				issuer: jwtSection["Issuer"],
				audience: jwtSection["Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(int.TryParse(jwtSection["ExpiresMinutes"], out var m) ? m : 120),
				signingCredentials: creds);
			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}


