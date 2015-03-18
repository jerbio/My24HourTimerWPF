﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{

    /// <summary>
    /// represents the Timeline of restriction
    /// </summary>
    public class RestrictionTimeLine
    {
        protected DateTimeOffset StartTimeOfDay;
        protected TimeSpan RangeTimeSpan;
        protected DateTimeOffset EndTimeOfDay;

        protected RestrictionTimeLine()
        {
            StartTimeOfDay = new DateTimeOffset();
            RangeTimeSpan = new TimeSpan();
            EndTimeOfDay = new DateTimeOffset();
        }

        public RestrictionTimeLine(DateTimeOffset Start, DateTimeOffset End)
        {
            StartTimeOfDay = new DateTimeOffset(1, 1, 1, Start.Hour, Start.Minute, Start.Second, new TimeSpan());
            End = End <= Start ? End.AddDays(1) : End;
            RangeTimeSpan = End - Start;
            EndTimeOfDay = StartTimeOfDay.Add(RangeTimeSpan);
        }


        public RestrictionTimeLine(DateTimeOffset Start, TimeSpan SpanDuration)
        {
            StartTimeOfDay = new DateTimeOffset(1, 1, 1, Start.Hour, Start.Minute, Start.Second, new TimeSpan());
            RangeTimeSpan = SpanDuration;
            EndTimeOfDay = StartTimeOfDay.Add(RangeTimeSpan);
        }

        public DateTimeOffset getInjectedStartHourMinIntoDateTime(DateTimeOffset refDateTimeOffset)
        {
            DateTimeOffset retValue = new DateTimeOffset(refDateTimeOffset.Year, refDateTimeOffset.Month, refDateTimeOffset.Day, StartTimeOfDay.Hour, StartTimeOfDay.Minute, StartTimeOfDay.Second, new TimeSpan());
            return retValue;
        }

        public DateTimeOffset getInjectedEndHourMinIntoDateTime(DateTimeOffset refDateTimeOffset)
        {
            DateTimeOffset retValue = new DateTimeOffset(refDateTimeOffset.Year, refDateTimeOffset.Month, refDateTimeOffset.Day, StartTimeOfDay.Hour, StartTimeOfDay.Minute, StartTimeOfDay.Second, new TimeSpan()).Add(RangeTimeSpan);
            return retValue;
        }

        public RestrictionTimeLine createCopy()
        {
            RestrictionTimeLine retValue = new RestrictionTimeLine();
            retValue.EndTimeOfDay = this.EndTimeOfDay;
            retValue.RangeTimeSpan = this.RangeTimeSpan;
            retValue.StartTimeOfDay = this.StartTimeOfDay;
            return retValue;
        }

        public TimeLine getTimeLineFromStartFrame(DateTimeOffset Start)
        {
            Start = getInjectedStartHourMinIntoDateTime(Start);
            TimeLine retValue = new TimeLine(Start ,Start .Add(RangeTimeSpan));
            return retValue;
        }

        public TimeLine getTimelineFromEndFrame(DateTimeOffset End)
        { 
            if((End.Hour==EndTimeOfDay.Hour)&&(End.Minute==EndTimeOfDay.Minute))//checks if the the Hour and Time in End parameter matches the end of Object. This avoids errors that might occur with using invalid entimes that dont match the restriction timeline
            {
                TimeLine retValue = new TimeLine(End.Add(-RangeTimeSpan), End);
                return retValue;
            }
            throw new Exception("Invalid End Datetimeoffset used for restriction timeline");
        }

        public DateTimeOffset getSampleTestTIme()
        {
            return StartTimeOfDay;
        }



        #region Properties
        public DateTimeOffset Start
        {
            get 
            {
                return StartTimeOfDay;
            }
        }

        public TimeSpan Span
        {
            get
            {
                return RangeTimeSpan;
            }
        }

        public DateTimeOffset End
        {
            get
            {
                return EndTimeOfDay;
            }
        }
        #endregion
    }
}
