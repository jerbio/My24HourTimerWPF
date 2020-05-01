using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerElements;
using System.Collections.Generic;
using System.Linq;
using TilerCore;
using TilerFront;
using My24HourTimerWPF;

namespace TilerTests
{
    [TestClass]
    public class ScheduleQuality
    {
        [TestMethod]
        public void SingleEventDayPreferencePicker()
        {
            DB_Schedule Schedule;
            int splitCount = 8;
            DateTimeOffset refNow = DateTimeOffset.UtcNow.Date;
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, 1, 0, 0, 0, new TimeSpan());
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddDays(28);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent = TestUtility
                .generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, splitCount, false);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();

            int firstDayIncrement = 2;
            int secondDayIncrement = 4;

            DateTimeOffset firstDayFromStart = refNow.AddDays(firstDayIncrement);
            DateTimeOffset secondDayFromStart = refNow.AddDays(secondDayIncrement);

            DayOfWeek firstDayOfWeek = firstDayFromStart.DayOfWeek;
            DayOfWeek secondDayOfWeek = secondDayFromStart.DayOfWeek;

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            refNow = firstDayFromStart;
            Schedule = new TestSchedule(user, refNow, includeUpdateHistory: true);
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents.First();
            Schedule.SetSubeventAsNow(subEvent.Id);
            Schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            Schedule.markSubEventAsComplete(subEvent.Id).Wait();
            Schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            refNow = secondDayFromStart;
            Schedule = new TestSchedule(user, refNow, includeUpdateHistory: true);
            testEvent = TestUtility.getCalendarEventById(testEvent.Id, user);
            subEvent = testEvent.ActiveSubEvents.First();
            testEvent = Schedule.getCalendarEvent(testEvent.Id);
            Schedule.SetSubeventAsNow(subEvent.Id);
            Schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            testEvent = Schedule.getCalendarEvent(testEvent.Id);
            subEvent = testEvent.ActiveSubEvents.First();
            Schedule.markSubEventAsComplete(subEvent.Id).Wait();
            Schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            Schedule.FindMeSomethingToDo(new Location()).Wait();
            testEvent = Schedule.getCalendarEvent(testEvent.Id);
            Schedule.persistToDB().Wait();

            List<SubCalendarEvent> subEvents = Schedule.getCalendarEvent(testEvent.Id).ActiveSubEvents.Where(sub => sub.End > refNow).ToList();
            List<DayOfWeek> daysOfWeek = subEvents.Select(tilerEvent => tilerEvent.Start.DayOfWeek).ToList();
            int count = 0;
            foreach(DayOfWeek weekDay in daysOfWeek )
            {
                if(weekDay == firstDayOfWeek || weekDay == secondDayOfWeek)
                {
                    count++;
                }
            }

            Assert.AreEqual(count, subEvents.Count);
        }
        [TestMethod]
        public void RepetitionDayPreferencePicker()
        {
            DB_Schedule Schedule;
            int splitCount = 8;
            DateTimeOffset refNow = TestUtility.parseAsUTC("7/7/2019 12:00:00 AM");
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddDays(28);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent0 = TestUtility
                .generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, splitCount, false);
            Schedule.AddToScheduleAndCommitAsync(testEvent0).Wait();

            TimeLine repeatTimeLine = new TimeLine(testEvent0.Start, testEvent0.End.AddDays(14));
            TimeLine calTimeLine = repeatTimeLine.CreateCopy();
            Repetition repetition = new Repetition(repeatTimeLine, Repetition.Frequency.WEEKLY, calTimeLine);
            int repeatSplitCount = 2;
            CalendarEvent testEvent = TestUtility
                .generateCalendarEvent(tilerUser, duration, repetition, calTimeLine.Start, calTimeLine.End, repeatSplitCount, false);
            List<CalendarEvent> recurringCalendarEvents = testEvent.RecurringCalendarEvents.OrderBy(o => o.Start).ToList();
            int firstDayIncrement = 2;
            int secondDayIncrement = 4;

            DateTimeOffset firstDayFromStart = refNow.AddDays(firstDayIncrement);
            DateTimeOffset secondDayFromStart = refNow.AddDays(secondDayIncrement);

            DayOfWeek firstDayOfWeek = firstDayFromStart.DayOfWeek;
            DayOfWeek secondDayOfWeek = secondDayFromStart.DayOfWeek;
            HashSet<DayOfWeek> repeatDays = new HashSet<DayOfWeek>() { firstDayOfWeek, secondDayOfWeek };

            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            refNow = firstDayFromStart;
            Schedule = new TestSchedule(user, refNow, includeUpdateHistory: true);
            CalendarEvent firstRecurrence = Schedule.getCalendarEvent(recurringCalendarEvents.First().Id);
            SubCalendarEvent subEvent = firstRecurrence.ActiveSubEvents.OrderBy(sub => sub.Start).First();
            Schedule.SetSubeventAsNow(subEvent.Id);
            Schedule.persistToDB().Wait();

            IEnumerable<CalendarEvent> allRepeatingCalevents = Schedule.getAllRelatedCalendarEvents(testEvent.Id);
            NowProfile nowProfile = allRepeatingCalevents.First().getNowInfo;
            EventPreference daypreference = allRepeatingCalevents.First().DayPreference;
            foreach (CalendarEvent calEvent in allRepeatingCalevents)
            {
                Assert.AreEqual(calEvent.getNowInfo, nowProfile);
                Assert.AreEqual(calEvent.getNowInfo.Id, nowProfile.Id);
                Assert.AreEqual(calEvent.NowProfileId, nowProfile.Id);
                Assert.AreEqual(calEvent.DayPreference, daypreference);
                Assert.AreEqual(calEvent.DayPreference.Id, daypreference.Id);
                Assert.AreEqual(calEvent.DayPreferenceId, daypreference.Id);
            }


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            firstRecurrence = Schedule.getCalendarEvent(firstRecurrence.Id);
            Schedule.markSubEventAsComplete(subEvent.Id).Wait();
            Schedule.persistToDB().Wait();

            allRepeatingCalevents = Schedule.getAllRelatedCalendarEvents(testEvent.Id);
            nowProfile = allRepeatingCalevents.First().getNowInfo;
            daypreference = allRepeatingCalevents.First().DayPreference;
            foreach (CalendarEvent calEvent in allRepeatingCalevents)
            {
                Assert.AreEqual(calEvent.getNowInfo, nowProfile);
                Assert.AreEqual(calEvent.getNowInfo.Id, nowProfile.Id);
                Assert.AreEqual(calEvent.NowProfileId, nowProfile.Id);
                Assert.AreEqual(calEvent.DayPreference, daypreference);
                Assert.AreEqual(calEvent.DayPreference.Id, daypreference.Id);
                Assert.AreEqual(calEvent.DayPreferenceId, daypreference.Id);
            }

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            refNow = secondDayFromStart;
            Schedule = new TestSchedule(user, refNow, includeUpdateHistory: true);
            CalendarEvent nextOccurence = Schedule.getAllActiveSubEvents().OrderBy(o=>o.Start).First().ParentCalendarEvent;
            subEvent = nextOccurence.ActiveSubEvents.OrderBy(sub => sub.Start).First();
            
            Schedule.SetSubeventAsNow(subEvent.Id);
            Schedule.persistToDB().Wait();

            allRepeatingCalevents = Schedule.getAllRelatedCalendarEvents(testEvent.Id);
            nowProfile = allRepeatingCalevents.First().getNowInfo;
            daypreference = allRepeatingCalevents.First().DayPreference;
            foreach (CalendarEvent calEvent in allRepeatingCalevents)
            {
                Assert.AreEqual(calEvent.getNowInfo, nowProfile);
                Assert.AreEqual(calEvent.getNowInfo.Id, nowProfile.Id);
                Assert.AreEqual(calEvent.NowProfileId, nowProfile.Id);
                Assert.AreEqual(calEvent.DayPreference, daypreference);
                Assert.AreEqual(calEvent.DayPreference.Id, daypreference.Id);
                Assert.AreEqual(calEvent.DayPreferenceId, daypreference.Id);
            }


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            nextOccurence = Schedule.getCalendarEvent(nextOccurence.Id);
            subEvent = nextOccurence.ActiveSubEvents.OrderBy(sub => sub.Start).First();
            Schedule.markSubEventAsComplete(subEvent.Id).Wait();
            Schedule.persistToDB().Wait();

            allRepeatingCalevents = Schedule.getAllRelatedCalendarEvents(testEvent.Id);
            nowProfile = allRepeatingCalevents.First().getNowInfo;
            daypreference = allRepeatingCalevents.First().DayPreference;
            foreach (CalendarEvent calEvent in allRepeatingCalevents)
            {
                Assert.AreEqual(calEvent.getNowInfo, nowProfile);
                Assert.AreEqual(calEvent.getNowInfo.Id, nowProfile.Id);
                Assert.AreEqual(calEvent.NowProfileId, nowProfile.Id);
                Assert.AreEqual(calEvent.DayPreference, daypreference);
                Assert.AreEqual(calEvent.DayPreference.Id, daypreference.Id);
                Assert.AreEqual(calEvent.DayPreferenceId, daypreference.Id);
            }



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            Schedule.FindMeSomethingToDo(new Location()).Wait();
            IEnumerable<CalendarEvent> calEvents = Schedule.getAllRelatedCalendarEvents(nextOccurence.Id);
            Schedule.persistToDB().Wait();

            allRepeatingCalevents = Schedule.getAllRelatedCalendarEvents(testEvent.Id);
            nowProfile = allRepeatingCalevents.First().getNowInfo;
            daypreference = allRepeatingCalevents.First().DayPreference;
            foreach (CalendarEvent calEvent in allRepeatingCalevents)
            {
                Assert.AreEqual(calEvent.getNowInfo, nowProfile);
                Assert.AreEqual(calEvent.getNowInfo.Id, nowProfile.Id);
                Assert.AreEqual(calEvent.NowProfileId, nowProfile.Id);
                Assert.AreEqual(calEvent.DayPreference, daypreference);
                Assert.AreEqual(calEvent.DayPreference.Id, daypreference.Id);
                Assert.AreEqual(calEvent.DayPreferenceId, daypreference.Id);
            }


            List<SubCalendarEvent> subEvents = calEvents.SelectMany(o =>o.ActiveSubEvents).ToList();
            List<DateTimeOffset> allCorrespondingRepeatDays = new List<DateTimeOffset>();
            List<DateTimeOffset> repeatDates = new List<DateTimeOffset>() { firstDayFromStart, secondDayFromStart };
            TimeLine activeTImeLine = new TimeLine(Schedule.Now.constNow, testEvent.End);

            repeatDates.ForEach((weekDay) =>
            {
                var correspondinWeekDays = getCorrespondingWeekdays(activeTImeLine, weekDay.DayOfWeek);
                allCorrespondingRepeatDays.AddRange(correspondinWeekDays);
            });

            List<DayOfWeek> repeatDaysOfWeek = subEvents.Select(tilerEvent => tilerEvent.Start.DayOfWeek).ToList();


            int repeatCount = 0;
            foreach (DayOfWeek weekDay in repeatDaysOfWeek)
            {
                if (repeatDays.Contains(weekDay))
                {
                    repeatCount++;
                }
            }

            int validatingCount = subEvents.Count < allCorrespondingRepeatDays.Count ? subEvents.Count : allCorrespondingRepeatDays.Count;

            Assert.AreEqual(repeatCount, validatingCount);
        }

        /// <summary>
        /// Function tries to ensure that even you set as now in the evening the schedule will schedule the event in the evening as opposed to any other part of the day sector
        /// </summary>
        [TestMethod]
        public void RepetitionDay_Set_As_Now_DaySetionPrefrence()
        {
            DB_Schedule Schedule;
            int splitCount = 2;
            DateTimeOffset refNow = TestUtility.parseAsUTC("7/7/2019 12:00:00 AM");
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddDays(28);


            TimeLine repeatTimeLine = new TimeLine(start, end.AddDays(14));
            TimeLine calTimeLine = repeatTimeLine.CreateCopy();
            Repetition repetition = new Repetition(repeatTimeLine, Repetition.Frequency.WEEKLY, calTimeLine);

            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent0 = TestUtility
                .generateCalendarEvent(tilerUser, duration, repetition, start, end, splitCount, false);
            Schedule.AddToScheduleAndCommitAsync(testEvent0).Wait();



            var daySections = Schedule.Now.getDaySections(testEvent0.StartToEnd);
            var daySectionTuple = daySections[7];
            DateTimeOffset secondRefNow = daySectionTuple.Item2.Start.Add(TimeSpan.FromSeconds(daySectionTuple.Item2.TimelineSpan.TotalSeconds / 2)).removeSecondsAndMilliseconds();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            HashSet<string>  calendarIds = new HashSet<string>();
            calendarIds.Add(testEvent0.Id);
            Schedule = new TestSchedule(user, secondRefNow, calendarIds: calendarIds, includeUpdateHistory: true);
            Schedule.SetCalendarEventAsNow(testEvent0.Id);
            Schedule.persistToDB().Wait();
            var sectionTuple = daySections[6];


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset start1 = sectionTuple.Item2.End.Add(-duration);
            DateTimeOffset end1 = start1.Add(duration);
            Repetition rigidRepetition = new Repetition(repeatTimeLine, Repetition.Frequency.DAILY, new TimeLine(start1, end1));
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, duration, rigidRepetition, start1, end1, splitCount, true);
            Schedule = new TestSchedule(user, secondRefNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent1).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, secondRefNow);
            Schedule.FindMeSomethingToDo(new Location()).Wait();
            Schedule.persistToDB().Wait();

            CalendarEvent testEvent0Retrieved = TestUtility.getCalendarEventById(testEvent0.Id, user);
            TimeLine optimizedWindow = new TimeLine(Schedule.Now.constNow, Schedule.Now.constNow.AddDays(Schedule.OptimizedDayLimit));

            List<SubCalendarEvent> subEventsAfterNow = testEvent0Retrieved.ActiveSubEvents.Where(sub => sub.ActiveSlot.doesTimeLineInterfere(optimizedWindow)).OrderBy(o => o.Start).ToList();

            for(int i=0; i< subEventsAfterNow.Count;i++)
            {
                SubCalendarEvent subEvent = subEventsAfterNow[i];
                bool isMatchingSector = false;
                var sectorTuples = Schedule.Now.getDaySections(subEvent.StartToEnd);
                foreach(var sectorTuple in sectorTuples)
                {
                    if(daySectionTuple.Item1 == sectorTuple.Item1)
                    {
                        isMatchingSector = true;
                        break;
                    }
                }

                Assert.IsTrue(isMatchingSector);

            }
            
        }




        [TestMethod]
        public void RepetitionMultipleEventWithDifferentDayPreferences()
        {
            DB_Schedule Schedule;
            int splitCount = 8;
            DateTimeOffset refNow = TestUtility.parseAsUTC("7/7/2019 12:00:00 AM");
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, 1, 0, 0, 0, new TimeSpan());
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddDays(28);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent0 = TestUtility
                .generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, splitCount, false);
            Schedule.AddToScheduleAndCommitAsync(testEvent0).Wait();

            TimeLine repeatTimeLine = new TimeLine(testEvent0.Start, testEvent0.End.AddDays(14));
            TimeLine calTimeLine = repeatTimeLine.CreateCopy();
            Repetition repetition = new Repetition(repeatTimeLine, Repetition.Frequency.WEEKLY, calTimeLine);
            int repeatSplitCount = 2;
            CalendarEvent repeatEvent = TestUtility
                .generateCalendarEvent(tilerUser, duration, repetition, calTimeLine.Start, calTimeLine.End, repeatSplitCount, false);

            int firstDayIncrement = 2;
            int secondDayIncrement = 4;
            int thirdDayIncrement = 6;

            DateTimeOffset firstDayFromStart = refNow.AddDays(firstDayIncrement);
            DateTimeOffset secondDayFromStart = refNow.AddDays(secondDayIncrement);
            DateTimeOffset thirdDayFromStart = refNow.AddDays(thirdDayIncrement);
            List<DateTimeOffset> testEvent0Dates = new List<DateTimeOffset>() { firstDayFromStart, secondDayFromStart, thirdDayFromStart };

            DateTimeOffset repeatFirstDayFromStart = refNow.AddDays(firstDayIncrement);
            DateTimeOffset repeatSecondDayFromStart = refNow.AddDays(secondDayIncrement);
            List<DateTimeOffset> repeatDates = new List<DateTimeOffset>() { repeatFirstDayFromStart, repeatSecondDayFromStart };

            DayOfWeek firstDayOfWeek = firstDayFromStart.DayOfWeek;
            DayOfWeek secondDayOfWeek = secondDayFromStart.DayOfWeek;
            DayOfWeek thirdDayOfWeek = thirdDayFromStart.DayOfWeek;
            HashSet<DayOfWeek> eventDays = new HashSet<DayOfWeek>() { firstDayOfWeek, secondDayOfWeek, thirdDayOfWeek };

            DayOfWeek repeatFirstDayOfWeek = repeatFirstDayFromStart.DayOfWeek;
            DayOfWeek repeatSecondDayOfWeek = repeatSecondDayFromStart.DayOfWeek;
            HashSet<DayOfWeek> repeatDays = new HashSet<DayOfWeek>() { repeatFirstDayOfWeek, repeatSecondDayOfWeek };



            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(repeatEvent).Wait();

            for (int i = 0; i < 2; i++)
            {
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                refNow = repeatDates[i];
                repeatEvent = TestUtility.getCalendarEventById(repeatEvent.Id, user);
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                Schedule = new TestSchedule(user, refNow, includeUpdateHistory: true);
                SubCalendarEvent subEvent = repeatEvent.ActiveSubEvents.OrderByDescending(obj => obj.Start).First();// ensures that the latter events get piccked "as now" because we when the final refnow is 7/7/2019 meaning the first calendarevents in recurring calendar events can only fit on the last day so it won't be capable of being optimized
                Schedule.SetSubeventAsNow(subEvent.Id);
                Schedule.persistToDB().Wait();


                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                Schedule = new TestSchedule(user, refNow);
                Schedule.markSubEventAsComplete(subEvent.Id).Wait();
                Schedule.persistToDB().Wait();
            }

            for (int i = 0; i < 3; i++)
            {
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                refNow = testEvent0Dates[i];
                testEvent0 = TestUtility.getCalendarEventById(testEvent0.Id, user);
                Schedule = new TestSchedule(user, refNow, includeUpdateHistory: true);
                SubCalendarEvent subEvent = testEvent0.ActiveSubEvents.First();
                Schedule.SetSubeventAsNow(subEvent.Id);
                Schedule.persistToDB().Wait();


                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                Schedule = new TestSchedule(user, refNow);
                Schedule.markSubEventAsComplete(subEvent.Id).Wait();
                Schedule.persistToDB().Wait();
            }


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            Schedule.FindMeSomethingToDo(new Location()).Wait();
            //repeatEvent = Schedule.getCalendarEvent(repeatEvent.Id);
            Schedule.persistToDB().Wait();

            //CalendarEvent repeatEventRetrieved = Schedule.getCalendarEvent(repeatEvent.Id);
            IEnumerable<CalendarEvent> repeatEventRetrieved = Schedule.getAllRelatedCalendarEvents(repeatEvent.Id);
            //CalendarEvent testEvent0Retrieved = Schedule.getCalendarEvent(testEvent0.Id);

            List<SubCalendarEvent> repeatSubEvents = repeatEventRetrieved.SelectMany(o => o.ActiveSubEvents).ToList();
            List<CalendarEvent> allCalendarEvnts = repeatEvent.Repeat.RecurringCalendarEvents().ToList();
            List<DateTimeOffset> allValidDays = new List<DateTimeOffset>();
            TimeLine activeTImeLine = new TimeLine(Schedule.Now.constNow, repeatEvent.End);
            List<DateTimeOffset> allCorrespondingRepeatDays = new List<DateTimeOffset>();
            repeatDates.ForEach((weekDay) =>
            {
                 var correspondinWeekDays = getCorrespondingWeekdays(activeTImeLine, weekDay.DayOfWeek);
                allCorrespondingRepeatDays.AddRange(correspondinWeekDays);
            });

            List<DateTimeOffset> allCorrespondingTestEvent0Days = new List<DateTimeOffset>();
            testEvent0Dates.ForEach((weekDay) =>
            {
                var correspondinWeekDays = getCorrespondingWeekdays(activeTImeLine, weekDay.DayOfWeek);
                allCorrespondingTestEvent0Days.AddRange(correspondinWeekDays);
            });



            List<DayOfWeek> daysOfWeekOfRepeatEvent = repeatSubEvents.Where(subEvent => subEvent.Start > refNow).Select(tilerEvent => tilerEvent.Start.DayOfWeek).ToList();

            List<SubCalendarEvent> testSubEvents = Schedule.getCalendarEvent(testEvent0.Id).ActiveSubEvents.ToList();
            List<DayOfWeek> daysOfWeekTestEvent0 = testSubEvents.Where(subEvent => subEvent.Start > refNow).Select(tilerEvent => tilerEvent.Start.DayOfWeek).ToList();
            
            foreach (DayOfWeek weekDay in daysOfWeekOfRepeatEvent)
            {
                Assert.IsTrue(repeatDays.Contains(weekDay));
            }
        }

        [TestMethod]
        public void InterfereWithNowEventsShouldStayInSameLocation()
        {
            var packet = TestUtility.CreatePacket();
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;

            DateTimeOffset iniRefNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            DateTimeOffset refNow = iniRefNow;

            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            DateTimeOffset end = start.AddHours(4);
            int splitCount = 2;
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent testEvent = TestUtility
                .generateCalendarEvent(tilerUser, duration, null, start, end, splitCount, false);
            TestSchedule schedule = new TestSchedule(user, refNow);
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            testEvent = TestUtility.getCalendarEventById(testEvent.Id, user);
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents.OrderBy(obj => obj.Start).First();
            refNow = subEvent.Start.Add(TimeSpan.FromMinutes(Math.Floor(subEvent.getActiveDuration.TotalMinutes / 2)));


            schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent1 = TestUtility
                .generateCalendarEvent(tilerUser, duration, null, start, end, splitCount, false);
            schedule.AddToScheduleAndCommitAsync(testEvent1).Wait();
            SubCalendarEvent subEventInMemory = schedule.getSubCalendarEvent(subEvent.Id);
            Assert.IsTrue(subEventInMemory.StartToEnd.isEqualStartAndEnd(subEvent.StartToEnd));



            TimeSpan rigidDuration = TimeSpan.FromHours(2);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset rigidStart = refNow.AddHours(4);
            DateTimeOffset rigidEnd = rigidStart.Add(rigidDuration);
            TimeLine timeLine = new TimeLine(rigidStart, rigidStart.AddDays(27));
            Repetition repetition = new Repetition(timeLine, Repetition.Frequency.DAILY, new TimeLine(rigidStart, rigidEnd));
            CalendarEvent rigidEvent1 = TestUtility
                .generateCalendarEvent(tilerUser, rigidDuration, repetition, rigidStart, rigidEnd, splitCount, true);
            schedule = new TestSchedule(user, refNow);
            schedule.AddToScheduleAndCommitAsync(rigidEvent1).Wait();

            DateTimeOffset rigidStart2 = rigidStart.AddMinutes(30);
            DateTimeOffset rigidEnd2 = rigidStart2.Add(rigidDuration);
            TimeLine timeLine2 = new TimeLine(rigidStart2, rigidStart2.AddDays(27));
            Repetition repetition1 = new Repetition(timeLine2, Repetition.Frequency.DAILY, new TimeLine(rigidStart2, rigidEnd2));
            CalendarEvent rigidEvent2 = TestUtility
                .generateCalendarEvent(tilerUser, rigidDuration, repetition1, rigidStart2, rigidEnd2, splitCount, true);
            refNow = rigidStart2;


            schedule = new TestSchedule(user, refNow);
            schedule.AddToScheduleAndCommitAsync(rigidEvent2).Wait();

            schedule = new TestSchedule(user, refNow);
            schedule.FindMeSomethingToDo(new Location()).Wait();


            SubCalendarEvent subEventFromTestEvent = testEvent.ActiveSubEvents.First();
            refNow = refNow.AddMinutes(5);
            schedule = new TestSchedule(user, refNow);
            schedule.markAsCompleteCalendarEventAndReadjust(subEventFromTestEvent.Id).Wait();
            subEventInMemory = schedule.getSubCalendarEvent(subEvent.Id);

        }

        /// <summary>
        /// Test verifies that calendar events with deadlines far out dont get scheduled within a busy chunk of time that is earlier in the schedule
        /// The event named "one percenter" has a deadline that is 19 days out but is scheduled within the "first week" of events. First week in this context means constNow till the first saturday
        /// I say "first saturday" because I tend to schedule events with a saturday deadline. Meaning a lot of events have a saturday as their deadline. Consequentially means the congestions tend to batch up and end on saturdays.
        /// The "first week" has a fairly high occupancy so the event "one percenter" should ideally find a slot outside the first week.
        /// </summary>
        [TestMethod]
        public void file_One_Percent_calevent_Late_Deadline_f06bc15b()
        {
            string scheduleId = "f06bc15b-1b00-435a-8210-e88ad523beda";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            Schedule schedule = scheduleAndDump.Item1;
            schedule.FindMeSomethingToDo(currentLocation).Wait();

            int dayDiff = DayOfWeek.Saturday - schedule.Now.constNow.DayOfWeek;
            DayTimeLine saturdayTImeLine = schedule.Now.getDayTimeLineByTime(schedule.Now.constNow.AddDays(dayDiff));
            TimeLine oneWeekTImeLine = new TimeLine(schedule.Now.constNow, saturdayTImeLine.End);
            DateTimeOffset lastDay = oneWeekTImeLine.End.AddDays(7);
            var subCalendarEvents = schedule.getAllCalendarEvents().SelectMany(cal => cal.ActiveSubEvents).Where(obj => obj.ActiveSlot.doesTimeLineInterfere(oneWeekTImeLine)).Where(obj => obj.ParentCalendarEvent.End > lastDay);
            Assert.IsTrue(subCalendarEvents.Count() == 0);

            ((TestSchedule)schedule).WriteFullScheduleToOutlook();
        }

        /// <summary>
        /// Test verifies that when a subevent is marked as complete for a given day, 
        /// the specific day would be dissentivized to have another subevent for the same day
        /// </summary>
        [TestMethod]
        public void set_subevent_as_complete_reduce_odds_of_calEventSubevent_on_day_of_completion()
        {
            var packet = TestUtility.CreatePacket();
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;

            DateTimeOffset iniRefNow = new DateTimeOffset( DateTimeOffset.UtcNow.removeSecondsAndMilliseconds().Date.ToUniversalTime());
            iniRefNow = new DateTimeOffset(iniRefNow.Year, iniRefNow.Month, iniRefNow.Day, 0, 0, 0, new TimeSpan());
            DateTimeOffset refNow = iniRefNow;

            int splitCount = 5;
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(splitCount);
            
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent testEvent = TestUtility
                .generateCalendarEvent(tilerUser, duration, null, start, end, splitCount, false);
            TestSchedule schedule = new TestSchedule(user, refNow);
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            schedule.FindMeSomethingToDo(new Location()).Wait();
            schedule.persistToDB().Wait();


            testEvent = TestUtility.getCalendarEventById(testEvent.Id, user);
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents.OrderBy(sub => sub.Start).ToList()[1];
            long dayIndex = schedule.Now.getDayIndexFromStartOfTime(subEvent.Start);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            schedule.markSubEventAsCompleteCalendarEventAndReadjust(subEvent.Id);
            schedule.persistToDB().Wait();

            CalendarEvent testEventRetrieved = TestUtility.getCalendarEventById(testEvent.Id, user);
            List<SubCalendarEvent> activeSubEvents = testEventRetrieved.ActiveSubEvents.OrderBy(sub => sub.Start).ToList();
            activeSubEvents.ForEach((retrievedSubEvent) => {
                long retrievedDayIndex = schedule.Now.getDayIndexFromStartOfTime(retrievedSubEvent.Start);
                if(retrievedDayIndex == dayIndex)
                {
                    Assert.Fail("Subevent should not get reassigned to the day of already marked as complete subcalendar event");
                }
            });


            //////// Adds sub events with twice the split count as there are days. 
            //////// Meaning there should be two events from this calendar event per day
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent testEventDoubleSplitCount = TestUtility
                .generateCalendarEvent(tilerUser, duration, null, start, end, splitCount*2, false);
            schedule = new TestSchedule(user, refNow);
            schedule.AddToScheduleAndCommitAsync(testEventDoubleSplitCount).Wait();

            subEvent = testEventDoubleSplitCount.ActiveSubEvents.OrderBy(sub => sub.Start).ToList()[3];
            dayIndex = schedule.Now.getDayIndexFromStartOfTime(subEvent.Start);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            schedule.markSubEventAsCompleteCalendarEventAndReadjust(subEvent.Id);/// If we mark a subevent as complete, then we should expect that the day with the completion will only have one event, while the others should have two for each day
            schedule.persistToDB().Wait();
            int dayIndexCounter = 0;
            CalendarEvent testEventDoubleSplitCountRetrieved = TestUtility.getCalendarEventById(testEventDoubleSplitCount.Id, user);
            activeSubEvents = testEventDoubleSplitCountRetrieved.ActiveSubEvents.OrderBy(sub => sub.Start).ToList();
            activeSubEvents.ForEach((retrievedSubEvent) => {
                long retrievedDayIndex = schedule.Now.getDayIndexFromStartOfTime(retrievedSubEvent.Start);
                if (retrievedDayIndex == dayIndex)
                {
                    ++dayIndexCounter;
                }
            });
            Assert.AreEqual(dayIndexCounter, 1);
        }

        /// <summary>
        /// The test verifies that the previous day and events of the current day between the beginning and the current time are not moved
        /// </summary>
        [TestMethod]
        public void scheduleShouldNotModify_precedingDay_and_currentDay()
        {
            var packet = TestUtility.CreatePacket();
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;

            DateTimeOffset iniRefNow = new DateTimeOffset(DateTimeOffset.UtcNow.removeSecondsAndMilliseconds().Date.ToUniversalTime());
            iniRefNow = new DateTimeOffset(iniRefNow.Year, iniRefNow.Month, iniRefNow.Day, 0, 0, 0, new TimeSpan());
            iniRefNow = DateTimeOffset.Parse("11/3/2019 12:00:00 AM");
            DateTimeOffset refNow = iniRefNow;

            int splitCount = 15;
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(splitCount);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent testEvent = TestUtility
                .generateCalendarEvent(tilerUser, duration, null, start, end, splitCount, false);

            TestSchedule schedule = new TestSchedule(user, refNow);
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            List<SubCalendarEvent> subEvents = schedule.getAllActiveSubEvents().OrderBy(o => o.Start).ToList();
            Dictionary<string, TimeLine> subEventToTimeLine = subEvents.ToDictionary(obj => obj.Id, obj => (TimeLine)obj.ActiveSlot.CreateCopy());
            foreach(SubCalendarEvent eacgSubEvent in subEvents)
            {
                Assert.IsTrue(eacgSubEvent.Start >= refNow);
            }
            SubCalendarEvent firstSubEvent = subEvents.First();
            DateTimeOffset secondRefNow = firstSubEvent.End.AddHours(1);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent testEvent0 = TestUtility
                .generateCalendarEvent(tilerUser, duration, null, start, end, splitCount, false);
            schedule = new TestSchedule(user, secondRefNow);
            schedule.AddToScheduleAndCommitAsync(testEvent0).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            SubCalendarEvent firstSubEventDBRetrieved = TestUtility.getSubEventById(firstSubEvent.Id, user);
            Assert.AreEqual(firstSubEvent.Start, firstSubEventDBRetrieved.Start);


            DateTimeOffset third_refNow = refNow.AddDays(2);
            DayTimeLine second_firstDay = schedule.Now.firstDay;
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent testEvent1 = TestUtility
                .generateCalendarEvent(tilerUser, duration, null, start, end, splitCount, false);
            schedule = new TestSchedule(user, third_refNow);
            DayTimeLine firstDay = schedule.Now.firstDay;
            DayTimeLine secondDay = schedule.Now.getDayTimeLineByDayIndex(firstDay.UniversalIndex + 1);
            TimeLine precedingDayAndCurrentTime = new TimeLine(secondDay.Start.AddDays(-1), secondDay.End.AddDays(-2));
            precedingDayAndCurrentTime = new TimeLine(precedingDayAndCurrentTime.Start, third_refNow);
            IEnumerable<SubCalendarEvent> dayBeforeCurrentDayBeforeUpdate = schedule.getAllActiveSubEvents().Where(sub => sub.ActiveSlot.doesTimeLineInterfere(precedingDayAndCurrentTime)).OrderBy(o => o.Start).ToList();
            schedule.AddToScheduleAndCommit(testEvent1);
            IEnumerable<SubCalendarEvent> dayBeforeCurrentDayAfterUpdate = schedule.getAllActiveSubEvents().Where(sub => sub.ActiveSlot.doesTimeLineInterfere(precedingDayAndCurrentTime)).OrderBy(o => o.Start).ToList();
            Assert.AreEqual(dayBeforeCurrentDayBeforeUpdate.Count(), dayBeforeCurrentDayAfterUpdate.Count());
        }

        /// <summary>
        /// With any schedule modfication, subevents that have not been marked or deleted within the first twenty four hours should not get updated because user might need to interact with them.
        /// Schedule modifications should leave them in place
        /// </summary>
        [TestMethod]
        public void eventsWithinCurrentDayTimeLineShouldNotBeReScheduled()
        {
            var packet = TestUtility.CreatePacket();
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;

            DateTimeOffset iniRefNow = DateTimeOffset.Parse("4/26/2020 12:00:00 AM");
            DateTimeOffset refNow = iniRefNow;


            TimeLine repeatTimeLine = new TimeLine(refNow, refNow.AddDays(28));
            TimeSpan duration = TimeSpan.FromHours(2);
            
            TimeLine calTimeLine = repeatTimeLine.CreateCopy();
            Repetition repetition = new Repetition(repeatTimeLine, Repetition.Frequency.WEEKLY, calTimeLine);


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            int splitCount = 7;
            CalendarEvent testEvent0 = TestUtility
                .generateCalendarEvent(tilerUser, duration, repetition, repeatTimeLine.Start, repeatTimeLine.End, splitCount, false);
            TestSchedule Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent0).Wait();




            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Repetition repetition1 = new Repetition(repeatTimeLine, Repetition.Frequency.WEEKLY, calTimeLine);
            CalendarEvent testEvent1 = TestUtility
                .generateCalendarEvent(tilerUser, duration, repetition1, repeatTimeLine.Start, repeatTimeLine.End, splitCount, false);

            

            

            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent1).Wait();
            Schedule.populateDayTimeLinesWithSubcalendarEvents();

            long dayIndex = Schedule.Now.getDayIndexFromStartOfTime(refNow.AddDays(4));
            DayTimeLine dayTimeline = Schedule.Now.getDayTimeLineByDayIndex(dayIndex);

            DateTimeOffset dayTimeLineStart = dayTimeline.Start;
            DateTimeOffset secondRefnow = Utility.MiddleTime(dayTimeline);
            List<SubCalendarEvent> AllSubeventsInDay = dayTimeline.getSubEventsInTimeLine();
            Assert.IsTrue(AllSubeventsInDay.Count >= 1);
            List<SubCalendarEvent> subEventsBeforeNow = Schedule.getAllActiveSubEvents().Where(o => o.Start < dayTimeLineStart).ToList();
            Assert.IsTrue(subEventsBeforeNow.Count >= 1);
            List<SubCalendarEvent> subEventsInSameDayTimeLineButBeforeNow = AllSubeventsInDay.Where(subEvent => subEvent.Start < secondRefnow).ToList();
            Assert.IsTrue(subEventsInSameDayTimeLineButBeforeNow.Count >= 1);

            Assert.IsTrue(subEventsInSameDayTimeLineButBeforeNow.All(o => dayTimeline.doesTimeLineInterfere(o)));
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            
            
            Repetition repetition2 = new Repetition(repeatTimeLine, Repetition.Frequency.WEEKLY, calTimeLine);
            CalendarEvent testEvent2 = TestUtility
                .generateCalendarEvent(tilerUser, duration, repetition2, repeatTimeLine.Start, repeatTimeLine.End, splitCount, false);
            Schedule = new TestSchedule(user, secondRefnow);
            Schedule.AddToScheduleAndCommitAsync(testEvent2).Wait();

            

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, secondRefnow);
            var subEventsInDayTimeline =  Schedule.getAllActiveSubEvents().Where(subEvent => dayTimeline.doesTimeLineInterfere(subEvent)).ToDictionary(subEvent => subEvent.Start, subEvent => subEvent);
            foreach(SubCalendarEvent subEvent in subEventsInSameDayTimeLineButBeforeNow)
            {
                Assert.IsTrue(subEventsInDayTimeline.ContainsKey(subEvent.Start));
            }


            List<SubCalendarEvent> subEventsBeforeNowAfterUpdate = Schedule.getAllActiveSubEvents().Where(o => o.Start < dayTimeLineStart).ToList();
            Assert.IsTrue(subEventsBeforeNowAfterUpdate.Count == 0);
        }


        /// <summary>
        /// Test creates a calendar event at current time. THen at some point in the future adds an event. Thhe ref now in the future is at a point farther outh the range than can be pulled from DB.
        /// THis test verifies that the refnow is out side the range of some of the subevents of the aforementioned create calendar event. And these sub events are not movable .
        /// </summary>
        [TestMethod]
        public void eventsOutsideScheduleBoundsShouldNotBeReScheduled()
        {
            var packet = TestUtility.CreatePacket();
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;

            DateTimeOffset iniRefNow = DateTimeOffset.Parse("4/26/2020 12:00:00 AM");
            DateTimeOffset refNow = iniRefNow;

            int durationCount = 100;
            TimeLine repeatTimeLine = new TimeLine(refNow, refNow.AddDays(durationCount));
            TimeSpan duration = TimeSpan.FromHours(durationCount);

            TimeLine calTimeLine = repeatTimeLine.CreateCopy();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            int splitCount = durationCount;
            CalendarEvent testEvent0 = TestUtility
                .generateCalendarEvent(tilerUser, duration, new Repetition(), repeatTimeLine.Start, repeatTimeLine.End, splitCount, false);
            TestSchedule Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent0).Wait();


            Schedule.populateDayTimeLinesWithSubcalendarEvents();

            long dayIndex = Schedule.Now.getDayIndexFromStartOfTime(refNow.AddDays(25));
            DayTimeLine dayTimeline = Schedule.Now.getDayTimeLineByDayIndex(dayIndex);

            DateTimeOffset dayTimeLineStart = dayTimeline.Start;
            DateTimeOffset secondRefnow = Utility.MiddleTime(dayTimeline);
            TimeLine ignoreTimeLine = new TimeLine(refNow, secondRefnow.AddDays(Utility.defaultBeginDay));
            List<SubCalendarEvent> IgnoreSubevents = Schedule.getAllActiveSubEvents().Where(o=>ignoreTimeLine.doesTimeLineInterfere(o)).ToList();


            Repetition repetition2 = new Repetition(repeatTimeLine, Repetition.Frequency.WEEKLY, calTimeLine);
            CalendarEvent testEvent2 = TestUtility
                .generateCalendarEvent(tilerUser, duration, repetition2, repeatTimeLine.Start, repeatTimeLine.End, splitCount, false);
            Schedule = new TestSchedule(user, secondRefnow);
            Schedule.AddToScheduleAndCommitAsync(testEvent2).Wait();



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, secondRefnow);
            var subEventsInDayTimeline = Schedule.getAllActiveSubEvents().Where(subEvent => ignoreTimeLine.doesTimeLineInterfere(subEvent)).ToDictionary(subEvent => subEvent.Start, subEvent => subEvent);
            Assert.AreEqual(0, subEventsInDayTimeline.Count);//The default DB logcontrol retrieval uses the Utility.defaultBeginDay, with respect to Now.constNow, as its start in the timeline from the DB. So anything earlier should not be pulled from the DB

            foreach (SubCalendarEvent subEvent in IgnoreSubevents)
            {
                SubCalendarEvent retrievedSubEvent = TestUtility.getSubEventById(subEvent.Id, user);
                Assert.IsTrue(retrievedSubEvent.StartToEnd.isEqualStartAndEnd(subEvent.StartToEnd));
            }
            
            
        }

        public List<DateTimeOffset> getCorrespondingWeekdays(TimeLine timeLine, DayOfWeek dayOfWeek)
        {
            List<DateTimeOffset> retValue = new List<DateTimeOffset>();
            int dayDiff = (int)dayOfWeek - (int)timeLine.Start.DayOfWeek;
            if(dayDiff < 0)
            {
                dayDiff = (int)dayOfWeek + 7 + dayDiff;
            }
            DateTimeOffset startingDay = timeLine.Start.AddDays(dayDiff);
            DateTimeOffset currentDay = startingDay;
            while (timeLine.IsDateTimeWithin(currentDay))
            {
                retValue.Add(currentDay);
                currentDay = currentDay.AddDays(7);
            }

            return retValue;
        }
    }
}
