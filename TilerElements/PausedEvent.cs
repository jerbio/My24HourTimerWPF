using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerElements
{
    [Table("PausedEvent")]
    public class PausedEvent
    {
        [Column(Order = 0),Key, ForeignKey("User"), Index("UserIdAndSubEventIdClustering",Order =0, IsUnique = true, IsClustered = false), Index("UserIdAndPauseStatus", Order = 0)]
        public string UserId { get; set; }
        TilerUser _User;
        /// <summary>
        /// User for which the event is associated
        /// </summary>
        public TilerUser User {
            get {
                return _User;
            }
            set {
                _User = value;
            }
        }

        /// <summary>
        /// Id of the sub evnet that is being paused
        /// </summary>
        [Column(Order = 1), Key, Index("UserIdAndSubEventIdClustering", Order = 1, IsUnique = true, IsClustered = false)]
        public string EventId { get; set; }
        /// <summary>
        /// When the event is paused. Accuracy is to the minute
        /// </summary>
        public DateTimeOffset PauseTime { get; set; } = DateTimeOffset.UtcNow;
        /// <summary>
        /// Is the current event paused
        /// </summary>
        [Index("UserIdAndPauseStatus", Order = 1)]
        public bool isPauseDeleted { get; set; } = false;
    }
}