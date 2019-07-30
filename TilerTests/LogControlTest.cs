using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using TilerElements;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront.Models;
using System.Data.Entity;
using TilerTests.Models;

namespace TilerTests
{
    public class LogControlTest: TilerFront.LogControlDirect
    {
    
        /*
        public LogControlDebug(string userid)
        {
            UserID = userid;
        }
        */
        public LogControlTest(TilerUser User, TilerDbContext dbContext = null):base(User, null)
        {
            _Context = dbContext ?? new TestDBContext();
        }

        public override bool Status
        {
            get
            {
                return LogStatus;
            }
        }

        public override void deleteAllCalendarEvents(string dirString = "")
        {
            _Context.CalEvents.ForEachAsync(calEvent => {
                    if(calEvent.CreatorId == _TilerUser.Id)
                    {
                        calEvent.Disable(false);
                    }    
                });
        }
    }
}