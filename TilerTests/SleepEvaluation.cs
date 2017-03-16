using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using My24HourTimerWPF;
using System.Linq;
using TilerElements;
using System.Collections.Generic;

namespace TilerTests
{
    [TestClass]
    public class SleepEvaluation
    {
        [ClassInitialize]
        public static void classInitialize(TestContext testContext)
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TestSchedule Schedule = new TestSchedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }

        /// <summary>
        /// Simple test. Test inserts a new event in a given fresh full day, and then test to ensure that the schedule has the schedule event in given day. 
        /// Also that fresh day has to be a full day i.e full twenty four hours, and not partially sliced because of the time of the calculation execution.
        /// The neww event has just enough sleep before the beginning of the event.
        /// </summary>
        [TestMethod]
        public void sleepTestMethod1()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TestSchedule Schedule = new TestSchedule(currentUser, refNow);
            ReferenceNow now = Schedule.Now;
            
            Location location = TestUtility.getLocations()[0];
            List<DayTimeLine> allValidDays = now.getAllDaysForCalc().ToList();
            DayTimeLine dayForCalculaition = allValidDays[1];
            CalendarEvent newCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(1), new Repetition(), dayForCalculaition.Start, dayForCalculaition.End, 1, false, location: location);
            Schedule.AddToScheduleAndCommit(newCalEvent).Wait();
            dayForCalculaition = now.getAllDaysForCalc().ToList()[1];
            TimeSpan atLeastSleepSpan = TimeSpan.FromHours(8);
            SubCalendarEvent subEvent = dayForCalculaition.getSubEventsInDayTimeLine().First();
            TimeSpan sleepSpan = subEvent.Start - dayForCalculaition.Start;

            bool assertValue = atLeastSleepSpan <= sleepSpan;
            Assert.IsTrue(assertValue);
        }


        [TestMethod]
        public void sleepTestMethod2()
        {
            UserAccount currentuser = TestUtility.getTestUser();
            currentuser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            DateTimeOffset startOfDay = DateTimeOffset.Parse("10:00 pm");
            TestSchedule schedule = new TestSchedule(currentuser, refNow, startOfDay);
            ReferenceNow now = schedule.Now;

            Location location = TestUtility.getLocations()[0];
            List<DayTimeLine> allValidDays = now.getAllDaysForCalc().ToList();
            DayTimeLine dayForCalculation0 = allValidDays[1];
            DayTimeLine dayForCalculation1 = allValidDays[2];
            DayTimeLine dayForCalculation2 = allValidDays[3];
            TimeSpan eachEventTimeSpan = TimeSpan.FromHours(1.5);
            CalendarEvent calEvent0 = TestUtility.generateCalendarEvent(eachEventTimeSpan, new Repetition(), dayForCalculation0.Start, dayForCalculation0.End, 1, false, location: location);
            DateTimeOffset day2EventsStartEvents = dayForCalculation1.End;
            DateTimeOffset day2EventsEndEvents = dayForCalculation1.End.Date.AddDays(1);
            CalendarEvent calEvent1 = TestUtility.generateCalendarEvent(eachEventTimeSpan, new Repetition(), day2EventsStartEvents, day2EventsEndEvents, 1, false, location: location);
            CalendarEvent calEvent2 = TestUtility.generateCalendarEvent(eachEventTimeSpan, new Repetition(), dayForCalculation2.Start, dayForCalculation2.End, 1, false, location: location);
            schedule.AddToScheduleAndCommit(calEvent0).Wait();
            schedule = new TestSchedule(currentuser, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(calEvent1).Wait();
            schedule = new TestSchedule(currentuser, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(calEvent2).Wait();
            now = schedule.Now;
            allValidDays = now.getAllDaysForCalc().ToList();
            dayForCalculation0 = allValidDays[1];
            dayForCalculation1 = allValidDays[2];
            dayForCalculation2 = allValidDays[3];
            SubCalendarEvent subEvent0 = dayForCalculation0.getSubEventsInDayTimeLine().First();
            SubCalendarEvent subEvent1 = dayForCalculation2.getSubEventsInDayTimeLine().OrderBy(subEvent => subEvent.Start).ToList()[0];
            SubCalendarEvent subEvent2 = dayForCalculation2.getSubEventsInDayTimeLine().OrderBy(subEvent => subEvent.Start).ToList()[1];
            TimeSpan sleepSpan = subEvent2.Start - dayForCalculation2.Start;
            TimeSpan atLeastSleepSpan = TimeSpan.FromHours(8);

            bool assertValue = atLeastSleepSpan <= sleepSpan;
            Assert.IsTrue(assertValue);
        }

        /// <summary>
        /// Test creates eight one hour event and schedules them for the same day. There is enough time for an eigh hour sleep time. test will validate that it occurs
        /// </summary>
        [TestMethod]
        public void sleepTestMethod3()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TestSchedule Schedule = new TestSchedule(currentUser, refNow);
            ReferenceNow now = Schedule.Now;

            Location location = TestUtility.getLocations()[0];
            List<DayTimeLine> allValidDays = now.getAllDaysForCalc().ToList();
            DayTimeLine dayForCalculaition = allValidDays[1];
            for (int i =0; i < 8; i++)
            {
                Schedule = new TestSchedule(currentUser, refNow);
                CalendarEvent newCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(1), new Repetition(), dayForCalculaition.Start, dayForCalculaition.End, 1, false, location: location);
                Schedule.AddToScheduleAndCommit(newCalEvent, true).Wait();
            }
            Schedule = new TestSchedule(currentUser, refNow);
            List<SubCalendarEvent> allSubEvents = Schedule.getAllCalendarEvents().SelectMany(calEvent=> calEvent.AllSubEvents).OrderBy(aSubEvent => aSubEvent.Start).ToList();
            dayForCalculaition = now.getAllDaysForCalc().ToList()[1];
            TimeSpan atLeastSleepSpan = TimeSpan.FromHours(8);
            SubCalendarEvent subEvent = allSubEvents.First();
            TimeSpan sleepSpan = subEvent.Start - dayForCalculaition.Start;

            bool assertValue = atLeastSleepSpan <= sleepSpan;
            Assert.IsTrue(assertValue);
        }

        [TestCleanup]
        public void eachTestCleanUp()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            TestSchedule Schedule = new TestSchedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }
    }
}
