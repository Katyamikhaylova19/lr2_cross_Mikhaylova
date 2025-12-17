using Microsoft.AspNetCore.Mvc;
using Mikhaylova_lr2.Models.Auth;
using Mikhaylova_lr2.Services;

namespace Mikhaylova_lr2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            var tokenResponse = await _authService.LoginAsync(login);
            if (tokenResponse == null)
            {
                return Unauthorized(new { message = "Неверное имя пользователя или пароль" });
            }

            // Если пользователь найден в базе, добавим информацию о студенте/преподавателе
            var user = await _authService.GetUserByUsernameAsync(login.Username);
            if (user != null)
            {
                tokenResponse.StudentId = user.StudentId;
                tokenResponse.TeacherId = user.TeacherId;
            }

            return Ok(tokenResponse);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel register)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _authService.UserExistsAsync(register.Username))
                return BadRequest(new { message = "Пользователь с таким именем уже существует" });

            var user = await _authService.RegisterAsync(register);
            if (user == null)
                return BadRequest(new { message = "Не удалось зарегистрировать пользователя" });

            // Создаем токен для автоматического входа
            var tokenResponse = new TokenResponse
            {
                Token = _authService.GenerateJwtToken(user.Username, user.Role),
                Expiration = DateTime.UtcNow.AddMinutes(30),
                Role = user.Role,
                Username = user.Username,
                StudentId = user.StudentId,
                TeacherId = user.TeacherId
            };

            return Ok(new
            {
                message = "Пользователь успешно зарегистрирован",
                user = new
                {
                    user.Id,
                    user.Username,
                    user.Role,
                    user.StudentId,
                    user.TeacherId
                },
                token = tokenResponse.Token
            });
        }

        [HttpGet("profile")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var username = User.Identity.Name;
            var user = await _authService.GetUserByUsernameAsync(username);

            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Role,
                user.StudentId,
                user.TeacherId,
                Student = user.Student != null ? new
                {
                    user.Student.Id,
                    user.Student.FullName,
                    user.Student.GroupNumber
                } : null,
                Teacher = user.Teacher != null ? new
                {
                    user.Teacher.Id,
                    user.Teacher.FullName
                } : null
            });
        }
    }
}
