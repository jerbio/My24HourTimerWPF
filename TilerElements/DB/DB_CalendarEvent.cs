using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Wpf;
using TilerElements;

namespace TilerElements.DB
{
    public abstract class DB_CalendarEvent : CalendarEvent, IDB_CalendarEvent
    {
        public abstract DateTimeOffset CalculationEnd { get; set; }
        public abstract int CompleteCount { get; set; }
        public abstract string CreatorId { get; set; }
        public abstract int DeleteCount { get; set; }
        public abstract DateTimeOffset InitializingStart { get; set; }
        public abstract bool isDeleted { get; set; }
        public abstract bool isDeletedByUser { get; set; }
        public abstract bool isDeviated { get; set; }
        public abstract bool isRepeat { get; set; }
        public abstract bool isRigid { get; set; }
        public abstract NowProfile LastNowProfile { get; set; }
        public abstract TimeSpan OriginalTimeSpanPerSplit { get; set; }
        public abstract Procrastination ProcrastinationProfile { get; set; }
        public abstract int SplitCount { get; set; }
        public abstract ICollection<SubCalendarEvent> SubCalendarEvents { get; set; }
        public abstract TimeSpan TimeSpanPerSplit { get; set; }
        public abstract EventDisplay UIData { get; set; }
        public abstract int Urgency { get; set; }
        public abstract ICollection<TilerUser> Users { get; set; }
        public abstract DateTimeOffset End { get; set; }
        public abstract Repetition EventRepetition { get; set; }
        //public abstract TimeLine EventSequence { get; set; }
        public abstract bool isComplete { get; set; }
        public abstract EventName Name { get; set; }
        public abstract MiscData Notes { get; set; }
        public abstract CalendarEvent RepeatRoot { get; set; }
        public abstract DateTimeOffset Start { get; set; }
    }


}
