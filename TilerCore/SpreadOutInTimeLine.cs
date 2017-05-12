using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TilerElements;



namespace TilerCore
{
    public class SpreadOutInTimeLine
    {
        List<TimelineWithSubcalendarEvents> Timelines;
        List<SubCalendarEvent> SubcalendarEvents;
        public SpreadOutInTimeLine (List<TimelineWithSubcalendarEvents> timeline, IEnumerable<SubCalendarEvent> subcalEvents)
        {
            Timelines = timeline.ToList();
            SubcalendarEvents = subcalEvents.ToList();
        }

        public TimelineWithSubcalendarEvents evaluateTimeLineToSubEvent(SubCalendarEvent subEvent)
        {
            List<IList<double>> multiDimensionalClaculation = new List<IList<double>>();
            foreach (TimelineWithSubcalendarEvents timeline in Timelines)
            {
                double distance = Location.calculateDistance(timeline.averageLocation, subEvent.Location);
                double tickRatio = (double)subEvent.getActiveDuration.Ticks / timeline.TotalFreeSpotAvailable.Ticks;
                double occupancy = (double)timeline.Occupancy;
                IList<double> dimensionsPerDay = new List<double>() { distance, tickRatio, occupancy };
                multiDimensionalClaculation.Add(dimensionsPerDay);
            }
            List<double> foundIndexes = Utility.multiDimensionCalculationNormalize(multiDimensionalClaculation);
            int bestIndex = foundIndexes.MinIndex();
            TimelineWithSubcalendarEvents retValue = Timelines[bestIndex];
            return retValue;
        }
    }
}
