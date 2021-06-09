using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TilerElements
{
    public class PausedTimeLineEntry : BusyTimeLine
    {
        protected bool _isDeleted = false;
        protected bool _isRigid = false;
        protected TimeSpan _initialTotalDuration = new TimeSpan();
        protected DateTimeOffset _creationTime = DateTimeOffset.UtcNow;


        public PausedTimeLineEntry()
            : base()
        {

        }
        public PausedTimeLineEntry(TimeSpan MyBusySpan):base(MyBusySpan)
        {
            
        }
        public PausedTimeLineEntry(string MyEventID, DateTimeOffset MyStartTime, DateTimeOffset MyEndTime) : base(MyEventID, MyStartTime, MyEndTime)
        {

        }

        public PausedTimeLineEntry(string eventID, TimeLine timeLine): base(eventID, timeLine)
        {

        }

        /// <summary>
        /// Function sets the paused timeline as final so the time line should be uneditable
        /// </summary>
        public void setAsFinal()
        {
            this.IsFinal = true;
        }

        public void setAsNotDeleted()
        {
            this.IsFinal = false;
        }

        public string getSubEventId()
        {
            return EventID.convertToSubcalendarEventID(this.Id).ToString();
        }

        [NotMapped]
        public virtual bool IsFinal
        {
            get
            {
                return _isDeleted;
            }
            set
            {
                _isDeleted = value;
            }
        }

        [NotMapped]
        public virtual TimeSpan InitialTotalDuration
        {
            get
            {
                return _initialTotalDuration;
            }
            set
            {
                _initialTotalDuration = value;
            }
        }

        [NotMapped]
        public virtual DateTimeOffset CreationTime
        {
            get
            {
                return _creationTime;
            }
            set
            {
                _creationTime = value;
            }
        }

        [NotMapped]
        public virtual bool IsRigid
        {
            get
            {
                return _isRigid;
            }
            set
            {
                _isRigid = value;
            }
        }

        public virtual bool IsDeleted_DB
        {
            get
            {
                return _isDeleted;
            }
            set
            {
                _isDeleted = value;
            }
        }

        public override JObject ToJson()
        {
            JObject retValue = base.ToJson();
            retValue.Add("InitialTotalDuration", (ulong)this.InitialTotalDuration.Ticks);
            retValue.Add("IsFinal", this.IsFinal);
            retValue.Add("IsRigid", this.IsRigid);
            retValue.Add("CreationTime", this.CreationTime.ToUnixTimeMilliseconds());
            return retValue;
        }


        public static PausedTimeLineEntry JobjectToTimeLine(JObject jObject)
        {
            string startTimeString = jObject.GetValue("start").ToString();
            string endTimeString = jObject.GetValue("end").ToString();
            string idString = jObject.GetValue("id").ToString();
            string isDeleted = jObject.GetValue("IsFinal").ToString();
            string isRigid = jObject.GetValue("IsRigid").ToString();
            string creationTimeString = jObject.GetValue("CreationTime").ToString();
            TimeSpan initialTotalDuration = TimeSpan.FromTicks( Convert.ToInt64( jObject.GetValue("InitialTotalDuration").ToString()));
            DateTimeOffset start = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(startTimeString));
            DateTimeOffset end = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(endTimeString));
            DateTimeOffset creationTime = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(creationTimeString));


            PausedTimeLineEntry retValue = new PausedTimeLineEntry(idString, start, end);
            retValue.IsFinal = Convert.ToBoolean(isDeleted);
            retValue.IsRigid = Convert.ToBoolean(isRigid);
            retValue.InitialTotalDuration = initialTotalDuration;
            retValue.CreationTime = creationTime;
            return retValue;
        }

    }
}
