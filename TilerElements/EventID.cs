using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class EventID
    {
        private static int CalendarEvenntLimitIndex = 2;
        string[] LayerID = new string[4];
        string s_FullID="";
        int FullID;
        static string delimiter = "_";
        public EventID(string myLayerID)
            : this(myLayerID.Split('_'))
        {

        }

        public EventID()
        {
        }
        private EventID(string[] myLayerID)
        {
            
            switch (myLayerID.Length)
            {
                case 0:
                {
                    LayerID = new string[4] { "0", "7", "0", "0" };
                }
                break;
                
                case 1:
                {
                    LayerID = new string[4] { myLayerID[0], "7", "0", "0" };
                }
                break;
                case 2:
                {
                    LayerID = new string[4] { myLayerID[0], "7", myLayerID[1],"0" };
                }
                break;
                case 3:
                {
                    LayerID = new string[4] { myLayerID[0], "7", myLayerID[1], myLayerID[2] };
                }
                break;
                case 4:
                {
                    LayerID = myLayerID.ToArray();
                }
                break;
                default:
                throw new Exception("Tried To initialize with invalid ID");
            }

            s_FullID = string.Join(delimiter,LayerID);

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

        private void AddNewComponentID(int index)
        { 
            string id= EventIDGenerator.generate().ToString();
            LayerID[index] = id;
            s_FullID = string.Join(delimiter, LayerID);
        }

        public static EventID GenerateCalendarEvent()
        {
            EventID retValue = new EventID();
            retValue.AddNewComponentID(0);
            return retValue;
        }

        
        

        public static EventID GenerateRepeatCalendarEvent(string ParentID)//,int weekDay=7)
        {
            EventID retValue = new EventID(ParentID);
            {
                retValue.LayerID[1] = "7";
                retValue.AddNewComponentID(2);
                return retValue;
            }
        }

        public static EventID GenerateRepeatDayCalendarEvent(string ParentID,int weekDay)
        {
            EventID retValue = new EventID(ParentID);
            {
                retValue.LayerID[1]=weekDay.ToString();
                retValue.AddNewComponentID(2);
                return retValue;
            }
        }

        public static EventID GenerateSubCalendarEvent(string ParentID)
        {
            EventID retValue = new EventID(ParentID);
            //if (retValue.LayerID.Count == 3)
            {
                retValue.AddNewComponentID(3);
                return retValue;
            }
        }


        public string getStringIDAtLevel(int LevelIndex)
        {
            int i = 0;
            string StringID = "";
            for (i = 0; (i <= LevelIndex - 1) && (LevelIndex < LayerID.Length); i++)
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

        public string getIDUpToCalendarEvent()
        {
            return getStringIDAtLevel(0);
        }

        public string getIDUpToRepeatDayCalendarEvent()
        {
            return getStringIDAtLevel(1);
        }

        public string getIDUpToRepeatCalendarEvent()
        {
            return getStringIDAtLevel(2);
        }

        public string getIDUpToSubCalendarEvent()
        {
            return getStringIDAtLevel(3);
        }


        public string getCalendarEventComponent()
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
            if ((LayerID.Length== 1) && (LayerID[0] == ""))//checks if LayerID is empty
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
