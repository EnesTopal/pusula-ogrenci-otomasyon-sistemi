namespace Pusula.Api.DTOs
{
	public record UserDto(string Id, string Email, string FullName, string Role);

	public record StudentDto(string Id, string UserId, string Email, string FullName, DateTime EnrollmentDate);
	public record CreateStudentRequest(string Email, string FullName, string Password);
	public record UpdateStudentRequest(string FullName);

	public record TeacherDto(string Id, string UserId, string Email, string FullName, DateTime HireDate);
	public record CreateTeacherRequest(string Email, string FullName, string Password);
	public record UpdateTeacherRequest(string FullName);

	public enum CourseStatusDto { NotStarted = 0, InProgress = 1, Completed = 2 }
	public record CourseDto(string Id, string Name, string? Description, CourseStatusDto Status, string TeacherId, string TeacherName);
	public record CreateCourseRequest(string Name, string? Description, string TeacherId);
	public record UpdateCourseStatusRequest(CourseStatusDto Status);
	public record EnrollmentRequest(string StudentId);

	public record GradeDto(string Id, string StudentId, string CourseId, decimal Value, DateTime GivenAt);
	public record CreateGradeRequest(string StudentId, string CourseId, decimal Value);

	public record AbsenceDto(string Id, string StudentId, string CourseId, DateTime Date, string? Reason);
	public record CreateAbsenceRequest(string StudentId, string CourseId, DateTime Date, string? Reason);

	public record TeacherCommentDto(string Id, string TeacherId, string StudentId, string CourseId, string Comment, DateTime CreatedAt);
	public record CreateTeacherCommentRequest(string StudentId, string Comment);
}


