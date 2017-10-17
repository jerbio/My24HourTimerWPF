using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class EventName:IUndoable
    {
        protected string _UndoId;
        protected string _Name = "";
        public string _UndoName;
        protected string _Id = Guid.NewGuid().ToString();
        public virtual bool FirstInstantiation { get; set; } = true;
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

        public void undoUpdate(Undo undo)
        {
            _UndoName = Name;
            FirstInstantiation = false;
        }

        public void undo(string undoId)
        {
            if(_UndoId == undoId)
            {
                Utility.Swap(ref _Name, ref _UndoName);
            }
        }

        public void redo(string undoId)
        {
            if (_UndoId == undoId)
            {
                Utility.Swap(ref _Name, ref _UndoName);
            }
        }
        #region dataModelProperties
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

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        public string UndoId
        {
            set
            {
                _UndoId = value;
            }
            get
            {
                return _UndoId;
            }
        }
        #endregion
    }
}
