using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class EventName
    {
        protected string _Name = "";
        protected string _Id = Guid.NewGuid().ToString();
        public EventName(string name = "")
        {
            _Name = name;
        }

        public string NameValue
        {
            get
            {
                return _Name;
            }
        }

        public string NameId
        {
            get
            {
                return _Id;
            }
        }

        public void updateName(string name)
        {
            _Name = name;
        }

        public EventName createCopy(string id = null)
        {
            EventName retValue = new EventName();
            retValue._Id = id;
            retValue._Name = this._Name;
            if (string.IsNullOrEmpty(id))
            {
                retValue._Id = this._Id;
            }
            return retValue;
        }

        public override string ToString()
        {
            return this._Name + "||" + this._Id;
        }
    }
}
