﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public abstract class TilerEvent
    {
        public static TimeSpan ZeroTimeSpan = new TimeSpan(0);
        protected string EventName="";
        protected DateTimeOffset StartDateTime;
        protected DateTimeOffset EndDateTime;
        protected bool Complete = false;
        protected bool Enabled = true;
        protected bool DeadlineElapsed = false;
        protected bool UserDeleted = false;
        protected bool FromRepeatEvent=false;
        protected Location_Elements LocationInfo;
        protected EventDisplay UiParams = new EventDisplay();
        protected MiscData DataBlob = new MiscData();
        protected bool RepetitionFlag;
        protected bool RigidSchedule;
        protected TimeSpan EventDuration;
        protected string otherPartyID;
        protected TimeSpan EventPreDeadline;
        protected TimeSpan PrepTime;
        protected EventID UniqueID;
        protected List<string> UserIDs= new List<string>();
        protected int Priority;
        protected bool isRestricted = false;
        protected static DateTimeOffset EventNow = DateTimeOffset.Now;
        protected static TimeSpan CalculationEndSpan = new TimeSpan(180, 0, 0, 0, 0);
        protected Procrastination ProfileOfProcrastination = new Procrastination(new DateTimeOffset(), new TimeSpan());
        protected NowProfile ProfileOfNow = new NowProfile();
        protected bool ThirdPartyFlag = false;
        protected string ThirdPartyUserIDInfo;
        protected ThirdPartyControl.CalendarTool ThirdPartyTypeInfo = ThirdPartyControl.CalendarTool.Tiler;
        protected string CreatorIDInfo;
        protected TimeSpan _UsedTime = new TimeSpan();
        protected Classification Semantics= new Classification();
        protected TimeOfDayPreferrence DaySectionPreference = new TimeOfDayPreferrence();

        async public Task InitializeClassification()
        {
            await Semantics.InitializeClassification(EventName);
        }
        public List<string> getAllUserIDs()
        {
            return UserIDs.ToList();
        }
        public void updateEventName(string NewName)
        {
            EventName = NewName;
        }

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

        virtual public  DateTimeOffset End
        {
            get
            {
                return EndDateTime;
            }
        }

        virtual public  DateTimeOffset Start
        {
            get
            {
                return StartDateTime;
            }
        }

        public ThirdPartyControl.CalendarTool ThirdpartyType
        {
            get 
            {
                return ThirdPartyTypeInfo;
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

        virtual public string ThirdPartyUserID
        {
            get
            {
                return ThirdPartyUserIDInfo;
            }
        }

        virtual public Location_Elements myLocation
        {
            set
            {
                LocationInfo = value;
            }
            get
            {
                return LocationInfo;
            }
        }

        virtual public bool FromRepeat
        {
            get
            {
                return FromRepeatEvent;
            }
        }

         virtual public TimeSpan Preparation
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
        public bool isDeadlineElapsed
         {
             get 
             {
                 return DeadlineElapsed;
             }
         }
        public string Name
        {
            get
            {
                return EventName;
            }
        }

        public bool isUserDeleted
        {
            get
            {
                return UserDeleted;
            }

        }

        public int EventPriority
        {
            get
            {
                return Priority;
            }
        }

        public Procrastination  ProcrastinationInfo
        {
            get
            {
                return ProfileOfProcrastination;
            }
        }

        public NowProfile NowInfo
        {
            get
            {
                return ProfileOfNow;
            }
        }

        public void InitializeDayPreference()
        { 
            DaySectionPreference.InitializeGrouping(this);// InitializeGrouping
        }

        public TimeOfDayPreferrence getDaySection()
        {
            return DaySectionPreference;
        }
        public string CreatorID
        {
            get
            {
                return CreatorIDInfo;
            }
        }
        virtual public TimeSpan UsedTime
        {
            set
            {
                throw new NotImplementedException("You are trying to set the used up time in a tiler events. Invalid action.");
            }

            get
            {
                return _UsedTime;
            }
        }

        virtual public string Id
        {
            get
            {
                return UniqueID.ToString();
            }
        }
        virtual public DateTimeOffset Deadline
        {
            get {
                return End;
            }
        }

        virtual public TimeSpan ActiveDuration
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        virtual public string ID
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        virtual public EventID TilerID
        {
            get
            {
                return UniqueID;
            }
        }
        public virtual string ToString()
        {
            return this.Start.ToString() + " - " + this.End.ToString() + "::" + this.ID + "\t\t::" + this.ActiveDuration.ToString();
        }
    }
}
