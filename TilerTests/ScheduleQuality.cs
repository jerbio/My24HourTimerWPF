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
        public void SingleEvenDayPreferencePicker()
        {
            DB_Schedule Schedule;
            int splitCount = 6;
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddDays(21);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent = TestUtility
                .generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, splitCount, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            int firstDayIncrement = 2;
            int secondDayIncrement = 4;

            DateTimeOffset firstDayFromStart = refNow.AddDays(firstDayIncrement);
            DateTimeOffset secondDayFromStart = refNow.AddDays(secondDayIncrement);

            DayOfWeek firstDayOfWeek = start.AddDays(2).DayOfWeek;
            DayOfWeek secondDayOfWeek = start.AddDays(4).DayOfWeek;

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            refNow = firstDayFromStart;
            Schedule = new TestSchedule(user, refNow);
            Schedule.SetCalendarEventAsNow(testEvent.Id);


            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            refNow = secondDayFromStart;
            Schedule = new TestSchedule(user, refNow);
            Schedule.SetCalendarEventAsNow(testEvent.Id);

            List<SubCalendarEvent> subEvents = Schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.ActiveSubEvents).ToList();
            List<DayOfWeek> daysOfWeek = subEvents.Select(subEvent => subEvent.Start.DayOfWeek).ToList();
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
    }
}
