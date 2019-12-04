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
        //Random random = new Random((int)DateTimeOffset.UtcNow.Ticks);


        /// <summary>
        /// This test runs a what if scenario on the schedule, by moving an event to a different time.
        /// This test works by building two different schedules. Then adding a test event to be constrained to different days. 
        /// The day with events that are clustered close to the newly event is determined to be the better day
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WhatIfMondayInsteadOfTuesdayLocation()
        {
            var packet = TestUtility.CreatePacket();
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;

            List<Location> locations = TestUtility.getAdHocLocations(tilerUser.Id);
            Location homeLocation = locations[0];
            //int mondayLocationIndex = random.Next(locations.Count);
            Location mondayLocation = homeLocation;
            Location TuesdayLocation = locations[1];
            TestUtility.addLocation(user, tilerUser, mondayLocation);
            TestUtility.addLocation(user, tilerUser, TuesdayLocation);

            List<CalendarEvent> mondayEvents = new List<CalendarEvent>();
            List<CalendarEvent> tuesdayEvents = new List<CalendarEvent>();
            TimeSpan durationOfCalEvent = TimeSpan.FromHours(1);
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
            DateTimeOffset refNow = TestUtility.parseAsUTC("11/10/2017 3:00am");
            DateTimeOffset mondayStart = getNextDateForDayOfWeek(DayOfWeek.Monday, refNow);
            DateTimeOffset tuesdayStart = mondayStart.AddDays(1);
            int numberOfEvents = 5;
            for (int i = 0; i < numberOfEvents; i++)
            {
                TestUtility.reloadTilerUser(ref user, ref tilerUser);
                DB_Schedule eachSchedule = new DB_Schedule(user, refNow);
                CalendarEvent mondayEvent = TestUtility.generateCalendarEvent(tilerUser, durationOfCalEvent, new Repetition(), mondayStart, mondayStart.AddDays(1), 1, false);
                mondayEvent.LocationId = mondayLocation.Id;
                mondayEvent.AllSubEvents.AsParallel().ForAll((eachSubEvent) => eachSubEvent.LocationId = mondayLocation.Id);
                eachSchedule.AddToScheduleAndCommit(mondayEvent);

                CalendarEvent tuesdayEvent = TestUtility.generateCalendarEvent(tilerUser, durationOfCalEvent, new Repetition(), tuesdayStart, tuesdayStart.AddDays(1), 1, false);
                tuesdayEvent.LocationId = TuesdayLocation.Id;
                tuesdayEvent.AllSubEvents.AsParallel().ForAll((eachSubEvent) => eachSubEvent.LocationId = TuesdayLocation.Id);
                eachSchedule.AddToScheduleAndCommit(tuesdayEvent);

                mondayEvents.Add(mondayEvent);
                tuesdayEvents.Add(tuesdayEvent);


            }
            List<CalendarEvent> allCalendarEvents = mondayEvents.Concat(tuesdayEvents).ToList();
            DateTimeOffset wednesdayStart = mondayStart.AddDays(2);// getNextDateForDayOfWeek(DayOfWeek.Wednesday, refNow);
            CalendarEvent wednesdayEvent = TestUtility.generateCalendarEvent(tilerUser, durationOfCalEvent, new Repetition(), mondayStart, wednesdayStart.AddDays(1), 1, false);
            TestUtility.updateLocation(wednesdayEvent, TuesdayLocation);
            DB_Schedule schedule = new DB_Schedule(user, refNow);
            schedule.AddToScheduleAndCommit(wednesdayEvent);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            CalendarEvent retrievedWednesdayEvent = schedule.getCalendarEvent(wednesdayEvent.Calendar_EventID);

            SubCalendarEvent whatIfSubEvent = retrievedWednesdayEvent.ActiveSubEvents.First();
            schedule = new DB_Schedule(user, refNow);
            Health tuesdayHealth = await schedule.WhatIfDifferentDay(tuesdayStart, whatIfSubEvent.SubEvent_ID).ConfigureAwait(false);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new DB_Schedule(user, refNow);
            Health mondayHealth = await schedule.WhatIfDifferentDay(wednesdayStart.AddDays(-2), whatIfSubEvent.SubEvent_ID).ConfigureAwait(false);
            HealthEvaluation mondayEvaluation = new HealthEvaluation(mondayHealth);
            HealthEvaluation tuesdayEvaluation = new HealthEvaluation(tuesdayHealth);

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
            var packet = TestUtility.CreatePacket();
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;

            List<Location> locations = TestUtility.getAdHocLocations(tilerUser.Id);
            TestUtility.addLocations(user, tilerUser, locations);
            Dictionary<string, Location> location_dict = locations.ToDictionary(location => location.Description.ToLower(), location => location);
            Location desiredLocation = locations[1];
            List<CalendarEvent> mondayEvents = new List<CalendarEvent>();
            List<CalendarEvent> tuesdayEvents = new List<CalendarEvent>();
            TimeSpan durationOfCalEvent = TimeSpan.FromHours(1);
            DateTimeOffset refNow = TestUtility.parseAsUTC("12/20/2017 3:00AM");
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("12/20/2017 2:00AM");
            DateTimeOffset endOfDay = startOfDay.AddDays(1);

            Location home = location_dict["home"];
            Location gym = location_dict["gym"];
            Location shaker = location_dict["shaker library"];
            Location work = location_dict["work"];
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DB_Schedule eachSchedule = new TestSchedule(user, refNow, EventID.LatestID);
            
            CalendarEvent homeEventA = TestUtility.generateCalendarEvent(tilerUser, durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false);
            TestUtility.updateLocation(homeEventA, home);
            eachSchedule.AddToScheduleAndCommit(homeEventA, true);

            CalendarEvent homeEventB = TestUtility.generateCalendarEvent(tilerUser, durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false);
            TestUtility.updateLocation(homeEventB, home);
            eachSchedule.AddToScheduleAndCommit(homeEventB, true);

            CalendarEvent gymEventA = TestUtility.generateCalendarEvent(tilerUser, durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false);
            TestUtility.updateLocation(gymEventA, gym);
            eachSchedule.AddToScheduleAndCommit(gymEventA, true);

            CalendarEvent shakerEventA = TestUtility.generateCalendarEvent(tilerUser, durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false);
            TestUtility.updateLocation(shakerEventA, shaker);
            eachSchedule.AddToScheduleAndCommit(shakerEventA, true);


            CalendarEvent workEventA = TestUtility.generateCalendarEvent(tilerUser, durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false); // the order matters because a sub event is selected based on the order it was created
            TestUtility.updateLocation(workEventA, work);
            eachSchedule.AddToScheduleAndCommit(workEventA, true);


            List<CalendarEvent> allCalendarEvents = new List<CalendarEvent>() { homeEventA, homeEventB, workEventA, gymEventA, shakerEventA };

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DB_Schedule findSomethingTodSchedule = new TestSchedule(user, refNow, EventID.LatestID);
            findSomethingTodSchedule.FindMeSomethingToDo(home).Wait();
            findSomethingTodSchedule.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DB_Schedule pushSchedule = new TestSchedule(user, refNow, EventID.LatestID);
            workEventA = pushSchedule.getCalendarEvent(workEventA.getId);
            SubCalendarEvent procrastinateSubevent = workEventA.ActiveSubEvents.First();
            EventID subEventId = new EventID(procrastinateSubevent.getId);
            DateTimeOffset limitOfProcrastination = procrastinateSubevent.getCalculationRange.End.AddHours(-2);
            DateTimeOffset start = findSomethingTodSchedule.Now.constNow > procrastinateSubevent.Start ? findSomethingTodSchedule.Now.constNow : procrastinateSubevent.Start;
            var Procrastinationpan = limitOfProcrastination - start;
            var beforAfteranalysis = await pushSchedule.WhatIfPushed(Procrastinationpan, subEventId, null);
            Assert.IsTrue(beforAfteranalysis.Item1.getScore() < beforAfteranalysis.Item2.getScore());
        }

        ///// <summary>
        ///// Test creates a subeevents tries pushing a sub event and pushing the sub event should result in a worse optimized schedule and thus generate an assesss value with a lesser score.
        ///// </summary>
        ///// <returns></returns>
        //[TestMethod]
        //public async Task WhatIfIPushedAllEvents()
        //{
        //    List<Location> locations = TestUtility.getLocations();
        //    Dictionary<string, Location> location_dict = locations.ToDictionary(location => location.Description.ToLower(), location => location);
        //    Location desiredLocation = locations[1];
        //    List<CalendarEvent> mondayEvents = new List<CalendarEvent>();
        //    List<CalendarEvent> tuesdayEvents = new List<CalendarEvent>();
        //    TimeSpan durationOfCalEvent = TimeSpan.FromHours(1);
        //    DateTimeOffset refNow = TestUtility.parseAsUTC("12/20/2017 3:00AM");
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("12/20/2017 2:00AM");
        //    DateTimeOffset endOfDay = startOfDay.AddDays(1);

        //    Location home = location_dict["home"];
        //    Location gym = location_dict["gym"];
        //    Location shaker = location_dict["shaker library"];
        //    Location work = location_dict["work"];

        //    CalendarEvent homeEventA = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false, home);
        //    CalendarEvent homeEventB = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false, home);
        //    CalendarEvent workEventA = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false, work);
        //    CalendarEvent gymEventA = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false, gym);
        //    CalendarEvent shakerEventA = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), startOfDay, endOfDay, 1, false, shaker);
        //    List<CalendarEvent> allCalendarEvents = new List<CalendarEvent>() { homeEventA, homeEventB, workEventA, gymEventA, shakerEventA };
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    foreach (CalendarEvent calEvent in allCalendarEvents)
        //    {
        //        DB_Schedule eachSchedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
        //        eachSchedule.AddToScheduleAndCommit(calEvent, true).Wait();
        //    }

        //    DB_Schedule findSomethingTodSchedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
        //    findSomethingTodSchedule.FindMeSomethingToDo(home).Wait();
        //    DB_Schedule pushSchedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
        //    workEventA = pushSchedule.getCalendarEvent(workEventA.getId);
        //    var Procrastinationpan = TimeSpan.FromHours(19);
        //    var beforAfteranalysis = await pushSchedule.WhatIfPushedAll(Procrastinationpan, null);
        //    Assert.IsTrue(beforAfteranalysis.Item1.evaluatePositioning() < beforAfteranalysis.Item2.evaluatePositioning());
        //    Assert.IsTrue(beforAfteranalysis.Item1.getScore() < beforAfteranalysis.Item2.getScore());
        //}



        /// <summary>
        /// Test loads a schedule dump and the dump has an example screenshot ,in the description, of what is expected. Essentially, if I clear schedule for six hours what are the consequences.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WhatIfIPushedAllFromDumpEvents()
        {
            string scheduleId = "0897a52c-708a-46cb-85f9-bbf26d6f7688";
            DateTimeOffset currentTime = new DateTimeOffset(2019, 12, 4, 11, 0, 0, new TimeSpan());
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            
            var pushScheduleDump = TestUtility.getSchedule(scheduleId, currentTime);
            DB_Schedule pushSchedule = (TestSchedule)pushScheduleDump.Item1;
            var Procrastinationpan = TimeSpan.FromHours(6);
            var beforAfteranalysis = await pushSchedule.WhatIfPushedAll(Procrastinationpan, null);
            var sleepEvaluation = beforAfteranalysis.Item2.evaluateSleepTimeFrameScore();

            Assert.IsTrue(beforAfteranalysis.Item1.evaluatePositioning() < beforAfteranalysis.Item2.evaluatePositioning());
            Assert.IsTrue(beforAfteranalysis.Item1.getScore() < beforAfteranalysis.Item2.getScore());
        }


        ///// <summary>
        ///// This test runs a what if scenario on the schedule, by checking if moving the event to a different time will cause it to have an unfavorable schedule. 
        ///// This test is targeted at modifying the schedule such that there is a conflict. A conflicting schedule should be less favorable than one that doesnt conflict
        ///// </summary>
        ///// <returns></returns>
        //[TestMethod]
        //public async Task WhatIfMondayInsteadOfTuesdayConflict()
        //{
        //    List<Location> locations = TestUtility.getLocations();
        //    Location desiredLocation = locations[1];
        //    List<CalendarEvent> mondayEvents = new List<CalendarEvent>();
        //    List<CalendarEvent> tuesdayEvents = new List<CalendarEvent>();
        //    TimeSpan durationOfCalEvent = TimeSpan.FromHours(1);
        //    DateTimeOffset refNow = TestUtility.parseAsUTC("11/6/2017 12:00AM");
        //    DateTimeOffset mondayStart = getNextDateForDayOfWeek(DayOfWeek.Monday, refNow);
        //    DateTimeOffset tuesdayStart = mondayStart.AddDays(1);
        //    DateTimeOffset mondayStartCopy = mondayStart;
        //    DateTimeOffset tuesdayStartCopy = tuesdayStart.AddHours(6);
        //    int numberOfEvents = 5;
        //    for (int i = 0; i < numberOfEvents; i++)
        //    {
        //        CalendarEvent mondayEvent = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), mondayStartCopy, mondayStartCopy.AddHours(1), 1, true, desiredLocation);
        //        CalendarEvent tuesdayEvent = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), tuesdayStartCopy, tuesdayStartCopy.AddHours(1), 1, true, desiredLocation);
        //        mondayStartCopy = mondayStartCopy.AddHours(1);
        //        tuesdayStartCopy = tuesdayStartCopy.AddHours(1);
        //        mondayEvents.Add(mondayEvent);
        //        tuesdayEvents.Add(tuesdayEvent);
        //    }
        //    List<CalendarEvent> allCalendarEvents = mondayEvents.Concat(tuesdayEvents).ToList();
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    foreach (CalendarEvent calEvent in allCalendarEvents)
        //    {
        //        DB_Schedule eachSchedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
        //        eachSchedule.AddToScheduleAndCommit(calEvent).Wait();
        //    }

        //    DateTimeOffset wednesdayStart = mondayStart.AddDays(2);
        //    CalendarEvent wednesdayEvent = TestUtility.generateCalendarEvent(durationOfCalEvent, new Repetition(), mondayStart, wednesdayStart.AddHours(1), 1, true, desiredLocation);
        //    DB_Schedule schedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
        //    schedule.AddToScheduleAndCommit(wednesdayEvent).Wait();
        //    schedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
        //    CalendarEvent retrievedWednesdayEvent = schedule.getCalendarEvent(wednesdayEvent.Calendar_EventID);

        //    schedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
        //    Health tuesdayHealth = await schedule.WhatIfDifferentDay(tuesdayStart, retrievedWednesdayEvent.ActiveSubEvents.First().SubEvent_ID).ConfigureAwait(false);
        //    schedule = new TestSchedule(currentUser, refNow, EventID.LatestID);
        //    Health mondayHealth = await schedule.WhatIfDifferentDay(wednesdayStart.AddDays(-2), retrievedWednesdayEvent.ActiveSubEvents.First().SubEvent_ID).ConfigureAwait(false);
        //    HealthEvaluation mondayEvaluation = new HealthEvaluation(mondayHealth);
        //    HealthEvaluation tuesdayEvaluation = new HealthEvaluation(tuesdayHealth);

        //    double mondayScore = mondayHealth.getScore();
        //    double tuesdayScore = tuesdayHealth.getScore();

        //    Assert.IsTrue(tuesdayScore < mondayScore);// this is known to fail
        //}

        public DateTimeOffset getNextDateForDayOfWeek(DayOfWeek dayOfeek, DateTimeOffset referenceTime)
        {
            DateTimeOffset retValue;

            if (referenceTime.DayOfWeek != dayOfeek)
            {
                //int dayCount = ((int)referenceTime.DayOfWeek + 7);
                int dayDiff = dayOfeek - referenceTime.DayOfWeek;
                if (dayDiff > 0)
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
