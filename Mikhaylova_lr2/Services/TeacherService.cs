using Microsoft.EntityFrameworkCore;
using Mikhaylova_lr2.Data;
using Mikhaylova_lr2.Models;

namespace Mikhaylova_lr2.Services;

public interface ITeacherService
{
    // CRUD операции
    Task<Teacher?> GetTeacherByIdAsync(int id);
    Task<IEnumerable<Teacher>> GetAllTeachersAsync();
    Task<Teacher> CreateTeacherAsync(Teacher teacher);
    Task<Teacher> UpdateTeacherAsync(int id, Teacher teacher);
    Task<bool> DeleteTeacherAsync(int id);

    // Специфичные операции
    Task<double> GetAverageRatingAsync(int teacherId);
    Task<IEnumerable<Teacher>> GetTeachersByGroupAsync(string groupNumber);
    Task<TeacherGroup> AddTeacherToGroupAsync(int teacherId, int groupId);
    Task<bool> RemoveTeacherFromGroupAsync(int teacherId, int groupId);

    // Методы для отчетов
    Task<IEnumerable<Teacher>> GetTopRatedTeachersAsync(int count = 5);
    Task<IEnumerable<Teacher>> GetTeachersWithNoRatingsAsync();
    Task<IEnumerable<object>> GetGroupStatisticsAsync(string groupNumber);
    Task<IEnumerable<object>> GetTeacherDetailedReportAsync(int teacherId);
}

public class TeacherService : ITeacherService
{
    private readonly AppDbContext _context;

    public TeacherService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Teacher?> GetTeacherByIdAsync(int id)
    {
        return await _context.Teachers
            .Include(t => t.Ratings)
            .Include(t => t.TeacherGroups)
                .ThenInclude(tg => tg.Group)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Teacher>> GetAllTeachersAsync()
    {
        return await _context.Teachers
            .Include(t => t.Ratings)
            .Include(t => t.TeacherGroups)
                .ThenInclude(tg => tg.Group)
            .ToListAsync();
    }

    public async Task<Teacher> CreateTeacherAsync(Teacher teacher)
    {
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();
        return teacher;
    }

    public async Task<Teacher> UpdateTeacherAsync(int id, Teacher teacher)
    {
        var existingTeacher = await _context.Teachers.FindAsync(id);
        if (existingTeacher == null)
            throw new ArgumentException("Teacher not found");

        existingTeacher.FirstName = teacher.FirstName;
        existingTeacher.LastName = teacher.LastName;
        existingTeacher.MiddleName = teacher.MiddleName;

        await _context.SaveChangesAsync();
        return existingTeacher;
    }

    public async Task<bool> DeleteTeacherAsync(int id)
    {
        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher == null)
            return false;

        _context.Teachers.Remove(teacher);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<double> GetAverageRatingAsync(int teacherId)
    {
        var teacher = await GetTeacherByIdAsync(teacherId);
        return teacher?.CalculateAverageRating() ?? 0;
    }

    public async Task<IEnumerable<Teacher>> GetTeachersByGroupAsync(string groupNumber)
    {
        return await _context.Teachers
            .Where(t => t.TeacherGroups.Any(tg => tg.Group.GroupNumber == groupNumber))
            .Include(t => t.Ratings)
            .Include(t => t.TeacherGroups)
                .ThenInclude(tg => tg.Group)
            .ToListAsync();
    }

    public async Task<TeacherGroup> AddTeacherToGroupAsync(int teacherId, int groupId)
    {
        var existing = await _context.TeacherGroups
            .FirstOrDefaultAsync(tg => tg.TeacherId == teacherId && tg.GroupId == groupId);

        if (existing != null)
            throw new InvalidOperationException("Teacher is already assigned to this group");

        var teacherGroup = new TeacherGroup
        {
            TeacherId = teacherId,
            GroupId = groupId
        };

        _context.TeacherGroups.Add(teacherGroup);
        await _context.SaveChangesAsync();
        return teacherGroup;
    }

    public async Task<bool> RemoveTeacherFromGroupAsync(int teacherId, int groupId)
    {
        var teacherGroup = await _context.TeacherGroups
            .FirstOrDefaultAsync(tg => tg.TeacherId == teacherId && tg.GroupId == groupId);

        if (teacherGroup == null)
            return false;

        _context.TeacherGroups.Remove(teacherGroup);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Teacher>> GetTopRatedTeachersAsync(int count = 5)
    {
        return await _context.Teachers
            .Include(t => t.Ratings)
            .Where(t => t.Ratings.Any())
            .OrderByDescending(t => t.Ratings.Average(r => r.Score))
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Teacher>> GetTeachersWithNoRatingsAsync()
    {
        return await _context.Teachers
            .Include(t => t.Ratings)
            .Where(t => !t.Ratings.Any())
            .ToListAsync();
    }

    public async Task<IEnumerable<object>> GetGroupStatisticsAsync(string groupNumber)
    {
        var group = await _context.Groups
            .Include(g => g.Students)
                .ThenInclude(s => s.Ratings)
            .Include(g => g.TeacherGroups)
                .ThenInclude(tg => tg.Teacher)
                .ThenInclude(t => t.Ratings)
            .FirstOrDefaultAsync(g => g.GroupNumber == groupNumber);

        if (group == null)
            return Enumerable.Empty<object>();

        var statistics = new List<object>();

        foreach (var teacherGroup in group.TeacherGroups)
        {
            var teacher = teacherGroup.Teacher;
            var ratings = teacher.Ratings.Where(r => r.Student.GroupId == group.Id).ToList();

            statistics.Add(new
            {
                Teacher = teacher.GetFullName(),
                AverageRating = ratings.Any() ? ratings.Average(r => r.Score) : 0,
                RatingCount = ratings.Count,
                ReviewsCount = ratings.Count(r => !string.IsNullOrEmpty(r.Review))
            });
        }

        return statistics;
    }

    public async Task<IEnumerable<object>> GetTeacherDetailedReportAsync(int teacherId)
    {
        var teacher = await GetTeacherByIdAsync(teacherId);
        if (teacher == null)
            return Enumerable.Empty<object>();

        var report = new List<object>();

        // Группы, в которых преподает
        var groups = teacher.TeacherGroups.Select(tg => new
        {
            GroupNumber = tg.Group.GroupNumber,
            StudentCount = tg.Group.Students.Count
        }).ToList();

        // Все оценки с деталями
        var ratings = teacher.Ratings.Select(r => new
        {
            Score = r.Score,
            Review = r.Review,
            IsAnonymous = r.IsAnonymous,
            Author = r.GetReviewAuthor(),
            StudentGroup = r.Student.Group.GroupNumber,
            CreatedDate = r.CreatedDate
        }).ToList();

        // Сводная статистика
        var summary = new
        {
            TeacherName = teacher.GetFullName(),
            AverageRating = teacher.CalculateAverageRating(),
            TotalRatings = teacher.Ratings.Count,
            TeachingGroups = groups,
            Ratings = ratings
        };

        report.Add(summary);
        return report;
    }
}
