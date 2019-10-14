using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    [Serializable]
    public class LocationJson:Location
    {
        public DateTimeOffset LastUsed { get; set; } = Utility.JSStartTime;
    }
}
