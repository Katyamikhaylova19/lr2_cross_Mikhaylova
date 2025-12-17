using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Mikhaylova_lr2.Models
{
    public class Student
    {
        public int Id { get; set; }
        [Required, StringLength(100)] public string LastName { get; set; }
        [Required, StringLength(100)] public string FirstName { get; set; }
        [StringLength(100)] public string? MiddleName { get; set; }

        [Required, StringLength(10), RegularExpression(@"^[А-Я]{2}-\d{2}-\d{2}$", ErrorMessage = "Формат группы: XX-00-00")]
        public string GroupNumber { get; set; }

        public string FullName => GetFullName();

        [JsonIgnore] public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        [JsonIgnore] public virtual ICollection<StudentTeacher> StudentTeachers { get; set; } = new List<StudentTeacher>();

        public string GetFullName()
        {
            return MiddleName != null
                ? $"{LastName} {FirstName} {MiddleName}"
                : $"{LastName} {FirstName}";
        }

        public bool CanRateTeacher(int teacherId)
        {
            return StudentTeachers?.Any(st => st.TeacherId == teacherId) ?? false;
        }

        public bool ValidateGroupNumber()
        {
            return Regex.IsMatch(GroupNumber, @"^[А-Я]{2}-\d{2}-\d{2}$");
        }
    }
}
