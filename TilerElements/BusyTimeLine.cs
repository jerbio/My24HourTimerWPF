using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace TilerElements
{
    public class BusyTimeLine : EventTimeLine
    {
        TimeSpan _BusySpan;

        public BusyTimeLine()
            : base()
        {

        }
        public BusyTimeLine(TimeSpan MyBusySpan)
        {
            _BusySpan = MyBusySpan;
            TimeLineEventID = null;
        }
        public BusyTimeLine(string MyEventID, DateTimeOffset MyStartTime, DateTimeOffset MyEndTime)
        {
            if(!String.IsNullOrEmpty(MyEventID))
            {
                StartTime = MyStartTime;
                EndTime = MyEndTime;
                _BusySpan = EndTime - StartTime;
                TimeLineEventID = MyEventID;
            }
            else
            {
                throw new ArgumentNullException("MyEventID");
            }
            
        }

        public BusyTimeLine(string eventID, TimeLine timeLine)
        {
            StartTime = timeLine.Start;
            EndTime = timeLine.End;
            _BusySpan = EndTime - StartTime;
            TimeLineEventID = eventID;
        }

        #region functions
        public override TimeLine CreateCopy()
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
            TimeLineEventID = BusyTimeLineEntry.Id;
            return this;
        }

        public override JObject ToJson()
        {
            var retValue = base.ToJson();
            retValue.Add("id", this.Id);
            return retValue;
        }

        public static BusyTimeLine JobjectToTimeLine(JObject jObject)
        {
            string startTimeString = jObject.GetValue("start").ToString();
            string endTimeString = jObject.GetValue("end").ToString();
            string idString = jObject.GetValue("id").ToString();
            DateTimeOffset start = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(startTimeString));
            DateTimeOffset end = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(endTimeString));


            BusyTimeLine retValue = new BusyTimeLine(idString, start, end);
            return retValue;
        }
        #endregion
        #region Properties
        public TimeSpan BusyTimeSpan
        {
            get
            {
                return _BusySpan;
            }
        }

        public override TimeSpan TotalActiveSpan => this.BusyTimeSpan;

        public TimeSpan BusyTimeSpan_DB
        {
            get
            {
                return _BusySpan;
            }
            set
            {
                _BusySpan = value;
            }
        }

        public TimeSpan UndoBusyTimeSpan_DB;

        public override void undo(string undoId)
        {
            if(this._UndoId == undoId)
            {
                base.undo(undoId);
                Utility.Swap(ref UndoBusyTimeSpan_DB, ref _BusySpan);
            }
        }

        public override void redo(string undoId)
        {
            if (this._UndoId == undoId)
            {
                base.undo(undoId);
                Utility.Swap(ref UndoBusyTimeSpan_DB, ref _BusySpan);
            }
        }

        #endregion
    }
}
