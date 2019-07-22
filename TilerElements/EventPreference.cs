using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TilerElements
{
    [Serializable]
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

        [XmlIgnore]
        DayConfig _SundayPreference;
        [XmlIgnore]
        DayConfig _MondayPreference;
        [XmlIgnore]
        DayConfig _TuesdayPreference;
        [XmlIgnore]
        DayConfig _WednesdayPreference;
        [XmlIgnore]
        DayConfig _ThursdayPreference;
        [XmlIgnore]
        DayConfig _FridayPreference;
        [XmlIgnore]
        DayConfig _SaturdayPreference;
        protected List<DayConfig> _DayConfigs;
        protected List<DayConfig> _OrderedDayConfigs;

        public void init ()
        {
            _SundayPreference = new SundayConfig() { Preference = this };
            _MondayPreference = new MondayConfig() { Preference = this };
            _TuesdayPreference = new TuesdayConfig() { Preference = this };
            _WednesdayPreference = new WednesdayConfig() { Preference = this };
            _ThursdayPreference = new ThursdayConfig() { Preference = this };
            _FridayPreference = new FridayConfig() { Preference = this };
            _SaturdayPreference = new SaturdayConfig() { Preference = this };

            _DayConfigs = new List<DayConfig>()
            {
                _SundayPreference, _MondayPreference, _TuesdayPreference, _WednesdayPreference, _ThursdayPreference, _FridayPreference, _SaturdayPreference
            };

            updateConfigOrder();
        }

        public EventPreference createCopy(string eventId = null)
        {
            EventPreference retValue = new EventPreference();
            retValue.SundayLastTimeUpdated = this.SundayLastTimeUpdated;
            retValue.SundayCount = this.SundayCount;
            retValue.SundayDawnCount = this.SundayDawnCount;
            retValue.SundayMorningCount = this.SundayMorningCount;
            retValue.SundayAfterNoonCount = this.SundayAfterNoonCount;
            retValue.SundayEveningCount = this.SundayEveningCount;
            retValue.SundayNightCount = this.SundayNightCount;
            retValue.MondayLastTimeUpdated = this.MondayLastTimeUpdated;
            retValue.MondayCount = this.MondayCount;
            retValue.MondayDawnCount = this.MondayDawnCount;
            retValue.MondayMorningCount = this.MondayMorningCount;
            retValue.MondayAfterNoonCount = this.MondayAfterNoonCount;
            retValue.MondayEveningCount = this.MondayEveningCount;
            retValue.MondayNightCount = this.MondayNightCount;
            retValue.TuesdayLastTimeUpdated = this.TuesdayLastTimeUpdated;
            retValue.TuesdayCount = this.TuesdayCount;
            retValue.TuesdayDawnCount = this.TuesdayDawnCount;
            retValue.TuesdayMorningCount = this.TuesdayMorningCount;
            retValue.TuesdayAfterNoonCount = this.TuesdayAfterNoonCount;
            retValue.TuesdayEveningCount = this.TuesdayEveningCount;
            retValue.TuesdayNightCount = this.TuesdayNightCount;
            retValue.WednesdayLastTimeUpdated = this.WednesdayLastTimeUpdated;
            retValue.WednesdayCount = this.WednesdayCount;
            retValue.WednesdayDawnCount = this.WednesdayDawnCount;
            retValue.WednesdayMorningCount = this.WednesdayMorningCount;
            retValue.WednesdayAfterNoonCount = this.WednesdayAfterNoonCount;
            retValue.WednesdayEveningCount = this.WednesdayEveningCount;
            retValue.WednesdayNightCount = this.WednesdayNightCount;
            retValue.ThursdayLastTimeUpdated = this.ThursdayLastTimeUpdated;
            retValue.ThursdayCount = this.ThursdayCount;
            retValue.ThursdayDawnCount = this.ThursdayDawnCount;
            retValue.ThursdayMorningCount = this.ThursdayMorningCount;
            retValue.ThursdayAfterNoonCount = this.ThursdayAfterNoonCount;
            retValue.ThursdayEveningCount = this.ThursdayEveningCount;
            retValue.ThursdayNightCount = this.ThursdayNightCount;
            retValue.FridayLastTimeUpdated = this.FridayLastTimeUpdated;
            retValue.FridayCount = this.FridayCount;
            retValue.FridayDawnCount = this.FridayDawnCount;
            retValue.FridayMorningCount = this.FridayMorningCount;
            retValue.FridayAfterNoonCount = this.FridayAfterNoonCount;
            retValue.FridayEveningCount = this.FridayEveningCount;
            retValue.FridayNightCount = this.FridayNightCount;
            retValue.SaturdayLastTimeUpdated = this.SaturdayLastTimeUpdated;
            retValue.SaturdayCount = this.SaturdayCount;
            retValue.SaturdayDawnCount = this.SaturdayDawnCount;
            retValue.SaturdayMorningCount = this.SaturdayMorningCount;
            retValue.SaturdayAfterNoonCount = this.SaturdayAfterNoonCount;
            retValue.SaturdayEveningCount = this.SaturdayEveningCount;
            retValue.SaturdayNightCount = this.SaturdayNightCount;
            retValue.init();
            return retValue;
        }

        public void updateConfigOrder()
        {
            _OrderedDayConfigs = _DayConfigs.OrderByDescending(dayConfig => dayConfig.Count).ToList();
        }

        public DayConfig this[int i]
        {
            get
            {
                if(_DayConfigs ==null)
                {
                    init();
                }
                return _DayConfigs[i];
            }
        }
        [NotMapped, XmlIgnore]
        public List<DayConfig> DayConfigs
        {
            get
            {
                return _DayConfigs;
            }
        }
        [XmlIgnore]
        public List<DayConfig> OrderedDayConfigs
        {
            get
            {
                return _OrderedDayConfigs;
            }
        }
        [XmlIgnore]
        public DayConfig this[DayOfWeek dayOfWeek]
        {
            get
            {
                if (_DayConfigs == null)
                {
                    init();
                }
                return _DayConfigs[(int)dayOfWeek];
            }
        }
        [XmlIgnore]
        DayConfig SundayPreference
        {
            get { return _SundayPreference; }
        }
        [XmlIgnore]
        DayConfig MondayPreference
        {
            get { return _MondayPreference; }
        }
        [XmlIgnore]
        DayConfig TuesdayPreference {
            get { return _TuesdayPreference; }
        }
        [XmlIgnore]
        DayConfig WednesdayPreference { get
            { return _WednesdayPreference; }
        }
        [XmlIgnore]
        DayConfig ThursdayPreference{
            get { return _ThursdayPreference; }
        }
        [XmlIgnore]
        DayConfig FridayPreference {
            get { return _FridayPreference; }
        }
        [XmlIgnore]
        DayConfig SaturdayPreference {
            get { return _SaturdayPreference; }
        }
    }

}
