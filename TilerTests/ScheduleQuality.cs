﻿using System;
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
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

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
            Schedule.SetEventAsNow(subEvent.Id);
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
            subEvent = testEvent.ActiveSubEvents.First();
            testEvent = Schedule.getCalendarEvent(testEvent.Id);
            Schedule.SetEventAsNow(subEvent.Id);
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
            CalendarEvent testEvent0 = TestUtility
                .generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, splitCount, false);
            Schedule.AddToScheduleAndCommit(testEvent0).Wait();

            TimeLine repeatTimeLine = new TimeLine(testEvent0.Start, testEvent0.End.AddDays(14));
            TimeLine calTimeLine = repeatTimeLine.CreateCopy();
            Repetition repetition = new Repetition(true, repeatTimeLine, Repetition.Frequency.WEEKLY, calTimeLine);
            int repeatSplitCount = 2;
            CalendarEvent testEvent = TestUtility
                .generateCalendarEvent(tilerUser, duration, repetition, calTimeLine.Start, calTimeLine.End, repeatSplitCount, false);

            int firstDayIncrement = 2;
            int secondDayIncrement = 4;

            DateTimeOffset firstDayFromStart = refNow.AddDays(firstDayIncrement);
            DateTimeOffset secondDayFromStart = refNow.AddDays(secondDayIncrement);

            DayOfWeek firstDayOfWeek = firstDayFromStart.DayOfWeek;
            DayOfWeek secondDayOfWeek = secondDayFromStart.DayOfWeek;


            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            refNow = firstDayFromStart;
            Schedule = new TestSchedule(user, refNow);
            SubCalendarEvent subEvent = testEvent.ActiveSubEvents.First();
            Schedule.SetEventAsNow(subEvent.Id);
            Schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            Schedule.markSubEventAsComplete(subEvent.Id).Wait();
            Schedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            refNow = secondDayFromStart;
            Schedule = new TestSchedule(user, refNow);
            subEvent = testEvent.ActiveSubEvents.First();
            testEvent = Schedule.getCalendarEvent(testEvent.Id);
            Schedule.SetEventAsNow(subEvent.Id);
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

            List<SubCalendarEvent> subEvents = Schedule.getCalendarEvent(testEvent.Id).ActiveSubEvents.ToList();
            List<DayOfWeek> daysOfWeek = subEvents.Select(tilerEvent => tilerEvent.Start.DayOfWeek).ToList();
            int count = 0;
            foreach (DayOfWeek weekDay in daysOfWeek)
            {
                if (weekDay == firstDayOfWeek || weekDay == secondDayOfWeek)
                {
                    count++;
                }
            }

            Assert.AreEqual(count, subEvents.Count);
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
            Schedule.AddToScheduleAndCommit(testEvent0).Wait();

            TimeLine repeatTimeLine = new TimeLine(testEvent0.Start, testEvent0.End.AddDays(14));
            TimeLine calTimeLine = repeatTimeLine.CreateCopy();
            Repetition repetition = new Repetition(true, repeatTimeLine, Repetition.Frequency.WEEKLY, calTimeLine);
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
            Schedule.AddToScheduleAndCommit(repeatEvent).Wait();

            for (int i = 0; i < 2; i++)
            {
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                refNow = repeatDates[i];
                Schedule = new TestSchedule(user, refNow);
                SubCalendarEvent subEvent = repeatEvent.ActiveSubEvents.First();
                Schedule.SetEventAsNow(subEvent.Id);
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
                Schedule = new TestSchedule(user, refNow);
                SubCalendarEvent subEvent = testEvent0.ActiveSubEvents.First();
                Schedule.SetEventAsNow(subEvent.Id);
                Schedule.persistToDB().Wait();


                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                Schedule = new TestSchedule(user, refNow);
                Schedule.markSubEventAsComplete(subEvent.Id).Wait();
                Schedule.persistToDB().Wait();
            }




            //TestUtility.reloadTilerUser(ref user, ref tilerUser);
            //refNow = repeatSecondDayFromStart;
            //Schedule = new TestSchedule(user, refNow);
            //subEvent = repeatEvent.ActiveSubEvents.First();
            //repeatEvent = Schedule.getCalendarEvent(repeatEvent.Id);
            //Schedule.SetEventAsNow(subEvent.Id);
            //Schedule.persistToDB().Wait();


            //TestUtility.reloadTilerUser(ref user, ref tilerUser);
            //Schedule = new TestSchedule(user, refNow);
            //repeatEvent = Schedule.getCalendarEvent(repeatEvent.Id);
            //subEvent = repeatEvent.ActiveSubEvents.First();
            //Schedule.markSubEventAsComplete(subEvent.Id).Wait();
            //Schedule.persistToDB().Wait();


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

            Assert.AreEqual(repeatCount, allCorrespondingRepeatDays.Count);
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
