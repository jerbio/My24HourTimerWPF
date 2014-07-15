using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    public class MiscData
    {
        string UserTypedData;
        int Type;//Entry Source. 0-> No Entry from Calendar Event. 1->From CalendarEvent. 2-> from SubCalendarEvent

        public class MiscData_DB
        {
            string UserTypedData;
            int Type;
        }

        #region constructor

        

        public MiscData()
        {
            UserTypedData = "";
            Type = 0;
        }
        
        public MiscData(string TypedNotes, int Type=0)
        {
            UserTypedData = TypedNotes;
            this.Type = Type;
        }
        #endregion

        #region Function
        public MiscData createCopy()
        {
            MiscData retValue = new MiscData();
            retValue.Type = Type;
            retValue.UserTypedData = UserTypedData;

            return retValue;
        }

        #endregion



        #region property

        public string UserNote
        {
            get
            {
                return UserTypedData;
            }
        }


        public int TypeSelection
        {
            get
            {
                return Type;
            }
        }

        #endregion

    }
}
