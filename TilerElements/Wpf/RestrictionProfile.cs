﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace TilerElements.Wpf
{
    /// <summary>
    /// Reperesents the restriction of profile. This is to be used with events. There is no to be a default constructor for semantic purposes
    /// </summary>
    public class RestrictionProfile
    {
        static DateTimeOffset SundayDate = new DateTimeOffset(2015, 3, 15, 0, 0, 0, new TimeSpan());
        public static readonly DayOfWeek[] AllDaysOfWeek = new DayOfWeek[7] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
        protected Tuple<DayOfWeek, RestrictionTimeLine>[] _daySelection { get; set; } = new Tuple<DayOfWeek, RestrictionTimeLine>[7];
        protected Tuple<DayOfWeek, RestrictionTimeLine>[] _noNull_DaySelections;
        protected List<Tuple<int,int>>[] _dayOfWeekToOverlappingIndexes = new List<Tuple<int,int>>[7];
        string ID = Guid.NewGuid().ToString();

        //bool[] ActiveDays = new bool[7];
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
                        _noNull_DaySelections = new Tuple<DayOfWeek, RestrictionTimeLine>[1];
                        for (int i = (int)beginningDayOfWeek; i < 2; i++)//starting i from 1 because workweek starts on Monday and Monday index is 1
                        {
                            _daySelection[i] = new Tuple<DayOfWeek, RestrictionTimeLine>(AllDaysOfWeek[i], new RestrictionTimeLine(Start, FiveDays));
                            _noNull_DaySelections[0] = _daySelection[i];
                        }
                    }
                    break;
                default://Work week and Office Hours
                    {
                        _noNull_DaySelections = new Tuple<DayOfWeek, RestrictionTimeLine>[5];
                        for (int i = (int)beginningDayOfWeek, j = 0; j < _noNull_DaySelections.Length; i++, j++)//starting i from 1 because workweek starts on Monday and Monday index is 1
                        {
                            _daySelection[i] = new Tuple<DayOfWeek, RestrictionTimeLine>(AllDaysOfWeek[i], new RestrictionTimeLine(Start, End));
                            _noNull_DaySelections[j] = _daySelection[i];
                        }
                    }
                    break;
            }
            InitializeOverLappingDictionary();
        }
        public RestrictionProfile(DateTimeOffset RestrictStart, TimeSpan RestrictDuration)
        {
            _daySelection = new Tuple<DayOfWeek, RestrictionTimeLine>[7];
            for (int i = 0; i < _daySelection.Length; i++)
            {
                _daySelection[i] = new Tuple<DayOfWeek, RestrictionTimeLine>(AllDaysOfWeek[i], new RestrictionTimeLine(RestrictStart, RestrictDuration));
                //ActiveDays[i] = true;
            }
            _noNull_DaySelections = _daySelection.ToArray();
            InitializeOverLappingDictionary();
        }

        public RestrictionProfile(IEnumerable<DayOfWeek> DaysOfWeekSelection, RestrictionTimeLine constrictionProgile)
        {
            DaysOfWeekSelection = DaysOfWeekSelection.OrderBy(obj=>obj).ToArray();
            startDayOfWeek =((DayOfWeek[]) DaysOfWeekSelection)[0];
            foreach (DayOfWeek eachDayOfWeek in DaysOfWeekSelection)
            {
                int DayOfWeekInt = (int)eachDayOfWeek;
                _daySelection[DayOfWeekInt] = new Tuple<DayOfWeek, RestrictionTimeLine>(eachDayOfWeek, constrictionProgile);
            }

            _noNull_DaySelections = _daySelection.Where(obj => obj != null).ToArray();
            InitializeOverLappingDictionary();
        }

        public RestrictionProfile(IEnumerable<DayOfWeek> DaysOfWeekSelection, IEnumerable<RestrictionTimeLine> constrictionProgiles)
        {
            if ((constrictionProgiles.Count() == DaysOfWeekSelection.Count()) && (DaysOfWeekSelection.Count() > 0))
            {
                DaysOfWeekSelection = DaysOfWeekSelection.OrderBy(obj => obj).ToArray();
                startDayOfWeek = ((DayOfWeek[])DaysOfWeekSelection)[0];
                List<DayOfWeek> AllDay = DaysOfWeekSelection.ToList();
                List<RestrictionTimeLine> RestrictingTimeLines = constrictionProgiles.ToList();

                for(int i=0;i< AllDay.Count;i++)
                {

                    DayOfWeek eachDayOfWeek = AllDay[i];
                    RestrictionTimeLine RestrictingFrame = RestrictingTimeLines[i];
                    int DayOfWeekInt = (int)eachDayOfWeek;
                    _daySelection[DayOfWeekInt] = new Tuple<DayOfWeek, RestrictionTimeLine>(eachDayOfWeek, RestrictingFrame);
                }

                _noNull_DaySelections = _daySelection.Where(obj => obj != null).ToArray();
                InitializeOverLappingDictionary();
            }
            else
            {
                if (DaysOfWeekSelection.Count()<1)
                {
                    throw new Exception("There are zero contriction frames");
                }
                throw new Exception("Number of days not equal to number of RestrictionTimeLine ");
            }

            
        }

        #region Functions
        
        protected void InitializeOverLappingDictionary()
        {
            TimeSpan twentyFourHourSpan = new  TimeSpan(1,0,0,0,0);
            for (int i = 0; i < _dayOfWeekToOverlappingIndexes.Length; i++)
            {
                _dayOfWeekToOverlappingIndexes[i] = new List<Tuple<int, int>>();
            }


            for (int i = 0; i < _noNull_DaySelections.Length; i++)
            {
                DateTimeOffset myStartTime=_noNull_DaySelections[i].Item2.getSampleTestTIme();
                DateTimeOffset StartOfNextDay = myStartTime.AddDays(1);
                StartOfNextDay = new DateTimeOffset(StartOfNextDay.Year, StartOfNextDay.Month, StartOfNextDay.Day, 0, 0, 0, new TimeSpan());
                TimeSpan RestOfDaySpan = StartOfNextDay - myStartTime;
                TimeSpan RestrictionSpanLeft = _noNull_DaySelections[i].Item2.Span - RestOfDaySpan;
                int NumberOfDaysExtra = (int)Math.Ceiling( (double)RestrictionSpanLeft.Ticks / (double)twentyFourHourSpan.Ticks);
                int StartingIndex = (int)_noNull_DaySelections[i].Item1;
                _dayOfWeekToOverlappingIndexes[StartingIndex].Add(new Tuple<int, int>(i, NumberOfDaysExtra));
                StartingIndex += 1;//increasing by one so that the index can begin on the next day
                for(int j=0;j<NumberOfDaysExtra;j++)
                {
                    int Myindex= (StartingIndex+j)%7;
                    _dayOfWeekToOverlappingIndexes[Myindex].Add(new Tuple<int, int>(i, NumberOfDaysExtra));
                }
            }
        }

        public RestrictionProfile createCopy()
        {
            RestrictionProfile retValue = new RestrictionProfile();
            retValue._daySelection = this._daySelection.Select(obj => obj == null ? obj : new Tuple<DayOfWeek, RestrictionTimeLine>(obj.Item1, obj.Item2.createCopy())).ToArray();
            retValue._noNull_DaySelections = this._daySelection.Where(obj=>obj!=null).Select(obj => new Tuple<DayOfWeek, RestrictionTimeLine>(obj.Item1, obj.Item2.createCopy())).ToArray();
            retValue._dayOfWeekToOverlappingIndexes = this._dayOfWeekToOverlappingIndexes.ToArray();
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

            List<Tuple<int, int>> AllInterFerringIndexes = _dayOfWeekToOverlappingIndexes[DayOfWeekInt];
            int RightIndex = -1;

            for (int i = 0; i < AllInterFerringIndexes.Count; i++)
            {
                Tuple<int, int> myTUple = AllInterFerringIndexes[i];
                int NumberOfDays = StartData.DayOfWeek - _noNull_DaySelections[myTUple.Item1].Item1;
                TimeLine DayFramTImeLine = getTImeLineFromTuple(_noNull_DaySelections[myTUple.Item1], StartData);
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

            List<TimeLine>[] TimeLinesPerDaySelection = _noNull_DaySelections.Select(obj=>new List<TimeLine>()).ToArray();

            TimeLine EncasingFrame = getEncasingFullFrame(RefTimeLine.Start);

            DayOfWeek InitializigWeekday = EncasingFrame!=null? EncasingFrame.Start.DayOfWeek: getEarliestFullframe(FirstFrame).Start.DayOfWeek;

            int initializingIndex =0;
            for (; initializingIndex< _noNull_DaySelections.Length; initializingIndex++)
            {
                if (_noNull_DaySelections[initializingIndex].Item1 == InitializigWeekday)
                {
                    break;
                }
            }
            int lengthOfNoNull_DaySelections = _noNull_DaySelections.Length;
            for (int i = initializingIndex, j=0; j < _noNull_DaySelections.Length; i++,j++)
            //for (int i = initializingIndex; i < NoNull_DaySelections.Length; i++)
            {
                i = (i + lengthOfNoNull_DaySelections) % lengthOfNoNull_DaySelections;
                Tuple<DayOfWeek, RestrictionTimeLine> eachTuple = _noNull_DaySelections[i];
                List<TimeLine> ListOfTimeLine = TimeLinesPerDaySelection[i];
                int DayDiff = ((eachTuple.Item1-FirstFrame.Start.DayOfWeek)+7) % 7;
                DateTimeOffset Start = FirstFrame.Start.AddDays(DayDiff);
                //Start= eachTuple.Item2.getInjectedStartHourMinIntoDateTime(Start);
                TimeLine myTimeLine = getTImeLineFromTuple(eachTuple, Start);// eachTuple.Item2.getTimeLineFromStartFrame(Start);
                while (myTimeLine.Start < LastFrame.End)
                //while (Start < LastFrame.End)
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
                selectedDay = _daySelection[OffSetStartIndex];
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
            for (int i = 7 ; i > 0; i--, StartIndexOfDayOfweek--)
            {
                int OffSetStartIndex = (StartIndexOfDayOfweek + 14) % 7;
                selectedDay = _daySelection[OffSetStartIndex];
                if (selectedDay != null)
                {


                    DayDiff = (((int)selectedDay.Item1) - (IniStartIndexOfDayIndex + 7)) % 7;//gets the delta to be added to the new 
                    DateTimeOffset newTime = RefTime.AddDays(DayDiff);
                    TimeLine currentFrame ;

                    if((int)selectedDay.Item1<=(int)RefTime.DayOfWeek)
                    {
                        currentFrame = getTImeLineFromTuple(selectedDay, RefTime);
                    }
                    else
                    {
                        currentFrame = getTImeLineFromTuple(selectedDay, RefTime.AddDays(-7));
                    }

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
        public TimeLine getLatestEndTimeWithinFrameBeforeRefTime(DateTimeOffset time)
        {
            DateTimeOffset RefTime = time;
            int DayOfWeekInt = (int)RefTime.DayOfWeek;
            TimeLine retValue;
            DateTimeOffset refSTart = new DateTimeOffset(1, 1, 1, RefTime.Hour, RefTime.Minute, RefTime.Second, new TimeSpan());

            List<Tuple<int,int>> AllInterFerringIndexes = _dayOfWeekToOverlappingIndexes[DayOfWeekInt];
            int RightIndex = -1;
            List<TimeLine> allPossibleTimeLines = new List<TimeLine>();
            DateTimeOffset pivotTime = time;
            for (int i = 0; i < AllInterFerringIndexes.Count; i++)
            {
                Tuple<int,int> myTUple=AllInterFerringIndexes[i];
                int NumberOfDays = _noNull_DaySelections[myTUple.Item1].Item1- pivotTime.DayOfWeek;
                //checks if the day difference is morethan the supposed span of coverage. If it is, simply subtract the supposed number of days. 
                //This will ensuree that the right week gets selected. thik Pivotday been a sunday and myTUple.item2 being a saturday. They are both on different weeks.
                //This ensures that later call to getTImeLineFromTuple. operates on the date in right week
                if (NumberOfDays >= myTUple.Item2)
                {
                    pivotTime = pivotTime.AddDays(-myTUple.Item2);
                }

                TimeLine dayFrameTImeLine = getTImeLineFromTuple(_noNull_DaySelections[myTUple.Item1], pivotTime);
                if (dayFrameTImeLine.IsDateTimeWithin(RefTime) || (dayFrameTImeLine.End == RefTime))// || (DayFramTImeLine.Start == RefTime))
                {
                    retValue = new TimeLine(dayFrameTImeLine.Start, pivotTime);
                    return retValue;
                }
                allPossibleTimeLines.Add(dayFrameTImeLine);
            }
            if (allPossibleTimeLines.Count > 0)//OPTIMIZATION figure out awayy to make the call not check through all the timelines
            {
                List<TimeLine> timelines = allPossibleTimeLines.Where(obj => obj.End <= RefTime).ToList();//gets timelines where they occur before current timeline
                if(timelines.Count>0)
                {
                    retValue = timelines.OrderByDescending(obj => obj.End).First();
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

            List<Tuple<int,int>> AllInterFerringIndexes = _dayOfWeekToOverlappingIndexes[DayOfWeekInt];
            int RightIndex = -1;

            for (int i = 0; i < AllInterFerringIndexes.Count; i++)
            {
                Tuple<int,int> myTUple=AllInterFerringIndexes[i];
                int NumberOfDays = StartData.DayOfWeek - _noNull_DaySelections[myTUple.Item1].Item1;
                TimeLine DayFramTImeLine = getTImeLineFromTuple(_noNull_DaySelections[myTUple.Item1],StartData);
                if (DayFramTImeLine.IsDateTimeWithin(StartData) || (DayFramTImeLine.Start == StartData))// || (DayFramTImeLine.End == StartData))
                {
                    retValue = new TimeLine(StartData, DayFramTImeLine.End);
                    return retValue;
                }
            }

            retValue = getEarliestStartTimeFrameBorder(StartData);
            return retValue;
            
        }
        

        /// <summary>
        /// Function generates a timeline based on myTuple and Start. The timeline represents the full frame for the provided "myTuple". The "Start" provides the time component. Note: the function creates the time frame for the week of "Start",a week starts in Sunday. The next two example explain the result.
        /// Example A: "Mytuple" => Thursday, (9:00am - 3:00pm, 6hours) and Start => 05/10/2015(This is a sunday). The function will return the time frame 05/14/2015 9:00am - 05/14/2014  3:00pm. THis is because it is within the week of 05/10/2015. Based on "myTuple" it has to be a span of 6 hours hence the span from 9:00am - 3:00pm on the same day of 05/14/2015
        /// Example B: "Mytuple" => Tuesday, (9:00am - 10:00am, 25hours) and Start => 05/15/2015(This is a friday). The function will return the time frame 05/12/2015 9:00am - 05/13/2014  10:00am. THis is because it is within the week of 05/10/2015. Based on "myTuple" it has to be a span of 25 hours hence the timeline crossing over two different days. 05/12/2015 & 05/13/2015
        /// </summary>
        /// <param name="myTuple"></param>
        /// <param name="Start"></param>
        /// <returns></returns>
        TimeLine getTImeLineFromTuple(Tuple<DayOfWeek, RestrictionTimeLine> myTuple, DateTimeOffset refDate)
        {
            int DayDifference = myTuple.Item1- refDate.DayOfWeek;
            DateTimeOffset refStart = refDate.AddDays(DayDifference);
            TimeLine retValue =  myTuple.Item2.getTimeLineFromStartFrame(refStart);
            return retValue;
        }
        



        bool isHourTimeOfBLaterThanOrEqualToA(DateTimeOffset DateTimeOffsetA, DateTimeOffset DateTimeOffsetB)
        {
            DateTimeOffset refDateTimeOffsetA = new DateTimeOffset(1, 1, 1, DateTimeOffsetA.Hour, DateTimeOffsetA.Minute, DateTimeOffsetA.Second, new TimeSpan());
            DateTimeOffset refDateTimeOffsetB = new DateTimeOffset(1, 1, 1, DateTimeOffsetB.Hour, DateTimeOffsetB.Minute, DateTimeOffsetB.Second, new TimeSpan());
            return refDateTimeOffsetB >= refDateTimeOffsetA;
        }


        public List<Tuple<DayOfWeek, RestrictionTimeLine>> getActiveDays()
        {
            return _noNull_DaySelections.ToList();
        }
        #endregion

        #region Properties
        virtual public string Id
        {
            get
            {
                return ID;

            }
            set
            {
                Guid testValue;
                if (Guid.TryParse(value, out testValue))
                {
                    ID = value;
                }
                else
                {
                    throw new Exception("Invalid id for event Restriction profile");
                }

            }
        }

        virtual public List<Tuple<int, int>>[] DayOfWeekToOverlappingIndexes
        {
            get
            {
                return _dayOfWeekToOverlappingIndexes;
            }
        }

        virtual public Tuple<DayOfWeek, RestrictionTimeLine>[] NoNull_DaySelections
        {
            get
            {
                return _noNull_DaySelections;
            }
        }

        virtual public Tuple<DayOfWeek, RestrictionTimeLine>[] DaySelection
        {
            get
            {
                return _daySelection;
            }
        }
        #endregion region
    }
}