using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace TilerElements
{
    public class Repetition: IUndoable
    {
        protected string _Id = Guid.NewGuid().ToString();
        protected string _ParentRepetitionId;
        protected string _LocationId;
        protected Frequency _RepetitionFrequency;
        protected TimeLine _RepetitionRange;//stores range for repetition so if assuming event happens on thursday from 9-11pm. The range is from today till november 31. The RepetitionRange will be today till November 31
        protected bool _EnableRepeat;
        protected ICollection<CalendarEvent> _RepeatingEvents;
        protected Dictionary<string, CalendarEvent> _DictionaryOfIDAndCalendarEvents;
        protected Dictionary<int, Repetition> _DictionaryOfWeekDayToRepetition = new Dictionary<int,Repetition>();
        protected Location _Location;
        protected TimeLine _initializingRange;//stores range for repetition so if assuming event happens on thursday from 9-11pm. The range is from today till november 31. The initializingRange will be 9-11pm
        protected int RepetitionWeekDay =7;
        static DayOfWeek[] Weekdays=new DayOfWeek[7]{DayOfWeek.Sunday,DayOfWeek.Monday,DayOfWeek.Tuesday,DayOfWeek.Wednesday,DayOfWeek.Thursday,DayOfWeek.Friday,DayOfWeek.Saturday};
        static DateTimeOffset CalculationStop = DateTimeOffset.UtcNow;
        public enum Frequency {DAILY, WEEKLY, MONTHLY, YEARLY, BIWEEKLY, NONE };
        protected string _UndoId;

        #region UndoMembers
        protected Frequency _UndoRepetitionFrequency;
        protected bool _UndoEnableRepeat;
        protected int _UndoRepetitionWeekDay = -5;
        protected Dictionary<int, Repetition> _UndoDictionaryOfWeekDayToRepetition = new Dictionary<int, Repetition>();
        protected DateTimeOffset _UndoInitializingRangeStart;
        protected DateTimeOffset _UndoInitializingRangeEnd;
        protected DateTimeOffset _UndoRepetitionRangeStart;
        protected DateTimeOffset _UndoRepetitionRangeEnd;

        #endregion

        public Repetition()
        {
            _RepetitionFrequency = Frequency.NONE;
            _RepetitionRange = new TimeLine();
            _EnableRepeat = false;
            _RepeatingEvents = new CalendarEvent[0];
            _Location = new Location();
            _initializingRange = new TimeLine();
            _DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
        }


        public Repetition(bool EnableFlag, TimeLine RepetitionRange_Entry, Frequency frequency, TimeLine EventActualRange, int[] WeekDayData)
        {
            _RepetitionRange = RepetitionRange_Entry;
            _RepetitionFrequency = frequency;
            _EnableRepeat = EnableFlag;
            _Location = new Location();
            _DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
            _initializingRange = EventActualRange;
            foreach (int eachWeekDay in WeekDayData)
            {
                _DictionaryOfWeekDayToRepetition.Add(eachWeekDay, new Repetition(EnableFlag, RepetitionRange_Entry.CreateCopy(), frequency, EventActualRange.CreateCopy(), eachWeekDay ));
            }
        }
        
        public Repetition(bool EnableFlag, TimeLine RepetitionRange_Entry, Frequency frequency, TimeLine EventActualRange, int WeekDayData=7 )
        {
            _RepetitionRange = RepetitionRange_Entry;
            _RepetitionFrequency = frequency;
            _EnableRepeat = EnableFlag;
            _Location = new Location();
            _DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
            _initializingRange = EventActualRange;
            RepetitionWeekDay = WeekDayData;
        }

        //public Repetition(bool EnableFlag,CalendarEvent BaseCalendarEvent,  TimeLine RepetitionRange_Entry, string Frequency)
        public Repetition(bool ReadFromFileEnableFlag, TimeLine ReadFromFileRepetitionRange_Entry, string ReadFromFileFrequency, CalendarEvent[] ReadFromFileRecurringListOfCalendarEvents, int DayOfWeek=7)
        {
            _EnableRepeat = ReadFromFileEnableFlag;
            _DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
            RepetitionWeekDay = DayOfWeek;
            foreach (CalendarEvent MyRepeatCalendarEvent in ReadFromFileRecurringListOfCalendarEvents)
            {
                _DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.getId, MyRepeatCalendarEvent);
            }

            _RepeatingEvents = _DictionaryOfIDAndCalendarEvents.Values.ToArray();
            _RepetitionFrequency = Utility.ParseEnum<Frequency>(ReadFromFileFrequency.ToUpper());
            _RepetitionRange = ReadFromFileRepetitionRange_Entry;
            if (ReadFromFileRecurringListOfCalendarEvents.Length > 0)
            {
                _Location = ReadFromFileRecurringListOfCalendarEvents[0].Location;
            }
        }


        public Repetition(bool ReadFromFileEnableFlag, TimeLine ReadFromFileRepetitionRange_Entry, string ReadFromFileFrequency, Repetition [] repetition_Weekday, int DayOfWeek=7)
        {
            _EnableRepeat = ReadFromFileEnableFlag;
            _DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
            RepetitionWeekDay = DayOfWeek;
            foreach (Repetition eachRepetition in repetition_Weekday)
            {
                _DictionaryOfWeekDayToRepetition.Add(eachRepetition.RepetitionWeekDay, eachRepetition);
            }
            

            _RepeatingEvents = _DictionaryOfIDAndCalendarEvents.Values.ToArray();
            _RepetitionFrequency = Utility.ParseEnum<Frequency>(ReadFromFileFrequency.ToUpper());
            _RepetitionRange = ReadFromFileRepetitionRange_Entry;
            if (repetition_Weekday.Length > 0)
            {
                _Location = repetition_Weekday[0]._Location;
            }
        }


        public CalendarEvent getCalendarEvent(string RepeatingEventID)
        {
            EventID eventId = new EventID(new EventID(RepeatingEventID).getIDUpToRepeatCalendarEvent());
            if (_DictionaryOfIDAndCalendarEvents.ContainsKey(eventId.ToString()))
            {
                return _DictionaryOfIDAndCalendarEvents[eventId.ToString()];
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
            _EnableRepeat = true;
            _RepetitionRange = MyParentEvent.Repeat.Range;
            _RepetitionFrequency = MyParentEvent.Repeat.getFrequency;
            if (_DictionaryOfWeekDayToRepetition.Count > 0)
            {
                foreach (KeyValuePair<int, Repetition> eachKeyValuePair in _DictionaryOfWeekDayToRepetition)
                {
                    eachKeyValuePair.Value.PopulateRepetitionParameters(MyParentEvent, eachKeyValuePair.Key);
                }
                return;
            }
            this.ParentEvent = MyParentEvent;

            RestrictionProfile restrictionProfile = new RestrictionProfile(_initializingRange.Start, _initializingRange.End - _initializingRange.Start);
            TimeLineRestricted segmentedTimeLine = new TimeLineRestricted(_RepetitionRange.Start, _RepetitionRange.End, restrictionProfile);
            TimeLine firstRepeatSequence = segmentedTimeLine.getTimeFrames().First();

            DateTimeOffset EachRepeatCalendarStart = firstRepeatSequence.Start;//Start DateTimeOffset Object for each recurring Calendar Event
            DateTimeOffset EachRepeatCalendarEnd = firstRepeatSequence.End;

            EventID MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
            CalendarEvent MyRepeatCalendarEvent;



            if (MyParentEvent.getRigid)
            {   
                EachRepeatCalendarStart = _initializingRange.Start;
                EachRepeatCalendarEnd = _initializingRange.End;
                MyRepeatCalendarEvent = new RigidCalendarEvent(MyParentEvent.getName, _initializingRange.Start, _initializingRange.End, MyParentEvent.getActiveDuration, MyParentEvent.getPreparation, MyParentEvent.getPreDeadline, new Repetition(), MyParentEvent.Location,  MyParentEvent.getUIParam, MyParentEvent.Notes, MyParentEvent.isEnabled, MyParentEvent.getIsComplete, MyParentEvent.getCreator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID);
            }
            else
            {
                EachRepeatCalendarStart = MyParentEvent.Start;
                EachRepeatCalendarEnd = MyParentEvent.Start.AddDays(1);
                if(EachRepeatCalendarEnd > MyParentEvent.Repeat.Range.End)
                {
                    EachRepeatCalendarEnd = MyParentEvent.Repeat.Range.End;
                    if((EachRepeatCalendarEnd - EachRepeatCalendarStart) < MyParentEvent.getActiveDuration)// if the duration of a subcalendar event cannot fit in the calendar event timeline then return.
                    {
                        return;
                    }
                }
                    MyRepeatCalendarEvent = new CalendarEvent(MyParentEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.getActiveDuration, MyParentEvent.getPreparation, MyParentEvent.getPreDeadline, MyParentEvent.NumberOfSplit, new Repetition(), MyParentEvent.myLocation, MyParentEvent.getUIParam, MyParentEvent.Notes, MyParentEvent.getProcrastinationInfo, MyParentEvent.getNowInfo, MyParentEvent.isEnabled, MyParentEvent.getIsComplete, MyParentEvent.getCreator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID);
            }

            if (EachRepeatCalendarStart > MyParentEvent.Repeat.Range.End || ((EachRepeatCalendarEnd - EachRepeatCalendarStart) < MyRepeatCalendarEvent.AverageTimeSpanPerSubEvent))
            {
                return;
            }

            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();

            for (; MyRepeatCalendarEvent.Start < MyParentEvent.Repeat.Range.End; )
            {
                MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                _DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.getId, MyRepeatCalendarEvent);
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, getFrequency); ;
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, getFrequency);
                if(EachRepeatCalendarStart > MyParentEvent.Repeat.Range.End || ((EachRepeatCalendarEnd - EachRepeatCalendarStart) < MyRepeatCalendarEvent.AverageTimeSpanPerSubEvent))
                {
                    break;
                }
                if (EachRepeatCalendarEnd > MyParentEvent.Repeat.Range.End)
                {
                    EachRepeatCalendarEnd = MyParentEvent.Repeat.Range.End;
                }
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
            _RepeatingEvents = _DictionaryOfIDAndCalendarEvents.Values.ToArray();
        }


        public void PopulateRepetitionParameters(CalendarEventRestricted MyParentEvent)
        {
            if (!MyParentEvent.Repeat.Enable)//Checks if Repetition object is enabled or disabled. If Disabled then just return else continue
            {
                return;
            }
            _EnableRepeat = true;
            _RepetitionRange = MyParentEvent.Repeat.Range;
            _RepetitionFrequency = MyParentEvent.Repeat.getFrequency;
            if (_DictionaryOfWeekDayToRepetition.Count > 0)
            {
                foreach (KeyValuePair<int, Repetition> eachKeyValuePair in _DictionaryOfWeekDayToRepetition)
                {
                    eachKeyValuePair.Value.PopulateRepetitionParameters(MyParentEvent, eachKeyValuePair.Key);
                }
                return;
            }

            RestrictionProfile restrictionProfile = new RestrictionProfile(_initializingRange.Start, _initializingRange.End - _initializingRange.Start);
            TimeLineRestricted segmentedTimeLine = new TimeLineRestricted(_RepetitionRange.Start, _RepetitionRange.End, restrictionProfile);
            TimeLine firstRepeatSequenct = segmentedTimeLine.getTimeFrames().First();

            DateTimeOffset EachRepeatCalendarStart = firstRepeatSequenct.Start;//Start DateTimeOffset Object for each recurring Calendar Event
            DateTimeOffset EachRepeatCalendarEnd = firstRepeatSequenct.End;

            EventID MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
            CalendarEventRestricted MyRepeatCalendarEvent = CalendarEventRestricted.InstantiateRepeatedCandidate(MyParentEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd,MyParentEvent.Calendar_EventID,MyParentEvent.RetrictionInfo, MyParentEvent.getActiveDuration,MyParentEvent.NumberOfSplit,MyParentEvent.Location,MyParentEvent.getUIParam,MyParentEvent.getRigid,MyParentEvent.getPreparation,MyParentEvent.ThirdPartyID);// MyParentEvent.Preparation, MyParentEvent.Rigid, new Repetition(), MyParentEvent.Rigid ? 1 : MyParentEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete);

            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();

            for (; MyRepeatCalendarEvent.Start < MyParentEvent.Repeat.Range.End; )
            {
                MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                _DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.getId, MyRepeatCalendarEvent);
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, getFrequency); ;
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, getFrequency);
                MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
                MyRepeatCalendarEvent = CalendarEventRestricted.InstantiateRepeatedCandidate(MyParentEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.Calendar_EventID, MyParentEvent.RetrictionInfo, MyParentEvent.getActiveDuration, MyParentEvent.NumberOfSplit, MyParentEvent.Location, MyParentEvent.getUIParam, MyParentEvent.getRigid, MyParentEvent.getPreparation, MyParentEvent.ThirdPartyID); //new CalendarEvent(MyEventCalendarID, MyRepeatCalendarEvent.Name, MyRepeatCalendarEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyRepeatCalendarEvent.Preparation, MyRepeatCalendarEvent.PreDeadline, MyRepeatCalendarEvent.Rigid, MyRepeatCalendarEvent.Repeat, MyRepeatCalendarEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled, MyParentEvent.UIParam, MyParentEvent.Notes, MyParentEvent.isComplete);

                MyRepeatCalendarEvent.Location = MyParentEvent.Location;
            }
            _RepeatingEvents = _DictionaryOfIDAndCalendarEvents.Values.ToArray();
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
            _RepetitionRange = MyParentEvent.Repeat.Range;
            _RepetitionFrequency = MyParentEvent.Repeat.getFrequency;
            _EnableRepeat = true;
            DateTimeOffset EachRepeatCalendarStart = _RepetitionRange.Start;
            DateTimeOffset EachRepeatCalendarEnd = _RepetitionRange.End;
            DateTimeOffset StartTimeLineForActity = getStartTimeForAppropriateWeek(_initializingRange.Start, Weekdays[WeekDay]);
            DateTimeOffset EndTimeLineForActity = StartTimeLineForActity.Add(_initializingRange.TimelineSpan);
            if((EachRepeatCalendarStart > _RepetitionRange.End) || (EachRepeatCalendarEnd - EachRepeatCalendarStart) < MyParentEvent.AverageTimeSpanPerSubEvent)
            {
                return;
            }

            TimeLine repetitionTimeline= new TimeLine(EachRepeatCalendarStart,EachRepeatCalendarEnd);
            TimeLine ActiveTimeline = new TimeLine(StartTimeLineForActity, EndTimeLineForActity);

            Repetition repetitionData = new Repetition(MyParentEvent.isEnabled, repetitionTimeline, this.getFrequency, ActiveTimeline,7);

            this.ParentEvent = MyParentEvent;
            this._initializingRange = ActiveTimeline;
            this._RepetitionRange = repetitionTimeline;
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
            _RepetitionRange = MyParentEvent.Repeat.Range;
            _RepetitionFrequency = MyParentEvent.Repeat.getFrequency;
            _EnableRepeat = true;
            DateTimeOffset EachRepeatCalendarStart = _RepetitionRange.Start;
            DateTimeOffset EachRepeatCalendarEnd = _RepetitionRange.End;
            DateTimeOffset StartTimeLineForActity = getStartTimeForAppropriateWeek(_initializingRange.Start, Weekdays[WeekDay]);
            DateTimeOffset EndTimeLineForActity = StartTimeLineForActity.Add(_initializingRange.TimelineSpan);

            TimeLine repetitionTimeline = new TimeLine(EachRepeatCalendarStart, EachRepeatCalendarEnd);
            TimeLine ActiveTimeline = new TimeLine(StartTimeLineForActity, EndTimeLineForActity);

            Repetition repetitionData = new Repetition(MyParentEvent.isEnabled, repetitionTimeline, this.getFrequency, ActiveTimeline, 7);


            this._initializingRange = ActiveTimeline;
            this._RepetitionRange = repetitionTimeline;
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


        public bool isExtraLayers()
        {
            return _DictionaryOfWeekDayToRepetition.Count > 0;
        }

        public List<Repetition> getDayRepetitions()
        {
            List<Repetition> retValue = _DictionaryOfWeekDayToRepetition.Values.ToList();
            return retValue;
        }

#region properties
        public bool Enable
        {
            get
            {
                return _EnableRepeat;
            }
        }

        public Frequency getFrequency
        {
            get
            {
                return _RepetitionFrequency;
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
                return _RepetitionRange;
            }
        }

        public TimeLine SingleInstanceTimeFrame
        {
            get
            {
                return _initializingRange;
            }
        }

        public Dictionary<int, Repetition> DayIndexToRepetition
        {
            get
            {
                Dictionary<int, Repetition> retValue = _DictionaryOfWeekDayToRepetition.ToDictionary(obj => obj.Key, obj => obj.Value);
                return retValue;
            }
        }

        public ICollection<CalendarEvent> RepeatingEvents
        {
            get
            {
                return _RepeatingEvents ?? (_RepeatingEvents = new List<CalendarEvent>());
            }
            set
            {
                _RepeatingEvents = value;
                _DictionaryOfIDAndCalendarEvents = _RepeatingEvents.ToDictionary(calEvent => calEvent.getId, calEvent => calEvent);
            }
        }

        public ICollection<Repetition> SubRepetitions
        {
            set
            {
                if (value != null)
                {
                    _DictionaryOfWeekDayToRepetition = value.ToDictionary(repetitionObj => repetitionObj.weekDay, repetitionObj => repetitionObj);
                }
                else
                {
                    _DictionaryOfWeekDayToRepetition = null;
                }
                
            }
            get
            {
                return this._DictionaryOfWeekDayToRepetition.Count > 0? this._DictionaryOfWeekDayToRepetition.Values : null;
            }
        }

        #region undoProperties
        public DateTimeOffset UndoInitializingRangeStart
        {
            get
            {
                return UndoInitializingRangeStart;
            }
            set
            {
                UndoInitializingRangeStart = value;
            }
        }
        public DateTimeOffset UndoInitializingRangeEnd
        {
            get
            {
                return _UndoInitializingRangeEnd;
            }
            set
            {
                _UndoInitializingRangeEnd = value;
            }
        }
        public DateTimeOffset UndoRepetitionRangeStart
        {
            get
            {
                return _UndoRepetitionRangeStart;
            }
            set
            {
                _UndoRepetitionRangeStart = value;
            }
        }
        public DateTimeOffset UndoRepetitionRangeEnd
        {
            get
            {
                return _UndoRepetitionRangeEnd;
            }
            set
            {
                _UndoRepetitionRangeEnd = value;
            }
        }

        public string UndoRepetitionFrequency
        {
            get
            {
                return _UndoRepetitionFrequency.ToString();
            }
            set
            {
                _UndoRepetitionFrequency = Utility.ParseEnum<Frequency>(value);
            }
        }
        public bool UndoEnableRepeat
        {
            get
            {
                return _UndoEnableRepeat;
            }
            set
            {
                _UndoEnableRepeat = value;
            }
        }
        public int UndoRepetitionWeekDay {
            get
            {
                return _UndoRepetitionWeekDay;
            }
            set
            {
                _UndoRepetitionWeekDay = value;
            }
        }
        #endregion

        #region dbproperties
        public string  RepetitionFrequency
        {
            get
            {
                return _RepetitionFrequency.ToString();
            }

            set
            {
                _RepetitionFrequency = Utility.ParseEnum<Frequency>(value);
            }
        }

        protected DateTimeOffset _RepetitionRangeStart;

        public DateTimeOffset RepetitionRangeStart
        {
            get
            {
                return _RepetitionRange.Start;
            }

            set
            {
                _RepetitionRangeStart = value;
                if(_RepetitionRange == null && _RepetitionRangeEnd != null && _RepetitionRangeStart != null)
                {
                    _RepetitionRange = new TimeLine(_RepetitionRangeStart, _RepetitionRangeEnd);
                }
            }
        }

        protected DateTimeOffset _RepetitionRangeEnd;

        public DateTimeOffset RepetitionRangeEnd
        {
            get
            {
                return _RepetitionRange.End;
            }

            set
            {
                _RepetitionRangeEnd = value;
                if (_RepetitionRange == null && _RepetitionRangeEnd != null && _RepetitionRangeStart != null)
                {
                    _RepetitionRange = new TimeLine(_RepetitionRangeStart, _RepetitionRangeEnd);
                }
            }
        }


        protected DateTimeOffset _initializingRangeStart;

        public DateTimeOffset initializingRangeStart
        {
            get
            {
                return _initializingRange.Start;
            }

            set
            {
                _initializingRangeStart = value;
                if (_initializingRange == null && _initializingRangeEnd != null && _initializingRangeStart != null)
                {
                    _initializingRange = new TimeLine(_initializingRangeStart, _initializingRangeEnd);
                }
            }
        }

        protected DateTimeOffset _initializingRangeEnd;

        public DateTimeOffset initializingRangeEnd
        {
            get
            {
                return _initializingRange.End;
            }

            set
            {
                _initializingRangeEnd = value;
                if (_initializingRange == null && _initializingRangeEnd != null && _initializingRangeStart != null)
                {
                    _initializingRange = new TimeLine(_initializingRangeStart, _initializingRangeEnd);
                }
            }
        }



        public bool EnableRepeat
        {
            get
            {
                return EnableRepeat;
            }

            set
            {
                EnableRepeat = value;
            }
        }

        public string LocationId
        {
            get
            {
                return _LocationId;
            }
            set
            {
                _LocationId = value;
            }
        }

        [ForeignKey("LocationId")]
        public Location Location
        {
            get
            {
                return _Location;
            }
            set
            {
                _Location = value;
            }
        }

        [ForeignKey("Id")]
        public CalendarEvent ParentEvent { get; set; }

        virtual public string Id
        {
            set
            {
                _Id = value;
            }
            get
            {
                return _Id;
            }
        }
        [ForeignKey("ParentRepetitionId")]
        public Repetition ParentRepetition { get; set; }

        virtual public string ParentRepetitionId
        {
            set
            {
                _ParentRepetitionId = value;
            }
            get
            {
                return _ParentRepetitionId;
            }
        }

        public ICollection<CalendarEvent> DictionaryOfIDAndCalendarEvents
        {
            get
            {
                return _DictionaryOfIDAndCalendarEvents.Values;
            }
            set
            {
                if (value != null)
                {
                    _DictionaryOfIDAndCalendarEvents = value.ToDictionary(eachValue => eachValue.Id, eachValue => eachValue);
                }
                else {
                    _DictionaryOfIDAndCalendarEvents = null;
                }
            }
        }

        public ICollection<Repetition> DictionaryOfWeekDayToRepetition
        {
            get
            {
                return _DictionaryOfWeekDayToRepetition.Values;
            }
            set
            {
                if (value != null)
                {
                    _DictionaryOfWeekDayToRepetition = value.ToDictionary(eachValue => eachValue.weekDay, eachValue => eachValue);
                }
                else
                {
                    _DictionaryOfWeekDayToRepetition = null;
                }
            }
        }

        public virtual bool FirstInstantiation { get; set; } = true;

        #endregion
        #endregion

        public CalendarEvent[] RecurringCalendarEvents()
        {
            CalendarEvent[] retValue = null;

            if (_DictionaryOfWeekDayToRepetition.Count > 0)
            {
                retValue = _DictionaryOfWeekDayToRepetition.SelectMany(obj => obj.Value.RecurringCalendarEvents()).ToArray();
            }
            else
            {
                retValue= _DictionaryOfIDAndCalendarEvents.Values.ToArray();
            }
            return retValue;
        }
       
        public Repetition CreateCopy()
        {
            Repetition repetition_cpy = new Repetition();
            if (this._RepeatingEvents.Count < 1)
            {
                return repetition_cpy;
            }
            repetition_cpy._RepetitionFrequency = this._RepetitionFrequency;
            repetition_cpy._RepetitionRange = this._RepetitionRange.CreateCopy();
            repetition_cpy._RepeatingEvents = _RepeatingEvents.AsParallel().Select(obj => obj.createCopy()).ToArray();
            repetition_cpy._Location = _Location.CreateCopy();
            repetition_cpy._EnableRepeat = _EnableRepeat;
            repetition_cpy.RepetitionWeekDay = RepetitionWeekDay;
            repetition_cpy._DictionaryOfIDAndCalendarEvents = _DictionaryOfIDAndCalendarEvents.AsParallel().ToDictionary(obj => obj.Key, obj1 => obj1.Value.createCopy());
            repetition_cpy._DictionaryOfWeekDayToRepetition = _DictionaryOfWeekDayToRepetition.AsParallel().ToDictionary(obj => obj.Key, obj1 => obj1.Value.CreateCopy());
            return repetition_cpy;
        }

        public void undoUpdate(Undo undo)
        {
            _UndoRepetitionFrequency = _RepetitionFrequency;
            _UndoEnableRepeat = this.EnableRepeat;
            Location.undoUpdate(undo);
            _UndoRepetitionWeekDay = RepetitionWeekDay;
            FirstInstantiation = false;
            this.UndoId = undo.id;
            List<CalendarEvent> AlreadyCreatedEvents = RepeatingEvents.Where(calEvent => !calEvent.FirstInstantiation).ToList();
            RepeatingEvents = AlreadyCreatedEvents;
            foreach (CalendarEvent calEvent in AlreadyCreatedEvents)
            {
                calEvent.undoUpdate(undo);
            }
            
            //protected ICollection<CalendarEvent> _UndoRepeatingEvents;
            //protected Dictionary<int, Repetition> _UndoDictionaryOfWeekDayToRepetition = new Dictionary<int, Repetition>();
        }

        public void undo(string undoId)
        {
            if (_UndoId == undoId)
            {
                Utility.Swap(ref _UndoRepetitionFrequency, ref _RepetitionFrequency);
                Utility.Swap(ref _UndoEnableRepeat, ref _EnableRepeat);
                Utility.Swap(ref _UndoInitializingRangeStart, ref _initializingRangeStart);
                Utility.Swap(ref _UndoInitializingRangeEnd, ref _initializingRangeEnd);
                Utility.Swap(ref _UndoRepetitionRangeStart, ref _RepetitionRangeStart);
                Utility.Swap(ref _UndoRepetitionRangeEnd, ref _RepetitionRangeEnd);
                foreach (KeyValuePair<int, Repetition> kvp in _DictionaryOfWeekDayToRepetition)
                {
                    kvp.Value.undo(undoId);
                }
                _Location.undo(undoId);
                foreach (CalendarEvent calEveent in RepeatingEvents)
                {
                    calEveent.undo(undoId);
                }
            }
        }

        public void redo(string undoId)
        {
            if (_UndoId == undoId)
            {
                Utility.Swap(ref _UndoRepetitionFrequency, ref _RepetitionFrequency);
                Utility.Swap(ref _UndoEnableRepeat, ref _EnableRepeat);
                Utility.Swap(ref _UndoInitializingRangeStart, ref _initializingRangeStart);
                Utility.Swap(ref _UndoInitializingRangeEnd, ref _initializingRangeEnd);
                Utility.Swap(ref _UndoRepetitionRangeStart, ref _RepetitionRangeStart);
                Utility.Swap(ref _UndoRepetitionRangeEnd, ref _RepetitionRangeEnd);
                foreach (KeyValuePair<int, Repetition> kvp in _DictionaryOfWeekDayToRepetition)
                {
                    kvp.Value.undo(undoId);
                }
                _Location.undo(undoId);
                throw new NotImplementedException("You need to implement the implement the undo for _RepeatingEvents");
            }
        }

        public string UndoId
        {
            set
            {
                _UndoId = value;
            }
            get
            {
                return _UndoId;
            }
        }
    }  
}
