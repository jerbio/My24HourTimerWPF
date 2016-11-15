﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using My24HourTimerWPF;
using TilerElements;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using GoogleMapsApi.Entities.Directions.Request;

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
            TestSchedule Schedule = new TestSchedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }

        /// <summary>
        /// Function evaluates the addition of a enwEvent to an already  populated schedule
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
            //for loop establishes the schedule with different days
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


            List<CalendarEvent> calendarEvents = mySchedule.getAllCalendarEvents().Select(calEvent => calEvent.createCopy()).OrderBy(obj=>obj.Start).ToList();
            Dictionary<int, Health> dayIndexToHealth = new Dictionary<int, Health>();
            newStart = Start;
            TimeLine encompassingTimeline = new TimeLine(Start, lastTime);

            Console.WriteLine("==============================================");
            
            CalendarEvent testCalEvent = TestUtility.generateCalendarEvent(durationOfEvents, new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, randomLocation);
            mySchedule = new TestSchedule(calendarEvents.Select(obj=>obj.createCopy()), user, refNow);
            mySchedule.AddToSchedule(testCalEvent);
            newStart = newStart.AddDays(1);
            Health scheduleHealth = new Health(mySchedule.getAllCalendarEvents(), encompassingTimeline.Start, encompassingTimeline.TimelineSpan, mySchedule.Now);
            
            SubCalendarEvent firstSubEvent = calendarEvents.First().ActiveSubEvents.First();

            SubCalendarEvent testSubEvent = testCalEvent.ActiveSubEvents.First();

            int dayIndex = (firstSubEvent.Start.Date - testSubEvent.Start.Date).Days;

            //Assert.AreEqual(dayIndex, index);
        }
        /// <summary>
        /// Test tries to evaluate changess to a schedule to see if the schedule was made for the better or worse
        /// </summary>
        [TestMethod]
        public void scheduleComparisonEvaluation()
        {
            Location_Elements homeLocation = new Location_Elements("2895 Van aken Blvd cleveland OH 44120");
            homeLocation.Validate();
            //if (homeLocation.isNull)
            //{
            //    throw new AssertFailedException("failed to Validate homeLocation");
            //}

            Location_Elements workLocation = new Location_Elements(41.5002762, -81.6839155, "1228 euclid Ave cleveland OH", "Work", false, false);
            workLocation.Validate();
            //if (workLocation.isNull)
            //{
            //    throw new AssertFailedException("failed to Validate workLocation");
            //}
            Location_Elements gymLocation = new Location_Elements(41.4987461, -81.6884993, "619 Prospect Avenue Cleveland, OH 44115", "Gym", false, false);
            gymLocation.Validate();
            //if (gymLocation.isNull)
            //{
            //    throw new AssertFailedException("failed to Validate gymLocation");
            //}

            Location_Elements churchLocation = new Location_Elements(41.569467, -81.539422, "1465 Dille Rd, Cleveland, OH 44117", "Church", false, false);
            churchLocation.Validate();
            //if (churchLocation.isNull)
            //{
            //    throw new AssertFailedException("failed to Validate churchLocation");
            //}

            Location_Elements shakerLibrary = new Location_Elements(41.4658937, -81.5664832, "16500 Van Aken Blvd, Shaker Heights, OH 44120", "Shake Library", false, false);
            shakerLibrary.Validate();
            //if (shakerLibrary.isNull)
            //{
            //    throw new AssertFailedException("failed to Validate shakerLibrary");
            //}

            List<Location_Elements> locations = new List<Location_Elements>() { homeLocation, homeLocation, workLocation, gymLocation };//, churchLocation };

            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Now;
            
            TimeSpan duration = TimeSpan.FromDays(1);
            TimeLine eachTimeLine = new TimeLine(refNow, refNow.Add(duration));
            HashSet<EventID> hashEventIDs = new HashSet<EventID>();
            HashSet<EventID> EventIDs = new HashSet<EventID>();
            for (int i =0;i<4;i++)
            {
                Schedule Schedule = new TestSchedule(currentUser, refNow);
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
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> procrastinatioCompletion =scheduleB.ProcrastinateJustAnEvent(second.Id, TimeSpan.FromHours(10));
            scheduleB.UpdateWithProcrastinateSchedule(procrastinatioCompletion.Item2).ConfigureAwait(false);
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
            DateTimeOffset refNow = DateTimeOffset.Now;
            Schedule Schedule = new Schedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
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
