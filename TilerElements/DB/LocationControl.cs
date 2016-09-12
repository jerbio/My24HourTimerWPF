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
        public virtual async Task<IEnumerable<Location_Elements>> searchLocationByName(string name, string userId)
        {
            List<DB_LocationElements> RetValue = new List<DB_LocationElements>();
            if(!string.IsNullOrEmpty(name) && !string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim().ToLower();
                RetValue = (Context.Location_Elements.Where(obj => (obj.CreatorId == userId) && (obj.FullAddress.Contains(name) || obj.Name.Contains(name))).ToList());
            }
            return RetValue;
        }

        public virtual async Task<Location_Elements> getByLocationCacheName(string name, string userId)
        {
            DB_LocationElements RetValue = null;
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim().ToLower();
                RetValue = (Context.Location_Elements.Where(obj => (obj.CreatorId == userId) && (obj.Name == name)).SingleOrDefault());
            }
            return RetValue;
        }
    }
}