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
        ConcurrentBag<int>[] EventCountPerDay;
        ConcurrentBag<TimeSpan>[] SpanPerDay;

        public EventDayBags(uint dayCount)
        {
            DayCount = dayCount;
            BagPerDay = new DayBag [DayCount];
            for (int i=0; i<dayCount ; i++)
            {
                BagPerDay[i] = new DayBag();
            }
            EventCountPerDay = new ConcurrentBag<int>[DayCount];
            SpanPerDay = new ConcurrentBag<TimeSpan>[DayCount];
        }

        public void reset()
        {
            BagPerDay = new DayBag[DayCount];
            for (int i = 0; i < DayCount; i++)
            {
                BagPerDay[i] = new DayBag();
            }
            EventCountPerDay = new ConcurrentBag<int>[DayCount];
            SpanPerDay = new ConcurrentBag<TimeSpan>[DayCount];
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
        ConcurrentBag<TilerEvent> SubEvents;
        TimeSpan TotalSpan = new TimeSpan();
        int CountOfSubEvents = 0;

        double _Score = 0;

        public DayBag ()
        {
            SubEvents = new ConcurrentBag<TilerEvent>();
            TotalSpan = new TimeSpan();
            CountOfSubEvents = 0;
        }

        public void addSubEvent (TilerEvent tilerEvent)
        {
            SubEvents.Add(tilerEvent);
            ++CountOfSubEvents;
            TotalSpan.Add(tilerEvent.getActiveDuration);
            updateScore();
        }

        public IEnumerable<TilerEvent> getTilerEvents ()
        {
            return SubEvents;
        }

        public void updateScore()
        {
            double sumOfSquares = Math.Pow(TotalSpan.Hours, 2) + Math.Pow(SubEvents.Count, 2);
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
