using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class ProcrastinateAllSubCalendarEvent:SubCalendarEvent
    {
        public ProcrastinateAllSubCalendarEvent(TilerUser user, TilerUserGroup group, string timeZone, TimeLine timeLine, EventID calendarEventId, Location location, ProcrastinateCalendarEvent calendarEvent):base()
        {
            this._Name = calendarEvent.getName;
            this.LocationInfo = location;
            this._TimeZone = timeZone;
            this.StartDateTime = timeLine.Start;
            this.EndDateTime = timeLine.End;
            this.RigidSchedule = true;
            this.UniqueID = EventID.GenerateSubCalendarEvent(calendarEventId);
            this._Creator = user;
            this._Users = group;
            this.ProfileOfNow = new NowProfile();
            this.ProfileOfProcrastination = new Procrastination(new DateTimeOffset(), new TimeSpan());
            isProcrastinateEvent = true;
            this.CalendarEventRange = calendarEvent.RangeTimeLine;
            this.BusyFrame = new BusyTimeLine(this.UniqueID.ToString(), Start, End);
            this.EventDuration = this.BusyFrame.BusyTimeSpan;
        }
    }
}
