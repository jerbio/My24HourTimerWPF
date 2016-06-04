﻿using System;
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
        public static TimeSpan EvaluationSpan = new TimeSpan(90, 0, 0, 0, 0);
        public TimeLine CalculationTimeline;
        List<SubCalendarEvent> RelevantSubEvents = new List<SubCalendarEvent>();
        public Health(IEnumerable<CalendarEvent> AllEvents, DateTimeOffset startTime)
        {
            IEnumerable<SubCalendarEvent> SubEvents = AllEvents.SelectMany(CalEvent => CalEvent.ActiveSubEvents);
            CalculationTimeline = new TimeLine(startTime, startTime.Add(EvaluationSpan));
            RelevantSubEvents = SubEvents.Where(SubEvent => SubEvent.RangeTimeLine.InterferringTimeLine(CalculationTimeline) != null).OrderBy(obj=>obj.Start).ToList();
        }

        public Health(IEnumerable<SubCalendarEvent> AllEvents, DateTimeOffset startTime)
        {
            IEnumerable<SubCalendarEvent> SubEvents = AllEvents;
            CalculationTimeline = new TimeLine(startTime, startTime.Add(EvaluationSpan));
            RelevantSubEvents = SubEvents.Where(SubEvent => SubEvent.RangeTimeLine.InterferringTimeLine(CalculationTimeline) != null).OrderBy(obj => obj.Start).ToList();
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
            double retValue = Location_Elements.calculateDistance(RelevantSubEvents.OrderBy(SubEvent => SubEvent.Start).Select(SubEvent => SubEvent.myLocation).ToList());
            return retValue;
        }

        public double evaluatePositioning()
        {
            double retValue = 0;
            if(RelevantSubEvents.Count > 0)
            {
                EventID lastId = RelevantSubEvents[0].SubEvent_ID;
                int indexCounter = 0;
                for (int i = 1; i < RelevantSubEvents.Count; i++)
                {
                    SubCalendarEvent SubEvent = RelevantSubEvents[i];
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
    }
}
