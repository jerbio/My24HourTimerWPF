using My24HourTimerWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;
using TilerFront;
using GoogleMapsApi.Entities.Directions.Request;

namespace TilerTests
{
    class TestSchedule : DB_Schedule
    {
        protected DateTimeOffset StartOfDay;
        public TestSchedule(UserAccount AccountEntry, DateTimeOffset referenceNow, DateTimeOffset startOfDay, uint LatestId = 0) : base(AccountEntry, referenceNow)
        {
            StartOfDay = startOfDay;
            Initialize(referenceNow, StartOfDay).Wait();
            myAccount = AccountEntry;
            if (LatestId != 0)
            {
                EventID.Initialize(LatestId);
            }
        }
        public TestSchedule(UserAccount AccountEntry, DateTimeOffset referenceNow, uint LatestId = 0) : base(AccountEntry, referenceNow)
        {
            if(LatestId!=0)
            {
                EventID.Initialize(LatestId);
            }
        }

        public TestSchedule(IEnumerable<CalendarEvent> calendarEvents ,UserAccount AccountEntry, DateTimeOffset referenceNow, uint LatestId = 0) : base(AccountEntry, referenceNow)
        {
            AllEventDictionary =  calendarEvents.ToDictionary(calEvent => calEvent.Calendar_EventID.getCalendarEventComponent(), calEvent => calEvent);
            if (LatestId != 0)
            {
                EventID.Initialize(LatestId);
            }
        }

        public override Task<CustomErrors> AddToScheduleAndCommit(CalendarEvent NewEvent, bool optimizeSchedule = false)
        {
            return base.AddToScheduleAndCommit(NewEvent, optimizeSchedule);
        }


        public Health getScheduleQuality(TimeLine timeLine)
        {
            Health retValue = this.getScheduleQuality(timeLine, this.Now);
            return retValue;
        }

        public Health getScheduleQuality(TimeLine timeLine, ReferenceNow refNow)
        {
            Health retValue = new Health(this.getAllCalendarEvents(), timeLine.Start, timeLine.TimelineSpan, refNow, this.getHomeLocation);
            return retValue;
        }

        public void populateDayTimeLinesWithSubcalendarEvents()
        {
            IEnumerable<CalendarEvent> calendarEvents = getAllCalendarEvents();
            foreach(SubCalendarEvent subevent in  calendarEvents.SelectMany(calEvent => calEvent.AllSubEvents))
            {
                DayTimeLine dayTimeLineStart  = Now.getDayTimeLineByTime(subevent.Start);
                DayTimeLine dayTimeLineAfter = Now.getDayTimeLineByTime(subevent.Start);
                dayTimeLineStart.AddToSubEventList(subevent);
                dayTimeLineAfter.AddToSubEventList(subevent);
            }
        }
    }
}
