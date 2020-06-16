using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using NodaTime;

namespace TilerElements
{
    public class TilerUser : IdentityUser, IHasId
    {
        public TilerUser():base()
        {

        }

        public static TilerUser autoUser = new TilerUser()
        {
            _Id = "autoUser",
            Email = "cantTouchThis@tiler.com"
        };

        public static TilerUser googleUser = new TilerUser()
        {
            _Id = "googleUser",
            Email = "googleUser@tiler.com"
        };
        public string FullName {
            get {
                return FirstName + " " + OtherName ?? "" + LastName;
            }
        }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string OtherName { get; set; } = "";
        protected DateTimeOffset _EndfOfDay { get; set; }
        protected string _EndfOfDayString { get; set; } = "10:00pm";
        protected TravelCache _TravelCache { get; set; }

        public DateTimeOffset LastScheduleModification { get; set; }
        public string ClearAllId { get; set; }
        public string LatestId { get; set; }
        public string CalendarType { get; set; } = ThirdPartyControl.CalendarTool.tiler.ToString();
        protected TimeSpan _TimeZoneDifference;
        protected string _Id{get;set;}

        public override string Id
        {
            get
            {
                return _Id;
            }

            set
            {
                Guid idAsGuid;
                if(Guid.TryParse(value, out idAsGuid))
                {
                    _Id = idAsGuid.ToString();
                }
                else
                {
                    throw new CustomErrors("Invalid id provided for tiler user");
                }
            }
        }

        protected string _TimeZone = "UTC";

        public string TimeZone
        {
            get
            {
                return _TimeZone;
            }
            set
            {
                _TimeZone = value;
            }
        }

        public TimeSpan TimeZoneDifference
        {
            get
            {
                return _TimeZoneDifference;
            }
        }

        virtual public double TimeZoneDifferenceMS_DB
        {
            set
            {
                _TimeZoneDifference = TimeSpan.FromMilliseconds(value);
            }
            get
            {
                return _TimeZoneDifference.TotalMilliseconds;
            }
        }

        public DateTimeOffset EndfOfDay
        {
            get
            {
                return _EndfOfDay;
            }
            set
            {
                _EndfOfDay = value;
            }
        }

        public string EndfOfDayString
        {
            get
            {
                return _EndfOfDayString;
            }
            set
            {
                _EndfOfDayString = value;
            }
        }

        public TravelCache TravelCache
        {
            set
            {
                _TravelCache = value;
            }
            get
            {
                return _TravelCache;
            }
        }

        public void updateTimeZoneTimeSpan(TimeSpan timeZoneDifference)
        {
            this._TimeZoneDifference = timeZoneDifference;
        }

        /// <summary>
        /// Longitude of user
        /// </summary>
        public double LastKnownLongitude { get; set; }
        /// <summary>
        /// Latitude of user
        /// </summary>
        public double LastKnownLatitude { get; set; }
        /// <summary>
        /// IsLocationVerified
        /// </summary>
        public bool LastKnownLocationVerified { get; set; }

        public void updateTimeZone()
        {
            DateTimeZone userTimeZone = DateTimeZoneProviders.Tzdb[this.TimeZone];
            DateTimeOffset localDate = DateTimeOffset.Parse(this._EndfOfDayString).removeSecondsAndMilliseconds();
            LocalDateTime time = new LocalDateTime(localDate.Year, localDate.Month, localDate.Day, localDate.Hour, localDate.Minute);
            _EndfOfDay = userTimeZone.AtStrictly(time).ToDateTimeUtc();
        }

        public DayOfWeek BeginningOfWeek { get; set; } = DayOfWeek.Sunday;

        public DateTimeOffset MidNight {
            get {
                return EndfOfDay.AddHours(3);
            }
        }

        public string getClearAllEventsId()
        {
            if (string.IsNullOrEmpty(ClearAllId))
            {
                ClearAllId = EventID.GenerateCalendarEvent().ToString();
            }
            return ClearAllId;
        }

        //public string UserName { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<TilerUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType


            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here


            return userIdentity;
        }
    }
}
