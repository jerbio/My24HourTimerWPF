using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements.Wpf
{
    public class NowProfile
    {
        protected string ID = Guid.NewGuid().ToString();
        protected DateTimeOffset TimePreferredForEvent;
        protected bool Initialized = false;

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
            return retValue;
        }
        public DateTimeOffset PreferredTime
        {
            get
            {
                return TimePreferredForEvent;
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
                    throw new Exception("Invalid id for now profile ID");
                }

            }
        }
    }
}
