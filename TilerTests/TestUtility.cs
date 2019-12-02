using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;
using My24HourTimerWPF;
using TilerFront;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using TilerTests.Models;
using System.Globalization;
using System.Data.Entity.Core.Objects;
using TilerCore;
using System.Xml;
using System.IO;

namespace TilerTests
{
    public static class TestUtility
    {
        const int _MonthLimit = 3;
        static readonly DateTimeOffset StartOfTime = new DateTimeOffset();
        static readonly DateTimeOffset _Start = DateTimeOffset.UtcNow.AddMonths(-MonthLimit);
        static readonly Random _Rand = new Random((int)DateTimeOffset.UtcNow.Ticks);
        static readonly string _UserName = "TestUserTiler";
        static readonly string _Email = "TestUserTiler@tiler.com";
        static readonly string _Password = "T35tU53r#";
        static readonly string _lastName = "Last Name TestUserTiler";
        static readonly string _firstName = "First Name TestUserTiler";
        const string testUserId = "065febec-d1fe-4c8b-bd32-548613d4479f";
        static bool isInitialized = false;
        static bool forceNoInternetConnection = true;
        static Dictionary<string, TilerDbContext> UserToContext = new Dictionary<string, TilerDbContext>();

        public static void init()
        {
            if (!isInitialized)
            {
                isInitialized = true;
            }
            initializeLocation();
        }

        public static Packet CreatePacket()
        {
            var tuple = createContextAndUser();
            Packet retValue = new Packet(tuple.Item2, tuple.Item1);
            return retValue;
        }

        public static TilerUser createUser (string userId = "", string userName = "", string password = "", string email = "")
        {
            var tuple = createContextAndUser(userId, userName, password, email);
            return tuple.Item2;
        }

        private static Tuple<DbContext, TilerUser> createContextAndUser(string userId = "", string userName = "", string password = "", string email = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = Guid.NewGuid().ToString();
            }

            if (string.IsNullOrEmpty(userName))
            {
                userName = userId + "userName";
            }

            if (string.IsNullOrEmpty(password))
            {
                password = userId + "password";
            }
            if (string.IsNullOrEmpty(email))
            {
                email = userId + "@testuser.com";
            }

            TilerUser user = new TilerUser()
            {
                Id = userId,
                UserName = userName,
                Email = email,
                PasswordHash = password
            };


            TilerUser user2 = new TilerUser()
            {
                Id = Guid.NewGuid().ToString(),
                UserName = userName + Guid.NewGuid().ToString(),
                Email = email + Guid.NewGuid().ToString(),
                PasswordHash = password + Guid.NewGuid().ToString()
            };
            TestDBContext context = new TestDBContext();
            CalendarEvent calEvent = createProcrastinateCalendarEvent(user);

            context.Users.Add(user);
            context.CalEvents.Add(calEvent);
            context.SaveChanges();
            TilerUser retrivedUser = context.Users.Find(userId);

            Tuple<DbContext, TilerUser> retValue = new Tuple<DbContext, TilerUser>(context, retrivedUser);

            return retValue;
        }

        public static CalendarEvent createProcrastinateCalendarEvent(TilerUser user)
        {
            DateTimeOffset now = Utility.ProcrastinateStartTime;
            CalendarEvent procrastinateCalEvent = ProcrastinateCalendarEvent.generateProcrastinateAll(now, user, TimeSpan.FromSeconds(0), "UTC");
            return procrastinateCalEvent;
        }

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
                //return DateTimeOffset.Parse("6/13/2019 6:53:59 AM +00:00");
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
            if (forceNoInternetConnection)
            {
                return false;
            } else
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
        }

        public static void reloadTilerUser( ref UserAccount userAccount, ref TilerUser tilerUser)
        {
            userAccount = getTestUser(userId: tilerUser.Id);
            tilerUser = userAccount.getTilerUser();
            userAccount.Login().Wait();
        }

        public static Location getLocation(ref UserAccount userAccount, ref TilerUser tilerUser, string locationId)
        {
            userAccount = getTestUser(userId: tilerUser.Id);
            tilerUser = userAccount.getTilerUser();
            userAccount.Login().Wait();
            TilerDbContext context = null;
            if (UserToContext.ContainsKey(tilerUser.Id))
            {
                context = UserToContext[tilerUser.Id];
            }
            if(context== null)
            {
                throw new Exception("Invalid location look up have you created a context, try using testutile.packet to create function");
            }

            return context.Locations.Find(locationId);
        }

        public static void initializeLocation ()
        {
            string apiKey = "AIzaSyDXrtMxPbt6Dqlllpm77AQ47vcCFxZ4oUU";
            Location.updateApiKey(apiKey);
        }

        public static List<Location> getLocations()
        {
            Location homeLocation = new Location(41.480352, -81.585446 , "2895 Van aken Blvd cleveland OH 44120", "Home", false, false);
            Location workLocation = new Location(41.5002762, -81.6839155, "1228 euclid Ave cleveland OH", "Work", false, false);
            Location gymLocation = new Location(41.4987461, -81.6884993, "619 Prospect Avenue Cleveland, OH 44115", "Gym", false, false);
            Location shakerLibrary = new Location(41.4658937, -81.5664832, "16500 Van Aken Blvd, Shaker Heights, OH 44120", "Shaker Library", false, false);
            Location churchLocation = new Location(41.569467, -81.539422, "1465 Dille Rd, Cleveland, OH 44117", "Church", false, false);
            if (CheckForInternetConnection())
            {
                homeLocation.verify();
                workLocation.verify();
                gymLocation.verify();
                shakerLibrary.verify();
                churchLocation.verify();
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

            List<Location> retValue = new List<Location>() {
                homeLocation, workLocation, gymLocation, shakerLibrary, churchLocation };
            return retValue;
        }

        /// <summary>
        /// Function generates ranging from possibly 10 year, two hour time frame that should surround the time line 
        /// created from refStart Plus the duration
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
                            retValue.Add(timeLine);
                        }
                    }
                }
            }

            return retValue;
        }


        public static CalendarEvent generateCalendarEvent(TilerUser testUser, TimeSpan duration, Repetition repetition, DateTimeOffset Start, DateTimeOffset End, int splitCount = 1, bool rigidFlags = false, Location location = null, RestrictionProfile restrictionProfile = null, MiscData note = null, ReferenceNow now = null, EventDisplay eventDisplay = null)
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
                location = new Location();
            }

            if(repetition == null)
            {
                repetition = new Repetition();
            }

            note = note ?? new MiscData();
            CalendarEvent RetValue;
            eventDisplay = eventDisplay ?? new EventDisplay();
            if(restrictionProfile == null)
            {
                EventName name = new EventName(null, null, "TestCalendarEvent-" + Guid.NewGuid().ToString());
                if(testUser == null)
                {
                    getTestUser(true);
                }
                if (rigidFlags)
                {
                    RetValue = new RigidCalendarEvent(
                        name, Start, End, duration, new TimeSpan(), new TimeSpan(), repetition, location, eventDisplay, note, true, false, testUser, new TilerUserGroup(), "UTC", null);
                }
                else
                {
                    RetValue = new CalendarEvent(
                        name, Start, End, duration, new TimeSpan(), new TimeSpan(), splitCount , repetition, location, eventDisplay, note, null, new NowProfile(), true, false, testUser, new TilerUserGroup(), "UTC", null);
                }
                name.Creator_EventDB = RetValue.getCreator;
                name.AssociatedEvent = RetValue;
            }
            else
            {
                if (now == null)
                {
                    throw new ArgumentNullException("now", "You need to add a referencenow object for creation of calendareventrestricted object");
                }
                EventName name = new EventName(null, null, "TestCalendarEvent-" + Guid.NewGuid().ToString() + "-Restricted");
                RetValue = new CalendarEventRestricted(testUser, new TilerUserGroup(), name, Start, End, restrictionProfile, duration, repetition, false, true, splitCount, false, new NowProfile(), location, new TimeSpan(), new TimeSpan(), null, now, new Procrastination(Utility.JSStartTime, new TimeSpan()), UiSettings: new EventDisplay(), NoteData: note);
                name.Creator_EventDB = RetValue.getCreator;
                name.AssociatedEvent = RetValue;
            }

            if (repetition.EnableRepeat)
            {
                if (RetValue.getIsEventRestricted) {
                    repetition.PopulateRepetitionParameters(RetValue as CalendarEventRestricted);
                } else
                {
                    repetition.PopulateRepetitionParameters(RetValue);
                }
                    
            }
            return RetValue;
        }

        public static UserAccount getTestUser(bool reloadTilerContext = true, string userId = testUserId, bool copyTestFolder = true) {
            if (!isInitialized) {
                init();
            }


            TilerDbContext _Context;
            if (UserToContext.ContainsKey(userId))
            {
                _Context = UserToContext[userId];
                if (reloadTilerContext)
                {
                    RefreshAll(_Context);
                }
                _Context = new TestDBContext();
                //_Context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
                UserToContext[userId] = _Context;
            }
            else
            {
                _Context = new TestDBContext();
                //_Context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
                UserToContext.Add(userId, _Context);
            }

            

            TilerUser tilerUser = _Context.Users.Find(userId);
            UserAccount userAccount = new UserAccountTest(tilerUser, _Context);
            return userAccount;
        }


        public static Tuple<Schedule, ScheduleDump> getSchedule(string userId, DateTimeOffset refNow, bool copyTestFolder = true, string filePath = "", string connectionName = "")
        {
            string currDir = filePath;
            if (String.IsNullOrEmpty(filePath) || String.IsNullOrWhiteSpace(filePath))
            {
                currDir = Directory.GetCurrentDirectory() + "\\" + "WagTapCalLogs\\";
            }


            string sourceFile = currDir + userId + "\\" + userId + ".xml";
            if (userId != testUserId && copyTestFolder)
            {
                string destinationFile = currDir + userId + ".xml";
                System.IO.File.Copy(sourceFile, destinationFile, true);
            }


            ScheduleDump dump = new ScheduleDump();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(sourceFile);
            dump.XmlDoc = xmlDoc;
            string notes = dump.XmlDoc.DocumentElement.SelectSingleNode("/ScheduleLog/scheduleNotes")?.InnerText;
            dump.Notes = notes;


            TilerUser User = new TilerUser() { UserName = userId + "@tiler-test.com", Id = userId };
            UserAccountDump accDebug = new UserAccountDump(User, connectionName);
            Schedule schedule;
            if (refNow.isBeginningOfTime())
            {
                schedule = new TestSchedule(dump, accDebug);
            }
            else
            {
                schedule = new TestSchedule(dump, accDebug, referenceNow: refNow);
            }

            
            Tuple<Schedule, ScheduleDump> retValue = new Tuple<Schedule, ScheduleDump>(schedule, dump);
            return retValue;
        }

        public static Tuple<Schedule, ScheduleDump> getSchedule(string userId, bool copyTestFolder = true, string filePath = "", string connectionName = "")
        {
            return getSchedule(userId, Utility.BeginningOfTime, copyTestFolder, filePath, connectionName);
        }

        public static void RefreshAll(TilerDbContext context)
        {
            context.Dispose();
        }

        public static CalendarEvent getCalendarEventById(EventID calEventId, UserAccount user)
        {
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(calEventId.ToString());
            waitVar.Wait();
            CalendarEvent retValue = waitVar.Result;
            return retValue;
        }

        public static CalendarEvent getCalendarEventById (string calEventId, UserAccount user)
        {
            Task<CalendarEvent> waitVar = user.ScheduleLogControl.getCalendarEventWithID(calEventId);
            waitVar.Wait();
            CalendarEvent retValue = waitVar.Result;
            return retValue;
        }

        public static SubCalendarEvent getSubEventById(string subEventId, UserAccount user)
        {
            Task<SubCalendarEvent> waitVar = user.ScheduleLogControl.getSubEventWithID(subEventId);
            waitVar.Wait();
            SubCalendarEvent retValue = waitVar.Result;
            return retValue;
        }


        public static void isSubCalendarEventUIEquivalenToScheduleLoaded(UserAccount useraccount, ReferenceNow now, TimeLine timeLine=null)
        {
            LogControl LogAccess = useraccount.ScheduleLogControl;
            TimeLine rangeOfLookup = timeLine ?? new TimeLine(now.constNow.AddDays(Utility.defaultBeginDay), now.constNow.AddDays(Utility.defaultEndDay));
            var task = LogAccess.getAllEnabledSubCalendarEvent(rangeOfLookup, now, retrievalOption: DataRetrivalOption.Ui);
            task.Wait();
            var allSubs = task.Result.ToList();

            var taskCal = LogAccess.getAllEnabledCalendarEvent(rangeOfLookup, now, retrievalOption: DataRetrivalOption.Ui);
            taskCal.Wait();
            var allCals = taskCal.Result.ToList();
            var calSubEVents = allCals.Select(obj => obj.Value).SelectMany(obj => obj.ActiveSubEvents).ToList();

            Schedule schedule = new TestSchedule(useraccount, now.constNow);
            IEnumerable<CalendarEvent> allCalendarEvents = schedule.getOnlyTilerCalendarEvents();
            var schedSubEvents = allCalendarEvents.SelectMany(calEvent => calEvent.ActiveSubEvents);
            Assert.AreEqual(allSubs.Count, calSubEVents.Count);
            Assert.AreEqual(allSubs.Count, schedSubEvents.Count());
        }

        internal static List<CalendarEvent> generateAllCalendarEventVaraints(TestSchedule schedule, TimeSpan duration, DateTimeOffset start, TilerUser testUser, UserAccount userAccount, Location location = null)
        {
            List<CalendarEvent> oneSubEvents = generateAllCalendarEvent(schedule, duration, start, testUser, userAccount, 1, location);
            List<CalendarEvent> twoSubEvents = generateAllCalendarEvent(schedule, duration, start, testUser, userAccount, 2, location);
            List<CalendarEvent> tenSubEvents = generateAllCalendarEvent(schedule, duration, start, testUser, userAccount, 10, location);
            List<CalendarEvent> retValue = oneSubEvents.Concat(twoSubEvents).Concat(tenSubEvents).ToList();
            return retValue;
        }

        internal static List<CalendarEvent> generateAllCalendarEvent(TestSchedule schedule, TimeSpan duration, DateTimeOffset start, TilerUser testUser, UserAccount userAccount, int split, Location location = null, bool enableScheduleBigdata = false)
        {
            if(location == null)
            {
                List<Location> locaitions = TestUtility.getLocations();
                location = Location.getDefaultLocation();
            }
            DateTimeOffset nowTime = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();

            if (start.isBeginningOfTime())
            {
                start = nowTime;
            }
            
            ReferenceNow now = new ReferenceNow(DateTimeOffset.UtcNow.removeSecondsAndMilliseconds(), nowTime, new TimeSpan());

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            DateTimeOffset end = start.Add(duration).Add(duration).Add(duration);
            CalendarEvent simpleCalEvent = generateCalendarEvent(testUser, duration, new Repetition(), start, end, split, false, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(simpleCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine dailyTimeLine = new TimeLine(start, start.AddDays(7));
            TimeLine dailyActiveTimeLine = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            Repetition dailyRepetion = new Repetition(dailyTimeLine, Repetition.Frequency.DAILY, dailyActiveTimeLine);
            CalendarEvent dailyCalEvent = generateCalendarEvent(testUser, duration, dailyRepetion, start, end, split, false, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(dailyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine weeklyTimeLine = new TimeLine(start, start.AddDays(28));
            TimeLine weeklyActiveTimeLine = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            Repetition weeklyRepetion = new Repetition(dailyTimeLine, Repetition.Frequency.WEEKLY, weeklyActiveTimeLine);
            CalendarEvent weeklyCalEvent = generateCalendarEvent(testUser, duration, weeklyRepetion, start, end, split, false, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(weeklyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine weekdayWeeklyTimeLine = new TimeLine(start, start.AddDays(28));
            TimeLine weekdayWeeklyActiveTimeLine = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            List<DayOfWeek> weekDays = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Sunday, DayOfWeek.Wednesday, DayOfWeek.Saturday };
            Repetition weekdayWeeklyRepetion = new Repetition(dailyTimeLine, Repetition.Frequency.WEEKLY, weeklyActiveTimeLine, weekDays.ToArray());
            CalendarEvent weekdayWeeklyCalEvent = generateCalendarEvent(testUser, duration, weekdayWeeklyRepetion, start, end, split, false, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(weekdayWeeklyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine monthlyTimeLine = new TimeLine(start, start.AddDays(180));
            TimeLine monthlyActiveTimeLine = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            Repetition monthlyRepetion = new Repetition(monthlyTimeLine, Repetition.Frequency.MONTHLY, monthlyActiveTimeLine);
            CalendarEvent monthlyCalEvent = generateCalendarEvent(testUser, duration, monthlyRepetion, start, end, split, false, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(monthlyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine monthlyWeekdayTimeLine = new TimeLine(start, start.AddDays(180));
            TimeLine monthlyWeekdayActiveTimeLine = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            List<DayOfWeek> monthlyWeekDays = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Sunday, DayOfWeek.Wednesday, DayOfWeek.Saturday };
            Repetition monthlyWeekdayRepetion = new Repetition(monthlyWeekdayTimeLine, Repetition.Frequency.MONTHLY, monthlyWeekdayActiveTimeLine, monthlyWeekDays.ToArray());
            CalendarEvent weekDayMonthlyCalEvent = generateCalendarEvent(testUser, duration, monthlyWeekdayRepetion, start, end, split, false, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(weekDayMonthlyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine yearlyTimeLine = new TimeLine(start, start.AddYears(2));
            TimeLine yearlyActiveTimeLine = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            Repetition yearlyWeekdayRepetion = new Repetition(monthlyWeekdayTimeLine, Repetition.Frequency.YEARLY, yearlyActiveTimeLine);
            CalendarEvent yearlyCalEvent = generateCalendarEvent(testUser, duration, yearlyWeekdayRepetion, start, end, split, false, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(yearlyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine weekdayYearlyTimeLine = new TimeLine(start, start.AddYears(2));
            TimeLine weekdayYearlyActiveTimeLine = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            List<DayOfWeek> yearlyWeekDays = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Sunday, DayOfWeek.Wednesday, DayOfWeek.Saturday };
            Repetition weekdayYearlyWeekdayRepetion = new Repetition(weekdayYearlyTimeLine, Repetition.Frequency.YEARLY, weekdayYearlyActiveTimeLine, yearlyWeekDays.ToArray());
            CalendarEvent weekDayMYearlyCalEvent = generateCalendarEvent(testUser, duration, weekdayYearlyWeekdayRepetion, start, end, split, false, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(weekDayMYearlyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            DateTimeOffset restrictionStart = start.Add(TimeSpan.FromMinutes(((long)duration.Minutes / 2)));
            TimeSpan restrictedDurationSpan = (duration) + (duration);
            RestrictionProfile restrictionProfile = new RestrictionProfile(restrictionStart, restrictedDurationSpan);

            CalendarEvent restrictedCalEvent = generateCalendarEvent(testUser, duration, new Repetition(), start, end, split, false, Location.getDefaultLocation(), restrictionProfile.createCopy(), now: now);
            schedule.AddToScheduleAndCommitAsync(restrictedCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine dailyTimeLineRestriction = new TimeLine(start, start.AddDays(7));
            TimeLine dailyActiveTimeLineRestriction = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            Repetition dailyRepetionRestriction = new Repetition(dailyTimeLine, Repetition.Frequency.DAILY, dailyActiveTimeLine);
            CalendarEvent restrictedDailyCalEvent = generateCalendarEvent(testUser, duration, dailyRepetionRestriction, start, end, split, false, Location.getDefaultLocation(), restrictionProfile.createCopy(), now: now);
            schedule.AddToScheduleAndCommitAsync(restrictedDailyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine weeklyTimeLineRestriction = new TimeLine(start, start.AddDays(28));
            TimeLine weeklyActiveTimeLineRestriction = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            Repetition weeklyRepetionRestriction = new Repetition(weeklyTimeLineRestriction, Repetition.Frequency.WEEKLY, weeklyActiveTimeLineRestriction);
            CalendarEvent restrictedWeeklyCalEvent = generateCalendarEvent(testUser, duration, weeklyRepetionRestriction, start, end, split, false, Location.getDefaultLocation(), restrictionProfile.createCopy(), now: now);
            schedule.AddToScheduleAndCommitAsync(restrictedWeeklyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine weekdayWeeklyTimeLineRestriction = new TimeLine(start, start.AddDays(28));
            TimeLine weekdayWeeklyActiveTimeLineRestriction = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            List<DayOfWeek> weekDaysRestriction = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Sunday, DayOfWeek.Wednesday, DayOfWeek.Saturday };
            Repetition weekdayWeeklyRepetionRestriction = new Repetition(weekdayWeeklyTimeLineRestriction, Repetition.Frequency.WEEKLY, weekdayWeeklyActiveTimeLineRestriction, weekDays.ToArray());
            CalendarEvent restrictedWeekdayWeeklyCalEvent = generateCalendarEvent(testUser, duration, weekdayWeeklyRepetionRestriction, start, end, split, false, Location.getDefaultLocation(), restrictionProfile.createCopy(), now: now);
            schedule.AddToScheduleAndCommitAsync(restrictedWeekdayWeeklyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine monthlyTimeLineRestriction = new TimeLine(start, start.AddDays(180));
            TimeLine monthlyActiveTimeLineRestriction = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            Repetition monthlyRepetionRestriction = new Repetition(monthlyTimeLineRestriction, Repetition.Frequency.MONTHLY, monthlyActiveTimeLineRestriction);
            CalendarEvent restrictedMonthlyCalEvent = generateCalendarEvent(testUser, duration, monthlyRepetionRestriction, start, end, split, false, Location.getDefaultLocation(), restrictionProfile.createCopy(), now: now);
            schedule.AddToScheduleAndCommitAsync(restrictedMonthlyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine monthlyWeekdayTimeLineRestriction = new TimeLine(start, start.AddDays(180));
            TimeLine monthlyWeekdayActiveTimeLineRestriction = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            List<DayOfWeek> monthlyWeekDaysRestriction = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Sunday, DayOfWeek.Wednesday, DayOfWeek.Saturday };
            Repetition monthlyWeekdayRepetionRestriction = new Repetition(monthlyWeekdayTimeLineRestriction, Repetition.Frequency.MONTHLY, monthlyWeekdayActiveTimeLineRestriction, monthlyWeekDays.ToArray());
            CalendarEvent restrictedWeekdayMonthlyCalEvent = generateCalendarEvent(testUser, duration, monthlyWeekdayRepetionRestriction, start, end, split, false, Location.getDefaultLocation(), restrictionProfile.createCopy(), now: now);
            schedule.AddToScheduleAndCommitAsync(restrictedWeekdayMonthlyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine yearlyTimeLineRestriction = new TimeLine(start, start.AddYears(2));
            TimeLine yearlyActiveTimeLineRestriction = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            Repetition yearlyRepetionRestriction = new Repetition(yearlyTimeLineRestriction, Repetition.Frequency.YEARLY, yearlyActiveTimeLineRestriction);
            CalendarEvent restrictedYearlyCalEvent = generateCalendarEvent(testUser, duration, yearlyRepetionRestriction, start, end, split, false, Location.getDefaultLocation(), restrictionProfile.createCopy(), now: now);
            schedule.AddToScheduleAndCommitAsync(restrictedYearlyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine weekdayYearlyTimeLineRestriction = new TimeLine(start, start.AddYears(2));
            TimeLine weekdayYearlyActiveTimeLineRestriction = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            List<DayOfWeek> yearlyWeekDaysRestriction = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Sunday, DayOfWeek.Wednesday, DayOfWeek.Saturday };
            Repetition weekdayYearlyWeekdayRepetionRestriction = new Repetition(weekdayYearlyTimeLineRestriction, Repetition.Frequency.YEARLY, weekdayYearlyActiveTimeLineRestriction, yearlyWeekDays.ToArray());
            CalendarEvent restrictedWeekdayYearlyCalEvent = generateCalendarEvent(testUser, duration, weekdayYearlyWeekdayRepetionRestriction, start, end, split, false, Location.getDefaultLocation(), restrictionProfile.createCopy(), now: now);
            schedule.AddToScheduleAndCommitAsync(restrictedWeekdayYearlyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            CalendarEvent rigidCalEvent = generateCalendarEvent(testUser, duration, new Repetition(), start, end, 1, true, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(rigidCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine rigidDailyTimeLine = new TimeLine(start, start.AddDays(7));
            TimeLine rigidDailyActiveTimeLine = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            Repetition rigidDailyRepetion = new Repetition(rigidDailyTimeLine, Repetition.Frequency.DAILY, rigidDailyActiveTimeLine);
            CalendarEvent rigidDailyCalEvent = generateCalendarEvent(testUser, duration, rigidDailyRepetion, start, end, 1, true, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(rigidDailyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine rigidWeeklyTimeLine = new TimeLine(start, start.AddDays(28));
            TimeLine rigidWeeklyActiveTimeLine = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            Repetition rigidWeeklyRepetion = new Repetition(rigidWeeklyTimeLine, Repetition.Frequency.WEEKLY, rigidWeeklyActiveTimeLine);
            CalendarEvent rigidWeeklyCalEvent = generateCalendarEvent(testUser, duration, rigidWeeklyRepetion, start, end, 1, true, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(rigidWeeklyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine rigidWeekdayWeeklyTimeLine = new TimeLine(start, start.AddDays(28));
            TimeLine rigidWeekdayWeeklyActiveTimeLine = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            List<DayOfWeek> rigidWeekDays = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Sunday, DayOfWeek.Wednesday, DayOfWeek.Saturday };
            Repetition rigidWeekdayWeeklyRepetion = new Repetition(rigidWeekdayWeeklyTimeLine, Repetition.Frequency.WEEKLY, rigidWeekdayWeeklyActiveTimeLine, rigidWeekDays.ToArray());
            CalendarEvent rigidWeekdayWeeklyCalEvent = generateCalendarEvent(testUser, duration, rigidWeekdayWeeklyRepetion, start, end, 1, true, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(rigidWeekdayWeeklyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine rigidMonthlyTimeLine = new TimeLine(start, start.AddDays(180));
            TimeLine rigidMonthlyActiveTimeLine = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            Repetition rigidMonthlyRepetion = new Repetition(rigidMonthlyTimeLine, Repetition.Frequency.MONTHLY, rigidMonthlyActiveTimeLine);
            CalendarEvent rigidMonthlyCalEvent = generateCalendarEvent(testUser, duration, rigidMonthlyRepetion, start, end, 1, true, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(rigidMonthlyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine rigidMonthlyWeekdayTimeLine = new TimeLine(start, start.AddDays(180));
            TimeLine rigidMonthlyWeekdayActiveTimeLine = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            List<DayOfWeek> rigidMonthlyWeekDays = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Sunday, DayOfWeek.Wednesday, DayOfWeek.Saturday };
            Repetition rigidMonthlyWeekdayRepetion = new Repetition(monthlyWeekdayTimeLine, Repetition.Frequency.MONTHLY, monthlyWeekdayActiveTimeLine, rigidMonthlyWeekDays.ToArray());
            CalendarEvent rigidWeekdayMonthlyCalEvent = generateCalendarEvent(testUser, duration, rigidMonthlyWeekdayRepetion, start, end, 1, true, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(rigidWeekdayMonthlyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine rigidYearlyTimeLine = new TimeLine(start, start.AddYears(2));
            TimeLine rigidYearlyActiveTimeLine = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            Repetition rigidYearlyWeekdayRepetion = new Repetition(rigidYearlyTimeLine, Repetition.Frequency.YEARLY, rigidYearlyActiveTimeLine);
            CalendarEvent rigidYearlyCalEvent = generateCalendarEvent(testUser, duration, rigidYearlyWeekdayRepetion, start, end, 1, true, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(rigidYearlyCalEvent).Wait();

            TestUtility.reloadTilerUser(ref userAccount, ref testUser); if (enableScheduleBigdata) { userAccount.ScheduleLogControl.enableUpdateBigData(); } else { userAccount.ScheduleLogControl.disableUpdateBigData(); }
            schedule = new TestSchedule(userAccount, nowTime);
            TimeLine rigidWeekdayYearlyTimeLine = new TimeLine(start, start.AddYears(2));
            TimeLine rigidWeekdayYearlyActiveTimeLine = new TimeLine(start.Add(duration), start.Add(duration).Add(duration));
            List<DayOfWeek> rigidWearlyWeekDays = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Sunday, DayOfWeek.Wednesday, DayOfWeek.Saturday };
            Repetition rigidWeekdayYearlyWeekdayRepetion = new Repetition(rigidWeekdayYearlyTimeLine, Repetition.Frequency.YEARLY, rigidWeekdayYearlyActiveTimeLine, rigidWearlyWeekDays.ToArray());
            CalendarEvent rigidWeekdayYearlyCalEvent = generateCalendarEvent(testUser, duration, rigidWeekdayYearlyWeekdayRepetion, start, end, 1, true, Location.getDefaultLocation());
            schedule.AddToScheduleAndCommitAsync(rigidWeekdayYearlyCalEvent).Wait();


            List<CalendarEvent> retValue = new List<CalendarEvent>()
            {
                simpleCalEvent, dailyCalEvent, weeklyCalEvent, weekdayWeeklyCalEvent,monthlyCalEvent, weekDayMonthlyCalEvent, yearlyCalEvent, weekDayMYearlyCalEvent,
                restrictedCalEvent, restrictedDailyCalEvent, restrictedWeeklyCalEvent, restrictedWeekdayWeeklyCalEvent, restrictedMonthlyCalEvent, restrictedWeekdayMonthlyCalEvent,restrictedYearlyCalEvent, restrictedWeekdayYearlyCalEvent,
                rigidCalEvent, rigidDailyCalEvent, rigidWeeklyCalEvent, rigidWeekdayWeeklyCalEvent, rigidMonthlyCalEvent, rigidWeekdayMonthlyCalEvent, rigidYearlyCalEvent, rigidWeekdayYearlyCalEvent
            };

            //IEnumerable<CalendarEvent> allCalEvents = retValue;
            //allCalEvents = allCalEvents.Except(new List<CalendarEvent>() { simpleCalEvent });
            //foreach (CalendarEvent calEvent in  retValue)
            //{
            //    if(calEvent.IsRepeat)
            //    {
            //        allCalEvents = allCalEvents.Concat(calEvent.Repeat.RecurringCalendarEvents());
            //    }
            //}
            
            //foreach (CalendarEvent calEvent in allCalEvents)
            //{
            //    calEvent.CreatorId = testUser.Id;
            //    calEvent.Creator_EventDB = null;
            //    calEvent.LocationId = location.Id;
            //    //calEvent.Location = null;
                
            //    foreach (TilerEvent tilerEvent in calEvent.AllSubEvents)
            //    {
            //        tilerEvent.CreatorId = testUser.Id;
            //        tilerEvent.Creator_EventDB = null;
            //        tilerEvent.LocationId = location.Id;
            //        //tilerEvent.Location = null;
            //    }


            //}

            return retValue;
        }

        static public void cleanupDB ()
        {
            //_Context.Database.ExecuteSqlCommand("TRUNCATE TABLE Undoes");
            //_Context.Database.ExecuteSqlCommand("TRUNCATE TABLE TilerEvents");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM Procrastinations");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  EventNames");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  Reasons");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  Locations");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  Repetitions");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  MiscDatas");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  TilerUserGroups");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  EventDisplays");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  TilerColors");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  AspNetUsers");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  RestrictionDays");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  RestrictionTimeLines");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  RestrictionProfiles");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  Classifications");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  NowProfiles");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  EventTimeLines");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  AspNetRoles");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  AspNetUserRoles");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  AspNetUserLogins");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  AspNetUserClaims");

            //isInitialized = false;
        }
        public static bool isTestEquivalent(this TilerEvent firstTilerEvent, TilerEvent secondTilerEvent)
        {
            bool retValue = true;
            string format = "MM/dd/yyyy HH:mm";
            DateTimeOffset firstStart = TestUtility.parseAsUTC( firstTilerEvent.Start.ToString(format));
            DateTimeOffset firstEnd = TestUtility.parseAsUTC(firstTilerEvent.End.ToString(format));
            DateTimeOffset secondStart = TestUtility.parseAsUTC(secondTilerEvent.Start.ToString(format));
            DateTimeOffset secondEnd = TestUtility.parseAsUTC(secondTilerEvent.End.ToString(format));
            Type eventType = secondTilerEvent.GetType();
            {
                if (firstTilerEvent.getId == secondTilerEvent.getId)
                {
                    if ((firstStart == secondStart) && (firstEnd == secondEnd))
                    {
                        if (firstTilerEvent.getProcrastinationInfo.isTestEquivalent(secondTilerEvent.getProcrastinationInfo) 
                            && firstTilerEvent.getNowInfo.isTestEquivalent(secondTilerEvent.getNowInfo)
                            && firstTilerEvent.Location.isTestEquivalent(secondTilerEvent.Location))
                        {
                            if(
                                (firstTilerEvent.AutoDeleted_EventDB == secondTilerEvent.AutoDeleted_EventDB)
                                && (firstTilerEvent.AutoDeletion_Reason == secondTilerEvent.AutoDeletion_Reason)
                                )
                            {
                                if ((firstTilerEvent.getIsComplete == secondTilerEvent.getIsComplete) && (firstTilerEvent.isEnabled == secondTilerEvent.isEnabled))
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
                else
                {
                    retValue = false;
                }
            }
            Assert.IsTrue(retValue);
            return retValue;
        }

        public static bool isTestEquivalent(this SubCalendarEvent firstSubevent, SubCalendarEvent secondSubevent)
        {
            bool retValue = (firstSubevent as TilerEvent).isTestEquivalent(secondSubevent as TilerEvent);
            Assert.IsTrue(retValue);
            retValue &= firstSubevent.isRepetitionLocked == secondSubevent.isRepetitionLocked;
            Assert.IsTrue(retValue);
            return retValue;
        }

        public static bool isTestEquivalent(this ReferenceNow firstReferenceNow, ReferenceNow secondReferenceNow)
        {
            bool retValue = true;
            retValue &= firstReferenceNow.constNow == secondReferenceNow.constNow;
            retValue &= firstReferenceNow.consttDayIndex == secondReferenceNow.consttDayIndex;
            Assert.IsTrue(retValue);
            return retValue;
        }

        public static bool isTestEquivalent(this Schedule FirstSchedule, Schedule SecondSchedule)
        {
            bool retValue = false;
            var firstCalendarEvents = FirstSchedule.getAllCalendarEvents();
            var secondCalendarEvents = SecondSchedule.getAllCalendarEvents();
            retValue = firstCalendarEvents.Count() == secondCalendarEvents.Count();
            if(retValue)
            {
                foreach(CalendarEvent firstCalEvent in firstCalendarEvents)
                {
                    var secondCalEvent = SecondSchedule.getCalendarEvent(firstCalEvent.Id);
                    retValue = secondCalEvent != null;
                    if(retValue)
                    {
                        retValue = firstCalEvent.isTestEquivalent(secondCalEvent);
                        Assert.IsTrue(retValue);
                    } else
                    {
                        break;
                    }
                }
            }


            var firstLocations = FirstSchedule.getAllLocations().Where(obj => !obj.isNull && !obj.isDefault);
            firstLocations = firstLocations.Concat(firstLocations.Where(location => location.LocationValidation != null).SelectMany(location => location.LocationValidation.locations));
            var secondLocations = SecondSchedule.getAllLocations().Where(obj => !obj.isNull && !obj.isDefault);
            secondLocations = secondLocations.Concat(secondLocations.Where(location => location.LocationValidation != null).SelectMany(location => location.LocationValidation.locations));

            Dictionary<string, Location> firstLocationDictionary = new Dictionary<string, Location>();
            foreach(Location location in firstLocations)
            {
                if(!firstLocationDictionary.ContainsKey(location.Id))
                {
                    firstLocationDictionary.Add(location.Id, location);
                }
            }
            Dictionary<string, Location> secondLocationDictionary = new Dictionary<string, Location>();
            foreach (Location location in secondLocations)
            {
                if (!secondLocationDictionary.ContainsKey(location.Id))
                {
                    secondLocationDictionary.Add(location.Id, location);
                }
            }
            firstLocations = firstLocationDictionary.Values;
            secondLocations = secondLocationDictionary.Values;
            retValue &= firstLocations.Count() == secondLocations.Count();
            if (retValue)
            {
                foreach (Location firsLocation in firstLocations)
                {
                    var secondLocation = SecondSchedule.getLocation(firsLocation.Description);
                    retValue = secondLocation != null;
                    if (retValue)
                    {
                        retValue = secondLocation.isTestEquivalent(firsLocation);
                        Assert.IsTrue(retValue);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            retValue &= FirstSchedule.Now.isTestEquivalent(SecondSchedule.Now);
            Assert.IsTrue(retValue);
            return retValue;
        }

        public static bool isTestEquivalent(this CalendarEvent firstCalEvent, CalendarEvent secondCalEvent)
        {
            bool retValue = true;
            if (firstCalEvent.AllSubEvents.Count() == secondCalEvent.AllSubEvents.Count() &&  firstCalEvent.NumberOfSplit == secondCalEvent.NumberOfSplit)
            {
                Dictionary<string, SubCalendarEvent> firstdictionary = firstCalEvent.AllSubEvents.ToDictionary(subEvent => subEvent.Id, subEvent => subEvent);
                Dictionary<string, SubCalendarEvent> seconddictionary = secondCalEvent.AllSubEvents.ToDictionary(subEvent => subEvent.Id, subEvent => subEvent);
                foreach (SubCalendarEvent subEvent in firstdictionary.Values)
                {
                    if (seconddictionary.ContainsKey(subEvent.Id))
                    {
                        var secondSubEvent = seconddictionary[subEvent.Id];
                        retValue = subEvent.isTestEquivalent(secondSubEvent)
                            && subEvent.Location.isTestEquivalent(secondSubEvent.Location)
                            && subEvent.getUIParam.isTestEquivalent(secondSubEvent.getUIParam);
                        if(subEvent.getIsEventRestricted)
                        {
                            retValue = retValue && (subEvent as SubCalendarEventRestricted).getRestrictionProfile().isTestEquivalent((secondSubEvent as SubCalendarEventRestricted).getRestrictionProfile());
                        }

                        if (!retValue)
                        {
                            break; 
                        }
                    } else
                    {
                        retValue = false;
                        break;
                    }
                }
            }
            else
            {
                retValue = false;
            }

            retValue &= isTestEquivalent(firstCalEvent as TilerEvent, secondCalEvent as TilerEvent);
            Assert.IsTrue(retValue);
            retValue &= (firstCalEvent.IsRecurring == secondCalEvent.IsRecurring ?
                    (firstCalEvent.IsRecurring ? isTestEquivalent(firstCalEvent.Repeat, secondCalEvent.Repeat) : true) : //if repeat is enabled the run equivalecy test else then passing test
                    false); // if calendar repeat flags aren't the same then the calEvents are not equal
            Assert.IsTrue(retValue);
            retValue &= (firstCalEvent.getIsEventRestricted == secondCalEvent.getIsEventRestricted ?
                    (firstCalEvent.getIsEventRestricted ? isTestEquivalent((firstCalEvent as CalendarEventRestricted).RestrictionProfile, (secondCalEvent as CalendarEventRestricted).RestrictionProfile) : true) : //if restriction profile is enabled the run equivalecy test else then passing test
                    false);
            Assert.IsTrue(retValue);
            retValue &= (firstCalEvent.getUIParam.isTestEquivalent(secondCalEvent.getUIParam));
            Assert.IsTrue(retValue);
            retValue &= (firstCalEvent.DayPreference.isTestEquivalent(secondCalEvent.DayPreference));
            Assert.IsTrue(retValue);


            Assert.IsTrue(retValue);
            return retValue;
        }

        public static bool isTestEquivalent(this Repetition firstRepetition, Repetition secondRepetition)
        {
            bool retValue = true;
            if(firstRepetition!=null && secondRepetition!=null)
            {
                if(firstRepetition.EnableRepeat == secondRepetition.EnableRepeat)
                {
                    if (firstRepetition.getFrequency == secondRepetition.getFrequency)
                    {
                        Dictionary<string, CalendarEvent> firstdictionary = firstRepetition.RepeatingEvents.ToDictionary(calEvent => calEvent.Id, calEvent => calEvent);
                        Dictionary<string, CalendarEvent> seconddictionary = secondRepetition.RepeatingEvents.ToDictionary(calEvent => calEvent.Id, calEvent => calEvent);
                        foreach (CalendarEvent calEvent in firstdictionary.Values)
                        {
                            if (seconddictionary.ContainsKey(calEvent.Id))
                            {
                                var secondCalEvent = seconddictionary[calEvent.Id];
                                retValue = calEvent.isTestEquivalent(secondCalEvent);
                                if (!retValue)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                retValue = false;
                                break;
                            }
                        }

                        if(retValue) {
                            Dictionary<string, Repetition> firstRepetitionDict = firstRepetition.SubRepetitions.ToDictionary(repetition => repetition.Id, repetition => repetition);
                            Dictionary<string, Repetition> secondRepetitionDict = secondRepetition.SubRepetitions.ToDictionary(repetition => repetition.Id, repetition => repetition);
                            foreach (Repetition repetition in firstRepetitionDict.Values)
                            {
                                if (secondRepetitionDict.ContainsKey(repetition.Id))
                                {
                                    var secondRepetitions = secondRepetitionDict[repetition.Id];
                                    retValue = repetition.isTestEquivalent(secondRepetitions);
                                    if (!retValue)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    retValue = false;
                                    break;
                                }
                            }
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
                retValue = firstRepetition == secondRepetition;// this will only be true when both are null
            }
            Assert.IsTrue(retValue);
            return retValue;
        }

        public static bool isTestEquivalent(this Procrastination firstProcrastination, Procrastination secondProcrastination)
        {
            bool retValue = true;
            {
                if (firstProcrastination!=null && secondProcrastination !=null)
                {
                    if ((firstProcrastination.DislikedDayOfWeek == secondProcrastination.DislikedDayOfWeek)
                    && (firstProcrastination.DislikedDaySection == secondProcrastination.DislikedDaySection)
                    && (firstProcrastination.DislikedStartTime == secondProcrastination.DislikedStartTime)
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
                    retValue = firstProcrastination == secondProcrastination;
                }
            }
            Assert.IsTrue(retValue);
            return retValue;
        }

        public static bool isTestEquivalent(this NowProfile firstNow, NowProfile secondNow)
        {
            bool retValue = true;
            {
                if(firstNow != null && secondNow != null)
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
                } else
                {
                    retValue = firstNow == secondNow;
                }
                
            }
            Assert.IsTrue(retValue);
            return retValue;
        }

        public static bool isTestEquivalent(this Location locationA, Location locationB)
        {
            bool retValue = true;
            if((!locationB.isNull && !locationA.isNull) && (!locationB.isDefault && !locationA.isDefault))
            {
                if (locationB != null && locationA != null)
                {
                    if ((locationA.Address == locationB.Address)
                    && (locationA.Description == locationB.Description)
                    && (locationA.Id == locationB.Id)
                    && (locationA.Latitude == locationB.Latitude)
                    && (Math.Round(locationA.Longitude, 6) == Math.Round(locationB.Longitude, 6))
                    && (locationA.UserId == locationB.UserId)
                    && (locationA.LocationValidation_DB == locationB.LocationValidation_DB)
                    && (locationA.LookupString == locationB.LookupString))
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
                    retValue = locationA == locationB;
                }
            } else
            {
                retValue = locationB.isNull == locationA.isNull;
                retValue &= locationB.isDefault == locationA.isDefault;
            }
            Assert.IsTrue(retValue);
            return retValue;
        }


        public static bool isTestEquivalent(this RestrictionProfile restrictionA, RestrictionProfile restrictionB)
        {
            bool retValue = true;

            if (restrictionA != null && restrictionB != null)
            {
                if (
                    (restrictionA.DaySelection.Count == restrictionB.DaySelection.Count)
                    && (restrictionA.FirstInstantiation == restrictionB.FirstInstantiation)
                    && (restrictionA.NoNull_DaySelections.Count == restrictionB.NoNull_DaySelections.Count)
                    && (restrictionA.StartDayOfWeek== restrictionB.StartDayOfWeek)
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
                retValue = restrictionA == restrictionB;
            }

            Assert.IsTrue(retValue);
            return retValue;
        }

        public static bool isTestEquivalent(this TilerUser firstUser, TilerUser seconduser)
        {
            bool retValue = false;
            if ((firstUser.FirstName == seconduser.FirstName) 
                && (firstUser.LastName == seconduser.LastName) 
                && (firstUser.Id == seconduser.Id)
                && (firstUser.Email == seconduser.Email)
                && (firstUser.OtherName == seconduser.OtherName)
                && (firstUser.TimeZone == seconduser.TimeZone))
            {
                retValue = true;
            }
            else
            {
                retValue = false;
            }
            Assert.IsTrue(retValue);
            return retValue;
        }

        public static bool isTestEquivalent(this EventDisplay firstDisplay, EventDisplay secondDisplay)
        {
            bool retValue = false;
            if((firstDisplay!=null)&& (secondDisplay!=null)&&(firstDisplay.isDefault != true) && (secondDisplay.isDefault != true)) {
                if ((firstDisplay.ColorId == secondDisplay.ColorId)
                    && (firstDisplay.Id == secondDisplay.Id)
                    && (firstDisplay.FirstInstantiation == secondDisplay.FirstInstantiation))
                {
                    retValue = true;
                }
                else
                {
                    retValue = false;
                }
            } else
            {
                if(firstDisplay == null)
                {
                    firstDisplay = new EventDisplay();
                }
                if (secondDisplay==null)
                {
                    secondDisplay = new EventDisplay();
                }
                retValue = firstDisplay.isDefault == secondDisplay.isDefault;
                Assert.IsTrue(retValue);
                return retValue;
            }
            retValue = retValue && firstDisplay.UIColor.isTestEquivalent(secondDisplay.UIColor);
            Assert.IsTrue(retValue);
            return retValue;
        }

        public static bool isTestEquivalent(this TilerColor firstColor, TilerColor secondColor)
        {
            bool retValue = false;
            if ((firstColor.B == secondColor.B)
                && (firstColor.G == secondColor.G)
                && (firstColor.O == secondColor.O)
                && (firstColor.R == secondColor.R)
                && (firstColor.FirstInstantiation == secondColor.FirstInstantiation)
                )
            {
                retValue = true;
            }
            else
            {
                retValue = false;
            }
            Assert.IsTrue(retValue);
            return retValue;
        }

        public static bool isTestEquivalent(this EventPreference firstPreference, EventPreference secondPreference)
        {
            bool retValue = false;
            if ((firstPreference.SundayCount == secondPreference.SundayCount)
                && (firstPreference.SundayAfterNoonCount == secondPreference.SundayAfterNoonCount)
                && (firstPreference.SundayDawnCount == secondPreference.SundayDawnCount)
                && (firstPreference.SundayEveningCount == secondPreference.SundayEveningCount)
                && (firstPreference.SundayLastTimeUpdated == secondPreference.SundayLastTimeUpdated)
                && (firstPreference.SundayMorningCount == secondPreference.SundayMorningCount)
                && (firstPreference.SundayNightCount== secondPreference.SundayNightCount)
                && (firstPreference.MondayCount == secondPreference.MondayCount)
                && (firstPreference.MondayAfterNoonCount == secondPreference.MondayAfterNoonCount)
                && (firstPreference.MondayDawnCount == secondPreference.MondayDawnCount)
                && (firstPreference.MondayEveningCount == secondPreference.MondayEveningCount)
                && (firstPreference.MondayLastTimeUpdated == secondPreference.MondayLastTimeUpdated)
                && (firstPreference.MondayMorningCount == secondPreference.MondayMorningCount)
                && (firstPreference.MondayNightCount == secondPreference.MondayNightCount)
                && (firstPreference.TuesdayCount == secondPreference.TuesdayCount)
                && (firstPreference.TuesdayAfterNoonCount == secondPreference.TuesdayAfterNoonCount)
                && (firstPreference.TuesdayDawnCount == secondPreference.TuesdayDawnCount)
                && (firstPreference.TuesdayEveningCount == secondPreference.TuesdayEveningCount)
                && (firstPreference.TuesdayLastTimeUpdated == secondPreference.TuesdayLastTimeUpdated)
                && (firstPreference.TuesdayMorningCount == secondPreference.TuesdayMorningCount)
                && (firstPreference.TuesdayNightCount == secondPreference.TuesdayNightCount)
                && (firstPreference.WednesdayCount == secondPreference.WednesdayCount)
                && (firstPreference.WednesdayAfterNoonCount == secondPreference.WednesdayAfterNoonCount)
                && (firstPreference.WednesdayDawnCount == secondPreference.WednesdayDawnCount)
                && (firstPreference.WednesdayEveningCount == secondPreference.WednesdayEveningCount)
                && (firstPreference.WednesdayLastTimeUpdated == secondPreference.WednesdayLastTimeUpdated)
                && (firstPreference.WednesdayMorningCount == secondPreference.WednesdayMorningCount)
                && (firstPreference.WednesdayNightCount == secondPreference.WednesdayNightCount)
                && (firstPreference.ThursdayCount == secondPreference.ThursdayCount)
                && (firstPreference.ThursdayAfterNoonCount == secondPreference.ThursdayAfterNoonCount)
                && (firstPreference.ThursdayDawnCount == secondPreference.ThursdayDawnCount)
                && (firstPreference.ThursdayEveningCount == secondPreference.ThursdayEveningCount)
                && (firstPreference.ThursdayLastTimeUpdated == secondPreference.ThursdayLastTimeUpdated)
                && (firstPreference.ThursdayMorningCount == secondPreference.ThursdayMorningCount)
                && (firstPreference.ThursdayNightCount == secondPreference.ThursdayNightCount)
                && (firstPreference.FridayCount == secondPreference.FridayCount)
                && (firstPreference.FridayAfterNoonCount == secondPreference.FridayAfterNoonCount)
                && (firstPreference.FridayDawnCount == secondPreference.FridayDawnCount)
                && (firstPreference.FridayEveningCount == secondPreference.FridayEveningCount)
                && (firstPreference.FridayLastTimeUpdated == secondPreference.FridayLastTimeUpdated)
                && (firstPreference.FridayMorningCount == secondPreference.FridayMorningCount)
                && (firstPreference.FridayNightCount == secondPreference.FridayNightCount)
                && (firstPreference.SaturdayCount == secondPreference.SaturdayCount)
                && (firstPreference.SaturdayAfterNoonCount == secondPreference.SaturdayAfterNoonCount)
                && (firstPreference.SaturdayDawnCount == secondPreference.SaturdayDawnCount)
                && (firstPreference.SaturdayEveningCount == secondPreference.SaturdayEveningCount)
                && (firstPreference.SaturdayLastTimeUpdated == secondPreference.SaturdayLastTimeUpdated)
                && (firstPreference.SaturdayMorningCount == secondPreference.SaturdayMorningCount)
                && (firstPreference.SaturdayNightCount == secondPreference.SaturdayNightCount)
                )
                {
                    retValue = true;
                }
            firstPreference.init();
            secondPreference.init();
            for (int i = 0; i < firstPreference.DayConfigs.Count; i++)
            {
                bool lookup = firstPreference[i].isTestEquivalent(secondPreference[i]);
                if(!lookup)
                {
                    break;
                }
                retValue = retValue && lookup;
            }
            Assert.IsTrue(retValue);
            return retValue;
        }

        public static bool isTestEquivalent(this DayConfig firstDayConfig, DayConfig secondDayConfig)
        {
            bool retValue = false;
            if( (firstDayConfig.Count == secondDayConfig.Count)
                && (firstDayConfig.AfterNoonCount == secondDayConfig.AfterNoonCount)
                && (firstDayConfig.DawnCount == secondDayConfig.DawnCount)
                && (firstDayConfig.EveningCount == secondDayConfig.EveningCount)
                && (firstDayConfig.LastTimeUpdated == secondDayConfig.LastTimeUpdated)
                && (firstDayConfig.MorningCount == secondDayConfig.MorningCount)
                && (firstDayConfig.NightCount == secondDayConfig.NightCount))
            {
                retValue = true;
            }

            Assert.IsTrue(retValue);
            return retValue;
        }

        public static DateTimeOffset parseAsUTC(string dateString)
        {
            return DateTimeOffset.Parse(dateString, null, DateTimeStyles.AssumeUniversal);
        }

        public class Packet
        {
            TilerUser _User;
            DbContext _Context;
            UserAccount _Account;
            public UserAccount Account
            {
                get
                {
                    return _Account;
                }
            }
            public TilerUser User { get {
                    return _User;
                }
            }
            public DbContext Context
            {
                get
                {
                    return _Context;
                }
            }
            internal Packet (TilerUser user, DbContext context)
            {
                _User = user;
                _Context = context;
                _Account = getTestUser(userId: _User.Id);
            }

            public void reloadAll ()
            {
                TestUtility.reloadTilerUser(ref _Account, ref _User);
            }
        }
    }
}

