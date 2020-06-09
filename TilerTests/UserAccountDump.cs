using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;
using TilerTests.Models;

namespace TilerTests
{
    public class UserAccountDump: UserAccountTest
    {
        public UserAccountDump(TilerUser user, string connectionName =""):base(user, string.IsNullOrEmpty(connectionName)? null : new TestDBContext(connectionName))
        {
            UserLog = string.IsNullOrEmpty(connectionName) || string.IsNullOrWhiteSpace(connectionName) ? new LogControlDump(user) : new LogControlDump(user, connectionName);
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