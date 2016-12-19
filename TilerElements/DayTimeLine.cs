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
                AllocatedSubEvents.AddOrUpdate(eachSubCal.Id, eachSubCal, (key, value) => eachSubCal);    
            });

            foreach(SubCalendarEvent eachSubCal in SubEventList)
            {
                base.AddBusySlots(eachSubCal.ActiveSlot);
            }
            
            updateOccupancyOfTimeLine();
        }

        public void AddToSubEventList(SubCalendarEvent eachSubCal)
        {
            //Parallel.ForEach(SubEventList, eachSubCal =>//(eachSubCal in SubEventList)
            {
                AllocatedSubEvents.AddOrUpdate(eachSubCal.Id, eachSubCal, (key, value) => eachSubCal);
                base.AddBusySlots(eachSubCal.ActiveSlot);
            }
            //);
            updateOccupancyOfTimeLine();
            
        }

        public void InitializeSubEventList(List<SubCalendarEvent> SubEventList)
        {
            AllocatedSubEvents = new ConcurrentDictionary<string, SubCalendarEvent>(SubEventList.ToDictionary(obj=>obj.Id,obj=>obj));
            updateOccupancyOfTimeLine();
        }
        public void updateOccupancyOfTimeLine()
        {
            OccupancyOfTImeLine = ((double)(SubCalendarEvent.TotalActiveDuration(AllocatedSubEvents.Values).Ticks+ ActiveTimeSlots.Sum(obj=>obj.BusyTimeSpan.Ticks)) / (double)TimelineSpan.Ticks);
            freeSpace = TimelineSpan - SubCalendarEvent.TotalActiveDuration(AllocatedSubEvents.Values);
            OccupiedSlots = AllocatedSubEvents.Select(obj => obj.Value.ActiveSlot).ToArray();
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
        

        public override void AddBusySlots(BusyTimeLine busyTimeLine)
        {
            ///This exception is thrown so we can enforce the calculation of coccupancy. In daytimeline we want the occupancy to be strongly related to subevents and not simply timelines
            throw new Exception("Cannot simply add  busytimeline to daytime it needs to be a subevent. You need to call AddToSubEventList");
        }

        
        public override void AddBusySlots(IEnumerable<BusyTimeLine> MyBusySlot)
        {
            ///This exception is thrown so we can enforce the calculation of coccupancy. In daytimeline we want the occupancy to be strongly related to subevents and not simply timelines
            throw new Exception("Cannot simply add  busytimelines to daytime it needs to be a subevent. You need to call AddToSubEventList");
        }

        public override string ToString()
        {
            return (base.ToString()+"||"+UniversalDayIndex.ToString());
        }
        
        /// <summary>
        /// Function returns a timeLine with the begining and end. It does not populate the busyslots. It returns a new TImeline object. If you want the Active slots to be included, you can call add busy slots
        /// </summary>
        /// <returns></returns>
        virtual public TimeLine getJustTimeLine(bool includeActiveSlots = false)
        {
            TimeLine RetValue = new TimeLine(this.StartTime, this.EndTime);
            if(includeActiveSlots)
            {
                RetValue.AddBusySlots(AllocatedSubEvents.Values.Select(subCal => subCal.ActiveSlot));
            }
            return RetValue;
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
