using System;
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
        List<TimeSpanWithID> ConstainedElements;
        Dictionary<string, List<TimeSpanWithID>> ParentID_TimeSpanID;
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

        public SnugArray(List<TimeSpanWithID> ConstrainedElements, List<TimeSpanWithID> EntryArrayOfStuff, TimeSpan MaxValue)
        {
            EntryArrayOfStuff=EntryArrayOfStuff.OrderBy(obj=>obj.TimeSpanID.ToString()).ToList();
            
            
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

            int i = 0;
            this.ConstainedElements = new List<TimeSpanWithID>();
            int j = 0;
            for (; j < ConstrainedElements.Count; j++)
            {

                TimeSpanWithID myConstrainedSubCalEvent = ConstrainedElements[j];
                ConstrainedElements.Remove(myConstrainedSubCalEvent);
                --j;
                this.ConstainedElements.Add(myConstrainedSubCalEvent);
                MaxValue = MaxValue - myConstrainedSubCalEvent.timeSpan;
                i = 0;
                for (; i < EntryArrayOfStuff.Count; i++)
                {
                    if (EntryArrayOfStuff[i].TimeSpanID.ToString() == myConstrainedSubCalEvent.TimeSpanID.ToString())
                    {
                        EntryArrayOfStuff.RemoveAt(i);
                        // if (MyDict.ContainsKey(myConstrainedSubCalEvent.TimeSpanID.getCalendarEventID()))
                        {

                            //(MyDict[myConstrainedSubCalEvent.TimeSpanID.getCalendarEventID()]);
                        }
                        //else
                        {

                            // MyDict.Add(myConstrainedSubCalEvent.TimeSpanID.getCalendarEventID(), 1);
                        }

                        break;
                    }
                }
            }

            Dictionary<string, ParentIDCount> TimeSpanBucket = generateBuckets(EntryArrayOfStuff);

            

            //SnugArray MyThis = new SnugArray(ConstrainedElements, EntryArrayOfStuff, MaxValue, new Dictionary<string, int>(), new List<Dictionary<string, int>>());
            SnugArray MyThis = new SnugArray(ConstrainedElements, TimeSpanBucket, MaxValue, new List<Dictionary<string, int>>());


            TopElements=MyThis.TopElements;
            SubSnugArrayElements=MyThis.SubSnugArrayElements;
            ListOfDictionaryOfID_Count=MyThis.ListOfDictionaryOfID_Count;//This is a List of Dictionaries with a string of ID and a count of the IDs found
            AlreadyFoundPermutation=MyThis.AlreadyFoundPermutation;
            //ConstainedElements=MyThis.ConstainedElements;
            MyDict = MyThis.MyDict;
        }

        private SnugArray(List<TimeSpanWithID> ConstrainedElements, Dictionary<string, ParentIDCount> EntryArrayOfStuff, TimeSpan MaxValue, List<Dictionary<string, int>> MyListOfDicts)
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
            
            TopElements = SmallerElements.ToArray();
            int LengthOfSubElmentArray = TopElements.Length;
            SubSnugArrayElements = new SnugArray[LengthOfSubElmentArray];

            i = 0;

            string[] MyFittableElemtsHolder = new string[SmallerElements.Count];//Array holds the current array of subelements for future reasons
            List<string> MyFittableElementsHolderList = new List<string>();//This is also holds a curent array of sub elements.
            SmallerElements.CopyTo(MyFittableElemtsHolder, 0);
            MyFittableElementsHolderList = MyFittableElemtsHolder.ToList();
            MyArrayCount = SmallerElements.Count;
            MyDict = EntryArrayOfStuff;

            if ((EntryArrayOfStuff.Count < 1) && (TopElements.Length > 0))
            {
                ;
            }

            Dictionary<string, ParentIDCount> EntryArrayOfStuff_Cpy = new Dictionary<string, ParentIDCount>(EntryArrayOfStuff);

            for (; i < MyArrayCount; i++)
            {
                //Dictionary<string, int> MyDictCpy = new Dictionary<string, int>(MyDict);
                
                ParentIDCount DictEntry= EntryArrayOfStuff_Cpy[TopElements[i]];
                //MyFittableElementsHolderList.Remove(TopElements[i]);//removes a value that meets criteria of less than max value. This is used to form a node. The sub nodes will be have only values less than or equal to Max Value-other elements
                if((i==3)&&(TopElements[i]=="450_452"))
                {
                ;
                }

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
                SubSnugArrayElements[i] = new SnugArray(ConstrainedElements, EntryArrayOfStuff_Cpy, (MaxValue - DictEntry.Item2), ListOfDictionaryOfID_Count);//creates new snug array with elements that are smaller
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





        Dictionary<string, ParentIDCount> generateBuckets(List<TimeSpanWithID> EntryArrayOfStuff)
        {
            Dictionary<string, ParentIDCount> retValue = new Dictionary<string, ParentIDCount>();
            foreach (TimeSpanWithID eachTimeSpanWithID in EntryArrayOfStuff)
            {
                //string key= eachTimeSpanWithID.TimeSpanID.ToString();
                string key = eachTimeSpanWithID.TimeSpanID.getCalendarEventID();
                
                if (retValue.ContainsKey(key))
                {
                    ++(retValue[key].Item1);
                }
                else
                {
                    
                    
                    retValue.Add(key, new ParentIDCount(1,eachTimeSpanWithID.timeSpan));
                }
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

        List<List<TimeSpanWithID>> GenerateSnugPossibilities(string MyID)
        {
            //List<string> MyFoundID = MyID.Split('#').ToList();

            List<List<TimeSpanWithID>> JustAllSubPossibilities;
            int i = 0;
            List<TimeSpanWithID> MyCurrentList = new List<TimeSpanWithID>();
            int j = 0;
            JustAllSubPossibilities = new List<List<TimeSpanWithID>>();
            List<List<TimeSpanWithID>> MyTotalSubPossibilities = new List<List<TimeSpanWithID>>(); ;
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

        public List<List<TimeSpanWithID>> MySnugPossibleEntries
        {
            get
            {

                Dictionary<string, TimeSpanWithID> Dict_StringAndDict = new Dictionary<string, TimeSpanWithID>();
                List<List<TimeSpanWithID>> retValue = new List<List<TimeSpanWithID>>();
                
                
                foreach (KeyValuePair<string, ParentIDCount> eachKeyPair in MyDict)
                {
                    Dict_StringAndDict.Add(eachKeyPair.Key, new TimeSpanWithID(eachKeyPair.Value.Item2.Ticks, new EventID(eachKeyPair.Key)));
                }
               
                int i=0;
                for (; i < TopElements.Length;i++ )
                {


                    List<List<TimeSpanWithID>>  Received_retValue = SubSnugArrayElements[i].MySnugPossibleEntries;
                    List<TimeSpanWithID> CurrentLine = new List<TimeSpanWithID>();
                    CurrentLine.Add(Dict_StringAndDict[TopElements[i]]);
                    if(ConstainedElements!=null)
                    {
                        CurrentLine.AddRange(ConstainedElements);
                    }

                    foreach (List<TimeSpanWithID> eachListOfTimeSpanWithID in Received_retValue)
                    {
                        eachListOfTimeSpanWithID.AddRange(CurrentLine);

                    }
                    if (Received_retValue.Count < 1)
                    { 
                        //CurrentLine.Add(Dict_StringAndDict[TopElements[i]]);
                        retValue.Add(CurrentLine);
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

        public string[] MyTopElements
        {
            get
            {
                return TopElements;
            }
        }
    }

}
