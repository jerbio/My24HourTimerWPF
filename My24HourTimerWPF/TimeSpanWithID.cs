using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    public class TimeSpanWithEventID
    {
        public TimeSpan timeSpan;
        public EventID TimeSpanID;
        public TimeSpanWithEventID(TimeSpan TimeSpanEntry, EventID ID)
        {
            timeSpan = TimeSpanEntry;
            TimeSpanID = ID;
        }
        public TimeSpanWithEventID(long ticks,EventID ID)
        {
            timeSpan=new TimeSpan(ticks);
            TimeSpanID = ID;
        }
        public TimeSpanWithEventID(int hours, int minutes, int seconds,EventID ID)
        {
            timeSpan = new TimeSpan(hours,  minutes,  seconds);
            TimeSpanID = new EventID(ID.ToString());
        }
        
        public TimeSpanWithEventID(int days, int hours, int minutes, int seconds, EventID ID)
        {
            timeSpan = new TimeSpan(days,hours, minutes, seconds);
            TimeSpanID = new EventID(ID.ToString());
        }
        
        public TimeSpanWithEventID(int days, int hours, int minutes, int seconds, int milliseconds, EventID ID)
        {
            timeSpan = new TimeSpan(days, hours, minutes, seconds);
            TimeSpanID = new EventID(ID.ToString());
        }
        
    }


    public class TimeSpanWithStringID
    {
        public TimeSpan timeSpan;
        public string ID;
        
        public TimeSpanWithStringID(TimeSpan Arg1, string Arg2)
        {
            timeSpan = Arg1;
            ID = Arg2;
        }
    }
  
}
