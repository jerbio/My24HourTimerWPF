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
        #region Constructor
        public DayTimeLine(DateTimeOffset Start, DateTimeOffset End, int BoundedIndex, ulong UniversalIndex)
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

        #endregion

    }
}
