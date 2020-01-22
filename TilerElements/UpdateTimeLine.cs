using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class UpdateTimeLine:EventTimeLine
    {
        public UpdateTimeLine (DateTimeOffset start, DateTimeOffset end, DateTimeOffset timeOfUpdate)
        {
            this.StartTime = start;
            this.StartTime = end;
            this._UpdateTime = timeOfUpdate;
        }
        protected DateTimeOffset _UpdateTime;
        public DateTimeOffset UpdateTime
        {
            get
            {
                return _UpdateTime;
            }
        }
        public long UpdateTime_DB {
            get {
                return _UpdateTime.ToUnixTimeMilliseconds();
            }
            set {
                _UpdateTime = DateTimeOffset.FromUnixTimeMilliseconds(value);
            }
        }
    }
}
