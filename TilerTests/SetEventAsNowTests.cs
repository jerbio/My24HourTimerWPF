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

            Schedule = new TestSchedule(user, refNow);
            TimeSpan procrastinationSpan = TimeSpan.FromHours(5);
            var procrastinateResult = Schedule.ProcrastinateJustAnEvent(testEvent.AllSubEvents.OrderBy(subEvent => subEvent.Start).First().getId, procrastinationSpan);
            Assert.IsNull(procrastinateResult.Item1);
            Schedule.persistToDB().Wait();
            Schedule = new TestSchedule(user, refNow);

            CalendarEvent testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            DateTimeOffset latestTime = refNow.Add(procrastinationSpan);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= latestTime);
            Assert.IsTrue(testEventCopy.getProcrastinationInfo.PreferredStartTime >= latestTime);
        }
        [TestMethod]
        public void setCalendarEventAsNowPastDeadline()
        {
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
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB().Wait();

            Schedule = new TestSchedule(user, refNow);
            TimeSpan procrastinationSpan = TimeSpan.FromHours(5);
            var procrassinateResult = Schedule.ProcrastinateJustAnEvent(testEvent.AllSubEvents.OrderBy(subEvent => subEvent.Start).First().getId, procrastinationSpan);
            Assert.IsNull(procrassinateResult.Item1);
            Schedule.persistToDB().Wait();
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            DateTimeOffset latestTime = refNow.Add(procrastinationSpan);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= latestTime);
        }
        [TestMethod]
        public void setSubCalendarEventAsNowPastDeadline()
        {
        }


        [TestMethod]
        public void discardSubCalendarEventAsNow()
        {
            
        }


        [TestMethod]
        public void discardCalendarEventAsNow()
        {

        }
    }
}
