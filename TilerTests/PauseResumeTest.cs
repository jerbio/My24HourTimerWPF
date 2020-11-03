using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerElements;
using TilerFront;
using static TilerTests.TestUtility;

namespace TilerTests
{
    [TestClass]
    public class PauseResumeTest
    {
        /// <summary>
        /// When a subevent is paused it should be the only paused subevent.
        /// It should also reset all previously paused events
        /// </summary>
        [TestMethod]
        public async Task PauseEvent()
        {
            Packet packet = CreatePacket();
            TilerUser tilerUser = packet.User;
            UserAccount user = getTestUser(userId: tilerUser.Id);
            reloadTilerUser(ref user, ref tilerUser);

            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            DateTimeOffset startOfDay = refNow;
            TimeLine calEventTimeLine = new TimeLine(refNow, refNow.AddDays(30));
            int eventsPerDay = 8;
            int totalSplit = eventsPerDay * (int)calEventTimeLine.TimelineSpan.TotalDays;
            TimeSpan durationPerEvent = TimeSpan.FromHours(210);
            CalendarEvent calEvent = TestUtility.generateCalendarEvent(tilerUser, durationPerEvent, null, calEventTimeLine.Start, calEventTimeLine.End, totalSplit);
            DB_Schedule schedule = new TestSchedule(user, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(calEvent);
            reloadTilerUser(ref user, ref tilerUser);

            CalendarEvent calEvent0 = TestUtility.generateCalendarEvent(tilerUser, durationPerEvent, null, calEventTimeLine.Start, calEventTimeLine.End, totalSplit);
            schedule = new TestSchedule(user, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(calEvent0);
            reloadTilerUser(ref user, ref tilerUser);

            schedule = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent calEventRetrieved = schedule.getCalendarEvent(calEvent.Id);
            CalendarEvent calEventRetrieved0 = schedule.getCalendarEvent(calEvent0.Id);

            SubCalendarEvent secondSubEvent = calEventRetrieved.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).ToList()[1];
            DateTimeOffset pausedRefNow = Utility.MiddleTime(secondSubEvent);
            schedule = new TestSchedule(user, pausedRefNow, startOfDay);
            Tuple<CustomErrors, SubCalendarEvent> pauseResult = schedule.PauseEvent().Result;
            Assert.AreEqual(pauseResult.Item2, secondSubEvent);
            throw new NotImplementedException("Still need to check for the pause time, check if previous paused are reset, check for multiple pauses per timeline");







        }

        /// <summary>
        /// When a subevent is resumed it should reset all paused parameters
        /// </summary>
        [TestMethod]
        public void ResumeEvent()
        {
        }

        /// <summary>
        /// When outside calendar range you should be able to resume event
        /// </summary>
        [TestMethod]
        public void ResumeEventEventOutSideCalendarLoadRange()
        {
        }
    }
}
