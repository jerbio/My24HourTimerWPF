using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements.Wpf
{
    public class SubCalendarEventPersist: SubCalendarEvent, ISubCalendarEvent
    {

        public virtual TilerUser Creator
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public DateTimeOffset CalendarEnd
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public DateTimeOffset CalendarStart
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public DateTimeOffset HumaneStart
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public DateTimeOffset HumaneEnd
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public DateTimeOffset NonHumaneStart
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public DateTimeOffset NonHumaneEnd
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public ulong OldDayIndex
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public ulong DesiredDayIndex
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public ulong InvalidDayIndex
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public Conflictability ConflictLevel
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public double Score
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public ConflictProfile conflict
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public DateTimeOffset InitializingStart
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public DateTimeOffset StartTime
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public DateTimeOffset EndTime
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public int Urgency
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public bool isDeletedByUser
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public bool isRigid
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public bool isDeleted
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public bool CompleteFlag
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public bool isRepeat
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public Procrastination ProcrastinationProfile
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public MiscData Notes
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public ICollection<TilerUser> Users
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public EventDisplay UIData
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public EventName Name
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        virtual public bool isDeviated
        {
            get
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }

            set
            {
                throw new NotImplementedException("Need to inherit class and implement properties");
            }
        }

        public override string Id
        {
            set
            {
                UniqueID = new EventID(value);
            }
        }

        override public TimeSpan UsedTime
        {
            set
            {
                _UsedTime = value;
            }

            get
            {
                return _UsedTime;
            }
        }

    }
}
