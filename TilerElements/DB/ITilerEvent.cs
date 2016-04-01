using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Wpf;

namespace TilerElements.DB
{
    public interface ITilerEvent
    {
        string Id { get; set; }
        DateTimeOffset InitializingStart { get; set; }
        DateTimeOffset Start { get; set; }
        DateTimeOffset End { get; set; }
        /// <summary>
        /// Function gets and sets the priority of the current task.
        /// </summary>
        int Urgency { get; set; }
        /// <summary>
        /// Is this tiler event deleted by the user or deleted by calculation
        /// </summary>
        bool isDeletedByUser { get; set; }
        /// <summary>
        /// Is this event rigid
        /// </summary>
        bool isRigid { get; set; }
        /// <summary>
        /// Is the event deleted
        /// </summary>
        bool isDeleted { get; set; }
        /// <summary>
        /// Is the event completed
        /// </summary>
        bool isComplete { get; set; }
        /// <summary>
        /// IS this event a recurring event
        /// </summary>
        bool isRepeat { get; set; }

        /// <summary>
        /// Classification of the event semantics
        /// </summary>
        Classification Classification { get; set; }
        /// <summary>
        /// procrastination info for the tiler event
        /// </summary>
        Procrastination ProcrastinationProfile { get; set; }
        /// <summary>
        /// NOtes information for the tilerevent
        /// </summary>
        MiscData Notes { get; set; }
        /// <summary>
        /// user ids associated with this event
        /// </summary>
        ICollection<TilerUser> Users { get; set; }
        /// <summary>
        /// Represesnts the UI elements like color associated with an event
        /// </summary>
        EventDisplay UIData { get; set; }
        /// <summary>
        /// event creator Id
        /// </summary>
        string CreatorId { get; set; }
        /// <summary>
        /// Name Of An Event
        /// </summary>
        EventName Name { get; set; }
        /// <summary>
        /// Is the current event deviating from normal calculation
        /// </summary>
        bool isRestricted
        {
            get;
        }
        /// <summary>
        /// is the current deviated from the normal tilerevent list
        /// </summary>
        bool isDeviated { set; get; }
    }
}