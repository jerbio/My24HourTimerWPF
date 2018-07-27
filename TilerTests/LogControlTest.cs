using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using TilerElements;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront.Models;

namespace TilerTests
{
    public class LogControlTest: TilerFront.LogControlDirect
    {
        string UserID;
        /*
        public LogControlDebug(string userid)
        {
            UserID = userid;
        }
        */
        public LogControlTest(TilerUser User, TilerDbContext db = null):base(User, null)
        {
            _Database = db ?? new ApplicationDbContext();
        }

        public override bool Status
        {
            get
            {
                return LogStatus;
            }
        }
    }
}