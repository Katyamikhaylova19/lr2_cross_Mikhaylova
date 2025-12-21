using Microsoft.EntityFrameworkCore;
using Mikhaylova_lr2.Data;
using Mikhaylova_lr2.Models;

namespace Mikhaylova_lr2.Services;

public interface IStudentService
{
    Task<Student?> GetStudentByIdAsync(int id);
    Task<IEnumerable<Student>> GetAllStudentsAsync();
    Task<Student> CreateStudentAsync(Student student);
    Task<Student> UpdateStudentAsync(int id, Student student);
    Task<bool> DeleteStudentAsync(int id);
    Task<IEnumerable<Student>> GetStudentsByGroupAsync(int groupId);
    Task<bool> CanStudentRateTeacherAsync(int studentId, int teacherId);
    Task<IEnumerable<object>> GetStudentRatingsReportAsync(int studentId);
}

public class StudentService : IStudentService
{
    private readonly AppDbContext _context;

    public StudentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        return await _context.Students
            .Include(s => s.Group)
            .Include(s => s.Ratings)
                .ThenInclude(r => r.Teacher)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Student>> GetAllStudentsAsync()
    {
        return await _context.Students
            .Include(s => s.Group)
            .Include(s => s.Ratings)
            .ToListAsync();
    }

    public async Task<Student> CreateStudentAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<Student> UpdateStudentAsync(int id, Student student)
    {
        var existingStudent = await _context.Students.FindAsync(id);
        if (existingStudent == null)
            throw new ArgumentException("Student not found");

        existingStudent.FirstName = student.FirstName;
        existingStudent.LastName = student.LastName;
        existingStudent.MiddleName = student.MiddleName;
        existingStudent.GroupId = student.GroupId;

        await _context.SaveChangesAsync();
        return existingStudent;
    }

    public async Task<bool> DeleteStudentAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
            return false;

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Student>> GetStudentsByGroupAsync(int groupId)
    {
        return await _context.Students
            .Where(s => s.GroupId == groupId)
            .Include(s => s.Ratings)
            .ToListAsync();
    }

    public async Task<bool> CanStudentRateTeacherAsync(int studentId, int teacherId)
    {
        var student = await _context.Students
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null || student.Group == null)
            return false;

        return await _context.TeacherGroups
            .AnyAsync(tg => tg.TeacherId == teacherId && tg.GroupId == student.GroupId);
    }

    public async Task<IEnumerable<object>> GetStudentRatingsReportAsync(int studentId)
    {
        var student = await GetStudentByIdAsync(studentId);
        if (student == null)
            return Enumerable.Empty<object>();

        var report = student.Ratings.Select(r => new
        {
            Teacher = r.Teacher.GetFullName(),
            Score = r.Score,
            Review = r.Review,
            IsAnonymous = r.IsAnonymous,
            CreatedDate = r.CreatedDate
        }).ToList();

        return report;
    }
}