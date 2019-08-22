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
            TimeSpan timeSPanPerSubEvent = duration;
            Location location = TestUtility.getLocations()[1];
            CalendarEvent increaseSplitCountTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, false, location);
            schedule.AddToScheduleAndCommit(increaseSplitCountTestEvent).Wait();
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
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
            location = TestUtility.getLocations()[2];
            CalendarEvent decreaseSplitCountTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 3, false, location);
            schedule.AddToScheduleAndCommit(decreaseSplitCountTestEvent).Wait();
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            user.Login().Wait();
            tilerUser = user.getTilerUser();
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
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
            //DateTimeOffset refNow = DateTimeOffset.UtcNow;
            //DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            DateTimeOffset refNow = TestUtility.parseAsUTC("4/9/2019 9:00:00 PM +00:00");
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("4/9/2019 10:00:00 PM +00:00");
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
        [ExpectedException(typeof (CustomErrors), "The select time slot for the schedule change does is before the current time, try loading a schedule which includes the current time")]
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
            schedule.AddToScheduleAndCommit(testEvent).Wait();

            DateTimeOffset newStart = testEvent.Start.Add(-TimeSpan.FromTicks((long)duration.Ticks / 2));
            DateTimeOffset newDeadline = testEvent.End.Add(duration);
            refNow = newStart;
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
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
            newStart = newStart.Add(-duration);
            newDeadline = newDeadline.Add(-TimeSpan.FromTicks((long)(duration.Ticks * 2)));
            scheduleReloaded = new TestSchedule(user, refNow, startOfDay);
            scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.ActiveSubEvents.First().getId, testEvent.getName, newStart, 
                newDeadline, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);//this should throw an error because newStart is before refnow

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
            Location location = TestUtility.getLocations()[1];
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent nonRigidCalendarEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 5, false, location);
            DB_Schedule Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommit(nonRigidCalendarEvent).Wait();
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
            foreach (TimeLine eachTimeLine in timeLines)
            {
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                DateTimeOffset TimeCreation = DateTimeOffset.UtcNow;
                CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), eachTimeLine.Start, eachTimeLine.End, 1, false);
                testEvent.TimeCreated = TimeCreation;
                Schedule = new TestSchedule(user, refNow);
                Schedule.AddToScheduleAndCommit(testEvent).Wait();
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
