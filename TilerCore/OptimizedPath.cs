using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;

namespace TilerCore
{
    class OptimizedPath
    {
        DayTimeLine DayInfo;
        Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping> AllGroupings = new Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping>();
        Location DefaultLocation = new Location();
        Dictionary<SubCalendarEvent, Dictionary<Reason.Options, Reason>> subEventToReason = new Dictionary<SubCalendarEvent, Dictionary<Reason.Options, Reason>>();
        Dictionary<SubCalendarEvent, Dictionary<TimeOfDayPreferrence.DaySection, Dictionary<int, HashSet<int>>>> subEvent_Dict_To_DaySecion = new Dictionary<SubCalendarEvent, Dictionary<TimeOfDayPreferrence.DaySection, Dictionary<int, HashSet<int>>>>();
        /// <summary>
        /// This holds the subevents that cannot fit anywhere within this and also have no partial timefram that works
        /// </summary>
        HashSet<SubCalendarEvent> NotInvolvedIncalculation = new HashSet<SubCalendarEvent>();
        public OptimizedPath(DayTimeLine dayData, Location beginLocation = null, Location endLocation = null, Location home = null)
        {
            initializeSubEvents(dayData);

            TimeSpan TotalDuration = SubCalendarEvent.TotalActiveDuration(dayData.getSubEventsInTimeLine());
            DefaultLocation = Location.AverageGPSLocation(DayInfo.getSubEventsInTimeLine().Select(obj => obj.Location), false);
            if (home == null)
            {
                home = DefaultLocation.CreateCopy();
            }

            Dictionary<TimeOfDayPreferrence.DaySection, TimeLine> timeSections = TimeOfDayPreferrence.splitIntoDaySections(dayData);
            List<TimeOfDayPreferrence.SingleTimeOfDayPreference> singleTimeOfDayPreferences = timeSections.Select(kvp => new TimeOfDayPreferrence.SingleTimeOfDayPreference(kvp.Key, new TimelineWithSubcalendarEvents(kvp.Value.Start, kvp.Value.End, null))).OrderBy(obj => obj.Timeline.Start).ToList();
            //TimeOfDayPreferrence.SingleTimeOfDayPreference sleepPreference = singleTimeOfDayPreferences.Single(obj => obj.DaySection == TimeOfDayPreferrence.DaySection.Sleep);
            TimeOfDayPreferrence.SingleTimeOfDayPreference nonePreference = singleTimeOfDayPreferences.Single(obj => obj.DaySection == TimeOfDayPreferrence.DaySection.None);
            List<TimeOfDayPreferrence.SingleTimeOfDayPreference> noNone = singleTimeOfDayPreferences.Where(obj => obj != nonePreference).ToList();

            AllGroupings.Add(nonePreference.DaySection, new OptimizedGrouping(nonePreference, TotalDuration, home));

            if (noNone.Count > 0)
            {
                TimeOfDayPreferrence.SingleTimeOfDayPreference firstTimeOfDayPreference = noNone.First();
                OptimizedGrouping firstGrouping = new OptimizedGrouping(firstTimeOfDayPreference, TotalDuration, DefaultLocation.CreateCopy());
                firstGrouping.setLeftStitch(beginLocation);
                AllGroupings.Add(firstTimeOfDayPreference.DaySection, firstGrouping);
                noNone.Remove(firstTimeOfDayPreference);
            }

            if(noNone.Count > 0)
            {
                TimeOfDayPreferrence.SingleTimeOfDayPreference lastTimeOfDayPreference = noNone.Last();
                OptimizedGrouping lastGrouping = new OptimizedGrouping(lastTimeOfDayPreference, TotalDuration, DefaultLocation.CreateCopy());
                lastGrouping.setRightStitch(endLocation);
                AllGroupings.Add(lastTimeOfDayPreference.DaySection, lastGrouping);
                noNone.Remove(lastTimeOfDayPreference);
            }



            foreach (TimeOfDayPreferrence.SingleTimeOfDayPreference singleTimeOfDayPreference in noNone)
            {
                AllGroupings.Add(singleTimeOfDayPreference.DaySection, new OptimizedGrouping(singleTimeOfDayPreference, TotalDuration, DefaultLocation.CreateCopy()));
            }
            assignRigidsToTimeGroupings(DayInfo.getSubEventsInTimeLine(), DayInfo);
        }

        void initializeSubEvents(DayTimeLine DayData)
        {
            List<SubCalendarEvent> subEventsForCalculation = DayData.getSubEventsInTimeLine();

            List<SubCalendarEvent> subEventsThatCannotExist = DayData.getSubEventsInTimeLine().Where(subEvent => !subEvent.canExistWithinTimeLine(DayData)).ToList();
            if (subEventsThatCannotExist.Count > 0)
            {
                Dictionary<SubCalendarEvent, TimeLine> subEventToViableTimeLine = subEventsThatCannotExist.ToDictionary(SubEvent => SubEvent, SubEvent => DayData.InterferringTimeLine(SubEvent.RangeTimeLine));
                HashSet<SubCalendarEvent> allSubEvents = new HashSet<SubCalendarEvent>(DayData.getSubEventsInTimeLine());
                HashSet<SubCalendarEvent> tempSubEvents = new HashSet<SubCalendarEvent>();
                foreach (SubCalendarEvent subEvent in subEventToViableTimeLine.Keys)
                {
                    allSubEvents.Remove(subEvent);
                    TimeLine interferringTimeline = subEventToViableTimeLine[subEvent];
                    if (interferringTimeline != null)
                    {
                        SubCalendarEvent slicedValidSubEvent = new SubCalendarEvent(subEvent.ParentCalendarEvent, TilerUser.autoUser, new TilerUserGroup(), subEvent.getTimeZone, subEvent.Id, subEvent.getName, interferringTimeline.Start, interferringTimeline.End, new BusyTimeLine(subEvent.Id, interferringTimeline.Start, interferringTimeline.End), subEvent.isRigid, subEvent.isEnabled, subEvent.getUIParam, subEvent.Notes, subEvent.getIsComplete, subEvent.Location, subEvent.getCalendarEventRange, subEvent.Conflicts);
                        tempSubEvents.Add(slicedValidSubEvent);
                    }
                    else
                    {
                        NotInvolvedIncalculation.Add(subEvent);
                    }
                }
                subEventsForCalculation = allSubEvents.Concat(tempSubEvents).ToList();
            }
            

            DayInfo = new DayTimeLine(DayData.Start, DayData.End, DayData.UniversalIndex, DayData.BoundedIndex);
            DayInfo.AddToSubEventList(subEventsForCalculation);
        }


        void assignRigidsToTimeGroupings(IEnumerable<SubCalendarEvent> subEvents, DayTimeLine DayData)
        {
            IEnumerable<SubCalendarEvent> rigidSubEvents = subEvents.Where(subEvent => subEvent.isLocked);
            HashSet<OptimizedGrouping> retrievedGroupings = new HashSet<OptimizedGrouping>();
            foreach(SubCalendarEvent subEvent in rigidSubEvents)
            {
                subEvent.InitializeDayPreference(DayData);
                TimeOfDayPreferrence daySection = subEvent.getDaySection();
                OptimizedGrouping grouping = AllGroupings[daySection.getCurrentDayPreference()];
                grouping.AddToStitchedEvents(subEvent);
                retrievedGroupings.Add(grouping);
            }
            foreach (OptimizedGrouping grouping in retrievedGroupings)
            {
                grouping.movePathStitchedToAcknowledged();
            }
        }

        public void OptimizePath()
        {
            foreach (SubCalendarEvent subEvent in DayInfo.getSubEventsInTimeLine())
            {
                subEvent.InitializeDayPreference(DayInfo);
                subEvent_Dict_To_DaySecion.Add(subEvent, new Dictionary<TimeOfDayPreferrence.DaySection, Dictionary<int, HashSet<int>>>());
            }

            while (true)
            {
                subEventToReason = DayInfo.getSubEventsInTimeLine().ToDictionary(subEvent => subEvent, subEVent => new Dictionary<Reason.Options, Reason>());
                List<SubCalendarEvent> AllSubCalendarEvents = DayInfo.getSubEventsInTimeLine();
                List<SubCalendarEvent> CurrentlyValid = AllSubCalendarEvents
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
                    OptimizedGrouping.buildStitchers(AllGroupings.Select(obj => obj.Value));
                    StitchAllGroupings();
                }
                else
                {
                    List<SubCalendarEvent> BestOrder = AllSubCalendarEvents.OrderBy(obj => obj.Start).ToList();
                    List<Location> BestOrderLocations = BestOrder.Select(obj => obj.Location).ToList();
                    List<SubCalendarEvent> NoPosition = AllSubCalendarEvents.Where(obj => (!obj.isLocked)).Where(obj => (!obj.isOptimized)).Where(obj =>
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
            List<SubCalendarEvent> disabledSubEvents = DayInfo.getSubEventsInTimeLine().Where(subEvent => subEvent.getDaySection().getCurrentDayPreference() == TimeOfDayPreferrence.DaySection.Disabled).ToList();
            if (disabledSubEvents.Count > 0)
            {
                List<SubCalendarEvent> correctlyAssignedevents = DayInfo.getSubEventsInTimeLine().Except(disabledSubEvents).OrderBy(obj=>obj.Start).ToList();

                Tuple<Dictionary<TilerEvent, double>, Tuple<Location, DateTimeOffset, TimeSpan>> evaluatedParams = evalulateParameter(disabledSubEvents, null);
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

        public List<SubCalendarEvent> getSubevents()
        {
            List<SubCalendarEvent> retValue = AllGroupings.SelectMany(group => group.Value.getPinnedEvents()).ToList();
            return retValue;
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
                Dictionary<SubCalendarEvent, mTuple<TimeLine, TimeLine>> subEventToAvailableSpaces = Utility.subEventToMaxSpaceAvailable(timeLine, correctlyAssignedevents);
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
                    if (orderedIndexes.Count > 0)
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
                            subEventsReadjusted = ConvertInterFerringEventsToBlobAndsubEvent(subEventsReadjusted);//
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
                                disabledSubEvent.shiftEvent(revisedTimeLine.End - disabledSubEvent.getActiveDuration);
                                subEventsReadjusted = ConvertInterFerringEventsToBlobAndsubEvent(subEventsReadjusted);
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
                            SubCalendarEvent before = subEventsReadjusted[beforeIndex];
                            SubCalendarEvent after = subEventsReadjusted[afterIndex];
                            TimeLine relevantTimeline = new TimeLine(before.Start, after.End);
                            TimeLine revisedOverlappingTImeline = disabledSubEvent.getTimeLineInterferringWithCalEvent(relevantTimeline).FirstOrDefault();
                            if (revisedOverlappingTImeline != null)
                            {
                                TimeLine centralizedTimeLine = Utility.CentralizeYourSelfWithinRange(revisedOverlappingTImeline, disabledSubEvent.getActiveDuration);
                                disabledSubEvent.shiftEvent(centralizedTimeLine.Start);
                                subEventsReadjusted = ConvertInterFerringEventsToBlobAndsubEvent(subEventsReadjusted);
                            }
                        }
                    }
                }
                return subEventsReadjusted;
            }

            throw new Exception("disabledSubEvent cannot exist within timeLine, consider providing a a time line where disabledSubEvent can exist");
            
        }

        /// <summary>
        /// Function takes a list of possibly conflicting subevent 'possiblyConflictingSubEvent' and then returns a non conflicting subevent. If there are conflicts in "possiblyConflictingSubEvent" the returned value will one or more blobsubcalendarevents
        /// </summary>
        /// <param name="possiblyConflictingSubEvent"></param>
        /// <returns></returns>
        List<SubCalendarEvent> ConvertInterFerringEventsToBlobAndsubEvent(List<SubCalendarEvent> possiblyConflictingSubEvent)
        {
            HashSet<SubCalendarEvent> allSubEvents = new HashSet<SubCalendarEvent>(possiblyConflictingSubEvent);
            List<BlobSubCalendarEvent> conflictingSubevent = Utility.getConflictingEvents(allSubEvents);
            foreach (SubCalendarEvent subEvent in conflictingSubevent.SelectMany(blob => blob.getSubCalendarEventsInBlob()))
            {
                allSubEvents.Remove(subEvent);
            }

            List<SubCalendarEvent> realignedSubEvents = allSubEvents.Concat(conflictingSubevent).ToList();
            possiblyConflictingSubEvent = realignedSubEvents.OrderBy(obj => obj.Start).ToList();
            return possiblyConflictingSubEvent;
        }

        Tuple<Dictionary<TilerEvent, double>, Tuple<Location, DateTimeOffset, TimeSpan>> evalulateParameter(IEnumerable<SubCalendarEvent> events, OptimizedGrouping.OptimizedAverage optimizedAverage)
        {
            
            if(events.Count() == 0) // handles scenario where there is not event
            {
                throw new Exception("events is empty");
            }
            Dictionary<TilerEvent, List<double>> dimensionsPerEvent = new Dictionary<TilerEvent, List<double>>();
            Dictionary<string, uint> fibboIndexes = new Dictionary<string, uint>();
            Location avgLocation;
            if (events.Where(eve => eve.isLocked).Count() > 0)// if there are rigids, let the rigid be the average location
            {
                avgLocation = Location.AverageGPSLocation(events.Where(eve => eve.isLocked).Select(obj => obj.Location));
            }
            else
            {
                avgLocation = Location.AverageGPSLocation(events.Select(obj => obj.Location));
            }

            if (optimizedAverage != null)
            {
                avgLocation = Location.AverageGPSLocation(new List<Location> (optimizedAverage?.SubEvents.Select(subevent => subevent.Location)));
            }


            foreach (TilerEvent Event in events)
            {
                double distance = Location.calculateDistance(Event.Location, avgLocation, 100);
                List<double> parameters = new List<double>();

                uint multiplier = 1;
                uint fiboindex = 1;
                string calendarID = Event.getTilerID.getCalendarEventComponent();

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
            double sum = (double)events.Sum(obj => (obj.getDeadline - Utility.StartOfTime).TotalSeconds);
            double averageRatio = sum / events.Count();
            long AverageTicks = TimeSpan.FromSeconds(averageRatio).Ticks;
            DateTimeOffset latestDeadline = new DateTimeOffset(AverageTicks, new TimeSpan());
            ///deals with scenarios where the deadline is later or earlier. the earlier the higher up in hierachy.
            foreach (TilerEvent Event in events)
            {
                double deadlineRatio = (double)Event.getDeadline.Ticks / AverageTicks;
                List<double> parameters = dimensionsPerEvent[Event];

                uint multiplier = 1;
                uint fiboindex = 1;
                string calendarID = Event.getTilerID.getCalendarEventComponent();

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
            long AverageDurationTicks = (long)events.Average(obj => obj.getActiveDuration.Ticks);
            ///deals with scenarios with duration. The bigger the duration the higher up it is. Hence the more the ticks the ratio is in the for loop

            List<double> origin = new List<double>() { 0, 0 };//, 0};


            Dictionary<TilerEvent, double> retValueDict = new Dictionary<TilerEvent, double>();
            List<TilerEvent> subeEVents = dimensionsPerEvent.OrderBy(obj => obj.Key.getTilerID.getCalendarEventComponent()).Select(obj => obj.Key).ToList();
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

            Tuple<Location, DateTimeOffset, TimeSpan> retValueTuple = new Tuple<Location, DateTimeOffset, TimeSpan>(avgLocation, latestDeadline, new TimeSpan(AverageDurationTicks));
            Tuple<Dictionary<TilerEvent, double>, Tuple<Location, DateTimeOffset, TimeSpan>> retValue = new Tuple<Dictionary<TilerEvent, double>, Tuple<Location, DateTimeOffset, TimeSpan>>(retValueDict, retValueTuple);
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
                Dictionary<SubCalendarEvent, mTuple<TimeLine, TimeLine>> subEventToAvailableSpaces = Utility.subEventToMaxSpaceAvailable(timeLine, Stitched_Revised);
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
            if (AllEvents.Count > 0)
            {
                Tuple<Dictionary<TilerEvent, double>, Tuple<Location, DateTimeOffset, TimeSpan>> evaluatedParams = evalulateParameter(AllEvents, Grouping.GroupAverage);
                Dictionary<TilerEvent, double> evaluatedEvents = evaluatedParams.Item1;
                List<KeyValuePair<TilerEvent, double>> subEventsEvaluated = evaluatedEvents.OrderBy(obj => obj.Value).ToList();

                IEnumerable<SubCalendarEvent> subEvents = (IEnumerable<SubCalendarEvent>)evaluatedEvents.OrderBy(obj => obj.Value).Select(obj => (SubCalendarEvent)obj.Key);
                List<String> locations = subEvents.Select(obj => "" + obj.Location.Latitude + "," + obj.Location.Longitude).ToList();
                Subevents = subEvents.Take(5).ToList();
                //Subevents= Utility.getBestPermutation(Subevents.ToList(), double.MaxValue, new Tuple<Location_Elements, Location_Elements>(Grouping.LeftBorder, Grouping.RightBorder)).ToList();
                Tuple<Location, Location> borderElements = new Tuple<Location, Location>(Grouping.LeftBorder, Grouping.RightBorder);
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

                LocationReason locationReason = new LocationReason(Stitched_Revised.Select(subEvent => subEvent.Location));
                Stitched_Revised.ForEach(subEvent =>
                {
                    updateSubeventReason(subEvent, locationReason);
                    if (subEvent.getActiveDuration > evaluatedParams.Item2.Item3)
                    {
                        updateSubeventReason(subEvent, new DurationReason(subEvent.getActiveDuration));
                    }
                    if (subEvent.getIsEventRestricted)
                    {
                        updateSubeventReason(subEvent, new RestrictedEventReason((subEvent as SubCalendarEventRestricted).getRestrictionProfile()));
                    }
                });


                Grouping.setPathStitchedEvents(Stitched_Revised);
            }

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
            List<SubCalendarEvent> rigidSubeevents = DayInfo.getSubEventsInTimeLine().Where(obj => obj.isLocked).ToList();
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
                List<SubCalendarEvent> NonRigidis = recursionSubEvents.Where(obj => (!obj.isLocked) && (!obj.isOptimized)).OrderBy(obj => obj.getActiveDuration).ToList();
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

        ILookup<TimeOfDayPreferrence.DaySection, SubCalendarEvent> groupEvents(IEnumerable<SubCalendarEvent> SubEvents, DayTimeLine dayInfo)
        {
            foreach (SubCalendarEvent subevent in SubEvents)
            {
                subevent.updateDayPreference(AllGroupings.Select(group => group.Value).ToList());
            }

            ILookup<TimeOfDayPreferrence.DaySection, SubCalendarEvent> RetValue = SubEvents.ToLookup(obj => obj.getDaySection().getCurrentDayPreference(), obj => obj);
            return RetValue;
        }
    }
}
