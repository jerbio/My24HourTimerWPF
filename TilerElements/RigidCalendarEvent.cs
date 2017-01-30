using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class RigidCalendarEvent: CalendarEvent
    {
        public RigidCalendarEvent(
            //EventID eventId, 
            EventName NameEntry, 
            DateTimeOffset StartData, 
            DateTimeOffset EndData, 
            TimeSpan EventDuration, 
            TimeSpan eventPrepTime, 
            TimeSpan PreDeadlineTimeSpan, 
            Repetition EventRepetitionEntry, 
            Location EventLocation, 
            EventDisplay UiData, MiscData NoteData, bool EnabledEventFlag, bool CompletionFlag, TilerUser creator, TilerUserGroup users, string timeZone, EventID eventId,bool initializeSubCalendarEvents = true)
            :base(
                 NameEntry, StartData, EndData, EventDuration, eventPrepTime, PreDeadlineTimeSpan, 1, EventRepetitionEntry, UiData, NoteData, EnabledEventFlag, CompletionFlag, null, null, EventLocation, creator, users, false, DateTimeOffset.UtcNow, timeZone)
        {
            UniqueID = eventId ?? this.UniqueID; /// already initialized by parent initialization
            RigidSchedule = true;
            if (EventRepetition.Enable)
            {
                _AverageTimePerSplit = new TimeSpan();
            }
            else
            {
                _AverageTimePerSplit = TimeSpan.FromTicks(((EventDuration.Ticks / Splits)));
            }
            if (initializeSubCalendarEvents)
            {
                initializeSubEvents();
            }

            EventSequence = new TimeLine(StartDateTime, EndDateTime);
            //UpdateLocationMatrix(LocationInfo);
        }

        protected RigidCalendarEvent(CalendarEvent MyUpdated, SubCalendarEvent[] MySubEvents) : base(MyUpdated, MySubEvents)
        {

        }

        public override void initializeSubEvents()
        {
            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            for (int i = 0; i < Splits; i++)
            {
                SubCalendarEvent newSubCalEvent = new SubCalendarEvent(getCreator, _Users, _TimeZone, _AverageTimePerSplit, this.getName, (EndDateTime - _AverageTimePerSplit), this.End, new TimeSpan(), UniqueID.ToString(), RigidSchedule, this.Enabled, this.UiParams, this.Notes, this.Complete, this.LocationInfo, this.RangeTimeLine);
                SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
            }
        }
    }
}
