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
        /// <summary>
        /// Test creates an event that doesn't cross the default deadline thresh hold
        /// </summary>
        [TestMethod]
        public void noDeadlineExtensionNeeded()
        {
            var packet = TestUtility.CreatePacket();
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
            DB_Schedule Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommit(calEvent);


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            IEnumerable<SubCalendarEvent> activeSubEVents = Schedule.getAllActiveSubEvents();
            ScheduleSuggestionsAnalysis scheduleSuggestion = new ScheduleSuggestionsAnalysis(activeSubEVents, Schedule.Now, tilerUser, Schedule.Analysis);

            var overoccupiedTimelines = scheduleSuggestion.getOverLoadedWeeklyTimelines(refNow);
            var suggestion = scheduleSuggestion.suggestScheduleChange(overoccupiedTimelines);

            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = Schedule.getCalendarEvent(calEvent.Id);
            TimeSpan readjustedTimeSpan = TimeSpan.FromHours(1600);
            DateTimeOffset minEndTime = refNow.Add(readjustedTimeSpan);
            Assert.IsTrue(retrievedCalendarEvent.DeadlineSuggestion.isBeginningOfJsTime());
        }


        /// <summary>
        /// Creates a schedule that is overscheduled and a deadline update is needed
        /// </summary>
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
            Schedule = new TestSchedule(user, refNow, startOfDay, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            IEnumerable<SubCalendarEvent> activeSubEVents = Schedule.getAllActiveSubEvents();
            ScheduleSuggestionsAnalysis scheduleSuggestion = new ScheduleSuggestionsAnalysis(activeSubEVents, Schedule.Now, tilerUser, Schedule.Analysis);

            var overoccupiedTimelines = scheduleSuggestion.getOverLoadedWeeklyTimelines(refNow);
            var suggestion = scheduleSuggestion.suggestScheduleChange(overoccupiedTimelines);

            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = Schedule.getCalendarEvent(calEvent.Id);
            TimeSpan readjustedTimeSpan = TimeSpan.FromHours(1600);
            DateTimeOffset minEndTime = refNow.Add(readjustedTimeSpan);
            Assert.IsTrue(retrievedCalendarEvent.DeadlineSuggestion > minEndTime);
        }

        /// <summary>
        /// Test creates two (basically equal )calendar events. Each calendar event isn't sufficient to break the default occupancy limit.
        /// However with both schedules this creates an overschedule calendar. So only one of the events has to be picked to be rescheduled.
        /// </summary>
        [TestMethod]
        public async Task onlyOneCalEventAutoSuggestedDeadlineExtensionNeeded()
        {
            var packet = TestUtility.CreatePacket();
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
            DB_Schedule Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommit(calEvent);


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            IEnumerable<SubCalendarEvent> activeSubEVents = Schedule.getAllActiveSubEvents();
            ScheduleSuggestionsAnalysis scheduleSuggestion = new ScheduleSuggestionsAnalysis(activeSubEVents, Schedule.Now, tilerUser, Schedule.Analysis);

            var overoccupiedTimelines = scheduleSuggestion.getOverLoadedWeeklyTimelines(refNow);
            var suggestion = scheduleSuggestion.suggestScheduleChange(overoccupiedTimelines);

            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = Schedule.getCalendarEvent(calEvent.Id);
            TimeSpan readjustedTimeSpan = TimeSpan.FromHours(1600);
            DateTimeOffset minEndTime = refNow.Add(readjustedTimeSpan);
            Assert.IsTrue(retrievedCalendarEvent.DeadlineSuggestion.isBeginningOfJsTime());

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent secondCalEvent = TestUtility.generateCalendarEvent(tilerUser, durationPerEvent, null, calEventTimeLine.Start, calEventTimeLine.End, totalSplit);
            Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommit(secondCalEvent);

            //Need to Shuffle because of preceding schedule containing initial subevents
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            await Schedule.FindMeSomethingToDo(new Location()).ConfigureAwait(false);
            await Schedule.persistToDB().ConfigureAwait(false);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            IEnumerable<SubCalendarEvent> activeAfterSecondSubEvents = Schedule.getAllActiveSubEvents();
            scheduleSuggestion = new ScheduleSuggestionsAnalysis(activeAfterSecondSubEvents, Schedule.Now, tilerUser, Schedule.Analysis);

            var overoccupiedTimelinesAfterSecondSubEvent = scheduleSuggestion.getOverLoadedWeeklyTimelines(refNow);
            var suggestionAfterSecondSubEvent = scheduleSuggestion.suggestScheduleChange(overoccupiedTimelinesAfterSecondSubEvent);
            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);

            CalendarEvent retrievedFirstCalendarEvent = Schedule.getCalendarEvent(calEvent.Id);
            CalendarEvent retrievedSecondCalendarEvent = Schedule.getCalendarEvent(secondCalEvent.Id);
            Assert.IsFalse(retrievedFirstCalendarEvent.DeadlineSuggestion.isBeginningOfJsTime() && retrievedSecondCalendarEvent.DeadlineSuggestion.isBeginningOfJsTime());
            Assert.IsTrue(retrievedFirstCalendarEvent.DeadlineSuggestion.isBeginningOfJsTime() || retrievedSecondCalendarEvent.DeadlineSuggestion.isBeginningOfJsTime());
        }

        /// <summary>
        /// Test verifies that the cal event with the a higher change history count has the deadline suggestion
        /// </summary>
        [TestMethod]
        public async Task longerChangeHistory_is_CalEventNeededForAutoSuggestedDeadlineExtension()
        {
            var packet = TestUtility.CreatePacket();
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
            DB_Schedule Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommit(calEvent);


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            IEnumerable<SubCalendarEvent> activeSubEVents = Schedule.getAllActiveSubEvents();
            ScheduleSuggestionsAnalysis scheduleSuggestion = new ScheduleSuggestionsAnalysis(activeSubEVents, Schedule.Now, tilerUser, Schedule.Analysis);

            var overoccupiedTimelines = scheduleSuggestion.getOverLoadedWeeklyTimelines(refNow);
            var suggestion = scheduleSuggestion.suggestScheduleChange(overoccupiedTimelines);

            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent retrievedCalendarEvent = Schedule.getCalendarEvent(calEvent.Id);
            TimeSpan readjustedTimeSpan = TimeSpan.FromHours(1600);
            DateTimeOffset minEndTime = refNow.Add(readjustedTimeSpan);
            Assert.IsTrue(retrievedCalendarEvent.DeadlineSuggestion.isBeginningOfJsTime());

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent secondCalEvent = TestUtility.generateCalendarEvent(tilerUser, durationPerEvent, null, calEventTimeLine.Start, calEventTimeLine.End, totalSplit);
            Schedule = new TestSchedule(user, refNow, startOfDay);
            Schedule.AddToScheduleAndCommit(secondCalEvent);

            //Need to Shuffle because of preceding schedule containing initial subevents
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            await Schedule.FindMeSomethingToDo(new Location()).ConfigureAwait(false);
            await Schedule.persistToDB().ConfigureAwait(false);

            // Update second calevent deadline
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            SubCalendarEvent subEvent = secondCalEvent.ActiveSubEvents.First();
            DateTimeOffset deadline = secondCalEvent.End.AddDays(1);
            Schedule.BundleChangeUpdate(subEvent.Id, subEvent.getName, subEvent.Start, subEvent.End, secondCalEvent.Start, deadline, secondCalEvent.NumberOfSplit, secondCalEvent.Notes.UserNote);
            await Schedule.persistToDB().ConfigureAwait(false);


            IEnumerable<SubCalendarEvent> activeAfterSecondSubEvents = Schedule.getAllActiveSubEvents();
            scheduleSuggestion = new ScheduleSuggestionsAnalysis(activeAfterSecondSubEvents, Schedule.Now, tilerUser, Schedule.Analysis);

            var overoccupiedTimelinesAfterSecondSubEvent = scheduleSuggestion.getOverLoadedWeeklyTimelines(refNow);
            var suggestionAfterSecondSubEvent = scheduleSuggestion.suggestScheduleChange(overoccupiedTimelinesAfterSecondSubEvent);
            Schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, startOfDay, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);

            CalendarEvent retrievedFirstCalendarEvent = Schedule.getCalendarEvent(calEvent.Id);
            CalendarEvent retrievedSecondCalendarEvent = Schedule.getCalendarEvent(secondCalEvent.Id);
            Assert.IsFalse(retrievedSecondCalendarEvent.DeadlineSuggestion.isBeginningOfJsTime());
            Assert.IsTrue(retrievedFirstCalendarEvent.DeadlineSuggestion.isBeginningOfJsTime());
        }
    }
}
