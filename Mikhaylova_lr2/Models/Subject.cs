namespace Mikhaylova_lr2.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public bool IsValidName()
        {
            return !string.IsNullOrWhiteSpace(Name) && Name.Length >= 3 && Name.Length <= 100;
        }

        public string GetInfo()
        {
            return $"{Id}: {Name}";
        }
    }

}
