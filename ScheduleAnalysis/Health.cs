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
            _conflictingEvents = Utility.getConflictingEvents(_orderedByStartThenEndSubEvents).Item1;
            Now = now;
            this._TravelMode = travelmode;
        }


        public Health(IEnumerable<CalendarEvent> AllEvents, DateTimeOffset startTime, TimeSpan evaluationSpan, ReferenceNow refNow, Location homeLocation, TravelMode travelMode = TravelMode.Driving) : this(AllEvents.SelectMany(CalEvent => CalEvent.ActiveSubEvents), startTime, evaluationSpan, refNow, homeLocation, travelMode)
        {
        }


        /// <summary>
        /// This calculates a score for the current schedule quality. Thats based on the positionscore, conflictscore, and congestion of subevents. Generally the lower the score, the better the schedule quality.
        /// This simply gets you the resultant of the positionscore, distance and conflict score. So be careful the distance can dominate the values
        /// </summary>
        /// <returns></returns>
        public double getScore()
        {
            Tuple<double, Dictionary<long, List<double>>, double> distanceEvaluation = evaluateTotalDistance();
            double positioningScore = evaluatePositioning();
            double conflictScore = evaluateConflicts().Item1;
            double eventPerDayScore = eventsPerDay();
            double retValue = Utility.CalcuateResultant(distanceEvaluation.Item3, positioningScore, conflictScore, SleepEvaluation.ScoreTimeLine(), eventPerDayScore);
            return retValue;
        }

        public List<SubCalendarEvent> TardyEvaluation()
        {
            IEnumerable<SubCalendarEvent> tardySubEVents = orderedByStartThenEndSubEvents.Where(subEvent => subEvent.isTardy);
            List<SubCalendarEvent> retValue = tardySubEVents.ToList();
            return retValue;
        }

        public JObject TardyJson ()
        {
            List<SubCalendarEvent> tardyResult = TardyEvaluation();
            JObject retValue = new JObject();
            JArray subEvents = new JArray(tardyResult.Select(o => o.Json));
            retValue.Add("count", tardyResult.Count);
            retValue.Add("subevents", subEvents);
            ILookup<long, SubCalendarEvent> dayToSubEvents = tardyResult.ToLookup(obj => Now.getDayIndexFromStartOfTime(obj.Start), obj => obj);
            JObject dayDistribution = new JObject();

            foreach(var dayToSubevent in dayToSubEvents)
            {
                JArray subEvents_Jobj = new JArray(dayToSubevent.Select(obj => obj.ToJson()));
                long beginningOfDay = Now.getClientBeginningOfDay(dayToSubevent.Key).ToUnixTimeMilliseconds();
                dayDistribution.Add(beginningOfDay.ToString(), subEvents_Jobj);
            }

            retValue.Add("days", dayDistribution);

            return retValue;

        }

        /// <summary>
        /// Function evaluates the distance travelled by user at crow flies.  The result returns a tuple with three Items. 
        /// Item 1 is total distance travelled. 
        /// Item 2 is a dictionary of day index to distance travelled for the specific day index
        /// Item 3 is the score of the distance travelled. 
        /// </summary>
        /// <param name="includeReturnHome"></param>
        /// <returns></returns>
        public Tuple<double, Dictionary<long, List<double>>, double> evaluateTotalDistance(bool includeReturnHome = true)
        {
            Dictionary<long, List<double>> combinedResult = new Dictionary<long, List<double>>();
            double totalDistanceTravelled = 0;
            
            List<SubCalendarEvent> relevantSubCalendarEventList = _orderedByStartThenEndSubEvents.Where(obj => !obj.getIsProcrastinateCalendarEvent && obj.getActiveDuration < Utility.LeastAllDaySubeventDuration).ToList();
            if(relevantSubCalendarEventList.Count > 0)
            {
                foreach (SubCalendarEvent subEvent in relevantSubCalendarEventList)
                {
                    if (subEvent.Location.isNull)
                    {
                        subEvent.Location.verify();
                    }
                }
                SubCalendarEvent firstSUbEvent = relevantSubCalendarEventList[0];
                DateTimeOffset refTIme = CalculationTimeline.IsDateTimeWithin(firstSUbEvent.Start) ? firstSUbEvent.Start : firstSUbEvent.End;
                long dayIndex = Now.getDayIndexFromStartOfTime(refTIme);
                SubCalendarEvent previousSubCalendarEvent = relevantSubCalendarEventList[0];
                List<double> distances = new List<double>();
                long currentDayIndex = dayIndex;
                combinedResult.Add(currentDayIndex, distances);
                for (int index = 1; index < relevantSubCalendarEventList.Count; index++)
                {

                    SubCalendarEvent currentSubEvent = relevantSubCalendarEventList[index];
                    refTIme = CalculationTimeline.IsDateTimeWithin(currentSubEvent.Start) ? currentSubEvent.Start : currentSubEvent.End;
                    long subEventDayIndex = Now.getDayIndexFromStartOfTime(refTIme);
                    if (subEventDayIndex != dayIndex)
                    {
                        if(includeReturnHome)
                        {
                            double distance = Location.calculateDistance(previousSubCalendarEvent.Location, _HomeLocation);
                            distances.Add(distance);
                            distances = new List<double>();
                            combinedResult.Add(subEventDayIndex, distances);
                            totalDistanceTravelled += distance;
                            if (index != 1)
                            {
                                distance = Location.calculateDistance(_HomeLocation, currentSubEvent.Location);
                                distances.Add(distance);
                                totalDistanceTravelled += distance;
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
                        totalDistanceTravelled += distance;
                        distances.Add(distance);
                    }
                    previousSubCalendarEvent = currentSubEvent;
                }
            }

            double score = 0;
            if(relevantSubCalendarEventList.Count > 1)
            {
                double totalAverageDistanceTravelled = ((double)totalDistanceTravelled / (double)(relevantSubCalendarEventList.Count - 1));// relevantSubCalendarEventList.Count - 1 because the travel count is always subevent count - 1
                IList<IList<double>> distancesPerDay = combinedResult.Select(o => (IList<double>)o.Value).ToList();
                if (distancesPerDay.Count > 1)
                {
                    double averageOfAverage = distancesPerDay.Select(obj => obj.Average()).Average();// average travel of each ay  and average of each days average gives of a statistical representation of te travel of the whole schedule

                    score = averageOfAverage / totalAverageDistanceTravelled;
                }
                else
                {
                    Location averageLocation = Location.AverageGPSLocation(relevantSubCalendarEventList.Select(o => o.Location));
                    double averageOfaverageByHomeAverage =  Location.calculateDistance(averageLocation, _HomeLocation);
                    score = totalAverageDistanceTravelled / averageOfaverageByHomeAverage;
                }
            }
            

            Tuple<double, Dictionary<long, List<double>>, double> retValueTuple = new Tuple<double, Dictionary<long, List<double>>, double>(totalDistanceTravelled, combinedResult, score);
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

        public Tuple<double, ILookup<long, BlobSubCalendarEvent>> evaluateConflicts()
        {
            List<BlobSubCalendarEvent> conflictingEvents = Utility.getConflictingEvents(_orderedByStartThenEndSubEvents.Where(o => o.getActiveDuration < Utility.LeastAllDaySubeventDuration)).Item1;
            double conflictTotal = conflictingEvents.Sum(blob => blob.getSubCalendarEventsInBlob().Count());
            ILookup<long, BlobSubCalendarEvent> subEventLookup = conflictingEvents.ToLookup(obj => Now.getClientBeginningOfDay( Now.getDayIndexFromStartOfTime(obj.Start)).ToUnixTimeMilliseconds(), obj => obj);
            Tuple<double, ILookup<long, BlobSubCalendarEvent>> retValue = new Tuple<double, ILookup<long, BlobSubCalendarEvent>>(conflictTotal, subEventLookup);
            return retValue;
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
        

        public JObject convertConflictEvalToJson(Tuple<double, ILookup<long, BlobSubCalendarEvent>> conflictEval)
        {
            JObject retValue = new JObject();
            retValue.Add("score", conflictEval.Item1);
            JObject days = new JObject();
            foreach(var lookup in conflictEval.Item2)
            {
                JArray subEvents = new JArray(lookup.Select(o=>o.Json));
                days.Add(lookup.Key.ToString(), subEvents);
            }
            retValue.Add("days", days);

            return retValue;
        }

    public JObject ToJson()
        {
            var distanceEvaluation = evaluateTotalDistance();
            double positioningScore = evaluatePositioning();
            var conflictEval = evaluateConflicts();
            double conflictScore = conflictEval.Item1;
            Tuple<double, List<Tuple<TimeLine, TimeLine, long>>> sleepEvaluation = SleepEvaluation.scoreAndTimeLine();

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
                TimeLine timeLine = Now.getDayTimeLineByDayIndex((long)eval.Key);
                
                dayResult.Add("startOfDay", timeLine.Start.toJSMilliseconds());
                dayResult.Add("distances", distances);
                return dayResult;
            })));
            var tardyJson = TardyJson();
            var conflictJson = convertConflictEvalToJson(conflictEval);

            retValue.Add("distance", distance);
            retValue.Add("position", positioningScore);
            retValue.Add("conflict", conflictJson);
            retValue.Add("sleep", sleep);
            retValue.Add("tardy", tardyJson);
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
                Tuple<long, long> dayIndexBoundaries = Now.indexRange(CalculationTimeline);
                long iniUniverslaIndex = Now.firstDay.UniversalIndex;
                for (long i = dayIndexBoundaries.Item1; i <= dayIndexBoundaries.Item2; i++)
                {
                    long universalIndex = iniUniverslaIndex + i;
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
            Tuple<long, long> dayIndexBoundaries = Now.indexRange(CalculationTimeline);
            long universlaIndex = Now.firstDay.UniversalIndex;
            long i = dayIndexBoundaries.Item1;
            for (; i <= dayIndexBoundaries.Item2; i++)
            {
                long universalIndex = universlaIndex + i;
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
