using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TilerElements
{
    public class SubCalendarEvent : TilerEvent, IDefinedRange
    {
        public static DateTimeOffset InitialPauseTime = Utility.JSStartTime;
        protected BusyTimeLine BusyFrame;
        protected BusyTimeLine TempBusyFrame;
        protected TimeLine _CalendarEventRange;
        protected DateTimeOffset _CalendarEventRangeStart;
        protected DateTimeOffset _CalendarEventRangeEnd;
        protected ConflictProfile _ConflictingEvents = new ConflictProfile();
        protected long preferredDayIndex = 0;
        protected int MiscIntData;
        protected bool Vestige = false;
        protected long UnUsableIndex;
        protected long OldPreferredIndex;
        protected bool CalculationMode = false;
        protected bool BlobEvent = false;
        protected bool OptimizationFlag = false;
        protected List<Reason> TimePositionReasons = new List<Reason>();
        protected DateTimeOffset _LastReasonStartTimeChanged;
        protected TimeLine CalculationTimeLine = null;
        protected CalendarEvent _calendarEvent;
        protected bool _LockToId = false;
        [NotMapped]
        public TimeSpan TravelTimeBefore { get; set; } = new TimeSpan(0);
        [NotMapped]
        public TimeSpan TravelTimeAfter { get; set; } = new TimeSpan(0);

        public bool isWake { get; set; } = false;
        public bool isSleep { get; set; } = false;
        [NotMapped]
        protected bool _RepetitionLock { get; set; } = false; // this is the lock for an event when repeat is clicked
        [NotMapped]
        protected bool _NowLock { get; set; } // This is the lock applied when an event is set as now
        protected bool _PauseLock { get; set;} // This is the lock applied when an event is paused
        protected bool tempLock { get; set; } = false;// This should never get persisted
        [NotMapped]
        protected bool conflictResolutionLock { get; set; } = false;// This should never get persisted, this is locked to artiificially lock a tile when a position for it is found 
        protected bool lockedPrecedingHours { get; set; }// This should never get persisted
        protected bool _enablePre_reschedulingTimelineLockDown { get; set; } = true;// This prevent locking for preceding twentyFour or for interferring with now
        protected bool _isTardy { get; set; } = false;//Named tardy 'cause we fancy like that
        protected TimeSpan _UsedPauseTime = new TimeSpan();
        /// <summary>
        /// This holds the current session reasons. It will updated based on data and calculation optimizations from HistoricalCurrentPosition
        /// </summary>
        protected Dictionary<TimeSpan, List<Reason>> ReasonsForCurrentPosition = new Dictionary<TimeSpan, List<Reason>>();
        /// <summary>
        /// Will hold the reasons that were collated from the last time the schedule was modified. This is to be only loaded from storage and not to be updated
        /// </summary>
        protected Dictionary<TimeSpan, List<Reason>> HistoricalReasonsCurrentPosition = new Dictionary<TimeSpan, List<Reason>>();
        [NotMapped]
        protected List<PausedTimeLine> _pausedTimeSlot = null;

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

        public SubCalendarEvent(CalendarEvent calendarEvent, TilerUser Creator, TilerUserGroup users, string timeZone, TimeSpan Event_Duration, EventName name, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, bool Enabled, EventDisplay UiParam, MiscData Notes, bool completeFlag, Location EventLocation = null, TimeLine calendarEventRange = null, ConflictProfile conflicts = null)
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
            updateEndTime(EventDeadline);
            _EventDuration = Event_Duration;
            _PrepTime = EventPrepTime;
            if (myParentID == "16")
            {
                ;
            }
            _UiParams = UiParam;
            _DataBlob = Notes;
            _Complete = completeFlag;
            UniqueID = EventID.GenerateSubCalendarEvent(myParentID);
            BusyFrame = new BusyTimeLine(this.getId, Start, End);//this is because in current implementation busy frame is the same as CalEvent frame
            this._LocationInfo = EventLocation;
            //            EventSequence = new EventTimeLine(UniqueID.ToString(), StartDateTime, EndDateTime);
            _RigidSchedule = Rigid;
            this._Enabled = Enabled;
            _LastReasonStartTimeChanged = this.Start;
            _calendarEvent = calendarEvent;
            this._IniStartTime = this.Start;
            this._IniEndTime = this.End;
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
            this._IniStartTime = this.Start;
            this._IniEndTime = this.End;
        }
        #endregion

        #region Class functions

        public void resetTardy()
        {
            _isTardy = false;
        }

        public void setAsTardy()
        {
            _isTardy = true;
        }
        public virtual void updateCalculationEventRange(TimeLine timeLine)
        {
            TimeLineRestricted restrictedTimeLine = timeLine as TimeLineRestricted;
            if (restrictedTimeLine != null)
            {
                if (!restrictedTimeLine.IsViable)
                {
                    return;
                }
            }

            TimeLine interferringTimeLine = this.getCalendarEventRange.InterferringTimeLine(timeLine);
            restrictedTimeLine = timeLine as TimeLineRestricted;
            if (restrictedTimeLine != null)
            {
                if (!restrictedTimeLine.IsViable)
                {
                    return;
                }
            }
            if (interferringTimeLine == null)
            {
                this.CalculationTimeLine = timeLine;
            }
            else
            {
                this.CalculationTimeLine = interferringTimeLine;
            }
        }

        public virtual void disable(CalendarEvent myCalEvent, ReferenceNow now)
        {
            if (this._Enabled)
            {
                this._Enabled = false;
                this._AutoDeleted = false;
                this._DeletionTime = now.constNow;
                myCalEvent.incrementDeleteCount(this.getActiveDuration);
            }
        }

        public virtual void autoDisable(CalendarEvent myCalEvent, TilerElements.Reason.AutoDeletion reason)
        {
            if (this._Enabled)
            {
                this._Enabled = false;
                this._DeletionTime = Utility.BeginningOfTime;
                myCalEvent.incrementAutoDeleteCount(this.getActiveDuration);
            }
            this._AutoDeleted = true;
        }

        internal void disableWithoutUpdatingCalEvent()
        {
            this._Enabled = false;
        }

        public virtual void complete(CalendarEvent myCalEvent, ReferenceNow now)
        {
            if (!this._Complete)
            {
                this._Complete = true;
                this._CompletionTime = now.constNow;
                myCalEvent.addCompletionTimes(this.Start);
                myCalEvent.incrementCompleteCount(this.getActiveDuration);
            }
        }

        public void nonComplete(CalendarEvent myCalEvent)
        {
            if (this._Complete)
            {
                this._Complete = false;
                myCalEvent.removeCompletionTimes(this.Start);
                myCalEvent.decrementCompleteCount(this.getActiveDuration);
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
                this._DeletionTime = Utility.BeginningOfTime;
                myCalEvent.decrementDeleteCount(this.getActiveDuration);
            }
        }

        internal void enableWithouUpdatingCalEvent()
        {
            this._Enabled = true;
        }

        protected void updateDayIndex(long dayIndex)
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

        public virtual void SetCompletionStatus(bool completeValue, CalendarEvent myCalendarEvent, ReferenceNow now)
        {
            if (completeValue != _Complete)
            {
                if (completeValue)
                {
                    complete(myCalendarEvent, now);
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


        public void conflictLockSubEvent()
        {
            conflictResolutionLock = true;
        }

        public void resetconflictLock()
        {
            conflictResolutionLock = false;
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

        virtual public void addToPausedTimeSlot(PausedTimeLine pausedTimeLine)
        {
            _pausedTimeSlot.Add(pausedTimeLine);
            _UsedPauseTime = TimeSpan.FromTicks(_pausedTimeSlot.Select(timeLine => timeLine.TimelineSpan.Ticks).Sum());
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

        public override NowProfile initializeNowProfile()
        {
            if (string.IsNullOrEmpty(this.NowProfileId) || string.IsNullOrWhiteSpace(this.NowProfileId))
            {
                _ProfileOfNow = ParentCalendarEvent.initializeNowProfile();
                return _ProfileOfNow;
            }
            throw new Exception("Now profile has already being initialized, try loading profile object to memory");
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
            retValue.updateStartTime(DateTimeOffset.UtcNow.removeSecondsAndMilliseconds());
            retValue.updateEndTime(retValue.Start);
            retValue._EventDuration = new TimeSpan(0);

            retValue._RigidSchedule = true;
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


        virtual public SubCalendarEvent CreateCopy(EventID eventId, CalendarEvent parentCalendarEvent)
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
            SubCalendarEvent copy = new SubCalendarEvent(parentCalendarEvent, getCreator, _Users, this._TimeZone, Id, this.getName?.createCopy(), Start, End, BusyFrame?.CreateCopy() as BusyTimeLine, this._RigidSchedule, this.isEnabled, this._UiParams?.createCopy(), this.Notes?.createCopy(), this._Complete, this._LocationInfo, new TimeLine(getCalendarEventRange.Start, getCalendarEventRange.End), _ConflictingEvents?.CreateCopy());
            copy.ThirdPartyID = this.ThirdPartyID;
            copy._AutoDeleted = this._AutoDeleted;
            copy._isEventRestricted = this._isEventRestricted;
            copy.preferredDayIndex = this.preferredDayIndex;
            copy._Creator = this._Creator;
            copy._Semantics = this._Semantics != null ? this._Semantics.createCopy() : null;
            copy._UsedPauseTime = this._UsedPauseTime;
            copy.OptimizationFlag = this.OptimizationFlag;
            copy._LastReasonStartTimeChanged = this._LastReasonStartTimeChanged;
            copy._DaySectionPreference = this._DaySectionPreference;
            copy._calendarEvent = this._calendarEvent;
            copy.TravelTimeAfter = this.TravelTimeAfter;
            copy.TravelTimeBefore = this.TravelTimeBefore;
            copy.isSleep = this.isSleep;
            copy.isWake = this.isWake;
            copy.userLocked = this._userLocked;
            copy.tempLock = this.tempLock;
            copy.LocationValidationId_DB = this.LocationValidationId_DB;
            copy.lockedPrecedingHours = this.lockedPrecedingHours;
            copy._enablePre_reschedulingTimelineLockDown = this._enablePre_reschedulingTimelineLockDown;
            copy._RepetitionLock = this._RepetitionLock;
            copy._NowLock = this._NowLock;
            copy._PauseLock = this._PauseLock;
            copy.ParentCalendarEvent = parentCalendarEvent;
            copy._isTardy = this._isTardy;
            copy._Priority = this._Priority;
            copy._EventScore = this._EventScore;
            copy.UnUsableIndex = this.UnUsableIndex;
            copy._UsedPauseTime = this._UsedPauseTime;
            copy.OptimizationFlag = this.OptimizationFlag;
            copy._PrepTime = this._PrepTime;
            copy.MiscIntData = this.MiscIntData;
            copy._DeletionTime = this._DeletionTime;
            if (this.CalculationTimeLine != null)
            {
                copy.CalculationTimeLine = this.CalculationTimeLine.CreateCopy();
            }

            return copy;
        }

        internal void designate(ReferenceNow now)
        {
            long dayIndex = now.getDayIndexFromStartOfTime(this.Start);
            updateDayIndex(dayIndex);
        }


        internal virtual void undesignate()
        {
            this.preferredDayIndex = ReferenceNow.UndesignatedDayIndex;
        }

        public void incrementScore(double score)
        {
            _EventScore += score;
        }

        public static void resetScores(IEnumerable<SubCalendarEvent> AllSUbevents)
        {
            AllSUbevents.AsParallel().ForAll(obj => obj._EventScore = 0);
        }

        public static TimeSpan TotalActiveDuration(IEnumerable<SubCalendarEvent> ListOfSubCalendarEvent)
        {
            TimeSpan TotalTimeSpan = new TimeSpan(0);

            foreach (SubCalendarEvent mySubCalendarEvent in ListOfSubCalendarEvent)
            {
                TotalTimeSpan = TotalTimeSpan.Add(mySubCalendarEvent.getActiveDuration);
            }

            return TotalTimeSpan;
        }

        /// <summary>
        /// This pins this subevent to the earliest possible start time of either <paramref name="limitingTimeLine"/> or the getCalculationRange.
        /// </summary>
        /// <param name="limitingTimeLine"></param>
        /// <returns></returns>
        virtual public bool PinToStart(TimeLine limitingTimeLine)
        {
            TimeLine reEvaluatedLimitingTimeLine = limitingTimeLine;
            if (this.isRestricted)
            {
                if(this.getIsEventRestricted)
                {
                    
                    if(!this.Location.isRestricted)
                    {
                        return PinToStartRestricted(limitingTimeLine, this.RestrictionProfile);
                    }
                    else
                    {
                        List<TimeLine> allPossibleTimelines = this.RestrictionProfile.getAllNonPartialTimeFrames(limitingTimeLine).Where(obj => obj.TimelineSpan >= getActiveDuration).OrderBy(obj => obj.Start).ToList();
                        bool pinResult = false;
                        foreach (TimeLine eachTimeline in allPossibleTimelines)
                        {
                            pinResult = PinToStartRestricted(eachTimeline, this.Location.RestrictionProfile);
                            if (pinResult)
                            {
                                break;
                            }
                        }
                        return pinResult;
                    }
                }
                return PinToStartRestricted(limitingTimeLine, this.Location.RestrictionProfile);
            }
            return PinToStartUnrestricted(limitingTimeLine);
        }



        virtual public bool PinToStartUnrestricted(TimeLine limitingTimeLine)
        {
            DateTimeOffset ReferenceStartTime = new DateTimeOffset();
            DateTimeOffset ReferenceEndTime = new DateTimeOffset();

            ReferenceStartTime = limitingTimeLine.Start;
            if (this.getCalculationRange.Start > limitingTimeLine.Start)
            {
                ReferenceStartTime = this.getCalculationRange.Start;
            }

            ReferenceEndTime = this.getCalculationRange.End;
            if (this.getCalculationRange.End > limitingTimeLine.End)
            {
                ReferenceEndTime = limitingTimeLine.End;
            }

            /*foreach (SubCalendarEvent MySubCalendarEvent in MySubCalendarEventList)
            {
                SubCalendarTimeSpan = SubCalendarTimeSpan.Add(MySubCalendarEvent.ActiveDuration);//  you might be able to combine the implementing for lopp with this in order to avoid several loops
            }*/
            TimeSpan TimeDifference = (ReferenceEndTime - ReferenceStartTime);

            if (this.isLocked)
            {
                return (limitingTimeLine.IsTimeLineWithin(this.StartToEnd));
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

            this.updateStartTime(ReferenceStartTime);
            this.updateEndTime(this.Start + this.getActiveDuration);
            //this.ActiveSlot = new BusyTimeLine(this.ID, (this.StartDateTime), this.EndDateTime);
            TimeSpan BusyTimeLineShift = this.Start - ActiveSlot.Start;
            ActiveSlot.shiftTimeline(BusyTimeLineShift);
            return true;
        }

        virtual public bool PinToStartRestricted(TimeLine MyTimeLineEntry, RestrictionProfile _ProfileOfRestriction)
        {
            if (this.isLocked)
            {
                return (MyTimeLineEntry.IsTimeLineWithin(this.StartToEnd));
            }

            TimeLine MyTimeLine = MyTimeLineEntry.InterferringTimeLine(getCalculationRange);
            if (MyTimeLine == null)
            {
                return false;
            }

            bool retValue = false;

            List<TimeLine> allPossibleTimelines = _ProfileOfRestriction.getAllNonPartialTimeFrames(MyTimeLine).Where(obj => obj.TimelineSpan >= getActiveDuration).OrderBy(obj => obj.Start).ToList();

            if (allPossibleTimelines.Count > 0)
            {
                foreach (TimeLine eachTimeline in allPossibleTimelines)
                {
                    TimeLine matchingTimeLine = MyTimeLine.InterferringTimeLine(eachTimeline);
                    retValue |= matchingTimeLine != null;

                    TimeLine RestrictedLimitingFrame = _ProfileOfRestriction.getEarliestActiveFrameAfterBeginning(matchingTimeLine).Item1;
                    if (RestrictedLimitingFrame.TimelineSpan < getActiveDuration)
                    {
                        RestrictedLimitingFrame = _ProfileOfRestriction.getEarliestFullframe(matchingTimeLine);
                    }
                    retValue = PinToStartUnrestricted(RestrictedLimitingFrame);
                    if (retValue)
                    {
                        break;
                    }
                }
            }
            else
            {
                return false;
            }

            return retValue;
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
                    if (interferringTimeLine != null)
                    {
                        if (this.canExistWithinTimeLine(group.TimeLine))
                        {
                            validGroupings.Add(sectionTOGrouping[section]);
                        } else
                        {
                            if (interferringTimeLine.TimelineSpan <= this.getActiveDuration)
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
                this.updateEndTime(SubEventEntry.End);
                this._EventPreDeadline = SubEventEntry.getPreDeadline;
                this._EventScore = SubEventEntry.Score;
                this._isEventRestricted = SubEventEntry.getIsEventRestricted;
                this._LocationInfo = SubEventEntry._LocationInfo;
                this.OldPreferredIndex = SubEventEntry.OldUniversalIndex;
                this._otherPartyID = SubEventEntry.ThirdPartyID;
                this.preferredDayIndex = SubEventEntry.UniversalDayIndex;
                this._PrepTime = SubEventEntry.getPreparation;
                this._Priority = SubEventEntry.getEventPriority;
                this._ProfileOfNow = SubEventEntry._ProfileOfNow;
                //this.RigidSchedule = SubEventEntry.Rigid;
                this.updateStartTime(SubEventEntry.Start);
                this._UiParams = SubEventEntry.getUIParam;
                this.UniqueID = SubEventEntry.SubEvent_ID;
                this._AutoDeleted = SubEventEntry.getIsUserDeleted;
                this._Users = SubEventEntry.getAllUsers();
                this.Vestige = SubEventEntry.isVestige;
                this._otherPartyID = SubEventEntry._otherPartyID;
                this._Creator = SubEventEntry._Creator;
                this._Semantics = SubEventEntry._Semantics;
                this._UsedPauseTime = SubEventEntry._UsedPauseTime;
                this._LocationValidationId = this._LocationValidationId;
                return true;
            }

            throw new Exception("Error Detected: Trying to update SubCalendar Event with non matching ID");
        }

        virtual public SubCalendarEvent getProcrastinationCopy(CalendarEvent CalendarEventData, Procrastination ProcrastinationData)
        {
            SubCalendarEvent retValue = getCalulationCopy();
            /*
            retValue.CalendarEventRange = CalendarEventData.RangeTimeLine;
            TimeSpan SpanShift = ProcrastinationData.PreferredStartTime - retValue.Start;
            */
            retValue._CalendarEventRange = new TimeLine(ProcrastinationData.PreferredStartTime, retValue.getCalendarEventRange.End);
            TimeSpan SpanShift = (retValue.getCalendarEventRange.End - retValue.getActiveDuration) - retValue.Start;
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventData.getId);
            retValue.shiftEvent(ProcrastinationData.PreferredStartTime, true);
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
            retValue.updateEndTime(this.End);
            retValue._EventPreDeadline = this.getPreDeadline;
            retValue._EventScore = this.Score;
            retValue._isEventRestricted = this.getIsEventRestricted;
            retValue._LocationInfo = (this._LocationInfo == null) ? Location.getNullLocation() : this._LocationInfo.CreateCopy();
            retValue.OldPreferredIndex = this.OldUniversalIndex;
            retValue.preferredDayIndex = this.UniversalDayIndex;
            retValue._PrepTime = this.getPreparation;
            retValue._Priority = this.getEventPriority;
            retValue._ProfileOfNow = this._ProfileOfNow;
            retValue._RigidSchedule = this._RigidSchedule;
            retValue.updateStartTime(this.Start);
            retValue._UiParams = this.getUIParam;
            retValue.UniqueID = this.SubEvent_ID;
            retValue._AutoDeleted = this.getIsUserDeleted;
            retValue._Users = this.getAllUsers();
            retValue.Vestige = this.isVestige;
            retValue._otherPartyID = this._otherPartyID;
            retValue._LocationValidationId = this._LocationValidationId;
            retValue._RepetitionLock = this._RepetitionLock;
            retValue._NowLock = this._NowLock;
            retValue._PauseLock = this._PauseLock;
            return retValue;
        }

        public static void updateMiscData(IList<SubCalendarEvent> AllSubCalendarEvents, IList<int> IntData)
        {
            if (AllSubCalendarEvents.Count != IntData.Count)
            {
                throw new Exception("trying to update MiscData  while Subcalendar events with not matching count of intData");
            }
            else
            {
                for (int i = 0; i < AllSubCalendarEvents.Count; i++)
                {
                    AllSubCalendarEvents[i].MiscIntData = IntData[i];
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

        /// <summary>
        /// This pins the sub event to the latest possible time based on either the endtime of <paramref name="limitingTimeLine"/> or the calculationRangeTimeLine
        /// </summary>
        /// <param name="limitingTimeLine"></param>
        /// <returns></returns>
        virtual public bool PinToEnd(TimeLine limitingTimeLine)
        {
            TimeLine reEvaluatedLimitingTimeLine = limitingTimeLine;
            if (this.isRestricted)
            {
                if (this.getIsEventRestricted)
                {

                    if (!this.Location.isRestricted)
                    {
                        return PinToEndRestricted(limitingTimeLine, this.RestrictionProfile);
                    }
                    else
                    {
                        List<TimeLine> allPossibleTimelines = this.RestrictionProfile.getAllNonPartialTimeFrames(limitingTimeLine).Where(obj => obj.TimelineSpan >= getActiveDuration).OrderByDescending(obj => obj.End).ToList();
                        bool pinResult = false;
                        foreach (TimeLine eachTimeline in allPossibleTimelines)
                        {
                            pinResult = PinToEndRestricted(eachTimeline, this.Location.RestrictionProfile);
                            if (pinResult)
                            {
                                break;
                            }
                        }
                        return pinResult;
                    }
                }
                return PinToEndRestricted(limitingTimeLine, this.Location.RestrictionProfile);
            }

            return PinToEndUnrestricted(limitingTimeLine);
        }


        virtual protected bool PinToEndUnrestricted(TimeLine LimitingTimeLine)
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


            if ((MyStartTime >= LimitingTimeLine.Start) && (MyStartTime >= getCalculationRange.Start))
            {

                updateStartTime(MyStartTime);
                //ActiveSlot = new BusyTimeLine(this.ID, (MyStartTime), ReferenceTime);
                TimeSpan BusyTimeLineShift = MyStartTime - ActiveSlot.Start;
                ActiveSlot.shiftTimeline(BusyTimeLineShift);
                updateEndTime(ReferenceTime);
                return true;
            }

            updateStartTime(ActiveSlot.Start);
            updateEndTime(ActiveSlot.End);
            return false;
        }


        virtual protected bool PinToEndRestricted(TimeLine LimitingTimeLineData, RestrictionProfile restrictionProfile)
        {
            if (this.isLocked)
            {
                return (LimitingTimeLineData.IsTimeLineWithin(this.StartToEnd));
            }

            TimeLine LimitingTimeLine = LimitingTimeLineData.InterferringTimeLine(getCalculationRange);
            if (LimitingTimeLine == null)
            {
                return false;
            }

            bool retValue = false;

            List<TimeLine> allPossibleTimelines = restrictionProfile.getAllNonPartialTimeFrames(LimitingTimeLine).Where(obj => obj.TimelineSpan >= getActiveDuration).OrderByDescending(obj => obj.End).ToList();
            if (allPossibleTimelines.Count > 0)
            {
                foreach (TimeLine eachTimeline in allPossibleTimelines)
                {
                    TimeLine matchingTimeLine = LimitingTimeLine.InterferringTimeLine(eachTimeline);
                    retValue |= matchingTimeLine != null;

                    TimeLine RestrictedLimitingFrame = restrictionProfile.getLatestActiveTimeFrameBeforeEnd(LimitingTimeLine).Item1;
                    if (RestrictedLimitingFrame.TimelineSpan < getActiveDuration)
                    {
                        RestrictedLimitingFrame = restrictionProfile.getLatestFullFrame(LimitingTimeLine);
                    }
                    retValue = PinToStartUnrestricted(RestrictedLimitingFrame);
                    if (retValue)
                    {
                        break;
                    }
                }
            }
            else
            {
                return false;
            }

            
            return retValue;
        }

        /// <summary>
        /// Shifts a subcalendar event by the specified "ChangeInTime". Function returns a false if the change in time will not fall within calendarevent range. It returns true if successful. The force variable makes the subcalendareventignore the check for fitting in the calendarevent range
        /// </summary>
        /// <param name="ChangeInTime"></param>
        /// <param name="force">Sift the sub event even though it outside the subevent timeline</param>
        /// <param name="lockToId">the subevent won't get shifted when the UI clean up runs</param>
        /// <returns></returns>
        virtual public bool shiftEvent(TimeSpan ChangeInTime, bool force = false, bool lockToId = false)
        {
            if (force)
            {
                updateStartTime(Start + ChangeInTime);
                updateEndTime(End + ChangeInTime);
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
                updateStartTime(Start + ChangeInTime);
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
                retValue *= distanceMultiplier;
            }
            return retValue;
        }


        public static double CalculateDistance(SubCalendarEvent Arg1, SubCalendarEvent Arg2, double worstDistance = double.MaxValue)
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
        public static double CalculateDistance(IList<SubCalendarEvent> Allevents, double worstDistance = double.MinValue, bool useFibonnacci = true)
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
                                if (distance == worstDistance)
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

        override public void updateTimeLine(TimeLine timeLine, ReferenceNow now = null)
        {
            updateStartTime(timeLine.Start);
            updateEndTime(timeLine.End);
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
                updateStartTime(start);
                updateEndTime(end);
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
            bool retValue = (ParentCalRange.Start <= (PossibleTimeLine.End - getActiveDuration)) && (ParentCalRange.End >= PossibleTimeLine.End) && (canExistWithinTimeLine(PossibleTimeLine));
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
        /// Function returns the largest Timeline that interferes with the calculation range. If this is a restricted subcalevent you can use the orderbystart to make a preference for selection. Essentially select the largest time line with earliest start time
        /// </summary>
        /// <param name="TimeLineData"></param>
        /// <returns></returns>
        virtual public List<TimeLine> getTimeLinesInterferringWithCalculationRange(TimeLine TimeLineData, bool orderByStart = true)
        {
            TimeLine retValuTimeLine = getCalculationRange.InterferringTimeLine(TimeLineData); ;
            List<TimeLine> retValue = null;
            if (retValuTimeLine != null)
            {
                retValue = new List<TimeLine>() { retValuTimeLine };
            }
            return retValue;
        }

        /// <summary>
        /// Function returns the largest Timeline that interferes with its calendar event range(If you want only calculation range use). If this is a restricted subcalevent you can use the orderbystart to make a preference for selection. Essentially select the largest time line with earliest start time
        /// </summary>
        /// <param name="TimeLineData"></param>
        /// <returns></returns>
        virtual public List<TimeLine> getTimeLineInterferringWithCalEvent(TimeLine TimeLineData, bool orderByStart = true)
        {
            TimeLine retValuTimeLine = getCalendarEventRange.InterferringTimeLine(TimeLineData); ;
            List<TimeLine> retValue = null;
            if (retValuTimeLine != null)
            {
                retValue = new List<TimeLine>() { retValuTimeLine };
            }
            return retValue;
        }

        /// <summary>
        /// Pauses this subevent. Locks the timeline of the beginning of the timespan to the current time of the subevent
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        virtual internal TimeSpan Pause(DateTimeOffset currentTime)
        {
            DateTimeOffset Start = this.Start;
            DateTimeOffset End = this.End;
            EventID pauseEventId = EventID.GeneratePauseId(this.SubEvent_ID);
            PausedTimeLine pauseTimeLine = new PausedTimeLine(pauseEventId.ToString() , Start, currentTime);
            addToPausedTimeSlot(pauseTimeLine);
            setPauseLock();
            return pauseTimeLine.TimelineSpan;
        }

        virtual protected void setPauseLock()
        {
            _PauseLock = true;
        }

        virtual public void disablePauseLock()
        {
            _PauseLock = false;
        }

        /// <summary>
        /// Resumes a subevent. This takes the rest of the available timeline after being paused and pins it to currentTime 
        /// </summary>
        /// <param name="currentTime"></param>
        /// <param name="forceOutSideDeadlinecurrentTime">force the resume even if is outside the deadlie of the calendar event</param>
        /// <returns></returns>
        virtual internal bool Continue(DateTimeOffset currentTime, bool forceOutSideDeadline = false)
        {
            TimeSpan timeDiff = (currentTime - UsedPauseTime) - (Start);
            bool RetValue = shiftEvent(timeDiff, force:forceOutSideDeadline);// NOTE WE DO NOT WANT TO DISABLE THE PAUSE LOCK, this because even after a subevent is continued it needs to stay locked so it wont get shifted
            
            
            return RetValue;
        }
        /// <summary>
        /// This resets all attributes related to the pausing of a sub event. Note this is not the same as the function Continue.
        /// This does not resume the event it just clears all paused parameters so this subevent doesnt seem paused
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        virtual public bool ResetPause(DateTimeOffset currentTime)
        {
            _pausedTimeSlot = new List<PausedTimeLine>();
            _UsedPauseTime = new TimeSpan();
            disablePauseLock();
            TimeSpan timeDiff = new TimeSpan();
            bool RetValue = shiftEvent(timeDiff);
            return RetValue;
        }

        public long UniversalDayIndex
        {
            get
            {
                return preferredDayIndex;
            }
        }

        public override void InitializeDayPreference(TimeLine timeLine)
        {
            if (_DaySectionPreference == null && !ParentCalendarEvent.isRigid)
            {
                _DaySectionPreference = ParentCalendarEvent.DayPreference.toTimeOfDayPreference(timeLine);
            }
            base.InitializeDayPreference(timeLine);
        }

        public void enableCalculationMode()
        {
            CalculationMode = true;
        }

        public void disableCalculationMode()
        {
            CalculationMode = false;
        }

        public virtual void enableNowLock()
        {
            _NowLock = true;
        }

        public virtual void disableNowLock()
        {
            _NowLock = false;
        }

        public virtual void enableRepetitionLock()
        {
            _RepetitionLock = true;
        }

        public virtual void disableRepetitionLock()
        {
            _RepetitionLock = false;
        }


        /// <summary>
        /// This checks if the parent Calendarevent for a subcalendarevent is complete.
        /// </summary>
        public override bool isParentComplete
        {
            get
            {
                bool retValue = false;
                if (this.ParentCalendarEvent != null)
                {
                    retValue = this.ParentCalendarEvent.getIsComplete;
                }
                if (!retValue && this.ParentCalendarEvent != null)
                {
                    if (!retValue && this.ParentCalendarEvent.RepeatParentEvent != null)
                    {
                        retValue |= this.ParentCalendarEvent.isParentComplete;
                        if (!retValue && this.ParentCalendarEvent.RepeatParentEvent != null)
                        {
                            retValue |= this.ParentCalendarEvent.RepeatParentEvent.getIsComplete;
                            retValue |= this.ParentCalendarEvent.RepeatParentEvent.isParentComplete;
                        }
                    }
                }

                if (!retValue && this.RepeatParentEvent != null)
                {
                    retValue |= this.RepeatParentEvent.isParentComplete;
                }
                return retValue;
            }
        }

        public override bool getIsComplete
        {
            get
            {
                bool retValue = base.getIsComplete || this.isParentComplete;
                return retValue;
            }
        }

        public override bool isLocked => base.isLocked || this.tempLock || this.lockedPrecedingHours || this.isRepetitionLocked || this.isNowLocked||this.isPauseLocked|| this.conflictResolutionLock;

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
                updateEndTime(Start.Add(_EventDuration));
                BusyFrame.updateBusyTimeLine(new BusyTimeLine(getId, ActiveSlot.Start, ActiveSlot.Start.Add(_EventDuration)));
                return;
            }
            throw new Exception("You are trying to reduce the Duration length to Less than zero");

        }

        internal virtual void changeCalendarEventRange(TimeLine newTimeLine, bool resetCalculationTimeLine = true)
        {
            _CalendarEventRange = newTimeLine.CreateCopy();
            if (resetCalculationTimeLine)
            {
                CalculationTimeLine = null;
            }
        }

        public void updateUnusables(long unwantedIndex)
        {
            UnUsableIndex = unwantedIndex;
        }

        public long getUnUsableIndex()
        {
            return UnUsableIndex;
        }

        public long resetAndgetUnUsableIndex()
        {
            long retValue = UnUsableIndex;
            UnUsableIndex = 0;
            return retValue;
        }


        public static void updateUnUsable(IEnumerable<SubCalendarEvent> SubEVents, long UnwantedIndex)
        {
            SubEVents.AsParallel().ForAll(obj => { obj.UnUsableIndex = UnwantedIndex; });
        }

        /// <summary>
        /// Function sets the subevent as Rigid. Note: this can be different from locked
        /// </summary>
        /// <param name="calEvent"></param>
        internal void RigidizeEvent(CalendarEvent calEvent)
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
        /// Is the subevent late
        /// </summary>
        public bool isTardy
        {
            get
            {
                return _isTardy;
            }
        }

        /// <summary>
        /// Is the subevent late
        /// </summary>
        public bool isOntime
        {
            get
            {
                return !isTardy;
            }
        }

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

        public long OldUniversalIndex
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
                return _EventScore;
            }
        }

        public int IntData
        {
            get
            {
                return MiscIntData;
            }
        }


        public override bool IsFromRecurring
        {
            get
            {
                return this.ParentCalendarEvent?.IsFromRecurring ?? base.IsFromRecurring;
            }
        }
        [NotMapped]
        public List<PausedTimeLine> pausedTimeLines
        {
            get
            {
                return _pausedTimeSlot;
            }
        }

        public double fittability
        {
            get
            {
                double retValue = ((double)getCalculationRange.TimelineSpan.Ticks) / ((double)getActiveDuration.Ticks);
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

        virtual public TimeSpan UsedPauseTime
        {
            get
            {
                return _UsedPauseTime;
            }
        }

        virtual public long UsedPauseTime_DB
        {
            set
            {
                this._UsedPauseTime = TimeSpan.FromMilliseconds(value);
            }

            get
            {
                return (long)_UsedPauseTime.TotalMilliseconds;
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
                updateStartTime(value);
                if (BusyFrame == null)
                {
                    BusyFrame = new BusyTimeLine(this.Id, Start, Start);
                } else {
                    BusyFrame = new BusyTimeLine(this.Id, Start, BusyFrame.End);
                }
            }
        }

        [Index("SubEventToParentCalendarEventId", Order = 0)]
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
                if (_CalendarEventRangeEnd != null)
                {
                    _CalendarEventRange = new TimeLine(_CalendarEventRangeStart, _CalendarEventRangeEnd);
                }
            }
            get
            {
                return getCalendarEventRange.Start;
            }
        }


        virtual public bool RepetitionLock_DB
        {
            set
            {
                _RepetitionLock = value;
            }
            get
            {
                return _RepetitionLock;
            }
        }


        virtual public string PausedTimeSlots_DB
        {
            set
            {
                _pausedTimeSlot = new List<PausedTimeLine>();
                if(value.isNot_NullEmptyOrWhiteSpace())
                {
                    JArray pauseSlots = JArray.Parse(value);
                    foreach (JObject timelineObj in pauseSlots)
                    {
                        PausedTimeLine timeLine = PausedTimeLine.JobjectToTimeLine(timelineObj);
                        _pausedTimeSlot.Add(timeLine);
                    }
                }
            }
            get
            {
                JArray retJValue = new JArray();
                if (_pausedTimeSlot != null && _pausedTimeSlot.Count > 0)
                {
                    foreach (TimeLine timeLine in _pausedTimeSlot)
                    {
                        retJValue.Add(timeLine.ToJson());
                    }

                }
                return retJValue.ToString();
            }
        }

        virtual public bool NowLock_DB
        {
            set
            {
                _NowLock = value;
            }
            get
            {
                return _NowLock;
            }
        }

        virtual public bool PauseLock_DB
        {
            
            set
            {
                _PauseLock= value;
            }
            get
            {
                return _PauseLock;
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

        public virtual long TravelTimeBefore_DB
        {
            get
            {
                return TravelTimeBefore.Ticks;
            }
            set
            {
                TravelTimeBefore = TimeSpan.FromTicks(value);
            }
        }

        public virtual long TravelTimeAfter_DB
        {
            get
            {
                return TravelTimeAfter.Ticks;
            }
            set
            {
                TravelTimeAfter = TimeSpan.FromTicks(value);
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


        virtual public bool isBlobEvent
        {
            get
            {
                return BlobEvent;
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

        public override NowProfile ProfileOfNow_EventDB
        {
            get
            {
                return _ProfileOfNow ?? (_ProfileOfNow = this.ParentCalendarEvent?.getNowInfo);
            }
            set
            {
                _ProfileOfNow = value;
            }
        }

        public virtual bool IsTardy_DB
        {
            get
            {
                return _isTardy;
            }

            set
            {
                _isTardy = value;
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
                return _ProfileOfNow?? ParentCalendarEvent.getNowInfo ?? RepeatParentEvent?.getNowInfo;
            }
        }

        public bool isPre_reschedulingEnabled
        {
            get
            {
                return _enablePre_reschedulingTimelineLockDown;
            }
        }

        public bool isRepetitionLocked
        {
            get
            {
                return _RepetitionLock;
            }
        }

        public bool isNowLocked
        {
            get
            {
                return _NowLock;
            }
        }

        public bool isPauseLocked
        {
            get
            {
                return _PauseLock;
            }
        }

        public override JObject Json
        {
            get
            {
                JObject retValue = base.Json;
                retValue.Add("SubCalCalEventStart", this.CalendarEventRangeStart.ToUnixTimeMilliseconds());
                retValue.Add("SubCalCalEventEnd", this.CalendarEventRangeEnd.ToUnixTimeMilliseconds());
                return retValue;
            }
        }
        #endregion

    }
}


