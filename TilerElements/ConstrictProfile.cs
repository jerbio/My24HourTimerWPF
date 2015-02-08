using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class ConstrictProfile
    {
        DateTimeOffset StartTime;
        TimeSpan SpanOfConstriction;
        static TimeSpan ZeroTimeSpan = new TimeSpan(0);
        int[] DaySelection = new int[7];
        bool isSet;


        public ConstrictProfile()
        {
            StartTime = new DateTimeOffset();
            SpanOfConstriction = new TimeSpan();
            isSet = false;
        }

        public ConstrictProfile(DateTimeOffset Start, DateTimeOffset End)
        {
            Start =  new DateTimeOffset(1,1,1,Start.Hour,Start.Minute,Start.Second,new TimeSpan());
            End = new DateTimeOffset(1, 1, 1, End.Hour, End.Minute, End.Second, new TimeSpan());

            TimeSpan Span = End - Start;
            if ((Span <= new TimeSpan(1, 0, 0, 0)) && (Span >= ZeroTimeSpan))
            {
                StartTime = Start;
                SpanOfConstriction =Span;
                isSet = true;
                if ((new TimeSpan(0)).Ticks == 0)
                {
                    isSet = false;
                }
            }
            else
            {
                throw new Exception("SpanOfConstriction is invalid span is less than zerospan or greater than twenty four hours");
            }

            
        }

        public ConstrictProfile(DateTimeOffset Start, TimeSpan Span)
        {
            if ((Span <= new TimeSpan(1, 0, 0, 0)) && (Span >= ZeroTimeSpan))
            {
                StartTime = Start;
                SpanOfConstriction = Span;
                isSet = true;
                if ((new TimeSpan(0)).Ticks == 0)
                {
                    isSet = false;
                }
            }
            else
            {
                throw new Exception("SpanOfConstriction is invalid span is less than zerospan or greater than twenty four hours");
            }
        }
    }
}
