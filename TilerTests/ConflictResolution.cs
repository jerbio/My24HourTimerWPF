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
    public class ConflictResolution
    {

        [TestMethod]
        public void file_1edc6fe0()
        {
            Location homeLocation = TestUtility.getAdHocLocations()[0];
            string scheduleId = "1edc6fe0-0bd1-453e-a77f-7014814ee9ef";
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            TestSchedule schedule = (TestSchedule)scheduleAndDump.Item1;
            schedule.FindMeSomethingToDo(homeLocation).Wait();
            string timeInString = "11/21/2019 12:00:00 PM +00:00";
            DateTimeOffset timeWithinDayWithConflict = DateTimeOffset.Parse(timeInString);
            DayTimeLine dayTimeLineWithConflict = schedule.Now.getDayTimeLineByTime(timeWithinDayWithConflict);
            TimeLine dayTimeLineWithConflict_StartToEnd = dayTimeLineWithConflict.StartToEnd;
            HashSet<SubCalendarEvent> subEventsWithinDayOfCOnflict = new HashSet<SubCalendarEvent>(schedule.getAllActiveSubEvents().Where(suEvent => suEvent.StartToEnd.doesTimeLineInterfere(dayTimeLineWithConflict_StartToEnd)));

            schedule.WriteFullScheduleToOutlook();

            var conflict = Utility.getConflictingEvents(subEventsWithinDayOfCOnflict);
            Assert.AreEqual(conflict.Count, 1);
        }

        [TestMethod]
        public void file_b1b1ed4c()
        {
            Location homeLocation = TestUtility.getAdHocLocations()[0];
            string scheduleId = "b1b1ed4c-433b-4691-9a4d-76740d9a8f6a";
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            TestSchedule schedule = (TestSchedule)scheduleAndDump.Item1;
            schedule.FindMeSomethingToDo(homeLocation).Wait();
            string timeInString = "10/9/2019 12:00:00 PM +00:00";
            DateTimeOffset timeWithinDayWithConflict = DateTimeOffset.Parse(timeInString);
            DayTimeLine dayTimeLineWithConflict = schedule.Now.getDayTimeLineByTime(timeWithinDayWithConflict);
            TimeLine dayTimeLineWithConflict_StartToEnd = dayTimeLineWithConflict.StartToEnd;
            HashSet<SubCalendarEvent> subEventsWithinDayOfCOnflict = new HashSet<SubCalendarEvent>(schedule.getAllActiveSubEvents().Where(suEvent => suEvent.StartToEnd.doesTimeLineInterfere(dayTimeLineWithConflict_StartToEnd)));
            
            var conflict = Utility.getConflictingEvents(subEventsWithinDayOfCOnflict);
            Assert.AreEqual(conflict.Count, 1);
        }

        [TestMethod]
        public void file_d3c1c7a2()
        {
            Location homeLocation = TestUtility.getAdHocLocations()[0];
            string scheduleId = "d3c1c7a2-9e36-4200-a411-f068c25bb2a4";
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            TestSchedule schedule = (TestSchedule)scheduleAndDump.Item1;
            schedule.FindMeSomethingToDo(homeLocation).Wait();
            string timeInString = "11/26/2019 12:00:00 PM +00:00";
            DateTimeOffset timeWithinDayWithConflict = DateTimeOffset.Parse(timeInString);
            DayTimeLine dayTimeLineWithConflict = schedule.Now.getDayTimeLineByTime(timeWithinDayWithConflict);
            TimeLine dayTimeLineWithConflict_StartToEnd = dayTimeLineWithConflict.StartToEnd;
            HashSet<SubCalendarEvent> subEventsWithinDayOfCOnflict = new HashSet<SubCalendarEvent>(schedule.getAllActiveSubEvents().Where(suEvent => suEvent.StartToEnd.doesTimeLineInterfere(dayTimeLineWithConflict_StartToEnd)));

            schedule.WriteFullScheduleToOutlook();

            var conflict = Utility.getConflictingEvents(subEventsWithinDayOfCOnflict);
            Assert.AreEqual(conflict.Count, 0);
        }

        [TestMethod]
        public void file_9af810bf()
        {
            Location homeLocation = TestUtility.getAdHocLocations()[0];
            string scheduleId = "9af810bf-c610-4de7-adf1-2ac6d46feef4";
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            TestSchedule schedule = (TestSchedule)scheduleAndDump.Item1;
            schedule.FindMeSomethingToDo(homeLocation).Wait();
            string timeInString = "10/24/2019 12:00:00 PM +00:00";
            DateTimeOffset timeWithinDayWithConflict = DateTimeOffset.Parse(timeInString);
            DayTimeLine dayTimeLineWithConflict = schedule.Now.getDayTimeLineByTime(timeWithinDayWithConflict);
            TimeLine dayTimeLineWithConflict_StartToEnd = dayTimeLineWithConflict.StartToEnd;
            HashSet<SubCalendarEvent> subEventsWithinDayOfCOnflict = new HashSet<SubCalendarEvent>(schedule.getAllActiveSubEvents().Where(suEvent => suEvent.StartToEnd.doesTimeLineInterfere(dayTimeLineWithConflict_StartToEnd)));

            schedule.WriteFullScheduleToOutlook();

            var conflict = Utility.getConflictingEvents(subEventsWithinDayOfCOnflict).OrderByDescending(o=>o.End).ToList();
            Assert.AreEqual(conflict.Count, 3);
            var firstSubEvent = conflict.First();
            Assert.AreEqual(firstSubEvent.getSubCalendarEventsInBlob().Count, 2);
        }
        
        [TestMethod]
        public void file_b0142c4c()
        {
            Location homeLocation = TestUtility.getAdHocLocations()[0];
            string scheduleId = "b0142c4c-696e-4de4-81b9-7d788074a69d";
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            TestSchedule schedule = (TestSchedule)scheduleAndDump.Item1;
            schedule.FindMeSomethingToDo(homeLocation).Wait();
            string timeInString_02_05 = "02/05/2020 12:00:00 PM +00:00";
            DateTimeOffset timeWithinDayWithConflict_02_05 = DateTimeOffset.Parse(timeInString_02_05);
            DayTimeLine dayTimeLineWithConflict_02_05 = schedule.Now.getDayTimeLineByTime(timeWithinDayWithConflict_02_05);
            TimeLine dayTimeLineWithConflict_StartToEnd_02_05 = dayTimeLineWithConflict_02_05.StartToEnd;
            HashSet<SubCalendarEvent> subEventsWithinDayOfCOnflict_02_05 = new HashSet<SubCalendarEvent>(schedule.getAllActiveSubEvents().Where(suEvent => suEvent.StartToEnd.doesTimeLineInterfere(dayTimeLineWithConflict_StartToEnd_02_05)));

            schedule.WriteFullScheduleToOutlook();

            var conflict_02_05 = Utility.getConflictingEvents(subEventsWithinDayOfCOnflict_02_05).OrderByDescending(o => o.End).ToList();
            Assert.AreEqual(conflict_02_05.Count, 0);

        }

        [TestMethod]
        public void file_4ff9a1ea()
        {
            Location homeLocation = TestUtility.getAdHocLocations()[0];
            string scheduleId = "4ff9a1ea-0b03-4d35-9bca-7d029f1bd8bf";
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            TestSchedule schedule = (TestSchedule)scheduleAndDump.Item1;
            schedule.FindMeSomethingToDo(homeLocation).Wait();
            string timeInString = "10/3/2019 12:00:00 PM +00:00";            
            schedule.WriteFullScheduleToOutlook();

            DateTimeOffset timeWithinDayWithConflict = DateTimeOffset.Parse(timeInString);
            DayTimeLine dayTimeLineWithConflict = schedule.Now.getDayTimeLineByTime(timeWithinDayWithConflict);
            TimeLine dayTimeLineWithConflict_StartToEnd = dayTimeLineWithConflict.StartToEnd;
            HashSet<SubCalendarEvent> subEventsWithinDayOfCOnflict = new HashSet<SubCalendarEvent>(schedule.getAllActiveSubEvents().Where(suEvent => suEvent.StartToEnd.doesTimeLineInterfere(dayTimeLineWithConflict_StartToEnd)));

            var conflict = Utility.getConflictingEvents(subEventsWithinDayOfCOnflict);
            Assert.IsTrue(conflict.Count == 1);

            string timeInString_oct_10 = "10/10/2019 12:00:00 PM +00:00";
            DateTimeOffset timeWithinDayWithConflict_oct_10 = DateTimeOffset.Parse(timeInString_oct_10);
            DayTimeLine dayTimeLineWithConflict_oct_10 = schedule.Now.getDayTimeLineByTime(timeWithinDayWithConflict_oct_10);
            TimeLine dayTimeLineWithConflict_StartToEnd_oct_10 = dayTimeLineWithConflict_oct_10.StartToEnd;
            HashSet<SubCalendarEvent> subEventsWithinDayOfCOnflict_oct_10 = new HashSet<SubCalendarEvent>(schedule.getAllActiveSubEvents().Where(suEvent => suEvent.StartToEnd.doesTimeLineInterfere(dayTimeLineWithConflict_StartToEnd_oct_10)));

            var conflict_oct_10 = Utility.getConflictingEvents(subEventsWithinDayOfCOnflict_oct_10);
            Assert.IsTrue(conflict_oct_10.Count == 1);
        }



        [TestMethod]
        public void file_c44f70d1()
        {
            Location homeLocation = TestUtility.getAdHocLocations()[0];
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
            string scheduleId = "c44f70d1-8c65-4c57-8d47-80b9186c2e44";
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            TestSchedule schedule = (TestSchedule)scheduleAndDump.Item1;
            schedule.SetCalendarEventAsNow("3bb2c573-fa4e-425f-bd2b-a99d4dd1d354_7_0_8be8d64f-dbe8-4989-85a6-1e889ea26b69");
            string timeInString = "2/10/2020 12:00:00 PM +00:00";
            DateTimeOffset timeWithinDayWithConflict = DateTimeOffset.Parse(timeInString);
            DayTimeLine dayTimeLineWithConflict = schedule.Now.getDayTimeLineByTime(timeWithinDayWithConflict);
            TimeLine dayTimeLineWithConflict_StartToEnd = dayTimeLineWithConflict.StartToEnd;
            HashSet<SubCalendarEvent> subEventsWithinDayOfCOnflict = new HashSet<SubCalendarEvent>(schedule.getAllActiveSubEvents().Where(suEvent => suEvent.StartToEnd.doesTimeLineInterfere(dayTimeLineWithConflict_StartToEnd)));

            var conflict =  Utility.getConflictingEvents(subEventsWithinDayOfCOnflict);
            Assert.AreEqual(conflict.Count, 1);
        }

        [TestMethod]
        public void file_1deb0ae5()
        {
            Location homeLocation = TestUtility.getAdHocLocations()[0];
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
            string scheduleId = "1deb0ae5-d271-44a3-83ab-9dca3484af9a";
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            TestSchedule schedule = (TestSchedule)scheduleAndDump.Item1;
            var resultOfShuffle = schedule.FindMeSomethingToDo(homeLocation);
            string timeInString = "12/16/2019 12:00:00 PM +00:00";
            DateTimeOffset timeWithinDayWithConflict = DateTimeOffset.Parse(timeInString);
            DayTimeLine dayTimeLineWithConflict = schedule.Now.getDayTimeLineByTime(timeWithinDayWithConflict);
            TimeLine dayTimeLineWithConflict_StartToEnd = dayTimeLineWithConflict.StartToEnd;
            HashSet<SubCalendarEvent> subEventsWithinDayOfCOnflict = new HashSet<SubCalendarEvent>(schedule.getAllActiveSubEvents().Where(suEvent => suEvent.StartToEnd.doesTimeLineInterfere(dayTimeLineWithConflict_StartToEnd)));
            resultOfShuffle.Wait();
            schedule.WriteFullScheduleToOutlook();

            var conflict = Utility.getConflictingEvents(subEventsWithinDayOfCOnflict);
            Assert.AreEqual(conflict.Count, 0);
        }

        /*
         *current utc time is April 18, 2017 10:40:59 pm +/- 5 mins current end of day 10:00pm
        When I shuffle for some reason there is an unnecessary conflict with 7170280_7_0_7170281 and 7156969_7_0_7156970.(Work on test bug ibm steez & Workout) Event though there is enough time in the day to let both time frames.
        */
        [TestMethod]
        public void file_499a0ab4()
        {
            Location homeLocation = TestUtility.getAdHocLocations()[0];
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
            string scheduleId = "499a0ab4-81d7-42df-a476-44fc4348e94b";
            DateTimeOffset refNow = TestUtility.parseAsUTC("04/18/2017 10:41pm ");
            var scheduleAndDump = TestUtility.getSchedule(scheduleId, refNow);
            TestSchedule schedule = (TestSchedule)scheduleAndDump.Item1;
            var resultOfShuffle = schedule.FindMeSomethingToDo(homeLocation);
            resultOfShuffle.Wait();
            schedule.WriteFullScheduleToLog().Wait();
            SubCalendarEvent subEventA = schedule.getSubCalendarEvent("7170280_7_0_7170281");
            SubCalendarEvent subEventB = schedule.getSubCalendarEvent("7156969_7_0_7156970");
            Assert.IsFalse(subEventB.ActiveSlot.doesTimeLineInterfere(subEventA.ActiveSlot));
        }

        /// <summary>
        /// Screenshot in xmldoc shows unnecessary conflicts
        /// You need to run this in UTC TimeZone
        /// </summary>

        [TestMethod]
        public void file_conflict_cd62d710_resolution()
        {
            string scheduleId = "cd62d710-1ba0-4515-8a42-66653934936d";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId);
            Schedule schedule = scheduleAndDump.Item1;
            schedule.FindMeSomethingToDo(currentLocation).Wait();

            List<SubCalendarEvent> evaluatedSubevents = schedule.EvaluatedSubEvents.ToList();
            List<SubCalendarEvent> conflictingSubevents = schedule.ConflictingSubEvents.ToList();

            HashSet<SubCalendarEvent> designatedubevents = new HashSet<SubCalendarEvent>(schedule.Now.getAllDaysForCalc().SelectMany(o => o.getSubEventsInTimeLine()));
            Assert.IsTrue(evaluatedSubevents.Count == designatedubevents.Count);// so far cannot solve last function
            ((TestSchedule)schedule).WriteFullScheduleToOutlook();
        }


        // Current UTC time: 10/23/2017 7:00:34 PM +00:00
        // End of day is : 10:00pm Est
        // For some reason adding a 30 min event with a deadline of 10/24/2017 3:59:00 AM(UTC), triggers unnecessary conflicts.With the events 8615397_7_0_8615398 and 8615271_7_0_8615272(IBM steez, check amqp stability and workout respectively). Not you have chill at work on your schedule from 9a - 6p(est) via google calendar.
        [TestMethod]
        public void file_712dd797()
        {
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
            DateTimeOffset refNow = TestUtility.parseAsUTC("10/23/2017 7:00:34PM");
            DateTimeOffset endOfEvent = TestUtility.parseAsUTC("10/24/2017 3:59:00AM");
            //UserAccount currentUser = TestUtility.getTestUser(userId: "712dd797-8991-4f79-90c1-7b51c744c4bd");

            string scheduleId = "712dd797-8991-4f79-90c1-7b51c744c4bd";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId, refNow);
            Schedule schedule = scheduleAndDump.Item1;
            TilerUser tilerUser = schedule.User;

            TimeLine actualTime = new TimeLine(TestUtility.parseAsUTC("10/21/2017 1:00PM"), TestUtility.parseAsUTC("10/21/2017 10:00PM"));
            Repetition repeating = new Repetition(new TimeLine(startOfDay.AddDays(-5), startOfDay.AddDays(18)), Repetition.Frequency.DAILY, actualTime);
            CalendarEvent repeatingCalEvent = TestUtility.generateCalendarEvent(tilerUser, actualTime.TimelineSpan, repeating, actualTime.Start, actualTime.End, rigidFlags: true);// simulation of google cal event
            schedule.AddToSchedule(repeatingCalEvent);
            CalendarEvent calEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromMinutes(30), new Repetition(), refNow, endOfEvent);
            schedule.AddToSchedule(calEvent);
            SubCalendarEvent subEventA = schedule.getSubCalendarEvent("8615397_7_0_8615398");
            SubCalendarEvent subEventB = schedule.getSubCalendarEvent("8615271_7_0_8615272");
            Assert.IsFalse(subEventB.ActiveSlot.doesTimeLineInterfere(subEventA.ActiveSlot));
        }

        // Current UTC time: 11/9/2017 5:28am utc
        // End of day is : 10:00pm Est
        // No matter how much I increase the number of splits count on this calendar event it never goes above 3
        [TestMethod]
        public void file_a56a5ac5()
        {
            string subEventId = "6418072_7_0_6418075";
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
            DateTimeOffset refNow = TestUtility.parseAsUTC("11/9/2017 5:28am");
            string scheduleId = "a56a5ac5-b474-4d4e-b878-bbb593a0d5b1";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId, refNow);
            Schedule schedule = scheduleAndDump.Item1;
            TilerUser tilerUser = schedule.User;

            CalendarEvent readjustCalendarEvent = schedule.getCalendarEvent("6418072_7_0_6418075");
            const int updatedSplitCount = 11;
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> updateResult = schedule.BundleChangeUpdate(readjustCalendarEvent.ActiveSubEvents.First().getId,
                readjustCalendarEvent.Name.createCopy(),
                readjustCalendarEvent.ActiveSubEvents.First().Start,
                readjustCalendarEvent.ActiveSubEvents.First().End,
                readjustCalendarEvent.Start,
                readjustCalendarEvent.End,
                updatedSplitCount,
                readjustCalendarEvent.Notes.UserNote);
            CalendarEvent latestCalendarEvent = schedule.getCalendarEvent(subEventId);
            Assert.AreEqual(latestCalendarEvent.NumberOfSplit, updatedSplitCount);
        }

        // Current UTC time: 12/2/2017 8:20pm utc
        // End of day is : 10:00pm Est
        // Trying to update the deadline of 8631313_7_0_8631314 to the time 12/3/2017 4:59pm causes it to crash
        [TestMethod]
        public void file_b10cdae0()
        {
            string subEventId = "8631313_7_0_8631314";
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
            DateTimeOffset refNow = TestUtility.parseAsUTC("12/2/2017 8:20pm");
            DateTimeOffset deadline = TestUtility.parseAsUTC("12/3/2017 4:59pm");

            string scheduleId = "b10cdae0-64e1-498f-82a2-8601da577255";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId, refNow);
            Schedule schedule = scheduleAndDump.Item1;

            CalendarEvent calEvent = schedule.getCalendarEvent(subEventId);
            SubCalendarEvent subEvent = schedule.getSubCalendarEvent(subEventId);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> updateResult = schedule.BundleChangeUpdate(
                subEvent.getId,
                subEvent.getName,
                subEvent.Start,
                subEvent.End,
                subEvent.getCalendarEventRange.Start,
                deadline,
                1,
                subEvent.Notes.UserNote);
            

            Assert.AreEqual(((SubCalendarEventRestricted)schedule.getSubCalendarEvent(subEventId)).getHardCalendarEventRange.End, deadline);
        }

        /// <summary>
        /// For some reason wheneve I hit the shuffle button there is always a conflict generated by the 
        /// sub event 8969308_7_0_8969309, named 'Migrate to rdbms - update/remove write single calendar event to xml' even though
        /// there is enough space after the sub event '9105097_7_0_9105098' named 'Debug why google calendar isn't showing'
        /// </summary>
        [TestMethod]
        public void file_c1fbe98f()
        {
            string currentClearAllId = "4939920_7_0_0";
            string subEventId = "8969308_7_0_8969309";
            string conflictingSubEventId = "9105097_7_0_9105098";
            Location homeLocation = new Location(41.4918975, -81.7727791, " 11900 edgewater dr, lakewood, oh 44107, usa", "home", false, false);
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
            DateTimeOffset refNow = TestUtility.parseAsUTC("12/30/2017 8:39am");

            string scheduleId = "c1fbe98f-cb7e-4b21-9d99-b4c3acaf670c";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId, refNow);
            TestSchedule schedule = scheduleAndDump.Item1 as TestSchedule;

            schedule.FindMeSomethingToDo(homeLocation).Wait();
            
            SubCalendarEvent subEventA = schedule.getSubCalendarEvent(subEventId);
            SubCalendarEvent subEventB = schedule.getSubCalendarEvent(conflictingSubEventId);
            Assert.IsFalse(subEventB.ActiveSlot.doesTimeLineInterfere(subEventA.ActiveSlot));
        }

        /// <summary>
        /// This tries to address a smarter way of resolving conflicts. Currently there are conflicts with the subevents '9107379_7_0_9107390', '9105096_7_0_9105097' and '4939920_7_0_9171782_0' after hitting shuffle.
        /// 9107389_7_0_9107390 => 'patch front end repositioning after data refresh'
        /// 4939920_7_0_9171782 => 'BLOCKED OUT' i.e procrsinate all sub event
        /// 9105096_7_0_9105097 => 'remove the need for complete count and delete count to rely on a data member'
        /// The shuffle could be resolved by moving event with the id 1_7_0_2 named 'Tiler mobile UI-create login page' to the next day since it has a later deadline.
        /// The move will leave sufficient time for of the events to get realocated.
        /// </summary>
        [TestMethod]
        public void file_7e65ea64()
        {
            string currentClearAllId = "4939920";
            string subEventId = "9107389_7_0_9107390";
            string conflictingSubEventId = "4939920_7_0_9171782";
            string subEventCId = "9105096_7_0_9105097";
            Location homeLocation = new Location(41.4918975, -81.7727791, " 11900 edgewater dr, lakewood, oh 44107, usa", "home", false, false);
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("2:00am");
            DateTimeOffset refNow = TestUtility.parseAsUTC("12/30/2017 8:39am");
            
            

            string scheduleId = "c1fbe98f-cb7e-4b21-9d99-b4c3acaf670c";
            Location currentLocation = new TilerElements.Location(39.9255867, -105.145055, "", "", false, false);
            var scheduleAndDump = TestUtility.getSchedule(scheduleId, refNow);
            TestSchedule schedule = scheduleAndDump.Item1 as TestSchedule;
            schedule.User.ClearAllId = currentClearAllId;

            schedule.FindMeSomethingToDo(homeLocation).Wait();

            DayTimeLine firstDay = schedule.Now.firstDay;
            IEnumerable<SubCalendarEvent> firstDaySUbevents = schedule.getAllCalendarEvents().Where(calevent => calevent.isActive).SelectMany(calEvent => calEvent.ActiveSubEvents).Where(subEvent => subEvent.ActiveSlot.doesTimeLineInterfere(firstDay));
            SubCalendarEvent subEventA = schedule.getSubCalendarEvent(subEventId);
            SubCalendarEvent subEventB = schedule.getSubCalendarEvent(conflictingSubEventId);
            SubCalendarEvent subEventC = schedule.getSubCalendarEvent(subEventCId);
            List<SubCalendarEvent> conflictingSubEvents = new List<SubCalendarEvent>() { subEventA, subEventB, subEventC };
            List<BlobSubCalendarEvent> conflictingBlob = Utility.getConflictingEvents(conflictingSubEvents);
            List<BlobSubCalendarEvent> firstDayConflict = Utility.getConflictingEvents(firstDaySUbevents);
            Assert.AreEqual(conflictingBlob.Count, 0);
            Assert.AreEqual(firstDayConflict.Count, 0);

            Assert.IsFalse(subEventB.ActiveSlot.doesTimeLineInterfere(subEventA.ActiveSlot));
        }

        // [TestCleanup]
        // public void eachTestCleanUp()
        // {
        //     UserAccount currentUser = TestUtility.getTestUser();
        //     currentUser.Login().Wait();
        //     DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //     Schedule Schedule = new TestSchedule(currentUser, refNow);
        //     currentUser.DeleteAllCalendarEvents();
        // }
    }
}
