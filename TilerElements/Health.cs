using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    /// <summary>
    /// Class analyses the health of a schedule
    /// </summary>
    public class Health
    {
        public TimeSpan EvaluationSpan = new TimeSpan(7, 0, 0, 0, 0);
        public TimeLine CalculationTimeline;
        ReferenceNow Now;
        List<SubCalendarEvent> orderedByStartThenEndSubEvents = new List<SubCalendarEvent>();
        
        public Health(IEnumerable<SubCalendarEvent> AllEvents, DateTimeOffset startTime, TimeSpan evaluationSpan, ReferenceNow now)
        {
            IEnumerable<SubCalendarEvent> SubEvents = AllEvents;
            CalculationTimeline = new TimeLine(startTime, startTime.Add(EvaluationSpan));
            orderedByStartThenEndSubEvents = SubEvents.Where(SubEvent => SubEvent.RangeTimeLine.InterferringTimeLine(CalculationTimeline) != null).OrderBy(obj => obj.Start).ThenByDescending(tilerEvent => tilerEvent.End).ToList();
            EvaluationSpan = evaluationSpan;
            Now = now;
        }

        public Health(IEnumerable<CalendarEvent> AllEvents, DateTimeOffset startTime, TimeSpan evaluationSpan, ReferenceNow refNow) : this(AllEvents.SelectMany(CalEvent => CalEvent.ActiveSubEvents), startTime, evaluationSpan, refNow)
        {
        }

        public double getScore()
        {
            double totalDistance = evaluateTotalDistance();
            double positioningScore = evaluatePositioning();
            double retValue = Utility.CalcuateResultant(totalDistance, positioningScore);
            return retValue;
        }

        public double evaluateTotalDistance()
        {
            double retValue = Location_Elements.calculateDistance(orderedByStartThenEndSubEvents.Select(SubEvent => SubEvent.myLocation).ToList());
            return retValue;
        }

        public double evaluatePositioning()
        {
            double retValue = 0;
            if(orderedByStartThenEndSubEvents.Count > 0)
            {
                EventID lastId = orderedByStartThenEndSubEvents[0].SubEvent_ID;
                int indexCounter = 0;
                for (int i = 1; i < orderedByStartThenEndSubEvents.Count; i++)
                {
                    SubCalendarEvent SubEvent = orderedByStartThenEndSubEvents[i];
                    EventID iterationId = SubEvent.SubEvent_ID;
                    if (iterationId.getCalendarEventComponent() .Equals(lastId.getCalendarEventComponent()))
                    {
                        ++indexCounter;
                        retValue += (double) Utility.getFibonnacciNumber((uint)indexCounter);
                    }
                    else
                    {
                        indexCounter = 0;
                    }
                    lastId = iterationId;
                }
            }

            return retValue;
        }

        Dictionary<TimeLine, TimeLine> evaluateSleepSchedule(IEnumerable<DayTimeLine> dayTimeLines)
        {
            List<SubCalendarEvent> OrderexListOfTilerEvents = this.orderedByStartThenEndSubEvents.ToList();
            List<TimeLine> daysSortedBystart = dayTimeLines.OrderBy(obj => obj.Start).Select(daytimeLine => daytimeLine.getJustTimeLine()).ToList();
            Dictionary<TimeLine, TimeLine> retValue = new Dictionary<TimeLine, TimeLine>();
            for (int i = 0; i < daysSortedBystart.Count; i++)
            {
                TimeLine timeLine = daysSortedBystart[i];
                List<SubCalendarEvent> interferringEvents = OrderexListOfTilerEvents.Where(tilerEvent => tilerEvent.RangeTimeLine.doesTimeLineInterfere(timeLine)).ToList();
                List<BusyTimeLine> allBusySlots = interferringEvents.Select(tilerEvent => new BusyTimeLine(tilerEvent.Id, tilerEvent.Start, tilerEvent.End)).ToList();
                timeLine.AddBusySlots(allBusySlots);
                foreach (SubCalendarEvent tilerEvent in interferringEvents.Where(tilerEvent => tilerEvent.End < timeLine.End))
                {
                    OrderexListOfTilerEvents.Remove(tilerEvent);
                }
            }
            retValue = daysSortedBystart.ToDictionary(timeLine => timeLine, timeLine => timeLine.getAllFreeSlots().OrderByDescending(obj => obj.TimelineSpan.Ticks).First());
            return retValue;
        }

        public Double TotalDistance {
            get
            {
                return evaluateTotalDistance();
            }
        }

        public TimeSpan SleepPerDay {
            get
            {
                IEnumerable<DayTimeLine> dayTimeLines = Now.getAllDaysCount(7);
                Dictionary<TimeLine, TimeLine> dayPerTimeLine = evaluateSleepSchedule(dayTimeLines);
                List <TimeLine> validTimeLines = dayPerTimeLine.Select(keyValuePair => keyValuePair.Value).ToList();
                TimeSpan totalSum = new TimeSpan();
                foreach (TimeSpan span in validTimeLines.Select(timeLine => timeLine.TimelineSpan))
                {
                    totalSum.Add(span);
                }
                TimeSpan retValue = TimeSpan.FromTicks( totalSum.Ticks / validTimeLines.Count);
                return retValue;
            }
        }

    }
}
