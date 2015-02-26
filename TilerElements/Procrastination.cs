using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    /// <summary>
    /// Reperesents the Procrastination parameters
    /// </summary>
    public class Procrastination
    {
        DateTimeOffset FromTime;
        TimeSpan Duration;
        DateTimeOffset BeginTIme;

        public Procrastination(DateTimeOffset From, TimeSpan Span)
        {
            FromTime = From;
            Duration = Span;
            BeginTIme = FromTime.Add(Duration);
        }

        public DateTimeOffset MinimumStartTime
        {
            get
            {
                return BeginTIme;
            }
        }

        public DayOfWeek DislikedDayOfWeek
        { 
            get
            {
                return FromTime.DayOfWeek;
            }
        }

        public Procrastination CreateCopy()
        {
            Procrastination retValue = new Procrastination(this.FromTime,this.Duration);
            return retValue ;
        }

    }
}
