using Microsoft.AspNetCore.Mvc;
using Mikhaylova_lr2.Models.Auth;
using Mikhaylova_lr2.Services;

namespace Mikhaylova_lr2.Controllers;

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
    public IActionResult Login([FromBody] LoginModel model)
    {
        var response = _authService.Authenticate(model.Username, model.Password);

        if (response == null)
            return Unauthorized(new { message = "Invalid username or password" });

        return Ok(response);
    }
}