using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    public static class EventIDGenerator
    {
        static uint idcounter = 0;

        static bool AlreadyInitialized = false;
        public static void Initialize(uint LastID)
        {
            idcounter = LastID;
        }



        public static uint generate()
        {
            //update xml file with last counter
            return ++idcounter;
        }
    }
}
