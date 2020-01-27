using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class EventTimeLine : TimeLine, IUndoable, IHasId
    {
        protected string TimeLineEventID = "";
        protected string _UndoId = "";

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
            ActiveTimeSlots = new System.Collections.Concurrent.ConcurrentBag<BusyTimeLine>( myActiveTimeSlots);
            TimeLineEventID = MyEventID;
        }

        public void undoUpdate(Undo undo)
        {
            UndoStartOfTimeLine = StartTime;
            UndoEndOfTimeLine = EndTime;
            FirstInstantiation = false;
        }

        public virtual void undo(string undoId)
        {
            if(undoId == this.UndoId)
            {
                DateTimeOffset end = EndTime;
                DateTimeOffset start = StartTime;
                StartTime = this.UndoStartOfTimeLine;
                EndTime = this.UndoEndOfTimeLine;
                UndoStartOfTimeLine = start;
                UndoEndOfTimeLine = end;
            }
        }

        public virtual void redo(string undoId)
        {
            if (undoId == this.UndoId)
            {
                DateTimeOffset end = EndTime;
                DateTimeOffset start = StartTime;
                StartTime = this.UndoStartOfTimeLine;
                EndTime = this.UndoEndOfTimeLine;
                UndoStartOfTimeLine = start;
                UndoEndOfTimeLine = end;
            }
        }


        #region properties
        public string TimeLineID
        {
            get
            {
                return TimeLineEventID;
            }
        }
        //[NotMapped]
        override public BusyTimeLine[] OccupiedSlots
        {
            set
            {
                ActiveTimeSlots = new System.Collections.Concurrent.ConcurrentBag<BusyTimeLine>( value);
            }
            get
            {
                return ActiveTimeSlots.ToArray();
            }
        }

        #region dbProperties
        [Key]
        public virtual string Id
        {
            set
            {
                TimeLineEventID = value;
            }
            get
            {
                return TimeLineEventID;
            }
        }
        public DateTimeOffset StartOfTimeLine
        {
            get
            {
                return this.StartTime;
            }
            set
            {
                this.StartTime = value;
            }
        }
        public DateTimeOffset EndOfTimeLine
        {
            get
            {
                return this.EndTime;
            }
            set
            {
                this.EndTime = value;
            }
        }

        public virtual DateTimeOffset UndoStartOfTimeLine{ get;set;}
        public virtual DateTimeOffset UndoEndOfTimeLine { get; set; }

        public virtual string UndoId
        {
            get
            {
                return _UndoId;
            }
            set
            {
                _UndoId = value;
            }
        }

        public virtual bool FirstInstantiation { get; set; } = true;
        #endregion
        #endregion
    }

}
