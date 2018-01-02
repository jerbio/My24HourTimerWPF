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
            this.Name = calendarEvent.getName;
            this._LocationInfo = location;
            this._TimeZone = timeZone;
            this.StartDateTime = timeLine.Start;
            this.EndDateTime = timeLine.End;
            this.RigidSchedule = true;
            this.UniqueID = EventID.GenerateSubCalendarEvent(calendarEventId);
            this._Creator = user;
            this._Users = group;
            this._ProfileOfNow = new NowProfile();
            this._ProfileOfProcrastination = new Procrastination(new DateTimeOffset(), new TimeSpan());
            isProcrastinateEvent = true;
            this.CalendarEventRange = calendarEvent.RangeTimeLine;
            this.BusyFrame = new BusyTimeLine(this.UniqueID.ToString(), Start, End);
            this._EventDuration = this.BusyFrame.BusyTimeSpan;
            this._DataBlob = new MiscData();
        }
    }
}
