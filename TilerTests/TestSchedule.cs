using My24HourTimerWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;
using TilerFront;

namespace TilerTests
{
    class TestSchedule : Schedule
    {
        public TestSchedule(UserAccount AccountEntry, DateTimeOffset referenceNow) : base(AccountEntry, referenceNow)
        {}

        public TestSchedule(IEnumerable<CalendarEvent> calendarEvents ,UserAccount AccountEntry, DateTimeOffset referenceNow) : base(AccountEntry, referenceNow)
        {
            AllEventDictionary =  calendarEvents.ToDictionary(calEvent => calEvent.Calendar_EventID.getCalendarEventComponent(), calEvent => calEvent);
        }

        public Health getScheduleQuality(TimeLine timeLine, ReferenceNow refNow)
        {
            Health retValue = new Health(this.getAllCalendarEvents(), timeLine.Start, timeLine.TimelineSpan, refNow);
            return retValue;
        }
    }
}
