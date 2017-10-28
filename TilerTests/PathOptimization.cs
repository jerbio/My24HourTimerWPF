using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using My24HourTimerWPF;
using TilerElements;
using System.Collections.Generic;
using System.Linq;
using TilerCore;
using TilerFront;

namespace TilerTests
{
    [TestClass]
    public class PathOptimization
    {
        /// <summary>
        /// Test checkss to see if the home will be treated as the default location.
        /// 2 work events, 2 gym events are added and 2 home events. By the time evaluation is done the home events should be at the edge of the day.
        /// THe home event should be at the beginning pf the day
        /// </summary>
        [TestMethod]
        public void TestMethod1()
        {
            CalendarEvent homeA, homeB, workA, workB, gymA, gymB;
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            List<Location> locations = TestUtility.getLocations();
            Location home = locations[0];
            Location work = locations[1];
            Location gym = locations[2];
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset refNow = DateTimeOffset.Parse("9:00pm");
            DateTimeOffset startOfDay = DateTimeOffset.Parse("10:00pm");
            DateTimeOffset start = startOfDay.AddDays(1);
            DateTimeOffset end = start.AddDays(1);
            homeA = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, location: home);
            homeB = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, location: home);
            workA = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, location: work);
            workB = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, location: work);
            gymA = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, location: gym);
            gymB = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, location: gym);
            TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(homeA);
            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(homeB);
            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(workA);
            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(workB);
            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(gymA);
            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(gymB);
            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.FindMeSomethingToDo(home).Wait();
            schedule.WriteFullScheduleToLogAndOutlook().Wait();

            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            List<SubCalendarEvent> subEvents= schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents).ToList();
            subEvents = subEvents.OrderBy(subEvent => subEvent.Start).ToList();
            HashSet<string> validSet = new HashSet<string>() {"01","05", "45" };

            string calcuatedIndexes = "";
            List<int> allHomeLocations = new List<int>();
            for ( int i=0; i < subEvents.Count; i++)
            {
                SubCalendarEvent subEvet = subEvents[i];
                if(subEvet.Location.Description.ToUpper() == "HOME")
                {
                    calcuatedIndexes += i.ToString();
                }
            }

            Assert.IsTrue(validSet.Contains(calcuatedIndexes));// This is known to fail
        }
    }
}
