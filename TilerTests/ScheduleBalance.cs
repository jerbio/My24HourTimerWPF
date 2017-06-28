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
    /// <summary>
    /// This function tries to create a schedule where path optimizatio would try to create an unnecesary conflict when there is sufficient space for all events involved
    /// </summary>
    [TestClass]
    public class ScheduleBalance
    {
        [ClassInitialize]
        public static void classInitialize(TestContext testContext)
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            Schedule schedule = new TestSchedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }

        /// <summary>
        /// Test creates a combination of rigid and non rigid evvents that the sum of their duration adds up to eight hours. 
        /// Test creates a rigid event and then tries to add the other non-rigid events. The none rigids have a timeline that starts at the smetime as the rigid, but ends eight hours after
        /// The non rigids hould be aable to fit, without a conflict. 
        /// You need to ensure the end of day is appropriately initialized to avoid weird computation error
        /// </summary>
        [TestMethod]
        public void TightScheduleNoUnnecessaryConflict()
        {
            Location homeLocation = new Location("2895 Van aken Blvd cleveland OH 44120");
            Location workLocation = new Location(41.5002762, -81.6839155, "1228 euclid Ave cleveland OH","Work",false,false);
            Location gymLocation = new Location(41.4987461 , -81.6884993, "619 Prospect Avenue Cleveland, OH 44115", "Gym", false, false);
            Location churchLocation = new Location(41.569467, -81.539422, "1465 Dille Rd, Cleveland, OH 44117", "Church", false, false);
            Location shakerLibrary = new Location(41.4658937, -81.5664832, "16500 Van Aken Blvd, Shaker Heights, OH 44120", "Shake Library", false, false);

            List<Location> locations = new List<Location>() { homeLocation, workLocation, gymLocation, churchLocation };
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, refNow.Day, 8, 0, 0, new TimeSpan());
            TestSchedule schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
            TimeLine encompassingTimeline = new TimeLine(refNow, refNow.AddHours(8));


            int rigidHoursSpan = 4;
            CalendarEvent testHomeRigidCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(rigidHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.Start.AddHours(rigidHoursSpan), 1, true, homeLocation);
            int nonRigidHoursSpan = 1;
            CalendarEvent testHomeNonRigidCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(nonRigidHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, homeLocation);
            CalendarEvent testWorkNonRigidCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(nonRigidHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, workLocation);
            int nonRigidTwoHoursSpan = 2;
            CalendarEvent testGymNonRigidCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(nonRigidTwoHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, gymLocation);

            schedule.AddToScheduleAndCommit(testHomeRigidCalEvent).Wait();
            IEnumerable<SubCalendarEvent> allSubEvents = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents);
            List<BlobSubCalendarEvent> conflicts = Utility.getConflictingEvents(allSubEvents);
            Assert.AreEqual(0, conflicts.Count);

            schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
            schedule.AddToScheduleAndCommit(testHomeNonRigidCalEvent).Wait();
            allSubEvents = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents);
            conflicts = Utility.getConflictingEvents(allSubEvents);
            Assert.AreEqual(0, conflicts.Count);

            schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
            schedule.AddToScheduleAndCommit(testWorkNonRigidCalEvent).Wait();
            allSubEvents = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents);
            conflicts = Utility.getConflictingEvents(allSubEvents);
            Assert.AreEqual(0, conflicts.Count);

            schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
            schedule.AddToSchedule(testGymNonRigidCalEvent);
            allSubEvents = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents);
            conflicts = Utility.getConflictingEvents(allSubEvents);
            Assert.AreEqual(0, conflicts.Count);
        }


        /// <summary>
        /// Current UTC time is 12:15 AM, Friday, June 2, 2017
        /// End of day is 10:00pm
        /// There is a conflict between the subevents 6413191_7_0_6413193 "Path optimization - implement optimization about beginning from home" and 6417040_7_0_6926266 "Event name analysis". 
        /// I tried shuffling and this doesnt resolve the issue. Even though event name analysis can be scheduled for a later date.
        /// Also WTF is 6418068_7_0_6909066('Spin up alternate tiler server for dbchanges') still doing there.It's deadline is sometime on the 16th
        /// </summary>
        [TestMethod]
        public void conflictResolution0()
        {
            Location homeLocation = TestUtility.getLocations()[0];
            DateTimeOffset startOfDay = DateTimeOffset.Parse("2:00am");
            UserAccount currentUser = TestUtility.getTestUser(userId: "982935bc-f5bc-4d5e-a372-7a5d5e40cfa0");
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Parse("06/02/2017 12:15am");
            TestSchedule schedule = new TestSchedule(currentUser, refNow, startOfDay);
            var resultOfShuffle = schedule.FindMeSomethingToDo(homeLocation);
            resultOfShuffle.Wait();
            schedule.WriteFullScheduleToLogAndOutlook().Wait();
            TimeLine timeLine = new TimeLine(refNow.AddDays(0), refNow.AddDays(7));
            List<SubCalendarEvent>subEvents = schedule.getAllCalendarEvents().Where(calEvent=> calEvent.isActive).SelectMany(calEvent => calEvent.ActiveSubEvents).Where(subEvent => subEvent.ActiveSlot.doesTimeLineInterfere(timeLine)).ToList();
            List<BlobSubCalendarEvent> conflictingSubEvents = Utility.getConflictingEvents(subEvents);
            Assert.AreEqual(conflictingSubEvents.Count, 0);
        }

        /// <summary>
        /// Test creates a combination of rigid and non rigid evvents that the sum of their duration adds up to eight hours. 
        /// Test creates a rigid event and then tries to add the other non-rigid events. The none rigids have a timeline that starts at the smetime as the rigid, but ends eight hours after
        /// The non rigids hould be aable to fit, without a conflict. The non rigids have a span of at least 1 hour
        /// The ed of day is created in suc a way that it is 30 mins after the end time of the rigid event.
        /// This creates a scenario where none of the non-rigids can fit between the rigid and the end of the day.
        /// Ideally the code should still fit all events without a conflict, however because nothing can fit between the end of the day and the new event this could cause a problem
        /// </summary>
        [TestMethod]
        public void TightSchedule_SufficienSpanOverall_InsufficientEachDay()
        {
            Location homeLocation = new Location("2895 Van aken Blvd cleveland OH 44120");
            Location workLocation = new Location(41.5002762, -81.6839155, "1228 euclid Ave cleveland OH", "Work", false, false);
            Location gymLocation = new Location(41.4987461, -81.6884993, "619 Prospect Avenue Cleveland, OH 44115", "Gym", false, false);
            Location churchLocation = new Location(41.569467, -81.539422, "1465 Dille Rd, Cleveland, OH 44117", "Church", false, false);
            Location shakerLibrary = new Location(41.4658937, -81.5664832, "16500 Van Aken Blvd, Shaker Heights, OH 44120", "Shake Library", false, false);

            List<Location> locations = new List<Location>() { homeLocation, workLocation, gymLocation, churchLocation };
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, refNow.Day, 8, 0, 0, new TimeSpan());
            TestSchedule schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
            TimeLine encompassingTimeline = new TimeLine(refNow, refNow.AddHours(8));

            
            int rigidHoursSpan = 4;
            DateTimeOffset endOfRigid = encompassingTimeline.Start.AddHours(rigidHoursSpan);
            DateTimeOffset startOfDay = endOfRigid.AddHours(.5);
            CalendarEvent testHomeRigidCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(rigidHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, endOfRigid, 1, true, homeLocation);
            int nonRigidHoursSpan = 1;
            CalendarEvent testHomeNonRigidCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(nonRigidHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, homeLocation);
            CalendarEvent testWorkNonRigidCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(nonRigidHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, workLocation);
            int nonRigidTwoHoursSpan = 2;
            CalendarEvent testGymNonRigidCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(nonRigidTwoHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, gymLocation);

            schedule.AddToScheduleAndCommit(testHomeRigidCalEvent).Wait();
            IEnumerable<SubCalendarEvent> allSubEvents = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents);
            List<BlobSubCalendarEvent> conflicts = Utility.getConflictingEvents(allSubEvents);
            Assert.AreEqual(0, conflicts.Count);

            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(testHomeNonRigidCalEvent).Wait();
            allSubEvents = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents);
            conflicts = Utility.getConflictingEvents(allSubEvents);
            Assert.AreEqual(0, conflicts.Count);

            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(testWorkNonRigidCalEvent).Wait();
            allSubEvents = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents);
            conflicts = Utility.getConflictingEvents(allSubEvents);
            Assert.AreEqual(0, conflicts.Count);

            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.AddToSchedule(testGymNonRigidCalEvent);
            allSubEvents = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents);
            conflicts = Utility.getConflictingEvents(allSubEvents);
            Assert.AreEqual(0, conflicts.Count);/// This is known to fail
        }


        /// <summary>
        /// This test creates one rigid event and a one non rigid event. The non-rigid has 6 splits.
        /// The test checks to see if half the non-rigids are scheduled before the rigid and the other half after.
        /// This should be the default schedule pattern events of the same calendar event should be scheduled as far apart as possible
        /// A shuffle is called to ensure a desired order isnt preffered
        /// </summary>
        [TestMethod]
        public void scheduleAroundRigidEvents()
        {
            List<Location> locations = TestUtility.getLocations();
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Parse("12:00AM");
            DateTimeOffset start = DateTimeOffset.Parse("2:00PM");
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset end = start.Add(duration);
            CalendarEvent hugeRigid = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, true, locations[0]);
            TestSchedule Schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
            Schedule.AddToScheduleAndCommit(hugeRigid).Wait();
            CalendarEvent randomSubEvents = TestUtility.generateCalendarEvent(TimeSpan.FromHours(6), new Repetition(), refNow, refNow.AddDays(1), 6, false, locations[1]);
            Schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
            Schedule.AddToScheduleAndCommit(randomSubEvents).Wait();
            List<SubCalendarEvent> allSubEvents = Schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents).OrderBy(meSubEvent => meSubEvent.Start).ToList();
            Schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
            var resultOfShuffle = Schedule.FindMeSomethingToDo(new Location());
            resultOfShuffle.Wait();
            Schedule.WriteFullScheduleToLogAndOutlook().Wait();
            Schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
            SubCalendarEvent subEvent = allSubEvents.Single(meSubEvent => meSubEvent.getId == hugeRigid.AllSubEvents.First().getId);
            int calidIndex = allSubEvents.Count / 2;
            int index = allSubEvents.IndexOf(subEvent);
            Assert.AreEqual(index, 3);/// This is known to fail
        }

        [TestInitialize]
        public void cleanUpLog()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            Schedule Schedule = new TestSchedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
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
