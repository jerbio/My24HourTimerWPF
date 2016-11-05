using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using My24HourTimerWPF;
using TilerElements;
using System.Collections.Generic;
using System.Linq;

namespace TilerTests
{
    /// <summary>
    /// This function tries to create a schedule where path optimizatio would try to create an unnecesary conflict when there is sufficient space for all events involved
    /// </summary>
    [TestClass]
    public class PathOptimization
    {
        [ClassInitialize]
        public static void classInitialize(TestContext testContext)
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Now;
            Schedule schedule = new Schedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }

        [TestMethod]
        public void PathOptimizationTIghtScheduleNoUnnecessaryConflict()
        {
            Location_Elements homeLocation = new Location_Elements("2895 Van aken Blvd cleveland OH 44120");
            //homeLocation.Validate();
            //if (homeLocation.isNull)
            //{
            //    throw new AssertFailedException("failed to Validate homeLocation");
            //}

            Location_Elements workLocation = new Location_Elements(41.5002762, -81.6839155, "1228 euclid Ave cleveland OH","Work",false,false);
            //workLocation.Validate();
            //if (workLocation.isNull)
            //{
            //    throw new AssertFailedException("failed to Validate workLocation");
            //}
            Location_Elements gymLocation = new Location_Elements(41.4987461 , -81.6884993, "619 Prospect Avenue Cleveland, OH 44115", "Gym", false, false);
            //gymLocation.Validate();
            //if (gymLocation.isNull)
            //{
            //    throw new AssertFailedException("failed to Validate gymLocation");
            //}

            Location_Elements churchLocation = new Location_Elements(41.569467, -81.539422, "1465 Dille Rd, Cleveland, OH 44117", "Church", false, false);
            //churchLocation.Validate();
            //if (churchLocation.isNull)
            //{
            //    throw new AssertFailedException("failed to Validate churchLocation");
            //}

            Location_Elements shakerLibrary = new Location_Elements(41.4658937, -81.5664832, "16500 Van Aken Blvd, Shaker Heights, OH 44120", "Shake Library", false, false);
            //shakerLibrary.Validate();
            //if (shakerLibrary.isNull)
            //{
            //    throw new AssertFailedException("failed to Validate shakerLibrary");
            //}

            List<Location_Elements> locations = new List<Location_Elements>() { homeLocation, workLocation, gymLocation, churchLocation };
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Now;
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, refNow.Day, 8, 0, 0, new TimeSpan());
            Schedule schedule = new TestSchedule(currentUser, refNow);
            TimeLine encompassingTimeline = new TimeLine(refNow, refNow.AddHours(8));


            int rigidHoursSpan = 4;
            CalendarEvent testHomeRigidCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(rigidHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.Start.AddHours(rigidHoursSpan), 1, true, homeLocation);
            int nonRigidHoursSpan = 1;
            CalendarEvent testHomeNonRigidCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(nonRigidHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, homeLocation);
            CalendarEvent testWorkNonRigidCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(nonRigidHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, workLocation);
            int nonRigidTwoHoursSpan = 2;
            CalendarEvent testGymNonRigidCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(nonRigidTwoHoursSpan), new TilerElements.Repetition(), encompassingTimeline.Start, encompassingTimeline.End, 1, false, gymLocation);

            schedule.AddToScheduleAndCommit(testHomeRigidCalEvent).Wait();
            schedule = new TestSchedule(currentUser, refNow);
            schedule.AddToScheduleAndCommit(testHomeNonRigidCalEvent).Wait();
            schedule = new TestSchedule(currentUser, refNow);
            schedule.AddToScheduleAndCommit(testWorkNonRigidCalEvent).Wait();
            schedule = new TestSchedule(currentUser, refNow);
            schedule.AddToSchedule(testGymNonRigidCalEvent);
            IEnumerable<SubCalendarEvent> allSubEvents = schedule.getAllCalendarEvents().SelectMany(calEvent => calEvent.AllSubEvents);
            List<BlobSubCalendarEvent> conflicts =  Utility.getConflictingEvents(allSubEvents);
            Assert.AreEqual(0, conflicts.Count);
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
