using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class EventTimeLine : TimeLine
    {
        protected string TimeLineEventID = "";

        public EventTimeLine()
            : base()
        {

        }

        public EventTimeLine(string MyEventID, DateTimeOffset MyStartTime, DateTimeOffset MyEndTime)
        {
            StartTime = MyStartTime;
            EndTime = MyEndTime;
            TimeLineEventID = MyEventID;
        }

        public void PopulateBusyTimeSlot(string MyEventID, BusyTimeLine[] myActiveTimeSlots)
        {
            ActiveTimeSlots = myActiveTimeSlots;
            TimeLineEventID = MyEventID;
        }

        public string TimeLineID
        {
            get
            {
                return TimeLineEventID;
            }
        }

        override public BusyTimeLine[] OccupiedSlots
        {
            set
            {
                ActiveTimeSlots = value;
            }
            get
            {
                return ActiveTimeSlots;
            }
        }
    }

}
