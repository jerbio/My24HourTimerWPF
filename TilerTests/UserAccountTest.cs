using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;
using TilerFront.Models;

namespace TilerTests
{
    public class UserAccountTest: TilerFront.UserAccountDirect
    {
        public UserAccountTest(TilerUser user, TilerDbContext Database = null)
        {
            UserLog = new LogControlTest(user, Database);
            ID = SessionUser.Id;
        }

        public override async System.Threading.Tasks.Task<bool> Login()
        {
            return UserLog.getTilerRetrievedUser() != null;
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