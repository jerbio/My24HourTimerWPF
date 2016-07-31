using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Connectors;

namespace TilerElements.Wpf
{
    public abstract class TilerEvent
    {
        public static TimeSpan ZeroTimeSpan = new TimeSpan(0);
        protected EventName NameOfEvent;// ="";
        protected DateTimeOffset StartDateTime;
        protected DateTimeOffset EndDateTime;
        protected bool Complete = false;
        protected bool Enabled = true;
        protected bool DeadlineElapsed = false;
        protected bool UserDeleted = false;
        protected bool FromRepeatEvent=false;
        protected Location_Elements LocationInfo;
        protected EventDisplay UiParams;// = new EventDisplay();
        protected MiscData _DataBlob;// = new MiscData();
        //protected bool RepetitionFlag;
        protected bool RigidSchedule;
        protected TimeSpan EventDuration;
        protected string otherPartyID;
        protected TimeSpan EventPreDeadline;
        protected TimeSpan PrepTime;
        protected EventID UniqueID;
        protected List<TilerUser> UserIDs= new List<TilerUser>();
        protected int Priority;
        protected static DateTimeOffset EventNow = DateTimeOffset.Now;
        protected static TimeSpan CalculationEndSpan = new TimeSpan(180, 0, 0, 0, 0);
        protected Procrastination ProfileOfProcrastination;// = new Procrastination(new DateTimeOffset(), new TimeSpan());
        protected NowProfile ProfileOfNow;// = new NowProfile();
        protected long RepetitionSequence = 0;
        protected DateTimeOffset OriginalStart;
        public enum Conflictability { Averse, Normal, Tolerant};
        protected Conflictability ConflictSetting = Conflictability.Normal;
        /// <summary>
        /// Holds the Id to the initializing the Event Id. This should be the Id to the calendarevent
        /// </summary>
        protected EventID OriginalEventID;
        protected Boolean DeviationFlag = false;
        protected bool IsEventModified = true;


        protected bool ThirdPartyFlag = false;
        protected string ThirdPartyUserIDInfo;
        protected ThirdPartyControl.CalendarTool ThirdPartyTypeInfo = ThirdPartyControl.CalendarTool.Tiler;
        protected string  _CreatorId;
        protected TilerUser _Creator;
        protected TimeSpan _UsedTime = new TimeSpan();
        protected Classification Semantics;//= new Classification();

        async public Task InitializeClassification()
        {
            if(Semantics == null)
            {
                Semantics = new Classification();
            }
            await Semantics.InitializeClassification(NameOfEvent.NameString);
        }
        public List<TilerUser> getAllUsers()
        {
            return UserIDs.ToList();
        }
        public void updateEventName(string NewName)
        {
            NameOfEvent = new EventName( NewName,Id);
        }

        public void updateEventName(EventName NewName)
        {
            NameOfEvent = NewName;
        }

        internal void setAsUserDeleted()
        {
            UserDeleted = true;
        }

        internal void setAsDeviated()
        {
            DeviationFlag = true;
        }

        internal void setAsNotDeviated()
        {
            DeviationFlag = false;
        }

        internal bool getDeviationFlag()
        {
            return DeviationFlag;
        }

        internal Conflictability getConflictSetting()
        {
            return this.ConflictSetting;
        }

        internal void disableAsUserDeleted()
        {
            UserDeleted = false;
        }

        virtual public void updateCreator(TilerUser user)
        {
            _Creator = user;
        }


        virtual public void setAsModified()
        {
            IsEventModified = true;
        }

        public virtual EventName getName()
        {
            return NameOfEvent;
        }

        abstract public void updateRepetitionIndex(long RepetitionIndex);

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

        virtual public DateTimeOffset End
        {
            get
            {
                return EndDateTime;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        virtual public DateTimeOffset Start
        {
            get
            {
                return StartDateTime;
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public ThirdPartyControl.CalendarTool ThirdpartyType
        {
            get 
            {
                return ThirdPartyTypeInfo;
            }
        }


        virtual public  string ThirdPartyID
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

        public string ThirdPartyUserID
        {
            get
            {
                return ThirdPartyUserIDInfo;
            }
        }

         public Location_Elements Location
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


        virtual public bool isRestricted
         {
             get
             {
                throw new NotImplementedException();
             }
         }
        public virtual bool isDeadlineElapsed
         {
             get 
             {
                 return DeadlineElapsed;
             }
         }
        public virtual string NameString
        {
            get
            {
                return NameOfEvent.NameString;
            }
        }

        public virtual bool isUserDeleted
        {
            get
            {
                return UserDeleted;
            }

        }

        public virtual int EventPriority
        {
            get
            {
                return Priority;
            }
        }

        public virtual Procrastination  ProcrastinationInfo
        {
            get
            {
                return ProfileOfProcrastination;
            }
        }



        public virtual NowProfile NowInfo
        {
            get
            {
                return ProfileOfNow;
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
        public virtual long RepetitionIndex
        {
            get
            {
                return RepetitionSequence;
            }
        }

        public virtual DateTimeOffset OrginalStartInfo
        {
            get
            {
                return OriginalStart;
            }
        }

        virtual  public string Id
        {
            get
            {
                return UniqueID.ToString();
            }
            set
            {
                throw new NotFiniteNumberException();
            }
        }

        virtual public Classification Classification
        {
            get
            {
                return Semantics;
            }
            set
            {
                Semantics = value;
            }
        }

        virtual public EventID OriginalEventId
        {
            get
            {
                return OriginalEventID;
            }
        }

        virtual public MiscData DataBlob
        {
            get
            {
                return _DataBlob;
            }
        }
        virtual public string CreatorId
        {
            get
            {
                return _CreatorId;
            }
            set
            {
                throw new NotImplementedException("You are trying to set creatorId from non storage access");
            }
        }
        public virtual TilerUser EventCreator
        {
            get
            {
                return _Creator;
            }
        }

        public bool isModified
        {
            get
            {
                return IsEventModified;
            }
        }
    }
}
