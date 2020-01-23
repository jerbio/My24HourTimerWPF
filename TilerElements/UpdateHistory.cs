using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    [Serializable]
    public class UpdateHistory
    {
        string _Id = Guid.NewGuid().ToString();
        protected SubEventDictionary<string, UpdateTimeLine> _TimeLines;

        public UpdateHistory() { }

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
            _TimeLines.Add(timeLine);
        }

        [NotMapped]
        public virtual ICollection<UpdateTimeLine> TimeLines
        {
            get
            {
                return _TimeLines?.Values.Where(obj => obj != null).ToArray();
            }
            
        }


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
