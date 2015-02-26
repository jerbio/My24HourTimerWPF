using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class CalendarEventRestricted:CalendarEvent
    {
        RestrictionProfile ProfileOfRestriction;

        CalendarEventRestricted ()
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
            UpdateLocationMatrix(Location);
            UiParams = UiSettings;
            DataBlob = NoteData;
            UniqueID = EventID.GenerateCalendarEvent();
            isRestricted = true;
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
            UpdateLocationMatrix(Location);
            UiParams = UiSettings;
            DataBlob = NoteData;
            UniqueID = EventIDEntry;
            isRestricted = true;
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

        void InstantiateSubEvents()
        {
            for (int i = 0; i < Splits; i++)
            {
                SubCalendarEventRestricted newEvent = new SubCalendarEventRestricted(UniqueID.ToString(), this.Start, this.End, ProfileOfRestriction, this.RangeTimeLine, true, true, new ConflictProfile());
                SubEvents.Add(newEvent.SubEvent_ID, newEvent);
            }
            
        }

        public RestrictionProfile TimeFrameRestriction
        {
            get
            {
                return ProfileOfRestriction;
            }
        }

    }
}
