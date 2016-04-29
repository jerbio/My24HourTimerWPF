
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading.Tasks;
using TilerElements;
using TilerElements.Wpf;




namespace TilerElements.DB
{
    public class UserAccount
    {
        
        protected ScheduleControl UserLog;
        protected string ID="";
        protected string Name;
        protected string Username;
        string Password;
        protected DBControl UserAccountDBAccess;
        protected TilerUser sessionUser;
        /// <summary>
        /// controls access to location elements for a given user account(s)
        /// </summary>
        protected LocationControl UserLocation;
        
        public UserAccount()
        {
            Username = "";
            Password = "";
        }
        /*
        public UserAccount(string UserName, string PassWord)
        {
            this.Username = UserName;
            this.Password = TilerFront.DBControl.encryptString(PassWord);
        }
        */
        public UserAccount(string UserName, string UserID)
        {
            this.Username = UserName;
            this.ID = UserID;
            this.Password = "";
        }

        virtual public async Task<bool> Login()
        {
            return await UserLog.VerifyUser(ID);
            //throw new NotImplementedException();
        }






        virtual async protected Task<Dictionary<string, CalendarEvent>>  getAllCalendarElements(TimeLine RangeOfLookup, string desiredDirectory="")
        {
            Dictionary<string, CalendarEvent> retValue=new Dictionary<string,CalendarEvent>();
            retValue = await UserLog.getCalendarEvents(RangeOfLookup);
            return retValue;
        }

        virtual async protected Task<DateTimeOffset> getDayReferenceTime(string desiredDirectory = "")
        {
            DateTimeOffset retValue = sessionUser.ReferenceDay;
            return retValue;
        }

        virtual public bool DeleteAllCalendarEvents()
        {
            bool retValue = false;
            
            if (UserLog.Status)
            {
                UserLog.deleteAllCalendarEvents();
                retValue = true;
            }
            else
            {
                retValue = false;
            }
            return retValue;
        }


        virtual async public Task<CustomErrors> DeleteLog()
        {
            return await UserLog.DeleteLog();
        }


        /// <summary>
        /// updates the end of day with referenceTime
        /// </summary>
        /// <param name="referenceTime"></param>
        async virtual public void UpdateReferenceDayTime(DateTimeOffset referenceTime)
        {
            throw new NotImplementedException();
            /*sessionUser.ReferenceDay = referenceTime;
            Task SaveChangesToDB = new Controllers.UserController().SaveUser(sessionUser);
            await SaveChangesToDB;*/
        }
        /*
        public void CommitEventToLog(CalendarEvent MyCalEvent)
        {
            UserLog.WriteToLog(MyCalEvent);
        }
        */
        virtual async public Task  CommitEventToLogOld(IEnumerable<CalendarEvent> AllEvents)
        {
            await UserLog.PersistCalendarEvents(AllEvents);//, LatestID, LogFile).ConfigureAwait(false);
        }


        virtual protected async Task<Dictionary<string, CalendarEvent>> getAllCalendarElements(TimeLine RangeOfLookup)//, string desiredDirectory = "")
        {
            Dictionary<string, CalendarEvent> retValue = new Dictionary<string, CalendarEvent>();
            retValue = await UserLog.getCalendarEvents(RangeOfLookup);
            return retValue;
        }





        #region properties


        /// <summary>
        /// gets the reference time that represents the end of the day for a given user. Its sent as a Datetimeoffset object so the date portin of the object isn't too useful
        /// </summary>
        public DateTimeOffset endOfdayTime
        {
            get
            {
                return this.sessionUser.ReferenceDay;
            }
        }

        public bool Status
        {
            get
            {
                return UserLog.Status;
            }
        }

        public string UserID
        {
            get 
            {
                return ID;
            }
        }

        public string UserName
        {
            get
            {
                return Username;
            }
        }

        public string Usersname
        {
            get
            {
                return Name;
            }
        }


        virtual public ScheduleControl ScheduleData
        {
            get
            {
                return UserLog;
            }
        }

        virtual public LocationControl Location
        {
            get
            {
                return UserLocation;
            }
        }

#endregion 

    }
}
