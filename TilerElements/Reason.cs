using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace TilerElements
{
    [XmlInclude(typeof(WeatherReason))]
    [XmlInclude(typeof(BestFitReason))]
    [XmlInclude(typeof(DeadlineApproaching))]
    [XmlInclude(typeof(PreservedOrder))]
    [Serializable]
    public abstract class Reason
    {
        public enum Options {
            [XmlEnum(Name = "None")]
            None,
            [XmlEnum(Name = "Initial")]
            Initial,
            [XmlEnum(Name = "PreservedOrder")]
            PreservedOrder,
            [XmlEnum(Name = "BestFit")]
            BestFit,
            [XmlEnum(Name = "HumidWeather")]
            HumidWeather,
            [XmlEnum(Name = "ColdWeather")]
            ColdWeather,
            [XmlEnum(Name = "WarmWeather")]
            WarmWeather,
            [XmlEnum(Name = "Weather")]
            Weather,
            [XmlEnum(Name = "CompletionRate")]
            CompletionRate,
            [XmlEnum(Name = "CompletionLevel")]
            CompletionLevel,
            [XmlEnum(Name = "DeletionRate")]
            DeletionRate,
            [XmlEnum(Name = "DeletionLevel")]
            DeletionLevel,
            [XmlEnum(Name = "Occupancy")]
            Occupancy,
            [XmlEnum(Name = "DeadlineApproaching")]
            DeadlineApproaching,
            [XmlEnum(Name = "HighExhaustion")]
            HighExhaustion,
            [XmlEnum(Name = "LowExhaustion")]
            LowExhaustion,
            [XmlEnum(Name = "Exhaustion")]
            Exhaustion,
            [XmlEnum(Name = "CloseToCluster")]
            CloseToCluster,
            [XmlEnum(Name = "Far")]
            Far,
            [XmlEnum(Name = "ReduceTransitTime")]
            ReduceTransitTime,
            [XmlEnum(Name = "IncreaseTransitTime")]
            IncreaseTransitTime,
            [XmlEnum(Name = "SimilarActivity")]
            SimilarActivity,
            [XmlEnum(Name = "WillConflict")]
            WillConflict,
            [XmlEnum(Name = "AvoidConflict")]
            AvoidConflict,
            [XmlEnum(Name = "ProcrastinationIncrease")]
            ProcrastinationIncrease,
            [XmlEnum(Name = "ProcrastinationDecrease")]
            ProcrastinationDecrease,
            [XmlEnum(Name = "SetAsNow")]
            SetAsNow,
            [XmlEnum(Name = "RestrictedEvent")]
            RestrictedEvent
        }
        protected Options _Option;

        virtual public Options Topic
        {
            get
            {
                return _Option;
            }
        }
        //[XmlAttribute(DataType = "Option")]
        virtual public Options Option {
            get
            {
                return _Option;
            }
            set
            {
                _Option = value;
            }
        }

    }

    [Serializable]
    public class WeatherReason : Reason
    {
        public enum Bounds
        {
            TooMuch,
            LessThan,
            WithinRange,
            None
        }
        public WeatherReason()
        {
            this._Option = Options.Weather;
        }
        public double Top { get; set; }
        public double Bottom { get; set; }
        public bool IsCelcius { get; set; }
        public Bounds WeatherBounds { get; set; }

        public WeatherReason(Options weatherOption, Bounds weatherBounds , double top = 70, double bottom = 90, bool isCelcius = true)
        {
            this.Top = top;
            this.Bottom = bottom;
            this.IsCelcius = isCelcius;
            this.WeatherBounds = weatherBounds;
            this._Option = weatherOption;
        }
    }

    public class InitialReason:Reason
    {
        public InitialReason()
        {
            this.Option = Options.Initial;
        }
    }

    [Serializable]
    public class BestFitReason : Reason
    {
        public BestFitReason()
        {
            this._Option = Options.BestFit;
        }
        [XmlIgnore]
        protected TimeSpan _UsedUp;
        [XmlIgnore]
        protected TimeSpan _Available;
        [XmlIgnore]
        protected TimeSpan _CurrentUse;

        //[XmlAttribute(DataType = "UsedUp")]
        public string UsedUp { get { return XmlConvert.ToString(_UsedUp); } set { _UsedUp = String.IsNullOrEmpty(value) ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value); } }
        //[XmlAttribute(DataType = "Available")]
        public string Available { get { return XmlConvert.ToString(_Available); } set { _Available = String.IsNullOrEmpty(value) ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value); } }
        //[XmlAttribute(DataType = "CurrentUse")]
        public string CurrentUse { get { return XmlConvert.ToString(_CurrentUse); } set { _CurrentUse = String.IsNullOrEmpty(value) ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value); } }
        public BestFitReason(TimeSpan usedUp, TimeSpan available, TimeSpan currentUse)
        {
            this._Option = Options.BestFit;
            this._UsedUp = usedUp;
            this._Available = available;
            this._CurrentUse = currentUse;
        }
    }

    [Serializable]
    public class DeadlineApproaching : Reason
    {
        public DeadlineApproaching()
        {
            this._Option = Options.DeadlineApproaching;
        }
    }

    [Serializable]
    public class PreservedOrder : Reason
    {
        public PreservedOrder()
        {
            this._Option = Options.PreservedOrder;
        }
    }

    [Serializable]
    public class DurationReason : Reason
    {
        public DurationReason()
        {
            this._Option = Options.Occupancy;
        }
    }

    [Serializable]
    public class RestrictedEventReason : Reason
    {
        public RestrictedEventReason()
        {
            this._Option = Options.RestrictedEvent;
        }
    }
    [Serializable]
    public class LocationReason : Reason
    {
        List<Location_Elements> _LocationCluster;
        public LocationReason(IEnumerable<Location_Elements> locations)
        {
            this._Option = Options.CloseToCluster;
            _LocationCluster = locations.Where(location=>location!=null).Where(location=> !location.isNull).ToList();
        }

        public LocationReason(Location_Elements location)
        {
            this._Option = Options.CloseToCluster;
            if (location != null)
            {
                _LocationCluster = new List<Location_Elements>() { location };
            }
        }

        public List<Location_Elements> LocationCluster
        {
            get
            {
                return _LocationCluster;
            }
            set
            {
                _LocationCluster = value;
            }
        }
    }

    [Serializable]
    public class NoReason : Reason
    {
        static NoReason NoReasonFactoryObject;
        public static NoReason getNoReasonInstanceFactory()
        {
            if (NoReasonFactoryObject == null)
            {
                NoReasonFactoryObject = new NoReasonFactory();
            }

            return NoReasonFactoryObject;
        }
        [Serializable]
        class NoReasonFactory : NoReason
        {
            public NoReasonFactory()
            {
                this._Option = Options.None;
            }
        }

    }

}
