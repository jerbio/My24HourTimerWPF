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
            EventDisplay UiData, MiscData NoteData, bool EnabledEventFlag, bool CompletionFlag, TilerUser creator, TilerUserGroup users, string timeZone, EventID eventId,bool initializeSubCalendarEvents = true, Classification semantics = null, int split = 1)
            : base(
                 NameEntry, StartData, EndData, EventDuration, eventPrepTime, PreDeadlineTimeSpan, split, EventRepetitionEntry, UiData, NoteData, EnabledEventFlag, CompletionFlag, null, null, EventLocation, creator, users, false, DateTimeOffset.UtcNow, timeZone, semantics)
        {
            UniqueID = eventId ?? this.UniqueID; /// already initialized by parent initialization
            _RigidSchedule = true;
            if (IsRepeat || split == 0)
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

        protected RigidCalendarEvent():base()
        {
            _RigidSchedule = true;
        }

        public RigidCalendarEvent(CalendarEvent MyUpdated, SubCalendarEvent[] MySubEvents) : base(MyUpdated, MySubEvents)
        {
            _RigidSchedule = true;
        }

        public override void initializeSubEvents()
        {
            _SubEvents = new SubEventDictionary<string, SubCalendarEvent>();
            for (int i = 0; i < _Splits; i++)
            {
                SubCalendarEvent newSubCalEvent = new SubCalendarEvent(this, getCreator, _Users, _TimeZone, _AverageTimePerSplit, this.getName, (EndDateTime - _AverageTimePerSplit), this.End, new TimeSpan(), UniqueID.ToString(), _RigidSchedule, this._Enabled, this._UiParams, this.Notes, this._Complete, this._LocationInfo, this.StartToEnd);
                newSubCalEvent.TimeCreated = this.TimeCreated;
                _SubEvents.Add(newSubCalEvent.Id, newSubCalEvent);
            }
        }

        public override short updateNumberOfSplits(int SplitCOunt)
        {
            return base.updateNumberOfSplits(1);
        }
    }
}
