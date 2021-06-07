using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class ScheduleProfile
    {
        protected TilerUser _User;
        protected string _Id = Guid.NewGuid().ToString();
        protected string _PausedTileId;
        protected SubCalendarEvent _PausedTile;




        public void clearPausedEventId()
        {
            this.PausedTileId = null;
            this.PausedTile_DB = null;
        }

        public void setPausedEventId(SubCalendarEvent pausedEvent)
        {
            this.PausedTile_DB = pausedEvent;
        }


        [Key]
        public virtual string Id
        {
            get
            {
                return _Id;
            }
            set
            {
                _Id = value;
            }
        }
        [ForeignKey("Id"), Required]
        public virtual TilerUser User_DB
        {
            get
            {
                return _User;
            }
            set
            {
                _User = value;
            }
        }

        public virtual string PausedTileId
        {
            get
            {
                return _PausedTileId;
            }
            set
            {
                _PausedTileId = value;
            }
        }
        [ForeignKey("PausedTileId")]
        public virtual SubCalendarEvent PausedTile_DB
        {
            get
            {
                return _PausedTile;
            }
            set
            {
                _PausedTile = value;
            }
        }


    }
}
