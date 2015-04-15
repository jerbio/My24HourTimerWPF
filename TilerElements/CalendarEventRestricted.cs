using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace TilerElements
{
    public class CalendarEventRestricted:CalendarEvent
    {
        protected RestrictionProfile ProfileOfRestriction;
        

        protected CalendarEventRestricted ()
        {
        ;
        }

        public CalendarEventRestricted(string Name, DateTimeOffset Start, DateTimeOffset End,DateTimeOffset OriginalStartData, RestrictionProfile restrictionProfile, TimeSpan Duration, Repetition RepetitionProfile, bool isCompleted, bool isEnabled, int Divisions, bool isRigid, Location_Elements Location,TimeSpan EventPreparation,TimeSpan Event_PreDeadline, EventDisplay UiSettings = null, MiscData NoteData=null)
        {
            EventName = Name;
            StartDateTime = Start;
            EndDateTime = End;
            RigidSchedule = isRigid;
            ProfileOfRestriction = restrictionProfile;
            Splits = Divisions;
            if (RepetitionProfile.Enable)
            {
                Splits = Divisions;
                End = RepetitionProfile.Range.End;
                TimePerSplit = new TimeSpan();
            }
            Complete = isCompleted;
            Enabled = isEnabled;
            UniqueID = EventID.GenerateCalendarEvent(RepetitionSequence);
            UpdateLocationMatrix(Location);
            UiParams = UiSettings;
            DataBlob = NoteData;
            LocationInfo = Location;
            
            EventDuration = Duration;
            TimePerSplit = TimeSpan.FromTicks(EventDuration.Ticks / Splits);
            isRestricted = true;
            ProfileOfNow = new NowProfile();
            OriginalStart = OriginalStartData;
            InstantiateSubEvents();
        }



        public CalendarEventRestricted(EventID EventIDEntry, string Name, DateTimeOffset Start, DateTimeOffset End, DateTimeOffset OriginalStartData, RestrictionProfile restrictionProfile, TimeSpan Duration, Repetition RepetitionProfile, bool isCompleted, bool isEnabled, int Divisions, bool isRigid, Location_Elements Location, TimeSpan EventPreparation, TimeSpan Event_PreDeadline, long RepetitionIndex, EventDisplay UiSettings = null, MiscData NoteData = null)
        {
            EventName = Name;
            StartDateTime = Start;
            EndDateTime = End;
            RigidSchedule = isRigid;
            ProfileOfRestriction = restrictionProfile;
            Splits = Divisions;
            RepetitionSequence = RepetitionIndex;
            if (RepetitionProfile.Enable)
            {
                Splits = Divisions;
                End = RepetitionProfile.Range.End;
                TimePerSplit = new TimeSpan();
            }
            Complete = isCompleted;
            Enabled = isEnabled;
            EventPreDeadline = Event_PreDeadline;
            UniqueID = EventIDEntry;
            UpdateLocationMatrix(Location);
            UiParams = UiSettings;
            DataBlob = NoteData;
            ProfileOfNow = new NowProfile();
            isRestricted = true;
            EventDuration = Duration;
            TimePerSplit = TimeSpan.FromTicks(EventDuration.Ticks / Splits);
            LocationInfo = Location;
            
            OriginalStart = OriginalStartData;
            InstantiateSubEvents();
        }

        static public CalendarEventRestricted InstantiateRepeatedCandidate(string Name, DateTimeOffset Start, DateTimeOffset End, EventID CalendarEventID, RestrictionProfile restrictionProfile, TimeSpan Duration, DateTimeOffset OriginalStartData,int division, Location_Elements Location, EventDisplay UiSettings, bool RigidFlag, TimeSpan preparation, string thirdPartyID, long NextSequence, ConcurrentDictionary<DateTimeOffset, CalendarEvent> OrginalStartToCalendarEvent)
        { 
            CalendarEventRestricted retValue = new CalendarEventRestricted();
            retValue.UniqueID = EventID.GenerateRepeatCalendarEvent(CalendarEventID.ToString(), NextSequence);
            retValue .EventName =Name;
            retValue.EventDuration = Duration;
            retValue.StartDateTime = Start;
            retValue.EndDateTime = End;
            retValue.ProfileOfRestriction = restrictionProfile;
            retValue.Complete = false;
            retValue.Enabled = true;
            retValue.DeadlineElapsed = End > TilerEvent.EventNow;
            retValue.Splits = division;
            retValue.TimePerSplit = TimeSpan.FromTicks( retValue.EventDuration.Ticks / division);
            retValue.isRestricted = true;
            retValue.LocationInfo = Location;
            retValue.EndOfCalculation = End < TilerEvent.EventNow.Add(CalculationEndSpan) ? End : TilerEvent.EventNow.Add(CalculationEndSpan);
            retValue.RigidSchedule = RigidFlag;
            retValue.Priority = 0;
            retValue.otherPartyID = thirdPartyID;
            retValue.UiParams = UiSettings;
            retValue.PrepTime = preparation;
            
            retValue.UpdateLocationMatrix(Location);
            retValue.InstantiateSubEvents();
            retValue.OriginalStart = OriginalStartData;

            while (!OrginalStartToCalendarEvent.TryAdd(OriginalStartData, retValue))
            {
                Thread.Sleep(10);
            }
            
            return retValue;
        }

        public override CalendarEvent createCopy(EventID eventId = null)
        {
            CalendarEventRestricted MyCalendarEventCopy = new CalendarEventRestricted();
            MyCalendarEventCopy.EventDuration = new TimeSpan(EventDuration.Ticks);
            MyCalendarEventCopy.EventName = EventName.ToString();
            MyCalendarEventCopy.StartDateTime = StartDateTime;
            MyCalendarEventCopy.EndDateTime = EndDateTime;
            MyCalendarEventCopy.EventPreDeadline = new TimeSpan(EventPreDeadline.Ticks);
            MyCalendarEventCopy.PrepTime = new TimeSpan(PrepTime.Ticks);
            MyCalendarEventCopy.Priority = Priority;
            //MyCalendarEventCopy.RepetitionFlag = RepetitionFlag;
            MyCalendarEventCopy.EventRepetition = EventRepetition.CreateCopy();// EventRepetition != null ? EventRepetition.CreateCopy() : EventRepetition;
            MyCalendarEventCopy.Complete = this.Complete;
            MyCalendarEventCopy.RigidSchedule = RigidSchedule;//hack
            MyCalendarEventCopy.Splits = Splits;
            MyCalendarEventCopy.TimePerSplit = new TimeSpan(TimePerSplit.Ticks);
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
            MyCalendarEventCopy._UsedTime = this._UsedTime;
            if (eventId != null)
            {
                MyCalendarEventCopy.UniqueID = eventId;
            }
            else
            {
                MyCalendarEventCopy.UniqueID = UniqueID;//hack
            }
            int counter = 0;
            foreach (SubCalendarEventRestricted eachSubCalendarEvent in this.SubEvents.Values)
            {
                MyCalendarEventCopy.SubEvents.Add(eachSubCalendarEvent.SubEvent_ID, eachSubCalendarEvent.createCopy(EventID.GenerateSubCalendarEvent(MyCalendarEventCopy.UniqueID, ++counter) ));
            }

            MyCalendarEventCopy.SchedulStatus = SchedulStatus;
            MyCalendarEventCopy.otherPartyID = otherPartyID == null ? null : otherPartyID.ToString();
            MyCalendarEventCopy.UserIDs = this.UserIDs.ToList();
            return MyCalendarEventCopy;
            
            //return base.createCopy();
        }

        void InstantiateSubEvents()
        {
            for (int i = 0; i < Splits; i++)
            {
                DateTimeOffset SubStart = this.Start;
                DateTimeOffset SubEnd = Start.Add(TimePerSplit);
                SubCalendarEventRestricted newEvent = new SubCalendarEventRestricted(UniqueID.ToString(),i+1, SubStart, SubEnd, ProfileOfRestriction, this.RangeTimeLine, true, false, new ConflictProfile(), RigidSchedule, PrepTime, EventPreDeadline, LocationInfo, UiParams, DataBlob, Priority, DeadlineElapsed, ThirdPartyID);
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
        protected override CalendarEvent getCalculationCopy()
        {
            CalendarEventRestricted RetValue = new CalendarEventRestricted();
            RetValue.EventDuration = this.ActiveDuration;
            RetValue.EventName = this.Name;
            RetValue.StartDateTime = this.Start;
            RetValue.EndDateTime = this.End;
            RetValue.EventPreDeadline = this.PreDeadline;
            RetValue.PrepTime = this.Preparation;
            RetValue.Priority = this.EventPriority;
            //RetValue.RepetitionFlag = this.RepetitionStatus;
            RetValue.EventRepetition = this.Repeat;// EventRepetition != this.null ? EventRepetition.CreateCopy() : EventRepetition;
            RetValue.Complete = this.isComplete;
            RetValue.RigidSchedule = this.Rigid;//hack
            RetValue.Splits = this.NumberOfSplit;
            RetValue.TimePerSplit = this.EachSplitTimeSpan;
            RetValue.UniqueID = EventID.GenerateCalendarEvent(0);
            //RetValue.EventSequence = this.EventSequence;
            RetValue.SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            RetValue.UiParams = this.UIParam.createCopy();
            RetValue.DataBlob = this.Notes;
            RetValue.Enabled = this.isEnabled;
            RetValue.isRestricted = this.isEventRestricted;
            RetValue.LocationInfo = this.myLocation;//hack you might need to make copy
            RetValue.ProfileOfProcrastination = this.ProcrastinationInfo.CreateCopy();
            RetValue.DeadlineElapsed = this.isDeadlineElapsed;
            RetValue.UserDeleted = this.isUserDeleted;
            RetValue.CompletedCount = this.CompletionCount;
            RetValue.DeletedCount = this.DeletionCount;
            RetValue.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            RetValue.ProfileOfNow = this.ProfileOfNow.CreateCopy();
            RetValue.otherPartyID = this.ThirdPartyID;// == this.null ? null : otherPartyID.ToString();
            RetValue.UserIDs = this.getAllUserIDs();//.ToList();
            RetValue.ProfileOfNow = this.ProfileOfNow.CreateCopy();
            RetValue.ProfileOfRestriction = this.ProfileOfRestriction.createCopy();
            RetValue.UpdateLocationMatrix(RetValue.LocationInfo);
            return RetValue;
        }

        public override void UpdateThis(CalendarEvent CalendarEventEntry)
        {
            if ((this.ID == CalendarEventEntry.ID))
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


    }
}
