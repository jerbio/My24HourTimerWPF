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

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            refNow = firstDayFromStart;
            Schedule = new TestSchedule(user, refNow);
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents.First();
            Schedule.SetSubeventAsNow(subEvent.Id);
            Schedule.persistToDB().Wait();


            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            Schedule = new TestSchedule(user, refNow);
            Schedule.markSubEventAsComplete(subEvent.Id).Wait();
            Schedule.persistToDB().Wait();


            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            refNow = secondDayFromStart;
            Schedule = new TestSchedule(user, refNow);
            testEvent = TestUtility.getCalendarEventById(testEvent.Id, user);
            subEvent = testEvent.ActiveSubEvents.First();
            testEvent = Schedule.getCalendarEvent(testEvent.Id);
            Schedule.SetSubeventAsNow(subEvent.Id);
            Schedule.persistToDB().Wait();


            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            Schedule = new TestSchedule(user, refNow);
            testEvent = Schedule.getCalendarEvent(testEvent.Id);
            subEvent = testEvent.ActiveSubEvents.First();
            Schedule.markSubEventAsComplete(subEvent.Id).Wait();
            Schedule.persistToDB().Wait();


            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            Schedule = new TestSchedule(user, refNow);
            Schedule.FindMeSomethingToDo(new Location()).Wait();
            testEvent = Schedule.getCalendarEvent(testEvent.Id);
            Schedule.persistToDB().Wait();

            List<SubCalendarEvent> subEvents = Schedule.getCalendarEvent(testEvent.Id).ActiveSubEvents.ToList();
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
            Schedule = new TestSchedule(user, refNow);
            testEvent = Schedule.getCalendarEvent(testEvent.Id);
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents.OrderBy(sub => sub.Start).First();
            Schedule.SetSubeventAsNow(subEvent.Id);
            Schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            testEvent = Schedule.getCalendarEvent(testEvent.Id);
            Schedule.markSubEventAsComplete(subEvent.Id).Wait();
            Schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            refNow = secondDayFromStart;
            Schedule = new TestSchedule(user, refNow);
            testEvent = Schedule.getCalendarEvent(testEvent.Id);
            subEvent = testEvent.ActiveSubEvents.OrderBy(sub => sub.Start).First();
            testEvent = Schedule.getCalendarEvent(testEvent.Id);
            Schedule.SetSubeventAsNow(subEvent.Id);
            Schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            testEvent = Schedule.getCalendarEvent(testEvent.Id);
            subEvent = testEvent.ActiveSubEvents.OrderBy(sub => sub.Start).First();
            Schedule.markSubEventAsComplete(subEvent.Id).Wait();
            Schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            Schedule.FindMeSomethingToDo(new Location()).Wait();
            testEvent = Schedule.getCalendarEvent(testEvent.Id);
            Schedule.persistToDB().Wait();

            List<SubCalendarEvent> subEvents = Schedule.getCalendarEvent(testEvent.Id).ActiveSubEvents.ToList();
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
                Schedule = new TestSchedule(user, refNow);
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
                Schedule = new TestSchedule(user, refNow);
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
            repeatEvent = Schedule.getCalendarEvent(repeatEvent.Id);
            Schedule.persistToDB().Wait();

            List<SubCalendarEvent> repeatSubEvents = Schedule.getCalendarEvent(repeatEvent.Id).ActiveSubEvents.ToList();
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



            List<DayOfWeek> repeatDaysOfWeek = repeatSubEvents.Select(tilerEvent => tilerEvent.Start.DayOfWeek).ToList();

            List<SubCalendarEvent> testSubEvents = Schedule.getCalendarEvent(testEvent0.Id).ActiveSubEvents.ToList();
            List<DayOfWeek> daysOfWeek = testSubEvents.Select(tilerEvent => tilerEvent.Start.DayOfWeek).ToList();


            int repeatCount = 0;
            foreach (DayOfWeek weekDay in repeatDaysOfWeek)
            {
                if (repeatDays.Contains(weekDay))
                {
                    repeatCount++;
                }
            }

            int eventCount = 0;
            foreach (DayOfWeek weekDay in daysOfWeek)
            {
                if (eventDays.Contains(weekDay))
                {
                    eventCount++;
                }
            }

            int count = allCorrespondingRepeatDays.Count - repeatSplitCount;// this should be 8 because the first week in the repeat sequence is 7/1/2019 - 7/8/2019 which leaves only one day after "refnow" which is 7/7/2019 so it cannot be assigned to one of the preference days
            Assert.AreEqual(repeatCount, count);
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

            SubCalendarEvent subEvent = testEvent.ActiveSubEvents.OrderBy(sub => sub.Start).ToList()[1];
            ulong dayIndex = schedule.Now.getDayIndexFromStartOfTime(subEvent.Start);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            schedule.markSubEventAsCompleteCalendarEventAndReadjust(subEvent.Id);
            schedule.persistToDB().Wait();

            CalendarEvent testEventRetrieved = TestUtility.getCalendarEventById(testEvent.Id, user);
            List<SubCalendarEvent> activeSubEvents = testEventRetrieved.ActiveSubEvents.OrderBy(sub => sub.Start).ToList();
            activeSubEvents.ForEach((retrievedSubEvent) => {
                ulong retrievedDayIndex = schedule.Now.getDayIndexFromStartOfTime(retrievedSubEvent.Start);
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
                ulong retrievedDayIndex = schedule.Now.getDayIndexFromStartOfTime(retrievedSubEvent.Start);
                if (retrievedDayIndex == dayIndex)
                {
                    ++dayIndexCounter;
                }
            });
            Assert.AreEqual(dayIndexCounter, 1);
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
