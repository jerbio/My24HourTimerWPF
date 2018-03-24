using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace TilerElements
{
    public class SubCalendarEvent : TilerEvent,IDefinedRange
    {
        public static DateTimeOffset InitialPauseTime  = new DateTimeOffset();
        protected BusyTimeLine BusyFrame;
        protected BusyTimeLine NonHumaneTimeLine= new BusyTimeLine();
        protected BusyTimeLine HumaneTimeLine = new BusyTimeLine();
        TimeSpan AvailablePreceedingFreeSpace;
        protected TimeLine CalendarEventRange;
        protected double EventScore;
        protected ConflictProfile ConflictingEvents = new ConflictProfile();
        protected ulong preferredDayIndex=0;
        protected int MiscIntData;
        protected bool Vestige = false;
        protected ulong UnUsableIndex;
        protected ulong OldPreferredIndex;
        protected bool CalculationMode = false;
        protected DateTimeOffset _PauseTime = InitialPauseTime;
        protected bool BlobEvent = false;
        protected bool OptimizationFlag = false;
        protected List<Reason> TimePositionReasons = new List<Reason>();
        protected DateTimeOffset _LastReasonStartTimeChanged;
        protected TimeLine CalculationTimeLine = null;
        public TimeSpan TravelTimeBefore { get; set; } = new TimeSpan(-1);
        public TimeSpan TravelTimeAfter { get; set; } = new TimeSpan(-1);
        public bool isWake { get; set; } = false;
        public bool isSleep { get; set; } = false;
        protected bool tempLock { get; set; } = false;
        /// <summary>
        /// This holds the current session reasons. It will updated based on data and calculation optimizations from HistoricalCurrentPosition
        /// </summary>
        protected Dictionary<TimeSpan, List<Reason>> ReasonsForCurrentPosition = new Dictionary<TimeSpan, List<Reason>>();
        /// <summary>
        /// Will hold the reasons that were collated from the last time the schedule was modified. This is to be only loaded from storage and not to be updated
        /// </summary>
        protected Dictionary<TimeSpan, List<Reason>> HistoricalReasonsCurrentPosition = new Dictionary<TimeSpan, List<Reason>>();

        #region Classs Constructor
        public SubCalendarEvent()
        { }

        public SubCalendarEvent(TilerUser Creator, TilerUserGroup users, string timeZone, TimeSpan Event_Duration, EventName name, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, bool Enabled, EventDisplay UiParam,MiscData Notes,bool completeFlag, Location EventLocation =null, TimeLine calendarEventRange = null, ConflictProfile conflicts=null)
        {
            if (EventDeadline < EventStart)
            {
                throw new Exception("SubCalendar Event cannot have an end time earlier than the start time");
            }
            _Name = name;
            _Creator = Creator;
            _Users = users;
            _TimeZone = timeZone;
            if (conflicts == null)
            {
                conflicts = new ConflictProfile();
            }
            ConflictingEvents = conflicts;
            CalendarEventRange = calendarEventRange;
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = Event_Duration;
            PrepTime = EventPrepTime;
            if (myParentID == "16")
            {
                ;
            }
            UiParams=UiParam;
            DataBlob = Notes;
            Complete=completeFlag;
            UniqueID = EventID.GenerateSubCalendarEvent(myParentID);
            BusyFrame = new BusyTimeLine(this.getId, StartDateTime, EndDateTime);//this is because in current implementation busy frame is the same as CalEvent frame
            this.LocationInfo = EventLocation;
//            EventSequence = new EventTimeLine(UniqueID.ToString(), StartDateTime, EndDateTime);
            RigidSchedule = Rigid;
            this.Enabled = Enabled;
            _LastReasonStartTimeChanged = this.Start;
        }


        public SubCalendarEvent(TilerUser Creator, TilerUserGroup users, string timeZone, string MySubEventID, EventName name, BusyTimeLine MyBusylot, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, bool Enabled, EventDisplay UiParam, MiscData Notes, bool completeFlag, Location EventLocation = null, TimeLine calendarEventRange = null, ConflictProfile conflicts = null)
        {
            if (EventDeadline < EventStart)
            {
                throw new Exception("SubCalendar Event cannot have an end time earlier than the start time");
            }
            _TimeZone = timeZone;
            _Name = name;
            _Creator = Creator;
            _Users = users;
            if (conflicts == null)
            {
                conflicts = new ConflictProfile();
            }
            ConflictingEvents = conflicts;
            CalendarEventRange = calendarEventRange;
            //string eventName, TimeSpan EventDuration, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, TimeSpan PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = MyBusylot.End - MyBusylot.Start;
            BusyFrame = MyBusylot;
            PrepTime = EventPrepTime;
            UniqueID = new EventID(MySubEventID);
            this.LocationInfo = EventLocation;
            
            UiParams=UiParam;
            DataBlob= Notes;
            Complete = completeFlag;

            this.Enabled = Enabled;
            //EventSequence = new EventTimeLine(UniqueID.ToString(), StartDateTime, EndDateTime);
            RigidSchedule = Rigid;
            _LastReasonStartTimeChanged = this.Start;
        }

        public SubCalendarEvent(TilerUser Creator, TilerUserGroup users, string timeZone, string MySubEventID, EventName name, DateTimeOffset EventStart, DateTimeOffset EventDeadline, BusyTimeLine SubEventBusy, bool Rigid, bool Enabled, EventDisplay UiParam, MiscData Notes, bool completeFlag, Location EventLocation = null, TimeLine calendarEventRange = null, ConflictProfile conflicts = null)
        {
            if (EventDeadline < EventStart)
            {
                throw new Exception("SubCalendar Event cannot have an end time earlier than the start time");
            }
            _TimeZone = timeZone;
            _Name = name;
            _Creator = Creator;
            _Users = users;
            if (conflicts == null)
            {
                conflicts = new ConflictProfile();
            }
            ConflictingEvents = conflicts;
            CalendarEventRange = calendarEventRange;
            UniqueID = new EventID(MySubEventID);
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = SubEventBusy.TimelineSpan;
            BusyFrame = SubEventBusy;
            RigidSchedule = Rigid;
            this.Enabled = Enabled;
            this.LocationInfo = EventLocation;
            UiParams = UiParam;
            DataBlob = Notes;
            Complete = completeFlag;
            _LastReasonStartTimeChanged = this.Start;
        }
        #endregion

        #region Class functions

        public override string ToString()
        {
            return this.Start.ToString() + " - " + this.End.ToString() + "::" + this.getId + "\t\t::" + this.getActiveDuration.ToString();
        }

        public virtual void updateCalculationEventRange(TimeLine timeLine)
        {
            TimeLine interferringTimeLine = this.getCalendarEventRange.InterferringTimeLine(timeLine);
            if(interferringTimeLine == null)
            {
                this.CalculationTimeLine = timeLine;
            }
            else
            {
                this.CalculationTimeLine = interferringTimeLine;
            }
        }

        public void disable(CalendarEvent myCalEvent)
        {
            if (this.Enabled)
            {
                this.Enabled = false;
                myCalEvent.incrementDeleteCount(this.RangeSpan);
            }
        }

        internal void disableWithoutUpdatingCalEvent()
        {
            this.Enabled = false;
        }

        public void complete(CalendarEvent myCalEvent)
        {
            if (!this.Complete)
            {
                this.Complete = true;
                myCalEvent.incrementCompleteCount(this.RangeSpan);
            }
        }

        public void nonComplete(CalendarEvent myCalEvent)
        {
            if (this.Complete)
            {
                this.Complete = false;
                myCalEvent.decrementCompleteCount(this.RangeSpan);
            }
            
        }

        internal void completeWithoutUpdatingCalEvent()
        {
            this.Complete = true;
        }

        internal void nonCompleteWithoutUpdatingCalEvent()
        {
            this.Complete = false;
        }

        public void Enable(CalendarEvent myCalEvent)
        {
            if (!this.Enabled)
            {
                this.Enabled = true;
                myCalEvent.decrementDeleteCount(this.RangeSpan);
            }
        }

        internal void enableWithouUpdatingCalEvent()
        {
            this.Enabled = true;
        }

        public void resetPreferredDayIndex()
        {
            preferredDayIndex = 0;
        }

        public void updateDayIndex(CalendarEvent myCalEvent)
        {
            preferredDayIndex = ReferenceNow.getDayIndexFromStartOfTime(StartDateTime);
            myCalEvent.removeDayTimeFromFreeUpdays(preferredDayIndex);
        }

        public void SetCompletionStatus(bool completeValue,CalendarEvent myCalendarEvent)
        {
            if (completeValue != Complete)
            {
                if (completeValue)
                {
                    complete(myCalendarEvent);
                }
                else
                {
                    nonComplete(myCalendarEvent);
                }
            }
            UiParams.setCompleteUI(Complete);
        }


        public void tempLockSubEvent()
        {
            tempLock = true;
        }

        public void resetTempUnlock()
        {
            RigidSchedule = true;
        }

        virtual public void addReasons(Reason eventReason)
        {
            TimeSpan TimeDelta = this.Start - _LastReasonStartTimeChanged;
            if (!ReasonsForCurrentPosition.ContainsKey(TimeDelta))
            {
                ReasonsForCurrentPosition.Add(TimeDelta, new List<Reason>());
            }

            ReasonsForCurrentPosition[TimeDelta].Add(eventReason);
            TimePositionReasons.Add(eventReason);
            _LastReasonStartTimeChanged = this.Start;
        }

        virtual public void clearAllReasons()
        {
            ReasonsForCurrentPosition = new Dictionary<TimeSpan, List<Reason>>();
        }
        override public IWhy Because()
        {
            throw new NotImplementedException("Yet to implement a because functionality for subcalendar event");
        }

        override public IWhy OtherWise()
        {
            throw new NotImplementedException("Yet to implement a OtherWise functionality for subcalendar event");
        }

        virtual public IWhy WhatIfDeadline(DateTimeOffset AssumedTime)
        {
            throw new NotImplementedException("Yet to implement a WhatIf functionality for subcalendar event");
        }

        virtual public IWhy WhatIfStartTime(DateTimeOffset AssumedTime)
        {
            throw new NotImplementedException("Yet to implement a WhatIf functionality for subcalendar event");
        }

        override public IWhy WhatIf(params Reason[] reasons)
        {
            throw new NotImplementedException("Yet to implement a WhatIf functionality for subcalendar event");
        }

        virtual public bool IsDateTimeWithin(DateTimeOffset DateTimeEntry)
        {
            return RangeTimeLine.IsDateTimeWithin(DateTimeEntry);
        }
        /// <summary>
        /// Function Subcalendarevent evaluates itself against the given parameters
        /// </summary>
        /// <param name="refLocation"></param>
        /// <param name="DayReference"></param>
        /// <returns></returns>
        virtual public Tuple<TimeLine,Double> evaluateAgainstOptimizationParameters(Location refLocation, TimeLine DayTimeLine)
        {
            
            double distance = Location.calculateDistance(refLocation,this.Location);
            TimeLine refTimeLine = new TimeLine(DayTimeLine.Start, CalendarEventRange.End);
            Tuple<TimeLine, double> retValue = new Tuple<TimeLine, double>(refTimeLine,distance);
            return retValue;
        }

        public static SubCalendarEvent getEmptySubCalendarEvent(EventID CalendarEventId)
        {
            SubCalendarEvent retValue = new SubCalendarEvent();
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventId.ToString());
            retValue.StartDateTime = DateTimeOffset.UtcNow;
            retValue.EndDateTime = DateTimeOffset.UtcNow;
            retValue.EventDuration = new TimeSpan(0);
            
            retValue.RigidSchedule= true;
            retValue.Complete = true;
            retValue.Enabled = false;
            return retValue;
        }

        virtual public void setAsOptimized()
        {
            OptimizationFlag = true;
        }

        virtual public void setAsUnOptimized()
        {
            OptimizationFlag = false;
        }


        virtual public SubCalendarEvent createCopy(EventID eventId)
        {
            string Id;
            if (eventId != null)
            {
                Id = eventId.ToString();
            }
            else
            {
                Id = this.getId;
            }
            SubCalendarEvent MySubCalendarEventCopy = new SubCalendarEvent(getCreator, _Users, this._TimeZone, Id, this.getName.createCopy(), Start, End, BusyFrame.CreateCopy(), this.RigidSchedule, this.isEnabled, this.UiParams?.createCopy(), this.Notes?.createCopy(), this.Complete, this.LocationInfo, new TimeLine(CalendarEventRange.Start, CalendarEventRange.End), ConflictingEvents?.CreateCopy());
            MySubCalendarEventCopy.ThirdPartyID = this.ThirdPartyID;
            MySubCalendarEventCopy.DeadlineElapsed = this.DeadlineElapsed;
            MySubCalendarEventCopy.UserDeleted = this.UserDeleted;
            MySubCalendarEventCopy.isRestricted = this.isRestricted;
            MySubCalendarEventCopy.preferredDayIndex = this.preferredDayIndex;
            MySubCalendarEventCopy._Creator = this._Creator;
            MySubCalendarEventCopy.Semantics = this.Semantics.createCopy();
            MySubCalendarEventCopy._UsedTime = this._UsedTime;
            MySubCalendarEventCopy.OptimizationFlag = this.OptimizationFlag;
            MySubCalendarEventCopy._LastReasonStartTimeChanged = this._LastReasonStartTimeChanged;
            MySubCalendarEventCopy.DaySectionPreference = this.DaySectionPreference;
            MySubCalendarEventCopy.TravelTimeAfter = this.TravelTimeAfter;
            MySubCalendarEventCopy.TravelTimeBefore= this.TravelTimeBefore;
            MySubCalendarEventCopy.isSleep = this.isSleep;
            MySubCalendarEventCopy.isWake = this.isWake;
            MySubCalendarEventCopy.userLocked = this._userLocked;
            MySubCalendarEventCopy.tempLock = this.tempLock;
            if (this.CalculationTimeLine != null)
            {
                MySubCalendarEventCopy.CalculationTimeLine = this.CalculationTimeLine.CreateCopy();
            }
            
            return MySubCalendarEventCopy;
        }

        public void updateDayIndex(ulong DayIndex)
        {
            this.preferredDayIndex = DayIndex;
        }

        public static void updateDayIndex(ulong DayIndex, IEnumerable<SubCalendarEvent> AllSUbevents)
        {
            foreach (SubCalendarEvent eachSubCalendarEvent in AllSUbevents)
            {
                eachSubCalendarEvent.preferredDayIndex = DayIndex;
            }
        }

        public void setScore(double score)
        {
            EventScore = score;
        }

        public void incrementScore(double score)
        {
            EventScore += score;
        }

        public static void resetScores(IEnumerable<SubCalendarEvent> AllSUbevents)
        {
            AllSUbevents.AsParallel().ForAll(obj => obj.EventScore = 0);
        }

        public static TimeSpan TotalActiveDuration(IEnumerable<SubCalendarEvent> ListOfSubCalendarEvent)
        {
            TimeSpan TotalTimeSpan = new TimeSpan(0);
            
            foreach (SubCalendarEvent mySubCalendarEvent in ListOfSubCalendarEvent)
            {
                TotalTimeSpan=TotalTimeSpan.Add(mySubCalendarEvent.getActiveDuration);
            }

            return TotalTimeSpan;
        }

        
        virtual public bool PinToStart(TimeLine MyTimeLine)
        {
            DateTimeOffset ReferenceStartTime = new DateTimeOffset();
            DateTimeOffset ReferenceEndTime = new DateTimeOffset();

            ReferenceStartTime = MyTimeLine.Start;
            if (this.getCalculationRange.Start > MyTimeLine.Start)
            {
                ReferenceStartTime = this.getCalculationRange.Start;
            }

            ReferenceEndTime = this.getCalculationRange.End;
            if (this.getCalculationRange.End > MyTimeLine.End)
            {
                ReferenceEndTime = MyTimeLine.End;
            }

            /*foreach (SubCalendarEvent MySubCalendarEvent in MySubCalendarEventList)
            {
                SubCalendarTimeSpan = SubCalendarTimeSpan.Add(MySubCalendarEvent.ActiveDuration);//  you might be able to combine the implementing for lopp with this in order to avoid several loops
            }*/
            TimeSpan TimeDifference = (ReferenceEndTime - ReferenceStartTime);

            if (this.isLocked)
            {
                return (MyTimeLine.IsTimeLineWithin( this.RangeTimeLine));
            }

            if (this.EventDuration > TimeDifference)
            {
                return false;
                //throw new Exception("Oh oh check PinSubEventsToStart Subcalendar is longer than available timeline");
            }
            if ((ReferenceStartTime > this.getCalculationRange.End) || (ReferenceEndTime < this.getCalculationRange.Start))
            {
                return false;
                //throw new Exception("Oh oh Calendar event isn't Timeline range. Check PinSubEventsToEnd :(");
            }

            List<BusyTimeLine> MyActiveSlot = new List<BusyTimeLine>();
            //foreach (SubCalendarEvent MySubCalendarEvent in MySubCalendarEventList)
            
                this.StartDateTime= ReferenceStartTime;
                this.EndDateTime = this.StartDateTime + this.getActiveDuration;
                //this.ActiveSlot = new BusyTimeLine(this.ID, (this.StartDateTime), this.EndDateTime);
                TimeSpan BusyTimeLineShift = this.StartDateTime - ActiveSlot.Start;
                ActiveSlot.shiftTimeline(BusyTimeLineShift);
                return true;
        }

        virtual public bool PinToPossibleLimit(TimeLine referenceTimeLine)
        { 
            TimeLine interferringTImeLine=CalendarEventRange.InterferringTimeLine( referenceTimeLine );
            if (interferringTImeLine == null)
            {
                return false;
            }
            DateTimeOffset EarliestEndTime = CalendarEventRange.Start + getActiveDuration;
            DateTimeOffset LatestEndTime = CalendarEventRange.End;

            DateTimeOffset DesiredEndtime = interferringTImeLine.End + (TimeSpan.FromTicks(((long)(getActiveDuration - interferringTImeLine.TimelineSpan).Ticks) / 2));

            if (DesiredEndtime < EarliestEndTime)
            {
                DesiredEndtime = EarliestEndTime;
            }

            if (DesiredEndtime > LatestEndTime)
            {
                DesiredEndtime = LatestEndTime;
            }
            TimeSpan shiftInEvent = DesiredEndtime-End;
            return shiftEvent(shiftInEvent);
        }


        /// <summary>
        /// function updates the parameters of the current sub calevent using SubEventEntry. However it doesnt change some datamemebres such as rigid, and isrestricted. You 
        /// </summary>
        /// <param name="SubEventEntry"></param>
        /// <returns></returns>
        virtual public bool UpdateThis(SubCalendarEvent SubEventEntry)
        {
            if (this.getId == SubEventEntry.getId)
            {
                this.BusyFrame = SubEventEntry.ActiveSlot;
                this.CalendarEventRange = SubEventEntry.getCalendarEventRange;
                this._Name = SubEventEntry.getName;
                this.EventDuration = SubEventEntry.getActiveDuration;
                this.Complete = SubEventEntry.getIsComplete;
                this.ConflictingEvents = SubEventEntry.Conflicts;
                this.DataBlob = SubEventEntry.Notes;
                this.DeadlineElapsed = SubEventEntry.getIsDeadlineElapsed;
                this.Enabled = SubEventEntry.isEnabled;
                this.EndDateTime = SubEventEntry.End;
                this.EventPreDeadline = SubEventEntry.getPreDeadline;
                this.EventScore = SubEventEntry.Score;
                this.isRestricted = SubEventEntry.getIsEventRestricted;
                this.LocationInfo = SubEventEntry.Location;
                this.OldPreferredIndex = SubEventEntry.OldUniversalIndex;
                this.otherPartyID = SubEventEntry.ThirdPartyID;
                this.preferredDayIndex = SubEventEntry.UniversalDayIndex;
                this.PrepTime = SubEventEntry.getPreparation;
                this.Priority = SubEventEntry.getEventPriority;
                this.ProfileOfNow = SubEventEntry.ProfileOfNow;
                this.ProfileOfProcrastination = SubEventEntry.ProfileOfProcrastination;
                //this.RigidSchedule = SubEventEntry.Rigid;
                this.StartDateTime = SubEventEntry.Start;
                this.UiParams = SubEventEntry.getUIParam;
                this.UniqueID = SubEventEntry.SubEvent_ID;
                this.UserDeleted = SubEventEntry.getIsUserDeleted;
                this._Users = SubEventEntry.getAllUsers();
                this.Vestige = SubEventEntry.isVestige;
                this.otherPartyID = SubEventEntry.otherPartyID;
                this._Creator = SubEventEntry._Creator;
                this.Semantics = SubEventEntry.Semantics;
                this._UsedTime = SubEventEntry._UsedTime;
                return true;
            }

            throw new Exception("Error Detected: Trying to update SubCalendar Event with non matching ID");
        }

        virtual public SubCalendarEvent getProcrastinationCopy(CalendarEvent CalendarEventData,Procrastination ProcrastinationData )
        {
            SubCalendarEvent retValue = getCalulationCopy();
            /*
            retValue.CalendarEventRange = CalendarEventData.RangeTimeLine;
            TimeSpan SpanShift = ProcrastinationData.PreferredStartTime - retValue.Start;
            */
            retValue.CalendarEventRange = new TimeLine(ProcrastinationData.PreferredStartTime, retValue.CalendarEventRange.End);
            TimeSpan SpanShift = (retValue.CalendarEventRange.End - retValue.RangeSpan) - retValue.Start;
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventData.getId);
            retValue.shiftEvent(SpanShift,true);
            return retValue;
        }

        virtual public SubCalendarEvent getNowCopy(EventID CalendarEventID, NowProfile NowData)
        {
            SubCalendarEvent retValue = getCalulationCopy();
            retValue.RigidSchedule = true;
            TimeSpan SpanShift = NowData.PreferredTime - retValue.Start;
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventID.ToString());
            retValue.shiftEvent(SpanShift, true);
            return retValue;
        }

        virtual protected SubCalendarEvent getCalulationCopy()
        {
            SubCalendarEvent retValue = new SubCalendarEvent();
            retValue.BusyFrame = this.ActiveSlot.CreateCopy();
            retValue.CalendarEventRange = this.getCalendarEventRange.CreateCopy();
            retValue._Name = this.getName;
            retValue.EventDuration = this.getActiveDuration;
            retValue.Complete = this.getIsComplete;
            retValue.ConflictingEvents = this.Conflicts;
            retValue.DataBlob = this.Notes;
            retValue.DeadlineElapsed = this.getIsDeadlineElapsed;
            retValue.Enabled = this.isEnabled;
            retValue.EndDateTime = this.End;
            retValue.EventPreDeadline = this.getPreDeadline;
            retValue.EventScore = this.Score;
            retValue.isRestricted = this.getIsEventRestricted;
            retValue.LocationInfo = this.Location.CreateCopy();
            retValue.OldPreferredIndex = this.OldUniversalIndex;
            retValue.otherPartyID = this.ThirdPartyID;
            retValue.preferredDayIndex = this.UniversalDayIndex;
            retValue.PrepTime = this.getPreparation;
            retValue.Priority = this.getEventPriority;
            retValue.ProfileOfNow = this.ProfileOfNow;
            retValue.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            retValue.RigidSchedule = this.isLocked;
            retValue.StartDateTime = this.Start;
            retValue.UiParams = this.getUIParam;
            retValue.UniqueID = this.SubEvent_ID;
            retValue.UserDeleted = this.getIsUserDeleted;
            retValue._Users = this.getAllUsers();
            retValue.Vestige = this.isVestige;
            retValue.otherPartyID = this.otherPartyID;
            return retValue;
        }

        public static void updateMiscData(IList<SubCalendarEvent>AllSubCalendarEvents, IList<int> IntData)
        {
            if(AllSubCalendarEvents.Count!=IntData.Count)
            {
                throw new Exception("trying to update MiscData  while Subcalendar events with not matching count of intData");
            }
            else
            {
                for(int i=0;i<AllSubCalendarEvents.Count;i++)
                {
                    AllSubCalendarEvents[i].MiscIntData=IntData[i];
                }
            }
        }

        public static void incrementMiscdata(IList<SubCalendarEvent> AllSubCalendarEvents)
        {
            for (int i = 0; i < AllSubCalendarEvents.Count; i++)
            {
                ++AllSubCalendarEvents[i].MiscIntData;// = IntData[i];
            }
        }

        public static void decrementMiscdata(IList<SubCalendarEvent> AllSubCalendarEvents)
        {
            for (int i = 0; i < AllSubCalendarEvents.Count; i++)
            {
                --AllSubCalendarEvents[i].MiscIntData;// = IntData[i];
            }
        }



        public static void updateMiscData(IList<SubCalendarEvent> AllSubCalendarEvents, int IntData)
        {
            for (int i = 0; i < AllSubCalendarEvents.Count; i++)
            {
                AllSubCalendarEvents[i].MiscIntData = IntData;
            }
        }

        virtual public bool PinToEnd(TimeLine LimitingTimeLine)
        {
            DateTimeOffset ReferenceTime = this.getCalculationRange.End;
            if (ReferenceTime > LimitingTimeLine.End)
            {
                ReferenceTime = LimitingTimeLine.End;
            }

            if (this.isLocked)
            {
                return (LimitingTimeLine.IsTimeLineWithin(this.RangeTimeLine));
            }


            DateTimeOffset MyStartTime = ReferenceTime - this.EventDuration;


            if ((MyStartTime>=LimitingTimeLine.Start )&&(MyStartTime>=getCalculationRange.Start))
            {

                StartDateTime = MyStartTime;
                //ActiveSlot = new BusyTimeLine(this.ID, (MyStartTime), ReferenceTime);
                TimeSpan BusyTimeLineShift = MyStartTime - ActiveSlot.Start;
                ActiveSlot.shiftTimeline(BusyTimeLineShift);
                EndDateTime = ReferenceTime;
                return true;
            }

            StartDateTime= ActiveSlot.Start;
            EndDateTime = ActiveSlot.End;
            return false;
        }

        /// <summary>
        /// Shifts a subcalendar event by the specified "ChangeInTime". Function returns a false if the change in time will not fall within calendarevent range. It returns true if successful. The force variable makes the subcalendareventignore the check for fitting in the calendarevent range
        /// </summary>
        /// <param name="ChangeInTime"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        virtual public bool shiftEvent(TimeSpan ChangeInTime, bool force=false)
        {
            if (force)
            {
                StartDateTime += ChangeInTime;
                EndDateTime += ChangeInTime;
                ActiveSlot.shiftTimeline(ChangeInTime);
                return true;
            }
            TimeLine UpdatedTimeLine = new TimeLine(this.Start + ChangeInTime, this.End + ChangeInTime);
            if (!(this.getCalculationRange.IsTimeLineWithin(UpdatedTimeLine)))
            {
                return false;
            }
            else
            {
                StartDateTime += ChangeInTime;
                EndDateTime += ChangeInTime;
                ActiveSlot.shiftTimeline(ChangeInTime);
                return true;
            }
        }

        virtual public bool shiftEvent(DateTimeOffset newStartTime, bool force = false)
        {
            return shiftEvent(newStartTime - this.Start, force);
        }

        public static double CalculateDistanceOfSubEventsWithSameCalendarEvent(IList<SubCalendarEvent> Allevents, double distanceMultiplier)
        {
            double retValue = 0;
            HashSet<string> allIds = new HashSet<string>(Allevents.Select(obj => obj.UniqueID.getCalendarEventComponent()));
            if (allIds.Count != 1)
            {
                throw new Exception("Calculation of distance with subeevnts with different calendart event ids");
            }
            if (Allevents.Count > 0)
            {
                retValue = Utility.getFibonacciSumToIndex((uint)Allevents.Count - 2);
                retValue*= distanceMultiplier;
            }
            return retValue;
        }


        public static double CalculateDistance(SubCalendarEvent Arg1,SubCalendarEvent Arg2, double worstDistance=double.MaxValue)
        {
            if (Arg1.SubEvent_ID.getIDUpToCalendarEvent() == Arg2.SubEvent_ID.getIDUpToCalendarEvent())
            {
                return worstDistance;
            }
            else
            {
                return Location.calculateDistance(Arg1.Location, Arg2.Location, worstDistance);
            }
        }

        /// <summary>
        /// Function calculates the total distance  by multiple sub calendar events. When SubEvents withtin the same Calendar event are ordered consecutively the distance between them is assigned the worst value. Note calculation uses double.minvalue to determine if this is a defaultentry;
        /// </summary>
        /// <param name="Allevents"></param>
        /// <param name="worstDistance"></param>
        /// <returns></returns>
         public static double CalculateDistance(IList<SubCalendarEvent> Allevents, double worstDistance=double.MinValue, bool useFibonnacci = true)
         {
            double retValue = 0;
            double distance = 0;
            double distanceMultiplier = 0;
            double multiplierCounter = 0;
            if (Allevents.Count >= 2)
            {
                if (worstDistance == double.MinValue)
                {
                    worstDistance = double.MaxValue / (Allevents.Count - 1);
                }
                if (useFibonnacci)
                {
                    bool reInitempList = false;
                    List<List<SubCalendarEvent>> subEventGroups = new List<List<SubCalendarEvent>>();
                    List<SubCalendarEvent> tempList = new List<SubCalendarEvent>();
                    SubCalendarEvent previousSubEvent = Allevents.First();
                    for (int i = 1; i < Allevents.Count - 1; i++)
                    {
                        SubCalendarEvent currentSubEvent = Allevents[i];
                        if (previousSubEvent.UniqueID.getCalendarEventComponent() == currentSubEvent.UniqueID.getCalendarEventComponent())
                        {
                            tempList.Add(previousSubEvent);
                            tempList.Add(currentSubEvent);
                            reInitempList = true;
                        }
                        else
                        {
                            if (reInitempList)
                            {
                                subEventGroups.Add(tempList);
                                tempList = new List<SubCalendarEvent>();
                                reInitempList = false;
                            }
                            //else
                            {
                                ++multiplierCounter;
                                distance = CalculateDistance(currentSubEvent, previousSubEvent, worstDistance);
                                if(distance == worstDistance)
                                {
                                    distanceMultiplier += 1;
                                }
                                else
                                {
                                    distanceMultiplier += distance;
                                }
                                retValue += distance;
                            }
                        }
                        previousSubEvent = currentSubEvent;
                    }

                    distanceMultiplier /= multiplierCounter;
                    subEventGroups.ForEach(listOfSubEvents => {
                        double fibboDIstance = CalculateDistanceOfSubEventsWithSameCalendarEvent(listOfSubEvents, distanceMultiplier);
                        retValue += fibboDIstance;
                    });
                    
                }
                else
                {
                    int j = 0;
                    for (int i = 0; i < Allevents.Count - 1; i++)
                    {
                        j = i + 1;
                        retValue += CalculateDistance(Allevents[i], Allevents[j], worstDistance);
                    }
                }
                return retValue;
            }
            return retValue;
         }

        

         virtual public bool canExistWithinTimeLine(TimeLine PossibleTimeLine)
         {
            bool retValue = false;
            if (!this.isLocked)
            {
                SubCalendarEvent thisCopy = this.createCopy(this.UniqueID);
                retValue = (thisCopy.PinToStart(PossibleTimeLine) && thisCopy.PinToEnd(PossibleTimeLine));
            }
            else
            {
                retValue = PossibleTimeLine.IsTimeLineWithin(this.RangeTimeLine);
            }
            return retValue;
        }

         virtual public bool canExistTowardsEndWithoutSpace(TimeLine PossibleTimeLine)
         {
             TimeLine ParentCalRange = getCalendarEventRange;
             bool retValue = (ParentCalRange.Start <= (PossibleTimeLine.End - getActiveDuration)) && (ParentCalRange.End>=PossibleTimeLine.End)&&(canExistWithinTimeLine(PossibleTimeLine));
             return retValue;
         }

        static public bool isConflicting(SubCalendarEvent firstEvent, SubCalendarEvent secondEvent)
        {
            bool retValue = firstEvent.RangeTimeLine.InterferringTimeLine(secondEvent.RangeTimeLine) != null;
            return retValue;
        }

         virtual public bool canExistTowardsStartWithoutSpace(TimeLine PossibleTimeLine)
         {
             TimeLine ParentCalRange = getCalendarEventRange;
             bool retValue = ((PossibleTimeLine.Start + getActiveDuration) <= ParentCalRange.End) && (ParentCalRange.Start <= PossibleTimeLine.Start) && (canExistWithinTimeLine(PossibleTimeLine));
             return retValue;
         }
         /// <summary>
         /// Function returns the largest Timeline interferes with its calendar event range. If restricted subcalevent you can use the orderbystart to make a preference for selection. Essentiall select the largest time line with earliest start time
         /// </summary>
         /// <param name="TimeLineData"></param>
         /// <returns></returns>
         virtual public List<TimeLine> getTimeLineInterferringWithCalEvent(TimeLine TimeLineData, bool orderByStart = true)
         {
             TimeLine retValuTimeLine= CalendarEventRange.InterferringTimeLine(TimeLineData);;
             List<TimeLine> retValue = null;
             if (retValuTimeLine!=null)
             {
                 retValue = new List<TimeLine>() { retValuTimeLine };
             }
             return retValue;
         }

        virtual public DateTimeOffset getPauseTime()
        {
            return _PauseTime;
        }
        virtual internal TimeSpan Pause(DateTimeOffset currentTime)
        {
            _PauseTime = currentTime;
            DateTimeOffset Start = this.Start;
            DateTimeOffset End = this.End;
            TimeSpan NewUsedTime = _PauseTime - Start;

            _UsedTime = NewUsedTime;
            return NewUsedTime;
        }

        virtual internal bool Continue(DateTimeOffset currentTime)
        {
            _PauseTime = new DateTimeOffset();
            TimeSpan timeDiff = (currentTime- UsedTime) - (Start);
            bool RetValue = shiftEvent(timeDiff);
            return RetValue;
        }

        virtual public bool UnPause(DateTimeOffset currentTime)
        {
            _PauseTime = new DateTimeOffset();
            _UsedTime = new TimeSpan();
            TimeSpan timeDiff = new TimeSpan();
            bool RetValue = shiftEvent(timeDiff);
            return RetValue;
        }

        public void UpdateInHumaneTimeLine()
         {
             NonHumaneTimeLine = ActiveSlot.CreateCopy();
         }

         public void UpdateHumaneTimeLine()
         {
             HumaneTimeLine = ActiveSlot.CreateCopy();
         }

         public ulong UniversalDayIndex
         {
             get
             {
                 return preferredDayIndex;
             }
         }

        public void enableCalculationMode()
        {
            CalculationMode = true;
        }

        public override bool isLocked => base.isLocked || this.tempLock;

        /// <summary>
        /// This changes the duration of the subevent. It requires the change in duration. This just adds/subtracts the delta to the end time
        /// </summary>
        /// <param name="Delta"></param>
        public virtual void addDurartion(TimeSpan Delta)
         {
             TimeSpan NewEventDuration = EventDuration.Add(Delta);
             if (NewEventDuration > new TimeSpan(0))
             {
                 EventDuration = NewEventDuration;
                 EndDateTime = StartDateTime.Add(EventDuration);
                 BusyFrame.updateBusyTimeLine(new BusyTimeLine(getId, ActiveSlot.Start, ActiveSlot.Start.Add(EventDuration)));
                 return;
             }
             throw new Exception("You are trying to reduce the Duration length to Less than zero");

         }

        internal void changeTimeLineRange(TimeLine newTimeLine, bool resetCalculationTimeLine = true)
        {
            CalendarEventRange = newTimeLine.CreateCopy();
            if(resetCalculationTimeLine)
            {
                CalculationTimeLine = null;
            }
        }

         public void updateUnusables(ulong unwantedIndex)
         {
             UnUsableIndex = unwantedIndex;
         }

         public ulong getUnUsableIndex()
         {
             return UnUsableIndex;
         }

         public ulong resetAndgetUnUsableIndex()
         {
             ulong retValue = UnUsableIndex;
             UnUsableIndex = 0;
             return retValue;
         }
        

        public static void updateUnUsable(IEnumerable<SubCalendarEvent>SubEVents,ulong UnwantedIndex)
        {
            SubEVents.AsParallel().ForAll(obj=>{obj.UnUsableIndex=UnwantedIndex;});
        }
        #endregion

        #region Class Properties

        /// <summary>
        /// Pathoptimization has been acknowledged on this subevent
        /// </summary>
        public bool isOptimized
        {
            get
            {
                return OptimizationFlag;
            }
        }

        public ulong OldUniversalIndex
        {
            get
            {
                return OldPreferredIndex;
            }

        }

        public bool isDesignated
        {
            get
            {
                bool retValue = preferredDayIndex != 0;
                return retValue;
            }
        }
         public ConflictProfile Conflicts
         {
             get
             {
                 return ConflictingEvents;
             }
         }

        public TimeLine getCalculationRange
        {
            get 
            {
                return CalculationTimeLine ?? (CalculationTimeLine = getCalendarEventRange); 
            }
        }

        public TimeLine getCalendarEventRange
        {
            get
            {
                return CalendarEventRange;
            }
        }

        public double Score
        {
            get 
            {
                return EventScore;
            }/*
            set
            {
                EventScore = value;
            }*/
        }

        public int IntData
        {
            get
            {
                return MiscIntData;
            }
        }


        public double fittability
        {
            get
            {
                double retValue = ((double)CalendarEventRange.TimelineSpan.Ticks )/ ((double)RangeSpan.Ticks);
                return retValue;
            }
        }

        public BusyTimeLine ActiveSlot
        {
            set
            {
                if (BusyFrame.TimelineSpan != value.TimelineSpan)
                {
                    throw new Exception("New lhs Activeslot isnt the same duration as old active slot. Check for inconsistency in code");
                }
                else 
                {
                    TimeSpan ChangeInTimeSpan = value.Start - BusyFrame.Start;
                    shiftEvent(ChangeInTimeSpan);
                }
                
            }
            get
            {
                return BusyFrame;
            }
        }

        override public TimeSpan getActiveDuration
        {
            get
            {
                return EventDuration;
            }
        }

        override public string getId
        {
            get
            {
                return UniqueID.ToString();
            }
        }

        

        public EventID SubEvent_ID
        {
            get
            {
                return UniqueID;//.ToString();
            }
        }

        public override  TimeLine RangeTimeLine
        {
            get
            {
                
                return ActiveSlot;
            }
        }


        public TimeSpan RangeSpan
        {
            get
            {
                return this.RangeTimeLine.TimelineSpan;
            }
        }

        virtual public bool isBlobEvent
        {
            get
            {
                return BlobEvent;
            }
    }
        

        virtual public MiscData Notes
        { 
            get
            {
                return DataBlob;
            }
        }

         virtual public bool isVestige
         {
             get 
             {
                 return Vestige;
             }
         }
         
        public bool isInCalculationMode
        {
            get
            {
                return CalculationMode;
            }
        }

        public bool isPaused
        {
            get
            {
                return getPauseTime() != InitialPauseTime;
            }
        }

        public override DateTimeOffset getDeadline
        {
	        get 
	        {
                return CalendarEventRange.End;
	        }
        }
        public virtual Dictionary<TimeSpan, List<Reason>>  ReasonsForPosiition
        {
            get {
                return ReasonsForCurrentPosition;
            }
        }

        public virtual Dictionary<TimeSpan, List<Reason>> ReasonsOnHistoryforPosition
        {
            get
            {
                return HistoricalReasonsCurrentPosition;
            }
        }
        #endregion

    }
}


