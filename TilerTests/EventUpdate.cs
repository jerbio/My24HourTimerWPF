
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using My24HourTimerWPF;
using System.Linq;
using TilerElements;
using System.Collections.Generic;
using TilerCore;
using System.Threading.Tasks;

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
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
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
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true); ;
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
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
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
            TimeSpan timeSPanPerSubEvent = duration;
            Location location = TestUtility.getAdHocLocations()[1];
            CalendarEvent increaseSplitCountTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, false, location);
            schedule.AddToScheduleAndCommitAsync(increaseSplitCountTestEvent).Wait();
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            int newSplitCount = increaseSplitCountTestEvent.NumberOfSplit + 1;
            var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(increaseSplitCountTestEvent.getId, increaseSplitCountTestEvent.getName, increaseSplitCountTestEvent.Start, increaseSplitCountTestEvent.End, newSplitCount, increaseSplitCountTestEvent.Notes.UserNote);
            increaseSplitCountTestEvent = scheduleReloaded.getCalendarEvent(increaseSplitCountTestEvent.Id);//Using this instead of TestUtility.getCalendarEventById because we need the calemdarevent in memory, not in storage for the future assert
            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(increaseSplitCountTestEvent.Calendar_EventID, user);
            foreach(SubCalendarEvent subEvent in retrievedCalendarEvent.AllSubEvents)
            {
                Assert.AreEqual(timeSPanPerSubEvent, subEvent.getActiveDuration);
            }
            Assert.AreEqual(retrievedCalendarEvent.NumberOfSplit, newSplitCount);
            Assert.AreEqual(retrievedCalendarEvent.AllSubEvents.Count(), newSplitCount);
            Assert.IsTrue(retrievedCalendarEvent.isTestEquivalent(increaseSplitCountTestEvent));

            /// Reducing the split count
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            schedule = new TestSchedule(user, refNow, startOfDay);
            location = TestUtility.getAdHocLocations()[2];
            CalendarEvent decreaseSplitCountTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 3, false, location);
            schedule.AddToScheduleAndCommitAsync(decreaseSplitCountTestEvent).Wait();
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            user.Login().Wait();
            tilerUser = user.getTilerUser();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            newSplitCount = decreaseSplitCountTestEvent.NumberOfSplit - 1;
            scheduleUpdated = scheduleReloaded.BundleChangeUpdate(decreaseSplitCountTestEvent.getId, decreaseSplitCountTestEvent.getName, decreaseSplitCountTestEvent.Start, decreaseSplitCountTestEvent.End, newSplitCount, decreaseSplitCountTestEvent.Notes.UserNote);
            scheduleReloaded.persistToDB().Wait();
            decreaseSplitCountTestEvent = scheduleReloaded.getCalendarEvent(decreaseSplitCountTestEvent.Id);//Using this instead of TestUtility.getCalendarEventById because we need the calemdarevent in memory, not in storage for the future assert
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
            DateTimeOffset refNow = TestUtility.parseAsUTC("4/9/2019 9:00:00 PM +00:00");
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("4/9/2019 10:00:00 PM +00:00");
            TestSchedule schedule = new TestSchedule(user, refNow, startOfDay);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(TimeSpan.FromTicks(duration.Ticks * 5));
            RestrictionProfile restrictionProfile = new RestrictionProfile(start.Add(duration), duration.Add(duration));
            CalendarEvent increaseSplitCountTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, false, null, restrictionProfile, now: schedule.Now);
            schedule.AddToScheduleAndCommitAsync(increaseSplitCountTestEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
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
            schedule.AddToScheduleAndCommitAsync(decreaseSplitCountTestEvent).Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
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
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
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
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            DateTimeOffset newDeadline = testEvent.End.Add(duration);
            var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, testEvent.Start, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            TimeSpan OneMinuteDiff = TimeSpan.FromMinutes(1);
            Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs
            Assert.IsFalse(retrievedCalendarEvent.End == end);// this should evaluate to false because we have modified the deadline of the original calendar event

            // decreases the deadline
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
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
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            DateTimeOffset newDeadline = end.Add(duration);
            var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, testEvent.Start, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            TimeSpan OneMinuteDiff = TimeSpan.FromMinutes(1);
            Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs
            Assert.IsFalse(retrievedCalendarEvent.End == end);// this should evaluate to false because we have modified the deadline of the original calendar event

            // decreases the deadline
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
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
            DateTimeOffset iniRefNow = TestUtility.parseAsUTC("8/21/2019 11:21:46 PM +00:00");
            DateTimeOffset refNow = iniRefNow;
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            TestSchedule schedule = new TestSchedule(user, refNow, startOfDay);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, true);
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();

            DateTimeOffset newStart = testEvent.Start.Add(-TimeSpan.FromTicks((long)duration.Ticks / 2));
            DateTimeOffset newDeadline = testEvent.End.Add(duration);
            refNow = newStart;
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.ActiveSubEvents.First().getId, testEvent.getName, newStart, newDeadline, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);
            Assert.IsTrue(scheduleReloaded.IsScheduleModified);
            scheduleReloaded.persistToDB().Wait();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            TimeSpan OneMinuteDiff = TimeSpan.FromMinutes(1);
            Assert.AreEqual(retrievedCalendarEvent.getActiveDuration, newDeadline - newStart);
            Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs
            Assert.IsTrue((retrievedCalendarEvent.StartToEnd.Start - newStart) < OneMinuteDiff);/// doing this because of the rounding that occurs
            Assert.IsFalse(retrievedCalendarEvent.StartToEnd.End == end);// this should evaluate to false because we have modified the deadline of the original calendar event

            // decreases the range
            newStart = newStart.Add(-duration);
            newDeadline = newDeadline.Add(-TimeSpan.FromTicks((long)(duration.Ticks * 2)));
            TimeLine shiftedTImeLine = new TimeLine(newStart, newDeadline);
            TimeSpan shiftSpan = refNow - shiftedTImeLine.End;
            newStart = shiftedTImeLine.Start.Add(shiftSpan).removeSecondsAndMilliseconds();
            newDeadline = shiftedTImeLine.End.Add(shiftSpan).removeSecondsAndMilliseconds();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.ActiveSubEvents.First().getId, testEvent.getName, newStart, 
                newDeadline, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);//this should not make any modification to the schedule since the event is moved to a time earlier than now
            Assert.IsFalse(scheduleReloaded.IsScheduleModified);
            refNow = newStart;
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
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
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
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
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
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
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
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
        [ExpectedException(typeof(CustomErrors), "The restricted timeline update cannot contain restriction time frames")]
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
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
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
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
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
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            newStart = newStart.Add(duration);
            newDeadline = newStart.Add(TimeSpan.FromTicks((long)(duration.Ticks / 2)));
            scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);


        }

        [TestMethod]
        public void RangeTimeLineRepeatRestrictedSubEventUpdate()
        {
            
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
            //DB_Schedule Schedule;
            DateTimeOffset refNow = TestUtility.parseAsUTC("9:00 am");
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            var packet = TestUtility.CreatePacket();
            TilerUser tilerUser = packet.User;
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(21);
            

            TimeSpan timeSPanPerSubEvent = duration;
            Location location = TestUtility.getAdHocLocations()[1];
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent nonRigidCalendarEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 5, false, location);
            DB_Schedule Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommitAsync(nonRigidCalendarEvent).Wait();
            nonRigidCalendarEvent = Schedule.getCalendarEvent(nonRigidCalendarEvent.Id);


            SubCalendarEvent testSubEvent = nonRigidCalendarEvent.ActiveSubEvents.OrderBy(sub => sub.Start).ToList()[2];// the ordeing is needed because you might get a sub event thats at the edge of the calendarEvent timeline. I selected index 2 just because ;)
            DateTimeOffset newStart = testSubEvent.Start.AddDays(10);
            DateTimeOffset newEnd = newStart.Add(duration);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay);
            var scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, newStart, newEnd, nonRigidCalendarEvent.Start, nonRigidCalendarEvent.End, nonRigidCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Task<SubCalendarEvent> subEentTask = user.ScheduleLogControl.getSubEventWithID(testSubEvent.Id);
            subEentTask.Wait();
            SubCalendarEvent subEvent = subEentTask.Result;
            Assert.IsTrue(subEvent.Start == newStart);
            Assert.IsTrue(subEvent.End == newEnd);
            Assert.IsTrue(subEvent.isLocked);

            List<TimeLine> timeLines = TestUtility.getTimeFrames(refNow, duration).GetRange(0, 10);
            List<CalendarEvent> createdCalevents = new List<CalendarEvent>();
            foreach (TimeLine eachTimeLine in timeLines)
            {
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                DateTimeOffset TimeCreation = DateTimeOffset.UtcNow;
                CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), eachTimeLine.Start, eachTimeLine.End, 1, false);
                testEvent.TimeCreated = TimeCreation;
                Schedule = new TestSchedule(user, refNow);
                Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
                createdCalevents.Add(testEvent);
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                string testEVentId = testEvent.getId;
                Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEVentId);
                waitVar.Wait();
                CalendarEvent newlyaddedevent = waitVar.Result;
                Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
                Assert.AreEqual(testEvent.TimeCreated, TimeCreation);
            }

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            subEentTask = user.ScheduleLogControl.getSubEventWithID(testSubEvent.Id);
            subEentTask.Wait();
            subEvent = subEentTask.Result;
            Assert.IsTrue(subEvent.Start == newStart);
            Assert.IsTrue(subEvent.End == newEnd);
            Assert.IsTrue(subEvent.isLocked);
        }

        [TestMethod]
        public void DeadlineUpdateBySubEventChange()
        {
            //DB_Schedule Schedule;
            DateTimeOffset refNow = TestUtility.parseAsUTC("9:00 am");
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            var packet = TestUtility.CreatePacket();
            TilerUser tilerUser = packet.User;
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(10);


            TimeSpan timeSPanPerSubEvent = duration;
            Location location = TestUtility.getAdHocLocations()[1];
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent nonRigidCalendarEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 5, false);
            DB_Schedule Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommitAsync(nonRigidCalendarEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay);
            nonRigidCalendarEvent = Schedule.getCalendarEvent(nonRigidCalendarEvent.Id);
            SubCalendarEvent testSubEvent = nonRigidCalendarEvent.ActiveSubEvents.OrderBy(subEventIter => subEventIter.Start).First();

            DateTimeOffset tileNewStart = start.AddDays(1);
            DateTimeOffset tileNewEnd = tileNewStart.Add(duration);
            var scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, tileNewStart, tileNewEnd, nonRigidCalendarEvent.Start, nonRigidCalendarEvent.End, nonRigidCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);

            SubCalendarEvent testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            CalendarEvent nonRigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            Assert.IsTrue(testSubEventRetrieved.Start == tileNewStart);
            Assert.IsTrue(testSubEventRetrieved.End == tileNewEnd);
            Assert.IsTrue(testSubEventRetrieved.isLocked);
            Assert.IsTrue(nonRigidCalendarEventRetrieved.Start == start);
            Assert.IsTrue(nonRigidCalendarEventRetrieved.End == end);


            DateTimeOffset rigidStart = refNow.AddDays(1);
            DateTimeOffset rigidEnd = rigidStart.AddHours(5);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent RigidCalendarEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), rigidStart, rigidEnd, 1, true);
            Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommitAsync(RigidCalendarEvent).Wait();
            RigidCalendarEvent = Schedule.getCalendarEvent(RigidCalendarEvent.Id);
            Assert.IsTrue(RigidCalendarEvent.Start == rigidStart);
            Assert.IsTrue(RigidCalendarEvent.End == rigidEnd);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            DateTimeOffset rigidNewStart = rigidStart.AddDays(1);
            DateTimeOffset rigidNewEnd = rigidNewStart.Add(duration);
            testSubEvent = RigidCalendarEvent.ActiveSubEvents.OrderBy(subEventIter => subEventIter.Start).First();
            scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, rigidNewStart, rigidNewEnd, RigidCalendarEvent.Start, RigidCalendarEvent.End, RigidCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            CalendarEvent RigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            Assert.IsTrue(testSubEventRetrieved.Start == rigidNewStart);
            Assert.IsTrue(testSubEventRetrieved.End == rigidNewEnd);
            Assert.IsTrue(testSubEventRetrieved.isLocked);
            Assert.IsTrue(RigidCalendarEventRetrieved.Start == rigidNewStart);
            Assert.IsTrue(RigidCalendarEventRetrieved.End == rigidNewEnd);



            DateTimeOffset restrictedStart = refNow.AddDays(1);
            DateTimeOffset restrictedEnd = restrictedStart.AddDays(10);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            RestrictionProfile restrictionProfile = new RestrictionProfile(restrictedStart.Add(duration), TimeSpan.FromMinutes(duration.TotalMinutes * 4));
            CalendarEvent restrictedCalendarEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), restrictedStart, restrictedEnd, 1, false);
            Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommitAsync(restrictedCalendarEvent).Wait();
            restrictedCalendarEvent = Schedule.getCalendarEvent(restrictedCalendarEvent.Id);
            Assert.IsTrue(restrictedCalendarEvent.Start == restrictedStart);
            Assert.IsTrue(restrictedCalendarEvent.End == restrictedEnd);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            DateTimeOffset restrictedNewStart = restrictedStart.AddDays(1);
            DateTimeOffset restrictedNewEnd = restrictedNewStart.Add(duration);
            testSubEvent = restrictedCalendarEvent.ActiveSubEvents.OrderBy(subEventIter => subEventIter.Start).First();
            scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, restrictedNewStart, restrictedNewEnd, restrictedCalendarEvent.Start, restrictedCalendarEvent.End, restrictedCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            CalendarEvent restrictedCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            Assert.IsTrue(testSubEventRetrieved.Start == restrictedNewStart);
            Assert.IsTrue(testSubEventRetrieved.End == restrictedNewEnd);
            Assert.IsTrue(testSubEventRetrieved.isLocked);
            Assert.IsTrue(restrictedCalendarEventRetrieved.Start == restrictedStart);
            Assert.IsTrue(restrictedCalendarEventRetrieved.End == restrictedEnd);


            ////////////////////////////////////////////////////////////////////AfterDeadline////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            nonRigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            testSubEvent = nonRigidCalendarEventRetrieved.ActiveSubEvents.OrderBy(o => o.Start).First();
            tileNewStart = nonRigidCalendarEventRetrieved.End.AddDays(1);
            tileNewEnd = tileNewStart.Add(testSubEvent.getActiveDuration);
            scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, tileNewStart, tileNewEnd, nonRigidCalendarEvent.Start, nonRigidCalendarEvent.End, nonRigidCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();
            testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            Assert.IsTrue(testSubEventRetrieved.isRigid);
            Assert.AreEqual(testSubEventRetrieved.End, tileNewEnd);

            /// Make deadline earlier than current deadline
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            DateTimeOffset nonRigidOldStart = nonRigidCalendarEventRetrieved.Start;
            List<SubCalendarEvent> orderedByStartSubEvent = nonRigidCalendarEventRetrieved.ActiveSubEvents.OrderBy(o => o.Start).ToList();
            testSubEvent = orderedByStartSubEvent[0];
            tileNewStart = testSubEvent.Start;
            tileNewEnd = testSubEvent.End;
            DateTimeOffset newCalEventEnd = nonRigidCalendarEventRetrieved.End.AddDays(-1);
            scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, tileNewStart, tileNewEnd, nonRigidCalendarEvent.Start, newCalEventEnd, nonRigidCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();

            nonRigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            Assert.IsTrue(nonRigidCalendarEventRetrieved.Start == nonRigidOldStart);
            Assert.IsTrue(nonRigidCalendarEventRetrieved.End == newCalEventEnd);

            /// Make deadline earlier than the latest sub event
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            nonRigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            nonRigidOldStart = nonRigidCalendarEventRetrieved.Start;
            orderedByStartSubEvent = nonRigidCalendarEventRetrieved.ActiveSubEvents.OrderBy(o => o.End).ToList();
            SubCalendarEvent lastSubEvent = orderedByStartSubEvent.Last();
            DateTimeOffset lastSubEventStart = lastSubEvent.Start;
            DateTimeOffset lastSubEventEnd = lastSubEvent.End;

            testSubEvent = orderedByStartSubEvent[orderedByStartSubEvent.Count - 2];// Using the second to the last sub event because the last sub event should be rigid and should not move
            tileNewStart = testSubEvent.Start;
            tileNewEnd = testSubEvent.End;
            newCalEventEnd = testSubEvent.End.AddDays(-1);
            scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, tileNewStart, tileNewEnd, nonRigidCalendarEvent.Start, newCalEventEnd, nonRigidCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();

            nonRigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            SubCalendarEvent LastSubEventRetrieved = TestUtility.getSubEventById(lastSubEvent.Id, user);
            Assert.IsTrue(nonRigidCalendarEventRetrieved.Start == nonRigidOldStart);
            Assert.IsTrue(nonRigidCalendarEventRetrieved.End == newCalEventEnd);
            Assert.IsTrue(testSubEventRetrieved.End <= newCalEventEnd);
            Assert.AreEqual(lastSubEventStart, LastSubEventRetrieved.Start);
            Assert.AreEqual(lastSubEventEnd, LastSubEventRetrieved.End);


            /// If subevent is scheduled after the calendar event deadline
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            nonRigidOldStart = nonRigidCalendarEventRetrieved.Start;
            testSubEvent = nonRigidCalendarEventRetrieved.ActiveSubEvents.First();
            tileNewStart = nonRigidCalendarEventRetrieved.End.AddDays(1);
            tileNewEnd = tileNewStart.Add(testSubEvent.getActiveDuration);
            scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, tileNewStart, tileNewEnd, nonRigidCalendarEventRetrieved.Start, nonRigidCalendarEventRetrieved.End, nonRigidCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();
            testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            nonRigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            Assert.IsTrue(testSubEventRetrieved.Start == tileNewStart);
            Assert.IsTrue(testSubEventRetrieved.End == tileNewEnd);
            Assert.IsTrue(testSubEventRetrieved.isLocked);
            Assert.IsTrue(nonRigidCalendarEventRetrieved.Start == nonRigidOldStart);
            Assert.IsTrue(nonRigidCalendarEventRetrieved.End == testSubEventRetrieved.End);


            /// If subevent is scheduled before the calendar event start time
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            nonRigidOldStart = nonRigidCalendarEventRetrieved.Start;
            DateTimeOffset nonRigidOldEnd = nonRigidCalendarEventRetrieved.End;
            testSubEvent = nonRigidCalendarEventRetrieved.ActiveSubEvents.First();
            tileNewStart = nonRigidCalendarEventRetrieved.Start.AddDays(-1);
            tileNewEnd = tileNewStart.Add(testSubEvent.getActiveDuration);
            scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, tileNewStart, tileNewEnd, nonRigidCalendarEventRetrieved.Start, nonRigidCalendarEventRetrieved.End, nonRigidCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();
            testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            nonRigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            Assert.IsTrue(testSubEventRetrieved.Start == tileNewStart);
            Assert.IsTrue(testSubEventRetrieved.End == tileNewEnd);
            Assert.IsTrue(testSubEventRetrieved.isLocked);
            Assert.IsTrue(nonRigidCalendarEventRetrieved.Start == tileNewStart);
            Assert.IsTrue(nonRigidCalendarEventRetrieved.End == nonRigidOldEnd);



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            testSubEvent = RigidCalendarEventRetrieved.ActiveSubEvents.First();
            tileNewStart = RigidCalendarEventRetrieved.End.AddDays(1);
            tileNewEnd = tileNewStart.Add(testSubEvent.getActiveDuration);
            scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, tileNewStart, tileNewEnd, 
                RigidCalendarEventRetrieved.Start, RigidCalendarEventRetrieved.End, RigidCalendarEventRetrieved.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();
            testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            RigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            Assert.IsTrue(testSubEventRetrieved.Start == tileNewStart);
            Assert.IsTrue(testSubEventRetrieved.End == tileNewEnd);
            Assert.IsTrue(testSubEventRetrieved.isLocked);
            Assert.IsTrue(RigidCalendarEventRetrieved.Start == testSubEventRetrieved.Start);
            Assert.IsTrue(RigidCalendarEventRetrieved.End == testSubEventRetrieved.End);


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            restrictedCalendarEventRetrieved = TestUtility.getCalendarEventById(restrictedCalendarEventRetrieved.Id, user);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            DateTimeOffset restrictedOldStart = restrictedCalendarEventRetrieved.Start;
            testSubEvent = restrictedCalendarEventRetrieved.ActiveSubEvents.OrderBy(o=>o.Start).First();
            tileNewStart = restrictedCalendarEventRetrieved.End.AddDays(1);
            tileNewEnd = tileNewStart.Add(testSubEvent.getActiveDuration);
            scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, tileNewStart, tileNewEnd,
                restrictedCalendarEventRetrieved.Start, restrictedCalendarEventRetrieved.End, restrictedCalendarEventRetrieved.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();
            testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            restrictedCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            Assert.IsTrue(testSubEventRetrieved.Start == tileNewStart);
            Assert.IsTrue(testSubEventRetrieved.End == tileNewEnd);
            Assert.IsTrue(testSubEventRetrieved.isLocked);
            Assert.IsTrue(restrictedCalendarEventRetrieved.Start == restrictedOldStart);
            Assert.IsTrue(restrictedCalendarEventRetrieved.End == testSubEventRetrieved.End);
        }


        [TestMethod]
        public void DeadlineUpdateBySubEventChange_Repeat()
        {
            //DB_Schedule Schedule;
            DateTimeOffset refNow = TestUtility.parseAsUTC("9:00 am");
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            var packet = TestUtility.CreatePacket();
            TilerUser tilerUser = packet.User;
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(10);


            TimeSpan timeSPanPerSubEvent = duration;
            Location location = TestUtility.getAdHocLocations()[1];
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TimeLine repeatTimeLine = new TimeLine(start, start.AddDays(21));
            TimeLine actualRangeTimeLine = new TimeLine(start, end);
            Repetition repeat = new Repetition(repeatTimeLine, Repetition.Frequency.DAILY, actualRangeTimeLine);
            CalendarEvent nonRigidCalendarEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repeat, start, end, 5, false);
            DB_Schedule Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommitAsync(nonRigidCalendarEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            HashSet<string> calendarIds = new HashSet<string>() { nonRigidCalendarEvent.Id };
            Schedule = new TestSchedule(user, refNow, startOfDay, calendarIds: calendarIds, includeUpdateHistory: true);
            nonRigidCalendarEvent = Schedule.getCalendarEvent(nonRigidCalendarEvent.Id);
            SubCalendarEvent testSubEvent = Schedule.getAllRelatedActiveSubEvents(nonRigidCalendarEvent.Id).OrderBy(subEventIter => subEventIter.Start).First();

            DateTimeOffset tileNewStart = start.AddDays(1);
            DateTimeOffset tileNewEnd = tileNewStart.Add(duration);
            var scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, tileNewStart, tileNewEnd, testSubEvent.ParentCalendarEvent.Start, testSubEvent.ParentCalendarEvent.End, nonRigidCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);

            SubCalendarEvent testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            CalendarEvent nonRigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            CalendarEvent parentNonRigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.getTilerID.getCalendarEventID(), user);
            Assert.IsTrue(testSubEventRetrieved.Start == tileNewStart);
            Assert.IsTrue(testSubEventRetrieved.End == tileNewEnd);
            Assert.IsTrue(testSubEventRetrieved.isLocked);
            Assert.IsTrue(nonRigidCalendarEventRetrieved.Start == testSubEvent.ParentCalendarEvent.Start);
            Assert.IsTrue(nonRigidCalendarEventRetrieved.End == tileNewEnd);
            Assert.IsTrue(parentNonRigidCalendarEventRetrieved.Start == start);
            Assert.IsTrue(parentNonRigidCalendarEventRetrieved.End == end);


            DateTimeOffset rigidStart = refNow.AddDays(1);
            DateTimeOffset rigidEnd = rigidStart.AddHours(5);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TimeLine RigidActualRangeTimeLine = new TimeLine(rigidStart, rigidEnd);
            Repetition rigidRepeat = new Repetition(repeatTimeLine, Repetition.Frequency.DAILY, RigidActualRangeTimeLine);
            CalendarEvent RigidCalendarEvent = TestUtility.generateCalendarEvent(tilerUser, duration, rigidRepeat, rigidStart, rigidEnd, 1, true);
            Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommitAsync(RigidCalendarEvent).Wait();
            RigidCalendarEvent = Schedule.getCalendarEvent(RigidCalendarEvent.Id);
            Assert.IsTrue(RigidCalendarEvent.Start == rigidStart);
            Assert.IsTrue(RigidCalendarEvent.End == rigidEnd);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            DateTimeOffset rigidNewStart = rigidStart.AddDays(1);
            DateTimeOffset rigidNewEnd = rigidNewStart.Add(duration);
            testSubEvent = RigidCalendarEvent.ActiveSubEvents.OrderBy(subEventIter => subEventIter.Start).First();
            scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, rigidNewStart, rigidNewEnd, testSubEvent.ParentCalendarEvent.Start, testSubEvent.ParentCalendarEvent.End, RigidCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            CalendarEvent RigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            CalendarEvent parentRigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.getTilerID.getCalendarEventID(), user);
            Assert.IsTrue(testSubEventRetrieved.Start == rigidNewStart);
            Assert.IsTrue(testSubEventRetrieved.End == rigidNewEnd);
            Assert.IsTrue(testSubEventRetrieved.isLocked);
            Assert.IsTrue(RigidCalendarEventRetrieved.Start == rigidNewStart);
            Assert.IsTrue(RigidCalendarEventRetrieved.End == rigidNewEnd);
            Assert.IsTrue(parentRigidCalendarEventRetrieved.Start == rigidNewStart);//for rigid events, this does not extend the timeline of the root calendarevent
            Assert.IsTrue(parentRigidCalendarEventRetrieved.End == rigidNewEnd);//for rigid events, this does not extend the timeline of the root calendarevent



            DateTimeOffset restrictedStart = refNow.AddDays(1);
            DateTimeOffset restrictedEnd = restrictedStart.AddDays(10);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TimeLine restrictedActualRangeTimeLine = new TimeLine(rigidStart, rigidEnd);
            Repetition restrictedRepeat = new Repetition(repeatTimeLine, Repetition.Frequency.DAILY, restrictedActualRangeTimeLine);
            RestrictionProfile restrictionProfile = new RestrictionProfile(restrictedStart.Add(duration), TimeSpan.FromMinutes(duration.TotalMinutes * 4));
            CalendarEvent restrictedCalendarEvent = TestUtility.generateCalendarEvent(tilerUser, duration, restrictedRepeat, restrictedStart, restrictedEnd, 1, false);
            Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommitAsync(restrictedCalendarEvent).Wait();
            restrictedCalendarEvent = Schedule.getCalendarEvent(restrictedCalendarEvent.Id);
            Assert.IsTrue(restrictedCalendarEvent.Start == restrictedStart);
            Assert.IsTrue(restrictedCalendarEvent.End == restrictedEnd);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            DateTimeOffset restrictedNewStart = restrictedStart.AddDays(1);
            DateTimeOffset restrictedNewEnd = restrictedNewStart.Add(duration);
            testSubEvent = restrictedCalendarEvent.ActiveSubEvents.OrderBy(subEventIter => subEventIter.Start).First();
            scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, restrictedNewStart, restrictedNewEnd, testSubEvent.ParentCalendarEvent.Start, testSubEvent.ParentCalendarEvent.End, restrictedCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            CalendarEvent restrictedCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            CalendarEvent parentRestrictedCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.getTilerID.getCalendarEventID(), user);
            Assert.IsTrue(testSubEventRetrieved.Start == restrictedNewStart);
            Assert.IsTrue(testSubEventRetrieved.End == restrictedNewEnd);
            Assert.IsTrue(testSubEventRetrieved.isLocked);
            Assert.IsTrue(restrictedCalendarEventRetrieved.Start == restrictedStart);
            Assert.IsTrue(restrictedCalendarEventRetrieved.End == restrictedNewEnd);
            Assert.IsTrue(parentRestrictedCalendarEventRetrieved.Start == restrictedStart);
            Assert.IsTrue(parentRestrictedCalendarEventRetrieved.End == restrictedEnd);


            ////////////////////////////////////////////////////////////////////AfterDeadline////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            testSubEvent = parentNonRigidCalendarEventRetrieved.ActiveSubEvents.First();
            tileNewStart = parentNonRigidCalendarEventRetrieved.End.AddDays(1);
            tileNewEnd = tileNewStart.Add(testSubEvent.getActiveDuration);
            scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, tileNewStart, tileNewEnd, 
                testSubEvent.ParentCalendarEvent.Start, testSubEvent.ParentCalendarEvent.End, nonRigidCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            DateTimeOffset nonRigidOldStart = nonRigidCalendarEventRetrieved.Start;
            testSubEvent = nonRigidCalendarEventRetrieved.ActiveSubEvents.OrderByDescending(obj => obj.End).First();
            tileNewStart = testSubEvent.End.AddDays(10);
            tileNewEnd = tileNewStart.Add(testSubEvent.getActiveDuration);
            scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, tileNewStart, tileNewEnd, 
                testSubEvent.ParentCalendarEvent.Start, testSubEvent.ParentCalendarEvent.End, nonRigidCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();
            testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            nonRigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            parentNonRigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.getTilerID.getCalendarEventID(), user);
            Assert.IsTrue(testSubEventRetrieved.Start == tileNewStart);
            Assert.IsTrue(testSubEventRetrieved.End == tileNewEnd);
            Assert.IsTrue(testSubEventRetrieved.isLocked);
            Assert.IsTrue(nonRigidCalendarEventRetrieved.Start == nonRigidOldStart);
            Assert.IsTrue(nonRigidCalendarEventRetrieved.End == testSubEventRetrieved.End);
            Assert.IsTrue(parentNonRigidCalendarEventRetrieved.Start == start);
            Assert.IsTrue(parentNonRigidCalendarEventRetrieved.End == tileNewEnd);



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            testSubEvent = RigidCalendarEventRetrieved.ActiveSubEvents.OrderByDescending(obj => obj.End).First();
            tileNewStart = testSubEvent.End.AddDays(10);
            tileNewEnd = tileNewStart.Add(testSubEvent.getActiveDuration);
            scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, tileNewStart, tileNewEnd,
                RigidCalendarEventRetrieved.Start, RigidCalendarEventRetrieved.End, RigidCalendarEventRetrieved.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();
            testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            RigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            parentRigidCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.getTilerID.getCalendarEventID(), user);
            Assert.IsTrue(testSubEventRetrieved.Start == tileNewStart);
            Assert.IsTrue(testSubEventRetrieved.End == tileNewEnd);
            Assert.IsTrue(testSubEventRetrieved.isLocked);
            Assert.IsTrue(RigidCalendarEventRetrieved.Start == testSubEventRetrieved.Start);
            Assert.IsTrue(RigidCalendarEventRetrieved.End == testSubEventRetrieved.End);
            Assert.IsTrue(parentRigidCalendarEventRetrieved.Start == tileNewStart);//for rigid events, this does not extend the timeline of the root calendarevent
            Assert.IsTrue(parentRigidCalendarEventRetrieved.End == tileNewEnd);//for rigid events, this does not extend the timeline of the root calendarevent


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            DateTimeOffset restrictedOldStart = restrictedCalendarEventRetrieved.Start;
            testSubEvent = restrictedCalendarEventRetrieved.ActiveSubEvents.First();
            tileNewStart = restrictedCalendarEventRetrieved.End.AddDays(1);
            tileNewEnd = tileNewStart.Add(testSubEvent.getActiveDuration);
            scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, tileNewStart, tileNewEnd,
                testSubEvent.ParentCalendarEvent.Start, testSubEvent.ParentCalendarEvent.End, restrictedCalendarEventRetrieved.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();
            testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            restrictedCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.Id, user);
            parentRestrictedCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.getTilerID.getCalendarEventID(), user);
            Assert.IsTrue(testSubEventRetrieved.Start == tileNewStart);
            Assert.IsTrue(testSubEventRetrieved.End == tileNewEnd);
            Assert.IsTrue(testSubEventRetrieved.isLocked);
            Assert.IsTrue(restrictedCalendarEventRetrieved.Start == restrictedOldStart);
            Assert.IsTrue(restrictedCalendarEventRetrieved.End == testSubEventRetrieved.End);
            Assert.IsTrue(parentRestrictedCalendarEventRetrieved.Start == restrictedStart);
            Assert.IsTrue(parentRestrictedCalendarEventRetrieved.End == restrictedEnd);
        }

        /// <summary>
        /// This test verfies if the you have a repetition. And one of the repetition sequences is updated out side the root calendar event
        /// then the root calendarEvent needs to be extended too
        /// </summary>
        [TestMethod]
        public void DeadlineUpdateOutsideRepetitionRangeShouldUpdateRootCalendarEVent_Repeat()
        {
            //DB_Schedule Schedule;
            DateTimeOffset refNow = TestUtility.parseAsUTC("9:00 am");
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            var packet = TestUtility.CreatePacket();
            TilerUser tilerUser = packet.User;
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(5);


            TimeSpan timeSPanPerSubEvent = duration;
            Location location = TestUtility.getAdHocLocations()[1];
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TimeLine repeatTimeLine = new TimeLine(start, start.AddDays(21));
            TimeLine actualRangeTimeLine = new TimeLine(start, end);
            Repetition repeat = new Repetition(repeatTimeLine, Repetition.Frequency.WEEKLY, actualRangeTimeLine);
            CalendarEvent nonRigidRepeatCalendarEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repeat, start, end, 5, false);
            DB_Schedule Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommitAsync(nonRigidRepeatCalendarEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            SubCalendarEvent testSubEvent = nonRigidRepeatCalendarEvent.ActiveSubEvents.First();
            DateTimeOffset tileNewStart = testSubEvent.Start;
            DateTimeOffset tileNewEnd = testSubEvent.End;
            DateTimeOffset calEventNewStart = nonRigidRepeatCalendarEvent.Start.AddDays(-1);
            DateTimeOffset calEventNewEnd = nonRigidRepeatCalendarEvent.End.AddDays(10);
            var scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, tileNewStart, tileNewEnd, calEventNewStart, calEventNewEnd, nonRigidRepeatCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            SubCalendarEvent testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            CalendarEvent repeatCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEventRetrieved.SubEvent_ID.getRepeatCalendarEventID(), user);

            Assert.AreEqual(repeatCalendarEventRetrieved.Start, calEventNewStart);
            Assert.AreEqual(repeatCalendarEventRetrieved.End, calEventNewEnd);


            CalendarEvent rootCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.SubEvent_ID.getCalendarEventID(), user);

            Assert.AreEqual(rootCalendarEventRetrieved.Start, calEventNewStart);
            Assert.AreEqual(rootCalendarEventRetrieved.End, calEventNewEnd);
        }


        /// <summary>
        /// This test verfies if the you have a repetition. And one of the repetition sequences is updated out side the root calendar event
        /// then the root calendarEvent needs to be extended too. This is using restricted cal event
        /// </summary>
        [TestMethod]
        public void DeadlineUpdateOutsideRepetitionRangeShouldUpdateRootCalendarEVent_Restricted_Repeat()
        {
            //DB_Schedule Schedule;
            DateTimeOffset refNow = TestUtility.parseAsUTC("8/21/2019 9:00 am +00:00");
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            var packet = TestUtility.CreatePacket();
            TilerUser tilerUser = packet.User;
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(5);


            TimeSpan timeSPanPerSubEvent = duration;
            Location location = TestUtility.getAdHocLocations()[1];
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TimeLine repeatTimeLine = new TimeLine(start, start.AddDays(21));
            TimeLine actualRangeTimeLine = new TimeLine(start, end);
            Repetition repeat = new Repetition(repeatTimeLine, Repetition.Frequency.WEEKLY, actualRangeTimeLine);
            RestrictionProfile restrictionProfile = new RestrictionProfile(start.Add(duration), TimeSpan.FromSeconds(duration.TotalSeconds * 4));
            DB_Schedule Schedule = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent nonRigidRepeatCalendarEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repeat, start, end, 5, false, restrictionProfile: restrictionProfile, now: Schedule.Now);

            Schedule.AddToScheduleAndCommitAsync(nonRigidRepeatCalendarEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            SubCalendarEvent testSubEvent = nonRigidRepeatCalendarEvent.ActiveSubEvents.First();
            DateTimeOffset tileNewStart = testSubEvent.Start;
            DateTimeOffset tileNewEnd = testSubEvent.End;
            DateTimeOffset calEventNewStart = nonRigidRepeatCalendarEvent.Start.AddDays(-1);
            DateTimeOffset calEventNewEnd = nonRigidRepeatCalendarEvent.End.AddDays(10);
            var scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, tileNewStart, tileNewEnd, calEventNewStart, calEventNewEnd, nonRigidRepeatCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            SubCalendarEvent testSubEventRetrieved = TestUtility.getSubEventById(testSubEvent.Id, user);
            CalendarEvent repeatCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEventRetrieved.SubEvent_ID.getRepeatCalendarEventID(), user);

            Assert.AreEqual(repeatCalendarEventRetrieved.Start, calEventNewStart);
            Assert.AreEqual(repeatCalendarEventRetrieved.End, calEventNewEnd);


            CalendarEvent rootCalendarEventRetrieved = TestUtility.getCalendarEventById(testSubEvent.SubEvent_ID.getCalendarEventID(), user);
            DateTimeOffset revisedStart = TestUtility.parseAsUTC("8/20/2019 10:00:00 AM +00:00");
            DateTimeOffset revisedEnd = TestUtility.parseAsUTC("9/20/2019 2:00:00 PM +00:00");

            Assert.AreEqual(rootCalendarEventRetrieved.Start, revisedStart);
            Assert.AreEqual(rootCalendarEventRetrieved.End, revisedEnd);
        }

        [TestMethod]
        public void RestrictedSubEventUpdate()
        {
            //DB_Schedule Schedule;
            DateTimeOffset iniRefNow = TestUtility.parseAsUTC("9:00 am");
            DateTimeOffset refNow = iniRefNow;
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            var packet = TestUtility.CreatePacket();
            TilerUser tilerUser = packet.User;
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(21);


            TimeSpan timeSPanPerSubEvent = duration;
            Location location = TestUtility.getAdHocLocations()[1];
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DB_Schedule Schedule = new TestSchedule(user, refNow, startOfDay);
            RestrictionProfile restrictionProfile = new RestrictionProfile(start.Add(duration), TimeSpan.FromSeconds(duration.TotalSeconds * 4));
            CalendarEvent nonResrictedCalendarEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 5, false, null, restrictionProfile, now: Schedule.Now);            
            Schedule.AddToScheduleAndCommitAsync(nonResrictedCalendarEvent).Wait();
            nonResrictedCalendarEvent = Schedule.getCalendarEvent(nonResrictedCalendarEvent.Id);


            SubCalendarEvent testSubEvent = nonResrictedCalendarEvent.ActiveSubEvents.OrderBy(sub => sub.Start).ToList()[2];// the ordeing is needed because you might get a sub event thats at the edge of the calendarEvent timeline. I selected index 2 just because ;)
            DateTimeOffset newStart = testSubEvent.Start.AddDays(10);
            DateTimeOffset newEnd = newStart.Add(duration);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            var scheduleUpdated = Schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, newStart, newEnd, nonResrictedCalendarEvent.Start, nonResrictedCalendarEvent.End, nonResrictedCalendarEvent.NumberOfSplit, testSubEvent.Notes.UserNote);
            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Task<SubCalendarEvent> subEentTask = user.ScheduleLogControl.getSubEventWithID(testSubEvent.Id);
            subEentTask.Wait();
            SubCalendarEvent subEvent = subEentTask.Result;
            Assert.IsTrue(subEvent.Start == newStart);
            Assert.IsTrue(subEvent.End == newEnd);
            Assert.IsTrue(subEvent.isLocked);

            List<TimeLine> timeLines = TestUtility.getTimeFrames(refNow, duration).GetRange(0, 10);
            foreach (TimeLine eachTimeLine in timeLines)
            {
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                DateTimeOffset TimeCreation = DateTimeOffset.UtcNow;
                CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), eachTimeLine.Start, eachTimeLine.End, 1, false);
                testEvent.TimeCreated = TimeCreation;
                Schedule = new TestSchedule(user, refNow);
                Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                string testEVentId = testEvent.getId;
                Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEVentId);
                waitVar.Wait();
                CalendarEvent newlyaddedevent = waitVar.Result;
                Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
                Assert.AreEqual(testEvent.TimeCreated, TimeCreation);
            }

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            subEentTask = user.ScheduleLogControl.getSubEventWithID(testSubEvent.Id);
            subEentTask.Wait();
            subEvent = subEentTask.Result;
            Assert.IsTrue(subEvent.Start == newStart);
            Assert.IsTrue(subEvent.End == newEnd);
            Assert.IsTrue(subEvent.isLocked);
        }

        /// <summary>
        /// Function runs an update on all possible types of tiler events
        /// </summary>
#if RunSlowTest
        [TestMethod]
#endif
        public void eventUpdateAllEventTypes()
        {

            string dateString = "9/13/2019 6:49:00 AM +00:00";
            DateTimeOffset iniRefNow;

            if (string.IsNullOrEmpty(dateString))
            {
                iniRefNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            } else
            {
                iniRefNow = DateTimeOffset.Parse(dateString);
            }

            var packet = TestUtility.CreatePacket();
            TilerUser tilerUser = packet.User;
            UserAccount user = packet.Account;
            DateTimeOffset start = iniRefNow;

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule schedule = new TestSchedule(user, iniRefNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            List<CalendarEvent> calEvents = TestUtility.generateAllCalendarEvent(schedule, duration, start, tilerUser, user, 10);
            List<SubCalendarEvent> allFirstActiveSubEvents = calEvents.Select(obj => obj.ActiveSubEvents.First()).ToList();
            DateTimeOffset refNow = iniRefNow;
            DateTimeOffset startOfDay = iniRefNow.AddHours(1);
            SubCalendarEvent previousSubEvent = null;
            string newNote = Guid.NewGuid().ToString();
            DateTimeOffset previousStart = Utility.BeginningOfTime;
            DateTimeOffset previousEnd = Utility.BeginningOfTime;
            string previousNote = "";
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow, startOfDay);
            foreach (SubCalendarEvent testSubEvent in allFirstActiveSubEvents)
            {
                DateTimeOffset newStart = testSubEvent.Start.AddDays(10);
                DateTimeOffset newEnd = newStart.Add(duration);
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                if (previousSubEvent != null)
                {
                    HashSet<string> calendarIds = new HashSet<string>() { previousSubEvent.getId, testSubEvent.getId };
                    schedule = new TestSchedule(user, refNow, startOfDay, calendarIds: calendarIds, includeUpdateHistory: true);
                    SubCalendarEvent previousInMemory = schedule.getSubCalendarEvent(previousSubEvent.Id);
                    previousInMemory.isTestEquivalent(previousSubEvent);
                    Assert.AreEqual(previousInMemory.Notes.UserNote, previousNote);
                    Assert.AreEqual(previousInMemory.Start, previousStart);
                    Assert.AreEqual(previousInMemory.End, previousEnd);

                } else
                {
                    HashSet<string> calendarIds = new HashSet<string>() { testSubEvent.getId };
                    schedule = new TestSchedule(user, refNow, startOfDay, calendarIds: calendarIds, includeUpdateHistory: true);
                }

                CalendarEvent calEVent = testSubEvent.ParentCalendarEvent;
                var scheduleUpdated = schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, newStart, newEnd, calEVent.Start, calEVent.End, calEVent.NumberOfSplit, newNote);
                previousSubEvent = schedule.getSubCalendarEvent(testSubEvent.getId);
                previousStart = newStart;
                previousEnd = newEnd;
                previousNote = newNote;
                schedule.persistToDB().Wait();
            }

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule schedule2Outlook = new TestSchedule(user, iniRefNow);
            schedule2Outlook.WriteFullScheduleToOutlook();
        }

#if RunSlowTest
        [TestMethod]
#endif
        public void eventUpdateTimeLineChange()
        {
            var packet = TestUtility.CreatePacket();
            TilerUser tilerUser = packet.User;
            UserAccount user = packet.Account;


            string dateString = "";
            DateTimeOffset iniRefNow;
            if (string.IsNullOrEmpty(dateString))
            {
                iniRefNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            }
            else
            {
                iniRefNow = DateTimeOffset.Parse(dateString);
            }

            DateTimeOffset start = iniRefNow;
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule schedule = new TestSchedule(user, iniRefNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            List<CalendarEvent> calEvents = TestUtility.generateAllCalendarEvent(schedule, duration, start, tilerUser, user, 10);
            foreach (CalendarEvent calEvent in calEvents)
            {
                Assert.AreEqual(calEvent.Start, start);
                Assert.AreEqual(calEvent.InitialStartTime, start);
            }
            List<SubCalendarEvent> allFirstActiveSubEvents = calEvents.Select(obj => obj.ActiveSubEvents.First()).ToList();
            Dictionary<string, TimeLine> calIdToInitialTimeLine = new Dictionary<string, TimeLine>();
            allFirstActiveSubEvents.ForEach((subEvent) =>
            {
                string calId = subEvent.ParentCalendarEvent.Id;
                TimeLine timeLine = subEvent.ParentCalendarEvent.StartToEnd;
                calIdToInitialTimeLine.Add(calId, timeLine);
            });
            DateTimeOffset refNow = iniRefNow;
            SubCalendarEvent previousSubEvent = null;
            DateTimeOffset previousStart = Utility.BeginningOfTime;
            DateTimeOffset previousEnd = Utility.BeginningOfTime;
            DateTimeOffset startOfDay = iniRefNow.AddHours(1);
            string previousNote = "";
            string newNote = Guid.NewGuid().ToString();
            foreach (SubCalendarEvent testSubEvent in allFirstActiveSubEvents)
            {
                DateTimeOffset newStart = testSubEvent.Start.AddDays(10);
                DateTimeOffset newEnd = newStart.Add(duration);
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                if (previousSubEvent != null)
                {
                    HashSet<string> calendarIds = new HashSet<string>() { previousSubEvent.getId, testSubEvent.getId };
                    schedule = new TestSchedule(user, refNow, startOfDay, calendarIds: calendarIds, includeUpdateHistory: true);
                    SubCalendarEvent previousInMemory = schedule.getSubCalendarEvent(previousSubEvent.Id);
                    previousInMemory.isTestEquivalent(previousSubEvent);
                    Assert.AreEqual(previousInMemory.Notes.UserNote, previousNote);
                    Assert.AreEqual(previousInMemory.Start, previousStart);
                    Assert.AreEqual(previousInMemory.End, previousEnd);

                    TimeLine initialTimeLine = previousInMemory.ParentCalendarEvent.InitialTimeLine;
                    TimeLine beforeEdit_iniTimeLine = calIdToInitialTimeLine[previousInMemory.ParentCalendarEvent.Id];
                    Assert.IsTrue(beforeEdit_iniTimeLine.isEqualStartAndEnd(initialTimeLine));
                    Assert.AreEqual(previousInMemory.ParentCalendarEvent.TimeLineHistory.TimeLines.Count, 1);// Since there's being only one update there has to be two timelines for each update
                }
                else
                {
                    HashSet<string> calendarIds = new HashSet<string>() { testSubEvent.getId };
                    schedule = new TestSchedule(user, refNow, startOfDay, calendarIds: calendarIds, includeUpdateHistory: true);
                }

                CalendarEvent calEVent = testSubEvent.ParentCalendarEvent;
                SubCalendarEvent subEventBeforeEdit = schedule.getSubCalendarEvent(testSubEvent.getId);
                Assert.AreEqual(subEventBeforeEdit.ParentCalendarEvent.TimeLineHistory.TimeLines.Count, 0);//this should be 1 because at the instantiation there should be one timeline update, in the list created at instantiation
                var scheduleUpdated = schedule.BundleChangeUpdate(testSubEvent.getId, testSubEvent.getName, newStart, newEnd, calEVent.Start.AddHours(1), calEVent.End.AddHours(1), calEVent.NumberOfSplit, newNote);
                previousSubEvent = schedule.getSubCalendarEvent(testSubEvent.getId);
                
                previousStart = newStart;
                previousEnd = newEnd;
                previousNote = newNote;
                schedule.persistToDB().Wait();
            }
        }

        [TestMethod]
        public void RepeatNonRigidSubEventUpdate()
        {
        }

        [TestMethod]
        public void RepeatRestrictedSubEventUpdate()
        {
        }





    }
}
