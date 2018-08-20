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

        public override Task<Dictionary<string, CalendarEvent>> getAllCalendarFromXml(TimeLine RangeOfLookUP)
        {
            if(RangeOfLookUP == null)
            {
                Dictionary<string, CalendarEvent> MyCalendarEventDictionary = _Database.CalEvents.ToDictionary(calEvent => calEvent.getId, calEvent => calEvent);
                Func<Dictionary<string, CalendarEvent>> retFunc = new Func<Dictionary<string, CalendarEvent>>(() => { return MyCalendarEventDictionary; });
                Task<Dictionary<string, CalendarEvent>> retTask = Task.Run(retFunc);
                return retTask;
            } else
            {
                return base.getAllCalendarFromXml(RangeOfLookUP);
            }
        }

        public override void deleteAllCalendarEvents(string dirString = "")
        {
            _Database.CalEvents.ForEachAsync(calEvent => {
                    if(calEvent.CreatorId == _TilerUser.Id)
                    {
                        calEvent.Disable(false);
                    }    
                });
        }
    }
}