using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Wpf;
using TilerElements;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TilerElements.DB
{
    public class DB_CalendarEvent : CalendarEventPersist
    {
        public override DateTimeOffset CalculationEnd { get; set; }
        public override int CompleteCount { get; set; }
        [Required]
        public override string CreatorId
        {
            get
            {
                return _CreatorId;
            }

            set
            {
                _CreatorId = value;
            }
        }
        [ForeignKey("CreatorId")]
        public override TilerUser Creator
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
        public override int DeleteCount { get; set; }
        public override DateTimeOffset InitializingStart { get; set; }
        public override bool isDeleted { get; set; }
        public override bool isDeletedByUser { get; set; }
        public override bool isDeviated { get; set; }
        public override bool isRepeat { get; set; }
        public override bool isRigid { get; set; }
        public override NowProfile LastNowProfile { get; set; }
        public override TimeSpan OriginalTimeSpanPerSplit { get; set; }
        public override Procrastination ProcrastinationProfile { get; set; }
        public override int SplitCount { get; set; }
        public override ICollection<SubCalendarEvent> SubCalendarEvents { get; set; }
        public override TimeSpan TimeSpanPerSplit { get; set; }
        public override EventDisplay UIData { get; set; }
        public override int Urgency { get; set; }
        public override ICollection<TilerUser> Users { get; set; }
        public override Repetition EventRepeat { get; set; }
        //public override TimeLine EventSequence { get; set; }
        public override bool CompleteFlag { get; set; }
        public override EventName Name { get; set; }
        public override MiscData Notes { get; set; }
        public override CalendarEvent RepeatRoot { get; set; }
        public override DateTimeOffset StartTime { get; set; }
        public override DateTimeOffset EndTime { get; set; }
    }


}
