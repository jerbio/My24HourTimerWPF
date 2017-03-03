using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleMapsApi.Entities.Directions.Request;

namespace TilerElements
{
    public class TravelTime
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
            if (subEvents.Count() > 0)
            {
                OrderedSubEvents = subEvents.OrderBy(subEvent => subEvent.Start).ThenBy(subEvent => subEvent.End).ToList();
                EventSequence = new TimeLine(OrderedSubEvents.First().Start, OrderedSubEvents.Last().End);
                EventSequence.AddBusySlots(OrderedSubEvents.Select(subEvent => subEvent.ActiveSlot));
                EventIdToSubEvent = OrderedSubEvents.ToDictionary(subEvent => subEvent.SubEvent_ID, subEvent => subEvent);
                TransitingIdsToWebTravelSpan = new ConcurrentDictionary<string, TimeSpan>();
                TransitingIdsToSuccess = new ConcurrentDictionary<string, TimeSpan>();
                if (EventSequence.OccupiedSlots.Length > 1)
                {
                    TransitingIdsToFreespot = new Dictionary<string, TimeLineWithEdgeElements>();
                    TimeLineWithEdgeElements[] timeLineWithEdge = EventSequence.getAllFreeSlotsWithEdges();
                    for (int i = 0; i < timeLineWithEdge.Length; i++)
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

}
