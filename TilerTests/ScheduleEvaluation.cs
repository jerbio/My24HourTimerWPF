using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using My24HourTimerWPF;
using TilerElements;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace TilerTests
{
    [TestClass]
    public class ScheduleEvaluation
    {
        [ClassInitialize]
        public static void classInitialize(TestContext testContext)
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Now;
            Schedule Schedule = new Schedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }

        /// <summary>
        /// Function evaluates the schedule based on the newly added event.
        /// Duplicate semantically equivalent schedules are created, each adding a the same calendarevent(newCalEvent) with the same parameters.
        /// in each duplicate schedule events on a particular day have the same location. The newCalEvent gets a random location that is best aligned for a given day.
        /// Each duplicate schedule is evaluated after the addition of a duplicate version newCalevent and tested to see if the day with the maching location of newCalevent has the best score.
        /// </summary>
        [TestMethod]
        public void scheduleEvaluation()
        {
            TimeSpan jitterSpan = TimeSpan.FromMinutes(10);
            TimeSpan durationOfEvents = TimeSpan.FromMinutes(60);
            DateTimeOffset Start = DateTimeOffset.UtcNow;
            DateTimeOffset newStart = Start;
            UserAccount user = TestUtility.getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Now.Date;
            Schedule mySchedule = new TestSchedule(user, refNow);
            int count = 5;

            List<CalendarEvent> allEvents = new List<CalendarEvent>();
            Location_Elements homeLocation = new Location_Elements("2895 Van aken Blvd cleveland OH 44120");
            homeLocation.Validate();
            if (homeLocation.isNull)
            {
                throw new AssertFailedException("failed to Validate homeLocation");
            }

            Location_Elements workLocation = new Location_Elements("1228 euclid Ave cleveland OH");
            workLocation.Validate();
            if (workLocation.isNull)
            {
                throw new AssertFailedException("failed to Validate workLocation");
            }
            Location_Elements gymLocation = new Location_Elements("619 Prospect Avenue Cleveland, OH 44115");
            gymLocation.Validate();
            if (gymLocation.isNull)
            {
                throw new AssertFailedException("failed to Validate gymLocation");
            }

            Location_Elements churchLocation = new Location_Elements("1465 Dille Rd, Cleveland, OH 44117");
            churchLocation.Validate();
            if (churchLocation.isNull)
            {
                throw new AssertFailedException("failed to Validate churchLocation");
            }

            Location_Elements shakerLibrary = new Location_Elements("16500 Van Aken Blvd, Shaker Heights, OH 44120");
            shakerLibrary.Validate();
            if (shakerLibrary.isNull)
            {
                throw new AssertFailedException("failed to Validate shakerLibrary");
            }



            List<Location_Elements> locations = new List<Location_Elements>() { homeLocation, workLocation, gymLocation, churchLocation};
            int index = new Random().Next(locations.Count);
            Location_Elements randomLocation = locations[index];
            DateTimeOffset lastTime = new DateTimeOffset();
            for (int j = 0; j < count; j++)
            {
                for (int i = 0; i < count; i++)
                {
                    Location_Elements location = locations[j % locations.Count];
                    CalendarEvent calendarEvent = TestUtility.generateCalendarEvent(durationOfEvents, new TilerElements.Repetition(), newStart, newStart.AddDays(1), 1, false, location);
                    lastTime = calendarEvent.End;
                    allEvents.Add(calendarEvent);
                    mySchedule.AddToSchedule(calendarEvent);
                }
                newStart = newStart.AddDays(1);
            }


            List<CalendarEvent> calendarEvents = mySchedule.getAllCalendarEvents().Select(calEvent => calEvent.createCopy()).ToList();
            Dictionary<int, Health> dayIndexToHealth = new Dictionary<int, Health>();
            newStart = Start;
            TimeLine encompassingTimeline = new TimeLine(Start, lastTime);

            Console.WriteLine("==============================================");
            for (int j = 0; j < count; j++)
            {
                CalendarEvent newCalEvent = TestUtility.generateCalendarEvent(durationOfEvents, new TilerElements.Repetition(), newStart, newStart.AddDays(1), 1, false, randomLocation);
                mySchedule = new TestSchedule(calendarEvents.Select(obj=>obj.createCopy()), user, refNow);
                mySchedule.AddToSchedule(newCalEvent);
                newStart = newStart.AddDays(1);
                Health scheduleHealth = new Health(mySchedule.getAllCalendarEvents(), encompassingTimeline.Start, encompassingTimeline.TimelineSpan, Schedule.Now);
                dayIndexToHealth.Add(j, scheduleHealth);
            }

            List<Tuple<double,  KeyValuePair<int, Health >>> orderedKeys = dayIndexToHealth.Select(keyValuePair => new Tuple<double, KeyValuePair<int, Health>>(keyValuePair.Value.getScore(), keyValuePair)).OrderBy(tuple => tuple.Item1).ToList();
            Assert.AreEqual(orderedKeys[0].Item2.Key, index);
        }

        [ClassCleanup]
        public static void cleanUpTest()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Now;
            Schedule Schedule = new Schedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }

    }
}
