using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class EventName
    {
        /// <summary>
        /// Id to event name
        /// </summary>
        protected string ID { set; get; } = Guid.NewGuid().ToString();
        protected string ActualName { set; get; } = "";
        /// <summary>
        /// Id with which this event is associated 
        /// </summary>
        protected string EventID { set; get; }

        public EventName()
        {

        }
        
        public EventName(string EventId, string Name= "")
        {
            ActualName = Name;
            EventID = EventId;
        }

        public EventName(EventID EventId, string Name)
        {
            ActualName = Name;
            EventID = EventId.ToString() ;
        }

        public EventName( EventID EventId)
        {
            ActualName = "";
            EventID = EventId.ToString() ;
        }
        public override string ToString()
        {
            return Name.ToString();
        }

        public EventName CreateCopy()
        {
            EventName RetValue = new EventName { ID = this.ID,
                ActualName = this.Name, EventID = this.EventID };

            return RetValue;
        }

        public string Name
        {
            get
            {
                return Name;
            }
        }

        public string EventId
        {
            get
            {
                return EventId;
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
