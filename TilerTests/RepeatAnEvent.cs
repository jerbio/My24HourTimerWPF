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
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, 1, 0, 0, 0, new TimeSpan());
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

            Repetition repetition0 = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, repetitionRange.CreateCopy());
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser, duration, repetition0, start, end, splitCount, false);
            schedule = new TestSchedule(user, refNow);
            schedule.AddToScheduleAndCommitAsync(testEvent0).Wait();


            SubCalendarEvent firstSubEvent = testEvent.OrderByStartActiveSubEvents.First();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            schedule.FindMeSomethingToDo(new Location()).Wait();
            schedule.persistToDB().Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);


            List<SubCalendarEvent> subEvents = repetitionInstance_DB.OrderByStartActiveSubEvents.ToList();
            SubCalendarEvent secondSubevent = subEvents[1];
            SubCalendarEvent thirdSubCalendarEvent = subEvents[2];
            Assert.IsTrue(secondSubevent.End != thirdSubCalendarEvent.Start);
            DateTimeOffset middleOfSubevent =  Utility.MiddleTime(secondSubevent);
            DateTimeOffset secondRefNow = middleOfSubevent;
            schedule = new TestSchedule(user, secondRefNow);
            Location location = TestUtility.getLocations()[0];
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            List<SubCalendarEvent> subEventsAfterRepetition = repetitionInstance_DB.ActiveSubEvents.OrderBy(o => o.Start).ToList();
            thirdSubCalendarEvent = subEventsAfterRepetition[2];
            Assert.IsTrue(thirdSubCalendarEvent.isRepetitionLocked);
            Assert.IsTrue(secondSubevent.End == thirdSubCalendarEvent.Start);
            Assert.AreEqual(repetitionInstance_DB.NumberOfSplit, splitCount);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            Assert.AreEqual(repetitionInstance_DB.NumberOfSplit, splitCount);
            List <SubCalendarEvent> subEventsAfterRepetitionSecond = repetitionInstance_DB.ActiveSubEvents.OrderBy(o => o.Start).ToList();
            thirdSubCalendarEvent = subEventsAfterRepetitionSecond[2];
            SubCalendarEvent fourtSubCalendarEvent = subEventsAfterRepetitionSecond[3];
            Assert.IsTrue(thirdSubCalendarEvent.isRepetitionLocked);
            Assert.IsTrue(fourtSubCalendarEvent.isRepetitionLocked);
            Assert.IsTrue(secondSubevent.End == thirdSubCalendarEvent.Start);
            Assert.IsTrue(thirdSubCalendarEvent.End == fourtSubCalendarEvent.Start);
        }

        [TestMethod]
        public void RepeatAnEventWithNoExtraSubeventsShouldCreateNewSubevents()
        {
            var setupPacket = TestUtility.CreatePacket();
            UserAccount user = setupPacket.Account;
            TilerUser tilerUser = setupPacket.User;
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, 1, 0, 0, 0, new TimeSpan());
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

            List<SubCalendarEvent> subEvents = testEvent_DB.OrderByStartActiveSubEvents.ToList();
            SubCalendarEvent firstSubEvent = subEvents[0];
            schedule = new TestSchedule(user, refNow);
            schedule.markSubEventAsComplete(firstSubEvent.Id).Wait();
            schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent repeatCalEventInstance = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            subEvents = repeatCalEventInstance.OrderByStartActiveSubEvents.ToList();
            SubCalendarEvent secondSubevent = subEvents[0];
            DateTimeOffset middleOfSubevent = Utility.MiddleTime(secondSubevent);
            DateTimeOffset secondRefNow = middleOfSubevent;
            Location location = TestUtility.getLocations()[0];
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            string repeatId = secondSubevent.getTilerID.getIDUpToRepeatCalendarEvent();
            repeatCalEventInstance = TestUtility.getCalendarEventById(repeatId, user);
            testEvent_DB = TestUtility.getCalendarEventById(repeatId, user);
            List<SubCalendarEvent> subEventsAfterRepetition = repeatCalEventInstance.ActiveSubEvents.OrderBy(o => o.Start).ToList();
            SubCalendarEvent thirdSubCalendarEvent = subEventsAfterRepetition[1];
            Assert.IsTrue(thirdSubCalendarEvent.isRepetitionLocked);
            Assert.IsTrue(secondSubevent.End == thirdSubCalendarEvent.Start);
            int firstSplitCountUpdate = splitCount + 1;
            Assert.AreEqual(repeatCalEventInstance.NumberOfSplit, firstSplitCountUpdate);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            string secondRepeatId = secondSubevent.getTilerID.getIDUpToRepeatCalendarEvent();
            CalendarEvent secondRepeatCalEventInstance = TestUtility.getCalendarEventById(secondRepeatId, user);
            int secondSplitCountUpdate = splitCount + 2;
            Assert.AreEqual(secondRepeatCalEventInstance.NumberOfSplit, secondSplitCountUpdate);
            List<SubCalendarEvent> subEventsAfterRepetitionSecond = secondRepeatCalEventInstance.OrderByStartActiveSubEvents.ToList();
            thirdSubCalendarEvent = subEventsAfterRepetitionSecond[1];
            SubCalendarEvent fourtSubCalendarEvent = subEventsAfterRepetitionSecond[2];
            Assert.IsTrue(thirdSubCalendarEvent.isRepetitionLocked);
            Assert.IsTrue(fourtSubCalendarEvent.isRepetitionLocked);
            Assert.IsTrue(secondSubevent.End == thirdSubCalendarEvent.Start);
            Assert.IsTrue(thirdSubCalendarEvent.End == fourtSubCalendarEvent.Start);
        }

        /// <summary>
        /// Procrastinateall should push sub events and still maintain repetition lock
        /// </summary>
        [TestMethod]
        public void procrastinateAllResetsAllrepetitionLock()
        {
            #region init
            var setupPacket = TestUtility.CreatePacket();
            UserAccount user = setupPacket.Account;
            TilerUser tilerUser = setupPacket.User;
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            int hourCount = 8;
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(21);
            TimeLine repetitionRange = new TimeLine(start, end.AddDays(-5).AddHours(-23));
            TimeSpan duration = TimeSpan.FromHours(hourCount);
            int splitCount = 5;
            #endregion
            #region addFirstEvent
            Repetition repetition0 = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, repetitionRange.CreateCopy());
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser, duration, repetition0, start, end, splitCount, false);
            TestSchedule schedule = new TestSchedule(user, refNow);
            schedule.AddToScheduleAndCommitAsync(testEvent0).Wait();
            #endregion
            #region addSecondEvent
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Repetition repetition1 = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, repetitionRange.CreateCopy());
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, duration, repetition1, start, end, splitCount, false);
            schedule = new TestSchedule(user, refNow);
            schedule.AddToScheduleAndCommitAsync(testEvent1).Wait();
            #endregion
            #region pick middle of second subevent in order of start and hit repeat
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            schedule.FindMeSomethingToDo(new Location()).Wait();
            schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            List<SubCalendarEvent> orderedByStart = testEvent0.OrderByStartActiveSubEvents.ToList();
            SubCalendarEvent firstSubEvent = orderedByStart.First();
            CalendarEvent repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            List<SubCalendarEvent> subEvents = repetitionInstance_DB.OrderByStartActiveSubEvents.ToList();
            SubCalendarEvent secondSubevent = subEvents[1];
            SubCalendarEvent thirdSubCalendarEvent = subEvents[2];
            Assert.IsTrue(secondSubevent.End != thirdSubCalendarEvent.Start);
            DateTimeOffset middleOfSubevent = Utility.MiddleTime(secondSubevent);
            DateTimeOffset secondRefNow = middleOfSubevent;
            Location location = TestUtility.getLocations()[0];
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();
            #endregion
            #region second repeat press on second subevent(by time) and verififcation
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            List<SubCalendarEvent> subEventsAfterRepetition = repetitionInstance_DB.OrderByStartActiveSubEvents.ToList();
            thirdSubCalendarEvent = subEventsAfterRepetition[2];
            Assert.IsTrue(thirdSubCalendarEvent.isRepetitionLocked);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();
            #endregion


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            List<SubCalendarEvent> subEventsAfterRepetitionSecond = repetitionInstance_DB.ActiveSubEvents.Where(subEvent => subEvent.End >= secondRefNow).OrderBy(o => o.Start).ToList();
            thirdSubCalendarEvent = subEventsAfterRepetition[2];
            SubCalendarEvent fourtSubCalendarEvent = subEventsAfterRepetition[3];
            Assert.IsTrue(thirdSubCalendarEvent.isRepetitionLocked);
            Assert.IsFalse(fourtSubCalendarEvent.isRepetitionLocked);
            

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent beforeRepetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            schedule = new TestSchedule(user, secondRefNow);
            TimeSpan threeHourSpan = TimeSpan.FromHours(3);
            var procrartinateResult = schedule.ProcrastinateAll(threeHourSpan);
            DateTimeOffset procrastinationStart = schedule.Now.constNow.Add(threeHourSpan);
            Assert.IsNull(procrartinateResult.Item1);
            schedule.persistToDB().Wait();


            List<SubCalendarEvent> subEventsAfterProcrastinateAll = beforeRepetitionInstance_DB.OrderByStartActiveSubEvents.Where(subEvent => subEvent.End >= procrastinationStart).ToList();

            SubCalendarEvent firstSubEventAfterProcrastinate = subEventsAfterProcrastinateAll[0];
            SubCalendarEvent secondSubEventAfterProcrastinate = subEventsAfterProcrastinateAll[1];
            SubCalendarEvent thirdSubEventAfterProcrastinate = subEventsAfterProcrastinateAll[2];
            Assert.IsTrue(firstSubEventAfterProcrastinate.isRepetitionLocked);// activating repeat subevent should still be locked
            Assert.IsTrue(secondSubEventAfterProcrastinate.isRepetitionLocked); // subsequent repeat event should still be locked
            Assert.IsTrue(thirdSubEventAfterProcrastinate.isRepetitionLocked); // Third subevent is repetition locked because of double repeat press
            Assert.IsTrue(firstSubEventAfterProcrastinate.Start == procrastinationStart);

        }

        /// <summary>
        /// Procrastinate just an event should reset repetion locks
        /// </summary>
        [TestMethod]
        public void procrastinateJustAnEventShouldResetRepetitionLocks()
        {
            #region init
            var setupPacket = TestUtility.CreatePacket();
            UserAccount user = setupPacket.Account;
            TilerUser tilerUser = setupPacket.User;
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, 1, 0, 0, 0, new TimeSpan());
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            int hourCount = 8;
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(21);
            TimeLine repetitionRange = new TimeLine(start, end.AddDays(-5).AddHours(-23));
            TimeSpan duration = TimeSpan.FromHours(hourCount);
            int splitCount = 5;
            #endregion
            #region addFirstEvent
            Repetition repetition0 = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, repetitionRange.CreateCopy());
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser, duration, repetition0, start, end, splitCount, false);
            TestSchedule schedule = new TestSchedule(user, refNow);
            schedule.AddToScheduleAndCommitAsync(testEvent0).Wait();
            #endregion
            #region addSecondEvent
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Repetition repetition1 = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, repetitionRange.CreateCopy());
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, duration, repetition1, start, end, splitCount, false);
            schedule = new TestSchedule(user, refNow);
            schedule.AddToScheduleAndCommitAsync(testEvent1).Wait();
            #endregion
            #region pick middle of second subevent in order of start and hit repeat
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            schedule.FindMeSomethingToDo(new Location()).Wait();
            schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            List<SubCalendarEvent> orderedByStart = testEvent0.OrderByStartActiveSubEvents.ToList();
            SubCalendarEvent firstSubEvent = orderedByStart.First();
            CalendarEvent repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            List<SubCalendarEvent> subEvents = repetitionInstance_DB.OrderByStartActiveSubEvents.ToList();
            SubCalendarEvent secondSubevent = subEvents[1];
            SubCalendarEvent thirdSubCalendarEvent = subEvents[2];
            Assert.IsTrue(secondSubevent.End != thirdSubCalendarEvent.Start);
            DateTimeOffset middleOfSubevent = Utility.MiddleTime(secondSubevent);
            DateTimeOffset secondRefNow = middleOfSubevent;
            Location location = TestUtility.getLocations()[0];
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();
            #endregion
            #region second repeat press on second subevent(by time) and verififcation
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            List<SubCalendarEvent> subEventsAfterRepetition = repetitionInstance_DB.OrderByStartActiveSubEvents.ToList();
            thirdSubCalendarEvent = subEventsAfterRepetition[2];
            Assert.IsTrue(thirdSubCalendarEvent.isRepetitionLocked);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();
            #endregion


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);

            DateTimeOffset thirdRefNow = secondRefNow.AddMinutes(1);
            List<SubCalendarEvent> subEventsAfterSecondRepetition = repetitionInstance_DB.OrderByStartActiveSubEvents.Where(obj => obj.End >= thirdRefNow).ToList();
            SubCalendarEvent subEvent = subEventsAfterSecondRepetition.First();
            schedule = new TestSchedule(user, thirdRefNow);
            var procrastinateResult = schedule.ProcrastinateJustAnEvent(subEvent.Id, TimeSpan.FromHours(2));
            Assert.IsNull(procrastinateResult.Item1);

            Assert.IsTrue(subEventsAfterSecondRepetition.Count == 4);// there should be only four events rescheduled since the first event happened earlier
            foreach (SubCalendarEvent eachSubEvent in subEventsAfterSecondRepetition)
            {
                Assert.IsFalse(eachSubEvent.isRepetitionLocked);
            }

        }

        /// <summary>
        /// Shuffle should reset all repetition locks after now
        /// </summary>
        [TestMethod]
        public void shuffleOfTheScheduleShouldRemoveAllRepeatEventsAfterNow()
        {
            #region init
            var setupPacket = TestUtility.CreatePacket();
            UserAccount user = setupPacket.Account;
            TilerUser tilerUser = setupPacket.User;
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, 1, 0, 0, 0, new TimeSpan());
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            int hourCount = 8;
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(21);
            TimeLine repetitionRange = new TimeLine(start, end.AddDays(-5).AddHours(-23));
            TimeSpan duration = TimeSpan.FromHours(hourCount);
            int splitCount = 5;
            #endregion
            #region addFirstEvent
            Repetition repetition0 = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, repetitionRange.CreateCopy());
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser, duration, repetition0, start, end, splitCount, false);
            TestSchedule schedule = new TestSchedule(user, refNow);
            schedule.AddToScheduleAndCommitAsync(testEvent0).Wait();
            #endregion
            #region addSecondEvent
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Repetition repetition1 = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, repetitionRange.CreateCopy());
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, duration, repetition1, start, end, splitCount, false);
            schedule = new TestSchedule(user, refNow);
            schedule.AddToScheduleAndCommitAsync(testEvent1).Wait();
            #endregion
            #region pick middle of second subevent in order of start and hit repeat
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            schedule.FindMeSomethingToDo(new Location()).Wait();
            schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            List<SubCalendarEvent> orderedByStart = testEvent0.OrderByStartActiveSubEvents.ToList();
            SubCalendarEvent firstSubEvent = orderedByStart.First();
            CalendarEvent repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            List<SubCalendarEvent> subEvents = repetitionInstance_DB.OrderByStartActiveSubEvents.ToList();
            SubCalendarEvent secondSubevent = subEvents[1];
            SubCalendarEvent thirdSubCalendarEvent = subEvents[2];
            Assert.IsTrue(secondSubevent.End != thirdSubCalendarEvent.Start);
            DateTimeOffset middleOfSubevent = Utility.MiddleTime(secondSubevent);
            DateTimeOffset secondRefNow = middleOfSubevent;
            Location location = TestUtility.getLocations()[0];
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();
            #endregion
            #region second repeat press on second subevent(by time) and verififcation
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            List<SubCalendarEvent> subEventsAfterRepetition = repetitionInstance_DB.OrderByStartActiveSubEvents.ToList();
            thirdSubCalendarEvent = subEventsAfterRepetition[2];
            Assert.IsTrue(thirdSubCalendarEvent.isRepetitionLocked);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();
            #endregion

            TestUtility.reloadTilerUser(ref user, ref tilerUser);

            DateTimeOffset thirdRefNow = secondRefNow.AddMinutes(1);
            schedule = new TestSchedule(user, thirdRefNow);
            schedule.FindMeSomethingToDo(new Location()).Wait();
            schedule.persistToDB().Wait();
            CalendarEvent repetitionInstance= TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            List<SubCalendarEvent> subEventsAfterRepetitionSecond = repetitionInstance.ActiveSubEvents.Where(subEvent => subEvent.End >= thirdRefNow).OrderBy(o => o.Start).ToList();
            Assert.IsTrue(subEventsAfterRepetitionSecond.Count == 5);// there should be only four events rescheduled since the first event happened earlier
            foreach (SubCalendarEvent subEvent in subEventsAfterRepetitionSecond)
            {
                Assert.IsFalse(subEvent.isRepetitionLocked);
            }

        }
    }
}
