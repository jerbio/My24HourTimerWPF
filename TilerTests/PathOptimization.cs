using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using My24HourTimerWPF;
using TilerElements;
using System.Collections.Generic;

namespace TilerTests
{
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
        public void TestMethod1()
        {
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

            List<Location_Elements> locations = new List<Location_Elements>() { homeLocation, workLocation, gymLocation, churchLocation };
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
