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
        public LogControlTest(TilerUser User, TilerDbContext dbContext = null):base(User, null)
        {
            _Context = dbContext ?? new ApplicationDbContext();
        }

        public override bool Status
        {
            get
            {
                return LogStatus;
            }
        }

        public override Task<Dictionary<string, CalendarEvent>> getAllCalendarFromXml(TimeLine RangeOfLookUP, bool includeSubEvents = true, DataRetrivalOption retrievalOption = DataRetrivalOption.Evaluation, string singleCalEventId = null)
        {
            if(RangeOfLookUP == null)
            {
                IQueryable<CalendarEvent> calEVents = _Context.CalEvents;
                if (includeSubEvents)
                {
                    //calEVents = _Database.CalEvents
                    //    .Include(calEvent => calEvent.UiParams_EventDB)
                    //    .Include(calEvent => calEvent.DataBlob_EventDB)
                    //    .Include(calEvent => calEvent.Name)
                    //    .Include(calEvent => calEvent.Name.Creator_EventDB)
                    //    .Include(calEvent => calEvent.Location_DB)
                    //    .Include(calEvent => calEvent.Creator_EventDB)
                    //    .Include(calEvent => calEvent.Repetition_EventDB)
                    //    .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.ParentCalendarEvent))
                    //    .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name))
                    //    .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name.Creator_EventDB))
                    //    .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Creator_EventDB))
                    //    .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.UiParams_EventDB))
                    //    .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Location_DB))
                    //    .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.DataBlob_EventDB));
                    calEVents = _Context.CalEvents
                        .Include(calEvent => calEvent.DataBlob_EventDB)
                        .Include(calEvent => calEvent.Name)
                        .Include(calEvent => calEvent.Name.Creator_EventDB)
                        .Include(calEvent => calEvent.Location_DB)
                        .Include(calEvent => calEvent.Creator_EventDB)
                        .Include(calEvent => calEvent.Repetition_EventDB)
                        .Include(calEvent => calEvent.Procrastination_EventDB)
                        .Include(calEvent => calEvent.ProfileOfNow_EventDB)
                        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.ParentCalendarEvent))
                        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name))
                        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.DataBlob_EventDB))
                        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name.Creator_EventDB))
                        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Creator_EventDB))
                        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Location_DB))
                        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.DataBlob_EventDB))
                        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Procrastination_EventDB))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents)
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name)))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.Location_DB))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.DataBlob_EventDB))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.Name))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.DataBlob_EventDB))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.Name.Creator_EventDB))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.Creator_EventDB))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.Repetition_EventDB))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.Location_DB))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.Procrastination_EventDB))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.ProfileOfNow_EventDB))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.ParentCalendarEvent)))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name)))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name.Creator_EventDB)))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.DataBlob_EventDB)))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Location_DB)))
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Procrastination_EventDB)))
                        .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions)
                        .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.SubRepetitions))
                        .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB)))
                        .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.Procrastination_EventDB)))
                        .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.ProfileOfNow_EventDB)));

                    if (retrievalOption == DataRetrivalOption.UiAll)
                    {
                        calEVents = calEVents.Include(calEvent => calEvent.UiParams_EventDB)
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.UiParams_EventDB));
                    }
                    else if (retrievalOption == DataRetrivalOption.UiSingle)
                    {
                        if (!string.IsNullOrEmpty(singleCalEventId) && !string.IsNullOrWhiteSpace(singleCalEventId))
                        {
                            calEVents = calEVents.Where(calEvent => calEvent.Id == singleCalEventId)
                            .Include(calEvent => calEvent.UiParams_EventDB)
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.UiParams_EventDB));
                        }
                        else
                        {
                            throw new ArgumentException("singleCalEventId cannot be null, empty or white space");
                        }
                    }
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
            _Context.CalEvents.ForEachAsync(calEvent => {
                    if(calEvent.CreatorId == _TilerUser.Id)
                    {
                        calEvent.Disable(false);
                    }    
                });
        }
    }
}