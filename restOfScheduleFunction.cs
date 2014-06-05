Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithListForFunCall = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
                        Dictionary<SubCalendarEvent, BusyTimeLine> SubCalendarEvent_OldTImeLine = new Dictionary<SubCalendarEvent, BusyTimeLine>();//Dictionary stores the Subcalendar event old TimeLine, just incase the do not get reassigned to the current timeline

                        foreach (mTuple<double, SubCalendarEvent> eachmTuple in CompatibleWithTimeLine)
                        {
                            TimeSpan ActiveTimeSpan = eachmTuple.Item2.ActiveDuration;
                            string subcalStringID = eachmTuple.Item2.ID;
                            SubCalendarEvent_OldTImeLine.Add(eachmTuple.Item2, eachmTuple.Item2.ActiveSlot.CreateCopy());

                            if (CompatibleWithListForFunCall.ContainsKey(ActiveTimeSpan))
                            {
                                ++CompatibleWithListForFunCall[ActiveTimeSpan].Item1;
                                ;
                            }
                            else
                            {
                                CompatibleWithListForFunCall.Add(ActiveTimeSpan, new mTuple<int, TimeSpanWithStringID>(1, new TimeSpanWithStringID(eachmTuple.Item2.ActiveDuration, ActiveTimeSpan.Ticks.ToString())));
                            }

                            if (PossibleEventsForFuncCall.ContainsKey(ActiveTimeSpan))
                            {
                                PossibleEventsForFuncCall[ActiveTimeSpan].Add(subcalStringID, new mTuple<bool, SubCalendarEvent>(true, eachmTuple.Item2));
                            }
                            else
                            {
                                PossibleEventsForFuncCall.Add(ActiveTimeSpan, new Dictionary<string, mTuple<bool, SubCalendarEvent>>());
                                PossibleEventsForFuncCall[ActiveTimeSpan].Add(subcalStringID, new mTuple<bool, SubCalendarEvent>(true, eachmTuple.Item2));
                            }
                        }



                        List<mTuple<bool, SubCalendarEvent>> UpdatedListForTimeLine = stitchUnRestrictedSubCalendarEvent(eachTimeLine, restrictedForFuncCall, PossibleEventsForFuncCall, CompatibleWithListForFunCall, Occupancy);//attempts to add new events into to the timelines with lesser occupancy than average

                        TimeSpan OccupiedSpace = Utility.SumOfActiveDuration(UpdatedListForTimeLine.Select(obj => obj.Item2).ToList());//checks for how much space is used up
                        TimeSpan ExcessSpace = OccupiedSpace - AverageTimeSpan;//checks how much excees
                        if (ExcessSpace.Ticks > 0)//tries to trim the UpdatedListForTimeLine. This is done by removing an element in the updated list until its detected that the origin timeline is below or equal to its average
                        {
                            IEnumerable<SubCalendarEvent> NewlyAddedElements = (UpdatedListForTimeLine.Where(obj => !AlreadyAlignedEvents[j].Contains(obj.Item2))).Select(obj => obj.Item2);//retrieves the newly added elements
                            List<mTuple<double, SubCalendarEvent>> NewlyAddedElementsWithCost = NewlyAddedElements.Select(obj => new mTuple<double, SubCalendarEvent>(DistanceSolver.AverageToAllNodes(obj.myLocation, UpdatedListForTimeLine.Select(obj3 => obj3.Item2).Where(obj1 => obj1 != obj).ToList().Select(obj2 => obj2.myLocation).ToList()), obj)).ToList();//creates mtuple of cost and subcal events
                            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibilityListFOrTIghtestForExtraAverga = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
                            Dictionary<TimeSpan, List<mTuple<double, SubCalendarEvent>>> CompatibilityListForNewlyAddedElements = new Dictionary<TimeSpan, List<mTuple<double, SubCalendarEvent>>>();
                            foreach (mTuple<double, SubCalendarEvent> eachmTUple in NewlyAddedElementsWithCost)
                            {
                                if (CompatibilityListForNewlyAddedElements.ContainsKey(eachmTUple.Item2.ActiveDuration))
                                {
                                    CompatibilityListForNewlyAddedElements[eachmTUple.Item2.ActiveDuration].Add(eachmTUple);
                                    //CompatibilityListForNewlyAddedElements[eachmTUple.Item2.ActiveDuration] = Utility.RandomizeIEnumerable(CompatibilityListForNewlyAddedElements[eachmTUple.Item2.ActiveDuration]);
                                    CompatibilityListForNewlyAddedElements[eachmTUple.Item2.ActiveDuration].Sort(delegate(mTuple<double, SubCalendarEvent> A, mTuple<double, SubCalendarEvent> B)
                                    {
                                        return A.Item1.CompareTo(B.Item1);
                                    });
                                }
                                else
                                {
                                    CompatibilityListForNewlyAddedElements.Add(eachmTUple.Item2.ActiveDuration, new List<mTuple<double, SubCalendarEvent>>() { eachmTUple });
                                }

                                if (CompatibilityListFOrTIghtestForExtraAverga.ContainsKey(eachmTUple.Item2.ActiveDuration))
                                {
                                    ++CompatibilityListFOrTIghtestForExtraAverga[eachmTUple.Item2.ActiveDuration].Item1;
                                }
                                else
                                {
                                    CompatibilityListFOrTIghtestForExtraAverga.Add(eachmTUple.Item2.ActiveDuration, new mTuple<int, TimeSpanWithStringID>(1, new TimeSpanWithStringID(eachmTUple.Item2.ActiveDuration, eachmTUple.Item2.ActiveDuration.Ticks.ToString())));
                                }
                            }

                            TimeSpan Space_NonAverage = TimeSpan.FromTicks((long)((1 - AverageOccupiedSchedule) * eachTimeLine.TimelineSpan.Ticks));//Space derive from subtracting the calculated expected average timespan for this time line from thie timeline
                            //ExcessSpace = Space_NonAverage;

                            SnugArray CompatibilityToBestAverageFit = new SnugArray(CompatibilityListFOrTIghtestForExtraAverga.Values.ToList(), ExcessSpace);
                            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AllPossibleTIghtExcessFits = CompatibilityToBestAverageFit.MySnugPossibleEntries;
                            AllPossibleTIghtExcessFits =SnugArray.SortListSnugPossibilities_basedOnTimeSpan(AllPossibleTIghtExcessFits);
                            Dictionary<int, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> tightestElements = SnugArray.SortListSnugPossibilities_basedOnNumberOfDiffering(AllPossibleTIghtExcessFits);
                            if (tightestElements.Count > 0)
                            { 
                                AllPossibleTIghtExcessFits = tightestElements.OrderBy(obj => obj.Key).Last().Value; 
                            }
                                
                                //SnugArray.SortListSnugPossibilities_basedOnTimeSpan(AllPossibleTIghtExcessFits);
                            List<mTuple<double, SubCalendarEvent>> removedElements = new List<mTuple<double, SubCalendarEvent>>();//stores element that dont get reassigned to this current timeLine
                            if (AllPossibleTIghtExcessFits.Count > 0)
                            {
                                Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> TIghtestFit = AllPossibleTIghtExcessFits[AllPossibleTIghtExcessFits.Count - 1];
                                foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in TIghtestFit)//Hack alert: Assumes tightest fit is most diverse
                                {

                                    while (eachKeyValuePair.Value.Item1 > 0)
                                    {
                                        removedElements.Add(CompatibilityListForNewlyAddedElements[eachKeyValuePair.Value.Item2.timeSpan][CompatibilityListForNewlyAddedElements[eachKeyValuePair.Value.Item2.timeSpan].Count - 1]);
                                        CompatibilityListForNewlyAddedElements[eachKeyValuePair.Value.Item2.timeSpan].RemoveAt(CompatibilityListForNewlyAddedElements[eachKeyValuePair.Value.Item2.timeSpan].Count - 1);
                                        --eachKeyValuePair.Value.Item1;
                                    }
                                }
                            }

                            NewlyAddedElements = CompatibilityListForNewlyAddedElements.SelectMany(obj => obj.Value).Select(obj => obj.Item2);
                            UpdatedListForTimeLine.RemoveAll(obj => removedElements.Select(obj1 => obj1.Item2).Contains(obj.Item2));//use LINQ to remove elements currently in "removedElements"
                            List<SubCalendarEvent> ListOfNewlyAddeedElements = NewlyAddedElements.ToList();

                            TimeSpan AllSumTImeSpan = Utility.SumOfActiveDuration(ListOfNewlyAddeedElements);
                            removedElements.ForEach(obj => obj.Item2.shiftEvent(SubCalendarEvent_OldTImeLine[obj.Item2].Start - obj.Item2.ActiveSlot.Start));


                            Dictionary<int, List<List<SubCalendarEvent>>> CurrentlyAssignedSubCalevents = new Dictionary<int, List<List<SubCalendarEvent>>>();//stores the index of each subcalendarevent timeline and its fellow Subcal events. Key= Index Of Current timeline. OuteList is grouping for each calendar event. Inner List is each Subcalevent within Calevent

                            ListOfNewlyAddeedElements=Utility.RandomizeIEnumerable(ListOfNewlyAddeedElements);
                            
                            for (int i = 0; i < ListOfNewlyAddeedElements.Count; i++)//removes Each reassigned element from its currently attached field
                            {
                                SubCalendarEvent eachSubCalendarEvent = ListOfNewlyAddeedElements[i];
                                Tuple<int, TimeLine> CurrentMatchingField = SubCalEventCurrentlyAssignedTImeLine[eachSubCalendarEvent];
                                mTuple<TimeSpan, TimeSpan> AverageTimeSpanAndTotalTimeSpan = TImeLine_ToAverageTimeSpan[CurrentMatchingField.Item2];
                                if (((AverageTimeSpanAndTotalTimeSpan.Item2 - eachSubCalendarEvent.ActiveDuration) >= AverageTimeSpanAndTotalTimeSpan.Item1))
                                {
                                    AverageTimeSpanAndTotalTimeSpan.Item2 -= eachSubCalendarEvent.ActiveDuration;
                                    AlreadyAlignedEvents[CurrentMatchingField.Item1].Remove(eachSubCalendarEvent);
                                    SubCalEventCurrentlyAssignedTImeLine[eachSubCalendarEvent] = new Tuple<int, TimeLine>(j, AllFreeSpots[j]);

                                    Dictionary<TimeLine, Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> AllPossibleEvents222222;
                                    TotalMovableList.RemoveAll(obj => NewlyAddedElements.Contains(obj.Item2));//removes the newly added element from Total possible movable elements
                                    AllPossibleEvents[AllFreeSpots[j]][eachSubCalendarEvent.ActiveDuration].Remove(eachSubCalendarEvent.ID);
                                    if (AllPossibleEvents[AllFreeSpots[j]][eachSubCalendarEvent.ActiveDuration].Count < 1)
                                    {
                                        AllPossibleEvents[AllFreeSpots[j]].Remove(eachSubCalendarEvent.ActiveDuration);
                                    }
                                }
                                else
                                {
                                    ListOfNewlyAddeedElements.Remove(eachSubCalendarEvent);
                                    eachSubCalendarEvent.shiftEvent(SubCalendarEvent_OldTImeLine[eachSubCalendarEvent].Start - eachSubCalendarEvent.ActiveSlot.Start);
                                    --i;
                                }
                            }
                            
                            NewlyAddedElements = ListOfNewlyAddeedElements;

                            AlreadyAlignedEvents[j].AddRange(NewlyAddedElements);

                            TotalActiveSpan = Utility.SumOfActiveDuration(AlreadyAlignedEvents[j]);
                            AverageTimeSpan = new TimeSpan((long)(AverageOccupiedSchedule * (double)eachTimeLine.TimelineSpan.Ticks));
                            Occupancy = (double)TotalActiveSpan.Ticks / (double)eachTimeLine.TimelineSpan.Ticks;// percentage of active duration relative to the size of the TimeLine Timespan

                        }