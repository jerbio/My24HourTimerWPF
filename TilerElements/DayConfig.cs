using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public abstract class DayConfig
    {
        string _Id { get; set; }
        public DateTimeOffset LastTimeUpdated { get; set; } = Utility.JSStartTime;
        public abstract double Count { get; set; }
        public abstract double DawnCount { get; set; }
        public abstract double MorningCount { get; set; }
        public abstract double AfterNoonCount { get; set; }
        public abstract double EveningCount { get; set; }
        public abstract double NightCount { get; set; }
        public EventPreference Preference { get; set; }

        public string Id
        {
            get
            {
                return _Id ?? (_Id = Guid.NewGuid().ToString());
            }
            set
            {
                _Id = value;
            }
        }
    }

    public class SundayConfig : DayConfig
    {
        public override double Count
        {
            get
            {
                return Preference.SundayCount;
            }
            set
            {
                Preference.SundayCount = value;
            }
        }
        public override double DawnCount
        {
            get
            {
                return Preference.SundayDawnCount;
            }
            set
            {
                Preference.SundayDawnCount = value;
            }
        }
        public override double MorningCount
        {
            get
            {
                return Preference.SundayMorningCount;
            }
            set
            {
                Preference.SundayMorningCount = value;
            }
        }
        public override double AfterNoonCount
        {
            get
            {
                return Preference.SundayAfterNoonCount;
            }
            set
            {
                Preference.SundayAfterNoonCount = value;
            }
        }
        public override double EveningCount
        {
            get
            {
                return Preference.SundayEveningCount;
            }
            set
            {
                Preference.SundayEveningCount = value;
            }
        }
        public override double NightCount
        {
            get
            {
                return Preference.SundayNightCount;
            }
            set
            {
                Preference.SundayNightCount = value;
            }
        }
    }

    public class MondayConfig : DayConfig
    {
        public override double Count
        {
            get
            {
                return Preference.MondayCount;
            }
            set
            {
                Preference.MondayCount = value;
            }
        }
        public override double DawnCount
        {
            get
            {
                return Preference.MondayDawnCount;
            }
            set
            {
                Preference.MondayDawnCount = value;
            }
        }
        public override double MorningCount
        {
            get
            {
                return Preference.MondayMorningCount;
            }
            set
            {
                Preference.MondayMorningCount = value;
            }
        }
        public override double AfterNoonCount
        {
            get
            {
                return Preference.MondayAfterNoonCount;
            }
            set
            {
                Preference.MondayAfterNoonCount = value;
            }
        }
        public override double EveningCount
        {
            get
            {
                return Preference.MondayEveningCount;
            }
            set
            {
                Preference.MondayEveningCount = value;
            }
        }
        public override double NightCount
        {
            get
            {
                return Preference.MondayNightCount;
            }
            set
            {
                Preference.MondayNightCount = value;
            }
        }
    }

    public class TuesdayConfig : DayConfig
    {
        public override double Count
        {
            get
            {
                return Preference.TuesdayCount;
            }
            set
            {
                Preference.TuesdayCount = value;
            }
        }
        public override double DawnCount
        {
            get
            {
                return Preference.TuesdayDawnCount;
            }
            set
            {
                Preference.TuesdayDawnCount = value;
            }
        }
        public override double MorningCount
        {
            get
            {
                return Preference.TuesdayMorningCount;
            }
            set
            {
                Preference.TuesdayMorningCount = value;
            }
        }
        public override double AfterNoonCount
        {
            get
            {
                return Preference.TuesdayAfterNoonCount;
            }
            set
            {
                Preference.TuesdayAfterNoonCount = value;
            }
        }
        public override double EveningCount
        {
            get
            {
                return Preference.TuesdayEveningCount;
            }
            set
            {
                Preference.TuesdayEveningCount = value;
            }
        }
        public override double NightCount
        {
            get
            {
                return Preference.TuesdayNightCount;
            }
            set
            {
                Preference.TuesdayNightCount = value;
            }
        }
    }

    public class WednesdayConfig : DayConfig
    {
        public override double Count
        {
            get
            {
                return Preference.WednesdayCount;
            }
            set
            {
                Preference.WednesdayCount = value;
            }
        }
        public override double DawnCount
        {
            get
            {
                return Preference.WednesdayDawnCount;
            }
            set
            {
                Preference.WednesdayDawnCount = value;
            }
        }
        public override double MorningCount
        {
            get
            {
                return Preference.WednesdayMorningCount;
            }
            set
            {
                Preference.WednesdayMorningCount = value;
            }
        }
        public override double AfterNoonCount
        {
            get
            {
                return Preference.WednesdayAfterNoonCount;
            }
            set
            {
                Preference.WednesdayAfterNoonCount = value;
            }
        }
        public override double EveningCount
        {
            get
            {
                return Preference.WednesdayEveningCount;
            }
            set
            {
                Preference.WednesdayEveningCount = value;
            }
        }
        public override double NightCount
        {
            get
            {
                return Preference.WednesdayNightCount;
            }
            set
            {
                Preference.WednesdayNightCount = value;
            }
        }
    }

    public class ThursdayConfig : DayConfig
    {
        public override double Count
        {
            get
            {
                return Preference.ThursdayCount;
            }
            set
            {
                Preference.ThursdayCount = value;
            }
        }
        public override double DawnCount
        {
            get
            {
                return Preference.ThursdayDawnCount;
            }
            set
            {
                Preference.ThursdayDawnCount = value;
            }
        }
        public override double MorningCount
        {
            get
            {
                return Preference.ThursdayMorningCount;
            }
            set
            {
                Preference.ThursdayMorningCount = value;
            }
        }
        public override double AfterNoonCount
        {
            get
            {
                return Preference.ThursdayAfterNoonCount;
            }
            set
            {
                Preference.ThursdayAfterNoonCount = value;
            }
        }
        public override double EveningCount
        {
            get
            {
                return Preference.ThursdayEveningCount;
            }
            set
            {
                Preference.ThursdayEveningCount = value;
            }
        }
        public override double NightCount
        {
            get
            {
                return Preference.ThursdayNightCount;
            }
            set
            {
                Preference.ThursdayNightCount = value;
            }
        }
    }

    public class FridayConfig : DayConfig
    {
        public override double Count
        {
            get
            {
                return Preference.FridayCount;
            }
            set
            {
                Preference.FridayCount = value;
            }
        }
        public override double DawnCount
        {
            get
            {
                return Preference.FridayDawnCount;
            }
            set
            {
                Preference.FridayDawnCount = value;
            }
        }
        public override double MorningCount
        {
            get
            {
                return Preference.FridayMorningCount;
            }
            set
            {
                Preference.FridayMorningCount = value;
            }
        }
        public override double AfterNoonCount
        {
            get
            {
                return Preference.FridayAfterNoonCount;
            }
            set
            {
                Preference.FridayAfterNoonCount = value;
            }
        }
        public override double EveningCount
        {
            get
            {
                return Preference.FridayEveningCount;
            }
            set
            {
                Preference.FridayEveningCount = value;
            }
        }
        public override double NightCount
        {
            get
            {
                return Preference.FridayNightCount;
            }
            set
            {
                Preference.FridayNightCount = value;
            }
        }
    }

    public class SaturdayConfig : DayConfig
    {
        public override double Count
        {
            get
            {
                return Preference.SaturdayCount;
            }
            set
            {
                Preference.SaturdayCount = value;
            }
        }
        public override double DawnCount
        {
            get
            {
                return Preference.SaturdayDawnCount;
            }
            set
            {
                Preference.SaturdayDawnCount = value;
            }
        }
        public override double MorningCount
        {
            get
            {
                return Preference.SaturdayMorningCount;
            }
            set
            {
                Preference.SaturdayMorningCount = value;
            }
        }
        public override double AfterNoonCount
        {
            get
            {
                return Preference.SaturdayAfterNoonCount;
            }
            set
            {
                Preference.SaturdayAfterNoonCount = value;
            }
        }
        public override double EveningCount
        {
            get
            {
                return Preference.SaturdayEveningCount;
            }
            set
            {
                Preference.SaturdayEveningCount = value;
            }
        }
        public override double NightCount
        {
            get
            {
                return Preference.SaturdayNightCount;
            }
            set
            {
                Preference.SaturdayNightCount = value;
            }
        }
    }



}
