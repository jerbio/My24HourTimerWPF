#define SetDefaultPreptimeToZero
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace My24HourTimerWPF
{
    public class CalendarEvent
    {
        // Fields
        static Dictionary<string, List<Double>> DistanceMatrix;
        static List<string> DistanceMatixKeys;
        static List<Location> Horizontal;

        
        protected TimeSpan EventDuration;
        string CalendarEventName;
        protected DateTime StartDateTime;
        protected DateTime EndDateTime;
        protected TimeSpan EventPreDeadline;
        protected TimeSpan PrepTime;
        protected int Priority;
        protected bool RepetitionFlag;
        protected Repetition EventRepetition;
        //protected bool Completed = false;
        protected bool RigidSchedule;
        protected int Splits;
        protected TimeSpan TimePerSplit;
        protected EventID CalendarEventID;
        protected TimeLine EventSequence;
        SubCalendarEvent[] ArrayOfSubEvents;
        protected bool SchedulStatus;
        
        protected Location LocationData;
        protected string otherPartyID;
        #region Constructor
        public CalendarEvent()
        {
            EventDuration = new TimeSpan();
            CalendarEventName = "";
            StartDateTime = new DateTime();
            EndDateTime = new DateTime();
            EventPreDeadline = new TimeSpan();
            PrepTime = new TimeSpan();
            Priority = 0;
            RepetitionFlag = false;
            EventRepetition = new Repetition();
            RigidSchedule = false;
            Splits = 1;
            LocationData = new Location();
            CalendarEventID = new EventID("");
            ArrayOfSubEvents = new SubCalendarEvent[0];
            SchedulStatus = false;
            otherPartyID = "";
            
            EventSequence = new TimeLine();
        }

        //CalendarEvent MyCalendarEvent = new CalendarEvent(NameEntry, Duration, StartDate, EndDate, PrepTime, PreDeadline, Rigid, Repeat, Split);
        public CalendarEvent(string EventIDEntry, string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag, Location EventLocation)
            : this(new ConstructorModified(EventIDEntry, NameEntry, StartTime, StartDateEntry, EndTime, EventEndDateEntry, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionEntry, DefaultPrepTimeflag, RigidScheduleFlag, eventPrepTime, PreDeadlineFlag), new EventID(EventIDEntry.Split('_')), EventLocation)
        { }
        public CalendarEvent(string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag,Location EventLocation)
            : this(new ConstructorModified(NameEntry, StartTime, StartDateEntry, EndTime, EventEndDateEntry, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionEntry, DefaultPrepTimeflag, RigidScheduleFlag, eventPrepTime, PreDeadlineFlag), EventLocation)
        {
        }
        public CalendarEvent(CalendarEvent MyUpdated, SubCalendarEvent[] MySubEvents)
        {
            CalendarEventName = MyUpdated.Name;
            
            StartDateTime = MyUpdated.StartDateTime;
            EndDateTime = MyUpdated.End;
            EventSequence = new TimeLine(StartDateTime, EndDateTime);
            EventDuration = MyUpdated.ActiveDuration;
            Splits = MyUpdated.Splits;
            PrepTime = MyUpdated.PrepTime;
            EventPreDeadline = MyUpdated.PreDeadline;
            RigidSchedule = MyUpdated.Rigid;
            TimePerSplit = MyUpdated.TimePerSplit;
            ArrayOfSubEvents = new SubCalendarEvent[Splits];
            if (MyUpdated.ID != null)
            {
                CalendarEventID = new EventID(MyUpdated.ID.Split('_'));
            }
            //CalendarEventID = new EventID(new string[] { EventIDGenerator.generate().ToString() });
            //ArrayOfSubEvents = generateSubEvent(ArrayOfSubEvents, 4, EventDuration, CalendarEventID.ToString());
            ArrayOfSubEvents = MySubEvents;
            SchedulStatus = false;
            EventRepetition = MyUpdated.Repeat;
            LocationData = MyUpdated.LocationData;
            UpdateLocationMatrix(LocationData);
            EventSequence = new TimeLine(StartDateTime, EndDateTime);
            //EventRepetition = new Repetition(EventRepetition.Enable, this, EventRepetition.Range, EventRepetition.Frequency);
        }
        private CalendarEvent(ConstructorModified UpdatedConstructor, EventID MyEventID, Location EventLocation=null)
            : this(MyEventID, UpdatedConstructor.Name, UpdatedConstructor.Duration, UpdatedConstructor.StartDate, UpdatedConstructor.EndDate, UpdatedConstructor.PrepTime, UpdatedConstructor.PreDeadline, UpdatedConstructor.Rigid, UpdatedConstructor.Repeat, UpdatedConstructor.Split, EventLocation)
        {
        }
        private CalendarEvent(ConstructorModified UpdatedConstructor, Location EventLocation)
            : this(UpdatedConstructor.Name, UpdatedConstructor.Duration, UpdatedConstructor.StartDate, UpdatedConstructor.EndDate, UpdatedConstructor.PrepTime, UpdatedConstructor.PreDeadline, UpdatedConstructor.Rigid, UpdatedConstructor.Repeat, UpdatedConstructor.Split, EventLocation)
        {
        }
        public CalendarEvent(EventID EventIDEntry, string EventName, TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan Event_PreDeadline, bool EventRigidFlag, Repetition EventRepetitionEntry, int EventSplit, Location EventLocation)
        {
            CalendarEventName = EventName;
            /*CalendarEventName = EventName.Split(',')[0];
            
            if (EventName.Split(',').Length > 1)
            {
                LocationString = EventName.Split(',')[1];
            }
            CalendarEventLocation = new Location();
            if (LocationString != "")
            {
                CalendarEventLocation = new Location(LocationString);
            }*/
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = Event_Duration;
            Splits = EventSplit;
            PrepTime = EventPrepTime;
            EventPreDeadline = Event_PreDeadline;
            RigidSchedule = EventRigidFlag;
            TimePerSplit = new TimeSpan(((EventDuration.Seconds / Splits) * 10000000));
            ArrayOfSubEvents = new SubCalendarEvent[Splits];
            CalendarEventID = EventIDEntry;
            EventRepetition = EventRepetitionEntry;
            LocationData = EventLocation;
            EventSequence = new TimeLine(StartDateTime, EndDateTime);
            UpdateLocationMatrix(EventLocation);
        }
        public CalendarEvent(string EventName, TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan Event_PreDeadline, bool EventRigidFlag, Repetition EventRepetitionEntry, int EventSplit, Location EventLocation)
        {
            CalendarEventName = EventName;
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
            Splits = EventSplit;
            PrepTime = EventPrepTime;
            EventPreDeadline = Event_PreDeadline;
            RigidSchedule = EventRigidFlag;
            TimePerSplit = new TimeSpan(((EventDuration.Seconds / Splits) * 10000000));
            ArrayOfSubEvents = new SubCalendarEvent[Splits];
            CalendarEventID = new EventID(new string[] { EventIDGenerator.generate().ToString() });
            EventRepetition = EventRepetitionEntry;
            LocationData = EventLocation;
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

        public override string ToString()
        {
            return this.ID+"::"+this.Start.ToString() + " - " + this.End.ToString();
        }

        void UpdateLocationMatrix(Location newLocation)
        { 
            
            int i = 0;
            int j = 0;
            if (DistanceMatrix == null)
            {
                DistanceMatrix = new Dictionary<string, List<double>>();
            }
            if(Horizontal==null)
            {
                Horizontal= new List<Location>();
            }

            if(!DistanceMatrix.ContainsKey(this.CalendarEventID.getLevelID(0)))
            {
                string myCalString = this.CalendarEventID.getLevelID(0);
                DistanceMatrix.Add(myCalString, new List<double>());
                Horizontal.Add(newLocation);
                DistanceMatixKeys= DistanceMatrix.Keys.ToList();
                foreach (string eachString in DistanceMatixKeys)
                {
                        
                        Location eachStringLocation=Horizontal[DistanceMatixKeys.IndexOf(eachString)];
                        double MyDistance = Location.calculateDistance(eachStringLocation, newLocation);

                        if (double.IsPositiveInfinity(MyDistance))
                        {
                            ;
                        }

                        if (eachString == this.CalendarEventID.getLevelID(0))
                        {
                            MyDistance = double.MaxValue / DistanceMatixKeys.Count;

                            

                            DistanceMatrix[myCalString].Add(MyDistance);
                        }
                        else
                        {

                            DistanceMatrix[eachString].Add(MyDistance);
                            DistanceMatrix[myCalString].Add(MyDistance);
                        }

                        
                        DistanceMatrix[eachString][DistanceMatixKeys.IndexOf(eachString)] = double.MaxValue / DistanceMatixKeys.Count; 
                        
                }
                
                
            }
        }

        public double getDistance(CalendarEvent CalEvent)
        {
            //get
            {
                return DistanceMatrix[this.CalendarEventID.getLevelID(0)][DistanceMatixKeys.IndexOf(CalEvent.CalendarEventID.getLevelID(0))];
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

             //= this.CalendarEventID.getLevelID(0); ;
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
            return DistanceToAllNodes(this.CalendarEventID.getLevelID(0));
        }

        CalendarEvent getRepeatingCalendarEvent(string RepeatingEventID)
        {
            return EventRepetition.getCalendarEvent(RepeatingEventID);
        }

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
            ArrayOfSubEvents = null;
        }

        public CalendarEvent getRepeatedCalendarEvent(string CalendarID)
        { 
            foreach(CalendarEvent MyCalendarEvent in EventRepetition.RecurringCalendarEvents)
            {
                if (MyCalendarEvent.ID == CalendarID)
                {
                    return MyCalendarEvent;
                }
            }
            return null;
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
            
            public Location CalendarEventLocation;

            public ConstructorModified(string EventIDEntry, string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetition, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag)
            {
                CalendarEventID = new EventID(EventIDEntry.Split('_'));
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
                Split = Convert.ToInt32(eventSplit);
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
            public ConstructorModified(string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetition, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag)
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
                Split = Convert.ToInt32(eventSplit);
                if (PreDeadlineFlag)
                {
                    PreDeadline = new TimeSpan(((int)AllMinutes % 10) * 60);
                }
                else
                {
                    PreDeadline = new TimeSpan(Convert.ToInt64(PreDeadlineTime));
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
                foreach (CalendarEvent MyCalendarEvent in EventRepetition.RecurringCalendarEvents)
                {
                    SubCalendarEvent MySubEvent = MyCalendarEvent.getSubEvent(SubEventID);
                    if (MySubEvent != null)
                    {
                        return MySubEvent;
                    }
                }
            }

            for (; i < ArrayOfSubEvents.Length; i++)
            {
                if (SubEventID.ToString() == ArrayOfSubEvents[i].ID)
                {
                    return ArrayOfSubEvents[i];
                }

            }
            return null;
        }



        virtual public bool updateSubEvent(EventID SubEventID,SubCalendarEvent UpdatedSubEvent)
        {
            if (this.RepetitionStatus)
            {
                foreach (CalendarEvent MyCalendarEvent in Repeat.RecurringCalendarEvents)
                {
                    if (MyCalendarEvent.updateSubEvent(SubEventID, UpdatedSubEvent))
                    {
                        return true;
                    }
                }
            }
            else 
            {
                int i = 0;
                for (i = 0; i < ArrayOfSubEvents.Length;i++)
                {
                    if (ArrayOfSubEvents[i].ID == SubEventID.ToString())
                    {

                        ArrayOfSubEvents[i] = new SubCalendarEvent(UpdatedSubEvent.ID, UpdatedSubEvent.Start, UpdatedSubEvent.End, UpdatedSubEvent.ActiveSlot, ArrayOfSubEvents[i].myLocation, this.EventTimeLine);
                        return true;
                    }
                }
            }

            return false;
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
            foreach (SubCalendarEvent mySubCalendarEvent in ArrayOfSubEvents)
            {
                if (mySubCalendarEvent != null)
                {
                    EventSequence.MergeTimeLines(mySubCalendarEvent.EventTimeLine);
                }
                
            }
        }

        virtual public bool shiftEvent(TimeSpan ChangeInTime, SubCalendarEvent[] UpdatedSubCalEvents)
        {
            TimeLine UpdatedTimeLine = new TimeLine(this.Start+ChangeInTime,this.End+ChangeInTime);
            
            foreach (SubCalendarEvent eachSubCalendarEvent in UpdatedSubCalEvents)
            { 
                if(!(UpdatedTimeLine.IsTimeLineWithin(eachSubCalendarEvent.EventTimeLine)))
                {
                    return false;
                }
            }
            StartDateTime = StartDateTime + ChangeInTime;
            EndDateTime = EndDateTime + ChangeInTime;
            ArrayOfSubEvents = UpdatedSubCalEvents.ToArray();

            return true;
        }

        #endregion


        public string ID
        {
            get
            {
                return CalendarEventID.ToString();
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
                return CalendarEventName;
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
        public bool Completed
        {
            get
            {
                if (DateTime.Now > EndDateTime)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public TimeSpan Preparation
        {
            get
            {
                return PrepTime;
            }
        }
        public TimeSpan PreDeadline
        {
            get
            {
                return EventPreDeadline;
            }
        }
        public TimeSpan ActiveDuration
        {
            get
            {
                return EventDuration;
            }
        }
        public SubCalendarEvent[] AllEvents
        {
            get
            {
                return ArrayOfSubEvents;
            }
        }

        public Location myLocation
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

        public SubCalendarEvent[] AllRepeatSubCalendarEvents
        {
            get
            {
                List<SubCalendarEvent> MyRepeatingSubCalendarEvents = new List<SubCalendarEvent>();
                if (this.Repeat.Enable)
                {
                    
                    foreach (CalendarEvent RepeatingElement in this.EventRepetition.RecurringCalendarEvents)
                    {
                        var HolderConcat = MyRepeatingSubCalendarEvents.Concat(RepeatingElement.AllEvents.ToList());
                        MyRepeatingSubCalendarEvents = HolderConcat.ToList();
                    }
                    return MyRepeatingSubCalendarEvents.ToArray();
                }
              
                return MyRepeatingSubCalendarEvents.ToArray();
            
            }
        
        }
        virtual public TimeLine EventTimeLine
        {
            get
            {
                updateEventSequence();
                return EventSequence;
            }
        }
        public CalendarEvent createCopy()
        {
            CalendarEvent MyCalendarEventCopy = new SubCalendarEvent();
            MyCalendarEventCopy.EventDuration=new TimeSpan(EventDuration.Ticks);
            MyCalendarEventCopy.CalendarEventName = CalendarEventName.ToString();
            MyCalendarEventCopy.StartDateTime = new DateTime(StartDateTime.Ticks);
            MyCalendarEventCopy.EndDateTime = new DateTime(EndDateTime.Ticks);
            MyCalendarEventCopy.EventPreDeadline = new TimeSpan(EventPreDeadline.Ticks);
            MyCalendarEventCopy.PrepTime = new TimeSpan(PrepTime.Ticks);
            MyCalendarEventCopy.Priority = Priority;
            MyCalendarEventCopy.RepetitionFlag = RepetitionFlag;
            MyCalendarEventCopy.EventRepetition = EventRepetition;
            //protected bool Completed = false;
            MyCalendarEventCopy.RigidSchedule = RigidSchedule;//hack
            MyCalendarEventCopy.Splits = Splits;
            MyCalendarEventCopy.TimePerSplit = new TimeSpan(TimePerSplit.Ticks);
            MyCalendarEventCopy.CalendarEventID = CalendarEventID;//hack
            MyCalendarEventCopy.EventSequence = EventSequence.CreateCopy();
            MyCalendarEventCopy.ArrayOfSubEvents = ArrayOfSubEvents.ToArray();
            
            MyCalendarEventCopy.LocationData = LocationData;//hack you might need to make copy
            
            for (int i=0; i<MyCalendarEventCopy.ArrayOfSubEvents.Length;i++)
            {
                MyCalendarEventCopy.ArrayOfSubEvents[i] = MyCalendarEventCopy.ArrayOfSubEvents[i].createCopy();
            }

            MyCalendarEventCopy.SchedulStatus = SchedulStatus;
            MyCalendarEventCopy.otherPartyID = otherPartyID == null ? null : otherPartyID.ToString();
            return MyCalendarEventCopy;
        }


    }
}
