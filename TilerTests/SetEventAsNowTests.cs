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
            Assert.IsTrue(testEvent.ActiveSubEvents.OrderBy(subevent => subevent.Start).First().Start == newRefNow);

        }

        [TestMethod]
        public void setSubCalendarEventAsNow()
        {
            
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
