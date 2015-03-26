using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace TilerElements
{
    /// <summary>
    /// Reperesents the restriction of profile. This is to be used with events. There is no to be a default constructor for semantic purposes
    /// </summary>
    public class RestrictionProfile
    {
        static DateTimeOffset SundayDate = new DateTimeOffset(2015, 3, 15, 0, 0, 0, new TimeSpan());
        public static readonly DayOfWeek[] AllDaysOfWeek = new DayOfWeek[7] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
        protected Tuple<DayOfWeek, RestrictionTimeLine>[] DaySelection = new Tuple<DayOfWeek, RestrictionTimeLine>[7];
        protected Tuple<DayOfWeek, RestrictionTimeLine>[] NoNull_DaySelections;
        protected List<Tuple<int,int>>[] DayOfWeekToOverlappingIndexes = new List<Tuple<int,int>>[7];


        bool[] ActiveDays = new bool[7];
        DayOfWeek startDayOfWeek = DayOfWeek.Monday;
        
        protected RestrictionProfile()
        {
            
        }

        public RestrictionProfile(int typeOfRestriction,DayOfWeek beginningDayOfWeek ,DateTimeOffset Start,DateTimeOffset End)
        {
            
            switch (typeOfRestriction)
            { 
                case 0://Work week
                    {
                        TimeSpan FiveDays = new TimeSpan(4, 0, 0, 0);
                        Start = new DateTimeOffset(SundayDate.Year, SundayDate.Month, SundayDate.Day, Start.Hour, Start.Minute, Start.Second, new TimeSpan()).AddDays((int)beginningDayOfWeek);
                        DateTimeOffset tempEnd  = Start.Add(FiveDays );
                        tempEnd = new DateTimeOffset(tempEnd  .Year,tempEnd  .Month,tempEnd  .Day,End.Hour,End.Minute,End.Second,new TimeSpan());
                        FiveDays=tempEnd-Start;
                        NoNull_DaySelections = new Tuple<DayOfWeek, RestrictionTimeLine>[1];
                        for (int i = (int)beginningDayOfWeek; i < 2; i++)//starting i from 1 because workweek starts on Monday and Monday index is 1
                        {
                            DaySelection[i] = new Tuple<DayOfWeek, RestrictionTimeLine>(AllDaysOfWeek[i], new RestrictionTimeLine(Start, FiveDays));
                            NoNull_DaySelections[0] = DaySelection[i];
                        }
                    }
                    break;
                default://Work week and Office Hours
                    {
                        NoNull_DaySelections = new Tuple<DayOfWeek, RestrictionTimeLine>[5];
                        for (int i = (int)beginningDayOfWeek, j = 0; j < NoNull_DaySelections.Length; i++, j++)//starting i from 1 because workweek starts on Monday and Monday index is 1
                        {
                            DaySelection[i] = new Tuple<DayOfWeek, RestrictionTimeLine>(AllDaysOfWeek[i], new RestrictionTimeLine(Start, End));
                            NoNull_DaySelections[j] = DaySelection[i];
                        }
                    }
                    break;
            }
            InitializeOverLappingDictionary();
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
            InitializeOverLappingDictionary();
        }

        public RestrictionProfile(IEnumerable<DayOfWeek> DaysOfWeekSelection, RestrictionTimeLine constrictionProgile)
        {
            DaysOfWeekSelection = DaysOfWeekSelection.OrderBy(obj=>obj).ToArray();
            startDayOfWeek =((DayOfWeek[]) DaysOfWeekSelection)[0];
            foreach (DayOfWeek eachDayOfWeek in DaysOfWeekSelection)
            {
                int DayOfWeekInt = (int)eachDayOfWeek;
                DaySelection[DayOfWeekInt] = new Tuple<DayOfWeek, RestrictionTimeLine>(eachDayOfWeek, constrictionProgile);
            }

            NoNull_DaySelections = DaySelection.Where(obj => obj != null).ToArray();
            InitializeOverLappingDictionary();
        }


        protected void InitializeOverLappingDictionary()
        {
            TimeSpan twentyFourHourSpan = new  TimeSpan(1,0,0,0,0);
            for (int i = 0; i < DayOfWeekToOverlappingIndexes.Length; i++)
            {
                DayOfWeekToOverlappingIndexes[i] = new List<Tuple<int, int>>();
            }


            for (int i = 0; i < NoNull_DaySelections.Length; i++)
            {
                DateTimeOffset myStartTime=NoNull_DaySelections[i].Item2.getSampleTestTIme();
                DateTimeOffset StartOfNextDay = myStartTime.AddDays(1);
                StartOfNextDay = new DateTimeOffset(StartOfNextDay.Year, StartOfNextDay.Month, StartOfNextDay.Day, 0, 0, 0, new TimeSpan());
                TimeSpan RestOfDaySpan = StartOfNextDay - myStartTime;
                TimeSpan RestrictionSpanLeft = NoNull_DaySelections[i].Item2.Span - RestOfDaySpan;
                int NumberOfDaysExtra = (int)Math.Ceiling( (double)RestrictionSpanLeft.Ticks / (double)twentyFourHourSpan.Ticks);
                int StartingIndex = (int)NoNull_DaySelections[i].Item1;
                DayOfWeekToOverlappingIndexes[StartingIndex].Add(new Tuple<int, int>(i, NumberOfDaysExtra));
                StartingIndex += 1;//increasing by one so that the index can begin on the next day
                for(int j=0;j<NumberOfDaysExtra;j++)
                {
                    int Myindex= (StartingIndex+j)%7;
                    DayOfWeekToOverlappingIndexes[Myindex].Add(new Tuple<int, int>(i, NumberOfDaysExtra));
                }
            }
        }

        public RestrictionProfile createCopy()
        {
            RestrictionProfile retValue = new RestrictionProfile();
            retValue.DaySelection = this.DaySelection.Select(obj => obj == null ? obj : new Tuple<DayOfWeek, RestrictionTimeLine>(obj.Item1, obj.Item2.createCopy())).ToArray();
            retValue.NoNull_DaySelections = this.DaySelection.Where(obj=>obj!=null).Select(obj => new Tuple<DayOfWeek, RestrictionTimeLine>(obj.Item1, obj.Item2.createCopy())).ToArray();
            retValue.DayOfWeekToOverlappingIndexes = this.DayOfWeekToOverlappingIndexes.ToArray();
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
            Start =  getEarliestStartTimeWithinAFrameAfterRefTime(Start).Start;

            DateTimeOffset End = RefTimeline.End;
            End = getLatestEndTimeWithinFrameBeforeRefTime(End).End;

            TimeLine retValue = new TimeLine(Start, End);
            return retValue;
        }

        public TimeLine getLatestActiveTimeFrameBeforeEnd(IDefinedRange RefTimeline)
        {
            TimeLine LatestFrame = getLatestEndTimeWithinFrameBeforeRefTime(RefTimeline.End);
            DateTimeOffset End = LatestFrame.End;
            DateTimeOffset Start = LatestFrame.Start;
            //Start = LatestFrame. getEarliestStartTimeFrameBorder(Start);
            Start = RefTimeline.Start>Start?RefTimeline.Start :Start;

            TimeLine retValue = new TimeLine(Start, End);
            return retValue;
        }

        public TimeLine getEarliestActiveFrameAfterBeginning(IDefinedRange RefTimeline)
        {
            TimeLine EarliestFrame = getEarliestStartTimeWithinAFrameAfterRefTime(RefTimeline.Start);;

            DateTimeOffset Start = EarliestFrame.Start;
            DateTimeOffset End = EarliestFrame.End;
            //End = getLatestEndTimeFrameBorder(End);
            End = RefTimeline.End < End ? RefTimeline.End : End;

            TimeLine retValue = new TimeLine(Start, End);
            return retValue;
        }

        /// <summary>
        /// Gets the First full frame bordered by the beginning of RefTimeline. Note it does not check for fitability
        /// </summary>
        /// <param name="RefTimeline"></param>
        /// <returns></returns>

        public TimeLine getEarliestFullframe(IDefinedRange RefTimeline)
        {
            TimeLine timeFrame = getEarliestStartTimeFrameBorder(RefTimeline.Start);
            DateTimeOffset Start = timeFrame.Start;
            TimeLine retValue = timeFrame;// DaySelection[(int)Start.DayOfWeek].Item2.getTimeLineFromStartFrame(Start);
            return retValue;
        }
        /// <summary>
        /// gets the last full restrictive frame bordered by the end of the timeline. It does not check if the full frame fits.
        /// </summary>
        /// <param name="RefTimeLine"></param>
        /// <returns></returns>
        public TimeLine getLatestFullFrame(IDefinedRange RefTimeLine)
        {
            TimeLine timeFrame = getLatestEndTimeFrameBorder(RefTimeLine.End);
            DateTimeOffset End = timeFrame.End;
            TimeLine retValue = timeFrame;// DaySelection[(int)End.DayOfWeek].Item2.getTimeLineFromStartFrame(End);
            return retValue;
        }

        /// <summary>
        /// functiontries to get the time full frame that encases "StartData". If there isnt it returns null
        /// </summary>
        /// <param name="StartData"></param>
        /// <returns></returns>

        public TimeLine getEncasingFullFrame(DateTimeOffset StartData)
        {
            int DayOfWeekInt = (int)StartData.DayOfWeek;
            TimeLine retValue;
            DateTimeOffset refSTart = new DateTimeOffset(1, 1, 1, StartData.Hour, StartData.Minute, StartData.Second, new TimeSpan());

            List<Tuple<int, int>> AllInterFerringIndexes = DayOfWeekToOverlappingIndexes[DayOfWeekInt];
            int RightIndex = -1;

            for (int i = 0; i < AllInterFerringIndexes.Count; i++)
            {
                Tuple<int, int> myTUple = AllInterFerringIndexes[i];
                int NumberOfDays = StartData.DayOfWeek - NoNull_DaySelections[myTUple.Item1].Item1;
                TimeLine DayFramTImeLine = getTImeLineFromTuple(NoNull_DaySelections[myTUple.Item1], StartData);
                if (DayFramTImeLine.IsDateTimeWithin(StartData) || (DayFramTImeLine.Start == StartData) || (DayFramTImeLine.End == StartData))
                {
                    retValue = DayFramTImeLine;
                    return retValue;
                }
            }

            return null;
        }

        /// <summary>
        /// Function gets all the available Restriction frames within a timeLine. Note this does not return partial framess. So if reftimeline was March 2 2015 monday 7:00am - 10:00am and Day selections were MTW 9:00a- 9:00p. It'll return only March 2 9:00am - 9:00pm. Note it didn't return a partial of March 2 9:00am-10:00am.
        /// </summary>
        /// <param name="RefTimeLine"></param>
        /// <returns></returns>

        public List<TimeLine> getAllTimePossibleTimeFrames(IDefinedRange RefTimeLine)
        {
            TimeLine FirstFrame = getEarliestActiveFrameAfterBeginning(RefTimeLine);
            TimeLine LastFrame = getLatestActiveTimeFrameBeforeEnd(RefTimeLine);

            List<TimeLine>[] TimeLinesPerDaySelection = NoNull_DaySelections.Select(obj=>new List<TimeLine>()).ToArray();

            TimeLine EncasingFrame = getEncasingFullFrame(RefTimeLine.Start);

            DayOfWeek InitializigWeekday = EncasingFrame!=null? EncasingFrame.Start.DayOfWeek: getEarliestFullframe(FirstFrame).Start.DayOfWeek;

            int initializingIndex =0;
            for (; initializingIndex< NoNull_DaySelections.Length; initializingIndex++)
            {
                if (NoNull_DaySelections[initializingIndex].Item1 == InitializigWeekday)
                {
                    break;
                }
            }
            int lengthOfNoNull_DaySelections = NoNull_DaySelections.Length;
            for (int i = initializingIndex, j=0; j < NoNull_DaySelections.Length; i++,j++)
            //for (int i = initializingIndex; i < NoNull_DaySelections.Length; i++)
            {
                i = (i + lengthOfNoNull_DaySelections) % lengthOfNoNull_DaySelections;
                Tuple<DayOfWeek, RestrictionTimeLine> eachTuple = NoNull_DaySelections[i];
                List<TimeLine> ListOfTimeLine = TimeLinesPerDaySelection[i];
                int DayDiff = ((eachTuple.Item1-FirstFrame.Start.DayOfWeek)+7) % 7;
                DateTimeOffset Start = FirstFrame.Start.AddDays(DayDiff);
                //Start= eachTuple.Item2.getInjectedStartHourMinIntoDateTime(Start);
                TimeLine myTimeLine = getTImeLineFromTuple(eachTuple, Start);// eachTuple.Item2.getTimeLineFromStartFrame(Start);
                while (Start < LastFrame.End)
                {
                    ListOfTimeLine.Add(myTimeLine);
                    Start=Start.AddDays(7);
                    myTimeLine = eachTuple.Item2.getTimeLineFromStartFrame(Start);
                }
            }

            List<TimeLine> retValue = TimeLinesPerDaySelection.SelectMany(obj => obj).ToList();
            if (retValue.Count>0)
            {
                if (retValue[0].Start < FirstFrame.Start)//ensures that the first frame is within RefTimeLine. THe preceding code always generates a full frame so, we need to generate a partial frame
                {
                    retValue[0] = FirstFrame;
                }

                if (retValue.Last().End > LastFrame.End)
                {
                    retValue[retValue.Count - 1] = LastFrame;
                }
                
            }
            return retValue;
        }
        


        /// <summary>
        /// function tries to get a timeline that has its borders as the frame of the various active events.
        /// </summary>
        /// <param name="RefTimeline"></param>
        /// <returns></returns>
        public TimeLine getWidestTimeLineBorders(IDefinedRange RefTimeline)
        {
            DateTimeOffset Start = RefTimeline.Start;
            Start = getEarliestStartTimeFrameBorder(Start).Start;

            DateTimeOffset End = RefTimeline.End;
            End = getLatestEndTimeFrameBorder(End).End;

            TimeLine retValue = new TimeLine(Start, End);
            return retValue;
        }

        /// <summary>
        /// function tries to select earliest time after the RefStart time that borders the beginning of a restriction frame. So Assuming you had frame Nov 11 2015 Mon 11a-2p, Nov 13 2015 Wed 11a-2p. And RefStart is Nov 11 2015 Mon 12p, it'll pick Nov 13 2015 Wed 11a because its a new border frame. It didn't select Mon's because it was past the start of the frame
        /// </summary>
        /// <param name="RefStart"></param>
        /// <returns></returns>
        public TimeLine getEarliestStartTimeFrameBorder(DateTimeOffset RefStart)
        {
            TimeLine retValue;
            DateTimeOffset Start = RefStart;
            int StartIndexOfDayOfweek = (int)Start.DayOfWeek;
            int IniStartIndexOfDayIndex = StartIndexOfDayOfweek;
            int DayDiff = 0;
            TimeLine preceedingCycleFrame = new TimeLine(RefStart, RefStart);
            Tuple<DayOfWeek, RestrictionTimeLine> selectedDay;
            for (int i = 0; i < 7; i++, StartIndexOfDayOfweek++)
            {
                int OffSetStartIndex = StartIndexOfDayOfweek % 7;
                selectedDay = DaySelection[OffSetStartIndex];
                if (selectedDay != null)
                {
                    DayDiff = (((int)selectedDay.Item1 - IniStartIndexOfDayIndex) + 7) % 7;
                    DateTimeOffset newTime = Start.AddDays(DayDiff);
                    TimeLine currentFrame = getTImeLineFromTuple(selectedDay, newTime);
                    newTime = currentFrame.Start;
                    if (newTime >= RefStart)
                    {
                        retValue = currentFrame;
                        return retValue;
                    }
                    preceedingCycleFrame = currentFrame;
                }
            }
            preceedingCycleFrame = new TimeLine(preceedingCycleFrame.Start.AddDays(-7), preceedingCycleFrame.End.AddDays(-7));
            return preceedingCycleFrame;
            /*throw new Exception("This isn't suppposed to happen in getEarliestStartTimeFrameBorder");
            DateTimeOffset retValueTime = RefStart.AddDays(7);
            retValueTime = new DateTimeOffset(retValueTime.Year, retValueTime.Month, retValueTime.Day, NoNull_DaySelections[0].Item2.Start.Hour, NoNull_DaySelections[0].Item2.Start.Minute, NoNull_DaySelections[0].Item2.Start.Second, new TimeSpan());
            retValue = NoNull_DaySelections[0].Item2.getTimeLineFromStartFrame(retValueTime);

            return retValue;*/
        }
        
        
        /*
        public TimeLine getEarliestStartTimeFrameBorder(DateTimeOffset RefStart)
        {
            TimeLine retValue;
            DateTimeOffset Start = RefStart;
            int StartIndexOfDayOfweek = (int)Start.DayOfWeek;
            int IniStartIndexOfDayIndex = StartIndexOfDayOfweek;
            int DayDiff = 0;
            Tuple<DayOfWeek, RestrictionTimeLine> selectedDay;
            for (int i = 0; i < 7; i++, StartIndexOfDayOfweek++)
            {
                int OffSetStartIndex  = StartIndexOfDayOfweek % 7;
                selectedDay = DaySelection[OffSetStartIndex];
                if (selectedDay   != null)
                {
                    DayDiff = (((int)selectedDay.Item1 - IniStartIndexOfDayIndex) + 7) % 7;
                    DateTimeOffset newTime = Start.AddDays(DayDiff);
                    newTime = new DateTimeOffset(newTime.Year, newTime.Month, newTime.Day, selectedDay.Item2.Start.Hour, selectedDay.Item2.Start.Minute, selectedDay.Item2.Start.Second, new TimeSpan());
                    if (newTime >= RefStart)
                    {
                        retValue = selectedDay.Item2.getTimeLineFromStartFrame(newTime);
                        return retValue;
                    }
                }
            }
            DateTimeOffset retValueTime = RefStart.AddDays(7);
            retValueTime = new DateTimeOffset(retValueTime.Year, retValueTime.Month, retValueTime.Day, NoNull_DaySelections[0].Item2.Start.Hour, NoNull_DaySelections[0].Item2.Start.Minute, NoNull_DaySelections[0].Item2.Start.Second, new TimeSpan());
            retValue = NoNull_DaySelections[0].Item2.getTimeLineFromStartFrame(retValueTime);
            
            return retValue;
        }
        */

        /// <summary>
        /// function tries to select Latest time before the RefTime time that borders the End of a restriction frame. So Assuming you had frame Nov 11 2015 Mon 11a-2p, Nov 13 2015 Wed 11a-2p. And RefTime is Nov 11 2015 Mon 12p, it'll pick Nov 11 2015 Mon 2p because its a new border frame. It didn't select Wed's because it was before the End of the frame
        /// </summary>
        /// <param name="RefTime"></param>
        /// <returns></returns>
        public TimeLine getLatestEndTimeFrameBorder(DateTimeOffset RefTime)
        {
            TimeLine retValue;
            DateTimeOffset End = RefTime;
            int StartIndexOfDayOfweek = (int)End.DayOfWeek;
            int IniStartIndexOfDayIndex = StartIndexOfDayOfweek;
            int DayDiff = 0;

            TimeLine preceedingCycleFrame = new TimeLine(RefTime, RefTime);
            Tuple<DayOfWeek, RestrictionTimeLine> selectedDay;
            for (int i = 7; i > 0; i--, StartIndexOfDayOfweek--)
            {
                int OffSetStartIndex = (StartIndexOfDayOfweek + 14) % 7;
                selectedDay = DaySelection[OffSetStartIndex];
                if (selectedDay != null)
                {
                    DayDiff = (((int)selectedDay.Item1 - IniStartIndexOfDayIndex)) % 7;//gets the delta to be added to the new 
                    DateTimeOffset newTime = End.AddDays(DayDiff);
                    TimeLine currentFrame = getTImeLineFromTuple(selectedDay, RefTime);

                    newTime = currentFrame.End;
                    if (newTime <= RefTime)
                    {
                        retValue = currentFrame;
                        return retValue;
                    }

                    preceedingCycleFrame = currentFrame;
                }
            }
            preceedingCycleFrame = new TimeLine(preceedingCycleFrame.Start.AddDays(-7), preceedingCycleFrame.End.AddDays(-7));
            return preceedingCycleFrame;
            /*
            throw new Exception("This isn't suppposed to happen in getLatestEndTimeFrameBorder");
            DateTimeOffset retValueTime = RefTime.AddDays(-7);
            retValueTime = new DateTimeOffset(retValueTime.Year, retValueTime.Month, retValueTime.Day, NoNull_DaySelections[0].Item2.End.Hour, NoNull_DaySelections[0].Item2.End.Minute, NoNull_DaySelections[0].Item2.End.Second, new TimeSpan());
            retValue = NoNull_DaySelections[0].Item2.getTimelineFromEndFrame(retValueTime);
            return retValue;*/
        }
        
        
        /*
        public TimeLine getLatestEndTimeFrameBorder(DateTimeOffset RefTime)
        {
            TimeLine retValue ;
            DateTimeOffset End = RefTime;
            int StartIndexOfDayOfweek = (int)End.DayOfWeek;
            int IniStartIndexOfDayIndex = StartIndexOfDayOfweek;
            int DayDiff = 0;
            Tuple<DayOfWeek, RestrictionTimeLine> selectedDay;
            for (int i = 7; i >0; i--, StartIndexOfDayOfweek--)
            {
                int OffSetStartIndex = (StartIndexOfDayOfweek + 14) % 7;
                selectedDay = DaySelection[OffSetStartIndex];
                if (selectedDay != null)
                {
                    DayDiff = (((int)selectedDay.Item1 - IniStartIndexOfDayIndex)) % 7;//gets the delta to be added to the new 
                    DateTimeOffset newTime = End.AddDays(DayDiff);
                    newTime = selectedDay.Item2.getInjectedEndHourMinIntoDateTime(newTime);
                    if (newTime <= RefTime)
                    {
                        retValue =selectedDay.Item2.getTimelineFromEndFrame( newTime);
                        return retValue;
                    }
                }
            }
            DateTimeOffset retValueTime = RefTime.AddDays(-7);
            retValueTime = new DateTimeOffset(retValueTime.Year, retValueTime.Month, retValueTime.Day, NoNull_DaySelections[0].Item2.End.Hour, NoNull_DaySelections[0].Item2.End.Minute, NoNull_DaySelections[0].Item2.End.Second, new TimeSpan());
            retValue = NoNull_DaySelections[0].Item2.getTimelineFromEndFrame(retValueTime);    
            return retValue;
        }
        */



        /// <summary>
        /// function tries to select Latest time before or on the RefTime time that borders the End of a restriction frame. So Assuming you had frame Nov 11 2015 Mon 11a-2p, Nov 13 2015 Wed 11a-2p. And RefTime is Nov 13 2015 Wed 12p, it'll pick Nov 13 2015 Wed 12p because its already within a border and can simply use its position as the final position
        /// </summary>
        /// <param name="RefTime"></param>
        /// <returns></returns>
        public TimeLine getLatestEndTimeWithinFrameBeforeRefTime(DateTimeOffset RefTime)
        {
            int DayOfWeekInt = (int)RefTime.DayOfWeek;
            TimeLine retValue;
            DateTimeOffset refSTart = new DateTimeOffset(1, 1, 1, RefTime.Hour, RefTime.Minute, RefTime.Second, new TimeSpan());

            List<Tuple<int,int>> AllInterFerringIndexes = DayOfWeekToOverlappingIndexes[DayOfWeekInt];
            int RightIndex = -1;

            for (int i = 0; i < AllInterFerringIndexes.Count; i++)
            {
                Tuple<int,int> myTUple=AllInterFerringIndexes[i];
                int NumberOfDays = RefTime.DayOfWeek - NoNull_DaySelections[myTUple.Item1].Item1;
                TimeLine DayFramTImeLine = getTImeLineFromTuple(NoNull_DaySelections[myTUple.Item1],RefTime);
                if (DayFramTImeLine.IsDateTimeWithin(RefTime) || (DayFramTImeLine.End == RefTime))// || (DayFramTImeLine.Start == RefTime))
                {
                    retValue = new TimeLine(DayFramTImeLine.Start, RefTime);
                    return retValue;
                }
                else
                {
                    retValue = getLatestEndTimeFrameBorder(RefTime);
                    return retValue;
                }
            }

            retValue = getLatestEndTimeFrameBorder(RefTime);
            return retValue;
                
        }


        /// <summary>
        /// function tries to select earliest time after or at the RefStart time that as is within any of the restriction frame. So Assuming you had frame Nov 11 2015 Mon 11a-2p, Nov 13 2015 Wed 11a-2p. And RefStart is Nov 11 2015 Mon 12p, it'll pick Nov 11 2015 Mon 12a because its within the Mon Frame.
        /// </summary>
        /// <param name="StartData"></param>
        /// <returns></returns>


        public TimeLine getEarliestStartTimeWithinAFrameAfterRefTime(DateTimeOffset StartData)
        {
            int DayOfWeekInt = (int)StartData.DayOfWeek;
            TimeLine retValue;
            DateTimeOffset refSTart = new DateTimeOffset(1, 1, 1, StartData.Hour, StartData.Minute, StartData.Second, new TimeSpan());

            List<Tuple<int,int>> AllInterFerringIndexes = DayOfWeekToOverlappingIndexes[DayOfWeekInt];
            int RightIndex = -1;

            for (int i = 0; i < AllInterFerringIndexes.Count; i++)
            {
                Tuple<int,int> myTUple=AllInterFerringIndexes[i];
                int NumberOfDays = StartData.DayOfWeek - NoNull_DaySelections[myTUple.Item1].Item1;
                TimeLine DayFramTImeLine = getTImeLineFromTuple(NoNull_DaySelections[myTUple.Item1],StartData);
                if (DayFramTImeLine.IsDateTimeWithin(StartData) || (DayFramTImeLine.Start == StartData))// || (DayFramTImeLine.End == StartData))
                {
                    retValue = new TimeLine(StartData, DayFramTImeLine.End);
                    return retValue;
                }
                else
                {
                    retValue = getEarliestStartTimeFrameBorder(StartData);
                    return retValue;
                }
            }

            retValue = getEarliestStartTimeFrameBorder(StartData);
            return retValue;
            
        }
        
        TimeLine getTImeLineFromTuple(Tuple<DayOfWeek, RestrictionTimeLine> myTuple, DateTimeOffset Start)
        {
            int DayDifference = myTuple.Item1- Start.DayOfWeek;
            DateTimeOffset refStart = Start.AddDays(DayDifference);
            TimeLine retValue =  myTuple.Item2.getTimeLineFromStartFrame(refStart);
            return retValue;
        }
        
        /*
        public TimeLine getEarliestStartTimeWithinAFrameAfterRefTime(DateTimeOffset RefStart)
        {    
            DateTimeOffset Start = RefStart;
            int StartIndexOfDayOfweek = (int)Start.DayOfWeek;
            int IniStartIndexOfDayIndex = StartIndexOfDayOfweek;
            int DayDiff = 0;
            Tuple<DayOfWeek, RestrictionTimeLine> selectedDay;
            for (int i = 0; i < 7; i++, StartIndexOfDayOfweek++)
            {
                int OffSetStartIndex  = StartIndexOfDayOfweek % 7;
                selectedDay = DaySelection[OffSetStartIndex];
                if (selectedDay != null)
                {
                    DayDiff = (((int)selectedDay.Item1 - IniStartIndexOfDayIndex) + 7) % 7;
                    DateTimeOffset newTime = Start.AddDays(DayDiff);//shifts start to the day of a reference frame;
                    DateTimeOffset newStartTime = selectedDay.Item2.getInjectedStartHourMinIntoDateTime(newTime);
                    DateTimeOffset newEndTime = selectedDay.Item2.getInjectedEndHourMinIntoDateTime(newTime);

                    TimeLine currentFrame = new TimeLine(newStartTime, newEndTime);
                    if (currentFrame.IsDateTimeWithin(newTime) || (newTime == newStartTime) || (newTime == newEndTime))
                    {
                        return new TimeLine(newTime, newEndTime);
                    }
                    if ((newTime < newStartTime))//checks if the new Time occurs before the supposed timeframe. Since it is not within the current timeframe and  it occurs before the timeframe , then the next possible timeframe is this time frame. i.e if time frame is 9:00am - 9:00pm 11/13/2014 and new time is 7:00am on 11/13/2014. The next earliest time frame will be 9:00am-9:00pm 11/13/2014
                    {
                        return new TimeLine(newStartTime, newEndTime);
                    }

                    if ((newTime > RefStart))//checks if it has progressed to the next time frame. That is the only time when newTime>RefStart and still not be within a frame
                    {
                        return new TimeLine(newStartTime, newEndTime);
                    }
                }
                else
                {
                    return getEarliestStartTimeFrameBorder(RefStart);
                }
            }
            DateTimeOffset retValue = RefStart.AddDays(7);
            
            retValue= new DateTimeOffset(retValue.Year,retValue .Month,retValue .Day,NoNull_DaySelections[0].Item2.Start.Hour,NoNull_DaySelections[0].Item2.Start.Minute,NoNull_DaySelections[0].Item2.Start.Second, new TimeSpan());
            TimeLine retValueTimeLine = NoNull_DaySelections[0].Item2.getTimeLineFromStartFrame(retValue);
            return retValueTimeLine;
        }
        */


        bool isHourTimeOfBLaterThanOrEqualToA(DateTimeOffset DateTimeOffsetA, DateTimeOffset DateTimeOffsetB)
        {
            DateTimeOffset refDateTimeOffsetA = new DateTimeOffset(1, 1, 1, DateTimeOffsetA.Hour, DateTimeOffsetA.Minute, DateTimeOffsetA.Second, new TimeSpan());
            DateTimeOffset refDateTimeOffsetB = new DateTimeOffset(1, 1, 1, DateTimeOffsetB.Hour, DateTimeOffsetB.Minute, DateTimeOffsetB.Second, new TimeSpan());
            return refDateTimeOffsetB >= refDateTimeOffsetA;
        }


        public List<Tuple<DayOfWeek, RestrictionTimeLine>> getActiveDays()
        {
            return NoNull_DaySelections.ToList();
        }
        

    }
}
