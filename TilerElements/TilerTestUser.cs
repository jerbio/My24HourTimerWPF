using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;

namespace My24HourTimerWPF
{
    public class TilerTestUser : TilerUser
    {
        protected TilerTestUser()
        {

        }
        public TilerTestUser(string id)
        {
            this.Id = id;
        }
    }
}
