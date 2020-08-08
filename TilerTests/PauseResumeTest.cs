using System;
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
        public void PauseEvent()
        {
            Packet packet = TestUtility.CreatePacket();
            TilerUser tilerUser = packet.User;
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);

            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            DateTimeOffset startOfDay = refNow;
            TimeLine calEventTimeLine = new TimeLine(refNow, refNow.AddDays(30));
            int eventsPerDay = 8;
            int totalSplit = eventsPerDay * (int)calEventTimeLine.TimelineSpan.TotalDays;
            TimeSpan durationPerEvent = TimeSpan.FromHours(210);
            CalendarEvent calEvent = TestUtility.generateCalendarEvent(tilerUser, durationPerEvent, null, calEventTimeLine.Start, calEventTimeLine.End, totalSplit);
            DB_Schedule schedule = new TestSchedule(user, refNow, startOfDay);
            await schedule.AddToScheduleAndCommit(calEvent).configureAwait(false);
            




        }

        /// <summary>
        /// When a subevent is resumed it should reset all paused parameters
        /// </summary>
        [TestMethod]
        public void ResumeEvent()
        {
        }
    }
}
