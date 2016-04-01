using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using TilerElements;

namespace DBTilerElement
{
    [Table("CalendarEventEventNames")]
    public class DB_TilerEventName: EventName
    {
        public new string Id {
            get
            {
                return ID;
            }

            set
            {
                Guid testValue;
                if (Guid.TryParse(value, out testValue))
                {
                    ID = value;
                }
                else
                {
                    throw new Exception("Invalid id for event display");
                }

            }
        }

    }
    [Table("ModifiedCalendarEventEventNames")]
    public class ModifiedTilerEventName: DB_TilerEventName
    {}

    [Table("ModifiedSubCalendarEventEventNames")]
    public class ModifiedSubCalendarEventEventName : DB_TilerEventName
    { }
}
