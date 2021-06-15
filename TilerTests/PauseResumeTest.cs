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

            Tuple<CustomErrors, SubCalendarEvent> preCurrentSubEvents = schedule.PauseEvent().Result;
            Assert.AreEqual(preCurrentSubEvents.Item1.Code, (int)CustomErrors.Errors.Pause_Event_There_Is_No_Current_To_Pause);
            


            SubCalendarEvent pausedSubEvent = calEventRetrieved.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).ToList()[1];
            DateTimeOffset pausedRefNow = Utility.MiddleTime(pausedSubEvent);
            schedule = new TestSchedule(user, pausedRefNow, startOfDay);
            CustomErrors resumeError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(resumeError.Code,
                (int)CustomErrors.Errors.Resume_Event_Paused_Event_Id_is_Null, 
                "Resume should return error because of no current paused events");

            Tuple<CustomErrors, SubCalendarEvent> pauseResult = schedule.PauseEvent().Result;
            Assert.AreEqual(pauseResult.Item2, pausedSubEvent);
            await schedule.persistToDB().ConfigureAwait(false);
            reloadTilerUser(ref user, ref tilerUser);
            Assert.AreEqual(
                tilerUser.PausedEventId.ToString(),
                pausedSubEvent.Id);


            DateTimeOffset nextRefNow = pausedRefNow.AddDays(1);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);

            SubCalendarEvent pausedSubEventRetrived = schedule.getSubCalendarEvent(tilerUser.PausedEventId);
            Assert.IsTrue(pausedSubEventRetrived.isPaused);
        }

        [TestMethod]
        public async Task PauseEventLoadXml()
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

            Tuple<CustomErrors, SubCalendarEvent> preCurrentSubEvents = schedule.PauseEvent().Result;
            Assert.AreEqual(preCurrentSubEvents.Item1.Code, (int)CustomErrors.Errors.Pause_Event_There_Is_No_Current_To_Pause);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow, startOfDay);

            var scheduleDumpask0 = schedule.CreateScheduleDump(schedule.Now);
            scheduleDumpask0.Wait();
            var scheduleDump0 = scheduleDumpask0.Result;
            TestSchedule scheduleDumpSchedule0 = new TestSchedule(scheduleDump0, user);
            TestUtility.isTestEquivalent(schedule, scheduleDumpSchedule0);


            reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset halfRefNow = Utility.MiddleTime(schedule.getSubCalendarEvent(calEventRetrieved.AllSubEvents.First().Id));
            schedule = new TestSchedule(user, halfRefNow, startOfDay);
            Tuple<CustomErrors, SubCalendarEvent> postCurrentSubEvents = schedule.PauseEvent().Result;
            await schedule.persistToDB().ConfigureAwait(false);
            var scheduleDumpask1 = schedule.CreateScheduleDump(schedule.Now);

            var scheduleDump1 = scheduleDumpask1.Result;
            TestSchedule scheduleDumpSchedule1 = new TestSchedule(scheduleDump1, user);
            TestUtility.isTestEquivalent(schedule, scheduleDumpSchedule1);


            reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset oneHourRefNow = Utility.MiddleTime(schedule.getSubCalendarEvent(calEventRetrieved.AllSubEvents.First().Id)).AddHours(1);
            schedule = new TestSchedule(user, oneHourRefNow, startOfDay);
            await schedule.ResumeEvent().ConfigureAwait(false);
            await schedule.persistToDB().ConfigureAwait(false);
            var scheduleDumpask2 = schedule.CreateScheduleDump(schedule.Now);
            var scheduleDump2 = scheduleDumpask2.Result;
            TestSchedule scheduleDumpSchedule2 = new TestSchedule(scheduleDump2, user);
            TestUtility.isTestEquivalent(schedule, scheduleDumpSchedule2);
        }

        /// <summary>
        /// When a subevent is resumed it should reset all paused parameters
        /// </summary>
        [TestMethod]
        public async Task ResumeEvent()
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

            SubCalendarEvent pausedSubEvent = calEventRetrieved.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).ToList()[1];
            DateTimeOffset pausedRefNow = Utility.MiddleTime(pausedSubEvent);
            TimeLine exhaustedTimeLine = new TimeLine(pausedSubEvent.Start, pausedRefNow);
            schedule = new TestSchedule(user, pausedRefNow, startOfDay);
            CustomErrors resumeError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(resumeError.Code,
                (int)CustomErrors.Errors.Resume_Event_Paused_Event_Id_is_Null,
                "Resume should return error because of no current paused events");

            Tuple<CustomErrors, SubCalendarEvent> pauseResult = schedule.PauseEvent().Result;
            Assert.AreEqual(pauseResult.Item2, pausedSubEvent);
            await schedule.persistToDB().ConfigureAwait(false);
            
            
            reloadTilerUser(ref user, ref tilerUser);
            Assert.AreEqual(
                tilerUser.PausedEventId.ToString(),
                pausedSubEvent.Id);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, pausedRefNow.AddHours(1), startOfDay);
            CustomErrors resumeNonError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.IsNull(resumeNonError, "There should be no errors, after pausing");
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset nextRefNow = pausedRefNow.AddDays(1);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);




            TimeLine timeLineOfEvents = new TimeLine(nextRefNow, nextRefNow.AddDays(1));
            SubCalendarEvent firstSubEventOfNextDay = schedule.getNearestEventToNow().Item2;
            schedule.SetSubeventAsNow(firstSubEventOfNextDay.Id);
            await schedule.persistToDB().ConfigureAwait(false);

            SubCalendarEvent futurePausedSubEvent = schedule.getSubCalendarEvent(firstSubEventOfNextDay.Id);


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);
            SubCalendarEvent notPausedSubEvent = schedule
                .getCalendarEvent(pausedSubEvent.getTilerID)
                .ActiveSubEvents.Where(subEvent => subEvent.Id != pausedSubEvent.Id)
                .FirstOrDefault();
            EventID pausedEventId = schedule.User.PausedEventId;
            Assert.IsNull(pausedEventId);
            Assert.IsFalse(notPausedSubEvent.isPaused);

            CustomErrors resumeNotPausedSubEventResult = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(
                resumeNotPausedSubEventResult.Code,
                (int)CustomErrors.Errors.Resume_Event_Paused_Event_Id_is_Null,
                "Resume should return error because of no current paused events, because after set as now the time locks should be cleared"
                );



            reloadTilerUser(ref user, ref tilerUser);
            pausedRefNow = Utility.MiddleTime(futurePausedSubEvent);
            exhaustedTimeLine = new TimeLine(futurePausedSubEvent.Start, pausedRefNow);
            schedule = new TestSchedule(user, pausedRefNow, startOfDay);
            Tuple<CustomErrors, SubCalendarEvent> pauseResultAfterSetAsNow = schedule
                .PauseEvent().Result;
            pausedSubEvent = futurePausedSubEvent;// the set as now sub event should be the pausable subEvent
            Assert.AreEqual(pauseResultAfterSetAsNow.Item2.Id, pausedSubEvent.Id);
            pausedEventId = pausedSubEvent.SubEvent_ID;
            await schedule.persistToDB().ConfigureAwait(false);
            CustomErrors resumeResult = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.IsNull(resumeResult, "There should be no errors because of successful resume");

            SubCalendarEvent resumeSubEvent = schedule.getSubCalendarEvent(pausedEventId);
            Assert.AreEqual(resumeSubEvent.ParentCalendarEvent.ActivePausedTimeLines.Count, 0);
            Assert.AreEqual(resumeSubEvent.ParentCalendarEvent.PausedTimeLines.Count, 1);
            Assert.IsFalse(resumeSubEvent.isPaused);
            Assert.IsTrue(resumeSubEvent.isPausedLocked);

            Assert.AreEqual(resumeSubEvent.Start, schedule.Now.constNow);
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);
            SubCalendarEvent retrievedResumeSubEvent = schedule.getSubCalendarEvent(pausedEventId);
            Assert.AreEqual(retrievedResumeSubEvent.ParentCalendarEvent.ActivePausedTimeLines.Count, 0);
            Assert.AreEqual(retrievedResumeSubEvent.ParentCalendarEvent.PausedTimeLines.Count, 1);
            Assert.IsTrue(exhaustedTimeLine.isTestEquivalent(retrievedResumeSubEvent.ParentCalendarEvent.PausedTimeLines[0]));
        }

        /// <summary>
        /// When outside calendar range you should be able to resume event
        /// </summary>
        [TestMethod]
        public async Task ResumeEventEventOutSideCalendarLoadRange()
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

            SubCalendarEvent pausedSubEvent = calEventRetrieved.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).ToList()[1];
            DateTimeOffset pausedRefNow = Utility.MiddleTime(pausedSubEvent);
            TimeLine exhaustedTimeLine = new TimeLine(pausedSubEvent.Start, pausedRefNow);
            schedule = new TestSchedule(user, pausedRefNow, startOfDay);
            CustomErrors resumeError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(resumeError.Code,
                (int)CustomErrors.Errors.Resume_Event_Paused_Event_Id_is_Null,
                "Resume should return error because of no current paused events");

            Tuple<CustomErrors, SubCalendarEvent> pauseResult = schedule.PauseEvent().Result;
            Assert.AreEqual(pauseResult.Item2, pausedSubEvent);
            await schedule.persistToDB().ConfigureAwait(false);
            reloadTilerUser(ref user, ref tilerUser);
            Assert.AreEqual(
                tilerUser.PausedEventId.ToString(),
                pausedSubEvent.Id);


            DateTimeOffset afterPausedEventDeadline = calEventRetrieved.End.AddDays(5);
            schedule = new TestSchedule(user, afterPausedEventDeadline, startOfDay);

            TimeLine timeLineOfEvents = new TimeLine(afterPausedEventDeadline, afterPausedEventDeadline.AddDays(1));
            //SubCalendarEvent firstSubEventOfNextDay = schedule.getNearestEventToNow().Item2;
            //schedule.SetSubeventAsNow(firstSubEventOfNextDay.Id);
            //await schedule.persistToDB().ConfigureAwait(false);


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, afterPausedEventDeadline, startOfDay);
            SubCalendarEvent notPausedSubEvent = schedule.getCalendarEvent(pausedSubEvent.getTilerID).ActiveSubEvents.Where(subEvent => subEvent.Id != pausedSubEvent.Id).FirstOrDefault();
            SubCalendarEvent pausedSubEventRetrieved = schedule.getSubCalendarEvent(schedule.User.PausedEventId);
            EventID pausedEventId = schedule.User.PausedEventId;
            Assert.AreEqual(pausedEventId.ToString(), pausedSubEvent.Id);

            CustomErrors resumeResult = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(resumeResult.Code, (int)CustomErrors.Errors.Resume_Event_Cannot_Outside_Deadline_Of_CalendarEvent, "Resuming outside range, should be forced");
            await schedule.persistToDB().ConfigureAwait(false);


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, afterPausedEventDeadline, startOfDay);
            CustomErrors forceResumeResult = await schedule.ResumeEvent(forceOutsideDeadline: true).ConfigureAwait(false);
            Assert.IsNull(forceResumeResult, "There should be no errors because of successful resume");

            SubCalendarEvent resumeSubEvent = schedule.getSubCalendarEvent(pausedEventId);
            Assert.AreEqual(resumeSubEvent.ParentCalendarEvent.PausedTimeLines.Count, 1);

            Assert.AreEqual(resumeSubEvent.Start, schedule.Now.constNow);
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, afterPausedEventDeadline, startOfDay);
            SubCalendarEvent retrievedResumeSubEvent = schedule.getSubCalendarEvent(pausedEventId);
            Assert.AreEqual(retrievedResumeSubEvent.ParentCalendarEvent.PausedTimeLines.Count, 1);// THis is known to fail because we of the reordering of tiles based on ids. This would mean moving the paused tile to an earlier time frame outside the schedlue retrieval window. We need to make the pause timeline part of the calendar event and not Sub calendar event
            Assert.IsTrue(exhaustedTimeLine.isTestEquivalent(retrievedResumeSubEvent.ParentCalendarEvent.PausedTimeLines[0]));
        }

        /// <summary>
        /// When a sub event is paused and resumed multiple times the sub event should have different timelines with different paused events
        /// </summary>
        [TestMethod]
        public async Task MultiplePauseTImeLines()
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

            Tuple<CustomErrors, SubCalendarEvent> preCurrentSubEvents = schedule.PauseEvent().Result;
            Assert.AreEqual(preCurrentSubEvents.Item1.Code, (int)CustomErrors.Errors.Pause_Event_There_Is_No_Current_To_Pause);



            SubCalendarEvent pausedSubEvent = calEventRetrieved.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).ToList()[1];
            TimeSpan intialDuration = pausedSubEvent.getActiveDuration;
            DateTimeOffset pausedRefNow = Utility.MiddleTime(pausedSubEvent);
            TimeSpan durationSplit = TimeSpan.FromTicks(intialDuration.Ticks / 2);
            schedule = new TestSchedule(user, pausedRefNow, startOfDay);
            CustomErrors resumeError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(resumeError.Code,
                (int)CustomErrors.Errors.Resume_Event_Paused_Event_Id_is_Null,
                "Resume should return error because of no current paused events");

            Tuple<CustomErrors, SubCalendarEvent> pauseResult = schedule.PauseEvent().Result;
            Assert.AreEqual(pauseResult.Item2, pausedSubEvent);
            await schedule.persistToDB().ConfigureAwait(false);
            reloadTilerUser(ref user, ref tilerUser);
            Assert.AreEqual(
                tilerUser.PausedEventId.ToString(),
                pausedSubEvent.Id);


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, pausedRefNow.AddHours(1), startOfDay);
            CustomErrors resumeNonError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.IsNull(resumeNonError, "There should be no errors, since we resume after pausing");

            DateTimeOffset nextRefNow = pausedRefNow.AddDays(1);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);
            SubCalendarEvent pausedSubEventScheduleMemory= schedule.getSubCalendarEvent(pausedSubEvent.Id);
            Assert.AreEqual((int)pausedSubEventScheduleMemory.StartToEnd.TimelineSpan.TotalMinutes, (int)durationSplit.TotalMinutes);// we are approximating to int because of time calculations are to the minute resolution

            Assert.IsNull(tilerUser.PausedEventId);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent calEvent1 = TestUtility.generateCalendarEvent(tilerUser, durationPerEvent, null, calEventTimeLine.Start, calEventTimeLine.End, totalSplit);
            schedule.AddToScheduleAndCommit(calEvent1);
            
            reloadTilerUser(ref user, ref tilerUser);

            SubCalendarEvent pausedSubEvent1 = calEvent1.ActiveSubEvents.First();

            
            DateTimeOffset quarterRefNow1 = Utility.MiddleTime(new TimeLine(pausedSubEvent1.Start, Utility.MiddleTime(pausedSubEvent1)));
            TimeSpan quarterTimeSpan = quarterRefNow1 - pausedSubEvent1.Start;
            schedule = new TestSchedule(user, quarterRefNow1, startOfDay);
            Tuple<CustomErrors, SubCalendarEvent> quarterPauseResult = schedule.PauseEvent().Result;
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, quarterRefNow1, startOfDay);
            CalendarEvent calEventRetrieved1 = schedule.getCalendarEvent(pausedSubEvent1.Id);
            Assert.AreEqual(calEventRetrieved1.ActivePausedTimeLines.Count, 1, "Since we haven't resumed there should be one active pause");
            Assert.AreEqual(calEventRetrieved1.PausedTimeLines.Count, 1);

            // Moving forward by a quarter timespan
            DateTimeOffset quarterRefNow2 = quarterRefNow1.Add(quarterTimeSpan);
            schedule = new TestSchedule(user, quarterRefNow2, startOfDay);
            await schedule.ResumeEvent().ConfigureAwait(false);
            await schedule.persistToDB().ConfigureAwait(false);
            calEventRetrieved1 = schedule.getCalendarEvent(pausedSubEvent1.Id);
            Assert.AreEqual(calEventRetrieved1.ActivePausedTimeLines.Count, 0, "Since we have resumed there should be no active pause");
            Assert.AreEqual(calEventRetrieved1.PausedTimeLines.Count, 1);

            //Moving to the halfway time frame
            reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset quarterRefNow3 = quarterRefNow2.Add(quarterTimeSpan);

            schedule = new TestSchedule(user, quarterRefNow3, startOfDay);
            Tuple<CustomErrors, SubCalendarEvent> halfPauseResult = schedule.PauseEvent().Result;// this is because we should have used up half the active time of tile
            SubCalendarEvent halfPauseSubEvent = halfPauseResult.Item2;
            await schedule.persistToDB().ConfigureAwait(false);
            CalendarEvent halfCalEventRetrieved1 = schedule.getCalendarEvent(halfPauseSubEvent.Id);
            Assert.AreEqual(halfCalEventRetrieved1.Id, halfPauseSubEvent.CalendarEventId);
            Assert.AreEqual(halfCalEventRetrieved1.ActivePausedTimeLines.Count, 1, "Since we haven't resumed the second paused instance");
            Assert.AreEqual(halfCalEventRetrieved1.PausedTimeLines.Count, 2);



            reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset quarterRefNow4 = quarterRefNow3.Add(quarterTimeSpan);
            schedule = new TestSchedule(user, quarterRefNow4, startOfDay);
            resumeNonError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.IsNull(resumeNonError, "There should be no errors, since we resume after pausing");
            await schedule.persistToDB().ConfigureAwait(false);


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, quarterRefNow4, startOfDay);
            halfCalEventRetrieved1 = schedule.getCalendarEvent(halfPauseSubEvent.Id);
            Assert.AreEqual(halfCalEventRetrieved1.ActivePausedTimeLines.Count, 0, "Since we have resumed there should be no active pause");
            Assert.AreEqual(halfCalEventRetrieved1.PausedTimeLines.Count, 2);

            SubCalendarEvent halfPauseSubEventFromMemory = schedule.getSubCalendarEvent(halfPauseSubEvent.Id);
            TimeSpan lowerBound = durationSplit.Add(-Utility.OneMinuteTimeSpan);
            TimeSpan upperBand = durationSplit.Add(Utility.OneMinuteTimeSpan);
            bool withinBounds = lowerBound <= halfPauseSubEventFromMemory.StartToEnd.TimelineSpan && halfPauseSubEventFromMemory.StartToEnd.TimelineSpan <= upperBand;
            Assert.IsTrue(withinBounds);
        }


        /// <summary>
        /// Test verifies that shuffling the schedule when there is a paused tile results in clearing of the paused tile.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ShuffleShouldResetAllActivePauses()
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

            Tuple<CustomErrors, SubCalendarEvent> preCurrentSubEvents = schedule.PauseEvent().Result;
            Assert.AreEqual(preCurrentSubEvents.Item1.Code, (int)CustomErrors.Errors.Pause_Event_There_Is_No_Current_To_Pause);



            SubCalendarEvent pausedSubEvent = calEventRetrieved.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).ToList()[1];
            TimeSpan intialDuration = pausedSubEvent.getActiveDuration;
            DateTimeOffset pausedRefNow = Utility.MiddleTime(pausedSubEvent);
            TimeSpan durationSplit = TimeSpan.FromTicks(intialDuration.Ticks / 2);
            schedule = new TestSchedule(user, pausedRefNow, startOfDay);
            CustomErrors resumeError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(resumeError.Code,
                (int)CustomErrors.Errors.Resume_Event_Paused_Event_Id_is_Null,
                "Resume should return error because of no current paused events");

            Tuple<CustomErrors, SubCalendarEvent> pauseResult = schedule.PauseEvent().Result;
            Assert.AreEqual(pauseResult.Item2, pausedSubEvent);
            await schedule.persistToDB().ConfigureAwait(false);
            reloadTilerUser(ref user, ref tilerUser);
            Assert.AreEqual(
                tilerUser.PausedEventId.ToString(),
                pausedSubEvent.Id);


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, pausedRefNow.AddHours(1), startOfDay);
            CustomErrors resumeNonError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.IsNull(resumeNonError, "There should be no errors, since we resume after pausing");
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset nextRefNow = pausedRefNow.AddDays(1);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);
            SubCalendarEvent pausedSubEventScheduleMemory = schedule.getSubCalendarEvent(pausedSubEvent.Id);
            Assert.AreEqual((int)pausedSubEventScheduleMemory.StartToEnd.TimelineSpan.TotalMinutes, (int)durationSplit.TotalMinutes);// we are approximating to int because of time calculations are to the minute resolution

            Assert.IsNull(tilerUser.PausedEventId);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent calEvent1 = TestUtility.generateCalendarEvent(tilerUser, durationPerEvent, null, calEventTimeLine.Start, calEventTimeLine.End, totalSplit);
            schedule.AddToScheduleAndCommit(calEvent1);

            reloadTilerUser(ref user, ref tilerUser);

            SubCalendarEvent pausedSubEvent1 = calEvent1.ActiveSubEvents.First();


            DateTimeOffset quarterRefNow1 = Utility.MiddleTime(new TimeLine(pausedSubEvent1.Start, Utility.MiddleTime(pausedSubEvent1)));
            TimeSpan quarterTimeSpan = quarterRefNow1 - pausedSubEvent1.Start;
            schedule = new TestSchedule(user, quarterRefNow1, startOfDay);
            Tuple<CustomErrors, SubCalendarEvent> quarterPauseResult = schedule.PauseEvent().Result;
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, quarterRefNow1, startOfDay);
            CalendarEvent calEventRetrieved1 = schedule.getCalendarEvent(pausedSubEvent1.Id);
            Assert.AreEqual(calEventRetrieved1.ActivePausedTimeLines.Count, 1, "Since we haven't resumed there should be one active pause");
            Assert.AreEqual(calEventRetrieved1.PausedTimeLines.Count, 1);

            // Moving forward by a quarter timespan
            DateTimeOffset quarterRefNow2 = quarterRefNow1.Add(quarterTimeSpan);
            schedule = new TestSchedule(user, quarterRefNow2, startOfDay);
            await schedule.ResumeEvent().ConfigureAwait(false);
            await schedule.persistToDB().ConfigureAwait(false);
            calEventRetrieved1 = schedule.getCalendarEvent(pausedSubEvent1.Id);
            Assert.AreEqual(calEventRetrieved1.ActivePausedTimeLines.Count, 0, "Since we have resumed there should be no active pause");
            Assert.AreEqual(calEventRetrieved1.PausedTimeLines.Count, 1);

            //Moving to the halfway time frame
            reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset quarterRefNow3 = quarterRefNow2.Add(quarterTimeSpan);

            schedule = new TestSchedule(user, quarterRefNow3, startOfDay);
            Tuple<CustomErrors, SubCalendarEvent> halfPauseResult = schedule.PauseEvent().Result;// this is because we should have used up half the active time of tile
            SubCalendarEvent halfPauseSubEvent = halfPauseResult.Item2;
            await schedule.persistToDB().ConfigureAwait(false);
            CalendarEvent halfCalEventRetrieved1 = schedule.getCalendarEvent(halfPauseSubEvent.Id);
            Assert.AreEqual(halfCalEventRetrieved1.Id, halfPauseSubEvent.CalendarEventId);
            Assert.AreEqual(halfCalEventRetrieved1.ActivePausedTimeLines.Count, 1, "Since we haven't resumed the second paused instance");
            Assert.AreEqual(halfCalEventRetrieved1.PausedTimeLines.Count, 2);



            reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset quarterRefNow4 = quarterRefNow3.Add(quarterTimeSpan);
            schedule = new TestSchedule(user, quarterRefNow4, startOfDay);
            await schedule.FindMeSomethingToDo(new Location()).ConfigureAwait(false);
            await schedule.persistToDB().ConfigureAwait(false);


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, quarterRefNow4, startOfDay);
            CalendarEvent afterCalEventRetrieved0 = schedule.getCalendarEvent(halfPauseSubEvent.Id);
            Assert.AreEqual(afterCalEventRetrieved0.Id, halfPauseSubEvent.CalendarEventId);
            Assert.AreEqual(afterCalEventRetrieved0.ActivePausedTimeLines.Count, 0, "Since we ran find me something to do, it should have forced reset of all paused timelines");
            Assert.AreEqual(afterCalEventRetrieved0.PausedTimeLines.Count, 1);

        }

        /// <summary>
        /// Test verifies that Revising the schedule when there is a paused tile results in clearing of the paused tile.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ReviseShouldResetAllActivePauses()
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

            Tuple<CustomErrors, SubCalendarEvent> preCurrentSubEvents = schedule.PauseEvent().Result;
            Assert.AreEqual(preCurrentSubEvents.Item1.Code, (int)CustomErrors.Errors.Pause_Event_There_Is_No_Current_To_Pause);



            SubCalendarEvent pausedSubEvent = calEventRetrieved.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).ToList()[1];
            TimeSpan intialDuration = pausedSubEvent.getActiveDuration;
            DateTimeOffset pausedRefNow = Utility.MiddleTime(pausedSubEvent);
            TimeSpan durationSplit = TimeSpan.FromTicks(intialDuration.Ticks / 2);
            schedule = new TestSchedule(user, pausedRefNow, startOfDay);
            CustomErrors resumeError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(resumeError.Code,
                (int)CustomErrors.Errors.Resume_Event_Paused_Event_Id_is_Null,
                "Resume should return error because of no current paused events");

            Tuple<CustomErrors, SubCalendarEvent> pauseResult = schedule.PauseEvent().Result;
            Assert.AreEqual(pauseResult.Item2, pausedSubEvent);
            await schedule.persistToDB().ConfigureAwait(false);
            reloadTilerUser(ref user, ref tilerUser);
            Assert.AreEqual(
                tilerUser.PausedEventId.ToString(),
                pausedSubEvent.Id);


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, pausedRefNow.AddHours(1), startOfDay);
            CustomErrors resumeNonError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.IsNull(resumeNonError, "There should be no errors, since we resume after pausing");
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset nextRefNow = pausedRefNow.AddDays(1);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);
            SubCalendarEvent pausedSubEventScheduleMemory = schedule.getSubCalendarEvent(pausedSubEvent.Id);
            Assert.AreEqual((int)pausedSubEventScheduleMemory.StartToEnd.TimelineSpan.TotalMinutes, (int)durationSplit.TotalMinutes);// we are approximating to int because of time calculations are to the minute resolution

            Assert.IsNull(tilerUser.PausedEventId);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent calEvent1 = TestUtility.generateCalendarEvent(tilerUser, durationPerEvent, null, calEventTimeLine.Start, calEventTimeLine.End, totalSplit);
            schedule.AddToScheduleAndCommit(calEvent1);

            reloadTilerUser(ref user, ref tilerUser);

            SubCalendarEvent pausedSubEvent1 = calEvent1.ActiveSubEvents.First();


            DateTimeOffset quarterRefNow1 = Utility.MiddleTime(new TimeLine(pausedSubEvent1.Start, Utility.MiddleTime(pausedSubEvent1)));
            TimeSpan quarterTimeSpan = quarterRefNow1 - pausedSubEvent1.Start;
            schedule = new TestSchedule(user, quarterRefNow1, startOfDay);
            Tuple<CustomErrors, SubCalendarEvent> quarterPauseResult = schedule.PauseEvent().Result;
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, quarterRefNow1, startOfDay);
            CalendarEvent calEventRetrieved1 = schedule.getCalendarEvent(pausedSubEvent1.Id);
            Assert.AreEqual(calEventRetrieved1.ActivePausedTimeLines.Count, 1, "Since we haven't resumed there should be one active pause");
            Assert.AreEqual(calEventRetrieved1.PausedTimeLines.Count, 1);

            // Moving forward by a quarter timespan
            DateTimeOffset quarterRefNow2 = quarterRefNow1.Add(quarterTimeSpan);
            schedule = new TestSchedule(user, quarterRefNow2, startOfDay);
            await schedule.ResumeEvent().ConfigureAwait(false);
            await schedule.persistToDB().ConfigureAwait(false);
            calEventRetrieved1 = schedule.getCalendarEvent(pausedSubEvent1.Id);
            Assert.AreEqual(calEventRetrieved1.ActivePausedTimeLines.Count, 0, "Since we have resumed there should be no active pause");
            Assert.AreEqual(calEventRetrieved1.PausedTimeLines.Count, 1);

            //Moving to the halfway time frame
            reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset quarterRefNow3 = quarterRefNow2.Add(quarterTimeSpan);

            schedule = new TestSchedule(user, quarterRefNow3, startOfDay);
            Tuple<CustomErrors, SubCalendarEvent> halfPauseResult = schedule.PauseEvent().Result;// this is because we should have used up half the active time of tile
            SubCalendarEvent halfPauseSubEvent = halfPauseResult.Item2;
            await schedule.persistToDB().ConfigureAwait(false);
            CalendarEvent halfCalEventRetrieved1 = schedule.getCalendarEvent(halfPauseSubEvent.Id);
            Assert.AreEqual(halfCalEventRetrieved1.Id, halfPauseSubEvent.CalendarEventId);
            Assert.AreEqual(halfCalEventRetrieved1.ActivePausedTimeLines.Count, 1, "Since we haven't resumed the second paused instance");
            Assert.AreEqual(halfCalEventRetrieved1.PausedTimeLines.Count, 2);



            reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset quarterRefNow4 = quarterRefNow3.Add(quarterTimeSpan);
            schedule = new TestSchedule(user, quarterRefNow4, startOfDay);
            await schedule.reviseSchedule(new Location()).ConfigureAwait(false);
            await schedule.persistToDB().ConfigureAwait(false);


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, quarterRefNow4, startOfDay);
            CalendarEvent afterCalEventRetrieved0 = schedule.getCalendarEvent(halfPauseSubEvent.Id);
            Assert.AreEqual(afterCalEventRetrieved0.Id, halfPauseSubEvent.CalendarEventId);
            Assert.AreEqual(afterCalEventRetrieved0.ActivePausedTimeLines.Count, 0, "Since we ran find me something to do, it should have forced reset of all paused timelines");
            Assert.AreEqual(afterCalEventRetrieved0.PausedTimeLines.Count, 1);

        }


        /// <summary>
        /// Test verifies that Setting tile as now schedule when there is a paused tile results in clearing of the paused tile.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task SetAsNowShouldResetAllActivePauses()
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

            Tuple<CustomErrors, SubCalendarEvent> preCurrentSubEvents = schedule.PauseEvent().Result;
            Assert.AreEqual(preCurrentSubEvents.Item1.Code, (int)CustomErrors.Errors.Pause_Event_There_Is_No_Current_To_Pause);



            SubCalendarEvent pausedSubEvent = calEventRetrieved.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).ToList()[1];
            TimeSpan intialDuration = pausedSubEvent.getActiveDuration;
            DateTimeOffset pausedRefNow = Utility.MiddleTime(pausedSubEvent);
            TimeSpan durationSplit = TimeSpan.FromTicks(intialDuration.Ticks / 2);
            schedule = new TestSchedule(user, pausedRefNow, startOfDay);
            CustomErrors resumeError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(resumeError.Code,
                (int)CustomErrors.Errors.Resume_Event_Paused_Event_Id_is_Null,
                "Resume should return error because of no current paused events");

            Tuple<CustomErrors, SubCalendarEvent> pauseResult = schedule.PauseEvent().Result;
            Assert.AreEqual(pauseResult.Item2, pausedSubEvent);
            await schedule.persistToDB().ConfigureAwait(false);
            reloadTilerUser(ref user, ref tilerUser);
            Assert.AreEqual(
                tilerUser.PausedEventId.ToString(),
                pausedSubEvent.Id);


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, pausedRefNow.AddHours(1), startOfDay);
            CustomErrors resumeNonError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.IsNull(resumeNonError, "There should be no errors, since we resume after pausing");
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset nextRefNow = pausedRefNow.AddDays(1);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);
            SubCalendarEvent pausedSubEventScheduleMemory = schedule.getSubCalendarEvent(pausedSubEvent.Id);
            Assert.AreEqual((int)pausedSubEventScheduleMemory.StartToEnd.TimelineSpan.TotalMinutes, (int)durationSplit.TotalMinutes);// we are approximating to int because of time calculations are to the minute resolution

            Assert.IsNull(tilerUser.PausedEventId);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent calEvent1 = TestUtility.generateCalendarEvent(tilerUser, durationPerEvent, null, calEventTimeLine.Start, calEventTimeLine.End, totalSplit);
            schedule.AddToScheduleAndCommit(calEvent1);

            reloadTilerUser(ref user, ref tilerUser);
            SubCalendarEvent pausedSubEvent1 = calEvent1.ActiveSubEvents.First();


            DateTimeOffset quarterRefNow1 = Utility.MiddleTime(new TimeLine(pausedSubEvent1.Start, Utility.MiddleTime(pausedSubEvent1)));
            TimeSpan quarterTimeSpan = quarterRefNow1 - pausedSubEvent1.Start;
            schedule = new TestSchedule(user, quarterRefNow1, startOfDay);
            Tuple<CustomErrors, SubCalendarEvent> quarterPauseResult = schedule.PauseEvent().Result;
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, quarterRefNow1, startOfDay);
            CalendarEvent calEventRetrieved1 = schedule.getCalendarEvent(pausedSubEvent1.Id);
            Assert.AreEqual(calEventRetrieved1.ActivePausedTimeLines.Count, 1, "Since we haven't resumed there should be one active pause");
            Assert.AreEqual(calEventRetrieved1.PausedTimeLines.Count, 1);

            // Moving forward by a quarter timespan
            DateTimeOffset quarterRefNow2 = quarterRefNow1.Add(quarterTimeSpan);
            schedule = new TestSchedule(user, quarterRefNow2, startOfDay);
            await schedule.ResumeEvent().ConfigureAwait(false);
            await schedule.persistToDB().ConfigureAwait(false);
            calEventRetrieved1 = schedule.getCalendarEvent(pausedSubEvent1.Id);
            Assert.AreEqual(calEventRetrieved1.ActivePausedTimeLines.Count, 0, "Since we have resumed there should be no active pause");
            Assert.AreEqual(calEventRetrieved1.PausedTimeLines.Count, 1);

            //Moving to the halfway time frame
            reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset quarterRefNow3 = quarterRefNow2.Add(quarterTimeSpan);

            schedule = new TestSchedule(user, quarterRefNow3, startOfDay);
            Tuple<CustomErrors, SubCalendarEvent> halfPauseResult = schedule.PauseEvent().Result;// this is because we should have used up half the active time of tile
            SubCalendarEvent halfPauseSubEvent = halfPauseResult.Item2;
            await schedule.persistToDB().ConfigureAwait(false);
            CalendarEvent halfCalEventRetrieved1 = schedule.getCalendarEvent(halfPauseSubEvent.Id);
            Assert.AreEqual(halfCalEventRetrieved1.Id, halfPauseSubEvent.CalendarEventId);
            Assert.AreEqual(halfCalEventRetrieved1.ActivePausedTimeLines.Count, 1, "Since we haven't resumed the second paused instance");
            Assert.AreEqual(halfCalEventRetrieved1.PausedTimeLines.Count, 2);



            reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset quarterRefNow4 = quarterRefNow3.Add(quarterTimeSpan);
            schedule = new TestSchedule(user, quarterRefNow4, startOfDay);
            schedule.SetSubeventAsNow(halfCalEventRetrieved1.ActiveSubEvents.Last().Id);
            await schedule.persistToDB().ConfigureAwait(false);


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, quarterRefNow4, startOfDay);
            CalendarEvent afterCalEventRetrieved0 = schedule.getCalendarEvent(halfPauseSubEvent.Id);
            Assert.AreEqual(afterCalEventRetrieved0.Id, halfPauseSubEvent.CalendarEventId);
            Assert.AreEqual(afterCalEventRetrieved0.ActivePausedTimeLines.Count, 0, "Since we ran find me something to do, it should have forced reset of all paused timelines");
            Assert.AreEqual(afterCalEventRetrieved0.PausedTimeLines.Count, 1);

        }

        /// <summary>
        /// Paused and resumed tiles should be completeable from the pausedTImelineEntry
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CompletePauseEvent()
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

            Tuple<CustomErrors, SubCalendarEvent> preCurrentSubEvents = schedule.PauseEvent().Result;
            Assert.AreEqual(preCurrentSubEvents.Item1.Code, (int)CustomErrors.Errors.Pause_Event_There_Is_No_Current_To_Pause);



            SubCalendarEvent pausedSubEvent = calEventRetrieved.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).ToList()[1];
            DateTimeOffset pausedRefNow = Utility.MiddleTime(pausedSubEvent);
            schedule = new TestSchedule(user, pausedRefNow, startOfDay);
            CustomErrors resumeError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(resumeError.Code,
                (int)CustomErrors.Errors.Resume_Event_Paused_Event_Id_is_Null,
                "Resume should return error because of no current paused events");

            Tuple<CustomErrors, SubCalendarEvent> pauseResult = schedule.PauseEvent().Result;
            Assert.AreEqual(pauseResult.Item2, pausedSubEvent);
            await schedule.persistToDB().ConfigureAwait(false);
            reloadTilerUser(ref user, ref tilerUser);
            
            
            Assert.AreEqual(
                tilerUser.PausedEventId.ToString(),
                pausedSubEvent.Id);


            DateTimeOffset nextRefNow = pausedRefNow.AddDays(1);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);

            SubCalendarEvent pausedSubEventRetrived = schedule.getSubCalendarEvent(tilerUser.PausedEventId);
            Assert.IsTrue(pausedSubEventRetrived.isPaused);


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);
            Assert.AreEqual(
                pausedSubEventRetrived.ParentCalendarEvent.ActivePausedTimeLines.Count
                , 1,
                "There should only be one paused timeline since we've paused once on thid Calendarevent");
            PausedTimeLineEntry pausedTimeLine = pausedSubEventRetrived.ParentCalendarEvent.ActivePausedTimeLines.First();
            schedule.markSubEventAsCompleteCalendarEventAndReadjust(pausedTimeLine.Id);
            await schedule.persistToDB().ConfigureAwait(false);


            reloadTilerUser(ref user, ref tilerUser);
            SubCalendarEvent willBeCompletedSubEventRetrieved = TestUtility.getSubEventById(tilerUser.PausedEventId.ToString(), user);
            CalendarEvent calendarEventOfRetrivedSubEvent = willBeCompletedSubEventRetrieved.ParentCalendarEvent;

            Assert.AreEqual(calendarEventOfRetrivedSubEvent.PausedTimeLines.Count, 1, "All active paused tiles should stay paused, to avoid possible side effects");
            Assert.IsTrue(willBeCompletedSubEventRetrieved.getIsComplete);
            
        }

        /// <summary>
        /// Paused and resumed tiles should be deletable from the pausedTImelineEntry
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DeletePauseEvent()
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

            Tuple<CustomErrors, SubCalendarEvent> preCurrentSubEvents = schedule.PauseEvent().Result;
            Assert.AreEqual(preCurrentSubEvents.Item1.Code, (int)CustomErrors.Errors.Pause_Event_There_Is_No_Current_To_Pause);



            SubCalendarEvent pausedSubEvent = calEventRetrieved.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).ToList()[1];
            DateTimeOffset pausedRefNow = Utility.MiddleTime(pausedSubEvent);
            schedule = new TestSchedule(user, pausedRefNow, startOfDay);
            CustomErrors resumeError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(resumeError.Code,
                (int)CustomErrors.Errors.Resume_Event_Paused_Event_Id_is_Null,
                "Resume should return error because of no current paused events");

            Tuple<CustomErrors, SubCalendarEvent> pauseResult = schedule.PauseEvent().Result;
            Assert.AreEqual(pauseResult.Item2, pausedSubEvent);
            await schedule.persistToDB().ConfigureAwait(false);
            reloadTilerUser(ref user, ref tilerUser);


            Assert.AreEqual(
                tilerUser.PausedEventId.ToString(),
                pausedSubEvent.Id);


            DateTimeOffset nextRefNow = pausedRefNow.AddDays(1);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);

            SubCalendarEvent pausedSubEventRetrived = schedule.getSubCalendarEvent(tilerUser.PausedEventId);
            Assert.IsTrue(pausedSubEventRetrived.isPaused);


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);
            Assert.AreEqual(
                pausedSubEventRetrived.ParentCalendarEvent.ActivePausedTimeLines.Count
                , 1,
                "There should only be one paused timeline since we've paused once on thid Calendarevent");
            PausedTimeLineEntry pausedTimeLine = pausedSubEventRetrived.ParentCalendarEvent.ActivePausedTimeLines.First();
            await schedule.deleteSubCalendarEventAndReadjust(pausedTimeLine.Id).ConfigureAwait(false);
            await schedule.persistToDB().ConfigureAwait(false);


            reloadTilerUser(ref user, ref tilerUser);
            SubCalendarEvent willBeDeletedSubEventRetrieved = TestUtility.getSubEventById(tilerUser.PausedEventId.ToString(), user);
            CalendarEvent calendarEventOfRetrivedSubEvent = willBeDeletedSubEventRetrieved.ParentCalendarEvent;

            Assert.AreEqual(calendarEventOfRetrivedSubEvent.PausedTimeLines.Count, 1, "All active paused tiles should stay paused, to avoid possible side effects");
            Assert.IsFalse(willBeDeletedSubEventRetrieved.getIsComplete);
            Assert.IsTrue(willBeDeletedSubEventRetrieved.getIsDeleted);
        }


        /// <summary>
        /// Paused and resumed tiles can be editable only through their deadlines and notes but not the actual tile itself
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task EditablePauseEvent()
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

            Tuple<CustomErrors, SubCalendarEvent> preCurrentSubEvents = schedule.PauseEvent().Result;
            Assert.AreEqual(preCurrentSubEvents.Item1.Code, (int)CustomErrors.Errors.Pause_Event_There_Is_No_Current_To_Pause);



            SubCalendarEvent pausedSubEvent = calEventRetrieved.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).ToList()[1];
            DateTimeOffset pausedRefNow = Utility.MiddleTime(pausedSubEvent);
            schedule = new TestSchedule(user, pausedRefNow, startOfDay);
            CustomErrors resumeError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(resumeError.Code,
                (int)CustomErrors.Errors.Resume_Event_Paused_Event_Id_is_Null,
                "Resume should return error because of no current paused events");

            Tuple<CustomErrors, SubCalendarEvent> pauseResult = schedule.PauseEvent().Result;
            Assert.AreEqual(pauseResult.Item2, pausedSubEvent);
            await schedule.persistToDB().ConfigureAwait(false);
            reloadTilerUser(ref user, ref tilerUser);


            Assert.AreEqual(
                tilerUser.PausedEventId.ToString(),
                pausedSubEvent.Id);


            DateTimeOffset nextRefNow = pausedRefNow.AddDays(1);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);

            SubCalendarEvent pausedSubEventRetrived = schedule.getSubCalendarEvent(tilerUser.PausedEventId);
            Assert.IsTrue(pausedSubEventRetrived.isPaused);
            await schedule.persistToDB().ConfigureAwait(false);


            DateTimeOffset afterPauseDeadline = pausedSubEventRetrived.End.AddDays(1);

            reloadTilerUser(ref user, ref tilerUser);
            pausedSubEventRetrived = TestUtility.getSubEventById(tilerUser.PausedEventId.ToString(), user);
            schedule = new TestSchedule(user, nextRefNow, startOfDay, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            schedule.BundleChangeUpdate(
                pausedSubEventRetrived.Id,
                pausedSubEventRetrived.Name.createCopy(), 
                pausedSubEventRetrived.Start, 
                pausedSubEventRetrived.End, 
                pausedSubEventRetrived.ParentCalendarEvent.Start, afterPauseDeadline, pausedSubEventRetrived.ParentCalendarEvent.NumberOfSplit, "");
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            SubCalendarEvent willBeDeletedSubEventRetrieved = TestUtility.getSubEventById(tilerUser.PausedEventId.ToString(), user);
            CalendarEvent calendarEventOfRetrivedSubEvent = willBeDeletedSubEventRetrieved.ParentCalendarEvent;

            Assert.AreEqual(calendarEventOfRetrivedSubEvent.End, afterPauseDeadline, "The calendarevent deadline should be the same as the deadline");

            ////Testing a name change and notes change
            string updatedNotes = "updatedNotes";
            string updatedName = "updatedName";

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, nextRefNow, startOfDay, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            SubCalendarEvent pausedSubEventRetrivedAftereadlineEdit = TestUtility.getSubEventById(tilerUser.PausedEventId.ToString(), user);
            EventName newName = new EventName(tilerUser, pausedSubEventRetrivedAftereadlineEdit, updatedName);
            schedule.BundleChangeUpdate(
                pausedSubEventRetrivedAftereadlineEdit.Id,
                newName,
                pausedSubEventRetrivedAftereadlineEdit.Start,
                pausedSubEventRetrivedAftereadlineEdit.End,
                pausedSubEventRetrivedAftereadlineEdit.ParentCalendarEvent.Start, pausedSubEventRetrivedAftereadlineEdit.ParentCalendarEvent.End,
                pausedSubEventRetrivedAftereadlineEdit.ParentCalendarEvent.NumberOfSplit, updatedNotes);
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            SubCalendarEvent afterSubEventHasNameUpdated = TestUtility.getSubEventById(tilerUser.PausedEventId.ToString(), user);
            Assert.AreEqual(afterSubEventHasNameUpdated.getName.NameValue, updatedName);
            Assert.AreEqual(afterSubEventHasNameUpdated.Notes.UserNote, updatedNotes);


            //Testing a name change and notes change with pauseTimeline Id
            string updatedNotesForPauseTimelineId = "updatedNotes from Pause timeline id";
            string updatedNameForPauseTimelineId = "updatedName from Pause timeline id";

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, nextRefNow, startOfDay, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            SubCalendarEvent pausedSubEventFromPausedTimeline = TestUtility.getSubEventById(tilerUser.PausedEventId.ToString(), user);
            EventName newNameForPauseTimelineId = new EventName(tilerUser, pausedSubEventFromPausedTimeline, updatedNameForPauseTimelineId);
            DateTimeOffset newDeadlineFromPauseTimelineId = pausedSubEventFromPausedTimeline.ParentCalendarEvent.End.AddDays(2);
            PausedTimeLineEntry pausedTimeLineEntry = pausedSubEventFromPausedTimeline.ParentCalendarEvent.ActivePausedTimeLines.First();
            var changeResult = schedule.BundleChangeUpdate(
                pausedTimeLineEntry.Id.ToString(),
                newNameForPauseTimelineId,
                pausedSubEventFromPausedTimeline.Start,
                pausedSubEventFromPausedTimeline.End,
                pausedSubEventFromPausedTimeline.ParentCalendarEvent.Start, newDeadlineFromPauseTimelineId,
                pausedSubEventFromPausedTimeline.ParentCalendarEvent.NumberOfSplit, updatedNotesForPauseTimelineId);
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            SubCalendarEvent subEventFrompausedTimelineId = TestUtility.getSubEventById(tilerUser.PausedEventId.ToString(), user);
            Assert.AreEqual(subEventFrompausedTimelineId.getName.NameValue, updatedNameForPauseTimelineId);
            Assert.AreEqual(subEventFrompausedTimelineId.Notes.UserNote, updatedNotesForPauseTimelineId);
            Assert.AreEqual(subEventFrompausedTimelineId.ParentCalendarEvent.End, newDeadlineFromPauseTimelineId);
            Assert.AreEqual((int)changeResult.Item1.Code, (int)CustomErrors.Errors.success);



            //With trying to update with the pausetimeline id
            string updatedNotesWithPauseTimelineAsSubEventTimeLine = "updatedNotes from Pause timeline id";
            string updatedNameWithPauseTimelineAsSubEventTimeLine = "updatedName from Pause timeline id";

            
            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, nextRefNow, startOfDay, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            SubCalendarEvent pausedSubEventWithPauseTimelineAsSubEventTimeLine = TestUtility.getSubEventById(tilerUser.PausedEventId.ToString(), user);

            string oldNotes = pausedSubEventWithPauseTimelineAsSubEventTimeLine.Notes.UserNote;
            string oldName = pausedSubEventWithPauseTimelineAsSubEventTimeLine.getName.NameValue;


            EventName newNameWithPauseTimelineAsSubEventTimeLine = new EventName(tilerUser, pausedSubEventWithPauseTimelineAsSubEventTimeLine, updatedNameWithPauseTimelineAsSubEventTimeLine);
            DateTimeOffset oldDeadline = pausedSubEventWithPauseTimelineAsSubEventTimeLine.ParentCalendarEvent.End;
            DateTimeOffset newDeadlineWithPauseTimelineAsSubEventTimeLine = pausedSubEventWithPauseTimelineAsSubEventTimeLine.ParentCalendarEvent.End.AddDays(2);
            PausedTimeLineEntry pausedTimeLineEntryWithPauseTimelineAsSubEventTimeLine = pausedSubEventWithPauseTimelineAsSubEventTimeLine.ParentCalendarEvent.ActivePausedTimeLines.First();
            var changeResultWithPauseTimelineAsSubEventTimeLine = schedule.BundleChangeUpdate(
                pausedTimeLineEntryWithPauseTimelineAsSubEventTimeLine.Id.ToString(),
                newNameWithPauseTimelineAsSubEventTimeLine,
                pausedSubEventWithPauseTimelineAsSubEventTimeLine.Start,
                pausedSubEventWithPauseTimelineAsSubEventTimeLine.End.AddHours(2),
                pausedSubEventWithPauseTimelineAsSubEventTimeLine.ParentCalendarEvent.Start, newDeadlineWithPauseTimelineAsSubEventTimeLine,
                pausedSubEventWithPauseTimelineAsSubEventTimeLine.ParentCalendarEvent.NumberOfSplit, updatedNotesWithPauseTimelineAsSubEventTimeLine);
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            SubCalendarEvent subEventWithPauseTimelineAsSubEventTimeLine = TestUtility.getSubEventById(tilerUser.PausedEventId.ToString(), user);
            Assert.AreEqual(subEventWithPauseTimelineAsSubEventTimeLine.getName.NameValue, oldName);
            Assert.AreEqual(subEventWithPauseTimelineAsSubEventTimeLine.Notes.UserNote, oldNotes);
            Assert.AreEqual(subEventWithPauseTimelineAsSubEventTimeLine.ParentCalendarEvent.End, oldDeadline);
            Assert.AreEqual((int)changeResultWithPauseTimelineAsSubEventTimeLine.Item1.Code, (int)CustomErrors.Errors.Cannot_update_timeline_of_pausedtimeline);


        }


    }
}
