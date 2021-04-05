#define RunSlowTest

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerElements;
using TilerFront;

namespace TilerTests
{
#if RunSlowTest
        [TestClass]
#endif
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

                var task = LogAccess.getAllEnabledSubCalendarEvent(rangeOfLookup, Schedule.Now, retrievalOptions: DataRetrievalSet.UiSet);
                task.Wait();
                var allSubs = task.Result.ToList();

                
                watch.Stop();
                TimeSpan finshedSubEventRetrieval = watch.Elapsed;
                
                watch.Reset();
                watch.Start();
                var taskCal = LogAccess.getAllEnabledCalendarEvent(rangeOfLookup, Schedule.Now, retrievalOptions: DataRetrievalSet.UiSet);
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
                Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            }
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            Schedule.FindMeSomethingToDo(new Location()).Wait();
            Schedule.persistToDB().Wait();
            watch.Stop();
            TimeSpan timeSpanToReloadSchedule = watch.Elapsed;
            Console.Write("The test took ", watch.Elapsed);
            Assert.AreEqual(numberOfEvents, Schedule.getAllCalendarEvents().ToLookup(o => o.getTilerID.getCalendarEventComponent()).Count);
        }

        [TestMethod]
        public void createAllPossibleCalendarEventSpan()
        {
            DateTimeOffset iniRefNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            var packet = TestUtility.CreatePacket();
            TilerUser tilerUser = packet.User;
            TilerUser testUser = packet.User;
            UserAccount user = packet.Account;
            UserAccount userAccount = packet.Account;

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule schedule = new TestSchedule(user, iniRefNow);
            Location location = Location.getDefaultLocation();
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = iniRefNow;

            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<CalendarEvent> oneSubEvents = TestUtility.generateAllCalendarEvent(schedule, duration, start, testUser, userAccount, 1, location);
            watch.Stop();
            TimeSpan singleEventSpan = watch.Elapsed;
            GC.Collect();
            Debug.WriteLine("Single Event took " + singleEventSpan.ToString());
            watch.Reset();
            watch.Start();
            List<CalendarEvent> twoEvents = TestUtility.generateAllCalendarEvent(schedule, duration, start, testUser, userAccount, 2, location);
            watch.Stop();
            TimeSpan twoEventSpan = watch.Elapsed;
            Debug.WriteLine("two Events took " + twoEventSpan.ToString());
            GC.Collect();
            watch.Reset();
            watch.Start();
            List<CalendarEvent> tenEvents = TestUtility.generateAllCalendarEvent(schedule, duration, start, testUser, userAccount, 10, location);
            watch.Stop();
            GC.Collect();
            long memory = GC.GetTotalMemory(true);
            TimeSpan tenEventSpan = watch.Elapsed;
            Debug.WriteLine("ten Events took " + tenEventSpan.ToString());
        }

        [TestMethod]
        public void CreateDailyRepeatWithDeletion()
        {
        }
    }
}
