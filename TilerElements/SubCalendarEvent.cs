using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
namespace TilerElements
{
    public class SubCalendarEvent : TilerEvent,IDefinedRange
    {
        public static DateTimeOffset InitialPauseTime  = new DateTimeOffset();
        protected BusyTimeLine BusyFrame;
        protected BusyTimeLine TempBusyFrame;
        protected TimeLine _CalendarEventRange;
        protected DateTimeOffset _CalendarEventRangeStart;
        protected DateTimeOffset _CalendarEventRangeEnd;
        protected double EventScore;
        protected ConflictProfile _ConflictingEvents = new ConflictProfile();
        protected ulong preferredDayIndex=0;
        protected int MiscIntData;
        protected bool Vestige = false;
        protected ulong UnUsableIndex;
        protected ulong OldPreferredIndex;
        protected bool CalculationMode = false;
        protected DateTimeOffset _PauseTime = InitialPauseTime;
        protected bool BlobEvent = false;
        protected bool OptimizationFlag = false;
        protected List<Reason> TimePositionReasons = new List<Reason>();
        protected DateTimeOffset _LastReasonStartTimeChanged;
        protected TimeLine CalculationTimeLine = null;
        protected CalendarEvent _calendarEvent;
        protected bool _LockToId = false;
        [NotMapped]
        public TimeSpan TravelTimeBefore { get; set; } = new TimeSpan(1);
        [NotMapped]
        public TimeSpan TravelTimeAfter { get; set; } = new TimeSpan(1);
        public bool isWake { get; set; } = false;
        public bool isSleep { get; set; } = false;
        protected bool tempLock { get; set; } = false;// This should never get persisted
        protected bool lockedPrecedingHours { get; set; } = false;// This should never get persisted
        protected bool _enablePre_reschedulingTimelineLockDown { get; set; } = true;// This prevent locking for preceding twentyFour or for interferring with now
        /// <summary>
        /// This holds the current session reasons. It will updated based on data and calculation optimizations from HistoricalCurrentPosition
        /// </summary>
        protected Dictionary<TimeSpan, List<Reason>> ReasonsForCurrentPosition = new Dictionary<TimeSpan, List<Reason>>();
        /// <summary>
        /// Will hold the reasons that were collated from the last time the schedule was modified. This is to be only loaded from storage and not to be updated
        /// </summary>
        protected Dictionary<TimeSpan, List<Reason>> HistoricalReasonsCurrentPosition = new Dictionary<TimeSpan, List<Reason>>();

        #region undoMembers

        #endregion

        public void updateprocrastinationtree(Procrastination procrastination)
        {
            if (procrastination != null && !procrastination.isNull && string.IsNullOrEmpty(this.ProcrastinationId))
            {
                //this.ProcrastinationId = null;
                this.Procrastination_EventDB = null;
                //this.ProcrastinationId = procrastination.Id;
                this.Procrastination_EventDB = procrastination;
            }
        }

        public void updatenowprofiletree(NowProfile nowProfile)
        {
            if (nowProfile != null && string.IsNullOrEmpty(this.NowProfileId))
            {
                this.ProfileOfNow_EventDB = null;
                //this.NowProfileId = null;
                this.ProfileOfNow_EventDB = nowProfile;
                //this.NowProfileId = nowProfile.Id;
            }
        }

        #region Classs Constructor
        protected SubCalendarEvent()
        { }

        public SubCalendarEvent(CalendarEvent calendarEvent, TilerUser Creator, TilerUserGroup users, string timeZone, TimeSpan Event_Duration, EventName name, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, bool Enabled, EventDisplay UiParam,MiscData Notes,bool completeFlag, Location EventLocation =null, TimeLine calendarEventRange = null, ConflictProfile conflicts=null)
        {
            if (EventDeadline < EventStart)
            {
                throw new Exception("SubCalendar Event cannot have an end time earlier than the start time");
            }
            _Name = name;
            _Creator = Creator;
            _Users = users;
            _TimeZone = timeZone;
            if (conflicts == null)
            {
                conflicts = new ConflictProfile();
            }
            _ConflictingEvents = conflicts;
            _CalendarEventRange = calendarEventRange;
            updateStartTime(EventStart);
            updateEndTime( EventDeadline);
            _EventDuration = Event_Duration;
            _PrepTime = EventPrepTime;
            if (myParentID == "16")
            {
                ;
            }
            _UiParams=UiParam;
            _DataBlob = Notes;
            _Complete=completeFlag;
            UniqueID = EventID.GenerateSubCalendarEvent(myParentID);
            BusyFrame = new BusyTimeLine(this.getId, Start, End);//this is because in current implementation busy frame is the same as CalEvent frame
            this._LocationInfo = EventLocation;
//            EventSequence = new EventTimeLine(UniqueID.ToString(), StartDateTime, EndDateTime);
            _RigidSchedule = Rigid;
            this._Enabled = Enabled;
            _LastReasonStartTimeChanged = this.Start;
            _calendarEvent = calendarEvent;
        }
        
        public SubCalendarEvent(CalendarEvent calendarEvent, TilerUser Creator, TilerUserGroup users, string timeZone, string MySubEventID, EventName name, DateTimeOffset EventStart, DateTimeOffset EventDeadline, BusyTimeLine SubEventBusy, bool Rigid, bool Enabled, EventDisplay UiParam, MiscData Notes, bool completeFlag, Location EventLocation = null, TimeLine calendarEventRange = null, ConflictProfile conflicts = null)
        {
            if (EventDeadline < EventStart)
            {
                throw new Exception("SubCalendar Event cannot have an end time earlier than the start time");
            }
            _TimeZone = timeZone;
            _Name = name;
            _Creator = Creator;
            _Users = users;
            if (conflicts == null)
            {
                conflicts = new ConflictProfile();
            }
            _ConflictingEvents = conflicts;
            _CalendarEventRange = calendarEventRange;
            UniqueID = new EventID(MySubEventID);
            updateStartTime(EventStart);
            updateEndTime(EventDeadline);
            _EventDuration = SubEventBusy.TimelineSpan;
            BusyFrame = SubEventBusy;
            _RigidSchedule = Rigid;
            this._Enabled = Enabled;
            this._LocationInfo = EventLocation;
            _UiParams = UiParam;
            _DataBlob = Notes;
            _Complete = completeFlag;
            _LastReasonStartTimeChanged = this.Start;
            _calendarEvent = calendarEvent;
        }
        #endregion

        #region Class functions

        public virtual void updateCalculationEventRange(TimeLine timeLine)
        {
            TimeLine interferringTimeLine = this.getCalendarEventRange.InterferringTimeLine(timeLine);
            if(interferringTimeLine == null)
            {
                this.CalculationTimeLine = timeLine;
            }
            else
            {
                this.CalculationTimeLine = interferringTimeLine;
            }
        }

        public virtual void disable(CalendarEvent myCalEvent)
        {
            if (this._Enabled)
            {
                this._Enabled = false;
                this._AutoDeleted = false;
                myCalEvent.incrementDeleteCount(this.RangeSpan);
            }
        }

        public virtual void autoDisable(CalendarEvent myCalEvent, TilerElements.Reason.AutoDeletion reason)
        {
            if (this._Enabled)
            {
                this._Enabled = false;
                myCalEvent.incrementAutoDeleteCount(this.RangeSpan);
            }
            this._AutoDeleted = true;
        }

        internal void disableWithoutUpdatingCalEvent()
        {
            this._Enabled = false;
        }

        public virtual void complete(CalendarEvent myCalEvent)
        {
            if (!this._Complete)
            {
                this._Complete = true;
                myCalEvent.addCompletionTimes(this.Start);
                myCalEvent.incrementCompleteCount(this.RangeSpan);
            }
        }

        public void nonComplete(CalendarEvent myCalEvent)
        {
            if (this._Complete)
            {
                this._Complete = false;
                myCalEvent.removeCompletionTimes(this.Start);
                myCalEvent.decrementCompleteCount(this.RangeSpan);
            }
            
        }

        internal void completeWithoutUpdatingCalEvent()
        {
            this._Complete = true;
        }

        internal void nonCompleteWithoutUpdatingCalEvent()
        {
            this._Complete = false;
        }

        public void Enable(CalendarEvent myCalEvent)
        {
            if (!this._Enabled)
            {
                this._Enabled = true;
                myCalEvent.decrementDeleteCount(this.RangeSpan);
            }
        }

        internal void enableWithouUpdatingCalEvent()
        {
            this._Enabled = true;
        }

        protected void updateDayIndex(ulong dayIndex)
        {
            if (dayIndex == ReferenceNow.UndesignatedDayIndex)
            {
                undesignate();
            }
            else
            {
                this.preferredDayIndex = dayIndex;
            }
        }

        public virtual void SetCompletionStatus(bool completeValue,CalendarEvent myCalendarEvent)
        {
            if (completeValue != _Complete)
            {
                if (completeValue)
                {
                    complete(myCalendarEvent);
                }
                else
                {
                    nonComplete(myCalendarEvent);
                }
            }
        }

        public override void storeTimeLine()
        {
            base.storeTimeLine();
            TempBusyFrame = BusyFrame;
        }

        public override void restoreTimeLine()
        {
            base.restoreTimeLine();
            BusyFrame = TempBusyFrame;
        }

        public void tempLockSubEvent()
        {
            tempLock = true;
        }

        public void resetTempUnlock()
        {
            tempLock = false;
        }

        public void lockPrecedingHours()
        {
            if (_enablePre_reschedulingTimelineLockDown)
            {
                lockedPrecedingHours = true;
            }
        }

        public void unLockPrecedingHours()
        {
            if (_enablePre_reschedulingTimelineLockDown)
            {
                lockedPrecedingHours = false;
            }
        }

        public void disablePreschedulingLock()
        {
            _enablePre_reschedulingTimelineLockDown = false;
        }

        public void enablePreschedulingLock()
        {
            _enablePre_reschedulingTimelineLockDown = true;
        }

        virtual public void addReasons(Reason eventReason)
        {
            TimeSpan TimeDelta = this.Start - _LastReasonStartTimeChanged;
            if (!ReasonsForCurrentPosition.ContainsKey(TimeDelta))
            {
                ReasonsForCurrentPosition.Add(TimeDelta, new List<Reason>());
            }

            ReasonsForCurrentPosition[TimeDelta].Add(eventReason);
            TimePositionReasons.Add(eventReason);
            _LastReasonStartTimeChanged = this.Start;
        }

        virtual public void clearAllReasons()
        {
            ReasonsForCurrentPosition = new Dictionary<TimeSpan, List<Reason>>();
        }
        override public IWhy Because()
        {
            throw new NotImplementedException("Yet to implement a because functionality for subcalendar event");
        }

        override public IWhy OtherWise()
        {
            throw new NotImplementedException("Yet to implement a OtherWise functionality for subcalendar event");
        }

        virtual public IWhy WhatIfDeadline(DateTimeOffset AssumedTime)
        {
            throw new NotImplementedException("Yet to implement a WhatIf functionality for subcalendar event");
        }

        virtual public IWhy WhatIfStartTime(DateTimeOffset AssumedTime)
        {
            throw new NotImplementedException("Yet to implement a WhatIf functionality for subcalendar event");
        }

        override public IWhy WhatIf(params Reason[] reasons)
        {
            throw new NotImplementedException("Yet to implement a WhatIf functionality for subcalendar event");
        }

        virtual public bool IsDateTimeWithin(DateTimeOffset DateTimeEntry)
        {
            return StartToEnd.IsDateTimeWithin(DateTimeEntry);
        }

        public static SubCalendarEvent getEmptySubCalendarEvent(EventID CalendarEventId)
        {
            SubCalendarEvent retValue = new SubCalendarEvent();
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventId.ToString());
            retValue.updateStartTime( DateTimeOffset.UtcNow.removeSecondsAndMilliseconds());
            retValue.updateEndTime( retValue.Start);
            retValue._EventDuration = new TimeSpan(0);
            
            retValue._RigidSchedule= true;
            retValue._Complete = true;
            retValue._Enabled = false;
            return retValue;
        }

        virtual public void setAsOptimized()
        {
            OptimizationFlag = true;
        }

        virtual public void setAsUnOptimized()
        {
            OptimizationFlag = false;
        }


        virtual public SubCalendarEvent CreateCopy(EventID eventId)
        {
            string Id;
            if (eventId != null)
            {
                Id = eventId.ToString();
            }
            else
            {
                Id = this.getId;
            }
            SubCalendarEvent MySubCalendarEventCopy = new SubCalendarEvent(this.ParentCalendarEvent, getCreator, _Users, this._TimeZone, Id, this.getName?.createCopy(), Start, End, BusyFrame?.CreateCopy() as BusyTimeLine, this._RigidSchedule, this.isEnabled, this._UiParams?.createCopy(), this.Notes?.createCopy(), this._Complete, this._LocationInfo, new TimeLine(getCalendarEventRange.Start, getCalendarEventRange.End), _ConflictingEvents?.CreateCopy());
            MySubCalendarEventCopy.ThirdPartyID = this.ThirdPartyID;
            MySubCalendarEventCopy._AutoDeleted = this._AutoDeleted;
            MySubCalendarEventCopy.isRestricted = this.isRestricted;
            MySubCalendarEventCopy.preferredDayIndex = this.preferredDayIndex;
            MySubCalendarEventCopy._Creator = this._Creator;
            MySubCalendarEventCopy._Semantics = this._Semantics !=null ?this._Semantics.createCopy() : null;
            MySubCalendarEventCopy._UsedTime = this._UsedTime;
            MySubCalendarEventCopy.OptimizationFlag = this.OptimizationFlag;
            MySubCalendarEventCopy._LastReasonStartTimeChanged = this._LastReasonStartTimeChanged;
            MySubCalendarEventCopy._DaySectionPreference = this._DaySectionPreference;
            MySubCalendarEventCopy._calendarEvent = this._calendarEvent;
            MySubCalendarEventCopy.TravelTimeAfter = this.TravelTimeAfter;
            MySubCalendarEventCopy.TravelTimeBefore= this.TravelTimeBefore;
            MySubCalendarEventCopy.isSleep = this.isSleep;
            MySubCalendarEventCopy.isWake = this.isWake;
            MySubCalendarEventCopy.userLocked = this._userLocked;
            MySubCalendarEventCopy.tempLock = this.tempLock;
            MySubCalendarEventCopy.LocationValidationId_DB = this.LocationValidationId_DB;
            MySubCalendarEventCopy.lockedPrecedingHours = this.lockedPrecedingHours;
            MySubCalendarEventCopy._enablePre_reschedulingTimelineLockDown = this._enablePre_reschedulingTimelineLockDown;
            if (this.CalculationTimeLine != null)
            {
                MySubCalendarEventCopy.CalculationTimeLine = this.CalculationTimeLine.CreateCopy();
            }
            
            return MySubCalendarEventCopy;
        }

        internal void designate(ReferenceNow now)
        {
            ulong dayIndex = now.getDayIndexFromStartOfTime(this.Start);
            updateDayIndex(dayIndex);
        }


        internal virtual void undesignate()
        {
            this.preferredDayIndex = ReferenceNow.UndesignatedDayIndex;
        }

        public void setScore(double score)
        {
            EventScore = score;
        }

        public void incrementScore(double score)
        {
            EventScore += score;
        }

        public static void resetScores(IEnumerable<SubCalendarEvent> AllSUbevents)
        {
            AllSUbevents.AsParallel().ForAll(obj => obj.EventScore = 0);
        }

        public static TimeSpan TotalActiveDuration(IEnumerable<SubCalendarEvent> ListOfSubCalendarEvent)
        {
            TimeSpan TotalTimeSpan = new TimeSpan(0);
            
            foreach (SubCalendarEvent mySubCalendarEvent in ListOfSubCalendarEvent)
            {
                TotalTimeSpan=TotalTimeSpan.Add(mySubCalendarEvent.getActiveDuration);
            }

            return TotalTimeSpan;
        }

        
        virtual public bool PinToStart(TimeLine MyTimeLine)
        {
            DateTimeOffset ReferenceStartTime = new DateTimeOffset();
            DateTimeOffset ReferenceEndTime = new DateTimeOffset();

            ReferenceStartTime = MyTimeLine.Start;
            if (this.getCalculationRange.Start > MyTimeLine.Start)
            {
                ReferenceStartTime = this.getCalculationRange.Start;
            }

            ReferenceEndTime = this.getCalculationRange.End;
            if (this.getCalculationRange.End > MyTimeLine.End)
            {
                ReferenceEndTime = MyTimeLine.End;
            }

            /*foreach (SubCalendarEvent MySubCalendarEvent in MySubCalendarEventList)
            {
                SubCalendarTimeSpan = SubCalendarTimeSpan.Add(MySubCalendarEvent.ActiveDuration);//  you might be able to combine the implementing for lopp with this in order to avoid several loops
            }*/
            TimeSpan TimeDifference = (ReferenceEndTime - ReferenceStartTime);

            if (this.isLocked)
            {
                return (MyTimeLine.IsTimeLineWithin( this.StartToEnd));
            }

            if (this._EventDuration > TimeDifference)
            {
                return false;
                //throw new Exception("Oh oh check PinSubEventsToStart Subcalendar is longer than available timeline");
            }
            if ((ReferenceStartTime > this.getCalculationRange.End) || (ReferenceEndTime < this.getCalculationRange.Start))
            {
                return false;
                //throw new Exception("Oh oh Calendar event isn't Timeline range. Check PinSubEventsToEnd :(");
            }

            List<BusyTimeLine> MyActiveSlot = new List<BusyTimeLine>();
            //foreach (SubCalendarEvent MySubCalendarEvent in MySubCalendarEventList)
            
                this.updateStartTime( ReferenceStartTime);
                this.updateEndTime( this.Start + this.getActiveDuration);
                //this.ActiveSlot = new BusyTimeLine(this.ID, (this.StartDateTime), this.EndDateTime);
                TimeSpan BusyTimeLineShift = this.Start - ActiveSlot.Start;
                ActiveSlot.shiftTimeline(BusyTimeLineShift);
                return true;
        }


        public override void updateDayPreference(List<OptimizedGrouping> groupings)
        {
            Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping> sectionTOGrouping = groupings.ToDictionary(group => group.DaySector, group => group);
            List<TimeOfDayPreferrence.DaySection> daySections = _DaySectionPreference.getPreferenceOrder();
            List<OptimizedGrouping> validGroupings = new List<OptimizedGrouping>();
            foreach (TimeOfDayPreferrence.DaySection section in daySections)
            {
                if (sectionTOGrouping.ContainsKey(section))
                {
                    OptimizedGrouping group = sectionTOGrouping[section];
                    var interferringTimeLine = this.getCalculationRange.InterferringTimeLine(group.TimeLine);
                    if (interferringTimeLine!=null)
                    {
                        if (this.canExistWithinTimeLine(group.TimeLine))
                        {
                            validGroupings.Add(sectionTOGrouping[section]);
                        } else
                        {
                            if(interferringTimeLine.TimelineSpan <= this.getActiveDuration)
                            {
                                validGroupings.Add(sectionTOGrouping[section]);
                            }
                        }
                    }
                    
                }
            }
            if (validGroupings.Count > 0)
            {
                List<OptimizedGrouping> updatedGroupingOrder = evaluateDayPreference(validGroupings);
                _DaySectionPreference.setPreferenceOrder(updatedGroupingOrder.Select(group => group.DaySector).ToList());
            }
        }

        /// <summary>
        /// function updates the parameters of the current sub calevent using SubEventEntry. However it doesnt change some datamemebres such as rigid, and isrestricted. You 
        /// </summary>
        /// <param name="SubEventEntry"></param>
        /// <returns></returns>
        virtual public bool UpdateThis(SubCalendarEvent SubEventEntry)
        {
            if (this.getId == SubEventEntry.getId)
            {
                this.BusyFrame = SubEventEntry.ActiveSlot;
                this._CalendarEventRange = SubEventEntry.getCalendarEventRange;
                this._Name = SubEventEntry.getName;
                this._EventDuration = SubEventEntry.getActiveDuration;
                this._Complete = SubEventEntry.getIsComplete;
                this._ConflictingEvents = SubEventEntry.Conflicts;
                this._DataBlob = SubEventEntry.Notes;
                this._Enabled = SubEventEntry.isEnabled;
                this.updateEndTime( SubEventEntry.End);
                this._EventPreDeadline = SubEventEntry.getPreDeadline;
                this.EventScore = SubEventEntry.Score;
                this.isRestricted = SubEventEntry.getIsEventRestricted;
                this._LocationInfo = SubEventEntry._LocationInfo;
                this.OldPreferredIndex = SubEventEntry.OldUniversalIndex;
                this._otherPartyID = SubEventEntry.ThirdPartyID;
                this.preferredDayIndex = SubEventEntry.UniversalDayIndex;
                this._PrepTime = SubEventEntry.getPreparation;
                this._Priority = SubEventEntry.getEventPriority;
                this._ProfileOfNow = SubEventEntry._ProfileOfNow;
                //this.RigidSchedule = SubEventEntry.Rigid;
                this.updateStartTime( SubEventEntry.Start);
                this._UiParams = SubEventEntry.getUIParam;
                this.UniqueID = SubEventEntry.SubEvent_ID;
                this._AutoDeleted = SubEventEntry.getIsUserDeleted;
                this._Users = SubEventEntry.getAllUsers();
                this.Vestige = SubEventEntry.isVestige;
                this._otherPartyID = SubEventEntry._otherPartyID;
                this._Creator = SubEventEntry._Creator;
                this._Semantics = SubEventEntry._Semantics;
                this._UsedTime = SubEventEntry._UsedTime;
                this._LocationValidationId = this._LocationValidationId;
                return true;
            }

            throw new Exception("Error Detected: Trying to update SubCalendar Event with non matching ID");
        }

        virtual public SubCalendarEvent getProcrastinationCopy(CalendarEvent CalendarEventData,Procrastination ProcrastinationData)
        {
            SubCalendarEvent retValue = getCalulationCopy();
            /*
            retValue.CalendarEventRange = CalendarEventData.RangeTimeLine;
            TimeSpan SpanShift = ProcrastinationData.PreferredStartTime - retValue.Start;
            */
            retValue._CalendarEventRange = new TimeLine(ProcrastinationData.PreferredStartTime, retValue.getCalendarEventRange.End);
            TimeSpan SpanShift = (retValue.getCalendarEventRange.End - retValue.RangeSpan) - retValue.Start;
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventData.getId);
            retValue.shiftEvent(SpanShift,true);
            return retValue;
        }

        virtual public SubCalendarEvent getNowCopy(EventID CalendarEventID, NowProfile NowData)
        {
            SubCalendarEvent retValue = getCalulationCopy();
            retValue._RigidSchedule = true;
            TimeSpan SpanShift = NowData.PreferredTime - retValue.Start;
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventID.ToString());
            retValue.shiftEvent(SpanShift, true);

            return retValue;
        }

        virtual protected SubCalendarEvent getCalulationCopy()
        {
            SubCalendarEvent retValue = new SubCalendarEvent();
            retValue.BusyFrame = this.ActiveSlot.CreateCopy() as BusyTimeLine;
            retValue._CalendarEventRange = this.getCalendarEventRange.CreateCopy();
            retValue._Name = this.getName;
            retValue._EventDuration = this.getActiveDuration;
            retValue._Complete = this.getIsComplete;
            retValue._ConflictingEvents = this.Conflicts;
            retValue._DataBlob = this.Notes;
            retValue._Enabled = this.isEnabled;
            retValue.updateEndTime( this.End);
            retValue._EventPreDeadline = this.getPreDeadline;
            retValue.EventScore = this.Score;
            retValue.isRestricted = this.getIsEventRestricted;
            retValue._LocationInfo = (this._LocationInfo == null) ? Location.getNullLocation() : this._LocationInfo.CreateCopy();
            retValue.OldPreferredIndex = this.OldUniversalIndex;
            retValue.preferredDayIndex = this.UniversalDayIndex;
            retValue._PrepTime = this.getPreparation;
            retValue._Priority = this.getEventPriority;
            retValue._ProfileOfNow = this._ProfileOfNow;
            retValue._RigidSchedule = this._RigidSchedule;
            retValue.updateStartTime( this.Start);
            retValue._UiParams = this.getUIParam;
            retValue.UniqueID = this.SubEvent_ID;
            retValue._AutoDeleted = this.getIsUserDeleted;
            retValue._Users = this.getAllUsers();
            retValue.Vestige = this.isVestige;
            retValue._otherPartyID = this._otherPartyID;
            retValue._LocationValidationId = this._LocationValidationId;
            return retValue;
        }

        public static void updateMiscData(IList<SubCalendarEvent>AllSubCalendarEvents, IList<int> IntData)
        {
            if(AllSubCalendarEvents.Count!=IntData.Count)
            {
                throw new Exception("trying to update MiscData  while Subcalendar events with not matching count of intData");
            }
            else
            {
                for(int i=0;i<AllSubCalendarEvents.Count;i++)
                {
                    AllSubCalendarEvents[i].MiscIntData=IntData[i];
                }
            }
        }

        public static void incrementMiscdata(IList<SubCalendarEvent> AllSubCalendarEvents)
        {
            for (int i = 0; i < AllSubCalendarEvents.Count; i++)
            {
                ++AllSubCalendarEvents[i].MiscIntData;// = IntData[i];
            }
        }

        public static void decrementMiscdata(IList<SubCalendarEvent> AllSubCalendarEvents)
        {
            for (int i = 0; i < AllSubCalendarEvents.Count; i++)
            {
                --AllSubCalendarEvents[i].MiscIntData;// = IntData[i];
            }
        }



        public static void updateMiscData(IList<SubCalendarEvent> AllSubCalendarEvents, int IntData)
        {
            for (int i = 0; i < AllSubCalendarEvents.Count; i++)
            {
                AllSubCalendarEvents[i].MiscIntData = IntData;
            }
        }

        virtual public bool PinToEnd(TimeLine LimitingTimeLine)
        {
            if (this.isLocked)
            {
                return (LimitingTimeLine.IsTimeLineWithin(this.StartToEnd));
            }

            DateTimeOffset ReferenceTime = this.getCalculationRange.End;
            if (ReferenceTime > LimitingTimeLine.End)
            {
                ReferenceTime = LimitingTimeLine.End;
            }

            DateTimeOffset MyStartTime = ReferenceTime - this._EventDuration;


            if ((MyStartTime>=LimitingTimeLine.Start )&&(MyStartTime>=getCalculationRange.Start))
            {

                updateStartTime( MyStartTime);
                //ActiveSlot = new BusyTimeLine(this.ID, (MyStartTime), ReferenceTime);
                TimeSpan BusyTimeLineShift = MyStartTime - ActiveSlot.Start;
                ActiveSlot.shiftTimeline(BusyTimeLineShift);
                updateEndTime( ReferenceTime);
                return true;
            }

            updateStartTime(ActiveSlot.Start);
            updateEndTime( ActiveSlot.End);
            return false;
        }

        /// <summary>
        /// Shifts a subcalendar event by the specified "ChangeInTime". Function returns a false if the change in time will not fall within calendarevent range. It returns true if successful. The force variable makes the subcalendareventignore the check for fitting in the calendarevent range
        /// </summary>
        /// <param name="ChangeInTime"></param>
        /// <param name="force">Sift the sub event even though it outside the subevent timeline</param>
        /// <param name="lockToId">the subevent won't get shifted when the UI clean up runs</param>
        /// <returns></returns>
        virtual public bool shiftEvent(TimeSpan ChangeInTime, bool force=false, bool lockToId = false)
        {
            if (force)
            {
                updateStartTime( Start + ChangeInTime);
                updateEndTime( End + ChangeInTime);
                ActiveSlot.shiftTimeline(ChangeInTime);
                _LockToId = lockToId;
                return true;
            }
            TimeLine UpdatedTimeLine = new TimeLine(this.Start + ChangeInTime, this.End + ChangeInTime);
            if (!(this.getCalculationRange.IsTimeLineWithin(UpdatedTimeLine)))
            {
                return false;
            }
            else
            {
                updateStartTime( Start + ChangeInTime);
                updateEndTime(End + ChangeInTime);
                ActiveSlot.shiftTimeline(ChangeInTime);
                _LockToId = lockToId;
                return true;
            }
        }

        virtual public bool shiftEvent(DateTimeOffset newStartTime, bool force = false)
        {
            return shiftEvent(newStartTime - this.Start, force);
        }

        public static double CalculateDistanceOfSubEventsWithSameCalendarEvent(IList<SubCalendarEvent> Allevents, double distanceMultiplier)
        {
            double retValue = 0;
            HashSet<string> allIds = new HashSet<string>(Allevents.Select(obj => obj.UniqueID.getCalendarEventComponent()));
            if (allIds.Count != 1)
            {
                throw new Exception("Calculation of distance with subeevnts with different calendart event ids");
            }
            if (Allevents.Count > 0)
            {
                retValue = Utility.getFibonacciSumToIndex((uint)Allevents.Count - 2);
                retValue*= distanceMultiplier;
            }
            return retValue;
        }


        public static double CalculateDistance(SubCalendarEvent Arg1,SubCalendarEvent Arg2, double worstDistance=double.MaxValue)
        {
            if (Arg1.SubEvent_ID.getIDUpToCalendarEvent() == Arg2.SubEvent_ID.getIDUpToCalendarEvent())
            {
                return worstDistance;
            }
            else
            {
                return Location.calculateDistance(Arg1.Location, Arg2.Location, worstDistance);
            }
        }

        /// <summary>
        /// Function calculates the total distance  by multiple sub calendar events. When SubEvents withtin the same Calendar event are ordered consecutively the distance between them is assigned the worst value. Note calculation uses double.minvalue to determine if this is a defaultentry;
        /// </summary>
        /// <param name="Allevents"></param>
        /// <param name="worstDistance"></param>
        /// <returns></returns>
         public static double CalculateDistance(IList<SubCalendarEvent> Allevents, double worstDistance=double.MinValue, bool useFibonnacci = true)
         {
            double retValue = 0;
            double distance = 0;
            double distanceMultiplier = 0;
            double multiplierCounter = 0;
            if (Allevents.Count >= 2)
            {
                if (worstDistance == double.MinValue)
                {
                    worstDistance = double.MaxValue / (Allevents.Count - 1);
                }
                if (useFibonnacci)
                {
                    bool reInitempList = false;
                    List<List<SubCalendarEvent>> subEventGroups = new List<List<SubCalendarEvent>>();
                    List<SubCalendarEvent> tempList = new List<SubCalendarEvent>();
                    SubCalendarEvent previousSubEvent = Allevents.First();
                    for (int i = 1; i < Allevents.Count - 1; i++)
                    {
                        SubCalendarEvent currentSubEvent = Allevents[i];
                        if (previousSubEvent.UniqueID.getCalendarEventComponent() == currentSubEvent.UniqueID.getCalendarEventComponent())
                        {
                            tempList.Add(previousSubEvent);
                            tempList.Add(currentSubEvent);
                            reInitempList = true;
                        }
                        else
                        {
                            if (reInitempList)
                            {
                                subEventGroups.Add(tempList);
                                tempList = new List<SubCalendarEvent>();
                                reInitempList = false;
                            }
                            //else
                            {
                                ++multiplierCounter;
                                distance = CalculateDistance(currentSubEvent, previousSubEvent, worstDistance);
                                if(distance == worstDistance)
                                {
                                    distanceMultiplier += 1;
                                }
                                else
                                {
                                    distanceMultiplier += distance;
                                }
                                retValue += distance;
                            }
                        }
                        previousSubEvent = currentSubEvent;
                    }

                    distanceMultiplier /= multiplierCounter;
                    subEventGroups.ForEach(listOfSubEvents => {
                        double fibboDIstance = CalculateDistanceOfSubEventsWithSameCalendarEvent(listOfSubEvents, distanceMultiplier);
                        retValue += fibboDIstance;
                    });
                    
                }
                else
                {
                    int j = 0;
                    for (int i = 0; i < Allevents.Count - 1; i++)
                    {
                        j = i + 1;
                        retValue += CalculateDistance(Allevents[i], Allevents[j], worstDistance);
                    }
                }
                return retValue;
            }
            return retValue;
         }

        override public void updateTimeLine(TimeLine timeLine)
        {
            updateStartTime( timeLine.Start);
            updateEndTime( timeLine.End);
            BusyFrame = new BusyTimeLine(this.Id, timeLine.CreateCopy());
        }

         virtual public bool canExistWithinTimeLine(TimeLine PossibleTimeLine)
         {
            bool retValue = false;
            if (!this.isLocked)
            {
                DateTimeOffset start = this.Start;
                DateTimeOffset end = this.End;
                retValue = (this.PinToStart(PossibleTimeLine) && this.PinToEnd(PossibleTimeLine));
                updateStartTime( start);
                updateEndTime( end);
                BusyFrame = new BusyTimeLine(this.Id, start, end);
            }
            else
            {
                retValue = PossibleTimeLine.IsTimeLineWithin(this.StartToEnd);
            }
            return retValue;
        }

        virtual public bool canExistTowardsEndWithoutSpace(TimeLine PossibleTimeLine)
         {
             TimeLine ParentCalRange = getCalculationRange;
             bool retValue = (ParentCalRange.Start <= (PossibleTimeLine.End - getActiveDuration)) && (ParentCalRange.End>=PossibleTimeLine.End)&&(canExistWithinTimeLine(PossibleTimeLine));
             return retValue;
         }

        static public bool isConflicting(SubCalendarEvent firstEvent, SubCalendarEvent secondEvent)
        {
            bool retValue = firstEvent.StartToEnd.InterferringTimeLine(secondEvent.StartToEnd) != null;
            return retValue;
        }

         virtual public bool canExistTowardsStartWithoutSpace(TimeLine PossibleTimeLine)
         {
             TimeLine ParentCalRange = getCalculationRange;
             bool retValue = ((PossibleTimeLine.Start + getActiveDuration) <= ParentCalRange.End) && (ParentCalRange.Start <= PossibleTimeLine.Start) && (canExistWithinTimeLine(PossibleTimeLine));
             return retValue;
         }
         /// <summary>
         /// Function returns the largest Timeline interferes with its calendar event range. If restricted subcalevent you can use the orderbystart to make a preference for selection. Essentiall select the largest time line with earliest start time
         /// </summary>
         /// <param name="TimeLineData"></param>
         /// <returns></returns>
         virtual public List<TimeLine> getTimeLineInterferringWithCalEvent(TimeLine TimeLineData, bool orderByStart = true)
         {
             TimeLine retValuTimeLine= getCalculationRange.InterferringTimeLine(TimeLineData);;
             List<TimeLine> retValue = null;
             if (retValuTimeLine!=null)
             {
                 retValue = new List<TimeLine>() { retValuTimeLine };
             }
             return retValue;
         }

        virtual public DateTimeOffset getPauseTime()
        {
            return _PauseTime;
        }
        virtual internal TimeSpan Pause(DateTimeOffset currentTime)
        {
            _PauseTime = currentTime;
            DateTimeOffset Start = this.Start;
            DateTimeOffset End = this.End;
            TimeSpan NewUsedTime = _PauseTime - Start;

            _UsedTime = NewUsedTime;
            return NewUsedTime;
        }

        virtual internal bool Continue(DateTimeOffset currentTime)
        {
            _PauseTime = new DateTimeOffset();
            TimeSpan timeDiff = (currentTime- UsedTime) - (Start);
            bool RetValue = shiftEvent(timeDiff);
            return RetValue;
        }

        virtual public bool UnPause(DateTimeOffset currentTime)
        {
            _PauseTime = new DateTimeOffset();
            _UsedTime = new TimeSpan();
            TimeSpan timeDiff = new TimeSpan();
            bool RetValue = shiftEvent(timeDiff);
            return RetValue;
        }

        public override void InitializeDayPreference(TimeLine timeLine)
        {
            if (_DaySectionPreference == null)
            {
                _DaySectionPreference = ParentCalendarEvent.DayPreference.toTimeOfDayPreference(timeLine);
            }
            base.InitializeDayPreference(timeLine);
        }



        public ulong UniversalDayIndex
         {
             get
             {
                 return preferredDayIndex;
             }
         }

        public void enableCalculationMode()
        {
            CalculationMode = true;
        }

        public void disableCalculationMode()
        {
            CalculationMode = false;
        }

        public override bool isLocked => base.isLocked || this.tempLock || this.lockedPrecedingHours;

        /// <summary>
        /// This changes the duration of the subevent. It requires the change in duration. This just adds/subtracts the delta to the end time
        /// </summary>
        /// <param name="Delta"></param>
        public virtual void addDurartion(TimeSpan Delta)
         {
             TimeSpan NewEventDuration = _EventDuration.Add(Delta);
             if (NewEventDuration > new TimeSpan(0))
             {
                 _EventDuration = NewEventDuration;
                 updateEndTime( Start.Add(_EventDuration));
                 BusyFrame.updateBusyTimeLine(new BusyTimeLine(getId, ActiveSlot.Start, ActiveSlot.Start.Add(_EventDuration)));
                 return;
             }
             throw new Exception("You are trying to reduce the Duration length to Less than zero");

         }

        internal virtual void changeCalendarEventRange(TimeLine newTimeLine, bool resetCalculationTimeLine = true)
        {
            _CalendarEventRange = newTimeLine.CreateCopy();
            if(resetCalculationTimeLine)
            {
                CalculationTimeLine = null;
            }
        }

         public void updateUnusables(ulong unwantedIndex)
         {
             UnUsableIndex = unwantedIndex;
         }

         public ulong getUnUsableIndex()
         {
             return UnUsableIndex;
         }

         public ulong resetAndgetUnUsableIndex()
         {
             ulong retValue = UnUsableIndex;
             UnUsableIndex = 0;
             return retValue;
         }
        

        public static void updateUnUsable(IEnumerable<SubCalendarEvent>SubEVents,ulong UnwantedIndex)
        {
            SubEVents.AsParallel().ForAll(obj=>{obj.UnUsableIndex=UnwantedIndex;});
        }

        /// <summary>
        /// Function sets the subevent as Rigid. Note: this can be different from locked
        /// </summary>
        /// <param name="calEvent"></param>
        internal void RigidizeEvent(CalendarEvent calEvent)
        {
            bool lockChangeAllowed = false;
            if((calEvent)!=null 
                && (
                    ((ParentCalendarEvent!=null) && ParentCalendarEvent.Id == calEvent.Id ) || 
                    ((RepeatParentEvent != null) && RepeatParentEvent.Id == calEvent.Id)
                )
            )
            {
                lockChangeAllowed = true;
            }

            if(lockChangeAllowed)
            {
                _RigidSchedule = true;
            }
            else
            {
                throw new Exception("Tried modifying the rigid status, but cannot validate the ParentCalendar Event or Repeat parent calendarevent");
            }
        }

        /// <summary>
        /// Function makes subevent non rigid. Note: this can be different from locked
        /// </summary>
        /// <param name="calEvent"></param>
        internal void UnRigidizeEvent(CalendarEvent calEvent)
        {
            bool lockChangeAllowed = false;
            if ((calEvent) != null
                && (
                    ((ParentCalendarEvent != null) && ParentCalendarEvent.Id == calEvent.Id) ||
                    ((RepeatParentEvent != null) && RepeatParentEvent.Id == calEvent.Id)
                )
            )
            {
                lockChangeAllowed = true;
            }

            if (lockChangeAllowed)
            {
                _RigidSchedule = false;
            }
            else
            {
                throw new Exception("Tried modifying the rigid status, but cannot validate the ParentCalendar Event or Repeat parent calendarevent");
            }

            
        }
        #endregion

        #region Class Properties

        /// <summary>
        /// Pathoptimization has been acknowledged on this subevent
        /// </summary>
        public bool isOptimized
        {
            get
            {
                return OptimizationFlag;
            }
        }

        public ulong OldUniversalIndex
        {
            get
            {
                return OldPreferredIndex;
            }

        }

        public bool isDesignated
        {
            get
            {
                bool retValue = preferredDayIndex != ReferenceNow.UndesignatedDayIndex;
                return retValue;
            }
        }
         public virtual ConflictProfile Conflicts
         {
             get
             {
                 return _ConflictingEvents;
             }
         }

        public virtual TimeLine getCalculationRange
        {
            get 
            {
                return CalculationTimeLine ?? (CalculationTimeLine = ParentCalendarEvent.CalculationStartToEnd); 
            }
        }

        public virtual TimeLine getCalendarEventRange
        {
            get
            {
                return _CalendarEventRange ?? ParentCalendarEvent.StartToEnd;
            }
        }

        public double Score
        {
            get 
            {
                return EventScore;
            }
        }

        public int IntData
        {
            get
            {
                return MiscIntData;
            }
        }


        public double fittability
        {
            get
            {
                double retValue = ((double)getCalculationRange.TimelineSpan.Ticks )/ ((double)RangeSpan.Ticks);
                return retValue;
            }
        }
        [NotMapped]
        public BusyTimeLine ActiveSlot
        {
            set
            {
                if (BusyFrame.TimelineSpan != value.TimelineSpan)
                {
                    throw new Exception("New lhs Activeslot isnt the same duration as old active slot. Check for inconsistency in code");
                }
                else 
                {
                    TimeSpan ChangeInTimeSpan = value.Start - BusyFrame.Start;
                    shiftEvent(ChangeInTimeSpan);
                }
                
            }
            get
            {
                return BusyFrame;
            }
        }

        override public DateTimeOffset StartTime_EventDB
        {
            get
            {
                return this.Start;
            }
            set
            {
                updateStartTime( value);
                if (BusyFrame == null)
                {
                    BusyFrame = new BusyTimeLine(this.Id, Start, Start);
                } else {
                    BusyFrame = new BusyTimeLine(this.Id, Start, BusyFrame.End);
                }
            }
        }

        virtual public string CalendarEventId { get; set; }
        [ForeignKey("CalendarEventId")]
        virtual public CalendarEvent ParentCalendarEvent
        {
            set
            {
                _calendarEvent = value;
            }
            get
            {
                return _calendarEvent;
            }
        }

        virtual public DateTimeOffset CalendarEventRangeStart
        {
            set
            {
                _CalendarEventRangeStart = value;
                if(_CalendarEventRangeEnd!=null)
                {
                    _CalendarEventRange = new TimeLine(_CalendarEventRangeStart, _CalendarEventRangeEnd);
                }
            }
            get
            {
                return getCalendarEventRange.Start;
            }
        }

        virtual public DateTimeOffset CalendarEventRangeEnd
        {
            set
            {
                _CalendarEventRangeEnd = value;
                if (_CalendarEventRangeStart != null)
                {
                    _CalendarEventRange = new TimeLine(_CalendarEventRangeStart, _CalendarEventRangeEnd);
                }
            }
            get
            {
                return getCalendarEventRange.End;
            }
        }

        override public DateTimeOffset EndTime_EventDB
        {
            get
            {
                return this.End;
            }
            set
            {
                updateEndTime( value);
                if (BusyFrame == null)
                {
                    BusyFrame = new BusyTimeLine(this.Id, End, End);
                }
                else
                {
                    BusyFrame = new BusyTimeLine(this.Id, BusyFrame.Start, End);
                }
            }
        }

        override public TimeSpan getActiveDuration
        {
            get
            {
                return _EventDuration;
            }
        }

        override public string getId
        {
            get
            {
                return UniqueID.ToString();
            }
        }

        

        public EventID SubEvent_ID
        {
            get
            {
                return UniqueID;//.ToString();
            }
        }

        public virtual  TimeLine StartToEnd
        {
            get
            {
                
                return ActiveSlot;
            }
        }


        public TimeSpan RangeSpan
        {
            get
            {
                return this.StartToEnd.TimelineSpan;
            }
        }

        virtual public bool isBlobEvent
        {
            get
            {
                return BlobEvent;
            }
    }
        

        virtual public MiscData Notes
        { 
            get
            {
                return _DataBlob;
            }
        }

         virtual public bool isVestige
         {
             get 
             {
                 return Vestige;
             }
         }
         
        public bool isInCalculationMode
        {
            get
            {
                return CalculationMode;
            }
        }

        public bool isPaused
        {
            get
            {
                return getPauseTime() != InitialPauseTime;
            }
        }

        public override DateTimeOffset getDeadline
        {
	        get 
	        {
                return getCalendarEventRange.End;
	        }
        }
        public virtual Dictionary<TimeSpan, List<Reason>>  ReasonsForPosiition
        {
            get {
                return ReasonsForCurrentPosition;
            }
        }

        public virtual Dictionary<TimeSpan, List<Reason>> ReasonsOnHistoryforPosition
        {
            get
            {
                return HistoricalReasonsCurrentPosition;
            }
        }
        public override Procrastination Procrastination_EventDB
        {
            get
            {
                return _calendarEvent?.Procrastination_EventDB;
            }
        }

        public override Procrastination getProcrastinationInfo
        {
            get
            {
                return _calendarEvent.getProcrastinationInfo;
            }
        }

        /// <summary>
        /// SInce the function shiftSUbEventsByTimeAndId reorders all sub events by time and Id, meaning the subevent withe lowest alphabetically ordered id gets the earliesttime
        /// This ensures that when the "shiftSUbEventsByTimeAndId" is called the subevent doesn't get reordered from the id.
        /// </summary>
        public virtual bool LockToId
        {
            get
            {
                return _LockToId;
            }
        }

        public override NowProfile getNowInfo
        {
            get
            {
                return _ProfileOfNow?? ParentCalendarEvent.getNowInfo;
            }
        }

        public bool isPre_reschedulingEnabled
        {
            get
            {
                return _enablePre_reschedulingTimelineLockDown;
            }
        }
        #endregion

    }
}


