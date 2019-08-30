using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using My24HourTimerWPF;
using TilerElements;
using System.Collections.Generic;
using System.Linq;
using TilerCore;

namespace TilerTests
{
    [TestClass]
    public class KnownBugList
    {

        /*
         * This test tries to see that there is sufficent diversity. Read notes scheule dump nodes
         * The 
        */
        /// <summary>
        /// Template for running test environment through log files.
        /// You need to run this in UTC TimeZone
        /// </summary>

        [TestMethod]
        public void file_template()
        {
            //string scheduleId = "24fe78f8-b9a9-4ca3-b4a1-cf5d458fe385";
            //Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            //var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            //Schedule schedule = scheduleAndDump.Item1;
            //((TestSchedule)schedule).WriteFullScheduleToOutlook();
        }

        /// <summary>
        /// Test catches scenario where trying to reschedule an event name "Get a hair cut" caused the server to crash.
        /// This was caused by the inappropriate undesignation of a subevent
        /// Note the image https://drive.google.com/file/d/1UK8HgDeFRH5Ffz7QiR7ke9sQB5FmdNip/view is no longer valid. 
        /// However, It is still the same reschedule error, the updated deadline should be 11:59pm 09/01/2019 instead of Aug 25 as suggested in the email
        /// </summary>

        [TestMethod]
        public void file_Undesignate_calEvent_causes_conflict_resolution_0e062820()
        {
            string scheduleId = "0e062820-967c-4afc-aa45-9ffd9a0a3e3d";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            Schedule schedule = scheduleAndDump.Item1;
            string eventId = "0ada4cb8-844e-41cb-a3c3-e2b7863e365a_7_0_0";
            CalendarEvent calendarEvent = schedule.getCalendarEvent(eventId);
            SubCalendarEvent subEvent = calendarEvent.ActiveSubEvents.First();
            DateTimeOffset end = calendarEvent.End;
            DateTimeOffset updatedEndTIme = new DateTimeOffset(end.Year, 9, 1, end.Hour, end.Minute, end.Second, new TimeSpan());
            EventName eventName = calendarEvent.Name.createCopy();
            schedule.BundleChangeUpdate(subEvent.Id, eventName, subEvent.Start, subEvent.End, calendarEvent.Start, updatedEndTIme, calendarEvent.NumberOfSplit, calendarEvent.Notes.UserNote);
            ((TestSchedule)schedule).WriteFullScheduleToOutlook();
        }

        /// <summary>
        /// This is a schedule health bug, on 08/18/2019 the events were scheduled towards the end of the day instead of towards the middle of the day
        /// THis test verifies the events are within the middle of the day
        /// </summary>
        [TestMethod]
        public void file_59754086()
        {
            string scheduleId = "59754086-6192-4364-a364-dffb4c71d7b6";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            Schedule schedule = scheduleAndDump.Item1;
            schedule.FindMeSomethingToDo(currentLocation).Wait();
            DateTimeOffset referenceDay = TestUtility.parseAsUTC("08/18/2019");

            DayTimeLine dayTimeLine = schedule.Now.getDayTimeLineByTime(referenceDay);
            TimeLine middleOfDay = new TimeLine(dayTimeLine.Start.AddHours(8), dayTimeLine.End.AddHours(-8));
            List<SubCalendarEvent>allSubEvents = dayTimeLine.getSubEventsInTimeLine();
            List<SubCalendarEvent> allSubEventsWithinTimeline = allSubEvents.Where(subEvent => middleOfDay.IsTimeLineWithin(subEvent.ActiveSlot)).ToList();
            Assert.AreEqual(allSubEvents.Count, allSubEventsWithinTimeline.Count);

            ((TestSchedule)schedule).WriteFullScheduleToOutlook();
        }

        /// <summary>
        /// "Tope in colorado" event fix. This test captures a scenario where an event with an earlier deadline but lesser duration was creating a conflict. 
        /// The tope in colorado event was scheduled to have less than a day in calendarevent rangetimeline duration and it also had a duration of about 1hour 30 mins.
        /// A conflict was created because it wasn't being prioritized because it had a duration of 1hour 30 minute event around 2 hour events. 
        /// However, it should have gotten higher priority because of the time chunk available for it to get scheduled. It was a 1hr30 min only available for less than a day while the 2 hour events had the rest of the week as possible days
        /// </summary>
        [TestMethod]
        public void file_92207108_Tope_In_Colorado()
        {
            string scheduleId = "92207108-8796-4f67-a945-5018db4ffb9d";
            string subEventId = "7e6771cc-d6c8-41f1-8c37-ca54de5ed00c_7_0_d60e3b98-ee6a-49e2-b568-ecc6167441a8";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            Schedule schedule = scheduleAndDump.Item1;
            schedule.FindMeSomethingToDo(currentLocation).Wait();
            SubCalendarEvent conflictingSubEvent = schedule.getSubCalendarEvent(subEventId);
            List<SubCalendarEvent> subEvents = schedule.getAllCalendarEvents().SelectMany(cal => cal.ActiveSubEvents).Where(subEvent => subEvent.RangeTimeLine.doesTimeLineInterfere(conflictingSubEvent.RangeTimeLine)).ToList();
            Assert.AreEqual(subEvents.Count, 1);// the look up should only conflict with itself
            ((TestSchedule)schedule).WriteFullScheduleToOutlook();
        }

        public void add9_5WorkSchedule (Schedule schedule)
        {
            string date = "" + schedule.Now.constNow.Month + "/" + schedule.Now.constNow.Day + "/" + schedule.Now.constNow.Year;
            TimeLine timeLine = new TimeLine(TestUtility.parseAsUTC("3:00pm " + date), TestUtility.parseAsUTC("11:00pm " + date));
            TimeLine rangeOfTempGoogleEvent = new TimeLine(timeLine.Start, timeLine.Start.AddYears(1));
            DayOfWeek[] weekDays = (new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }).ToArray();
            EventName eventName = new EventName(null, null, "google-made-up");
            Repetition repeat = new Repetition(rangeOfTempGoogleEvent, Repetition.Frequency.WEEKLY, timeLine, weekDays);
            CalendarEvent googleCalSimulation = TestUtility.generateCalendarEvent(
                schedule.User,
                TimeSpan.FromHours(8),
                repeat,
                rangeOfTempGoogleEvent.Start,
                rangeOfTempGoogleEvent.End,
                1,
                true);
            schedule.AddToSchedule(googleCalSimulation);
        }


        //[TestMethod]
        //public void file_e94713a5()
        //{
        //    string currentClearAllId = "4939920_7_0_0";
        //    Location homeLocation = TestUtility.getLocations()[0];
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
        //    DateTimeOffset refNow = TestUtility.parseAsUTC("12/31/2017 9:10pm");
        //    UserAccount currentUser = TestUtility.getTestUser(userId: "e94713a5-9ab6-4c6b-9b46-edafa4b0dafc");
        //    currentUser.getTilerUser().ClearAllId = currentClearAllId;
        //    TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
        //    schedule.FindMeSomethingToDo(homeLocation).Wait();
        //    schedule.WriteFullScheduleToLogAndOutlook().Wait();
        //    DayTimeLine day0 = schedule.Now.getDayTimeLineByTime(refNow.AddDays(0));
        //    DayTimeLine day1 = schedule.Now.getDayTimeLineByTime(refNow.AddDays(1));
        //    TimeLine sleepTimeLine = new TimeLine(day0.SleepSubEvent.End, day1.WakeSubEvent.Start);
        //    Assert.IsTrue(sleepTimeLine.TimelineSpan > TimeSpan.FromHours(5));// This is known to fail
        //}

        ///// <summary>
        ///// There is an unnecessary conflict on February 27 and shuffling doesn't help. Shuffling the schedule should sort out the conflict. I feel it has something to do with the end of the day.
        ///// </summary>
        //[TestMethod]
        //public void file_6439fc14()
        //{
        //    string currentClearAllId = "4939920_7_0_0";
        //    Location homeLocation = TestUtility.getLocations()[0];
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
        //    DateTimeOffset refNow = TestUtility.parseAsUTC("2/27/2018 12:36am");
        //    TimeLine conflictingTimeline = new TimeLine(refNow.AddDays(-1), refNow.AddDays(1));
        //    UserAccount currentUser = TestUtility.getTestUser(userId: "6439fc14-ad0d-419f-acc8-86b17cc100c2");
        //    currentUser.getTilerUser().ClearAllId = currentClearAllId;
        //    TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
        //    List<SubCalendarEvent> allSubeventsWInthinPertinentTImeLine = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.ActiveSubEvents).Where(subEvent => subEvent.ActiveSlot.doesTimeLineInterfere(conflictingTimeline)).ToList();
        //    Utility.ConflictEvaluation conflictEvaluation = new Utility.ConflictEvaluation(allSubeventsWInthinPertinentTImeLine);
        //    Assert.IsTrue(conflictEvaluation.ConflictingTimeRange.Count() > 0);
        //    schedule.FindMeSomethingToDo(homeLocation).Wait();
        //    schedule.WriteFullScheduleToLogAndOutlook().Wait();
        //    allSubeventsWInthinPertinentTImeLine = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.ActiveSubEvents).Where(subEvent => subEvent.ActiveSlot.doesTimeLineInterfere(conflictingTimeline)).ToList();
        //    conflictEvaluation = new Utility.ConflictEvaluation(allSubeventsWInthinPertinentTImeLine);
        //    Assert.IsTrue(conflictEvaluation.ConflictingTimeRange.Count() == 0);// This is known to fail
        //}

        ///// <summary>
        ///// There is a crash when I try to add a 45 min home locationed tile that deadlines at on 11:59p 2/26/2018 EST.
        ///// </summary>
        //[TestMethod]
        //public void file_2ndBug_6439fc14()
        //{
        //    string currentClearAllId = "4939920_7_0_0";
        //    Location homeLocation = TestUtility.getLocations()[0];
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
        //    DateTimeOffset refNow = TestUtility.parseAsUTC("2/27/2018 12:36am");
        //    UserAccount currentUser = TestUtility.getTestUser(userId: "6439fc14-ad0d-419f-acc8-86b17cc100c2");
        //    CalendarEvent calEvent = TestUtility.generateCalendarEvent(TimeSpan.FromMinutes(45), new Repetition(), refNow, refNow.AddHours(4), location: homeLocation);
        //    currentUser.getTilerUser().ClearAllId = currentClearAllId;
        //    TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
        //    schedule.AddToScheduleAndCommit(calEvent);
        //}

        //[TestCleanup]
        //public void eachTestCleanUp()
        //{
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    Schedule Schedule = new TestSchedule(currentUser, refNow);
        //    currentUser.DeleteAllCalendarEvents();
        //}
    }
}
