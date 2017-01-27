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
    class TestSchedule : Schedule
    {
        protected DateTimeOffset StartOfDay;
        UserAccount myAccount;
        public TestSchedule(UserAccount AccountEntry, DateTimeOffset referenceNow, DateTimeOffset startOfDay) : base(AccountEntry, referenceNow)
        {
            StartOfDay = startOfDay;
            Initialize(referenceNow, StartOfDay).Wait();
            myAccount = AccountEntry;
        }
        public TestSchedule(UserAccount AccountEntry, DateTimeOffset referenceNow) : base(AccountEntry, referenceNow)
        {}

        async virtual protected Task Initialize(DateTimeOffset referenceNow)
        {
            DateTimeOffset StartOfDay = await myAccount.ScheduleData.getDayReferenceTime().ConfigureAwait(false);
            _Now = new ReferenceNow(referenceNow, StartOfDay);
            Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location>> profileData = await myAccount.ScheduleData.getProfileInfo().ConfigureAwait(false);
            if (profileData != null)
            {
                DateTimeOffset referenceDayTimeNow = new DateTimeOffset(Now.calculationNow.Year, Now.calculationNow.Month, Now.calculationNow.Day, profileData.Item2.Hour, profileData.Item2.Minute, profileData.Item2.Second, new TimeSpan());// profileData.Item2;
                ReferenceDayTIime = Now.calculationNow < referenceDayTimeNow ? referenceDayTimeNow.AddDays(-1) : referenceDayTimeNow;
                AllEventDictionary = profileData.Item1;
                if (AllEventDictionary != null)
                {
                    //setAsComplete();
                    EventID.Initialize((uint)(myAccount.LastEventTopNodeID));
                    initializeThirdPartyCalendars();
                    updateThirdPartyCalendars(ThirdPartyControl.CalendarTool.outlook, new List<CalendarEvent>() { });
                    CompleteSchedule = getTimeLine();

                    //EventIDGenerator.Initialize((uint)(this.LastScheduleIDNumber));
                }
                Locations = profileData.Item3;
            }
        }

        async virtual protected Task Initialize(DateTimeOffset referenceNow, DateTimeOffset StartOfDay)
        {
            if (!myAccount.Status)
            {
                throw new Exception("Using non verified tiler Account, try logging into account first.");
            }
            _Now = new ReferenceNow(referenceNow, StartOfDay);
            Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location>> profileData = await myAccount.ScheduleData.getProfileInfo().ConfigureAwait(false);
            if (profileData != null)
            {
                DateTimeOffset referenceDayTimeNow = new DateTimeOffset(Now.calculationNow.Year, Now.calculationNow.Month, Now.calculationNow.Day, profileData.Item2.Hour, profileData.Item2.Minute, profileData.Item2.Second, new TimeSpan());// profileData.Item2;
                ReferenceDayTIime = Now.calculationNow < referenceDayTimeNow ? referenceDayTimeNow.AddDays(-1) : referenceDayTimeNow;
                AllEventDictionary = profileData.Item1;
                if (AllEventDictionary != null)
                {
                    //setAsComplete();
                    EventID.Initialize((uint)(myAccount.LastEventTopNodeID));
                    initializeThirdPartyCalendars();
                    updateThirdPartyCalendars(ThirdPartyControl.CalendarTool.outlook, new List<CalendarEvent>() { });
                    CompleteSchedule = getTimeLine();
                }
                Locations = profileData.Item3;
            }
        }

        public TestSchedule(IEnumerable<CalendarEvent> calendarEvents ,UserAccount AccountEntry, DateTimeOffset referenceNow) : base(AccountEntry, referenceNow)
        {
            AllEventDictionary =  calendarEvents.ToDictionary(calEvent => calEvent.Calendar_EventID.getCalendarEventComponent(), calEvent => calEvent);
        }

        public Health getScheduleQuality(TimeLine timeLine)
        {
            Health retValue = this.getScheduleQuality(timeLine, this.Now);
            return retValue;
        }

        public Health getScheduleQuality(TimeLine timeLine, ReferenceNow refNow)
        {
            Health retValue = new Health(this.getAllCalendarEvents(), timeLine.Start, timeLine.TimelineSpan, refNow);
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
