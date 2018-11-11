using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using My24HourTimerWPF;
using TilerElements;
using TilerCore;
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
        /// This test runs a what if scenario on the schedule, by moving an event to a different time.
        /// This test works by building two different schedules. Then adding a test event to be constrained to different days. 
        /// The day with events that are clustered close to the newly event is determined to be the better day
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WhatIfMondayInsteadOfTuesdayLocation()
        {
            List<Location> locations = TestUtility.getLocations();
            Location homeLocation = locations[0];
            //int mondayLocationIndex = random.Next(locations.Count);
            Location mondayLocation = homeLocation;
            Location TuesdayLocation = locations[1];
            List<CalendarEvent> mondayEvents = new List<CalendarEvent>();
            List<CalendarEvent> tuesdayEvents = new List<CalendarEvent>();
            TimeSpan durationOfCalEvent = TimeSpan.FromHours(1);
            DateTimeOffset startOfDay = DateTimeOffset.Parse("2:00am");
            DateTimeOffset refNow = DateTimeOffset.Parse("11/10/2017 3:00am");
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
        /// Test creates a subeevents tries pushing a sub event and pushing the sub event should result in a worse optimized schedule and thus generate an assesss value with a lesser score.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WhatIfIPushedEvent()
        {
            List<Location> locations = TestUtility.getLocations();
            Dictionary<string, Location> location_dict = locations.ToDictionary(location => location.Description.ToLower(), location => location);
            Location desiredLocation = locations[1];
            List<CalendarEvent> mondayEvents = new List<CalendarEvent>();
            List<CalendarEvent> tuesdayEvents = new List<CalendarEvent>();
            TimeSpan durationOfCalEvent = TimeSpan.FromHours(1);
            DateTimeOffset refNow = DateTimeOffset.Parse("12/20/2017 3:00AM");
            DateTimeOffset startOfDay = DateTimeOffset.Parse("12/20/2017 2:00AM");
            DateTimeOffset endOfDay = startOfDay.AddDays(1);

            Location home = location_dict["home"];
            Location gym = location_dict["gym"];
            Location shaker = location_dict["shaker library"];
            Location work = location_dict["work"];

            CalendarEvent homeEventA = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false, home);
            CalendarEvent homeEventB = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false, home);
            CalendarEvent workEventA = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false, work);
            CalendarEvent gymEventA = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false, gym);
            CalendarEvent shakerEventA = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false, shaker);
            List<CalendarEvent> allCalendarEvents = new List<CalendarEvent>() { homeEventA, homeEventB, workEventA, gymEventA, shakerEventA};
            UserAccount currentUser = TestUtility.getTestUser();
            foreach (CalendarEvent calEvent in allCalendarEvents)
            {
                DB_Schedule eachSchedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
                eachSchedule.AddToScheduleAndCommit(calEvent, true).Wait();
            }

            DB_Schedule findSomethingTodSchedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
            findSomethingTodSchedule.FindMeSomethingToDo(home).Wait();
            DB_Schedule pushSchedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
            workEventA = pushSchedule.getCalendarEvent(workEventA.getId);
            SubCalendarEvent procrastinateSubevent = workEventA.ActiveSubEvents.First();
            EventID subEventId = new EventID(procrastinateSubevent.getId);
            DateTimeOffset limitOfProcrastination = procrastinateSubevent.getCalculationRange.End.AddHours(-2);
            DateTimeOffset start = findSomethingTodSchedule.Now.constNow > procrastinateSubevent.Start ? findSomethingTodSchedule.Now.constNow : procrastinateSubevent.Start;
            var Procrastinationpan = limitOfProcrastination - start;
            var beforAfteranalysis = await pushSchedule.WhatIfPushed(Procrastinationpan, subEventId, null);
            Assert.IsTrue(beforAfteranalysis.Item1.getScore() < beforAfteranalysis.Item2.getScore());
        }

        /// <summary>
        /// Test creates a subeevents tries pushing a sub event and pushing the sub event should result in a worse optimized schedule and thus generate an assesss value with a lesser score.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WhatIfIPushedAllEvents()
        {
            List<Location> locations = TestUtility.getLocations();
            Dictionary<string, Location> location_dict = locations.ToDictionary(location => location.Description.ToLower(), location => location);
            Location desiredLocation = locations[1];
            List<CalendarEvent> mondayEvents = new List<CalendarEvent>();
            List<CalendarEvent> tuesdayEvents = new List<CalendarEvent>();
            TimeSpan durationOfCalEvent = TimeSpan.FromHours(1);
            DateTimeOffset refNow = DateTimeOffset.Parse("12/20/2017 3:00AM");
            DateTimeOffset startOfDay = DateTimeOffset.Parse("12/20/2017 2:00AM");
            DateTimeOffset endOfDay = startOfDay.AddDays(1);

            Location home = location_dict["home"];
            Location gym = location_dict["gym"];
            Location shaker = location_dict["shaker library"];
            Location work = location_dict["work"];

            CalendarEvent homeEventA = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false, home);
            CalendarEvent homeEventB = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false, home);
            CalendarEvent workEventA = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false, work);
            CalendarEvent gymEventA = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false, gym);
            CalendarEvent shakerEventA = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false, shaker);
            List<CalendarEvent> allCalendarEvents = new List<CalendarEvent>() { homeEventA, homeEventB, workEventA, gymEventA, shakerEventA };
            UserAccount currentUser = TestUtility.getTestUser();
            foreach (CalendarEvent calEvent in allCalendarEvents)
            {
                DB_Schedule eachSchedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
                eachSchedule.AddToScheduleAndCommit(calEvent, true).Wait();
            }

            DB_Schedule findSomethingTodSchedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
            findSomethingTodSchedule.FindMeSomethingToDo(home).Wait();
            DB_Schedule pushSchedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
            workEventA = pushSchedule.getCalendarEvent(workEventA.getId);
            var Procrastinationpan = TimeSpan.FromHours(19);
            var beforAfteranalysis = await pushSchedule.WhatIfPushedAll(Procrastinationpan, null);
            Assert.IsTrue(beforAfteranalysis.Item1.evaluatePositioning() < beforAfteranalysis.Item2.evaluatePositioning());
            Assert.IsTrue(beforAfteranalysis.Item1.getScore() < beforAfteranalysis.Item2.getScore());
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
            Location desiredLocation = locations[1];
            List<CalendarEvent> mondayEvents = new List<CalendarEvent>();
            List<CalendarEvent> tuesdayEvents = new List<CalendarEvent>();
            TimeSpan durationOfCalEvent = TimeSpan.FromHours(1);
            DateTimeOffset refNow = DateTimeOffset.Parse("11/6/2017 12:00AM");
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
                DB_Schedule eachSchedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
                eachSchedule.AddToScheduleAndCommit(calEvent).Wait();
            }

            DateTimeOffset wednesdayStart = mondayStart.AddDays(2);
            CalendarEvent wednesdayEvent = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), mondayStart, wednesdayStart.AddHours(1), 1, true, desiredLocation);
            DB_Schedule schedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
            schedule.AddToScheduleAndCommit(wednesdayEvent).Wait();
            schedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
            CalendarEvent retrievedWednesdayEvent = schedule.getCalendarEvent(wednesdayEvent.Calendar_EventID);

            schedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
            Health tuesdayHealth = await schedule.WhatIfDifferentDay(tuesdayStart, retrievedWednesdayEvent.ActiveSubEvents.First().SubEvent_ID).ConfigureAwait(false);
            schedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
            Health mondayHealth = await schedule.WhatIfDifferentDay(wednesdayStart.AddDays(-2), retrievedWednesdayEvent.ActiveSubEvents.First().SubEvent_ID).ConfigureAwait(false);
            HealthEvaluation mondayEvaluation = new HealthEvaluation(mondayHealth);
            HealthEvaluation tuesdayEvaluation = new HealthEvaluation(tuesdayHealth);

            double mondayScore = mondayHealth.getScore();
            double tuesdayScore = tuesdayHealth.getScore();

            Assert.IsTrue(tuesdayScore < mondayScore);// this is know to fail
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
