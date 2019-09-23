using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    /// <summary>
    /// Interface describes an element that provides a start and end, together with a way to simply gets its appropraite RangeTimeline
    /// </summary>
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

        TimeLine StartToEnd
        {
            get;
        }
        bool IsDateTimeWithin(DateTimeOffset DateTimeEntry);
    }
}
