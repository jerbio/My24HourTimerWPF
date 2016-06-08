using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements.Wpf;

namespace TilerElements.DB
{
    public class DB_SubCalendarEventRestricted:SubCalendarEventRestricted, IDB_SubCalendarEventRestricted
    {
        public DB_SubCalendarEventRestricted(SubCalendarEvent mySubCalEvent, DB_RestrictionProfile restrictionData)
        {
            this.BusyFrame = mySubCalEvent.ActiveSlot;
            this.HardCalendarEventRange =mySubCalEvent.getCalendarEventRange;
            this.ProfileOfRestriction = restrictionData;
            this.OldPreferredIndex = mySubCalEvent.UniversalDayIndex;
            this.otherPartyID = mySubCalEvent.ThirdPartyID;
            this.StartDateTime = mySubCalEvent.Start;
            this.EndDateTime = mySubCalEvent.End;
            



            //this.CalendarEventRange = CalendarEventRange.CreateCopy();
            this.Complete = mySubCalEvent.isComplete;
            this.ConflictingEvents = mySubCalEvent.Conflicts;
            this.DataBlob = mySubCalEvent.Notes;
            this.DeadlineElapsed = mySubCalEvent.isDeadlineElapsed;
            this.Enabled = mySubCalEvent.isEnabled;
            
            this.EventDuration = mySubCalEvent.ActiveDuration;
            
            this.EventPreDeadline = mySubCalEvent.PreDeadline;
            this.FromRepeatEvent = mySubCalEvent.FromRepeat;
            
            
            this.Vestige = mySubCalEvent.isVestige;
            this.LocationInfo = mySubCalEvent.Location;
            this.MiscIntData = mySubCalEvent.IntData;
            //this.NonHumaneTimeLine = mySubCalEvent.NonHumaneTimeLine.CreateCopy();
            this.PrepTime = mySubCalEvent.Preparation;
            this.Priority = mySubCalEvent.EventPriority;
            this.FromRepeatEvent = mySubCalEvent.FromRepeat;
            this.RigidSchedule = mySubCalEvent.Rigid;
            

            this.UiParams = mySubCalEvent.UIParam;
            this.UniqueID = mySubCalEvent.SubEvent_ID;
            this.NameOfEvent = new EventName(this.UniqueID , mySubCalEvent.Name);
            this.UnUsableIndex = 0;
            this.UserDeleted = mySubCalEvent.isUserDeleted;
            this.UserIDs = mySubCalEvent.getAllUserIDs();
            initializeCalendarEventRange(restrictionData, this.HardCalendarEventRange);
        }

        protected DB_SubCalendarEventRestricted()
        {
            CalendarEventRange = new TimeLine();
            BusyFrame = new BusyTimeLine();
            HumaneTimeLine = new BusyTimeLine();
        }

        virtual public DateTimeOffset CalendarEnd
        {
            get
            {
                return CalendarEventRange.End;
            }

            set
            {
                if (CalendarEventRange == null)
                {
                    CalendarEventRange = new TimeLine(BusyFrame.Start, value);
                }
                else
                {
                    CalendarEventRange = new TimeLine(BusyFrame.Start, value);
                }
            }
        }

        virtual public DateTimeOffset CalendarStart
        {
            get
            {
                return CalendarEventRange.Start;
            }
            set
            {
                if (BusyFrame == null)
                {
                    CalendarEventRange = new TimeLine(value, CalendarEventRange.End);
                }
                else
                {
                    CalendarEventRange = new TimeLine(value, CalendarEventRange.End);
                }
            }
        }

        virtual public ConflictProfile conflict
        {
            get
            {
                return ConflictingEvents;
            }
            set
            {
                ConflictingEvents = value;
            }
        }

        virtual public Conflictability ConflictLevel
        {
            get
            {
                return ConflictSetting;
            }
            set
            {
                ConflictSetting = value;
            }
        }

        virtual public string CreatorId
        {
            get
            {
                return CreatorIDInfo;
            }
            set
            {
                CreatorIDInfo = value;
            }
        }

        virtual public ulong DesiredDayIndex
        {
            get
            {
                return PreferredDayIndex;
            }
            set
            {
                PreferredDayIndex = value;
            }
        }

        virtual public DateTimeOffset HumaneEnd
        {
            get
            {
                return HumaneTimeLine.End;
            }
            set
            {
                HumaneTimeLine = new BusyTimeLine(Id, HumaneTimeLine.Start, value);
            }
        }

        virtual public DateTimeOffset HumaneStart
        {
            get
            {
                return HumaneTimeLine.Start;
            }
            set
            {
                HumaneTimeLine = new BusyTimeLine(Id, value, HumaneTimeLine.End);
            }
        }

        virtual public DateTimeOffset InitializingStart
        {
            get
            {
                return OriginalStart;
            }
            set
            {
                OriginalStart = value;
            }
        }

        virtual public ulong InvalidDayIndex
        {
            get
            {
                return UnUsableIndex;
            }
            set
            {
                UnUsableIndex = value;
            }
        }

        virtual public bool isDeleted
        {
            get
            {
                return Enabled;
            }

            set
            {
                Enabled = value;
            }
        }

        virtual public bool isDeletedByUser
        {
            get
            {
                return UserDeleted;
            }
            set
            {
                UserDeleted = value;
            }
        }

        override public bool isRestricted
        {
            get
            {
                return true;
            }
        }

        virtual public bool isRepeat
        {
            get
            {
                return FromRepeatEvent;
            }

            set
            {
                FromRepeatEvent = value;
            }
        }

        virtual public bool isRigid
        {
            get
            {
                return RigidSchedule;
            }

            set
            {
                RigidSchedule = value;
            }
        }

        virtual public DateTimeOffset NonHumaneEnd
        {
            get
            {
                return NonHumaneTimeLine.End;
            }
            set
            {
                NonHumaneTimeLine = new BusyTimeLine(Id, NonHumaneTimeLine.Start, value);
            }
        }

        virtual public DateTimeOffset NonHumaneStart
        {
            get
            {
                return NonHumaneTimeLine.Start;
            }
            set
            {
                NonHumaneTimeLine = new BusyTimeLine(Id, value, NonHumaneTimeLine.End);
            }
        }

        virtual public ulong OldDayIndex
        {
            get
            {
                return OldPreferredIndex;
            }
            set
            {
                OldPreferredIndex = value;
            }
        }

        virtual public Procrastination ProcrastinationProfile
        {
            get
            {
                return ProfileOfProcrastination;
            }
            set
            {
                ProfileOfProcrastination = value;
            }
        }

        virtual public RestrictionProfile Restriction
        {
            get
            {
                return this.ProfileOfRestriction;
            }

            set
            {
                ProfileOfRestriction = value;
            }
        }

        virtual public EventDisplay UIData
        {
            get
            {
                return UiParams;
            }

            set
            {
                UiParams = value;
            }
        }

        virtual public int Urgency
        {
            get
            {
                return Priority;
            }
            set
            {
                Priority = value;
            }
        }

        virtual public ICollection<TilerUser> Users
        {
            get
            {
                return UserIDs;
            }

            set
            {
                UserIDs = value.ToList();
            }
        }

        public new bool isComplete
        {
            get
            {
                return Complete;
            }
            set
            {
                Complete = value;
            }
        }

        public new EventName Name
        {
            get
            {
                return NameOfEvent;
            }

            set
            {
                NameOfEvent = value;
            }
        }

        public new MiscData Notes
        {
            get
            {
                return DataBlob;
            }

            set
            {
                DataBlob = value;
            }
        }

        public new double Score
        {
            get
            {
                return EventScore;
            }
            set
            {
                EventScore = value;
            }
        }

        public bool isDeviated
        {
            get
            {
                return this.DeviationFlag;
            }

            set
            {
                this.DeviationFlag = value;
            }
        }

        public override TimeSpan UsedTime
        {
            get
            {
                return _UsedTime;
            }

            set
            {
                _UsedTime = value;
            }
        }
    }
}