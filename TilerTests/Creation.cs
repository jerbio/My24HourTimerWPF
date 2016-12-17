﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerElements;
using My24HourTimerWPF;
using TilerFront;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TilerTests
{
    [TestClass]
    public class Creation
    {
        Schedule Schedule;
        static DateTimeOffset refNow = DateTimeOffset.Now;
        CalendarEvent CalendarEvent1;
        CalendarEvent CalendarEvent2;
        CalendarEvent CalendarEvent3;



        [TestMethod]
        public void TestCreationOfNonRigid()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            
            Schedule = new Schedule(currentUser, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            List<TimeLine> timeLines = TestUtility.getTimeFrames(refNow , duration);
            foreach(TimeLine eachTimeLine in timeLines)
            {
                CalendarEvent testEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(1),  new Repetition(), eachTimeLine.Start, eachTimeLine.End, 1, false);
                Schedule.AddToSchedule(testEvent);
                CalendarEvent newlyaddedevent = Schedule.getCalendarEvent(testEvent.Calendar_EventID);
                Assert.AreEqual(testEvent.Id, newlyaddedevent.Id);
            }
            
        }

        [TestMethod]
        public void TestCreationOfRigid()
        {
            UserAccount user = TestUtility.getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Now;
            Schedule = new Schedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, true);
            Schedule.AddToSchedule(testEvent);
            CalendarEvent newlyaddedevent = Schedule.getCalendarEvent(testEvent.Calendar_EventID);
            Assert.AreEqual(testEvent.Id, newlyaddedevent.Id);
        }

        [TestMethod]
        public void testPersistedCalendarEvent()
        {
            UserAccount user = TestUtility.getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Now;
            Schedule schedule = new Schedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, true);
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            schedule = null;
            schedule = new Schedule(user, refNow);
            CalendarEvent newlyaddedevent = schedule.getCalendarEvent(testEvent.Calendar_EventID);
            Assert.IsTrue(newlyaddedevent.isTestEquivalent(testEvent));
        }


        [TestMethod]
        public void testPersistedSubCalendarEvent()
        {
            UserAccount user = TestUtility.getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Now;
            Schedule schedule = new Schedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, true);
            string currentID = testEvent.Id;
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            CalendarEvent tempEvent = schedule.getCalendarEvent(testEvent.Calendar_EventID);
            Schedule tempSchedule = schedule;
            schedule = null;
            schedule = new Schedule(user, refNow);
            CalendarEvent newlyaddedevent = schedule.getCalendarEvent(testEvent.Calendar_EventID);
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
            Schedule schedule = new TestSchedule(user, refNow);
            TimeSpan duration = TimeSpan.FromHours(1);
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.Add(duration);//.Add(duration).Add(duration);
            CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, true);
            EventName oldName = testEvent.Name;
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            schedule = new TestSchedule(user, refNow);
            CalendarEvent copyOfTestEvent = schedule.getCalendarEvent(testEvent.Id);
            EventName newName = new EventName("test-Event-For-stack-"+Guid.NewGuid().ToString());
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> tupleResult = schedule.BundleChangeUpdate(testEvent.ActiveSubEvents.First().Id, 
                newName, 
                testEvent.ActiveSubEvents.First().Start, 
                testEvent.ActiveSubEvents.First().End, 
                testEvent.ActiveSubEvents.First().Start, 
                testEvent.ActiveSubEvents.First().End, 
                testEvent.NumberOfSplit);
            schedule.UpdateWithDifferentSchedule(tupleResult.Item2).Wait();
            TestSchedule scheduleReloaded = new TestSchedule(user, refNow);
            CalendarEvent renamedEvent = scheduleReloaded.getCalendarEvent(testEvent.Id);
            Assert.AreEqual(renamedEvent.Name.NameValue, newName.NameValue);
            Assert.AreEqual(renamedEvent.ActiveSubEvents.First().Name.NameValue, newName.NameValue);
            Assert.AreEqual(renamedEvent.Name.NameId, testEvent.Name.NameId);

        }

        [TestCleanup]
        void cleanUpForEachTest()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Now;
            Schedule Schedule = new Schedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
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
