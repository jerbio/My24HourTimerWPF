using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;
using static TilerElements.TimeOfDayPreferrence;

namespace TilerCore
{
    class OptimizedPath
    {
        DayTimeLine DayInfo;
        Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping> AllGroupings = new Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping>();
        Location DefaultLocation = new Location();
        Location _BeginLocation = new Location();
        Location _EndLocation = new Location();
        Dictionary<SubCalendarEvent, Dictionary<Reason.Options, Reason>> subEventToReason = new Dictionary<SubCalendarEvent, Dictionary<Reason.Options, Reason>>();
        Dictionary<SubCalendarEvent, Dictionary<TimeOfDayPreferrence.DaySection, HashSet<string>>> subEvent_Dict_To_DaySecion = new Dictionary<SubCalendarEvent, Dictionary<TimeOfDayPreferrence.DaySection, HashSet<string>>>();
        protected List<SubCalendarEvent> UnassignedSubevents = new List<SubCalendarEvent>();// These are disabled events, events that could not find a slot
        protected List<SubCalendarEvent> ReassignedDisabledSubevents = new List<SubCalendarEvent>();
        /// <summary>
        /// This holds the subevents that cannot fit anywhere within this and also have no partial timefram that works
        /// </summary>
        HashSet<SubCalendarEvent> NotInvolvedIncalculation = new HashSet<SubCalendarEvent>();
        public OptimizedPath(DayTimeLine dayData, Location beginLocation = null, Location endLocation = null, Location home = null)
        {
            initializeSubEvents(dayData);
            _BeginLocation = beginLocation;
            _EndLocation = endLocation;

            TimeSpan TotalDuration = SubCalendarEvent.TotalActiveDuration(dayData.getSubEventsInTimeLine());
            DefaultLocation = Location.AverageGPSLocation(DayInfo.getSubEventsInTimeLine().Select(obj => obj.Location), false);
            if (home == null)
            {
                home = DefaultLocation.CreateCopy();
            }

            Dictionary<TimeOfDayPreferrence.DaySection, TimeLine> timeSections = TimeOfDayPreferrence.splitIntoDaySections(dayData);
            List<TimeOfDayPreferrence.SingleTimeOfDayPreference> singleTimeOfDayPreferences = timeSections.Select(kvp => new TimeOfDayPreferrence.SingleTimeOfDayPreference(kvp.Key, new TimelineWithSubcalendarEvents(kvp.Value.Start, kvp.Value.End, null))).OrderBy(obj => obj.Timeline.Start).ToList();
            //TimeOfDayPreferrence.SingleTimeOfDayPreference sleepPreference = singleTimeOfDayPreferences.Single(obj => obj.DaySection == TimeOfDayPreferrence.DaySection.Sleep);
            TimeOfDayPreferrence.SingleTimeOfDayPreference nonePreference = new TimeOfDayPreferrence.SingleTimeOfDayPreference(TimeOfDayPreferrence.DaySection.None, (DayTimeLine)dayData.CreateCopy());
            List<TimeOfDayPreferrence.SingleTimeOfDayPreference> noNone = singleTimeOfDayPreferences.ToList();

            var grouping = new OptimizedGrouping(nonePreference, TotalDuration, home, home);
            AllGroupings.Add(nonePreference.DaySection, grouping);

            if (noNone.Count > 0)
            {
                TimeOfDayPreferrence.SingleTimeOfDayPreference firstTimeOfDayPreference = noNone.First();
                OptimizedGrouping firstGrouping = new OptimizedGrouping(firstTimeOfDayPreference, TotalDuration, DefaultLocation.CreateCopy(), home);
                firstGrouping.setLeftStitch(beginLocation);
                AllGroupings.Add(firstTimeOfDayPreference.DaySection, firstGrouping);

                TimeOfDayPreferrence.SingleTimeOfDayPreference lastTimeOfDayPreference = noNone.Last();
                OptimizedGrouping lastGrouping = new OptimizedGrouping(lastTimeOfDayPreference, TotalDuration, DefaultLocation.CreateCopy(), home);
                lastGrouping.setRightStitch(endLocation);
                if (!AllGroupings.ContainsKey(lastTimeOfDayPreference.DaySection))
                {
                    AllGroupings.Add(lastTimeOfDayPreference.DaySection, lastGrouping);
                }
            }



            foreach (TimeOfDayPreferrence.SingleTimeOfDayPreference singleTimeOfDayPreference in noNone)
            {
                if (!AllGroupings.ContainsKey(singleTimeOfDayPreference.DaySection))
                {
                    var eachGrouping = new OptimizedGrouping(singleTimeOfDayPreference, TotalDuration, DefaultLocation.CreateCopy(), home);
                    AllGroupings.Add(singleTimeOfDayPreference.DaySection, eachGrouping);
                }
            }
            assignRigidsToTimeGroupings(DayInfo.getSubEventsInTimeLine(), DayInfo);
        }

        void initializeSubEvents(DayTimeLine DayData)
        {
            List<SubCalendarEvent> subEventsForCalculation = new List<SubCalendarEvent>();
            foreach (var subEvent in DayData.getSubEventsInTimeLine())
            {
                subEvent.setAsUnOptimized();
                subEventsForCalculation.Add(subEvent);
            }

            List<SubCalendarEvent> subEventsThatCannotExist = DayData.getSubEventsInTimeLine().Where(subEvent => !subEvent.canExistWithinTimeLine(DayData)).ToList();
            if (subEventsThatCannotExist.Count > 0)
            {
                Dictionary<SubCalendarEvent, TimeLine> subEventToViableTimeLine = subEventsThatCannotExist.ToDictionary(SubEvent => SubEvent, SubEvent => DayData.InterferringTimeLine(SubEvent.StartToEnd));
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
            foreach (SubCalendarEvent subEvent in rigidSubEvents)
            {
                subEvent.InitializeDayPreference(DayData);
                TimeOfDayPreferrence daySection = subEvent.getDayPreference();
                //OptimizedGrouping grouping = null;/* AllGroupings[TimeOfDayPreferrence.DaySection.None];//defaults to none day section unless the a preference is found in the loop*/
                foreach (OptimizedGrouping grouping in ActiveDaySectionGrouping)
                {
                    if (grouping.TimeLine.doesTimeLineInterfere(subEvent.ActiveSlot)) {
                        grouping.AddToStitchedEvents(subEvent);
                        retrievedGroupings.Add(grouping);
                    }
                }
            }
            foreach (OptimizedGrouping grouping in retrievedGroupings)
            {
                grouping.movePathStitchedToAcknowledged();
            }
        }

        public void OptimizePath()
        {
            int troubleshootingCOunter = 0;
            bool alreadySetInitialLeftStitch = false;// this flag checks if the default location for the intial begin location is set. This is needed so when the first call to OptimizeGrouping is called the first selected grouping ses the appropriate left border location
            OptimizedGrouping initialLeftBorderGroupingStitch = null;
            foreach (SubCalendarEvent subEvent in DayInfo.getSubEventsInTimeLine())
            {
                subEvent.InitializeDayPreference(DayInfo);
                subEvent_Dict_To_DaySecion.Add(subEvent, new Dictionary<TimeOfDayPreferrence.DaySection, HashSet<string>>());
            }

            while (true)
            {
                ++troubleshootingCOunter;
                subEventToReason = DayInfo.getSubEventsInTimeLine().ToDictionary(subEvent => subEvent, subEVent => new Dictionary<Reason.Options, Reason>());
                List<SubCalendarEvent> AllSubCalendarEvents = DayInfo.getSubEventsInTimeLine();
                List<SubCalendarEvent> CurrentlyValid = AllSubCalendarEvents
                    .Where(obj => (!obj.isOptimized)).Where(obj =>
                    {
                        var TimeOfDay = obj.getDayPreference().getCurrentDayPreference();
                        return ((TimeOfDay != TimeOfDayPreferrence.DaySection.Disabled));
                    }).ToList();
                if (CurrentlyValid.Count > 0)
                {
                    ILookup<TimeOfDayPreferrence.DaySection, SubCalendarEvent> SubEventRegrouping = groupEvents(CurrentlyValid.OrderBy(o => o.Start), DayInfo);// this is ordered by start because you need to make sure the subevents are in some known order. So the code can be more deterministic, the last thing you need is the Id of the subcalendarevent controlling the order of the Ilookup "value"
                    List<OptimizedGrouping> DaySectorGrouping = new List<OptimizedGrouping>();
                    foreach (IGrouping<TimeOfDayPreferrence.DaySection, SubCalendarEvent> eachIGrouping in SubEventRegrouping.OrderBy(o => o.Key))
                    {
                        if (eachIGrouping.Key != TimeOfDayPreferrence.DaySection.Disabled)
                        {
                            var grouping = AllGroupings[eachIGrouping.Key];
                            if (!alreadySetInitialLeftStitch)
                            {
                                if (BeginLocation != null && BeginLocation.isNotNullAndNotDefault && BeginLocation.IsVerified)// this ensures that initially each grouping assumes the timeline is using the approriate leftborder for the location
                                {
                                    grouping.setLeftStitch(BeginLocation);
                                }
                                alreadySetInitialLeftStitch = true;
                                initialLeftBorderGroupingStitch = grouping;
                            }
                            grouping.clearPathStitchedEvents();
                            OptimizeGrouping(AllGroupings[eachIGrouping.Key], SubEventRegrouping[eachIGrouping.Key], DayInfo);
                            if(alreadySetInitialLeftStitch 
                                && initialLeftBorderGroupingStitch == grouping 
                                && AllGroupings[eachIGrouping.Key].getPathStitchedSubevents().Count < 1 
                                && AllGroupings[eachIGrouping.Key].getPinnedEvents().Count < 1)// checks if there was no actual initial stitching then revese the stitching
                            {
                                alreadySetInitialLeftStitch = false;
                            }
                        }
                    }
                    OptimizedGrouping.buildStitchers(AllGroupings);
                    StitchAllGroupings();
                }
                else
                {
                    List<SubCalendarEvent> BestOrder = AllSubCalendarEvents.OrderBy(obj => obj.Start).ToList();
                    List<Location> BestOrderLocations = BestOrder.Select(obj => obj.Location).ToList();
                    List<SubCalendarEvent> NoPosition = AllSubCalendarEvents.Where(obj => (!obj.isLocked)).Where(obj => (!obj.isOptimized)).Where(obj =>
                    {
                        var TimeOfDay = obj.getDayPreference().getCurrentDayPreference();
                        return ((TimeOfDay != TimeOfDayPreferrence.DaySection.Disabled));
                    }).ToList();
                    optimizeDisabledEvents();
                    break;
                }
            }
        }

        /// <summary>
        /// Function tries to find the best spot for a disabled subevent. If it can't a conflict is created. The disabled sub event is undesignated from its daytimeline
        /// </summary>
        void optimizeDisabledEvents()
        {
            List<SubCalendarEvent> disabledSubEvents = DayInfo.getSubEventsInTimeLine().Where(subEvent => subEvent.getDayPreference().getCurrentDayPreference() == TimeOfDayPreferrence.DaySection.Disabled).ToList();
            if (disabledSubEvents.Count > 0)
            {
                List<SubCalendarEvent> correctlyAssignedevents = DayInfo.getSubEventsInTimeLine().Except(disabledSubEvents).OrderBy(obj => obj.Start).ToList();

                Tuple<Dictionary<TilerEvent, double>, Tuple<Location, DateTimeOffset, TimeSpan>> evaluatedParams = evalulateSubEventsWithRespectToGroup(disabledSubEvents, null);
                Dictionary<TilerEvent, double> evaluatedEvents = evaluatedParams.Item1;
                List<KeyValuePair<TilerEvent, double>> subEventsEvaluated = evaluatedEvents.OrderBy(obj => obj.Value).ToList();

                foreach (SubCalendarEvent disabledSubEvent in subEventsEvaluated.Select(obj => obj.Key))
                {
                    List<SubCalendarEvent> reOptimizedSubevents = optimizeDisabledEvent(DayInfo, correctlyAssignedevents, disabledSubEvent);
                    if (reOptimizedSubevents.Count > correctlyAssignedevents.Count && Utility.tryPinSubEventsToStart(reOptimizedSubevents, DayInfo))
                    {
                        correctlyAssignedevents = reOptimizedSubevents.ToList();
                        ReassignedDisabledSubevents.Add(disabledSubEvent);
                    }
                    else
                    {
                        disabledSubEvent.setAsUnOptimized();
                        disabledSubEvent.ParentCalendarEvent.undesignateSubEvent(disabledSubEvent);
                        DayInfo.RemoveSubEvent(disabledSubEvent.Id);
                        UnassignedSubevents.Add(disabledSubEvent);
                    }
                }
            }
        }

        /// <summary>
        /// Returns all the pinned subevents and the reassigned disabled sub events
        /// </summary>
        /// <returns></returns>
        public List<SubCalendarEvent> getOptimizedSubevents()
        {
            List<SubCalendarEvent> retValue = new List<SubCalendarEvent>();
            HashSet<SubCalendarEvent> subEventSet = new HashSet<SubCalendarEvent>();
            foreach (SubCalendarEvent subEvent in AllGroupings.SelectMany(group => group.Value.getPinnedEvents()).ToList())
            {
                if (!subEventSet.Contains(subEvent))
                {
                    subEventSet.Add(subEvent);
                    retValue.Add(subEvent);
                }
            }

            retValue.AddRange(ReassignedDisabledSubevents);
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
                List<SubCalendarEvent> fittable = new List<SubCalendarEvent>();
                HashSet<int> validIndexes = new HashSet<int>();
                if (correctlyAssignedeventsToIndex.Count > 0)
                {
                    Dictionary<SubCalendarEvent, mTuple<TimeLine, TimeLine>> subEventToAvailableSpaces = Utility.subEventToMaxSpaceAvailable(timeLine, correctlyAssignedevents);
                    i = 0;


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
                }
                else
                {
                    if (disabledSubEvent.canExistWithinTimeLine(timeLine))
                    {
                        fittable.Add(disabledSubEvent);
                        validIndexes.Add(0);
                        correctlyAssignedevents.Add(disabledSubEvent);
                    }

                    return correctlyAssignedevents;
                }


                List<SubCalendarEvent> subEventsReadjusted = correctlyAssignedevents.ToList();



                List<int> orderedIndexes = validIndexes.OrderBy(validIndex => validIndex).ToList();



                List<int> invalidIndexes = new List<int>();
                for (i = 0; i < correctlyAssignedevents.Count + 1; i++)// for loop builds the invalid indexes, if an index is not in the orderedIndexes it is added to the invalidIndexes. Remmber orderedIndexes is sorted
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
                if (index != -1)
                {
                    subEventsReadjusted.Insert(index, disabledSubEvent);
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
        /// <summary>
        /// Function evaluates the subcalendar events relative to the optimized group. It returns a Tuple. 
        /// Item1 of the tuple is ta dictionary of the subEVent to its score relative to the optimized group.
        /// Item2 is a 3item Tuple.
        /// Item2.Item1 is the average location of the optimize group. This location is based on The average of all the rigids or the average of all the events
        /// Item2.Item2 is the latest deadline of the subevents
        /// Item2.Item3 is the average span of the provided events
        /// </summary>
        /// <param name="events"></param>
        /// <param name="optimizedAverage"></param>
        /// <returns></returns>
        Tuple<Dictionary<TilerEvent, double>, Tuple<Location, DateTimeOffset, TimeSpan>> evalulateSubEventsWithRespectToGroup(IEnumerable<SubCalendarEvent> events, OptimizedGrouping.OptimizedAverage optimizedAverage)
        {

            if (events.Count() == 0) // handles scenario where there is not event
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
                avgLocation = Location.AverageGPSLocation(new List<Location>(optimizedAverage?.SubEvents.Select(subevent => subevent.Location)));
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
            List<SubCalendarEvent> memoryBoundSubsetSubevents = new List<SubCalendarEvent>();
            List<SubCalendarEvent> alreadyStitched = Grouping.getPinnedEvents().OrderBy(obj => obj.Start).ThenBy(o => o.getActiveDuration).ToList();
            List<SubCalendarEvent> Stitched_Revised = alreadyStitched.ToList();
            List<SubCalendarEvent> Stitched_Revised_Chopped_ToFitTimeLine = new List<SubCalendarEvent>();
            TimeSpan totalEventSpan = new TimeSpan();
            int initializingCount = Stitched_Revised.Count;
            int i = 0;
            int j = 0;

            List<SubCalendarEvent> fittable = new List<SubCalendarEvent>();
            if (Stitched_Revised.Count > 0)//if there are current events that are currently known to be stitched into the current day section
            {
                TimeLine groupTimeLine = Grouping.TimeLine;
                groupTimeLine = Grouping.TimeLine;
                for (i = 0; i < Stitched_Revised.Count; i++)
                {
                    SubCalendarEvent subEvent = Stitched_Revised[i];
                    if (subEvent.isLocked)
                    {
                        TimeLine choppedTimeLine = subEvent.ActiveSlot.InterferringTimeLine(groupTimeLine);
                        EventID choppedCalId = EventID.GenerateCalendarEvent();
                        CalendarEvent choppedCalEvent = CalendarEvent.getEmptyCalendarEvent(choppedCalId, choppedTimeLine.Start, choppedTimeLine.End);
                        subEvent = choppedCalEvent.AllSubEvents.First();
                        subEvent.updateTimeLine(choppedTimeLine);
                        subEvent.Location = subEvent.Location;
                    }
                    Stitched_Revised_Chopped_ToFitTimeLine.Add(subEvent);
                }
                TimeLine pintimeLine = timeLine;
                if (Utility.tryPinSubEventsToStart(Stitched_Revised_Chopped_ToFitTimeLine, pintimeLine))
                {
                    totalEventSpan = TimeSpan.FromSeconds(Stitched_Revised_Chopped_ToFitTimeLine.Sum(subEvent => subEvent.ActiveSlot.InterferringTimeLine(pintimeLine).TimelineSpan.TotalSeconds));
                    Dictionary<SubCalendarEvent, mTuple<TimeLine, TimeLine>> subEventToAvailableSpaces = Utility.subEventToMaxSpaceAvailable(pintimeLine, Stitched_Revised_Chopped_ToFitTimeLine);
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
                            subEvent.getDayPreference().rejectCurrentPreference(Grouping.DaySector);
                        }
                    }
                }
                else
                {
                    for (i = 0; i < AllEvents.Count; i++)
                    {
                        SubCalendarEvent subEvent = AllEvents[i];//unacknowledged subevent for this grouping
                        subEvent.getDayPreference().rejectCurrentPreference(Grouping.DaySector);
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
                Tuple<Dictionary<TilerEvent, double>, Tuple<Location, DateTimeOffset, TimeSpan>> evaluatedParams = evalulateSubEventsWithRespectToGroup(AllEvents, Grouping.GroupAverage);
                Dictionary<TilerEvent, double> evaluatedEvents = evaluatedParams.Item1;
                List<KeyValuePair<TilerEvent, double>> subEventsEvaluated = evaluatedEvents.OrderBy(obj => obj.Value).ToList();

                IEnumerable<SubCalendarEvent> evaluatedSubEvents = (IEnumerable<SubCalendarEvent>)evaluatedEvents.OrderBy(obj => obj.Value).Select(obj => (SubCalendarEvent)obj.Key);
                List<String> locations = evaluatedSubEvents.Select(obj => "" + obj.Location.Latitude + "," + obj.Location.Longitude).ToList();
                memoryBoundSubsetSubevents = evaluatedSubEvents.Take(5).ToList();
                //Subevents= Utility.getBestPermutation(Subevents.ToList(), double.MaxValue, new Tuple<Location_Elements, Location_Elements>(Grouping.LeftBorder, Grouping.RightBorder)).ToList();
                Tuple<Location, Location> borderElements = new Tuple<Location, Location>(Grouping.LeftBorder, Grouping.RightBorder);
                memoryBoundSubsetSubevents = Utility.getBestPermutation(memoryBoundSubsetSubevents.ToList(), borderElements, 0).ToList();
                Dictionary<SubCalendarEvent, int> subEventToIndex = new Dictionary<SubCalendarEvent, int>();
                Func<SubCalendarEvent, int, bool> addToStitched_Revised = (subEvent, index) =>
                {
                    bool retValue = false;
                    if (totalEventSpan < Grouping.TimeLine.TimelineSpan)
                    {
                        if (index < 0)
                        {
                            Stitched_Revised.Add(subEvent);
                        } else
                        {
                            Stitched_Revised.Insert(index, subEvent);
                        }
                        totalEventSpan = totalEventSpan.Add(subEvent.getActiveDuration);
                        subEventToIndex.Add(subEvent, index);
                        retValue = true;
                    }
                    return retValue;
                };

                string hash = getEventHash(alreadyStitched);
                List<SubCalendarEvent> reversedMemoryBoundSubsetSubevents = memoryBoundSubsetSubevents.ToList();
                reversedMemoryBoundSubsetSubevents.Reverse();//the reverse is crucial because of how best position works. Best position at some point uses minIndex which doesnt use "<= less than or equal to" which is crucial to evaluation. This is because an optimized path of A->B->C, has the same path score for C->B->A. If you try to verify the path one at a time so A then A->B because of min-dex implementation B-> A will be returned as the better position because it is the first encountered calculation and min-dex uses < and not <=
                for (i = 0, j = 0; i < reversedMemoryBoundSubsetSubevents.Count; i++, j++)
                {
                    SubCalendarEvent subEvent = reversedMemoryBoundSubsetSubevents[i];
                    if (subEvent_Dict_To_DaySecion[subEvent].ContainsKey(Grouping.DaySector))
                    {
                        if (!subEvent_Dict_To_DaySecion[subEvent][Grouping.DaySector].Contains(hash))
                        {
                            int BestPostion = getBestPosition(timeLine, subEvent, Stitched_Revised, BorderElements: borderElements);
                            if (BestPostion != -1)
                            {
                                addToStitched_Revised(subEvent, BestPostion);
                            }
                            else
                            {
                                subEvent.getDayPreference().rejectCurrentPreference();
                            }
                            subEvent_Dict_To_DaySecion[subEvent][Grouping.DaySector].Add(hash);
                        }
                        else
                        {
                            subEvent.getDayPreference().rejectCurrentPreference();
                        }
                    }
                    else
                    {
                        int BestPostion = getBestPosition(timeLine, subEvent, Stitched_Revised, BorderElements: borderElements);
                        subEvent_Dict_To_DaySecion[subEvent].Add(Grouping.DaySector, new HashSet<string>());
                        if (BestPostion != -1)
                        {
                            addToStitched_Revised(subEvent, BestPostion);
                            subEvent_Dict_To_DaySecion[subEvent][Grouping.DaySector].Add(hash);
                        }
                        else
                        {
                            subEvent.getDayPreference().rejectCurrentPreference();
                        }
                    }
                }
                Grouping.setPathStitchedEvents(Stitched_Revised);
            }

        }

        string getEventHash(IEnumerable<SubCalendarEvent> orderedSubEvents)
        {
            StringBuilder stringBuilder = new StringBuilder();
            String retValue = "";
            foreach (String s in orderedSubEvents.Select(subEvent => subEvent.getTilerID.getIDUpToRepeatCalendarEvent()))
            {
                stringBuilder.Append(s).Append("||");
            }
            retValue = stringBuilder.ToString();
            return retValue;
        }

        int getBestPosition(TimeLine timeLine, SubCalendarEvent subEvent, IEnumerable<SubCalendarEvent> CurrentList, HashSet<int> unusableIndexes = null, Tuple<Location, Location> BorderElements = null)
        {
            Dictionary<SubCalendarEvent, mTuple<SubCalendarEvent, int>> subeventToIndex = CurrentList.Select((obj, index) => new mTuple<SubCalendarEvent, int>(obj, index)).ToDictionary(obj => obj.Item1, obj => obj);
            List<SubCalendarEvent> allSubEvents = CurrentList.ToList();

            List<TimeLine> timeLineWorks = new List<TimeLine>();
            if (unusableIndexes == null)
            {
                unusableIndexes = new HashSet<int>();
            }
            if (allSubEvents.Count > 0)
            {
                var subEventToViableTimeLine = Utility.subEventToMaxSpaceAvailable(timeLine, allSubEvents);
                bool foundViableFromPreceding = false;
                for (int i = 0; i < allSubEvents.Count; i++)
                {
                    var currentActiveSubEvent = allSubEvents[i];
                    var beforeAfterTimeLines = subEventToViableTimeLine[currentActiveSubEvent];
                    if (!foundViableFromPreceding)
                    {
                        var possibleBeforeTimeLine = subEvent.getTimeLineInterferringWithCalEvent(beforeAfterTimeLines.Item1) ?? new List<TimeLine>();
                        List<TimeLine> viableBeforeTImeLines = new List<TimeLine>();
                        foreach (var viableTimeLine in possibleBeforeTimeLine.Where(subTimeLine => subEvent.canExistWithinTimeLine(subTimeLine)))
                        {
                            viableBeforeTImeLines.AddRange(subEvent.getTimeLineInterferringWithCalEvent(viableTimeLine));
                        }
                        if (viableBeforeTImeLines.Count < 1)
                        {
                            unusableIndexes.Add(i);
                        }
                    }


                    var possibleAfterTimeLine = subEvent.getTimeLineInterferringWithCalEvent(beforeAfterTimeLines.Item2) ?? new List<TimeLine>();
                    List<TimeLine> viableAterTImeLines = new List<TimeLine>();
                    foreach (var viableTimeLine in possibleAfterTimeLine.Where(subTimeLine => subEvent.canExistWithinTimeLine(subTimeLine))) {
                        viableAterTImeLines.AddRange(subEvent.getTimeLineInterferringWithCalEvent(viableTimeLine));
                        foundViableFromPreceding = true;
                    }
                    if (viableAterTImeLines.Count < 1)
                    {
                        unusableIndexes.Add(i + 1);
                        foundViableFromPreceding = false;
                    }

                }
                timeLineWorks = subEvent.getTimeLineInterferringWithCalEvent(timeLine);
            }
            else {
                timeLineWorks = subEvent.getTimeLineInterferringWithCalEvent(timeLine);
            }
            List<SubCalendarEvent> pinnedToStart = new List<SubCalendarEvent>();
            List<SubCalendarEvent> pinnedToEnd = new List<SubCalendarEvent>();
            if (Utility.PinSubEventsToStart(CurrentList, timeLine))
            {
                foreach (TimeLine timeLineWork in timeLineWorks)
                {
                    pinnedToStart.AddRange(CurrentList.Where(obj => obj.getCalculationRange.doesTimeLineInterfere(timeLineWork)).OrderBy(obj => obj.Start));
                }
            }

            if (Utility.PinSubEventsToEnd(CurrentList, timeLine))
            {
                foreach (TimeLine timeLineWork in timeLineWorks)
                {
                    pinnedToEnd.AddRange(CurrentList.Where(obj => obj.getCalculationRange.doesTimeLineInterfere(timeLineWork)).OrderBy(obj => obj.End));
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
                if (unusableIndexes != null && unusableIndexes.Count > 0)
                {
                    foreach (int unusabeIndex in unusableIndexes)
                    {
                        int newIndex = unusabeIndex - startingIndex;
                        if ((newIndex <= fullSublist.Count) && newIndex >= 0)
                        {
                            updatedHashSet.Add(newIndex);
                        }
                    }
                }

            }
            int retValue = -1;
            if (fullSublist.Count > 0)
            {
                retValue = getBestPosition(subEvent, fullSublist, updatedHashSet, BorderElements);
                if (retValue != -1)
                {
                    retValue = startingIndex + retValue;
                }
            } else if (fullSublist.Count == 0 && (unusableIndexes == null || !unusableIndexes.Contains(0)))
            {
                retValue = 0;
            }

            return retValue;
        }


        int getBestPosition(SubCalendarEvent SubEvent, IEnumerable<SubCalendarEvent> CurrentList, HashSet<int> unusableIndexes = null, Tuple<Location, Location> BorderElements = null)
        {
            int i = 0;
            int RetValue = -1;
            if (unusableIndexes == null)
            {
                unusableIndexes = new HashSet<int>();
            }
            if (unusableIndexes.Count < CurrentList.Count() + 1)
            {
                int currentCount = CurrentList.Count();
                List<SubCalendarEvent> FullList = CurrentList.ToList();
                double[] TotalDistances = new double[currentCount + 1];
                int countLimit = TotalDistances.Count();
                foreach (int index in unusableIndexes)
                {
                    if (index < countLimit)
                    {
                        TotalDistances[index] = double.MaxValue;
                    }

                }

                double worstValue = double.MaxValue / (CurrentList.Count() + 2);
                for (; i <= currentCount; i++)
                {
                    if (!unusableIndexes.Contains(i))
                    {
                        List<SubCalendarEvent> FullList_Copy = FullList.ToList();
                        FullList_Copy.Insert(i, SubEvent);
                        Location firstBorderLocation = BorderElements?.Item1;
                        Location secondBorderLocation = BorderElements?.Item2;
                        double TotalDistance = SubCalendarEvent.CalculateDistance(FullList_Copy, 0, useFibonnacci: false);
                        if (firstBorderLocation != null)
                        {
                            TotalDistance += Location.calculateDistance(FullList_Copy.First().Location, firstBorderLocation, worstValue);
                        }

                        if (secondBorderLocation != null)
                        {
                            TotalDistance += Location.calculateDistance(FullList_Copy.Last().Location, secondBorderLocation, worstValue);
                        }
                        TotalDistances[i] = TotalDistance;
                    }

                }

                RetValue = TotalDistances.MinIndex();
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
            HashSet<SubCalendarEvent> subEventSet = new HashSet<SubCalendarEvent>();
            List<SubCalendarEvent> SubEventsInrespectivepaths = new List<SubCalendarEvent>();

            foreach (SubCalendarEvent subEvent in OrderedOptimizedGroupings.SelectMany(obj => obj.getEventsForStitichingWithOtherOptimizedGroupings()))
            {
                if (!subEventSet.Contains(subEvent))
                {
                    subEventSet.Add(subEvent);
                    SubEventsInrespectivepaths.Add(subEvent);
                }
            }

            List<SubCalendarEvent> SubEventsWithNoLocationPreference = AllGroupings[TimeOfDayPreferrence.DaySection.None].getPathStitchedSubevents();
            List<SubCalendarEvent> rigidSubeevents = DayInfo.getSubEventsInTimeLine().Where(obj => obj.isLocked).ToList();
            SubEventsInrespectivepaths.ForEach(subEVent => SubEventsWithNoLocationPreference.Remove(subEVent));

            List<SubCalendarEvent> splicedResults = spliceInNoneTimeOfDayPreferemce(SubEventsInrespectivepaths, SubEventsWithNoLocationPreference);




            List<SubCalendarEvent> pinnedResults = tryPinningInCurrentDayTimeline(myDayTimeLine, splicedResults, OrderedOptimizedGroupings);

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


        List<SubCalendarEvent> tryPinningInCurrentDayTimeline(TimeLine AllTimeLine, List<SubCalendarEvent> subEvents, List<OptimizedGrouping> OrderedOptimizedGroupings)
        {
            var retTuple = recursivelyPinnOrderedSubEventsInDayTimeline(AllTimeLine, subEvents);
            foreach (var kvp in retTuple.Item2)
            {
                OptimizedGrouping grouping = AllGroupings[kvp.Key];
                var groupSubevents = new HashSet<SubCalendarEvent>(grouping.getPathStitchedSubevents().Concat(grouping.getPinnedEvents()));
                List<SubCalendarEvent> orderedSubevents = groupSubevents.OrderBy(o => o.Start).ToList();
                grouping.clearPathStitchedEvents();
                grouping.ClearPinnedSubEvents();
                grouping.setPathStitchedEvents(orderedSubevents);
            }

            foreach (var grouping in OrderedOptimizedGroupings)
            {
                if (grouping.getPathStitchedSubevents().Count > 0)
                {
                    grouping.movePathStitchedToAcknowledged();
                }
            }
            return retTuple.Item1;
        }

        Tuple<List<SubCalendarEvent>, Dictionary<DaySection, int[]>, Dictionary<SubCalendarEvent, mTuple<TimeLine, TimeLine>>> recursivelyPinnOrderedSubEventsInDayTimeline(TimeLine AllTimeLine, List<SubCalendarEvent> SubEvents)
        {
            Dictionary<SubCalendarEvent, mTuple<TimeLine, TimeLine>> subEventToAvailableSpaces = new Dictionary<SubCalendarEvent, mTuple<TimeLine, TimeLine>>();// holds the latest spacing of each acknowledged sub event to the spacing before and after the sub event
            List<SubCalendarEvent> subEventList = new List<SubCalendarEvent>();//holds and ordered list of pinned sub events
            Dictionary<DaySection, int[]> daySectionToIndexes = new Dictionary<DaySection, int[]>();// holds the the lowest index and highest index of subevents in the daysection which is the key
            NoReason Noreason = NoReason.getNoReasonInstanceFactory();
            ///Function updates daySectionToIndexes appropriately
            Action<int, DaySection> updateEventList = (index, daySection) => {
                if (daySection != DaySection.None && daySection != DaySection.Disabled)
                {
                    int[] minAndMaxIndex;
                    if (daySectionToIndexes.ContainsKey(daySection))
                    {
                        minAndMaxIndex = daySectionToIndexes[daySection];
                        int minIndex = minAndMaxIndex[0];
                        int maxIndex = minAndMaxIndex[1];

                        if (index <= minIndex)
                        {
                            minAndMaxIndex[0] = index;
                        }

                        if (index >= maxIndex)
                        {
                            minAndMaxIndex[1] = index;
                        }
                    }
                    else
                    {
                        minAndMaxIndex = new int[2];
                        minAndMaxIndex[0] = index;
                        minAndMaxIndex[1] = index;
                        daySectionToIndexes.Add(daySection, minAndMaxIndex);
                    }
                }
            };

            if (Utility.PinSubEventsToStart(SubEvents, AllTimeLine))
            {
                if (SubEvents.Count > 0)
                {
                    for (int i = 0; i < SubEvents.Count; i++)
                    {
                        SubCalendarEvent eachSubCalendarEvent = SubEvents[i];
                        TimeOfDayPreferrence.DaySection DaySection = eachSubCalendarEvent.getDayPreference().getCurrentDayPreference();
                        if (DaySection == TimeOfDayPreferrence.DaySection.None)
                        {
                            eachSubCalendarEvent.getDayPreference().assignSectorBasedOnTIme(eachSubCalendarEvent.Start, AllTimeLine);
                            DaySection = eachSubCalendarEvent.getDayPreference().getCurrentDayPreference();
                        }
                        if (DaySection != TimeOfDayPreferrence.DaySection.Disabled)
                        {
                            AllGroupings[DaySection].AddToStitchedEvents(eachSubCalendarEvent);
                            eachSubCalendarEvent.getDayPreference().setCurrentdayPreference(DaySection);
                            Dictionary<Reason.Options, Reason> positionReasons = subEventToReason[eachSubCalendarEvent];
                            List<Reason> reasons = positionReasons.Where(keyValuePair => keyValuePair.Value.Option != Reason.Options.None).Select(keyValuePair => keyValuePair.Value).ToList();
                            foreach (Reason positionReason in reasons)
                            {
                                eachSubCalendarEvent.addReasons(positionReason);
                                positionReasons.Remove(positionReason.Option);
                            }
                        }
                        subEventList.Add(eachSubCalendarEvent);
                        updateEventList(i, DaySection);
                    }
                    subEventToAvailableSpaces = Utility.subEventToMaxSpaceAvailable(AllTimeLine, subEventList);
                }
            }
            else
            {
                List<SubCalendarEvent> recursionSubEvents = SubEvents.ToList();
                List<SubCalendarEvent> NonRigidis = recursionSubEvents.Where(obj => (!obj.isLocked) && (!obj.isOptimized)).OrderByDescending(obj => obj.Score).ToList();
                SubCalendarEvent UnwantedEvent = NonRigidis[0];
                recursionSubEvents.Remove(UnwantedEvent);
                TimeOfDayPreferrence.DaySection DaySection = UnwantedEvent.getDayPreference().getCurrentDayPreference();
                AllGroupings[DaySection].removeFromAcknowledged(UnwantedEvent);
                AllGroupings[DaySection].removeFromStitched(UnwantedEvent);
                UnwantedEvent.setAsUnOptimized();
                var recursionResult = recursivelyPinnOrderedSubEventsInDayTimeline(AllTimeLine, recursionSubEvents);
                subEventList = recursionResult.Item1;
                daySectionToIndexes = recursionResult.Item2;
                subEventToAvailableSpaces = recursionResult.Item3;
                HashSet<int> avoidIndexes = new HashSet<int>();
                int lowerBound = 0;// lowerbound holds the lowest viable index that a subevent can be inserted into before encroaching into another Daysector list of events
                int upperBound = subEventList.Count;// upperbound holds the highest viable index that a subevent can be inserted into before encroaching into another Daysector list of events. Note, this can is often one index above the current max index (if there is a subevent in the sector) because if the new sub event is inserted it automatically bumps the previous day sector
                if (daySectionToIndexes.ContainsKey(DaySection))
                {
                    avoidIndexes = new HashSet<int>();
                    int[] indexes = daySectionToIndexes[DaySection];
                    int minIdex = indexes[0];
                    int maxIdex = indexes[1];
                    lowerBound = minIdex;
                    upperBound = maxIdex;
                    upperBound += 1;
                }
                else
                {
                    Tuple<DaySection, DaySection> earlierAndLaterDaySection = getEarlierAndLaterDaySection(daySectionToIndexes, DaySection);
                    DaySection earlierDaySection = earlierAndLaterDaySection.Item1;
                    DaySection laterDaySection = earlierAndLaterDaySection.Item2;

                    if (earlierDaySection != DaySection.None)
                    {
                        int earlierDaySectionHigherIndex = daySectionToIndexes[earlierDaySection][1];
                        lowerBound = earlierDaySectionHigherIndex + 1;
                    }

                    if (laterDaySection != DaySection.None)
                    {
                        int laterDaySectionHigherIndex = daySectionToIndexes[laterDaySection][0];
                        upperBound = laterDaySectionHigherIndex;
                    }
                }

                int lowerUndesiredIndexes = lowerBound - 1;
                int upperUndesiredIndexes = upperBound + 1;
                avoidIndexes.Add(lowerUndesiredIndexes);
                avoidIndexes.Add(upperUndesiredIndexes);
                while (lowerUndesiredIndexes > 0)
                {
                    lowerUndesiredIndexes -= 1;
                    avoidIndexes.Add(lowerUndesiredIndexes);
                }

                while (upperUndesiredIndexes < subEventList.Count)
                {
                    upperUndesiredIndexes += 1;
                    avoidIndexes.Add(upperUndesiredIndexes);
                }


                int subEventIndex = lowerBound;
                int afterSubEventIndex = subEventIndex + 1;
                if (subEventIndex < subEventList.Count)// checks if lowerbound is not an index of an element in the list. This should only occur if lowerbound = subEventList.Count. This situat can arise if the current daysection is after all the other day section. eg DaySection is eveinnng and the other daysection with subevents are Morning and Evening
                {
                    do
                    {
                        SubCalendarEvent subEvent = subEventList[subEventIndex];
                        var timeLineBounds = subEventToAvailableSpaces[subEvent];
                        if (!UnwantedEvent.canExistWithinTimeLine(timeLineBounds.Item1))
                        {
                            avoidIndexes.Add(subEventIndex);
                        }
                        if (afterSubEventIndex <= upperBound)
                        {
                            if (!UnwantedEvent.canExistWithinTimeLine(timeLineBounds.Item2))
                            {
                                avoidIndexes.Add(afterSubEventIndex);
                            }
                        }
                        ++subEventIndex;
                        afterSubEventIndex = subEventIndex + 1;
                    } while (afterSubEventIndex <= upperBound);
                } else
                {
                    SubCalendarEvent subEvent = subEventList[subEventIndex - 1];
                    var timeLineBounds = subEventToAvailableSpaces[subEvent];
                    if (!UnwantedEvent.canExistWithinTimeLine(timeLineBounds.Item2))
                    {
                        avoidIndexes.Add(subEventIndex);
                    }
                }

                int bestPositionIndex = getBestPosition(AllTimeLine, UnwantedEvent, subEventList, avoidIndexes);
                if (bestPositionIndex != -1)
                {
                    AllGroupings[DaySection].AddToStitchedEvents(UnwantedEvent);
                    subEventList.Insert(bestPositionIndex, UnwantedEvent);
                    if (!Utility.tryPinSubEventsToEnd(subEventList, AllTimeLine))
                    {
                        throw new Exception("Something is wrong with best position");
                    }
                    else
                    {
                        if (daySectionToIndexes.ContainsKey(DaySection))
                        {
                            ++daySectionToIndexes[DaySection][1];//we need to increase the upper bound because we just inserted a subcalendar event into the sections list of sub event. The lower index will always staty the same, just the upper bound increases
                        } else
                        {
                            updateEventList(bestPositionIndex, DaySection);
                        }



                        int index = DaysectionToIndexDictionary[DaySection];
                        while (++index < ActiveDaySections.Count)
                        {
                            DaySection nextDaySection = ActiveDaySections[index];
                            increaseDaySectorCount(daySectionToIndexes, nextDaySection);
                        }
                    }


                    subEventToAvailableSpaces = Utility.subEventToMaxSpaceAvailable(AllTimeLine, subEventList);
                }


            }
            var retValue = new Tuple<List<SubCalendarEvent>, Dictionary<DaySection, int[]>, Dictionary<SubCalendarEvent, mTuple<TimeLine, TimeLine>>>(subEventList, daySectionToIndexes, subEventToAvailableSpaces);
            return retValue;
        }

        Tuple<DaySection, DaySection> getEarlierAndLaterDaySection(Dictionary<DaySection, int[]> DaySectionDictionary, DaySection currentDaySection)
        {
            ImmutableList<DaySection> daySections = ActiveDaySections;
            int currentSectorIndex = daySections.IndexOf(currentDaySection);
            DaySection earlierDaySection = getEarlierDaySection(DaySectionDictionary, currentDaySection);
            DaySection laterDaySection = getLaterDaySection(DaySectionDictionary, currentDaySection);
            var retValue = new Tuple<DaySection, DaySection>(earlierDaySection, laterDaySection);
            return retValue;
        }

        DaySection getEarlierDaySection(Dictionary<DaySection, int[]> DaySectionDictionary, DaySection currentDaySection)
        {
            ImmutableList<DaySection> daySections = ActiveDaySections;
            int currentSectorIndex = daySections.IndexOf(currentDaySection);
            DaySection earlierDaySection = DaySection.None;
            for (int i = currentSectorIndex - 1; i >= 0; i--)
            {
                DaySection daySection = daySections[i];
                if (DaySectionDictionary.ContainsKey(daySection))
                {
                    earlierDaySection = daySection;
                    break;
                }
            }

            return earlierDaySection;
        }

        DaySection getLaterDaySection(Dictionary<DaySection, int[]> DaySectionDictionary, DaySection currentDaySection)
        {
            ImmutableList<DaySection> daySections = ActiveDaySections;
            int currentSectorIndex = daySections.IndexOf(currentDaySection);
            DaySection laterDaySection = DaySection.None;

            for (int i = currentSectorIndex + 1; i < daySections.Count; i++)
            {
                DaySection daySection = daySections[i];
                if (DaySectionDictionary.ContainsKey(daySection))
                {
                    laterDaySection = daySection;
                    break;
                }
            }

            return laterDaySection;
        }

        void increaseDaySectorCount(Dictionary<DaySection, int[]> DaySectionDictionary, DaySection currentDaySection)
        {
            if (DaySectionDictionary.ContainsKey(currentDaySection))
            {
                var indexes = DaySectionDictionary[currentDaySection];
                ++indexes[0];
                ++indexes[1];
            }
        }




        ILookup<TimeOfDayPreferrence.DaySection, SubCalendarEvent> groupEvents(IEnumerable<SubCalendarEvent> SubEvents, DayTimeLine dayInfo)
        {
            foreach (SubCalendarEvent subevent in SubEvents)
            {
                subevent.updateDayPreference(AllGroupings.Select(group => group.Value).ToList());
            }

            ILookup<TimeOfDayPreferrence.DaySection, SubCalendarEvent> RetValue = SubEvents.ToLookup(obj => obj.getDayPreference().getCurrentDayPreference(), obj => obj);
            return RetValue;
        }

        #region Properties
        public IEnumerable<SubCalendarEvent> UnassignedSubEvents
        {
            get
            {
                return UnassignedSubevents.ToList();
            }
        }

        IEnumerable<OptimizedGrouping> ActiveDaySectionGrouping
        {
            get
            {
                return TimeOfDayPreferrence.ActiveDaySections.Where(daySection => this.AllGroupings.ContainsKey(daySection)).Select(daySection => this.AllGroupings[daySection]);
            }
        }

        public Location BeginLocation {
            get{
                return _BeginLocation;
            }
        }

        public Location EndLocation
        {
            get
            {
                return _EndLocation;
            }
        }
        #endregion
    }
}
