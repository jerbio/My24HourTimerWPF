using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerElements;
using My24HourTimerWPF;
using TilerFront;

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
            CalendarEvent RetValue = new CalendarEvent("TestCalendarEvent" + Guid.NewGuid().ToString(), duration,Start, End, new TimeSpan(), new TimeSpan(),rigidFlags, repetition,splitCount,)
        }

        [TestMethod]
        async public void TestMethod1()
        {
            TilerFront.Models.LoginViewModel myLogin = new TilerFront.Models.LoginViewModel() { Username = TestUtility.UserName, Password = TestUtility.Password, RememberMe = true };

            TilerFront.Models.AuthorizedUser AuthorizeUser = new TilerFront.Models.AuthorizedUser() { UserID = "065febec-d1fe-4c8b-bd32-548613d4479f", UserName = TestUtility.UserName };

            UserAccount currentUser = await AuthorizeUser.getUserAccountDebug();// new UserAccountDebug("18");
            await currentUser.Login();
            DateTimeOffset refNow = DateTimeOffset.Now;
            //refNow = DateTimeOffset.Parse("8:20 AM , April 6, 2016");
            Schedule = new Schedule(currentUser, refNow);
        }
    }
}
