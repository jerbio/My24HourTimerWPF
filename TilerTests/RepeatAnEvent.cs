using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerElements;
using TilerFront;
using System.Linq;
using System.Collections.Generic;

namespace TilerTests
{
    [TestClass]
    public class RepeatAnEvent
    {
        // TODO
        // procrastinateAll, or procrastinate should reset should release the subevent, repeat for all event types
        // What if you have no extra sub events




        [TestMethod]
        public void RepeatAnEventWithSubeventsAfterShouldAutomaticallyAddTheLaterSubeventsAfter ()
        {
            var setupPacket = TestUtility.CreatePacket();
            UserAccount user = setupPacket.Account;
            TilerUser tilerUser = setupPacket.User;
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule schedule = new TestSchedule(user, refNow);
            int hourCount = 2;
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(21);
            TimeLine repetitionRange = new TimeLine(start, end.AddDays(-5).AddHours(-23));
            TimeSpan duration = TimeSpan.FromHours(hourCount);
            int splitCount = 5;
            Repetition repetition = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, repetitionRange.CreateCopy());
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, splitCount, false);
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent testEvent_DB = TestUtility.getCalendarEventById(testEvent.Id, user);

            List<SubCalendarEvent> subEvents = testEvent_DB.ActiveSubEvents.OrderBy(o => o.Start).ToList();
            SubCalendarEvent secondSubevent = subEvents[1];
            SubCalendarEvent thirdSubCalendarEvent = subEvents[2];
            Assert.IsTrue(secondSubevent.End != thirdSubCalendarEvent.Start);
            DateTimeOffset middleOfSubevent =  Utility.MiddleTime(secondSubevent);
            DateTimeOffset secondRefNow = middleOfSubevent;
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id);
            schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            testEvent_DB = TestUtility.getCalendarEventById(testEvent.Id, user);
            List<SubCalendarEvent> subEventsAfterRepetition = testEvent_DB.ActiveSubEvents.OrderBy(o => o.Start).ToList();
            thirdSubCalendarEvent = subEventsAfterRepetition[2];
            Assert.IsTrue(thirdSubCalendarEvent.RepetitionLock);
            Assert.IsTrue(secondSubevent.End == thirdSubCalendarEvent.Start);
            Assert.AreEqual(testEvent_DB.NumberOfSplit, splitCount);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id);
            schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            testEvent_DB = TestUtility.getCalendarEventById(testEvent.Id, user);
            Assert.AreEqual(testEvent_DB.NumberOfSplit, splitCount);
            List <SubCalendarEvent> subEventsAfterRepetitionSecond = testEvent_DB.ActiveSubEvents.OrderBy(o => o.Start).ToList();
            thirdSubCalendarEvent = subEventsAfterRepetition[2];
            SubCalendarEvent fourtSubCalendarEvent = subEventsAfterRepetition[3];
            Assert.IsTrue(thirdSubCalendarEvent.RepetitionLock);
            Assert.IsTrue(fourtSubCalendarEvent.RepetitionLock);
            Assert.IsTrue(secondSubevent.End == thirdSubCalendarEvent.Start);
            Assert.IsTrue(thirdSubCalendarEvent.End == fourtSubCalendarEvent.Start);
        }

        [TestMethod]
        public void RepeatAnEventWithNoExtraSubeventsShouldCreteNewSubevents()
        {
            var setupPacket = TestUtility.CreatePacket();
            UserAccount user = setupPacket.Account;
            TilerUser tilerUser = setupPacket.User;
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule schedule = new TestSchedule(user, refNow);
            int hourCount = 2;
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(21);
            TimeLine repetitionRange = new TimeLine(start, end.AddDays(-5).AddHours(-23));
            TimeSpan duration = TimeSpan.FromHours(hourCount);
            int splitCount = 2;
            Repetition repetition = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, repetitionRange.CreateCopy());
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, splitCount, false);
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent testEvent_DB = TestUtility.getCalendarEventById(testEvent.Id, user);

            List<SubCalendarEvent> subEvents = testEvent_DB.ActiveSubEvents.OrderBy(o => o.Start).ToList();
            SubCalendarEvent secondSubevent = subEvents[1];
            SubCalendarEvent thirdSubCalendarEvent = subEvents[2];
            Assert.IsTrue(secondSubevent.End != thirdSubCalendarEvent.Start);
            DateTimeOffset middleOfSubevent = Utility.MiddleTime(secondSubevent);
            DateTimeOffset secondRefNow = middleOfSubevent;
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id);
            schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            testEvent_DB = TestUtility.getCalendarEventById(testEvent.Id, user);
            List<SubCalendarEvent> subEventsAfterRepetition = testEvent_DB.ActiveSubEvents.OrderBy(o => o.Start).ToList();
            thirdSubCalendarEvent = subEventsAfterRepetition[2];
            Assert.IsTrue(thirdSubCalendarEvent.RepetitionLock);
            Assert.IsTrue(secondSubevent.End == thirdSubCalendarEvent.Start);
            Assert.AreEqual(testEvent_DB.NumberOfSplit, 3);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id);
            schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            testEvent_DB = TestUtility.getCalendarEventById(testEvent.Id, user);
            Assert.AreEqual(testEvent_DB.NumberOfSplit, 4);
            List<SubCalendarEvent> subEventsAfterRepetitionSecond = testEvent_DB.ActiveSubEvents.OrderBy(o => o.Start).ToList();
            thirdSubCalendarEvent = subEventsAfterRepetition[2];
            SubCalendarEvent fourtSubCalendarEvent = subEventsAfterRepetition[3];
            Assert.IsTrue(thirdSubCalendarEvent.RepetitionLock);
            Assert.IsTrue(fourtSubCalendarEvent.RepetitionLock);
            Assert.IsTrue(secondSubevent.End == thirdSubCalendarEvent.Start);
            Assert.IsTrue(thirdSubCalendarEvent.End == fourtSubCalendarEvent.Start);
        }
    }
}
