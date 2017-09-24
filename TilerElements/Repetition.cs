using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class Repetition
    {

        protected Frequency RepetitionFrequency;
        protected TimeLine RepetitionRange;//stores range for repetition so if assuming event happens on thursday from 9-11pm. The range is from today till november 31. The RepetitionRange will be today till November 31
        protected bool EnableRepeat;
        protected CalendarEvent[] RepeatingEvents;
        protected Dictionary<string, CalendarEvent> DictionaryOfIDAndCalendarEvents;
        protected Dictionary<int, Repetition> DictionaryOfWeekDayToRepetition = new Dictionary<int,Repetition>();
        protected Location RepeatLocation;
        protected TimeLine initializingRange;//stores range for repetition so if assuming event happens on thursday from 9-11pm. The range is from today till november 31. The initializingRange will be 9-11pm
        protected int RepetitionWeekDay =7;
        static DayOfWeek[] Weekdays=new DayOfWeek[7]{DayOfWeek.Sunday,DayOfWeek.Monday,DayOfWeek.Tuesday,DayOfWeek.Wednesday,DayOfWeek.Thursday,DayOfWeek.Friday,DayOfWeek.Saturday};
        static DateTimeOffset CalculationStop = DateTimeOffset.UtcNow;
        public enum Frequency {DAILY, WEEKLY, MONTHLY, YEARLY, BIWEEKLY, NONE };

        public Repetition()
        {
            RepetitionFrequency = Frequency.NONE;
            RepetitionRange = new TimeLine();
            EnableRepeat = false;
            RepeatingEvents = new CalendarEvent[0];
            RepeatLocation = new Location();
            initializingRange = new TimeLine();
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
        }

        public Repetition(bool EnableFlag, TimeLine RepetitionRange_Entry, Frequency frequency, TimeLine EventActualRange, int[] WeekDayData)
        {
            RepetitionRange = RepetitionRange_Entry;
            RepetitionFrequency = frequency;
            EnableRepeat = EnableFlag;
            RepeatLocation = new Location();
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
            initializingRange = EventActualRange;
            foreach (int eachWeekDay in WeekDayData)
            {
                DictionaryOfWeekDayToRepetition.Add(eachWeekDay, new Repetition(EnableFlag, RepetitionRange_Entry.CreateCopy(), frequency, EventActualRange.CreateCopy(), eachWeekDay ));
            }
        }
        
        public Repetition(bool EnableFlag, TimeLine RepetitionRange_Entry, Frequency frequency, TimeLine EventActualRange, int WeekDayData=7 )
        {
            RepetitionRange = RepetitionRange_Entry;
            RepetitionFrequency = frequency;
            EnableRepeat = EnableFlag;
            RepeatLocation = new Location();
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
            RepetitionFrequency = Utility.ParseEnum<Frequency>(ReadFromFileFrequency.ToUpper());
            RepetitionRange = ReadFromFileRepetitionRange_Entry;
            if (ReadFromFileRecurringListOfCalendarEvents.Length > 0)
            {
                RepeatLocation = ReadFromFileRecurringListOfCalendarEvents[0].Location;
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
            RepetitionFrequency = Utility.ParseEnum<Frequency>(ReadFromFileFrequency.ToUpper());
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
            RepetitionFrequency = MyParentEvent.Repeat.getFrequency;
            if (DictionaryOfWeekDayToRepetition.Count > 0)
            {
                foreach (KeyValuePair<int, Repetition> eachKeyValuePair in DictionaryOfWeekDayToRepetition)
                {
                    eachKeyValuePair.Value.PopulateRepetitionParameters(MyParentEvent, eachKeyValuePair.Key);
                }
                return;
            }


            RestrictionProfile restrictionProfile = new RestrictionProfile(initializingRange.Start, initializingRange.End - initializingRange.Start);
            TimeLineRestricted segmentedTimeLine = new TimeLineRestricted(RepetitionRange.Start, RepetitionRange.End, restrictionProfile);
            TimeLine firstRepeatSequenct = segmentedTimeLine.getTimeFrames().First();

            DateTimeOffset EachRepeatCalendarStart = firstRepeatSequenct.Start;//Start DateTimeOffset Object for each recurring Calendar Event
            DateTimeOffset EachRepeatCalendarEnd = firstRepeatSequenct.End;

            EventID MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
            CalendarEvent MyRepeatCalendarEvent;
            if (MyParentEvent.getRigid)
            {
                EachRepeatCalendarStart = initializingRange.Start;
                EachRepeatCalendarEnd = initializingRange.End;
                MyRepeatCalendarEvent = new RigidCalendarEvent(MyParentEvent.getName, initializingRange.Start, initializingRange.End, MyParentEvent.getActiveDuration, MyParentEvent.getPreparation, MyParentEvent.getPreDeadline, new Repetition(), MyParentEvent.Location,  MyParentEvent.getUIParam, MyParentEvent.Notes, MyParentEvent.isEnabled, MyParentEvent.getIsComplete, MyParentEvent.getCreator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID);
            }
            else
            {
                MyRepeatCalendarEvent = new CalendarEvent(MyParentEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.getActiveDuration, MyParentEvent.getPreparation, MyParentEvent.getPreDeadline, MyParentEvent.NumberOfSplit, new Repetition(), MyParentEvent.Location, MyParentEvent.getUIParam, MyParentEvent.Notes, MyParentEvent.getProcrastinationInfo, MyParentEvent.getNowInfo, MyParentEvent.isEnabled, MyParentEvent.getIsComplete, MyParentEvent.getCreator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID);
            }
            //= new CalendarEvent();

            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();

            for (; MyRepeatCalendarEvent.Start < MyParentEvent.Repeat.Range.End; )
            {
                MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.getId, MyRepeatCalendarEvent);
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, getFrequency); ;
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, getFrequency);
                MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
                if (MyRepeatCalendarEvent.getRigid)
                {
                    MyRepeatCalendarEvent = new RigidCalendarEvent(MyRepeatCalendarEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyRepeatCalendarEvent.getActiveDuration, MyParentEvent.getPreparation, MyParentEvent.getPreDeadline, MyRepeatCalendarEvent.Repeat,MyParentEvent.Location,  MyParentEvent.getUIParam, MyParentEvent.Notes, MyParentEvent.isEnabled, MyParentEvent.getIsComplete, MyParentEvent.getCreator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID);
                } else
                {
                    MyRepeatCalendarEvent = new CalendarEvent(MyRepeatCalendarEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyRepeatCalendarEvent.getActiveDuration, MyParentEvent.getPreparation, MyParentEvent.getPreDeadline, MyRepeatCalendarEvent.NumberOfSplit, MyRepeatCalendarEvent.Repeat, MyParentEvent.Location, MyParentEvent.getUIParam, MyParentEvent.Notes, MyParentEvent.getProcrastinationInfo, MyParentEvent.getNowInfo, MyParentEvent.isEnabled, MyParentEvent.getIsComplete, MyParentEvent.getCreator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID);
                }

                MyRepeatCalendarEvent.Location = MyParentEvent.Location;
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
            RepetitionFrequency = MyParentEvent.Repeat.getFrequency;
            if (DictionaryOfWeekDayToRepetition.Count > 0)
            {
                foreach (KeyValuePair<int, Repetition> eachKeyValuePair in DictionaryOfWeekDayToRepetition)
                {
                    eachKeyValuePair.Value.PopulateRepetitionParameters(MyParentEvent, eachKeyValuePair.Key);
                }
                return;
            }

            RestrictionProfile restrictionProfile = new RestrictionProfile(initializingRange.Start, initializingRange.End - initializingRange.Start);
            TimeLineRestricted segmentedTimeLine = new TimeLineRestricted(RepetitionRange.Start, RepetitionRange.End, restrictionProfile);
            TimeLine firstRepeatSequenct = segmentedTimeLine.getTimeFrames().First();

            DateTimeOffset EachRepeatCalendarStart = firstRepeatSequenct.Start;//Start DateTimeOffset Object for each recurring Calendar Event
            DateTimeOffset EachRepeatCalendarEnd = firstRepeatSequenct.End;

            EventID MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
            CalendarEventRestricted MyRepeatCalendarEvent = CalendarEventRestricted.InstantiateRepeatedCandidate(MyParentEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd,MyParentEvent.Calendar_EventID,MyParentEvent.RetrictionInfo, MyParentEvent.getActiveDuration,MyParentEvent.NumberOfSplit,MyParentEvent.Location,MyParentEvent.getUIParam,MyParentEvent.getRigid,MyParentEvent.getPreparation,MyParentEvent.ThirdPartyID);// MyParentEvent.Preparation, MyParentEvent.Rigid, new Repetition(), MyParentEvent.Rigid ? 1 : MyParentEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete);

            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();

            for (; MyRepeatCalendarEvent.Start < MyParentEvent.Repeat.Range.End; )
            {
                MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.getId, MyRepeatCalendarEvent);
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, getFrequency); ;
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, getFrequency);
                MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
                MyRepeatCalendarEvent = CalendarEventRestricted.InstantiateRepeatedCandidate(MyParentEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.Calendar_EventID, MyParentEvent.RetrictionInfo, MyParentEvent.getActiveDuration, MyParentEvent.NumberOfSplit, MyParentEvent.Location, MyParentEvent.getUIParam, MyParentEvent.getRigid, MyParentEvent.getPreparation, MyParentEvent.ThirdPartyID); //new CalendarEvent(MyEventCalendarID, MyRepeatCalendarEvent.Name, MyRepeatCalendarEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyRepeatCalendarEvent.Preparation, MyRepeatCalendarEvent.PreDeadline, MyRepeatCalendarEvent.Rigid, MyRepeatCalendarEvent.Repeat, MyRepeatCalendarEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete);

                MyRepeatCalendarEvent.Location = MyParentEvent.Location;
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
            RepetitionFrequency = MyParentEvent.Repeat.getFrequency;
            EnableRepeat = true;
            DateTimeOffset EachRepeatCalendarStart = RepetitionRange.Start;
            DateTimeOffset EachRepeatCalendarEnd = RepetitionRange.End;
            DateTimeOffset StartTimeLineForActity = getStartTimeForAppropriateWeek(initializingRange.Start, Weekdays[WeekDay]);
            DateTimeOffset EndTimeLineForActity = StartTimeLineForActity.Add(initializingRange.TimelineSpan);

            TimeLine repetitionTimeline= new TimeLine(EachRepeatCalendarStart,EachRepeatCalendarEnd);
            TimeLine ActiveTimeline = new TimeLine(StartTimeLineForActity, EndTimeLineForActity);

            Repetition repetitionData = new Repetition(MyParentEvent.isEnabled, repetitionTimeline, this.getFrequency, ActiveTimeline,7);


            this.initializingRange = ActiveTimeline;
            this.RepetitionRange = repetitionTimeline;
            EventID MyEventCalendarID = EventID.GenerateRepeatDayCalendarEvent(MyParentEvent.getId, WeekDay);
            CalendarEvent MyRepeatCalendarEvent;
            if (MyParentEvent.getRigid)
            {
                MyRepeatCalendarEvent = new RigidCalendarEvent(MyParentEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.getActiveDuration, MyParentEvent.getPreparation, MyParentEvent.getPreDeadline, repetitionData, MyParentEvent.Location, MyParentEvent.getUIParam, MyParentEvent.Notes, MyParentEvent.isEnabled,  MyParentEvent.getIsComplete, MyParentEvent.getCreator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID);
            }
            else
            {
                MyRepeatCalendarEvent = new CalendarEvent(MyParentEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.getActiveDuration, MyParentEvent.getPreparation, MyParentEvent.getPreDeadline, MyParentEvent.NumberOfSplit, repetitionData,  MyParentEvent.Location, MyParentEvent.getUIParam, MyParentEvent.Notes, MyParentEvent.getProcrastinationInfo, MyParentEvent.getNowInfo, MyParentEvent.isEnabled, MyParentEvent.getIsComplete, MyParentEvent.getCreator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID);
            }

            this.PopulateRepetitionParameters(MyRepeatCalendarEvent);
        }

        private void PopulateRepetitionParameters(CalendarEventRestricted MyParentEvent, int WeekDay)
        {
            RepetitionRange = MyParentEvent.Repeat.Range;
            RepetitionFrequency = MyParentEvent.Repeat.getFrequency;
            EnableRepeat = true;
            DateTimeOffset EachRepeatCalendarStart = RepetitionRange.Start;
            DateTimeOffset EachRepeatCalendarEnd = RepetitionRange.End;
            DateTimeOffset StartTimeLineForActity = getStartTimeForAppropriateWeek(initializingRange.Start, Weekdays[WeekDay]);
            DateTimeOffset EndTimeLineForActity = StartTimeLineForActity.Add(initializingRange.TimelineSpan);

            TimeLine repetitionTimeline = new TimeLine(EachRepeatCalendarStart, EachRepeatCalendarEnd);
            TimeLine ActiveTimeline = new TimeLine(StartTimeLineForActity, EndTimeLineForActity);

            Repetition repetitionData = new Repetition(MyParentEvent.isEnabled, repetitionTimeline, this.getFrequency, ActiveTimeline, 7);


            this.initializingRange = ActiveTimeline;
            this.RepetitionRange = repetitionTimeline;
            EventID MyEventCalendarID = EventID.GenerateRepeatDayCalendarEvent(MyParentEvent.getId, WeekDay);
            CalendarEventRestricted MyRepeatCalendarEvent = new CalendarEventRestricted(MyParentEvent.getCreator , MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyParentEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.RetrictionInfo, MyParentEvent.getActiveDuration, repetitionData, MyParentEvent.getIsComplete, MyParentEvent.isEnabled, MyParentEvent.getRigid ? 1 : MyParentEvent.NumberOfSplit, MyParentEvent.getRigid, MyParentEvent.Location, MyParentEvent.getPreparation,MyParentEvent.getPreDeadline, MyEventCalendarID, MyParentEvent.getUIParam, MyParentEvent.Notes);
            this.PopulateRepetitionParameters(MyRepeatCalendarEvent);
        }
        DateTimeOffset IncreaseByFrequency(DateTimeOffset MyTime, Frequency Frequency, string timeZone = "UTC")
        {
            switch (Frequency)
            {
                case Frequency.DAILY:
                    {
                        return MyTime.AddDays(1);
                    }
                case Frequency.WEEKLY:
                    {
                        return MyTime.AddDays(7);
                    }
                case Frequency.BIWEEKLY:
                    {
                        return MyTime.AddDays(14);
                    }
                case Frequency.MONTHLY:
                    {
                        return MyTime.AddMonths(1);
                    }
                case Frequency.YEARLY:
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

        public Frequency getFrequency
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
