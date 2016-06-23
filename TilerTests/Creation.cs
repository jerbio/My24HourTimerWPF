using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerElements;
using My24HourTimerWPF;
using TilerFront;
using System.Threading.Tasks;

namespace TilerTests
{
    [TestClass]
    public class Creation
    {
        Schedule Schedule;
        DateTimeOffset StartOfTime = new DateTimeOffset();
        CalendarEvent CalendarEvent1;
        CalendarEvent CalendarEvent2;
        CalendarEvent CalendarEvent3;
        

        CalendarEvent generateCalendarEvent(TimeSpan duration, Repetition repetition, int splitCount = 1, bool rigidFlags = false,  DateTimeOffset Start = new DateTimeOffset() )
        {
            if(Start == StartOfTime)
            {
                Start = TestUtility.Start;
            }
            DateTimeOffset End = Start.AddMonths(TestUtility.Rand.Next() % TestUtility.MonthLimit);
            CalendarEvent RetValue = new CalendarEvent("TestCalendarEvent" + Guid.NewGuid().ToString(), duration, Start, End, new TimeSpan(), new TimeSpan(), rigidFlags, repetition, splitCount, new Location_Elements(), true, new EventDisplay(), new MiscData(), false);
            return RetValue;
        }

        UserAccount getTestUser()
        {
            TilerFront.Models.LoginViewModel myLogin = new TilerFront.Models.LoginViewModel() { Username = TestUtility.UserName, Password = TestUtility.Password, RememberMe = true };

            TilerFront.Models.AuthorizedUser AuthorizeUser = new TilerFront.Models.AuthorizedUser() { UserID = "065febec-d1fe-4c8b-bd32-548613d4479f", UserName = TestUtility.UserName };
            Task<UserAccountDebug> waitForUseraccount = AuthorizeUser.getUserAccountDebug();
            waitForUseraccount.Wait();

            return waitForUseraccount.Result;
        }

        [TestMethod]
        public void TestCreationOfNonRigid()
        {

            UserAccount currentUser = getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Now;
            Schedule = new Schedule(currentUser, refNow);
            CalendarEvent testEvent =  generateCalendarEvent(TimeSpan.FromHours(1), new Repetition(), 1, false);
            Schedule.AddToSchedule(testEvent);
            CalendarEvent newlyaddedevent = Schedule.getCalendarEvent(testEvent.Calendar_EventID);
            Assert.AreEqual(testEvent.ID, newlyaddedevent.ID);
        }

        [TestMethod]
        public void TestCreationOfRigid()
        {
            UserAccount user = getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Now;
            Schedule = new Schedule(user, refNow);
            CalendarEvent testEvent = generateCalendarEvent(TimeSpan.FromHours(1), new Repetition(), 1, true);
            Schedule.AddToSchedule(testEvent);
            CalendarEvent newlyaddedevent = Schedule.getCalendarEvent(testEvent.Calendar_EventID);
            Assert.AreEqual(testEvent.ID, newlyaddedevent.ID);
        }

        [TestMethod]
        public void testPersistedCalendarEvent()
        {
            UserAccount user = getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Now;
            Schedule schedule = new Schedule(user, refNow);
            CalendarEvent testEvent = generateCalendarEvent(TimeSpan.FromHours(1), new Repetition(), 1, true);
            schedule.AddToScheduleAndCommit(testEvent).Wait();
            schedule = null;
            schedule = new Schedule(user, refNow);
            CalendarEvent newlyaddedevent = schedule.getCalendarEvent(testEvent.Calendar_EventID);
            Assert.AreEqual(testEvent.ID, newlyaddedevent.ID);
        }

    }
}
