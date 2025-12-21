using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Mikhaylova_lr2.Models;

public class Rating
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Range(1, 5)]
    public int Score { get; set; }

    [MaxLength(1000)]
    public string? Review { get; set; }

    [Required]
    public bool IsAnonymous { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Внешние ключи
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    // Навигационные свойства
    [ForeignKey("StudentId")]
    [JsonIgnore] public virtual Student Student { get; set; } = null!;

    [ForeignKey("TeacherId")]
    [JsonIgnore] public virtual Teacher Teacher { get; set; } = null!;

    // Бизнес-методы
    public string GetReviewAuthor()
    {
        if (IsAnonymous)
            return "Аноним";

        return $"{Student?.LastName} {Student?.FirstName?.Substring(0, 1)}.";
    }

    public bool CanBeModifiedByStudent(int studentId)
    {
        return StudentId == studentId;
    }

    public void UpdateRating(int score, string? review, bool isAnonymous)
    {
        Score = score;
        Review = review;
        IsAnonymous = isAnonymous;
        CreatedDate = DateTime.UtcNow;
    }
}