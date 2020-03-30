using System;
using System.Collections.Generic;
using System.Text;
using TilerElements;
using System.Linq;

namespace ScheduleAnalysis
{
    public class ScheduleSuggestionsAnalysis
    {
        List<SubCalendarEvent> OrderedSubEvents;
        HashSet<CalendarEvent> MovableCalEvents = new HashSet<CalendarEvent>();
        HashSet<CalendarEvent> ReduceableCalEvents = new HashSet<CalendarEvent>();
        HashSet<CalendarEvent> RigidCalEvents = new HashSet<CalendarEvent>();
        Dictionary<string, SubCalendarEvent> subEventId_to_Subevents = new Dictionary<string, SubCalendarEvent>();
        Dictionary<string, CalendarEvent> calEventId_to_CalEvents = new Dictionary<string, CalendarEvent>();
        ILookup<string, SubCalendarEvent> calEventId_to_Subevents;
        ReferenceNow Now;
        TilerUser TilerUser;
        const double activeRatioBound = 0.30;
        public ScheduleSuggestionsAnalysis(IEnumerable<SubCalendarEvent> subEvents, ReferenceNow now, TilerUser tilerUser)
        {
            HashSet<SubCalendarEvent> subEventSet = new HashSet<SubCalendarEvent>(subEvents);
            OrderedSubEvents = subEventSet.OrderBy(o=>o.Start).ThenBy(o=>o.End).ToList();
            subEventId_to_Subevents = new Dictionary<string, SubCalendarEvent>();
            calEventId_to_Subevents = OrderedSubEvents.ToLookup(obj => obj.SubEvent_ID.getAllEventDictionaryLookup.ToString());
            foreach (SubCalendarEvent subEvent in subEventSet)
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
            Now = now;
            TilerUser = tilerUser;
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
                    if (timeLineHistory.TimeLines.Count > 0)
                    {
                        TimeSpan averageTimeSpanHistoryChange = TimeSpan.FromMilliseconds(timeLineHistory.TimeLines.Average(o => o.TimelineSpan.TotalMilliseconds));
                        TimeLine timeLine = calEvent.InitialTimeLine;
                        double totalDays = Math.Round(averageTimeSpanHistoryChange.TotalDays)+1;
                        double eventsPerDay = calEvent.NumberOfSplit / totalDays;
                        featureArgs.Add(eventsPerDay);
                    }
                    else
                    {
                        featureArgs.Add(0);
                    }
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
            if(this.OrderedSubEvents!=null && this.OrderedSubEvents.Count > 0)
            {
                List<SubCalendarEvent> allCompletedSubevents = OrderedSubEvents.Where(o => o.getIsComplete && o.isParentComplete).ToList();
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
        public List<TimeLine> temp_fix(DateTimeOffset currentNow)
        {
            List<TimeLine> retValue = new List<TimeLine>();
            if(this.OrderedSubEvents.Count >0)
            {
                DateTimeOffset lastSubEventDate = this.OrderedSubEvents.Last().End;
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
                List<SubCalendarEvent> subeEventsForProcessing = this.OrderedSubEvents.Where(o => o.Start > weekTimeline.Start).ToList();

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
                List<TimeLine> possibleTimeLines = orderedTimelines.Skip(i + 1).ToList();
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
                            TimeLine finalTimeLine = orderedTimelines.Last();
                            TimeSpan totalActiveSpan = TimeSpan.FromTicks(finalTimeLine.OccupiedSlots.Sum(timeLineObj => timeLineObj.TotalActiveSpan.Ticks));// I chose not to use TimeLine.TotalActiveSpan because it conflict timeslots are merged intoone
                            TimeSpan updatedTimeLineTimeSpan = TimeSpan.FromTicks((long)(((double)totalActiveSpan.Ticks) / activeRatioBound));

                            updatedTimeLine = new TimeLine(finalTimeLine.Start, finalTimeLine.Start.Add(updatedTimeLineTimeSpan));

                            long dayIndex = Now.getDayIndexFromStartOfTime(updatedTimeLine.End);
                            DateTimeOffset dayBeginningOfEndTime = Now.getClientBeginningOfDay(dayIndex);// This  extra logic is to ensure we get an 11:59pm time frame
                            updatedTimeLine = new TimeLine(updatedTimeLine.Start, dayBeginningOfEndTime.AddDays(1).AddMinutes(-1));
                            updatedTimeLine.AddBusySlots(finalTimeLine.OccupiedSlots);
                            var totalTicks = (calEvent.ActiveSubEvents.Sum(o => o.RangeSpan.Ticks));
                            orderedTimelines.Add(updatedTimeLine);
                            possibleTimeLines.Add(updatedTimeLine);
                            foreach (BusyTimeLine busyTimeLine in calEvent.ActiveSubEvents.Select(o => o.ActiveSlot))
                            {
                                timeLine.RemoveBusySlots(busyTimeLine);
                            }
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
