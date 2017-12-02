#define SetDefaultPreptimeToZero
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace TilerElements
{
    public class CalendarEvent : TilerEvent, IDefinedRange
    {
        protected DateTimeOffset EndOfCalculation = DateTime.Now.AddMonths(3);
        protected int Splits;
        protected TimeSpan _AverageTimePerSplit;
        protected int CompletedCount;
        protected int DeletedCount;
//        protected bool FromRepetion=false;
        protected Dictionary<EventID, SubCalendarEvent> SubEvents;

        CustomErrors CalendarError = null;
        protected TimeLine EventSequence;
        protected HashSet<SubCalendarEvent> CalculableSubEvents = new HashSet<SubCalendarEvent>();
        protected HashSet<SubCalendarEvent> UnDesignables = new HashSet<SubCalendarEvent>();
        protected bool isCalculableInitialized = false;
        protected bool isUnDesignableInitialized = false;
        Dictionary<ulong, DayTimeLine> CalculationLimitationWithUnUsables;
        Dictionary<ulong, DayTimeLine> CalculationLimitation;
        Dictionary<ulong, DayTimeLine> FreeDaysLimitation;

        #region Constructor
        public CalendarEvent()
        {
            EventDuration = new TimeSpan();
            _Name = new EventName();
            StartDateTime = new DateTimeOffset();
            EndDateTime = new DateTimeOffset();
            EventPreDeadline = new TimeSpan();
            PrepTime = new TimeSpan();
            Priority = 0;
            EventRepetition = new Repetition();
            RigidSchedule = false;
            Splits = 1;
            LocationInfo = new Location();
            UniqueID = EventID.GenerateCalendarEvent();
            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            Semantics = new Classification();
            otherPartyID = "";
            CalendarError = null;
            EventSequence = new TimeLine();
            ProfileOfProcrastination = new Procrastination(new DateTimeOffset(), new TimeSpan());
            ProfileOfNow = new NowProfile();
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
            Location location, TilerUser creator, TilerUserGroup otherUsers, bool userDeleted, DateTimeOffset timeOfCreation, string timeZoneOrigin)
        {
            if (end < start)
            {
                throw new Exception("Calendar Event cannot have an end time earlier than the start time");
            }

            this.StartDateTime = start;
            this.EndDateTime = end;
            this.Splits = split;
            this._Creator = creator;
            this.DataBlob = miscData;
            this.UniqueID = EventID.GenerateCalendarEvent();
            this.EventDuration = duration;
            this._Users = otherUsers;
            this.EventPreDeadline = preDeadline;
            this.PrepTime = prepTime;
            this.ProfileOfNow = nowProfile??new NowProfile();
            this.ProfileOfProcrastination = procrastinationProfile??new Procrastination(new DateTimeOffset(), new TimeSpan());
            this.EventRepetition = repetition;
            this.Complete = completeflag;
            this.Enabled = isEnabled;
            this.LocationInfo = location;
            this.UiParams = displayData;
            this._Name = name;
            this.UserDeleted = userDeleted;
            this._TimeZone = timeZoneOrigin;
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
            bool initializeSubCalendarEvents = true)
            :this(
                 //eventId, 
                 NameEntry, StartData, EndData, eventDuration, eventPrepTimeSpan, preDeadlineTimeSpan, eventSplit, EventRepetitionEntry, UiData, NoteData, EnabledEventFlag, CompletionFlag, nowProfile, procrastination, EventLocation, creator, users, false, DateTimeOffset.UtcNow, timeZone)
        {
            UniqueID = eventId ?? this.UniqueID; /// already initialized by parent initialization
            _AverageTimePerSplit = TimeSpan.FromTicks(((eventDuration.Ticks / Splits)));
            this.EventDuration = eventDuration;
            if (initializeSubCalendarEvents)
            {
                initializeSubEvents();
            }
            EventSequence = new TimeLine(StartDateTime, EndDateTime);
            //UpdateLocationMatrix(LocationInfo);
        }

        virtual public void initializeSubEvents()
        {
            SubEvents = new Dictionary<EventID, SubCalendarEvent>();

            for (int i = 0; i < Splits; i++)
            {
                SubCalendarEvent newSubCalEvent = new SubCalendarEvent(getCreator, _Users, _TimeZone, _AverageTimePerSplit, this.getName, (EndDateTime - _AverageTimePerSplit), this.End, new TimeSpan(), UniqueID.ToString(), RigidSchedule, this.Enabled, this.UiParams, this.Notes, this.Complete, this.LocationInfo, this.RangeTimeLine);
                SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
            }
        }

        public CalendarEvent(CalendarEvent MyUpdated, SubCalendarEvent[] MySubEvents)
        {
            _Name = MyUpdated._Name;
            UniqueID = MyUpdated.UniqueID;
            StartDateTime = MyUpdated.StartDateTime;
            EndDateTime = MyUpdated.End;
            EventSequence = new TimeLine(StartDateTime, EndDateTime);
            EventDuration = MyUpdated.getActiveDuration;
            Splits = MyUpdated.Splits;
            PrepTime = MyUpdated.PrepTime;
            EventPreDeadline = MyUpdated.getPreDeadline;
            RigidSchedule = MyUpdated.getRigid;
            _AverageTimePerSplit = MyUpdated._AverageTimePerSplit;
            Enabled = MyUpdated.isEnabled;
            Complete = MyUpdated.getIsComplete;
            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            for (int i = 0; i < MySubEvents.Length; i++)//using MySubEvents.length for the scenario of the call for repeat event. Remember the parent event does not generate subevents
            {
                SubCalendarEvent newSubCalEvent = MySubEvents[i];
                if (SubEvents.ContainsKey(newSubCalEvent.SubEvent_ID))
                { 
                    SubEvents[newSubCalEvent.SubEvent_ID]=newSubCalEvent;
                }
                else
                {
                    SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
                }   
            }

            this.Priority = MyUpdated.getEventPriority;
            this.UiParams = MyUpdated.getUIParam;
            this.DataBlob = MyUpdated.Notes;
            this.isRestricted = MyUpdated.getIsEventRestricted;
            this.LocationInfo = MyUpdated.myLocation;//hack you might need to make copy
            this.ProfileOfProcrastination = MyUpdated.getProcrastinationInfo;
            this.DeadlineElapsed = MyUpdated.getIsDeadlineElapsed;
            this.UserDeleted = MyUpdated.getIsUserDeleted;
            this.CompletedCount = MyUpdated.CompletionCount;
            this.DeletedCount = MyUpdated.DeletionCount;
            EventRepetition = MyUpdated.Repeat;
            ProfileOfNow = MyUpdated.getNowInfo;
            EventSequence = new TimeLine(StartDateTime, EndDateTime);
            _Creator = MyUpdated.getCreator;
            _UsedTime = MyUpdated.UsedTime;
            _Users = MyUpdated._Users;
            _TimeZone = MyUpdated._TimeZone;
            isProcrastinateEvent = MyUpdated.isProcrastinateEvent;
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
            if (getIsRepeat)
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
            TimeLine subEventActiveTime = subEvent.RangeTimeLine;
            TimeSpan dayDiffSpan = timeLine.Start.Date - subEventActiveTime.Start.Date;
            TimeLine subEvenRangeReadjustedToexpectedTimeLine = new TimeLine(subEventActiveTime.Start.Add(dayDiffSpan), subEventActiveTime.End.Add(dayDiffSpan));
            calEvent.EventRepetition = new Repetition(true, timeLine, Repetition.Frequency.YEARLY, subEvenRangeReadjustedToexpectedTimeLine);
            calEvent.EventRepetition.PopulateRepetitionParameters(calEvent);
            CalendarEvent calEventCpy = calEvent.Repeat.RecurringCalendarEvents().Single();// using ssingle because this must always return a single calendarevent. Because we generated a repeat event which should only have one calendar event;
            SubCalendarEvent subEventCopy = calEventCpy.AllSubEvents.First();
            SubCalendarEvent duplicateOfOriginal = subEvent.createCopy(subEventCopy.SubEvent_ID);
            
            subEventCopy.UpdateThis(duplicateOfOriginal);
            //calEventCpy.SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            calEventCpy.StartDateTime = timeLine.Start;
            calEventCpy.EndDateTime= timeLine.End;
            subEventCopy.updateCalculationEventRange(timeLine);
            if (subEventCopy.getRigid)/// this is optimized for this use case
            {
                DateTimeOffset dayStart = timeLine.Start;
                DateTimeOffset preferredStart = new DateTimeOffset(dayStart.Year, dayStart.Month, dayStart.Day, subEventCopy.Start.Hour, subEventCopy.Start.Minute, subEventCopy.Start.Second, new TimeSpan());
                if(preferredStart < dayStart)
                {
                    preferredStart=preferredStart.AddDays(1);
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

        virtual public void ReverseWhatIf(TempTilerEventChanges toBeReverted)
        {
            CalendarEvent calEvent = toBeReverted.allChanges[0] as CalendarEvent;
            calEvent.EventRepetition = new Repetition();
            SubCalendarEvent subEventIni = calEvent.TempChanges.allChanges[0] as SubCalendarEvent;
            SubCalendarEvent subEventCopy = calEvent.TempChanges.allChanges[1] as SubCalendarEvent;
            subEventIni.Enable(calEvent);
            //subEventIni.shiftEvent(subEventCopy.Start);
        }
        #endregion

        /// <summary>
        /// Calendarevent Identifies the subeevents that are to be used for the calculation of a schedule. You need to initialize this for calculation of schedule
        /// </summary>
        public void initializeCalculables()
        {
            CalculableSubEvents = new HashSet<SubCalendarEvent>(SubEvents.Values.Where(obj => obj.isInCalculationMode));
            isCalculableInitialized = true;
        }

        public void initializeUndesignables()
        {
            if (isCalculableInitialized)
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

        public void updateDesignablesFromCalculables()
        { 
            
        }


        ///*
        public void updateProcrastinate(Procrastination ProcrastinationTime)
        {
            ProfileOfProcrastination = ProcrastinationTime;
            ProfileOfNow.reset();
        }
        //*/

        protected Dictionary<EventID, SubCalendarEvent> getSubEvents()
        {
            return SubEvents;
        }

        virtual public CalendarEvent createCopy(EventID Id=null)
        {
            CalendarEvent MyCalendarEventCopy = new CalendarEvent();
            MyCalendarEventCopy.EventDuration = new TimeSpan(EventDuration.Ticks);
            MyCalendarEventCopy._Name = this._Name.createCopy();
            MyCalendarEventCopy.StartDateTime = StartDateTime;
            MyCalendarEventCopy.EndDateTime = EndDateTime;
            MyCalendarEventCopy.EventPreDeadline = new TimeSpan(EventPreDeadline.Ticks);
            MyCalendarEventCopy.PrepTime = new TimeSpan(PrepTime.Ticks);
            MyCalendarEventCopy.Priority = Priority;
            MyCalendarEventCopy.EventRepetition = EventRepetition.CreateCopy();// EventRepetition != null ? EventRepetition.CreateCopy() : EventRepetition;
            MyCalendarEventCopy.Complete = this.Complete;
            MyCalendarEventCopy.RigidSchedule = RigidSchedule;//hack
            MyCalendarEventCopy.Splits = Splits;
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
            MyCalendarEventCopy.SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            MyCalendarEventCopy.UiParams = this.UiParams.createCopy();
            MyCalendarEventCopy.DataBlob = this.DataBlob.createCopy();
            MyCalendarEventCopy.Enabled = this.Enabled;
            MyCalendarEventCopy.isRestricted = this.isRestricted;
            MyCalendarEventCopy.LocationInfo = LocationInfo;//hack you might need to make copy
            MyCalendarEventCopy.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            MyCalendarEventCopy.DeadlineElapsed = this.DeadlineElapsed;
            MyCalendarEventCopy.UserDeleted= this.UserDeleted;
            MyCalendarEventCopy.CompletedCount = this.CompletedCount;
            MyCalendarEventCopy.DeletedCount = this.DeletedCount;
            MyCalendarEventCopy.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            MyCalendarEventCopy.ProfileOfNow = this.getNowInfo.CreateCopy();
            MyCalendarEventCopy.Semantics = this.Semantics.createCopy();
            MyCalendarEventCopy._UsedTime = this._UsedTime;

            foreach (SubCalendarEvent eachSubCalendarEvent in this.SubEvents.Values)
            {
                MyCalendarEventCopy.SubEvents.Add(eachSubCalendarEvent.SubEvent_ID, eachSubCalendarEvent.createCopy(EventID.GenerateSubCalendarEvent(MyCalendarEventCopy.UniqueID)));
            }

            //MyCalendarEventCopy.SchedulStatus = SchedulStatus;
            MyCalendarEventCopy.otherPartyID = otherPartyID == null ? null : otherPartyID.ToString();
            MyCalendarEventCopy._Users = this._Users;
            MyCalendarEventCopy.DaySectionPreference = this.DaySectionPreference;
            return MyCalendarEventCopy;
        }

        public void UpdateNowProfile(NowProfile ProfileNowData)
        {
            ProfileOfNow = ProfileNowData;
            ProfileOfProcrastination.reset();
        }

        public static CalendarEvent getEmptyCalendarEvent( EventID myEventID,DateTimeOffset Start=new DateTimeOffset(), DateTimeOffset End=new DateTimeOffset())
        {
            CalendarEvent retValue = new CalendarEvent();
            retValue.UniqueID = new EventID( myEventID.getCalendarEventID());
            retValue.StartDateTime = Start;
            retValue.EndDateTime = End;
            retValue.EventDuration = new TimeSpan(0);
            SubCalendarEvent emptySubEvent = SubCalendarEvent.getEmptySubCalendarEvent(retValue.UniqueID);
            retValue.SubEvents.Add(emptySubEvent.SubEvent_ID, emptySubEvent);
            retValue.Splits = 1;
            retValue.RigidSchedule = true;
            retValue.Complete = true;

            retValue.Enabled = false;
            //retValue.UpdateLocationMatrix(new Location_Elements());
            return retValue;
        }

        internal void decrementDeleteCount(TimeSpan span)
        {
            int currentCount = DeletedCount - 1;
            if (DeletedCount < 0)
            {
                throw new Exception("You are deleting more event Than is available");
            }

            _UsedTime += span;


            DeletedCount = currentCount;
        }

        internal void incrementDeleteCount(TimeSpan span)
        {
            int currentCount = DeletedCount + 1;
            if (DeletedCount <= Splits)
            {
                DeletedCount = currentCount;
                _UsedTime += span;
            }
            else
            {
                throw new Exception("You are Increasing more event Than is available");
            }
        }

        internal void decrementCompleteCount(TimeSpan span)
        {
            int currentCount = CompletedCount - 1;
            if (currentCount >= 0)
            {
                CompletedCount = currentCount;
                _UsedTime += span;
            }
            else
            {
                throw new Exception("You are Completing more event Than is available");
            }
        }

        virtual public void Disable(bool goDeep=true)
        { 
            this.Enabled=false;
            if (goDeep)
            { 
                DisableSubEvents(AllSubEvents); 
            }
        }

        virtual public void Enable(bool goDeep = false)
        {
            this.Enabled = true;
            if (goDeep)
            {
                EnableSubEvents(AllSubEvents);
            }
        }


        internal void incrementCompleteCount(TimeSpan span)
        {
            int CurrentCount = CompletedCount;
            CurrentCount += 1;
            if ((CurrentCount + DeletedCount) <= Splits)
            {
                CompletedCount = CurrentCount;
                _UsedTime += span;
                return;
            }

            throw new Exception("You are Completing more tasks Than is available");

        }

        virtual public void SetCompletion(bool CompletionStatus, bool goDeep=false)
        {
            Complete = CompletionStatus;

            if (RepetitionStatus)
            {
                if (goDeep)
                {
                    IEnumerable<CalendarEvent> AllrepeatingCalEvents=EventRepetition.RecurringCalendarEvents();
                    foreach (CalendarEvent eachCalendarEvent in AllrepeatingCalEvents)
                    {
                        eachCalendarEvent.SetCompletion(CompletionStatus, goDeep);
                    }
                }
            }
            else
            {
                if(goDeep)
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


            UiParams.setCompleteUI(CompletionStatus);
        }
        
        /// <summary>
        /// Pauses a sub event in the calendar event
        /// </summary>
        /// <param name="SubEventId"></param>
        /// <param name="CurrentTime"></param>
        virtual public void PauseSubEvent(EventID SubEventId, DateTimeOffset CurrentTime, EventID CurrentPausedEventId = null)
        {
            SubCalendarEvent SubEvent =  getSubEvent(SubEventId);
            if (!SubEvent.getRigid)
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
            return this.getId+"::"+this.Start.ToString() + " - " + this.End.ToString();
        }


        public bool IsDateTimeWithin(DateTimeOffset DateTimeEntry)
        {
            return RangeTimeLine.IsDateTimeWithin(DateTimeEntry);
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
            int CurrentCount = CompletedCount - NumberToBeNonComplete;
            if (CurrentCount >=0)
            {
                CompletedCount = CurrentCount;
                mySubEvents.Where(obj => obj.getIsComplete).AsParallel().ForAll(obj => obj.nonCompleteWithoutUpdatingCalEvent());
                return;
            }
            throw new Exception("You are trying to complete more events than are avalable splits, check nonCompleteSubEvents");
        }

        public void completeSubEvents(IEnumerable<SubCalendarEvent> mySubEvents)
        {
            int NumberToBeEComplete = mySubEvents.Where(obj => !obj.getIsComplete).Count();
            int CurrentCount = CompletedCount + NumberToBeEComplete;

            if (CurrentCount <= (Splits-DeletedCount))
            {
                CompletedCount = CurrentCount;
                mySubEvents.Where(obj => !obj.getIsComplete).AsParallel().ForAll(obj => obj.completeWithoutUpdatingCalEvent());
                return;
            }

            throw new Exception("You are trying to complete more events than are avalable splits, check nonCompleteSubEvent");
        }

        public void EnableSubEvents(IEnumerable<SubCalendarEvent> mySubEvents)
        {
            int NumberToBeEnabled = mySubEvents.Where(obj => !obj.isEnabled).Count();
            int CurrentCount =DeletedCount - NumberToBeEnabled;

            if (CurrentCount >=0)
            {
                DeletedCount = CurrentCount;
                mySubEvents.Where(obj => !obj.isEnabled).AsParallel().ForAll(obj => obj.enableWithouUpdatingCalEvent());
                return;
            }

            throw new Exception("You are trying to enable more events than is avalable");
        }


        public void DisableSubEvents(IEnumerable<SubCalendarEvent> mySubEvents)
        {
            int NumberToBeDeleted = mySubEvents.Where(obj => obj.isEnabled).Count();
            int CurrentCount = DeletedCount + NumberToBeDeleted;
            if (CurrentCount <= Splits)
            {
                DeletedCount = CurrentCount;
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
            StartDateTime = StartTime;
            EndDateTime = EndTime;
            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
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
            if (EventRepetition.Enable)
            {
                return EventRepetition.getCalendarEvent(CalendarIDUpToRepeatCalEvent);
            }
            else
            {
                return this;
            }
            
        }

        virtual public void SetEventEnableStatus(bool EnableDisableFlag)
        {
            this.Enabled = EnableDisableFlag;
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
            TimeSpan TimeDifference = (ReferenceEndTime- ReferenceStartTime);

            if (this.getRigid)
            {
                return null;
            }

            if (SubCalendarTimeSpan > TimeDifference)
            {
                return null;
                //throw new Exception("Oh oh check PinSubEventsToStart Subcalendar is longer than available timeline");
            }
            if ((ReferenceStartTime>this.End)||(ReferenceEndTime<this.Start))
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
        public SubCalendarEvent getSubEvent(EventID SubEventID)
        {
            int i = 0;

            if (Repeat.Enable)
            {
                IEnumerable<CalendarEvent> AllrepeatingCalEvents = EventRepetition.RecurringCalendarEvents();
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
                if (SubEvents.ContainsKey(SubEventID))
                {
                    return SubEvents[SubEventID];
                }
            }
            return null;
        }



        public virtual bool updateSubEvent(EventID SubEventID,SubCalendarEvent UpdatedSubEvent)
        {
            if (this.RepetitionStatus)
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
                if (SubEvents.ContainsKey(SubEventID))
                {
                    //SubCalendarEvent NewSubCalEvent = new SubCalendarEvent(SubEventID.ToString(), UpdatedSubEvent.Start, UpdatedSubEvent.End, UpdatedSubEvent.ActiveSlot, UpdatedSubEvent.Rigid, UpdatedSubEvent.isEnabled, UpdatedSubEvent.UIParam, UpdatedSubEvent.Notes, UpdatedSubEvent.isComplete, UpdatedSubEvent.myLocation, this.RangeTimeLine);
                    SubCalendarEvent CurrentSubEvent = SubEvents[SubEventID];
                    CurrentSubEvent.UpdateThis(UpdatedSubEvent);
                    
                    
                    //NewSubCalEvent.ThirdPartyID = CurrentSubEvent.ThirdPartyID;
                    //SubEvents[SubEventID] = NewSubCalEvent;//using method as opposed to the UpdateThis function because of the canexistwithintimeline function test in the UpdateThis function
                    return true;
                }
            }
            return false;
        }

        public virtual List<TimeLine> getInterferringWithTimeLine(TimeLine timeLine)
        {
            TimeLine interFerringTimeLine = this.RangeTimeLine.InterferringTimeLine(timeLine);
            return new List<TimeLine>() { interFerringTimeLine };
        }

        public bool removeSubCalEvents(IEnumerable<SubCalendarEvent> ElementsToBeRemoved)
        {
            SubCalendarEvent[] SubCalEVentsArray= ElementsToBeRemoved.ToArray();
            bool retValue = true;
            for (int i = 0; i < SubCalEVentsArray.Length;i++)
            {
                SubCalendarEvent mySubcalEvent=SubCalEVentsArray[i];
                if (SubEvents.ContainsKey(mySubcalEvent.SubEvent_ID))
                {
                    SubEvents[mySubcalEvent.SubEvent_ID] = null;
                }
                else
                {
                    retValue = false;
                }
            }
            return retValue;
        }

        public override List<double> EvaluateTimeLines(List<TimelineWithSubcalendarEvents> timeLines, List<Tuple<Location, Location>> borderLocations = null)
        {
            List<IList<double>> multiDimensionalCalculation = new List<IList<double>>();
            List<TimelineWithSubcalendarEvents> validTimeLine = timeLines.Select(timeLine => {
                if (timeLine.doesTimeLineInterfere(this.RangeTimeLine)) {
                    return timeLine;
                }
                else {
                    return null;
                }
            }).ToList();
            TimeSpan totalAvailableSpan = TimeSpan.FromTicks(timeLines.Sum(timeLine => timeLine.TimelineSpan.Ticks));

            foreach (TimelineWithSubcalendarEvents timeline in validTimeLine)
            {
                if (timeline != null)
                {
                    List<TimeLine> interferringTImeLines = getInterferringWithTimeLine(timeline);
                    TimeSpan totalInterferringSpan = TimeSpan.FromTicks(interferringTImeLines.Sum(objTimeLine => objTimeLine.TimelineSpan.Ticks));
                    double distance = Location.calculateDistance(timeline.averageLocation, this.Location, 0);
                    double tickRatio = (double)this.getActiveDuration.Ticks / totalInterferringSpan.Ticks;
                    double occupancy = (double)timeline.Occupancy;
                    double availableSpanRatio = (double)totalInterferringSpan.Ticks / totalAvailableSpan.Ticks;
                    IList<double> dimensionsPerDay = new List<double>() { distance, tickRatio, occupancy };
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
            foreach(var coordinates in multiDimensionalCalculation)
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
            return new CalendarEvent();
        }

        virtual public void updateEventSequence()
        {
            EventSequence = new TimeLine(this.Start, this.End);

            foreach (SubCalendarEvent mySubCalendarEvent in ActiveSubEvents)
            {
                if (mySubCalendarEvent != null)
                {
                    EventSequence.MergeTimeLineBusySlots(mySubCalendarEvent.RangeTimeLine);
                }
                
            }
        }


        public virtual void UpdateThis(CalendarEvent CalendarEventEntry)
        {
            if ((this.getId == CalendarEventEntry.getId))
            {
                EventDuration=CalendarEventEntry.getActiveDuration;
                _Name=CalendarEventEntry._Name;
                StartDateTime=CalendarEventEntry.StartDateTime;
                EndDateTime=CalendarEventEntry.EndDateTime;
                EventPreDeadline=CalendarEventEntry.getPreDeadline;
                PrepTime=CalendarEventEntry.PrepTime;
                Priority=CalendarEventEntry.Priority;
                EventRepetition=CalendarEventEntry.EventRepetition;
                Complete = CalendarEventEntry.Complete;
                RigidSchedule = CalendarEventEntry.RigidSchedule;
                Splits=CalendarEventEntry.Splits;
                _AverageTimePerSplit=CalendarEventEntry._AverageTimePerSplit;
                UniqueID=CalendarEventEntry.UniqueID;
                EventSequence=CalendarEventEntry.EventSequence;;
                SubEvents=CalendarEventEntry.SubEvents;
                //SchedulStatus=CalendarEventEntry.SchedulStatus;
                CalendarError = CalendarEventEntry.CalendarError;
                Enabled=CalendarEventEntry.Enabled;
                UiParams=CalendarEventEntry.UiParams;
                DataBlob=CalendarEventEntry.DataBlob;
                LocationInfo =CalendarEventEntry.LocationInfo;
                otherPartyID = CalendarEventEntry.otherPartyID;
                this._Creator = CalendarEventEntry._Creator;
                this.CalculableSubEvents = CalendarEventEntry.CalculableSubEvents;
                this.CompletedCount = CalendarEventEntry.CompletedCount;
                this.DeletedCount = CalendarEventEntry.DeletedCount;
                this.EndOfCalculation = CalendarEventEntry.EndOfCalculation;
                this.isCalculableInitialized = CalendarEventEntry.isCalculableInitialized;
                this.isUnDesignableInitialized = CalendarEventEntry.isUnDesignableInitialized;
                this.Splits = CalendarEventEntry.Splits;
                this.UnDesignables = CalendarEventEntry.UnDesignables;
                this.ProfileOfNow= CalendarEventEntry.ProfileOfNow;
                this.ProfileOfProcrastination = CalendarEventEntry.ProfileOfProcrastination;
                this.Semantics=CalendarEventEntry.Semantics;
                this.ThirdPartyFlag=CalendarEventEntry.ThirdPartyFlag;
                this.ThirdPartyTypeInfo=CalendarEventEntry.ThirdPartyTypeInfo;
                this.ThirdPartyUserIDInfo= CalendarEventEntry.ThirdPartyUserIDInfo;
                this.UserDeleted=CalendarEventEntry.UserDeleted;
                this._Users = CalendarEventEntry._Users;
                this._UsedTime= CalendarEventEntry._UsedTime;
                return;
            }
        
            throw new Exception("Invalid Calendar ID used in Update Calendar Event");    
        }
        virtual public bool shiftEvent(TimeSpan ChangeInTime, SubCalendarEvent[] UpdatedSubCalEvents)
        {
            TimeLine UpdatedTimeLine = new TimeLine(this.Start+ChangeInTime,this.End+ChangeInTime);
            bool retValue = true;
            foreach (SubCalendarEvent eachSubCalendarEvent in UpdatedSubCalEvents)//test if the updated locations for the subevents fall within the shifted timeline
            { 
                if(!(UpdatedTimeLine.IsTimeLineWithin(eachSubCalendarEvent.RangeTimeLine)))
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
            

            StartDateTime = StartDateTime + ChangeInTime;
            EndDateTime = EndDateTime + ChangeInTime;

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


        virtual public long getNumberOfDesignatedAndActiveSubEventsFromCalculables(bool forceDesignableRefresh=true)
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
            CalculableSubEvents.AsParallel().ForAll(obj => obj.resetPreferredDayIndex());
            return;
        }

        public virtual void InitialCalculationLookupDays(IEnumerable<DayTimeLine> RelevantDays)
        {
            CalculationLimitation = RelevantDays.Where(obj => obj.InterferringTimeLine(RangeTimeLine) != null).ToDictionary(obj => obj.UniversalIndex, obj => obj);
            FreeDaysLimitation=CalculationLimitation.ToDictionary(obj => obj.Key, obj => obj.Value);
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
                foreach(ulong eachIndex in AllUniversalDayIndexes )
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

        public List<DayTimeLine> getDaysOnOrAfterProcrastination(bool forceUpdateFreeTimeLine = true, List<DayTimeLine> AllFreeDayTIme = null)
        {

            AllFreeDayTIme = AllFreeDayTIme ?? CalculationLimitation.Values.ToList();
            if (forceUpdateFreeTimeLine)
            {
                AllFreeDayTIme.AsParallel().ForAll(obj => { obj.updateOccupancyOfTimeLine(); });
            }
            
            List<DayTimeLine> retValue = AllFreeDayTIme.Where(obj => obj.UniversalIndex >= getProcrastinationInfo.PreferredDayIndex).ToList();
            return retValue;
        }

        public List<DayTimeLine> getTimeLineWithoutMySubEventsAndEnoughDuration(bool forceUpdateFreeTimeLine = true, List<DayTimeLine> AllFreeDayTIme = null)
        {

            AllFreeDayTIme = AllFreeDayTIme ?? FreeDaysLimitation.Values.ToList();
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

            List<DayTimeLine> retValue  = AllFreeDayTIme.Where(obj => obj.Occupancy <= occupancy).ToList();
            return retValue;
        }

        public List<DayTimeLine> getTimeLineWithEnoughDuration(bool forceUpdateFreeTimeLine = true, List<DayTimeLine> AllFreeDayTIme=null)
        {

            AllFreeDayTIme = AllFreeDayTIme ?? CalculationLimitation.Values.ToList();
            if (forceUpdateFreeTimeLine)
            {
                AllFreeDayTIme.AsParallel().ForAll(obj => { obj.updateOccupancyOfTimeLine(); });
            }

            List<DayTimeLine> retValue = AllFreeDayTIme.Where(obj => obj.TotalFreeSpotAvailable > _AverageTimePerSplit).Where(dayTimeLine => {
                TimeLine timeLine = dayTimeLine.InterferringTimeLine(this.RangeTimeLine);
                if(timeLine !=null)
                {
                    return timeLine.TimelineSpan >= _AverageTimePerSplit;
                }
                return false;
            }) .ToList();
            return retValue;
        }


        public static long getUsableDaysTotal(IEnumerable<CalendarEvent> AllCalendarEvents)
        {
            long retValue = AllCalendarEvents.Sum(obj => obj.CalculationLimitation.Count);
            return retValue;
        }

        public static long getTotalUndesignatedEvents(IEnumerable<CalendarEvent> AllCalendarEvents)
        {
            List<SubCalendarEvent> UnassignedEvents = AllCalendarEvents.Where(obj => !obj.getRigid).SelectMany(obj => obj.UnDesignables).ToList();
            long retValue = AllCalendarEvents.Where(obj=>!obj.getRigid).Sum(obj => obj.UnDesignables.Count);
            return retValue;
        }

        public static List<CalendarEvent> removeCalEventsWitNoUndesignablesFromCalculables(IEnumerable<CalendarEvent> AllCalendarEvents, bool forceDesignableRefresh = true)
        {
            List<CalendarEvent> retValue;// AllCalendarEvents.Where(obj => obj.UnDesignables.Count > 0).ToList();
            if (forceDesignableRefresh)
            {
                AllCalendarEvents.AsParallel().ForAll(obj=>obj.updatedUnDesignated());
            }
            retValue = AllCalendarEvents.Where(obj => obj.UnDesignables.Count > 0).ToList();
            return retValue;
        }

        virtual protected CalendarEvent getCalculationCopy()
        {
            CalendarEvent RetValue = new CalendarEvent();
            RetValue.EventDuration = this.getActiveDuration;
            RetValue._Name = this._Name.createCopy();
            RetValue.StartDateTime = this.Start;
            RetValue.EndDateTime = this.End;
            RetValue.EventPreDeadline = this.getPreDeadline;
            RetValue.PrepTime = this.getPreparation;
            RetValue.Priority = this.getEventPriority;
            RetValue.EventRepetition = this.Repeat;// EventRepetition != this.null ? EventRepetition.CreateCopy() : EventRepetition;
            RetValue.Complete = this.getIsComplete;
            RetValue.RigidSchedule = this.getRigid;//hack
            RetValue.Splits = this.NumberOfSplit;
            RetValue._AverageTimePerSplit = this.AverageTimeSpanPerSubEvent;
            RetValue.UniqueID = EventID.GenerateCalendarEvent();
            RetValue.SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            RetValue.UiParams = this.getUIParam;
            RetValue.DataBlob = this.Notes;
            RetValue.Enabled = this.isEnabled;
            RetValue.isRestricted = this.getIsEventRestricted;
            RetValue.LocationInfo = this.myLocation;//hack you might need to make copy
            RetValue.ProfileOfProcrastination = this.getProcrastinationInfo.CreateCopy();
            RetValue.DeadlineElapsed = this.getIsDeadlineElapsed;
            RetValue.UserDeleted = this.getIsUserDeleted;
            RetValue.CompletedCount = this.CompletionCount;
            RetValue.DeletedCount = this.DeletionCount;
            RetValue.ProfileOfNow = this.ProfileOfNow.CreateCopy();
            RetValue.otherPartyID = this.ThirdPartyID;// == this.null ? null : otherPartyID.ToString();
            RetValue._Users = this._Users;
            //RetValue.UpdateLocationMatrix(RetValue.LocationInfo);
            return RetValue;
        }

        virtual public CalendarEvent getProcrastinationCopy(Procrastination ProcrastinationProfileData)
        {
            CalendarEvent retValue = getCalculationCopy();
            retValue.ProfileOfProcrastination=ProcrastinationProfileData;
            retValue.StartDateTime = ProcrastinationProfileData.PreferredStartTime;
            retValue.EventSequence = new TimeLine(retValue.StartDateTime, retValue.EndDateTime);
            List<SubCalendarEvent> ProcrastinatonCopy = this.ActiveSubEvents.Select(obj => obj.getProcrastinationCopy(retValue, ProcrastinationProfileData)).ToList();
            ProcrastinatonCopy.ForEach(obj => retValue.SubEvents.Add(obj.SubEvent_ID, obj));
            //retValue.SubEvents.Add(ProcrastinatonCopy.SubEvent_ID, ProcrastinatonCopy);
            return retValue;
        }

        virtual public CalendarEvent getNowCalculationCopy(NowProfile NowProfileData )
        {
            CalendarEvent retValue = getCalculationCopy();
            retValue.ProfileOfNow = NowProfileData;
            retValue.StartDateTime = NowProfileData.PreferredTime;
            retValue.EventSequence = new TimeLine(retValue.StartDateTime, retValue.EndDateTime);
            SubCalendarEvent ProcrastinatonCopy = this.ActiveSubEvents[0].getNowCopy(retValue.UniqueID, NowProfileData);
            retValue.EndDateTime = ProcrastinatonCopy.End;
            retValue.RigidSchedule = true;
            retValue.SubEvents.Add(ProcrastinatonCopy.SubEvent_ID, ProcrastinatonCopy);
            return retValue;
        }

        public short updateNumberOfSplits(int SplitCOunt)
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

                    if (RepetitionStatus)
                    {
                        EventRepetition.RecurringCalendarEvents().AsParallel().ForAll(obj => obj.IncreaseSplitCount(Change));
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
                    if (RepetitionStatus)
                    {
                        EventRepetition.RecurringCalendarEvents().AsParallel().ForAll(obj => obj.ReduceSplitCount(Change));
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
            if (delta<Splits)
            {
                List<SubCalendarEvent> orderedByActive = SubEvents.OrderByDescending(subEvent => subEvent.Value.isActive).Select(subEvemt => subEvemt.Value).ToList();
                for (int i=0; i<delta;i++)
                {
                    SubCalendarEvent SubEvent = orderedByActive.First();
                    if (SubEvent.getIsComplete)
                    {
                        decrementCompleteCount(SubEvent.RangeSpan);
                    } else if (SubEvent.getIsDeleted)
                    {
                        decrementDeleteCount(SubEvent.RangeSpan);
                    }
                    SubEvents.Remove(SubEvent.SubEvent_ID);
                    EventDuration = EventDuration.Add(-SubEvent.getActiveDuration);
                    orderedByActive.RemoveAt(0);
                }
                Splits -= (int)delta;
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
                SubCalendarEvent newSubCalEvent = new SubCalendarEvent(getCreator, _Users,_TimeZone, _AverageTimePerSplit, this.getName, (EndDateTime - _AverageTimePerSplit), this.End, new TimeSpan(), UniqueID.ToString(), RigidSchedule, this.isEnabled, this.UiParams, this.Notes, this.Complete, LocationInfo, this.RangeTimeLine);
                SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
                EventDuration = EventDuration.Add(newSubCalEvent.getActiveDuration);
            }
            Splits += (int)delta;
            UpdateTimePerSplit();
        }

        virtual protected void IncreaseSplitCount(uint delta, IEnumerable<SubCalendarEvent> subEvents)
        {
            List<SubCalendarEvent> newSubs = subEvents.ToList();
            for (int i = 0; i < delta; i++)
            {
                SubCalendarEvent newSubCalEvent = newSubs[i];
                SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
            }
            Splits += (int)delta;
            UpdateTimePerSplit();
        }

        public virtual short UpdateTimePerSplit()
        {
            short retValue = 0;
            EventDuration = TimeSpan.FromTicks(SubEvents.Values.Sum(subEvent => subEvent.getActiveDuration.Ticks));
            _AverageTimePerSplit = TimeSpan.FromTicks(EventDuration.Ticks / Splits);
            return retValue;
        }

        public void updateUnusableDaysAndRemoveDaysWithInsufficientFreeSpace()
        {
            updateUnusableDays();
            removeDayTimeLinesWithInsufficientSpace();
        }
        

        public void removeDayTimeLinesWithInsufficientSpace()
        {
            List<DayTimeLine> DaysWithInSufficientSpace=CalculationLimitation.Values.Where(obj => obj.TotalFreeSpotAvailable < _AverageTimePerSplit).ToList();
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
            if(isUnDesignableInitialized)
            {
                UnDesignables.RemoveWhere(obj => obj.isDesignated);
                return;
            }

            throw new Exception("You haven't initialized undesignated");
        }
        

        virtual public void updateTimeLine(TimeLine newTImeLine)
        {
            TimeLine oldTimeLine = new TimeLine(StartDateTime, EndDateTime);
            AllSubEvents.AsParallel().ForAll(obj => obj.changeTimeLineRange(newTImeLine));
            bool worksForAllSubevents = true;
            SubCalendarEvent failingSubEvent = SubCalendarEvent.getEmptySubCalendarEvent(this.Calendar_EventID);
            foreach(var obj in AllSubEvents)
            {
                if (!obj.canExistWithinTimeLine(newTImeLine))
                {
                    worksForAllSubevents = false;
                    failingSubEvent = obj;
                }
            }
            if(worksForAllSubevents)
            {
                StartDateTime = newTImeLine.Start;
                EndDateTime = newTImeLine.End;
                if (this.getRigid)
                {
                    EventDuration = EndDateTime - StartDateTime;
                }
            } else
            {
                AllSubEvents.AsParallel().ForAll(obj => obj.changeTimeLineRange(oldTimeLine));
                CustomErrors customError = new CustomErrors("Cannot update the timeline for the calendar event with sub event " + failingSubEvent.getId + ". Most likely because the new time line won't fit the sub event", 40000001);
                throw  customError;
            }
        }

        virtual public void updateTimeLine(SubCalendarEvent subEvent,  TimeLine newTImeLine)
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
            foreach(SubCalendarEvent subEvent in this.AllSubEvents)
            {
                subEvent.updateEventName(NewName);
            }
            if (!justThisCalendarEvent && getIsRepeat)
            {
                foreach(CalendarEvent calEvent in Repeat.RecurringCalendarEvents().Where(obj => obj.getId != this.getId))
                {
                    calEvent.updateEventName(NewName, true);
                }
            }
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

        
        public TimeSpan TimeLeftBeforeDeadline
        {
            get
            {
                return EndDateTime - DateTimeOffset.UtcNow;
            }
        }

        public int NumberOfSplit
        {
            get
            {
                return Splits;
            }
        }
        
        public virtual bool RepetitionStatus
        {
            get
            {
                return EventRepetition.Enable;
            }
        }
        public Repetition Repeat
        {
            get
            {
                return EventRepetition;
            }
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
                return EventDuration;
            }
        }
        public SubCalendarEvent[] ActiveSubEvents//needs to update to get only active events
        {//return All Subcalevents that are enabled. returns 
            get
            {
                if (RepetitionStatus)
                {
                    return this.ActiveRepeatSubCalendarEvents;
                }
                
                IEnumerable<SubCalendarEvent> AllSubEvents =SubEvents.Values.Where(obj => obj != null).Where(obj=>(obj.isActive));

                return AllSubEvents.ToArray();
            }
        }



        public SubCalendarEvent[] EnabledSubEvents//needs to update to get only active events
        {//return All Subcalevents that are enabled.
            get
            {
                if (RepetitionStatus)
                {
                    return this.ActiveRepeatSubCalendarEvents;
                }

                IEnumerable<SubCalendarEvent> AllSubEvents = SubEvents.Values.Where(obj => obj != null).Where(obj => (obj.isEnabled));

                return AllSubEvents.ToArray();
            }
        }


        public virtual SubCalendarEvent[] AllSubEvents
        {//return All Subcalevents that enabled or not.
            get
            {
                if (this.Repeat.Enable)
                {
                    return this.Repeat.RecurringCalendarEvents().SelectMany(obj => obj.AllSubEvents).ToArray();
                }

                return SubEvents.Values.Where(obj=>obj!=null).ToArray();
            }
        }

        virtual public Location myLocation
        {
            set
            {
                LocationInfo=value;
            }
            get
            {
                return LocationInfo;
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
                if (this.Repeat.Enable)
                {
                    return this.Repeat.RecurringCalendarEvents().SelectMany(obj => obj.ActiveSubEvents).ToArray();
                    /*foreach (CalendarEvent RepeatingElement in this.EventRepetition.RecurringCalendarEvents)
                    {
                        var HolderConcat = MyRepeatingSubCalendarEvents.Concat(RepeatingElement.AllActiveSubEvents.ToList());
                        MyRepeatingSubCalendarEvents = HolderConcat.ToList();
                    }
                    return MyRepeatingSubCalendarEvents.ToArray();*/
                }
              
                return MyRepeatingSubCalendarEvents.ToArray();
            
            }
        
        }

        public SubCalendarEvent[] EnabledRepeatSubCalendarEvents
        {
            get
            {
                List<SubCalendarEvent> MyRepeatingSubCalendarEvents = new List<SubCalendarEvent>();
                if (this.Repeat.Enable)
                {
                    return this.Repeat.RecurringCalendarEvents().SelectMany(obj => obj.EnabledSubEvents).ToArray();
                }

                return MyRepeatingSubCalendarEvents.ToArray();

            }

        }
        override public TimeLine RangeTimeLine
        {
            get
            {
                //updateEventSequence();
                EventSequence = new TimeLine(this.Start, this.End);
                return EventSequence;
            }
        }

        public void InitializeCounts(int Deletion, int Completion)
        {
            DeletedCount = Deletion;
            CompletedCount = Completion;
        }

        public void UpdateError(CustomErrors Error)
        {
            CalendarError = Error;
        }

        public void ClearErrorMessage()
        {
            CalendarError = new CustomErrors(string.Empty);
        }

        public TimeSpan RangeSpan
        {
            get
            {
                return this.RangeTimeLine.TimelineSpan;
            }
        }

        


        virtual public int CompletionCount
        {
            get 
            {
                return CompletedCount;
            }
        }

        virtual public int DeletionCount
        {
            get
            {
                return DeletedCount;
            }
        }

        

        virtual public MiscData Notes
        {
            get
            {
                return DataBlob;
            }
        }

        /*
        virtual public NowProfile NowProfile
        {
            get
            {
                return ProfileOfNow;
            }
        }
        */

        #endregion

    }
}
