using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mikhaylova_lr2.Models;
using Mikhaylova_lr2.Services;

namespace Mikhaylova_lr2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeachersController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        // GET: api/teachers
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllTeachers()
        {
            var teachers = await _teacherService.GetAllTeachersAsync();
            var result = teachers.Select(t => new
            {
                t.Id,
                t.FullName,
                AverageRating = t.CalculateAverageRating(),
                RatingsCount = t.GetRatingsCount()
            });
            return Ok(result);
        }

        // GET: api/teachers/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTeacher(int id)
        {
            var teacher = await _teacherService.GetTeacherByIdAsync(id);
            if (teacher == null)
                return NotFound();

            var result = new
            {
                teacher.Id,
                teacher.FullName,
                teacher.LastName,
                teacher.FirstName,
                teacher.MiddleName,
                AverageRating = teacher.CalculateAverageRating(),
                RatingsCount = teacher.GetRatingsCount(),
                Ratings = teacher.Ratings.Select(r => new
                {
                    r.Id,
                    r.Stars,
                    r.Review,
                    r.IsAnonymous,
                    ReviewerName = r.GetReviewerDisplayName(),
                    r.CreatedAt
                }),
                AssignedStudents = teacher.StudentTeachers.Select(st => new
                {
                    st.Student.Id,
                    st.Student.FullName,
                    st.Student.GroupNumber
                })
            };

            return Ok(result);
        }

        // POST: api/teachers
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTeacher([FromBody] Teacher teacher)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdTeacher = await _teacherService.CreateTeacherAsync(teacher);
            return CreatedAtAction(nameof(GetTeacher), new { id = createdTeacher.Id }, createdTeacher);
        }

        // PUT: api/teachers/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTeacher(int id, [FromBody] Teacher teacher)
        {
            if (id != teacher.Id)
                return BadRequest();

            try
            {
                await _teacherService.UpdateTeacherAsync(teacher);
            }
            catch
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/teachers/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _teacherService.GetTeacherByIdAsync(id);
            if (teacher == null)
                return NotFound();

            await _teacherService.DeleteTeacherAsync(id);
            return NoContent();
        }

        // Дополнительные запросы с LINQ:

        // GET: api/teachers/top/3
        [HttpGet("top/{count}")]
        [Authorize]
        public async Task<IActionResult> GetTopRatedTeachers(int count)
        {
            var teachers = await _teacherService.GetTopRatedTeachersAsync(count);
            return Ok(teachers);
        }

        // GET: api/teachers/with-ratings
        [HttpGet("with-ratings")]
        [Authorize]
        public async Task<IActionResult> GetTeachersWithRatings()
        {
            var teachers = await _teacherService.GetTeachersWithRatingsAsync();
            return Ok(teachers);
        }

        // GET: api/teachers/rating-range?min=4&max=5
        [HttpGet("rating-range")]
        [Authorize]
        public async Task<IActionResult> GetTeachersByRatingRange([FromQuery] int min = 3, [FromQuery] int max = 5)
        {
            var teachers = await _teacherService.GetTeachersByRatingRangeAsync(min, max);
            return Ok(teachers);
        }

        // GET: api/teachers/5/average-rating
        [HttpGet("{id}/average-rating")]
        [Authorize]
        public async Task<IActionResult> GetAverageRating(int id)
        {
            var average = await _teacherService.GetAverageRatingAsync(id);
            return Ok(new { TeacherId = id, AverageRating = average });
        }
    }
}
