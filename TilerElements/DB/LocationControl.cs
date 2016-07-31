using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using TilerElements.Wpf;
namespace TilerElements.DB
{
    public class LocationControl
    {
        TilerDbContext Context;
        TilerUser User;
        public LocationControl(TilerDbContext db, TilerUser user)
        {
            Context = db;
            User = user;
        }
        public virtual async Task<IEnumerable<Location_Elements>> getCachedLocationByName(string name)
        {
            List<DB_LocationElements> RetValue = new List<DB_LocationElements>();
            name = name.Trim();
            if(!string.IsNullOrEmpty(name) && !string.IsNullOrWhiteSpace(name))
            {
                RetValue = (Context.Location_Elements.Where(obj => obj.FullAddress.Contains(name)).ToList());
            }
            return RetValue;
        }
    }
}