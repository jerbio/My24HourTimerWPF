using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    class TupleDict
    {
        string CompoundedKey;
        List<List<int>> MatchedIDCount;
        Dictionary<string, TupleDict> ExtendedDictTuples;
        
        public TupleDict()
        {
            ExtendedDictTuples = new Dictionary<string, TupleDict>();
            CompoundedKey = null;
            MatchedIDCount = new List<List<int>>();
        }

        public TupleDict(string CompoundedKey, List<int> NumberOFIDMatch)
        {
            this.CompoundedKey = CompoundedKey;
            MatchedIDCount = new List<List<int>>();
            MatchedIDCount.Add(NumberOFIDMatch);
            ExtendedDictTuples = new Dictionary<string, TupleDict>();
        }
        
        public TupleDict(Dictionary<string, int> Arg1)
        { 
            
        }

        public void UpdateDictTuple(Dictionary<string, int> Arg1)
        {
            string []KeyArray =  Arg1.Keys.ToArray();
            string key2String=string.Join("#",KeyArray);


            if (key2String != CompoundedKey)
            {
                goThroughExtendedDictTuples(key2String, Arg1);
            }
            else 
            {
                foreach (List<int> eachListOfInt in MatchedIDCount)
                {
                    foreach (int eachInt in eachListOfInt)
                    {
                        if (true)
                        {
                            //retValue = true;
                        }
                        else
                        {
                            //retValue = false;
                            break;
                        }
                    }
                }
            }
            
        }

        private void goThroughExtendedDictTuples(string key2String, Dictionary<string, int> Arg1)
        {
            int i = 0;
            string key2String_cpy = string.Copy(key2String);
            bool noExitWhile=true;
            while ((key2String_cpy != "") || (noExitWhile))
            {
                key2String_cpy = getCompundString(key2String_cpy, i++);//getsSubring Compounded String

                noExitWhile = !(ExtendedDictTuples.ContainsKey(key2String_cpy));
            }

            if (key2String_cpy == "")
            { 
                ExtendedDictTuples.Add(key2String,new TupleDict(key2String,Arg1.Values.ToList()));
            }
            else
            {
                ExtendedDictTuples[key2String_cpy].UpdateDictTuple(Arg1);
            }

            
        }

        string getCompundString(string key2String, int indexFromBack)
        {
            List<string> key2StringSplit = key2String.Split('#').ToList();
            key2StringSplit.RemoveRange(key2StringSplit.Count - indexFromBack, indexFromBack);
            key2String=string.Join("#",key2StringSplit);
            return key2String;

        }
    }
}
