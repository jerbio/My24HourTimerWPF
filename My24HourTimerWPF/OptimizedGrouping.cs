using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;

namespace My24HourTimerWPF
{
    public class OptimizedGrouping
    {
        TimeOfDayPreferrence.DaySection Section;
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
        Location_Elements LeftStitch = new Location_Elements();
        Location_Elements RightStitch = new Location_Elements();
        Location_Elements DefaultLocation;
        TimeSpan TotalDuration;
        OptimizedAverage AverageOfStitched;

        public OptimizedGrouping(TimeOfDayPreferrence.DaySection SectionData, TimeSpan SubeventDurationSum, Location_Elements DefaultLocation)
        {
            Section = SectionData;
            AcknowlegdedEvents = new HashSet<SubCalendarEvent>();
            PathStitchedSubEvents = new HashSet<SubCalendarEvent>();
            PathStitchedSubEventsList = new List<SubCalendarEvent>();
            TotalDuration = SubeventDurationSum;
            this.DefaultLocation = DefaultLocation;
        }



        void setAcknowledgedEvents(IEnumerable<SubCalendarEvent> SubEvents)
        {
            AcknowlegdedEvents = new HashSet<SubCalendarEvent>((SubEvents));
            foreach(SubCalendarEvent SubCalendarEvent in AcknowlegdedEvents)
            {
                SubCalendarEvent.setAsOptimized();
            }

            AverageOfStitched = new OptimizedAverage(AcknowlegdedEvents);
        }

        public void movePathStitchedToAcknowledged()
        {
            setAcknowledgedEvents(PathStitchedSubEvents);
            clearPathStitchedEvents();
        }

        public static Dictionary<OptimizedGrouping, Location_Elements> getAverageLocation(IEnumerable<OptimizedGrouping> Groupings)
        {
            List<OptimizedGrouping> OrderedGrouping = Groupings.OrderBy(obj => (int)obj.Section).ToList();
            Dictionary<OptimizedGrouping, Location_Elements> RetValue = OrderedGrouping.ToDictionary(obj => obj, obj => Location_Elements.AverageGPSLocation(obj.PathStitchedSubEvents.Select(obj1 => obj1.myLocation)));
            return RetValue;
        }
        public static Dictionary<OptimizedGrouping, Location_Elements> buildStitchers(IEnumerable<OptimizedGrouping> Groupings)
        {
            Dictionary<OptimizedGrouping, Location_Elements> AverageLocation = getAverageLocation(Groupings);
            List<KeyValuePair<OptimizedGrouping, Location_Elements>> OrderedAvergeLocation = Groupings.Select(obj => new KeyValuePair<OptimizedGrouping, Location_Elements>(obj, AverageLocation[obj])).ToList();
            for (int i = 0; i < OrderedAvergeLocation.Count; i++)
            {
                if ((i != 0) && (i != AverageLocation.Count - 1))
                {
                    if (i == 1)/// go with previous eleement that isn't none. None in this case is currently 0
                    {
                        KeyValuePair<OptimizedGrouping, Location_Elements> Current = OrderedAvergeLocation[i];
                        KeyValuePair<OptimizedGrouping, Location_Elements> Previous = Current;
                        KeyValuePair<OptimizedGrouping, Location_Elements> Next = OrderedAvergeLocation[i + 1];
                        Current.Key.LeftStitch = Location_Elements.getClosestLocation(Previous.Key.PathStitchedSubEvents.Select(obj => obj.myLocation), Current.Value);
                        Current.Key.RightStitch = Location_Elements.getClosestLocation(Next.Key.PathStitchedSubEvents.Select(obj => obj.myLocation), Current.Value);

                        if (Current.Key.LeftStitch == null)
                        {
                            Current.Key.LeftStitch = Previous.Key.DefaultLocation;
                        }
                        if (Current.Key.RightStitch == null)
                        {
                            Current.Key.RightStitch = Next.Key.DefaultLocation;
                        }
                    }
                    else
                    {


                        KeyValuePair<OptimizedGrouping, Location_Elements> Previous = OrderedAvergeLocation[i - 1];
                        KeyValuePair<OptimizedGrouping, Location_Elements> Current = OrderedAvergeLocation[i];
                        KeyValuePair<OptimizedGrouping, Location_Elements> Next = OrderedAvergeLocation[i + 1];
                        Current.Key.LeftStitch = Location_Elements.getClosestLocation(Previous.Key.PathStitchedSubEvents.Select(obj => obj.myLocation), Current.Value);
                        Current.Key.RightStitch = Location_Elements.getClosestLocation(Next.Key.PathStitchedSubEvents.Select(obj => obj.myLocation), Current.Value);

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

                        KeyValuePair<OptimizedGrouping, Location_Elements> Current = OrderedAvergeLocation[i];
                        KeyValuePair<OptimizedGrouping, Location_Elements> Next = Current;// OrderedAvergeLocation[i + 1];
                        List<SubCalendarEvent> NextPhaseSubevent = Next.Key.PathStitchedSubEvents.ToList();
                        Current.Key.LeftStitch = Current.Value;
                        Current.Key.RightStitch = NextPhaseSubevent.Count > 0 ? Location_Elements.getClosestLocation(NextPhaseSubevent.Select(obj => obj.myLocation), Current.Value) : Current.Value;

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
                        if (i == AverageLocation.Count - 1)//sleep
                        {
                            KeyValuePair<OptimizedGrouping, Location_Elements> Previous = OrderedAvergeLocation[i - 1];
                            KeyValuePair<OptimizedGrouping, Location_Elements> Current = OrderedAvergeLocation[i];
                            List<SubCalendarEvent> PrevPhaseSubevent = Previous.Key.PathStitchedSubEvents.ToList();
                            Current.Key.LeftStitch = PrevPhaseSubevent.Count > 0 ? Location_Elements.getClosestLocation(PrevPhaseSubevent.Select(obj => obj.myLocation), Current.Value) : Current.Value;
                            Current.Key.RightStitch = Current.Value;

                            if (Current.Key.LeftStitch == null)
                            {
                                Current.Key.LeftStitch = Previous.Key.DefaultLocation;
                            }

                            if (Current.Key.RightStitch == null)
                            {
                                Current.Key.RightStitch = Current.Key.DefaultLocation;
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
            return AcknowlegdedEvents.ToList();
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

        public void removeFromAcknwledged(SubCalendarEvent SubEvent)
        {
            AcknowlegdedEvents.Remove(SubEvent);
            AverageOfStitched = new OptimizedAverage(AcknowlegdedEvents);
            PathStitchedSubEventsList.Remove(SubEvent);
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

        public Location_Elements LeftBorder
        {
            get
            {
                return LeftStitch;
            }
        }

        public Location_Elements RightBorder
        {
            get
            {
                return RightStitch;
            }
        }

        public TimeOfDayPreferrence.DaySection DaySector
        {
            get
            {
                return Section;
            }
        }

        public OptimizedAverage GroupAverage
        {
            get
            {
                return AverageOfStitched;
            }
        }

        public class OptimizedAverage
        {
            List<SubCalendarEvent> _SubEvents;
            Location_Elements Location;
            TimeSpan Duration;
            TimeLine Range;
            public OptimizedAverage(HashSet<SubCalendarEvent> subEvents)
            {
                if (subEvents != null)
                {
                    if (subEvents.Count > 0)
                    {
                        _SubEvents = (subEvents).OrderBy(obj => obj.Start).ThenBy(obj => obj.End).ToList(); ;
                        Location = Location_Elements.AverageGPSLocation(_SubEvents.Select(obj => obj.myLocation));
                        Duration = TimeSpan.FromTicks((long)(_SubEvents.Average(obj => (obj.RangeSpan.Ticks))));
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
                
            }

            void nullOrEmptyListIniialization()
            {
                _SubEvents = new List<SubCalendarEvent>();
                Location = Location_Elements.AverageGPSLocation(_SubEvents.Select(obj => obj.myLocation));
                Duration = new TimeSpan();
                Range = new TimeLine();
            }

            public IEnumerable<SubCalendarEvent> SubEvents
            {
                get
                {
                    return _SubEvents;
                }
            }

            public Location_Elements AverageLocation
            {
                get
                {
                    return Location.CreateCopy();
                }
            }

            public TimeSpan AverageDuration 
            {
                get
                {
                    return Duration;
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
