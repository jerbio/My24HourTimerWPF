using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using TilerElements;
using TilerFront;

namespace TilerTests
{
    /// <summary>
    /// This test verify that the entities of the various retrieval sets DB retrievals are actual retrieved
    /// </summary>
    [TestClass]
    public class DBRetrievalOption
    {
        /// <summary>
        /// This handles the scenario where a retrieval set which is targeted at schedule manipulation
        /// </summary>
        [TestMethod]
        public void ScheduleManipulation()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            List<Location> locations = TestUtility.getAdHocLocations();
            Location homeLocation = locations.First();
            homeLocation.verify();
            homeLocation.User = tilerUser;

            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false);
            TestSchedule Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule ScheduleRefreshed = new TestSchedule(user, refNow);
            CalendarEvent scheduledCalendarEvent = ScheduleRefreshed.getCalendarEvent(testEvent.Id);
            Assert.IsNull(scheduledCalendarEvent.Repeat);
            List<SubCalendarEvent> subEvents =  ScheduleRefreshed.getAllRelatedActiveSubEvents(testEvent.Id).ToList();
            Assert.AreEqual(subEvents.Count, testEvent.AllSubEvents.Count(), "The number of sub events before adding should be the same as thos after adding");
            Assert.IsNull(subEvents.First().Name);
            Assert.IsNull(subEvents.First().RestrictionProfile);
            Assert.IsNotNull(subEvents.First().getNowInfo);
            Assert.IsNotNull(subEvents.First().Location);

            DateTimeOffset start = refNow;
            DateTimeOffset restrictionStart = start.Add(TimeSpan.FromMinutes(((long)duration.Minutes / 2)));
            TimeSpan restrictedDurationSpan = (duration) + (duration);
            RestrictionProfile restrictionProfile = new RestrictionProfile(restrictionStart, restrictedDurationSpan);

            // Adds restricted calendarevent and tests if restriction conponents are retrieved
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent restrictedEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false, homeLocation, restrictionProfile, now: Schedule.Now);
            TestSchedule ScheduleWithRestrictedTIles = new TestSchedule(user, refNow);
            ScheduleWithRestrictedTIles.AddToScheduleAndCommitAsync(restrictedEvent).Wait();
            
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule ScheduleRetrieedRefreshed = new TestSchedule(user, refNow);
            List<SubCalendarEvent> retrievedRestrictedSubEvents = ScheduleRetrieedRefreshed.getAllRelatedActiveSubEvents(restrictedEvent.Id).ToList();
            Assert.AreEqual(retrievedRestrictedSubEvents.Count, restrictedEvent.AllSubEvents.Count(), "The number of sub events before adding should be the same as thos after adding");
            Assert.IsNull(retrievedRestrictedSubEvents.First().Name);
            Assert.IsNotNull(retrievedRestrictedSubEvents.First().getNowInfo);
            Assert.IsNotNull(retrievedRestrictedSubEvents.First().Location);
            Assert.IsNotNull(retrievedRestrictedSubEvents.First().RestrictionProfile);
        }


        /// <summary>
        /// This handles the scenario where a retrieval set which is targeted at schedule manipulation with repetition object included
        /// </summary>
        [TestMethod]
        public void ScheduleManipulationWithRepeat()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            List<Location> locations = TestUtility.getAdHocLocations();
            Location homeLocation = locations.First();
            homeLocation.verify();
            homeLocation.User = tilerUser;
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false, homeLocation);
            TestSchedule Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithRepeat);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule ScheduleRefreshed = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithRepeat);
            CalendarEvent scheduledCalendarEvent = ScheduleRefreshed.getCalendarEvent(testEvent.Id);
            Assert.IsNull(scheduledCalendarEvent.Repeat);


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            var repetitionRange = timeLine.StartToEnd;
            DateTimeOffset repeatStart = refNow;
            DateTimeOffset repeatEnd = refNow.Add(duration);
            Repetition repetition = new Repetition(repetitionRange, Repetition.Frequency.DAILY, new TimeLine(repeatStart, repeatEnd));
            CalendarEvent testEventRepetition = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), repetition, timeLine.Start, timeLine.End, 1, false);
            TestSchedule ReoeatSchedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithRepeat);
            ReoeatSchedule.AddToScheduleAndCommitAsync(testEventRepetition).Wait();

            TestSchedule RepeatSchedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithRepeat);
            CalendarEvent scheduledRepeatCalendarEventRetrieced = RepeatSchedule.getCalendarEvent(testEventRepetition.Id);
            Assert.IsNotNull(scheduledRepeatCalendarEventRetrieced.Repeat);


            ScheduleRefreshed = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithRepeat);
            List<SubCalendarEvent> subEvents = ScheduleRefreshed.getAllRelatedActiveSubEvents(testEvent.Id).ToList();
            Assert.AreEqual(subEvents.Count, testEvent.AllSubEvents.Count(), "The number of sub events before adding should be the same as thos after adding");
            Assert.IsNull(subEvents.First().Name);
            Assert.IsNull(subEvents.First().RestrictionProfile);
            Assert.IsNotNull(subEvents.First().getNowInfo);
            Assert.IsNotNull(subEvents.First().Location);

            DateTimeOffset start = refNow;
            DateTimeOffset restrictionStart = start.Add(TimeSpan.FromMinutes(((long)duration.Minutes / 2)));
            TimeSpan restrictedDurationSpan = (duration) + (duration);


            // Adds restricted calendarevent and tests if restriction conponents are retrieved

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            RestrictionProfile restrictionProfile = new RestrictionProfile(restrictionStart, restrictedDurationSpan);
            Location otherLocation = TestUtility.getAdHocLocations()[1];
            otherLocation.User = tilerUser;
            CalendarEvent restrictedEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false, otherLocation, restrictionProfile, now: Schedule.Now);
            TestSchedule ScheduleResrictionProfile = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithRepeat);
            ScheduleResrictionProfile.AddToScheduleAndCommitAsync(restrictedEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule ScheduleResrictionProfileRetrieved = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithRepeat);
            List<SubCalendarEvent> restrictedSubEvents = ScheduleResrictionProfileRetrieved.getAllRelatedActiveSubEvents(restrictedEvent.Id).ToList();
            Assert.AreEqual(restrictedSubEvents.Count, restrictedEvent.AllSubEvents.Count(), "The number of sub events before adding should be the same as thos after adding");
            Assert.IsNull(restrictedSubEvents.First().Name);
            Assert.IsNotNull(restrictedSubEvents.First().getNowInfo);
            Assert.IsNotNull(restrictedSubEvents.First().Location);
            Assert.IsNotNull(restrictedSubEvents.First().RestrictionProfile);


            // Adds repeating restricted calendarevent and tests if restriction conponents are retrieved

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            RestrictionProfile repeatingRestrictionProfile = new RestrictionProfile(restrictionStart, restrictedDurationSpan);
            Location repeatOtherLocation = TestUtility.getAdHocLocations()[4];
            repeatOtherLocation.User = tilerUser;
            Repetition restrictedRepetition = new Repetition(repetitionRange, Repetition.Frequency.DAILY, new TimeLine(repeatStart, repeatEnd));
            CalendarEvent repeatRestrictedEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), restrictedRepetition, timeLine.Start, timeLine.End, 1, false, repeatOtherLocation, repeatingRestrictionProfile, now: Schedule.Now);
            TestSchedule ScheduleRepeatResrictionProfile = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithRepeat);
            ScheduleRepeatResrictionProfile.AddToScheduleAndCommitAsync(repeatRestrictedEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule ScheduleRepeatResrictionProfileRetrieved = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithRepeat);
            Assert.IsNotNull(repeatRestrictedEvent.Repeat);
            List<SubCalendarEvent> repeatRestrictedSubEvents = ScheduleRepeatResrictionProfileRetrieved.getAllRelatedActiveSubEvents(repeatRestrictedEvent.Id).ToList();
            Assert.IsNull(repeatRestrictedSubEvents.First().Name);
            Assert.IsNotNull(repeatRestrictedSubEvents.First().getNowInfo);
            Assert.IsNotNull(repeatRestrictedSubEvents.First().Location);
            Assert.IsNotNull(repeatRestrictedSubEvents.First().RestrictionProfile);
        }


        /// <summary>
        /// This handles the scenario where a retrieval set which is targeted at Timeline update history
        /// </summary>
        [TestMethod]
        public void ScheduleManipulationWithUpdateHistory()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            List<Location> locations = TestUtility.getAdHocLocations();
            Location homeLocation = locations.First();
            homeLocation.verify();
            homeLocation.User = tilerUser;
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false, homeLocation);
            TestSchedule Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule ScheduleRefreshed = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            CalendarEvent scheduledCalendarEvent = ScheduleRefreshed.getCalendarEvent(testEvent.Id);
            Assert.IsNotNull(scheduledCalendarEvent.TimeLineHistory);
            Assert.IsNotNull(scheduledCalendarEvent.Location);
        }


        /// <summary>
        /// This handles the scenario where a retrieval set which is targeted at UI retrieval, essentially for the front end
        /// </summary>
        [TestMethod]
        public void ScheduleManipulationWithUiSet()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            List<Location> locations = TestUtility.getAdHocLocations();
            Location homeLocation = locations.First();
            homeLocation.verify();
            homeLocation.User = tilerUser;
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false, homeLocation);
            TestSchedule Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule ScheduleForSetAsNow = new TestSchedule(user, refNow);

            ScheduleForSetAsNow.SetCalendarEventAsNow(testEvent.Id);
            ScheduleForSetAsNow.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule ScheduleRefreshed = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.UiSet);
            CalendarEvent scheduledCalendarEventRetrieved = ScheduleRefreshed.getCalendarEvent(testEvent.Id);
            Assert.IsNotNull(scheduledCalendarEventRetrieved.Name);
            Assert.IsNotNull(scheduledCalendarEventRetrieved.Location);
            Assert.IsNotNull(scheduledCalendarEventRetrieved.AllSubEvents);
            Assert.IsNull(scheduledCalendarEventRetrieved.getNowInfo);

            SubCalendarEvent firstSubEvent = scheduledCalendarEventRetrieved.AllSubEvents.First();
            Assert.IsNotNull(firstSubEvent.Name);
            Assert.IsNotNull(firstSubEvent.Location);
            Assert.IsNull(firstSubEvent.getNowInfo);
        }



        /// <summary>
        /// This handles the scenario where a retrieval set which is targeted at UI retrieval, essentially for the front end
        /// </summary>
        [TestMethod]
        public void ScheduleManipulationWitAnalysisManipulation()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            List<Location> locations = TestUtility.getAdHocLocations();
            Location homeLocation = locations.First();
            homeLocation.verify();
            homeLocation.User = tilerUser;
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false, homeLocation);
            TestSchedule Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule ScheduleForSetAsNow = new TestSchedule(user, refNow);

            ScheduleForSetAsNow.SetCalendarEventAsNow(testEvent.Id);
            ScheduleForSetAsNow.persistToDB().Wait();


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule ScheduleRefreshed = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.analysisManipulation);
            CalendarEvent scheduledCalendarEventRetrieved = ScheduleRefreshed.getCalendarEvent(testEvent.Id);
            Assert.IsNull(scheduledCalendarEventRetrieved.Name);
            Assert.IsNotNull(scheduledCalendarEventRetrieved.Location);
            Assert.IsNotNull(scheduledCalendarEventRetrieved.AllSubEvents);
            Assert.IsNotNull(scheduledCalendarEventRetrieved.getNowInfo);
            Assert.IsNotNull(scheduledCalendarEventRetrieved.TimeLineHistory);

            SubCalendarEvent firstSubEvent = scheduledCalendarEventRetrieved.AllSubEvents.First();
            Assert.IsNull(firstSubEvent.Name);
            Assert.IsNotNull(firstSubEvent.Location);
            Assert.IsNotNull(firstSubEvent.getNowInfo);
        }


    }
}
