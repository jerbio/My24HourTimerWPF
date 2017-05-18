using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using My24HourTimerWPF;
using TilerElements;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TilerTests
{
    [TestClass]
    public class WhatIfTest
    {
        Random random = new Random((int)DateTimeOffset.UtcNow.Ticks);
        [ClassInitialize]
        public static void classInitialize(TestContext testContext)
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DB_Schedule schedule = new DB_Schedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }

        [TestInitialize]
        public void cleanUpLog()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DB_Schedule Schedule = new DB_Schedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }

        [ClassCleanup]
        public static void cleanUpTest()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DB_Schedule Schedule = new DB_Schedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }
        /// <summary>
        /// This test runs a what if scenario on the schedule, by moving an event to a different time.
        /// This test works by building two different schedules. Then adding a test event to be constrained to different days. 
        /// The day with events that are clustered close to the newly event is determined to be the better day
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WhatIfMondayInsteadOfTuesdayLocation()
        {
            List<Location> locations = TestUtility.getLocations();
            int mondayLocationIndex = random.Next(locations.Count);
            Location mondayLocation = locations[mondayLocationIndex];
            Location TuesdayLocation = locations[(mondayLocationIndex + 1)% locations.Count];
            List<CalendarEvent> mondayEvents = new List<CalendarEvent>();
            List<CalendarEvent> tuesdayEvents = new List<CalendarEvent>();
            TimeSpan durationOfCalEvent = TimeSpan.FromHours(1);
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset mondayStart = getNextDateForDayOfWeek(DayOfWeek.Monday, refNow);
            DateTimeOffset tuesdayStart = mondayStart.AddDays(1);
            int numberOfEvents = 5;
            for (int i = 0; i < numberOfEvents; i++)
            {
                CalendarEvent mondayEvent = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), mondayStart, mondayStart.AddDays(1),1, false, mondayLocation);
                CalendarEvent tuesdayEvent = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), tuesdayStart, tuesdayStart.AddDays(1), 1, false, TuesdayLocation);
                mondayEvents.Add(mondayEvent);
                tuesdayEvents.Add(tuesdayEvent);
            }
            List<CalendarEvent> allCalendarEvents = mondayEvents.Concat(tuesdayEvents).ToList();
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            foreach (CalendarEvent calEvent in allCalendarEvents)
            {
                DB_Schedule eachSchedule = new DB_Schedule(currentUser, refNow);
                eachSchedule.AddToScheduleAndCommit(calEvent).Wait();
            }

            DateTimeOffset wednesdayStart = mondayStart.AddDays(2);// getNextDateForDayOfWeek(DayOfWeek.Wednesday, refNow);
            CalendarEvent wednesdayEvent = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), mondayStart, wednesdayStart.AddDays(1), 1, false, TuesdayLocation);
            DB_Schedule schedule = new DB_Schedule(currentUser, refNow);
            schedule.AddToScheduleAndCommit(wednesdayEvent).Wait();
            schedule = new TestSchedule(currentUser, refNow);
            CalendarEvent retrievedWednesdayEvent = schedule.getCalendarEvent(wednesdayEvent.Calendar_EventID);

            schedule = new DB_Schedule(currentUser, refNow);
            Health tuesdayHealth = await schedule.WhatIfDifferentDay(tuesdayStart, retrievedWednesdayEvent.ActiveSubEvents.First().SubEvent_ID).ConfigureAwait(false);
            schedule = new DB_Schedule(currentUser, refNow);
            Health mondayHealth = await schedule.WhatIfDifferentDay(wednesdayStart.AddDays(-2), retrievedWednesdayEvent.ActiveSubEvents.First().SubEvent_ID).ConfigureAwait(false);
            HealthEvaluation mondayEvaluation = new HealthEvaluation(mondayHealth);
            HealthEvaluation tuesdayEvaluation= new HealthEvaluation(tuesdayHealth);

            double mondayScore = mondayHealth.getScore();
            double tuesdayScore = tuesdayHealth.getScore();
            
            Assert.IsTrue(tuesdayScore < mondayScore);
        }

        /// <summary>
        /// This test runs a what if scenario on the schedule, by checking if moving the event to a different time will cause it to have an unfavorable schedule. 
        /// This test is targeted at modifying the schedule such that there is a conflict. A conflicting schedule should be less favorable than one that doesnt conflict
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WhatIfMondayInsteadOfTuesdayConflict()
        {
            List<Location> locations = TestUtility.getLocations();
            int mondayLocationIndex = random.Next(locations.Count);
            Location desiredLocation = locations[mondayLocationIndex];
            List<CalendarEvent> mondayEvents = new List<CalendarEvent>();
            List<CalendarEvent> tuesdayEvents = new List<CalendarEvent>();
            TimeSpan durationOfCalEvent = TimeSpan.FromHours(1);
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset mondayStart = getNextDateForDayOfWeek(DayOfWeek.Monday, refNow);
            DateTimeOffset tuesdayStart = mondayStart.AddDays(1);
            DateTimeOffset mondayStartCopy = mondayStart;
            DateTimeOffset tuesdayStartCopy = tuesdayStart.AddHours(6);
            int numberOfEvents = 5;
            for (int i = 0; i < numberOfEvents; i++)
            {
                CalendarEvent mondayEvent = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), mondayStartCopy, mondayStartCopy.AddHours(1), 1, true, desiredLocation);
                CalendarEvent tuesdayEvent = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), tuesdayStartCopy, tuesdayStartCopy.AddHours(1), 1, true, desiredLocation);
                mondayStartCopy = mondayStartCopy.AddHours(1);
                tuesdayStartCopy = tuesdayStartCopy.AddHours(1);
                mondayEvents.Add(mondayEvent);
                tuesdayEvents.Add(tuesdayEvent);
            }
            List<CalendarEvent> allCalendarEvents = mondayEvents.Concat(tuesdayEvents).ToList();
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            foreach (CalendarEvent calEvent in allCalendarEvents)
            {
                DB_Schedule eachSchedule = new DB_Schedule(currentUser, refNow);
                eachSchedule.AddToScheduleAndCommit(calEvent).Wait();
            }

            DateTimeOffset wednesdayStart = mondayStart.AddDays(2);
            CalendarEvent wednesdayEvent = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), mondayStart, wednesdayStart.AddHours(1), 1, true, desiredLocation);
            DB_Schedule schedule = new DB_Schedule(currentUser, refNow);
            schedule.AddToScheduleAndCommit(wednesdayEvent).Wait();
            schedule = new TestSchedule(currentUser, refNow);
            CalendarEvent retrievedWednesdayEvent = schedule.getCalendarEvent(wednesdayEvent.Calendar_EventID);

            schedule = new DB_Schedule(currentUser, refNow);
            Health tuesdayHealth = await schedule.WhatIfDifferentDay(tuesdayStart, retrievedWednesdayEvent.ActiveSubEvents.First().SubEvent_ID).ConfigureAwait(false);
            schedule = new DB_Schedule(currentUser, refNow);
            Health mondayHealth = await schedule.WhatIfDifferentDay(wednesdayStart.AddDays(-2), retrievedWednesdayEvent.ActiveSubEvents.First().SubEvent_ID).ConfigureAwait(false);
            HealthEvaluation mondayEvaluation = new HealthEvaluation(mondayHealth);
            HealthEvaluation tuesdayEvaluation = new HealthEvaluation(tuesdayHealth);

            double mondayScore = mondayHealth.getScore();
            double tuesdayScore = tuesdayHealth.getScore();

            Assert.IsTrue(tuesdayScore < mondayScore);
        }

        public DateTimeOffset getNextDateForDayOfWeek(DayOfWeek dayOfeek, DateTimeOffset referenceTime)
        {
            DateTimeOffset retValue;

            if(referenceTime.DayOfWeek != dayOfeek)
            {
                //int dayCount = ((int)referenceTime.DayOfWeek + 7);
                int dayDiff = dayOfeek - referenceTime.DayOfWeek;
                if(dayDiff > 0)
                {
                    retValue = referenceTime.AddDays(dayDiff);
                }
                else
                {
                    retValue = referenceTime.AddDays(dayDiff + 7);
                }
                
            }
            else
            {
                retValue = referenceTime;
                retValue = retValue.LocalDateTime;
            }

            return retValue;

        }
    }
}
