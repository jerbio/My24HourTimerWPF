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
        static List<TilerUser> createdUsers;

        [ClassInitialize]
        public static void cleanUpTest(TestContext context)
        {
            createdUsers = new List<TilerUser>();
        }

        [TestMethod]
        public void CompleteSubEvent()
        {
            DB_Schedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            createdUsers.Add(tilerUser);
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 2, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            string completedSubEventId = testEvent.AllSubEvents[0].getId;
            SubCalendarEvent initialSubEVent = testEvent.AllSubEvents[0];
            Assert.IsFalse(initialSubEVent.getIsComplete);
            Schedule = new TestSchedule(user, refNow);
            
            Schedule.markSubEventAsComplete(completedSubEventId).Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();
            SubCalendarEvent subEvent = TestUtility.getSubEVentById(completedSubEventId, user);
            Assert.IsTrue(subEvent.getIsComplete);
        }


        [TestMethod]
        public void CompleteCalendarEvent()
        {
            DB_Schedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            createdUsers.Add(tilerUser);
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 2, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            Schedule = new TestSchedule(user, refNow);
            string completedSubEventId = testEvent.AllSubEvents[0].getId;
            Schedule.markAsCompleteCalendarEventAndReadjust(completedSubEventId).Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();
            EventID evenId = new EventID(completedSubEventId);
            
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(evenId.getCalendarEventID(), user);
            Assert.IsTrue(retrievedCalendarEvent.getIsComplete);
        }

        [TestMethod]
        public void CompleteSubEventCount()
        {
            DB_Schedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            createdUsers.Add(tilerUser);
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            int numberOfSubEvent = 5;
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            TimeSpan duration0 = TimeSpan.FromHours(2);
            DateTimeOffset start0 = refNow;
            DateTimeOffset end0 = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser, duration0, new Repetition(), start0, end0, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent0).Wait();

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            TimeSpan duration1 = TimeSpan.FromHours(2);
            DateTimeOffset start1 = refNow;
            DateTimeOffset end1 = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, duration1, new Repetition(), start1, end1, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent1).Wait();

            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Id, user);
            Assert.AreEqual(retrievedCalendarEvent.CompletionCount, 0);
            Assert.AreEqual(retrievedCalendarEvent.ActiveSubEvents.Length, numberOfSubEvent);


            Schedule = new TestSchedule(user, refNow);
            string completedSubEventId = testEvent.AllSubEvents[0].getId;
            Schedule.markSubEventAsComplete(completedSubEventId).Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();
            SubCalendarEvent subEvent = TestUtility.getSubEVentById(completedSubEventId, user);
            Assert.IsTrue(subEvent.getIsComplete);

            EventID evenId = new EventID(completedSubEventId);
            retrievedCalendarEvent = TestUtility.getCalendarEventById(evenId.getCalendarEventID(), user);
            Assert.AreEqual(retrievedCalendarEvent.CompletionCount, 1);

            // Running completion on the same subEvent, we should get the same completion count
            Schedule = new TestSchedule(user, refNow);
            Schedule.markSubEventAsComplete(completedSubEventId).Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();
            subEvent = TestUtility.getSubEVentById(completedSubEventId, user);
            Assert.IsTrue(subEvent.getIsComplete);
            evenId = new EventID(completedSubEventId);
            retrievedCalendarEvent = TestUtility.getCalendarEventById(evenId.getCalendarEventID(), user); 
            Assert.AreEqual(retrievedCalendarEvent.CompletionCount, 1);
        }

        [TestMethod]
        public void CompleteSubEventCountAndReadjust()
        {
            DB_Schedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            createdUsers.Add(tilerUser);
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            int numberOfSubEvent = 5;
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            TimeSpan duration0 = TimeSpan.FromHours(2);
            DateTimeOffset start0 = refNow;
            DateTimeOffset end0 = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser, duration0, new Repetition(), start0, end0, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent0).Wait();

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            TimeSpan duration1 = TimeSpan.FromHours(2);
            DateTimeOffset start1 = refNow;
            DateTimeOffset end1 = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, duration1, new Repetition(), start1, end1, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent1).Wait();

            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Id, user);
            Assert.AreEqual(retrievedCalendarEvent.CompletionCount, 0);
            Assert.AreEqual(retrievedCalendarEvent.ActiveSubEvents.Length, numberOfSubEvent);


            Schedule = new TestSchedule(user, refNow);
            string completedSubEventId = testEvent.AllSubEvents[0].getId;
            Schedule.markSubEventAsCompleteCalendarEventAndReadjust(completedSubEventId);
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();
            SubCalendarEvent subEvent = TestUtility.getSubEVentById(completedSubEventId, user);
            Assert.IsTrue(subEvent.getIsComplete);

            EventID evenId = new EventID(completedSubEventId);
            retrievedCalendarEvent = TestUtility.getCalendarEventById(evenId.getCalendarEventID(), user);
            Assert.AreEqual(retrievedCalendarEvent.CompletionCount, 1);

            // Running completion on the same subEvent, we should get the same completion count
            Schedule = new TestSchedule(user, refNow);
            Schedule.markSubEventAsCompleteCalendarEventAndReadjust(completedSubEventId);
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();
            subEvent = TestUtility.getSubEVentById(completedSubEventId, user);
            Assert.IsTrue(subEvent.getIsComplete);
            evenId = new EventID(completedSubEventId);
            retrievedCalendarEvent = TestUtility.getCalendarEventById(evenId.getCalendarEventID(), user);
            Assert.AreEqual(retrievedCalendarEvent.CompletionCount, 1);
        }


        [TestMethod]
        public void CompleteSubEventMultiple()
        {
            DB_Schedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            createdUsers.Add(tilerUser);
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            int numberOfSubEvent = 5;
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            user.Login().Wait();
            TimeSpan duration0 = TimeSpan.FromHours(2);
            DateTimeOffset start0 = refNow;
            DateTimeOffset end0 = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser, duration0, new Repetition(), start0, end0, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent0).Wait();

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            user.Login().Wait();
            TimeSpan duration1 = TimeSpan.FromHours(2);
            DateTimeOffset start1 = refNow;
            DateTimeOffset end1 = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, duration1, new Repetition(), start1, end1, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent1).Wait();

            SubCalendarEvent testSubEvent = testEvent.ActiveSubEvents[0];
            SubCalendarEvent testSubEvent0 = testEvent0.ActiveSubEvents[0];
            SubCalendarEvent testSubEvent1 = testEvent1.ActiveSubEvents[0];
            testSubEvent = TestUtility.getSubEVentById(testSubEvent.getId, user);
            testSubEvent0 = TestUtility.getSubEVentById(testSubEvent0.getId, user);
            testSubEvent1 = TestUtility.getSubEVentById(testSubEvent1.getId, user);
            Assert.IsFalse(testSubEvent.getIsComplete);
            Assert.IsFalse(testSubEvent0.getIsComplete);
            Assert.IsFalse(testSubEvent1.getIsComplete);




            Schedule = new TestSchedule(user, refNow);
            Schedule.markAsCompleteCalendarEventAndReadjust(testSubEvent.getId).Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();
            testSubEvent = TestUtility.getSubEVentById(testSubEvent.getId, user);


            List<EventID> subEventIds = new List<EventID>() { testSubEvent.SubEvent_ID, testSubEvent0.SubEvent_ID, testSubEvent1.SubEvent_ID };
            Schedule = new TestSchedule(user, refNow);
            Schedule.markSubEventsAsComplete(subEventIds.Select(subeventid => subeventid.ToString())).Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();


            Schedule = new TestSchedule(user, refNow);
            testSubEvent = TestUtility.getSubEVentById(testSubEvent.getId, user);
            testSubEvent0 = TestUtility.getSubEVentById(testSubEvent0.getId, user);
            testSubEvent1 = TestUtility.getSubEVentById(testSubEvent1.getId, user);
            Assert.IsTrue(testSubEvent.getIsComplete);
            Assert.IsTrue(testSubEvent0.getIsComplete);
            Assert.IsTrue(testSubEvent1.getIsComplete);

            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            CalendarEvent retrievedCalendarEvent0 = TestUtility.getCalendarEventById(testEvent0.getId, user);
            CalendarEvent retrievedCalendarEvent1 = TestUtility.getCalendarEventById(testEvent1.getId, user);
            Assert.AreEqual(retrievedCalendarEvent.CompletionCount, 1);
            Assert.AreEqual(retrievedCalendarEvent0.CompletionCount, 1);
            Assert.AreEqual(retrievedCalendarEvent1.CompletionCount, 1);



            /// Re running just to prevent duplicate additions
            Schedule = new TestSchedule(user, refNow);
            Schedule.markSubEventsAsComplete(subEventIds.Select(subeventid => subeventid.ToString())).Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();

            Schedule = new TestSchedule(user, refNow);
            testSubEvent = TestUtility.getSubEVentById(testSubEvent.getId, user);
            testSubEvent0 = TestUtility.getSubEVentById(testSubEvent0.getId, user);
            testSubEvent1 = TestUtility.getSubEVentById(testSubEvent1.getId, user);
            Assert.IsTrue(testSubEvent.getIsComplete);
            Assert.IsTrue(testSubEvent0.getIsComplete);
            Assert.IsTrue(testSubEvent1.getIsComplete);

            retrievedCalendarEvent = TestUtility.getCalendarEventById(testSubEvent.SubEvent_ID.getCalendarEventID(), user);
            retrievedCalendarEvent0 = TestUtility.getCalendarEventById(testSubEvent0.SubEvent_ID.getCalendarEventID(), user);
            retrievedCalendarEvent1 = TestUtility.getCalendarEventById(testSubEvent1.SubEvent_ID.getCalendarEventID(), user);
            Assert.AreEqual(retrievedCalendarEvent.CompletionCount, 1);
            Assert.AreEqual(retrievedCalendarEvent0.CompletionCount, 1);
            Assert.AreEqual(retrievedCalendarEvent1.CompletionCount, 1);


        }
    }
}
