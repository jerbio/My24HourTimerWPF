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
        [TestInitialize]
        public void initializeTests() {
            TestUtility.init();
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
            string userId = Guid.NewGuid().ToString();
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();

            Schedule = new TestSchedule(currentUser, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            CalendarEvent testCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false);
            SubCalendarEvent testEvent = testCalEvent.EnabledSubEvents.First();
            testEvent.ParentCalendarEvent = null;
            string testEventId = testEvent.Id;
            var mockContext = TestUtility.getContext;
            mockContext.SubEvents.Add(testEvent);
            mockContext.SaveChanges();
            mockContext = new TestDBContext();
            var verificationEventPulled = mockContext.SubEvents.Find(testEventId);

            Assert.IsNotNull(testEvent);
            Assert.IsNotNull(verificationEventPulled);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));
        }

        [TestMethod]
        public void createAddCalendarEventToDB()
        {
            string userId = Guid.NewGuid().ToString();
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();

            Schedule = new TestSchedule(currentUser, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            TimeLine timeLine = TestUtility.getTimeFrames(refNow, duration).First();
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(1), new Repetition(), timeLine.Start, timeLine.End, 1, false);
            string testEVentId = testEvent.Id;
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            var mockContext = new TestDBContext();
            UserAccount user = TestUtility.getTestUser(true);

            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            CalendarEvent verificationEventPulled = waitVar.Result;

            Assert.IsNotNull(testEvent);
            Assert.IsNotNull(verificationEventPulled);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));
        }

        [TestMethod]
        public void TestCreationOfNonRigid()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            
            Schedule = new TestSchedule(currentUser, refNow);
            TimeSpan duration = TimeSpan.FromHours(5);
            List<TimeLine> timeLines = TestUtility.getTimeFrames(refNow , duration);
            foreach(TimeLine eachTimeLine in timeLines)
            {
                DateTimeOffset TimeCreation = DateTimeOffset.UtcNow;
                CalendarEvent testEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(1),  new Repetition(), eachTimeLine.Start, eachTimeLine.End, 1, false);
                testEvent.TimeCreated = TimeCreation;
                currentUser = TestUtility.getTestUser();
                Schedule.AddToScheduleAndCommit(testEvent).Wait();
                currentUser = TestUtility.getTestUser();
                currentUser.Login().Wait();
                Schedule = new TestSchedule(currentUser, refNow);
            }
        }

        [TestMethod]
        public void TestCreationOfRigid()
        {
            UserAccount user = TestUtility.getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            DateTimeOffset TimeCreation = DateTimeOffset.UtcNow;
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, true);
            testEvent.TimeCreated = TimeCreation;
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            var testEVentId = testEvent.Id;
            var mockContext = new TestDBContext();
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEVentId);
            CalendarEvent verificationEventPulled = waitVar.Result;
            Assert.IsNotNull(testEvent);
            Assert.IsNotNull(verificationEventPulled);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));

        }

        [TestMethod]
        public void TestCreationOfRepeatRigid()
        {
            UserAccount user = TestUtility.getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            Schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(14));
            Repetition repetition = new Repetition(true, repetitionRange, Repetition.Frequency.DAILY, new TimeLine(start, end));
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, repetition, start, end, 1, true);
            string testEVentId = testEvent.getId;
            Schedule.AddToScheduleAndCommit(testEvent).Wait();
            var mockContext = new TestDBContext();
            user = TestUtility.getTestUser(true);

            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEVentId);
            waitVar.Wait();
            CalendarEvent verificationEventPulled = waitVar.Result;
            Assert.IsNotNull(testEvent);
            Assert.IsNotNull(verificationEventPulled);
            Assert.IsTrue(testEvent.isTestEquivalent(verificationEventPulled));
        }

        [TestMethod]
        public void TestCreationOfWeekdayRepeatRigid()
        {
            UserAccount user = TestUtility.getTestUser();
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
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, repetition, start, end, 1, true);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            var mockContext = new TestDBContext();
            user = TestUtility.getTestUser(true);

            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
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
            UserAccount user = TestUtility.getTestUser(true);
            DateTimeOffset refNow = DateTimeOffset.Parse("12:00AM 12/2/2017");
            user.Login().Wait();
            TimeSpan duration = TimeSpan.FromHours(4);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            TimeLine repetitionRange = new TimeLine(start, start.AddDays(13).AddHours(-23));
            List<DayOfWeek> weekDays = new List<DayOfWeek>() { start.DayOfWeek, (DayOfWeek)(((int)start.DayOfWeek + 2)%7), (DayOfWeek)(((int)start.DayOfWeek + 4)%7)};
            //List<DayOfWeek> weekDays = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday ,DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday};
            List<int> weekDaysAsInt = weekDays.Select(obj => (int)obj).ToList();
            DayOfWeek startingWeekDay = start.DayOfWeek;
            Repetition repetition = new Repetition(true, repetitionRange, Repetition.Frequency.WEEKLY, new TimeLine(start, end), weekDaysAsInt.ToArray());
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, repetition, repetitionRange.Start, repetitionRange.End, 1, false);
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(testEvent).Wait();

            var checkingNull = testEvent.getRepeatedCalendarEvent(testEvent.ActiveSubEvents.First().SubEvent_ID.getIDUpToRepeatCalendarEvent());
            var all = testEvent.AllSubEvents;
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
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
        public void testPersistedCalendarEvent()
        {
            UserAccount user = TestUtility.getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TestSchedule schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, true);
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            schedule = null;
            user = TestUtility.getTestUser(true);
            schedule = new TestSchedule(user, refNow);
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            CalendarEvent newlyaddedevent = waitVar.Result;
            Assert.IsTrue(newlyaddedevent.isTestEquivalent(testEvent));
        }

        [TestMethod]
        public void testPersistedSubCalendarEvent()
        {
            UserAccount user = TestUtility.getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TestSchedule schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, true);
            string currentID = testEvent.getId;
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            CalendarEvent tempEvent = schedule.getCalendarEvent(testEvent.Calendar_EventID);
            Schedule tempSchedule = schedule;
            schedule = null;
            user = TestUtility.getTestUser(true);

            var mockContext = new TestDBContext();
            user = TestUtility.getTestUser(true);

            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
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
            UserAccount user = TestUtility.getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, refNow.Day, refNow.Hour, refNow.Minute, refNow.Second, new TimeSpan());
            TestSchedule schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);//.Add(duration).Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, true);
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
            schedule.UpdateWithDifferentSchedule(tupleResult.Item2).Wait();
            var mockContext = new TestDBContext();
            user = TestUtility.getTestUser(true);

            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            CalendarEvent renamedEvent = waitVar.Result;

            Assert.AreEqual(renamedEvent.getName.NameValue, newName);
            Assert.AreEqual(renamedEvent.ActiveSubEvents.First().getName.NameValue, newName);
            Assert.AreEqual(renamedEvent.getName.NameId, testEvent.getName.NameId);
        }

        [TestMethod]
        public void testChangeOfNotesOfEvent()
        {
            UserAccount user = TestUtility.getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = new DateTimeOffset(refNow.Year, refNow.Month, refNow.Day, refNow.Hour, refNow.Minute, refNow.Second, new TimeSpan());
            TestSchedule schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);//.Add(duration).Add(duration);
            string oldNoteName = "test initial note";
            string newNoteName = "test change note";
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, true, note: new MiscData(oldNoteName));
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
            schedule.UpdateWithDifferentSchedule(tupleResult.Item2).Wait();

            var mockContext = new TestDBContext();
            user = TestUtility.getTestUser(true);

            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(testEvent.Id);
            CalendarEvent renamedEvent = waitVar.Result;

            Assert.AreEqual(renamedEvent.Notes.UserNote, newNoteName);
        }

        [TestCleanup()]
        public void cleanUpForEachTest()
        {
            TestUtility.cleanupDB();
            UserAccount user = TestUtility.getTestUser(true);
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
