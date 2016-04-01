using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements.Wpf;
using TilerElements.DB;

namespace TilerElements.DB
{
    public interface IDB_CalendarEvent:ITilerEvent
    {
        int CompleteCount { get; set; }
        int DeleteCount { get; set; }
        //TimeLine EventSequence { get; set; }
        DateTimeOffset CalculationEnd { get; set; }
        Repetition EventRepetition { get; set; }
        NowProfile LastNowProfile { get; set; }
        int SplitCount { get; set; }
        CalendarEvent RepeatRoot { get; set; }
        TimeSpan TimeSpanPerSplit { get; set; }
        TimeSpan OriginalTimeSpanPerSplit { get; set; }
        ICollection<SubCalendarEvent> SubCalendarEvents { get; set; }       
    }
}