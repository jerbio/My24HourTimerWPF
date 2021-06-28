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
        //[ClassInitialize]
        //public static void classInitialize(TestContext testContext)
        //{
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    Schedule schedule = new TestSchedule(currentUser, refNow);
        //    currentUser.DeleteAllCalendarEvents();
        //}

        /// <summary>
        /// Test creates a combination of rigid and non rigid evvents that the sum of their duration adds up to eight hours. 
        /// Test creates a rigid event and then tries to add the other non-rigid events. The none rigids have a timeline that starts at the smetime as the rigid, but ends eight hours after
        /// The non rigids hould be aable to fit, without a conflict. 
        /// You need to ensure the end of day is appropriately initialized to avoid weird computation error
        /// </summary>
        [TestMethod]
        public void TightScheduleNoUnnecessaryConflict()
        {
            var packet = TestUtility.CreatePacket();
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;

            Location homeLocation = new Location("2895 Van aken Blvd cleveland OH 44120");
            Location workLocation = new Location(41.5002762, -81.6839155, "1228 euclid Ave cleveland OH","Work",false,false);
            Location gymLocation = new Location(41.4987461 , -81.6884993, "619 Prospect Avenue Cleveland, OH 44115", "Gym", false, false);
            Location churchLocation = new Location(41.569467, -81.539422, "1465 Dille Rd, Cleveland, OH 44117", "Church", false, false);
            Location shakerLibrary = new Location(41.4658937, -81.5664832, "16500 Van Aken Blvd, Shaker Heights, OH 44120", "Shake Library", false, false);

            List<Location> locations = new List<Location>() { homeLocation, workLocation, gymLocation, churchLocation };
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, refNow.Day, 8, 0, 0, new TimeSpan());
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule schedule = new TestSchedule(user, refNow, refNow.AddDays(5));
            TimeLine encompassingTimeline = new TimeLine(refNow, refNow.AddHours(8));


            int rigidHoursSpan = 4;
            CalendarEvent testHomeRigidCalEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(rigidHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.Start.AddHours(rigidHoursSpan), 1, true, homeLocation);
            schedule.AddToScheduleAndCommit(testHomeRigidCalEvent);
            schedule.WriteFullScheduleToOutlook();
            IEnumerable<SubCalendarEvent> allSubEvents = schedule.getAllActiveSubEvents();
            List<BlobSubCalendarEvent> conflicts = Utility.getConflictingEvents(allSubEvents).Item1;
            Assert.AreEqual(0, conflicts.Count);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            homeLocation = TestUtility.getLocation(user, tilerUser, homeLocation.Id);
            schedule = new TestSchedule(user, refNow, refNow.AddDays(5));
            int nonRigidHoursSpan = 1;
            CalendarEvent testHomeNonRigidCalEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(nonRigidHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, null);
            testHomeNonRigidCalEvent.LocationId = homeLocation.Id;
            testHomeNonRigidCalEvent.AllSubEvents.AsParallel().ForAll((eachSubEvent) => eachSubEvent.LocationId = homeLocation.Id);
            schedule.AddToScheduleAndCommit(testHomeNonRigidCalEvent);
            allSubEvents = schedule.getAllActiveSubEvents();
            conflicts = Utility.getConflictingEvents(allSubEvents).Item1;
            Assert.AreEqual(0, conflicts.Count);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow, refNow.AddDays(5));
            CalendarEvent testWorkNonRigidCalEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(nonRigidHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, workLocation);
            schedule.AddToScheduleAndCommit(testWorkNonRigidCalEvent);
            allSubEvents = schedule.getAllActiveSubEvents();
            conflicts = Utility.getConflictingEvents(allSubEvents).Item1;
            Assert.AreEqual(0, conflicts.Count);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow, refNow.AddDays(5));
            int nonRigidTwoHoursSpan = 2;
            CalendarEvent testGymNonRigidCalEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(nonRigidTwoHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, gymLocation);
            schedule.AddToSchedule(testGymNonRigidCalEvent);
            allSubEvents = schedule.getAllActiveSubEvents();
            conflicts = Utility.getConflictingEvents(allSubEvents).Item1;
            Assert.AreEqual(0, conflicts.Count);
        }


        /// <summary>
        /// Test tries to ensure there aren't unusual concentration of tiles. 
        /// It ensures that tiles with multiple counts are scheduled to be in time frames if you split the timeline between the counts.
        /// So for example if a tile is five times a month, a tile should be scheduled for every fifth timeline of a month as opposed to scheduling everything at the end of the month.
        /// </summary>
        [TestMethod]
        public void TileScheduleAsTwiceAMonthShouldHaveTileInSeparateTwoWeeks()
        {
            var packet = TestUtility.CreatePacket();
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;

            Location homeLocation = new Location("2895 Van aken Blvd cleveland OH 44120");
            Location workLocation = new Location(41.5002762, -81.6839155, "1228 euclid Ave cleveland OH", "Work", false, false);
            Location gymLocation = new Location(41.4987461, -81.6884993, "619 Prospect Avenue Cleveland, OH 44115", "Gym", false, false);
            Location churchLocation = new Location(41.569467, -81.539422, "1465 Dille Rd, Cleveland, OH 44117", "Church", false, false);
            Location shakerLibrary = new Location(41.4658937, -81.5664832, "16500 Van Aken Blvd, Shaker Heights, OH 44120", "Shake Library", false, false);

            List<Location> locations = new List<Location>() { homeLocation, workLocation, gymLocation, churchLocation };
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = DateTimeOffset.Parse("6/28/2021 8:00:00 AM +00:00");
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, refNow.Day, 8, 0, 0, new TimeSpan());
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            
            TestSchedule schedule = new TestSchedule(user, refNow);
            TimeLine encompassingTimeline = new TimeLine(refNow, refNow.AddMonths(3));
            TimeSpan calEventSpan = TimeSpan.FromHours(10);
            Repetition repetition = new Repetition(encompassingTimeline, Repetition.Frequency.WEEKLY, new TimeLine(encompassingTimeline.Start, encompassingTimeline.Start.AddDays(1)));

            CalendarEvent twoHourEachTIleFor_3Months= TestUtility.generateCalendarEvent(tilerUser, calEventSpan, repetition, encompassingTimeline.Start, encompassingTimeline.End, 5, false);
            schedule.AddToScheduleAndCommit(twoHourEachTIleFor_3Months);


            Repetition oneHourRepetition = new Repetition(encompassingTimeline, Repetition.Frequency.WEEKLY, new TimeLine(encompassingTimeline.Start, encompassingTimeline.Start.AddDays(1)));
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            CalendarEvent oneHourEachTIleFor_3Months = TestUtility.generateCalendarEvent(tilerUser, calEventSpan, oneHourRepetition, encompassingTimeline.Start, encompassingTimeline.End, 10, false);
            schedule.AddToScheduleAndCommit(oneHourEachTIleFor_3Months);


            TimeSpan threeHourSpan = TimeSpan.FromHours(30);
            Repetition threeHourRepetition = new Repetition(encompassingTimeline, Repetition.Frequency.WEEKLY, new TimeLine(encompassingTimeline.Start, encompassingTimeline.Start.AddDays(1)));
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow);
            CalendarEvent threeHourEachTIleFor_3Months = TestUtility.generateCalendarEvent(tilerUser, threeHourSpan, threeHourRepetition, encompassingTimeline.Start, encompassingTimeline.End, 10, false);
            schedule.AddToScheduleAndCommit(threeHourEachTIleFor_3Months);


            TimeSpan fourHourSpan = TimeSpan.FromHours(40);
            Repetition fourHourRepetition = new Repetition(encompassingTimeline, Repetition.Frequency.WEEKLY, new TimeLine(encompassingTimeline.Start, encompassingTimeline.Start.AddDays(1)));
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset startOfDay= TestUtility.parseAsUTC("06/02/2017 7:00am");
            schedule = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent fourHourEachTIleFor_3Months = TestUtility.generateCalendarEvent(tilerUser, fourHourSpan, fourHourRepetition, encompassingTimeline.Start, encompassingTimeline.End, 10, false);
            schedule.AddToScheduleAndCommit(fourHourEachTIleFor_3Months);


            Repetition fourHourRepetitionCopy = new Repetition(encompassingTimeline, Repetition.Frequency.WEEKLY, new TimeLine(encompassingTimeline.Start, encompassingTimeline.Start.AddDays(1)));
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent fourHourEachTIleFor_3MonthsCopy = TestUtility.generateCalendarEvent(tilerUser, fourHourSpan, fourHourRepetitionCopy, encompassingTimeline.Start, encompassingTimeline.End, 10, false);
            Utility.debugString = "activeBug";
            schedule.AddToScheduleAndCommit(fourHourEachTIleFor_3MonthsCopy);


            TimeSpan FailingTile = TimeSpan.FromHours(1);
            Repetition twoHour_PerTwoWeeksRepetition = new Repetition(encompassingTimeline, Repetition.Frequency.MONTHLY, new TimeLine(encompassingTimeline.Start, encompassingTimeline.Start.AddDays(1)));
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow, startOfDay);
            EventName tileName = new EventName(null, null, "Single Tile Every two weeks");
            CalendarEvent thirtyMin_PerTwoWeeks = TestUtility.generateCalendarEvent(tilerUser, FailingTile, twoHour_PerTwoWeeksRepetition, encompassingTimeline.Start, encompassingTimeline.End, 2, false, tileName: tileName);
            schedule.AddToScheduleAndCommit(thirtyMin_PerTwoWeeks);


            TimeSpan fiveDays = TimeSpan.FromDays(5);
            foreach(CalendarEvent eachCalEvent in schedule.getAllRelatedCalendarEvents( thirtyMin_PerTwoWeeks.Id).Where(calEvent => calEvent.IsRepeatsChildCalEvent))
            {
                List<SubCalendarEvent> subEvents = eachCalEvent.AllSubEvents.OrderBy(o => o.Start).ToList();
                TimeSpan subEventSpacing = subEvents[1].End - subEvents[0].End;
                Assert.IsTrue(subEventSpacing >= fiveDays);
            }

            // We want to complete all tiles from currentNow till the beginning of timeLineLimitForCompletion(not right now we're going with arbitrar number of days)
            // and want to see if there is a repriotization of the monthly tile. This is because with the completion of tiles it means the 'not compeleted' latest added tile should get higher priority and should lead to better spacing
            DateTimeOffset timeLineLimitForCompletion = thirtyMin_PerTwoWeeks.Start.AddDays(8);
            TimeLine timeline = new TimeLine(schedule.Now.constNow, timeLineLimitForCompletion);
            List<SubCalendarEvent> toBeCompletedSubEvents =  schedule
                .getAllActiveSubEvents()
                .Where(subEvent => 
                    timeline.doesTimeLineInterfere(subEvent) && 
                    !subEvent.Id.Contains(thirtyMin_PerTwoWeeks.Calendar_EventID.getCalendarEventComponent()))
                .ToList();



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow, timeline.End);
            schedule.markSubEventsAsComplete(toBeCompletedSubEvents.Select(o => o.Id)).Wait();
            schedule.persistToDB().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, timeline.End);
            schedule.disableConflictResolution();
            Utility.debugString = thirtyMin_PerTwoWeeks.RecurringCalendarEvents.First().Calendar_EventID.getRepeatCalendarEventComponent();
            schedule.FindMeSomethingToDo(homeLocation).Wait();

            foreach (CalendarEvent eachCalEvent in schedule.getAllRelatedCalendarEvents(thirtyMin_PerTwoWeeks.Id).Where(calEvent => calEvent.IsRepeatsChildCalEvent))
            {
                List<SubCalendarEvent> subEvents = eachCalEvent.AllSubEvents.OrderBy(o => o.Start).ToList();
                TimeSpan subEventSpacing = subEvents[1].End - subEvents[0].End;
                Assert.IsTrue(subEventSpacing >= TimeSpan.FromDays(1));
            }


            schedule.WriteFullScheduleToOutlook();

        }


        /// Current UTC time is 12:15 AM, Friday, June 2, 2017
        /// End of day is 10:00pm
        /// There is a conflict between the subevents 6413191_7_0_6413193 "Path optimization - implement optimization about beginning from home" and 6417040_7_0_6926266 "Event name analysis". 
        /// I tried shuffling and this doesnt resolve the issue. Even though event name analysis can be scheduled for a later date.
        /// Also WTF is 6418068_7_0_6909066('Spin up alternate tiler server for dbchanges') still doing there.It's deadline is sometime on the 16th
        /// </summary>
        [TestMethod]
        public void conflictResolution0()
        {
            string scheduleId = "982935bc-f5bc-4d5e-a372-7a5d5e40cfa0";
            DateTimeOffset refNow = TestUtility.parseAsUTC("06/02/2017 12:15am");
            var scheduleAndDump = TestUtility.getSchedule(scheduleId, refNow);
            Schedule schedule = scheduleAndDump.Item1;
            Location homeLocation = TestUtility.getAdHocLocations(schedule.User.Id)[0];
            var resultOfShuffle = schedule.FindMeSomethingToDo(homeLocation);
            resultOfShuffle.Wait();
            //schedule.WriteFullScheduleToLog().Wait();
            TimeLine timeLine = new TimeLine(refNow.AddDays(0), refNow.AddDays(7));
            List<SubCalendarEvent> subEvents = schedule.getAllCalendarEvents().Where(calEvent => calEvent.isActive).SelectMany(calEvent => calEvent.ActiveSubEvents).Where(subEvent => subEvent.ActiveSlot.doesTimeLineInterfere(timeLine)).ToList();
            List<BlobSubCalendarEvent> conflictingSubEvents = Utility.getConflictingEvents(subEvents).Item1;
            Assert.AreEqual(conflictingSubEvents.Count, 0);
        }

        /// <summary>
        /// Test creates a combination of rigid and non rigid evvents that the sum of their duration adds up to eight hours. 
        /// Test creates a rigid event and then tries to add the other non-rigid events. The none rigids have a timeline that starts at the smetime as the rigid, but ends eight hours after
        /// The non rigids should be aable to fit, without a conflict. The non rigids have a span of at least 1 hour
        /// The end of day is created in suc a way that it is 30 mins after the end time of the rigid event.
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
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount currentUser = TestUtility.getTestUser(userId: tilerUser.Id);
            currentUser.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("2:00am");
            DateTimeOffset now = refNow = DateTimeOffset.UtcNow;
            refNow = new DateTimeOffset(now.Year, now.Month, now.Day, 8, 0, 0, new TimeSpan());
            
            TestSchedule schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
            TimeLine encompassingTimeline = new TimeLine(refNow, refNow.AddHours(8));


            int rigidHoursSpan = 4;
            DateTimeOffset endOfRigid = encompassingTimeline.Start.AddHours(rigidHoursSpan);
            DateTimeOffset startOfDay = endOfRigid.AddHours(.5);
            CalendarEvent testHomeRigidCalEvent = TestUtility.generateCalendarEvent(schedule.User, TimeSpan.FromHours(rigidHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, endOfRigid, 1, true, homeLocation);
            int nonRigidHoursSpan = 1;
            CalendarEvent testHomeNonRigidCalEvent = TestUtility.generateCalendarEvent(schedule.User, TimeSpan.FromHours(nonRigidHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, homeLocation);
            CalendarEvent testWorkNonRigidCalEvent = TestUtility.generateCalendarEvent(schedule.User, TimeSpan.FromHours(nonRigidHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, workLocation);
            int nonRigidTwoHoursSpan = 2;
            CalendarEvent testGymNonRigidCalEvent = TestUtility.generateCalendarEvent(schedule.User, TimeSpan.FromHours(nonRigidTwoHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, gymLocation);

            schedule.AddToScheduleAndCommit(testHomeRigidCalEvent);
            IEnumerable<SubCalendarEvent> allSubEvents = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents);
            List<BlobSubCalendarEvent> conflicts = Utility.getConflictingEvents(allSubEvents).Item1;
            Assert.AreEqual(0, conflicts.Count);

            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(testHomeNonRigidCalEvent);
            allSubEvents = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents);
            conflicts = Utility.getConflictingEvents(allSubEvents).Item1;
            Assert.AreEqual(0, conflicts.Count);

            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(testWorkNonRigidCalEvent);
            allSubEvents = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents);
            conflicts = Utility.getConflictingEvents(allSubEvents).Item1;
            Assert.AreEqual(0, conflicts.Count);

            schedule = new TestSchedule(currentUser, refNow, startOfDay);
            //schedule.disableConflictResolution();
            schedule.AddToSchedule(testGymNonRigidCalEvent);
            allSubEvents = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents);
            schedule.WriteFullScheduleToOutlook();
            conflicts = Utility.getConflictingEvents(allSubEvents).Item1;
            Assert.AreEqual(0, conflicts.Count);
        }


        ///// <summary>
        ///// This test creates one rigid event and a one non rigid event. The non-rigid has 6 splits.
        ///// The test checks to see if half the non-rigids are scheduled before the rigid and the other half after.
        ///// This should be the default schedule pattern events of the same calendar event should be scheduled as far apart as possible
        ///// A shuffle is called to ensure a desired order isnt preffered
        ///// </summary>
        //[TestMethod]
        //public void scheduleAroundRigidEvents()
        //{
        //    List<Location> locations = TestUtility.getLocations();
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM");
        //    DateTimeOffset start = TestUtility.parseAsUTC("2:00PM");
        //    TimeSpan duration = TimeSpan.FromHours(4);
        //    DateTimeOffset end = start.Add(duration);
        //    CalendarEvent hugeRigid = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, true, locations[0]);
        //    TestSchedule Schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
        //    Schedule.AddToScheduleAndCommit(hugeRigid).Wait();
        //    CalendarEvent randomSubEvents = TestUtility.generateCalendarEvent(TimeSpan.FromHours(6), new Repetition(), refNow, refNow.AddDays(1), 6, false, locations[1]);
        //    Schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
        //    Schedule.AddToScheduleAndCommit(randomSubEvents).Wait();
        //    List<SubCalendarEvent> allSubEvents = Schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents).OrderBy(meSubEvent => meSubEvent.Start).ToList();
        //    Schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
        //    var resultOfShuffle = Schedule.FindMeSomethingToDo(new Location());
        //    resultOfShuffle.Wait();
        //    Schedule.WriteFullScheduleToLogAndOutlook().Wait();
        //    Schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
        //    SubCalendarEvent subEvent = allSubEvents.Single(meSubEvent => meSubEvent.getId == hugeRigid.AllSubEvents.First().getId);
        //    int calidIndex = allSubEvents.Count / 2;
        //    int index = allSubEvents.IndexOf(subEvent);
        //    Assert.AreEqual(index, 3);/// This is known to fail
        //}

        /// <summary>
        /// Test events should always start or end from home or at least the location closest to the 'current location' passed to the current location when shuffle is clicked
        /// </summary>
        [TestMethod]
        public void testInitialRandomEventShouldConsiderLocationAsSourceOfAction()
        {
            Dictionary<String, Location> locationsDict = TestUtility.getAdHocLocations().ToDictionary(obj => obj.Description, obj => obj);
            List<Location> locations = locationsDict.Values.ToList();
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount currentUser = TestUtility.getTestUser(userId: tilerUser.Id);
            currentUser.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            DateTimeOffset start = TestUtility.parseAsUTC("2:00AM 12/2/2017");
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset end = start.Add(duration);


            TestSchedule schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
            CalendarEvent hugeRigid = TestUtility.generateCalendarEvent(schedule.User, duration, new Repetition(), start, end, 1, true, locations[0]);
            

            CalendarEvent shaker0 = TestUtility.generateCalendarEvent(schedule.User, TimeSpan.FromHours(1), new Repetition(), refNow, refNow.AddDays(1), 2, false, locationsDict["Shaker Library"]);
            schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5), EventID.LatestID);
            schedule.AddToScheduleAndCommit(shaker0);
            CalendarEvent gym0 = TestUtility.generateCalendarEvent(schedule.User, TimeSpan.FromHours(1), new Repetition(), refNow, refNow.AddDays(1), 1, false, locationsDict["Gym"]);
            schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5), EventID.LatestID);
            schedule.AddToScheduleAndCommit(gym0);
            CalendarEvent gym1 = TestUtility.generateCalendarEvent(schedule.User, TimeSpan.FromHours(1), new Repetition(), refNow, refNow.AddDays(1), 1, false, locationsDict["Gym"]);
            schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5), EventID.LatestID);
            schedule.AddToScheduleAndCommit(gym1);
            CalendarEvent work0 = TestUtility.generateCalendarEvent(schedule.User, TimeSpan.FromHours(2), new Repetition(), refNow, refNow.AddDays(1), 2, false, locationsDict["Work"]);
            schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5), EventID.LatestID);
            schedule.AddToScheduleAndCommit(work0);
            Location work = locationsDict["Work"];

            schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5), EventID.LatestID);
            schedule.FindMeSomethingToDo(work).Wait();
            
            schedule.WriteFullScheduleToOutlook();
            List<SubCalendarEvent> subevents = schedule.getAllCalendarEvents().SelectMany(subevent => subevent.AllSubEvents).OrderBy(subevent => subevent.Start).ToList();
            Assert.AreEqual(subevents.First().Location.Description, work.Description);

        }

        ///// <summary>
        ///// Test tries to ensure that the edge events for a day(beginning and end of day) should be events closest to the home provided all other schedule altering variables stay constant
        ///// </summary>
        //[TestMethod]
        //public void testHomeAsEdgeOfDayOfAction()
        //{
        //    Dictionary<String, Location> locationsDict = TestUtility.getLocations().ToDictionary(obj => obj.Description, obj => obj);
        //    List<Location> locations = locationsDict.Values.ToList();
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
        //    DateTimeOffset start = TestUtility.parseAsUTC("2:00AM 12/2/2017");
        //    TimeSpan duration = TimeSpan.FromHours(4);
        //    DateTimeOffset end = start.Add(duration);
        //    CalendarEvent hugeRigid = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, true, locations[0]);
        //    TestSchedule schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));

        //    CalendarEvent shaker0 = TestUtility.generateCalendarEvent(TimeSpan.FromHours(1), new Repetition(), refNow, refNow.AddDays(1), 2, false, locationsDict["Shaker Library"]);
        //    schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5), EventID.LatestID);
        //    schedule.AddToScheduleAndCommit(shaker0, true).Wait();
        //    CalendarEvent gym1 = TestUtility.generateCalendarEvent(TimeSpan.FromHours(1), new Repetition(), refNow, refNow.AddDays(1), 1, false, locationsDict["Gym"]);
        //    schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5), EventID.LatestID);
        //    schedule.AddToScheduleAndCommit(gym1, true).Wait();
        //    CalendarEvent home0 = TestUtility.generateCalendarEvent(TimeSpan.FromHours(1), new Repetition(), refNow, refNow.AddDays(1), 1, false, locationsDict["Home"]);
        //    schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5), EventID.LatestID);
        //    schedule.AddToScheduleAndCommit(home0, true).Wait();
        //    CalendarEvent home1 = TestUtility.generateCalendarEvent(TimeSpan.FromHours(1), new Repetition(), refNow, refNow.AddDays(1), 1, false, locationsDict["Home"]);
        //    schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5), EventID.LatestID);
        //    schedule.AddToScheduleAndCommit(home1, true).Wait();
        //    CalendarEvent work0 = TestUtility.generateCalendarEvent(TimeSpan.FromHours(2), new Repetition(), refNow, refNow.AddDays(1), 1, false, locationsDict["Work"]);
        //    schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5), EventID.LatestID);
        //    schedule.AddToScheduleAndCommit(work0, true).Wait();
        //    Location Home = locationsDict["Home"];

        //    schedule.WriteFullScheduleToLogAndOutlook().Wait();
        //    List<SubCalendarEvent> subevents = schedule.getAllCalendarEvents().SelectMany(subevent => subevent.AllSubEvents).OrderBy(subevent => subevent.Start).ToList();
        //    bool isAtEdge = false;
        //    if(subevents.Last().Location.Description == Home.Description || subevents.First().Location.Description == Home.Description)
        //    {
        //        isAtEdge = true;
        //    }
        //    Assert.IsTrue(isAtEdge);

        //}

        ///// <summary>
        ///// Function test a scenario where there is a change of no consequence is applied to a users schedule. This modification should not necessarily trigger a reordering of events.
        ///// In this case a rigid calendar event is extended by 5 minutes, amongst non-rigids. This should not result in in the order of things. There might be a change in start times
        ///// </summary>
        //[TestMethod]
        //public void noConsequenceChange ()
        //{
        //    List<Location> locations = TestUtility.getLocations();
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = TestUtility.parseAsUTC("11/6/2017 12:00AM");
        //    DateTimeOffset start = TestUtility.parseAsUTC("11/6/2017 2:00PM");
        //    TimeSpan duration = TimeSpan.FromHours(4);
        //    TimeSpan rigidDuration = TimeSpan.FromHours(1);
        //    DateTimeOffset end = start.Add(duration);
        //    CalendarEvent noneRigid0 = TestUtility.generateCalendarEvent(duration, new Repetition(), start, start.AddDays(0.8), 4, false, locations[0]);
        //    CalendarEvent noneRigid1 = TestUtility.generateCalendarEvent(duration, new Repetition(), start, start.AddDays(0.8), 4, false, locations[0]);
        //    CalendarEvent rigid0 = TestUtility.generateCalendarEvent(rigidDuration, new Repetition(), start, start.Add(rigidDuration), 1, true, locations[0]);
        //    CalendarEvent rigid1 = TestUtility.generateCalendarEvent(rigidDuration, new Repetition(), rigid0.Start.AddHours(4), rigid0.Start.AddHours(4).Add(rigidDuration), 1, true, locations[0]);

        //    TestSchedule schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
        //    schedule.AddToScheduleAndCommit(noneRigid0).Wait();
        //    schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
        //    schedule.AddToScheduleAndCommit(noneRigid1).Wait();
        //    schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
        //    schedule.AddToScheduleAndCommit(rigid0).Wait();
        //    schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
        //    schedule.AddToScheduleAndCommit(rigid1).Wait();
        //    schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
        //    schedule.FindMeSomethingToDo(locations[0]).Wait();

        //    List<SubCalendarEvent> subEvents = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents).ToList();
        //    subEvents = subEvents.OrderBy(obj => obj.Start).ToList();
        //    List<string> orderedByTimelIds = subEvents.Select(subEvent => subEvent.getId).ToList();
        //    schedule = new TestSchedule(currentUser, refNow, refNow.AddDays(5));
        //    SubCalendarEvent subeventNoConsequence = rigid1.AllSubEvents.First();
        //    TimeSpan timeDelta = TimeSpan.FromMinutes(30);
        //    Tuple < CustomErrors, Dictionary < string, CalendarEvent >> bundleResult = schedule.BundleChangeUpdate(subeventNoConsequence.getId, subeventNoConsequence.getName, subeventNoConsequence.Start, subeventNoConsequence.End.Add(timeDelta), rigid1.Start, rigid1.End, 1, rigid1.Notes.UserNote);

        //    List<SubCalendarEvent> reOrderedSubEvents = bundleResult.Item2.Values.SelectMany(calEvent => calEvent.AllSubEvents).OrderBy(obj => obj.Start).ToList();
        //    List<string> reOrderedByTimelIds = reOrderedSubEvents.Select(subEvent => subEvent.getId).ToList();
        //    Assert.AreEqual(reOrderedByTimelIds.Count, orderedByTimelIds.Count);
        //    for(int i =0; i < orderedByTimelIds.Count; i++)
        //    {
        //        Assert.AreEqual(reOrderedByTimelIds[i], orderedByTimelIds[i]);
        //    }
        //}
        //[TestInitialize]
        //public void cleanUpLog()
        //{
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    Schedule Schedule = new TestSchedule(currentUser, refNow);
        //    currentUser.DeleteAllCalendarEvents();
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

        //[ClassCleanup]
        //public static void cleanUpTest()
        //{
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    Schedule Schedule = new TestSchedule(currentUser, refNow);
        //    currentUser.DeleteAllCalendarEvents();
        //}

    }
}
