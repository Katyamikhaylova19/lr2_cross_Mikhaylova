using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Mikhaylova_lr2.Models;

public class Group
{
    [Key]
    public int Id { get; set; }

    [Required]
    [RegularExpression(@"^[А-Я]{2}-\d{2}-\d{2}$")]
    public string GroupNumber { get; set; } = string.Empty;

    // Навигационные свойства
    [JsonIgnore] public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    [JsonIgnore] public virtual ICollection<TeacherGroup> TeacherGroups { get; set; } = new List<TeacherGroup>();

    // Бизнес-методы
    public int GetStudentsCount()
    {
        return Students?.Count ?? 0;
    }

    public int GetTeachersCount()
    {
        return TeacherGroups?.Count ?? 0;
    }
}