using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TilerElements
{
    public class DayTimeLine:TimeLine
    {
        ulong UniversalDayIndex;
        int BoundDayIndex;
        ConcurrentDictionary<string,SubCalendarEvent> AllocatedSubEvents;
        double OccupancyOfTImeLine = 0;
        TimeSpan freeSpace;
        #region Constructor
        public DayTimeLine(DateTimeOffset Start, DateTimeOffset End, ulong UniversalIndex, int BoundedIndex=-1)
        {
            StartTime = Start;
            EndTime = End;
            if (End <= Start)
            {
                //StartTime = MyStartTime;
                EndTime = Start;
            }
            UniversalDayIndex = UniversalIndex;
            BoundDayIndex = BoundedIndex;
            AllocatedSubEvents = new ConcurrentDictionary<string, SubCalendarEvent>();
            freeSpace = EndTime - StartTime;
        }
        #endregion
        #region Function

        public void AddToSubEventList(IEnumerable<SubCalendarEvent> SubEventList)
        {
            Parallel.ForEach(SubEventList, eachSubCal =>//(eachSubCal in SubEventList)
            {
                AllocatedSubEvents.AddOrUpdate(eachSubCal.ID, eachSubCal, (key, value) => eachSubCal);
                
            });
            
            updateOccupancyOfTimeLine();
        }

        public void AddToSubEventList(SubCalendarEvent eachSubCal)
        {
            //Parallel.ForEach(SubEventList, eachSubCal =>//(eachSubCal in SubEventList)
            {
                AllocatedSubEvents.AddOrUpdate(eachSubCal.ID, eachSubCal, (key, value) => eachSubCal);

            }
            //);

            updateOccupancyOfTimeLine();
        }

        public void InitializeSubEventList(List<SubCalendarEvent> SubEventList)
        {
            AllocatedSubEvents = new ConcurrentDictionary<string, SubCalendarEvent>(SubEventList.ToDictionary(obj=>obj.ID,obj=>obj));
            updateOccupancyOfTimeLine();
        }
        public void updateOccupancyOfTimeLine()
        {
            OccupancyOfTImeLine = ((double)SubCalendarEvent.TotalActiveDuration(AllocatedSubEvents.Values).Ticks / (double)TimelineSpan.Ticks);
            freeSpace = TimelineSpan - SubCalendarEvent.TotalActiveDuration(AllocatedSubEvents.Values);
        }

        public List<SubCalendarEvent> getSubEventsInDayTimeLine()
        {
            List<SubCalendarEvent> retValue  =AllocatedSubEvents.Values.ToList();
            return retValue;
        }

        override public TimeLine CreateCopy()
        {
            DayTimeLine CopyTimeLine = new DayTimeLine(this.StartTime, this.EndTime, UniversalDayIndex, BoundDayIndex);
            CopyTimeLine.AllocatedSubEvents = new ConcurrentDictionary<string, SubCalendarEvent>(AllocatedSubEvents);
            CopyTimeLine.OccupancyOfTImeLine = this.OccupancyOfTImeLine;
            /*
            BusyTimeLine[] TempActiveSlotsHolder = new BusyTimeLine[ActiveTimeSlots.Count()];
            for (int i = 0; i < TempActiveSlotsHolder.Length;i++ )
            {
                TempActiveSlotsHolder[i] = ActiveTimeSlots[i].CreateCopy();
            }

            CopyTimeLine.ActiveTimeSlots = TempActiveSlotsHolder;
            */
            return CopyTimeLine;
        }

        


        #endregion
        #region Properties

        public TimeSpan TotalFreeSpace
        {
            get
            {
                return freeSpace;
            }

        }
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
