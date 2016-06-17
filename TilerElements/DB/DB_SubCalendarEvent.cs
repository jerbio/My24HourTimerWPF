using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Wpf;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TilerElements.DB
{
    public class DB_SubCalendarEvent : SubCalendarEventPersist
    {
        public override DateTimeOffset CalendarEnd { get; set; }
        public override DateTimeOffset CalendarStart { get; set; }
        public override ConflictProfile conflict { get; set; }
        public override Conflictability ConflictLevel { get; set; }
        [Required]
        public override string CreatorId
        {
            get
            {
                return base.CreatorId;
            }

            set
            {
                base.CreatorId = value;
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
        public override ulong DesiredDayIndex { get; set; }
        public override DateTimeOffset HumaneEnd { get; set; }
        public override DateTimeOffset HumaneStart { get; set; }
        public override DateTimeOffset InitializingStart { get; set; }
        public override ulong InvalidDayIndex { get; set; }
        public override bool isDeleted { get; set; }
        public override bool isDeletedByUser { get; set; }
        public override bool isDeviated { get; set; }
        public override bool isRepeat { get; set; }
        public override bool isRigid { get; set; }
        public override DateTimeOffset NonHumaneEnd { get; set; }
        public override DateTimeOffset NonHumaneStart { get; set; }
        public override ulong OldDayIndex { get; set; }
        public override Procrastination ProcrastinationProfile { get; set; }
        public override EventDisplay UIData { get; set; }
        public override int Urgency { get; set; }
        public override ICollection<TilerUser> Users { get; set; }
        public override bool CompleteFlag { get; set; }
        public override EventName Name { get; set; }
        override public MiscData Notes { get; set; }
        public override double Score { get; set; }

        public override DateTimeOffset StartTime
        {
            get
            {
                return StartDateTime;
            }
            set
            {
                this.StartDateTime = value;
            }
        }
        public override DateTimeOffset EndTime
        {
            get
            {
                return this.End;
            }

            set
            {
                this.EndDateTime = value;
            }
        }
        public override TimeSpan UsedTime
        {
            get
            {
                return _UsedTime;
            }

            set
            {
                _UsedTime = value;
            }
        }
    }
}
