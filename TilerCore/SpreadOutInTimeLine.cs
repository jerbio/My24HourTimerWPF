using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TilerElements;



namespace TilerCore
{
    public class SpreadOutInTimeLine
    {
        List<TimelineWithSubcalendarEvents> Timelines;
        List<SubCalendarEvent> SubcalendarEvents;
        public SpreadOutInTimeLine (List<TimelineWithSubcalendarEvents> timeline, IEnumerable<SubCalendarEvent> subcalEvents)
        {
            Timelines = timeline.ToList();
            SubcalendarEvents = subcalEvents.ToList();
        }
    }
}
