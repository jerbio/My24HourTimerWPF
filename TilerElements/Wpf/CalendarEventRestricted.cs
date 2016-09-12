﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace TilerElements.Wpf
{
    public class CalendarEventRestricted:CalendarEventPersist
    {
        protected RestrictionProfile ProfileOfRestriction;
        

        protected CalendarEventRestricted ()
        {
            
        }

        public CalendarEventRestricted(string Name, DateTimeOffset Start, DateTimeOffset End,DateTimeOffset OriginalStartData, RestrictionProfile restrictionProfile, TimeSpan Duration, Repetition RepetitionProfile, bool isCompleted, bool isEnabled, int Divisions, bool isRigid, Location_Elements Location,TimeSpan EventPreparation,TimeSpan Event_PreDeadline, EventDisplay UiSettings = null, MiscData NoteData=null)
        {
            
            StartDateTime = Start;
            EndDateTime = End;
            RigidSchedule = isRigid;
            ProfileOfRestriction = restrictionProfile;
            Splits = Divisions;
            if (RepetitionProfile.Enable)
            {
                Splits = Divisions;
                End = RepetitionProfile.Range.End;
                TimePerSplit = new TimeSpan();
            }
            Complete = isCompleted;
            Enabled = isEnabled;
            UniqueID = EventID.GenerateCalendarEvent();
            UpdateLocationMatrix(Location);
            UiParams = UiSettings;
            _DataBlob = NoteData;
            LocationInfo = Location;
      
            NameOfEvent = new EventName( UniqueID, Name);
            EventDuration = Duration;
            TimePerSplit = TimeSpan.FromTicks(EventDuration.Ticks / Splits);
            ProfileOfNow = new NowProfile();
            OriginalStart = OriginalStartData;
            InstantiateSubEvents();
        }



        public CalendarEventRestricted(EventID EventIDEntry, string Name, DateTimeOffset Start, DateTimeOffset End, DateTimeOffset OriginalStartData, RestrictionProfile restrictionProfile, TimeSpan Duration, Repetition RepetitionProfile, bool isCompleted, bool isEnabled, int Divisions, bool isRigid, Location_Elements Location, TimeSpan EventPreparation, TimeSpan Event_PreDeadline, long RepetitionIndex, EventDisplay UiSettings = null, MiscData NoteData = null)
        {
            StartDateTime = Start;
            EndDateTime = End;
            RigidSchedule = isRigid;
            ProfileOfRestriction = restrictionProfile;
            Splits = Divisions;
            RepetitionSequence = RepetitionIndex;
            if (RepetitionProfile.Enable)
            {
                Splits = Divisions;
                End = RepetitionProfile.Range.End;
                TimePerSplit = new TimeSpan();
            }
            Complete = isCompleted;
            Enabled = isEnabled;    
            EventPreDeadline = Event_PreDeadline;
            UniqueID = EventIDEntry;
            UpdateLocationMatrix(Location);
            UiParams = UiSettings;
            _DataBlob = NoteData;
            ProfileOfNow = new NowProfile();
            EventDuration = Duration;
            TimePerSplit = TimeSpan.FromTicks(EventDuration.Ticks / Splits);
            LocationInfo = Location;
            
            OriginalStart = OriginalStartData;
            NameOfEvent = new EventName(UniqueID, Name);
            InstantiateSubEvents();
        }

        static public CalendarEventRestricted InstantiateRepeatedCandidate(string Name, DateTimeOffset Start, DateTimeOffset End, EventID CalendarEventID, RestrictionProfile restrictionProfile, TimeSpan Duration, DateTimeOffset OriginalStartData,int division, Location_Elements Location, EventDisplay UiSettings, bool RigidFlag, TimeSpan preparation, string thirdPartyID, long NextSequence, ConcurrentDictionary<DateTimeOffset, CalendarEvent> OrginalStartToCalendarEvent,CalendarEvent RepeatRootData)
        { 
            CalendarEventRestricted retValue = new CalendarEventRestricted();
            retValue.UniqueID = EventID.GenerateRepeatCalendarEvent(CalendarEventID.ToString(), NextSequence);
            
            retValue.EventDuration = Duration;
            retValue.StartDateTime = Start;
            retValue.EndDateTime = End;
            retValue.ProfileOfRestriction = restrictionProfile;
            retValue.Complete = false;
            retValue.Enabled = true;
            retValue.DeadlineElapsed = End > TilerEvent.EventNow;
            retValue.Splits = division;
            retValue.TimePerSplit = TimeSpan.FromTicks( retValue.EventDuration.Ticks / division);
            retValue.LocationInfo = Location;
            retValue.EndOfCalculation = End < TilerEvent.EventNow.Add(CalculationEndSpan) ? End : TilerEvent.EventNow.Add(CalculationEndSpan);
            retValue.RigidSchedule = RigidFlag;
            retValue.Priority = 0;
            retValue.otherPartyID = thirdPartyID;
            retValue.UiParams = UiSettings;
            retValue.PrepTime = preparation;
            
            retValue.UpdateLocationMatrix(Location);
            retValue.InstantiateSubEvents();
            retValue.OriginalStart = OriginalStartData;
            retValue.RootOfRepeat = RepeatRootData;
            retValue.NameOfEvent =new EventName( retValue.Id, Name);

            while (!OrginalStartToCalendarEvent.TryAdd(OriginalStartData, retValue))
            {
                Thread.Sleep(10);
            }
            
            return retValue;
        }

        public override CalendarEvent createCopy(EventID eventId = null)
        {
            CalendarEventRestricted MyCalendarEventCopy = new CalendarEventRestricted();
            MyCalendarEventCopy.EventDuration = new TimeSpan(EventDuration.Ticks);
            MyCalendarEventCopy.NameOfEvent = NameOfEvent.CreateCopy();
            MyCalendarEventCopy.StartDateTime = StartDateTime;
            MyCalendarEventCopy.EndDateTime = EndDateTime;
            MyCalendarEventCopy.EventPreDeadline = new TimeSpan(EventPreDeadline.Ticks);
            MyCalendarEventCopy.PrepTime = new TimeSpan(PrepTime.Ticks);
            MyCalendarEventCopy.Priority = Priority;
            //MyCalendarEventCopy.RepetitionFlag = RepetitionFlag;
            MyCalendarEventCopy.EventRepetition = EventRepetition.CreateCopy();// EventRepetition != null ? EventRepetition.CreateCopy() : EventRepetition;
            MyCalendarEventCopy.Complete = this.Complete;
            MyCalendarEventCopy.RigidSchedule = RigidSchedule;//hack
            MyCalendarEventCopy.Splits = Splits;
            MyCalendarEventCopy.TimePerSplit = new TimeSpan(TimePerSplit.Ticks);
            MyCalendarEventCopy.EventSequence = EventSequence.CreateCopy();
            MyCalendarEventCopy.SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            MyCalendarEventCopy.UiParams = this.UiParams.createCopy();
            MyCalendarEventCopy._DataBlob = this._DataBlob.createCopy();
            MyCalendarEventCopy.Enabled = this.Enabled;
            MyCalendarEventCopy.LocationInfo = LocationInfo;//hack you might need to make copy
            MyCalendarEventCopy.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            MyCalendarEventCopy.DeadlineElapsed = this.DeadlineElapsed;
            MyCalendarEventCopy.UserDeleted = this.UserDeleted;
            MyCalendarEventCopy.CompletedCount = this.CompletedCount;
            MyCalendarEventCopy.DeletedCount = this.DeletedCount;
            MyCalendarEventCopy.ProfileOfRestriction = this.ProfileOfRestriction.createCopy();
            MyCalendarEventCopy.ProfileOfNow = this.ProfileOfNow.CreateCopy();
            MyCalendarEventCopy.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            MyCalendarEventCopy.Semantics = this.Semantics.createCopy();
            MyCalendarEventCopy._UsedTime = this._UsedTime;
            if (eventId != null)
            {
                MyCalendarEventCopy.UniqueID = eventId;
            }
            else
            {
                MyCalendarEventCopy.UniqueID = UniqueID;//hack
            }
            int counter = 0;
            foreach (SubCalendarEventRestricted eachSubCalendarEvent in this.SubEvents.Values)
            {
                MyCalendarEventCopy.SubEvents.Add(eachSubCalendarEvent.SubEvent_ID, eachSubCalendarEvent.createCopy(EventID.GenerateSubCalendarEvent(MyCalendarEventCopy.UniqueID, ++counter) ));
            }

            //MyCalendarEventCopy.SchedulStatus = SchedulStatus;
            MyCalendarEventCopy.otherPartyID = otherPartyID == null ? null : otherPartyID.ToString();
            MyCalendarEventCopy.UserIDs = this.UserIDs.ToList();
            MyCalendarEventCopy._Creator = this._Creator;
            return MyCalendarEventCopy;
            
            //return base.createCopy();
        }

        void InstantiateSubEvents()
        {
            for (int i = 0; i < Splits; i++)
            {
                DateTimeOffset SubStart = this.Start;
                DateTimeOffset SubEnd = Start.Add(TimePerSplit);
                SubCalendarEventRestricted newEvent = new SubCalendarEventRestricted(UniqueID.ToString(),i+1, SubStart, SubEnd, ProfileOfRestriction, this.RangeTimeLine, true, false, new ConflictProfile(), RigidSchedule, PrepTime, EventPreDeadline, LocationInfo, UiParams, _DataBlob, Priority, DeadlineElapsed, ThirdPartyID);
                SubEvents.Add(newEvent.SubEvent_ID, newEvent);
            }
            
        }

        public RestrictionProfile RetrictionInfo
        {
            get
            {
                return ProfileOfRestriction;
            }
        }
        protected override CalendarEvent getCalculationCopy()
        {
            CalendarEventRestricted RetValue = new CalendarEventRestricted();
            RetValue.EventDuration = this.Duration;
            RetValue.NameOfEvent = this.NameOfEvent.CreateCopy();
            RetValue.StartDateTime = this.Start;
            RetValue.EndDateTime = this.End;
            RetValue.EventPreDeadline = this.PreDeadline;
            RetValue.PrepTime = this.Preparation;
            RetValue.Priority = this.EventPriority;
            //RetValue.RepetitionFlag = this.RepetitionStatus;
            RetValue.EventRepetition = this.Repeat;// EventRepetition != this.null ? EventRepetition.CreateCopy() : EventRepetition;
            RetValue.Complete = this.isComplete;
            RetValue.RigidSchedule = this.Rigid;//hack
            RetValue.Splits = this.NumberOfSplit;
            RetValue.TimePerSplit = this.EachSplitTimeSpan;
            RetValue.UniqueID = EventID.GenerateCalendarEvent();
            //RetValue.EventSequence = this.EventSequence;
            RetValue.SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            RetValue.UiParams = this.UIParam.createCopy();
            RetValue._DataBlob = this.DataBlob;
            RetValue.Enabled = this.isEnabled;
            RetValue.LocationInfo = this.Location.CreateCopy();//hack you might need to make copy
            RetValue.ProfileOfProcrastination = this.ProcrastinationInfo.CreateCopy();
            RetValue.DeadlineElapsed = this.isDeadlineElapsed;
            RetValue.UserDeleted = this.isUserDeleted;
            RetValue.CompletedCount = this.CompletionCount;
            RetValue.DeletedCount = this.DeletionCount;
            RetValue.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            RetValue.ProfileOfNow = this.ProfileOfNow.CreateCopy();
            RetValue.otherPartyID = this.ThirdPartyID;// == this.null ? null : otherPartyID.ToString();
            RetValue.UserIDs = this.getAllUsers();//.ToList();
            RetValue.ProfileOfNow = this.ProfileOfNow.CreateCopy();
            RetValue.ProfileOfRestriction = this.ProfileOfRestriction.createCopy();
            RetValue.UpdateLocationMatrix(RetValue.LocationInfo);
            return RetValue;
        }

        public override void UpdateThis(CalendarEvent CalendarEventEntry)
        {
            if ((this.Id == CalendarEventEntry.Id))
            {
                base.UpdateThis(CalendarEventEntry);
                CalendarEventRestricted castedEvent = CalendarEventEntry as CalendarEventRestricted;
                if (castedEvent != null)
                {
                    this.ProfileOfRestriction = castedEvent.ProfileOfRestriction;
                }
                return;
            }

            throw new Exception("Invalid Calendar ID used in Update Calendar Event");
        }



        public override CalendarEventPersist ConvertToPersistable()
        {
            DB.DB_CalendarEventRestricted RetValue = new DB.DB_CalendarEventRestricted()
            {
                DeviationFlag = this.getDeviationFlag(),
                CalculationEnd = this.getCalculationEnd(),
                Complete = this.isComplete,
                DeletedCount = this.DeletionCount,
                ConflictSetting = this.getConflictSetting(),
                DeadlineElapsed = this.isDeadlineElapsed,
                Enabled = this.isEnabled,
                EndDateTime = this.End,
                EventDuration = this.Duration,
                EventPreDeadline = this.PreDeadline,
                EventRepetition = this.Repeat,
                CompletedCount = this.CompletionCount,
                FromRepeatEvent = this.FromRepeat,
                InitializingTimeSpanPerSplit = this.getInitializingTimeSpanPerSplit(),
                InitializingStart = this.getInitializingStart(),
                isDeleted = this.isEnabled,
                isDeletedByUser = this.isUserDeleted,
                RigidSchedule = this.Rigid,
                LastNowProfile = this.NowInfo,
                SplitCount = this.NumberOfSplit,
                TimePerSplit = this.EachSplitTimeSpan,
                Urgency = this.EventPriority,
                Users = this.getAllUsers(),
                Notes = this.DataBlob,
                RepeatRoot = this.Repeat.getRootCalendarEvent(),
                StartTime = this.Start,
                EndTime = this.End,
                isCalculableInitialized = this.isCalculableInitialized,
                isUnDesignableInitialized = this.isUnDesignableInitialized,
                ProfileOfRestriction = this.ProfileOfRestriction,
                IsEventModified = this.IsEventModified,
                ProfileOfNow = this.NowInfo.isInitialized ? DB.DB_NowProfile.ConvertToPersistable(this.NowInfo) : null,
                ProfileOfProcrastination = this.ProcrastinationInfo.IsInitialized() ? DB.DB_Procrastination.ConvertToPersistable(this.ProcrastinationInfo) : null,
                LocationInfo = DB.DB_LocationElements.ConvertToPersistable(this.LocationInfo, this.CreatorId),
                Name = DB.DB_EventName.ConvertToPersistable(this.getName()),
                _DataBlob = DB.DB_MiscData.ConvertToPersistable(this._DataBlob),
                Classification = DB.DB_Classification.ConvertToPersistable(this.Classification),
                UIData = DB.DB_EventDisplay.ConvertToPersistable(this.UIParam)
            };
            RetValue.SubEvents = this.getConvertToPersistedSubEvents(RetValue.Location, RetValue.NowInfo, RetValue.ProcrastinationInfo, RetValue.Name, RetValue.DataBlob, RetValue.Classification, RetValue.UIData);
            return RetValue;
        }

    }
}
