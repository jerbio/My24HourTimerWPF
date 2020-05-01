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



        [TestMethod]
        public void RepeatAnEventWithSubeventsAfterShouldAutomaticallyAddTheLaterSubeventsAfter ()
        {
            var setupPacket = TestUtility.CreatePacket();
            UserAccount user = setupPacket.Account;
            TilerUser tilerUser = setupPacket.User;
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, 1, 0, 0, 0, new TimeSpan()).getClosestDayOfWeekAfterTime(DayOfWeek.Sunday);// Adding the sunday part to avoid autodeletions reducing the active count
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
            Location location = TestUtility.getAdHocLocations()[0];
            TimeLine secondSubEventStartAndEnd = secondSubevent.StartToEnd;
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            secondSubevent = repetitionInstance_DB.getSubEvent(secondSubevent.Id);
            Assert.IsTrue(secondSubevent.isRepetitionLocked);

            secondSubEventStartAndEnd.isEqualStartAndEnd(secondSubevent.StartToEnd);

            List<SubCalendarEvent> subEventsAfterRepetition = repetitionInstance_DB.ActiveSubEvents.Where(o=>o.Start >= secondSubevent.End).OrderBy(o => o.Start).ToList();// need to get subevents after the recently repetion locked event
            thirdSubCalendarEvent = subEventsAfterRepetition.First();
            Assert.IsTrue(thirdSubCalendarEvent.isRepetitionLocked);
            Assert.IsTrue(secondSubevent.End == thirdSubCalendarEvent.Start);
            Assert.AreEqual(repetitionInstance_DB.NumberOfSplit, splitCount);
            TimeLine thirdSubEventStartAndEnd = thirdSubCalendarEvent.StartToEnd;

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            Assert.AreEqual(repetitionInstance_DB.NumberOfSplit, splitCount);
            List <SubCalendarEvent> subEventsAfterRepetitionSecond = repetitionInstance_DB.ActiveSubEvents.Where(o => o.Start >= secondSubevent.End).OrderBy(o => o.Start).ToList();
            thirdSubCalendarEvent = subEventsAfterRepetitionSecond[0];
            thirdSubEventStartAndEnd.isEqualStartAndEnd(thirdSubCalendarEvent.StartToEnd);
            SubCalendarEvent fourtSubCalendarEvent = subEventsAfterRepetitionSecond[1];
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
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, 1, 0, 0, 0, new TimeSpan()).getClosestDayOfWeekAfterTime(DayOfWeek.Sunday);// Adding the sunday part to avoid autodeletions reducing the active count
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
            Location location = TestUtility.getAdHocLocations()[0];
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
        /// Procrastinateall should reset repetition locks after "Now"
        /// </summary>
        [TestMethod]
        public void procrastinateAllResetsAllrepetitionLock()
        {
            #region init
            var setupPacket = TestUtility.CreatePacket();
            UserAccount user = setupPacket.Account;
            TilerUser tilerUser = setupPacket.User;
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, 1, 0, 0, 0, new TimeSpan()).getClosestDayOfWeekAfterTime(DayOfWeek.Sunday);// Adding the sunday part to avoid autodeletions reducing the active count
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
            Location location = TestUtility.getAdHocLocations()[0];
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();
            #endregion
            #region second repeat press on second subevent(by time) and verififcation
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            List<SubCalendarEvent> subEventsAfterRepetition = repetitionInstance_DB.OrderByStartActiveSubEvents.Where(o => o.Start >= secondSubevent.End).ToList();
            thirdSubCalendarEvent = subEventsAfterRepetition[0];
            Assert.IsTrue(thirdSubCalendarEvent.isRepetitionLocked);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();
            #endregion


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            List<SubCalendarEvent> subEventsAfterRepetitionSecond = repetitionInstance_DB.OrderByStartActiveSubEvents.Where(subEvent => subEvent.End >= secondRefNow).ToList();
            firstSubEvent = subEventsAfterRepetitionSecond[0];
            secondSubevent = subEventsAfterRepetitionSecond[1];
            thirdSubCalendarEvent = subEventsAfterRepetitionSecond[2];
            SubCalendarEvent fourtSubCalendarEvent = subEventsAfterRepetitionSecond[3];
            Assert.IsTrue(firstSubEvent.isRepetitionLocked);
            Assert.IsTrue(secondSubevent.isRepetitionLocked);
            Assert.IsTrue(thirdSubCalendarEvent.isRepetitionLocked);
            Assert.IsFalse(fourtSubCalendarEvent.isRepetitionLocked);
            

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            var calendarIds = new HashSet<string>() { tilerUser.ClearAllId };
            schedule = new TestSchedule(user, secondRefNow, calendarIds: calendarIds);
            TimeSpan threeHourSpan = TimeSpan.FromHours(3);
            var procrartinateResult = schedule.ProcrastinateAll(threeHourSpan);
            DateTimeOffset procrastinationStart = schedule.Now.constNow.Add(threeHourSpan);
            Assert.IsNull(procrartinateResult.Item1);
            schedule.persistToDB().Wait();

            CalendarEvent afterProcrastinataionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            List<SubCalendarEvent> subEventsAfterProcrastinateAll = afterProcrastinataionInstance_DB.OrderByStartActiveSubEvents.Where(subEvent => subEvent.End >= procrastinationStart).ToList();

            Assert.IsTrue(subEventsAfterProcrastinateAll.Count == 5);
            foreach (SubCalendarEvent eachSubEvent in subEventsAfterProcrastinateAll)
            {
                Assert.IsFalse(eachSubEvent.isRepetitionLocked);
            }

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
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, 1, 0, 0, 0, new TimeSpan()).getClosestDayOfWeekAfterTime(DayOfWeek.Sunday);// Adding the sunday part to avoid autodeletions reducing the active count
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
            Location location = TestUtility.getAdHocLocations()[0];
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();
            #endregion
            #region second repeat press on second subevent(by time) and verififcation
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            List<SubCalendarEvent> subEventsAfterRepetition = repetitionInstance_DB.OrderByStartActiveSubEvents.Where(o => o.Start >= secondSubevent.End).ToList();
            thirdSubCalendarEvent = subEventsAfterRepetition[0];
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

            Assert.IsTrue(subEventsAfterSecondRepetition.Count == 5);// there should be only four events rescheduled since the first event happened earlier
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
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, 1, 0, 0, 0, new TimeSpan()).getClosestDayOfWeekAfterTime(DayOfWeek.Sunday);// Adding the sunday part to avoid autodeletions reducing the active count
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
            Location location = TestUtility.getAdHocLocations()[0];
            schedule = new TestSchedule(user, secondRefNow);
            schedule.RepeatEvent(secondSubevent.Id, location);
            schedule.persistToDB().Wait();
            #endregion
            #region second repeat press on second subevent(by time) and verififcation
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            repetitionInstance_DB = TestUtility.getCalendarEventById(firstSubEvent.Id, user);
            List<SubCalendarEvent> subEventsAfterRepetition = repetitionInstance_DB.OrderByStartActiveSubEvents.Where(o => o.Start >= secondSubevent.End).ToList();
            thirdSubCalendarEvent = subEventsAfterRepetition[0];
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
            Assert.IsTrue(subEventsAfterRepetitionSecond.Count == 5);
            foreach (SubCalendarEvent subEvent in subEventsAfterRepetitionSecond)
            {
                Assert.IsFalse(subEvent.isRepetitionLocked);
            }

        }


        /// <summary>
        /// Repeat of restricted event out side the restricted time frame should still work, so if an 2 hour restricted tile is has restriction of 2-5 (3 hour restricted window) if repeat is called then the next repeat event should be attached and should stil work. Even though repeating means breaking the 3 hour window
        /// </summary>
        [TestMethod]
        public void canRepeatRestrictionOutsideRestrictionFrame()
        {
            #region init
            var setupPacket = TestUtility.CreatePacket();
            UserAccount user = setupPacket.Account;
            TilerUser tilerUser = setupPacket.User;
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, 1, 0, 0, 0, new TimeSpan());
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            int hourCount = 2;
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(21);
            TimeLine repetitionRange = new TimeLine(start, end.AddDays(-5).AddHours(-23));
            TimeSpan duration = TimeSpan.FromHours(hourCount);
            TimeSpan restrictionSpan = TimeSpan.FromHours(3);
            int splitCount = 1;
            #endregion
            #region addFirstEvent
            TestSchedule schedule = new TestSchedule(user, refNow);
            DateTimeOffset RestrictionStartTime = start.AddHours(10);
            RestrictionProfile restrictionProfile = new RestrictionProfile(RestrictionStartTime, restrictionSpan);
            Repetition repetition0 = new Repetition(repetitionRange, Repetition.Frequency.DAILY, repetitionRange.CreateCopy());
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser, duration, repetition0, start, end, splitCount, false, restrictionProfile: restrictionProfile, now: schedule.Now);
            schedule.AddToScheduleAndCommitAsync(testEvent0).Wait();
            #endregion

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            List<SubCalendarEvent> subEvents = schedule.getAllActiveSubEvents().OrderBy(o => o.Start).ToList();
            bool pinSuccess = Utility.PinSubEventsToStart(subEvents, testEvent0.StartToEnd);
            SubCalendarEvent firstSubEvent = subEvents[0];
            SubCalendarEvent secondSubEvent = subEvents[1];
            if (!pinSuccess)
            {
                string errorMessage = "Test failed to successfully pin sub events currently scheduled, seems like a broken setup. Refnow is" + refNow.ToString();
                throw new Exception(errorMessage);
            }
            Assert.AreNotEqual(firstSubEvent.End, secondSubEvent.Start);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, firstSubEvent.Start, includeUpdateHistory: true);
            schedule.SetSubeventAsNow(firstSubEvent.Id);
            schedule.persistToDB().Wait();



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, firstSubEvent.Start);
            SubCalendarEvent firstSubEventAsNow = schedule.getSubCalendarEvent(firstSubEvent.Id);
            DateTimeOffset middleOfFirst = Utility.MiddleTime(firstSubEventAsNow);
            schedule = new TestSchedule(user, middleOfFirst);
            schedule.RepeatEvent(firstSubEventAsNow.Id, new Location());
            schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            List<SubCalendarEvent> subEventsAfterRepeat = TestUtility.getCalendarEventById(firstSubEventAsNow.Id, user).OrderByStartActiveSubEvents.ToList();
            SubCalendarEvent firstSubeventAfterRepeat = subEventsAfterRepeat[0];
            SubCalendarEvent secondSubeventAfterRepeat = subEventsAfterRepeat[1];
            Assert.IsTrue(firstSubeventAfterRepeat.isRepetitionLocked);
            Assert.IsTrue(secondSubeventAfterRepeat.isRepetitionLocked);
            Assert.AreEqual(firstSubeventAfterRepeat.End, secondSubeventAfterRepeat.Start);
        }


        /// <summary>
        /// When a non rigid subevent is reajusted by adjusting the start and end time then repeat. the other sub event should be able to snap into place even though the subevent is now rigid.
        /// </summary>
        [TestMethod]
        public void canRepeatAfterNonRigidSubEventIsSetTORigideRestrictionFrame()
        {
            #region init
            var setupPacket = TestUtility.CreatePacket();
            UserAccount user = setupPacket.Account;
            TilerUser tilerUser = setupPacket.User;
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, 1, 0, 0, 0, new TimeSpan());
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            int hourCount = 2;
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(21);
            TimeLine repetitionRange = new TimeLine(start, end.AddDays(-5).AddHours(-23));
            TimeSpan duration = TimeSpan.FromHours(hourCount);
            TimeSpan restrictionSpan = TimeSpan.FromHours(3);
            int splitCount = 1;
            #endregion
            #region addFirstEvent
            TestSchedule schedule = new TestSchedule(user, refNow);
            DateTimeOffset RestrictionStartTime = start.AddHours(10);
            RestrictionProfile restrictionProfile = new RestrictionProfile(RestrictionStartTime, restrictionSpan);
            Repetition repetition0 = new Repetition(repetitionRange, Repetition.Frequency.DAILY, repetitionRange.CreateCopy());
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser, duration, repetition0, start, end, splitCount, false, restrictionProfile: restrictionProfile, now: schedule.Now);
            schedule.AddToScheduleAndCommitAsync(testEvent0).Wait();
            #endregion

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            List<SubCalendarEvent> subEvents = schedule.getAllActiveSubEvents().OrderBy(o => o.Start).ToList();
            bool pinSuccess = Utility.PinSubEventsToStart(subEvents, testEvent0.StartToEnd);
            SubCalendarEvent firstSubEvent = subEvents[0];
            SubCalendarEvent secondSubEvent = subEvents[1];
            if (!pinSuccess)
            {
                string errorMessage = "Test failed to successfully pin sub events currently scheduled, seems like a broken setup. Refnow is" + refNow.ToString();
                throw new Exception(errorMessage);
            }
            Assert.AreNotEqual(firstSubEvent.End, secondSubEvent.Start);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            Assert.IsFalse(firstSubEvent.isRigid);
            schedule.BundleChangeUpdate(firstSubEvent.Id, firstSubEvent.Name, refNow, refNow.Add(restrictionSpan), new DateTimeOffset(), firstSubEvent.CalendarEventRangeEnd, firstSubEvent.ParentCalendarEvent.NumberOfSplit, firstSubEvent.Notes.UserNote);
            schedule.persistToDB().Wait();



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, firstSubEvent.Start);
            SubCalendarEvent firstSubEventReadjusted = schedule.getSubCalendarEvent(firstSubEvent.Id);
            Assert.IsFalse(firstSubEventReadjusted.isRepetitionLocked);
            Assert.IsTrue(firstSubEventReadjusted.isRigid);


            DateTimeOffset middleOfFirst = Utility.MiddleTime(firstSubEventReadjusted);
            schedule = new TestSchedule(user, middleOfFirst);
            schedule.RepeatEvent(firstSubEventReadjusted.Id, new Location());
            schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            List<SubCalendarEvent> subEventsAfterRepeat = TestUtility.getCalendarEventById(firstSubEventReadjusted.Id, user).OrderByStartActiveSubEvents.ToList();
            SubCalendarEvent firstSubeventAfterRepeat = subEventsAfterRepeat[0];
            SubCalendarEvent secondSubeventAfterRepeat = subEventsAfterRepeat[1];
            Assert.IsTrue(firstSubeventAfterRepeat.isRepetitionLocked);
            Assert.IsTrue(secondSubeventAfterRepeat.isRepetitionLocked);
            Assert.AreEqual(firstSubeventAfterRepeat.End, secondSubeventAfterRepeat.Start);
        }



        /// <summary>
        /// Repetition can only run on current event
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CustomErrors), "Cannot repeat Tile that is not current active tile")]
        public void currentEventCanOnlyBeSetAsRepeated()
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

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            List<SubCalendarEvent> subEvents = schedule.getAllActiveSubEvents().OrderBy(o => o.Start).ToList();
            bool pinSuccess = Utility.PinSubEventsToEnd(subEvents, testEvent0.StartToEnd);
            SubCalendarEvent firstSubEvent = subEvents.First();
            if (!pinSuccess || schedule.Now.constNow == firstSubEvent.Start)
            {
                string errorMessage = "Test failed to successfully pin sub events currently scheduled, seems like a broken setup. Refnow is" + refNow.ToString();
                throw new Exception(errorMessage);
            }

            schedule.RepeatEvent(firstSubEvent.Id, new Location());
        }
            

        /// <summary>
        /// Repetition can only run on non rigid events but should work on locked events
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CustomErrors), "Cannot repeat a rigid event")]
        public void repeatCannotRunOnRigid()
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

            #region addRigidEvent
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset rigidStart = refNow.AddDays(1.5);
            DateTimeOffset rigidEnd = rigidStart.AddHours(2);
            Repetition rigidRepetition = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, new TimeLine(rigidStart, rigidEnd));
            TimeSpan rigidDuration = rigidEnd - rigidStart;
            CalendarEvent rigidEvent = TestUtility.generateCalendarEvent(tilerUser, rigidDuration, rigidRepetition, rigidStart, rigidEnd, 1, true);
            schedule = new TestSchedule(user, refNow);
            schedule.AddToScheduleAndCommitAsync(rigidEvent).Wait();
            List<SubCalendarEvent> subEvents = schedule.getCalendarEvent(rigidEvent.Id).ActiveSubEvents.OrderBy(o => o.Start).ToList();
            
            #endregion

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            SubCalendarEvent firstRigidSubevent = schedule.getSubCalendarEvent(subEvents.First().Id);
            DateTimeOffset middleOfRigid = Utility.MiddleTime(firstRigidSubevent);
            schedule = new TestSchedule(user, middleOfRigid);
            schedule.RepeatEvent(firstRigidSubevent.Id, new Location());
        }


    }
}
