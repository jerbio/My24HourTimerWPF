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
        // Fields
        public static Dictionary<string, List<Double>> DistanceMatrix;
        static List<string> DistanceMatixKeys;
        static List<Location_Elements> Horizontal;
        protected Repetition EventRepetition;
        protected int Splits;
        protected TimeSpan TimePerSplit;
//        protected bool FromRepetion=false;
        protected Dictionary<EventID, SubCalendarEvent> SubEvents;
        protected bool SchedulStatus;
        CustomErrors CalendarError = new CustomErrors(false, string.Empty);
        List<mTuple<EventID,string>> RemovedIDs;
        #region Constructor
        public CalendarEvent(CustomErrors Error)
        {
            EventDuration = new TimeSpan();
            EventName = "";
            StartDateTime = new DateTime();
            EndDateTime = new DateTime();
            EventPreDeadline = new TimeSpan();
            PrepTime = new TimeSpan();
            Priority = 0;
            RepetitionFlag = false;
            EventRepetition = new Repetition();
            RigidSchedule = false;
            Splits = 1;
            LocationData = new Location_Elements();
            UniqueID = new EventID("");
            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            SchedulStatus = false;
            otherPartyID = "";
            CalendarError = Error;
            EventSequence = new TimeLine();
        }


        public CalendarEvent()
        {
            EventDuration = new TimeSpan();
            EventName = "";
            StartDateTime = new DateTime();
            EndDateTime = new DateTime();
            EventPreDeadline = new TimeSpan();
            PrepTime = new TimeSpan();
            Priority = 0;
            RepetitionFlag = false;
            EventRepetition = new Repetition();
            RigidSchedule = false;
            Splits = 1;
            LocationData = new Location_Elements();
            UniqueID = new EventID("");
            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            SchedulStatus = false;
            otherPartyID = "";
            CalendarError = new CustomErrors(false, string.Empty);
            EventSequence = new TimeLine();
        }

        //CalendarEvent MyCalendarEvent = new CalendarEvent(NameEntry, Duration, StartDate, EndDate, PrepTime, PreDeadline, Rigid, Repeat, Split);
        public CalendarEvent(string EventIDEntry, string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag, Location_Elements EventLocation, bool EnableFlag, EventDisplay UiData, MiscData NoteData, bool CompletionFlag)
            : this(new ConstructorModified(EventIDEntry, NameEntry, StartTime, StartDateEntry, EndTime, EventEndDateEntry, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionEntry, DefaultPrepTimeflag, RigidScheduleFlag, eventPrepTime, PreDeadlineFlag, EnableFlag,  UiData,  NoteData, CompletionFlag), new EventID(EventIDEntry), EventLocation)
        { }
        public CalendarEvent(string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag,Location_Elements EventLocation,bool EnabledEventFlag, EventDisplay UiData,MiscData NoteData,bool CompletionFlag)
            : this(new ConstructorModified(NameEntry, StartTime, StartDateEntry, EndTime, EventEndDateEntry, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionEntry, DefaultPrepTimeflag, RigidScheduleFlag, eventPrepTime, PreDeadlineFlag, EnabledEventFlag,  UiData, NoteData, CompletionFlag), EventLocation)
        {
        }
        public CalendarEvent(CalendarEvent MyUpdated, SubCalendarEvent[] MySubEvents)
        {
            EventName = MyUpdated.Name;
            
            StartDateTime = MyUpdated.StartDateTime;
            EndDateTime = MyUpdated.End;
            EventSequence = new TimeLine(StartDateTime, EndDateTime);
            EventDuration = MyUpdated.ActiveDuration;
            Splits = MyUpdated.Splits;
            PrepTime = MyUpdated.PrepTime;
            EventPreDeadline = MyUpdated.PreDeadline;
            RigidSchedule = MyUpdated.Rigid;
            TimePerSplit = MyUpdated.TimePerSplit;
            if (MyUpdated.ID != null)
            {
                UniqueID = new EventID(MyUpdated.ID);
            }
            Enabled = MyUpdated.isEnabled;

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

            SchedulStatus = false;
            EventRepetition = MyUpdated.Repeat;
            LocationData = MyUpdated.LocationData;
            UpdateLocationMatrix(LocationData);
            EventSequence = new TimeLine(StartDateTime, EndDateTime);
        }
        private CalendarEvent(ConstructorModified UpdatedConstructor, EventID MyEventID, Location_Elements EventLocation=null)
            : this(MyEventID, UpdatedConstructor.Name, UpdatedConstructor.Duration, UpdatedConstructor.StartDate, UpdatedConstructor.EndDate, UpdatedConstructor.PrepTime, UpdatedConstructor.PreDeadline, UpdatedConstructor.Rigid, UpdatedConstructor.Repeat, UpdatedConstructor.Split, EventLocation, UpdatedConstructor.Enabled, UpdatedConstructor.ui, UpdatedConstructor.noteData, UpdatedConstructor.complete)
        {
        }
        private CalendarEvent(ConstructorModified UpdatedConstructor, Location_Elements EventLocation)
            : this(UpdatedConstructor.Name, UpdatedConstructor.Duration, UpdatedConstructor.StartDate, UpdatedConstructor.EndDate, UpdatedConstructor.PrepTime, UpdatedConstructor.PreDeadline, UpdatedConstructor.Rigid, UpdatedConstructor.Repeat, UpdatedConstructor.Split, EventLocation,UpdatedConstructor.Enabled,UpdatedConstructor.ui,UpdatedConstructor.noteData,UpdatedConstructor.complete)
        {
        }
        public CalendarEvent(EventID EventIDEntry, string EventName, TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan Event_PreDeadline, bool EventRigidFlag, Repetition EventRepetitionEntry, int EventSplit, Location_Elements EventLocation, bool enabledFlag, EventDisplay UiData, MiscData NoteData, bool CompletionFlag)
        {
            EventName = EventName;
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = Event_Duration;
            Enabled = enabledFlag;
            EventRepetition = EventRepetitionEntry;
            PrepTime = EventPrepTime;
            EventPreDeadline = Event_PreDeadline;
            RigidSchedule = EventRigidFlag;
            LocationData = EventLocation;
            UniqueID = EventIDEntry;
            UiParams = UiData;
            DataBlob = NoteData;
            Complete = CompletionFlag;

            if (EventRepetition.Enable)
            {
                Splits = EventSplit;
                TimePerSplit = new TimeSpan();
            }
            else
            {
                Splits = EventSplit;
                TimePerSplit = TimeSpan.FromTicks(((EventDuration.Ticks / Splits)));
            }
            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            for (int i = 0; i < Splits; i++)
            {
                //(TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, Location EventLocation =null, TimeLine RangeOfSubCalEvent = null)
                SubCalendarEvent newSubCalEvent = new SubCalendarEvent(TimePerSplit, (EndDateTime - TimePerSplit), this.End, new TimeSpan(), UniqueID.ToString(), RigidSchedule,this.isEnabled, this.UiParams,this.Notes,this.Complete, EventLocation, this.RangeTimeLine);
                SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
            }

            EventSequence = new TimeLine(StartDateTime, EndDateTime);
            UpdateLocationMatrix(EventLocation);
        }
        public CalendarEvent(string EventName, TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan Event_PreDeadline, bool EventRigidFlag, Repetition EventRepetitionEntry, int EventSplit, Location_Elements EventLocation, bool EnableFlag, EventDisplay UiData, MiscData NoteData, bool CompletionFlag)
        {
            EventName = EventName;
            /*CalendarEventName = EventName.Split(',')[0];
            LocationString = "";
            if (EventName.Split(',').Length > 1)
            {
                LocationString = EventName.Split(',')[1];
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
            LocationData = EventLocation;
            UniqueID = EventID.GenerateCalendarEvent();
            EventRepetition = EventRepetitionEntry;

            UiParams = UiData;
            DataBlob = NoteData;
            Complete = CompletionFlag;


            if (EventRepetition.Enable)
            {
                Splits = EventSplit;
                TimePerSplit = new TimeSpan();
            }
            else
            {
                Splits = EventSplit;
                TimePerSplit = TimeSpan.FromTicks(((EventDuration.Ticks / Splits)));
            }

            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            for (int i = 0; i < Splits; i++)
            {
                SubCalendarEvent newSubCalEvent = new SubCalendarEvent(TimePerSplit, (EndDateTime - TimePerSplit), this.End, new TimeSpan(), UniqueID.ToString(), RigidSchedule, this.Enabled, this.UiParams, this.Notes, this.Complete, EventLocation, this.RangeTimeLine); //new SubCalendarEvent(CalendarEventID);
                SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
            }

            
            
            EventSequence = new TimeLine(StartDateTime, EndDateTime);
            UpdateLocationMatrix(LocationData);
        }
        #endregion

        #region Functions
        /*SubCalendarEvent[] generateSubEvent(SubCalendarEvent[] ArrayOfEvents, int NumberOfSplit, TimeSpan TotalActiveDurationSubEvents, string ParentID)
        {
            TimeSpan TimeSpanEvent = EndDateTime - StartDateTime;
            //new TimeSpan((long)((().TotalSeconds/ ArrayOfEvents.Length)*100000000));
            TimeSpanEvent = new TimeSpan(((long)TimeSpanEvent.TotalMilliseconds * 10000) / ArrayOfEvents.Length);
            TimeSpan ActiveDurationPerSubEvents = new TimeSpan((long)(((TotalActiveDurationSubEvents.TotalSeconds) * 10000000) / ArrayOfEvents.Length));
            DateTime SubStart;
            DateTime SubEnd;
            for (int i = 0; i < ArrayOfEvents.Length; i++)
            {
                SubStart = StartDateTime.AddSeconds(TimeSpanEvent.TotalSeconds * i);
                SubEnd = StartDateTime.AddSeconds(TimeSpanEvent.TotalSeconds * (i + 1));
                ArrayOfEvents[i] = new SubCalendarEvent(ActiveDurationPerSubEvents, SubStart, SubEnd, PrepTime, ParentID);
            }

            return ArrayOfEvents;
        }*/


        public CalendarEvent createCopy()
        {
            CalendarEvent MyCalendarEventCopy = new CalendarEvent();
            MyCalendarEventCopy.EventDuration = new TimeSpan(EventDuration.Ticks);
            MyCalendarEventCopy.EventName = EventName.ToString();
            MyCalendarEventCopy.StartDateTime = new DateTime(StartDateTime.Ticks);
            MyCalendarEventCopy.EndDateTime = new DateTime(EndDateTime.Ticks);
            MyCalendarEventCopy.EventPreDeadline = new TimeSpan(EventPreDeadline.Ticks);
            MyCalendarEventCopy.PrepTime = new TimeSpan(PrepTime.Ticks);
            MyCalendarEventCopy.Priority = Priority;
            MyCalendarEventCopy.RepetitionFlag = RepetitionFlag;
            MyCalendarEventCopy.EventRepetition = EventRepetition.CreateCopy();// EventRepetition != null ? EventRepetition.CreateCopy() : EventRepetition;
            MyCalendarEventCopy.Complete = this.Complete;
            MyCalendarEventCopy.RigidSchedule = RigidSchedule;//hack
            MyCalendarEventCopy.Splits = Splits;
            MyCalendarEventCopy.TimePerSplit = new TimeSpan(TimePerSplit.Ticks);
            MyCalendarEventCopy.UniqueID = UniqueID;//hack
            MyCalendarEventCopy.EventSequence = EventSequence.CreateCopy();
            MyCalendarEventCopy.SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            MyCalendarEventCopy.UiParams = this.UiParams.createCopy();
            MyCalendarEventCopy.DataBlob = this.DataBlob.createCopy();
            MyCalendarEventCopy.Enabled = this.Enabled;

            MyCalendarEventCopy.LocationData = LocationData;//hack you might need to make copy



            foreach (SubCalendarEvent eachSubCalendarEvent in this.SubEvents.Values)
            {
                MyCalendarEventCopy.SubEvents.Add(eachSubCalendarEvent.SubEvent_ID, eachSubCalendarEvent.createCopy());
            }

            MyCalendarEventCopy.SchedulStatus = SchedulStatus;
            MyCalendarEventCopy.otherPartyID = otherPartyID == null ? null : otherPartyID.ToString();
            return MyCalendarEventCopy;
        }

        public static CalendarEvent getEmptyCalendarEvent( EventID myEventID,DateTime Start=new DateTime(), DateTime End=new DateTime())
        {
            CalendarEvent retValue = new CalendarEvent();
            retValue.UniqueID = new EventID( myEventID.getCalendarEventID());
            retValue.StartDateTime = Start;
            retValue.EndDateTime = End;
            retValue.EventDuration = new TimeSpan(0);
            SubCalendarEvent emptySubEvent = SubCalendarEvent.getEmptyCalendarEvent();
            retValue.SubEvents.Add(emptySubEvent.SubEvent_ID, emptySubEvent);
            retValue.Splits = 1;
            retValue.Rigid = true;
            retValue.Complete = true;

            retValue.Enabled = false;
            retValue.UpdateLocationMatrix(new Location_Elements());
            return retValue;
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

        virtual public void SetCompletion(bool CompletionStatus, bool goDeep=false)
        {
            Complete = CompletionStatus;

            if (RepetitionStatus)
            {
                IEnumerable<CalendarEvent> AllrepeatingCalEvents=EventRepetition.RecurringCalendarEvents();
                foreach (CalendarEvent eachCalendarEvent in AllrepeatingCalEvents)
                {
                    eachCalendarEvent.SetCompletion(CompletionStatus);
                }
            }
            else
            {
                if(goDeep)
                { 
                    foreach (SubCalendarEvent eachSubCalendarEvent in AllSubEvents)
                    {
                        setSubEventCompletionStatus(CompletionStatus, eachSubCalendarEvent);
                    }
                }
            }


            UiParams.setCompleteUI(CompletionStatus);
        }


        public void setSubEventCompletionStatus(bool completionStatus,SubCalendarEvent mySubEVent)
        {
            if (ActiveSubEvents.Count() < 1)
            {
                Complete = true;//hack alert this can pose a problem if all events are not loaded into memory make a check if fully loaded into memory
            }
        }
        public override string ToString()
        {
            return this.ID+"::"+this.Start.ToString() + " - " + this.End.ToString();
        }


        public bool IsDateTimeWithin(DateTime DateTimeEntry)
        {
            return RangeTimeLine.IsDateTimeWithin(DateTimeEntry);
        }


        void UpdateLocationMatrix(Location_Elements newLocation)
        { 
            
            int i = 0;
            int j = 0;
            if (DistanceMatrix == null)
            {
                DistanceMatrix = new Dictionary<string, List<double>>();
            }
            if(Horizontal==null)
            {
                Horizontal= new List<Location_Elements>();
            }

            if(!DistanceMatrix.ContainsKey(this.UniqueID.getCalendarEventComponent()))
            {
                string myCalString = this.UniqueID.getCalendarEventComponent();
                DistanceMatrix.Add(myCalString, new List<double>());
                Horizontal.Add(newLocation);
                DistanceMatixKeys= DistanceMatrix.Keys.ToList();
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

                            DistanceMatrix[myCalString].Add(MyDistance);
                        }
                        else
                        {

                            DistanceMatrix[eachString].Add(MyDistance);
                            DistanceMatrix[myCalString].Add(MyDistance);
                        }


                        DistanceMatrix[eachString][DistanceMatixKeys.IndexOf(eachString)] = 200000;
                        
                }
                
                
            }
        }

        public double getDistance(CalendarEvent CalEvent)
        {
            //get
            {
                return DistanceMatrix[this.UniqueID.getCalendarEventComponent()][DistanceMatixKeys.IndexOf(CalEvent.UniqueID.getCalendarEventComponent())];
            }
        }

        public static Dictionary<string, double> DistanceToAllNodes(string CalEventstring)
        {
            if (CalEventstring == "")
            { 
                int randIndex=0;
                Random randNum= new Random();
                randIndex=randNum.Next(0,DistanceMatrix.Count);
                CalEventstring = DistanceMatrix.Keys.ToList()[randIndex];
            }
            Dictionary<string, double> retValue = new Dictionary<string, double>();
            List<Tuple<string, double>> AllCombos = new List<Tuple<string, double>>();

             //= this.CalendarEventID.getCalendarEventComponent(); ;
            for (int i = 0; i < DistanceMatixKeys.Count; i++)
            {

                AllCombos.Add(new Tuple<string, double>(DistanceMatixKeys[i], DistanceMatrix[CalEventstring][i]));
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

        virtual public TimeLine PinToEnd(TimeLine MyTimeLine, List<SubCalendarEvent> MySubCalendarEventList)
        {
            /*
             *Name: Jerome Biotidara
             *Description: This funciton is only called when the Timeline can fit the total timeline ovcupied by the Subcalendarevent. essentially it tries to pin itself to the last available spot
             */
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
            DateTime ReferenceTime = new DateTime();
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

        virtual public void ReassignTime(DateTime StartTime, DateTime EndTime)
        {
            StartDateTime = StartTime;
            EndDateTime = EndTime;
            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
        }

        public CalendarEvent getRepeatedCalendarEvent(string CalendarID)
        { 
            /*foreach(CalendarEvent MyCalendarEvent in EventRepetition.RecurringCalendarEvents)
            {
                if (MyCalendarEvent.ID == CalendarID)
                {
                    return MyCalendarEvent;
                }
            }
            return null;*/

            return EventRepetition.getCalendarEvent(CalendarID);
        }

        virtual public void SetEventEnableStatus(bool EnableDisableFlag)
        {
            this.Enabled = EnableDisableFlag;
        }
        
        virtual public TimeLine PinSubEventsToStart(TimeLine MyTimeLine, List<SubCalendarEvent> MySubCalendarEventList)
        {
            TimeSpan SubCalendarTimeSpan = new TimeSpan();
            DateTime ReferenceStartTime = new DateTime();
            DateTime ReferenceEndTime = new DateTime();
            
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
                DateTime MyStartTime = ReferenceStartTime;
                DateTime EndTIme = MyStartTime + MySubCalendarEvent.ActiveDuration;
                MySubCalendarEvent.ActiveSlot = new BusyTimeLine(MySubCalendarEvent.ID, (MyStartTime), EndTIme);
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
            public DateTime StartDate;
            public DateTime EndDate;
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

            public ConstructorModified(string EventIDEntry, string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetition, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag, bool EnabledEventFlag, EventDisplay UiData, MiscData NoteData, bool CompletionFlag)
            {
                CalendarEventID = new EventID(EventIDEntry);
                Enabled = EnabledEventFlag;
                Name = NameEntry;
                EventDuration=EventDuration.Replace(".", ":");
                //EventDuration = EventDuration + ":00";
                string MiltaryStartTime = convertTimeToMilitary(StartTime);
                StartDate = new DateTime(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(MiltaryStartTime.Split(':')[0]), Convert.ToInt32(MiltaryStartTime.Split(':')[1]), 0);
                string MiltaryEndTime = convertTimeToMilitary(EndTime);
                EndDate = new DateTime(EventEndDateEntry.Year, EventEndDateEntry.Month, EventEndDateEntry.Day, Convert.ToInt32(MiltaryEndTime.Split(':')[0]), Convert.ToInt32(MiltaryEndTime.Split(':')[1]), 0);
                
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
            public ConstructorModified(string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetition, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag, bool EnabledEventFlag, EventDisplay UiData, MiscData NoteData, bool CompletionFlag)
            {
                Name = NameEntry;//.Split(',')[0];
                
                //EventDuration = EventDuration
                string MiltaryStartTime = convertTimeToMilitary(StartTime);
                StartDate = new DateTime(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(MiltaryStartTime.Split(':')[0]), Convert.ToInt32(MiltaryStartTime.Split(':')[1]), 0);
                string MiltaryEndTime = convertTimeToMilitary(EndTime);
                EndDate = new DateTime(EventEndDateEntry.Year, EventEndDateEntry.Month, EventEndDateEntry.Day, Convert.ToInt32(MiltaryEndTime.Split(':')[0]), Convert.ToInt32(MiltaryEndTime.Split(':')[1]), 0);
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
               // TimeString = (DateTime.Parse(TimeString)).ToString("YYYY-MM-DD HH:mm:ss");
                DateTime ParsedTime = (DateTime.Parse(TimeString));
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
                    SubCalendarEvent NewSubCalEvent = new SubCalendarEvent(SubEventID.ToString(), UpdatedSubEvent.Start, UpdatedSubEvent.End, UpdatedSubEvent.ActiveSlot, UpdatedSubEvent.Rigid, UpdatedSubEvent.isEnabled, UpdatedSubEvent.UIParam, UpdatedSubEvent.Notes, UpdatedSubEvent.isComplete, UpdatedSubEvent.myLocation, this.RangeTimeLine);
                    SubCalendarEvent CurrentSubEvent = SubEvents[SubEventID];
                    NewSubCalEvent.ThirdPartyID = CurrentSubEvent.ThirdPartyID;
                    SubEvents[SubEventID] = NewSubCalEvent;//using method as opposed to the UpdateThis function because of the canexistwithintimeline function test in the UpdateThis function
                    return true;
                }
            }

            return false;
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

        public void DisableSubEvents(IEnumerable<SubCalendarEvent> ElementsToBeRemoved)
        {
            /*
             * Function replaces sets the enable flag of the subevents as false
             */

            Parallel.ForEach(ElementsToBeRemoved, eachSubCalendarEvent => { SubEvents[eachSubCalendarEvent.SubEvent_ID].SetEventEnableStatus(false); });

            if (ActiveSubEvents.Count() <1)
            {
                Enabled = false;
            }
        }

        public void EnableSubEvents(IEnumerable<SubCalendarEvent> ElementsToBeRemoved)
        {
            /*
             * Function replaces sets the enable flag of the subevents as true
             */

            Parallel.ForEach(ElementsToBeRemoved, eachSubCalendarEvent => { SubEvents[eachSubCalendarEvent.SubEvent_ID].SetEventEnableStatus(true); });
            if (ActiveSubEvents.Count() > 0)
            {
                Enabled = true;
            }
        }

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
        protected DateTime[] getActiveSlots()
        {
            return new DateTime[0];
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
                    EventSequence.MergeTimeLines(mySubCalendarEvent.RangeTimeLine);
                }
                
            }
        }


        public virtual void UpdateThis(CalendarEvent CalendarEventEntry)
        {
            if ((this.ID == CalendarEventEntry.ID))
            {
                EventDuration=CalendarEventEntry.ActiveDuration;
                EventName=CalendarEventEntry.Name;
                StartDateTime=CalendarEventEntry.StartDateTime;
                EndDateTime=CalendarEventEntry.EndDateTime;
                EventPreDeadline=CalendarEventEntry.PreDeadline;
                PrepTime=CalendarEventEntry.PrepTime;
                Priority=CalendarEventEntry.Priority;
                RepetitionFlag=CalendarEventEntry.RepetitionFlag;
                EventRepetition=CalendarEventEntry.EventRepetition;
                Complete = CalendarEventEntry.Complete;
                RigidSchedule = CalendarEventEntry.RigidSchedule;
                Splits=CalendarEventEntry.Splits;
                TimePerSplit=CalendarEventEntry.TimePerSplit;
                UniqueID=CalendarEventEntry.UniqueID;
                EventSequence=CalendarEventEntry.EventSequence;;
                SubEvents=CalendarEventEntry.SubEvents;
                SchedulStatus=CalendarEventEntry.SchedulStatus;
                CalendarError = CalendarEventEntry.CalendarError;
                Enabled=CalendarEventEntry.Enabled;
                UiParams=CalendarEventEntry.UiParams;
                DataBlob=CalendarEventEntry.DataBlob;
                LocationData =CalendarEventEntry.LocationData;
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

        

        public virtual string ThirdPartyID
        {
            set
            {
                otherPartyID = value;
            }
            get
            {
                return otherPartyID;
            }
        }
        public string Name
        {
            get
            {
                return EventName;
            }
        }
        public TimeSpan TimeLeftBeforeDeadline
        {
            get
            {
                return EndDateTime - DateTime.Now;
            }
        }
        virtual public DateTime Start
        {
            get
            {
                return StartDateTime;
            }
        }
        virtual public DateTime End
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
            set 
            {
                RigidSchedule = value;
            }
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
        

        
        
        public TimeSpan ActiveDuration
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

        virtual public Location_Elements myLocation
        {
            set
            {
                LocationData=value;
            }
            get
            {
                return LocationData;
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
                updateEventSequence();
                return EventSequence;
            }
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

        

        virtual public Event_Struct toEvent_Struct
        {
            get
            {
                Event_Struct retValue = new Event_Struct();
                //retValue.StartTicks = StartDateTime.Ticks;
                //retValue.EndTicks = EndDateTime.Ticks;
                //retValue.DurationTicks = EventDuration.Ticks;
                //retValue.EventID = ID;
                retValue.EventLocation = myLocation.toStruct();
                return retValue;
            }
        }


        

        virtual public MiscData Notes
        {
            get
            {
                return DataBlob;
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
