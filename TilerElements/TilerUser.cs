using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using Microsoft.AspNet.Identity;

namespace TilerElements
{
    public class TilerUser : IdentityUser, IHasId
    {

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
        public string TimeZone { get; set; } = "UTC";
        public DateTimeOffset EndfOfDay { get; set; }
        public DateTimeOffset LastScheduleModification { get; set; }
        public string ClearAllId { get; set; }
        public string LatestId { get; set; }
        public string CalendarType { get; set; } = ThirdPartyControl.CalendarTool.tiler.ToString();
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
