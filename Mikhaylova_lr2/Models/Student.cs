using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Mikhaylova_lr2.Models;

public class Student
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

    [Required]
    public int GroupId { get; set; }

    // Навигационные свойства
    [ForeignKey("GroupId")]
    [JsonIgnore] public virtual Group Group { get; set; } = null!;
    [JsonIgnore]  public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    // Бизнес-методы
    public string GetFullName()
    {
        return $"{LastName} {FirstName} {MiddleName}".Trim();
    }

    public bool CanRateTeacher(Teacher teacher)
    {
        if (teacher == null || Group == null) return false;

        // Проверяем, преподает ли учитель в группе студента
        return teacher.TeacherGroups?.Any(tg => tg.GroupId == GroupId) ?? false;
    }

    public Rating? GetRatingForTeacher(int teacherId)
    {
        return Ratings?.FirstOrDefault(r => r.TeacherId == teacherId);
    }
}