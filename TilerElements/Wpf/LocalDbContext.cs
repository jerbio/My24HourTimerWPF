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
    public class LocalDbContext: IdentityDbContext<TilerUser>
    {
        public virtual DbSet<CalendarEvent> CalendarEvents { get; set; }
        public virtual DbSet<SubCalendarEvent> SubCalendarevents { get; set; }
        public virtual DbSet<Repetition> Repetitions { get; set; }

        public LocalDbContext(): base("DefaultConnection", throwIfV1Schema: false)
        {

        }

        public LocalDbContext(string Connection = "DefaultConnection", bool throwIfV1Schema = false)
            : base(Connection, throwIfV1Schema: false)
        {
        }

        public static LocalDbContext Create()
        {
            return new LocalDbContext();
        }
    }
}
