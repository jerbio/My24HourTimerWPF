using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    public static class EventIDGenerator
    {
        static ulong idcounter = 0;

        static bool AlreadyInitialized = false;
        public static void Initialize(ulong LastID)
        {
            idcounter = LastID;
        }

        public static ulong generate()
        {
            //update xml file with last counter
            return ++idcounter;
        }

        public static ulong LatestID
        {
            get 
            {
                return idcounter;
            }
        }
    }
}
