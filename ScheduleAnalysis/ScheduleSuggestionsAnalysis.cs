using System;
using System.Collections.Generic;
using System.Text;
using TilerElements;
using System.Linq;

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
        TilerUser TilerUser;
        static readonly double DefaultActivationRatio = 0.30;
        static readonly double MaxActivationRatio = 0.75;
        double activeRatioBound;// The active ratio limit for when over scheduling is assumed to occur
        public ScheduleSuggestionsAnalysis(IEnumerable<SubCalendarEvent> subEvents, ReferenceNow now, TilerUser tilerUser, Analysis analysis = null)
        {
            HashSet<SubCalendarEvent> allSubEvent = new HashSet<SubCalendarEvent>(subEvents);
            HashSet<SubCalendarEvent> subEventsForEvaluation = new HashSet<SubCalendarEvent>(allSubEvent.Where(subEvent => subEvent.isActive && subEvent.getActiveDuration <= Utility.LeastAllDaySubeventDuration));
            OrderedActiveSubEvents = subEventsForEvaluation.OrderBy(o=>o.Start).ThenBy(o=>o.End).ToList();
            subEventId_to_Subevents = new Dictionary<string, SubCalendarEvent>();
            activeRatioBound = analysis?.CompletionRate ?? DefaultActivationRatio;
            calEventId_to_Subevents = OrderedActiveSubEvents.ToLookup(obj => obj.SubEvent_ID.getAllEventDictionaryLookup.ToString());
            foreach (SubCalendarEvent subEvent in subEventsForEvaluation)
            {
                subEventId_to_Subevents.Add(subEvent.Id, subEvent);
                if(!calEventId_to_CalEvents.ContainsKey(subEvent.SubEvent_ID.getAllEventDictionaryLookup))
                {
                    subEvent.ParentCalendarEvent.resetAllSuggestions();
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
                (lastFourWeekTimeLine.IsTimeLineWithin(subEvent.StartToEnd) && subEvent.isRigid) ||
                ((subEvent.getIsComplete && (lastFourWeekTimeLine.IsDateTimeWithin(subEvent.CompletionTime) || lastFourWeekTimeLine.IsTimeLineWithin(subEvent))))).OrderBy(o=>o.Start).ToList();

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
                TimeSpan totalWeekSpan= TimeSpan.FromTicks(validWeeks.Sum(o => o.TimelineSpan.Ticks));

                activeRatioBound = totalCompletedSpan.TotalHours / totalWeekSpan.TotalHours;
                if(activeRatioBound> MaxActivationRatio)
                {
                    activeRatioBound = MaxActivationRatio;
                }
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
                DayOfWeek dayOfWeek = dayOfWeek_timeline.Item1;
                int currentDayOfWeekIndex = (int)dayOfWeek;
                int beginningOfWeekIndex = (int)TilerUser.BeginningOfWeek;
                int endOfWeekIndex = ((beginningOfWeekIndex - 1) + numberOfDaysInWeek) % numberOfDaysInWeek;

                int dayDiff = (endOfWeekIndex - currentDayOfWeekIndex);
                if (dayDiff <= 0)
                {
                    dayDiff += numberOfDaysInWeek;
                }

                List<TimeLine> eachWeekTimeline = new List<TimeLine>();
                List<TimeLine> compoundedWeekTimelines = new List<TimeLine>();

                DateTimeOffset timeLineStart = currentNow;
                DateTimeOffset timeLineEnd = timeLineStart.AddDays(dayDiff);

                TimeLine timeline = new TimeLine(timeLineStart, timeLineEnd);

                TimeLine weekTimeline = timeline.StartToEnd;
                TimeLine compoundTimeline = weekTimeline.StartToEnd;
                eachWeekTimeline.Add(weekTimeline);
                compoundedWeekTimelines.Add(weekTimeline);
                List<SubCalendarEvent> subeEventsForProcessing = this.OrderedActiveSubEvents.Where(o => o.Start > weekTimeline.Start).ToList();

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
                            timeLineEnd = timeLineStart.AddDays(7);
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


        public ScheduleSuggestion suggestScheduleChange(List<TimeLine> timelines )
        {
            ScheduleSuggestion suggestion = new ScheduleSuggestion();
            List<TimeLine> orderedTimelines = timelines.OrderBy(o => o.End).ToList();
            List<TimeLine> allTimelines = timelines.OrderBy(o => o.End).ToList();
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
                        if(MovableCalEvents.Contains(calEvent))
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
                            var totalTicks = (calEvent.ActiveSubEvents.Sum(o => o.RangeSpan.Ticks));
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
                        }
                        
                    }
                }
            }
            suggestion.updateDeadlineSuggestions();
            return suggestion;
        }
    }
}
