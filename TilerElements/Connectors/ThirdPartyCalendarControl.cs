using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TilerElements.Wpf;
using System.ComponentModel.DataAnnotations.Schema;
using TilerElements.DB;


namespace TilerElements.Connectors
{
    [Table("ThirdPartyCalendar")]
    public abstract class ThirdPartyCalendarControl
    {
        protected ThirdPartyControl.CalendarTool SelectedCalendarTool;
        protected Dictionary<string, CalendarEvent> IDToCalendarEvent = new Dictionary<string, CalendarEvent>();
        ThirdPartyCalendarEvent ThirdpartyCalendarEventInfo;
        protected ThirdPartyCalendarControl()
        {}
        
        abstract public void DeleteAppointment(SubCalendarEvent ActiveSection, string NameOfParentCalendarEvent="", string entryID="");
        abstract public string AddAppointment(SubCalendarEvent ActiveSection, string NameOfParentCalendarEvent = "");

        public virtual Dictionary<string, CalendarEvent> getAllCalendarEvents()
        {
            return IDToCalendarEvent.ToDictionary(obj => obj.Key, obj => obj.Value);
        }

        void populateThirdpartyCalendarEventInfo ()
        {
            ThirdpartyCalendarEventInfo = new ThirdPartyCalendarEvent(IDToCalendarEvent.Values);

        }

        virtual public CalendarEvent getThirdpartyCalendarEvent()
        {
            if (ThirdpartyCalendarEventInfo==null)
            {
                populateThirdpartyCalendarEventInfo();
            }
            return ThirdpartyCalendarEventInfo;
        }

    }

    class ThirdPartyCalendarEvent:CalendarEvent
    {
        public ThirdPartyCalendarEvent(IEnumerable<CalendarEvent>AllCalendarEvent)
        {
            this.EventDuration = new TimeSpan(50);
            this.Splits = 1;
            this.TimePerSplit = EventDuration;
            this.UiParams = new EventDisplay();
            this.UnDesignables = new HashSet<SubCalendarEvent>();
            this.UniqueID = EventID.generateGoogleCalendarEventID((uint)AllCalendarEvent.Count());
            this.UserDeleted = false;
            this.UserIDs = new List<TilerUser>();
            this.StartDateTime = DateTimeOffset.Now.AddDays(-90);
            this.EndDateTime = this.StartDateTime.AddDays(180);
            this.Enabled = true;
            this.Complete = false;
            this.DeletedCount = 1;
            this.CompletedCount = 1;
            this.EventRepetition = new DB_Repetition(true, this.RangeTimeLine, "Daily", AllCalendarEvent.ToArray());
            this.NameOfEvent = new EventName(this.UniqueID,  "GOOGLE MOTHER EVENT");
            this.ProfileOfNow = new NowProfile();
        }
    }
}
