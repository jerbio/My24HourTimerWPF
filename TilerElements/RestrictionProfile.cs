using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    /// <summary>
    /// Reperesents the restriction of profile. This is to be used with events. There is no to be a default constructor for semantic purposes
    /// </summary>
    public class RestrictionProfile
    {
        static DayOfWeek[] AllDaysOfWeek = { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
        Tuple<DayOfWeek, RestrictionTimeLine>[] DaySelection = new Tuple<DayOfWeek, RestrictionTimeLine>[7];
        Tuple<DayOfWeek, RestrictionTimeLine>[] NoNull_DaySelections;
        
        bool[] ActiveDays = new bool[7];
        DayOfWeek startDayOfWeek = DayOfWeek.Monday;
        
        RestrictionProfile()
        {

        }
        public RestrictionProfile(DateTimeOffset RestrictStart, TimeSpan RestrictDuration)
        {
            DaySelection = new Tuple<DayOfWeek, RestrictionTimeLine>[7];
            for (int i = 0; i < DaySelection.Length; i++)
            {
                DaySelection[i] = new Tuple<DayOfWeek, RestrictionTimeLine>(AllDaysOfWeek[i], new RestrictionTimeLine(RestrictStart, RestrictDuration));
                ActiveDays[i] = true;
            }
            NoNull_DaySelections = DaySelection.ToArray();
        }

        public RestrictionProfile(IEnumerable<DayOfWeek> DaysOfWeekSelection, RestrictionTimeLine constrictionProgile)
        {
            DaysOfWeekSelection = DaysOfWeekSelection.OrderBy(obj=>obj).ToList();
            startDayOfWeek =((DayOfWeek[]) DaysOfWeekSelection)[0];
            foreach (DayOfWeek eachDayOfWeek in DaysOfWeekSelection)
            {
                int DayOfWeekInt = (int)eachDayOfWeek;
                DaySelection[DayOfWeekInt] = new Tuple<DayOfWeek, RestrictionTimeLine>(eachDayOfWeek, constrictionProgile);
            }

            NoNull_DaySelections = DaySelection.Where(obj => obj != null).ToArray();
        }


        public RestrictionProfile createCopy()
        {
            RestrictionProfile retValue = new RestrictionProfile();
            retValue.DaySelection = this.DaySelection.Select(obj => obj == null ? obj : new Tuple<DayOfWeek, RestrictionTimeLine>(obj.Item1, obj.Item2.createCopy())).ToArray();
            retValue.NoNull_DaySelections = this.DaySelection.Select(obj => new Tuple<DayOfWeek, RestrictionTimeLine>(obj.Item1, obj.Item2.createCopy())).ToArray();
            return retValue;
        }

        /// <summary>
        /// Function gets you the widest timeline for which the timeLine boundaries fall within the active time frames
        /// </summary>
        /// <param name="RefTimeline"></param>
        /// <returns></returns>
        public TimeLine getWidestTimeLineInterSectingWithFrame(TimeLine RefTimeline)
        {
            DateTimeOffset Start = RefTimeline.Start;
            Start =  getEarliestStartTimeWithinAFrameAfterRefTime(Start);

            DateTimeOffset End = RefTimeline.End;
            End = getLatestEndTimeWithinFrameBeforeRefTime(End);

            TimeLine retValue = new TimeLine(Start, End);
            return retValue;
        }

        public TimeLine getLatestActiveTimeFrameBeforeEnd(TimeLine RefTimeline)
        {
            DateTimeOffset End = getLatestEndTimeWithinFrameBeforeRefTime(RefTimeline.End);
            DateTimeOffset Start = getLatestEndTimeFrameBorder(RefTimeline.End);
            Start = getEarliestStartTimeFrameBorder(Start);
            Start = RefTimeline.Start>Start?RefTimeline.Start :Start;

            TimeLine retValue = new TimeLine(Start, End);
            return retValue;
        }

        public TimeLine getEarliestActiveFrameAfterBeginning(TimeLine RefTimeline)
        {
            DateTimeOffset Start = getEarliestStartTimeWithinAFrameAfterRefTime(RefTimeline.Start);
            DateTimeOffset End = getEarliestStartTimeFrameBorder(RefTimeline.Start);
            End = getLatestEndTimeFrameBorder(End);
            End = RefTimeline.End < End ? RefTimeline.End : End;

            TimeLine retValue = new TimeLine(Start, End);
            return retValue;
        }

        /// <summary>
        /// Gets the First full frame bordered by the beginning of RefTimeline. Note it does not check for fitability
        /// </summary>
        /// <param name="RefTimeline"></param>
        /// <returns></returns>

        public TimeLine getEarliestFullframe(TimeLine RefTimeline)
        {
            DateTimeOffset Start = getEarliestStartTimeFrameBorder(RefTimeline.Start);
            TimeLine retValue = DaySelection[Start.Day].Item2.getTimeLineFromStartFrame(Start);
            return retValue;
        }
        /// <summary>
        /// gets the last full restrictive frame bordered by the end of the timeline. It does not check if the full frame fits.
        /// </summary>
        /// <param name="RefTimeLine"></param>
        /// <returns></returns>
        public TimeLine getLatestFullFrame(TimeLine RefTimeLine)
        {
            DateTimeOffset End = getLatestEndTimeFrameBorder(RefTimeLine.End);
            TimeLine retValue = DaySelection[End.Day].Item2.getTimeLineFromStartFrame(End);
            return retValue;
        }

        /// <summary>
        /// Function gets all the available Restriction frames within a timeLine
        /// </summary>
        /// <param name="RefTimeLine"></param>
        /// <returns></returns>

        public List<TimeLine> getAllTimePossibleTimeFrames(TimeLine RefTimeLine)
        {
            TimeLine FirstFrame = getEarliestActiveFrameAfterBeginning(RefTimeLine);
            TimeLine LastFrame = getLatestActiveTimeFrameBeforeEnd(RefTimeLine);

            List<TimeLine>[] TimeLinesPerDaySelection = NoNull_DaySelections.Select(obj=>new List<TimeLine>()).ToArray();

            for( int i= 0; i< NoNull_DaySelections.Length;i++)
            {
                Tuple<DayOfWeek, RestrictionTimeLine> eachTuple = NoNull_DaySelections[i];
                List<TimeLine> ListOfTimeLine = TimeLinesPerDaySelection[i];
                int DayDiff = ((FirstFrame.Start.DayOfWeek - eachTuple.Item1) + 7) % 7;
                DateTimeOffset Start = FirstFrame.Start.AddDays(DayDiff);
                Start= eachTuple.Item2.getInjectedStartHourMinIntoDateTime(Start);
                TimeLine myTimeLine = eachTuple.Item2.getTimeLineFromStartFrame(Start);
                while (Start < LastFrame.End)
                {
                    ListOfTimeLine.Add(myTimeLine);
                    Start.AddDays(7);
                    myTimeLine = eachTuple.Item2.getTimeLineFromStartFrame(Start);
                }
            }

            List<TimeLine> retValue = TimeLinesPerDaySelection.SelectMany(obj => obj).ToList();
            return retValue;
        }
        


        /// <summary>
        /// function tries to get a timeline that has its borders as the frame of the various active events.
        /// </summary>
        /// <param name="RefTimeline"></param>
        /// <returns></returns>
        public TimeLine getWidestTimeLineBorders(TimeLine RefTimeline)
        {
            DateTimeOffset Start = RefTimeline.Start;
            Start = getEarliestStartTimeFrameBorder(Start);

            DateTimeOffset End = RefTimeline.End;
            End = getLatestEndTimeFrameBorder(End);

            TimeLine retValue = new TimeLine(Start, End);
            return retValue;
        }

        /// <summary>
        /// function tries to select earliest time after the RefStart time that borders the beginning of a restriction frame. So Assuming you had frame Nov 11 2015 Mon 11a-2p, Nov 13 2015 Wed 11a-2p. And RefStart is Nov 11 2015 Mon 12p, it'll pick Nov 13 2015 Wed 11a because its a new border frame. It didn't select Mon's because it was past the start of the frame
        /// </summary>
        /// <param name="RefStart"></param>
        /// <returns></returns>
        public DateTimeOffset getEarliestStartTimeFrameBorder(DateTimeOffset RefStart)
        {    
            DateTimeOffset Start = RefStart;
            int StartIndexOfDayOfweek = (int)Start.DayOfWeek;
            int DayDiff = 0;
            Tuple<DayOfWeek, RestrictionTimeLine> selectedDay;
            for (int i = 0; i < 7; i++, StartIndexOfDayOfweek++)
            {
                int OffSetStartIndex  = StartIndexOfDayOfweek % 7;
                selectedDay = DaySelection[OffSetStartIndex];
                if (selectedDay   != null)
                {
                    DayDiff = (((int)selectedDay.Item1 - OffSetStartIndex) + 7) % 7;
                    DateTimeOffset newTime = Start.AddDays(DayDiff);
                    newTime = new DateTimeOffset(newTime.Year, newTime.Month, newTime.Day, selectedDay.Item2.Start.Hour, selectedDay.Item2.Start.Minute, selectedDay.Item2.Start.Second, new TimeSpan());
                    if (newTime >= RefStart)
                    {
                        return newTime;
                    }
                }
            }
            DateTimeOffset retValue = RefStart.AddDays(7);
            retValue= new DateTimeOffset(retValue.Year,retValue .Month,retValue .Day,NoNull_DaySelections[0].Item2.Start.Hour,NoNull_DaySelections[0].Item2.Start.Minute,NoNull_DaySelections[0].Item2.Start.Second, new TimeSpan());

            
            return retValue;
        }


        /// <summary>
        /// function tries to select Latest time before the RefTime time that borders the End of a restriction frame. So Assuming you had frame Nov 11 2015 Mon 11a-2p, Nov 13 2015 Wed 11a-2p. And RefTime is Nov 11 2015 Mon 12p, it'll pick Nov 11 2015 Mon 2p because its a new border frame. It didn't select Wed's because it was before the End of the frame
        /// </summary>
        /// <param name="RefTime"></param>
        /// <returns></returns>
        public DateTimeOffset getLatestEndTimeFrameBorder(DateTimeOffset RefTime)
        {
            DateTimeOffset End = RefTime;
            int StartIndexOfDayOfweek = (int)End.DayOfWeek;
            int DayDiff = 0;
            Tuple<DayOfWeek, RestrictionTimeLine> selectedDay;
            for (int i = 7; i >0; i--, StartIndexOfDayOfweek--)
            {
                int OffSetStartIndex = (StartIndexOfDayOfweek + 14) % 7;
                selectedDay = DaySelection[OffSetStartIndex];
                if (selectedDay != null)
                {
                    DayDiff = (((int)selectedDay.Item1 - OffSetStartIndex)) % 7;//gets the delta to be added to the new 
                    DateTimeOffset newTime = End.AddDays(DayDiff);
                    newTime = selectedDay.Item2.getInjectedEndHourMinIntoDateTime(newTime);
                    if (newTime <= RefTime)
                    {
                        return newTime;
                    }
                }
            }
            DateTimeOffset retValue = RefTime.AddDays(-7);
            retValue = new DateTimeOffset(retValue.Year, retValue.Month, retValue.Day, NoNull_DaySelections[0].Item2.End.Hour, NoNull_DaySelections[0].Item2.End.Minute, NoNull_DaySelections[0].Item2.End.Second, new TimeSpan());
            return retValue;
        }



        /// <summary>
        /// function tries to select Latest time before or on the RefTime time that borders the End of a restriction frame. So Assuming you had frame Nov 11 2015 Mon 11a-2p, Nov 13 2015 Wed 11a-2p. And RefTime is Nov 13 2015 Wed 12p, it'll pick Nov 13 2015 Wed 12p because its already within a border and can simply use its position as the final position
        /// </summary>
        /// <param name="RefTime"></param>
        /// <returns></returns>
        public DateTimeOffset getLatestEndTimeWithinFrameBeforeRefTime(DateTimeOffset RefTime)
        {
            DateTimeOffset End = RefTime;
            int StartIndexOfDayOfweek = (int)End.DayOfWeek;
            int DayDiff = 0;
            Tuple<DayOfWeek, RestrictionTimeLine> selectedDay;
            for (int i = 7; i > 0; i--, StartIndexOfDayOfweek--)
            {
                int OffSetStartIndex = (StartIndexOfDayOfweek + 14) % 7;
                selectedDay = DaySelection[OffSetStartIndex];
                if (selectedDay != null)
                {
                    DayDiff = (((int)selectedDay.Item1 - OffSetStartIndex)) % 7;//gets the delta to be added to the new 
                    DateTimeOffset newTime = End.AddDays(DayDiff);
                    DateTimeOffset newStartTime = selectedDay.Item2.getInjectedStartHourMinIntoDateTime(newTime);
                    DateTimeOffset newEndTime = selectedDay.Item2.getInjectedEndHourMinIntoDateTime(newTime);

                    TimeLine currentFrame = new TimeLine(newStartTime, newEndTime);
                    if (currentFrame.IsDateTimeWithin(newTime) || (newTime == newStartTime) || (newTime == newEndTime))
                    {
                        return newTime;
                    }
                    if (newTime > newEndTime)//Checks if reftime is after the End of the Timeline TImeFrame . Since fuction tries to get the latest time before the ref time, if the timeline frame is before the reftime then then the End of timeframe is latest endtime possible. 
                    {
                        return newEndTime;
                    }
                }
            }
            DateTimeOffset retValue = RefTime.AddDays(-7);
            retValue = new DateTimeOffset(retValue.Year, retValue.Month, retValue.Day, NoNull_DaySelections[0].Item2.End.Hour, NoNull_DaySelections[0].Item2.End.Minute, NoNull_DaySelections[0].Item2.End.Second, new TimeSpan());
            return retValue;
        }


        /// <summary>
        /// function tries to select earliest time after or at the RefStart time that as is within any of the restriction frame. So Assuming you had frame Nov 11 2015 Mon 11a-2p, Nov 13 2015 Wed 11a-2p. And RefStart is Nov 11 2015 Mon 12p, it'll pick Nov 11 2015 Mon 12a because its within the Mon Frame.
        /// </summary>
        /// <param name="RefStart"></param>
        /// <returns></returns>
        public DateTimeOffset getEarliestStartTimeWithinAFrameAfterRefTime(DateTimeOffset RefStart)
        {    
            DateTimeOffset Start = RefStart;
            int StartIndexOfDayOfweek = (int)Start.DayOfWeek;
            int DayDiff = 0;
            Tuple<DayOfWeek, RestrictionTimeLine> selectedDay;
            for (int i = 0; i < 7; i++, StartIndexOfDayOfweek++)
            {
                int OffSetStartIndex  = StartIndexOfDayOfweek % 7;
                selectedDay = DaySelection[OffSetStartIndex];
                if (selectedDay   != null)
                {
                    DayDiff = (((int)selectedDay.Item1 - OffSetStartIndex) + 7) % 7;
                    DateTimeOffset newTime= Start.AddDays(DayDiff);//shifts start to the day of a reference frame;
                    DateTimeOffset newStartTime = selectedDay.Item2.getInjectedStartHourMinIntoDateTime(newTime);
                    DateTimeOffset newEndTime = selectedDay.Item2.getInjectedEndHourMinIntoDateTime(newTime);

                    TimeLine currentFrame = new TimeLine(newStartTime, newEndTime);
                    if (currentFrame.IsDateTimeWithin(newTime) || (newTime == newStartTime) || (newTime == newEndTime))
                    {
                        return newTime;
                    }
                    if (newTime < newStartTime)//checks if the new Time occurs before the supposed timeframe. Since it is not within the current timeframe and  it occurs before the timeframe , then the next possible timeframe is this time frame. i.e if time frame is 9:00am - 9:00pm 11/13/2014 and new time is 7:00am on 11/13/2014. The next earliest time frame will be 9:00am-9:00pm 11/13/2014
                    {
                        return newStartTime;
                    }
                }
            }
            DateTimeOffset retValue = RefStart.AddDays(7);
            retValue= new DateTimeOffset(retValue.Year,retValue .Month,retValue .Day,NoNull_DaySelections[0].Item2.Start.Hour,NoNull_DaySelections[0].Item2.Start.Minute,NoNull_DaySelections[0].Item2.Start.Second, new TimeSpan());
            return retValue;
        }


        bool isHourTimeOfBLaterThanOrEqualToA(DateTimeOffset DateTimeOffsetA, DateTimeOffset DateTimeOffsetB)
        {
            DateTimeOffset refDateTimeOffsetA = new DateTimeOffset(1, 1, 1, DateTimeOffsetA.Hour, DateTimeOffsetA.Minute, DateTimeOffsetA.Second, new TimeSpan());
            DateTimeOffset refDateTimeOffsetB = new DateTimeOffset(1, 1, 1, DateTimeOffsetB.Hour, DateTimeOffsetB.Minute, DateTimeOffsetB.Second, new TimeSpan());
            return refDateTimeOffsetB >= refDateTimeOffsetA;
        }

    }
}
