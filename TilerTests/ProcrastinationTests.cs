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
    public class ProcrastinationTests
    {
        [TestMethod]
        public void procrastinateSingle()
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
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB().Wait();
            testEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            Assert.IsTrue(testEvent.ActiveSubEvents.OrderBy(subevent => subevent.Start).First().Start == refNow);
            Schedule = new TestSchedule(user, refNow);
            TimeSpan procrastinationSpan = TimeSpan.FromHours(5);
            var procrastinateResult = Schedule.ProcrastinateJustAnEvent(testEvent.AllSubEvents.OrderBy(subEvent => subEvent.Start).First().getId, procrastinationSpan);
            Assert.IsNull(procrastinateResult.Item1);
            Schedule.persistToDB().Wait();
            Schedule = new TestSchedule(user, refNow);

            CalendarEvent testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            DateTimeOffset latestTime = refNow.Add(procrastinationSpan);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= latestTime);
            Assert.IsTrue(testEventCopy.getProcrastinationInfo.PreferredStartTime >= latestTime);
            ///Quality of procrastinate all////
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TimeSpan secondDuration = TimeSpan.FromHours(4);
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser, secondDuration, new Repetition(), start, end, 2, false);
            
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent0).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            latestTime = refNow.Add(procrastinationSpan);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= latestTime);
            Assert.IsTrue(testEventCopy.getProcrastinationInfo.PreferredStartTime >= latestTime);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            TimeSpan secondProcrastinationSpan = TimeSpan.FromHours(2);
            DateTimeOffset secondLatestTime = refNow.Add(secondProcrastinationSpan);
            var testEvent0Copy = TestUtility.getCalendarEventById(testEvent0.getId, user);
            procrastinateResult = Schedule.ProcrastinateJustAnEvent(testEvent0Copy.AllSubEvents.OrderBy(subEvent => subEvent.Start).First().getId, secondProcrastinationSpan);
            Assert.IsNull(procrastinateResult.Item1);
            Schedule.persistToDB().Wait();
            Schedule = new TestSchedule(user, refNow);
            testEvent0Copy = TestUtility.getCalendarEventById(testEvent0.getId, user);
            Assert.IsTrue(testEvent0Copy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= secondLatestTime);
            Assert.IsTrue(testEvent0Copy.getProcrastinationInfo.PreferredStartTime >= secondLatestTime);
            testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            latestTime = refNow.Add(procrastinationSpan);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= latestTime);
            Assert.IsTrue(testEventCopy.getProcrastinationInfo.PreferredStartTime >= latestTime);
            Assert.IsFalse(testEventCopy.getNowInfo.isInitialized);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset thirdRefNow = refNow.AddHours(1);
            Schedule = new TestSchedule(user, thirdRefNow);
            Schedule.SetCalendarEventAsNow(testEventCopy.Id);
            Schedule.persistToDB().Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start == thirdRefNow);
            Assert.IsFalse(testEventCopy.getProcrastinationInfo.isNull);
            Assert.AreEqual(testEventCopy.getProcrastinationInfo.PreferredStartTime, Utility.JSStartTime);
            Assert.AreEqual(testEventCopy.getProcrastinationInfo.DislikedStartTime, Utility.JSStartTime);
            Assert.IsTrue(testEventCopy.getNowInfo.isInitialized);

            testEvent0Copy = TestUtility.getCalendarEventById(testEvent0.getId, user);
            Assert.IsTrue(testEvent0Copy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= secondLatestTime);
            Assert.IsTrue(testEvent0Copy.getProcrastinationInfo.PreferredStartTime >= secondLatestTime);
        }

        [TestMethod]
        public void procrastinateSingleRestricted()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2018");
            refNow = refNow.removeSecondsAndMilliseconds();
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DateTimeOffset end = repetitionRange.End.AddDays(14);
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition();
            TimeSpan fullDuration = duration + duration;
            var restrictionProfile = new RestrictionProfile(start.AddHours(2), fullDuration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 4, false, restrictionProfile: restrictionProfile, now: Schedule.Now) as CalendarEventRestricted;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB().Wait();
            testEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            Assert.IsTrue(testEvent.ActiveSubEvents.OrderBy(subevent => subevent.Start).First().Start == refNow);

            Schedule = new TestSchedule(user, refNow);
            TimeSpan procrastinationSpan = TimeSpan.FromHours(5);
            var procrastinateResult = Schedule.ProcrastinateJustAnEvent(testEvent.AllSubEvents.OrderBy(subEvent => subEvent.Start).First().getId, procrastinationSpan);
            Assert.IsNull(procrastinateResult.Item1);
            Schedule.persistToDB().Wait();
            Schedule = new TestSchedule(user, refNow);

            CalendarEvent testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            DateTimeOffset latestTime = refNow.Add(procrastinationSpan);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= latestTime);
            Assert.IsTrue(testEventCopy.getProcrastinationInfo.PreferredStartTime >= latestTime);
        }

        [TestMethod]
        public void procrastinateSingleEventAroundMultipleEvents()
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
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TimeLine repeatTimeLine = new TimeLine(start, end.AddDays(21));
            TimeLine calTimeLine = new TimeLine(start, start.Add(duration));
            Repetition repetition = new Repetition(repeatTimeLine, Repetition.Frequency.WEEKLY, calTimeLine);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 2, false);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);

            TimeSpan duration0 = TimeSpan.FromHours(2);
            DateTimeOffset start0 = refNow;
            DateTimeOffset end0 = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser, duration0, new Repetition(), start0, end0, 2, false);
            Schedule.AddToScheduleAndCommitAsync(testEvent0).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TimeSpan duration1 = TimeSpan.FromHours(2);
            DateTimeOffset start1 = refNow;
            DateTimeOffset end1 = refNow.AddHours(7);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, duration1, new Repetition(), start1, end1, 2, false);
            Schedule.AddToScheduleAndCommitAsync(testEvent1).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB().Wait();

            Schedule = new TestSchedule(user, refNow);
            TimeSpan procrastinationSpan = TimeSpan.FromHours(5);
            var procrassinateResult = Schedule.ProcrastinateJustAnEvent(testEvent.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().getId, procrastinationSpan);
            Assert.IsNull(procrassinateResult.Item1);
            Schedule.persistToDB().Wait();
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            DateTimeOffset latestTime = refNow.Add(procrastinationSpan);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= latestTime);


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TimeLine repeatTimeLineRestricted = new TimeLine(start, end.AddDays(21));
            TimeLine calTimeLineRestricted = new TimeLine(start, start.Add(duration));
            Repetition repetitionRestricted = new Repetition(repeatTimeLineRestricted, Repetition.Frequency.WEEKLY, calTimeLineRestricted);
            TimeSpan reestrictionDuration = TimeSpan.FromHours(8);
            var restrictionProfile = new RestrictionProfile(start.AddHours(2), reestrictionDuration);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEventRestricted = TestUtility.generateCalendarEvent(tilerUser, duration, repetitionRestricted, start, end, 2, false, restrictionProfile: restrictionProfile, now: Schedule.Now);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEventRestricted).Wait();

            CalendarEvent testEventRestrictedCopy = TestUtility.getCalendarEventById(testEventRestricted.getId, user);
            TimeSpan procrastinationSpanResticted = TimeSpan.FromHours(1);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            var procrassinateRestictedResult = Schedule.ProcrastinateJustAnEvent(testEventRestrictedCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().getId, procrastinationSpanResticted);
            Assert.IsNull(procrassinateRestictedResult.Item1);
            Schedule.persistToDB().Wait();
            testEventRestrictedCopy = TestUtility.getCalendarEventById(testEventRestricted.getId, user);
            DateTimeOffset latestTimeRestricted = refNow.Add(procrastinationSpan);
            Assert.IsTrue(testEventRestrictedCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= latestTimeRestricted);

        }


        [TestMethod]
        [ExpectedException(typeof(CustomErrors), "Procrasting past the deadline should be generated.")]
        public void procrastinateSinglePastDeadline()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddDays(7);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser,duration, new Repetition(), start, end, 1, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            CalendarEvent testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            Schedule = new TestSchedule(user, refNow);
            TimeSpan procrastinationSpan = TimeSpan.FromDays(10);
            var procrassinateResult = Schedule.ProcrastinateJustAnEvent(testEvent.AllSubEvents.First().getId, procrastinationSpan);
            Assert.IsNotNull(procrassinateResult.Item1);
        }

        [TestMethod]
        public void procrastinateAll()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            //DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset refNow = TestUtility.parseAsUTC("11:15AM 12/27/2018");// try 18:15AM 12/27/2018
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);

            TimeLine repeatTimeLine = new TimeLine(start, end.AddDays(21));
            TimeLine calTimeLine = new TimeLine(start, start.Add(duration));
            Repetition repetition = new Repetition(repeatTimeLine, Repetition.Frequency.WEEKLY, calTimeLine, new DayOfWeek[] {DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Friday});

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser,duration, repetition, start, end, 2, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB().Wait();
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            TimeSpan procrastinationSpan = TimeSpan.FromHours(2);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> procrastinateResult = Schedule.ProcrastinateAll(procrastinationSpan);
            Assert.IsNull(procrastinateResult.Item1);
            Schedule.persistToDB().Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            EventID procrastinateId = new EventID(user.getTilerUser().ClearAllId);
            CalendarEvent procrastinationEvent = TestUtility.getCalendarEventById(procrastinateId.getCalendarEventID(), user);
            DateTimeOffset endOfProcrastinateAll = refNow.Add(procrastinationSpan);
            Assert.AreEqual(procrastinationEvent.End, endOfProcrastinateAll);
            SubCalendarEvent lastFreeSpaceBlock = procrastinationEvent.AllSubEvents.OrderByDescending(obj => obj.End).First();
            Assert.AreEqual(lastFreeSpaceBlock.End, endOfProcrastinateAll);
            Assert.IsNotNull(procrastinationEvent);
            CalendarEvent testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            DateTimeOffset earliestTImeForNonProcrastinationSubEventTime = refNow.Add(procrastinationSpan);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= earliestTImeForNonProcrastinationSubEventTime);
            int numberOfBlockedOfTimeSlots = procrastinationEvent.ActiveSubEvents.Length;

            // Procrastinate  all that over laps. RefNow is  1 hour after the previous refnow. There should be just one single subevent during overlaps

            DateTimeOffset refNow0 = refNow.AddHours(1);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow0);
            TimeSpan additionalProcrastinationSpan = TimeSpan.FromHours(2);
            procrastinateResult = Schedule.ProcrastinateAll(additionalProcrastinationSpan);
            Assert.IsNull(procrastinateResult.Item1);
            Schedule.persistToDB().Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow0);
            procrastinateId = new EventID(user.getTilerUser().ClearAllId);
            procrastinationEvent = TestUtility.getCalendarEventById(procrastinateId.getCalendarEventID(), user);
            endOfProcrastinateAll = refNow0.Add(additionalProcrastinationSpan);
            Assert.AreEqual(procrastinationEvent.End, endOfProcrastinateAll);
            lastFreeSpaceBlock = procrastinationEvent.AllSubEvents.OrderByDescending(obj => obj.End).First();
            Assert.AreEqual(lastFreeSpaceBlock.End, endOfProcrastinateAll);
            Assert.IsNotNull(procrastinationEvent);
            testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            earliestTImeForNonProcrastinationSubEventTime = refNow0.Add(additionalProcrastinationSpan);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= earliestTImeForNonProcrastinationSubEventTime);
            Assert.AreEqual(procrastinationEvent.ActiveSubEvents.Length, numberOfBlockedOfTimeSlots);

            /// if the user choses to use the edit fields as opposed to the normal procrastinate scroll wheel. And the edit range is less than the already established range of procrastinate calendar sub events
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow0);
            DateTimeOffset startOfProcrastinateAll = refNow.AddHours(.5);
            DateTimeOffset newEndOfProcrastinateAll = refNow.AddHours(2.5);
            procrastinationEvent = TestUtility.getCalendarEventById(user.getTilerUser().getClearAllEventsId(), user);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            procrastinationEvent = TestUtility.getCalendarEventById(user.getTilerUser().getClearAllEventsId(), user);
            SubCalendarEvent firstClearedBlock = procrastinationEvent.ActiveSubEvents.OrderBy(obj => obj.Start).First();
            procrastinateResult = Schedule.BundleChangeUpdate(firstClearedBlock.getId, procrastinationEvent.getName, startOfProcrastinateAll, newEndOfProcrastinateAll, startOfProcrastinateAll, newEndOfProcrastinateAll, procrastinationEvent.NumberOfSplit, procrastinationEvent.Notes.UserNote);

            Assert.IsNull(procrastinateResult.Item1);
            Schedule.persistToDB().Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow0);
            procrastinateId = new EventID(user.getTilerUser().ClearAllId);
            procrastinationEvent = TestUtility.getCalendarEventById(procrastinateId.getCalendarEventID(), user);
            endOfProcrastinateAll = newEndOfProcrastinateAll;
            Assert.AreEqual(procrastinationEvent.End, endOfProcrastinateAll);
            lastFreeSpaceBlock = procrastinationEvent.AllSubEvents.OrderByDescending(obj => obj.End).First();
            Assert.AreEqual(lastFreeSpaceBlock.End, endOfProcrastinateAll);
            Assert.IsNotNull(procrastinationEvent);
            testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            earliestTImeForNonProcrastinationSubEventTime = endOfProcrastinateAll;
            Assert.IsTrue(testEventCopy.ActiveSubEvents.OrderBy(subEvent => subEvent.Start).First().Start >= earliestTImeForNonProcrastinationSubEventTime);
            Assert.AreEqual(procrastinationEvent.ActiveSubEvents.Length, numberOfBlockedOfTimeSlots);


            // Procrastinate all if there are no overlaps then we should have multiple active all events
            DateTimeOffset refNow1 = refNow.AddHours(4);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow1);
            
            TimeSpan additionalProcrastinationSpan0 = TimeSpan.FromHours(1);
            procrastinateResult = Schedule.ProcrastinateAll(additionalProcrastinationSpan0);
         
            Assert.IsNull(procrastinateResult.Item1);
            Task updateWait = Schedule.persistToDB();
            
            updateWait.Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow1);
            procrastinateId = new EventID(user.getTilerUser().ClearAllId);
            procrastinationEvent = TestUtility.getCalendarEventById(procrastinateId.getCalendarEventID(), user);
            endOfProcrastinateAll = refNow1.Add(additionalProcrastinationSpan0);
            Assert.AreEqual(procrastinationEvent.End, endOfProcrastinateAll);
            lastFreeSpaceBlock = procrastinationEvent.AllSubEvents.OrderByDescending(obj => obj.End).First();
            Assert.AreEqual(lastFreeSpaceBlock.End, endOfProcrastinateAll);
            Assert.IsNotNull(procrastinationEvent);
            Assert.AreEqual(procrastinationEvent.ActiveSubEvents.Length, numberOfBlockedOfTimeSlots + 1);


            /// if the user choses to use the edit fields as opposed to the normal procrastinate scroll wheel. And the edit is creates a conflict in procrastinateall subcaledar events. It should generaate one contigous block
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow1);
            startOfProcrastinateAll = refNow.AddHours(.5);
            newEndOfProcrastinateAll = refNow.AddHours(4.5);
            procrastinationEvent = TestUtility.getCalendarEventById(user.getTilerUser().getClearAllEventsId(), user);
            DateTimeOffset desiredEndTIme = procrastinationEvent.End;
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            procrastinationEvent = TestUtility.getCalendarEventById(user.getTilerUser().getClearAllEventsId(), user);
            firstClearedBlock = procrastinationEvent.ActiveSubEvents.OrderBy(obj => obj.Start).First();
            procrastinateResult = Schedule.BundleChangeUpdate(firstClearedBlock.getId, procrastinationEvent.getName, startOfProcrastinateAll, newEndOfProcrastinateAll, startOfProcrastinateAll, newEndOfProcrastinateAll, procrastinationEvent.NumberOfSplit, firstClearedBlock.Notes.UserNote);

            Assert.IsNull(procrastinateResult.Item1);
            Schedule.persistToDB().Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow0);
            procrastinateId = new EventID(user.getTilerUser().ClearAllId);
            procrastinationEvent = TestUtility.getCalendarEventById(procrastinateId.getCalendarEventID(), user);
            endOfProcrastinateAll = newEndOfProcrastinateAll;
            Assert.AreEqual(procrastinationEvent.End, desiredEndTIme);
            lastFreeSpaceBlock = procrastinationEvent.AllSubEvents.OrderByDescending(obj => obj.End).First();
            Assert.AreEqual(lastFreeSpaceBlock.End, desiredEndTIme);
            Assert.IsNotNull(procrastinationEvent);
            testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            earliestTImeForNonProcrastinationSubEventTime = desiredEndTIme;
            Assert.IsTrue(testEventCopy.ActiveSubEvents.First().Start >= earliestTImeForNonProcrastinationSubEventTime);
            Assert.AreEqual(procrastinationEvent.ActiveSubEvents.Length, numberOfBlockedOfTimeSlots);

        }

        [TestMethod]
        public void procrastinateAll_ClearsCurrentEvents()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            //DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset refNow = TestUtility.parseAsUTC("11:15AM 12/27/2018");// try 18:15AM 12/27/2018
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);

            TimeLine repeatTimeLine = new TimeLine(start, end.AddDays(21));
            TimeLine calTimeLine = new TimeLine(start, start.Add(duration));
            Repetition repetition = new Repetition(repeatTimeLine, Repetition.Frequency.WEEKLY, calTimeLine, new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Friday });

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 2, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset secondRefNow = refNow.AddHours(4);
            Schedule = new TestSchedule(user, secondRefNow);
            Schedule.SetCalendarEventAsNow(testEvent.Id);
            Schedule.persistToDB().Wait();
            CalendarEvent testEvent_DB = TestUtility.getCalendarEventById(testEvent.Id, user);
            SubCalendarEvent subEvent = testEvent_DB.ActiveSubEvents.OrderBy(o => o.Start).First();
            Assert.AreEqual(subEvent.Start, secondRefNow);


            DateTimeOffset thirdRefNow = subEvent.Start.Add(TimeSpan.FromMinutes(Math.Round(subEvent.getActiveDuration.TotalMinutes / 2)));
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, thirdRefNow);
            TimeSpan procrastinateSpan = TimeSpan.FromHours(3);
            Schedule.ProcrastinateAll(procrastinateSpan);
            Schedule.persistToDB().Wait();

            DateTimeOffset newBeginningTime = thirdRefNow.Add(procrastinateSpan);
            testEvent_DB = TestUtility.getCalendarEventById(testEvent.Id, user);
            foreach(SubCalendarEvent retrievedSubEvent in testEvent_DB.ActiveSubEvents.OrderBy(o => o.Start))
            {
                Assert.IsTrue(retrievedSubEvent.Start >= newBeginningTime);
            }
        }


        [TestMethod]
        public void scheduleModificationWithProcrastinateAll()
        {
            DB_Schedule Schedule;
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
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB().Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            TimeSpan procrastinationSpan = TimeSpan.FromHours(4.5);
            var procrassinateResult = Schedule.ProcrastinateAll(procrastinationSpan);
            Assert.IsNull(procrassinateResult.Item1);
            Schedule.persistToDB().Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            EventID procrastinateId = new EventID(user.getTilerUser().ClearAllId);
            CalendarEvent procrastinationEvent = TestUtility.getCalendarEventById(procrastinateId.ToString(), user);
            Assert.IsNotNull(procrastinationEvent);
            CalendarEvent testEventCopy = TestUtility.getCalendarEventById(testEvent.getId, user);
            DateTimeOffset latestTime = refNow.Add(procrastinationSpan);
            Assert.IsTrue(testEventCopy.ActiveSubEvents.First().Start >= latestTime);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent0 = TestUtility.generateCalendarEvent(tilerUser,duration, new Repetition(), start, end.AddDays(1), 2, false);
            Schedule.AddToScheduleAndCommitAsync(testEvent0).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser,duration, new Repetition(), start, end.AddDays(1), 2, false);
            Schedule.AddToScheduleAndCommitAsync(testEvent1).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow.AddHours(5));
            Schedule.FindMeSomethingToDo(new Location()).Wait();
            Schedule.WriteFullScheduleToLog().Wait();
        }
    }
}
