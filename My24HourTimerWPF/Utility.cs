using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    class Utility
    {
        static Utility()
        { 
        
        }

        public static List<SubCalendarEvent> sortSubCalEventByDeadline(List<SubCalendarEvent> SubCalEventRestricted, bool SecondSortByStartDate)
        {
            
            SubCalEventRestricted = SubCalEventRestricted.OrderBy(obj => obj.End).ToList();
            Dictionary<long, List<SubCalendarEvent>> Dict_DeadlineAndClashingDeadlineSubCalEvents = new Dictionary<long, List<SubCalendarEvent>>();
            foreach (SubCalendarEvent MySubCalEvent in SubCalEventRestricted)
            {

                if (Dict_DeadlineAndClashingDeadlineSubCalEvents.ContainsKey(MySubCalEvent.End.Ticks))
                {
                    Dict_DeadlineAndClashingDeadlineSubCalEvents[MySubCalEvent.End.Ticks].Add(MySubCalEvent);
                }
                else
                {
                    Dict_DeadlineAndClashingDeadlineSubCalEvents.Add(MySubCalEvent.End.Ticks, new List<SubCalendarEvent>());
                    Dict_DeadlineAndClashingDeadlineSubCalEvents[MySubCalEvent.End.Ticks].Add(MySubCalEvent);
                }
            }


            SubCalEventRestricted = new List<SubCalendarEvent>();
            foreach (long DeadLine in Dict_DeadlineAndClashingDeadlineSubCalEvents.Keys.ToArray())
            {
                Dict_DeadlineAndClashingDeadlineSubCalEvents[DeadLine] = Dict_DeadlineAndClashingDeadlineSubCalEvents[DeadLine].OrderBy(obj => obj.Start).ToList();
                if (!SecondSortByStartDate)
                { Dict_DeadlineAndClashingDeadlineSubCalEvents[DeadLine].Reverse(); }
                SubCalEventRestricted.AddRange(Dict_DeadlineAndClashingDeadlineSubCalEvents[DeadLine]);
            }


            
            return SubCalEventRestricted;
        }

        public static Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> NotInList_NoEffect(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> ListToCheck, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> MyCurrentList)
        {
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> retValue = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
            if (MyCurrentList.Keys.Count < 1)
            {
                foreach (TimeSpan eachTimespan in ListToCheck.Keys)
                {
                    retValue.Add(eachTimespan, new mTuple<int, TimeSpanWithStringID>(ListToCheck[eachTimespan].Item1, ListToCheck[eachTimespan].Item2));
                }
                return retValue;                
            }

            foreach (TimeSpan eachTimeSpan in ListToCheck.Keys)
            {
                if (MyCurrentList.ContainsKey(eachTimeSpan))
                {
                    retValue.Add(eachTimeSpan, new mTuple<int, TimeSpanWithStringID>(ListToCheck[eachTimeSpan].Item1 - MyCurrentList[eachTimeSpan].Item1, ListToCheck[eachTimeSpan].Item2));
                }
                else 
                {
                    retValue.Add(eachTimeSpan, new mTuple<int, TimeSpanWithStringID>(ListToCheck[eachTimeSpan].Item1, ListToCheck[eachTimeSpan].Item2));
                }
            }

            
            return retValue;
        }


        public static List<T> NotInList<T>(List<T> ListToCheck, List<T> MyCurrentList)
        {
            foreach (T MySubCalendarEvent in MyCurrentList)
            {
                ListToCheck.Remove(MySubCalendarEvent);
            }
            return ListToCheck;
        }


        public static List<T> NotInList_NoEffect<T>(List<T> ListToCheck, List<T> MyCurrentList)
        {
            List<T> ListToCheck_Cpy = ListToCheck.ToList();
            
            foreach (T MySubCalendarEvent in MyCurrentList)
            {
                ListToCheck_Cpy.Remove(MySubCalendarEvent);
            }
            return ListToCheck_Cpy;
        }

        public static Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> ListIntersection(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> DictToCheck, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> MyCurrentDict)
        {
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> InListElements = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();

            foreach (TimeSpan eachTimeSpan in DictToCheck.Keys)
            {
                if(MyCurrentDict.ContainsKey(eachTimeSpan))
                {
                    mTuple<int, TimeSpanWithStringID> MyCurrentDictTimeSpanWithStringID =new mTuple<int,TimeSpanWithStringID> (MyCurrentDict[eachTimeSpan].Item1,MyCurrentDict[eachTimeSpan].Item2) ;
                    mTuple<int, TimeSpanWithStringID> DictToCheckTimeSpanWithStringID = new mTuple<int, TimeSpanWithStringID>(DictToCheck[eachTimeSpan].Item1, DictToCheck[eachTimeSpan].Item2);
                    InListElements.Add(eachTimeSpan, (MyCurrentDictTimeSpanWithStringID.Item1 < DictToCheckTimeSpanWithStringID.Item1 ? MyCurrentDictTimeSpanWithStringID : DictToCheckTimeSpanWithStringID));
                }
            }

            

            return InListElements;
        }


        public static List<SubCalendarEvent> ListIntersection(List<SubCalendarEvent> ListToCheck, List<SubCalendarEvent> MyCurrentList)
        {
            List<SubCalendarEvent> InListElements = new List<SubCalendarEvent>();

            foreach (SubCalendarEvent MySubCalendarEvent0 in ListToCheck)
            {
                foreach (SubCalendarEvent MySubCalendarEvent1 in MyCurrentList)
                {
                    if (MySubCalendarEvent1.ID == MySubCalendarEvent0.ID)
                    {
                        InListElements.Add(MySubCalendarEvent1);
                    }
                }
            }
            return InListElements;
        }


        /*public static List<SubCalendarEvent> ListIntersection(List<SubCalendarEvent> ListToCheck, List<SubCalendarEvent> MyCurrentList)
        {
            List<SubCalendarEvent> InListElements = new List<SubCalendarEvent>();

            foreach (SubCalendarEvent MySubCalendarEvent0 in ListToCheck)
            {
                foreach (SubCalendarEvent MySubCalendarEvent1 in MyCurrentList)
                {
                    if (MySubCalendarEvent1.ID == MySubCalendarEvent0.ID)
                    {
                        InListElements.Add(MySubCalendarEvent1);
                    }
                }
            }
            return InListElements;
        }*/

        public static List<T> InListAButNotInB<T>(List<T> ListA, List<T> ListB)
        {
            List<T> retValue = new List<T>();

            foreach(T eachT in ListA)
            {
                if (!(ListB.Contains(eachT)))
                {
                    retValue.Add(eachT);
                }
            }

            return retValue;
        }

        public static Tuple<int, List<List<Dictionary<T, mTuple<int, U>>>>> getHighestCountList<T, U>(List<List<Dictionary<T, mTuple<int, U>>>> PossibleMatches, List<Dictionary<T, mTuple<int, U>>> ConstrainedList)
        {
            int HighstSum = 0;
            List<List<Dictionary<T, mTuple<int, U>>>> retValue= new List<List<Dictionary<T,mTuple<int,U>>>>();
            //List<TimeLine> AllTimeLines = ConstrainedList.Keys.ToList();
            List<Dictionary<T, mTuple<int, U>>> ListOfDict = ConstrainedList;

            int j = 0;
            foreach (List<Dictionary<T, mTuple<int, U>>> eachList in PossibleMatches)
            {
                int CurrentSum = 0;
                int i = 0;

                foreach (Dictionary<T, mTuple<int, U>> eachDictionary in eachList)
                {
                    ICollection<KeyValuePair<T, mTuple<int, U>>> AllData = eachDictionary;
                    foreach (KeyValuePair<T, mTuple<int, U>> eachKeyValuePair in AllData)
                    {
                        int evpItem1 = 0;
                        if (ListOfDict[i].ContainsKey(eachKeyValuePair.Key))
                        {

                            int evpItem2 = ListOfDict[i][eachKeyValuePair.Key].Item1;
                            evpItem1 = eachKeyValuePair.Value.Item1 + evpItem2;
                        }
                        else
                        {
                            evpItem1 = eachKeyValuePair.Value.Item1;
                        }

                        evpItem1 = eachKeyValuePair.Value.Item1;
                        CurrentSum += evpItem1;
                    }
                    ++i;
                }

                if (CurrentSum >= HighstSum)
                {
                    retValue.Add(eachList);
                    if (CurrentSum > HighstSum)
                    {
                        retValue = new List<List<Dictionary<T, mTuple<int, U>>>>();
                        retValue.Add(eachList);
                    }
                    HighstSum = CurrentSum;
                }
                j++;
            }

            return new Tuple<int,List<List<Dictionary<T,mTuple<int,U>>>>>(HighstSum,retValue);
        }

        static public bool PinSubEventsToStart(List<SubCalendarEvent> arg1, TimeLine Arg2)
        {
            return PinSubEventsToStart_NoEdit(arg1.ToList(), Arg2);
        }

        static private bool PinSubEventsToStart_NoEdit(List<SubCalendarEvent> arg1, TimeLine Arg2)
        {
            bool retValue = true;
            if (arg1.Count > 0)
            {
                retValue=arg1[0].PinSubEventsToStart(Arg2);
                TimeLine var0 = new TimeLine(arg1[0].ActiveSlot.End, Arg2.End);
                arg1.RemoveAt(0);
                if (retValue && PinSubEventsToStart_NoEdit(arg1, var0))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return retValue;
        }

        static public bool PinSubEventsToEnd(List<SubCalendarEvent> arg1, TimeLine Arg2)
        {
            //Pin Sub Events to the end of the TimeLine Each SubCalendarEvent Stays within the confines of its Calendar Event. The pinning starts from the last SubCalevent in the list
            
            return PinSubEventsToEnd_NoEdit(arg1.ToList(), Arg2);
        }

        static private bool PinSubEventsToEnd_NoEdit(List<SubCalendarEvent> arg1, TimeLine Arg2)
        {
            bool retValue=true;
            if (arg1.Count > 0)
            {//hack notice you need to ensure that each subcalevent can fit within the timeline. YOu need a way to resolve this if not possible
                retValue = arg1[arg1.Count - 1].PinToEnd(Arg2);
                TimeLine var0 = new TimeLine(Arg2.Start, arg1[arg1.Count - 1].ActiveSlot.Start);
                arg1.RemoveAt(arg1.Count - 1);
                if (retValue && PinSubEventsToEnd(arg1, var0))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return retValue;
        }

        static public List<List<T>> SerializeList<T>(List<List<List<T>>> Arg1)
        {
            List<List<T>> retValue = new List<List<T>>();
            int i = 0;
            for (; i < Arg1.Count; i++)
            {
                retValue = SerializeList(retValue, Arg1[i].ToList());
            }

            return retValue;
        }


        static private List<List<T>> SerializeList<T>(List<List<T>> Arg1, List<List<T>> Arg2)
        {
            /*
             * This Takes two List Arguements. Each List objection in Arg1 (the inner List object in Arg1) , Lets call it ListX, generates another List that appends each List in arg2 to ListX
             * e.g Arg1= {List0,List1,List2,List3}, Arg2= {ListA,ListB,ListC,ListD}
             * retValue Will Have {List0+ListA,List0+ListB,List0+ListC,List1+ListA,List1+ListB,List1+ListC,   List2+ListA,List2+ListB,List2+ListC}
             */


            List<List<T>> retValue = new List<List<T>>();

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

            foreach (List<T> eachList in Arg1)
            {


                foreach (List<T> eachList0 in Arg2)
                {
                    List<T> eachList_cpy = eachList.ToList();
                    eachList_cpy.AddRange(eachList0);
                    retValue.Add(eachList_cpy);
                }



            }

            return retValue;
        }

        public static TimeSpan SumOfActiveDuration(IEnumerable<SubCalendarEvent> ListOfSubCalEvent)
        {
            TimeSpan retValue = new TimeSpan(0);
            foreach (SubCalendarEvent eachSubCalendarEvent in ListOfSubCalEvent)
            {
                retValue += eachSubCalendarEvent.ActiveDuration;
            }
            return retValue;
        }

        static public double calculateDistance(List<SubCalendarEvent>  allSubCalEvents,Dictionary<string, List<Double>> DistanceMatrix)
        {
            List<string> AllIds = allSubCalEvents.Select(obj => obj.SubEvent_ID.getLevelID(0)).ToList();
            List<string> Allkeys = DistanceMatrix.Keys.ToList();
            int i = 0;
            int j = i + 1;
            double retValue = 0;
            while (j < allSubCalEvents.Count)
            {
                string iIndex = AllIds[i];
                string jIndex = AllIds[j];
                int valueIndexofkeyValuePair = Allkeys.IndexOf(jIndex);
                retValue += DistanceMatrix[AllIds[i]][valueIndexofkeyValuePair];
                i++; j++;
            }
            return retValue;
        }


        /*public double calculateDistance(SubCalendarEvent SubCalendarEventA, SubCalendarEvent SubCalendarEventB, ref Dictionary<string, List<Double>> DistanceMatrix)
        { 
            
        }*/


        public static mTuple<bool, List<TimeSpanWithStringID>> isViolatingTimeSpanString(Dictionary<string, mTuple<int, TimeSpanWithStringID>> GuardList, List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> PotentialError_list)
        {
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> recompiledListToMatch_GuardList = new Dictionary<string, mTuple<int, TimeSpanWithStringID>>();

            mTuple<bool, List<TimeSpanWithStringID>> retValue = new mTuple<bool, List<TimeSpanWithStringID>>(false, new List<TimeSpanWithStringID>());
            foreach(Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in PotentialError_list)
            {
                foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in eachDictionary)
                {
                    if (recompiledListToMatch_GuardList.ContainsKey(eachKeyValuePair.Key))
                    {
                        recompiledListToMatch_GuardList[eachKeyValuePair.Key].Item1 += eachKeyValuePair.Value.Item1;
                    }
                    else 
                    {
                        recompiledListToMatch_GuardList.Add(eachKeyValuePair.Key, new mTuple<int, TimeSpanWithStringID>(eachKeyValuePair.Value.Item1, eachKeyValuePair.Value.Item2));
                    }
                }
            }

            foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in recompiledListToMatch_GuardList)
            {
                if (GuardList.ContainsKey(eachKeyValuePair.Key))
                {
                    if (GuardList[eachKeyValuePair.Key].Item1 < eachKeyValuePair.Value.Item1)
                    {
                        retValue.Item1 = true;
                        retValue.Item2.Add(eachKeyValuePair.Value.Item2);
                    }
                }
                else 
                {
                    retValue.Item1 = true;
                    retValue.Item2.Add(eachKeyValuePair.Value.Item2);
                }
            }

            return retValue;


        }

        static public TimeLine AddSubCaleventsToTimeLine(TimeLine EncasingTimeLine, IEnumerable<SubCalendarEvent> SubCalendarEvents)
        {
            EncasingTimeLine=EncasingTimeLine.CreateCopy();
            foreach (SubCalendarEvent eachSubCalendarEvent in SubCalendarEvents)
            {
                if (eachSubCalendarEvent.canExistWithinTimeLine(EncasingTimeLine))
                {
                    EncasingTimeLine.AddBusySlots(eachSubCalendarEvent.ActiveSlot);
                }
                else
                {
                    return null;
                }
            }

            return EncasingTimeLine;
        }

        static public IEnumerable<T> serialLizeMtuple_MTupleToVarT<T>(IEnumerable<mTuple<int, T>> EnunerableData)
        {
            List<T> retValue = new List<T>();
            foreach (mTuple<int, T> eachmTuple in EnunerableData)
            {
                int i = 0;
                for (; i < eachmTuple.Item1; i++)
                {
                    retValue.Add(eachmTuple.Item2);
                }
            }

            return retValue;
        }

        static public List<SubCalendarEvent> mTupleToSubCalEvents(List<mTuple<bool, SubCalendarEvent>> ListOfMTuples)
        {
            List<SubCalendarEvent> retValue = new System.Collections.Generic.List<SubCalendarEvent>();
            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in ListOfMTuples)
            {
                retValue.Add(eachmTuple.Item2);
            }

            return retValue;
        }



        public static List<T> RandomizeIEnumerable<T>(List<T> EntryList)
        {
            //EntryList=EntryList.ToList();
            List<T> retValue = EntryList.ToList();
            Random myRand = new Random();
            for (int i = 0; i < EntryList.Count;i++ )
            {
                int MyNumb = myRand.Next(0, EntryList.Count);
                T Temp = retValue[i];
                retValue[i]=retValue[MyNumb];
                retValue[MyNumb] = Temp;
            }

            return retValue;
        }

        static public List<mTuple<T, SubCalendarEvent>> SubCalEventsTomTuple<T>(List<SubCalendarEvent> ListOfMTuples, T DefaultValue)
        {
            List<mTuple<T, SubCalendarEvent>> retValue = new System.Collections.Generic.List<mTuple<T, SubCalendarEvent>>();
            foreach (SubCalendarEvent eachmSubCalendarEvent in ListOfMTuples)
            {
                retValue.Add(new mTuple<T, SubCalendarEvent>(DefaultValue, eachmSubCalendarEvent));
            }

            return retValue;
        }


        static public List<mTuple<T, R>> mTupleTomTuple<T, U, R>(List<mTuple<U, R>> ListOfMTuples, T DefaultValue)
        {
            List<mTuple<T, R>> retValue = new System.Collections.Generic.List<mTuple<T, R>>();
            foreach (mTuple<U, R> eachmSubCalendarEvent in ListOfMTuples)
            {
                retValue.Add(new mTuple<T, R>(DefaultValue, eachmSubCalendarEvent.Item2));
            }

            return retValue;
        }
        
    }
}
