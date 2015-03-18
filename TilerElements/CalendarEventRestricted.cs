using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class CalendarEventRestricted:CalendarEvent
    {
        protected RestrictionProfile ProfileOfRestriction;
        

        protected CalendarEventRestricted ()
        {
        ;
        }

        public CalendarEventRestricted(string Name, DateTimeOffset Start, DateTimeOffset End, RestrictionProfile restrictionProfile, TimeSpan Duration, Repetition RepetitionProfile, bool isCompleted, bool isEnabled, int Divisions, bool isRigid, Location_Elements Location,TimeSpan EventPreparation,TimeSpan Event_PreDeadline, EventDisplay UiSettings = null, MiscData NoteData=null)
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
            UniqueID = EventID.GenerateCalendarEvent();
            UpdateLocationMatrix(Location);
            UiParams = UiSettings;
            DataBlob = NoteData;
            
            EventDuration = Duration;
            TimePerSplit = TimeSpan.FromTicks(EventDuration.Ticks / Splits);
            isRestricted = true;
            ProfileOfNow = new NowProfile();
            InstantiateSubEvents();
        }



        public CalendarEventRestricted(EventID EventIDEntry, string Name, DateTimeOffset Start, DateTimeOffset End, RestrictionProfile restrictionProfile, TimeSpan Duration, Repetition RepetitionProfile, bool isCompleted, bool isEnabled, int Divisions, bool isRigid, Location_Elements Location,TimeSpan EventPreparation,TimeSpan Event_PreDeadline, EventDisplay UiSettings = null, MiscData NoteData = null)
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
            EventPreDeadline = Event_PreDeadline;
            UniqueID = EventIDEntry;
            UpdateLocationMatrix(Location);
            UiParams = UiSettings;
            DataBlob = NoteData;
            ProfileOfNow = new NowProfile();
            isRestricted = true;
            EventDuration = Duration;
            TimePerSplit = TimeSpan.FromTicks(EventDuration.Ticks / Splits);
            
            InstantiateSubEvents();
        }

        static public CalendarEventRestricted InstantiateRepeatedCandidate(string Name, DateTimeOffset Start, DateTimeOffset End, EventID CalendarEventID, RestrictionProfile restrictionProfile, TimeSpan Duration, int division, Location_Elements Location,EventDisplay UiSettings,bool RigidFlag,TimeSpan preparation, string thirdPartyID)
        { 
            CalendarEventRestricted retValue = new CalendarEventRestricted();
            retValue .UniqueID = EventID.GenerateRepeatCalendarEvent(CalendarEventID.ToString());
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
            retValue.LocationData = Location;
            retValue.EndOfCalculation = End < TilerEvent.EventNow.Add(CalculationEndSpan) ? End : TilerEvent.EventNow.Add(CalculationEndSpan);
            retValue.RigidSchedule = RigidFlag;
            retValue.Priority = 0;
            retValue.otherPartyID = thirdPartyID;
            retValue.UiParams = UiSettings;
            retValue.PrepTime = preparation;
            
            retValue.UpdateLocationMatrix(Location);
            retValue.InstantiateSubEvents();
            
            return retValue;
        }

        public override CalendarEvent createCopy()
        {
            CalendarEventRestricted MyCalendarEventCopy = new CalendarEventRestricted();
            MyCalendarEventCopy.EventDuration = new TimeSpan(EventDuration.Ticks);
            MyCalendarEventCopy.EventName = EventName.ToString();
            MyCalendarEventCopy.StartDateTime = StartDateTime;
            MyCalendarEventCopy.EndDateTime = EndDateTime;
            MyCalendarEventCopy.EventPreDeadline = new TimeSpan(EventPreDeadline.Ticks);
            MyCalendarEventCopy.PrepTime = new TimeSpan(PrepTime.Ticks);
            MyCalendarEventCopy.Priority = Priority;
            MyCalendarEventCopy.RepetitionFlag = RepetitionFlag;
            MyCalendarEventCopy.EventRepetition = EventRepetition.CreateCopy();// EventRepetition != null ? EventRepetition.CreateCopy() : EventRepetition;
            MyCalendarEventCopy.Complete = this.Complete;
            MyCalendarEventCopy.RigidSchedule = RigidSchedule;//hack
            MyCalendarEventCopy.Splits = Splits;
            MyCalendarEventCopy.TimePerSplit = new TimeSpan(TimePerSplit.Ticks);
            MyCalendarEventCopy.UniqueID = UniqueID;//hack
            MyCalendarEventCopy.EventSequence = EventSequence.CreateCopy();
            MyCalendarEventCopy.SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            MyCalendarEventCopy.UiParams = this.UiParams.createCopy();
            MyCalendarEventCopy.DataBlob = this.DataBlob.createCopy();
            MyCalendarEventCopy.Enabled = this.Enabled;
            MyCalendarEventCopy.isRestricted = this.isRestricted;
            MyCalendarEventCopy.LocationData = LocationData;//hack you might need to make copy
            MyCalendarEventCopy.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            MyCalendarEventCopy.DeadlineElapsed = this.DeadlineElapsed;
            MyCalendarEventCopy.UserDeleted = this.UserDeleted;
            MyCalendarEventCopy.CompletedCount = this.CompletedCount;
            MyCalendarEventCopy.DeletedCount = this.DeletedCount;
            MyCalendarEventCopy.ProfileOfRestriction = this.ProfileOfRestriction.createCopy();
            MyCalendarEventCopy.ProfileOfNow = this.ProfileOfNow.CreateCopy();
            MyCalendarEventCopy.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();


            foreach (SubCalendarEventRestricted eachSubCalendarEvent in this.SubEvents.Values)
            {
                MyCalendarEventCopy.SubEvents.Add(eachSubCalendarEvent.SubEvent_ID, eachSubCalendarEvent.createCopy());
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
                SubCalendarEventRestricted newEvent = new SubCalendarEventRestricted(UniqueID.ToString(), SubStart, SubEnd, ProfileOfRestriction, this.RangeTimeLine, true, false, new ConflictProfile(), RigidSchedule, PrepTime, EventPreDeadline, LocationData, UiParams, DataBlob, Priority, DeadlineElapsed, ThirdPartyID);
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

    }
}
