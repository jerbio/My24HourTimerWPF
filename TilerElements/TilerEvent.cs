using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public abstract class TilerEvent: IWhy, IUndoable, IHasId
    {
        public static TimeSpan ZeroTimeSpan = new TimeSpan(0);
        protected DateTimeOffset StartDateTime;
        protected DateTimeOffset EndDateTime;
        protected bool _Complete = false;
        protected bool _Enabled = true;
        protected bool _UserDeleted = false;
        protected Location _LocationInfo = Location.getNullLocation();
        protected EventDisplay _UiParams;
        protected MiscData _DataBlob;
        protected Repetition _EventRepetition;
        protected bool RigidSchedule;
        protected TimeSpan _EventDuration;
        protected string _otherPartyID;
        protected TimeSpan _EventPreDeadline;
        protected TimeSpan _PrepTime;
        protected EventID UniqueID;
        protected int _Priority;
        protected bool isRestricted = false;
        protected static DateTimeOffset EventNow = DateTimeOffset.UtcNow;
        protected static TimeSpan CalculationEndSpan = new TimeSpan(180, 0, 0, 0, 0);
        protected Procrastination _ProfileOfProcrastination;
        protected NowProfile _ProfileOfNow;
        protected bool _ThirdPartyFlag = false;
        protected string ThirdPartyUserIDInfo;
        protected ThirdPartyControl.CalendarTool ThirdPartyTypeInfo = ThirdPartyControl.CalendarTool.tiler;
        protected TilerUser _Creator;
        protected TimeSpan _UsedTime = new TimeSpan();
        protected Classification _Semantics;
        protected TimeOfDayPreferrence DaySectionPreference;
        protected TilerUserGroup _Users;
        protected string _TimeZone = "UTC";
        protected bool _isProcrastinateEvent = false;
        internal TempTilerEventChanges TempChanges = new TempTilerEventChanges();
        protected EventName _Name;
        protected string _UndoId;
        protected bool _IsRepeat = false;

        #region undoParameters
        public DateTimeOffset UndoStartDateTime;
        public DateTimeOffset UndoEndDateTime;
        public bool UndoComplete = false;
        public bool UndoEnabled = true;
        public bool UndoUserDeleted = false;
        public bool UndoRigidSchedule;
        public TimeSpan UndoEventDuration;
        public string UndootherPartyID;
        public TimeSpan UndoEventPreDeadline;
        public TimeSpan UndoPrepTime;
        public int UndoPriority;
        public bool UndoIsRestricted = false;
        public static TimeSpan UndoCalculationEndSpan;
        public bool UndoThirdPartyFlag;
        public string UndoThirdPartyUserIDInfo;
        public ThirdPartyControl.CalendarTool UndoThirdPartyTypeInfo;
        public TimeSpan UndoUsedTime;
        public string UndoTimeZone = "UTC";
        public bool UndoIsProcrastinateEvent;

        #endregion
        protected bool _userLocked { get; set; } = false;

        #region IwhyImplementation
        abstract public IWhy Because();

        abstract public IWhy OtherWise();

        abstract public IWhy WhatIf(params Reason[] reasons);
        #endregion

        async public Task InitializeClassification()
        {
            //await Semantics.InitializeClassification(_Name.NameValue);
        }
        public TilerUserGroup getAllUsers()
        {
            return _Users;
        }

        /// <summary>
        /// This updates the name of an event
        /// </summary>
        /// <param name="NewName">The new name of the calendar event</param>
        virtual public void updateEventName(string NewName)
        {
            _Name.updateName(NewName);
        }

        public List<OptimizedGrouping> evaluateDayPreference(IList<OptimizedGrouping> groupings)
        {
            Dictionary<TimelineWithSubcalendarEvents, OptimizedGrouping> TimelinesDict = groupings.ToDictionary(grouping => grouping.GroupAverage.TimeLine, grouping => grouping);
            Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping> TimeOfDayToGroup = groupings.ToDictionary(grouping => grouping.DaySector, grouping => grouping);
            List<TimelineWithSubcalendarEvents> Timelines = orderBasedOnProductivity(TimeOfDayToGroup);
            List<double> foundIndexes = EvaluateTimeLines(Timelines);//
            List<Tuple<double, OptimizedGrouping>> indexToGrouping = foundIndexes.Select((score, index) => { return new Tuple<double, OptimizedGrouping>(score, TimelinesDict[Timelines[index]]); }).OrderBy(tuple => tuple.Item1).ToList();
            int bestIndex = foundIndexes.MinIndex();
            List<OptimizedGrouping> retValue = indexToGrouping.Select(tuple => tuple.Item2).ToList();
            return retValue;
        }

        public virtual List<double> EvaluateTimeLines(List<TimelineWithSubcalendarEvents> timeLines, List<Tuple<Location, Location>> borderLocations = null)
        {
            double worstDistanceInKM = 7;
            List<IList<double>> multiDimensionalClaculation = new List<IList<double>>();
            for (int i = 0; i < timeLines.Count; i++)
            {
                TimelineWithSubcalendarEvents timeline = timeLines[i];
                double distance = Location.calculateDistance(timeline.averageLocation, this.Location, worstDistanceInKM);
                double tickRatio = (double)this.getActiveDuration.Ticks / timeline.TotalFreeSpotAvailable.Ticks;
                double occupancy = (double)timeline.Occupancy;
                IList<double> dimensionsPerDay = new List<double>() { distance, tickRatio, occupancy };
                if (borderLocations != null && borderLocations.Count == timeLines.Count)
                {
                    Tuple<Location, Location> borderLocation = borderLocations[i];
                    double borderLocationsDistance = Location.sumDistance(worstDistanceInKM, borderLocation.Item1, this.Location, borderLocation.Item2);
                    dimensionsPerDay.Add(borderLocationsDistance);
                }
                multiDimensionalClaculation.Add(dimensionsPerDay);
            }
            List<double> foundIndexes = Utility.multiDimensionCalculationNormalize(multiDimensionalClaculation);
            return foundIndexes;
        }

        /// <summary>
        /// Function tries to order the timelines in a manner that is most likely desired by a user for a user.
        /// </summary>
        /// <param name="timeLines"></param>
        /// <returns></returns>
        protected List<TimelineWithSubcalendarEvents> orderBasedOnProductivity(Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping> AllGroupings)
        {
            //TODO need to use machine learning to order the timelines right now the implemenation simple favors a morning schedule
            List<TimeOfDayPreferrence.DaySection> daySectionsPreferredOrder = (new List<TimeOfDayPreferrence.DaySection>() { TimeOfDayPreferrence.DaySection.Morning, TimeOfDayPreferrence.DaySection.Afternoon, TimeOfDayPreferrence.DaySection.Evening, TimeOfDayPreferrence.DaySection.Sleep }).Where(section => AllGroupings.ContainsKey(section)).ToList();
            List<TimelineWithSubcalendarEvents> retValue = daySectionsPreferredOrder.Select(timeOfDay => AllGroupings[timeOfDay].GroupAverage.TimeLine).ToList();
            return retValue;
        }

        public void updateDayPreference(List<OptimizedGrouping> groupings)
        {
            Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping> sectionTOGrouping = groupings.ToDictionary(group => group.DaySector, group => group);
            List<TimeOfDayPreferrence.DaySection> daySections = DaySectionPreference.getPreferenceOrder();
            List<OptimizedGrouping> validGroupings = new List<OptimizedGrouping>();
            foreach (TimeOfDayPreferrence.DaySection section in daySections)
            {
                if (sectionTOGrouping.ContainsKey(section))
                {
                    validGroupings.Add(sectionTOGrouping[section]);
                }
            }
            if (validGroupings.Count > 0)
            {
                List<OptimizedGrouping> updatedGroupingOrder = evaluateDayPreference(validGroupings);
                DaySectionPreference.setPreferenceOrder(updatedGroupingOrder.Select(group => group.DaySector).ToList());
            }
        }

        #region undoFunctions
        public virtual void undoUpdate(Undo undo)
        {
            UndoStartDateTime = StartDateTime;
            UndoEndDateTime = EndDateTime;
            UndoComplete = _Complete;
            UndoEnabled = _Enabled;
            UndoUserDeleted = _UserDeleted;
            _LocationInfo.undoUpdate(undo);
            _UiParams.undoUpdate(undo);
            _DataBlob.undoUpdate(undo);
            _EventRepetition.undoUpdate(undo);
            UndoRigidSchedule = RigidSchedule;
            UndoEventDuration = _EventDuration;
            UndootherPartyID = _otherPartyID;
            UndoEventPreDeadline = _EventPreDeadline;
            UndoPrepTime = _PrepTime;
            UndoPriority = _Priority;
            UndoIsRestricted = isRestricted;
            UndoCalculationEndSpan = CalculationEndSpan;
            _ProfileOfProcrastination.undoUpdate(undo);
            _ProfileOfNow.undoUpdate(undo);
            UndoThirdPartyFlag = _ThirdPartyFlag;
            UndoThirdPartyUserIDInfo = ThirdPartyUserIDInfo;
            UndoThirdPartyTypeInfo = ThirdPartyTypeInfo;
            UndoUsedTime = _UsedTime;
            _Semantics.undoUpdate(undo);
            UndoTimeZone = _TimeZone;
            Name.undoUpdate(undo);
            FirstInstantiation = false;
            this._UndoId = undo.id;
        }

        public virtual void undo(string undoId)
        {
            if(undoId == UndoId)
            {
                Utility.Swap(ref UndoStartDateTime, ref StartDateTime);
                Utility.Swap(ref UndoEndDateTime, ref EndDateTime);
                Utility.Swap(ref UndoComplete, ref _Complete);
                Utility.Swap(ref UndoEnabled, ref _Enabled);
                Utility.Swap(ref UndoUserDeleted, ref _UserDeleted);
                _LocationInfo.undo(undoId);
                _UiParams.undo(undoId);
                _DataBlob.undo(undoId);
                _EventRepetition.undo(undoId);
                Utility.Swap(ref UndoRigidSchedule, ref RigidSchedule);
                Utility.Swap(ref UndoEventDuration, ref _EventDuration);
                Utility.Swap(ref UndootherPartyID, ref _otherPartyID);
                Utility.Swap(ref UndoEventPreDeadline, ref _EventPreDeadline);
                Utility.Swap(ref UndoPrepTime, ref _PrepTime);
                Utility.Swap(ref UndoPriority, ref _Priority);
                Utility.Swap(ref UndoIsRestricted, ref isRestricted);
                Utility.Swap(ref UndoCalculationEndSpan, ref CalculationEndSpan);
                _ProfileOfProcrastination.undo(undoId);
                _ProfileOfNow.undo(undoId);
                Utility.Swap(ref UndoThirdPartyFlag, ref _ThirdPartyFlag);
                Utility.Swap(ref UndoThirdPartyUserIDInfo, ref ThirdPartyUserIDInfo);
                Utility.Swap(ref UndoThirdPartyTypeInfo, ref ThirdPartyTypeInfo);
                Utility.Swap(ref UndoUsedTime, ref _UsedTime);
                _Semantics.undo(undoId);
                Utility.Swap(ref UndoTimeZone, ref _TimeZone);
                Name.undo(undoId);
            }
        }

        public virtual void redo(string undoId)
        {
            if (undoId == UndoId)
            {
                Utility.Swap(ref UndoStartDateTime, ref StartDateTime);
                Utility.Swap(ref UndoEndDateTime, ref EndDateTime);
                Utility.Swap(ref UndoComplete, ref _Complete);
                Utility.Swap(ref UndoEnabled, ref _Enabled);
                Utility.Swap(ref UndoUserDeleted, ref _UserDeleted);
                _LocationInfo.undo(undoId);
                _UiParams.undo(undoId);
                _DataBlob.undo(undoId);
                _EventRepetition.undo(undoId);
                Utility.Swap(ref UndoRigidSchedule, ref RigidSchedule);
                Utility.Swap(ref UndoEventDuration, ref _EventDuration);
                Utility.Swap(ref UndootherPartyID, ref _otherPartyID);
                Utility.Swap(ref UndoEventPreDeadline, ref _EventPreDeadline);
                Utility.Swap(ref UndoPrepTime, ref _PrepTime);
                Utility.Swap(ref UndoPriority, ref _Priority);
                Utility.Swap(ref UndoIsRestricted, ref isRestricted);
                Utility.Swap(ref UndoCalculationEndSpan, ref CalculationEndSpan);
                _ProfileOfProcrastination.undo(undoId);
                _ProfileOfNow.undo(undoId);
                Utility.Swap(ref UndoThirdPartyFlag, ref _ThirdPartyFlag);
                Utility.Swap(ref UndoThirdPartyUserIDInfo, ref ThirdPartyUserIDInfo);
                Utility.Swap(ref UndoThirdPartyTypeInfo, ref ThirdPartyTypeInfo);
                Utility.Swap(ref UndoUsedTime, ref _UsedTime);
                _Semantics.undo(undoId);
                Utility.Swap(ref UndoTimeZone, ref _TimeZone);
                Name.undo(undoId);
            }
        }
        #endregion 
        /// <summary>
        /// This updates the notes of the event
        /// </summary>
        /// <param name="NewName">The new name of the calendar event</param>
        virtual public void updateMiscData(string Notes)
        {
            this._DataBlob.UserNote = Notes;
        }

        public bool getIsComplete
        {
            get
            {
                return _Complete;
            }
        }

        public EventDisplay getUIParam
        {
            get
            {
                return _UiParams;
            }
        }

        public bool isActive
        {
            get
            {
                return ((!getIsComplete) && (isEnabled));
            }
        }

        virtual public bool isEnabled
        {
            get
            {
                return _Enabled;
            }
        }

        virtual public bool getIsDeleted
        {
            get
            {
                return !isEnabled;
            }
        }

        virtual public  DateTimeOffset End
        {
            get
            {
                return EndDateTime;
            }
        }

        virtual public  DateTimeOffset Start
        {
            get
            {
                return StartDateTime;
            }
        }

        public ThirdPartyControl.CalendarTool ThirdpartyType
        {
            get 
            {
                return ThirdPartyTypeInfo;
            }
        }


        public bool getIsProcrastinateCalendarEvent
        {
            get
            {
                return _isProcrastinateEvent;
            }
        }
        public bool isThirdParty
        {
            get
            {
                return ThirdpartyType != ThirdPartyControl.CalendarTool.tiler;
            }
        }

        public  string ThirdPartyID
        {
            get
            {
                return _otherPartyID;
            }
            set
            {
                _otherPartyID = value;
            }
        }

        virtual public string getThirdPartyUserID
        {
            get
            {
                return ThirdPartyUserIDInfo;
            }
        }

        public Repetition Repeat
        {
            get
            {
                return _EventRepetition;
            }
        }
        #region dbProperties
        virtual public DateTimeOffset TimeCreated { get; set; } = DateTimeOffset.Parse(DateTimeOffset.UtcNow.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt"));

        virtual public string Id
        {
            get
            {
                return this.UniqueID.ToString();
            }
            set
            {
                this.UniqueID = new EventID(value);
            }
        }

        public string LocationId { get; set; }
        [NotMapped]
        virtual public Location Location
        {
            set
            {
                _LocationInfo = value;
            }
            get
            {
                return _LocationInfo;
            }
        }

        [ForeignKey("LocationId")]
        virtual public Location Location_DB
        {
            set
            {
                _LocationInfo = value;
            }
            get
            {
                return _LocationInfo.isNull ? null : _LocationInfo;
            }
        }

        virtual public bool isProcrastinateEvent
        {
            set
            {
                _isProcrastinateEvent = value;
            }
            get
            {
                return _isProcrastinateEvent;
            }
        }

        public virtual string NameId { get; set; }
        [ForeignKey("NameId")]
        public virtual EventName Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        public string UiParamsId { get; set; }
        [ForeignKey("UiParamsId")]
        public EventDisplay UiParams_EventDB
        {
            get
            {
                return _UiParams;
            }
            set
            {
                _UiParams = value;
            }
        }


        virtual public DateTimeOffset StartTime_EventDB
        {
            get
            {
                return this.StartDateTime;
            }

            set
            {
                StartDateTime = value;
            }
        }

        virtual public DateTimeOffset EndTime_EventDB
        {
            get
            {
                return this.EndDateTime;
            }
            set
            {
                EndDateTime = value;
            }
        }

        public bool Complete_EventDB
        {
            get
            {
                return this._Complete;
            }
            set
            {
                this._Complete = value;
            }
        }

        public bool UserDeleted_EventDB
        {
            get
            {
                return this._UserDeleted;
            }
            set
            {
                this._UserDeleted = value;
            }
        }

        public string DataBlobId { get; set; }
        [ForeignKey("DataBlobId")]
        public MiscData DataBlob_EventDB
        {
            get
            {
                return this._DataBlob;
            }
            set
            {
                this._DataBlob = value;
            }
        }

        public string EventRepetitionId { get; set; }
        [ForeignKey("EventRepetitionId")]
        public Repetition Repetition_EventDB
        {
            get
            {
                return IsRepeat && _EventRepetition!=null && _EventRepetition.isPersistable ?  _EventRepetition : null;
            }
            set
            {
                this._EventRepetition = value;
            }
        }

        public double Duration_EventDB
        {
            get
            {
                return this._EventDuration.TotalMilliseconds;
            }
            set
            {
                this._EventDuration = TimeSpan.FromMilliseconds(value);
            }
        }


        public string otherPartyID_EventDB
        {
            get
            {
                return this._otherPartyID;
            }
            set
            {
                this._otherPartyID = value;
            }
        }

        public TimeSpan PreDeadline_EventDB
        {
            get
            {
                return this._EventPreDeadline;
            }
            set
            {
                this._EventPreDeadline = value;
            }
        }

        public TimeSpan Preptime_EventDB
        {
            get
            {
                return this._PrepTime;
            }
            set
            {
                this._PrepTime = value;
            }
        }

        public bool RigidSchedule_EventDB
        {
            get
            {
                return this.RigidSchedule;
            }
            set
            {
                this.RigidSchedule = value;
            }
        }

        public int Priority_EventDB
        {
            get
            {
                return this._Priority;
            }
            set
            {
                this._Priority = value;
            }
        }

        public bool isRestricted_EventDB
        {
            get
            {
                return this.isRestricted;
            }
            set
            {
                this.isRestricted = value;
            }
        }

        public string ProcrastinationId { get; set; }
        [ForeignKey("ProcrastinationId")]
        public virtual Procrastination Procrastination_EventDB
        {
            get
            {
                return _ProfileOfProcrastination;
            }
            set
            {
                _ProfileOfProcrastination = value;
            }
        }

        public string NowProfileId { get; set; }
        [ForeignKey("NowProfileId")]
        public NowProfile ProfileOfNow_EventDB
        {
            get
            {
                return _ProfileOfNow;
            }
            set
            {
                _ProfileOfNow = value;
            }
        }

        public TimeSpan UsedTime_EventDB
        {
            get
            {
                return _UsedTime;
            }
            set
            {
                _UsedTime = value;
            }
        }

        public bool ThirdPartyFlag_EventDB
        {
            get
            {
                return _ThirdPartyFlag;
            }
            set
            {
                _ThirdPartyFlag = value;
            }
        }

        public string ThirdPartyTypeInfo_EventDB
        {
            get
            {
                return ThirdPartyTypeInfo.ToString().ToLower();
            }
            set
            {
                ThirdPartyTypeInfo = Utility.ParseEnum<ThirdPartyControl.CalendarTool>(value);
            }
        }

        public string CreatorId { get; set; }
        [ForeignKey("CreatorId")]
        public TilerUser Creator_EventDB
        {
            get
            {
                return _Creator;
            }
            set
            {
                _Creator = value;
            }
        }

        public string SemanticsId { get; set; }
        [ForeignKey("SemanticsId")]
        public Classification Semantics_EventDB
        {
            get
            {
                return _Semantics;
            }
            set
            {
                _Semantics = value;
            }
        }

        public string TilerUserGroupId { get; set; }
        [ForeignKey("TilerUserGroupId")]
        TilerUserGroup Users_EventDB
        {
            get
            {
                return _Users;
            }
            set
            {
                _Users = value;
            }
        }

        public virtual RestrictionProfile RetrictionProfile { get; set; } = null;
        #region undoProperties
        public virtual bool FirstInstantiation { get; set; } = true;

        public string UndoId
        {
            set
            {
                _UndoId = value;
            }
            get
            {
                return _UndoId;
            }
        }
        #endregion
        #endregion


        public virtual bool IsRepeat
        {
            get
            {
                bool retValue = _EventRepetition != null ? _EventRepetition.EnableRepeat : _IsRepeat;
                return retValue;
            }

            set
            {
                _IsRepeat = value;
            }
        }

        virtual public TimeSpan getPreparation
         {
             get
             {
                 return _PrepTime;
             }
         }
         public TimeSpan getPreDeadline
         {
             get
             {
                 return _EventPreDeadline;
             }
         }

         public bool getIsEventRestricted
         {
             get
             {
                 return isRestricted;
             }
         }
        public bool getIsDeadlineElapsed
         {
             get 
             {
                 return End < DateTimeOffset.UtcNow;
             }
         }
        public EventName getName
        {
            get
            {
                return _Name;
            }
        }

        public bool getIsUserDeleted
        {
            get
            {
                return _UserDeleted;
            }

        }

        public int getEventPriority
        {
            get
            {
                return _Priority;
            }
        }

        public virtual Procrastination  getProcrastinationInfo
        {
            get
            {
                return _ProfileOfProcrastination ?? (string.IsNullOrEmpty(ProcrastinationId) ? Procrastination.getDefaultProcrastination() : null);
            }
        }

        public virtual NowProfile getNowInfo
        {
            get
            {
                return _ProfileOfNow;
            }
        }

        public void InitializeDayPreference(TimeLine timeLine)
        {
            if (DaySectionPreference == null)
            {
                DaySectionPreference = new TimeOfDayPreferrence(timeLine);
            }
            DaySectionPreference.InitializeGrouping(this);// InitializeGrouping
        }

        public TimeOfDayPreferrence getDaySection()
        {
            return DaySectionPreference;
        }

        public virtual List<TimeLine> getInterferringWithTimeLine(TimeLine timeLine)
        {
            TimeLine interFerringTimeLine = this.RangeTimeLine.InterferringTimeLine(timeLine);
            return new List<TimeLine>() { interFerringTimeLine };
        }

        public string getCreatorId
        {
            get
            {
                return _Creator.Id;
            }
        }

        public TilerUser getCreator
        {
            get
            {
                return _Creator;
            }
        }

        [NotMapped]
        virtual public TimeSpan UsedTime
        {
            set
            {
                throw new NotImplementedException("You are trying to set the used up time in a tiler events. Invalid action.");
            }

            get
            {
                return _UsedTime;
            }
        }

        virtual public string getId
        {
            get
            {
                return UniqueID.ToString();
            }
        }

        virtual public DateTimeOffset getDeadline
        {
            get {
                return End;
            }
        }

        virtual public TimeSpan getActiveDuration
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        virtual public EventID getTilerID
        {
            get
            {
                return UniqueID;
            }
        }
        public override string ToString()
        {
            return this.Start.ToString() + " - " + this.End.ToString() + "::" + this.getId + "\t\t::" + this.getActiveDuration.ToString();
        }

        virtual public bool isLocked
        {
            get
            {
                return isRigid || _userLocked;
            }
        }

        virtual public bool isRigid
        {
            get
            {
                return RigidSchedule;
            }
        }

        virtual public bool userLocked
        {
            get
            {
                return _userLocked;
            }
            set
            {
                _userLocked = value;
            }
        }

        virtual public TimeLine RangeTimeLine
        {
            get
            {
                TimeLine retValue = new TimeLine(this.Start, this.End);
                return retValue;
            }
        }

        virtual public string getTimeZone
        {
            get
            {
                return _TimeZone;
            }
        }
    }
}
