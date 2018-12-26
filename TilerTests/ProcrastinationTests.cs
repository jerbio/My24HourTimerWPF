using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerFront;
using My24HourTimerWPF;
using TilerElements;
using TilerCore;

namespace TilerTests
{
    [TestClass]
    public class ProcrastinationTests
    {
        //[ClassCleanup]
        //public static void cleanUpTest()
        //{
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    TestSchedule Schedule = new TestSchedule(currentUser, refNow);
        //    currentUser.DeleteAllCalendarEvents();
        //}

        [TestMethod]
        public void procrastinateSingle()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser,duration, new Repetition(), start, end, 2, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            Schedule = new TestSchedule(user, refNow);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.UpdateWithDifferentSchedule(setAsNowResult.Item2).Wait();

            Schedule = new TestSchedule(user, refNow);
            TimeSpan procrastinationSpan = TimeSpan.FromHours(5);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> procrastinateResult = Schedule.ProcrastinateJustAnEvent(testEvent.AllSubEvents.OrderBy(subEvent => subEvent.Start).First().getId, procrastinationSpan);
            Assert.IsNull(procrastinateResult.Item1);
            Schedule.UpdateWithDifferentSchedule(procrastinateResult.Item2).Wait();
            Schedule = new TestSchedule(user, refNow);

            CalendarEvent testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            DateTimeOffset latestTime = refNow.Add(procrastinationSpan);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= latestTime);
            Assert.IsTrue(testEventCopy.getProcrastinationInfo.PreferredStartTime >= latestTime);
        }



        [TestMethod]
        public void procrastinateSingleEventAroundMultipleEvents()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser,duration, new Repetition(), start, end, 2, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            user.Login().Wait();
            TimeSpan duration0 = TimeSpan.FromHours(2);
            DateTimeOffset start0 = refNow;
            DateTimeOffset end0 = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser,duration0, new Repetition(), start0, end0, 2, false);
            Schedule.AddToScheduleAndCommit(testEvent0).Wait();

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            user.Login().Wait();
            TimeSpan duration1 = TimeSpan.FromHours(2);
            DateTimeOffset start1 = refNow;
            DateTimeOffset end1 = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser,duration1, new Repetition(), start1, end1, 2, false);
            Schedule.AddToScheduleAndCommit(testEvent1).Wait();

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            user.Login().Wait();
            Schedule = new TestSchedule(user, refNow);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.UpdateWithDifferentSchedule(setAsNowResult.Item2).Wait();

            Schedule = new TestSchedule(user, refNow);
            TimeSpan procrastinationSpan = TimeSpan.FromHours(5);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> procrassinateResult = Schedule.ProcrastinateJustAnEvent(testEvent.AllSubEvents.OrderBy(subEvent => subEvent.Start).First().getId, procrastinationSpan);
            Assert.IsNull(procrassinateResult.Item1);
            Schedule.UpdateWithDifferentSchedule(procrassinateResult.Item2).Wait();
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            DateTimeOffset latestTime = refNow.Add(procrastinationSpan);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= latestTime);
        }


        [TestMethod]
        [ExpectedException(typeof(CustomErrors), "Procrasting past the deadline should be geenrated.")]
        public void procrastinateSinglePastDeadline()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddDays(7);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser,duration, new Repetition(), start, end, 1, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            CalendarEvent testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            Schedule = new TestSchedule(user, refNow);
            TimeSpan procrastinationSpan = TimeSpan.FromDays(10);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> procrassinateResult = Schedule.ProcrastinateJustAnEvent(testEvent.AllSubEvents.First().getId, procrastinationSpan);
            Assert.IsNotNull(procrassinateResult.Item1);
        }

        [TestMethod]
        public void procrastinateAll()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser,duration, new Repetition(), start, end, 2, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            Schedule = new TestSchedule(user, refNow);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.UpdateWithDifferentSchedule(setAsNowResult.Item2).Wait();
            Schedule = new TestSchedule(user, refNow);
            TimeSpan procrastinationSpan = TimeSpan.FromHours(2);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> procrassinateResult = Schedule.ProcrastinateAll(procrastinationSpan);
            Assert.IsNull(procrassinateResult.Item1);
            Schedule.UpdateWithDifferentSchedule(procrassinateResult.Item2).Wait();
            Schedule = new TestSchedule(user, refNow);
            EventID procrastinateId = new EventID(user.getTilerUser().ClearAllId);
            CalendarEvent procrastinationEvent = TestUtility.getCalendarEventById(procrastinateId.getCalendarEventID(), user);
            DateTimeOffset endOfProcrastinateAll = refNow.Add(procrastinationSpan);
            Assert.AreEqual(procrastinationEvent.End, endOfProcrastinateAll);
            SubCalendarEvent lastFreeSpaceBlock = procrastinationEvent.AllSubEvents.OrderByDescending(obj => obj.End).First();
            Assert.AreEqual(lastFreeSpaceBlock.End, endOfProcrastinateAll);
            Assert.IsNotNull(procrastinationEvent);
            CalendarEvent testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            DateTimeOffset earliestTImeForNonProcrastinationSubEventTime = refNow.Add(procrastinationSpan);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= earliestTImeForNonProcrastinationSubEventTime);
            int numberOfBlockedOfTimeSlots = procrastinationEvent.ActiveSubEvents.Length;

            // Procrastinate  all that over laps. RefNow is  1 hour after the previous refnow. There should be just one single subevent during overlaps

            DateTimeOffset refNow0 = refNow.AddHours(1);
            Schedule = new TestSchedule(user, refNow0);
            TimeSpan additionalProcrastinationSpan = TimeSpan.FromHours(2);
            procrassinateResult = Schedule.ProcrastinateAll(additionalProcrastinationSpan);
            Assert.IsNull(procrassinateResult.Item1);
            Schedule.UpdateWithDifferentSchedule(procrassinateResult.Item2).Wait();
            Schedule = new TestSchedule(user, refNow0);
            procrastinateId = new EventID(user.getTilerUser().ClearAllId);
            procrastinationEvent = Schedule.getCalendarEvent(procrastinateId);
            endOfProcrastinateAll = refNow0.Add(additionalProcrastinationSpan);
            Assert.AreEqual(procrastinationEvent.End, endOfProcrastinateAll);
            lastFreeSpaceBlock = procrastinationEvent.AllSubEvents.OrderByDescending(obj => obj.End).First();
            Assert.AreEqual(lastFreeSpaceBlock.End, endOfProcrastinateAll);
            Assert.IsNotNull(procrastinationEvent);
            testEventCopy = Schedule.getCalendarEvent(testEvent.getId);
            earliestTImeForNonProcrastinationSubEventTime = refNow0.Add(additionalProcrastinationSpan);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.First().Start >= earliestTImeForNonProcrastinationSubEventTime);
            Assert.AreEqual(procrastinationEvent.ActiveSubEvents.Length, numberOfBlockedOfTimeSlots);

            /// if the user choses to use the edit fields as opposed to the normal procrastinate scroll wheel. And the edit range is less than the already established range of procrastinate calendar sub events
            Schedule = new TestSchedule(user, refNow0);
            DateTimeOffset startOfProcrastinateAll = refNow.AddHours(.5);
            DateTimeOffset newEndOfProcrastinateAll = refNow.AddHours(2.5);
            procrastinationEvent = Schedule.getCalendarEvent(user.getTilerUser().getClearAllEventsId());
            Schedule = new TestSchedule(user, refNow);
            procrastinationEvent = Schedule.getCalendarEvent(user.getTilerUser().getClearAllEventsId());
            SubCalendarEvent firstClearedBlock = procrastinationEvent.ActiveSubEvents.OrderBy(obj => obj.Start).First();
            procrassinateResult = Schedule.BundleChangeUpdate(firstClearedBlock.getId, procrastinationEvent.getName, startOfProcrastinateAll, newEndOfProcrastinateAll, startOfProcrastinateAll, newEndOfProcrastinateAll, procrastinationEvent.NumberOfSplit, procrastinationEvent.Notes.UserNote);

            Assert.IsNull(procrassinateResult.Item1);
            Schedule.UpdateWithDifferentSchedule(procrassinateResult.Item2).Wait();
            Schedule = new TestSchedule(user, refNow0);
            procrastinateId = new EventID(user.getTilerUser().ClearAllId);
            procrastinationEvent = Schedule.getCalendarEvent(procrastinateId);
            endOfProcrastinateAll = newEndOfProcrastinateAll;
            Assert.AreEqual(procrastinationEvent.End, endOfProcrastinateAll);
            lastFreeSpaceBlock = procrastinationEvent.AllSubEvents.OrderByDescending(obj => obj.End).First();
            Assert.AreEqual(lastFreeSpaceBlock.End, endOfProcrastinateAll);
            Assert.IsNotNull(procrastinationEvent);
            testEventCopy = Schedule.getCalendarEvent(testEvent.getId);
            earliestTImeForNonProcrastinationSubEventTime = endOfProcrastinateAll;
            Assert.IsTrue(testEventCopy.ActiveSubEvents.First().Start >= earliestTImeForNonProcrastinationSubEventTime);
            Assert.AreEqual(procrastinationEvent.ActiveSubEvents.Length, numberOfBlockedOfTimeSlots);


            // Procrastinate all if there are no overlaps then we should have multiple active all events
            DateTimeOffset refNow1 = refNow.AddHours(4);
            Schedule = new TestSchedule(user, refNow1);
            TimeSpan additionalProcrastinationSpan0 = TimeSpan.FromHours(1);
            procrassinateResult = Schedule.ProcrastinateAll(additionalProcrastinationSpan0);
            Assert.IsNull(procrassinateResult.Item1);
            Schedule.UpdateWithDifferentSchedule(procrassinateResult.Item2).Wait();
            Schedule = new TestSchedule(user, refNow1);
            procrastinateId = new EventID(user.getTilerUser().ClearAllId);
            procrastinationEvent = Schedule.getCalendarEvent(procrastinateId);
            endOfProcrastinateAll = refNow1.Add(additionalProcrastinationSpan0);
            Assert.AreEqual(procrastinationEvent.End, endOfProcrastinateAll);
            lastFreeSpaceBlock = procrastinationEvent.AllSubEvents.OrderByDescending(obj => obj.End).First();
            Assert.AreEqual(lastFreeSpaceBlock.End, endOfProcrastinateAll);
            Assert.IsNotNull(procrastinationEvent);
            Assert.AreEqual(procrastinationEvent.ActiveSubEvents.Length, numberOfBlockedOfTimeSlots + 1);


            /// if the user choses to use the edit fields as opposed to the normal procrastinate scroll wheel. And the edit is creates a conflict in procrastinateall subcaledar events. It should generaate one contigous block
            Schedule = new TestSchedule(user, refNow1);
            startOfProcrastinateAll = refNow.AddHours(.5);
            newEndOfProcrastinateAll = refNow.AddHours(4.5);
            procrastinationEvent = Schedule.getCalendarEvent(user.getTilerUser().getClearAllEventsId());
            DateTimeOffset desiredEndTIme = procrastinationEvent.End;
            Schedule = new TestSchedule(user, refNow);
            procrastinationEvent = Schedule.getCalendarEvent(user.getTilerUser().getClearAllEventsId());
            firstClearedBlock = procrastinationEvent.ActiveSubEvents.OrderBy(obj => obj.Start).First();
            procrassinateResult = Schedule.BundleChangeUpdate(firstClearedBlock.getId, procrastinationEvent.getName, startOfProcrastinateAll, newEndOfProcrastinateAll, startOfProcrastinateAll, newEndOfProcrastinateAll, procrastinationEvent.NumberOfSplit, firstClearedBlock.Notes.UserNote);

            Assert.IsNull(procrassinateResult.Item1);
            Schedule.UpdateWithDifferentSchedule(procrassinateResult.Item2).Wait();
            Schedule = new TestSchedule(user, refNow0);
            procrastinateId = new EventID(user.getTilerUser().ClearAllId);
            procrastinationEvent = Schedule.getCalendarEvent(procrastinateId);
            endOfProcrastinateAll = newEndOfProcrastinateAll;
            Assert.AreEqual(procrastinationEvent.End, desiredEndTIme);
            lastFreeSpaceBlock = procrastinationEvent.AllSubEvents.OrderByDescending(obj => obj.End).First();
            Assert.AreEqual(lastFreeSpaceBlock.End, desiredEndTIme);
            Assert.IsNotNull(procrastinationEvent);
            testEventCopy = Schedule.getCalendarEvent(testEvent.getId);
            earliestTImeForNonProcrastinationSubEventTime = desiredEndTIme;
            Assert.IsTrue(testEventCopy.ActiveSubEvents.First().Start >= earliestTImeForNonProcrastinationSubEventTime);
            Assert.AreEqual(procrastinationEvent.ActiveSubEvents.Length, numberOfBlockedOfTimeSlots);

        }


        [TestMethod]
        public void scheduleModificationWithProcrastinateAll()
        {
            DB_Schedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser,duration, new Repetition(), start, end, 2, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            Schedule = new TestSchedule(user, refNow);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.UpdateWithDifferentSchedule(setAsNowResult.Item2).Wait();
            Schedule = new TestSchedule(user, refNow);
            TimeSpan procrastinationSpan = TimeSpan.FromHours(4.5);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> procrassinateResult = Schedule.ProcrastinateAll(procrastinationSpan);
            Assert.IsNull(procrassinateResult.Item1);
            Schedule.UpdateWithDifferentSchedule(procrassinateResult.Item2).Wait();
            Schedule = new TestSchedule(user, refNow);
            EventID procrastinateId = new EventID(user.getTilerUser().ClearAllId);
            CalendarEvent procrastinationEvent = Schedule.getCalendarEvent(procrastinateId);
            Assert.IsNotNull(procrastinationEvent);
            CalendarEvent testEventCopy = Schedule.getCalendarEvent(testEvent.getId);
            DateTimeOffset latestTime = refNow.Add(procrastinationSpan);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.First().Start >= latestTime);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser,duration, new Repetition(), start, end.AddDays(1), 2, false);
            Schedule.AddToScheduleAndCommit(testEvent0).Wait();
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser,duration, new Repetition(), start, end.AddDays(1), 2, false);
            Schedule.AddToScheduleAndCommit(testEvent1).Wait();
            Schedule = new TestSchedule(user, refNow.AddHours(5));
            Schedule.FindMeSomethingToDo(new Location()).Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();
        }

        //[ClassInitialize]
        //public static void cleanUpTest(TestContext context)
        //{
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    Schedule Schedule = new TestSchedule(currentUser, refNow);
        //    currentUser.DeleteAllCalendarEvents();
        //}

        //[TestInitialize]
        //public void cleanUpLog()
        //{
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    Schedule Schedule = new TestSchedule(currentUser, refNow);
        //    currentUser.DeleteAllCalendarEvents();
        //}

        //[TestCleanup]
        //public void eachTestCleanUp()
        //{
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    Schedule Schedule = new TestSchedule(currentUser, refNow);
        //    currentUser.DeleteAllCalendarEvents();
        //}
    }
}
