using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mikhaylova_lr2.Models;
using Mikhaylova_lr2.Services;

namespace Mikhaylova_lr2.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    // GET: api/Students
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
    {
        var students = await _studentService.GetAllStudentsAsync();
        return Ok(students);
    }

    // GET: api/Students/5
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<Student>> GetStudent(int id)
    {
        var student = await _studentService.GetStudentByIdAsync(id);

        if (student == null)
            return NotFound();

        return Ok(student);
    }

    // GET: api/Students/5/ratings-report
    [HttpGet("{id}/ratings-report")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<object>>> GetStudentRatingsReport(int id)
    {
        var report = await _studentService.GetStudentRatingsReportAsync(id);
        return Ok(report);
    }

    // GET: api/Students/by-group/1
    [HttpGet("by-group/{groupId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Student>>> GetStudentsByGroup(int groupId)
    {
        var students = await _studentService.GetStudentsByGroupAsync(groupId);
        return Ok(students);
    }

    // POST: api/Students
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Student>> PostStudent(Student student)
    {
        var createdStudent = await _studentService.CreateStudentAsync(student);
        return CreatedAtAction(nameof(GetStudent), new { id = createdStudent.Id }, createdStudent);
    }

    // PUT: api/Students/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutStudent(int id, Student student)
    {
        if (id != student.Id)
            return BadRequest();

        try
        {
            await _studentService.UpdateStudentAsync(id, student);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/Students/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var result = await _studentService.DeleteStudentAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    // GET: api/Students/with-ratings
    [HttpGet("with-ratings")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<object>>> GetStudentsWithRatings()
    {
        var students = await _studentService.GetAllStudentsAsync();

        var result = students
            .Where(s => s.Ratings != null && s.Ratings.Any())
            .Select(s => new
            {
                s.Id,
                FullName = s.GetFullName(),
                GroupNumber = s.Group.GroupNumber,
                RatingsCount = s.Ratings.Count,
                AverageScore = s.Ratings.Average(r => r.Score)
            })
            .ToList();

        return Ok(result);
    }
}