using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class ProcrastinateAllSubCalendarEvent:SubCalendarEvent
    {
        protected ProcrastinateAllSubCalendarEvent():base()
        {
            _isProcrastinateEvent = true;
            this._RigidSchedule = true;
        }
        public ProcrastinateAllSubCalendarEvent(TilerUser user, TilerUserGroup group, string timeZone, TimeLine timeLine, EventID calendarEventId, Location location, ProcrastinateCalendarEvent calendarEvent):base()
        {
            this.Name = calendarEvent.getName;
            this._LocationInfo = location;
            this._TimeZone = timeZone;
            this.updateStartTime( timeLine.Start);
            this.updateEndTime( timeLine.End);
            this._RigidSchedule = true;
            this.UniqueID = EventID.GenerateSubCalendarEvent(calendarEventId);
            this._Creator = user;
            this._Users = group;
            this._ProfileOfNow = new NowProfile();
            this._ProfileOfProcrastination = new Procrastination(new DateTimeOffset(), new TimeSpan());
            _isProcrastinateEvent = true;
            this._CalendarEventRange = calendarEvent.StartToEnd;
            this.BusyFrame = new BusyTimeLine(this.UniqueID.ToString(), Start, End);
            this._EventDuration = this.BusyFrame.BusyTimeSpan;
            this._DataBlob = new MiscData();
            this.ParentCalendarEvent = calendarEvent;
        }
    }
}
