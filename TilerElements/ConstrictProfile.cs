using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class ConstrictProfile
    {
        DateTime Start;
        DateTime End;
        public ConstrictProfile(DateTime StartTime,DateTime EndTime)
        {
            Start = StartTime;
            End=EndTime;
        }
    }
}
