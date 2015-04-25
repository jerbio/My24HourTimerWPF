using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

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
        protected ConcurrentDictionary<DateTimeOffset, CalendarEvent> DictionaryOfStartToCalEvent = new ConcurrentDictionary<DateTimeOffset, CalendarEvent>();
        protected Location_Elements RepeatLocation;
        protected TimeLine initializingRange;//stores range for repetition so if assuming event happens on thursday from 9-11pm. The range is from today till november 31. The initializingRange will be 9-11pm
        protected int RepetitionWeekDay = 7;
        static DayOfWeek[] Weekdays=new DayOfWeek[7]{DayOfWeek.Sunday,DayOfWeek.Monday,DayOfWeek.Tuesday,DayOfWeek.Wednesday,DayOfWeek.Thursday,DayOfWeek.Friday,DayOfWeek.Saturday};
        static DateTimeOffset CalculationStop = DateTimeOffset.Now;
        protected long StartingIndex=0;
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

        public Repetition(bool EnableFlag, TimeLine RepetitionRange_Entry, string Frequency, TimeLine EventActualRange, int[] WeekDayData)//,long StartingIndexData=0)
        {
            RepetitionRange = RepetitionRange_Entry;
            RepetitionFrequency = Frequency.ToUpper();
            EnableRepeat = EnableFlag;
            RepeatLocation = new Location_Elements();
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
            initializingRange = EventActualRange;
            //StartingIndex = StartingIndexData;
            //StartingIndex = (StartingIndexData % 7) + 0;
            foreach (int eachWeekDay in WeekDayData)
            {
                DictionaryOfWeekDayToRepetition.Add(eachWeekDay, new Repetition(EnableFlag, RepetitionRange_Entry.CreateCopy(), Frequency, EventActualRange.CreateCopy(), eachWeekDay ));
            }
        }
        
        private Repetition(bool EnableFlag, TimeLine RepetitionRange_Entry, string Frequency, TimeLine EventActualRange, int WeekDayData=7 )
        {
            RepetitionRange = RepetitionRange_Entry;
            RepetitionFrequency = Frequency.ToUpper();
            EnableRepeat = EnableFlag;
            RepeatLocation = new Location_Elements();
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
            initializingRange = EventActualRange;
            RepetitionWeekDay = WeekDayData;
            //StartingIndex = (WeekDayData % 7) + 0;
        }

        //public Repetition(bool EnableFlag,CalendarEvent BaseCalendarEvent,  TimeLine RepetitionRange_Entry, string Frequency)
       

        


        public CalendarEvent getCalendarEvent(string RepeatingEventID)
        {
            if (DictionaryOfIDAndCalendarEvents.ContainsKey(RepeatingEventID))
            {
                return DictionaryOfIDAndCalendarEvents[RepeatingEventID];
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// this function of repetition, is responsible for populating the repetition object in "MyParentEvent".
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
            StartingIndex = MyParentEvent.RepetitionIndex;
            ++StartingIndex;
            EventID MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.ID, StartingIndex);

            CalendarEvent MyRepeatCalendarEvent = CalendarEvent.InstantiateRepeatedCandidate(MyEventCalendarID, MyParentEvent.Name, MyParentEvent.Duration, EachRepeatCalendarStart, EachRepeatCalendarEnd, EachRepeatCalendarStart, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.Rigid, new Repetition(), MyParentEvent.Rigid ? 1 : MyParentEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete, StartingIndex, DictionaryOfStartToCalEvent);

            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();

            for (; MyRepeatCalendarEvent.Start < MyParentEvent.Repeat.Range.End; )
            {
                //++RepetitionIndex;
                MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.ID, MyRepeatCalendarEvent);
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, Frequency); ;
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, Frequency);
                ++StartingIndex;
                MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.ID, StartingIndex);
                MyRepeatCalendarEvent = CalendarEvent.InstantiateRepeatedCandidate(MyEventCalendarID, MyRepeatCalendarEvent.Name, MyRepeatCalendarEvent.Duration, EachRepeatCalendarStart, EachRepeatCalendarEnd, EachRepeatCalendarStart, MyRepeatCalendarEvent.Preparation, MyRepeatCalendarEvent.PreDeadline, MyRepeatCalendarEvent.Rigid, MyRepeatCalendarEvent.Repeat, MyRepeatCalendarEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete, StartingIndex, DictionaryOfStartToCalEvent);

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

            StartingIndex = MyParentEvent.RepetitionIndex;
            ++StartingIndex;
            //EventID MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.ID, StartingIndex);


            CalendarEventRestricted MyRepeatCalendarEvent = CalendarEventRestricted.InstantiateRepeatedCandidate(MyParentEvent.Name, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.Calendar_EventID, MyParentEvent.RetrictionInfo, MyParentEvent.Duration, EachRepeatCalendarStart, MyParentEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.UIParam, MyParentEvent.Rigid, MyParentEvent.Preparation, MyParentEvent.ThirdPartyID, StartingIndex, DictionaryOfStartToCalEvent);

            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();

            for (; MyRepeatCalendarEvent.Start < MyParentEvent.Repeat.Range.End; )
            {
                MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.ID, MyRepeatCalendarEvent);
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, Frequency); ;
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, Frequency);
                ++StartingIndex;
                //MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.ID, StartingIndex);
                MyRepeatCalendarEvent = CalendarEventRestricted.InstantiateRepeatedCandidate(MyParentEvent.Name, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.Calendar_EventID, MyParentEvent.RetrictionInfo, MyParentEvent.Duration, EachRepeatCalendarStart, MyParentEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.UIParam, MyParentEvent.Rigid, MyParentEvent.Preparation, MyParentEvent.ThirdPartyID, StartingIndex, DictionaryOfStartToCalEvent);

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
            EventID MyEventCalendarID = EventID.GenerateRepeatDayCalendarEvent(MyParentEvent.ID, WeekDay, 0);//using the 0 sequence because MyRepeatCalendarEvent is only needed to generate the parameters for repetition
            //CalendarEvent MyRepeatCalendarEvent = CalendarEvent.InstantiateRepeatedCandidate(MyEventCalendarID, MyParentEvent.Name, MyParentEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, StartTimeLineForActity, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.Rigid, repetitionData, MyParentEvent.Rigid ? 1 : MyParentEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete, 0, DictionaryOfStartToCalEvent);
            CalendarEvent MyRepeatCalendarEvent = new CalendarEvent(MyEventCalendarID, MyParentEvent.Name,MyParentEvent.Duration, EachRepeatCalendarStart, EachRepeatCalendarEnd, StartTimeLineForActity, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.Rigid, repetitionData, MyParentEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete, 0);
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
            EventID MyEventCalendarID = EventID.GenerateRepeatDayCalendarEvent(MyParentEvent.ID, WeekDay, 0);//using the 0 sequence because MyRepeatCalendarEvent is only needed to generate the parameters for repetition
            CalendarEventRestricted MyRepeatCalendarEvent = new CalendarEventRestricted(MyEventCalendarID, MyParentEvent.Name, EachRepeatCalendarStart, EachRepeatCalendarEnd, StartTimeLineForActity, MyParentEvent.RetrictionInfo, MyParentEvent.Duration, repetitionData, MyParentEvent.isComplete, MyParentEvent.isEnabled, MyParentEvent.Rigid ? 1 : MyParentEvent.NumberOfSplit, MyParentEvent.Rigid, MyParentEvent.myLocation, MyParentEvent.Preparation, MyParentEvent.PreDeadline, -1, MyParentEvent.UIParam, MyParentEvent.Notes); 
            this.PopulateRepetitionParameters(MyRepeatCalendarEvent);
        }
        DateTimeOffset IncreaseByFrequency(DateTimeOffset MyTime, string Frequency)
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


        public TimeLine SubEventRange
        {
            get
            {
                return initializingRange;
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

        public List<int> WeekDaySelections
        {
            get
            {
                return DictionaryOfWeekDayToRepetition.Keys.ToList();
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

        public Location_Elements myLocation
        {
            get
            {
                return RepeatLocation;
            }
        }

        

    }
    
   
}
