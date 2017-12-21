using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TilerElements
{
    public class DayTimeLine: TimelineWithSubcalendarEvents
    {
        ulong UniversalDayIndex;
        int BoundDayIndex;
        #region Constructor
        public DayTimeLine(DateTimeOffset Start, DateTimeOffset End, ulong UniversalIndex, int BoundedIndex=-1):base(Start, End, null)
        {
            AllocatedSubEvents = new ConcurrentDictionary<string, SubCalendarEvent>();
            freeSpace = EndTime - StartTime;
            this.UniversalDayIndex = UniversalIndex;
            this.BoundDayIndex = BoundedIndex;
        }
        #endregion
        #region functions
        override public TimeLine CreateCopy()
        {
            DayTimeLine CopyTimeLine = new DayTimeLine(this.StartTime, this.EndTime, UniversalDayIndex, BoundDayIndex);
            CopyTimeLine.AllocatedSubEvents = new ConcurrentDictionary<string, SubCalendarEvent>(AllocatedSubEvents);
            CopyTimeLine.OccupancyOfTImeLine = this.OccupancyOfTImeLine;
            return CopyTimeLine;
        }
        public override void updateOccupancyOfTimeLine()
        {
            base.updateOccupancyOfTimeLine();
        }
        #endregion

        #region Properties
        public int BoundedIndex
        {
            get 
            {
                return BoundDayIndex;
            }
        }

        public ulong UniversalIndex
        {
            get 
            {
                return UniversalDayIndex;
            }
        }

        public TimeLineWithEdgeElements SleepTimeLine
        {
            get
            {
                TimeLineWithEdgeElements retValue = new TimeLineWithEdgeElements(this.Start, this.Start, "", "");
                DateTimeOffset start = this.Start;
                DateTimeOffset end = this.End;
                string idToEnd = "";
                BusyTimeLine timeLine = this.OrderedOcupiedSlots.FirstOrDefault();
                if(timeLine!=null)
                {
                    end = timeLine.Start;
                    idToEnd = timeLine.ID;
                    if(end < start)
                    {
                        return retValue;
                    }
                }

                retValue = new TimeLineWithEdgeElements(start, end, "", idToEnd);
                return retValue;
            }
        }
        #endregion

    }
}
