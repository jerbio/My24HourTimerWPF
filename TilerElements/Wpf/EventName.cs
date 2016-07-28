using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements.Wpf
{
    public class EventName
    {
        /// <summary>
        /// Id to event name
        /// </summary>
        protected string ID { set; get; } = Guid.NewGuid().ToString();
        protected string _Name = "";
        /// <summary>
        /// Id with which this event is associated 
        /// </summary>
        protected string _EventId;
        protected string _SecondaryEventId;

        public EventName()
        {

        }
        
        public EventName(string EventId, string Name= "")
        {
            _Name = Name;
            _EventId = EventId;
        }

        public EventName(EventID EventId, string Name)
        {
            _Name = Name;
            _EventId = EventId.ToString() ;
        }

        public EventName( EventID EventId)
        {
            _Name = "";
            _EventId = EventId.ToString() ;
        }
        public override string ToString()
        {
            return NameString.ToString();
        }

        public EventName CreateCopy()
        {
            EventName RetValue = new EventName { ID = this.ID,
                _Name = this.NameString, _EventId = this._EventId };

            return RetValue;
        }

        public string NameString
        {
            get
            {
                return _Name;
            }
        }

        public string AssosciatedEventId
        {
            get
            {
                return _EventId;
            }
        }

        public string AssociatedSecondaryEventId
        {
            get
            {
                return _SecondaryEventId;
            }
        }


        virtual public string Id
        {
            get
            {
                return ID;

            }
            set
            {
                Guid testValue;
                if (Guid.TryParse(value, out testValue))
                {
                    ID = value;
                }
                else
                {
                    throw new Exception("Invalid id for event event Name");
                }

            }
        }

    }
}
