using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleMapsApi.Entities.Directions.Request;
using Newtonsoft.Json.Linq;
using TilerElements;

namespace ScheduleAnalysis
{
    /// <summary>
    /// Class analyses the health of a schedule
    /// </summary>
    public class Health : IComparable<Health>, IJson
    {
        public Guid id = Guid.NewGuid();
        public TimeSpan EvaluationSpan = new TimeSpan(7, 0, 0, 0, 0);
        public TimeLine CalculationTimeline;
        ReferenceNow Now;
        List<SubCalendarEvent> _orderedByStartThenEndSubEvents = new List<SubCalendarEvent>();
        List<BlobSubCalendarEvent> _conflictingEvents;
        GoogleMapsApi.Entities.Directions.Request.TravelMode _TravelMode;
        bool alreadyEvaluated = false;
        HealthEvaluation evaluation;
        Location _HomeLocation;
        SleepEvaluation _sleepEvaluation;
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
            _orderedByStartThenEndSubEvents = SubEvents.Where(SubEvent => SubEvent.StartToEnd.InterferringTimeLine(CalculationTimeline) != null).OrderBy(obj => obj.Start).ThenByDescending(tilerEvent => tilerEvent.End).ToList();
            _conflictingEvents = Utility.getConflictingEvents(_orderedByStartThenEndSubEvents);
            Now = now;
            this._TravelMode = travelmode;
        }


        public Health(IEnumerable<CalendarEvent> AllEvents, DateTimeOffset startTime, TimeSpan evaluationSpan, ReferenceNow refNow, Location homeLocation, TravelMode travelMode = TravelMode.Driving) : this(AllEvents.SelectMany(CalEvent => CalEvent.ActiveSubEvents), startTime, evaluationSpan, refNow, homeLocation, travelMode)
        {
        }

        public double getScore()
        {
            Tuple<double, Dictionary<ulong, List<double>>> distanceEvaluation = evaluateTotalDistance();
            double positioningScore = evaluatePositioning();
            double conflictScore = evaluateConflicts().Sum(blob => blob.getSubCalendarEventsInBlob().Count());
            double eventPerDayScore = eventsPerDay();
            double retValue = Utility.CalcuateResultant(distanceEvaluation.Item1, positioningScore, conflictScore, SleepEvaluation.ScoreTimeLine(), eventPerDayScore);
            return retValue;
        }

        /// <summary>
        /// Function evaluates the distance travelled by user at crow flies.  The result returns a tuple with two Items. Item 1 is total distance travelled. Item 2 is a dictionary of day index to distance travelled subevent
        /// </summary>
        /// <param name="includeReturnHome"></param>
        /// <returns></returns>
        public Tuple<double, Dictionary<ulong, List<double>>> evaluateTotalDistance(bool includeReturnHome = true)
        {
            Dictionary<ulong, List<double>> combinedResult = new Dictionary<ulong, List<double>>();
            double retValue = 0;
            
            List<SubCalendarEvent> relevantSubCalendarEventList = _orderedByStartThenEndSubEvents.Where(obj => !obj.getIsProcrastinateCalendarEvent).ToList();
            if(relevantSubCalendarEventList.Count > 0)
            {
                relevantSubCalendarEventList.AsParallel().ForAll((Action<SubCalendarEvent>)(subEvent =>
                    {
                        if (subEvent.Location.isNull)
                        {
                            subEvent.Location.verify();
                        }
                    })
                );
                SubCalendarEvent firstSUbEvent = relevantSubCalendarEventList[0];
                DateTimeOffset refTIme = CalculationTimeline.IsDateTimeWithin(firstSUbEvent.Start) ? firstSUbEvent.Start : firstSUbEvent.End;
                ulong dayIndex = Now.getDayIndexFromStartOfTime(refTIme);
                SubCalendarEvent previousSubCalendarEvent = relevantSubCalendarEventList[0];
                List<double> distances = new List<double>();
                ulong currentDayIndex = dayIndex;
                combinedResult.Add(currentDayIndex, distances);
                for (int index = 1; index < relevantSubCalendarEventList.Count; index++)
                {

                    SubCalendarEvent currentSubEvent = relevantSubCalendarEventList[index];
                    refTIme = CalculationTimeline.IsDateTimeWithin(currentSubEvent.Start) ? currentSubEvent.Start : currentSubEvent.End;
                    ulong subEventDayIndex = Now.getDayIndexFromStartOfTime(refTIme);
                    if (subEventDayIndex != dayIndex)
                    {
                        if(includeReturnHome)
                        {
                            double distance = Location.calculateDistance(previousSubCalendarEvent.Location, _HomeLocation);
                            distances.Add(distance);
                            distances = new List<double>();
                            combinedResult.Add(subEventDayIndex, distances);
                            retValue += distance;
                            if (index != 1)
                            {
                                distance = Location.calculateDistance(_HomeLocation, currentSubEvent.Location);
                                distances.Add(distance);
                                retValue += distance;
                            }
                            dayIndex = subEventDayIndex;// currentSubEvent.UniversalDayIndex;
                        }
                        else
                        {
                            double distance = Location.calculateDistance(previousSubCalendarEvent.Location, currentSubEvent.Location);
                            distances.Add(distance);
                            distances = new List<double>();
                            combinedResult.Add(subEventDayIndex, distances);
                        }
                    }
                    else
                    {
                        double distance = Location.calculateDistance(previousSubCalendarEvent.Location, currentSubEvent.Location);
                        retValue += distance;
                        distances.Add(distance);
                    }
                    previousSubCalendarEvent = currentSubEvent;
                }
            }


            Tuple<double, Dictionary<ulong, List<double>>> retValueTuple = new Tuple<double, Dictionary<ulong, List<double>>>(retValue, combinedResult);
            return retValueTuple;
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
                    if(TravelTimeAfter.Ticks == 1)
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

        internal HealthEvaluation getEvaluation(bool forceReevaluation = false)
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
            var distanceEvaluation = evaluateTotalDistance();
            double positioningScore = evaluatePositioning();
            double conflictScore = evaluateConflicts().Sum(blob => blob.getSubCalendarEventsInBlob().Count());
            Tuple<double, List<Tuple<TimeLine, ulong>>> sleepEvaluation = SleepEvaluation.scoreAndTimeLine();

            double score = Utility.CalcuateResultant(distanceEvaluation.Item1, positioningScore, conflictScore, sleepEvaluation.Item1);
            double eventPerDayScore = eventsPerDay();
            JObject retValue = new JObject();
            JObject sleep = SleepEvaluation.ToJson();
            JObject distance = new JObject();
            distance.Add("score", distanceEvaluation.Item1);
            distance.Add("evaluation", new JArray(distanceEvaluation.Item2.Select(eval =>
            {
                JObject dayResult = new JObject();
                JArray distances = new JArray(eval.Value);
                TimeLine timeLine = Now.getDayTimeLineByDayIndex((ulong)eval.Key);
                
                dayResult.Add("startOfDay", timeLine.Start.toJSMilliseconds());
                dayResult.Add("distances", distances);
                return dayResult;
            })));
            retValue.Add("distance", distance);
            retValue.Add("position", positioningScore);
            retValue.Add("conflict", conflictScore);
            retValue.Add("sleep", sleep);
            retValue.Add("eventPerDayScore", eventPerDayScore);
            retValue.Add("scheduleScore", score);
            

            return retValue;
        }

        #region properties
        public Double TotalDistance
        {
            get
            {
                return evaluateTotalDistance().Item1;
            }
        }

        public TimeSpan SleepPerDay
        {
            get
            {
                List<TimeLine> sleepTimeLine = new List<TimeLine>();
                TimeSpan totalSleepSpan = new TimeSpan();
                TimeSpan totalDayspans = new TimeSpan();
                Tuple<ulong, ulong> dayIndexBoundaries = Now.indexRange(CalculationTimeline);
                ulong iniUniverslaIndex = Now.firstDay.UniversalIndex;
                for (ulong i = dayIndexBoundaries.Item1; i <= dayIndexBoundaries.Item2; i++)
                {
                    ulong universalIndex = iniUniverslaIndex + (ulong)i;
                    DayTimeLine dayTimeLine = Now.getDayTimeLineByDayIndex(universalIndex);

                    if (dayTimeLine.TimelineSpan.TotalHours > 20 && universalIndex > iniUniverslaIndex)
                    {
                        DayTimeLine previousDayTimeLine = Now.getDayTimeLineByDayIndex(universalIndex - 1);
                        totalDayspans = totalDayspans.Add(dayTimeLine.TimelineSpan);
                        DateTimeOffset sleepStart = previousDayTimeLine.SleepSubEvent?.End ?? previousDayTimeLine.End;
                        DateTimeOffset sleepEnd = dayTimeLine.WakeSubEvent?.Start ?? sleepStart.Add(Now.SleepSpan);
                        TimeLine sleepTImeLine =new TimeLine(sleepStart, sleepEnd);
                        totalSleepSpan = totalSleepSpan.Add(sleepTImeLine.TimelineSpan);
                    }
                    
                }
                double averageSleepSpan = (double)totalSleepSpan.Ticks / totalDayspans.Ticks;
                TimeSpan retValue = TimeSpan.FromHours(averageSleepSpan * 24);
                return retValue;
            }
        }

        public double eventsPerDay()
        {
            double retValue = 0;
            double sum = 0;
            List<SubCalendarEvent> allSubEvents = new List<SubCalendarEvent>();
            Tuple<ulong, ulong> dayIndexBoundaries = Now.indexRange(CalculationTimeline);
            ulong universlaIndex = Now.firstDay.UniversalIndex;
            ulong i = dayIndexBoundaries.Item1;
            for (; i <= dayIndexBoundaries.Item2; i++)
            {
                ulong universalIndex = universlaIndex + (ulong)i;
                DayTimeLine dayTimeLine = Now.getDayTimeLineByDayIndex(universalIndex);
                allSubEvents.AddRange(dayTimeLine.getSubEventsInTimeLine());
                sum += dayTimeLine.getSubEventsInTimeLine().Count;
            }
            if (i > 0)
            {
                retValue = (double)sum / i;
            }
            
            return retValue;
        }

        public SleepEvaluation SleepEvaluation
        {
            get
            {
                return _sleepEvaluation ?? (_sleepEvaluation = new SleepEvaluation(Now, CalculationTimeline));
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
    }
}
