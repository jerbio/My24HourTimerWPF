using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class EventID
    {
        const int lengthOfField = 4;
        string pauseString = "";
        string[] LayerID = new string[lengthOfField] { "0", "7", "0", "0"};
        string eventID_string="";
        string fullString = "";
        static char delimiter_char = '_';
        static string delimiter = ""+ delimiter_char;
        static char pauseDelimiter_char = '*';
        static string pauseDelimiter = ""+ pauseDelimiter_char;
        
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
        }

        void Initializer(string[] myLayerID)
        {
            switch (myLayerID.Length)
            {
                case 0:
                    {
                        LayerID = new string[lengthOfField] { "0", "7", "0", "0" };
                    }
                    break;

                case 1:
                    {
                        LayerID = new string[lengthOfField] { myLayerID[0], "7", "0", "0" };
                    }
                    break;
                case 2:
                    {
                        LayerID = new string[lengthOfField] { myLayerID[0], myLayerID[1],"0", "0" };
                    }
                    break;
                case 3:
                    {
                        LayerID = new string[lengthOfField] { myLayerID[0], myLayerID[1], myLayerID[2], "0" };
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

            updateFullString();
        }

        public static EventID convertToSubcalendarEventID(string stringID)
        {
            string tileId = stringID.Split(pauseDelimiter_char)[0];
            string[] splitStringResult = tileId.Split(delimiter_char);
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
            retValue.updateFullString();
            return retValue;
        }

        public static bool isPauseId(string eventId)
        {
            return eventId.Contains(pauseDelimiter_char);
        }

        public static bool isPauseId(EventID eventId)
        {
            return eventId.ToString().Contains(pauseDelimiter_char);
        }

        /// <summary>
        /// Just updates the full tile event id, this concats the strings of all the tile id components using the delimiter
        /// </summary>
        private void updateTileIdString()
        {
            eventID_string = string.Join(delimiter, LayerID);
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
            string id = Guid.NewGuid().ToString();
            LayerID[index] = id;
            updateFullString();
        }

        /// <summary>
        /// Updates the pause id;
        /// </summary>
        private void AddPauseID()
        {
            string id = Guid.NewGuid().ToString();
            pauseString = id;
        }

        /// <summary>
        /// Updates the full string, this updates the full id which includes both the tile id and pauseId
        /// </summary>
        public void updateFullString()
        {
            updateTileIdString();
            string tileString = eventID_string.ToString();
            fullString = tileString;
            if(pauseString.isNot_NullEmptyOrWhiteSpace())
            {
                fullString += pauseDelimiter + pauseString + pauseDelimiter;
            }
        }

        public static EventID GenerateCalendarEvent()
        {
            EventID retValue = new EventID("0_7_0_0");
            retValue.AddNewComponentID(0);
            return retValue;
        }


        public static EventID generateGoogleAuthenticationID(uint CurrentIndex)
        {
            EventID retValue = new EventID((int)ThirdPartyControl.CalendarTool.google + "_" + CurrentIndex + "_0_0");
            return retValue;
        }

        public static EventID generateGoogleCalendarEventID(uint CurrentIndex)
        {
            EventID retValue = new EventID((int)ThirdPartyControl.CalendarTool.google + "_0_" + CurrentIndex + "_0");
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
            EventID retValue = new EventID((int)ThirdPartyControl.CalendarTool.facebook + "_7_0_0");
            return retValue;
        }

        public static EventID generateOutlookCalendarEventID()
        {
            EventID retValue = new EventID((int)ThirdPartyControl.CalendarTool.facebook + "_7_0_0");
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
            retValue.AddNewComponentID(3);
            return retValue;
        }

        public static EventID GenerateSubCalendarEvent(EventID ParentID)
        {
            EventID retValue = new EventID(ParentID.ToString());
            retValue.AddNewComponentID(3);
            return retValue;
        }

        public static EventID GeneratePauseId(EventID ParentID)
        {
            EventID retValue = new EventID(ParentID.ToString());
            retValue.AddPauseID();
            retValue.updateFullString();
            return retValue;
        }


        private string getStringIDAtLevel(int LevelIndex)
        {
            int i = 0;
            string StringID = "";
            for (i = 0; (i <= LevelIndex - 1) && (LevelIndex < LayerID.Length); i++)
            {
                StringID += LayerID[i] + delimiter;
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
            return  Convert.ToInt32(LayerID[Level]);
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
            return EventID.convertToSubcalendarEventID(this.ToString()).ToString();
        }

        public string getCalendarEventComponent()
        {
            return getLevelID(0);
        }

        public string getRepeatCalendarEventComponent()
        {
            return getLevelID(2); 
        }

        public string getRepeatDayCalendarEventComponent()
        {
            return getLevelID(1);
        }

        public string getSubCalendarEventComponent()
        {
            return getLevelID(3);
        }

        public string getPauseComponent()
        {
            return this.pauseString;
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


        public static bool isLikeTilerId(string id)
        {
            EventID eventId = new EventID(id);
            string calComponent = eventId.getCalendarEventComponent();
            string dayComponent = eventId.getRepeatDayCalendarEventComponent();
            string repeatComponent = eventId.getRepeatCalendarEventComponent();
            string subCalendarComponent = eventId.getRepeatCalendarEventComponent();

            bool calComponentIsValid = calComponent.isGuid() || calComponent.isInt();
            bool dayComponentIsValid = dayComponent.isGuid() || dayComponent.isInt();
            bool repeatComponentIsValid = repeatComponent.isGuid() || repeatComponent.isInt();
            bool subCalendarComponentIsValid = subCalendarComponent.isGuid() || subCalendarComponent.isInt();

            return calComponentIsValid && dayComponentIsValid && repeatComponentIsValid && subCalendarComponentIsValid;
        }

        public override string ToString()
        {
            if ((LayerID.Length== 1) && (LayerID[0] == ""))//checks if LayerID is empty
            {
                return "";
            }
            return fullString;
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

            return (this.fullString == p.fullString);
        }

        public override int GetHashCode()
        {
            return fullString.GetHashCode();
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

        public string getAllEventDictionaryLookup
        {
            get
            {
                string retValue = this.getRepeatCalendarEventID();
                return retValue;
            }
        }

        private static class EventIDGenerator
        {
            static uint idcounter = 0;

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
