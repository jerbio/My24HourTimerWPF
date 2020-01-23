using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class UpdateTimeLine:EventTimeLine
    {
        //string _Id = Guid.NewGuid().ToString();
        public UpdateTimeLine() {
            
        }
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
        [Index("TimeLineToCalendarId", Order = 0), StringLength(512)]
        public string CalendarId
        {
            get;set;
        }
        [Index("TimeLineToRepeatCalendarId", Order = 0), StringLength(512)]
        public string RepeatCalendarId
        {
            get; set;
        }

        public override string Id {
            get {
                return !this.TimeLineEventID.isNot_NullEmptyOrWhiteSpace() ? (this.TimeLineEventID = Guid.NewGuid().ToString()) : this.TimeLineEventID;
            }
                
            set => base.Id = value;
        }
    }
}
