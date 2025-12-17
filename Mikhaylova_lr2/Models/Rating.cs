using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Mikhaylova_lr2.Models
{
    public class Rating
    {
        public int Id { get; set; }
        [Required, Range(1, 5)] public int Stars { get; set; }
        [StringLength(1000)] public string? Review { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int TeacherId { get; set; }
        public int StudentId { get; set; }

        [JsonIgnore] public virtual Teacher Teacher { get; set; }
        [JsonIgnore] public virtual Student Student { get; set; }

        public string GetReviewerDisplayName()
        {
            if (IsAnonymous)
                return "Анонимный пользователь";

            return Student?.FullName ?? "Неизвестный студент";
        }

        public string GetStarsDisplay()
        {
            return new string('★', Stars) + new string('☆', 5 - Stars);
        }

        public bool IsValidForStudent(Student student)
        {
            if (student == null)
                return false;

            return student.CanRateTeacher(TeacherId);
        }

        public void SetAnonymous(bool isAnonymous)
        {
            IsAnonymous = isAnonymous;
        }
    }
}
