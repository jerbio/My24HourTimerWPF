using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class ProcrastinateAllCalendarEvent:CalendarEvent
    {
        static int numberOfYearsFromNow;
        public static EventID ProcrastinateAllEventId;
        public ProcrastinateAllCalendarEvent(): base()
        {
            StartDateTime = new DateTimeOffset();
            this.UniqueID = ProcrastinateAllEventId;
            EndDateTime = DateTimeOffset.UtcNow.AddYears(numberOfYearsFromNow);
        }

        public ProcrastinateAllCalendarEvent(IEnumerable<SubCalendarEvent> allSubEvents) : this()
        {
            this.Rigid = true;
            this.SubEvents = AllSubEvents.Cast<ProcrastinateAllSubCalendarEvent>().ToDictionary(subEvent => subEvent.SubEvent_ID, subEvent => (SubCalendarEvent)subEvent);
        }

        public ProcrastinateAllCalendarEvent(TimeSpan duration, DateTimeOffset currentTime):this()
        {
            this.Rigid = true;
            if (SubEvents != null)
            {
                if(SubEvents.Count() == 0)
                {
                    initializeSubEvent();
                }
            } else
            {
                initializeSubEvent();
            }
        }
        public ProcrastinateAllCalendarEvent(TimeSpan duration) : this(duration, DateTimeOffset.UtcNow)
        {

        }

        void initializeSubEvent()
        {
            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            SubCalendarEvent emptySubEvent = createProcrastinateAllSubEvent(new TimeLine());
            SubEvents.Add(emptySubEvent.SubEvent_ID, emptySubEvent);
        }


        virtual protected ProcrastinateAllSubCalendarEvent createProcrastinateAllSubEvent(TimeLine timeLine)
        {
            ProcrastinateAllSubCalendarEvent retValue = new ProcrastinateAllSubCalendarEvent(timeLine, this.Calendar_EventID);
            return retValue;
        }

        public void createProcrastinationSubEvent(TimeLine timeLine)
        {
            ProcrastinateAllSubCalendarEvent procrastinateSubEvent = createProcrastinateAllSubEvent(timeLine);
            SubEvents.Add(procrastinateSubEvent.SubEvent_ID, procrastinateSubEvent);
            EventDuration += timeLine.TotalActiveSpan;
            ++Splits;
            UpdateTimePerSplit();
        }

    }
}
