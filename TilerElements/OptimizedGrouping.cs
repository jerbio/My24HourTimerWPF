using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;
using static TilerElements.TimeOfDayPreferrence;

namespace TilerElements
{
    public class OptimizedGrouping
    {
        TimeOfDayPreferrence.SingleTimeOfDayPreference _Section;
        /// <summary>
        /// these events that have being verified to be able to fit within the daytimeline, after evaluating their path optimized. THis have been fully stiuck to the end.Note this is not an ordered  set;
        /// </summary>
        HashSet<SubCalendarEvent> AcknowlegdedEvents;
        /// <summary>
        /// This onliy exist because of PathStitchedSubEventsList. It is to ensure there are no duplicates in PathStitchedSubEventsList
        /// </summary>
        HashSet<SubCalendarEvent> PathStitchedSubEvents;
        /// <summary>
        /// This is a list thaat just contains subevents that have only being optimized for the path. This does not take into consideration their ability to fit within a specific day
        /// </summary>
        List<SubCalendarEvent> PathStitchedSubEventsList;
        List<SubCalendarEvent> OrderedAcknowledgedEvents;
        Location LeftStitch = new Location();
        Location RightStitch = new Location();
        Location DefaultLocation;
        Location HomeLocation;
        TimeSpan TotalDuration;
        OptimizedAverage AverageOfStitched;

        public OptimizedGrouping(TimeOfDayPreferrence.SingleTimeOfDayPreference SectionData, TimeSpan SubeventDurationSum, Location DefaultLocation, Location HomeLocation)
        {
            _Section = SectionData;
            AcknowlegdedEvents = new HashSet<SubCalendarEvent>();
            PathStitchedSubEvents = new HashSet<SubCalendarEvent>();
            PathStitchedSubEventsList = new List<SubCalendarEvent>();
            OrderedAcknowledgedEvents = new List<SubCalendarEvent>();
            TotalDuration = SubeventDurationSum;
            this.HomeLocation = HomeLocation;
            this.DefaultLocation = DefaultLocation;
            AverageOfStitched = new OptimizedAverage(new HashSet<SubCalendarEvent>(), SectionData.Timeline);
        }



        void setAcknowledgedEvents(IEnumerable<SubCalendarEvent> SubEvents)
        {
            AcknowlegdedEvents = new HashSet<SubCalendarEvent>((SubEvents));
            foreach(SubCalendarEvent SubCalendarEvent in AcknowlegdedEvents)
            {
                SubCalendarEvent.setAsOptimized();
            }

            AverageOfStitched = new OptimizedAverage(AcknowlegdedEvents, _Section.Timeline);
            orderAndStorePinnedEventsByStartTime();
        }

        public void movePathStitchedToAcknowledged()
        {
            setAcknowledgedEvents(PathStitchedSubEvents);
            clearPathStitchedEvents();
        }

        public static Dictionary<OptimizedGrouping, Location> getAverageLocation(IEnumerable<OptimizedGrouping> OrderedGroupings)
        {
            List<OptimizedGrouping> OrderedGrouping = OrderedGroupings.OrderBy(obj => (int)obj._Section.DaySection).ToList();
            Dictionary<OptimizedGrouping, Location> RetValue = OrderedGrouping.ToDictionary(obj => obj, obj => Location.AverageGPSLocation(obj.PathStitchedSubEvents.Select(obj1 => obj1.Location)));
            return RetValue;
        }
        public static Dictionary<OptimizedGrouping, Location> buildStitchers(Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping> groupingsDictionary)
        {
            IEnumerable<OptimizedGrouping> Groupings = groupingsDictionary.Values;
            Dictionary<OptimizedGrouping, Location> AverageLocation = getAverageLocation(Groupings);
            foreach (KeyValuePair<OptimizedGrouping, Location> kvp in AverageLocation)
            {
                kvp.Key.DefaultLocation = kvp.Value;
            }
            List<OptimizedGrouping> groupReordered = TimeOfDayPreferrence.ActiveDaySections
                .Where(daySection => groupingsDictionary.ContainsKey(daySection))
                .Select(daySection => groupingsDictionary[daySection]).ToList();
            OptimizedGrouping noneGrouping;
            if (groupingsDictionary.ContainsKey(DaySection.None))
            {
                noneGrouping = groupingsDictionary[DaySection.None];
            } else
            {
                noneGrouping = new OptimizedGrouping(new SingleTimeOfDayPreference(DaySection.None, new TimelineWithSubcalendarEvents()), new TimeSpan(), new Location(), new Location());
            }
            groupReordered.Insert(0, noneGrouping);


            List<KeyValuePair<OptimizedGrouping, Location>> OrderedAvergeLocation = groupReordered.Select(obj => new KeyValuePair<OptimizedGrouping, Location>(obj, AverageLocation[obj])).ToList();
            for (int i = 0; i < OrderedAvergeLocation.Count; i++)
            {
                if ((i != 0) && (i != AverageLocation.Count - 1))
                {
                    if (i == 1)/// go with previous eleement that isn't none. None in this case is currently 0
                    {
                        KeyValuePair<OptimizedGrouping, Location> Current = OrderedAvergeLocation[i];
                        KeyValuePair<OptimizedGrouping, Location> Previous = Current;
                        KeyValuePair<OptimizedGrouping, Location> Next = OrderedAvergeLocation[i + 1];

                        Location leftLocation = !OrderedAvergeLocation[i].Key.LeftBorder.isDefault && !OrderedAvergeLocation[i].Key.LeftBorder.isNull ? OrderedAvergeLocation[i].Key.LeftBorder : Current.Value;
                        Current.Key.RightStitch = Location.getClosestLocation(Next.Key.PathStitchedSubEvents.Select(obj => obj.Location), Current.Value);
                        //Not updating the Left stitch because it is the beginning of the actual path
                        if (Current.Key.RightStitch == null)
                        {
                            Current.Key.RightStitch = Next.Key.DefaultLocation;
                        }
                    }
                    else
                    {
                        KeyValuePair<OptimizedGrouping, Location> Previous = OrderedAvergeLocation[i - 1];
                        KeyValuePair<OptimizedGrouping, Location> Current = OrderedAvergeLocation[i];
                        KeyValuePair<OptimizedGrouping, Location> Next = OrderedAvergeLocation[i + 1];
                        Current.Key.LeftStitch = Location.getClosestLocation(Previous.Key.PathStitchedSubEvents.Select(obj => obj.Location), Current.Value);
                        Current.Key.RightStitch = Location.getClosestLocation(Next.Key.PathStitchedSubEvents.Select(obj => obj.Location), Current.Value);

                        if (Current.Key.LeftStitch == null)
                        {
                            Current.Key.LeftStitch = Previous.Key.DefaultLocation;
                        }
                        if (Current.Key.RightStitch == null)
                        {
                            Current.Key.RightStitch = Next.Key.DefaultLocation;
                        }
                    }
                }
                else
                {
                    if (i == 0)//none
                    {
                        KeyValuePair<OptimizedGrouping, Location> Current = OrderedAvergeLocation[i];
                        KeyValuePair<OptimizedGrouping, Location> Next = Current;// OrderedAvergeLocation[i + 1];
                        List<SubCalendarEvent> NextPhaseSubevent = Next.Key.PathStitchedSubEvents.ToList();
                        Current.Key.LeftStitch = Current.Value;
                        Current.Key.RightStitch = NextPhaseSubevent.Count > 0 ? Location.getClosestLocation(NextPhaseSubevent.Select(obj => obj.Location), Current.Value) : Current.Value;

                        if (Current.Key.LeftStitch == null)
                        {
                            Current.Key.LeftStitch = Current.Key.DefaultLocation;
                        }

                        if (Current.Key.RightStitch == null)
                        {
                            Current.Key.RightStitch = Next.Key.DefaultLocation;
                        }
                    }
                    else
                    {
                        if (i == AverageLocation.Count - 1)//Evening
                        {
                            KeyValuePair<OptimizedGrouping, Location> Previous = OrderedAvergeLocation[i - 1];
                            KeyValuePair<OptimizedGrouping, Location> Current = OrderedAvergeLocation[i];
                            List<SubCalendarEvent> PrevPhaseSubevent = Previous.Key.PathStitchedSubEvents.ToList();
                            Current.Key.LeftStitch = PrevPhaseSubevent.Count > 0 ? Location.getClosestLocation(PrevPhaseSubevent.Select(obj => obj.Location), Current.Value) : Current.Value;
                            if (Current.Key.LeftStitch == null)
                            {
                                Current.Key.LeftStitch = Previous.Key.DefaultLocation;
                            }
                        }
                    }
                }
            }

            return AverageLocation;
        }
        public void ClearPinnedSubEvents()
        {
            AcknowlegdedEvents.Clear();
            OrderedAcknowledgedEvents.Clear();
            AverageOfStitched = null;
        }


        public List<SubCalendarEvent> getPathStitchedSubevents()
        {
            //return PathStitchedSubEventsList.OrderBy(obj => obj.Start).ToList();
            return PathStitchedSubEventsList;
        }

        /// <summary>
        /// this function is to be used when you need all subevents viable for stitching with other grouping object. It'll check if there are subevents in the the PathStitchedSubEventsList. 
        /// If there are none then it goes on to select the acknowledged list.
        /// </summary>
        /// <returns></returns>
        public List<SubCalendarEvent> getEventsForStitichingWithOtherOptimizedGroupings()
        {
            List<SubCalendarEvent> retValue;
            if (PathStitchedSubEventsList.Count < 1)
            {
                retValue = AcknowlegdedEvents.OrderBy(subEvent => subEvent.Start).ToList();
                
            }
            else
            {
                retValue = getPathStitchedSubevents();
            }
            return retValue;
        }


        /// <summary>
        /// This gets you all the subevents that have been acknwledged to work within setion. Note the are not necessarily ordered.
        /// </summary>
        /// <returns></returns>
        public List<SubCalendarEvent> getPinnedEvents()
        {
            return OrderedAcknowledgedEvents.ToList();
        }

        /// <summary>
        /// This gets you all the subevents that have been acknwledged to work within setion. These are ordered whenever the collection of Pinned sub events gets modified. 
        /// </summary>
        /// <returns></returns>
        public List<SubCalendarEvent> getOrderedPinnedEvents()
        {
            return OrderedAcknowledgedEvents.ToList();
        }

        /// <summary>
        /// Function orders the pinned events by their start time and then locks them in order This way you can lock in the order of the pinned events
        /// </summary>
        public List<SubCalendarEvent> orderAndStorePinnedEventsByStartTime ()
        {
            OrderedAcknowledgedEvents = AcknowlegdedEvents.OrderBy(o => o.Start).ToList();
            return OrderedAcknowledgedEvents;
        }

        public void AddToStitchedEvents(SubCalendarEvent SubEvent)
        {
            //if (PathStitchedSubEvents.Contains(SubEvent))
            {
                PathStitchedSubEventsList.Remove(SubEvent);
            }
            PathStitchedSubEvents.Add(SubEvent);
            PathStitchedSubEventsList.Add(SubEvent);
        }


        public void removeFromStitched(SubCalendarEvent SubEvent)
        {
            PathStitchedSubEvents.Remove(SubEvent);
            PathStitchedSubEventsList.Remove(SubEvent);
        }

        public void removeFromAcknowledged(SubCalendarEvent SubEvent)
        {
            AcknowlegdedEvents.Remove(SubEvent);
            AverageOfStitched = new OptimizedAverage(AcknowlegdedEvents, _Section.Timeline);
            PathStitchedSubEventsList.Remove(SubEvent);
            orderAndStorePinnedEventsByStartTime();
        }

        public void setPathStitchedEvents(IEnumerable<SubCalendarEvent> SubEvents)
        {
            PathStitchedSubEvents = new HashSet<SubCalendarEvent>(SubEvents);//.ToList();
            PathStitchedSubEventsList = SubEvents.ToList();
        }


        public void clearPathStitchedEvents()
        {
            PathStitchedSubEvents.Clear();
            PathStitchedSubEventsList.Clear();
        }


        public void setRightStitch(Location location)
        {
            RightStitch = location;
        }

        public void setLeftStitch(Location location)
        {
            LeftStitch = location;
        }

        public Location LeftBorder
        {
            get
            {
                if(LeftStitch.isNull)
                {
                    if(HomeLocation == null || HomeLocation.isNull)
                    {
                        return DefaultLocation;
                    }else
                    {
                        return HomeLocation;
                    }
                }

                return LeftStitch;
                
            }
        }

        public Location RightBorder
        {
            get
            {
                if (RightStitch.isNull)
                {
                    if (HomeLocation == null || HomeLocation.isNull)
                    {
                        return DefaultLocation;
                    }
                    else
                    {
                        return HomeLocation;
                    }
                }

                return RightStitch;
            }
        }

        public TimeOfDayPreferrence.DaySection DaySector
        {
            get
            {
                return _Section.DaySection;
            }
        }

        public OptimizedAverage GroupAverage
        {
            get
            {
                return AverageOfStitched;
            }
        }

        public TimeLine TimeLine
        {
            get
            {
                return _Section.Timeline;
            }
        }

        public override string ToString()
        {
            return this.DaySector.ToString() + "||" + this._Section.Timeline.ToString();
        }

        public class OptimizedAverage
        {
            List<SubCalendarEvent> _SubEvents;
            TimelineWithSubcalendarEvents timeLine;
            TimeLine Range;// This is the beginning of the earliest subevent and subevent with the latest end time
            public OptimizedAverage(HashSet<SubCalendarEvent> subEvents, TimeLine estimatedRange)
            {
                if (subEvents != null)
                {
                    if (subEvents.Count > 0)
                    {
                        _SubEvents = (subEvents).OrderBy(obj => obj.Start).ThenBy(obj => obj.End).ToList();
                        DateTimeOffset latestEnd = _SubEvents.Max(obj => obj.End);
                        DateTimeOffset earliestEnd = _SubEvents.Min(obj => obj.Start);
                        Range = new TimeLine(earliestEnd, latestEnd);
                    }
                    else
                    {
                        nullOrEmptyListIniialization();
                    }
                }
                else
                {
                    nullOrEmptyListIniialization();
                }

                if (estimatedRange != null)
                {
                    timeLine = new TimelineWithSubcalendarEvents(estimatedRange.Start, estimatedRange.End, subEvents);
                }
            }

            void nullOrEmptyListIniialization()
            {
                _SubEvents = new List<SubCalendarEvent>();
                timeLine = new TimelineWithSubcalendarEvents();
                Range = new TimeLine();
            }

            public IEnumerable<SubCalendarEvent> SubEvents
            {
                get
                {
                    return _SubEvents;
                }
            }

            public TimeSpan AverageDuration 
            {
                get
                {
                    return timeLine.TotalActiveSpan;
                }
            }

            public TimelineWithSubcalendarEvents TimeLine
            {
                get
                {
                    return (TimelineWithSubcalendarEvents)timeLine;
                }
            }

            public TimeLine RangeTimeLine
            {
                get
                {
                    return Range.CreateCopy();
                }
            }

        }
    }

}
