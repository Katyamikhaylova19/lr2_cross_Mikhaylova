using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Mikhaylova_lr2.Models;

public class Teacher
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? MiddleName { get; set; }

    // Навигационные свойства
    [JsonIgnore] public virtual ICollection<TeacherGroup> TeacherGroups { get; set; } = new List<TeacherGroup>();
    [JsonIgnore] public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    // Бизнес-методы
    public string GetFullName()
    {
        return $"{LastName} {FirstName} {MiddleName}".Trim();
    }

    public double CalculateAverageRating()
    {
        if (Ratings == null || !Ratings.Any())
            return 0;

        return Ratings.Average(r => r.Score);
    }

    public bool HasRatingFromStudent(int studentId)
    {
        return Ratings?.Any(r => r.StudentId == studentId) ?? false;
    }

    public IEnumerable<string> GetTeachingGroups()
    {
        return TeacherGroups?.Select(tg => tg.Group?.GroupNumber ?? "")
               ?? Enumerable.Empty<string>();
    }
}