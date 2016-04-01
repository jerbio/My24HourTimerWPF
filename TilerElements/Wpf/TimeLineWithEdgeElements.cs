using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements.Wpf
{
    public class TimeLineWithEdgeElements:BusyTimeLine
    {
        string startingEventID;
        string endingEventID;
        public TimeLineWithEdgeElements():base()
        { 
        
        }
        public TimeLineWithEdgeElements(DateTimeOffset start, DateTimeOffset End, string StartingEdgeEleemnt, string EndingEdgeElement)
        {
            StartTime = start;
            EndTime = End;
            startingEventID = StartingEdgeEleemnt;
            endingEventID = EndingEdgeElement;
        }
        
    }
}
