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
    }
}
