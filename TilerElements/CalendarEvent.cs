#define SetDefaultPreptimeToZero
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace TilerElements
{
    public class CalendarEvent : TilerEvent, IDefinedRange, IUndoable
    {
        protected DateTimeOffset EndOfCalculation = DateTime.Now.AddMonths(3);
        protected int _Splits;
        protected TimeSpan _AverageTimePerSplit;
        protected int _CompletedCount;
        protected int _DeletedCount;
        protected int _AutoDeletedCount = 0;
        protected SubEventDictionary<string, SubCalendarEvent> _SubEvents;

        CustomErrors CalendarError = null;
        protected TimeLine EventSequence;
        protected HashSet<SubCalendarEvent> CalculableSubEvents = new HashSet<SubCalendarEvent>();
        protected HashSet<SubCalendarEvent> UnDesignables = new HashSet<SubCalendarEvent>();
        protected bool _isCalculableInitialized = false;
        protected bool isUnDesignableInitialized = false;
        protected Dictionary<ulong, DayTimeLine> CalculationLimitationWithUnUsables;
        protected Dictionary<ulong, DayTimeLine> CalculationLimitation;
        protected Dictionary<ulong, DayTimeLine> FreeDaysLimitation;// Holds days that do not contain subevents within this time line
        protected HashSet<ulong> DaysWithSubEvents = new HashSet<ulong>();
        protected List<SubCalendarEvent> _RemovedSubEvents = new List<SubCalendarEvent>();
        protected EventPreference _EventDayPreference;
        protected string _LastCompletionTime;
        protected CalendarEvent _DefaultCalendarEvent;
        DateTimeOffset[] completionDates = new DateTimeOffset[0];
        HashSet<ulong> completeDayIndexes = new HashSet<ulong>();
        protected TimeLine _CalculationStartToEnd;
        #region undoMembers
        public int UndoSplits;
        public TimeSpan UndoAverageTimePerSplit;
        public int UndoCompletedCount;
        public int UndoDeletedCount;
        public int UndoAutoDeletedCount;
        public string UndoLastCompletionTime;
        #endregion

        public override void undoUpdate(Undo undo)
        {
            UndoSplits = _Splits;
            UndoAverageTimePerSplit = _AverageTimePerSplit;
            UndoCompletedCount = _CompletedCount;
            UndoDeletedCount = _DeletedCount;
            UndoAutoDeletedCount = _AutoDeletedCount;
            base.undoUpdate(undo);
        }

        public override void undo(string undoId)
        {
            if (UndoId == undoId)
            {
                Utility.Swap(ref UndoSplits, ref _Splits);
                Utility.Swap(ref UndoAverageTimePerSplit, ref _AverageTimePerSplit);
                Utility.Swap(ref UndoCompletedCount, ref _CompletedCount);
                Utility.Swap(ref UndoDeletedCount, ref _DeletedCount);
                Utility.Swap(ref UndoAutoDeletedCount, ref _AutoDeletedCount);
                Utility.Swap(ref UndoLastCompletionTime, ref _LastCompletionTime);
            }
            base.undo(undoId);
        }

        public override void redo(string undoId)
        {
            if (UndoId == undoId)
            {
                Utility.Swap(ref UndoSplits, ref _Splits);
                Utility.Swap(ref UndoAverageTimePerSplit, ref _AverageTimePerSplit);
                Utility.Swap(ref UndoCompletedCount, ref _CompletedCount);
                Utility.Swap(ref UndoDeletedCount, ref _DeletedCount);
                Utility.Swap(ref UndoAutoDeletedCount, ref _AutoDeletedCount);
                Utility.Swap(ref UndoLastCompletionTime, ref _LastCompletionTime);
            }
            base.redo(undoId);
        }
        #region Constructor

        protected CalendarEvent()
        {
            EventSequence = new TimeLine();
        }
        public CalendarEvent(bool initialize = true)
        {
            if (initialize)
            {
                this.initialize();
            }
        }

        public CalendarEvent(CustomErrors Error) : this()
        {
            CalendarError = Error;
        }

        protected CalendarEvent(
            EventName name,
            DateTimeOffset start,
            DateTimeOffset end,
            TimeSpan duration,
            TimeSpan prepTime,
            TimeSpan preDeadline,
            int split,
            Repetition repetition,
            EventDisplay displayData,
            MiscData miscData, bool isEnabled, bool completeflag, NowProfile nowProfile, Procrastination procrastinationProfile,
            Location location, TilerUser creator, TilerUserGroup otherUsers, bool userDeleted, DateTimeOffset timeOfCreation, string timeZoneOrigin, Classification semantics)
        {
            if (end < start)
            {
                throw new Exception("Calendar Event cannot have an end time earlier than the start time");
            }

            this.updateStartTime(start);
            this.updateEndTime( end);
            this._Splits = split;
            this._Creator = creator;
            this._DataBlob = miscData;
            this.UniqueID = EventID.GenerateCalendarEvent();
            this._EventDuration = duration;
            this._Users = otherUsers;
            this._EventPreDeadline = preDeadline;
            this._PrepTime = prepTime;
            this._ProfileOfNow = nowProfile ?? new NowProfile();
            this._ProfileOfProcrastination = procrastinationProfile ?? new Procrastination(new DateTimeOffset(), new TimeSpan());
            this._EventRepetition = repetition;
            this._Complete = completeflag;
            this._Enabled = isEnabled;
            this._LocationInfo = location;
            this._UiParams = displayData;
            this._Name = name;
            this._AutoDeleted = userDeleted;
            this._TimeZone = timeZoneOrigin;
            this.TimeCreated = DateTimeOffset.UtcNow;
            this._Semantics = semantics ?? new Classification(this);
            if(this.Location_DB!=null) {
                _LocationInfo.User = this.getCreator;
            }
            
            _EventDayPreference = new EventPreference();
        }

        public CalendarEvent(
            EventName NameEntry,
            DateTimeOffset StartData,
            DateTimeOffset EndData,
            TimeSpan eventDuration,
            TimeSpan eventPrepTimeSpan,
            TimeSpan preDeadlineTimeSpan,
            int eventSplit,
            Repetition EventRepetitionEntry,
            Location EventLocation,
            EventDisplay UiData,
            MiscData NoteData,
            Procrastination procrastination,
            NowProfile nowProfile,
            bool EnabledEventFlag,
            bool CompletionFlag,
            TilerUser creator,
            TilerUserGroup users,
            string timeZone,
            EventID eventId,
            bool initializeSubCalendarEvents = true,
            Classification semantics = null)
            : this(
                 NameEntry, StartData, EndData, eventDuration, eventPrepTimeSpan, preDeadlineTimeSpan, eventSplit, EventRepetitionEntry, UiData, NoteData, EnabledEventFlag, CompletionFlag, nowProfile, procrastination, EventLocation, creator, users, false, DateTimeOffset.UtcNow, timeZone, semantics)
        {
            UniqueID = eventId ?? this.UniqueID; /// already initialized by parent initialization
            if (_Splits != 0)
            {
                _AverageTimePerSplit = TimeSpan.FromTicks(((eventDuration.Ticks / _Splits)));
            } else
            {
                _AverageTimePerSplit = new TimeSpan();
            }

            this._EventDuration = eventDuration;
            if (initializeSubCalendarEvents)
            {
                initializeSubEvents();
            }
            EventSequence = new TimeLine(Start, End);
            //UpdateLocationMatrix(LocationInfo);
        }

        virtual public void initializeSubEvents()
        {
            _SubEvents = new SubEventDictionary<string, SubCalendarEvent>();

            for (int i = 0; i < _Splits; i++)// This is still is still called when dealing with repeat events. Meaning repeat calendar events all have an unnecessary extra subevent
            {
                SubCalendarEvent newSubCalEvent = new SubCalendarEvent(this, getCreator, _Users, _TimeZone, _AverageTimePerSplit, this.getName, (End - _AverageTimePerSplit), this.End, new TimeSpan(), UniqueID.ToString(), _RigidSchedule, this._Enabled, this._UiParams, this.Notes, this._Complete, this._LocationInfo, this.StartToEnd);
                newSubCalEvent.TimeCreated = this.TimeCreated;
                _SubEvents.Add(newSubCalEvent.Id, newSubCalEvent);
            }
        }

        public CalendarEvent(CalendarEvent MyUpdated, SubCalendarEvent[] MySubEvents)
        {
            _Name = MyUpdated._Name;
            UniqueID = MyUpdated.UniqueID;
            updateStartTime(MyUpdated.Start);
            updateEndTime( MyUpdated.End);
            EventSequence = new TimeLine(Start, End);
            _EventDuration = MyUpdated.getActiveDuration;
            _Splits = MyUpdated._Splits;
            _PrepTime = MyUpdated._PrepTime;
            _EventPreDeadline = MyUpdated.getPreDeadline;
            _RigidSchedule = MyUpdated._RigidSchedule;
            _userLocked = MyUpdated.userLocked;
            _AverageTimePerSplit = MyUpdated._AverageTimePerSplit;
            _Enabled = MyUpdated.isEnabled;
            _Complete = MyUpdated.getIsComplete;
            _SubEvents = new SubEventDictionary<string, SubCalendarEvent>();
            for (int i = 0; i < MySubEvents.Length; i++)//using MySubEvents.length for the scenario of the call for repeat event. Remember the parent event does not generate subevents
            {
                SubCalendarEvent newSubCalEvent = MySubEvents[i];
                newSubCalEvent.ParentCalendarEvent = this;
                newSubCalEvent.TimeCreated = this.TimeCreated;
                if (_SubEvents.ContainsKey(newSubCalEvent.Id))
                {
                    _SubEvents[newSubCalEvent.Id] = newSubCalEvent;
                }
                else
                {
                    _SubEvents.Add(newSubCalEvent.Id, newSubCalEvent);
                }
            }

            this._Priority = MyUpdated.getEventPriority;
            this._UiParams = MyUpdated.getUIParam;
            this._DataBlob = MyUpdated.Notes;
            this.isRestricted = MyUpdated.getIsEventRestricted;
            this._LocationInfo = MyUpdated.Location;//hack you might need to make copy
            this._ProfileOfProcrastination = MyUpdated.getProcrastinationInfo;
            this._AutoDeleted = MyUpdated.getIsUserDeleted;
            this._CompletedCount = MyUpdated.CompletionCount;
            this._DeletedCount = MyUpdated.DeletionCount;
            _EventRepetition = MyUpdated.Repeat;
            _ProfileOfNow = MyUpdated.getNowInfo;
            EventSequence = new TimeLine(Start, End);
            _Creator = MyUpdated.getCreator;
            _UsedTime = MyUpdated.UsedTime;
            _Users = MyUpdated._Users;
            _TimeZone = MyUpdated._TimeZone;
            _isProcrastinateEvent = MyUpdated._isProcrastinateEvent;
            ThirdPartyTypeInfo = MyUpdated.ThirdPartyTypeInfo;
            _EventDayPreference = MyUpdated._EventDayPreference;
        }
        #endregion

        #region Functions

        #region IwhyImplementation
        override public IWhy Because()
        {
            throw new NotImplementedException("Yet to implement a because functionality for subcalendar event");
        }

        override public IWhy OtherWise()
        {
            throw new NotImplementedException("Yet to implement a OtherWise functionality for subcalendar event");
        }

        override public IWhy WhatIf(params Reason[] reasons)
        {
            throw new NotImplementedException("Yet to implement a OtherWise functionality for subcalendar event");
        }

        virtual public TempTilerEventChanges prepForWhatIfDifferentDay(TimeLine timeLine, EventID eventId)
        {
            CalendarEvent calEvent;
            TempTilerEventChanges retvalue = new TempTilerEventChanges();
            if (base.IsRepeat)
            {
                calEvent = Repeat.getCalendarEvent(eventId.ToString());
            } else
            {
                calEvent = this;
            }

            SubCalendarEvent subEvent = getSubEvent(eventId);
            subEvent.TempChanges.allChanges.Add(subEvent);
            //calEventCpy = calEvent.createCopy(EventID.GenerateRepeatCalendarEvent( calEvent.Calendar_EventID.ToString()));


            subEvent.TempChanges.allChanges.Add(subEvent);
            TimeLine subEventActiveTime = subEvent.StartToEnd;
            TimeSpan dayDiffSpan = timeLine.Start.Date - subEventActiveTime.Start.Date;
            TimeLine subEvenRangeReadjustedToexpectedTimeLine = new TimeLine(subEventActiveTime.Start.Add(dayDiffSpan), subEventActiveTime.End.Add(dayDiffSpan));
            calEvent._EventRepetition = new Repetition(timeLine, Repetition.Frequency.YEARLY, subEvenRangeReadjustedToexpectedTimeLine);
            calEvent._EventRepetition.PopulateRepetitionParameters(calEvent);
            CalendarEvent calEventCpy = calEvent.Repeat.RecurringCalendarEvents().Single();// using ssingle because this must always return a single calendarevent. Because we generated a repeat event which should only have one calendar event;
            SubCalendarEvent subEventCopy = calEventCpy.AllSubEvents.First();
            SubCalendarEvent duplicateOfOriginal = subEvent.CreateCopy(subEventCopy.SubEvent_ID);

            subEventCopy.UpdateThis(duplicateOfOriginal);
            //calEventCpy.SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            calEventCpy.updateStartTime(timeLine.Start);
            calEventCpy.updateEndTime( timeLine.End);
            subEventCopy.updateCalculationEventRange(timeLine);
            if (subEventCopy.isLocked)/// this is optimized for this use case
            {
                DateTimeOffset dayStart = timeLine.Start;
                DateTimeOffset preferredStart = new DateTimeOffset(dayStart.Year, dayStart.Month, dayStart.Day, subEventCopy.Start.Hour, subEventCopy.Start.Minute, subEventCopy.Start.Second, new TimeSpan());
                if (preferredStart < dayStart)
                {
                    preferredStart = preferredStart.AddDays(1);
                }
                subEventCopy.shiftEvent(preferredStart, true);
            }

            //calEventCpy.SubEvents.Add(subEventCopy.SubEvent_ID, subEventCopy);
            subEvent.TempChanges.allChanges.Add(subEvent);
            subEvent.TempChanges.allChanges.Add(subEventCopy);
            calEvent.TempChanges.allChanges.Add(subEvent);
            calEvent.TempChanges.allChanges.Add(subEventCopy);
            retvalue.allChanges.Add(calEvent);
            subEvent.disable(calEvent);
            return retvalue;
        }

        virtual public TempTilerEventChanges prepForWhatIfDifferentStartTime(DateTimeOffset startTime, EventID eventId)
        {
            CalendarEvent calEvent;
            TempTilerEventChanges retvalue = new TempTilerEventChanges();
            if (base.IsRepeat)
            {
                calEvent = Repeat.getCalendarEvent(eventId.ToString());
            }
            else
            {
                calEvent = this;
            }

            SubCalendarEvent subEvent = getSubEvent(eventId);
            subEvent.TempChanges.allChanges.Add(subEvent);
            TimeLine timeLine = new TimeLine(startTime, calEvent.StartToEnd.End);
            if (timeLine.TimelineSpan >= subEvent.RangeSpan)
            {
                TimeLine subEventActiveTime = subEvent.StartToEnd;
                TimeLine subEvenRangeReadjustedToexpectedTimeLine = new TimeLine(
                    startTime,
                    subEventActiveTime.End);
                calEvent._EventRepetition = new Repetition(
                    timeLine,
                    Repetition.Frequency.YEARLY,
                    subEvenRangeReadjustedToexpectedTimeLine);
                calEvent._EventRepetition.PopulateRepetitionParameters(calEvent);
                CalendarEvent calEventCpy = calEvent.Repeat.RecurringCalendarEvents().Single();// using ssingle because this must always return a single calendarevent. Because we generated a repeat event which should only have one calendar event;
                SubCalendarEvent subEventCopy = calEventCpy.AllSubEvents.First();
                SubCalendarEvent duplicateOfOriginal = subEvent.CreateCopy(subEventCopy.SubEvent_ID);

                subEventCopy.UpdateThis(duplicateOfOriginal);

                calEventCpy.updateStartTime(timeLine.Start);
                calEventCpy.updateEndTime( timeLine.End);
                subEventCopy.updateCalculationEventRange(timeLine);
                if (subEventCopy.isLocked)/// this is optimized for this use case
                {
                    DateTimeOffset dayStart = timeLine.Start;
                    DateTimeOffset preferredStart = new DateTimeOffset(dayStart.Year, dayStart.Month, dayStart.Day, subEventCopy.Start.Hour, subEventCopy.Start.Minute, subEventCopy.Start.Second, new TimeSpan());
                    if (preferredStart < dayStart)
                    {
                        preferredStart = preferredStart.AddDays(1);
                    }
                    subEventCopy.shiftEvent(preferredStart, true);
                }

                subEvent.TempChanges.allChanges.Add(subEvent);
                subEvent.TempChanges.allChanges.Add(subEventCopy);
                calEvent.TempChanges.allChanges.Add(subEvent);
                calEvent.TempChanges.allChanges.Add(subEventCopy);
                retvalue.allChanges.Add(calEvent);
                subEvent.disable(calEvent);
                return retvalue;
            }
            else
            {
                throw new CustomErrors("Cannot fit " + subEvent.getName.NameValue + "within the timeframe after " + startTime.ToString(), (int)CustomErrors.Errors.cannotFitWithinTimeline);
            }

        }

        virtual public void ReverseWhatIf(TempTilerEventChanges toBeReverted)
        {
            CalendarEvent calEvent = toBeReverted.allChanges[0] as CalendarEvent;
            calEvent._EventRepetition = new Repetition();
            SubCalendarEvent subEventIni = calEvent.TempChanges.allChanges[0] as SubCalendarEvent;
            SubCalendarEvent subEventCopy = calEvent.TempChanges.allChanges[1] as SubCalendarEvent;
            subEventIni.Enable(calEvent);
            //subEventIni.shiftEvent(subEventCopy.Start);
        }
        #endregion

        public void updateprocrastinationtree(Procrastination procrastination)
        {
            if (procrastination != null && !procrastination.isNull && string.IsNullOrEmpty(this.ProcrastinationId))
            {
                this.Procrastination_EventDB = null;
                //this.ProcrastinationId = null;
                this.Procrastination_EventDB = procrastination;
                //this.ProcrastinationId = procrastination.Id;
                foreach (SubCalendarEvent subEVent in this.AllSubEvents)
                {
                    subEVent.updateprocrastinationtree(procrastination);
                }
                if (this.IsRepeat)
                {
                    foreach (CalendarEvent cal in this.Repeat.RecurringCalendarEvents())
                    {
                        cal.updateprocrastinationtree(procrastination);
                    }
                }

            }
        }

        public void updatenowprofiletree(NowProfile nowProfile)
        {
            if (nowProfile != null && string.IsNullOrEmpty(this.NowProfileId))
            {
                //this.NowProfileId = null;
                this.ProfileOfNow_EventDB = null;
                //this.NowProfileId = nowProfile.Id;
                this.ProfileOfNow_EventDB = nowProfile;
                if (nowProfile.AssociatedEvent==null)
                {
                    nowProfile.AssociatedEvent = this;
                }
                foreach (SubCalendarEvent subEVent in this.AllSubEvents)
                {
                    subEVent.updatenowprofiletree(nowProfile);
                }

                if (this.IsRepeat)
                {
                    foreach (CalendarEvent cal in this.Repeat.RecurringCalendarEvents())
                    {
                        cal.updatenowprofiletree(nowProfile);
                    }
                }

            }
        }

        protected void initialize()
        {
            _EventDuration = new TimeSpan();

            updateStartTime(new DateTimeOffset());
            updateEndTime( new DateTimeOffset());
            _EventPreDeadline = new TimeSpan();
            _PrepTime = new TimeSpan();
            _Priority = 0;
            _EventRepetition = new Repetition();
            _RigidSchedule = false;
            _Splits = 1;
            _LocationInfo = new Location();
            UniqueID = EventID.GenerateCalendarEvent();
            _SubEvents = new SubEventDictionary<string, SubCalendarEvent>();
            _otherPartyID = "";
            CalendarError = null;
            EventSequence = new TimeLine();
            _ProfileOfProcrastination = new Procrastination(new DateTimeOffset(), new TimeSpan());
            _ProfileOfNow = new NowProfile();
            _Name = new EventName(_Creator, this);
            this.TimeCreated = DateTimeOffset.UtcNow;
            _Semantics = new Classification(this);
            if (this.Location_DB != null)
            {
                _LocationInfo.User = this.getCreator;
            }
            _EventDayPreference = new EventPreference();
        }

        /// <summary>
        /// Calendarevent Identifies the subeevents that are to be used for the calculation of a schedule. You need to initialize this for calculation of schedule
        /// </summary>
        public void initializeCalculables()
        {
            CalculableSubEvents = new HashSet<SubCalendarEvent>(ActiveSubEvents.Where(obj => obj.isInCalculationMode));
            _isCalculableInitialized = true;
        }

        public void initializeUndesignables()
        {
            if (_isCalculableInitialized)
            {
                UnDesignables = new HashSet<SubCalendarEvent>((CalculableSubEvents.Where(obj => !obj.isDesignated)));
                isUnDesignableInitialized = true;
                return;
            }
            throw new Exception("You haven't initialized calculables");
        }

        public void initializeCalculablesAndUndesignables()
        {
            initializeCalculables();
            initializeUndesignables();
        }


        ///*
        public void updateProcrastinate(Procrastination ProcrastinationTime)
        {
            _ProfileOfProcrastination.Update(ProcrastinationTime);
            _ProfileOfNow.reset();
        }
        //*/

        protected Dictionary<string, SubCalendarEvent> getSubEvents()
        {
            return _SubEvents.Collection;
        }

        virtual public CalendarEvent createCopy(EventID Id = null)
        {
            CalendarEvent MyCalendarEventCopy = new CalendarEvent(true);
            MyCalendarEventCopy._EventDuration = new TimeSpan(_EventDuration.Ticks);
            MyCalendarEventCopy._Name = this._Name.createCopy();
            MyCalendarEventCopy.updateStartTime(this.Start);
            MyCalendarEventCopy.updateEndTime( this.End);
            MyCalendarEventCopy._EventPreDeadline = new TimeSpan(_EventPreDeadline.Ticks);
            MyCalendarEventCopy._PrepTime = new TimeSpan(_PrepTime.Ticks);
            MyCalendarEventCopy._Priority = _Priority;
            MyCalendarEventCopy._EventRepetition = _EventRepetition?.CreateCopy();
            MyCalendarEventCopy._Complete = this._Complete;
            MyCalendarEventCopy._RigidSchedule = _RigidSchedule;//hack
            MyCalendarEventCopy._Splits = _Splits;
            MyCalendarEventCopy._userLocked = this._userLocked;
            MyCalendarEventCopy._AverageTimePerSplit = new TimeSpan(_AverageTimePerSplit.Ticks);

            if (Id != null)
            {
                MyCalendarEventCopy.UniqueID = Id;
            }
            else
            {
                MyCalendarEventCopy.UniqueID = UniqueID;//hack
            }
            MyCalendarEventCopy.EventSequence = EventSequence.CreateCopy();
            MyCalendarEventCopy._SubEvents = new SubEventDictionary<string, SubCalendarEvent>();
            MyCalendarEventCopy._UiParams = this._UiParams?.createCopy();
            MyCalendarEventCopy._DataBlob = this._DataBlob?.createCopy();
            MyCalendarEventCopy._Enabled = this._Enabled;
            MyCalendarEventCopy.isRestricted = this.isRestricted;
            MyCalendarEventCopy._LocationInfo = _LocationInfo;//hack you might need to make copy
            MyCalendarEventCopy._ProfileOfProcrastination = this._ProfileOfProcrastination?.CreateCopy();
            MyCalendarEventCopy._AutoDeleted = this._AutoDeleted;
            MyCalendarEventCopy._CompletedCount = this._CompletedCount;
            MyCalendarEventCopy._DeletedCount = this._DeletedCount;
            MyCalendarEventCopy._ProfileOfNow = this.getNowInfo?.CreateCopy();
            MyCalendarEventCopy._Semantics = this._Semantics?.createCopy();
            MyCalendarEventCopy._UsedTime = this._UsedTime;

            foreach (SubCalendarEvent eachSubCalendarEvent in this.SubEvents.Values)
            {
                MyCalendarEventCopy._SubEvents.Add(eachSubCalendarEvent.Id, eachSubCalendarEvent.CreateCopy(EventID.GenerateSubCalendarEvent(MyCalendarEventCopy.UniqueID)));
            }

            //MyCalendarEventCopy.SchedulStatus = SchedulStatus;
            MyCalendarEventCopy._otherPartyID = _otherPartyID == null ? null : _otherPartyID.ToString();
            MyCalendarEventCopy._Users = this._Users;
            MyCalendarEventCopy.DaySectionPreference = this.DaySectionPreference;
            MyCalendarEventCopy._EventDayPreference = this.DayPreference?.createCopy();
            return MyCalendarEventCopy;
        }

        public virtual void UpdateNowProfile(NowProfile ProfileNowData)
        {
            _ProfileOfNow.update(ProfileNowData);
            getProcrastinationInfo.reset();
        }

        public static CalendarEvent getEmptyCalendarEvent(EventID myEventID, DateTimeOffset Start = new DateTimeOffset(), DateTimeOffset End = new DateTimeOffset())
        {
            CalendarEvent retValue = new CalendarEvent(true);
            retValue.UniqueID = new EventID(myEventID.getCalendarEventID());
            retValue.updateStartTime(Start);
            retValue.updateEndTime( End);
            retValue._EventDuration = new TimeSpan(0);
            SubCalendarEvent emptySubEvent = SubCalendarEvent.getEmptySubCalendarEvent(retValue.UniqueID);
            retValue._SubEvents.Add(emptySubEvent.Id, emptySubEvent);
            retValue._Splits = 1;
            retValue._RigidSchedule = true;
            retValue._Complete = true;

            retValue._Enabled = false;
            //retValue.UpdateLocationMatrix(new Location_Elements());
            return retValue;
        }

        internal void decrementDeleteCount(TimeSpan span)
        {
            int currentCount = _DeletedCount - 1;
            if (_DeletedCount < 0)
            {
                throw new Exception("You are deleting more event Than is available");
            }

            _UsedTime += span;


            _DeletedCount = currentCount;
        }

        internal void incrementDeleteCount(TimeSpan span)
        {
            int currentCount = _DeletedCount + 1;
            if (_DeletedCount <= _Splits)
            {
                _DeletedCount = currentCount;
                _UsedTime += span;
            }
            else
            {
                throw new Exception("You are Increasing more event Than is available");
            }
        }

        internal void incrementAutoDeleteCount(TimeSpan span)
        {
            int currentCount = _AutoDeletedCount + 1;
            if (_AutoDeletedCount <= _Splits)
            {
                _AutoDeletedCount = currentCount;
                _UsedTime += span;
            }
            else
            {
                throw new Exception("You are Increasing the auto deltion count to a value more than the number of splits");
            }
        }

        internal void decrementCompleteCount(TimeSpan span)
        {
            int currentCount = _CompletedCount - 1;
            if (currentCount >= 0)
            {
                _CompletedCount = currentCount;
                _UsedTime += span;
            }
            else
            {
                throw new Exception("You are Completing more event Than is available");
            }
        }

        virtual public void Disable(bool goDeep = true)
        {
            this._Enabled = false;
            if (goDeep)
            {
                DisableSubEvents(AllSubEvents);
            }
        }

        virtual public void Enable(bool goDeep = false)
        {
            this._Enabled = true;
            if (goDeep)
            {
                EnableSubEvents(AllSubEvents);
            }
        }

        virtual public void RigidizeSubEvent(EventID subEventId)
        {
            RigidizeSubEvent(subEventId.ToString());
        }

        virtual public void UnRigidizeSubEvent(EventID subEventId)
        {
            UnRigidizeSubEvent(subEventId.ToString());
        }

        virtual public void RigidizeSubEvent(string subEventId)
        {
            SubCalendarEvent subEVent = getSubEvent(subEventId);
            if (subEVent != null)
            {
                subEVent.RigidizeEvent(this);
            }
        }

        virtual public void UnRigidizeSubEvent(string subEventId)
        {
            SubCalendarEvent subEVent = getSubEvent(subEventId);
            if (subEVent != null)
            {
                subEVent.UnRigidizeEvent(this);
            }
        }


        internal void incrementCompleteCount(TimeSpan span)
        {
            int CurrentCount = _CompletedCount;
            CurrentCount += 1;
            if ((CurrentCount + _DeletedCount) <= _Splits)
            {
                _CompletedCount = CurrentCount;
                _UsedTime += span;
                return;
            }

            throw new Exception("You are Completing more tasks Than is available");

        }

        internal void addCompletionTimes(DateTimeOffset time)
        {
            _LastCompletionTime = (_LastCompletionTime ?? "")+ time.ToUnixTimeMilliseconds().ToString() + ",";
        }

        internal void removeCompletionTimes(DateTimeOffset time)
        {
            _LastCompletionTime = (_LastCompletionTime ?? "");
            string timeString = time.ToUnixTimeMilliseconds().ToString() +",";
            int index = _LastCompletionTime.IndexOf(timeString);
            _LastCompletionTime.Remove(index, timeString.Count());
        }

        virtual public void SetCompletion(bool CompletionStatus, bool goDeep = false)
        {
            _Complete = CompletionStatus;
            if (IsRepeat)
            {
                if (goDeep)
                {
                    IEnumerable<CalendarEvent> AllrepeatingCalEvents = _EventRepetition.RecurringCalendarEvents();
                    foreach (CalendarEvent eachCalendarEvent in AllrepeatingCalEvents)
                    {
                        eachCalendarEvent.SetCompletion(CompletionStatus, goDeep);
                    }
                }
            }
            else
            {
                if (goDeep)
                {
                    if (CompletionStatus)
                    {
                        completeSubEvents(AllSubEvents);
                    }
                    else
                    {
                        nonCompleteSubEvents(AllSubEvents);
                    }

                }
            }
        }

        virtual public void setDayPreference(EventPreference dayPreference)
        {
            this._EventDayPreference = dayPreference;
        }

        /// <summary>
        /// Pauses a sub event in the calendar event
        /// </summary>
        /// <param name="SubEventId"></param>
        /// <param name="CurrentTime"></param>
        virtual public void PauseSubEvent(EventID SubEventId, DateTimeOffset CurrentTime, EventID CurrentPausedEventId = null)
        {
            SubCalendarEvent SubEvent = getSubEvent(SubEventId);
            if (!SubEvent.isLocked)
            {
                TimeSpan TimeDelta = SubEvent.Pause(CurrentTime);
                _UsedTime += TimeDelta;
            }
        }

        virtual public bool ContinueSubEvent(EventID SubEventId, DateTimeOffset CurrentTime)
        {
            SubCalendarEvent SubEvent = getSubEvent(SubEventId);
            return SubEvent.Continue(CurrentTime);
        }


        public override string ToString()
        {
            return this.getId + "::" + this.Start.ToString() + " - " + this.End.ToString();
        }


        public bool IsDateTimeWithin(DateTimeOffset DateTimeEntry)
        {
            return StartToEnd.IsDateTimeWithin(DateTimeEntry);
        }

        public void completeSubEvent(SubCalendarEvent mySubEvent)
        {
            if (!mySubEvent.getIsComplete)
            {
                mySubEvent.completeWithoutUpdatingCalEvent();
                incrementCompleteCount(mySubEvent.RangeSpan);
            }
        }


        public void disableSubEvent(SubCalendarEvent mySubEvent)
        {
            if (mySubEvent.isEnabled)
            {
                mySubEvent.disableWithoutUpdatingCalEvent();
                incrementDeleteCount(mySubEvent.RangeSpan);
            }
        }
        public void nonCompleteSubEvents(IEnumerable<SubCalendarEvent> mySubEvents)
        {
            int NumberToBeNonComplete = mySubEvents.Where(obj => obj.getIsComplete).Count();
            int CurrentCount = _CompletedCount - NumberToBeNonComplete;
            if (CurrentCount >= 0)
            {
                _CompletedCount = CurrentCount;
                mySubEvents.Where(obj => obj.getIsComplete).AsParallel().ForAll(obj => obj.nonCompleteWithoutUpdatingCalEvent());
                return;
            }
            throw new Exception("You are trying to complete more events than are avalable splits, check nonCompleteSubEvents");
        }

        public void completeSubEvents(IEnumerable<SubCalendarEvent> mySubEvents)
        {
            int NumberToBeEComplete = mySubEvents.Where(obj => !obj.getIsComplete).Count();
            int CurrentCount = _CompletedCount + NumberToBeEComplete;

            if (CurrentCount <= (_Splits - _DeletedCount))
            {
                _CompletedCount = CurrentCount;
                mySubEvents.Where(obj => !obj.getIsComplete).AsParallel().ForAll(obj => obj.completeWithoutUpdatingCalEvent());
                return;
            }

            throw new Exception("You are trying to complete more events than are avalable splits, check nonCompleteSubEvent");
        }

        public void EnableSubEvents(IEnumerable<SubCalendarEvent> mySubEvents)
        {
            int NumberToBeEnabled = mySubEvents.Where(obj => !obj.isEnabled).Count();
            int CurrentCount = _DeletedCount - NumberToBeEnabled;

            if (CurrentCount >= 0)
            {
                _DeletedCount = CurrentCount;
                mySubEvents.Where(obj => !obj.isEnabled).AsParallel().ForAll(obj => obj.enableWithouUpdatingCalEvent());
                return;
            }

            throw new Exception("You are trying to enable more events than is avalable");
        }


        public void DisableSubEvents(IEnumerable<SubCalendarEvent> mySubEvents)
        {
            int NumberToBeDeleted = mySubEvents.Where(obj => obj.isEnabled).Count();
            int CurrentCount = _DeletedCount + NumberToBeDeleted;
            if (CurrentCount <= _Splits)
            {
                _DeletedCount = CurrentCount;
                mySubEvents.AsParallel().ForAll(obj => obj.disableWithoutUpdatingCalEvent());
                return;
            }

            throw new Exception("You are trying to delete more events than is available. Check disableSubEvents");
        }


        public static int CompareByEndDate(CalendarEvent CalendarEvent1, CalendarEvent CalendarEvent2)
        {
            return CalendarEvent1.End.CompareTo(CalendarEvent2.End);
        }

        public static int CompareByStartDate(CalendarEvent CalendarEvent1, CalendarEvent CalendarEvent2)
        {
            return CalendarEvent1.Start.CompareTo(CalendarEvent2.Start);
        }

        virtual public void ReassignTime(DateTimeOffset StartTime, DateTimeOffset EndTime)
        {
            updateStartTime(StartTime);
            updateEndTime( EndTime);
            _SubEvents = new SubEventDictionary<string, SubCalendarEvent>();
        }


        /// <summary>
        /// Returns a repeating calendarevent. The ID has to be the string up the repeat CalEvent
        /// </summary>
        /// <param name="CalendarIDUpToRepeatCalEvent"></param>
        /// <returns></returns>
        public CalendarEvent getRepeatedCalendarEvent(string CalendarIDUpToRepeatCalEvent)
        {
            /*foreach(CalendarEvent MyCalendarEvent in EventRepetition.RecurringCalendarEvents)
            {
                if (MyCalendarEvent.ID == CalendarID)
                {
                    return MyCalendarEvent;
                }
            }
            return null;*/
            if (IsRepeat)
            {
                return _EventRepetition.getCalendarEvent(CalendarIDUpToRepeatCalEvent);
            }
            else
            {
                return this;
            }

        }

        virtual public void SetEventEnableStatus(bool EnableDisableFlag)
        {
            this._Enabled = EnableDisableFlag;
        }

        virtual public TimeLine PinSubEventsToStart(TimeLine MyTimeLine, List<SubCalendarEvent> MySubCalendarEventList)
        {
            TimeSpan SubCalendarTimeSpan = new TimeSpan();
            DateTimeOffset ReferenceStartTime = new DateTimeOffset();
            DateTimeOffset ReferenceEndTime = new DateTimeOffset();

            ReferenceStartTime = MyTimeLine.Start;
            if (this.Start > MyTimeLine.Start)
            {
                ReferenceStartTime = this.Start;
            }

            ReferenceEndTime = this.End;
            if (this.End > MyTimeLine.End)
            {
                ReferenceEndTime = MyTimeLine.End;
            }

            foreach (SubCalendarEvent MySubCalendarEvent in MySubCalendarEventList)
            {
                SubCalendarTimeSpan = SubCalendarTimeSpan.Add(MySubCalendarEvent.getActiveDuration);//  you might be able to combine the implementing for lopp with this in order to avoid several loops
            }
            TimeSpan TimeDifference = (ReferenceEndTime - ReferenceStartTime);

            if (this.isLocked)
            {
                return null;
            }

            if (SubCalendarTimeSpan > TimeDifference)
            {
                return null;
                //throw new Exception("Oh oh check PinSubEventsToStart Subcalendar is longer than available timeline");
            }
            if ((ReferenceStartTime > this.End) || (ReferenceEndTime < this.Start))
            {
                return null;
                //throw new Exception("Oh oh Calendar event isn't Timeline range. Check PinSubEventsToEnd :(");
            }

            List<BusyTimeLine> MyActiveSlot = new List<BusyTimeLine>();
            foreach (SubCalendarEvent MySubCalendarEvent in MySubCalendarEventList)
            {
                DateTimeOffset MyStartTime = ReferenceStartTime;
                DateTimeOffset EndTIme = MyStartTime + MySubCalendarEvent.getActiveDuration;
                MySubCalendarEvent.ActiveSlot = new BusyTimeLine(MySubCalendarEvent.getId, (MyStartTime), EndTIme);
                ReferenceStartTime = EndTIme;
                MyActiveSlot.Add(MySubCalendarEvent.ActiveSlot);
            }

            MyTimeLine.OccupiedSlots = MyActiveSlot.ToArray();
            return MyTimeLine;
        }

        //CalendarEvent Methods
        public SubCalendarEvent getSubEvent(string SubEventID)
        {
            EventID eventId = new EventID(SubEventID);
            return getSubEvent(eventId);
        }


        public SubCalendarEvent getSubEvent(EventID SubEventID)
        {
            int i = 0;

            if (IsRepeat)
            {
                IEnumerable<CalendarEvent> AllrepeatingCalEvents = _EventRepetition.RecurringCalendarEvents();
                foreach (CalendarEvent MyCalendarEvent in AllrepeatingCalEvents)
                {
                    SubCalendarEvent MySubEvent = MyCalendarEvent.getSubEvent(SubEventID);
                    if (MySubEvent != null)
                    {
                        return MySubEvent;
                    }
                }
            }
            else
            {
                if (_SubEvents.ContainsKey(SubEventID.ToString()))
                {
                    return _SubEvents[SubEventID.ToString()];
                }
            }
            return null;
        }



        public virtual bool updateSubEvent(EventID SubEventID, SubCalendarEvent UpdatedSubEvent)
        {
            if (this.IsRepeat)
            {
                IEnumerable<CalendarEvent> AllrepeatingCalEvents = Repeat.RecurringCalendarEvents();

                foreach (CalendarEvent MyCalendarEvent in AllrepeatingCalEvents)
                {
                    if (MyCalendarEvent.updateSubEvent(SubEventID, UpdatedSubEvent))
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (_SubEvents.ContainsKey(SubEventID.ToString()))
                {
                    //SubCalendarEvent NewSubCalEvent = new SubCalendarEvent(SubEventID.ToString(), UpdatedSubEvent.Start, UpdatedSubEvent.End, UpdatedSubEvent.ActiveSlot, UpdatedSubEvent.Rigid, UpdatedSubEvent.isEnabled, UpdatedSubEvent.UIParam, UpdatedSubEvent.Notes, UpdatedSubEvent.isComplete, UpdatedSubEvent.myLocation, this.RangeTimeLine);
                    SubCalendarEvent CurrentSubEvent = _SubEvents[SubEventID.ToString()];
                    CurrentSubEvent.UpdateThis(UpdatedSubEvent);


                    //NewSubCalEvent.ThirdPartyID = CurrentSubEvent.ThirdPartyID;
                    //SubEvents[SubEventID] = NewSubCalEvent;//using method as opposed to the UpdateThis function because of the canexistwithintimeline function test in the UpdateThis function
                    return true;
                }
            }
            return false;
        }

        public bool removeSubCalEvents(IEnumerable<SubCalendarEvent> ElementsToBeRemoved)
        {
            SubCalendarEvent[] SubCalEVentsArray = ElementsToBeRemoved.ToArray();
            bool retValue = true;
            for (int i = 0; i < SubCalEVentsArray.Length; i++)
            {
                SubCalendarEvent mySubcalEvent = SubCalEVentsArray[i];
                if (_SubEvents.ContainsKey(mySubcalEvent.Id))
                {
                    _SubEvents[mySubcalEvent.Id] = null;
                }
                else
                {
                    retValue = false;
                }
            }
            return retValue;
        }
        /// <summary>
        /// Function evaluates the various scores for the different days based on location, occupancy percentage, ratio of active time to tipespan within timeline of calendar event
        /// </summary>
        /// <param name="timeLines"></param>
        /// <param name="borderLocations"></param>
        /// <returns></returns>
        public virtual List<double> EvaluateTimeLines(List<TimelineWithSubcalendarEvents> timeLines, ReferenceNow referenceNow, List<Tuple<Location, Location>> borderLocations = null, bool factorInTimelineOrder = false)
        {
            List<IList<double>> multiDimensionalCalculation = new List<IList<double>>();
            List<TimelineWithSubcalendarEvents> validTimeLine = timeLines.Select(timeLine => {
                if (timeLine.doesTimeLineInterfere(this.StartToEnd)) {
                    return timeLine;
                }
                else {
                    return null;
                }
            }).ToList();
            TimeSpan totalAvailableSpan = TimeSpan.FromTicks(timeLines.Sum(timeLine => timeLine.TimelineSpan.Ticks));

            for (int i = 0; i< validTimeLine.Count;i++)
            {
                TimelineWithSubcalendarEvents timeline = validTimeLine[i];
                if (timeline != null)
                {
                    List<TimeLine> interferringTImeLines = getInterferringWithTimeLine(timeline);
                    TimeSpan totalInterferringSpan = TimeSpan.FromTicks(interferringTImeLines.Sum(objTimeLine => objTimeLine.TimelineSpan.Ticks));
                    double distance = Location.calculateDistance(timeline.averageLocation, this.Location, 0);
                    double tickRatio = (double)this.getActiveDuration.Ticks / totalInterferringSpan.Ticks;
                    double occupancy = (double)timeline.Occupancy;
                    double availableSpanRatio = (double)totalInterferringSpan.Ticks / totalAvailableSpan.Ticks;
                    DayOfWeek weekDay = referenceNow.getDayOfTheWeek(timeline);
                    double dayIndexScore = DayPreference[weekDay].EvaluationScore;
                    ulong completeDayIndex = referenceNow.getDayIndexFromStartOfTime(timeline.Start);
                    double dayIndexCount = completeDayIndexes.Contains(completeDayIndex) ? 1 : 0;// if day has already being marked with event as complete 
                    IList<double> dimensionsPerDay;
                    if (!factorInTimelineOrder)
                    {
                        dimensionsPerDay = new List<double>() { distance, tickRatio, occupancy, dayIndexScore, dayIndexCount };
                    }
                    else
                    {
                        dimensionsPerDay = new List<double>() { distance, tickRatio, occupancy, dayIndexScore, dayIndexCount, i + 1 };
                    }
                    multiDimensionalCalculation.Add(dimensionsPerDay);
                }
                else
                {
                    multiDimensionalCalculation.Add(null);
                }
            }
            var NotNullMultidimenstionValues = multiDimensionalCalculation.Where(obj => obj != null).ToList();
            List<double> foundIndexes = Utility.multiDimensionCalculationNormalize(NotNullMultidimenstionValues);
            List<double> retValue = new List<double>();
            int notNullCounter = 0;
            foreach (var coordinates in multiDimensionalCalculation)
            {
                if (coordinates != null)
                {
                    retValue.Add(foundIndexes[notNullCounter++]);
                }
                else
                {
                    retValue.Add(double.NaN);
                }
            }
            return retValue;
        }

        static public string convertTimeToMilitary(string TimeString)
        {
            TimeString = TimeString.Replace(" ", "").ToUpper();
            string[] TimeIsolated = TimeString.Split(':');

            if (TimeIsolated.Length < 2)//ensures that the length of split is above or equal to 2 so rejects :MM or HH:
            {
                return null;
            }
            if (TimeIsolated.Length == 2)//checks if time is in format HH:MMAM as opposed to HH:MM:SSAM 
            {
                char AorP = TimeIsolated[1][2];
                TimeIsolated[1] = TimeIsolated[1].Substring(0, 2) + ":00" + AorP + "M";
                return convertTimeToMilitary(TimeIsolated[0] + ":" + TimeIsolated[1]);
            }
            //this part of the code can only be accessed if string format is HH:MM:SSAM
            if (TimeIsolated[2].Length > 2)//checks if the length of the 'Second' string is greater 2 which infers there might be a P or A at end of the string
            {
                if (!((TimeIsolated[2][2] == 'P') || (TimeIsolated[2][2] == 'A')))//checks if the 'Second' string has an A or P or else return null
                {
                    return null;
                }
            }
            else
            {
                uint MyUIntTest = 0;
                bool UintTest = uint.TryParse(TimeIsolated[0], out MyUIntTest);
                if ((UintTest) && (MyUIntTest <= 24))//checks if value is uint and checks if its less than 24, e.g 13:xx as opposed to 25:xx
                {
                    return TimeString;
                }
                else
                {
                    return null;
                }
            }

            int HourInt = Convert.ToInt32(TimeIsolated[0]);
            if (TimeIsolated[2][2] == 'P')
            {
                HourInt = Convert.ToInt32(TimeIsolated[0]);
                HourInt += 12;
            }

            /*                Replace("PM", "");
                        TimeString = TimeString.Replace("AM", "");
                        TimeIsolated = TimeString.Split(':');*/
            if ((HourInt % 12) == 0)
            {
                HourInt = HourInt - 12;
            }
            TimeIsolated[0] = HourInt.ToString();
            TimeIsolated[1] = TimeIsolated[1].Substring(0, 2);
            TimeString = TimeIsolated[0] + ":" + TimeIsolated[1];

            //TimeString=TimeString.Substring(0, 5);
            return TimeString;
        }
        protected DateTimeOffset[] getActiveSlots()
        {
            return new DateTimeOffset[0];
        }
        public CalendarEvent GetAllScheduleEventsFromXML()
        {
            XmlTextReader reader = new XmlTextReader("MyEventLog.xml");
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        Console.Write("<" + reader.Name);
                        Console.WriteLine(">");
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        Console.WriteLine(reader.Value);
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        Console.Write("</" + reader.Name);
                        Console.WriteLine(">");
                        break;
                }
            }
            return new CalendarEvent(true);
        }

        virtual public void updateEventSequence()
        {
            EventSequence = new TimeLine(this.Start, this.End);

            foreach (SubCalendarEvent mySubCalendarEvent in ActiveSubEvents)
            {
                if (mySubCalendarEvent != null)
                {
                    EventSequence.MergeTimeLineBusySlots(mySubCalendarEvent.StartToEnd);
                }

            }
        }


        public virtual void UpdateThis(CalendarEvent CalendarEventEntry)
        {
            if ((this.getId == CalendarEventEntry.getId))
            {
                _EventDuration = CalendarEventEntry.getActiveDuration;
                _Name = CalendarEventEntry._Name;
                updateStartTime(CalendarEventEntry.Start);
                updateEndTime( CalendarEventEntry.End);
                _EventPreDeadline = CalendarEventEntry.getPreDeadline;
                _PrepTime = CalendarEventEntry._PrepTime;
                _Priority = CalendarEventEntry._Priority;
                _EventRepetition = CalendarEventEntry._EventRepetition;
                _Complete = CalendarEventEntry._Complete;
                _RigidSchedule = CalendarEventEntry._RigidSchedule;
                _Splits = CalendarEventEntry._Splits;
                _AverageTimePerSplit = CalendarEventEntry._AverageTimePerSplit;
                UniqueID = CalendarEventEntry.UniqueID;
                EventSequence = CalendarEventEntry.EventSequence; ;
                _SubEvents = CalendarEventEntry._SubEvents;
                //SchedulStatus=CalendarEventEntry.SchedulStatus;
                CalendarError = CalendarEventEntry.CalendarError;
                _Enabled = CalendarEventEntry._Enabled;
                _UiParams = CalendarEventEntry._UiParams;
                _DataBlob = CalendarEventEntry._DataBlob;
                _LocationInfo = CalendarEventEntry._LocationInfo;
                _otherPartyID = CalendarEventEntry._otherPartyID;
                this._Creator = CalendarEventEntry._Creator;
                this.CalculableSubEvents = CalendarEventEntry.CalculableSubEvents;
                this._CompletedCount = CalendarEventEntry._CompletedCount;
                this._DeletedCount = CalendarEventEntry._DeletedCount;
                this.EndOfCalculation = CalendarEventEntry.EndOfCalculation;
                this._isCalculableInitialized = CalendarEventEntry._isCalculableInitialized;
                this.isUnDesignableInitialized = CalendarEventEntry.isUnDesignableInitialized;
                this._Splits = CalendarEventEntry._Splits;
                this.UnDesignables = CalendarEventEntry.UnDesignables;
                this._ProfileOfNow = CalendarEventEntry._ProfileOfNow;
                this._ProfileOfProcrastination = CalendarEventEntry._ProfileOfProcrastination;
                this._Semantics = CalendarEventEntry._Semantics;
                this._ThirdPartyFlag = CalendarEventEntry._ThirdPartyFlag;
                this.ThirdPartyTypeInfo = CalendarEventEntry.ThirdPartyTypeInfo;
                this.ThirdPartyUserIDInfo = CalendarEventEntry.ThirdPartyUserIDInfo;
                this._AutoDeleted = CalendarEventEntry._AutoDeleted;
                this._Users = CalendarEventEntry._Users;
                this._UsedTime = CalendarEventEntry._UsedTime;
                return;
            }

            throw new Exception("Invalid Calendar ID used in Update Calendar Event");
        }
        virtual public bool shiftEvent(TimeSpan ChangeInTime, SubCalendarEvent[] UpdatedSubCalEvents)
        {
            TimeLine UpdatedTimeLine = new TimeLine(this.Start + ChangeInTime, this.End + ChangeInTime);
            bool retValue = true;
            foreach (SubCalendarEvent eachSubCalendarEvent in UpdatedSubCalEvents)//test if the updated locations for the subevents fall within the shifted timeline
            {
                if (!(UpdatedTimeLine.IsTimeLineWithin(eachSubCalendarEvent.StartToEnd)))
                {
                    return false;
                }
            }




            foreach (SubCalendarEvent eachSubCalendarEvent in UpdatedSubCalEvents)
            {
                if (!updateSubEvent(eachSubCalendarEvent.SubEvent_ID, eachSubCalendarEvent))
                {
                    retValue = false;//there was some error updating the subevent
                }
            }


            updateStartTime(Start + ChangeInTime);
            updateEndTime( End + ChangeInTime);

            return retValue;
        }

        virtual public List<SubCalendarEvent> AllDesignatedSubEventsFromCalculables(bool forceDesignableRefresh = true)
        {
            if (forceDesignableRefresh)
            {
                updatedUnDesignated();
            }

            List<SubCalendarEvent> retValue = CalculableSubEvents.Except(UnDesignables).ToList();
            return retValue;
        }




        virtual public List<SubCalendarEvent> AllUnDesignatedAndActiveSubEventsFromCalculables(bool forceDesignableRefresh = true)
        {
            if (forceDesignableRefresh)
            {
                updatedUnDesignated();
            }
            List<SubCalendarEvent> retValue = UnDesignables.ToList();
            return retValue;
        }


        virtual public long getNumberOfDesignatedAndActiveSubEventsFromCalculables(bool forceDesignableRefresh = true)
        {
            if (forceDesignableRefresh)
            {
                updatedUnDesignated();
            }

            long retValue = CalculableSubEvents.Count - UnDesignables.Count;
            return retValue;
        }



        virtual public void resetDesignationAllActiveEventsInCalculables()
        {
            CalculableSubEvents.AsParallel().ForAll(obj => obj.undesignate());
            return;
        }

        public virtual void InitialCalculationLookupDays(IEnumerable<DayTimeLine> RelevantDays, ReferenceNow now = null)
        {
            CalculationLimitation = RelevantDays.Where(obj => {
                var timeLine = obj.InterferringTimeLine(StartToEnd);
                if (timeLine != null && timeLine.TimelineSpan >= _AverageTimePerSplit)
                {
                    return true;
                }
                else return false;
            }).ToDictionary(obj => obj.UniversalIndex, obj => obj);
            FreeDaysLimitation = CalculationLimitation.Where( obj => !DaysWithSubEvents.Contains(obj.Key)) .ToDictionary(obj => obj.Key, obj => obj.Value);
            CalculationLimitationWithUnUsables = CalculationLimitation.ToDictionary(obj => obj.Key, obj => obj.Value);
        }

        /// <summary>
        /// function updates the free days by removing the Daytimeline using the universal dayindex days selected. Note not thread safe
        /// </summary>
        /// <param name="UniversalDayIndex"></param>
        public void removeDayTimeFromFreeUpdays(ulong UniversalDayIndex)
        {
            if (FreeDaysLimitation != null)
            {
                FreeDaysLimitation.Remove(UniversalDayIndex);
                return;
            }
            throw new Exception("You have not Initialized FreeDaysLimitation. Try making a call to InitialCalculationLookupDays");
        }
        /// <summary>
        /// function updates the free days by removing the Daytimelines using the universal dayindexes provided. Note not thread safe
        /// </summary>
        /// <param name="AllUniversalDayIndexes"></param>
        public void removeDayTimesFromFreeUpdays(IEnumerable<ulong> AllUniversalDayIndexes)
        {
            if (FreeDaysLimitation != null)
            {
                foreach (ulong eachIndex in AllUniversalDayIndexes)
                {
                    FreeDaysLimitation.Remove(eachIndex);
                }
                return;
            }
            throw new Exception("You have not Initialized FreeDaysLimitation. Try making a call to InitialCalculationLookupDays");
        }


        public void removeDayTimesFromFreeUpdays(ulong AllUniversalDayIndex)
        {
            if (FreeDaysLimitation != null)
            {
                FreeDaysLimitation.Remove(AllUniversalDayIndex);
                return;
            }
            throw new Exception("You have not Initialized FreeDaysLimitation. Try making a call to InitialCalculationLookupDays");
        }

        public List<DayTimeLine> getTimeLineWithoutMySubEvents()
        {
            List<DayTimeLine> retValue = FreeDaysLimitation.Values.ToList();
            return retValue;
        }

        public List<DayTimeLine> getDaysOnOrAfterProcrastination(ReferenceNow now, bool forceUpdateFreeTimeLine = true, List<DayTimeLine> AllFreeDayTIme = null)
        {

            AllFreeDayTIme = AllFreeDayTIme ?? CalculationLimitation.Values.ToList();
            if (forceUpdateFreeTimeLine)
            {
                AllFreeDayTIme.AsParallel().ForAll(obj => { obj.updateOccupancyOfTimeLine(); });
            }

            List<DayTimeLine> retValue = AllFreeDayTIme.Where(obj => obj.UniversalIndex >= now.getDayIndexFromStartOfTime(getProcrastinationInfo.PreferredStartTime)).ToList();
            return retValue;
        }

        public List<DayTimeLine> getTimeLineWithoutMySubEventsAndEnoughDuration(bool forceUpdateFreeTimeLine = true, List<DayTimeLine> AllFreeDayTIme = null)
        {
            if (AllFreeDayTIme == null)
            {
                AllFreeDayTIme = FreeDaysLimitation.Values.ToList();
            }
            else
            {
                AllFreeDayTIme = AllFreeDayTIme.Where(day => FreeDaysLimitation.ContainsKey(day.UniversalIndex)).ToList();
            }
            if (forceUpdateFreeTimeLine)
            {
                AllFreeDayTIme.AsParallel().ForAll(obj => { obj.updateOccupancyOfTimeLine(); });
            }

            List<DayTimeLine> retValue = AllFreeDayTIme.Where(obj => obj.TotalFreeSpotAvailable > _AverageTimePerSplit).ToList();
            return retValue;
        }

        public List<DayTimeLine> getDayTimeLineWhereOccupancyIsLess(double occupancy, bool forceUpdateFreeTimeLine = true, List<DayTimeLine> AllFreeDayTIme = null)
        {

            AllFreeDayTIme = AllFreeDayTIme ?? CalculationLimitation.Values.ToList();
            if (forceUpdateFreeTimeLine)
            {
                AllFreeDayTIme.AsParallel().ForAll(obj => { obj.updateOccupancyOfTimeLine(); });
            }

            List<DayTimeLine> retValue = AllFreeDayTIme.Where(obj => obj.Occupancy <= occupancy).ToList();
            return retValue;
        }

        public List<DayTimeLine> getTimeLineWithEnoughDuration(bool forceUpdateFreeTimeLine = true, List<DayTimeLine> AllFreeDayTIme = null)
        {
            AllFreeDayTIme = AllFreeDayTIme ?? CalculationLimitation.Values.ToList();
            if (forceUpdateFreeTimeLine)
            {
                AllFreeDayTIme.AsParallel().ForAll(obj => { obj.updateOccupancyOfTimeLine(); });
            }

            List<DayTimeLine> retValue = AllFreeDayTIme.Where(obj => obj.TotalFreeSpotAvailable > _AverageTimePerSplit).Where(dayTimeLine => {
                TimeLine timeLine = dayTimeLine.InterferringTimeLine(this.CalculationStartToEnd);
                if (timeLine != null)
                {
                    return timeLine.TimelineSpan >= _AverageTimePerSplit;
                }
                return false;
            }).ToList();
            return retValue;
        }


        public static long getUsableDaysTotal(IEnumerable<CalendarEvent> AllCalendarEvents)
        {
            long retValue = AllCalendarEvents.Sum(obj => obj.CalculationLimitation.Count);
            return retValue;
        }

        public static long getTotalUndesignatedEvents(IEnumerable<CalendarEvent> AllCalendarEvents)
        {
            List<SubCalendarEvent> UnassignedEvents = AllCalendarEvents.Where(obj => !obj.isLocked).SelectMany(obj => obj.UnDesignables).ToList();
            long retValue = AllCalendarEvents.Where(obj => !obj.isLocked).Sum(obj => obj.UnDesignables.Count);
            return retValue;
        }

        public static List<CalendarEvent> removeCalEventsWitNoUndesignablesFromCalculables(IEnumerable<CalendarEvent> AllCalendarEvents, bool forceDesignableRefresh = true)
        {
            List<CalendarEvent> retValue;// AllCalendarEvents.Where(obj => obj.UnDesignables.Count > 0).ToList();
            if (forceDesignableRefresh)
            {
                AllCalendarEvents.AsParallel().ForAll(obj => obj.updatedUnDesignated());
            }
            retValue = AllCalendarEvents.Where(obj => obj.UnDesignables.Count > 0).ToList();
            return retValue;
        }

        virtual protected CalendarEvent getCalculationCopy()
        {
            CalendarEvent RetValue = new CalendarEvent(true);
            RetValue._EventDuration = this.getActiveDuration;
            RetValue._Name = this._Name.createCopy();
            RetValue._EventPreDeadline = this.getPreDeadline;
            RetValue._PrepTime = this.getPreparation;
            RetValue._Priority = this.getEventPriority;
            RetValue._EventRepetition = this.Repeat;// EventRepetition != this.null ? EventRepetition.CreateCopy() : EventRepetition;
            RetValue._Complete = this.getIsComplete;
            RetValue._RigidSchedule = this._RigidSchedule;//hack
            RetValue._Splits = this._Splits;
            RetValue._AverageTimePerSplit = this.AverageTimeSpanPerSubEvent;
            RetValue.UniqueID = EventID.GenerateCalendarEvent();
            RetValue._SubEvents = new SubEventDictionary<string, SubCalendarEvent>();
            RetValue._UiParams = this.getUIParam;
            RetValue._DataBlob = this.Notes;
            RetValue._Enabled = this.isEnabled;
            RetValue.isRestricted = this.getIsEventRestricted;
            RetValue._LocationInfo = this._LocationInfo;//hack you might need to make copy
            RetValue._ProfileOfProcrastination = this.getProcrastinationInfo?.CreateCopy();
            RetValue._AutoDeleted = this.getIsUserDeleted;
            RetValue._CompletedCount = this.CompletionCount;
            RetValue._DeletedCount = this.DeletionCount;
            RetValue._ProfileOfNow = this._ProfileOfNow?.CreateCopy();
            RetValue._otherPartyID = this.ThirdPartyID;// == this.null ? null : otherPartyID.ToString();
            RetValue._Users = this._Users;
            RetValue._EventDayPreference = this._EventDayPreference?.createCopy();
            RetValue.updateStartTime(this.Start);
            RetValue.updateEndTime(this.End);
            //RetValue.UpdateLocationMatrix(RetValue.LocationInfo);
            return RetValue;
        }

        virtual public CalendarEvent getProcrastinationCopy(Procrastination ProcrastinationProfileData)
        {
            CalendarEvent retValue = getCalculationCopy();
            retValue._ProfileOfProcrastination = ProcrastinationProfileData;
            retValue.updateStartTime(ProcrastinationProfileData.PreferredStartTime);
            retValue.EventSequence = new TimeLine(retValue.Start, retValue.End);
            List<SubCalendarEvent> ProcrastinatonCopy = this.ActiveSubEvents
                .Select(
                obj => {
                    var subEvent = obj.getProcrastinationCopy(retValue, ProcrastinationProfileData);
                    subEvent.ParentCalendarEvent = retValue;
                    return subEvent;
                }).ToList();
            ProcrastinatonCopy.ForEach(obj => retValue._SubEvents.Add(obj.Id, obj));
            //retValue.SubEvents.Add(ProcrastinatonCopy.SubEvent_ID, ProcrastinatonCopy);
            return retValue;
        }

        virtual public CalendarEvent getNowCalculationCopy(NowProfile NowProfileData)
        {
            CalendarEvent retValue = getCalculationCopy();
            retValue._ProfileOfNow = NowProfileData;
            retValue.updateStartTime(NowProfileData.PreferredTime);
            retValue.EventSequence = new TimeLine(retValue.Start, retValue.End);
            SubCalendarEvent ProcrastinatonCopy = this.ActiveSubEvents[0].getNowCopy(retValue.UniqueID, NowProfileData);
            ProcrastinatonCopy.ParentCalendarEvent = retValue;
            retValue.updateEndTime( ProcrastinatonCopy.End);
            retValue._RigidSchedule = true;
            retValue._SubEvents.Add(ProcrastinatonCopy.Id, ProcrastinatonCopy);
            return retValue;
        }

        public virtual short updateNumberOfSplits(int SplitCOunt)
        {
            return updateSplitCount(SplitCOunt);
        }

        protected short updateSplitCount(int SplitCOunt)
        {
            if (NumberOfSplit == SplitCOunt)
            {
                return 0;
            }
            else
            {
                int delta = (SplitCOunt - NumberOfSplit);
                uint Change = (uint)Math.Abs(delta);
                if (delta > 0)
                {

                    if (IsRepeat)
                    {
                        _EventRepetition.RecurringCalendarEvents().AsParallel().ForAll(obj => obj.IncreaseSplitCount(Change));
                        return 2;
                    }
                    else
                    {
                        IncreaseSplitCount(Change);
                        return 2;
                    }
                }
                else
                {
                    if (IsRepeat)
                    {
                        _EventRepetition.RecurringCalendarEvents().AsParallel().ForAll(obj => obj.ReduceSplitCount(Change));
                        return 1;
                    }
                    else
                    {
                        ReduceSplitCount(Change);
                        return 1;
                    }
                }
            }
        }

        virtual protected void ReduceSplitCount(uint delta)
        {
            if (delta < _Splits)
            {
                List<SubCalendarEvent> orderedByActive = _SubEvents.Collection.OrderByDescending(subEvent => !subEvent.Value.isActive).Select(subEvemt => subEvemt.Value).ToList();
                for (int i = 0; i < delta; i++)
                {
                    SubCalendarEvent SubEvent = orderedByActive.First();
                    if (SubEvent.getIsComplete)
                    {
                        decrementCompleteCount(SubEvent.RangeSpan);
                    } else if (SubEvent.getIsDeleted)
                    {
                        decrementDeleteCount(SubEvent.RangeSpan);
                    }
                    _SubEvents.Remove(SubEvent.Id);
                    _RemovedSubEvents.Add(SubEvent);
                    _EventDuration = _EventDuration.Add(-SubEvent.getActiveDuration);
                    orderedByActive.RemoveAt(0);
                }
                _Splits -= (int)delta;
                UpdateTimePerSplit();
                return;
            }

            throw new Exception("You are trying to reduce the number of subevents past the min count");
        }

        virtual protected void IncreaseSplitCount(uint delta)
        {
            List<SubCalendarEvent> newSubs = new List<SubCalendarEvent>();
            for (int i = 0; i < delta; i++)
            {
                SubCalendarEvent newSubCalEvent = new SubCalendarEvent(this, getCreator, _Users, _TimeZone, _AverageTimePerSplit, this.getName, (End - _AverageTimePerSplit), this.End, new TimeSpan(), UniqueID.ToString(), _RigidSchedule, this.isEnabled, this._UiParams, this.Notes, this._Complete, _LocationInfo, this.StartToEnd);
                _SubEvents.Add(newSubCalEvent.Id, newSubCalEvent);
                _EventDuration = _EventDuration.Add(newSubCalEvent.getActiveDuration);
                newSubCalEvent.UiParamsId = this.UiParamsId;
                newSubCalEvent.DataBlobId = this.DataBlobId;
            }
            _Splits += (int)delta;
            UpdateTimePerSplit();
        }

        virtual protected void IncreaseSplitCount(uint delta, IEnumerable<SubCalendarEvent> subEvents)
        {
            List<SubCalendarEvent> newSubs = subEvents.ToList();
            for (int i = 0; i < delta; i++)
            {
                SubCalendarEvent newSubCalEvent = newSubs[i];
                _SubEvents.Add(newSubCalEvent.Id, newSubCalEvent);
            }
            _Splits += (int)delta;
            UpdateTimePerSplit();
        }

        public virtual short UpdateTimePerSplit()
        {
            short retValue = 0;
            var pertinentSubEvents = _SubEvents.Values.Where(SubEvent => SubEvent.isRigid == this.isRigid);
            if (pertinentSubEvents.Count() == 0)
            {
                pertinentSubEvents = _SubEvents.Values;
            }
            int count = pertinentSubEvents.Count();
            _EventDuration = TimeSpan.FromTicks(pertinentSubEvents.Sum(subEvent => subEvent.getActiveDuration.Ticks));
            _AverageTimePerSplit = TimeSpan.FromMinutes(Math.Round(TimeSpan.FromTicks(_EventDuration.Ticks / count).TotalMinutes));
            return retValue;
        }

        public void updateUnusableDaysAndRemoveDaysWithInsufficientFreeSpace()
        {
            updateUnusableDays();
            removeDayTimeLinesWithInsufficientSpace();
        }


        public void removeDayTimeLinesWithInsufficientSpace()
        {
            List<DayTimeLine> DaysWithInSufficientSpace = CalculationLimitation.Values.Where(obj => obj.TotalFreeSpotAvailable < _AverageTimePerSplit).ToList();
            DaysWithInSufficientSpace.ForEach(obj => CalculationLimitation.Remove(obj.UniversalIndex));
            DaysWithInSufficientSpace.ForEach(obj => FreeDaysLimitation.Remove(obj.UniversalIndex));

        }
        public void updateUnusableDays()
        {
            List<SubCalendarEvent> Undesignated = UnDesignables.ToList();

            foreach (SubCalendarEvent eachSubCalendarEvent in UnDesignables)
            {
                ulong unWantedIndex = eachSubCalendarEvent.resetAndgetUnUsableIndex();
                CalculationLimitation.Remove(unWantedIndex);
                FreeDaysLimitation.Remove(unWantedIndex);
            }
        }

        public void updateUnusableDaysAndRemoveDaysWithInsufficientFreeSpace(IEnumerable<ulong> UnUsableDays)
        {
            updateUnusableDays(UnUsableDays);
            removeDayTimeLinesWithInsufficientSpace();
        }

        public void updateUnusableDays(IEnumerable<ulong> UnUsableDays)
        {
            List<SubCalendarEvent> Undesignated = UnDesignables.ToList();

            foreach (SubCalendarEvent eachSubCalendarEvent in UnDesignables)
            {
                ulong unWantedIndex = eachSubCalendarEvent.resetAndgetUnUsableIndex();
                CalculationLimitation.Remove(unWantedIndex);
                FreeDaysLimitation.Remove(unWantedIndex);
            }
        }
        public void updatedUnDesignated()
        {
            if (isUnDesignableInitialized)
            {
                UnDesignables.RemoveWhere(obj => obj.isDesignated);
                return;
            }

            throw new Exception("You haven't initialized undesignated");
        }


        override public void updateTimeLine(TimeLine newTImeLine)
        {
            TimeLine oldTimeLine = new TimeLine(Start, End);
            AllSubEvents.AsParallel().ForAll(obj => obj.changeCalendarEventRange(newTImeLine));
            bool worksForAllSubevents = true;
            SubCalendarEvent failingSubEvent = SubCalendarEvent.getEmptySubCalendarEvent(this.Calendar_EventID);
            foreach (var obj in AllSubEvents)
            {
                if (!obj.canExistWithinTimeLine(newTImeLine))
                {
                    worksForAllSubevents = false;
                    failingSubEvent = obj;
                    break;
                }
            }
            if (worksForAllSubevents)
            {
                updateStartTime(newTImeLine.Start);
                updateEndTime( newTImeLine.End);
                if (this.isLocked)
                {
                    _EventDuration = End - Start;
                }
            } else
            {
                AllSubEvents.AsParallel().ForAll(obj => obj.changeCalendarEventRange(oldTimeLine));
                CustomErrors customError = new CustomErrors(CustomErrors.Errors.cannotFitWithinTimeline, "Cannot update the timeline for the calendar event with sub event " + failingSubEvent.getId + ". Most likely because the new time line won't fit the sub event");
                throw customError;
            }
            updateCalculationStartToEnd();
        }

        virtual public void updateTimeLine(SubCalendarEvent subEvent, TimeLine newTImeLine)
        {
            updateTimeLine(newTImeLine);
        }

        /// <summary>
        /// This updates the name of a calendar event
        /// </summary>
        /// <param name="NewName">The new name of the calendar event</param>
        /// <param name="justThisCalendarEvent">If this is a repeat calendar event does thisjust update this single instance</param>
        public virtual void updateEventName(string NewName, bool justThisCalendarEvent = false)
        {
            base.updateEventName(NewName);
            foreach (SubCalendarEvent subEvent in this.AllSubEvents)
            {
                subEvent.updateEventName(NewName);
            }
            if (!justThisCalendarEvent && base.IsRepeat)
            {
                foreach (CalendarEvent calEvent in Repeat.RecurringCalendarEvents().Where(obj => obj.getId != this.getId))
                {
                    calEvent.updateEventName(NewName, true);
                }
            }
        }

        public virtual void undesignateSubEvent(SubCalendarEvent subEvent)
        {
            UnDesignables.Add(subEvent);
            subEvent.undesignate();
        }

        public virtual void designateSubEvent(SubCalendarEvent subEvent, ReferenceNow now)
        {
            UnDesignables.Remove(subEvent);
            subEvent.designate(now);
            DaysWithSubEvents.Add(subEvent.UniversalDayIndex);
            if(FreeDaysLimitation!=null)
            {
                this.removeDayTimeFromFreeUpdays(subEvent.UniversalDayIndex);
            }
            
        }

        public virtual void deleteAllSubCalendarEventsFromRepeatParentCalendarEvent()
        {
            foreach(var subEvent in this.SubEvents.Values)
            {
                subEvent.RepeatParentEvent = null;
                subEvent.RepeatParentEventId = null;
                subEvent.ParentCalendarEvent = null;
                subEvent.CalendarEventId = null;
            }
            this._SubEvents = new SubEventDictionary<string, SubCalendarEvent>();
            _CompletedCount = 0;
            _DeletedCount = 0;
        }

        public void InitializeCounts(int Deletion, int Completion)
        {
            _DeletedCount = Deletion;
            _CompletedCount = Completion;
        }

        public void UpdateError(CustomErrors Error)
        {
            CalendarError = Error;
        }

        public void ClearErrorMessage()
        {
            CalendarError = new CustomErrors(string.Empty);
        }

        public void setRepeatParent(CalendarEvent repeatParent)
        {
            CalendarEvent currentParent = repeatParent;
            while (currentParent.RepeatParent_DB != null)
            {
                currentParent = currentParent.RepeatParent_DB;
            }
            _RepeatParentEvent = currentParent;
        }

        virtual public void updateCompletionTimeArray(ReferenceNow now)
        {
            _LastCompletionTime = (_LastCompletionTime ?? "");
            if (!string.IsNullOrEmpty(_LastCompletionTime) && !string.IsNullOrWhiteSpace(_LastCompletionTime))
            {
                string[] dateTimeInMs = _LastCompletionTime.Split(',')
                    .Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(str)).ToArray();
                ;
                completionDates = new DateTimeOffset[dateTimeInMs.Length];
                for (int i = 0; i < dateTimeInMs.Length; i++)
                {
                    string timeString = dateTimeInMs[i];
                    timeString = timeString.Trim();
                    if (!string.IsNullOrEmpty(timeString) && !string.IsNullOrWhiteSpace(timeString))
                    {
                        long timeInMs = long.Parse(timeString);
                        DateTimeOffset time = DateTimeOffset.FromUnixTimeMilliseconds(timeInMs);
                        completionDates[i] = time;
                        ulong dayIndex = now.getDayIndexFromStartOfTime(time);
                        completeDayIndexes.Add(dayIndex);
                    }

                }
                completionDates = completionDates.OrderBy(obj => obj).ToArray();
            }

        }

        virtual protected void updateCalculationStartToEnd()
        {
            _CalculationStartToEnd = new TimeLine(this.CalculationStart, this.End);
        }

        protected override void updateStartTime(DateTimeOffset time)
        {
            base.updateStartTime(time);
            updateCalculationStartToEnd();
        }

        protected override void updateEndTime(DateTimeOffset time)
        {
            base.updateEndTime(time);
            updateCalculationStartToEnd();
        }

        #endregion

        #region Properties
        override public string getId
        {
            get
            {
                return UniqueID.ToString();
            }
        }

        public EventID Calendar_EventID
        {
            get
            {
                return UniqueID;
            }
        }

        public bool isCalculableInitialized
        {
            get
            {
                return _isCalculableInitialized;
            }
        }


        public TimeSpan TimeLeftBeforeDeadline
        {
            get
            {
                return End - DateTimeOffset.UtcNow;
            }
        }

        public int NumberOfSplit
        {
            set
            {
                _Splits = value;
            }
            get
            {
                return _Splits;
            }
        }


        public bool IsRepeatsChildCalEvent
        {
            get; set;
        }

        public TimeSpan AverageTimeSpanPerSubEvent
        {
            get
            {
                return _AverageTimePerSplit;
            }
        }

        override public TimeSpan getActiveDuration
        {
            get
            {
                return _EventDuration;
            }
        }
        public SubCalendarEvent[] ActiveSubEvents//needs to update to get only active events
        {//return All Subcalevents that are enabled. returns 
            get
            {
                if (IsRepeat)
                {
                    return this.ActiveRepeatSubCalendarEvents;
                }

                IEnumerable<SubCalendarEvent> AllSubEvents = _SubEvents.Values.Where(obj => obj != null).Where(obj => (obj.isActive));

                return AllSubEvents.ToArray();
            }
        }



        public SubCalendarEvent[] EnabledSubEvents//needs to update to get only active events
        {//return All Subcalevents that are enabled.
            get
            {
                if (IsRepeat)
                {
                    return this.ActiveRepeatSubCalendarEvents;
                }

                IEnumerable<SubCalendarEvent> AllSubEvents = _SubEvents.Values.Where(obj => obj != null).Where(obj => (obj.isEnabled));

                return AllSubEvents.ToArray();
            }
        }


        public virtual SubCalendarEvent[] AllSubEvents
        {//return All Subcalevents that enabled or not.
            get
            {
                if (IsRepeat)
                {
                    return this.Repeat.RecurringCalendarEvents().SelectMany(obj => obj.AllSubEvents).ToArray();
                }

                return _SubEvents.Values.Where(obj => obj != null).ToArray();
            }
        }

        public virtual ICollection<SubCalendarEvent> AllSubEvents_DB
        {
            set
            {
                this._SubEvents = new SubEventDictionary<string, SubCalendarEvent>();
                if (value != null)
                {
                    this._SubEvents = new SubEventDictionary<string, SubCalendarEvent>(value);
                }
            }
            get
            {
                return _SubEvents ?? (_SubEvents = new SubEventDictionary<string, SubCalendarEvent>());
            }
        }

        [DefaultValue(0)]
        virtual public int CompletionCount_DB
        {
            set
            {
                _CompletedCount = value;
            }
            get
            {
                return _CompletedCount;
            }
        }
        [DefaultValue(0)]
        virtual public int DeletionCount_DB
        {
            set
            {
                _DeletedCount = value;
            }

            get
            {
                return _DeletedCount;
            }
        }

        override public int AutoDeletionCount_DB
        {
            set
            {
                _AutoDeletedCount = value;
            }

            get
            {
                return _AutoDeletedCount;
            }
        }


        public string DayPreferenceId
        {
            get; set;
        }
        [ForeignKey("DayPreferenceId")]
        virtual public EventPreference DayPreference_DB
        {
            get
            {
                if (_EventDayPreference == null)
                {
                    return null;
                }
                else
                {
                    return _EventDayPreference.isNull ? null : _EventDayPreference;
                }
            }
            set
            {
                _EventDayPreference = value;
            }
        }

        public string RepeatParentId
        {
            get {
                return this.RepeatParentEventId;
            }
            set
            {
                this.RepeatParentEventId = value;
            }
        }
        [ForeignKey("RepeatParentId")]
        virtual public CalendarEvent RepeatParent_DB
        {
            get
            {
                return _RepeatParentEvent;
            }
            set
            {
                _RepeatParentEvent = value;
            }
        }


        virtual public double AverageTimeSpanPerSubEvent_DB
        {
            set
            {
                _AverageTimePerSplit = TimeSpan.FromMilliseconds(value);
            }
            get
            {
                return _AverageTimePerSplit.TotalMilliseconds;
            }
        }

        public CustomErrors Error
        {

            get
            {
                return CalendarError;
            }
        }

        public SubCalendarEvent[] ActiveRepeatSubCalendarEvents
        {
            get
            {
                List<SubCalendarEvent> MyRepeatingSubCalendarEvents = new List<SubCalendarEvent>();
                if (IsRepeat)
                {
                    return this.Repeat.RecurringCalendarEvents().Where(calEvent => calEvent.isActive).SelectMany(obj => obj.ActiveSubEvents).ToArray();
                }

                return MyRepeatingSubCalendarEvents.ToArray();

            }

        }

        public SubCalendarEvent[] EnabledRepeatSubCalendarEvents
        {
            get
            {
                List<SubCalendarEvent> MyRepeatingSubCalendarEvents = new List<SubCalendarEvent>();
                if (IsRepeat)
                {
                    return this.Repeat.RecurringCalendarEvents().SelectMany(obj => obj.EnabledSubEvents).ToArray();
                }

                return MyRepeatingSubCalendarEvents.ToArray();

            }

        }
        public override TimeLine StartToEnd
        {
            get
            {
                EventSequence = new TimeLine(this.Start, this.End);
                return EventSequence;
            }
        }

        virtual public TimeLine CalculationStartToEnd
        {
            get
            {
                if (_CalculationStartToEnd != null)
                {
                    return _CalculationStartToEnd;
                }
                else
                {
                    updateCalculationStartToEnd();
                    return _CalculationStartToEnd;
                }
            }
        }

        protected SubEventDictionary<string, SubCalendarEvent> SubEvents
        { 
            get {
                return _SubEvents?? (_SubEvents = new SubEventDictionary<string, SubCalendarEvent>());
            }
        }

        public TimeSpan RangeSpan
        {
            get
            {
                return this.StartToEnd.TimelineSpan;
            }
        }
        
        virtual public int CompletionCount
        {
            get 
            {
                return _CompletedCount;
            }
        }

        virtual public int DeletionCount
        {
            get
            {
                return _DeletedCount;
            }
        }

        virtual public int AutoDeletionCount
        {
            get
            {
                return _AutoDeletedCount;
            }
        }

        virtual public string LastCompletionTime_DB
        {
            set
            {
                _LastCompletionTime = value;
            }
            get
            {
                return _LastCompletionTime;
            }
        }

        public virtual DateTimeOffset CalculationStart
        {
            get
            {
                DateTimeOffset retValue = this.Start;
                if (getProcrastinationInfo != null && !getProcrastinationInfo.isNull)
                {
                    retValue = retValue >= this.getProcrastinationInfo.PreferredStartTime ? retValue : this.getProcrastinationInfo.PreferredStartTime;
                }

                return retValue;
            }
        }

        virtual public DateTimeOffset[] LastCompletionTime
        {
            get
            {
                return completionDates;
            }
        }

        virtual public MiscData Notes
        {
            get
            {
                return _DataBlob;
            }
        }

        [NotMapped]
        virtual public CalendarEvent DefaultCalendarEvent
        {
            set
            {
                _DefaultCalendarEvent = value;
            }
            get
            {
                return _DefaultCalendarEvent;
            }
        }

        virtual public IEnumerable<SubCalendarEvent> RemoveSubEventFromEntity
        {
            get
            {
                if(IsRepeat)
                {
                    var recurringCalEvents = Repeat.RecurringCalendarEvents();
                    return recurringCalEvents.SelectMany(calEVent => calEVent._RemovedSubEvents);
                } else
                {
                    return _RemovedSubEvents;
                }
            }
        }

        public EventPreference DayPreference
        {
            get
            {
                if (string.IsNullOrEmpty(DayPreferenceId) && this._EventDayPreference == null)
                {
                    _EventDayPreference = new EventPreference();
                    if (this.IsRepeat)
                    {
                        foreach (CalendarEvent calEvent in this.Repeat.RecurringCalendarEvents())
                        {
                            if ((calEvent._EventDayPreference == null || calEvent.DayPreference != _EventDayPreference)
                                && (string.IsNullOrEmpty(calEvent.DayPreferenceId) || calEvent.DayPreferenceId != this.DayPreferenceId)
                                && calEvent.DayPreference.isNull)
                            {
                                calEvent._EventDayPreference = _EventDayPreference;
                            }
                        }
                    }
                }
                return this._EventDayPreference;
            }
        }
        #endregion

    }
}
