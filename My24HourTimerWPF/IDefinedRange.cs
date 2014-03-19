using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    interface IDefinedRange
    {
        DateTime Start
        {
              get;
        }
        DateTime End
        {
            get;
        }

        TimeLine RangeTimeLine
        {
            get;
        }

        

       bool IsDateTimeWithin(DateTime DateTimeEntry);
    }
}
