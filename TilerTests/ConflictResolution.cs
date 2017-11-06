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
    public class ConflictResolution
    {
        /*
         *current utc time is April 18, 2017 10:40:59 pm +/- 5 mins current end of day 10:00pm
        When I shuffle for some reason there is an unnecessary conflict with 7170280_7_0_7170281 and 7156969_7_0_7156970.(Work on test bug ibm steez & Workout) Event though there is enough time in the day to let both time frames.
        */
        [TestMethod]
        public void file499a0ab4()
        {
            Location homeLocation = TestUtility.getLocations()[0];
            DateTimeOffset startOfDay = DateTimeOffset.Parse("2:00am");
            UserAccount currentUser = TestUtility.getTestUser(userId: "499a0ab4-81d7-42df-a476-44fc4348e94b");
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Parse("04/18/2017 10:41pm ");
            TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
            var resultOfShuffle = schedule.FindMeSomethingToDo(homeLocation);
            resultOfShuffle.Wait();
            schedule.WriteFullScheduleToLogAndOutlook().Wait();
            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            SubCalendarEvent subEventA = schedule.getSubCalendarEvent("7170280_7_0_7170281");
            SubCalendarEvent subEventB = schedule.getSubCalendarEvent("7156969_7_0_7156970");
            Assert.IsFalse(subEventB.ActiveSlot.doesTimeLineInterfere(subEventA.ActiveSlot));// this is known to fail
        }
    }
}
