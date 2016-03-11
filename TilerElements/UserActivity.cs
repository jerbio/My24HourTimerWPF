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
        public enum ActivityType {Creation, Undo, ThirdPartyUpdate, SetAsNowSingle, SetAsNowCalendarEvent, ProcrastinateSingle, ProcrastinateAll, ProcrastinateCalendarEvent, InternalUpdate, InternalUpdateCalendarEvent, CompleteSingle, CompleteMultiple, CompleteCalendarEvent, DeleteSingle, DeleteMultiple, DeleteCalendarEvent, NewEventCreation, None};
        protected ActivityType Type { get; set; }
        protected List<string> updatedIds = new List<string>();
        public UserActivity(ReferenceNow triggerTime, ActivityType type, IEnumerable<String> ids = null): this(triggerTime.constNow, type, ids)
        {
            
        }

        public UserActivity(DateTimeOffset triggerTime, ActivityType type, IEnumerable<String> ids = null)
        {
            TriggerTimeForEvent = triggerTime;
            Type = type;
            if (ids != null)
            {
                updatedIds.AddRange(ids);
            }
        }

        public DateTimeOffset ActivityTriggerTime // format: 2011-11-11T15:05:46.473340ids6+01:00
        {
            get { return TriggerTimeForEvent;
            }
        }


        public ActivityType TriggerType// format: 2011-11-11T15:05:46.4733406+01:00
        {
            get { return Type; }
        }
    }


}
