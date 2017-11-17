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
            if (_EventRepetition.Enable)
            {
                _AverageTimePerSplit = new TimeSpan();
            }
            else
            {
                _AverageTimePerSplit = TimeSpan.FromTicks(((EventDuration.Ticks / _Splits)));
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
            for (int i = 0; i < _Splits; i++)
            {
                SubCalendarEvent newSubCalEvent = new SubCalendarEvent(this, getCreator, _Users, _TimeZone, _AverageTimePerSplit, this.getName, (EndDateTime - _AverageTimePerSplit), this.End, new TimeSpan(), UniqueID.ToString(), RigidSchedule, this._Enabled, this._UiParams, this.Notes, this._Complete, this._LocationInfo, this.RangeTimeLine);
                newSubCalEvent.TimeCreated = this.TimeCreated;
                SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
            }
        }
    }
}
