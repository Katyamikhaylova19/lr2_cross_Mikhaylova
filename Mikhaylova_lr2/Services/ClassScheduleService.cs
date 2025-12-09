using Microsoft.EntityFrameworkCore;
using Mikhaylova_lr2.Models;

namespace Mikhaylova_lr2.Services
{
    public interface IClassScheduleService
    {
        Task<IEnumerable<ClassSchedule>> GetAllAsync();
        Task<ClassSchedule> GetByIdAsync(int id);
        Task<ClassSchedule> CreateAsync(ClassSchedule classSchedule);
        Task UpdateAsync(ClassSchedule classSchedule);
        Task DeleteAsync(int id);
        Task<IEnumerable<ClassSchedule>> GetByGroupAsync(string groupNumber);
        Task<IEnumerable<ClassSchedule>> GetByTeacherAsync(string teacherName);
        Task<IEnumerable<ClassSchedule>> GetByDateAsync(DateTime date);
        Task<IEnumerable<ClassSchedule>> GetByTypeAsync(string classType);
        Task<IEnumerable<string>> GetBusyClassroomsAsync(DateTime date);
        Task<IEnumerable<object>> GetSchedulesWithSubjectInfoAsync();
        Task<bool> ExistsAsync(int id);
    }

    public class ClassScheduleService : IClassScheduleService
    {
        private readonly ApplicationDbContext _context;

        public ClassScheduleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ClassSchedule>> GetAllAsync()
        {
            return await _context.ClassSchedules
                .Include(c => c.Subject)
                .ToListAsync();
        }

        public async Task<ClassSchedule> GetByIdAsync(int id)
        {
            return await _context.ClassSchedules
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ClassSchedule> CreateAsync(ClassSchedule classSchedule)
        {
            if (!classSchedule.IsValid())
                throw new ArgumentException("Данные о занятии невалидны");

            if (!classSchedule.IsValidGroupNumber())
                throw new ArgumentException("Номер группы должен быть в формате XX-00-00");

            _context.ClassSchedules.Add(classSchedule);
            await _context.SaveChangesAsync();
            return classSchedule;
        }

        public async Task UpdateAsync(ClassSchedule classSchedule)
        {
            if (!classSchedule.IsValid())
                throw new ArgumentException("Данные о занятии невалидны");

            _context.Entry(classSchedule).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var classSchedule = await _context.ClassSchedules.FindAsync(id);
            if (classSchedule != null)
            {
                _context.ClassSchedules.Remove(classSchedule);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ClassSchedule>> GetByGroupAsync(string groupNumber)
        {
            return await _context.ClassSchedules
                .Include(c => c.Subject)
                .Where(c => c.GroupNumber == groupNumber)
                .OrderBy(c => c.Date)
                .ThenBy(c => c.PairNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClassSchedule>> GetByTeacherAsync(string teacherName)
        {
            return await _context.ClassSchedules
                .Include(c => c.Subject)
                .Where(c => c.TeacherName.Contains(teacherName))
                .OrderBy(c => c.Date)
                .ThenBy(c => c.PairNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClassSchedule>> GetByDateAsync(DateTime date)
        {
            return await _context.ClassSchedules
                .Include(c => c.Subject)
                .Where(c => c.Date.Date == date.Date)
                .OrderBy(c => c.PairNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClassSchedule>> GetByTypeAsync(string classType)
        {
            return await _context.ClassSchedules
                .Include(c => c.Subject)
                .Where(c => c.ClassType.ToLower() == classType.ToLower())
                .OrderBy(c => c.Date)
                .ThenBy(c => c.PairNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetBusyClassroomsAsync(DateTime date)
        {
            return await _context.ClassSchedules
                .Where(c => c.Date.Date == date.Date)
                .Select(c => c.Classroom)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetSchedulesWithSubjectInfoAsync()
        {
            return await _context.ClassSchedules
                .Include(c => c.Subject)
                .Select(c => new
                {
                    c.Id,
                    Date = c.Date.ToString("yyyy-MM-dd"),
                    Day = c.DayOfWeek,
                    Time = c.GetPairTime(),
                    SubjectName = c.Subject.Name,
                    c.Classroom,
                    c.GroupNumber,
                    c.ClassType,
                    c.TeacherName
                })
                .OrderBy(c => c.Date)
                .ThenBy(c => c.Day)
                .ThenBy(c => c.Time)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ClassSchedules.AnyAsync(e => e.Id == id);
        }
    }
}
