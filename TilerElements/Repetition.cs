using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class Repetition
    {

        string RepetitionFrequency;
        TimeLine RepetitionRange;
        bool EnableRepeat;
        CalendarEvent[] RepeatingEvents;
        Dictionary<string, CalendarEvent> DictionaryOfIDAndCalendarEvents;
        Location_Elements RepeatLocation;
        TimeLine initializingRange;

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
        public Repetition(bool EnableFlag, TimeLine RepetitionRange_Entry, string Frequency, TimeLine EventActualRange)
        {
            RepetitionRange = RepetitionRange_Entry;
            RepetitionFrequency = Frequency.ToUpper();
            EnableRepeat = EnableFlag;
            RepeatLocation = new Location_Elements();
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
            initializingRange = EventActualRange;
        }

        //public Repetition(bool EnableFlag,CalendarEvent BaseCalendarEvent,  TimeLine RepetitionRange_Entry, string Frequency)
        public Repetition(bool ReadFromFileEnableFlag, TimeLine ReadFromFileRepetitionRange_Entry, string ReadFromFileFrequency, CalendarEvent[] ReadFromFileRecurringListOfCalendarEvents)
        {
            EnableRepeat = ReadFromFileEnableFlag;
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();

            foreach (CalendarEvent MyRepeatCalendarEvent in ReadFromFileRecurringListOfCalendarEvents)
            {
                DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.ID, MyRepeatCalendarEvent);
            }

            RepeatingEvents = DictionaryOfIDAndCalendarEvents.Values.ToArray();
            RepetitionFrequency = ReadFromFileFrequency;
            RepetitionRange = ReadFromFileRepetitionRange_Entry;
            if (ReadFromFileRecurringListOfCalendarEvents.Length > 0)
            {
                RepeatLocation = ReadFromFileRecurringListOfCalendarEvents[0].myLocation;
            }
        }

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

        public void PopulateRepetitionParameters(CalendarEvent MyParentEvent)//this function of repetition, is responsible for populating the repetition object in the passed CalendarEvent.
        {
            if (!MyParentEvent.Repeat.Enable)//Checks if Repetition object is enabled or disabled. If Disabled then just return else continue
            {
                return;
            }

            RepetitionRange = MyParentEvent.Repeat.Range;
            RepetitionFrequency = MyParentEvent.Repeat.Frequency;
            EnableRepeat = true;
            DateTime EachRepeatCalendarStart = initializingRange.Start;//Start DateTime Object for each recurring Calendar Event
            DateTime EachRepeatCalendarEnd = initializingRange.End;//End DateTime Object for each recurring Calendar Event

            EventID MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.ID);
            CalendarEvent MyRepeatCalendarEvent = new CalendarEvent(MyEventCalendarID, MyParentEvent.Name, MyParentEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.Rigid, new Repetition(), MyParentEvent.Rigid ? 1 : MyParentEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete);

            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();

            for (; MyRepeatCalendarEvent.Start < MyParentEvent.Repeat.Range.End; )
            {
                MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.ID, MyRepeatCalendarEvent);
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, Frequency); ;
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, Frequency);
                MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.ID);
                MyRepeatCalendarEvent = new CalendarEvent(MyEventCalendarID, MyRepeatCalendarEvent.Name, MyRepeatCalendarEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyRepeatCalendarEvent.Preparation, MyRepeatCalendarEvent.PreDeadline, MyRepeatCalendarEvent.Rigid, MyRepeatCalendarEvent.Repeat, MyRepeatCalendarEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete);

                MyRepeatCalendarEvent.myLocation = MyParentEvent.myLocation;
            }
            RepeatingEvents = DictionaryOfIDAndCalendarEvents.Values.ToArray();
        }

        private DateTime getStartTimeForAppropriateWeek(DateTime refTime, DayOfWeek SearchedDayOfweek)
        {
            while (refTime.DayOfWeek != SearchedDayOfweek)
            {
                refTime = refTime.AddDays(1);
            }

            return refTime;
        }

        private void PopulateRepetitionParameters(CalendarEvent MyParentEvent, DayOfWeek weekDay)
        { 
            RepetitionRange = MyParentEvent.Repeat.Range;
            RepetitionFrequency = MyParentEvent.Repeat.Frequency;
            EnableRepeat = true;
            DateTime EachRepeatCalendarStart = initializingRange.Start;//Start DateTime Object for each recurring Calendar Event
            EachRepeatCalendarStart = getStartTimeForAppropriateWeek(EachRepeatCalendarStart, weekDay);
            
            DateTime EachRepeatCalendarEnd = initializingRange.End;//End DateTime Object for each recurring Calendar Event
            TimeLine repetitionTimeline= new TimeLine(EachRepeatCalendarStart,EachRepeatCalendarEnd);

            Repetition repetitionData = new Repetition(MyParentEvent.isEnabled, repetitionTimeline, this.Frequency, repetitionTimeline);
            EventID MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.ID,(int)weekDay);
            CalendarEvent MyRepeatCalendarEvent = new CalendarEvent(MyEventCalendarID, MyParentEvent.Name, MyParentEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.Rigid,repetitionData , MyParentEvent.Rigid ? 1 : MyParentEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete);
        }
        public DateTime IncreaseByFrequency(DateTime MyTime, string Frequency)
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


        /*public Repetition(bool EnableFlag, DateTime StartTime, TimeSpan Frequency)
        {
            Start = StartTime;
            RepetitionFrequency = Frequency;
            EnableRepeat = EnableFlag;
        }*/

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

        public TimeLine Range
        {
            get
            {
                return RepetitionRange;
            }
        }

        public CalendarEvent[] RecurringCalendarEvents
        {
            set
            {
                foreach (CalendarEvent MyCalEvent in value)
                {
                    DictionaryOfIDAndCalendarEvents[MyCalEvent.ID] = MyCalEvent;
                }

                RepeatingEvents = DictionaryOfIDAndCalendarEvents.Values.ToArray();//assign od diffe list can generate inconsistencies...watchout for bugs
            }
            get
            {
                return DictionaryOfIDAndCalendarEvents.Values.ToArray();
            }
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
            repetition_cpy.RepeatingEvents = RepeatingEvents.Select(obj => obj.createCopy()).ToArray();
            repetition_cpy.RepeatLocation = RepeatLocation.CreateCopy();
            repetition_cpy.EnableRepeat = EnableRepeat;
            repetition_cpy.DictionaryOfIDAndCalendarEvents = DictionaryOfIDAndCalendarEvents.ToDictionary(obj => obj.Key, obj1 => obj1.Value.createCopy());
            return repetition_cpy;
        }

    }
    
   
}
