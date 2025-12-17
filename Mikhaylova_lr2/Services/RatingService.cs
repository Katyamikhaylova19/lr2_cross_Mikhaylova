using Microsoft.EntityFrameworkCore;
using Mikhaylova_lr2.Models;

namespace Mikhaylova_lr2.Services
{
    public interface IRatingService
    {
        Task<IEnumerable<Rating>> GetAllRatingsAsync();
        Task<Rating?> GetRatingByIdAsync(int id);
        Task<Rating> CreateRatingAsync(Rating rating);
        Task UpdateRatingAsync(Rating rating);
        Task DeleteRatingAsync(int id);
        Task<bool> CanStudentRateTeacherAsync(int studentId, int teacherId);
        Task<IEnumerable<Rating>> GetRatingsByTeacherIdAsync(int teacherId);
        Task<IEnumerable<object>> GetRatingsWithDetailsAsync();
        Task<IEnumerable<object>> GetRecentRatingsAsync(int count);
        Task<IEnumerable<object>> GetAnonymousRatingsAsync();
    }

    public class RatingService : IRatingService
    {
        private readonly ApplicationDbContext _context;

        public RatingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Rating>> GetAllRatingsAsync()
        {
            return await _context.Ratings
                .Include(r => r.Teacher)
                .Include(r => r.Student)
                .ToListAsync();
        }

        public async Task<Rating?> GetRatingByIdAsync(int id)
        {
            return await _context.Ratings
                .Include(r => r.Teacher)
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Rating> CreateRatingAsync(Rating rating)
        {
            // Проверяем, может ли студент оценить этого преподавателя
            if (!await CanStudentRateTeacherAsync(rating.StudentId, rating.TeacherId))
                throw new InvalidOperationException("Студент не может оценить данного преподавателя.");

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();
            return rating;
        }

        public async Task UpdateRatingAsync(Rating rating)
        {
            _context.Entry(rating).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRatingAsync(int id)
        {
            var rating = await _context.Ratings.FindAsync(id);
            if (rating != null)
            {
                _context.Ratings.Remove(rating);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CanStudentRateTeacherAsync(int studentId, int teacherId)
        {
            return await _context.StudentTeachers
                .AnyAsync(st => st.StudentId == studentId && st.TeacherId == teacherId);
        }

        public async Task<IEnumerable<Rating>> GetRatingsByTeacherIdAsync(int teacherId)
        {
            return await _context.Ratings
                .Include(r => r.Student)
                .Where(r => r.TeacherId == teacherId)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetRatingsWithDetailsAsync()
        {
            var ratings = await _context.Ratings
                .Include(r => r.Teacher)
                .Include(r => r.Student)
                .ToListAsync();

            var result = ratings.Select(r => new
            {
                r.Id,
                r.Stars,
                r.Review,
                r.IsAnonymous,
                ReviewerName = r.GetReviewerDisplayName(),
                StarsDisplay = r.GetStarsDisplay(),
                r.CreatedAt,
                Teacher = new { r.Teacher.Id, r.Teacher.FullName },
                Student = new { r.Student.Id, r.Student.FullName, r.Student.GroupNumber }
            });

            return result;
        }

        public async Task<IEnumerable<object>> GetRecentRatingsAsync(int count)
        {
            return await _context.Ratings
                .Include(r => r.Teacher)
                .Include(r => r.Student)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .Select(r => new
                {
                    r.Id,
                    r.Stars,
                    r.Review,
                    r.IsAnonymous,
                    ReviewerName = r.GetReviewerDisplayName(),
                    r.CreatedAt,
                    TeacherName = r.Teacher.FullName,
                    StudentName = r.Student.FullName
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetAnonymousRatingsAsync()
        {
            return await _context.Ratings
                .Include(r => r.Teacher)
                .Where(r => r.IsAnonymous)
                .Select(r => new
                {
                    r.Id,
                    r.Stars,
                    r.Review,
                    TeacherName = r.Teacher.FullName,
                    r.CreatedAt
                })
                .ToListAsync();
        }
    }
}
