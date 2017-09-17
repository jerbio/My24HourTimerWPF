using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class RestrictionDay
    {
        DayOfWeek _DayOfWeek;
        RestrictionTimeLine _RestrictionTimeLine;
        public RestrictionDay(DayOfWeek dayOfWeek, RestrictionTimeLine restrictionTimeLine)
        {
            this._DayOfWeek = dayOfWeek;
            this._RestrictionTimeLine = restrictionTimeLine;
        }

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
        public string DayOfWeek
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
        #endregion
        #endregion
    }
}
