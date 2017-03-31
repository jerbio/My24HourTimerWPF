using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using My24HourTimerWPF;
using TilerElements;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using GoogleMapsApi.Entities.Directions.Request;
using TilerCore;

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
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TestSchedule Schedule = new TestSchedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }

        /// <summary>
        /// Function evaluates the addition of a newEvent to an already  populated schedule
        /// The newly added sub event should fall on the same day as events with the same location as the event.
        /// The test is structured in such away that the established schedule has each calendarevent on a given day will all have the same location. .
        /// So if the algorithm is correct it should select a day in which all calendar events have the same loocation as the newly tested event
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
            DateTimeOffset refNow = DateTimeOffset.UtcNow.Date;
            Schedule mySchedule = new TestSchedule(user, refNow);
            int count = 5;

            List<CalendarEvent> allEvents = new List<CalendarEvent>();
            Location homeLocation = new Location("2895 Van aken Blvd cleveland OH 44120");
            homeLocation.Validate();
            if (homeLocation.isNull)
            {
                throw new AssertFailedException("failed to Validate homeLocation");
            }

            Location workLocation = new Location("1228 euclid Ave cleveland OH");
            workLocation.Validate();
            if (workLocation.isNull)
            {
                throw new AssertFailedException("failed to Validate workLocation");
            }
            Location gymLocation = new Location("619 Prospect Avenue Cleveland, OH 44115");
            gymLocation.Validate();
            if (gymLocation.isNull)
            {
                throw new AssertFailedException("failed to Validate gymLocation");
            }

            Location churchLocation = new Location("1465 Dille Rd, Cleveland, OH 44117");
            churchLocation.Validate();
            if (churchLocation.isNull)
            {
                throw new AssertFailedException("failed to Validate churchLocation");
            }

            Location shakerLibrary = new Location("16500 Van Aken Blvd, Shaker Heights, OH 44120");
            shakerLibrary.Validate();
            if (shakerLibrary.isNull)
            {
                throw new AssertFailedException("failed to Validate shakerLibrary");
            }



            List<Location> locations = new List<Location>() { homeLocation, workLocation, gymLocation, churchLocation};
            int index = new Random().Next(locations.Count);
            Location randomLocation = locations[index];
            DateTimeOffset lastTime = new DateTimeOffset();
            //for loop establishes the schedule with different days
            for (int j = 0; j < count; j++)
            {
                for (int i = 0; i < count; i++)
                {
                    Location location = locations[j % locations.Count];
                    CalendarEvent calendarEvent = TestUtility.generateCalendarEvent(durationOfEvents, new TilerElements.Repetition(), newStart, newStart.AddDays(1), 1, false, location);
                    lastTime = calendarEvent.End;
                    allEvents.Add(calendarEvent);
                    mySchedule.AddToSchedule(calendarEvent);
                }
                newStart = newStart.AddDays(1);
            }


            List<CalendarEvent> calendarEvents = mySchedule.getAllCalendarEvents().Select(calEvent => calEvent.createCopy()).OrderBy(obj=>obj.Start).ToList();
            Dictionary<int, Health> dayIndexToHealth = new Dictionary<int, Health>();
            newStart = Start;
            TimeLine encompassingTimeline = new TimeLine(Start, lastTime);

            Console.WriteLine("==============================================");
            
            CalendarEvent testCalEvent = TestUtility.generateCalendarEvent(durationOfEvents, new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, randomLocation);
            mySchedule = new TestSchedule(calendarEvents.Select(obj=>obj.createCopy()), user, refNow);
            mySchedule.AddToSchedule(testCalEvent);
            newStart = newStart.AddDays(1);
            Health scheduleHealth = new Health(mySchedule.getAllCalendarEvents(), encompassingTimeline.Start, encompassingTimeline.TimelineSpan, mySchedule.Now, mySchedule.getHomeLocation);
            
            SubCalendarEvent firstSubEvent = calendarEvents.First().ActiveSubEvents.First();

            SubCalendarEvent testSubEvent = testCalEvent.ActiveSubEvents.First();

            int dayIndex = (firstSubEvent.Start.Date - testSubEvent.Start.Date).Days;

            //Assert.AreEqual(dayIndex, index);
        }
        /// <summary>
        /// Test tries to evaluate changes to a schedule to see if the schedule was made for the better or worse
        /// </summary>
        [TestMethod]
        public void scheduleComparisonEvaluation()
        {
            Location homeLocation = new Location("2895 Van aken Blvd cleveland OH 44120");
            homeLocation.Validate();
            Location workLocation = new Location(41.5002762, -81.6839155, "1228 euclid Ave cleveland OH", "Work", false, false);
            workLocation.Validate();
            
            Location gymLocation = new Location(41.4987461, -81.6884993, "619 Prospect Avenue Cleveland, OH 44115", "Gym", false, false);
            gymLocation.Validate();
            

            Location churchLocation = new Location(41.569467, -81.539422, "1465 Dille Rd, Cleveland, OH 44117", "Church", false, false);
            churchLocation.Validate();
            
            Location shakerLibrary = new Location(41.4658937, -81.5664832, "16500 Van Aken Blvd, Shaker Heights, OH 44120", "Shake Library", false, false);
            shakerLibrary.Validate();
            

            List<Location> locations = new List<Location>() { homeLocation, homeLocation, workLocation, gymLocation };//, churchLocation };

            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            
            TimeSpan duration = TimeSpan.FromDays(1);
            TimeLine eachTimeLine = new TimeLine(refNow, refNow.Add(duration));
            HashSet<EventID> hashEventIDs = new HashSet<EventID>();
            HashSet<EventID> EventIDs = new HashSet<EventID>();
            for (int i =0;i<4;i++)
            {
                TestSchedule Schedule = new TestSchedule(currentUser, refNow);
                CalendarEvent testEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(1), new Repetition(), eachTimeLine.Start,i == 0 ? eachTimeLine.End.AddHours(-12) : eachTimeLine.End, 1, false, locations[i]);
                Schedule.AddToScheduleAndCommit(testEvent).Wait();
                hashEventIDs.Add(testEvent.Calendar_EventID);
                EventIDs.Add(testEvent.Calendar_EventID);
            }



            TestSchedule scheduleA = new TestSchedule(currentUser, refNow);
            List<CalendarEvent> allCalEvents = scheduleA.getAllCalendarEvents().ToList();
            List<SubCalendarEvent> subEvents = allCalEvents.SelectMany(obj => obj.AllSubEvents).OrderBy(obj => obj.Start).ToList();
            SubCalendarEvent second = subEvents[1];
            TestSchedule scheduleB = new TestSchedule(currentUser, refNow);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> procrastinatioCompletion =scheduleB.ProcrastinateJustAnEvent(second.getId, TimeSpan.FromHours(10));
            scheduleB.UpdateWithDifferentSchedule(procrastinatioCompletion.Item2).ConfigureAwait(false);
            scheduleB = new TestSchedule(currentUser, refNow);
            allCalEvents = scheduleB.getAllCalendarEvents().ToList();
            subEvents = allCalEvents.SelectMany(obj => obj.AllSubEvents).OrderBy(obj => obj.Start).ToList();


            Health healthA = scheduleA.getScheduleQuality(eachTimeLine);
            Health healthB = scheduleB.getScheduleQuality(eachTimeLine);

            List<Health> healths = new List<Health>() { healthB, healthA };
            healths.Sort();
            Assert.AreEqual(healths.First(), healthA);
        }

        [TestCleanup]
        public void eachTestCleanUp()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            Schedule Schedule = new TestSchedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }

        /// <summary>
        /// This test tries to evaluate a simple schedule balance.
        /// Essentially we are creating seven calendar events. Each having only 1 splits. The scheduler should initially schedule things to be initially evenly spaced so every day should have seven sub calendar events.
        /// There should be one and only one event on each day
        /// </summary>
        [TestMethod]
        public void testScheduleCountBalance()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.LocalDateTime;
            TimeSpan activeDuration = TimeSpan.FromHours(1);
            int numberOfDays = 7;
            TimeSpan timeLineDuration = TimeSpan.FromDays(numberOfDays);
            TimeLine eventTimeLine = new TimeLine(refNow, refNow.Add(timeLineDuration));
            int numberOfSubeventPerCalendarEvent = 1;
            int numberOfCalendarEvent = 7;
            List<CalendarEvent> allCalendarevents = new List<CalendarEvent>();
            Location location = TestUtility.getLocations()[0];
            TestSchedule schedule = new TestSchedule(currentUser, refNow, refNow);
            for (int i = 0; i < numberOfCalendarEvent; i++)
            {
                CalendarEvent calEvent = TestUtility.generateCalendarEvent(activeDuration, new TilerElements.Repetition(), eventTimeLine.Start, eventTimeLine.End, numberOfSubeventPerCalendarEvent, false, location);
                allCalendarevents.Add(calEvent);
                schedule.AddToScheduleAndCommit(calEvent).Wait();
                schedule = new TestSchedule(currentUser, refNow, refNow);
                schedule.FindMeSomethingToDo(location).Wait();
                schedule.WriteFullScheduleToLogAndOutlook().Wait();
                schedule = new TestSchedule(currentUser, refNow, refNow);
            }

            List<DayTimeLine> daytimeLines = new List<DayTimeLine>();

            for (int i=0; i< numberOfDays; i++)
            {
                DateTimeOffset start = eventTimeLine.Start.AddDays(i);
                DayTimeLine dayTimeLine = schedule.Now.getDayTimeLineByTime(start);
                daytimeLines.Add(dayTimeLine);
            }

            schedule.populateDayTimeLinesWithSubcalendarEvents();

            foreach (DayTimeLine daytimeLine in daytimeLines)
            {
                int numberOfSubevent = daytimeLine.getSubEventsInDayTimeLine().Count;
                Assert.AreEqual(numberOfSubevent, numberOfSubeventPerCalendarEvent);//This is known to fail
            }
        }

        /// <summary>
        /// This test tries to evaluate a simple schedule balance.
        /// Essentially we are creating seven calendar events. Each having only 7 splits. The scheduler should initially schedule things to be initially evenly spaced so every day should have seven sub calendar events.
        /// There should be 7 and only one event on each day
        /// </summary>
        [TestMethod]
        public void testScheduleCountBalance0()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.LocalDateTime;
            TimeSpan activeDuration = TimeSpan.FromHours(1);
            int numberOfDays = 7;
            TimeSpan timeLineDuration = TimeSpan.FromDays(numberOfDays);
            TimeLine eventTimeLine = new TimeLine(refNow, refNow.Add(timeLineDuration));
            int numberOfCalendarEvent = 7;
            int numberOfSubevents = 7;
            int numberOfSubeventPerCalendarEvent = 7;
            List<CalendarEvent> allCalendarevents = new List<CalendarEvent>();
            Location location = TestUtility.getLocations()[0];
            TestSchedule schedule = new TestSchedule(currentUser, refNow, refNow);
            for (int i = 0; i < numberOfCalendarEvent; i++)
            {
                CalendarEvent calEvent = TestUtility.generateCalendarEvent(TimeSpan.FromTicks(activeDuration.Ticks * numberOfSubevents), new TilerElements.Repetition(), eventTimeLine.Start, eventTimeLine.End, numberOfSubevents, false, location);
                allCalendarevents.Add(calEvent);
                schedule.AddToScheduleAndCommit(calEvent).Wait();
                schedule = new TestSchedule(currentUser, refNow, refNow);
                schedule.FindMeSomethingToDo(location).Wait();
                schedule.WriteFullScheduleToLogAndOutlook().Wait();
                schedule = new TestSchedule(currentUser, refNow, refNow);
            }

            List<DayTimeLine> daytimeLines = new List<DayTimeLine>();

            for (int i = 0; i < numberOfDays; i++)
            {
                DateTimeOffset start = eventTimeLine.Start.AddDays(i);
                DayTimeLine dayTimeLine = schedule.Now.getDayTimeLineByTime(start);
                daytimeLines.Add(dayTimeLine);
            }

            schedule.populateDayTimeLinesWithSubcalendarEvents();

            foreach (DayTimeLine daytimeLine in daytimeLines)
            {
                int numberOfSubevent = daytimeLine.getSubEventsInDayTimeLine().Count;
                Assert.AreEqual(numberOfSubevent, numberOfSubeventPerCalendarEvent);
            }
        }

        [TestMethod]
        public void scheduleBalanceSleepAllocation()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Parse("9:00pm");
            DateTimeOffset startOfDay = DateTimeOffset.Parse("10:00pm");
            TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
            DateTimeOffset startTimeOfHugeRigid = startOfDay.AddHours(10);
            CalendarEvent bigHugeRigidEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(8), new Repetition(), startTimeOfHugeRigid, startTimeOfHugeRigid.AddHours(8));
            schedule.AddToScheduleAndCommit(bigHugeRigidEvent).Wait();
            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            CalendarEvent nonRigids = TestUtility.generateCalendarEvent(TimeSpan.FromHours(8), new Repetition(), startOfDay, startOfDay.AddDays(1),8);
            schedule.AddToScheduleAndCommit(nonRigids).Wait();
            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            Location location = TestUtility.getLocations()[0];
            schedule.FindMeSomethingToDo(location).Wait();
            TimeSpan eightHourSpan = TimeSpan.FromHours(8);
            List<SubCalendarEvent> subEveents = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents).OrderBy(obj => obj.Start).ToList();
            SubCalendarEvent subEvent = subEveents.First();
            TimeSpan spanOfEvents = (subEvent.Start - startOfDay);
            Assert.IsTrue(spanOfEvents >= eightHourSpan);
        }

        [ClassCleanup]
        public static void cleanUpTest()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            Schedule Schedule = new TestSchedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }
    }
}
