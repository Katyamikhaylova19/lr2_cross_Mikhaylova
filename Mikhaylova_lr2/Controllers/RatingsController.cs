using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mikhaylova_lr2.Models;
using Mikhaylova_lr2.Services;

namespace Mikhaylova_lr2.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;
    private readonly IStudentService _studentService;

    public RatingsController(IRatingService ratingService, IStudentService studentService)
    {
        _ratingService = ratingService;
        _studentService = studentService;
    }

    // GET: api/Ratings
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Rating>>> GetRatings()
    {
        var ratings = await _ratingService.GetRatingsByStudentAsync(0); // Будет переопределено
        return Ok(ratings);
    }

    // GET: api/Ratings/5
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<Rating>> GetRating(int id)
    {
        var rating = await _ratingService.GetRatingByIdAsync(id);

        if (rating == null)
            return NotFound();

        return Ok(rating);
    }

    // GET: api/Ratings/teacher/5
    [HttpGet("teacher/{teacherId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Rating>>> GetRatingsByTeacher(int teacherId)
    {
        var ratings = await _ratingService.GetRatingsByTeacherAsync(teacherId);
        return Ok(ratings);
    }

    // GET: api/Ratings/student/5
    [HttpGet("student/{studentId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Rating>>> GetRatingsByStudent(int studentId)
    {
        var ratings = await _ratingService.GetRatingsByStudentAsync(studentId);
        return Ok(ratings);
    }

    // GET: api/Ratings/my-ratings
    [HttpGet("my-ratings")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Rating>>> GetMyRatings()
    {
        // В реальном приложении здесь будет извлечение studentId из JWT токена
        var studentId = GetCurrentStudentId();

        var ratings = await _ratingService.GetRatingsByStudentAsync(studentId);
        return Ok(ratings);
    }

    // POST: api/Ratings
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Rating>> PostRating(Rating rating)
    {
        var studentId = GetCurrentStudentId();

        // Проверяем, может ли студент оценить этого преподавателя
        var canRate = await _studentService.CanStudentRateTeacherAsync(studentId, rating.TeacherId);
        if (!canRate)
            return BadRequest("Student cannot rate this teacher");

        try
        {
            var createdRating = await _ratingService.CreateRatingAsync(rating, studentId);
            return CreatedAtAction(nameof(GetRating), new { id = createdRating.Id }, createdRating);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    // PUT: api/Ratings/5
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> PutRating(int id, Rating rating)
    {
        if (id != rating.Id)
            return BadRequest();

        var studentId = GetCurrentStudentId();

        try
        {
            await _ratingService.UpdateRatingAsync(id, rating, studentId);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }

        return NoContent();
    }

    // DELETE: api/Ratings/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteRating(int id)
    {
        var studentId = GetCurrentStudentId();

        try
        {
            var result = await _ratingService.DeleteRatingAsync(id, studentId);
            if (!result)
                return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }

        return NoContent();
    }

    // GET: api/Ratings/high-rated
    [HttpGet("high-rated")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<object>>> GetHighRated()
    {
        var studentId = GetCurrentStudentId();
        var ratings = await _ratingService.GetRatingsByStudentAsync(studentId);

        var result = ratings
            .Where(r => r.Score >= 4)
            .Select(r => new
            {
                TeacherName = r.Teacher.GetFullName(),
                r.Score,
                r.Review,
                r.CreatedDate
            })
            .ToList();

        return Ok(result);
    }

    // Вспомогательный метод для получения studentId (заглушка для демо)
    private int GetCurrentStudentId()
    {
        // В реальном приложении здесь будет извлечение из JWT токена
        // Для демо возвращаем 1 (первый студент из seed данных)
        return 1;
    }
}