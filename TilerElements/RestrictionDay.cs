using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    [Serializable]
    public class RestrictionDay: IUndoable
    {
        protected string _Id = Guid.NewGuid().ToString();
        DayOfWeek _DayOfWeek;
        RestrictionTimeLine _RestrictionTimeLine;

        #region undoMembers
        public string UndoDayOfWeek;
        public RestrictionTimeLine UndoRestrictionTimeLine;
        public string _UndoId;
        #endregion
        #region constructor
        protected RestrictionDay()
        {

        }

        public RestrictionDay(DayOfWeek dayOfWeek, RestrictionTimeLine restrictionTimeLine)
        {
            this._DayOfWeek = dayOfWeek;
            this._RestrictionTimeLine = restrictionTimeLine;
        }
        #endregion
        #region functions
        public override string ToString()
        {
            string retValue = "" + this.DayOfWeekString + "||" + _RestrictionTimeLine.Start + " - " + _RestrictionTimeLine.End;
            return retValue;
        }
        public virtual string ToDbString()
        {
            string retValue = JsonConvert.SerializeObject(this);
            return retValue;
        }
        #endregion

        #region properties
        [NotMapped]
        public DayOfWeek WeekDay
        {
            get
            {
                return this._DayOfWeek;
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
        /// <summary>
        /// This return the string for day of the week, if you want the actual day of the week use Weekday
        /// </summary>
        [Column("DayOfWeek")]
        public string DayOfWeekString
        {
            get
            {
                return this._DayOfWeek.ToString();
            }
            set
            {
                if (value != null)
                {
                    _DayOfWeek = Utility.ParseEnum<DayOfWeek>(value);
                }
            }
        }

        public string RestrictionTimeLineId { get; set; }
        [ForeignKey("RestrictionTimeLineId")]
        public RestrictionTimeLine RestrictionTimeLine
        {
            get
            {
                return this._RestrictionTimeLine;
            }
            set
            {
                this._RestrictionTimeLine = value;
            }
        }

        public virtual bool FirstInstantiation { get; set; } = true;

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

        public void redo(string undoId)
        {
            if (undoId == UndoId)
            {
                DayOfWeek weekDay = Utility.ParseEnum<DayOfWeek>(UndoDayOfWeek);
                Utility.Swap(ref weekDay, ref _DayOfWeek);
                UndoDayOfWeek = weekDay.ToString();
                _RestrictionTimeLine.undo(undoId);
            }
        }

        public void undo(string undoId)
        {
            if (undoId == UndoId)
            {
                DayOfWeek weekDay = Utility.ParseEnum<DayOfWeek>(UndoDayOfWeek);
                Utility.Swap(ref weekDay, ref _DayOfWeek);
                UndoDayOfWeek = weekDay.ToString();
                _RestrictionTimeLine.undo(undoId);
            }
        }

        public void undoUpdate(Undo undo)
        {
            UndoDayOfWeek = _DayOfWeek.ToString();
            _RestrictionTimeLine.undoUpdate(undo);
            this.UndoId = undo.id;
            FirstInstantiation = false;
        }
        #endregion
        #endregion
    }
}
