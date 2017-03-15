using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleMapsApi.Entities.Directions.Request;

namespace TilerElements
{
    /// <summary>
    /// Class analyses the health of a schedule
    /// </summary>
    public class Health : IComparable<Health>
    {
        public TimeSpan EvaluationSpan = new TimeSpan(7, 0, 0, 0, 0);
        public TimeLine CalculationTimeline;
        ReferenceNow Now;
        List<SubCalendarEvent> _orderedByStartThenEndSubEvents = new List<SubCalendarEvent>();
        GoogleMapsApi.Entities.Directions.Request.TravelMode _TravelMode;
        bool alreadyEvaluated = false;
        HealthEvaluation evaluation;
        Location _HomeLocation;

        /// <summary>
        /// This ge
        /// </summary>
        /// <param name="AllEvents">All subcalendar Events needed for the calculation</param>
        /// <param name="startTime">Time from which to start the schedule evaluation</param>
        /// <param name="evaluationSpan">timespan for the range of the calculation</param>
        /// <param name="now">Reference now time to be used as the frame for evaluation</param>
        /// <param name="travelmode">The mode of travel used for evaluation</param>
        public Health(IEnumerable<SubCalendarEvent> AllEvents, DateTimeOffset startTime, TimeSpan evaluationSpan, ReferenceNow now, Location homeLocation, TravelMode travelmode = TravelMode.Driving)
        {
            _HomeLocation = homeLocation ?? Location.getDefaultLocation();
            IEnumerable<SubCalendarEvent> SubEvents = AllEvents;
            CalculationTimeline = new TimeLine(startTime, startTime.Add(EvaluationSpan));
            _orderedByStartThenEndSubEvents = SubEvents.Where(SubEvent => SubEvent.RangeTimeLine.InterferringTimeLine(CalculationTimeline) != null).OrderBy(obj => obj.Start).ThenByDescending(tilerEvent => tilerEvent.End).ToList();
            EvaluationSpan = evaluationSpan;
            Now = now;
            this._TravelMode = travelmode;
        }


        public Health(IEnumerable<CalendarEvent> AllEvents, DateTimeOffset startTime, TimeSpan evaluationSpan, ReferenceNow refNow, Location homeLocation, TravelMode travelMode = TravelMode.Driving) : this(AllEvents.SelectMany(CalEvent => CalEvent.ActiveSubEvents), startTime, evaluationSpan, refNow, homeLocation, travelMode)
        {
        }

        public double getScore()
        {
            double totalDistance = evaluateTotalDistance();
            double positioningScore = evaluatePositioning();
            double conflictScore = evaluateConflicts().Sum(blob => blob.getSubCalendarEventsInBlob().Count());
            double retValue = Utility.CalcuateResultant(totalDistance, positioningScore);
            return retValue;
        }

        public double evaluateTotalDistance(bool includeReturnHome = true)
        {
            double retValue = 0;
            
            if (includeReturnHome)
            {
                int dayIndex = Now.getDayIndexComputationBound(_orderedByStartThenEndSubEvents[0].Start);
                SubCalendarEvent previousSubCalendarEvent = _orderedByStartThenEndSubEvents[0];
                for(int index=1; index < _orderedByStartThenEndSubEvents.Count; index++)
                {
                    SubCalendarEvent currentSubEvent = _orderedByStartThenEndSubEvents[index];
                    int currentDayIndex = Now.getDayIndexComputationBound(currentSubEvent.Start);
                    if (currentDayIndex != dayIndex)
                    {
                        retValue += Location.calculateDistance(previousSubCalendarEvent.Location, _HomeLocation);
                        retValue += Location.calculateDistance(_HomeLocation, currentSubEvent.Location);
                        dayIndex = currentDayIndex;// currentSubEvent.UniversalDayIndex;
                    }
                    else
                    {
                        retValue += Location.calculateDistance(previousSubCalendarEvent.Location, currentSubEvent.Location);
                    }
                    previousSubCalendarEvent = currentSubEvent;
                }
            }
            else
            {
                retValue = Location.calculateDistance(_orderedByStartThenEndSubEvents.Select(SubEvent => SubEvent.Location).ToList());
            }
            
            return retValue;
        }

        public double evaluatePositioning()
        {
            double retValue = 0;
            if(_orderedByStartThenEndSubEvents.Count > 0)
            {
                EventID lastId = _orderedByStartThenEndSubEvents[0].SubEvent_ID;
                int indexCounter = 0;
                for (int i = 1; i < _orderedByStartThenEndSubEvents.Count; i++)
                {
                    SubCalendarEvent SubEvent = _orderedByStartThenEndSubEvents[i];
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

        public List<BlobSubCalendarEvent> evaluateConflicts()
        {
            List<BlobSubCalendarEvent> conflictingEvents = Utility.getConflictingEvents(_orderedByStartThenEndSubEvents);
            return conflictingEvents;
        }

        Dictionary<TimeLine, TimeLine> evaluateSleepSchedule(IEnumerable<DayTimeLine> dayTimeLines)
        {
            List<SubCalendarEvent> OrderexListOfTilerEvents = this._orderedByStartThenEndSubEvents.ToList();
            List<TimeLine> daysSortedBystart = dayTimeLines.OrderBy(obj => obj.Start).Select(daytimeLine => daytimeLine.getJustTimeLine()).ToList();
            Dictionary<TimeLine, TimeLine> retValue = new Dictionary<TimeLine, TimeLine>();
            for (int i = 0; i < daysSortedBystart.Count; i++)
            {
                TimeLine timeLine = daysSortedBystart[i];
                List<SubCalendarEvent> interferringEvents = OrderexListOfTilerEvents.Where(tilerEvent => tilerEvent.RangeTimeLine.doesTimeLineInterfere(timeLine)).ToList();
                List<BusyTimeLine> allBusySlots = interferringEvents.Select(tilerEvent => new BusyTimeLine(tilerEvent.getId, tilerEvent.Start, tilerEvent.End)).ToList();
                timeLine.AddBusySlots(allBusySlots);
                foreach (SubCalendarEvent tilerEvent in interferringEvents.Where(tilerEvent => tilerEvent.End < timeLine.End))
                {
                    OrderexListOfTilerEvents.Remove(tilerEvent);
                }
            }
            retValue = daysSortedBystart.ToDictionary(timeLine => timeLine, timeLine => timeLine.getAllFreeSlots().OrderByDescending(obj => obj.TimelineSpan.Ticks).First());
            return retValue;
        }

        HealthEvaluation getEvaluation(bool forceReevaluation = false)
        {
            if(forceReevaluation || !alreadyEvaluated)
            {
                evaluation = new HealthEvaluation(this);
                evaluation.evaluate(CalculationTimeline).Wait();
                evaluation.TravelTimeAnalysis.evaluate(forceReevaluation).Wait();
            }
            return evaluation;
        }

        public int CompareTo(Health other)
        {
            HealthComparison comparison = new HealthComparison(this, other);
            int retValue = comparison.Compare;
            return retValue;
        }

#region properties
        public Double TotalDistance
        {
            get
            {
                return evaluateTotalDistance();
            }
        }

        public TimeSpan SleepPerDay
        {
            get
            {
                IEnumerable<DayTimeLine> dayTimeLines = Now.getAllDaysCount((uint)(this.CalculationTimeline.TimelineSpan.TotalDays));
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

        public List<TimeLine>  SleepTimeLines
        {
            get
            {
                IEnumerable<DayTimeLine> dayTimeLines = Now.getAllDaysCount((uint)(this.CalculationTimeline.TimelineSpan.TotalDays));
                Dictionary<TimeLine, TimeLine> dayPerTimeLine = evaluateSleepSchedule(dayTimeLines);
                List<TimeLine> validTimeLines = dayPerTimeLine.Select(keyValuePair => keyValuePair.Value).ToList();
                return validTimeLines;
            }
        }

        public List<SubCalendarEvent> orderedByStartThenEndSubEvents
        {
            get
            {
                return _orderedByStartThenEndSubEvents;
            }
        }

        public TravelMode TravelMode
        {
            get
            {
                return this._TravelMode;
            }
        }
#endregion


        public class HealthComparison
        {
            static TimeSpan timeDiff = new TimeSpan();
            TimeSpan _DistanceTravelTimeSpanDifference = new TimeSpan();
            double _DistanceTravelDifference;
            TimeSpan _SleepDifference;
            Health FirstHealth;
            Health SecondHealth;
            HealthEvaluation FirsstEvaluation;
            HealthEvaluation SecondEvaluation;

            public HealthComparison(Health firstHealth, Health secondHealth)
            {
                FirstHealth = firstHealth;
                SecondHealth = secondHealth;
                FirsstEvaluation = FirstHealth.getEvaluation();
                FirsstEvaluation.TravelTimeAnalysis.evaluate().Wait();
                SecondEvaluation = SecondHealth.getEvaluation();
                SecondEvaluation.TravelTimeAnalysis.evaluate().Wait();
                
            }
            

            /// <summary>
            /// This function is to be used with a comparator function
            /// </summary>
            public int Compare
            {
                get
                {
                    double retValue = 0;
                    double DistanceTravelTimeSpanDifferenceCriteria = 0;
                    double DistanceTravelDifferenceCriteria = 0;
                    double SleepDifferenceCriteria = 0;

                    TravelTime firstTravelTime = FirsstEvaluation.TravelTimeAnalysis;
                    firstTravelTime.evaluate().Wait();
                    TimeSpan firstTravelTImeSpanDiff = TimeSpan.FromTicks(firstTravelTime.result().Sum(kvp => kvp.Value.Ticks));

                    TravelTime secondTravelTime = SecondEvaluation.TravelTimeAnalysis;
                    secondTravelTime.evaluate().Wait();
                    TimeSpan secondTravelTimeSpanDiff = TimeSpan.FromTicks(secondTravelTime.result().Sum(kvp => kvp.Value.Ticks));

                    if (firstTravelTImeSpanDiff > secondTravelTimeSpanDiff)
                    {
                        DistanceTravelTimeSpanDifferenceCriteria = -1;
                    } else if (firstTravelTImeSpanDiff < secondTravelTimeSpanDiff)
                    {
                        DistanceTravelTimeSpanDifferenceCriteria = 1;
                    }

                    if (FirsstEvaluation.TotalDistance > SecondEvaluation.TotalDistance)
                    {
                        DistanceTravelDifferenceCriteria = 1;
                    }
                    else if (FirsstEvaluation.TotalDistance > SecondEvaluation.TotalDistance)
                    {
                        DistanceTravelDifferenceCriteria = -1;
                    }


                    TimeSpan firstSleep = TimeSpan.FromTicks(FirsstEvaluation.SleepSchedule.Sum(sleepSpans => sleepSpans.Ticks));
                    TimeSpan secondSleep = TimeSpan.FromTicks(SecondEvaluation.SleepSchedule.Sum(sleepSpans => sleepSpans.Ticks));

                    if (firstSleep > secondSleep)
                    {
                        SleepDifferenceCriteria = -1;
                    }
                    else if (firstSleep < secondSleep)
                    {
                        SleepDifferenceCriteria = 1;
                    }




                    retValue = DistanceTravelTimeSpanDifferenceCriteria + DistanceTravelDifferenceCriteria + SleepDifferenceCriteria;

                    if (retValue > 0)
                    {
                        retValue = 1;
                    }
                    else if (retValue < 0)
                    {
                        retValue = -1;
                    }

                    return (int)retValue;
                }
            }
        }


        
    }
}
