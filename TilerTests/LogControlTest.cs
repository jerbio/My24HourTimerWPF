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

        public override Task<Dictionary<string, CalendarEvent>> getAllCalendarFromXml(TimeLine RangeOfLookUP, bool includeSubEvents = true)
        {
            if(RangeOfLookUP == null)
            {
                IQueryable<CalendarEvent> calEVents = _Database.CalEvents;
                if (includeSubEvents)
                {
                    calEVents = _Database.CalEvents
                        .Include(calEvent => calEvent.UiParams_EventDB)
                        .Include(calEvent => calEvent.DataBlob_EventDB)
                        .Include(calEvent => calEvent.Name)
                        .Include(calEvent => calEvent.Name.Creator_EventDB)
                        .Include(calEvent => calEvent.Location_DB)
                        .Include(calEvent => calEvent.Creator_EventDB)
                        .Include(calEvent => calEvent.Repetition_EventDB)
                        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.ParentCalendarEvent))
                        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name))
                        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name.Creator_EventDB))
                        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Creator_EventDB))
                        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.UiParams_EventDB))
                        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Location_DB))
                        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.DataBlob_EventDB));

                }
                Dictionary<string, CalendarEvent> MyCalendarEventDictionary = calEVents.Where(calEvent => calEvent.CreatorId == _TilerUser.Id && !calEvent.IsRepeatsChildCalEvent).ToDictionary(calEvent => calEvent.Calendar_EventID.getCalendarEventComponent(), calEvent => calEvent);
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