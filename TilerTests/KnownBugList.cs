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
         * This test evaluates the sleep spacing.
         * The 
        */

        /// <summary>
        /// This test evaluates the sleep spacing available between Dec 31 2017 and jan 1 2018. 
        /// When the shuffle is hit there should be a spacing of at least five hours hours somewhere in the schedule however for some reason the best spacing 50 mins
        /// </summary>
        [TestMethod]
        public void file_e94713a5()
        {
            string currentClearAllId = "4939920_7_0_0";
            Location homeLocation = TestUtility.getLocations()[0];
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
            DateTimeOffset refNow = TestUtility.parseAsUTC("12/31/2017 9:10pm");
            UserAccount currentUser = TestUtility.getTestUser(userId: "e94713a5-9ab6-4c6b-9b46-edafa4b0dafc");
            currentUser.getTilerUser().ClearAllId = currentClearAllId;
            TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.FindMeSomethingToDo(homeLocation).Wait();
            schedule.WriteFullScheduleToLogAndOutlook().Wait();
            DayTimeLine day0 = schedule.Now.getDayTimeLineByTime(refNow.AddDays(0));
            DayTimeLine day1 = schedule.Now.getDayTimeLineByTime(refNow.AddDays(1));
            TimeLine sleepTimeLine = new TimeLine(day0.SleepSubEvent.End, day1.WakeSubEvent.Start);
            Assert.IsTrue(sleepTimeLine.TimelineSpan > TimeSpan.FromHours(5));// This is known to fail
        }

        /// <summary>
        /// There is an unnecessary conflict on February 27 and shuffling doesn't help. Shuffling the schedule should sort out the conflict. I feel it has something to do with the end of the day.
        /// </summary>
        [TestMethod]
        public void file_6439fc14()
        {
            string currentClearAllId = "4939920_7_0_0";
            Location homeLocation = TestUtility.getLocations()[0];
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
            DateTimeOffset refNow = TestUtility.parseAsUTC("2/27/2018 12:36am");
            TimeLine conflictingTimeline = new TimeLine(refNow.AddDays(-1), refNow.AddDays(1));
            UserAccount currentUser = TestUtility.getTestUser(userId: "6439fc14-ad0d-419f-acc8-86b17cc100c2");
            currentUser.getTilerUser().ClearAllId = currentClearAllId;
            TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
            List<SubCalendarEvent> allSubeventsWInthinPertinentTImeLine = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.ActiveSubEvents).Where(subEvent => subEvent.ActiveSlot.doesTimeLineInterfere(conflictingTimeline)).ToList();
            Utility.ConflictEvaluation conflictEvaluation = new Utility.ConflictEvaluation(allSubeventsWInthinPertinentTImeLine);
            Assert.IsTrue(conflictEvaluation.ConflictingTimeRange.Count() > 0);
            schedule.FindMeSomethingToDo(homeLocation).Wait();
            schedule.WriteFullScheduleToLogAndOutlook().Wait();
            allSubeventsWInthinPertinentTImeLine = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.ActiveSubEvents).Where(subEvent => subEvent.ActiveSlot.doesTimeLineInterfere(conflictingTimeline)).ToList();
            conflictEvaluation = new Utility.ConflictEvaluation(allSubeventsWInthinPertinentTImeLine);
            Assert.IsTrue(conflictEvaluation.ConflictingTimeRange.Count() == 0);// This is known to fail
        }

        /// <summary>
        /// There is a crash when I try to add a 45 min home locationed tile that deadlines at on 11:59p 2/26/2018 EST.
        /// </summary>
        [TestMethod]
        public void file_2ndBug_6439fc14()
        {
            string currentClearAllId = "4939920_7_0_0";
            Location homeLocation = TestUtility.getLocations()[0];
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
            DateTimeOffset refNow = TestUtility.parseAsUTC("2/27/2018 12:36am");
            UserAccount currentUser = TestUtility.getTestUser(userId: "6439fc14-ad0d-419f-acc8-86b17cc100c2");
            CalendarEvent calEvent = TestUtility.generateCalendarEvent(TimeSpan.FromMinutes(45), new Repetition(), refNow, refNow.AddHours(4), location: homeLocation);
            currentUser.getTilerUser().ClearAllId = currentClearAllId;
            TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(calEvent);
        }

        [TestCleanup]
        public void eachTestCleanUp()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            Schedule Schedule = new TestSchedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }
    }
}
