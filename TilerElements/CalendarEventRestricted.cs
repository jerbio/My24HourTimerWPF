using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class CalendarEventRestricted:CalendarEvent, IUndoable
    {
        protected RestrictionProfile _ProfileOfRestriction;

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
        public CalendarEventRestricted(TilerUser creator, TilerUserGroup userGroup, EventName name, DateTimeOffset start, DateTimeOffset end, RestrictionProfile restrictionProfile, TimeSpan Duration, Repetition RepetitionProfile, bool isCompleted, bool isEnabled, int Divisions, bool isRigid, NowProfile profileOfNow,  Location Location,TimeSpan EventPreparation,TimeSpan Event_PreDeadline, EventID eventId, ReferenceNow now, Procrastination procrastination, UpdateHistory updateHistory, EventDisplay UiSettings = null, MiscData NoteData=null, string timeZone = null)
        {
           _Name =  name;
            _RigidSchedule = isRigid;
            _ProfileOfRestriction = restrictionProfile;
            _Splits = Divisions;
            _Complete = isCompleted;
            _Enabled = isEnabled;
            UniqueID = EventID.GenerateCalendarEvent();
            UniqueID = eventId ?? this.UniqueID; /// already initialized by preceeding code initialization
            _UiParams = UiSettings;
            _DataBlob = NoteData;
            _LocationInfo = Location;
            
            _EventDuration = Duration;
            if (_Splits != 0)
            {
                _AverageTimePerSplit = TimeSpan.FromTicks(_EventDuration.Ticks / _Splits);
            }
            else
            {
                _AverageTimePerSplit = new TimeSpan();
            }
            isRestricted = true;
            _ProfileOfNow = profileOfNow?? new NowProfile();
            this._Creator = creator;
            this._Users = userGroup;
            this._TimeZone = timeZone;
            _Now = now;
            if (this.Location_DB != null)
            {
                _LocationInfo.User = this.getCreator;
            }
            _EventDayPreference = new EventPreference();
            _ProfileOfProcrastination = procrastination;
            updateStartTime(start);
            updateEndTime(end);
            if (RepetitionProfile.EnableRepeat)
            {
                _Splits = Divisions;
                updateEndTime(RepetitionProfile.Range.End);
                _AverageTimePerSplit = new TimeSpan();
                this._EventRepetition = RepetitionProfile;
            }
            this._IniStartTime = start;
            this._IniEndTime = end;
            this._UpdatesHistory = updateHistory ?? new UpdateHistory();
            addUpdatedTimeLine(this.StartToEnd);
            InstantiateSubEvents();
        }



        public CalendarEventRestricted(TilerUser creator, TilerUserGroup userGroup, string timeZone, EventName Name, DateTimeOffset Start, DateTimeOffset End, RestrictionProfile restrictionProfile, TimeSpan Duration, Repetition RepetitionProfile, bool isCompleted, bool isEnabled, int Divisions, bool isRigid, NowProfile profileOfNow, Location Location,TimeSpan EventPreparation,TimeSpan Event_PreDeadline, EventID eventId, ReferenceNow now, Procrastination procrastination, UpdateHistory updateHistory, EventDisplay UiSettings = null, MiscData NoteData = null)
        {
            _Name = Name;
            _RigidSchedule = isRigid;
            _ProfileOfRestriction = restrictionProfile;
            _Splits = Divisions;
            _Complete = isCompleted;
            _Enabled = isEnabled;
            _EventPreDeadline = Event_PreDeadline;
            UniqueID = EventID.GenerateCalendarEvent();
            UniqueID = eventId ?? this.UniqueID; /// already initialized by preceeding code initialization
            _UiParams = UiSettings;
            _DataBlob = NoteData;
            _ProfileOfNow = profileOfNow??new NowProfile();
            isRestricted = true;
            _EventDuration = Duration;
            if (_Splits != 0) {
                _AverageTimePerSplit = TimeSpan.FromTicks(_EventDuration.Ticks / _Splits);
            } else
            {
                _AverageTimePerSplit = new TimeSpan();
            }
            
            _LocationInfo = Location;
            this._Creator = creator;
            this._Users = userGroup;
            this._TimeZone = timeZone;
            _Now = now;
            if (this.Location_DB != null)
            {
                _LocationInfo.User = this.getCreator;
            }
            _EventDayPreference = new EventPreference();
            _ProfileOfProcrastination = procrastination;
            updateStartTime(Start);
            updateEndTime(End);
            if (RepetitionProfile.EnableRepeat)
            {
                _Splits = Divisions;
                updateEndTime(RepetitionProfile.Range.End);
                _AverageTimePerSplit = new TimeSpan();
                this._EventRepetition = RepetitionProfile;
            }
            this._IniStartTime = this.Start;
            this._IniEndTime = this.End;
            this._UpdatesHistory = updateHistory ?? new UpdateHistory();
            addUpdatedTimeLine(this.StartToEnd);
            InstantiateSubEvents();
        }

        static public CalendarEventRestricted InstantiateRepeatedCandidate(EventName Name, DateTimeOffset Start, DateTimeOffset End, EventID CalendarEventID, RestrictionProfile restrictionProfile, TimeSpan Duration, int division, Location Location,EventDisplay UiSettings,bool RigidFlag,TimeSpan preparation, string thirdPartyID, ReferenceNow now, TilerUser tilerUser, MiscData miscData, NowProfile nowProfile, Procrastination procrastination)
        { 
            CalendarEventRestricted retValue = new CalendarEventRestricted();
            retValue .UniqueID = EventID.GenerateRepeatCalendarEvent(CalendarEventID.ToString());
            retValue ._Name =  Name;
            retValue._EventDuration = Duration;
            retValue._ProfileOfRestriction = restrictionProfile;
            retValue._Complete = false;
            retValue._Enabled = true;
            retValue._Splits = division;
            retValue._AverageTimePerSplit = TimeSpan.FromTicks( retValue._EventDuration.Ticks / division);
            retValue.isRestricted = true;
            retValue._LocationInfo = Location;
            retValue.EndOfCalculation = End < TilerEvent.EventNow.Add(CalculationEndSpan) ? End : TilerEvent.EventNow.Add(CalculationEndSpan);
            retValue._RigidSchedule = RigidFlag;
            retValue._Priority = 0;
            retValue._otherPartyID = thirdPartyID;
            retValue._UiParams = UiSettings;
            retValue._PrepTime = preparation;
            retValue._Now = now;
            retValue._EventDayPreference = new EventPreference();
            retValue._Creator = tilerUser;
            retValue._DataBlob = miscData;
            retValue._ProfileOfNow = nowProfile;
            retValue._ProfileOfProcrastination = procrastination;
            retValue.updateStartTime(Start);
            retValue.updateEndTime(End);
            retValue.InstantiateSubEvents();
            
            return retValue;
        }

        public override CalendarEvent createCopy(EventID eventId = null)
        {
            CalendarEventRestricted MyCalendarEventCopy = new CalendarEventRestricted();
            MyCalendarEventCopy._EventDuration = new TimeSpan(_EventDuration.Ticks);
            MyCalendarEventCopy._Name = this._Name.createCopy();
            MyCalendarEventCopy._EventPreDeadline = new TimeSpan(_EventPreDeadline.Ticks);
            MyCalendarEventCopy._PrepTime = new TimeSpan(_PrepTime.Ticks);
            MyCalendarEventCopy._Priority = _Priority;
            MyCalendarEventCopy._EventRepetition = _EventRepetition?.CreateCopy();
            MyCalendarEventCopy._Complete = this._Complete;
            MyCalendarEventCopy._RigidSchedule = _RigidSchedule;//hack
            MyCalendarEventCopy._Splits = _Splits;
            MyCalendarEventCopy._AverageTimePerSplit = new TimeSpan(_AverageTimePerSplit.Ticks);
            MyCalendarEventCopy.EventSequence = EventSequence.CreateCopy();
            MyCalendarEventCopy._SubEvents = new SubEventDictionary<string, SubCalendarEvent>();
            MyCalendarEventCopy._UiParams = this._UiParams?.createCopy();
            MyCalendarEventCopy._DataBlob = this._DataBlob?.createCopy();
            MyCalendarEventCopy._Enabled = this._Enabled;
            MyCalendarEventCopy.isRestricted = this.isRestricted;
            MyCalendarEventCopy._LocationInfo = _LocationInfo;//hack you might need to make copy
            MyCalendarEventCopy._AutoDeleted = this._AutoDeleted;
            MyCalendarEventCopy._CompletedCount = this._CompletedCount;
            MyCalendarEventCopy._DeletedCount = this._DeletedCount;
            MyCalendarEventCopy._ProfileOfRestriction = this._ProfileOfRestriction?.createCopy();
            MyCalendarEventCopy._ProfileOfNow = this.getNowInfo?.CreateCopy();
            MyCalendarEventCopy._ProfileOfProcrastination = this._ProfileOfProcrastination?.CreateCopy();
            MyCalendarEventCopy._Semantics = this._Semantics?.createCopy();
            MyCalendarEventCopy._Now = this._Now;
            MyCalendarEventCopy._UsedTime = this._UsedTime;
            MyCalendarEventCopy._EventDayPreference = this._EventDayPreference?.createCopy();
            MyCalendarEventCopy.updateStartTime(Start);
            MyCalendarEventCopy.updateEndTime(End);
            MyCalendarEventCopy.UpdateHistory_DB = _UpdatesHistory;
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
                SubCalendarEvent subEventCopy = eachSubCalendarEvent.CreateCopy(EventID.GenerateSubCalendarEvent(MyCalendarEventCopy.UniqueID), MyCalendarEventCopy);
                subEventCopy.ParentCalendarEvent = MyCalendarEventCopy;
                MyCalendarEventCopy._SubEvents.Add(eachSubCalendarEvent.Id, subEventCopy);
            }

            MyCalendarEventCopy._otherPartyID = _otherPartyID == null ? null : _otherPartyID.ToString();
            MyCalendarEventCopy._Users = this._Users;
            return MyCalendarEventCopy;
           
        }

        void InstantiateSubEvents()
        {
            _SubEvents = new SubEventDictionary<string, SubCalendarEvent>();
            TimeLine eachStart = _ProfileOfRestriction.getEarliestStartTimeWithinAFrameAfterRefTime(this.Start).Item1;
            for (int i = 0; i < _Splits; i++)
            {
                DateTimeOffset SubStart = eachStart.Start;
                DateTimeOffset SubEnd = SubStart.Add(_AverageTimePerSplit);
                SubCalendarEventRestricted newEvent = new SubCalendarEventRestricted(this, this.getCreator, this._Users, UniqueID.ToString(), this.getName, SubStart, SubEnd, _ProfileOfRestriction, this.StartToEnd, true, false, new ConflictProfile(), _RigidSchedule, _PrepTime, _EventPreDeadline, _LocationInfo, _UiParams, _DataBlob, _Now, this.getNowInfo, _Priority, ThirdPartyID);
                newEvent.TimeCreated = this.TimeCreated;
                _SubEvents.Add(newEvent.Id, newEvent);
            }
        }

        public override RestrictionProfile RestrictionProfile_DB
        {
            set
            {
                _ProfileOfRestriction = value;
            }

            get
            {
                return _ProfileOfRestriction;
            }
        }

        public override RestrictionProfile RestrictionProfile
        {
            get
            {
                return _ProfileOfRestriction;
            }
        }

        public void setNow (ReferenceNow now, bool updateCalendarEventRange = false)
        {
            _Now = now;
            if (this.IsFromRecurringAndNotChildRepeatCalEvent)
            {
                if (Repeat!=null)
                {
                    foreach (CalendarEventRestricted calRestricted in Repeat.RecurringCalendarEvents())
                    {
                        calRestricted.setNow(_Now, updateCalendarEventRange);
                    }
                }
            } else
            {
                foreach (SubCalendarEvent subEvent in AllSubEvents)
                {//only selecting Active Subevents for performance reasons
                    SubCalendarEventRestricted subEventAsRestricted = (subEvent as SubCalendarEventRestricted);
                    subEventAsRestricted?.setNow(now, updateCalendarEventRange);
                };
            }
            

        }

        protected override CalendarEvent getCalculationCopy()
        {
            CalendarEventRestricted RetValue = new CalendarEventRestricted();
            RetValue._EventDuration = this.getActiveDuration;
            RetValue._Name = this.getName.createCopy();
            RetValue._EventPreDeadline = this.getPreDeadline;
            RetValue._PrepTime = this.getPreparation;
            RetValue._Priority = this.getEventPriority;
            RetValue._EventRepetition = this.Repeat;
            RetValue._Complete = this.getIsComplete;
            RetValue._RigidSchedule = this._RigidSchedule;
            RetValue._userLocked= this._userLocked;
            RetValue._Splits = this.NumberOfSplit;
            RetValue._AverageTimePerSplit = this.AverageTimeSpanPerSubEvent;
            RetValue.UniqueID = EventID.GenerateCalendarEvent();
            //RetValue.EventSequence = this.EventSequence;
            RetValue._SubEvents = new SubEventDictionary<string,SubCalendarEvent>();
            RetValue._UiParams = this.getUIParam?.createCopy();
            RetValue._DataBlob = this.Notes;
            RetValue._Enabled = this.isEnabled;
            RetValue.isRestricted = this.getIsEventRestricted;
            RetValue._LocationInfo = this.Location;//hack you might need to make copy
            RetValue._AutoDeleted = this.getIsUserDeleted;
            RetValue._CompletedCount = this.CompletionCount;
            RetValue._DeletedCount = this.DeletionCount;
            RetValue._ProfileOfProcrastination = this._ProfileOfProcrastination?.CreateCopy();
            RetValue._ProfileOfNow = this._ProfileOfNow?.CreateCopy();
            RetValue._otherPartyID = this.ThirdPartyID;// == this.null ? null : otherPartyID.ToString();
            RetValue._Users = this.getAllUsers();//.ToList();
            RetValue._ProfileOfNow = this._ProfileOfNow?.CreateCopy();
            RetValue._ProfileOfRestriction = this._ProfileOfRestriction?.createCopy();
            RetValue._EventDayPreference = this._EventDayPreference?.createCopy();
            RetValue._Now = this._Now;
            RetValue.updateStartTime(this.Start);
            RetValue.updateEndTime(this.End);
            return RetValue;
        }

        override protected List<SubCalendarEvent> IncreaseSplitCount(uint delta)
        {
            List<SubCalendarEvent> newSubs = new List<SubCalendarEvent>();
            TimeLine eachStart = _ProfileOfRestriction.getEarliestStartTimeWithinAFrameAfterRefTime(this.Start).Item1;
            for (int i = 0; i < delta; i++)
            {
                DateTimeOffset SubStart = eachStart.Start;
                DateTimeOffset SubEnd = SubStart.Add(_AverageTimePerSplit);
                SubCalendarEventRestricted newEvent = new SubCalendarEventRestricted(this, this.getCreator, this._Users, UniqueID.ToString(), this.getName, SubStart, SubEnd, _ProfileOfRestriction, this.StartToEnd, true, false, new ConflictProfile(), _RigidSchedule, _PrepTime, _EventPreDeadline, _LocationInfo, _UiParams, _DataBlob, _Now, this.getNowInfo, _Priority, ThirdPartyID);
                _SubEvents.Add(newEvent.Id, newEvent);
                newEvent.UiParamsId = this.UiParamsId;
                newEvent.DataBlobId = this.DataBlobId;
                newSubs.Add(newEvent);

            }
            _Splits += (int)delta;
            _EventDuration = TimeSpan.FromTicks(_SubEvents.Values.Sum(subEvent => subEvent.getActiveDuration.Ticks));
            return newSubs;
        }

        public override void UpdateThis(CalendarEvent CalendarEventEntry)
        {
            if ((this.getId == CalendarEventEntry.getId))
            {
                base.UpdateThis(CalendarEventEntry);
                CalendarEventRestricted castedEvent = CalendarEventEntry as CalendarEventRestricted;
                if (castedEvent != null)
                {
                    this._ProfileOfRestriction = castedEvent._ProfileOfRestriction;
                }
                return;
            }

            throw new Exception("Invalid Calendar ID used in Update Calendar Event");
        }

        public override List<TimeLine> getInterferringWithTimeLine(TimeLine timeLine)
        {
            List<TimeLine> nonPartialFrames = _ProfileOfRestriction.getAllNonPartialTimeFrames(timeLine);
            TimeLine earliestFrame = _ProfileOfRestriction.getEarliestActiveFrameAfterBeginning(timeLine).Item1;
            TimeLine latestFrame = _ProfileOfRestriction.getLatestActiveTimeFrameBeforeEnd(timeLine).Item1;
            nonPartialFrames = nonPartialFrames.Where(objTimeLine => objTimeLine.Start != earliestFrame.Start && objTimeLine.End != latestFrame.End ).ToList();
            nonPartialFrames.Insert(0, earliestFrame);
            nonPartialFrames.Add(latestFrame);
            return nonPartialFrames;
        }

        public override void InitialCalculationLookupDays(IEnumerable<DayTimeLine> RelevantDays, ReferenceNow now = null)
        {
            TimeLineRestricted RangeTimeLine = new TimeLineRestricted(this.StartToEnd.Start, this.StartToEnd.End, _ProfileOfRestriction, now);
            this.CalculationLimitation = RelevantDays.Where(obj => {
                var timeLine = obj.InterferringTimeLine(RangeTimeLine);
                if (timeLine != null && timeLine.TimelineSpan >= _AverageTimePerSplit)
                {
                    return true;
                }
                else return false;
            }).ToDictionary(obj => obj.UniversalIndex, obj => obj);
            FreeDaysLimitation = CalculationLimitation.ToDictionary(obj => obj.Key, obj => obj.Value);
            CalculationLimitationWithUnUsables = CalculationLimitation.ToDictionary(obj => obj.Key, obj => obj.Value);
        }

        override public void updateTimeLine(TimeLine timeLine)
        {
            TimeLineRestricted newTimeLine = new TimeLineRestricted(timeLine.Start, timeLine.End, this._ProfileOfRestriction, _Now);
            if(newTimeLine.IsViable)
            {
                TimeLine oldTimeLine = new TimeLineRestricted(this.Start, this.End, this._ProfileOfRestriction, _Now);
                AllSubEvents.AsParallel().ForAll(obj => obj.changeCalendarEventRange(newTimeLine));
                bool worksForAllSubevents = true;
                SubCalendarEvent failingSubEvent = SubCalendarEvent.getEmptySubCalendarEvent(this.Calendar_EventID);
                TimeLine newTimeLineUpdated = newTimeLine;
                foreach (var obj in AllSubEvents)
                {
                    if (obj.isLocked)
                    {
                        newTimeLineUpdated = new TimeLine(newTimeLine.StartToEnd.Start, newTimeLine.StartToEnd.End);
                    }
                    if (!obj.canExistWithinTimeLine(newTimeLineUpdated))
                    {
                        worksForAllSubevents = false;
                        failingSubEvent = obj;
                    }
                }
                if (worksForAllSubevents)
                {
                    TimeLine startAndEndTimeLine = newTimeLineUpdated.StartToEnd;
                    updateStartTime( startAndEndTimeLine.Start);
                    updateEndTime( startAndEndTimeLine.End);
                    if (this.isLocked)
                    {
                        _EventDuration = End - Start;
                    }
                    addUpdatedTimeLine(oldTimeLine);
                    AllSubEvents.AsParallel().ForAll(obj =>
                    {
                        obj.changeCalendarEventRange(this.StartToEnd);
                        obj.updateCalculationEventRange(this.CalculationStartToEnd);
                    });
                }
                else
                {
                    AllSubEvents.AsParallel().ForAll(obj => obj.changeCalendarEventRange(oldTimeLine));
                    CustomErrors customError = new CustomErrors("Cannot update the timeline for the calendar event with sub event " + failingSubEvent.getId + ". Most likely because the new time line won't fit the sub event", 40000001);
                    throw customError;
                }
                updateCalculationStartToEnd();
            } else
            {
                CustomErrors customError = new CustomErrors(CustomErrors.Errors.restrictedTimeLineUpdateInValid, "The restricted timeline update cannot contain restriction time frames");
                throw customError;
            }
            
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

        protected override void updateCalculationStartToEnd()
        {
            _CalculationStartToEnd = new TimeLineRestricted(this.CalculationStart, this.End, this.RestrictionProfile, Now);
        }
        public override TimeLine CalculationStartToEnd
        {
            get
            {
                if (_CalculationStartToEnd!=null)
                {
                    return _CalculationStartToEnd;
                } else
                {
                    updateCalculationStartToEnd();
                    return _CalculationStartToEnd;
                }
            }
        }
    }
}
