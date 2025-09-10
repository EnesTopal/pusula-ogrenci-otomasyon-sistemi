using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pusula.Api.Domain;

namespace Pusula.Api.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}

		public DbSet<Student> Students => Set<Student>();
		public DbSet<Teacher> Teachers => Set<Teacher>();
		public DbSet<Course> Courses => Set<Course>();
		public DbSet<Enrollment> Enrollments => Set<Enrollment>();
		public DbSet<Grade> Grades => Set<Grade>();
		public DbSet<Absence> Absences => Set<Absence>();
		public DbSet<TeacherComment> TeacherComments => Set<TeacherComment>();

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<ApplicationUser>(b =>
			{
				b.HasOne(u => u.StudentProfile)
					.WithOne(s => s.User)
					.HasForeignKey<Student>(s => s.UserId)
					.OnDelete(DeleteBehavior.Cascade);

				b.HasOne(u => u.TeacherProfile)
					.WithOne(t => t.User)
					.HasForeignKey<Teacher>(t => t.UserId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			builder.Entity<Course>(b =>
			{
				b.Property(c => c.Name).IsRequired().HasMaxLength(200);
				b.HasOne(c => c.Teacher)
					.WithMany(t => t.Courses)
					.HasForeignKey(c => c.TeacherId)
					.OnDelete(DeleteBehavior.Restrict);
			});

			builder.Entity<Enrollment>(b =>
			{
				b.HasOne(e => e.Student)
					.WithMany(s => s.Enrollments)
					.HasForeignKey(e => e.StudentId)
					.OnDelete(DeleteBehavior.Cascade);
				b.HasOne(e => e.Course)
					.WithMany(c => c.Enrollments)
					.HasForeignKey(e => e.CourseId)
					.OnDelete(DeleteBehavior.Cascade);
				b.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
			});

			builder.Entity<Grade>(b =>
			{
				b.HasOne(g => g.Student)
					.WithMany(s => s.Grades)
					.HasForeignKey(g => g.StudentId)
					.OnDelete(DeleteBehavior.Cascade);
				b.HasOne(g => g.Course)
					.WithMany()
					.HasForeignKey(g => g.CourseId)
					.OnDelete(DeleteBehavior.Cascade);
				b.Property(g => g.Value).HasPrecision(5, 2);
			});

			builder.Entity<Absence>(b =>
			{
				b.HasOne(a => a.Student)
					.WithMany(s => s.Absences)
					.HasForeignKey(a => a.StudentId)
					.OnDelete(DeleteBehavior.Cascade);
				b.HasOne(a => a.Course)
					.WithMany()
					.HasForeignKey(a => a.CourseId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			builder.Entity<TeacherComment>(b =>
			{
				b.HasOne(tc => tc.Teacher)
					.WithMany()
					.HasForeignKey(tc => tc.TeacherId)
					.OnDelete(DeleteBehavior.Cascade);
				b.HasOne(tc => tc.Student)
					.WithMany()
					.HasForeignKey(tc => tc.StudentId)
					.OnDelete(DeleteBehavior.Cascade);
				b.HasOne(tc => tc.Course)
					.WithMany(c => c.Comments)
					.HasForeignKey(tc => tc.CourseId)
					.OnDelete(DeleteBehavior.Cascade);
				b.Property(tc => tc.Comment).IsRequired().HasMaxLength(1000);
			});
		}
	}
}


