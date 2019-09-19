using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using My24HourTimerWPF;
using TilerElements;
using System.Collections.Generic;
using System.Linq;
using TilerCore;
using System.Threading.Tasks;

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
            Schedule.WriteFullScheduleToLog().Wait();
            SubCalendarEvent subEvent = TestUtility.getSubEventById(completedSubEventId, user);
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
            int beforeCompletionCount = Schedule.getAllCalendarEvents().Count();
            string completedSubEventId = testEvent.AllSubEvents[0].getId;
            Schedule.markAsCompleteCalendarEventAndReadjust(completedSubEventId).Wait();
            Schedule.WriteFullScheduleToLog().Wait();
            EventID evenId = new EventID(completedSubEventId);
            
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(evenId.getCalendarEventID(), user);
            Assert.IsTrue(retrievedCalendarEvent.getIsComplete);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            int afterCompletionCount = Schedule.getAllCalendarEvents().Count();
            Assert.AreEqual(beforeCompletionCount, afterCompletionCount + 1);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestUtility.isSubCalendarEventUIEquivalenToScheduleLoaded(user, Schedule.Now);


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TimeLine RepetitionTImeLine = new TimeLine(start, start.AddDays(21));
            TimeLine RepetitionActualTImeLine = new TimeLine(start, end);
            Repetition repetition = new Repetition(RepetitionTImeLine, Repetition.Frequency.DAILY, RepetitionActualTImeLine);
            CalendarEvent repeatTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 2, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(repeatTestEvent).Wait();
            int beforeRepeatCompletionCount = Schedule.getAllCalendarEvents().Count();

            string completedRepeatSubEventId = repeatTestEvent.AllSubEvents[0].getId;

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            Schedule.markAsCompleteCalendarEventAndReadjust(repeatTestEvent.Id).Wait();
            Schedule.WriteFullScheduleToLog().Wait();



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            int afterRepeatCompletionCount = Schedule.getAllCalendarEvents().Count();
            Assert.AreEqual(beforeRepeatCompletionCount, afterRepeatCompletionCount + 1);
            EventID repeatEventId = new EventID(completedRepeatSubEventId);
            CalendarEvent retrievedRepeatCalendarEvent = TestUtility.getCalendarEventById(repeatEventId.getCalendarEventID(), user);
            Assert.IsTrue(retrievedRepeatCalendarEvent.getIsComplete);
            TestUtility.isSubCalendarEventUIEquivalenToScheduleLoaded(user, Schedule.Now);






            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TimeLine RigidRepetitionTImeLine = new TimeLine(start, start.AddDays(21));
            TimeLine RigidRepetitionActualTImeLine = new TimeLine(start, start.AddHours(4));
            Repetition RigidRepetition = new Repetition(RigidRepetitionTImeLine, Repetition.Frequency.DAILY, RigidRepetitionActualTImeLine);
            CalendarEvent RigidRepeatTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, RigidRepetition, start, RigidRepetitionActualTImeLine.End, 1, true);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(RigidRepeatTestEvent).Wait();
            int beforeRigidRepeatCompletionCount = Schedule.getAllCalendarEvents().Count();

            string completedRigidRepeatSubEventId = RigidRepeatTestEvent.AllSubEvents[0].getId;

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            Schedule.markAsCompleteCalendarEventAndReadjust(RigidRepeatTestEvent.Id).Wait();
            Schedule.WriteFullScheduleToLog().Wait();



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            int afterRigidRepeatCompletionCount = Schedule.getAllCalendarEvents().Count();
            Assert.AreEqual(beforeRigidRepeatCompletionCount, afterRigidRepeatCompletionCount + 1);
            EventID RigidRepeatEventId = new EventID(completedRigidRepeatSubEventId);
            CalendarEvent retrievedRigidRepeatCalendarEvent = TestUtility.getCalendarEventById(RigidRepeatEventId.getCalendarEventID(), user);
            Assert.IsTrue(retrievedRigidRepeatCalendarEvent.getIsComplete);
            TestUtility.isSubCalendarEventUIEquivalenToScheduleLoaded(user, Schedule.Now);
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
            Schedule.markSubEventAsCompleteCalendarEventAndReadjust(completedSubEventId);
            Schedule.WriteFullScheduleToLog().Wait();
            SubCalendarEvent subEvent = TestUtility.getSubEventById(completedSubEventId, user);
            Assert.IsTrue(subEvent.getIsComplete);

            EventID evenId = new EventID(completedSubEventId);
            user = TestUtility.getTestUser(userId: tilerUser.Id, reloadTilerContext: true);
            retrievedCalendarEvent = TestUtility.getCalendarEventById(evenId.getCalendarEventID(), user);
            Assert.AreEqual(retrievedCalendarEvent.CompletionCount, 1);

            // Running completion on the same subEvent, we should get the same completion count
            Schedule = new TestSchedule(user, refNow);
            Schedule.markSubEventAsComplete(completedSubEventId).Wait();
            Schedule.WriteFullScheduleToLog().Wait();
            subEvent = TestUtility.getSubEventById(completedSubEventId, user);
            Assert.IsTrue(subEvent.getIsComplete);
            evenId = new EventID(completedSubEventId);
            user = TestUtility.getTestUser(userId: tilerUser.Id, reloadTilerContext: true);
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
            Schedule.WriteFullScheduleToLog().Wait();
            SubCalendarEvent subEvent = TestUtility.getSubEventById(completedSubEventId, user);
            Assert.IsTrue(subEvent.getIsComplete);

            EventID evenId = new EventID(completedSubEventId);
            retrievedCalendarEvent = TestUtility.getCalendarEventById(evenId.getCalendarEventID(), user);
            Assert.AreEqual(retrievedCalendarEvent.CompletionCount, 1);

            // Running completion on the same subEvent, we should get the same completion count
            Schedule = new TestSchedule(user, refNow);
            Schedule.markSubEventAsCompleteCalendarEventAndReadjust(completedSubEventId);
            Schedule.WriteFullScheduleToLog().Wait();
            subEvent = TestUtility.getSubEventById(completedSubEventId, user);
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
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            user.Login().Wait();
            tilerUser = user.getTilerUser();
            TimeSpan duration0 = TimeSpan.FromHours(2);
            DateTimeOffset start0 = refNow;
            DateTimeOffset end0 = refNow.AddHours(7);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser, duration0, new Repetition(), start0, end0, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent0).Wait();


            user = TestUtility.getTestUser(userId: tilerUser.Id);
            user.Login().Wait();
            tilerUser = user.getTilerUser();
            TimeSpan duration1 = TimeSpan.FromHours(2);
            DateTimeOffset start1 = refNow;
            DateTimeOffset end1 = refNow.AddHours(7);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, duration1, new Repetition(), start1, end1, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent1).Wait();

            SubCalendarEvent testSubEvent = testEvent.ActiveSubEvents[0];
            SubCalendarEvent testSubEvent0 = testEvent0.ActiveSubEvents[0];
            SubCalendarEvent testSubEvent1 = testEvent1.ActiveSubEvents[0];
            testSubEvent = TestUtility.getSubEventById(testSubEvent.getId, user);
            testSubEvent0 = TestUtility.getSubEventById(testSubEvent0.getId, user);
            testSubEvent1 = TestUtility.getSubEventById(testSubEvent1.getId, user);
            Assert.IsFalse(testSubEvent.getIsComplete);
            Assert.IsFalse(testSubEvent0.getIsComplete);
            Assert.IsFalse(testSubEvent1.getIsComplete);



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            Schedule.markSubEventAsComplete(testSubEvent.getId).Wait();
            Schedule.WriteFullScheduleToLog().Wait();
            testSubEvent = TestUtility.getSubEventById(testSubEvent.getId, user);


            List<EventID> subEventIds = new List<EventID>() { testSubEvent.SubEvent_ID, testSubEvent0.SubEvent_ID, testSubEvent1.SubEvent_ID };
            TestUtility.reloadTilerUser(ref user, ref tilerUser);

            Schedule = new TestSchedule(user, refNow);
            Schedule.markSubEventsAsComplete(subEventIds.Select(subeventid => subeventid.ToString())).Wait();
            Schedule.WriteFullScheduleToLog().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            testSubEvent = TestUtility.getSubEventById(testSubEvent.getId, user);
            testSubEvent0 = TestUtility.getSubEventById(testSubEvent0.getId, user);
            testSubEvent1 = TestUtility.getSubEventById(testSubEvent1.getId, user);
            Assert.IsTrue(testSubEvent.getIsComplete);
            Assert.IsTrue(testSubEvent0.getIsComplete);
            Assert.IsTrue(testSubEvent1.getIsComplete);

            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            CalendarEvent retrievedCalendarEvent0 = TestUtility.getCalendarEventById(testEvent0.getId, user);
            CalendarEvent retrievedCalendarEvent1 = TestUtility.getCalendarEventById(testEvent1.getId, user);
            Assert.AreEqual(retrievedCalendarEvent.CompletionCount, 1);
            Assert.AreEqual(retrievedCalendarEvent0.CompletionCount, 1);
            Assert.AreEqual(retrievedCalendarEvent1.CompletionCount, 1);

            Schedule = new TestSchedule(user, refNow);
            Schedule.markSubEventsAsComplete(subEventIds.Select(subeventid => subeventid.ToString())).Wait();
            Schedule.WriteFullScheduleToLog().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            testSubEvent = TestUtility.getSubEventById(testSubEvent.getId, user);
            testSubEvent0 = TestUtility.getSubEventById(testSubEvent0.getId, user);
            testSubEvent1 = TestUtility.getSubEventById(testSubEvent1.getId, user);
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


        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void testOutOfRangeOfScheduleButCompletionSubEvent()
        {
            DB_Schedule Schedule;
            DateTimeOffset refNow = TestUtility.parseAsUTC("7/7/2019 12:00:00 AM");
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, 1, 0, 0, 0, new TimeSpan());
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddDays(28);

            TimeLine repeatTimeLine = new TimeLine(start, end.AddDays(200));
            TimeLine calTimeLine = repeatTimeLine.CreateCopy();
            Repetition repetition = new Repetition(repeatTimeLine, Repetition.Frequency.WEEKLY, calTimeLine);
            int repeatSplitCount = 2;
            CalendarEvent repeatEvent = TestUtility
                .generateCalendarEvent(tilerUser, duration, repetition, calTimeLine.Start, calTimeLine.End, repeatSplitCount, false);
            int initialCount = repeatEvent.AllSubEvents.Count();
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(repeatEvent).Wait();
            const int dayDelta = 7;
            TimeLine lookupWindow = new TimeLine(refNow.AddDays(-dayDelta), refNow.AddDays(dayDelta * 2));


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, rangeOfLookup: lookupWindow);
            Location location = TestUtility.getLocations()[0];
            Schedule.FindMeSomethingToDo(location).Wait();
            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, rangeOfLookup: lookupWindow);
            SubCalendarEvent subEvent = repeatEvent.ActiveSubEvents.First();
            Schedule.markSubEventAsComplete(subEvent.getId).Wait();
            Schedule.persistToDB().Wait();

            Task<CalendarEvent> waitRetrievedEvent = user.ScheduleLogControl.getCalendarEventWithID(subEvent.SubEvent_ID.getRepeatCalendarEventID());
            waitRetrievedEvent.Wait();
            CalendarEvent retrievedEvent = waitRetrievedEvent.Result;
            Assert.AreEqual(retrievedEvent.CompletionCount, 1);


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, rangeOfLookup: lookupWindow);
            CalendarEvent calEvent = Schedule.getCalendarEvent(subEvent.SubEvent_ID.getRepeatCalendarEventID());
            Assert.AreEqual(calEvent.CompletionCount, 1);


            Assert.IsTrue(calEvent.ActiveSubEvents.Count() == 1);
            

            UserAccount userAcc = TestUtility.getTestUser(userId: tilerUser.Id);
            Task<CalendarEvent> waitVar = userAcc.ScheduleLogControl.getCalendarEventWithID(repeatEvent.Id);
            waitVar.Wait();
            CalendarEvent verificationEventPulled = waitVar.Result;
            Assert.IsTrue(verificationEventPulled.ActiveSubEvents.Count() == (initialCount - 1));


            lookupWindow = new TimeLine(refNow, repeatTimeLine.End);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, retrievalOption: DataRetrivalOption.All, rangeOfLookup: lookupWindow);
            CalendarEvent repeatFromSchedule = Schedule.getCalendarEvent(repeatEvent.Id);
            Assert.IsTrue(repeatFromSchedule.isTestEquivalent(verificationEventPulled));
        }
    }
}
