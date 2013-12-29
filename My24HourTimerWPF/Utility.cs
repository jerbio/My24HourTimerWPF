using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    class Utility
    {
        static Utility()
        { 
        
        }

        public static List<SubCalendarEvent> sortSubCalEventByDeadline(List<SubCalendarEvent> SubCalEventRestricted, bool SecondSortByStartDate)
        {
            
            SubCalEventRestricted = SubCalEventRestricted.OrderBy(obj => obj.End).ToList();
            Dictionary<long, List<SubCalendarEvent>> Dict_DeadlineAndClashingDeadlineSubCalEvents = new Dictionary<long, List<SubCalendarEvent>>();
            foreach (SubCalendarEvent MySubCalEvent in SubCalEventRestricted)
            {

                if (Dict_DeadlineAndClashingDeadlineSubCalEvents.ContainsKey(MySubCalEvent.End.Ticks))
                {
                    Dict_DeadlineAndClashingDeadlineSubCalEvents[MySubCalEvent.End.Ticks].Add(MySubCalEvent);
                }
                else
                {
                    Dict_DeadlineAndClashingDeadlineSubCalEvents.Add(MySubCalEvent.End.Ticks, new List<SubCalendarEvent>());
                    Dict_DeadlineAndClashingDeadlineSubCalEvents[MySubCalEvent.End.Ticks].Add(MySubCalEvent);
                }
            }


            SubCalEventRestricted = new List<SubCalendarEvent>();
            foreach (long DeadLine in Dict_DeadlineAndClashingDeadlineSubCalEvents.Keys.ToArray())
            {
                Dict_DeadlineAndClashingDeadlineSubCalEvents[DeadLine] = Dict_DeadlineAndClashingDeadlineSubCalEvents[DeadLine].OrderBy(obj => obj.Start).ToList();
                if (!SecondSortByStartDate)
                { Dict_DeadlineAndClashingDeadlineSubCalEvents[DeadLine].Reverse(); }
                SubCalEventRestricted.AddRange(Dict_DeadlineAndClashingDeadlineSubCalEvents[DeadLine]);
            }


            
            return SubCalEventRestricted;
        }


        public static List<SubCalendarEvent> NotInList(List<SubCalendarEvent> ListToCheck, List<SubCalendarEvent> MyCurrentList)
        {
            foreach (SubCalendarEvent MySubCalendarEvent in MyCurrentList)
            {
                ListToCheck.Remove(MySubCalendarEvent);
            }
            return ListToCheck;
        }


        public static List<SubCalendarEvent> NotInList_NoEffect(List<SubCalendarEvent> ListToCheck, List<SubCalendarEvent> MyCurrentList)
        {
            List<SubCalendarEvent> ListToCheck_Cpy = ListToCheck.ToList();
            
            foreach (SubCalendarEvent MySubCalendarEvent in MyCurrentList)
            {
                ListToCheck_Cpy.Remove(MySubCalendarEvent);
            }
            return ListToCheck_Cpy;
        }


        public static List<SubCalendarEvent> ListPartOfList(List<SubCalendarEvent> ListToCheck, List<SubCalendarEvent> MyCurrentList)
        {
            List<SubCalendarEvent> InListElements = new List<SubCalendarEvent>();

            foreach (SubCalendarEvent MySubCalendarEvent0 in ListToCheck)
            {
                foreach (SubCalendarEvent MySubCalendarEvent1 in MyCurrentList)
                {
                    if (MySubCalendarEvent1.ID == MySubCalendarEvent0.ID)
                    {
                        InListElements.Add(MySubCalendarEvent1);
                    }
                }
            }
            return InListElements;
        }
    }
}
