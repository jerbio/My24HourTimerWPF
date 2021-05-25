using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TilerElements
{
    public class PausedTimeLine : BusyTimeLine
    {
        protected bool _isDeleted = false;

        public PausedTimeLine()
            : base()
        {

        }
        public PausedTimeLine(TimeSpan MyBusySpan):base(MyBusySpan)
        {
            
        }
        public PausedTimeLine(string MyEventID, DateTimeOffset MyStartTime, DateTimeOffset MyEndTime) : base(MyEventID, MyStartTime, MyEndTime)
        {

        }

        public PausedTimeLine(string eventID, TimeLine timeLine): base(eventID, timeLine)
        {

        }

        [NotMapped]
        public virtual bool IsDeleted
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
            var retValue = base.ToJson();
            retValue.Add("isDeleted", this.IsDeleted);
            return retValue;
        }


        public static PausedTimeLine JobjectToTimeLine(JObject jObject)
        {
            string startTimeString = jObject.GetValue("start").ToString();
            string endTimeString = jObject.GetValue("end").ToString();
            string idString = jObject.GetValue("id").ToString();
            string isDeleted = jObject.GetValue("isDeleted").ToString();
            DateTimeOffset start = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(startTimeString));
            DateTimeOffset end = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(endTimeString));


            PausedTimeLine retValue = new PausedTimeLine(idString, start, end);
            retValue.IsDeleted = Convert.ToBoolean(isDeleted);
            return retValue;
        }

    }
}
