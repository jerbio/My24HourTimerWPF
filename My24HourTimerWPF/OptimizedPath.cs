using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;

namespace My24HourTimerWPF
{
    class OptimizedPath
    {
        DayTimeLine DayInfo;
        Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping> AllGroupings = new Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping>();
        public OptimizedPath(DayTimeLine DayData)
        {
            DayInfo = DayData;
            TimeSpan TotalDuration = SubCalendarEvent.TotalActiveDuration(DayData.getSubEventsInDayTimeLine());
            AllGroupings.Add(TimeOfDayPreferrence.DaySection.Sleep, new OptimizedGrouping(TimeOfDayPreferrence.DaySection.Sleep, TotalDuration));
            AllGroupings.Add(TimeOfDayPreferrence.DaySection.Morning, new OptimizedGrouping(TimeOfDayPreferrence.DaySection.Morning, TotalDuration));
            AllGroupings.Add(TimeOfDayPreferrence.DaySection.Afternoon, new OptimizedGrouping(TimeOfDayPreferrence.DaySection.Afternoon, TotalDuration));
            AllGroupings.Add(TimeOfDayPreferrence.DaySection.Evening, new OptimizedGrouping(TimeOfDayPreferrence.DaySection.Evening, TotalDuration));
            AllGroupings.Add(TimeOfDayPreferrence.DaySection.None, new OptimizedGrouping(TimeOfDayPreferrence.DaySection.None, TotalDuration));
        }

        public void OptimizePath()
        {
            while (true)
            {
                List<SubCalendarEvent> AllSubCalendarEvents = DayInfo.getSubEventsInDayTimeLine();
                List<SubCalendarEvent> CurrentlyValid = AllSubCalendarEvents.Where(obj => (!obj.Rigid)).Where(obj => (!obj.isOptimized)).Where(obj => 
                {
                    var TimeOfDay = obj.getDaySection().getCurrentDayPreference(); 
                    return ((TimeOfDay != TimeOfDayPreferrence.DaySection.Disabled)&&(TimeOfDay!=TimeOfDayPreferrence.DaySection.None));
                }).ToList();
                if (CurrentlyValid.Count > 0)
                {
                    ILookup<TimeOfDayPreferrence.DaySection, SubCalendarEvent> SubEventRegrouping = groupEvents(AllSubCalendarEvents);
                    List<OptimizedGrouping> DaySectorGrouping = new List<OptimizedGrouping>();
                    foreach (IGrouping<TimeOfDayPreferrence.DaySection, SubCalendarEvent> eachIGrouping in SubEventRegrouping)
                    {
                        
                        if (eachIGrouping.Key != TimeOfDayPreferrence.DaySection.Disabled)
                        {
                            AllGroupings[eachIGrouping.Key].ClearAllSubEvents();
                            AllGroupings[eachIGrouping.Key].updateSubEvents(SubEventRegrouping[eachIGrouping.Key]);
                            OptimizeGrouping(AllGroupings[eachIGrouping.Key], SubEventRegrouping[eachIGrouping.Key]);
                        }
                    }
                    OptimizedGrouping.buildStitchers(AllGroupings.Select(obj => obj.Value));
                    
                }
                else
                {
                    break;
                }
            }
        }

        void OptimizeGrouping(OptimizedGrouping Grouping, IEnumerable<SubCalendarEvent>AllEvents )
        {
            Grouping.clearUnStitchedEvents();
            List<SubCalendarEvent> Subevents = new List<SubCalendarEvent>();
            List<SubCalendarEvent> Acknowledged_Revised = Grouping.getAcknowledgedSubevents();
            int i = 0;
            foreach(SubCalendarEvent eachSubCalendarEvent in AllEvents.OrderBy(obj=>obj.ActiveDuration))
            {
                if(i<5)
                {
                    Subevents.Add(eachSubCalendarEvent);
                }
                else
                {
                    break;
                }
                i++;
            }
            Subevents= Utility.getBestPermutation(Subevents, double.MaxValue, new Tuple<Location_Elements, Location_Elements>(Grouping.LeftBorder, Grouping.RightBorder)).ToList();
            for(i=0;i<Subevents.Count;i++)
            {
                SubCalendarEvent mySubEvent = Subevents[i];
                int BestPostion = getBestPosition(mySubEvent , Acknowledged_Revised);
                Acknowledged_Revised.Insert(BestPostion,mySubEvent);
            }

            Grouping.setUnStitchedEvents(Subevents);
            //Grouping.updateSubEvents(Acknowledged_Revised);
        }

        int getBestPosition(SubCalendarEvent SubEvent, IEnumerable<SubCalendarEvent> CurrentList)
        { 
            int i=0;
            int currentCount= CurrentList.Count();
            List<SubCalendarEvent> FullList = CurrentList.ToList();
            double[] TotalDistances = new double[currentCount];
            for (; i <= currentCount; i++)
            {
                List<SubCalendarEvent> FullList_Copy = FullList.ToList();
                FullList_Copy.Insert(i, SubEvent);
                double TotalDistance = SubCalendarEvent.CalculateDistance(FullList_Copy);
                TotalDistances[i] = TotalDistance;
            }
            int RetValue = TotalDistances.MinIndex();
            return RetValue;
        }
        void StitchAllGroupings()
        {
            List<OptimizedGrouping> OrderedOptimizedGroupings = AllGroupings.Where(obj=>obj.Key!=TimeOfDayPreferrence.DaySection.None).OrderBy(obj => obj.Key).Select(obj=>obj.Value).ToList();
            TimeLine myDayTimeLine=  DayInfo.getJustTimeLine();
            List<SubCalendarEvent> SubEvents = OrderedOptimizedGroupings.SelectMany(obj => obj.getUnstitchedSubevents()).ToList();
            List<SubCalendarEvent> SubEventsWithNoLocationPreference = AllGroupings[TimeOfDayPreferrence.DaySection.None].getUnstitchedSubevents();

            List <SubCalendarEvent> PinResults = spliceInNoneTimeOfDayPreferemce(SubEvents, SubEventsWithNoLocationPreference);
            tryPinning(myDayTimeLine, PinResults);
        }

        List<SubCalendarEvent> spliceInNoneTimeOfDayPreferemce(IEnumerable<SubCalendarEvent> ArrangementWithoutNones, IEnumerable<SubCalendarEvent> SubEventsOfNone)
        {
            int IndexCount = ArrangementWithoutNones.Count()+1;
            Queue<mTuple<int, SubCalendarEvent>>[] PreferredLocations = new Queue<mTuple<int, SubCalendarEvent>>[IndexCount];
            for (int i=0 ;i<IndexCount;i++)
            {
                PreferredLocations[i] = new Queue<mTuple<int, SubCalendarEvent>>();
            }

            List<mTuple<int, SubCalendarEvent>> UnoptimizedSubEventsOfNone = SubEventsOfNone.Select((obj, i) => { return new mTuple<int, SubCalendarEvent>(i, obj); }).ToList();

            for(int i=0; i<UnoptimizedSubEventsOfNone.Count;i++)
            {
                SubCalendarEvent RefSubaEvent = UnoptimizedSubEventsOfNone[i].Item2;
                int BestIndex=getBestPosition(RefSubaEvent, ArrangementWithoutNones);

                PreferredLocations[BestIndex].Enqueue(UnoptimizedSubEventsOfNone[i]);
            }
            List<SubCalendarEvent> RetValue = ArrangementWithoutNones.ToList();
            int delta = 0;
            for(int i= 0 ; i<PreferredLocations.Length;i++)
            {
                Queue<mTuple<int, SubCalendarEvent>> Myqueue = PreferredLocations[i];
                int insertionIndex = i+delta;
                RetValue.InsertRange(insertionIndex, Myqueue.OrderBy(obj=>obj.Item1).Select(obj=>obj.Item2));
                delta += Myqueue.Count;
            }

            return RetValue;
        }


        void tryPinning(TimeLine AllTimeLine, List<SubCalendarEvent> SubEvents)
        {
            if(Utility.PinSubEventsToStart(SubEvents,AllTimeLine))
            {
                foreach (OptimizedGrouping eachOptimizedGrouping in AllGroupings.Values)
                {
                    eachOptimizedGrouping.ClearAcknowlwedgedSubEvents();
                }
                foreach (SubCalendarEvent eachSubCalendarEvent in SubEvents)
                {
                    TimeOfDayPreferrence.DaySection DaySection = eachSubCalendarEvent.getDaySection().getCurrentDayPreference();
                    if (DaySection != TimeOfDayPreferrence.DaySection.Disabled)
                    {   
                        AllGroupings[DaySection].AddToAcknowledgedEvents(eachSubCalendarEvent);
                    }
                }
                SubEvents.ForEach(obj => obj.getDaySection().rejectCurrentPreference());
            }
            else
            {
                List<SubCalendarEvent> NonRigidis = SubEvents.Where(obj=>(!obj.Rigid)&&(!obj.isOptimized)).OrderBy(obj=>obj.ActiveDuration).ToList();
                SubCalendarEvent UnwantedEvent = NonRigidis[0];
                UnwantedEvent.getDaySection().rejectCurrentPreference();
                SubEvents.Remove(UnwantedEvent);
                tryPinning(AllTimeLine, SubEvents);
            }
        }
        void ResolveRejects()
        {

        }

        ILookup<TimeOfDayPreferrence.DaySection, SubCalendarEvent> groupEvents(IEnumerable<SubCalendarEvent> SubEvents)
        {
            SubEvents.AsParallel().ForAll(obj => obj.InitializeDayPreference());
            ILookup<TimeOfDayPreferrence.DaySection, SubCalendarEvent> RetValue = SubEvents.ToLookup(obj => obj.getDaySection().getCurrentDayPreference(), obj => obj);
            return RetValue;
        }


        class OptimizedGrouping
        {
            TimeOfDayPreferrence.DaySection Section;
            HashSet<SubCalendarEvent> AllSubEvents;
            Dictionary<string, SubCalendarEvent> DeubggingAcknowledgement = new Dictionary<string, SubCalendarEvent>();
            List<SubCalendarEvent> AcknowlegdedEvents;
            List<SubCalendarEvent> unstitchedEvents;
            Location_Elements LeftStitch = new Location_Elements();
            Location_Elements RightStitch = new Location_Elements();
            TimeSpan TotalDuration;

            public OptimizedGrouping(TimeOfDayPreferrence.DaySection SectionData, TimeSpan SubeventDurationSum)
            {
                Section = SectionData;
                AllSubEvents = new HashSet<SubCalendarEvent>();
                AcknowlegdedEvents = new List<SubCalendarEvent>();
                unstitchedEvents = new List<SubCalendarEvent>();
                TotalDuration = SubeventDurationSum;
            }

            public void updateSubEvents(IEnumerable<SubCalendarEvent>SubEvents)
            {
                AllSubEvents = new HashSet<SubCalendarEvent>(AllSubEvents.Concat(SubEvents));
            }

            public static Dictionary<OptimizedGrouping, Location_Elements>  getAverageLocation(IEnumerable<OptimizedGrouping> Groupings)
            {
                List<OptimizedGrouping> OrderedGrouping = Groupings.OrderBy(obj => (int)obj.Section).ToList();
                Dictionary<OptimizedGrouping, Location_Elements> RetValue =OrderedGrouping.ToDictionary(obj=>obj,obj=>Location_Elements.AverageGPSLocation( obj.AllSubEvents.Select(obj1=>obj1.myLocation)));
                return RetValue;
            }
            public static Dictionary<OptimizedGrouping, Location_Elements>  buildStitchers(IEnumerable<OptimizedGrouping> Groupings)
            {
                Dictionary<OptimizedGrouping, Location_Elements> AverageLocation = getAverageLocation(Groupings);
                List<KeyValuePair<OptimizedGrouping, Location_Elements>> OrderedAvergeLocation = AverageLocation.ToList();
                for(int i=0; i<OrderedAvergeLocation.Count;i++)
                {
                    if((i!=0)&&(i!=AverageLocation.Count-1))
                    {
                        KeyValuePair<OptimizedGrouping, Location_Elements> Previous = OrderedAvergeLocation[i - 1];
                        KeyValuePair<OptimizedGrouping, Location_Elements> Current = OrderedAvergeLocation[i ];
                        KeyValuePair<OptimizedGrouping, Location_Elements> Next = OrderedAvergeLocation[i + 1];
                        Current.Key.LeftStitch = Location_Elements.getClosestLocation(Previous.Key.AllSubEvents.Select(obj => obj.myLocation), Current.Value);
                        Current.Key.RightStitch= Location_Elements.getClosestLocation(Next.Key.AllSubEvents.Select(obj => obj.myLocation), Current.Value);
                    }
                    else
                    {
                        if(i==0)
                        {
                            
                            KeyValuePair<OptimizedGrouping, Location_Elements> Current = OrderedAvergeLocation[i];
                            KeyValuePair<OptimizedGrouping, Location_Elements> Next = OrderedAvergeLocation[i + 1];
                            Current.Key.LeftStitch = Current.Value;
                            Current.Key.RightStitch = Location_Elements.getClosestLocation(Next.Key.AllSubEvents.Select(obj => obj.myLocation), Current.Value);
                        }
                        else
                        {
                            if(i==AverageLocation.Count-1)
                            {
                                KeyValuePair<OptimizedGrouping, Location_Elements> Previous = OrderedAvergeLocation[i - 1];
                                KeyValuePair<OptimizedGrouping, Location_Elements> Current = OrderedAvergeLocation[i];
                                Current.Key.LeftStitch = Location_Elements.getClosestLocation(Previous.Key.AllSubEvents.Select(obj => obj.myLocation), Current.Value);
                                Current.Key.RightStitch = Current.Value;
                            }
                        }
                    }                    
                }

                return AverageLocation;
            }
            public void ClearAllSubEvents()
            {
                AllSubEvents.Clear();
            }

            public List<SubCalendarEvent>getUnstitchedSubevents()
            {
                return unstitchedEvents.ToList();
            }


            public void AddToAcknowledgedEvents(SubCalendarEvent SubEvent)
            {
                AcknowlegdedEvents.Add(SubEvent);
                DeubggingAcknowledgement.Add(SubEvent.ID,SubEvent);
                SubEvent.setAsOptimized();
            }

            public void AddTounStitchedEvents(SubCalendarEvent SubEvent)
            {
                unstitchedEvents.Add(SubEvent);
            }


            public void setUnStitchedEvents(IEnumerable<SubCalendarEvent >SubEvents)
            {
                unstitchedEvents = SubEvents.ToList();
            }

            public void clearUnStitchedEvents()
            {
                unstitchedEvents.Clear();
            }

            

            public void ClearAcknowlwedgedSubEvents()
            {
                AcknowlegdedEvents.Clear();
                DeubggingAcknowledgement.Clear();
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

            public List<SubCalendarEvent> getAcknowledgedSubevents()
            {
                return AcknowlegdedEvents.ToList();
            }
        }
        
    }
}
