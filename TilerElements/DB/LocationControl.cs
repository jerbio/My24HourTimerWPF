using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace TilerElements.DB
{
    public class LocationControl
    {
        public virtual async Task<IEnumerable<DB_LocationElements>> getCachedLocationByName(string name)
        {
            throw new NotImplementedException();
        }
    }
}