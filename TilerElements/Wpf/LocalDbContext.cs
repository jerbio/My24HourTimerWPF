using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Wpf;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TilerElements.Wpf
{
    /// <summary>
    /// THis class serve a way to access data locally through wpf. This should hvae limited mixing with DBTilerElement.ScheduleContext. This could be easily used to import data from xml or some other datasource
    /// </summary>
    public class LocalContext: IdentityDbContext<TilerUser>
    {
        public virtual DbSet<CalendarEventPersist> CalendarEvents {get; set;}
        public virtual DbSet<DB.DB_LocationElements> Location_Elements { get; set; }
        public virtual DbSet<SubCalendarEventPersist> SubCalendarevents { get; set; }

        public virtual IQueryable<CalendarEventPersist> CalendarEventsQuery
        {
            get
            {
                return CalendarEvents;
            }
        }
        public virtual IQueryable<SubCalendarEventPersist> SubCalendareventsQuery
        {
            get
            {
                return SubCalendarevents;
            }
        }
        public virtual DbSet<Repetition> Repetitions { get; set; }

        public LocalContext(): base("DefaultConnection", throwIfV1Schema: false)
        {

        }

        public LocalContext(string Connection = "DefaultConnection", bool throwIfV1Schema = false)
            : base(Connection, throwIfV1Schema: false)
        {
        }

        public static LocalContext Create()
        {
            return new LocalContext();
        }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //    modelBuilder.Entity<Location_Elements>().ToTable("Location_Elements");
        //}
    }
}
