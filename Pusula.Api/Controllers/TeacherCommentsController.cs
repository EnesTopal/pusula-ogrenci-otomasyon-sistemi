using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pusula.Api.Data;
using Pusula.Api.DTOs;
using Pusula.Api.Domain;
using System.Security.Claims;

namespace Pusula.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeacherCommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TeacherCommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<TeacherCommentWithDetailsDto>>> GetCommentsForStudent(Guid studentId)
        {
            var comments = await _context.TeacherComments
                .Include(tc => tc.Teacher)
                .ThenInclude(t => t.User)
                .Include(tc => tc.Course)
                .Where(tc => tc.StudentId == studentId)
                .OrderByDescending(tc => tc.CreatedAt)
                .Select(tc => new TeacherCommentWithDetailsDto(
                    tc.Id.ToString(),
                    tc.TeacherId.ToString(),
                    tc.StudentId.ToString(),
                    tc.CourseId.ToString(),
                    tc.Comment,
                    tc.CreatedAt,
                    tc.Teacher.User.FullName ?? "Unknown Teacher",
                    tc.Course.Name
                ))
                .ToListAsync();

            return Ok(comments);
        }

        [HttpGet("my-students")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<IEnumerable<StudentWithCoursesDto>>> GetMyStudents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == Guid.Parse(userId));
            if (teacher == null) return NotFound("Teacher not found");

            var students = await _context.Enrollments
                .Include(e => e.Student)
                .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Where(e => e.Course.TeacherId == teacher.Id)
                .Select(e => new StudentWithCoursesDto(
                    e.Student.Id.ToString(),
                    e.Student.User.FullName ?? "Unknown Student",
                    e.Course.Id.ToString(),
                    e.Course.Name
                ))
                .Distinct()
                .ToListAsync();

            return Ok(students);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<TeacherCommentDto>> CreateComment(CreateTeacherCommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.StudentId))
                return BadRequest("Student ID is required");
            
            if (string.IsNullOrWhiteSpace(request.Comment))
                return BadRequest("Comment is required");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == Guid.Parse(userId));
            if (teacher == null) return NotFound("Teacher not found");

            // Verify the student is enrolled in a course taught by this teacher
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.StudentId == Guid.Parse(request.StudentId) && 
                                        e.Course.TeacherId == teacher.Id);
            if (enrollment == null) return BadRequest("Student is not enrolled in any of your courses");

            var comment = new TeacherComment
            {
                Id = Guid.NewGuid(),
                TeacherId = teacher.Id,
                StudentId = Guid.Parse(request.StudentId),
                CourseId = enrollment.CourseId,
                Comment = request.Comment.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.TeacherComments.Add(comment);
            await _context.SaveChangesAsync();

            var commentDto = new TeacherCommentDto(
                comment.Id.ToString(),
                comment.TeacherId.ToString(),
                comment.StudentId.ToString(),
                comment.CourseId.ToString(),
                comment.Comment,
                comment.CreatedAt
            );

            return CreatedAtAction(nameof(GetCommentsForStudent), new { studentId = comment.StudentId }, commentDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == Guid.Parse(userId));
            if (teacher == null) return NotFound("Teacher not found");

            var comment = await _context.TeacherComments
                .FirstOrDefaultAsync(tc => tc.Id == id && tc.TeacherId == teacher.Id);
            if (comment == null) return NotFound();

            _context.TeacherComments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
