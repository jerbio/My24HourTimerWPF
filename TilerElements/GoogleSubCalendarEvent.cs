using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;

namespace TilerElements
{
    public class GoogleSubCalendarEvent : SubCalendarEvent
    {
        protected GoogleSubCalendarEvent(): base(){
        }
        public GoogleSubCalendarEvent(SubCalEvent SubCalData, TilerElements.Location location = null)
        {
            DateTimeOffset Start = (new DateTimeOffset()).Add(Utility.StartOfTimeTimeSpan).AddMilliseconds(SubCalData.SubCalStartDate);
            DateTimeOffset End = (new DateTimeOffset()).Add(Utility.StartOfTimeTimeSpan).AddMilliseconds(SubCalData.SubCalEndDate);
            _Creator = new GoogleTilerUser(SubCalData.ThirdPartyUserID);
            this._Name = new EventName(_Creator, this.ParentCalendarEvent, SubCalData.SubCalCalendarName != null ? SubCalData.SubCalCalendarName : "");
            updateStartTime( Start);
            updateEndTime(End);
            BusyFrame = new BusyTimeLine(SubCalData.ID, Start, End);
            _CalendarEventRange = new TimeLine(Start, End);
            UniqueID = new EventID(SubCalData.ID);
            _RigidSchedule = true;
            _EventPreDeadline = new TimeSpan();
            _Priority = SubCalData.Priority;
            _ConflictingEvents = new ConflictProfile();
            _Enabled = true;
            _Complete = false;
            _EventDuration = BusyFrame.TimelineSpan;
            _LocationInfo = location?? new TilerElements.Location();
            _ThirdPartyFlag = true;
            ThirdPartyTypeInfo = ThirdPartyControl.CalendarTool.google;
            ThirdPartyUserIDInfo = SubCalData.ThirdPartyUserID;
            _DataBlob = new MiscData();
            _ProfileOfNow = new NowProfile();
            this._Access = SubCalData.isReadOnly ? AccessType.reader : AccessType.owner;
            _Users = new TilerUserGroup()
            {

            };
            _otherPartyID = SubCalData.ThirdPartyEventID;
        }

        static public SubCalendarEvent convertFromGoogleToSubCalendarEvent(SubCalEvent SubCalEventData, TilerElements.Location location = null)
        {
            if (location != null && !String.IsNullOrEmpty(SubCalEventData.SubCalAddress))
            {
                location = new TilerElements.Location(SubCalEventData.SubCalAddress);
            }
            SubCalendarEvent RetValue = new GoogleSubCalendarEvent(SubCalEventData, location);
            return RetValue;
        }
    }

}
