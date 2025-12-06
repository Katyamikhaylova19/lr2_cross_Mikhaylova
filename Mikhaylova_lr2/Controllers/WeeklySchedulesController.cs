using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mikhaylova_lr2.Models;

namespace Mikhaylova_lr2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WeeklySchedulesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WeeklySchedulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/WeeklySchedules
        [HttpGet]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<WeeklySchedule>>> GetWeeklySchedules()
        {
            return await _context.WeeklySchedules
                .Include(w => w.Classes)
                .ThenInclude(c => c.Subject)
                .ToListAsync();
        }

        // GET: api/WeeklySchedules/5
        [HttpGet("{id}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<WeeklySchedule>> GetWeeklySchedule(int id)
        {
            var weeklySchedule = await _context.WeeklySchedules
                .Include(w => w.Classes)
                .ThenInclude(c => c.Subject)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (weeklySchedule == null)
            {
                return NotFound();
            }

            return weeklySchedule;
        }

        // POST: api/WeeklySchedules
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<WeeklySchedule>> PostWeeklySchedule(WeeklySchedule weeklySchedule)
        {
            if (!weeklySchedule.IsDailyScheduleValid())
            {
                return BadRequest("В день не может быть более 7 пар");
            }

            _context.WeeklySchedules.Add(weeklySchedule);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWeeklySchedule", new { id = weeklySchedule.Id }, weeklySchedule);
        }

        // PUT: api/WeeklySchedules/5
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> PutWeeklySchedule(int id, WeeklySchedule weeklySchedule)
        {
            if (id != weeklySchedule.Id)
            {
                return BadRequest();
            }

            if (!weeklySchedule.IsDailyScheduleValid())
            {
                return BadRequest("В день не может быть более 7 пар");
            }

            _context.Entry(weeklySchedule).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WeeklyScheduleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/WeeklySchedules/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteWeeklySchedule(int id)
        {
            var weeklySchedule = await _context.WeeklySchedules.FindAsync(id);
            if (weeklySchedule == null)
            {
                return NotFound();
            }

            _context.WeeklySchedules.Remove(weeklySchedule);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // === ДОПОЛНИТЕЛЬНЫЕ ЗАПРОСЫ ===

        // 1. Получить недельное расписание для группы
        [HttpGet("group/{groupNumber}/week/{weekNumber}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<WeeklySchedule>> GetWeeklyScheduleByGroup(string groupNumber, int weekNumber)
        {
            var weeklySchedule = await _context.WeeklySchedules
                .Include(w => w.Classes)
                .ThenInclude(c => c.Subject)
                .Where(w => w.GroupNumber == groupNumber && w.WeekNumber == weekNumber)
                .FirstOrDefaultAsync();

            if (weeklySchedule == null)
            {
                return NotFound();
            }

            return weeklySchedule;
        }

        // 2. Получить всех преподавателей в недельном расписании
        [HttpGet("{id}/teachers")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<string>>> GetTeachersInWeeklySchedule(int id)
        {
            var weeklySchedule = await _context.WeeklySchedules
                .Include(w => w.Classes)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (weeklySchedule == null)
            {
                return NotFound();
            }

            return weeklySchedule.GetAllTeachers();
        }

        // 3. Получить статистику по типам занятий за неделю
        [HttpGet("{id}/statistics")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<object>> GetWeeklyStatistics(int id)
        {
            var weeklySchedule = await _context.WeeklySchedules
                .Include(w => w.Classes)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (weeklySchedule == null)
            {
                return NotFound();
            }

            var classTypesCount = weeklySchedule.GetClassTypesCount();
            var totalClasses = weeklySchedule.Classes.Count;

            return new
            {
                TotalClasses = totalClasses,
                ClassTypes = classTypesCount,
                TeachersCount = weeklySchedule.GetAllTeachers().Count
            };
        }

        private bool WeeklyScheduleExists(int id)
        {
            return _context.WeeklySchedules.Any(e => e.Id == id);
        }
    }
}
