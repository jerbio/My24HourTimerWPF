using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class MiscData
    {
        string _Id;
        string _UserTypedData;
        int _Type;//Entry Source. 0-> No Entry from Calendar Event. 1->From CalendarEvent. 2-> from SubCalendarEvent

        #region constructor

        

        public MiscData()
        {
            _UserTypedData = "";
            _Type = 0;
        }
        
        public MiscData(string TypedNotes, int Type=0)
        {
            _UserTypedData = TypedNotes;
            this._Type = Type;
        }
        #endregion

        #region Function
        public MiscData createCopy()
        {
            MiscData retValue = new MiscData();
            retValue._Type = _Type;
            retValue._UserTypedData = _UserTypedData;

            return retValue;
        }

        #endregion



        #region property

        public string UserNote
        {
            get
            {
                return _UserTypedData;
            }
        }


        public int TypeSelection
        {
            get
            {
                return _Type;
            }
        }

        #region dbProperties
        public int Type
        {
            get
            {
                return _Type;
            }

            set
            {
                _Type = value;
            }
        }


        public string UserTypedData
        {
            get
            {
                return _UserTypedData;
            }

            set
            {
                _UserTypedData = value;
            }
        }

        public string Id
        {
            get
            {
                return _Id ?? (_Id = Guid.NewGuid().ToString());
            }
            set
            {
                _Id = value;
            }
        }

        #endregion
        #endregion

    }
}
