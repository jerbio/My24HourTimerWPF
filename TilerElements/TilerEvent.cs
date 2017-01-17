using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public abstract class TilerEvent: IWhy
    {
        public static TimeSpan ZeroTimeSpan = new TimeSpan(0);
        //protected string EventName="";
        protected EventName _Name;
        protected DateTimeOffset StartDateTime;
        protected DateTimeOffset EndDateTime;
        protected bool Complete = false;
        protected bool Enabled = true;
        protected bool DeadlineElapsed = false;
        protected bool UserDeleted = false;
        protected Location_Elements LocationInfo;
        protected EventDisplay UiParams = new EventDisplay();
        protected MiscData DataBlob = new MiscData();
        protected Repetition EventRepetition;
        //protected bool RepetitionFlag;
        protected bool RigidSchedule;
        protected TimeSpan EventDuration;
        protected string otherPartyID;
        protected TimeSpan EventPreDeadline;
        protected TimeSpan PrepTime;
        protected EventID UniqueID;
        protected int Priority;
        protected bool isRestricted = false;
        protected static DateTimeOffset EventNow = DateTimeOffset.UtcNow;
        protected static TimeSpan CalculationEndSpan = new TimeSpan(180, 0, 0, 0, 0);
        protected Procrastination ProfileOfProcrastination = new Procrastination(new DateTimeOffset(), new TimeSpan());
        protected NowProfile ProfileOfNow = new NowProfile();
        protected bool ThirdPartyFlag = false;
        protected string ThirdPartyUserIDInfo;
        protected ThirdPartyControl.CalendarTool ThirdPartyTypeInfo = ThirdPartyControl.CalendarTool.tiler;
        protected TilerUser _Creator;
        protected TimeSpan _UsedTime = new TimeSpan();
        protected Classification Semantics= new Classification();
        protected TimeOfDayPreferrence DaySectionPreference;
        protected TilerUserGroup _Users;
        protected string _TimeZone = "UTC";
        protected bool isProcrastinateEvent = false;

        #region IwhyImplementation
        abstract public IWhy Because();

        abstract public IWhy OtherWise();

        abstract public IWhy WhatIf(params Reason[] reasons);
        #endregion

        async public Task InitializeClassification()
        {
            await Semantics.InitializeClassification(_Name.NameValue);
        }
        public TilerUserGroup getAllUsers()
        {
            return _Users;
        }

        /// <summary>
        /// This updates the name of a calendar event
        /// </summary>
        /// <param name="NewName">The new name of the calendar event</param>
        virtual public void updateEventName(string NewName)
        {
            _Name.updateName(NewName);
        }

        public bool isComplete
        {
            get
            {
                return Complete;
            }
        }

        //virtual protected void setEventId(EventID id)
        //{
        //    this.UniqueID = id;
        //}

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

        virtual public bool isEnabled
        {
            get
            {
                return Enabled;
            }
        }

        virtual public bool isDeleted
        {
            get
            {
                return !isEnabled;
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


        public bool isProcrastinateCalendarEvent
        {
            get
            {
                return isProcrastinateEvent;
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

        public virtual bool isRepeat
        {
            get
            {
                return EventRepetition.Enable;
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
        public EventName Name
        {
            get
            {
                return _Name;
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

        public void InitializeDayPreference(TimeLine timeLine)
        {
            if (DaySectionPreference == null)
            {
                DaySectionPreference = new TimeOfDayPreferrence(timeLine);
            }
            DaySectionPreference.InitializeGrouping(this);// InitializeGrouping
        }

        public TimeOfDayPreferrence getDaySection()
        {
            return DaySectionPreference;
        }
        public string CreatorId
        {
            get
            {
                return _Creator.Id;
            }
        }

        public TilerUser Creator
        {
            get
            {
                return _Creator;
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

        virtual public string getId
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

        virtual public EventID TilerID
        {
            get
            {
                return UniqueID;
            }
        }
        public override string ToString()
        {
            return this.Start.ToString() + " - " + this.End.ToString() + "::" + this.getId + "\t\t::" + this.ActiveDuration.ToString();
        }

        virtual public bool Rigid
        {
            set
            {
                RigidSchedule = value;
            }
            get
            {
                return RigidSchedule;
            }
        }

        virtual public TimeLine RangeTimeLine
        {
            get
            {
                TimeLine retValue = new TimeLine(this.Start, this.End);
                return retValue;
            }
        }

        virtual public string getTimeZone
        {
            get
            {
                return _TimeZone;
            }
        }
    }
}
