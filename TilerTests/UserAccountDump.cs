using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerTests
{
    public class UserAccountDump: UserAccountTest
    {
        public UserAccountDump(TilerUser user):base(user)
        {
            UserLog = new LogControlDump(user, "");
            ID = SessionUser.Id;
        }

        public override async System.Threading.Tasks.Task<bool> Login()
        {
            bool retValue = UserLog.Status && (SessionUser != null);
            return retValue;
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