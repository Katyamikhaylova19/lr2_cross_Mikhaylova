using System.Text.Json.Serialization;

namespace Mikhaylova_lr2.Models
{
    public class StudentTeacher
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int TeacherId { get; set; }

        [JsonIgnore] public virtual Student Student { get; set; }
        [JsonIgnore] public virtual Teacher Teacher { get; set; }
    }
}
