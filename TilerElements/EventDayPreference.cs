using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class EventDayPreference
    {
        public DayPrefrence Sunday { get; set; }
        public DayPrefrence Monday { get; set; }
        public DayPrefrence Tuesday { get; set; }
        public DayPrefrence Wednesday { get; set; }
        public DayPrefrence Thursday { get; set; }
        public DayPrefrence Friday { get; set; }
        public DayPrefrence Saturday { get; set; }
    }

    public class DayPrefrence
    {
        public DateTimeOffset LastTimeUpdated { get; set; } = Utility.JSStartTime;
        public double Count { get; set; } = 0;
        public double DawnCount { get; set; } = 0;
        public double MorningCount { get; set; } = 0;
        public double AfterNoonCount { get; set; } = 0;
        public double EveningCount { get; set; } = 0;
        public double NightCount { get; set; } = 0;
    }
}
