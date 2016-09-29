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
        Location_Elements DefaultLocation = new Location_Elements();
        Dictionary<SubCalendarEvent, Dictionary<Reason.Options, Reason>> subEventToReason = new Dictionary<SubCalendarEvent, Dictionary<Reason.Options, Reason>>();
        Dictionary<SubCalendarEvent, Dictionary<TimeOfDayPreferrence.DaySection, Dictionary<int, HashSet<int>>>> subEvent_Dict_To_DaySecion = new Dictionary<SubCalendarEvent, Dictionary<TimeOfDayPreferrence.DaySection, Dictionary<int, HashSet<int>>>>();
        /// <summary>
        /// This holds the subevents that cannot fit anywhere within this and also have no partial timefram that works
        /// </summary>
        HashSet<SubCalendarEvent> NotInvolvedIncalculation = new HashSet<SubCalendarEvent>();
        public OptimizedPath(DayTimeLine DayData, Location_Elements home = null)
        {
            DayInfo = DayData;

            List<SubCalendarEvent> subEventsThatCannotExist = DayData.getSubEventsInDayTimeLine().Where(subEvent => !subEvent.canExistWithinTimeLine(DayData)).ToList();
            if (subEventsThatCannotExist.Count > 0)
            {
                Dictionary<SubCalendarEvent, TimeLine> subEventToViableTimeLine = subEventsThatCannotExist.ToDictionary(SubEvent => SubEvent, SubEvent => DayData.InterferringTimeLine(SubEvent.RangeTimeLine));
                HashSet<SubCalendarEvent> allSubEvents = new HashSet<SubCalendarEvent>(DayData.getSubEventsInDayTimeLine());
                HashSet<SubCalendarEvent> tempSubEvents = new HashSet<SubCalendarEvent>();
                foreach (SubCalendarEvent subEvent in subEventToViableTimeLine.Keys)
                {
                    allSubEvents.Remove(subEvent);
                    TimeLine interferringTimeline = subEventToViableTimeLine[subEvent];
                    if (interferringTimeline != null)
                    {
                        SubCalendarEvent slicedValidSubEvent = new SubCalendarEvent(subEvent.Id, interferringTimeline.Start, interferringTimeline.End, new BusyTimeLine(subEvent.Id, interferringTimeline.Start, interferringTimeline.End), subEvent.Rigid, subEvent.isEnabled, subEvent.UIParam, subEvent.Notes, subEvent.isComplete, subEvent.myLocation, subEvent.getCalendarEventRange, subEvent.Conflicts, subEvent.CreatorID);
                        tempSubEvents.Add(slicedValidSubEvent);
                    }
                    else
                    {
                        NotInvolvedIncalculation.Add(subEvent);
                    }
                }

                DayInfo = new DayTimeLine(DayData.Start, DayData.End, DayInfo.UniversalIndex, DayInfo.BoundedIndex);
                DayInfo.AddToSubEventList(allSubEvents.Concat(tempSubEvents));
            }

            TimeSpan TotalDuration = SubCalendarEvent.TotalActiveDuration(DayData.getSubEventsInDayTimeLine());
            DefaultLocation = Location_Elements.AverageGPSLocation(DayInfo.getSubEventsInDayTimeLine().Select(obj => obj.myLocation), false);
            if (home == null)
            {
                home = DefaultLocation.CreateCopy();
            }

            AllGroupings.Add(TimeOfDayPreferrence.DaySection.Morning, new OptimizedGrouping(TimeOfDayPreferrence.DaySection.Morning, TotalDuration, DefaultLocation.CreateCopy()));
            AllGroupings.Add(TimeOfDayPreferrence.DaySection.Afternoon, new OptimizedGrouping(TimeOfDayPreferrence.DaySection.Afternoon, TotalDuration, DefaultLocation.CreateCopy()));
            AllGroupings.Add(TimeOfDayPreferrence.DaySection.Evening, new OptimizedGrouping(TimeOfDayPreferrence.DaySection.Evening, TotalDuration, DefaultLocation.CreateCopy()));
            AllGroupings.Add(TimeOfDayPreferrence.DaySection.Sleep, new OptimizedGrouping(TimeOfDayPreferrence.DaySection.Sleep, TotalDuration, home));
            AllGroupings.Add(TimeOfDayPreferrence.DaySection.None, new OptimizedGrouping(TimeOfDayPreferrence.DaySection.None, TotalDuration, DefaultLocation.CreateCopy()));
        }

        public void OptimizePath()
        {
            foreach (SubCalendarEvent subEvent in DayInfo.getSubEventsInDayTimeLine())
            {
                subEvent.InitializeDayPreference(DayInfo);
                subEvent_Dict_To_DaySecion.Add(subEvent, new Dictionary<TimeOfDayPreferrence.DaySection, Dictionary<int, HashSet<int>>>());
            }

            while (true)
            {
                subEventToReason = DayInfo.getSubEventsInDayTimeLine().ToDictionary(subEvent => subEvent, subEVent => new Dictionary<Reason.Options, Reason>());
                List<SubCalendarEvent> AllSubCalendarEvents = DayInfo.getSubEventsInDayTimeLine();
                List<SubCalendarEvent> CurrentlyValid = AllSubCalendarEvents
                    //.Where(obj => (!obj.Rigid))
                    .Where(obj => (!obj.isOptimized)).Where(obj =>
                    {
                        var TimeOfDay = obj.getDaySection().getCurrentDayPreference();
                        return ((TimeOfDay != TimeOfDayPreferrence.DaySection.Disabled));
                    }).ToList();
                if (CurrentlyValid.Count > 0)
                {
                    ILookup<TimeOfDayPreferrence.DaySection, SubCalendarEvent> SubEventRegrouping = groupEvents(CurrentlyValid, DayInfo);

                    List<OptimizedGrouping> DaySectorGrouping = new List<OptimizedGrouping>();
                    foreach (IGrouping<TimeOfDayPreferrence.DaySection, SubCalendarEvent> eachIGrouping in SubEventRegrouping)
                    {
                        if (eachIGrouping.Key != TimeOfDayPreferrence.DaySection.Disabled)
                        {
                            AllGroupings[eachIGrouping.Key].clearPathStitchedEvents();
                            OptimizeGrouping(AllGroupings[eachIGrouping.Key], SubEventRegrouping[eachIGrouping.Key], DayInfo);
                        }
                    }



                    //AllGroupings.SelectMany(obj => obj.Value.getPathStitchedSubevents());
                    OptimizedGrouping.buildStitchers(AllGroupings.Select(obj => obj.Value));
                    StitchAllGroupings();
                }
                else
                {
                    List<SubCalendarEvent> BestOrder = AllSubCalendarEvents.OrderBy(obj => obj.Start).ToList();
                    List<Location_Elements> BestOrderLocations = BestOrder.Select(obj => obj.myLocation).ToList();
                    List<SubCalendarEvent> NoPosition = AllSubCalendarEvents.Where(obj => (!obj.Rigid)).Where(obj => (!obj.isOptimized)).Where(obj =>
                    {
                        var TimeOfDay = obj.getDaySection().getCurrentDayPreference();
                        //return ((TimeOfDay != TimeOfDayPreferrence.DaySection.Disabled)&&(TimeOfDay!=TimeOfDayPreferrence.DaySection.None));
                        return ((TimeOfDay != TimeOfDayPreferrence.DaySection.Disabled));
                    }).ToList();
                    optimizeDisabledEvents();
                    break;
                }
            }
        }

        void optimizeDisabledEvents()
        {
            List<SubCalendarEvent> disabledSubEvents = DayInfo.getSubEventsInDayTimeLine().Where(subEvent => subEvent.getDaySection().getCurrentDayPreference() == TimeOfDayPreferrence.DaySection.Disabled).ToList();
            if (disabledSubEvents.Count > 0)
            {
                List<SubCalendarEvent> correctlyAssignedevents = DayInfo.getSubEventsInDayTimeLine().Except(disabledSubEvents).OrderBy(obj=>obj.Start).ToList();

                Tuple<Dictionary<TilerEvent, double>, Tuple<Location_Elements, DateTimeOffset, TimeSpan>> evaluatedParams = evalulateParameter(disabledSubEvents, null);
                Dictionary<TilerEvent, double> evaluatedEvents = evaluatedParams.Item1;
                List<KeyValuePair<TilerEvent, double>> subEventsEvaluated = evaluatedEvents.OrderBy(obj => obj.Value).ToList();

                foreach (SubCalendarEvent disabledSubEvent in subEventsEvaluated.Select(obj => obj.Key))
                {
                    List<SubCalendarEvent> reOptimizdSubevents = optimizeDisabledEvent(DayInfo, correctlyAssignedevents, disabledSubEvent);
                    if (Utility.PinSubEventsToStart(reOptimizdSubevents, DayInfo))
                    {
                        correctlyAssignedevents = reOptimizdSubevents.ToList();
                    }
                    else
                    {
                        throw new Exception("there seems to be an error with pinning subevents in a disabled list");
                    }
                }
            }




        }


        public List<SubCalendarEvent> optimizeDisabledEvent(TimeLine timeLine, List<SubCalendarEvent> correctlyAssignedevents, SubCalendarEvent disabledSubEvent)
        {
            if (disabledSubEvent.canExistWithinTimeLine(timeLine))
            {
                Dictionary<SubCalendarEvent, int> correctlyAssignedeventsToIndex = new Dictionary<SubCalendarEvent, int>();
                int i;
                for (i = 0; i < correctlyAssignedevents.Count; i++)
                {
                    var subEvent = correctlyAssignedevents[i];
                    correctlyAssignedeventsToIndex.Add(subEvent, i);
                }
                Dictionary<SubCalendarEvent, mTuple<TimeLine, TimeLine>> subEventToAvailableSpaces = subEventToMaxSpaceAvailable(timeLine, correctlyAssignedevents);
                i = 0;
                List<SubCalendarEvent> fittable = new List<SubCalendarEvent>();
                HashSet<int> validIndexes = new HashSet<int>();

                //for (i = 0; i < disabledSubEvents.Count; i++)
                {
                    //disabledSubEvent = disabledSubEvents[i];
                    foreach (KeyValuePair<SubCalendarEvent, mTuple<TimeLine, TimeLine>> keyValuePair in subEventToAvailableSpaces)
                    {
                        if (disabledSubEvent.canExistWithinTimeLine(keyValuePair.Value.Item1))
                        {
                            fittable.Add(disabledSubEvent);
                            validIndexes.Add(correctlyAssignedeventsToIndex[keyValuePair.Key]);
                        }

                        if (disabledSubEvent.canExistWithinTimeLine(keyValuePair.Value.Item2))
                        {
                            fittable.Add(disabledSubEvent);
                            validIndexes.Add(correctlyAssignedeventsToIndex[keyValuePair.Key] + 1);
                        }
                    }
                }

                List<SubCalendarEvent> subEventsReadjusted = correctlyAssignedevents.ToList();



                List<int> orderedIndexes = validIndexes.OrderBy(validIndex => validIndex).ToList();



                List<int> invalidIndexes = new List<int>();
                for (i = 0; i < correctlyAssignedevents.Count + 1; i++)
                {
                    if (validIndexes.Count > 0)
                    {
                        if (i == orderedIndexes[0])
                        {
                            orderedIndexes.RemoveAt(0);
                        }
                        else
                        {
                            invalidIndexes.Add(i);
                        }
                    }
                    else
                    {
                        invalidIndexes.Add(i);
                    }
                }
                int index = getBestPosition(timeLine, disabledSubEvent, correctlyAssignedevents, new HashSet<int>(invalidIndexes));
                if (index == -1)
                {
                    index = getBestPosition(timeLine, disabledSubEvent, correctlyAssignedevents, new HashSet<int>());
                }
                subEventsReadjusted.Insert(index, disabledSubEvent);

                if (!Utility.PinSubEventsToStart(subEventsReadjusted, timeLine))// try to pin subevent for readjust
                {
                    DateTimeOffset start = timeLine.Start;
                    DateTimeOffset end = timeLine.End;
                    if (index == 0)// if it is the new starting element then pin to beginning
                    {
                        TimeLine revisedTimeLine = new TimeLine(start, end);
                        revisedTimeLine = disabledSubEvent.getTimeLineInterferringWithCalEvent(revisedTimeLine).FirstOrDefault();
                        if (revisedTimeLine != null)
                        {
                            disabledSubEvent.shiftEvent(revisedTimeLine.Start);
                        }
                        else
                        {
                            throw new Exception("there is an issue with pathoptimization of a disabled event");
                        }
                    }
                    else
                    {
                        if (index == subEventsReadjusted.Count - 1)// if is the last element, then pin it to the end
                        {
                            TimeLine revisedTimeLine = new TimeLine(start, end);
                            revisedTimeLine = disabledSubEvent.getTimeLineInterferringWithCalEvent(revisedTimeLine).LastOrDefault();
                            if (revisedTimeLine != null)
                            {
                                disabledSubEvent.shiftEvent(revisedTimeLine.End - disabledSubEvent.ActiveDuration);
                            }
                            else
                            {
                                throw new Exception("there is an issue with pathoptimization of a disabled event");
                            }
                        }
                        else
                        {
                            int beforeIndex = index - 1;
                            int afterIndex = index + 1;
                            SubCalendarEvent before = correctlyAssignedevents[beforeIndex];
                            SubCalendarEvent after = correctlyAssignedevents[afterIndex];
                            TimeLine relevantTimeline = new TimeLine(before.Start, after.End);
                            TimeLine revisedOverlappingTImeline = disabledSubEvent.getTimeLineInterferringWithCalEvent(relevantTimeline).FirstOrDefault();
                            if (revisedOverlappingTImeline != null)
                            {
                                TimeLine centralizedTimeLine = Utility.CentralizeYourSelfWithinRange(revisedOverlappingTImeline, disabledSubEvent.ActiveDuration);
                                disabledSubEvent.shiftEvent(centralizedTimeLine.Start);
                                List<SubCalendarEvent> interferringSubEVentsEvents = subEventsReadjusted.Where(subEvent => subEvent.RangeTimeLine.doesTimeLineInterfere(centralizedTimeLine)).ToList();

                                BlobSubCalendarEvent bloberrized = new BlobSubCalendarEvent(interferringSubEVentsEvents);
                                int firstRemovedElement = subEventsReadjusted.IndexOf(interferringSubEVentsEvents[0]);
                                subEventsReadjusted.RemoveRange(firstRemovedElement, interferringSubEVentsEvents.Count);


                                subEventsReadjusted.Insert(firstRemovedElement, bloberrized);
                            }
                        }
                    }
                }
                return subEventsReadjusted;
            }

            throw new Exception("disabledSubEvent cannot exist within timeLine, consider providing a a time line where disabledSubEvent can exist");
            
        }

        Tuple<Dictionary<TilerEvent, double>, Tuple<Location_Elements, DateTimeOffset, TimeSpan>> evalulateParameter(IEnumerable<SubCalendarEvent> events, OptimizedGrouping.OptimizedAverage optimizedAverage)
        {
            Dictionary<TilerEvent, List<double>> dimensionsPerEvent = new Dictionary<TilerEvent, List<double>>();
            Dictionary<string, uint> fibboIndexes = new Dictionary<string, uint>();
            Location_Elements avgLocation;
            if (events.Where(eve => eve.Rigid).Count() > 0)// if there are rigids, let the rigid be the average location
            {
                avgLocation = Location_Elements.AverageGPSLocation(events.Where(eve => eve.Rigid).Select(obj => obj.myLocation));
            }
            else
            {
                avgLocation = Location_Elements.AverageGPSLocation(events.Select(obj => obj.myLocation));
            }

            if (optimizedAverage != null)
            {
                avgLocation = Location_Elements.AverageGPSLocation(new List<Location_Elements> (optimizedAverage?.SubEvents.Select(subevent => subevent.myLocation)));
            }


            foreach (TilerEvent Event in events)
            {
                double distance = Location_Elements.calculateDistance(Event.myLocation, avgLocation, 100);
                List<double> parameters = new List<double>();

                uint multiplier = 1;
                uint fiboindex = 1;
                string calendarID = Event.TilerID.getCalendarEventComponent();

                if (fibboIndexes.ContainsKey(calendarID))
                {
                    fiboindex = fibboIndexes[calendarID];
                    fibboIndexes[calendarID] = fiboindex + 1;
                    multiplier = Utility.getFibonnacciNumber(fiboindex);
                }
                else
                {
                    fibboIndexes.Add(calendarID, 1);
                }
                distance *= multiplier;
                parameters.Add(distance);
                dimensionsPerEvent.Add(Event, parameters);
            }
            fibboIndexes = new Dictionary<string, uint>();
            double sum = (double)events.Sum(obj => (obj.Deadline - Utility.StartOfTime).TotalSeconds);
            double averageRatio = sum / events.Count();
            long AverageTicks = TimeSpan.FromSeconds(averageRatio).Ticks; //(long)events.Average(obj => obj.Deadline.Ticks);
            //long AverageTicks = 0; //(long)events.Average(obj => obj.Deadline.Ticks);
            DateTimeOffset latestDeadline = new DateTimeOffset(AverageTicks, new TimeSpan());
            ///deals with scenarios where the deadline is later or earlier. the earlier the higher up in hierachy.
            foreach (TilerEvent Event in events)
            {
                double deadlineRatio = (double)Event.Deadline.Ticks / AverageTicks;
                List<double> parameters = dimensionsPerEvent[Event];

                uint multiplier = 1;
                uint fiboindex = 1;
                string calendarID = Event.TilerID.getCalendarEventComponent();

                if (fibboIndexes.ContainsKey(calendarID))
                {
                    fiboindex = fibboIndexes[calendarID];
                    fibboIndexes[calendarID] = fiboindex + 1;
                    multiplier = Utility.getFibonnacciNumber(fiboindex);
                }
                else
                {
                    fibboIndexes.Add(calendarID, 1);
                }
                deadlineRatio *= multiplier;
                parameters.Add(deadlineRatio);
            }
            fibboIndexes = new Dictionary<string, uint>();
            long AverageDurationTicks = (long)events.Average(obj => obj.ActiveDuration.Ticks);
            ///deals with scenarios with duration. The bigger the duration the higher up it is. Hence the more the ticks the ratio is in the for loop
            //foreach (TilerEvent Event in events)
            //{
            //    double durationRatio = (double)AverageDurationTicks / Event.ActiveDuration.Ticks;
            //    List<double> parameters = dimensionsPerEvent[Event];

            //    uint multiplier = 1;
            //    uint fiboindex = 1;
            //    string calendarID = Event.TilerID.getCalendarEventComponent();

            //    if (fibboIndexes.ContainsKey(calendarID))
            //    {
            //        fiboindex = fibboIndexes[calendarID];
            //        fibboIndexes[calendarID] = fiboindex + 1;
            //        multiplier = Utility.getFibonnacciNumber(fiboindex);
            //    }
            //    else
            //    {
            //        fibboIndexes.Add(calendarID, 1);
            //    }
            //    durationRatio *= multiplier;
            //    parameters.Add(durationRatio);
            //}


            List<double> origin = new List<double>() { 0, 0 };//, 0};


            Dictionary<TilerEvent, double> retValueDict = new Dictionary<TilerEvent, double>();
            List<TilerEvent> subeEVents = dimensionsPerEvent.OrderBy(obj => obj.Key.TilerID.getCalendarEventComponent()).Select(obj => obj.Key).ToList();
            List<List<double>> allCalcs = subeEVents.Select(obj => dimensionsPerEvent[obj]).ToList();
            IList<IList<double>> multidimensionalListOfValues = dimensionsPerEvent.Values.Select(obj => ((IList<double>)obj)).ToList();
            List<double> evalaution = Utility.multiDimensionCalculationNormalize(multidimensionalListOfValues, origin);
            int counter = 0;
            foreach (TilerEvent eachEvent in events)
            {
                if (counter > events.Count())
                {
                    throw new Exception("moreEvents than multidimensional calculation");
                }
                retValueDict.Add(eachEvent, evalaution[counter]);
                counter++;
            }

            Tuple<Location_Elements, DateTimeOffset, TimeSpan> retValueTuple = new Tuple<Location_Elements, DateTimeOffset, TimeSpan>(avgLocation, latestDeadline, new TimeSpan(AverageDurationTicks));
            Tuple<Dictionary<TilerEvent, double>, Tuple<Location_Elements, DateTimeOffset, TimeSpan>> retValue = new Tuple<Dictionary<TilerEvent, double>, Tuple<Location_Elements, DateTimeOffset, TimeSpan>>(retValueDict, retValueTuple);
            return retValue;
        }

        void updateSubeventReason(SubCalendarEvent subEvent, Reason reason)
        {
            Dictionary<Reason.Options, Reason> reasonToOptions = subEventToReason[subEvent];
            if (reasonToOptions.ContainsKey(reason.Option))
            {
                reasonToOptions[reason.Option] = reason;
            }
            else
            {
                reasonToOptions.Add(reason.Option, reason);
            }
        }

        Dictionary<SubCalendarEvent, mTuple<TimeLine, TimeLine>> subEventToMaxSpaceAvailable(TimeLine maxTImeLine, IEnumerable<SubCalendarEvent> subEvents)
        {
            List<SubCalendarEvent> ordedsubEvents = subEvents.ToList();
            Dictionary<SubCalendarEvent, mTuple<TimeLine, TimeLine>> retValue = new Dictionary<SubCalendarEvent, mTuple<TimeLine, TimeLine>>();
            TimeLine timeLine = new TimeLine();
            DateTimeOffset start = maxTImeLine.Start;
            DateTimeOffset end = maxTImeLine.End;
            DateTimeOffset startBefore = maxTImeLine.Start;
            DateTimeOffset endBefore = maxTImeLine.End;
            DateTimeOffset startAfter = maxTImeLine.Start;
            DateTimeOffset endAfter = maxTImeLine.End;



            TimeLine iterationTImeLine = new TimeLine(start, end);
            TimeLine timeLineBefore = new TimeLine();
            TimeLine timeLineAfter = new TimeLine();
            if (Utility.PinSubEventsToStart(ordedsubEvents, maxTImeLine))
            {
                for (int i = 0; i < ordedsubEvents.Count; i++)
                {
                    List<SubCalendarEvent> subList = ordedsubEvents.Skip(i).ToList();
                    SubCalendarEvent anchorIterationEvent = subList.First();
                    if (Utility.PinSubEventsToEnd(subList, iterationTImeLine))
                    {
                        startBefore = iterationTImeLine.Start;
                        endBefore = anchorIterationEvent.Start;
                        timeLineBefore = new TimeLine(startBefore, endBefore);
                    }

                    if (Utility.PinSubEventsToStart(subList, iterationTImeLine))
                    {
                        startAfter = anchorIterationEvent.End;
                        endAfter = subList.Count > 1 ? subList[1].Start : maxTImeLine.End;
                        timeLineAfter = new TimeLine(startAfter, endAfter);
                    }
                    mTuple<TimeLine, TimeLine> tupleData = new mTuple<TimeLine, TimeLine>(timeLineBefore, timeLineAfter);
                    retValue.Add(anchorIterationEvent, tupleData);
                }
            }
            else
            {
                throw new Exception("There is a problem pinning the first initial bunch of elements in subEventToMaxSpaceAvailable");
            }

            return retValue;
        }

        void OptimizeGrouping(OptimizedGrouping Grouping, IEnumerable<SubCalendarEvent> eventEntry, TimeLine timeLine)
        {
            List<SubCalendarEvent> AllEvents = eventEntry.ToList();
            List<SubCalendarEvent> Subevents = new List<SubCalendarEvent>();
            List<SubCalendarEvent> Stitched_Revised = Grouping.getPinnedEvents().OrderBy(obj => obj.Start).ToList();
            int initializingCount = Stitched_Revised.Count;
            int i = 0;

            List<SubCalendarEvent> fittable = new List<SubCalendarEvent>();
            if (Stitched_Revised.Count > 0)//if there are current events that are currently known to be stitched into the current day section
            {
                Dictionary<SubCalendarEvent, mTuple<TimeLine, TimeLine>> subEventToAvailableSpaces = subEventToMaxSpaceAvailable(timeLine, Stitched_Revised);
                bool NoTimeLineAvailable = true;//flag holds signal for if a viable space has been found. If no viable timeline is found then this this daysector is removed
                for (i = 0; i < AllEvents.Count; i++)
                {
                    SubCalendarEvent subEvent = AllEvents[i];//unacknowledged subevent for this grouping
                    foreach (KeyValuePair<SubCalendarEvent, mTuple<TimeLine, TimeLine>> keyValuePair in subEventToAvailableSpaces)//Loop checkss each timeline before and after an aknowledged subevent to see subEvent can exist
                    {
                        if (subEvent.canExistWithinTimeLine(keyValuePair.Value.Item1))
                        {
                            fittable.Add(subEvent);
                            NoTimeLineAvailable = false;//sets flagging signaling a timeline waas found
                            break;
                        }

                        if (subEvent.canExistWithinTimeLine(keyValuePair.Value.Item2))
                        {
                            fittable.Add(subEvent);
                            NoTimeLineAvailable = false;//sets flagging signaling a timeline waas found
                            break;

                        }
                    }
                    if (NoTimeLineAvailable)//If no viable timeline is found then this this daysector is removed
                    {
                        subEvent.getDaySection().rejectCurrentPreference(Grouping.DaySector);
                    }
                    

                }
            }
            else
            {
                fittable = AllEvents.ToList();
            }

            i = 0;
            AllEvents = fittable.ToList();
            Tuple<Dictionary<TilerEvent, double>, Tuple<Location_Elements, DateTimeOffset, TimeSpan>> evaluatedParams = evalulateParameter(AllEvents, Grouping.GroupAverage);
            Dictionary<TilerEvent, double> evaluatedEvents = evaluatedParams.Item1;
            List<KeyValuePair<TilerEvent, double>> subEventsEvaluated = evaluatedEvents.OrderBy(obj => obj.Value).ToList();

            IEnumerable<SubCalendarEvent> subEvents = (IEnumerable<SubCalendarEvent>)evaluatedEvents.OrderBy(obj => obj.Value).Select(obj => (SubCalendarEvent)obj.Key);
            List<String> locations = subEvents.Select(obj => "" + obj.myLocation.XCoordinate + "," + obj.myLocation.YCoordinate).ToList();
            Subevents = subEvents.Take(5).ToList();
            //Subevents= Utility.getBestPermutation(Subevents.ToList(), double.MaxValue, new Tuple<Location_Elements, Location_Elements>(Grouping.LeftBorder, Grouping.RightBorder)).ToList();
            Tuple<Location_Elements, Location_Elements> borderElements = null;
            //if (!Grouping.LeftBorder.isNull && !Grouping.RightBorder.isNull)
            //{
            //    borderElements = new Tuple<Location_Elements, Location_Elements>(Grouping.LeftBorder, Grouping.RightBorder);
            //}

            Subevents = Utility.getBestPermutation(Subevents.ToList(), borderElements, 0).ToList();
            Dictionary<SubCalendarEvent, int> subEventTOIndex = new Dictionary<SubCalendarEvent, int>();

            if (Stitched_Revised.Count == 0)
            {
                for (i = 0; i < Subevents.Count; i++)
                {
                    SubCalendarEvent mySubEvent = Subevents[i];
                    if (subEvent_Dict_To_DaySecion[mySubEvent].ContainsKey(Grouping.DaySector))
                    {
                        if (subEvent_Dict_To_DaySecion[mySubEvent][Grouping.DaySector].ContainsKey(initializingCount))
                        {
                            HashSet<int> unwantedIndexes = subEvent_Dict_To_DaySecion[mySubEvent][Grouping.DaySector][initializingCount];
                            int BestPostion = getBestPosition(mySubEvent, Stitched_Revised, unwantedIndexes);
                            if (BestPostion != -1)
                            {
                                Stitched_Revised.Insert(BestPostion, mySubEvent);
                                subEventTOIndex.Add(mySubEvent, BestPostion);
                            }
                            else
                            {
                                mySubEvent.getDaySection().rejectCurrentPreference();
                            }
                        }
                        else
                        {
                            Stitched_Revised.Add(mySubEvent);
                            subEventTOIndex.Add(mySubEvent, i);
                        }
                    }
                    else
                    {
                        Stitched_Revised.Add(mySubEvent);
                        subEventTOIndex.Add(mySubEvent, i);
                    }


                }
            }
            else
            {
                HashSet<int> unwantedIndexes = new HashSet<int>();
                for (i = 0; i < Subevents.Count; i++)
                {
                    unwantedIndexes = new HashSet<int>();
                    SubCalendarEvent mySubEvent = Subevents[i];

                    if (subEvent_Dict_To_DaySecion[mySubEvent].ContainsKey(Grouping.DaySector))
                    {
                        if (subEvent_Dict_To_DaySecion[mySubEvent][Grouping.DaySector].ContainsKey(initializingCount))
                        {
                            unwantedIndexes = subEvent_Dict_To_DaySecion[mySubEvent][Grouping.DaySector][initializingCount];
                        }
                    }


                    int BestPostion = getBestPosition(mySubEvent, Stitched_Revised, unwantedIndexes);
                    if (BestPostion != -1)
                    {
                        Stitched_Revised.Insert(BestPostion, mySubEvent);
                        subEventTOIndex.Add(mySubEvent, BestPostion);
                    }
                    else
                    {
                        mySubEvent.getDaySection().rejectCurrentPreference();
                    }

                }
            }

            List<SubCalendarEvent> noHistoryOfindexFailureEvents = Stitched_Revised.Intersect(Subevents).ToList();
            foreach (SubCalendarEvent subEvent in noHistoryOfindexFailureEvents)
            {
                if (!subEvent_Dict_To_DaySecion[subEvent].ContainsKey(Grouping.DaySector))
                {
                    HashSet<int> indexes = new HashSet<int>();
                    Dictionary<int, HashSet<int>> countToUnwantedIndex = new Dictionary<int, HashSet<int>>();
                    indexes.Add(subEventTOIndex[subEvent]);
                    countToUnwantedIndex.Add(initializingCount, indexes);
                    subEvent_Dict_To_DaySecion[subEvent].Add(Grouping.DaySector, countToUnwantedIndex);
                }
                else
                {
                    Dictionary<int, HashSet<int>> countToUnwantedIndexes = subEvent_Dict_To_DaySecion[subEvent][Grouping.DaySector];
                    if (countToUnwantedIndexes.ContainsKey(initializingCount))
                    {
                        countToUnwantedIndexes[initializingCount].Add(subEventTOIndex[subEvent]);
                    }
                    else
                    {
                        HashSet<int> indexes = new HashSet<int>();
                        countToUnwantedIndexes.Add(initializingCount, indexes);
                        indexes.Add(subEventTOIndex[subEvent]);
                    }
                }
            }

            LocationReason locationReason = new LocationReason(Stitched_Revised.Select(subEvent => subEvent.myLocation));
            DurationReason durationReason = new DurationReason();
            RestrictedEventReason restrictedReason = new RestrictedEventReason();
            Stitched_Revised.ForEach(subEvent =>
            {
                updateSubeventReason(subEvent, locationReason);
                if (subEvent.ActiveDuration > evaluatedParams.Item2.Item3)
                {
                    updateSubeventReason(subEvent, durationReason);
                }
                if (subEvent.isEventRestricted)
                {
                    updateSubeventReason(subEvent, restrictedReason);
                }
            });


            Grouping.setPathStitchedEvents(Stitched_Revised);
            //Grouping.setPathStitchedEvents(Subevents);
            //Grouping.updateSubEvents(Acknowledged_Revised);
        }

        int getBestPosition(TimeLine timeLine, SubCalendarEvent subEvent, IEnumerable<SubCalendarEvent> CurrentList, HashSet<int> unusableIndexes = null)
        {
            List<TimeLine> timeLineWorks = subEvent.getTimeLineInterferringWithCalEvent(timeLine);
            Dictionary<SubCalendarEvent, mTuple<SubCalendarEvent, int>> subeventToIndex = CurrentList.Select((obj, index) => new mTuple<SubCalendarEvent, int>(obj, index)).ToDictionary(obj => obj.Item1, obj => obj);
            List<SubCalendarEvent> allSubEvents = CurrentList.ToList();
            List<SubCalendarEvent> pinnedToStart = new List<SubCalendarEvent>();
            List<SubCalendarEvent> pinnedToEnd = new List<SubCalendarEvent>();
            if (Utility.PinSubEventsToStart(CurrentList, timeLine))
            {
                foreach (TimeLine timeLineWork in timeLineWorks)
                {
                    pinnedToStart.AddRange(CurrentList.Where(obj => obj.RangeTimeLine.doesTimeLineInterfere(timeLineWork)).OrderBy(obj => obj.Start));
                }
            }

            if (Utility.PinSubEventsToEnd(CurrentList, timeLine))
            {
                foreach (TimeLine timeLineWork in timeLineWorks)
                {
                    pinnedToEnd.AddRange(CurrentList.Where(obj => obj.RangeTimeLine.doesTimeLineInterfere(timeLineWork)).OrderBy(obj => obj.End));
                }
            }
            List<mTuple<SubCalendarEvent, int>> subEventsWithIndexes = new List<mTuple<SubCalendarEvent, int>>();

            foreach (SubCalendarEvent eachSubEvent in new HashSet<SubCalendarEvent>(pinnedToStart.Concat(pinnedToEnd)))
            {
                subEventsWithIndexes.Add(subeventToIndex[eachSubEvent]);
            }

            List<SubCalendarEvent> fullSublist = subEventsWithIndexes.OrderBy(obj => obj.Item2).Select(obj => obj.Item1).ToList();
            HashSet<int> updatedHashSet = new HashSet<int>();
            int startingIndex = 0;
            if (fullSublist.Count > 0)
            {
                startingIndex = allSubEvents.IndexOf(fullSublist[0]);
                foreach (int unusabeIndex in unusableIndexes)
                {
                    int newIndex = unusabeIndex - startingIndex;
                    if ((newIndex <= fullSublist.Count) && newIndex >= 0)
                    {
                        updatedHashSet.Add(newIndex);
                    }
                }
            }
            int retValue = getBestPosition(subEvent, fullSublist, updatedHashSet);
            if (retValue != -1)
            {
                retValue = startingIndex + retValue;
            }
            return retValue;
        }


        int getBestPosition(SubCalendarEvent SubEvent, IEnumerable<SubCalendarEvent> CurrentList, HashSet<int> unusableIndexes = null)
        {
            int i = 0;
            int currentCount = CurrentList.Count();
            List<SubCalendarEvent> FullList = CurrentList.ToList();
            double[] TotalDistances = new double[currentCount + 1];
            if (unusableIndexes != null)
            {
                int countLimit = TotalDistances.Count();
                foreach (int index in unusableIndexes)
                {
                    if (index < countLimit)
                    {
                        TotalDistances[index] = double.MaxValue;
                    }

                }
            }

            for (; i <= currentCount; i++)
            {
                if (!unusableIndexes.Contains(i))
                {
                    List<SubCalendarEvent> FullList_Copy = FullList.ToList();
                    FullList_Copy.Insert(i, SubEvent);
                    double TotalDistance = SubCalendarEvent.CalculateDistance(FullList_Copy,0, useFibonnacci: false);
                    TotalDistances[i] = TotalDistance;
                }

            }
            int RetValue = TotalDistances.MinIndex();
            if (unusableIndexes != null)
            {
                if (unusableIndexes.Contains(RetValue))
                {
                    RetValue = -1;
                }
            }
            return RetValue;
        }
        void StitchAllGroupings()
        {
            List<OptimizedGrouping> OrderedOptimizedGroupings = AllGroupings.Where(obj => obj.Key != TimeOfDayPreferrence.DaySection.None).OrderBy(obj => obj.Key).Select(obj => obj.Value).ToList();
            TimeLine myDayTimeLine = DayInfo.getJustTimeLine();
            List<SubCalendarEvent> SubEventsInrespectivepaths = OrderedOptimizedGroupings.SelectMany(obj => obj.getEventsForStitichingWithOtherOptimizedGroupings()).ToList();
            ///*hash set test
            if ((new HashSet<SubCalendarEvent>(SubEventsInrespectivepaths)).Count != SubEventsInrespectivepaths.Count)
            {

            }
            //*/
            List<SubCalendarEvent> SubEventsWithNoLocationPreference = AllGroupings[TimeOfDayPreferrence.DaySection.None].getPathStitchedSubevents();
            List<SubCalendarEvent> rigidSubeevents = DayInfo.getSubEventsInDayTimeLine().Where(obj => obj.Rigid).ToList();
            SubEventsInrespectivepaths.ForEach(subEVent => SubEventsWithNoLocationPreference.Remove(subEVent));

            ///*hash set test
            if ((new HashSet<SubCalendarEvent>(SubEventsInrespectivepaths)).Count != SubEventsInrespectivepaths.Count)
            {

            }
            //*/


            ///*hash set test
            if ((new HashSet<SubCalendarEvent>(SubEventsWithNoLocationPreference)).Count != SubEventsWithNoLocationPreference.Count)
            {

            }
            //*/


            List<SubCalendarEvent> splicedResults = spliceInNoneTimeOfDayPreferemce(SubEventsInrespectivepaths, SubEventsWithNoLocationPreference);

            ///*hash set test
            if ((new HashSet<SubCalendarEvent>(splicedResults)).Count != splicedResults.Count)
            {

            }
            //*/
            foreach (var grouping in OrderedOptimizedGroupings)
            {
                grouping.ClearPinnedSubEvents();
                grouping.clearPathStitchedEvents();
                //return grouping;
            }


            List<SubCalendarEvent> pinnedResults = tryPinningInCurrentDayTimeline(myDayTimeLine, splicedResults, rigidSubeevents);
            foreach (var grouping in OrderedOptimizedGroupings)
            {
                grouping.movePathStitchedToAcknowledged();
                //return grouping;
            }
            clearSubEventToReasonDictionary();
        }

        void clearSubEventToReasonDictionary()
        {
            subEventToReason = new Dictionary<SubCalendarEvent, Dictionary<Reason.Options, Reason>>();
        }

        List<SubCalendarEvent> spliceInNoneTimeOfDayPreferemce(IEnumerable<SubCalendarEvent> ArrangementWithoutNones, IEnumerable<SubCalendarEvent> SubEventsOfNone)
        {
            int IndexCount = ArrangementWithoutNones.Count() + 1;
            Queue<mTuple<int, SubCalendarEvent>>[] PreferredLocations = new Queue<mTuple<int, SubCalendarEvent>>[IndexCount];
            for (int i = 0; i < IndexCount; i++)
            {
                PreferredLocations[i] = new Queue<mTuple<int, SubCalendarEvent>>();
            }

            List<mTuple<int, SubCalendarEvent>> UnoptimizedSubEventsOfNone = SubEventsOfNone.Select((obj, i) => { return new mTuple<int, SubCalendarEvent>(i, obj); }).ToList();

            for (int i = 0; i < UnoptimizedSubEventsOfNone.Count; i++)
            {
                SubCalendarEvent RefSubaEvent = UnoptimizedSubEventsOfNone[i].Item2;
                int BestIndex = getBestPosition(RefSubaEvent, ArrangementWithoutNones, new HashSet<int>());
                PreferredLocations[BestIndex].Enqueue(UnoptimizedSubEventsOfNone[i]);// TODO you need to ensure that subevents with the same index still need to be optimized. To ensure that we stil so if you have A, B, C for index 0. This means they'll be associated with element 0 in ArrangementWithoutNones. So you ensure that a good path is still setup for this scenario
            }
            List<SubCalendarEvent> RetValue = ArrangementWithoutNones.ToList();
            int delta = 0;
            for (int i = 0; i < PreferredLocations.Length; i++)
            {
                Queue<mTuple<int, SubCalendarEvent>> Myqueue = PreferredLocations[i];
                int insertionIndex = i + delta;
                RetValue.InsertRange(insertionIndex, Myqueue.OrderBy(obj => obj.Item1).Select(obj => obj.Item2));
                delta += Myqueue.Count;
            }

            return RetValue;
        }


        List<SubCalendarEvent> tryPinningInCurrentDayTimeline(TimeLine AllTimeLine, List<SubCalendarEvent> SubEvents, IEnumerable<SubCalendarEvent> rigidevents)
        {
            List<SubCalendarEvent> retValue = new List<SubCalendarEvent>();
            NoReason Noreason = NoReason.getNoReasonInstanceFactory();
            if (Utility.PinSubEventsToStart(SubEvents, AllTimeLine))
            {
                foreach (SubCalendarEvent eachSubCalendarEvent in SubEvents)
                {
                    TimeOfDayPreferrence.DaySection DaySection = eachSubCalendarEvent.getDaySection().getCurrentDayPreference();
                    if (DaySection == TimeOfDayPreferrence.DaySection.None)
                    {
                        eachSubCalendarEvent.getDaySection().assignSectorBasedOnTIme(eachSubCalendarEvent.Start, AllTimeLine);
                        DaySection = eachSubCalendarEvent.getDaySection().getCurrentDayPreference();
                    }
                    if (DaySection != TimeOfDayPreferrence.DaySection.Disabled)
                    {
                        AllGroupings[DaySection].AddToStitchedEvents(eachSubCalendarEvent);
                        eachSubCalendarEvent.getDaySection().setCurrentdayPreference(DaySection);
                        Dictionary<Reason.Options, Reason> positionReasons = subEventToReason[eachSubCalendarEvent];
                        List<Reason> reasons = positionReasons.Where(keyValuePair => keyValuePair.Value.Option != Reason.Options.None).Select(keyValuePair => keyValuePair.Value).ToList();
                        foreach (Reason positionReason in reasons)
                        {
                            eachSubCalendarEvent.addReasons(positionReason);
                            positionReasons.Remove(positionReason.Option);
                        }
                    }
                }
                retValue = SubEvents.OrderBy(obj => obj.Start).ToList();
            }
            else
            {
                List<SubCalendarEvent> recursionSubEvents = SubEvents.ToList();
                List<SubCalendarEvent> NonRigidis = recursionSubEvents.Where(obj => (!obj.Rigid) && (!obj.isOptimized)).OrderBy(obj => obj.ActiveDuration).ToList();
                SubCalendarEvent UnwantedEvent = NonRigidis[0];
                //UnwantedEvent.getDaySection().rejectCurrentPreference();
                recursionSubEvents.Remove(UnwantedEvent);
                TimeOfDayPreferrence.DaySection DaySection = UnwantedEvent.getDaySection().getCurrentDayPreference();
                AllGroupings[DaySection].removeFromAcknwledged(UnwantedEvent);
                AllGroupings[DaySection].removeFromStitched(UnwantedEvent);
                UnwantedEvent.setAsUnOptimized();
                retValue = tryPinningInCurrentDayTimeline(AllTimeLine, recursionSubEvents, rigidevents);
            }
            return retValue;
        }

        void ResolveRejects()
        {

        }

        ILookup<TimeOfDayPreferrence.DaySection, SubCalendarEvent> groupEvents(IEnumerable<SubCalendarEvent> SubEvents, DayTimeLine dayInfo)
        {
            //SubEvents.AsParallel().ForAll(obj => obj.InitializeDayPreference(dayInfo));
            ILookup<TimeOfDayPreferrence.DaySection, SubCalendarEvent> RetValue = SubEvents.ToLookup(obj => obj.getDaySection().getCurrentDayPreference(), obj => obj);
            return RetValue;
        }


    }
}
