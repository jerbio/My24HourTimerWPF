using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerElements;
using TilerFront;

namespace TilerTests
{
    [TestClass]
    public class PerformanceTest
    {
        //[TestMethod]
        //public void CreateDailyRepeat()
        //{
        //    const int numberOfEvents = 100;
        //    TilerUser tilerUser = TestUtility.createUser();
        //    TimeSpan duration = TimeSpan.FromHours(1);
        //    UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
        //    tilerUser = user.getTilerUser();
        //    user.Login().Wait();
        //    DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
        //    TestSchedule Schedule = new TestSchedule(user, refNow);
        //    DateTimeOffset start = refNow;
        //    DateTimeOffset end = refNow.AddDays(100);
        //    DayOfWeek startingWeekDay = start.DayOfWeek;
        //    TimeLine repetitionTimeLine = new TimeLine(start, end);
        //    TimeLine eachRepeatFrame = new TimeLine(start, start.AddDays(1).AddMilliseconds(-1));
        //    List<CalendarEvent> allCalendarEvents = new List<CalendarEvent>();

        //    for (int i = 0; i < numberOfEvents; i++)
        //    {
        //        // cannot simply run addroscheduleAndCommit because you'll un into memory issues
        //        Repetition repetition = new Repetition(true, repetitionTimeLine, Repetition.Frequency.DAILY, eachRepeatFrame);
        //        Schedule = new TestSchedule(user, refNow);
        //        CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 5, false, restrictionProfile: new RestrictionProfile(start, duration + duration), now: Schedule.Now) as CalendarEventRestricted;
        //        allCalendarEvents.Add(testEvent);
                
        //    }
        //    user.ScheduleLogControl.Database.CalEvents.AddRange(allCalendarEvents);
        //    user.ScheduleLogControl.Database.SaveChanges();
        //    Schedule = new TestSchedule(user, refNow);
        //    Schedule.FindMeSomethingToDo(new Location()).Wait();
        //    Schedule.persistToDB().Wait();
        //    Assert.AreEqual(numberOfEvents, Schedule.getAllCalendarEvents().Count() - 1);


        //}

        [TestMethod]
        public void CreateDailyRepeatWithDeletion()
        {
        }
    }
}
