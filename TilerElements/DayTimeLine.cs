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
        ulong UniversalDayIndex;
        int BoundDayIndex;
        SubCalendarEvent _sleepSubEVent;
        SubCalendarEvent _wakeSubEVent;
        #region Constructor
        public DayTimeLine(DateTimeOffset Start, DateTimeOffset End, ulong UniversalIndex, int BoundedIndex=-1):base(Start, End, null)
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

        public ulong UniversalIndex
        {
            get 
            {
                return UniversalDayIndex;
            }
        }
        
        public SubCalendarEvent SleepSubEvent
        {
            get
            {
                return _sleepSubEVent ?? AllocatedSubEvents.Values.FirstOrDefault(subEvent => subEvent.isSleep);
            }
            set
            {
                if(_sleepSubEVent != null)
                {
                    _sleepSubEVent.isSleep = false;
                }
                _sleepSubEVent = value;
                _sleepSubEVent.isSleep = true;
            }
        }

        public SubCalendarEvent WakeSubEvent
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

        public SubCalendarEvent PrecedingDaySleepSubEvent { get; set; }
        #endregion

    }
}
