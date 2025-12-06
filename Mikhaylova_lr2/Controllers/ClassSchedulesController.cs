using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mikhaylova_lr2.Models;

namespace Mikhaylova_lr2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClassSchedulesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClassSchedulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ClassSchedules
        [HttpGet]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<ClassSchedule>>> GetClassSchedules()
        {
            return await _context.ClassSchedules
                .Include(c => c.Subject)
                .ToListAsync();
        }

        // GET: api/ClassSchedules/5
        [HttpGet("{id}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<ClassSchedule>> GetClassSchedule(int id)
        {
            var classSchedule = await _context.ClassSchedules
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classSchedule == null)
            {
                return NotFound();
            }

            return classSchedule;
        }

        // POST: api/ClassSchedules
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ClassSchedule>> PostClassSchedule(ClassSchedule classSchedule)
        {
            if (!classSchedule.IsValid())
            {
                return BadRequest("Данные о занятии невалидны");
            }

            if (!classSchedule.IsValidGroupNumber())
            {
                return BadRequest("Номер группы должен быть в формате XX-00-00");
            }

            _context.ClassSchedules.Add(classSchedule);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClassSchedule", new { id = classSchedule.Id }, classSchedule);
        }

        // PUT: api/ClassSchedules/5
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> PutClassSchedule(int id, ClassSchedule classSchedule)
        {
            if (id != classSchedule.Id)
            {
                return BadRequest();
            }

            if (!classSchedule.IsValid())
            {
                return BadRequest("Данные о занятии невалидны");
            }

            _context.Entry(classSchedule).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClassScheduleExists(id))
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

        // DELETE: api/ClassSchedules/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteClassSchedule(int id)
        {
            var classSchedule = await _context.ClassSchedules.FindAsync(id);
            if (classSchedule == null)
            {
                return NotFound();
            }

            _context.ClassSchedules.Remove(classSchedule);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // === ЗАПРОСЫ С ИСПОЛЬЗОВАНИЕМ LINQ ===

        // 1. Получить расписание для определенной группы
        [HttpGet("group/{groupNumber}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<ClassSchedule>>> GetScheduleByGroup(string groupNumber)
        {
            var schedules = await _context.ClassSchedules
                .Include(c => c.Subject)
                .Where(c => c.GroupNumber == groupNumber)
                .OrderBy(c => c.Date)
                .ThenBy(c => c.PairNumber)
                .ToListAsync();

            return schedules;
        }

        // 2. Получить расписание для определенного преподавателя
        [HttpGet("teacher/{teacherName}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<ClassSchedule>>> GetScheduleByTeacher(string teacherName)
        {
            var schedules = await _context.ClassSchedules
                .Include(c => c.Subject)
                .Where(c => c.TeacherName.Contains(teacherName))
                .OrderBy(c => c.Date)
                .ThenBy(c => c.PairNumber)
                .ToListAsync();

            return schedules;
        }

        // 3. Получить расписание на определенную дату
        [HttpGet("date/{date}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<ClassSchedule>>> GetScheduleByDate(DateTime date)
        {
            var schedules = await _context.ClassSchedules
                .Include(c => c.Subject)
                .Where(c => c.Date.Date == date.Date)
                .OrderBy(c => c.PairNumber)
                .ToListAsync();

            return schedules;
        }

        // 4. Получить занятия по типу (лекция/семинар/лабораторная)
        [HttpGet("type/{classType}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<ClassSchedule>>> GetScheduleByType(string classType)
        {
            var schedules = await _context.ClassSchedules
                .Include(c => c.Subject)
                .Where(c => c.ClassType.ToLower() == classType.ToLower())
                .OrderBy(c => c.Date)
                .ThenBy(c => c.PairNumber)
                .ToListAsync();

            return schedules;
        }

        // 5. Получить сводку по аудиториям (какие аудитории заняты в выбранный день)
        [HttpGet("classrooms/{date}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<string>>> GetBusyClassrooms(DateTime date)
        {
            var classrooms = await _context.ClassSchedules
                .Where(c => c.Date.Date == date.Date)
                .Select(c => c.Classroom)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return classrooms;
        }

        // 6. Получить все занятия с информацией о предмете (используем Select для проекции)
        [HttpGet("with-subject-info")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<object>>> GetSchedulesWithSubjectInfo()
        {
            var schedules = await _context.ClassSchedules
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

            return schedules;
        }

        private bool ClassScheduleExists(int id)
        {
            return _context.ClassSchedules.Any(e => e.Id == id);
        }
    }
}
