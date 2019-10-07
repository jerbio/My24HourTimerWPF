using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
    [XmlInclude(typeof(LocationReason))]
    [XmlInclude(typeof(DurationReason))]
    [XmlInclude(typeof(RestrictedEventReason))]
    [XmlInclude(typeof(NoReason))]
    [XmlInclude(typeof(DayOfWeekReason))]

    [Serializable]
    public abstract class Reason: IUndoable
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
            [XmlEnum(Name = "ProcrastinationSame")]
            ProcrastinationSame,
            [XmlEnum(Name = "SetAsNow")]
            SetAsNow,
            [XmlEnum(Name = "RestrictedEvent")]
            RestrictedEvent,
            [XmlEnum(Name = "DayOfWeek")]
            DayOfWeek
        }
        public enum AutoDeletion { RepetitionSpanTooSmall };
        protected Options _Option;
        public string _UndoId;
        protected string _Id = Guid.NewGuid().ToString();

        #region undoMembers
        public string UndoOption;
        #endregion
        virtual public Options Topic
        {
            get
            {
                return _Option;
            }
        }

        [NotMapped]
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

        virtual public string Option_DB
        {
            get
            {
                return _Option.ToString();
            }
            set
            {
                _Option = Utility.ParseEnum<Options>(value);
            }
        }

        public virtual bool FirstInstantiation { get; set; } = true;
        public virtual string UndoId
        {
            get
            {
                return _UndoId;
            }
            set
            {
                _UndoId = value;
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

        public virtual void undoUpdate(Undo undo)
        {
            this._UndoId = undo.id;
            this.FirstInstantiation = false;
        }
        public virtual void redo(string undoId)
        {
            if (this._UndoId == undoId)
            {
                Options optionsConverted = Utility.ParseEnum<Options>(UndoOption);
                Utility.Swap(ref _Option, ref optionsConverted);
                UndoOption = optionsConverted.ToString();
            }
        }

        public virtual void undo(string undoId)
        {
            if (this._UndoId == undoId)
            {
                Options optionsConverted = Utility.ParseEnum<Options>(UndoOption);
                Utility.Swap(ref _Option, ref optionsConverted);
                UndoOption = optionsConverted.ToString();
            }
        }
    }

    [Serializable]
    public class DayOfWeekReason : Reason
    {
        DayOfWeek WeekDay;
        DateTimeOffset DesiredDate;
        DateTimeOffset ReferenceTime;

        #region undoMembers
        public string UndoWeekDay;
        public DateTimeOffset UndoDesiredDate;
        public DateTimeOffset UndoReferenceTime;
        #endregion
        public override bool FirstInstantiation { get; set; } = true;

        protected DayOfWeekReason()
        {
            this._Option = Options.DayOfWeek;
        }
        public DayOfWeekReason(DayOfWeek dayOfWeek, DateTimeOffset referenceDate):this()
        {
            ReferenceTime = referenceDate;
            WeekDay = dayOfWeek;
            this.DesiredDate = getNextDateForDayOfWeek(dayOfWeek, referenceDate);
        }

        public SubCalendarEvent modifyEvent(SubCalendarEvent subEvent, DateTimeOffset date)
        {
            subEvent.shiftEvent(date, true);
            SubCalendarEvent retValue = subEvent;
            return retValue;
        }

        protected DateTimeOffset getNextDateForDayOfWeek(DayOfWeek dayOfeek, DateTimeOffset referenceTime)
        {
            DateTimeOffset retValue;

            if (referenceTime.DayOfWeek != dayOfeek)
            {
                int dayCount = ((int)referenceTime.DayOfWeek + 7);
                int dayDiff = dayCount - (int)dayOfeek;
                retValue = referenceTime.AddDays(dayDiff);
            }
            else
            {
                retValue = referenceTime;
                retValue = retValue.LocalDateTime;
            }

            return retValue;

        }

        public override void undoUpdate(Undo undo)
        {
            UndoDesiredDate = DesiredDate;
            UndoReferenceTime = ReferenceTime;
            UndoWeekDay = WeekDay.ToString();
            this._UndoId = undo.id;
        }

        public override void undo(string undoId)
        {
            if (this._UndoId == undoId)
            {
                Utility.Swap(ref UndoDesiredDate, ref DesiredDate);
                Utility.Swap(ref UndoReferenceTime, ref ReferenceTime);
                DayOfWeek weekdayConverted = Utility.ParseEnum<DayOfWeek>(UndoWeekDay);
                Utility.Swap(ref weekdayConverted, ref WeekDay);
                UndoWeekDay = weekdayConverted.ToString();
            }
        }

        public override void redo(string undoId)
        {
            if (this._UndoId == undoId)
            {
                Utility.Swap(ref UndoDesiredDate, ref DesiredDate);
                Utility.Swap(ref UndoReferenceTime, ref ReferenceTime);
                DayOfWeek weekdayConverted = Utility.ParseEnum<DayOfWeek>(UndoWeekDay);
                Utility.Swap(ref weekdayConverted, ref WeekDay);
                UndoWeekDay = weekdayConverted.ToString();
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
        public double Top;
        public double Bottom;
        public bool IsCelcius;
        public Bounds WeatherBounds;

        #region undoMembers
        public double UndoTop;
        public double UndoBottom;
        public bool UndoIsCelcius;
        public string UndoWeatherBounds;
        #endregion
        public override bool FirstInstantiation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public WeatherReason(Options weatherOption, Bounds weatherBounds , double top = 70, double bottom = 90, bool isCelcius = true)
        {
            this.Top = top;
            this.Bottom = bottom;
            this.IsCelcius = isCelcius;
            this.WeatherBounds = weatherBounds;
            this._Option = weatherOption;
        }

        public override void undoUpdate(Undo undo)
        {
            base.undoUpdate(undo);
            UndoTop = Top;
            UndoBottom = Bottom;
            UndoIsCelcius = IsCelcius;
            UndoWeatherBounds = WeatherBounds.ToString();
    }

        public override void undo(string undoId)
        {
            if (this._UndoId == undoId)
            {
                Utility.Swap(ref UndoTop, ref Top);
                Utility.Swap(ref UndoBottom, ref Bottom);
                Utility.Swap(ref UndoIsCelcius, ref IsCelcius);
                Bounds boundsConverted = Utility.ParseEnum<Bounds>(UndoWeatherBounds);
                Utility.Swap(ref boundsConverted, ref WeatherBounds);
                UndoWeatherBounds = boundsConverted.ToString();
            }
        }

        public override void redo(string undoId)
        {
            if (this._UndoId == undoId)
            {
                Utility.Swap(ref UndoTop, ref Top);
                Utility.Swap(ref UndoBottom, ref Bottom);
                Utility.Swap(ref UndoIsCelcius, ref IsCelcius);
                Bounds boundsConverted = Utility.ParseEnum<Bounds>(UndoWeatherBounds);
                Utility.Swap(ref boundsConverted, ref WeatherBounds);
                UndoWeatherBounds = boundsConverted.ToString();
            }
        }
    }

    public class InitialReason:Reason
    {
        public InitialReason()
        {
            this.Option = Options.Initial;
        }

        public override void redo(string undoId)
        {
            if (this._UndoId == undoId)
            {
                Options optionsConverted = Utility.ParseEnum<Options>(UndoOption);
                Utility.Swap(ref _Option, ref optionsConverted);
                UndoOption = optionsConverted.ToString();
            }
        }

        public override void undo(string undoId)
        {
            if (this._UndoId == undoId)
            {
                Options optionsConverted = Utility.ParseEnum<Options>(UndoOption);
                Utility.Swap(ref _Option, ref optionsConverted);
                UndoOption = optionsConverted.ToString();
            }
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

        [XmlIgnore]
        public TimeSpan UndoUsedUp;
        [XmlIgnore]
        public TimeSpan UndoAvailable;
        [XmlIgnore]
        public TimeSpan UndoCurrentUse;

        public TimeSpan UsedUp_DB
        {
            get
            {
                return _UsedUp;
            }
            set
            {
                _UsedUp = value;
            }
        }

        public TimeSpan Available_DB
        {
            get
            {
                return _Available;
            }
            set
            {
                _Available = value;
            }
        }

        public TimeSpan CurrentUse_DB
        {
            get
            {
                return _CurrentUse;
            }
            set
            {
                _CurrentUse = value;
            }
        }

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

        public override void undo(string undoId)
        {
            if (undoId == UndoId)
            {
                Utility.Swap(ref UndoUsedUp, ref _UsedUp);
                Utility.Swap(ref UndoAvailable, ref _Available);
                Utility.Swap(ref UndoCurrentUse, ref _CurrentUse);
            }
            base.undo(undoId);
        }

        public override void redo(string undoId)
        {
            if (undoId == UndoId)
            {
                Utility.Swap(ref UndoUsedUp, ref _UsedUp);
                Utility.Swap(ref UndoAvailable, ref _Available);
                Utility.Swap(ref UndoCurrentUse, ref _CurrentUse);
            }
            base.undo(undoId);
        }

        public override void undoUpdate(Undo undo)
        {
            UndoUsedUp = _UsedUp;
            UndoAvailable = _Available;
            UndoCurrentUse = _CurrentUse;
            base.undoUpdate(undo);
        }
    }

    [Serializable]
    public class DeadlineApproaching : Reason
    {
        DateTimeOffset _Deadline;
        public DateTimeOffset UndoDeadline;
        protected DeadlineApproaching()
        {

        }
        public DateTimeOffset Deadline
        {
            get
            {
                return _Deadline;
            }
            set
            {
                _Deadline = value;
            }
        }
        public DeadlineApproaching(DateTimeOffset deadLine)
        {
            this._Deadline = deadLine;
            this._Option = Options.DeadlineApproaching;
        }

        public override void undoUpdate(Undo undo)
        {
            UndoDeadline = _Deadline;
            base.undoUpdate(undo);
        }

        public override void undo(string undoId)
        {
            base.undo(undoId);
            if(undoId == _UndoId)
            {
                Utility.Swap(ref UndoDeadline, ref _Deadline);
            }
        }

        public override void redo(string undoId)
        {
            base.redo(undoId);
            if (undoId == _UndoId)
            {
                Utility.Swap(ref UndoDeadline, ref _Deadline);
            }
        }
    }

    [Serializable]
    public class ProcrastinationReason : Reason
    {
        public ProcrastinationReason(Procrastination oldProcrastination, Procrastination newProcrastination)
        {
            if (oldProcrastination.PreferredStartTime < newProcrastination.PreferredStartTime)
            {
                this._Option = Options.ProcrastinationIncrease;
            }
            else
            {
                if (oldProcrastination.PreferredStartTime == newProcrastination.PreferredStartTime)
                {
                    this._Option = Options.ProcrastinationSame;
                }
                else
                {
                    this._Option = Options.ProcrastinationDecrease;
                }
            }
        }
    }

        [Serializable]
    public class PreservedOrder : Reason
    {
        List<EventID> _IdOrders;
        protected PreservedOrder()
        {

        }

        public PreservedOrder(List<EventID> eventIds)
        {
            _IdOrders = eventIds.ToList();
            this._Option = Options.PreservedOrder;
        }

        public string IdOrders
        {
            get
            {
                string result = "";
                int lastCommaIndex = _IdOrders.Count - 1;
                for (int index = 0; index  < _IdOrders.Count; index++)
                {
                    string id = _IdOrders[index].ToString();
                    result += id;
                    if (lastCommaIndex != index)
                    {
                        result += ",";
                    }
                }
                return result;
            }
            set
            {
                _IdOrders = value.Split(',').Select((id) =>
                {
                    return new EventID(id);
                }).ToList();
            }
        }
    }

    [Serializable]
    public class DurationReason : Reason
    {
        TimeSpan _Duration;
        public TimeSpan UndoDuration;
        protected DurationReason()
        {

        }
        public DurationReason(TimeSpan duration)
        {
            this._Duration = duration;
            this._Option = Options.Occupancy;
        }

        public override void undoUpdate(Undo undo)
        {
            UndoDuration = _Duration;
            base.undoUpdate(undo);
        }

        public override void undo(string undoId)
        {
            base.undo(undoId);
            if (undoId == _UndoId)
            {
                Utility.Swap(ref UndoDuration, ref _Duration);
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return _Duration;
            }
            set
            {
                _Duration = value;
            }
        }

        public override void redo(string undoId)
        {
            base.redo(undoId);
            if (undoId == _UndoId)
            {
                Utility.Swap(ref UndoDuration, ref _Duration);
            }
        }
    }

    [Serializable]
    public class RestrictedEventReason : Reason
    {
        RestrictionProfile Profile;
        protected RestrictedEventReason()
        {

        }
        public RestrictedEventReason(RestrictionProfile restrictionProfile)
        {
            Profile = restrictionProfile;
            this._Option = Options.RestrictedEvent;
        }
    }
    [Serializable]
    public class LocationReason : Reason
    {
        List<Location> _LocationCluster;
        public LocationReason()
        {
            this._Option = Options.CloseToCluster;
        }
        public LocationReason(IEnumerable<Location> locations)
        {
            this._Option = Options.CloseToCluster;
            _LocationCluster = locations.Where(location=>location!=null).Where(location=> !location.isNull).ToList();
        }

        public LocationReason(Location location)
        {
            this._Option = Options.CloseToCluster;
            if (location != null)
            {
                _LocationCluster = new List<Location>() { location };
            }
        }

        //public ICollection<Location> LocationCluster
        //{
        //    get
        //    {
        //        return _LocationCluster;
        //    }
        //    set
        //    {
        //        _LocationCluster = value.ToList();
        //    }
        //}

        //[XmlAttribute(DataType = "Option")]
        override public Options Option {
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
