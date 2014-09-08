using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class EventID
    {
        private static int CalendarEvenntLimitIndex = 2;
        List<string> LayerID;
        string s_FullID="";
        int FullID;
        static string delimiter = "_";
        public EventID(string myLayerID)
            : this(myLayerID.Split('_').ToList())
        {

        }

        public EventID()
        {
            LayerID = new List<string>();
        }
        private EventID(List<string> myLayerID)
        {
            LayerID = myLayerID;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string myId = "";
            foreach (string eachString in LayerID)
            {
                sb.Append(eachString);
                myId += eachString;
            }

            s_FullID = myId;

            /*//string currConcat=sb.ToString();
            string currConcat = myId;
            if (string.IsNullOrEmpty(currConcat))
            {
                FullID = 0;
            }
            else
            {
                FullID = Convert.ToInt32(currConcat);
            }*/

        }

        /*
        public string[] ID
        {
            get
            {
                return LayerID;
            }
        }
        */

        private void AddNewComponentID()
        { 
            string id= EventIDGenerator.generate().ToString();
            LayerID.Add(id);
            s_FullID = string.Join(delimiter, LayerID);
        }

        public static EventID GenerateCalendarEvent()
        {
            EventID retValue = new EventID();
            retValue.AddNewComponentID();
            return retValue;
        }

        public static EventID GenerateRepeatCalendarEvent(string ParentID)
        {
            EventID retValue = new EventID(ParentID);
            if (retValue.LayerID.Count == 1)
            {
                retValue.AddNewComponentID();
                return retValue;
            }
            else
            {
                throw new Exception("Invalid parent ID used for GenerateRepeatCalendarEvent");
            }
        }

        public static EventID GenerateRepeatDayCalendarEvent(string ParentID)
        {
            EventID retValue = new EventID(ParentID);
            if (retValue.LayerID.Count == 2)
            {
                retValue.AddNewComponentID();
                return retValue;
            }
            else
            {
                throw new Exception("Invalid parent ID used for GenerateRepeatDayCalendarEvent");
            }
        }

        public static EventID GenerateSubCalendarEvent(string ParentID)
        {
            EventID retValue = new EventID(ParentID);
            if (retValue.LayerID.Count > 1)
            {
                retValue.AddNewComponentID();
                return retValue;
            }
            else
            {
                throw new Exception("Invalid parent ID used for GenerateSubCalendarEvent");
            }
        }


        public string getStringIDAtLevel(int LevelIndex)
        {
            int i = 0;
            string StringID = "";
            for (i = 0; (i <= LevelIndex - 1) && (LevelIndex < LayerID.Count); i++)
            {
                StringID += LayerID[i] + "_";
            }
            StringID += LayerID[LevelIndex];
            return StringID;

        }

        public string getLevelID(int Level)
        {
            return LayerID[Level];
        }

        public int getLevelID_Int(int Level)
        {
            return  Convert.ToInt32( LayerID[Level]);
        }



        public string getCalendarEventPartition()
        {
            return getLevelID(0);
        }

        public string getRepeatCalendarEventComponent()
        {
            return getLevelID(1);
        }

        public string getRepeatDayCalendarEventComponent()
        {
            return getLevelID(2);
        }

        public string getSubCalendarEventComponent()
        {
            return getLevelID(3);
        }


        


        public override string ToString()
        {
            if ((LayerID.Count== 1) && (LayerID[0] == ""))//checks if LayerID is empty
            {
                return "";
            }
            return s_FullID;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }


            EventID p = obj as EventID;
            if ((System.Object)p == null)
            {
                return false;
            }

            return (this.s_FullID == p.s_FullID);
        }

        public override int GetHashCode()
        {
            return s_FullID.GetHashCode();
        }

        public static uint LatestID
        {
            get
            {
                uint retValue=EventIDGenerator.LatestID;
                return retValue;
            }
        }

        public static void Initialize(uint LastID)
        {
            EventIDGenerator.Initialize(LastID);
        }

        

        private static class EventIDGenerator
        {
            static uint idcounter = 0;

            static bool AlreadyInitialized = false;
            public static void Initialize(uint LastID)
            {
                idcounter = LastID;
            }

            public static uint generate()
            {
                //update xml file with last counter
                return ++idcounter;
            }

            public static uint LatestID
            {
                get
                {
                    return idcounter;
                }
            }
        }
    }

    
     
}
