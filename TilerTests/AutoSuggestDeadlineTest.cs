using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerElements;
using TilerFront;

namespace TilerTests
{
    [TestClass]
    public class AutoSuggestDeadlineTest
    {
        [TestMethod]
        [ExpectedException(typeof(CustomErrors), "Tiler cannot confidently make prediction")]
        public void errorOnInsufficientData()
        {
            var packet = TestUtility.CreatePacket();
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;

            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromMinutes(30);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule Schedule = new TestSchedule(user, refNow);

            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddDays(10);
            Repetition repetition = new Repetition();

            int splitCount = 4;

            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, splitCount, false, restrictionProfile: new RestrictionProfile(start, duration + duration), now: Schedule.Now) as CalendarEventRestricted;
            Schedule.AddToScheduleAndCommit(testEvent);


            CalendarEventRestricted secondTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, splitCount, false, restrictionProfile: new RestrictionProfile(start, duration + duration), now: Schedule.Now) as CalendarEventRestricted;
            Schedule.TimeStone.projectedCompletionDate(secondTestEvent);
        }
    }
}
