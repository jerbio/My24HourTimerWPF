using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class ProcrastinateAllSubCalendarEvent:SubCalendarEvent
    {
        public ProcrastinateAllSubCalendarEvent(TimeLine timeLine, EventID calendarEventId):base()
        {
            this.StartDateTime = timeLine.Start;
            this.EndDateTime = timeLine.End;
            this.Rigid = true;
            this.UniqueID = EventID.generateGoogleSubCalendarEventID(calendarEventId);
        }
    }
}
