using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class EventID
    {
        private static int CalendarEvenntLimitIndex = 2;
        string[] LayerID;
        string s_FullID;
        int FullID;
        public EventID(string myLayerID)
            : this(myLayerID.Split('_'))
        {

        }
        public EventID(string[] myLayerID)
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

        public string[] ID
        {
            get
            {
                return LayerID;
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

        public string getCalendarEventID()
        {
            if (LayerID.Length > CalendarEvenntLimitIndex)
            {
                return getStringIDAtLevel(1);
            }
            else
            {
                return getStringIDAtLevel(0);
            }
        }


        public override string ToString()
        {
            string IDCombination = "";
            if ((LayerID.Length == 1) && (LayerID[0] == ""))//checks if LayerID is empty
            {
                return "";
            }
            foreach (string MyString in LayerID)
            {
                IDCombination += MyString + "_";
            }
            return IDCombination.Substring(0, (IDCombination.Length - 1));
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
    }

    
     
}
