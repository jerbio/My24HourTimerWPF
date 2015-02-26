using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public abstract class TilerEvent
    {
        protected string EventName;
        protected DateTimeOffset StartDateTime;
        protected DateTimeOffset EndDateTime;
        protected bool Complete = false;
        protected bool Enabled = true;
        protected bool DeadlineElapsed = false;
        protected bool UserDeleted = false;
        protected bool FromRepeatEvent=false;
        protected Location_Elements LocationData;
        protected EventDisplay UiParams = new EventDisplay();
        protected MiscData DataBlob = new MiscData();
        protected bool RepetitionFlag;
        protected bool RigidSchedule;
        protected TimeSpan EventDuration;
        protected string otherPartyID;
        protected TimeLine EventSequence;
        protected TimeSpan EventPreDeadline;
        protected TimeSpan PrepTime;
        protected EventID UniqueID;
        protected List<string> UserIDs;
        protected int Priority;
        protected bool isRestricted = false;
        protected static DateTimeOffset EventNow = DateTimeOffset.Now;
        protected static TimeSpan CalculationEndSpan = new TimeSpan(180, 0, 0, 0, 0);

        public bool isComplete
        {
            get
            {
                return Complete;
            }
        }

        public EventDisplay UIParam
        {
            get
            {
                return UiParams;
            }
        }

        public bool isActive
        {
            get
            {
                return ((!isComplete) && (isEnabled));
            }
        }

        public bool isEnabled
        {
            get
            {
                return Enabled;
            }
        }

        public  DateTimeOffset End
        {
            get
            {
                return EndDateTime;
            }
        }

        public  DateTimeOffset Start
        {
            get
            {
                return StartDateTime;
            }
        }


        public  string ThirdPartyID
        {
            get
            {
                return otherPartyID;
            }
            set
            {
                otherPartyID = value;
            }
        }

         public Location_Elements myLocation
        {
            set
            {
                LocationData = value;
            }
            get
            {
                return LocationData;
            }
        }

        public bool FromRepeat
        {
            get
            {
                return FromRepeatEvent;
            }
        }

         public TimeSpan Preparation
         {
             get
             {
                 return PrepTime;
             }
         }
         public TimeSpan PreDeadline
         {
             get
             {
                 return EventPreDeadline;
             }
         }

         public bool isEventRestricted
         {
             get
             {
                 return isRestricted;
             }
         }

    }
}
