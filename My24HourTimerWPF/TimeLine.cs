using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    public class TimeLine
    {
        protected DateTime EndTime;
        protected DateTime StartTime;
        protected BusyTimeLine[] ActiveTimeSlots;

        #region constructor
        public TimeLine()
        {
            StartTime = new DateTime(0);
            EndTime = StartTime;
            ActiveTimeSlots = new BusyTimeLine[0];
        }


        public TimeLine(DateTime MyStartTime, DateTime MyEndTime)
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

        public BusyTimeLine toBusyTimeLine(string ID)
        {
            return new BusyTimeLine(ID, this.Start, this.End);
        }
        #endregion

        #region functions

        public bool IsDateTimeWithin(DateTime MyDateTime)
        {
            if ((MyDateTime > StartTime) && (MyDateTime < EndTime))//you might need to review semantics
            {
                return true;
            }

            return false;
        }



        public bool doesTimeLineInterfere(TimeLine TimeLine0)
        {
            if (IsDateTimeWithin(TimeLine0.End) || IsDateTimeWithin(TimeLine0.Start))
            {
                return true;
            }

            return false;
        }

        public TimeLine CreateCopy()
        {
            TimeLine CopyTimeLine = new TimeLine();
            CopyTimeLine.EndTime = new DateTime(EndTime.Ticks);
            CopyTimeLine.StartTime = new DateTime(StartTime.Ticks);
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

        public void AddBusySlots(BusyTimeLine MyBusySlot)//further update will be to check if it interferes
        {
            List<BusyTimeLine> MyListOfActiveSlots = ActiveTimeSlots.ToList();//;
            MyListOfActiveSlots.Add(MyBusySlot);
            ActiveTimeSlots = MyListOfActiveSlots.ToArray();
        }

        public void AddBusySlots(BusyTimeLine[] MyBusySlot)//further update will be to check if it interferes
        {
            var MyNewArray = ActiveTimeSlots.Concat(MyBusySlot);
            ActiveTimeSlots = MyNewArray.ToArray();
        }

        public void MergeTimeLines(TimeLine OtherTimeLine)
        {
            AddBusySlots(OtherTimeLine.OccupiedSlots);
        }

        public List<BusyTimeLine> getBusyTimeLineWithinSlots(DateTime StartTime, DateTime EndTime)
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

            DateTime PreceedingDateTime = StartTime;

            foreach (BusyTimeLine MyActiveSlot in ActiveTimeSlots)
            {
                TimeLine FreeSpot = new TimeLine(PreceedingDateTime, MyActiveSlot.Start);
                if (FreeSpot.TimelineSpan.Ticks > 0)
                {
                    ListOfFreeSpots.Add(FreeSpot);
                }

                PreceedingDateTime = MyActiveSlot.End;
            }
            ListOfFreeSpots.Add(new TimeLine(ActiveTimeSlots[ActiveTimeSlots.Length - 1].End, EndTime));

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

        public DateTime Start
        {
            get
            {
                return StartTime;
            }
        }

        public DateTime End
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
                return EndTime - DateTime.Now;
            }
        }
        public TimeSpan TimeTillStart
        {
            get
            {
                return StartTime - DateTime.Now;
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
                DateTime LatestDeadlineOfClashing = new DateTime(0);
                BusyTimeLine busyTimeSlotWithLatestEnd = new BusyTimeLine();


                Dictionary<int, List<BusyTimeLine>> myClashingTimelines = this.ClashingTimelines;
                int[] ArrayOfIndex = myClashingTimelines.Keys.ToArray();
                if (ActiveTimeSlots.Length < 1)
                {
                    return new TimeSpan(0);
                }

                DateTime ReferenceStartTime = ActiveTimeSlots[0].Start;
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

                DateTime ReferenceTimeLine = ActiveTimeSlots[0].Start;

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

    }
}
