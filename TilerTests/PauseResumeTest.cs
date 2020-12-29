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
            Assert.IsTrue(pausedSubEventRetrived.isPauseLocked);
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
            Assert.IsFalse(notPausedSubEvent.isPauseLocked);

            CustomErrors resumeNotPausedSubEventResult = await schedule.ResumeEvent(notPausedSubEvent.getTilerID).ConfigureAwait(false);
            Assert.AreEqual(
                resumeNotPausedSubEventResult.Code,
                (int)CustomErrors.Errors.Resume_Event_Cannot_Resume_Not_Paused_SubEvent,
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



            Assert.AreEqual(resumeNotPausedSubEventResult.Code, (int)CustomErrors.Errors.Resume_Event_Cannot_Resume_Not_Paused_SubEvent, "Trying to resume not paused event");
            CustomErrors resumeResult = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.IsNull(resumeResult, "There should be no errors because of successful resume");

            SubCalendarEvent resumeSubEvent = schedule.getSubCalendarEvent(pausedEventId);
            Assert.AreEqual(resumeSubEvent.pausedTimeLines.Count, 1);
            Assert.IsFalse(resumeSubEvent.isPauseLocked);

            TimeSpan totalPausedSpan = TimeSpan.FromTicks(resumeSubEvent.pausedTimeLines.Sum((timeLine) => timeLine.TimelineSpan.Ticks));
            DateTimeOffset pausedEventStart = schedule.Now.constNow - totalPausedSpan;
            Assert.AreEqual(resumeSubEvent.Start, pausedEventStart);
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);
            SubCalendarEvent retrievedResumeSubEvent = schedule.getSubCalendarEvent(pausedEventId);
            Assert.AreEqual(retrievedResumeSubEvent.pausedTimeLines.Count, 1);
            Assert.IsTrue(exhaustedTimeLine.isTestEquivalent(retrievedResumeSubEvent.pausedTimeLines[0]));
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

            CustomErrors resumeNotPausedSubEventResult = await schedule.ResumeEvent(notPausedSubEvent.getTilerID).ConfigureAwait(false);
            Assert.AreEqual(resumeNotPausedSubEventResult.Code, (int)CustomErrors.Errors.Resume_Event_Cannot_Resume_Not_Paused_SubEvent, "Trying to resume not paused event");
            CustomErrors resumeResult = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(resumeResult.Code, (int)CustomErrors.Errors.Resume_Event_Cannot_Outside_Deadline_Of_CalendarEvent, "Resuming outside range, should be forced");

            CustomErrors forceResumeResult = await schedule.ResumeEvent(forceOutsideDeadline: true).ConfigureAwait(false);
            Assert.IsNull(forceResumeResult, "There should be no errors because of successful resume");

            SubCalendarEvent resumeSubEvent = schedule.getSubCalendarEvent(pausedEventId);
            Assert.AreEqual(resumeSubEvent.pausedTimeLines.Count, 1);

            TimeSpan totalPausedSpan = TimeSpan.FromTicks(resumeSubEvent.pausedTimeLines.Sum((timeLine) => timeLine.TimelineSpan.Ticks));
            DateTimeOffset pausedEventStart = schedule.Now.constNow - totalPausedSpan;
            Assert.AreEqual(resumeSubEvent.Start, pausedEventStart);
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, afterPausedEventDeadline, startOfDay);
            SubCalendarEvent retrievedResumeSubEvent = schedule.getSubCalendarEvent(pausedEventId);
            Assert.AreEqual(retrievedResumeSubEvent.pausedTimeLines.Count, 1);
            Assert.IsTrue(exhaustedTimeLine.isTestEquivalent(retrievedResumeSubEvent.pausedTimeLines[0]));
        }

        /// <summary>
        /// When a sub event is paused and resumed multiple times the sub event should have different timelines with different paused events
        /// </summary>
        [TestMethod]
        public void MultiplePauseTImeLines()
        {
        }
    }
}
