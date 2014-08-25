using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class TimeLineWithEdgeElements:BusyTimeLine
    {
        string startingEventID;
        string endingEventID;
        public TimeLineWithEdgeElements():base()
        { 
        
        }
        public TimeLineWithEdgeElements(DateTime start, DateTime End, string StartingEdgeEleemnt, string EndingEdgeElement)
        {
            StartTime = start;
            EndTime = End;
            startingEventID = StartingEdgeEleemnt;
            endingEventID = EndingEdgeElement;
        }
        
    }
}
