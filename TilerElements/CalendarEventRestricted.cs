using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class CalendarEventRestricted:CalendarEvent, IUndoable
    {
        protected RestrictionProfile ProfileOfRestriction;

        public RestrictionProfile UndoProfileOfRestriction;
        protected CalendarEventRestricted ()
        {
        ;
        }

        public override void undoUpdate(Undo undo)
        {
            UndoProfileOfRestriction.undoUpdate(undo);
            base.undoUpdate(undo);
        }

        public override void undo(string undoId)
        {
            if (UndoId == undoId)
            {
                UndoProfileOfRestriction.undo(undoId);
            }
            base.undo(undoId);
        }

        public override void redo(string undoId)
        {
            if (UndoId == undoId)
            {
                UndoProfileOfRestriction.redo(undoId);
            }
            base.redo(undoId);
        }

        public CalendarEventRestricted(TilerUser creator, TilerUserGroup userGroup, EventName Name, DateTimeOffset Start, DateTimeOffset End, RestrictionProfile restrictionProfile, TimeSpan Duration, Repetition RepetitionProfile, bool isCompleted, bool isEnabled, int Divisions, bool isRigid, Location Location,TimeSpan EventPreparation,TimeSpan Event_PreDeadline, EventID eventId, EventDisplay UiSettings = null, MiscData NoteData=null, string timeZone = null)
        {
           _Name =  Name;
            StartDateTime = Start;
            EndDateTime = End;
            RigidSchedule = isRigid;
            ProfileOfRestriction = restrictionProfile;
            _Splits = Divisions;
            if (RepetitionProfile.EnableRepeat)
            {
                _Splits = Divisions;
                End = RepetitionProfile.Range.End;
                _AverageTimePerSplit = new TimeSpan();
                this._EventRepetition = RepetitionProfile;
            }
            _Complete = isCompleted;
            _Enabled = isEnabled;
            UniqueID = EventID.GenerateCalendarEvent();
            UniqueID = eventId ?? this.UniqueID; /// already initialized by preceeding code initialization
            _UiParams = UiSettings;
            _DataBlob = NoteData;
            _LocationInfo = Location;
            
            _EventDuration = Duration;
            _AverageTimePerSplit = TimeSpan.FromTicks(_EventDuration.Ticks / _Splits);
            isRestricted = true;
            _ProfileOfNow = new NowProfile();
            this._Creator = creator;
            this._Users = userGroup;
            this._TimeZone = timeZone;
            InstantiateSubEvents();
        }



        public CalendarEventRestricted(TilerUser creator, TilerUserGroup userGroup, string timeZone, EventName Name, DateTimeOffset Start, DateTimeOffset End, RestrictionProfile restrictionProfile, TimeSpan Duration, Repetition RepetitionProfile, bool isCompleted, bool isEnabled, int Divisions, bool isRigid, Location Location,TimeSpan EventPreparation,TimeSpan Event_PreDeadline, EventID eventId, EventDisplay UiSettings = null, MiscData NoteData = null)
        {
            _Name = Name;
            StartDateTime = Start;
            EndDateTime = End;
            RigidSchedule = isRigid;
            ProfileOfRestriction = restrictionProfile;
            _Splits = Divisions;
            if (RepetitionProfile.EnableRepeat)
            {
                _Splits = Divisions;
                End = RepetitionProfile.Range.End;
                _AverageTimePerSplit = new TimeSpan();
                this._EventRepetition = RepetitionProfile;
            }
            _Complete = isCompleted;
            _Enabled = isEnabled;
            _EventPreDeadline = Event_PreDeadline;
            UniqueID = EventID.GenerateCalendarEvent();
            UniqueID = eventId ?? this.UniqueID; /// already initialized by preceeding code initialization
            _UiParams = UiSettings;
            _DataBlob = NoteData;
            _ProfileOfNow = new NowProfile();
            isRestricted = true;
            _EventDuration = Duration;
            _AverageTimePerSplit = TimeSpan.FromTicks(_EventDuration.Ticks / _Splits);
            _LocationInfo = Location;
            this._Creator = creator;
            this._Users = userGroup;
            this._TimeZone = timeZone;

            InstantiateSubEvents();
        }

        static public CalendarEventRestricted InstantiateRepeatedCandidate(EventName Name, DateTimeOffset Start, DateTimeOffset End, EventID CalendarEventID, RestrictionProfile restrictionProfile, TimeSpan Duration, int division, Location Location,EventDisplay UiSettings,bool RigidFlag,TimeSpan preparation, string thirdPartyID)
        { 
            CalendarEventRestricted retValue = new CalendarEventRestricted();
            retValue .UniqueID = EventID.GenerateRepeatCalendarEvent(CalendarEventID.ToString());
            retValue ._Name =  Name;
            retValue._EventDuration = Duration;
            retValue.StartDateTime = Start;
            retValue.EndDateTime = End;
            retValue.ProfileOfRestriction = restrictionProfile;
            retValue._Complete = false;
            retValue._Enabled = true;
            retValue._Splits = division;
            retValue._AverageTimePerSplit = TimeSpan.FromTicks( retValue._EventDuration.Ticks / division);
            retValue.isRestricted = true;
            retValue._LocationInfo = Location;
            retValue.EndOfCalculation = End < TilerEvent.EventNow.Add(CalculationEndSpan) ? End : TilerEvent.EventNow.Add(CalculationEndSpan);
            retValue.RigidSchedule = RigidFlag;
            retValue._Priority = 0;
            retValue._otherPartyID = thirdPartyID;
            retValue._UiParams = UiSettings;
            retValue._PrepTime = preparation;
            
            //retValue.UpdateLocationMatrix(Location);
            retValue.InstantiateSubEvents();
            
            return retValue;
        }

        public override CalendarEvent createCopy(EventID eventId = null)
        {
            CalendarEventRestricted MyCalendarEventCopy = new CalendarEventRestricted();
            MyCalendarEventCopy._EventDuration = new TimeSpan(_EventDuration.Ticks);
            MyCalendarEventCopy._Name = this._Name.createCopy();
            MyCalendarEventCopy.StartDateTime = StartDateTime;
            MyCalendarEventCopy.EndDateTime = EndDateTime;
            MyCalendarEventCopy._EventPreDeadline = new TimeSpan(_EventPreDeadline.Ticks);
            MyCalendarEventCopy._PrepTime = new TimeSpan(_PrepTime.Ticks);
            MyCalendarEventCopy._Priority = _Priority;
            MyCalendarEventCopy._EventRepetition = _EventRepetition != null ? _EventRepetition.CreateCopy() : _EventRepetition;
            MyCalendarEventCopy._Complete = this._Complete;
            MyCalendarEventCopy.RigidSchedule = RigidSchedule;//hack
            MyCalendarEventCopy._Splits = _Splits;
            MyCalendarEventCopy._AverageTimePerSplit = new TimeSpan(_AverageTimePerSplit.Ticks);
            MyCalendarEventCopy.EventSequence = EventSequence.CreateCopy();
            MyCalendarEventCopy.SubEvents = new SubEventDictionary<string, SubCalendarEvent>();
            MyCalendarEventCopy._UiParams = this._UiParams != null ? this._UiParams.createCopy() : null;
            MyCalendarEventCopy._DataBlob = this._DataBlob != null ? this._DataBlob.createCopy() : null;
            MyCalendarEventCopy._Enabled = this._Enabled;
            MyCalendarEventCopy.isRestricted = this.isRestricted;
            MyCalendarEventCopy._LocationInfo = _LocationInfo;//hack you might need to make copy
            MyCalendarEventCopy._ProfileOfProcrastination = this._ProfileOfProcrastination.CreateCopy();
            MyCalendarEventCopy._UserDeleted = this._UserDeleted;
            MyCalendarEventCopy._CompletedCount = this._CompletedCount;
            MyCalendarEventCopy._DeletedCount = this._DeletedCount;
            MyCalendarEventCopy.ProfileOfRestriction = this.ProfileOfRestriction.createCopy();
            MyCalendarEventCopy._ProfileOfNow = this.getNowInfo != null ? this.getNowInfo.CreateCopy() : null;
            MyCalendarEventCopy._ProfileOfProcrastination = this._ProfileOfProcrastination.CreateCopy();
            MyCalendarEventCopy._Semantics = this._Semantics != null ? this._Semantics.createCopy() : null;
            MyCalendarEventCopy._UsedTime = this._UsedTime;
            if (eventId != null)
            {
                MyCalendarEventCopy.UniqueID = eventId;
            }
            else
            {
                MyCalendarEventCopy.UniqueID = UniqueID;//hack
            }

            foreach (SubCalendarEventRestricted eachSubCalendarEvent in this.SubEvents.Values)
            {
                MyCalendarEventCopy.SubEvents.Add(eachSubCalendarEvent.Id, eachSubCalendarEvent.createCopy(EventID.GenerateSubCalendarEvent(MyCalendarEventCopy.UniqueID) ));
            }

            //MyCalendarEventCopy.SchedulStatus = SchedulStatus;
            MyCalendarEventCopy._otherPartyID = _otherPartyID == null ? null : _otherPartyID.ToString();
            MyCalendarEventCopy._Users = this._Users;
            return MyCalendarEventCopy;
            
            //return base.createCopy();
        }

        void InstantiateSubEvents()
        {
            SubEvents = new SubEventDictionary<string, SubCalendarEvent>();
            TimeLine eachStart = ProfileOfRestriction.getEarliestStartTimeWithinAFrameAfterRefTime(this.Start);
            for (int i = 0; i < _Splits; i++)
            {
                DateTimeOffset SubStart = eachStart.Start;
                DateTimeOffset SubEnd = SubStart.Add(_AverageTimePerSplit);
                SubCalendarEventRestricted newEvent = new SubCalendarEventRestricted(this.getCreator, this._Users, UniqueID.ToString(), this.getName, SubStart, SubEnd, ProfileOfRestriction, this.RangeTimeLine, true, false, new ConflictProfile(), RigidSchedule, _PrepTime, _EventPreDeadline, _LocationInfo, _UiParams, _DataBlob, _Priority, ThirdPartyID);
                newEvent.TimeCreated = this.TimeCreated;
                SubEvents.Add(newEvent.Id, newEvent);
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
            RetValue._EventDuration = this.getActiveDuration;
            RetValue._Name = this.getName.createCopy();
            RetValue.StartDateTime = this.Start;
            RetValue.EndDateTime = this.End;
            RetValue._EventPreDeadline = this.getPreDeadline;
            RetValue._PrepTime = this.getPreparation;
            RetValue._Priority = this.getEventPriority;
            RetValue._EventRepetition = this.Repeat;
            RetValue._Complete = this.getIsComplete;
            RetValue.RigidSchedule = this.RigidSchedule;
            RetValue._userLocked= this._userLocked;
            RetValue._Splits = this.NumberOfSplit;
            RetValue._AverageTimePerSplit = this.AverageTimeSpanPerSubEvent;
            RetValue.UniqueID = EventID.GenerateCalendarEvent();
            //RetValue.EventSequence = this.EventSequence;
            RetValue.SubEvents = new SubEventDictionary<string,SubCalendarEvent>();
            RetValue._UiParams = this.getUIParam.createCopy();
            RetValue._DataBlob = this.Notes;
            RetValue._Enabled = this.isEnabled;
            RetValue.isRestricted = this.getIsEventRestricted;
            RetValue._LocationInfo = this.Location;//hack you might need to make copy
            RetValue._ProfileOfProcrastination = this.getProcrastinationInfo.CreateCopy();
            RetValue._UserDeleted = this.getIsUserDeleted;
            RetValue._CompletedCount = this.CompletionCount;
            RetValue._DeletedCount = this.DeletionCount;
            RetValue._ProfileOfProcrastination = this._ProfileOfProcrastination.CreateCopy();
            RetValue._ProfileOfNow = this._ProfileOfNow.CreateCopy();
            RetValue._otherPartyID = this.ThirdPartyID;// == this.null ? null : otherPartyID.ToString();
            RetValue._Users = this.getAllUsers();//.ToList();
            RetValue._ProfileOfNow = this._ProfileOfNow.CreateCopy();
            RetValue.ProfileOfRestriction = this.ProfileOfRestriction.createCopy();
            //RetValue.UpdateLocationMatrix(RetValue.LocationInfo);
            return RetValue;
        }

        override protected void IncreaseSplitCount(uint delta)
        {
            List<SubCalendarEvent> newSubs = new List<SubCalendarEvent>();
            TimeLine eachStart = ProfileOfRestriction.getEarliestStartTimeWithinAFrameAfterRefTime(this.Start);
            for (int i = 0; i < delta; i++)
            {
                DateTimeOffset SubStart = eachStart.Start;
                DateTimeOffset SubEnd = SubStart.Add(_AverageTimePerSplit);
                SubCalendarEventRestricted newEvent = new SubCalendarEventRestricted(this.getCreator, this._Users, UniqueID.ToString(), this.getName, SubStart, SubEnd, ProfileOfRestriction, this.RangeTimeLine, true, false, new ConflictProfile(), RigidSchedule, _PrepTime, _EventPreDeadline, _LocationInfo, _UiParams, _DataBlob, _Priority, ThirdPartyID);
                SubEvents.Add(newEvent.Id, newEvent);
            }
            _Splits += (int)delta;
            _EventDuration = TimeSpan.FromTicks(SubEvents.Values.Sum(subEvent => subEvent.getActiveDuration.Ticks));
        }

        public override void UpdateThis(CalendarEvent CalendarEventEntry)
        {
            if ((this.getId == CalendarEventEntry.getId))
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

        public override List<TimeLine> getInterferringWithTimeLine(TimeLine timeLine)
        {
            List<TimeLine> nonPartialFrames = ProfileOfRestriction.getAllNonPartialTimeFrames(timeLine);
            TimeLine earliestFrame = ProfileOfRestriction.getEarliestActiveFrameAfterBeginning(timeLine);
            TimeLine latestFrame = ProfileOfRestriction.getLatestActiveTimeFrameBeforeEnd(timeLine);
            nonPartialFrames = nonPartialFrames.Where(objTimeLine => objTimeLine.Start != earliestFrame.Start && objTimeLine.End != latestFrame.End ).ToList();
            nonPartialFrames.Insert(0, earliestFrame);
            nonPartialFrames.Add(latestFrame);
            return nonPartialFrames;
        }
    }
}
