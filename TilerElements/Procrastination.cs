using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace TilerElements
{
    /// <summary>
    /// Reperesents the Procrastination parameters
    /// </summary>
    public class Procrastination : IUndoable
    {
        static Procrastination defaultProcrastination;
        protected string _Id;
        protected DateTimeOffset _FromTime;//Time from which an event was procrastinated
        protected DateTimeOffset _BeginTIme;//Next time for a possible calculation of a new schedule
        protected int _SectionOfDay;// stores the section of day from which it was procrastinated
        
        protected TilerEvent _UndoAssociatedEvent;
        protected DateTimeOffset _UndoFromTime;//Time from which an event was procrastinated
        protected DateTimeOffset _UndoBeginTime;//Next time for a possible calculation of a new schedule
        protected int _UndoSectionOfDay;// stores the section of day from which it was procrastinated
        protected bool isNull = false;
        protected string _UndoId = "";
        protected Procrastination()
        {

        }

        public Procrastination(DateTimeOffset From, TimeSpan Span)
        {
            _FromTime = From;
            _BeginTIme = _FromTime.Add(Span);
        }


        public void reset()
        {
            _FromTime = new DateTimeOffset();
            _BeginTIme = new DateTimeOffset();
        }


        public Procrastination CreateCopy(string id = "")
        {
            Procrastination retValue = new Procrastination(this._FromTime, _BeginTIme - _FromTime);
            if (string.IsNullOrEmpty(id))
            {
                retValue._Id = this.Id;
            }
            else
            {
                retValue._Id = id;
            }

            return retValue;
        }

        public static Procrastination getDefaultProcrastination ()
        {
            if(defaultProcrastination == null)
            {
                defaultProcrastination = new Procrastination();
                defaultProcrastination.isNull = true;
            }

            return defaultProcrastination;
        }

        [NotMapped]
        public TilerEvent UndoAssociatedEvent
        {
            get
            {
                return _UndoAssociatedEvent;
            }
            set
            {
                _UndoAssociatedEvent = value;
            }
        }
        public DateTimeOffset UndoFromTime
        {
            get
            {
                return _UndoFromTime;
            }
            set
            {
                _UndoFromTime = value;
            }
        }
        public DateTimeOffset UndoBeginTime
        {
            get
            {
                return _UndoBeginTime;
            }
            set
            {
                _UndoBeginTime = value;
            }
        }
        public int UndoSectionOfDay
        {
            get
            {
                return _UndoSectionOfDay;
            }
            set
            {
                _UndoSectionOfDay = value;
            }
        }

        public void undoUpdate(Undo undo)
        {
            _UndoFromTime = _FromTime;
            _UndoBeginTime = _BeginTIme;
            _UndoSectionOfDay = _SectionOfDay;
            FirstInstantiation = false;
            this._UndoId = undo.id;
        }

        public void undo(string undoId)
        {
            if (undoId == this.UndoId)
            {
                Utility.Swap(ref _UndoFromTime, ref _FromTime);
                Utility.Swap(ref _UndoBeginTime, ref _BeginTIme);
                Utility.Swap(ref _UndoSectionOfDay, ref _SectionOfDay);
            }
        }

        public void redo(string undoId)
        {
            if (undoId == this.UndoId)
            {
                Utility.Swap(ref _UndoFromTime, ref _FromTime);
                Utility.Swap(ref _UndoBeginTime, ref _BeginTIme);
                Utility.Swap(ref _UndoSectionOfDay, ref _SectionOfDay);
            }
        }
        #region properties
        public DateTimeOffset PreferredStartTime
        {
            get
            {
                return _BeginTIme;
            }
        }

        public DateTimeOffset DislikedStartTime
        {
            get
            {
                return _FromTime;
            }
        }

        public DayOfWeek DislikedDayOfWeek
        {
            get
            {
                return _FromTime.DayOfWeek;
            }
        }

        public int DislikedDaySection
        {
            get
            {
                return _SectionOfDay;
            }
        }

        public ulong DislikedDayIndex(ReferenceNow now)
        {
            return now.getDayIndexFromStartOfTime(FromTime);
        }

        public ulong PreferredDayIndex(ReferenceNow now)
        {
            return now.getDayIndexFromStartOfTime(BeginTIme);
        }

        #region dbProperties
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

        public DateTimeOffset FromTime
        {
            set
            {
                _FromTime = value;
            }
            get
            {
                return _FromTime;
            }
        }
        public DateTimeOffset BeginTIme
        {
            set
            {
                _BeginTIme = value;
            }
            get
            {
                return _BeginTIme;
            }
        }
        public int SectionOfDay
        {
            set
            {
                _SectionOfDay = value;
            }
            get
            {
                return _SectionOfDay;
            }
        }

        public bool FirstInstantiation { get; set; } = true;

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
        #endregion
        #endregion
    }
}
