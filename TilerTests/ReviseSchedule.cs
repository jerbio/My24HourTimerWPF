using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TilerElements;
using TilerFront;

namespace TilerTests
{
    [TestClass]
    public class ReviseSchedule
    {
        /// <summary>
        /// A schedule revision should move al events not scheduled
        /// </summary>
        [TestMethod]
        public async Task SimpleRevise()
        {
            var userPacket = TestUtility.CreatePacket();
            TilerUser tilerUser = userPacket.User;
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);

            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            DateTimeOffset startOfDay = refNow;
            TimeLine calEventTimeLine = new TimeLine(refNow, refNow.AddDays(30));
            int eventsPerDay = 8;
            int totalSplit = eventsPerDay * (int)calEventTimeLine.TimelineSpan.TotalDays;
            TimeSpan totalEventDuration = TimeSpan.FromHours(210);
            CalendarEvent calEvent0 = TestUtility.generateCalendarEvent(tilerUser, totalEventDuration, null, calEventTimeLine.Start, calEventTimeLine.End, totalSplit);
            DB_Schedule Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommit(calEvent0);
            
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent calEvent1 = TestUtility.generateCalendarEvent(tilerUser, totalEventDuration, null, calEventTimeLine.Start, calEventTimeLine.End, totalSplit);
            Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommit(calEvent1);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent calEvent2 = TestUtility.generateCalendarEvent(tilerUser, totalEventDuration, null, calEventTimeLine.Start, calEventTimeLine.End, totalSplit);
            Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommit(calEvent2);


            DateTimeOffset nextDayRefNow = refNow.AddDays(3.5);
            DateTimeOffset precedingTwentyFourHoursRefNow = nextDayRefNow.AddDays(-1);

            TimeLine precedingTwentyHoursTimeline = new TimeLine(precedingTwentyFourHoursRefNow, nextDayRefNow);
            List<SubCalendarEvent> subeventsInFirstTwentyFourHours = Schedule.getInterferringSubEvents(precedingTwentyHoursTimeline, Schedule.getAllActiveSubEvents()).ToList();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, nextDayRefNow, startOfDay);
            await Schedule.FindMeSomethingToDo(new Location()).ConfigureAwait(false);
            List<SubCalendarEvent> subeventsInFirstTwentyFourHoursAfterShuffle = Schedule.getInterferringSubEvents(precedingTwentyHoursTimeline, Schedule.getAllActiveSubEvents()).ToList();
            Assert.IsTrue(subeventsInFirstTwentyFourHoursAfterShuffle.Count > 0);
            await Schedule.persistToDB(false).ConfigureAwait(false);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, nextDayRefNow, startOfDay);
            await Schedule.reviseSchedule(new Location()).ConfigureAwait(false);
            List<SubCalendarEvent> subeventsInFirstTwentyFourHoursAfterRevise = Schedule.getInterferringSubEvents(precedingTwentyHoursTimeline, Schedule.getAllActiveSubEvents()).ToList();
            Assert.AreEqual(subeventsInFirstTwentyFourHoursAfterRevise.Count, 0, "After revising schedule there should be no tiles before now");
        }
    }
}
