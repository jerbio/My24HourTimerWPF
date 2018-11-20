using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class CalendarEventRestricted:CalendarEvent
    {
        protected RestrictionProfile ProfileOfRestriction;
        protected ReferenceNow _Now;

        protected CalendarEventRestricted ()
        {
        ;
        }

        public CalendarEventRestricted(TilerUser creator, TilerUserGroup userGroup, EventName Name, DateTimeOffset Start, DateTimeOffset End, RestrictionProfile restrictionProfile, TimeSpan Duration, Repetition RepetitionProfile, bool isCompleted, bool isEnabled, int Divisions, bool isRigid, Location Location,TimeSpan EventPreparation,TimeSpan Event_PreDeadline, EventID eventId, ReferenceNow now, EventDisplay UiSettings = null, MiscData NoteData=null, string timeZone = null)
        {
           _Name =  Name;
            StartDateTime = Start;
            EndDateTime = End;
            RigidSchedule = isRigid;
            ProfileOfRestriction = restrictionProfile;
            Splits = Divisions;
            if (RepetitionProfile.Enable)
            {
                Splits = Divisions;
                End = RepetitionProfile.Range.End;
                _AverageTimePerSplit = new TimeSpan();
            }
            Complete = isCompleted;
            Enabled = isEnabled;
            UniqueID = EventID.GenerateCalendarEvent();
            UniqueID = eventId ?? this.UniqueID; /// already initialized by preceeding code initialization
            UiParams = UiSettings;
            DataBlob = NoteData;
            LocationInfo = Location;
            
            EventDuration = Duration;
            _AverageTimePerSplit = TimeSpan.FromTicks(EventDuration.Ticks / Splits);
            isRestricted = true;
            ProfileOfNow = new NowProfile();
            LocationInfo = Location;
            this._Creator = creator;
            this._Users = userGroup;
            this._TimeZone = timeZone;
            _Now = now;
            InstantiateSubEvents();
        }



        public CalendarEventRestricted(TilerUser creator, TilerUserGroup userGroup, string timeZone, EventName Name, DateTimeOffset Start, DateTimeOffset End, RestrictionProfile restrictionProfile, TimeSpan Duration, Repetition RepetitionProfile, bool isCompleted, bool isEnabled, int Divisions, bool isRigid, Location Location,TimeSpan EventPreparation,TimeSpan Event_PreDeadline, EventID eventId, ReferenceNow now, EventDisplay UiSettings = null, MiscData NoteData = null)
        {
            _Name = Name;
            StartDateTime = Start;
            EndDateTime = End;
            RigidSchedule = isRigid;
            ProfileOfRestriction = restrictionProfile;
            Splits = Divisions;
            if (RepetitionProfile.Enable)
            {
                Splits = Divisions;
                End = RepetitionProfile.Range.End;
                _AverageTimePerSplit = new TimeSpan();
            }
            Complete = isCompleted;
            Enabled = isEnabled;
            EventPreDeadline = Event_PreDeadline;
            UniqueID = EventID.GenerateCalendarEvent();
            UniqueID = eventId ?? this.UniqueID; /// already initialized by preceeding code initialization
            UiParams = UiSettings;
            DataBlob = NoteData;
            ProfileOfNow = new NowProfile();
            isRestricted = true;
            EventDuration = Duration;
            _AverageTimePerSplit = TimeSpan.FromTicks(EventDuration.Ticks / Splits);
            LocationInfo = Location;
            this._Creator = creator;
            this._Users = userGroup;
            this._TimeZone = timeZone;
            _Now = now;
            InstantiateSubEvents();
        }

        static public CalendarEventRestricted InstantiateRepeatedCandidate(EventName Name, DateTimeOffset Start, DateTimeOffset End, EventID CalendarEventID, RestrictionProfile restrictionProfile, TimeSpan Duration, int division, Location Location,EventDisplay UiSettings,bool RigidFlag,TimeSpan preparation, string thirdPartyID, ReferenceNow now)
        { 
            CalendarEventRestricted retValue = new CalendarEventRestricted();
            retValue .UniqueID = EventID.GenerateRepeatCalendarEvent(CalendarEventID.ToString());
            retValue ._Name =  Name;
            retValue.EventDuration = Duration;
            retValue.StartDateTime = Start;
            retValue.EndDateTime = End;
            retValue.ProfileOfRestriction = restrictionProfile;
            retValue.Complete = false;
            retValue.Enabled = true;
            retValue.DeadlineElapsed = End > TilerEvent.EventNow;
            retValue.Splits = division;
            retValue._AverageTimePerSplit = TimeSpan.FromTicks( retValue.EventDuration.Ticks / division);
            retValue.isRestricted = true;
            retValue.LocationInfo = Location;
            retValue.EndOfCalculation = End < TilerEvent.EventNow.Add(CalculationEndSpan) ? End : TilerEvent.EventNow.Add(CalculationEndSpan);
            retValue.RigidSchedule = RigidFlag;
            retValue.Priority = 0;
            retValue.otherPartyID = thirdPartyID;
            retValue.UiParams = UiSettings;
            retValue.PrepTime = preparation;
            retValue._Now = now;
            //retValue.UpdateLocationMatrix(Location);
            retValue.InstantiateSubEvents();
            
            return retValue;
        }

        public override CalendarEvent createCopy(EventID eventId = null)
        {
            CalendarEventRestricted MyCalendarEventCopy = new CalendarEventRestricted();
            MyCalendarEventCopy.EventDuration = new TimeSpan(EventDuration.Ticks);
            MyCalendarEventCopy._Name = this._Name.createCopy();
            MyCalendarEventCopy.StartDateTime = StartDateTime;
            MyCalendarEventCopy.EndDateTime = EndDateTime;
            MyCalendarEventCopy.EventPreDeadline = new TimeSpan(EventPreDeadline.Ticks);
            MyCalendarEventCopy.PrepTime = new TimeSpan(PrepTime.Ticks);
            MyCalendarEventCopy.Priority = Priority;
            MyCalendarEventCopy.EventRepetition = EventRepetition.CreateCopy();// EventRepetition != null ? EventRepetition.CreateCopy() : EventRepetition;
            MyCalendarEventCopy.Complete = this.Complete;
            MyCalendarEventCopy.RigidSchedule = RigidSchedule;//hack
            MyCalendarEventCopy.Splits = Splits;
            MyCalendarEventCopy._AverageTimePerSplit = new TimeSpan(_AverageTimePerSplit.Ticks);
            MyCalendarEventCopy.EventSequence = EventSequence.CreateCopy();
            MyCalendarEventCopy.SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            MyCalendarEventCopy.UiParams = this.UiParams.createCopy();
            MyCalendarEventCopy.DataBlob = this.DataBlob.createCopy();
            MyCalendarEventCopy.Enabled = this.Enabled;
            MyCalendarEventCopy.isRestricted = this.isRestricted;
            MyCalendarEventCopy.LocationInfo = LocationInfo;//hack you might need to make copy
            MyCalendarEventCopy.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            MyCalendarEventCopy.DeadlineElapsed = this.DeadlineElapsed;
            MyCalendarEventCopy.UserDeleted = this.UserDeleted;
            MyCalendarEventCopy.CompletedCount = this.CompletedCount;
            MyCalendarEventCopy.DeletedCount = this.DeletedCount;
            MyCalendarEventCopy.ProfileOfRestriction = this.ProfileOfRestriction.createCopy();
            MyCalendarEventCopy.ProfileOfNow = this.ProfileOfNow.CreateCopy();
            MyCalendarEventCopy.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            MyCalendarEventCopy.Semantics = this.Semantics.createCopy();
            MyCalendarEventCopy._Now = this._Now;
            MyCalendarEventCopy._UsedTime = this._UsedTime;
            if (eventId != null)
            {
                MyCalendarEventCopy.UniqueID = eventId;
            }
            else
            {
                MyCalendarEventCopy.UniqueID = UniqueID;//hack
            }

            foreach (SubCalendarEventRestricted eachSubCalendarEvent in this.SubEvents.Values)
            {
                MyCalendarEventCopy.SubEvents.Add(eachSubCalendarEvent.SubEvent_ID, eachSubCalendarEvent.createCopy(EventID.GenerateSubCalendarEvent(MyCalendarEventCopy.UniqueID) ));
            }

            //MyCalendarEventCopy.SchedulStatus = SchedulStatus;
            MyCalendarEventCopy.otherPartyID = otherPartyID == null ? null : otherPartyID.ToString();
            MyCalendarEventCopy._Users = this._Users;
            return MyCalendarEventCopy;
            
            //return base.createCopy();
        }

        void InstantiateSubEvents()
        {
            TimeLine eachStart = ProfileOfRestriction.getEarliestStartTimeWithinAFrameAfterRefTime(this.Start);
            for (int i = 0; i < Splits; i++)
            {
                DateTimeOffset SubStart = eachStart.Start;
                DateTimeOffset SubEnd = SubStart.Add(_AverageTimePerSplit);
                SubCalendarEventRestricted newEvent = new SubCalendarEventRestricted(this.getCreator, this._Users, UniqueID.ToString(), this.getName, SubStart, SubEnd, ProfileOfRestriction, this.RangeTimeLine, true, false, new ConflictProfile(), RigidSchedule, PrepTime, EventPreDeadline, LocationInfo, UiParams, DataBlob, _Now, Priority, DeadlineElapsed, ThirdPartyID);
                SubEvents.Add(newEvent.SubEvent_ID, newEvent);
            }
        }

        public RestrictionProfile RetrictionInfo
        {
            get
            {
                return ProfileOfRestriction;
            }
        }

        public ReferenceNow Now
        {
            get
            {
                return _Now;
            }
        }
        protected override CalendarEvent getCalculationCopy()
        {
            CalendarEventRestricted RetValue = new CalendarEventRestricted();
            RetValue.EventDuration = this.getActiveDuration;
            RetValue._Name = this.getName.createCopy();
            RetValue.StartDateTime = this.Start;
            RetValue.EndDateTime = this.End;
            RetValue.EventPreDeadline = this.getPreDeadline;
            RetValue.PrepTime = this.getPreparation;
            RetValue.Priority = this.getEventPriority;
            RetValue.EventRepetition = this.Repeat;// EventRepetition != this.null ? EventRepetition.CreateCopy() : EventRepetition;
            RetValue.Complete = this.getIsComplete;
            RetValue.RigidSchedule = this.isLocked;//hack
            RetValue.Splits = this.NumberOfSplit;
            RetValue._AverageTimePerSplit = this.AverageTimeSpanPerSubEvent;
            RetValue.UniqueID = EventID.GenerateCalendarEvent();
            //RetValue.EventSequence = this.EventSequence;
            RetValue.SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            RetValue.UiParams = this.getUIParam.createCopy();
            RetValue.DataBlob = this.Notes;
            RetValue.Enabled = this.isEnabled;
            RetValue.isRestricted = this.getIsEventRestricted;
            RetValue.LocationInfo = this.Location;//hack you might need to make copy
            RetValue.ProfileOfProcrastination = this.getProcrastinationInfo.CreateCopy();
            RetValue.DeadlineElapsed = this.getIsDeadlineElapsed;
            RetValue.UserDeleted = this.getIsUserDeleted;
            RetValue.CompletedCount = this.CompletionCount;
            RetValue.DeletedCount = this.DeletionCount;
            RetValue.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            RetValue.ProfileOfNow = this.ProfileOfNow.CreateCopy();
            RetValue.otherPartyID = this.ThirdPartyID;// == this.null ? null : otherPartyID.ToString();
            RetValue._Users = this.getAllUsers();//.ToList();
            RetValue.ProfileOfNow = this.ProfileOfNow.CreateCopy();
            RetValue.ProfileOfRestriction = this.ProfileOfRestriction.createCopy();
            //RetValue.UpdateLocationMatrix(RetValue.LocationInfo);
            return RetValue;
        }

        override protected void IncreaseSplitCount(uint delta)
        {
            List<SubCalendarEvent> newSubs = new List<SubCalendarEvent>();
            TimeLine eachStart = ProfileOfRestriction.getEarliestStartTimeWithinAFrameAfterRefTime(this.Start);
            for (int i = 0; i < delta; i++)
            {
                DateTimeOffset SubStart = eachStart.Start;
                DateTimeOffset SubEnd = SubStart.Add(_AverageTimePerSplit);
                SubCalendarEventRestricted newEvent = new SubCalendarEventRestricted(this.getCreator, this._Users, UniqueID.ToString(), this.getName, SubStart, SubEnd, ProfileOfRestriction, this.RangeTimeLine, true, false, new ConflictProfile(), RigidSchedule, PrepTime, EventPreDeadline, LocationInfo, UiParams, DataBlob, _Now, Priority, DeadlineElapsed, ThirdPartyID);
                SubEvents.Add(newEvent.SubEvent_ID, newEvent);
            }
            Splits += (int)delta;
            EventDuration = TimeSpan.FromTicks(SubEvents.Values.Sum(subEvent => subEvent.getActiveDuration.Ticks));
        }

        public override void UpdateThis(CalendarEvent CalendarEventEntry)
        {
            if ((this.getId == CalendarEventEntry.getId))
            {
                base.UpdateThis(CalendarEventEntry);
                CalendarEventRestricted castedEvent = CalendarEventEntry as CalendarEventRestricted;
                if (castedEvent != null)
                {
                    this.ProfileOfRestriction = castedEvent.ProfileOfRestriction;
                }
                return;
            }

            throw new Exception("Invalid Calendar ID used in Update Calendar Event");
        }

        public override List<TimeLine> getInterferringWithTimeLine(TimeLine timeLine)
        {
            List<TimeLine> nonPartialFrames = ProfileOfRestriction.getAllNonPartialTimeFrames(timeLine);
            TimeLine earliestFrame = ProfileOfRestriction.getEarliestActiveFrameAfterBeginning(timeLine);
            TimeLine latestFrame = ProfileOfRestriction.getLatestActiveTimeFrameBeforeEnd(timeLine);
            nonPartialFrames = nonPartialFrames.Where(objTimeLine => objTimeLine.Start != earliestFrame.Start && objTimeLine.End != latestFrame.End ).ToList();
            nonPartialFrames.Insert(0, earliestFrame);
            nonPartialFrames.Add(latestFrame);
            return nonPartialFrames;
        }
    }
}
