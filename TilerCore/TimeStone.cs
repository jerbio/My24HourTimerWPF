using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;
using ScheduleAnalysis;


namespace TilerCore
{
    public class TimeStone
    {
        public Schedule schedule { get; set; }
        virtual public async Task<Health> SubeventDifferentDay(DateTimeOffset newDay, EventID eventId)
        {
            CalendarEvent calEvent = schedule.getCalendarEvent(eventId);
            DayTimeLine timeLine = schedule.Now.getDayTimeLineByTime(newDay);
            TempTilerEventChanges tilerChanges = calEvent.prepForWhatIfDifferentDay(timeLine, eventId);
            schedule.updateAllEventDictionary(calEvent);

            if (schedule.CurrentLocation== null)
            {
                schedule.CurrentLocation = Location.getDefaultLocation();
            }
            await schedule.FindMeSomethingToDo(schedule.CurrentLocation).ConfigureAwait(false);
            Health scheduleHealth = new Health(schedule.getAllActiveCalendarEvents(), schedule.Now.ComputationRange.Start, schedule.Now.ComputationRange.TimelineSpan, schedule.Now, schedule.getHomeLocation);
            calEvent.ReverseWhatIf(tilerChanges);
            return scheduleHealth;
        }

        /// <summary>
        /// function assesses changes to a schedule. It tests if te start time is deferred to a different start time and then tries to see the effect of the schedule change.
        /// </summary>
        /// <param name="pushSpan"></param>
        /// <param name="eventId"></param>
        /// <param name="assessmentWindow"></param>
        /// <returns></returns>
        virtual public async Task<Tuple<Health, Health>> PushSingleEvent(TimeSpan pushSpan, EventID eventId, TimeLine assessmentWindow)
        {
            if (assessmentWindow == null)
            {
                assessmentWindow = new TimeLine(schedule.Now.constNow, schedule.Now.constNow.AddDays(7));
            }
            CalendarEvent calEvent = schedule.getCalendarEvent(eventId);
            DateTimeOffset newStartTime = schedule.Now.constNow + pushSpan;

            var beforeNow = new ReferenceNow(schedule.Now.constNow, schedule.Now.EndOfDay, schedule.Now.TimeZoneDifference);
            Health beforeChange = new Health(schedule.getAllCalendarEvents().Where(obj => obj.isActive).Select(obj => obj.createCopy()), beforeNow.constNow, assessmentWindow.TimelineSpan, beforeNow, schedule.getHomeLocation);
            if (schedule.CurrentLocation== null)
            {
                schedule.CurrentLocation = Location.getDefaultLocation();
            }
            var procrastinateResult = schedule.ProcrastinateJustAnEvent(eventId.ToString(), pushSpan);
            Health afterChange = new Health(procrastinateResult.Item2.Values.Where(obj => obj.isActive), schedule.Now.constNow, assessmentWindow.TimelineSpan, schedule.Now, schedule.getHomeLocation);
            var retValue = new Tuple<Health, Health>(beforeChange, afterChange);
            return retValue;
        }

        /// <summary>
        /// function assesses changes to a schedule. It tests if a time chunk is cleared out. It tries to see the effect of the schedule change.
        /// </summary>
        /// <param name="pushSpan"></param>
        /// <param name="assessmentWindow"></param>
        /// <returns></returns>
        virtual public async Task<Tuple<Health, Health>> PushedAll(TimeSpan pushSpan, TimeLine assessmentWindow)
        {
            if (assessmentWindow == null)
            {
                assessmentWindow = new TimeLine(schedule.Now.constNow, schedule.Now.constNow.AddDays(7));
            }
            DateTimeOffset newStartTime = schedule.Now.constNow + pushSpan;
            if (schedule.CurrentLocation== null)
            {
                schedule.CurrentLocation= Location.getDefaultLocation();
            }
            var beforeNow = new ReferenceNow(schedule.Now.constNow, schedule.Now.EndOfDay, schedule.Now.TimeZoneDifference);
            var procradstinateResult = schedule.ProcrastinateAll(pushSpan);

            var beforeCalevents = procradstinateResult.Item2.Values.Where(obj => obj.isActive).Select(obj => obj.createCopy()).ToList();
            List<SubCalendarEvent> beforeSubEvents = beforeCalevents.SelectMany(calEvent => calEvent.ActiveSubEvents).Where(subEvent => !subEvent.isDesignated).ToList();
            var orderedDayTimeLines = beforeNow.getAllDaysLookup().OrderBy(obj => obj.Key).Select(obj => obj.Value).ToList();


            WhatIfSubEventDayDesignation(orderedDayTimeLines.ToArray(), beforeSubEvents);
            Health beforeChange = new Health(procradstinateResult.Item2.Values.Where(obj => obj.isActive).Select(obj => obj.createCopy()), beforeNow.constNow, assessmentWindow.TimelineSpan, beforeNow, schedule.getHomeLocation);

            var afterSubEVents = schedule.getAllActiveCalendarEvents().SelectMany(calEvent => calEvent.ActiveSubEvents).Where(subEvent => { subEvent.resetAndgetUnUsableIndex(); return true; });//.Where(subEvent => !subEvent.isDesignated).ToList();
            var afterNow = new ReferenceNow(schedule.Now.constNow, schedule.Now.EndOfDay, schedule.Now.TimeZoneDifference);
            var afterCalevents = schedule.getAllActiveCalendarEvents().Where(obj => obj.isActive).ToList();
            var afterorderedDayTimeLines = afterNow.getAllDaysLookup().OrderBy(obj => obj.Key).Select(obj => obj.Value);
            //afterCalevents.AsParallel().ForAll((calEvent) => calEvent.InitialCalculationLookupDays(afterorderedDayTimeLines, afterNow));
            WhatIfSubEventDayDesignation(afterorderedDayTimeLines.ToArray(), afterSubEVents);

            Health afterChange = new Health(schedule.getAllCalendarEvents().Where(obj => obj.isActive), afterNow.constNow, assessmentWindow.TimelineSpan, afterNow, schedule.getHomeLocation);
            var retValue = new Tuple<Health, Health>(beforeChange, afterChange);
            return retValue;
        }

        /// <summary>
        /// function assesses changes to a schedule. It tests if a time chunk is cleared out. It tries to see the effect of the schedule change.
        /// </summary>
        /// <param name="pushSpan"></param>
        /// <param name="assessmentWindow"></param>
        /// <returns></returns>
        virtual public async Task<Tuple<Health, Health>> EventUpdate(
            DateTimeOffset SubeventStart,
            DateTimeOffset SubeventEnd,
            DateTimeOffset TimeLineStart,
            DateTimeOffset TimeLineEnd,
            int SplitCount,
            string eventId)
        {
            SubCalendarEvent subEvent = schedule.getSubCalendarEvent(eventId);
            var travelCache = schedule.TravelCache;
            var deepCopy = schedule.getDeepCopyOfEventDictionary();
            var locationCache = schedule.getAllLocations().ToDictionary(location => location.Id, location => location);

            schedule.BundleChangeUpdate(eventId, subEvent.Name, SubeventStart, SubeventEnd, TimeLineStart, TimeLineEnd, SplitCount, "PREViEW CHANGES");

            Schedule beforeSchedule = new Schedule(deepCopy, schedule.Now.EndOfDayStartOfTime, locationCache, schedule.Now.constNow, schedule.User, null);
            beforeSchedule.updateTravelCache(travelCache);

            Health beforeSetAsNow = new Health(beforeSchedule.getAllCalendarEvents(), schedule.Now.ComputationRange.Start, schedule.Now.ComputationRange.TimelineSpan, beforeSchedule.Now, beforeSchedule.getHomeLocation);
            Health afterSetAsNow = new Health(schedule.getAllCalendarEvents(), schedule.Now.ComputationRange.Start, schedule.Now.ComputationRange.TimelineSpan, schedule.Now, schedule.getHomeLocation);
            var retValue = new Tuple<Health, Health>(beforeSetAsNow, afterSetAsNow);
            return retValue;
        }

        virtual public async Task<Tuple<Health, Health>> EventUpdate()
        {
            var travelCache = schedule.TravelCache;
            var deepCopy = schedule.getDeepCopyOfEventDictionary();
            var locationCache = schedule.getAllLocations().ToDictionary(location => location.Id, location => location);
            schedule.UpdateSchedule();

            Schedule beforeSchedule = new Schedule(deepCopy, schedule.Now.EndOfDayStartOfTime, locationCache, schedule.Now.constNow, schedule.User, null);
            beforeSchedule.updateTravelCache(travelCache);

            

            Health beforeSetAsNow = new Health(beforeSchedule.getAllCalendarEvents(), schedule.Now.ComputationRange.Start, schedule.Now.ComputationRange.TimelineSpan, beforeSchedule.Now, beforeSchedule.getHomeLocation);
            Health afterSetAsNow = new Health(schedule.getAllCalendarEvents(), schedule.Now.ComputationRange.Start, schedule.Now.ComputationRange.TimelineSpan, schedule.Now, schedule.getHomeLocation);
            var retValue = new Tuple<Health, Health>(beforeSetAsNow, afterSetAsNow);
            return retValue;
        }


        Dictionary<SubCalendarEvent, List<long>> WhatIfSubEventDayDesignation(DayTimeLine[] OrderedyAscendingAllDays, IEnumerable<SubCalendarEvent> subEvents)
        {
            long First = OrderedyAscendingAllDays.First().UniversalIndex;
            Dictionary<SubCalendarEvent, List<long>> RetValue = new Dictionary<SubCalendarEvent, List<long>>();

            List<SubCalendarEvent> orderedSubevents = subEvents.OrderBy(o => o.Start).ToList();

            // handles scenario where fore some reason issleep and iswake events are not instantiated
            List<SubCalendarEvent> sleepEvents = new List<SubCalendarEvent>();
            List<SubCalendarEvent> wakeEvents = new List<SubCalendarEvent>();
            foreach (SubCalendarEvent subEvent in subEvents)
            {
                if (subEvent.isWake)
                {
                    wakeEvents.Add(subEvent);
                }
                if (subEvent.isSleep)
                {
                    sleepEvents.Add(subEvent);
                }
            }

            if (orderedSubevents.Count > 0)
            {
                SubCalendarEvent previousSubevent = orderedSubevents[0];
                int BoundedIndex = -1;
                int previousBoundedIndex = -1;
                for (int subEventDayindex = 0; subEventDayindex < orderedSubevents.Count; subEventDayindex++)
                {
                    SubCalendarEvent eachSubCalendarEvent = orderedSubevents[subEventDayindex];
                    List<long> myDays = new List<long>();
                    long SubCalFirstIndex = schedule.Now.getDayIndexFromStartOfTime(eachSubCalendarEvent.Start);
                    long SubCalLastIndex = schedule.Now.getDayIndexFromStartOfTime(eachSubCalendarEvent.End);
                    long DayDiff = SubCalLastIndex - SubCalFirstIndex;

                    BoundedIndex = (int)(SubCalFirstIndex - First);
                    if ((BoundedIndex < 0) || (BoundedIndex >= OrderedyAscendingAllDays.Length))
                    {
                        continue;
                    }
                    myDays.Add(SubCalFirstIndex);
                    OrderedyAscendingAllDays[BoundedIndex].AddToSubEventList(eachSubCalendarEvent);
                    eachSubCalendarEvent.ParentCalendarEvent.designateSubEvent(eachSubCalendarEvent, schedule.Now);
                    Action updateIsWakeAndSleep = () =>
                    {
                        if (sleepEvents.Count > 0 && wakeEvents.Count > 0)
                        {
                            if (eachSubCalendarEvent.isWake)
                            {
                                OrderedyAscendingAllDays[BoundedIndex].WakeSubEvent = eachSubCalendarEvent;
                            }

                            if (eachSubCalendarEvent.isSleep)
                            {
                                OrderedyAscendingAllDays[BoundedIndex].SleepSubEvent = eachSubCalendarEvent;
                            }
                        }
                        else// handles just in case issleep and isWake is false on all subevents. it defaults to last and first events of day hence the ordering of subevents
                        {
                            if (OrderedyAscendingAllDays[BoundedIndex].WakeSubEvent == null)
                            {
                                OrderedyAscendingAllDays[BoundedIndex].WakeSubEvent = eachSubCalendarEvent;
                            }

                            if (previousBoundedIndex != BoundedIndex && previousBoundedIndex != -1)
                            {
                                if (OrderedyAscendingAllDays[previousBoundedIndex].SleepSubEvent == null)
                                {
                                    OrderedyAscendingAllDays[previousBoundedIndex].SleepSubEvent = previousSubevent;
                                    previousBoundedIndex = BoundedIndex;
                                }
                            }
                        }
                    };
                    if (DayDiff > 0)
                    {
                        for (long i = SubCalFirstIndex + 1, j = 0; j < DayDiff; j++, i++)
                        {
                            BoundedIndex = (int)(i - First);
                            if (BoundedIndex < OrderedyAscendingAllDays.Length)// in case the rigid sub event day index is higher than OrderedyAscendingAllDays max index
                            {
                                OrderedyAscendingAllDays[BoundedIndex].AddToSubEventList(eachSubCalendarEvent);
                                updateIsWakeAndSleep();
                                myDays.Add(i);
                            }

                        }
                    }
                    else
                    {
                        updateIsWakeAndSleep();
                    }

                    RetValue.Add(eachSubCalendarEvent, myDays);
                    previousSubevent = eachSubCalendarEvent;
                    previousBoundedIndex = BoundedIndex;
                }

                if (sleepEvents.Count == 0 || wakeEvents.Count == 0)// handles just in case issleep and isWake is false on all subevents. it defaults to last and first events of day hence the ordering of subevents
                {
                    if (OrderedyAscendingAllDays[BoundedIndex].SleepSubEvent == null)
                    {
                        OrderedyAscendingAllDays[BoundedIndex].SleepSubEvent = previousSubevent;
                        previousBoundedIndex = BoundedIndex;
                    }
                }
            }
            return RetValue;
        }
        
        virtual public async Task<Tuple<Health, Health>> WhatIfSetAsNow(string eventId) {
            SubCalendarEvent subEvent = schedule.getSubCalendarEvent(eventId);
            var travelCache = schedule.TravelCache;
            var deepCopy = schedule.getDeepCopyOfEventDictionary();
            var locationCache = schedule.getAllLocations().ToDictionary(location => location.Id, location => location);
            if (subEvent == null)
            {
                schedule.SetSubeventAsNow(eventId);
            }
            else
            {
                schedule.SetCalendarEventAsNow(eventId);
            }
            Schedule beforeSchedule = new Schedule(deepCopy, schedule.Now.EndOfDayStartOfTime, locationCache, schedule.Now.constNow, null, null);
            beforeSchedule.updateTravelCache(travelCache);

            Health beforeSetAsNow = new Health(beforeSchedule.getAllCalendarEvents(), schedule.Now.ComputationRange.Start, schedule.Now.ComputationRange.TimelineSpan, beforeSchedule.Now, beforeSchedule.getHomeLocation);
            Health afterSetAsNow = new Health(schedule.getAllCalendarEvents(), schedule.Now.ComputationRange.Start, schedule.Now.ComputationRange.TimelineSpan, schedule.Now, schedule.getHomeLocation);
            var retValue = new Tuple<Health, Health>(beforeSetAsNow, afterSetAsNow);
            return retValue;
        }



        virtual public async Task<Tuple<Health, Health>> WhatIfSetAsNow(EventID eventId) {
            return await WhatIfSetAsNow(eventId.ToString()).ConfigureAwait(false);
        }
    }
}
