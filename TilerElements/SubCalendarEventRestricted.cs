using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class SubCalendarEventRestricted : SubCalendarEvent
    {
        protected TimeLine HardCalendarEventRange;//this does not include the restriction
        protected RestrictionProfile _ProfileOfRestriction;

        public TimeLine UndoHardCalendarEventRange;
        public RestrictionProfile UndoProfileOfRestriction;
        protected ReferenceNow _Now;
        #region Constructor
        public SubCalendarEventRestricted(CalendarEventRestricted calendarEvent, 
            TilerUser creator, 
            TilerUserGroup users,  
            string CalEventID, 
            EventName name, 
            DateTimeOffset Start, 
            DateTimeOffset End, 
            RestrictionProfile constrictionProgile, 
            TimeLine HardCalEventTimeRange, 
            bool isEnabled, 
            bool isComplete, 
            ConflictProfile conflictingEvents, 
            bool RigidFlag,
            TimeSpan PrepTimeData,
            TimeSpan PreDeadline, 
            Location Locationdata,
            EventDisplay UiData,
            MiscData Notes,
            ReferenceNow now,
            NowProfile nowProfile,
            int Priority = 0,
            string thirdPartyID = "",
            string subEventID = ""
            )
        { 
            isRestricted =true;
            this.updateStartTime( Start);
            this.updateEndTime( End);
            _EventDuration = End - Start;
            _Name = name;
            UniqueID = !string.IsNullOrEmpty(subEventID) && !string.IsNullOrWhiteSpace(subEventID) ? new EventID(subEventID) : EventID.GenerateSubCalendarEvent(CalEventID);
            _ProfileOfRestriction = constrictionProgile;
            HardCalendarEventRange = HardCalEventTimeRange;
            _Now = now;
            initializeCalendarEventRange(_ProfileOfRestriction,HardCalendarEventRange);
            BusyFrame = new BusyTimeLine(UniqueID.ToString(),Start, End);
            _Users = new TilerUserGroup();
            _RigidSchedule = RigidFlag;
            _Complete = isComplete;
            _Enabled = isEnabled;
            _EventPreDeadline = PreDeadline;
            this._Priority = Priority;
            this._LocationInfo = Locationdata;
            _otherPartyID = thirdPartyID;
            this._UiParams = UiData;
            this._ConflictingEvents = conflictingEvents ?? new ConflictProfile();
            _DataBlob = Notes;
            _PrepTime = PrepTimeData;
            _LastReasonStartTimeChanged = this.Start;
            this._Creator = creator;
            this._Users = users;
            _calendarEvent = calendarEvent;
            this._ProfileOfNow = nowProfile;
            this._IniStartDateTime = this.Start;
            this._IniEndDateTime = this.End;
        }

        public SubCalendarEventRestricted()
        {
            isRestricted = true;
            updateStartTime( new DateTimeOffset());
            updateEndTime( new DateTimeOffset());
            _EventDuration = End - Start;
            UniqueID = null;
            _ProfileOfRestriction = null;
            HardCalendarEventRange = null;
            _LastReasonStartTimeChanged = this.Start;
            this._IniStartDateTime = this.Start;
            this._IniEndDateTime = this.End;
        }
        #endregion

        #region Functions

        public override void undoUpdate(Undo undo)
        {
            UndoProfileOfRestriction.undoUpdate(undo);
            UndoHardCalendarEventRange = HardCalendarEventRange.CreateCopy();
            base.undoUpdate(undo);
        }

        public override void undo(string undoId)
        {
            if (UndoId == undoId)
            {
                UndoProfileOfRestriction.undo(undoId);
                Utility.Swap(ref HardCalendarEventRange, ref UndoHardCalendarEventRange);
            }
            base.undo(undoId);
        }

        public override void redo(string undoId)
        {
            if (UndoId == undoId)
            {
                UndoProfileOfRestriction.redo(undoId);
                Utility.Swap(ref HardCalendarEventRange, ref UndoHardCalendarEventRange);
            }
            base.redo(undoId);
        }

        public IEnumerable<TimeLine> getFeasibleTimeLines(TimeLine TimeLineEntry)
        {
            return _ProfileOfRestriction.getAllNonPartialTimeFrames(TimeLineEntry);
        }


        public override bool PinToEnd(TimeLine LimitingTimeLineData)
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

            List<TimeLine> allPossibleTimelines = _ProfileOfRestriction.getAllNonPartialTimeFrames(LimitingTimeLine).Where(obj => obj.TimelineSpan >= getActiveDuration).OrderByDescending(obj => obj.End).ToList();
            if (allPossibleTimelines.Count > 0)
            {
                LimitingTimeLine = LimitingTimeLine.InterferringTimeLine( allPossibleTimelines[0]);
                if (LimitingTimeLine == null)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            TimeLine RestrictedLimitingFrame = _ProfileOfRestriction.getLatestActiveTimeFrameBeforeEnd(LimitingTimeLine).Item1;
            if (RestrictedLimitingFrame.TimelineSpan<getActiveDuration)
            {
                RestrictedLimitingFrame = _ProfileOfRestriction.getLatestFullFrame(LimitingTimeLine);
            }
            bool retValue=base.PinToEnd(RestrictedLimitingFrame);
            return retValue;
        }

        public override bool PinToStart(TimeLine MyTimeLineEntry)
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



            List<TimeLine> allPossibleTimelines = _ProfileOfRestriction.getAllNonPartialTimeFrames(MyTimeLine).Where(obj=>obj.TimelineSpan>=getActiveDuration).OrderBy(obj=>obj.Start).ToList();

            if (allPossibleTimelines.Count > 0)
            {
                MyTimeLine = MyTimeLine.InterferringTimeLine(allPossibleTimelines[0]);
                if (MyTimeLine == null)
                {
                    return false;
                }
            }
            else 
            {
                return false;
            }

            

            TimeLine RestrictedLimitingFrame = _ProfileOfRestriction.getEarliestActiveFrameAfterBeginning(MyTimeLine).Item1;
            if (RestrictedLimitingFrame.TimelineSpan < getActiveDuration)
            {
                RestrictedLimitingFrame = _ProfileOfRestriction.getEarliestFullframe(MyTimeLine);
            }
            bool retValue=base.PinToStart(RestrictedLimitingFrame);
            return retValue;
        }
        /// <summary>
        /// Function initializes the CalendarEventRange. CalendarEventRange is the range for the calendar event. Since this is the restricted class then it sets the timeline to use the earliest possible Start Time and latest possible Datetime to set the rangetimeline.
        /// </summary>
        /// <param name="refTimeLine"></param>
        protected void initializeCalendarEventRange(RestrictionProfile RestrictionData ,TimeLine refTimeLine=null)
        {
            if (refTimeLine == null)
            {
                refTimeLine = HardCalendarEventRange;
            }

            DateTimeOffset myStart = _ProfileOfRestriction.getEarliestStartTimeWithinAFrameAfterRefTime(refTimeLine.Start).Item1.Start;
            DateTimeOffset myEnd = _ProfileOfRestriction.getLatestEndTimeWithinFrameBeforeRefTime(refTimeLine.End).Item1.End;
            _CalendarEventRange = new TimeLineRestricted(myStart, myEnd, RestrictionData, _Now);
        }
        ///*
        public override bool canExistTowardsEndWithoutSpace(TimeLine PossibleTimeLine)
        {
            bool retValue = false;
            List<TimeLine> AllTimeLines = _ProfileOfRestriction.getAllNonPartialTimeFrames(PossibleTimeLine).OrderBy(obj=>obj.Start).ToList();
            if (AllTimeLines.Count > 0)
            {
                return base.canExistTowardsEndWithoutSpace(AllTimeLines.Last());
            }
            else 
            {
                return retValue;
            }
        }

        public override bool canExistTowardsStartWithoutSpace(TimeLine PossibleTimeLine)
        {
            bool retValue = false;
            List<TimeLine> AllTimeLines = _ProfileOfRestriction.getAllNonPartialTimeFrames(PossibleTimeLine).OrderBy(obj => obj.Start).ToList();
            if (AllTimeLines.Count > 0)
            {
                return base.canExistTowardsStartWithoutSpace(AllTimeLines.First());
            }
            else
            {
                return retValue;
            }
        }
        public override bool canExistWithinTimeLine(TimeLine PossibleTimeLine)
        {
            return base.canExistWithinTimeLine(PossibleTimeLine);
        }

        public override SubCalendarEvent CreateCopy(EventID eventId, CalendarEvent parentCalendarEvent)
        {
            SubCalendarEventRestricted copy = new SubCalendarEventRestricted();
            copy.BusyFrame = this.BusyFrame.CreateCopy() as BusyTimeLine;
            copy._CalendarEventRange = getCalendarEventRange.CreateCopy();
            copy._Complete = _Complete;
            copy._ConflictingEvents = this._ConflictingEvents.CreateCopy();
            copy._DataBlob = this._DataBlob?.createCopy();
            copy._Enabled = this._Enabled;
            copy.updateEndTime( this.End);
            copy._EventDuration = this._EventDuration;
            copy._Name = this.getName.createCopy();
            copy._EventPreDeadline = this._EventPreDeadline;
            copy.EventScore = this.EventScore;
            copy.HardCalendarEventRange = this.HardCalendarEventRange?.CreateCopy();
            copy.isRestricted = this.isRestricted;
            copy.Vestige = this.Vestige;
            copy._LocationInfo = this.LocationObj;
            copy.LocationValidationId_DB = this.LocationValidationId_DB;
            copy.MiscIntData = this.MiscIntData;
            copy._otherPartyID = this._otherPartyID;
            copy.preferredDayIndex = this.preferredDayIndex;
            copy._PrepTime = this._PrepTime;
            copy._Priority = this._Priority;
            copy._ProfileOfRestriction = this._ProfileOfRestriction.createCopy();
            copy._RigidSchedule = this._RigidSchedule;
            copy.updateStartTime( this.Start);
            copy._UiParams = this._UiParams?.createCopy();
            if (eventId != null)
            {
                copy.UniqueID = eventId;
            }
            else
            {
                copy.UniqueID = UniqueID;//hack
            }
            copy.ParentCalendarEvent = parentCalendarEvent;
            copy.UnUsableIndex = this.UnUsableIndex;
            copy._AutoDeleted = this._AutoDeleted;
            copy._Users = this._Users;
            copy._Semantics = this._Semantics?.createCopy();
            copy._UsedTime = this._UsedTime;
            copy.OptimizationFlag = this.OptimizationFlag;
            copy.tempLock = this.tempLock;
            copy.lockedPrecedingHours = this.lockedPrecedingHours;
            copy._enablePre_reschedulingTimelineLockDown = this._enablePre_reschedulingTimelineLockDown;
            copy._RepetitionLock = this._RepetitionLock;
            copy.isSleep = this.isSleep;
            copy.isWake = this.isWake;
            copy._isTardy = this._isTardy;
            if (this.CalculationTimeLine != null)
            {
                copy.CalculationTimeLine = this.CalculationTimeLine.CreateCopy();
            }
            return copy;
        }

        public override bool IsDateTimeWithin(DateTimeOffset DateTimeEntry)
        {
            return base.IsDateTimeWithin(DateTimeEntry);
        }

        public override bool shiftEvent(TimeSpan ChangeInTime, bool force = false, bool lockToId = false)
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
            TimeLine myTImeLine =  _ProfileOfRestriction.getLatestActiveTimeFrameBeforeEnd(UpdatedTimeLine).Item1;
            if (myTImeLine.TimelineSpan >= UpdatedTimeLine.TimelineSpan)
            {
                updateStartTime(Start + ChangeInTime);
                updateEndTime(End + ChangeInTime);
                ActiveSlot.shiftTimeline(ChangeInTime);
                _LockToId = lockToId;
                return true;
            }

            myTImeLine = _ProfileOfRestriction.getEarliestActiveFrameAfterBeginning(UpdatedTimeLine).Item1;
            if (myTImeLine.TimelineSpan >= UpdatedTimeLine.TimelineSpan)
            {
                updateStartTime( Start + ChangeInTime);
                updateEndTime(End + ChangeInTime);
                ActiveSlot.shiftTimeline(ChangeInTime);
                _LockToId = lockToId;
                return true;
            }
            return false;
        }


        /*
        public override void updateEventSequence()
        {
            base.updateEventSequence();
        }
        */
        /// <summary>
        /// Function returns the largest Timeline interferes with its calendar event range
        /// </summary>
        /// <param name="TimeLineData"></param>
        /// <returns></returns>
        public override List<TimeLine> getTimeLineInterferringWithCalEvent(TimeLine TimeLineData, bool orderByStart = true)
        {
            List<TimeLine> retValue = null;
            List<TimeLine> possibleTimeLines = orderByStart ? _ProfileOfRestriction.getAllNonPartialTimeFrames(TimeLineData).OrderByDescending(obj => obj.TimelineSpan).ThenBy(obj => obj.Start).ToList() : _ProfileOfRestriction.getAllNonPartialTimeFrames(TimeLineData).OrderByDescending(obj => obj.TimelineSpan).ThenBy(obj => obj.Start).ToList();
            if (possibleTimeLines.Count > 0)
            {
                retValue = possibleTimeLines;
            }
            return retValue;
        }


        public RestrictionProfile RetrictionInfo
        {
            get 
            {
                return _ProfileOfRestriction;
            }
        }
        
        public override bool UpdateThis(SubCalendarEvent SubEventEntryData)
        {
            if ((this.getId == SubEventEntryData.getId) && canExistWithinTimeLine(SubEventEntryData.getCalculationRange))
            {
                SubCalendarEventRestricted SubEventEntry = (SubCalendarEventRestricted)SubEventEntryData;
                this.BusyFrame = SubEventEntry.ActiveSlot;
                this._CalendarEventRange = SubEventEntry.getCalendarEventRange;
                this._Name = SubEventEntry.getName;
                this._EventDuration = SubEventEntry.getActiveDuration;
                this._Complete = SubEventEntry.getIsComplete;
                this._ConflictingEvents = SubEventEntry.Conflicts;
                this._DataBlob = SubEventEntry.Notes;
                this._Enabled = SubEventEntry.isEnabled;
                updateEndTime( SubEventEntry.End);
                this._EventPreDeadline = SubEventEntry.getPreDeadline;
                this.EventScore = SubEventEntry.Score;
                this._LocationInfo = SubEventEntry.Location;
                this.OldPreferredIndex = SubEventEntry.OldUniversalIndex;
                this._otherPartyID = SubEventEntry.ThirdPartyID;
                this.preferredDayIndex = SubEventEntry.UniversalDayIndex;
                this._PrepTime = SubEventEntry.getPreparation;
                this._Priority = SubEventEntry.getEventPriority;
                this._ProfileOfNow = SubEventEntry._ProfileOfNow;
                this._ProfileOfProcrastination = SubEventEntry._ProfileOfProcrastination;
                //this.RigidSchedule = this.rig
                updateStartTime( SubEventEntry.Start);
                this._UiParams = SubEventEntry.getUIParam;
                this.UniqueID = SubEventEntry.SubEvent_ID;
                this._AutoDeleted = SubEventEntry.getIsUserDeleted;
                this._Users = SubEventEntry.getAllUsers();
                this.Vestige = SubEventEntry.isVestige;
                this._otherPartyID = SubEventEntry._otherPartyID;
                this._ProfileOfRestriction = SubEventEntry._ProfileOfRestriction;
                this._Creator = SubEventEntry._Creator;
                this._Semantics = SubEventEntry._Semantics;
                this._UsedTime = SubEventEntry._UsedTime;
                return true;
            }

            throw new Exception("Error Detected: Trying to update SubCalendar Event with non matching ID");
        }

        protected override SubCalendarEvent getCalulationCopy()
        {
            SubCalendarEventRestricted retValue = new SubCalendarEventRestricted();
            retValue.BusyFrame = this.ActiveSlot;
            retValue._CalendarEventRange = this.getCalendarEventRange.CreateCopy();
            retValue._Name = this.getName.createCopy();
            retValue._EventDuration = this.getActiveDuration;
            retValue._Complete = this.getIsComplete;
            retValue._ConflictingEvents = this.Conflicts;
            retValue._DataBlob = this.Notes;
            retValue._Enabled = this.isEnabled;
            retValue.updateEndTime( this.End);
            retValue._EventPreDeadline = this.getPreDeadline;
            retValue.EventScore = this.Score;
            retValue.isRestricted = this.getIsEventRestricted;
            retValue._LocationInfo = this.Location;
            retValue.OldPreferredIndex = this.OldUniversalIndex;
            retValue._otherPartyID = this.ThirdPartyID;
            retValue.preferredDayIndex = this.UniversalDayIndex;
            retValue._PrepTime = this.getPreparation;
            retValue._Priority = this.getEventPriority;
            retValue._ProfileOfNow = this._ProfileOfNow?.CreateCopy();
            retValue._ProfileOfProcrastination = this._ProfileOfProcrastination?.CreateCopy();
            retValue._RigidSchedule = this._RigidSchedule;
            retValue.updateStartTime( this.Start);
            retValue._UiParams = this.getUIParam;
            retValue.UniqueID = this.SubEvent_ID;
            retValue._AutoDeleted = this.getIsUserDeleted;
            retValue._Users = this.getAllUsers();
            retValue.Vestige = this.isVestige;
            retValue._otherPartyID = this._otherPartyID;
            retValue._ProfileOfRestriction = this._ProfileOfRestriction;
            retValue._Now = this._Now;
            retValue._RepetitionLock = this._RepetitionLock;
            return retValue;
        }


        public override SubCalendarEvent getNowCopy(EventID CalendarEventID, NowProfile NowData)
        {
            SubCalendarEventRestricted retValue = (SubCalendarEventRestricted)getCalulationCopy();
            TimeSpan SpanShift = NowData.PreferredTime - retValue.Start;
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventID.ToString());
            retValue.shiftEvent(SpanShift, true);
            retValue._RigidSchedule = true;
            return retValue;
        }

        public void setNow(ReferenceNow now, bool updateCalendarEventRange = false)
        {
            _Now = now;
            if(updateCalendarEventRange)
            {
                initializeCalendarEventRange(this._ProfileOfRestriction);
            }
        }

        public override SubCalendarEvent getProcrastinationCopy(CalendarEvent CalendarEventData, Procrastination ProcrastinationData)
        {
            SubCalendarEvent thisCopy = getCalulationCopy();
            SubCalendarEventRestricted retValue = (SubCalendarEventRestricted)thisCopy;


            retValue.HardCalendarEventRange= new TimeLineRestricted(ProcrastinationData.PreferredStartTime, CalendarEventData.StartToEnd.End,retValue._ProfileOfRestriction, _Now);
            TimeSpan SpanShift = ProcrastinationData.PreferredStartTime - retValue.Start;
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventData.getId);
            retValue.initializeCalendarEventRange(retValue._ProfileOfRestriction, CalendarEventData.StartToEnd);
            retValue.shiftEvent(ProcrastinationData.PreferredStartTime, true);
            return retValue;
        }

        internal override void changeCalendarEventRange(TimeLine newTimeLine, bool resetCalculationTimeLine = true)
        {
            base.changeCalendarEventRange(newTimeLine, resetCalculationTimeLine);
            this.HardCalendarEventRange = _CalendarEventRange;
        }
        //*/
        #endregion

        #region properties

        public RestrictionProfile getRestrictionProfile()
        {
            return _ProfileOfRestriction;
        }

        virtual public DateTimeOffset HardRangeStartTime_EventDB
        {
            get
            {
                return this.HardCalendarEventRange.Start;
            }
            set
            {
                if(this.HardCalendarEventRange == null)
                {
                    this.HardCalendarEventRange = new TimeLine(value, value);
                }
                else
                {
                    this.HardCalendarEventRange = new TimeLine(value, this.HardCalendarEventRange.End);
                    
                }
            }
        }

        virtual public DateTimeOffset HardRangeEndTime_EventDB
        {
            get
            {
                return this.HardCalendarEventRange.End;
            }
            set
            {
                if (this.HardCalendarEventRange == null)
                {
                    this.HardCalendarEventRange = new TimeLine(value, value);
                }
                else
                {
                    this.HardCalendarEventRange = new TimeLine(this.HardCalendarEventRange.Start, value);
                };
            }
        }
        public TimeLine getHardCalendarEventRange
        {
            get
            {
                return HardCalendarEventRange;
            }
        }

        public override RestrictionProfile RestrictionProfile
        {
            get
            {
                return _ProfileOfRestriction;
            }

            set
            {
                _ProfileOfRestriction = value;
            }
        }
        #endregion
    }
}
