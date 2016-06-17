﻿#define UseDefaultLocation

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using TilerElements.Wpf;
using System.Threading.Tasks;
//using DBTilerElement;
using System.Diagnostics;
using System.Data.Entity;

//using CassandraUserLog;
using TilerSearch;
using System.Data.Entity.Validation;

namespace TilerElements.DB
{
    public class ScheduleControl
    {
        protected TilerUser User;
        protected string UserName;
        string NameOfUser;
        protected TilerDbContext DataBase;
        protected int LastLocationID;
        protected string CurrentLog;
        protected bool LogStatus;
        protected Dictionary<string, Location_Elements> CachedLocation;
        protected Location_Elements DefaultLocation= new Location_Elements();
        protected int newImplementation = 0;
        Stopwatch debugStopWatch = new Stopwatch();
        public long totalReadMS = 0;
        protected DB_UserActivity activity;
        protected Location_Elements NewLocation;
#if ForceReadFromXml
#else
        //protected CassandraUserLog.CassandraLog myCassandraAccess;
        protected TilerSearch.EventNameSearchHandler NameSearcher;
        protected LocationSearchHandler LocationSearcher;
#endif
        Tuple<bool, string, DateTimeOffset, long> ScheduleMetadata;
        

        protected ScheduleControl()
        {
            User = null;
            UserName="";
            NameOfUser="";
            LastLocationID= 0;
            CurrentLog="";
            LogStatus=false;
#if ForceReadFromXml
#else
            NameSearcher = new EventNameSearchHandler();
            LocationSearcher = new LocationSearchHandler();
#endif
            Dictionary<string, Location_Elements> CachedLocation = new Dictionary<string, Location_Elements>();
        }


        public ScheduleControl(TilerDbContext Db)
        {
            DataBase = Db;
            LogStatus = false;
            CachedLocation = new Dictionary<string, Location_Elements>();
        }
        #region Functions

        public void updateUserActivty(UserActivity activity)
        {
            this.activity = new DB_UserActivity(activity);
        }

        /// <summary>
        /// function undoes the last activity to tile
        /// </summary>
        /// <param name="RangeOfLookup"></param>
        /// <returns></returns>
        public async Task<CustomErrors> Undo()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// updates the logcontrol with a possible new location
        /// </summary>
        /// <param name="NewLocation"></param>
        public void updateNewLocation(Location_Elements NewLocation)
        {
            this.NewLocation = NewLocation;
        }

        #region Write Data
        /// <summary>
        /// Function returns the only calendar event with the specified ID
        /// </summary>
        /// <param name="calendareventId"></param>
        /// <returns></returns>
        public CalendarEvent getCalendarEvent(string calendareventId)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Funciton gets calendar events with names similar to the getCalendarEventByName
        /// </summary>
        /// <param name="Name"></param>
        /// <returns>
        /// returns a list of calendar event with names closely matching the specified values
        /// </returns>
        public IEnumerable<CalendarEvent> getCalendarEventByName(string Name)
        {
            throw new NotImplementedException();
        }

        public async Task updateDayReference(DateTimeOffset referenceTime)
        {
            User.ReferenceDay = referenceTime;
            DataBase.Entry(User).State = EntityState.Modified;
            
            try
            {
                await DataBase.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw e;
            }
        }

        /// <summary>
        /// function persists the provided calendar events to the DB. newly added events are the events that need to be added with the changes
        /// </summary>
        /// <param name="allEvents"></param>
        /// <param name="newlyAddedEvents"></param>
        /// <returns></returns>
        public Task PersistCalendarEvents(Dictionary<string, CalendarEvent> allEvents, CalendarEvent newlyAddedEvents=null)
        {
            if (newlyAddedEvents != null)
            {
                allEvents.Remove(newlyAddedEvents.Id);
            }
            
            foreach (CalendarEvent calEvent in allEvents.Values.Where(obj=>obj.isModified))
            {
                DataBase.Entry(calEvent).State = EntityState.Modified;
            }
            CalendarEventPersist converted = newlyAddedEvents.ConvertToPersistable();
            ICollection<SubCalendarEvent> subevents=  converted.AllSubEvents;
            if (newlyAddedEvents != null)
            {
                DataBase.CalendarEvents.Add(converted);
            }


            try { 
            return DataBase.SaveChangesAsync();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw e;
            }
        }


        /// <summary>
        /// function persists the provided calendar events to the DB
        /// </summary>
        /// <param name="allEvents"></param>
        /// <returns></returns>
        //public Task PersistCalendarEvents(Dictionary<string, CalendarEvent> allEvents)
        //{
        //    foreach (CalendarEvent calEvent in allEvents.Values.Where(obj => obj.isModified))
        //    {
        //        DataBase.Entry(calEvent).State = EntityState.Modified;
        //    }
        //    return DataBase.SaveChangesAsync();
        //}

        public DateTimeOffset Truncate(DateTimeOffset dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }

        /// <summary>
        /// Funtion deletes a user's tiler account
        /// </summary>
        /// <returns></returns>
        public Task<CustomErrors> DeleteLog()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Function deletes all calendar events associated with a user account
        /// </summary>
        /// <returns></returns>
        public Task<bool> deleteAllCalendarEvents()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Read Data
        /// <summary>
        /// function returns all calendarevents that interfer with RangeOfLookup
        /// </summary>
        /// <param name="RangeOfLookup"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, CalendarEvent>> getCalendarEvents(TimeLine RangeOfLookup = null)
        {
            if(RangeOfLookup == null)
            {
                DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(-14);
                DateTimeOffset end = DateTimeOffset.UtcNow.AddDays(14);
                RangeOfLookup = new TimeLine(start, end);
            }
            IQueryable<CalendarEvent> calendarEvents  =  DataBase.CalendarEventsQuery .Where(calEvent => (calEvent.CreatorId == User.Id) && (calEvent.EndTime > RangeOfLookup.Start && calEvent.StartTime < RangeOfLookup.End));
            calendarEvents.Include(calendarEvent => calendarEvent.AllSubEvents).Include(obj=>obj.Repeat);
            Task<Dictionary<string, CalendarEvent>> RetValue = calendarEvents.ToDictionaryAsync(CalendarEvent => CalendarEvent.Id, CalendarEvent => (CalendarEvent)CalendarEvent);
            return await RetValue.ConfigureAwait(false);

            //Dictionary<string, CalendarEvent> RetValue = calendarEvents.ToDictionary(CalendarEvent => CalendarEvent.ID, CalendarEvent => (CalendarEvent)CalendarEvent);
            //return RetValue;
        }

        virtual public async Task<bool> VerifyUser(string userId)
        {
            TilerUser User = DataBase.Users.Find(userId);
            LogStatus = User != null;
            bool RetValue = LogStatus;
            if (LogStatus)
            {
                this.User = User;
            }
            return RetValue;
        }


        #endregion



        #endregion

        #region Properties

        public bool Status
        {
            get
            {
                return LogStatus;
            }
        }

        public TilerUser VerifiedUser
        {
            get
            {
                return User;
            }
        }

        public string LoggedUserID
        {
            get
            {
                return User.Id;
            }
        }

        public Location_Elements defaultLocation
        {
            get
            {
                return DefaultLocation;
            }
        }

        public string Usersname
        {
            get
            {
                return NameOfUser;
            }
        }


        #endregion
    }

}
