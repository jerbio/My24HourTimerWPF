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
    public class CompletionTest
    {
        [ClassInitialize]
        public static void cleanUpTest(TestContext context)
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            Schedule Schedule = new TestSchedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }

        [ClassCleanup]
        public static void cleanUpTest()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            Schedule Schedule = new TestSchedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }

        [TestInitialize]
        public void cleanUpLog()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            Schedule Schedule = new TestSchedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
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

        [TestMethod]
        public void CompleteSubEvent()
        {
            DB_Schedule Schedule;
            UserAccount user = TestUtility.getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 2, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            Schedule = new TestSchedule(user, refNow);
            string completedSubEventId = testEvent.AllSubEvents[0].getId;
            Schedule.markSubEventAsComplete(completedSubEventId).Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();
            Schedule = new TestSchedule(user, refNow);
            SubCalendarEvent subEvent = Schedule.getSubCalendarEvent(completedSubEventId);
            Assert.IsTrue(subEvent.getIsComplete);
        }


        [TestMethod]
        public void CompleteCalendarEvent()
        {
            DB_Schedule Schedule;
            UserAccount user = TestUtility.getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 2, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            Schedule = new TestSchedule(user, refNow);
            string completedSubEventId = testEvent.AllSubEvents[0].getId;
            Schedule.markAsCompleteCalendarEventAndReadjust(completedSubEventId).Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent retrievedCalendarEvent = Schedule.getCalendarEvent(completedSubEventId);
            Assert.IsTrue(retrievedCalendarEvent.getIsComplete);
        }

        [TestMethod]
        public void CompleteSubEventCount()
        {
            DB_Schedule Schedule;
            UserAccount user = TestUtility.getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            int numberOfSubEvent = 5;
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            user = TestUtility.getTestUser();
            user.Login().Wait();
            TimeSpan duration0 = TimeSpan.FromHours(2);
            DateTimeOffset start0 = refNow;
            DateTimeOffset end0 = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(duration0, new Repetition(), start0, end0, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent0).Wait();

            user = TestUtility.getTestUser();
            user.Login().Wait();
            TimeSpan duration1 = TimeSpan.FromHours(2);
            DateTimeOffset start1 = refNow;
            DateTimeOffset end1 = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(duration1, new Repetition(), start1, end1, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent1).Wait();
            Schedule = new TestSchedule(user, refNow);
            string completedSubEventId = testEvent.AllSubEvents[0].getId;
            Schedule.markSubEventAsComplete(completedSubEventId).Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();
            Schedule = new TestSchedule(user, refNow);
            SubCalendarEvent subEvent = Schedule.getSubCalendarEvent(completedSubEventId);
            Assert.IsTrue(subEvent.getIsComplete);
            CalendarEvent calendarEvent = Schedule.getCalendarEvent(completedSubEventId);
            Assert.AreEqual(calendarEvent.CompletionCount, 1);

            // Running completion on the same subEvent, we should get the same completion count
            Schedule = new TestSchedule(user, refNow);
            Schedule.markSubEventAsComplete(completedSubEventId).Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();
            Schedule = new TestSchedule(user, refNow);
            subEvent = Schedule.getSubCalendarEvent(completedSubEventId);
            Assert.IsTrue(subEvent.getIsComplete);
            calendarEvent = Schedule.getCalendarEvent(completedSubEventId);
            Assert.AreEqual(calendarEvent.CompletionCount, 1);

        }


        [TestMethod]
        public void CompleteSubEventMultiple()
        {
            DB_Schedule Schedule;
            UserAccount user = TestUtility.getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            int numberOfSubEvent = 5;
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            user = TestUtility.getTestUser();
            user.Login().Wait();
            TimeSpan duration0 = TimeSpan.FromHours(2);
            DateTimeOffset start0 = refNow;
            DateTimeOffset end0 = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(duration0, new Repetition(), start0, end0, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent0).Wait();

            user = TestUtility.getTestUser();
            user.Login().Wait();
            TimeSpan duration1 = TimeSpan.FromHours(2);
            DateTimeOffset start1 = refNow;
            DateTimeOffset end1 = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(duration1, new Repetition(), start1, end1, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent1).Wait();
            Schedule = new TestSchedule(user, refNow);
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();
            string completedSubEventId = testEvent.AllSubEvents[0].getId;
            Schedule.markSubEventAsComplete(completedSubEventId).Wait();
            Schedule = new TestSchedule(user, refNow);
            SubCalendarEvent testSubEvent = testEvent.ActiveSubEvents[0];
            SubCalendarEvent testSubEvent0 = testEvent0.ActiveSubEvents[0];
            SubCalendarEvent testSubEvent1 = testEvent1.ActiveSubEvents[0];
            Schedule = new TestSchedule(user, refNow);
            Schedule.markAsCompleteCalendarEventAndReadjust(testSubEvent.getId).Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();
            testSubEvent = Schedule.getSubCalendarEvent(testSubEvent.getId);
            List<EventID> subEventIds = new List<EventID>() { testSubEvent.SubEvent_ID, testSubEvent0.SubEvent_ID, testSubEvent1.SubEvent_ID };
            Schedule = new TestSchedule(user, refNow);
            Schedule.markSubEventsAsComplete(subEventIds.Select(subeventid=> subeventid.ToString())).Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();

            Schedule = new TestSchedule(user, refNow);
            testSubEvent = Schedule.getSubCalendarEvent(testSubEvent.getId);
            testSubEvent0 = Schedule.getSubCalendarEvent(testSubEvent0.getId);
            testSubEvent1 = Schedule.getSubCalendarEvent(testSubEvent1.getId);
            Assert.IsTrue(testSubEvent.getIsComplete);
            Assert.IsTrue(testSubEvent0.getIsComplete);
            Assert.IsTrue(testSubEvent1.getIsComplete);

            CalendarEvent retrievedCalendarEvent = Schedule.getCalendarEvent(testSubEvent.getId);
            CalendarEvent retrievedCalendarEvent0 = Schedule.getCalendarEvent(testSubEvent0.getId);
            CalendarEvent retrievedCalendarEvent1 = Schedule.getCalendarEvent(testSubEvent1.getId);
            Assert.AreEqual(retrievedCalendarEvent.CompletionCount, 1);
            Assert.AreEqual(retrievedCalendarEvent0.CompletionCount, 1);
            Assert.AreEqual(retrievedCalendarEvent1.CompletionCount, 1);



            /// Re running just to prevent duplicate additions
            Schedule = new TestSchedule(user, refNow);
            Schedule.markSubEventsAsComplete(subEventIds.Select(subeventid => subeventid.ToString())).Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();

            Schedule = new TestSchedule(user, refNow);
            testSubEvent = Schedule.getSubCalendarEvent(testSubEvent.getId);
            testSubEvent0 = Schedule.getSubCalendarEvent(testSubEvent0.getId);
            testSubEvent1 = Schedule.getSubCalendarEvent(testSubEvent1.getId);
            Assert.IsTrue(testSubEvent.getIsComplete);
            Assert.IsTrue(testSubEvent0.getIsComplete);
            Assert.IsTrue(testSubEvent1.getIsComplete);

            retrievedCalendarEvent = Schedule.getCalendarEvent(testSubEvent.getId);
            retrievedCalendarEvent0 = Schedule.getCalendarEvent(testSubEvent0.getId);
            retrievedCalendarEvent1 = Schedule.getCalendarEvent(testSubEvent1.getId);
            Assert.AreEqual(retrievedCalendarEvent.CompletionCount, 1);
            Assert.AreEqual(retrievedCalendarEvent0.CompletionCount, 1);
            Assert.AreEqual(retrievedCalendarEvent1.CompletionCount, 1);


        }
    }
}
