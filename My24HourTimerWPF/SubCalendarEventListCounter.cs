using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    class SubCalendarEventListCounter
    {
        List<SubCalendarEvent> ListOfSubCalEvents;
        int CurrentIndex;
        string ParentID;

        public SubCalendarEventListCounter(SubCalendarEvent StartingSubCalendarEvent, string ParentID)
        {
            this.ParentID = ParentID;
            ListOfSubCalEvents = new List<SubCalendarEvent>() { StartingSubCalendarEvent };
            CurrentIndex = 0;
        }

        public void reset()
        {
                CurrentIndex = 0;
                return;
        }

        public SubCalendarEvent getNextSubCalendarEvent
        {
            get 
            {
                return ListOfSubCalEvents[CurrentIndex++];
            }
        }

        public SubCalendarEvent UpdateList
        {
            set 
            {
                ListOfSubCalEvents.Add(value);
            }
        }
    
    }
}
