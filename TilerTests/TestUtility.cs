using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;

namespace TilerTests
{
    public static class TestUtility
    {
        const int _MonthLimit = 3;
        static readonly DateTimeOffset _Start = DateTimeOffset.UtcNow.AddMonths(-MonthLimit);
        static readonly Random _Rand = new Random((int)DateTimeOffset.Now.Ticks);
        static readonly string _UserName = "TestUserTiler";
        static readonly string _Password = "T35tU53r#";


        public static int MonthLimit
        {
            get
            {
                return _MonthLimit;
            }
        }

        public static DateTimeOffset Start
        {
            get
            {
                return _Start;
            }
        }
        public static Random Rand
        {
            get { return _Rand; }
        }

        public static string UserName
        {
            get {
                return _UserName;
            }
        }
        public static string Password
        {
            get
            {
                return _Password;
            }
        }

        public static bool isTestEquivalent(this TilerEvent firstCalEvent, TilerEvent secondCalEvent)
        {
            bool retValue = true;
            Type eventType = secondCalEvent.GetType();
            {
                if (firstCalEvent.Id == secondCalEvent.Id)
                {
                    if ((firstCalEvent.Start == secondCalEvent.Start) && (firstCalEvent.End == secondCalEvent.End))
                    {
                        if (firstCalEvent.ProcrastinationInfo.isTestEquivalent(secondCalEvent.ProcrastinationInfo) 
                            && firstCalEvent.NowInfo.isTestEquivalent(secondCalEvent.NowInfo)
                            )
                        {
                            retValue = true;
                        }
                        else
                        {
                            retValue = false;
                        }
                        
                    }
                    else
                    {
                        retValue = false;
                    }
                }
                else
                {
                    retValue = false;
                }
            }
            return retValue;
        }

        public static bool isTestEquivalent(this Procrastination firstProcrastination, Procrastination secondProcrastination)
        {
            bool retValue = true;
            {
                if (firstProcrastination.DislikedDayIndex == secondProcrastination.DislikedDayIndex)
                {
                    if ((firstProcrastination.DislikedDayOfWeek == secondProcrastination.DislikedDayOfWeek) 
                        && (firstProcrastination.DislikedDaySection == secondProcrastination.DislikedDaySection) 
                        && (firstProcrastination .DislikedStartTime == secondProcrastination.DislikedStartTime) 
                        && (secondProcrastination.PreferredDayIndex == firstProcrastination.PreferredDayIndex) 
                        && (secondProcrastination.PreferredStartTime == firstProcrastination.PreferredStartTime))
                    {
                        retValue = true;
                    }
                    else
                    {
                        retValue = false;
                    }
                }
                else
                {
                    retValue = false;
                }
            }
            return retValue;
        }

        public static bool isTestEquivalent(this NowProfile firstNow, NowProfile secondNow)
        {
            bool retValue = true;
            {
                if ((firstNow.isInitialized == secondNow.isInitialized) 
                    && (firstNow.PreferredTime == secondNow.PreferredTime))
                {
                    retValue = true;
                }
                else
                {
                    retValue = false;
                }
            }
            return retValue;
        }
    }
}

