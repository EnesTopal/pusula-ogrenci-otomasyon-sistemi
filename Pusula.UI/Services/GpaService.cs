using Pusula.Api.DTOs;

namespace Pusula.UI.Services
{
    public class GpaService
    {
        private readonly ApiClient _apiClient;

        public GpaService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<CourseGpaDto>> GetGpaByCourseAsync()
        {
            try
            {
                // First get the current student's information
                var student = await _apiClient.GetAsync<StudentDto>("api/students/me");
                if (student == null)
                    return new List<CourseGpaDto>();

                var studentId = Guid.Parse(student.Id);

                // Get the grades for this student (this returns GradeDto, not GradeWithCourseDto)
                var grades = await _apiClient.GetAsync<List<GradeDto>>($"api/grades/by-student/{studentId}");
                
                if (grades == null || !grades.Any())
                    return new List<CourseGpaDto>();

                // Get all courses to map course IDs to names
                var courses = await _apiClient.GetAsync<List<CourseDto>>("api/courses");
                var courseNames = courses?.ToDictionary(c => c.Id, c => c.Name) ?? new Dictionary<string, string>();

                var gpaByCourse = grades
                    .GroupBy(g => g.CourseId)
                    .Select(group => new CourseGpaDto
                    {
                        CourseId = group.Key,
                        CourseName = courseNames.TryGetValue(group.Key, out var name) ? name : $"Course {group.Key}",
                        AverageGrade = Math.Round(group.Average(g => g.Value), 2),
                        GradeCount = group.Count()
                    })
                    .OrderBy(c => c.CourseName)
                    .ToList();

                return gpaByCourse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating GPA: {ex.Message}");
                return new List<CourseGpaDto>();
            }
        }
    }

    public class CourseGpaDto
    {
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public decimal AverageGrade { get; set; }
        public int GradeCount { get; set; }
    }
}
