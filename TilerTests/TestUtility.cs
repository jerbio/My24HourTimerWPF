using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;
using My24HourTimerWPF;
using TilerFront;

namespace TilerTests
{
    public static class TestUtility
    {
        const int _MonthLimit = 3;
        static readonly DateTimeOffset StartOfTime = new DateTimeOffset();
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

        /// <summary>
        /// Function 
        /// </summary>
        /// <returns></returns>
        static public List<TimeLine> getTimeFrames(DateTimeOffset refSTart, TimeSpan duration)
        {
            TimeSpan FiveYears = TimeSpan.FromDays(365 * 5);
            TimeSpan ThreeYears = TimeSpan.FromDays(365 * 3);
            TimeSpan OneYears = TimeSpan.FromDays(365 * 1);
            TimeSpan SixMonths = TimeSpan.FromDays(180);
            TimeSpan OneMonth = TimeSpan.FromDays(30);
            TimeSpan OneWeek = TimeSpan.FromDays(7);
            TimeSpan OneDay = TimeSpan.FromDays(1);
            TimeSpan OneHour = TimeSpan.FromHours(1);
            TimeSpan ZeroSpan = new TimeSpan();
            List<TimeLine> retValue = new List<TimeLine>();
            List<TimeSpan> durations = new List<TimeSpan>() { FiveYears, ThreeYears, OneYears, SixMonths, OneMonth, OneWeek, OneDay, OneHour, ZeroSpan };
            if(refSTart == StartOfTime)
            {
                refSTart = DateTimeOffset.UtcNow;
            }
            TimeLine minTImeLine = new TimeLine(refSTart, refSTart.Add(duration));
            //List<TimeSpan> activeDurations = durations.Where(durationObj => durationObj <= duration).OrderBy(obj => obj.Ticks).ToList();

            for(int i =0; i< durations.Count; i++)
            {
                for (int j = 0; j < durations.Count; j++)
                {
                    DateTimeOffset start = refSTart.Add(-durations[i]);
                    DateTimeOffset end = refSTart.Add(durations[j]);
                    TimeLine timeLine = new TimeLine(start, end);

                    TimeLine validFrame = timeLine.InterferringTimeLine(minTImeLine);
                    if (validFrame != null)
                    {
                        if (validFrame.TimelineSpan >= duration)
                        {
                            retValue.Add(validFrame);
                        }
                    }
                }
            }

            return retValue;
        }

        public static CalendarEvent generateCalendarEvent(TimeSpan duration, Repetition repetition, DateTimeOffset Start, DateTimeOffset End, int splitCount = 1, bool rigidFlags = false)
        {
            if (Start == StartOfTime)
            {
                Start = TestUtility.Start;
            }
            if(End == StartOfTime)
            {
                End = Start.Add(duration);
            }
            CalendarEvent RetValue = new CalendarEvent("TestCalendarEvent-" + Guid.NewGuid().ToString(), duration, Start, End, new TimeSpan(), new TimeSpan(), rigidFlags, repetition, splitCount, new Location_Elements(), true, new EventDisplay(), new MiscData(), false);
            return RetValue;
        }

        public static UserAccount getTestUser()
        {
            TilerFront.Models.LoginViewModel myLogin = new TilerFront.Models.LoginViewModel() { Username = TestUtility.UserName, Password = TestUtility.Password, RememberMe = true };

            TilerFront.Models.AuthorizedUser AuthorizeUser = new TilerFront.Models.AuthorizedUser() { UserID = "065febec-d1fe-4c8b-bd32-548613d4479f", UserName = TestUtility.UserName };
            Task<UserAccountDebug> waitForUseraccount = AuthorizeUser.getUserAccountDebug();
            waitForUseraccount.Wait();
            return waitForUseraccount.Result;
        }

        public static bool isTestEquivalent(this TilerEvent firstCalEvent, TilerEvent secondCalEvent)
        {
            bool retValue = true;
            string format = "MM/dd/yyyy HH:mm";
            DateTimeOffset firstStart = DateTimeOffset.Parse( firstCalEvent.Start.ToString(format));
            DateTimeOffset firstEnd = DateTimeOffset.Parse(firstCalEvent.End.ToString(format));
            DateTimeOffset secondStart = DateTimeOffset.Parse(secondCalEvent.Start.ToString(format));
            DateTimeOffset secondEnd = DateTimeOffset.Parse(secondCalEvent.End.ToString(format));
            Type eventType = secondCalEvent.GetType();
            {
                if (firstCalEvent.Id == secondCalEvent.Id)
                {
                    if ((firstStart == secondStart) && (firstEnd == secondEnd))
                    {
                        if (firstCalEvent.ProcrastinationInfo.isTestEquivalent(secondCalEvent.ProcrastinationInfo) 
                            && firstCalEvent.NowInfo.isTestEquivalent(secondCalEvent.NowInfo))
                        {
                            if ((firstCalEvent.isComplete == secondCalEvent.isComplete) && (firstCalEvent.isEnabled == secondCalEvent.isEnabled))
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

