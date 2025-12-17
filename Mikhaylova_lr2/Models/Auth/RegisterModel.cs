using System.ComponentModel.DataAnnotations;

namespace Mikhaylova_lr2.Models.Auth
{
    public class RegisterModel
    {
        [Required, StringLength(50, MinimumLength = 3)] public string Username { get; set; }

        [Required, MinLength(6)] public string Password { get; set; }

        [Required] public string Role { get; set; } // "Admin", "User", "Student", "Teacher"

        public int? StudentId { get; set; }
        public int? TeacherId { get; set; }
    }
}
