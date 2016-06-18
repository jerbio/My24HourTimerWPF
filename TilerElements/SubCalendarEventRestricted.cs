using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class SubCalendarEventRestricted : SubCalendarEvent
    {
        protected TimeLine HardCalendarEventRange;//this does not include the restriction
        protected RestrictionProfile ProfileOfRestriction;
        #region Constructor
        public SubCalendarEventRestricted(string CalEventID, DateTimeOffset Start, DateTimeOffset End, RestrictionProfile constrictionProgile, TimeLine HardCalEventTimeRange, bool isEnabled, bool isComplete, ConflictProfile conflictingEvents, bool RigidFlag,TimeSpan PrepTimeData ,TimeSpan PreDeadline, Location_Elements Locationdata, EventDisplay UiData, MiscData Notes, int Priority = 0, bool isDeadlineElapsed = false, string thirdPartyID = "", ConflictProfile conflicts = null)
        { 
            isRestricted =true;
            StartDateTime = Start;
            EndDateTime = End;
            EventDuration = EndDateTime - StartDateTime;
            
            UniqueID = EventID.GenerateSubCalendarEvent(CalEventID);
            ProfileOfRestriction = constrictionProgile;
            HardCalendarEventRange = HardCalEventTimeRange;
            initializeCalendarEventRange(ProfileOfRestriction,HardCalendarEventRange);
            BusyFrame = new BusyTimeLine(UniqueID.ToString(),StartDateTime, EndDateTime);
            UserIDs = new List<string>();
            RigidSchedule = RigidFlag;
            Complete = isComplete;
            DeadlineElapsed = isDeadlineElapsed;
            Enabled = isEnabled;
            EventPreDeadline = PreDeadline;
            this.Priority = Priority;
            this.LocationInfo = Locationdata;
            otherPartyID = thirdPartyID;
            UserIDs = this.UserIDs.ToList();
            this.UiParams = UiData;
            this.ConflictingEvents = conflicts;
            DataBlob = Notes;
            PrepTime = PrepTimeData;
            ConflictingEvents = new ConflictProfile();
            HumaneTimeLine = BusyFrame.CreateCopy();
            NonHumaneTimeLine = BusyFrame.CreateCopy();
        }

        public SubCalendarEventRestricted()
        {
            isRestricted = true;
            StartDateTime = new DateTimeOffset();
            EndDateTime = new DateTimeOffset();
            EventDuration = EndDateTime - StartDateTime;
            UniqueID = null;
            ProfileOfRestriction = null;
            HardCalendarEventRange = new TimeLine();
        }
        #endregion

        #region Functions
        


        public IEnumerable<TimeLine> getFeasibleTimeLines(TimeLine TimeLineEntry)
        {
            return ProfileOfRestriction.getAllTimePossibleTimeFrames(TimeLineEntry);
        }


        public override bool PinToEnd(TimeLine LimitingTimeLineData)
        {
            TimeLine LimitingTimeLine = LimitingTimeLineData.InterferringTimeLine(CalendarEventRange);
            if (LimitingTimeLine == null)
            {
                return false;
            }
            List<TimeLine> allPossibleTimelines = ProfileOfRestriction.getAllTimePossibleTimeFrames(LimitingTimeLine).Where(obj => obj.TimelineSpan >= ActiveDuration).OrderByDescending(obj => obj.End).ToList();
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

            TimeLine RestrictedLimitingFrame = ProfileOfRestriction.getLatestActiveTimeFrameBeforeEnd(LimitingTimeLine);
            if(RestrictedLimitingFrame.TimelineSpan<ActiveDuration)
            {
                RestrictedLimitingFrame = ProfileOfRestriction.getLatestFullFrame(LimitingTimeLine);
            }
            bool retValue=base.PinToEnd(RestrictedLimitingFrame);
            return retValue;
        }

        public override bool PinToStart(TimeLine MyTimeLineEntry)
        {
            TimeLine MyTimeLine = MyTimeLineEntry.InterferringTimeLine(CalendarEventRange);
            if (MyTimeLine == null)
            {
                return false;
            }
            List<TimeLine> allPossibleTimelines = ProfileOfRestriction.getAllTimePossibleTimeFrames(MyTimeLine).Where(obj=>obj.TimelineSpan>=ActiveDuration).OrderBy(obj=>obj.Start).ToList();

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

            

            TimeLine RestrictedLimitingFrame = ProfileOfRestriction.getEarliestActiveFrameAfterBeginning(MyTimeLine);
            if (RestrictedLimitingFrame.TimelineSpan < ActiveDuration)
            {
                RestrictedLimitingFrame = ProfileOfRestriction.getEarliestFullframe(MyTimeLine);
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

            DateTimeOffset myStart = ProfileOfRestriction.getEarliestStartTimeWithinAFrameAfterRefTime(refTimeLine.Start).Start;
            DateTimeOffset myEnd = ProfileOfRestriction.getLatestEndTimeWithinFrameBeforeRefTime(refTimeLine.End).End;
            CalendarEventRange = new TimeLineRestricted(myStart, myEnd, RestrictionData);
        }
        ///*
        public override bool canExistTowardsEndWithoutSpace(TimeLine PossibleTimeLine)
        {
            bool retValue = false;
            List<TimeLine> AllTimeLines = ProfileOfRestriction.getAllTimePossibleTimeFrames(PossibleTimeLine).OrderBy(obj=>obj.Start).ToList();
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
            List<TimeLine> AllTimeLines = ProfileOfRestriction.getAllTimePossibleTimeFrames(PossibleTimeLine).OrderBy(obj => obj.Start).ToList();
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
            copy.CalendarEventRange = CalendarEventRange.CreateCopy();
            copy.Complete = Complete;
            copy.ConflictingEvents = this.ConflictingEvents.CreateCopy();
            copy.DataBlob = this.DataBlob.createCopy();
            copy.DeadlineElapsed = this.DeadlineElapsed;
            copy.Enabled = this.Enabled;
            copy.EndDateTime = this.EndDateTime;
            copy.EventDuration = this.EventDuration;
            copy.EventName = this.EventName;
            copy.EventPreDeadline = this.EventPreDeadline;
            copy.EventScore = this.EventScore;
            //copy.EventSequence = this.EventSequence.CreateCopy();
            copy.FromRepeatEvent = this.FromRepeatEvent;
            copy.HardCalendarEventRange = this.HardCalendarEventRange.CreateCopy();
            copy.HumaneTimeLine = this.HumaneTimeLine.CreateCopy();
            copy.isRestricted = this.isRestricted;
            copy.Vestige = this.Vestige;
            copy.LocationInfo = this.LocationInfo.CreateCopy();
            copy.MiscIntData = this.MiscIntData;
            copy.NonHumaneTimeLine = this.NonHumaneTimeLine.CreateCopy();
            copy.otherPartyID = this.otherPartyID;
            copy.preferredDayIndex = this.preferredDayIndex;
            copy.PrepTime = this.PrepTime;
            copy.Priority = this.Priority;
            copy.ProfileOfRestriction = this.ProfileOfRestriction.createCopy();
            copy.RepetitionFlag = this.RepetitionFlag;
            copy.RigidSchedule = this.RigidSchedule;
            copy.StartDateTime = this.StartDateTime;
            copy.UiParams = this.UiParams.createCopy();

            if (eventId != null)
            {
                copy.UniqueID = eventId;
            }
            else
            {
                copy.UniqueID = UniqueID;//hack
            }
            copy.UnUsableIndex = this.UnUsableIndex;
            copy.UserDeleted = this.UserDeleted;
            copy.UserIDs = this.UserIDs.ToList();
            copy.Semantics = this.Semantics.createCopy();
            copy._UsedTime = this._UsedTime;
            return copy;
        }

        public override Tuple<TimeLine, double> evaluateAgainstOptimizationParameters(Location_Elements refLocation, TimeLine DayTimeLine)
        {
            return base.evaluateAgainstOptimizationParameters(refLocation, DayTimeLine);
        }
        public override bool IsDateTimeWithin(DateTimeOffset DateTimeEntry)
        {
            /*
            bool retValue=false;
            TimeLine myTImelineA =  ProfileOfRestriction.getEarliestActiveFrameAfterBeginning(new TimeLine(DateTimeEntry.AddMilliseconds(-1),DateTimeEntry.AddMilliseconds(1)));
            if (myTImelineA.IsDateTimeWithin(DateTimeEntry))
            {
                return true;
            }

            myTImelineA = ProfileOfRestriction.getLatestActiveTimeFrameBeforeEnd(new TimeLine(DateTimeEntry.AddMilliseconds(-1), DateTimeEntry.AddMilliseconds(1)));
            if (myTImelineA.IsDateTimeWithin(DateTimeEntry))
            {
                return false;
            }*/
            return base.IsDateTimeWithin(DateTimeEntry);
        }

        public override bool shiftEvent(TimeSpan ChangeInTime, bool force = false)
        {
            TimeLine UpdatedTimeLine = new TimeLine(this.Start + ChangeInTime, this.End + ChangeInTime);
            TimeLine myTImeLine =  ProfileOfRestriction.getLatestActiveTimeFrameBeforeEnd(UpdatedTimeLine);
            if (myTImeLine.TimelineSpan >= UpdatedTimeLine.TimelineSpan)
            {
                StartDateTime += ChangeInTime;
                EndDateTime += ChangeInTime;
                ActiveSlot.shiftTimeline(ChangeInTime);
                return true;
            }

            myTImeLine = ProfileOfRestriction.getEarliestActiveFrameAfterBeginning(UpdatedTimeLine);
            if (myTImeLine.TimelineSpan >= UpdatedTimeLine.TimelineSpan)
            {
                StartDateTime += ChangeInTime;
                EndDateTime += ChangeInTime;
                ActiveSlot.shiftTimeline(ChangeInTime);
                return true;
            }
            return false;
        }

        /*
        public override bool PinToEndAndIncludeInTimeLine(TimeLine LimitingTimeLine)
        {
            return base.PinToEndAndIncludeInTimeLine(LimitingTimeLine);
        }
        */

        public override bool PinToPossibleLimit(TimeLine referenceTimeLine)
        {
            List<TimeLine> AllPossibleTimeLines = ProfileOfRestriction.getAllTimePossibleTimeFrames(referenceTimeLine).   Where(obj => obj.TimelineSpan >= this.ActiveDuration).OrderByDescending (obj=>obj.End). ToList();
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
            List<TimeLine> possibleTimeLines = orderByStart ? ProfileOfRestriction.getAllTimePossibleTimeFrames(TimeLineData).OrderByDescending(obj => obj.TimelineSpan).ThenBy(obj => obj.Start).ToList() : ProfileOfRestriction.getAllTimePossibleTimeFrames(TimeLineData).OrderByDescending(obj => obj.TimelineSpan).ThenBy(obj => obj.Start).ToList();
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
                return ProfileOfRestriction;
            }
        }
        
        public override bool UpdateThis(SubCalendarEvent SubEventEntryData)
        {
            if ((this.ID == SubEventEntryData.ID) && canExistWithinTimeLine(SubEventEntryData.getCalendarEventRange))
            {
                SubCalendarEventRestricted SubEventEntry = (SubCalendarEventRestricted)SubEventEntryData;
                this.BusyFrame = SubEventEntry.ActiveSlot;
                this.CalendarEventRange = SubEventEntry.getCalendarEventRange;
                this.FromRepeatEvent = SubEventEntry.FromRepeat;
                this.EventName = SubEventEntry.Name;
                this.EventDuration = SubEventEntry.ActiveDuration;
                this.Complete = SubEventEntry.isComplete;
                this.ConflictingEvents = SubEventEntry.Conflicts;
                this.DataBlob = SubEventEntry.Notes;
                this.DeadlineElapsed = SubEventEntry.isDeadlineElapsed;
                this.Enabled = SubEventEntry.isEnabled;
                this.EndDateTime = SubEventEntry.End;
                this.EventPreDeadline = SubEventEntry.PreDeadline;
                this.EventScore = SubEventEntry.Score;
                //this.isRestricted = true;
                this.LocationInfo = SubEventEntry.myLocation;
                this.OldPreferredIndex = SubEventEntry.OldUniversalIndex;
                this.otherPartyID = SubEventEntry.ThirdPartyID;
                this.preferredDayIndex = SubEventEntry.UniversalDayIndex;
                this.PrepTime = SubEventEntry.Preparation;
                this.Priority = SubEventEntry.EventPriority;
                this.ProfileOfNow = SubEventEntry.ProfileOfNow;
                this.ProfileOfProcrastination = SubEventEntry.ProfileOfProcrastination;
                this.RepetitionFlag = SubEventEntry.FromRepeat;
                //this.RigidSchedule = this.rig
                this.StartDateTime = SubEventEntry.Start;
                this.UiParams = SubEventEntry.UIParam;
                this.UniqueID = SubEventEntry.SubEvent_ID;
                this.UserDeleted = SubEventEntry.isUserDeleted;
                this.UserIDs = SubEventEntry.getAllUserIDs();
                this.Vestige = SubEventEntry.isVestige;
                this.otherPartyID = SubEventEntry.otherPartyID;
                this.ProfileOfRestriction = SubEventEntry.ProfileOfRestriction;
                this.CreatorIDInfo = SubEventEntry.CreatorIDInfo;
                this.Semantics = SubEventEntry.Semantics;
                this._UsedTime = SubEventEntry._UsedTime;
                return true;
            }

            throw new Exception("Error Detected: Trying to update SubCalendar Event with non matching ID");
        }

        protected override SubCalendarEvent getCalulationCopy()
        {
            SubCalendarEventRestricted retValue = new SubCalendarEventRestricted();
            retValue.BusyFrame = this.ActiveSlot;
            retValue.CalendarEventRange = this.getCalendarEventRange.CreateCopy();
            retValue.FromRepeatEvent = this.FromRepeat;
            retValue.EventName = this.Name;
            retValue.EventDuration = this.ActiveDuration;
            retValue.Complete = this.isComplete;
            retValue.ConflictingEvents = this.Conflicts;
            retValue.DataBlob = this.Notes;
            retValue.DeadlineElapsed = this.isDeadlineElapsed;
            retValue.Enabled = this.isEnabled;
            retValue.EndDateTime = this.End;
            retValue.EventPreDeadline = this.PreDeadline;
            retValue.EventScore = this.Score;
            retValue.isRestricted = this.isEventRestricted;
            retValue.LocationInfo = this.myLocation;
            retValue.OldPreferredIndex = this.OldUniversalIndex;
            retValue.otherPartyID = this.ThirdPartyID;
            retValue.preferredDayIndex = this.UniversalDayIndex;
            retValue.PrepTime = this.Preparation;
            retValue.Priority = this.EventPriority;
            retValue.ProfileOfNow = this.ProfileOfNow.CreateCopy();
            retValue.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            retValue.RepetitionFlag = this.FromRepeat;
            retValue.RigidSchedule = this.Rigid;
            retValue.StartDateTime = this.Start;
            retValue.UiParams = this.UIParam;
            retValue.UniqueID = this.SubEvent_ID;
            retValue.UserDeleted = this.isUserDeleted;
            retValue.UserIDs = this.getAllUserIDs();
            retValue.Vestige = this.isVestige;
            retValue.otherPartyID = this.otherPartyID;
            retValue.ProfileOfRestriction = this.ProfileOfRestriction;
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

        public override SubCalendarEvent getProcrastinationCopy(CalendarEvent CalendarEventData, Procrastination ProcrastinationData)
        {
            SubCalendarEvent thisCopy = getCalulationCopy();
            SubCalendarEventRestricted retValue = (SubCalendarEventRestricted)thisCopy;


            retValue.HardCalendarEventRange= new TimeLineRestricted(ProcrastinationData.PreferredStartTime, CalendarEventData.RangeTimeLine.End,retValue.ProfileOfRestriction);
            TimeSpan SpanShift = ProcrastinationData.PreferredStartTime - retValue.Start;
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventData.ID);
            retValue.initializeCalendarEventRange(retValue.ProfileOfRestriction, CalendarEventData.RangeTimeLine);
            retValue.shiftEvent(SpanShift, true);
            return retValue;
        }
        //*/
        #endregion   
    }
}
