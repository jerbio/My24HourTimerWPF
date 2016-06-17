using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements.Wpf;

namespace TilerElements.DB
{
    public class DB_CalendarEventRestricted : CalendarEventRestricted//, IRestrictedEvent, IDB_CalendarEvent
    {
        public DB_CalendarEventRestricted()
        {
            
        }
        public DB_CalendarEventRestricted(CalendarEvent CalendarEventData, RestrictionProfile restrictionData)
        {
            //CalendarEventRestricted MyCalendarEventCopy = CalendarEventData.new CalendarEventRestricted();
            this.EventDuration = CalendarEventData.Duration;
            
            this.StartDateTime = CalendarEventData.Start;
            this.EndDateTime = CalendarEventData.End;
            this.EventPreDeadline = CalendarEventData.PreDeadline;
            this.PrepTime = CalendarEventData.Preparation;
            this.Priority = CalendarEventData.EventPriority;
            //this.RepetitionFlag = CalendarEventData.RepetitionStatus;
            this.EventRepetition = (CalendarEventData).Repeat;// EventRepetition != CalendarEventData.null ? EventRepetition.CreateCopy() : EventRepetition;
            this.Complete = CalendarEventData.isComplete;
            this.RigidSchedule = CalendarEventData.Rigid;//hack
            this.Splits = CalendarEventData.NumberOfSplit;
            this.TimePerSplit = CalendarEventData.EachSplitTimeSpan;
            this.UniqueID = CalendarEventData.Calendar_EventID;//hack
            //this.EventSequence = CalendarEventData.EventSequence;
            this.SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            this.UiParams = CalendarEventData.UIParam;
            this._DataBlob = CalendarEventData.DataBlob;
            this.Enabled = CalendarEventData.isEnabled;
            //this.isRestricted = CalendarEventData.isEventRestricted;
            this.LocationInfo= CalendarEventData.Location;//hack you might need to make copy
            this.ProfileOfProcrastination = CalendarEventData.ProcrastinationInfo;
            this.DeadlineElapsed = CalendarEventData.isDeadlineElapsed;
            this.UserDeleted = CalendarEventData.isUserDeleted;
            this.CompletedCount = CalendarEventData.CompletionCount;
            this.DeletedCount = CalendarEventData.DeletionCount;
            this.ProfileOfRestriction = restrictionData;
            //this.SubEvents = ((DB_CalendarEventRestricted)CalendarEventData).getSubEvents();
            this.NameOfEvent = new EventName(this.UniqueID, CalendarEventData.NameString);
            if (!this.EventRepetition.Enable)
            {
                foreach (SubCalendarEventRestricted eachSubCalendarEvent in CalendarEventData.AllSubEvents)
                {
                    this.SubEvents.Add(eachSubCalendarEvent.SubEvent_ID, eachSubCalendarEvent);
                }
            }
            this.otherPartyID = CalendarEventData.ThirdPartyID;// == CalendarEventData.null ? null : otherPartyID.ToString();
            this.UserIDs = CalendarEventData.getAllUsers();//.ToList();
            //return MyCalendarEventCopy;
        }

        public override CalendarEventPersist ConvertToPersistable()
        {
            return this;
        }

        //public RestrictionProfile Restriction
        //{
        //    get
        //    {
        //        return this.ProfileOfRestriction;
        //    }

        //    set
        //    {
        //        ProfileOfRestriction = value;
        //    }
        //}

        //public int CompleteCount
        //{
        //    get
        //    {
        //        return this.CompletedCount;
        //    }

        //    set
        //    {
        //        this.CompletedCount = value;
        //    }
        //}

        //public int DeleteCount
        //{
        //    get
        //    {
        //        return this.DeletedCount;
        //    }

        //    set
        //    {
        //        this.DeletedCount = value;
        //    }
        //}

        //public DateTimeOffset CalculationEnd
        //{
        //    get
        //    {
        //        return this.EndOfCalculation;
        //    }

        //    set
        //    {
        //        this.EndOfCalculation = value;
        //    }
        //}

        //public Repetition EventRepetition
        //{
        //    get
        //    {
        //        return Repeat;
        //    }

        //    set
        //    {
        //        this.EventRepetition = value;
        //    }
        //}

        //public NowProfile NowProfile
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public int SplitCount
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public CalendarEvent RepeatRoot
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public TimeSpan TimeSpanPerSplit
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public TimeSpan OriginalTimeSpanPerSplit
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public ICollection<SubCalendarEvent> SubCalendarEvents
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public DateTimeOffset InitializingStart
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //DateTimeOffset ITilerEvent.Start
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //DateTimeOffset ITilerEvent.End
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public int Urgency
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool isDeletedByUser
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool isRigid
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool isDeleted
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //bool ITilerEvent.isComplete
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool isRepeat
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public Procrastination ProcrastinationProfile
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //MiscData ITilerEvent.Notes
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public ICollection<TilerUser> Users
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public EventDisplay UIData
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public string CreatorId
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //EventName ITilerEvent.Name
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public bool isDeviated
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
    }
}