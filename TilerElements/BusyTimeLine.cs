using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            StartTime = MyStartTime;
            EndTime = MyEndTime;
            _BusySpan = EndTime - StartTime;
            TimeLineEventID = MyEventID;
        }

        public BusyTimeLine(string eventID, TimeLine timeLine)
        {
            StartTime = timeLine.Start;
            EndTime = timeLine.End;
            _BusySpan = EndTime - StartTime;
            TimeLineEventID = eventID;
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
                return _BusySpan;
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
