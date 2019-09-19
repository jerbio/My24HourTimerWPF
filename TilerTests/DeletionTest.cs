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
    public class DeletionTest
    {
        [TestMethod]
        public void DeleteSubEvent()
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
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 2, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            Schedule = new TestSchedule(user, refNow);
            string deletedSubEventId = testEvent.AllSubEvents[0].getId;
            Schedule.deleteSubCalendarEvent(deletedSubEventId).Wait();
            Schedule.persistToDB().Wait();
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            Schedule = new TestSchedule(user, refNow);
            SubCalendarEvent subEvent = TestUtility.getSubEventById(deletedSubEventId, user);
            Assert.IsFalse(subEvent.isEnabled);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException), "The Calendar event given key was not present in the dictionary of loaded schedule.")]
        public void DeleteCalendarEvent()
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
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 2, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);

            Schedule = new TestSchedule(user, refNow);
            int beforeDeletionCount = Schedule.getAllCalendarEvents().Count();
            string deletedSubEventId = testEvent.AllSubEvents[0].getId;
            Schedule.deleteCalendarEventAndReadjust(deletedSubEventId).Wait();
            Schedule.persistToDB().Wait();

            EventID id = new EventID(deletedSubEventId);
            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(id.getRepeatCalendarEventID(), user);
            Assert.IsFalse(retrievedCalendarEvent.isEnabled);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            int afterDeletionCount = Schedule.getAllCalendarEvents().Count();
            Assert.AreEqual(beforeDeletionCount, afterDeletionCount + 1);
            CalendarEvent calEventLoadedIntoScheduleMemory = Schedule.getCalendarEvent(id);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            afterDeletionCount = Schedule.getAllCalendarEvents().Count();
            Assert.AreEqual(beforeDeletionCount, afterDeletionCount + 1);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestUtility.isSubCalendarEventUIEquivalenToScheduleLoaded(user, Schedule.Now);

            /////////////////Week day repeat/////////////////////////////////
            List<DayOfWeek> weekDays = new List<DayOfWeek>() { start.DayOfWeek, (DayOfWeek)(((int)start.DayOfWeek + 2) % 7), (DayOfWeek)(((int)start.DayOfWeek + 4) % 7) };
            List<DayOfWeek> weekDaysAsInt = weekDays.ToList();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TimeLine RepetitionTImeLine = new TimeLine(start, start.AddDays(21));
            TimeLine RepetitionActualTImeLine = new TimeLine(start, end);
            Repetition repetition = new Repetition(RepetitionTImeLine, Repetition.Frequency.WEEKLY, RepetitionActualTImeLine, weekDaysAsInt.ToArray());
            CalendarEvent repeatTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 2, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(repeatTestEvent).Wait();
            int beforeRepeatDeletionCount = Schedule.getAllCalendarEvents().Count();
            string DeletedRepeatSubEventId = repeatTestEvent.AllSubEvents[0].getId;

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            Schedule.deleteCalendarEventAndReadjust(repeatTestEvent.Id).Wait();
            Schedule.WriteFullScheduleToLog().Wait();



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            int afterRepeatDeletionCount = Schedule.getAllCalendarEvents().Count();
            Assert.AreEqual(beforeRepeatDeletionCount, afterRepeatDeletionCount + 1);
            EventID repeatEventId = new EventID(DeletedRepeatSubEventId);
            CalendarEvent retrievedRepeatCalendarEvent = TestUtility.getCalendarEventById(repeatEventId.getCalendarEventID(), user);
            Assert.IsFalse(retrievedRepeatCalendarEvent.isEnabled);
            TestUtility.isSubCalendarEventUIEquivalenToScheduleLoaded(user, Schedule.Now);


            ///////////////////////Repeat daily ////////////////

            repetition = new Repetition(RepetitionTImeLine, Repetition.Frequency.DAILY, RepetitionActualTImeLine);
            repeatTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 2, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(repeatTestEvent).Wait();
            beforeRepeatDeletionCount = Schedule.getAllCalendarEvents().Count();
            DeletedRepeatSubEventId = repeatTestEvent.AllSubEvents[0].getId;

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            Schedule.deleteCalendarEventAndReadjust(repeatTestEvent.Id).Wait();
            Schedule.WriteFullScheduleToLog().Wait();



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            afterRepeatDeletionCount = Schedule.getAllCalendarEvents().Count();
            Assert.AreEqual(beforeRepeatDeletionCount, afterRepeatDeletionCount + 1);
            repeatEventId = new EventID(DeletedRepeatSubEventId);
            retrievedRepeatCalendarEvent = TestUtility.getCalendarEventById(repeatEventId.getCalendarEventID(), user);
            Assert.IsFalse(retrievedRepeatCalendarEvent.isEnabled);
            TestUtility.isSubCalendarEventUIEquivalenToScheduleLoaded(user, Schedule.Now);




            //////////////////////////Weekday repeat rigid//////////////////////////////
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TimeLine RigidRepetitionTImeLine = new TimeLine(start, start.AddDays(21));
            TimeLine RigidRepetitionActualTImeLine = new TimeLine(start, start.AddHours(4));
            Repetition RigidRepetition = new Repetition(RigidRepetitionTImeLine, Repetition.Frequency.WEEKLY, RigidRepetitionActualTImeLine, weekDaysAsInt.ToArray());
            CalendarEvent RigidRepeatTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, RigidRepetition, start, RigidRepetitionActualTImeLine.End, 1, true);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(RigidRepeatTestEvent).Wait();
            int beforeRigidRepeatDeletionCount = Schedule.getAllCalendarEvents().Count();

            string DeletedRigidRepeatSubEventId = RigidRepeatTestEvent.AllSubEvents[0].getId;

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            Schedule.deleteCalendarEventAndReadjust(RigidRepeatTestEvent.Id).Wait();
            Schedule.WriteFullScheduleToLog().Wait();



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            int afterRigidRepeatDeletionCount = Schedule.getAllCalendarEvents().Count();
            Assert.AreEqual(beforeRigidRepeatDeletionCount, afterRigidRepeatDeletionCount + 1);
            EventID RigidRepeatEventId = new EventID(DeletedRigidRepeatSubEventId);
            CalendarEvent retrievedRigidRepeatCalendarEvent = TestUtility.getCalendarEventById(RigidRepeatEventId.getCalendarEventID(), user);
            Assert.IsFalse(retrievedRigidRepeatCalendarEvent.isEnabled);
            TestUtility.isSubCalendarEventUIEquivalenToScheduleLoaded(user, Schedule.Now);


            //////////////////////////Daily repeat rigid//////////////////////////////
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            RigidRepetitionTImeLine = new TimeLine(start, start.AddDays(21));
            RigidRepetitionActualTImeLine = new TimeLine(start, start.AddHours(4));
            RigidRepetition = new Repetition(RigidRepetitionTImeLine, Repetition.Frequency.DAILY, RigidRepetitionActualTImeLine);
            RigidRepeatTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, RigidRepetition, start, RigidRepetitionActualTImeLine.End, 1, true);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(RigidRepeatTestEvent).Wait();
            beforeRigidRepeatDeletionCount = Schedule.getAllCalendarEvents().Count();

            DeletedRigidRepeatSubEventId = RigidRepeatTestEvent.AllSubEvents[0].getId;

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            Schedule.deleteCalendarEventAndReadjust(RigidRepeatTestEvent.Id).Wait();
            Schedule.WriteFullScheduleToLog().Wait();



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            afterRigidRepeatDeletionCount = Schedule.getAllCalendarEvents().Count();
            Assert.AreEqual(beforeRigidRepeatDeletionCount, afterRigidRepeatDeletionCount + 1);
            RigidRepeatEventId = new EventID(DeletedRigidRepeatSubEventId);
            retrievedRigidRepeatCalendarEvent = TestUtility.getCalendarEventById(RigidRepeatEventId.getCalendarEventID(), user);
            Assert.IsFalse(retrievedRigidRepeatCalendarEvent.isEnabled);
            TestUtility.isSubCalendarEventUIEquivalenToScheduleLoaded(user, Schedule.Now);

        }

        [TestMethod]
        public void DeleteSubEventCount()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
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
            tilerUser = user.getTilerUser();
            TimeSpan duration0 = TimeSpan.FromHours(2);
            DateTimeOffset start0 = refNow;
            DateTimeOffset end0 = refNow.AddHours(7);
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser, duration0, new Repetition(), start0, end0, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent0).Wait();

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            user.Login().Wait();
            tilerUser = user.getTilerUser();
            TimeSpan duration1 = TimeSpan.FromHours(2);
            DateTimeOffset start1 = refNow;
            DateTimeOffset end1 = refNow.AddHours(7);
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, duration1, new Repetition(), start1, end1, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent1).Wait();

            Schedule = new TestSchedule(user, refNow);
            string deletedSubEventId = testEvent.AllSubEvents[0].getId;
            Schedule.deleteSubCalendarEvent(deletedSubEventId).Wait();
            Schedule.persistToDB().Wait();
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            Schedule = new TestSchedule(user, refNow);
            SubCalendarEvent subEvent = TestUtility.getSubEventById(deletedSubEventId, user);
            Assert.IsFalse(subEvent.isEnabled);
            CalendarEvent calendarEvent = TestUtility.getCalendarEventById(new EventID( deletedSubEventId).getCalendarEventID(), user);
            Assert.AreEqual(calendarEvent.DeletionCount, 1);

            // Running deletion on the same subEvent, we should get the same deletion count
            Schedule = new TestSchedule(user, refNow);
            Schedule.deleteSubCalendarEvent(deletedSubEventId).Wait();
            Schedule.persistToDB().Wait();
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            Schedule = new TestSchedule(user, refNow);
            subEvent = TestUtility.getSubEventById(deletedSubEventId, user);
            Assert.IsFalse(subEvent.isEnabled);
            calendarEvent = TestUtility.getCalendarEventById(new EventID(deletedSubEventId).getRepeatCalendarEventID(), user);
            Assert.AreEqual(calendarEvent.DeletionCount, 1);

        }

        [TestMethod]
        public void DeleteSubEventMultiple()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
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
            tilerUser = user.getTilerUser();
            TimeSpan duration0 = TimeSpan.FromHours(2);
            DateTimeOffset start0 = refNow;
            DateTimeOffset end0 = refNow.AddHours(7);
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser, duration0, new Repetition(), start0, end0, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent0).Wait();

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            user.Login().Wait();
            tilerUser = user.getTilerUser();
            TimeSpan duration1 = TimeSpan.FromHours(2);
            DateTimeOffset start1 = refNow;
            DateTimeOffset end1 = refNow.AddHours(7);
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, duration1, new Repetition(), start1, end1, numberOfSubEvent, false);
            Schedule.AddToScheduleAndCommit(testEvent1).Wait();

            Schedule = new TestSchedule(user, refNow);
            string deletedSubEventId = testEvent.AllSubEvents[0].getId;
            Schedule.deleteSubCalendarEvent(deletedSubEventId).Wait();
            Schedule.persistToDB().Wait();

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            Schedule = new TestSchedule(user, refNow);

            testEvent = TestUtility.getCalendarEventById(testEvent.ActiveSubEvents[0].getId, user);
            testEvent0 = TestUtility.getCalendarEventById(testEvent0.ActiveSubEvents[0].getId, user);
            testEvent1 = TestUtility.getCalendarEventById(testEvent1.ActiveSubEvents[0].getId, user);

            SubCalendarEvent testSubEvent = testEvent.ActiveSubEvents[0];
            SubCalendarEvent testSubEvent0 = testEvent0.ActiveSubEvents[0];
            SubCalendarEvent testSubEvent1 = testEvent1.ActiveSubEvents[0];
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            Schedule = new TestSchedule(user, refNow);
            Schedule.deleteSubCalendarEventAndReadjust(testSubEvent.getId).Wait();
            Schedule.persistToDB().Wait();
            //testEvent = TestUtility.getCalendarEventById(testEvent.Id, user);
            //testSubEvent = testEvent.ActiveSubEvents[0];
            testSubEvent = TestUtility.getSubEventById(testSubEvent.getId, user);
            List<EventID> subEventIds = new List<EventID>() { testSubEvent.SubEvent_ID, testSubEvent0.SubEvent_ID, testSubEvent1.SubEvent_ID };// This tries to redelete testSubEvent.SubEvent_ID and that should have no effect so the count should stay the same
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            Schedule = new TestSchedule(user, refNow);
            Schedule.deleteSubCalendarEvents(subEventIds.Select(subeventid => subeventid.ToString())).Wait();
            Schedule.persistToDB().Wait();

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            Schedule = new TestSchedule(user, refNow);
            testSubEvent = TestUtility.getSubEventById(testSubEvent.getId, user);
            testSubEvent0 = TestUtility.getSubEventById(testSubEvent0.getId, user);
            testSubEvent1 = TestUtility.getSubEventById(testSubEvent1.getId, user);
            Assert.IsTrue(testSubEvent.getIsDeleted);
            Assert.IsTrue(testSubEvent0.getIsDeleted);
            Assert.IsTrue(testSubEvent1.getIsDeleted);

            CalendarEvent retrievedCalendarEvent = TestUtility.getCalendarEventById(testSubEvent.getId, user);
            CalendarEvent retrievedCalendarEvent0 = TestUtility.getCalendarEventById(testSubEvent0.getId, user);
            CalendarEvent retrievedCalendarEvent1 = TestUtility.getCalendarEventById(testSubEvent1.getId, user);
            Assert.AreEqual(retrievedCalendarEvent.DeletionCount, 2);
            Assert.AreEqual(retrievedCalendarEvent0.DeletionCount, 1);
            Assert.AreEqual(retrievedCalendarEvent1.DeletionCount, 1);


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            testSubEvent = TestUtility.getSubEventById(testSubEvent.getId, user);
            testSubEvent0 = TestUtility.getSubEventById(testSubEvent0.getId, user);
            testSubEvent1 = TestUtility.getSubEventById(testSubEvent1.getId, user);
            Assert.IsTrue(testSubEvent.getIsDeleted);
            Assert.IsTrue(testSubEvent0.getIsDeleted);
            Assert.IsTrue(testSubEvent1.getIsDeleted);

            retrievedCalendarEvent = TestUtility.getCalendarEventById(testSubEvent.getId, user);
            retrievedCalendarEvent0 = TestUtility.getCalendarEventById(testSubEvent0.getId, user);
            retrievedCalendarEvent1 = TestUtility.getCalendarEventById(testSubEvent1.getId, user);

            Assert.AreEqual(retrievedCalendarEvent.DeletionCount, 2);
            Assert.AreEqual(retrievedCalendarEvent0.DeletionCount, 1);
            Assert.AreEqual(retrievedCalendarEvent1.DeletionCount, 1);
        }
    }
}
