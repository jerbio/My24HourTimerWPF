using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements.Wpf
{
    public interface IDefinedRange
    {
        DateTimeOffset Start
        {
              get;
        }
        DateTimeOffset End
        {
            get;
        }

        TimeLine RangeTimeLine
        {
            get;
        }

        

       bool IsDateTimeWithin(DateTimeOffset DateTimeEntry);
    }
}
