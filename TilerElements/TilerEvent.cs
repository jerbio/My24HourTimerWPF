using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TilerElements.Reason;
using static TilerElements.TimeOfDayPreferrence;

namespace TilerElements
{
    public abstract class TilerEvent : IWhy, IUndoable, IHasId, IJson
    {
        public enum AccessType { owner, writer, reader, none }
        protected AccessType _Access = AccessType.owner;
        public static TimeSpan ZeroTimeSpan = new TimeSpan(0);
        private DateTimeOffset StartDateTime;
        private DateTimeOffset EndDateTime;
        protected DateTimeOffset _IniStartTime;
        protected DateTimeOffset _IniEndTime;
        private DateTimeOffset TempStartDateTime;
        private DateTimeOffset TempEndDateTime;
        protected bool _Complete = false;
        protected bool _Enabled = true;
        protected bool _AutoDeleted = false;
        protected Reason.AutoDeletion _AutoDeletionReason;
        protected Location _LocationInfo = Location.getNullLocation();
        protected string _LocationValidationId { get; set; }
        protected EventDisplay _UiParams;
        protected MiscData _DataBlob;
        protected Repetition _EventRepetition;
        protected bool _RigidSchedule;
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
        protected TimeOfDayPreferrence _DaySectionPreference;
        protected TilerUserGroup _Users;
        protected string _TimeZone = "UTC";
        protected bool _isProcrastinateEvent = false;
        internal TempTilerEventChanges TempChanges = new TempTilerEventChanges();
        protected EventName _Name;
        protected string _UndoId;
        protected bool _IsRepeat = false;
        protected CalendarEvent _RepeatParentEvent;
        protected DateTimeOffset _TimeOfScheduleLoad;
        protected bool _ValidationIsRun = false;
        protected DateTimeOffset _DeletionTime;
        protected DateTimeOffset _CompletionTime;
        protected ReferenceNow _Now;
        protected double _EventScore = double.NaN;
        protected DateTimeOffset _DeadlineSuggestion;// Holds the deadline suggestion, and by default gets cleared when the deadline is reset
        protected DateTimeOffset _LastDeadlineSuggestion;// Holds the last set deadline suggestions. It is never cleared


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

        public JObject ToJson()
        {
            return Json;
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
            List<Tuple<TimelineWithSubcalendarEvents, OptimizedGrouping, double>> Timelines = orderBasedOnProductivity(TimeOfDayToGroup);
            List<double> foundIndexes = EvaluateTimeLines(
                Timelines.Select(obj => obj.Item1).ToList(),
                Timelines.Select(obj => new Tuple<Location, Location>(obj.Item2.LeftBorder, obj.Item2.RightBorder)).ToList(),
                true,
                Timelines.Select(obj => obj.Item3).ToList()
                );//
            List<Tuple<double, OptimizedGrouping>> indexToGrouping = foundIndexes.Select((score, index) => { return new Tuple<double, OptimizedGrouping>(score, TimelinesDict[Timelines[index].Item1]); }).OrderBy(tuple => tuple.Item1).ToList();
            int bestIndex = foundIndexes.MinIndex();
            List<OptimizedGrouping> retValue = indexToGrouping.Select(tuple => tuple.Item2).ToList();
            return retValue;
        }

        public virtual List<double> EvaluateTimeLines(List<TimelineWithSubcalendarEvents> timeLines, List<Tuple<Location, Location>> borderLocations = null, bool factorInTimelineOrder = false, List<double> weights = null)
        {
            double worstDistanceInKM = 7;
            List<IList<double>> multiDimensionalClaculation = new List<IList<double>>();
            double weight = 1;
            
            for (int i = 0; i < timeLines.Count; i++)
            {
                TimelineWithSubcalendarEvents timeline = timeLines[i];
                double distance = Location.calculateDistance(timeline.averageLocation, this.Location, worstDistanceInKM);
                double tickRatio = (double)this.getActiveDuration.Ticks / timeline.TotalFreeSpotAvailable.Ticks;
                double occupancy = (double)timeline.Occupancy;
                if(weights!=null)
                {
                    weight *= weights[i];
                }
                IList<double> dimensionsPerDay;
                if (factorInTimelineOrder)
                {
                    dimensionsPerDay = new List<double>() { distance* weight, tickRatio * weight, occupancy * weight, (i + 1)* weight };
                }
                else
                {
                    dimensionsPerDay = new List<double>() { distance * weight, tickRatio * weight, occupancy * weight };
                }
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
        protected List<Tuple<TimelineWithSubcalendarEvents, OptimizedGrouping, double>> orderBasedOnProductivity(Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping> AllGroupings)
        {
            //TODO need to use machine learning to order the timelines right now the implemenation simple favors a morning schedule
            List<TimeOfDayPreferrence.DaySection> daySectionsPreferredOrder = null;
            if(getDayPreference().isDefaultOrdering)
            {
                daySectionsPreferredOrder = (new List<TimeOfDayPreferrence.DaySection>() { TimeOfDayPreferrence.DaySection.Morning, TimeOfDayPreferrence.DaySection.Afternoon, TimeOfDayPreferrence.DaySection.Evening, TimeOfDayPreferrence.DaySection.Sleep }).Where(section => AllGroupings.ContainsKey(section)).ToList();
            }
            else
            {
                daySectionsPreferredOrder = getDayPreference().getPreferenceOrder().Where(daySector => AllGroupings.ContainsKey(daySector)).ToList();
            }

            
            List<Tuple<TimelineWithSubcalendarEvents, OptimizedGrouping, double>> retValue = daySectionsPreferredOrder.Select(timeOfDay => new Tuple<TimelineWithSubcalendarEvents, OptimizedGrouping, double>(
                AllGroupings[timeOfDay].GroupAverage.TimeLine, 
                AllGroupings[timeOfDay], 
                TimeOfDayPreferrence.DaySection.Sleep != timeOfDay ? 1 : 8)).ToList();
            return retValue;
        }

        public virtual void updateDayPreference(List<OptimizedGrouping> groupings)
        {
            Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping> sectionTOGrouping = groupings.ToDictionary(group => group.DaySector, group => group);
            List<TimeOfDayPreferrence.DaySection> daySections = _DaySectionPreference.getPreferenceOrder();
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
                _DaySectionPreference.setPreferenceOrder(updatedGroupingOrder.Select(group => group.DaySector).ToList());
            }
        }

        public virtual void storeTimeLine()
        {
            TempStartDateTime = Start;
            TempEndDateTime = End;
        }

        public virtual void restoreTimeLine()
        {
            updateStartTime(TempStartDateTime);
            updateEndTime(TempEndDateTime);
        }

        public void resetScore()
        {
            _EventScore = double.NaN;
        }

        public void setScore(double score)
        {
            _EventScore = score;
        }

        virtual public void updateDeadlineSuggestion(DateTimeOffset deadline)
        {
            _DeadlineSuggestion = deadline;
            _LastDeadlineSuggestion = deadline;
        }

        virtual public void resetAllDeadlineSuggestions()
        {
            resetDeadlineSuggestion();
            resetAutoLastDeadlineSuggestion();
        }

        virtual public void resetDeadlineSuggestion()
        {
            _DeadlineSuggestion = Utility.JSStartTime;
        }

        virtual public void resetAutoLastDeadlineSuggestion()
        {
            _LastDeadlineSuggestion = new DateTimeOffset();
        }

        /// <summary>
        /// Function updates the locationvalidation Id  gotten from third party location services
        /// </summary>
        /// <param name="locationValidationId"></param>
        public virtual void updateLocationValidationId(string locationValidationId)
        {
            this._LocationValidationId = locationValidationId;
        }

        public void validateLocation(Location location)
        {
            _ValidationIsRun = true;
            Location validatedLocation = this._LocationInfo.validate(location, TimeOfScheduleLoad);
            if (validatedLocation != null && !validatedLocation.isNull && !validatedLocation.isDefault)
            {
                _LocationValidationId = validatedLocation.Id;
            }
        }

        public abstract NowProfile initializeNowProfile();


        protected virtual void updateStartTime(DateTimeOffset time)
        {
            this.StartDateTime = time;
        }

        protected virtual void updateEndTime(DateTimeOffset time)
        {
            this.EndDateTime = time;
        }
        abstract public void updateTimeLine(TimeLine newTImeLine, ReferenceNow now);


        #region undoFunctions
        public virtual void undoUpdate(Undo undo)
        {
            UndoStartDateTime = Start;
            UndoEndDateTime = End;
            UndoComplete = _Complete;
            UndoEnabled = _Enabled;
            UndoUserDeleted = _AutoDeleted;
            _LocationInfo.undoUpdate(undo);
            _UiParams.undoUpdate(undo);
            _DataBlob.undoUpdate(undo);
            _EventRepetition.undoUpdate(undo);
            UndoRigidSchedule = _RigidSchedule;
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
            if (undoId == UndoId)
            {
                Utility.Swap(ref UndoStartDateTime, ref StartDateTime);
                Utility.Swap(ref UndoEndDateTime, ref EndDateTime);
                Utility.Swap(ref UndoComplete, ref _Complete);
                Utility.Swap(ref UndoEnabled, ref _Enabled);
                Utility.Swap(ref UndoUserDeleted, ref _AutoDeleted);
                _LocationInfo.undo(undoId);
                _UiParams.undo(undoId);
                _DataBlob.undo(undoId);
                _EventRepetition.undo(undoId);
                Utility.Swap(ref UndoRigidSchedule, ref _RigidSchedule);
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
                Utility.Swap(ref UndoUserDeleted, ref _AutoDeleted);
                _LocationInfo.undo(undoId);
                _UiParams.undo(undoId);
                _DataBlob.undo(undoId);
                _EventRepetition.undo(undoId);
                Utility.Swap(ref UndoRigidSchedule, ref _RigidSchedule);
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

        /// <summary>
        /// This checks if the parent Calendarevent for a subcalendarevent or if the repeatparent of a calendarevent is complete.
        /// </summary>
        abstract public bool isParentComplete { get; }

        public virtual bool getIsComplete
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

        public virtual bool isActive
        {
            get
            {
                return ((!getIsComplete) && (isEnabled));
            }
        }

        public DateTimeOffset CompletionTime
        {
            get
            {
                return this._CompletionTime;
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

        virtual public DateTimeOffset End
        {
            get
            {
                return EndDateTime;
            }
        }

        virtual public DateTimeOffset Start
        {
            get
            {
                return StartDateTime;
            }
        }

        /// <summary>
        /// Return if validation has already being run
        /// </summary>
        virtual public bool IsValidationRun
        {
            get
            {
                return _ValidationIsRun;
            }
        }

        virtual public bool IsLocationValidated
        {
            get
            {
                if (this.Location != null || !this.Location.isDefault)
                {
                    if (!this.Location.IsAmbiguous)
                    {
                        return !this.Location.IsAmbiguous;
                    }
                    else
                    {
                        if (!(string.IsNullOrEmpty(this._LocationValidationId) && string.IsNullOrWhiteSpace(this._LocationValidationId)))
                        {
                            return !this.Location.isDefault;
                        }
                        else
                        {
                            return false;
                        }


                    }
                } else
                {
                    return false;
                }

            }
        }

        public virtual ThirdPartyControl.CalendarTool ThirdpartyType
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

        public string ThirdPartyID
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

        public virtual AutoDeletion AutoDeletion_Reason
        {
            get
            {
                return this._AutoDeletionReason;
            }
        }
        public string LocationValidationId
        {
            get
            {
                return this._LocationValidationId;
            }
        }

        virtual public DateTimeOffset InitialStartTime
        {
            get
            {
                return _IniStartTime;
            }
        }

        virtual public DateTimeOffset InitialEndTme
        {
            get
            {
                return _IniEndTime;
            }
        }


        /// <summary>
        /// This is the location of the tiler event and is inferred from the initialization.
        /// If there is no location set this returns the default location
        /// </summary>
        [NotMapped]
        virtual public Location Location
        {
            set
            {
                _LocationInfo = value;
            }
            get
            {
                if (_LocationInfo != null && _LocationInfo.IsVerified)
                {
                    return _LocationInfo;
                }
                if (_LocationInfo != null && _LocationInfo.IsAmbiguous)
                {
                    Location retValue = _LocationInfo.getLocationThroughValidation(_LocationValidationId, TimeOfScheduleLoad);
                    if (retValue != null && !retValue.isDefault)
                    {
                        updateLocationValidationId(retValue?.Id);
                    }
                    return retValue ?? Location.getDefaultLocation();
                }
                return _LocationInfo;
            }
        }
        [NotMapped]
        virtual public bool isLocationAmbiguous
        {
            get
            {
                if (_LocationInfo == null)
                    return true;
                else
                {
                    return _LocationInfo.IsAmbiguous;
                }
            }
        }

        /// <summary>
        /// Gets the direct object as it is in the object without any internal lookups and verifications.
        /// Get the tiler event object. Retunrns null if it isnt set.
        /// </summary>
        virtual public Location LocationObj
        {
            get
            {
                return _LocationInfo;
            }
        }

        virtual public DateTimeOffset DeadlineSuggestion
        {
            get
            {
                return _DeadlineSuggestion;
            }
        }

        virtual public DateTimeOffset LastDeadlineSuggestion
        {
            get
            {
                return _LastDeadlineSuggestion;
            }
        }

        [NotMapped]
        public ReferenceNow Now
        {
            set
            {
                _Now = value;
            }
            get
            {
                return _Now;
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

        public string LocationValidationId_DB
        {
            get {
                return _LocationValidationId;
            }
            set {
                _LocationValidationId = value;
            }
        }

        public long DeadlineSuggestion_DB
        {
            get
            {
                return _DeadlineSuggestion.ToUnixTimeMilliseconds();
            }

            set
            {
                _DeadlineSuggestion = DateTimeOffset.FromUnixTimeMilliseconds(value);
            }
        }

        public long LastDeadlineSuggestion_DB
        {
            get
            {
                return _LastDeadlineSuggestion.ToUnixTimeMilliseconds();
            }

            set
            {
                _LastDeadlineSuggestion = DateTimeOffset.FromUnixTimeMilliseconds(value);
            }
        }

        public string LocationId { get; set; }

        [ForeignKey("LocationId")]
        virtual public Location Location_DB
        {
            set
            {
                _LocationInfo = value;
            }
            get
            {
                if (_LocationInfo == null)
                {
                    return null;
                }
                else
                {
                    return _LocationInfo.isNull || _LocationInfo.isDefault ? null : _LocationInfo;
                }
            }
        }

        virtual public bool IsEnabled_DB
        {
            get
            {
                return _Enabled;
            }
            set
            {
                _Enabled = value;
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

        [Index("UserIdAndStart", Order = 1)]
        virtual public DateTimeOffset StartTime_EventDB
        {
            get
            {
                return this.StartDateTime;
            }

            set
            {
                this.StartDateTime = value;
            }
        }

        [Index("UserIdAndEnd", Order = 1)]
        virtual public DateTimeOffset EndTime_EventDB
        {
            get
            {
                return this.EndDateTime;
            }
            set
            {
                this.EndDateTime = value;
            }
        }

        [Index("UserIdAndIniStart", Order = 1)]
        virtual public long InitialStartTime_DB
        {
            get
            {
                return _IniStartTime.ToUnixTimeMilliseconds();
            }
            set
            {

                _IniStartTime = DateTimeOffset.FromUnixTimeMilliseconds(value);
            }
        }

        [Index("UserIdAndIniEnd", Order = 1)]
        virtual public long InitialEndTime_DB
        {
            get
            {
                return _IniEndTime.ToUnixTimeMilliseconds();
            }
            set
            {
                this._IniEndTime = DateTimeOffset.FromUnixTimeMilliseconds(value);
            }
        }

        [Index("UserIdAndCompleteTime", Order = 1)]
        virtual public long CompletionTime_EventDB
        {
            get
            {
                return this._CompletionTime.ToUnixTimeMilliseconds();
            }
            set
            {
                this._CompletionTime = DateTimeOffset.FromUnixTimeMilliseconds(value);
            }
        }

        [Index("UserIdAndDeletionTime", Order = 1)]
        virtual public long DeletionTime_DB
        {
            get
            {
                return this._DeletionTime.ToUnixTimeMilliseconds();
            }
            set
            {
                this._DeletionTime = DateTimeOffset.FromUnixTimeMilliseconds(value);
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

        public virtual bool AutoDeleted_EventDB
        {
            get
            {
                return this._AutoDeleted;
            }
            set
            {
                this._AutoDeleted = value;
            }
        }

        [NotMapped]
        public virtual DateTimeOffset TimeOfScheduleLoad
        {
            get {
                return _TimeOfScheduleLoad;
            }
            set
            {
                _TimeOfScheduleLoad = value;
            }
        }

        [DefaultValue(0)]
        virtual public int AutoDeletionCount_DB
        {
            set; get;
        }

        public virtual string AutoDeletion_ReasonDB
        {
            get
            {
                return this._AutoDeletionReason.ToString().ToLower();
            }
            set
            {
                if (value != null)
                {
                    _AutoDeletionReason = Utility.ParseEnum<Reason.AutoDeletion>(value);
                } else
                {
                    _AutoDeletionReason = AutoDeletion.None;
                }

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


        public virtual bool IsRecurring
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

        public string EventRepetitionId { get; set; }
        [ForeignKey("EventRepetitionId")]
        public Repetition Repetition_EventDB
        {
            get
            {
                return IsRecurring && _EventRepetition!=null && _EventRepetition.isPersistable ?  _EventRepetition : null;
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

        public virtual double PreDeadline_EventDB
        {
            get
            {
                return this._EventPreDeadline.TotalMilliseconds;
            }
            set
            {
                this._EventPreDeadline = TimeSpan.FromMilliseconds(value);
            }
        }

        public virtual double Preptime_EventDB
        {
            get
            {
                return this._PrepTime.TotalMilliseconds;
            }
            set
            {
                this._PrepTime = TimeSpan.FromMilliseconds(value);
            }
        }

        public bool RigidSchedule_EventDB
        {
            get
            {
                return this._RigidSchedule;
            }
            set
            {
                this._RigidSchedule = value;
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
                if (_ProfileOfProcrastination == null)
                {
                    return null;
                }
                else
                {
                    return _ProfileOfProcrastination.isNull ? null : _ProfileOfProcrastination;
                }
            }
            set
            {
                _ProfileOfProcrastination = value;
            }
        }

        public string NowProfileId { get; set; }
        [ForeignKey("NowProfileId")]
        public virtual NowProfile ProfileOfNow_EventDB
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

        public double UsedTime_EventDB
        {
            get
            {
                return this._UsedTime.TotalMilliseconds;
            }
            set
            {
                this._UsedTime = TimeSpan.FromMilliseconds(value);
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

        public AccessType Access
        {
            get
            {
                return _Access;
            }
        }

        public string Access_DB
        {
            get
            {
                return _Access.ToString().ToLower();
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value))
                {
                    _Access = Utility.ParseEnum<AccessType>(value);
                }
                else
                {
                    _Access = AccessType.owner;
                }
            }
        }

        public virtual bool isReadOnly {
            get {
                return this.Access == AccessType.reader;
            }
        }

        public virtual bool isModifiable {
            get
            {
                return this.Access == AccessType.owner || this.Access== AccessType.writer;
            }
        }

        public virtual bool isNoAcces
        {
            get
            {
                return this.Access == AccessType.none;
            }
        }

        [Index("UserIdAndStart", Order = 0)]
        [Index("UserIdAndEnd", Order = 0)]
        [Index("UserIdAndIniStart", Order = 0)]
        [Index("UserIdAndIniEnd", Order = 0)]
        [Index("UserIdAndCompleteTime", Order = 0)]
        [Index("UserIdAndDeletionTime", Order = 0)]
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

        [Index("RepeatParentCalendarEventId", Order = 0)]
        public virtual string RepeatParentEventId { get; set; }
        /// <summary>
        /// I chose to the class TilerEvent as the type for this Data memeber because if I use calendarevent(as one might assume),
        /// entity framework would try to create one to many relationship with the "AllSubevents_DB" data meber in calendarEvent which isnt what we're going for.
        /// This Data member only stores the CalendarEvent from which repetition was created
        /// </summary>
        [ForeignKey("RepeatParentEventId")]
        public virtual TilerEvent RepeatParentEvent
        {
            get
            {
                return _RepeatParentEvent;
            }
            set
            {
                _RepeatParentEvent = value as CalendarEvent;
            }
        }

        public virtual string RestrictionProfileId { get; set; }
        [ForeignKey("RestrictionProfileId")]
        public virtual RestrictionProfile RestrictionProfile_DB { get; set; } = null;

        public virtual RestrictionProfile RestrictionProfile { get; }
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

        
        /// <summary>
        /// This means this enetity is from a repeating origin
        /// So for example a subEvent with a repeating parent event will be true
        /// or a repeating calendarevent for monday, Wednesday, and Friday will all have this as true. 
        /// The only time this is is when there is no related repeat object
        /// </summary>
        public virtual bool IsFromRecurring
        {
            get
            {
                return IsRecurring;
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
                return _AutoDeleted;
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

        public virtual void InitializeDayPreference(TimeLine timeLine)
        {
            if (_DaySectionPreference == null)
            {
                _DaySectionPreference = new TimeOfDayPreferrence(timeLine);
            }
            _DaySectionPreference.InitializeGrouping(this);// InitializeGrouping
        }

        public TimeOfDayPreferrence getDayPreference()
        {
            return _DaySectionPreference;
        }

        public virtual DaySection getCurrentDaySection()
        {
            return getDayPreference().getCurrentDayPreference();
        }

        public virtual List<TimeLine> getInterferringWithTimeLine(TimeLine timeLine)
        {
            TimeLine interFerringTimeLine = this.StartToEnd.InterferringTimeLine(timeLine);
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
            return this.Start.toTimeZoneTime().ToString() + " - " + this.End.toTimeZoneTime().ToString()  + "\t\t::" + this.getActiveDuration.ToString() + "::" + this.getId;
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
                return _RigidSchedule;
            }
        }

        virtual public TimeLine StartToEnd
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

        virtual public double EventScore
        {
            get
            {
                return _EventScore;
            }
        }

        virtual public JObject Json
        {
            get
            {
                JObject retValue = new JObject();
                retValue.Add("id", this.Id);
                retValue.Add("start", this.Start.ToUnixTimeMilliseconds());
                retValue.Add("end", this.End.ToUnixTimeMilliseconds());
                retValue.Add("name", this.Name?.NameValue);
                retValue.Add("isRigid", this.isRigid);

                return retValue;
            }
        }
    }
}
