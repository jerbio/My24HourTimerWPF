using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;

namespace TilerCore
{
    internal class EventDayBags
    {
        readonly uint DayCount;

        DayBag [] BagPerDay;

        public EventDayBags(uint dayCount)
        {
            DayCount = dayCount;
            BagPerDay = new DayBag [DayCount];
            for (int i=0; i<dayCount ; i++)
            {
                BagPerDay[i] = new DayBag();
            }
        }

        public void removeAllUndesignated()
        {
            for (int i = 0; i < DayCount; i++)
            {
                BagPerDay[i].removeAllUndesignated();
            }
        }

        public void reset()
        {
            BagPerDay = new DayBag[DayCount];
            for (int i = 0; i < DayCount; i++)
            {
                BagPerDay[i] = new DayBag();
            }
        }

        public DayBag this[int i]
        {
            get
            {
                return this.BagPerDay[i];
            }
        }

        public List<DayBag> DayBags()
        {
            return BagPerDay.ToList();
        }
    }

    public class DayBag
    {
        Dictionary<SubCalendarEvent, TimeSpan> DesignatedSubEventsToDuration;// Holds subevents that have being designated so will affect the scoring. If SubEventsToDuration is emptied we lose the designated events which will always be present since already designated
        Dictionary<SubCalendarEvent, TimeSpan> SubEventsToDuration;//Holds sub events that have not been permanently designated
        TimeSpan TotalSpan = new TimeSpan();

        double _Score = 0;

        public DayBag ()
        {
            SubEventsToDuration = new Dictionary<SubCalendarEvent, TimeSpan>();
            DesignatedSubEventsToDuration = new Dictionary<SubCalendarEvent, TimeSpan>();
            TotalSpan = new TimeSpan();
        }

        public void removeAllUndesignated()
        {
            foreach(var kvp in SubEventsToDuration)
            {
                SubCalendarEvent subEvent = kvp.Key;
                if (subEvent.isDesignated)
                {
                    DesignatedSubEventsToDuration.Add(subEvent, kvp.Value);
                }
            }
            SubEventsToDuration = new Dictionary<SubCalendarEvent, TimeSpan>();
            TotalSpan = new TimeSpan();
            foreach(TimeSpan timeSpan in DesignatedSubEventsToDuration.Values)
            {
                TotalSpan += timeSpan;
            }

            SubEventsToDuration = new Dictionary<SubCalendarEvent, TimeSpan>();
        }

        public void addSubEvent (SubCalendarEvent subEvent, TimeSpan duration)
        {
            if(SubEventsToDuration.ContainsKey(subEvent))
            {
                TimeSpan currentDuration = SubEventsToDuration[subEvent];
                TotalSpan -= currentDuration;
                SubEventsToDuration[subEvent] = duration;
            }
            else
            {
                SubEventsToDuration.Add(subEvent,duration);
            }
            
            TotalSpan+=duration;
            updateScore();
        }

        public IEnumerable<SubCalendarEvent> getNewlyAddedSubEvents()
        {
            return SubEventsToDuration.Keys;
        }

        public void updateScore()
        {
            int eventCount = SubEventsToDuration.Count + DesignatedSubEventsToDuration.Count;
            double sumOfSquares = Math.Pow(TotalSpan.TotalHours, 2) + Math.Pow(eventCount, 2);
            _Score = Math.Sqrt(sumOfSquares);
        }


        public double Score
        {
            get
            {
                return _Score;
            }
        }

    }
}
