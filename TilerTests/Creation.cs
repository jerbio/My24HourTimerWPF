﻿using System;
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

namespace TilerTests
{
    [TestClass]
    public class Creation
    {
        TestSchedule Schedule;
        static DateTimeOffset refNow = DateTimeOffset.UtcNow;
        CalendarEvent CalendarEvent1;
        CalendarEvent CalendarEvent2;
        CalendarEvent CalendarEvent3;

        public TestContext TestContext
        {
            get;
            set;
        }
        //[TestInitialize]
        //public void initializeTests() {
        //    TestUtility.init();
        //}

        [TestMethod]
        public void TestContextCaching ()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false);
            Assert.IsFalse(testEvent.isCalculableInitialized);
            string testEVentId = testEvent.Id;
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
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
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            Schedule = new TestSchedule(user, refNow);
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
                                            .FirstOrDefault(subevent => subevent.Id == testEventId);

            Assert.IsNotNull(testEvent);
            Assert.IsNotNull(verificationEventPulled);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));
        }

        [TestMethod]
        public void createAddCalendarEventToDB()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            TilerColor tilerColor = new TilerColor(255, 255, 0, 1, 5);
            EventDisplay eventdisplay = new EventDisplay(true, tilerColor);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false, eventDisplay: eventdisplay);
            string testEVentId = testEvent.Id;
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            var mockContext = new TestDBContext();
            UserAccount userAcc = TestUtility.getTestUser(true, userId: tilerUser.Id);

            Task<CalendarEvent> waitVar = userAcc.ScheduleLogControl.getCalendarEventWithID(testEvent.Id, DataRetrivalOption.UiAll);
            waitVar.Wait();
            CalendarEvent verificationEventPulled = waitVar.Result;

            Assert.IsNotNull(testEvent);
            Assert.IsNotNull(verificationEventPulled);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));
        }

        [TestMethod]
        public void storeScheudlueDumpToDB()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            Schedule = new TestSchedule(user, refNow, retrievalOption: DataRetrivalOption.UiAll);
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            TilerColor tilerColor = new TilerColor(255, 255, 0, 1, 5);
            EventDisplay eventdisplay = new EventDisplay(true, tilerColor);

            Location location = TestUtility.getLocations()[0];
            location.Validate();

            // Adding event one
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false, eventDisplay: eventdisplay, location: location);
            string testEVentId = testEvent.Id;
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            Task<ScheduleDump> dumpWait = Schedule.CreateAndPersistScheduleDump();
            dumpWait.Wait();
            ScheduleDump scheduleDump = dumpWait.Result;

            var mockContext = user.ScheduleLogControl.Database;

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            mockContext = user.ScheduleLogControl.Database;
            ScheduleDump retrievedDump = mockContext.ScheduleDumps.Find(scheduleDump.Id);

            Schedule = new TestSchedule(user, refNow, retrievalOption: DataRetrivalOption.UiAll);

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            Schedule scheduleFromDump = new TestSchedule(retrievedDump, user);

            Assert.IsTrue(scheduleFromDump.isTestEquivalent(Schedule));

            // Adding event two
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            Location location1 = TestUtility.getLocations()[1];
            TilerColor tilerColor1 = new TilerColor(255, 255, 0, 1, 5);
            EventDisplay eventdisplay1 = new EventDisplay(true, tilerColor1);
            CalendarEvent testEvent1 = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false, eventDisplay: eventdisplay1, location: location1);
            string testEVentId1 = testEvent1.Id;
            TestSchedule Schedule1 = new TestSchedule(user, refNow, retrievalOption: DataRetrivalOption.UiAll);
            Schedule1.AddToScheduleAndCommit(testEvent1).Wait();
            Task<ScheduleDump> tempScheduleDumpTask = Schedule1.CreateScheduleDump();
            tempScheduleDumpTask.Wait();
            ScheduleDump tempScheduleDump = tempScheduleDumpTask.Result;
            Task <ScheduleDump> dumpWait1 = Schedule1.CreateAndPersistScheduleDump(tempScheduleDump);
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

            Schedule1 = new TestSchedule(user, refNow, retrievalOption: DataRetrivalOption.UiAll);

            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            Schedule scheduleFromDump1 = new TestSchedule(retrievedDump1, user);

            Assert.IsTrue(scheduleFromDump1.isTestEquivalent(Schedule1));
        }



        [TestMethod]
        public void TestCreationOfNonRigid()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(5);
            List<TimeLine> timeLines = TestUtility.getTimeFrames(refNow , duration);
            foreach(TimeLine eachTimeLine in timeLines)
            {
                DateTimeOffset TimeCreation = DateTimeOffset.UtcNow;
                tilerUser = user.getTilerUser();
                CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1),  new Repetition(), eachTimeLine.Start, eachTimeLine.End, 1, false);
                testEvent.TimeCreated = TimeCreation;
                Schedule.AddToScheduleAndCommit(testEvent).Wait();
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
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            DateTimeOffset TimeCreation = DateTimeOffset.UtcNow;
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, true);
            testEvent.TimeCreated = TimeCreation;
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            var testEVentId = testEvent.Id;
            var mockContext = new TestDBContext();
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEVentId);
            waitVar.Wait();
            CalendarEvent verificationEventPulled = waitVar.Result;
            Assert.IsNotNull(testEvent);
            Assert.IsNotNull(verificationEventPulled);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));
        }

        [TestMethod]
        public void TestCreationOfRepeatRigid()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(14));
            Repetition repetition = new Repetition(true, repetitionRange, Repetition.Frequency.DAILY, new TimeLine(start, end));
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, true);
            string testEVentId = testEvent.getId;
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();

            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEVentId);
            waitVar.Wait();
            CalendarEvent verificationEventPulled = waitVar.Result;
            Assert.IsNotNull(testEvent);
            Assert.IsNotNull(verificationEventPulled);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));
        }


        [TestMethod]
        public void TestCreationOfNonRigidWithLocation()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(5);
            Location location = TestUtility.getLocations()[0];
            location.Validate();
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            DateTimeOffset TimeCreation = DateTimeOffset.UtcNow;
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false, location);
            testEvent.TimeCreated = TimeCreation;
            //user = TestUtility.getTestUser(userId: tilerUser.Id);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
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
        }

        [TestMethod]
        public void TestCreationOfRigidithLocation()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            Location location = TestUtility.getLocations()[0];
            location.Validate();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            DateTimeOffset TimeCreation = DateTimeOffset.UtcNow;
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end, 1, true, location);
            testEvent.TimeCreated = TimeCreation;
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            var mockContext = new TestDBContext();
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);
            var testEVentId = testEvent.Id;
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEVentId);
            waitVar.Wait();
            CalendarEvent verificationEventPulled = waitVar.Result;
            Assert.IsNotNull(testEvent);
            Assert.IsNotNull(verificationEventPulled);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));
        }

        //[TestMethod]
        //public void TestCreationOfRepeatRigid()
        //{
        //    UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
        //    user.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    Schedule = new TestSchedule(user, refNow);
        //    TimeSpan duration = TimeSpan.FromHours(1);
        //    DateTimeOffset start = refNow;
        //    DateTimeOffset end = refNow.Add(duration);
        //    TimeLine repetitionRange = new TimeLine(start, start.AddDays(14));
        //    Repetition repetition = new Repetition(true, repetitionRange, Repetition.Frequency.DAILY, new TimeLine(start, end));
        //    CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, true);
        //    string testEVentId = testEvent.getId;
        //    Schedule.AddToScheduleAndCommit(testEvent).Wait();
        //    var mockContext = new TestDBContext();
        //    user = TestUtility.getTestUser(true, userId: tilerUser.Id);

        //    Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEVentId);
        //    waitVar.Wait();
        //    CalendarEvent verificationEventPulled = waitVar.Result;
        //    Assert.IsNotNull(testEvent);
        //    Assert.IsNotNull(verificationEventPulled);
        //    Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));
        //}


        /// <summary>
        /// Test creates a restrictedP profile calendarEvent
        /// </summary>
        [TestMethod]
        public void TestCreationOfCalendarEventRestricted()
        {

            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition();
            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, restrictionProfile: new RestrictionProfile(start, duration + duration), now: Schedule.Now) as CalendarEventRestricted;
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            var mockContext = new TestDBContext();
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            
            CalendarEvent newlyaddedevent = waitVar.Result;
            Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
            Assert.IsTrue(testEvent.isTestEquivalent(newlyaddedevent));
        }

        [TestMethod]
        public void TestCreationOfCalendarEventRestrictedWithLocation()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Location location = TestUtility.getLocations()[0];
            location.Validate();
            Repetition repetition = new Repetition();
            CalendarEventRestricted testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, location, restrictionProfile: new RestrictionProfile(start, duration + duration), now: Schedule.Now) as CalendarEventRestricted;
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            var mockContext = new TestDBContext();
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            CalendarEvent newlyaddedevent = waitVar.Result;
            Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
            Assert.IsTrue(testEvent.isTestEquivalent(newlyaddedevent));
        }

        [TestMethod]
        public void TestCreationOfWeekdayRepeatRigid()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            Schedule = new TestSchedule(user, refNow);
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
            List<int> weekDaysAsInt = weekDays.Select(obj => (int)obj).ToList();
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition(true, repetitionRange, Repetition.Frequency.WEEKLY, new TimeLine(start, end), weekDaysAsInt.ToArray());
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, true);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

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
        }

        [TestMethod]
        public void TestCreationOfWeekdayRepeatNonRigid()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            user.Login().Wait();
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            List<DayOfWeek> weekDays = new List<DayOfWeek>() { start.DayOfWeek, (DayOfWeek)(((int)start.DayOfWeek + 2)%7), (DayOfWeek)(((int)start.DayOfWeek + 4)%7)};
            //List<DayOfWeek> weekDays = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday ,DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday};
            List<int> weekDaysAsInt = weekDays.Select(obj => (int)obj).ToList();
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition(true, repetitionRange, Repetition.Frequency.WEEKLY, new TimeLine(start, end), weekDaysAsInt.ToArray());
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, repetitionRange.Start, repetitionRange.End, 1, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            var checkingNull = testEvent.getRepeatedCalendarEvent(testEvent.ActiveSubEvents.First().SubEvent_ID.getIDUpToRepeatCalendarEvent());
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
        }


        [TestMethod]
        public void TestCreationOfDailyRepeatNonRigidIsLessThanADayEvent()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            //List<DayOfWeek> weekDays = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday ,DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday};
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition(true, repetitionRange, Repetition.Frequency.DAILY, new TimeLine(start, end));
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, repetitionRange.Start, repetitionRange.End, 1, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            CalendarEvent newlyaddedevent = Schedule.getCalendarEvent(testEvent.Calendar_EventID);
            Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
            CalendarEvent newlyaddedevent0 = Schedule.getCalendarEvent(newlyaddedevent.ActiveSubEvents.First().SubEvent_ID.getIDUpToRepeatCalendarEvent());
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
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            //List<DayOfWeek> weekDays = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday ,DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday};
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition(true, repetitionRange, Repetition.Frequency.DAILY, repetitionRange.CreateCopy());
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            CalendarEvent newlyaddedevent = Schedule.getCalendarEvent(testEvent.Calendar_EventID);
            Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
            CalendarEvent newlyaddedevent0 = Schedule.getCalendarEvent(newlyaddedevent.Calendar_EventID.getIDUpToRepeatCalendarEvent());
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
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            //List<DayOfWeek> weekDays = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday ,DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday};
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Location location = TestUtility.getLocations()[0];
            location.Validate();
            Repetition repetition = new Repetition(true, repetitionRange, Repetition.Frequency.DAILY, repetitionRange.CreateCopy());
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser, duration, repetition, start, end, 1, false, location, restrictionProfile: new RestrictionProfile(start, duration + duration), now: Schedule.Now);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            CalendarEvent newlyaddedevent = waitVar.Result;
            Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
            string repatCalEventId = newlyaddedevent.Calendar_EventID.getIDUpToRepeatCalendarEvent();

            List <SubCalendarEvent> subEvents = newlyaddedevent.AllSubEvents.OrderBy(subEvent => subEvent.Start).ToList();

            Assert.IsTrue(testEvent.isTestEquivalent(newlyaddedevent));
            int currentDayIndex = (int)repetitionRange.Start.DayOfWeek;
            for (int index = 0; index < subEvents.Count; index++)
            {
                DayOfWeek dayOfWeek = (DayOfWeek)currentDayIndex;
                Assert.AreEqual(subEvents[index].Start.DayOfWeek, dayOfWeek);
                currentDayIndex += 1;
                currentDayIndex %= 7;
            }
            //Assert.AreEqual(newlyaddedevent.Calendar_EventID.getCalendarEventComponent(), newlyaddedevent0.Calendar_EventID.getCalendarEventComponent());
        }

        /// <summary>
        /// Test creates a scenario where the origin calendar event has timeline that is wider than a day, but greater than repeat range
        /// </summary>
        [TestMethod]
        public void TestCreationCalendarEvent ()
        {
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            DateTimeOffset refNow = TestUtility.parseAsUTC("12:00AM 12/2/2017");
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = start.AddDays(21);
            TimeLine repetitionRange = new TimeLine(start, end.AddDays(-5).AddHours(-23));
            //List<DayOfWeek> weekDays = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday ,DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday};
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition(true, repetitionRange, Repetition.Frequency.WEEKLY, repetitionRange.CreateCopy());
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(tilerUser,duration, repetition, start, end, 1, false);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            CalendarEvent newlyaddedevent = waitVar.Result;
            Assert.AreEqual(testEvent.getId, newlyaddedevent.getId);
            
            List<SubCalendarEvent> subEvents = newlyaddedevent.AllSubEvents.OrderBy(subEvent => subEvent.Start).ToList();
            DayOfWeek dayOfWeek = repetitionRange.Start.DayOfWeek;
            for (int index = 0; index < subEvents.Count; index++)
            {
                Assert.AreEqual(subEvents[index].Start.DayOfWeek, dayOfWeek);
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
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            schedule = null;
            user = TestUtility.getTestUser(true, userId: tilerUser.Id);
            schedule = new TestSchedule(user, refNow);
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            waitVar.Wait();
            CalendarEvent newlyaddedevent = waitVar.Result;
            Assert.IsTrue(newlyaddedevent.isTestEquivalent(testEvent));
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
            schedule.AddToScheduleAndCommit(testEvent).Wait();
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
            schedule.AddToScheduleAndCommit(testEvent).Wait();
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
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            schedule = new TestSchedule(user, refNow);
            CalendarEvent copyOfTestEvent = schedule.getCalendarEvent(testEvent.getId);
            
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> tupleResult = schedule.BundleChangeUpdate(testEvent.ActiveSubEvents.First().getId,
                testEvent.getName.createCopy(),
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
