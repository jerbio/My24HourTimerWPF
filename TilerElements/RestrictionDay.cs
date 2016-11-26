using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class RestrictionDay
    {
        DayOfWeek _DayOfWeek;
        RestrictionTimeLine _RestrictionTimeLine;
        public RestrictionDay(DayOfWeek dayOfWeek, RestrictionTimeLine restrictionTimeLine)
        {
            this._DayOfWeek = dayOfWeek;
            this._RestrictionTimeLine = restrictionTimeLine;
        }

        public DayOfWeek DayOfWeek
        {
            get
            {
                return this._DayOfWeek;
            }
        }

        public RestrictionTimeLine RestrictionTimeLine
        {
            get
            {
                return this._RestrictionTimeLine;
            }
        }

    }
}
