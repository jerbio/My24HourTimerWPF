using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements.Wpf
{
    public class CalendarEventPersist:CalendarEvent, IDB_CalendarEvent
    {
        internal CalendarEventPersist()
        {
            IsEventModified = false;
        }
        public virtual DateTimeOffset CalculationEnd { get; set; }
        public virtual int CompleteCount { get; set; }
        [ForeignKey("CreatorId")]
        public virtual TilerUser Creator
        {
            set
            {
                _Creator = value;
            }
            get
            {
                return _Creator;
            }
        }
        public virtual int DeleteCount { get; set; }
        public virtual DateTimeOffset InitializingStart { get; set; }
        public virtual bool isDeleted { get; set; }
        public virtual bool isDeletedByUser { get; set; }
        public virtual bool isDeviated { get; set; }
        public virtual bool isRepeat { get; set; }
        public virtual bool isRigid { get; set; }
        public virtual NowProfile LastNowProfile { get; set; }
        public virtual TimeSpan OriginalTimeSpanPerSplit { get; set; }
        public virtual Procrastination ProcrastinationProfile { get; set; }
        public virtual int SplitCount { get; set; }
        public virtual ICollection<SubCalendarEvent> SubCalendarEvents { get; set; }
        public virtual TimeSpan TimeSpanPerSplit { get; set; }
        public virtual EventDisplay UIData { get; set; }
        public virtual int Urgency { get; set; }
        public virtual ICollection<TilerUser> Users { get; set; }
        public virtual Repetition EventRepeat { get; set; }
        //public virtual TimeLine EventSequence { get; set; }
        public virtual bool CompleteFlag { get; set; }
        public virtual EventName Name { get; set; }
        public virtual MiscData Notes { get; set; }
        public virtual CalendarEvent RepeatRoot { get; set; }
        public virtual DateTimeOffset StartTime { get; set; }
        public virtual DateTimeOffset EndTime { get; set; }
    }
}
