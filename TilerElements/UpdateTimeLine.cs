using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    [Serializable]
    [Table("UpdateTimeLine")]
    public class UpdateTimeLine:EventTimeLine
    {
        protected TilerUser _Creator;
        public UpdateTimeLine() {
            
        }
        public UpdateTimeLine (DateTimeOffset start, DateTimeOffset end, DateTimeOffset timeOfUpdate)
        {
            this.StartTime = start;
            this.StartTime = end;
            this._UpdateTime = timeOfUpdate;
        }


        public override TimeLine CreateCopy()
        {
            UpdateTimeLine retValue = new UpdateTimeLine(this.Start, this.End, this._UpdateTime);
            retValue.Id = this.Id;
            retValue._Creator = this._Creator;
            retValue.OccupiedSlots = this.OccupiedSlots;
            retValue.RepeatCalendarId = this.RepeatCalendarId;
            retValue.CalendarId= this.CalendarId;
            retValue.AddBusySlots(this.ActiveTimeSlots);
            return retValue;
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

        public string CreatorId { get; set; }
        [Required, ForeignKey("CreatorId")]
        [Index("UserToTimeLineId", Order = 0)]
        [System.Xml.Serialization.XmlIgnore]
        public TilerUser Creator_EventDB
        {
            get
            {
                return _Creator;
            }
            set
            {
                _Creator = value;
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
