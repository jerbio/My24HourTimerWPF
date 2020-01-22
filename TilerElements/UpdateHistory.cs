using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    [Serializable]
    public class UpdateHistory
    {
        string _Id = Guid.NewGuid().ToString();
        protected SubEventDictionary<string, UpdateTimeLine> _TimeLines = new SubEventDictionary<string, UpdateTimeLine>();


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

        public virtual ICollection<UpdateTimeLine> TimeLines
        {
            get
            {
                return _TimeLines?.Values.Where(obj => obj != null).ToArray();
            }
        }
    }
}
