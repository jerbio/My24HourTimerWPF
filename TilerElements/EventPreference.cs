using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class EventPreference
    {
        string _Id { get; set; }
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
        public DateTimeOffset SundayLastTimeUpdated { get; set; } = Utility.JSStartTime;
        public double SundayCount { get; set; } = 0;
        public double SundayDawnCount { get; set; } = 0;
        public double SundayMorningCount { get; set; } = 0;
        public double SundayAfterNoonCount { get; set; } = 0;
        public double SundayEveningCount { get; set; } = 0;
        public double SundayNightCount { get; set; } = 0;
        public DateTimeOffset MondayLastTimeUpdated { get; set; } = Utility.JSStartTime;
        public double MondayCount { get; set; } = 0;
        public double MondayDawnCount { get; set; } = 0;
        public double MondayMorningCount { get; set; } = 0;
        public double MondayAfterNoonCount { get; set; } = 0;
        public double MondayEveningCount { get; set; } = 0;
        public double MondayNightCount { get; set; } = 0;
        public DateTimeOffset TuesdayLastTimeUpdated { get; set; } = Utility.JSStartTime;
        public double TuesdayCount { get; set; } = 0;
        public double TuesdayDawnCount { get; set; } = 0;
        public double TuesdayMorningCount { get; set; } = 0;
        public double TuesdayAfterNoonCount { get; set; } = 0;
        public double TuesdayEveningCount { get; set; } = 0;
        public double TuesdayNightCount { get; set; } = 0;
        public DateTimeOffset WednesdayLastTimeUpdated { get; set; } = Utility.JSStartTime;
        public double WednesdayCount { get; set; } = 0;
        public double WednesdayDawnCount { get; set; } = 0;
        public double WednesdayMorningCount { get; set; } = 0;
        public double WednesdayAfterNoonCount { get; set; } = 0;
        public double WednesdayEveningCount { get; set; } = 0;
        public double WednesdayNightCount { get; set; } = 0;
        public DateTimeOffset ThursdayLastTimeUpdated { get; set; } = Utility.JSStartTime;
        public double ThursdayCount { get; set; } = 0;
        public double ThursdayDawnCount { get; set; } = 0;
        public double ThursdayMorningCount { get; set; } = 0;
        public double ThursdayAfterNoonCount { get; set; } = 0;
        public double ThursdayEveningCount { get; set; } = 0;
        public double ThursdayNightCount { get; set; } = 0;
        public DateTimeOffset FridayLastTimeUpdated { get; set; } = Utility.JSStartTime;
        public double FridayCount { get; set; } = 0;
        public double FridayDawnCount { get; set; } = 0;
        public double FridayMorningCount { get; set; } = 0;
        public double FridayAfterNoonCount { get; set; } = 0;
        public double FridayEveningCount { get; set; } = 0;
        public double FridayNightCount { get; set; } = 0;
        public DateTimeOffset SaturdayLastTimeUpdated { get; set; } = Utility.JSStartTime;
        public double SaturdayCount { get; set; } = 0;
        public double SaturdayDawnCount { get; set; } = 0;
        public double SaturdayMorningCount { get; set; } = 0;
        public double SaturdayAfterNoonCount { get; set; } = 0;
        public double SaturdayEveningCount { get; set; } = 0;
        public double SaturdayNightCount { get; set; } = 0;

        DayConfig _SundayPreference;
        DayConfig _MondayPreference;
        DayConfig _TuesdayPreference;
        DayConfig _WednesdayPreference;
        DayConfig _ThursdayPreference;
        DayConfig _FridayPreference;
        DayConfig _SaturdayPreference;
        public List<DayConfig> DayConfigs;

        public void init ()
        {
            _SundayPreference = new SundayConfig();
            _MondayPreference = new MondayConfig();
            _TuesdayPreference = new TuesdayConfig();
            _WednesdayPreference = new WednesdayConfig();
            _ThursdayPreference = new ThursdayConfig();
            _FridayPreference = new FridayConfig();
            _SaturdayPreference = new SaturdayConfig();

            DayConfigs = new List<DayConfig>()
            {
                _SundayPreference, _MondayPreference, _TuesdayPreference, _WednesdayPreference, _ThursdayPreference, _FridayPreference, _SaturdayPreference
            };
        }

        public DayConfig this[int i]
        {
            get
            {
                return DayConfigs[i];
            }
        }

        public DayConfig this[DayOfWeek dayOfWeek]
        {
            get
            {
                return DayConfigs[(int)dayOfWeek];
            }
        }

        DayConfig SundayPreference
        {
            get { return _SundayPreference; }
        }
        DayConfig MondayPreference
        {
            get { return _MondayPreference; }
        }
        DayConfig TuesdayPreference {
            get { return _TuesdayPreference; }
        }
        DayConfig WednesdayPreference { get
            { return _WednesdayPreference; }
        }
        DayConfig ThursdayPreference{
            get { return _ThursdayPreference; }
        }
        DayConfig FridayPreference {
            get { return _FridayPreference; }
        }
        DayConfig SaturdayPreference {
            get { return _SaturdayPreference; }
        }
    }

}
