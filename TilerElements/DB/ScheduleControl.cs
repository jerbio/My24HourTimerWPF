#define UseDefaultLocation

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
#if ForceReadFromXml
#else
//using CassandraUserLog;
using TilerSearch;
#endif



namespace TilerElements.DB
{
    public class ScheduleControl
    {
        protected string ID;
        protected string UserName;
        string NameOfUser;
        protected DBControl LogDBDataAccess;
        protected int LastLocationID;
        protected string CurrentLog;
        protected bool LogStatus;
        protected Dictionary<string, Location_Elements> CachedLocation;
        protected Location_Elements DefaultLocation= new Location_Elements();
        protected int newImplementation = 0;
        Stopwatch debugStopWatch = new Stopwatch();
        public long totalReadMS = 0;
#if ForceReadFromXml
#else
        //protected CassandraUserLog.CassandraLog myCassandraAccess;
        protected TilerSearch.EventNameSearchHandler NameSearcher;
        protected LocationSearchHandler LocationSearcher;
#endif
        Tuple<bool, string, DateTimeOffset, long> ScheduleMetadata;
        
#if ForceReadFromXml
#else
        public static bool useCassandra=true;
#endif

        protected ScheduleControl()
        { 
            ID="";
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


        public ScheduleControl(DBControl DBAccess)
        {
            LogDBDataAccess = DBAccess;
            LogStatus = false;
            CachedLocation = new Dictionary<string, Location_Elements>();
        }
        #region Functions



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

        /// <summary>
        /// function persists the provided calendar events to the DB
        /// </summary>
        /// <param name="AllEvents"></param>
        /// <returns></returns>
        public Task PersistCalendarEvents(IEnumerable<CalendarEvent> AllEvents)
        {
            throw new NotImplementedException();
        }

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
            throw new NotImplementedException();
        }

        #endregion


        #region Cassandra Functions

        void TransferXmlFileToCassandra()
        { 
        
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

        public string LoggedUserID
        {
            get
            {
                return ID;
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
