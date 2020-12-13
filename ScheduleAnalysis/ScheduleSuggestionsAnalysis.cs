﻿using System;
using System.Collections.Generic;
using System.Text;
using TilerElements;
using System.Linq;
using GoogleMapsApi.Entities.DistanceMatrix.Request;

namespace ScheduleAnalysis
{
    public class ScheduleSuggestionsAnalysis
    {
        List<SubCalendarEvent> OrderedActiveSubEvents;
        List<SubCalendarEvent> AllSubEvents;
        HashSet<CalendarEvent> MovableCalEvents = new HashSet<CalendarEvent>();
        HashSet<CalendarEvent> ReduceableCalEvents = new HashSet<CalendarEvent>();
        HashSet<CalendarEvent> RigidCalEvents = new HashSet<CalendarEvent>();
        Dictionary<string, SubCalendarEvent> subEventId_to_Subevents = new Dictionary<string, SubCalendarEvent>();
        Dictionary<string, CalendarEvent> calEventId_to_CalEvents = new Dictionary<string, CalendarEvent>();
        ILookup<string, SubCalendarEvent> calEventId_to_Subevents;
        ReferenceNow Now;
        Analysis analysis;
        TilerUser TilerUser;
        static readonly double MaxActivationRatio = 0.75;
        double activeRatioBound;// The active ratio limit for when over scheduling is assumed to occur
        public ScheduleSuggestionsAnalysis(IEnumerable<SubCalendarEvent> subEvents, ReferenceNow now, TilerUser tilerUser, Analysis analysis)
        {
            this.analysis = analysis;
            HashSet<SubCalendarEvent> allSubEvent = new HashSet<SubCalendarEvent>(subEvents);
            HashSet<SubCalendarEvent> subEventsForEvaluation = new HashSet<SubCalendarEvent>(allSubEvent.Where(subEvent => subEvent.isActive && !subEvent.isProcrastinateEvent && subEvent.getActiveDuration <= Utility.LeastAllDaySubeventDuration));
            OrderedActiveSubEvents = subEventsForEvaluation.OrderBy(o=>o.Start).ThenBy(o=>o.End).ToList();
            subEventId_to_Subevents = new Dictionary<string, SubCalendarEvent>();
            activeRatioBound = analysis?.CompletionRate ?? Analysis.DefaultActivationRatio;
            calEventId_to_Subevents = OrderedActiveSubEvents.ToLookup(obj => obj.SubEvent_ID.getAllEventDictionaryLookup.ToString());
            foreach (SubCalendarEvent subEvent in subEventsForEvaluation)
            {
                subEventId_to_Subevents.Add(subEvent.Id, subEvent);
                if(!calEventId_to_CalEvents.ContainsKey(subEvent.SubEvent_ID.getAllEventDictionaryLookup))
                {
                    subEvent.ParentCalendarEvent.resetAllDeadlineSuggestions();
                    calEventId_to_CalEvents.Add(subEvent.SubEvent_ID.getAllEventDictionaryLookup, subEvent.ParentCalendarEvent);
                }

                if (subEvent.ParentCalendarEvent.isRigid)
                {
                    RigidCalEvents.Add(subEvent.ParentCalendarEvent);
                }
                else
                {
                    if (subEvent.IsFromRecurring)
                    {
                        ReduceableCalEvents.Add(subEvent.ParentCalendarEvent);
                    }
                    else if (!subEvent.isRigid)
                    {
                        MovableCalEvents.Add(subEvent.ParentCalendarEvent);
                    }
                }
            }
            this.AllSubEvents = allSubEvent.ToList();
            Now = now;
            TilerUser = tilerUser;
            updateCompletionRate(this.AllSubEvents);

        }

        public void updateCompletionRate(IEnumerable<SubCalendarEvent> subEvents)
        {
            DateTimeOffset timelineStart = Now.getClientBeginningOfDay(Now.getDayIndexFromStartOfTime(Now.constNow.AddDays(-28)));
            DateTimeOffset timelineEnd = Now.getClientBeginningOfDay(Now.getDayIndexFromStartOfTime(Now.constNow));
            TimeLine lastFourWeekTimeLine = new TimeLine(timelineStart, timelineEnd);
            IEnumerable<SubCalendarEvent> completedOrRigidSubEvents = subEvents.Where(subEvent =>
                    (!subEvent.isProcrastinateEvent && subEvent.getActiveDuration <= Utility.LeastAllDaySubeventDuration) &&
                    ((lastFourWeekTimeLine.IsTimeLineWithin(subEvent.StartToEnd) && subEvent.isRigid) ||
                    ((subEvent.getIsComplete && 
                        (lastFourWeekTimeLine.IsDateTimeWithin(subEvent.CompletionTime) || 
                        lastFourWeekTimeLine.IsTimeLineWithin(subEvent))
                    )))
                ).OrderBy(o=>o.Start).ToList();

            List<SubCalendarEvent> completeSubevents = new List<SubCalendarEvent>();
            List<SubCalendarEvent> rigidSubevents = new List<SubCalendarEvent>();

            List<TimeLine> allWeeks = new List<TimeLine>();
            DateTimeOffset timelineLimit = timelineStart;
            while (timelineLimit < timelineEnd)
            {
                DateTimeOffset eachTimeLineEnd = timelineLimit.AddDays(7);
                TimeLine timeline = new TimeLine(timelineLimit, eachTimeLineEnd);
                allWeeks.Add(timeline);
                timelineLimit = eachTimeLineEnd;
            }

            foreach (SubCalendarEvent subEvent in completedOrRigidSubEvents)
            {
                if(subEvent.isRigid)
                {
                    rigidSubevents.Add(subEvent);
                }
                else if (subEvent.getIsComplete)
                {
                    completeSubevents.Add(subEvent);
                }
            }

            var conflictingAndNonConflictingRigids = Utility.getConflictingEvents(rigidSubevents);
            List<SubCalendarEvent> nonConflictingRigidSubevents = conflictingAndNonConflictingRigids.Item1.Concat(conflictingAndNonConflictingRigids.Item2).OrderBy(o=>o.Start).ToList();

            HashSet<TimeLine> validWeeks = new HashSet<TimeLine>();// holds the weeks in which non rigid events were actually found. This is crucial because if it's common to have google calendar events that are scheduled in the past

            foreach(SubCalendarEvent subEvent in completeSubevents)
            {
                foreach(TimeLine timeline in allWeeks.Where(timeLine => timeLine.doesTimeLineInterfere(subEvent)))
                {
                    validWeeks.Add(timeline);
                    timeline.AddBusySlots(subEvent.ActiveSlot);
                }
            }

            foreach (SubCalendarEvent subEvent in nonConflictingRigidSubevents)
            {
                foreach (TimeLine timeline in allWeeks.Where(timeLine => timeLine.doesTimeLineInterfere(subEvent)))
                {
                    timeline.AddBusySlots(subEvent.ActiveSlot);
                }
            }

            if(validWeeks.Count > 0)
            {
                TimeSpan totalCompletedSpan = TimeSpan.FromTicks(validWeeks.Sum(o => o.OccupiedSlots.Sum(activeSlot => activeSlot.TimelineSpan.Ticks)));
                TimeSpan totalWeekSpan = TimeSpan.FromHours(validWeeks.Sum(o => (o.TimelineSpan.TotalHours)));

                TimeSpan additionalArbitraryUsedUpHoursPerDay =TimeSpan.FromHours( (totalWeekSpan.TotalHours / Utility.TwentyFourHoursAlmostTimeSpan.TotalHours) * Utility.ArbitraryDayUseUpTimeSpan.TotalHours);

                TimeSpan readjustedWeekSpan = totalWeekSpan + additionalArbitraryUsedUpHoursPerDay;

                activeRatioBound = totalCompletedSpan.TotalHours / readjustedWeekSpan.TotalHours;
                if(activeRatioBound> MaxActivationRatio)
                {
                    activeRatioBound = MaxActivationRatio;
                }
            }
            if (this.analysis!=null)
            {
                this.analysis.setComplentionRate(activeRatioBound, DateTimeOffset.UtcNow);
            }
        }

        public void evaluateCalendarEvents(IEnumerable<CalendarEvent>calEvents)
        {
            if(calEvents!=null && calEvents.Count() > 0 )
            {
                List<CalendarEvent> calendarEventsForAnalysis = calEvents.ToList();
                IList<IList<double>> multiDimensionalVar = new List<IList<double>>();
                foreach(CalendarEvent calEvent in calendarEventsForAnalysis)
                {
                    calEvent.resetScore();
                    double ratio = ((double)calEvent.CompletionCount) / (calEvent.NumberOfSplit);
                    TimeLineHistory timeLineHistory = calEvent.TimeLineHistory;
                    int timelineChangeCount = timeLineHistory.TimeLines.Count+1;
                    List<double> featureArgs = new List<double>() { ratio, timelineChangeCount };
                    multiDimensionalVar.Add(featureArgs);
                    
                }
                List<double> scores  = Utility.multiDimensionCalculationNormalize(multiDimensionalVar);

                for(int i=0; i < calendarEventsForAnalysis.Count;i++)
                {
                    CalendarEvent calEvent = calendarEventsForAnalysis[i];
                    double score = scores[i];
                    calEvent.setScore(score);
                }
            }
        }

        public void Analyze()
        {
            if(this.OrderedActiveSubEvents!=null && this.OrderedActiveSubEvents.Count > 0)
            {
                List<SubCalendarEvent> allCompletedSubevents = OrderedActiveSubEvents.Where(o => o.getIsComplete && o.isParentComplete).ToList();
                List<SubCalendarEvent> completedSubeventsOrderedByStart = allCompletedSubevents.OrderBy(sub => sub.CompletionTime).ToList();
                SubCalendarEvent firstCompletedSubevent = completedSubeventsOrderedByStart.First();
                TimeSpan oneWeekSpan = Utility.OneWeekTimeSpan;
                TimeLine currentWeekTimeline = new TimeLine(firstCompletedSubevent.Start, firstCompletedSubevent.Start.Add(oneWeekSpan));
                Dictionary<TimeLine, List<SubCalendarEvent>> weekTimeLineToCompletedSubevents = new Dictionary<TimeLine, List<SubCalendarEvent>>();
                weekTimeLineToCompletedSubevents.Add(currentWeekTimeline, new List<SubCalendarEvent>());
                for ( int i=0; i< completedSubeventsOrderedByStart.Count;i++)
                {
                    SubCalendarEvent completedSubEvent = completedSubeventsOrderedByStart[i];
                    if(currentWeekTimeline.IsDateTimeWithin(completedSubEvent.Start))
                    {
                        weekTimeLineToCompletedSubevents[currentWeekTimeline].Add(completedSubEvent);
                    } else
                    {
                        TimeLine nextTimeLine = new TimeLine(currentWeekTimeline.End, currentWeekTimeline.End.Add(oneWeekSpan));
                        while (!nextTimeLine.IsDateTimeWithin(completedSubEvent.Start))
                        {
                            weekTimeLineToCompletedSubevents.Add(nextTimeLine, new List<SubCalendarEvent>());
                            nextTimeLine = new TimeLine(nextTimeLine.End, nextTimeLine.End.Add(oneWeekSpan));
                        }
                        List<SubCalendarEvent> subEventWithinWeekList = new List<SubCalendarEvent>() { completedSubEvent };
                        weekTimeLineToCompletedSubevents.Add(nextTimeLine, subEventWithinWeekList);
                    }
                }
            }
        }

        /// <summary>
        /// Function simply tries to detect if there is an over scheduling and then matches each subevent to the week where they are currently overscheduled
        /// </summary>
        /// <param name="currentNow"></param>
        public List<TimeLine> getOverLoadedWeeklyTimelines(DateTimeOffset currentNow)
        {
            List<TimeLine> retValue = new List<TimeLine>();
            if(this.OrderedActiveSubEvents.Count >0)
            {
                DateTimeOffset lastSubEventDate = this.OrderedActiveSubEvents.Last().End;
                const int numberOfDaysInWeek = 7;
                var dayOfWeek_timeline = Now.getDayOfTheWeek(currentNow);

                int dayDiff = 1;
                List<TimeLine> eachWeekTimeline = new List<TimeLine>();
                List<TimeLine> compoundedWeekTimelines = new List<TimeLine>();

                DateTimeOffset timeLineStart = currentNow;
                DateTimeOffset timeLineEnd = Now.getDayTimeLineByTime(currentNow).End;

                TimeLine timeline = new TimeLine(timeLineStart, timeLineEnd);

                TimeLine weekTimeline = timeline.StartToEnd;
                TimeLine compoundTimeline = weekTimeline.StartToEnd;
                eachWeekTimeline.Add(weekTimeline);
                compoundedWeekTimelines.Add(weekTimeline);
                List<SubCalendarEvent> subeEventsForProcessing = this.OrderedActiveSubEvents.Where(o => o.Start > weekTimeline.Start).ToList();
                TimeLine testTimeline = null;
                if(testTimeline!=null)
                {
                    var testActiveSubEvents = this.OrderedActiveSubEvents.Where(o => testTimeline.doesTimeLineInterfere(o)).ToList();
                    testTimeline.AddBusySlots(testActiveSubEvents.Select(o=>o.ActiveSlot));



                }
                for (int i = 0; i>=0 && i< subeEventsForProcessing.Count;i++)
                {
                    SubCalendarEvent subEvent = subeEventsForProcessing[i];
                    TimeLine subevent_startToEnd = subEvent.StartToEnd;
                    if (subEvent.Start < weekTimeline.End)
                    {
                        if (weekTimeline.doesTimeLineInterfere(subevent_startToEnd))
                        {
                            weekTimeline.AddBusySlots(subEvent.ActiveSlot);
                            compoundTimeline.AddBusySlots(subEvent.ActiveSlot);
                        }
                    } else
                    {
                        if(subEvent.End >= weekTimeline.Start)
                        {
                            timeLineStart = weekTimeline.End;
                            timeLineEnd = timeLineStart.AddDays(dayDiff);
                            weekTimeline = new TimeLine(timeLineStart, timeLineEnd);
                            TimeLine previousTimeline = compoundTimeline;
                            compoundTimeline = new TimeLine(currentNow, weekTimeline.End);
                            compoundTimeline.AddBusySlots(previousTimeline.OccupiedSlots);
                            eachWeekTimeline.Add(weekTimeline);
                            compoundedWeekTimelines.Add(compoundTimeline);
                            if (weekTimeline.doesTimeLineInterfere(subevent_startToEnd))
                            {
                                weekTimeline.AddBusySlots(subEvent.ActiveSlot);
                                compoundTimeline.AddBusySlots(subEvent.ActiveSlot);
                                compoundTimeline.AddBusySlots(subeEventsForProcessing.Take(i).Select(o => o.ActiveSlot));
                            } else
                            {
                                --i;
                            }
                        } else
                        {
                            break;
                        }
                    }
                }

                List<TimeLine> overOccupied = new List<TimeLine>();
                foreach(TimeLine eachCopoundTimeline in compoundedWeekTimelines)
                {
                    double activeRatio = eachCopoundTimeline.ActiveRatio;
                    if (activeRatio >= activeRatioBound)
                    {
                        overOccupied.Add(eachCopoundTimeline);

                    }
                }
                if(overOccupied.Count > 0)
                {
                    retValue = overOccupied;
                }
            }

            return retValue;
        }

        public DateTimeOffset evaluateIdealDeadline(CalendarEvent calEvent, List<CalendarEvent> scheduledEvents)
        {
            DateTimeOffset retValue = new DateTimeOffset();
            if (!calEvent.IsFromRecurring)
            {
                DateTimeOffset start = Now.constNow;
                List<SubCalendarEvent> subEvents = new List<SubCalendarEvent>(scheduledEvents
                    .SelectMany(eachCalEvent => eachCalEvent.ActiveSubEvents)
                    .Where(subEvent =>
                        subEvent.getActiveDuration <= Utility.OneDayTimeSpan &&
                        subEvent.End >= start
                    )
                    .OrderBy(subEvent => subEvent.Start)
                    .ThenBy(subEvent => subEvent.End));
                Dictionary<SubCalendarEvent, int> subEVentToIndex = new Dictionary<SubCalendarEvent, int>();
                TimeSpan subEventDuration = calEvent.getActiveDuration;
                
                double multiplier = 1;
                TimeSpan sevenDaySpan = TimeSpan.FromDays(7);
                if (calEvent.getIsEventRestricted)
                {
                    TimeSpan totalSubEVentActiveSpan = TimeSpan.FromTicks((calEvent.RestrictionProfile.DaySelection.Select(restrictionDay => restrictionDay.RestrictionTimeLine.Span).Sum(timeSpan => timeSpan.Ticks)));
                    multiplier = ((double)sevenDaySpan.Ticks / (double)totalSubEVentActiveSpan.Ticks);
                }
                TimeSpan effectiveSubEventSpan = TimeSpan.FromTicks((long)(subEventDuration.Ticks * multiplier));
                TimeSpan idealFreeSpan = TimeSpan.FromTicks((long)((double)(effectiveSubEventSpan.Ticks) / (this.activeRatioBound)));
                TimeLine encompassedTimeLine = new TimeLine(start, start.Add(idealFreeSpan));
                TimeSpan otherTotalUsedUpTimeSpan = new TimeSpan();
                List<SubCalendarEvent> subEventsWithinTimeline = new List<SubCalendarEvent>( subEvents.Where(eachSubEvent => encompassedTimeLine.doesTimeLineInterfere(eachSubEvent)));
                List<SubCalendarEvent> updatedSubEventsWithinTimeline = new List<SubCalendarEvent>(subEventsWithinTimeline);




                do
                {
                    foreach (SubCalendarEvent earliestSubEvent in subEventsWithinTimeline)
                    {
                        TimeSpan effectiveOtherSubEventSpan = earliestSubEvent.getActiveDuration;
                        double otherSubEventmultiplier = 1;
                        if (earliestSubEvent.getIsEventRestricted)
                        {
                            TimeSpan totalOtherSubEVentActiveSpan = TimeSpan.FromTicks(
                                (earliestSubEvent.RestrictionProfile.DaySelection
                                    .Select(restrictionDay => restrictionDay.RestrictionTimeLine.Span)
                                    .Sum(timeSpan => timeSpan.Ticks)));
                            otherSubEventmultiplier = ((double)sevenDaySpan.Ticks / (double)totalOtherSubEVentActiveSpan.Ticks);
                        }
                        effectiveOtherSubEventSpan = TimeSpan.FromTicks((long)(effectiveOtherSubEventSpan.Ticks * otherSubEventmultiplier));
                        otherTotalUsedUpTimeSpan.Add(effectiveOtherSubEventSpan);
                    }

                    TimeSpan totalUsedUpSpan = effectiveSubEventSpan + otherTotalUsedUpTimeSpan;
                    TimeSpan idealTimeLineSpan= TimeSpan.FromTicks((long)((double)(totalUsedUpSpan.Ticks) / (this.activeRatioBound)));
                    encompassedTimeLine = new TimeLine(encompassedTimeLine.Start, start.Add(idealTimeLineSpan));

                    subEventsWithinTimeline = new List<SubCalendarEvent>(subEvents.Skip(updatedSubEventsWithinTimeline.Count).Where(eachSubEvent => encompassedTimeLine.doesTimeLineInterfere(eachSubEvent)));
                    updatedSubEventsWithinTimeline = updatedSubEventsWithinTimeline.Concat(subEventsWithinTimeline).ToList();
                }
                while (subEventsWithinTimeline.Count != 0 && updatedSubEventsWithinTimeline.Count != subEvents.Count);
                retValue = encompassedTimeLine.End;
            }

            return retValue;
        }

        public ScheduleSuggestion suggestScheduleChange(List<TimeLine> timelines )
        {
            ScheduleSuggestion suggestion = new ScheduleSuggestion();
            List<TimeLine> orderedTimelines = timelines.OrderBy(o => o.End).ToList();
            List<TimeLine> allTimelines = timelines.OrderBy(o => o.End).ToList();
            HashSet<CalendarEvent> alreadySuggested = new HashSet<CalendarEvent>();
            for (int i=0; i< orderedTimelines.Count;i++)
            {
                TimeLine timeLine = orderedTimelines[i];
                BusyTimeLine[] activeSlots = timeLine.OccupiedSlots;
                HashSet<string> caleventIds = new HashSet<string>(activeSlots.Select(activeSlot => new EventID(activeSlot.Id).getAllEventDictionaryLookup));
                List<CalendarEvent> calEvents = new List<CalendarEvent>();

                foreach(string calEventId in caleventIds)
                {
                    if (calEventId_to_CalEvents.ContainsKey(calEventId))
                    {
                        CalendarEvent calEvent = calEventId_to_CalEvents[calEventId];
                        if(MovableCalEvents.Contains(calEvent) && !alreadySuggested.Contains(calEvent))
                        {
                            calEvents.Add(calEvent);
                        }   
                    }
                }
                evaluateCalendarEvents(calEvents);
                List<CalendarEvent> calEventOrderedByScore = calEvents.OrderByDescending(calEvent => calEvent.EventScore).ToList();
                List<TimeLine> possibleTimeLines = allTimelines.Skip(i + 1).ToList();
                foreach (CalendarEvent calEvent in calEventOrderedByScore)
                {
                    if(timeLine.ActiveRatio > activeRatioBound)
                    {
                        TimeLine updatedTimeLine = possibleTimeLines.FirstOrDefault(possibletimeline => possibletimeline.ActiveRatio <= activeRatioBound);
                        if (updatedTimeLine != null)
                        {
                            foreach (BusyTimeLine busyTimeLine in calEvent.ActiveSubEvents.Select(o => o.ActiveSlot))
                            {
                                timeLine.RemoveBusySlots(busyTimeLine);
                            }
                        }
                        else
                        {
                            TimeLine finalTimeLine = allTimelines.Last();
                            TimeSpan totalActiveSpan = TimeSpan.FromTicks(finalTimeLine.OccupiedSlots.Sum(timeLineObj => timeLineObj.TotalActiveSpan.Ticks));// I chose not to use TimeLine.TotalActiveSpan because it conflict timeslots are merged intoone
                            TimeSpan updatedTimeLineTimeSpan = TimeSpan.FromTicks((long)(((double)totalActiveSpan.Ticks) / activeRatioBound));

                            updatedTimeLine = new TimeLine(finalTimeLine.Start, finalTimeLine.Start.Add(updatedTimeLineTimeSpan));

                            long dayIndex = Now.getDayIndexFromStartOfTime(updatedTimeLine.End);
                            // The  extra logic below is to ensure we get an 11:59pm time frame. The extra + 1 is to ensure the situation where a an event has a sleep time of 10:00Am 
                            // This would mean for the day (4/12/2020) the dayTimeline will be from 4/12/2020 10:00Am - 4/12/2020 9:59AM.
                            // Using the above daytimeline and a UTC timezone a call for Now.getClientBeginningOfDay will return 4/12/2020 12:00AM 
                            // This is because given a start time of 4/12/2020 the client which is in UTC will have the same start time 4/12/2020 12:00AM  which is not within the dayTimeline
                            DateTimeOffset dayBeginningOfEndTime = Now.getClientBeginningOfDay(dayIndex + 1);
                            updatedTimeLine = new TimeLine(updatedTimeLine.Start, dayBeginningOfEndTime.AddDays(1).AddMinutes(-1));
                            updatedTimeLine.AddBusySlots(finalTimeLine.OccupiedSlots);
                            var totalTicks = (calEvent.ActiveSubEvents.Sum(o => o.getActiveDuration.Ticks));
                            foreach (BusyTimeLine busyTimeLine in calEvent.ActiveSubEvents.Select(o => o.ActiveSlot))
                            {
                                foreach(var OtherTimelines in orderedTimelines.Skip(i))// updates each timeline busy content
                                {
                                    OtherTimelines.RemoveBusySlots(busyTimeLine);
                                }
                            }
                            allTimelines.Add(updatedTimeLine);
                            possibleTimeLines.Add(updatedTimeLine);
                            
                        }

                        if(updatedTimeLine.End > calEvent.End)
                        {
                            updatedTimeLine.AddBusySlots(calEvent.ActiveSubEvents.Select(o => o.ActiveSlot));
                            suggestion.addCalendarEventAndTimeline(calEvent, updatedTimeLine);
                            alreadySuggested.Add(calEvent);
                        }
                    }
                }
            }
            suggestion.updateDeadlineSuggestions();
            return suggestion;
        }
    }
}
