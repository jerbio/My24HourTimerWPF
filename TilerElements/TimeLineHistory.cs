using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    [Serializable]
    public class TimeLineHistory
    {
        string _Id = Guid.NewGuid().ToString();
        protected SubEventDictionary<string, UpdateTimeLine> _TimeLines;

        public TimeLineHistory() { }

        public TimeLineHistory CreateCopy ()
        {
            TimeLineHistory retValue = new TimeLineHistory();
            retValue.Id = this.Id;
            retValue.TimeLines_DB = this.TimeLines?.Select(o=>o.CreateCopy() as UpdateTimeLine) .ToList();
            return retValue;
        }

        public string Id
        {
            get
            {
                return _Id;
            }
            set
            {
                _Id = value;
            }
        }

        public void addTimeLine(UpdateTimeLine timeLine)
        {
            (_TimeLines ?? (_TimeLines = new SubEventDictionary<string, UpdateTimeLine>())).Add(timeLine);
        }

        [NotMapped]
        public virtual ICollection<UpdateTimeLine> TimeLines
        {
            get
            {
                return (_TimeLines ?? (_TimeLines = new SubEventDictionary<string, UpdateTimeLine>()))?.Values.Where(obj => obj != null).ToArray();
            }
            
        }

        public virtual List<UpdateTimeLine> TimeLines_DB_XML
        {
            set
            {
                this._TimeLines = new SubEventDictionary<string, UpdateTimeLine>();
                if (value != null)
                {
                    this._TimeLines = new SubEventDictionary<string, UpdateTimeLine>(value);
                }
            }
            get
            {
                var retValue = (_TimeLines ?? (_TimeLines = new SubEventDictionary<string, UpdateTimeLine>())).Collection;
                return retValue.Select(o=>o.Value).ToList();
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public virtual ICollection<UpdateTimeLine> TimeLines_DB
        {
            set
            {
                this._TimeLines = new SubEventDictionary<string, UpdateTimeLine>();
                if (value != null)
                {
                    this._TimeLines = new SubEventDictionary<string, UpdateTimeLine>(value);
                }
            }
            get
            {
                return _TimeLines ?? (_TimeLines = new SubEventDictionary<string, UpdateTimeLine>());
            }
        }
    }
}
