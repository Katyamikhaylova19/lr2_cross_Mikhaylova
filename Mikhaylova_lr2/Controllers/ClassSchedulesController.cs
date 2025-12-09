using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mikhaylova_lr2.Models;
using Mikhaylova_lr2.Services;

namespace Mikhaylova_lr2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClassSchedulesController : ControllerBase
    {
        private readonly IClassScheduleService _classScheduleService;

        public ClassSchedulesController(IClassScheduleService classScheduleService)
        {
            _classScheduleService = classScheduleService;
        }

        // GET: api/ClassSchedules
        [HttpGet]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<ClassSchedule>>> GetClassSchedules()
        {
            var schedules = await _classScheduleService.GetAllAsync();
            return Ok(schedules);
        }

        // GET: api/ClassSchedules/5
        [HttpGet("{id}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<ClassSchedule>> GetClassSchedule(int id)
        {
            var classSchedule = await _classScheduleService.GetByIdAsync(id);
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
            try
            {
                var createdSchedule = await _classScheduleService.CreateAsync(classSchedule);
                return CreatedAtAction("GetClassSchedule", new { id = createdSchedule.Id }, createdSchedule);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
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

            try
            {
                await _classScheduleService.UpdateAsync(classSchedule);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                if (!await _classScheduleService.ExistsAsync(id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // DELETE: api/ClassSchedules/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteClassSchedule(int id)
        {
            try
            {
                await _classScheduleService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // === ЗАПРОСЫ С ИСПОЛЬЗОВАНИЕМ LINQ ===

        // 1. Получить расписание для определенной группы
        [HttpGet("group/{groupNumber}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<ClassSchedule>>> GetScheduleByGroup(string groupNumber)
        {
            var schedules = await _classScheduleService.GetByGroupAsync(groupNumber);
            return Ok(schedules);
        }

        // 2. Получить расписание для определенного преподавателя
        [HttpGet("teacher/{teacherName}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<ClassSchedule>>> GetScheduleByTeacher(string teacherName)
        {
            var schedules = await _classScheduleService.GetByTeacherAsync(teacherName);
            return Ok(schedules);
        }

        // 3. Получить расписание на определенную дату
        [HttpGet("date/{date}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<ClassSchedule>>> GetScheduleByDate(DateTime date)
        {
            var schedules = await _classScheduleService.GetByDateAsync(date);
            return Ok(schedules);
        }

        // 4. Получить занятия по типу (лекция/семинар/лабораторная)
        [HttpGet("type/{classType}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<ClassSchedule>>> GetScheduleByType(string classType)
        {
            var schedules = await _classScheduleService.GetByTypeAsync(classType);
            return Ok(schedules);
        }

        // 5. Получить сводку по аудиториям (какие аудитории заняты в выбранный день)
        [HttpGet("classrooms/{date}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<string>>> GetBusyClassrooms(DateTime date)
        {
            var classrooms = await _classScheduleService.GetBusyClassroomsAsync(date);
            return Ok(classrooms);
        }

        // 6. Получить все занятия с информацией о предмете
        [HttpGet("with-subject-info")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<object>>> GetSchedulesWithSubjectInfo()
        {
            var schedules = await _classScheduleService.GetSchedulesWithSubjectInfoAsync();
            return Ok(schedules);
        }
    }
}
