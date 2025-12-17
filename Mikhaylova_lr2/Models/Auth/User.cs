using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Mikhaylova_lr2.Models.Auth
{
    public class User
    {
        public int Id { get; set; }
        [Required] public string Username { get; set; }
        [Required, JsonIgnore] public string PasswordHash { get; set; }
        [Required] public string Role { get; set; } // "Admin", "User", "Student", "Teacher"

        public int? StudentId { get; set; }
        public int? TeacherId { get; set; }

        [JsonIgnore] public virtual Student? Student { get; set; }
        [JsonIgnore] public virtual Teacher? Teacher { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
