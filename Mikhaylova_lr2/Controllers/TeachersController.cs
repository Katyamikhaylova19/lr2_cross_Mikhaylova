using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mikhaylova_lr2.Models;
using Mikhaylova_lr2.Services;

namespace Mikhaylova_lr2.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TeachersController : ControllerBase
{
    private readonly ITeacherService _teacherService;

    public TeachersController(ITeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    // GET: api/Teachers
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Teacher>>> GetTeachers()
    {
        var teachers = await _teacherService.GetAllTeachersAsync();
        return Ok(teachers);
    }

    // GET: api/Teachers/5
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<Teacher>> GetTeacher(int id)
    {
        var teacher = await _teacherService.GetTeacherByIdAsync(id);

        if (teacher == null)
            return NotFound();

        return Ok(teacher);
    }

    // GET: api/Teachers/top-rated?count=5
    [HttpGet("top-rated")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Teacher>>> GetTopRatedTeachers([FromQuery] int count = 5)
    {
        var teachers = await _teacherService.GetTopRatedTeachersAsync(count);
        return Ok(teachers);
    }

    // GET: api/Teachers/no-ratings
    [HttpGet("no-ratings")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Teacher>>> GetTeachersWithNoRatings()
    {
        var teachers = await _teacherService.GetTeachersWithNoRatingsAsync();
        return Ok(teachers);
    }

    // GET: api/Teachers/group-statistics/АС-22-04
    [HttpGet("group-statistics/{groupNumber}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<object>>> GetGroupStatistics(string groupNumber)
    {
        var statistics = await _teacherService.GetGroupStatisticsAsync(groupNumber);
        return Ok(statistics);
    }

    // GET: api/Teachers/5/detailed-report
    [HttpGet("{id}/detailed-report")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<object>>> GetTeacherDetailedReport(int id)
    {
        var report = await _teacherService.GetTeacherDetailedReportAsync(id);
        return Ok(report);
    }

    // POST: api/Teachers
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Teacher>> PostTeacher(Teacher teacher)
    {
        var createdTeacher = await _teacherService.CreateTeacherAsync(teacher);
        return CreatedAtAction(nameof(GetTeacher), new { id = createdTeacher.Id }, createdTeacher);
    }

    // PUT: api/Teachers/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutTeacher(int id, Teacher teacher)
    {
        if (id != teacher.Id)
            return BadRequest();

        try
        {
            await _teacherService.UpdateTeacherAsync(id, teacher);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/Teachers/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTeacher(int id)
    {
        var result = await _teacherService.DeleteTeacherAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    // POST: api/Teachers/5/groups/3
    [HttpPost("{teacherId}/groups/{groupId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TeacherGroup>> AddTeacherToGroup(int teacherId, int groupId)
    {
        try
        {
            var teacherGroup = await _teacherService.AddTeacherToGroupAsync(teacherId, groupId);
            return CreatedAtAction(nameof(AddTeacherToGroup), new { teacherId, groupId }, teacherGroup);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    // DELETE: api/Teachers/5/groups/3
    [HttpDelete("{teacherId}/groups/{groupId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveTeacherFromGroup(int teacherId, int groupId)
    {
        var result = await _teacherService.RemoveTeacherFromGroupAsync(teacherId, groupId);
        if (!result)
            return NotFound();

        return NoContent();
    }

    // GET: api/Teachers/search?name=иван
    [HttpGet("search")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<object>>> SearchTeachers([FromQuery] string name)
    {
        var teachers = await _teacherService.GetAllTeachersAsync();

        var result = teachers
            .Where(t => t.LastName.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                       t.FirstName.Contains(name, StringComparison.OrdinalIgnoreCase))
            .Select(t => new
            {
                t.Id,
                FullName = t.GetFullName(),
                AverageRating = t.CalculateAverageRating(),
                TeachingGroups = t.TeacherGroups.Select(tg => tg.Group.GroupNumber)
            })
            .ToList();

        return Ok(result);
    }
}