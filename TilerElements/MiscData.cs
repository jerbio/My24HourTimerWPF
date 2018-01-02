using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class MiscData: IUndoable
    {
        string _Id;
        string _UserTypedData;
        int _Type;//Entry Source. 0-> No Entry from Calendar Event. 1->From CalendarEvent. 2-> from SubCalendarEvent

        public int _UndoType;
        public string _UndoUserTypedData;
        protected string _UndoId = "";

        #region constructor



        public MiscData()
        {
            _UserTypedData = "";
            _Type = 0;
        }
        
        public MiscData(string TypedNotes, int Type=0)
        {
            _UserTypedData = TypedNotes;
            this._Type = Type;
        }
        #endregion

        #region Function
        public MiscData createCopy()
        {
            MiscData retValue = new MiscData();
            retValue._Type = _Type;
            retValue._UserTypedData = _UserTypedData;

            return retValue;
        }

        public void undoUpdate(Undo undo)
        {
            _UndoType = _Type;
            _UndoUserTypedData = _UserTypedData;
        }

        public void undo(string undoId)
        {
            if (undoId == this.UndoId)
            {
                Utility.Swap(ref _UndoType, ref _Type);
                Utility.Swap(ref _UndoUserTypedData, ref _UserTypedData);
            }
        }

        public void redo(string undoId)
        {
            if (undoId == this.UndoId)
            {
                Utility.Swap(ref _UndoType, ref _Type);
                Utility.Swap(ref _UndoUserTypedData, ref _UserTypedData);
            }
        }

        #endregion



        #region property

        public string UserNote
        {
            get
            {
                return _UserTypedData;
            }
            set
            {
                UserTypedData = value;
            }
        }


        public int TypeSelection
        {
            get
            {
                return _Type;
            }
        }

        #region dbProperties
        public int Type
        {
            get
            {
                return _Type;
            }

            set
            {
                _Type = value;
            }
        }


        public string UserTypedData
        {
            get
            {
                return _UserTypedData;
            }

            set
            {
                _UserTypedData = value;
            }
        }

        public string Id
        {
            get
            {
                return _Id ?? (_Id = Guid.NewGuid().ToString());
            }
            set
            {
                _Id = value;
            }
        }

        public virtual bool FirstInstantiation { get; set; } = true;

        public virtual string UndoId
        {
            get
            {
                return _UndoId;
            }
            set
            {
                _UndoId = value;
            }
        }

        public int UndoType {
            get
            {
                return _UndoType;
            }
            set
            {
                _UndoType = value;
            }
        }
        public string UndoUserTypedData
        {
            get
            {
                return _UndoUserTypedData;
            }
            set
            {
                _UndoUserTypedData = value;
            }
        }

        #endregion
        #endregion

    }
}
