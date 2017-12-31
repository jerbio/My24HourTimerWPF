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
            DateTimeOffset startOfDay = DateTimeOffset.Parse("2:00am");
            DateTimeOffset refNow = DateTimeOffset.Parse("12/31/2017 9:10pm");
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
    }
}
