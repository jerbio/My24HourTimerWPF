using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using TilerElements;
using System.Threading.Tasks;
using System.Xml;
using TilerTests.Models;

namespace TilerTests
{
    public class LogControlDump:LogControlTest
    {
        string UserID;
        /*
        public LogControlDebug(string userid)
        {
            UserID = userid;
        }
        */
        public LogControlDump(TilerUser User, string connectionName=""):base(User, connectionName)
        {
            _Context = string.IsNullOrEmpty(connectionName) || string.IsNullOrWhiteSpace(connectionName) ? new TestDBContext() : new TestDBContext(connectionName);
        }


        public override bool Status
        {
            get
            {
                return LogStatus;
            }
        }

        public override Task updateBigData(XmlDocument oldData, XmlDocument newData)
        {

            Task retValue = new Task(() => { });
            retValue.RunSynchronously();
            retValue.Wait();
            return retValue;
        }
    }
}