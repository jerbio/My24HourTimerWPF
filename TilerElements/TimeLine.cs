using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class TimeLine : IDefinedRange
    {
        protected DateTimeOffset EndTime;
        protected DateTimeOffset StartTime;
        protected BusyTimeLine[] ActiveTimeSlots;

        #region constructor
        public TimeLine()
        {
            StartTime = new DateTimeOffset();
            EndTime = StartTime;
            ActiveTimeSlots = new BusyTimeLine[0];
            
        }


        public TimeLine(DateTimeOffset MyStartTime, DateTimeOffset MyEndTime)
        {
            StartTime = MyStartTime;
            EndTime = MyEndTime;
            ActiveTimeSlots = new BusyTimeLine[0];
            //MessageBox.Show("Error In TimeLine Arguments End Time is less than Start Time");
            if (MyEndTime <= MyStartTime)
            {
                StartTime = MyStartTime;
                EndTime = MyStartTime;
            }
            //Debug.Assert(MyEndTime <= MyStartTime,"Error In TimeLine Arguments End Time is less than Start Time");
        }

        public override string ToString()
        {
            return this.Start.ToString() + " - " + this.End.ToString() +"::"+this.TimelineSpan.ToString();
        }

        public BusyTimeLine toBusyTimeLine(string ID)
        {
            return new BusyTimeLine(ID, this.Start, this.End);
        }
        #endregion

        #region functions

        public bool IsDateTimeWithin(DateTimeOffset MyDateTime)
        {
            if ((MyDateTime > StartTime) && (MyDateTime < EndTime))//you might need to review semantics
            {
                return true;
            }

            return false;
        }

        public TimeLine InterferringTimeLine(TimeLine PossibleTimeLine)
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

        public bool doesTimeLineInterfere(TimeLine TimeLine0)
        {

            if (IsDateTimeWithin(TimeLine0.End) || IsDateTimeWithin(TimeLine0.Start) || ((this.Start == TimeLine0.Start) && (this.End == TimeLine0.End)))
            {
                return true;
            }

            return false;
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

        public TimeLine CreateCopy()
        {
            TimeLine CopyTimeLine = new TimeLine();
            CopyTimeLine.EndTime = EndTime; 
            CopyTimeLine.StartTime = StartTime;
            List<BusyTimeLine> TempActiveSlotsHolder = new List<BusyTimeLine>();
            foreach (BusyTimeLine MyBusyTimeLine in ActiveTimeSlots)
            {
                TempActiveSlotsHolder.Add(MyBusyTimeLine.CreateCopy());
            }

            CopyTimeLine.ActiveTimeSlots = TempActiveSlotsHolder.ToArray();
            return CopyTimeLine;
        }

        public bool IsTimeLineWithin(TimeLine MyTimeLine)
        {
            if ((MyTimeLine.Start >= StartTime) && (MyTimeLine.End <= EndTime))
            {
                return true;
            }

            return false;
        }

        public void AddBusySlots(BusyTimeLine MyBusySlot)//Hack Alert further update will be to check if it interferes
        {
            List<BusyTimeLine> MyListOfActiveSlots = ActiveTimeSlots.ToList();//;
            MyListOfActiveSlots.Add(MyBusySlot);
            ActiveTimeSlots = MyListOfActiveSlots.ToArray();
        }

        public void AddBusySlots(IEnumerable<BusyTimeLine> MyBusySlot)//Hack Alert further update will be to check if it busy slots fall within range of the timeLine
        {
            var MyNewArray = ActiveTimeSlots.Concat(MyBusySlot);
            ActiveTimeSlots = MyNewArray.ToArray();
        }

        public void MergeTimeLines(TimeLine OtherTimeLine)
        {
            AddBusySlots(OtherTimeLine.OccupiedSlots);
        }

        public List<BusyTimeLine> getBusyTimeLineWithinSlots(DateTimeOffset StartTime, DateTimeOffset EndTime)
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

        public TimeLine[] getAllFreeSlots()
        {
            List<TimeLine> ListOfFreeSpots = new List<TimeLine>();
            if (ActiveTimeSlots.Length < 1)
            {
                //List<TimeLine> SingleTimeline= new List<TimeLine>();
                ListOfFreeSpots.Add(this);

                return ListOfFreeSpots.ToArray();
            }

            DateTimeOffset PreceedingDateTime = StartTime;


            Tuple<IEnumerable<IDefinedRange>, IEnumerable<IDefinedRange>> BusySlotsAndSeparateActiveSlots = Utility.getConflictingRangeElements(ActiveTimeSlots);

            IEnumerable<IDefinedRange> AllActiveSlots = BusySlotsAndSeparateActiveSlots.Item1.Concat(BusySlotsAndSeparateActiveSlots.Item2).OrderBy(obj => obj.End);

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


        public TimeLineWithEdgeElements[] getAllFreeSlotsWithEdges()
        {
            List<TimeLineWithEdgeElements> ListOfFreeSpots = new List<TimeLineWithEdgeElements>();
            if (ActiveTimeSlots.Length < 1)
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

        public List<BusyTimeLine> getBusyTimeLineWithinSlots(TimeLine MyTimeLineRange)
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

        public TimeSpan TimelineSpan
        {
            get
            {
                return EndTime - StartTime;
            }
        }

        public TimeSpan TimeTillEnd
        {
            get
            {
                return EndTime - DateTimeOffset.Now;
            }
        }
        public TimeSpan TimeTillStart
        {
            get
            {
                return StartTime - DateTimeOffset.Now;
            }
        }

        public TimeSpan TotalFreeSpotAvailable
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


        virtual public BusyTimeLine[] OccupiedSlots
        {
            set
            {
                ActiveTimeSlots = value;
            }
            get
            {
                return ActiveTimeSlots;
            }
        }

        public TimeSpan TotalActiveSpan//yet to debug
        {
            get
            {
                //TimeLine[] AllFreeSlots=Schedule.getAllFreeSpots_NoCompleteSchedule(this);
                //AllFreeSlots = AllFreeSlots.OrderBy(obj => obj.Start).ToArray();
                TimeSpan SumOfNoneClashing = new TimeSpan(0);
                TimeSpan SumOfClashing = new TimeSpan(0);
                DateTimeOffset LatestDeadlineOfClashing = new DateTimeOffset();;
                BusyTimeLine busyTimeSlotWithLatestEnd = new BusyTimeLine();


                Dictionary<int, List<BusyTimeLine>> myClashingTimelines = this.ClashingTimelines;
                int[] ArrayOfIndex = myClashingTimelines.Keys.ToArray();
                if (ActiveTimeSlots.Length < 1)
                {
                    return new TimeSpan(0);
                }

                DateTimeOffset ReferenceStartTime = ActiveTimeSlots[0].Start;
                busyTimeSlotWithLatestEnd = ActiveTimeSlots[0];

                foreach (int Index in ArrayOfIndex)
                {
                    if (myClashingTimelines[Index].Count < 1)//Detects if BusyTimeLine has non clashing BusyTimeLine after it
                    {
                        if (ActiveTimeSlots[Index].Start < busyTimeSlotWithLatestEnd.End)//checks if the non clashing BusyTimeLine clashes with preceeding events
                        {
                            if (!ActiveTimeSlots[Index].End.Equals(busyTimeSlotWithLatestEnd.End))
                            {
                                //this part of code should never run because, The only time a possible clashing index active slot from  the myClashingTimelines has value with a List count of zero, it means it is an isolated TimeLine. It might clash with a preceding index list however it can only clash if it has the same latest endtime with an element in its list or itself
                                throw new Exception("invalid matching Detected in Total Active Span logic. Need to debug ClashingTimelines logic to generate right matchup");
                            }
                        }
                        else
                        {
                            SumOfNoneClashing.Add(ActiveTimeSlots[Index].TimelineSpan);
                            SumOfClashing.Add(busyTimeSlotWithLatestEnd.End - ReferenceStartTime);
                        }
                    }
                    else
                    {
                        if (ActiveTimeSlots[Index].Start < busyTimeSlotWithLatestEnd.End)
                        {
                            busyTimeSlotWithLatestEnd = getLatestEndBusyTime(myClashingTimelines[Index], busyTimeSlotWithLatestEnd);
                        }
                        else
                        {
                            SumOfClashing.Add(busyTimeSlotWithLatestEnd.End - ReferenceStartTime);
                            ReferenceStartTime = ActiveTimeSlots[Index].Start;
                        }
                    }
                }

                if (ActiveTimeSlots[ActiveTimeSlots.Length - 1].Start < busyTimeSlotWithLatestEnd.End)//checks if the last ActiveTimeSlot BusyTimeLine clashes with preceeding events
                {
                    if (!ActiveTimeSlots[ActiveTimeSlots.Length - 1].End.Equals(busyTimeSlotWithLatestEnd.End))
                    {
                        //this part of code should never run because, The only time a possible clashing index active slot from  the myClashingTimelines has value with a List count of zero, it means it is an isolated TimeLine. It might clash with a preceding index list however it can only clash if it has the same latest endtime with an element in its list or itself
                        throw new Exception("invalid matching Detected in Total Active Span logic. Need to debug ClashingTimelines logic to generate right matchup");
                    }
                }
                else
                {
                    SumOfNoneClashing.Add(ActiveTimeSlots[ActiveTimeSlots.Length - 1].TimelineSpan);
                }


                return SumOfClashing.Add(SumOfNoneClashing);

                //After Call to Clashing TimeLines ActiveTimeSlot gets rearranged according to startime and then endtime


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

        public Dictionary<int, List<BusyTimeLine>> ClashingTimelines//yet to debug
        {
            get
            {
                ActiveTimeSlots = ActiveTimeSlots.OrderBy(obj => obj.Start).ToArray();//crucial to correct execution
                Dictionary<int, List<BusyTimeLine>> DictionaryOfClashingTimelines = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<BusyTimeLine>>();
                TimeSpan ExpectedSum = new TimeSpan(0);
                TimeSpan CalculatedSum = new TimeSpan(0);



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

                    if ((MyBustTimeLine.Start == ActiveTimeSlots[i + 1].Start))
                    {
                        if (MyBustTimeLine.End < ActiveTimeSlots[i + 1].End)
                        {
                            ActiveTimeSlots[i] = ActiveTimeSlots[i + 1];
                            ActiveTimeSlots[i + 1] = MyBustTimeLine;
                            MyBustTimeLine = ActiveTimeSlots[i];
                            InterferringBusyTimeLines.Add(ActiveTimeSlots[i + 1]);
                            i = 0;//this can be optimized
                            DictionaryOfClashingTimelines = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<BusyTimeLine>>();

                            flip = true;
                        }
                        else
                        {
                            if (MyBustTimeLine.End == ActiveTimeSlots[i + 1].End)
                            {
                                InterferringBusyTimeLines.Add(ActiveTimeSlots[i + 1]);
                            }
                        }
                    }


                    int j = i + 1;
                    for (j = i + 1; j < ActiveTimeSlots.Length; j++)
                    {
                        if (flip)
                        {
                            flip = false;
                            break;
                        }

                        if (MyBustTimeLine.IsDateTimeWithin(ActiveTimeSlots[j].Start))
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
                ActiveTimeSlots = new BusyTimeLine[0];
            }
        }

        public TimeSpan RangeSpan
        {
            get
            {
                return this.TimelineSpan;
            }
        }

        public TimeLine RangeTimeLine
        {
            get 
            {
                return this;
            }
        }

    }
}
