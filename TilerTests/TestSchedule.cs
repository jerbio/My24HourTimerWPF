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
    internal class TestSchedule : DB_Schedule
    {
        protected DateTimeOffset StartOfDay;
        public TestSchedule(UserAccount AccountEntry, DateTimeOffset referenceNow, DateTimeOffset startOfDay, uint LatestId = 0, DataRetrivalOption retrievalOption = DataRetrivalOption.Evaluation, TimeLine rangeOfLookup = null, HashSet<string> calendarIds = null) : base(AccountEntry, referenceNow, startOfDay, retrievalOption, rangeOfLookup, calendarIds: calendarIds)
        {
            StartOfDay = startOfDay;
            this.retrievalOption = retrievalOption;
            myAccount = AccountEntry;
            if (LatestId != 0)
            {
                EventID.Initialize(LatestId);
            }
        }
        public TestSchedule(UserAccount AccountEntry, DateTimeOffset referenceNow, uint LatestId = 0, DataRetrivalOption retrievalOption = DataRetrivalOption.Evaluation, TimeLine rangeOfLookup = null, HashSet<string> calendarIds = null) : base(AccountEntry, referenceNow, retrievalOption, rangeOfLookup, calendarIds: calendarIds)
        {
            if (LatestId != 0)
            {
                EventID.Initialize(LatestId);
            }
        }

        async override protected Task Initialize(DateTimeOffset referenceNow, DateTimeOffset StartOfDay, HashSet<string> calendarIds = null)
        {
            _Now = new ReferenceNow(referenceNow, StartOfDay, myAccount.getTilerUser().TimeZoneDifference);
            this.RangeOfLookup = this.RangeOfLookup ?? new TimeLine(_Now.constNow.AddDays(DB_Schedule.TimeLookUpDayStart), _Now.constNow.AddDays(DB_Schedule.TimeLookUpDayEnd));
            Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location>> profileData = await myAccount.ScheduleData.getProfileInfo(RangeOfLookup, _Now, retrievalOption, calendarIds: calendarIds).ConfigureAwait(false);
            TravelCache travelCache = await myAccount.ScheduleData.getTravelCache(myAccount.UserID).ConfigureAwait(false);
            updateTravelCache(travelCache);
            myAccount.Now = _Now;
            if (profileData != null)
            {
                DateTimeOffset referenceDayTimeNow = new DateTimeOffset(Now.calculationNow.Year, Now.calculationNow.Month, Now.calculationNow.Day, profileData.Item2.Hour, profileData.Item2.Minute, profileData.Item2.Second, new TimeSpan());// profileData.Item2;
                ReferenceDayTIime = Now.calculationNow < referenceDayTimeNow ? referenceDayTimeNow.AddDays(-1) : referenceDayTimeNow;
                initializeAllEventDictionary(profileData.Item1.Values);
                if (AllEventDictionary != null)
                {
                    EventID.Initialize((uint)(myAccount.LastEventTopNodeID));
                    initializeThirdPartyCalendars();
                    updateThirdPartyCalendars(ThirdPartyControl.CalendarTool.outlook, new List<CalendarEvent>() { });
                    CompleteSchedule = getTimeLine();
                }
                Locations = profileData.Item3;
            }
            TilerUser = myAccount.getTilerUser();
        }

        async override protected Task Initialize(DateTimeOffset referenceNow, HashSet<string> calendarIds = null)
        {
            DateTimeOffset StartOfDay = myAccount.ScheduleData.getDayReferenceTime();
            await Initialize(referenceNow, StartOfDay, calendarIds).ConfigureAwait(false);
        }

        public TestSchedule(IEnumerable<CalendarEvent> calendarEvents ,UserAccount AccountEntry, DateTimeOffset referenceNow, IEnumerable<Location> Locations, uint LatestId = 0) : base(AccountEntry, referenceNow)
        {
            initializeAllEventDictionary(calendarEvents);
            this.Locations = Locations.ToDictionary(obj => obj.Description, obj => obj);
            if (LatestId != 0)
            {
                EventID.Initialize(LatestId);
            }
        }

        public TestSchedule(ScheduleDump scheduleDump, UserAccount AccountEntry, DateTimeOffset referenceNow, uint LatestId = 0) : base()
        {
            myAccount = AccountEntry;
            TilerUser = AccountEntry.getTilerUser();
            _Now = new ReferenceNow(referenceNow, scheduleDump.StartOfDay, myAccount.getTilerUser().TimeZoneDifference);
            this.myAccount.ScheduleLogControl.Now = _Now;

            TravelCache travelCache = new TravelCache()
            {
                Id = TilerUser.Id
            };
            updateTravelCache(travelCache);
            var scheduleData = AccountEntry.ScheduleLogControl.getAllCalendarFromXml(scheduleDump, _Now);
            initializeAllEventDictionary(scheduleData.Item1.Values);
            ThirdPartyCalendars = scheduleData.Item2;
            foreach (CalendarEvent calEvent in ThirdPartyCalendars.SelectMany(o=>o.Value))
            {
                addCalendarEventToAllEventDictionary(calEvent);
            }
            ReferenceDayTIime = Now.calculationNow;
            this.Locations = AccountEntry.ScheduleLogControl.getLocationCache(scheduleDump);
            CompleteSchedule = getTimeLine();
            if (LatestId != 0)
            {
                EventID.Initialize(LatestId);
            }
        }

        public TestSchedule(ScheduleDump scheduleDump, UserAccount AccountEntry, uint LatestId = 0) : this(scheduleDump, AccountEntry, scheduleDump.ReferenceNow)
        {
        }

        public override Task<CustomErrors> AddToScheduleAndCommitAsync(CalendarEvent NewEvent, bool optimizeSchedule = false)
        {
            return base.AddToScheduleAndCommitAsync(NewEvent, optimizeSchedule);
        }


        //public Health getScheduleQuality(TimeLine timeLine)
        //{
        //    Health retValue = this.getScheduleQuality(timeLine, this.Now);
        //    return retValue;
        //}

        //public Health getScheduleQuality(TimeLine timeLine, ReferenceNow refNow)
        //{
        //    Health retValue = new Health(this.getAllCalendarEvents(), timeLine.Start, timeLine.TimelineSpan, refNow, this.getHomeLocation);
        //    return retValue;
        //}

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
