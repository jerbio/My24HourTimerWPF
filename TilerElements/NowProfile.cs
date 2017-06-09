using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class NowProfile
    {
        protected string _Id { get; set; }
        protected TilerEvent _AssociatedEvent { get; set; }
        DateTimeOffset TimePreferredForEvent;
        bool Initialized = false;

        public NowProfile()
        {
            TimePreferredForEvent = new DateTimeOffset();
            Initialized = false;
        }

        public NowProfile(DateTimeOffset CurrentTime, bool InitializedData)
        {
            TimePreferredForEvent = CurrentTime;
            Initialized = InitializedData;
        }

        public NowProfile CreateCopy()
        {
            NowProfile retValue = new NowProfile(TimePreferredForEvent, Initialized);
            retValue.Id = this.Id;
            return retValue;
        }
        public DateTimeOffset PreferredTime
        {
            get
            {
                return TimePreferredForEvent;
            }
            set
            {
                TimePreferredForEvent = value;
            }
        }

        public string Id
        {
            get
            {
                return _Id;
            }
            set
            {
                _Id = value;
            }
        }

        [ForeignKey("Id")]
        public TilerEvent AssociatedEvent
        {
            get {
                return _AssociatedEvent;
            }
            set
            {
                _AssociatedEvent = value;
            }
        }

        
        public void reset()
        {
            TimePreferredForEvent = new DateTimeOffset();
            Initialized = false;
        }

        public bool isInitialized
        {
            get
            {
                return Initialized;
            }
        }
    }
}
