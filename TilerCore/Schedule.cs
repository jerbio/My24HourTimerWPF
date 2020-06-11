#define StitcohRestrictedFromLeft
#define useLockedImplementation
#define useNonLockedImplementation
//#define INPROD
//#define EnableRestrictedLocationOptimization

//#define createCopyOfImplementation


//#define ForceSequentialSnugArray




//#define enableSequentialAcces

#define reversed
#define enableTimer



#if enableSequentialAcces
#undef enableMultithreading
#endif

#define enableDebugging

#define StitchRestrictedFromRight

#if StitchRestrictedFromLeft
#undef StitchRestrictedFromRight
#endif

using System.Threading;
using System.Threading.Tasks;
//#define EnableClashLog

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using TilerElements;


using System.IO;
using static TilerElements.TimeOfDayPreferrence;
using static TilerElements.LocationCacheEntry;
using util = TilerElements.Utility;

namespace TilerCore
{
    public class Schedule : IWhy
    {
        const int _optimizedDayLimit = 10;
        TimeSpan _cacheInvalidationTimeSpan = Utility.OneHourTimeSpan;
        TimeStone _TimeStone;
        public int OptimizedDayLimit
        {
            get
            {
                return _optimizedDayLimit;
            }
        }

        public virtual TimeStone TimeStone
        {
            get
            {
                return _TimeStone ?? (_TimeStone = new TimeStone() { schedule = this });
            }
        }
        public static int TimeLookUpDayStart {
            get {
                return Utility.defaultBeginDay;
            }
        }
        public static int TimeLookUpDayEnd {
            get {
                return Utility.defaultEndDay;
            }
        }
        string _Id { get; set; }
        bool _isScheduleModified = false;
        public bool IsScheduleModified
        {
            get
            {
                return _isScheduleModified;
            }
        }

        public TimeSpan CacheInvalidationTimeSpan
        {
            get
            {
                return _cacheInvalidationTimeSpan;
            }
        }

        public string Id
        {
            get
            {
                return _Id ?? (_Id = Guid.NewGuid().ToString());
            }
            set
            {
                _Id = value;
            }
        }
        protected Dictionary<string, HashSet<CalendarEvent>> RepeatParentToCalendarEvents;
        private Dictionary<string, CalendarEvent> AllEventDictionary;
        protected DateTimeOffset ReferenceDayTIime;
        protected Dictionary<string, Location> Locations;
        protected TimeLine CompleteSchedule;
        public TimeSpan TwentyFourHourTimeSpan = new TimeSpan(1, 0, 0, 0);
        public TimeSpan OnewWeekTimeSpan = new TimeSpan(7, 0, 0, 0);
        public TimeSpan HourTimeSpan = new TimeSpan(0, 1, 0, 0);
        protected Dictionary<ThirdPartyControl.CalendarTool, List<CalendarEvent>> ThirdPartyCalendars = new Dictionary<ThirdPartyControl.CalendarTool, List<CalendarEvent>>();
        protected DateTimeOffset StartofDay;
        protected bool retrievedThirdParty = false;
        protected TimeLine RangeOfLookup = null;

        protected bool UseTilerFront = false;
        Stopwatch myWatch = new Stopwatch();
        protected TilerUser TilerUser;
        protected int LatesMainID;
        TimeSpan TimeZoneDifference = new TimeSpan();
        protected Location _CurrentLocation = Location.getDefaultLocation();
        protected bool _isTravelCacheUpdated = false;
        protected TravelCache _TravelCache;

        protected double PercentageOccupancy = 0;
        protected ReferenceNow _Now;
        protected TimeSpan _MorningPreparationTime = TimeSpan.FromMinutes(45);// trying to get 45 minutes
        protected HashSet<SubCalendarEvent> _ConflictingSubEvents = new HashSet<SubCalendarEvent>();
        protected HashSet<SubCalendarEvent> _EvaluatedSubevents = new HashSet<SubCalendarEvent>(); // Subevents that were used to for schedule evaluation
        protected DataRetrivalOption retrievalOption = DataRetrivalOption.Evaluation;
        protected bool _OptimizationEnabled = true;
        protected bool _isConflictResolveEnabled = true;
        public ReferenceNow Now
        {
            get
            {
                return _Now;
            }
        }

        public Location getHomeLocation
        {
            get
            {
                Location home = null;
                if (Locations.ContainsKey("home"))
                {
                    home = Locations["home"];
                    return home;
                }
                else
                {
                    return Location.getDefaultLocation();
                }
            }
        }

        public bool OptimizationEnabled
        {
            get
            {
                return _OptimizationEnabled;
            }
        }

        public bool isConflictResolveEnabled
        {
            get
            {
                return _isConflictResolveEnabled;
            }
        }

        public Location CurrentLocation
        {
            set
            {
                Location val = value;
                if(val == null || val.isDefault || val.isNull)
                {
                    _CurrentLocation = Location.getDefaultLocation();
                } else
                {
                    _CurrentLocation = value;
                    if (this.TilerUser!=null && _CurrentLocation!=null)
                    {
                        this.TilerUser.LastKnownLongitude = _CurrentLocation.Longitude;
                        this.TilerUser.LastKnownLatitude = _CurrentLocation.Latitude;
                        this.TilerUser.LastKnownLocationVerified = !_CurrentLocation.isDefault && _CurrentLocation.isNull;
                    }
                } 
            }
            get
            {
                return _CurrentLocation;
            }
        }

        public TimeSpan MorningPreparationTime
        {
            set
            {
                _MorningPreparationTime = value;
            }
            get
            {
                return _MorningPreparationTime;
            }
        }

        public TravelCache TravelCache
        {
            get
            {
                return _TravelCache;
            }
        }

        public virtual IEnumerable<SubCalendarEvent> ConflictingSubEvents
        {
            get
            {
                return _ConflictingSubEvents;
            }
        }

        public virtual IEnumerable<SubCalendarEvent> EvaluatedSubEvents
        {
            get
            {
                return _EvaluatedSubevents;
            }
        }
        int DebugCounter = 0;

        #region Constructor
        protected Schedule()
        {
        }

        public Schedule(Dictionary<string, CalendarEvent> allEventDictionary, DateTimeOffset starOfDay, Dictionary<string, Location> locations, DateTimeOffset referenceNow, TilerUser user, TimeLine rangeOfLookup) : base()
        {
            _Now = new ReferenceNow(referenceNow, starOfDay, user.TimeZoneDifference);
            initializeAllEventDictionary(allEventDictionary.Values);
            TilerUser = user;
            TimeZoneDifference = user.TimeZoneDifference;
            this.RangeOfLookup = rangeOfLookup;
            this.Locations = locations;
        }


        #endregion

        #region Properties
        public TilerUser User
        {
            get
            {
                return TilerUser;
            }
        }

        public Dictionary<string, CalendarEvent> getAllEventDictionary
        {
            get
            {
                return AllEventDictionary;
            }
        }
        #endregion

        #region IwhyImplementation
        virtual public IWhy Because()
        {
            throw new NotImplementedException("Yet to implement a because functionality for subcalendar event");
        }

        virtual public IWhy OtherWise()
        {
            throw new NotImplementedException("Yet to implement a OtherWise functionality for subcalendar event");
        }

        virtual public IWhy WhatIf(params Reason[] reasons)
        {
            throw new NotImplementedException("Yet to implement a OtherWise functionality for subcalendar event");
        }

        virtual public IWhy WhatIf(TilerEvent modified, params Reason[] reasons)
        {

            modified.WhatIf(reasons);
            return this;
        }


        #endregion
        public virtual void disableDayOptimization()
        {
            _OptimizationEnabled = false;
        }

        public virtual void disableConflictResolution()
        {
            _isConflictResolveEnabled = false;
        }


        public virtual void updateTravelCache(TravelCache travelCache)
        {
            _isTravelCacheUpdated = true;
            this._TravelCache = travelCache;
        }

        public void updateDataSetWithThirdPartyData(Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>> ThirdPartyData)
        {
            if (ThirdPartyData != null)
            {
                ThirdPartyCalendars.Add(ThirdPartyData.Item1, ThirdPartyData.Item2.ToList());

                foreach (CalendarEvent ThirdPartyCalData in ThirdPartyData.Item2.SelectMany(o=> o.Repeat.RecurringCalendarEvents()))
                {
                    addCaleventToAllEventDictionary(ThirdPartyCalData.Calendar_EventID.getAllEventDictionaryLookup, ThirdPartyCalData);
                }
            }

            retrievedThirdParty = true;
        }

        public void updateDataSetWithThirdPartyData(Tuple<ThirdPartyControl.CalendarTool, CalendarEvent> ThirdPartyData)
        {
            if (ThirdPartyData != null)
            {
                List<CalendarEvent> CalEvents;
                if (ThirdPartyCalendars.ContainsKey(ThirdPartyData.Item1))
                {
                    CalEvents = ThirdPartyCalendars[ThirdPartyData.Item1];
                    CalEvents.Add(ThirdPartyData.Item2);
                }
                else
                {
                    CalEvents = new List<CalendarEvent>();
                    CalEvents.Add(ThirdPartyData.Item2);
                    ThirdPartyCalendars.Add(ThirdPartyData.Item1, CalEvents);
                }

                string id = ThirdPartyData.Item2.Calendar_EventID.getAllEventDictionaryLookup;

                if (AllEventDictionary.ContainsKey(id))
                {
                    AllEventDictionary[id] = ThirdPartyData.Item2;
                }
                else
                {
                    addCaleventToAllEventDictionary(id, ThirdPartyData.Item2);
                }
            }

            retrievedThirdParty = true;
        }



        protected void initializeThirdPartyCalendars()
        {
            ThirdPartyCalendars = new Dictionary<ThirdPartyControl.CalendarTool, List<CalendarEvent>>();
        }

        public void updateThirdPartyCalendars(ThirdPartyControl.CalendarTool calendarOption, IEnumerable<CalendarEvent> calendarEvents)
        {
            if (!ThirdPartyCalendars.ContainsKey(calendarOption))
            {
                ThirdPartyCalendars.Add(calendarOption, calendarEvents.ToList());
            }
            else
            {
                ThirdPartyCalendars[calendarOption].AddRange(calendarEvents);
            }
        }

        public CalendarEvent getCalendarEvent(string EventID)
        {
            EventID userEvent = new EventID(EventID);
            return getCalendarEvent(userEvent);
        }

        public virtual IEnumerable<SubCalendarEvent> getAllRelatedActiveSubEvents(EventID eventId)
        {
            return getAllRelatedCalendarEvents(eventId).SelectMany(obj => obj.ActiveSubEvents);
        }

        public virtual IEnumerable<SubCalendarEvent> getAllRelatedActiveSubEvents(string eventId)
        {
            EventID eventIdObj = new EventID(eventId);
            return getAllRelatedActiveSubEvents(eventIdObj);
        }

        public virtual IEnumerable<SubCalendarEvent> getAllRelatedAllSubEvents(EventID eventId)
        {
            return getAllRelatedCalendarEvents(eventId).SelectMany(obj => obj.AllSubEvents);
        }

        public virtual IEnumerable<SubCalendarEvent> getAllRelatedAllSubEvents(string eventId)
        {
            EventID eventIdObj = new EventID(eventId);
            return getAllRelatedActiveSubEvents(eventIdObj);
        }


        public virtual IEnumerable<CalendarEvent> getAllRelatedCalendarEvents (string eventId)
        {
            EventID eventIdObj = new EventID(eventId);
            return getAllRelatedCalendarEvents(eventIdObj);
        }
        public virtual IEnumerable<CalendarEvent> getAllRelatedCalendarEvents (EventID eventId)
        {
            string calendarId = eventId.getCalendarEventComponent();
            var retValue = RepeatParentToCalendarEvents[calendarId];
            return retValue;
        }

        /// <summary>
        /// function retrieves a calendarevent. 
        /// If the ID is repeating event ID it'll get the repeating calendar event
        /// </summary>
        /// <param name="myEventID"></param>
        /// <returns></returns>
        public CalendarEvent getCalendarEvent(EventID myEventID)
        {
            CalendarEvent calEvent = null, repeatEvent = null;
            if (AllEventDictionary.ContainsKey(myEventID.getAllEventDictionaryLookup))
            {
                calEvent = AllEventDictionary[myEventID.getAllEventDictionaryLookup];
            } else
            {
                string calendarId = myEventID.getCalendarEventID();
                if (AllEventDictionary.ContainsKey(calendarId))
                {
                    calEvent = AllEventDictionary[calendarId];
                    repeatEvent = calEvent.getRepeatedCalendarEvent(myEventID.getIDUpToRepeatCalendarEvent());
                }
            }


            if (repeatEvent == null)
            {
                return calEvent;
            }
            else
            {
                return repeatEvent;
            }
        }
        

        public ProcrastinateCalendarEvent getProcrastinateAllEvent()
        {
            EventID myEventID = new EventID(User.getClearAllEventsId());

            ProcrastinateCalendarEvent retValue = getCalendarEvent(myEventID) as ProcrastinateCalendarEvent;
            return retValue;
        }

        public SubCalendarEvent getSubCalendarEvent(EventID EventID)
        {
            return getSubCalendarEvent(EventID.ToString());
        }

        public SubCalendarEvent getSubCalendarEvent(string EventID)
        {
            CalendarEvent myCalendarEvent = getCalendarEvent(EventID);
            return myCalendarEvent.getSubEvent(new EventID(EventID));
        }

        public IEnumerable<CalendarEvent> getGoogleCalendarEvents()
        {
            List<CalendarEvent> retValue = this.ThirdPartyCalendars[ThirdPartyControl.CalendarTool.google];
            return retValue;
        }

        protected bool removeFromAllEventDictionary(string eventId)
        {
            EventID eventIdObj = new EventID(eventId);
            var calEvent = getCalendarEvent(eventIdObj);
            bool retValue = AllEventDictionary.Remove(eventIdObj.getAllEventDictionaryLookup);
            if(calEvent!=null)
            {
                string calendarEventId = calEvent.Calendar_EventID.getCalendarEventComponent();
                if(RepeatParentToCalendarEvents.ContainsKey(calendarEventId))
                {
                    HashSet<CalendarEvent> calEvents = RepeatParentToCalendarEvents[calendarEventId];
                    calEvents.Remove(calEvent);
                    if (calEvents.Count < 1)
                    {
                        RepeatParentToCalendarEvents.Remove(calendarEventId);
                    }
                }
                
            }
            
            return retValue;
        }

        public void updateAllEventDictionary(CalendarEvent calendarEvent)
        {
            if (!calendarEvent.IsFromRecurringAndNotChildRepeatCalEvent)
            {
                if (AllEventDictionary.ContainsKey(calendarEvent.Calendar_EventID.getAllEventDictionaryLookup))
                {
                    removeFromAllEventDictionary(calendarEvent.Calendar_EventID.getAllEventDictionaryLookup);
                }
            } else
            {
                foreach (CalendarEvent calEvent in calendarEvent.Repeat.RecurringCalendarEvents())
                {
                    removeFromAllEventDictionary(calEvent.Calendar_EventID.getAllEventDictionaryLookup);
                }
                removeFromAllEventDictionary(calendarEvent.Calendar_EventID.getAllEventDictionaryLookup);
            }
                
            addCalendarEventToAllEventDictionary(calendarEvent);
        }

        public virtual void addCalendarEventToAllEventDictionary(CalendarEvent calendarEvent)
        {
            addCaleventToAllEventDictionary(calendarEvent.getTilerID.getAllEventDictionaryLookup, calendarEvent);
        }

        virtual public void initializeAllEventDictionary()
        {
            AllEventDictionary = new Dictionary<string, CalendarEvent>();
            RepeatParentToCalendarEvents = new Dictionary<string, HashSet<CalendarEvent>>();
        }

        virtual protected void initializeAllEventDictionary(IEnumerable<CalendarEvent> calEvents)
        {
            initializeAllEventDictionary();
            foreach (CalendarEvent calEvent in calEvents)
            {
                addCalendarEventToAllEventDictionary(calEvent);
            }
        }

        void addCaleventToAllEventDictionary(EventID eventId, CalendarEvent NewEvent)
        {
            if(!NewEvent.IsFromRecurringAndNotChildRepeatCalEvent)
            {
                AllEventDictionary.Add(eventId.getAllEventDictionaryLookup, NewEvent);
                NewEvent.TimeOfScheduleLoad = Now.constNow;
                NewEvent.Now = Now;
            } else
            {
                if(NewEvent.isRepeatLoaded)
                {
                    foreach (CalendarEvent calEvent in NewEvent.RecurringCalendarEvents)
                    {
                        addCaleventToAllEventDictionary(calEvent.Calendar_EventID.getAllEventDictionaryLookup, calEvent);
                    }
                }
                AllEventDictionary.Add(eventId.getAllEventDictionaryLookup, NewEvent);
                NewEvent.TimeOfScheduleLoad = Now.constNow;
                NewEvent.Now = Now;
            }
            string calendarEventId = NewEvent.Calendar_EventID.getCalendarEventComponent();
            HashSet<CalendarEvent> calendarEvents;
            if (RepeatParentToCalendarEvents.ContainsKey(calendarEventId))
            {
                calendarEvents = RepeatParentToCalendarEvents[calendarEventId];
            } else
            {
                calendarEvents = new HashSet<CalendarEvent>();
                RepeatParentToCalendarEvents.Add(calendarEventId, calendarEvents);
            }
            calendarEvents.Add(NewEvent);
        }

        void addCaleventToAllEventDictionary(string eventId, CalendarEvent NewEvent)
        {
            EventID EventIdObj = new EventID(eventId);
            addCaleventToAllEventDictionary(EventIdObj, NewEvent);
        }

        public virtual Dictionary<string, CalendarEvent>  getDeepCopyOfEventDictionary()
        {
            Dictionary<string, CalendarEvent> retVallue = new Dictionary<string, CalendarEvent>();
            retVallue = AllEventDictionary.ToDictionary(obj => obj.Key, obj => obj.Value.createCopy());
            return retVallue;
        }

        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> BundleChangeUpdate(string SubEventID,
            EventName NewName,
            DateTimeOffset SubeventStart,
            DateTimeOffset SubeventEnd,
            DateTimeOffset TimeLineStart,
            DateTimeOffset TimeLineEnd,
            int SplitCount,
            string Notes)
        {
            EventID myEventID = new EventID(SubEventID);
            SubCalendarEvent mySubCalEvent = getSubCalendarEvent(SubEventID);
            CalendarEvent myCalendarEvent = getCalendarEvent(SubEventID);
            DateTimeOffset calEventStart = TimeLineStart.isBeginningOfTime() ? myCalendarEvent.Start : TimeLineStart;
            DateTimeOffset calEventEnd = TimeLineEnd.isBeginningOfTime() ? myCalendarEvent.End : TimeLineEnd;
            TimeLine calendarEventRange = null;
            bool isFromRigidEvent = false;

            ProcrastinateCalendarEvent procrastinateCalEvent = (mySubCalEvent.ParentCalendarEvent as ProcrastinateCalendarEvent);
            if (procrastinateCalEvent != null)
            {
                SplitCount = procrastinateCalEvent.NumberOfSplit;
            }

            calendarEventRange = new TimeLine(calEventStart, calEventEnd);
            if (mySubCalEvent.isLocked && myCalendarEvent.isLocked)
            {
                calendarEventRange = new TimeLine(SubeventStart, SubeventEnd);
                calEventStart = SubeventStart;
                calEventEnd = SubeventEnd;
                isFromRigidEvent = true;
            }

            SubCalendarEvent ChangedSubCal;
            if (!mySubCalEvent.getIsEventRestricted)
            {
                ChangedSubCal = new SubCalendarEvent(mySubCalEvent.ParentCalendarEvent,
                    mySubCalEvent.getCreator,
                    mySubCalEvent.getAllUsers(),
                    mySubCalEvent.getTimeZone,
                    mySubCalEvent.Id,
                    mySubCalEvent.getName,
                    SubeventStart,
                    SubeventEnd,
                    new BusyTimeLine(mySubCalEvent.Id, SubeventStart, SubeventEnd),
                    mySubCalEvent.isRigid,
                    mySubCalEvent.isEnabled,
                    mySubCalEvent.getUIParam, mySubCalEvent.Notes, mySubCalEvent.getIsComplete, mySubCalEvent.LocationObj, calendarEventRange, mySubCalEvent.Conflicts);
            }
            else
            {
                ChangedSubCal = new SubCalendarEventRestricted((CalendarEventRestricted)mySubCalEvent.ParentCalendarEvent,
                    mySubCalEvent.getCreator, mySubCalEvent.getAllUsers(), mySubCalEvent.ParentCalendarEvent.Id,
                    mySubCalEvent.getName, SubeventStart, SubeventEnd,
                    ((SubCalendarEventRestricted)mySubCalEvent).RestrictionProfile, mySubCalEvent.ParentCalendarEvent.StartToEnd,
                    mySubCalEvent.isEnabled, mySubCalEvent.getIsComplete, mySubCalEvent.Conflicts, mySubCalEvent.isRigid,
                    new TimeSpan(), new TimeSpan(),
                    mySubCalEvent.LocationObj, mySubCalEvent.getUIParam, mySubCalEvent.Notes, Now, mySubCalEvent.ParentCalendarEvent.getNowInfo, mySubCalEvent.Priority_EventDB, mySubCalEvent.ThirdPartyID, subEventID: mySubCalEvent.Id);
            }
            ChangedSubCal.updateLocationValidationId(mySubCalEvent.LocationValidationId);



            //bool InitialRigidStatus = mySubCalEvent.Rigid;
            TimeSpan timeSpanStartDiff = TimeSpan.FromTicks(Math.Abs((mySubCalEvent.Start - SubeventStart).Ticks));
            TimeSpan timeSpanEndDiff = TimeSpan.FromTicks(Math.Abs((mySubCalEvent.End - SubeventEnd).Ticks));
            bool subEventTimeLineChange = (timeSpanStartDiff >= TimeSpan.FromMinutes(1)) || (timeSpanEndDiff >= TimeSpan.FromMinutes(1));

            if (subEventTimeLineChange)
            {
                if (!isFromRigidEvent)
                {
                    mySubCalEvent.UpdateThis(ChangedSubCal);
                    myCalendarEvent.RigidizeSubEvent(mySubCalEvent.Id);
                    mySubCalEvent.tempLockSubEvent();
                }
                else
                {
                    mySubCalEvent.shiftEvent(ChangedSubCal.Start, true);
                    mySubCalEvent.UpdateThis(ChangedSubCal);
                }
                calEventStart = calendarEventRange.Start < mySubCalEvent.Start ? calendarEventRange.Start : mySubCalEvent.Start;
                calEventEnd = calendarEventRange.End > mySubCalEvent.End ? calendarEventRange.End : mySubCalEvent.End;
            }
            calendarEventRange = new TimeLine(calEventStart, calEventEnd);
            return BundleChangeUpdate(mySubCalEvent.SubEvent_ID.ToString(), NewName, calendarEventRange.Start, calendarEventRange.End, SplitCount, Notes, subEventTimeLineChange, mySubCalEvent);
        }
        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> BundleChangeUpdate(string EventId, EventName NewName, DateTimeOffset newStart, DateTimeOffset newEnd, int newSplitCount, string notes, bool forceRecalculation = false, SubCalendarEvent triggerSubEvent = null)
        {
            CalendarEvent myCalendarEvent = getCalendarEvent(EventId);
            TimeLine initialTimeLine = myCalendarEvent.StartToEnd;
            bool isNameChange = NewName.NameValue != myCalendarEvent.getName?.NameValue;
            bool isDeadlineChange = (newEnd) != myCalendarEvent.End;
            bool isStartChange = newStart != myCalendarEvent.Start;
            bool isSplitDiff = myCalendarEvent.NumberOfSplit != newSplitCount;

            if (isSplitDiff || isStartChange || isDeadlineChange || forceRecalculation)
            {
                myCalendarEvent.updateNumberOfSplits(newSplitCount);
                if (triggerSubEvent != null)
                {
                    TimeLine newCalTimeLine = new TimeLine(newStart, newEnd);
                    myCalendarEvent.updateTimeLine(triggerSubEvent, newCalTimeLine);
                }
                else
                {
                    myCalendarEvent.updateTimeLine(new TimeLine(newStart, newEnd));
                }

                HashSet<SubCalendarEvent> NoDoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
                foreach (SubCalendarEvent subEvent in myCalendarEvent.ActiveSubEvents)
                {
                    NoDoneYet.Remove(subEvent);
                }

                foreach (SubCalendarEvent subEVent in myCalendarEvent.ActiveSubEvents)
                {
                    if (!subEVent.IsDateTimeWithin(Now.constNow))
                    {
                        subEVent.PinToEnd(myCalendarEvent.StartToEnd);
                    }
                }


                if (
                    (
                    (triggerSubEvent == null && (myCalendarEvent.End > Now.constNow)) ||
                    (triggerSubEvent.isLocked && triggerSubEvent.End > Now.constNow) ||
                    (!triggerSubEvent.isLocked && (myCalendarEvent.End > Now.constNow)))
                    )
                {
                    myCalendarEvent = EvaluateTotalTimeLineAndAssignValidTimeSpots(myCalendarEvent, NoDoneYet, null, null, 0);
                }
            }

            if (isNameChange)
            {
                myCalendarEvent.updateEventName(NewName.NameValue);
            }
            var note = myCalendarEvent.Notes;
            if (note == null)
            {
                note = (triggerSubEvent.RepeatParentEvent as CalendarEvent).Notes;
            }


            note.UserNote = notes;

            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue = new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(myCalendarEvent.Error, AllEventDictionary);
            return retValue;
        }

        /// <summary>
        /// Function simply updates the schedule with nothing. Meaning it just triggers a change without adding removing or shuffling the schedule
        /// </summary>
        /// <returns></returns>
        public CustomErrors UpdateSchedule()
        {
            DateTimeOffset start = Now.constNow.AddDays(-1);
            DateTimeOffset end = start.AddMilliseconds(1);

            HashSet<SubCalendarEvent> NoDoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            EventID eventId = new EventID("0");
            CalendarEvent tempCalendarEvent = CalendarEvent.getEmptyCalendarEvent(eventId, start, end);//creates an "empty" calendar event. If the calEvent is rigid it has time span of zero
            tempCalendarEvent = EvaluateTotalTimeLineAndAssignValidTimeSpots(tempCalendarEvent, NoDoneYet, null, null, 0);
            CustomErrors retValue = tempCalendarEvent.Error;
            return retValue;
        }


        public BusyTimeLine NextActivity
        {
            get
            {
                //KeyValuePair<string, int> 
                List<BusyTimeLine> MyTotalSubEvents = new List<BusyTimeLine>(0);
                foreach (CalendarEvent eachCalendarEvent in getAllCalendarEvents().Where(obj => obj.isActive))
                {
                    foreach (SubCalendarEvent MySubCalendarEvent in eachCalendarEvent.ActiveSubEvents)
                    {
                        MyTotalSubEvents.Add(MySubCalendarEvent.ActiveSlot);
                    }
                }
                MyTotalSubEvents = Schedule.SortBusyTimeline(MyTotalSubEvents, true);
                DateTimeOffset MyNow = Now.constNow;//Moved Out of For loop for Speed boost
                for (int i = 0; i < MyTotalSubEvents.Count; i++)
                {
                    if (MyTotalSubEvents[i].Start > MyNow)
                    {
                        return MyTotalSubEvents[i];
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Function creates a timeline of all the events that were retrieved from storage, and newly added events.
        /// </summary>
        /// <returns></returns>
        protected virtual TimeLine getTimeLine()
        {
            DateTimeOffset LastDeadline = Now.calculationNow.AddHours(1);
            List<BusyTimeLine> MyTotalBusySlots = new List<BusyTimeLine>(0);
            //var Holder=new List();
            foreach (CalendarEvent MyCalendarEvent in getAllCalendarEvents())
            {
                var Holder = MyTotalBusySlots.Concat(GetBusySlotPerCalendarEvent(MyCalendarEvent));
                MyTotalBusySlots = Holder.ToList();
            }
            MyTotalBusySlots = SortBusyTimeline(MyTotalBusySlots, true);
            TimeLine MyTimeLine = new TimeLine(Now.calculationNow, Now.calculationNow.AddHours(1));
            if (MyTotalBusySlots.Count > 0)
            {
                MyTimeLine = new TimeLine(Now.calculationNow, MyTotalBusySlots[MyTotalBusySlots.Count - 1].End);
            }
            MyTimeLine.OccupiedSlots = MyTotalBusySlots.ToArray();
            return MyTimeLine;
        }

        public List<SubCalendarEvent> getSubweventsForDay(DateTimeOffset time)
        {
            DayTimeLine daytimeLine = Now.getDayTimeLineByTime(time);
            List<SubCalendarEvent> retValue = daytimeLine.getSubEventsInTimeLine();
            return retValue;
        }

        /// <summary>
        /// Gets current Calendar events in Memory. It does not retrieve data from DB
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CalendarEvent> getAllCalendarEvents()
        {
            return AllEventDictionary.Values;
        }

        /// <summary>
        /// Gets calendar events that are not completed and not deleted. 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CalendarEvent> getAllActiveCalendarEvents()
        {
            IEnumerable<CalendarEvent> retValue = AllEventDictionary.Values
                .Where(calEvent => (
                                    (calEvent.isActive && calEvent.IsFromRecurringAndIsChildCalEvent && (calEvent.RepeatParentEvent == null || calEvent.RepeatParentEvent.isActive)) ||
                                    (!calEvent.IsFromRecurring && calEvent.isActive))); 


            return retValue;
        }

        /// <summary>
        /// Gets current Active subevents in Memory. It does not retrieve data from DB
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SubCalendarEvent> getAllActiveSubEvents()
        {
            return getAllCalendarEvents().SelectMany(cal => cal.ActiveSubEvents);
        }

        /// <summary>
        /// Gets current Calendar events in Memory. It does not retrieve data from DB
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CalendarEvent> getOnlyTilerCalendarEvents()
        {
            return getAllCalendarEvents().Where(calEvent => calEvent.ThirdpartyType == ThirdPartyControl.CalendarTool.tiler);
        }

        /// <summary>
        /// Gets current Calendar events in Memory. It does not retrieve data from DB
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Location> getAllLocations()
        {
            return this.Locations.Values;
        }

        /// <summary>
        /// Function makes clears the cached locations that haven't being used in over 30 days
        /// </summary>
        protected void purgeLocations()
        {
            foreach(var location in this.Locations.Values)
            {
                location.purgeLocationCache(Now.constNow);
            }
        }

        public Location getLocation(string locationDescription)
        {
            Location retValue = this.Locations[locationDescription.ToLower()];
            return retValue;
        }

        public void EmptyMemory()
        {
            AllEventDictionary = new Dictionary<string, CalendarEvent>();
        }

        BusyTimeLine[] GetBusySlotPerCalendarEvent(CalendarEvent MyEvent)
        {
            List<BusyTimeLine> MyTotalSubEventBusySlots = new List<BusyTimeLine>(0);
            BusyTimeLine[] ArrayOfBusySlotsInRepeat = new BusyTimeLine[0];
            DateTimeOffset LastDeadline = Now.calculationNow.AddHours(1);

            if (MyEvent.IsFromRecurringAndNotChildRepeatCalEvent && MyEvent.Repeat!=null)
            {
                ArrayOfBusySlotsInRepeat = GetBusySlotsPerRepeat(MyEvent.Repeat);
            }

            foreach (SubCalendarEvent MySubCalendarEvent in MyEvent.ActiveSubEvents)//Active Fix
            {
                if (!MyEvent.IsFromRecurringAndNotChildRepeatCalEvent)
                { MyTotalSubEventBusySlots.Add(MySubCalendarEvent.ActiveSlot); }
            }

            var Holder = MyTotalSubEventBusySlots.Concat(ArrayOfBusySlotsInRepeat);
            BusyTimeLine[] ConcatenatSumOfAllBusySlots = Holder.ToArray();
            return ConcatenatSumOfAllBusySlots;
        }

        BusyTimeLine[] GetBusySlotsPerRepeat(Repetition RecurringEvents)
        {
            int i = 0;
            BusyTimeLine[] ArrayOfBusySlotsPerRecurringEvents;
            List<BusyTimeLine[]> MyListOfWithArrayOfBusySlots = new List<BusyTimeLine[]>();//this can be used as a list since we know the limits of each repeating element. Only using list becaue it'll be faster to implement
            CalendarEvent[] AllRepeatingEvents = RecurringEvents.RecurringCalendarEvents();
            for (; i < AllRepeatingEvents.Length; i++)
            {
                MyListOfWithArrayOfBusySlots.Add(GetBusySlotPerCalendarEvent(AllRepeatingEvents[i]));
            }
            List<BusyTimeLine> MyListOfBusySlots = new List<BusyTimeLine>();
            i = 0;
            int j = 0;
            for (; i < MyListOfWithArrayOfBusySlots.Count; i++)
            {
                j = 0;
                for (; j < MyListOfWithArrayOfBusySlots[i].Length; j++)
                {
                    MyListOfBusySlots.Add(MyListOfWithArrayOfBusySlots[i][j]);
                }
            }

            return MyListOfBusySlots.ToArray();


        }


        string LogInfo = "";


        /// <summary>
        /// function returns the subevents that interfere with the current now time
        /// </summary>
        /// <returns></returns>
        List<SubCalendarEvent> getCurrentSubEvent()
        {
            List<SubCalendarEvent> RetValue = getAllCalendarEvents().SelectMany(obj => obj.ActiveSubEvents).Where(obj => obj.IsDateTimeWithin(Now.constNow)).ToList();
            return RetValue;
        }
        async public Task<CustomErrors> PauseEvent()
        {
            List<SubCalendarEvent> SubEvents = getCurrentSubEvent();
            SubEvents = SubEvents.OrderByDescending(obj => obj.getActiveDuration).ToList();
            SubCalendarEvent relevantSubEvent = SubEvents.FirstOrDefault();
            CustomErrors RetValue;
            if (relevantSubEvent != null)
            {
                RetValue = await PauseEvent(relevantSubEvent.SubEvent_ID);
            }
            else
            {
                RetValue = null;
            }
            return RetValue;
        }

        public async Task<CustomErrors> PauseEvent(string Event, string CurrentPausedEventId = null)
        {
            EventID id = new EventID(Event);
            EventID currentPausedId = null;
            if (!string.IsNullOrEmpty(CurrentPausedEventId))
            {
                currentPausedId = new EventID(CurrentPausedEventId);
            }
            return await PauseEvent(id, currentPausedId);
        }

        public async Task<CustomErrors> PauseEvent(EventID EventId, EventID CurrentPausedEventId = null)
        {
            CalendarEvent CalEvent = getCalendarEvent(EventId.ToString());

            if (CurrentPausedEventId != null)
            {
                CalendarEvent PreviousPausedCalEvent = getCalendarEvent(CurrentPausedEventId.ToString());
                SubCalendarEvent currentPausedEvent = PreviousPausedCalEvent.getSubEvent(CurrentPausedEventId);
                currentPausedEvent.UnPause(Now.constNow);
            }

            CalEvent.PauseSubEvent(EventId, Now.constNow, CurrentPausedEventId);
            //await UpdateWithDifferentSchedule(AllEventDictionary);
            CustomErrors RetValue = null;
            return RetValue;
        }

        public async Task<CustomErrors> ContinueEvent(string Event)
        {
            EventID id = new EventID(Event);
            return await ResumeEvent(id);
        }

        public async Task<CustomErrors> ResumeEvent(EventID EventId)
        {
            CalendarEvent CalEvent = getCalendarEvent(EventId.ToString());
            if (CalEvent.IsFromRecurringAndNotChildRepeatCalEvent)
            {
                CalEvent = CalEvent.getRepeatedCalendarEvent(EventId.getIDUpToRepeatCalendarEvent());
            }

            bool errorState = CalEvent.ContinueSubEvent(EventId, Now.constNow);
            SubCalendarEvent SubEvent = CalEvent.getSubEvent(EventId);
            CustomErrors RetValue;
            {
                if (errorState)
                {

                    Now.UpdateNow(SubEvent.Start);
                    CalendarEvent CalEventCopy = CalEvent.createCopy(EventID.GenerateCalendarEvent());
                    SubEvent.disable(CalEvent);
                    SubCalendarEvent unDisabled = CalEventCopy.ActiveSubEvents.First();
                    foreach (SubCalendarEvent SubCalendarEvent in CalEventCopy.AllSubEvents.Except(new List<SubCalendarEvent>() { unDisabled }))
                    {
                        SubCalendarEvent.disable(CalEventCopy);
                    }
                    TimeSpan timeDiffBeforePause = (SubEvent.Start - unDisabled.Start);
                    unDisabled.shiftEvent(timeDiffBeforePause);
                    unDisabled.tempLockSubEvent();

                    HashSet<SubCalendarEvent> NotDoneYets = getNoneDoneYetBetweenNowAndReerenceStartTIme();
                    NotDoneYets.RemoveWhere(obj => obj.StartToEnd.doesTimeLineInterfere(unDisabled.StartToEnd));
                    CalEventCopy = EvaluateTotalTimeLineAndAssignValidTimeSpots(CalEventCopy, NotDoneYets, null, InterringWithNowEvent: 2);


                    SubEvent.Enable(CalEvent);
                    TimeSpan timeDiff = (unDisabled.Start - SubEvent.Start);
                    SubEvent.shiftEvent(timeDiff);
                    RetValue = null;
                }

                else
                {
                    RetValue = new CustomErrors("could not continue sub event because it is out of calendar event range error", 40000001);
                }
            }




            return RetValue;
        }



        public async Task<CustomErrors> deleteCalendarEventAndReadjust(string EventId)
        {
            CalendarEvent removedCalEvent = getCalendarEvent(EventId);
            CalendarEvent CalendarEventTOBeRemoved = removedCalEvent;


            CalendarEventTOBeRemoved.Disable(false);
            //CalendarEventTOBeRemoved.DisableSubEvents(CalendarEventTOBeRemoved.ActiveSubEvents);
            if (CalendarEventTOBeRemoved.isLocked)
            {
                CalendarEventTOBeRemoved = new RigidCalendarEvent(
                    CalendarEventTOBeRemoved.getName, CalendarEventTOBeRemoved.Start, CalendarEventTOBeRemoved.End, CalendarEventTOBeRemoved.getActiveDuration, CalendarEventTOBeRemoved.getPreparation, CalendarEventTOBeRemoved.getPreDeadline, new Repetition(), CalendarEventTOBeRemoved.Location, new EventDisplay(), new MiscData(), false, false, CalendarEventTOBeRemoved.getCreator, CalendarEventTOBeRemoved.getAllUsers(), CalendarEventTOBeRemoved.getTimeZone, null, CalendarEventTOBeRemoved.getNowInfo, CalendarEventTOBeRemoved.TimeLineHistory);
            }
            else
            {
                CalendarEventTOBeRemoved = new CalendarEvent(
                    CalendarEventTOBeRemoved.getName, CalendarEventTOBeRemoved.Start, CalendarEventTOBeRemoved.End, CalendarEventTOBeRemoved.getActiveDuration, CalendarEventTOBeRemoved.getPreparation, CalendarEventTOBeRemoved.getPreDeadline, 1, new Repetition(), CalendarEventTOBeRemoved.Location, new EventDisplay(), new MiscData(), null, new NowProfile(), false, false, CalendarEventTOBeRemoved.getCreator, CalendarEventTOBeRemoved.getAllUsers(), CalendarEventTOBeRemoved.getTimeZone, null, CalendarEventTOBeRemoved.TimeLineHistory);
            }
            CalendarEventTOBeRemoved.DisableSubEvents(CalendarEventTOBeRemoved.ActiveSubEvents);

            HashSet<SubCalendarEvent> NotDOneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            CalendarEvent retValue = EvaluateTotalTimeLineAndAssignValidTimeSpots(CalendarEventTOBeRemoved, NotDOneYet, null);


            removeFromAllEventDictionary(CalendarEventTOBeRemoved.getId);//removes the false calendar event
                                                                      //            await UpdateWithDifferentSchedule(AllEventDictionary).ConfigureAwait(false);
            return retValue.Error;
        }






        public async Task<CustomErrors> markAsCompleteCalendarEventAndReadjust(string EventId)
        {
            CalendarEvent CalendarEventTOBeRemoved = getCalendarEvent(EventId);
            CalendarEventTOBeRemoved.SetCompletion(true, this.Now);

            if (CalendarEventTOBeRemoved.isLocked)
            {
                CalendarEventTOBeRemoved = new RigidCalendarEvent(
                    CalendarEventTOBeRemoved.getName, CalendarEventTOBeRemoved.Start, CalendarEventTOBeRemoved.End, CalendarEventTOBeRemoved.getActiveDuration, CalendarEventTOBeRemoved.getPreparation, CalendarEventTOBeRemoved.getPreDeadline, new Repetition(), CalendarEventTOBeRemoved.Location, new EventDisplay(), new MiscData(), true, true, CalendarEventTOBeRemoved.getCreator, CalendarEventTOBeRemoved.getAllUsers(), CalendarEventTOBeRemoved.getTimeZone, null, CalendarEventTOBeRemoved.getNowInfo, CalendarEventTOBeRemoved.TimeLineHistory);
            }
            else
            {
                CalendarEventTOBeRemoved = new CalendarEvent(
                    CalendarEventTOBeRemoved.getName, CalendarEventTOBeRemoved.Start, CalendarEventTOBeRemoved.End, CalendarEventTOBeRemoved.getActiveDuration, CalendarEventTOBeRemoved.getPreparation, CalendarEventTOBeRemoved.getPreDeadline, 1, new Repetition(), CalendarEventTOBeRemoved.Location, new EventDisplay(), new MiscData(), null, new NowProfile(), true, true, CalendarEventTOBeRemoved.getCreator, CalendarEventTOBeRemoved.getAllUsers(), CalendarEventTOBeRemoved.getTimeZone, null, CalendarEventTOBeRemoved.TimeLineHistory);
            }
            CalendarEventTOBeRemoved.DisableSubEvents(CalendarEventTOBeRemoved.ActiveSubEvents);


            HashSet<SubCalendarEvent> NotDOneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            CalendarEvent retValue = EvaluateTotalTimeLineAndAssignValidTimeSpots(CalendarEventTOBeRemoved, NotDOneYet, null);


            removeFromAllEventDictionary(CalendarEventTOBeRemoved.getId);//removes the false calendar event
            //await UpdateWithDifferentSchedule(AllEventDictionary).ConfigureAwait(false);
            return retValue.Error;
        }



        public void markSubEventAsCompleteCalendarEventAndReadjust(string EventId)
        {

            CalendarEvent referenceCalendarEventWithSubEvent = getCalendarEvent(EventId);
            SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(EventId);

            EventID SubEventID = new EventID(EventId);

            if (ReferenceSubEvent != null)
            {

                bool InitialRigid = ReferenceSubEvent.isLocked;


                if (referenceCalendarEventWithSubEvent.IsFromRecurringAndNotChildRepeatCalEvent && referenceCalendarEventWithSubEvent.isRepeatLoaded)
                {
                    referenceCalendarEventWithSubEvent = referenceCalendarEventWithSubEvent.getRepeatedCalendarEvent(SubEventID.getIDUpToRepeatCalendarEvent());
                }
                Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
                AllEventDictionary_Cpy = getDeepCopyOfEventDictionary();
                List<SubCalendarEvent> AllValidSubCalEvents = new List<SubCalendarEvent>() { ReferenceSubEvent };// ProcrastinateEvent.AllActiveSubEvents.Where(obj => obj.End > ReferenceSubEvent.Start).ToList();
                DateTimeOffset StartTime = Now.calculationNow;
                DateTimeOffset EndTime = StartTime.Add(ReferenceSubEvent.getActiveDuration); ;
                ReferenceSubEvent.SetCompletionStatus(true, referenceCalendarEventWithSubEvent, this.Now);


                TimeSpan TotalActiveDuration = Utility.SumOfActiveDuration(AllValidSubCalEvents);

                DateTimeOffset StartData = DateTimeOffset.Parse(referenceCalendarEventWithSubEvent.Start.ToString("hh:mm tt") + " " + referenceCalendarEventWithSubEvent.Start.Date.ToShortDateString());
                DateTimeOffset EndData = DateTimeOffset.Parse(referenceCalendarEventWithSubEvent.End.ToString("hh:mm tt") + " " + referenceCalendarEventWithSubEvent.End.Date.ToShortDateString());

                //CalendarEvent(string NameEntry, string StartTime, DateTimeOffset StartDateEntry, string EndTime, DateTimeOffset EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag,Location EventLocation)
                CalendarEvent ScheduleUpdated;
                if (ReferenceSubEvent.isLocked)
                {
                    ScheduleUpdated = new RigidCalendarEvent(
                        //EventID.GenerateCalendarEvent(), 
                        referenceCalendarEventWithSubEvent.getName, StartData, EndData, ReferenceSubEvent.getActiveDuration, referenceCalendarEventWithSubEvent.getPreDeadline, referenceCalendarEventWithSubEvent.getPreparation, new Repetition(), ReferenceSubEvent.Location, new EventDisplay(), new MiscData(), true, true, referenceCalendarEventWithSubEvent.getCreator, referenceCalendarEventWithSubEvent.getAllUsers(), ReferenceSubEvent.getTimeZone, null, referenceCalendarEventWithSubEvent.getNowInfo, referenceCalendarEventWithSubEvent.TimeLineHistory);
                }
                else
                {
                    ScheduleUpdated = new CalendarEvent(
                        //EventID.GenerateCalendarEvent(), 
                        referenceCalendarEventWithSubEvent.getName, StartData, EndData, ReferenceSubEvent.getActiveDuration, referenceCalendarEventWithSubEvent.getPreparation, referenceCalendarEventWithSubEvent.getPreDeadline, 1, new Repetition(), ReferenceSubEvent.Location, new EventDisplay(), new MiscData(), null, new NowProfile(), true, true, referenceCalendarEventWithSubEvent.getCreator, referenceCalendarEventWithSubEvent.getAllUsers(), ReferenceSubEvent.getTimeZone, null, referenceCalendarEventWithSubEvent.TimeLineHistory);
                }
                ScheduleUpdated.DisableSubEvents(ScheduleUpdated.AllSubEvents);//hackalert


                HashSet<SubCalendarEvent> NotDOneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
                ScheduleUpdated = EvaluateTotalTimeLineAndAssignValidTimeSpots(ScheduleUpdated, NotDOneYet, null);

                removeFromAllEventDictionary(ScheduleUpdated.getId);//removes the false calendar event
            }
        }

        /// <summary>
        /// function marks a Subevent as complete. This process forces the calendarevent to check if its complete.
        /// </summary>
        /// <param name="EventID"></param>
        public async Task markSubEventAsComplete(string EventID)
        {

            CalendarEvent referenceCalendarEventWithSubEvent = getCalendarEvent(EventID);
            SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(EventID);
            if (ReferenceSubEvent != null)
            {
                ReferenceSubEvent.SetCompletionStatus(true, referenceCalendarEventWithSubEvent, this.Now);
            }
        }

        /// <summary>
        /// Function sets multiple subevents as complete. Using the IDs of each subevent
        /// </summary>
        /// <param name="EventIDs"></param>
        public async Task markSubEventsAsComplete(IEnumerable<string> EventIDs)
        {
            foreach (string EventID in EventIDs)
            {
                CalendarEvent referenceCalendarEventWithSubEvent = getCalendarEvent(EventID);
                SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(EventID);
                if(ReferenceSubEvent!=null)
                {
                    ReferenceSubEvent.SetCompletionStatus(true, referenceCalendarEventWithSubEvent, this.Now);
                }
            }
        }


        /// <summary>
        /// function takes an EventID, deletes from the schedule and attempts a schedule optimization
        /// </summary>
        /// <param name="EventId"></param>

        async public Task deleteSubCalendarEventAndReadjust(string EventId)
        {



            CalendarEvent referenceCalendarEventWithSubEvent = getCalendarEvent(EventId);
            SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(EventId);

            EventID SubEventID = new EventID(EventId);


            bool InitialRigid = ReferenceSubEvent.isLocked;


            if (referenceCalendarEventWithSubEvent.IsFromRecurringAndNotChildRepeatCalEvent && referenceCalendarEventWithSubEvent.isRepeatLoaded)
            {
                referenceCalendarEventWithSubEvent = referenceCalendarEventWithSubEvent.getRepeatedCalendarEvent(SubEventID.getIDUpToRepeatCalendarEvent());
            }

            Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
            AllEventDictionary_Cpy = getDeepCopyOfEventDictionary();

            List<SubCalendarEvent> AllValidSubCalEvents = new List<SubCalendarEvent>() { ReferenceSubEvent };// ProcrastinateEvent.AllActiveSubEvents.Where(obj => obj.End > ReferenceSubEvent.Start).ToList();
            DateTimeOffset StartTime = Now.calculationNow;
            DateTimeOffset EndTime = StartTime.Add(ReferenceSubEvent.getActiveDuration); ;



            referenceCalendarEventWithSubEvent.DisableSubEvents(AllValidSubCalEvents);

            TimeSpan TotalActiveDuration = Utility.SumOfActiveDuration(AllValidSubCalEvents);
            //CalendarEvent(string NameEntry, string StartTime, DateTimeOffset StartDateEntry, string EndTime, DateTimeOffset EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag,Location EventLocation)

            DateTimeOffset StartData = DateTimeOffset.Parse(referenceCalendarEventWithSubEvent.Start.ToString("hh:mm tt") + " " + referenceCalendarEventWithSubEvent.Start.Date.ToShortDateString());
            DateTimeOffset EndData = DateTimeOffset.Parse(referenceCalendarEventWithSubEvent.End.ToString("hh:mm tt") + " " + referenceCalendarEventWithSubEvent.End.Date.ToShortDateString());

            CalendarEvent ScheduleUpdated;
            if (ReferenceSubEvent.isLocked)
            {
                ScheduleUpdated = new RigidCalendarEvent(
                    referenceCalendarEventWithSubEvent.getName, StartData, EndData, ReferenceSubEvent.getActiveDuration, referenceCalendarEventWithSubEvent.getPreparation, referenceCalendarEventWithSubEvent.getPreDeadline, new Repetition(), ReferenceSubEvent.Location, new EventDisplay(), new MiscData(), false, false, ReferenceSubEvent.getCreator, ReferenceSubEvent.getAllUsers(), ReferenceSubEvent.getTimeZone, null, referenceCalendarEventWithSubEvent.getNowInfo, referenceCalendarEventWithSubEvent.TimeLineHistory);
            }
            else
            {
                ScheduleUpdated = new CalendarEvent(
                    referenceCalendarEventWithSubEvent.getName, StartData, EndData, ReferenceSubEvent.getActiveDuration, referenceCalendarEventWithSubEvent.getPreparation, referenceCalendarEventWithSubEvent.getPreDeadline, 1, new Repetition(), ReferenceSubEvent.Location, new EventDisplay(), new MiscData(), null, new NowProfile(), false, false, ReferenceSubEvent.getCreator, ReferenceSubEvent.getAllUsers(), ReferenceSubEvent.getTimeZone, null, referenceCalendarEventWithSubEvent.TimeLineHistory);
            }


            ScheduleUpdated.DisableSubEvents(ScheduleUpdated.AllSubEvents);


            HashSet<SubCalendarEvent> NotDOneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            removeFromAllEventDictionary(ScheduleUpdated.getId);//removes the false calendar event
        }

        /// <summary>
        /// Function simply deletes a subevent from a schedule. Note this doesnt attempt to readjust the schedule. If you need to reoptimize the schedule then use deleteSubCalendarEventAndReadjust
        /// </summary>
        /// <param name="EventID"></param>
        async public Task deleteSubCalendarEvent(string EventID)
        {
            CalendarEvent referenceCalendarEventWithSubEvent = getCalendarEvent(EventID);
            SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(EventID);
            ReferenceSubEvent.disable(referenceCalendarEventWithSubEvent);
        }

        /// <summary>
        /// This Deletes several subevents from the user schedule without trying to adjust the schedule
        /// </summary>
        /// <param name="EventID"></param>
        async public Task deleteSubCalendarEvents(IEnumerable<string> EventIDs)
        {
            foreach (string eachString in EventIDs)
            {
                CalendarEvent referenceCalendarEventWithSubEvent = getCalendarEvent(eachString);
                if(referenceCalendarEventWithSubEvent!=null)
                {
                    SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(eachString);
                    if (ReferenceSubEvent != null)
                    {
                        ReferenceSubEvent.disable(referenceCalendarEventWithSubEvent);
                    }
                }
                
            }
        }

        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ProcrastinateAll(TimeSpan DelaySpan, string NameOfEvent = "BLOCKED OUT", string timeZone = "UTC")
        {

            if (DelaySpan.Ticks > 0)
            {
                EventDisplay ProcrastinateDisplay = new EventDisplay(true, new TilerColor(), 2);
                Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
                AllEventDictionary_Cpy = getDeepCopyOfEventDictionary();

                EventName blockName = new EventName(null, null, NameOfEvent);
                TilerUser user = this.User;

                ProcrastinateCalendarEvent retrievedProcrastinateAll = getProcrastinateAllEvent();
                ProcrastinateCalendarEvent procrastinateAll = ProcrastinateCalendarEvent.generateProcrastinateAll(Now.constNow, user, DelaySpan, timeZone, retrievedProcrastinateAll, NameOfEvent);

                blockName.Creator_EventDB = procrastinateAll.getCreator;
                blockName.AssociatedEvent = procrastinateAll;
                HashSet<SubCalendarEvent> NotdoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
                clearAllRepetitionLock();
                procrastinateAll = EvaluateTotalTimeLineAndAssignValidTimeSpots(procrastinateAll, NotdoneYet, null, null, 1) as ProcrastinateCalendarEvent;

                Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue = new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(procrastinateAll.Error, AllEventDictionary_Cpy);
                return retValue;
            }
            throw new CustomErrors(CustomErrors.Errors.procrastinationBeforeNow, "Cannot go back in time quite yet");
        }


        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> SetCalendarEventAsNow(string CalendarID, bool Force = false)
        {
            //CalendarEvent calendarEvent = getCalendarEvent(CalendarID);
            IEnumerable<SubCalendarEvent> orderedSubEvents = getAllRelatedCalendarEvents(CalendarID).SelectMany(o => o.ActiveSubEvents).Where(obj => obj.End > Now.constNow).OrderBy(obj => obj.End);
            
            
            if (orderedSubEvents.Count() < 1)
            {
                orderedSubEvents = getAllRelatedCalendarEvents(CalendarID).SelectMany(o => o.ActiveSubEvents).OrderBy(obj => obj.End).Reverse();//I didn't do OrderByDescending because an interest situation where two events I ordered have the same start then they are aordered by the id. Which means the lesser Id gets picked
            }
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue = new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(new CustomErrors("No Active Event Found", 100), null);
            if (orderedSubEvents.Count() > 0)
            {
                Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = getDeepCopyOfEventDictionary();
                SubCalendarEvent mySubCalendarEvent = orderedSubEvents.First();
                retValue = new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(SetSubeventAsNow(mySubCalendarEvent.getId, true, false).Item1, AllEventDictionary_Cpy);
            }
            return retValue;
        }

        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> SetSubeventAsNow(string EventID, bool Force = false, bool lockToId = true)
        {
            CalendarEvent referenceCalendarEvent = getCalendarEvent(EventID);
            SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(EventID);
            ReferenceSubEvent.disableRepetitionLock();
            referenceCalendarEvent.DayPreference.init();
            TimeSpan bumperTimeSpan = Utility.ZeroTimeSpan;

            /// Adds a buffer span to ensure one can travel to location after hitting "Do now"
            if(CurrentLocation != null && CurrentLocation.isNotNullAndNotDefault)
            {
                if(ReferenceSubEvent.isLocationAmbiguous)
                {
                    ReferenceSubEvent.validateLocation(CurrentLocation);
                }
                bumperTimeSpan = getTravelSpan(CurrentLocation, ReferenceSubEvent.Location);
            }
            NowProfile nowProfile = ReferenceSubEvent.getNowInfo ?? ReferenceSubEvent.initializeNowProfile();

            if (Now.getDayIndexFromStartOfTime( nowProfile.PreferredTime) != Now.getDayIndexFromStartOfTime(Now.constNow) || !nowProfile.isInitialized)
            {
                var dayPreference = referenceCalendarEvent.DayPreference[Now.ConstDayOfWeek];
                ++dayPreference.Count;
                TimeOfDayPreferrence.DaySection daySection = Now.getDaySection(Now.constNow);
                dayPreference[daySection] += 1;
                NowProfile myNow = new NowProfile(Now.constNow, true);
                referenceCalendarEvent.UpdateNowProfile(myNow);
            }

            EventID SubEventID = new EventID(EventID);
            bool InitialRigid = ReferenceSubEvent.isRigid;

            TimeSpan subeventStartTimeShift = Now.calculationNow - ReferenceSubEvent.Start + bumperTimeSpan;

            if (!ReferenceSubEvent.shiftEvent(subeventStartTimeShift, Force, lockToId: lockToId) && !Force)
            {
                return new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(new CustomErrors("You will be going outside the limits of this event, Is that Ok?", 5), null);
            }

            Now.UpdateNow(ReferenceSubEvent.Start);

            if (ReferenceSubEvent.End > referenceCalendarEvent.End || ReferenceSubEvent.Start < referenceCalendarEvent.Start)
            {
                DateTimeOffset newStart = referenceCalendarEvent.Start;
                DateTimeOffset newEnd = referenceCalendarEvent.End;
                newStart = ReferenceSubEvent.Start < referenceCalendarEvent.Start ? ReferenceSubEvent.Start : referenceCalendarEvent.Start;
                newEnd = ReferenceSubEvent.End > referenceCalendarEvent.End ? ReferenceSubEvent.End : referenceCalendarEvent.End;
                
                TimeLine newTImeLine = new TimeLine(newStart, newEnd);
                referenceCalendarEvent.updateTimeLine(newTImeLine);
            }

            if (!InitialRigid)
            {
                referenceCalendarEvent.RigidizeSubEvent(ReferenceSubEvent.Id);
            }

            if (referenceCalendarEvent.IsFromRecurringAndNotChildRepeatCalEvent)
            {
                referenceCalendarEvent = referenceCalendarEvent.getRepeatedCalendarEvent(SubEventID.getIDUpToRepeatCalendarEvent());
            }

            Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = getDeepCopyOfEventDictionary();

            CalendarEvent ScheduleUpdated = CalendarEvent.getEmptyCalendarEvent(new EventID());
            HashSet<SubCalendarEvent> NotDOneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();


            ScheduleUpdated = EvaluateTotalTimeLineAndAssignValidTimeSpots(ScheduleUpdated, NotDOneYet, null, null, 1);
            if (!InitialRigid)
            {
                referenceCalendarEvent.UnRigidizeSubEvent(ReferenceSubEvent.Id);
            }


            if (ScheduleUpdated.Error != null)
            {
                LogStatus(ScheduleUpdated, "Set as now");
            }

            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue = new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(ScheduleUpdated.Error, AllEventDictionary_Cpy);
            return retValue;
        }

        public Tuple<List<SubCalendarEvent>[], DayTimeLine[], List<SubCalendarEvent>> peekIntoSchedule(CalendarEvent NewEvent)
        {
            HashSet<SubCalendarEvent> NotdoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            Tuple<List<SubCalendarEvent>[], DayTimeLine[], List<SubCalendarEvent>> retValue;
            if (!NewEvent.isLocked)
            {
                retValue = peekEvaluateTotalTimeLineAndAssignValidTimeSpots(NewEvent, NotdoneYet);
            }
            else
            {
                retValue = peekEvaluateTotalTimeLineAndAssignValidTimeSpots(NewEvent, NotdoneYet, null, 1);
            }


            return retValue;
        }

        public CustomErrors AddToSchedule(CalendarEvent NewEvent, bool optimizeSchedule = true)
        {
            Now.InitializeParameters();
            foreach (SubCalendarEvent subEvent in NewEvent.ActiveSubEvents)
            {
                subEvent.PinToEnd(CompleteSchedule);
            }

            NewEvent.TimeOfScheduleLoad = Now.constNow;// ensures that any latter dependencies on time loaded(e.g locaiton validation) use the verified location
            HashSet<SubCalendarEvent> NotdoneYet = new HashSet<SubCalendarEvent>();
            NewEvent = EvaluateTotalTimeLineAndAssignValidTimeSpots(NewEvent, NotdoneYet, null);
            

            if (NewEvent == null)//checks if event was assigned and ID ehich means it was successfully able to find a spot
            {
                return NewEvent.Error;
            }

            if (NewEvent.getId == "" || NewEvent == null)//checks if event was assigned and ID ehich means it was successfully able to find a spot
            {
                return NewEvent.Error;
            }


            if (NewEvent.Error != null)
            {
                LogStatus(NewEvent, "Adding New Event");
            }

            addCaleventToAllEventDictionary(NewEvent.Calendar_EventID.getCalendarEventComponent(), NewEvent);
            return NewEvent.Error;
        }


        /// <summary>
        /// FUnction atttempts to get the best next event for the current user on major factors affecting schedule. e.g Based on location wweather, time of day and oth
        /// </summary>
        /// <param name="currentLocation"></param>
        /// <returns></returns>
        async public Task<CustomErrors> FindMeSomethingToDo(Location currentLocation, string timeZone = "UTC")
        {
#if enableTimer
            myWatch.Start();
#endif
            EventID id = EventID.GenerateCalendarEvent();
            TimeSpan duration = TimeSpan.FromMinutes(1);
            EventName name = new EventName(null, null, "NothingToDo");
            CalendarEvent NewEvent = new RigidCalendarEvent(
                name, Now.constNow, Now.constNow.Add(duration), duration, new TimeSpan(), new TimeSpan(), new Repetition(), currentLocation, null, null, false, false, TilerUser, new TilerUserGroup(), timeZone, null, new NowProfile(), null);
            name.Creator_EventDB = NewEvent.getCreator;
            name.AssociatedEvent = NewEvent;
            clearAllRepetitionLock();
            NewEvent = EvaluateTotalTimeLineAndAssignValidTimeSpots(NewEvent, new HashSet<SubCalendarEvent>(), currentLocation, null, 1, true, false);

            CustomErrors RetValue = NewEvent.Error;
            removeFromAllEventDictionary(NewEvent.Calendar_EventID.getCalendarEventComponent());
            return RetValue;
        }

        void triggerTimer()
        {
#if enableTimer
            myWatch.Stop();
            long usedup = myWatch.ElapsedMilliseconds;
            Environment.Exit(1);
#endif
        }
        Dictionary<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> createCopyOfPossibleEvents(Dictionary<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> PossibleEntries)
        {
            Dictionary<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> retValue = new Dictionary<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>>();
            foreach (KeyValuePair<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> eachKeyValuePair in PossibleEntries)
            {
                Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> NewDict0 = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();
                foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair0 in eachKeyValuePair.Value)
                {
                    Dictionary<string, mTuple<bool, SubCalendarEvent>> NewDict1 = new Dictionary<string, mTuple<bool, SubCalendarEvent>>();

                    foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> eachKeyValuePair1 in eachKeyValuePair0.Value)
                    {
                        NewDict1.Add(eachKeyValuePair1.Key, new mTuple<bool, SubCalendarEvent>(eachKeyValuePair1.Value.Item1, eachKeyValuePair1.Value.Item2));
                    }
                    NewDict0.Add(eachKeyValuePair0.Key, NewDict1);
                }

                retValue.Add(eachKeyValuePair.Key, NewDict0);
            }
            return retValue;
        }

        List<mTuple<double, SubCalendarEvent>> PopulateCompatibleList(List<mTuple<double, SubCalendarEvent>> AllSubCalEvents, List<SubCalendarEvent> PossibleSubcalEvents, TimeLine PertinentTimeLine, TimeSpan TotalFreeSpaceInTImeLine)
        {
            /*
             * this function generates a list of List<mTuple<double, SubCalendarEvent>> that can work within the specified timeLine.
             * The PossibleSubcalevents parameter is a list of possible Calendar events that were calculated to be permissible within this timeLine. Note this includes the restricted valeues
             * The AllSubcalevents is a list of mTuples. Each mtuple has a subcalendar event and its average distance cost to the other nodes within its timeLine
             */

            IEnumerable<SubCalendarEvent> AllSubCalEvents_Unverified = AllSubCalEvents.Select(obj => obj.Item2);//generates an IEnumerableOf Subcalevents from the AllSubCalEvents which is an mTUple
            IEnumerable<SubCalendarEvent> UsableSubCalevents = AllSubCalEvents_Unverified.Where(obj => PossibleSubcalEvents.Contains(obj));//checks if the subcalevents are possible for the Timeline
            IEnumerable<mTuple<double, SubCalendarEvent>> UsableWithinTIimeLineAndFits = AllSubCalEvents.Where(obj1 => ((UsableSubCalevents.Contains(obj1.Item2)) && ((obj1.Item2.getActiveDuration <= TotalFreeSpaceInTImeLine))));//checks if the active duration will possibly fit the timespan requried to reach average
            List<mTuple<double, SubCalendarEvent>> retValue = UsableWithinTIimeLineAndFits.ToList();
            return retValue;
        }

        bool ShiftEvent(CalendarEvent CurrentEvent, TimeSpan DelayTime)
        {
            return true;
        }

        public CalendarEvent GenerateRigidSubEvents(CalendarEvent MyCalendarEvent)
        {
            List<SubCalendarEvent> MyArrayOfSubEvents = new List<SubCalendarEvent>();

            if (MyCalendarEvent.IsFromRecurringAndNotChildRepeatCalEvent)
            {

                SubCalendarEvent MySubEvent = new SubCalendarEvent(MyCalendarEvent, MyCalendarEvent.getCreator, MyCalendarEvent.getAllUsers(), MyCalendarEvent.getTimeZone, MyCalendarEvent.getActiveDuration, MyCalendarEvent.getName, MyCalendarEvent.Repeat.Range.Start, MyCalendarEvent.Repeat.Range.End, MyCalendarEvent.getPreparation, MyCalendarEvent.getId, MyCalendarEvent.isRigid, MyCalendarEvent.isEnabled, MyCalendarEvent.getUIParam, MyCalendarEvent.Notes, MyCalendarEvent.getIsComplete, MyCalendarEvent.Location, MyCalendarEvent.StartToEnd);


                //new SubCalendarEvent(MyCalendarEvent.End, MyCalendarEvent.Repeat.Range.Start, MyCalendarEvent.Repeat.Range.End, MyCalendarEvent.Preparation, MyCalendarEvent.ID);

                for (; MySubEvent.Start < MyCalendarEvent.Repeat.Range.End;)
                {
                    MyArrayOfSubEvents.Add(MySubEvent);
                    switch (MyCalendarEvent.Repeat.getFrequency)
                    {
                        case Repetition.Frequency.DAILY:
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent, MyCalendarEvent.getCreator, MyCalendarEvent.getAllUsers(), MyCalendarEvent.getTimeZone, MyCalendarEvent.getActiveDuration, MyCalendarEvent.getName, MyCalendarEvent.Repeat.Range.Start.AddDays(1), MyCalendarEvent.Repeat.Range.End.AddDays(1), MyCalendarEvent.getPreparation, MyCalendarEvent.getId, MyCalendarEvent.isRigid, MyCalendarEvent.isEnabled, MyCalendarEvent.getUIParam, MyCalendarEvent.Notes, MyCalendarEvent.getIsComplete, MyCalendarEvent.Location, MyCalendarEvent.StartToEnd);
                                break;
                            }
                        case Repetition.Frequency.WEEKLY:
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent, MyCalendarEvent.getCreator, MyCalendarEvent.getAllUsers(), MyCalendarEvent.getTimeZone, MyCalendarEvent.getActiveDuration, MyCalendarEvent.getName, MyCalendarEvent.Repeat.Range.Start.AddDays(7), MyCalendarEvent.Repeat.Range.End.AddDays(7), MyCalendarEvent.getPreparation, MyCalendarEvent.getId, MyCalendarEvent.isRigid, MyCalendarEvent.isEnabled, MyCalendarEvent.getUIParam, MyCalendarEvent.Notes, MyCalendarEvent.getIsComplete, MyCalendarEvent.Location, MyCalendarEvent.StartToEnd);
                                break;
                            }
                        case Repetition.Frequency.BIWEEKLY:
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent, MyCalendarEvent.getCreator, MyCalendarEvent.getAllUsers(), MyCalendarEvent.getTimeZone, MyCalendarEvent.getActiveDuration, MyCalendarEvent.getName, MyCalendarEvent.Repeat.Range.Start.AddDays(14), MyCalendarEvent.Repeat.Range.End.AddDays(14), MyCalendarEvent.getPreparation, MyCalendarEvent.getId, MyCalendarEvent.isRigid, MyCalendarEvent.isEnabled, MyCalendarEvent.getUIParam, MyCalendarEvent.Notes, MyCalendarEvent.getIsComplete, MyCalendarEvent.Location, MyCalendarEvent.StartToEnd);
                                break;
                            }
                        case Repetition.Frequency.MONTHLY:
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent, MyCalendarEvent.getCreator, MyCalendarEvent.getAllUsers(), MyCalendarEvent.getTimeZone, MyCalendarEvent.getActiveDuration, MyCalendarEvent.getName, MyCalendarEvent.Repeat.Range.Start.AddMonths(1), MyCalendarEvent.Repeat.Range.End.AddMonths(1), MyCalendarEvent.getPreparation, MyCalendarEvent.getId, MyCalendarEvent.isRigid, MyCalendarEvent.isEnabled, MyCalendarEvent.getUIParam, MyCalendarEvent.Notes, MyCalendarEvent.getIsComplete, MyCalendarEvent.Location, MyCalendarEvent.StartToEnd);
                                break;
                            }
                        case Repetition.Frequency.YEARLY:
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent, MyCalendarEvent.getCreator, MyCalendarEvent.getAllUsers(), MyCalendarEvent.getTimeZone, MyCalendarEvent.getActiveDuration, MyCalendarEvent.getName, MyCalendarEvent.Repeat.Range.Start.AddYears(1), MyCalendarEvent.Repeat.Range.End.AddYears(1), MyCalendarEvent.getPreparation, MyCalendarEvent.getId, MyCalendarEvent.isRigid, MyCalendarEvent.isEnabled, MyCalendarEvent.getUIParam, MyCalendarEvent.Notes, MyCalendarEvent.getIsComplete, MyCalendarEvent.Location, MyCalendarEvent.StartToEnd);
                                break;
                            }
                    }


                }
            }

            return MyCalendarEvent;
            //
        }

        public List<TimeLine> getOnlyPertinentTimeFrame(TimeLine[] ArraytOfFreeSpots, TimeLine myTimeLine)
        {
            /*
             * Name: Jerome Biotidara
             * Description: Function only takes a TImeLine and Array Of TimeLine FreeSpots. It returns a List Of TimeLine In whcih each elements  exist within the range of TimeLine
             */
            List<TimeLine> PertinentTimeLine = new List<TimeLine>();
            List<TimeLine> OutLiers = new List<TimeLine>();

            foreach (TimeLine MyFreeTimeLine in ArraytOfFreeSpots)
            {
                if (myTimeLine.IsTimeLineWithin(MyFreeTimeLine))
                {
                    PertinentTimeLine.Add(MyFreeTimeLine);
                }
                else
                {
                    OutLiers.Add(MyFreeTimeLine);
                }
            }

            foreach (TimeLine Outlier in OutLiers)//this can be embedded in the preceeding foreach loop above in the else branch
            {
                if (myTimeLine.IsDateTimeWithin(Outlier.Start))
                {
                    PertinentTimeLine.Add(new TimeLine(Outlier.Start, myTimeLine.End));
                }
                else
                {
                    if (myTimeLine.IsDateTimeWithin(Outlier.End))
                    {
                        PertinentTimeLine.Add(new TimeLine(myTimeLine.Start, Outlier.End));
                    }
                    else
                    {
                        if (Outlier.IsTimeLineWithin(myTimeLine))
                        {
                            PertinentTimeLine.Add(Outlier);
                        }
                    }
                }
            }

            return PertinentTimeLine;



            //return new List<TimeLine>();
        }

        public Tuple<List<SubCalendarEvent>[], DayTimeLine[], List<SubCalendarEvent>> peekEvaluateTotalTimeLineAndAssignValidTimeSpots(CalendarEvent MyEvent, HashSet<SubCalendarEvent> UnDoneEvents, List<CalendarEvent> NoneCOmmitedCalendarEvent = null, int InterringWithNowEvent = 0)
        {
            Tuple<List<SubCalendarEvent>[], DayTimeLine[], List<SubCalendarEvent>> RetValue;
            if (NoneCOmmitedCalendarEvent == null)
            {
                NoneCOmmitedCalendarEvent = new List<CalendarEvent>();
            }
            if (MyEvent.IsFromRecurringAndNotChildRepeatCalEvent)
            {
                CalendarEvent tempCalendarEvent = CalendarEvent.getEmptyCalendarEvent(MyEvent.Calendar_EventID, MyEvent.CalculationStart, MyEvent.isLocked ? MyEvent.CalculationStart : MyEvent.End);//creates an "empty" calendar event. If the calEvent is rigid it has time span of zero

                RetValue = peekEvaluateTotalTimeLineAndAssignValidTimeSpots(tempCalendarEvent, UnDoneEvents, MyEvent.Repeat.RecurringCalendarEvents().ToList());
                return RetValue;
            }

            RetValue = peekIntoDays(MyEvent, NoneCOmmitedCalendarEvent.ToList(), InterringWithNowEvent, UnDoneEvents, null);
            return RetValue;

        }

        /// <summary>
        /// function evaluates the schedule. 
        /// MyEvent is the new event to be added to the schedule
        /// UnDoneEvents are the events that are yet to be marked as complete for the current day
        /// NoneCOmmitedCalendarEvent is the list of calendar events that are under a repeating calendarevent that are yet to be persisted to storage
        /// optimizeFirstTwentyFourHours should the scheduler try to optimize the first twenty four hours for shortest path recognition
        /// preserveFirstTwentyFourHours should the scheduler try to ensure that the events for the next twenty four hours stay within a certain order
        /// InterringWithNowEvent deals with how the interferrence with the now should be resolved.
        /// InterringWithNowEvent == 0. Ignore events interfering with now
        /// InterringWithNowEvent == 1. Including all events in calculation window of calculation usually, 90 days befor and after now
        /// InterringWithNowEvent == 2. Ignore only the current added calendar event
        /// </summary>
        /// <param name="MyEvent"></param>
        /// <param name="UnDoneEvents"></param>
        /// <param name="NoneCOmmitedCalendarEvent"></param>
        /// <param name="InterringWithNowEvent"></param>
        /// <param name="optimizeFirstTwentyFourHours"></param>
        /// <returns></returns>
        public CalendarEvent EvaluateTotalTimeLineAndAssignValidTimeSpots(CalendarEvent MyEvent, HashSet<SubCalendarEvent> UnDoneEvents, Location callLocation, List<CalendarEvent> NoneCOmmitedCalendarEvent = null, int InterringWithNowEvent = 0, bool optimizeFirstTwentyFourHours = true, bool preserveFirstTwentyFourHours = true, bool shuffle = false)
        {
            if (NoneCOmmitedCalendarEvent == null)
            {
                NoneCOmmitedCalendarEvent = new List<CalendarEvent>();
            }
            if (MyEvent.IsFromRecurringAndNotChildRepeatCalEvent)
            {
                CalendarEvent tempCalendarEvent = CalendarEvent.getEmptyCalendarEvent(MyEvent.Calendar_EventID, MyEvent.CalculationStart, MyEvent.isLocked ? MyEvent.CalculationStart : MyEvent.End);//creates an "empty" calendar event. If the calEvent is rigid it has time span of zero

                EvaluateTotalTimeLineAndAssignValidTimeSpots(tempCalendarEvent, UnDoneEvents, callLocation, MyEvent.Repeat.RecurringCalendarEvents().ToList(), InterringWithNowEvent, optimizeFirstTwentyFourHours, preserveFirstTwentyFourHours, shuffle);
                return MyEvent;
            }
            Stopwatch watch = new Stopwatch();
            watch.Start();
            CalendarEvent MyCalendarEventUpdated = ReArrangeTimeLineWithinWithinCalendaEventRangeUpdated(MyEvent, NoneCOmmitedCalendarEvent.ToList(), InterringWithNowEvent, UnDoneEvents, callLocation, optimizeFirstTwentyFourHours, preserveFirstTwentyFourHours, shuffle);
            watch.Stop();
            TimeSpan scheduleCal = watch.Elapsed;
            Debug.WriteLine("Schedule calculation took " + scheduleCal.ToString());
            Debug.WriteLine("-----------------------------------------------------");
            return MyCalendarEventUpdated;
        }


        Dictionary<TimeLine, List<SubCalendarEvent>> JustPickCollidingTimeline(List<TimeLine> AllTImelines, List<SubCalendarEvent> AllSubCalendarEvent)
        {
            AllSubCalendarEvent = AllSubCalendarEvent.ToList();
            Dictionary<TimeLine, List<SubCalendarEvent>> retValue = AllTImelines.ToDictionary(obj => obj, obj => new List<SubCalendarEvent>());

            for (int i = 0; i < AllSubCalendarEvent.Count; i++)
            {
                for (int j = 0; j < AllTImelines.Count; j++)
                {
                    if (AllSubCalendarEvent[i].StartToEnd.InterferringTimeLine(AllTImelines[j]) != null)
                    {
                        retValue[AllTImelines[j]].Add(AllSubCalendarEvent[i]);
                        break;
                    }
                }
            }


            retValue = retValue.ToDictionary(obj => obj.Key, obj => {
                List<SubCalendarEvent> ListOfEvents = obj.Value.OrderBy(obj1 => obj1.Start).ToList();
                if(!Utility.tryPinSubEventsToStart(ListOfEvents, obj.Key))
                {
                    throw new Exception("sub events should fit in secion");
                }
                return ListOfEvents;
            });
            return retValue;
        }


        /// <summary>
        /// Given a collection of timeLines, this function returns a Dictionary of Timeline and list sub events. These subevents can only exist within this timeline. If a subevent can exist in 2 ot more time line from "timeLines" it will not be added to the return dictionary
        /// </summary>
        /// <param name="timeLines"></param>
        /// <param name="subEvents"></param>
        /// <returns></returns>
        Dictionary<TimeLine, List<SubCalendarEvent>> generateConstrainedList(List<TimeLine> timeLines, List<SubCalendarEvent> subEvents)
        {
            Dictionary<TimeLine, List<SubCalendarEvent>> retValue = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<SubCalendarEvent>>();

            foreach (TimeLine eachTimeLine in timeLines)
            {
                retValue.Add(eachTimeLine, new List<SubCalendarEvent>());
            }

            foreach (SubCalendarEvent eachSubEvent in subEvents)
            {
                List<TimeLine> CompatibleTimeLines = timeLines.Where(timeLine => eachSubEvent.canExistWithinTimeLine(timeLine)).ToList();
                if (CompatibleTimeLines.Count == 1)
                {
                    retValue[CompatibleTimeLines[0]].Add(eachSubEvent);
                }
            }


            return retValue;
        }

        CalendarEvent CheckUncommitedForSubCalevent(List<CalendarEvent> UncommitedCalendarEvents, SubCalendarEvent possibleSubCalendarevent)
        {
            List<CalendarEvent> PertinentCalendarEvent = UncommitedCalendarEvents.Where(obj => obj.ActiveSubEvents.Contains(possibleSubCalendarevent)).ToList();
            if (PertinentCalendarEvent.Count > 0)
            {
                return PertinentCalendarEvent[0];
            }
            else { return null; }
        }

        Dictionary<CalendarEvent, List<SubCalendarEvent>> generateDictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents(List<SubCalendarEvent> ListOfInterferringElements, List<CalendarEvent> UncommitedCalendarEvents)
        {
            /*
             Name:Function takes the list of interferring arrays and used to build a Calendar To "List of SubCalendarEvent" dictionary. 
             */

            int i = 0;
            Dictionary<CalendarEvent, List<SubCalendarEvent>> DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents = new Dictionary<CalendarEvent, List<SubCalendarEvent>>();
            int j = 0;
            for (; i < ListOfInterferringElements.Count; i++)
            {
                EventID MyEventID = new EventID(ListOfInterferringElements[i].getId);
                string ParentID = MyEventID.getCalendarEventComponent();//This gets the parentID of the SubCalendarEventID
                CalendarEvent UncomittedCalendar = CheckUncommitedForSubCalevent(UncommitedCalendarEvents, ListOfInterferringElements[i]);
                if (UncomittedCalendar != null)
                {

                    if (DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.ContainsKey(UncomittedCalendar))
                    {
                        DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[UncomittedCalendar].Add(ListOfInterferringElements[i]);
                        j++;
                    }
                    else
                    {
                        DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Add(UncomittedCalendar, new List<SubCalendarEvent>());
                        DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[UncomittedCalendar].Add(ListOfInterferringElements[i]);
                        j++;
                    }
                }

                else
                {
                    CalendarEvent calEvent = getCalendarEvent(MyEventID);
                    if (calEvent.IsFromRecurringAndNotChildRepeatCalEvent)
                    {
                        CalendarEvent repeatCalEvent = calEvent.getRepeatedCalendarEvent(MyEventID.getIDUpToRepeatCalendarEvent());

                        if (DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.ContainsKey(repeatCalEvent))
                        {
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[repeatCalEvent].Add(ListOfInterferringElements[i]);
                            j++;
                        }
                        else
                        {
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Add(repeatCalEvent, new List<SubCalendarEvent>());
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[repeatCalEvent].Add(ListOfInterferringElements[i]);
                            j++;
                        }


                    }
                    else
                    {
                        CalendarEvent nonRepeatCalEvent = calEvent;
                        if (DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.ContainsKey(nonRepeatCalEvent))
                        {
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[nonRepeatCalEvent].Add(ListOfInterferringElements[i]);
                            j++;
                        }
                        else
                        {
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Add(nonRepeatCalEvent, new List<SubCalendarEvent>());
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[nonRepeatCalEvent].Add(ListOfInterferringElements[i]);
                            j++;
                        }

                    }
                }
                /*catch (Exception e)
                {
                    if (AllEventDictionary[ParentID].RepetitionStatus)
                    { 
                        
                    }
                    else
                    {
                        DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[AllEventDictionary[ParentID]].Add(ListOfInterferringElements[i]);
                    }
                }*/


            }

            return DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents;
        }


        Tuple<IEnumerable<SubCalendarEvent>, DateTimeOffset, int, IEnumerable<SubCalendarEvent>> getStartTimeWhenCurrentTimeClashesWithSubcalevent(IEnumerable<SubCalendarEvent> CollectionOfSubCalEvent, DateTimeOffset ReferenceTime, int FlagType)
        {
            /*
             * function tries to derive the best start time and valid subcalendar events for the evaluation of schedule
             * It takes a collection of possible subcalendar events and an initializing reference time, the latter serves as the initializing position for the calculation.
             * It compares the referencetime with "Now". If Referencetime is earlier than Now it sets the reference time to now. Function checks if now clashes with any events. If it does, it checks if its past 10% of its duration. If yes, it drops it as an interferring element. It selects the end time of the Subcalendarevent (which is interferred by now) as the new reference time
             */
            ReferenceTime = ReferenceTime > Now.calculationNow ? ReferenceTime : Now.calculationNow;
            IEnumerable<SubCalendarEvent> retrievedData;
            Tuple<IEnumerable<SubCalendarEvent>, DateTimeOffset, int, IEnumerable<SubCalendarEvent>> retValue;
            int retFlagType = FlagType;
            IEnumerable<SubCalendarEvent> interfersWithNow;

            CollectionOfSubCalEvent = CollectionOfSubCalEvent.Where(obj => obj.End > ReferenceTime);//ensures we are selecting the sub events that are actuve after the reference time
            if (ReferenceTime == Now.calculationNow)
            {
                interfersWithNow = CollectionOfSubCalEvent.Where(obj => obj.IsDateTimeWithin(ReferenceTime));

                List<SubCalendarEvent> currentList = CollectionOfSubCalEvent.ToList();
                if (interfersWithNow.Count() > 0)
                {
                    switch (FlagType)
                    {
                        case 0://Do not include event interferring with now in calculations. Select the element with last deadline and set event endtime as now. THis ensures that any further calculations do not try to include now
                            {
                                interfersWithNow = interfersWithNow.OrderBy(obj => obj.End);
                                DateTimeOffset newNow = interfersWithNow.Last().End;
                                Now.UpdateNow(newNow);
                                ReferenceTime = newNow;
                                CollectionOfSubCalEvent = currentList.Where(obj => !interfersWithNow.Contains(obj));
                                retFlagType = 1;
                            }
                            break;
                        case 1:
                            {
                                interfersWithNow = interfersWithNow.OrderBy(obj => obj.End);
                                retFlagType = 1;
                            }
                            break;
                        default:
                            { }
                            break;
                    }
                }
                retValue = new Tuple<IEnumerable<SubCalendarEvent>, DateTimeOffset, int, IEnumerable<SubCalendarEvent>>(CollectionOfSubCalEvent, ReferenceTime, retFlagType, interfersWithNow);
            }
            else
            {
                retrievedData = CollectionOfSubCalEvent.Where(obj => obj.StartToEnd.IsDateTimeWithin(ReferenceTime));//selects subcal event in which the Reference time intersect with the timeline. Just in case the reference timeline currently intersects it can reel in that new sub calendar event
                retrievedData = retrievedData.OrderBy(obj => obj.Start);
                List<SubCalendarEvent> allData = retrievedData.ToList();
                DateTimeOffset retValueDateTime = ReferenceTime;
                IEnumerable<SubCalendarEvent> retValueIenumerable = CollectionOfSubCalEvent;
                if (allData.Count > 0)
                {
                    retValueDateTime = allData[0].Start;
                    return getStartTimeWhenCurrentTimeClashesWithSubcalevent(retValueIenumerable, retValueDateTime, FlagType);
                }
                retValue = new Tuple<IEnumerable<SubCalendarEvent>, DateTimeOffset, int, IEnumerable<SubCalendarEvent>>(retValueIenumerable, retValueDateTime, FlagType, new List<SubCalendarEvent>());
            }

            return retValue;
        }


        /// <summary>
        /// Function gets the SubEvents that will be involved in the calculation in the timeLine. initializingCalendarEvent calendar event to be added to the new schedule. NoneCommitedCalendarEventsEvents is calendarevent that have not been added to the AllEventDictionary(Used for repeting events). Flag type checks if the algorithm should preserve events interferring with Now time. Flag type of  0 means check if it interfers with now. 1 means ignore events. ExemptFromCalculation Evets that have not being marked as done or completed. It returns a 3 item tuple(triple). Item1 is the list of interferring elements. Item 2 The custom error is set when there is less space than the calculation will allow. Item 3 is the list of element that collide with the current Now time
        /// The ExemptFromCalculation are exempted and removed from the list
        /// FlagType deals with how the interferrence with the now should be resolved.
        /// FlagType == 0. Ignore events interfeering with now
        /// FlagType == 1. Ignore all events interferring with now
        /// FlagType == 2. Ignore only the current added calendar event
        /// </summary>
        /// <param name="initializingCalendarEvent"></param>
        /// <param name="NoneCommitedCalendarEventsEvents"></param>
        /// <param name="FlagType"></param>
        /// <param name="ExemptFromCalculation"></param>
        /// <param name="iniTimeLine"></param>
        /// <returns>
        /// A Quadruple
        /// Item1 = The time line ideal for the calculation based on iniTimeLine
        /// Item2 = List of subevents that can be used in the evaluation, they shouldn't include events interferring with now
        /// Item3 = A CustomError object in the scenario where the sum of the duration of all the sub events is more than the CalculationTimeline. Note CalculationTimeline!=iniTimeLine it's  Now.CalcutionNow - iniTimeLine.End
        /// Item4 = Subevents interferring with the current time
        /// </returns>
        Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors, IEnumerable<SubCalendarEvent>> getAllInterferringEventsAndTimeLineInCurrentEvaluation(CalendarEvent initializingCalendarEvent, List<CalendarEvent> NoneCommitedCalendarEventsEvents, int FlagType, HashSet<SubCalendarEvent> ExemptFromCalculation, TimeLine iniTimeLine)
        {
            TimeLine RangeOfCalculation = iniTimeLine != null ? iniTimeLine : initializingCalendarEvent.StartToEnd;
            HashSet<CalendarEvent> NonCommitedCalEvents = new HashSet<CalendarEvent>(NoneCommitedCalendarEventsEvents);
            NonCommitedCalEvents.Add(initializingCalendarEvent);
            List<SubCalendarEvent> interferringWithNow = new List<SubCalendarEvent>();


            TimeLine CalculationTImeLine = new TimeLine(Now.calculationNow, iniTimeLine.End);
            List<SubCalendarEvent> SubEventsForCalculation = getAllElementsForCalculation(NonCommitedCalEvents.ToList(), RangeOfCalculation, CalculationTImeLine);
            CustomErrors retCustomErrors = null;

            if (SubCalendarEvent.TotalActiveDuration(SubEventsForCalculation) > CalculationTImeLine.TimelineSpan)
            {
                retCustomErrors = new CustomErrors("Total duration of events is greater than TImeLine length");
            }

            SubEventsForCalculation = SubEventsForCalculation.Except(ExemptFromCalculation).ToList();

            switch (FlagType)
            {
                case 0:
                    {
                        DateTimeOffset currentNowTime = Now.constNow;
                        mTuple<List<SubCalendarEvent>, DateTimeOffset> interFerringData = getElementsThatInterferWithNow(SubEventsForCalculation, Now.constNow);
                        interferringWithNow = interFerringData.Item1;
                        if (currentNowTime != interFerringData.Item2)
                        {
                            Now.UpdateNow(interFerringData.Item2, false);
                            SubEventsForCalculation = SubEventsForCalculation.Except(interferringWithNow).ToList();
                        }
                        CalculationTImeLine = new TimeLine(Now.calculationNow, CalculationTImeLine.End);

                    }
                    break;
                case 2:
                    {
                        DateTimeOffset currentNowTime = initializingCalendarEvent.ActiveSubEvents.Max(obj => obj.End);
                        Now.UpdateNow(currentNowTime, false);
                        SubEventsForCalculation = SubEventsForCalculation.Except(initializingCalendarEvent.ActiveSubEvents).ToList();
                        CalculationTImeLine = new TimeLine(Now.calculationNow, CalculationTImeLine.End);
                    }
                    break;
                case 1:
                default:
                    {
                        ;
                    }
                    break;
            }
            //throw new Exception("You need to lock events that cannot exist within calculation timeline");
            //SubEventsForCalculation = SubEventsForCalculation.Where(obj => obj.canExistWithinTimeLine(CalculationTImeLine)).ToList();
            Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors, IEnumerable<SubCalendarEvent>> retValue = new Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors, IEnumerable<SubCalendarEvent>>(
                CalculationTImeLine,
                SubEventsForCalculation,
                retCustomErrors,
                interferringWithNow);
            return retValue;

        }

        /// <summary>
        /// Gets all events that "interferr" with the Now calculation.
        /// </summary>
        /// <param name="AllSubevents"></param>
        /// <param name="nowTime"></param>
        /// <returns>
        /// an mtuple the first is a list of interffing elements
        /// THe latest end time of the subcalendar events interferring with now
        /// </returns>
        mTuple<List<SubCalendarEvent>, DateTimeOffset> getElementsThatInterferWithNow(IEnumerable<SubCalendarEvent> AllSubevents, DateTimeOffset nowTime)
        {
            DateTimeOffset latestTime = nowTime;
            List<SubCalendarEvent> InterFerringEvents = AllSubevents
                .Where(obj => obj.Start < nowTime)
                .Where(obj => obj.IsDateTimeWithin(nowTime))
                .Select(obj =>
                {
                    if (obj.End > latestTime)
                    {
                        latestTime = obj.End;
                    }
                    return obj;
                })
                .ToList();
            mTuple<List<SubCalendarEvent>, DateTimeOffset> retValue = new mTuple<List<SubCalendarEvent>, DateTimeOffset>(InterFerringEvents, latestTime);
            return retValue;
        }

        /// <summary>
        /// function gets all the subevents needed for calculation
        /// </summary>
        /// <param name="InitializingCalEvents"></param>
        /// <param name="IniTImeLine"></param>
        /// <returns></returns>
        List<SubCalendarEvent> getAllElementsForCalculation(List<CalendarEvent> InitializingCalEvents, TimeLine IniTImeLine, TimeLine CalculationTImeLine)
        {
            /*
             * The function tries to select all elemnents that interfer with the time frame. It assumes IniTImeLine is wide enough. It does not try to recalulate. If an element cannot exist within the timeline it simply removed from the calculation to ensure some correctness
             * */
            DateTimeOffset NowTIme = Now.constNow;
            HashSet<SubCalendarEvent> subEventsInSet = new HashSet<SubCalendarEvent>(getAllActiveCalendarEvents().Concat(InitializingCalEvents).Where(calEvent => calEvent.isActive)
                .SelectMany(calEvent => calEvent.ActiveSubEvents).
                Where(subEvent => subEvent.getCalculationRange.End > NowTIme).
                Where(subEvent => (subEvent.isRigid && subEvent.ActiveSlot.IsDateTimeWithin(NowTIme)) || subEvent.ActiveSlot.doesTimeLineInterfere(CalculationTImeLine) || subEvent.canExistWithinTimeLine(CalculationTImeLine) || subEvent.getIsProcrastinateCalendarEvent));
            TimeSpan maxSubeventSpan = Utility.LeastAllDaySubeventDuration;
            ConcurrentBag<SubCalendarEvent> subEvents = new ConcurrentBag<SubCalendarEvent>();
            foreach (SubCalendarEvent subEvent in subEventsInSet)
            {
                if (subEvent.getActiveDuration <= maxSubeventSpan)
                {
                    if (subEvent.isLocked)
                    {
                        if (subEvent.StartToEnd.doesTimeLineInterfere(CalculationTImeLine))
                        {
                            subEvents.Add(subEvent);
                        }
                    }
                    else
                    {
                        if (!subEvent.getIsEventRestricted)
                        {
                            subEvents.Add(subEvent);
                        }
                        else
                        {
                            if (subEvent.getCalculationRange.doesTimeLineInterfere(CalculationTImeLine))
                            {
                                subEvents.Add(subEvent);
                            }
                        }
                    }
                }
            }

            List<SubCalendarEvent> retValue = subEvents.ToList();
            return retValue;
        }

        Tuple<IEnumerable<SubCalendarEvent>, IEnumerable<SubCalendarEvent>> PintNotDoneYestSubEventToEndOfTimeLine(TimeLine encasingTimeLine, IEnumerable<SubCalendarEvent> NotDoneYetEvents)
        {
            List<SubCalendarEvent> retValueSuccesfull = new List<SubCalendarEvent>();
            List<SubCalendarEvent> retValueFailure = new List<SubCalendarEvent>();

            Tuple<IEnumerable<SubCalendarEvent>, IEnumerable<SubCalendarEvent>> retValue = new Tuple<IEnumerable<SubCalendarEvent>, IEnumerable<SubCalendarEvent>>(retValueSuccesfull, retValueFailure);

            foreach (SubCalendarEvent eachSubCalendarEvent in NotDoneYetEvents)
            {
                if (eachSubCalendarEvent.PinToEnd(encasingTimeLine))
                {
                    ((List<SubCalendarEvent>)retValue.Item1).Add(eachSubCalendarEvent);
                }
                else
                {
                    ((List<SubCalendarEvent>)retValue.Item2).Add(eachSubCalendarEvent);
                }
            }

            return retValue;
        }

        HashSet<SubCalendarEvent> getNoneDoneYetBetweenNowAndReerenceStartTIme()
        {/*
          * function gets the none done events within the current day frame.
          */


            TimeLine TimeLineBetweenNowAndReferenceStartTIme = new TimeLine(ReferenceDayTIime.AddDays(-90), Now.calculationNow);
            SubCalendarEvent[] NotDoneYet = getInterferringSubEvents(TimeLineBetweenNowAndReferenceStartTIme);
            IEnumerable<SubCalendarEvent> retValue = NotDoneYet.Where(obj => (!obj.isLocked) && (!obj.StartToEnd.IsDateTimeWithin(Now.calculationNow)));
            HashSet<SubCalendarEvent> retValue_HashSet = new HashSet<SubCalendarEvent>();
            foreach (SubCalendarEvent eachSubCalendarEvent in retValue)
            {
                retValue_HashSet.Add(eachSubCalendarEvent);
            }




            return retValue_HashSet;
        }

        Tuple<List<SubCalendarEvent>, List<BlobSubCalendarEvent>> PrepareElementsThatWillNotFit(IEnumerable<SubCalendarEvent> AllEvents, TimeLine ReferenceTimeline)
        {
            List<SubCalendarEvent> retValue = AllEvents.ToList();
            IEnumerable<SubCalendarEvent> AllRigidEvents = AllEvents.Where(obj => obj.isLocked);
            List<SubCalendarEvent> CannotFitInAnyFreespot = AllEvents.Except(AllRigidEvents).ToList();

            TimeLine refTImeLine = ReferenceTimeline.CreateCopy();

            refTImeLine.AddBusySlots(AllRigidEvents.Select(obj => obj.ActiveSlot));
            IEnumerable<TimeLine> AllFreeSpots = refTImeLine.getAllFreeSlots();
            foreach (TimeLine eachTimeLine in AllFreeSpots)// gets all events that cannot fully exsit in any free spot
            {
                CannotFitInAnyFreespot = CannotFitInAnyFreespot.Except(CannotFitInAnyFreespot.Where(obj => obj.canExistWithinTimeLine(eachTimeLine))).ToList();
                //CannotFitInAnyFreespot = CannotFitInAnyFreespot.Except(CannotFitInAnyFreespot.Where(obj => obj.canExistWithinTimeLine(eachTimeLine))).ToList();
            }

            IEnumerable<SubCalendarEvent> willNotFit = CannotFitInAnyFreespot.ToList();
#if INPROD

#else
            Dictionary<SubCalendarEvent, List<TimeLine>> NoFreeSpaceToConflictingSpaces = new Dictionary<SubCalendarEvent, List<TimeLine>>();

            foreach (SubCalendarEvent eachSubCalendarEvent in CannotFitInAnyFreespot)//builds dictionary NoFreeSpaceToConflictingSpaces by getting all the non free spots possible withing the attained freespots
            {
                List<TimeLine> PossibleSpaces = AllFreeSpots.Select(obj => obj.InterferringTimeLine(eachSubCalendarEvent.getCalculationRange)).Where(obj => obj != null).OrderByDescending(obj => obj.TimelineSpan).ToList();

                NoFreeSpaceToConflictingSpaces.Add(eachSubCalendarEvent, PossibleSpaces);
            }

            willNotFit = NoFreeSpaceToConflictingSpaces.Keys;
#endif

            List<BlobSubCalendarEvent> AllConflictingEvents = Utility.getConflictingEvents(AllRigidEvents.Concat(willNotFit)).Item1;
            retValue = retValue.Except(AllConflictingEvents.SelectMany(obj => obj.getSubCalendarEventsInBlob())).ToList();
            retValue = retValue.Concat(AllConflictingEvents).ToList();


            Tuple<List<SubCalendarEvent>, List<BlobSubCalendarEvent>> retValueTuple = new Tuple<List<SubCalendarEvent>, List<BlobSubCalendarEvent>>(retValue, AllConflictingEvents);
            return retValueTuple;

        }

        /// <summary>
        /// InterringWithNowEvent deals with how the interferrence with the now should be resolved.
        /// InterringWithNowEvent == 0. Ignore events interfeering with now
        /// InterringWithNowEvent == 1. Ignore all events interferring with now
        /// InterringWithNowEvent == 2. Ignore only the current added calendar event
        /// </summary>
        /// <param name="MyCalendarEvent"></param>
        /// <param name="NoneCommitedCalendarEventsEvents"></param>
        /// <param name="InterferringWithNowFlag"></param>
        /// <param name="NotDoneYet"></param>
        /// <param name="OptimizeFirstTwentyFour"></param>
        /// <param name="preserveFirstTwentyFourHours"></param>
        /// <returns></returns>
        CalendarEvent ReArrangeTimeLineWithinWithinCalendaEventRangeUpdated(
            CalendarEvent MyCalendarEvent,
            List<CalendarEvent> NoneCommitedCalendarEventsEvents,
            int InterferringWithNowFlag,
            HashSet<SubCalendarEvent> NotDoneYet,
            Location callLocation,
            bool OptimizeFirstTwentyFour = true,
            bool preserveFirstTwentyFourHours = true,
            bool shuffle = false)// this looks at the timeline of the calendar event and then tries to rearrange all subevents within the range to suit final output. Such that there will be sufficient time space for each subevent
        {
            /*
                Name{: Jerome Biotidara
             * this function is responsible for making sure there is some dynamic allotment of time to the subeevents. It takes a calendarevent, checks the alloted time frame and tries to move subevents within the time frame to satisfy the final goal.
             */
            if (MyCalendarEvent.IsFromRecurringAndNotChildRepeatCalEvent != false)//Artificially generates random subevents for the calendar event
            {
                throw new Exception("invalid calendar event detected in ReArrangeTimeLineWithinWithinCalendaEventRange. Repeat not allowed");
            }

            NoneCommitedCalendarEventsEvents.Add(MyCalendarEvent);
            var ProcrastinateAllEvent = getProcrastinateAllEvent();
            if (ProcrastinateAllEvent != null)
            {
                foreach (var SubEvent in ProcrastinateAllEvent.AllSubEvents)
                {
                    SubEvent.Location = CurrentLocation;
                }
            }

            TimeLine timeline = new TimeLine(Now.constNow.AddDays(util.defaultBeginDay), Now.ComputationRange.End);
            Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors, IEnumerable<SubCalendarEvent>> allInterferringSubCalEventsAndTimeLine = getAllInterferringEventsAndTimeLineInCurrentEvaluation(MyCalendarEvent, NoneCommitedCalendarEventsEvents, InterferringWithNowFlag, new HashSet<SubCalendarEvent>(), timeline);
            List<SubCalendarEvent> collectionOfInterferringSubCalEvents = allInterferringSubCalEventsAndTimeLine.Item2.ToList();
            List<SubCalendarEvent> ArrayOfInterferringSubEvents = allInterferringSubCalEventsAndTimeLine.Item2.ToList();
            foreach(var subEvent in ArrayOfInterferringSubEvents.Concat(allInterferringSubCalEventsAndTimeLine.Item4))
            {
                subEvent.disableCalculationMode();
            }
            
            TimeLine RangeForScheduleUpdate = allInterferringSubCalEventsAndTimeLine.Item1;
            IEnumerable<SubCalendarEvent> NowEvents = allInterferringSubCalEventsAndTimeLine.Item4.Where(obj => !obj.isLocked).ToList();
            if (InterferringWithNowFlag == 0)
            {
                foreach (SubCalendarEvent eachSubCalendarEvent in NowEvents)
                {
                    eachSubCalendarEvent.tempLockSubEvent();
                    eachSubCalendarEvent.ParentCalendarEvent.designateSubEvent(eachSubCalendarEvent, Now);
                }

            }
            Now.UpdateNow(RangeForScheduleUpdate.Start);


            TimeSpan SumOfAllEventsTimeSpan = Utility.SumOfActiveDuration(ArrayOfInterferringSubEvents);
            Tuple<List<SubCalendarEvent>, List<BlobSubCalendarEvent>> preppedDataForNExtStage = PrepareElementsThatWillNotFit(ArrayOfInterferringSubEvents, RangeForScheduleUpdate);
            ArrayOfInterferringSubEvents = preppedDataForNExtStage.Item1;
            var AllConflictingEvents = preppedDataForNExtStage.Item2;
            List<CalendarEvent> allCalEvent = AllConflictingEvents.Select(obj => CalendarEvent.getEmptyCalendarEvent(new EventID(obj.SubEvent_ID.getCalendarEventID()), obj.Start, obj.End)).ToList();
            for (int i = 0; i < allCalEvent.Count; i++)
            {
                CalendarEvent eachCalEvent = new CalendarEvent(allCalEvent[i], new SubCalendarEvent[] { AllConflictingEvents[i] });
                addCaleventToAllEventDictionary(eachCalEvent.Calendar_EventID.getIDUpToCalendarEvent(), eachCalEvent);
            }


            Dictionary<CalendarEvent, List<SubCalendarEvent>> DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents = new Dictionary<CalendarEvent, List<SubCalendarEvent>>();
            List<SubCalendarEvent> RigidSubCalendarEvents = new List<SubCalendarEvent>(0);
            List<BusyTimeLine> RigidSubCalendarEventsBusyTimeLine = new List<BusyTimeLine>(0);
            RigidSubCalendarEvents = ArrayOfInterferringSubEvents.Where(obj => obj.isLocked).ToList();
            RigidSubCalendarEventsBusyTimeLine = RigidSubCalendarEvents.Select(obj => obj.ActiveSlot).ToList();


            DayTimeLine[] AllDayTImeLine = Now.getAllDaysLookup().Select(obj => obj.Value).ToArray();
            foreach (DayTimeLine dayTimeLine in AllDayTImeLine)
            {
                dayTimeLine.Empty();
            }
            DayTimeLine firstDay = Now.firstDay;
            DayTimeLine secondDay = AllDayTImeLine[1];
            TimeLine precedingStart = new TimeLine(secondDay.Start.AddDays(-1), firstDay.Start);
            List<SubCalendarEvent> preceding24HourSubevent = new List<SubCalendarEvent>();// holds subevents that are within the preceding dayTImeline and preceding hours of now
            List<SubCalendarEvent> notPreceding24HourSubevent = new List<SubCalendarEvent>();// holds sub events that will be used for calculation

            foreach (SubCalendarEvent subEvent in ArrayOfInterferringSubEvents)
            {
                if (subEvent.ActiveSlot.End<= precedingStart.End && subEvent.ActiveSlot.doesTimeLineInterfere(precedingStart) && subEvent.isPre_reschedulingEnabled)
                {
                    preceding24HourSubevent.Add(subEvent);
                    subEvent.ParentCalendarEvent.designateSubEvent(subEvent, Now);
                    subEvent.lockPrecedingHours();
                }
                else
                {
                    notPreceding24HourSubevent.Add(subEvent);
                }
            }
            ArrayOfInterferringSubEvents = notPreceding24HourSubevent;

            List<SubCalendarEvent> isInterFerringWithNow = ArrayOfInterferringSubEvents.Where(obj => obj.isLocked && obj.IsDateTimeWithin(Now.calculationNow)).ToList();
            BlobSubCalendarEvent interferringWithNowBlob = null;
            CalendarEvent interferringWithNowCalEvent = null;
            if (isInterFerringWithNow.Count > 0)
            {
                interferringWithNowBlob = new BlobSubCalendarEvent(isInterFerringWithNow);
                interferringWithNowBlob.updateTimeLine(new TimeLine(Now.calculationNow, interferringWithNowBlob.End));
                foreach(var subEvent in isInterFerringWithNow)
                {
                    ArrayOfInterferringSubEvents.Remove(subEvent);
                    subEvent.ParentCalendarEvent.designateSubEvent(subEvent, Now);
                }
                ArrayOfInterferringSubEvents.Add(interferringWithNowBlob);
                interferringWithNowCalEvent = CalendarEvent.getEmptyCalendarEvent(interferringWithNowBlob.getTilerID, interferringWithNowBlob.Start, interferringWithNowBlob.End);
                interferringWithNowCalEvent = new CalendarEvent(interferringWithNowCalEvent, new SubCalendarEvent[] { interferringWithNowBlob });
                addCaleventToAllEventDictionary(interferringWithNowCalEvent.Calendar_EventID.getAllEventDictionaryLookup, interferringWithNowCalEvent);
            }
            

            double OccupancyOfTimeLineSPan = (double)SumOfAllEventsTimeSpan.Ticks / (double)RangeForScheduleUpdate.TimelineSpan.Ticks;
            //ArrayOfInterferringSubEvents = Utility.NotInList(ArrayOfInterferringSubEvents.ToList(), RigidSubCalendarEvents).ToList();//remove rigid elements


            TimeLine ReferenceTimeLine = RangeForScheduleUpdate.CreateCopy();
            ReferenceTimeLine.AddBusySlots(RigidSubCalendarEventsBusyTimeLine.ToArray());//Adds all the rigid elements

            TimeLine[] ArrayOfFreeSpots = getOnlyPertinentTimeFrame(getAllFreeSpots_NoCompleteSchedule(ReferenceTimeLine), ReferenceTimeLine).ToArray();
            ArrayOfFreeSpots = getOnlyPertinentTimeFrame(ArrayOfFreeSpots, ReferenceTimeLine).ToArray();

            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents = generateDictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents(ArrayOfInterferringSubEvents.ToList(), NoneCommitedCalendarEventsEvents);//generates a dictionary of a Calendar Event and the interferring events in the respective Calendar event
            //DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Add(MyCalendarEvent, MyCalendarEvent.AllEvents.ToList());//artificially adds enew calendar event to dictionary


            List<CalendarEvent> SortedInterFerringCalendarEvents_Deadline = DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Keys.ToList();
            SortedInterFerringCalendarEvents_Deadline = SortedInterFerringCalendarEvents_Deadline.OrderBy(obj => obj.End).ToList();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            ParallelizeCallsToDay(SortedInterFerringCalendarEvents_Deadline, ArrayOfInterferringSubEvents, AllDayTImeLine, callLocation, OptimizeFirstTwentyFour, preserveFirstTwentyFourHours, shuffle);
            purgeLocations();
            watch.Stop();
            TimeSpan scheduleElapsedTime = watch.Elapsed;
            Debug.WriteLine("ParallelizeCallsToDay took " + scheduleElapsedTime.ToString());
            preceding24HourSubevent.ForEach((subEvent) => {
                subEvent.unLockPrecedingHours();
            });

            foreach (BlobSubCalendarEvent eachSubCalEvent in preppedDataForNExtStage.Item2)
            {
                removeFromAllEventDictionary(eachSubCalEvent.SubEvent_ID.getIDUpToCalendarEvent());
            }

            if (interferringWithNowCalEvent != null)
            {
                removeFromAllEventDictionary(interferringWithNowCalEvent.Calendar_EventID.getAllEventDictionaryLookup);
            }

            if (ProcrastinateAllEvent != null)
            {
                Location location = new Location();
                foreach (var SubEvent in ProcrastinateAllEvent.AllSubEvents)
                {
                    SubEvent.Location = location;
                }
            }

            return MyCalendarEvent;

        }

        public void populateDayTimeLinesWithSubcalendarEvents()
        {
            IEnumerable<CalendarEvent> calendarEvents = getAllCalendarEvents().Where(calEvent => calEvent.isActive);
            TimeLine startToEnd = Now.StartToEnd;
            foreach (SubCalendarEvent subevent in calendarEvents.SelectMany(calEvent => calEvent.ActiveSubEvents))
            {
                
                if (startToEnd.doesTimeLineInterfere( subevent))
                {
                    TimeLine interFerringTimeline = startToEnd.InterferringTimeLine(subevent.StartToEnd);
                    DayTimeLine dayTimeLineStart = Now.getDayTimeLineByTime(interFerringTimeline.Start);
                    DayTimeLine dayTimeLineAfter = Now.getDayTimeLineByTime(interFerringTimeline.Start);
                    dayTimeLineStart.AddToSubEventList(subevent);
                    dayTimeLineAfter.AddToSubEventList(subevent);
                }
                
            }
        }

        string BuildStringIndexForMatch(BusyTimeLine PrecedingTimeLineEvent, BusyTimeLine NextTimeLineEvent)
        {
            EventID MyEventID = new EventID(PrecedingTimeLineEvent.TimeLineID);
            int PrecedingCalendarEventID = Convert.ToInt16(MyEventID.getCalendarEventComponent());
            int NextCalendarEventID = Convert.ToInt16(new EventID(NextTimeLineEvent.TimeLineID).getCalendarEventComponent());

            if (PrecedingCalendarEventID == NextCalendarEventID)
            {
                return "sameElement";
            }
            else
            {
                if (PrecedingCalendarEventID < NextCalendarEventID)
                {
                    return PrecedingCalendarEventID + "_" + NextCalendarEventID;
                }
                return NextCalendarEventID + "_" + PrecedingCalendarEventID;
            }
        }

        double EvaluateClumpingIndex(TimeLine ReferenFilledReferenceTimeLine, Dictionary<string, double> CurrentDictionary)
        {
            BusyTimeLine[] ListOfBusySlots = ReferenFilledReferenceTimeLine.OccupiedSlots;
            int i, j = 0;
            double CurrentSumOfLocationData = 0;
            for (i = 0; i < (ListOfBusySlots.Length - 1); i++)
            {
                j = i + 1;
                string generatedIndexMatch = BuildStringIndexForMatch(ListOfBusySlots[i], ListOfBusySlots[j]);

                CurrentSumOfLocationData += CurrentDictionary[generatedIndexMatch];
            }

            return CurrentSumOfLocationData;
        }

        /// <summary>
        /// Function tries to ensure it can fit subevent in its bound timeline with other subevents. Each timeline in <paramref name="AllTimeLines"/> must be part of the <paramref name="singleTimelineToCapableSubevents"/> dictionary key.
        /// Every subCalendarevent in the List that is the value of the dictionary <paramref name="singleTimelineToCapableSubevents"/> must only be able to fit in the timeline that is the key of <paramref name="singleTimelineToCapableSubevents"/>
        /// </summary>
        /// <param name="AllTimeLines"></param>
        /// <param name="singleTimelineToCapableSubevents"></param>
        /// <returns></returns>
        Dictionary<TimeLine, List<SubCalendarEvent>> stitchRestrictedSubCalendarEvent(List<TimeLine> AllTimeLines, Dictionary<TimeLine, List<SubCalendarEvent>> singleTimelineToCapableSubevents)
        {
            List<SubCalendarEvent> AllSubCall = singleTimelineToCapableSubevents.SelectMany(obj => obj.Value).ToList();
            SubCalendarEvent.updateMiscData(AllSubCall, 0);

            Dictionary<string, bool> var1 = new System.Collections.Generic.Dictionary<string, bool>();
            foreach (TimeLine eachTimeLine in AllTimeLines)//checks of subcal can exist within time frame increases the intData for every TimeLine it can fit in
            {
                SubCalendarEvent.incrementMiscdata(AllSubCall.Where(obj => obj.canExistWithinTimeLine(eachTimeLine)).ToList());
                List<mTuple<bool, SubCalendarEvent>> testSendData = Utility.SubCalEventsTomTuple(singleTimelineToCapableSubevents[eachTimeLine], true);
                singleTimelineToCapableSubevents[eachTimeLine] = stitchRestrictedSubCalendarEvent(testSendData, eachTimeLine).Select(OBJ => OBJ.Item2).ToList();
            }


            int i = 0;
            foreach (TimeLine eachTimeLine in AllTimeLines)
            {
                List<mTuple<bool, SubCalendarEvent>> testSendData = Utility.SubCalEventsTomTuple(singleTimelineToCapableSubevents[eachTimeLine], true);
                singleTimelineToCapableSubevents[eachTimeLine] = stitchRestrictedSubCalendarEvent(testSendData, eachTimeLine).Select(obj => obj.Item2).ToList();
                bool willpin = Utility.PinSubEventsToStart(singleTimelineToCapableSubevents[eachTimeLine], eachTimeLine);
                i++;
            }
            SubCalendarEvent.updateMiscData(AllSubCall, 0);
            return singleTimelineToCapableSubevents;
        }

        Tuple<List<SubCalendarEvent>[], DayTimeLine[], List<SubCalendarEvent>> peekIntoDays(CalendarEvent MyCalendarEvent, List<CalendarEvent> NoneCommitedCalendarEventsEvents, int InterferringWithNowFlag, HashSet<SubCalendarEvent> NotDoneYet, Location callLocation, uint NumberOfDays = 28)
        {
            List<SubCalendarEvent>[] AssignedEvents = new List<SubCalendarEvent>[NumberOfDays];
            List<DayTimeLine>[] TimeFrame = new List<DayTimeLine>[NumberOfDays];
            for (int j = 0; j < AssignedEvents.Length; j++)
            {
                AssignedEvents[j] = new List<SubCalendarEvent>();
                TimeFrame[j] = new List<DayTimeLine>();
            }
            if (MyCalendarEvent.IsFromRecurringAndNotChildRepeatCalEvent != false)//Artificially generates random subevents for the calendar event
            {
                throw new Exception("invalid calendar event detected in peekIntoDays. Repeat not allowed");
            }

            NoneCommitedCalendarEventsEvents.Add(MyCalendarEvent);

            var ProcrastinateAllEvent = getProcrastinateAllEvent();
            if (ProcrastinateAllEvent != null)
            {
                foreach (var SubEvent in ProcrastinateAllEvent.AllSubEvents)
                {
                    SubEvent.Location = CurrentLocation;
                }
            }

            TimeLine timeline = new TimeLine(Now.constNow.AddDays(util.defaultBeginDay), Now.getAllDaysCount((uint)NumberOfDays).Last().End);
            Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors, IEnumerable<SubCalendarEvent>> allInterferringSubCalEventsAndTimeLine = getAllInterferringEventsAndTimeLineInCurrentEvaluation(MyCalendarEvent, NoneCommitedCalendarEventsEvents, InterferringWithNowFlag, new HashSet<SubCalendarEvent>(), timeline);
            //Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors, IEnumerable<SubCalendarEvent>> allInterferringSubCalEventsAndTimeLine = getAllInterferringEventsAndTimeLineInCurrentEvaluationOldDesign(MyCalendarEvent, NoneCommitedCalendarEventsEvents, InterferringWithNowFlag, ExemptFromCalculation, new TimeLine(Now.constNow.AddDays(-90), Now.ComputationRange.End));
            List<SubCalendarEvent> collectionOfInterferringSubCalEvents = allInterferringSubCalEventsAndTimeLine.Item2.ToList();
            List<SubCalendarEvent> ArrayOfInterferringSubEvents = allInterferringSubCalEventsAndTimeLine.Item2.ToList();
            TimeLine RangeForScheduleUpdate = allInterferringSubCalEventsAndTimeLine.Item1;
            IEnumerable<SubCalendarEvent> NowEvents = allInterferringSubCalEventsAndTimeLine.Item4.Where(obj => !obj.isLocked).ToList();
            if (InterferringWithNowFlag == 0)
            {
                foreach (SubCalendarEvent eachSubCalendarEvent in NowEvents)
                {
                    eachSubCalendarEvent.tempLockSubEvent();
                    eachSubCalendarEvent.ParentCalendarEvent.designateSubEvent(eachSubCalendarEvent, Now);
                }

            }
            Now.UpdateNow(RangeForScheduleUpdate.Start);


            TimeSpan SumOfAllEventsTimeSpan = Utility.SumOfActiveDuration(ArrayOfInterferringSubEvents);
            Tuple<List<SubCalendarEvent>, List<BlobSubCalendarEvent>> preppedDataForNExtStage = PrepareElementsThatWillNotFit(ArrayOfInterferringSubEvents, RangeForScheduleUpdate);
            ArrayOfInterferringSubEvents = preppedDataForNExtStage.Item1;
            var AllConflictingEvents = preppedDataForNExtStage.Item2;
            List<CalendarEvent> allCalEvent = AllConflictingEvents.Select(obj => CalendarEvent.getEmptyCalendarEvent(new EventID(obj.SubEvent_ID.getCalendarEventID()), obj.Start, obj.End)).ToList();
            for (int i = 0; i < allCalEvent.Count; i++)
            {
                CalendarEvent eachCalEvent = new CalendarEvent(allCalEvent[i], new SubCalendarEvent[] { AllConflictingEvents[i] });
                addCaleventToAllEventDictionary(eachCalEvent.Calendar_EventID.getIDUpToCalendarEvent(), eachCalEvent);
            }

            Dictionary<CalendarEvent, List<SubCalendarEvent>> DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents = new Dictionary<CalendarEvent, List<SubCalendarEvent>>();
            List<SubCalendarEvent> RigidSubCalendarEvents = new List<SubCalendarEvent>(0);
            List<BusyTimeLine> RigidSubCalendarEventsBusyTimeLine = new List<BusyTimeLine>(0);
            RigidSubCalendarEvents = ArrayOfInterferringSubEvents.Where(obj => obj.isLocked).ToList();
            RigidSubCalendarEventsBusyTimeLine = RigidSubCalendarEvents.Select(obj => obj.ActiveSlot).ToList();

            DayTimeLine[] AllDays = Now.getAllDaysCount((uint)NumberOfDays).OrderBy(o => o.Start).ToArray();
            foreach (DayTimeLine dayTimeLine in AllDays)
            {
                dayTimeLine.Empty();
            }
            DayTimeLine firstDay = Now.firstDay;
            DayTimeLine secondDay = AllDays[1];
            TimeLine precedingStart = new TimeLine(secondDay.Start.AddDays(-1), firstDay.Start);
            List<SubCalendarEvent> preceding24HourSubevent = new List<SubCalendarEvent>();// holds subevents that are within the preceding dayTImeline and preceding hours of now
            List<SubCalendarEvent> notPreceding24HourSubevent = new List<SubCalendarEvent>();// holds sub events that will be used for calculation
            foreach (SubCalendarEvent subEvent in ArrayOfInterferringSubEvents)
            {
                if (subEvent.ActiveSlot.End <= precedingStart.End && subEvent.ActiveSlot.doesTimeLineInterfere(precedingStart) && subEvent.isPre_reschedulingEnabled)
                {
                    preceding24HourSubevent.Add(subEvent);
                    subEvent.ParentCalendarEvent.designateSubEvent(subEvent, Now);
                    subEvent.lockPrecedingHours();
                }
                else
                {
                    notPreceding24HourSubevent.Add(subEvent);
                }
            }
            ArrayOfInterferringSubEvents = notPreceding24HourSubevent;

            List<SubCalendarEvent> isInterFerringWithNow = ArrayOfInterferringSubEvents.Where(obj => obj.isLocked && obj.IsDateTimeWithin(Now.calculationNow)).ToList();
            BlobSubCalendarEvent interferringWithNowBlob = null;
            CalendarEvent interferringWithNowCalEvent = null;
            if (isInterFerringWithNow.Count > 0)
            {
                interferringWithNowBlob = new BlobSubCalendarEvent(isInterFerringWithNow);
                interferringWithNowBlob.updateTimeLine(new TimeLine(Now.calculationNow, interferringWithNowBlob.End));
                foreach (var subEvent in isInterFerringWithNow)
                {
                    subEvent.ParentCalendarEvent.designateSubEvent(subEvent, Now);
                    ArrayOfInterferringSubEvents.Remove(subEvent);
                }
                ArrayOfInterferringSubEvents.Add(interferringWithNowBlob);
                interferringWithNowCalEvent = CalendarEvent.getEmptyCalendarEvent(interferringWithNowBlob.getTilerID, interferringWithNowBlob.Start, interferringWithNowBlob.End);
                interferringWithNowCalEvent = new CalendarEvent(interferringWithNowCalEvent, new SubCalendarEvent[] { interferringWithNowBlob });
                addCaleventToAllEventDictionary(interferringWithNowCalEvent.Calendar_EventID.getIDUpToCalendarEvent(), interferringWithNowCalEvent);
            }


            double OccupancyOfTimeLineSPan = (double)SumOfAllEventsTimeSpan.Ticks / (double)RangeForScheduleUpdate.TimelineSpan.Ticks;
            //ArrayOfInterferringSubEvents = Utility.NotInList(ArrayOfInterferringSubEvents.ToList(), RigidSubCalendarEvents).ToList();//remove rigid elements


            TimeLine ReferenceTimeLine = RangeForScheduleUpdate.CreateCopy();
            ReferenceTimeLine.AddBusySlots(RigidSubCalendarEventsBusyTimeLine.ToArray());//Adds all the rigid elements

            TimeLine[] ArrayOfFreeSpots = getOnlyPertinentTimeFrame(getAllFreeSpots_NoCompleteSchedule(ReferenceTimeLine), ReferenceTimeLine).ToArray();
            ArrayOfFreeSpots = getOnlyPertinentTimeFrame(ArrayOfFreeSpots, ReferenceTimeLine).ToArray();

            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents = generateDictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents(ArrayOfInterferringSubEvents.ToList(), NoneCommitedCalendarEventsEvents);//generates a dictionary of a Calendar Event and the interferring events in the respective Calendar event


            List<BlobSubCalendarEvent> interFerringBlob = Utility.getConflictingEvents(RigidSubCalendarEvents).Item1;

            List<CalendarEvent> SortedInterFerringCalendarEvents_Deadline = DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Keys.ToList();
            SortedInterFerringCalendarEvents_Deadline = SortedInterFerringCalendarEvents_Deadline.OrderBy(obj => obj.End).ToList();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            ParallelizeCallsToDay(SortedInterFerringCalendarEvents_Deadline, ArrayOfInterferringSubEvents.ToList(), AllDays, callLocation, false);
            watch.Stop();
            TimeSpan scheduleElapsedTime = watch.Elapsed;
            Debug.WriteLine("ParallelizeCallsToDay took " + scheduleElapsedTime.ToString());
            preceding24HourSubevent.ForEach((subEvent) => {
                subEvent.unLockPrecedingHours();
            });

            foreach (BlobSubCalendarEvent eachSubCalEvent in preppedDataForNExtStage.Item2)
            {
                removeFromAllEventDictionary(eachSubCalEvent.SubEvent_ID.getIDUpToCalendarEvent());
            }

            if (interferringWithNowCalEvent != null)
            {
                removeFromAllEventDictionary(interferringWithNowCalEvent.Calendar_EventID.getIDUpToCalendarEvent());
            }

            List<SubCalendarEvent> ConflictingEvents = new List<SubCalendarEvent>();
            foreach (SubCalendarEvent eachSubCalendarEvent in ArrayOfInterferringSubEvents)
            {
                int index = (int)(eachSubCalendarEvent.UniversalDayIndex - Now.firstDay.UniversalIndex);

                if ((index < NumberOfDays) && (index >= 0))
                {
                    AssignedEvents[index].Add(eachSubCalendarEvent);
                }
                else
                {
                    ConflictingEvents.Add(eachSubCalendarEvent);
                }
            }

            if (ProcrastinateAllEvent != null)
            {
                Location location = new Location();
                foreach (var SubEvent in ProcrastinateAllEvent.AllSubEvents)
                {
                    SubEvent.Location = location;
                }
            }
            interFerringBlob.ForEach(obj => ConflictingEvents.AddRange(obj.getSubCalendarEventsInBlob()));

            List<SubCalendarEvent>[] First7DaysEvents = AssignedEvents.Take(7).ToArray();
            DayTimeLine[] First7Days = AllDays.Take(7).ToArray();

            Tuple<List<SubCalendarEvent>[], DayTimeLine[], List<SubCalendarEvent>> RetValue = new Tuple<List<SubCalendarEvent>[], DayTimeLine[], List<SubCalendarEvent>>(First7DaysEvents, First7Days, ConflictingEvents);
            return RetValue;
        }



        /// <summary>
        /// Function assigns a bunch of rigid subevents to a given day. Each Rigid designates itself to a given day. Each rigid adds itself to the busy slot of the timeLine.
        /// </summary>
        /// <param name="AllDays"></param>
        /// <param name="AllRigidSubEvents"></param>
        /// returns the dictionary of the designated subcalendar events and their days. Note if subevent was exceeds the bounds then it wont be in return value
        Dictionary<SubCalendarEvent, List<long>> DesignateSubEventsToDayTimeLine(DayTimeLine[] OrderedyAscendingAllDays, IEnumerable<SubCalendarEvent> AllRigidSubEvents)
        {
            long First = OrderedyAscendingAllDays.First().UniversalIndex;
            Dictionary<SubCalendarEvent, List<long>> RetValue = new Dictionary<SubCalendarEvent, List<long>>();

            //Parallel.ForEach(AllRigidSubEvents, eachSubCalendarEvent =>

            foreach (SubCalendarEvent eachSubCalendarEvent in AllRigidSubEvents)
            {
                List<long> myDays = new List<long>();
                long SubCalFirstIndex = Now.getDayIndexFromStartOfTime(eachSubCalendarEvent.Start);
                long SubCalLastIndex = Now.getDayIndexFromStartOfTime(eachSubCalendarEvent.End);
                long DayDiff = SubCalLastIndex - SubCalFirstIndex;

                int BoundedIndex = (int)(SubCalFirstIndex - First);
                if ((BoundedIndex < 0) || (BoundedIndex >= OrderedyAscendingAllDays.Length))
                {
                    continue;
                }
                myDays.Add(SubCalFirstIndex);
                OrderedyAscendingAllDays[BoundedIndex].AddToSubEventList(eachSubCalendarEvent);
                eachSubCalendarEvent.ParentCalendarEvent.designateSubEvent(eachSubCalendarEvent, Now);
                _ConflictingSubEvents.Remove(eachSubCalendarEvent);
                //eachSubCalendarEvent.updateDayIndex(SubCalFirstIndex, eachSubCalendarEvent.ParentCalendarEvent);
                eachSubCalendarEvent.ParentCalendarEvent.removeDayTimesFromFreeUpdays(SubCalFirstIndex);
                for (long i = SubCalFirstIndex + 1, j = 0; j < DayDiff; j++, i++)
                {
                    BoundedIndex = (int)(i - First);
                    if (BoundedIndex < OrderedyAscendingAllDays.Length)// in case the rigid sub event day index is higher than OrderedyAscendingAllDays max index
                    {
                        OrderedyAscendingAllDays[BoundedIndex].AddToSubEventList(eachSubCalendarEvent);
                        myDays.Add(i);
                    }

                }
                RetValue.Add(eachSubCalendarEvent, myDays);
            }
            //);
            return RetValue;
        }

        public Tuple<CalendarEvent, SubCalendarEvent> getNearestEventToNow()
        {
            TimeLine timeline = getTimeLine();
            List<SubCalendarEvent> subEvents = getAllCalendarEvents().Where(calEvent => calEvent.isActive).SelectMany(calEvent => calEvent.ActiveSubEvents).OrderBy(subEvent => subEvent.Start).Where(subevent => subevent.End >= Now.constNow).ToList();
            SubCalendarEvent nearestSubEvent = subEvents.FirstOrDefault();
            CalendarEvent subEventCalEvent = null;
            if (nearestSubEvent != null)
            {
                subEventCalEvent = getCalendarEvent(nearestSubEvent.getId);
            }
            Tuple<CalendarEvent, SubCalendarEvent> retValue = new Tuple<CalendarEvent, SubCalendarEvent>(subEventCalEvent, nearestSubEvent);
            return retValue;
        }

        /// <summary>
        /// Funtion makes iterative calls to the daily optimizers
        /// </summary>
        /// <param name="AllDayTimeLine">A list of days to be optimied</param>
        /// <param name="location">The location passed from the client where this call is being made, this is most likely passed from say shuffle action</param>
        IDictionary<DayTimeLine, OptimizedPath> optimizeDays(List<DayTimeLine> AllDayTimeLine, Location location)
        {
            ConcurrentDictionary<DayTimeLine, OptimizedPath> dayToOPtimization = new ConcurrentDictionary<DayTimeLine, OptimizedPath>();
            Location home = null;
            if (Locations.ContainsKey("home"))
            {
                home = Locations["home"];
            }
            else
            {
                home = CurrentLocation;
            }
            for (int i = 0; i < AllDayTimeLine.Count; i++)
            {
                Stopwatch eachLoop = new Stopwatch();
                eachLoop.Start();
                DayTimeLine EachDay = AllDayTimeLine[i];
                HashSet<string> ids = new HashSet<string>(EachDay.getSubEventsInTimeLine().Select(subEvent => subEvent.getId));
                Location beginLocation, endLocation = home;

                if (i == 0 && location != null)// its he first day and the location provided is not null
                {
                    beginLocation = location;
                }
                else
                {
                    beginLocation = home;
                }

                EachDay.BeginLocation = beginLocation;

                if(!EachDay.BeginLocation.IsVerified)
                {
                    EachDay.BeginLocation.verify();
                }

                    Dictionary<Location, int> durationToTimeSpan = new Dictionary<Location, int>();
                if(EachDay.BeginLocation != null && EachDay.BeginLocation.IsVerified)
                {

                    TimeSpan sleepSpan = Utility.SixHourTimeSpan;
                    TimeSpan minFullDaySpanNeededForFullSleep = TimeSpan.FromHours(1);
                    if (EachDay.TimelineSpan < minFullDaySpanNeededForFullSleep)
                    {
                        sleepSpan = Utility.OneHourTimeSpan;
                    }
                    int durationQutient = ((int)Math.Round(sleepSpan.TotalMinutes / Utility.QuarterHourTimeSpan.TotalMinutes));
                    if (durationToTimeSpan.ContainsKey(EachDay.BeginLocation))
                    {
                        durationToTimeSpan[EachDay.BeginLocation] += durationQutient;
                    }
                    else
                    {
                        durationToTimeSpan.Add(EachDay.BeginLocation, durationQutient);
                    }
                }

                if (EachDay.EndLocation!=null && EachDay.EndLocation.IsVerified)
                {
                    int durationQutient = ((int)Math.Round(Utility.OneHourTimeSpan.TotalMinutes / Utility.QuarterHourTimeSpan.TotalMinutes));//Choosing one hour span so we have a total of Seven hours (Six hours from the preceding EachDay.BeginLocation)  dedicated to border locations. 
                    if (durationToTimeSpan.ContainsKey(EachDay.EndLocation))
                    {
                        durationToTimeSpan[EachDay.EndLocation] += durationQutient;
                    }
                    else
                    {
                        durationToTimeSpan.Add(EachDay.EndLocation, durationQutient);
                    }
                }


                Dictionary<string, List<SubCalendarEvent>> calEventToSubEvents = new Dictionary<string, List<SubCalendarEvent>>();
                foreach (SubCalendarEvent subEvent in EachDay.getSubEventsInTimeLine())
                {
                    if(!subEvent.ParentCalendarEvent.isLocked)
                    {
                        string calComponent = subEvent.getTilerID.getCalendarEventComponent();
                        List<SubCalendarEvent> subeventOfSameCal = null;
                        if (calEventToSubEvents.ContainsKey(calComponent))
                        {
                            subeventOfSameCal = calEventToSubEvents[calComponent];
                        }
                        else
                        {
                            subeventOfSameCal = new List<SubCalendarEvent>();
                            calEventToSubEvents[calComponent] = subeventOfSameCal;
                        }
                        subeventOfSameCal.Add(subEvent);
                    }
                }


                Dictionary<string, SubCalendarEvent> calEventToSubeventWithVerifiedForDayLocation = new Dictionary<string, SubCalendarEvent>();
                foreach (SubCalendarEvent subEvent in EachDay.getSubEventsInTimeLine().Where(sub => sub.LocationObj.IsVerified || (sub.isLocationAmbiguous && sub.IsValidationRun)))
                {
                    int durationQutient = ((int)Math.Round(subEvent.getActiveDuration.TotalMinutes / Utility.QuarterHourTimeSpan.TotalMinutes));
                    if (durationToTimeSpan.ContainsKey(subEvent.Location))
                    {
                        durationToTimeSpan[subEvent.Location] += durationQutient;
                    }
                    else {
                        durationToTimeSpan.Add(subEvent.Location, durationQutient);
                    }
                    string calIdComponent = subEvent.getTilerID.getCalendarEventComponent();
                    if (calEventToSubEvents.ContainsKey(calIdComponent))
                    {
                        if(!calEventToSubeventWithVerifiedForDayLocation.ContainsKey(calIdComponent))
                        {
                            calEventToSubeventWithVerifiedForDayLocation.Add(calIdComponent, subEvent);
                        }
                    }
                }

                Stopwatch locationValidation = new Stopwatch();
                locationValidation.Start();
                foreach (SubCalendarEvent subEvent in EachDay.getSubEventsInTimeLine().Where(sub => sub.isLocationAmbiguous && !sub.IsValidationRun).OrderBy(o=>o.Score))
                {
                    if(
                        subEvent.LocationObj.Address.isNot_NullEmptyOrWhiteSpace() || 
                        subEvent.LocationObj.Description.isNot_NullEmptyOrWhiteSpace() || 
                        subEvent.LocationObj.SearchdDescription.isNot_NullEmptyOrWhiteSpace())// checks if the provided location object actually includes strings to be looked up
                    {
                        List<Location> otherSubEventLocation = new List<Location>();
                        foreach (var kvp in durationToTimeSpan)
                        {
                            if (kvp.Key != subEvent.Location)
                            {
                                for (int locationCount = 0; locationCount < durationToTimeSpan[kvp.Key]; locationCount++)
                                {
                                    otherSubEventLocation.Add(kvp.Key);
                                }
                            }
                        }

                        Location averageLocation = Location.AverageGPSLocation(otherSubEventLocation);
                        if (averageLocation != null && (averageLocation.isNull || averageLocation.isDefault))
                        {
                            averageLocation = home != null && !home.isDefault && !home.isNull ? home : CurrentLocation;
                        }

                        string calIdComponent = subEvent.getTilerID.getCalendarEventComponent();

                        if (calEventToSubeventWithVerifiedForDayLocation.ContainsKey(calIdComponent))
                        {
                            SubCalendarEvent subEventWithValidatedLocation = calEventToSubeventWithVerifiedForDayLocation[calIdComponent];
                            if(subEventWithValidatedLocation.LocationObj == subEvent.LocationObj)
                            {
                                subEvent.updateLocationValidationId(subEventWithValidatedLocation.LocationValidationId);
                            }
                            else
                            {
                                throw new Exception("Sonmething is wrong with location obj, check your app logic for consistency");
                            }
                            
                        }
                        else
                        {
                            if (averageLocation != null && !averageLocation.isDefault && !averageLocation.isNull)
                            {
                                subEvent.validateLocation(averageLocation);/// This might kill performance because of multiple calls to google for validation
                                int durationQutient = ((int)Math.Round(subEvent.getActiveDuration.TotalMinutes / Utility.QuarterHourTimeSpan.TotalMinutes));
                                if (subEvent.Location.isNotNullAndNotDefault)
                                {
                                    if (durationToTimeSpan.ContainsKey(subEvent.Location))
                                    {
                                        durationToTimeSpan[subEvent.Location] += durationQutient;
                                    }
                                    else
                                    {
                                        durationToTimeSpan.Add(subEvent.Location, durationQutient);
                                    }
                                }

                            }
                        }

                        
                    }
                }
                locationValidation.Stop();
                Debug.WriteLine("validation Loop" + locationValidation.Elapsed.ToString());
                OptimizedPath dayPath = new OptimizedPath(EachDay, beginLocation, endLocation, home);

                Stopwatch eachDayOPtimization = new Stopwatch();
                eachDayOPtimization.Start();
                dayToOPtimization.AddOrUpdate(EachDay, dayPath, ((key, oldValue) => { return dayPath; }));
                dayPath.OptimizePath();
                eachDayOPtimization.Stop();
                //Debug.WriteLine("each day optimization" + eachDayOPtimization.Elapsed.ToString());
                foreach (SubCalendarEvent subEvent in dayPath.UnassignedSubEvents)
                {
                    EachDay.RemoveSubEvent(subEvent.Id);
                }
                eachLoop.Stop();
                //Debug.WriteLine("each day Loop" + eachLoop.Elapsed.ToString());
                
            }
            return dayToOPtimization;
        }

        /// <summary>
        /// Given a collection of lockedSubEvents that interfere with dayTimeLine, function returns a collection of subevents that only interfere with daytimeline.
        /// These returned subevents are not subevents in lockedSubEvents. They are different rigid objects
        /// </summary>
        /// <param name="dayTimeLine"></param>
        /// <param name="lockedSubEvents"></param>
        /// <returns></returns>
        IEnumerable<SubCalendarEvent> intersectingRigidEvents(DayTimeLine dayTimeLine, IEnumerable<SubCalendarEvent> lockedSubEvents) {
            List<SubCalendarEvent> retValue = new List<SubCalendarEvent>();
            foreach (SubCalendarEvent eachSubEvent in lockedSubEvents)
            {
                if(eachSubEvent.isLocked)
                {
                    TimeLine interferringTimeLine = eachSubEvent.ActiveSlot.InterferringTimeLine(dayTimeLine);
                    if (interferringTimeLine!=null)
                    {
                        CalendarEvent calendarEvent = CalendarEvent.getEmptyCalendarEvent(EventID.GenerateCalendarEvent(), interferringTimeLine.Start, interferringTimeLine.End);
                        SubCalendarEvent subEVent = calendarEvent.AllSubEvents.First();
                        subEVent.Location = eachSubEvent.Location;
                        subEVent.updateLocationValidationId( eachSubEvent.LocationValidationId);
                        subEVent.Enable(calendarEvent);
                        retValue.Add(subEVent);
                    }
                }
                else
                {
                    throw new Exception("This function only deals with locked subevents ");
                }
                
            }


            return retValue;
        }

        Tuple<SubCalendarEvent, SubCalendarEvent, SubCalendarEvent> spaceEventsByTravelTime(DayTimeLine dayTimeline, List<SubCalendarEvent> subEvents, bool useRemoteCalls = true, DayTimeLine precedingDay = null)
        {
            SubCalendarEvent wakeSubEvent = null, SleepSubEvent = null, SleepOfPreviousDay = null;
            subEvents.ForEach(subEvent => subEvent.Conflicts.UpdateConflictFlag(false));
            List<SubCalendarEvent> orderedByStartSubEvents = subEvents.OrderBy(subEvent => subEvent.Start).ThenBy(o => o.getActiveDuration).ToList();// the duration is needed for instances where there are two events with the same start. Think Zero time span events, where start time and end times are the same. In this case the next event after a zero time span events can have a same start time thats the same as the zero timespan event. An example zero time sapn event is the initializing procrastinate all event
            TimeLine dayOnlyTimeLine = dayTimeline.getJustTimeLine();
            Location precedingDayLocation = null;
            if(precedingDay != null)
            {
                SubCalendarEvent precedingDaySleepSubEvent = precedingDay.SleepSubEvent;
                if(precedingDaySleepSubEvent!=null)
                {
                    precedingDayLocation = precedingDaySleepSubEvent.Location;
                }
            }

            Dictionary<SubCalendarEvent, int> subEventToIndex = new Dictionary<SubCalendarEvent, int>();
            for(int i = 0; i< orderedByStartSubEvents.Count; i++)
            {
                SubCalendarEvent subEvent = orderedByStartSubEvents[i];
                subEventToIndex.Add(subEvent, i);
            }

            if (orderedByStartSubEvents.Count > 0)
            {
                DateTimeOffset EarliestStart = dayOnlyTimeLine.Start;// AllSubEvents.Max(obj => obj.Start);
                DateTimeOffset LatestEnd = dayOnlyTimeLine.End;//AllSubEvents.Max(obj=>obj.End);

                DateTimeOffset LowestSubEventStart = orderedByStartSubEvents.Min(obj => obj.Start);
                DateTimeOffset HighestSubEventStart = orderedByStartSubEvents.Max(obj => obj.End);

                EarliestStart = LowestSubEventStart < EarliestStart ? LowestSubEventStart : EarliestStart;
                LatestEnd = HighestSubEventStart > LatestEnd ? HighestSubEventStart : LatestEnd;
                TimeLine RefTimeLine = new TimeLine(EarliestStart, LatestEnd);
                TimeLine LastSuccessfull = RefTimeLine.StartToEnd;
                RefTimeLine = new TimeLine(EarliestStart, LatestEnd);

                var subEventssToTimelines = Utility.subEventToMaxSpaceAvailable(RefTimeLine, orderedByStartSubEvents);
                List<KeyValuePair<SubCalendarEvent, nTuple<TimeLine, TimeLine, TimeLine>>> kvpSubEventssToTimelines = subEventssToTimelines.ToList();
                Dictionary<SubCalendarEvent, List<double>> subEventToDimensions = kvpSubEventssToTimelines.ToDictionary(kvp => kvp.Key, kvp => getTimeLineSpaceAndTimeLineStart(RefTimeLine.End, kvp.Value.Item1));
                List<KeyValuePair<SubCalendarEvent, List<double>>> kvpSubEventToDimensions = subEventToDimensions.ToList();
                List<double> result = Utility.multiDimensionCalculation((kvpSubEventToDimensions.Select(obj => (IList<double>)obj.Value).ToList()));
                int maxIndex = result.MaxIndex();
                var sleepKvp = kvpSubEventssToTimelines[maxIndex];
                TimeLine afterSleepTimeLine = new TimeLine(sleepKvp.Value.Item1.Start, RefTimeLine.End);

                Dictionary<SubCalendarEvent, mTuple<TimeSpan, TimeSpan>> subEventToBuffer = new Dictionary<SubCalendarEvent, mTuple<TimeSpan, TimeSpan>>();
                if(orderedByStartSubEvents.Count > 1)
                {
                    subEventToBuffer = CreateBufferForEachEvent(orderedByStartSubEvents, RefTimeLine, useRemoteCalls);
                }
                else
                {
                    subEventToBuffer.Add(orderedByStartSubEvents[0], new mTuple<TimeSpan, TimeSpan>(Utility.ZeroTimeSpan, Utility.ZeroTimeSpan));
                }
                foreach(var kvp in subEventToBuffer)
                {
                    kvp.Key.TravelTimeAfter = kvp.Value.Item2;
                    int index = subEventToIndex[kvp.Key];
                    if(index >0)
                    {
                        orderedByStartSubEvents[index].TravelTimeBefore = kvp.Value.Item1;
                    }
                }

                TimeLine maxSleepTimeLine = sleepKvp.Value.Item1.TimelineSpan >= sleepKvp.Value.Item2.TimelineSpan ? sleepKvp.Value.Item1 : sleepKvp.Value.Item2;

                List<SubCalendarEvent> orderedByScoreSubEvent = orderedByStartSubEvents.OrderBy(sub => sub.Score).ToList();
                Dictionary<DaySection, TimeLine> splitIntoDaySection = TimeOfDayPreferrence.splitIntoDaySections(dayOnlyTimeLine);
                List<SubCalendarEvent> postSleepSubEvents = new List<SubCalendarEvent>();
                TimeLine postSleepTimeline = new TimeLine(RefTimeLine.Start, RefTimeLine.End);
                TimeLine sleepTimeLine = null;
                if (splitIntoDaySection.ContainsKey(DaySection.Sleep))
                {
                    int previousDaySleepSubeventIndex = -1;// Holds the last subevent index before the postsubEvent starts
                    int sleepSubEventindex = subEventToIndex[sleepKvp.Key];
                    TimeLine sleepSectionTimeLine = splitIntoDaySection[DaySection.Sleep];
                    TimeLine beforeTimeLine = sleepSectionTimeLine.InterferringTimeLine(sleepKvp.Value.Item1);
                    if(beforeTimeLine!=null && beforeTimeLine.TimelineSpan <= Utility.SleepSpan)// this ensures we try to get at six hours of sleep. If the timeframe interferring with the sleep section is less than six hours but the timeline available to the "free space " before the sleep subeent is more than six hours then there might be some lee way
                        //&& sleepKvp.Value.Item1.TimelineSpan >= Utility.SleepSpan
                    {
                        TimeSpan sleepTimeSpanBound = sleepKvp.Value.Item1.TimelineSpan < Utility.SleepSpan ? sleepKvp.Value.Item1.TimelineSpan : Utility.SleepSpan;
                        TimeSpan leftOverpan = sleepTimeSpanBound - beforeTimeLine.TimelineSpan;
                        beforeTimeLine = new TimeLine(beforeTimeLine.Start, beforeTimeLine.End.Add(leftOverpan));
                    }
                    TimeLine afterTimeLine = sleepSectionTimeLine.InterferringTimeLine(sleepKvp.Value.Item2);
                    if (afterTimeLine != null && afterTimeLine.TimelineSpan <= Utility.SleepSpan)// this ensures we try to get at six hours of sleep. If the timeframe interferring with the sleep section is less than six hours but the timeline available to the "free space " before the sleep subeent is more than six hours then there might be some lee way
                        //&& sleepKvp.Value.Item2.TimelineSpan >= Utility.SleepSpan
                    {
                        TimeSpan sleepTimeSpanBound = sleepKvp.Value.Item2.TimelineSpan < Utility.SleepSpan ? sleepKvp.Value.Item2.TimelineSpan : Utility.SleepSpan;
                        TimeSpan leftOverpan = sleepTimeSpanBound - afterTimeLine.TimelineSpan;
                        afterTimeLine = new TimeLine(afterTimeLine.Start, afterTimeLine.End.Add(leftOverpan));
                    }


                    if (beforeTimeLine!=null && afterTimeLine != null)
                    {
                        if(beforeTimeLine.TimelineSpan >= afterTimeLine.TimelineSpan)
                        {
                            if(!Utility.tryPinSubEventsToEnd(orderedByStartSubEvents, dayOnlyTimeLine))
                            {
                                throw new Exception("this pinning should not fail when it is the max Available spot");
                            }
                            postSleepSubEvents.AddRange(orderedByStartSubEvents.Skip(sleepSubEventindex));
                            previousDaySleepSubeventIndex = sleepSubEventindex - 1;
                            postSleepTimeline = new TimeLine(beforeTimeLine.End, dayOnlyTimeLine.End);
                            sleepTimeLine = new TimeLine(beforeTimeLine.Start, beforeTimeLine.End);
                        }
                        else
                        {
                            if (!Utility.tryPinSubEventsToStart(orderedByStartSubEvents, dayOnlyTimeLine))
                            {
                                throw new Exception("this pinning should not fail when it is the max Available spot");
                            }
                            postSleepSubEvents.AddRange(orderedByStartSubEvents.Skip(sleepSubEventindex+1));
                            previousDaySleepSubeventIndex = sleepSubEventindex;
                            postSleepTimeline = new TimeLine(afterTimeLine.End, dayOnlyTimeLine.End);
                            sleepTimeLine = new TimeLine(afterTimeLine.Start, afterTimeLine.End);
                        }
                    } else if(beforeTimeLine!=null)
                    {
                        if (!Utility.tryPinSubEventsToEnd(orderedByStartSubEvents, dayOnlyTimeLine))
                        {
                            throw new Exception("this pinning should not fail when it is the max Available spot");
                        }
                        postSleepSubEvents.AddRange(orderedByStartSubEvents.Skip(sleepSubEventindex));
                        previousDaySleepSubeventIndex = sleepSubEventindex - 1;
                        postSleepTimeline = new TimeLine(beforeTimeLine.End, dayOnlyTimeLine.End);
                        sleepTimeLine = new TimeLine(beforeTimeLine.Start, beforeTimeLine.End);
                    }
                    else if (afterTimeLine != null)
                    {
                        if (!Utility.tryPinSubEventsToStart(orderedByStartSubEvents, dayOnlyTimeLine))
                        {
                            throw new Exception("this pinning should not fail when it is the max Available spot");
                        }
                        postSleepSubEvents.AddRange(orderedByStartSubEvents.Skip(sleepSubEventindex+1));
                        previousDaySleepSubeventIndex = sleepSubEventindex;
                        postSleepTimeline = new TimeLine(afterTimeLine.End, dayOnlyTimeLine.End);
                        sleepTimeLine = new TimeLine(afterTimeLine.Start, afterTimeLine.End);
                    } else
                    {
                        postSleepSubEvents = orderedByStartSubEvents.ToList();
                    }

                    if(previousDaySleepSubeventIndex >= 0)// if there is going to be a sleepEvent for he previous day
                    {
                        SleepOfPreviousDay = orderedByStartSubEvents[previousDaySleepSubeventIndex];
                        List<SubCalendarEvent> subEventsBeforeSleep = orderedByStartSubEvents.Take(previousDaySleepSubeventIndex + 1).ToList();
                        if(!Utility.tryPinSubEventsToStart(subEventsBeforeSleep, dayOnlyTimeLine))
                        {
                            throw new Exception("There is a wrong misconception in your algorithm jay");
                        }
                    }
                }
                else
                {
                    postSleepSubEvents = orderedByStartSubEvents.ToList();
                }

                #region tryToFindBetterTimeFrameForSleep
                /// This region tries to find a different postSleepTimeline that can accomodate six hours of sleep. It pins all post events to the end and then sees if we sumed all the timespan of the buffers we then and tacked it on to the beginneing , can we still get enough for six hours of sleep
                if(!Utility.tryPinSubEventsToEnd(postSleepSubEvents, postSleepTimeline))// Trying to see if we can ensure more than 6 hours of sleep
                {
                    throw new Exception("Post sleep should be pinnable to end");
                }
                TimeSpan totalBufferSpan = new TimeSpan();
                foreach(var kvp in subEventToBuffer)
                {
                    if(kvp.Value.Item2 != Utility.NegativeTimeSpan)
                    {
                        totalBufferSpan += kvp.Value.Item2;
                    }
                    else
                    {
                        totalBufferSpan += new TimeSpan();
                    }
                }

                SubCalendarEvent firstSubCalendarEvent = postSleepSubEvents.FirstOrDefault();
                if(firstSubCalendarEvent!=null && sleepTimeLine!=null)
                {
                    DateTimeOffset postSleepTimelineStart = firstSubCalendarEvent.Start.Add(-totalBufferSpan);
                    DateTimeOffset sixHourTimeSpanEmd = sleepTimeLine.Start.Add(Utility.SixHourTimeSpan);
                    if (sleepTimeLine.TimelineSpan < Utility.SixHourTimeSpan && sixHourTimeSpanEmd <= postSleepTimelineStart)
                    {
                        postSleepTimeline = new TimeLine(sixHourTimeSpanEmd, postSleepTimeline.End);
                    }
                }
                #endregion

                #region try to add additional space for current location and first post sleep subevent
                if (dayTimeline.BeginLocation != null && dayTimeline.BeginLocation.isNotNullAndNotDefault)
                {
                    SubCalendarEvent firstPostSleepSubEvent = postSleepSubEvents.FirstOrDefault();
                    if(firstPostSleepSubEvent!=null)
                    {
                        TimeSpan travelSpan = getTravelSpan(dayTimeline.BeginLocation, firstPostSleepSubEvent.Location);
                        TimeLine newPostTimeLine = new TimeLine(postSleepTimeline.Start.Add(travelSpan), postSleepTimeline.End);
                        if (Utility.tryPinSubEventsToStart(postSleepSubEvents, newPostTimeLine))
                        {
                            postSleepTimeline = newPostTimeLine;
                        }
                        else
                        {
                            firstPostSleepSubEvent.setAsTardy();
                        }
                    }
                        
                }
                #endregion

                #region try to add additional space for morning preparation
                if (sleepTimeLine!=null && sleepTimeLine.TimelineSpan >= Utility.SleepSpan)
                {
                    if(postSleepSubEvents.Count > 0)
                    {
                        SubCalendarEvent firstPostSleepSubEvent = postSleepSubEvents.First();
                        TimeLine newPostTimeLine = new TimeLine(postSleepTimeline.Start.Add(MorningPreparationTime), postSleepTimeline.End);
                        if (Utility.tryPinSubEventsToStart(postSleepSubEvents, newPostTimeLine))
                        {
                            postSleepTimeline = newPostTimeLine;
                        }
                    }
                    
                }
                #endregion

                if (!Utility.PinSubEventsToStart(postSleepSubEvents, postSleepTimeline))
                {
                    throw new Exception("Post sleep should be pinnable");
                }

                if (postSleepSubEvents.Count > 0)
                {
                    wakeSubEvent = postSleepSubEvents.First();
                    SleepSubEvent = postSleepSubEvents.Last();
                }
                

                HashSet<SubCalendarEvent> allReadyBeforeBumped = new HashSet<SubCalendarEvent>();
                HashSet<SubCalendarEvent> allReadyAfterBumped = new HashSet<SubCalendarEvent>();
                Func<SubCalendarEvent, SubCalendarEvent> getPrecedingSubEvent = (subEvent) =>
                {
                    int index = subEventToIndex[subEvent];
                    if(index >0)
                    {
                        return orderedByStartSubEvents[index - 1];
                    }
                    return null;
                };

                Func<SubCalendarEvent, SubCalendarEvent> getSubsequentSubEvent = (subEvent) =>
                {
                    int index = subEventToIndex[subEvent];
                    if (index < orderedByStartSubEvents.Count - 1)
                    {
                        return orderedByStartSubEvents[index + 1];
                    }
                    return null;
                };

                Func<List<SubCalendarEvent>, SubCalendarEvent, TimeLine, bool> pinToEnd = (subsequentSubevents, subEvent, initialTimeLine) =>
                {
                    Dictionary<SubCalendarEvent, TimeLine> subEventsToInitialSlot = subsequentSubevents.ToDictionary(sub => sub, sub => sub.ActiveSlot.StartToEnd);
                    subEventsToInitialSlot.Add(subEvent, subEvent.ActiveSlot.StartToEnd);
                    var buffer = subEventToBuffer[subEvent];
                    SubCalendarEvent precedingSubEvent = getPrecedingSubEvent(subEvent);
                    DateTimeOffset startTime;
                    if (precedingSubEvent != null)
                    {
                        TimeSpan currentBufferSpan = initialTimeLine.Start - precedingSubEvent.End ;
                        bool isNotSufficientBuffering = currentBufferSpan.Ticks <= buffer.Item1.Ticks;
                        if (isNotSufficientBuffering)
                        {
                            TimeSpan beforeTimeSpan = buffer.Item1;
                            beforeTimeSpan = beforeTimeSpan == Utility.NegativeTimeSpan ? Utility.ZeroTimeSpan : beforeTimeSpan;
                            startTime = precedingSubEvent.End + beforeTimeSpan;
                        }
                        else
                        {
                            startTime = initialTimeLine.Start;
                        }
                    } else
                    {
                        startTime = initialTimeLine.Start;
                    }


                    TimeLine timeLine = new TimeLine(startTime, dayOnlyTimeLine.End);
                    bool allIsPinned = true && Utility.tryPinSubEventsToStart(new List<SubCalendarEvent>() { subEvent}, timeLine);
                    SubCalendarEvent previousSubEvent = subEvent;
                    foreach (SubCalendarEvent eachSubEvent in subsequentSubevents)
                    {
                        if(allIsPinned)
                        {
                            if (allReadyBeforeBumped.Contains(eachSubEvent))
                            {
                                TimeSpan beforeTimeSpan = subEventToBuffer[eachSubEvent].Item1;
                                beforeTimeSpan = beforeTimeSpan == Utility.NegativeTimeSpan ? Utility.ZeroTimeSpan : beforeTimeSpan;
                                timeLine = new TimeLine(previousSubEvent.End + beforeTimeSpan, dayOnlyTimeLine.End);
                                if(eachSubEvent.Start >= timeLine.Start)// subsequent subevent is already later than the previous subevent + its buffer span
                                {
                                    break;
                                }
                            }
                            else
                            {
                                timeLine = new TimeLine(previousSubEvent.End, dayOnlyTimeLine.End);
                            }
                            bool pinResult = eachSubEvent.PinToStart(timeLine);
                            allIsPinned &= pinResult;
                        }
                        else
                        {
                            break;
                        }
                        previousSubEvent = eachSubEvent;
                    }

                    if (!allIsPinned)
                    {
                        foreach (var subEventAndOldTime in subEventsToInitialSlot)
                        {
                            subEventAndOldTime.Key.updateTimeLine(subEventAndOldTime.Value);
                        }
                    } else if (allIsPinned) {
                        allReadyBeforeBumped.Add(subEvent);
                    }

                    return allIsPinned;
                };
                List<SubCalendarEvent>orderedByScorePostSleepSubEVents = postSleepSubEvents.OrderBy(o => o.Score).ToList();
                foreach (SubCalendarEvent subEvent in orderedByScorePostSleepSubEVents)
                {
                    DaySection daySection = DaySection.None;
                    if(subEvent.getDayPreference()!=null)
                    {
                        daySection = subEvent.getCurrentDaySection();
                    }
                    
                    var beforeAndAfter = subEventssToTimelines[subEvent];
                    DateTimeOffset start = beforeAndAfter.Item1.Start > subEvent.Start ? beforeAndAfter.Item1.Start : subEvent.Start;
                    TimeLine BoundTimeLine = new TimeLine(start, beforeAndAfter.Item2.End);
                    if (daySection != DaySection.None && daySection != DaySection.Disabled)
                    {
                        if(splitIntoDaySection.ContainsKey(daySection))
                        {
                            TimeLine DaySectionTimeLine = splitIntoDaySection[daySection];
                            DateTimeOffset timeLineStart = DaySectionTimeLine.Start - (subEvent.getActiveDuration - Utility.OneMinuteTimeSpan);// we want the lowest start that still encompasses the time Day section. So think if day section starts from 12:00pm -4:00pm and the subevent has duration of we want a time 2 hours we want the start time for the subevent to be possibly 10:01Am - 12:01pm  since it still part of the afternoon timeframe
                            DateTimeOffset timeLineEnd = DaySectionTimeLine.End + (subEvent.getActiveDuration - Utility.OneMinuteTimeSpan); // See comment for "timeLineStart" this only applies to the end

                            TimeLine timeLine = new TimeLine(timeLineStart, timeLineEnd)
                                .InterferringTimeLine(BoundTimeLine)?
                                .InterferringTimeLine(postSleepTimeline);
                            if (timeLine != null)
                            {
                                if (subEvent.canExistWithinTimeLine(timeLine))
                                {
                                    BoundTimeLine = timeLine;
                                }
                            }
                        }
                        
                    }

                    int index = subEventToIndex[subEvent]; 
                    int beginIndex = index + 1;
                    List<SubCalendarEvent> subSequentSubEvents = new List<SubCalendarEvent>();
                    if(beginIndex < orderedByStartSubEvents.Count && beginIndex >=0 )
                    {
                        subSequentSubEvents = orderedByStartSubEvents.GetRange(beginIndex, orderedByStartSubEvents.Count - beginIndex);
                    }

                    bool pinResult = pinToEnd(subSequentSubEvents, subEvent, BoundTimeLine);
                    if(!pinResult)
                    {
                        subEvent.setAsTardy();
                    }
                }
            }
            Tuple<SubCalendarEvent, SubCalendarEvent, SubCalendarEvent> retValue = new Tuple<SubCalendarEvent, SubCalendarEvent, SubCalendarEvent>(wakeSubEvent, SleepSubEvent, SleepOfPreviousDay);
            return retValue;
        }

        TimeSpan getTravelSpan (Location firstLocation, Location secondLocation, TravelMedium medium = TravelMedium.driving, bool ignoreCache = false)
        {
            TimeSpan retValue;
            if(!ignoreCache)
            {
                var cacheEntry = TravelCache.getLocationCacheEntry(firstLocation, secondLocation, Now.constNow, medium);
                if(cacheEntry!=null)
                {
                    retValue = cacheEntry.TimeSpan;
                }
                else
                {
                    retValue = Location.getDrivingTimeFromWeb(firstLocation, secondLocation).removeSecondsAndMilliseconds();
                }
            }
            else
            {
                retValue = Location.getDrivingTimeFromWeb(firstLocation, secondLocation).removeSecondsAndMilliseconds();
            }

            return retValue;
        }

        /// <summary>
        /// To be used with multidimensional calculation for sleep.
        /// We need to get the largest sleep span possible with the event with earliest possible start time
        /// </summary>
        /// <param name="endOfDay">The end of the day of a given time frame</param>
        /// <param name="timeLine"></param>
        /// <returns></returns>
        List<double> getTimeLineSpaceAndTimeLineStart(DateTimeOffset endOfDay, TimeLine timeLine)
        {
            double span = timeLine.TimelineSpan.TotalHours;// Wider the span the more likly it can allow more 'sleep-time'
            double startTIme = (endOfDay - timeLine.Start).TotalHours;/// the larger the span the earlier the event can possibly be
            List<double> retValue = new List<double>() { span, startTIme };
            return retValue;
        }

        ulong ParallelizeCallsToDay(List<CalendarEvent> AllCalEvents, List<SubCalendarEvent> TotalActiveEvents, DayTimeLine[] AllDayTimeLine, Location callLocation, bool Optimize = true, bool preserveFirttwentyFourHours = true, bool shuffle = false)
        {
            _isScheduleModified = true;
            uint TotalDays = (uint)AllDayTimeLine.Length;
            long DayIndex = AllDayTimeLine.First().UniversalIndex;
            double occupancyThreshold = 0.67;// this placies a soft threshold for the occupancy that different cal events would use to determine if they should continue
            EventDayBags bagsPerDay = new EventDayBags(TotalDays);
            foreach (SubCalendarEvent subEvent in TotalActiveEvents)
            {
                subEvent.isWake = false; subEvent.isSleep = false;
                subEvent.resetTardy();
            }
            TotalActiveEvents.ForEach((subEvent) => _ConflictingSubEvents.Add(subEvent));
            _EvaluatedSubevents = new HashSet<SubCalendarEvent>(TotalActiveEvents);
            AllCalEvents.ForEach
                //.AsParallel().ForAll
                (obj => {
                    obj.resetDesignationAllActiveEventsInCalculables();
                    obj.InitialCalculationLookupDays(AllDayTimeLine, this.Now);
                    obj.updateCompletionTimeArray(Now);
                    obj.resetAutoDeadlineSuggestion();
                });
            foreach (SubCalendarEvent subEvent in TotalActiveEvents)
            {
                subEvent.enableCalculationMode();
            }
            #region presetupAllRigids_Ensures_They_Are_All_Assigned
            List<SubCalendarEvent> AllRigids = TotalActiveEvents.Where(obj => obj.isLocked).ToList();// you need to call this after PrepFirstTwentyFOurHours to prevent resetting of indexes
            var rigidToDayMapping = DesignateSubEventsToDayTimeLine(AllDayTimeLine, AllRigids);
            foreach(var dayTimeLine in AllDayTimeLine)
            {
                var rigidsInDay = dayTimeLine.getSubEventsInTimeLine().Where(sub => sub.isLocked);
                foreach(var subEvent in rigidsInDay)
                {
                    TimeLine timeLine = subEvent.ActiveSlot.InterferringTimeLine(dayTimeLine);
                    if (timeLine != null)
                    {
                        bagsPerDay[dayTimeLine.BoundedIndex].addSubEvent(subEvent, timeLine.TimelineSpan);
                    }
                    
                }
            }
            #endregion

            #region configures firstyTwentyfour hours ensures they are all assigned
            bool runPreseveFirstTwentyFour = preserveFirttwentyFourHours && !shuffle;
            if (runPreseveFirstTwentyFour)
            {
                int dayIndex = 0;
                DayTimeLine dayTimeLine = AllDayTimeLine[dayIndex];
                var SetForFirstDay = PrepFirstTwentyFourHours(TotalActiveEvents, dayTimeLine.StartToEnd);
                SetForFirstDay.ForEach(subEvent => {
                    subEvent.ParentCalendarEvent.designateSubEvent(subEvent, Now);
                    this._ConflictingSubEvents.Remove(subEvent);
                    TimeLine timeLine = subEvent.ActiveSlot.InterferringTimeLine(dayTimeLine);
                    if (timeLine != null)
                    {
                        bagsPerDay[dayTimeLine.BoundedIndex].addSubEvent(subEvent, timeLine.TimelineSpan);
                    }
                });
                AllDayTimeLine[0].AddToSubEventList(SetForFirstDay);
            }
            #endregion


            int numberOfDays = AllDayTimeLine.Count();
            AllCalEvents.ForEach(obj => obj.initializeCalculablesAndUndesignables());
            AllCalEvents.ForEach(obj => obj.DayPreference?.init());
            Dictionary<string, CalendarEvent> DictOfCalEvents = AllCalEvents.ToDictionary(obj => obj.getId, obj => obj);
            Dictionary<string, SubCalendarEvent> DictOfSubEvents = TotalActiveEvents.ToDictionary(obj => obj.getId, obj => obj);

            Parallel.ForEach(AllDayTimeLine, (obj, state, index) => {
                obj.updateOccupancyOfTimeLine();
            });

            ulong totalDaysAvailable = (ulong)CalendarEvent.getUsableDaysTotal(AllCalEvents);
            ulong totalNumberOfEvents = (ulong)CalendarEvent.getTotalUndesignatedEvents(AllCalEvents);
            

            List<IList<double>> allCalEventsFeatureCalibrations = generateMultiDimensionalParams(AllCalEvents);
            List<double> scores = Utility.multiDimensionCalculationNormalize(allCalEventsFeatureCalibrations);
            List<Tuple<double, CalendarEvent>> scoreAndCalEvent = scores.Select(
                (score, i) =>
                {
                    return new Tuple<double, CalendarEvent>(score, AllCalEvents[i]);
                }
                ).ToList();

            AllCalEvents = scoreAndCalEvent.OrderBy(calEvent => calEvent.Item1).Select(calEvent => calEvent.Item2).ToList();
            var allCalEventCall  = new Stopwatch();
            allCalEventCall.Start();
            while ((totalDaysAvailable > 0) && (totalNumberOfEvents > 0))
            {
                long OldNumberOfAssignedElements = -1;
                long DesignatedAndAssignedSubEventCount = -1;
                do
                {
                    ConcurrentBag<CalendarEvent> UnUsableCalEvents = new ConcurrentBag<CalendarEvent>();
                    bagsPerDay.removeAllUndesignated();// This needs to be called to ensure that the bags have only undesignateed events in their colelction for next iterations
                    OldNumberOfAssignedElements = DesignatedAndAssignedSubEventCount;

                    foreach (CalendarEvent eachCal in AllCalEvents)
                    {
                        eachCal.updateUnusableDaysAndRemoveDaysWithInsufficientFreeSpace();
                        List<DayTimeLine> DaysToUse;
                        List<SubCalendarEvent> UndesignatedEvents = eachCal.AllUnDesignatedAndActiveSubEventsFromCalculables();
                        List<DayTimeLine> WorksWithProcrastination = eachCal.getDaysOnOrAfterProcrastination(Now, true);
                        List<DayTimeLine> CurrDaysToUse;

                        if (WorksWithProcrastination.Count > 0)
                        {
                            CurrDaysToUse = WorksWithProcrastination;
                            DaysToUse = CurrDaysToUse;
                            CurrDaysToUse = eachCal.getTimeLineWithEnoughDuration(true, CurrDaysToUse);

                            List<DayTimeLine> UnWantedDays = DaysToUse.Except(CurrDaysToUse).ToList();
                            eachCal.updateUnusableDaysAndRemoveDaysWithInsufficientFreeSpace(UnWantedDays.Select(obj => obj.UniversalIndex));
                            DaysToUse = CurrDaysToUse;
                            if (CurrDaysToUse.Count > UndesignatedEvents.Count)
                            {
                                CurrDaysToUse = eachCal.getDaysOnOrAfterProcrastination(Now, false, CurrDaysToUse);
                                if (CurrDaysToUse.Count > UndesignatedEvents.Count)
                                {
                                    DaysToUse = CurrDaysToUse;
                                    CurrDaysToUse = eachCal.getTimeLineWithoutMySubEventsAndEnoughDuration(false, CurrDaysToUse);
                                    if (CurrDaysToUse.Count > UndesignatedEvents.Count)
                                    {
                                        DaysToUse = CurrDaysToUse;
                                        CurrDaysToUse = eachCal.getDayTimeLineWhereOccupancyIsLess(occupancyThreshold, false, CurrDaysToUse);
                                        if (CurrDaysToUse.Count > UndesignatedEvents.Count)
                                        {
                                            DaysToUse = CurrDaysToUse;
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            CurrDaysToUse = eachCal.getTimeLineWithEnoughDuration(true);
                            DaysToUse = CurrDaysToUse;
                            if (CurrDaysToUse.Count > UndesignatedEvents.Count)
                            {
                                CurrDaysToUse = eachCal.getDaysOnOrAfterProcrastination(Now, false);
                                if (CurrDaysToUse.Count > UndesignatedEvents.Count)
                                {
                                    DaysToUse = CurrDaysToUse;
                                    CurrDaysToUse = eachCal.getTimeLineWithoutMySubEventsAndEnoughDuration(false);
                                    if (CurrDaysToUse.Count > UndesignatedEvents.Count)
                                    {
                                        DaysToUse = CurrDaysToUse;
                                        CurrDaysToUse = eachCal.getDayTimeLineWhereOccupancyIsLess(occupancyThreshold, false);
                                        if (CurrDaysToUse.Count > UndesignatedEvents.Count)
                                        {
                                            DaysToUse = CurrDaysToUse;
                                        }
                                    }

                                }
                            }
                        }

                        if (DaysToUse.Count > 0)
                        {
                            long preferredIndex = DayIndex;
                            List<Tuple<long, SubCalendarEvent>> AllEvents = EvaluateEachDayIndexForEvent(UndesignatedEvents, DaysToUse, eachCal, bagsPerDay, preferredIndex);
                            if (AllEvents.Count != 0)
                            {
                                for(int i =0; i< AllEvents.Count; i++)
                                //Parallel.ForEach(AllEvents, eachTuple =>
                                {
                                    var eachTuple = AllEvents[i];
                                    bagsPerDay[(int)(eachTuple.Item1 - DayIndex)].addSubEvent(eachTuple.Item2, eachTuple.Item2.getActiveDuration);
                                }
                                //);
                            }
                            else
                            {
                                UnUsableCalEvents.Add(eachCal);
                            }
                        }
                        else
                        {
                            UnUsableCalEvents.Add(eachCal);
                        }
                    }
                    AllCalEvents = AllCalEvents.Except(UnUsableCalEvents).ToList();


                    for (int i = 0; i < numberOfDays; i++)
                    {
                        List<SubCalendarEvent> subEvents = bagsPerDay[i].getNewlyAddedSubEvents().Select(obj => obj).ToList();
                        List<SubCalendarEvent> newSubEventAdditions = processTwentyFourHours(AllDayTimeLine[i], subEvents);
                        foreach (SubCalendarEvent eachSucal in newSubEventAdditions)
                        {
                            DictOfCalEvents[eachSucal.SubEvent_ID.getRepeatCalendarEventID()].removeDayTimeFromFreeUpdays(eachSucal.UniversalDayIndex);
                        }
                    }

                    foreach(DayTimeLine dayTimeline in AllDayTimeLine)
                    {
                        dayTimeline.updateOccupancyOfTimeLine();
                    }
                    DesignatedAndAssignedSubEventCount = AllCalEvents.Sum(obj => obj.getNumberOfDesignatedAndActiveSubEventsFromCalculables());
                    AllCalEvents = CalendarEvent.removeCalEventsWitNoUndesignablesFromCalculables(AllCalEvents);
                }
                while (DesignatedAndAssignedSubEventCount != OldNumberOfAssignedElements);

                totalDaysAvailable = (ulong)CalendarEvent.getUsableDaysTotal(AllCalEvents);
                totalNumberOfEvents = (ulong)CalendarEvent.getTotalUndesignatedEvents(AllCalEvents);
            }
            allCalEventCall.Stop();
            Debug.WriteLine("allCalEventCall pull took" + allCalEventCall.Elapsed.ToString());


            List<SubCalendarEvent> orderedByStart = TotalActiveEvents.OrderBy(obj => obj.Start).ToList();

            HashSet<SubCalendarEvent> designatedSubEvents = new HashSet<SubCalendarEvent>(TotalActiveEvents.Where(o=>o.isDesignated));
            this._ConflictingSubEvents.RemoveWhere(subEvent => designatedSubEvents.Contains(subEvent));
            List<SubCalendarEvent> conflictingSubEvents = this._ConflictingSubEvents.OrderBy(o=>o.Score).ToList();

            if (isConflictResolveEnabled)
            {
                var conflictResolutionResult = resolveConflicts(conflictingSubEvents, AllDayTimeLine);

                List<SubCalendarEvent> unresolvedConflicts = conflictResolutionResult.Item2.Where(subEvent => subEvent.Start >= Now.calculationNow).ToList();
                unresolvedConflicts.ForEach((subEvent) =>
                {
                    DayTimeLine dayTimeLine = Now.getDayTimeLineByTime(subEvent.Start);
                    bool conflictResolve = singleDayConflictResolution(subEvent, dayTimeLine);
                    if (conflictResolve)
                    {
                        _ConflictingSubEvents.Remove(subEvent);
                    }
                });
            }
            



            List<DayTimeLine> OptimizedDays = new List<DayTimeLine>();
            var allOptimizationWatch = new Stopwatch();
            IDictionary<DayTimeLine, OptimizedPath> dayToOptimization = null;
            allOptimizationWatch.Start();
            if (Optimize && this.OptimizationEnabled)
            {
                HashSet<SubCalendarEvent> undesignatedAfterSubeventsOptimization = new HashSet<SubCalendarEvent>();
                OptimizedDays = AllDayTimeLine.Take(OptimizedDayLimit).ToList();
                long FirstIndex = AllDayTimeLine[0].UniversalIndex;
                try
                {
                    ILookup<long, SubCalendarEvent> DayToSubEvent = AllRigids.ToLookup(obj => obj.UniversalDayIndex, obj => obj);
                    foreach (IGrouping<long, SubCalendarEvent> eachGrouping in DayToSubEvent)
                    {
                        IEnumerable<SubCalendarEvent> subEvents = DayToSubEvent[eachGrouping.Key];
                        foreach (SubCalendarEvent subEvent in subEvents)
                        {
                            _ConflictingSubEvents.Remove(subEvent);
                        }
                        int currentIndex = (int)(eachGrouping.Key - FirstIndex);
                        AllDayTimeLine[currentIndex].AddToSubEventList(DayToSubEvent[eachGrouping.Key]);
                    }

                    dayToOptimization = optimizeDays(OptimizedDays, callLocation);

                    foreach(var kvp in dayToOptimization)
                    {
                        var optimizedPath = kvp.Value;
                        DayTimeLine EachDay = kvp.Key;
                        foreach (SubCalendarEvent subEvent in optimizedPath.UnassignedSubEvents)
                        {
                            undesignatedAfterSubeventsOptimization.Add(subEvent);
                            subEvent.ParentCalendarEvent.undesignateSubEvent(subEvent);
                        }
                    }
                    foreach (SubCalendarEvent subEvent in OptimizedDays.SelectMany(day => day.getSubEventsInTimeLine()))
                    {
                        _ConflictingSubEvents.Remove(subEvent);
                    }


                    if(isConflictResolveEnabled)
                    {
                        var optimizationConflictResolutionResult = resolveConflicts(undesignatedAfterSubeventsOptimization.ToList(), AllDayTimeLine);
                        foreach (SubCalendarEvent subEvent in optimizationConflictResolutionResult.Item2)
                        {
                            _ConflictingSubEvents.Add(subEvent);
                        }
                        foreach (SubCalendarEvent subEvent in optimizationConflictResolutionResult.Item1)
                        {
                            subEvent.setAsOptimized();
                        }
                    }
                }
                catch (Exception E)
                {
                    throw E;
                }
            }

            
            readjustSleepSchedule(AllDayTimeLine);
            if(dayToOptimization!=null)
            {
                var dayToOptimizationAsList = dayToOptimization.OrderBy(o => o.Key.UniversalIndex).ToList();

                for (int i = 0; i< dayToOptimizationAsList.Count; i++)
                {
                    var kvp = dayToOptimizationAsList[i];
                    var dayPath = kvp.Value;
                    var EachDay = kvp.Key;
                    Dictionary<string, SubCalendarEvent> slicedSubEvents = new Dictionary<string, SubCalendarEvent>();
                    List<SubCalendarEvent> subEvents = EachDay.getSubEventsInTimeLine();
                    List<SubCalendarEvent> spaceSubEvents = new List<SubCalendarEvent>();
                    foreach(SubCalendarEvent eachSubEvent in subEvents)// loop handles scenario where a rigid event cuts across multiple days
                    {
                        SubCalendarEvent subEvent = eachSubEvent;
                        if (subEvent.isLocked && !subEvent.canExistWithinTimeLine(EachDay))
                        {
                            TimeLine interferringTimeline = subEvent.StartToEnd.InterferringTimeLine(EachDay);
                            if(interferringTimeline != null)
                            {
                                SubCalendarEvent slicedValidSubEvent = new SubCalendarEvent(subEvent.ParentCalendarEvent, TilerUser.autoUser, new TilerUserGroup(), subEvent.getTimeZone, subEvent.Id, subEvent.getName, interferringTimeline.Start, interferringTimeline.End, new BusyTimeLine(subEvent.Id, interferringTimeline.Start, interferringTimeline.End), subEvent.isRigid, subEvent.isEnabled, subEvent.getUIParam, subEvent.Notes, subEvent.getIsComplete, subEvent.Location, subEvent.getCalendarEventRange, subEvent.Conflicts);
                                subEvent = slicedValidSubEvent;
                                slicedSubEvents.Add(slicedValidSubEvent.Id, subEvent);
                                spaceSubEvents.Add(subEvent);
                            }
                            
                        } else
                        {
                            spaceSubEvents.Add(subEvent);
                        }
                        
                    }

                    var spaceByTime = new Stopwatch();
                    spaceByTime.Start();
                    Tuple<SubCalendarEvent, SubCalendarEvent, SubCalendarEvent> wakeAndSleepEvents = spaceEventsByTravelTime(EachDay, spaceSubEvents);
                    spaceByTime.Stop();
                    Debug.WriteLine("each space by time" + spaceByTime.Elapsed.ToString());

                    if (wakeAndSleepEvents.Item1 != null)
                    {
                        SubCalendarEvent subEvent = slicedSubEvents.ContainsKey(wakeAndSleepEvents.Item1.Id) ? slicedSubEvents[wakeAndSleepEvents.Item1.Id] : wakeAndSleepEvents.Item1;
                        EachDay.WakeSubEvent = subEvent;
                    }

                    if (wakeAndSleepEvents.Item2 != null)
                    {
                        SubCalendarEvent subEvent = slicedSubEvents.ContainsKey(wakeAndSleepEvents.Item2.Id) ? slicedSubEvents[wakeAndSleepEvents.Item2.Id] : wakeAndSleepEvents.Item2;
                        EachDay.SleepSubEvent = subEvent;
                    }

                    if (wakeAndSleepEvents.Item3 != null)
                    {
                        SubCalendarEvent subEvent = slicedSubEvents.ContainsKey(wakeAndSleepEvents.Item3.Id) ? slicedSubEvents[wakeAndSleepEvents.Item3.Id] : wakeAndSleepEvents.Item3;
                        EachDay.PrecedingDaySleepSubEvent = subEvent;
                    }


                    if (i > 0 && EachDay.PrecedingDaySleepSubEvent != null)
                    {
                        DayTimeLine previousDay = AllDayTimeLine[i - 1];
                        previousDay.SleepSubEvent = EachDay.PrecedingDaySleepSubEvent;
                    }
                }
            }

            List<DayTimeLine> moveToMiddleDays = AllDayTimeLine.Skip(OptimizedDayLimit).ToList();
            Debug.WriteLine("Total optimization took" + allOptimizationWatch.Elapsed.ToString());
            foreach (DayTimeLine dayTimeLine in moveToMiddleDays)
            {
                tryToCentralizeSubEvents(dayTimeLine);
            }

            List<BlobSubCalendarEvent> afterPathOptimizationConflictingEvetns = Utility.getConflictingEvents(TotalActiveEvents.OrderBy(obj => obj.Start).ToList()).Item1;
            List<SubCalendarEvent> ordereByStartTime = TotalActiveEvents.OrderBy(SubEvent => SubEvent.Start).ToList();
            return totalNumberOfEvents;
        }

        /// <summary>
        /// This function tries to ensure the subevents are reorganized to be within the active time frame of timeline. The active time frame is defined as the latter 2/3rds of <paramref name="timeline"/>
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="subEvents"></param>
        void readjustSleepSchedule(TimeLine timeline, IEnumerable<SubCalendarEvent> subEvents)
        {
            List<SubCalendarEvent> orderedsubEventsInTimeline = subEvents.OrderBy(o => o.Start).ThenBy(o => o.End).ToList();

            if (orderedsubEventsInTimeline.Count > 0)
            {
                if (orderedsubEventsInTimeline == null)
                {
                    orderedsubEventsInTimeline = new List<SubCalendarEvent>();
                }

                SubCalendarEvent firstSubevent = orderedsubEventsInTimeline.FirstOrDefault();
                SubCalendarEvent lastSubevent = orderedsubEventsInTimeline.LastOrDefault();
                DateTimeOffset timeLineStart = timeline.Start;
                if (firstSubevent != null)
                {
                    timeLineStart = firstSubevent.Start <= timeline.Start ? firstSubevent.Start : timeline.Start;
                }

                DateTimeOffset timeLineEnd = timeline.End;
                if (lastSubevent != null)
                {
                    timeLineEnd = lastSubevent.End >= timeline.End ? lastSubevent.End : timeline.End;
                }
                TimeLine revisedTimeLineUpdatedForOverlappingEvents = new TimeLine(timeLineStart, timeLineEnd);// this ensures the subEvents are verified to fit within the timeline. This handles cases where an event might cross over say a rigid whih belongs to multiple days

                TimeSpan activeTimeSpan = Utility.TwentyFourHoursAlmostTimeSpan - TimeSpan.FromTicks(Utility.TwentyFourHoursAlmostTimeSpan.Ticks / 3);
                if (revisedTimeLineUpdatedForOverlappingEvents.TimelineSpan >= activeTimeSpan)
                {
                    if (!Utility.PinSubEventsToEnd(orderedsubEventsInTimeline, revisedTimeLineUpdatedForOverlappingEvents))
                    {
                        throw new Exception("Bug in optimize days and conflict resolution");
                    }

                    TimeLine activeTimeline = new TimeLine(revisedTimeLineUpdatedForOverlappingEvents.End.Subtract(activeTimeSpan), revisedTimeLineUpdatedForOverlappingEvents.End);
                    TimeLine sleepTimeline = new TimeLine(revisedTimeLineUpdatedForOverlappingEvents.Start, activeTimeline.Start);
                    List<SubCalendarEvent> sleepSubevents = new List<SubCalendarEvent>();
                    List<SubCalendarEvent> activeTimeframeSubevents = new List<SubCalendarEvent>();
                    foreach (SubCalendarEvent subevent in orderedsubEventsInTimeline)
                    {
                        if (sleepTimeline.IsTimeLineWithin(subevent.StartToEnd))// we want locked events to be part of the active time frame events because they cannot be moved
                        {
                            sleepSubevents.Add(subevent);
                        }
                        else
                        {
                            activeTimeframeSubevents.Add(subevent);
                        }
                    }

                    SubCalendarEvent activeFirstSubevent = activeTimeframeSubevents.FirstOrDefault();
                    SubCalendarEvent activeLastSubevent = activeTimeframeSubevents.LastOrDefault();
                    DateTimeOffset activeTimeLineStart = activeTimeline.Start;
                    if (activeFirstSubevent != null)
                    {
                        activeTimeLineStart = activeFirstSubevent.Start <= activeTimeline.Start ? activeFirstSubevent.Start : activeTimeline.Start;
                    }

                    DateTimeOffset activeTimeLineEnd = activeTimeline.End;
                    if (activeLastSubevent != null)
                    {
                        activeTimeLineEnd = activeLastSubevent.End >= activeTimeline.End ? activeLastSubevent.End : activeTimeline.End;
                    }


                    TimeLine revisedActivetimeline = new TimeLine(activeTimeLineStart, activeTimeLineEnd);
                    if (sleepSubevents.Count > 0)
                    {
                        var newlyaddedMovedSleepevents = adjustEachSleepDay(revisedActivetimeline, sleepSubevents, activeTimeframeSubevents);
                        if(newlyaddedMovedSleepevents!=null)
                        {
                            HashSet<SubCalendarEvent> noLongerSleep = newlyaddedMovedSleepevents.Item1;
                            List<SubCalendarEvent> currentSleep = sleepSubevents.Where(o => !noLongerSleep.Contains(o)).ToList();
                            List<SubCalendarEvent> newOrderOfSubEvents = currentSleep.Concat(newlyaddedMovedSleepevents.Item2).ToList();
                            if(!Utility.PinSubEventsToEnd(newOrderOfSubEvents, revisedTimeLineUpdatedForOverlappingEvents))
                            {
                                throw new Exception("There's an issue with optimization for sleep schedule");
                            }
                        }
                        
                    }
                }
            }
        }


        /// <summary>
        /// Funtion readjst each day and moves subevents withing the sleep time frame towards the active timeframe
        /// </summary>
        /// <param name="daytimeLines"></param>
        void readjustSleepSchedule(IEnumerable<DayTimeLine> daytimeLines)
        {
            foreach(DayTimeLine dayTimeline in daytimeLines)
            {
                List<SubCalendarEvent> orderedsubEventsInTimeline = dayTimeline.getSubEventsInTimeLine().OrderBy(o => o.Start).ThenBy(o => o.End).ToList();
                readjustSleepSchedule(dayTimeline, orderedsubEventsInTimeline);
            }
        }

        /// <summary>
        /// funtion tries to find a spot for sleepsubevents within the active timeline. Note all the acive time framesubevents must all fit within active timeline
        /// Function returns a tuple
        /// Item1 is a HashSet of newly ressigned from the sleep time frame 
        /// Item2 is a list of new ordered activeTimeFrame events
        /// </summary>
        /// <param name="activeTimeline"></param>
        /// <param name="sleepTimeFrameSubevents"></param>
        /// <param name="activeTimeframeSubevents"></param>
        Tuple<HashSet<SubCalendarEvent>, List<SubCalendarEvent>> adjustEachSleepDay(TimeLine activeTimeline, IEnumerable<SubCalendarEvent> sleepTimeFrameSubevents, IEnumerable<SubCalendarEvent> activeTimeframeSubevents)
        {
            Tuple<HashSet<SubCalendarEvent>, List<SubCalendarEvent>> retValue = null;
            HashSet<SubCalendarEvent> reorderedFromSleep = new HashSet<SubCalendarEvent>();
            HashSet<SubCalendarEvent> activeSubevents = new HashSet<SubCalendarEvent>( activeTimeframeSubevents);
            List<SubCalendarEvent> repositionableSleepSubEvents = sleepTimeFrameSubevents.Where(o => !o.isLocked).ToList();
            const int maxBruteforceCount = 5;
            if(repositionableSleepSubEvents.Count>0)
            {
                if(repositionableSleepSubEvents.Count > maxBruteforceCount)
                {
                    List<SubCalendarEvent> reoptimizedSubevents = BuildAllPossibleSnugLists(repositionableSleepSubEvents, activeTimeline, 1, activeSubevents);
                    reorderedFromSleep = new HashSet<SubCalendarEvent>( reoptimizedSubevents.Where(o => !activeSubevents.Contains(o)));
                    retValue = new Tuple<HashSet<SubCalendarEvent>, List<SubCalendarEvent>>(reorderedFromSleep, reoptimizedSubevents);
                }
                else
                {
                    List<SubCalendarEvent> alreadyAssigned = activeSubevents.OrderBy(o => o.Start).ToList();
                    List<SubCalendarEvent> sortedByScore = sleepTimeFrameSubevents.OrderBy(sub => sub.Score).ToList();
                    foreach(SubCalendarEvent subEvent in sortedByScore)
                    {
                        int index = Utility.getBestPosition(activeTimeline, subEvent, alreadyAssigned);
                        if(index != -1 )
                        {
                            alreadyAssigned.Insert(index, subEvent);
                            if(!Utility.PinSubEventsToEnd(alreadyAssigned, activeTimeline))
                            {
                                throw new Exception("There is a bug in sleep evaluation");
                            }
                            reorderedFromSleep.Add(subEvent);
                        }
                    }
                    retValue = new Tuple<HashSet<SubCalendarEvent>, List<SubCalendarEvent>>(reorderedFromSleep, alreadyAssigned);
                }
            }
            return retValue;
        }

        /// <summary>
        /// Function tries to resolve conflicts by prioritizing the <paramref name="conflictingSubEvent"/>. It looks through subevents events in <paramref name="dayTimeline"/>
        /// and finds subevents within the available timeLine <paramref name="conflictingSubEvent"/> then removes them and tries scheduling <paramref name="conflictingSubEvent"/> then tries scheduling the initially removed subevents around this new priority.
        /// If its successfult at scheduling the removed subevents it reutrns a true else a false;
        /// </summary>
        /// <param name="conflictingSubEvent"></param>
        /// <param name="dayTimeline"></param>
        /// <returns></returns>
        bool singleDayConflictResolution(SubCalendarEvent conflictingSubEvent, DayTimeLine dayTimeline)
        {
            List<TimeLine> conflictingSubeventTimelines = conflictingSubEvent.ParentCalendarEvent.getInterferringWithTimeLine(dayTimeline).OrderBy(o => o.Start).ToList();
            List<SubCalendarEvent> subEventsInTimeline = new HashSet<SubCalendarEvent>(dayTimeline.getSubEventsInTimeLine()).OrderBy(o=>o.Start).ToList();
            HashSet<SubCalendarEvent> subEventsInConflictingTimeLine = new HashSet<SubCalendarEvent>();
            HashSet<SubCalendarEvent> preservedSubEventWithinTimeLine = new HashSet<SubCalendarEvent>(subEventsInTimeline);// THis will be the subevents in which their order need to be stuck in place since they are either not conflicting with timeframe for the conflicting subevent or they are rigid subevent
            int expectedSubEventCountAfterConflictResolution = preservedSubEventWithinTimeLine.Count + 1;
            int indexOfDaytimeline = 0;
            int nextIndex = indexOfDaytimeline;
            
            if (subEventsInTimeline!=null && conflictingSubeventTimelines!=null && subEventsInTimeline.Count > 0 && conflictingSubeventTimelines.Count > 0)
            {
                TimeLine firstTimeLine = conflictingSubeventTimelines[0];
                List<SubCalendarEvent> plausibleConflicts = subEventsInTimeline.Where(sub => sub.End > firstTimeLine.Start).ToList();
                bool foundPlausible = false;
                int subEventIndex = 0;
                for (; indexOfDaytimeline < conflictingSubeventTimelines.Count && subEventIndex < plausibleConflicts.Count;)
                {
                    TimeLine conflictingTimeline = conflictingSubeventTimelines[indexOfDaytimeline];
                    for (; subEventIndex < plausibleConflicts.Count; subEventIndex++)
                    {
                        SubCalendarEvent subEvent = plausibleConflicts[subEventIndex];
                        if (conflictingTimeline.doesTimeLineInterfere(subEvent.StartToEnd))
                        {
                            subEventsInConflictingTimeLine.Add(subEvent);
                            foundPlausible = true;
                        }
                        else
                        {
                            int nextTimeLineIndex = ++indexOfDaytimeline;
                            int nextSubEventIndex = ++subEventIndex;
                            foundPlausible = false;
                            if (nextTimeLineIndex < conflictingSubeventTimelines.Count) { 
                                TimeLine nextConflictingTimeline = conflictingSubeventTimelines[nextTimeLineIndex];
                                
                                for (; nextSubEventIndex < plausibleConflicts.Count; nextSubEventIndex++)
                                {
                                    SubCalendarEvent nextSubEvent = plausibleConflicts[nextSubEventIndex];
                                    if (nextSubEvent.End > nextConflictingTimeline.Start)
                                    {
                                        indexOfDaytimeline = nextTimeLineIndex;
                                        subEventIndex = nextSubEventIndex;
                                        foundPlausible = true;
                                        --subEventIndex;// extra decrement because of outer loop increment
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                indexOfDaytimeline = nextTimeLineIndex;
                            }
                            
                            if (!foundPlausible)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            List<SubCalendarEvent> toBeReorganizedForConflictSubevents = new List<SubCalendarEvent>();
            subEventsInTimeline.ForEach((subEvent) =>// this preserves the order for which the latter call to stitchUnRestrictedSubCalendarEvent will try its best effort around. preservedSubEventWithinTimeLine needs to include all locked events and events not within the calendarevent calculation timeframe of conflicting event. This is especially noteworthy for restricted subevents
            {
                if (subEventsInConflictingTimeLine.Contains(subEvent) && !subEvent.isLocked)//  
                {
                    toBeReorganizedForConflictSubevents.Add(subEvent);
                    preservedSubEventWithinTimeLine.Remove(subEvent);
                }
            });

            var preservedTimeLineSubevents = Utility.getTilerEventTimeline(subEventsInTimeline);
            bool conflictResolved = false;
            List<SubCalendarEvent> orderedSubevent = preservedSubEventWithinTimeLine.OrderBy(o => o.Start).ToList();
            if(conflictingSubEvent.canExistWithinTimeLine(dayTimeline))
            {
                var positionEvaluationResult = getPositionInDaytimeline(dayTimeline, conflictingSubEvent, orderedSubevent);

                int preferredIndex = positionEvaluationResult.Item1;
                if (preferredIndex > -1)
                {
                    List<SubCalendarEvent> reoptimizedSubEvent = orderedSubevent.ToList();
                    reoptimizedSubEvent.Insert(preferredIndex, conflictingSubEvent);
                    TimeLine timeline = positionEvaluationResult.Item2;
                    if (!Utility.PinSubEventsToStart(reoptimizedSubEvent, timeline))
                    {
                        throw new Exception("There is a flaw in your logic for conflict resoution");
                    }

                    List<SubCalendarEvent> allSubevents = new HashSet<SubCalendarEvent>(reoptimizedSubEvent.Concat(toBeReorganizedForConflictSubevents)).ToList();

                    List<SubCalendarEvent> stitchResult = stitchUnRestrictedSubCalendarEvent(timeline, reoptimizedSubEvent, toBeReorganizedForConflictSubevents);
                    
                    if (stitchResult.Count == expectedSubEventCountAfterConflictResolution)
                    {
                        conflictResolved = true;
                        conflictingSubEvent.ParentCalendarEvent.designateSubEvent(conflictingSubEvent, Now);
                        this._ConflictingSubEvents.Remove(conflictingSubEvent);
                        dayTimeline.AddToSubEventList(conflictingSubEvent);
                    }
                }
            }
            

            if(!conflictResolved)
            {
                foreach(var subevent_Initimeline_kvp in preservedTimeLineSubevents)
                {
                    subevent_Initimeline_kvp.Key.shiftEvent(subevent_Initimeline_kvp.Value.Start, true);
                }
            }

            return conflictResolved;
        }


        /// <summary>
        /// Function tries to get the best index position to insert a <paramref name="subEvent"/> that can works within <paramref name="dayTimeline"/>. By default it uses the Subevents assigned to <paramref name="dayTimeline"/>, and uses the order of the start time of the subevents with the daytimeline.
        /// If you want a different set of bounding subevents you can provide that in <paramref name="orderedsubEventsInTimeline"/>. Note the order of the subevents matters.
        /// </summary>
        /// <param name="dayTimeline"></param>
        /// <param name="subEvent"></param>
        /// <param name="orderedsubEventsInTimeline"></param>
        /// <returns></returns>
        Tuple<int, TimeLine> getPositionInDaytimeline (DayTimeLine dayTimeline, SubCalendarEvent subEvent, List<SubCalendarEvent> orderedsubEventsInTimeline = null)
        {
            if(orderedsubEventsInTimeline == null)
            {
                orderedsubEventsInTimeline = dayTimeline.getSubEventsInTimeLine().OrderBy(o => o.Start).ThenBy(o => o.End).ToList();
                if(orderedsubEventsInTimeline== null)
                {
                    orderedsubEventsInTimeline = new List<SubCalendarEvent>();
                }
            }

            SubCalendarEvent firstSubevent = orderedsubEventsInTimeline.FirstOrDefault();
            SubCalendarEvent lastSubevent = orderedsubEventsInTimeline.LastOrDefault();
            DateTimeOffset timeLineStart = dayTimeline.Start;
            if(firstSubevent!=null)
            {
                timeLineStart = firstSubevent.Start <= dayTimeline.Start ? firstSubevent.Start : dayTimeline.Start;
            }

            DateTimeOffset timeLineEnd = dayTimeline.End;
            if(lastSubevent!=null)
            {
                timeLineEnd = lastSubevent.End >= dayTimeline.End ? lastSubevent.End : dayTimeline.End;
            }
            TimeLine revisedTimeLineUpdatedForOverlappingEvents = new TimeLine(timeLineStart, timeLineEnd);// this ensures the subEvents are verified to fit within the timeline. This handles cases where an event might cross over say a rigid whih belongs to multiple days
            int preferredIndex = Utility.getBestPosition(revisedTimeLineUpdatedForOverlappingEvents, subEvent, orderedsubEventsInTimeline);
            var retValue = new Tuple<int, TimeLine>(preferredIndex, revisedTimeLineUpdatedForOverlappingEvents);
            return retValue;
        }

        /// <summary>
        /// given a list of <paramref name="conflictingSubEvents"/> that are not designated to any of <paramref name="allDaytimelines"/>, this function tries to reorder already assigned subevents to different days to ensure items in <paramref name="conflictingSubEvents"/> can be assigned without a conflict.
        /// It returns a tuple of which Item1 is the resolved and item2 aren't resolved
        /// </summary>
        /// <param name="conflictingSubEvents"></param>
        /// <param name="allDayTimeLines"></param>
        /// <returns></returns>
        Tuple<List<SubCalendarEvent>, List<SubCalendarEvent>> resolveConflicts(List<SubCalendarEvent> conflictingSubEvents, IList<DayTimeLine> allDayTimeLines)
        {
            List<SubCalendarEvent> resolvedConflicts = new List<SubCalendarEvent>();
            List<SubCalendarEvent> unResolvedConflicts = new List<SubCalendarEvent>();
            if (allDayTimeLines != null && allDayTimeLines.Count > 0 && conflictingSubEvents != null && conflictingSubEvents.Count > 0)
            {
                List<SubCalendarEvent> orderedConflictingSubevent = conflictingSubEvents.OrderBy(subEvent => subEvent.Score).ToList();
                foreach(SubCalendarEvent conflictingSubEvent in orderedConflictingSubevent)
                {
                    var conflictResult = repositionAlreadyAssignedEvents(conflictingSubEvent, allDayTimeLines);
                    if(conflictResult)
                    {
                        resolvedConflicts.Add(conflictingSubEvent);
                    } else
                    {
                        unResolvedConflicts.Add(conflictingSubEvent);
                    }
                }
            }

            var retValue = new Tuple<List<SubCalendarEvent>, List<SubCalendarEvent>>(resolvedConflicts, unResolvedConflicts);
            return retValue;
        }

        /// <summary>
        /// Function looks through <paramref name="allDayTimeLinesDays"/> and provides different timelines for already assigned subevents
        /// Function returns a dictionary of movable subevents to their viable time slots.
        /// </summary>
        /// <param name="allDayTimeLinesDays"></param>
        /// <returns></returns>
        bool repositionAlreadyAssignedEvents(SubCalendarEvent conflictingSubEvent, IList<DayTimeLine> allDayTimeLinesDays)
        {
            TimeLine subEventTimeLine = conflictingSubEvent.StartToEnd;// I'm going with 'conflictingSubEvent.StartToEnd' because for some reason my tests do better with this than 'conflictingSubEvent.getCalculationRange'. It'll be interesting to do some statistical tests to see why
            List<DayTimeLine>  conflictingSubEventDays = allDayTimeLinesDays.Where(dayTimeLine => subEventTimeLine.doesTimeLineInterfere(dayTimeLine)).ToList();
            Dictionary<SubCalendarEvent, HashSet<DayTimeLine>> subEventsCanbeMoved = new Dictionary<SubCalendarEvent, HashSet<DayTimeLine>>();
            foreach(DayTimeLine dayTimeLine in conflictingSubEventDays)
            {
                foreach(SubCalendarEvent subEvent in dayTimeLine.getSubEventsInTimeLine())
                {
                    HashSet<DayTimeLine> dayTimeLines = null;
                    if(!subEventsCanbeMoved.ContainsKey(subEvent))
                    {
                        dayTimeLines = new HashSet<DayTimeLine>();
                        subEventsCanbeMoved.Add(subEvent, dayTimeLines);
                    } else
                    {
                        dayTimeLines = subEventsCanbeMoved[subEvent];
                    }

                    dayTimeLines.Add(dayTimeLine);
                }
            }
            List<SubCalendarEvent> allPossibleSubeventsThatCanBeMoved = subEventsCanbeMoved.Keys.ToList();
            List<SubCalendarEvent> worseScoredSubevents = subEventsCanbeMoved.Keys.Where(subEvent => subEvent.Score > conflictingSubEvent.Score).ToList();
            List<SubCalendarEvent> possibleRepositionableSubEvents = null;
            if (worseScoredSubevents.Count< 1)// If there are no worse scored subevents then try all possible events :(
            {
                possibleRepositionableSubEvents = allPossibleSubeventsThatCanBeMoved
                    .OrderByDescending(subEvent => {
                        var interferringResult = conflictingSubEvent.getTimeLinesInterferringWithCalculationRange(subEvent.StartToEnd);
                        bool isInterferringWithSubevent = interferringResult != null;
                        if (isInterferringWithSubevent)
                        {
                            return interferringResult.Count > 0;
                        }
                        return isInterferringWithSubevent;

                    })
                    .ThenByDescending(o => o.getActiveDuration)
                    .ToList();
            } else
            {
                possibleRepositionableSubEvents = worseScoredSubevents;
            }
            TimeSpan totalRepositioned = Utility.ZeroTimeSpan;
            TimeSpan conflictingSubEventSpan = conflictingSubEvent.getActiveDuration;
            TimeSpan maxTimeSpan = conflictingSubEventSpan >= Utility.SixHourTimeSpan ? conflictingSubEventSpan : TimeSpan.FromHours(conflictingSubEventSpan.TotalHours * 1.5);
            maxTimeSpan = maxTimeSpan.removeSecondsAndMilliseconds();
            Dictionary<SubCalendarEvent, Dictionary<DayTimeLine, Dictionary<DateTimeOffset, TimeLine>>> retValue = new Dictionary<SubCalendarEvent, Dictionary<DayTimeLine, Dictionary<DateTimeOffset, TimeLine>>>();
            Dictionary<DayTimeLine, HashSet<SubCalendarEvent>> dayTimeLineToPossibleNewSubEVents = new Dictionary<DayTimeLine, HashSet<SubCalendarEvent>>();// This holds a mapping of a daytimeline to subevents that can possibly be added to it. These subevents will get moved from other days and daytimeline(the key of the dictionary) will be the new destination
            bool resolvedConflict = false;


            if (possibleRepositionableSubEvents.Count > 0)
            {
                ///This lookup hold all the subevents grouped by day. Generally you want to have multiple sub events within the same day to get moved so the chance of a huge time chunk being available is high. With the look up we try to maximize the space available based on the duration and the score of the subevents
                //var lookupForSubEventsByDay = worseScoredSubevents.ToLookup(o => o.UniversalDayIndex);
                var subEventGrouping = possibleRepositionableSubEvents.ToLookup(subEvent => subEvent.UniversalDayIndex);
                List<long> keys = new List<long>();
                List<IList<double>> multiDimesionalVars = new List<IList<double>>();
                foreach (var groupingKey in subEventGrouping) {
                    List<double> paramList = new List<double>();
                    keys.Add(groupingKey.Key);
                    TimeSpan TotalTime = TimeSpan.FromMilliseconds(groupingKey.Sum(subEvent => subEvent.getActiveDuration.TotalMilliseconds));
                    double totalTimeDim = Math.Pow(TotalTime.TotalHours, -1);
                    if (totalTimeDim == 0)
                    {
                        totalTimeDim = double.MaxValue / 2;
                    }

                    paramList.Add(totalTimeDim);
                    paramList.Add(totalTimeDim);//adding a second time for more emphasis
                    double subEventCount = 1.0/(double)groupingKey.Count();
                    paramList.Add(subEventCount);
                    


                    multiDimesionalVars.Add(paramList);
                }

                var multiVarResult = Utility.multiDimensionCalculationNormalize(multiDimesionalVars);

                List<Tuple<double, int>> multivarEvaluationToIndex = new List<Tuple<double, int>>();
                for (int i = 0; i < multiVarResult.Count; i++)
                {
                    double evaluatedResult = multiVarResult[i];
                    var tuple = new Tuple<double, int>(evaluatedResult, i);
                    multivarEvaluationToIndex.Add(tuple);
                }

                List<SubCalendarEvent> orderedWorseSubEVents = multivarEvaluationToIndex.OrderByDescending(o => o.Item1).SelectMany(o => subEventGrouping[keys[o.Item2]]).ToList();



                HashSet<DayTimeLine> repostionedBeforeDays = new HashSet<DayTimeLine>();
                foreach (SubCalendarEvent lessPrioritySubEvent in orderedWorseSubEVents)
                {
                    if(totalRepositioned <= maxTimeSpan && !resolvedConflict)
                    {
                        repostionedBeforeDays = new HashSet<DayTimeLine>();
                        HashSet<DayTimeLine> dayTimeLinesOfCOnflictingSubEvent = subEventsCanbeMoved[lessPrioritySubEvent];
                        List<DayTimeLine> plausibleRepositionableDays = allDayTimeLinesDays.Where(dayTimeLine =>
                            {
                                bool predicateBool = false;
                                if (!dayTimeLinesOfCOnflictingSubEvent.Contains(dayTimeLine))
                                {
                                    List<TimeLine> viableTImelines = lessPrioritySubEvent.getTimeLinesInterferringWithCalculationRange(dayTimeLine);
                                    if(viableTImelines!=null)
                                    {
                                        predicateBool = viableTImelines.Any(timeline => lessPrioritySubEvent.canExistWithinTimeLine(timeline));
                                    }
                                }
                                return predicateBool;
                            })
                            .OrderBy(dayTimeLine => dayTimeLine.Occupancy)
                            .ToList();
                        bool isAlternateDayFoundForWorseSubevent = false;
                        foreach (var dayTimeline in plausibleRepositionableDays)
                        {
                            if(!isAlternateDayFoundForWorseSubevent)
                            {
                                var subEventsInTimeline = dayTimeline.getSubEventsInTimeLine().OrderBy(o => o.Start).ThenBy(o => o.End).ToList();
                                var positionEvaluationResult = getPositionInDaytimeline(dayTimeline, lessPrioritySubEvent, subEventsInTimeline);
                                int preferredIndex = positionEvaluationResult.Item1;
                                var revisedTimeLineUpdatedForOverlappingEvents = positionEvaluationResult.Item2;

                                if (preferredIndex > -1)
                                {
                                    subEventsInTimeline.Insert(preferredIndex, lessPrioritySubEvent);
                                    if (!Utility.PinSubEventsToStart(subEventsInTimeline, revisedTimeLineUpdatedForOverlappingEvents))
                                    {
                                        throw new Exception("There is a flaw in your logic for conflict resoution");
                                    }

                                    var currentDayTimelines = subEventsCanbeMoved[lessPrioritySubEvent];
                                    foreach (DayTimeLine currentTimeLineForSubevent in currentDayTimelines)
                                    {
                                        currentTimeLineForSubevent.RemoveSubEvent(lessPrioritySubEvent.Id);
                                        repostionedBeforeDays.Add(currentTimeLineForSubevent);
                                    }
                                    dayTimeline.AddToSubEventList(lessPrioritySubEvent);
                                    totalRepositioned += lessPrioritySubEvent.getActiveDuration;
                                    isAlternateDayFoundForWorseSubevent = true;
                                }
                            } else
                            {
                                break;
                            }
                        }
                        if(totalRepositioned >= conflictingSubEventSpan)
                        {
                            foreach(DayTimeLine dayTimeline in repostionedBeforeDays)
                            {
                                if(!resolvedConflict)
                                {
                                    var subEventsInTimeline = dayTimeline.getSubEventsInTimeLine().OrderBy(o => o.Start).ThenBy(o => o.End).ToList();
                                    var positionEvaluationResult = getPositionInDaytimeline(dayTimeline, conflictingSubEvent, subEventsInTimeline);
                                    int preferredIndex = positionEvaluationResult.Item1;
                                    var revisedTimeLineUpdatedForOverlappingEvents = positionEvaluationResult.Item2;
                                    if (preferredIndex > -1)
                                    {
                                        var subEventsWithConflicts = subEventsInTimeline.ToList();
                                        subEventsWithConflicts.Insert(preferredIndex, conflictingSubEvent);
                                        if (!Utility.PinSubEventsToEnd(subEventsWithConflicts, revisedTimeLineUpdatedForOverlappingEvents))
                                        {
                                            throw new Exception("There is a flaw in your logic for conflict resoution after finding slot for conflict");
                                        }
                                        resolvedConflict = true;
                                        dayTimeline.AddToSubEventList(conflictingSubEvent);
                                        conflictingSubEvent.ParentCalendarEvent.designateSubEvent(conflictingSubEvent, Now);
                                        this._ConflictingSubEvents.Remove(conflictingSubEvent);
                                    }
                                }
                            }
                        }
                        if(resolvedConflict)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                
            }
            return resolvedConflict;
        }

        void tryToCentralizeSubEvents(DayTimeLine dayTimeLine)
        {
            List<SubCalendarEvent> AllRigids = new List<SubCalendarEvent>();
            HashSet<SubCalendarEvent> NonRigidSubEvents = new HashSet<SubCalendarEvent>();
            HashSet<SubCalendarEvent> subEvents = new HashSet<SubCalendarEvent>();
            dayTimeLine.getSubEventsInTimeLine().ForEach((subEvent) =>
            {
                if (subEvent.isLocked)
                {
                    AllRigids.Add(subEvent);
                }
                else
                {
                    NonRigidSubEvents.Add(subEvent);
                }
                subEvents.Add(subEvent);
            });


            IEnumerable<SubCalendarEvent> rigidTimeLine = intersectingRigidEvents(dayTimeLine, AllRigids);
            List<SubCalendarEvent> subEventsWithinDaytimeLine = NonRigidSubEvents.Concat(rigidTimeLine).ToList();
            spaceEventsByTravelTime(dayTimeLine, subEventsWithinDaytimeLine, false);
        }

        /// <summary>
        /// gets continuous three days given a list of ordered days and the index of the given day.
        /// It uses the index as the middle day and picks a day before and after. 
        /// If the index is an edge day it picks the two other days using the closest two days within the list.
        /// </summary>
        /// <param name="orderedTimeLines"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        Tuple<TimelineWithSubcalendarEvents, IEnumerable<SubCalendarEvent>, List<TimelineWithSubcalendarEvents>> getThreeContinuousDay(IList<TimelineWithSubcalendarEvents> orderedTimeLines, int index)
        {
            int beforeIndex = index - 1;
            int afterIndex = index + 1;
            List<int> indexes = new List<int> { beforeIndex, index, afterIndex };
            if (beforeIndex < 0)
            {
                beforeIndex = afterIndex + 1;
                indexes[0] = index;
                indexes[1] = afterIndex;
                indexes[2] = beforeIndex;
            }
            else if (orderedTimeLines.Count - 1 == index)
            {
                afterIndex = index - 2;
                indexes[0] = afterIndex;
                indexes[1] = beforeIndex;
                indexes[2] = index;
            }
            TimelineWithSubcalendarEvents first = orderedTimeLines[indexes[0]];
            TimelineWithSubcalendarEvents second = orderedTimeLines[indexes[1]];
            TimelineWithSubcalendarEvents third = orderedTimeLines[indexes[2]];
            IEnumerable<SubCalendarEvent> allSubEVents = first.getSubEventsInTimeLine().Concat(second.getSubEventsInTimeLine()).Concat(third.getSubEventsInTimeLine());
            TimelineWithSubcalendarEvents retValue = new TimelineWithSubcalendarEvents(first.Start, third.End, new HashSet<SubCalendarEvent>(allSubEVents));
            return new Tuple<TimelineWithSubcalendarEvents, IEnumerable<SubCalendarEvent>, List<TimelineWithSubcalendarEvents>>(retValue, allSubEVents, new List<TimelineWithSubcalendarEvents>() { first, second, third });
        }

        List<SubCalendarEvent> PrepFirstTwentyFourHours(List<SubCalendarEvent> possibleSubevents, TimeLine FirstTwentyFour)
        {
            TimeLine FirstFortyEight = new TimeLine(Now.firstDay.Start, Now.firstDay.Start.AddDays(2));
            List<SubCalendarEvent> ForCalculation = possibleSubevents.OrderBy(o=> o.Start).Where(obj => obj.StartToEnd.InterferringTimeLine(FirstTwentyFour) != null).ToList();

            List<SubCalendarEvent> AllRigids = ForCalculation.Where(obj => obj.isLocked).ToList();

            ForCalculation = ForCalculation.Except(AllRigids).ToList();
            ForCalculation.ForEach(obj => obj.addReasons(new PreservedOrder(ForCalculation.Select(subEvent => subEvent.SubEvent_ID).ToList())));

            List<SubCalendarEvent> OrderedPreviousTwentyfourNonrigids = new List<SubCalendarEvent>(ForCalculation);
            OrderedPreviousTwentyfourNonrigids = OrderedPreviousTwentyfourNonrigids.OrderBy(sub => sub.Start).ToList();
            FirstTwentyFour.AddBusySlots(AllRigids.Select(o => o.ActiveSlot));
            List<SubCalendarEvent> PopulatedSubcals = BuildAllPossibleSnugLists(ForCalculation, FirstTwentyFour, 1, new List<SubCalendarEvent>());

            List<SubCalendarEvent> retValue = PopulatedSubcals.ToList();

            return retValue;
        }


        List<SubCalendarEvent> processTwentyFourHours(DayTimeLine myDayTimeLine, List<SubCalendarEvent> AllSubEvents)//,List<BusyTimeLine>BusySlots)
        {
            ++CountCall;
            List<SubCalendarEvent> AllreadyAssigned = myDayTimeLine.getSubEventsInTimeLine();
            List<SubCalendarEvent> AllRigids = new List<SubCalendarEvent>();
            foreach(var subEvent in AllSubEvents.Concat(AllreadyAssigned))
            {
                if(subEvent.isLocked)
                {
                    AllRigids.Add(subEvent);
                }
            }

            List<SubCalendarEvent> Movables = AllSubEvents.Except(AllRigids).ToList();
            if (Movables.Count < 1)
            {
                return AllRigids;
            }
            Location AvgLocation = Location.AverageGPSLocation((AllRigids.Concat(myDayTimeLine.getSubEventsInTimeLine())).Select(obj => obj.Location));
            SubCalendarEvent.resetScores(AllSubEvents);
            TimeLine timeLineForCalc = myDayTimeLine;
            Tuple<TimeLine, Double> BaseLine = scoreAllSubeventsForReferenceCalculation(Movables, myDayTimeLine, AvgLocation);
            Movables = Movables.OrderBy(obj => obj.Score).ThenBy(obj => obj.fittability).ToList();
            Movables.ForEach(sub => sub.ParentCalendarEvent.undesignateSubEvent(sub));

            List<SubCalendarEvent> Calculables = Movables.Count > 6 ? Movables.GetRange(0, 7) : Movables.ToList();


            List<SubCalendarEvent> Reassigned = BuildAllPossibleSnugLists(Calculables, timeLineForCalc, 1, AllreadyAssigned.Except(AllRigids).ToList());
            foreach (SubCalendarEvent subcalendaEvent in Reassigned.Except(AllreadyAssigned))
            {
                BestFitReason bestFit = new BestFitReason(timeLineForCalc.TotalActiveSpan, timeLineForCalc.TotalFreeSpotAvailable, subcalendaEvent.getActiveDuration);
                subcalendaEvent.addReasons(bestFit);
            }

            SubCalendarEvent.updateUnUsable(Calculables.Except(Reassigned), myDayTimeLine.UniversalIndex);//updates the un unsable days

            Reassigned.AddRange(AllRigids);
            HashSet<SubCalendarEvent> ReassignedHash = new HashSet<SubCalendarEvent>(Reassigned);


            Movables = Movables.Except(Reassigned).ToList();
            myDayTimeLine.AddToSubEventList(Reassigned);

            HashSet<SubCalendarEvent> ReassignedHashDayTime = new HashSet<SubCalendarEvent>(myDayTimeLine.getSubEventsInTimeLine());
            HashSet<SubCalendarEvent> diff = new HashSet<SubCalendarEvent>(ReassignedHashDayTime.Except(ReassignedHash));
            foreach(SubCalendarEvent subEvent in Reassigned.Concat(AllRigids))
            {
                subEvent.ParentCalendarEvent.designateSubEvent(subEvent, Now);
                _ConflictingSubEvents.Remove(subEvent);
            }
            return Reassigned;
        }



        Tuple<TimeLine, Double> scoreAllSubeventsForReferenceCalculation(IEnumerable<SubCalendarEvent> AllSubEvents, TimeLine DayTimeLine, Location rigidLocation)
        {
            Dictionary<SubCalendarEvent, TimeLine> SUbEventTOInterferringTimeLine = AllSubEvents.ToDictionary(obj => obj, obj => new TimeLine(DayTimeLine.Start > obj.getProcrastinationInfo.PreferredStartTime ? DayTimeLine.Start : obj.getProcrastinationInfo.PreferredStartTime, obj.getCalculationRange.End));//selects the relevenat timeline per subeevent. Tries to use the prcrastination info to provide better preferenceing
            Dictionary<SubCalendarEvent, double> Distances = AllSubEvents.ToDictionary(obj => obj, obj => Location.calculateDistance(rigidLocation, obj.Location));
            int ParameterCount = 2;
            double MaxPerParameter = (double)100 / ParameterCount;

            KeyValuePair<SubCalendarEvent, TimeLine> BestTimeLine = SUbEventTOInterferringTimeLine.First();
            foreach (KeyValuePair<SubCalendarEvent, TimeLine> eachKeyValuePair in SUbEventTOInterferringTimeLine)
            {
                if (eachKeyValuePair.Value.TimelineSpan < BestTimeLine.Value.TimelineSpan)
                {
                    continue;
                }
                else
                {
                    BestTimeLine = eachKeyValuePair;
                }
            }

            KeyValuePair<SubCalendarEvent, double> LongestDistance = Distances.First();
            foreach (KeyValuePair<SubCalendarEvent, double> eachKeyValuePair in Distances)
            {
                if (eachKeyValuePair.Value < LongestDistance.Value)
                {
                    continue;
                }
                else
                {
                    LongestDistance = eachKeyValuePair;
                }
            }
            Dictionary<SubCalendarEvent, List<double>> subEventToMultiDimenstionVars = AllSubEvents.ToDictionary(obj => obj, obj => new List<double>());
            //evaluates the score for each event against the base Timeline range base case
            foreach (KeyValuePair<SubCalendarEvent, TimeLine> eachKeyValuePair in SUbEventTOInterferringTimeLine)
            {
                double score = ((double)eachKeyValuePair.Value.TimelineSpan.Ticks / BestTimeLine.Value.TimelineSpan.Ticks) * MaxPerParameter;
                subEventToMultiDimenstionVars[eachKeyValuePair.Key].Add(score);
            }

            if (LongestDistance.Value > 0)
            {
                //evaluates the score for each event against the base Distance range base case
                foreach (KeyValuePair<SubCalendarEvent, double> eachKeyValuePair in Distances)
                {
                    double score = (double)(eachKeyValuePair.Value / LongestDistance.Value) * MaxPerParameter;
                    subEventToMultiDimenstionVars[eachKeyValuePair.Key].Add(score);
                }
            }

            List<double> resultants = Utility.multiDimensionCalculation(subEventToMultiDimenstionVars.Select(obj => ((IList<double>)obj.Value)).ToList());
            int i = 0;
            foreach (SubCalendarEvent subcalendarEvent in subEventToMultiDimenstionVars.Keys)
            {
                subcalendarEvent.incrementScore(resultants[i]);
                i++;
            }
            Tuple<TimeLine, Double> retValue = new Tuple<TimeLine, double>(BestTimeLine.Value, LongestDistance.Value);
            return retValue;
        }


        /// <summary>
        /// FUnction generates a multidimesional array that generates the feature set of each calendar event.
        /// Below the options with * means not implemented but can be imimplemented in the future
        /// Feaure set includes the time of creation, completion ratio, duration of calEvent, time till deadline, *distance from home
        /// </summary>
        /// <param name="allCalEvents"></param>
        /// <returns></returns>
        List<IList<double>> generateMultiDimensionalParams(List<CalendarEvent> allCalEvents)
        {
            List<IList<double>> retValue = new List<IList<double>>();

            foreach (CalendarEvent calEvent in allCalEvents)
            {
                TimeSpan span = Now.constNow - calEvent.TimeCreated;
                double spanHours = Math.Abs(span.TotalHours);
                spanHours = spanHours == 0 ? 100000 : 1.0 / spanHours;
                double completionCount = (double)calEvent.CompletionCount < 1 ? 0.001 : calEvent.CompletionCount;
                double completionRatio = calEvent.CompletionCount / calEvent.NumberOfSplit;
                double duration = calEvent.AverageTimeSpanPerSubEvent.TotalHours * (calEvent.NumberOfSplit - calEvent.CompletionCount);
                duration = 1 / duration;

                TimeSpan timeSpanToDeadline = Now.constNow - calEvent.End;
                double timeTillDeadline = Math.Abs(timeSpanToDeadline.TotalHours);
                timeTillDeadline = double.IsNaN(timeTillDeadline) ? double.MaxValue - allCalEvents.Count : timeTillDeadline;

                List<double> features = new List<double>() { spanHours, completionRatio, completionCount, duration, timeTillDeadline };
                retValue.Add(features);
            }
            return retValue;
        }

        /// <summary>
        /// Function selects the days to be used for the calculation of a schedule. All SubEvents need to be part of the same Calendar event.
        /// </summary>
        /// <param name="AllSubEvents"></param>
        /// <param name="AllDays"></param>
        /// <returns>
        /// a list of tuples, each tuple has: 
        ///     item1 is the preferred day for a subevent
        ///     item2 is the subevent
        /// 
        /// </returns>
        List<Tuple<long, SubCalendarEvent>> EvaluateEachDayIndexForEvent(
            List<SubCalendarEvent> AllSubEvents,
            List<DayTimeLine> AllDays,
            CalendarEvent calEvent,
            EventDayBags bagsPerDay,
            long balancingStartingindex)
        {

            List<Tuple<long, SubCalendarEvent>> retValue = new List<Tuple<long, SubCalendarEvent>>();
            List<TimeLine> daysSelected = new List<TimeLine>();
            if (AllSubEvents.Count > 0)
            {
                Procrastination procrastinationProfile = AllSubEvents[0].getProcrastinationInfo;
                ILookup<TimeSpan, SubCalendarEvent> durationsToSubEvents = AllSubEvents.ToLookup(subEvent => subEvent.getActiveDuration, subEvent => subEvent);
                if (durationsToSubEvents.Count == 1)
                {
                    SubCalendarEvent subEvent = durationsToSubEvents[durationsToSubEvents.First().Key].First();
                }

                DateTimeOffset preferredStartTime = procrastinationProfile.PreferredStartTime > calEvent.CalculationStart ? procrastinationProfile.PreferredStartTime : calEvent.CalculationStart;
                long PreferrdDayIndex = Now.getDayIndexFromStartOfTime(preferredStartTime);
                long startDayIndex = Now.getDayIndexFromStartOfTime(calEvent.CalculationStart);
                if (balancingStartingindex > PreferrdDayIndex)
                {
                    PreferrdDayIndex = balancingStartingindex;
                }

                List <mTuple<bool, DayTimeLine>> OptimizedDayTimeLine = AllDays.Select(obj => new mTuple<bool, DayTimeLine>(((long)(obj.UniversalIndex - PreferrdDayIndex) >= 0), obj)).ToList();//this line orders Daytimeline by  if they are after the procrastination day.

                List<mTuple<bool, DayTimeLine>> beforeProcrastination = OptimizedDayTimeLine.Where(obj => !obj.Item1).ToList();
                OptimizedDayTimeLine = OptimizedDayTimeLine.GetRange(beforeProcrastination.Count, OptimizedDayTimeLine.Count - beforeProcrastination.Count);// this reorders all the days with before or on procrastination to the back of list
                int bagCount = bagsPerDay.DayBags().Count;
                List<DayBag> dayBags = bagsPerDay.DayBags().GetRange(OptimizedDayTimeLine.First().Item2.BoundedIndex, bagCount - OptimizedDayTimeLine.First().Item2.BoundedIndex)
                    .Concat(bagsPerDay.DayBags().GetRange(0, OptimizedDayTimeLine.First().Item2.BoundedIndex + 1)).ToList();
                List<double> timeLineScores = calEvent.EvaluateTimeLines(OptimizedDayTimeLine.Select(timeLine => (TimelineWithSubcalendarEvents)timeLine.Item2).ToList(), Now);

                List<IList<double>> combinedDOubles = timeLineScores.Select((score, i) => {
                    IList<double> comValue = new List<double> { score, dayBags[i].Score, calEvent.getSubeventsInDay(OptimizedDayTimeLine[i].Item2.UniversalIndex).Count() };
                    return comValue;
                }).ToList();

                timeLineScores = Utility.multiDimensionCalculationNormalize(combinedDOubles);
                double subEventPerDay = ((double)AllSubEvents.Count) / (double)timeLineScores.Count; // this holds the ideal sub event per day 

                List <Tuple<int, double, DayTimeLine>> dayIndexToTImeLine = timeLineScores.Select((score, index) => { return new Tuple<int, double, DayTimeLine>(index, score, OptimizedDayTimeLine[index].Item2); }).ToList();

                //DayTimeLineCurrentProperties holds the propeties of all the daytimeline elements. The tuple has the folloiwng Left, Right, Difference, score
                Dictionary<DayTimeLine, DayTempEvaluation> DayTimeLineCurrentProperties = new Dictionary<DayTimeLine, DayTempEvaluation>();
                

                List<mTuple<double, DayTimeLine>> orderedOnEvaluation = dayIndexToTImeLine.Where(tuple => !double.IsNaN(tuple.Item2)).OrderBy(tuple => tuple.Item2).Select(tuple => new mTuple<double, DayTimeLine>(tuple.Item2, tuple.Item3)).ToList();
                List<long> dayIndexes = orderedOnEvaluation.Select(obj => obj.Item2.UniversalIndex).OrderBy(index => index).ToList();
                List<DayTimeLine> useUpOrder = new List<DayTimeLine>();
                mTuple<double, DayTimeLine> lastDaySelected = orderedOnEvaluation.FirstOrDefault();

                if (lastDaySelected != null)
                {
                    long selectedDayIndex = lastDaySelected.Item2.UniversalIndex;
                    SubCalendarEvent subEvent = AllSubEvents.First();
                    daysSelected.Add(lastDaySelected.Item2);
                    retValue.Add(new Tuple<long, SubCalendarEvent>(selectedDayIndex, subEvent));
                    useUpOrder.Add(lastDaySelected.Item2);
                    if (orderedOnEvaluation.Count != 0)
                    {
                        long iniIndex = (long)dayIndexes[0];
                        long finalIndex = (long)dayIndexes.Last();
                        DayTimeLineCurrentProperties = orderedOnEvaluation.ToDictionary(dayTuple =>
                        {
                            return dayTuple.Item2;
                        },
                            dayTuple =>
                            {
                                long left = (long)dayTuple.Item2.UniversalIndex - iniIndex;
                                long right = finalIndex- (long)dayTuple.Item2.UniversalIndex;
                                long diff = (long)left - (long)right;
                                long uDiff = (long)Math.Abs(diff);
                                return new DayTempEvaluation(subEventPerDay)
                                {
                                    Diff = uDiff,
                                    Left = left,
                                    Right = right,
                                    Score = dayTuple.Item1,
                                    TimeLineScore = dayTuple.Item1,
                                    InitialTimeLineScore = dayTuple.Item1,
                                    AssignedSubEventsInDay = calEvent.getSubeventsInDay(dayTuple.Item2.UniversalIndex).Count(),
                                    DayIndex = (long)dayTuple.Item2.UniversalIndex
                                };
                            }
                        );
                        DayTimeLineCurrentProperties[lastDaySelected.Item2].incrementDayElectionCount();
                        for (int i = 1; i < AllSubEvents.Count; i++)
                        {
                            subEvent = AllSubEvents[i];
                            //if (useUpOrder.Count != dayIndexes.Count)
                            {
                                List<IList<double>> data = orderedOnEvaluation.Select(obj => (IList<double>)DayTimeLineCurrentProperties[obj.Item2].toMultiArrayDict()).ToList();
                                List<double> values = Utility.multiDimensionCalculationNormalize(data);
                                int lowestIndex = values.MinIndex();
                                lastDaySelected = orderedOnEvaluation[lowestIndex];
                                DayTimeLine minDayTimeLine = lastDaySelected.Item2;
                                daysSelected.Add(lastDaySelected.Item2);
                                retValue.Add(new Tuple<long, SubCalendarEvent>(minDayTimeLine.UniversalIndex, subEvent));
                                selectedDayIndex = lastDaySelected.Item2.UniversalIndex;
                                DayTimeLineCurrentProperties[minDayTimeLine].incrementDayElectionCount();
                                useUpOrder.Add(minDayTimeLine);
                            }
                            //else
                            //{
                            //    int j = 0;
                            //    int usedUPLength = useUpOrder.Count;
                            //    for (; i < AllSubEvents.Count; i++, j++)
                            //    {
                            //        SubCalendarEvent excessSubEvent = AllSubEvents[i];
                            //        int dayIndex = j % usedUPLength;
                            //        DayTimeLine dayTimeLine = useUpOrder[dayIndex];
                            //        DayTimeLineCurrentProperties[dayTimeLine].incrementDayElectionCount();
                            //        retValue.Add(new Tuple<long, SubCalendarEvent>(dayTimeLine.UniversalIndex, excessSubEvent));
                            //    }
                            //}
                        }
                    }
                }
            }
            return retValue;

        }
        /// <summary>
        /// Giving a ReferenceTimeLine this function will fimnd the most efficient way to pack the reftime with a list of sub events
        /// </summary>
        /// <param name="subEventsForCalculation"></param>
        /// <param name="ReferenceTimeLine"></param>
        /// <param name="Occupancy"></param>
        /// <param name="AlreadyAssignedEvents">Sub events that are already assigned to the timeline</param>
        /// <returns></returns>
        List<SubCalendarEvent> BuildAllPossibleSnugLists(
            IEnumerable<SubCalendarEvent> subEventsForCalculation,
            TimeLine ReferenceTimeLine,
            double Occupancy,
            IEnumerable<SubCalendarEvent> AlreadyAssignedEvents)
        {
            TimeLine[] JustFreeSpots = getAllFreeSpots_NoCompleteSchedule(ReferenceTimeLine).OrderByDescending(obj => obj.End).ToArray();

            List<SubCalendarEvent> retValue = new List<SubCalendarEvent>();

            List<SubCalendarEvent> AllInterferringSubEvents = subEventsForCalculation.ToList();
            List<SubCalendarEvent> NoChangeTOOrder_AllInterferringSubEvents = AllInterferringSubEvents.ToList();
            List<SubCalendarEvent> AllInterferringSubEvents_Cpy = AllInterferringSubEvents.ToList();


            HashSet<SubCalendarEvent> AllInterferringSubEvents_Cpy_Hash = new HashSet<SubCalendarEvent>(AllInterferringSubEvents_Cpy.Concat(AlreadyAssignedEvents));
            AllInterferringSubEvents_Cpy = AllInterferringSubEvents_Cpy_Hash.Except(AlreadyAssignedEvents).ToList();
            Dictionary<string, SubCalendarEvent> AllEvents_Dict = AllInterferringSubEvents_Cpy_Hash.ToDictionary(obj => obj.getId, obj => obj);
            Dictionary<TimeLine, List<SubCalendarEvent>> Dict_ConstrainedElements;
            if (AlreadyAssignedEvents.Count() > 0)
            {
                Dict_ConstrainedElements = JustPickCollidingTimeline(JustFreeSpots.ToList(), new HashSet<SubCalendarEvent>(AlreadyAssignedEvents).ToList());
            }
            else
            {
                var onlyViableTimeLineForSubevent = generateConstrainedList(JustFreeSpots.ToList(), AllInterferringSubEvents_Cpy);
                Dict_ConstrainedElements = stitchRestrictedSubCalendarEvent(JustFreeSpots.ToList(), onlyViableTimeLineForSubevent);
            }


            IList<KeyValuePair<TimeLine, List<SubCalendarEvent>>> Dict_ConstrainedElements_List = Dict_ConstrainedElements.ToList();


            Dictionary<string, SubCalendarEvent> AllReassignedElements = new Dictionary<string, SubCalendarEvent>();
            List<TimeLine> JustFreeSpots_Cpy = JustFreeSpots.ToList();

            DateTimeOffset TestTime = new DateTimeOffset(2014, 9, 30, 0, 0, 0, new TimeSpan());

            for (int i = 0; i < JustFreeSpots.Length; i++)
            {
                TimeLine refTImeLine = JustFreeSpots[i];
                List<SubCalendarEvent> COnstrainedElementsForTimeLine = Dict_ConstrainedElements[refTImeLine];
                if (!Utility.PinSubEventsToStart(COnstrainedElementsForTimeLine, refTImeLine))
                {
                    bool testMe = Utility.PinSubEventsToEnd(COnstrainedElementsForTimeLine, refTImeLine);
                    throw new Exception("Error before call to stitchunrestricted 0");
                }

                List<SubCalendarEvent> reassignedElements = stitchUnRestrictedSubCalendarEvent(JustFreeSpots[i], COnstrainedElementsForTimeLine, AllInterferringSubEvents_Cpy.ToList());
                DebugCounter++;
                foreach (SubCalendarEvent eachSubCalendarEvent in reassignedElements)
                {
                    AllReassignedElements.Add(eachSubCalendarEvent.getId, eachSubCalendarEvent);
                    AllInterferringSubEvents_Cpy.Remove(eachSubCalendarEvent);
                    AllInterferringSubEvents_Cpy_Hash.Remove(eachSubCalendarEvent);
                }

                JustFreeSpots_Cpy.RemoveAt(0);
                if ((JustFreeSpots_Cpy.Count > 0) && (AlreadyAssignedEvents.Count() == 0))
                {
                    SubCalendarEvent.updateMiscData(AllInterferringSubEvents_Cpy, 0);
                    //I believe the next two lines of code can be optimized and combined to one
                    ///*
                    Dict_ConstrainedElements = generateConstrainedList(JustFreeSpots_Cpy.ToList(), AllInterferringSubEvents_Cpy);
                    Dict_ConstrainedElements = stitchRestrictedSubCalendarEvent(JustFreeSpots_Cpy.ToList(), Dict_ConstrainedElements);
                    if (!Utility.PinSubEventsToStart(Dict_ConstrainedElements[JustFreeSpots_Cpy[0]], JustFreeSpots_Cpy[0]))
                    {
                        bool testMe = Utility.PinSubEventsToEnd(Dict_ConstrainedElements[JustFreeSpots_Cpy[0]], JustFreeSpots_Cpy[0]);
                        throw new Exception("Error before call to stitchunrestricted 1");
                    }

                }


                retValue.AddRange(reassignedElements);

            }
            return retValue;
        }

        List<TimeLine> reorderFreeSpotBasedOnTimeSpanAndEndtime(IEnumerable<TimeLine> AllTimeLines)
        {
            Dictionary<TimeSpan, List<TimeLine>> TImespanTOEvents = new Dictionary<TimeSpan, List<TimeLine>>();
            foreach (TimeLine eachTimeLine in AllTimeLines)
            {
                if (TImespanTOEvents.ContainsKey(eachTimeLine.TimelineSpan))
                {
                    TImespanTOEvents[eachTimeLine.TimelineSpan].Add(eachTimeLine);
                }
                else
                {
                    TImespanTOEvents.Add(eachTimeLine.TimelineSpan, new List<TimeLine>() { eachTimeLine });
                }
            }
            Dictionary<TimeSpan, List<TimeLine>> ret_Prepare = new Dictionary<TimeSpan, List<TimeLine>>();

            foreach (KeyValuePair<TimeSpan, List<TimeLine>> eachKeyValuePair in TImespanTOEvents)
            {
                ret_Prepare.Add(eachKeyValuePair.Key, eachKeyValuePair.Value.OrderByDescending(obj => obj.End).ToList());//gets each TImespan keyvaluepair orders, descendingly, based on the end time. THis ensures that the timeline from the end get picked first
            }

            List<TimeLine> retValue = ret_Prepare.OrderBy(obj => obj.Key).SelectMany(obj => obj.Value).ToList();//orders timeline timespan. Ensutres smaller timelines are at first.
            return retValue;
        }


        List<SubCalendarEvent> OrderSubEventsBasedOnOrder(IEnumerable<SubCalendarEvent> SomeOrderedEvents, Dictionary<string, Tuple<SubCalendarEvent, int>> SubEventToOrder, TimeLine RestrictingTimeLine)
        {
            List<SubCalendarEvent> InitalOrder = SomeOrderedEvents.ToList();
            List<SubCalendarEvent> RetValue = new List<SubCalendarEvent>();
            List<Tuple<SubCalendarEvent, int>> OrderedSubEVents = SomeOrderedEvents.Where(obj => SubEventToOrder.ContainsKey(obj.getId)).Select(obj => SubEventToOrder[obj.getId]).OrderBy(obj => obj.Item2).ToList();
            List<SubCalendarEvent> NotInOrder = SomeOrderedEvents.Where(obj => !SubEventToOrder.ContainsKey(obj.getId)).ToList();


            List<SubCalendarEvent> CurrentList = InitalOrder.ToList();
            int i = 0;
            int preferredIndex = i;//Index to try insertion of next sub event
            for (; i < OrderedSubEVents.Count; i++)
            {
                SubCalendarEvent MySub = OrderedSubEVents[i].Item1;
                List<SubCalendarEvent> NextSeries = CurrentList.ToList();
                NextSeries.Remove(MySub);
                if (i < OrderedSubEVents.Count - 1)
                {
                    int j = preferredIndex;
                    for (; j < NextSeries.Count - 1; j++)
                    {
                        NextSeries.Insert(j, MySub);
                        if (Utility.PinSubEventsToStart(NextSeries, RestrictingTimeLine))
                        {
                            CurrentList = NextSeries;
                            preferredIndex = j + 1;
                            break;
                        }
                        preferredIndex = j + 1;
                        NextSeries.Remove(MySub);
                    }
                }
            }

            if (!Utility.PinSubEventsToStart(CurrentList, RestrictingTimeLine))
            {
                throw new Exception("THeres a problem with OrderSubEventsBasedOnOrder. Some how the reordered elements wont fit in RestrictingTimeLine");
            }

            RetValue = CurrentList;
            return RetValue;
        }


        /// <summary>
        /// Thie gets the buffer timespan between each subevent. Note this tries to ensure it can be constrained within the provided timeline
        /// </summary>
        /// <param name="AllEvents">All sub events needed to be sapced</param>
        /// <param name="restrictingTimeline">Timeline within which this may or may not fit with the buffer, its major use is to verify that the sub events can fit</param>
        /// <param name="calculateRemoely">Should the buffer loo kup reach out to google maps if not in buffer is not in cache</param>
        /// <param name="useNonDefaultLocation">if there is a default or no location use the next non-location as the locaition. Note next is based on the order of subevents and not geolocation</param>
        Dictionary<SubCalendarEvent, mTuple<TimeSpan, TimeSpan>> CreateBufferForEachEvent(List<SubCalendarEvent> AllEvents, TimeLine restrictingTimeline, bool calculateRemoely = true, bool useNonDefaultLocation = true)
        {
            if (AllEvents.Count < 1)
            {
                return null;
            }

            List<SubCalendarEvent> AllEventsDeduped = new List<SubCalendarEvent>();
            HashSet<SubCalendarEvent> allEventshash = new HashSet<SubCalendarEvent>();
            foreach (SubCalendarEvent eachSubcalendarEvent in AllEvents)
            {
                if(!allEventshash.Contains(eachSubcalendarEvent))
                {
                    allEventshash.Add(eachSubcalendarEvent);
                    AllEventsDeduped.Add(eachSubcalendarEvent);
                }
            }

            AllEvents = AllEventsDeduped.ToList();
            List<SubCalendarEvent> justThemRigids = AllEvents.Where(obj => obj.isLocked).ToList();

            TimeLine myTimeLines = new TimeLine(restrictingTimeline.Start, restrictingTimeline.End);
            List<SubCalendarEvent> allEvents_reversed = AllEvents.ToList();
            allEvents_reversed.Reverse();
            foreach (SubCalendarEvent eachSubCalendarEvent in allEvents_reversed)
            {
                TimeLine oldTimeLine = myTimeLines;
                bool pinSuccess = eachSubCalendarEvent.PinToEnd(myTimeLines);
                myTimeLines = new TimeLine(restrictingTimeline.Start, eachSubCalendarEvent.Start);
                if (!pinSuccess && !eachSubCalendarEvent.isLocked)
                {
                    throw new Exception("Invalid list used in CreateBufferForEachEvent");//hack alert we need to involve conflicting events
                }

            }


            TimeSpan bufferPerMile = new TimeSpan(0, 4, 0);
            int nextSubEventIndex = 0;
            TimeLine referencePinningTImeline = new TimeLine(restrictingTimeline.Start, restrictingTimeline.End);
            SubCalendarEvent firstEvent = AllEvents[0];
            if (!firstEvent.PinToStart(referencePinningTImeline) && !firstEvent.isLocked)
            {
                throw new Exception("this is a weird bug to have in CreateBufferForEachEvent");
            }
            Location lastKnownNonDefaultLocation = null;
            Dictionary<SubCalendarEvent, mTuple<TimeSpan, TimeSpan>> retValue = new Dictionary<SubCalendarEvent, mTuple<TimeSpan, TimeSpan>>();
            for (int currentSubEventIndex = 0; currentSubEventIndex < AllEvents.Count - 1; currentSubEventIndex++)
            {
                nextSubEventIndex = currentSubEventIndex + 1;
                Tuple<SubCalendarEvent, SubCalendarEvent> myCoEvents = new Tuple<SubCalendarEvent, SubCalendarEvent>(AllEvents[currentSubEventIndex], AllEvents[nextSubEventIndex]);
                TimeSpan bufferSpan = Utility.NegativeTimeSpan;
                Location currentLocation = myCoEvents.Item1.Location;
                Location nextLocation = myCoEvents.Item2.Location;
                bool currentLocationIsLastKnownGood = false;
                bool nextLocationIsLastKnownGood = false;
                if (lastKnownNonDefaultLocation!=null && useNonDefaultLocation)
                {
                    if(currentLocation.isDefault || currentLocation.isNull)
                    {
                        currentLocation = lastKnownNonDefaultLocation;
                        currentLocationIsLastKnownGood = true;
                    } else if (nextLocation.isDefault || nextLocation.isNull)
                    {
                        nextLocation = lastKnownNonDefaultLocation;
                        nextLocationIsLastKnownGood = true;
                    }
                }

                LocationCacheEntry cacheEntry = getTravelEntry(currentLocation, nextLocation);

                if (calculateRemoely)
                {
                    if(currentLocation != nextLocation)
                    {
                        double distance = Location.calculateDistance(currentLocation, nextLocation);
                        bool isCacheEntryInvalid = cacheEntry == null || (Now.constNow - cacheEntry.LastUpdate >= CacheInvalidationTimeSpan);
                        if (isCacheEntryInvalid)
                        {
                            if (distance < 0.5)
                            {
                                bufferSpan = TimeSpan.FromMinutes(2);
                            }
                            else if (distance == double.MaxValue)
                            {
                                bufferSpan = Utility.ZeroTimeSpan;
                            }
                            else
                            {
                                bufferSpan = Location.getDrivingTimeFromWeb(currentLocation, nextLocation).removeSecondsAndMilliseconds();
                                if (bufferSpan.Ticks >= 0)
                                {
                                    TravelCache.AddOrupdateLocationCache(currentLocation, nextLocation, bufferSpan, Now.constNow, LocationCacheEntry.TravelMedium.driving, distance);
                                }
                            }
                        } else
                        {
                            bufferSpan = cacheEntry.TimeSpan.removeSecondsAndMilliseconds();
                        }     
                    }
                    else
                    {
                        bufferSpan = new TimeSpan(0);
                    }
                }
                else
                {
                    double distance = Location.calculateDistance(currentLocation, nextLocation);
                    cacheEntry = getTravelEntry(currentLocation, nextLocation);
                    if (cacheEntry != null)
                    {
                        bufferSpan = cacheEntry.TimeSpan.removeSecondsAndMilliseconds();
                    }
                }

                if (bufferSpan.Ticks < 0)
                {
                    double distance = SubCalendarEvent.CalculateDistance(myCoEvents.Item1, myCoEvents.Item2, -1);
                    if (distance < .5)
                    {
                        distance = 1;
                    }
                    bufferSpan = new TimeSpan((long)(bufferPerMile.Ticks * distance)).removeSecondsAndMilliseconds();
                }

                mTuple<TimeSpan, TimeSpan> beforeAfter;
                if (retValue.ContainsKey(myCoEvents.Item1)) // index i event
                {
                    beforeAfter = retValue[myCoEvents.Item1];
                    beforeAfter.Item2 = bufferSpan;
                } else
                {
                    beforeAfter = new mTuple<TimeSpan, TimeSpan>(Utility.NegativeTimeSpan, bufferSpan);
                    retValue.Add(myCoEvents.Item1, beforeAfter);
                }

                if (retValue.ContainsKey(myCoEvents.Item2))// index j event
                {
                    beforeAfter = retValue[myCoEvents.Item2];
                    beforeAfter.Item1 = bufferSpan;
                }
                else
                {
                    beforeAfter = new mTuple<TimeSpan, TimeSpan>(bufferSpan, Utility.NegativeTimeSpan);
                    retValue.Add(myCoEvents.Item2, beforeAfter);
                }


                
                if ((!(nextLocation.isNull || nextLocation.isDefault)) && !nextLocationIsLastKnownGood)
                {
                    lastKnownNonDefaultLocation = nextLocation;
                } else if ((!(currentLocation.isNull || currentLocation.isDefault)) && !currentLocationIsLastKnownGood)
                {
                    lastKnownNonDefaultLocation = currentLocation;
                }

            }

            return retValue;
        }


        LocationCacheEntry getTravelEntry(Location locationA, Location locationB)
        {
            LocationCacheEntry entry = null;
            if (_isTravelCacheUpdated)
            {
                entry = TravelCache.getLocationCacheEntry(locationA, locationB, Now.constNow);
            }
            return entry;
        }

        double getHighestOccupancyOfMonth(List<SubCalendarEvent> alleventsWithinMonth, TimeLine monthTimeline)
        {
            TimeLine[] AllWeeks = new TimeLine[4];
            double[] occupancy = new double[AllWeeks.Length];
            DateTimeOffset startOfWeek = monthTimeline.Start;
            for (int i = 0; i < AllWeeks.Length; i++)
            {
                AllWeeks[i] = new TimeLine(startOfWeek, startOfWeek.AddDays(7));
                startOfWeek = AllWeeks[i].End;
            }
            Parallel.For(0, AllWeeks.Length, (Action<int>)(j => {
                long totalDuration = getInterferringSubEvents(AllWeeks[j], alleventsWithinMonth).Sum((Func<SubCalendarEvent, long>)(obj => (long)obj.getActiveDuration.Ticks));
                occupancy[j] = (double)totalDuration / OnewWeekTimeSpan.Ticks;
            }));

            return occupancy.Max();

        }
        


        List<SubCalendarEvent> stitchUnRestrictedSubCalendarEvent(
            TimeLine freeTimeLine, 
            IEnumerable<SubCalendarEvent> PinnedToStartRestrictedEvents,
            IEnumerable<SubCalendarEvent> OtherSubeventsToBeScheduledAroundPinnedToStartRestrictedEvents,
            Tuple<SubCalendarEvent, SubCalendarEvent> BoundaryElements = null
        )
        {
            List<SubCalendarEvent> retValue = new List<SubCalendarEvent>();
            Dictionary<DateTimeOffset, List<SubCalendarEvent>> Deadline_To_MatchingEvents = new Dictionary<DateTimeOffset, List<SubCalendarEvent>>();//Has the deadlines and events with matching deadlines
            Dictionary<TimeSpan, List<SubCalendarEvent>> Timespan_To_MatchingEvents = new Dictionary<TimeSpan, List<SubCalendarEvent>>();
            Dictionary<DateTimeOffset, List<SubCalendarEvent>> DeadLineWithinFreeTime = new Dictionary<DateTimeOffset, List<SubCalendarEvent>>();//populates dictionary of subevents that have deadline ending within
            Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> allPossbileEvents_Nonrestricted = new Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>>();

            SubCalendarEvent initBoundaryElementA = null;
            SubCalendarEvent initBoundaryElementB = null;


            if (BoundaryElements == null)
            {
                BoundaryElements = new Tuple<SubCalendarEvent, SubCalendarEvent>(initBoundaryElementA, initBoundaryElementB);
            }
            else
            {
                initBoundaryElementA = BoundaryElements.Item1;
                initBoundaryElementB = BoundaryElements.Item2;
            }


            SubCalendarEvent rightBoundary = initBoundaryElementB;
            SubCalendarEvent leftBoundary = initBoundaryElementA;

            List<SubCalendarEvent> WillFitSubEvents = OtherSubeventsToBeScheduledAroundPinnedToStartRestrictedEvents.Where(subEvent => subEvent.canExistWithinTimeLine(freeTimeLine)).ToList();

            Dictionary<string, SubCalendarEvent> ID_To_SubEvent_Nonrestricted = new Dictionary<string, SubCalendarEvent>();
            Dictionary<string, SubCalendarEvent> ID_To_SubEvent_Restricted = new Dictionary<string, SubCalendarEvent>();// holds a dictionary of the id of a subevent to subevents(same subevent) that must exist within the timeline
            HashSet<SubCalendarEvent> DistinctSubEvents = new HashSet<SubCalendarEvent>();
            HashSet<SubCalendarEvent> DistinctSubEvents_Restricted = new HashSet<SubCalendarEvent>();
            HashSet<SubCalendarEvent> DistincEvents_NoRestricted = new HashSet<SubCalendarEvent>();
            List<SubCalendarEvent> AllSubCalEvents_NorestrictedValues = WillFitSubEvents.ToList();
            AllSubCalEvents_NorestrictedValues.RemoveAll(obj => PinnedToStartRestrictedEvents.Contains(obj));

            if (AllSubCalEvents_NorestrictedValues.Where(obj => obj.isLocked).Count() > 0)
            {
                throw new Exception("You have a rigid subevent in call to stitchUnRestrictedSubCalendarEvent, shouldn't this be part of PinnedToStartRestrictedEvents");
            }


            foreach (SubCalendarEvent eachSubCalendarEvent in AllSubCalEvents_NorestrictedValues)//populates hashset without restrictedValues
            {
                DistinctSubEvents.Add(eachSubCalendarEvent);
                DistincEvents_NoRestricted.Add(eachSubCalendarEvent);
            }
            foreach (SubCalendarEvent eachSubCalendarEvent in PinnedToStartRestrictedEvents)
            {
                DistinctSubEvents.Add(eachSubCalendarEvent);
                DistinctSubEvents_Restricted.Add(eachSubCalendarEvent);
            }


            foreach (SubCalendarEvent eachSubCalendarEvent in DistinctSubEvents_Restricted)//populates dictionary of deadlines and matching subEvents
            {
                ID_To_SubEvent_Restricted.Add(eachSubCalendarEvent.getId, eachSubCalendarEvent);
            }




            foreach (SubCalendarEvent eachSubCalendarEvent in DistincEvents_NoRestricted)//populates dictionary of deadlines and matching subEvents
            {
                if (Deadline_To_MatchingEvents.ContainsKey(eachSubCalendarEvent.getCalculationRange.End))
                {
                    Deadline_To_MatchingEvents[eachSubCalendarEvent.getCalculationRange.End].Add(eachSubCalendarEvent);
                }
                else
                {
                    Deadline_To_MatchingEvents.Add(eachSubCalendarEvent.getCalculationRange.End, new List<SubCalendarEvent>() { eachSubCalendarEvent });
                }
                if (Timespan_To_MatchingEvents.ContainsKey(eachSubCalendarEvent.getActiveDuration))//populates dictionary of timeSpan and matching subEvents
                {
                    Timespan_To_MatchingEvents[eachSubCalendarEvent.getActiveDuration].Add(eachSubCalendarEvent);
                    allPossbileEvents_Nonrestricted[eachSubCalendarEvent.getActiveDuration].Add(eachSubCalendarEvent.getId, eachSubCalendarEvent);
                }
                else
                {
                    Timespan_To_MatchingEvents.Add(eachSubCalendarEvent.getActiveDuration, new List<SubCalendarEvent>() { eachSubCalendarEvent });
                    allPossbileEvents_Nonrestricted.Add(eachSubCalendarEvent.getActiveDuration, new Dictionary<string, SubCalendarEvent>() { { eachSubCalendarEvent.getId, eachSubCalendarEvent } });
                }

                if (freeTimeLine.IsDateTimeWithin(eachSubCalendarEvent.getCalculationRange.End))//populates elements in which deadline occur within the free timeLine
                {
                    if (DeadLineWithinFreeTime.ContainsKey(eachSubCalendarEvent.getCalculationRange.End))
                    {
                        DeadLineWithinFreeTime[eachSubCalendarEvent.getCalculationRange.End].Add(eachSubCalendarEvent);
                    }
                    else
                    {
                        DeadLineWithinFreeTime.Add(eachSubCalendarEvent.getCalculationRange.End, new List<SubCalendarEvent>() { eachSubCalendarEvent });
                    }
                }
                ID_To_SubEvent_Nonrestricted.Add(eachSubCalendarEvent.getId, eachSubCalendarEvent);
            }






            List<SubCalendarEvent> restricted_EventsOrderedBasedOnDeadline = PinnedToStartRestrictedEvents.OrderByDescending(obj => obj.End).ToList();// be care full of pin to start in the system in reverse restricted
            List<DateTimeOffset> DeadLinesWithinFreeTimeLine = DeadLineWithinFreeTime.Keys.OrderByDescending(obj => obj).ToList();
            List<DateTimeOffset> DeadLinesWithinFreeTimeLine_cpy;
            List<SubCalendarEvent> ID_To_SubEvent_Restricted_List = ID_To_SubEvent_Restricted.OrderByDescending(obj => obj.Value.End).Select(obj => obj.Value).ToList();

            DateTimeOffset restrictedStopper;
            DateTimeOffset EarliestStartTime = freeTimeLine.Start;
            DateTimeOffset EndTime = freeTimeLine.End;
            TimeLine pertinentFreeSpot;
            List<SubCalendarEvent> LowestOrderedElements = new List<SubCalendarEvent>();

            for (int i = 0; ((i < ID_To_SubEvent_Restricted_List.Count) && (i >= 0));)
            {
                SubCalendarEvent restrictedStoppingEvent = ID_To_SubEvent_Restricted_List[i];
                LowestOrderedElements = new List<SubCalendarEvent>();
                restrictedStopper = ID_To_SubEvent_Restricted_List[i].End;
                EarliestStartTime = restrictedStopper;

                DeadLinesWithinFreeTimeLine.RemoveAll(obj => obj > EndTime);//carefull of scenario where you end up assigning an element with a deadline earlier from the preceding iteration which can result in inefficient packing
                if (DeadLinesWithinFreeTimeLine.Count > 0)
                {


                    if (restrictedStopper <= DeadLinesWithinFreeTimeLine[0])
                    {
                        EarliestStartTime = DeadLinesWithinFreeTimeLine[0];
                        DeadLinesWithinFreeTimeLine_cpy = DeadLinesWithinFreeTimeLine.ToList();
                        DeadLinesWithinFreeTimeLine.RemoveAt(0);
                    }
                }

                if (EarliestStartTime > EndTime)
                {
                    throw new Exception("sign of error in Pin to end -1 in Stitchun restricted \"reversed\"");
                }
                pertinentFreeSpot = new TimeLine(EarliestStartTime, EndTime);

                leftBoundary = restricted_EventsOrderedBasedOnDeadline[i];
                BoundaryElements = new Tuple<SubCalendarEvent, SubCalendarEvent>(leftBoundary, rightBoundary);
                LowestOrderedElements = OptimizeArrangeOfSubCalEvent(pertinentFreeSpot, BoundaryElements, allPossbileEvents_Nonrestricted);
                if (!Utility.PinSubEventsToEnd(LowestOrderedElements, pertinentFreeSpot))
                {
                    throw new Exception("error in Pin to end 0 in Stitchun restricted \"reversed\"");
                }


                if (LowestOrderedElements.Count > 0)
                {
                    EndTime = LowestOrderedElements[0].Start;
                }


                TimeLine timelineForFurtherPinning = new TimeLine(restrictedStopper, EndTime);
                List<SubCalendarEvent> reassignableEvents = ID_To_SubEvent_Restricted_List.Concat(allPossbileEvents_Nonrestricted.SelectMany(obj => obj.Value.Select(obj1 => obj1.Value))).Where(obj => !LowestOrderedElements.Contains(obj)).ToList();



                List<TimeLine> timeLinesForFurtherConstrainging = new List<TilerElements.TimeLine>() { timelineForFurtherPinning }; // singleton list. Uses the timeline established by the stopper
                Dictionary<TimeLine, List<SubCalendarEvent>> Dict_ConstrainedElements = generateConstrainedList(timeLinesForFurtherConstrainging, ID_To_SubEvent_Restricted_List);
                Dict_ConstrainedElements = stitchRestrictedSubCalendarEvent(timeLinesForFurtherConstrainging.ToList(), Dict_ConstrainedElements);// calls tries to see if the pinned subevents can be further constrained within the new timeLine
                List<SubCalendarEvent> COnstrainedElementsForTimeLine = Dict_ConstrainedElements[timelineForFurtherPinning];
                if (!Utility.PinSubEventsToStart(Dict_ConstrainedElements[timelineForFurtherPinning], timelineForFurtherPinning))
                {
                    throw new Exception("Error before call to stitchunrestricted");
                }

                reassignableEvents = reassignableEvents.Except(COnstrainedElementsForTimeLine)
                    .Where(subEvent => !subEvent.getIsEventRestricted || subEvent.canExistWithinTimeLine(timelineForFurtherPinning))
                    .ToList();
                List<SubCalendarEvent> reassignedElements;
                if (reassignableEvents.Count > 0)
                {
                    reassignedElements = stitchUnRestrictedSubCalendarEvent(timelineForFurtherPinning, COnstrainedElementsForTimeLine, reassignableEvents);
                }
                else
                {
                    reassignedElements = Dict_ConstrainedElements[timelineForFurtherPinning];
                }

                LowestOrderedElements.InsertRange(0, reassignedElements);
                timelineForFurtherPinning = new TimeLine(restrictedStopper, pertinentFreeSpot.End);//lowest orderelements contains element populated within pertinentFreeSpot and timelineForFurtherPinning. So we need a timeline that encompasses both timelines 
                if (!LowestOrderedElements.Contains(restrictedStoppingEvent))
                {
                    LowestOrderedElements.Insert(0, restrictedStoppingEvent);
                    timelineForFurtherPinning = new TimeLine(restrictedStoppingEvent.Start, pertinentFreeSpot.End);//lowest orderelements contains element populated within pertinentFreeSpot and timelineForFurtherPinning. So we need a timeline that encompasses both timelines and also the Timeline as a result of the restricted stopper
                }

                if (!Utility.PinSubEventsToEnd(LowestOrderedElements, timelineForFurtherPinning))
                {
                    throw new Exception("error in Pin to end 0 in Stitchun restricted \"reversed\"");
                }

                //clearing out already assigned events
                foreach (SubCalendarEvent eachSubCalendarEvent in LowestOrderedElements)
                {
                    ID_To_SubEvent_Nonrestricted.Remove(eachSubCalendarEvent.getId);
                    ID_To_SubEvent_Restricted_List.Remove(eachSubCalendarEvent);
                    if (allPossbileEvents_Nonrestricted.ContainsKey(eachSubCalendarEvent.getActiveDuration))
                    {
                        allPossbileEvents_Nonrestricted[eachSubCalendarEvent.getActiveDuration].Remove(eachSubCalendarEvent.getId);
                        if (allPossbileEvents_Nonrestricted[eachSubCalendarEvent.getActiveDuration].Count == 0)
                        {
                            allPossbileEvents_Nonrestricted.Remove(eachSubCalendarEvent.getActiveDuration);
                        }
                    }
                    DistincEvents_NoRestricted.Remove(eachSubCalendarEvent);

                }
                retValue.InsertRange(0, LowestOrderedElements);
                if (!Utility.PinSubEventsToEnd(retValue, freeTimeLine))
                {
                    throw new Exception("error in Pin to end 2 in Stitchun restricted \"reversed\"");
                }
                SubCalendarEvent startingElement = retValue[0];
                EndTime = startingElement.Start;

                BoundaryElements = new Tuple<SubCalendarEvent, SubCalendarEvent>(leftBoundary, startingElement);
            }
            EarliestStartTime = freeTimeLine.Start;
            pertinentFreeSpot = new TimeLine(freeTimeLine.Start, EndTime);
            List<SubCalendarEvent> DistincEvents_NoRestricted_List = DistincEvents_NoRestricted.ToList();
            ConcurrentBag<SubCalendarEvent> AllPossibleInterferringEvents = new ConcurrentBag<SubCalendarEvent>();

            Parallel.For(0, DistincEvents_NoRestricted.Count, (i, loopState) =>//checks if any element can fit in the remaining timeLine
            {
                if (DistincEvents_NoRestricted_List[i].canExistWithinTimeLine(pertinentFreeSpot))
                {
                    AllPossibleInterferringEvents.Add(DistincEvents_NoRestricted_List[i]);
                }


            });

            if (AllPossibleInterferringEvents.Count > 0)
            {
                SubCalendarEvent LatestDeadline = AllPossibleInterferringEvents.ToList().OrderByDescending(obj => obj.getCalculationRange.End).First();
                BoundaryElements = new Tuple<SubCalendarEvent, SubCalendarEvent>(leftBoundary, BoundaryElements.Item2);
                pertinentFreeSpot = new TimeLine(pertinentFreeSpot.Start, LatestDeadline.getCalculationRange.End <= EndTime ? LatestDeadline.getCalculationRange.End : EndTime);

                LowestOrderedElements = OptimizeArrangeOfSubCalEvent(pertinentFreeSpot, BoundaryElements, allPossbileEvents_Nonrestricted);
                if (!Utility.PinSubEventsToEnd(LowestOrderedElements, pertinentFreeSpot))
                {
                    throw new Exception("error in Pin to end 3 in Stitchun restricted \"reversed\"");
                }


                if (LowestOrderedElements.Count > 0)
                {
                    EndTime = LowestOrderedElements[0].Start;
                }


                Tuple<DateTimeOffset, bool, SubCalendarEvent> possiblebetterTime = ObtainBetterReferencetimeCloseToStar(new TimeLine(EarliestStartTime, EndTime), ID_To_SubEvent_Nonrestricted, ID_To_SubEvent_Restricted, retValue.Concat(LowestOrderedElements).ToList());
                if (possiblebetterTime != null)
                {
                    EndTime = possiblebetterTime.Item1;
                    LowestOrderedElements.Insert(0, (possiblebetterTime.Item3));
                }

                if (!Utility.PinSubEventsToEnd(LowestOrderedElements, pertinentFreeSpot))
                {
                    throw new Exception("error in Pin to end 4 in Stitchun restricted \"reversed\"");
                }

                retValue.InsertRange(0, LowestOrderedElements);
                if (!Utility.PinSubEventsToEnd(retValue, freeTimeLine))
                {
                    throw new Exception("error in Pin to end 4 in Stitchun restricted \"reversed\"");
                }

            }

            return retValue;


        }



        Tuple<DateTimeOffset, bool, SubCalendarEvent> ObtainBetterReferencetimeCloseToStar(TimeLine beforeStopper, Dictionary<string, SubCalendarEvent> AllPossibleEventNonRestricting, Dictionary<string, SubCalendarEvent> restrictedValues, List<SubCalendarEvent> CUrrentlyDesignatedEleemnts)
        {
            Tuple<DateTimeOffset, bool, SubCalendarEvent> retavlue = null;
            List<SubCalendarEvent> AllPossibleEvents = AllPossibleEventNonRestricting.Values.Concat(restrictedValues.Values).ToList();
            AllPossibleEvents.RemoveAll(obj => CUrrentlyDesignatedEleemnts.Contains(obj));
            List<SubCalendarEvent> AllValidEvents = AllPossibleEvents.Where(obj => obj.canExistTowardsEndWithoutSpace(beforeStopper)).ToList();
            AllValidEvents = AllValidEvents.OrderByDescending(obj => obj.getActiveDuration).ToList();
            if (AllValidEvents.Count > 0)
            {

                SubCalendarEvent refSubCalevent = AllValidEvents[0];
                refSubCalevent.PinToEnd(beforeStopper);
                retavlue = new Tuple<DateTimeOffset, bool, SubCalendarEvent>(refSubCalevent.Start, false, refSubCalevent);
                if (restrictedValues.ContainsKey(refSubCalevent.getId))
                {
                    retavlue = new Tuple<DateTimeOffset, bool, SubCalendarEvent>(refSubCalevent.Start, true, refSubCalevent);
                }
            }
            else
            {
                AllValidEvents = AllPossibleEvents.Where(obj => obj.canExistWithinTimeLine(beforeStopper)).ToList();
                AllValidEvents = AllValidEvents.OrderByDescending(obj => obj.getCalculationRange.End).ToList();//we chose end because we want to select event that has the latest deadline. This allows for the "beforestopper" variable to be used up as soon as possible, from the end. Since there is a likely hood that it will can be filled up in the next time line query in stitch unrestricted
                if (AllValidEvents.Count > 0)
                {
                    SubCalendarEvent refSubCalevent = AllValidEvents[0];
                    refSubCalevent.PinToEnd(beforeStopper);
                    retavlue = new Tuple<DateTimeOffset, bool, SubCalendarEvent>(refSubCalevent.Start, false, refSubCalevent);
                    if (restrictedValues.ContainsKey(refSubCalevent.getId))
                    {
                        retavlue = new Tuple<DateTimeOffset, bool, SubCalendarEvent>(refSubCalevent.Start, true, refSubCalevent);
                    }
                }

            }
            return retavlue;
        }

        List<SubCalendarEvent> OptimizeArrangeOfSubCalEvent(TimeLine PertinentFreeSpot, Tuple<SubCalendarEvent, SubCalendarEvent> BoundaryCalendarEvent, Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> PossibleEntries_Cpy, double occupancy = 0)
        {
            List<mTuple<int, TimeSpanWithStringID>> CompatibleWithList = new List<mTuple<int, TimeSpanWithStringID>>();
            Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> PossibleSubCalEvents = removeSubCalEventsThatCantWorkWithTimeLine(PertinentFreeSpot, PossibleEntries_Cpy);
            Dictionary<DateTimeOffset, Dictionary<TimeSpan, int>> DeadLineTODuration = new Dictionary<DateTimeOffset, Dictionary<TimeSpan, int>>();
            foreach (KeyValuePair<TimeSpan, Dictionary<string, SubCalendarEvent>> eachKeyValuePair in PossibleSubCalEvents)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                CompatibleWithList.Add(new mTuple<int, TimeSpanWithStringID>(eachKeyValuePair.Value.Count, new TimeSpanWithStringID(eachKeyValuePair.Value.ToList()[0].Value.getActiveDuration, eachKeyValuePair.Key.Ticks.ToString())));

                foreach (SubCalendarEvent eachSubcalevent in eachKeyValuePair.Value.Values)
                {
                    DateTimeOffset endTime = eachSubcalevent.getCalculationRange.End;
                    if (DeadLineTODuration.ContainsKey(endTime))
                    {
                        if (DeadLineTODuration[endTime].ContainsKey(eachSubcalevent.getActiveDuration))
                        {
                            ++DeadLineTODuration[endTime][eachSubcalevent.getActiveDuration];
                        }
                        else
                        {
                            DeadLineTODuration[endTime].Add(eachSubcalevent.getActiveDuration, 1);
                        }
                    }
                    else
                    {
                        DeadLineTODuration.Add(endTime, new Dictionary<TimeSpan, int>());
                        DeadLineTODuration[endTime].Add(eachSubcalevent.getActiveDuration, 1);

                    }

                    DeadLineTODuration[endTime].OrderBy(obj => obj);
                }

            }


            SnugArray BestFit_beforeBreak = new SnugArray(CompatibleWithList, PertinentFreeSpot.TimelineSpan);
            TimeSpan AverageTimeSpan = new TimeSpan((long)(occupancy * (double)PertinentFreeSpot.TimelineSpan.Ticks));
            List<SubCalendarEvent> LowestCostArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AllPossibleBestFit_beforeBreak = BestFit_beforeBreak.MySnugPossibleEntries;
            AllPossibleBestFit_beforeBreak = SnugArray.SortListSnugPossibilities_basedOnTimeSpan(AllPossibleBestFit_beforeBreak, new TimeSpanWithStringID(AverageTimeSpan, AverageTimeSpan.Ticks.ToString()));
            AllPossibleBestFit_beforeBreak.Reverse();
            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> var3_beforeBreak = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
            if (AllPossibleBestFit_beforeBreak.Count > 0)
            {
                var3_beforeBreak.Add(AllPossibleBestFit_beforeBreak);
                List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AveragedBestFit = OptimizeForDeadLine(DeadLineTODuration, PertinentFreeSpot.TimelineSpan);
                Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> removedImpossibleValue = removeSubCalEventsThatCantWorkWithTimeLine(PertinentFreeSpot, PossibleSubCalEvents);
                LowestCostArrangement = generateCombinationForDifferentEntries_NoMtuple(AveragedBestFit[0], removedImpossibleValue).FirstOrDefault(subEventOrder => Utility.PinSubEventsToEnd(subEventOrder, PertinentFreeSpot));
                if (LowestCostArrangement == null)
                {
                    LowestCostArrangement = new List<SubCalendarEvent>();
                }
            }

            return LowestCostArrangement;

        }

        List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> OptimizeForDeadLine(IEnumerable<KeyValuePair<DateTimeOffset, Dictionary<TimeSpan, int>>> DeadLinePreference, TimeSpan CurrentFreeSpace, bool Aggressive = true)
        {
            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> retValue = new List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();
            HashSet<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> qualifiesForNextStage = new HashSet<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();
            HashSet<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AggressiveSet = new HashSet<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();
            IEnumerable<KeyValuePair<DateTimeOffset, Dictionary<TimeSpan, int>>> AllBestFitOptions_IEnu = DeadLinePreference;
            IEnumerable<KeyValuePair<DateTimeOffset, Dictionary<TimeSpan, int>>> DeadLinePreference_Qualified;
            HashSet<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> forNextLevel = new HashSet<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();

            AllBestFitOptions_IEnu = AllBestFitOptions_IEnu.OrderBy(obj => obj.Key);
            //AllBestFitOptions_IEnu.Reverse();
            bool iniAggressive = Aggressive;
            foreach (KeyValuePair<DateTimeOffset, Dictionary<TimeSpan, int>> eachKeyValuePair in AllBestFitOptions_IEnu)
            {
                //IEnumerable<KeyValuePair<TimeSpan, int>> AllTimeSpan = eachKeyValuePair.Value.OrderBy(obj => obj);
                IEnumerable<mTuple<int, TimeSpanWithStringID>> AllTimeSpan = eachKeyValuePair.Value.Select(obj => new mTuple<int, TimeSpanWithStringID>(obj.Value, new TimeSpanWithStringID(obj.Key, obj.Key.Ticks.ToString())));
                SnugArray BestFit_OfDeadline = new SnugArray(AllTimeSpan.ToList(), CurrentFreeSpace);
                List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AllPossibleBestFit_beforeBreak = SnugArray.SortListSnugPossibilities_basedOnTimeSpan(BestFit_OfDeadline.MySnugPossibleEntries);

                AllPossibleBestFit_beforeBreak.Reverse();

                if (AllPossibleBestFit_beforeBreak.Count > 0)
                {
                    Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> TightestConfiguration = AllPossibleBestFit_beforeBreak[0];
                    retValue.Add(TightestConfiguration);
                    TimeSpan UsedUpSpace = SnugArray.TotalTimeSpanOfSnugPossibility(TightestConfiguration);
                    CurrentFreeSpace -= UsedUpSpace;
                    DeadLinePreference_Qualified = AllBestFitOptions_IEnu.Where(obj => obj.Value != eachKeyValuePair.Value);
                    List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> furtherCalls = OptimizeForDeadLine(DeadLinePreference_Qualified, CurrentFreeSpace, Aggressive);
                    if (furtherCalls.Count > 0)
                    {
                        foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in furtherCalls)
                        {
                            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair0 in TightestConfiguration)
                            {
                                if (eachDictionary.ContainsKey(eachKeyValuePair0.Key))
                                {
                                    eachDictionary[eachKeyValuePair0.Key].Item1 += eachKeyValuePair0.Value.Item1;
                                }
                                else
                                {
                                    eachDictionary.Add(eachKeyValuePair0.Key, eachKeyValuePair0.Value);
                                }
                            }
                        }
                    }
                    else
                    {
                        furtherCalls.Add(TightestConfiguration);
                    }

                    return furtherCalls;
                }
            }

            return retValue;
        }

        Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> removeSubCalEventsThatCantWorkWithTimeLine(TimeLine PertinentFreeSpot, Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> PossibleEntries, bool MustFitAnyWhere = false)
        {
            /*
             * THis funcction checks if the active duration of each subcalevent in PossibleEntries can fit within the PertinentFreeSpot. Also the it checks if PertinentFreeSpot breaks the enclosing timeLine for the currently echeckcked subcalendar event. if both conditions are satisfied the subcalevent gets inserted into the retvalue
             * MustFitAnyWhere variable checks if the subcalevent can exist any where within the PertinentFreeSpot timeline, as well as verify the steps above. Its default value is false i.e it checks if the full subcalevent can exist in part of the pertinentfreespot
             
             */

            List<SubCalendarEvent> retValueList = new System.Collections.Generic.List<SubCalendarEvent>();
            Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> retValue = new Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>>();


            foreach (KeyValuePair<TimeSpan, Dictionary<string, SubCalendarEvent>> eachKeyValuePair in PossibleEntries)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                Dictionary<string, mTuple<bool, SubCalendarEvent>> UpdatedEntries = new System.Collections.Generic.Dictionary<string, mTuple<bool, SubCalendarEvent>>();

                foreach (KeyValuePair<string, SubCalendarEvent> eachKeyValuePair0 in eachKeyValuePair.Value)
                {
                    if (MustFitAnyWhere)
                    {
                        if (eachKeyValuePair0.Value.getCalculationRange.IsTimeLineWithin(PertinentFreeSpot) && (eachKeyValuePair0.Value.getActiveDuration <= PertinentFreeSpot.TimelineSpan))
                        {
                            retValueList.Add(eachKeyValuePair0.Value);
                        }
                    }
                    else
                    {
                        if (eachKeyValuePair0.Value.canExistWithinTimeLine(PertinentFreeSpot))
                        {
                            retValueList.Add(eachKeyValuePair0.Value);
                        }
                    }
                }
            }

            foreach (SubCalendarEvent eachmTuple in retValueList)
            {
                if (retValue.ContainsKey(eachmTuple.getActiveDuration))
                {
                    retValue[eachmTuple.getActiveDuration].Add(eachmTuple.getId, eachmTuple);
                }
                else
                {
                    retValue.Add(eachmTuple.getActiveDuration, new Dictionary<string, SubCalendarEvent>());
                    retValue[eachmTuple.getActiveDuration].Add(eachmTuple.getId, eachmTuple);
                }
            }


            return retValue;


        }

        List<SubCalendarEvent> getArrangementWithLowestDistanceCost(List<List<SubCalendarEvent>> viableCombinations, Tuple<SubCalendarEvent, SubCalendarEvent> BoundinngSubCaEvents)
        {
            return viableCombinations.First();
        }

        double calculateCostOSubCalArrangement(List<SubCalendarEvent> CurrentArrangementOfSubcalevent)
        {
            int i = 0;
            double retValue = 0;



            for (; i < CurrentArrangementOfSubcalevent.Count - 1; i++)
            {
                retValue += SubCalendarEvent.CalculateDistance(CurrentArrangementOfSubcalevent[i], CurrentArrangementOfSubcalevent[i + 1]);
            }

            return retValue;
        }

        Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> useAggressivePossibilitiesEntry(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithList, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries)
        {

            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> retValue = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();
            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in CompatibleWithList)
            {
                IEnumerable<KeyValuePair<string, mTuple<bool, SubCalendarEvent>>> possibleEntries_IEnu = PossibleEntries[eachKeyValuePair.Key];
                possibleEntries_IEnu = possibleEntries_IEnu.OrderBy(obj => obj.Value.Item2.getCalculationRange.End);
                //                //IEnumerable<int> AllIndexesWithValidEndtime = 

                possibleEntries_IEnu = possibleEntries_IEnu.Where((obj, index) => index < eachKeyValuePair.Value.Item1); //keeps looping as long as index is less than eachKeyValuePair.value.item1
                retValue.Add(eachKeyValuePair.Key, possibleEntries_IEnu.ToDictionary(obj => obj.Key, obj => obj.Value));
            }

            return retValue;
        }

        Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> useAggressivePossibilitiesEntry_NoMtuple(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithList, Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> PossibleEntries)
        {

            Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> retValue = new Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>>();
            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in CompatibleWithList)
            {
                IEnumerable<KeyValuePair<string, SubCalendarEvent>> PossibleEntries_IEnu = PossibleEntries[eachKeyValuePair.Key];
                PossibleEntries_IEnu = PossibleEntries_IEnu.OrderBy(obj => obj.Value.getCalculationRange.End);
                //                //IEnumerable<int> AllIndexesWithValidEndtime = 

                PossibleEntries_IEnu = PossibleEntries_IEnu.Where((obj, index) => index < eachKeyValuePair.Value.Item1); //keeps looping as long as index is less than eachKeyValuePair.value.item1
                retValue.Add(eachKeyValuePair.Key, PossibleEntries_IEnu.ToDictionary(obj => obj.Key, obj => obj.Value));
            }

            return retValue;
        }


        static public int CountCall = 0;
        List<List<SubCalendarEvent>> generateCombinationForDifferentEntries_NoMtuple(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithList, Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> PossibleEntries, bool Aggressive = true)
        {
            /*
             * Function attempts to generate multiple combinations of compatible sub calendar event for Snug fit entry
             * CompatibleWithList is an snug fit result
             * PossibleEntries are the possible sub calendar that can be used in the combinatorial result
             */
            List<List<List<string>>> MAtrixedSet = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<string>>>();
            Dictionary<string, mTuple<int, List<SubCalendarEvent>>> var4 = new System.Collections.Generic.Dictionary<string, mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>>();
            List<List<SubCalendarEvent>> retValue = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();

            if (PossibleEntries.Count > 0)
            {
                if (Aggressive)
                {
                    PossibleEntries = useAggressivePossibilitiesEntry_NoMtuple(CompatibleWithList, PossibleEntries);

                }
                foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair0 in CompatibleWithList)//loops every timespan in Snug FIt possibility
                {
                    TimeSpan eachTimeSpan = eachKeyValuePair0.Key;

                    Dictionary<string, SubCalendarEvent> var1 = PossibleEntries[eachTimeSpan];
                    List<List<string>> receivedValue = new System.Collections.Generic.List<System.Collections.Generic.List<string>>();
                    Dictionary<string, int> var2 = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (KeyValuePair<string, SubCalendarEvent> eachKeyValuePair in var1)
                    {
                        string ParentID = eachKeyValuePair.Value.SubEvent_ID.getIDUpToCalendarEvent();
                        if (var2.ContainsKey(ParentID))
                        {
                            ++var2[ParentID];
                            var4[ParentID].Item2.Add(eachKeyValuePair.Value);
                        }
                        else
                        {
                            var2.Add(ParentID, 1);
                            List<SubCalendarEvent> var5 = new System.Collections.Generic.List<SubCalendarEvent>();
                            var5.Add(eachKeyValuePair.Value);
                            var4.Add(ParentID, new mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>(0, var5));
                        }
                    }
                    List<mTuple<string, int>> PossibleCalEvents = new System.Collections.Generic.List<mTuple<string, int>>();
                    foreach (KeyValuePair<string, int> eachKeyValuePair in var2)
                    {
                        PossibleCalEvents.Add(new mTuple<string, int>(eachKeyValuePair.Key, eachKeyValuePair.Value));
                    }

                    List<List<string>> var3 = generateCombinationForSpecficTimeSpanStringID(eachKeyValuePair0.Value.Item1, PossibleCalEvents);
                    MAtrixedSet.Add(var3);
                }

                List<List<string>> serializedList = Utility.SerializeList(MAtrixedSet);
                Dictionary<TimeSpan, long> TimeSpanOfEventToTotalDeadlineDateTime = new Dictionary<TimeSpan, long>();

                foreach (List<string> eachList in serializedList)//serializedList has a list of fittable ParentIDs, the loop replaces each List of strings with List of subCalendarEvents
                {
                    List<SubCalendarEvent> var6 = new System.Collections.Generic.List<SubCalendarEvent>();
                    mTuple<int, List<SubCalendarEvent>> var7 = new mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>(0, new System.Collections.Generic.List<SubCalendarEvent>());
                    foreach (string eachString in eachList)
                    {
                        var7 = var4[eachString];
                        var6.Add(var7.Item2[var7.Item1++]);
                    }
                    foreach (KeyValuePair<string, mTuple<int, List<SubCalendarEvent>>> eachKeyValuePair in var4)
                    {
                        eachKeyValuePair.Value.Item1 = 0;
                    }

                    TimeSpan TotalActiveDuration = Utility.SumOfActiveDuration(var6);


                    retValue.Add(var6);
                }

                return retValue;
            }
            else
            {
                return retValue;
            }
        }

        List<List<string>> generateCombinationForSpecficTimeSpanStringID(int Count, List<mTuple<string, int>> PossibleCalEvents)
        {
            int CountCpy = Count;

            int i = 0;
            List<List<string>> retValue = new System.Collections.Generic.List<System.Collections.Generic.List<string>>();
            if (Count == 0)
            {
                return retValue;
            }
            for (; i < PossibleCalEvents.Count; i++)
            {

                List<mTuple<string, int>> PossibleCalEvents_Param = PossibleCalEvents.ToList();
                PossibleCalEvents_Param[i] = new mTuple<string, int>(PossibleCalEvents_Param[i].Item1, PossibleCalEvents_Param[i].Item2);
                mTuple<string, int> refSubCalEventUmbrella = PossibleCalEvents_Param[i];
                //refSubCalEventUmbrella = 

                if (refSubCalEventUmbrella.Item2 > 0)
                {
                    --refSubCalEventUmbrella.Item2;

                    List<List<string>> receivedCombination = generateCombinationForSpecficTimeSpanStringID(CountCpy - 1, PossibleCalEvents_Param);
                    foreach (List<string> eachList in receivedCombination)
                    {
                        eachList.Add(refSubCalEventUmbrella.Item1);
                    }
                    if (receivedCombination.Count < 1)
                    {
                        receivedCombination.Add(new System.Collections.Generic.List<string>() { refSubCalEventUmbrella.Item1 });
                    }
                    retValue.AddRange(receivedCombination);
                }
                PossibleCalEvents.RemoveAt(i);
                --i;
            }
            return retValue;


        }



        public List<mTuple<bool, SubCalendarEvent>> stitchRestrictedSubCalendarEvent(List<mTuple<bool, SubCalendarEvent>> Arg1, TimeLine RestrictingTimeLine, SubCalendarEvent PrecedingPivot = null)
        {
            List<SubCalendarEvent> retValue = stitchRestrictedSubCalendarEvent(Arg1.Select(obj => new mTuple<TimeLine, SubCalendarEvent>(new TimeLine(), obj.Item2)).ToList(), RestrictingTimeLine, PrecedingPivot);
            return retValue.Select(obj => new mTuple<bool, SubCalendarEvent>(true, obj)).ToList();
        }

        public List<SubCalendarEvent> stitchRestrictedSubCalendarEvent(List<mTuple<TimeLine, SubCalendarEvent>> Arg1, TimeLine RestrictingTimeLine, SubCalendarEvent PrecedingPivot = null)
        {
            List<SubCalendarEvent> retValue = Arg1.Select(obj => obj.Item2).ToList();

            TimeSpan SumOfAllSubCalEvent = Utility.SumOfActiveDuration(Arg1.Select(obj => obj.Item2));
            List<SubCalendarEvent> CopyOfAllList = Arg1.Select(obj => obj.Item2).ToList();
            if (retValue.Count < 1)//if arg1 is empty return the list
            {
                return retValue;
            }
            List<SubCalendarEvent> AllSubCalEvents = Arg1.Select(obj => obj.Item2).ToList();
            List<mTuple<TimeLine, SubCalendarEvent>> AvaialableTimeSpan = new List<mTuple<TimeLine, SubCalendarEvent>>();

            TimeSpan SmallestAssignedTimeSpan = new TimeSpan(3650, 0, 0, 0);//sets the smallest TimeSpan To 10 years
            DateTimeOffset SmallestDateTime = new DateTimeOffset(3000, 12, 31, 0, 0, 0, new TimeSpan());


            Arg1 = Arg1.Select(obj => //using Linq to update timeline
            {
                List<TimeLine> AllTimeLines = obj.Item2.getTimeLinesInterferringWithCalculationRange(RestrictingTimeLine);
                TimeLine relevantTimeLine = AllTimeLines != null ? AllTimeLines[0] : null;
                obj.Item1 = relevantTimeLine;//gets interferring TImeLine
                return obj;
            }).ToList();
            Arg1 = Arg1.OrderBy(obj => obj.Item2.Score).ToList();
            List<mTuple<TimeLine, SubCalendarEvent>> WorkableList = Arg1.Where(obj => obj.Item1 != null).ToList();
            WorkableList = WorkableList.Where(obj => obj.Item1.TimelineSpan >= obj.Item2.getActiveDuration).ToList();

#if reversed
            //Build Strict Towards right of the tree
            if (WorkableList.Count > 0)
            {
                //CountCall++;
                mTuple<TimeLine, SubCalendarEvent> PivotNodeData = WorkableList[0];
                bool PinningSuccess = PivotNodeData.Item2.PinToEnd(RestrictingTimeLine);
                DateTimeOffset StartTimeOfLeftTree = RestrictingTimeLine.Start;
                DateTimeOffset EndTimeOfLeftTree = RestrictingTimeLine.End;
                SubCalendarEvent includeSubCalendarEvent = null;
                TimeLine leftOver = new TimeLine();
                if (PinningSuccess)//hack alert Subevent fittable a double less than 1 e,g 0.5
                {
                    EndTimeOfLeftTree = PivotNodeData.Item2.Start;
                    includeSubCalendarEvent = PivotNodeData.Item2;
                    leftOver = new TimeLine(PivotNodeData.Item2.End, RestrictingTimeLine.End);//gets the left of timeline to ensure that 
                }

                WorkableList.RemoveAt(0);

                TimeLine LefTimeLine = new TimeLine(StartTimeOfLeftTree, EndTimeOfLeftTree);

                List<mTuple<TimeLine, SubCalendarEvent>> WorkableList_Cpy = WorkableList.ToList();

                List<SubCalendarEvent> willFitInleftOver = WorkableList.Where(obj => obj.Item2.canExistWithinTimeLine(leftOver)).Select(obj => obj.Item2).ToList();

                SubCalendarEvent.incrementMiscdata(willFitInleftOver);

                List<SubCalendarEvent> leftTreeResult = stitchRestrictedSubCalendarEvent(WorkableList, LefTimeLine);
                if (includeSubCalendarEvent != null)
                {
                    leftTreeResult.Add(includeSubCalendarEvent);
                }

                if (!Utility.PinSubEventsToStart(leftTreeResult, RestrictingTimeLine))
                {
                    throw new Exception("oops jerome check reveresed section of stitchrestricted left treeresult ");
                };

                //Build Strict Towards Left of the tree
                DateTimeOffset StartTimeOfRightTree = RestrictingTimeLine.Start;
                DateTimeOffset EndTimeOfRightTree = RestrictingTimeLine.End;
                if (leftTreeResult.Count > 0)
                {
                    StartTimeOfRightTree = leftTreeResult.Last().End;
                }

                TimeLine RightTimeLine = new TimeLine(StartTimeOfRightTree, EndTimeOfRightTree);

                WorkableList_Cpy.RemoveAll(obj => leftTreeResult.Contains(obj.Item2));




                List<SubCalendarEvent> RightTreeResult = stitchRestrictedSubCalendarEvent(WorkableList_Cpy, RightTimeLine);
                retValue = leftTreeResult.Concat(RightTreeResult).ToList();
                if (!Utility.PinSubEventsToStart(retValue, RestrictingTimeLine))
                {
                    throw new Exception("oops jerome check reveresed section of stitchrestricted right treeresult ");
                    ;
                }

                AllSubCalEvents.RemoveAll(obj => retValue.Contains(obj));
                SubCalendarEvent.decrementMiscdata(AllSubCalEvents.Intersect(willFitInleftOver).ToList());

                //SubCalendarEvent.decrementMiscdata(Arg1.Select(obj => obj.Item2.Item2).ToList());
            }
            else //if there are no feasible TimeLine that are withing RestrictingTimeLine that can also contain a subcalevent
            {
                return new List<SubCalendarEvent>();
            }
            return retValue;
#else

            //Build Strict Towards right of the tree
            if (WorkableList.Count > 0)
            {
                mTuple<double, mTuple<TimeLine, SubCalendarEvent>> PivotNodeData = WorkableList[0];


                bool PinningSuccess= PivotNodeData.Item2.Item2.PinSubEventsToStart(RestrictingTimeLine);
                DateTimeOffset StartTimeOfRightTree = RestrictingTimeLine.Start;
                DateTimeOffset EndTimeOfRightTree = RestrictingTimeLine.End;
                SubCalendarEvent includentSubCakendarEvent=null;
                TimeLine leftOver = new TimeLine();
                if (PinningSuccess)//hack alert Subevent fittable a double less than 1 e,g 0.5
                {
                    StartTimeOfRightTree = PivotNodeData.Item2.Item2.End;
                    includentSubCakendarEvent = PivotNodeData.Item2.Item2;
                    leftOver = new TimeLine(RestrictingTimeLine.Start, PivotNodeData.Item2.Item2.Start);//gets the left of timeline to ensure that 
                }

                WorkableList.RemoveAt(0);

                TimeLine RightTimeLine = new TimeLine(StartTimeOfRightTree, EndTimeOfRightTree);

                List<mTuple<double, mTuple<TimeLine, SubCalendarEvent>>> WorkableList_Cpy = WorkableList.ToList();

                List<SubCalendarEvent>willFitInleftOver= WorkableList.Where(obj => obj.Item2.Item2.canExistWithinTimeLine(leftOver)).Select(obj=>obj.Item2.Item2).ToList();

                SubCalendarEvent.incrementMiscdata(willFitInleftOver);

                List<SubCalendarEvent> rightTreeResult = stitchRestrictedSubCalendarEvent(WorkableList, RightTimeLine);
                if(includentSubCakendarEvent!=null)
                {
                    rightTreeResult.Insert(0, includentSubCakendarEvent);
                }

                if (!Utility.PinSubEventsToEnd(rightTreeResult, RestrictingTimeLine))
                {
                    ;
                };

                //Build Strict Towards Left of the tree
                DateTimeOffset StartTimeOfleftTree = RestrictingTimeLine.Start;
                DateTimeOffset EndTimeOfLeftTree = RestrictingTimeLine.End;
                if (rightTreeResult.Count > 0)
                {
                    EndTimeOfLeftTree = rightTreeResult[0].Start;
                }

                TimeLine LeftTimeLine = new TimeLine(StartTimeOfleftTree, EndTimeOfLeftTree);

                WorkableList_Cpy.RemoveAll(obj => rightTreeResult.Contains(obj.Item2.Item2));




                List<SubCalendarEvent> LeftTreeResult = stitchRestrictedSubCalendarEvent(WorkableList_Cpy, LeftTimeLine);
                retValue = LeftTreeResult.Concat(rightTreeResult).ToList();
                Utility.PinSubEventsToEnd(retValue, RestrictingTimeLine);

                AllSubCalEvents.RemoveAll(obj => retValue.Contains(obj));
                SubCalendarEvent.decrementMiscdata(AllSubCalEvents.Intersect(willFitInleftOver).ToList());
                
                //SubCalendarEvent.decrementMiscdata(Arg1.Select(obj => obj.Item2.Item2).ToList());
            }
            else //if there are no feasible TimeLine that are withing RestrictingTimeLine that can also contain a subcalevent
            {
                return new List<SubCalendarEvent>();
            }
            return retValue;
#endif
        }


        Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> stitchRestrictedSubCalendarEvent(List<TimeLine> AllTimeLines, int TimeLineIndex, Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> arg1)
        {
            /*
             * arg1 is adictionary that has a timeline witha a list of restricted Sub calendar events             
             */
            List<SubCalendarEvent> AllSubCall = arg1.Values.SelectMany(obj => obj.Select(obj1 => obj1.Item2)).ToList();
            SubCalendarEvent.updateMiscData(AllSubCall, 0);

            Dictionary<string, bool> var1 = new System.Collections.Generic.Dictionary<string, bool>();
            foreach (TimeLine eachTimeLine in AllTimeLines)//checks of subcal can exist within time frame increases the intData for every TimeLine it can fit in
            {
                SubCalendarEvent.incrementMiscdata(AllSubCall.Where(obj => obj.canExistWithinTimeLine(eachTimeLine)).ToList());
                arg1[eachTimeLine] = stitchRestrictedSubCalendarEvent(arg1[eachTimeLine], eachTimeLine);
            }



            foreach (TimeLine eachTimeLine in AllTimeLines)
            {
                arg1[eachTimeLine] = stitchRestrictedSubCalendarEvent(arg1[eachTimeLine], eachTimeLine);
            }
            SubCalendarEvent.updateMiscData(AllSubCall, 0);
            return arg1;
        }

        List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> generateTreeCallsToSnugArray(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> AvailableSubCalendarEvents, List<TimeLine> AllTimeLines, int TimeLineIndex, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar, Dictionary<TimeLine, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> DictionaryOfTimelineAndSubcalendarEvents, Dictionary<TimeLine, Tuple<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> DictionaryOfTimelineAndConstrainedElements)//, List<SubCalendarEvent> usedSubCalendarEvensts)
        {
            /*
                * Name: jerome Biotidara
                * Description: This function is responsible for generating the snug possibilities for a list containing all timelines it takes 5 elements as its parameters. 
                    -AvailableSubCalendarEvents ->The remaining List of subcalendar events. 
                    -AllTimeLines->A list of the Timelines that needto be populated with the desired snug possibility
                    -TimeLineIndex->An Index keeping track of what timeline the funciton is current working on
                    -FullTimeLineWithSnugListOfSubcalendarEvents->is a list containing A list of subcalendarEvents for each TimeLine
             *      -PertainingSnugPossibilityForTimieline-> is a list of snugpossibilities that apply to a TimeLine in a TimeLineIndex
             *      -DictionaryOfTimelineAndSubcalendarEvents-> A dictionary of TimeLine and interferring subcalendarevents
             */

            if (AvailableSubCalendarEvents.Count > MaxNumberOfInterferringSubcalEvents)
            {
                MaxNumberOfInterferringSubcalEvents = AvailableSubCalendarEvents.Count;
            }
            if (TimeLineIndex > LargestTimeIndex)
            {
                LargestTimeIndex = TimeLineIndex;
            }

            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> ListOfAllSnugPossibilitiesInRespectiveTImeLines = new List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
            if (AllTimeLines.Count < 1)
            {
                return null;
            }

            if ((TimeLineIndex >= AllTimeLines.Count) && (AvailableSubCalendarEvents.Count > 0))
            {
                LogInfo += ("Couldnt Find For this timeLine timeLine\n");
            }

            if ((TimeLineIndex >= AllTimeLines.Count) || (AvailableSubCalendarEvents.Count < 1))
            {
                if (TimeLineIndex < AllTimeLines.Count)
                {
                    LogInfo += ("Weird Exit\n");
                }

                if (AvailableSubCalendarEvents.Count < 1)
                {
                    ;
                }





                ListOfAllSnugPossibilitiesInRespectiveTImeLines.Add(SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar);
                return ListOfAllSnugPossibilitiesInRespectiveTImeLines;
            }


            int TOtalEvents = 0;



            if ((TimeLineIndex == 13) || (TimeLineIndex == 13))
            {

                //foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar)
                {
                    foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in AvailableSubCalendarEvents)
                    {
                        TOtalEvents += eachKeyValuePair.Value.Item1;
                    }
                }
                if (TOtalEvents == 1)
                {
                    ;
                }
            }

            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> MyPertinentSubcalendarEvents;
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CantBeUsedInCurrentTimeLine = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> RestrictedToThisTimeLineElements = DictionaryOfTimelineAndConstrainedElements[AllTimeLines[TimeLineIndex]].Item1;



            TimeSpan TotalTimeUsedUpByConstrained = new TimeSpan(0);

            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in RestrictedToThisTimeLineElements)//HACK THIS does not take into account two clashing restricted events
            {
                int CantBeUsedCount = DictionaryOfTimelineAndConstrainedElements[AllTimeLines[TimeLineIndex]].Item2[eachKeyValuePair.Key].Item1 - RestrictedToThisTimeLineElements[eachKeyValuePair.Key].Item1;

                CantBeUsedInCurrentTimeLine.Add(eachKeyValuePair.Key, new mTuple<int, TimeSpanWithStringID>(CantBeUsedCount, eachKeyValuePair.Value.Item2));
                AvailableSubCalendarEvents[eachKeyValuePair.Key].Item1 -= CantBeUsedCount;
                //AvailableSubCalendarEvents[eachKeyValuePair.Key].Item1 -= RestrictedToThisTimeLineElements[eachKeyValuePair.Key].Item1;
                //DictionaryOfTimelineAndSubcalendarEvents[AllTimeLines[TimeLineIndex]][eachKeyValuePair.Key].Item1 -= RestrictedToThisTimeLineElements[eachKeyValuePair.Key].Item1;


                //AvailableSubCalendarEvents[eachKeyValuePair.Key].Item1 -= AvailableSubCalendarEventsCountReduction;


                ;

            }

            MyPertinentSubcalendarEvents = Utility.ListIntersection(AvailableSubCalendarEvents, DictionaryOfTimelineAndSubcalendarEvents[AllTimeLines[TimeLineIndex]]);
            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in RestrictedToThisTimeLineElements)
            {

                MyPertinentSubcalendarEvents[eachKeyValuePair.Key].Item1 -= eachKeyValuePair.Value.Item1;
                TotalTimeUsedUpByConstrained += (TimeSpan.FromTicks(eachKeyValuePair.Value.Item2.timeSpan.Ticks * eachKeyValuePair.Value.Item1));
            }
            List<mTuple<int, TimeSpanWithStringID>> ListOfTimeSpanWithID_WithCounts = MyPertinentSubcalendarEvents.Values.ToList();


            SnugArray MySnugArray = new SnugArray(ListOfTimeSpanWithID_WithCounts, AllTimeLines[TimeLineIndex].TimelineSpan - TotalTimeUsedUpByConstrained);
            //SnugArray MySnugArray = new SnugArray(ConstrainedMySubcalendarEventTimespans, MySubcalendarEventTimespans, AllTimeLines[TimeLineIndex].TimelineSpan);



            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AllSnugPossibilities = MySnugArray.MySnugPossibleEntries;

            AllSnugPossibilities = SnugArray.SortListSnugPossibilities_basedOnTimeSpan(AllSnugPossibilities);
            AllSnugPossibilities.Reverse();
            if (AllSnugPossibilities.Count > 1)
            {
                AllSnugPossibilities.RemoveRange(1, (AllSnugPossibilities.Count - 1));
            }

            foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in AllSnugPossibilities)
            {
                /*if ((TimeLineIndex ==5)||(TimeLineIndex ==9))
                {
                    if (eachDictionary.ContainsKey("90000000000"))
                    {
                        if (eachDictionary["90000000000"].Item1 >= 2)
                        {
                            ;
                        }
                    }
                }*/

                foreach (TimeSpan eachTimeSpan in RestrictedToThisTimeLineElements.Keys)
                {
                    if (eachDictionary.ContainsKey(eachTimeSpan))
                    {
                        eachDictionary[eachTimeSpan].Item1 += RestrictedToThisTimeLineElements[eachTimeSpan].Item1;
                    }
                    else
                    {
                        eachDictionary.Add(eachTimeSpan, new mTuple<int, TimeSpanWithStringID>(RestrictedToThisTimeLineElements[eachTimeSpan].Item1, RestrictedToThisTimeLineElements[eachTimeSpan].Item2));
                    }
                }
            }



            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in CantBeUsedInCurrentTimeLine)//restores restricted for other timeLines into AvailableSubCalendarEvents
            {
                AvailableSubCalendarEvents[eachKeyValuePair.Key].Item1 += eachKeyValuePair.Value.Item1;
            }


            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> SerialIzedListOfSubCalendarEvents = AllSnugPossibilities;
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> AvailableSubCalendarEventsAfterRemovingAssingedSnugElements = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();






            if (SerialIzedListOfSubCalendarEvents.Count > 0)
            {

                foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> AlreadyAssignedSubCalendarEvent in AllSnugPossibilities)
                {
                    AvailableSubCalendarEventsAfterRemovingAssingedSnugElements = Utility.NotInList_NoEffect(AvailableSubCalendarEvents, AlreadyAssignedSubCalendarEvent);



                    SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar[TimeLineIndex] = AlreadyAssignedSubCalendarEvent;
                    /*if (checkSumOfTimeEvent(SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar) >= MaxNumberOfInterferringSubcalEvents)
                    {
                        ListOfAllSnugPossibilitiesInRespectiveTImeLines_hack.Add(SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar);
                        //if (ListOfAllSnugPossibilitiesInRespectiveTImeLines_hack.Count >= maxHackConstant)
                        {
                            //return ListOfAllSnugPossibilitiesInRespectiveTImeLines_hack;
                        }
                    }*/

                    ListOfAllSnugPossibilitiesInRespectiveTImeLines.AddRange(generateTreeCallsToSnugArray(AvailableSubCalendarEventsAfterRemovingAssingedSnugElements, AllTimeLines, TimeLineIndex + 1, SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar.ToList(), DictionaryOfTimelineAndSubcalendarEvents, DictionaryOfTimelineAndConstrainedElements));
                    //ListOfAllSnugPossibilitiesInRespectiveTImeLines = MyHolder.ToList();
                }
            }
            else
            {
                AvailableSubCalendarEventsAfterRemovingAssingedSnugElements = Utility.NotInList_NoEffect(AvailableSubCalendarEvents, new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>());//Hack this can be optimized... the whole "notinlist" call can be optimized as a call to AvailableSubCalendarEvents. Review to see if references to func are affected.
                ListOfAllSnugPossibilitiesInRespectiveTImeLines.AddRange(generateTreeCallsToSnugArray(AvailableSubCalendarEventsAfterRemovingAssingedSnugElements, AllTimeLines, TimeLineIndex + 1, SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar.ToList(), DictionaryOfTimelineAndSubcalendarEvents, DictionaryOfTimelineAndConstrainedElements));
            }




            return ListOfAllSnugPossibilitiesInRespectiveTImeLines;
        }



        List<List<List<SubCalendarEvent>>> SerializeDictionary(Dictionary<int, List<List<SubCalendarEvent>>> DictOfSubCalEvent)
        {
            List<List<List<SubCalendarEvent>>> retValue = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();
            //            List<List<List<SubCalendarEvent>>> WorkWihList  = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();

            for (int i = 0; i < DictOfSubCalEvent.Keys.Count; i++)//Goes through each dictionary index
            {
                List<List<List<SubCalendarEvent>>> NonTaintedListToWorkWith = retValue.ToList();
                List<List<List<SubCalendarEvent>>> MyTempList = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();
                foreach (List<SubCalendarEvent> ListOfClumpPermutation in DictOfSubCalEvent[i])
                {
                    List<List<List<SubCalendarEvent>>> TaintedListToWorkWith = NonTaintedListToWorkWith.ToList();
                    foreach (List<List<SubCalendarEvent>> ListSoFar in TaintedListToWorkWith)
                    {
                        List<List<SubCalendarEvent>> ListSoFar_cpy = ListSoFar.ToList();
                        ListSoFar_cpy.Add(ListOfClumpPermutation);
                        MyTempList.Add(ListSoFar_cpy);
                    }
                    if (TaintedListToWorkWith.Count < 1)
                    {
                        List<List<SubCalendarEvent>> ListSoFar = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
                        ListSoFar.Add(ListOfClumpPermutation);
                        MyTempList.Add(ListSoFar);

                    }
                }
                if (DictOfSubCalEvent[i].Count > 0)
                {
                    retValue = MyTempList;
                }
                else
                {
                    foreach (List<List<SubCalendarEvent>> AvailList in retValue)
                    {
                        AvailList.Add(new List<SubCalendarEvent>());
                    }
                }

            }

            return retValue;
        }

        int MaxNumberOfInterferringSubcalEvents = 0;

        int LargestTimeIndex = -1;

        List<IDefinedRange>[] getEdgeElements(TimeLine MyRangeOfTimeLine, IEnumerable<IDefinedRange> ArrayOfTInterferringime)
        {
            /*
             * Function looks through interferring elements collection, it checks for the elements that crossover the beginning datetime and end datetime
             
             */

            List<IDefinedRange> ListOfEdgeElements = new List<IDefinedRange>();
            List<IDefinedRange> StartEdge = new List<IDefinedRange>();
            List<IDefinedRange> EndEdge = new List<IDefinedRange>();
            int i = 0;

            ListOfEdgeElements = ArrayOfTInterferringime.Where(obj => (obj.IsDateTimeWithin(MyRangeOfTimeLine.Start) || obj.IsDateTimeWithin(MyRangeOfTimeLine.End))).ToList();


            /*for (; i < ArrayOfTInterferringime.Length; i++)
            {
                if (!MyRangeOfTimeLine.IsTimeLineWithin(ArrayOfTInterferringime[i]))
                {
                    ListOfEdgeElements.Add(ArrayOfTInterferringime[i]);
                }
            }*/

            i = 0;

            for (; i < ListOfEdgeElements.Count; i++)
            {
                if (ListOfEdgeElements[i].Start < MyRangeOfTimeLine.Start)
                {
                    StartEdge.Add(ListOfEdgeElements[i]);
                }
                else
                {
                    EndEdge.Add(ListOfEdgeElements[i]);
                }
            }

            List<IDefinedRange>[] StartAndEndEdgeList = new List<IDefinedRange>[2];
            StartAndEndEdgeList[0] = StartEdge;
            StartAndEndEdgeList[1] = EndEdge;
            return StartAndEndEdgeList;
        }

        /// <summary>
        /// Gets all the interferring events within the RangeTImeLine of "MyCalendarEvent". It Uses the AllEventDictionary as the source for the interferring events. You can also include NonCOmmitedCalendarEvemts as an additional source if you know the calendar events are not in the AllEventDictionary
        /// </summary>
        /// <param name="MyCalendarEvent"></param>
        /// <param name="NonCOmmitedCalendarEvemts"></param>
        /// <returns></returns>
        private SubCalendarEvent[] getInterferringSubEvents(CalendarEvent MyCalendarEvent, List<CalendarEvent> NonCOmmitedCalendarEvemts)
        {
            if (MyCalendarEvent.IsFromRecurringAndNotChildRepeatCalEvent)
            {
                throw new Exception("Weird error, found repeating event repeaing evemt");
            }
            return getInterferringSubEvents(MyCalendarEvent.StartToEnd, NonCOmmitedCalendarEvemts);
        }


        /// <summary>
        /// Gets all the interferring events within the timeline of EventRange. It uses PossibleSubCalEVents as the source of subevents.
        /// </summary>
        /// <param name="EventRange"></param>
        /// <param name="PossibleSubCalEVents"></param>
        /// <returns></returns>
        public IEnumerable<SubCalendarEvent> getInterferringSubEvents(TimeLine EventRange, IEnumerable<SubCalendarEvent> PossibleSubCalEVents)//gets list of subcalendar event in which the busytimeline interfer with Event range
        {
            return PossibleSubCalEVents.Where(obj => (EventRange.InterferringTimeLine(obj.ActiveSlot) != null));
        }

        /// <summary>
        /// Gets all the interferring events within the RangeTImeLine. It Uses the AllEventDictionary as the source for the interferring events. You can also include NonCOmmitedCalendarEvemts as an additional source if you know the calendar events are not in the AllEventDictionary
        /// </summary>
        /// <param name="EventRange"></param>
        /// <param name="NonCommitedCalendarEvemts"></param>
        /// <returns></returns>
        private SubCalendarEvent[] getInterferringSubEvents(TimeLine EventRange, List<CalendarEvent> NonCommitedCalendarEvemts = null)
        {

            List<SubCalendarEvent> MyArrayOfInterferringSubCalendarEvents = new List<SubCalendarEvent>(0);//List that stores the InterFerring List
            int lengthOfCalendarSubEvent = 0;

            foreach (CalendarEvent calEvent in getAllCalendarEvents().Where(obj => obj.isActive))
            {
                if (calEvent.IsFromRecurringAndNotChildRepeatCalEvent)
                {
                    SubCalendarEvent[] ArrayOfSubcalendarEventsFromRepeatingEvents = calEvent.ActiveRepeatSubCalendarEvents.Where(obj => obj != null).ToArray();//hack alert you should be able to remove the LINQ test for null
                    lengthOfCalendarSubEvent = ArrayOfSubcalendarEventsFromRepeatingEvents.Length;
                    MyArrayOfInterferringSubCalendarEvents.AddRange(calEvent.ActiveRepeatSubCalendarEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.StartToEnd) != null).ToList());
                }
                else
                {
                    SubCalendarEvent[] ArrayOfSubcalendarEventsFromNonRepeatingEvents = calEvent.ActiveSubEvents.Where(obj => obj != null).ToArray();//hack alert you should be able to remove the LINQ test for null
                    lengthOfCalendarSubEvent = ArrayOfSubcalendarEventsFromNonRepeatingEvents.Length;
                    MyArrayOfInterferringSubCalendarEvents.AddRange(calEvent.ActiveSubEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.StartToEnd) != null).ToList());
                }
            }

            if (NonCommitedCalendarEvemts != null)
            {
                foreach (CalendarEvent eachCalendarEvent in NonCommitedCalendarEvemts)
                {
                    int i = 0;
                    if (eachCalendarEvent.IsFromRecurringAndNotChildRepeatCalEvent)
                    {
                        lengthOfCalendarSubEvent = eachCalendarEvent.ActiveRepeatSubCalendarEvents.Length;
                        SubCalendarEvent[] ArrayOfSubcalendarEventsFromRepeatingEvents = eachCalendarEvent.ActiveRepeatSubCalendarEvents;

                        MyArrayOfInterferringSubCalendarEvents.AddRange(eachCalendarEvent.ActiveRepeatSubCalendarEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.StartToEnd) != null).ToList());
                    }
                    else
                    {
                        SubCalendarEvent[] ArrayOfSubcalendarEventsFromNonRepeatingEvents = eachCalendarEvent.ActiveSubEvents.Where(obj => obj != null).ToArray();//hack alert you should be able to remove the LINQ test for null
                        lengthOfCalendarSubEvent = ArrayOfSubcalendarEventsFromNonRepeatingEvents.Length;


                        MyArrayOfInterferringSubCalendarEvents.AddRange(eachCalendarEvent.ActiveSubEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.StartToEnd) != null).ToList());
                    }
                }
            }

            return MyArrayOfInterferringSubCalendarEvents.ToArray();

        }

        static public List<BusyTimeLine> SortBusyTimeline(List<BusyTimeLine> MyUnsortedEvents, bool StartOrEnd)//True is from start. False is from end
        {
            List<BusyTimeLine> retValue;
            if (StartOrEnd)
            {
                retValue = MyUnsortedEvents.OrderBy(obj => obj.Start).ToList();
            }
            else
            {
                retValue = MyUnsortedEvents.OrderBy(obj => obj.End).ToList();
            }

            return retValue;
        }

        static public TimeLine[] getAllFreeSpots_NoCompleteSchedule(TimeLine MyTimeLine)//Gets an array of all the freespots between busy spots in given time line, note attribute completeschedule is not used in finding freespots
        {
            BusyTimeLine[] AllBusySlots = MyTimeLine.OccupiedSlots;
            DateTimeOffset FinalCompleteScheduleDate;
            AllBusySlots = SortBusyTimeline(AllBusySlots.ToList(), true).ToArray();
            TimeLine[] AllFreeSlots = new TimeLine[AllBusySlots.Length];

            if (AllBusySlots.Length > 1)
            {
                AllFreeSlots = new TimeLine[(AllBusySlots.Length) + 1];
            }
            else
            {
                if (AllBusySlots.Length == 1)
                {
                    AllFreeSlots = new TimeLine[2];
                    AllFreeSlots[0] = new TimeLine(MyTimeLine.Start, AllBusySlots[0].Start.AddMilliseconds(0));
                    AllFreeSlots[1] = new TimeLine(AllBusySlots[0].End.AddMilliseconds(0), MyTimeLine.End);
                }
                else
                {
                    AllFreeSlots = new TimeLine[1];
                    AllFreeSlots[0] = new TimeLine(MyTimeLine.Start, MyTimeLine.End);
                }
                return AllFreeSlots.Where(timeLine => timeLine.TimelineSpan.TotalMinutes > 0).ToArray();
            }
            DateTimeOffset ReferenceTime = MyTimeLine.Start;

            for (int i = 0; i < (AllBusySlots.Length); i++)//Free Spots Are only between two busy Slots. So Index Counter starts from 1 get start of second busy
            {

                AllFreeSlots[i] = new TimeLine(ReferenceTime, AllBusySlots[i].Start);
                ReferenceTime = AllBusySlots[i].End;
                //AllFreeSlots[i] = new TimeLine(AllBusySlots[i].End, AllBusySlots[i + 1].Start.AddMilliseconds(-1));
            }
            FinalCompleteScheduleDate = MyTimeLine.End;
            if (FinalCompleteScheduleDate < MyTimeLine.End)
            {
                FinalCompleteScheduleDate = MyTimeLine.End;
            }

            AllFreeSlots[AllFreeSlots.Length - 1] = new TimeLine(AllBusySlots[AllBusySlots.Length - 1].End, FinalCompleteScheduleDate);
            List<TimeLine> SpecificFreeSpots = new List<TimeLine>(0);
            for (int i = 0; i < AllFreeSlots.Length; i++)//Free Spots Are only between two busy Slots. So Index Counter starts from 1 get start of second busy
            {
                //                AllFreeSlots[i] = new TimeLine(AllBusySlots[i].End.AddMilliseconds(1), AllBusySlots[i + 1].Start.AddMilliseconds(-1));
                if ((MyTimeLine.Start < AllFreeSlots[i].End) && (AllFreeSlots[i].Start < MyTimeLine.End))
                {
                    if ((AllFreeSlots[i].TimelineSpan.TotalSeconds > 1))
                    { SpecificFreeSpots.Add(AllFreeSlots[i]); }
                }
            }

            return SpecificFreeSpots.Where(timeLine => timeLine.TimelineSpan.TotalMinutes > 0).ToArray();
        }

        public TimeLine[] getAllFreeSpots(TimeLine MyTimeLine)//Gets an array of all the freespots between busy spots in given time line. Checks CompleteSchedule for the limits of free spots
        {
            BusyTimeLine[] AllBusySlots = CompleteSchedule.OccupiedSlots;
            DateTimeOffset FinalCompleteScheduleDate;
            AllBusySlots = SortBusyTimeline(AllBusySlots.ToList(), true).ToArray();
            TimeLine[] AllFreeSlots = new TimeLine[AllBusySlots.Length];

            if (AllBusySlots.Length > 1)
            {
                AllFreeSlots = new TimeLine[(AllBusySlots.Length) + 1];
            }
            else
            {
                if (AllBusySlots.Length == 1)
                {
                    AllFreeSlots = new TimeLine[2];
                    AllFreeSlots[0] = new TimeLine(MyTimeLine.Start, AllBusySlots[0].Start.AddMilliseconds(0));
                    AllFreeSlots[1] = new TimeLine(AllBusySlots[0].End.AddMilliseconds(0), MyTimeLine.End);
                }
                else
                {
                    AllFreeSlots = new TimeLine[1];
                    AllFreeSlots[0] = new TimeLine(MyTimeLine.Start, MyTimeLine.End);
                }
                return AllFreeSlots;
            }
            DateTimeOffset ReferenceTime = CompleteSchedule.Start;

            for (int i = 0; i < (AllBusySlots.Length); i++)//Free Spots Are only between two busy Slots. So Index Counter starts from 1 get start of second busy
            {

                AllFreeSlots[i] = new TimeLine(ReferenceTime, AllBusySlots[i].Start);
                ReferenceTime = AllBusySlots[i].End;
                //AllFreeSlots[i] = new TimeLine(AllBusySlots[i].End, AllBusySlots[i + 1].Start.AddMilliseconds(-1));
            }
            FinalCompleteScheduleDate = CompleteSchedule.End;
            if (FinalCompleteScheduleDate < MyTimeLine.End)
            {
                FinalCompleteScheduleDate = MyTimeLine.End;
            }
            //AllFreeSlots[AllBusySlots.Length-1] = new TimeLine(DateTimeOffset.UtcNow, AllBusySlots[0].Start);
            AllFreeSlots[AllFreeSlots.Length - 1] = new TimeLine(AllBusySlots[AllBusySlots.Length - 1].End, FinalCompleteScheduleDate);
            List<TimeLine> SpecificFreeSpots = new List<TimeLine>(0);
            for (int i = 0; i < AllFreeSlots.Length; i++)//Free Spots Are only between two busy Slots. So Index Counter starts from 1 get start of second busy
            {
                //                AllFreeSlots[i] = new TimeLine(AllBusySlots[i].End.AddMilliseconds(1), AllBusySlots[i + 1].Start.AddMilliseconds(-1));
                if ((MyTimeLine.Start < AllFreeSlots[i].End) && (AllFreeSlots[i].Start < MyTimeLine.End))
                {
                    if ((AllFreeSlots[i].TimelineSpan.TotalSeconds > 1))
                    { SpecificFreeSpots.Add(AllFreeSlots[i]); }
                }
            }

            return SpecificFreeSpots.ToArray();
        }

        void clearAllRepetitionLock()
        {
            IEnumerable<SubCalendarEvent> subEvents = getAllActiveSubEvents();
            foreach(SubCalendarEvent eachSubEvent in subEvents)
            {
                if (eachSubEvent.End > Now.constNow && eachSubEvent.isRepetitionLocked)
                {
                    eachSubEvent.disableRepetitionLock();
                }
            };
        }


        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ProcrastinateJustAnEvent(string EventID, TimeSpan RangeOfPush, bool force = false)
        {
            CalendarEvent ProcrastinateEvent = getCalendarEvent(EventID);
            SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(EventID);
            if (ReferenceSubEvent != null)
            {
                EventID SubEventID = new EventID(EventID);
                DateTimeOffset ReferenceStart = Now.calculationNow > ReferenceSubEvent.Start ? Now.calculationNow : ReferenceSubEvent.Start;
                Procrastination procrastinateData = new Procrastination(ReferenceStart, RangeOfPush);
                TimeLine timeLineAfterProcrastination = new TimeLine(procrastinateData.PreferredStartTime, ReferenceSubEvent.getCalendarEventRange.End);
                ReferenceSubEvent.disableRepetitionLock();
                if (ReferenceSubEvent.canExistWithinTimeLine(timeLineAfterProcrastination))
                {
                    ProcrastinateEvent.updateProcrastinate(procrastinateData);
                    DateTimeOffset StartTimeOfProcrastinate = ReferenceStart + RangeOfPush;
                    DateTimeOffset limitOfProcrastination = ReferenceSubEvent.getCalendarEventRange.End;
                    List<SubCalendarEvent> orderedByStartSubEvents = ProcrastinateEvent.ActiveSubEvents.OrderBy(o => o.Start).ToList();
                    orderedByStartSubEvents.ForEach(subEvent => subEvent.disableRepetitionLock());

                    ProcrastinateEvent.disableRepetitionLocks(timeLineAfterProcrastination.Start);
                    if (!Utility.tryPinSubEventsToEnd(orderedByStartSubEvents, timeLineAfterProcrastination) && !force)
                    {
                        return new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(new CustomErrors(CustomErrors.Errors.procrastinationAllSubeventsCannotFitDeadline), null);
                    }


                    if (ProcrastinateEvent.IsFromRecurringAndNotChildRepeatCalEvent)
                    {
                        ProcrastinateEvent = ProcrastinateEvent.getRepeatedCalendarEvent(SubEventID.getIDUpToRepeatCalendarEvent());
                    }
                    Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = getDeepCopyOfEventDictionary();

                    List<SubCalendarEvent> AllValidSubCalEvents = ProcrastinateEvent.ActiveSubEvents.ToList();
                    foreach(SubCalendarEvent subevent in AllValidSubCalEvents)
                    {
                        subevent.disablePreschedulingLock();
                    }

                    CalendarEvent ScheduleUpdated = CalendarEvent.getEmptyCalendarEvent(new EventID());
                    HashSet<SubCalendarEvent> NotDoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
                    ScheduleUpdated = EvaluateTotalTimeLineAndAssignValidTimeSpots(ScheduleUpdated, NotDoneYet, null);

                    foreach (SubCalendarEvent subevent in AllValidSubCalEvents)
                    {
                        subevent.enablePreschedulingLock();
                    }

                    if (ScheduleUpdated.Error != null)
                    {
                        LogStatus(ScheduleUpdated, "Procrastinate Single Event");
                    }

                    Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue = new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(ScheduleUpdated.Error, AllEventDictionary_Cpy);
                    return retValue;
                }
                else
                {
                    throw new CustomErrors((int)CustomErrors.Errors.procrastinationPastDeadline);
                }

            }
            else
            {
                throw new CustomErrors("Invalid subcalendarevent Id provided.");
            }
        }

        static TimeLine ScheduleTimeline = new TimeLine();

        public virtual void RepeatEvent(EventID eventId, Location location)
        {
            CalendarEvent calEvent = getCalendarEvent(eventId);
            if(!calEvent.isRigid && !calEvent.isProcrastinateEvent && !calEvent.isThirdParty)
            {
                SubCalendarEvent subEvent = getSubCalendarEvent(eventId);
                if(subEvent.IsDateTimeWithin(Now.constNow)) {
                    subEvent.ParentCalendarEvent.repeatLockSubEvent(subEvent.Id);
                    subEvent.ParentCalendarEvent.repeatEventAfterTime(subEvent.End);
                    HashSet<SubCalendarEvent> NotDoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
                    CalendarEvent ScheduleUpdated = CalendarEvent.getEmptyCalendarEvent(new EventID());
                    addCalendarEventToAllEventDictionary(ScheduleUpdated);
                    ScheduleUpdated = EvaluateTotalTimeLineAndAssignValidTimeSpots(ScheduleUpdated, NotDoneYet, location);
                } else
                {
                    throw new CustomErrors(CustomErrors.Errors.Repeated_Tile_Is_Not_Current_Tile);
                }
            }
            else
            {
                if (calEvent.isThirdParty)
                {
                    throw new CustomErrors(CustomErrors.Errors.TilerConfig_Repeat_Third_Party);
                }
                else if(calEvent.isProcrastinateEvent)
                {
                    throw new CustomErrors(CustomErrors.Errors.TilerConfig_Repeat_Procrastinate_All);
                }
                else
                {
                    throw new CustomErrors(CustomErrors.Errors.TilerConfig_Repeat_Rigid);
                }
                
            }
        }

        public virtual void RepeatEvent(string eventId, Location location)
        {
            EventID id = new EventID(eventId);
            RepeatEvent(id, location);
        }
        

        public void LogStatus(CalendarEvent triggerEvent, string Trigger)//writes to an XML Log file. Takes calendar event as an argument
        {
#if EnableClashLog
            
            XmlDocument xmldoc = new XmlDocument();
            string LogFile=Now.ToString();
            LogFile = "..\\..\\CustomErrorLogs\\" + LogFile.Replace('/', '_') + "_" + Trigger + ".xml";
            LogFile=LogFile.Replace(':','_');
            try
            {
                xmldoc.Load(LogFile);
            }
            catch
            {
                XmlDocument xmldoc1 = new XmlDocument();
                XmlElement MyEventScheduleNode = xmldoc1.CreateElement("LogReport");

                
                MyEventScheduleNode.PrependChild(xmldoc1.CreateElement("Error"));
                MyEventScheduleNode.ChildNodes[0].InnerText = triggerEvent.ErrorMessage;
                MyEventScheduleNode.PrependChild(xmldoc1.CreateElement("Action"));
                MyEventScheduleNode.ChildNodes[0].InnerText = Trigger;
                MyEventScheduleNode.PrependChild(xmldoc1.CreateElement("TriggerCalEvent"));
                XmlElement EventScheduleNode1 = CreateEventScheduleNode(triggerEvent);

                XmlNode MyImportedNode1 = xmldoc1.ImportNode(EventScheduleNode1 as XmlNode, true);
                MyEventScheduleNode.ChildNodes[0].PrependChild(MyImportedNode1);
                //xmldoc1.DocumentElement.SelectSingleNode("/LogReport/TriggerCalEvent").AppendChild(MyImportedNode1);
                MyEventScheduleNode.PrependChild(xmldoc1.CreateElement("CurrentCalEvents"));
                //xmldoc=xmldoc1;
                XmlNode initializedNode = xmldoc.ImportNode(MyEventScheduleNode as XmlNode, true);
                xmldoc.AppendChild(initializedNode);
                xmldoc.Save(LogFile);
            }


            XmlDocument doc = new XmlDocument();
            string NameOfFile = "MyEventLog.xml";
            
            doc.Load(NameOfFile);
            XmlNode RootNode = doc.DocumentElement.SelectSingleNode("/ScheduleLog");
            XmlNode MyImportedNode = xmldoc.ImportNode(RootNode as XmlNode, true);
            xmldoc.DocumentElement.SelectSingleNode("/LogReport/CurrentCalEvents").AppendChild(MyImportedNode);
            xmldoc.Save(LogFile);
#endif
        }
    }
}
