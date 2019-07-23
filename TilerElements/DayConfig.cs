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
                if(!Preference.isNull)
                {
                    Preference.SundayLastTimeUpdated = value;
                }
                
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
                if (!Preference.isNull)
                {
                    Preference.SundayCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.SundayDawnCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.SundayMorningCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.SundayAfterNoonCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.SundayEveningCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.SundayNightCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.MondayLastTimeUpdated = value;
                }
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
                if (!Preference.isNull)
                {
                    Preference.MondayCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.MondayDawnCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.MondayMorningCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.MondayAfterNoonCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.MondayEveningCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.MondayNightCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.TuesdayLastTimeUpdated = value;
                }
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
                if (!Preference.isNull)
                {
                    Preference.TuesdayCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.TuesdayDawnCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.TuesdayMorningCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.TuesdayAfterNoonCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.TuesdayEveningCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.TuesdayNightCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.WednesdayLastTimeUpdated = value;
                }
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
                if (!Preference.isNull)
                {
                    Preference.WednesdayCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.WednesdayDawnCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.WednesdayMorningCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.WednesdayAfterNoonCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.WednesdayEveningCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.WednesdayNightCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.ThursdayLastTimeUpdated = value;
                }
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
                if (!Preference.isNull)
                {
                    Preference.ThursdayCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.ThursdayDawnCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.ThursdayMorningCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.ThursdayAfterNoonCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.ThursdayEveningCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.ThursdayNightCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.FridayLastTimeUpdated = value;
                }
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
                if (!Preference.isNull)
                {
                    Preference.FridayCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.FridayDawnCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.FridayMorningCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.FridayAfterNoonCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.FridayEveningCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.FridayNightCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.SaturdayLastTimeUpdated = value;
                }
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
                if (!Preference.isNull)
                {
                    Preference.SaturdayCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.SaturdayDawnCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.SaturdayMorningCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.SaturdayAfterNoonCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.SaturdayEveningCount = value;
                    Preference.updateConfigOrder();
                }
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
                if (!Preference.isNull)
                {
                    Preference.SaturdayNightCount = value;
                    Preference.updateConfigOrder();
                }
            }
        }
    }



}
