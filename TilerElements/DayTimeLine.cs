using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TilerElements
{
    public class DayTimeLine: TimelineWithSubcalendarEvents
    {
        long UniversalDayIndex;
        int BoundDayIndex;
        SubCalendarEvent _sleepSubEvent;
        SubCalendarEvent _wakeSubEVent;
        SubCalendarEvent _previousDaySleepSubEvent;
        #region Constructor
        public DayTimeLine(DateTimeOffset Start, DateTimeOffset End, long UniversalIndex, int BoundedIndex=-1):base(Start, End, null)
        {
            AllocatedSubEvents = new ConcurrentDictionary<string, SubCalendarEvent>();
            freeSpace = EndTime - StartTime;
            this.UniversalDayIndex = UniversalIndex;
            this.BoundDayIndex = BoundedIndex;
        }
        #endregion
        #region functions
        override public TimeLine CreateCopy()
        {
            DayTimeLine CopyTimeLine = new DayTimeLine(this.StartTime, this.EndTime, UniversalDayIndex, BoundDayIndex);
            CopyTimeLine.AllocatedSubEvents = new ConcurrentDictionary<string, SubCalendarEvent>(AllocatedSubEvents);
            CopyTimeLine.OccupancyOfTImeLine = this.OccupancyOfTImeLine;
            return CopyTimeLine;
        }
        public override void updateOccupancyOfTimeLine()
        {
            base.updateOccupancyOfTimeLine();
        }
        #endregion

        #region Properties
        public int BoundedIndex
        {
            get 
            {
                return BoundDayIndex;
            }
        }

        public long UniversalIndex
        {
            get 
            {
                return UniversalDayIndex;
            }
        }


        /// <summary>
        /// This is the subevent for the current day after which sleep is expected. Note this is always towards the end of the day
        /// </summary>
        public virtual SubCalendarEvent SleepSubEvent
        {
            get
            {
                return _sleepSubEvent;
            }
            set
            {
                if(_sleepSubEvent != null)
                {
                    _sleepSubEvent.isSleep = false;
                }
                _sleepSubEvent = value;
                _sleepSubEvent.isSleep = true;
            }
        }
        /// <summary>
        /// This is the subevent for the current day before which sleep is expected. So there sholuld be a sleep span before this sub event
        /// </summary>
        public virtual SubCalendarEvent WakeSubEvent
        {
            get
            {
                return _wakeSubEVent ?? AllocatedSubEvents.Values.FirstOrDefault(subEvent => subEvent.isWake);
            }
            set
            {
                if (_wakeSubEVent != null)
                {
                    _wakeSubEVent.isWake = false;
                }
                _wakeSubEVent = value;
                _wakeSubEVent.isWake = true;
            }
        }


        /// <summary>
        /// This is the subevent for the current day after which sleep is expected. This sleep time chunk is before "WakeSubevent". This often occurs if an event has a deadline that is within the sleep time frame. So for example a 1 hour subevent with a 2:00am deadline when the sleep time frame of 12:00AM- 6:00AM 
        /// </summary>
        public SubCalendarEvent PrecedingDaySleepSubEvent { get; set; }
        #endregion

    }
}
