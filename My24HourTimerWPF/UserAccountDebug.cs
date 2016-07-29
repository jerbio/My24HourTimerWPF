using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements.Wpf;
using TilerElements.DB;
using TilerElements;
using Microsoft.AspNet.Identity;

namespace My24HourTimerWPF
{
    public class UserAccountDebug: UserAccount
    {
        
        public UserAccountDebug(TilerUser user, bool Passive = false)
        {
            //LocalContext db = new LocalContext();
            LocalContext db = new TilerDbContext();
            //throw new NotImplementedException();
            sessionUser = user;
            UserLog = new ScheduleControl(db as TilerDbContext, user);
            ID = sessionUser.Id;
        }
        protected override async System.Threading.Tasks.Task<DateTimeOffset> getDayReferenceTime(string desiredDirectory = "")
        {
            throw new NotImplementedException("You are trying to get the reference day from uderdebug without it being implemented");
        }

        /*
        public override async System.Threading.Tasks.Task<bool> Login()
        {
            await UserLog.Initialize();
            return UserLog.Status;
        }
        */
    }
}