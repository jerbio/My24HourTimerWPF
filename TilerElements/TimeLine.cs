﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace TilerElements
{
    public class TimeLine : IDefinedRange
    {
        protected DateTimeOffset EndTime;
        protected DateTimeOffset StartTime;
        protected ConcurrentBag <BusyTimeLine> ActiveTimeSlots;

        #region constructor
        public TimeLine()
        {
            StartTime = new DateTimeOffset();
            EndTime = StartTime;
            ActiveTimeSlots = new ConcurrentBag<BusyTimeLine>();
            
        }


        public TimeLine(DateTimeOffset MyStartTime, DateTimeOffset MyEndTime)
        {
            StartTime = MyStartTime;
            EndTime = MyEndTime;
            ActiveTimeSlots = new ConcurrentBag<BusyTimeLine>();
            //MessageBox.Show("Error In TimeLine Arguments End Time is less than Start Time");
            if (MyEndTime <= MyStartTime)
            {
                //StartTime = MyStartTime;
                EndTime = MyStartTime;
            }
            //Debug.Assert(MyEndTime <= MyStartTime,"Error In TimeLine Arguments End Time is less than Start Time");
        }

        public override string ToString()
        {
            return this.Start.ToString() + " - " + this.End.ToString() +"::"+this.TimelineSpan.ToString();
        }

        virtual public BusyTimeLine toBusyTimeLine(string ID)
        {
            return new BusyTimeLine(ID, this.Start, this.End);
        }
        #endregion

        #region functions

        virtual public bool IsDateTimeWithin(DateTimeOffset MyDateTime)
        {
            if ((MyDateTime > StartTime) && (MyDateTime < EndTime))//you might need to review semantics
            {
                return true;
            }

            return false;
        }

        virtual public TimeLine InterferringTimeLine(TimeLine PossibleTimeLine)
        {
            DateTimeOffset InterferringStarTime;
            DateTimeOffset InterferringEndTime;
            if ((this.Start == PossibleTimeLine.Start) && (this.End == PossibleTimeLine.End))//checks if both "this and "PossibleTimeLine" are within the same range
            {
                return this.CreateCopy();
            }

            if (this.doesTimeLineInterfere(PossibleTimeLine))
            {
                InterferringStarTime = PossibleTimeLine.Start;
                if (this.Start > PossibleTimeLine.Start)
                {
                    InterferringStarTime = this.Start;
                }

                InterferringEndTime = this.End;
                if (this.End > PossibleTimeLine.End)
                {
                    InterferringEndTime = PossibleTimeLine.End;
                }

                return new TimeLine(InterferringStarTime, InterferringEndTime);
            }
            else
            {
                if (PossibleTimeLine.doesTimeLineInterfere(this))
                return PossibleTimeLine.InterferringTimeLine(this);//checks if PossibleTimeLine is the same as or bigger than "this" timeline
            }

            return null;
        }

        virtual public bool doesTimeLineInterfere(TimeLine TimeLine0)
        {
            bool retValue = false;
            if((this.Start < TimeLine0.End) && (this.End > TimeLine0.Start)){
                retValue = true;
            }
            //if (IsDateTimeWithin(TimeLine0.End) || IsDateTimeWithin(TimeLine0.Start) || ((this.Start == TimeLine0.Start) && (this.End == TimeLine0.End)))
            //{
            //    return true;
            //}

            return retValue;
        }


        public static TimeSpan sumAllTimeLineTimeSpan(ICollection<TimeLine> AllTimeLine)
        {
            TimeSpan retValue = new TimeSpan(0);

            foreach (TimeLine eachTimeLine in AllTimeLine)
            {
                retValue += eachTimeLine.TimelineSpan;
            }

            return retValue;
        }

        static bool doTimeLinesCollide(TimeLine TimelineA, TimeLine TimelineB)
        {
            return TimelineA.doesTimeLineInterfere(TimelineB) || TimelineB.doesTimeLineInterfere(TimelineA);
        }

        virtual public TimeLine CreateCopy()
        {
            TimeLine CopyTimeLine = new TimeLine();
            CopyTimeLine.EndTime = EndTime; 
            CopyTimeLine.StartTime = StartTime;
            List<BusyTimeLine> TempActiveSlotsHolder = new List<BusyTimeLine>();
            foreach (BusyTimeLine MyBusyTimeLine in ActiveTimeSlots)
            {
                TempActiveSlotsHolder.Add(MyBusyTimeLine.CreateCopy());
            }

            CopyTimeLine.ActiveTimeSlots =  new ConcurrentBag<BusyTimeLine>( TempActiveSlotsHolder);//.ToArray();
            return CopyTimeLine;
        }

        virtual public bool IsTimeLineWithin(TimeLine MyTimeLine)
        {
            if ((MyTimeLine.Start >= StartTime) && (MyTimeLine.End <= EndTime))
            {
                return true;
            }

            return false;
        }

        virtual public void AddBusySlots(BusyTimeLine MyBusySlot)//Hack Alert further update will be to check if it interferes
        {
            //List<BusyTimeLine> MyListOfActiveSlots = ActiveTimeSlots.ToList();//;
            //MyListOfActiveSlots.Add(MyBusySlot);
            //ActiveTimeSlots = MyListOfActiveSlots.ToArray();
            ActiveTimeSlots.Add(MyBusySlot);
        }

        virtual public void AddBusySlots(IEnumerable<BusyTimeLine> MyBusySlot)//Hack Alert further update will be to check if it busy slots fall within range of the timeLine
        {
            IEnumerable<BusyTimeLine> AllBusyTImeLine = MyBusySlot.Where(obj => obj.Start <= this.End);
            AllBusyTImeLine = AllBusyTImeLine.Where(obj => obj.End >= this.Start);
            //var MyNewArray = ActiveTimeSlots.Concat(AllBusyTImeLine);
            //ActiveTimeSlots = MyNewArray.ToArray();
            AllBusyTImeLine.AsParallel().ForAll(obj => ActiveTimeSlots.Add(obj));
        }


        /// <summary>
        /// Adds the busy slot of "OtherTimeLine" to the current busy slot in my timeline
        /// </summary>
        /// <param name="OtherTimeLine"></param>
        virtual public void MergeTimeLineBusySlots(TimeLine OtherTimeLine)
        {
            AddBusySlots(OtherTimeLine.OccupiedSlots);
        }

        virtual public List<BusyTimeLine> getBusyTimeLineWithinSlots(DateTimeOffset StartTime, DateTimeOffset EndTime)
        {
            TimeLine TempTimeLine = new TimeLine(StartTime, EndTime);
            List<BusyTimeLine> ActiveSlots = new List<BusyTimeLine>();
            foreach (BusyTimeLine MyBusyTimeline in ActiveTimeSlots)
            {
                if (TempTimeLine.IsTimeLineWithin(MyBusyTimeline))
                {
                    ActiveSlots.Add(MyBusyTimeline);
                }
            }
            return ActiveSlots;
        }


        /// <summary>
        /// Evaluates all the free spot in a timeline. Overlapping timelines are collapsed into the same time line to avoid duplicated calculation. So this gets only timeline with not designated busy frames
        /// </summary>
        /// <returns></returns>
        virtual public TimeLine[] getAllFreeSlots()
        {
            List<TimeLine> ListOfFreeSpots = new List<TimeLine>();
            BusyTimeLine[] ActiveTimeSlots = this.ActiveTimeSlots.ToArray();
            if (ActiveTimeSlots.Length < 1)
            {
                //List<TimeLine> SingleTimeline= new List<TimeLine>();
                ListOfFreeSpots.Add(this.CreateCopy());

                return ListOfFreeSpots.ToArray();
            }

            DateTimeOffset PreceedingDateTime = StartTime;

            Utility.ConflictEvaluation conflictEvaluation = new Utility.ConflictEvaluation(ActiveTimeSlots);

            IEnumerable<IDefinedRange> AllActiveSlots = conflictEvaluation.NonConflictingTimeRange.Concat(conflictEvaluation.ConflictingTimeRange).OrderBy(obj => obj.End);

            foreach (TimeLine MyActiveSlot in AllActiveSlots)
            {
                TimeLine FreeSpot = new TimeLine(PreceedingDateTime, MyActiveSlot.Start);
                if (FreeSpot.TimelineSpan.Ticks > 0)
                {
                    ListOfFreeSpots.Add(FreeSpot);
                }

                PreceedingDateTime = MyActiveSlot.End;
            }
            ListOfFreeSpots.Add(new TimeLine(AllActiveSlots.Last().End, EndTime));

            return ListOfFreeSpots.ToArray();
        }


        virtual public TimeLineWithEdgeElements[] getAllFreeSlotsWithEdges()
        {
            List<TimeLineWithEdgeElements> ListOfFreeSpots = new List<TimeLineWithEdgeElements>();
            BusyTimeLine[] ActiveTimeSlots = this.ActiveTimeSlots.OrderBy(busyTimeLine => busyTimeLine.Start).ToArray();
            if (ActiveTimeSlots.Length< 1)
            {
                //List<TimeLine> SingleTimeline= new List<TimeLine>();
                ListOfFreeSpots.Add(new TimeLineWithEdgeElements(this.Start,this.End,null,null));
                return ListOfFreeSpots.ToArray();
            }

            DateTimeOffset PreceedingDateTime = StartTime;
            string startEdgeElement = null;

            foreach (BusyTimeLine MyActiveSlot in ActiveTimeSlots)
            {
                TimeLineWithEdgeElements FreeSpot = new TimeLineWithEdgeElements(PreceedingDateTime, MyActiveSlot.Start, startEdgeElement, MyActiveSlot.ID);
                if (FreeSpot.TimelineSpan.Ticks > 0)
                {
                    ListOfFreeSpots.Add(FreeSpot);
                }

                PreceedingDateTime = MyActiveSlot.End;
            }
            BusyTimeLine lastElement = ActiveTimeSlots[ActiveTimeSlots.Length - 1];
            ListOfFreeSpots.Add(new TimeLineWithEdgeElements(lastElement.End, EndTime, lastElement.ID,null));

            return ListOfFreeSpots.ToArray();
        }

        virtual public List<BusyTimeLine> getBusyTimeLineWithinSlots(TimeLine MyTimeLineRange)
        {
            List<BusyTimeLine> ActiveSlots = new List<BusyTimeLine>();
            foreach (BusyTimeLine MyBusyTimeline in ActiveTimeSlots)
            {
                if (MyTimeLineRange.IsTimeLineWithin(MyBusyTimeline))
                {
                    ActiveSlots.Add(MyBusyTimeline);
                }
            }
            return ActiveSlots;
        }

        

        #endregion

        public DateTimeOffset Start
        {
            get
            {
                return StartTime;
            }
        }

        public DateTimeOffset End
        {
            get
            {
                return EndTime;
            }
        }

        virtual public TimeSpan TimelineSpan
        {
            get
            {
                return EndTime - StartTime;
            }
        }

        public virtual TimeSpan TimeTillEnd
        {
            get
            {
                return EndTime - DateTimeOffset.Now;
            }
        }
        public virtual TimeSpan TimeTillStart
        {
            get
            {
                return StartTime - DateTimeOffset.Now;
            }
        }

        public virtual TimeSpan TotalFreeSpotAvailable
        {
            get
            {
                TimeSpan retValue = new TimeSpan();
                TimeLine[] AllFreeSpot= this.getAllFreeSlots();
                foreach(TimeLine eachTimeLint in AllFreeSpot)
                {
                    retValue+=eachTimeLint.TimelineSpan;
                }
                return retValue;
            }
        }

        /// <summary>
        /// Gets all the occupied slots within the timeline. NOTE these busy slots are not ordered in anyway
        /// </summary>
        virtual public BusyTimeLine[] OccupiedSlots
        {
            set
            {
                ActiveTimeSlots = new ConcurrentBag<BusyTimeLine>(value);
            }
            get
            {
                return ActiveTimeSlots.ToArray();
            }
        }

        /// <summary>
        /// Gets all the occupied slots within the timeline. They are ordered in ascending order by start and then,in a descending order by the end time
        /// </summary>
        virtual public BusyTimeLine[] OrderedOcupiedSlots
        {
            set
            {
                ActiveTimeSlots = new ConcurrentBag<BusyTimeLine>(value);
            }
            get
            {
                return ActiveTimeSlots.OrderBy(busyTimeLine => busyTimeLine.Start).ThenByDescending(busyTimeLine => busyTimeLine.Start).ToArray();
            }
        }

        public virtual double FreeRatio
        {
            get
            {
                TimeSpan completeSpan = EndTime - StartTime;
                TimeSpan freeSpan = TotalFreeSpotAvailable;
                double retValue = (double)freeSpan.Ticks / completeSpan.Ticks;
                return retValue;
            }
        }

        public virtual double ActiveRatio
        {
            get
            {
                TimeSpan completeSpan = EndTime - StartTime;
                TimeSpan activeSpan = TotalActiveSpan;
                double retValue = (double)activeSpan.Ticks / completeSpan.Ticks;
                return retValue;
            }
        }


        virtual public TimeSpan TotalActiveSpan
        {
            get
            {
                Utility.ConflictEvaluation ConflictEvaluation = new Utility.ConflictEvaluation(this.OccupiedSlots);
                TimeSpan totalSpan = TimeSpan.FromTicks(ConflictEvaluation.ConflictingTimeRange.Concat(ConflictEvaluation.NonConflictingTimeRange).Select(timeRange => timeRange.RangeTimeLine.TimelineSpan).Sum(timeSpan => timeSpan.Ticks));
                return totalSpan;
            }
        }

        public BusyTimeLine getLatestEndBusyTime(List<BusyTimeLine> List_BusyTimekLine, BusyTimeLine LatestEndBusyTime)//yet to debug
        {

            foreach (BusyTimeLine MyBusyTimeLine in List_BusyTimekLine)
            {
                if (MyBusyTimeLine.End > LatestEndBusyTime.End)
                {
                    LatestEndBusyTime = MyBusyTimeLine;
                }
            }

            return LatestEndBusyTime;
        }
        #region Properties
        public Dictionary<int, List<BusyTimeLine>> ClashingTimelines
        {
            get
            {
                //this.ActiveTimeSlots = new ConcurrentBag<BusyTimeLine>( this.ActiveTimeSlots.OrderBy(obj => obj.Start));//crucial to correct execution
                Dictionary<int, List<BusyTimeLine>> DictionaryOfClashingTimelines = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<BusyTimeLine>>();
                TimeSpan ExpectedSum = new TimeSpan(0);
                TimeSpan CalculatedSum = new TimeSpan(0);

                BusyTimeLine[] ActiveTimeSlots = this.OrderedOcupiedSlots;//crucial to correct execution


                if (ActiveTimeSlots.Length < 2)
                {
                    try
                    {
                        if (this.IsTimeLineWithin(ActiveTimeSlots[0]))
                        {
                            return DictionaryOfClashingTimelines;
                        }
                        else
                        {
                            throw new Exception("invalid TimeLine detected");
                        }
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        return DictionaryOfClashingTimelines;
                    }
                }

                DateTimeOffset ReferenceTimeLine = ActiveTimeSlots[0].Start;

                List<BusyTimeLine> InterferringBusyTimeLines = new System.Collections.Generic.List<BusyTimeLine>();

                //foreach ( in ActiveTimeSlots)
                int i = 0;
                List<BusyTimeLine> ClashingBusyTimeline = new System.Collections.Generic.List<BusyTimeLine>();
                bool flip = false;
                for (; i < ActiveTimeSlots.Length - 1; ++i)
                {
                    BusyTimeLine MyBustTimeLine = ActiveTimeSlots[i];
                    InterferringBusyTimeLines = new System.Collections.Generic.List<BusyTimeLine>();

                    //if ((MyBustTimeLine.Start == ActiveTimeSlots[i + 1].Start))
                    //{
                    //    if (MyBustTimeLine.End < ActiveTimeSlots[i + 1].End)
                    //    {
                    //        ActiveTimeSlots[i] = ActiveTimeSlots[i + 1];
                    //        ActiveTimeSlots[i + 1] = MyBustTimeLine;
                    //        MyBustTimeLine = ActiveTimeSlots[i];
                    //        InterferringBusyTimeLines.Add(ActiveTimeSlots[i + 1]);
                    //        i = 0;//this can be optimized
                    //        DictionaryOfClashingTimelines = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<BusyTimeLine>>();

                    //        flip = true;
                    //    }
                    //    else
                    //    {
                    //        if (MyBustTimeLine.End == ActiveTimeSlots[i + 1].End)
                    //        {
                    //            InterferringBusyTimeLines.Add(ActiveTimeSlots[i + 1]);
                    //        }
                    //    }
                    //}


                    int j = i + 1;
                    for (j = i + 1; j < ActiveTimeSlots.Length; j++)
                    {
                        //if (flip)
                        //{
                        //    flip = false;
                        //    break;
                        //}

                        if (MyBustTimeLine.doesTimeLineInterfere(ActiveTimeSlots[j]))
                        {
                            InterferringBusyTimeLines.Add(ActiveTimeSlots[j]);
                        }

                    }

                    //if (InterferringBusyTimeLines.Count > 0)
                    {
                        if (DictionaryOfClashingTimelines.ContainsKey(i))
                        {
                            DictionaryOfClashingTimelines[i] = InterferringBusyTimeLines;
                        }
                        else
                        {
                            DictionaryOfClashingTimelines.Add(i, InterferringBusyTimeLines);
                        }
                    }



                }

                return DictionaryOfClashingTimelines;
            }
        }

        public void Empty()
        {
            
            {
                ActiveTimeSlots = new ConcurrentBag<BusyTimeLine>();
            }
        }


        public TimeLine RangeTimeLine
        {
            get 
            {
                return this;
            }
        }


        #endregion
    }
}