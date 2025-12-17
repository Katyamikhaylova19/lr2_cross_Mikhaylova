using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mikhaylova_lr2.Models;
using Mikhaylova_lr2.Services;

namespace Mikhaylova_lr2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // GET: api/students
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _studentService.GetAllStudentsAsync();
            var result = students.Select(s => new
            {
                s.Id,
                s.FullName,
                s.GroupNumber,
                RatingsCount = s.Ratings.Count
            });
            return Ok(result);
        }

        // GET: api/students/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStudent(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
                return NotFound();

            var result = new
            {
                student.Id,
                student.FullName,
                student.LastName,
                student.FirstName,
                student.MiddleName,
                student.GroupNumber,
                Ratings = student.Ratings.Select(r => new
                {
                    r.Id,
                    r.Stars,
                    r.Review,
                    r.IsAnonymous,
                    r.CreatedAt,
                    Teacher = new { r.Teacher.Id, r.Teacher.FullName }
                }),
                Teachers = student.StudentTeachers.Select(st => new
                {
                    st.Teacher.Id,
                    st.Teacher.FullName
                })
            };

            return Ok(result);
        }

        // POST: api/students
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateStudent([FromBody] Student student)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!student.ValidateGroupNumber())
                return BadRequest("Неверный формат номера группы. Формат должен быть: XX-00-00");

            var createdStudent = await _studentService.CreateStudentAsync(student);
            return CreatedAtAction(nameof(GetStudent), new { id = createdStudent.Id }, createdStudent);
        }

        // PUT: api/students/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] Student student)
        {
            if (id != student.Id)
                return BadRequest();

            if (!student.ValidateGroupNumber())
                return BadRequest("Неверный формат номера группы. Формат должен быть: XX-00-00");

            try
            {
                await _studentService.UpdateStudentAsync(student);
            }
            catch
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/students/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
                return NotFound();

            await _studentService.DeleteStudentAsync(id);
            return NoContent();
        }

        // Дополнительные запросы с LINQ:

        // POST: api/students/assign-teacher
        [HttpPost("assign-teacher")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignTeacher([FromQuery] int studentId, [FromQuery] int teacherId)
        {
            var success = await _studentService.AssignTeacherToStudentAsync(studentId, teacherId);
            if (!success)
                return BadRequest("Не удалось назначить преподавателя студенту");

            return Ok();
        }

        // GET: api/students/by-group/АС-22-04
        [HttpGet("by-group/{groupNumber}")]
        [Authorize]
        public async Task<IActionResult> GetStudentsByGroup(string groupNumber)
        {
            var students = await _studentService.GetStudentsByGroupAsync(groupNumber);
            return Ok(students);
        }

        // GET: api/students/5/teachers
        [HttpGet("{id}/teachers")]
        [Authorize]
        public async Task<IActionResult> GetStudentTeachers(int id)
        {
            var teachers = await _studentService.GetTeachersByStudentIdAsync(id);
            var result = teachers.Select(t => new
            {
                t.Id,
                t.FullName,
                AverageRating = t.CalculateAverageRating()
            });
            return Ok(result);
        }
    }
}
