using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    class EfficientTimeLine
    {

        public Dictionary<int, Tuple<DateTime, TimeLine, DateTime, TimeLine>> TimeLineData;
        public TimeLine RestrictingTimeLine;
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
        public EfficientTimeLine(TimeLine RestrictiveTimeLine, Dictionary<SubCalendarEvent, TimeLine> SubCalEvent_RestrictingTimeLine)
        {
            List<SubCalendarEvent> SubCalEventRestricted = SubCalEvent_RestrictingTimeLine.Keys.ToList();
            SubCalEventRestricted=SubCalEventRestricted.OrderBy(obj => obj.End).ToList();
            Dictionary<long, List<SubCalendarEvent>> Dict_DeadlineAndClashingDeadlineSubCalEvents = new Dictionary<long, List<SubCalendarEvent>>();
            foreach (SubCalendarEvent MySubCalEvent in SubCalEventRestricted)
            {

                if (Dict_DeadlineAndClashingDeadlineSubCalEvents.ContainsKey(MySubCalEvent.End.Ticks))
                {
                    Dict_DeadlineAndClashingDeadlineSubCalEvents[MySubCalEvent.End.Ticks].Add(MySubCalEvent);
                }
                else 
                {
                    Dict_DeadlineAndClashingDeadlineSubCalEvents.Add(MySubCalEvent.End.Ticks,new List<SubCalendarEvent>());
                    Dict_DeadlineAndClashingDeadlineSubCalEvents[MySubCalEvent.End.Ticks].Add(MySubCalEvent);
                }
            }

            SubCalEventRestricted = new List<SubCalendarEvent>();
            foreach (long DeadLine in Dict_DeadlineAndClashingDeadlineSubCalEvents.Keys.ToArray())
            {
                Dict_DeadlineAndClashingDeadlineSubCalEvents[DeadLine] = Dict_DeadlineAndClashingDeadlineSubCalEvents[DeadLine].OrderBy(obj => obj.Start).ToList();
                SubCalEventRestricted.AddRange(Dict_DeadlineAndClashingDeadlineSubCalEvents[DeadLine]);
            }

            

            Dictionary<SubCalendarEvent, TimeLine> SubCalEvent_RestrictingTimeLine_Sorted = new Dictionary<SubCalendarEvent, TimeLine>();
            foreach (SubCalendarEvent MySubCalEvent in SubCalEventRestricted)
            {
                SubCalEvent_RestrictingTimeLine_Sorted.Add(MySubCalEvent, SubCalEvent_RestrictingTimeLine[MySubCalEvent]);
            }

            BuildIndex(RestrictiveTimeLine, SubCalEvent_RestrictingTimeLine_Sorted);
            RestrictingTimeLine = RestrictiveTimeLine;
        }

        private void BuildIndex(TimeLine RestrictingTimeLine, Dictionary<SubCalendarEvent, TimeLine> SubCalEvent_RestrictingTimeLine)
        {
            int Index = 0;
            SubCalendarEvent[] ListOfSortedSubCalEvents = SubCalEvent_RestrictingTimeLine.Keys.ToArray();
            TimeLineData.Add(Index, new Tuple<DateTime,TimeLine,DateTime,TimeLine>(RestrictingTimeLine.Start,new TimeLine(RestrictingTimeLine.Start,RestrictingTimeLine.Start), ListOfSortedSubCalEvents[Index].Start,SubCalEvent_RestrictingTimeLine[ListOfSortedSubCalEvents[Index]]));
            for (; Index < ListOfSortedSubCalEvents.Length-1; Index++)
            {
                TimeLineData.Add(Index+1, new Tuple<DateTime, TimeLine, DateTime, TimeLine>(ListOfSortedSubCalEvents[Index].End, SubCalEvent_RestrictingTimeLine[ListOfSortedSubCalEvents[Index]], ListOfSortedSubCalEvents[Index+1].Start, SubCalEvent_RestrictingTimeLine[ListOfSortedSubCalEvents[Index+1]])); 
            }
            TimeLineData.Add(Index + 1, new Tuple<DateTime, TimeLine, DateTime, TimeLine>(ListOfSortedSubCalEvents[Index].End, SubCalEvent_RestrictingTimeLine[ListOfSortedSubCalEvents[Index]], RestrictingTimeLine.End,new TimeLine(RestrictingTimeLine.End,RestrictingTimeLine.End)  )); 
        }



        public TimeSpan getIndex(int Index)
        {
            return (TimeLineData[Index].Item3 - TimeLineData[Index].Item1);
        }





    }
}
