using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class TimelineWithSubcalendarEvents:TimeLine
    {
        protected ConcurrentDictionary<string, SubCalendarEvent> AllocatedSubEvents;
        protected double OccupancyOfTImeLine = 0;
        protected TimeSpan freeSpace;
        protected Location _averageLocation;
        protected TimeSpan _TotalActiveSpan;
        #region Constructor
        public TimelineWithSubcalendarEvents()
        {
            AllocatedSubEvents = new ConcurrentDictionary<string, SubCalendarEvent>();
            _averageLocation = Location.AverageGPSLocation(AllocatedSubEvents.Values.Select(subEvent => subEvent.Location));
            freeSpace = new TimeSpan();
            _TotalActiveSpan = new TimeSpan();
        }
        public TimelineWithSubcalendarEvents(DateTimeOffset Start, DateTimeOffset End, IEnumerable<SubCalendarEvent> subcalendarEvents)
        {
            StartTime = Start;
            EndTime = End;
            if(End <= Start)
            {
                EndTime = Start;
            }
            AllocatedSubEvents = subcalendarEvents == null? new ConcurrentDictionary<string, SubCalendarEvent>() : new ConcurrentDictionary<string, SubCalendarEvent>(subcalendarEvents.ToDictionary(obj => obj.getId, obj => obj));
            _averageLocation = Location.AverageGPSLocation(AllocatedSubEvents.Values.Select(subEvent => subEvent.Location));
            freeSpace = EndTime - StartTime;
            _TotalActiveSpan = new TimeSpan();
            updateOccupancyOfTimeLine();
            evaluateTotalActiveTimeSpan();
        }
        #endregion

        #region Function

        protected void evaluateTotalActiveTimeSpan()
        {
            Utility.ConflictEvaluation ConflictEvaluation = new Utility.ConflictEvaluation(this.OccupiedSlots);
            TimeSpan totalSpan = TimeSpan.FromTicks(ConflictEvaluation.ConflictingTimeRange.Concat(ConflictEvaluation.NonConflictingTimeRange).Select(timeRange => timeRange.RangeTimeLine.TimelineSpan).Sum(timeSpan => timeSpan.Ticks));
            _TotalActiveSpan = totalSpan;
        }

        public virtual void AddToSubEventList(IEnumerable<SubCalendarEvent> SubEventList)
        {
            Parallel.ForEach(SubEventList, eachSubCal =>//(eachSubCal in SubEventList)
            {
                AllocatedSubEvents.AddOrUpdate(eachSubCal.getId, eachSubCal, (key, value) => eachSubCal);
            });

            foreach (SubCalendarEvent eachSubCal in SubEventList)
            {
                base.AddBusySlots(eachSubCal.ActiveSlot);
            }

            updateOccupancyOfTimeLine();
            updateAverageLocation();
            evaluateTotalActiveTimeSpan();
        }

        public virtual void AddToSubEventList(SubCalendarEvent eachSubCal)
        {
            {
                AllocatedSubEvents.AddOrUpdate(eachSubCal.getId, eachSubCal, (key, value) => eachSubCal);
                base.AddBusySlots(eachSubCal.ActiveSlot);
            }
            updateOccupancyOfTimeLine();
            updateAverageLocation();
            evaluateTotalActiveTimeSpan();
        }

        public virtual void InitializeSubEventList(List<SubCalendarEvent> SubEventList)
        {
            AllocatedSubEvents = new ConcurrentDictionary<string, SubCalendarEvent>(SubEventList.ToDictionary(obj => obj.getId, obj => obj));
            updateOccupancyOfTimeLine();
        }
        public virtual void updateOccupancyOfTimeLine()
        {
            OccupancyOfTImeLine = ((double)(SubCalendarEvent.TotalActiveDuration(AllocatedSubEvents.Values).Ticks + ActiveTimeSlots.Sum(obj => obj.BusyTimeSpan.Ticks)) / (double)TimelineSpan.Ticks);
            freeSpace = TimelineSpan - SubCalendarEvent.TotalActiveDuration(AllocatedSubEvents.Values);
            OccupiedSlots = AllocatedSubEvents.Select(obj => obj.Value.ActiveSlot).ToArray();
        }

        public virtual void updateAverageLocation()
        {
            _averageLocation = Location.AverageGPSLocation(AllocatedSubEvents.Values.Select(subEvent => subEvent.Location));
        }

        public virtual List<SubCalendarEvent> getSubEventsInTimeLine()
        {
            List<SubCalendarEvent> retValue = AllocatedSubEvents.Values.ToList();
            return retValue;
        }

        override public TimeLine CreateCopy()
        {
            TimelineWithSubcalendarEvents CopyTimeLine = new TimelineWithSubcalendarEvents(this.StartTime, this.EndTime, this.AllocatedSubEvents.Values);
            CopyTimeLine.AllocatedSubEvents = new ConcurrentDictionary<string, SubCalendarEvent>(AllocatedSubEvents);
            CopyTimeLine.OccupancyOfTImeLine = this.OccupancyOfTImeLine;
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
            return (base.ToString() + " || Duration:" + (EndTime - StartTime).ToString());
        }

        /// <summary>
        /// Function returns a timeLine with the begining and end. It does not populate the busyslots. It returns a new TImeline object. If you want the Active slots to be included, you can call add busy slots
        /// </summary>
        /// <returns></returns>
        virtual public TimeLine getJustTimeLine(bool includeActiveSlots = false)
        {
            TimeLine RetValue = new TimeLine(this.StartTime, this.EndTime);
            if (includeActiveSlots)
            {
                RetValue.AddBusySlots(AllocatedSubEvents.Values.Select(subCal => subCal.ActiveSlot));
            }
            return RetValue;
        }

        #endregion

        #region Properties

        public override TimeSpan TotalFreeSpotAvailable
        {
            get
            {
                return freeSpace;
            }

        }

        virtual public double Occupancy
        {
            get
            {
                return OccupancyOfTImeLine;
            }
        }

        virtual  public Location averageLocation
        {
            get
            {
                return _averageLocation;
            }
        }

        public override TimeSpan TotalActiveSpan
        {
            get
            {
                return _TotalActiveSpan;
            }
        }
        #endregion

    }
}
