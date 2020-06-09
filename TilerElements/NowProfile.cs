using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class NowProfile: IUndoable
    {
        protected string _Id { get; set; }
        protected TilerEvent _AssociatedEvent;
        DateTimeOffset TimePreferredForEvent;
        bool _Initialized = false;

        protected TilerEvent _UndoAssociatedEvent;
        DateTimeOffset _UndoTimePreferredForEvent;
        bool _UndoInitialized = false;
        protected string _UndoId = "";

        public NowProfile()
        {
            TimePreferredForEvent = new DateTimeOffset();
            _Initialized = false;
        }

        public NowProfile(DateTimeOffset CurrentTime, bool InitializedData)
        {
            TimePreferredForEvent = CurrentTime;
            _Initialized = InitializedData;
        }

        public NowProfile CreateCopy()
        {
            NowProfile retValue = new NowProfile(TimePreferredForEvent, _Initialized);
            retValue.Id = this.Id;
            return retValue;
        }

        #region dbProperties
        public DateTimeOffset PreferredTime
        {
            get
            {
                return TimePreferredForEvent;
            }
            set
            {
                TimePreferredForEvent = value;
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
        [NotMapped]
        public TilerEvent AssociatedEvent
        {
            get {
                return _AssociatedEvent;
            }
            set
            {
                _AssociatedEvent = value;
            }
        }
        #endregion

        public void reset()
        {
            TimePreferredForEvent = new DateTimeOffset();
            _Initialized = false;
        }

        public void undoUpdate(Undo undo)
        {
            _UndoAssociatedEvent = _AssociatedEvent;
            _UndoTimePreferredForEvent = TimePreferredForEvent;
            _UndoInitialized = _Initialized;
            FirstInstantiation = false;
            this._UndoId = undo.id;
        }

        public void undo(string undoId)
        {
            if (undoId == this.UndoId)
            {
                Utility.Swap(ref _UndoAssociatedEvent, ref _AssociatedEvent);
                Utility.Swap(ref _UndoTimePreferredForEvent, ref TimePreferredForEvent);
                Utility.Swap(ref _UndoInitialized, ref _Initialized);
            }
        }

        public void redo(string undoId)
        {
            if (undoId == this.UndoId)
            {
                Utility.Swap(ref _UndoAssociatedEvent, ref _AssociatedEvent);
                Utility.Swap(ref _UndoTimePreferredForEvent, ref TimePreferredForEvent);
                Utility.Swap(ref _UndoInitialized, ref _Initialized);
            }
        }

        public void update(NowProfile nowProfile)
        {
            if(nowProfile!=null)
            {
                this._AssociatedEvent = nowProfile._AssociatedEvent;
                this._Initialized = nowProfile._Initialized;
                this.PreferredTime = nowProfile.PreferredTime;
            }
        }

        public bool isInitialized
        {
            get
            {
                return _Initialized;
            }
            set
            {
                _Initialized = value;
            }
        }

        public virtual bool FirstInstantiation { get; set; } = true;

        public string UndoId
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
    }
}
