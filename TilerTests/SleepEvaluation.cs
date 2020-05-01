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
        //[ClassInitialize]
        //public static void classInitialize(TestContext testContext)
        //{
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    TestSchedule Schedule = new TestSchedule(currentUser, refNow);
        //    currentUser.DeleteAllCalendarEvents();
        //}

        /// <summary>
        /// Simple test. Test inserts a new event in a given fresh full day, and then test to ensure that the schedule has the schedule event in given day. 
        /// Also that fresh day has to be a full day i.e full twenty four hours, and not partially sliced because of the time of the calculation execution.
        /// The neww event has just enough sleep before the beginning of the event.
        /// </summary>
        [TestMethod]
        public void sleepTestMethod1()
        {
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            var packet = TestUtility.CreatePacket();
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            TestSchedule Schedule = new TestSchedule(user, refNow);
            ReferenceNow now = Schedule.Now;

            Location location = TestUtility.getAdHocLocations()[0];
            List<DayTimeLine> allValidDays = now.getAllDaysForCalc().ToList();
            DayTimeLine dayForCalculaition = allValidDays[1];
            CalendarEvent newCalEvent = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(1), new Repetition(), dayForCalculaition.Start, dayForCalculaition.End, 1, false, location: location);
            Schedule.AddToScheduleAndCommit(newCalEvent);
            Schedule.populateDayTimeLinesWithSubcalendarEvents();
            dayForCalculaition = now.getAllDaysForCalc().ToList()[1];
            TimeSpan atLeastSleepSpan = TimeSpan.FromHours(6);
            SubCalendarEvent subEvent = dayForCalculaition.getSubEventsInTimeLine().OrderBy(sub => sub.End).First();
            TimeSpan sleepSpan = subEvent.Start - dayForCalculaition.Start;

            bool assertValue = atLeastSleepSpan <= sleepSpan;
            Assert.IsTrue(assertValue);
        }

        /// <summary>
        /// Test creates multiple calendar events and verifies if the sleep span is still preserved
        /// </summary>
        [TestMethod]
        public void sleepTestMethod2()
        {
            DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            var packet = TestUtility.CreatePacket();
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;
            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
            TestSchedule schedule = new TestSchedule(user, refNow, startOfDay);
            ReferenceNow now = schedule.Now;

            Location location = TestUtility.getAdHocLocations()[0];
            List<DayTimeLine> allValidDays = now.getAllDaysForCalc().ToList();
            DayTimeLine dayForCalculation0 = allValidDays[1];
            DayTimeLine dayForCalculation1 = allValidDays[2];
            DayTimeLine dayForCalculation2 = allValidDays[3];
            TimeSpan eachEventTimeSpan = TimeSpan.FromHours(1.5);
            CalendarEvent calEvent0 = TestUtility.generateCalendarEvent(tilerUser, eachEventTimeSpan, new Repetition(), dayForCalculation0.Start, dayForCalculation0.End, 1, false, location: location);
            DateTimeOffset day2EventsStartEvents = dayForCalculation1.End;
            DateTimeOffset day2EventsEndEvents = dayForCalculation1.End.Date.AddDays(1);
            schedule.AddToScheduleAndCommit(calEvent0);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow, startOfDay);
            CalendarEvent calEvent1 = TestUtility.generateCalendarEvent(tilerUser, eachEventTimeSpan, new Repetition(), day2EventsStartEvents, day2EventsEndEvents, 1, false);
            calEvent1.LocationId = location.Id;
            calEvent1.AllSubEvents.AsParallel().ForAll((eachSubEvent) => eachSubEvent.LocationId = location.Id);
            schedule.AddToScheduleAndCommit(calEvent1);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            location = TestUtility.getLocation(user, tilerUser, location.Id);
            CalendarEvent calEvent2 = TestUtility.generateCalendarEvent(tilerUser, eachEventTimeSpan, new Repetition(), dayForCalculation2.Start, dayForCalculation2.End, 1, false);
            calEvent2.LocationId = location.Id;
            calEvent2.AllSubEvents.AsParallel().ForAll((eachSubEvent) => eachSubEvent.LocationId = location.Id);
            schedule = new TestSchedule(user, refNow, startOfDay);
            schedule.AddToScheduleAndCommit(calEvent2);

            TestUtility.reloadTilerUser(ref user, ref tilerUser);
            schedule = new TestSchedule(user, refNow, startOfDay);
            schedule.FindMeSomethingToDo(new Location()).Wait();
            now = schedule.Now;
            allValidDays = now.getAllDaysForCalc().ToList();
            dayForCalculation0 = allValidDays[1];
            dayForCalculation1 = allValidDays[2];
            dayForCalculation2 = allValidDays[3];

            List<DayTimeLine> dayTimeLines = new List<DayTimeLine>() { dayForCalculation0, dayForCalculation1, dayForCalculation2 };


            TimeSpan atLeastSleepSpan = TimeSpan.FromHours(6);
            foreach (DayTimeLine dayTimeline in dayTimeLines)
            {
                if(dayTimeline.getSubEventsInTimeLine().Count > 0)
                {
                    SubCalendarEvent subEvent = dayTimeline.WakeSubEvent;

                    DateTimeOffset sleepStart = dayTimeline.Start;
                    if(dayTimeline.PrecedingDaySleepSubEvent !=null)
                    {
                        sleepStart = dayTimeline.PrecedingDaySleepSubEvent.End;
                    }

                    TimeSpan sleepSpan = subEvent.Start - sleepStart;
                    bool assertValue = atLeastSleepSpan <= sleepSpan;
                    Assert.IsTrue(assertValue);
                }
                
            }
        }

        ///// <summary>
        ///// Test creates eight one hour event and schedules them for the same day. There is enough time for an eigh hour sleep time. test will validate that it occurs
        ///// </summary>
        //[TestMethod]
        //public void sleepTestMethod3()
        //{
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    TestSchedule Schedule = new TestSchedule(currentUser, refNow);
        //    ReferenceNow now = Schedule.Now;

        //    Location location = TestUtility.getLocations()[0];
        //    List<DayTimeLine> allValidDays = now.getAllDaysForCalc().ToList();
        //    DayTimeLine dayForCalculaition = allValidDays[1];
        //    for (int i =0; i < 8; i++)
        //    {
        //        Schedule = new TestSchedule(currentUser, refNow);
        //        CalendarEvent newCalEvent = TestUtility.generateCalendarEvent(TimeSpan.FromHours(1), new Repetition(), dayForCalculaition.Start, dayForCalculaition.End, 1, false, location: location);
        //        Schedule.AddToScheduleAndCommit(newCalEvent, true).Wait();
        //    }
        //    Schedule = new TestSchedule(currentUser, refNow);
        //    List<SubCalendarEvent> allSubEvents = Schedule.getAllCalendarEvents().SelectMany(calEvent=> calEvent.AllSubEvents).OrderBy(aSubEvent => aSubEvent.Start).ToList();
        //    dayForCalculaition = now.getAllDaysForCalc().ToList()[1];
        //    TimeSpan atLeastSleepSpan = TimeSpan.FromHours(8);
        //    SubCalendarEvent subEvent = allSubEvents.First();
        //    TimeSpan sleepSpan = subEvent.Start - dayForCalculaition.Start;

        //    bool assertValue = atLeastSleepSpan <= sleepSpan;
        //    Assert.IsTrue(assertValue);
        //}

        //[TestCleanup]
        //public void eachTestCleanUp()
        //{
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    TestSchedule Schedule = new TestSchedule(currentUser, refNow);
        //    currentUser.DeleteAllCalendarEvents();
        //}
    }
}
