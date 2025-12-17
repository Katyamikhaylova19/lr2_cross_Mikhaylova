using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Mikhaylova_lr2.Models.Auth;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mikhaylova_lr2.Services
{
    public interface IAuthService
    {
        Task<TokenResponse> LoginAsync(LoginModel login);
        Task<User> RegisterAsync(RegisterModel register);
        Task<bool> UserExistsAsync(string username);
        string GenerateJwtToken(string username, string role);
        Task<User?> GetUserByUsernameAsync(string username);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<TokenResponse> LoginAsync(LoginModel login)
        {
            var user = await GetUserByUsernameAsync(login.Username);

            // Проверяем хардкод пользователей или хеш из базы
            if (user == null)
            {
                // Проверяем стандартных пользователей (admin/user)
                if (login.Username == "admin" && login.Password == "admin123")
                {
                    var token = GenerateJwtToken("admin", "Admin");
                    return new TokenResponse
                    {
                        Token = token,
                        Expiration = DateTime.UtcNow.AddMinutes(30),
                        Role = "Admin",
                        Username = "admin"
                    };
                }
                else if (login.Username == "user" && login.Password == "user123")
                {
                    var token = GenerateJwtToken("user", "User");
                    return new TokenResponse
                    {
                        Token = token,
                        Expiration = DateTime.UtcNow.AddMinutes(30),
                        Role = "User",
                        Username = "user"
                    };
                }
                return null;
            }

            // Для пользователей из базы данных проверяем хеш пароля
            if (!VerifyPassword(login.Password, user.PasswordHash))
                return null;

            var jwtToken = GenerateJwtToken(user.Username, user.Role);
            return new TokenResponse
            {
                Token = jwtToken,
                Expiration = DateTime.UtcNow.AddMinutes(30),
                Role = user.Role,
                Username = user.Username
            };
        }

        public async Task<User> RegisterAsync(RegisterModel register)
        {
            if (await UserExistsAsync(register.Username))
                return null;

            var user = new User
            {
                Username = register.Username,
                PasswordHash = HashPassword(register.Password),
                Role = register.Role,
                StudentId = register.StudentId,
                TeacherId = register.TeacherId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public string GenerateJwtToken(string username, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        private string HashPassword(string password)
        {
            // В реальном приложении используйте BCrypt
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return HashPassword(password) == passwordHash;
        }
    }
}