using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements.Wpf;
using System.ComponentModel.DataAnnotations.Schema;

namespace TilerElements.DB
{
    public class DB_RestrictionTimeLine : RestrictionTimeLine
    {
        public string  RestrictionProfileId { get; set; }
        [ForeignKey("RestrictionProfileId")]
        public DB_RestrictionProfile RestrictionProfile { get; set; }
        public DayOfWeek WeekDay { get; set; }
        public DB_RestrictionTimeLine(DayOfWeek weekDay, DateTimeOffset start, DateTimeOffset end, TimeSpan span, DB_RestrictionProfile profile)
        {
            this.StartTimeOfDay = start;
            this.EndTimeOfDay = end;
            this.RangeTimeSpan = span;
            this.RestrictionProfile = profile;
            this.WeekDay = weekDay;
        }

        public new DateTimeOffset Start
        {
            get
            {
                return this.StartTimeOfDay;
            }
            set
            {
                this.StartTimeOfDay = value;
            }
        }

        public new DateTimeOffset End
        {
            get
            {
                return this.EndTimeOfDay;
            }
            set
            {
                this.EndTimeOfDay = value;
            }
        }
            
        public new TimeSpan Span
        {
            get
            {
                return this.RangeTimeSpan;
            }
            set
            {
                this.RangeTimeSpan = value;
            }
        }
        
    }

     

}