using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TilerElements.Wpf;

namespace My24HourTimerWPF
{
    class InListOutOfList
    {


        Dictionary<List<List<SubCalendarEvent>>, List<SubCalendarEvent>> DictData;
        Dictionary<List<TimeLine>, List<SubCalendarEvent>> DictData_TimeLine;
        public InListOutOfList(List<SubCalendarEvent> FullList, List<List<List<SubCalendarEvent>>> AllMyList)
        {
            DictData = new Dictionary<List<List<SubCalendarEvent>>, List<SubCalendarEvent>>();
            foreach (List<List<SubCalendarEvent>> ListToCheck in AllMyList)
            {
                List<SubCalendarEvent> TotalList = new List<SubCalendarEvent>();
                foreach(List<SubCalendarEvent> myList in ListToCheck)
                {
                    TotalList.AddRange(myList);
                }

                DictData.Add(ListToCheck, Utility.NotInList_NoEffect(FullList, TotalList));
            }
        }

        public InListOutOfList(List<SubCalendarEvent> FullList, List<List<List<SubCalendarEvent>>> AllMyList, List<TimeLine> MyFreeSpots)
        {
            DictData_TimeLine = new Dictionary<List<TimeLine>, List<SubCalendarEvent>>();
            foreach (List<List<SubCalendarEvent>> ListToCheck in AllMyList)
            {
                List<SubCalendarEvent> TotalList = new List<SubCalendarEvent>();
                int i = 0;
                List<TimeLine> timeLineEntry = new List<TimeLine>();
                foreach (List<SubCalendarEvent> myList in ListToCheck)
                {
                    TimeLine myTimeLine = new TimeLine(MyFreeSpots[i].Start, MyFreeSpots[i].End);
                    TotalList.AddRange(myList);

                    TimeSpan TotalTimeSpan = new TimeSpan(0);
                    foreach (SubCalendarEvent mySubEvent in myList)
                    {
                        TotalTimeSpan=TotalTimeSpan.Add(mySubEvent.ActiveSlot.BusyTimeSpan);
                    }

                    BusyTimeLine EffectivebusySlot = new BusyTimeLine("1000000_1000001", MyFreeSpots[i].Start, MyFreeSpots[i].Start.Add(TotalTimeSpan));
                    myTimeLine.AddBusySlots(EffectivebusySlot);
                    timeLineEntry.Add(myTimeLine);
                    i++;
                }

                DictData_TimeLine.Add(timeLineEntry, Utility.NotInList_NoEffect(FullList, TotalList));
            }
        }
    }
}
