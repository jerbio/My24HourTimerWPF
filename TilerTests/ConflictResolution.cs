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
        public void file_499a0ab4()
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
            Assert.IsFalse(subEventB.ActiveSlot.doesTimeLineInterfere(subEventA.ActiveSlot)); // This is known to fail and is on bug list
        }


        // Current UTC time: 10/23/2017 7:00:34 PM +00:00
        // End of day is : 10:00pm Est
        // For some reason adding a 30 min event with a deadline of 10/24/2017 3:59:00 AM(UTC), triggers unnecessary conflicts.With the events 8615397_7_0_8615398 and 8615271_7_0_8615272(IBM steez, check amqp stability and workout respectively). Not you have chill at work on your schedule from 9a - 6p(est) via google calendar.
       [TestMethod]
        public void file_712dd797()
        {
            Location homeLocation = TestUtility.getLocations()[0];
            DateTimeOffset startOfDay = DateTimeOffset.Parse("2:00am");
            DateTimeOffset refNow = DateTimeOffset.Parse("10/23/2017 7:00:34PM");
            DateTimeOffset endOfEvent = DateTimeOffset.Parse("10/24/2017 3:59:00AM");
            UserAccount currentUser = TestUtility.getTestUser(userId: "712dd797-8991-4f79-90c1-7b51c744c4bd");
            TimeLine actualTime = new TimeLine(DateTimeOffset.Parse("10/21/2017 1:00PM"), DateTimeOffset.Parse("10/21/2017 10:00PM"));
            Repetition repeating = new Repetition(true, new TimeLine(startOfDay.AddDays(-5), startOfDay.AddDays(18)), Repetition.Frequency.DAILY, actualTime);
            CalendarEvent repeatingCalEvent = TestUtility.generateCalendarEvent(actualTime.TimelineSpan, repeating, actualTime.Start, actualTime.End, rigidFlags: true);// simulation of google cal event
            TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(repeatingCalEvent);
            currentUser.Login().Wait();
            CalendarEvent calEvent = TestUtility.generateCalendarEvent(TimeSpan.FromMinutes(30), new Repetition(), refNow, endOfEvent);
            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(calEvent);
            SubCalendarEvent subEventA = schedule.getSubCalendarEvent("8615397_7_0_8615398");
            SubCalendarEvent subEventB = schedule.getSubCalendarEvent("8615271_7_0_8615272");
            Assert.IsFalse(subEventB.ActiveSlot.doesTimeLineInterfere(subEventA.ActiveSlot));
        }
    }
}
