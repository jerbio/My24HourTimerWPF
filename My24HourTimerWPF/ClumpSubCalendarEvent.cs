using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    class ClumpSubCalendarEvent//:SubCalendarEvent
    {
        //List<SubCalendarEvent> OverLapping;
        //List<SubCalendarEvent> NonOverLapping;
        SubCalendarEvent BaseEvent;
        List<ClumpSubCalendarEvent> NonOverLapping_Clump;
        static List<List<SubCalendarEvent>> CompleteResolvedNonOverlapping;
        public static int Completed=0;
        List<ClumpSubCalendarEvent> OverLapping_Clump;
        DateTime ReferenceStartTime;
        public ClumpSubCalendarEvent(SubCalendarEvent BaseSubCalendarEvent, List<SubCalendarEvent> Appendables, TimeLine BoundaryTimeLine)
        {
            
            //OverLapping = new List<SubCalendarEvent>();
            NonOverLapping_Clump = new List<ClumpSubCalendarEvent>();
            //NonOverLapping= new List<SubCalendarEvent>();
            //NonOverLapping.Add(BaseSubCalendarEvent);
            BaseEvent=BaseSubCalendarEvent;
            ReferenceStartTime = BaseEvent.getCalendarEventRange.End;
            //NonOverLapping_Clump.Add(BaseSubCalendarEvent);
            int i=0;
            for (; i < Appendables.Count; i++)
            {
                List<SubCalendarEvent> ReferenceClump =new List<SubCalendarEvent>();
                ReferenceClump.Add(BaseSubCalendarEvent);
                DateTime TimeLimit=ReferenceStartTime - Appendables[i].ActiveDuration;
                bool Zero = (Appendables[i].getCalendarEventRange.Start <= TimeLimit);
                bool One = (TimeLimit >= BoundaryTimeLine.Start) ;
                bool Two = (BoundaryTimeLine.TimelineSpan>=Appendables[i].EventTimeLine.TimelineSpan);


                if (Zero && One && Two)
                {
                    List<SubCalendarEvent> Removed_Unnecessary = Appendables.ToList();
                    List<SubCalendarEvent> ExtendedList = ReferenceClump.ToList();
                    SubCalendarEvent RelativeSubEvent = Appendables[i];
                    Removed_Unnecessary.Remove(RelativeSubEvent);


                    ExtendedList.Add((Appendables[i]));
                    NonOverLapping_Clump.Add(new ClumpSubCalendarEvent(RelativeSubEvent, Removed_Unnecessary, BoundaryTimeLine));
                    if (Removed_Unnecessary.Count > 0)
                    {
                     
                        ++Completed;
                        if (Completed >= 100)
                        {
                            break;
                        }
                    }
                }
                /*else
                {
                    List<SubCalendarEvent> Removed_Unnecessary = Appendables.ToList();
                    List<SubCalendarEvent> ExtendedList = ReferenceClump.ToList();
                    Removed_Unnecessary.Remove(Appendables[i]);
                    

                    ExtendedList.Add((Appendables[i]));
                    OverLapping = ExtendedList;
                    OverLapping_Clump.Add(new ClumpSubCalendarEvent(ExtendedList, Removed_Unnecessary, BoundaryTimeLine));
                    
                }*/
            }
            List<SubCalendarEvent> Fittable = new List<SubCalendarEvent>();
            
        }


        public ClumpSubCalendarEvent(List<SubCalendarEvent> BaseClump, List<SubCalendarEvent> Appendables, TimeLine BoundaryTimeLine)
        {
            int i = 0;
            NonOverLapping_Clump = new List<ClumpSubCalendarEvent>();
            //CompleteResolvedNonOverlapping = new List<SubCalendarEvent>();
            OverLapping_Clump = new List<ClumpSubCalendarEvent>();
            if (BaseClump.Count < 1)
            {
                if (Appendables.Count > 0)
                {
                    Appendables = Appendables.OrderBy(obj => obj.getCalendarEventRange.End).ToList();
                    SubCalendarEvent RelativeSubEvent = Appendables[0];
                    Appendables.Remove(Appendables[0]);
                    ClumpSubCalendarEvent myThis = new ClumpSubCalendarEvent(RelativeSubEvent, Appendables, BoundaryTimeLine);
                    NonOverLapping_Clump = myThis.NonOverLapping_Clump;
                    //OverLapping = myThis.OverLapping;
                    //NonOverLapping = myThis.NonOverLapping;
                    BaseEvent = myThis.BaseEvent;
                    NonOverLapping_Clump = myThis.NonOverLapping_Clump;
                    OverLapping_Clump = myThis.OverLapping_Clump;
                    ReferenceStartTime = myThis.ReferenceStartTime;
                }

            }
            else
            {
                BaseEvent = BaseClump[BaseClump.Count - 1];//this can be reevaluated to cater to the most constrained. i.e the one with limted a later sart time and percentage fill for whatever is left. 
                ReferenceStartTime = BaseClump[BaseClump.Count - 1].getCalendarEventRange.End;
                for (; i < Appendables.Count; i++)
                {

                    List<SubCalendarEvent> ReferenceClump = new List<SubCalendarEvent>(BaseClump);
                    DateTime TimeLimit = ReferenceStartTime - Appendables[i].ActiveDuration;
                    bool Zero = (Appendables[i].getCalendarEventRange.Start <= TimeLimit);
                    bool One = (TimeLimit >= BoundaryTimeLine.Start);
                    bool Two = (BoundaryTimeLine.TimelineSpan >= Appendables[i].EventTimeLine.TimelineSpan);


                    if (Zero && One && Two)
                    {
                        List<SubCalendarEvent> Removed_Unnecessary = Appendables.ToList();
                        List<SubCalendarEvent> ExtendedList = ReferenceClump.ToList();
                        Removed_Unnecessary.Remove(Appendables[i]);


                        ExtendedList.Add((Appendables[i]));
                        //NonOverLapping = ExtendedList;
                        /*if (NonOverLapping.Count == (BaseClump.Count + Appendables.Count))
                        {
                            if (CompleteResolvedNonOverlapping == null)
                            {
                                CompleteResolvedNonOverlapping = new List<List<SubCalendarEvent>>();
                            }
                            CompleteResolvedNonOverlapping.Add(ExtendedList);
                        }*/
                        ClumpSubCalendarEvent NewClump = new ClumpSubCalendarEvent(ExtendedList, Removed_Unnecessary, BoundaryTimeLine);
                        
                        NonOverLapping_Clump.Add(NewClump);
                        if (CompleteResolvedNonOverlapping.Count > 100)//This is a hack to resolve the memory issue
                        {
                            break;
                        }
                    }
                    /*else
                    {
                        List<SubCalendarEvent> Removed_Unnecessary = Appendables.ToList();
                        List<SubCalendarEvent> ExtendedList = ReferenceClump.ToList();
                        Removed_Unnecessary.Remove(Appendables[i]);


                        ExtendedList.Add((Appendables[i]));
                        OverLapping = ExtendedList;
                        ClumpSubCalendarEvent newClump = new ClumpSubCalendarEvent(ExtendedList, Removed_Unnecessary, BoundaryTimeLine);
                        OverLapping_Clump.Add(newClump);
                    }*/
                }
            }
        }



        public List<List<SubCalendarEvent>> GenerateList(int TypeOfList)
        {
            List<List<SubCalendarEvent>> retValue = new List<List<SubCalendarEvent>>();

            if (BaseEvent == null)
            {
                return retValue;
            }

            int i = 0;
            List<List<SubCalendarEvent>> temp_ListOfClump = new List<List<SubCalendarEvent>>();

            for (; i < NonOverLapping_Clump.Count; i++)
            {
                temp_ListOfClump = NonOverLapping_Clump[i].GenerateList(TypeOfList);
                List<List<SubCalendarEvent>> temp_ListOfClumpcpy = temp_ListOfClump.ToList();
                foreach (List<SubCalendarEvent> mySubcalList in temp_ListOfClumpcpy)
                {
                    mySubcalList.Add(BaseEvent);
                }
                if ((temp_ListOfClumpcpy.Count < 1)&&(BaseEvent!=null))
                {
                    List<SubCalendarEvent> SingletonList = new List<SubCalendarEvent>();
                    SingletonList.Add(BaseEvent);
                    temp_ListOfClumpcpy.Add(SingletonList);
                }

                retValue.AddRange(temp_ListOfClumpcpy);   
            }

            if (NonOverLapping_Clump.Count < 1)
            {
                List<SubCalendarEvent> SingletonList = new List<SubCalendarEvent>();
                SingletonList.Add(BaseEvent);
                temp_ListOfClump.Add(SingletonList);
                retValue.AddRange(temp_ListOfClump);   
            }
            /*
            foreach (List<SubCalendarEvent> mySubCalList in temp_ListOfClump)
            {
                mySubCalList.AddRange(NonOverLapping);
            }
            */

            return retValue;
        }

        

        

    }
}