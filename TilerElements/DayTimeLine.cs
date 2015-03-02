using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class DayTimeLine:TimeLine
    {
        ulong UniversalDayIndex;
        int BoundDayIndex;
        List<SubCalendarEvent> AllocatedSubEvents;
        double OccupancyOfTImeLine = 0;
        #region Constructor
        public DayTimeLine(DateTimeOffset Start, DateTimeOffset End, ulong UniversalIndex, int BoundedIndex=-1)
        {
            StartTime = Start;
            EndTime = End;
            UniversalDayIndex = UniversalIndex;
            BoundDayIndex = BoundedIndex;
            AllocatedSubEvents = new List<SubCalendarEvent>();
        }
        #endregion
        #region Function

        public void updateSubEventList(List<SubCalendarEvent> SubEventList)
        {
            AllocatedSubEvents = SubEventList.ToList();
            OccupancyOfTImeLine = (double)(SubCalendarEvent.TotalActiveDuration(AllocatedSubEvents).Ticks / RangeSpan.Ticks);
        }

        public DayTimeLine CreateCopy()
        {
            DayTimeLine CopyTimeLine = new DayTimeLine(this.StartTime, this.EndTime, UniversalDayIndex, BoundDayIndex);
            CopyTimeLine.AllocatedSubEvents  = AllocatedSubEvents.ToList();
            BusyTimeLine[] TempActiveSlotsHolder = new BusyTimeLine[ActiveTimeSlots.Count()];
            for (int i = 0; i < TempActiveSlotsHolder.Length;i++ )
            {
                TempActiveSlotsHolder[i] = ActiveTimeSlots[i].CreateCopy();
            }

            CopyTimeLine.ActiveTimeSlots = TempActiveSlotsHolder;
            return CopyTimeLine;
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


        public double Occupancy
        {
            get
            {
                return OccupancyOfTImeLine;
            }
        }


        #endregion

    }
}
