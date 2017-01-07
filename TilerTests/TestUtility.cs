﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;
using My24HourTimerWPF;
using TilerFront;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        static TilerUser testUser;


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

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    using (var stream = client.OpenRead("http://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static List<Location_Elements> getLocations()
        {
            Location_Elements homeLocation = new Location_Elements(41.480352, -81.585446 , "2895 Van aken Blvd cleveland OH 44120", "Home", false, false);
            Location_Elements workLocation = new Location_Elements(41.5002762, -81.6839155, "1228 euclid Ave cleveland OH", "Work", false, false);
            Location_Elements gymLocation = new Location_Elements(41.4987461, -81.6884993, "619 Prospect Avenue Cleveland, OH 44115", "Gym", false, false);
            Location_Elements shakerLibrary = new Location_Elements(41.4658937, -81.5664832, "16500 Van Aken Blvd, Shaker Heights, OH 44120", "Shake Library", false, false);
            Location_Elements churchLocation = new Location_Elements(41.569467, -81.539422, "1465 Dille Rd, Cleveland, OH 44117", "Church", false, false);
            if (CheckForInternetConnection())
            {
                homeLocation.Validate();
                workLocation.Validate();
                gymLocation.Validate();
                shakerLibrary.Validate();
                churchLocation.Validate();
            }
            if (homeLocation.isNull)
            {
                throw new AssertFailedException("failed to Validate homeLocation");
            }

            if (workLocation.isNull)
            {
                throw new AssertFailedException("failed to Validate workLocation");
            }

            if (gymLocation.isNull)
            {
                throw new AssertFailedException("failed to Validate gymLocation");
            }

            if (churchLocation.isNull)
            {
                throw new AssertFailedException("failed to Validate churchLocation");
            }

            if (shakerLibrary.isNull)
            {
                throw new AssertFailedException("failed to Validate shakerLibrary");
            }

            List<Location_Elements> retValue = new List<Location_Elements>() {
                homeLocation, workLocation, gymLocation, shakerLibrary, churchLocation };
            return retValue;
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


        public static CalendarEvent generateCalendarEvent(TimeSpan duration, Repetition repetition, DateTimeOffset Start, DateTimeOffset End, int splitCount = 1, bool rigidFlags = false, Location_Elements location = null, RestrictionProfile restrictionProfile = null)
        {
            if (Start == StartOfTime)
            {
                Start = TestUtility.Start;
            }
            if(End == StartOfTime)
            {
                End = Start.Add(duration);
            }

            if(location == null)
            {
                location = new Location_Elements();
            }

            CalendarEvent RetValue;
            if(restrictionProfile == null)
            {
                EventName name = new EventName("TestCalendarEvent-" + Guid.NewGuid().ToString());
                if(testUser == null)
                {
                    getTestUser(true);
                }
                if (rigidFlags)
                {
                    RetValue = new RigidCalendarEvent(
                        //EventID.GenerateCalendarEvent(), 
                        name, Start, End, duration, new TimeSpan(), new TimeSpan(), repetition, location, new EventDisplay(), new MiscData(), true, false, testUser, new TilerUserGroup(), "UTC", null);
                }
                else
                {
                    RetValue = new CalendarEvent(
                        //EventID.GenerateCalendarEvent(), 
                        name, Start, End, duration, new TimeSpan(), new TimeSpan(), splitCount , repetition, location, new EventDisplay(), new MiscData(), null, new NowProfile(), true, false, testUser, new TilerUserGroup(), "UTC", null);
                }
            }
            else
            {
                EventName name = new EventName("TestCalendarEvent-" + Guid.NewGuid().ToString() + "-Restricted");
                RetValue = new CalendarEventRestricted(name, Start, End, restrictionProfile, duration, repetition, false, true, splitCount, false, location, new TimeSpan(), new TimeSpan(), null, UiSettings: new EventDisplay(), NoteData: new MiscData());

            }

            if (repetition.Enable)
            {
                repetition.PopulateRepetitionParameters(RetValue);
            }
            
            return RetValue;
        }

        public static UserAccount getTestUser(bool forxeUpdateOfTilerUser = false)
        {
            TilerFront.Models.LoginViewModel myLogin = new TilerFront.Models.LoginViewModel() { Username = TestUtility.UserName, Password = TestUtility.Password, RememberMe = true };

            TilerFront.Models.AuthorizedUser AuthorizeUser = new TilerFront.Models.AuthorizedUser() { UserID = "065febec-d1fe-4c8b-bd32-548613d4479f", UserName = TestUtility.UserName };
            Task<UserAccountDebug> waitForUseraccount = AuthorizeUser.getUserAccountDebug();
            waitForUseraccount.Wait();
            if((testUser == null )|| (forxeUpdateOfTilerUser))
            {
                testUser = new TilerTestUser(AuthorizeUser.UserID);
            }
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
                if (firstCalEvent.getId == secondCalEvent.getId)
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

