using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScheduleAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;
using TilerFront;

namespace TilerTests
{
    [TestClass]
    public class AnalysisTest
    {
        [TestMethod]
        public void simpleRatioImplentation()
        {
            var packet = TestUtility.CreatePacket();
            TilerUser tilerUser = packet.User;
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");

            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            TimeLine calEventTimeLine = new TimeLine(refNow, refNow.AddDays(30));
            int eventsPerDay = 8;
            int totalSplit = eventsPerDay * (int)calEventTimeLine.TimelineSpan.TotalDays;
            TimeSpan durationPerEvent = TimeSpan.FromHours(480);
            CalendarEvent calEvent = TestUtility.generateCalendarEvent(tilerUser, durationPerEvent, null, calEventTimeLine.Start, calEventTimeLine.End, totalSplit);
            DB_Schedule Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommit(calEvent);


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, includeUpdateHistory: true);
            IEnumerable<SubCalendarEvent> activeSubEVents = Schedule.getAllActiveSubEvents();
            ScheduleSuggestionsAnalysis scheduleSuggestion = new ScheduleSuggestionsAnalysis(activeSubEVents, Schedule.Now, tilerUser);
            
            var overoccupiedTimelines = scheduleSuggestion.getOverLoadedWeeklyTimelines(refNow);
            var suggestion = scheduleSuggestion.suggestScheduleChange(overoccupiedTimelines);
            
            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = Schedule.getCalendarEvent(calEvent.Id);
            TimeSpan readjustedTimeSpan =  TimeSpan.FromHours(1600);
            DateTimeOffset minEndTime = refNow.Add(readjustedTimeSpan);
            Assert.IsTrue(retrievedCalendarEvent.DeadlineSuggestion > minEndTime);
        }
    }
}
