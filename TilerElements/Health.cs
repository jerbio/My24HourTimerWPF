using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleMapsApi.Entities.Directions.Request;
using Newtonsoft.Json.Linq;

namespace TilerElements
{
    /// <summary>
    /// Class analyses the health of a schedule
    /// </summary>
    public class Health : IComparable<Health>, IJson
    {
        public TimeSpan EvaluationSpan = new TimeSpan(7, 0, 0, 0, 0);
        public TimeSpan SleepSpan = new TimeSpan(0, 7, 0, 0, 0);
        public TimeLine CalculationTimeline;
        ReferenceNow Now;
        List<SubCalendarEvent> _orderedByStartThenEndSubEvents = new List<SubCalendarEvent>();
        List<BlobSubCalendarEvent> _conflictingEvents;
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
            EvaluationSpan = evaluationSpan;
            CalculationTimeline = new TimeLine(startTime, startTime.Add(EvaluationSpan));
            _orderedByStartThenEndSubEvents = SubEvents.Where(SubEvent => SubEvent.RangeTimeLine.InterferringTimeLine(CalculationTimeline) != null).OrderBy(obj => obj.Start).ThenByDescending(tilerEvent => tilerEvent.End).ToList();
            _conflictingEvents = Utility.getConflictingEvents(_orderedByStartThenEndSubEvents);
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
            double sleepScore = evaluateSleepTimeFrameScore();
            double retValue = Utility.CalcuateResultant(totalDistance, positioningScore, conflictScore, sleepScore);
            return retValue;
        }

        public double evaluateTotalDistance(bool includeReturnHome = true)
        {
            double retValue = 0;
            if (includeReturnHome)
            {
                List<SubCalendarEvent> relevantSubCalendarEventList = _orderedByStartThenEndSubEvents.Where(obj => !obj.getIsProcrastinateCalendarEvent).ToList();
                if(relevantSubCalendarEventList.Count > 0)
                {
                    int dayIndex = Now.getDayIndexComputationBound(relevantSubCalendarEventList[0].Start);
                    SubCalendarEvent previousSubCalendarEvent = relevantSubCalendarEventList[0];
                    for (int index = 1; index < relevantSubCalendarEventList.Count; index++)
                    {
                        SubCalendarEvent currentSubEvent = relevantSubCalendarEventList[index];
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
                
            }
            else
            {
                retValue = Location.calculateDistance(_orderedByStartThenEndSubEvents.Select(SubEvent => SubEvent.Location).ToList());
            }
            
            return retValue;
        }

        public double evaluateSleepTimeFrameScore()
        {
            double retValue = (double)TimeSpan.FromHours(24).Ticks / SleepPerDay.Ticks;
            return retValue;
        }

        /// <summary>
        /// This assess if the position of the subevents is suitable for its completion.
        /// It considers if the spacing is wide enough to allow the travel time.
        /// It will also consider the time it is scheduled, does it have a higher percentage of success.
        /// </summary>
        /// <returns></returns>
        public double evaluatePositioning()
        {
            double retValue = 0;
            List<SubCalendarEvent> relevantSubCalendarEventList = _orderedByStartThenEndSubEvents.Where(obj => !_conflictingEvents.Any(conflicting => conflicting.getSubCalendarEventsInBlob().Contains( obj))).ToList();
            relevantSubCalendarEventList.AddRange(_conflictingEvents);
            relevantSubCalendarEventList = relevantSubCalendarEventList.OrderBy(subEvent => subEvent.Start).ToList();
            if (relevantSubCalendarEventList.Count > 0)
            {
                EventID lastId = relevantSubCalendarEventList[0].SubEvent_ID;
                TimeSpan totalTravelSpan = new TimeSpan();
                TimeSpan idealTotalSpan = new TimeSpan();
                TimeSpan conflictingSpan = new TimeSpan();
                for (int i = 0, j = 1; (j < relevantSubCalendarEventList.Count && i < relevantSubCalendarEventList.Count); i++, j++)
                {
                    SubCalendarEvent before = relevantSubCalendarEventList[i];
                    SubCalendarEvent after = relevantSubCalendarEventList[j];
                    TimeSpan timeSpan = after.Start - before.End;
                   
                    totalTravelSpan = totalTravelSpan.Add(timeSpan);
                    TimeSpan TravelTimeAfter = before.TravelTimeAfter;
                    if(TravelTimeAfter.Ticks == -1)
                    {
                        TravelTimeAfter = TimeSpan.FromMinutes( Location.calculateDistance(before.Location, after.Location, 30) *12);
                    }
                    idealTotalSpan = idealTotalSpan.Add(TravelTimeAfter);
                }
                // need to strip milliseconds and seconds
                idealTotalSpan = idealTotalSpan.Add(-TimeSpan.FromSeconds(idealTotalSpan.Seconds));
                idealTotalSpan = idealTotalSpan.Add(-TimeSpan.FromMilliseconds(idealTotalSpan.Milliseconds));
                if (totalTravelSpan.Ticks < 0)
                {
                    totalTravelSpan = -conflictingSpan;
                }


                if (totalTravelSpan.TotalMilliseconds != 0)
                {
                    double ratioSpan = (double)idealTotalSpan.TotalMilliseconds / totalTravelSpan.TotalMilliseconds;
                    retValue = ratioSpan;
                } else
                {
                    if(idealTotalSpan.TotalMilliseconds!=0 )
                    {
                        retValue = 100;
                    }
                }
            }
            return retValue;
        }

        public List<BlobSubCalendarEvent> evaluateConflicts()
        {
            List<BlobSubCalendarEvent> conflictingEvents = Utility.getConflictingEvents(_orderedByStartThenEndSubEvents);
            return conflictingEvents;
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

        public JObject ToJson()
        {
            double totalDistance = evaluateTotalDistance();
            double positioningScore = evaluatePositioning();
            double conflictScore = evaluateConflicts().Sum(blob => blob.getSubCalendarEventsInBlob().Count());
            double sleepScore = evaluateSleepTimeFrameScore();
            double score = Utility.CalcuateResultant(totalDistance, positioningScore, conflictScore, sleepScore);
            JObject retValue = new JObject();
            retValue.Add("Distance", totalDistance);
            retValue.Add("Position", positioningScore);
            retValue.Add("Conflict", conflictScore);
            retValue.Add("Sleep", sleepScore);
            retValue.Add("scheduleScore", score);
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
                List<TimeLine> sleepTimeLine = new List<TimeLine>();
                TimeSpan totalSleepSpan = new TimeSpan();
                TimeSpan totalDayspans = new TimeSpan();
                Tuple<int, int> dayIndexBoundaries = Now.indexRange(CalculationTimeline);
                ulong universlaIndex = Now.firstDay.UniversalIndex;
                for (int i = dayIndexBoundaries.Item1; i <= dayIndexBoundaries.Item2; i++)
                {
                    ulong universalIndex = universlaIndex + (ulong)i;
                    DayTimeLine dayTimeLine = Now.getDayTimeLineByDayIndex(universalIndex);
                    if(dayTimeLine.TimelineSpan.TotalHours > 20)
                    {
                        totalDayspans= totalDayspans.Add(dayTimeLine.TimelineSpan);
                        totalSleepSpan = totalSleepSpan.Add(dayTimeLine.SleepTimeLine.TimelineSpan);
                    }
                    
                }
                double averageSleepSpan = (double)totalSleepSpan.Ticks / totalDayspans.Ticks;
                TimeSpan retValue = TimeSpan.FromHours(averageSleepSpan * 24);
                return retValue;
            }
        }

        public List<TimeLine>  SleepTimeLines
        {
            get
            {
                List<TimeLine> sleepTimeLine = new List<TimeLine>();
                Tuple<int, int> dayIndexBoundaries = Now.indexRange(CalculationTimeline);
                ulong universlaIndex = Now.firstDay.UniversalIndex;
                for (int i = dayIndexBoundaries.Item1; i <= dayIndexBoundaries.Item2; i++)
                {
                    ulong universalIndex = universlaIndex + (ulong)i;
                    DayTimeLine dayTimeLine = Now.getDayTimeLineByDayIndex(universalIndex);
                    sleepTimeLine.Add(dayTimeLine.SleepTimeLine);
                }
                return sleepTimeLine;
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
