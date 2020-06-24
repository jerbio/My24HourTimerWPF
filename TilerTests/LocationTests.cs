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
            Assert.IsTrue(testEvent1.ActiveSubEvents.First().isLocationAmbiguous);
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
            HashSet<string> calendarIds = new HashSet<string>() { tilerUser.ClearAllId };
            Schedule = new TestSchedule(user, secondRefnow, calendarIds: calendarIds);
            CalendarEvent testEvent2 = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, location: locationC);
            Schedule.CurrentLocation = currLocation;
            Schedule.AddToScheduleAndCommitAsync(testEvent2).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset thirdRefnow = refNow.AddHours(2.5);
            // Cache entries are only 30 minutes old
            Schedule = new TestSchedule(user, thirdRefnow, calendarIds: calendarIds);
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

            Assert.AreEqual(3, lastActiveSchedulingLookupCount);
            Assert.AreEqual(0, lastActiveSchedulingUpdateCount);


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


        /// <summary>
        /// Test validates that the right cache location is picked relative to the current location. It verifies that the cache is also purged after the CacheExpirationTimeSpan timespan
        /// </summary>
        [TestMethod]
        public void LocationCacheAndPurging()
        {
            string gymName = "24 hour fitness";
            Location coloadoSpringLocation = new Location(38.8659815, -104.7189151);
            coloadoSpringLocation.IsVerified = true;
            Location broomfieldLocation = new Location(39.9456167, -105.1376022);
            broomfieldLocation.IsVerified = true;
            Location boulderLocation = new Location(40.0293704, -105.2749966);
            boulderLocation.IsVerified = true;
            Location homeLocation = new Location(39.9257505, -105.1480946);
            homeLocation.IsVerified = true;
            Location workLocation = new Location(40.0202094, -105.2511518);
            workLocation.IsVerified = true;
            Location gymLocation = new Location(gymName);
            string coSpringsString = "colorado springs";
            string boulderString = "boulder";
            string broomfieldString = "broomfield";
            string lafayetteString = "lafayette";

            var packet = TestUtility.CreatePacket();
            TilerUser tilerUser = packet.User;
            UserAccount user = packet.Account;
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            refNow = refNow.Date.ToUniversalTime();


            ///Adding event around colorado spring location should have all subevents select a gym around 24 hour fitness
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(2);
            int splitCount = 2;
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(90);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, splitCount, false, location: gymLocation);
            schedule.CurrentLocation = coloadoSpringLocation;
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            foreach (Location location in schedule.getAllCalendarEvents().SelectMany(o => o.ActiveSubEvents.Select(sub => sub.Location)).ToList())
            {
                Assert.IsTrue(location.Address.ToLower().Contains(coSpringsString));
            }

            ///Running a schedule update around broomfield should have all gym locations around broomfield. The old colorado springs is going to be used one more time(this around parallelcallstotwentyfourhours) before a refresh around optimization
            DateTimeOffset oneDayAfterRefNow = refNow.AddDays(1);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, oneDayAfterRefNow);
            schedule.CurrentLocation = broomfieldLocation;
            schedule.FindMeSomethingToDo(broomfieldLocation).Wait();
            schedule.persistToDB().Wait();
            Assert.AreEqual(schedule.getAllLocations().Count(), 1);
            foreach (Location location in schedule.getAllCalendarEvents().SelectMany(o => o.ActiveSubEvents.Select(sub => sub.Location)).ToList())
            {
                Assert.IsTrue(location.Address.ToLower().Contains(broomfieldString) || location.Address.ToLower().Contains(lafayetteString));// this can sometime use lafayette. This happened in my case when I ran this test with a VPN location in london. If this fails verify your PCs location
                (location as LocationJson).LastUsed = schedule.Now.constNow;
            }

            ///Running a schedule update around boulder should have all gym locations around boulder. 
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset fifteenDayAfterRefNow = refNow.AddDays(15);
            schedule = new TestSchedule(user, fifteenDayAfterRefNow);
            schedule.CurrentLocation = boulderLocation;
            schedule.FindMeSomethingToDo(boulderLocation).Wait();
            schedule.persistToDB().Wait();
            Assert.AreEqual(schedule.getAllLocations().Count(), 1);
            foreach (Location location in schedule.getAllCalendarEvents().SelectMany(o => o.ActiveSubEvents.Select(sub => sub.Location)).ToList())
            {

                Assert.IsTrue(location.Address.ToLower().Contains(boulderString));
                Assert.AreEqual((location as LocationJson).LastUsed, schedule.Now.constNow);
            }


            ///Running a schedule update around colorado springs should have all gym locations around colorado springs.
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset thirtyDayAfterRefNow = refNow.AddDays(30);
            schedule = new TestSchedule(user, thirtyDayAfterRefNow);
            schedule.CurrentLocation = coloadoSpringLocation;
            schedule.FindMeSomethingToDo(coloadoSpringLocation).Wait();
            schedule.persistToDB().Wait();
            foreach (Location location in schedule.getAllCalendarEvents().SelectMany(o => o.ActiveSubEvents.Select(sub => sub.Location)).ToList())
            {
                Assert.IsTrue(location.Address.ToLower().Contains(coSpringsString));
                Assert.AreEqual((location as LocationJson).LastUsed, schedule.Now.constNow);
            }

            Assert.AreEqual(schedule.getAllLocations().Count(), 1);

            Location twentyHourFitnessAmbiguous = schedule.getAllLocations().First();

            Assert.AreEqual(twentyHourFitnessAmbiguous.LocationValidation.locations.Count(), 2);
            bool boulderCheck = false, coloradoSpringCheck = false, broomfieldCheck = false;

            foreach (Location locations in twentyHourFitnessAmbiguous.LocationValidation.locations)
            {
                boulderCheck |= locations.Address.ToLower().Contains(boulderString);
                broomfieldCheck |= locations.Address.ToLower().Contains(broomfieldString);// brommfield should be false because it got purged during the bpulder inclusion
                coloradoSpringCheck |= locations.Address.ToLower().Contains(coSpringsString);
            }

            Assert.IsTrue(boulderCheck);
            Assert.IsTrue(coloradoSpringCheck);
            Assert.IsFalse(broomfieldCheck);

            ///Running a schedule update around colorado springs should have all gym locations around colorado springs. The old broomfield is going to be used one more time(this around parallelcallstotwentyfourhours) before a refresh when optimization
            DateTimeOffset sixtyDayAfterRefNow = refNow.AddDays(45);
            schedule = new TestSchedule(user, sixtyDayAfterRefNow);
            schedule.CurrentLocation = coloadoSpringLocation;
            schedule.FindMeSomethingToDo(coloadoSpringLocation).Wait();
            schedule.persistToDB().Wait();
            foreach (Location location in schedule.getAllCalendarEvents().SelectMany(o => o.ActiveSubEvents.Select(sub => sub.Location)).ToList())
            {
                Assert.IsTrue(location.Address.ToLower().Contains(coSpringsString));
                Assert.AreEqual((location as LocationJson).LastUsed, schedule.Now.constNow);
            }

            Assert.AreEqual(schedule.getAllLocations().Count(), 1);

            twentyHourFitnessAmbiguous = schedule.getAllLocations().First();
            Assert.AreEqual(twentyHourFitnessAmbiguous.LocationValidation.locations.Count(), 1);

            boulderCheck = false;
            coloradoSpringCheck = false;
            broomfieldCheck = false;

            foreach (Location locations in twentyHourFitnessAmbiguous.LocationValidation.locations)
            {
                boulderCheck |= locations.Address.ToLower().Contains(boulderString);
                broomfieldCheck |= locations.Address.ToLower().Contains(broomfieldString);
                coloradoSpringCheck |= locations.Address.ToLower().Contains(coSpringsString);
            }

            Assert.IsFalse(boulderCheck);
            Assert.IsTrue(coloradoSpringCheck);
            Assert.IsFalse(broomfieldCheck);

        }

        /// <summary>
        /// Test ensures that when subevents of the same caledarevent within the same day have the same location.
        /// So if you have CalenarEvent A (subevent AA and subevent AB) and also CalendarEvent B (with subevent BA and subevent BB)
        /// If subevent AA and AB are in the same day you want them to have the same location. 
        /// However if they're on different days its ok if their dynamic locations are on different days
        /// </summary>
        [TestMethod]
        public void SubeventsOfSameCalendarOnSameDayShouldHaveSameLocation()
        {
            //throw new Exception("test is not written");
            var packet = TestUtility.CreatePacket();
            string locationName = "Walmart";
            Location currLocation = new Location("3755 Moorhead Ave, Boulder, CO 80305");
            currLocation.verify();
            Location home = new Location("200 summit blvd Boulder CO", "home");
            Location work = new Location("3333 walnut rd boulder CO", "work");
            Location walmartLocation = new Location(locationName);
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;

            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(3);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration + duration + duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            TimeLine actualRange = new TimeLine(start, start.AddDays(1).AddHours(-1));



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Repetition walmartRepetition = new Repetition(repetitionRange, Repetition.Frequency.DAILY, actualRange);
            CalendarEvent workTestCalEvent = TestUtility.generateCalendarEvent(tilerUser, duration, walmartRepetition, start, end, 3, location: walmartLocation);

            TestSchedule Schedule = new TestSchedule(user, refNow);
            Schedule.CurrentLocation = home;
            Schedule.AddToScheduleAndCommitAsync(workTestCalEvent).Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Repetition repetition = new Repetition(repetitionRange, Repetition.Frequency.DAILY, actualRange);
            CalendarEvent testCalEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 3, location: work);

            Schedule = new TestSchedule(user, refNow);
            Schedule.CurrentLocation = home;
            Schedule.AddToScheduleAndCommitAsync(testCalEvent).Wait();



            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Repetition rigidRepetition = new Repetition(repetitionRange, Repetition.Frequency.DAILY, actualRange);
            TimeSpan rigidDuration = TimeSpan.FromHours(10);
            CalendarEvent rigidTestCalEvent = TestUtility.generateCalendarEvent(tilerUser, rigidDuration, rigidRepetition, start, end, location: home, rigidFlags: true);

            Schedule = new TestSchedule(user, refNow);
            Schedule.CurrentLocation = home;
            Schedule.AddToScheduleAndCommitAsync(rigidTestCalEvent).Wait();
            List<DayTimeLine> dayTimelines = Schedule.Now.getAllDaysForCalc().ToList();
            foreach(DayTimeLine dayTimeline in dayTimelines)
            {
                Dictionary<string, Location> calEventAddressToLocation = new Dictionary<string, Location>();
                foreach (SubCalendarEvent subevent in dayTimeline.getSubEventsInTimeLine())
                {
                    string calId = subevent.SubEvent_ID.getAllEventDictionaryLookup;
                    if (!calEventAddressToLocation.ContainsKey(calId))
                    {
                        calEventAddressToLocation[calId] = subevent.Location;
                    } 
                    else
                    {
                        Location sameSubeventLocation = calEventAddressToLocation[calId];
                        Assert.AreEqual(subevent.Location.Longitude, sameSubeventLocation.Longitude);
                        Assert.AreEqual(subevent.Location.Latitude, sameSubeventLocation.Latitude);
                    }
                }
            }
        }
    }
}
