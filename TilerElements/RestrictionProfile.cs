using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace TilerElements
{
    /// <summary>
    /// Reperesents the restriction of profile. This is to be used with events. There is no to be a default constructor for semantic purposes
    /// </summary>
    public class RestrictionProfile: IUndoable
    {
        protected string _Id = Guid.NewGuid().ToString();
        static DateTimeOffset SundayDate = new DateTimeOffset(2015, 3, 15, 0, 0, 0, new TimeSpan());
        public static readonly DayOfWeek[] AllDaysOfWeek = new DayOfWeek[7] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
        protected List<RestrictionDay> _DaySelection;
        protected List<RestrictionDay> _NoNull_DaySelections;
        protected string _UndoId;
        /// <summary>
        /// given a series of days to their restricted timelines, this data member holds the number of days the restricted timeline overlaps. Note even though it always has seven memebers the first element in the tuple is always in respect to the index of nonull dayselections.
        /// The first element in the tuple represents the day to which the restricting timeline is associated. The second element in the tuple is the number of days from the day of origin for which this will cover.
        /// So for example if you have a restricted profile with just one restricted timeline of Saturday => "4:00 PM - 10:00AM". Note: this is from Saturday, from 4:00pm to sunday 10:00 am
        /// This will translate to DayOfWeekToOverlappingIndexes having [[<0,1>],[],[],[],[],[],[<0,1>]].
        /// Notice there are seven internal arrays. Each internal array is for each day of the week. Where 0(zero) is for Sunday and 6 is Saturday. Only the arrays for sunday and saturday have elements in them.
        /// The way to read each element in the internal array:
        /// <0,1> is in element 0 means. 0 index means it is on Sunday. There is a restrictive timeline that interacts with Sunday.
        /// The Tuple Item1 '0' means the origin is from the day in the 0 index of NoNull_DaySelections.
        /// The Tuple Item2 '1' means it is overlaps 1 day after the original. So it overlaps one more day, which is in this case Sunday. The original day in this case is Saturday.
        /// </summary>
        protected List<Tuple<int,int>>[] _DayOfWeekToOverlappingIndexes = new List<Tuple<int,int>>[7];


        bool[] _ActiveDays = new bool[7];
        DayOfWeek _startDayOfWeek = DayOfWeek.Monday;
        #region constructor
        protected RestrictionProfile()
        {

        }
        public RestrictionProfile(int typeOfRestriction, DayOfWeek beginningDayOfWorkWeek, DateTimeOffset Start, DateTimeOffset End)
        {
            _DaySelection = new RestrictionDay[7].ToList();
            switch (typeOfRestriction)
            {
                case 0://Work week
                    {
                        TimeSpan FiveDays = new TimeSpan(4, 0, 0, 0);
                        Start = new DateTimeOffset(SundayDate.Year, SundayDate.Month, SundayDate.Day, Start.Hour, Start.Minute, Start.Second, new TimeSpan()).AddDays((int)beginningDayOfWorkWeek);
                        DateTimeOffset tempEnd = Start.Add(FiveDays);
                        tempEnd = new DateTimeOffset(tempEnd.Year, tempEnd.Month, tempEnd.Day, End.Hour, End.Minute, End.Second, new TimeSpan());
                        FiveDays = tempEnd - Start;
                        _NoNull_DaySelections = new RestrictionDay[1].ToList();
                        for (int i = (int)beginningDayOfWorkWeek; i < 2; i++)//starting i from 1 because workweek starts on Monday and Monday index is 1
                        {
                            _DaySelection[i] = new RestrictionDay(AllDaysOfWeek[i], new RestrictionTimeLine(Start, FiveDays));
                            _NoNull_DaySelections[0] = _DaySelection[i];
                        }
                    }
                    break;
                default://Work week and Office Hours
                    {
                        _NoNull_DaySelections = new RestrictionDay[5].ToList();
                        for (int i = (int)beginningDayOfWorkWeek, j = 0; j < _NoNull_DaySelections.Count; i++, j++)//starting i from 1 because workweek starts on Monday and Monday index is 1
                        {
                            _DaySelection[i] = new RestrictionDay(AllDaysOfWeek[i], new RestrictionTimeLine(Start, End));
                            _NoNull_DaySelections[j] = _DaySelection[i];
                        }
                    }
                    break;
            }
            InitializeOverLappingDictionary();
        }
        public RestrictionProfile(DateTimeOffset RestrictStart, TimeSpan RestrictDuration)
        {
            _DaySelection = new RestrictionDay[7].ToList();
            for (int i = 0; i < _DaySelection.Count; i++)
            {
                _DaySelection[i] = new RestrictionDay(AllDaysOfWeek[i], new RestrictionTimeLine(RestrictStart, RestrictDuration));
                _ActiveDays[i] = true;
            }
            _NoNull_DaySelections = _DaySelection.ToList();
            InitializeOverLappingDictionary();
        }

        public RestrictionProfile(IEnumerable<DayOfWeek> DaysOfWeekSelection, RestrictionTimeLine constrictionProgile)
        {
            _DaySelection = new RestrictionDay[7].ToList();
            DaysOfWeekSelection = DaysOfWeekSelection.OrderBy(obj => obj).ToArray();
            _startDayOfWeek = ((DayOfWeek[])DaysOfWeekSelection)[0];
            foreach (DayOfWeek eachDayOfWeek in DaysOfWeekSelection)
            {
                int DayOfWeekInt = (int)eachDayOfWeek;
                _DaySelection[DayOfWeekInt] = new RestrictionDay(eachDayOfWeek, constrictionProgile);
            }

            _NoNull_DaySelections = _DaySelection.Where(obj => obj != null).ToList();
            InitializeOverLappingDictionary();
        }

        public RestrictionProfile(IEnumerable<DayOfWeek> DaysOfWeekSelection, IEnumerable<RestrictionTimeLine> constrictionProgiles)
        {
            _DaySelection = new RestrictionDay[7].ToList();
            if ((constrictionProgiles.Count() == DaysOfWeekSelection.Count()) && (DaysOfWeekSelection.Count() > 0))
            {
                DaysOfWeekSelection = DaysOfWeekSelection.OrderBy(obj => obj).ToArray();
                _startDayOfWeek = ((DayOfWeek[])DaysOfWeekSelection)[0];
                List<DayOfWeek> AllDay = DaysOfWeekSelection.ToList();
                List<RestrictionTimeLine> RestrictingTimeLines = constrictionProgiles.ToList();

                for (int i = 0; i < AllDay.Count; i++)
                {

                    DayOfWeek eachDayOfWeek = AllDay[i];
                    RestrictionTimeLine RestrictingFrame = RestrictingTimeLines[i];
                    int DayOfWeekInt = (int)eachDayOfWeek;
                    _DaySelection[DayOfWeekInt] = new RestrictionDay(eachDayOfWeek, RestrictingFrame);
                }

                _NoNull_DaySelections = _DaySelection.Where(obj => obj != null).ToList();
                InitializeOverLappingDictionary();
            }
            else
            {
                if (DaysOfWeekSelection.Count() < 1)
                {
                    throw new Exception("There are zero contriction frames");
                }
                throw new Exception("Number of days not equal to number of RestrictionTimeLine ");
            }
        }

        #endregion constructor
        
        #region undoMembers
        public ICollection<RestrictionDay> UndoDaySelection;
        public ICollection<RestrictionDay> UndoNoNull_DaySelection;
        #endregion
        
        #region functions
        public void InitializeOverLappingDictionary()
        {
            TimeSpan twentyFourHourSpan = new  TimeSpan(1,0,0,0,0);
            for (int i = 0; i < _DayOfWeekToOverlappingIndexes.Length; i++)
            {
                _DayOfWeekToOverlappingIndexes[i] = new List<Tuple<int, int>>();
            }
            var DaySelectionCpy = _DaySelection.Where(obj => obj != null).OrderBy(daySelection => daySelection.WeekDay).ToList();
            _DaySelection = new RestrictionDay[7].ToList();
            foreach ( RestrictionDay day in DaySelectionCpy)
            {
                DayOfWeek eachDayOfWeek = day.WeekDay;
                int DayOfWeekInt = (int)eachDayOfWeek;
                _DaySelection[DayOfWeekInt] = day;
            }
            

            _NoNull_DaySelections = _NoNull_DaySelections.OrderBy(daySelection => daySelection.WeekDay).ToList();

            for (int i = 0; i < _NoNull_DaySelections.Count; i++)
            {
                DateTimeOffset myStartTime=_NoNull_DaySelections[i].RestrictionTimeLine.getSampleTestTIme();
                DateTimeOffset StartOfNextDay = myStartTime.AddDays(1);
                StartOfNextDay = new DateTimeOffset(StartOfNextDay.Year, StartOfNextDay.Month, StartOfNextDay.Day, 0, 0, 0, new TimeSpan());
                TimeSpan RestOfDaySpan = StartOfNextDay - myStartTime;
                TimeSpan RestrictionSpanLeft = _NoNull_DaySelections[i].RestrictionTimeLine.Span - RestOfDaySpan;
                int NumberOfDaysExtra = (int)Math.Ceiling( (double)RestrictionSpanLeft.Ticks / (double)twentyFourHourSpan.Ticks);
                int StartingIndex = (int)_NoNull_DaySelections[i].WeekDay;
                _DayOfWeekToOverlappingIndexes[StartingIndex].Add(new Tuple<int, int>(i, NumberOfDaysExtra));
                StartingIndex += 1;//increasing by one so that the index can begin on the next day
                for(int j=0;j<NumberOfDaysExtra;j++)
                {
                    int Myindex= (StartingIndex+j)%7;
                    _DayOfWeekToOverlappingIndexes[Myindex].Add(new Tuple<int, int>(i, NumberOfDaysExtra));
                }
            }
        }

        public RestrictionProfile createCopy()
        {
            RestrictionProfile retValue = new RestrictionProfile();
            retValue._DaySelection = this._DaySelection.Select(obj => obj == null ? obj : new RestrictionDay(obj.WeekDay, obj.RestrictionTimeLine.createCopy())).ToList();
            retValue._NoNull_DaySelections = this._DaySelection.Where(obj=>obj!=null).Select(obj => new RestrictionDay(obj.WeekDay, obj.RestrictionTimeLine.createCopy())).ToList();
            retValue._DayOfWeekToOverlappingIndexes = this._DayOfWeekToOverlappingIndexes.ToArray();
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
            Start =  getEarliestStartTimeWithinAFrameAfterRefTime(Start).Item1.Start;

            DateTimeOffset End = RefTimeline.End;
            End = getLatestEndTimeWithinFrameBeforeRefTime(End).Item1.End;

            TimeLine retValue = new TimeLine(Start, End);
            return retValue;
        }

        public Tuple<TimeLine, DateTimeOffset> getLatestActiveTimeFrameBeforeEnd(IDefinedRange RefTimeline)
        {
            var tuple = getLatestEndTimeWithinFrameBeforeRefTime(RefTimeline.End);
            TimeLine LatestFrame = tuple.Item1;
            DateTimeOffset End = LatestFrame.End;
            DateTimeOffset Start = LatestFrame.Start;
            Start = RefTimeline.Start>Start?RefTimeline.Start :Start;

            Tuple<TimeLine, DateTimeOffset> retValue = new Tuple<TimeLine, DateTimeOffset>(new TimeLine(Start, End), tuple.Item2);
            return retValue;
        }

        public Tuple <TimeLine, DateTimeOffset> getEarliestActiveFrameAfterBeginning(IDefinedRange RefTimeline)
        {
            var tuple = getEarliestStartTimeWithinAFrameAfterRefTime(RefTimeline.Start);
            TimeLine EarliestFrame = tuple.Item1;

            DateTimeOffset Start = EarliestFrame.Start;
            DateTimeOffset End = EarliestFrame.End;
            //End = getLatestEndTimeFrameBorder(End);
            End = RefTimeline.End < End ? RefTimeline.End : End;

            Tuple<TimeLine, DateTimeOffset> retValue = new Tuple<TimeLine, DateTimeOffset>( new TimeLine(Start, End), tuple.Item2);
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
        /// function tries to get the time full frame that encases "StartData". If there isnt it returns null
        /// </summary>
        /// <param name="StartData"></param>
        /// <returns></returns>

        public TimeLine getEncasingFullFrame(DateTimeOffset StartData)
        {
            int DayOfWeekInt = (int)StartData.DayOfWeek;
            TimeLine retValue;
            DateTimeOffset refSTart = new DateTimeOffset(1, 1, 1, StartData.Hour, StartData.Minute, StartData.Second, new TimeSpan());

            List<Tuple<int, int>> AllInterFerringIndexes = _DayOfWeekToOverlappingIndexes[DayOfWeekInt];;
            TimeLine DayFramTImeLine;
            for (int i = 0; i < AllInterFerringIndexes.Count; i++)
            {
                Tuple<int, int> myTUple = AllInterFerringIndexes[i];
                DayOfWeek noNullDayOfweek = _NoNull_DaySelections[myTUple.Item1].WeekDay;
                if (StartData.DayOfWeek == DayOfWeek.Sunday && noNullDayOfweek == DayOfWeek.Saturday)
                {
                    DateTimeOffset readjustToPreviousWeek = StartData.AddDays(-7);
                    readjustToPreviousWeek = new DateTimeOffset(readjustToPreviousWeek.Year, readjustToPreviousWeek.Month, readjustToPreviousWeek.Day, StartData.Hour, StartData.Minute, StartData.Second, new TimeSpan());
                    DayFramTImeLine = getTimeLinesFromTuple(_NoNull_DaySelections[myTUple.Item1], readjustToPreviousWeek);

                }
                else
                {
                    DayFramTImeLine = getTimeLinesFromTuple(_NoNull_DaySelections[myTUple.Item1], StartData);
                }

                if (DayFramTImeLine.IsDateTimeWithin(StartData) || (DayFramTImeLine.Start == StartData) || (DayFramTImeLine.End == StartData))
                {
                    retValue = DayFramTImeLine;
                    return retValue;
                }
            }

            return null;
        }

        /// <summary>
        /// Function gets all the available Restriction frames within a timeLine. So if reftimeline was March 2 2015 monday 7:00am - 10:00am and Day selections were MTW 9:00a- 9:00p. It'll return only March 2 9:00am - 10:00am.
        /// </summary>
        /// <param name="RefTimeLine"></param>
        /// <returns></returns>

        public List<TimeLine> getAllNonPartialTimeFrames(IDefinedRange RefTimeLine)
        {
            var FirstFrame = getEarliestActiveFrameAfterBeginning(RefTimeLine);
            var LastFrame = getLatestActiveTimeFrameBeforeEnd(RefTimeLine);

            List<TimeLine>[] TimeLinesPerDaySelection = _NoNull_DaySelections.Select(obj=>new List<TimeLine>()).ToArray();

            TimeLine EncasingFrame = getEncasingFullFrame(RefTimeLine.Start);

            DayOfWeek InitializigWeekday = EncasingFrame!=null? EncasingFrame.Start.DayOfWeek: getEarliestFullframe(FirstFrame.Item1).Start.DayOfWeek;

            int initializingIndex =0;
            for (; initializingIndex< _NoNull_DaySelections.Count; initializingIndex++)
            {
                if (_NoNull_DaySelections[initializingIndex].WeekDay == InitializigWeekday)
                {
                    break;
                }
            }
            int lengthOfNoNull_DaySelections = _NoNull_DaySelections.Count;
            for (int i = initializingIndex, j=0; j < _NoNull_DaySelections.Count; i++,j++)
            {
                i = (i + lengthOfNoNull_DaySelections) % lengthOfNoNull_DaySelections;
                RestrictionDay eachTuple = _NoNull_DaySelections[i];
                List<TimeLine> ListOfTimeLine = TimeLinesPerDaySelection[i];
                int DayDiff = ((eachTuple.WeekDay-FirstFrame.Item2.DayOfWeek)+7) % 7;
                DateTimeOffset refDayInCalculatedWeek = FirstFrame.Item2.AddDays(DayDiff);
                TimeLine myTimeLine = getTimeLinesFromTuple(eachTuple, refDayInCalculatedWeek);// eachTuple.Item2.getTimeLineFromStartFrame(Start);
                DateTimeOffset start = FirstFrame.Item1.Start > myTimeLine.Start ? FirstFrame.Item1.Start : myTimeLine.Start;
                DateTimeOffset end = myTimeLine.End >= LastFrame.Item1.End ? LastFrame.Item1.End : myTimeLine.End;
                myTimeLine = new TimeLine(start, end);
                if(myTimeLine.TimelineSpan.Ticks > 0)
                {
                    while (myTimeLine.Start < LastFrame.Item1.End)
                    {
                        ListOfTimeLine.Add(myTimeLine);
                        refDayInCalculatedWeek = refDayInCalculatedWeek.AddDays(7);
                        myTimeLine = eachTuple.RestrictionTimeLine.getTimeLineFromStartFrame(refDayInCalculatedWeek);
                    }
                }

            }

            List<TimeLine> retValue = TimeLinesPerDaySelection.SelectMany(obj => obj).ToList();
            if (retValue.Count>0)
            {
                TimeLine firstTimeLine = retValue[0];
                if ((firstTimeLine.Start < FirstFrame.Item1.Start) && (firstTimeLine.End == FirstFrame.Item1.End))//ensures that the first frame is within RefTimeLine. THe preceding code always generates a full frame so, we need to generate a partial frame
                {
                    retValue[0] = FirstFrame.Item1;
                }
                else if ((firstTimeLine.Start < FirstFrame.Item1.Start) && (firstTimeLine.End != FirstFrame.Item1.End))
                {
                    throw new Exception("There is something wrong with the implementation of getAllNonPartialTimeFrames, the first frame should always be part of the set");
                }

                TimeLine lastTimeLine = retValue.Last();
                if ((lastTimeLine.End > LastFrame.Item1.End)&& (lastTimeLine.Start == LastFrame.Item1.Start))
                {
                    retValue[retValue.Count - 1] = LastFrame.Item1;
                }
                else if ((lastTimeLine.End > LastFrame.Item1.End) && (lastTimeLine.Start != LastFrame.Item1.Start))
                {
                    throw new Exception("There is something wrong with the implementation of getAllNonPartialTimeFrames, the last frame should always be part of the established set");
                }
            }
            else
            {
                retValue = new List<TimeLine>();
                if (FirstFrame.Item1.isEqualStartAndEnd(LastFrame.Item1))
                {
                    if (FirstFrame.Item1.TimelineSpan.Ticks > 0)
                    {
                        retValue.Add(FirstFrame.Item1);
                    }

                }
                else {
                    if (FirstFrame.Item1.TimelineSpan.Ticks > 0)
                    {
                        retValue.Add(FirstFrame.Item1);
                    }

                    if (LastFrame.Item1.TimelineSpan.Ticks > 0)
                    {
                        retValue.Add(LastFrame.Item1);
                    }
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
        /// function tries to select earliest time after the RefStart time that borders the beginning of a restriction frame. 
        /// So Assuming you had frame Nov 11 2015 Mon 11a-2p, Nov 13 2015 Wed 11a-2p. And RefStart is Nov 11 2015 Mon 12p, it'll pick Nov 13 2015 Wed 11a because its a new border frame. 
        /// It didn't select Mon's because it was past the start of the frame
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
            RestrictionDay selectedDay;
            for (int i = 0; i < 7; i++, StartIndexOfDayOfweek++)
            {
                int OffSetStartIndex = StartIndexOfDayOfweek % 7;
                selectedDay = _DaySelection[OffSetStartIndex];
                if (selectedDay != null)
                {
                    DayDiff = (((int)selectedDay.WeekDay - IniStartIndexOfDayIndex) + 7) % 7;
                    DateTimeOffset newTime = Start.AddDays(DayDiff);
                    TimeLine currentFrame = getTimeLinesFromTuple(selectedDay, newTime);
                    newTime = currentFrame.Start;
                    if (newTime >= RefStart)
                    {
                        retValue = currentFrame;
                        return retValue;
                    }
                    preceedingCycleFrame = currentFrame;
                }
            }
            preceedingCycleFrame = new TimeLine(preceedingCycleFrame.Start.AddDays(7), preceedingCycleFrame.End.AddDays(7));
            return preceedingCycleFrame;
        }
        
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
            RestrictionDay selectedDay;
            for (int i = 7 ; i > 0; i--, StartIndexOfDayOfweek--)
            {
                int OffSetStartIndex = (StartIndexOfDayOfweek + 14) % 7;
                selectedDay = _DaySelection[OffSetStartIndex];
                if (selectedDay != null)
                {


                    DayDiff = (((int)selectedDay.WeekDay) - (IniStartIndexOfDayIndex + 7)) % 7;//gets the delta to be added to the new 
                    DateTimeOffset newTime = RefTime.AddDays(DayDiff);
                    TimeLine currentFrame ;

                    if((int)selectedDay.WeekDay<=(int)RefTime.DayOfWeek)
                    {
                        currentFrame = getTimeLinesFromTuple(selectedDay, RefTime);
                    }
                    else
                    {
                        currentFrame = getTimeLinesFromTuple(selectedDay, RefTime.AddDays(-7));
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
        }



        /// <summary>
        /// function tries to select Latest time before or on the RefTime time that borders the End of a restriction frame. 
        /// So Assuming you had frame Nov 11 2015 Mon 11a-2p, Nov 13 2015 Wed 11a-2p. And RefTime is Nov 13 2015 Wed 12p, 
        /// it'll pick Nov 13 2015 Wed 12p because its already within a border and can simply use its position as the final position
        /// </summary>
        /// <param name="RefTime"></param>
        /// <returns>
        /// Tuple<TimeLine, DateTimeOffset> where Item1 is the timeline and Item2 is the day with which the timeline belongs.
        /// The latter item is mostly crucial when dealing with a RestrictionDay that curs through multiple days.
        /// So for example if Item1 is a timeline 4/6/2019 1:00am - 3:00am, which is on Saturday but it is based on a Friday restriction Day which is from 1:00pm -3:00am. Notice this resriction day cuts across multiple days.
        /// Based on this item2 needs to be 4/05/2019 1:00pm.
        /// </returns>
        public Tuple<TimeLine, DateTimeOffset> getLatestEndTimeWithinFrameBeforeRefTime(DateTimeOffset time)
        {
            DateTimeOffset RefTime = time;
            int DayOfWeekInt = (int)RefTime.DayOfWeek;
            Tuple<TimeLine, DateTimeOffset> retValue;
            DateTimeOffset refSTart = new DateTimeOffset(1, 1, 1, RefTime.Hour, RefTime.Minute, RefTime.Second, new TimeSpan());

            List<Tuple<int,int>> AllInterFerringIndexes = _DayOfWeekToOverlappingIndexes[DayOfWeekInt];
            List<TimeLine> allPossibleTimeLines = new List<TimeLine>();
            TimeLine latestTimeLine;
            DateTimeOffset pivotTime = time;
            for (int i = 0; i < AllInterFerringIndexes.Count; i++)
            {
                Tuple<int,int> myTUple=AllInterFerringIndexes[i];
                int NumberOfDays = _NoNull_DaySelections[myTUple.Item1].WeekDay- pivotTime.DayOfWeek;
                //checks if the day difference is morethan the supposed span of coverage. If it is, simply subtract the supposed number of days. 
                //This will ensuree that the right week gets selected. thik Pivotday been a sunday and myTUple.item2 being a saturday. They are both on different weeks.
                //This ensures that later call to getTImeLineFromTuple. operates on the date in right week
                if (NumberOfDays >= myTUple.Item2)
                {
                    pivotTime = pivotTime.AddDays(-myTUple.Item2);
                }

                TimeLine dayFrameTImeLine = getTimeLinesFromTuple(_NoNull_DaySelections[myTUple.Item1], pivotTime);
                if (dayFrameTImeLine.IsDateTimeWithin(RefTime) || (dayFrameTImeLine.End == RefTime))// || (DayFramTImeLine.Start == RefTime))
                {
                    latestTimeLine = new TimeLine(dayFrameTImeLine.Start, RefTime);
                    retValue = new Tuple<TimeLine, DateTimeOffset>(latestTimeLine, dayFrameTImeLine.Start);
                    return retValue;
                }
                allPossibleTimeLines.Add(dayFrameTImeLine);
            }
            if (allPossibleTimeLines.Count > 0)//OPTIMIZATION or TODO figure out awayy to make the call not check through all the timelines
            {
                List<TimeLine> timelines = allPossibleTimeLines.Where(obj => obj.End <= RefTime).ToList();//gets timelines where they occur before current timeline
                if(timelines.Count>0)
                {
                    latestTimeLine = timelines.OrderByDescending(obj => obj.End).First();
                    retValue = new Tuple<TimeLine, DateTimeOffset>(latestTimeLine, latestTimeLine.Start);
                    return retValue;
                }
            }
            latestTimeLine = getLatestEndTimeFrameBorder(RefTime);
            retValue = new Tuple<TimeLine, DateTimeOffset>(latestTimeLine, latestTimeLine.Start);
            return retValue;
                
        }

        /// <summary>
        /// function tries to select earliest time after or at the RefStart time that as is within any of the restriction frame. 
        /// So Assuming you had frame Nov 11 2015 Mon 11a-2p, Nov 13 2015 Wed 11a-2p. And RefStart is Nov 11 2015 Mon 12p, it'll pick Nov 11 2015 Mon 12a because its within the Mon Frame.
        /// </summary>
        /// <param name="StartData"></param>
        /// <returns>
        /// Tuple<TimeLine, DateTimeOffset> where Item1 is the timeline and Item2 is the day with which the timeline belongs.
        /// The latter item is mostly crucial when dealing with a RestrictionDay that curs through multiple days.
        /// So for example if Item1 is a timeline 4/6/2019 1:00am - 3:00am, which is on Saturday but it is based on a Friday restriction Day which is from 1:00pm -3:00am.
        /// Notice this resriction day cuts across multiple days.
        /// Based on this item2 needs to be 4/05/2019 1:00pm.
        /// </returns>


        public Tuple<TimeLine, DateTimeOffset> getEarliestStartTimeWithinAFrameAfterRefTime(DateTimeOffset StartData)
        {
            int DayOfWeekInt = (int)StartData.DayOfWeek;
            Tuple<TimeLine, DateTimeOffset> retValue;
            DateTimeOffset refSTart = new DateTimeOffset(1, 1, 1, StartData.Hour, StartData.Minute, StartData.Second, new TimeSpan());

            List<Tuple<int,int>> AllInterFerringIndexes = _DayOfWeekToOverlappingIndexes[DayOfWeekInt];
            TimeLine DayFramTImeLine, earliestTimeLine;
            for (int i = 0; i < AllInterFerringIndexes.Count; i++)
            {
                Tuple<int,int> myTUple=AllInterFerringIndexes[i];
                DayOfWeek noNullDayOfweek = _NoNull_DaySelections[myTUple.Item1].WeekDay;
                if (StartData.DayOfWeek == DayOfWeek.Sunday && noNullDayOfweek == DayOfWeek.Saturday)
                {
                    DateTimeOffset readjustToPreviousWeek = StartData.AddDays(-7);
                    readjustToPreviousWeek = new DateTimeOffset(readjustToPreviousWeek.Year, readjustToPreviousWeek.Month, readjustToPreviousWeek.Day, StartData.Hour, StartData.Minute, StartData.Second, new TimeSpan());
                    DayFramTImeLine = getTimeLinesFromTuple(_NoNull_DaySelections[myTUple.Item1], readjustToPreviousWeek);

                }
                else
                {
                    DayFramTImeLine = getTimeLinesFromTuple(_NoNull_DaySelections[myTUple.Item1], StartData);
                }


                if (DayFramTImeLine.IsDateTimeWithin(StartData) || (DayFramTImeLine.Start == StartData))// || (DayFramTImeLine.End == StartData))
                {
                    earliestTimeLine = new TimeLine(StartData, DayFramTImeLine.End);
                    retValue = new Tuple<TimeLine, DateTimeOffset>(earliestTimeLine, DayFramTImeLine.Start);
                    return retValue;
                }
            }
            earliestTimeLine = getEarliestStartTimeFrameBorder(StartData);
            retValue = new Tuple<TimeLine, DateTimeOffset>(earliestTimeLine, earliestTimeLine.Start);
            return retValue;
        }

        /// <summary>
        /// Function generates a timeline based on myTuple and Start. The timeline represents the full frame for the provided "myTuple". The "Start" provides the time component. Note: the function of "start" is to select the week on which the rest of the evaluation will be based on, a week starts in Sunday. The next two example explain the result.
        /// Example A: "Mytuple" => Thursday, (9:00am - 3:00pm, 6hours) and Start => 05/10/2015(This is a sunday). The function will return the time frame 05/14/2015 9:00am - 05/14/2014  3:00pm. THis is because it is within the week of 05/10/2015. Based on "myTuple" it has to be a span of 6 hours hence the span from 9:00am - 3:00pm on the same day of 05/14/2015
        /// Example B: "Mytuple" => Tuesday, (9:00am - 10:00am, 25hours) and Start => 05/15/2015(This is a friday). The function will return the time frame 05/12/2015 9:00am - 05/13/2014  10:00am. THis is because it is within the week of 05/10/2015. Based on "myTuple" it has to be a span of 25 hours hence the timeline crossing over two different days. 05/12/2015 & 05/13/2015
        /// </summary>
        /// <param name="myTuple"></param>
        /// <param name="Start"></param>
        /// <returns></returns>
        TimeLine getTimeLinesFromTuple(RestrictionDay myTuple, DateTimeOffset refDate)
        {
            int DayDifference = myTuple.WeekDay - refDate.DayOfWeek;
            DateTimeOffset refStart = refDate.AddDays(DayDifference);
            TimeLine retValueOrigin = myTuple.RestrictionTimeLine.getTimeLineFromStartFrame(refStart);
            TimeLine retValueA = new TimeLine(retValueOrigin.Start.AddDays(-7), retValueOrigin.End.AddDays(-7));
            TimeLine retValueB = new TimeLine(retValueOrigin.Start.AddDays(7), retValueOrigin.End.AddDays(7));
            Tuple<TimeLine, TimeLine, TimeLine> retValue = new Tuple<TimeLine, TimeLine, TimeLine>(retValueA, retValueOrigin, retValueB);
            //return retValue;
            return retValueOrigin;
        }

        bool isHourTimeOfBLaterThanOrEqualToA(DateTimeOffset DateTimeOffsetA, DateTimeOffset DateTimeOffsetB)
        {
            DateTimeOffset refDateTimeOffsetA = new DateTimeOffset(1, 1, 1, DateTimeOffsetA.Hour, DateTimeOffsetA.Minute, DateTimeOffsetA.Second, new TimeSpan());
            DateTimeOffset refDateTimeOffsetB = new DateTimeOffset(1, 1, 1, DateTimeOffsetB.Hour, DateTimeOffsetB.Minute, DateTimeOffsetB.Second, new TimeSpan());
            return refDateTimeOffsetB >= refDateTimeOffsetA;
        }


        public List<RestrictionDay> getActiveDays()
        {
            return _NoNull_DaySelections.ToList();
        }

        public void undoUpdate(Undo undo)
        {
            foreach (RestrictionDay day in UndoDaySelection)
            {
                day.undoUpdate(undo);
            }
            foreach (RestrictionDay noNullday in UndoNoNull_DaySelection)
            {
                noNullday.undoUpdate(undo);
            }
            this._UndoId = undo.id;
            FirstInstantiation = false;
        }

        public void undo(string undoId)
        {
            foreach (RestrictionDay day in UndoDaySelection)
            {
                day.undo(undoId);
            }
            foreach (RestrictionDay noNullDay in UndoNoNull_DaySelection)
            {
                noNullDay.undo(undoId);
            }
        }

        public void redo(string undoId)
        {
            foreach (RestrictionDay day in UndoDaySelection)
            {
                day.redo(undoId);
            }
            foreach (RestrictionDay noNullDay in UndoNoNull_DaySelection)
            {
                noNullDay.redo(undoId);
            }
        }
        #endregion

        #region properties
        [NotMapped]
        public ICollection<RestrictionDay> DaySelection {
            get
            {
                return _DaySelection ?? (_DaySelection= new List<RestrictionDay>());
            }
            set
            {
                _DaySelection = value.ToList();
            }
        }
        [NotMapped]
        public ICollection<RestrictionDay> NoNull_DaySelections
        {
            get {
                return _NoNull_DaySelections ?? (_NoNull_DaySelections = new List<RestrictionDay>());
            }
            set
            {
                _NoNull_DaySelections = value.ToList();
            }
        }

        public string DaySelection_DbString
        {
            get
            {
                string retValue = JsonConvert.SerializeObject(this.DaySelection);
                return retValue;
            }
            set
            {
                if(value!= null)
                {
                    var daySelection = JsonConvert.DeserializeObject<ICollection<RestrictionDay>>(value);
                    _DaySelection = daySelection.ToList();
                }
                
            }
        }
        public string NoNull_DaySelections_DbString
        {
            get
            {
                string retValue = JsonConvert.SerializeObject(this.NoNull_DaySelections);
                return retValue;
            }
            set
            {
                if (value != null)
                {
                    var _NoNullDaySelections = JsonConvert.DeserializeObject<ICollection<RestrictionDay>>(value);
                    _NoNull_DaySelections = _NoNullDaySelections.ToList();
                }
            }
        }

        public string StartDayOfWeek
        {
            get
            {
                return _startDayOfWeek.ToString();
            }
            set
            {
                _startDayOfWeek = Utility.ParseEnum<DayOfWeek>(value);
            }
        }

        public string Id
        {
            get
            {
                return _Id ?? (_Id = Guid.NewGuid().ToString());
            }
            set
            {
                _Id = value;
            }
        }

        public virtual bool FirstInstantiation { get; set; } = true;

        public string UndoId
        {
            set
            {
                _UndoId = value;
            }
            get
            {
                return _UndoId;
            }
        }
        #endregion

    }
}
