using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    [NotMapped]
    public abstract class DayConfig
    {
        string _Id { get; set; }
        public abstract DateTimeOffset LastTimeUpdated { get; set; } 
        public abstract double Count { get; set; }
        public abstract double DawnCount { get; set; }
        public abstract double MorningCount { get; set; }
        public abstract double AfterNoonCount { get; set; }
        public abstract double EveningCount { get; set; }
        public abstract double NightCount { get; set; }
        public EventPreference Preference { get; set; }
        protected virtual DayOfWeek _WeekDay { get; set; }
        public virtual DayOfWeek WeekDay
        {
            get
            {
                return _WeekDay;
            }
        }

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
        override protected DayOfWeek _WeekDay { get; set; } = DayOfWeek.Sunday;
        public override DateTimeOffset LastTimeUpdated
        {
            get
            {
                return Preference.SundayLastTimeUpdated;
            }
            set
            {
                Preference.SundayLastTimeUpdated = value;
            }
        }

        public override double Count
        {
            get
            {
                return Preference.SundayCount;
            }
            set
            {
                Preference.SundayCount = value;
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
            }
        }
    }

    public class MondayConfig : DayConfig
    {
        override protected DayOfWeek _WeekDay { get; set; } = DayOfWeek.Monday;
        public override DateTimeOffset LastTimeUpdated
        {
            get
            {
                return Preference.MondayLastTimeUpdated;
            }
            set
            {
                Preference.MondayLastTimeUpdated = value;
            }
        }
        public override double Count
        {
            get
            {
                return Preference.MondayCount;
            }
            set
            {
                Preference.MondayCount = value;
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
            }
        }
    }

    public class TuesdayConfig : DayConfig
    {
        override protected DayOfWeek _WeekDay { get; set; } = DayOfWeek.Tuesday;
        public override DateTimeOffset LastTimeUpdated
        {
            get
            {
                return Preference.TuesdayLastTimeUpdated;
            }
            set
            {
                Preference.TuesdayLastTimeUpdated = value;
            }
        }
        public override double Count
        {
            get
            {
                return Preference.TuesdayCount;
            }
            set
            {
                Preference.TuesdayCount = value;
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
            }
        }
    }

    public class WednesdayConfig : DayConfig
    {
        override protected DayOfWeek _WeekDay { get; set; } = DayOfWeek.Wednesday;

        public override DateTimeOffset LastTimeUpdated
        {
            get
            {
                return Preference.WednesdayLastTimeUpdated;
            }
            set
            {
                Preference.WednesdayLastTimeUpdated = value;
            }
        }
        public override double Count
        {
            get
            {
                return Preference.WednesdayCount;
            }
            set
            {
                Preference.WednesdayCount = value;
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
            }
        }
    }

    public class ThursdayConfig : DayConfig
    {
        override protected DayOfWeek _WeekDay { get; set; } = DayOfWeek.Thursday;

        public override DateTimeOffset LastTimeUpdated
        {
            get
            {
                return Preference.ThursdayLastTimeUpdated;
            }
            set
            {
                Preference.ThursdayLastTimeUpdated = value;
            }
        }

        public override double Count
        {
            get
            {
                return Preference.ThursdayCount;
            }
            set
            {
                Preference.ThursdayCount = value;
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
            }
        }
    }

    public class FridayConfig : DayConfig
    {
        override protected DayOfWeek _WeekDay { get; set; } = DayOfWeek.Friday;
        public override DateTimeOffset LastTimeUpdated
        {
            get
            {
                return Preference.FridayLastTimeUpdated;
            }
            set
            {
                Preference.FridayLastTimeUpdated = value;
            }
        }
        public override double Count
        {
            get
            {
                return Preference.FridayCount;
            }
            set
            {
                Preference.FridayCount = value;
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
            }
        }
    }

    public class SaturdayConfig : DayConfig
    {
        override protected DayOfWeek _WeekDay { get; set; } = DayOfWeek.Saturday;
        public override DateTimeOffset LastTimeUpdated
        {
            get
            {
                return Preference.SaturdayLastTimeUpdated;
            }
            set
            {
                Preference.SaturdayLastTimeUpdated = value;
            }
        }

        public override double Count
        {
            get
            {
                return Preference.SaturdayCount;
            }
            set
            {
                Preference.SaturdayCount = value;
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
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
				Preference.updateConfigOrder();
            }
        }
    }



}
