using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;

namespace TilerElements
{
    public class GoogleCalendarEvent : ThirdPartyCalendarEvent
    {
        protected GoogleCalendarEvent(): base()
        {

        }
        public GoogleCalendarEvent(SubCalEvent SubCalData)
        {
            DateTimeOffset Start = (new DateTimeOffset()).Add(Utility.StartOfTimeTimeSpan).AddMilliseconds(SubCalData.SubCalStartDate);
            DateTimeOffset End = (new DateTimeOffset()).Add(Utility.StartOfTimeTimeSpan).AddMilliseconds(SubCalData.SubCalEndDate);
            _Creator = new GoogleTilerUser(SubCalData.ThirdPartyUserID);
            this._Name = new EventName( this.Creator_EventDB, null, SubCalData.SubCalCalendarName!=null?SubCalData.SubCalCalendarName:"");
            StartDateTime = Start;
            EndDateTime = End;
            _Splits = 1;
            _RigidSchedule = true;
            UniqueID = new EventID(new EventID(SubCalData.ID).getRepeatCalendarEventID());
            _RigidSchedule = true;

            _EventPreDeadline = new TimeSpan();
            _Priority = SubCalData.Priority;
            _Enabled = true;
            _Complete = false;
            _EventDuration = End - Start;
            _LocationInfo = String.IsNullOrEmpty(SubCalData.SubCalAddressDescription) ? new TilerElements.Location() : new TilerElements.Location(SubCalData.SubCalAddressDescription);
            _ThirdPartyFlag = true;

            ThirdPartyTypeInfo = ThirdPartyControl.CalendarTool.google;
            _otherPartyID = SubCalData.ThirdPartyEventID;
            _Users = new TilerUserGroup();
            _DataBlob = new MiscData();
            _ProfileOfNow = new NowProfile();
            SubCalendarEvent mySubCal = GoogleSubCalendarEvent.convertFromGoogleToSubCalendarEvent( SubCalData, _LocationInfo);//.convertFromGoogleToSubCalendarEvent();
            mySubCal.ParentCalendarEvent = this;
            _SubEvents = new SubEventDictionary<string, SubCalendarEvent>();
            _SubEvents.Collection.Add(mySubCal.Id, mySubCal);
        }

        public GoogleCalendarEvent(IEnumerable<CalendarEvent> AllCalendarEvent, TilerUser user)
        {
            this._EventDuration = new TimeSpan(50);
            this._Splits = 1;
            this._AverageTimePerSplit = _EventDuration;
            this._UiParams = new EventDisplay();
            this.UnDesignables = new HashSet<SubCalendarEvent>();
            this.UniqueID = EventID.generateGoogleCalendarEventID((uint)AllCalendarEvent.Count());
            this._UserDeleted = false;
            this._Users = new TilerUserGroup();
            this.StartDateTime = DateTimeOffset.Now.AddDays(-90);
            this.EndDateTime = this.StartDateTime.AddDays(180);
            this._Enabled = true;
            this._Complete = false;
            this._DeletedCount = 1;
            this._CompletedCount = 1;
            this._Creator = user;
            this._EventRepetition = new Repetition(true, this.RangeTimeLine, "Daily", AllCalendarEvent.ToArray());
            this._Name = new EventName(user, this, "GOOGLE MOTHER EVENT");
            this._DataBlob = new MiscData();
            this._ProfileOfNow = new NowProfile();
            this.ThirdPartyTypeInfo = ThirdPartyControl.CalendarTool.google;
            _SubEvents = new SubEventDictionary<string, SubCalendarEvent>();
        }


        static public CalendarEvent convertFromGoogleToCalendarEvent(SubCalEvent SubCalEventData)
        {
            CalendarEvent RetValue = new GoogleCalendarEvent(SubCalEventData);
            return RetValue;
        }

    }
}
