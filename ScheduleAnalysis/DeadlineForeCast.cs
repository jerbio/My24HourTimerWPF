using System;
using System.Collections.Generic;
using System.Text;
using TilerElements;
using System.Linq;

namespace ScheduleAnalysis
{
    public class DeadlineForeCast
    {
        List<SubCalendarEvent> SubEvents;
        ReferenceNow Now;
        public DeadlineForeCast(IEnumerable<SubCalendarEvent> subEvents, ReferenceNow now)
        {
            SubEvents = subEvents.ToList();
            Now = now;
        }

        public void Analyze()
        {
            if(this.SubEvents!=null && this.SubEvents.Count > 0)
            {
                List<SubCalendarEvent> allCompletedSubevents = SubEvents.Where(o => o.getIsComplete && o.isParentComplete).ToList();
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
    }
}
