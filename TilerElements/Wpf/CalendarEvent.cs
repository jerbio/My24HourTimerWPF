#define SetDefaultPreptimeToZero
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.Concurrent;

namespace TilerElements.Wpf
{
    public class CalendarEvent : TilerEvent, IDefinedRange
    {
        protected DateTimeOffset EndOfCalculation = DateTime.Now.AddMonths(3);
        // Fields
        static Dictionary<string, List<Double>> DistanceMatrixData;
        static List<string> DistanceMatixKeys;
        static List<Location_Elements> Horizontal;
        protected Repetition EventRepetition;
        protected int Splits;
        protected TimeSpan TimePerSplit;
        protected TimeSpan InitializingTimeSpanPerSplit;
        protected int CompletedCount;
        protected int DeletedCount;
//        protected bool FromRepetion=false;
        protected Dictionary<EventID, SubCalendarEvent> SubEvents { get; set; }
        //protected bool SchedulStatus;
        CustomErrors CalendarError = new CustomErrors(false, string.Empty);
        DateTime CalculationEnd;
        List<mTuple<EventID,string>> RemovedIDs;
        protected TimeLine EventSequence;
        protected HashSet<SubCalendarEvent> CalculableSubEvents = new HashSet<SubCalendarEvent>();
        protected HashSet<SubCalendarEvent> UnDesignables = new HashSet<SubCalendarEvent>();
        protected bool isCalculableInitialized = false;
        protected bool isUnDesignableInitialized = false;
        protected CalendarEvent RootOfRepeat;
        Dictionary<ulong, DayTimeLine> CalculationLimitationWithUnUsables;
        Dictionary<ulong, DayTimeLine> CalculationLimitation;
        Dictionary<ulong, DayTimeLine> FreeDaysLimitation;
        Deviation DeviatingSubCalendarEventInfo = new Deviation();
        Deviation DeviatingCalendarEventInfo = new Deviation();
        public enum DeviationType {deleted, completed, NowProfile};
        protected NowProfile ProfileOfNow = new NowProfile();
        protected bool transferSubCalendarEventsToDictionaries = false;
        



        /// <summary>
        /// Class defines the subevents that deviate from the norm of the calendar event subevent. Thsi could be subebvents that have a completed, deleted, or set as now.
        /// </summary>
        class Deviation 
        { 
            Dictionary<string, TilerEvent>[] DeviatingInfo = new Dictionary<string, TilerEvent>[] {new Dictionary<string, TilerEvent>(),new Dictionary<string, TilerEvent>(),new Dictionary<string, TilerEvent>() };
            public Deviation()
            {

            }


            /// <summary>
            /// UPdates the deviating data set. 0 type = deleted, 1 type = completed, 2 = Now Profile
            /// </summary>
            /// <param name="type"></param>
            /// <param name="TilerEvent"></param>
            public void updateDeviatingData(DeviationType type, TilerEvent TilerEvent)
            {
                DeviatingInfo[(int)type].Add((TilerEvent.Id), TilerEvent);
                TilerEvent.setAsDeviated();
            }

            public void removeFromDeviatingData(DeviationType type,string EventID)
            {
                DeviatingInfo[(int)type].Remove(EventID);
            }

            public List<Tuple<int, TilerEvent>> getSubEvents()
            {
                List<List<Tuple<int, TilerEvent>>> RetValueCombined = DeviatingInfo.Select((obj, i) => obj.Values.Select(obj1 => new Tuple<int, TilerEvent>(i, obj1)).ToList()).ToList();
                List<Tuple<int, TilerEvent>> RetValue = RetValueCombined.SelectMany(obj=>obj).ToList();
               return RetValue;
            }

            public IEnumerable<TilerEvent> getDeviationDataByType(int Type)
            {
                return DeviatingInfo[Type].Values;
            }
        }
        

        #region Constructor
        public CalendarEvent(CustomErrors Error)
        {
            EventDuration = new TimeSpan();
            
            StartDateTime = new DateTimeOffset();
            EndDateTime = new DateTimeOffset();
            EventPreDeadline = new TimeSpan();
            PrepTime = new TimeSpan();
            Priority = 0;
            //RepetitionFlag = false;
            EventRepetition = new Repetition();
            RigidSchedule = false;
            Splits = 1;
            LocationInfo = new Location_Elements();
            UniqueID = EventID.GenerateCalendarEvent();
            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            //SchedulStatus = false;
            otherPartyID = "";
            CalendarError = Error;
            EventSequence = new TimeLine();
            ProfileOfProcrastination = new Procrastination(new DateTimeOffset(), new TimeSpan());
            ProfileOfNow = new NowProfile();
            NameOfEvent = new EventName( UniqueID);
        }


        public CalendarEvent()
        {
            EventDuration = new TimeSpan();
            
            StartDateTime = new DateTimeOffset();
            EndDateTime = new DateTimeOffset();
            EventPreDeadline = new TimeSpan();
            PrepTime = new TimeSpan();
            Priority = 0;
            //RepetitionFlag = false;
            EventRepetition = new Repetition();
            RigidSchedule = false;
            Splits = 1;
            LocationInfo = new Location_Elements();
            UniqueID = EventID.GenerateCalendarEvent();
            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            //SchedulStatus = false;
            otherPartyID = "";
            CalendarError = new CustomErrors(false, string.Empty);
            EventSequence = new TimeLine();
            ProfileOfProcrastination = new Procrastination(new DateTimeOffset(), new TimeSpan());
            ProfileOfNow = new NowProfile();
            NameOfEvent = new EventName(UniqueID);
        }

        //CalendarEvent MyCalendarEvent = new CalendarEvent(NameEntry, Duration, StartDate, EndDate, PrepTime, PreDeadline, Rigid, Repeat, Split);

        /// <summary>
        /// to be used by xml initializer
        /// </summary>
        /// <param name="EventIDEntry"></param>
        /// <param name="NameEntry"></param>
        /// <param name="StartTime"></param>
        /// <param name="StartDateEntry"></param>
        /// <param name="EndTime"></param>
        /// <param name="EventEndDateEntry"></param>
        /// <param name="eventSplit"></param>
        /// <param name="PreDeadlineTime"></param>
        /// <param name="EventDuration"></param>
        /// <param name="EventRepetitionEntry"></param>
        /// <param name="DefaultPrepTimeflag"></param>
        /// <param name="RigidScheduleFlag"></param>
        /// <param name="eventPrepTime"></param>
        /// <param name="PreDeadlineFlag"></param>
        /// <param name="EventLocation"></param>
        /// <param name="EnableFlag"></param>
        /// <param name="UiData"></param>
        /// <param name="NoteData"></param>
        /// <param name="CompletionFlag"></param>
        public CalendarEvent(string EventIDEntry, string NameEntry, string StartTime, DateTimeOffset StartDateEntry, string EndTime, DateTimeOffset EventEndDateEntry,DateTimeOffset OriginalStartData,  string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag, Location_Elements EventLocation, bool EnableFlag, EventDisplay UiData, MiscData NoteData, bool CompletionFlag)
            : this(new ConstructorModified(EventIDEntry, NameEntry, StartTime, StartDateEntry, EndTime, EventEndDateEntry, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionEntry, DefaultPrepTimeflag, RigidScheduleFlag, eventPrepTime, PreDeadlineFlag, EnableFlag, UiData, NoteData, CompletionFlag), new EventID(EventIDEntry), OriginalStartData, EventLocation)
        { }
        /*
        internal CalendarEvent(string NameEntry, string StartTime, DateTimeOffset StartDateEntry, string EndTime, DateTimeOffset EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag,Location_Elements EventLocation,bool EnabledEventFlag, EventDisplay UiData,MiscData NoteData,bool CompletionFlag)
            : this(new ConstructorModified(NameEntry, StartTime, StartDateEntry, EndTime, EventEndDateEntry, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionEntry, DefaultPrepTimeflag, RigidScheduleFlag, eventPrepTime, PreDeadlineFlag, EnabledEventFlag,  UiData, NoteData, CompletionFlag), EventLocation)
        {
        }*/

        /// <summary>
        /// TO be used for adding a new event to Tiler
        /// </summary>
        /// <param name="NameEntry"></param>
        /// <param name="StartData"></param>
        /// <param name="EndData"></param>
        /// <param name="eventSplit"></param>
        /// <param name="PreDeadlineTime"></param>
        /// <param name="EventDuration"></param>
        /// <param name="EventRepetitionEntry"></param>
        /// <param name="DefaultPrepTimeflag"></param>
        /// <param name="RigidScheduleFlag"></param>
        /// <param name="eventPrepTime"></param>
        /// <param name="PreDeadlineFlag"></param>
        /// <param name="EventLocation"></param>
        /// <param name="EnabledEventFlag"></param>
        /// <param name="UiData"></param>
        /// <param name="NoteData"></param>
        /// <param name="CompletionFlag"></param>
        public CalendarEvent(string NameEntry, DateTimeOffset StartData, DateTimeOffset EndData,DateTimeOffset OriginalStartData, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag, Location_Elements EventLocation, bool EnabledEventFlag, EventDisplay UiData, MiscData NoteData, bool CompletionFlag)
            : this(new ConstructorModified(NameEntry, StartData, EndData, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionEntry, DefaultPrepTimeflag, RigidScheduleFlag, eventPrepTime, PreDeadlineFlag, EnabledEventFlag, UiData, NoteData, CompletionFlag), EventLocation, OriginalStartData)
        {
        }

        public CalendarEvent(CalendarEvent MyUpdated, SubCalendarEvent[] MySubEvents)
        {
            
            
            StartDateTime = MyUpdated.StartDateTime;
            EndDateTime = MyUpdated.End;
            EventSequence = new TimeLine(StartDateTime, EndDateTime);
            EventDuration = MyUpdated.Duration;
            Splits = MyUpdated.Splits;
            PrepTime = MyUpdated.PrepTime;
            EventPreDeadline = MyUpdated.PreDeadline;
            RigidSchedule = MyUpdated.Rigid;
            TimePerSplit = MyUpdated.TimePerSplit;
            InitializingTimeSpanPerSplit = MyUpdated.InitializingTimeSpanPerSplit;
            if (MyUpdated.ID != null)
            {
                UniqueID = new EventID(MyUpdated.ID);
            }
            Enabled = MyUpdated.isEnabled;
            Complete = MyUpdated.isComplete;
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
            NameOfEvent = MyUpdated.NameOfEvent;
            OriginalStart = MyUpdated.OriginalStart;
            //SchedulStatus = false;
            EventRepetition = MyUpdated.Repeat;
            ProfileOfProcrastination = MyUpdated.ProfileOfProcrastination;
            ProfileOfNow = MyUpdated.NowInfo;
            LocationInfo = MyUpdated.LocationInfo;
            UpdateLocationMatrix(LocationInfo);
            EventSequence = new TimeLine(StartDateTime, EndDateTime);
        }
        private CalendarEvent(ConstructorModified UpdatedConstructor, EventID MyEventID, DateTimeOffset OriginalStartData, Location_Elements EventLocation=null)
            : this(MyEventID, UpdatedConstructor.Name, UpdatedConstructor.Duration, UpdatedConstructor.StartDate, UpdatedConstructor.EndDate,OriginalStartData, UpdatedConstructor.PrepTime, UpdatedConstructor.PreDeadline, UpdatedConstructor.Rigid, UpdatedConstructor.Repeat, UpdatedConstructor.Split, EventLocation, UpdatedConstructor.Enabled, UpdatedConstructor.ui, UpdatedConstructor.noteData, UpdatedConstructor.complete,0)
        {
        }
        private CalendarEvent(ConstructorModified UpdatedConstructor, Location_Elements EventLocation,DateTimeOffset OriginalStartData)
            : this(UpdatedConstructor.Name, UpdatedConstructor.Duration, UpdatedConstructor.StartDate, UpdatedConstructor.EndDate,OriginalStartData, UpdatedConstructor.PrepTime, UpdatedConstructor.PreDeadline, UpdatedConstructor.Rigid, UpdatedConstructor.Repeat, UpdatedConstructor.Split, EventLocation,UpdatedConstructor.Enabled,UpdatedConstructor.ui,UpdatedConstructor.noteData,UpdatedConstructor.complete,0)
        {
        }


        public CalendarEvent(EventID EventIDEntry, string EventName, TimeSpan Event_Duration, DateTimeOffset EventStart, DateTimeOffset EventDeadline,DateTimeOffset OriginalStartData ,TimeSpan EventPrepTime, TimeSpan Event_PreDeadline, bool EventRigidFlag, Repetition EventRepetitionEntry, int EventSplit, Location_Elements EventLocation, bool enabledFlag, EventDisplay UiData, MiscData NoteData, bool CompletionFlag,long RepeatIndex)
        {
            
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = Event_Duration;
            Enabled = enabledFlag;
            EventRepetition = EventRepetitionEntry;
            PrepTime = EventPrepTime;
            EventPreDeadline = Event_PreDeadline;
            RigidSchedule = EventRigidFlag;
            LocationInfo = EventLocation;
            UniqueID = EventIDEntry;
            UiParams = UiData;
            DataBlob = NoteData;
            Complete = CompletionFlag;
            RepetitionSequence = RepeatIndex;
            OriginalStart = OriginalStartData;
            Splits = EventSplit;
            TimePerSplit = TimeSpan.FromTicks(((EventDuration.Ticks / Splits)));
            InitializingTimeSpanPerSplit = TimePerSplit;
            this.NameOfEvent = new EventName(UniqueID, EventName);

            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            if (!EventRepetition.Enable)
            {
                for (int i = 0; i < Splits; i++)
                {
                    //(TimeSpan Event_Duration, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, Location EventLocation =null, TimeLine RangeOfSubCalEvent = null)
                    SubCalendarEvent newSubCalEvent = new SubCalendarEvent(TimePerSplit, (EndDateTime - TimePerSplit), this.End, new TimeSpan(), OriginalStartData, UniqueID.ToString(),RigidSchedule, this.isEnabled, this.UiParams, this.Notes, this.Complete, i+1, EventLocation, this.RangeTimeLine);
                    SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
                }
            }
            EventSequence = new TimeLine(StartDateTime, EndDateTime);
            UpdateLocationMatrix(EventLocation);
        }

        public CalendarEvent(string EventName, TimeSpan Event_Duration, DateTimeOffset EventStart, DateTimeOffset EventDeadline, DateTimeOffset OriginalStartData ,TimeSpan EventPrepTime, TimeSpan Event_PreDeadline, bool EventRigidFlag, Repetition EventRepetitionEntry, int EventSplit, Location_Elements EventLocation, bool EnableFlag, EventDisplay UiData, MiscData NoteData, bool CompletionFlag, long RepetitionIndexData)
        {
            /*CalendarEventName = NameOfEvent.Split(',')[0];
            LocationString = "";
            if (NameOfEvent.Split(',').Length > 1)
            {
                LocationString = NameOfEvent.Split(',')[1];
            }
            CalendarEventLocation = null;
            CalendarEventLocation = new Location();
            if (LocationString != "")
            {
                CalendarEventLocation = new Location(LocationString);
            }
            */
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = Event_Duration;
            PrepTime = EventPrepTime;
            EventPreDeadline = Event_PreDeadline;
            RigidSchedule = EventRigidFlag;
            LocationInfo = EventLocation;
            EventRepetition = EventRepetitionEntry;
            RepetitionSequence = RepetitionIndexData;
            UniqueID = EventID.GenerateCalendarEvent();
            EventRepetition = EventRepetitionEntry;
            UiParams = UiData;
            DataBlob = NoteData;
            Complete = CompletionFlag;
            OriginalStart = OriginalStartData;
            Splits = EventSplit;
            TimePerSplit = TimeSpan.FromTicks(((EventDuration.Ticks / Splits)));
            InitializingTimeSpanPerSplit = TimePerSplit;
            //IAppDomainSetup n
            this.NameOfEvent = new EventName(UniqueID, EventName);

            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            if (!EventRepetition.Enable)
            {
                    for (int i = 0; i < Splits; i++)
                {
                    SubCalendarEvent newSubCalEvent = new SubCalendarEvent(TimePerSplit, (EndDateTime - TimePerSplit), this.End, new TimeSpan(), OriginalStart, UniqueID.ToString(), RigidSchedule, this.Enabled, this.UiParams, this.Notes, this.Complete, i+1, EventLocation, this.RangeTimeLine); //new SubCalendarEvent(CalendarEventID);
                    SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
                }
            }
            
            
            EventSequence = new TimeLine(StartDateTime, EndDateTime);
            UpdateLocationMatrix(LocationInfo);
        }
        #endregion

        #region Functions
        
        /// <summary>
        /// Function instantiates a single repetition calendarevent
        /// </summary>
        /// <param name="EventIDEntry"></param>
        /// <param name="EventName"></param>
        /// <param name="Event_Duration"></param>
        /// <param name="EventStart"></param>
        /// <param name="EventDeadline"></param>
        /// <param name="OriginalStartData"></param>
        /// <param name="EventPrepTime"></param>
        /// <param name="Event_PreDeadline"></param>
        /// <param name="EventRigidFlag"></param>
        /// <param name="EventRepetitionEntry"></param>
        /// <param name="EventSplit"></param>
        /// <param name="EventLocation"></param>
        /// <param name="enabledFlag"></param>
        /// <param name="UiData"></param>
        /// <param name="NoteData"></param>
        /// <param name="CompletionFlag"></param>
        /// <param name="RepeatIndex"></param>
        /// <returns></returns>

        static public CalendarEvent InstantiateRepeatedCandidate(EventID EventIDEntry, string EventName, TimeSpan Event_Duration, DateTimeOffset EventStart, DateTimeOffset EventDeadline, DateTimeOffset OriginalStartData, TimeSpan EventPrepTime, TimeSpan Event_PreDeadline, bool EventRigidFlag, Repetition EventRepetitionEntry, int EventSplit, Location_Elements EventLocation, bool enabledFlag, EventDisplay UiData, MiscData NoteData, bool CompletionFlag, long RepeatIndex, ConcurrentDictionary<DateTimeOffset, CalendarEvent> OrginalStartToCalendarEvent, CalendarEvent RepeatRootData)
        {
            CalendarEvent RetValue= new CalendarEvent();
            
            RetValue.StartDateTime = EventStart;
            RetValue.EndDateTime = EventDeadline;
            RetValue.EventDuration = Event_Duration;
            RetValue.Enabled = enabledFlag;
            RetValue.EventRepetition = EventRepetitionEntry;
            RetValue.PrepTime = EventPrepTime;
            RetValue.EventPreDeadline = Event_PreDeadline;
            RetValue.RigidSchedule = EventRigidFlag;
            RetValue.LocationInfo = EventLocation;
            RetValue.UniqueID = EventIDEntry;
            RetValue.UiParams = UiData;
            RetValue.DataBlob = NoteData;
            RetValue.Complete = CompletionFlag;
            RetValue.RepetitionSequence = RepeatIndex;
            RetValue.OriginalStart = OriginalStartData;
            RetValue.Splits = EventSplit;
            RetValue.TimePerSplit = TimeSpan.FromTicks(((RetValue.EventDuration.Ticks / RetValue.Splits)));
            RetValue.InitializingTimeSpanPerSplit = RetValue.TimePerSplit;
            RetValue.FromRepeatEvent = true;
            RetValue.NameOfEvent = new EventName(RetValue.UniqueID, EventName);
            /*
            if (RetValue.EventRepetition.Enable)
            {
                RetValue.Splits = EventSplit;
                RetValue.TimePerSplit = new TimeSpan();
            }
            else
            {
                RetValue.Splits = EventSplit;
            }
            */
            RetValue.SubEvents = new Dictionary<EventID, SubCalendarEvent>();

            if (!RetValue.EventRepetition.Enable)
            { 
                for (int i = 0; i < RetValue.Splits; i++)
                {
                    //(TimeSpan Event_Duration, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, Location EventLocation =null, TimeLine RangeOfSubCalEvent = null)
                    SubCalendarEvent newSubCalEvent = new SubCalendarEvent(RetValue.TimePerSplit, (RetValue.EndDateTime - RetValue.TimePerSplit), RetValue.End, new TimeSpan(), OriginalStartData, RetValue.UniqueID.ToString(), RetValue.RigidSchedule, RetValue.isEnabled, RetValue.UiParams, RetValue.Notes, RetValue.Complete, i+1, EventLocation, RetValue.RangeTimeLine);
                    RetValue.SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
                }
            }
            RetValue.EventSequence = new TimeLine(RetValue.StartDateTime, RetValue.EndDateTime);
            RetValue.RootOfRepeat = RepeatRootData;
            RetValue.UpdateLocationMatrix(RetValue.LocationInfo);
            

            while(! OrginalStartToCalendarEvent.TryAdd(OriginalStartData,RetValue))
            {
                Thread.Sleep(10);
            }
            return RetValue;
        }




        
        
        /// <summary>
        /// Calendarevent Identifies the subeevents that are to be used for the calculation of a schedule. You need to initialize this for calculation of schedule
        /// </summary>
        public void initializeCalculables()
        {
            CalculableSubEvents = new HashSet<SubCalendarEvent>(SubEvents.Values.Where(obj => obj.isInCalculationMode));
            isCalculableInitialized = true;
        }

        /// <summary>
        /// Function scans the current subevents and checks for the subevents that have the deviated flag set
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tuple<int, SubCalendarEvent>>getAllDeviatingSubEvents()
        {
            //List<SubCalendarEvent> RetValue = SubEvents.Values.Where(obj => obj.isDeviated).ToList();
            List<Tuple<int, TilerEvent>> RetValueAsTilerEvents = DeviatingSubCalendarEventInfo.getSubEvents();
            List<Tuple<int, SubCalendarEvent>> RetValue = RetValueAsTilerEvents.Select(obj => new Tuple<int, SubCalendarEvent>(obj.Item1, (SubCalendarEvent)obj.Item2)).ToList();
            return RetValue;
        }

        /// <summary>
        /// Function scans the current subevents and checks for the subevents that have the deviated flag set
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tuple<int, CalendarEvent>> getAllDeviatingCalendarEvents()
        {
            //List<SubCalendarEvent> RetValue = SubEvents.Values.Where(obj => obj.isDeviated).ToList();
            List<Tuple<int, TilerEvent>> RetValueAsTilerEvents = DeviatingSubCalendarEventInfo.getSubEvents();
            List<Tuple<int, CalendarEvent>> RetValue = RetValueAsTilerEvents.Select(obj => new Tuple<int, CalendarEvent>(obj.Item1, (CalendarEvent)obj.Item2)).ToList();
            return RetValue;
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
            //AllSubEvents.AsParallel().ForAll(obj => obj.NowInfo.reset());
        }


        public virtual void resetNowProfile()
        {
            IEnumerable<SubCalendarEvent> AllSubCalEvents = DeviatingSubCalendarEventInfo.getDeviationDataByType(2).Cast<SubCalendarEvent>();
            foreach(SubCalendarEvent eachSubcal in AllSubCalEvents )
            {
                //eachSubcal.resetNowProfile();
                DeviatingSubCalendarEventInfo.removeFromDeviatingData(DeviationType.NowProfile, eachSubcal.ID);
            }
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
            
            if (Id != null)
            {
                MyCalendarEventCopy.UniqueID = Id;
            }
            else
            {
                MyCalendarEventCopy.UniqueID = UniqueID;//hack
            }

            MyCalendarEventCopy.TimePerSplit = this.InitializingTimeSpanPerSplit;
            MyCalendarEventCopy.EventSequence = EventSequence.CreateCopy();
            MyCalendarEventCopy.SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            MyCalendarEventCopy.UiParams = this.UiParams.createCopy();
            MyCalendarEventCopy.DataBlob = this.DataBlob.createCopy();
            MyCalendarEventCopy.Enabled = this.Enabled;
            MyCalendarEventCopy.LocationInfo = LocationInfo;//hack you might need to make copy
            MyCalendarEventCopy.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            MyCalendarEventCopy.DeadlineElapsed = this.DeadlineElapsed;
            MyCalendarEventCopy.UserDeleted= this.UserDeleted;
            MyCalendarEventCopy.CompletedCount = this.CompletedCount;
            MyCalendarEventCopy.DeletedCount = this.DeletedCount;
            MyCalendarEventCopy.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            MyCalendarEventCopy.ProfileOfNow = this.NowInfo.CreateCopy();
            MyCalendarEventCopy.Semantics = this.Semantics.createCopy();
            MyCalendarEventCopy._UsedTime = this._UsedTime;

            int counter = 0;
            foreach (SubCalendarEvent eachSubCalendarEvent in this.SubEvents.Values)
            {
                MyCalendarEventCopy.SubEvents.Add(eachSubCalendarEvent.SubEvent_ID, eachSubCalendarEvent.createCopy(EventID.GenerateSubCalendarEvent(MyCalendarEventCopy.UniqueID, ++counter)));
            }

            //MyCalendarEventCopy;//.SchedulStatus = SchedulStatus;
            MyCalendarEventCopy.otherPartyID = otherPartyID == null ? null : otherPartyID.ToString();
            MyCalendarEventCopy.UserIDs = this.UserIDs.ToList();
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
            retValue.UpdateLocationMatrix(new Location_Elements());
            return retValue;
        }
        /// <summary>
        /// UPdates the deviating data set. 0 type = deleted, 1 type = completed, 2 = Now Profile
        /// </summary>
        /// <param name="type"></param>
        /// <param name="mySubcalendarEvent"></param>
        protected internal void updateDeviationList(DeviationType type, SubCalendarEvent mySubcalendarEvent)
        {
            DeviatingSubCalendarEventInfo.updateDeviatingData(type, mySubcalendarEvent);
            
            //DeviatingSubEvents.Add(mySubcalendarEvent.ID, mySubcalendarEvent);
            if (RootOfRepeat!=null)
            {
                RootOfRepeat.updateDeviationList(type,mySubcalendarEvent);
            }
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
            if (!SubEvent.Rigid)
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
            return this.ID+"::"+this.Start.ToString() + " - " + this.End.ToString();
        }


        public bool IsDateTimeWithin(DateTimeOffset DateTimeEntry)
        {
            return RangeTimeLine.IsDateTimeWithin(DateTimeEntry);
        }

        public void completeSubEvent(SubCalendarEvent mySubEvent)
        {
            if (!mySubEvent.isComplete)
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
            int NumberToBeNonComplete = mySubEvents.Where(obj => obj.isComplete).Count();
            int CurrentCount = CompletedCount - NumberToBeNonComplete;
            if (CurrentCount >=0)
            {
                CompletedCount = CurrentCount;
                mySubEvents.Where(obj => obj.isComplete).AsParallel().ForAll(obj => obj.nonCompleteWithoutUpdatingCalEvent());
                return;
            }
            throw new Exception("You are trying to complete more events than are avalable splits, check nonCompleteSubEvents");
        }

        public void completeSubEvents(IEnumerable<SubCalendarEvent> mySubEvents)
        {
            int NumberToBeEComplete = mySubEvents.Where(obj => !obj.isComplete).Count();
            int CurrentCount = CompletedCount + NumberToBeEComplete;

            if (CurrentCount <= (Splits-DeletedCount))
            {
                CompletedCount = CurrentCount;
                mySubEvents.Where(obj => !obj.isComplete).AsParallel().ForAll(obj => obj.completeWithoutUpdatingCalEvent());
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
            int CurrentCount=DeletedCount +NumberToBeDeleted;
            if (CurrentCount <= Splits)
            {
                DeletedCount = CurrentCount;
                mySubEvents.AsParallel().ForAll(obj => obj.disableWithoutUpdatingCalEvent());
                return;
            }

            throw new Exception("You are trying to delete more events than is available. Check disableSubEvents");
            
            //SubEvents[myEventID].disableWithoutUpdatingCalEvent();
        }

        /// <summary>
        /// Function triggeres the deletion of the calendar event. Note deletion is different from disable. Deletion sends a trigger to the deviation
        /// </summary>
        virtual public void delete(bool goDeep = true)
        {
            this.Enabled = false;
            setAsUserDeleted();
            if (goDeep)
            {
                DisableSubEvents(AllSubEvents);
            }
        }

        virtual public void deleteSubEvents(IEnumerable<SubCalendarEvent> mySubEvents)
        {
            DisableSubEvents(mySubEvents);
            foreach (SubCalendarEvent eachSubCalendarEvent in mySubEvents)
            {
                eachSubCalendarEvent.delete(this);
            }
            
        }

        static public void UpdateLocationMatrixFromCassandra(Dictionary<string, CalendarEvent> AllEvents)
        {
            foreach (CalendarEvent eachCalendarEvent in AllEvents.Select(obj => obj.Value))
            {
                eachCalendarEvent.UpdateLocationMatrix(eachCalendarEvent.Location);
            }
        }

        protected void UpdateLocationMatrix(Location_Elements newLocation)
        { 
            
            int i = 0;
            int j = 0;
            if (DistanceMatrixData == null)
            {
                DistanceMatrixData = new Dictionary<string, List<double>>();
            }
            if(Horizontal==null)
            {
                Horizontal= new List<Location_Elements>();
            }

            if(!DistanceMatrixData.ContainsKey(this.UniqueID.getCalendarEventComponent()))
            {
                string myCalString = this.UniqueID.getCalendarEventComponent();
                DistanceMatrixData.Add(myCalString, new List<double>());
                Horizontal.Add(newLocation);
                DistanceMatixKeys= DistanceMatrixData.Keys.ToList();
                foreach (string eachString in DistanceMatixKeys)
                {
                        
                        Location_Elements eachStringLocation=Horizontal[DistanceMatixKeys.IndexOf(eachString)];
                        double MyDistance = Location_Elements.calculateDistance(eachStringLocation, newLocation);

                        if (double.IsPositiveInfinity(MyDistance))
                        {
                            ;
                        }

                        if (eachString == this.UniqueID.getCalendarEventComponent())
                        {
                            //MyDistance = double.MaxValue / DistanceMatixKeys.Count;

                            MyDistance = 200000;

                            DistanceMatrixData[myCalString].Add(MyDistance);
                        }
                        else
                        {

                            DistanceMatrixData[eachString].Add(MyDistance);
                            DistanceMatrixData[myCalString].Add(MyDistance);
                        }


                        DistanceMatrixData[eachString][DistanceMatixKeys.IndexOf(eachString)] = 200000;
                        
                }
                
                
            }
        }

        public double getDistance(CalendarEvent CalEvent)
        {
            //get
            {
                return DistanceMatrixData[this.UniqueID.getCalendarEventComponent()][DistanceMatixKeys.IndexOf(CalEvent.UniqueID.getCalendarEventComponent())];
            }
        }

        public static Dictionary<string, double> DistanceToAllNodes(string CalEventstring)
        {
            if (CalEventstring == "")
            { 
                int randIndex=0;
                Random randNum= new Random();
                randIndex=randNum.Next(0,DistanceMatrixData.Count);
                CalEventstring = DistanceMatrixData.Keys.ToList()[randIndex];
            }
            Dictionary<string, double> retValue = new Dictionary<string, double>();
            List<Tuple<string, double>> AllCombos = new List<Tuple<string, double>>();

             //= this.CalendarEventID.getCalendarEventComponent(); ;
            for (int i = 0; i < DistanceMatixKeys.Count; i++)
            {

                AllCombos.Add(new Tuple<string, double>(DistanceMatixKeys[i], DistanceMatrixData[CalEventstring][i]));
            }
            AllCombos = AllCombos.OrderBy(obj => obj.Item2).ToList();

            foreach (Tuple<string, double> eachTUple in AllCombos)
            {
                retValue.Add(eachTUple.Item1, eachTUple.Item2);
            }

            return retValue;

        }


        public Dictionary<string, double> DistanceToAllNodes()
        {
            return DistanceToAllNodes(this.UniqueID.getCalendarEventComponent());
        }
        /*
        CalendarEvent getRepeatingCalendarEvent(string RepeatingEventID)
        {
            return EventRepetition.getCalendarEvent(RepeatingEventID);
        }
        */
        public static int CompareByEndDate(CalendarEvent CalendarEvent1, CalendarEvent CalendarEvent2)
        {
            return CalendarEvent1.End.CompareTo(CalendarEvent2.End);
        }

        public static int CompareByStartDate(CalendarEvent CalendarEvent1, CalendarEvent CalendarEvent2)
        {
            return CalendarEvent1.Start.CompareTo(CalendarEvent2.Start);
        }
        /*
        virtual public TimeLine PinToEnd(TimeLine MyTimeLine, List<SubCalendarEvent> MySubCalendarEventList)
        {
            //Name: Jerome Biotidara
            //Description: This funciton is only called when the Timeline can fit the total timeline ovcupied by the Subcalendarevent. essentially it tries to pin itself to the last available spot
             
            TimeSpan SubCalendarTimeSpan = new TimeSpan();
            foreach (SubCalendarEvent MySubCalendarEvent in MySubCalendarEventList)
            {
                SubCalendarTimeSpan.Add(MySubCalendarEvent.ActiveSlot.BusyTimeSpan);//  you might be able to combine the implementing for lopp with this in order to avoid several loops
            }
            if (SubCalendarTimeSpan > MyTimeLine.TimelineSpan)
            {
                throw new Exception("Oh oh check PinSubEventsToEnd Subcalendar is longer than timeline");
            }
            if (!MyTimeLine.IsDateTimeWithin(Start))
            {
                throw new Exception("Oh oh Calendar event isn't within Timeline range. Check PinSubEventsToEnd :(");
            }
            DateTimeOffset ReferenceTime = new DateTimeOffset();
            if (End > MyTimeLine.End)
            {
                ReferenceTime = MyTimeLine.End;
            }
            else
            {
                ReferenceTime = End;
            }
            List<BusyTimeLine> MyActiveSlot = new List<BusyTimeLine>();
            foreach (SubCalendarEvent MySubCalendarEvent in MySubCalendarEventList)
            {
                MySubCalendarEvent.PinToEndAndIncludeInTimeLine(MyTimeLine, this);//hack you need to handle cases where you cant shift subcalevent
            }

            
            return MyTimeLine;
        }
        */
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
        public CalendarEvent getRepeatedCalendarEvent(string RepeatCalendarEventID)
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
                return EventRepetition.getCalendarEvent(RepeatCalendarEventID);
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
                SubCalendarTimeSpan = SubCalendarTimeSpan.Add(MySubCalendarEvent.ActiveDuration);//  you might be able to combine the implementing for lopp with this in order to avoid several loops
            }
            TimeSpan TimeDifference = (ReferenceEndTime- ReferenceStartTime);

            if (this.Rigid)
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
                DateTimeOffset EndTIme = MyStartTime + MySubCalendarEvent.ActiveDuration;
                MySubCalendarEvent.shiftEvent((MyStartTime - MySubCalendarEvent.Start));
                //MySubCalendarEvent.ActiveSlot = new BusyTimeLine(MySubCalendarEvent.ID, (MyStartTime), EndTIme);
                ReferenceStartTime = EndTIme;
                MyActiveSlot.Add(MySubCalendarEvent.ActiveSlot);
            }

            MyTimeLine.OccupiedSlots = MyActiveSlot.ToArray();
            return MyTimeLine;
        }
        private class ConstructorModified
        {
            public string Name;
            public TimeSpan Duration;
            public DateTimeOffset StartDate;
            public DateTimeOffset EndDate;
            public TimeSpan PrepTime;
            public TimeSpan PreDeadline;
            public bool Rigid;
            public Repetition Repeat;
            public int Split;//Make Sure this is UInt
            public EventID CalendarEventID;
            public bool Enabled;
            public EventDisplay ui;
            public bool complete;
            public MiscData noteData;
            
            public Location_Elements CalendarEventLocation;

            public ConstructorModified(string EventIDEntry, string NameEntry, string StartTime, DateTimeOffset StartDateEntry, string EndTime, DateTimeOffset EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetition, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag, bool EnabledEventFlag, EventDisplay UiData, MiscData NoteData, bool CompletionFlag)
            {
                CalendarEventID = new EventID(EventIDEntry);
                Enabled = EnabledEventFlag;
                Name = NameEntry;
                EventDuration=EventDuration.Replace(".", ":");
                //EventDuration = EventDuration + ":00";
                string MiltaryStartTime = convertTimeToMilitary(StartTime);
                StartDate = new DateTimeOffset(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(MiltaryStartTime.Split(':')[0]), Convert.ToInt32(MiltaryStartTime.Split(':')[1]), 0, new TimeSpan());
                string MiltaryEndTime = convertTimeToMilitary(EndTime);
                EndDate = new DateTimeOffset(EventEndDateEntry.Year, EventEndDateEntry.Month, EventEndDateEntry.Day, Convert.ToInt32(MiltaryEndTime.Split(':')[0]), Convert.ToInt32(MiltaryEndTime.Split(':')[1]), 0, new TimeSpan());
                
                string[] TimeDuration = EventDuration.Split(':');
                double AllMinutes = TimeSpan.Parse(EventDuration).TotalMinutes;
                Duration = TimeSpan.Parse(EventDuration);

                if (RigidScheduleFlag)//enforces rigid restriction
                {
                    Duration = TimeSpan.Parse(EventDuration);
                }
                Split = Convert.ToInt32(eventSplit);

                ui = UiData;
                complete = CompletionFlag;
                noteData = NoteData;



                if (PreDeadlineFlag)
                {
                    PreDeadline = new TimeSpan(((int)AllMinutes % 10) * 60);
                }
                else
                {
                    PreDeadline = new TimeSpan(ConvertToMinutes(PreDeadlineTime) * 60 * 10000000);
                }
                if (DefaultPrepTimeflag)
                {
#if (SetDefaultPreptimeToZero)
                    PrepTime = new TimeSpan(0);
#else
                    PrepTime = new TimeSpan((long)((15 * 60)*10000000));
#endif
                }
                else
                {
                    //uint MyNumber = Convert.ToInt32(eventPrepTime);
                    PrepTime = new TimeSpan((long)ConvertToMinutes(eventPrepTime) * 60 * 10000000);
                }
                Rigid = RigidScheduleFlag;
                Repeat = EventRepetition;
            }

            public ConstructorModified(string NameEntry, DateTimeOffset StartData, DateTimeOffset EndData, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetition, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag, bool EnabledEventFlag, EventDisplay UiData, MiscData NoteData, bool CompletionFlag)
            {
                Name = NameEntry;//.Split(',')[0];

                StartDate = StartData;
                EndDate = EndData;
                //
                //string[] TimeDuration = EventDuration.Split(':');
                double AllMinutes = TimeSpan.Parse(EventDuration).TotalMinutes;
                Duration = new TimeSpan((int)(AllMinutes / 60), (int)(AllMinutes % 60), 0);
                if (RigidScheduleFlag)//enforces rigid restriction
                {
                    Duration = TimeSpan.Parse(EventDuration);
                }


                Split = Convert.ToInt32(eventSplit);
                Enabled = EnabledEventFlag;

                if (PreDeadlineFlag)
                {
                    PreDeadline = new TimeSpan(((int)AllMinutes % 10) * 60);
                }
                else
                {
                    PreDeadline = new TimeSpan(Convert.ToInt64(PreDeadlineTime));
                }

                ui = UiData;
                complete = CompletionFlag;
                noteData = NoteData;

                if (DefaultPrepTimeflag)
                {
#if (SetDefaultPreptimeToZero)
                    PrepTime = new TimeSpan(0);
#else
                    PrepTime = new TimeSpan((long)((15 * 60)*10000000));
#endif
                }
                else
                {
                    //uint MyNumber = Convert.ToInt32(eventPrepTime);
                    PrepTime = new TimeSpan((long)ConvertToMinutes(eventPrepTime) * 60 * 10000000);
                }
                Rigid = RigidScheduleFlag;
                Repeat = EventRepetition;
            }


















            public ConstructorModified(string NameEntry, string StartTime, DateTimeOffset StartDateEntry, string EndTime, DateTimeOffset EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetition, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag, bool EnabledEventFlag, EventDisplay UiData, MiscData NoteData, bool CompletionFlag)
            {
                Name = NameEntry;//.Split(',')[0];
                
                //EventDuration = EventDuration
                string MiltaryStartTime = convertTimeToMilitary(StartTime);
                StartDate = new DateTimeOffset(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(MiltaryStartTime.Split(':')[0]), Convert.ToInt32(MiltaryStartTime.Split(':')[1]), 0, new TimeSpan());
                string MiltaryEndTime = convertTimeToMilitary(EndTime);
                EndDate = new DateTimeOffset(EventEndDateEntry.Year, EventEndDateEntry.Month, EventEndDateEntry.Day, Convert.ToInt32(MiltaryEndTime.Split(':')[0]), Convert.ToInt32(MiltaryEndTime.Split(':')[1]), 0, new TimeSpan());
                //
                //string[] TimeDuration = EventDuration.Split(':');
                double AllMinutes = TimeSpan.Parse(EventDuration).TotalMinutes;
                Duration = new TimeSpan((int)(AllMinutes / 60), (int)(AllMinutes % 60), 0);
                if (RigidScheduleFlag)//enforces rigid restriction
                {
                    Duration = TimeSpan.Parse(EventDuration);
                }


                Split = Convert.ToInt32(eventSplit);
                Enabled = EnabledEventFlag;
                
                if (PreDeadlineFlag)
                {
                    PreDeadline = new TimeSpan(((int)AllMinutes % 10) * 60);
                }
                else
                {
                    PreDeadline = new TimeSpan(Convert.ToInt64(PreDeadlineTime));
                }

                ui = UiData;
                complete = CompletionFlag;
                noteData = NoteData;

                if (DefaultPrepTimeflag)
                {
#if (SetDefaultPreptimeToZero)
                    PrepTime = new TimeSpan(0);
#else
                    PrepTime = new TimeSpan((long)((15 * 60)*10000000));
#endif
                }
                else
                {
                    //uint MyNumber = Convert.ToInt32(eventPrepTime);
                    PrepTime = new TimeSpan((long)ConvertToMinutes(eventPrepTime) * 60 * 10000000);
                }
                Rigid = RigidScheduleFlag;
                Repeat = EventRepetition;
            }

            static public uint ConvertToMinutes(string TimeEntry)
            {
                int MaxTimeIndexCounter = 5;
                string[] ArrayOfTimeComponent = TimeEntry.Split(':');
                Array.Reverse(ArrayOfTimeComponent);
                uint TotalMinutes = 0;
                for (int x = 0; x < ArrayOfTimeComponent.Length; x++)
                {
                    int Multiplier = 0;
                    switch (x)
                    {
                        case 0:
                            Multiplier = 0;
                            break;
                        case 1:
                            Multiplier = 1;
                            break;
                        case 2:
                            Multiplier = 60;
                            break;
                        case 3:
                            Multiplier = 36 * 24;
                            break;
                        case 4:
                            Multiplier = 36 * 24 * 365;
                            break;
                    }
                    string JustHold = ArrayOfTimeComponent[x];
                    Int64 MyNumber = (Int64)Convert.ToDouble(JustHold);
                    TotalMinutes = (uint)(TotalMinutes + (Multiplier * MyNumber));

                }

                return TotalMinutes;

            }

            string convertTimeToMilitary(string TimeString)
            {
               // TimeString = (DateTimeOffset.Parse(TimeString)).ToString("YYYY-MM-DD HH:mm:ss");
                DateTimeOffset ParsedTime = (DateTimeOffset.Parse(TimeString));
                TimeString = ParsedTime.ToString("HH:mm");
                return TimeString;
                
                TimeString = TimeString.Replace(" ", "").ToUpper();
                string[] TimeIsolated = TimeString.Split(':');
                if (TimeIsolated.Length == 2)//checks if time is in format HH:MMAM as opposed to HH:MM:SSAM 
                {
                    char AorP = TimeIsolated[1][2];
                    TimeIsolated[1] = TimeIsolated[1].Substring(0, 2) + ":00" + AorP + "M";
                    return convertTimeToMilitary(TimeIsolated[0] + ":" + TimeIsolated[1]);

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




        /*
        public void DisableSubEvents(IEnumerable<SubCalendarEvent> ElementsToBeRemoved)
        {
                      

            Parallel.ForEach(ElementsToBeRemoved, eachSubCalendarEvent => { SubEvents[eachSubCalendarEvent.SubEvent_ID].Dis(false); });

            
        }
        */
        /*
        public void EnableSubEvents(IEnumerable<SubCalendarEvent> ElementsToBeRemoved)
        {
            
             * Function replaces sets the enable flag of the subevents as true
             

            Parallel.ForEach(ElementsToBeRemoved, eachSubCalendarEvent => { SubEvents[eachSubCalendarEvent.SubEvent_ID].SetEventEnableStatus(true); });
            if (ActiveSubEvents.Count() > 0)
            {
                Enabled = true;
            }
        }
            */
        /*
        public bool replaceNullSubCalevents(List<SubCalendarEvent> ElementsToBeRemoved)
        {
            ///\*
             * Function replaces null sub cal events. It is only to be called after a preceeding call to removeSubCalEvents. This simply replaces them
             *\/

            int LastDetectedIndex = ArrayOfSubEvents.ToList().IndexOf(null);
            do
            {
                if (LastDetectedIndex > -1)
                {

                    SubCalendarEvent NewSubCalEvent = new SubCalendarEvent(RemovedIDs[0].Item1.ToString(), ElementsToBeRemoved[0].Start, ElementsToBeRemoved[0].End, ElementsToBeRemoved[0].ActiveSlot, ElementsToBeRemoved[0].Rigid, ElementsToBeRemoved[0].myLocation, this.RangeTimeLine);

                    string thirdPartyID = RemovedIDs[0].Item2;
                    ArrayOfSubEvents[LastDetectedIndex] = NewSubCalEvent;
                    ArrayOfSubEvents[LastDetectedIndex].ThirdPartyID = thirdPartyID;
                    ElementsToBeRemoved.Remove(ElementsToBeRemoved[0]);
                    RemovedIDs.RemoveAt(0);
                }
                LastDetectedIndex = ArrayOfSubEvents.ToList().IndexOf(null);
            }
            while ((LastDetectedIndex > -1) && (ElementsToBeRemoved.Count > 0));


            return !((LastDetectedIndex < 0) && (ElementsToBeRemoved.Count > 0));
        }*/

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
            if ((this.ID == CalendarEventEntry.ID))
            {
                EventDuration=CalendarEventEntry.Duration;
                NameOfEvent=CalendarEventEntry.NameOfEvent;
                StartDateTime=CalendarEventEntry.StartDateTime;
                EndDateTime=CalendarEventEntry.EndDateTime;
                EventPreDeadline=CalendarEventEntry.PreDeadline;
                PrepTime=CalendarEventEntry.PrepTime;
                Priority=CalendarEventEntry.Priority;
                //RepetitionFlag=CalendarEventEntry.RepetitionFlag;
                EventRepetition=CalendarEventEntry.EventRepetition;
                Complete = CalendarEventEntry.Complete;
                RigidSchedule = CalendarEventEntry.RigidSchedule;
                Splits=CalendarEventEntry.Splits;
                TimePerSplit=CalendarEventEntry.TimePerSplit;
                InitializingTimeSpanPerSplit = CalendarEventEntry.InitializingTimeSpanPerSplit;
                UniqueID =CalendarEventEntry.UniqueID;
                EventSequence=CalendarEventEntry.EventSequence;;
                SubEvents=CalendarEventEntry.SubEvents;
                //SchedulStatus=CalendarEventEntry.SchedulStatus;
                CalendarError = CalendarEventEntry.CalendarError;
                Enabled=CalendarEventEntry.Enabled;
                UiParams=CalendarEventEntry.UiParams;
                DataBlob=CalendarEventEntry.DataBlob;
                LocationInfo =CalendarEventEntry.LocationInfo;
                otherPartyID = CalendarEventEntry.otherPartyID;
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
            
            List<DayTimeLine> retValue = AllFreeDayTIme.Where(obj => obj.UniversalIndex >= ProcrastinationInfo.PreferredDayIndex).ToList();
            return retValue;
        }

        public List<DayTimeLine> getTimeLineWithoutMySubEventsAndEnoughDuration(bool forceUpdateFreeTimeLine = true, List<DayTimeLine> AllFreeDayTIme = null)
        {

            AllFreeDayTIme = AllFreeDayTIme ?? FreeDaysLimitation.Values.ToList();
            if (forceUpdateFreeTimeLine)
            {
                AllFreeDayTIme.AsParallel().ForAll(obj => { obj.updateOccupancyOfTimeLine(); });
            }

            List<DayTimeLine> retValue = AllFreeDayTIme.Where(obj => obj.TotalFreeSpace > TimePerSplit).ToList();
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

            List<DayTimeLine> retValue = AllFreeDayTIme.Where(obj => obj.TotalFreeSpace > TimePerSplit).ToList();
            return retValue;
        }


        public static long getUsableDaysTotal(IEnumerable<CalendarEvent> AllCalendarEvents)
        {
            long retValue = AllCalendarEvents.Sum(obj => obj.CalculationLimitation.Count);
            return retValue;
        }

        public static long getTotalUndesignatedEvents(IEnumerable<CalendarEvent> AllCalendarEvents)
        {
            List<SubCalendarEvent> UnassignedEvents = AllCalendarEvents.Where(obj => !obj.Rigid).SelectMany(obj => obj.UnDesignables).ToList();
            long retValue = AllCalendarEvents.Where(obj=>!obj.Rigid).Sum(obj => obj.UnDesignables.Count);
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
            RetValue.InitializingTimeSpanPerSplit = this.InitializingTimeSpanPerSplit;
            RetValue.UniqueID = EventID.GenerateCalendarEvent();
            RetValue.SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            RetValue.UiParams = this.UIParam;
            RetValue.DataBlob = this.Notes;
            RetValue.Enabled = this.isEnabled;
            RetValue.LocationInfo = this.Location;//hack you might need to make copy
            RetValue.ProfileOfProcrastination = this.ProcrastinationInfo.CreateCopy();
            RetValue.DeadlineElapsed = this.isDeadlineElapsed;
            RetValue.UserDeleted = this.isUserDeleted;
            RetValue.CompletedCount = this.CompletionCount;
            RetValue.DeletedCount = this.DeletionCount;
            RetValue.ProfileOfNow = this.ProfileOfNow.CreateCopy();
            RetValue.otherPartyID = this.ThirdPartyID;// == this.null ? null : otherPartyID.ToString();
            RetValue.UserIDs = this.getAllUserIDs();//.ToList();
            RetValue.UpdateLocationMatrix(RetValue.LocationInfo);
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

        virtual public CalendarEvent getNowCalculationCopy(NowProfile NowProfileData,SubCalendarEvent RefSubCalEvent )
        {
            CalendarEvent retValue = getCalculationCopy();
            retValue.ProfileOfNow = NowProfileData;
            retValue.StartDateTime = NowProfileData.PreferredTime;
            retValue.EventSequence = new TimeLine(retValue.StartDateTime, retValue.EndDateTime);
            SubCalendarEvent subEventToBeNowCopy = RefSubCalEvent.getNowCopy(retValue.UniqueID, NowProfileData);
            updateDeviationList(DeviationType.NowProfile, RefSubCalEvent);
            retValue.EndDateTime = subEventToBeNowCopy.End;
            retValue.RigidSchedule = true;
            retValue.SubEvents.Add(subEventToBeNowCopy.SubEvent_ID, subEventToBeNowCopy);
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
                SplitCOunt = Math.Abs(SplitCOunt);
                int delta = (SplitCOunt - NumberOfSplit);
                uint Change = (uint) delta;
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

        void ReduceSplitCount(uint delta)
        {
            if (delta<Splits)
            {
                for(int i=0; i<delta;i++)
                {
                    SubCalendarEvent SubEvent = SubEvents.Last().Value;
                    SubEvents.Remove(SubEvent.SubEvent_ID);
                }
                Splits -= (int)delta;    
                return;
            }
            throw new Exception("You are trying to reduce the number of subevents past the min count");
        }

        void IncreaseSplitCount(uint delta)
        {
            List<SubCalendarEvent> newSubs = new List<SubCalendarEvent>();
            for (int i = 0; i < delta; i++)
            {
                SubCalendarEvent newSubCalEvent = new SubCalendarEvent(InitializingTimeSpanPerSplit, (EndDateTime - InitializingTimeSpanPerSplit), this.End, new TimeSpan(), this.OriginalStart, UniqueID.ToString(), RigidSchedule, this.isEnabled, this.UiParams, this.Notes, this.Complete, i, LocationInfo, this.RangeTimeLine);
                SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
            }
            Splits += (int)delta;
        }

        public short ChangeTimePerSplit(TimeSpan newTimePerSplit)
        {
            short retValue = 0;
            if (TimePerSplit == newTimePerSplit)
            {
                return retValue;
            }
            else
            {
                if (TimePerSplit < newTimePerSplit)
                {
                    retValue = 2;
                }
                else 
                {
                    retValue = 1;
                }

                TimeSpan Delta = newTimePerSplit - TimePerSplit;
                TimePerSplit = newTimePerSplit;
                ActiveSubEvents.AsParallel().ForAll(obj => obj.changeDurartion(Delta));
            }
            return retValue;
        }

        public void updateUnusableDaysAndRemoveDaysWithInsufficientFreeSpace()
        {
            updateUnusableDays();
            removeDayTimeLinesWithInsufficientSpace();
        }
        

        public void removeDayTimeLinesWithInsufficientSpace()
        {
            List<DayTimeLine> DaysWithInSufficientSpace=CalculationLimitation.Values.Where(obj => obj.TotalFreeSpace < TimePerSplit).ToList();
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
        

        public void updateTimeLine(TimeLine newTImeLine)
        {
            StartDateTime = newTImeLine.Start;
            EndDateTime = newTImeLine.End;
            ActiveSubEvents.AsParallel().ForAll(obj => obj.changeTimeLineRange(newTImeLine));
        }

        public override void updateRepetitionIndex(long RepetitionIndex)
        {
            RepetitionSequence = RepetitionIndex;
            AllSubEvents.AsParallel().ForAll(obj => obj.updateRepetitionIndex(RepetitionIndex));
        }

        #endregion

        #region Properties

        

        
        
        public string ID
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

        

        //public virtual string ThirdPartyID
        //{
        //    set
        //    {
        //        otherPartyID = value;
        //    }
        //    get
        //    {
        //        return otherPartyID;
        //    }
        //}
        
        public TimeSpan TimeLeftBeforeDeadline
        {
            get
            {
                return EndDateTime - DateTimeOffset.Now;
            }
        }
        virtual public DateTimeOffset Start
        {
            get
            {
                return StartDateTime;
            }
        }
        virtual public DateTimeOffset End
        {
            get
            {
                return EndDateTime;
            }
        }
        public int NumberOfSplit
        {
            get
            {
                return Splits;
            }
        }
        virtual public bool Rigid
        {
            //set 
            //{
            //    RigidSchedule = value;
            //}
            get
            {
                return RigidSchedule;
            }
        }
        public bool RepetitionStatus
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

        public TimeSpan EachSplitTimeSpan
        {
            get
            {
                return TimePerSplit;
            }
        }

        /*
        public Procrastination ProcrastinationProfile
        {
            get 
            {
                return ProfileOfProcrastination;
            }
        }

        */
        
        public TimeSpan Duration
        {
            get
            {

                return TimeSpan.FromTicks( Splits * TimePerSplit.Ticks);
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


        public SubCalendarEvent[] AllSubEvents
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


        public bool ErrorStatus
        {
            get 
            {
                return CalendarError.Status;
            }
        }

        public string ErrorMessage
        {
            get
            {
                return CalendarError.Message;
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
        virtual public TimeLine RangeTimeLine
        {
            get
            {
                //updateEventSequence();
                EventSequence = new TimeLine(this.Start, this.End);
                return EventSequence;
            }
        }

        public  void InitializeCounts(int Deletion, int Completion)
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
            CalendarError = new CustomErrors(false, string.Empty);
        }

        public TimeSpan RangeSpan
        {
            get
            {
                return this.RangeTimeLine.TimelineSpan;
            }
        }

        static public Dictionary<string, List<Double>> DistanceMatrix
        {
            get
            {
                return DistanceMatrixData;
            }
        }
        

        virtual public Event_Struct toEvent_Struct
        {
            get
            {
                Event_Struct retValue = new Event_Struct();
                retValue.EventLocation = Location.toStruct();
                return retValue;
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

        public NowProfile NowInfo
        {
            get
            {
                return ProfileOfNow;
            }
        }

        #endregion

    }

    public struct Event_Struct
    {
        //public string EventID;
        public float Testint;
        //public long StartTicks;
        //public long EndTicks;
        //public long DurationTicks;
        public Location_struct EventLocation;
        //public bool Enabled;
        public Event_Struct(float number, Location_struct locationData)
        {
            Testint = number;
            EventLocation = locationData;
        }

    }
}
