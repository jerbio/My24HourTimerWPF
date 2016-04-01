using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TilerElements
{
    public class TilerUser : IdentityUser
    {
        public string FullName { get; set; }
        public DateTime LastChange { get; set; }

        public DateTimeOffset ReferenceDay { get; set; } = new DateTimeOffset();

        //public string UserName { get; set; }
        virtual public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<TilerUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType


            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here


            return userIdentity;
        }


    }
}
