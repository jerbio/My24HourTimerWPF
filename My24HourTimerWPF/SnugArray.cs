
//#define ForceSequentialSnugArray

#if ForceSequentialSnugArray
#undef enableMultithreading
#endif



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace My24HourTimerWPF
{
    class SnugArray
    {
        TimeSpan[] TopElements;
        SnugArray[] SubSnugArrayElements;
        List<Dictionary<string, int>> ListOfDictionaryOfID_Count;//This is a List of Dictionaries with a string of ID and a count of the IDs found
        List<string> AlreadyFoundPermutation;
        //List<TimeSpanWithEventID> ConstainedElements;
        //Dictionary<string, List<TimeSpanWithEventID>> ParentID_TimeSpanID;
        Dictionary<TimeSpan, ParentIDCount> MyDict;
//        public bool NotValid;
        Dictionary<TimeSpan, ParentIDCount> ArrayOfStuff;
        TimeSpan maxTimeSpan;

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



            Dictionary<TimeSpan, ParentIDCount> TimeSpanBucket = generateBuckets(EntryArrayOfStuff);

            

            //SnugArray MyThis = new SnugArray(ConstrainedElements, EntryArrayOfStuff, MaxValue, new Dictionary<string, int>(), new List<Dictionary<string, int>>());
            SnugArray MyThis = new SnugArray(TimeSpanBucket, MaxValue, new List<Dictionary<string, int>>());


            TopElements=MyThis.TopElements;
            SubSnugArrayElements=MyThis.SubSnugArrayElements;
            ListOfDictionaryOfID_Count=MyThis.ListOfDictionaryOfID_Count;//This is a List of Dictionaries with a string of ID and a count of the IDs found
            AlreadyFoundPermutation=MyThis.AlreadyFoundPermutation;
            //ConstainedElements=MyThis.ConstainedElements;
            MyDict = MyThis.MyDict;
            maxTimeSpan = MaxValue;
            ArrayOfStuff = MyThis.ArrayOfStuff;
        }

        private SnugArray(Dictionary<TimeSpan, ParentIDCount> EntryArrayOfStuff, TimeSpan MaxValue, List<Dictionary<string, int>> MyListOfDicts)
        //private SnugArray(List<TimeSpanWithID> ConstrainedElements,List<TimeSpanWithID> EntryArrayOfStuff, TimeSpan MaxValue,List<Dictionary<string, int>> MyListOfDicts)
        //private SnugArray(List<TimeSpanWithID> ConstrainedElements,List<TimeSpanWithID> EntryArrayOfStuff, TimeSpan MaxValue,Dictionary<string, int> MyDict,List<Dictionary<string, int>> MyListOfDicts)
        {

            EntryArrayOfStuff = new Dictionary<TimeSpan, ParentIDCount>(EntryArrayOfStuff);
            ArrayOfStuff = EntryArrayOfStuff;
            int i=0;
            maxTimeSpan = MaxValue;
            List<TimeSpan> EventKeys = EntryArrayOfStuff.Keys.ToList();


            List<TimeSpan> SmallerElements = new List<TimeSpan>();
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
                for (; counter > 0;counter-- )//including the index 0 in for loop means index is small enough to include all indexes
                {
                    TimeSpan KeyToLastElement = SmallerElements[counter - 1];
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
            }

            TopElements = SmallerElements.ToArray();
            //int LengthOfSubElmentArray = TopElements.Length;
            

            i = 0;

            TimeSpan[] MyFittableElemtsHolder = new TimeSpan[SmallerElements.Count];//Array holds the current array of subelements for future reasons
            List<TimeSpan> MyFittableElementsHolderList = new List<TimeSpan>();//This is also holds a curent array of sub elements.
            SmallerElements.CopyTo(MyFittableElemtsHolder, 0);
            MyFittableElementsHolderList = MyFittableElemtsHolder.ToList();
            MyArrayCount = SmallerElements.Count - IncludeOrDontIncludeLastElement;
            SubSnugArrayElements = new SnugArray[MyArrayCount];
            Tuple<Dictionary<TimeSpan, ParentIDCount>, TimeSpan, List<Dictionary<string, int>>>[] PreppedDataArray = new Tuple<Dictionary<TimeSpan, ParentIDCount>, TimeSpan, List<Dictionary<string, int>>>[MyArrayCount];
            
            MyDict = EntryArrayOfStuff;

            Dictionary<TimeSpan, ParentIDCount> EntryArrayOfStuff_Cpy = new Dictionary<TimeSpan, ParentIDCount>(EntryArrayOfStuff);




#if enableMultithreading

            int j = 0;
            for (; j < MyArrayCount; j++)//prepares data for multithreading breaks dependent references
            {
                
                ParentIDCount DictEntry = EntryArrayOfStuff_Cpy[TopElements[j]];
                DictEntry = DictEntry.CreateCopy;
                --(DictEntry.Item1);
                EntryArrayOfStuff_Cpy[TopElements[j]] = DictEntry;

                Tuple<Dictionary<TimeSpan, ParentIDCount>, TimeSpan, List<Dictionary<string, int>>> PreppedData = new Tuple<Dictionary<TimeSpan, ParentIDCount>, TimeSpan, List<Dictionary<string, int>>>(EntryArrayOfStuff_Cpy, (MaxValue - DictEntry.Item2), ListOfDictionaryOfID_Count);//creates new snug array with elements that are smaller
                PreppedDataArray[j] = PreppedData;
                EntryArrayOfStuff_Cpy.Remove(TopElements[j]);
            }





            Parallel.For(0, MyArrayCount, k =>//makes parallel calls
            {
                
                if (k > 0)
                {
                    EntryArrayOfStuff_Cpy.Remove(TopElements[j - 1]);
                }


                Tuple<Dictionary<TimeSpan, ParentIDCount>, TimeSpan, List<Dictionary<string, int>>> PreppedData0 = PreppedDataArray[k];
                SubSnugArrayElements[k] = new SnugArray(PreppedData0.Item1, PreppedData0.Item2, PreppedData0.Item3);//creates graph on current node

            }

);

#else
            for (; i < MyArrayCount; i++)
            {
               ParentIDCount DictEntry = EntryArrayOfStuff_Cpy[TopElements[i]];
                DictEntry = DictEntry.CreateCopy;
                --(DictEntry.Item1);
                EntryArrayOfStuff_Cpy[TopElements[i]] = DictEntry;
                SubSnugArrayElements[i] = new SnugArray(EntryArrayOfStuff_Cpy, (MaxValue - DictEntry.Item2), ListOfDictionaryOfID_Count);//creates new graph for current node
                EntryArrayOfStuff_Cpy.Remove(TopElements[i]);//removes current node from graph
            }
#endif

            


































 
  




        
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





        Dictionary<TimeSpan, ParentIDCount> generateBuckets(List<mTuple<int, TimeSpanWithStringID>> EntryArrayOfStuff)
        {
            EntryArrayOfStuff = EntryArrayOfStuff.OrderBy(obj => obj.Item2.timeSpan).ToList();
            EntryArrayOfStuff.Reverse();
            Dictionary<TimeSpan, ParentIDCount> retValue = new Dictionary<TimeSpan, ParentIDCount>();
            foreach (mTuple<int, TimeSpanWithStringID> eachmTuple in EntryArrayOfStuff)
            {
                //string key= eachTimeSpanWithID.TimeSpanID.ToString();
                TimeSpan key = eachmTuple.Item2.timeSpan;
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

        public List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> MySnugPossibleEntries
        {
            get
            {

                Dictionary<TimeSpan, TimeSpanWithStringID> Dict_StringAndDict = new Dictionary<TimeSpan, TimeSpanWithStringID>();
                List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> retValue = new List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();
                Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> singleDictionary = new Dictionary<TimeSpan,mTuple<int,TimeSpanWithStringID>>();

                foreach (KeyValuePair<TimeSpan, ParentIDCount> eachKeyPair in MyDict)
                {
                    Dict_StringAndDict.Add(eachKeyPair.Key, new TimeSpanWithStringID(eachKeyPair.Value.Item2, eachKeyPair.Key.Ticks.ToString()));
                    
                }


                if ((SubSnugArrayElements.Length < 1) && (TopElements.Length>0))
                {
                    //foreach (KeyValuePair<TimeSpan, ParentIDCount> eachKeyPair in ArrayOfStuff)
                    foreach (TimeSpan eachTimeSpan in TopElements)
                    {
                        singleDictionary.Add(eachTimeSpan, new mTuple<int, TimeSpanWithStringID>(ArrayOfStuff[eachTimeSpan].Item1, new TimeSpanWithStringID(eachTimeSpan, eachTimeSpan.Ticks.ToString()))); 
                    }
                }

                if (singleDictionary.Count > 0)//handles scenario where all the timespan fits the inintializing max timespan
                {
                    retValue.Add(singleDictionary);
                    return retValue;
                }
                

                int i=0;
                for (; i < SubSnugArrayElements.Length; i++)
                {


                    List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> Received_retValue = SubSnugArrayElements[i].MySnugPossibleEntries;

                    TimeSpan eachTimeSpan = TopElements[i];
                    foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in Received_retValue)
                    {
                        
                        if (eachDictionary.ContainsKey(eachTimeSpan))
                        {
                            ++eachDictionary[eachTimeSpan].Item1;
                        }
                        else 
                        {
                            eachDictionary.Add(eachTimeSpan, new mTuple<int, TimeSpanWithStringID>(1, Dict_StringAndDict[eachTimeSpan]));
                        }
                        
                    }
                    if (Received_retValue.Count < 1)
                    {
                        Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CurrentDict = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
                        CurrentDict.Add(eachTimeSpan, new mTuple<int, TimeSpanWithStringID>(1, Dict_StringAndDict[eachTimeSpan]));
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

        static public Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CreateCopyOFSnuPossibilities(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> SnugPossibility)
        {
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> retValue = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in SnugPossibility)
            {
                retValue.Add(eachKeyValuePair.Key,new mTuple<int, TimeSpanWithStringID>(eachKeyValuePair.Value.Item1, eachKeyValuePair.Value.Item2));
                
                //retValue[eachKeyValuePair.Key] = 
            }

            return retValue;
        }

        static public TimeSpan TotalTimeSpanOfSnugPossibility(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> SnugPossibility)
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

        static public Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> AddToSnugPossibilityList(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> PossibiityA, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> PossibiityB)
        {
            //handle null inputs

            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> retValue = CreateCopyOFSnuPossibilities(PossibiityA);


            foreach (TimeSpan eachTimeSpan in PossibiityB.Keys)
            {
                if (retValue.ContainsKey(eachTimeSpan))
                {
                    retValue[eachTimeSpan].Item1 += PossibiityB[eachTimeSpan].Item1;
                }
                else 
                { 
                    retValue.Add(eachTimeSpan, new mTuple<int,TimeSpanWithStringID>(PossibiityB[eachTimeSpan].Item1,PossibiityB[eachTimeSpan].Item2));
                }
            }
            return retValue;
        }

        static public Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> RemoveSnugPossibilityFromAnother(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> PossibiityA, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> PossibiityB)
        {
            //handle null inputs
            //You remove PossibilityB from PossibilityA

            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> retValue = CreateCopyOFSnuPossibilities(PossibiityA);


            foreach (TimeSpan eachTimeSpan in PossibiityB.Keys)
            {
                if (retValue.ContainsKey(eachTimeSpan))
                {
                    retValue[eachTimeSpan].Item1 -= PossibiityB[eachTimeSpan].Item1;
                    if (retValue[eachTimeSpan].Item1 < 1)
                    {
                        retValue.Remove(eachTimeSpan);
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

        public static List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> SortListSnugPossibilities_basedOnTimeSpan(List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> ListOfSnugPossibilities, TimeSpanWithStringID myTimesSpan = null)
        {
            Dictionary<TimeSpan, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> AllData = new Dictionary<TimeSpan, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
            Dictionary<TimeSpan, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> AllData_AboveAverage = new Dictionary<TimeSpan, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
            Dictionary<TimeSpan, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> PertinentDict;
            foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in ListOfSnugPossibilities)
            {
                TimeSpan TotalTime= TotalTimeSpanOfSnugPossibility(eachDictionary);
                if (myTimesSpan != null)
                {
                    if (TotalTime >= myTimesSpan.timeSpan)
                    {   
                        if (AllData_AboveAverage.ContainsKey(TotalTime))
                        {
                            AllData_AboveAverage[TotalTime].Add(eachDictionary);
                        }
                        else
                        {
                            AllData_AboveAverage.Add(TotalTime, new List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>() { eachDictionary });
                        }
                    }
                }
                
                
                if (AllData.ContainsKey(TotalTime))
                {
                    AllData[TotalTime].Add(eachDictionary);
                }
                else 
                {
                    AllData.Add(TotalTime, new List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>() { eachDictionary });
                }
            }

            PertinentDict = AllData_AboveAverage.Count > 0 ? AllData_AboveAverage : AllData;
            List<TimeSpan> AllKeys;
            AllKeys = PertinentDict.Keys.ToList();


            AllKeys.Sort();
            
            if (AllData_AboveAverage.Count > 0)
            {
                IEnumerable<KeyValuePair<TimeSpan, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>> dict_asList = AllData_AboveAverage;
                dict_asList.OrderBy(obj => obj.Value.Count);
                AllKeys = dict_asList.Select(obj => obj.Key).ToList();
                AllKeys.Reverse();
            }



            

            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> retValue = new List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();

            foreach (TimeSpan eachTimeSpan in AllKeys)
            {
                PertinentDict[eachTimeSpan] = PertinentDict[eachTimeSpan].OrderBy(obj => obj.Count).ToList();
                retValue.AddRange(AllData[eachTimeSpan]);
            }
            return retValue;

        }



        public static Dictionary<int, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> SortListSnugPossibilities_basedOnNumberOfDiffering(List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> ListOfSnugPossibilities, TimeSpanWithStringID myTimesSpan = null) 
        {
            Dictionary<int, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> retValue = new Dictionary<int, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();

            foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in ListOfSnugPossibilities)
            {
                if (retValue.ContainsKey(eachDictionary.Count))
                {
                    retValue[eachDictionary.Count].Add(eachDictionary);
                }
                else
                {
                    retValue.Add(eachDictionary.Count, new List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> { eachDictionary });
                }
            }

            Parallel.ForEach(retValue.Values, currentList => {
                currentList=SortListSnugPossibilities_basedOnTimeSpan(currentList);
            });

            return retValue;
        }

        public TimeSpan[] MyTopElements
        {
            get
            {
                return TopElements;
            }
        }
    }

}
