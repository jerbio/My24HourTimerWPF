#define StitcohRestrictedFromLeft
#define useLockedImplementation
#define useNonLockedImplementation
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
namespace TilerCore
{
    public class Schedule : IWhy
    {
        
        protected Dictionary<string, CalendarEvent> AllEventDictionary;
        protected DateTimeOffset ReferenceDayTIime;
        protected Dictionary<string, Location> Locations;
        protected TimeLine CompleteSchedule;
        public TimeSpan ZeroTimeSpan = new TimeSpan(0);
        public TimeSpan TwentyFourHourTimeSpan = new TimeSpan(1,0,0,0);
        public TimeSpan OnewWeekTimeSpan = new TimeSpan(7, 0, 0, 0);
        public TimeSpan HourTimeSpan = new TimeSpan(0, 1, 0, 0);
        protected Dictionary<ThirdPartyControl.CalendarTool, List<CalendarEvent>> ThirdPartyCalendars = new Dictionary<ThirdPartyControl.CalendarTool, List<CalendarEvent>>();
        protected DateTimeOffset StartofDay;
        protected bool retrievedThirdParty = false;

        protected bool UseTilerFront = false;
        Stopwatch myWatch = new Stopwatch();
        protected TilerUser TilerUser;
        protected int LatesMainID;
        string CurrentTimeZone = "UTC";
        protected Location CurrentLocation;

        protected double PercentageOccupancy = 0;
        //public static DateTimeOffset Now = new DateTimeOffset(2014,4,6,0,0,0);//DateTimeOffset.UtcNow;
        protected ReferenceNow _Now;// = new ReferenceNow( DateTimeOffset.UtcNow);

        public ReferenceNow Now
        {
            get
            {
                return _Now;
            }
        }

        public Location getHomeLocation {
            get
            {
                Location home = null;
                if (Locations.ContainsKey("home"))
                {
                    home = Locations["home"];
                    return home;
                } else
                {
                    return Location.getDefaultLocation();
                }
            }
        }
        static string stageOfProgram = "";
        int DebugCounter = 0;

        #region Constructor
        protected Schedule()
        {

        }

        public Schedule(Dictionary<string, CalendarEvent> allEventDictionary, DateTimeOffset starOfDay, Dictionary<string, Location> locations,  DateTimeOffset referenceNow, TilerUser user)
        {
            AllEventDictionary = allEventDictionary;
            TilerUser = user;
            _Now = new ReferenceNow(referenceNow, starOfDay);
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


        #region ToBeDeleted
        //public void RemoveAllCalendarEventFromLogAndCalendar()//MyTemp Function for deleting all calendar events
        //{
        //    throw new NotImplementedException();
        //}

        //async public Task WriteFullScheduleToLogAndOutlook()
        //{
        //    throw new NotImplementedException();
        //}

        //public void removeAllFromOutlook()
        //{
        //    throw new NotImplementedException();
        //}
        //public void WriteFullScheduleToOutlook()
        //{
        //    throw new NotImplementedException();
        //}

        //async virtual public Task UpdateWithDifferentSchedule(Dictionary<string, CalendarEvent> UpdatedSchedule)
        //{
        //    throw new NotImplementedException();
        //}
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

        virtual public async Task <Health> WhatIfDifferentDay(DateTimeOffset newDay, EventID eventId)
        {
            CalendarEvent calEvent = getCalendarEvent(eventId);
            DayTimeLine timeLine = Now.getDayTimeLineByTime(newDay);
            TempTilerEventChanges tilerChanges = calEvent.prepForWhatIfDifferentDay(timeLine, eventId);
            //subevent.updateCalculationEventRange(timeLine);
            if(CurrentLocation == null)
            {
                CurrentLocation = Location.getDefaultLocation();
            }
            if(string.IsNullOrEmpty(CurrentTimeZone))
            {
                CurrentTimeZone = "UTC";
            }
            await this.FindMeSomethingToDo(CurrentLocation, CurrentTimeZone).ConfigureAwait(false);
            Health scheduleHealth = new Health(getAllCalendarEvents(), Now.ComputationRange.Start, Now.ComputationRange.TimelineSpan, Now, this.getHomeLocation);
            calEvent.ReverseWhatIf(tilerChanges);
            return scheduleHealth;
        }
#endregion

        public void updateDataSetWithThirdPartyData(Tuple<ThirdPartyControl.CalendarTool,IEnumerable<CalendarEvent>> ThirdPartyData)
        {
            if (ThirdPartyData!=null)
            {
                ThirdPartyCalendars.Add(ThirdPartyData.Item1, ThirdPartyData.Item2.ToList());

                foreach(CalendarEvent ThirdPartyCalData in ThirdPartyData.Item2)
                {
                    AllEventDictionary.Add(ThirdPartyCalData.Calendar_EventID.getCalendarEventComponent(), ThirdPartyCalData);
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

        public CalendarEvent getCalendarEvent(EventID myEventID)
        {
            /*
             * function retrieves a calendarevent. 
             * If the ID is repeating event ID it'll get the repeating calendar event
             */
            CalendarEvent calEvent = AllEventDictionary[myEventID.getCalendarEventComponent()];
            CalendarEvent repeatEvent = calEvent.getRepeatedCalendarEvent(myEventID.getIDUpToRepeatCalendarEvent());


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
            
            ProcrastinateCalendarEvent retValue = null;
            if (AllEventDictionary.ContainsKey(myEventID.getCalendarEventComponent()))
            {
                retValue = getCalendarEvent(myEventID) as ProcrastinateCalendarEvent;
            }

            return retValue;
        }

        public SubCalendarEvent getSubCalendarEvent(string EventID)
        {
            CalendarEvent myCalendarEvent = getCalendarEvent(EventID);
            return myCalendarEvent.getSubEvent(new EventID(EventID));
        }


        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> UpdateCalEventTimeLine(CalendarEvent myCalendarEvent, TimeLine NewTimeLine)
        {
            myCalendarEvent.updateTimeLine(NewTimeLine);
            HashSet<SubCalendarEvent> NoDoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            
            Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
            AllEventDictionary_Cpy = AllEventDictionary.ToDictionary(obj => obj.Key, obj => obj.Value.createCopy());


            myCalendarEvent = EvaluateTotalTimeLineAndAssignValidTimeSpots(myCalendarEvent, NoDoneYet);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue = new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(myCalendarEvent.Error, AllEventDictionary);
            AllEventDictionary=AllEventDictionary_Cpy;
            return retValue ;
        }


        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> UpdateDeadLine(string EventId, DateTimeOffset NewDeadline)
        {
            CalendarEvent myCalendarEvent = getCalendarEvent(EventId);
            return UpdateCalEventTimeLine(myCalendarEvent, new TimeLine(myCalendarEvent.Start, NewDeadline));
        }


        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> updateSplitCount(string EventId, int SplitCount)
        {
            CalendarEvent myCalendarEvent = getCalendarEvent(EventId);
            myCalendarEvent.updateNumberOfSplits(SplitCount);
            return UpdateCalEventTimeLine(myCalendarEvent, new TimeLine(myCalendarEvent.Start, myCalendarEvent.End));
        }


        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> BundleChangeUpdate(string SubEventID, EventName NewName, DateTimeOffset SubeventStart, DateTimeOffset SubeventEnd, DateTimeOffset TimeLineStart, DateTimeOffset TimeLineEnd, int SplitCount)
        {
            EventID myEventID = new EventID(SubEventID);
            SubCalendarEvent mySubCalEvent = getSubCalendarEvent(SubEventID);
            CalendarEvent myCalendarEvent = getCalendarEvent(SubEventID);
            DateTimeOffset calEventStart = new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan());
            TimeLine calendarEventRange = null;
            bool isFromRigidEvent = false;
            if (mySubCalEvent.getRigid && myCalendarEvent.getRigid)
            {
                calendarEventRange = new TimeLine(SubeventStart, SubeventEnd);
                calEventStart = SubeventStart;
                TimeLineEnd = SubeventEnd;
                isFromRigidEvent = true;
            }
            else
            {
                calendarEventRange = mySubCalEvent.getCalculationRange;
            }

            SubCalendarEvent ChangedSubCal = new SubCalendarEvent(mySubCalEvent.getCreator, mySubCalEvent.getAllUsers(), mySubCalEvent.getTimeZone, mySubCalEvent.getId, mySubCalEvent.getName, SubeventStart, SubeventEnd, new BusyTimeLine(mySubCalEvent.getId, SubeventStart, SubeventEnd), mySubCalEvent.getRigid, mySubCalEvent.isEnabled, mySubCalEvent.getUIParam, mySubCalEvent.Notes, mySubCalEvent.getIsComplete, mySubCalEvent.Location, calendarEventRange, mySubCalEvent.Conflicts);

            //bool InitialRigidStatus = mySubCalEvent.Rigid;
            TimeSpan timeSpanStartDiff = TimeSpan.FromTicks( Math.Abs((mySubCalEvent.Start - SubeventStart).Ticks));
            TimeSpan timeSpanEndDiff = TimeSpan.FromTicks(Math.Abs((mySubCalEvent.End - SubeventEnd).Ticks));
            bool timeLineChange = (timeSpanStartDiff >= TimeSpan.FromMinutes(1)) || (timeSpanEndDiff >= TimeSpan.FromMinutes(1));

            if (timeLineChange)
            {
                if (!isFromRigidEvent)
                {
                    mySubCalEvent.UpdateThis(ChangedSubCal);
                    mySubCalEvent.SetAsRigid();
                }
                else
                {
                    mySubCalEvent.shiftEvent(ChangedSubCal.Start, true);
                    mySubCalEvent.UpdateThis(ChangedSubCal);
                }
                    
            }
            return BundleChangeUpdate(mySubCalEvent.SubEvent_ID.ToString(), NewName, calEventStart, TimeLineEnd, SplitCount, timeLineChange, mySubCalEvent);
        }
        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> BundleChangeUpdate(string EventId, EventName NewName, DateTimeOffset newStart, DateTimeOffset newEnd, int newSplitCount, bool forceRecalculation=false, SubCalendarEvent triggerSubEvent = null)
        {
            CalendarEvent myCalendarEvent = getCalendarEvent(EventId);
            bool isNameChange = NewName.NameValue != myCalendarEvent.getName.NameValue;
            bool isDeadlineChange = (newEnd) != myCalendarEvent.End;
            bool isStartChange = newStart != myCalendarEvent.Start;
            //bool isDurationDiff = myCalendarEvent.EachSplitTimeSpan != TimePerSplitCount;
            bool isSplitDiff = myCalendarEvent.NumberOfSplit != newSplitCount;

            Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
            AllEventDictionary_Cpy = AllEventDictionary.ToDictionary(obj => obj.Key, obj => obj.Value.createCopy());

            //if (isSplitDiff || isDurationDiff || isStartChange || isDeadlineChange||forceRecalculation)
            if (isSplitDiff || isStartChange || isDeadlineChange || forceRecalculation)
            {
                //myCalendarEvent.ChangeTimePerSplit(TimePerSplitCount);
                myCalendarEvent.updateNumberOfSplits(newSplitCount);
                //myCalendarEvent.ActiveSubEvents.AsParallel().ForAll(obj => obj.PinToEnd(myCalendarEvent.RangeTimeLine));
                if (triggerSubEvent != null)
                {
                    myCalendarEvent.updateTimeLine(triggerSubEvent, new TimeLine(newStart, newEnd)); 
                }
                else
                {
                    myCalendarEvent.updateTimeLine(new TimeLine(newStart, newEnd));
                }
                
                HashSet<SubCalendarEvent> NoDoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
                foreach(SubCalendarEvent subEvent in myCalendarEvent.ActiveSubEvents)
                {
                    NoDoneYet.Remove(subEvent);
                }
                myCalendarEvent = EvaluateTotalTimeLineAndAssignValidTimeSpots(myCalendarEvent, NoDoneYet,null,1);
            }

            if (isNameChange)
            {
                myCalendarEvent.updateEventName(NewName.NameValue);
            }

            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue = new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(myCalendarEvent.Error, AllEventDictionary);
            AllEventDictionary = AllEventDictionary_Cpy;
            return retValue;
        }

        public void FindNewSlotForSubEvent(EventID MyEventID)
        {
            return;
        }

        public BusyTimeLine NextActivity
        {
            get
            {
                //KeyValuePair<string, int> 
                List<BusyTimeLine> MyTotalSubEvents = new List<BusyTimeLine>(0);
                foreach ( CalendarEvent eachCalendarEvent in AllEventDictionary.Values.Where(obj=>obj.isActive))
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

        protected virtual TimeLine getTimeLine()
        {
            DateTimeOffset LastDeadline = Now.calculationNow.AddHours(1);
            List<BusyTimeLine> MyTotalBusySlots = new List<BusyTimeLine>(0);
            //var Holder=new List();
            foreach (KeyValuePair<string, CalendarEvent> MyCalendarEvent in AllEventDictionary)
            {
                var Holder = MyTotalBusySlots.Concat(GetBusySlotPerCalendarEvent(MyCalendarEvent.Value));
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
            List<SubCalendarEvent> retValue = daytimeLine.getSubEventsInDayTimeLine();
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

        public void EmptyMemory()
        {
            AllEventDictionary = new Dictionary<string, CalendarEvent>();
        }

        BusyTimeLine[] GetBusySlotPerCalendarEvent(CalendarEvent MyEvent)
        {
            int i = 0;
            List<BusyTimeLine> MyTotalSubEventBusySlots = new List<BusyTimeLine>(0);
            BusyTimeLine[] ArrayOfBusySlotsInRepeat = new BusyTimeLine[0];
            DateTimeOffset LastDeadline = Now.calculationNow.AddHours(1);

            if (MyEvent.RepetitionStatus)
            {
                ArrayOfBusySlotsInRepeat = GetBusySlotsPerRepeat(MyEvent.Repeat);
            }

            /*for (;i<MyEvent.AllEvents.Length;i++)
            {
                {*/
            foreach (SubCalendarEvent MySubCalendarEvent in MyEvent.ActiveSubEvents)//Active Fix
            {
                if (!MyEvent.RepetitionStatus)
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
                SubCalendarEvent currentPausedEvent = PreviousPausedCalEvent. getSubEvent(CurrentPausedEventId);
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
            if (CalEvent.RepetitionStatus)
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
                    foreach (SubCalendarEvent SubCalendarEvent in CalEventCopy.AllSubEvents.Except( new List<SubCalendarEvent>(){ unDisabled}))
                    {
                        SubCalendarEvent.disable(CalEventCopy);
                    }
                    bool isRigidInitial = false;
                    TimeSpan timeDiffBeforePause = (SubEvent.Start- unDisabled.Start);
                    unDisabled.shiftEvent(timeDiffBeforePause);
                    if (!SubEvent.getRigid)
                    {
                        unDisabled.SetAsRigid();
                    }
                    else
                    {
                        isRigidInitial = true;
                    }

                    HashSet<SubCalendarEvent> NotDoneYets = getNoneDoneYetBetweenNowAndReerenceStartTIme();
                    NotDoneYets.RemoveWhere(obj => obj.RangeTimeLine.doesTimeLineInterfere(unDisabled.RangeTimeLine));
                    CalEventCopy = EvaluateTotalTimeLineAndAssignValidTimeSpots(CalEventCopy, NotDoneYets,InterringWithNowEvent:2);
                    if (!isRigidInitial)
                    {
                        unDisabled.SetAsNonRigid();
                    }

                    SubEvent.Enable(CalEvent);
                    TimeSpan timeDiff = (unDisabled.Start - SubEvent.Start);
                    SubEvent.shiftEvent(timeDiff);
                    //await UpdateWithDifferentSchedule(AllEventDictionary);
                    RetValue = null;
                }

                else
                {
                    RetValue = new CustomErrors( "could not continue sub event because it is out of calendar event range error", 40000001);
                }
            }
            
            


            return RetValue;
        }



        public async Task<CustomErrors> deleteCalendarEventAndReadjust(string EventId)
        {
            CalendarEvent CalendarEventTOBeRemoved = getCalendarEvent(EventId);
            CalendarEventTOBeRemoved.Disable(false);
            //CalendarEventTOBeRemoved.DisableSubEvents(CalendarEventTOBeRemoved.ActiveSubEvents);
            if (CalendarEventTOBeRemoved.getRigid)
            {
                CalendarEventTOBeRemoved = new RigidCalendarEvent(
                    //EventID.GenerateCalendarEvent(), 
                    CalendarEventTOBeRemoved.getName, CalendarEventTOBeRemoved.Start, CalendarEventTOBeRemoved.End, CalendarEventTOBeRemoved.getActiveDuration, CalendarEventTOBeRemoved.getPreparation, CalendarEventTOBeRemoved.getPreDeadline, new Repetition(), CalendarEventTOBeRemoved.myLocation, new EventDisplay(), new MiscData(), false, false, CalendarEventTOBeRemoved.getCreator, CalendarEventTOBeRemoved.getAllUsers(), CalendarEventTOBeRemoved.getTimeZone, null);
            }
            else
            {
                CalendarEventTOBeRemoved = new CalendarEvent(
                    //EventID.GenerateCalendarEvent(), 
                    CalendarEventTOBeRemoved.getName, CalendarEventTOBeRemoved.Start, CalendarEventTOBeRemoved.End, CalendarEventTOBeRemoved.getActiveDuration, CalendarEventTOBeRemoved.getPreparation, CalendarEventTOBeRemoved.getPreDeadline, 1, new Repetition(), CalendarEventTOBeRemoved.myLocation, new EventDisplay(), new MiscData(), null, new NowProfile(), false, false, CalendarEventTOBeRemoved.getCreator, CalendarEventTOBeRemoved.getAllUsers(), CalendarEventTOBeRemoved.getTimeZone, null);
            }
            CalendarEventTOBeRemoved.DisableSubEvents(CalendarEventTOBeRemoved.ActiveSubEvents);

            HashSet<SubCalendarEvent> NotDOneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            CalendarEvent retValue= EvaluateTotalTimeLineAndAssignValidTimeSpots(CalendarEventTOBeRemoved, NotDOneYet);


            AllEventDictionary.Remove(CalendarEventTOBeRemoved.getId);//removes the false calendar event
//            await UpdateWithDifferentSchedule(AllEventDictionary).ConfigureAwait(false);
            return retValue.Error;
        }






        public async Task<CustomErrors >markAsCompleteCalendarEventAndReadjust(string EventId)
        {
            CalendarEvent CalendarEventTOBeRemoved = getCalendarEvent(EventId);
            CalendarEventTOBeRemoved.SetCompletion(true);

            if (CalendarEventTOBeRemoved.getRigid)
            {
                CalendarEventTOBeRemoved = new RigidCalendarEvent(
                    //EventID.GenerateCalendarEvent(), 
                    CalendarEventTOBeRemoved.getName, CalendarEventTOBeRemoved.Start, CalendarEventTOBeRemoved.End, CalendarEventTOBeRemoved.getActiveDuration, CalendarEventTOBeRemoved.getPreparation, CalendarEventTOBeRemoved.getPreDeadline, new Repetition(), CalendarEventTOBeRemoved.myLocation, new EventDisplay(), new MiscData(), true, true, CalendarEventTOBeRemoved.getCreator, CalendarEventTOBeRemoved.getAllUsers(), CalendarEventTOBeRemoved.getTimeZone, null);
            }
            else
            {
                CalendarEventTOBeRemoved = new CalendarEvent(
                    //EventID.GenerateCalendarEvent(), 
                    CalendarEventTOBeRemoved.getName, CalendarEventTOBeRemoved.Start, CalendarEventTOBeRemoved.End, CalendarEventTOBeRemoved.getActiveDuration, CalendarEventTOBeRemoved.getPreparation, CalendarEventTOBeRemoved.getPreDeadline, 1, new Repetition(), CalendarEventTOBeRemoved.myLocation, new EventDisplay(), new MiscData(), null, new NowProfile(), true, true, CalendarEventTOBeRemoved.getCreator, CalendarEventTOBeRemoved.getAllUsers(), CalendarEventTOBeRemoved.getTimeZone, null);
            }
            CalendarEventTOBeRemoved.DisableSubEvents(CalendarEventTOBeRemoved.ActiveSubEvents);


            HashSet<SubCalendarEvent> NotDOneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            CalendarEvent retValue= EvaluateTotalTimeLineAndAssignValidTimeSpots(CalendarEventTOBeRemoved, NotDOneYet);


            AllEventDictionary.Remove(CalendarEventTOBeRemoved.getId);//removes the false calendar event
            //await UpdateWithDifferentSchedule(AllEventDictionary).ConfigureAwait(false);
            return retValue.Error;
        }

        

        public void markSubEventAsCompleteCalendarEventAndReadjust(string EventId)
        {

            CalendarEvent referenceCalendarEventWithSubEvent = getCalendarEvent(EventId);
            SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(EventId);

            EventID SubEventID = new EventID(EventId);

            

            bool InitialRigid = ReferenceSubEvent.getRigid;


            if (referenceCalendarEventWithSubEvent.RepetitionStatus)
            {
                referenceCalendarEventWithSubEvent = referenceCalendarEventWithSubEvent.getRepeatedCalendarEvent(SubEventID.getIDUpToRepeatCalendarEvent() );
            }
            Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
            AllEventDictionary_Cpy = AllEventDictionary.ToDictionary(obj => obj.Key, obj => obj.Value.createCopy());
            List<SubCalendarEvent> AllValidSubCalEvents = new List<SubCalendarEvent>() { ReferenceSubEvent };// ProcrastinateEvent.AllActiveSubEvents.Where(obj => obj.End > ReferenceSubEvent.Start).ToList();
            DateTimeOffset StartTime = Now.calculationNow;
            DateTimeOffset EndTime = StartTime.Add(ReferenceSubEvent.getActiveDuration); ;
            ReferenceSubEvent.SetCompletionStatus(true, referenceCalendarEventWithSubEvent);


            TimeSpan TotalActiveDuration = Utility.SumOfActiveDuration(AllValidSubCalEvents);

            DateTimeOffset StartData = DateTimeOffset.Parse(referenceCalendarEventWithSubEvent.Start.ToString("hh:mm tt") + " " + referenceCalendarEventWithSubEvent.Start.Date.ToShortDateString());
            DateTimeOffset EndData = DateTimeOffset.Parse(referenceCalendarEventWithSubEvent.End.ToString("hh:mm tt") + " " + referenceCalendarEventWithSubEvent.End.Date.ToShortDateString());

            //CalendarEvent(string NameEntry, string StartTime, DateTimeOffset StartDateEntry, string EndTime, DateTimeOffset EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag,Location EventLocation)
            CalendarEvent ScheduleUpdated;
            if (ReferenceSubEvent.getRigid)
            {
                ScheduleUpdated = new RigidCalendarEvent(
                    //EventID.GenerateCalendarEvent(), 
                    referenceCalendarEventWithSubEvent.getName, StartData, EndData, ReferenceSubEvent.getActiveDuration, referenceCalendarEventWithSubEvent.getPreDeadline, referenceCalendarEventWithSubEvent.getPreparation,  new Repetition(), ReferenceSubEvent.Location, new EventDisplay(), new MiscData(), true, true, referenceCalendarEventWithSubEvent.getCreator, referenceCalendarEventWithSubEvent.getAllUsers(), ReferenceSubEvent.getTimeZone, null);
            }
            else
            {
                ScheduleUpdated = new CalendarEvent(
                    //EventID.GenerateCalendarEvent(), 
                    referenceCalendarEventWithSubEvent.getName, StartData, EndData, ReferenceSubEvent.getActiveDuration, referenceCalendarEventWithSubEvent.getPreparation, referenceCalendarEventWithSubEvent.getPreDeadline, 1, new Repetition(),  ReferenceSubEvent.Location, new EventDisplay(), new MiscData(), null, new NowProfile(), true, true, referenceCalendarEventWithSubEvent.getCreator, referenceCalendarEventWithSubEvent.getAllUsers(), ReferenceSubEvent.getTimeZone, null);
            }
            ScheduleUpdated.DisableSubEvents(ScheduleUpdated.AllSubEvents);//hackalert


            HashSet<SubCalendarEvent> NotDOneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            ScheduleUpdated = EvaluateTotalTimeLineAndAssignValidTimeSpots(ScheduleUpdated, NotDOneYet);

            AllEventDictionary.Remove(ScheduleUpdated.getId);//removes the false calendar event
        }

        /// <summary>
        /// function marks a Subevent as complete. This process forces the calendarevent to check if its complete.
        /// </summary>
        /// <param name="EventID"></param>
        public async Task markSubEventAsComplete(string EventID)
        {

            CalendarEvent referenceCalendarEventWithSubEvent = getCalendarEvent(EventID);
            SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(EventID);
            ReferenceSubEvent.SetCompletionStatus(true, referenceCalendarEventWithSubEvent);
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
                ReferenceSubEvent.SetCompletionStatus(true, referenceCalendarEventWithSubEvent);
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


            bool InitialRigid = ReferenceSubEvent.getRigid;

            
            if (referenceCalendarEventWithSubEvent.RepetitionStatus)
            {
                referenceCalendarEventWithSubEvent = referenceCalendarEventWithSubEvent.getRepeatedCalendarEvent(SubEventID.getIDUpToRepeatCalendarEvent());
            }

            Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
            AllEventDictionary_Cpy = AllEventDictionary.ToDictionary(obj => obj.Key, obj => obj.Value.createCopy());

            List<SubCalendarEvent> AllValidSubCalEvents = new List<SubCalendarEvent>() { ReferenceSubEvent };// ProcrastinateEvent.AllActiveSubEvents.Where(obj => obj.End > ReferenceSubEvent.Start).ToList();
            DateTimeOffset StartTime = Now.calculationNow;
            DateTimeOffset EndTime = StartTime.Add(ReferenceSubEvent.getActiveDuration); ;



            referenceCalendarEventWithSubEvent.DisableSubEvents(AllValidSubCalEvents);

            TimeSpan TotalActiveDuration = Utility.SumOfActiveDuration(AllValidSubCalEvents);
            //CalendarEvent(string NameEntry, string StartTime, DateTimeOffset StartDateEntry, string EndTime, DateTimeOffset EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag,Location EventLocation)

            DateTimeOffset StartData = DateTimeOffset.Parse(referenceCalendarEventWithSubEvent.Start.ToString("hh:mm tt")+" "+ referenceCalendarEventWithSubEvent.Start.Date.ToShortDateString());
            DateTimeOffset EndData = DateTimeOffset.Parse(referenceCalendarEventWithSubEvent.End.ToString("hh:mm tt") + " " + referenceCalendarEventWithSubEvent.End.Date.ToShortDateString());

            CalendarEvent ScheduleUpdated;
            if (ReferenceSubEvent.getRigid)
            {
                ScheduleUpdated = new RigidCalendarEvent(
                    //EventID.GenerateCalendarEvent(), 
                    referenceCalendarEventWithSubEvent.getName, StartData, EndData, ReferenceSubEvent.getActiveDuration, referenceCalendarEventWithSubEvent.getPreparation, referenceCalendarEventWithSubEvent.getPreDeadline, new Repetition(), ReferenceSubEvent.Location, new EventDisplay(), new MiscData(), false, false, ReferenceSubEvent.getCreator, ReferenceSubEvent.getAllUsers(), ReferenceSubEvent.getTimeZone, null);
            }
            else
            {
                ScheduleUpdated  = new CalendarEvent(
                    //EventID.GenerateCalendarEvent(), 
                    referenceCalendarEventWithSubEvent.getName, StartData, EndData, ReferenceSubEvent.getActiveDuration, referenceCalendarEventWithSubEvent.getPreparation, referenceCalendarEventWithSubEvent.getPreDeadline, 1, new Repetition(), ReferenceSubEvent.Location, new EventDisplay(), new MiscData(), null, new NowProfile(), false, false, ReferenceSubEvent.getCreator, ReferenceSubEvent.getAllUsers(), ReferenceSubEvent.getTimeZone, null);
            }
            
            
            ScheduleUpdated.DisableSubEvents(ScheduleUpdated.AllSubEvents);


            HashSet<SubCalendarEvent> NotDOneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            //ScheduleUpdated = EvaluateTotalTimeLineAndAssignValidTimeSpots(ScheduleUpdated, NotDOneYet);



            AllEventDictionary.Remove(ScheduleUpdated.getId);//removes the false calendar event
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
                SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(eachString);
                ReferenceSubEvent.disable(referenceCalendarEventWithSubEvent);
            }
        }

        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ProcrastinateAll(TimeSpan DelaySpan, string NameOfEvent = "BLOCKED OUT", string timeZone = "UTC")
        {
            EventName blockName = new EventName(NameOfEvent);
            TilerUser user = this.User;
            ProcrastinateCalendarEvent procratinateAll = getProcrastinateAllEvent();

            CalendarEvent procrastinateAll = ProcrastinateCalendarEvent.generateProcrastinateAll(Now.constNow, user, DelaySpan, timeZone, procratinateAll , NameOfEvent);
            return Procrastinate(procrastinateAll);
        }

        private Tuple<CustomErrors, Dictionary<string, CalendarEvent>> Procrastinate(CalendarEvent NewEvent)
        {
            HashSet<SubCalendarEvent> NotdoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            /*Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
            AllEventDictionary_Cpy = AllEventDictionary.ToDictionary(obj => obj.Key, obj => obj.Value.createCopy());*/
            NewEvent = EvaluateTotalTimeLineAndAssignValidTimeSpots(NewEvent, NotdoneYet,null,2);
            //AllEventDictionary.Remove(NewEvent.Id);

            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue = new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(NewEvent.Error, AllEventDictionary);
            if (!retValue.Item2.ContainsKey(NewEvent.Calendar_EventID.getCalendarEventComponent()))
            {
                retValue.Item2.Add(NewEvent.Calendar_EventID.getCalendarEventComponent(), NewEvent);
            }
            return retValue;
        }


        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> SetEventAsNow(string EventID,bool Force=false)
        {
            CalendarEvent ProcrastinateEvent = getCalendarEvent(EventID);
            SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(EventID);

            NowProfile myNow = new NowProfile(Now.constNow, true);
            
            EventID SubEventID = new EventID(EventID);


            bool InitialRigid = ReferenceSubEvent.getRigid;

            if (!ReferenceSubEvent.shiftEvent(Now.calculationNow - ReferenceSubEvent.Start) && !Force)
            {
                return new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(new CustomErrors("You will be going outside the limits of this event, Is that Ok?",5), AllEventDictionary);
            }


            if (ProcrastinateEvent.RepetitionStatus)
            {
                ProcrastinateEvent = ProcrastinateEvent.getRepeatedCalendarEvent(SubEventID.getIDUpToRepeatCalendarEvent());
            }

            Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
            AllEventDictionary_Cpy = AllEventDictionary.ToDictionary(obj => obj.Key, obj => obj.Value.createCopy());

            List<SubCalendarEvent> AllValidSubCalEvents = new List<SubCalendarEvent>() { ReferenceSubEvent };// ProcrastinateEvent.AllActiveSubEvents.Where(obj => obj.End > ReferenceSubEvent.Start).ToList();

            DateTimeOffset StartTime = Now.calculationNow;
            DateTimeOffset EndTime = StartTime.Add(ReferenceSubEvent.getActiveDuration); ;


            
            
            
            TimeSpan TotalActiveDuration = Utility.SumOfActiveDuration(AllValidSubCalEvents);
            //CalendarEvent(string NameEntry, string StartTime, DateTimeOffset StartDateEntry, string EndTime, DateTimeOffset EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag,Location EventLocation)
            //CalendarEvent ScheduleUpdated = new CalendarEvent(ProcrastinateEvent.Name, StartTime.ToString(), StartTime, EndTime.ToString(), EndTime, AllValidSubCalEvents.Count.ToString(), ProcrastinateEvent.PreDeadline.ToString(), TotalActiveDuration.ToString(), new Repetition(), true, true, ProcrastinateEvent.Preparation.ToString(), true, ReferenceSubEvent.myLocation,true, new EventDisplay(),new MiscData(),false);
            //SubCalendarEvent RigidizedEvent =ScheduleUpdated.ActiveSubEvents[0];

            CalendarEvent ScheduleUpdated = ProcrastinateEvent.getNowCalculationCopy(myNow);//.getNowCalculationCopy(myNow);
            ProcrastinateEvent.DisableSubEvents(AllValidSubCalEvents);
            SubCalendarEvent RigidizedEvent = ScheduleUpdated.ActiveSubEvents[0];

            //RigidizedEvent.shiftEvent(Now.calculationNow - RigidizedEvent.Start, Force);//remember to fix shift force option


            //SubCalendarEvent RigidSubCalendarEvent = new SubCalendarEvent(RigidizedEvent.ID, RigidizedEvent.Start, RigidizedEvent.End, RigidizedEvent.ActiveSlot, true, RigidizedEvent.isEnabled, ReferenceSubEvent.UIParam, ReferenceSubEvent.Notes, ReferenceSubEvent.isComplete, RigidizedEvent.myLocation, RigidizedEvent.getCalendarEventRange);
            //RigidizedEvent.UpdateThis(RigidSubCalendarEvent);
            string IDOfRigidized = RigidizedEvent.getId;
            HashSet<SubCalendarEvent> NotDOneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();


            ScheduleUpdated = EvaluateTotalTimeLineAndAssignValidTimeSpots(ScheduleUpdated,NotDOneYet ,null,1);

            SubCalendarEvent[] UpdatedSubCalevents = ScheduleUpdated.ActiveSubEvents;

            for (int i = 0; i < AllValidSubCalEvents.Count; i++)//updates the subcalevents
            {
                bool Rigid = AllValidSubCalEvents[i].getRigid;
                if (IDOfRigidized == AllValidSubCalEvents[i].getId)
                { 
                    Rigid=InitialRigid;
                }

                SubCalendarEvent updatedSubCal = new SubCalendarEvent(AllValidSubCalEvents[i].getCreator, AllValidSubCalEvents[i].getAllUsers(), AllValidSubCalEvents[i].getId, AllValidSubCalEvents[i].getTimeZone, UpdatedSubCalevents[i].getName, UpdatedSubCalevents[i].Start, UpdatedSubCalevents[i].End, UpdatedSubCalevents[i].ActiveSlot, Rigid, AllValidSubCalEvents[i].isEnabled, AllValidSubCalEvents[i].getUIParam, AllValidSubCalEvents[i].Notes, AllValidSubCalEvents[i].getIsComplete, AllValidSubCalEvents[i].Location, ProcrastinateEvent.RangeTimeLine);
                AllValidSubCalEvents[i].shiftEvent(updatedSubCal.Start - AllValidSubCalEvents[i].Start, true);///not using update this because of possible issues with subevent not being restricted
                //ProcrastinateEvent.updateSubEvent(updatedSubCal.SubEvent_ID, updatedSubCal);
            }

            ProcrastinateEvent.EnableSubEvents(AllValidSubCalEvents);
            ProcrastinateEvent.UpdateNowProfile(myNow);

            if (ScheduleUpdated.Error!=null)
            {
                LogStatus(ScheduleUpdated, "Set as now");
            }

            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue = new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(ScheduleUpdated.Error, AllEventDictionary);
            //AllEventDictionary = AllEventDictionary_Cpy;

            //UpdateWithProcrastinateSchedule(AllEventDictionary);
            return retValue;
        }


        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> SetCalendarEventAsNow(string CalendarID, bool Force = false)
        {
            CalendarEvent CalendarEvent = getCalendarEvent(CalendarID);
            IEnumerable<SubCalendarEvent> orderedSubEvents = CalendarEvent.ActiveSubEvents.OrderBy(obj => obj.Start);
            Dictionary<string,CalendarEvent> AllEventDictionary_Cpy = AllEventDictionary.ToDictionary(obj => obj.Key, obj => obj.Value.createCopy());
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue = new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(new CustomErrors("No Active Event Found", 100), AllEventDictionary_Cpy);
            if (orderedSubEvents.Count() > 0)
            {
                SubCalendarEvent mySubCalendarEvent = orderedSubEvents.First();
                retValue = SetEventAsNow(mySubCalendarEvent.getId, true);
            }
            return retValue;
        }


        public Tuple<List<SubCalendarEvent>[], DayTimeLine[], List<SubCalendarEvent>> peekIntoSchedule(CalendarEvent NewEvent)
        {
            HashSet<SubCalendarEvent> NotdoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            Tuple<List<SubCalendarEvent>[], DayTimeLine[], List<SubCalendarEvent>> retValue;
            if (!NewEvent.getRigid)
            {
                retValue = peekEvaluateTotalTimeLineAndAssignValidTimeSpots(NewEvent, NotdoneYet);
            }
            else
            {
                retValue = peekEvaluateTotalTimeLineAndAssignValidTimeSpots(NewEvent, NotdoneYet, null, 1);
            }


            return retValue;
        }

        public CustomErrors AddToSchedule(CalendarEvent NewEvent)
        {
#if enableTimer
            myWatch.Start();
#endif
            //HashSet<SubCalendarEvent> NotdoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            HashSet<SubCalendarEvent> NotdoneYet = new HashSet<SubCalendarEvent>();// getNoneDoneYetBetweenNowAndReerenceStartTIme();
            if (!NewEvent.getRigid)
            {
                NewEvent = EvaluateTotalTimeLineAndAssignValidTimeSpots(NewEvent, NotdoneYet);
            }
            else
            {
                NewEvent = EvaluateTotalTimeLineAndAssignValidTimeSpots(NewEvent, NotdoneYet, null, 1);
            }

            
            ///

            if (NewEvent == null)//checks if event was assigned and ID ehich means it was successfully able to find a spot
            {
                return NewEvent.Error;
            }

            if (NewEvent.getId == "" || NewEvent == null)//checks if event was assigned and ID ehich means it was successfully able to find a spot
            {
                return NewEvent.Error;
            }


            if (NewEvent.Error!=null)
            {
                LogStatus(NewEvent, "Adding New Event");
            }

            try
            {
                AllEventDictionary.Add(NewEvent.Calendar_EventID.getCalendarEventComponent(), NewEvent);
            }
            catch
            {
                AllEventDictionary[NewEvent.getId] = NewEvent;
            }

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
            EventName name = new EventName("NothingToDo");
            CalendarEvent NewEvent = new RigidCalendarEvent(
                //EventID.GenerateCalendarEvent(), 
                name, Now.constNow, Now.constNow.Add(duration), duration, new TimeSpan(), new TimeSpan(), new Repetition(), currentLocation, null, null, false, false, TilerUser, new TilerUserGroup(), timeZone, null);
            NewEvent = EvaluateTotalTimeLineAndAssignValidTimeSpots(NewEvent, new HashSet<SubCalendarEvent>(), null, 1,true, false);
            CustomErrors RetValue = NewEvent.Error;
            AllEventDictionary.Remove(NewEvent.Calendar_EventID.getCalendarEventComponent());
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
            int i = 0;
            List<SubCalendarEvent> MyArrayOfSubEvents = new List<SubCalendarEvent>();

            if (MyCalendarEvent.RepetitionStatus)
            {

                SubCalendarEvent MySubEvent = new SubCalendarEvent(MyCalendarEvent.getCreator, MyCalendarEvent.getAllUsers(), MyCalendarEvent.getTimeZone, MyCalendarEvent.getActiveDuration, MyCalendarEvent.getName, MyCalendarEvent.Repeat.Range.Start, MyCalendarEvent.Repeat.Range.End, MyCalendarEvent.getPreparation, MyCalendarEvent.getId, MyCalendarEvent.getRigid, MyCalendarEvent.isEnabled, MyCalendarEvent.getUIParam, MyCalendarEvent.Notes, MyCalendarEvent.getIsComplete, MyCalendarEvent.myLocation, MyCalendarEvent.RangeTimeLine);


                //new SubCalendarEvent(MyCalendarEvent.End, MyCalendarEvent.Repeat.Range.Start, MyCalendarEvent.Repeat.Range.End, MyCalendarEvent.Preparation, MyCalendarEvent.ID);

                for (; MySubEvent.Start < MyCalendarEvent.Repeat.Range.End; )
                {
                    MyArrayOfSubEvents.Add(MySubEvent);
                    switch (MyCalendarEvent.Repeat.getFrequency)
                    {
                        case Repetition.Frequency.DAILY:
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.getCreator, MyCalendarEvent.getAllUsers(), MyCalendarEvent.getTimeZone, MyCalendarEvent.getActiveDuration, MyCalendarEvent.getName,  MyCalendarEvent.Repeat.Range.Start.AddDays(1), MyCalendarEvent.Repeat.Range.End.AddDays(1), MyCalendarEvent.getPreparation, MyCalendarEvent.getId, MyCalendarEvent.getRigid, MyCalendarEvent.isEnabled, MyCalendarEvent.getUIParam, MyCalendarEvent.Notes, MyCalendarEvent.getIsComplete, MyCalendarEvent.myLocation, MyCalendarEvent.RangeTimeLine);
                                break;
                            }
                        case Repetition.Frequency.WEEKLY:
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.getCreator, MyCalendarEvent.getAllUsers(), MyCalendarEvent.getTimeZone, MyCalendarEvent.getActiveDuration, MyCalendarEvent.getName, MyCalendarEvent.Repeat.Range.Start.AddDays(7), MyCalendarEvent.Repeat.Range.End.AddDays(7), MyCalendarEvent.getPreparation, MyCalendarEvent.getId, MyCalendarEvent.getRigid, MyCalendarEvent.isEnabled, MyCalendarEvent.getUIParam, MyCalendarEvent.Notes, MyCalendarEvent.getIsComplete, MyCalendarEvent.myLocation, MyCalendarEvent.RangeTimeLine);
                                break;
                            }
                        case Repetition.Frequency.BIWEEKLY:
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.getCreator, MyCalendarEvent.getAllUsers(), MyCalendarEvent.getTimeZone, MyCalendarEvent.getActiveDuration, MyCalendarEvent.getName, MyCalendarEvent.Repeat.Range.Start.AddDays(14), MyCalendarEvent.Repeat.Range.End.AddDays(14), MyCalendarEvent.getPreparation, MyCalendarEvent.getId, MyCalendarEvent.getRigid, MyCalendarEvent.isEnabled, MyCalendarEvent.getUIParam, MyCalendarEvent.Notes, MyCalendarEvent.getIsComplete, MyCalendarEvent.myLocation, MyCalendarEvent.RangeTimeLine);
                                break;
                            }
                        case Repetition.Frequency.MONTHLY:
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.getCreator, MyCalendarEvent.getAllUsers(), MyCalendarEvent.getTimeZone, MyCalendarEvent.getActiveDuration, MyCalendarEvent.getName, MyCalendarEvent.Repeat.Range.Start.AddMonths(1), MyCalendarEvent.Repeat.Range.End.AddMonths(1), MyCalendarEvent.getPreparation, MyCalendarEvent.getId, MyCalendarEvent.getRigid, MyCalendarEvent.isEnabled, MyCalendarEvent.getUIParam, MyCalendarEvent.Notes, MyCalendarEvent.getIsComplete, MyCalendarEvent.myLocation, MyCalendarEvent.RangeTimeLine);
                                break;
                            }
                        case Repetition.Frequency.YEARLY:
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.getCreator, MyCalendarEvent.getAllUsers(), MyCalendarEvent.getTimeZone, MyCalendarEvent.getActiveDuration, MyCalendarEvent.getName, MyCalendarEvent.Repeat.Range.Start.AddYears(1), MyCalendarEvent.Repeat.Range.End.AddYears(1), MyCalendarEvent.getPreparation, MyCalendarEvent.getId, MyCalendarEvent.getRigid, MyCalendarEvent.isEnabled, MyCalendarEvent.getUIParam, MyCalendarEvent.Notes, MyCalendarEvent.getIsComplete, MyCalendarEvent.myLocation, MyCalendarEvent.RangeTimeLine);
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



        bool[] CheckIfPotentialSubEventClashesWithAnyOtherSubEvents(CalendarEvent MyPotentialCalendarEvent, TimeLine MyTimeLineOfEvent)
        {
            BusyTimeLine[] ArrayOfBusySlots = MyTimeLineOfEvent.OccupiedSlots;
            bool[] StatusOfCollision = new bool[] { false, false };
            foreach (BusyTimeLine MyBusySlot in ArrayOfBusySlots)
            {
                if (MyBusySlot.doesTimeLineInterfere(MyPotentialCalendarEvent.RangeTimeLine))
                {
                    StatusOfCollision[0] = true;
                    if (AllEventDictionary[(new EventID(MyBusySlot.TimeLineID)).getCalendarEventComponent()].getRigid)
                    {
                        StatusOfCollision[1] = true;
                    }

                }
            }

            return StatusOfCollision;
        }

        CalendarEvent ResolveWithDataOutsideCalendarEventSchedule(CalendarEvent MyEvent, List<CalendarEvent> ListOfOtherCalendarEvents)
        {

            return new CalendarEvent();
        }

        List<TimeLine> CheckTimeLineListForEncompassingTimeLine(List<TimeLine> ListOfTimeLine, TimeLine MyTimeLine)
        {
            /*
             * Function forces takes an array of timeLine that possibly Encompasses another timeline. If a timeline in the List the second arguement it is restricted to timeLimits of the TimeLine or else it is just added.
             */
            List<TimeLine> UpdatedTimeLine = new System.Collections.Generic.List<TimeLine>();
            foreach (TimeLine EncompassingTimeLine in ListOfTimeLine)
            {
                if (EncompassingTimeLine.IsTimeLineWithin(MyTimeLine))
                {
                    UpdatedTimeLine.Add(new TimeLine(MyTimeLine.Start, MyTimeLine.End));
                }
                else
                {
                    UpdatedTimeLine.Add(EncompassingTimeLine);
                }
            }
            return UpdatedTimeLine;
        }

        public Tuple<List<SubCalendarEvent>[], DayTimeLine[], List<SubCalendarEvent>> peekEvaluateTotalTimeLineAndAssignValidTimeSpots(CalendarEvent MyEvent, HashSet<SubCalendarEvent> UnDoneEvents, List<CalendarEvent> NoneCOmmitedCalendarEvent = null, int InterringWithNowEvent = 0)
        {
            Tuple<List<SubCalendarEvent>[], DayTimeLine[], List<SubCalendarEvent>> RetValue;
            int i = 0;
            if (NoneCOmmitedCalendarEvent == null)
            {
                NoneCOmmitedCalendarEvent = new List<CalendarEvent>();
            }
            if (MyEvent.RepetitionStatus)
            {
                CalendarEvent tempCalendarEvent = CalendarEvent.getEmptyCalendarEvent(MyEvent.Calendar_EventID, MyEvent.Start, MyEvent.getRigid ? MyEvent.Start : MyEvent.End);//creates an "empty" calendar event. If the calEvent is rigid it has time span of zero

                RetValue=peekEvaluateTotalTimeLineAndAssignValidTimeSpots(tempCalendarEvent, UnDoneEvents, MyEvent.Repeat.RecurringCalendarEvents().ToList());
                return RetValue;
            }

            RetValue = peekIntoDays(MyEvent, NoneCOmmitedCalendarEvent.ToList(), InterringWithNowEvent, UnDoneEvents);
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
        public CalendarEvent EvaluateTotalTimeLineAndAssignValidTimeSpots(CalendarEvent MyEvent,HashSet<SubCalendarEvent> UnDoneEvents, List<CalendarEvent> NoneCOmmitedCalendarEvent = null, int InterringWithNowEvent=0,bool optimizeFirstTwentyFourHours = true, bool preserveFirstTwentyFourHours = true)
        {

            int i = 0;
            if (NoneCOmmitedCalendarEvent == null)
            {
                NoneCOmmitedCalendarEvent = new List<CalendarEvent>();
            }
            if (MyEvent.RepetitionStatus)
            {
                CalendarEvent tempCalendarEvent = CalendarEvent.getEmptyCalendarEvent(MyEvent.Calendar_EventID, MyEvent.Start,MyEvent.getRigid?MyEvent.Start: MyEvent.End);//creates an "empty" calendar event. If the calEvent is rigid it has time span of zero

                EvaluateTotalTimeLineAndAssignValidTimeSpots(tempCalendarEvent, UnDoneEvents, MyEvent.Repeat.RecurringCalendarEvents().ToList(), InterringWithNowEvent, optimizeFirstTwentyFourHours, preserveFirstTwentyFourHours);
                return MyEvent;
            }
            CalendarEvent MyCalendarEventUpdated = ReArrangeTimeLineWithinWithinCalendaEventRangeUpdated(MyEvent, NoneCOmmitedCalendarEvent.ToList(), InterringWithNowEvent, UnDoneEvents, optimizeFirstTwentyFourHours, preserveFirstTwentyFourHours);
            return MyCalendarEventUpdated;
        }


        List<TimeLine> getOnlyCompatibleTimeLines(SubCalendarEvent SubEvent, List<TimeLine> TimeLines)
        {
            List<TimeLine> retValue = new System.Collections.Generic.List<TimeLine>();

            foreach (TimeLine eachTimeLine in TimeLines)
            {
                if (SubEvent.canExistWithinTimeLine(eachTimeLine))
                {
                    retValue.Add(eachTimeLine);
                }
            }


            return retValue;
        }

        Dictionary<TimeLine, List<SubCalendarEvent>> JustPickCollidingTimeline(List<TimeLine> AllTImelines, List<SubCalendarEvent> AllSubCalendarEvent)
        { 
            AllSubCalendarEvent = AllSubCalendarEvent.ToList();
            Dictionary<TimeLine,List<SubCalendarEvent>> retValue = AllTImelines.ToDictionary(obj=>obj,obj=>new List<SubCalendarEvent>());

            for(int i=0; i<AllSubCalendarEvent.Count;i++)
            {
                for (int j = 0; j < AllTImelines.Count; j++)
                {
                    if (AllSubCalendarEvent[i].RangeTimeLine.InterferringTimeLine(AllTImelines[j]) != null)
                    {
                        retValue[AllTImelines[j]].Add(AllSubCalendarEvent[i]);
                        break;
                    }
                }
            }
            

            retValue = retValue.ToDictionary(obj => obj.Key, obj => {
                                List<SubCalendarEvent> ListOfEvents = obj.Value.OrderBy(obj1 => obj1.Start).ToList();
                                Utility.PinSubEventsToStart(ListOfEvents, obj.Key);
                                return ListOfEvents;
            });
            return retValue;            
        }



        Dictionary<TimeLine, List< SubCalendarEvent>> generateConstrainedList(List<TimeLine> AvailableTImeLines, List<SubCalendarEvent> AllEvents)
        {
            //populates dictionary by verifying if timeLine is the only available timeLine plausible for an event to fit in
            Dictionary<TimeLine, List< SubCalendarEvent>> retValue = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List< SubCalendarEvent>>();

            foreach (TimeLine eachTimeLine in AvailableTImeLines)
            {
                retValue.Add(eachTimeLine, new List<SubCalendarEvent>());
            }

            foreach (SubCalendarEvent eachmTuple in AllEvents)
            {
                List<TimeLine> CompatibleTimeLines = getOnlyCompatibleTimeLines(eachmTuple, AvailableTImeLines);
                if (CompatibleTimeLines.Count == 1)
                {
                    retValue[CompatibleTimeLines[0]].Add(eachmTuple);
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
                    if (AllEventDictionary[ParentID].RepetitionStatus)
                    {
                        CalendarEvent repeatCalEvent = AllEventDictionary[ParentID].getRepeatedCalendarEvent(MyEventID.getIDUpToRepeatCalendarEvent());

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
                        CalendarEvent nonRepeatCalEvent = AllEventDictionary[ParentID];

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
            IEnumerable<SubCalendarEvent> tenPercentdateTimeLine;
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
                retrievedData = CollectionOfSubCalEvent.Where(obj => obj.RangeTimeLine.IsDateTimeWithin(ReferenceTime));//selects subcal event in which the Reference time intersect with the timeline. Just in case the reference timeline currently intersects it can reel in that new sub calendar event
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
        /// <returns></returns>
        Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors, IEnumerable<SubCalendarEvent>> getAllInterferringEventsAndTimeLineInCurrentEvaluation(CalendarEvent initializingCalendarEvent, List<CalendarEvent> NoneCommitedCalendarEventsEvents, int FlagType, HashSet<SubCalendarEvent> ExemptFromCalculation, TimeLine iniTimeLine)
        {
            TimeLine RangeOfCalculation = iniTimeLine != null ? iniTimeLine : initializingCalendarEvent.RangeTimeLine;
            HashSet<CalendarEvent> NonCommitedCalEvents= new HashSet<CalendarEvent>(NoneCommitedCalendarEventsEvents);
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
                            Now.UpdateNow(interFerringData.Item2);
                            SubEventsForCalculation = SubEventsForCalculation.Except(interferringWithNow).ToList();
                        }
                        CalculationTImeLine = new TimeLine(Now.calculationNow, CalculationTImeLine.End);

                    }
                    break;
                case 2:
                    {
                        DateTimeOffset currentNowTime = initializingCalendarEvent.ActiveSubEvents.Max(obj => obj.End);
                        Now.UpdateNow(currentNowTime);
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

            Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors, IEnumerable<SubCalendarEvent>> retValue = new Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors, IEnumerable<SubCalendarEvent>>(CalculationTImeLine, SubEventsForCalculation, retCustomErrors, interferringWithNow);
            return retValue;

        }

        /// <summary>
        /// Gets all events that "interferr" with the Now calculation.
        /// </summary>
        /// <param name="AllSubevents"></param>
        /// <param name="nowTime"></param>
        /// <returns></returns>
        mTuple< List<SubCalendarEvent>,DateTimeOffset> getElementsThatInterferWithNow(IEnumerable<SubCalendarEvent> AllSubevents,DateTimeOffset nowTime )
        {
            List<SubCalendarEvent>InterFerringEvents= AllSubevents.Where(obj => obj.Start < nowTime).Where(obj => obj.IsDateTimeWithin(nowTime)).ToList();
            mTuple<List<SubCalendarEvent>, DateTimeOffset> retValue = new mTuple<List<SubCalendarEvent>, DateTimeOffset>(InterFerringEvents, nowTime);
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

            HashSet<SubCalendarEvent> subEventsInSet = new HashSet<SubCalendarEvent>(getInterferringSubEvents(IniTImeLine, InitializingCalEvents).AsParallel().Where(obj => obj.getCalendarEventRange.End > NowTIme));
            ConcurrentBag<SubCalendarEvent> subEvents = new ConcurrentBag<SubCalendarEvent>();
            subEventsInSet.AsParallel().ForAll((subEvent) =>
            {
                if (subEvent.getRigid)
                {
                    if (subEvent.RangeTimeLine.doesTimeLineInterfere(CalculationTImeLine))
                    {
                        subEvents.Add(subEvent);
                    }
                }
                else
                {
                    if(!subEvent.getIsEventRestricted)
                    {
                        subEvents.Add(subEvent);
                    }
                    else
                    {
                        if (subEvent.getCalendarEventRange.doesTimeLineInterfere(CalculationTImeLine))
                        {
                            subEvents.Add(subEvent);
                        }
                    }
                }
            });

            List<SubCalendarEvent> retValue = subEvents.ToList();
            return retValue;
        }


        Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors, IEnumerable<SubCalendarEvent>> getAllInterferringEventsAndTimeLineInCurrentEvaluationOldDesign(CalendarEvent initializingCalendarEvent, List<CalendarEvent> NoneCommitedCalendarEventsEvents, int FlagType, HashSet<SubCalendarEvent> NotDoneYet, TimeLine iniTimeLine = null)
        {
            
            DateTimeOffset EarliestStartTime;
            DateTimeOffset LatestEndTime;
            List<SubCalendarEvent> collectionOfInterferringSubCalEvents;
            List<SubCalendarEvent> InterFerringWithNow = new List<SubCalendarEvent>();
            Tuple<IEnumerable<SubCalendarEvent>, DateTimeOffset, int, IEnumerable<SubCalendarEvent>> refinedStartTimeAndInterferringEvents;
            List<SubCalendarEvent> SubEventsholder;
            TimeLine RangeForScheduleUpdate = iniTimeLine!=null?iniTimeLine:initializingCalendarEvent.RangeTimeLine;
            IEnumerable<SubCalendarEvent> PertinentNotDoneYet = NotDoneYet.Where(obj => obj.getCalculationRange.InterferringTimeLine(RangeForScheduleUpdate) != null);
            DateTimeOffset LatestInNonComited = NoneCommitedCalendarEventsEvents.Max(obj => obj.End);

            LatestEndTime = PertinentNotDoneYet != null ? (PertinentNotDoneYet.Count() > 0 ? PertinentNotDoneYet.Select(obj => obj.getCalculationRange.End).Max() > RangeForScheduleUpdate.End ? PertinentNotDoneYet.Select(obj => obj.getCalculationRange.End).Max() : RangeForScheduleUpdate.End : RangeForScheduleUpdate.End) : RangeForScheduleUpdate.End;



            LatestEndTime = LatestEndTime >= LatestInNonComited ? LatestEndTime : LatestInNonComited;

            
            RangeForScheduleUpdate = new TimeLine(RangeForScheduleUpdate.Start, LatestEndTime);//updates the range for scheduling

            
            List<SubCalendarEvent> ArrayOfInterferringSubEvents = getInterferringSubEvents(RangeForScheduleUpdate, NoneCommitedCalendarEventsEvents).ToList();//It gets all the subevents within the time frame
            SubEventsholder = ArrayOfInterferringSubEvents.ToList();//holder List object for ArrayOfInterferringSubEvents
            SubEventsholder.AddRange(PertinentNotDoneYet.ToList());//Pins the Not done yet elements

            ArrayOfInterferringSubEvents=ArrayOfInterferringSubEvents.OrderBy(obj => obj.End).ToList();// sorts the elements by end date
            List<IDefinedRange>[] MyEdgeElements = getEdgeElements(RangeForScheduleUpdate, ArrayOfInterferringSubEvents);//gets the subevents crossing over the timeLine
            EarliestStartTime = MyEdgeElements[0].Count > 0 ? MyEdgeElements[0].OrderBy(obj => obj.Start).ToList()[0].Start : RangeForScheduleUpdate.Start;
            LatestEndTime = MyEdgeElements[1].Count > 0 ? MyEdgeElements[1].OrderBy(obj => obj.End).ToList()[MyEdgeElements[1].Count - 1].End : RangeForScheduleUpdate.End;
            EarliestStartTime = EarliestStartTime < Now.calculationNow ? Now.calculationNow : EarliestStartTime;
            RangeForScheduleUpdate = new TimeLine(EarliestStartTime, LatestEndTime);//updates the range of schedule

            


            refinedStartTimeAndInterferringEvents = getStartTimeWhenCurrentTimeClashesWithSubcalevent(ArrayOfInterferringSubEvents, EarliestStartTime, FlagType);//gets the start time relatve to the ArrayOfInterferringSubEvents and flag type
            InterFerringWithNow.AddRange(refinedStartTimeAndInterferringEvents.Item4);
            FlagType = refinedStartTimeAndInterferringEvents.Item3;//updates the flag type
            EarliestStartTime = refinedStartTimeAndInterferringEvents.Item2;//updates the earliest time from preceding function call
            ArrayOfInterferringSubEvents = refinedStartTimeAndInterferringEvents.Item1.ToList();//Updates the ArrayOfInterferringSubEvents just in case the Now element is dropped
            ArrayOfInterferringSubEvents.ToList();
            ArrayOfInterferringSubEvents.AddRange(PertinentNotDoneYet.ToList());//adds the PertinentNotDoneYet to the SubEventsholder list
            
            RangeForScheduleUpdate = new TimeLine(EarliestStartTime, LatestEndTime);//updates the RangeForScheduleUpdate timeline
            CustomErrors errorStatus = null;
            TimeSpan SumOfAllEventsTimeSpan = Utility.SumOfActiveDuration(ArrayOfInterferringSubEvents.ToList());//sum all events
            bool AddTill7days = false;

            do
            {
                if (AddTill7days)
                {
                    RangeForScheduleUpdate = new TimeLine(RangeForScheduleUpdate.Start, RangeForScheduleUpdate.Start.AddDays(7));
                    ArrayOfInterferringSubEvents = getInterferringSubEvents(RangeForScheduleUpdate, NoneCommitedCalendarEventsEvents).ToList();//It gets all the subevents within the time frame
                    ArrayOfInterferringSubEvents.AddRange(NotDoneYet);
                }
                while ((SumOfAllEventsTimeSpan > RangeForScheduleUpdate.TimelineSpan)||(AddTill7days))//loops untill the sum all the interferring events can possibly fit within the timeline. Essentially possibly fittable//hack alert to ensure usage of time space. THe extra addition has to be one pertaining to the occupancy
                {
                    PertinentNotDoneYet = NotDoneYet.Where(obj => obj.getCalculationRange.InterferringTimeLine(RangeForScheduleUpdate) != null);
                    ArrayOfInterferringSubEvents.AddRange(PertinentNotDoneYet.ToList());
                    if (ArrayOfInterferringSubEvents.Count < 1)
                    {
                        EarliestStartTime = RangeForScheduleUpdate.Start;
                        LatestEndTime = RangeForScheduleUpdate.End;
                        break;
                    }
                    EarliestStartTime = ArrayOfInterferringSubEvents.OrderBy(obj => obj.getCalculationRange.Start).ToList()[0].getCalculationRange.Start;//attempts to get subcalevent with a calendarevent with earliest start time
                    LatestEndTime = ArrayOfInterferringSubEvents.OrderBy(obj => obj.getCalculationRange.End).ToList()[ArrayOfInterferringSubEvents.Count() - 1].getCalculationRange.End;//attempts to get subcalevent with a calendarevent with latest Endtime
                    EarliestStartTime = EarliestStartTime < Now.calculationNow ? Now.calculationNow : EarliestStartTime;


                    refinedStartTimeAndInterferringEvents = getStartTimeWhenCurrentTimeClashesWithSubcalevent(ArrayOfInterferringSubEvents, EarliestStartTime, FlagType);
                    InterFerringWithNow.AddRange(refinedStartTimeAndInterferringEvents.Item4);
                    FlagType = refinedStartTimeAndInterferringEvents.Item3;
                    EarliestStartTime = refinedStartTimeAndInterferringEvents.Item2;
                    ArrayOfInterferringSubEvents = refinedStartTimeAndInterferringEvents.Item1.ToList();
                    ArrayOfInterferringSubEvents.AddRange(PertinentNotDoneYet.ToList());

                    RangeForScheduleUpdate = new TimeLine(EarliestStartTime, LatestEndTime);//updates range of scan
                    collectionOfInterferringSubCalEvents = getInterferringSubEvents(RangeForScheduleUpdate, NoneCommitedCalendarEventsEvents).ToList();//updates interferring events list


                    ArrayOfInterferringSubEvents = collectionOfInterferringSubCalEvents.ToList();
                    ArrayOfInterferringSubEvents = ArrayOfInterferringSubEvents.OrderBy(obj => obj.End).ToList();
                    MyEdgeElements = getEdgeElements(RangeForScheduleUpdate, ArrayOfInterferringSubEvents);
                    EarliestStartTime = MyEdgeElements[0].Count > 0 ? MyEdgeElements[0].OrderBy(obj => obj.Start).ToList()[0].Start : RangeForScheduleUpdate.Start;//if there is crossover with start time RangeForScheduleUpdate select the crossover subcalevent start time
                    LatestEndTime = MyEdgeElements[1].Count > 0 ? MyEdgeElements[1].OrderBy(obj => obj.End).ToList()[MyEdgeElements[1].Count - 1].End : RangeForScheduleUpdate.End;
                    EarliestStartTime = EarliestStartTime < Now.calculationNow ? Now.calculationNow : EarliestStartTime;


                    refinedStartTimeAndInterferringEvents = getStartTimeWhenCurrentTimeClashesWithSubcalevent(ArrayOfInterferringSubEvents, EarliestStartTime, FlagType);
                    InterFerringWithNow.AddRange(refinedStartTimeAndInterferringEvents.Item4);
                    FlagType = refinedStartTimeAndInterferringEvents.Item3;
                    EarliestStartTime = refinedStartTimeAndInterferringEvents.Item2;

                    ArrayOfInterferringSubEvents = refinedStartTimeAndInterferringEvents.Item1.ToList();
                    ArrayOfInterferringSubEvents.AddRange(PertinentNotDoneYet.ToList());
                    RangeForScheduleUpdate = new TimeLine(EarliestStartTime, LatestEndTime);
                    TimeSpan newSumOfAllTimeSpans = Utility.SumOfActiveDuration(ArrayOfInterferringSubEvents);
                    if (newSumOfAllTimeSpans == SumOfAllEventsTimeSpan)
                    {
                        errorStatus = new CustomErrors("Total sum of events exceeds available time span");
                        break;
                        //throw new Exception("You have events that cannot fit our time frame");
                    }
                    else
                    {
                        SumOfAllEventsTimeSpan = newSumOfAllTimeSpans;
                    }
                }
                AddTill7days = true;
            }
            while ((RangeForScheduleUpdate.TimelineSpan < new TimeSpan(7, 0, 0, 0) && errorStatus!=null));//checks if selected timespan is greater than a week and no errors flagged.

            return new Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors, IEnumerable<SubCalendarEvent>>(RangeForScheduleUpdate, ArrayOfInterferringSubEvents, errorStatus, InterFerringWithNow.Distinct());
        }


        Tuple<IEnumerable<SubCalendarEvent>,IEnumerable<SubCalendarEvent>> PintNotDoneYestSubEventToEndOfTimeLine(TimeLine encasingTimeLine, IEnumerable<SubCalendarEvent> NotDoneYetEvents)
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
            IEnumerable<SubCalendarEvent> retValue = NotDoneYet.Where(obj => (!obj.getRigid) && (!obj.RangeTimeLine.IsDateTimeWithin(Now.calculationNow)));
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
            IEnumerable<SubCalendarEvent> AllRigidEvents = AllEvents.Where(obj => obj.getRigid);
            List<SubCalendarEvent> CannotFitInAnyFreespot = AllEvents.Except(AllRigidEvents).ToList();

            TimeLine refTImeLine = ReferenceTimeline.CreateCopy();

            refTImeLine.AddBusySlots(AllRigidEvents.Select(obj => obj.ActiveSlot));
            IEnumerable<TimeLine> AllFreeSpots= refTImeLine.getAllFreeSlots();
            foreach (TimeLine eachTimeLine in AllFreeSpots)// gets all events that cannot fully exsit in any free spot
            {
                CannotFitInAnyFreespot=CannotFitInAnyFreespot.Except(CannotFitInAnyFreespot.Where(obj => obj.canExistWithinTimeLine(eachTimeLine))).ToList();
                //CannotFitInAnyFreespot = CannotFitInAnyFreespot.Except(CannotFitInAnyFreespot.Where(obj => obj.canExistWithinTimeLine(eachTimeLine))).ToList();
            }

            Dictionary<SubCalendarEvent, List<TimeLine>> NoFreeSpaceToConflictingSpaces= new Dictionary<SubCalendarEvent,List<TimeLine>>();

            foreach (SubCalendarEvent eachSubCalendarEvent in CannotFitInAnyFreespot)//builds dictionary NoFreeSpaceToConflictingSpaces by getting all the non free spots possible withing the attained freespots
            {
                List<TimeLine> PossibleSpaces = AllFreeSpots.Select(obj => obj.InterferringTimeLine(eachSubCalendarEvent.getCalculationRange)).Where(obj => obj != null).OrderByDescending(obj => obj.TimelineSpan).ToList();
                
                NoFreeSpaceToConflictingSpaces.Add(eachSubCalendarEvent, PossibleSpaces);
            }


            foreach (KeyValuePair<SubCalendarEvent, List<TimeLine>> AllAvailableTimeLines in NoFreeSpaceToConflictingSpaces.OrderBy(obj => obj.Value.Count))
            {
                if (AllAvailableTimeLines.Value.Count > 0)
                {
                    TimeLine MaxFreeSpotAvailable = AllAvailableTimeLines.Value.First();
                    if (!AllAvailableTimeLines.Key.PinToPossibleLimit(MaxFreeSpotAvailable))
                    {
                        throw new Exception("There is an error in PrepareElementsThatWillNotFit PinToPossibleLimit");
                    }
                }
            }


            List<BlobSubCalendarEvent> AllConflictingEvents = Utility.getConflictingEvents(AllRigidEvents.Concat(NoFreeSpaceToConflictingSpaces.Keys));

            List<CalendarEvent> allCalEvent = AllConflictingEvents.Select(obj => CalendarEvent.getEmptyCalendarEvent(new EventID(obj.SubEvent_ID.getCalendarEventID()), obj.Start, obj.End)).ToList();
            

            for(int i=0;i< allCalEvent.Count;i++)
            {
                CalendarEvent eachCalEvent = new CalendarEvent(allCalEvent[i], new SubCalendarEvent[] {AllConflictingEvents[i]});
                AllEventDictionary.Add(eachCalEvent.Calendar_EventID.getIDUpToCalendarEvent(), eachCalEvent);
            }
            
            
            retValue=retValue.Except(AllConflictingEvents.SelectMany(obj=>obj.getSubCalendarEventsInBlob())).ToList();
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
        CalendarEvent ReArrangeTimeLineWithinWithinCalendaEventRangeUpdated(CalendarEvent MyCalendarEvent, List<CalendarEvent> NoneCommitedCalendarEventsEvents, int InterferringWithNowFlag, HashSet<SubCalendarEvent> NotDoneYet, bool OptimizeFirstTwentyFour = true, bool preserveFirstTwentyFourHours = true)// this looks at the timeline of the calendar event and then tries to rearrange all subevents within the range to suit final output. Such that there will be sufficient time space for each subevent
        {
            /*
                Name{: Jerome Biotidara
             * this function is responsible for making sure there is some dynamic allotment of time to the subeevents. It takes a calendarevent, checks the alloted time frame and tries to move subevents within the time frame to satisfy the final goal.
             */
            int i = 0;

            if (MyCalendarEvent.RepetitionStatus != false)//Artificially generates random subevents for the calendar event
            {
                throw new Exception("invalid calendar event detected in ReArrangeTimeLineWithinWithinCalendaEventRange. Repeat not allowed");
            }

            NoneCommitedCalendarEventsEvents.Add(MyCalendarEvent);
            DateTimeOffset testDateTime = new DateTimeOffset(2014, 8, 11, 0, 0, 0, new TimeSpan());
            Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors, IEnumerable<SubCalendarEvent>> allInterferringSubCalEventsAndTimeLine = getAllInterferringEventsAndTimeLineInCurrentEvaluation(MyCalendarEvent, NoneCommitedCalendarEventsEvents, InterferringWithNowFlag, NotDoneYet, new TimeLine(Now.constNow.AddDays(-90), Now.ComputationRange.End));
            List<SubCalendarEvent> collectionOfInterferringSubCalEvents = allInterferringSubCalEventsAndTimeLine.Item2.ToList();
            List<SubCalendarEvent> ArrayOfInterferringSubEvents = allInterferringSubCalEventsAndTimeLine.Item2.ToList();
            TimeLine RangeForScheduleUpdate = allInterferringSubCalEventsAndTimeLine.Item1;
            IEnumerable<SubCalendarEvent> NowEvents = allInterferringSubCalEventsAndTimeLine.Item4.Where(obj => !obj.getRigid).ToList();
            if (InterferringWithNowFlag == 0)
            {
                foreach (SubCalendarEvent eachSubCalendarEvent in NowEvents)
                {
                    //if(!eachSubCalendarEvent.Rigid)
                    {
                        eachSubCalendarEvent.SetAsRigid();
                    }
                }

            }
            Now.UpdateNow(RangeForScheduleUpdate.Start);


            TimeSpan SumOfAllEventsTimeSpan = Utility.SumOfActiveDuration(ArrayOfInterferringSubEvents);
            Tuple<List<SubCalendarEvent>, List<BlobSubCalendarEvent>> preppedDataForNExtStage = PrepareElementsThatWillNotFit(ArrayOfInterferringSubEvents, RangeForScheduleUpdate);
            ArrayOfInterferringSubEvents = preppedDataForNExtStage.Item1;

            Dictionary<CalendarEvent, List<SubCalendarEvent>> DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents = new Dictionary<CalendarEvent, List<SubCalendarEvent>>();
            List<SubCalendarEvent> RigidSubCalendarEvents = new List<SubCalendarEvent>(0);
            List<BusyTimeLine> RigidSubCalendarEventsBusyTimeLine = new List<BusyTimeLine>(0);
            RigidSubCalendarEvents = ArrayOfInterferringSubEvents.Where(obj => obj.getRigid).ToList();
            RigidSubCalendarEventsBusyTimeLine = RigidSubCalendarEvents.Select(obj => obj.ActiveSlot).ToList();

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


            DayTimeLine[] AllDayTImeLine = Now.getAllDaysLookup().Select(obj => obj.Value).ToArray();
            ParallelizeCallsToDay(SortedInterFerringCalendarEvents_Deadline, ArrayOfInterferringSubEvents, AllDayTImeLine,OptimizeFirstTwentyFour, preserveFirstTwentyFourHours);

            foreach (BlobSubCalendarEvent eachSubCalEvent in preppedDataForNExtStage.Item2)
            {
                AllEventDictionary.Remove(eachSubCalEvent.SubEvent_ID.getIDUpToCalendarEvent());
            }



            if (InterferringWithNowFlag == 0)
            {
                foreach (SubCalendarEvent eachSubCalendarEvent in NowEvents)
                {
                    //if(!eachSubCalendarEvent.Rigid)
                    {
                        eachSubCalendarEvent.SetAsNonRigid();
                    }
                }
            }

            return MyCalendarEvent;

        }


        List<SubCalendarEvent> getWillNotFit(IEnumerable<TimeLine> AllFreeSpots, IEnumerable<SubCalendarEvent> AllSubCalendarEvents)
        {
            List<SubCalendarEvent> AllSubCalendarEvents_List = AllSubCalendarEvents.ToList();
            foreach (TimeLine eachTimeLine in AllFreeSpots)
            {
                AllSubCalendarEvents_List = AllSubCalendarEvents_List.Where(obj => !obj.canExistWithinTimeLine(eachTimeLine)).ToList();
            }
            return AllSubCalendarEvents_List;
        }

        List<mTuple<SubCalendarEvent, TimeLineWithEdgeElements>> DealWithWillNotFit(IEnumerable<SubCalendarEvent> ClashingSubEvents, IEnumerable<TimeLineWithEdgeElements> AllFreeSpot)
        {
            EventLearn Learning = new EventLearn();
            List<SubCalendarEvent> AllSubCalendarEvents = ClashingSubEvents.ToList();
            List<mTuple<SubCalendarEvent, TimeLineWithEdgeElements>> reassignedSubCalendarEvents = new List<mTuple<SubCalendarEvent, TimeLineWithEdgeElements>>();

            Dictionary<SubCalendarEvent, TimeLine> OptimizationPerSubCalendarEvent = new Dictionary<SubCalendarEvent, TimeLine>();
            foreach (TimeLineWithEdgeElements eachTimeline in AllFreeSpot)
            {
                if (AllSubCalendarEvents.Count < 1)
                {
                    break;
                }
                mTuple<SubCalendarEvent,double> selectedSubEvent= Learning.SelectBestSubCalendarEvent(eachTimeline, AllSubCalendarEvents);
                reassignedSubCalendarEvents.Add(new mTuple<SubCalendarEvent, TimeLineWithEdgeElements>(selectedSubEvent.Item1,eachTimeline));
                AllSubCalendarEvents.Remove(selectedSubEvent.Item1);
                selectedSubEvent.Item1.PinToPossibleLimit(eachTimeline);
            }

            
            return reassignedSubCalendarEvents;
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

        Dictionary<string, double> BuildDictionaryDistanceEdge(TimeLine ReferenceTimeline, CalendarEvent ReferenceCalendarEvent, Dictionary<string, double> CurrentDictionary)
        {
            BusyTimeLine[] ListOfBusySlots = ReferenceTimeline.OccupiedSlots;
            int i = 0;
            //Dictionary<string, double> CurrentDictionaryFrom
            int j = 0;
            for (i = 0; i < (ListOfBusySlots.Length - 1); i++)
            {
                j = i + 1;
                string generatedIndexMatch = BuildStringIndexForMatch(ListOfBusySlots[i], ListOfBusySlots[j]);
                CalendarEvent MyPrecedingCalendarEvent;
                CalendarEvent MyNextCalendarEvent;
                try
                {
                    MyPrecedingCalendarEvent = AllEventDictionary[generatedIndexMatch.Split('_')[0]];
                }
                catch
                {
                    MyPrecedingCalendarEvent = ReferenceCalendarEvent;
                }

                try
                {
                    MyNextCalendarEvent = AllEventDictionary[generatedIndexMatch.Split('_')[1]];
                }

                catch
                {
                    MyNextCalendarEvent = ReferenceCalendarEvent;
                }
                if (!(CurrentDictionary.ContainsKey(generatedIndexMatch)))
                {
                    double Distance = Location.calculateDistance(MyPrecedingCalendarEvent.myLocation, MyNextCalendarEvent.myLocation);
                    CurrentDictionary.Add(generatedIndexMatch, Distance);
                }
            }

            return CurrentDictionary;
        }

        Dictionary<TimeLine, List<SubCalendarEvent>> stitchRestrictedSubCalendarEvent(List<TimeLine> AllTimeLines, Dictionary<TimeLine, List<SubCalendarEvent>> arg1)
        {
            List<SubCalendarEvent> AllSubCall=arg1.SelectMany(obj=>obj.Value).ToList();
            SubCalendarEvent.updateMiscData(AllSubCall, 0);

            Dictionary<string, bool> var1 = new System.Collections.Generic.Dictionary<string, bool>();
            foreach (TimeLine eachTimeLine in AllTimeLines)//checks of subcal can exist within time frame increases the intData for every TimeLine it can fit in
            {
                SubCalendarEvent.incrementMiscdata(AllSubCall.Where(obj => obj.canExistWithinTimeLine(eachTimeLine)).ToList());
                List<mTuple<bool,SubCalendarEvent>> testSendData=Utility.SubCalEventsTomTuple( arg1[eachTimeLine],true);
                arg1[eachTimeLine] = stitchRestrictedSubCalendarEvent(testSendData, eachTimeLine).Select(OBJ=>OBJ.Item2).ToList();
            }


            int i = 0;
            foreach (TimeLine eachTimeLine in AllTimeLines)
            {
                
                
                List<mTuple<bool, SubCalendarEvent>> testSendData = Utility.SubCalEventsTomTuple(arg1[eachTimeLine], true);
                arg1[eachTimeLine] = stitchRestrictedSubCalendarEvent(testSendData, eachTimeLine).Select(obj=>obj.Item2).ToList();
                bool willpin = Utility.PinSubEventsToStart(arg1[eachTimeLine], eachTimeLine);
                i++;
            }
            SubCalendarEvent.updateMiscData(AllSubCall, 0);
            return arg1;
        }


        Dictionary<TimeLine, List<SubCalendarEvent>> stitchRestrictedSubCalendarEvent(List<TimeLine> AllTimeLines, List<SubCalendarEvent> AllSubCall)
        {
            Dictionary<TimeLine, List<SubCalendarEvent>> RetValue = AllTimeLines.ToDictionary(obj => obj, obj => new List<SubCalendarEvent>());
            AllSubCall = AllSubCall.ToList();
            SubCalendarEvent.updateMiscData(AllSubCall, 0);
            
            Dictionary<string, bool> var1 = new System.Collections.Generic.Dictionary<string, bool>();
            foreach (TimeLine eachTimeLine in AllTimeLines)//checks of subcal can exist within time frame increases the intData for every TimeLine it can fit in
            {
                if (AllSubCall.Count>0)
                {
                    SubCalendarEvent.incrementMiscdata(AllSubCall.Where(obj => obj.canExistWithinTimeLine(eachTimeLine)).ToList());
                    List<mTuple<bool, SubCalendarEvent>> testSendData = Utility.SubCalEventsTomTuple(AllSubCall, true);
                    List<SubCalendarEvent> reassigneElements = stitchRestrictedSubCalendarEvent(testSendData, eachTimeLine).Select(OBJ => OBJ.Item2).ToList();
                    AllSubCall = AllSubCall.Except(reassigneElements).ToList();
                    RetValue[eachTimeLine] = reassigneElements;
                }
                
            }
            
            return RetValue;
        }

        Tuple<List<SubCalendarEvent>[],DayTimeLine[],List<SubCalendarEvent>>peekIntoDays(CalendarEvent MyCalendarEvent, List<CalendarEvent> NoneCommitedCalendarEventsEvents, int InterferringWithNowFlag, HashSet<SubCalendarEvent> NotDoneYet,uint NumberOfDays=28)
        {
            int i = 0;
            List<SubCalendarEvent>[] AssignedEvents = new List<SubCalendarEvent>[NumberOfDays];
            List<DayTimeLine>[] TimeFrame = new List<DayTimeLine>[NumberOfDays];
            for (int j = 0; j < AssignedEvents.Length;j++ )
            {
                AssignedEvents[j] = new List<SubCalendarEvent>();
                TimeFrame[j] = new List<DayTimeLine>();
            }
                if (MyCalendarEvent.RepetitionStatus != false)//Artificially generates random subevents for the calendar event
                {
                    throw new Exception("invalid calendar event detected in peekIntoDays. Repeat not allowed");
                }

            NoneCommitedCalendarEventsEvents.Add(MyCalendarEvent);
            DateTimeOffset testDateTime = new DateTimeOffset(2014, 8, 11, 0, 0, 0, new TimeSpan());
            
            Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors, IEnumerable<SubCalendarEvent>> allInterferringSubCalEventsAndTimeLine = getAllInterferringEventsAndTimeLineInCurrentEvaluation(MyCalendarEvent, NoneCommitedCalendarEventsEvents, InterferringWithNowFlag, NotDoneYet, new TimeLine(Now.constNow.AddDays(-90), Now.getAllDaysCount((uint)NumberOfDays).Last().End));
            //Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors, IEnumerable<SubCalendarEvent>> allInterferringSubCalEventsAndTimeLine = getAllInterferringEventsAndTimeLineInCurrentEvaluationOldDesign(MyCalendarEvent, NoneCommitedCalendarEventsEvents, InterferringWithNowFlag, ExemptFromCalculation, new TimeLine(Now.constNow.AddDays(-90), Now.ComputationRange.End));
            List<SubCalendarEvent> collectionOfInterferringSubCalEvents = allInterferringSubCalEventsAndTimeLine.Item2.ToList();
            List<SubCalendarEvent> ArrayOfInterferringSubEvents = allInterferringSubCalEventsAndTimeLine.Item2.ToList();
            TimeLine RangeForScheduleUpdate = allInterferringSubCalEventsAndTimeLine.Item1;
            IEnumerable<SubCalendarEvent> NowEvents = allInterferringSubCalEventsAndTimeLine.Item4.Where(obj => !obj.getRigid).ToList();
            if (InterferringWithNowFlag == 0)
            {
                foreach (SubCalendarEvent eachSubCalendarEvent in NowEvents)
                {
                    //if(!eachSubCalendarEvent.Rigid)
                    {
                        eachSubCalendarEvent.SetAsRigid();
                    }
                }

            }
            Now.UpdateNow(RangeForScheduleUpdate.Start);


            TimeSpan SumOfAllEventsTimeSpan = Utility.SumOfActiveDuration(ArrayOfInterferringSubEvents);
            Tuple<List<SubCalendarEvent>, List<BlobSubCalendarEvent>> preppedDataForNExtStage = PrepareElementsThatWillNotFit(ArrayOfInterferringSubEvents, RangeForScheduleUpdate);
            ArrayOfInterferringSubEvents = preppedDataForNExtStage.Item1;

            Dictionary<CalendarEvent, List<SubCalendarEvent>> DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents = new Dictionary<CalendarEvent, List<SubCalendarEvent>>();
            List<SubCalendarEvent> RigidSubCalendarEvents = new List<SubCalendarEvent>(0);
            List<BusyTimeLine> RigidSubCalendarEventsBusyTimeLine = new List<BusyTimeLine>(0);
            RigidSubCalendarEvents = ArrayOfInterferringSubEvents.Where(obj => obj.getRigid).ToList();
            RigidSubCalendarEventsBusyTimeLine = RigidSubCalendarEvents.Select(obj => obj.ActiveSlot).ToList();

            double OccupancyOfTimeLineSPan = (double)SumOfAllEventsTimeSpan.Ticks / (double)RangeForScheduleUpdate.TimelineSpan.Ticks;
            //ArrayOfInterferringSubEvents = Utility.NotInList(ArrayOfInterferringSubEvents.ToList(), RigidSubCalendarEvents).ToList();//remove rigid elements


            TimeLine ReferenceTimeLine = RangeForScheduleUpdate.CreateCopy();
            ReferenceTimeLine.AddBusySlots(RigidSubCalendarEventsBusyTimeLine.ToArray());//Adds all the rigid elements

            TimeLine[] ArrayOfFreeSpots = getOnlyPertinentTimeFrame(getAllFreeSpots_NoCompleteSchedule(ReferenceTimeLine), ReferenceTimeLine).ToArray();
            ArrayOfFreeSpots = getOnlyPertinentTimeFrame(ArrayOfFreeSpots, ReferenceTimeLine).ToArray();
            
            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents = generateDictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents(ArrayOfInterferringSubEvents.ToList(), NoneCommitedCalendarEventsEvents);//generates a dictionary of a Calendar Event and the interferring events in the respective Calendar event


            List<BlobSubCalendarEvent> interFerringBlob = Utility.getConflictingEvents(RigidSubCalendarEvents);

            List<CalendarEvent> SortedInterFerringCalendarEvents_Deadline = DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Keys.ToList();
            SortedInterFerringCalendarEvents_Deadline = SortedInterFerringCalendarEvents_Deadline.OrderBy(obj => obj.End).ToList();
            DayTimeLine[] AllDays = Now.getAllDaysCount((uint)NumberOfDays).ToArray();
            ParallelizeCallsToDay(SortedInterFerringCalendarEvents_Deadline, ArrayOfInterferringSubEvents.ToList(),AllDays,false );
            List<SubCalendarEvent> ConflictingEvents = new List<SubCalendarEvent>();
            foreach(SubCalendarEvent eachSubCalendarEvent in ArrayOfInterferringSubEvents)
            {
                int index = (int) (eachSubCalendarEvent.UniversalDayIndex - Now.consttDayIndex);

                if((index<NumberOfDays)&&(index>=0))
                {
                    AssignedEvents[index].Add(eachSubCalendarEvent);
                }
                else 
                {
                    ConflictingEvents.Add(eachSubCalendarEvent);
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
        Dictionary<SubCalendarEvent, List<ulong>> DesignateRigidsTODays(DayTimeLine[] OrderedyAscendingAllDays, IEnumerable<SubCalendarEvent>AllRigidSubEvents)
        {
            ulong First = OrderedyAscendingAllDays.First().UniversalIndex;
            //ulong Last = OrderedyAscendingAllDays.Last().UniversalIndex;
            //ConcurrentBag<SubCalendarEvent>[] BagPerDay=OrderedyAscendingAllDays.Select(obj=>new ConcurrentBag<SubCalendarEvent>()).ToArray();
            Dictionary<SubCalendarEvent, List<ulong>> RetValue = new Dictionary<SubCalendarEvent, List<ulong>>();

            //Parallel.ForEach(AllRigidSubEvents, eachSubCalendarEvent =>

            foreach (SubCalendarEvent eachSubCalendarEvent in AllRigidSubEvents)
            {

                List<ulong> myDays = new List<ulong>();
                ulong SubCalFirstIndex = ReferenceNow.getDayIndexFromStartOfTime(eachSubCalendarEvent.Start);
                ulong SubCalLastIndex = ReferenceNow.getDayIndexFromStartOfTime(eachSubCalendarEvent.End);
                ulong DayDiff = SubCalLastIndex - SubCalFirstIndex;

                int BoundedIndex = (int)(SubCalFirstIndex - First);
                if ((BoundedIndex < 0)||(BoundedIndex >= OrderedyAscendingAllDays.Length))
                {
                    continue;
                }
                myDays.Add(SubCalFirstIndex);
                OrderedyAscendingAllDays[BoundedIndex].AddToSubEventList(eachSubCalendarEvent);
                eachSubCalendarEvent.updateDayIndex(SubCalFirstIndex);
                for (ulong i = SubCalFirstIndex + 1, j = 0; j < DayDiff; j++, i++)
                {
                    BoundedIndex = (int)(i - First);
                    if (BoundedIndex < OrderedyAscendingAllDays.Length)// in case the rigid sub event day index is higher than OrderedyAscendingAllDays max index
                    {
                        OrderedyAscendingAllDays[BoundedIndex].AddToSubEventList(eachSubCalendarEvent);
                        OrderedyAscendingAllDays[BoundedIndex].AddToSubEventList(eachSubCalendarEvent);
                        myDays.Add(i);
                    }

                }
                RetValue.Add(eachSubCalendarEvent, myDays);
            }
            //);
            return RetValue;
        }

        /// <summary>
        /// Funtion makes iterative calls to the daily optimizers
        /// </summary>
        /// <param name="AllDayTimeLine"></param>
        IDictionary<DayTimeLine, OptimizedPath> SortForSleep(IEnumerable<DayTimeLine>AllDayTimeLine)
        {
            ConcurrentDictionary<DayTimeLine, OptimizedPath> dayToOPtimization = new ConcurrentDictionary<DayTimeLine, OptimizedPath>();
            //Parallel.ForEach(AllDayTimeLine, EachDay=>
            Location home = null;
            if (Locations.ContainsKey("home"))
            {
                home = Locations["home"];
            }
            foreach (DayTimeLine EachDay in AllDayTimeLine)
            {
                HashSet<string> ids = new HashSet<string>(EachDay.getSubEventsInDayTimeLine().Select(subEvent => subEvent.getId));
                OptimizedPath dayPath = new OptimizedPath(EachDay, home);
                dayToOPtimization.AddOrUpdate(EachDay, dayPath, ((key, oldValue) => { return dayPath; }));
                dayPath.OptimizePath();
                optimizeDay(EachDay, dayPath.getSubevents());
                List<SubCalendarEvent> optimizedForDay = EachDay.getSubEventsInDayTimeLine().OrderBy(obj => obj.Start).ToList();
                

            }
            //);
            return dayToOPtimization;
        }

        void optimizeDay(DayTimeLine myDay, List<SubCalendarEvent> subEvents)
        {
            subEvents.ForEach(subEvent => subEvent.Conflicts.UpdateConflictFlag(false));
            List<SubCalendarEvent> orderedByStartSubEvents = subEvents.OrderBy(subEvent => subEvent.Start).ToList();
            TimeLine myTimeLine = myDay.getJustTimeLine();
            if(orderedByStartSubEvents.Count>0)
            {
                DateTimeOffset EarliestStart = myTimeLine.Start;// AllSubEvents.Max(obj => obj.Start);
                DateTimeOffset LatestEnd = myTimeLine.End;//AllSubEvents.Max(obj=>obj.End);
                
                int numberOfHoursBeforPinningCanStopcount = 10;

                DateTimeOffset LowestSubEventStart = orderedByStartSubEvents.Min(obj => obj.Start);
                DateTimeOffset HighestSubEventStart = orderedByStartSubEvents.Max(obj => obj.End);

                EarliestStart = LowestSubEventStart < EarliestStart ? LowestSubEventStart : EarliestStart;
                LatestEnd = HighestSubEventStart > LatestEnd ? HighestSubEventStart : LatestEnd;
                TimeLine RefTimeLine = new TimeLine(EarliestStart, LatestEnd);
                TimeLine LastSuccessfull = RefTimeLine.CreateCopy();
                RefTimeLine = new TimeLine(EarliestStart, LatestEnd);

                Dictionary<SubCalendarEvent, mTuple<TimeLine, TimeLine>> subEventssToTimelines = Utility.subEventToMaxSpaceAvailable(RefTimeLine, orderedByStartSubEvents);
                List<KeyValuePair<SubCalendarEvent, mTuple<TimeLine, TimeLine>>> kvpSubEventssToTimelines = subEventssToTimelines.ToList();
                Dictionary<SubCalendarEvent, List<double>> subEventToDimensions = kvpSubEventssToTimelines.ToDictionary(kvp => kvp.Key, kvp => getTimeLineSpaceAndTimeLineStart(RefTimeLine.End, kvp.Value.Item1));
                List<KeyValuePair<SubCalendarEvent, List<double>>> kvpSubEventToDimensions = subEventToDimensions.ToList();
                List<double> result= Utility.multiDimensionCalculation((kvpSubEventToDimensions.Select(obj => (IList<double>)obj.Value).ToList()));
                int maxIndex = result.MaxIndex();
                KeyValuePair<SubCalendarEvent, mTuple<TimeLine, TimeLine>> validKvp = kvpSubEventssToTimelines[maxIndex];
                TimeLine afterSleepTimeLine = new TimeLine(validKvp.Value.Item1.Start, RefTimeLine.End);

                int beginningIndex = orderedByStartSubEvents.IndexOf(validKvp.Key);
                List<SubCalendarEvent> subSetAfterSleepOfSubevent = orderedByStartSubEvents.GetRange(beginningIndex, orderedByStartSubEvents.Count - beginningIndex);
                List<SubCalendarEvent> subSetBeforeSleepOfSubevent = orderedByStartSubEvents.GetRange(0, beginningIndex);
                do
                {
                    LastSuccessfull = afterSleepTimeLine.CreateCopy();
                    EarliestStart = afterSleepTimeLine.Start.AddHours(1);
                    afterSleepTimeLine = new TimeLine(EarliestStart, LatestEnd);
                    --numberOfHoursBeforPinningCanStopcount;
                }
                while ((Utility.PinSubEventsToStart(subSetAfterSleepOfSubevent, afterSleepTimeLine)) && (numberOfHoursBeforPinningCanStopcount > 0));

                bool DidYouWork = Utility.PinSubEventsToStart(subSetAfterSleepOfSubevent, LastSuccessfull);
                ///First call tries to pin a subset of subevents  to  the section that can afford the max amount for sleep
                CreateBufferForEachEvent(subSetAfterSleepOfSubevent, LastSuccessfull);
                ///Second call pins the remaining that are not optimal to the previous day. It takes the beginning of the new day to the begin time of the preceding group. This takes advantage of the fact that CreateBufferForEachEvent favors pinning to start
                DateTimeOffset startTimeOfBeforeSet = LastSuccessfull.Start;
                if(subSetAfterSleepOfSubevent.Count > 0)
                {
                    startTimeOfBeforeSet = subSetAfterSleepOfSubevent.First().Start;
                }

                TimeLine beforeSleepTimeline = new TimeLine(myTimeLine.Start, startTimeOfBeforeSet);
                CreateBufferForEachEvent(subSetBeforeSleepOfSubevent, beforeSleepTimeline);
            }

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
        
        ulong ParallelizeCallsToDay(List<CalendarEvent> AllCalEvents,List<SubCalendarEvent> TotalActiveEvents, DayTimeLine[] AllDayTImeLine ,bool Optimize=true, bool preserveFirttwentyFourHours=true)
        {
            int TotalDays = (int)AllDayTImeLine.Length;
            Now.getAllDaysForCalc();
            ulong DayIndex = Now.consttDayIndex;
            ConcurrentBag< SubCalendarEvent>[] BagPerDay = new ConcurrentBag< SubCalendarEvent>[TotalDays];


            AllCalEvents.AsParallel().ForAll(obj => { obj.resetDesignationAllActiveEventsInCalculables(); obj.InitialCalculationLookupDays(AllDayTImeLine); });
            ILookup<ulong, SubCalendarEvent> SetForFirstDay = (new List<SubCalendarEvent>()).ToLookup(obj => (ulong)0, obj => obj);
            preserveFirttwentyFourHours = false;
            if (preserveFirttwentyFourHours)
            {
                SetForFirstDay = PrepFirstTwentyFOurHours(AllCalEvents, AllDayTImeLine[0].getJustTimeLine());
            }

            List<SubCalendarEvent> AllRigids = TotalActiveEvents.Where(obj => obj.getRigid).ToList();// you need to call this after PrepFirstTwentyFOurHours to prevent resetting of indexes
            DesignateRigidsTODays(AllDayTImeLine, AllRigids);

            int numberOfDays = AllDayTImeLine.Count();

            foreach (IGrouping<ulong, SubCalendarEvent> eachGrouping in SetForFirstDay)
            {
                int index = (int)(eachGrouping.Key - AllDayTImeLine[0].UniversalIndex);
                AllDayTImeLine[index].AddToSubEventList(SetForFirstDay[eachGrouping.Key]);
            }
            ConcurrentBag<List<SubCalendarEvent>> unassignedEvents = new ConcurrentBag<List<SubCalendarEvent>>();
            Dictionary<ulong, List<CalendarEvent>> DeadlineToCalEvents = new Dictionary<ulong, List<CalendarEvent>>();

            
            TotalActiveEvents.AsParallel().ForAll(obj => obj.enableCalculationMode());
            long totalRigidCount = TotalActiveEvents.Where(obj => obj.getRigid).LongCount();


            
            AllCalEvents.AsParallel().ForAll(obj => obj.initializeCalculablesAndUndesignables());
            Dictionary<string, CalendarEvent> DictOfCalEvents = AllCalEvents.ToDictionary(obj => obj.getId, obj => obj);
            Dictionary<string, SubCalendarEvent> DictOfSubEvents = TotalActiveEvents.ToDictionary(obj => obj.getId, obj => obj);
            
            Parallel.ForEach(AllDayTImeLine, (obj, state, index) => {
                obj.updateOccupancyOfTimeLine();
            });

            ulong totalDaysAvailable = (ulong)CalendarEvent.getUsableDaysTotal(AllCalEvents);
            ulong totalNumberOfEvents = (ulong)CalendarEvent.getUsableDaysTotal(AllCalEvents);


            while ((totalDaysAvailable>0)&&(totalNumberOfEvents>0))
            {
                long OldNumberOfAssignedElements = -1;
                long DesignatedAndAssignedSubEventCount = -1;
                do
                {
                    ConcurrentBag<CalendarEvent> UnUsableCalEvents = new ConcurrentBag<CalendarEvent>();
                    unassignedEvents = new ConcurrentBag<List<SubCalendarEvent>>();
                    BagPerDay = BagPerDay.Select(obj => new ConcurrentBag<SubCalendarEvent>()).ToArray();
                    OldNumberOfAssignedElements = DesignatedAndAssignedSubEventCount;

                    foreach (CalendarEvent eachCal in AllCalEvents)
                    {
                        eachCal.updateUnusableDaysAndRemoveDaysWithInsufficientFreeSpace();
                        List<DayTimeLine> DaysToUse ;
                        List<SubCalendarEvent> UndesignatedEvents = eachCal.AllUnDesignatedAndActiveSubEventsFromCalculables();
                        List<DayTimeLine> WorksWithProcrastination = eachCal.getDaysOnOrAfterProcrastination(true);
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
                                CurrDaysToUse = eachCal.getDaysOnOrAfterProcrastination(false, CurrDaysToUse);
                                if (CurrDaysToUse.Count > UndesignatedEvents.Count)
                                {
                                    DaysToUse = CurrDaysToUse;
                                    CurrDaysToUse = eachCal.getTimeLineWithoutMySubEventsAndEnoughDuration(false, CurrDaysToUse);
                                    if (CurrDaysToUse.Count > UndesignatedEvents.Count)
                                    {
                                        DaysToUse = CurrDaysToUse;
                                        CurrDaysToUse = eachCal.getDayTimeLineWhereOccupancyIsLess(0, false, CurrDaysToUse);
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
                                CurrDaysToUse = eachCal.getDaysOnOrAfterProcrastination(false);
                                if (CurrDaysToUse.Count > UndesignatedEvents.Count)
                                {
                                    DaysToUse = CurrDaysToUse;
                                    CurrDaysToUse = eachCal.getTimeLineWithoutMySubEventsAndEnoughDuration(false);
                                    if (CurrDaysToUse.Count > UndesignatedEvents.Count)
                                    {
                                        DaysToUse = CurrDaysToUse;
                                        CurrDaysToUse = eachCal.getDayTimeLineWhereOccupancyIsLess(0, false);
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
                            List<Tuple<ulong, SubCalendarEvent>> AllEvents = EvaluateEachDayIndexForEvent(UndesignatedEvents, DaysToUse);
                            Parallel.ForEach(AllEvents, eachTuple =>
                            {
                                BagPerDay[(int)(eachTuple.Item1 - DayIndex)].Add(eachTuple.Item2);
                            });
                        }
                        else
                        {
                            UnUsableCalEvents.Add(eachCal);
                        }
                    }
                    AllCalEvents = AllCalEvents.Except(UnUsableCalEvents).ToList();


                    for (int i = 0; i < numberOfDays; i++)
                    {
                        List<SubCalendarEvent> newSubEventAdditions = processTwentyFourHours(AllDayTImeLine[i], BagPerDay[i].ToList());
                        foreach (SubCalendarEvent eachSucal in newSubEventAdditions)
                        {
                            DictOfCalEvents[eachSucal.SubEvent_ID.getRepeatCalendarEventID()].removeDayTimeFromFreeUpdays(eachSucal.UniversalDayIndex);
                        }
                        unassignedEvents.Add(newSubEventAdditions);
                    }

                    AllDayTImeLine.AsParallel().ForAll(obj => obj.updateOccupancyOfTimeLine());
                    DesignatedAndAssignedSubEventCount = AllCalEvents.Sum(obj => obj.getNumberOfDesignatedAndActiveSubEventsFromCalculables());
                    AllCalEvents = CalendarEvent.removeCalEventsWitNoUndesignablesFromCalculables(AllCalEvents);
                }
                while (DesignatedAndAssignedSubEventCount != OldNumberOfAssignedElements);
                
                totalDaysAvailable = (ulong)CalendarEvent.getUsableDaysTotal(AllCalEvents);
                totalNumberOfEvents = (ulong)CalendarEvent.getTotalUndesignatedEvents(AllCalEvents);
            }


            List<SubCalendarEvent> undesignatedSubevents = TotalActiveEvents.Where(subEvent => !subEvent.isDesignated).ToList();
            List<SubCalendarEvent> orderedByStart = TotalActiveEvents.OrderBy(obj => obj.Start).ToList(); ;
            List<BlobSubCalendarEvent> beforePathOptimizationConflictingEvetns = Utility.getConflictingEvents(orderedByStart);


            int optimizedDayLimit = 10;
            IDictionary<DayTimeLine, OptimizedPath> dayToOptimization = null;
            if (Optimize)
            {
                ulong FirstIndex = AllDayTImeLine[0].UniversalIndex;
                try
                {
                    ILookup<ulong, SubCalendarEvent> DayToSubEvent = AllRigids.ToLookup(obj => obj.UniversalDayIndex, obj => obj);
                    foreach(IGrouping<ulong,SubCalendarEvent>eachGrouping in DayToSubEvent)
                    {
                        int currentIndex = (int)(eachGrouping.Key - FirstIndex);
                        AllDayTImeLine[currentIndex].AddToSubEventList(DayToSubEvent[eachGrouping.Key]);
                    }
                    List<DayTimeLine> OptimizedDays = AllDayTImeLine.Take(optimizedDayLimit).ToList();
                    dayToOptimization = SortForSleep(OptimizedDays);
                    
                }
                catch(Exception E)
                {
                    throw E;
                } 
            }
            List<BlobSubCalendarEvent> afterPathOptimizationConflictingEvetns = Utility.getConflictingEvents(TotalActiveEvents.OrderBy(obj => obj.Start).ToList());


            //foreach (SubCalendarEvent element in TotalActiveEvents.OrderBy(SubEvent => SubEvent.Start))
            //{
            //    Console.WriteLine(element.myLocation.justLongLatString());
            //}
            double distanceCovered = Location.calculateDistance(TotalActiveEvents.OrderBy(SubEvent=> SubEvent.Start).Select(SubEvent => SubEvent.Location).ToList(),0);
            Health scheduleHealth = new Health(TotalActiveEvents, Now.calculationNow, new TimeSpan(7,0,0,0), Now,this.getHomeLocation);

            //Console.WriteLine("Distance covered is {0}, Optimize is set to {1}\n Health Score is {2}", distanceCovered, Optimize, scheduleHealth.getScore());
            
            return totalNumberOfEvents;
        }

        


        ILookup<ulong, SubCalendarEvent> PrepFirstTwentyFOurHours(List<CalendarEvent> AllCalEvents, TimeLine FirstTwentyFour)
        {
            //TimeLine FirstTwentyFour = Now.firstDay.CreateCopy();// new TimeLine(Now.firstDay.Start, Now.firstDay.Start.AddDays(1));
            Dictionary<string, CalendarEvent> IDToCalendarEvent = AllCalEvents.ToDictionary(obj => obj.getId, obj => obj);
            TimeLine FirstFortyEight = new TimeLine(Now.firstDay.Start, Now.firstDay.Start.AddDays(2));
            List<Tuple<CalendarEvent, TimeLine>> ListOfTuples = AllCalEvents.Select(obj => new Tuple<CalendarEvent, TimeLine>(obj, obj.RangeTimeLine.InterferringTimeLine(FirstFortyEight))).Where(obj => obj.Item2 != null).ToList();
            ListOfTuples = ListOfTuples.Where(obj => ((obj.Item1.getRigid)||(new TimeLine(obj.Item2.End, obj.Item1.End).TimelineSpan < obj.Item1.AverageTimeSpanPerSubEvent))).ToList();//gets elements that have to exist within timeframe, checks if span per fit is less than the intersecting span greater than first forty eight
            List<SubCalendarEvent> ForCalculation =ListOfTuples .SelectMany(obj => obj.Item1.ActiveSubEvents).ToList();
            List<CalendarEvent> possibleCalEvents = AllCalEvents.Where(obj => obj.RangeTimeLine.InterferringTimeLine(FirstTwentyFour) != null).ToList();
            possibleCalEvents.AsParallel().ForAll(obj => obj.ActiveSubEvents.AsParallel().ForAll(obj1 => obj1.updateDayIndex(IDToCalendarEvent[obj1.SubEvent_ID.getRepeatCalendarEventID()])));
            ulong dayIndexLimit = Now.consttDayIndex +1;
            List<SubCalendarEvent> possibleSubCals = possibleCalEvents.SelectMany(obj => obj.ActiveSubEvents.Where(obj1 => obj1.UniversalDayIndex <= dayIndexLimit)).OrderBy(obj=>obj.Start).ToList();
            List<SubCalendarEvent> CurrentConstituents = possibleSubCals.Where(obj => obj.RangeTimeLine.InterferringTimeLine(FirstTwentyFour)!=null).ToList();
            
            List<SubCalendarEvent> AllRigids =  ForCalculation.Where(obj => obj.getRigid).ToList();

            ForCalculation = ForCalculation.Except(AllRigids).ToList();
            ForCalculation.ForEach(obj => obj.addReasons(new PreservedOrder(ForCalculation.Select(subEvent => subEvent.SubEvent_ID).ToList())));

            HashSet<SubCalendarEvent> OrderedPreviousTwentyfourNorigids = new HashSet<SubCalendarEvent>( ForCalculation.OrderBy(obj=>obj.Start));
            FirstTwentyFour.AddBusySlots(AllRigids.Select(obj => obj.ActiveSlot));
            List<SubCalendarEvent> PopulatedSubcals = BuildAllPossibleSnugLists(ForCalculation, FirstTwentyFour, 1, new List<SubCalendarEvent>());
            //loop updates reason for current assignment
            foreach (SubCalendarEvent subcalendaEvent in PopulatedSubcals.Except(ForCalculation))
            {
                BestFitReason bestFit = new BestFitReason(FirstTwentyFour.TotalActiveSpan, FirstTwentyFour.TotalFreeSpotAvailable, subcalendaEvent.getActiveDuration);
                subcalendaEvent.addReasons(bestFit);
            }

            List<SubCalendarEvent> retValue =new List<SubCalendarEvent>();
            List<SubCalendarEvent> PopulatedSubcalsCpy = PopulatedSubcals.ToList();

            IEnumerable<TimeLine> freeSpots=FirstTwentyFour.getAllFreeSlots();
            foreach(TimeLine eachTimeLine in freeSpots)
            {
                List<SubCalendarEvent> AllreadyCOnstituents = PopulatedSubcalsCpy.Where(obj => obj.RangeTimeLine.InterferringTimeLine(eachTimeLine) != null).ToList();

                List<SubCalendarEvent> newlyAssignedAssignments = PreserveFirstTwentyFourHours(AllreadyCOnstituents, OrderedPreviousTwentyfourNorigids.ToList(), eachTimeLine);
                
                foreach(SubCalendarEvent eachSubCalendarEvent in newlyAssignedAssignments)
                {
                    OrderedPreviousTwentyfourNorigids.Remove(eachSubCalendarEvent);
                    PopulatedSubcalsCpy.Remove(eachSubCalendarEvent);
                }
                retValue.AddRange(newlyAssignedAssignments);
            }

            //List<SubCalendarEvent> retValue = PreserveFirstTwentyFourHours(PopulatedSubcals, OrderedPreviousTwentyfour, FirstTwentyFour);
            possibleCalEvents.AsParallel().ForAll(obj => obj.ActiveSubEvents.AsParallel().ForAll(obj1 => obj1.resetPreferredDayIndex()));
            retValue.ForEach(obj => obj.updateDayIndex((IDToCalendarEvent[obj.SubEvent_ID.getRepeatCalendarEventID()])));
            ILookup<ulong, SubCalendarEvent> retVal_Dict = retValue.ToLookup(obj => obj.UniversalDayIndex, obj => obj);

            return retVal_Dict;
        }


        List<SubCalendarEvent> processTwentyFourHours(DayTimeLine myDayTimeLine, List<SubCalendarEvent> AllSubEvents)//,List<BusyTimeLine>BusySlots)
        {
            ++CountCall;
            List<SubCalendarEvent> AllreadyAssigned = myDayTimeLine.getSubEventsInDayTimeLine();
            List<SubCalendarEvent> AllRigids = AllSubEvents.Concat(AllreadyAssigned).Where(obj => obj.getRigid).ToList();
            
            List<SubCalendarEvent> Movables = AllSubEvents.Except(AllRigids ).ToList();
            if (Movables.Count < 1)
            {
                return AllRigids;
            }
            Location AvgLocation = Location.AverageGPSLocation((AllRigids.Concat(myDayTimeLine.getSubEventsInDayTimeLine())).Select(obj => obj.Location));
            SubCalendarEvent.resetScores(AllSubEvents);
            /*
            TimeLine timeLineForCalc = new TimeLine(myDayTimeLine.Start, myDayTimeLine.End);
            timeLineForCalc.AddBusySlots(BusySlots);
             //*/
            TimeLine timeLineForCalc = myDayTimeLine;

            //Movables = stitchRestrictedSubCalendarEvent(timeLineForCalc.getAllFreeSlots().ToList(), Movables).SelectMany(obj => obj.Value).ToList();

            Tuple<TimeLine, Double> BaseLine = getAllReferenceCalculation(Movables, myDayTimeLine, AvgLocation);
            Movables = Movables.OrderBy(obj => obj.Score).ThenBy(obj=>obj.fittability).ToList();
            SubCalendarEvent.updateDayIndex(0, Movables);
            
            List<SubCalendarEvent> Calculables = Movables .Count>6?Movables.GetRange(0, 7):Movables.ToList();
            
            
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

            HashSet<SubCalendarEvent> ReassignedHashDayTime = new HashSet<SubCalendarEvent>(myDayTimeLine.getSubEventsInDayTimeLine());
            HashSet<SubCalendarEvent> diff = new HashSet<SubCalendarEvent>(ReassignedHashDayTime.Except(ReassignedHash));
            SubCalendarEvent.updateDayIndex(myDayTimeLine.UniversalIndex, Reassigned.Concat(AllRigids));
            return Reassigned;
        }



        Tuple<TimeLine, Double> getAllReferenceCalculation(IEnumerable<SubCalendarEvent> AllSubEvents, TimeLine DayTimeLine, Location rigidLocation)
        {
            Dictionary<SubCalendarEvent, TimeLine> SUbEventTOInterferringTimeLine = AllSubEvents.ToDictionary(obj => obj, obj => new TimeLine(DayTimeLine.Start > obj.getProcrastinationInfo.PreferredStartTime ? DayTimeLine.Start : obj.getProcrastinationInfo.PreferredStartTime, obj.getCalendarEventRange.End));//selects the relevenat timeline per subeevent. Tries to use the prcrastination info to provide better preferenceing
            Dictionary<SubCalendarEvent, double> Distances = AllSubEvents.ToDictionary(obj => obj, obj => Location.calculateDistance(rigidLocation,obj.Location));
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
            Dictionary<SubCalendarEvent,List<double>> subEventToMultiDimenstionVars = AllSubEvents.ToDictionary(obj=>obj, obj=>new List<double>());
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
            foreach(SubCalendarEvent subcalendarEvent in subEventToMultiDimenstionVars.Keys) 
            {
                subcalendarEvent.incrementScore( resultants[i]);
                i++;
            }
            Tuple<TimeLine, Double> retValue = new Tuple<TimeLine, double>(BestTimeLine.Value, LongestDistance.Value);
            return retValue;
        }

        /// <summary>
        /// Function selects the days to be used for the calculation of a schedule. All SubEvents need to be part of the same Calendar event.
        /// </summary>
        /// <param name="AllSubEvents"></param>
        /// <param name="AllDays"></param>
        /// <returns></returns>
        List<Tuple<ulong,SubCalendarEvent>>EvaluateEachDayIndexForEvent(List<SubCalendarEvent> AllSubEvents, List<DayTimeLine>AllDays )
        {
            List<Tuple<ulong,SubCalendarEvent>> retValue = new List<Tuple<ulong,SubCalendarEvent>>();
            if (AllSubEvents.Count > 0)
            {
                Procrastination procrastinationProfile = AllSubEvents[0].getProcrastinationInfo;
                ulong PreferrdDayIndex = ReferenceNow.getDayIndexFromStartOfTime(procrastinationProfile.PreferredStartTime);

                List<mTuple<bool, DayTimeLine>> OptimizedDayTimeLine = AllDays.Select(obj => new mTuple<bool, DayTimeLine>(((long)(obj.UniversalIndex - PreferrdDayIndex) >= 0), obj)).ToList();//this line orders Daytimeline by  if they are after the procrastination day.
              
                //AllDays = AllDays.OrderBy(obj => obj.Occupancy).ToList();
                AllDays = OptimizedDayTimeLine.OrderByDescending(obj => obj.Item1).ThenBy(obj => obj.Item2.Occupancy).Select(obj => obj.Item2).ToList();//
                

                int NumberOfDaysPossible = AllDays.Count;// (IndexRange.Item2 - IndexRange.Item1) + 1;
                int iniRation = (int)AllSubEvents.Count / NumberOfDaysPossible;
                int AllSubEventsStartingIndex = 0;
                //ulong rangeDayIndex = Now.getDayIndexFromStartOfTime(PossibLeRangeForCalc.Start);
                for (int i = 0; i < iniRation; i++)
                {
                    ulong StartIndex = 0;// AllDays[0].UniversalIndex;
                    for (int j = 0; j < NumberOfDaysPossible; j++, ++StartIndex, ++AllSubEventsStartingIndex)
                    {
                        retValue.Add(new Tuple<ulong, SubCalendarEvent>(AllDays[j].UniversalIndex, AllSubEvents[AllSubEventsStartingIndex]));
                    }
                }

                int UnAssignedCount = AllSubEvents.Count - AllSubEventsStartingIndex;
                if (UnAssignedCount != 0)
                { 
                    int Spacing = (NumberOfDaysPossible / UnAssignedCount);
                    int Delta = 0;
                    int iniindex = 0;


                    for (int i = AllSubEventsStartingIndex, j = 0; i < AllSubEvents.Count; i++, j++)
                    {
                        SubCalendarEvent refSubEvent = AllSubEvents[i];
                        int MultiPlicator = j * Spacing;
                        Delta = MultiPlicator;
                        if (MultiPlicator < NumberOfDaysPossible)
                        {
                            ;
                        }
                        else
                        {
                            j = 0;
                            ++iniindex;
                            Delta = 0;
                        }

                        //ulong DayIndex = rangeDayIndex + (ulong)(Delta + iniindex);
                        ulong DayIndex = AllDays[(Delta + iniindex)].UniversalIndex;
                        retValue.Add(new Tuple<ulong, SubCalendarEvent>(DayIndex, refSubEvent));
                    }
                }
            }
            return retValue;

        }

        List<SubCalendarEvent> BuildAllPossibleSnugLists(List<SubCalendarEvent> DictionaryWithBothCalendarEventsAndListOfInterferringSubEvents, TimeLine ReferenceTimeLine, double Occupancy, List<SubCalendarEvent> AlreadyAssigneEvents)
        {
            TimeLine[] JustFreeSpots = getAllFreeSpots_NoCompleteSchedule(ReferenceTimeLine).OrderByDescending(obj => obj.End).ToArray();


            List<SubCalendarEvent> MyListOfSubCalendarEvents = DictionaryWithBothCalendarEventsAndListOfInterferringSubEvents.ToList();
            List<SubCalendarEvent> ListOfAlreadyAssignedSubCalendarEvents = new System.Collections.Generic.List<SubCalendarEvent>();
            List<TimeSpan> ListOfAllInterferringTimeSpans = new List<TimeSpan>();
            TimeLine FirstTwentyFOurTimeLine = new TimeLine();
            List<mTuple<SubCalendarEvent, TimeLine>> FirstTwentyFourHours = new List<mTuple<SubCalendarEvent, TimeLine>>();
            List<SubCalendarEvent> ListOfAllInterferringSubCalendarEvents = new List<SubCalendarEvent>();// AlreadyAssigneEvents.ToList();
            
            if (JustFreeSpots.Length > 0)
            {
                DateTimeOffset refStartTime24Hour = Now.calculationNow;
                FirstTwentyFOurTimeLine = new TimeLine(refStartTime24Hour, refStartTime24Hour.AddDays(1));
            }

            List<SubCalendarEvent> retValue = new List<SubCalendarEvent>();

            foreach (SubCalendarEvent MySubEvents in MyListOfSubCalendarEvents)//Loop creates a List of interferring SubCalendarEvens
            {
                //foreach (SubCalendarEvent MySubEvents in MyList)
                {
                    ListOfAllInterferringSubCalendarEvents.Add(MySubEvents);
                    ListOfAllInterferringTimeSpans.Add(MySubEvents.ActiveSlot.BusyTimeSpan);
                    if (FirstTwentyFOurTimeLine.InterferringTimeLine(MySubEvents.RangeTimeLine) != null)
                    {
                        FirstTwentyFourHours.Add(new mTuple<SubCalendarEvent, TimeLine>(MySubEvents, MySubEvents.RangeTimeLine));
                    }
                }
            }
            FirstTwentyFourHours = FirstTwentyFourHours.OrderBy(obj => obj.Item1.Start).ToList();//orders the element by start time in order to preserve order for later calculation
            
            
            List<SubCalendarEvent> AllInterferringSubEvents= DictionaryWithBothCalendarEventsAndListOfInterferringSubEvents.ToList();
            List<SubCalendarEvent> NoChangeTOOrder_AllInterferringSubEvents = AllInterferringSubEvents.ToList();
            List<SubCalendarEvent> AllInterferringSubEvents_Cpy = AllInterferringSubEvents.ToList();
            
            
            HashSet<SubCalendarEvent> AllInterferringSubEvents_Cpy_Hash = new HashSet<SubCalendarEvent>(AllInterferringSubEvents_Cpy.Concat(AlreadyAssigneEvents));
            AllInterferringSubEvents_Cpy = AllInterferringSubEvents_Cpy_Hash.Except(AlreadyAssigneEvents).ToList();
            Dictionary<string, SubCalendarEvent> AllEvents_Dict=AllInterferringSubEvents_Cpy_Hash.ToDictionary(obj=>obj.getId,obj=>obj);
            Dictionary<TimeLine, List<SubCalendarEvent>> Dict_ConstrainedElements;
            if (AlreadyAssigneEvents.Count > 0)
            {
                Dict_ConstrainedElements = JustPickCollidingTimeline(JustFreeSpots.ToList(), new HashSet<SubCalendarEvent>(AlreadyAssigneEvents).ToList());
            }
            else
            { 
                Dict_ConstrainedElements= generateConstrainedList(JustFreeSpots.ToList(), AllInterferringSubEvents_Cpy);
                Dict_ConstrainedElements = stitchRestrictedSubCalendarEvent(JustFreeSpots.ToList(), Dict_ConstrainedElements);
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
                
                List<SubCalendarEvent> reassignedElements= stitchUnRestrictedSubCalendarEvent(JustFreeSpots[i], COnstrainedElementsForTimeLine, AllInterferringSubEvents_Cpy.ToList());
                DebugCounter++;
                foreach (SubCalendarEvent eachSubCalendarEvent in reassignedElements)
                {
                    AllReassignedElements.Add(eachSubCalendarEvent.getId, eachSubCalendarEvent);
                    AllInterferringSubEvents_Cpy.Remove(eachSubCalendarEvent);
                    AllInterferringSubEvents_Cpy_Hash.Remove(eachSubCalendarEvent);
                }

                JustFreeSpots_Cpy.RemoveAt(0);
                if ((JustFreeSpots_Cpy.Count > 0)&&(AlreadyAssigneEvents.Count==0))
                {
                    SubCalendarEvent.updateMiscData(AllInterferringSubEvents_Cpy,0);
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

            try
            {
                
                /*

                OPtimizeNextSevenDays(new List<CalendarEvent>(){ToBeFittedTimeLine});
            //*/
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            

            //triggerTimer();
            return retValue;
        }

        List<TimeLine> reorderFreeSpotBasedOnTimeSpanAndEndtime(IEnumerable<TimeLine> AllTImeLines)
        {
            Dictionary<TimeSpan, List<TimeLine>> TImespanTOEvents = new Dictionary<TimeSpan, List<TimeLine>>();
            IList<KeyValuePair<TimeSpan, List<TimeLine>>> TImespanTOEvents_list;
            foreach (TimeLine eachTimeLine in AllTImeLines)
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
            
            foreach (KeyValuePair<TimeSpan,List<TimeLine>> eachKeyValuePair in TImespanTOEvents)
            {
                ret_Prepare.Add(eachKeyValuePair.Key, eachKeyValuePair.Value.OrderByDescending(obj => obj.End).ToList());//gets each TImespan keyvaluepair orders, descendingly, based on the end time. THis ensures that the timeline from the end get picked first
            }

            List<TimeLine> retValue = ret_Prepare.OrderBy(obj => obj.Key).SelectMany(obj=>obj.Value).ToList();//orders timeline timespan. Ensutres smaller timelines are at first.
            return retValue;
        }

        /// <summary>
        /// Function tries to preserve the order of the first twenty four hours. OrderedPreviousTwentyFOurHours is elements that were in twentyfour hours before calculation. Not it has to be ordered by the start time before calculations were triggered. You also need to make sure preference for rigids/blobs are placed. Essentially rigids/blobs should have to be higher in the sequence
        /// </summary>
        /// <param name="CurrentConstituents"></param>
        /// <param name="OrderedPreviousTwentyFOurHours"></param>
        /// <param name="refTImeLine"></param>
        /// <returns></returns>
        List<SubCalendarEvent> PreserveFirstTwentyFourHours(List<SubCalendarEvent> CurrentConstituents, List<SubCalendarEvent>OrderedPreviousTwentyFOurHours, TimeLine refTImeLine)
        {
            List<SubCalendarEvent> iniParam_CurrentConstituents = CurrentConstituents.ToList();
            List<SubCalendarEvent> iniParam_OrderedPreviousTwentyFOurHours = OrderedPreviousTwentyFOurHours.ToList();
            TimeLine iniParam_refTImeLine = refTImeLine.CreateCopy();

            int i=0;

             Dictionary<string, Tuple<SubCalendarEvent,int>> Dict_IDToTuple =  OrderedPreviousTwentyFOurHours.ToDictionary(obj => obj.getId, obj => new Tuple<SubCalendarEvent,int>( obj,i++));

            
            


            CurrentConstituents = iniParam_CurrentConstituents.ToList();
            OrderedPreviousTwentyFOurHours = iniParam_OrderedPreviousTwentyFOurHours.ToList();
            refTImeLine = iniParam_refTImeLine.CreateCopy();




            
            TimeLine refTImeLine_Ini = refTImeLine.CreateCopy();

            CurrentConstituents = CurrentConstituents.OrderBy(obj => obj.Start).ToList();

            HashSet<SubCalendarEvent> AllEvents = new HashSet<SubCalendarEvent>(CurrentConstituents);
            ///*
             while (true)//loop tries to check if there are any elements clashing with the reftimeLine start time. If it does you want the start time to get readjusted to the endtime of the clashing event
            {
                IEnumerable<SubCalendarEvent>interferringWithStartTime=AllEvents.Where(obj => obj.RangeTimeLine.IsDateTimeWithin(refTImeLine_Ini.Start));
                if ((interferringWithStartTime.Count()>0)&&(refTImeLine_Ini.TimelineSpan.Ticks>0))
                {
                    AllEvents = new HashSet<SubCalendarEvent>(AllEvents.Except(interferringWithStartTime));
                    CurrentConstituents = CurrentConstituents.Except(interferringWithStartTime).ToList();
                    OrderedPreviousTwentyFOurHours = OrderedPreviousTwentyFOurHours.Except(interferringWithStartTime).ToList();
                    refTImeLine_Ini = new TimeLine(interferringWithStartTime.Select(obj => obj.End).Max(), refTImeLine_Ini.End);
                }
                else
                {
                    break;
                }

            }
             AllEvents = new HashSet<SubCalendarEvent>(CurrentConstituents.Concat(OrderedPreviousTwentyFOurHours));
            if ((AllEvents.Count < 1) || (refTImeLine_Ini.TimelineSpan.Ticks<1))
            {
                return new List<SubCalendarEvent>();
            }
            //*/

            
            List<SubCalendarEvent> CurrentConstituents_ini = CurrentConstituents.ToList();
            IEnumerable<SubCalendarEvent> ordered_CurrentConstituents = CurrentConstituents.OrderBy(obj => obj.End);
            List<SubCalendarEvent> OrderedPreviousTwentyFOurHours_ini = OrderedPreviousTwentyFOurHours.ToList();

            CurrentConstituents = CurrentConstituents.ToList();
            refTImeLine = refTImeLine.CreateCopy();
            
            

            OrderedPreviousTwentyFOurHours = OrderedPreviousTwentyFOurHours.Distinct().ToList();

            List<SubCalendarEvent> AlreadyInCurrentConstituents = CurrentConstituents.Intersect(OrderedPreviousTwentyFOurHours).ToList();

            List<SubCalendarEvent> AlreadyInCurrentConstituents_ini = AlreadyInCurrentConstituents.ToList();
            //HashSet<SubCalendarEvent> AllreadyInConstituents_Hash = new HashSet<SubCalendarEvent>(AlreadyInCurrentConstituents_ini);
            HashSet<SubCalendarEvent> OrderedPreviousTwentyFOurHours_Hash = new HashSet<SubCalendarEvent>(OrderedPreviousTwentyFOurHours);
            HashSet<SubCalendarEvent> AllreadyInConstituents_HashOrdered = new HashSet<SubCalendarEvent>();
            
            foreach (SubCalendarEvent eachSubcalendarEvent in OrderedPreviousTwentyFOurHours)
            {
                if (AlreadyInCurrentConstituents.Contains(eachSubcalendarEvent))
                {
                    AllreadyInConstituents_HashOrdered.Add(eachSubcalendarEvent);
                }
            }

            AlreadyInCurrentConstituents=AlreadyInCurrentConstituents.OrderBy(obj => obj.End).ToList();
            HashSet<SubCalendarEvent> CurrentConstituents_hash = new HashSet<SubCalendarEvent>(CurrentConstituents);

            TimeLine myTimeLines = refTImeLine_Ini.CreateCopy();
            List<SubCalendarEvent> TempHolder = CurrentConstituents.OrderBy(obj => obj.End).ToList();
            TempHolder.Reverse();
            foreach (SubCalendarEvent eachSubCalendarEvent0 in TempHolder)
            {
                bool pinSuccess = eachSubCalendarEvent0.PinToEnd(myTimeLines);
                
                if (!pinSuccess && !eachSubCalendarEvent0.getRigid)
                {
                    throw new Exception("Theres an issue with preserving the first twenty four. Check pin to start 1");
                }

                myTimeLines = new TimeLine(refTImeLine_Ini.Start, eachSubCalendarEvent0.Start);
            }


            /*
            if (!Utility.PinSubEventsToEnd(CurrentConstituents, refTImeLine_Ini))
            {
                throw new Exception("Theres an issue with preserving the first twenty four. Check pin to start 1");
            }

            */
            List<SubCalendarEvent> newlyArranged = new List<SubCalendarEvent>();
            List<SubCalendarEvent> restrictedElements = new List<SubCalendarEvent>();
            HashSet <SubCalendarEvent> restrictedElements_Updated = new HashSet<SubCalendarEvent>(CurrentConstituents_hash);//restricted elements  will be souced from this for iterative call of stitchunrestricted
            List<SubCalendarEvent> possibleElements = new List<SubCalendarEvent>();
            TimeLine IterarionrefTimeLine = refTImeLine_Ini.CreateCopy();

            List<SubCalendarEvent> retValue = restrictedElements_Updated.OrderBy(obj => obj.End).ToList();
            Dictionary<SubCalendarEvent, DateTimeOffset> preservedStartTime = new Dictionary<SubCalendarEvent, DateTimeOffset>();
            CountCall = 0;
            foreach (SubCalendarEvent eachSubCalendarEvent in OrderedPreviousTwentyFOurHours_Hash)
            {
                possibleElements.Clear();
                preservedStartTime = AllEvents.ToDictionary(obj => obj, obj => obj.Start);
                IterarionrefTimeLine=refTImeLine_Ini.CreateCopy();
                restrictedElements = restrictedElements_Updated.OrderBy(obj => obj.End).ToList();
                DateTimeOffset iterationEndTime = refTImeLine_Ini.End;
                /*
                if (!Utility.PinSubEventsToEnd(restrictedElements_Updated.OrderBy(obj=>obj.End), refTImeLine_Ini))
                {
                    throw new Exception("Theres an issue with preserving the first twenty four. Check pin to end 1");
                }
                */
                TimeLine myTimeLines0 = refTImeLine_Ini.CreateCopy();
                TempHolder = restrictedElements_Updated.OrderBy(obj => obj.End).ToList();
                TempHolder.Reverse();
                foreach (SubCalendarEvent eachSubCalendarEvent0 in TempHolder)
                {
                    bool pinSuccess = eachSubCalendarEvent0.PinToEnd(myTimeLines0);
                    myTimeLines0 = new TimeLine(refTImeLine_Ini.Start, eachSubCalendarEvent0.Start);
                    if (!pinSuccess && !eachSubCalendarEvent0.getRigid)
                    {
                        throw new Exception("Invalid list used in CreateBufferForEachEvent");//hack alert we need to involve conflicting events
                    }
                }


                if (AllreadyInConstituents_HashOrdered.Remove(eachSubCalendarEvent))
                {
                    IterarionrefTimeLine = refTImeLine_Ini.CreateCopy();
                    restrictedElements = restrictedElements_Updated.OrderBy(obj => obj.End).ToList();
                    restrictedElements.Remove(eachSubCalendarEvent);//removing from restricted list so that it is not part of restricted element in call to stitcunrestricted. If included It will not be allowedd to move to the possble earlier position.

                    if (AllreadyInConstituents_HashOrdered.Count > 0)
                    {
                        List<SubCalendarEvent> myrestrictedElements_Updated=restrictedElements_Updated.OrderBy(obj => obj.End).ToList();
                        myrestrictedElements_Updated.Remove(eachSubCalendarEvent);
                        if(!Utility.PinSubEventsToEnd(myrestrictedElements_Updated, IterarionrefTimeLine))
                        {
                            throw new Exception("Invalid use of PreserveFirstTwentyFourHours");//hack alert we need to involve conflicting events
                        }
                        iterationEndTime = AllreadyInConstituents_HashOrdered.OrderBy(obj=>obj.Start).First().Start;
                        IterarionrefTimeLine = new TimeLine(IterarionrefTimeLine.Start, iterationEndTime);//if there is already an element from the ordered 24, let it's start time be the end time for the ref timeline in stitch unrestricted. This ensures that elements preserve their order in the calculation.
                        restrictedElements = getInterferringSubEvents(IterarionrefTimeLine, myrestrictedElements_Updated).OrderBy(obj => obj.End).ToList();
                    }
                    
                    
                    possibleElements = restrictedElements.ToList();
                    possibleElements.Add(eachSubCalendarEvent);
                }
                else 
                {
                    restrictedElements = restrictedElements_Updated.OrderBy(obj => obj.End).ToList();
                    restrictedElements.Remove(eachSubCalendarEvent);
                    if (AllreadyInConstituents_HashOrdered.Count > 0)
                    {
                        iterationEndTime = AllreadyInConstituents_HashOrdered.OrderBy(obj => obj.Start).First().Start;
                        IterarionrefTimeLine = new TimeLine(IterarionrefTimeLine.Start, iterationEndTime);//if there is already an element from the ordered 24, let it's start time be the end time for the ref timeline in stitch unrestricted. This ensures that elements preserve their order in the calculation.
                        restrictedElements = getInterferringSubEvents(IterarionrefTimeLine, restrictedElements_Updated).OrderBy(obj => obj.End).ToList();
                    }
                    possibleElements = restrictedElements.ToList();
                    possibleElements.Add(eachSubCalendarEvent);
                }
                if (!Utility.PinSubEventsToEnd(restrictedElements, IterarionrefTimeLine))
                {
                    throw new Exception("Invalid use of PreserveFirstTwentyFourHours");//hack alert we need to involve conflicting events
                }

                List<SubCalendarEvent> reassignedElements = StitchUnrestricted(IterarionrefTimeLine, restrictedElements, possibleElements,false);
                if (reassignedElements.Count > restrictedElements.Count)
                {
                    restrictedElements_Updated.Add(eachSubCalendarEvent);
                }
                else 
                {
                    preservedStartTime.AsParallel().ForAll(obj =>
                    {
                        obj.Key.shiftEvent((obj.Value - obj.Key.Start));//if there are no new additions send all events to initial positions
                    });
                }
                retValue = restrictedElements_Updated.OrderBy(obj => obj.End).ToList();
                //CountCall++;
            }
            retValue = restrictedElements_Updated.OrderBy(obj=>obj.End).ToList();
            retValue = OrderSubEventsBasedOnOrder(retValue,Dict_IDToTuple,refTImeLine_Ini);
            return retValue;
        }


        List<SubCalendarEvent> OrderSubEventsBasedOnOrder(IEnumerable<SubCalendarEvent> SomeOrderedEvents, Dictionary<string, Tuple<SubCalendarEvent,int>>SubEventToOrder,TimeLine RestrictingTimeLine)
        {
            List<SubCalendarEvent> InitalOrder = SomeOrderedEvents.ToList();
            List<SubCalendarEvent> RetValue = new List<SubCalendarEvent>();
            List<Tuple<SubCalendarEvent,int>> OrderedSubEVents =  SomeOrderedEvents.Where(obj=>SubEventToOrder.ContainsKey(obj.getId)).Select(obj=>SubEventToOrder[obj.getId]).OrderBy(obj=>obj.Item2).ToList();
            List<SubCalendarEvent> NotInOrder = SomeOrderedEvents.Where(obj=>!SubEventToOrder.ContainsKey(obj.getId)).ToList();


            List<SubCalendarEvent> CurrentList =InitalOrder.ToList();
            int i=0;
            int preferredIndex = i;//Index to try insertion of next sub event
            for(; i<OrderedSubEVents.Count;i++)
            {
                SubCalendarEvent MySub= OrderedSubEVents[i].Item1;
                List<SubCalendarEvent> NextSeries = CurrentList.ToList();
                NextSeries.Remove(MySub);
                if(i<OrderedSubEVents.Count-1)
                {
                    int j = preferredIndex;
                    for (; j < NextSeries.Count - 1; j++)
                    {
                        NextSeries.Insert(j,MySub);
                        if(Utility.PinSubEventsToStart(NextSeries,RestrictingTimeLine))
                        {
                            CurrentList=NextSeries;
                            preferredIndex = j+1;
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
        /// This creates the time span for sleeping. NOTE: None of the events must conflict for this to run correctly
        /// </summary>
        /// <param name="AllEvents"></param>
        /// <param name="restrictingTimeline"></param>
        void CreateBufferForEachEvent(List<SubCalendarEvent> AllEvents, TimeLine restrictingTimeline)
        {
            if (AllEvents.Count < 1)
            {
                return;
            }

            if (AllEvents.Count < 1)
            {
                return;
            }
            HashSet<SubCalendarEvent> allEventshash = new HashSet<SubCalendarEvent>();
            foreach(SubCalendarEvent eachSubcalendarEvent in AllEvents)
            {
                allEventshash.Add(eachSubcalendarEvent);
            }

            AllEvents = allEventshash.ToList();
            List<SubCalendarEvent> justThemRigids = AllEvents.Where(obj => obj.getRigid).ToList();

            TimeLine myTimeLines = new TimeLine(restrictingTimeline.Start, restrictingTimeline.End);
            List<SubCalendarEvent> allEvents_reversed=AllEvents.ToList();
            allEvents_reversed.Reverse();
            foreach (SubCalendarEvent eachSubCalendarEvent in allEvents_reversed)
            {
                TimeLine oldTimeLine = myTimeLines;
                bool pinSuccess=eachSubCalendarEvent.PinToEnd(myTimeLines);
                myTimeLines = new TimeLine(restrictingTimeline.Start, eachSubCalendarEvent.Start);
                if (!pinSuccess && !eachSubCalendarEvent.getRigid)
                {
                    throw new Exception("Invalid list used in CreateBufferForEachEvent");//hack alert we need to involve conflicting events
                }

            }
            

            TimeSpan bufferPerMile = new TimeSpan(0, 12, 0);
            
            Dictionary<Tuple<SubCalendarEvent, SubCalendarEvent>, double> beforeAfterDistance = new Dictionary<Tuple<SubCalendarEvent, SubCalendarEvent>, double>();
            Dictionary<SubCalendarEvent, mTuple<DateTimeOffset, DateTimeOffset>> beforeFailing = new Dictionary<SubCalendarEvent, mTuple<DateTimeOffset, DateTimeOffset>>();
            
            //AllEvents.Reverse();
            int j=0;
            TimeLine referencePinningTImeline = new TimeLine(restrictingTimeline.Start, restrictingTimeline.End);
            SubCalendarEvent firstEvent = AllEvents[0];
            if (!firstEvent.PinToStart(referencePinningTImeline) && !firstEvent.getRigid)
            {
                throw new Exception("this is a weird bug to have in CreateBufferForEachEvent");
            }


            for (int i = 0; i < AllEvents.Count - 1; i++)
            {
                j = i + 1;
                Tuple<SubCalendarEvent, SubCalendarEvent> myCoEvents = new Tuple<SubCalendarEvent, SubCalendarEvent>(AllEvents[i], AllEvents[j]);
                
                TimeSpan bufferSpan = Location.getDrivingTimeFromWeb(myCoEvents.Item1.Location, myCoEvents.Item2.Location);
                if (bufferSpan.Ticks < 0)
                {
                    double distance = SubCalendarEvent.CalculateDistance(myCoEvents.Item1, myCoEvents.Item2, -1);
                    if (distance < .5)
                    {
                        distance = 1;
                    }
                    bufferSpan = new TimeSpan((long)(bufferPerMile.Ticks * distance));
                }

                
                referencePinningTImeline = new TimeLine(myCoEvents.Item1.End.Add(bufferSpan), myCoEvents.Item2.End);
                if (!myCoEvents.Item2.PinToStart(referencePinningTImeline))
                {
                    //break;
                }
            }
        }


        double getHighestOccupancyOfMonth(List<SubCalendarEvent> alleventsWithinMonth, TimeLine monthTimeline)
        {
            TimeLine[] AllWeeks = new TimeLine[4];
            double[] occupancy= new double[AllWeeks.Length];
            DateTimeOffset startOfWeek=monthTimeline.Start;
            for (int i = 0; i < AllWeeks.Length; i++)
            {
                AllWeeks[i] = new TimeLine(startOfWeek, startOfWeek.AddDays(7));
                startOfWeek = AllWeeks[i].End;
            }
            Parallel.For(0,AllWeeks.Length,j=>{
                long totalDuration = getInterferringSubEvents( AllWeeks[j],alleventsWithinMonth).Sum(obj => obj.getActiveDuration.Ticks);
                occupancy[j] = (double)totalDuration / OnewWeekTimeSpan.Ticks;
            });

            return occupancy.Max();

        }

        void ScoreEvents(IEnumerable<SubCalendarEvent> EventsToBeScored, IEnumerable<SubCalendarEvent> referenceScoringEvents)
        {
            EventsToBeScored.AsParallel().ForAll(obj => obj.setScore(EvaluateScore(referenceScoringEvents, obj, Now.calculationNow)));
        }

        double EvaluateScore(IEnumerable<SubCalendarEvent> CurrentEvents, SubCalendarEvent refEvents, DateTimeOffset refTIme)
        {
            double score = 500;
            score += CurrentEvents.AsParallel().Sum(obj1 => SubCalendarEvent.CalculateDistance(obj1, refEvents, 100));
            TimeSpan spanBeforeDeadline = refEvents.getCalculationRange.End - refTIme;
            if (spanBeforeDeadline < TwentyFourHourTimeSpan)
            {
                spanBeforeDeadline = TwentyFourHourTimeSpan;
            }

            long NumberOfhours= spanBeforeDeadline.Ticks / TwentyFourHourTimeSpan.Ticks;
            score -= (double)100 / NumberOfhours;
            return score;

        }

        List<SubCalendarEvent> stitchUnRestrictedSubCalendarEvent(TimeLine freeTimeLine, List<SubCalendarEvent> PinnedToStartRestrictedEvents, List<SubCalendarEvent> AllPossibleEvents, Tuple<SubCalendarEvent, SubCalendarEvent> BoundaryElements=null,bool EnableBetterOptimization=true)
        {
            


            List<SubCalendarEvent> retValue = new List<SubCalendarEvent>();
            Dictionary<DateTimeOffset, List<SubCalendarEvent>> Deadline_To_MatchingEvents = new Dictionary<DateTimeOffset, List<SubCalendarEvent>>();//Has the deadlines and events with matching deadlines
            Dictionary<TimeSpan, List<SubCalendarEvent>> Timespan_To_MatchingEvents = new Dictionary<TimeSpan, List<SubCalendarEvent>>();
            Dictionary<DateTimeOffset, List<SubCalendarEvent>> DeadLineWithinFreeTime = new Dictionary<DateTimeOffset, List<SubCalendarEvent>>();//populates dictionary of subevents that have deadline ending within
            Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> allPossbileEvents_Nonrestricted = new Dictionary<TimeSpan,Dictionary<string,SubCalendarEvent>>();
            
            SubCalendarEvent initBoundaryElementA=null;
            SubCalendarEvent initBoundaryElementB=null;
            
            
            if(BoundaryElements==null)
            {
                BoundaryElements=new Tuple<SubCalendarEvent,SubCalendarEvent>(initBoundaryElementA,initBoundaryElementB);
            }
            else
            {
                initBoundaryElementA=BoundaryElements.Item1;
                initBoundaryElementB=BoundaryElements.Item2;
            }


            SubCalendarEvent rightBoundary=initBoundaryElementB;
            SubCalendarEvent leftBoundary=initBoundaryElementA;
            
            Dictionary<string, SubCalendarEvent> ID_To_SubEvent_Nonrestricted = new Dictionary<string, SubCalendarEvent>();
            Dictionary<string, SubCalendarEvent> ID_To_SubEvent_Restricted = new Dictionary<string, SubCalendarEvent>();
            HashSet<SubCalendarEvent> DistinctSubEvents = new HashSet<SubCalendarEvent>();
            HashSet<SubCalendarEvent> DistinctSubEvents_Restricted = new HashSet<SubCalendarEvent>();
            HashSet<SubCalendarEvent> DistincEvents_NoRestricted=new HashSet<SubCalendarEvent>();
            List<SubCalendarEvent> AllSubCalEvents_NorestrictedValues = AllPossibleEvents.ToList();
            AllSubCalEvents_NorestrictedValues.RemoveAll(obj => PinnedToStartRestrictedEvents.Contains(obj));

            if (AllSubCalEvents_NorestrictedValues.Where(obj => obj.getRigid).Count()>0)
            {
                throw new Exception("You have a rigid subevent in call to stitchUnRestrictedSubCalendarEvent");
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
            

            

            foreach (SubCalendarEvent eachSubCalendarEvent in DistincEvents_NoRestricted )//populates dictionary of deadlines and matching subEvents
            {
                if (Deadline_To_MatchingEvents.ContainsKey(eachSubCalendarEvent.getCalculationRange.End))
                {
                    Deadline_To_MatchingEvents[eachSubCalendarEvent.getCalculationRange.End].Add(eachSubCalendarEvent);
                }
                else 
                {
                    Deadline_To_MatchingEvents.Add(eachSubCalendarEvent.getCalculationRange.End, new List<SubCalendarEvent>() { eachSubCalendarEvent });
                }
                if (Timespan_To_MatchingEvents.ContainsKey(eachSubCalendarEvent.RangeSpan))//populates dictionary of timeSpan and matching subEvents
                {
                    Timespan_To_MatchingEvents[eachSubCalendarEvent.RangeSpan].Add(eachSubCalendarEvent);
                    allPossbileEvents_Nonrestricted[eachSubCalendarEvent.RangeSpan].Add(eachSubCalendarEvent.getId,eachSubCalendarEvent);
                }
                else
                {
                    Timespan_To_MatchingEvents.Add(eachSubCalendarEvent.RangeSpan, new List<SubCalendarEvent>() { eachSubCalendarEvent });                
                    allPossbileEvents_Nonrestricted.Add(eachSubCalendarEvent.RangeSpan, new Dictionary<string,SubCalendarEvent>(){{eachSubCalendarEvent.getId,eachSubCalendarEvent}});
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
            List<DateTimeOffset> DeadLinesWithinFreeTimeLine = DeadLineWithinFreeTime.Keys.ToList();
            List<DateTimeOffset> DeadLinesWithinFreeTimeLine_cpy;
            DeadLinesWithinFreeTimeLine=DeadLinesWithinFreeTimeLine.OrderByDescending(obj => obj).ToList();
            List<SubCalendarEvent> ID_To_SubEvent_Restricted_List = ID_To_SubEvent_Restricted.OrderByDescending(obj=>obj.Value.End). Select(obj=>obj.Value). ToList();

            int decrement = 0;
            DateTimeOffset restrictedStopper;
            DateTimeOffset EarliestStartTime = freeTimeLine.Start;
            DateTimeOffset EndTime = freeTimeLine.End;
            TimeLine pertinentFreeSpot;
            List<SubCalendarEvent> LowestOrderedElements = new List<SubCalendarEvent>();

            
            //++CountCall;
            for (int i = 0; ((i < ID_To_SubEvent_Restricted_List.Count)&&(i>=0)); )
            {
                SubCalendarEvent restrictedStoppingEvent = ID_To_SubEvent_Restricted_List[i];
                LowestOrderedElements = new List<SubCalendarEvent>();
                restrictedStopper = ID_To_SubEvent_Restricted_List[i].End;
                EarliestStartTime = restrictedStopper;


                if (i > 0)
                {
                    ;
                }

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
                
                leftBoundary=restricted_EventsOrderedBasedOnDeadline[i];
                BoundaryElements=new Tuple<SubCalendarEvent,SubCalendarEvent>(leftBoundary,rightBoundary);
                LowestOrderedElements = OptimizeArrangeOfSubCalEvent_NoMtuple(pertinentFreeSpot, BoundaryElements, allPossbileEvents_Nonrestricted);
                if (!Utility.PinSubEventsToEnd(LowestOrderedElements, pertinentFreeSpot))
                {
                    throw new Exception("error in Pin to end 0 in Stitchun restricted \"reversed\"");
                }
                
                
                if(LowestOrderedElements.Count>0)
                {
                    EndTime= LowestOrderedElements[0].Start;
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

                reassignableEvents = reassignableEvents.Except(COnstrainedElementsForTimeLine).ToList();
                List<SubCalendarEvent> reassignedElements;
                if (reassignableEvents.Count > 0)
                {
                    reassignedElements = stitchUnRestrictedSubCalendarEvent(timelineForFurtherPinning, COnstrainedElementsForTimeLine, reassignableEvents);
                } else
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
                    if (allPossbileEvents_Nonrestricted.ContainsKey(eachSubCalendarEvent.RangeSpan))
                    {
                        allPossbileEvents_Nonrestricted[eachSubCalendarEvent.RangeSpan].Remove(eachSubCalendarEvent.getId);
                        if (allPossbileEvents_Nonrestricted[eachSubCalendarEvent.RangeSpan].Count == 0)
                        {
                            allPossbileEvents_Nonrestricted.Remove(eachSubCalendarEvent.RangeSpan);
                        }
                    }
                    DistincEvents_NoRestricted.Remove(eachSubCalendarEvent);

                }
                retValue.InsertRange(0, LowestOrderedElements);
                if(!Utility.PinSubEventsToEnd(retValue, freeTimeLine))
                {
                    throw new Exception("error in Pin to end 2 in Stitchun restricted \"reversed\"");
                }
                SubCalendarEvent startingElement = retValue[0];
                EndTime = startingElement.Start;

                BoundaryElements = new Tuple<SubCalendarEvent, SubCalendarEvent>(leftBoundary, startingElement);
            }
            EarliestStartTime = freeTimeLine.Start;
            pertinentFreeSpot = new TimeLine(freeTimeLine.Start, EndTime);
            List<SubCalendarEvent> DistincEvents_NoRestricted_List=DistincEvents_NoRestricted.ToList();
            ConcurrentBag<SubCalendarEvent> AllPossibleInterferringEvents = new ConcurrentBag<SubCalendarEvent>();
            
            bool extraElementsCanExist = false;
            Parallel.For(0, DistincEvents_NoRestricted.Count, (i, loopState) =>//checks if any element can fit in the remaining timeLine
                {
                    if (DistincEvents_NoRestricted_List[i].canExistWithinTimeLine(pertinentFreeSpot))
                    {
                        AllPossibleInterferringEvents.Add(DistincEvents_NoRestricted_List[i]);
                    }
                    
                    
                });

            if (AllPossibleInterferringEvents.Count>0)
            {
                SubCalendarEvent LatestDeadline= AllPossibleInterferringEvents.ToList().OrderByDescending(obj=>obj.getCalculationRange.End).First();
                BoundaryElements = new Tuple<SubCalendarEvent, SubCalendarEvent>(leftBoundary, BoundaryElements.Item2);
                TimeLine pertinentFreeSpotDebugCopy = pertinentFreeSpot.CreateCopy();
                pertinentFreeSpot = new TimeLine(pertinentFreeSpot.Start, LatestDeadline.getCalculationRange.End <= EndTime ? LatestDeadline.getCalculationRange.End : EndTime);

                LowestOrderedElements = OptimizeArrangeOfSubCalEvent_NoMtuple(pertinentFreeSpot, BoundaryElements, allPossbileEvents_Nonrestricted);
                if(!Utility.PinSubEventsToEnd(LowestOrderedElements, pertinentFreeSpot))
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



        Tuple<DateTimeOffset,bool, SubCalendarEvent> ObtainBetterReferencetimeCloseToStar(TimeLine beforeStopper, Dictionary <string,SubCalendarEvent>AllPossibleEventNonRestricting,Dictionary <string,SubCalendarEvent>restrictedValues, List<SubCalendarEvent> CUrrentlyDesignatedEleemnts)
        {
            Tuple<DateTimeOffset, bool, SubCalendarEvent> retavlue = null;
            List<SubCalendarEvent> AllPossibleEvents = AllPossibleEventNonRestricting.Values.Concat(restrictedValues.Values).ToList();
            AllPossibleEvents.RemoveAll(obj => CUrrentlyDesignatedEleemnts.Contains(obj));
            List<SubCalendarEvent> AllValidEvents = AllPossibleEvents.Where(obj => obj.canExistTowardsEndWithoutSpace(beforeStopper)).ToList();
            AllValidEvents=AllValidEvents.OrderByDescending(obj => obj.getActiveDuration).ToList();
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

        Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> InterferringTimeSpanWithStringID_Cpy = new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();


        List<List<SubCalendarEvent>> TossScheduleTOTHeEnd(List<List<SubCalendarEvent>>AllSubCalEvents, List<TimeLine>AllFreeSpots)
        {
            List<SubCalendarEvent> AllEventsSerialized= AllSubCalEvents.SelectMany(obj=>obj).ToList();
            int initCount = AllEventsSerialized.Count;
            List<List<SubCalendarEvent>>retValue= new List<List<SubCalendarEvent>>();

            for(int i=AllSubCalEvents.Count-1;i>=0;i--)
            {
                List<SubCalendarEvent> currentConstituents = AllSubCalEvents[i];//populates with events within timeline

                currentConstituents = currentConstituents.Where(obj => AllEventsSerialized.Contains(obj)).ToList();
                TimeLine CopyOfFreeSpot = AllFreeSpots[i].CreateCopy();
                CopyOfFreeSpot.Empty();
                Utility.PinSubEventsToStart(currentConstituents, CopyOfFreeSpot);
                List<SubCalendarEvent>reassignedElements= TossAllEventsToEnd(AllEventsSerialized, currentConstituents, CopyOfFreeSpot);//reassignes elements to end of list
                CopyOfFreeSpot.Empty();
                CopyOfFreeSpot.AddBusySlots(reassignedElements.Select(obj => obj.ActiveSlot));
                TimeLine[] freespaces = CopyOfFreeSpot.getAllFreeSlots();

                reassignedElements.ForEach(delegate(SubCalendarEvent obj) { currentConstituents.Remove(obj); });//
                if (!Utility.PinSubEventsToEnd(reassignedElements, CopyOfFreeSpot))
                {
                    throw new Exception("oops error with toss Endwards");
                }
                retValue.Insert(0, reassignedElements);
            }

            IList<SubCalendarEvent> JustAnother = retValue.SelectMany(obj => obj).ToList();

            return retValue;
        }


        List<Tuple <List<SubCalendarEvent>,int>> resolveFirsTwentryFourHours(List<Tuple<mTuple<TimeLine,List<SubCalendarEvent>>,int>> InterferringTImeLine, List<mTuple<SubCalendarEvent, TimeLine>> FirstTwentyFourHours)
        {
            Dictionary<SubCalendarEvent, mTuple<int, TimeLine>> initialFirstTwentyFourHoursValues = FirstTwentyFourHours.ToDictionary(obj => obj.Item1, obj => new mTuple<int, TimeLine>(obj.Item1.IntData, obj.Item1.RangeTimeLine));
            List<Tuple<List<SubCalendarEvent>, int>> retValue = new List<Tuple<List<SubCalendarEvent>, int>>();
            foreach (Tuple<mTuple<TimeLine, List<SubCalendarEvent>>,int> eachTuple in InterferringTImeLine)
            {
                int CurrentIndex= eachTuple.Item2;

                mTuple<TimeLine, List<SubCalendarEvent>> eachmTuple=eachTuple.Item1;
                eachmTuple.Item2 = eachmTuple.Item2.OrderBy(obj => obj.Start).ToList();
                Utility.PinSubEventsToStart(eachmTuple.Item2,eachmTuple.Item1);
                

                IEnumerable<SubCalendarEvent> AllSubCalEvents = eachmTuple.Item2.Concat(FirstTwentyFourHours.Select(obj=>obj.Item1)).ToList();
                HashSet<SubCalendarEvent> NonRepeatingSubCalEvents = new HashSet<SubCalendarEvent>();

                foreach(SubCalendarEvent eachSubCalendarEvent in  AllSubCalEvents )
                {
                    NonRepeatingSubCalEvents.Add(eachSubCalendarEvent);
                }

                Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries= new Dictionary<TimeSpan,Dictionary<string,mTuple<bool,SubCalendarEvent>>>();

                foreach (SubCalendarEvent eachSubCalendarEvent in NonRepeatingSubCalEvents)//grenerates possible entries for stitchUnRestrictedSubCalendarEvent
                {
                    TimeSpan ActiveTimeSpan = eachSubCalendarEvent.getActiveDuration;
                    string subcalStringID = eachSubCalendarEvent.getId;
                    
                    if (PossibleEntries.ContainsKey(ActiveTimeSpan))
                    {
                        PossibleEntries[ActiveTimeSpan].Add(subcalStringID, new mTuple<bool, SubCalendarEvent>(true, eachSubCalendarEvent));
                    }
                    else
                    {
                        PossibleEntries.Add(ActiveTimeSpan, new Dictionary<string, mTuple<bool, SubCalendarEvent>>());
                        PossibleEntries[ActiveTimeSpan].Add(subcalStringID, new mTuple<bool, SubCalendarEvent>(true, eachSubCalendarEvent));
                    }
                }


                List<mTuple<bool, SubCalendarEvent>> reassignedElements = stitchUnRestrictedSubCalendarEvent(eachmTuple.Item1, eachmTuple.Item2.Select(obj => new mTuple<bool, SubCalendarEvent>(false, obj)).ToList(), PossibleEntries, new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>(), 0);
                SubCalendarEvent.updateMiscData(reassignedElements.Select(obj => obj.Item2).ToList(), -1);//sets all subevents to be removed from FirstTwentyFourHours
                List<SubCalendarEvent>removedSubEvents= FirstTwentyFourHours.Where(obj => obj.Item1.IntData == -1).Select(obj=>obj.Item1).ToList();//gets elements to be reomved from FirstTwentyFourHours
                FirstTwentyFourHours.RemoveAll(obj => obj.Item1.IntData == -1);//removes all Subevents
                SubCalendarEvent.updateMiscData(removedSubEvents,initialFirstTwentyFourHoursValues.Where(obj => removedSubEvents.Contains(obj.Key)).Select(obj => obj.Value.Item1).ToList());
                retValue.Add(new Tuple<List<SubCalendarEvent>,int>(reassignedElements.Select(obj => obj.Item2).ToList(),CurrentIndex));
            }
            return retValue;
        }


        List<SubCalendarEvent> TossAllEventsToEnd(List<SubCalendarEvent> AllSubEvents, List<SubCalendarEvent> CurrentlyAssignedElements, TimeLine CurrentTimeLine)
        {
            Utility.PinSubEventsToStart(CurrentlyAssignedElements, CurrentTimeLine);
            TimeLine initTimeLine=CurrentTimeLine.CreateCopy();
            TimeLine CurrentTimeLine_Cpy = CurrentTimeLine.CreateCopy();
            CurrentTimeLine_Cpy.AddBusySlots(CurrentlyAssignedElements.Select(obj => obj.ActiveSlot));
            IList<TimeLine> AllFreeSpots = CurrentTimeLine_Cpy.getAllFreeSlots();
            List<SubCalendarEvent> CompleteReassignedElements = new List<SubCalendarEvent>();
            for (int i = AllFreeSpots.Count() - 1; AllFreeSpots.Count() > 0; )
            {
                TimeLine eachTimeLine = AllFreeSpots[i];
                IEnumerable<SubCalendarEvent> reassignedElements=TossEndWards(AllSubEvents, eachTimeLine);
                reassignedElements=reassignedElements.OrderBy(obj => obj.End).ToList();
                if(!Utility.PinSubEventsToEnd(reassignedElements, eachTimeLine))
                {
                    throw new Exception("oops error with TossAllEventsToEnd AAA");
                }
                CurrentlyAssignedElements.RemoveAll(obj => reassignedElements.Contains(obj));

                AllSubEvents.RemoveAll(obj => reassignedElements.Contains(obj));
                CompleteReassignedElements = reassignedElements.Concat(CompleteReassignedElements).ToList();//
                //CompleteReassignedElements.AddRange(reassignedElements.ToList());
                SubCalendarEvent lastElement;
                if (reassignedElements.Count() > 0)
                {
                    lastElement = reassignedElements.First();
                    CurrentTimeLine = new TimeLine(CurrentTimeLine.Start, lastElement.Start);
                }
                else
                {
                    CurrentlyAssignedElements.OrderBy(obj => obj.End);
                    if (CurrentlyAssignedElements.Count() > 0)//hack alert you need to coscious of coliision scenario
                    {
                        lastElement = CurrentlyAssignedElements.Last();
                        lastElement.PinToEnd(CurrentTimeLine);
                        CurrentlyAssignedElements.Remove(lastElement);
                        AllSubEvents.Remove(lastElement);
                        CompleteReassignedElements.Insert(0, lastElement);
                        CurrentTimeLine = new TimeLine(CurrentTimeLine.Start, lastElement.Start);
                    }
                    else
                    {
                        break;
                    }
                }

                if(!Utility.PinSubEventsToStart(CurrentlyAssignedElements, CurrentTimeLine))
                {
                    throw new Exception("oops error with TossAllEventsToEnd BBB");
                }
                CurrentTimeLine.AddBusySlots(CurrentlyAssignedElements.Select(obj => obj.ActiveSlot));
                AllFreeSpots = CurrentTimeLine.getAllFreeSlots().ToList();
                i = AllFreeSpots.Count() - 1;
            }
            if (!Utility.PinSubEventsToEnd(CompleteReassignedElements, initTimeLine))
            { 
                throw new Exception("oops error with TossAllEventsToEnd AAA"); 
            }
            CompleteReassignedElements = CompleteReassignedElements.OrderBy(obj => obj.Start).ToList();
            return CompleteReassignedElements;
        }


        TimeSpan MinutesPerMile = new TimeSpan(0, 15,0);

        public List<SubCalendarEvent> StitchUnrestricted(TimeLine ReferenceTImeLine, List<SubCalendarEvent> orderedCOnstrictingevents, List<SubCalendarEvent> PossibleSubcalevents, bool EnableBetterOptimization=true)
        {
            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();
            PossibleSubcalevents = PossibleSubcalevents.Distinct().ToList();

            foreach (SubCalendarEvent eachmTuple in PossibleSubcalevents)
            {
                TimeSpan ActiveTimeSpan = eachmTuple.getActiveDuration;
                string subcalStringID = eachmTuple.getId;

                if (PossibleEntries.ContainsKey(ActiveTimeSpan))
                {
                    PossibleEntries[ActiveTimeSpan].Add(subcalStringID, new mTuple<bool, SubCalendarEvent>(true, eachmTuple));
                }
                else
                {
                    PossibleEntries.Add(ActiveTimeSpan, new Dictionary<string, mTuple<bool, SubCalendarEvent>>());
                    PossibleEntries[ActiveTimeSpan].Add(subcalStringID, new mTuple<bool, SubCalendarEvent>(true, eachmTuple));
                }
            }
            List<mTuple<bool, SubCalendarEvent>> retValue = stitchUnRestrictedSubCalendarEvent(ReferenceTImeLine, orderedCOnstrictingevents.Select(obj => new mTuple<bool, SubCalendarEvent>(false, obj)).ToList(), PossibleEntries, new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>(), 1, EnableBetterOptimization);
            return retValue.Select(obj => obj.Item2).ToList();
        }
        

        List<SubCalendarEvent> SpaceOutEVentsWithin24Hours(IList<SubCalendarEvent> AllSubCalEvent, TimeLine referenceDay, mTuple<SubCalendarEvent,SubCalendarEvent>EdgeElement=null)
        {
            Utility.PinSubEventsToStart(AllSubCalEvent, referenceDay);
            SubCalendarEvent referenceSubcalendarEvent = null;
            if (EdgeElement != null)
            {
                if (EdgeElement.Item2 != null)
                {
                    referenceSubcalendarEvent = EdgeElement.Item2;
                }
            }
            return AllSubCalEvent.ToList();
        }

        public List<SubCalendarEvent> TossEndWards(IEnumerable<SubCalendarEvent> AllSubEvents, TimeLine FreeSpot)
        {
            List<SubCalendarEvent>retValue=new List<SubCalendarEvent>();
            List<SubCalendarEvent> AllSubEvents_ini = AllSubEvents.ToList();
            List<SubCalendarEvent> AllSubEvents_noEdit = AllSubEvents.ToList();

            int count = AllSubEvents_ini.Count;

            AllSubEvents=AllSubEvents.Where(obj => obj.canExistWithinTimeLine(FreeSpot));
            if (AllSubEvents.Count() > 0)
            {
                AllSubEvents=AllSubEvents.OrderBy(obj => obj.getCalculationRange.End).Reverse();
                SubCalendarEvent FirstElement= AllSubEvents.First();
                FirstElement.PinToEnd(FreeSpot);
                retValue.Add(FirstElement);
                AllSubEvents_ini.Remove(FirstElement);
                AllSubEvents = AllSubEvents_ini;
                FreeSpot = new TimeLine(FreeSpot.Start,FirstElement.Start );
                Utility.PinSubEventsToStart(AllSubEvents, FreeSpot);
                retValue.AddRange(TossEndWards(AllSubEvents, FreeSpot));
            }

            return retValue;
        }

        Tuple<DateTimeOffset, List<SubCalendarEvent>> ObtainBetterEarlierReferenceTime(List<SubCalendarEvent> CurrentlyOptimizedList, Dictionary<string, Dictionary<string, SubCalendarEvent>> CalendarIDAndNonPartialSubCalEvents, TimeLine TimeLineBeforStopper, DateTimeOffset CurrentEarliestReferenceTIme, TimeLine PinToStartTimeLine, SubCalendarEvent LastSubCalEvent, bool Aggressive = true)
        {
            Tuple<DateTimeOffset, List<SubCalendarEvent>> retValue = null;
            CurrentlyOptimizedList = CurrentlyOptimizedList.ToList();
            HashSet<string> AllValidNodes = new HashSet<string>(AllEventDictionary.Keys);
            //AllValidNodes = CalendarEvent.DistanceToAllNodes("");
            //if (LastSubCalEvent != null)
            //{
            //    AllValidNodes = CalendarEvent.DistanceToAllNodes(LastSubCalEvent.SubEvent_ID.getCalendarEventComponent());
            //}


            //if (CurrentlyOptimizedList.Count > 0)
            {

                //LastSubCalEvent = CurrentlyOptimizedList[CurrentlyOptimizedList.Count - 1];




                DateTimeOffset EarliestReferenceTIme = new DateTimeOffset();
                SubCalendarEvent AppendableEVent;
                bool BreakOutsideForLoop = false;
                if (Aggressive)
                {
                    IEnumerable<string> plausibleStrings = new List<string>();
                    plausibleStrings = AllValidNodes.Where(obj => CalendarIDAndNonPartialSubCalEvents.ContainsKey(obj));
                    IEnumerable<Dictionary<string, SubCalendarEvent>> AllValidDicts;
                    if (plausibleStrings.Count() > 0)
                    {
                        AllValidDicts = plausibleStrings.Select(obj => CalendarIDAndNonPartialSubCalEvents[obj]);
                        if (AllValidDicts.Count() > 0)
                        {
                            SubCalendarEvent earliestSubCalEvent = null;
                            AllValidDicts = AllValidDicts.OrderBy(obj => obj.Values.ToArray()[0].getCalculationRange.End);


                            foreach (Dictionary<string, SubCalendarEvent> eachDict in AllValidDicts)
                            {
                                IEnumerable<SubCalendarEvent> AllSubCalevents = eachDict.Values.OrderBy(obj => obj.getCalculationRange.Start);
                                //AllSubCalevents = AllSubCalevents.Where(obj => (obj.ActiveDuration <= (LimitingTimeSpan)) && (!CurrentlyOptimizedList.Contains(obj)));
                                AllSubCalevents = AllSubCalevents.Where(obj => (obj.canExistWithinTimeLine(TimeLineBeforStopper)) && (!CurrentlyOptimizedList.Contains(obj)));
                                if (AllSubCalevents.Count() > 0)
                                {
                                    if (earliestSubCalEvent == null)
                                    {
                                        earliestSubCalEvent = AllSubCalevents.ToList()[0];
                                    }
                                    else
                                    {
                                        SubCalendarEvent retrievedEarliestSubCal = AllSubCalevents.ToList()[0];
                                        if (retrievedEarliestSubCal.getCalculationRange.End < earliestSubCalEvent.getCalculationRange.End)
                                        {
                                            earliestSubCalEvent = retrievedEarliestSubCal;
                                        }
                                        else
                                        {
                                            if ((retrievedEarliestSubCal.getCalendarEventRange.Start == earliestSubCalEvent.getCalendarEventRange.Start) && (retrievedEarliestSubCal.getActiveDuration > earliestSubCalEvent.getActiveDuration))
                                            {
                                                earliestSubCalEvent = retrievedEarliestSubCal;
                                            }
                                        }
                                    }

                                }
                            }
                            if (earliestSubCalEvent != null)
                            {
                                CurrentlyOptimizedList.Add(earliestSubCalEvent);
                                bool error = Utility.PinSubEventsToStart(CurrentlyOptimizedList, PinToStartTimeLine);
                                if (error)
                                {
                                    EarliestReferenceTIme = earliestSubCalEvent.End;

                                    retValue = new Tuple<DateTimeOffset, List<SubCalendarEvent>>(EarliestReferenceTIme, CurrentlyOptimizedList);
                                    BreakOutsideForLoop = true;
                                }
                                else
                                {
                                    CurrentlyOptimizedList.Remove(earliestSubCalEvent);
                                }

                            }
                        }
                    }

                }
                else
                {
                    foreach (string eachstring in AllValidNodes)
                    {

                        if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(eachstring))
                        {
                            List<KeyValuePair<string, SubCalendarEvent>> AllSubCalEvent = CalendarIDAndNonPartialSubCalEvents[eachstring].ToList();
                            for (int i = 0; i < AllSubCalEvent.Count; i++)
                            {
                                AppendableEVent = CalendarIDAndNonPartialSubCalEvents[eachstring].ToList()[i].Value;//Assumes Theres Always an element
                                if ((AppendableEVent.canExistWithinTimeLine(TimeLineBeforStopper)) && (!CurrentlyOptimizedList.Contains(AppendableEVent)))
                                {
                                    CurrentlyOptimizedList.Add(AppendableEVent);
                                    CalendarIDAndNonPartialSubCalEvents[eachstring].Remove(AppendableEVent.getId);
                                    if (CalendarIDAndNonPartialSubCalEvents[eachstring].Count < 1)//checks if List is empty. Deletes keyValuepair if list is empty
                                    {
                                        CalendarIDAndNonPartialSubCalEvents.Remove(eachstring);
                                    }
                                    //FreeSpotUpdated = new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End);
                                    Utility.PinSubEventsToStart(CurrentlyOptimizedList, PinToStartTimeLine);
                                    EarliestReferenceTIme = AppendableEVent.End;
                                    //retValue = new Tuple<DateTimeOffset, List<SubCalendarEvent>>(new DateTimeOffset(), new List<SubCalendarEvent>());
                                    retValue = new Tuple<DateTimeOffset, List<SubCalendarEvent>>(EarliestReferenceTIme, CurrentlyOptimizedList);
                                    BreakOutsideForLoop = true;
                                    break;
                                }
                            }

                        }
                        if (BreakOutsideForLoop)
                        {
                            break;
                        }
                    }
                }
            }

            return retValue;
        }


        Tuple<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> UpdateCompatibleListOfTimeLine(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CurrentCompatibleList, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> MovedOverSet, TimeLine ReferenceTimeLine, TimeSpanWithStringID LeftOuts, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> TotalOfMovedVariables)
        {
            //Hack alert: You need to create a situation that enforces a restricted event as being assigned first

            TimeSpan CurrentTotalOfSnugVariables = new TimeSpan(0);
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> retValue_MovedVariables = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> retValue_CurrentCompatibleList = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();

            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CurrentCompatibleList_Cpy = SnugArray.CreateCopyOFSnuPossibilities(CurrentCompatibleList);
            //TimeSpan SumOfLeftOuts= new TimeSpan(0);
            TimeSpan RemainderTimeSpan = new TimeSpan(0);
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> MovedOverListUpdate = new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>();

            if (LeftOuts.timeSpan > ReferenceTimeLine.TimelineSpan)
            {
                return null;
            }





            CurrentTotalOfSnugVariables = SnugArray.TotalTimeSpanOfSnugPossibility(CurrentCompatibleList_Cpy);

            RemainderTimeSpan = ReferenceTimeLine.TimelineSpan - CurrentTotalOfSnugVariables;
            TimeSpan RemainderOfLeftOverChunk = LeftOuts.timeSpan - RemainderTimeSpan;
            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> MovedOverSet_Cpy = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();

            foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in MovedOverSet)
            {
                MovedOverSet_Cpy.Add(compareMovedOverSetWithTotalPossibleEntries(eachDictionary, TotalOfMovedVariables));

            }


            while (RemainderOfLeftOverChunk.Ticks < 0)
            {
                ReferenceTimeLine = new TimeLine(ReferenceTimeLine.Start, ReferenceTimeLine.End - LeftOuts.timeSpan);
                RemainderOfLeftOverChunk = LeftOuts.timeSpan - RemainderTimeSpan;
            }


            SnugArray FitsInChunkOfRemainder_SnugArray = new SnugArray(CurrentCompatibleList_Cpy.Values.ToList(), RemainderOfLeftOverChunk);
            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> SnugPossibilities = FitsInChunkOfRemainder_SnugArray.MySnugPossibleEntries;
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> Viable_SnugPossibilities = getPlausibleEntriesFromMovedOverSet(MovedOverSet_Cpy, SnugPossibilities);
            Dictionary<Dictionary<string, mTuple<int, TimeSpanWithStringID>>, List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> LeftAfterRemovalSnugPossibilities = new System.Collections.Generic.Dictionary<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>, System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> BestSnugPossibility = Viable_SnugPossibilities;
            retValue_MovedVariables = SnugArray.AddToSnugPossibilityList(retValue_MovedVariables, BestSnugPossibility);
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CurrentCompatibleList_Cpy_updated = SnugArray.RemoveSnugPossibilityFromAnother(CurrentCompatibleList_Cpy, BestSnugPossibility);


            retValue_CurrentCompatibleList = SnugArray.RemoveSnugPossibilityFromAnother(CurrentCompatibleList, retValue_MovedVariables);
            //retValue_CurrentCompatibleList = SnugArray.AddToSnugPossibilityList(CurrentCompatibleList, MovedOverListUpdate);


            Tuple<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> retValue = new Tuple<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>(retValue_CurrentCompatibleList, retValue_MovedVariables);
            //item1 is Updated CurrentCompatible List
            //item2 is Best Snug change to timeLine

            return retValue;
        }

        Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> compareMovedOverSetWithTotalPossibleEntries(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> MovedOverSet, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> TotalSet)
        {
            MovedOverSet = SnugArray.CreateCopyOFSnuPossibilities(MovedOverSet);
            List<KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>>> ListForDict = MovedOverSet.ToList();

            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in ListForDict)
            {
                if (TotalSet.ContainsKey(eachKeyValuePair.Key))
                {
                    if (TotalSet[eachKeyValuePair.Key].Item1 < eachKeyValuePair.Value.Item1)
                    {
                        MovedOverSet[eachKeyValuePair.Key].Item1 = TotalSet[eachKeyValuePair.Key].Item1;
                    }
                }
                else
                {
                    MovedOverSet.Remove(eachKeyValuePair.Key);
                }
            }

            return MovedOverSet;
        }


        Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> getBestSnugPossiblity(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CurrentCompatibleList, Dictionary<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> LeftAfterRemovalSnugPossibilities)
        {
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CurrentCompatibleList_cpy = SnugArray.CreateCopyOFSnuPossibilities(CurrentCompatibleList);
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> retValue = new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
            foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in LeftAfterRemovalSnugPossibilities.Keys)//tries each snugPossibility as a potential i
            {
                Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> myCurrentCompatibleList = SnugArray.CreateCopyOFSnuPossibilities(CurrentCompatibleList_cpy);

                List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> ListOfLeftOfMovedOverSnugArray = LeftAfterRemovalSnugPossibilities[eachDictionary];
                foreach (TimeSpan eachTimeSpan in eachDictionary.Keys)
                {
                    myCurrentCompatibleList[eachTimeSpan].Item1 -= eachDictionary[eachTimeSpan].Item1;
                    if (myCurrentCompatibleList[eachTimeSpan].Item1 < 1)//removes mTuple where the TImeSpan spring is zero
                    {
                        myCurrentCompatibleList.Remove(eachTimeSpan);
                    }
                }

                List<KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>>> OtherValuesFromMyCurrenCompatibleList = myCurrentCompatibleList.ToList();
                OtherValuesFromMyCurrenCompatibleList = OtherValuesFromMyCurrenCompatibleList.OrderBy(obj => obj.Value.Item2.timeSpan).ToList();
                OtherValuesFromMyCurrenCompatibleList.Reverse();


                foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary0 in ListOfLeftOfMovedOverSnugArray)
                {
                    Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> Potential_retValue = SnugArray.CreateCopyOFSnuPossibilities(eachDictionary);
                    foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in OtherValuesFromMyCurrenCompatibleList)
                    {
                        if (eachDictionary0.ContainsKey(eachKeyValuePair.Key))
                        {
                            mTuple<int, TimeSpanWithStringID> UpdatedmTuple;
                            if (eachDictionary0[eachKeyValuePair.Key].Item1 < eachKeyValuePair.Value.Item1)
                            {
                                UpdatedmTuple = eachDictionary0[eachKeyValuePair.Key];
                            }
                            else
                            {
                                UpdatedmTuple = eachKeyValuePair.Value;
                            }
                            if (Potential_retValue.ContainsKey(UpdatedmTuple.Item2.timeSpan))
                            {
                                Potential_retValue[UpdatedmTuple.Item2.timeSpan].Item1 += UpdatedmTuple.Item1;
                            }

                        }
                    }

                    TimeSpan TotalTimeSpanSnug_Possible = SnugArray.TotalTimeSpanOfSnugPossibility(Potential_retValue);
                    TimeSpan TotalTimeSpanSnugRetValue = SnugArray.TotalTimeSpanOfSnugPossibility(retValue);
                    if (TotalTimeSpanSnugRetValue < TotalTimeSpanSnug_Possible)
                    {
                        retValue = Potential_retValue;
                    }
                    else
                    {
                        if ((TotalTimeSpanSnug_Possible == TotalTimeSpanSnugRetValue) && (retValue.Count < Potential_retValue.Count))
                        {
                            retValue = Potential_retValue;
                        }
                    }
                }
            }

            return retValue;
        }

        Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> getPlausibleEntriesFromMovedOverSet(List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> MovedOverSet, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> SnugPossibilities)
        {
            /*
             * This function goes through the MovedOverSet and compares it with the snug possibilities.
             * It tries to find any set in the Movedoverset that has all the keys in the snug possibilities variable.
             * If it finds one with all these keys, it selects dict in which the int Count of the matching mTuple has a greater than or equal the current option of the current snug possibility mtuple
             */
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> retValue = new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();



            foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in MovedOverSet)
            {
                foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary0 in SnugPossibilities)
                {
                    IEnumerable<TimeSpan> Intersection = eachDictionary0.Keys.Intersect(eachDictionary.Keys);
                    Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> Possible_RetValue = new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
                    foreach (TimeSpan eachTimeSpan in Intersection)
                    {
                        Possible_RetValue.Add(eachTimeSpan, eachDictionary[eachTimeSpan].Item1 < eachDictionary0[eachTimeSpan].Item1 ? new mTuple<int, TimeSpanWithStringID>(eachDictionary[eachTimeSpan]) : new mTuple<int, TimeSpanWithStringID>(eachDictionary0[eachTimeSpan]));
                    }
                    if (SnugArray.TotalTimeSpanOfSnugPossibility(retValue) < SnugArray.TotalTimeSpanOfSnugPossibility(Possible_RetValue))
                    {
                        retValue = Possible_RetValue;
                    }
                }
            }
            return retValue;
        }

        public List<mTuple<bool, SubCalendarEvent>> stitchUnRestrictedSubCalendarEvent(TimeLine FreeBoundary, List<mTuple<bool, SubCalendarEvent>> restrictedSnugFitAvailable, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithList, double Occupancy,bool EnableBetterOptimization=true)
        {
            //++CountCall;
            TimeLine[] AllFreeSpots = FreeBoundary.getAllFreeSlots();
            int TotalEventsForThisTImeLine = 0;

            if (FreeBoundary.End == new DateTimeOffset(2014, 04, 8, 17, 00, 0, new TimeSpan()))
            {
                ;
            }

            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in CompatibleWithList)
            {
                TotalEventsForThisTImeLine += eachKeyValuePair.Value.Item1;
            }

            CompatibleWithList.Clear();


            DateTimeOffset EarliestReferenceTIme = FreeBoundary.Start;
            List<mTuple<bool, SubCalendarEvent>> FrontPartials = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            List<mTuple<bool, SubCalendarEvent>> EndPartials = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            Dictionary<DateTimeOffset, List<mTuple<bool, SubCalendarEvent>>> FrontPartials_Dict = new System.Collections.Generic.Dictionary<DateTimeOffset, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();
            Dictionary<DateTimeOffset, List<mTuple<bool, SubCalendarEvent>>> EndPartials_Dict = new System.Collections.Generic.Dictionary<DateTimeOffset, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();
            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries_Cpy = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();
            Dictionary<string, Dictionary<string, SubCalendarEvent>> CalendarIDAndNonPartialSubCalEvents = new Dictionary<string, Dictionary<string, SubCalendarEvent>>();//List of non partials for current Reference StartTime To End of FreeBoundary. Its gets updated with Partials once the earliest reference time passes the partial event start time

            foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleEntries)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                Dictionary<string, mTuple<bool, SubCalendarEvent>> NewDictEntry = new Dictionary<string, mTuple<bool, SubCalendarEvent>>();

                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> KeyValuePair0 in eachKeyValuePair.Value)
                {
                    mTuple<bool, SubCalendarEvent> MyEvent = KeyValuePair0.Value;

                    bool isInrestrictedSnugFitAvailable = false;
                    if (CompatibleWithList.ContainsKey(eachKeyValuePair.Key))
                    {
                        ++CompatibleWithList[eachKeyValuePair.Key].Item1;
                    }
                    else
                    {
                        CompatibleWithList.Add(eachKeyValuePair.Key, new mTuple<int, TimeSpanWithStringID>(1, new TimeSpanWithStringID(KeyValuePair0.Value.Item2.getActiveDuration, KeyValuePair0.Value.Item2.getActiveDuration.Ticks.ToString())));
                    }

                    foreach (mTuple<bool, SubCalendarEvent> eachMtuple in restrictedSnugFitAvailable)//checks if event is in restricted list
                    {
                        if (eachMtuple.Item2.getId == MyEvent.Item2.getId)
                        {
                            isInrestrictedSnugFitAvailable = true;
                            break;
                        }

                    }


                    if (!isInrestrictedSnugFitAvailable)
                    {
                        NewDictEntry.Add(KeyValuePair0.Value.Item2.getId, KeyValuePair0.Value);
                        if (FreeBoundary.IsDateTimeWithin(KeyValuePair0.Value.Item2.getCalculationRange.Start))
                        {
                            FrontPartials.Add(KeyValuePair0.Value);
                        }
                        else
                        {
                            if (FreeBoundary.IsDateTimeWithin(KeyValuePair0.Value.Item2.getCalculationRange.End))
                            {
                                EndPartials.Add(KeyValuePair0.Value);
                            }
                            string CalLevel0ID = KeyValuePair0.Value.Item2.SubEvent_ID.getCalendarEventComponent();
                            if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(CalLevel0ID))
                            {
                                CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(KeyValuePair0.Value.Item2.getId, KeyValuePair0.Value.Item2);
                            }
                            else
                            {
                                //CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new List<SubCalendarEvent>() { KeyValuePair0.Value.Item2 });
                                CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new Dictionary<string, SubCalendarEvent>());
                                CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(KeyValuePair0.Value.Item2.getId, KeyValuePair0.Value.Item2);
                            }
                        }
                    }
                }
                if (NewDictEntry.Count > 0)
                { PossibleEntries_Cpy.Add(eachKeyValuePair.Key, NewDictEntry); }

            }

            FrontPartials = FrontPartials.OrderBy(obj => obj.Item2.getCalculationRange.Start).ToList();
            EndPartials = EndPartials.OrderBy(obj => obj.Item2.getCalculationRange.End).ToList();

            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in FrontPartials)//populates FrontPartials_Dict in ordered manner since FrontPartials is ordered
            {
                if (FrontPartials_Dict.ContainsKey(eachmTuple.Item2.getCalculationRange.Start))
                {
                    FrontPartials_Dict[eachmTuple.Item2.getCalculationRange.Start].Add(eachmTuple);
                }
                else
                {
                    FrontPartials_Dict.Add(eachmTuple.Item2.getCalculationRange.Start, new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>() { eachmTuple });
                }

            }

            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in EndPartials)//populates EndPartials_Dict in ordered manner since EndPartials is ordered
            {
                if (EndPartials_Dict.ContainsKey(eachmTuple.Item2.getCalculationRange.Start))
                {
                    EndPartials_Dict[eachmTuple.Item2.getCalculationRange.Start].Add(eachmTuple);
                }
                else
                {
                    EndPartials_Dict.Add(eachmTuple.Item2.getCalculationRange.Start, new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>() { eachmTuple });
                }
            }


            List<SubCalendarEvent> CompleteArranegement = new System.Collections.Generic.List<SubCalendarEvent>();
            int StartingReferneceIndex = 0;


            /*foreach (mTuple<bool, SubCalendarEvent> eachmTuple in restrictedSnugFitAvailable)//removes the restricted from CompatibleWithList
            {
                --CompatibleWithList[eachmTuple.Item2.ActiveDuration.Ticks.ToString()].Item1;
                //PossibleEntries_Cpy[eachmTuple.Item2.ActiveDuration.Ticks.ToString()].Remove(eachmTuple.Item2.ID);
            }*/

            List<DateTimeOffset> ListOfFrontPartialsStartTime = FrontPartials_Dict.Keys.ToList();

            int i = 0;
            int j = 0;
            int FrontPartialCounter = 0;

            if (restrictedSnugFitAvailable.Count < 1)
            {
                ;
            }

            Tuple<DateTimeOffset, List<SubCalendarEvent>> TimeLineUpdated = null;
            SubCalendarEvent BorderElementBeginning = null;
            SubCalendarEvent BorderElementEnd = null;
            SubCalendarEvent LastSubCalElementForEarlierReferenceTime = null;
            int a = restrictedSnugFitAvailable.Count;
            int previ = i;
            for (; i < restrictedSnugFitAvailable.Count; i++)
            {
                //bool isFreeSpotBeforeRigid = AllFreeSpots[i].End <= restrictedSnugFitAvailable[i].Item2.Start;
                TimeLineUpdated = null;



                if (restrictedSnugFitAvailable[i].Item2.getCalculationRange.Start <= EarliestReferenceTIme)//this is to ensure the tightest configuration. If the restricted element calendarevent start range already preceedes the current start time then it can be appended immediately. because every other element is less restricted
                {
                    CompleteArranegement.Add(restrictedSnugFitAvailable[i].Item2);
                    if (!Utility.PinSubEventsToStart(CompleteArranegement, FreeBoundary))
                    {
                        throw new Exception("theres a bug in stitchunrestricted when restricted overlapsstart range  ");
                    }
                    EarliestReferenceTIme = CompleteArranegement[CompleteArranegement.Count - 1].End;
                    List<SubCalendarEvent> TempList = new List<SubCalendarEvent>() { restrictedSnugFitAvailable[i].Item2 };
                    //ContinueTrestrictedSnugFitAvailableoForLoop = true;//forces the continuation of the for loop for (; i < restrictedSnugFitAvailable.Count; i++)
                    //PreserveRestrictedIndex = false;
                    continue;
                }


                if (a != restrictedSnugFitAvailable.Count)
                {
                    ;
                }
                if (i == 9)
                {
                    ;
                }
                previ = i;

                int DateTimeCounter = 0;
                List<SubCalendarEvent> LowestCostArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
                TimeLine PertinentFreeSpot = null;
                TimeLine FreeSpotUpdated = null;
                j = i + 1;
                if (ListOfFrontPartialsStartTime.Count > 0)//fits any sub calEvent in preceeding restricting free spot
                {
                    DateTimeOffset RestrictedStopper = restrictedSnugFitAvailable[i].Item2.Start;


                    bool breakForLoop = false;
                    bool PreserveRestrictedIndex = false;
                    bool ContinueTrestrictedSnugFitAvailableoForLoop = false;
                    for (; ((FrontPartialCounter < ListOfFrontPartialsStartTime.Count) && (i < restrictedSnugFitAvailable.Count)); FrontPartialCounter++)
                    {
                        TimeLineUpdated = null;
                        DateTimeOffset PertinentFreeSpotStart = EarliestReferenceTIme;
                        DateTimeOffset PertinentFreeSpotEnd;

                        



                        if ((ListOfFrontPartialsStartTime[FrontPartialCounter] < RestrictedStopper))
                        {
                            PertinentFreeSpotEnd = ListOfFrontPartialsStartTime[FrontPartialCounter];
                            //FrontPartials_Dict.Remove(ListOfFrontPartialsStartTime[FrontPartialCounter]);
                            ListOfFrontPartialsStartTime.RemoveAt(FrontPartialCounter);
                            --FrontPartialCounter;
                            PreserveRestrictedIndex = true;
                        }
                        else
                        {
                            PertinentFreeSpotEnd = RestrictedStopper;

                            if (breakForLoop)
                            {//populates with final boundary for each restricted
                                LowestCostArrangement = PlaceSubCalEventInLowestCostPosition(FreeBoundary, restrictedSnugFitAvailable[i].Item2, CompleteArranegement);
                                if (!Utility.PinSubEventsToStart(LowestCostArrangement, FreeBoundary))
                                {
                                    throw new Exception("theres a bug in stitchunrestricted when trying to place restricted n minimum");
                                }

                                CompleteArranegement = LowestCostArrangement;
                                EarliestReferenceTIme = LowestCostArrangement[LowestCostArrangement.Count - 1].End;

                                PreserveRestrictedIndex = false;
                                break;
                            }

                            --FrontPartialCounter;
                            if (j < restrictedSnugFitAvailable.Count)
                            {
                                RestrictedStopper = restrictedSnugFitAvailable[i].Item2.getCalculationRange.End > restrictedSnugFitAvailable[j].Item2.Start ? restrictedSnugFitAvailable[j].Item2.Start : restrictedSnugFitAvailable[i].Item2.getCalculationRange.End;
                            }
                            else
                            {
                                RestrictedStopper = restrictedSnugFitAvailable[i].Item2.getCalculationRange.End > FreeBoundary.End ? FreeBoundary.End : restrictedSnugFitAvailable[i].Item2.getCalculationRange.End;
                            }
                            RestrictedStopper -= restrictedSnugFitAvailable[i].Item2.getActiveDuration;
                            PertinentFreeSpotEnd = RestrictedStopper;//as a result of the comment sections with the string "elemenating excess comments" 
                            breakForLoop = true;
                        }
                        PertinentFreeSpot = new TimeLine(PertinentFreeSpotStart, PertinentFreeSpotEnd);

                        BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                        BorderElementEnd = restrictedSnugFitAvailable[i].Item2;//uses restricted value as boundary element

                        LowestCostArrangement = OptimizeArrangeOfSubCalEvent(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, Occupancy);
                        DateTimeOffset LatestDaterforEarlierReferenceTime = PertinentFreeSpot.Start;
                        LastSubCalElementForEarlierReferenceTime = ((CompleteArranegement.Count < 1) || (CompleteArranegement == null) ? null : CompleteArranegement[CompleteArranegement.Count - 1]);//updates the last element as either null or the last element in the current Complete arrangement
                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();
                        if (LowestCostArrangement.Count > 0)
                        {
                            if (!(LowestCostArrangement[0].getCalculationRange.Start == PertinentFreeSpot.Start))//Pin SubEvents To Start
                            {//if the first element is not a partial Sub Cal Event element
                                FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                                if(!Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated))
                                {
                                    throw new Exception("theres a bug in stitchunrestricted when trying to pin with partial subs, if the first element is not a partial Sub Cal Event element");
                                }

                            }
                            else
                            {
                                //FreeSpotUpdated = new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                                FreeSpotUpdated = PertinentFreeSpot.CreateCopy();
                                if(!Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot))
                                {
                                    throw new Exception("theres a bug in stitchunrestricted when trying to to pin with partial subs, if the first element is a partial Sub Cal Event element");
                                }
                            }
                            EarliestReferenceTIme = PertinentFreeSpot.End;

                            ///Comeback to this
                            ///
                            SubCalendarEvent LastSubCalEvent = LowestCostArrangement[LowestCostArrangement.Count - 1];
                            LatestDaterforEarlierReferenceTime = LastSubCalEvent.End;
                            LastSubCalElementForEarlierReferenceTime = LastSubCalEvent;
                        }


                        TimeLineUpdated = null;

                        if (restrictedSnugFitAvailable[i].Item2.getCalculationRange.Start > LatestDaterforEarlierReferenceTime)
                        {
                            TimeLineUpdated = EnableBetterOptimization? ObtainBetterEarlierReferenceTime(LowestCostArrangement, CalendarIDAndNonPartialSubCalEvents, new TimeLine(LatestDaterforEarlierReferenceTime, RestrictedStopper), EarliestReferenceTIme, new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End), LastSubCalElementForEarlierReferenceTime):null;
                            //errorline

                            if (TimeLineUpdated != null)
                            {
                                LowestCostArrangement = TimeLineUpdated.Item2;
                                EarliestReferenceTIme = TimeLineUpdated.Item1;
                            }
                        }


                        foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                        {
                            --CompatibleWithList[eachSubCalendarEvent.getActiveDuration].Item1;
                            PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Remove(eachSubCalendarEvent.getId);
                            string SubCalString = eachSubCalendarEvent.SubEvent_ID.getCalendarEventComponent();
                            if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                            {
                                CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.getId);
                                if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                                {
                                    CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                                }
                            }
                            if (PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Count < 1)
                            {
                                PossibleEntries_Cpy.Remove(eachSubCalendarEvent.getActiveDuration);
                            }
                        }
                        CompleteArranegement.AddRange(LowestCostArrangement);


                        DateTimeCounter = 0;
                        for (; DateTimeCounter < FrontPartials_Dict.Keys.Count; DateTimeCounter++)//updates CalendarIDAndNonPartialSubCalEvents if frontpartial Startime has been passed. Alls updates FrontPartials_Dict
                        {
                            DateTimeOffset eachDateTIme = FrontPartials_Dict.Keys.ToList()[DateTimeCounter];
                            if (EarliestReferenceTIme >= eachDateTIme)
                            {
                                List<mTuple<bool, SubCalendarEvent>> mTUpleSubCalEvents = FrontPartials_Dict[eachDateTIme];
                                foreach (mTuple<bool, SubCalendarEvent> eachmTUple in mTUpleSubCalEvents)
                                {

                                    string CalLevel0ID = eachmTUple.Item2.SubEvent_ID.getCalendarEventComponent();
                                    if (!CompleteArranegement.Contains(eachmTUple.Item2))
                                    {
                                        if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(CalLevel0ID))
                                        {
                                            CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(eachmTUple.Item2.getId, eachmTUple.Item2);
                                        }
                                        else
                                        {
                                            //CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new List<SubCalendarEvent>() { KeyValuePair0.Value.Item2 });
                                            CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new Dictionary<string, SubCalendarEvent>());
                                            CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(eachmTUple.Item2.getId, eachmTUple.Item2);
                                        }
                                    }
                                }
                                FrontPartials_Dict.Remove(eachDateTIme);
                            }
                        }

                        //ListOfFrontPartialsStartTime = FrontPartials_Dict.Keys.ToList();


                    }
                    if (PreserveRestrictedIndex)//verifies if we took the path of restricted or front partial element. The latter needs a preservation of the current restricted Subcalevent index index 
                    {
                        --i;
                    }
                }
                else
                {//No FrontPartials
                    DateTimeOffset ReferenceEndTime = restrictedSnugFitAvailable[i].Item2.Start;
                    PertinentFreeSpot = new TimeLine(EarliestReferenceTIme, ReferenceEndTime);

                    BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                    BorderElementEnd = restrictedSnugFitAvailable[i].Item2;//uses restricted value as boundary element

                    LowestCostArrangement = OptimizeArrangeOfSubCalEvent(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, Occupancy);

                    if (LowestCostArrangement.Count > 0)
                    {
                        if (!(LowestCostArrangement[0].getCalculationRange.Start == PertinentFreeSpot.Start))//Pin SubEvents To Start
                        {//if the first element is not a partial Sub Cal Event element
                            FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                            if (!Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated))
                            {
                                throw new Exception("theres a bug in stitchunrestricted when trying to pin with partial subs, if the first element is not a partial Sub Cal Event element");
                            }
                        }
                        else
                        {
                            FreeSpotUpdated = new TimeLine(LowestCostArrangement[0].getCalculationRange.Start, PertinentFreeSpot.End);
                            if (!Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot))
                            {
                                throw new Exception("theres a bug in stitchunrestricted when trying to to pin with partial subs, if the first element is a partial Sub Cal Event element");
                            }
                        }
                        EarliestReferenceTIme = LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                    }

                    foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                    {
                        --CompatibleWithList[eachSubCalendarEvent.getActiveDuration].Item1;
                        PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Remove(eachSubCalendarEvent.getId);
                        string SubCalString = eachSubCalendarEvent.SubEvent_ID.getCalendarEventComponent();
                        if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                        {
                            CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.getId);
                            if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                            {
                                CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                            }
                        }
                        if (PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Count < 1)
                        {
                            PossibleEntries_Cpy.Remove(eachSubCalendarEvent.getActiveDuration);
                        }
                    }


                    List<SubCalendarEvent> AdditionalCOstArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
                    DateTimeOffset RelativeEndTime;
                    if (j < restrictedSnugFitAvailable.Count)
                    {
                        //DateTimeOffset StartDateTimeAfterFitting = PertinentFreeSpot.End;
                        DateTimeOffset StartDateTimeAfterFitting = EarliestReferenceTIme;//this is the barring end time of the preceding boundary search. Earliest would have been updated if there was some event detected.


                        RelativeEndTime = restrictedSnugFitAvailable[i].Item2.getCalculationRange.End > restrictedSnugFitAvailable[j].Item2.Start ? restrictedSnugFitAvailable[j].Item2.Start : restrictedSnugFitAvailable[i].Item2.getCalculationRange.End;

                        RelativeEndTime -= restrictedSnugFitAvailable[i].Item2.getActiveDuration;
                        TimeLine CurrentlyFittedTimeLine = new TimeLine(StartDateTimeAfterFitting, RelativeEndTime);

                        BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                        BorderElementEnd = restrictedSnugFitAvailable[i].Item2;//uses restricted value as boundary element

                        AdditionalCOstArrangement = OptimizeArrangeOfSubCalEvent(CurrentlyFittedTimeLine, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, Occupancy);
                        if (AdditionalCOstArrangement.Count > 0)
                        {//Additional get populated
                            if (!(AdditionalCOstArrangement[0].getCalculationRange.Start == CurrentlyFittedTimeLine.Start))//Pin SubEvents To Start
                            {//if the first element is not a partial Sub Cal Event element
                                FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, CurrentlyFittedTimeLine.End);
                                if(!Utility.PinSubEventsToStart(AdditionalCOstArrangement, FreeSpotUpdated))
                                {
                                    throw new Exception("theres a bug in stitchunrestricted when trying to pin with partial subs, if the first element is not a partial Sub Cal Event element");
                                }
                            }
                            else
                            {
                                FreeSpotUpdated = new TimeLine(AdditionalCOstArrangement[0].getCalculationRange.Start, CurrentlyFittedTimeLine.End);
                                
                                if (!Utility.PinSubEventsToStart(AdditionalCOstArrangement, FreeSpotUpdated))
                                {
                                    throw new Exception("theres a bug in stitchunrestricted when trying to to pin with partial subs, if the first element is a partial Sub Cal Event element");
                                }
                            }
                            //++CountCall;
                            foreach (SubCalendarEvent eachSubCalendarEvent in AdditionalCOstArrangement)
                            {
                                --CompatibleWithList[eachSubCalendarEvent.getActiveDuration].Item1;
                                PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Remove(eachSubCalendarEvent.getId);
                                string SubCalString = eachSubCalendarEvent.SubEvent_ID.getCalendarEventComponent();
                                if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                                {
                                    CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.getId);
                                    if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                                    {
                                        CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                                    }
                                }
                                if (PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Count < 1)
                                {
                                    PossibleEntries_Cpy.Remove(eachSubCalendarEvent.getActiveDuration);
                                }
                            }


                            RelativeEndTime = AdditionalCOstArrangement[AdditionalCOstArrangement.Count - 1].End;
                            RelativeEndTime += restrictedSnugFitAvailable[i].Item2.getActiveDuration; ;
                            CurrentlyFittedTimeLine = new TimeLine(FreeSpotUpdated.Start, RelativeEndTime);
                            //AdditionalCOstArrangement = PlaceSubCalEventInLowestCostPosition(CurrentlyFittedTimeLine, restrictedSnugFitAvailable[i].Item2, AdditionalCOstArrangement);
                        }
                        else
                        {//if there is no other Restricted in list
                            RelativeEndTime += restrictedSnugFitAvailable[i].Item2.getActiveDuration;
                            CurrentlyFittedTimeLine = new TimeLine(CurrentlyFittedTimeLine.Start, RelativeEndTime);
                            //AdditionalCOstArrangement = PlaceSubCalEventInLowestCostPosition(CurrentlyFittedTimeLine, restrictedSnugFitAvailable[i].Item2, AdditionalCOstArrangement);
                        }
                    }
                    else
                    {
                        RelativeEndTime = restrictedSnugFitAvailable[i].Item2.getCalculationRange.End > FreeBoundary.End ? FreeBoundary.End : restrictedSnugFitAvailable[i].Item2.getCalculationRange.End;
                        TimeLine CurrentlyFittedTimeLine = new TimeLine(EarliestReferenceTIme, RelativeEndTime);
                        //AdditionalCOstArrangement = PlaceSubCalEventInLowestCostPosition(CurrentlyFittedTimeLine, restrictedSnugFitAvailable[i].Item2, AdditionalCOstArrangement);
                    }

                    CompleteArranegement.AddRange(LowestCostArrangement);
                    CompleteArranegement.AddRange(AdditionalCOstArrangement);
                    TimeLine encasingTimeLine = new TimeLine(FreeBoundary.Start, RelativeEndTime);
                    CompleteArranegement = PlaceSubCalEventInLowestCostPosition(encasingTimeLine, restrictedSnugFitAvailable[i].Item2, CompleteArranegement);
                    if(!Utility.PinSubEventsToStart(CompleteArranegement, FreeBoundary))
                    {
                        throw new Exception("theres a bug in stitchunrestricted when trying to pin CompleteArranegement");
                    }
                    if (CompleteArranegement.Count > 0)
                    {
                        EarliestReferenceTIme = CompleteArranegement[CompleteArranegement.Count - 1].End;
                    }
                }
            }


            { //Handles THe Last Free Space outside of rigids
                TimeLine FreeSpotOutSideRigids = new TimeLine(EarliestReferenceTIme, FreeBoundary.End);
                TimeLine PertinentFreeSpot = new TimeLine(EarliestReferenceTIme, FreeBoundary.End); ;
                TimeLine FreeSpotUpdated;
                List<SubCalendarEvent> LowestCostArrangement;
                if (ListOfFrontPartialsStartTime.Count > 0)
                {
                    for (FrontPartialCounter = 0; FrontPartialCounter < ListOfFrontPartialsStartTime.Count; FrontPartialCounter++)
                    {
                        DateTimeOffset PertinentFreeSpotStart = EarliestReferenceTIme;
                        DateTimeOffset PertinentFreeSpotEnd;
                        PertinentFreeSpotEnd = ListOfFrontPartialsStartTime[FrontPartialCounter];
                        //FrontPartials_Dict.Remove(ListOfFrontPartialsStartTime[FrontPartialCounter]);
                        ListOfFrontPartialsStartTime.RemoveAt(FrontPartialCounter);
                        --FrontPartialCounter;
                        PertinentFreeSpot = new TimeLine(PertinentFreeSpotStart, PertinentFreeSpotEnd);
                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();

                        BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                        BorderElementEnd = null;

                        LowestCostArrangement = OptimizeArrangeOfSubCalEvent(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, Occupancy);
                        DateTimeOffset LatestTimeForBetterEarlierReferenceTime = PertinentFreeSpot.Start;
                        LastSubCalElementForEarlierReferenceTime = ((CompleteArranegement.Count < 1) || (CompleteArranegement == null) ? null : CompleteArranegement[CompleteArranegement.Count - 1]);
                        if (LowestCostArrangement.Count > 0)
                        {
                            if ((LowestCostArrangement[0].getCalculationRange.Start != PertinentFreeSpot.Start))//Pin SubEvents To Start
                            {//if the first element is not a partial Sub Cal Event element
                                FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                                if (!Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated))
                                {
                                    throw new Exception("theres a bug in stitchunrestricted when trying to pin in nonrigids section");
                                }
                            }
                            else
                            {
                                FreeSpotUpdated = PertinentFreeSpot.CreateCopy();// new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                                if(!Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated))
                                {
                                    throw new Exception("theres a bug in stitchunrestricted when trying to pin in nonrigids section");
                                }
                            }
                            EarliestReferenceTIme = PertinentFreeSpot.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                            SubCalendarEvent LastSubCalEvent = LowestCostArrangement[LowestCostArrangement.Count - 1];
                            LatestTimeForBetterEarlierReferenceTime = LastSubCalEvent.End;
                            LastSubCalElementForEarlierReferenceTime = LastSubCalEvent;
                        }


                        TimeLineUpdated = null;
                        TimeLineUpdated = EnableBetterOptimization? ObtainBetterEarlierReferenceTime(LowestCostArrangement, CalendarIDAndNonPartialSubCalEvents, new TimeLine( LatestTimeForBetterEarlierReferenceTime,FreeBoundary.End), EarliestReferenceTIme, new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End), LastSubCalElementForEarlierReferenceTime):null;
                        if (TimeLineUpdated != null)
                        {
                            LowestCostArrangement = TimeLineUpdated.Item2;
                            EarliestReferenceTIme = TimeLineUpdated.Item1;
                        }

                        foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                        {
                            --CompatibleWithList[eachSubCalendarEvent.getActiveDuration].Item1;
                            PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Remove(eachSubCalendarEvent.getId);
                            string SubCalString = eachSubCalendarEvent.SubEvent_ID.getCalendarEventComponent();
                            if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                            {
                                CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.getId);
                                if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                                {
                                    CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                                }
                            }
                            if (PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Count < 1)
                            {
                                PossibleEntries_Cpy.Remove(eachSubCalendarEvent.getActiveDuration);
                            }
                        }
                        CompleteArranegement.AddRange(LowestCostArrangement);
                    }
                }


                DateTimeOffset ReferenceEndTime = FreeBoundary.End;
                PertinentFreeSpot = new TimeLine(EarliestReferenceTIme, ReferenceEndTime);
                BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                BorderElementEnd = null;

                LowestCostArrangement = OptimizeArrangeOfSubCalEvent(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, Occupancy);
                LastSubCalElementForEarlierReferenceTime = ((CompleteArranegement.Count < 1) || (CompleteArranegement == null) ? null : CompleteArranegement[CompleteArranegement.Count - 1]);
                DateTimeOffset LimitForBetterEarlierReferencTime = EarliestReferenceTIme;
                FreeSpotUpdated = PertinentFreeSpot.CreateCopy();
                if (LowestCostArrangement.Count > 0)
                {
                    if ((LowestCostArrangement[0].getCalculationRange.Start != PertinentFreeSpot.Start))//Pin SubEvents To Start
                    {//if the first element is not a partial Sub Cal Event element
                        FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                        if(!Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated))
                        {
                            throw new Exception("theres a bug in stitchunrestricted when trying to pin in nonrigids section");
                        }
                    }
                    else
                    {
                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();// new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                        if(!Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot))
                        {
                                throw new Exception("theres a bug in stitchunrestricted when trying to pin in nonrigids section");
                        }
                    }
                    EarliestReferenceTIme = PertinentFreeSpot.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                    SubCalendarEvent LastSubCalEvent = LowestCostArrangement[LowestCostArrangement.Count - 1];
                    LimitForBetterEarlierReferencTime = LastSubCalEvent.End;
                    LastSubCalElementForEarlierReferenceTime = LastSubCalEvent;

                }
                TimeLineUpdated = null;
                TimeLineUpdated = EnableBetterOptimization?ObtainBetterEarlierReferenceTime(LowestCostArrangement, CalendarIDAndNonPartialSubCalEvents, new TimeLine( LimitForBetterEarlierReferencTime,FreeBoundary.End), EarliestReferenceTIme, new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End), LastSubCalElementForEarlierReferenceTime):null;
                if (TimeLineUpdated != null)
                {
                    LowestCostArrangement = TimeLineUpdated.Item2;
                    EarliestReferenceTIme = TimeLineUpdated.Item1;
                }


                foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                {
                    --CompatibleWithList[eachSubCalendarEvent.getActiveDuration].Item1;
                    PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Remove(eachSubCalendarEvent.getId);
                    string SubCalString = eachSubCalendarEvent.SubEvent_ID.getCalendarEventComponent();
                    if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                    {
                        CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.getId);
                        if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                        {
                            CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                        }
                    }

                    if (PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Count < 1)
                    {
                        PossibleEntries_Cpy.Remove(eachSubCalendarEvent.getActiveDuration);
                    }
                }
                CompleteArranegement.AddRange(LowestCostArrangement);

            }

            if(CompleteArranegement.Count>0)
            { 
                Utility.PinSubEventsToStart(CompleteArranegement,FreeBoundary);
                TimeLine newFreeBoundary= new TimeLine( CompleteArranegement.Last().End,FreeBoundary.End);
                List<mTuple<bool, SubCalendarEvent>> newRestricted=new List<mTuple<bool,SubCalendarEvent>>();
                
                Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> newCompatibleWithList =new Dictionary<TimeSpan,mTuple<int,TimeSpanWithStringID>>();

                IEnumerable<SubCalendarEvent> PossibleEntriesWhatsLeft= PossibleEntries_Cpy.Select(obj=>obj.Value).SelectMany(obj=>obj.Values).Select(obj=>obj.Item2).Where(obj=>obj.canExistWithinTimeLine(newFreeBoundary));
                if(PossibleEntriesWhatsLeft.Count()>0)
                {
                    CompleteArranegement.AddRange(stitchUnRestrictedSubCalendarEvent(newFreeBoundary, newRestricted, PossibleEntries_Cpy, newCompatibleWithList, Occupancy).Select(obj=>obj.Item2));
                }
            }

            List<mTuple<bool, SubCalendarEvent>> retValue = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            foreach (SubCalendarEvent eachSubCalendarEvent in CompleteArranegement)
            {
                PossibleEntries[eachSubCalendarEvent.getActiveDuration][eachSubCalendarEvent.getId].Item1 = true;
                retValue.Add(PossibleEntries[eachSubCalendarEvent.getActiveDuration][eachSubCalendarEvent.getId]);
            }

            //List<List<SubCalendarEvent>> unrestrictedValidCombinations = generateCombinationForDifferentEntries(CompatibleWithList, PossibleEntries);
            retValue = reAlignSubCalEvents(FreeBoundary, retValue);
            if (TotalEventsForThisTImeLine != retValue.Count)
            {
                ;
            }
            
            if (!Utility.PinSubEventsToStart(retValue.Select(obj=>obj.Item2), FreeBoundary))
            {
                throw new Exception("theres an error with stitch unrestricted at iteration" + CountCall);
            }

            
            return retValue;
        }






        List<SubCalendarEvent> OptimizeArrangeOfSubCalEvent(TimeLine PertinentFreeSpot, Tuple<SubCalendarEvent, SubCalendarEvent> BoundaryCalendarEvent, List<mTuple<int, TimeSpanWithStringID>> CompatibleWithList, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries_Cpy, double occupancy = 0, bool Aggressive = true)
        {
            CompatibleWithList.Clear();
            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleSubCalEvents = removeSubCalEventsThatCantWorkWithTimeLine(PertinentFreeSpot, PossibleEntries_Cpy, true);
            Dictionary<DateTimeOffset, Dictionary<TimeSpan, int>> DeadLineTODuration = new Dictionary<DateTimeOffset, Dictionary<TimeSpan, int>>();
            foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleSubCalEvents)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                CompatibleWithList.Add(new mTuple<int, TimeSpanWithStringID>(eachKeyValuePair.Value.Count, new TimeSpanWithStringID(eachKeyValuePair.Value.ToList()[0].Value.Item2.getActiveDuration, eachKeyValuePair.Key.Ticks.ToString())));

                foreach (SubCalendarEvent eachSubcalevent in eachKeyValuePair.Value.Values.Select(obj => obj.Item2))
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

            List<SubCalendarEvent> AllSubEvents_ForDebugging = PossibleSubCalEvents.SelectMany(obj => obj.Value.Select(obj1 => obj1.Value.Item2)).ToList();


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
                if (AllPossibleBestFit_beforeBreak.Count > 1)
                {
                    ;
                }
                //List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AveragedBestFit = getAveragedOutTIimeLine(var3_beforeBreak, 0);
                List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AveragedBestFit = OptimizeForDeadLine(DeadLineTODuration, PertinentFreeSpot.TimelineSpan);
                Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> removedImpossibleValue = removeSubCalEventsThatCantWorkWithTimeLine(PertinentFreeSpot, PossibleSubCalEvents);
                List<List<SubCalendarEvent>> PossibleSubCaleventsCobination = removedImpossibleValue.Count > 0 ? generateCombinationForDifferentEntries(AveragedBestFit[0], removedImpossibleValue) : new List<List<SubCalendarEvent>>();

                if (Aggressive)
                {
                    if (PossibleSubCaleventsCobination.Count > 1)
                    {
                        PossibleSubCaleventsCobination.OrderByDescending(obj => obj.Count);

                        PossibleSubCaleventsCobination = PossibleSubCaleventsCobination.GetRange(0, 1);
                    }
                }

                if (PossibleSubCaleventsCobination.Count>1)
                {
                    ;

                }
                //PossibleSubCaleventsCobination = Utility.RandomizeIEnumerable(PossibleSubCaleventsCobination);
                LowestCostArrangement = getArrangementWithLowestDistanceCost(PossibleSubCaleventsCobination, BoundaryCalendarEvent);
                //TimeLine FreeSpotUpdated;
            }

            return LowestCostArrangement;

        }


        List<SubCalendarEvent> OptimizeArrangeOfSubCalEvent_NoMtuple (TimeLine PertinentFreeSpot, Tuple<SubCalendarEvent, SubCalendarEvent> BoundaryCalendarEvent,  Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> PossibleEntries_Cpy, double occupancy = 0, bool Aggressive = true)
        {
            List<mTuple<int, TimeSpanWithStringID>> CompatibleWithList = new List<mTuple<int, TimeSpanWithStringID>>();
            Dictionary<TimeSpan, Dictionary<string,  SubCalendarEvent>> PossibleSubCalEvents = removeSubCalEventsThatCantWorkWithTimeLine_NoMtuple(PertinentFreeSpot, PossibleEntries_Cpy, true);
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
                if (AllPossibleBestFit_beforeBreak.Count > 1)
                {
                    ;
                }
                //List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AveragedBestFit = getAveragedOutTIimeLine(var3_beforeBreak, 0);
                List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AveragedBestFit = OptimizeForDeadLine(DeadLineTODuration, PertinentFreeSpot.TimelineSpan);
                Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> removedImpossibleValue = removeSubCalEventsThatCantWorkWithTimeLine_NoMtuple(PertinentFreeSpot, PossibleSubCalEvents);
                List<List<SubCalendarEvent>> PossibleSubCaleventsCobination = generateCombinationForDifferentEntries_NoMtuple (AveragedBestFit[0], removedImpossibleValue);

                if (Aggressive)
                {
                    if (PossibleSubCaleventsCobination.Count > 1)
                    {
                        PossibleSubCaleventsCobination.OrderByDescending(obj => obj.Count);

                        PossibleSubCaleventsCobination = PossibleSubCaleventsCobination.GetRange(0, 1);
                    }
                }

                if (PossibleSubCaleventsCobination.Count > 1)
                {
                    ;

                }
                //PossibleSubCaleventsCobination = Utility.RandomizeIEnumerable(PossibleSubCaleventsCobination);
                LowestCostArrangement = getArrangementWithLowestDistanceCost(PossibleSubCaleventsCobination, BoundaryCalendarEvent);
                //TimeLine FreeSpotUpdated;
            }

            return LowestCostArrangement;

        }


        List<SubCalendarEvent> OptimizeArrangeOfSubCalEvent_NonAggressive(TimeLine PertinentFreeSpot, Tuple<SubCalendarEvent, SubCalendarEvent> BoundaryCalendarEvent, List<mTuple<int, TimeSpanWithStringID>> CompatibleWithList, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries_Cpy, List<mTuple<bool, SubCalendarEvent>> restOfRestrictedElements, double occupancy = 0)
        {
            CompatibleWithList.Clear();

            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in restOfRestrictedElements)
            {
                if (PossibleEntries_Cpy.ContainsKey(eachmTuple.Item2.RangeSpan))
                {
                    PossibleEntries_Cpy[eachmTuple.Item2.RangeSpan].Add(eachmTuple.Item2.getId, eachmTuple);
                }
                else
                {
                    PossibleEntries_Cpy.Add(eachmTuple.Item2.RangeSpan, new Dictionary<string, mTuple<bool, SubCalendarEvent>>());
                    PossibleEntries_Cpy[eachmTuple.Item2.RangeSpan].Add(eachmTuple.Item2.getId, eachmTuple);
                }
            }


            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleSubCalEvents = removeSubCalEventsThatCantWorkWithTimeLine(PertinentFreeSpot, PossibleEntries_Cpy, true);
            foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleSubCalEvents)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                CompatibleWithList.Add(new mTuple<int, TimeSpanWithStringID>(eachKeyValuePair.Value.Count, new TimeSpanWithStringID(eachKeyValuePair.Value.ToList()[0].Value.Item2.getActiveDuration, eachKeyValuePair.Key.Ticks.ToString())));
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
                List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AveragedBestFit = getAveragedOutTIimeLine(var3_beforeBreak, 0);
                Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> removedImpossibleValue = removeSubCalEventsThatCantWorkWithTimeLine(PertinentFreeSpot, PossibleEntries_Cpy);
                List<List<SubCalendarEvent>> PossibleSubCaleventsCobination = generateCombinationForDifferentEntriesNonAggressive(AveragedBestFit[0], removedImpossibleValue);
                //PossibleSubCaleventsCobination = Utility.RandomizeIEnumerable(PossibleSubCaleventsCobination);
                LowestCostArrangement = getArrangementWithLowestDistanceCost(PossibleSubCaleventsCobination, BoundaryCalendarEvent);
                TimeLine FreeSpotUpdated;

            }


            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in restOfRestrictedElements)
            {
                if (PossibleEntries_Cpy.ContainsKey(eachmTuple.Item2.RangeSpan))
                {
                    PossibleEntries_Cpy[eachmTuple.Item2.RangeSpan].Remove(eachmTuple.Item2.getId);
                }
                /*else
                {
                    PossibleEntries_Cpy.Add(eachmTuple.Item2.RangeSpan, new Dictionary<string, mTuple<bool, SubCalendarEvent>>());
                    PossibleEntries_Cpy[eachmTuple.Item2.RangeSpan].Add(eachmTuple.Item2.ID, eachmTuple);
                }*/
            }



            return LowestCostArrangement;

        }



        int CountMostRestrictedElementsInDict(IEnumerable<TimeSpan> AllTImeSpan, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> myDict)
        {
            int CountSoFar = 0;
            foreach (TimeSpan eachTimeSpan in AllTImeSpan)
            {
                if (myDict.ContainsKey(eachTimeSpan))
                {
                    ++CountSoFar;
                }
            }
            return CountSoFar;
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




        //Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> removedImpossibleValue = removeSubCalEventsThatCantWorkWithTimeLine(PertinentFreeSpot, PossibleEntries_Cpy);
        List<mTuple<bool, SubCalendarEvent>> removeSubCalEventsThatCantWorkWithTimeLine(TimeLine PertinentFreeSpot, List<KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> PossibleEntries, bool MustFitAnyWhere = false)
        {
            /*
             * THis funcction checks if the active duration of each subcalevent in PossibleEntries can fit within the PertinentFreeSpot. Also the it checks if PertinentFreeSpot breaks the enclosing timeLine for the currently echeckcked subcalendar event. if both conditions are satisfied the subcalevent gets inserted into the retvalue
             * MustFitAnyWhere variable checks if the subcalevent can exist any where within the PertinentFreeSpot timeline, as well as verify the steps above. Its default value is false i.e it checks if the full subcalevent can exist in part of the pertinentfreespot
             
             */

            List<mTuple<bool, SubCalendarEvent>> retValue = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();



            foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleEntries)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                Dictionary<string, mTuple<bool, SubCalendarEvent>> UpdatedEntries = new System.Collections.Generic.Dictionary<string, mTuple<bool, SubCalendarEvent>>();

                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> eachKeyValuePair0 in eachKeyValuePair.Value)
                {
                    if (MustFitAnyWhere)
                    {
                        if (eachKeyValuePair0.Value.Item2.getCalendarEventRange.IsTimeLineWithin(PertinentFreeSpot) && (eachKeyValuePair0.Value.Item2.getActiveDuration <= PertinentFreeSpot.TimelineSpan))
                        {
                            retValue.Add(eachKeyValuePair0.Value);
                        }
                    }
                    else
                    {
                        if (eachKeyValuePair0.Value.Item2.canExistWithinTimeLine(PertinentFreeSpot))
                        {
                            retValue.Add(eachKeyValuePair0.Value);
                        }
                    }
                }
            }

            return retValue;


        }



        Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> removeSubCalEventsThatCantWorkWithTimeLine(TimeLine PertinentFreeSpot, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries, bool MustFitAnyWhere = false)
        {
            /*
             * THis funcction checks if the active duration of each subcalevent in PossibleEntries can fit within the PertinentFreeSpot. Also the it checks if PertinentFreeSpot breaks the enclosing timeLine for the currently echeckcked subcalendar event. if both conditions are satisfied the subcalevent gets inserted into the retvalue
             * MustFitAnyWhere variable checks if the subcalevent can exist any where within the PertinentFreeSpot timeline, as well as verify the steps above. Its default value is false i.e it checks if the full subcalevent can exist in part of the pertinentfreespot
             
             */

            List<mTuple<bool, SubCalendarEvent>> retValueList = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> retValue = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();


            foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleEntries)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                Dictionary<string, mTuple<bool, SubCalendarEvent>> UpdatedEntries = new System.Collections.Generic.Dictionary<string, mTuple<bool, SubCalendarEvent>>();

                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> eachKeyValuePair0 in eachKeyValuePair.Value)
                {
                    if (MustFitAnyWhere)
                    {
                        if (eachKeyValuePair0.Value.Item2.getCalendarEventRange.IsTimeLineWithin(PertinentFreeSpot) && (eachKeyValuePair0.Value.Item2.getActiveDuration <= PertinentFreeSpot.TimelineSpan))
                        {
                            retValueList.Add(eachKeyValuePair0.Value);
                        }
                    }
                    else
                    {
                        if (eachKeyValuePair0.Value.Item2.canExistWithinTimeLine(PertinentFreeSpot))
                        {
                            retValueList.Add(eachKeyValuePair0.Value);
                        }
                    }
                }
            }

            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in retValueList)
            {
                if (retValue.ContainsKey(eachmTuple.Item2.getActiveDuration))
                {
                    retValue[eachmTuple.Item2.getActiveDuration].Add(eachmTuple.Item2.getId, eachmTuple);
                }
                else
                {
                    retValue.Add(eachmTuple.Item2.getActiveDuration, new Dictionary<string, mTuple<bool, SubCalendarEvent>>());
                    retValue[eachmTuple.Item2.getActiveDuration].Add(eachmTuple.Item2.getId, eachmTuple);
                }
            }


            return retValue;


        }


        Dictionary<TimeSpan, Dictionary<string,  SubCalendarEvent>> removeSubCalEventsThatCantWorkWithTimeLine_NoMtuple(TimeLine PertinentFreeSpot, Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> PossibleEntries, bool MustFitAnyWhere = false)
        {
            /*
             * THis funcction checks if the active duration of each subcalevent in PossibleEntries can fit within the PertinentFreeSpot. Also the it checks if PertinentFreeSpot breaks the enclosing timeLine for the currently echeckcked subcalendar event. if both conditions are satisfied the subcalevent gets inserted into the retvalue
             * MustFitAnyWhere variable checks if the subcalevent can exist any where within the PertinentFreeSpot timeline, as well as verify the steps above. Its default value is false i.e it checks if the full subcalevent can exist in part of the pertinentfreespot
             
             */

            List<SubCalendarEvent> retValueList = new System.Collections.Generic.List<SubCalendarEvent>();
            Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> retValue = new Dictionary<TimeSpan, Dictionary<string,  SubCalendarEvent>>();


            foreach (KeyValuePair<TimeSpan, Dictionary<string, SubCalendarEvent>> eachKeyValuePair in PossibleEntries)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                Dictionary<string, mTuple<bool, SubCalendarEvent>> UpdatedEntries = new System.Collections.Generic.Dictionary<string, mTuple<bool, SubCalendarEvent>>();

                foreach (KeyValuePair<string,  SubCalendarEvent> eachKeyValuePair0 in eachKeyValuePair.Value)
                {
                    if (MustFitAnyWhere)
                    {
                        if (eachKeyValuePair0.Value.getCalendarEventRange.IsTimeLineWithin(PertinentFreeSpot) && (eachKeyValuePair0.Value.getActiveDuration <= PertinentFreeSpot.TimelineSpan))
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
                    retValue.Add(eachmTuple.getActiveDuration, new Dictionary<string,  SubCalendarEvent>());
                    retValue[eachmTuple.getActiveDuration].Add(eachmTuple.getId, eachmTuple);
                }
            }


            return retValue;


        }




        Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> removeSubCalEventsThatCantWorkWithTimeLine(TimeLine PertinentFreeSpot, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries)
        {
            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> retValue = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();


            foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleEntries)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                Dictionary<string, mTuple<bool, SubCalendarEvent>> UpdatedEntries = new System.Collections.Generic.Dictionary<string, mTuple<bool, SubCalendarEvent>>();

                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> eachKeyValuePair0 in eachKeyValuePair.Value)
                {
                    if (eachKeyValuePair0.Value.Item2.canExistWithinTimeLine(PertinentFreeSpot))
                    {
                        UpdatedEntries.Add(eachKeyValuePair0.Key, eachKeyValuePair0.Value);
                    }
                }


                if (UpdatedEntries.Count > 0)
                { retValue.Add(eachKeyValuePair.Key, UpdatedEntries); }
            }


            return retValue;
        }

        List<List<mTuple<bool, SubCalendarEvent>>> ThisWillClash = new System.Collections.Generic.List<System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();

        List<mTuple<bool, SubCalendarEvent>> reAlignSubCalEvents(TimeLine BoundaryTimeLine, List<mTuple<bool, SubCalendarEvent>> ListOfEvents)
        {
            DateTimeOffset ReferenceTime = BoundaryTimeLine.Start;
            TimeLine Boundary_Cpy = BoundaryTimeLine.CreateCopy();
            List<mTuple<bool, SubCalendarEvent>> myClashers = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();


            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in ListOfEvents)
            {



                if (!eachmTuple.Item2.PinToStart(Boundary_Cpy))
                {
                    myClashers.Add(eachmTuple);

                    //throw new Exception("error in your shift algorithm");
                }
                else
                {
                    Boundary_Cpy = new TimeLine(eachmTuple.Item2.End, BoundaryTimeLine.End);
                };
            }


            ThisWillClash.Add(myClashers);


            return ListOfEvents;
        }



        List<SubCalendarEvent> PlaceSubCalEventInLowestCostPosition(TimeLine MyLimitingTimeLine, SubCalendarEvent mySubcalevent, List<SubCalendarEvent> OptimizedArrangementOfEvent)
        {

            /**Hack Solution Start, this just assumes all events are right next to each other and appends mySubcalevetn to the end. It also shifts this sub cal event to represent this shift **/
            DateTimeOffset RelativeStartTime = MyLimitingTimeLine.Start + Utility.SumOfActiveDuration(OptimizedArrangementOfEvent);
            TimeLine encasingTimeLine = MyLimitingTimeLine.InterferringTimeLine(mySubcalevent.getCalculationRange);

            IEnumerable<SubCalendarEvent> Interferringevents = getInterferringSubEvents(encasingTimeLine, OptimizedArrangementOfEvent);
            List<SubCalendarEvent> ListSofar = Interferringevents.ToList();

            List<List<SubCalendarEvent>> AllPertinentList = new List<List<SubCalendarEvent>>();
            List<SubCalendarEvent> retValue = OptimizedArrangementOfEvent.ToList();
            retValue.Add(mySubcalevent);

#if EnableRestrictedLocationOptimization
            int NumberOfLists = ListSofar.Count;
            int LastInt = 0;
            List<SubCalendarEvent> currList;

            if (ListSofar.Count > 0)
            {
                ListSofar.RemoveAt(0);
                IEnumerable<int> AllValidIndexes = ListSofar.Select(obj => OptimizedArrangementOfEvent.IndexOf(obj));
                foreach (int eachInt in AllValidIndexes)
                {
                    currList = OptimizedArrangementOfEvent.ToList();
                    currList.Insert(eachInt, mySubcalevent);
                    AllPertinentList.Add(currList);
                    LastInt = eachInt;
                }

                currList = OptimizedArrangementOfEvent.ToList();
                currList.Insert(LastInt + 1, mySubcalevent);
                AllPertinentList.Add(currList);
                List<List<SubCalendarEvent>> ListThatWorks = new List<List<SubCalendarEvent>>();

                foreach (List<SubCalendarEvent> eachList in AllPertinentList)
                {
                    if (Utility.PinSubEventsToStart(eachList, MyLimitingTimeLine))
                    {
#if createCopyOfImplementation
                        List<SubCalendarEvent> eachListCpy = eachList.Select(obj => obj.createCopy()).ToList();
                        ListThatWorks.Add(eachListCpy);
#else
                        ListThatWorks.Add(eachList);
#endif
                    }
                }

                double lowestSofar = double.MaxValue;
            
                foreach (List<SubCalendarEvent> eachList in ListThatWorks)
                {
                    double currDistance = Utility.calculateDistance(eachList.ToList(), CalendarEvent.DistanceMatrix);
                    if (currDistance < lowestSofar)
                    { 
                        lowestSofar=currDistance;
#if createCopyOfImplementation
                        retValue = eachList.SelectMany(obj => retValue.Where(obj0 => obj0.ID == obj.ID)).ToList(); ;
#else
                        retValue =eachList;
#endif

                    }
                }
            }

            Utility.PinSubEventsToStart(retValue, MyLimitingTimeLine);

#endif

            return retValue;
        }

        List<SubCalendarEvent> getArrangementWithLowestDistanceCost(List<List<SubCalendarEvent>> viableCombinations, Tuple<SubCalendarEvent, SubCalendarEvent> BoundinngSubCaEvents)
        {
            return viableCombinations.First();
        }

        Tuple<List<SubCalendarEvent>, double> rearrangeForOptimizedLocationOptimizedLocation(List<SubCalendarEvent> ListOfLocations)
        {
            Tuple<List<SubCalendarEvent>, double> retValue = new Tuple<System.Collections.Generic.List<SubCalendarEvent>, double>(ListOfLocations, calculateCostOSubCalArrangement(ListOfLocations));
            return retValue;
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

        Dictionary<TimeSpan, Dictionary<string,  SubCalendarEvent>> useAggressivePossibilitiesEntry_NoMtuple(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithList, Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> PossibleEntries)
        {

            Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> retValue = new Dictionary<TimeSpan, Dictionary<string,  SubCalendarEvent>>();
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
        List<List<SubCalendarEvent>> generateCombinationForDifferentEntries(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithList, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries, bool Aggressive = true)
        {
            /*
             * Function attempts to generate multiple combinations of compatible sub calendar event for Snug fit entry
             * CompatibleWithList is an snug fit result
             * PossibleEntries are the possible sub calendar that can be used in the combinatorial result
             */


            if (Aggressive)
            {
                PossibleEntries = useAggressivePossibilitiesEntry(CompatibleWithList, PossibleEntries);

            }



            List<List<List<string>>> MAtrixedSet = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<string>>>();
            Dictionary<string, mTuple<int, List<SubCalendarEvent>>> var4 = new System.Collections.Generic.Dictionary<string, mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>>();
            List<List<SubCalendarEvent>> retValue = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair0 in CompatibleWithList)//loops every timespan in Snug FIt possibility
            {
                TimeSpan eachTimeSpan = eachKeyValuePair0.Key;

                Dictionary<string, mTuple<bool, SubCalendarEvent>> var1 = PossibleEntries[eachTimeSpan];
                List<List<string>> receivedValue = new System.Collections.Generic.List<System.Collections.Generic.List<string>>();
                Dictionary<string, int> var2 = new System.Collections.Generic.Dictionary<string, int>();
                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> eachKeyValuePair in var1)
                {
                    string ParentID = eachKeyValuePair.Value.Item2.SubEvent_ID.getIDUpToCalendarEvent();
                    if (var2.ContainsKey(ParentID))
                    {
                        ++var2[ParentID];
                        var4[ParentID].Item2.Add(eachKeyValuePair.Value.Item2);
                    }
                    else
                    {
                        var2.Add(ParentID, 1);
                        List<SubCalendarEvent> var5 = new System.Collections.Generic.List<SubCalendarEvent>();
                        var5.Add(eachKeyValuePair.Value.Item2);
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

        List<List<SubCalendarEvent>> generateCombinationForDifferentEntries_NoMtuple(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithList, Dictionary<TimeSpan, Dictionary<string, SubCalendarEvent>> PossibleEntries, bool Aggressive = true)
        {
            /*
             * Function attempts to generate multiple combinations of compatible sub calendar event for Snug fit entry
             * CompatibleWithList is an snug fit result
             * PossibleEntries are the possible sub calendar that can be used in the combinatorial result
             */


            if (Aggressive)
            {
                PossibleEntries = useAggressivePossibilitiesEntry_NoMtuple(CompatibleWithList, PossibleEntries);

            }



            List<List<List<string>>> MAtrixedSet = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<string>>>();
            Dictionary<string, mTuple<int, List<SubCalendarEvent>>> var4 = new System.Collections.Generic.Dictionary<string, mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>>();
            List<List<SubCalendarEvent>> retValue = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
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


        List<List<SubCalendarEvent>> OptimizeForLocation(IEnumerable<IEnumerable<SubCalendarEvent>> AllSubCalEvents, IEnumerable<TimeLine> AllFreeSpots)
        {
            TimeLine[] AllFreeSpots_Array = AllFreeSpots.ToArray();
            List<List<SubCalendarEvent>> retValue = AllSubCalEvents.Select(obj => obj.ToList()).ToList();
#if enableMultithreading

            //int i = 0;



            Parallel.For(0, AllFreeSpots.Count(), i =>
            {
                IEnumerable<SubCalendarEvent> eachListOfSubCalEvents = retValue[i];

                List<SubCalendarEvent> beforeValue = eachListOfSubCalEvents.ToList();
                List<SubCalendarEvent> Aftervalue = OptimizeEachFreeSpotForLocation(AllFreeSpots_Array[i], beforeValue);
                if (beforeValue.Count != Aftervalue.Count)
                {
                    Utility.PinSubEventsToStart(beforeValue, AllFreeSpots_Array[i]);

                    Aftervalue = beforeValue;
                }
                retValue[i] = Aftervalue;


                i++;
            });
            return retValue;


#else
            int i = 0;


            foreach (IEnumerable<SubCalendarEvent> eachListOfSubCalEvents in AllSubCalEvents)
            {
                List<SubCalendarEvent> beforeValue = eachListOfSubCalEvents.ToList();

                if (i == 5)
                {
                    ;
                }


                List<SubCalendarEvent> Aftervalue = OptimizeEachFreeSpotForLocation(AllFreeSpots_Array[i], beforeValue);
                if (beforeValue.Count != Aftervalue.Count)
                {

                    Utility.PinSubEventsToStart(beforeValue, AllFreeSpots_Array[i]);

                    Aftervalue = beforeValue;
                }



                retValue[i] = Aftervalue;


                i++;
            }
            return retValue;
#endif

        }


        List<SubCalendarEvent> OptimizeEachFreeSpotForLocation(TimeLine referenceTimeline, List<SubCalendarEvent> AllInsertedElements)
        {
            Dictionary<DateTimeOffset, List<SubCalendarEvent>> frontPartialsDict = new Dictionary<DateTimeOffset, List<SubCalendarEvent>>();
            Dictionary<DateTimeOffset, List<SubCalendarEvent>> endPartialDict = new Dictionary<DateTimeOffset, List<SubCalendarEvent>>();
            IEnumerable<SubCalendarEvent> StartingBeforeReferenceTimeLine = AllInsertedElements.Where(obj => obj.getCalculationRange.Start <= referenceTimeline.Start);
            IEnumerable<SubCalendarEvent> startingAfterReferenceTimeLineStart = AllInsertedElements.Where(obj => obj.getCalculationRange.Start > referenceTimeline.Start);
            IEnumerable<SubCalendarEvent> endingBefore = AllInsertedElements.Where(obj => obj.getCalculationRange.End <= referenceTimeline.End);



            IEnumerable<SubCalendarEvent> endingBeforeAndStartBefore = AllInsertedElements.Where(obj => ((obj.getCalculationRange.End <= referenceTimeline.End) && (obj.getCalculationRange.Start <= referenceTimeline.Start)));

            IEnumerable<SubCalendarEvent> freeRoaming = AllInsertedElements.Where(obj => ((obj.getCalculationRange.Start <= referenceTimeline.Start) && (obj.getCalculationRange.End >= referenceTimeline.End)));

            IEnumerable<SubCalendarEvent> restrictedElements = startingAfterReferenceTimeLineStart;//AllInsertedElements.Where(obj => !freeRoaming.Contains(obj));
            IEnumerable<mTuple<bool, SubCalendarEvent>> CompatibleWithTimeLine = AllInsertedElements.Select(obj => new mTuple<bool, SubCalendarEvent>(false, obj));
            List<mTuple<bool, SubCalendarEvent>> restrictedSnugFitAvailable = stitchRestrictedSubCalendarEvent(CompatibleWithTimeLine.ToList(), referenceTimeline);

            //restrictedElements.Select(obj=> new mTuple<bool,SubCalendarEvent>(false,obj)).ToList();
            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();



            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in CompatibleWithTimeLine)
            {
                TimeSpan ActiveTimeSpan = eachmTuple.Item2.getActiveDuration;
                string subcalStringID = eachmTuple.Item2.getId;

                if (PossibleEntries.ContainsKey(ActiveTimeSpan))
                {
                    PossibleEntries[ActiveTimeSpan].Add(subcalStringID, new mTuple<bool, SubCalendarEvent>(true, eachmTuple.Item2));
                }
                else
                {
                    PossibleEntries.Add(ActiveTimeSpan, new Dictionary<string, mTuple<bool, SubCalendarEvent>>());
                    PossibleEntries[ActiveTimeSpan].Add(subcalStringID, new mTuple<bool, SubCalendarEvent>(true, eachmTuple.Item2));
                }
            }

            List<SubCalendarEvent> retvalue = stitchUnRestrictedSubCalendarEvent_NonAggressive(referenceTimeline, restrictedSnugFitAvailable, PossibleEntries, new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>(), 1).Select(obj => obj.Item2).ToList();

            return retvalue;
        }



        List<mTuple<bool, SubCalendarEvent>> stitchUnRestrictedSubCalendarEvent_NonAggressive(TimeLine FreeBoundary, List<mTuple<bool, SubCalendarEvent>> restrictedSnugFitAvailable, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithList, double Occupancy)
        {
            TimeLine[] AllFreeSpots = FreeBoundary.getAllFreeSlots();
            int TotalEventsForThisTImeLine = 0;

            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in CompatibleWithList)
            {
                TotalEventsForThisTImeLine += eachKeyValuePair.Value.Item1;
            }

            CompatibleWithList.Clear();


            DateTimeOffset EarliestReferenceTIme = FreeBoundary.Start;
            List<mTuple<bool, SubCalendarEvent>> FrontPartials = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            List<mTuple<bool, SubCalendarEvent>> EndPartials = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            Dictionary<DateTimeOffset, List<mTuple<bool, SubCalendarEvent>>> FrontPartials_Dict = new System.Collections.Generic.Dictionary<DateTimeOffset, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();
            Dictionary<DateTimeOffset, List<mTuple<bool, SubCalendarEvent>>> EndPartials_Dict = new System.Collections.Generic.Dictionary<DateTimeOffset, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();
            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries_Cpy = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();
            Dictionary<string, Dictionary<string, SubCalendarEvent>> CalendarIDAndNonPartialSubCalEvents = new Dictionary<string, Dictionary<string, SubCalendarEvent>>();//List of non partials for current Reference StartTime To End of FreeBoundary. Its gets updated with Partials once the earliest reference time passes the partial event start time

            foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleEntries)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                Dictionary<string, mTuple<bool, SubCalendarEvent>> NewDictEntry = new Dictionary<string, mTuple<bool, SubCalendarEvent>>();

                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> KeyValuePair0 in eachKeyValuePair.Value)
                {
                    mTuple<bool, SubCalendarEvent> MyEvent = KeyValuePair0.Value;

                    if (MyEvent.Item2.getId == "469_471")
                    {
                        ;
                    }

                    bool isInrestrictedSnugFitAvailable = false;
                    if (CompatibleWithList.ContainsKey(eachKeyValuePair.Key))
                    {
                        ++CompatibleWithList[eachKeyValuePair.Key].Item1;
                    }
                    else
                    {
                        CompatibleWithList.Add(eachKeyValuePair.Key, new mTuple<int, TimeSpanWithStringID>(1, new TimeSpanWithStringID(KeyValuePair0.Value.Item2.getActiveDuration, KeyValuePair0.Value.Item2.getActiveDuration.Ticks.ToString())));
                    }

                    foreach (mTuple<bool, SubCalendarEvent> eachMtuple in restrictedSnugFitAvailable)//checks if event is in restricted list
                    {
                        if (eachMtuple.Item2.getId == MyEvent.Item2.getId)
                        {
                            isInrestrictedSnugFitAvailable = true;
                            break;
                        }

                    }


                    if (!isInrestrictedSnugFitAvailable)//stops restricted elements from being used in caslculation
                    {
                        NewDictEntry.Add(KeyValuePair0.Value.Item2.getId, KeyValuePair0.Value);
                        if (FreeBoundary.IsDateTimeWithin(KeyValuePair0.Value.Item2.getCalculationRange.Start))
                        {
                            FrontPartials.Add(KeyValuePair0.Value);
                        }
                        else
                        {
                            if (FreeBoundary.IsDateTimeWithin(KeyValuePair0.Value.Item2.getCalculationRange.End))
                            {
                                EndPartials.Add(KeyValuePair0.Value);
                            }
                            else
                            {
                                string CalLevel0ID = KeyValuePair0.Value.Item2.SubEvent_ID.getCalendarEventComponent();
                                if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(CalLevel0ID))
                                {
                                    CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(KeyValuePair0.Value.Item2.getId, KeyValuePair0.Value.Item2);
                                }
                                else
                                {
                                    //CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new List<SubCalendarEvent>() { KeyValuePair0.Value.Item2 });
                                    CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new Dictionary<string, SubCalendarEvent>());
                                    CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(KeyValuePair0.Value.Item2.getId, KeyValuePair0.Value.Item2);
                                }

                            }
                        }
                    }
                }
                if (NewDictEntry.Count > 0)
                { PossibleEntries_Cpy.Add(eachKeyValuePair.Key, NewDictEntry); }

            }

            FrontPartials = FrontPartials.OrderBy(obj => obj.Item2.getCalculationRange.Start).ToList();
            EndPartials = EndPartials.OrderBy(obj => obj.Item2.getCalculationRange.End).ToList();

            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in FrontPartials)//populates FrontPartials_Dict in ordered manner since FrontPartials is ordered
            {
                if (FrontPartials_Dict.ContainsKey(eachmTuple.Item2.getCalculationRange.Start))
                {
                    FrontPartials_Dict[eachmTuple.Item2.getCalculationRange.Start].Add(eachmTuple);
                }
                else
                {
                    FrontPartials_Dict.Add(eachmTuple.Item2.getCalculationRange.Start, new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>() { eachmTuple });
                }

            }

            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in EndPartials)//populates EndPartials_Dict in ordered manner since EndPartials is ordered
            {
                if (EndPartials_Dict.ContainsKey(eachmTuple.Item2.getCalculationRange.Start))
                {
                    EndPartials_Dict[eachmTuple.Item2.getCalculationRange.Start].Add(eachmTuple);
                }
                else
                {
                    EndPartials_Dict.Add(eachmTuple.Item2.getCalculationRange.Start, new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>() { eachmTuple });
                }
            }

            List<mTuple<bool, SubCalendarEvent>> restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
            IEnumerable<SubCalendarEvent> selectedRestrictedElements;
            List<SubCalendarEvent> CompleteArranegement = new System.Collections.Generic.List<SubCalendarEvent>();
            int StartingReferneceIndex = 0;


            /*foreach (mTuple<bool, SubCalendarEvent> eachmTuple in restrictedSnugFitAvailable)//removes the restricted from CompatibleWithList
            {
                --CompatibleWithList[eachmTuple.Item2.ActiveDuration.Ticks.ToString()].Item1;
                //PossibleEntries_Cpy[eachmTuple.Item2.ActiveDuration.Ticks.ToString()].Remove(eachmTuple.Item2.ID);
            }*/

            List<DateTimeOffset> ListOfFrontPartialsStartTime = FrontPartials_Dict.Keys.ToList();

            int i = 0;
            int j = 0;
            int FrontPartialCounter = 0;

            Tuple<DateTimeOffset, List<SubCalendarEvent>> TimeLineUpdated = null;
            SubCalendarEvent BorderElementBeginning = null;
            SubCalendarEvent BorderElementEnd = null;
            SubCalendarEvent LastSubCalElementForEarlierReferenceTime = null;
            int a = restrictedSnugFitAvailable.Count;
            int previ = i;

            Utility.PinSubEventsToEnd(restrictedSnugFitAvailable.Select(obj => obj.Item2).ToList(), FreeBoundary);
            bool ignorePlaceRestrictedinBestPosition = false;

            for (; i < restrictedSnugFitAvailable.Count; i++)
            {
                //bool isFreeSpotBeforeRigid = AllFreeSpots[i].End <= restrictedSnugFitAvailable[i].Item2.Start;
                TimeLineUpdated = null;

                previ = i;

                //restrictedSnugFitAvailable[i].Item2.PinSubEventsToStart(new TimeLine(EarliestReferenceTIme, restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End));
                List<BusyTimeLine> RestrictedBusySlots = new System.Collections.Generic.List<BusyTimeLine>();
                FreeBoundary = new TimeLine(FreeBoundary.Start, FreeBoundary.End);
                foreach (mTuple<bool, SubCalendarEvent> eachmTuple in restrictedSnugFitAvailable)
                {
                    eachmTuple.Item1 = true;
                    RestrictedBusySlots.Add(eachmTuple.Item2.ActiveSlot);
                    string timeSpanString = eachmTuple.Item2.getActiveDuration.Ticks.ToString();
                    string SubEventID = eachmTuple.Item2.getId;

                }
                FreeBoundary.AddBusySlots(RestrictedBusySlots.ToArray());

                List<SubCalendarEvent> LowestCostArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
                TimeLine PertinentFreeSpot = null;
                TimeLine FreeSpotUpdated = null;
                j = i + 1;
                if (ListOfFrontPartialsStartTime.Count > 0)//fits any sub calEvent in preceeding restricting free spot
                {
                    DateTimeOffset RestrictedStopper = restrictedSnugFitAvailable[i].Item2.Start;
                    bool breakForLoop = false;
                    bool PreserveRestrictedIndex = false;
                    for (; FrontPartialCounter < ListOfFrontPartialsStartTime.Count; FrontPartialCounter++)//for loop tries to prioritize the front partial elements as the boundary of the calculation of fittable elements.
                    {
                        TimeLineUpdated = null;
                        DateTimeOffset PertinentFreeSpotStart = EarliestReferenceTIme;
                        DateTimeOffset PertinentFreeSpotEnd;

                        if ((ListOfFrontPartialsStartTime[FrontPartialCounter] < RestrictedStopper))
                        {
                            PertinentFreeSpotEnd = ListOfFrontPartialsStartTime[FrontPartialCounter];
                            ListOfFrontPartialsStartTime.RemoveAt(FrontPartialCounter);
                            --FrontPartialCounter;
                            PreserveRestrictedIndex = true;
                        }
                        else
                        {
                            PertinentFreeSpotEnd = RestrictedStopper;

                            if (breakForLoop)
                            {//this allows for the population or insertion of the restrictedSnugFitAvailable[i]. Within the enclosing for loop the restrictedSnugFitAvailable[i] never gets appended until this point. 
                                PertinentFreeSpot = new TimeLine(PertinentFreeSpotStart, PertinentFreeSpotEnd);

                                BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                                BorderElementEnd = restrictedSnugFitAvailable[i].Item2;//uses restricted value as boundary element


                                restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
                                for (int q = i; q < restrictedSnugFitAvailable.Count; q++)
                                {
                                    restOfrestrictedSnugFitAvailable.Add(restrictedSnugFitAvailable[q]);
                                }

                                selectedRestrictedElements = LowestCostArrangement.Intersect(restOfrestrictedSnugFitAvailable.Select(obj => obj.Item2));
                                if (selectedRestrictedElements.Count() > 0)
                                {
                                    if (selectedRestrictedElements.Contains(restrictedSnugFitAvailable[i].Item2))
                                    {
                                        ignorePlaceRestrictedinBestPosition = true;//forces the call to PlaceSubCalEventInLowestCostPosition to be ignored. THis is needed if one of the elements in selected restricted elements is the current restrictedSnugFitAvailable
                                    }
                                    restrictedSnugFitAvailable.RemoveAll(obj => selectedRestrictedElements.Contains(obj.Item2));
                                }

                                LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restOfrestrictedSnugFitAvailable, Occupancy);





                                //LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restrictedSnugFitAvailable,Occupancy);
                                DateTimeOffset EarliestTimeForBetterEarlierReferenceTime = PertinentFreeSpot.Start;
                                LastSubCalElementForEarlierReferenceTime = ((CompleteArranegement.Count < 1) || (CompleteArranegement == null) ? null : CompleteArranegement[CompleteArranegement.Count - 1]);
                                FreeSpotUpdated = PertinentFreeSpot.CreateCopy();
                                if (LowestCostArrangement.Count > 0)
                                {
                                    if (!(LowestCostArrangement[0].getCalculationRange.Start == PertinentFreeSpot.Start))//Pin SubEvents To Start
                                    {//if the first element is not a partial Sub Cal Event element
                                        FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                                        Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                                    }
                                    else
                                    {
                                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();// new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                                        Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot);
                                    }
                                    EarliestReferenceTIme = PertinentFreeSpot.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;

                                    SubCalendarEvent LastSubCalEvent = LowestCostArrangement[LowestCostArrangement.Count - 1];
                                    EarliestTimeForBetterEarlierReferenceTime = LastSubCalEvent.End;
                                    LastSubCalElementForEarlierReferenceTime = LastSubCalEvent;

                                }
                                TimeLineUpdated = null;
                                /*TimeLineUpdated = ObtainBetterEarlierReferenceTime(LowestCostArrangement, CalendarIDAndNonPartialSubCalEvents, RestrictedStopper - EarliestTimeForBetterEarlierReferenceTime, EarliestReferenceTIme, new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End), LastSubCalElementForEarlierReferenceTime);
                                if (TimeLineUpdated != null)
                                {
                                    LowestCostArrangement = TimeLineUpdated.Item2;
                                    EarliestReferenceTIme = TimeLineUpdated.Item1;
                                }
                                */

                                foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                                {
                                    if (!selectedRestrictedElements.Contains(eachSubCalendarEvent))
                                    {
                                        --CompatibleWithList[eachSubCalendarEvent.getActiveDuration].Item1;
                                        PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Remove(eachSubCalendarEvent.getId);
                                        string SubCalString = eachSubCalendarEvent.SubEvent_ID.getCalendarEventComponent();
                                        if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                                        {
                                            CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.getId);
                                            if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                                            {
                                                CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                                            }
                                        }
                                        if (PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Count < 1)
                                        {
                                            PossibleEntries_Cpy.Remove(eachSubCalendarEvent.getActiveDuration);
                                        }
                                    }
                                }


                                LowestCostArrangement = CompleteArranegement.Concat(LowestCostArrangement).ToList();
                                if (!ignorePlaceRestrictedinBestPosition)
                                {
                                    LowestCostArrangement = PlaceSubCalEventInLowestCostPosition(FreeBoundary, restrictedSnugFitAvailable[i].Item2, LowestCostArrangement);
                                }

                                ignorePlaceRestrictedinBestPosition = false;
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeBoundary);

                                CompleteArranegement = LowestCostArrangement;
                                EarliestReferenceTIme = LowestCostArrangement[LowestCostArrangement.Count - 1].End;

                                PreserveRestrictedIndex = false;
                                break;
                            }


                            /*restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
                            for (int q = i+1; q < restrictedSnugFitAvailable.Count; q++)
                            {
                                restOfrestrictedSnugFitAvailable.Add(restrictedSnugFitAvailable[q]);
                            }*/


                            //if (restOfrestrictedSnugFitAvailable.Count > 0)
                            {
                                //Utility.PinSubEventsToEnd(restOfrestrictedSnugFitAvailable.Select(obj=>obj.Item2).ToList(), FreeBoundary);
                                --FrontPartialCounter;
                                if (j < restrictedSnugFitAvailable.Count)
                                {
                                    RestrictedStopper = restrictedSnugFitAvailable[i].Item2.getCalculationRange.End > restrictedSnugFitAvailable[j].Item2.Start ? restrictedSnugFitAvailable[j].Item2.Start : restrictedSnugFitAvailable[i].Item2.getCalculationRange.End;
                                }
                                else
                                {
                                    RestrictedStopper = restrictedSnugFitAvailable[i].Item2.getCalculationRange.End > FreeBoundary.End ? FreeBoundary.End : restrictedSnugFitAvailable[i].Item2.getCalculationRange.End;
                                }
                                RestrictedStopper -= restrictedSnugFitAvailable[i].Item2.getActiveDuration;
                                breakForLoop = true;
                            }
                        }
                        PertinentFreeSpot = new TimeLine(PertinentFreeSpotStart, PertinentFreeSpotEnd);

                        BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                        BorderElementEnd = restrictedSnugFitAvailable[i].Item2;//uses restricted value as boundary element
                        restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
                        for (int q = i; q < restrictedSnugFitAvailable.Count; q++)
                        {
                            restOfrestrictedSnugFitAvailable.Add(restrictedSnugFitAvailable[q]);
                        }



                        LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restOfrestrictedSnugFitAvailable, Occupancy);

                        selectedRestrictedElements = LowestCostArrangement.Intersect(restOfrestrictedSnugFitAvailable.Select(obj => obj.Item2));
                        if (selectedRestrictedElements.Count() > 0)
                        {

                        }














                        LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restrictedSnugFitAvailable, Occupancy);
                        DateTimeOffset LatestDaterforEarlierReferenceTime = PertinentFreeSpot.Start;
                        LastSubCalElementForEarlierReferenceTime = ((CompleteArranegement.Count < 1) || (CompleteArranegement == null) ? null : CompleteArranegement[CompleteArranegement.Count - 1]);//updates the last element as either null or the last element in the current Complete arrangement
                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();
                        if (LowestCostArrangement.Count > 0)
                        {
                            if (!(LowestCostArrangement[0].getCalculationRange.Start == PertinentFreeSpot.Start))//Pin SubEvents To Start
                            {//if the first element is not a partial Sub Cal Event element
                                FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);

                            }
                            else
                            {
                                //FreeSpotUpdated = new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                                FreeSpotUpdated = PertinentFreeSpot.CreateCopy();
                                Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot);
                            }
                            EarliestReferenceTIme = PertinentFreeSpot.End;

                            ///Comeback to this
                            ///
                            SubCalendarEvent LastSubCalEvent = LowestCostArrangement[LowestCostArrangement.Count - 1];
                            LatestDaterforEarlierReferenceTime = LastSubCalEvent.End;
                            LastSubCalElementForEarlierReferenceTime = LastSubCalEvent;
                        }

                        /*TimeLineUpdated = ObtainBetterEarlierReferenceTime(LowestCostArrangement, CalendarIDAndNonPartialSubCalEvents, RestrictedStopper - LatestDaterforEarlierReferenceTime, EarliestReferenceTIme, new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End), LastSubCalElementForEarlierReferenceTime);
                        //errorline

                        if (TimeLineUpdated != null)
                        {
                            LowestCostArrangement = TimeLineUpdated.Item2;
                            EarliestReferenceTIme = TimeLineUpdated.Item1;
                        }

                        */
                        foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                        {
                            --CompatibleWithList[eachSubCalendarEvent.getActiveDuration].Item1;
                            PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Remove(eachSubCalendarEvent.getId);
                            string SubCalString = eachSubCalendarEvent.SubEvent_ID.getCalendarEventComponent();
                            if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                            {
                                CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.getId);
                                if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                                {
                                    CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                                }
                            }
                            if (PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Count < 1)
                            {
                                PossibleEntries_Cpy.Remove(eachSubCalendarEvent.getActiveDuration);
                            }
                        }
                        CompleteArranegement.AddRange(LowestCostArrangement);


                        int DateTimeCounter = 0;
                        for (; DateTimeCounter < FrontPartials_Dict.Keys.Count; DateTimeCounter++)//updates CalendarIDAndNonPartialSubCalEvents if frontpartial Startime has been passed. Alls updates FrontPartials_Dict
                        {
                            DateTimeOffset eachDateTIme = FrontPartials_Dict.Keys.ToList()[DateTimeCounter];
                            if (EarliestReferenceTIme >= eachDateTIme)
                            {
                                List<mTuple<bool, SubCalendarEvent>> mTUpleSubCalEvents = FrontPartials_Dict[eachDateTIme];
                                foreach (mTuple<bool, SubCalendarEvent> eachmTUple in mTUpleSubCalEvents)
                                {

                                    string CalLevel0ID = eachmTUple.Item2.SubEvent_ID.getCalendarEventComponent();
                                    if (!CompleteArranegement.Contains(eachmTUple.Item2))
                                    {
                                        if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(CalLevel0ID))
                                        {
                                            CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(eachmTUple.Item2.getId, eachmTUple.Item2);
                                        }
                                        else
                                        {
                                            //CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new List<SubCalendarEvent>() { KeyValuePair0.Value.Item2 });
                                            CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new Dictionary<string, SubCalendarEvent>());
                                            CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(eachmTUple.Item2.getId, eachmTUple.Item2);
                                        }
                                    }
                                }
                                FrontPartials_Dict.Remove(eachDateTIme);
                            }
                        }


                    }
                    if (PreserveRestrictedIndex)//verifies if we took the path of restricted or front partial element. The latter needs a preservation of the current restricted Subcalevent index index 
                    {
                        --i;
                    }
                }
                else
                {//No FrontPartials
                    DateTimeOffset ReferenceEndTime = restrictedSnugFitAvailable[i].Item2.Start;
                    PertinentFreeSpot = new TimeLine(EarliestReferenceTIme, ReferenceEndTime);

                    BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                    BorderElementEnd = restrictedSnugFitAvailable[i].Item2;//uses restricted value as boundary element

                    restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
                    for (int q = i; q < restrictedSnugFitAvailable.Count; q++)
                    {
                        restOfrestrictedSnugFitAvailable.Add(restrictedSnugFitAvailable[q]);
                    }



                    LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restOfrestrictedSnugFitAvailable, Occupancy);

                    selectedRestrictedElements = LowestCostArrangement.Intersect(restOfrestrictedSnugFitAvailable.Select(obj => obj.Item2));
                    if (selectedRestrictedElements.Count() > 0)
                    {
                        ;
                    }


                    if (LowestCostArrangement.Count > 0)
                    {
                        if (!(LowestCostArrangement[0].getCalculationRange.Start == PertinentFreeSpot.Start))//Pin SubEvents To Start
                        {//if the first element is not a partial Sub Cal Event element
                            FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                            Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                        }
                        else
                        {
                            FreeSpotUpdated = new TimeLine(LowestCostArrangement[0].getCalculationRange.Start, PertinentFreeSpot.End);
                            Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot);
                        }
                        EarliestReferenceTIme = FreeSpotUpdated.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                    }

                    foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                    {
                        --CompatibleWithList[eachSubCalendarEvent.getActiveDuration].Item1;
                        PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Remove(eachSubCalendarEvent.getId);
                        string SubCalString = eachSubCalendarEvent.SubEvent_ID.getCalendarEventComponent();
                        if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                        {
                            CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.getId);
                            if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                            {
                                CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                            }
                        }
                        if (PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Count < 1)
                        {
                            PossibleEntries_Cpy.Remove(eachSubCalendarEvent.getActiveDuration);
                        }
                    }


                    List<SubCalendarEvent> AdditionalCOstArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
                    DateTimeOffset RelativeEndTime;
                    if (j < restrictedSnugFitAvailable.Count)
                    {
                        //DateTimeOffset StartDateTimeAfterFitting = PertinentFreeSpot.End;
                        DateTimeOffset StartDateTimeAfterFitting = EarliestReferenceTIme;//this is the barring end time of the preceding boundary search. Earliest would have been updated if there was some event detected.


                        RelativeEndTime = restrictedSnugFitAvailable[i].Item2.getCalculationRange.End > restrictedSnugFitAvailable[j].Item2.Start ? restrictedSnugFitAvailable[j].Item2.Start : restrictedSnugFitAvailable[i].Item2.getCalculationRange.End;

                        RelativeEndTime -= restrictedSnugFitAvailable[i].Item2.getActiveDuration;
                        TimeLine CurrentlyFittedTimeLine = new TimeLine(StartDateTimeAfterFitting, RelativeEndTime);

                        BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                        BorderElementEnd = restrictedSnugFitAvailable[i].Item2;//uses restricted value as boundary element


                        restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
                        for (int q = i; q < restrictedSnugFitAvailable.Count; q++)
                        {
                            restOfrestrictedSnugFitAvailable.Add(restrictedSnugFitAvailable[q]);
                        }



                        AdditionalCOstArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(CurrentlyFittedTimeLine, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restOfrestrictedSnugFitAvailable, Occupancy);

                        selectedRestrictedElements = AdditionalCOstArrangement.Intersect(restOfrestrictedSnugFitAvailable.Select(obj => obj.Item2));
                        if (selectedRestrictedElements.Count() > 0)
                        {
                            ;
                        }



                        AdditionalCOstArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(CurrentlyFittedTimeLine, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restrictedSnugFitAvailable, Occupancy);
                        if (AdditionalCOstArrangement.Count > 0)
                        {//Additional get populated
                            if (!(AdditionalCOstArrangement[0].getCalculationRange.Start == CurrentlyFittedTimeLine.Start))//Pin SubEvents To Start
                            {//if the first element is not a partial Sub Cal Event element
                                FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, CurrentlyFittedTimeLine.End);
                                Utility.PinSubEventsToStart(AdditionalCOstArrangement, FreeSpotUpdated);
                            }
                            else
                            {
                                FreeSpotUpdated = new TimeLine(AdditionalCOstArrangement[0].getCalculationRange.Start, CurrentlyFittedTimeLine.End);
                                Utility.PinSubEventsToStart(AdditionalCOstArrangement, FreeSpotUpdated);
                            }

                            foreach (SubCalendarEvent eachSubCalendarEvent in AdditionalCOstArrangement)
                            {
                                --CompatibleWithList[eachSubCalendarEvent.getActiveDuration].Item1;
                                PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Remove(eachSubCalendarEvent.getId);
                                string SubCalString = eachSubCalendarEvent.SubEvent_ID.getCalendarEventComponent();
                                if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                                {
                                    CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.getId);
                                    if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                                    {
                                        CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                                    }
                                }
                                if (PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Count < 1)
                                {
                                    PossibleEntries_Cpy.Remove(eachSubCalendarEvent.getActiveDuration);
                                }
                            }


                            RelativeEndTime = AdditionalCOstArrangement[AdditionalCOstArrangement.Count - 1].End;
                            RelativeEndTime += restrictedSnugFitAvailable[i].Item2.getActiveDuration; ;
                            CurrentlyFittedTimeLine = new TimeLine(FreeSpotUpdated.Start, RelativeEndTime);
                            //AdditionalCOstArrangement = PlaceSubCalEventInLowestCostPosition(CurrentlyFittedTimeLine, restrictedSnugFitAvailable[i].Item2, AdditionalCOstArrangement);
                        }
                        else
                        {//if there is no other Restricted in list
                            RelativeEndTime += restrictedSnugFitAvailable[i].Item2.getActiveDuration;
                            CurrentlyFittedTimeLine = new TimeLine(CurrentlyFittedTimeLine.Start, RelativeEndTime);
                            //AdditionalCOstArrangement = PlaceSubCalEventInLowestCostPosition(CurrentlyFittedTimeLine, restrictedSnugFitAvailable[i].Item2, AdditionalCOstArrangement);
                        }
                    }
                    else
                    {
                        RelativeEndTime = restrictedSnugFitAvailable[i].Item2.getCalculationRange.End > FreeBoundary.End ? FreeBoundary.End : restrictedSnugFitAvailable[i].Item2.getCalculationRange.End;
                        TimeLine CurrentlyFittedTimeLine = new TimeLine(EarliestReferenceTIme, RelativeEndTime);
                        //AdditionalCOstArrangement = PlaceSubCalEventInLowestCostPosition(CurrentlyFittedTimeLine, restrictedSnugFitAvailable[i].Item2, AdditionalCOstArrangement);
                    }

                    CompleteArranegement.AddRange(LowestCostArrangement);
                    CompleteArranegement.AddRange(AdditionalCOstArrangement);
                    TimeLine encasingTimeLine = new TimeLine(FreeBoundary.Start, RelativeEndTime);
                    CompleteArranegement = PlaceSubCalEventInLowestCostPosition(encasingTimeLine, restrictedSnugFitAvailable[i].Item2, CompleteArranegement);
                    if (CompleteArranegement.Count > 0)
                    {
                        EarliestReferenceTIme = CompleteArranegement[CompleteArranegement.Count - 1].End;
                    }
                }
            }


            { //Handles THe Last Free Space outside of rigids
                TimeLine FreeSpotOutSideRigids = new TimeLine(EarliestReferenceTIme, FreeBoundary.End);
                TimeLine PertinentFreeSpot = new TimeLine(EarliestReferenceTIme, FreeBoundary.End); ;
                TimeLine FreeSpotUpdated;
                List<SubCalendarEvent> LowestCostArrangement;
                if (ListOfFrontPartialsStartTime.Count > 0)
                {
                    for (FrontPartialCounter = 0; FrontPartialCounter < ListOfFrontPartialsStartTime.Count; FrontPartialCounter++)
                    {
                        DateTimeOffset PertinentFreeSpotStart = EarliestReferenceTIme;
                        DateTimeOffset PertinentFreeSpotEnd;
                        PertinentFreeSpotEnd = ListOfFrontPartialsStartTime[FrontPartialCounter];
                        //FrontPartials_Dict.Remove(ListOfFrontPartialsStartTime[FrontPartialCounter]);
                        ListOfFrontPartialsStartTime.RemoveAt(FrontPartialCounter);
                        --FrontPartialCounter;
                        PertinentFreeSpot = new TimeLine(PertinentFreeSpotStart, PertinentFreeSpotEnd);
                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();

                        BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                        BorderElementEnd = null;


                        restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
                        for (int q = i; q < restrictedSnugFitAvailable.Count; q++)
                        {
                            restOfrestrictedSnugFitAvailable.Add(restrictedSnugFitAvailable[q]);
                        }



                        LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restOfrestrictedSnugFitAvailable, Occupancy);

                        selectedRestrictedElements = LowestCostArrangement.Intersect(restOfrestrictedSnugFitAvailable.Select(obj => obj.Item2));
                        if (selectedRestrictedElements.Count() > 0)
                        {
                            ;
                        }







                        LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restrictedSnugFitAvailable, Occupancy);
                        DateTimeOffset LatestTimeForBetterEarlierReferenceTime = PertinentFreeSpot.Start;
                        LastSubCalElementForEarlierReferenceTime = ((CompleteArranegement.Count < 1) || (CompleteArranegement == null) ? null : CompleteArranegement[CompleteArranegement.Count - 1]);
                        if (LowestCostArrangement.Count > 0)
                        {
                            if ((LowestCostArrangement[0].getCalculationRange.Start != PertinentFreeSpot.Start))//Pin SubEvents To Start
                            {//if the first element is not a partial Sub Cal Event element
                                FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);

                            }
                            else
                            {
                                FreeSpotUpdated = PertinentFreeSpot.CreateCopy();// new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                            }
                            EarliestReferenceTIme = PertinentFreeSpot.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                            SubCalendarEvent LastSubCalEvent = LowestCostArrangement[LowestCostArrangement.Count - 1];
                            LatestTimeForBetterEarlierReferenceTime = LastSubCalEvent.End;
                            LastSubCalElementForEarlierReferenceTime = LastSubCalEvent;
                            /*
                            Dictionary<string, double> AllValidNodes = CalendarEvent.DistanceToAllNodes(LastSubCalEvent.SubEvent_ID.getCalendarEventComponent());
                            SubCalendarEvent AppendableEVent;
                            foreach (string eachstring in AllValidNodes.Keys)
                            {
                                if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(eachstring))
                                {
                                    AppendableEVent = CalendarIDAndNonPartialSubCalEvents[eachstring].ToList()[0].Value;//Assumes Theres Always an element
                                    

                                    if ((AppendableEVent.ActiveDuration <= (FreeBoundary.End - LastSubCalEvent.End)) && (!LowestCostArrangement.Contains(AppendableEVent)))
                                    {
                                        LowestCostArrangement.Add(AppendableEVent);
                                        CalendarIDAndNonPartialSubCalEvents[eachstring].Remove(AppendableEVent.ID);
                                        if (CalendarIDAndNonPartialSubCalEvents[eachstring].Count < 1)//checks if List is empty. Deletes keyValuepair if list is empty
                                        {
                                            CalendarIDAndNonPartialSubCalEvents.Remove(eachstring);
                                        }
                                        FreeSpotUpdated = new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End);
                                        Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                                        EarliestReferenceTIme = AppendableEVent.End;
                                        break;
                                    }
                                }
                            }*/
                        }


                        TimeLineUpdated = null;
                        /*TimeLineUpdated = ObtainBetterEarlierReferenceTime(LowestCostArrangement, CalendarIDAndNonPartialSubCalEvents, FreeBoundary.End - LatestTimeForBetterEarlierReferenceTime, EarliestReferenceTIme, new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End), LastSubCalElementForEarlierReferenceTime);
                        if (TimeLineUpdated != null)
                        {
                            LowestCostArrangement = TimeLineUpdated.Item2;
                            EarliestReferenceTIme = TimeLineUpdated.Item1;
                        }
                        */
                        foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                        {
                            --CompatibleWithList[eachSubCalendarEvent.getActiveDuration].Item1;
                            PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Remove(eachSubCalendarEvent.getId);
                            string SubCalString = eachSubCalendarEvent.SubEvent_ID.getCalendarEventComponent();
                            if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                            {
                                CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.getId);
                                if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                                {
                                    CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                                }
                            }
                            if (PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Count < 1)
                            {
                                PossibleEntries_Cpy.Remove(eachSubCalendarEvent.getActiveDuration);
                            }
                        }
                        CompleteArranegement.AddRange(LowestCostArrangement);
                    }
                }


                DateTimeOffset ReferenceEndTime = FreeBoundary.End;
                PertinentFreeSpot = new TimeLine(EarliestReferenceTIme, ReferenceEndTime);
                /*LowestCostArrangement = OptimizeArrangeOfSubCalEvent(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(null, null), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy);

                if (LowestCostArrangement.Count > 0)
                {
                    if (!(LowestCostArrangement[0].getCalendarEventRange.Start == PertinentFreeSpot.Start))//Pin SubEvents To Start
                    {//if the first element is not a partial Sub Cal Event element
                        FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                        Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                    }
                    else
                    {
                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();// new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                        Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                    }
                    EarliestReferenceTIme = FreeSpotUpdated.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                }*/
                BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                BorderElementEnd = null;




                restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
                for (int q = i; q < restrictedSnugFitAvailable.Count; q++)
                {
                    restOfrestrictedSnugFitAvailable.Add(restrictedSnugFitAvailable[q]);
                }



                LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restOfrestrictedSnugFitAvailable, Occupancy);

                selectedRestrictedElements = LowestCostArrangement.Intersect(restOfrestrictedSnugFitAvailable.Select(obj => obj.Item2));
                if (selectedRestrictedElements.Count() > 0)
                {
                    ;
                }














                LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restrictedSnugFitAvailable, Occupancy);
                LastSubCalElementForEarlierReferenceTime = ((CompleteArranegement.Count < 1) || (CompleteArranegement == null) ? null : CompleteArranegement[CompleteArranegement.Count - 1]);
                DateTimeOffset LimitForBetterEarlierReferencTime = EarliestReferenceTIme;
                FreeSpotUpdated = PertinentFreeSpot.CreateCopy();
                if (LowestCostArrangement.Count > 0)
                {
                    if ((LowestCostArrangement[0].getCalculationRange.Start != PertinentFreeSpot.Start))//Pin SubEvents To Start
                    {//if the first element is not a partial Sub Cal Event element
                        FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                        Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);

                    }
                    else
                    {
                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();// new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                        Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot);
                    }
                    EarliestReferenceTIme = PertinentFreeSpot.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                    SubCalendarEvent LastSubCalEvent = LowestCostArrangement[LowestCostArrangement.Count - 1];
                    LimitForBetterEarlierReferencTime = LastSubCalEvent.End;
                    LastSubCalElementForEarlierReferenceTime = LastSubCalEvent;



                    /*
                    
                    
                    Dictionary<string, double> AllValidNodes = CalendarEvent.DistanceToAllNodes(LastSubCalEvent.SubEvent_ID.getCalendarEventComponent());
                    SubCalendarEvent AppendableEVent;
                    foreach (string eachstring in AllValidNodes.Keys)
                    {
                        if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(eachstring))
                        {
                            AppendableEVent = CalendarIDAndNonPartialSubCalEvents[eachstring].ToList()[0].Value;//Assumes Theres Always an element

                            if ((AppendableEVent.ActiveDuration <= (FreeBoundary.End - LastSubCalEvent.End)) && (!LowestCostArrangement.Contains(AppendableEVent)))
                            {
                                LowestCostArrangement.Add(AppendableEVent);
                                CalendarIDAndNonPartialSubCalEvents[eachstring].Remove(AppendableEVent.ID);
                                if (CalendarIDAndNonPartialSubCalEvents[eachstring].Count < 1)//checks if List is empty. Deletes keyValuepair if list is empty
                                {
                                    CalendarIDAndNonPartialSubCalEvents.Remove(eachstring);
                                }
                                FreeSpotUpdated = new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End);
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                                EarliestReferenceTIme = AppendableEVent.End;
                                break;
                            }
                        }
                    }*/

                }
                TimeLineUpdated = null;
                /*TimeLineUpdated = ObtainBetterEarlierReferenceTime(LowestCostArrangement, CalendarIDAndNonPartialSubCalEvents, FreeBoundary.End - LimitForBetterEarlierReferencTime, EarliestReferenceTIme, new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End), LastSubCalElementForEarlierReferenceTime);
                if (TimeLineUpdated != null)
                {
                    LowestCostArrangement = TimeLineUpdated.Item2;
                    EarliestReferenceTIme = TimeLineUpdated.Item1;
                }
                */

                foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                {
                    --CompatibleWithList[eachSubCalendarEvent.getActiveDuration].Item1;
                    PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Remove(eachSubCalendarEvent.getId);
                    string SubCalString = eachSubCalendarEvent.SubEvent_ID.getCalendarEventComponent();
                    if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                    {
                        CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.getId);
                        if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                        {
                            CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                        }
                    }

                    if (PossibleEntries_Cpy[eachSubCalendarEvent.getActiveDuration].Count < 1)
                    {
                        PossibleEntries_Cpy.Remove(eachSubCalendarEvent.getActiveDuration);
                    }
                }
                CompleteArranegement.AddRange(LowestCostArrangement);

            }






            List<mTuple<bool, SubCalendarEvent>> retValue = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();

            foreach (SubCalendarEvent eachSubCalendarEvent in CompleteArranegement)
            {
                PossibleEntries[eachSubCalendarEvent.getActiveDuration][eachSubCalendarEvent.getId].Item1 = true;
                retValue.Add(PossibleEntries[eachSubCalendarEvent.getActiveDuration][eachSubCalendarEvent.getId]);
            }

            //List<List<SubCalendarEvent>> unrestrictedValidCombinations = generateCombinationForDifferentEntries(CompatibleWithList, PossibleEntries);

            retValue = reAlignSubCalEvents(FreeBoundary, retValue);
            if (TotalEventsForThisTImeLine != retValue.Count)
            {
                ;
            }
            return retValue;

        }




        List<List<SubCalendarEvent>> generateCombinationForDifferentEntriesNonAggressive(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithList, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries)
        {
            /*
             * Function attempts to generate multiple combinations of compatible sub calendar event for Snug fit entry
             * CompatibleWithList is an snug fit result
             * PossibleEntries are the possible sub calendar that can be used in the combinatorial result
             */
            List<List<List<string>>> MAtrixedSet = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<string>>>();
            Dictionary<string, mTuple<int, List<SubCalendarEvent>>> var4 = new System.Collections.Generic.Dictionary<string, mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>>();
            List<List<SubCalendarEvent>> retValue = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair0 in CompatibleWithList)//loops every timespan in Snug FIt possibility
            {
                TimeSpan eachTimeSpan = eachKeyValuePair0.Key;

                Dictionary<string, mTuple<bool, SubCalendarEvent>> var1 = PossibleEntries[eachTimeSpan];
                List<List<string>> receivedValue = new System.Collections.Generic.List<System.Collections.Generic.List<string>>();
                Dictionary<string, int> var2 = new System.Collections.Generic.Dictionary<string, int>();
                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> eachKeyValuePair in var1)
                {
                    string ParentID = eachKeyValuePair.Value.Item2.SubEvent_ID.getIDUpToCalendarEvent();
                    if (var2.ContainsKey(ParentID))
                    {
                        ++var2[ParentID];
                        var4[ParentID].Item2.Add(eachKeyValuePair.Value.Item2);
                    }
                    else
                    {
                        var2.Add(ParentID, 1);
                        List<SubCalendarEvent> var5 = new System.Collections.Generic.List<SubCalendarEvent>();
                        var5.Add(eachKeyValuePair.Value.Item2);
                        var4.Add(ParentID, new mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>(0, var5));
                    }
                }
                List<mTuple<string, int>> PossibleCalEvents = new System.Collections.Generic.List<mTuple<string, int>>();
                foreach (KeyValuePair<string, int> eachKeyValuePair in var2)
                {
                    PossibleCalEvents.Add(new mTuple<string, int>(eachKeyValuePair.Key, eachKeyValuePair.Value));
                }

                List<List<string>> var3 = generateCombinationForSpecficTimeSpanStringID(eachKeyValuePair0.Value.Item1, PossibleCalEvents);

                if (var3.Count > 1)
                {
                    var3 = var3.Where(obj => obj.Count == eachKeyValuePair0.Value.Item1).ToList();
                }

                MAtrixedSet.Add(var3);
            }

            List<List<string>> serializedList = Utility.SerializeList(MAtrixedSet);
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

                //var7.Item1 = 0;

                retValue.Add(var6);
            }

            return retValue;

        }


        public List<mTuple<bool, SubCalendarEvent>> stitchRestrictedSubCalendarEvent(List<mTuple<bool, SubCalendarEvent>> Arg1, TimeLine RestrictingTimeLine, SubCalendarEvent PrecedingPivot = null)
        {
            List<SubCalendarEvent> retValue = stitchRestrictedSubCalendarEvent(Arg1.Select(obj => new mTuple<double, mTuple<TimeLine, SubCalendarEvent>>(0, new mTuple<TimeLine, SubCalendarEvent>(new TimeLine(), obj.Item2))).ToList(), RestrictingTimeLine, PrecedingPivot);
            return retValue.Select(obj => new mTuple<bool, SubCalendarEvent>(true, obj)).ToList();
        }

        public List<SubCalendarEvent> stitchRestrictedSubCalendarEvent(List<mTuple<double, mTuple<TimeLine, SubCalendarEvent>>> Arg1, TimeLine RestrictingTimeLine, SubCalendarEvent PrecedingPivot = null)
        {
            List<SubCalendarEvent> retValue = Arg1.Select(obj => obj.Item2.Item2).ToList();

            TimeSpan SumOfAllSubCalEvent = Utility.SumOfActiveDuration(Arg1.Select(obj => obj.Item2.Item2));
            List<SubCalendarEvent> CopyOfAllList = Arg1.Select(obj => obj.Item2.Item2).ToList();
            if (retValue.Count < 1)//if arg1 is empty return the list
            {
                return retValue;
            }
            List<SubCalendarEvent> AllSubCalEvents = Arg1.Select(obj => obj.Item2.Item2).ToList();
            List<mTuple<TimeLine, SubCalendarEvent>> AvaialableTimeSpan = new List<mTuple<TimeLine, SubCalendarEvent>>();
            int indexOfSmallest = -2222;
            int i = 0;


            TimeLine InterferringTimeLine = RestrictingTimeLine.CreateCopy();
            TimeSpan SmallestAssignedTimeSpan = new TimeSpan(3650, 0, 0, 0);//sets the smallest TimeSpan To 10 years
            DateTimeOffset SmallestDateTime = new DateTimeOffset(3000, 12, 31, 0, 0, 0, new TimeSpan());


            Arg1 =Arg1.Select(obj => //using Linq to update timeline
            {
                List<TimeLine>AllTimeLines=obj.Item2.Item2.getTimeLineInterferringWithCalEvent(RestrictingTimeLine);
                TimeLine relevantTimeLine = AllTimeLines!=null?AllTimeLines[0]:null;
                obj.Item2.Item1 = relevantTimeLine;//gets interferring TImeLine
                obj.Item1 += (obj.Item1*10) + (obj.Item2.Item1 != null ? obj.Item2.Item1.TimelineSpan.TotalSeconds / obj.Item2.Item2.getActiveDuration.TotalSeconds : 0);
                return obj; 
            }).ToList();
            Arg1=Arg1.OrderBy(obj => obj.Item1).ToList();
            List<mTuple<double, mTuple<TimeLine, SubCalendarEvent>>> WorkableList = Arg1.Where(obj => obj.Item2.Item1 != null).ToList();
            WorkableList = WorkableList.Where(obj => obj.Item2.Item1.TimelineSpan >= obj.Item2.Item2.getActiveDuration).ToList();

#if reversed
            //Build Strict Towards right of the tree
            if (WorkableList.Count > 0)
            {
                //CountCall++;
                mTuple<double, mTuple<TimeLine, SubCalendarEvent>> PivotNodeData = WorkableList[0];
                bool PinningSuccess = PivotNodeData.Item2.Item2.PinToEnd (RestrictingTimeLine);
                DateTimeOffset StartTimeOfLeftTree = RestrictingTimeLine.Start;
                DateTimeOffset EndTimeOfLeftTree = RestrictingTimeLine.End;
                SubCalendarEvent includeSubCalendarEvent = null;
                TimeLine leftOver = new TimeLine();
                if (PinningSuccess)//hack alert Subevent fittable a double less than 1 e,g 0.5
                {
                    EndTimeOfLeftTree = PivotNodeData.Item2.Item2.Start;
                    includeSubCalendarEvent = PivotNodeData.Item2.Item2;
                    leftOver = new TimeLine(PivotNodeData.Item2.Item2.End, RestrictingTimeLine.End);//gets the left of timeline to ensure that 
                }

                WorkableList.RemoveAt(0);

                TimeLine LefTimeLine = new TimeLine(StartTimeOfLeftTree, EndTimeOfLeftTree);

                List<mTuple<double, mTuple<TimeLine, SubCalendarEvent>>> WorkableList_Cpy = WorkableList.ToList();

                List<SubCalendarEvent> willFitInleftOver = WorkableList.Where(obj => obj.Item2.Item2.canExistWithinTimeLine(leftOver)).Select(obj => obj.Item2.Item2).ToList();

                SubCalendarEvent.incrementMiscdata(willFitInleftOver);

                List<SubCalendarEvent> leftTreeResult = stitchRestrictedSubCalendarEvent(WorkableList, LefTimeLine);
                if (includeSubCalendarEvent != null)
                {
                    leftTreeResult.Add(includeSubCalendarEvent);
                }

                if (!Utility.PinSubEventsToStart(leftTreeResult, RestrictingTimeLine))
                {
                    throw new Exception("oops jerome check reveresed section of stitchrestricted left treeresult ");
                    ;
                };

                //Build Strict Towards Left of the tree
                DateTimeOffset StartTimeOfRightTree = RestrictingTimeLine.Start;
                DateTimeOffset EndTimeOfRightTree = RestrictingTimeLine.End;
                if (leftTreeResult.Count > 0)
                {
                    StartTimeOfRightTree = leftTreeResult.Last().End;
                }

                TimeLine RightTimeLine = new TimeLine(StartTimeOfRightTree, EndTimeOfRightTree);

                WorkableList_Cpy.RemoveAll(obj => leftTreeResult.Contains(obj.Item2.Item2));




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
            List<SubCalendarEvent> AllSubCall=arg1.Values.SelectMany(obj => obj.Select(obj1 => obj1.Item2)).ToList();
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


        List<SubCalendarEvent> getOPtimized(List<List<SubCalendarEvent>> Arg1)
        {
            List<SubCalendarEvent> retValue = new System.Collections.Generic.List<SubCalendarEvent>();

            if (Arg1.Count > 0)
            {
                return Arg1[0];
            }
            return retValue;
        }


        List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> getAveragedOutTIimeLine(List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> arg1, int PreecdintTimeSpanWithSringIDCoun)
        {
            /*
             * Function takes a list of valid possible matches. It uses this list of valid matches to calculate an average which will be used to calculate the best snug possibility
             * arg1= The List of possible snug time Lines
             */

            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> retValue = new List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();
            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> Total = new List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();
            /*if (arg1.Count<1)
            {
                return retValue;
            }*/
            foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in arg1[0])//initializes Total with the first element in the list
            {
                Total.Add(new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>());
            }



            int i = 0;
            int j = 0;

            for (; j < arg1.Count; j++)
            {
                List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> eachList = arg1[j];
                if (j == 30)
                {
                    ;
                }
                i = 0;
                foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in eachList)
                {

                    foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in eachDictionary)
                    {
                        if (Total[i].ContainsKey(eachKeyValuePair.Key))
                        {
                            Total[i][eachKeyValuePair.Key].Item1 += eachKeyValuePair.Value.Item1;
                        }
                        else
                        {
                            Total[i].Add(eachKeyValuePair.Key, new mTuple<int, TimeSpanWithStringID>(eachKeyValuePair.Value.Item1, eachKeyValuePair.Value.Item2));
                        }
                    }
                    ++i;
                }
            }

            Dictionary<TimeSpan, int> RoundUpRoundDown = new System.Collections.Generic.Dictionary<TimeSpan, int>();
            i = 0;
            foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in Total)//initializes Total with the first element in the list
            {

                retValue.Add(new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>());
                foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in eachDictionary)
                {
                    double arg0 = eachKeyValuePair.Value.Item1 / j;
                    int PassedToRet = (int)Math.Round(arg0);
                    string[] Int_Decimal = arg0.ToString().Split('.');
                    if (Int_Decimal.Length == 2)
                    {
                        if (Int_Decimal[1] == "5")
                        {
                            if (RoundUpRoundDown.ContainsKey(eachKeyValuePair.Key))
                            {
                                if (RoundUpRoundDown[eachKeyValuePair.Key] == 0)
                                {
                                    RoundUpRoundDown[eachKeyValuePair.Key] = 1;
                                }
                                else
                                {
                                    PassedToRet = (int)Math.Floor(arg0);
                                    RoundUpRoundDown[eachKeyValuePair.Key] = 0;
                                }
                            }
                            else
                            {
                                PassedToRet = (int)Math.Floor(arg0);
                                RoundUpRoundDown.Add(eachKeyValuePair.Key, 0);
                            }
                        }
                    }

                    if (PassedToRet > 0)
                    {
                        retValue[i].Add(eachKeyValuePair.Key, new mTuple<int, TimeSpanWithStringID>(PassedToRet, eachKeyValuePair.Value.Item2));
                    }
                }
                i++;
            }


            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> ListOfPossibleretValue = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();


            i = 0;
            foreach (List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> eachList in arg1)
            {
                i = 0;
                bool arg3 = false;
                foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in retValue)//initializes Total with the first element in the list
                {
                    Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> arg2 = eachList[i];
                    foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in eachDictionary)
                    {
                        if (arg2.ContainsKey(eachKeyValuePair.Key))
                        {
                            if (eachKeyValuePair.Value.Item1 <= arg2[eachKeyValuePair.Key].Item1)
                            {
                                arg3 = true;
                            }
                            else
                            {
                                arg3 = false;
                                break;
                            }
                        }
                    }

                    if (!arg3)
                    {
                        break;
                    }
                    i++;
                }

                if (arg3)
                {
                    ListOfPossibleretValue.Add(eachList);
                }


            }
            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> SpreadOutResult = getMostSpreadOut(ListOfPossibleretValue);
            Random myRandomizer = new Random();
            if (((SpreadOutResult.Count < 1) && (arg1.Count > 0)))//hack alert, this says if for some reason we have no generated possible ListOfPossibleretValue, we can pick one at random
            {
                return arg1[myRandomizer.Next(0, arg1.Count)];
            }
            ListOfPossibleretValue = SpreadOutResult;




            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> TempList = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
            TempList.Add(retValue);

            Tuple<int, List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>> arg4 = Utility.getHighestCountList(TempList, retValue);

            if (arg4.Item1 <= PreecdintTimeSpanWithSringIDCoun)
            {
                return retValue;
            }

            else
            {
                return getAveragedOutTIimeLine(ListOfPossibleretValue, arg4.Item1);
            }



        }


        List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> getMostSpreadOut(List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> arg1)
        {
            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> retValue = new List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
            int i = 0;

            int MaxCount = 0;
            foreach (List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> eachList in arg1)
            {
                i = 0;
                int CurrenCount = 0;

                foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in eachList)//initializes Total with the first element in the list
                {
                    CurrenCount += eachDictionary.Count;
                }

                if (CurrenCount >= MaxCount)
                {
                    if (CurrenCount > MaxCount)
                    {
                        retValue = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
                    }
                    retValue.Add(eachList);
                }

            }
            return retValue;
        }


        List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> getValidMatches(List<SubCalendarEvent> ListOfInterferringEvents, List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> PossibleMatches, Dictionary<TimeLine, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> ConstrainedList)
        {
            int MaxPossibleMatched = ListOfInterferringEvents.Count;
            int HighstSum = 0;
            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> retValue = new List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
            List<TimeLine> AllTimeLines = ConstrainedList.Keys.ToList();
            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> ListOfDict = ConstrainedList.Values.ToList();

            int j = 0;
            foreach (List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> eachList in PossibleMatches)
            {
                int CurrentSum = 0;
                int i = 0;

                foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in eachList)
                {
                    ICollection<KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AllData = eachDictionary;
                    foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in AllData)
                    {
                        int evpItem1 = 0;
                        if (ListOfDict[i].ContainsKey(eachKeyValuePair.Key))
                        {

                            int evpItem2 = ListOfDict[i][eachKeyValuePair.Key].Item1;
                            evpItem1 = eachKeyValuePair.Value.Item1 + evpItem2;
                        }
                        else
                        {
                            evpItem1 = eachKeyValuePair.Value.Item1;
                        }

                        evpItem1 = eachKeyValuePair.Value.Item1;
                        CurrentSum += evpItem1;
                    }
                    ++i;
                }

                if (CurrentSum >= HighstSum)
                {
                    if (CurrentSum > HighstSum)
                    {
                        retValue = new List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
                        //retValue.Add(eachList);
                    }
                    retValue.Add(eachList);
                    HighstSum = CurrentSum;
                }
                j++;
            }
            return retValue;
        }



        Tuple<List<TimeSpanWithStringID>, List<mTuple<bool, SubCalendarEvent>>> ConvertSubCalendarEventToTimeSpanWitStringID(List<SubCalendarEvent> Arg1)
        {
            List<TimeSpanWithStringID> retValue = new System.Collections.Generic.List<TimeSpanWithStringID>();
            List<mTuple<bool, SubCalendarEvent>> retValue0 = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            foreach (SubCalendarEvent eachSubCalendarEvent in Arg1)
            {
                retValue.Add(new TimeSpanWithStringID(eachSubCalendarEvent.getActiveDuration, eachSubCalendarEvent.getActiveDuration.Ticks.ToString()));
                retValue0.Add(new mTuple<bool, SubCalendarEvent>(false, eachSubCalendarEvent));
            }

            return new Tuple<List<TimeSpanWithStringID>, List<mTuple<bool, SubCalendarEvent>>>(retValue, retValue0);
        }


        int HighestSum = 0;
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

            //if (ListOfAllSnugPossibilitiesInRespectiveTImeLines_hack.Count >= maxHackConstant)
            {
                //return ListOfAllSnugPossibilitiesInRespectiveTImeLines_hack;
            }





            if (AvailableSubCalendarEvents.Count > MaxNumberOfInterferringSubcalEvents)
            {
                MaxNumberOfInterferringSubcalEvents = AvailableSubCalendarEvents.Count;
            }
            if (TimeLineIndex > LargestTimeIndex)
            {
                LargestTimeIndex = TimeLineIndex;
            }

            int indexChecker = 14;
            /*if (TimeLineIndex == indexChecker)
            {
                mTuple<bool, List<TimeSpanWithStringID>> statusOfError=Utility.isViolatingTimeSpanString(InterferringTimeSpanWithStringID_Cpy, SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar);

                if (statusOfError.Item1 )
                {
                    ;
                }
            }*/

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






        List<CalendarEvent> getClashingCalendarEvent(List<CalendarEvent> MyListOfCalednarEvents, TimeLine MyTImeLine)
        {
            List<CalendarEvent> MyInterferringCaliendarEvents = getPertinentCalendarEvents(MyListOfCalednarEvents.ToArray(), MyTImeLine).ToList();
            List<CalendarEvent> ListOfCalendarEventsLargerThanTimeLine = new System.Collections.Generic.List<CalendarEvent>();

            foreach (CalendarEvent MyCalendarEvent in MyListOfCalednarEvents)
            {
                if (((MyCalendarEvent.End > MyTImeLine.Start) && (MyCalendarEvent.Start <= MyTImeLine.Start)) || ((MyTImeLine.Start > MyCalendarEvent.Start) && (MyTImeLine.Start <= MyCalendarEvent.Start)))
                {
                    MyInterferringCaliendarEvents.Add(MyCalendarEvent);
                }
            }

            return MyInterferringCaliendarEvents;
        }


        mTuple<bool, SubCalendarEvent> getmTupleSubCalendarEvent(List<mTuple<bool, SubCalendarEvent>> MyListOfInterferringmTupleSubCalendarEvents, SubCalendarEvent Arg1)
        {
            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in MyListOfInterferringmTupleSubCalendarEvents)
            {
                if (eachmTuple.Item2 == Arg1)
                {
                    return eachmTuple;
                }
            }

            return null;
        }


        List<mTuple<bool, SubCalendarEvent>> getmTupleSubCalendarEvent(List<mTuple<bool, SubCalendarEvent>> MyListOfInterferringmTupleSubCalendarEvents, List<SubCalendarEvent> Arg1)
        {
            List<mTuple<bool, SubCalendarEvent>> retValue = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            int i = 0;
            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in MyListOfInterferringmTupleSubCalendarEvents)
            {
                int Arg2 = Arg1.IndexOf(eachmTuple.Item2);
                if (Arg2 > -1)
                {
                    retValue.Add(eachmTuple);
                    if (++i == Arg1.Count)
                    {
                        return retValue;
                    }
                }
            }

            return null;
        }

        private Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> BuildDicitionaryOfTimeLineAndSubcalendarEvents(List<mTuple<bool, SubCalendarEvent>> MyListOfInterferringmTupleSubCalendarEvents, Dictionary<TimeLine, Dictionary<CalendarEvent, List<SubCalendarEvent>>> DicitonaryTimeLineAndPertinentCalendarEventDictionary, CalendarEvent MyEvent)
        {


            List<TimeLine> MyListOfFreeTimelines = DicitonaryTimeLineAndPertinentCalendarEventDictionary.Keys.ToList();
            Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> DictionaryofTimeLineAndPertinentSubcalendar = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();


            foreach (TimeLine MyTimeLine in MyListOfFreeTimelines)
            {
                //Dictionary<TimeLine, Dictionary<CalendarEvent, List<SubCalendarEvent>>> DicitonaryTimeLineAndPertinentCalendarEventDictionary1;

                Dictionary<CalendarEvent, List<SubCalendarEvent>> MyListOfDictionaryOfCalendarEventAndSubCalendarEvent = DicitonaryTimeLineAndPertinentCalendarEventDictionary[MyTimeLine];
                List<CalendarEvent> MyListOfPertitnentCalendars = MyListOfDictionaryOfCalendarEventAndSubCalendarEvent.Keys.ToList();
                MyListOfPertitnentCalendars = MyListOfPertitnentCalendars.OrderBy(obj => obj.End).ToList();
                List<mTuple<bool, SubCalendarEvent>> MyListOfPertinentSubEvent = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
                foreach (CalendarEvent MyCalendarEvent in MyListOfPertitnentCalendars)
                {
                    List<SubCalendarEvent> InterFerringSubCalendarEventS = MyListOfDictionaryOfCalendarEventAndSubCalendarEvent[MyCalendarEvent];
                    if (MyCalendarEvent.Repeat.Enable)
                    {
                        List<CalendarEvent> MyListOfAffectedRepeatingCalendarEvent = getClashingCalendarEvent(MyCalendarEvent.Repeat.RecurringCalendarEvents().ToList(), MyTimeLine);



                        List<mTuple<bool, SubCalendarEvent>> ListOfAffectedSubcalendarEvents = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
                        foreach (CalendarEvent MyRepeatCalendarEvent in MyListOfAffectedRepeatingCalendarEvent)
                        {
                            SubCalendarEvent[] MyListOfSubCalendarEvents = MyRepeatCalendarEvent.ActiveSubEvents;
                            foreach (SubCalendarEvent PosibleClashingSubCalEvent in MyListOfSubCalendarEvents)
                            {
                                foreach (SubCalendarEvent eachInterFerringSubCalendarEvent in InterFerringSubCalendarEventS)
                                {
                                    if (PosibleClashingSubCalEvent.getId == eachInterFerringSubCalendarEvent.getId)
                                    {
                                        ListOfAffectedSubcalendarEvents.Add(getmTupleSubCalendarEvent(MyListOfInterferringmTupleSubCalendarEvents, eachInterFerringSubCalendarEvent));
                                    }
                                }
                            }
                        }
                        MyListOfPertinentSubEvent.AddRange(ListOfAffectedSubcalendarEvents);
                    }
                    else
                    {
                        MyListOfPertinentSubEvent.AddRange(getmTupleSubCalendarEvent(MyListOfInterferringmTupleSubCalendarEvents, MyListOfDictionaryOfCalendarEventAndSubCalendarEvent[MyCalendarEvent]));

                    }
                }
                //var ConcatVar = MyListOfPertinentSubEvent.Concat(TempSubCalendarEventsForMyCalendarEvent.ToList());
                //MyListOfPertinentSubEvent = ConcatVar.ToList();
                DictionaryofTimeLineAndPertinentSubcalendar.Add(MyTimeLine, MyListOfPertinentSubEvent);
            }

            return DictionaryofTimeLineAndPertinentSubcalendar;

            /*foreach(TimeLine MyTimeLine in MyListOfFreeTimelines)
            {
                List<SubCalendarEvent> MyTimeLineListToWorkWith = getIntersectionList(MyInterferringSubCalendarEvents, DictionsryofTimeLineAndPertinentSubcalenda[MyTimeLine]);
                
            }
            */


        }

        int MaxNumberOfInterferringSubcalEvents = 0;
        int maxHackConstant = 2;
        List<List<List<SubCalendarEvent>>> ListOfAllSnugPossibilitiesInRespectiveTImeLines_hack = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();

        int LargestTimeIndex = -1;

        private CalendarEvent[] getPertinentCalendarEvents(CalendarEvent[] PossibleCalendarEvents, TimeLine VerifiableSpace)
        {
            List<CalendarEvent> MyPertinentCalendarList = new List<CalendarEvent>();

            foreach (CalendarEvent MyCalendarEvent in PossibleCalendarEvents)
            {

                if ((MyCalendarEvent.Start < VerifiableSpace.End) && (MyCalendarEvent.End > VerifiableSpace.Start))
                {
                    MyPertinentCalendarList.Add(MyCalendarEvent);
                }
            }
            return MyPertinentCalendarList.ToArray();
        }
       

        List<List<List<SubCalendarEvent>>> BuildListMatchingTimelineAndSubCalendarEvent(List<List<TimeSpan>> ListOfSnugPossibilities, List<SubCalendarEvent> ListOfSubCalendarEvents)
        {
            /*
             *Name: Jerome Biotidara
             *Description: This function is to be called when all the snug possibilites are generated and we need to verify a match of the subcalendar events and Timespan. Essentially this function builds a permutation of the timeline and subcalendar match ups. Take for example two Subcalendar events with the same Timespan it means they can both possibly fit. So it builds two lists with bit possibilities.
             *It gets ListOfSnugPossibilities which is a List of Snug Timespan Permutation e.g for a time slot of 10 and entries 1,2,3,4 ListOfSnugPossibilities has ({1,2,3,4},{4,2,3,1,{1,3,2,4}}), ListOfSubCalendarEvents is just a list of SubCalendar Events
             *It ultimately returns a List similar to ListOfSnugPossibilities however each time Element is an array of Subcalendar Events that match the TimeSpan of time element
             *Date:07/02/2013
             */
            List<TimeSpan> AllTimesSpan = new List<TimeSpan>();
            Dictionary<TimeSpan, List<SubCalendarEvent>> DictionaryOfTImespanandSubCalendarEvent = new System.Collections.Generic.Dictionary<TimeSpan, System.Collections.Generic.List<SubCalendarEvent>>();
            List<List<List<SubCalendarEvent>>> MatchingListOfSnugPossibilitesWithSubcalendarEvents = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();
            /*foreach (List<TimeSpan> MySnugPossibility in ListOfSnugPossibilities)
            {
                var MyConcatList = AllTimesSpan.Concat(MySnugPossibility);
                AllTimesSpan=MyConcatList.ToList();
            }*/

            foreach (SubCalendarEvent MySubCalendar in ListOfSubCalendarEvents)
            {
                try
                {
                    DictionaryOfTImespanandSubCalendarEvent[MySubCalendar.ActiveSlot.BusyTimeSpan].Add(MySubCalendar);
                }
                catch (Exception e)
                {

                    DictionaryOfTImespanandSubCalendarEvent.Add(MySubCalendar.ActiveSlot.BusyTimeSpan, new List<SubCalendarEvent>());
                    DictionaryOfTImespanandSubCalendarEvent[MySubCalendar.ActiveSlot.BusyTimeSpan].Add(MySubCalendar);
                    //DictionaryOfTImespanandSubCalendarEvent.Add(MyTimeSpan, new List<SubCalendarEvent>());
                }
            }




            foreach (List<TimeSpan> MySnugPossibility in ListOfSnugPossibilities)
            {
                List<List<SubCalendarEvent>> SnugPossibiltySubcalendarEvent = new List<List<SubCalendarEvent>>();
                foreach (TimeSpan MyTimeSpan in MySnugPossibility)
                {

                    SnugPossibiltySubcalendarEvent.Add(DictionaryOfTImespanandSubCalendarEvent[MyTimeSpan]);
                }
                MatchingListOfSnugPossibilitesWithSubcalendarEvents.Add(SnugPossibiltySubcalendarEvent);
            }

            return MatchingListOfSnugPossibilitesWithSubcalendarEvents;

        }

        Dictionary<TimeSpan, List<SubCalendarEvent>> BuildDictionaryOfSubCalendarEventsAndTimespan(List<TimeSpan> MyListOfTimeSpan, List<SubCalendarEvent> MyListOfSubCalenedarEvents)
        {
            Dictionary<TimeSpan, List<SubCalendarEvent>> MyDictionaryOfTimeSpanAndSubCalendarEvents = new Dictionary<TimeSpan, List<SubCalendarEvent>>();
            foreach (SubCalendarEvent MySubCalendarEvent in MyListOfSubCalenedarEvents)
            {
                try
                {
                    MyDictionaryOfTimeSpanAndSubCalendarEvents.Add(MySubCalendarEvent.getActiveDuration, new List<SubCalendarEvent>());//Test to see if  the dictionary key is built with an object reference oir the value of the timespan. This is because the object is probably passed by referencxe
                    MyDictionaryOfTimeSpanAndSubCalendarEvents[MySubCalendarEvent.getActiveDuration].Add(MySubCalendarEvent);

                }
                catch (Exception ex)
                {
                    MyDictionaryOfTimeSpanAndSubCalendarEvents[MySubCalendarEvent.getActiveDuration].Add(MySubCalendarEvent);
                }
            }
            return MyDictionaryOfTimeSpanAndSubCalendarEvents;
        }


        List<BusyTimeLine>[] CategorizeTimeLine_noEventSchedule(TimeLine MyRange, BusyTimeLine[] SortedByStartArrayOfBusyTimeLine)//This  returns An array of a List of SubCalendarEvents. The function goes through each of the BUsytimeline Events and verifies if its parent calendar Event has a start Time that is eiter earlier than range, within range or start and ends within the range
        {
            List<BusyTimeLine>[] ArrayOfDifferentVaryingSubEventsCategories = new List<BusyTimeLine>[4];


            List<BusyTimeLine> MyArrayOfBeforeAndEndsAfterBusyTimelines = new List<BusyTimeLine>(0);


            List<BusyTimeLine> MyArrayOfBeforeAndEndsBeforeBusyTimelines = new List<BusyTimeLine>(0);


            List<BusyTimeLine> MyArrayOfStartsAfterAndEndsAfterBusyTimelines = new List<BusyTimeLine>(0);


            List<BusyTimeLine> MyArrayOfStartsAndEndsBeforeBusyTimelines = new List<BusyTimeLine>(0);

            int i = 0;
            for (; i < SortedByStartArrayOfBusyTimeLine.Length; i++)
            {
                EventID MyEventID = new EventID(SortedByStartArrayOfBusyTimeLine[i].TimeLineID);
                string ParentCalendarEventID = MyEventID.getCalendarEventComponent();

                if ((MyRange.Start < SortedByStartArrayOfBusyTimeLine[i].Start) && (MyRange.End > SortedByStartArrayOfBusyTimeLine[i].End))//checks if Calendar Event Starts starts before range and ends after range
                {
                    MyArrayOfStartsAndEndsBeforeBusyTimelines.Add(SortedByStartArrayOfBusyTimeLine[i]);

                }

                if ((MyRange.Start < SortedByStartArrayOfBusyTimeLine[i].Start) && (MyRange.End <= SortedByStartArrayOfBusyTimeLine[i].End))//checks if Calendar Event Starts starts before range and ends within range
                {
                    MyArrayOfStartsAfterAndEndsAfterBusyTimelines.Add(SortedByStartArrayOfBusyTimeLine[i]);

                }

                if ((MyRange.Start >= SortedByStartArrayOfBusyTimeLine[i].Start) && (MyRange.End > SortedByStartArrayOfBusyTimeLine[i].End))//checks if Calendar Event Starts within range and ends after range
                {
                    MyArrayOfBeforeAndEndsBeforeBusyTimelines.Add(SortedByStartArrayOfBusyTimeLine[i]);

                }

                if ((MyRange.Start >= SortedByStartArrayOfBusyTimeLine[i].Start) && (MyRange.End <= SortedByStartArrayOfBusyTimeLine[i].End))//checks if Calendar Event Starts within range and ends within range
                {
                    MyArrayOfBeforeAndEndsAfterBusyTimelines.Add(SortedByStartArrayOfBusyTimeLine[i]);
                }


            }
            ArrayOfDifferentVaryingSubEventsCategories[0] = MyArrayOfBeforeAndEndsAfterBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[1] = MyArrayOfBeforeAndEndsBeforeBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[2] = MyArrayOfStartsAfterAndEndsAfterBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[3] = MyArrayOfStartsAndEndsBeforeBusyTimelines;


            return ArrayOfDifferentVaryingSubEventsCategories;
        }


        List<CalendarEvent>[] CategorizeCalendarEventTimeLine(TimeLine MyRange, BusyTimeLine[] SortedByStartArrayOfBusyTimeLine)//This  returns An array of a List of SubCalendarEvents. The function goes through each of the BUsytimeline Events and verifies if its parent calendar Event has a start Time that is eiter earlier than range, within range or start and ends within the range
        {
            List<SubCalendarEvent>[] ArrayOfDifferentVaryingSubEventsCategories = new List<SubCalendarEvent>[4];
            List<CalendarEvent>[] ArrayCalendarEventOfDifferentVaryingSubEventsCategories = new List<CalendarEvent>[4];

            List<SubCalendarEvent> MyArrayOfBeforeAndEndsAfterBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfBeforeAndEndsAfterBusyTimelines = new List<CalendarEvent>(0);

            List<SubCalendarEvent> MyArrayOfBeforeAndEndsBeforeBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfBeforeAndEndsBeforeBusyTimelines = new List<CalendarEvent>(0);

            List<SubCalendarEvent> MyArrayOfStartsAfterAndEndsAfterBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfStartsAfterAndEndsAfterBusyTimelines = new List<CalendarEvent>(0);

            List<SubCalendarEvent> MyArrayOfStartsAndEndsBeforeBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfStartsAndEndsBeforeBusyTimelines = new List<CalendarEvent>(0);
            int i = 0;
            for (; i < SortedByStartArrayOfBusyTimeLine.Length; i++)
            {
                EventID MyEventID = new EventID(SortedByStartArrayOfBusyTimeLine[i].TimeLineID);
                string ParentCalendarEventID = MyEventID.getCalendarEventComponent();

                if ((AllEventDictionary[ParentCalendarEventID].Start < MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End > MyRange.End))//checks if Calendar Event Starts starts before range and ends after range
                {
                    MyArrayOfBeforeAndEndsAfterBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfBeforeAndEndsAfterBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID]);
                }

                if ((AllEventDictionary[ParentCalendarEventID].Start < MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End <= MyRange.End))//checks if Calendar Event Starts starts before range and ends within range
                {
                    MyArrayOfBeforeAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfBeforeAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID]);
                }

                if ((AllEventDictionary[ParentCalendarEventID].Start >= MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End <= MyRange.End))//checks if Calendar Event Starts within range and ends within range
                {
                    MyArrayOfStartsAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfStartsAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID]);
                }

                if ((AllEventDictionary[ParentCalendarEventID].Start >= MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End > MyRange.End))//checks if Calendar Event Starts within range and ends after range
                {
                    MyArrayOfStartsAfterAndEndsAfterBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfStartsAfterAndEndsAfterBusyTimelines.Add((AllEventDictionary[ParentCalendarEventID]));
                }
            }
            ArrayOfDifferentVaryingSubEventsCategories[0] = MyArrayOfBeforeAndEndsAfterBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[1] = MyArrayOfBeforeAndEndsBeforeBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[2] = MyArrayOfStartsAfterAndEndsAfterBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[3] = MyArrayOfStartsAndEndsBeforeBusyTimelines;

            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[0] = MyArrayCalendarEventOfBeforeAndEndsAfterBusyTimelines;
            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[1] = MyArrayCalendarEventOfBeforeAndEndsBeforeBusyTimelines;
            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[2] = MyArrayCalendarEventOfStartsAfterAndEndsAfterBusyTimelines;
            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[3] = MyArrayCalendarEventOfStartsAndEndsBeforeBusyTimelines;
            return ArrayCalendarEventOfDifferentVaryingSubEventsCategories;
        }

        List<SubCalendarEvent>[] CategorizeSubEventsTimeLine(TimeLine MyRange, BusyTimeLine[] SortedByStartArrayOfBusyTimeLine)//This  returns An array of a List of SubCalendarEvents. The function goes through each of the BUsytimeline Events and verifies if its parent calendar Event has a start Time that is eiter earlier than range, within range or start and ends within the range
        {
            List<SubCalendarEvent>[] ArrayOfDifferentVaryingSubEventsCategories = new List<SubCalendarEvent>[4];
            List<CalendarEvent>[] ArrayCalendarEventOfDifferentVaryingSubEventsCategories = new List<CalendarEvent>[4];

            List<SubCalendarEvent> MyArrayOfBeforeAndEndsAfterBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfBeforeAndEndsAfterBusyTimelines = new List<CalendarEvent>(0);

            List<SubCalendarEvent> MyArrayOfBeforeAndEndsBeforeBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfBeforeAndEndsBeforeBusyTimelines = new List<CalendarEvent>(0);

            List<SubCalendarEvent> MyArrayOfStartsAfterAndEndsAfterBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfStartsAfterAndEndsAfterBusyTimelines = new List<CalendarEvent>(0);

            List<SubCalendarEvent> MyArrayOfStartsAndEndsBeforeBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfStartsAndEndsBeforeBusyTimelines = new List<CalendarEvent>(0);
            int i = 0;
            for (; i < SortedByStartArrayOfBusyTimeLine.Length; i++)
            {
                EventID MyEventID = new EventID(SortedByStartArrayOfBusyTimeLine[i].TimeLineID);
                string ParentCalendarEventID = MyEventID.getCalendarEventComponent();

                if ((AllEventDictionary[ParentCalendarEventID].Start < MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End > MyRange.End))//checks if Calendar Event Starts starts before range and ends after range
                {
                    MyArrayOfBeforeAndEndsAfterBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfBeforeAndEndsAfterBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID]);
                }

                if ((AllEventDictionary[ParentCalendarEventID].Start < MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End <= MyRange.End))//checks if Calendar Event Starts starts before range and ends within range
                {
                    MyArrayOfBeforeAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfBeforeAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID]);
                }

                if ((AllEventDictionary[ParentCalendarEventID].Start >= MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End <= MyRange.End))//checks if Calendar Event Starts within range and ends within range
                {
                    MyArrayOfStartsAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfStartsAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID]);
                }

                if ((AllEventDictionary[ParentCalendarEventID].Start >= MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End > MyRange.End))//checks if Calendar Event Starts within range and ends after range
                {
                    MyArrayOfStartsAfterAndEndsAfterBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfStartsAfterAndEndsAfterBusyTimelines.Add((AllEventDictionary[ParentCalendarEventID]));
                }
            }
            ArrayOfDifferentVaryingSubEventsCategories[0] = MyArrayOfBeforeAndEndsAfterBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[1] = MyArrayOfBeforeAndEndsBeforeBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[2] = MyArrayOfStartsAfterAndEndsAfterBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[3] = MyArrayOfStartsAndEndsBeforeBusyTimelines;

            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[0] = MyArrayCalendarEventOfBeforeAndEndsAfterBusyTimelines;
            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[1] = MyArrayCalendarEventOfBeforeAndEndsBeforeBusyTimelines;
            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[2] = MyArrayCalendarEventOfStartsAfterAndEndsAfterBusyTimelines;
            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[3] = MyArrayCalendarEventOfStartsAndEndsBeforeBusyTimelines;
            return ArrayOfDifferentVaryingSubEventsCategories;
        }



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
            if (MyCalendarEvent.RepetitionStatus)
            {
                throw new Exception("Weird error, found repeating event repeaing evemt");
            }
            return getInterferringSubEvents(MyCalendarEvent.RangeTimeLine, NonCOmmitedCalendarEvemts);
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

            IEnumerable<KeyValuePair<string, CalendarEvent>> content = AllEventDictionary.Where(obj => obj.Value.isActive);

            foreach (KeyValuePair<string, CalendarEvent> MyCalendarEventDictionaryEntry in AllEventDictionary.Where(obj=>obj.Value.isActive))
            {
                if (MyCalendarEventDictionaryEntry.Value.RepetitionStatus)
                {
                    SubCalendarEvent[] ArrayOfSubcalendarEventsFromRepeatingEvents = MyCalendarEventDictionaryEntry.Value.ActiveRepeatSubCalendarEvents.Where(obj => obj != null).ToArray();//hack alert you should be able to remove the LINQ test for null
                    lengthOfCalendarSubEvent = ArrayOfSubcalendarEventsFromRepeatingEvents.Length;
                    MyArrayOfInterferringSubCalendarEvents.AddRange(MyCalendarEventDictionaryEntry.Value.ActiveRepeatSubCalendarEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.RangeTimeLine) != null).ToList());
                }
                else
                {
                    SubCalendarEvent[] ArrayOfSubcalendarEventsFromNonRepeatingEvents = MyCalendarEventDictionaryEntry.Value.ActiveSubEvents.Where(obj => obj != null).ToArray();//hack alert you should be able to remove the LINQ test for null
                    lengthOfCalendarSubEvent = ArrayOfSubcalendarEventsFromNonRepeatingEvents.Length;
                    MyArrayOfInterferringSubCalendarEvents.AddRange(MyCalendarEventDictionaryEntry.Value.ActiveSubEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.RangeTimeLine) != null).ToList());
                }
            }

            if (NonCommitedCalendarEvemts != null)
            {
                foreach (CalendarEvent eachCalendarEvent in NonCommitedCalendarEvemts)
                {

                    int i = 0;
                    if (eachCalendarEvent.RepetitionStatus)
                    {
                        lengthOfCalendarSubEvent = eachCalendarEvent.ActiveRepeatSubCalendarEvents.Length;
                        SubCalendarEvent[] ArrayOfSubcalendarEventsFromRepeatingEvents = eachCalendarEvent.ActiveRepeatSubCalendarEvents;

                        MyArrayOfInterferringSubCalendarEvents.AddRange(eachCalendarEvent.ActiveRepeatSubCalendarEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.RangeTimeLine) != null).ToList());
                    }
                    else
                    {
                        SubCalendarEvent[] ArrayOfSubcalendarEventsFromNonRepeatingEvents = eachCalendarEvent.ActiveSubEvents.Where(obj => obj != null).ToArray();//hack alert you should be able to remove the LINQ test for null
                        lengthOfCalendarSubEvent = ArrayOfSubcalendarEventsFromNonRepeatingEvents.Length;


                        MyArrayOfInterferringSubCalendarEvents.AddRange(eachCalendarEvent.ActiveSubEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.RangeTimeLine) != null).ToList());
                    }
                }
            }

            return MyArrayOfInterferringSubCalendarEvents.ToArray();

        }



        public TimeLine[] SplitFreeSpotsInToSubEventTimeSlots(TimeLine[] AllAvailableFreeSpots, int NumberOfAllotments, TimeSpan TotalActiveDuration)//function is responsible for dividing busy timeline. Also sapcing them out
        {
            TimeLine[] MyArrayOfToBeAssignedTimeLine = new TimeLine[NumberOfAllotments];
            double IdealTimePerAllotment = TotalActiveDuration.TotalSeconds / NumberOfAllotments;
            Dictionary<TimeLine, int> TimeLineAndFitCount = new Dictionary<TimeLine, int>();
            int i = 0;
            int FitCount = 0;
            for (; i < AllAvailableFreeSpots.Length; i++)
            {
                FitCount = (int)(AllAvailableFreeSpots[i].TimelineSpan.TotalSeconds / IdealTimePerAllotment);
                TimeLineAndFitCount.Add(AllAvailableFreeSpots[i], FitCount);
            }
            i = 0;
            int[] AllFitCount = TimeLineAndFitCount.Values.ToArray();
            int myTotalFitCount = 0;
            for (; i < AllFitCount.Length; i++)
            {
                myTotalFitCount += AllFitCount[i];
            }

            if (myTotalFitCount >= NumberOfAllotments)
            {
                MyArrayOfToBeAssignedTimeLine = IdealAllotAndInsert(TimeLineAndFitCount, MyArrayOfToBeAssignedTimeLine, new TimeSpan((long)IdealTimePerAllotment * 10000000));
            }
            else
            {
                return null;
            }

            return MyArrayOfToBeAssignedTimeLine;
        }

        public TimeLine[] IdealAllotAndInsert(Dictionary<TimeLine, int> Dict_AvailablFreeTimeLineAndFitCount, TimeLine[] MyArrayOfToBeAssignedTimeLine, TimeSpan IdealTimePerAllotment)
        {
            int i = 0;
            int j = 0;
            int k = 0;
            int TopCounter = 0;
            TimeLine[] ArrayOfFreeTimeline = Dict_AvailablFreeTimeLineAndFitCount.Keys.ToArray();//array of FreeTimeLineRanges
            Dictionary<TimeLine, TimeLine[]> TimeLineDictionary = new Dictionary<TimeLine, TimeLine[]>();
            int[] ArrayOfFitCount = Dict_AvailablFreeTimeLineAndFitCount.Values.ToArray();//array list of fit count

            /*foreach (TimeLine MyTimeLine in MyArrayOfToBeAssignedTimeLine)
            { 
                TimeLineDictionary.Add(MyTimeLine, new TimeLine[1]);
            }
            */
            int BlockSplitsPerTimeLine = 0;
            int TotalAssignedSoFar = 0;

            for (; (i < MyArrayOfToBeAssignedTimeLine.Length) && (TotalAssignedSoFar < MyArrayOfToBeAssignedTimeLine.Length); i++)
            {
                ++BlockSplitsPerTimeLine;
                TotalAssignedSoFar = 0;
                for (j = 0; (j < ArrayOfFreeTimeline.Length) && (TotalAssignedSoFar < MyArrayOfToBeAssignedTimeLine.Length); j++)
                {
                    if (Dict_AvailablFreeTimeLineAndFitCount[ArrayOfFreeTimeline[j]] > 0)
                    {

                        if (Dict_AvailablFreeTimeLineAndFitCount[ArrayOfFreeTimeline[j]] >= BlockSplitsPerTimeLine)
                        {
                            try
                            { TimeLineDictionary.Add(ArrayOfFreeTimeline[j], SplitAndAssign(ArrayOfFreeTimeline[j], BlockSplitsPerTimeLine, IdealTimePerAllotment)); }
                            catch (Exception ex)
                            {
                                TimeLineDictionary[ArrayOfFreeTimeline[j]] = SplitAndAssign(ArrayOfFreeTimeline[j], BlockSplitsPerTimeLine, IdealTimePerAllotment);
                            }
                        }


                        TotalAssignedSoFar += TimeLineDictionary[ArrayOfFreeTimeline[j]].Length;
                    }

                }
            }

            

            TimeLine[][] MyArrayOfTimeLines = TimeLineDictionary.Values.ToArray();

            k = 0;
            i = 0;
            //for (; i < MyArrayOfToBeAssignedTimeLine.Length; )
            {
                j = 0;
                for (; ((j < MyArrayOfTimeLines.Length) && (i < MyArrayOfToBeAssignedTimeLine.Length)); j++)
                {

                    foreach (TimeLine MyTimeLine in MyArrayOfTimeLines[j])
                    {
                        if (i < MyArrayOfToBeAssignedTimeLine.Length)
                        {
                            MyArrayOfToBeAssignedTimeLine[i] = MyTimeLine;
                            i++;
                        }
                    }
                }
            }

            if (MyArrayOfToBeAssignedTimeLine.Length > i)
            {
                //MessageBox.Show("ALERT!!! NOT ALL SUBCALENDAR EVENTS WILL BE ASSIGNED");
            }

            return MyArrayOfToBeAssignedTimeLine;
        }

        TimeLine JustFillEachOneFirst(TimeLine MyRange, TimeSpan MyActiveDuration)
        {
            return Utility.CentralizeYourSelfWithinRange(MyRange, MyActiveDuration);

        }

        TimeLine[] SplitAndAssign(TimeLine RangeOfSplit, int NumberOfSplits, TimeSpan DurationLength)
        {

            TimeSpan MySpanForEachSection = new TimeSpan((long)((RangeOfSplit.TimelineSpan.TotalMilliseconds / NumberOfSplits) * 10000));
            TimeLine[] MyArrayOfElements = new TimeLine[NumberOfSplits];
            int i = 0;
            TimeLine MyReferenceRange = new TimeLine();
            double TotalMilliseconds = i * (MySpanForEachSection.TotalMilliseconds);
            DateTimeOffset RangeStart;
            DateTimeOffset RangeEnd;
            for (; i < NumberOfSplits; i++)
            {
                TotalMilliseconds = i * (MySpanForEachSection.TotalMilliseconds);
                RangeStart = RangeOfSplit.Start.AddMilliseconds(TotalMilliseconds);
                RangeEnd = RangeStart.Add(MySpanForEachSection);
                MyArrayOfElements[i] = Utility. CentralizeYourSelfWithinRange(new TimeLine(RangeStart, RangeEnd), DurationLength);
            }
            return MyArrayOfElements;

        }
        

        private SubCalendarEvent AssignSubEventTimeSlot(SubCalendarEvent MySubEvent)
        {
            BusyTimeLine[] ArrayOfInvadingEvents = CheckIfMyRangeAlreadyHasSchedule(CompleteSchedule.OccupiedSlots, MySubEvent);
            TimeLine[] AvailableFreeSpots = getAllFreeSpots(new TimeLine(MySubEvent.Start, MySubEvent.End));
            for (int i = 0; i < AvailableFreeSpots.Length; i++)
            {
                if (AvailableFreeSpots[i].TimelineSpan > (MySubEvent.getActiveDuration))
                {
                    DateTimeOffset DurationStartTime;
                    TimeSpan MyTimeSpan = new TimeSpan((((AvailableFreeSpots[i].TimelineSpan - MySubEvent.getActiveDuration).Milliseconds) / 2) * 10000);
                    DurationStartTime = AvailableFreeSpots[i].Start.Add(MyTimeSpan);
                    MySubEvent.ActiveSlot = new BusyTimeLine(MySubEvent.getId, DurationStartTime, DurationStartTime.Add(MySubEvent.getActiveDuration));
                }
            }
            return MySubEvent;
        }
        
        static public int GetLeftThatsGreaterFromStart(List<BusyTimeLine> MyArray, int MyPivot, int LeftStartinPosition)
        {
            for (int i = LeftStartinPosition; i <= MyPivot; i++)
            {
                if (MyArray[i].Start > MyArray[MyPivot].Start)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public int GetRightThatsLessFromStart(List<BusyTimeLine> MyArray, int MyPivot, int RightStartinPosition)
        {
            for (int i = RightStartinPosition; i >= MyPivot; i--)
            {
                if (MyArray[i].Start < MyArray[MyPivot].Start)
                {
                    return i;
                }
            }
            return MyPivot;
        }
        
        static public int GetLeftThatsGreaterFromEnd(List<BusyTimeLine> MyArray, int MyPivot, int LeftStartinPosition)
        {
            for (int i = LeftStartinPosition; i <= MyPivot; i++)
            {
                if (MyArray[i].End > MyArray[MyPivot].End)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public int GetRightThatsLessFromEnd(List<BusyTimeLine> MyArray, int MyPivot, int RightStartinPosition)
        {
            for (int i = RightStartinPosition; i >= MyPivot; i--)
            {
                if (MyArray[i].End < MyArray[MyPivot].End)
                {
                    return i;
                }
            }
            return MyPivot;
        }
        
        static public int GetLeftCalendarEventThatsGreaterFromEnd(List<CalendarEvent> MyArray, int MyPivot, int LeftStartinPosition)
        {
            for (int i = LeftStartinPosition; i <= MyPivot; i++)
            {
                if (MyArray[i].End > MyArray[MyPivot].End)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public int GetRightCalendarEventThatsLessFromEnd(List<CalendarEvent> MyArray, int MyPivot, int RightStartinPosition)
        {
            for (int i = RightStartinPosition; i >= MyPivot; i--)
            {
                if (MyArray[i].End < MyArray[MyPivot].End)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public int GetLeftCalendarEventThatsGreaterFromStart(List<CalendarEvent> MyArray, int MyPivot, int LeftStartinPosition)
        {
            for (int i = LeftStartinPosition; i <= MyPivot; i++)
            {
                if (MyArray[i].Start > MyArray[MyPivot].Start)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public int GetRightCalendarEventThatsLessFromStart(List<CalendarEvent> MyArray, int MyPivot, int RightStartinPosition)
        {
            for (int i = RightStartinPosition; i >= MyPivot; i--)
            {
                if (MyArray[i].Start < MyArray[MyPivot].Start)
                {
                    return i;
                }
            }
            return MyPivot;
        }
        
        static public int GetLeftSubCalendarEventThatsGreaterFromEnd(List<SubCalendarEvent> MyArray, int MyPivot, int LeftStartinPosition)
        {
            for (int i = LeftStartinPosition; i <= MyPivot; i++)
            {
                if (MyArray[i].End > MyArray[MyPivot].End)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public int GetRightSubCalendarEventThatsLessFromEnd(List<SubCalendarEvent> MyArray, int MyPivot, int RightStartinPosition)
        {
            for (int i = RightStartinPosition; i >= MyPivot; i--)
            {
                if (MyArray[i].End < MyArray[MyPivot].End)
                {
                    return i;
                }
            }
            return MyPivot;
        }
        static public int GetLeftSubCalendarEventThatsGreaterFromStart(List<SubCalendarEvent> MyArray, int MyPivot, int LeftStartinPosition)
        {
            for (int i = LeftStartinPosition; i <= MyPivot; i++)
            {
                if (MyArray[i].Start > MyArray[MyPivot].Start)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public int GetRightSubCalendarEventThatsLessFromStart(List<SubCalendarEvent> MyArray, int MyPivot, int RightStartinPosition)
        {
            for (int i = RightStartinPosition; i >= MyPivot; i--)
            {
                if (MyArray[i].Start < MyArray[MyPivot].Start)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        /*QUICK SORT SECTION END*/

        public BusyTimeLine[] CheckIfMyRangeAlreadyHasSchedule(BusyTimeLine[] BusySlots, SubCalendarEvent MySubEvent)
        {
            List<BusyTimeLine> InvadingEvents = new List<BusyTimeLine>();
            int i = 0;
            for (; i < BusySlots.Length; i++)
            {
                if ((BusySlots[i].End > MySubEvent.Start) && (BusySlots[i].End < MySubEvent.End))
                {
                    InvadingEvents.Add(BusySlots[i]);
                }
            }


            InvadingEvents = SortBusyTimeline(InvadingEvents, true);


            return InvadingEvents.ToArray();
        }

        static public List<BusyTimeLine> SortBusyTimeline(List<BusyTimeLine> MyUnsortedEvents, bool StartOrEnd)//True is from start. False is from end
        {
            List<BusyTimeLine> retValue;
            if(StartOrEnd)
            {
                retValue = MyUnsortedEvents.OrderBy(obj=>obj.Start).ToList();
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
                return AllFreeSlots;
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

            return SpecificFreeSpots.ToArray();
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


        /*
        DateTimeOffset UpdateNow(DateTimeOffset UpdatedNow)
        {
            Now = UpdatedNow;
            return Now;
        }

        */
        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ProcrastinateJustAnEvent(string EventID, TimeSpan RangeOfPush)
        {

            CalendarEvent ProcrastinateEvent = getCalendarEvent(EventID);
            SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(EventID);
            if (ReferenceSubEvent != null)
            {
                EventID SubEventID = new EventID(EventID);
                DateTimeOffset ReferenceStart = Now.calculationNow > ReferenceSubEvent.Start ? Now.calculationNow : ReferenceSubEvent.Start;
                Procrastination procrastinateData = new Procrastination(ReferenceSubEvent.Start, RangeOfPush);

                ReferenceStart = Now.UpdateNow(ReferenceStart);
                DateTimeOffset StartTimeOfProcrastinate = ReferenceStart + RangeOfPush;
                DateTimeOffset limitOfProcrastination = ReferenceSubEvent.getCalendarEventRange.End;
                TimeSpan ActiveSubEventSpan = TimeSpan.FromTicks(ProcrastinateEvent.ActiveSubEvents.Select(subEvent => subEvent.getActiveDuration.Ticks).Sum());
                limitOfProcrastination = limitOfProcrastination.Add(-ActiveSubEventSpan);
                if (StartTimeOfProcrastinate > limitOfProcrastination)
                {
                    return new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(new CustomErrors("Procrastinated deadline event is before end of selected timeline space"), AllEventDictionary);
                }


                if (ProcrastinateEvent.RepetitionStatus)
                {
                    ProcrastinateEvent = ProcrastinateEvent.getRepeatedCalendarEvent(SubEventID.getIDUpToRepeatCalendarEvent());
                }

                Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
                AllEventDictionary_Cpy = AllEventDictionary.ToDictionary(obj => obj.Key, obj => obj.Value.createCopy());

                List<SubCalendarEvent> AllValidSubCalEvents = ProcrastinateEvent.ActiveSubEvents.Where(obj => obj.End > ReferenceSubEvent.Start).ToList();



                TimeSpan TotalActiveDuration = Utility.SumOfActiveDuration(AllValidSubCalEvents);
                //CalendarEvent(string NameEntry, string StartTime, DateTimeOffset StartDateEntry, string EndTime, DateTimeOffset EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag,Location EventLocation)
                CalendarEvent ScheduleUpdated = ProcrastinateEvent.getProcrastinationCopy(procrastinateData); // new CalendarEvent(ProcrastinateEvent.Name, StartTimeOfProcrastinate.ToString("hh:mm tt"), StartTimeOfProcrastinate, ProcrastinateEvent.End.ToString("hh:mm tt"), ProcrastinateEvent.End, AllValidSubCalEvents.Count.ToString(), ProcrastinateEvent.PreDeadline.ToString(), TotalActiveDuration.ToString(), new Repetition(), true, ProcrastinateEvent.Rigid, ProcrastinateEvent.Preparation.ToString(), true, ProcrastinateEvent.myLocation, true, new EventDisplay(), new MiscData(), false);
                ProcrastinateEvent.DisableSubEvents(AllValidSubCalEvents);
                HashSet<SubCalendarEvent> NotDoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
                ScheduleUpdated = EvaluateTotalTimeLineAndAssignValidTimeSpots(ScheduleUpdated, NotDoneYet);

                SubCalendarEvent[] UpdatedSubCalevents = ScheduleUpdated.ActiveSubEvents;

                for (int i = 0; i < AllValidSubCalEvents.Count; i++)//updates the subcalevents
                {
                    SubCalendarEvent updatedSubCal = new SubCalendarEvent(AllValidSubCalEvents[i].getCreator, AllValidSubCalEvents[i].getAllUsers(), AllValidSubCalEvents[i].getTimeZone, AllValidSubCalEvents[i].getId, UpdatedSubCalevents[i].getName, UpdatedSubCalevents[i].Start, UpdatedSubCalevents[i].End, UpdatedSubCalevents[i].ActiveSlot, UpdatedSubCalevents[i].getRigid, AllValidSubCalEvents[i].isEnabled, AllValidSubCalEvents[i].getUIParam, AllValidSubCalEvents[i].Notes, AllValidSubCalEvents[i].getIsComplete, UpdatedSubCalevents[i].Location, ProcrastinateEvent.RangeTimeLine);
                    AllValidSubCalEvents[i].shiftEvent(updatedSubCal.Start - AllValidSubCalEvents[i].Start, true);///not using update this because of possible issues with subevent not being restricted
                    //AllValidSubCalEvents[i].UpdateThis(updatedSubCal);
                }

                ProcrastinateEvent.EnableSubEvents(AllValidSubCalEvents);
                ProcrastinateEvent.updateProcrastinate(procrastinateData);

                if (ScheduleUpdated.Error != null)
                {
                    LogStatus(ScheduleUpdated, "Procrastinate Single Event");
                }

                Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue = new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(ScheduleUpdated.Error, AllEventDictionary);
                AllEventDictionary = AllEventDictionary_Cpy;
                return retValue;
            } else
            {
                throw new CustomErrors("Invalid subcalendarevent Id provided.");
            }
        }

        static TimeLine ScheduleTimeline = new TimeLine();


        //public XmlElement CreateEventScheduleNode(CalendarEvent MyEvent, XmlDocument xmldoc)

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




        //Properties

        public uint LastScheduleIDNumber
        {
            get
            {
                return (EventID.LatestID);
            }
        }
    }
}
