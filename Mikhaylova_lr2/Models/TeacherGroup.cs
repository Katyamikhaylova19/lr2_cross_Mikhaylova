using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Mikhaylova_lr2.Models;

public class TeacherGroup
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [Required]
    public int GroupId { get; set; }

    // Навигационные свойства
    [ForeignKey("TeacherId")]
    [JsonIgnore] public virtual Teacher Teacher { get; set; } = null!;

    [ForeignKey("GroupId")]
    [JsonIgnore] public virtual Group Group { get; set; } = null!;
}