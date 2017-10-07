﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace TilerElements
{
    /// <summary>
    /// Reperesents the Procrastination parameters
    /// </summary>
    public class Procrastination:IUndoable
    {
        protected string _Id { get; set; }
        protected TilerEvent _AssociatedEvent;
        protected DateTimeOffset _FromTime;//Time from which an event was procrastinated
        protected DateTimeOffset _BeginTIme;//Next time for a possible calculation of a new schedule
        protected int _SectionOfDay;// stores the section of day from which it was procrastinated

        protected TilerEvent _UndoAssociatedEvent;
        protected DateTimeOffset _UndoFromTime;//Time from which an event was procrastinated
        protected DateTimeOffset _UndoBeginTime;//Next time for a possible calculation of a new schedule
        protected int _UndoSectionOfDay;// stores the section of day from which it was procrastinated

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
            _UndoAssociatedEvent = _AssociatedEvent;
            FirstInstantiation = false;
        }

        public void undo(string undoId)
        {
            if (undoId == this.UndoId)
            {
                Utility.Swap(ref _UndoFromTime, ref _FromTime);
                Utility.Swap(ref _UndoBeginTime, ref _BeginTIme);
                Utility.Swap(ref _UndoSectionOfDay, ref _SectionOfDay);
                Utility.Swap(ref _UndoAssociatedEvent, ref _AssociatedEvent);
            }
        }

        public void redo(string undoId)
        {
            if (undoId == this.UndoId)
            {
                Utility.Swap(ref _UndoFromTime, ref _FromTime);
                Utility.Swap(ref _UndoBeginTime, ref _BeginTIme);
                Utility.Swap(ref _UndoSectionOfDay, ref _SectionOfDay);
                Utility.Swap(ref _UndoAssociatedEvent, ref _AssociatedEvent);
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

        public ulong DislikedDayIndex
        {
            get
            {
                return ReferenceNow.getDayIndexFromStartOfTime(_FromTime);
            }
        }

        public ulong PreferredDayIndex
        {
            get
            {
                return ReferenceNow.getDayIndexFromStartOfTime(_BeginTIme);
            }
        }

        #region dbProperties
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

        [ForeignKey("Id")]
        public TilerEvent AssociatedEvent
        {
            get
            {
                return _AssociatedEvent;
            }
            set
            {
                _AssociatedEvent = value;
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
                return SectionOfDay;
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
