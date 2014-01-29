using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    class ClumpSubCalendarEvent:SubCalendarEvent
    {
        List<SubCalendarEvent> SubCalEventsOverLapWithBase;
        //List<SubCalendarEvent> NonOverLapping;
        SubCalendarEvent BaseEvent;
        //List<SubCalendarEvent> BaseClump;
        //List<ClumpSubCalendarEvent> NonOverLapping_Clump;
        ClumpSubCalendarEvent BreakOffClump;
        Dictionary<SubCalendarEvent, ClumpSubCalendarEvent> ClumpedResults;
        TimeLine BoundaryTimeLine;
        static List<List<SubCalendarEvent>> CompleteResolvedNonOverlapping;
        public static int Completed = 0;
        //List<ClumpSubCalendarEvent> OverLapping_Clump;
        //DateTime ReferenceStartTime;
        public ClumpSubCalendarEvent(List<SubCalendarEvent> Appendables, TimeLine BoundaryTimeLine)
        {
            Appendables=Appendables.OrderBy(obj => obj.getCalendarEventRange.End).ToList();
            SubCalendarEvent RelativeSubEvent = Appendables[0];
            Appendables.Remove(RelativeSubEvent);
            ClumpSubCalendarEvent myThis = new ClumpSubCalendarEvent(RelativeSubEvent, Appendables, BoundaryTimeLine.CreateCopy());
            SubCalEventsOverLapWithBase = myThis.SubCalEventsOverLapWithBase;
            //List<SubCalendarEvent> NonOverLapping;
            BaseEvent= myThis.BaseEvent;
            this.BoundaryTimeLine =myThis.BoundaryTimeLine;
            //List<SubCalendarEvent> BaseClump;
            //List<ClumpSubCalendarEvent> NonOverLapping_Clump;
            BreakOffClump= myThis.BreakOffClump;
            ClumpedResults= myThis.ClumpedResults;
        }
        
        public ClumpSubCalendarEvent(SubCalendarEvent BaseSubCalendarEvent, List<SubCalendarEvent> Appendables, TimeLine BoundaryTimeLine)
        {

            SubCalEventsOverLapWithBase = new List<SubCalendarEvent>();
            
            BaseEvent = BaseSubCalendarEvent;
            DateTime var1 = BaseEvent.getCalendarEventRange.End < BoundaryTimeLine.End ? BaseEvent.getCalendarEventRange.End : BoundaryTimeLine.End;//hack assumes base can fit within boundary
            this.BoundaryTimeLine = new TimeLine(BoundaryTimeLine.Start, var1);
            DateTime  ReferenceStartTime = var1 - BaseEvent.ActiveDuration;
            //NonOverLapping_Clump.Add(BaseSubCalendarEvent);
            ClumpedResults = new Dictionary<SubCalendarEvent, ClumpSubCalendarEvent>();
            int i = 0;
            for (; i < Appendables.Count; i++)
            {
                List<SubCalendarEvent> ReferenceClump = new List<SubCalendarEvent>();
                ReferenceClump.Add(BaseSubCalendarEvent);
                DateTime TimeLimit = ReferenceStartTime - Appendables[i].ActiveDuration;
                bool Zero = (Appendables[i].getCalendarEventRange.Start <= TimeLimit);
                bool One = (TimeLimit >= BoundaryTimeLine.Start);
                //bool Two = (BoundaryTimeLine.TimelineSpan>=Appendables[i].EventTimeLine.TimelineSpan);//this is a hack, since the length of SubcalEvent Event TimeLine is the same length of time as its busy time span


                if (Zero && One)// && Two)
                {
                    List<SubCalendarEvent> Removed_Unnecessary = Appendables.ToList();
//                    List<SubCalendarEvent> ExtendedList = ReferenceClump.ToList();
                    SubCalendarEvent RelativeSubEvent = Appendables[i];
                    
                    Removed_Unnecessary.Remove(RelativeSubEvent);

  //                  ExtendedList.Add((Appendables[i]));
                    ClumpedResults.Add(RelativeSubEvent, null);
                    
                    //NonOverLapping_Clump.Add();
                    if (Removed_Unnecessary.Count > 0)
                    {

                        ++Completed;
                        if (Completed >= 100)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    //List<SubCalendarEvent> Removed_Unnecessary = Appendables.ToList();
                    //List<SubCalendarEvent> ExtendedList = ReferenceClump.ToList();
                    //Removed_Unnecessary.Remove(Appendables[i]);
                    SubCalEventsOverLapWithBase.Add(Appendables[i]);
                }
            }

            List<SubCalendarEvent> arg1=ClumpedResults.Keys.ToList();
            //if(arg1!=null)
            {
                int j=0;
                for (;j<arg1.Count;j++)
                {
                    ClumpedResults[arg1[j]] = populateClumpedResults(BaseEvent.End, arg1[j], ClumpedResults[arg1[j]], ReferenceStartTime - arg1[j].ActiveDuration, BoundaryTimeLine);
                }

            }

            if (SubCalEventsOverLapWithBase.Count > 0)
            {
                SubCalendarEvent BreakOffSubCalEvent = SubCalEventsOverLapWithBase[0];
                SubCalEventsOverLapWithBase.Remove(BreakOffSubCalEvent);
                BreakOffClump = new ClumpSubCalendarEvent(BreakOffSubCalEvent, SubCalEventsOverLapWithBase, new TimeLine((getLeftMostPossibleStartLine(BaseEvent, BoundaryTimeLine) + BaseEvent.ActiveDuration), BoundaryTimeLine.End));
            }
            List<SubCalendarEvent> Fittable = new List<SubCalendarEvent>();
        }


        public ClumpSubCalendarEvent(DateTime ReferenceStartTime, DateTime PreCeedingBaseEndtime,List<SubCalendarEvent> Appendables, TimeLine BoundaryTimeLine)
        {
            /*
             * This Constructor is called when a continuation in Clumping is made.  Look at diagram below
             * 
             *  |     |Clump1||baseEvent||         |
             *  
             * The vertical bars represent the various boundaries(StartTime and Stop Time). Note there is no TimeSpace between Clump1 and baseEvent
             * 
             *  Clump1 in the image is a continuation in the clumping. Remember Clumping tries to build towards the Left. The ReferenceStartTime is the calculated start time of Clump1. PreCeedingBaseEndtime is the End time of the Base Event in the diagram. It will be used in the line with "BreakOffClump = new ClumpSubCalendarEvent..."
            
             */
            int i = 0;
            //NonOverLapping_Clump = new List<ClumpSubCalendarEvent>();
            //OverLapping_Clump = new List<ClumpSubCalendarEvent>();
            SubCalEventsOverLapWithBase = new List<SubCalendarEvent>();
            List<SubCalendarEvent> UnClumppable = new List<SubCalendarEvent>();
            ClumpedResults = new Dictionary<SubCalendarEvent, ClumpSubCalendarEvent>();
            
            /*BaseEvent = BaseClump[0];//this can be reevaluated to cater to the most constrained. i.e the one with limted a later sart time and percentage fill for whatever is left.                 
            ReferenceStartTime = BaseEvent.getCalendarEventRange.End < BoundaryTimeLine.End ? BaseEvent.getCalendarEventRange.End : BoundaryTimeLine.End;
            foreach (SubCalendarEvent mySubCalendarEvent in BaseClump)
            {
                ReferenceStartTime -= BaseEvent.ActiveDuration;
            }*/

            for (; i < Appendables.Count; i++)
            {

                //List<SubCalendarEvent> ReferenceClump = new List<SubCalendarEvent>(BaseClump);
                DateTime TimeLimit = ReferenceStartTime - Appendables[i].ActiveDuration;
                bool Zero = (Appendables[i].getCalendarEventRange.Start <= TimeLimit);
                bool One = (TimeLimit >= BoundaryTimeLine.Start);
                //bool Two = (BoundaryTimeLine.TimelineSpan >= Appendables[i].EventTimeLine.TimelineSpan);


                if (Zero && One)// && Two)
                {
                    List<SubCalendarEvent> Removed_Unnecessary = Appendables.ToList();
                    
                    SubCalendarEvent RelativeSubEvent = Appendables[i];

                    Removed_Unnecessary.Remove(RelativeSubEvent);

                    
                    ClumpedResults.Add(RelativeSubEvent, null);
                    
                    /*if (CompleteResolvedNonOverlapping.Count > 100)//This is a hack to resolve the memory issue
                    {
                        break;
                    }*/
                }
                else
                {
                    SubCalEventsOverLapWithBase.Add(Appendables[i]);
                }
            }

            List<SubCalendarEvent> arg1 = ClumpedResults.Keys.ToList();
            int j = 0;
            for (; j < arg1.Count; j++)
            {
                ClumpedResults[arg1[j]] = populateClumpedResults(PreCeedingBaseEndtime, arg1[j], ClumpedResults[arg1[j]], ReferenceStartTime - arg1[j].ActiveDuration, BoundaryTimeLine);
            }

            if (SubCalEventsOverLapWithBase.Count > 0)
            {
                SubCalendarEvent BreakOffSubCalEvent = SubCalEventsOverLapWithBase[0];
                SubCalEventsOverLapWithBase.Remove(BreakOffSubCalEvent);
                BreakOffClump = new ClumpSubCalendarEvent(BreakOffSubCalEvent, SubCalEventsOverLapWithBase, new TimeLine(PreCeedingBaseEndtime, BoundaryTimeLine.End));
            }
            
        }


        ClumpSubCalendarEvent populateClumpedResults(DateTime BaseEndTime,SubCalendarEvent refSubCalendarEvent, ClumpSubCalendarEvent refClumpSubCalendarEvent, DateTime RefereceStartTime, TimeLine BoundaryTimeLine)
        {
            List<SubCalendarEvent> Arg1 = ClumpedResults.Keys.ToList();
            bool temp = Arg1.Remove(refSubCalendarEvent);
            refClumpSubCalendarEvent = new ClumpSubCalendarEvent(RefereceStartTime,BaseEndTime, Arg1, BoundaryTimeLine);
            return refClumpSubCalendarEvent;
        }
        
        
        /*public ClumpSubCalendarEvent(List<SubCalendarEvent> BaseClump, List<SubCalendarEvent> Appendables, TimeLine BoundaryTimeLine)
        {
            int i = 0;
            NonOverLapping_Clump = new List<ClumpSubCalendarEvent>();
            OverLapping_Clump = new List<ClumpSubCalendarEvent>();
            List<SubCalendarEvent> UnClumppable = new List<SubCalendarEvent>();
            if (BaseClump.Count < 1)
            {
                if (Appendables.Count > 0)
                {
                    Appendables = Appendables.OrderBy(obj => obj.getCalendarEventRange.End).ToList();
                    SubCalendarEvent RelativeSubEvent = Appendables[0];
                    Appendables.Remove(Appendables[0]);
                    ClumpSubCalendarEvent myThis = new ClumpSubCalendarEvent(RelativeSubEvent, Appendables, BoundaryTimeLine);
                    NonOverLapping_Clump = myThis.NonOverLapping_Clump;
                    BaseEvent = myThis.BaseEvent;
                    NonOverLapping_Clump = myThis.NonOverLapping_Clump;
                    OverLapping_Clump = myThis.OverLapping_Clump;
                    ReferenceStartTime = myThis.ReferenceStartTime;
                    SubCalEventsOverLapWithBase = myThis.SubCalEventsOverLapWithBase;
                    BreakOffClump = myThis.BreakOffClump;

                }

            }
            else
            {
                BaseEvent = BaseClump[0];//this can be reevaluated to cater to the most constrained. i.e the one with limted a later sart time and percentage fill for whatever is left.                 
                ReferenceStartTime = BaseEvent.getCalendarEventRange.End < BoundaryTimeLine.End ? BaseEvent.getCalendarEventRange.End : BoundaryTimeLine.End;
                foreach (SubCalendarEvent mySubCalendarEvent in BaseClump)
                {
                    ReferenceStartTime -= BaseEvent.ActiveDuration;
                }

                for (; i < Appendables.Count; i++)
                {

                    List<SubCalendarEvent> ReferenceClump = new List<SubCalendarEvent>(BaseClump);
                    DateTime TimeLimit = ReferenceStartTime - Appendables[i].ActiveDuration;
                    bool Zero = (Appendables[i].getCalendarEventRange.Start <= TimeLimit);
                    bool One = (TimeLimit >= BoundaryTimeLine.Start);
                    //bool Two = (BoundaryTimeLine.TimelineSpan >= Appendables[i].EventTimeLine.TimelineSpan);


                    if (Zero && One)// && Two)
                    {
                        List<SubCalendarEvent> Removed_Unnecessary = Appendables.ToList();
                        List<SubCalendarEvent> ExtendedList = ReferenceClump.ToList();
                        Removed_Unnecessary.Remove(Appendables[i]);
                        ExtendedList.Add((Appendables[i]));
                        ClumpSubCalendarEvent NewClump = new ClumpSubCalendarEvent(ExtendedList, Removed_Unnecessary, BoundaryTimeLine);
                        NonOverLapping_Clump.Add(NewClump);
                        if (CompleteResolvedNonOverlapping.Count > 100)//This is a hack to resolve the memory issue
                        {
                            break;
                        }
                    }
                    else
                    {
                        SubCalEventsOverLapWithBase.Add(Appendables[i]);
                    }

                    if (SubCalEventsOverLapWithBase.Count > 0)
                    {
                        SubCalendarEvent BreakOffSubCalEvent = SubCalEventsOverLapWithBase[0];
                        SubCalEventsOverLapWithBase.Remove(BreakOffSubCalEvent);
                        BreakOffClump = new ClumpSubCalendarEvent(BreakOffSubCalEvent, SubCalEventsOverLapWithBase, new TimeLine((getLeftMostPossibleStartLine(BaseEvent, BoundaryTimeLine) + BaseEvent.ActiveDuration), BoundaryTimeLine.End));
                    }

                }
            }
        }*/



        DateTime getLeftMostPossibleStartLine(SubCalendarEvent Sorted, TimeLine Boundary)
        {
            if (Boundary.Start >= Sorted.getCalendarEventRange.Start)
            {
                return new DateTime(Boundary.Start.Ticks);
            }
            else
            {
                return new DateTime(Sorted.getCalendarEventRange.Start.Ticks);
            }
        }

        public List<List<SubCalendarEvent>> GenerateList(int TypeOfList)
        {
            List<List<SubCalendarEvent>> retValue = new List<List<SubCalendarEvent>>();

            /*if (BaseEvent == null)
            {
                return retValue;
            }*/

            int i = 0;
            List<List<SubCalendarEvent>> temp_ListOfClump = new List<List<SubCalendarEvent>>();
            List<List<SubCalendarEvent>> temp_ListOfClump_BreakOfClump = new List<List<SubCalendarEvent>>(); ;

            foreach (KeyValuePair<SubCalendarEvent, ClumpSubCalendarEvent> eachKeyValuePair in ClumpedResults)
            {
                temp_ListOfClump = eachKeyValuePair.Value.GenerateList(TypeOfList);
                if (temp_ListOfClump.Count > 0)
                {
                    foreach (List<SubCalendarEvent> eachList in temp_ListOfClump)
                    {
                        eachList.Add(eachKeyValuePair.Key);
                    }
                }
                else
                {
                    List<SubCalendarEvent> MyList = new List<SubCalendarEvent>() { eachKeyValuePair.Key };
                    temp_ListOfClump = new List<List<SubCalendarEvent>>() { MyList };
                }
                

                retValue.AddRange(temp_ListOfClump);
            }

            if (BaseEvent != null)
            {
                List<List<SubCalendarEvent>> retValueCopied = new List<List<SubCalendarEvent>>();// retValue.ToList();
                SubCalendarEvent BaseEvent_cpy = BaseEvent.createCopy();
                DateTime BaseEnd = BoundaryTimeLine.End;
                DateTime BaseStart = BaseEnd - BaseEvent_cpy.ActiveDuration;
                BaseEvent_cpy = new SubCalendarEvent(BaseEvent_cpy.ID, BaseStart, BaseEnd, new BusyTimeLine(BaseEvent_cpy.ID, BaseStart, BaseEnd), BaseEvent_cpy.myLocation, BaseEvent_cpy.getCalendarEventRange);
                //BaseEvent_cpy.updateSubEvent(BaseEvent_cpy.SubEvent_ID, Arg1);
                DateTime refTime = BaseStart;
                foreach (List<SubCalendarEvent> eachList in retValue)
                {
                    List<SubCalendarEvent> eachList_Updated = new List<SubCalendarEvent>();
                    foreach (SubCalendarEvent eachSubCalendarEvent in eachList)
                    {
                        SubCalendarEvent eachSubCalendarEvent_Clumped = eachSubCalendarEvent.createCopy();
                        DateTime eachSubCalendarEvent_ClumpedEnd = refTime;
                        DateTime eachSubCalendarEvent_ClumpedStart = eachSubCalendarEvent_ClumpedEnd - eachSubCalendarEvent_Clumped.ActiveDuration;
                        eachSubCalendarEvent_Clumped = new SubCalendarEvent(eachSubCalendarEvent_Clumped.ID, eachSubCalendarEvent_ClumpedStart, eachSubCalendarEvent_ClumpedEnd, new BusyTimeLine(eachSubCalendarEvent_Clumped.ID, eachSubCalendarEvent_ClumpedStart, eachSubCalendarEvent_ClumpedEnd), eachSubCalendarEvent_Clumped.myLocation, eachSubCalendarEvent_Clumped.getCalendarEventRange);
                        //eachSubCalendarEvent.updateSubEvent(eachSubCalendarEvent_Clumped.SubEvent_ID, Arg2);
                        refTime = eachSubCalendarEvent_ClumpedStart;
                        eachList_Updated.Add(eachSubCalendarEvent_Clumped);
                    }

                    retValueCopied.Add(eachList_Updated);
                }
                if(retValue.Count<1)
                {
                    List<SubCalendarEvent> MyList = new List<SubCalendarEvent>() { BaseEvent_cpy };
                    retValueCopied = new List<List<SubCalendarEvent>>() { MyList };
                }
                retValue = retValueCopied;
            }


            if (BreakOffClump != null)
            {
                temp_ListOfClump_BreakOfClump = BreakOffClump.GenerateList(TypeOfList);
                

            }
            retValue = SerializeListOfClumpsWithBreakOffClumps(retValue, temp_ListOfClump_BreakOfClump);
            
            

            return retValue;



        }

        public List<List<SubCalendarEvent>> SerializeListOfClumpsWithBreakOffClumps(List<List<SubCalendarEvent>> Arg1, List<List<SubCalendarEvent>> Arg2)
        {
            /*
             * This Takes two List Arguements. Each List objection in Arg1 (the inner List object in Arg1) , Lets call it ListX, generates another List that appends each List in arg2 to ListX
             * e.g Arg1= {List0,List1,List2,List3}, Arg2= {ListA,ListB,ListC,ListD}
             * retValue Will Have {List0+ListA,List0+ListB,List0+ListC,List1+ListA,List1+ListB,List1+ListC,   List2+ListA,List2+ListB,List2+ListC}
             */
            

            List < List < SubCalendarEvent >> retValue = new List<List<SubCalendarEvent>>();

            if (Arg1.Count < 1)
            {
                //var Arg3 = retValue.Concat(Arg2);
                retValue = Arg2;//
                return retValue;
            }
            if (Arg2.Count < 1)
            {
                retValue = Arg1;//
                return retValue;
            }

            foreach (List<SubCalendarEvent> eachList in Arg1)
            {
                

                foreach (List<SubCalendarEvent> eachList0 in Arg2)
                {
                    List<SubCalendarEvent> eachList_cpy = eachList.ToList();
                    eachList_cpy.AddRange(eachList0);
                    retValue.Add(eachList_cpy);
                }
                


            }

            return retValue;
        }
    }
}