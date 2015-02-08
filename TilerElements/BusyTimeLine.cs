using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class BusyTimeLine : EventTimeLine
    {
        TimeSpan BusySpan;

        public BusyTimeLine()
            : base()
        {

        }
        public BusyTimeLine(TimeSpan MyBusySpan)
        {
            BusySpan = MyBusySpan;
            TimeLineEventID = null;
        }
        public BusyTimeLine(string MyEventID, DateTimeOffset MyStartTime, DateTimeOffset MyEndTime)
        {
            StartTime = MyStartTime;
            EndTime = MyEndTime;
            BusySpan = EndTime - StartTime;
            TimeLineEventID = MyEventID;
        }

        #region functions
        public BusyTimeLine CreateCopy()
        {
            BusyTimeLine MyBusyTimlineCopy = new BusyTimeLine(this.TimeLineID, StartTime, EndTime);
            return MyBusyTimlineCopy;
        }

        public void shiftTimeline(TimeSpan ChangeInTime)
        {
            StartTime += ChangeInTime;
            EndTime += ChangeInTime;
        }

        public BusyTimeLine updateBusyTimeLine(BusyTimeLine BusyTimeLineEntry)
        {
            StartTime = BusyTimeLineEntry.StartTime;
            EndTime = BusyTimeLineEntry.EndTime;
            TimeLineEventID = BusyTimeLineEntry.ID;
            return this;
        }


        #endregion
        #region Properties
        public TimeSpan BusyTimeSpan
        {
            get
            {
                return BusySpan;
            }
        }

        public string ID
        {
            get
            {
                return this.TimeLineEventID;
            }
        }


        public DateTimeOffset Start
        {
            get
            {
                return StartTime;
            }
        }
        public DateTimeOffset End
        {
            get
            {
                return EndTime;
            }
        }
        #endregion
    }
}
