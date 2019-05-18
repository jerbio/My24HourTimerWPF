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
        public TestSchedule(UserAccount AccountEntry, DateTimeOffset referenceNow, DateTimeOffset startOfDay, uint LatestId = 0, DataRetrivalOption retrievalOption = DataRetrivalOption.Evaluation) : base(AccountEntry, referenceNow, retrievalOption)
        {
            StartOfDay = startOfDay;
            this.retrievalOption = retrievalOption;
            Initialize(referenceNow, StartOfDay).Wait();
            myAccount = AccountEntry;
            if (LatestId != 0)
            {
                EventID.Initialize(LatestId);
            }
        }
        public TestSchedule(UserAccount AccountEntry, DateTimeOffset referenceNow, uint LatestId = 0, DataRetrivalOption retrievalOption = DataRetrivalOption.Evaluation) : base(AccountEntry, referenceNow, retrievalOption)
        {
            if (LatestId != 0)
            {
                EventID.Initialize(LatestId);
            }
        }

        async override protected Task Initialize(DateTimeOffset referenceNow)
        {
            DateTimeOffset StartOfDay = myAccount.ScheduleData.getDayReferenceTime();
            _Now = new ReferenceNow(referenceNow, StartOfDay);
            TimeLine RangeOfLookup = null;
            Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location>> profileData = await myAccount.ScheduleData.getProfileInfo(RangeOfLookup, _Now, retrievalOption).ConfigureAwait(false);
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
            TilerUser = myAccount.getTilerUser();
        }

        public TestSchedule(IEnumerable<CalendarEvent> calendarEvents ,UserAccount AccountEntry, DateTimeOffset referenceNow, IEnumerable<Location> Locations, uint LatestId = 0) : base(AccountEntry, referenceNow)
        {
            AllEventDictionary =  calendarEvents.ToDictionary(calEvent => calEvent.Calendar_EventID.getCalendarEventComponent(), calEvent => calEvent);
            this.Locations = Locations.ToDictionary(obj => obj.Description, obj => obj);
            if (LatestId != 0)
            {
                EventID.Initialize(LatestId);
            }
        }

        public TestSchedule(ScheduleDump scheduleDump, UserAccount AccountEntry, uint LatestId = 0) : base(AccountEntry, scheduleDump.ReferenceNow)
        {
            _Now = new ReferenceNow(scheduleDump.ReferenceNow, scheduleDump.StartOfDay);
            this.myAccount.ScheduleLogControl.Now = _Now;
            AllEventDictionary = AccountEntry.ScheduleLogControl.getAllCalendarFromXml(scheduleDump);
            this.Locations = AccountEntry.ScheduleLogControl.getLocationCache(scheduleDump);
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
            IEnumerable<CalendarEvent> calendarEvents = getAllCalendarEvents().Where(calEvent => calEvent.isActive);
            foreach(SubCalendarEvent subevent in  calendarEvents.SelectMany(calEvent => calEvent.ActiveSubEvents))
            {
                DayTimeLine dayTimeLineStart  = Now.getDayTimeLineByTime(subevent.Start);
                DayTimeLine dayTimeLineAfter = Now.getDayTimeLineByTime(subevent.Start);
                dayTimeLineStart.AddToSubEventList(subevent);
                dayTimeLineAfter.AddToSubEventList(subevent);
            }
        }
    }
}
