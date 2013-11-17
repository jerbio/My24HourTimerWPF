using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    public class TimeSpanWithID
    {
        public TimeSpan MyTimeSpan;
        public EventID TimeSpanID;
        public TimeSpanWithID(TimeSpan TimeSpanEntry, EventID ID)
        {
            MyTimeSpan = TimeSpanEntry;
            TimeSpanID = ID;
        }
        public TimeSpanWithID(long ticks,EventID ID)
        {
            MyTimeSpan=new TimeSpan(ticks);
            TimeSpanID = ID;
        }
        public TimeSpanWithID(int hours, int minutes, int seconds,EventID ID)
        {
            MyTimeSpan = new TimeSpan(hours,  minutes,  seconds);
            TimeSpanID = new EventID(ID.ToString());
        }
        
        public TimeSpanWithID(int days, int hours, int minutes, int seconds, EventID ID)
        {
            MyTimeSpan = new TimeSpan(days,hours, minutes, seconds);
            TimeSpanID = new EventID(ID.ToString());
        }
        
        public TimeSpanWithID(int days, int hours, int minutes, int seconds, int milliseconds, EventID ID)
        {
            MyTimeSpan = new TimeSpan(days, hours, minutes, seconds);
            TimeSpanID = new EventID(ID.ToString());
        }
        
    }
}
