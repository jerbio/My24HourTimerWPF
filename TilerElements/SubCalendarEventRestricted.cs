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
        public SubCalendarEventRestricted(CalendarEventRestricted calendarEvent, TilerUser creator, TilerUserGroup users,  string CalEventID, EventName name, DateTimeOffset Start, DateTimeOffset End, RestrictionProfile constrictionProgile, TimeLine HardCalEventTimeRange, bool isEnabled, bool isComplete, ConflictProfile conflictingEvents, bool RigidFlag,TimeSpan PrepTimeData ,TimeSpan PreDeadline, Location Locationdata, EventDisplay UiData, MiscData Notes, ReferenceNow now, int Priority = 0, string thirdPartyID = "", ConflictProfile conflicts = null )
        { 
            isRestricted =true;
            StartDateTime = Start;
            EndDateTime = End;
            _EventDuration = EndDateTime - StartDateTime;
            _Name = name;
            UniqueID = EventID.GenerateSubCalendarEvent(CalEventID);
            _ProfileOfRestriction = constrictionProgile;
            HardCalendarEventRange = HardCalEventTimeRange;
            _Now = now;
            initializeCalendarEventRange(_ProfileOfRestriction,HardCalendarEventRange);
            BusyFrame = new BusyTimeLine(UniqueID.ToString(),StartDateTime, EndDateTime);
            _Users = new TilerUserGroup();
            RigidSchedule = RigidFlag;
            _Complete = isComplete;
            _Enabled = isEnabled;
            _EventPreDeadline = PreDeadline;
            this._Priority = Priority;
            this._LocationInfo = Locationdata;
            _otherPartyID = thirdPartyID;
            this._UiParams = UiData;
            this.ConflictingEvents = conflicts;
            _DataBlob = Notes;
            _PrepTime = PrepTimeData;
            ConflictingEvents = new ConflictProfile();
            _LastReasonStartTimeChanged = this.Start;
            this._Creator = creator;
            this._Users = users;
            _calendarEvent = calendarEvent;
        }

        public SubCalendarEventRestricted()
        {
            isRestricted = true;
            StartDateTime = new DateTimeOffset();
            EndDateTime = new DateTimeOffset();
            _EventDuration = EndDateTime - StartDateTime;
            UniqueID = null;
            _ProfileOfRestriction = null;
            HardCalendarEventRange = new TimeLine();
            _LastReasonStartTimeChanged = this.Start;
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
            TimeLine LimitingTimeLine = LimitingTimeLineData.InterferringTimeLine(getCalendarEventRange);
            if (LimitingTimeLine == null)
            {
                return false;
            }

            if (this.isLocked)
            {
                return (LimitingTimeLineData.IsTimeLineWithin(this.RangeTimeLine));
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
            TimeLine MyTimeLine = MyTimeLineEntry.InterferringTimeLine(getCalendarEventRange);
            if (MyTimeLine == null)
            {
                return false;
            }

            if (this.isLocked)
            {
                return (MyTimeLineEntry.IsTimeLineWithin(this.RangeTimeLine));
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

        public override SubCalendarEvent createCopy(EventID eventId )
        {
            SubCalendarEventRestricted copy = new SubCalendarEventRestricted();
            copy.BusyFrame = this.BusyFrame.CreateCopy();
            copy._CalendarEventRange = getCalendarEventRange.CreateCopy();
            copy._Complete = _Complete;
            copy.ConflictingEvents = this.ConflictingEvents.CreateCopy();
            copy._DataBlob = this._DataBlob?.createCopy();
            copy._Enabled = this._Enabled;
            copy.EndDateTime = this.EndDateTime;
            copy._EventDuration = this._EventDuration;
            copy._Name = this.getName.createCopy();
            copy._EventPreDeadline = this._EventPreDeadline;
            copy.EventScore = this.EventScore;
            copy.HardCalendarEventRange = this.HardCalendarEventRange.CreateCopy();
            copy.isRestricted = this.isRestricted;
            copy.Vestige = this.Vestige;
            copy._LocationInfo = this._LocationInfo.CreateCopy();
            copy.MiscIntData = this.MiscIntData;
            copy._otherPartyID = this._otherPartyID;
            copy.preferredDayIndex = this.preferredDayIndex;
            copy._PrepTime = this._PrepTime;
            copy._Priority = this._Priority;
            copy._ProfileOfRestriction = this._ProfileOfRestriction.createCopy();
            copy.RigidSchedule = this.RigidSchedule;
            copy.StartDateTime = this.StartDateTime;
            copy._UiParams = this._UiParams?.createCopy();

            if (eventId != null)
            {
                copy.UniqueID = eventId;
            }
            else
            {
                copy.UniqueID = UniqueID;//hack
            }
            copy.UnUsableIndex = this.UnUsableIndex;
            copy._UserDeleted = this._UserDeleted;
            copy._Users = this._Users;
            copy._Semantics = this._Semantics?.createCopy();
            copy._UsedTime = this._UsedTime;
            copy.OptimizationFlag = this.OptimizationFlag;
            return copy;
        }

        public override Tuple<TimeLine, double> evaluateAgainstOptimizationParameters(Location refLocation, TimeLine DayTimeLine)
        {
            return base.evaluateAgainstOptimizationParameters(refLocation, DayTimeLine);
        }
        public override bool IsDateTimeWithin(DateTimeOffset DateTimeEntry)
        {
            return base.IsDateTimeWithin(DateTimeEntry);
        }

        public override bool shiftEvent(TimeSpan ChangeInTime, bool force = false, bool lockToId = false)
        {
            if (force)
            {
                StartDateTime += ChangeInTime;
                EndDateTime += ChangeInTime;
                ActiveSlot.shiftTimeline(ChangeInTime);
                _LockToId = lockToId;
                return true;
            }

            TimeLine UpdatedTimeLine = new TimeLine(this.Start + ChangeInTime, this.End + ChangeInTime);
            TimeLine myTImeLine =  _ProfileOfRestriction.getLatestActiveTimeFrameBeforeEnd(UpdatedTimeLine).Item1;
            if (myTImeLine.TimelineSpan >= UpdatedTimeLine.TimelineSpan)
            {
                StartDateTime += ChangeInTime;
                EndDateTime += ChangeInTime;
                ActiveSlot.shiftTimeline(ChangeInTime);
                _LockToId = lockToId;
                return true;
            }

            myTImeLine = _ProfileOfRestriction.getEarliestActiveFrameAfterBeginning(UpdatedTimeLine).Item1;
            if (myTImeLine.TimelineSpan >= UpdatedTimeLine.TimelineSpan)
            {
                StartDateTime += ChangeInTime;
                EndDateTime += ChangeInTime;
                ActiveSlot.shiftTimeline(ChangeInTime);
                _LockToId = lockToId;
                return true;
            }
            return false;
        }


        public override bool PinToPossibleLimit(TimeLine referenceTimeLine)
        {
            List<TimeLine> AllPossibleTimeLines = _ProfileOfRestriction.getAllNonPartialTimeFrames(referenceTimeLine).   Where(obj => obj.TimelineSpan >= this.getActiveDuration).OrderByDescending (obj=>obj.End). ToList();
            if (AllPossibleTimeLines.Count > 0)
            {
                return base.PinToEnd(AllPossibleTimeLines[0]);
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
                this.ConflictingEvents = SubEventEntry.Conflicts;
                this._DataBlob = SubEventEntry.Notes;
                this._Enabled = SubEventEntry.isEnabled;
                this.EndDateTime = SubEventEntry.End;
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
                this.StartDateTime = SubEventEntry.Start;
                this._UiParams = SubEventEntry.getUIParam;
                this.UniqueID = SubEventEntry.SubEvent_ID;
                this._UserDeleted = SubEventEntry.getIsUserDeleted;
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
            retValue.ConflictingEvents = this.Conflicts;
            retValue._DataBlob = this.Notes;
            retValue._Enabled = this.isEnabled;
            retValue.EndDateTime = this.End;
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
            retValue.RigidSchedule = this.RigidSchedule;
            retValue.StartDateTime = this.Start;
            retValue._UiParams = this.getUIParam;
            retValue.UniqueID = this.SubEvent_ID;
            retValue._UserDeleted = this.getIsUserDeleted;
            retValue._Users = this.getAllUsers();
            retValue.Vestige = this.isVestige;
            retValue._otherPartyID = this._otherPartyID;
            retValue._ProfileOfRestriction = this._ProfileOfRestriction;
            retValue._Now = this._Now;
            return retValue;
        }


        public override SubCalendarEvent getNowCopy(EventID CalendarEventID, NowProfile NowData)
        {
            SubCalendarEventRestricted retValue = (SubCalendarEventRestricted)getCalulationCopy();
            TimeSpan SpanShift = NowData.PreferredTime - retValue.Start;
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventID.ToString());
            retValue.shiftEvent(SpanShift, true);
            retValue.RigidSchedule = true;
            return retValue;
        }

        public void setNow(ReferenceNow now)
        {
            _Now = now;
        }

        public override SubCalendarEvent getProcrastinationCopy(CalendarEvent CalendarEventData, Procrastination ProcrastinationData)
        {
            SubCalendarEvent thisCopy = getCalulationCopy();
            SubCalendarEventRestricted retValue = (SubCalendarEventRestricted)thisCopy;


            retValue.HardCalendarEventRange= new TimeLineRestricted(ProcrastinationData.PreferredStartTime, CalendarEventData.RangeTimeLine.End,retValue._ProfileOfRestriction, _Now);
            TimeSpan SpanShift = ProcrastinationData.PreferredStartTime - retValue.Start;
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventData.getId);
            retValue.initializeCalendarEventRange(retValue._ProfileOfRestriction, CalendarEventData.RangeTimeLine);
            retValue.shiftEvent(SpanShift, true);
            return retValue;
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
                    this.HardCalendarEventRange = new TimeLine(this.HardCalendarEventRange.Start, value);
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
                    this.HardCalendarEventRange = new TimeLine(value, this.HardCalendarEventRange.End);
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

        public override RestrictionProfile RetrictionProfile
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
