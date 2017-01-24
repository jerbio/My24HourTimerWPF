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
        List<SubCalendarEvent> orderedByStartThenEndSubEvents = new List<SubCalendarEvent>();
        GoogleMapsApi.Entities.Directions.Request.TravelMode TravelMode;
        bool alreadyEvaluated = false;
        HealthEvaluation evaluation;

        public Health(IEnumerable<SubCalendarEvent> AllEvents, DateTimeOffset startTime, TimeSpan evaluationSpan, ReferenceNow now, TravelMode travelmode = TravelMode.Driving)
        {
            IEnumerable<SubCalendarEvent> SubEvents = AllEvents;
            CalculationTimeline = new TimeLine(startTime, startTime.Add(EvaluationSpan));
            orderedByStartThenEndSubEvents = SubEvents.Where(SubEvent => SubEvent.RangeTimeLine.InterferringTimeLine(CalculationTimeline) != null).OrderBy(obj => obj.Start).ThenByDescending(tilerEvent => tilerEvent.End).ToList();
            EvaluationSpan = evaluationSpan;
            Now = now;
            this.TravelMode = travelmode;
        }


        public Health(IEnumerable<CalendarEvent> AllEvents, DateTimeOffset startTime, TimeSpan evaluationSpan, ReferenceNow refNow, TravelMode travelMode = TravelMode.Driving) : this(AllEvents.SelectMany(CalEvent => CalEvent.ActiveSubEvents), startTime, evaluationSpan, refNow, travelMode)
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

        public double evaluateTotalDistance()
        {
            double retValue = Location.calculateDistance(orderedByStartThenEndSubEvents.Select(SubEvent => SubEvent.Location).ToList());
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

        public List<BlobSubCalendarEvent> evaluateConflicts()
        {
            List<BlobSubCalendarEvent> conflictingEvents = Utility.getConflictingEvents(orderedByStartThenEndSubEvents);
            return conflictingEvents;
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

        class TravelTime
        {
            List<SubCalendarEvent> OrderedSubEvents;
            Dictionary<EventID, SubCalendarEvent> EventIdToSubEvent;
            TimeLine EventSequence;
            TravelMode TravelMode;
            bool alreadyEvaluated = false;

            /// <summary>
            /// Dictionary holds subevent Ids to free timeline. If the Id doesnt exist then it should then a freespot greater than zero ticks was not found
            /// </summary>
            /// <param name="transitingIdsToFreespot"></param>
            Dictionary<string, TimeLineWithEdgeElements> TransitingIdsToFreespot;
            ConcurrentDictionary<string, TimeSpan> TransitingIdsToWebTravelSpan;
            ConcurrentDictionary<string, TimeSpan> TransitingIdsToSuccess;
            public TravelTime(IEnumerable<SubCalendarEvent> subEvents, TravelMode travelMode = TravelMode.Driving)
            {
                TravelMode = travelMode;
                if (subEvents.Count()>0)
                {
                    OrderedSubEvents = subEvents.OrderBy(subEvent => subEvent.Start).ThenBy(subEvent => subEvent.End).ToList();
                    EventSequence = new TimeLine(OrderedSubEvents.First().Start, OrderedSubEvents.Last().End);
                    EventSequence.AddBusySlots(OrderedSubEvents.Select(subEvent => subEvent.ActiveSlot));
                    EventIdToSubEvent = OrderedSubEvents.ToDictionary(subEvent => subEvent.SubEvent_ID, subEvent => subEvent);
                    TransitingIdsToWebTravelSpan = new ConcurrentDictionary<string, TimeSpan>();
                    TransitingIdsToSuccess = new ConcurrentDictionary<string, TimeSpan>();
                    if (EventSequence.OccupiedSlots.Length> 1)
                    {
                        TransitingIdsToFreespot = new Dictionary<string, TimeLineWithEdgeElements>();
                        TimeLineWithEdgeElements[] timeLineWithEdge = EventSequence.getAllFreeSlotsWithEdges();
                        for(int i=0; i< timeLineWithEdge.Length; i++)
                        {
                            TimeLineWithEdgeElements freeSpotWithEdge = timeLineWithEdge[i];
                            string[] ids = { freeSpotWithEdge.BeginningEventId, freeSpotWithEdge.EndingEventId };
                            TransitingIdsToFreespot.Add(string.Join(",", ids), freeSpotWithEdge);
                        }
                    }
                    
                }
                else
                {
                    OrderedSubEvents = new List<SubCalendarEvent>();
                    EventSequence = new TimeLine();
                    EventIdToSubEvent = new Dictionary<EventID, SubCalendarEvent>();
                    TransitingIdsToFreespot = new Dictionary<string, TimeLineWithEdgeElements>();
                    TransitingIdsToWebTravelSpan = new ConcurrentDictionary<string, TimeSpan>();
                    TransitingIdsToSuccess = new ConcurrentDictionary<string, TimeSpan>();
                } 
            }

            async public Task evaluate(bool forceReEvaluation = false)
            {
                if (forceReEvaluation || !alreadyEvaluated)
                {
                    if (EventSequence.OccupiedSlots.Length > 1)
                    {
                        Parallel.For(0, EventSequence.OccupiedSlots.Length - 1, (i) =>
                        //for (int i = 0; i< EventSequence.OccupiedSlots.Length-1; i++)
                        {
                            int j = i + 1;
                            SubCalendarEvent firstSubEvent = OrderedSubEvents[i];
                            SubCalendarEvent secondSubEvent = OrderedSubEvents[j];
                            TimeSpan travelSpan = Location.getDrivingTimeFromWeb(firstSubEvent.Location, secondSubEvent.Location, this.TravelMode);
                            string[] ids = { firstSubEvent.getId, secondSubEvent.getId };
                            string concatId = string.Join(",", ids);
                            TransitingIdsToWebTravelSpan.AddOrUpdate(concatId, travelSpan, ((key, oldValue) => { return travelSpan; }));
                            TimeSpan freeSpotSpan = new TimeSpan();
                            if (TransitingIdsToFreespot.ContainsKey(concatId))
                            {
                                freeSpotSpan = TransitingIdsToFreespot[concatId].TimelineSpan;
                            }
                            TimeSpan differenceSpan = (freeSpotSpan - travelSpan);
                            TransitingIdsToSuccess.AddOrUpdate(concatId, differenceSpan, ((key, oldValue) => { return differenceSpan; }));
                        }
                        );
                    }
                    alreadyEvaluated = true;
                }
                
            }

            public Dictionary<string, TimeSpan> result()
            {
                Dictionary<string, TimeSpan> retValue = TransitingIdsToSuccess.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                return retValue;
            }
        }

        /// <summary>
        /// Class is simply to provide a way to hold the result of an evaluated schedule. It is not meant to be used outside this Class. 
        /// </summary>
        class HealthEvaluation
        {
            Health ScheduleHealth;
            List<BlobSubCalendarEvent> _ConflictingEvents { get; set; }
            double _TotalDistance { get; set; }
            double _PositioningScore { get; set; }
            List<TimeSpan> _SleepSchedule { get; set; }
            TravelTime _TravelTimeAnalysis { get; set; }
            public HealthEvaluation (Health health)
            {
                this.ScheduleHealth = health;
            }
            
            public async Task evaluate(TimeLine timeLine)
            {
                this._ConflictingEvents = this.ScheduleHealth.evaluateConflicts();
                this._TotalDistance = this.ScheduleHealth.TotalDistance;
                this._PositioningScore = this.ScheduleHealth.evaluatePositioning();
                this._SleepSchedule = this.ScheduleHealth.SleepTimeLines.Select(sleepTimeLine => sleepTimeLine.TimelineSpan).ToList();
                this._TravelTimeAnalysis = new TravelTime(this.ScheduleHealth.orderedByStartThenEndSubEvents, this.ScheduleHealth.TravelMode);
            }

            
            public List<BlobSubCalendarEvent> ConflictingEvents {
                get {
                    return _ConflictingEvents;
                }
            }
            public double TotalDistance {
                get {
                    return _TotalDistance;
                }
            }
            public double PositioningScore {
                get {
                    return _PositioningScore;
                }
            }
            public List<TimeSpan> SleepSchedule {
                get {
                    return _SleepSchedule;
                }
            }
            public TravelTime TravelTimeAnalysis {
                get
                {
                    return _TravelTimeAnalysis;
                }
            }
        }

    }
}
