using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class TimeLineRestricted:TimeLine
    {
        RestrictionProfile RestrictionInfo;
        Dictionary<ulong, HashSet<TimeLine>> DayOfYearToTimeLine;
        DateTimeOffset NonViableStart;
        DateTimeOffset NonViableEnd;
        TimeSpan RangeSpanInfo;
        bool isPlausible = false;
        ulong EarliestDayIndex;
        ulong LatestDayIndex;

        public TimeLineRestricted(DateTimeOffset StartData, DateTimeOffset EndData, RestrictionProfile RestrictionData)
        {
            RestrictionInfo = RestrictionData;
            NonViableStart = StartData;
            NonViableEnd = EndData;
            initialize();
        }

        void initialize()
        {
            isPlausible = false;
            DayOfYearToTimeLine = new Dictionary<ulong, HashSet<TimeLine>>();
            List<TimeLine> AllTImeLines = RestrictionInfo.getAllTimePossibleTimeFrames(new TimeLine(NonViableStart, NonViableEnd)).OrderBy(obj => obj.Start).ToList();

            ILookup<ulong, TimeLine> lookUpData0 = AllTImeLines.ToLookup(obj => ReferenceNow.getDayIndexFromStartOfTime(obj.Start), obj => obj);
            ILookup<ulong, TimeLine> lookUpData1 = AllTImeLines.ToLookup(obj => ReferenceNow.getDayIndexFromStartOfTime(obj.End), obj => obj);


            //ILookup<ulong,TimeLine>lookUpData =.ToLookup(obj=>ReferenceNow.getDayIndexFromStartOfTime(obj.Start),obj=>obj);
            RangeSpanInfo=new TimeSpan(0);
            long TotalTicks =0;
            foreach (IGrouping<ulong,TimeLine> eachIGrouping in lookUpData0)
            {
                DayOfYearToTimeLine.Add(eachIGrouping.Key, new HashSet<TimeLine>(lookUpData0[eachIGrouping.Key]));

                TotalTicks += lookUpData0[eachIGrouping.Key].Sum(obj => obj.TimelineSpan.Ticks);
            }

            foreach (IGrouping<ulong, TimeLine> eachIGrouping in lookUpData1)
            {
                if (DayOfYearToTimeLine.ContainsKey(eachIGrouping.Key))
                { 
                    foreach(TimeLine eachTImeLine in lookUpData1[eachIGrouping.Key] )
                    {
                        DayOfYearToTimeLine[eachIGrouping.Key].Add(eachTImeLine);
                    }
                }
                else
                {
                    DayOfYearToTimeLine.Add(eachIGrouping.Key, new HashSet<TimeLine>(lookUpData1[eachIGrouping.Key]));
                }
                //DayOfYearToTimeLine.Add(eachIGrouping.Key, );
                //TotalTicks += lookUpData1[eachIGrouping.Key].Sum(obj => obj.TimelineSpan.Ticks);
            }


            DayOfYearToTimeLine = DayOfYearToTimeLine.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
            RangeSpanInfo = TimeSpan.FromTicks(TotalTicks);
            if (DayOfYearToTimeLine.Count > 0)
            {
                TimeLine FirstTImeLine = DayOfYearToTimeLine.First().Value.First();
                TimeLine LAstTimeLine = DayOfYearToTimeLine.Last().Value.Last();
                StartTime = FirstTImeLine.Start;
                EndTime = LAstTimeLine.End;
                EarliestDayIndex = ReferenceNow.getDayIndexFromStartOfTime(StartTime);
                LatestDayIndex = ReferenceNow.getDayIndexFromStartOfTime(LAstTimeLine.End);
                isPlausible = true;
            }

            
            

        }

        /// <summary>
        /// Just adds elements to the free spots withou checking id it falls within a usable field
        /// </summary>
        /// <param name="MyBusySlot"></param>
        public override void AddBusySlots(BusyTimeLine MyBusySlot)
        {
            base.AddBusySlots(MyBusySlot);
        }

        public override void AddBusySlots(IEnumerable<BusyTimeLine> MyBusySlot)
        {
            base.AddBusySlots(MyBusySlot);
        }

        public override bool doesTimeLineInterfere(TimeLine TimeLineData)
        {
            ulong StartIndex = ReferenceNow.getDayIndexFromStartOfTime(TimeLineData.Start);
            ulong EndIndex = ReferenceNow.getDayIndexFromStartOfTime(TimeLineData.End);

            StartIndex = EarliestDayIndex > StartIndex ? EarliestDayIndex : StartIndex;
            EndIndex = LatestDayIndex < EndIndex ? LatestDayIndex : EndIndex;

            bool retValue = false;

            bool breakOuter = false;
            for (ulong i = StartIndex; i <= EndIndex; i++)
            {
                if (DayOfYearToTimeLine.ContainsKey(i))
                {
                    foreach (TimeLine eachTimeLine in DayOfYearToTimeLine[i])
                    {
                        if (eachTimeLine.doesTimeLineInterfere(TimeLineData))
                        {
                            retValue = true;
                            breakOuter = true;
                            break;
                        }
                        else
                        {
                            if (TimeLineData.doesTimeLineInterfere(eachTimeLine))
                            {
                                retValue = true;
                                breakOuter = true;
                                break;
                            }
                        }
                    }
                }
                else 
                {
                    HashSet<TimeLine> AllTimelines = new HashSet<TimeLine>(DayOfYearToTimeLine.Where(obj => obj.Key < StartIndex).SelectMany(obj => obj.Value));
                    foreach (TimeLine eachTimeLine in AllTimelines)
                    {
                        if (eachTimeLine.doesTimeLineInterfere(TimeLineData))
                        {
                            retValue = true;
                            breakOuter = true;
                            break;
                        }
                        ;
                    }
                }

                if (breakOuter)
                {
                    break;
                }
                
            }
            return retValue;
        }

        public override TimeLine[] getAllFreeSlots()
        {
            return base.getAllFreeSlots();
        }


        public override TimeLineWithEdgeElements[] getAllFreeSlotsWithEdges()
        {
            return base.getAllFreeSlotsWithEdges();
        }

        public override List<BusyTimeLine> getBusyTimeLineWithinSlots(DateTimeOffset StartTime, DateTimeOffset EndTime)
        {
            return base.getBusyTimeLineWithinSlots(StartTime, EndTime);
        }

        public override TimeLine InterferringTimeLine(TimeLine PossibleTimeLine)
        {
            TimeLine retValue = base.InterferringTimeLine(PossibleTimeLine); ;

            if (retValue != null)
            {
                TimeLineRestricted retValueRestricted = new TimeLineRestricted(retValue.Start, retValue.End, RestrictionInfo);
                if (retValueRestricted.RangeSpanInfo.Ticks == 0)
                {
                    return null;
                }
                return retValueRestricted;
            }
            else
            {
                return retValue;
            }
        }

        public override List<BusyTimeLine> getBusyTimeLineWithinSlots(TimeLine MyTimeLineRange)
        {
            return base.getBusyTimeLineWithinSlots(MyTimeLineRange);
        }


        public override BusyTimeLine toBusyTimeLine(string ID)
        {
            return base.toBusyTimeLine(ID);
        }

        public override void MergeTimeLineBusySlots(TimeLine OtherTimeLine)
        {
            base.MergeTimeLineBusySlots(OtherTimeLine);
        }
        /// <summary>
        /// Checks if TimeLine is within one of the free spots available
        /// </summary>
        /// <param name="MyTimeLine"></param>
        /// <returns></returns>
        public override bool IsTimeLineWithin(TimeLine MyTimeLine)
        {
            List<TimeLine> timeFrames = RestrictionInfo.getAllTimePossibleTimeFrames(new TimeLine (this.StartTime,this.EndTime)).Where(obj=>obj.TimelineSpan >= MyTimeLine.TimelineSpan).ToList();

            foreach (TimeLine eachTimeLine in timeFrames)
            { 
                if(eachTimeLine.IsTimeLineWithin(MyTimeLine))
                {
                    return true;
                }
            }




            return false;
        }

        /// <summary>
        /// Checks if DateTime is withi one of the active frames
        /// </summary>
        /// <param name="MyDateTime"></param>
        /// <returns></returns>
        public override bool IsDateTimeWithin(DateTimeOffset MyDateTime)
        {
            TimeLine timeFrame = RestrictionInfo.getEarliestStartTimeWithinAFrameAfterRefTime(MyDateTime);
            TimeLine tempFrame = new TimeLine(this.Start, this.EndTime);
            tempFrame = tempFrame.InterferringTimeLine(timeFrame);
            if (tempFrame!=null)
            {
                return tempFrame.IsDateTimeWithin(MyDateTime) || timeFrame.Start == MyDateTime || timeFrame.End == MyDateTime;
            }
            return false;
            
        }

        /// <summary>
        /// gets All time Frames for the designated Restricted timeframes
        /// </summary>
        /// <returns></returns>
        public List<TimeLine>getTimeFrames()
        {
            List<TimeLine> retValue= DayOfYearToTimeLine.SelectMany(obj => obj.Value).ToList();
            return retValue;
        }
    }
}
