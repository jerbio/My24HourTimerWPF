using System;
using System.Collections.Generic;
using System.Text;
using TilerElements;
using System.Linq;

namespace ScheduleAnalysis
{
    public class DeadlineForeCast
    {
        List<SubCalendarEvent> OrderedSubEvents;
        ReferenceNow Now;
        TilerUser TilerUser;
        public DeadlineForeCast(IEnumerable<SubCalendarEvent> subEvents, ReferenceNow now, TilerUser tilerUser)
        {
            
            OrderedSubEvents = subEvents.OrderBy(o=>o.Start).ThenBy(o=>o.End).ToList();
            Now = now;
            TilerUser = tilerUser;
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
        public void temp_fix(DateTimeOffset currentNow)
        {
            if(this.OrderedSubEvents.Count >0)
            {
                DateTimeOffset lastSubEventDate = this.OrderedSubEvents.Last().End;
                const int numberOfDaysInWeek = 7;
                const double activeRatioBound = 0.65;
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
                List<SubCalendarEvent> subeEVentsForProcessing = this.OrderedSubEvents.Where(o => o.Start > weekTimeline.Start).ToList();

                for (int i = 0; i>=0 && i< subeEVentsForProcessing.Count;i++)
                {
                    SubCalendarEvent subEvent = this.OrderedSubEvents[i];
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
                            compoundTimeline = new TimeLine(currentNow, weekTimeline.End);
                            eachWeekTimeline.Add(weekTimeline);
                            compoundedWeekTimelines.Add(compoundTimeline);
                            if (weekTimeline.doesTimeLineInterfere(subevent_startToEnd))
                            {
                                weekTimeline.AddBusySlots(subEvent.ActiveSlot);
                                compoundTimeline.AddBusySlots(subEvent.ActiveSlot);
                                compoundTimeline.AddBusySlots(subeEVentsForProcessing.Take(i).Select(o => o.ActiveSlot));
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

                double activeRatio = weekTimeline.ActiveRatio;
                if (activeRatio >= activeRatioBound)
                {

                }
            }
            


        }
    }
}
