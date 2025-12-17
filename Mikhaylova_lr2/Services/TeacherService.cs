using Microsoft.EntityFrameworkCore;
using Mikhaylova_lr2.Models;

namespace Mikhaylova_lr2.Services
{
    public interface ITeacherService
    {
        Task<IEnumerable<Teacher>> GetAllTeachersAsync();
        Task<Teacher?> GetTeacherByIdAsync(int id);
        Task<Teacher> CreateTeacherAsync(Teacher teacher);
        Task UpdateTeacherAsync(Teacher teacher);
        Task DeleteTeacherAsync(int id);
        Task<double> GetAverageRatingAsync(int teacherId);
        Task<IEnumerable<object>> GetTeachersWithRatingsAsync();
        Task<IEnumerable<object>> GetTopRatedTeachersAsync(int count);
        Task<IEnumerable<object>> GetTeachersByRatingRangeAsync(int min, int max);
    }

    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _context;

        public TeacherService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Teacher>> GetAllTeachersAsync()
        {
            return await _context.Teachers.ToListAsync();
        }

        public async Task<Teacher?> GetTeacherByIdAsync(int id)
        {
            return await _context.Teachers
                .Include(t => t.Ratings)
                .ThenInclude(r => r.Student)
                .Include(t => t.StudentTeachers)
                .ThenInclude(st => st.Student)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teacher> CreateTeacherAsync(Teacher teacher)
        {
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
            return teacher;
        }

        public async Task UpdateTeacherAsync(Teacher teacher)
        {
            _context.Entry(teacher).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTeacherAsync(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher != null)
            {
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<double> GetAverageRatingAsync(int teacherId)
        {
            var teacher = await _context.Teachers
                .Include(t => t.Ratings)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

            if (teacher == null)
                return 0;

            return teacher.CalculateAverageRating();
        }

        public async Task<IEnumerable<object>> GetTeachersWithRatingsAsync()
        {
            var teachers = await _context.Teachers
                .Include(t => t.Ratings)
                .ThenInclude(r => r.Student)
                .ToListAsync();

            var result = teachers.Select(t => new
            {
                t.Id,
                t.FullName,
                AverageRating = t.CalculateAverageRating(),
                RatingsCount = t.GetRatingsCount(),
                Ratings = t.Ratings.Select(r => new
                {
                    r.Id,
                    r.Stars,
                    r.Review,
                    r.IsAnonymous,
                    ReviewerName = r.GetReviewerDisplayName(),
                    r.CreatedAt
                })
            });

            return result;
        }

        public async Task<IEnumerable<object>> GetTopRatedTeachersAsync(int count)
        {
            var teachers = await _context.Teachers
                .Include(t => t.Ratings)
                .ToListAsync();

            return teachers
                .Where(t => t.Ratings.Any())
                .OrderByDescending(t => t.CalculateAverageRating())
                .Take(count)
                .Select(t => new
                {
                    t.Id,
                    t.FullName,
                    AverageRating = t.CalculateAverageRating(),
                    RatingsCount = t.GetRatingsCount()
                });
        }

        public async Task<IEnumerable<object>> GetTeachersByRatingRangeAsync(int min, int max)
        {
            var teachers = await _context.Teachers
                .Include(t => t.Ratings)
                .ToListAsync();

            return teachers
                .Where(t =>
                    t.Ratings.Any() &&
                    t.CalculateAverageRating() >= min &&
                    t.CalculateAverageRating() <= max)
                .Select(t => new
                {
                    t.Id,
                    t.FullName,
                    AverageRating = t.CalculateAverageRating(),
                    RatingsCount = t.GetRatingsCount()
                });
        }
    }
}
