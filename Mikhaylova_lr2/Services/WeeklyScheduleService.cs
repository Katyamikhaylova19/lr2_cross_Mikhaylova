using Microsoft.EntityFrameworkCore;
using Mikhaylova_lr2.Models;

namespace Mikhaylova_lr2.Services
{
    public interface IWeeklyScheduleService
    {
        Task<IEnumerable<WeeklySchedule>> GetAllAsync();
        Task<WeeklySchedule> GetByIdAsync(int id);
        Task<WeeklySchedule> CreateAsync(WeeklySchedule weeklySchedule);
        Task UpdateAsync(WeeklySchedule weeklySchedule);
        Task DeleteAsync(int id);
        Task<WeeklySchedule> GetByGroupAndWeekAsync(string groupNumber, int weekNumber);
        Task<IEnumerable<string>> GetTeachersInWeeklyScheduleAsync(int id);
        Task<object> GetWeeklyStatisticsAsync(int id);
        Task<bool> ExistsAsync(int id);
    }

    public class WeeklyScheduleService : IWeeklyScheduleService
    {
        private readonly ApplicationDbContext _context;

        public WeeklyScheduleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WeeklySchedule>> GetAllAsync()
        {
            return await _context.WeeklySchedules
                .Include(w => w.Classes)
                .ThenInclude(c => c.Subject)
                .ToListAsync();
        }

        public async Task<WeeklySchedule> GetByIdAsync(int id)
        {
            return await _context.WeeklySchedules
                .Include(w => w.Classes)
                .ThenInclude(c => c.Subject)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<WeeklySchedule> CreateAsync(WeeklySchedule weeklySchedule)
        {
            if (!weeklySchedule.IsDailyScheduleValid())
                throw new ArgumentException("В день не может быть более 7 пар");

            _context.WeeklySchedules.Add(weeklySchedule);
            await _context.SaveChangesAsync();
            return weeklySchedule;
        }

        public async Task UpdateAsync(WeeklySchedule weeklySchedule)
        {
            if (!weeklySchedule.IsDailyScheduleValid())
                throw new ArgumentException("В день не может быть более 7 пар");

            _context.Entry(weeklySchedule).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var weeklySchedule = await _context.WeeklySchedules.FindAsync(id);
            if (weeklySchedule != null)
            {
                _context.WeeklySchedules.Remove(weeklySchedule);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<WeeklySchedule> GetByGroupAndWeekAsync(string groupNumber, int weekNumber)
        {
            return await _context.WeeklySchedules
                .Include(w => w.Classes)
                .ThenInclude(c => c.Subject)
                .Where(w => w.GroupNumber == groupNumber && w.WeekNumber == weekNumber)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<string>> GetTeachersInWeeklyScheduleAsync(int id)
        {
            var weeklySchedule = await _context.WeeklySchedules
                .Include(w => w.Classes)
                .FirstOrDefaultAsync(w => w.Id == id);

            return weeklySchedule?.GetAllTeachers() ?? new List<string>();
        }

        public async Task<object> GetWeeklyStatisticsAsync(int id)
        {
            var weeklySchedule = await _context.WeeklySchedules
                .Include(w => w.Classes)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (weeklySchedule == null)
                return null;

            var classTypesCount = weeklySchedule.GetClassTypesCount();
            var totalClasses = weeklySchedule.Classes.Count;

            return new
            {
                TotalClasses = totalClasses,
                ClassTypes = classTypesCount,
                TeachersCount = weeklySchedule.GetAllTeachers().Count
            };
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.WeeklySchedules.AnyAsync(e => e.Id == id);
        }
    }
}
