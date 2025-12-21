using Microsoft.EntityFrameworkCore;
using Mikhaylova_lr2.Data;
using Mikhaylova_lr2.Models;

namespace Mikhaylova_lr2.Services;

public interface IRatingService
{
    Task<Rating?> GetRatingByIdAsync(int id);
    Task<IEnumerable<Rating>> GetRatingsByTeacherAsync(int teacherId);
    Task<IEnumerable<Rating>> GetRatingsByStudentAsync(int studentId);
    Task<Rating> CreateRatingAsync(Rating rating, int studentId);
    Task<Rating> UpdateRatingAsync(int id, Rating rating, int studentId);
    Task<bool> DeleteRatingAsync(int id, int studentId);
}

public class RatingService : IRatingService
{
    private readonly AppDbContext _context;

    public RatingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Rating?> GetRatingByIdAsync(int id)
    {
        return await _context.Ratings
            .Include(r => r.Teacher)
            .Include(r => r.Student)
                .ThenInclude(s => s.Group)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Rating>> GetRatingsByTeacherAsync(int teacherId)
    {
        return await _context.Ratings
            .Where(r => r.TeacherId == teacherId)
            .Include(r => r.Student)
                .ThenInclude(s => s.Group)
            .ToListAsync();
    }

    public async Task<IEnumerable<Rating>> GetRatingsByStudentAsync(int studentId)
    {
        return await _context.Ratings
            .Where(r => r.StudentId == studentId)
            .Include(r => r.Teacher)
            .ToListAsync();
    }

    public async Task<Rating> CreateRatingAsync(Rating rating, int studentId)
    {
        // Проверяем, может ли студент оценить преподавателя

        // Проверяем, не существует ли уже оценка
        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.StudentId == studentId && r.TeacherId == rating.TeacherId);

        if (existingRating != null)
            throw new InvalidOperationException("Student has already rated this teacher");

        rating.StudentId = studentId;
        rating.CreatedDate = DateTime.UtcNow;

        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();
        return rating;
    }

    public async Task<Rating> UpdateRatingAsync(int id, Rating rating, int studentId)
    {
        var existingRating = await _context.Ratings
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (existingRating == null)
            throw new ArgumentException("Rating not found");

        if (!existingRating.CanBeModifiedByStudent(studentId))
            throw new UnauthorizedAccessException("Student cannot modify this rating");

        existingRating.UpdateRating(rating.Score, rating.Review, rating.IsAnonymous);

        await _context.SaveChangesAsync();
        return existingRating;
    }

    public async Task<bool> DeleteRatingAsync(int id, int studentId)
    {
        var rating = await _context.Ratings.FindAsync(id);
        if (rating == null)
            return false;

        if (!rating.CanBeModifiedByStudent(studentId))
            throw new UnauthorizedAccessException("Student cannot delete this rating");

        _context.Ratings.Remove(rating);
        await _context.SaveChangesAsync();
        return true;
    }
}