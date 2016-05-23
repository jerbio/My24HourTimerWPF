using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class EventID
    {
        private static int CalendarEvenntLimitIndex = 2;
        string[] LayerID = new string[4] { "0", "7", "0", "0"};
        string s_FullID="";
        int FullID;
        static string delimiter = "_";
        static char delimiter_char ='_';
        public EventID(string stringId)
        {
            if (!string.IsNullOrEmpty(stringId))
            {
                Initializer(stringId.Split('_'));
            }
            else
            {
                throw new ArgumentNullException("stringId", "Invalid attempt to create an EventId object from a null string object ");
            }

        }

        public EventID()
        {
        }
        private EventID(string[] myLayerID)
        {

            Initializer(myLayerID);

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

        void Initializer(string[] myLayerID)
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
                        LayerID = new string[4] { myLayerID[0], "7", myLayerID[1], "0" };
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

            s_FullID = string.Join(delimiter, LayerID);
        }

        public static EventID convertToSubcalendarEventID(string stringID)
        {
            string[] splitStringResult = stringID.Split(delimiter_char);
            EventID retValue = new EventID();
            if (splitStringResult.Length > 3)
            {
                retValue.LayerID[0] = splitStringResult[0];
                retValue.LayerID[1] = splitStringResult[1];
                retValue.LayerID[2] = splitStringResult[2];
                retValue.LayerID[3] = splitStringResult[3];
            }
            else 
            {
                if (splitStringResult.Length > 2)
                {
                    retValue.LayerID[0] = splitStringResult[0];
                    retValue.LayerID[2] = splitStringResult[1];
                    retValue.LayerID[3] = splitStringResult[2];
                }
                else
                {
                    retValue.LayerID[0] = splitStringResult[0];
                    retValue.LayerID[3] = splitStringResult[1];
                }
                
            }
            retValue.updateSFullID();
            return retValue;
        }

        private void updateSFullID()
        {
            s_FullID=string.Join("_", LayerID);
        }

        public static EventID convertToRepeatCalendarEventID(string stringID)
        {
            string[] splitStringResult = stringID.Split(delimiter_char);
            EventID retValue = new EventID();
            if (splitStringResult.Length > 3)
            {
                retValue.LayerID[0] = splitStringResult[0];
                retValue.LayerID[1] = splitStringResult[1];
                retValue.LayerID[2] = splitStringResult[2];
                retValue.LayerID[3] = splitStringResult[3];
            }
            else
            {
                retValue.LayerID[0] = splitStringResult[0];
                retValue.LayerID[2] = splitStringResult[1];
            }
            return retValue;
        }



        private void AddNewComponentID(int index)
        { 
            string id= EventIDGenerator.generate().ToString();
            LayerID[index] = id;
            s_FullID = string.Join(delimiter, LayerID);
        }

        public static EventID GenerateCalendarEvent()
        {
            EventID retValue = new EventID("0_7_0_0");
            retValue.AddNewComponentID(0);
            return retValue;
        }


        public static EventID generateGoogleAuthenticationID(uint CurrentIndex)
        {
            EventID retValue = new EventID((int)ThirdPartyControl.CalendarTool.Google + "_" + CurrentIndex + "_0_0");
            return retValue;
        }

        public static EventID generateGoogleCalendarEventID(uint CurrentIndex)
        {
            EventID retValue = new EventID((int)ThirdPartyControl.CalendarTool.Google + "_0_" + CurrentIndex + "_0");
            return retValue;
        }

        public static EventID generateGoogleSubCalendarEventID(EventID CalendarEventID)
        {
            EventID retValue = new EventID(CalendarEventID.getIDUpToRepeatCalendarEvent() + "_1");
            return retValue;
        }

        public static EventID generateRepeatGoogleSubCalendarEventID(EventID CalendarEventID, uint currentIndex)
        {
            EventID retValue = new EventID( CalendarEventID.getIDUpToRepeatCalendarEvent() + "_" + currentIndex);
            return retValue;
        }

        public static EventID generateFacebookCalendarEventID()
        {
            EventID retValue = new EventID((int)ThirdPartyControl.CalendarTool.Facebook + "_7_0_0");
            return retValue;
        }

        public static EventID generateOutlookCalendarEventID()
        {
            EventID retValue = new EventID((int)ThirdPartyControl.CalendarTool.Facebook + "_7_0_0");
            return retValue;
        }


        public static EventID GenerateRepeatCalendarEvent(string ParentID)//,int weekDay=7)
        {
            EventID retValue = new EventID(ParentID);
            {
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

        public static EventID GenerateSubCalendarEvent(EventID ParentID)
        {
            EventID retValue = new EventID(ParentID.ToString());
            //if (retValue.LayerID.Count == 3)
            {
                retValue.AddNewComponentID(3);
                return retValue;
            }
        }


        private string getStringIDAtLevel(int LevelIndex)
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

        private string getLevelID(int Level)
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






        public string getCalendarEventID()
        {
            string retValue = getIDUpToCalendarEvent() + "_7_0_0";
            return retValue;
        }

        public string getRepeatCalendarEventID()
        {
            string retValue = getIDUpToRepeatCalendarEvent() + "_0";
            return retValue;
        }

        public string getRepeatDayCalendarEventID()
        {
            string retValue = getIDUpToRepeatDayCalendarEvent() + "_0_0";
            return retValue;
        }

        public string getSubCalendarEventID()
        {
            string retValue = getIDUpToSubCalendarEvent();
            return retValue;
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
