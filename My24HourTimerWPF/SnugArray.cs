﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace My24HourTimerWPF
{
    class SnugArray
    {
        string[] TopElements;
        SnugArray[] SubSnugArrayElements;
        List<Dictionary<string, int>> ListOfDictionaryOfID_Count;//This is a List of Dictionaries with a string of ID and a count of the IDs found
        List<string> AlreadyFoundPermutation;
        //List<TimeSpanWithEventID> ConstainedElements;
        //Dictionary<string, List<TimeSpanWithEventID>> ParentID_TimeSpanID;
        Dictionary<string, ParentIDCount> MyDict;
//        public bool NotValid;

        public SnugArray()
        {
            /*This works recursively. This works by generating a list of elements that can fit within the Timespan to be snugly fit
             Each element in the aforementioned creates a copy of the timespan and then subtracts itself from the copy of the timespan.
             Then a new snug array List is created with the rest of the elements of the array and the remaining time.
             */

        }


        class ParentIDCount
        {
            public int Item1;
            public TimeSpan Item2;
            public ParentIDCount(int Item1, TimeSpan Item2)
            {
                this.Item1 = Item1;
                this.Item2 = Item2;
            }
            public ParentIDCount CreateCopy_Deeper
            {
                get 
                {
                    return new ParentIDCount(Item1, new TimeSpan(Item2.Ticks));
                }
            }

            public ParentIDCount CreateCopy
            {
                get
                {
                    return new ParentIDCount(Item1, Item2);
                }
            }
        }

        public SnugArray(List<mTuple<int, TimeSpanWithStringID>> EntryArrayOfStuff, TimeSpan MaxValue)
        {
            //EntryArrayOfStuff=EntryArrayOfStuff.OrderBy(obj=>obj.TimeSpanID.ToString()).ToList();
            
            
            /*ParentID_TimeSpanID = new Dictionary<string,List<TimeSpanWithID>>();
            foreach (TimeSpanWithID MyTimeSpanWithID in EntryArrayOfStuff)
            {
                string ParentCharString=MyTimeSpanWithID.TimeSpanID.getCalendarEventID();
                if(ParentID_TimeSpanID.ContainsKey(ParentCharString ))
                {
                    
                    ParentID_TimeSpanID[MyTimeSpanWithID.TimeSpanID.getCalendarEventID()].Add(MyTimeSpanWithID);
                }
                else
                {
                    ParentID_TimeSpanID.Add(ParentCharString,new List<TimeSpanWithID>());
                    ParentID_TimeSpanID[ParentCharString].Add(MyTimeSpanWithID);
                }
            }*/

            

            Dictionary<string, ParentIDCount> TimeSpanBucket = generateBuckets(EntryArrayOfStuff);

            

            //SnugArray MyThis = new SnugArray(ConstrainedElements, EntryArrayOfStuff, MaxValue, new Dictionary<string, int>(), new List<Dictionary<string, int>>());
            SnugArray MyThis = new SnugArray(TimeSpanBucket, MaxValue, new List<Dictionary<string, int>>());


            TopElements=MyThis.TopElements;
            SubSnugArrayElements=MyThis.SubSnugArrayElements;
            ListOfDictionaryOfID_Count=MyThis.ListOfDictionaryOfID_Count;//This is a List of Dictionaries with a string of ID and a count of the IDs found
            AlreadyFoundPermutation=MyThis.AlreadyFoundPermutation;
            //ConstainedElements=MyThis.ConstainedElements;
            MyDict = MyThis.MyDict;
        }

        private SnugArray(Dictionary<string, ParentIDCount> EntryArrayOfStuff, TimeSpan MaxValue, List<Dictionary<string, int>> MyListOfDicts)
        //private SnugArray(List<TimeSpanWithID> ConstrainedElements,List<TimeSpanWithID> EntryArrayOfStuff, TimeSpan MaxValue,List<Dictionary<string, int>> MyListOfDicts)
        //private SnugArray(List<TimeSpanWithID> ConstrainedElements,List<TimeSpanWithID> EntryArrayOfStuff, TimeSpan MaxValue,Dictionary<string, int> MyDict,List<Dictionary<string, int>> MyListOfDicts)
        {
            
            EntryArrayOfStuff=new Dictionary<string, ParentIDCount>(EntryArrayOfStuff);
            
            int i=0;

            List<string> EventKeys = EntryArrayOfStuff.Keys.ToList();
            

            List<string> SmallerElements = new List<string>();
            i = 0;
            int MyArrayCount = EventKeys.Count;
            ListOfDictionaryOfID_Count = MyListOfDicts;
            for (; i < MyArrayCount; i++)//loop gets element only smaller or equal to max size
            {
                ParentIDCount MyEntry = EntryArrayOfStuff[EventKeys[i]];
                if ((MyEntry.Item2 <= MaxValue)&&(MyEntry.Item1 >0))//checks if smaller than max value
                {
                    SmallerElements.Add(EventKeys[i]); 
                }
            }
            int IncludeOrDontIncludeLastElement = 0;
            if (SmallerElements.Count > 1)
            {
                int counter = SmallerElements.Count;
                TimeSpan TotalSumOfTimeSoFar = new TimeSpan(0);
                for (; counter > 0;counter-- )
                {
                    string KeyToLastElement = SmallerElements[counter - 1];
                    ParentIDCount CurrentParentIDCOunt = EntryArrayOfStuff[KeyToLastElement];
                    TotalSumOfTimeSoFar += TimeSpan.FromTicks(CurrentParentIDCOunt.Item2.Ticks * CurrentParentIDCOunt.Item1);
                    TimeSpan TimeSpanDiff = MaxValue - TotalSumOfTimeSoFar;
                    if (TimeSpanDiff >= EntryArrayOfStuff[SmallerElements[0]].Item2)
                    {
                        ++IncludeOrDontIncludeLastElement;
                    }
                    else 
                    {
                        break;
                    }
                }
                /*string KeyToLastElement = SmallerElements[SmallerElements.Count - 1];
                
                
                ParentIDCount CurrentParentIDCOunt=EntryArrayOfStuff[KeyToLastElement];
                TimeSpan TimeSpanDiff = TimeSpan.FromTicks(CurrentParentIDCOunt.Item2.Ticks * CurrentParentIDCOunt.Item1);
                
                
                TimeSpanDiff = MaxValue - TimeSpanDiff;
                if (TimeSpanDiff >= EntryArrayOfStuff[SmallerElements[0]].Item2)
                {
                    IncludeOrDontIncludeLastElement = 1;
                }*/
            }

            TopElements = SmallerElements.ToArray();
            //int LengthOfSubElmentArray = TopElements.Length;
            

            i = 0;

            string[] MyFittableElemtsHolder = new string[SmallerElements.Count];//Array holds the current array of subelements for future reasons
            List<string> MyFittableElementsHolderList = new List<string>();//This is also holds a curent array of sub elements.
            SmallerElements.CopyTo(MyFittableElemtsHolder, 0);
            MyFittableElementsHolderList = MyFittableElemtsHolder.ToList();
            MyArrayCount = SmallerElements.Count - IncludeOrDontIncludeLastElement;
            SubSnugArrayElements = new SnugArray[MyArrayCount];
            
            MyDict = EntryArrayOfStuff;

            Dictionary<string, ParentIDCount> EntryArrayOfStuff_Cpy = new Dictionary<string, ParentIDCount>(EntryArrayOfStuff);

            for (; i < MyArrayCount; i++)
            {
                //Dictionary<string, int> MyDictCpy = new Dictionary<string, int>(MyDict);
                
                ParentIDCount DictEntry= EntryArrayOfStuff_Cpy[TopElements[i]];
                //MyFittableElementsHolderList.Remove(TopElements[i]);//removes a value that meets criteria of less than max value. This is used to form a node. The sub nodes will be have only values less than or equal to Max Value-other elements
                

                DictEntry = DictEntry.CreateCopy;
                --(DictEntry.Item1);
                EntryArrayOfStuff_Cpy[TopElements[i]] = DictEntry;
                /*EventID TimeSpanID = TopElements[i].TimeSpanID;
                if (CheckIfNewIdCansSpinNewTree(MyDictCpy, TimeSpanID.getCalendarEventID()))
                {
                    if (MyDictCpy.ContainsKey(TimeSpanID.getCalendarEventID()))
                    {

                        ++(MyDictCpy[TopElements[i].TimeSpanID.getCalendarEventID()]); 
                    }
                    else
                    {

                        MyDictCpy.Add(TimeSpanID.getCalendarEventID(), 1); 
                    }
                    //ListOfDictionaryOfID_Count.Add(MyDict);
                    
                }*/
                
                //EntryArrayOfStuff_Cpy[TopElements[i]] = DictEntry;
                
                //SubSnugArrayElements[i] = new SnugArray(ConstrainedElements, MyFittableElementsHolderList, (MaxValue - TopElements[i].timeSpan), MyDictCpy, ListOfDictionaryOfID_Count);//creates new snug array with elements that are smaller
                SubSnugArrayElements[i] = new SnugArray(EntryArrayOfStuff_Cpy, (MaxValue - DictEntry.Item2), ListOfDictionaryOfID_Count);//creates new snug array with elements that are smaller
                EntryArrayOfStuff_Cpy.Remove(TopElements[i]);
            }

            if (MyArrayCount == 36)
            {
                ;
            }
        
        }

        bool CheckIfNewIdCansSpinNewTree(Dictionary<string, int> MyDict, string ID)
        {
            if (MyDict.ContainsKey(ID))
            {
                ++(MyDict[ID]);
                if(DoIExist(MyDict))
                {
                    --MyDict[ID];
                    return false;
                }
                else
                {
                    --MyDict[ID];
                    return true;
                }
            }
            else 
            {
                return true;
            }
        }





        Dictionary<string, ParentIDCount> generateBuckets(List<mTuple<int, TimeSpanWithStringID>> EntryArrayOfStuff)
        {
            EntryArrayOfStuff = EntryArrayOfStuff.OrderBy(obj => obj.Item2.timeSpan).ToList();
            EntryArrayOfStuff.Reverse();
            Dictionary<string, ParentIDCount> retValue = new Dictionary<string, ParentIDCount>();
            foreach (mTuple<int, TimeSpanWithStringID> eachmTuple in EntryArrayOfStuff)
            {
                //string key= eachTimeSpanWithID.TimeSpanID.ToString();
                string key = eachmTuple.Item2.ID;
                retValue.Add(key, new ParentIDCount(eachmTuple.Item1, eachmTuple.Item2.timeSpan));
                
            }

            return retValue;
        }


        private bool DoIExist(Dictionary<string, int> MyDict)
        {
            string[] DetectedID = MyDict.Keys.ToArray();
            bool retValue = false;

            List<Dictionary<string, int>> PertinentDicts = new List<Dictionary<string, int>>();

            foreach (Dictionary<string, int> PossibleMatchinDict in ListOfDictionaryOfID_Count)
            {
                if (PossibleMatchinDict.Keys.Count >= MyDict.Keys.Count)
                {
                    foreach (string IDKEY in MyDict.Keys)
                    {
                        if (PossibleMatchinDict.ContainsKey(IDKEY))
                        {
                            if (MyDict[IDKEY] <= PossibleMatchinDict[IDKEY])
                            {
                                retValue = true;
                            }
                            else
                            {
                                retValue = false;
                                break;
                            }
                        }
                        else
                        {
                            retValue = false;
                            break;
                        }
                    }

                    if (retValue)
                    {
                        return retValue;
                    }
                    
                    //PertinentDicts.Add(PossibleMatchinDict);
                }
            }

            /*foreach (Dictionary<string, int> PossibleMatchInDict in PertinentDicts)// this will only work for a scenario where we are trying to build the tightest schedule
            {
                foreach (string IDKEY in MyDict.Keys)
                {
                    if (PossibleMatchInDict.ContainsKey(IDKEY))
                    {
                        if (MyDict[IDKEY] == PossibleMatchInDict[IDKEY])
                        {
                            retValue = true;
                        }
                        else
                        {
                            retValue = false;
                            break;
                        }
                    }
                    else
                    {
                        retValue = false;
                        break;
                    }
                }

                if (retValue)
                {
                    return retValue;
                }
            }*/


            return retValue;
        }

        List<List<TimeSpanWithEventID>> GenerateSnugPossibilities(string MyID)
        {
            //List<string> MyFoundID = MyID.Split('#').ToList();

            List<List<TimeSpanWithEventID>> JustAllSubPossibilities;
            int i = 0;
            List<TimeSpanWithEventID> MyCurrentList = new List<TimeSpanWithEventID>();
            int j = 0;
            JustAllSubPossibilities = new List<List<TimeSpanWithEventID>>();
            List<List<TimeSpanWithEventID>> MyTotalSubPossibilities = new List<List<TimeSpanWithEventID>>(); ;
            /*for (; i < TopElements.Length; i++)
            {
                MyCurrentList.Add(TopElements[i]);

                JustAllSubPossibilities = SubSnugArrayElements[i].GenerateSnugPossibilities(MyID);
                //JustAllSubPossibilities.Add(MyCurrentList);
                j = 0;
                for (; j < JustAllSubPossibilities.Count; j++)
                {
                    JustAllSubPossibilities[j].Add(TopElements[i]);
                    JustAllSubPossibilities[j].AddRange(ConstainedElements);
                }
                if (JustAllSubPossibilities.Count == 0)
                {
                    JustAllSubPossibilities.Add(MyCurrentList);

                }
                MyCurrentList = new List<TimeSpanWithID>();
                MyTotalSubPossibilities = MyTotalSubPossibilities.Concat(JustAllSubPossibilities).ToList();
            }*/
            return MyTotalSubPossibilities;
        }

        public List<Dictionary<string,mTuple<int,TimeSpanWithStringID>>> MySnugPossibleEntries
        {
            get
            {

                Dictionary<string, TimeSpanWithStringID> Dict_StringAndDict = new Dictionary<string, TimeSpanWithStringID>();
                List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> retValue = new List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>();
                
                
                foreach (KeyValuePair<string, ParentIDCount> eachKeyPair in MyDict)
                {
                    Dict_StringAndDict.Add(eachKeyPair.Key, new TimeSpanWithStringID(eachKeyPair.Value.Item2, eachKeyPair.Key));
                }
               
                int i=0;
                for (; i < SubSnugArrayElements.Length; i++)
                {


                    List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> Received_retValue = SubSnugArrayElements[i].MySnugPossibleEntries;
                    
                    string myString = TopElements[i];
                    foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in Received_retValue)
                    {
                        
                        if (eachDictionary.ContainsKey(myString))
                        {
                            ++eachDictionary[myString].Item1;
                        }
                        else 
                        {
                            eachDictionary.Add(myString, new mTuple<int, TimeSpanWithStringID>(1, Dict_StringAndDict[myString]));
                        }
                        
                    }
                    if (Received_retValue.Count < 1)
                    {
                        Dictionary<string, mTuple<int, TimeSpanWithStringID>> CurrentDict = new Dictionary<string, mTuple<int, TimeSpanWithStringID>>();
                        CurrentDict.Add(myString, new mTuple<int, TimeSpanWithStringID>(1, Dict_StringAndDict[myString]));
                        retValue.Add(CurrentDict);
                    }

                    retValue.AddRange(Received_retValue);
                }

                return retValue;
                
                
                /*List<List<TimeSpanWithID>> reValue = new List<List<TimeSpanWithID>>();
                
                foreach (Dictionary<string, int> mySnugDictionary in ListOfDictionaryOfID_Count)
                {
                    List<string> DictionaryKeys = mySnugDictionary.Keys.ToList();
                    List<TimeSpanWithID> FullTImeLine = new List<TimeSpanWithID>();

                    foreach (string ParentID_Matched in DictionaryKeys)
                    {
                        int CountMax = mySnugDictionary[ParentID_Matched];
                        int i = 0;
                        for (; i < CountMax; i++)
                        {
                            FullTImeLine.Add(ParentID_TimeSpanID[ParentID_Matched][i]);
                        }
                    }
                    reValue.Add(FullTImeLine);
                }


                return reValue;*/
                
                
                
                /*
                List<List<TimeSpanWithID>> JustAllSubPossibilities;
                int i = 0;
                List<TimeSpanWithID> MyCurrentList = new List<TimeSpanWithID>();
                int j = 0;
                JustAllSubPossibilities = new List<List<TimeSpanWithID>>();
                List<List<TimeSpanWithID>> MyTotalSubPossibilities = new List<List<TimeSpanWithID>>(); ;
                for (; i < TopElements.Length; i++)
                {
                    MyCurrentList.Add(TopElements[i]);
                    
                    JustAllSubPossibilities = SubSnugArrayElements[i].MySnugPossibleEntries;
                    j = 0;
                    for (; j < JustAllSubPossibilities.Count; j++)
                    {
                        JustAllSubPossibilities[j].Add(TopElements[i]);
                        JustAllSubPossibilities[j].AddRange(ConstainedElements);    
                    }
                    if (JustAllSubPossibilities.Count == 0)
                    {
                        JustAllSubPossibilities.Add(MyCurrentList);
                    }
                    MyCurrentList = new List<TimeSpanWithID>();
                    MyTotalSubPossibilities = MyTotalSubPossibilities.Concat(JustAllSubPossibilities).ToList();
                }
                return MyTotalSubPossibilities;*/
            }
        }

        static public Dictionary<string, mTuple<int, TimeSpanWithStringID>> CreateCopyOFSnuPossibilities(Dictionary<string, mTuple<int, TimeSpanWithStringID>> SnugPossibility)
        {
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> retValue = new Dictionary<string, mTuple<int, TimeSpanWithStringID>>();
            foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in SnugPossibility)
            {
                retValue.Add(eachKeyValuePair.Key,new mTuple<int, TimeSpanWithStringID>(eachKeyValuePair.Value.Item1, eachKeyValuePair.Value.Item2));
                
                //retValue[eachKeyValuePair.Key] = 
            }

            return retValue;
        }

        static public TimeSpan TotalTimeSpanOfSnugPossibility(Dictionary<string,mTuple<int,TimeSpanWithStringID>> SnugPossibility)
        {
            return TotalTimeSpanOfSnugPossibility(SnugPossibility.Values);
        }

        static public  TimeSpan TotalTimeSpanOfSnugPossibility(IEnumerable<mTuple<int, TimeSpanWithStringID>> SnugPossibility)
        { 
            TimeSpan retValue = new TimeSpan(0);
            foreach (mTuple<int, TimeSpanWithStringID> eachmTuple in SnugPossibility)
            { 
                retValue+=TimeSpan.FromTicks(eachmTuple.Item2.timeSpan.Ticks*eachmTuple.Item1);
            }

            return retValue;
        }

        static public Dictionary<string, mTuple<int, TimeSpanWithStringID>>  AddToSnugPossibilityList(Dictionary<string, mTuple<int, TimeSpanWithStringID>> PossibiityA,Dictionary<string, mTuple<int, TimeSpanWithStringID>> PossibiityB)
        {
            //handle null inputs
            
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> retValue = CreateCopyOFSnuPossibilities(PossibiityA);


            foreach(string eachString in PossibiityB.Keys)
            {
                if (retValue.ContainsKey(eachString))
                {
                    retValue[eachString].Item1 += PossibiityB[eachString].Item1;
                }
                else 
                { 
                    retValue.Add(eachString, new mTuple<int,TimeSpanWithStringID>(PossibiityB[eachString].Item1,PossibiityB[eachString].Item2));
                }
            }
            return retValue;
        }

        static public Dictionary<string, mTuple<int, TimeSpanWithStringID>> RemoveSnugPossibilityFromAnother(Dictionary<string, mTuple<int, TimeSpanWithStringID>> PossibiityA, Dictionary<string, mTuple<int, TimeSpanWithStringID>> PossibiityB)
        {
            //handle null inputs
            //You remove PossibilityB from PossibilityA

            Dictionary<string, mTuple<int, TimeSpanWithStringID>> retValue = CreateCopyOFSnuPossibilities(PossibiityA);


            foreach (string eachString in PossibiityB.Keys)
            {
                if (retValue.ContainsKey(eachString))
                {
                    retValue[eachString].Item1 -= PossibiityB[eachString].Item1;
                    if (retValue[eachString].Item1 < 1)
                    {
                        retValue.Remove(eachString);
                    }
                }
            }
            return retValue;
        }


        static public bool Equals(Dictionary<string, mTuple<int, TimeSpanWithStringID>> PossibiityA, Dictionary<string, mTuple<int, TimeSpanWithStringID>> PossibiityB)
        {
            //handle null inputs
            //Checks if both Possibilities have the same content

            if (PossibiityA.Count != PossibiityB.Count)
            {
                return false;
            }
            foreach (string eachString in PossibiityB.Keys)
            {
                if (PossibiityA.ContainsKey(eachString))
                {
                    if (PossibiityA[eachString].Item1 == PossibiityB[eachString].Item1)
                    {
                        ;
                    }
                    else 
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public static List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> SortListSnugPossibilities(List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> ListOfSnugPossibilities)
        {
            Dictionary<TimeSpan, List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> AllData = new Dictionary<TimeSpan, List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();
            foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in ListOfSnugPossibilities)
            {
                TimeSpan TotalTime= TotalTimeSpanOfSnugPossibility(eachDictionary);

                if (AllData.ContainsKey(TotalTime))
                {
                    AllData[TotalTime].Add(eachDictionary);
                }
                else 
                {
                    AllData.Add(TotalTime, new List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>() { eachDictionary });
                }
            }

            List<TimeSpan> AllKeys= AllData.Keys.ToList();
            AllKeys.Sort();

            List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> retValue = new List<Dictionary<string,mTuple<int,TimeSpanWithStringID>>>();

            foreach (TimeSpan eachTimeSpan in AllKeys)
            {
                AllData[eachTimeSpan]=AllData[eachTimeSpan].OrderBy(obj => obj.Count).ToList();
                retValue.AddRange(AllData[eachTimeSpan]);
            }
            return retValue;

        }

        
        public string[] MyTopElements
        {
            get
            {
                return TopElements;
            }
        }
    }

}