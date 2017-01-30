using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;
using TilerFront;

namespace My24HourTimerWPF
{
    public class WPF_Schedule:DB_Schedule
    {
        public WPF_Schedule(Dictionary<string, CalendarEvent> allEventDictionary, DateTimeOffset starOfDay, Dictionary<string, Location> locations, DateTimeOffset referenceNow, TilerUser user):base(allEventDictionary, starOfDay, locations, referenceNow, user)
        {

        }
    }
}
