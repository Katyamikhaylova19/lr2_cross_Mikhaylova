namespace Mikhaylova_lr2.Models
{
    public class WeeklySchedule
    {
        public int Id { get; set; }
        public string GroupNumber { get; set; }
        public int WeekNumber { get; set; } // Номер недели в семестре
        public List<ClassSchedule> Classes { get; set; } = new List<ClassSchedule>();

        public List<ClassSchedule> GetScheduleForDay(int dayOfWeek)
        {
            return Classes.Where(c => c.DayOfWeek == dayOfWeek)
                         .OrderBy(c => c.PairNumber)
                         .ToList();
        }

        public bool IsDailyScheduleValid()
        {
            for (int day = 1; day <= 7; day++)
            {
                var dayClasses = GetScheduleForDay(day);
                if (dayClasses.Count > 7)
                    return false;
            }
            return true;
        }

        public List<string> GetAllTeachers()
        {
            return Classes.Select(c => c.TeacherName)
                         .Distinct()
                         .ToList();
        }

        public Dictionary<string, int> GetClassTypesCount()
        {
            return Classes.GroupBy(c => c.ClassType)
                         .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
