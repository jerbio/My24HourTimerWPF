using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;

namespace TilerElements
{
    public class OptimizedGrouping
    {
        TimeOfDayPreferrence.SingleTimeOfDayPreference Section;
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
        Location LeftStitch = new Location();
        Location RightStitch = new Location();
        Location DefaultLocation;
        TimeSpan TotalDuration;
        OptimizedAverage AverageOfStitched;

        public OptimizedGrouping(TimeOfDayPreferrence.SingleTimeOfDayPreference SectionData, TimeSpan SubeventDurationSum, Location DefaultLocation)
        {
            Section = SectionData;
            AcknowlegdedEvents = new HashSet<SubCalendarEvent>();
            PathStitchedSubEvents = new HashSet<SubCalendarEvent>();
            PathStitchedSubEventsList = new List<SubCalendarEvent>();
            TotalDuration = SubeventDurationSum;
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

            AverageOfStitched = new OptimizedAverage(AcknowlegdedEvents, Section.Timeline);
        }

        public void movePathStitchedToAcknowledged()
        {
            setAcknowledgedEvents(PathStitchedSubEvents);
            clearPathStitchedEvents();
        }

        public static Dictionary<OptimizedGrouping, Location> getAverageLocation(IEnumerable<OptimizedGrouping> OrderedGroupings)
        {
            List<OptimizedGrouping> OrderedGrouping = OrderedGroupings.OrderBy(obj => (int)obj.Section.DaySection).ToList();
            Dictionary<OptimizedGrouping, Location> RetValue = OrderedGrouping.ToDictionary(obj => obj, obj => Location.AverageGPSLocation(obj.PathStitchedSubEvents.Select(obj1 => obj1.Location)));
            return RetValue;
        }
        public static Dictionary<OptimizedGrouping, Location> buildStitchers(IEnumerable<OptimizedGrouping> Groupings)
        {
            Dictionary<OptimizedGrouping, Location> AverageLocation = getAverageLocation(Groupings);
            foreach (KeyValuePair<OptimizedGrouping, Location> kvp in AverageLocation)
            {
                kvp.Key.DefaultLocation = kvp.Value;
            }
            List<KeyValuePair<OptimizedGrouping, Location>> OrderedAvergeLocation = Groupings.Select(obj => new KeyValuePair<OptimizedGrouping, Location>(obj, AverageLocation[obj])).ToList();
            for (int i = 0; i < OrderedAvergeLocation.Count; i++)
            {
                if ((i != 0) && (i != AverageLocation.Count - 1))
                {
                    if (i == 1)/// go with previous eleement that isn't none. None in this case is currently 0
                    {
                        KeyValuePair<OptimizedGrouping, Location> Current = OrderedAvergeLocation[i];
                        KeyValuePair<OptimizedGrouping, Location> Previous = Current;
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
                        if (i == AverageLocation.Count - 1)//sleep
                        {
                            KeyValuePair<OptimizedGrouping, Location> Previous = OrderedAvergeLocation[i - 1];
                            KeyValuePair<OptimizedGrouping, Location> Current = OrderedAvergeLocation[i];
                            List<SubCalendarEvent> PrevPhaseSubevent = Previous.Key.PathStitchedSubEvents.ToList();
                            Current.Key.LeftStitch = PrevPhaseSubevent.Count > 0 ? Location.getClosestLocation(PrevPhaseSubevent.Select(obj => obj.Location), Current.Value) : Current.Value;
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
            AverageOfStitched = new OptimizedAverage(AcknowlegdedEvents, Section.Timeline);
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
                return LeftStitch;
            }
        }

        public Location RightBorder
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
                return Section.DaySection;
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
