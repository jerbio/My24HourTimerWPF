using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodaTime;

namespace TilerElements
{
    public class Repetition
    {

        protected string RepetitionFrequency;
        protected TimeLine RepetitionRange;//stores range for repetition so if assuming event happens on thursday from 9-11pm. The range is from today till november 31. The RepetitionRange will be today till November 31
        protected bool EnableRepeat;
        protected CalendarEvent[] RepeatingEvents;
        protected Dictionary<string, CalendarEvent> DictionaryOfIDAndCalendarEvents;
        protected Dictionary<int, Repetition> DictionaryOfWeekDayToRepetition = new Dictionary<int,Repetition>();
        protected Location_Elements RepeatLocation;
        protected TimeLine initializingRange;//stores range for repetition so if assuming event happens on thursday from 9-11pm. The range is from today till november 31. The initializingRange will be 9-11pm
        protected int RepetitionWeekDay =7;
        static DayOfWeek[] Weekdays=new DayOfWeek[7]{DayOfWeek.Sunday,DayOfWeek.Monday,DayOfWeek.Tuesday,DayOfWeek.Wednesday,DayOfWeek.Thursday,DayOfWeek.Friday,DayOfWeek.Saturday};
        static DateTimeOffset CalculationStop = DateTimeOffset.Now;

        public Repetition()
        {
            RepetitionFrequency = "";
            RepetitionRange = new TimeLine();
            EnableRepeat = false;
            RepeatingEvents = new CalendarEvent[0];
            RepeatLocation = new Location_Elements();
            initializingRange = new TimeLine();
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
        }

        public Repetition(bool EnableFlag, TimeLine RepetitionRange_Entry, string Frequency, TimeLine EventActualRange, int[] WeekDayData)
        {
            RepetitionRange = RepetitionRange_Entry;
            RepetitionFrequency = Frequency.ToUpper();
            EnableRepeat = EnableFlag;
            RepeatLocation = new Location_Elements();
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
            initializingRange = EventActualRange;
            foreach (int eachWeekDay in WeekDayData)
            {
                DictionaryOfWeekDayToRepetition.Add(eachWeekDay, new Repetition(EnableFlag, RepetitionRange_Entry.CreateCopy(), Frequency, EventActualRange.CreateCopy(), eachWeekDay ));
            }
        }
        
        public Repetition(bool EnableFlag, TimeLine RepetitionRange_Entry, string Frequency, TimeLine EventActualRange, int WeekDayData=7 )
        {
            RepetitionRange = RepetitionRange_Entry;
            RepetitionFrequency = Frequency.ToUpper();
            EnableRepeat = EnableFlag;
            RepeatLocation = new Location_Elements();
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
            initializingRange = EventActualRange;
            RepetitionWeekDay = WeekDayData;
        }

        //public Repetition(bool EnableFlag,CalendarEvent BaseCalendarEvent,  TimeLine RepetitionRange_Entry, string Frequency)
        public Repetition(bool ReadFromFileEnableFlag, TimeLine ReadFromFileRepetitionRange_Entry, string ReadFromFileFrequency, CalendarEvent[] ReadFromFileRecurringListOfCalendarEvents, int DayOfWeek=7)
        {
            EnableRepeat = ReadFromFileEnableFlag;
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
            RepetitionWeekDay = DayOfWeek;
            foreach (CalendarEvent MyRepeatCalendarEvent in ReadFromFileRecurringListOfCalendarEvents)
            {
                DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.getId, MyRepeatCalendarEvent);
            }

            RepeatingEvents = DictionaryOfIDAndCalendarEvents.Values.ToArray();
            RepetitionFrequency = ReadFromFileFrequency;
            RepetitionRange = ReadFromFileRepetitionRange_Entry;
            if (ReadFromFileRecurringListOfCalendarEvents.Length > 0)
            {
                RepeatLocation = ReadFromFileRecurringListOfCalendarEvents[0].myLocation;
            }
        }


        public Repetition(bool ReadFromFileEnableFlag, TimeLine ReadFromFileRepetitionRange_Entry, string ReadFromFileFrequency, Repetition [] repetition_Weekday, int DayOfWeek=7)
        {
            EnableRepeat = ReadFromFileEnableFlag;
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
            RepetitionWeekDay = DayOfWeek;
            foreach (Repetition eachRepetition in repetition_Weekday)
            {
                DictionaryOfWeekDayToRepetition.Add(eachRepetition.RepetitionWeekDay, eachRepetition);
            }
            

            RepeatingEvents = DictionaryOfIDAndCalendarEvents.Values.ToArray();
            RepetitionFrequency = ReadFromFileFrequency;
            RepetitionRange = ReadFromFileRepetitionRange_Entry;
            if (repetition_Weekday.Length > 0)
            {
                RepeatLocation = repetition_Weekday[0].RepeatLocation;
            }
        }


        public CalendarEvent getCalendarEvent(string RepeatingEventID)
        {
            EventID eventId = new EventID(new EventID(RepeatingEventID).getIDUpToRepeatCalendarEvent());
            if (DictionaryOfIDAndCalendarEvents.ContainsKey(eventId.ToString()))
            {
                return DictionaryOfIDAndCalendarEvents[eventId.ToString()];
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// this function of repetition, is responsible for populating the repetition object in the passed CalendarEvent.
        /// </summary>
        /// <param name="MyParentEvent"></param>
        public void PopulateRepetitionParameters(CalendarEvent MyParentEvent)
        {
            if (!MyParentEvent.Repeat.Enable)//Checks if Repetition object is enabled or disabled. If Disabled then just return else continue
            {
                return;
            }
            EnableRepeat = true;
            RepetitionRange = MyParentEvent.Repeat.Range;
            RepetitionFrequency = MyParentEvent.Repeat.Frequency;
            if (DictionaryOfWeekDayToRepetition.Count > 0)
            {
                foreach (KeyValuePair<int, Repetition> eachKeyValuePair in DictionaryOfWeekDayToRepetition)
                {
                    eachKeyValuePair.Value.PopulateRepetitionParameters(MyParentEvent, eachKeyValuePair.Key);
                }
                return;
            }

            
            
            DateTimeOffset EachRepeatCalendarStart = initializingRange.Start;//Start DateTimeOffset Object for each recurring Calendar Event
            DateTimeOffset EachRepeatCalendarEnd = initializingRange.End > MyParentEvent.End ? MyParentEvent.End : initializingRange.End;//End DateTimeOffset Object for each recurring Calendar Event

            EventID MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
            CalendarEvent MyRepeatCalendarEvent;
            if (MyParentEvent.Rigid)
            {
                MyRepeatCalendarEvent = new RigidCalendarEvent(MyParentEvent.Name, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.ActiveDuration, MyParentEvent.Preparation, MyParentEvent.PreDeadline, new Repetition(), MyParentEvent.myLocation,  MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isEnabled, MyParentEvent.isComplete, MyParentEvent.Creator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID);
            }
            else
            {
                MyRepeatCalendarEvent = new CalendarEvent(MyParentEvent.Name, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.ActiveDuration, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.NumberOfSplit, new Repetition(), MyParentEvent.myLocation, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.ProcrastinationInfo, MyParentEvent.NowInfo, MyParentEvent.isEnabled, MyParentEvent.isComplete, MyParentEvent.Creator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID);
            }
            //= new CalendarEvent();

            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();

            for (; MyRepeatCalendarEvent.Start < MyParentEvent.Repeat.Range.End; )
            {
                MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.getId, MyRepeatCalendarEvent);
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, Frequency); ;
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, Frequency);
                MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
                if (MyRepeatCalendarEvent.Rigid)
                {
                    MyRepeatCalendarEvent = new RigidCalendarEvent(MyRepeatCalendarEvent.Name, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyRepeatCalendarEvent.ActiveDuration, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyRepeatCalendarEvent.Repeat,MyParentEvent.myLocation,  MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isEnabled, MyParentEvent.isComplete, MyParentEvent.Creator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID);
                } else
                {
                    MyRepeatCalendarEvent = new CalendarEvent(MyRepeatCalendarEvent.Name, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyRepeatCalendarEvent.ActiveDuration, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyRepeatCalendarEvent.NumberOfSplit, MyRepeatCalendarEvent.Repeat, MyParentEvent.myLocation, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.ProcrastinationInfo, MyParentEvent.NowInfo, MyParentEvent.isEnabled, MyParentEvent.isComplete, MyParentEvent.Creator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID);
                }

                MyRepeatCalendarEvent.myLocation = MyParentEvent.myLocation;
            }
            RepeatingEvents = DictionaryOfIDAndCalendarEvents.Values.ToArray();
        }


        public void PopulateRepetitionParameters(CalendarEventRestricted MyParentEvent)
        {
            if (!MyParentEvent.Repeat.Enable)//Checks if Repetition object is enabled or disabled. If Disabled then just return else continue
            {
                return;
            }
            EnableRepeat = true;
            RepetitionRange = MyParentEvent.Repeat.Range;
            RepetitionFrequency = MyParentEvent.Repeat.Frequency;
            if (DictionaryOfWeekDayToRepetition.Count > 0)
            {
                foreach (KeyValuePair<int, Repetition> eachKeyValuePair in DictionaryOfWeekDayToRepetition)
                {
                    eachKeyValuePair.Value.PopulateRepetitionParameters(MyParentEvent, eachKeyValuePair.Key);
                }
                return;
            }



            DateTimeOffset EachRepeatCalendarStart = initializingRange.Start;//Start DateTimeOffset Object for each recurring Calendar Event
            DateTimeOffset EachRepeatCalendarEnd = initializingRange.End > MyParentEvent.End ? MyParentEvent.End : initializingRange.End;//End DateTimeOffset Object for each recurring Calendar Event

            EventID MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
            CalendarEventRestricted MyRepeatCalendarEvent = CalendarEventRestricted.InstantiateRepeatedCandidate(MyParentEvent.Name, EachRepeatCalendarStart, EachRepeatCalendarEnd,MyParentEvent.Calendar_EventID,MyParentEvent.RetrictionInfo, MyParentEvent.ActiveDuration,MyParentEvent.NumberOfSplit,MyParentEvent.myLocation,MyParentEvent.UIParam,MyParentEvent.Rigid,MyParentEvent.Preparation,MyParentEvent.ThirdPartyID);// MyParentEvent.Preparation, MyParentEvent.Rigid, new Repetition(), MyParentEvent.Rigid ? 1 : MyParentEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete);

            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();

            for (; MyRepeatCalendarEvent.Start < MyParentEvent.Repeat.Range.End; )
            {
                MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.getId, MyRepeatCalendarEvent);
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, Frequency); ;
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, Frequency);
                MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
                MyRepeatCalendarEvent = CalendarEventRestricted.InstantiateRepeatedCandidate(MyParentEvent.Name, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.Calendar_EventID, MyParentEvent.RetrictionInfo, MyParentEvent.ActiveDuration, MyParentEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.UIParam, MyParentEvent.Rigid, MyParentEvent.Preparation, MyParentEvent.ThirdPartyID); //new CalendarEvent(MyEventCalendarID, MyRepeatCalendarEvent.Name, MyRepeatCalendarEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyRepeatCalendarEvent.Preparation, MyRepeatCalendarEvent.PreDeadline, MyRepeatCalendarEvent.Rigid, MyRepeatCalendarEvent.Repeat, MyRepeatCalendarEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete);

                MyRepeatCalendarEvent.myLocation = MyParentEvent.myLocation;
            }
            RepeatingEvents = DictionaryOfIDAndCalendarEvents.Values.ToArray();
        }
        



        private DateTimeOffset getStartTimeForAppropriateWeek(DateTimeOffset refTime, DayOfWeek SearchedDayOfweek)
        {
            while (refTime.DayOfWeek != SearchedDayOfweek)
            {
                refTime = refTime.AddDays(1);
            }

            return refTime;
        }

        private void PopulateRepetitionParameters(CalendarEvent MyParentEvent, int WeekDay)
        { 
            RepetitionRange = MyParentEvent.Repeat.Range;
            RepetitionFrequency = MyParentEvent.Repeat.Frequency;
            EnableRepeat = true;
            DateTimeOffset EachRepeatCalendarStart = RepetitionRange.Start;
            DateTimeOffset EachRepeatCalendarEnd = RepetitionRange.End;
            DateTimeOffset StartTimeLineForActity = getStartTimeForAppropriateWeek(initializingRange.Start, Weekdays[WeekDay]);
            DateTimeOffset EndTimeLineForActity = StartTimeLineForActity.Add(initializingRange.TimelineSpan);

            TimeLine repetitionTimeline= new TimeLine(EachRepeatCalendarStart,EachRepeatCalendarEnd);
            TimeLine ActiveTimeline = new TimeLine(StartTimeLineForActity, EndTimeLineForActity);

            Repetition repetitionData = new Repetition(MyParentEvent.isEnabled, repetitionTimeline, this.Frequency, ActiveTimeline,7);


            this.initializingRange = ActiveTimeline;
            this.RepetitionRange = repetitionTimeline;
            EventID MyEventCalendarID = EventID.GenerateRepeatDayCalendarEvent(MyParentEvent.getId, WeekDay);
            CalendarEvent MyRepeatCalendarEvent;
            if (MyParentEvent.Rigid)
            {
                MyRepeatCalendarEvent = new RigidCalendarEvent(MyParentEvent.Name, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.ActiveDuration, MyParentEvent.Preparation, MyParentEvent.PreDeadline, repetitionData, MyParentEvent.myLocation, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isEnabled,  MyParentEvent.isComplete, MyParentEvent.Creator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID);
            }
            else
            {
                MyRepeatCalendarEvent = new CalendarEvent(MyParentEvent.Name, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.ActiveDuration, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.NumberOfSplit, repetitionData,  MyParentEvent.myLocation, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.ProcrastinationInfo, MyParentEvent.NowInfo, MyParentEvent.isEnabled, MyParentEvent.isComplete, MyParentEvent.Creator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID);
            }

            this.PopulateRepetitionParameters(MyRepeatCalendarEvent);
        }

        private void PopulateRepetitionParameters(CalendarEventRestricted MyParentEvent, int WeekDay)
        {
            RepetitionRange = MyParentEvent.Repeat.Range;
            RepetitionFrequency = MyParentEvent.Repeat.Frequency;
            EnableRepeat = true;
            DateTimeOffset EachRepeatCalendarStart = RepetitionRange.Start;
            DateTimeOffset EachRepeatCalendarEnd = RepetitionRange.End;
            DateTimeOffset StartTimeLineForActity = getStartTimeForAppropriateWeek(initializingRange.Start, Weekdays[WeekDay]);
            DateTimeOffset EndTimeLineForActity = StartTimeLineForActity.Add(initializingRange.TimelineSpan);

            TimeLine repetitionTimeline = new TimeLine(EachRepeatCalendarStart, EachRepeatCalendarEnd);
            TimeLine ActiveTimeline = new TimeLine(StartTimeLineForActity, EndTimeLineForActity);

            Repetition repetitionData = new Repetition(MyParentEvent.isEnabled, repetitionTimeline, this.Frequency, ActiveTimeline, 7);


            this.initializingRange = ActiveTimeline;
            this.RepetitionRange = repetitionTimeline;
            EventID MyEventCalendarID = EventID.GenerateRepeatDayCalendarEvent(MyParentEvent.getId, WeekDay);
            CalendarEventRestricted MyRepeatCalendarEvent = new CalendarEventRestricted(MyParentEvent.Creator , MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyParentEvent.Name, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.RetrictionInfo, MyParentEvent.ActiveDuration, repetitionData, MyParentEvent.isComplete, MyParentEvent.isEnabled, MyParentEvent.Rigid ? 1 : MyParentEvent.NumberOfSplit, MyParentEvent.Rigid, MyParentEvent.myLocation, MyParentEvent.Preparation,MyParentEvent.PreDeadline, MyEventCalendarID, MyParentEvent.UIParam, MyParentEvent.Notes);
            this.PopulateRepetitionParameters(MyRepeatCalendarEvent);
        }
        DateTimeOffset IncreaseByFrequency(DateTimeOffset MyTime, string Frequency, string timeZone = "UTC")
        {
            Frequency = Frequency.ToUpper();
            switch (Frequency)
            {
                case "DAILY":
                    {
                        return MyTime.AddDays(1);
                    }
                case "WEEKLY":
                    {
                        return MyTime.AddDays(7);
                    }
                case "BI-WEEKLY":
                    {
                        return MyTime.AddDays(14);
                    }
                case "MONTHLY":
                    {
                        return MyTime.AddMonths(1);
                    }
                case "YEARLY":
                    {
                        return MyTime.AddYears(1);
                    }
                default:
                    {
                        return MyTime;
                    }
            }
        }


        /*public Repetition(bool EnableFlag, DateTimeOffset StartTime, TimeSpan Frequency)
        {
            Start = StartTime;
            RepetitionFrequency = Frequency;
            EnableRepeat = EnableFlag;
        }*/

        public bool isExtraLayers()
        {
            return DictionaryOfWeekDayToRepetition.Count > 0;
        }

        public List<Repetition> getDayRepetitions()
        {
            List<Repetition> retValue = DictionaryOfWeekDayToRepetition.Values.ToList();
            return retValue;
        }

        public bool Enable
        {
            get
            {
                return EnableRepeat;
            }
        }

        public string Frequency
        {
            get
            {
                return RepetitionFrequency;
            }
        }


        public int weekDay
        {
            get
            {
                return RepetitionWeekDay;
            }
        }
        /// <summary>
        /// Range for repetition so if assuming event happens on thursday from 9-11pm. The range is from today till november 31. The RepetitionRange will be today till November 31
        /// </summary>
        public TimeLine Range
        {
            get
            {
                return RepetitionRange;
            }
        }

        public TimeLine SingleInstanceTimeFrame
        {
            get
            {
                return initializingRange;
            }
        }

        public Dictionary<int, Repetition> DayIndexToRepetition
        {
            get
            {
                Dictionary<int, Repetition> retValue = DictionaryOfWeekDayToRepetition.ToDictionary(obj => obj.Key, obj => obj.Value);
                return retValue;
            }
        }

        public CalendarEvent[] RecurringCalendarEvents()
        {
            CalendarEvent[] retValue = null;

            if (DictionaryOfWeekDayToRepetition.Count > 0)
            {
                retValue = DictionaryOfWeekDayToRepetition.SelectMany(obj => obj.Value.RecurringCalendarEvents()).ToArray();
            }
            else
            {
                retValue= DictionaryOfIDAndCalendarEvents.Values.ToArray();
            }
            return retValue;
        }

        public Repetition CreateCopy()
        {
            Repetition repetition_cpy = new Repetition();
            if (this.RepeatingEvents.Length < 1)
            {
                return repetition_cpy;
            }
            repetition_cpy.RepetitionFrequency = this.RepetitionFrequency;
            repetition_cpy.RepetitionRange = this.RepetitionRange.CreateCopy();
            repetition_cpy.RepeatingEvents = RepeatingEvents.AsParallel().Select(obj => obj.createCopy()).ToArray();
            repetition_cpy.RepeatLocation = RepeatLocation.CreateCopy();
            repetition_cpy.EnableRepeat = EnableRepeat;
            repetition_cpy.RepetitionWeekDay = RepetitionWeekDay;
            repetition_cpy.DictionaryOfIDAndCalendarEvents = DictionaryOfIDAndCalendarEvents.AsParallel().ToDictionary(obj => obj.Key, obj1 => obj1.Value.createCopy());
            repetition_cpy.DictionaryOfWeekDayToRepetition = DictionaryOfWeekDayToRepetition.AsParallel().ToDictionary(obj => obj.Key, obj1 => obj1.Value.CreateCopy());
            return repetition_cpy;
        }

        

    }
    
   
}
