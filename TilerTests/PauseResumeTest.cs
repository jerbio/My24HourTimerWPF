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

            var scheduleDumpask = schedule.CreateScheduleDump(schedule.Now);
            scheduleDumpask.Wait();
            var scheduleDump = scheduleDumpask.Result;
            TestSchedule scheduleDumpSchedule = new TestSchedule(scheduleDump, user);
            TestUtility.isTestEquivalent(schedule, scheduleDumpSchedule);
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
            Assert.AreEqual(resumeSubEvent.ParentCalendarEvent.pausedTimeLines.Count, 1);
            Assert.IsFalse(resumeSubEvent.isPauseLocked);

            TimeSpan totalPausedSpan = TimeSpan.FromTicks(resumeSubEvent.ParentCalendarEvent.pausedTimeLines.Sum((timeLine) => timeLine.TimelineSpan.Ticks));
            DateTimeOffset pausedEventStart = schedule.Now.constNow - totalPausedSpan;
            Assert.AreEqual(resumeSubEvent.Start, pausedEventStart);
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);
            SubCalendarEvent retrievedResumeSubEvent = schedule.getSubCalendarEvent(pausedEventId);
            Assert.AreEqual(retrievedResumeSubEvent.ParentCalendarEvent.pausedTimeLines.Count, 1);
            Assert.IsTrue(exhaustedTimeLine.isTestEquivalent(retrievedResumeSubEvent.ParentCalendarEvent.pausedTimeLines[0]));
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
            Assert.AreEqual(resumeSubEvent.ParentCalendarEvent.pausedTimeLines.Count, 1);

            TimeSpan totalPausedSpan = TimeSpan.FromTicks(resumeSubEvent.ParentCalendarEvent.pausedTimeLines.Sum((timeLine) => timeLine.TimelineSpan.Ticks));
            DateTimeOffset pausedEventStart = schedule.Now.constNow - totalPausedSpan;
            Assert.AreEqual(resumeSubEvent.Start, pausedEventStart);
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, afterPausedEventDeadline, startOfDay);
            SubCalendarEvent retrievedResumeSubEvent = schedule.getSubCalendarEvent(pausedEventId);
            Assert.AreEqual(retrievedResumeSubEvent.ParentCalendarEvent.pausedTimeLines.Count, 1);// THis is known to fail because we of the reordering of tiles based on ids. This would mean moving the paused tile to an earlier time frame outside the schedlue retrieval window. We need to make the pause timeline part of the calendar event and not Sub calendar event
            Assert.IsTrue(exhaustedTimeLine.isTestEquivalent(retrievedResumeSubEvent.ParentCalendarEvent.pausedTimeLines[0]));
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


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, pausedRefNow.AddHours(1), startOfDay);
            CustomErrors resumeNonError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.IsNull(resumeNonError, "There should be no errors, since we resume after pausing");

            DateTimeOffset nextRefNow = pausedRefNow.AddDays(1);
            schedule = new TestSchedule(user, nextRefNow, startOfDay);

            SubCalendarEvent pausedSubEventRetrived = schedule.getSubCalendarEvent(tilerUser.PausedEventId);
            Assert.IsTrue(pausedSubEventRetrived.isPauseLocked);



            schedule = new TestSchedule(user, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(calEvent0);
            reloadTilerUser(ref user, ref tilerUser);

            schedule = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent calEventRetrieved2 = schedule.getCalendarEvent(calEvent.Id);




            SubCalendarEvent pausedSubEventAgain = calEventRetrieved2.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).ToList().Last();

            TimeSpan quarterSpan = TimeSpan.FromSeconds(pausedSubEventAgain.StartToEnd.TotalActiveSpan.TotalSeconds / 4);
            DateTimeOffset pausedRefNowAgain = Utility.MiddleTime(pausedSubEventAgain);
            schedule = new TestSchedule(user, pausedRefNowAgain, startOfDay);
            CustomErrors resumeErrorAgain = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(resumeErrorAgain.Code,
                (int)CustomErrors.Errors.Resume_Event_Paused_Event_Id_is_Null,
                "Resume should return error because of no current paused events");

            Tuple<CustomErrors, SubCalendarEvent> pauseResultNowAgain = schedule.PauseEvent().Result;
            DateTimeOffset nextPausedTime = pausedRefNowAgain.Add(quarterSpan);
            Assert.IsTrue(calEventRetrieved2.pausedTimeLines.Count >= 2, "There should be at least 2 paused timelines due to the double pauses and resumes");
            Assert.AreEqual(pauseResult.Item2, pausedSubEvent);
            await schedule.persistToDB().ConfigureAwait(false);

            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, pausedRefNow.AddHours(1), startOfDay);
            resumeNonError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.IsNull(resumeNonError, "There should be no errors, since we resume after pausing");
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


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, pausedRefNow.AddHours(1), startOfDay);
            Location broomfieldLocation = new Location(39.9456167, -105.1376022);
            broomfieldLocation.IsVerified = true;
            schedule.FindMeSomethingToDo(broomfieldLocation).Wait();
            CustomErrors shuffleResumeNonError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(shuffleResumeNonError.Code,
                (int)CustomErrors.Errors.Resume_Event_Paused_Event_Id_is_Null,
                "Resume should return error because of no current paused events");

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


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, pausedRefNow.AddHours(1), startOfDay);
            Location broomfieldLocation = new Location(39.9456167, -105.1376022);
            broomfieldLocation.IsVerified = true;
            schedule.reviseSchedule(broomfieldLocation).Wait();
            CustomErrors shuffleResumeNonError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(shuffleResumeNonError.Code,
                (int)CustomErrors.Errors.Resume_Event_Paused_Event_Id_is_Null,
                "Resume should return error because of no current paused events");

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


            reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, pausedRefNow.AddHours(1), startOfDay);
            Location broomfieldLocation = new Location(39.9456167, -105.1376022);
            broomfieldLocation.IsVerified = true;
            schedule.reviseSchedule(broomfieldLocation).Wait();
            CustomErrors shuffleResumeNonError = await schedule.ResumeEvent().ConfigureAwait(false);
            Assert.AreEqual(shuffleResumeNonError.Code,
                (int)CustomErrors.Errors.Resume_Event_Paused_Event_Id_is_Null,
                "Resume should return error because of no current paused events");

        }

    }
}
