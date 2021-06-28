using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerElements;
using My24HourTimerWPF;
using TilerFront;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TilerCore;
using TilerTests.Models;
using System.Data.Entity;
using System.Globalization;
using Google.Apis.Calendar.v3.Data;
using static TilerElements.Reason;

namespace TilerTests
{
    [TestClass]
    public class Creation
    {
        public TestContext TestContext
        {
            get;
            set;
        }

        [TestMethod]
        public void ReferenceNowLogic()
        {
            TimeSpan timeDifference = TimeSpan.FromHours(-6);
            DateTimeOffset currentTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, timeDifference);
            DateTimeOffset endOfDay = new DateTimeOffset(1970, 1, 1, 22, 0, 0, timeDifference);

            ReferenceNow now = new ReferenceNow(currentTime, endOfDay, timeDifference);
            DateTimeOffset dayTimeOffset = now.getClientBeginningOfDay(0);
            Assert.AreEqual(dayTimeOffset.Hour, 6);
            Assert.AreEqual(dayTimeOffset.Year, 1970);
            Assert.AreEqual(dayTimeOffset.Day, 2);
        }

        [TestMethod]
        public void TestContextCaching ()
        {
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false);
            Assert.IsFalse(testEvent.isCalculableInitialized);
            string testEVentId = testEvent.Id;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            Assert.IsTrue(testEvent.isCalculableInitialized);// this should be true because AddToScheduleAndCommit should trigger a switch of isCalculableInitialized

            UserAccount userAcc = TestUtility.getTestUser(userId: tilerUser.Id);
            Task<CalendarEvent> waitVar = userAcc.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            CalendarEvent verificationEventPulled = waitVar.Result;
            Assert.IsFalse(verificationEventPulled.isCalculableInitialized);

            //after refreshing context the data should be reset
            userAcc = TestUtility.getTestUser(true, userId: tilerUser.Id);
            waitVar = userAcc.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            verificationEventPulled = waitVar.Result;
            Assert.IsFalse(verificationEventPulled.isCalculableInitialized);


            Assert.IsNotNull(testEvent);
            Assert.IsNotNull(verificationEventPulled);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));
        }

        [TestMethod]
        public void createUserTest()
        {
            TestDBContext mockContext = new TestDBContext();

            string userId = Guid.NewGuid().ToString();
            string FirstName = "First Name " + userId;
            string lastName = "Last Name " + userId;
            string userName = "userName " + userId;
            string testEmail = userId + "test@tiler.com";
            TilerUser user = new TilerUser()
            {
                Id = userId,
                UserName = userName,
                Email = testEmail,
                FirstName = FirstName,
                LastName = lastName
            };
            //var mockContext = TestUtility.getContext;


            mockContext.Users.Add(user);
            mockContext.SaveChanges();
            var verificationUserPulled = mockContext.Users.Find(userId);
            Assert.IsNotNull(user);
            Assert.IsNotNull(verificationUserPulled);
            Assert.IsTrue(user.isTestEquivalent(verificationUserPulled));
        }

        [TestMethod]
        public void createAddSubCalendarEventToDB()
        {
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            CalendarEvent testCalEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false);
            SubCalendarEvent testEvent = testCalEvent.EnabledSubEvents.First();
            string testEventId = testEvent.Id;
            var mockContext = user.ScheduleLogControl.Database;
            mockContext.SubEvents.Add(testEvent);
            mockContext.SaveChanges();
            mockContext.Dispose();
            mockContext = new TestDBContext();
            var verificationEventPulled = mockContext.SubEvents
                                            .Include(subEvent => subEvent.ParentCalendarEvent)
                                            .Include(subEvent => subEvent.ParentCalendarEvent.Procrastination_EventDB)
                                            .Include(subEvent => subEvent.ParentCalendarEvent.ProfileOfNow_EventDB)
                                            .FirstOrDefault(subevent => subevent.Id == testEventId);

            Assert.IsNotNull(testEvent);
            Assert.IsNotNull(verificationEventPulled);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));
        }

        [TestMethod]
        public void createAddCalendarEventToDB()
        {
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            TilerColor tilerColor = new TilerColor(255, 255, 0, 1, 5);
            EventDisplay eventdisplay = new EventDisplay(true, tilerColor);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false, eventDisplay: eventdisplay);
            string testEVentId = testEvent.Id;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            var mockContext = new TestDBContext();
            UserAccount userAcc = TestUtility.getTestUser(true, userId: tilerUser.Id);

            Task<CalendarEvent> waitVar = userAcc.ScheduleLogControl.getCalendarEventWithID(testEvent.Id, DataRetrievalSet.All);
            waitVar.Wait();
            CalendarEvent verificationEventPulled = waitVar.Result;

            Assert.IsNotNull(testEvent);
            Assert.IsNotNull(verificationEventPulled);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            CalendarEvent loadedFromSchedule = Schedule.getCalendarEvent(testEvent.Id);
            Assert.IsTrue(testEvent.isTestEquivalent(loadedFromSchedule));
            Assert.IsTrue(verificationEventPulled.isTestEquivalent(loadedFromSchedule));
        }


#if RunSlowTest
        [TestMethod]
#endif
        public void createAllPossibleCalendarEventToSchedule()
        {
            DateTimeOffset iniRefNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            DateTimeOffset refNow = iniRefNow;
            var packet = TestUtility.CreatePacket();
            TilerUser tilerUser = packet.User;
            UserAccount user = packet.Account;

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            List<CalendarEvent> calEvents = TestUtility.generateAllCalendarEventVaraints(schedule,duration, refNow, tilerUser, user);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule Schedule = new TestSchedule(user, refNow);
            Task<ScheduleDump> taskScheduleDump = Schedule.CreateScheduleDump(Schedule.Now);
            taskScheduleDump.Wait();
            ScheduleDump scheduleDump = taskScheduleDump.Result;
            TestSchedule scheduleDumpSchedule = new TestSchedule(scheduleDump, user);
            TestUtility.isTestEquivalent(Schedule, scheduleDumpSchedule);

        }
        /// <summary>
        /// UTC TimeZone Required
        /// </summary>
        [TestMethod]
        public void storeScheudlueDumpToDB()
        {
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            TestSchedule Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.All);
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            TilerColor tilerColor = new TilerColor(255, 255, 0, 1, 5);
            EventDisplay eventdisplay = new EventDisplay(true, tilerColor);

            Location location = TestUtility.getAdHocLocations()[0];
            location.verify();

            // Adding event one
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false, eventDisplay: eventdisplay, location: location);
            string testEVentId = testEvent.Id;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            Task<ScheduleDump> dumpWait = Schedule.CreateAndPersistScheduleDump();
            dumpWait.Wait();
            ScheduleDump scheduleDump = dumpWait.Result;

            var mockContext = user.ScheduleLogControl.Database;

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            mockContext = user.ScheduleLogControl.Database;
            ScheduleDump retrievedDump = mockContext.ScheduleDumps.Find(scheduleDump.Id);

            Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.All);

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            Schedule scheduleFromDump = new TestSchedule(retrievedDump, user);

            Assert.IsTrue(Schedule.isTestEquivalent(scheduleFromDump));

            // Adding event two
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            Location location1 = TestUtility.getAdHocLocations()[1];
            TilerColor tilerColor1 = new TilerColor(255, 255, 0, 1, 5);
            EventDisplay eventdisplay1 = new EventDisplay(true, tilerColor1);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false, eventDisplay: eventdisplay1, location: location1);
            string testEVentId1 = testEvent1.Id;
            TestSchedule Schedule1 = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.All);
            Schedule1.AddToScheduleAndCommitAsync(testEvent1).Wait();
            Task<ScheduleDump> tempScheduleDumpTask = Schedule1.CreateScheduleDump();
            tempScheduleDumpTask.Wait();
            ScheduleDump tempScheduleDump = tempScheduleDumpTask.Result;
            Task<ScheduleDump> dumpWait1 = Schedule1.CreateAndPersistScheduleDump(tempScheduleDump);
            dumpWait1.Wait();
            ScheduleDump scheduleDump1 = dumpWait1.Result;

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            var mockContext1 = user.ScheduleLogControl.Database;

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            mockContext1 = user.ScheduleLogControl.Database;
            ScheduleDump retrievedDump1 = mockContext1.ScheduleDumps.Find(scheduleDump1.Id);

            Schedule1 = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.All);

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            Schedule scheduleFromDump1 = new TestSchedule(retrievedDump1, user);

            Assert.IsTrue(Schedule1.isTestEquivalent(scheduleFromDump1));
        }



        /// <summary>
        /// UTC TimeZone Required
        /// </summary>
        [TestMethod]
        public void sameScheduleChangeAfterSameInput()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            TestSchedule Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.All);
            TimeSpan duration = TimeSpan.FromHours(4);

            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            TilerColor tilerColor = new TilerColor(255, 255, 0, 1, 5);
            EventDisplay eventdisplay = new EventDisplay(true, tilerColor);

            Location location = TestUtility.getAdHocLocations()[0];
            location.verify();

            // Adding event one
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition();
            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end,4, false, restrictionProfile: new RestrictionProfile(start, duration + duration), now: Schedule.Now) as CalendarEventRestricted;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.All);
            Location location1 = TestUtility.getAdHocLocations()[1];
            TilerColor tilerColor1 = new TilerColor(255, 255, 0, 1, 5);
            EventDisplay eventdisplay1 = new EventDisplay(true, tilerColor1);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false, eventDisplay: eventdisplay1, location: location1);
            Schedule.AddToScheduleAndCommitAsync(testEvent1).Wait();


            Task<ScheduleDump> dumpWait = Schedule.CreateAndPersistScheduleDump();
            dumpWait.Wait();
            ScheduleDump scheduleDump = dumpWait.Result;
            var mockContext = user.ScheduleLogControl.Database;

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            mockContext = user.ScheduleLogControl.Database;
            ScheduleDump retrievedDump = mockContext.ScheduleDumps.Find(scheduleDump.Id);

            TestSchedule ScheduleFromRDBMS = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.All);

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            TestSchedule scheduleFromDump = new TestSchedule(retrievedDump, user);

            Assert.IsTrue(ScheduleFromRDBMS.isTestEquivalent(scheduleFromDump));

            // Adding event two
            Location location2 = TestUtility.getAdHocLocations()[2];
            TilerColor tilerColor2 = new TilerColor(255, 255, 0, 1, 5);
            EventDisplay eventdisplay2 = new EventDisplay(true, tilerColor2);

            CalendarEvent testEvent2 = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(5), new Repetition(), timeLine.Start, timeLine.End, 4, false, eventDisplay: eventdisplay2, location: location2);
            CalendarEvent testEvent2Cpy = testEvent2.createCopy();
            //CalendarEvent testEvent2Cpy = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(5), new Repetition(), timeLine.Start, timeLine.End, 4, false, eventDisplay: eventdisplay2, location: location2);
            ScheduleFromRDBMS = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.All);



            ScheduleFromRDBMS.AddToSchedule(testEvent2);
            scheduleFromDump.AddToSchedule(testEvent2Cpy);
            List<SubCalendarEvent> dumpSubEvents = scheduleFromDump.getAllCalendarEvents().Where(calEvent => calEvent.isActive).SelectMany(calEvent => calEvent.AllSubEvents).OrderBy(subEvent=> subEvent.Start).ToList();
            List<SubCalendarEvent> rdbmsSubEvents = ScheduleFromRDBMS.getAllCalendarEvents().Where(calEvent => calEvent.isActive).SelectMany(calEvent => calEvent.AllSubEvents).OrderBy(subEvent => subEvent.Start).ToList();


            Assert.IsTrue(dumpSubEvents.Count == rdbmsSubEvents.Count);
            for (int i=0; i< dumpSubEvents.Count;i++)
            {
                SubCalendarEvent dumpSubEvent = dumpSubEvents[i];
                SubCalendarEvent rdbmsSubEvent = rdbmsSubEvents[i];
                Assert.IsTrue(dumpSubEvent.Start == rdbmsSubEvent.Start);
                Assert.IsTrue(dumpSubEvent.End== rdbmsSubEvent.End);
            }
        }



        [TestMethod]
        public void TestCreationOfNonRigid()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(5);
            List<TimeLine> timeLines = TestUtility.getTimeFrames(refNow , duration);
            foreach(TimeLine eachTimeLine in timeLines)
            {
                DateTimeOffset TimeCreation = DateTimeOffset.UtcNow;
                tilerUser = user.getTilerUser();
                CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1),  new Repetition(), eachTimeLine.Start, eachTimeLine.End, 1, false);
                testEvent.TimeCreated = TimeCreation;
                Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
                user = TestUtility.getTestUser(userId: tilerUser.Id);
                user.Login().Wait();
                Schedule = new TestSchedule(user, refNow);
                string testEVentId = testEvent.getId;
                Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEVentId);
                waitVar.Wait();
                CalendarEvent newlyaddedevent = waitVar.Result;
                Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
                Assert.AreEqual(testEvent.TimeCreated, TimeCreation);
            }
        }

        [TestMethod]
        public void TestCreationOfRigid()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            DateTimeOffset TimeCreation = DateTimeOffset.UtcNow;
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, true);
            testEvent.TimeCreated = TimeCreation;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            var testEVentId = testEvent.Id;
            var mockContext = new TestDBContext();
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEVentId);
            waitVar.Wait();
            CalendarEvent verificationEventPulled = waitVar.Result;
            Assert.IsNotNull(testEvent);
            Assert.IsNotNull(verificationEventPulled);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            CalendarEvent loadedFromSchedule = Schedule.getCalendarEvent(testEvent.Id);
            Assert.IsTrue(testEvent.isTestEquivalent(loadedFromSchedule));
            Assert.IsTrue(verificationEventPulled.isTestEquivalent(loadedFromSchedule));
        }

        [TestMethod]
        public void TestCreationOfRepeatRigid()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(14));
            Repetition repetition = new Repetition(repetitionRange, Repetition.Frequency.DAILY, new TimeLine(start, end));
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, true);
            string testEVentId = testEvent.getId;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEVentId);
            waitVar.Wait();
            CalendarEvent verificationEventPulled = waitVar.Result;
            Assert.IsNotNull(testEvent);
            Assert.IsNotNull(verificationEventPulled);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.All);
            CalendarEvent loadedFromSchedule = Schedule.getCalendarEvent(testEvent.Id);
            Assert.IsTrue(testEvent.isTestEquivalent(loadedFromSchedule));
            Assert.IsTrue(verificationEventPulled.isTestEquivalent(loadedFromSchedule));
        }


        [TestMethod]
        public void TestCreationOfNonRigidWithLocation()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(5);
            Location location = TestUtility.getAdHocLocations()[0];
            location.verify();
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            DateTimeOffset TimeCreation = DateTimeOffset.UtcNow;
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false, location);
            testEvent.TimeCreated = TimeCreation;
            //user = TestUtility.getTestUser(userId: tilerUser.Id);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            user.Login().Wait();
            Schedule = new TestSchedule(user, refNow);
            string testEVentId = testEvent.getId;
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEVentId);
            waitVar.Wait();
            CalendarEvent verificationEventPulled = waitVar.Result;
            Assert.AreEqual(testEvent.getId, verificationEventPulled.getId);
            Assert.AreEqual(testEvent.TimeCreated, TimeCreation);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            CalendarEvent loadedFromSchedule = Schedule.getCalendarEvent(testEvent.Id);
            Assert.IsTrue(testEvent.isTestEquivalent(loadedFromSchedule));
            Assert.IsTrue(verificationEventPulled.isTestEquivalent(loadedFromSchedule));
        }

        [TestMethod]
        public void TestCreationOfRigidithLocation()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            Location location = TestUtility.getAdHocLocations()[0];
            location.verify();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            DateTimeOffset TimeCreation = DateTimeOffset.UtcNow;
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, true, location);
            testEvent.TimeCreated = TimeCreation;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            var mockContext = new TestDBContext();
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);
            var testEVentId = testEvent.Id;
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEVentId);
            waitVar.Wait();
            CalendarEvent verificationEventPulled = waitVar.Result;
            Assert.IsNotNull(testEvent);
            Assert.IsNotNull(verificationEventPulled);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            CalendarEvent loadedFromSchedule = Schedule.getCalendarEvent(testEvent.Id);
            Assert.IsTrue(testEvent.isTestEquivalent(loadedFromSchedule));
            Assert.IsTrue(verificationEventPulled.isTestEquivalent(loadedFromSchedule));
        }

        /// <summary>
        /// Test creates a restricted profile calendarEvent
        /// </summary>
        [TestMethod]
        public void TestCreationOfCalendarEventRestricted()
        {

            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition();
            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, restrictionProfile: new RestrictionProfile(start, duration + duration), now: Schedule.Now) as CalendarEventRestricted;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            var mockContext = new TestDBContext();
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            
            CalendarEvent newlyaddedevent = waitVar.Result;
            Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
            Assert.IsTrue(testEvent.isTestEquivalent(newlyaddedevent));

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            CalendarEvent loadedFromSchedule = Schedule.getCalendarEvent(testEvent.Id);
            Assert.IsTrue(testEvent.isTestEquivalent(loadedFromSchedule));
            Assert.IsTrue(newlyaddedevent.isTestEquivalent(loadedFromSchedule));
        }


        /// <summary>
        /// Test creates a non-viable restricted calevent with a restriction profile and should throw an error.
        /// Non-viable means a tile thats cannot be scheduled. For example a restriceted timeline for weekday but the tile is scheduled only during the weekend.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CustomErrors), "The timeline for tile is non viable check to see that you're scheduling the tile within a time frame that can contain tile")]
        public void TestCreationOfNonViableCalendarEventRestricted()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddDays(1);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition();
            var restrictionProfile = new RestrictionProfile(7, tilerUser.BeginningOfWeek, start, start.Add( duration + duration));
            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, restrictionProfile: restrictionProfile, now: Schedule.Now) as CalendarEventRestricted;
            Assert.IsFalse(testEvent.willAllTileFit, "Because this is a non viable tile the tiles should not be pinnable");
            Schedule.AddToSchedule(testEvent);
        }

        [TestMethod]
        public void TestCreationOfCalendarEventRestrictedWithLocation()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Location location = TestUtility.getAdHocLocations()[0];
            location.verify();
            Repetition repetition = new Repetition();
            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, location, restrictionProfile: new RestrictionProfile(start, duration + duration), now: Schedule.Now) as CalendarEventRestricted;
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            var mockContext = new TestDBContext();
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            CalendarEvent newlyaddedevent = waitVar.Result;
            Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
            Assert.IsTrue(testEvent.isTestEquivalent(newlyaddedevent));

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            CalendarEvent loadedFromSchedule = Schedule.getCalendarEvent(testEvent.Id);
            Assert.IsTrue(testEvent.isTestEquivalent(loadedFromSchedule));
            Assert.IsTrue(newlyaddedevent.isTestEquivalent(loadedFromSchedule));
        }

        [TestMethod]
        public void TestCreationOfWeekdayRepeatRigid()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(14));
            int numberOfDays = 5;
            List<DayOfWeek> weekDays = new List<DayOfWeek>();
            for(int index = (int)start.DayOfWeek, counter =0; counter < numberOfDays; index++, counter++)
            {
                int currentDayIndex = index % 7;
                DayOfWeek dayOfWeek = (DayOfWeek)currentDayIndex;
                weekDays.Add(dayOfWeek);
            }
            List<DayOfWeek> weekDaysAsInt = weekDays.Select(obj => (int)obj).Select(num => (DayOfWeek)num).ToList();
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, new TimeLine(start, end), weekDaysAsInt.ToArray());
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, true);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();

            var mockContext = new TestDBContext();
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);

            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            CalendarEvent verificationEventPulled = waitVar.Result;
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));
            List<SubCalendarEvent> subEvents = verificationEventPulled.AllSubEvents.OrderBy(subEvent => subEvent.Start).ToList();
            for(int index = 0; index < subEvents.Count; index++)
            {
                int currentDayIndex = index % weekDays.Count;
                DayOfWeek dayOfWeek = weekDays[currentDayIndex];
                Assert.AreEqual(subEvents[index].Start.DayOfWeek, dayOfWeek);
            }

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.All);
            CalendarEvent loadedFromSchedule = Schedule.getCalendarEvent(testEvent.Id);
            Assert.IsTrue(testEvent.isTestEquivalent(loadedFromSchedule));
            Assert.IsTrue(verificationEventPulled.isTestEquivalent(loadedFromSchedule));
        }

        [TestMethod]
        public void TestCreationOfWeekdayRepeatNonRigid()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            user.Login().Wait();
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            List<DayOfWeek> weekDays = new List<DayOfWeek>() { start.DayOfWeek, (DayOfWeek)(((int)start.DayOfWeek + 2)%7), (DayOfWeek)(((int)start.DayOfWeek + 4)%7)};
            List<DayOfWeek> weekDaysAsInt = weekDays.ToList();
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, new TimeLine(start, end), weekDaysAsInt.ToArray());
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, repetitionRange.Start, repetitionRange.End, 1, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();


            var checkingNull = testEvent.getRepeatedCalendarEvent(testEvent.ActiveSubEvents.First().SubEvent_ID.getAllEventDictionaryLookup);
            var all = testEvent.AllSubEvents;
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            CalendarEvent verificationEventPulled = waitVar.Result;
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));
            List<SubCalendarEvent> subEvents = verificationEventPulled.AllSubEvents.OrderBy(subEvent => subEvent.Start).ToList();
            for (int index = 0; index < subEvents.Count; index++)
            {
                int currentDayIndex = index % weekDays.Count;
                DayOfWeek dayOfWeek = weekDays[currentDayIndex];
                Assert.AreEqual(subEvents[index].Start.DayOfWeek, dayOfWeek);
            }


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.All);
            CalendarEvent loadedFromSchedule = Schedule.getCalendarEvent(testEvent.Id);
            Assert.IsTrue(testEvent.isTestEquivalent(loadedFromSchedule));
            Assert.IsTrue(verificationEventPulled.isTestEquivalent(loadedFromSchedule));
        }

        /// <summary>
        /// This test test to see that the sub events rendered on the front end are correlated on the front end.
        /// 
        /// It verifies the auto deleted subevents are not sent to the front end and even if they are included in the schedule manipulation set are not added to the schedule evaluation
        /// </summary>
        [TestMethod]
        public void TestCreationOfSubeventVerifyWebRetrievaMatchesEvaluationRetrieval()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            user.Login().Wait();
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            List<DayOfWeek> weekDays = new List<DayOfWeek>() { start.DayOfWeek, (DayOfWeek)(((int)start.DayOfWeek + 2) % 7), (DayOfWeek)(((int)start.DayOfWeek + 4) % 7) };
            List<DayOfWeek> weekDaysAsInt = weekDays.ToList();
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, new TimeLine(start, end), weekDaysAsInt.ToArray());


            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, repetitionRange.Start, repetitionRange.End, 1, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TimeLine rangeOfLookup = new TimeLine(refNow, testEvent.End);
            LogControl LogAccess = user.ScheduleLogControl;

            var task = LogAccess.getAllEnabledSubCalendarEvent(rangeOfLookup, Schedule.Now, retrievalOptions: DataRetrievalSet.UiSet);
            task.Wait();
            var allSubs = task.Result.ToList();

            var taskCal = LogAccess.getAllEnabledCalendarEvent(rangeOfLookup, Schedule.Now, retrievalOptions: DataRetrievalSet.UiSet);
            taskCal.Wait();
            var allCals = taskCal.Result.ToList();
            var calSubEVents = allCals.Select(obj => obj.Value).SelectMany(obj => obj.AllSubEvents).ToList();
            Assert.AreEqual(allSubs.Count, calSubEVents.Count);
            int subEventCount = 6;
            Assert.AreEqual(allSubs.Count, subEventCount);


            //This creates a repeition tile with and verifies that the subevent counts are valid with those sent to the front end
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            var restrictionProfile = new RestrictionProfile(start, duration + duration);
            repetition = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, new TimeLine(start, end), weekDaysAsInt.ToArray());
            CalendarEvent testEventRestriction = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, repetitionRange.Start, repetitionRange.End, 1, false, restrictionProfile: restrictionProfile, now: Schedule.Now);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEventRestriction).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            rangeOfLookup = new TimeLine(refNow, testEvent.End);
            LogAccess = user.ScheduleLogControl;

            task = LogAccess.getAllEnabledSubCalendarEvent(rangeOfLookup, Schedule.Now, retrievalOptions: DataRetrievalSet.UiSet);
            task.Wait();
            allSubs = task.Result.ToList();

            taskCal = LogAccess.getAllEnabledCalendarEvent(rangeOfLookup, Schedule.Now, retrievalOptions: DataRetrievalSet.scheduleManipulation);
            taskCal.Wait();
            allCals = taskCal.Result.ToList();
            calSubEVents = allCals.Select(obj => obj.Value).SelectMany(obj => obj.AllSubEvents).ToList();
            int repetitionSubEventCount = 6;
            Assert.AreEqual(allSubs.Count, repetitionSubEventCount + subEventCount);
            Assert.AreEqual(allSubs.Count, calSubEVents.Count);

            //This creates a restricte tile with and verifies that the subevent count schedulmanipualtionset matches those of the UI set
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            restrictionProfile = new RestrictionProfile(start, duration + duration);
            TimeLine repetitionActualRange = new TimeLine(start, end);
            repetition = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, repetitionActualRange);
            testEventRestriction = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, repetitionRange.Start, repetitionRange.End, 10, false, restrictionProfile: restrictionProfile, now: Schedule.Now);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(testEventRestriction).Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            rangeOfLookup = new TimeLine(refNow, testEvent.End);
            LogAccess = user.ScheduleLogControl;

            task = LogAccess.getAllEnabledSubCalendarEvent(rangeOfLookup, Schedule.Now, retrievalOptions: DataRetrievalSet.UiSet);
            task.Wait();
            allSubs = task.Result.ToList();

            taskCal = LogAccess.getAllEnabledCalendarEvent(rangeOfLookup, Schedule.Now, retrievalOptions: DataRetrievalSet.scheduleManipulation);
            taskCal.Wait();
            allCals = taskCal.Result.ToList();
            calSubEVents = allCals.Select(obj => obj.Value).SelectMany(obj => obj.AllSubEvents).ToList();
            var activeCalSubEVents = allCals.Select(obj => obj.Value).SelectMany(obj => obj.ActiveSubEvents).ToList();
            int repetitionWeeklySubEventCount = 11;
            int AutoDeletionCount = 9;
            Assert.AreEqual(AutoDeletionCount, testEventRestriction.Repeat.RecurringCalendarEvents().Sum(cal => cal.AutoDeletionCount));
            Assert.AreEqual(allSubs.Count, repetitionSubEventCount + subEventCount + repetitionWeeklySubEventCount);
            
            Assert.AreEqual(allSubs.Count + 9, calSubEVents.Count);
            Assert.AreEqual(allSubs.Count, activeCalSubEVents.Count);

        }


        [TestMethod]
        public void TestCreationOfDailyRepeatNonRigidIsLessThanADayEvent()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            TestSchedule Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.All);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            //List<DayOfWeek> weekDays = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday ,DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday};
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition(repetitionRange, Repetition.Frequency.DAILY, new TimeLine(start, end));
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, repetitionRange.Start, repetitionRange.End, 1, false);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            CalendarEvent newlyaddedevent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
            CalendarEvent newlyaddedevent0 = Schedule.getCalendarEvent(newlyaddedevent.ActiveSubEvents.First().SubEvent_ID.getAllEventDictionaryLookup);
            List<SubCalendarEvent> subEvents = newlyaddedevent0.AllSubEvents.OrderBy(subEvent => subEvent.Start).ToList();
            int currentDayIndex = (int)repetitionRange.Start.DayOfWeek;
            for (int index = 0; index < subEvents.Count; index++)
            {
                DayOfWeek dayOfWeek = (DayOfWeek)currentDayIndex;
                Assert.AreEqual(subEvents[index].Start.DayOfWeek, dayOfWeek);
                currentDayIndex += 1;
                currentDayIndex %= 7;
            }
            Assert.AreEqual(newlyaddedevent.Calendar_EventID.getCalendarEventComponent(), newlyaddedevent0.Calendar_EventID.getCalendarEventComponent());
        }

        /// <summary>
        /// Test creates a scenario where the origin calendar event has timeline that is wider than a day, but less than the repetition range
        /// </summary>
        [TestMethod]
        public void TestCreationOfDailyRepeatNonRigidCalendarEventSpanLong()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            //List<DayOfWeek> weekDays = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday ,DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday};
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition(repetitionRange, Repetition.Frequency.DAILY, repetitionRange.CreateCopy());
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            CalendarEvent newlyaddedevent = TestUtility.getCalendarEventById(testEvent.Calendar_EventID, user);
            Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
            HashSet<string> calendarIds = new HashSet<string>() { newlyaddedevent.Id };
            Schedule = new TestSchedule(user, refNow, calendarIds: calendarIds);
            CalendarEvent newlyaddedevent0 = Schedule.getCalendarEvent(newlyaddedevent.Calendar_EventID.getAllEventDictionaryLookup);
            List<SubCalendarEvent> subEvents = newlyaddedevent0.AllSubEvents.OrderBy(subEvent => subEvent.Start).ToList();
            int currentDayIndex = (int)repetitionRange.Start.DayOfWeek;
            for (int index = 0; index < subEvents.Count; index++)
            {
                DayOfWeek dayOfWeek = (DayOfWeek)currentDayIndex;
                Assert.AreEqual(subEvents[index].Start.DayOfWeek, dayOfWeek);
                currentDayIndex += 1;
                currentDayIndex %= 7;
            }
            Assert.AreEqual(newlyaddedevent.Calendar_EventID.getCalendarEventComponent(), newlyaddedevent0.Calendar_EventID.getCalendarEventComponent());
        }

        /// <summary>
        /// Test creates a scenario where the origin calendar event has timeline that is wider than a day, but less than the repetition range
        /// </summary>
        [TestMethod]
        public void TestCreationOfDailyRepeatNonRigidCalendarEventRestrictedSpanLong()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            //List<DayOfWeek> weekDays = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday ,DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday};
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Location location = TestUtility.getAdHocLocations()[0];
            location.verify();
            List<DayOfWeek> weekDays = new List<DayOfWeek>() { start.DayOfWeek, (DayOfWeek)(((int)start.DayOfWeek + 2) % 7), (DayOfWeek)(((int)start.DayOfWeek + 4) % 7) };
            //List<DayOfWeek> weekDays = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday ,DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday};
            List<DayOfWeek> weekDaysAsInt = weekDays.ToList();
            Repetition repetition = new Repetition(repetitionRange, Repetition.Frequency.DAILY, repetitionRange.CreateCopy(), weekDaysAsInt.ToArray());
            int splitCount = 1;
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, splitCount, false, location, restrictionProfile: new RestrictionProfile(start, duration + duration), now: Schedule.Now);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            CalendarEvent newlyaddedevent = waitVar.Result;
            Assert.IsTrue(newlyaddedevent.NumberOfSplit == splitCount);
            Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
            string repatCalEventId = newlyaddedevent.Calendar_EventID.getIDUpToRepeatCalendarEvent();

            List <SubCalendarEvent> subEvents = newlyaddedevent.AllSubEvents.OrderBy(subEvent => subEvent.Start).ToList();

            Assert.IsTrue(testEvent.isTestEquivalent(newlyaddedevent));
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithRepeat);
            CalendarEvent calendarEventFromSchedule = Schedule.getCalendarEvent(testEvent.Id);
            Assert.IsNotNull(calendarEventFromSchedule);
            //Assert.IsTrue(testEvent.isTestEquivalent(calendarEventFromSchedule));
            int currentDayIndex = (int)repetitionRange.Start.DayOfWeek;
            for (int index = 0; index < subEvents.Count; index++)
            {
                DayOfWeek dayOfWeek = (DayOfWeek)currentDayIndex;
                Assert.AreEqual(subEvents[index].Start.DayOfWeek, dayOfWeek);
                currentDayIndex += 1;
                currentDayIndex %= 7;
            }
        }

        /// <summary>
        /// Test creates a scenario where the origin calendar event has timeline that is wider than a day, but less than the repetition range
        /// </summary>
        [TestMethod]
        public void TestWeekdayRestrictionsCreationOfDailyRepeatCalendarEventRestrictedSpanLong()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/3/2017");
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(14));
            //List<DayOfWeek> weekDays = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday ,DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday};
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Location location = TestUtility.getAdHocLocations()[0];
            location.verify();
            //List<DayOfWeek> weekDays = new List<DayOfWeek>() { start.DayOfWeek, (DayOfWeek)(((int)start.DayOfWeek + 2) % 7), (DayOfWeek)(((int)start.DayOfWeek + 4) % 7) };
            ////List<DayOfWeek> weekDays = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday ,DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday};
            //List<int> weekDaysAsInt = weekDays.Select(obj => (int)obj).ToList();
            Repetition repetition = new Repetition(repetitionRange, Repetition.Frequency.DAILY, repetitionRange.CreateCopy());
            int subEventCount = 3;
            var restrictionProfile = new RestrictionProfile(1, DayOfWeek.Monday, start, start.AddHours(8));
            int numberOfWeeks = (repetitionRange.TimelineSpan.Days / 7);
            int totalNumberOfEvents = restrictionProfile.NoNull_DaySelections.Count * subEventCount* numberOfWeeks;

            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, subEventCount, false, location, restrictionProfile: restrictionProfile, now: Schedule.Now);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            Assert.AreEqual(totalNumberOfEvents, testEvent.AllSubEvents.Count());
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            CalendarEvent newlyaddedevent = waitVar.Result;
            Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
            string repatCalEventId = newlyaddedevent.Calendar_EventID.getIDUpToRepeatCalendarEvent();

            List<SubCalendarEvent> subEvents = newlyaddedevent.AllSubEvents.OrderBy(subEvent => subEvent.Start).ToList();

            Assert.IsTrue(testEvent.isTestEquivalent(newlyaddedevent));
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithRepeat);
            CalendarEvent calendarEventFromSchedule = Schedule.getCalendarEvent(testEvent.Id);
            Assert.IsNotNull(calendarEventFromSchedule);
            //Assert.IsTrue(testEvent.isTestEquivalent(calendarEventFromSchedule));
            int currentDayIndex = (int)repetitionRange.Start.DayOfWeek;
            List<RestrictionDay> daySelections = restrictionProfile.NoNull_DaySelections as List<RestrictionDay>;
            for (int index = 0; index < subEvents.Count; index++)
            {
                for(int j= 0; j < restrictionProfile.NoNull_DaySelections.Count; j++)
                {
                    DayOfWeek dayOfWeek = daySelections[j].WeekDay;
                    for (int i = 0; i < subEventCount; i++, index++)
                    {

                        Assert.AreEqual(subEvents[index].Start.DayOfWeek, dayOfWeek);

                    }
                }
                --index;
            }
        }

        /// <summary>
        /// Test creates a scenario where the origin calendar event has timeline that is wider than a day, and greater than repeat range
        /// </summary>
        [TestMethod]
        public void TestCreationCalendarEvent ()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(21);
            TimeLine repetitionRange = new TimeLine(start, end.AddDays(-5).AddHours(-23));
            //List<DayOfWeek> weekDays = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday ,DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday};
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, repetitionRange.CreateCopy());
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser,duration, repetition, start, end, 1, false);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            CalendarEvent newlyaddedevent = waitVar.Result;
            Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
            
            List<SubCalendarEvent> subEvents = newlyaddedevent.AllSubEvents.OrderBy(subEvent => subEvent.Start).ToList();
            DayOfWeek dayOfWeek = repetitionRange.Start.DayOfWeek;
            for (int index = 0; index < subEvents.Count; index++)
            {
                var subEvent = subEvents[index];
                Assert.IsTrue(subEvent.End < repetitionRange.End);
            }
        }

        [TestMethod]
        public void testPersistedCalendarEvent()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TestSchedule schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, true);
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            schedule = null;
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);
            schedule = new TestSchedule(user, refNow);
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            CalendarEvent newlyaddedevent = waitVar.Result;
            Assert.IsTrue(newlyaddedevent.isTestEquivalent(testEvent));

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            CalendarEvent loadedFromSchedule = Schedule.getCalendarEvent(testEvent.Id);
            Assert.IsTrue(testEvent.isTestEquivalent(loadedFromSchedule));
            Assert.IsTrue(newlyaddedevent.isTestEquivalent(loadedFromSchedule));
        }

        [TestMethod]
        public void testPersistedSubCalendarEvent()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            TestSchedule schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, true);
            string currentID = testEvent.getId;
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            CalendarEvent tempEvent = schedule.getCalendarEvent(testEvent.Calendar_EventID);
            Schedule tempSchedule = schedule;
            schedule = null;
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);

            var mockContext = new TestDBContext();
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);

            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            CalendarEvent newlyaddedevent = waitVar.Result;
            Assert.IsTrue(newlyaddedevent.isTestEquivalent(testEvent));

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            CalendarEvent loadedFromSchedule = Schedule.getCalendarEvent(testEvent.Id);
            Assert.IsTrue(testEvent.isTestEquivalent(loadedFromSchedule));
            Assert.IsTrue(newlyaddedevent.isTestEquivalent(loadedFromSchedule));
            foreach (SubCalendarEvent eachSubCalendarEvent in newlyaddedevent.AllSubEvents)
            {
                SubCalendarEvent tempSubevent =  testEvent.getSubEvent(eachSubCalendarEvent.SubEvent_ID);
                Assert.IsTrue(tempSubevent.isTestEquivalent(eachSubCalendarEvent));
            }
        }

        [TestMethod]
        public void testChangeOfNameOfEvent()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, refNow.Day, refNow.Hour, refNow.Minute, refNow.Second, new TimeSpan());
            TestSchedule schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);//.Add(duration).Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, true);
            EventName oldName = testEvent.getName;
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            schedule = new TestSchedule(user, refNow);
            CalendarEvent copyOfTestEvent = schedule.getCalendarEvent(testEvent.getId);
            string newName = "test-Event-For-stack-" + Guid.NewGuid().ToString();
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> tupleResult = schedule.BundleChangeUpdate(testEvent.ActiveSubEvents.First().getId, 
                new EventName(user.getTilerUser(), copyOfTestEvent, newName), 
                testEvent.ActiveSubEvents.First().Start, 
                testEvent.ActiveSubEvents.First().End, 
                testEvent.ActiveSubEvents.First().Start, 
                testEvent.ActiveSubEvents.First().End, 
                testEvent.NumberOfSplit, testEvent.Notes.UserNote);
            schedule.persistToDB().Wait();
            var mockContext = new TestDBContext();
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);

            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            CalendarEvent renamedEvent = waitVar.Result;

            Assert.AreEqual(renamedEvent.getName.NameValue, newName);
            Assert.AreEqual(renamedEvent.ActiveSubEvents.First().getName.NameValue, newName);
            Assert.AreEqual(renamedEvent.getName.NameId, testEvent.getName.NameId);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            CalendarEvent loadedFromSchedule = Schedule.getCalendarEvent(testEvent.Id);
            Assert.IsTrue(testEvent.isTestEquivalent(loadedFromSchedule));
            Assert.IsTrue(renamedEvent.isTestEquivalent(loadedFromSchedule));
        }

        [TestMethod]
        public void testChangeOfNotesOfEvent()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, refNow.Day, refNow.Hour, refNow.Minute, refNow.Second, new TimeSpan());
            TestSchedule schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);//.Add(duration).Add(duration);
            string oldNoteName = "test initial note";
            string newNoteName = "test change note";
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, true, note: new MiscData(oldNoteName));
            schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            schedule = new TestSchedule(user, refNow);
            CalendarEvent copyOfTestEvent = schedule.getCalendarEvent(testEvent.getId);
            
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> tupleResult = schedule.BundleChangeUpdate(testEvent.ActiveSubEvents.First().getId,
                testEvent.getName?.createCopy(),
                testEvent.ActiveSubEvents.First().Start,
                testEvent.ActiveSubEvents.First().End,
                testEvent.ActiveSubEvents.First().Start,
                testEvent.ActiveSubEvents.First().End,
                testEvent.NumberOfSplit, newNoteName);
            schedule.persistToDB().Wait();

            var mockContext = new TestDBContext();
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);

            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            CalendarEvent renamedEvent = waitVar.Result;

            Assert.AreEqual(renamedEvent.Notes.UserNote, newNoteName);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            CalendarEvent loadedFromSchedule = Schedule.getCalendarEvent(testEvent.Id);
            Assert.IsTrue(testEvent.isTestEquivalent(loadedFromSchedule));
            Assert.IsTrue(renamedEvent.isTestEquivalent(loadedFromSchedule));
        }

        /// <summary>
        /// UTC TimeZone Required
        /// </summary>
        [TestMethod]
        public void thirdPartyScheduling ()
        {
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = new TimeLine(refNow, refNow.AddHours(2));
            TilerColor tilerColor = new TilerColor(255, 255, 0, 1, 5);
            EventDisplay eventdisplay = new EventDisplay(true, tilerColor);
            int count = 10;
            List<Event> googleEvents = new List<Event>();
            List<CalendarEvent> originCalEventForGoogleEvent = new List<CalendarEvent>();

            for( int i= 0; i<count; i++ )
            {
                CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start.AddHours(i), timeLine.End.AddHours(i), 1, false, eventDisplay: eventdisplay);
                originCalEventForGoogleEvent.Add(testEvent);
                EventDateTime start = new EventDateTime();
                start.DateTime = timeLine.Start.DateTime;
                EventDateTime end = new EventDateTime();
                end.DateTime = timeLine.End.DateTime;

                TestGoogleEvent googleEvent = new TestGoogleEvent();
                googleEvent.Start = start;
                googleEvent.End = end;
                googleEvent.Summary = testEvent.Name.Name;
                googleEvent.Location = testEvent.Location.Address;
                googleEvent.Organizer = new Event.OrganizerData();
                googleEvents.Add(googleEvent);
            }
            googleEvents = googleEvents.OrderBy(obj => obj.Start.DateTime).ToList();



            TimeLine tilerEventTimeLine = new TimeLine(timeLine.Start, timeLine.Start.AddDays(1));
            CalendarEvent nonGoogleEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), tilerEventTimeLine.Start, tilerEventTimeLine.End, 1, false);
            EventID googleAuthenticationID = EventID.generateGoogleAuthenticationID(0.ToString());

            Task<IEnumerable<CalendarEvent>> googleCalWait = GoogleCalExtension.getAllCalEvents(googleEvents, null, tilerUser.Id, googleAuthenticationID, new TimeLine(refNow.AddDays(-15), refNow.AddDays(15)), false);
            googleCalWait.Wait();
            List<CalendarEvent> allCaleEvents = googleCalWait.Result.ToList();

            ThirdPartyCalendarEvent thirdPartyCalendarEvent = new GoogleCalendarEvent(allCaleEvents, tilerUser);
            Schedule.updateDataSetWithThirdPartyData(new Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>(ThirdPartyControl.CalendarTool.google, new List<CalendarEvent>() { thirdPartyCalendarEvent }));
            Schedule.AddToSchedule(nonGoogleEvent);

            //user = TestUtility.getTestUser(userId: tilerUser.Id);
            //tilerUser = user.getTilerUser();
            //user.Login().Wait();
            //Schedule = new TestSchedule(user, refNow);
            Task<ScheduleDump> getDumpTask = Schedule.CreateScheduleDump();
            getDumpTask.Wait();
            ScheduleDump scheduleDump = getDumpTask.Result;

            Schedule scheduleFromDump = new TestSchedule(scheduleDump, user);

            List<SubCalendarEvent> googleCalendarEvents = scheduleFromDump.getGoogleCalendarEvents().First().AllSubEvents.OrderBy(subEvent => subEvent.Start).ToList();

            for(int i=0; i< googleCalendarEvents.Count; i++ )
            {
                TilerEvent googleCalEvent = googleCalendarEvents[i];
                Event googleEvent = googleEvents[i];
                DateTimeOffset start = googleCalEvent.Start.removeSecondsAndMilliseconds();
                DateTimeOffset end = googleCalEvent.End.removeSecondsAndMilliseconds();
                Assert.IsTrue(start == googleEvent.Start.DateTime);
                Assert.IsTrue(end == googleEvent.End.DateTime);
            }

        }

        /// <summary>
        /// UTC TimeZone Required third party location
        /// </summary>
        [TestMethod]
        public void thirdPartySchedulingWithDefaultLocation()
        {
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = new TimeLine(refNow, refNow.AddHours(2));
            TilerColor tilerColor = new TilerColor(255, 255, 0, 1, 5);
            EventDisplay eventdisplay = new EventDisplay(true, tilerColor);
            int count = 10;
            List<Event> googleEvents = new List<Event>();
            List<CalendarEvent> originCalEventForGoogleEvent = new List<CalendarEvent>();

            for (int i = 0; i < count; i++)
            {
                CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start.AddHours(i), timeLine.End.AddHours(i), 1, false, eventDisplay: eventdisplay);
                originCalEventForGoogleEvent.Add(testEvent);
                EventDateTime start = new EventDateTime();
                start.DateTime = timeLine.Start.DateTime;
                EventDateTime end = new EventDateTime();
                end.DateTime = timeLine.End.DateTime;

                TestGoogleEvent googleEvent = new TestGoogleEvent();
                googleEvent.Start = start;
                googleEvent.End = end;
                googleEvent.Summary = testEvent.Name.Name;
                googleEvent.Location = testEvent.Location.Address;
                googleEvent.Organizer = new Event.OrganizerData();
                googleEvents.Add(googleEvent);
            }
            googleEvents = googleEvents.OrderBy(obj => obj.Start.DateTime).ToList();



            TimeLine tilerEventTimeLine = new TimeLine(timeLine.Start, timeLine.Start.AddDays(1));
            CalendarEvent nonGoogleEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), tilerEventTimeLine.Start, tilerEventTimeLine.End, 1, false);
            EventID googleAuthenticationID = EventID.generateGoogleAuthenticationID(0.ToString());
            TestUtility.enableInterconnectionCheck();
            IEnumerable<Location> locations = TestUtility.generateAdHocLocations(user);
            Location workLocation = locations.Where(o => o.Description.ToLower().Contains("work")).Single();

            Task<IEnumerable<CalendarEvent>> googleCalWait = GoogleCalExtension.getAllCalEvents(googleEvents, null, tilerUser.Id, googleAuthenticationID, new TimeLine(refNow.AddDays(-15), refNow.AddDays(15)), false, workLocation);
            googleCalWait.Wait();
            List<CalendarEvent> allCaleEvents = googleCalWait.Result.ToList();

            ThirdPartyCalendarEvent thirdPartyCalendarEvent = new GoogleCalendarEvent(allCaleEvents, tilerUser);
            foreach(var eachCalEvent in allCaleEvents)
            {
                workLocation.isTestEquivalent(eachCalEvent.Location);
            }

            Schedule.updateDataSetWithThirdPartyData(new Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>(ThirdPartyControl.CalendarTool.google, new List<CalendarEvent>() { thirdPartyCalendarEvent }));
            Schedule.AddToSchedule(nonGoogleEvent);

            //user = TestUtility.getTestUser(userId: tilerUser.Id);
            //tilerUser = user.getTilerUser();
            //user.Login().Wait();
            //Schedule = new TestSchedule(user, refNow);
            Task<ScheduleDump> getDumpTask = Schedule.CreateScheduleDump();
            getDumpTask.Wait();
            ScheduleDump scheduleDump = getDumpTask.Result;

            Schedule scheduleFromDump = new TestSchedule(scheduleDump, user);

            List<SubCalendarEvent> googleCalendarEvents = scheduleFromDump.getGoogleCalendarEvents().First().AllSubEvents.OrderBy(subEvent => subEvent.Start).ToList();

            for (int i = 0; i < googleCalendarEvents.Count; i++)
            {
                TilerEvent googleCalEvent = googleCalendarEvents[i];
                Event googleEvent = googleEvents[i];
                DateTimeOffset start = googleCalEvent.Start.removeSecondsAndMilliseconds();
                DateTimeOffset end = googleCalEvent.End.removeSecondsAndMilliseconds();
                Assert.IsTrue(start == googleEvent.Start.DateTime);
                Assert.IsTrue(end == googleEvent.End.DateTime);
            }

        }



        /// <summary>
        /// Test verifies that schedule can be operated on even though full calendar event isn't loaded into memmory
        /// </summary>
        [TestMethod]
        public void testOutOfRangeOfScheduleLoad ()
        {
            DB_Schedule Schedule;
            int splitCount = 8;
            DateTimeOffset refNow = TestUtility.parseAsUTC("7/7/2019 12:00:00 AM");
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, 1, 0, 0, 0, new TimeSpan());
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            refNow = refNow.removeSecondsAndMilliseconds();
            TimeSpan duration = TimeSpan.FromHours(2);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddDays(28);

            TimeLine repeatTimeLine = new TimeLine(start, end.AddDays(200));
            TimeLine calTimeLine = repeatTimeLine.CreateCopy();
            Repetition repetition = new Repetition(repeatTimeLine, Repetition.Frequency.WEEKLY, calTimeLine);
            int repeatSplitCount = 2;
            CalendarEvent repeatEvent = TestUtility
                .generateCalendarEvent(tilerUser, duration, repetition, calTimeLine.Start, calTimeLine.End, repeatSplitCount, false);
            int initialCount = repeatEvent.AllSubEvents.Count();
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(repeatEvent).Wait();
            const int dayDelta = 7;
            TimeLine lookupWindow = new TimeLine(refNow.AddDays(-dayDelta), refNow.AddDays(dayDelta*2));

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, rangeOfLookup: lookupWindow);
            Location location = TestUtility.getAdHocLocations()[0];
            Schedule.FindMeSomethingToDo(location).Wait();
            Schedule.persistToDB().Wait();
            IEnumerable<CalendarEvent> calEvents = Schedule.getAllRelatedCalendarEvents(repeatEvent.Calendar_EventID);
            Assert.IsTrue(calEvents.SelectMany(obj=> obj.ActiveSubEvents).Where(eachSubEvent => lookupWindow.doesTimeLineInterfere(eachSubEvent)).Count() == 5);// this is 5 because of we ignore the subevents outside the range

            UserAccount userAcc = TestUtility.getTestUser(userId: tilerUser.Id);
            Task<CalendarEvent> waitVar = userAcc.ScheduleLogControl.getCalendarEventWithID(repeatEvent.Id);
            waitVar.Wait();
            CalendarEvent verificationEventPulled = waitVar.Result;
            Assert.IsTrue(verificationEventPulled.ActiveSubEvents.Count() == initialCount);


            lookupWindow = new TimeLine(refNow, repeatTimeLine.End);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, rangeOfLookup: lookupWindow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);


            foreach (CalendarEvent calEvent in verificationEventPulled.RecurringCalendarEvents)
            {
                CalendarEvent repeatFromSchedule = Schedule.getCalendarEvent(calEvent.Id);
                Assert.IsTrue(repeatFromSchedule.isTestEquivalent(calEvent));
            }

            
        }


        /// <summary>
        /// Test verifies the creation of weekly events  have the right aportion of sub events for the initial week which is depenednt on the setting for the beginning of the week
        /// </summary>
        [TestMethod]
        public void ValidateRepeatWeekly()
        {
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/7/2017");// this should be a thursday
            DayOfWeek userWeekDay = DayOfWeek.Thursday;
            ReferenceNow now = new ReferenceNow(refNow, refNow, new TimeSpan());
            const int splitCount = 16;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            tilerUser.BeginningOfWeek = userWeekDay;
            TestSchedule Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(21);
            TimeLine repetitionRange = new TimeLine(start, end);
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition(repetitionRange, Repetition.Frequency.WEEKLY, repetitionRange.CreateCopy());
            CalendarEvent testEvent= TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, splitCount, false);
            Schedule.AddToScheduleAndCommitAsync(testEvent).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent testEventRetrieved = TestUtility.getCalendarEventById(testEvent.Id, user);
            List<CalendarEvent> calEvents = testEventRetrieved.Repeat.RecurringCalendarEvents().OrderBy(o => o.Start).ToList();
            CalendarEvent firstRepeatingCalEvent = calEvents[0];
            CalendarEvent secondRepeatingCalEvent = calEvents[1];
            DayOfWeek previousWeekDay = (DayOfWeek)((((int)tilerUser.BeginningOfWeek - 1)+7)%7);
            Assert.AreEqual(tilerUser.BeginningOfWeek, DayOfWeek.Thursday);
            Assert.AreEqual(previousWeekDay, firstRepeatingCalEvent.End.DayOfWeek, "The First recurring calendar event should end the day before the beginning of the week for registered user, because the next recurring event should continue on the beginning of the week for the user");
            Assert.AreEqual(tilerUser.BeginningOfWeek, secondRepeatingCalEvent.Start.DayOfWeek);
            int sumOfAutoDeletion = calEvents.Sum(cal => cal.AutoDeletionCount);
            Assert.AreEqual(0, sumOfAutoDeletion);


            //////// Does same test but for bi-weekly
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            userWeekDay = DayOfWeek.Friday;
            tilerUser.BeginningOfWeek = userWeekDay;
            DateTimeOffset biWeeklyEnd = start.AddDays(48);
            TimeLine biWeeklyRepetitionRange = new TimeLine(start, biWeeklyEnd);
            Repetition biWeeklyRepetition = new Repetition(biWeeklyRepetitionRange, Repetition.Frequency.BIWEEKLY, biWeeklyRepetitionRange.CreateCopy());
            CalendarEvent biWeeklyTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, biWeeklyRepetition, start, biWeeklyEnd, splitCount, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(biWeeklyTestEvent).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent biWeeklyTestEventRetrieved = TestUtility.getCalendarEventById(biWeeklyTestEvent.Id, user);
            List<CalendarEvent> biWeeklyCalEvents = biWeeklyTestEventRetrieved.Repeat.RecurringCalendarEvents().OrderBy(o => o.Start).ToList();
            CalendarEvent biWeeklyFirstRepeatingCalEvent = biWeeklyCalEvents[0];
            CalendarEvent biWeeklySecondRepeatingCalEvent = biWeeklyCalEvents[1];
            DayOfWeek biWeeklyPreviousWeekDay = (DayOfWeek)((((int)tilerUser.BeginningOfWeek - 1) + 7) % 7);
            Assert.AreEqual(biWeeklyPreviousWeekDay, biWeeklyFirstRepeatingCalEvent.End.DayOfWeek);
            Assert.AreEqual(tilerUser.BeginningOfWeek, biWeeklySecondRepeatingCalEvent.Start.DayOfWeek);
            Assert.AreEqual(tilerUser.BeginningOfWeek, userWeekDay);
            var biweeklyWithAutoDeletions = biWeeklyCalEvents.Where(cal => cal.AutoDeletionCount > 0);
            Assert.AreEqual(biweeklyWithAutoDeletions.First().ActiveSubEvents.First().AutoDeletion_Reason, AutoDeletion.RepetitionSpanTooSmall);
            int sumOfBiweeklyAutoDeletion = biweeklyWithAutoDeletions.Sum(cal => cal.AutoDeletionCount);
            Assert.AreEqual(15, sumOfBiweeklyAutoDeletion);


            //////// Does same test but for RestrictedProfile
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            userWeekDay = DayOfWeek.Thursday;
            tilerUser.BeginningOfWeek = userWeekDay;
            DateTimeOffset restrictedEnd = start.AddDays(48);
            TimeLine restrictedRepetitionRange = new TimeLine(start, restrictedEnd);
            RestrictionProfile restrictionProfile = new RestrictionProfile(start, duration + duration);
            Repetition restrictedRepetition = new Repetition(restrictedRepetitionRange, Repetition.Frequency.WEEKLY, restrictedRepetitionRange.CreateCopy());
            CalendarEvent restrictedTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, restrictedRepetition, start, restrictedEnd, splitCount, false, restrictionProfile: restrictionProfile, now: Schedule.Now);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(restrictedTestEvent).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent restrictedTestEventRetrieved = TestUtility.getCalendarEventById(restrictedTestEvent.Id, user);
            List<CalendarEvent> restrictedCalEvents = restrictedTestEventRetrieved.Repeat.RecurringCalendarEvents().OrderBy(o => o.Start).ToList();
            CalendarEvent restrictedFirstRepeatingCalEvent = restrictedCalEvents[0];
            CalendarEvent restrictedSecondRepeatingCalEvent = restrictedCalEvents[1];
            DayOfWeek restrictedPreviousWeekDay = (DayOfWeek)((((int)tilerUser.BeginningOfWeek - 1) + 7) % 7);
            Assert.AreEqual(restrictedPreviousWeekDay, restrictedFirstRepeatingCalEvent.End.DayOfWeek);
            Assert.AreEqual(tilerUser.BeginningOfWeek, restrictedSecondRepeatingCalEvent.Start.DayOfWeek);
            Assert.AreEqual(tilerUser.BeginningOfWeek, userWeekDay);

            var restrictedWithAutoDeletions = restrictedCalEvents.Where(cal => cal.AutoDeletionCount > 0);
            int sumOfrestrictedAutoDeletion = restrictedWithAutoDeletions.Sum(cal => cal.AutoDeletionCount);
            Assert.AreEqual(0, sumOfrestrictedAutoDeletion);


            //////// Does same test but for restrictedBiweeklyProfile
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            userWeekDay = DayOfWeek.Friday;
            tilerUser.BeginningOfWeek = userWeekDay;
            DateTimeOffset restrictedBiweeklyEnd = start.AddDays(48);
            TimeLine restrictedBiweeklyRepetitionRange = new TimeLine(start, restrictedBiweeklyEnd);
            RestrictionProfile restrictionBiweeklyProfile = new RestrictionProfile(start, duration + duration);
            Repetition restrictedBiweeklyRepetition = new Repetition(restrictedBiweeklyRepetitionRange, Repetition.Frequency.BIWEEKLY, restrictedBiweeklyRepetitionRange.CreateCopy());
            CalendarEvent restrictedBiweeklyTestEvent = TestUtility.generateCalendarEvent(tilerUser, duration, restrictedBiweeklyRepetition, start, restrictedBiweeklyEnd, splitCount, false, restrictionProfile: restrictionBiweeklyProfile, now: Schedule.Now);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommitAsync(restrictedBiweeklyTestEvent).Wait();
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent restrictedBiweeklyTestEventRetrieved = TestUtility.getCalendarEventById(restrictedBiweeklyTestEvent.Id, user);
            List<CalendarEvent> restrictedBiweeklyCalEvents = restrictedBiweeklyTestEventRetrieved.Repeat.RecurringCalendarEvents().OrderBy(o => o.Start).ToList();
            CalendarEvent restrictedBiweeklyFirstRepeatingCalEvent = restrictedBiweeklyCalEvents[0];
            CalendarEvent restrictedBiweeklySecondRepeatingCalEvent = restrictedBiweeklyCalEvents[1];
            DayOfWeek restrictedBiweeklyPreviousWeekDay = (DayOfWeek)((((int)tilerUser.BeginningOfWeek - 1) + 7) % 7);
            Assert.AreEqual(restrictedBiweeklyPreviousWeekDay, restrictedBiweeklyFirstRepeatingCalEvent.End.DayOfWeek);
            Assert.AreEqual(tilerUser.BeginningOfWeek, restrictedBiweeklySecondRepeatingCalEvent.Start.DayOfWeek);
            Assert.AreEqual(tilerUser.BeginningOfWeek, userWeekDay);

            var restrictedBiweeklyWithAutoDeletions = restrictedBiweeklyCalEvents.Where(cal => cal.AutoDeletionCount > 0);
            Assert.AreEqual(restrictedBiweeklyWithAutoDeletions.First().ActiveSubEvents.First().AutoDeletion_Reason, AutoDeletion.RepetitionSpanTooSmall);
            int sumOfrestrictedBiweeklyAutoDeletion = restrictedBiweeklyWithAutoDeletions.Sum(cal => cal.AutoDeletionCount);
            Assert.AreEqual(15, sumOfrestrictedBiweeklyAutoDeletion);


            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow, retrievalOptions: DataRetrievalSet.scheduleManipulationWithUpdateHistory);
            Task<ScheduleDump> taskScheduleDump = Schedule.CreateScheduleDump(Schedule.Now);
            taskScheduleDump.Wait();
            ScheduleDump scheduleDump = taskScheduleDump.Result;
            TestSchedule scheduleDumpSchedule = new TestSchedule(scheduleDump, user);
            TestUtility.isTestEquivalent(Schedule, scheduleDumpSchedule);

        }


        /// <summary>
        /// Test verifies the persisting of travel time
        /// </summary>
        [TestMethod]
        public async Task TravelTime()
        {
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/7/2017");// this should be a thursday
            DayOfWeek userWeekDay = DayOfWeek.Thursday;
            ReferenceNow now = new ReferenceNow(refNow, refNow, new TimeSpan());
            const int splitCount = 1;
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();


            List<Location> adHocLocations = TestUtility.getAdHocLocations(tilerUser.Id);
            List<Location> persistedLocations = TestUtility.addLocations(user, tilerUser, adHocLocations).ToList();

            Location workLocation = persistedLocations.Single(Location => Location.Description == "Work");
            Location homeLocation = persistedLocations.Single(Location => Location.Description == "Home");
            Location gymLocation = persistedLocations.Single(Location => Location.Description == "Gym");

            //adHocLocations

            Location currentLocation = gymLocation;
            TimeSpan gym_work =  Location.getDrivingTimeFromWeb(gymLocation, workLocation);
            TimeSpan home_work = Location.getDrivingTimeFromWeb(homeLocation, workLocation);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            tilerUser.BeginningOfWeek = userWeekDay;
            TestSchedule Schedule = new TestSchedule(user, refNow);
            Schedule.CurrentLocation = currentLocation;
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(1);
            TimeLine repetitionRange = new TimeLine(start, end);
            DayOfWeek startingWeekDay = start.DayOfWeek;
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, splitCount, false, gymLocation);
            Schedule.AddToSchedule(testEvent);
            testEvent.LocationId = gymLocation.Id;
            testEvent.Location_DB = null;
            foreach(SubCalendarEvent eachSubEvent in testEvent.AllSubEvents)
            {
                eachSubEvent.LocationId = gymLocation.Id;
                eachSubEvent.Location_DB = null;
            }
            await Schedule.WriteFullScheduleToLog(testEvent).ConfigureAwait(false);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent testEventRetrieved = TestUtility.getCalendarEventById(testEvent.Id, user);
            SubCalendarEvent subEvent = testEventRetrieved.ActiveSubEvents.First();
            TimeSpan oneMinSpan = TimeSpan.FromMinutes(1);
            Assert.IsTrue(subEvent.TravelTimeBefore < oneMinSpan, "subevent travel time should be less than one minute since the current location is gym location");

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            Schedule = new TestSchedule(user, refNow);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, splitCount, false, workLocation);
            Schedule.AddToSchedule(testEvent1);
            testEvent1.LocationId = workLocation.Id;
            testEvent1.Location_DB = null;
            foreach (SubCalendarEvent eachSubEvent in testEvent1.AllSubEvents)
            {
                eachSubEvent.LocationId = workLocation.Id;
                eachSubEvent.Location_DB = null;
            }
            await Schedule.WriteFullScheduleToLog(testEvent1).ConfigureAwait(false);
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            CalendarEvent testEvent1Retrieved = TestUtility.getCalendarEventById(testEvent1.Id, user);
            SubCalendarEvent subEvent1 = testEvent1Retrieved.ActiveSubEvents.First();


            List<SubCalendarEvent> allSubEvents = Schedule.getAllActiveSubEvents().OrderBy(o=>o.Start).ToList();
            SubCalendarEvent firstSubEvent = allSubEvents.First();
            SubCalendarEvent secondSubEvent = allSubEvents.Last();

            Assert.IsTrue(firstSubEvent.TravelTimeBefore < oneMinSpan, "subevent travel time should be less than one minute since the current location is gym location, so the first sub event should be the one closes to gym");
            Assert.AreEqual(firstSubEvent.TravelTimeAfter, secondSubEvent.TravelTimeBefore, "first subevent travel after time should be the same as sub event travel time before");
        }




        [TestMethod]
        public void VerifyDayPreferenceOrder()
        {
            Assert.AreEqual(TimeOfDayPreferrence.ActiveDaySections.Count, 4);
            Assert.AreEqual(TimeOfDayPreferrence.ActiveDaySections[0], TimeOfDayPreferrence.DaySection.Sleep);
            Assert.AreEqual(TimeOfDayPreferrence.ActiveDaySections[1], TimeOfDayPreferrence.DaySection.Morning);
            Assert.AreEqual(TimeOfDayPreferrence.ActiveDaySections[2], TimeOfDayPreferrence.DaySection.Afternoon);
            Assert.AreEqual(TimeOfDayPreferrence.ActiveDaySections[3], TimeOfDayPreferrence.DaySection.Evening);

            Assert.AreEqual(0, TimeOfDayPreferrence.DaysectionToIndexDictionary[TimeOfDayPreferrence.DaySection.Sleep]);
            Assert.AreEqual(1, TimeOfDayPreferrence.DaysectionToIndexDictionary[TimeOfDayPreferrence.DaySection.Morning]);
            Assert.AreEqual(2, TimeOfDayPreferrence.DaysectionToIndexDictionary[TimeOfDayPreferrence.DaySection.Afternoon]);
            Assert.AreEqual(3, TimeOfDayPreferrence.DaysectionToIndexDictionary[TimeOfDayPreferrence.DaySection.Evening]);

            var values = Enum.GetValues(typeof(TimeOfDayPreferrence.DaySection)).Cast<TimeOfDayPreferrence.DaySection>().ToList();
            Assert.AreEqual(values.Count - 2, TimeOfDayPreferrence.ActiveDaySections.Count);
        }

        [TestCleanup]
        public void cleanUpForEachTest()
        {
            TestUtility.cleanupDB();
            //UserAccount user = TestUtility.getTestUser(true, userId: tilerUser.Id);
        }

        [ClassCleanup]
        public static void cleanUpTest()
        {
            //UserAccount currentUser = TestUtility.getTestUser(userId: tilerUser.Id);
            //currentUser.Login().Wait();
            //DateTimeOffset refNow = DateTimeOffset.UtcNow;
            //Schedule Schedule = new TestSchedule(currentUser, refNow);
            //currentUser.DeleteAllCalendarEvents();
        }
    }
}
