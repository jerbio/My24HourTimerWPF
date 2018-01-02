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

        // Current UTC time: 11/9/2017 5:28am utc
        // End of day is : 10:00pm Est
        // No matter how much I increase the number of splits count on this calendar event it never goes above 3
        [TestMethod]
        public void file_a56a5ac5()
        {
            string subEventId = "6418072_7_0_6418075";
            Location homeLocation = TestUtility.getLocations()[0];
            DateTimeOffset startOfDay = DateTimeOffset.Parse("2:00am");
            DateTimeOffset refNow = DateTimeOffset.Parse("11/9/2017 5:28am");
            UserAccount currentUser = TestUtility.getTestUser(userId: "a56a5ac5-b474-4d4e-b878-bbb593a0d5b1");
            TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
            CalendarEvent readjustCalendarEvent = schedule.getCalendarEvent("6418072_7_0_6418075");
            const int updatedSplitCount = 11;
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> updateResult= schedule.BundleChangeUpdate(readjustCalendarEvent.ActiveSubEvents.First().getId,
                readjustCalendarEvent.getName.NameValue,
                readjustCalendarEvent.ActiveSubEvents.First().Start,
                readjustCalendarEvent.ActiveSubEvents.First().End,
                readjustCalendarEvent.Start,
                readjustCalendarEvent.End,
                updatedSplitCount, 
                readjustCalendarEvent.Notes.UserNote);
            schedule.UpdateWithDifferentSchedule(updateResult.Item2).Wait();
            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            CalendarEvent latestCalendarEvent = schedule.getCalendarEvent(subEventId);
            Assert.AreEqual(latestCalendarEvent.NumberOfSplit, updatedSplitCount);
        }

        // Current UTC time: 12/2/2017 8:20pm utc
        // End of day is : 10:00pm Est
        // Trying to update the deadline of 8631313_7_0_8631314 to the time 12/3/2017 4:59pm causes it to crash
        [TestMethod]
        public void file_b10cdae0()
        {
            string subEventId = "8631313_7_0_8631314";
            Location homeLocation = TestUtility.getLocations()[0];
            DateTimeOffset startOfDay = DateTimeOffset.Parse("2:00am");
            DateTimeOffset refNow = DateTimeOffset.Parse("12/2/2017 8:20pm");
            DateTimeOffset deadline = DateTimeOffset.Parse("12/3/2017 4:59pm");
            UserAccount currentUser = TestUtility.getTestUser(userId: "b10cdae0-64e1-498f-82a2-8601da577255");
            TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
            CalendarEvent calEvent = schedule.getCalendarEvent(subEventId);
            SubCalendarEvent subEvent = schedule.getSubCalendarEvent(subEventId);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> updateResult = schedule.BundleChangeUpdate(
                subEvent.getId,
                subEvent.getName,
                subEvent.Start,
                subEvent.End,
                subEvent.getCalendarEventRange.Start,
                deadline,
                1, 
                subEvent.Notes.UserNote); ///this is known to fail
            schedule.UpdateWithDifferentSchedule(updateResult.Item2).Wait();
            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            
            Assert.AreEqual(((SubCalendarEventRestricted)schedule.getSubCalendarEvent(subEventId)).getHardCalendarEventRange.End, deadline);
        }

        /// <summary>
        /// For some reason wheneve I hit the shuffle button there is always a conflict generated by the 
        /// sub event 8969308_7_0_8969309, named 'Migrate to rdbms - update/remove write single calendar event to xml' even though
        /// there is enough space after the sub event '9105097_7_0_9105098' named 'Debug why google calendar isn't showing'
        /// </summary>
        [TestMethod]
        public void file_c1fbe98f()
        {
            string currentClearAllId = "4939920_7_0_0";
            string subEventId = "8969308_7_0_8969309";
            string conflictingSubEventId = "9105097_7_0_9105098";
            Location homeLocation = TestUtility.getLocations()[0];
            DateTimeOffset startOfDay = DateTimeOffset.Parse("2:00am");
            DateTimeOffset refNow = DateTimeOffset.Parse("12/30/2017 8:39am");
            UserAccount currentUser = TestUtility.getTestUser(userId: "c1fbe98f-cb7e-4b21-9d99-b4c3acaf670c");
            currentUser.getTilerUser().ClearAllId = currentClearAllId;
            TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.FindMeSomethingToDo(homeLocation).Wait();
            schedule.WriteFullScheduleToLogAndOutlook().Wait();
            schedule = new TestSchedule(currentUser, refNow, startOfDay, EventID.LatestID);
            SubCalendarEvent subEventA = schedule.getSubCalendarEvent(subEventId);
            SubCalendarEvent subEventB = schedule.getSubCalendarEvent(conflictingSubEventId);
            Assert.IsFalse(subEventB.ActiveSlot.doesTimeLineInterfere(subEventA.ActiveSlot));// This is known to fail
        }

        /// <summary>
        /// This tries to address a smarter way of resolving conflicts. Currently there are conflicts with the subevents '9107379_7_0_9107390', '9105096_7_0_9105097' and '4939920_7_0_9171782_0' after hitting shuffle.
        /// 9107389_7_0_9107390 => 'patch front end repositioning after data refresh'
        /// 4939920_7_0_9171782 => 'BLOCKED OUT' i.e procrsinate all sub event
        /// 9105096_7_0_9105097 => 'remove the need for complete count and delete count to rely on a data member'
        /// The shuffle could be resolved by moving event with the id 1_7_0_2 named 'Tiler mobile UI-create login page' to the next day since it has a later deadline.
        /// The move will leave sufficient time for of the events to get realocated.
        /// </summary>
        [TestMethod]
        public void file_7e65ea64()
        {
            string currentClearAllId = "4939920";
            string subEventId = "9107389_7_0_9107390";
            string conflictingSubEventId = "4939920_7_0_9171782";
            string subEventCId = "9105096_7_0_9105097";
            Location homeLocation = TestUtility.getLocations()[0];
            DateTimeOffset startOfDay = DateTimeOffset.Parse("2:00am");
            DateTimeOffset refNow = DateTimeOffset.Parse("12/30/2017 8:39am");
            UserAccount currentUser = TestUtility.getTestUser(userId: "c1fbe98f-cb7e-4b21-9d99-b4c3acaf670c");
            currentUser.getTilerUser().ClearAllId = currentClearAllId;
            TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.FindMeSomethingToDo(homeLocation).Wait();
            schedule.WriteFullScheduleToLogAndOutlook().Wait();
            schedule = new TestSchedule(currentUser, refNow, startOfDay, EventID.LatestID);
            DayTimeLine firstDay = schedule.Now.firstDay;
            IEnumerable<SubCalendarEvent> firstDaySUbevents = schedule.getAllCalendarEvents().Where(calevent => calevent.isActive).SelectMany(calEvent => calEvent.ActiveSubEvents).Where(subEvent => subEvent.ActiveSlot.doesTimeLineInterfere(firstDay));
            SubCalendarEvent subEventA = schedule.getSubCalendarEvent(subEventId);
            SubCalendarEvent subEventB = schedule.getSubCalendarEvent(conflictingSubEventId);
            SubCalendarEvent subEventC = schedule.getSubCalendarEvent(subEventCId);
            List<SubCalendarEvent> conflictingSubEvents = new List<SubCalendarEvent>() { subEventA, subEventB, subEventC };
            List<BlobSubCalendarEvent> conflictingBlob = Utility.getConflictingEvents(conflictingSubEvents);
            List<BlobSubCalendarEvent> firstDayConflict = Utility.getConflictingEvents(firstDaySUbevents);
            Assert.AreEqual(conflictingBlob.Count, 0);
            Assert.AreEqual(firstDayConflict.Count, 0);

            Assert.IsFalse(subEventB.ActiveSlot.doesTimeLineInterfere(subEventA.ActiveSlot));// This is known to fail
        }
    }
}
