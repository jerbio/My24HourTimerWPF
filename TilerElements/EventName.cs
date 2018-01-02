using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        protected TilerUser _Creator;
        protected TilerEvent _Event;
        protected EventName()
        {
            
        }
        public EventName(TilerUser tilerUser, TilerEvent tilerEvent, string name = "")
        {
            _Name = name;
            _Creator = tilerUser;
            _Event = tilerEvent;
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

        public string CreatorId { get; set; }
        [Required, ForeignKey("CreatorId")]
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

        public TilerEvent AssociatedEvent
        {
            get
            {
                return _Event;
            }
            set
            {
                _Event = value;
            }
        }

        public EventName createCopy(string id = null)
        {
            EventName retValue = new EventName(this.Creator_EventDB, this.AssociatedEvent);
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

        public virtual string Name
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

        public virtual string UndoId
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
        public override string ToString()
        {
            return this._Name + "||" + this._Id;
        }
    }
}
