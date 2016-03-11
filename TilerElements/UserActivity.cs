using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class UserActivity
    {
        ReferenceNow TriggerTimeForEvent;
        public enum ActivityType {ProcrastinateSingle, ProcrastinateAll, ProcrastinateCalendarEvent, AdjustDeadline, CompleteSingle, CompleteMultiple, CompleteCalendarEvent, DeleteSingle, DeleteMultiple, DeleteCalendarEvent, NewEventCreation, None};
        ActivityType Type;
        public UserActivity(ReferenceNow triggerTime, ActivityType type)
        {
            TriggerTimeForEvent = triggerTime;
            Type = type;
        }
    }


}
