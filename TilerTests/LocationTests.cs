using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerCore;
using TilerElements;
using TilerFront;

namespace TilerTests
{
    [TestClass]
    public class LocationTests
    {
        [TestMethod]
        public void LocationVeification()
        {
            var packet = TestUtility.CreatePacket();
            Location currLocation = new Location("3755 Moorhead Ave, Boulder, CO 80305");
            currLocation.verify();
            Location homeLocation = new Location("200 summit blvd Boulder CO", "home");
            Location otherLocation = new Location("200 summit blvd Boulder CO", "home");
            Location workLocation = new Location("3333 walnut rd boulder CO", "work");
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;

            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration + duration + duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule Schedule = new TestSchedule(user, refNow);
            Schedule.CurrentLocation = currLocation;
            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 4, false, restrictionProfile: new RestrictionProfile(start, duration + duration), now: Schedule.Now, location: workLocation) as CalendarEventRestricted;
            Schedule.CurrentLocation = currLocation;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            Assert.IsTrue(testEvent.ActiveSubEvents.First().isLocationAmbiguous);// Location string from google is more than 45% differenet, count wise
            Assert.IsTrue(testEvent.ActiveSubEvents.First().Location.IsVerified);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            CalendarEventRestricted testEvent1 = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 4, false, restrictionProfile: new RestrictionProfile(start, duration + duration), now: Schedule.Now, location: homeLocation) as CalendarEventRestricted;
            Schedule.CurrentLocation = currLocation;
            Schedule.AddToScheduleAndCommitAsync(testEvent1).Wait();
            Assert.IsFalse(testEvent1.ActiveSubEvents.First().isLocationAmbiguous);
            Assert.IsTrue(testEvent1.ActiveSubEvents.First().Location.IsVerified);
        }

        /// <summary>
        /// This test verifies that the location cache is updated whenever an event is looked up. It also verifies the last time the cache entries were updated
        /// The test needs an internet connection to pass
        /// </summary>
        [TestMethod]
        public void LocationCacheValidation()
        {
            var packet = TestUtility.CreatePacket();
            Location currLocation= new Location("3755 Moorhead Ave, Boulder, CO 80305");
            currLocation.verify();
            Location locationA= new Location("200 summit blvd Boulder CO", "home");
            Location locationB = new Location("3333 walnut rd boulder CO", "work");
            Location locationC = new Location("Makola African Store", "African Store");
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;

            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            DateTimeOffset EndfOfDay = refNow.AddHours(-1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(1).AddHours(-1);
            Repetition repetition = new Repetition();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            tilerUser.EndfOfDay = EndfOfDay;
            TestSchedule Schedule = new TestSchedule(user, refNow);
            Schedule.CurrentLocation = currLocation;
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, location: locationA);
            Schedule.CurrentLocation = currLocation;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset secondRefnow = refNow.AddHours(2);
            Schedule = new TestSchedule(user, secondRefnow);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, location: locationB);
            Schedule.CurrentLocation = currLocation;
            Schedule.AddToScheduleAndCommitAsync(testEvent1).Wait();
            TravelCache travelCache = Schedule.TravelCache;
            Assert.IsTrue(travelCache.LocationCombo.Count == 1);
            LocationCacheEntry locationCacheFirstEntry = null;
            foreach (LocationCacheEntry locationCacheEntry in travelCache.LocationCombo)
            {
                Assert.AreEqual(locationCacheEntry.LastLookup, secondRefnow);
                Assert.AreEqual(locationCacheEntry.LastUpdate, secondRefnow);
                locationCacheFirstEntry = locationCacheEntry;
            }

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, secondRefnow);
            CalendarEvent testEvent2 = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, location: locationC);
            Schedule.CurrentLocation = currLocation;
            Schedule.AddToScheduleAndCommitAsync(testEvent2).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset thirdRefnow = refNow.AddHours(2.5);
            // Cache entries are only 30 minutes old
            Schedule = new TestSchedule(user, thirdRefnow);
            Schedule.ProcrastinateAll(TimeSpan.FromMinutes(20));
            Schedule.persistToDB().Wait();

            DateTimeOffset fourthRefnow = refNow.AddHours(3);
            Schedule = new TestSchedule(user, fourthRefnow);
            TravelCache secondTravelCache = Schedule.TravelCache;
            Assert.IsTrue(secondTravelCache.LocationCombo.Count == 3);
            int lastActiveSchedulingLookupCount = 0;
            int lastActiveSchedulingUpdateCount = 0;
            foreach (LocationCacheEntry locationCacheEntry in secondTravelCache.LocationCombo)
            {
                if(locationCacheEntry.LastLookup == thirdRefnow)
                {
                    ++lastActiveSchedulingLookupCount;
                }

                if (locationCacheEntry.LastUpdate == thirdRefnow)
                {
                    ++lastActiveSchedulingUpdateCount;
                }
            }

            Assert.AreEqual(lastActiveSchedulingLookupCount, 2);
            Assert.AreEqual(lastActiveSchedulingUpdateCount, 0);


            //cache should be 1.5 hours old so should be updated
            DateTimeOffset fifthRefnow = refNow.AddHours(5);
            Schedule = new TestSchedule(user, fifthRefnow);
            Schedule.FindMeSomethingToDo(currLocation).Wait();
            Schedule.persistToDB().Wait();
            Schedule = new TestSchedule(user, fourthRefnow);
            TravelCache thirdTravelCache = Schedule.TravelCache;
            Assert.IsTrue(thirdTravelCache.LocationCombo.Count == 3);
            lastActiveSchedulingLookupCount = 0;
            lastActiveSchedulingUpdateCount = 0;
            foreach (LocationCacheEntry locationCacheEntry in thirdTravelCache.LocationCombo)
            {
                if (locationCacheEntry.LastLookup == fifthRefnow)
                {
                    ++lastActiveSchedulingLookupCount;
                }

                if (locationCacheEntry.LastUpdate == fifthRefnow)
                {
                    ++lastActiveSchedulingUpdateCount;
                }
            }

            Assert.AreEqual(lastActiveSchedulingLookupCount, 2);
            Assert.AreEqual(lastActiveSchedulingUpdateCount, 2);
        }
    }
}
