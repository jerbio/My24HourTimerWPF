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
        CalendarEvent CalendarEvent1;
        CalendarEvent CalendarEvent2;
        CalendarEvent CalendarEvent3;
        string UserName = "TestUserTiler";
        string Password = "T35tU53r#";
        [TestMethod]
        async public void TestMethod1()
        {
            TilerFront.Models.LoginViewModel myLogin = new TilerFront.Models.LoginViewModel() { Username = UserName, Password = Password, RememberMe = true };

            TilerFront.Models.AuthorizedUser AuthorizeUser = new TilerFront.Models.AuthorizedUser() { UserID = "065febec-d1fe-4c8b-bd32-548613d4479f", UserName = UserName };

            UserAccount currentUser = await AuthorizeUser.getUserAccountDebug();// new UserAccountDebug("18");
            await currentUser.Login();
            DateTimeOffset refNow = DateTimeOffset.Now;
            //refNow = DateTimeOffset.Parse("8:20 AM , April 6, 2016");
            Schedule = new Schedule(currentUser, refNow);
        }
    }
}
