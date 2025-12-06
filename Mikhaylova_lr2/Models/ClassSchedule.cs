namespace Mikhaylova_lr2.Models
{
    public class ClassSchedule
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
        public DateTime Date { get; set; }
        public int DayOfWeek { get; set; } // 1-7 (Понедельник-Воскресенье)
        public int PairNumber { get; set; } // 1-7
        public string Classroom { get; set; }
        public string GroupNumber { get; set; } // Формат XX-00-00
        public string ClassType { get; set; } // лекция/семинар/лабораторная/экзамен/консультация
        public string TeacherName { get; set; }

        public bool IsValidPairNumber()
        {
            return PairNumber >= 1 && PairNumber <= 7;
        }

        public bool IsValidGroupNumber()
        {
            if (string.IsNullOrWhiteSpace(GroupNumber))
                return false;

            var parts = GroupNumber.Split('-');
            return parts.Length == 3
                && parts[0].Length == 2
                && parts[1].Length == 2
                && parts[2].Length == 2;
        }

        public string GetPairTime()
        {
            var times = new Dictionary<int, string>
            {
                {1, "08:30 - 10:00"},
                {2, "10:15 - 11:45"},
                {3, "12:00 - 13:30"},
                {4, "14:00 - 15:30"},
                {5, "15:45 - 17:15"},
                {6, "17:30 - 18:45"},
                {7, "19:00 - 20:30"}
            };

            return times.ContainsKey(PairNumber) ? times[PairNumber] : "Время не определено";
        }

        public bool IsValid()
        {
            return IsValidPairNumber()
                && IsValidGroupNumber()
                && !string.IsNullOrWhiteSpace(Classroom)
                && !string.IsNullOrWhiteSpace(TeacherName)
                && SubjectId > 0;
        }
    }
}
