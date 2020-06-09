using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    /// <summary>
    /// Class is a timeline in which the Id of the events before and after its time line are provided so assuming we have A -> {9 am-11 am, Id = 1222_5664} and B -> {12p am-1 pm, Id = 9999_5665}. 
    /// This object will be a timeline between A and B so 'this'  -> {11a am- 12 pm, startng = 1222_5664 ending = 9999_5665}. 
    /// </summary>
    public class TimeLineWithEdgeElements:BusyTimeLine
    {
        string _StartingEventID;
        string _EndingEventID;
        public TimeLineWithEdgeElements():base()
        { 
        
        }
        public TimeLineWithEdgeElements(DateTimeOffset start, DateTimeOffset End, string StartingEdgeEleemnt, string EndingEdgeElement)
        {
            StartTime = start;
            EndTime = End;
            _StartingEventID = StartingEdgeEleemnt;
            _EndingEventID = EndingEdgeElement;
        }

        public virtual string BeginningEventId {
            get
            {
                return _StartingEventID;
            }
        }
        public virtual string EndingEventId
        {
            get
            {
                return _EndingEventID;
            }
        }
    }
}
