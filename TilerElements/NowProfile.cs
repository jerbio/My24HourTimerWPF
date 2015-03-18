using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class NowProfile
    {
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
            return retValue;
        }
        public DateTimeOffset PreferredTime
        {
            get
            {
                return TimePreferredForEvent;
            }
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
