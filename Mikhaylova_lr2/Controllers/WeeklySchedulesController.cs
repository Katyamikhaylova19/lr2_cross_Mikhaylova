using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mikhaylova_lr2.Models;
using Mikhaylova_lr2.Services;

namespace Mikhaylova_lr2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WeeklySchedulesController : ControllerBase
    {
        private readonly IWeeklyScheduleService _weeklyScheduleService;

        public WeeklySchedulesController(IWeeklyScheduleService weeklyScheduleService)
        {
            _weeklyScheduleService = weeklyScheduleService;
        }

        // GET: api/WeeklySchedules
        [HttpGet]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<WeeklySchedule>>> GetWeeklySchedules()
        {
            var schedules = await _weeklyScheduleService.GetAllAsync();
            return Ok(schedules);
        }

        // GET: api/WeeklySchedules/5
        [HttpGet("{id}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<WeeklySchedule>> GetWeeklySchedule(int id)
        {
            var weeklySchedule = await _weeklyScheduleService.GetByIdAsync(id);
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
            try
            {
                var createdSchedule = await _weeklyScheduleService.CreateAsync(weeklySchedule);
                return CreatedAtAction("GetWeeklySchedule", new { id = createdSchedule.Id }, createdSchedule);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
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

            try
            {
                await _weeklyScheduleService.UpdateAsync(weeklySchedule);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                if (!await _weeklyScheduleService.ExistsAsync(id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // DELETE: api/WeeklySchedules/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteWeeklySchedule(int id)
        {
            try
            {
                await _weeklyScheduleService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // === ДОПОЛНИТЕЛЬНЫЕ ЗАПРОСЫ ===

        // 1. Получить недельное расписание для группы
        [HttpGet("group/{groupNumber}/week/{weekNumber}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<WeeklySchedule>> GetWeeklyScheduleByGroup(string groupNumber, int weekNumber)
        {
            var weeklySchedule = await _weeklyScheduleService.GetByGroupAndWeekAsync(groupNumber, weekNumber);
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
            var teachers = await _weeklyScheduleService.GetTeachersInWeeklyScheduleAsync(id);
            return Ok(teachers);
        }

        // 3. Получить статистику по типам занятий за неделю
        [HttpGet("{id}/statistics")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<object>> GetWeeklyStatistics(int id)
        {
            var statistics = await _weeklyScheduleService.GetWeeklyStatisticsAsync(id);
            if (statistics == null)
            {
                return NotFound();
            }
            return Ok(statistics);
        }
    }
}
