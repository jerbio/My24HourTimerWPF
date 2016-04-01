using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements.DB
{
    interface IDB_SubCalendarEventRestricted: IDB_SubCalendarEvent, IRestrictedEvent
    {
    }
}
