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
    public class SetEventAsNowTests
    {
        [TestMethod]
        public void setCalendarEventAsNow()
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
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB().Wait();
            testEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            Assert.IsTrue(testEvent.ActiveSubEvents.OrderBy(subevent => subevent.Start).First().Start == refNow);
        }
        [TestMethod]
        public void setCalendarEventAsNowPastDeadline()
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
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 2, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            DateTimeOffset newRefNow = end.AddDays(1);
            Schedule = new TestSchedule(user, newRefNow);
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB().Wait();
            testEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            SubCalendarEvent latesSubEvent = testEvent.ActiveSubEvents.OrderBy(subevent => subevent.Start).Last();
            Assert.IsTrue(latesSubEvent.Start == newRefNow);
            Assert.IsTrue(latesSubEvent.End == testEvent.End);
        }

        [TestMethod]
        public void setSubCalendarEventAsNow()
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
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 3, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents[1];
            Schedule = new TestSchedule(user, refNow);
            var setAsNowResult = Schedule.SetSubeventAsNow(subEvent.Id);
            Schedule.persistToDB().Wait();
            SubCalendarEvent testSubEvent = TestUtility.getSubEVentById(subEvent.getId, user);
            Assert.IsTrue(testSubEvent.Start == refNow);
        }
        [TestMethod]
        public void setSubCalendarEventAsNowPastDeadline()
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
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 3, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents[1];
            DateTimeOffset newRefNow = end.AddDays(1);
            Schedule = new TestSchedule(user, newRefNow);
            var setAsNowResult = Schedule.SetSubeventAsNow(subEvent.Id, true);
            Schedule.persistToDB().Wait();
            SubCalendarEvent testSubEvent = TestUtility.getSubEVentById(subEvent.getId, user);
            testEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            Assert.IsTrue(testSubEvent.Start == newRefNow);
            Assert.IsTrue(testSubEvent.End == testEvent.End);
            Assert.IsFalse(testEvent.End == end);
        }


        [TestMethod]
        public void discardSubCalendarEventAsNow()
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
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 3, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents[1];
            CalendarEvent beforeSetAsNowCalendarEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            DateTimeOffset newRefNow = end.AddDays(1);
            Schedule = new TestSchedule(user, newRefNow);
            var setAsNowResult = Schedule.SetSubeventAsNow(subEvent.Id, true);
            Schedule.persistToDB(false).Wait();
            SubCalendarEvent testSubEvent = TestUtility.getSubEVentById(subEvent.getId, user);
            CalendarEvent afterSetAsNowCalendarEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            Assert.IsTrue( testSubEvent.isTestEquivalent(subEvent));
            Assert.IsTrue(afterSetAsNowCalendarEvent.isTestEquivalent(beforeSetAsNowCalendarEvent));
        }


        [TestMethod]
        public void discardCalendarEventAsNow()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 3, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            CalendarEvent beforeSetAsNowCalendarEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            DateTimeOffset newRefNow = end.AddDays(1);
            Schedule = new TestSchedule(user, newRefNow);
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB(false).Wait();
            CalendarEvent afterSetAsNowCalendarEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            Assert.IsTrue(afterSetAsNowCalendarEvent.isTestEquivalent(beforeSetAsNowCalendarEvent));
        }
        [TestMethod]
        public void CalendarEventRestrictedAsNow()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition();
            TimeSpan fullDuration = duration + duration;
            var restrictionProfile = new RestrictionProfile(start, fullDuration);
            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, restrictionProfile: restrictionProfile, now: Schedule.Now) as CalendarEventRestricted;
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            DateTimeOffset newRefNow = end.AddHours(1);
            Schedule = new TestSchedule(user, newRefNow);
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB().Wait();
            CalendarEvent afterSetAsNowCalendarEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            SubCalendarEvent nowSubEvent = afterSetAsNowCalendarEvent.AllSubEvents.SingleOrDefault(subEvent => subEvent.Start == newRefNow);
            Assert.IsNotNull(nowSubEvent);
        }

        [TestMethod]
        public void SubCalendarEventRestrictedAsNow()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition();
            var restrictionProfile = new RestrictionProfile(start, duration + duration);
            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, restrictionProfile: restrictionProfile, now: Schedule.Now) as CalendarEventRestricted;
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            DateTimeOffset newRefNow = start.AddHours(1);
            Schedule = new TestSchedule(user, newRefNow);
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents.First();
            var setAsNowResult = Schedule.SetSubeventAsNow(subEvent.getId);
            Schedule.persistToDB().Wait();
            SubCalendarEvent subCalendarEventNow = TestUtility.getSubEVentById(subEvent.getId, user);
            Assert.IsTrue(subCalendarEventNow.Start == newRefNow);
        }

        [TestMethod]
        public void CalendarEventRestrictedAsNowOutsideRestrictedFrame()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DateTimeOffset end = repetitionRange.End.AddDays(14);
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition();
            TimeSpan fullDuration = duration + duration;
            var restrictionProfile = new RestrictionProfile(start.AddHours(2), fullDuration);
            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, restrictionProfile: restrictionProfile, now: Schedule.Now) as CalendarEventRestricted;
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            DateTimeOffset newRefNow = start.AddHours(1);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, newRefNow);
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB().Wait();
            CalendarEvent afterSetAsNowCalendarEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            SubCalendarEvent nowSubEvent = afterSetAsNowCalendarEvent.AllSubEvents.SingleOrDefault(subEvent => subEvent.Start == newRefNow);
            Assert.IsNotNull(nowSubEvent);
        }

        [TestMethod]
        public void SubsCalendarEventRestrictedAsNowOutsideRestrictedFrame()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DateTimeOffset end = repetitionRange.End.AddDays(14);
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition();
            TimeSpan fullDuration = duration + duration;
            var restrictionProfile = new RestrictionProfile(start.AddHours(2), fullDuration);
            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, restrictionProfile: restrictionProfile, now: Schedule.Now) as CalendarEventRestricted;
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            DateTimeOffset newRefNow = start.AddHours(1);
            Schedule = new TestSchedule(user, newRefNow);
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents.First();
            var setAsNowResult = Schedule.SetSubeventAsNow(subEvent.getId, true);
            Schedule.persistToDB().Wait();
            SubCalendarEvent subCalendarEventNow = TestUtility.getSubEVentById(subEvent.getId, user);
            Assert.IsTrue(subCalendarEventNow.Start == newRefNow);
        }
    }
}
