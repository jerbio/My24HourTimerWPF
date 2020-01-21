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
    public class SetEventAsNowTests
    {
        [TestMethod]
        public void setCalendarEventAsNow()
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
            Schedule = new TestSchedule(user, refNow, retrievalOption: DataRetrivalOption.All);
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB().Wait();
            testEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            Assert.IsTrue(testEvent.ActiveSubEvents.OrderBy(subevent => subevent.Start).First().Start == refNow);
        }
        [TestMethod]
        public void setCalendarEventAsNowPastDeadline()
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
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 2, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            DateTimeOffset newRefNow = end.AddDays(1);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, newRefNow, retrievalOption: DataRetrivalOption.All);
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB().Wait();
            testEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            SubCalendarEvent latesSubEvent = testEvent.ActiveSubEvents.OrderBy(subevent => subevent.Start).Last();
            Assert.IsTrue(latesSubEvent.Start == newRefNow);
            Assert.IsTrue(latesSubEvent.End == testEvent.End);
        }

        [TestMethod]
        public void setSubCalendarEventAsNow()
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
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 3, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents[1];
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            var setAsNowResult = Schedule.SetSubeventAsNow(subEvent.Id);
            Schedule.persistToDB().Wait();
            SubCalendarEvent testSubEvent = TestUtility.getSubEventById(subEvent.getId, user);
            Assert.IsTrue(testSubEvent.Start == refNow);
        }
        [TestMethod]
        public void setSubCalendarEventAsNowPastDeadline()
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
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 3, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents[1];
            DateTimeOffset newRefNow = end.AddDays(1);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, newRefNow);
            var setAsNowResult = Schedule.SetSubeventAsNow(subEvent.Id, true);
            Schedule.persistToDB().Wait();
            subEvent = Schedule.getCalendarEvent(subEvent.getId).ActiveSubEvents.OrderBy(obj => obj.Start).Last();// this sorting is needed because of the extra sorting by id that occurs on the front end to ensure ids are sorted alphabetically
            SubCalendarEvent testSubEvent = TestUtility.getSubEventById(subEvent.getId, user);
            testEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            Assert.IsTrue(testSubEvent.Start == newRefNow);
            Assert.IsTrue(testSubEvent.End == testEvent.End);
            Assert.IsFalse(testEvent.End == end);
        }


        [TestMethod]
        public void discardSubCalendarEventAsNow()
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
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 3, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents[1];
            CalendarEvent beforeSetAsNowCalendarEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            DateTimeOffset newRefNow = end.AddDays(1);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, newRefNow);
            var setAsNowResult = Schedule.SetSubeventAsNow(subEvent.Id, true);
            Schedule.persistToDB(false).Wait();
            SubCalendarEvent testSubEvent = TestUtility.getSubEventById(subEvent.getId, user);
            CalendarEvent afterSetAsNowCalendarEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            Assert.IsTrue( testSubEvent.isTestEquivalent(subEvent));
            Assert.IsTrue(afterSetAsNowCalendarEvent.isTestEquivalent(beforeSetAsNowCalendarEvent));
        }


        [TestMethod]
        public void discardCalendarEventAsNow()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddHours(7);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 3, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            CalendarEvent beforeSetAsNowCalendarEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            DateTimeOffset newRefNow = end.AddDays(1);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            HashSet<string> calendarIds = new HashSet<string>() { testEvent.Id };
            Schedule = new TestSchedule(user, newRefNow, calendarIds: calendarIds);
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB(false).Wait();
            CalendarEvent afterSetAsNowCalendarEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            Assert.IsTrue(afterSetAsNowCalendarEvent.isTestEquivalent(beforeSetAsNowCalendarEvent));
        }
        [TestMethod]
        public void CalendarEventRestrictedAsNow()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition();
            TimeSpan fullDuration = duration + duration;
            var restrictionProfile = new RestrictionProfile(start, fullDuration);
            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, restrictionProfile: restrictionProfile, now: Schedule.Now) as CalendarEventRestricted;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            DateTimeOffset newRefNow = end.AddHours(1);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            HashSet<string> calendarIds = new HashSet<string>() { testEvent.Id };
            Schedule = new TestSchedule(user, newRefNow, calendarIds: calendarIds);
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB().Wait();
            CalendarEvent afterSetAsNowCalendarEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            SubCalendarEvent nowSubEvent = afterSetAsNowCalendarEvent.AllSubEvents.SingleOrDefault(subEvent => subEvent.Start == newRefNow);
            Assert.IsNotNull(nowSubEvent);
        }

        [TestMethod]
        public void SubCalendarEventRestrictedAsNow()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition();
            var restrictionProfile = new RestrictionProfile(start, duration + duration);
            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, restrictionProfile: restrictionProfile, now: Schedule.Now) as CalendarEventRestricted;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            DateTimeOffset newRefNow = start.AddHours(1);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, newRefNow);
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents.First();
            var setAsNowResult = Schedule.SetSubeventAsNow(subEvent.getId);
            Schedule.persistToDB().Wait();
            SubCalendarEvent subCalendarEventNow = TestUtility.getSubEventById(subEvent.getId, user);
            Assert.IsTrue(subCalendarEventNow.Start == newRefNow);
        }

        [TestMethod]
        public void CalendarEventRestrictedAsNowOutsideRestrictedFrame()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DateTimeOffset end = repetitionRange.End.AddDays(14);
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition();
            TimeSpan fullDuration = duration + duration;
            var restrictionProfile = new RestrictionProfile(start.AddHours(2), fullDuration);
            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, restrictionProfile: restrictionProfile, now: Schedule.Now) as CalendarEventRestricted;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            DateTimeOffset newRefNow = start.AddHours(1);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            HashSet<string> calendarIds = new HashSet<string>() { testEvent.Id };
            Schedule = new TestSchedule(user, newRefNow, calendarIds: calendarIds);
            var setAsNowResult = Schedule.SetCalendarEventAsNow(testEvent.getId);
            Schedule.persistToDB().Wait();
            CalendarEvent afterSetAsNowCalendarEvent = TestUtility.getCalendarEventById(testEvent.getId, user);
            SubCalendarEvent nowSubEvent = afterSetAsNowCalendarEvent.AllSubEvents.SingleOrDefault(subEvent => subEvent.Start == newRefNow);
            Assert.IsNotNull(nowSubEvent);
        }

        [TestMethod]
        public void SubsCalendarEventRestrictedAsNowOutsideRestrictedFrame()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DateTimeOffset end = repetitionRange.End.AddDays(14);
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition();
            TimeSpan fullDuration = duration + duration;
            var restrictionProfile = new RestrictionProfile(start.AddHours(2), fullDuration);
            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, restrictionProfile: restrictionProfile, now: Schedule.Now) as CalendarEventRestricted;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            DateTimeOffset newRefNow = start.AddHours(1);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, newRefNow);
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents.First();
            var setAsNowResult = Schedule.SetSubeventAsNow(subEvent.getId, true);
            Schedule.persistToDB().Wait();
            SubCalendarEvent subCalendarEventNow = TestUtility.getSubEventById(subEvent.getId, user);
            Assert.IsTrue(subCalendarEventNow.Start == newRefNow);
        }


#if RunSlowTest
        [TestMethod]
#endif
        public void CalendarEventAsNowShouldUseFirstSubEventAfterNow()
        {
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset iniRefNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            DateTimeOffset refNow = iniRefNow;
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DateTimeOffset end = repetitionRange.End.AddDays(14);
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Location location = new Location();

            List<CalendarEvent> calEvents = TestUtility.generateAllCalendarEvent(Schedule, duration, start, tilerUser, user, 2);
            foreach(CalendarEvent calEVent in calEvents)
            {
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                Schedule = new TestSchedule(user, refNow);
                Schedule.FindMeSomethingToDo(location).Wait();
                Schedule.persistToDB().Wait();
                /// now is refNow
                refNow = iniRefNow;
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                CalendarEvent calEventRetrieved = TestUtility.getCalendarEventById(calEVent.Id, user);
                HashSet<string> calendarIds = new HashSet<string>() { calEventRetrieved.Id };
                Schedule = new TestSchedule(user, refNow, calendarIds: calendarIds);
                Schedule.SetCalendarEventAsNow(calEventRetrieved.Id);
                Schedule.persistToDB().Wait();
                calEventRetrieved = TestUtility.getCalendarEventById(calEVent.Id, user);
                SubCalendarEvent subEVent = calEventRetrieved.ActiveSubEvents.Where(o => o.Start >= Schedule.Now.constNow).OrderBy(o => o.End).First();
                Assert.AreEqual(subEVent.Start, refNow);

                /// now is After first
                refNow = iniRefNow;
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                calEventRetrieved = TestUtility.getCalendarEventById(calEVent.Id, user);
                List<SubCalendarEvent> activeSubEVents = calEventRetrieved.ActiveSubEvents.OrderBy(sub=> sub.End).ToList();
                if (activeSubEVents.Count > 1)
                {
                    refNow = activeSubEVents[0].End.AddMinutes(1);
                    TestUtility.reloadTilerUser(ref user, ref tilerUser);
                    calendarIds = new HashSet<string>() { calEventRetrieved.Id };
                    Schedule = new TestSchedule(user, refNow, calendarIds: calendarIds);
                    Schedule.SetCalendarEventAsNow(calEventRetrieved.Id);
                    Schedule.persistToDB().Wait();
                    subEVent = activeSubEVents[1];
                    subEVent = TestUtility.getSubEventById(subEVent.Id, user);
                    Assert.AreEqual(subEVent.Start, refNow);
                } else
                {
                    refNow = activeSubEVents[0].End.AddMinutes(1);
                    TestUtility.reloadTilerUser(ref user, ref tilerUser);
                    calendarIds = new HashSet<string>() { calEventRetrieved.Id };
                    Schedule = new TestSchedule(user, refNow, calendarIds: calendarIds);
                    Schedule.SetCalendarEventAsNow(calEventRetrieved.Id);
                    Schedule.persistToDB().Wait();
                    subEVent = activeSubEVents[0];
                    subEVent = TestUtility.getSubEventById(subEVent.Id, user);
                    Assert.AreEqual(subEVent.Start, refNow);
                }

                /// now is After End of Last Subevent
                refNow = iniRefNow;
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                calEventRetrieved = TestUtility.getCalendarEventById(calEVent.Id, user);
                activeSubEVents = calEventRetrieved.ActiveSubEvents.OrderBy(sub => sub.End).ToList();
                if (activeSubEVents.Count > 1)
                {
                    refNow = activeSubEVents.Last().End.AddMinutes(1);
                    TestUtility.reloadTilerUser(ref user, ref tilerUser);
                    calendarIds = new HashSet<string>() { calEventRetrieved.Id };
                    Schedule = new TestSchedule(user, refNow, calendarIds: calendarIds);
                    Schedule.SetCalendarEventAsNow(calEventRetrieved.Id);
                    Schedule.persistToDB().Wait();
                    subEVent = activeSubEVents.Last();
                    subEVent = TestUtility.getSubEventById(subEVent.Id, user);
                    Assert.AreEqual(subEVent.Start, refNow);
                }
                else
                {
                    refNow = activeSubEVents.Last().End.AddMinutes(1);
                    TestUtility.reloadTilerUser(ref user, ref tilerUser);
                    calendarIds = new HashSet<string>() { calEventRetrieved.Id };
                    Schedule = new TestSchedule(user, refNow, calendarIds: calendarIds);
                    Schedule.SetCalendarEventAsNow(calEventRetrieved.Id);
                    Schedule.persistToDB().Wait();
                    subEVent = activeSubEVents[0];
                    subEVent = TestUtility.getSubEventById(subEVent.Id, user);
                    Assert.AreEqual(subEVent.Start, refNow);
                }


            }
        }

        /// <summary>
        /// Test ensures that when set as now is called there is a bumper for travel time.
        /// This also test agains shuffle too.
        /// </summary>
        [TestMethod]
        public void setSubCalendarEventAsNowSpacing()
        {
            TestUtility.initializeLocationApi();
            Location currentLocation = new Location("3333 Walnut Rd Boulder, CO");
            currentLocation.verify();
            Location homeLocation = new Location("413 summit blvd broomfield CO");
            homeLocation.verify();
            Location tangerineLocation = new Location("300 S Public Rd, Lafayette, CO 80026");
            tangerineLocation.verify();
            TestSchedule Schedule;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.removeSecondsAndMilliseconds();
            DateTimeOffset endOfDay = new DateTimeOffset(1, 1, 1, 20, 0, 0, new TimeSpan());
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddDays(14);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 3, false, location: homeLocation);
            Schedule = new TestSchedule(user, refNow, endOfDay);
            Schedule.CurrentLocation = currentLocation;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();

            CalendarEvent testEvent_DB = TestUtility.getCalendarEventById(testEvent.Id, user);
            SubCalendarEvent firstSubEvent = testEvent_DB.OrderByStartActiveSubEvents.First();
            TimeSpan suggestingDriviingSpan = TimeSpan.FromMinutes(14);// it usually takes over 14 minutes to drive to work so this should handle the drive time
            TimeSpan evaluatedDrivingSpan = firstSubEvent.Start - refNow;
            Assert.IsTrue(evaluatedDrivingSpan > suggestingDriviingSpan);



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset secondRefnow = refNow.AddHours(1);
            Schedule = new TestSchedule(user, secondRefnow, endOfDay);
            Schedule.FindMeSomethingToDo(currentLocation).Wait();
            Schedule.persistToDB().Wait();

            CalendarEvent testEvent_DB0 = TestUtility.getCalendarEventById(testEvent.Id, user);
            SubCalendarEvent firstSubEVentAfterShuffle = testEvent_DB0.OrderByStartActiveSubEvents. Where(sub => sub.Start >= secondRefnow).First();
            evaluatedDrivingSpan = firstSubEVentAfterShuffle.Start - secondRefnow;
            Assert.IsTrue(evaluatedDrivingSpan > suggestingDriviingSpan);



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset thirdRefnow = refNow.AddHours(2);
            SubCalendarEvent setAsNowSubEvent = firstSubEVentAfterShuffle;
            Schedule = new TestSchedule(user, thirdRefnow, endOfDay);
            Schedule.CurrentLocation = currentLocation;
            Schedule.SetSubeventAsNow(setAsNowSubEvent.Id);
            Schedule.persistToDB().Wait();

            CalendarEvent testEvent_DB1 = TestUtility.getCalendarEventById(testEvent.Id, user);
            SubCalendarEvent firstSubEventAfterSetAsNow = testEvent_DB1.OrderByStartActiveSubEvents.Where(sub => sub.Start >= thirdRefnow).First();
            evaluatedDrivingSpan = firstSubEventAfterSetAsNow.Start - thirdRefnow;
            Assert.IsTrue(evaluatedDrivingSpan > suggestingDriviingSpan);


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset refNowAfter3Days = Schedule.Now.firstDay.End.AddDays(4);
            Schedule = new TestSchedule(user, refNowAfter3Days, endOfDay);
            Schedule.CurrentLocation = currentLocation;
            Schedule.FindMeSomethingToDo(tangerineLocation).Wait();
            Schedule.persistToDB().Wait();
            SubCalendarEvent subEvent = Schedule.getAllActiveSubEvents().OrderBy(sub => sub.Start).Where(o => o.Start > refNowAfter3Days).First();
            DateTimeOffset earliestTIme = Schedule.Now.firstDay.Start + Utility.SleepSpan + Schedule.MorningPreparationTime;
            Assert.IsTrue(subEvent.Start >= earliestTIme);


        }
    }
}
