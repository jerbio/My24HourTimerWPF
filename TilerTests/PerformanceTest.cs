using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        //    Stopwatch watch = new Stopwatch();
        //    const int numberOfEvents = 100;
        //    watch.Start();
        //    TilerUser tilerUser = TestUtility.createUser();
        //    TimeSpan duration = TimeSpan.FromHours(1);
        //    UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
        //    tilerUser = user.getTilerUser();
        //    user.Login().Wait();
        //    DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
        //    TestSchedule Schedule = new TestSchedule(user, refNow);
        //    DateTimeOffset start = refNow;
        //    DateTimeOffset end = refNow.AddDays(70);
        //    DayOfWeek startingWeekDay = start.DayOfWeek;
        //    TimeLine repetitionTimeLine = new TimeLine(start, end);
        //    TimeLine eachRepeatFrame = new TimeLine(start, start.AddDays(1).AddMilliseconds(-1));
        //    List<CalendarEvent> allCalendarEvents = new List<CalendarEvent>();

        //    for (int i = 0; i < numberOfEvents; i++)
        //    {
        //        // cannot simply run addroscheduleAndCommit because you'll un into memory issues
        //        Repetition repetition = new Repetition(repetitionTimeLine, Repetition.Frequency.DAILY, eachRepeatFrame);
        //        Schedule = new TestSchedule(user, refNow);
        //        CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 5, false, restrictionProfile: new RestrictionProfile(start, duration + duration), now: Schedule.Now) as CalendarEventRestricted;
        //        allCalendarEvents.Add(testEvent);

        //    }
        //    user.ScheduleLogControl.Database.CalEvents.AddRange(allCalendarEvents);
        //    user.ScheduleLogControl.Database.SaveChanges();
        //    watch.Stop();
        //    TimeSpan saveToDbSpan = watch.Elapsed;
        //    watch.Restart();
        //    TestUtility.reloadTilerUser(ref user, ref tilerUser);
        //    Schedule = new TestSchedule(user, refNow);
        //    Schedule.FindMeSomethingToDo(new Location()).Wait();
        //    Schedule.persistToDB().Wait();
        //    watch.Stop();
        //    TimeSpan timeSpanToReloadSchedule = watch.Elapsed;
        //    Assert.AreEqual(numberOfEvents, Schedule.getAllCalendarEvents().Count() - 1);
        //}

        [TestMethod]
        public void CreateDailyRepeatAndAddEach()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            const int numberOfEvents = 5;
            TilerUser tilerUser = TestUtility.createUser();
            TimeSpan duration = TimeSpan.FromHours(1);
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            TestSchedule Schedule = new TestSchedule(user, refNow);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddDays(100);
            DayOfWeek startingWeekDay = start.DayOfWeek;
            TimeLine repetitionTimeLine = new TimeLine(start, end);
            TimeLine eachRepeatFrame = new TimeLine(start, start.AddDays(1).AddMilliseconds(-1));
            TimeLine rangeOfLookup = new TimeLine(start.AddDays(-30), start.AddDays(60));
            watch.Stop();
            List<CalendarEvent> allCalendarEvents = new List<CalendarEvent>();

            for (int i = 0; i < numberOfEvents; i++)
            {
                // cannot simply run addroscheduleAndCommit because you'll un into memory issues

                //getAllEnabledSubCalendarEvent(TimelineForData, now, true, DataRetrivalOption.Ui).
                LogControl LogAccess = user.ScheduleLogControl;
                watch.Restart();

                var task = LogAccess.getAllEnabledSubCalendarEvent(rangeOfLookup, Schedule.Now, retrievalOption: DataRetrivalOption.Ui);
                task.Wait();
                var allSubs = task.Result.ToList();

                
                watch.Stop();
                TimeSpan finshedSubEventRetrieval = watch.Elapsed;
                
                watch.Reset();
                watch.Start();
                var taskCal = LogAccess.getAllEnabledCalendarEvent(rangeOfLookup, Schedule.Now, retrievalOption: DataRetrivalOption.Ui);
                taskCal.Wait();
                var allCals = taskCal.Result.ToList();
                watch.Stop();
                TimeSpan finshedCalEventRetrieval = watch.Elapsed;


                Debug.WriteLine("Sub event retrieval took " + finshedSubEventRetrieval.ToString());
                Debug.WriteLine("Cal event retrieval took " + finshedCalEventRetrieval.ToString());
                Debug.WriteLine("-------------------------------------------------------");
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                Schedule = new TestSchedule(user, refNow);
                Repetition repetition = new Repetition(repetitionTimeLine, Repetition.Frequency.DAILY, eachRepeatFrame);
                CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 5, false, restrictionProfile: new RestrictionProfile(start, duration + duration), now: Schedule.Now) as CalendarEventRestricted;
                allCalendarEvents.Add(testEvent);
                Schedule.AddToScheduleAndCommit(testEvent).Wait();
            }
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            Schedule.FindMeSomethingToDo(new Location()).Wait();
            Schedule.persistToDB().Wait();
            watch.Stop();
            TimeSpan timeSpanToReloadSchedule = watch.Elapsed;
            Console.Write("The test took ", watch.Elapsed);
            Assert.AreEqual(numberOfEvents, Schedule.getAllCalendarEvents().Count() - 1);
        }

        [TestMethod]
        public void CreateDailyRepeatWithDeletion()
        {
        }
    }
}
