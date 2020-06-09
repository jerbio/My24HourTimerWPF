using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public abstract class ThirdPartyCalendarEvent : CalendarEvent
    {
        protected ThirdPartyCalendarEvent(): base()
        {

        }

        public ThirdPartyCalendarEvent(IEnumerable<CalendarEvent> AllCalendarEvent, ThirdPartyControl.CalendarTool calendarSource, TilerUser user)
        {
            this._EventDuration = new TimeSpan(50);
            this._Splits = 1;
            this._AverageTimePerSplit = _EventDuration;
            this._UiParams = new EventDisplay();
            this.UnDesignables = new HashSet<SubCalendarEvent>();
            this.UniqueID = EventID.generateGoogleCalendarEventID((uint)AllCalendarEvent.Count());
            this._AutoDeleted = false;
            this._Users = new TilerUserGroup();
            this.updateStartTime( DateTimeOffset.Now.AddDays(-90));
            this.updateEndTime( this.Start.AddDays(180));
            this._Enabled = true;
            this._Complete = false;
            this._DeletedCount = 1;
            this._CompletedCount = 1;
            this._Creator = user;
            this.ThirdPartyTypeInfo = calendarSource;
            this._EventRepetition = new Repetition(true, this.StartToEnd, "Daily", AllCalendarEvent.ToArray());
            this._Name = new EventName(user, this, "GOOGLE MOTHER EVENT");
            this._DataBlob = new MiscData();
            this._ProfileOfNow = new NowProfile();
            this.ThirdPartyTypeInfo = ThirdPartyControl.CalendarTool.google;
            _SubEvents = new SubEventDictionary<string, SubCalendarEvent>();
        }
    }
}
