using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mikhaylova_lr2.Models;
using Mikhaylova_lr2.Services;

namespace Mikhaylova_lr2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        // GET: api/ratings
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllRatings()
        {
            var ratings = await _ratingService.GetAllRatingsAsync();
            var result = ratings.Select(r => new
            {
                r.Id,
                r.Stars,
                r.Review,
                r.IsAnonymous,
                ReviewerName = r.GetReviewerDisplayName(),
                r.CreatedAt,
                Teacher = new { r.Teacher.Id, r.Teacher.FullName },
                Student = new { r.Student.Id, r.Student.FullName }
            });
            return Ok(result);
        }

        // GET: api/ratings/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetRating(int id)
        {
            var rating = await _ratingService.GetRatingByIdAsync(id);
            if (rating == null)
                return NotFound();

            var result = new
            {
                rating.Id,
                rating.Stars,
                rating.Review,
                rating.IsAnonymous,
                ReviewerName = rating.GetReviewerDisplayName(),
                StarsDisplay = rating.GetStarsDisplay(),
                rating.CreatedAt,
                Teacher = new { rating.Teacher.Id, rating.Teacher.FullName },
                Student = new { rating.Student.Id, rating.Student.FullName, rating.Student.GroupNumber }
            };

            return Ok(result);
        }

        // POST: api/ratings
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRating([FromBody] Rating rating)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdRating = await _ratingService.CreateRatingAsync(rating);
                return CreatedAtAction(nameof(GetRating), new { id = createdRating.Id }, createdRating);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/ratings/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRating(int id, [FromBody] Rating rating)
        {
            if (id != rating.Id)
                return BadRequest();

            try
            {
                await _ratingService.UpdateRatingAsync(rating);
            }
            catch
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/ratings/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRating(int id)
        {
            var rating = await _ratingService.GetRatingByIdAsync(id);
            if (rating == null)
                return NotFound();

            await _ratingService.DeleteRatingAsync(id);
            return NoContent();
        }

        // Дополнительные запросы с LINQ:

        // GET: api/ratings/by-teacher/5
        [HttpGet("by-teacher/{teacherId}")]
        [Authorize]
        public async Task<IActionResult> GetRatingsByTeacher(int teacherId)
        {
            var ratings = await _ratingService.GetRatingsByTeacherIdAsync(teacherId);
            var result = ratings.Select(r => new
            {
                r.Id,
                r.Stars,
                r.Review,
                r.IsAnonymous,
                ReviewerName = r.GetReviewerDisplayName(),
                r.CreatedAt,
                Student = new { r.Student.Id, r.Student.FullName, r.Student.GroupNumber }
            });
            return Ok(result);
        }

        // GET: api/ratings/recent/10
        [HttpGet("recent/{count}")]
        [Authorize]
        public async Task<IActionResult> GetRecentRatings(int count)
        {
            var ratings = await _ratingService.GetRecentRatingsAsync(count);
            return Ok(ratings);
        }

        // GET: api/ratings/anonymous
        [HttpGet("anonymous")]
        [Authorize]
        public async Task<IActionResult> GetAnonymousRatings()
        {
            var ratings = await _ratingService.GetAnonymousRatingsAsync();
            return Ok(ratings);
        }

        // GET: api/ratings/with-details
        [HttpGet("with-details")]
        [Authorize]
        public async Task<IActionResult> GetRatingsWithDetails()
        {
            var ratings = await _ratingService.GetRatingsWithDetailsAsync();
            return Ok(ratings);
        }
    }
}
