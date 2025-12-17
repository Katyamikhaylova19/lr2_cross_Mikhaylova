using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Mikhaylova_lr2.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        [Required, StringLength(100)] public string LastName { get; set; }
        [Required, StringLength(100)] public string FirstName { get; set; }
        [StringLength(100)] public string? MiddleName { get; set; }

        public string FullName => GetFullName();

        [JsonIgnore] public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        [JsonIgnore] public virtual ICollection<StudentTeacher> StudentTeachers { get; set; } = new List<StudentTeacher>();

        public string GetFullName()
        {
            return MiddleName != null
                ? $"{LastName} {FirstName} {MiddleName}"
                : $"{LastName} {FirstName}";
        }

        public double CalculateAverageRating()
        {
            if (Ratings == null || !Ratings.Any())
                return 0;

            return Ratings.Average(r => r.Stars);
        }

        public int GetRatingsCount()
        {
            return Ratings?.Count ?? 0;
        }
    }
}
