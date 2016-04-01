using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements.Wpf;

namespace TilerElements.DB
{
    public interface IDB_SubCalendarEvent: ITilerEvent
    {
        #region properties

        

        DateTimeOffset CalendarEnd { get; set; }


        DateTimeOffset CalendarStart { get; set; }

        /// <summary>
        /// returns the humane end time i.e ideal start time for a subevent to start
        /// </summary>
        DateTimeOffset HumaneStart { get; set; }
        /// <summary>
        /// returns the humane end time i.e ideal end time for a subevent to end
        /// </summary>
        DateTimeOffset HumaneEnd { get; set; }

        /// <summary>
        /// returns the NonHumane end time i.e ideal start time for a subevent to start
        /// </summary>
        DateTimeOffset NonHumaneStart { get; set; }
        /// <summary>
        /// returns the NonHumane end time i.e ideal end time for a subevent to end
        /// </summary>
        DateTimeOffset NonHumaneEnd { get; set; }

        ulong OldDayIndex { get; set; }
        ulong DesiredDayIndex { get; set; }

        ulong InvalidDayIndex { get; set; }

        string CreatorId { get; set; }
        /// <summary>
        /// Function gets and sets the conflict setting for an event. It can be either averse, normal, Tolerant.
        /// </summary>
        TilerElements.Wpf.TilerEvent.Conflictability ConflictLevel { get; set; }

        /// <summary>
        /// Holds the evaluated efficiency of the current subevent. Its based on a scale of 1
        /// </summary>
        double Score { get; set; }
        ConflictProfile conflict { get; set; }
        #endregion
    }
}