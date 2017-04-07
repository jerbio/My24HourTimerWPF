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
        protected Classification _claasification;
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

        public virtual async Task AnalyzeName()
        {
            if (_claasification == null)
            {
                _claasification = new Classification();
            }
            await _claasification.InitializeClassification(this._Name);
            return;
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
    }
}
