using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using My24HourTimerWPF;
using TilerElements;
using System.Collections.Generic;
using System.Linq;
using TilerCore;

namespace TilerTests
{
    [TestClass]
    public class KnownBugList
    {

        /*
         * This test tries to see that there is sufficent diversity. Read notes scheule dump nodes
         * The 
        */
        /// <summary>
        /// Template for running test environment through log files.
        /// You need to run this in UTC TimeZone
        /// </summary>

        [TestMethod]
        public void file_template()
        {
            string scheduleId = "24fe78f8-b9a9-4ca3-b4a1-cf5d458fe385";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            Schedule schedule = scheduleAndDump.Item1;
            ((TestSchedule)schedule).WriteFullScheduleToOutlook();
        }


        /// <summary>
        /// Bug creates a scenario where hitting "do now" some how creates multiple subevents of the same calendar events on the same day
        /// </summary>
        [TestMethod]
        public void file_24fe78f8()
        {
            string scheduleId = "24fe78f8-b9a9-4ca3-b4a1-cf5d458fe385";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            Schedule schedule = scheduleAndDump.Item1;

            TimeLine timeLine = new TimeLine(schedule.Now.constNow, schedule.Now.constNow.AddHours(3));
            var subEvents = schedule.getAllCalendarEvents().SelectMany(cal => cal.ActiveSubEvents).Where(obj => obj.canExistWithinTimeLine(timeLine)).ToList();
            var setAsNowCalEvent = subEvents.Where(sub => sub.Name.Name.ToLower().Contains("work")).First();
            var asNowResult = schedule.SetCalendarEventAsNow(setAsNowCalEvent.Id);
            var calendarEvent = schedule.getCalendarEvent(setAsNowCalEvent.Id);
            var interferringSubEvents = schedule.getAllCalendarEvents().SelectMany(cal => cal.ActiveSubEvents).Where(obj => obj.ActiveSlot.doesTimeLineInterfere(schedule.Now.firstDay)).ToList();
            var subeventsOfTheSameCalendar = interferringSubEvents.Where(subEvent => calendarEvent.getTilerID.getCalendarEventComponent() == subEvent.getTilerID.getCalendarEventComponent());
            Assert.IsTrue(subeventsOfTheSameCalendar.Count() == 1);
            ((TestSchedule)schedule).WriteFullScheduleToOutlook();
        }

        public void add9_5WorkSchedule (Schedule schedule)
        {
            string date = "" + schedule.Now.constNow.Month + "/" + schedule.Now.constNow.Day + "/" + schedule.Now.constNow.Year;
            TimeLine timeLine = new TimeLine(TestUtility.parseAsUTC("3:00pm " + date), TestUtility.parseAsUTC("11:00pm " + date));
            TimeLine rangeOfTempGoogleEvent = new TimeLine(timeLine.Start, timeLine.Start.AddYears(1));
            DayOfWeek[] weekDays = (new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }).ToArray();
            EventName eventName = new EventName(null, null, "google-made-up");
            Repetition repeat = new Repetition(rangeOfTempGoogleEvent, Repetition.Frequency.WEEKLY, timeLine, weekDays);
            CalendarEvent googleCalSimulation = TestUtility.generateCalendarEvent(
                schedule.User,
                TimeSpan.FromHours(8),
                repeat,
                rangeOfTempGoogleEvent.Start,
                rangeOfTempGoogleEvent.End,
                1,
                true);
            schedule.AddToSchedule(googleCalSimulation);
        }


        //[TestMethod]
        //public void file_e94713a5()
        //{
        //    string currentClearAllId = "4939920_7_0_0";
        //    Location homeLocation = TestUtility.getLocations()[0];
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
        //    DateTimeOffset refNow = TestUtility.parseAsUTC("12/31/2017 9:10pm");
        //    UserAccount currentUser = TestUtility.getTestUser(userId: "e94713a5-9ab6-4c6b-9b46-edafa4b0dafc");
        //    currentUser.getTilerUser().ClearAllId = currentClearAllId;
        //    TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
        //    schedule.FindMeSomethingToDo(homeLocation).Wait();
        //    schedule.WriteFullScheduleToLogAndOutlook().Wait();
        //    DayTimeLine day0 = schedule.Now.getDayTimeLineByTime(refNow.AddDays(0));
        //    DayTimeLine day1 = schedule.Now.getDayTimeLineByTime(refNow.AddDays(1));
        //    TimeLine sleepTimeLine = new TimeLine(day0.SleepSubEvent.End, day1.WakeSubEvent.Start);
        //    Assert.IsTrue(sleepTimeLine.TimelineSpan > TimeSpan.FromHours(5));// This is known to fail
        //}

        ///// <summary>
        ///// There is an unnecessary conflict on February 27 and shuffling doesn't help. Shuffling the schedule should sort out the conflict. I feel it has something to do with the end of the day.
        ///// </summary>
        //[TestMethod]
        //public void file_6439fc14()
        //{
        //    string currentClearAllId = "4939920_7_0_0";
        //    Location homeLocation = TestUtility.getLocations()[0];
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
        //    DateTimeOffset refNow = TestUtility.parseAsUTC("2/27/2018 12:36am");
        //    TimeLine conflictingTimeline = new TimeLine(refNow.AddDays(-1), refNow.AddDays(1));
        //    UserAccount currentUser = TestUtility.getTestUser(userId: "6439fc14-ad0d-419f-acc8-86b17cc100c2");
        //    currentUser.getTilerUser().ClearAllId = currentClearAllId;
        //    TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
        //    List<SubCalendarEvent> allSubeventsWInthinPertinentTImeLine = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.ActiveSubEvents).Where(subEvent => subEvent.ActiveSlot.doesTimeLineInterfere(conflictingTimeline)).ToList();
        //    Utility.ConflictEvaluation conflictEvaluation = new Utility.ConflictEvaluation(allSubeventsWInthinPertinentTImeLine);
        //    Assert.IsTrue(conflictEvaluation.ConflictingTimeRange.Count() > 0);
        //    schedule.FindMeSomethingToDo(homeLocation).Wait();
        //    schedule.WriteFullScheduleToLogAndOutlook().Wait();
        //    allSubeventsWInthinPertinentTImeLine = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.ActiveSubEvents).Where(subEvent => subEvent.ActiveSlot.doesTimeLineInterfere(conflictingTimeline)).ToList();
        //    conflictEvaluation = new Utility.ConflictEvaluation(allSubeventsWInthinPertinentTImeLine);
        //    Assert.IsTrue(conflictEvaluation.ConflictingTimeRange.Count() == 0);// This is known to fail
        //}

        ///// <summary>
        ///// There is a crash when I try to add a 45 min home locationed tile that deadlines at on 11:59p 2/26/2018 EST.
        ///// </summary>
        //[TestMethod]
        //public void file_2ndBug_6439fc14()
        //{
        //    string currentClearAllId = "4939920_7_0_0";
        //    Location homeLocation = TestUtility.getLocations()[0];
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
        //    DateTimeOffset refNow = TestUtility.parseAsUTC("2/27/2018 12:36am");
        //    UserAccount currentUser = TestUtility.getTestUser(userId: "6439fc14-ad0d-419f-acc8-86b17cc100c2");
        //    CalendarEvent calEvent = TestUtility.generateCalendarEvent(TimeSpan.FromMinutes(45), new Repetition(), refNow, refNow.AddHours(4), location: homeLocation);
        //    currentUser.getTilerUser().ClearAllId = currentClearAllId;
        //    TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
        //    schedule.AddToScheduleAndCommit(calEvent);
        //}

        //[TestCleanup]
        //public void eachTestCleanUp()
        //{
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    Schedule Schedule = new TestSchedule(currentUser, refNow);
        //    currentUser.DeleteAllCalendarEvents();
        //}
    }
}
