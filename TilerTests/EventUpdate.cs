using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using My24HourTimerWPF;
using System.Linq;
using TilerElements;
using System.Collections.Generic;
using TilerCore;

namespace TilerTests
{
    [TestClass]
    public class EventUpdate
    {
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
        //    cleanUpLog();
        //}

        // Test group test the change of the name of sub event
        [TestMethod]
        public void NameOfRigidSubEventUpdate()
        {
            string newName = Guid.NewGuid().ToString();
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            TestSchedule schedule = new TestSchedule(user, refNow, startOfDay);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, true);
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, new EventName(user.getTilerUser(), retrievedCalendarEvent, newName), retrievedCalendarEvent.Start, retrievedCalendarEvent.End, testEvent.NumberOfSplit, retrievedCalendarEvent.Notes.UserNote);
            scheduleReloaded.persistToDB().Wait();
            retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            Assert.AreEqual(retrievedCalendarEvent.getName.NameValue, newName);
            Assert.IsTrue(retrievedCalendarEvent.isTestEquivalent(testEvent));
        }
        [TestMethod]
        public void NameOfRepeatRigidSubEventUpdate()
        {
            /// todo write update implementation for repeating events
        }

        [TestMethod]
        public void NameOfNonRigidSubEventUpdate()
        {
            string newName = Guid.NewGuid().ToString();
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            TestSchedule schedule = new TestSchedule(user, refNow, startOfDay);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration.Add(duration));
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, false);
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, new EventName(user.getTilerUser(), testEvent, newName), testEvent.Start, testEvent.End, testEvent.NumberOfSplit, testEvent.Notes.UserNote);
            scheduleReloaded.persistToDB().Wait();
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            Assert.AreEqual(retrievedCalendarEvent.getName.NameValue, newName);
            Assert.IsTrue(retrievedCalendarEvent.isTestEquivalent(testEvent));
        }

        [TestMethod]
        public void NameOfRepeatNonRigidSubEventUpdate()
        {
            /// todo write update implementation for repeating events
        }

        [TestMethod]
        public void NameOfRestrictedSubEventUpdate()
        {
            string newName = Guid.NewGuid().ToString();
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            TestSchedule schedule = new TestSchedule(user, refNow, startOfDay);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration.Add(duration).Add(duration).Add(duration));
            RestrictionProfile restrictionProfile = new RestrictionProfile(start.Add(duration), duration.Add(duration));
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, false, null, restrictionProfile, now: schedule.Now);
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, new EventName(user.getTilerUser(), testEvent, newName), testEvent.Start, testEvent.End, testEvent.NumberOfSplit, testEvent.Notes.UserNote);
            scheduleReloaded.persistToDB().Wait();
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            Assert.AreEqual(retrievedCalendarEvent.getName.NameValue, newName);
            Assert.IsTrue(retrievedCalendarEvent.isTestEquivalent(testEvent));
        }

        [TestMethod]
        public void NameOfRepeatRestrictedSubEventUpdate()
        {
            /// todo write update implementation for repeating events
        }

        // Test group test the change of the split count of events
        [TestMethod]
        public void SplitCountOfRepeatRigidSubEventUpdate()
        {
            /// todo write update implementation for repeating events
        }

        [TestMethod]
        public void SplitCountOfNonRigidSubEventUpdate()
        {
            /// Increasing the split count
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            TestSchedule schedule = new TestSchedule(user, refNow, startOfDay);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(TimeSpan.FromTicks(duration.Ticks * 5));
            CalendarEvent increaseSplitCountTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, false);
            schedule.AddToScheduleAndCommit(increaseSplitCountTestEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            int newSplitCount = increaseSplitCountTestEvent.NumberOfSplit + 1;
            var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(increaseSplitCountTestEvent.getId, increaseSplitCountTestEvent.getName, increaseSplitCountTestEvent.Start, increaseSplitCountTestEvent.End, newSplitCount, increaseSplitCountTestEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(increaseSplitCountTestEvent.Calendar_EventID, user);
            Assert.AreEqual(retrievedCalendarEvent.NumberOfSplit, newSplitCount);
            Assert.AreEqual(retrievedCalendarEvent.AllSubEvents.Count(), newSplitCount);
            Assert.IsTrue(retrievedCalendarEvent.isTestEquivalent(increaseSplitCountTestEvent));

            /// Reducing the split count
            CalendarEvent decreaseSplitCountTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 3, false);
            schedule.AddToScheduleAndCommit(decreaseSplitCountTestEvent).Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            newSplitCount = decreaseSplitCountTestEvent.NumberOfSplit - 1;
            scheduleUpdated = scheduleReloaded.BundleChangeUpdate(decreaseSplitCountTestEvent.getId, decreaseSplitCountTestEvent.getName, decreaseSplitCountTestEvent.Start, decreaseSplitCountTestEvent.End, newSplitCount, decreaseSplitCountTestEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            retrievedCalendarEvent = TestUtility.getCalendarEventById(decreaseSplitCountTestEvent.Calendar_EventID, user);
            Assert.AreEqual(retrievedCalendarEvent.NumberOfSplit, newSplitCount);
            Assert.AreEqual(retrievedCalendarEvent.AllSubEvents.Count(), newSplitCount);
            Assert.IsTrue(retrievedCalendarEvent.isTestEquivalent(decreaseSplitCountTestEvent));
        }

        [TestMethod]
        public void SplitCountOfRepeatNonRigidSubEventUpdate()
        {
            /// todo write update implementation for repeating events
        }

        [TestMethod]
        public void SplitCountOfRestrictedSubEventUpdate()
        {
            /// Increasing the split count
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            TestSchedule schedule = new TestSchedule(user, refNow, startOfDay);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(TimeSpan.FromTicks(duration.Ticks * 5));
            RestrictionProfile restrictionProfile = new RestrictionProfile(start.Add(duration), duration.Add(duration));
            CalendarEvent increaseSplitCountTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, false, null, restrictionProfile, now: schedule.Now);
            schedule.AddToScheduleAndCommit(increaseSplitCountTestEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            int newSplitCount = increaseSplitCountTestEvent.NumberOfSplit + 1;
            var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(increaseSplitCountTestEvent.getId, increaseSplitCountTestEvent.getName, increaseSplitCountTestEvent.Start, increaseSplitCountTestEvent.End, newSplitCount, increaseSplitCountTestEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(increaseSplitCountTestEvent.Calendar_EventID, user);
            Assert.AreEqual(retrievedCalendarEvent.NumberOfSplit, newSplitCount);
            Assert.AreEqual(retrievedCalendarEvent.AllSubEvents.Count(), newSplitCount);
            Assert.IsTrue(retrievedCalendarEvent.isTestEquivalent(increaseSplitCountTestEvent));

            /// Reducing the split count
            CalendarEvent decreaseSplitCountTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 3, false, null, restrictionProfile, now: schedule.Now);
            schedule.AddToScheduleAndCommit(decreaseSplitCountTestEvent).Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            newSplitCount = decreaseSplitCountTestEvent.NumberOfSplit - 1;
            scheduleUpdated = scheduleReloaded.BundleChangeUpdate(decreaseSplitCountTestEvent.getId, decreaseSplitCountTestEvent.getName, decreaseSplitCountTestEvent.Start, decreaseSplitCountTestEvent.End, newSplitCount, decreaseSplitCountTestEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            retrievedCalendarEvent = TestUtility.getCalendarEventById(decreaseSplitCountTestEvent.Calendar_EventID, user);
            Assert.AreEqual(retrievedCalendarEvent.NumberOfSplit, newSplitCount);
            Assert.AreEqual(retrievedCalendarEvent.AllSubEvents.Count(), newSplitCount);
            Assert.IsTrue(retrievedCalendarEvent.isTestEquivalent(decreaseSplitCountTestEvent));
        }

        [TestMethod]
        public void SplitCountOfRepeatRestrictedSubEventUpdate()
        {
            /// todo write update implementation for repeating events
        }

        // Test group test the change of the deadline
        [TestMethod]
        public void DeadlineOfRigidSubEventUpdate()
        {
            // increases the deadline
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            TestSchedule schedule = new TestSchedule(user, refNow, startOfDay);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, true);
            CalendarEvent testEventCopy = testEvent.createCopy();
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            DateTimeOffset newDeadline = testEvent.End.Add(duration);
            var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.ActiveSubEvents.First().getId, testEvent.getName, testEvent.Start, newDeadline, testEvent.Start, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            TimeSpan OneMinuteDiff = TimeSpan.FromMinutes(1);
            Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs

            // decreases the deadline
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            newDeadline = testEvent.End.Add(TimeSpan.FromTicks((long)duration.Ticks / 2));
            scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.ActiveSubEvents.First().getId, testEvent.getName, testEvent.Start, newDeadline, testEvent.Start, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs when storing events

        }

        public void DeadlineOfRepeatRigidSubEventUpdate()
        {
            /// todo write update implementation for repeating events
        }

        [TestMethod]
        public void DeadlineOfNonRigidSubEventUpdate()
        {
            // increases the deadline
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            TestSchedule schedule = new TestSchedule(user, refNow, startOfDay);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, false);
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            DateTimeOffset newDeadline = testEvent.End.Add(duration);
            var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, testEvent.Start, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            TimeSpan OneMinuteDiff = TimeSpan.FromMinutes(1);
            Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs
            Assert.IsFalse(retrievedCalendarEvent.End == end);// this should evaluate to false because we have modified the deadline of the original calendar event

            // decreases the deadline
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            newDeadline = testEvent.End.Add(TimeSpan.FromTicks((long)duration.Ticks / 2));
            scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, testEvent.Start, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs when storing events
            Assert.IsFalse(retrievedCalendarEvent.End == end);// this should evaluate to false because we have modified the deadline of the original calendar event
        }

        [TestMethod]
        public void DeadlineOfRepeatNonRigidSubEventUpdate()
        {
            /// todo write update implementation for repeating events
        }

        [TestMethod]
        public void DeadlineOfRestrictedSubEventUpdate()
        {
            // increases the deadline
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            TestSchedule schedule = new TestSchedule(user, refNow, startOfDay);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.Add(TimeSpan.FromTicks(duration.Ticks * 5));
            RestrictionProfile restrictionProfile = new RestrictionProfile(start.Add(duration), duration.Add(duration));
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, false, null, restrictionProfile, now: schedule.Now);
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            DateTimeOffset newDeadline = end.Add(duration);
            var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, testEvent.Start, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            TimeSpan OneMinuteDiff = TimeSpan.FromMinutes(1);
            Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs
            Assert.IsFalse(retrievedCalendarEvent.End == end);// this should evaluate to false because we have modified the deadline of the original calendar event

            // decreases the deadline
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            newDeadline = end.Add(-TimeSpan.FromTicks(duration.Ticks * 3));
            scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, testEvent.Start, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs when storing events
            Assert.IsFalse(retrievedCalendarEvent.End == end);// this should evaluate to false because we have modified the deadline of the original calendar event
        }

        [TestMethod]
        public void DeadlineOfRepeatRestrictedSubEventUpdate()
        {
            /// todo write update implementation for repeating events
        }

        //// Test group test the change of the Range time line of the calendar event
        [TestMethod]
        public void RangeTimeLineRigidSubEventUpdate()
        {
            // increases the range
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            TestSchedule schedule = new TestSchedule(user, refNow, startOfDay);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, true);
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            DateTimeOffset newStart = testEvent.Start.Add(-TimeSpan.FromTicks((long)duration.Ticks / 2));
            DateTimeOffset newDeadline = testEvent.End.Add(duration);
            var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.ActiveSubEvents.First().getId, testEvent.getName, newStart, newDeadline, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            TimeSpan OneMinuteDiff = TimeSpan.FromMinutes(1);
            Assert.AreEqual(retrievedCalendarEvent.getActiveDuration, newDeadline - newStart);
            Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs
            Assert.IsTrue((retrievedCalendarEvent.Start - newStart) < OneMinuteDiff);/// doing this because of the rounding that occurs
            Assert.IsFalse(retrievedCalendarEvent.End == end);// this should evaluate to false because we have modified the deadline of the original calendar event

            // decreases the range
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            newStart = newStart.Add(-duration);
            newDeadline = newDeadline.Add(-TimeSpan.FromTicks((long)(duration.Ticks * 2)));
            scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.ActiveSubEvents.First().getId, testEvent.getName, newStart, newDeadline, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            Assert.AreEqual(retrievedCalendarEvent.getActiveDuration, newDeadline - newStart);
            Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs when storing events
            Assert.IsTrue((retrievedCalendarEvent.Start - newStart) < OneMinuteDiff);/// doing this because of the rounding that occurs
            Assert.IsFalse(retrievedCalendarEvent.End == end);// this should evaluate to false because we have modified the deadline of the original calendar event
        }
        [TestMethod]
        public void RangeTimeLineRepeatRigidSubEventUpdate()
        {
            /// todo write update implementation for repeating events
        }

        [TestMethod]
        [ExpectedException(typeof(CustomErrors), "Timeline is to small to fit subevent")]
        public void RangeTimeLineNonRigidSubEventUpdate()
        {
            // increases the range
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            TestSchedule schedule = new TestSchedule(user, refNow, startOfDay);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, false);
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            DateTimeOffset newStart = start.Add(-TimeSpan.FromTicks((long)duration.Ticks / 2));
            DateTimeOffset newDeadline = end.Add(duration);
            var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            TimeSpan OneMinuteDiff = TimeSpan.FromMinutes(1);
            Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs
            Assert.IsTrue((retrievedCalendarEvent.Start - newStart) < OneMinuteDiff);/// doing this because of the rounding that occurs
            Assert.IsFalse(retrievedCalendarEvent.End == end);// this should evaluate to false because we have modified the deadline of the original calendar event

            // decreases the range
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            newStart = newStart.Add(-duration);
            newDeadline = newDeadline.Add(-TimeSpan.FromTicks((long)(duration.Ticks * 2)));
            scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs when storing events
            Assert.IsTrue((retrievedCalendarEvent.Start - newStart) < OneMinuteDiff);/// doing this because of the rounding that occurs
            Assert.IsFalse(retrievedCalendarEvent.End == end);// this should evaluate to false because we have modified the deadline of the original calendar event

            // too small a range
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            newStart = newStart.Add(duration);
            newDeadline = newStart.Add(TimeSpan.FromTicks((long)(duration.Ticks / 2)));
            scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);
        }

        [TestMethod]
        public void RangeTimeLineRepeatNonRigidSubEventUpdate()
        {
            /// todo write update implementation for repeating events
        }

        [TestMethod]
        [ExpectedException(typeof(CustomErrors), "Timeline is to small to fit subevent")]
        public void RangeTimeLineRestrictedSubEventUpdate()
        {
            // increases the range
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            TestSchedule schedule = new TestSchedule(user, refNow, startOfDay);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.Add(duration.Add(duration).Add(duration));
            RestrictionProfile restrictionProfile = new RestrictionProfile(start.Add(duration), duration.Add(duration));
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, false, null, restrictionProfile, now: schedule.Now);
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            DateTimeOffset newStart = start.Add(-TimeSpan.FromTicks((long)duration.Ticks / 2));
            DateTimeOffset newDeadline = end.Add(duration);
            var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            TimeSpan OneMinuteDiff = TimeSpan.FromMinutes(1);
            Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs
            Assert.IsTrue((retrievedCalendarEvent.Start - newStart) < OneMinuteDiff);/// doing this because of the rounding that occurs
            Assert.IsFalse(retrievedCalendarEvent.End == end);// this should evaluate to false because we have modified the deadline of the original calendar event

            // decreases the range
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            newStart = newStart.Add(-duration);
            newDeadline = newDeadline.Add(-TimeSpan.FromTicks((long)(duration.Ticks * 2)));
            scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs when storing events
            Assert.IsTrue((retrievedCalendarEvent.Start - newStart) < OneMinuteDiff);/// doing this because of the rounding that occurs
            Assert.IsFalse(retrievedCalendarEvent.End == end);// this should evaluate to false because we have modified the deadline of the original calendar event

            // too small a range
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            newStart = newStart.Add(duration);
            newDeadline = newStart.Add(TimeSpan.FromTicks((long)(duration.Ticks / 2)));
            scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

            
        }

        [TestMethod]
        public void RangeTimeLineRepeatRestrictedSubEventUpdate()
        {
            /// todo write update implementation for repeating events
        }

        [TestMethod]
        public void RigidSubEventUpdate()
        {
        }

        [TestMethod]
        public void RepeatRigidSubEventUpdate()
        {
        }

        [TestMethod]
        public void NonRigidSubEventUpdate()
        {
        }

        [TestMethod]
        public void RepeatNonRigidSubEventUpdate()
        {
        }

        [TestMethod]
        public void RestrictedSubEventUpdate()
        {
        }

        [TestMethod]
        public void RepeatRestrictedSubEventUpdate()
        {
        }





    }
}
