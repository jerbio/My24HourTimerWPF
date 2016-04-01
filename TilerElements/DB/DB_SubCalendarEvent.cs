using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TilerElements.DB
{
    public abstract class DB_SubCalendarEvent : SubCalendarEvent, IDB_SubCalendarEvent
    {
        public abstract DateTimeOffset CalendarEnd { get; set; }
        public abstract DateTimeOffset CalendarStart { get; set; }
        public abstract ConflictProfile conflict { get; set; }
        public abstract Conflictability ConflictLevel { get; set; }
        public abstract string CreatorId { get; set; }
        public abstract ulong DesiredDayIndex { get; set; }
        public abstract DateTimeOffset HumaneEnd { get; set; }
        public abstract DateTimeOffset HumaneStart { get; set; }
        public abstract DateTimeOffset InitializingStart { get; set; }
        public abstract ulong InvalidDayIndex { get; set; }
        public abstract bool isDeleted { get; set; }
        public abstract bool isDeletedByUser { get; set; }
        public abstract bool isDeviated { get; set; }
        public abstract bool isRepeat { get; set; }
        public abstract bool isRigid { get; set; }
        public abstract DateTimeOffset NonHumaneEnd { get; set; }
        public abstract DateTimeOffset NonHumaneStart { get; set; }
        public abstract ulong OldDayIndex { get; set; }
        public abstract Procrastination ProcrastinationProfile { get; set; }
        public abstract EventDisplay UIData { get; set; }
        public abstract int Urgency { get; set; }
        public abstract ICollection<TilerUser> Users { get; set; }
        public abstract new bool isComplete { get; set; }
        public abstract new EventName Name { get; set; }
        abstract public new MiscData Notes { get; set; }
        public abstract new double Score { get; set; }
    }
}
