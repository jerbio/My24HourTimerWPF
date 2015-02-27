using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class SubCalendarEventRestricted : SubCalendarEvent
    {
        static TimeSpan ZeroTimeSpan = new TimeSpan(0);
        TimeLine HardCalendarEventRange;//this does not include the restriction
        RestrictionProfile ProfileOfRestriction;
        #region Constructor
        public SubCalendarEventRestricted(string CalEventID, DateTimeOffset Start, DateTimeOffset End, RestrictionProfile constrictionProgile, TimeLine HardCalEventTimeRange, bool isEnabled, bool isComplete, ConflictProfile conflictingEvents)
        { 
            isRestricted =true;
            StartDateTime = Start;
            EndDateTime = End;
            EventDuration = EndDateTime - StartDateTime;
            UniqueID = EventID.GenerateSubCalendarEvent(CalEventID);
            ProfileOfRestriction = constrictionProgile;
            HardCalendarEventRange = HardCalEventTimeRange;
            initializeCalendarEventRange(HardCalendarEventRange);
            
        }
        #endregion

        #region Functions
        


        public IEnumerable<TimeLine> getFeasibleTimeLines(TimeLine TimeLineEntry)
        {
            return ProfileOfRestriction.getAllTimePossibleTimeFrames(TimeLineEntry);
        }


        public override bool PinToEnd(TimeLine LimitingTimeLine)
        {
            TimeLine RestrictedLimitingFrame = ProfileOfRestriction.getLatestActiveTimeFrameBeforeEnd(LimitingTimeLine);
            if(RestrictedLimitingFrame.RangeSpan<ActiveDuration)
            {
                RestrictedLimitingFrame = ProfileOfRestriction.getLatestFullFrame(LimitingTimeLine);
            }
            return base.PinToEnd(RestrictedLimitingFrame);
        }

        public override bool PinToStart(TimeLine MyTimeLine)
        {
            TimeLine RestrictedLimitingFrame = ProfileOfRestriction.getEarliestActiveFrameAfterBeginning(MyTimeLine);
            if (RestrictedLimitingFrame.RangeSpan < ActiveDuration)
            {
                RestrictedLimitingFrame = ProfileOfRestriction.getEarliestFullframe(MyTimeLine);
            }
            return base.PinToStart(RestrictedLimitingFrame);
        }
        /// <summary>
        /// Function initializes the CalendarEventRange. CalendarEventRange is the range for the calendar event. Since this is the restricted class then it sets the timeline to use the earliest possible Start Time and latest possible Datetime to set the rangetimeline.
        /// </summary>
        /// <param name="refTimeLine"></param>
        void initializeCalendarEventRange(TimeLine refTimeLine=null)
        {
            if (refTimeLine == null)
            {
                refTimeLine = HardCalendarEventRange;
            }

            DateTimeOffset myStart = ProfileOfRestriction.getEarliestStartTimeWithinAFrameAfterRefTime(refTimeLine.Start);
            DateTimeOffset myEnd = ProfileOfRestriction.getLatestEndTimeWithinFrameBeforeRefTime(refTimeLine.End);
            CalendarEventRange = new TimeLine(myStart, myEnd);
        }

        
        #endregion   
    }
}
