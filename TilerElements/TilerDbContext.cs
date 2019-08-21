using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;
using TilerElements;

namespace TilerElements
{
    // You can add profile data for the user by adding more properties to your TilerUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class TilerDbContext : IdentityDbContext<TilerUser>
    {
        public TilerDbContext()
        {
            this.Configuration.LazyLoadingEnabled = false;
        }
        public TilerDbContext(string connectionName)
            : base(connectionName, throwIfV1Schema: false)
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        virtual public System.Data.Entity.DbSet<TilerEvent> events { get; set; }
        virtual public System.Data.Entity.DbSet<SubCalendarEvent> SubEvents { get; set; }
        virtual public System.Data.Entity.DbSet<CalendarEvent> CalEvents { get; set; }
        virtual public System.Data.Entity.DbSet<Repetition> Repetitions { get; set; }
        virtual public System.Data.Entity.DbSet<RestrictionProfile> Restrictions { get; set; }
        virtual public System.Data.Entity.DbSet<Location> Locations { get; set; }
        virtual public System.Data.Entity.DbSet<EventDisplay> UiParams { get; set; }
        virtual public System.Data.Entity.DbSet<MiscData> MiscData { get; set; }
        virtual public System.Data.Entity.DbSet<RestrictionDay> RestrictionDays { get; set; }
        virtual public System.Data.Entity.DbSet<TilerColor> UserColors { get; set; }
        virtual public System.Data.Entity.DbSet<Undo> undos { get; set; }
        virtual public System.Data.Entity.DbSet<CalendarEventRestricted> RestrictedCalEvents { get; set; }
        virtual public System.Data.Entity.DbSet<SubCalendarEventRestricted> RestrictedSubCalEvents { get; set; }
        virtual public System.Data.Entity.DbSet<RigidCalendarEvent> RigidCalEvents { get; set; }
        virtual public System.Data.Entity.DbSet<BusyTimeLine> BusyTimelines { get; set; }
        virtual public System.Data.Entity.DbSet<EventName> EventNames { get; set; }
        virtual public System.Data.Entity.DbSet<EventTimeLine> EventTimeLines { get; set; }
        virtual public System.Data.Entity.DbSet<Classification> EventType { get; set; }
        virtual public System.Data.Entity.DbSet<GoogleTilerUser> Googleusers { get; set; }
        virtual public System.Data.Entity.DbSet<NowProfile> NowProfiles { get; set; }
        virtual public System.Data.Entity.DbSet<ProcrastinateCalendarEvent> ProcrastinteAlls { get; set; }
        virtual public System.Data.Entity.DbSet<Procrastination> Procrastinations { get; set; }
        virtual public System.Data.Entity.DbSet<Reason> Reasons { get; set; }
        virtual public System.Data.Entity.DbSet<RestrictionTimeLine> RestrictionTimeLines { get; set; }
        virtual public System.Data.Entity.DbSet<ThirdPartyTilerUser> ThirdPartyTilerUsers { get; set; }
        virtual public System.Data.Entity.DbSet<TilerUserGroup> TilerUserGroups { get; set; }
        virtual public System.Data.Entity.DbSet<ScheduleDump> ScheduleDumps{ get; set; }
        virtual public System.Data.Entity.DbSet<EventPreference> EventPreferences { get; set; }

        public static TilerDbContext Create()
        {
            return new TilerDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new TilerElements.Fluent.TilerEventMapping());
        }
    }
}