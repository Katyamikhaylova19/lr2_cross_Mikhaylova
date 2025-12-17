using Microsoft.EntityFrameworkCore;
using Mikhaylova_lr2.Models;

namespace Mikhaylova_lr2.Services
{
    public interface IStudentService
    {
        Task<IEnumerable<Student>> GetAllStudentsAsync();
        Task<Student?> GetStudentByIdAsync(int id);
        Task<Student> CreateStudentAsync(Student student);
        Task UpdateStudentAsync(Student student);
        Task DeleteStudentAsync(int id);
        Task<bool> AssignTeacherToStudentAsync(int studentId, int teacherId);
        Task<IEnumerable<Teacher>> GetTeachersByStudentIdAsync(int studentId);
        Task<IEnumerable<object>> GetStudentsByGroupAsync(string groupNumber);
    }

    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;

        public StudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            return await _context.Students.ToListAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(int id)
        {
            return await _context.Students
                .Include(s => s.Ratings)
                .ThenInclude(r => r.Teacher)
                .Include(s => s.StudentTeachers)
                .ThenInclude(st => st.Teacher)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Student> CreateStudentAsync(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return student;
        }

        public async Task UpdateStudentAsync(Student student)
        {
            _context.Entry(student).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStudentAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> AssignTeacherToStudentAsync(int studentId, int teacherId)
        {
            var student = await _context.Students.FindAsync(studentId);
            var teacher = await _context.Teachers.FindAsync(teacherId);

            if (student == null || teacher == null)
                return false;

            // Проверяем, не назначен ли уже этот преподаватель
            var existing = await _context.StudentTeachers
                .FirstOrDefaultAsync(st => st.StudentId == studentId && st.TeacherId == teacherId);

            if (existing != null)
                return false;

            var studentTeacher = new StudentTeacher
            {
                StudentId = studentId,
                TeacherId = teacherId
            };

            _context.StudentTeachers.Add(studentTeacher);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Teacher>> GetTeachersByStudentIdAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.StudentTeachers)
                .ThenInclude(st => st.Teacher)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                return Enumerable.Empty<Teacher>();

            return student.StudentTeachers.Select(st => st.Teacher);
        }

        public async Task<IEnumerable<object>> GetStudentsByGroupAsync(string groupNumber)
        {
            return await _context.Students
                .Where(s => s.GroupNumber == groupNumber)
                .Select(s => new
                {
                    s.Id,
                    s.FullName,
                    s.GroupNumber,
                    RatingsCount = s.Ratings.Count
                })
                .ToListAsync();
        }
    }
}
