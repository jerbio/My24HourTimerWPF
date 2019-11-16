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

        [TestMethod]
        public void file_unnecessary_shifiting_back_and_forth_of_workout_7f453aa2()
        {
            string scheduleId = "7f453aa2-c1d4-4a5e-9121-b33b4176e98c";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            Schedule schedule = scheduleAndDump.Item1;
            TimeLine firstDay = schedule.Now.firstDay;
            List<SubCalendarEvent> subEvents = schedule.getAllActiveSubEvents().Where(obj => obj.StartToEnd.doesTimeLineInterfere(firstDay)).OrderBy(o => o.Start).ToList();
            schedule.ProcrastinateAll(TimeSpan.FromMinutes(2));
            List<SubCalendarEvent> subEventsAfterProcrastinate = schedule.getAllActiveSubEvents().Where(obj => obj.StartToEnd.doesTimeLineInterfere(firstDay)).OrderBy(o => o.Start).ToList();

            Assert.AreEqual(subEventsAfterProcrastinate.Count, subEvents.Count);

            for(int i = 0; i< subEvents.Count; i++ )
            {
                Assert.AreEqual(subEvents[i].Id, subEventsAfterProcrastinate[i].Id);
            }

            ((TestSchedule)schedule).WriteFullScheduleToOutlook();
        }

        /// <summary>
        /// There is no sleep time in the morning for November 17 
        /// </summary>
        [TestMethod]
        public void file_missing_sleep_time_chunk_f02c36aa()
        {
            string scheduleId = "f02c36aa-cdcf-446d-97e0-f4b8ce3b548c";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            Schedule schedule = scheduleAndDump.Item1;
            schedule.FindMeSomethingToDo(currentLocation).Wait();

            ((TestSchedule)schedule).WriteFullScheduleToOutlook();
        }

        [TestMethod]
        public void file_unnecessary_morning_scheduling_0a0e2ca8()
        {
            string scheduleId = "0a0e2ca8-62b7-4336-93ee-49a0d1039073";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            Schedule schedule = scheduleAndDump.Item1;
            schedule.FindMeSomethingToDo(currentLocation).Wait();
            DateTimeOffset oct25 = new DateTimeOffset(2019, 10, 25, 14, 0, 0, new TimeSpan());
            DateTimeOffset oct26 = new DateTimeOffset(2019, 10, 26, 14, 0, 0, new TimeSpan());

            DayTimeLine oct25DayTimeLine = schedule.Now.getDayTimeLineByTime(oct25);
            DayTimeLine oct26DayTimeLine = schedule.Now.getDayTimeLineByTime(oct26);

            List<SubCalendarEvent> oct25subEvents = oct25DayTimeLine.getSubEventsInTimeLine().OrderBy(o => o.Start).ToList();
            List<SubCalendarEvent> oct26subEvents = oct26DayTimeLine.getSubEventsInTimeLine().OrderBy(o => o.Start).ToList();

            foreach(SubCalendarEvent subEvent in oct25subEvents)
            {
                Assert.IsTrue(subEvent.Start >= oct25);
            }

            foreach (SubCalendarEvent subEvent in oct26subEvents)
            {
                Assert.IsTrue(subEvent.Start >= oct26);// this is KnownBugList to fail
            }

            ((TestSchedule)schedule).WriteFullScheduleToOutlook();
        }

        /// <summary>
        /// In this test the current location is 39.710835, -104.812500 which is in Aurora, CO.
        /// The event named "Get a hair cut" with the Id 0ada4cb8-844e-41cb-a3c3-e2b7863e365a_7_0_92db8eb5-9c7a-498b-af94-7385bf67b042 is in Auroa Colorado so it should be the next event
        /// </summary>
        [TestMethod]
        public void file_currentLocation_should_schew_next_event_when_shuffling_61651f57()
        {
            string scheduleId = "61651f57-0cc3-4da1-bbca-c655013d0642";
            Location currentLocation = new TilerElements.Location(39.710835, -104.812500, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            Schedule schedule = scheduleAndDump.Item1;
            List<SubCalendarEvent> subEvents = schedule.getAllActiveSubEvents().OrderBy(o => o.Start).ToList();
            TimeLine currentTimeline = new TimeLine(schedule.Now.constNow, schedule.Now.constNow.AddDays(120));
            var subEventsInTimeLine = subEvents.Where(sub => sub.StartToEnd.doesTimeLineInterfere(currentTimeline));
            SubCalendarEvent firstSubEvent = subEventsInTimeLine.First();
            string subEventId = "0ada4cb8-844e-41cb-a3c3-e2b7863e365a_7_0_92db8eb5-9c7a-498b-af94-7385bf67b042";
            Assert.IsFalse(firstSubEvent.Id == subEventId);
            schedule.FindMeSomethingToDo(currentLocation).Wait();

            List<SubCalendarEvent> subEventAfterShuffle = schedule.getAllActiveSubEvents().OrderBy(o => o.Start).ToList();
            subEventsInTimeLine = subEventAfterShuffle.Where(sub => sub.StartToEnd.doesTimeLineInterfere(currentTimeline));
            SubCalendarEvent firstSubEventRetrieved = subEventsInTimeLine.First();
            Assert.IsTrue(firstSubEventRetrieved.Id == subEventId);

            ((TestSchedule)schedule).WriteFullScheduleToOutlook();
        }

        /// <summary>
        /// Test captures the scenario where a subevent shifts between multiple daytimeline caused by tryconflict resolution
        /// </summary>
        [TestMethod]
        public void file_conflictResolution_shifting_event_e8642806()
        {
            string scheduleId = "e8642806-2a88-4443-aa3a-f19dca5d37c2";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            Schedule schedule = scheduleAndDump.Item1;
            schedule.ProcrastinateAll(TimeSpan.FromMinutes(30));
            ((TestSchedule)schedule).WriteFullScheduleToOutlook();
        }

        [TestMethod]
        public void file_cannot_update_restricted_subevent_aad38888()
        {
            string scheduleId = "aad38888-ed1d-435f-a814-0152c0fe8128";
            string subEventId = "721935a5-e51a-4399-9337-e12d13c92c03_7_ae10acca-247b-4502-b9a8-744219717cac_57ab1560-deca-4985-bdb1-1f4262229b9f";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            Schedule schedule = scheduleAndDump.Item1;
            SubCalendarEvent subEvent = schedule.getSubCalendarEvent(subEventId);
            DateTimeOffset newSubEventEndTime = subEvent.End.AddHours(2);
            schedule.BundleChangeUpdate(subEventId, subEvent.Name.createCopy(), subEvent.Start, newSubEventEndTime, subEvent.ParentCalendarEvent.Start, subEvent.ParentCalendarEvent.End, subEvent.ParentCalendarEvent.NumberOfSplit, subEvent.ParentCalendarEvent.Notes.UserNote);
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
            List<SubCalendarEvent> subEvents = schedule.getAllCalendarEvents().SelectMany(cal => cal.ActiveSubEvents).Where(subEvent => subEvent.StartToEnd.doesTimeLineInterfere(conflictingSubEvent.StartToEnd)).ToList();
            ((TestSchedule)schedule).WriteFullScheduleToOutlook();
            Assert.AreEqual(subEvents.Count, 1);// the look up should only conflict with itself

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
