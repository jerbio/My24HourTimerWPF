using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Wpf;

namespace TilerElements.DB
{
    class DB_EventName : EventName
    {
        #region properties
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        public string EventId
        {
            get
            {
                return _EventId;
            }
            set
            {
                _EventId = value;
            }
        }

        public string SecondaryEventId
        {
            get
            {
                return _SecondaryEventId;
            }
            set
            {
                _SecondaryEventId = value;
            }
        }

        public static DB_EventName ConvertToPersistable(EventName eventName)
        {
            DB_EventName retValue = new DB_EventName()
            {
                Name = eventName.NameString,
                _EventId = eventName.AssosciatedEventId,
                _SecondaryEventId = eventName.AssociatedSecondaryEventId
            };
            retValue.Id = eventName.Id;
            return retValue;
        }
        #endregion 
    }
}
