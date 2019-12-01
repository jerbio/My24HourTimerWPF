using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class UserActivity
    {
        protected DateTimeOffset TriggerTimeForEvent { get; set; }
        public enum ActivityType {Undo, ThirdPartyUpdate, SetAsNowSingle, SetAsNowCalendarEvent, ProcrastinateSingle, ProcrastinateAll, ProcrastinateCalendarEvent, InternalUpdate, InternalUpdateCalendarEvent, CompleteSingle, CompleteMultiple, CompleteCalendarEvent, DeleteSingle, DeleteMultiple, DeleteCalendarEvent, NewEventCreation, None, Shuffle, Pause, Resume, Repeat};
        protected ActivityType Type { get; set; }

        protected string MiscelaneousExtraInfo;
        protected List<string> updatedIds = new List<string>();
        public UserActivity(ReferenceNow triggerTime, ActivityType type, IEnumerable<string> ids = null, string miscData =null): this(triggerTime.constNow, type, ids,miscData)
        {
            
        }

        public UserActivity(DateTimeOffset triggerTime, ActivityType type, IEnumerable<string> ids = null, string miscData = null)
        {
            MiscelaneousExtraInfo = miscData;
            TriggerTimeForEvent = triggerTime;
            Type = type;
            if (ids != null)
            {
                updatedIds.AddRange(ids);
                updatedIds = updatedIds.ToList();
            }
        }

        public DateTimeOffset ActivityTriggerTime // format: 2011-11-11T15:05:46.473340ids6+01:00
        {
            get { return TriggerTimeForEvent;
            }
        }

        public void updateMiscelaneousInfo (string data)
        {
            MiscelaneousExtraInfo = data;
        }

        public string getMiscdata()
        {
            return MiscelaneousExtraInfo;
        }

        public ActivityType TriggerType// type of trigger
        {
            get { return Type; }
        }

        public List<string> eventIds// type of trigger
        {
            get { return updatedIds; }
        }

        
    }


}
