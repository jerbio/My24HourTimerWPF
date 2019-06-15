using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{

    /// <summary>
    /// represents the Timeline of restriction
    /// </summary>
    public class RestrictionTimeLine: IUndoable
    {
        //ToDo restriction timeline needs to include a day component
        string _Id;
        static TimeSpan TwentyFourHourTImeSpan = TimeSpan.FromDays(1);
        protected DateTimeOffset StartTimeOfDay;
        protected TimeSpan RangeTimeSpan;   
        protected DateTimeOffset EndTimeOfDay;

        #region undoMembers
        public DateTimeOffset UndoStartTimeOfDay;
        public TimeSpan UndoRangeTimeSpan;
        public DateTimeOffset UndoEndTimeOfDay;
        public string _UndoId;
        #endregion

        protected RestrictionTimeLine()
        {
            StartTimeOfDay = new DateTimeOffset();
            RangeTimeSpan = new TimeSpan();
            EndTimeOfDay = new DateTimeOffset();
        }

        public RestrictionTimeLine(DateTimeOffset Start, DateTimeOffset End)
        {
            StartTimeOfDay = new DateTimeOffset(1, 1, 1, Start.Hour, Start.Minute, Start.Second, new TimeSpan());
            End = End <= Start ? End.AddDays(1) : End;
            RangeTimeSpan = End - Start;
            if(RangeTimeSpan > TwentyFourHourTImeSpan)
            {
                throw new Exception("RestrictionTimeLine cannot have a time span more than twenty four hours");
            }
            EndTimeOfDay = StartTimeOfDay.Add(RangeTimeSpan);
        }


        public RestrictionTimeLine(DateTimeOffset Start, TimeSpan SpanDuration)
        {
            StartTimeOfDay = new DateTimeOffset(1, 1, 1, Start.Hour, Start.Minute, Start.Second, new TimeSpan());
            RangeTimeSpan = SpanDuration;
            if (RangeTimeSpan > TwentyFourHourTImeSpan)
            {
                throw new Exception("RestrictionTimeLine cannot have a time span more than twenty four hours");
            }
            EndTimeOfDay = StartTimeOfDay.Add(RangeTimeSpan);
        }

        public DateTimeOffset getInjectedStartHourMinIntoDateTime(DateTimeOffset refDateTimeOffset)
        {
            DateTimeOffset retValue = new DateTimeOffset(refDateTimeOffset.Year, refDateTimeOffset.Month, refDateTimeOffset.Day, StartTimeOfDay.Hour, StartTimeOfDay.Minute, StartTimeOfDay.Second, new TimeSpan());
            return retValue;
        }

        public DateTimeOffset getInjectedEndHourMinIntoDateTime(DateTimeOffset refDateTimeOffset)
        {
            DateTimeOffset retValue = new DateTimeOffset(refDateTimeOffset.Year, refDateTimeOffset.Month, refDateTimeOffset.Day, StartTimeOfDay.Hour, StartTimeOfDay.Minute, StartTimeOfDay.Second, new TimeSpan()).Add(RangeTimeSpan);
            return retValue;
        }

        public RestrictionTimeLine createCopy()
        {
            RestrictionTimeLine retValue = new RestrictionTimeLine();
            retValue.EndTimeOfDay = this.EndTimeOfDay;
            retValue.RangeTimeSpan = this.RangeTimeSpan;
            retValue.StartTimeOfDay = this.StartTimeOfDay;
            return retValue;
        }

        public TimeLine getTimeLineFromStartFrame(DateTimeOffset Start)
        {
            Start = getInjectedStartHourMinIntoDateTime(Start);
            TimeLine retValue = new TimeLine(Start ,Start .Add(RangeTimeSpan));
            return retValue;
        }

        public TimeLine getTimelineFromEndFrame(DateTimeOffset End)
        { 
            if((End.Hour==EndTimeOfDay.Hour)&&(End.Minute==EndTimeOfDay.Minute))//checks if the the Hour and Time in End parameter matches the end of Object. This avoids errors that might occur with using invalid entimes that dont match the restriction timeline
            {
                TimeLine retValue = new TimeLine(End.Add(-RangeTimeSpan), End);
                return retValue;
            }
            throw new Exception("Invalid End Datetimeoffset used for restriction timeline");
        }

        public DateTimeOffset getSampleTestTIme()
        {
            return StartTimeOfDay;
        }

        public override string ToString()
        {
            string retValue = StartTimeOfDay + "-" + EndTimeOfDay + "||" + RangeTimeSpan;
            return retValue;

        }

        public void undoUpdate(Undo undo)
        {
            UndoStartTimeOfDay = StartTimeOfDay;
            UndoRangeTimeSpan = RangeTimeSpan;
            UndoEndTimeOfDay = EndTimeOfDay;
            _UndoId = undo.id;
            FirstInstantiation = false;
        }

        public void undo(string undoId)
        {
            if(undoId == UndoId)
            {
                Utility.Swap(ref UndoStartTimeOfDay, ref StartTimeOfDay);
                Utility.Swap(ref UndoRangeTimeSpan, ref RangeTimeSpan);
                Utility.Swap(ref UndoEndTimeOfDay, ref EndTimeOfDay);
            }
        }

        public void redo(string undoId)
        {
            if (undoId == UndoId)
            {
                Utility.Swap(ref UndoStartTimeOfDay, ref StartTimeOfDay);
                Utility.Swap(ref UndoRangeTimeSpan, ref RangeTimeSpan);
                Utility.Swap(ref UndoEndTimeOfDay, ref EndTimeOfDay);
            }
        }

        #region Properties
        public DateTimeOffset Start
        {
            set
            {
                StartTimeOfDay = value;
            }
            get 
            {
                return StartTimeOfDay;
            }
        }

        public TimeSpan Span
        {
            set
            {
                RangeTimeSpan = value;
            }
            get
            {
                return RangeTimeSpan;
            }
        }

        public DateTimeOffset End
        {
            set
            {
                EndTimeOfDay = value;
            }
            get
            {
                return EndTimeOfDay;
            }
        }

        public string Id
        {
            get
            {
                return _Id ?? (_Id = Guid.NewGuid().ToString());
            }
            set
            {
                _Id = value;
            }
        }

        public virtual bool FirstInstantiation { get; set; } = true;

        public string UndoId
        {
            set
            {
                _UndoId = value;
            }
            get
            {
                return _UndoId;
            }
        }
        #endregion
    }
}
