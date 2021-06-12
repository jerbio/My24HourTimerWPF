﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace TilerElements
{
    public class Repetition : IUndoable, IHasId
    {
        protected string _Id = Guid.NewGuid().ToString();
        protected string _ParentRepetitionId;
        protected string _LocationId;
        protected Frequency _RepetitionFrequency;
        protected TimeLine _RepetitionRange;//stores range for repetition so if assuming event happens on thursday from 9-11pm. The range is from today till november 31. The RepetitionRange will be today till November 31
        protected bool _EnableRepeat;
        protected SubEventDictionary<string, CalendarEvent> _DictionaryOfIDAndCalendarEvents;
        protected SubEventDictionary<int, Repetition> _DictionaryOfWeekDayToRepetition;
        protected Location _Location;
        protected TimeLine _initializingRange;//stores range for repetition so if assuming event happens on thursday from 9-11pm. The range is from today till november 31. The initializingRange will be 9-11pm
        protected int RepetitionWeekDay = 7;
        protected Repetition parentRepetition;
        protected static DayOfWeek[] Weekdays = new DayOfWeek[7] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
        static DateTimeOffset CalculationStop = DateTimeOffset.UtcNow;
        public enum Frequency { DAILY, WEEKLY, MONTHLY, YEARLY, BIWEEKLY, NONE };
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

        #region Constructors
        protected Repetition()
        {

        }

        public Repetition(bool isInitialaized = true)
        {
            _RepetitionFrequency = Frequency.NONE;
            _RepetitionRange = new TimeLine();
            _EnableRepeat = false;
            _Location = new Location();
            _initializingRange = new TimeLine();
            _DictionaryOfIDAndCalendarEvents = new SubEventDictionary<string, CalendarEvent>();
            _DictionaryOfWeekDayToRepetition = new SubEventDictionary<int, Repetition>();
        }

        /// <summary>
        /// RepetitionRange_Entry is the timeline from the start time till when the repetition is supposed to stop repeating
        /// Frequency is the repetition frequency. Note if it is daily, WeekDayData will be ignored
        /// </summary>
        /// <param name="RepetitionRange_Entry"></param>
        /// <param name="frequency"></param>
        /// <param name="EventActualRange"></param>
        /// <param name="WeekDayData"></param>
        public Repetition(TimeLine RepetitionRange_Entry, Frequency frequency, TimeLine EventActualRange, DayOfWeek[] WeekDayData)
        {
            _RepetitionRange = RepetitionRange_Entry;
            _RepetitionFrequency = frequency;
            _EnableRepeat = true;
            _Location = new Location();
            _DictionaryOfIDAndCalendarEvents = new SubEventDictionary<string, CalendarEvent>();
            _DictionaryOfWeekDayToRepetition = new SubEventDictionary<int, Repetition>();
            _initializingRange = EventActualRange;
            if (_RepetitionFrequency != Frequency.DAILY)
            {
                foreach (DayOfWeek dayOfWeek in WeekDayData)
                {
                    int eachWeekDay = (int)dayOfWeek;
                    Repetition repetition = new Repetition(RepetitionRange_Entry.CreateCopy(), frequency, EventActualRange.CreateCopy(), eachWeekDay);
                    repetition.parentRepetition = this;
                    _DictionaryOfWeekDayToRepetition.Add(eachWeekDay, repetition);
                }
            }
        }

        public Repetition(TimeLine RepetitionRange_Entry, Frequency frequency, TimeLine EventActualRange, int WeekDayData = 7)
        {
            _RepetitionRange = RepetitionRange_Entry;
            _RepetitionFrequency = frequency;
            _EnableRepeat = true;
            _Location = new Location();
            _DictionaryOfIDAndCalendarEvents = new SubEventDictionary<string, CalendarEvent>();
            _DictionaryOfWeekDayToRepetition = new SubEventDictionary<int, Repetition>();
            _initializingRange = EventActualRange;
            RepetitionWeekDay = WeekDayData;
        }

        //public Repetition(bool EnableFlag,CalendarEvent BaseCalendarEvent,  TimeLine RepetitionRange_Entry, string Frequency)
        public Repetition(bool ReadFromFileEnableFlag, TimeLine ReadFromFileRepetitionRange_Entry, string ReadFromFileFrequency, CalendarEvent[] ReadFromFileRecurringListOfCalendarEvents, int DayOfWeek = 7)
        {
            _EnableRepeat = false;
            _DictionaryOfIDAndCalendarEvents = new SubEventDictionary<string, CalendarEvent>();
            _DictionaryOfWeekDayToRepetition = new SubEventDictionary<int, Repetition>();
            RepetitionWeekDay = DayOfWeek;
            if (ReadFromFileRecurringListOfCalendarEvents.Length > 0)
            {
                foreach (CalendarEvent MyRepeatCalendarEvent in ReadFromFileRecurringListOfCalendarEvents)
                {
                    _DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.getId, MyRepeatCalendarEvent);
                    MyRepeatCalendarEvent.IsRepeatsChildCalEvent = true;
                }
                _Location = ReadFromFileRecurringListOfCalendarEvents[0].LocationObj;
                _EnableRepeat = ReadFromFileEnableFlag;
            }

            _RepetitionFrequency = Utility.ParseEnum<Frequency>(ReadFromFileFrequency.ToUpper());
            _RepetitionRange = ReadFromFileRepetitionRange_Entry;
        }


        public Repetition(bool ReadFromFileEnableFlag, TimeLine ReadFromFileRepetitionRange_Entry, string ReadFromFileFrequency, Repetition[] repetition_Weekday, int DayOfWeek = 7)
        {
            _EnableRepeat = ReadFromFileEnableFlag;
            _DictionaryOfIDAndCalendarEvents = new SubEventDictionary<string, CalendarEvent>();
            RepetitionWeekDay = DayOfWeek;
            foreach (Repetition eachRepetition in repetition_Weekday)
            {
                _DictionaryOfWeekDayToRepetition.Add(eachRepetition.RepetitionWeekDay, eachRepetition);
            }


            _RepetitionFrequency = Utility.ParseEnum<Frequency>(ReadFromFileFrequency.ToUpper());
            _RepetitionRange = ReadFromFileRepetitionRange_Entry;
            if (repetition_Weekday.Length > 0)
            {
                _Location = repetition_Weekday[0]._Location;
            }
        }

        #endregion
        public CalendarEvent getCalendarEvent(string RepeatingEventID)
        {
            EventID eventId = new EventID(new EventID(RepeatingEventID).getIDUpToRepeatCalendarEvent());
            CalendarEvent retValue = null;
            if (_DictionaryOfWeekDayToRepetition != null && _DictionaryOfWeekDayToRepetition.Count > 0)
            {
                retValue = _DictionaryOfWeekDayToRepetition.Values.Select(obj => obj.getCalendarEvent(RepeatingEventID)).Where(obj => obj != null).SingleOrDefault();
            }
            else
            {
                if (_DictionaryOfIDAndCalendarEvents.ContainsKey(eventId.ToString()))
                {
                    return _DictionaryOfIDAndCalendarEvents[eventId.ToString()];
                }
                else
                {
                    return null;
                }
            }
            return retValue;

        }

        DayOfWeek getBeginningOfRepeatWeekDay(CalendarEvent MyParentEvent)
        {
            DayOfWeek[] dayOfWeekOrder = new DayOfWeek[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
            TilerUser tilerUser = MyParentEvent.getCreator ?? MyParentEvent.DefaultCalendarEvent.getCreator;
            DayOfWeek beginningOfWeek = dayOfWeekOrder.First();
            if (tilerUser != null)
            {
                beginningOfWeek = tilerUser.BeginningOfWeek;
            }
            return beginningOfWeek;
        }
        /// <summary>
        /// this function of repetition, is responsible for populating the repetition object in the passed CalendarEvent.
        /// </summary>
        /// <param name="MyParentEvent"></param>
        public void PopulateRepetitionParameters(CalendarEvent MyParentEvent)
        {

            TimeLine calendarTimeLine = null;
            if (!MyParentEvent.IsRecurring)//Checks if Repetition object is enabled or disabled. If Disabled then just return else continue
            {
                return;
            }
            _EnableRepeat = true;
            _RepetitionRange = MyParentEvent.Repeat.Range;
            _RepetitionFrequency = MyParentEvent.Repeat.getFrequency;
            if (this._DictionaryOfWeekDayToRepetition.Count > 0)
            {
                foreach (KeyValuePair<string, Repetition> eachKeyValuePair in _DictionaryOfWeekDayToRepetition)
                {
                    eachKeyValuePair.Value.PopulateRepetitionParameters(MyParentEvent, Convert.ToInt32(eachKeyValuePair.Key));
                }
                MyParentEvent.deleteAllSubCalendarEventsFromRepeatParentCalendarEvent();
                return;
            }
            this.ParentEvent = MyParentEvent;


            DateTimeOffset IniRepeatStart;
            DateTimeOffset IniRepeatEnd;
            DateTimeOffset EachRepeatCalendarStart;//Start DateTimeOffset Object for each recurring Calendar Event
            DateTimeOffset EachRepeatCalendarEnd;



            EventID MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
            CalendarEvent MyRepeatCalendarEvent;
            int splitCount = MyParentEvent.NumberOfSplit;


            if (MyParentEvent.isRigid)
            {

                EachRepeatCalendarStart = _initializingRange.Start;
                EachRepeatCalendarEnd = _initializingRange.End;
                MyRepeatCalendarEvent = new RigidCalendarEvent(MyParentEvent.getName, _initializingRange.Start, _initializingRange.End, MyParentEvent.getActiveDuration, MyParentEvent.getPreparation, MyParentEvent.getPreDeadline, new Repetition(), MyParentEvent.LocationObj, MyParentEvent.getUIParam, MyParentEvent.Notes, MyParentEvent.isEnabled, MyParentEvent.getIsComplete, MyParentEvent.getCreator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID, MyParentEvent.getNowInfo, MyParentEvent.TimeLineHistory);
                calendarTimeLine = new TimeLine(EachRepeatCalendarStart, EachRepeatCalendarEnd);
            }
            else
            {
                ReferenceNow now = new ReferenceNow(_initializingRange.Start, MyParentEvent.getCreator.EndfOfDay, new TimeSpan());
                TimeLine dayTImeLine = now.getDayTimeLineByTime(_initializingRange.Start);
                IniRepeatStart = EachRepeatCalendarStart = dayTImeLine.Start;//Start DateTimeOffset Object for each recurring Calendar Event
                IniRepeatEnd = EachRepeatCalendarEnd = IncreaseByFrequency(dayTImeLine.Start, getFrequency, MyParentEvent.getCreator.TimeZone);//  we want the calendar event range to end based on what repeat frequency is selected. E.g if the Calendar event is WEEKLY and the initial range from 10/14/2018 - 10/23/2018(which is over a week), then the we want each calendar event to weekly, so the first calendar event will be 10/14/2018 12:00am - 10/21/2018 11:59PM
                int daySpan = getFrequency == Frequency.WEEKLY ? 7 : getFrequency == Frequency.BIWEEKLY ? 14 : -1;
                if (weekDay != 7)// if a weekday was selected then intuitive user will expect it to only occur on twentyfour hour bases. I just don't want to deal with weird corner cases
                {
                    IniRepeatEnd = EachRepeatCalendarEnd = dayTImeLine.End;
                }
                else
                {
                    if (getFrequency == Frequency.WEEKLY || getFrequency == Frequency.BIWEEKLY)/// this handles the scenario where the repetition is weekly and the user's beginning of week starts on thursdays but the event was created on a monday. A partial calendar event should be created with auto deleted subevents
                    {
                        DayOfWeek dayOfWeek = now.getDayOfTheWeek(EachRepeatCalendarStart).Item1;
                        int dayOfWeekIndex = (int)dayOfWeek;
                        DayOfWeek beginningOfWeek_Day = getBeginningOfRepeatWeekDay(MyParentEvent);
                        int beginningWeekDayIndex = (int)beginningOfWeek_Day;
                        int dayDiff = ((beginningWeekDayIndex - dayOfWeekIndex) + daySpan) % daySpan;
                        dayDiff = dayDiff == 0 ? daySpan : dayDiff;/// handles scenario if the current day is the same day as start of week
                        long endDayIndex = now.getDayIndexFromStartOfTime(EachRepeatCalendarStart) + dayDiff;
                        DayTimeLine endDayTimeLine = now.getDayTimeLineByDayIndex(endDayIndex);
                        IniRepeatEnd = EachRepeatCalendarEnd = endDayTimeLine.Start.AddMinutes(-1);
                        TimeSpan timeSpanForPossiblePartialFrequencyFrame = TimeSpan.FromDays(daySpan);
                        double ratio = (IniRepeatEnd - IniRepeatStart).TotalSeconds / timeSpanForPossiblePartialFrequencyFrame.TotalSeconds;
                        splitCount = (int)Math.Round(ratio * MyParentEvent.NumberOfSplit);

                        EachRepeatCalendarStart = IniRepeatEnd.AddMinutes(1).AddDays(-daySpan);
                        EachRepeatCalendarEnd = IniRepeatEnd;
                    }
                }
                calendarTimeLine = new TimeLine(EachRepeatCalendarStart, EachRepeatCalendarEnd);
                DateTimeOffset repeatEnd = IniRepeatEnd;
                if (repeatEnd > MyParentEvent.Repeat.Range.End)
                {
                    repeatEnd = MyParentEvent.Repeat.Range.End;
                    if ((repeatEnd - MyParentEvent.Start) < MyParentEvent.getActiveDuration)// if the duration of a subcalendar event cannot fit in the calendar event timeline then return.
                    {
                        return;
                    }
                }
                MyRepeatCalendarEvent = new CalendarEvent(MyParentEvent.getName, IniRepeatStart, repeatEnd, MyParentEvent.getActiveDuration, MyParentEvent.getPreparation, MyParentEvent.getPreDeadline, MyParentEvent.NumberOfSplit, new Repetition(), MyParentEvent.LocationObj, MyParentEvent.getUIParam, MyParentEvent.Notes, MyParentEvent.getProcrastinationInfo, MyParentEvent.getNowInfo, MyParentEvent.isEnabled, MyParentEvent.getIsComplete, MyParentEvent.getCreator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID, MyParentEvent.TimeLineHistory);
                int autoDisableCount = MyParentEvent.NumberOfSplit - splitCount;
                var subEvents = MyRepeatCalendarEvent.ActiveSubEvents.OrderBy(o => o.Start).Take(autoDisableCount).ToList();
                foreach (SubCalendarEvent subEvent in subEvents)
                {
                    subEvent.autoDisable(subEvent.ParentCalendarEvent, Reason.AutoDeletion.RepetitionSpanTooSmall);
                }
            }
            MyRepeatCalendarEvent.IsRepeatsChildCalEvent = true;
            MyRepeatCalendarEvent.setDayPreference(ParentEvent.DayPreference);
            MyRepeatCalendarEvent.setRepeatParent(MyParentEvent);
            //EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, getFrequency);
            //EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, getFrequency);
            calendarTimeLine = new TimeLine(EachRepeatCalendarStart, EachRepeatCalendarEnd);
            foreach (SubCalendarEvent subEvent in MyRepeatCalendarEvent.AllSubEvents)
            {
                subEvent.RepeatParentEvent = MyParentEvent;
            }
            if (EachRepeatCalendarStart > MyParentEvent.Repeat.Range.End)
            {
                return;
            }

            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();


            _DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.getId, MyRepeatCalendarEvent);
            this.extendTimeline(calendarTimeLine, MyParentEvent);
        }
        public void PopulateRepetitionParameters(CalendarEventRestricted MyParentEvent)
        {
            if (!MyParentEvent.IsRecurring)//Checks if Repetition object is enabled or disabled. If Disabled then just return else continue
            {
                return;
            }
            _EnableRepeat = true;
            _RepetitionRange = MyParentEvent.Repeat.Range;
            _RepetitionFrequency = MyParentEvent.Repeat.getFrequency;
            if (_DictionaryOfWeekDayToRepetition.Count > 0)
            {
                foreach (KeyValuePair<string, Repetition> eachKeyValuePair in _DictionaryOfWeekDayToRepetition)
                {
                    eachKeyValuePair.Value.PopulateRepetitionParameters(MyParentEvent, Convert.ToInt32(eachKeyValuePair.Key));
                }
                MyParentEvent.deleteAllSubCalendarEventsFromRepeatParentCalendarEvent();
                return;
            }
            this.ParentEvent = MyParentEvent;
            ReferenceNow now = new ReferenceNow(_initializingRange.Start, MyParentEvent.getCreator.EndfOfDay, new TimeSpan());
            TimeLine dayTImeLine = now.getDayTimeLineByTime(_initializingRange.Start);
            DateTimeOffset EachRepeatCalendarStart = dayTImeLine.Start;//Start DateTimeOffset Object for each recurring Calendar Event
            DateTimeOffset EachRepeatCalendarEnd = IncreaseByFrequency(dayTImeLine.Start, getFrequency, MyParentEvent.getCreator.TimeZone);//  we want the calendar event range to end based on what repeat frequency is selected. E.g if the Calendar event is WEEKLY and the initial range from 10/14/2018 - 10/23/2018(which is over a week), then the we want each calendar event to weekly, so the first calendar event will be 10/14/2018 12:00am - 10/21/2018 11:59PM
            DateTimeOffset IniRepeatStart = EachRepeatCalendarStart;
            DateTimeOffset IniRepeatEnd = EachRepeatCalendarEnd;
            int daySpan = getFrequency == Frequency.WEEKLY ? 7 : getFrequency == Frequency.BIWEEKLY ? 14 : -1;
            int splitCount = MyParentEvent.NumberOfSplit;
            if (weekDay != 7)// if a weekday was selected then intuitive user will expect it to only occur on twentyfour hour bases. I just don't want to deal with weird corner cases
            {
                IniRepeatEnd = EachRepeatCalendarEnd = dayTImeLine.End;
            }
            else
            {
                if (getFrequency == Frequency.WEEKLY || getFrequency == Frequency.BIWEEKLY)/// this handles the scenario where the repetition is weekly and the user's beginning of week starts on thursdays but the event was created on a monday. A partial calendar event should be created with auto deleted subevents
                {
                    DayOfWeek dayOfWeek = now.getDayOfTheWeek(EachRepeatCalendarStart).Item1;
                    int dayOfWeekIndex = (int)dayOfWeek;
                    DayOfWeek beginningOfWeek_Day = getBeginningOfRepeatWeekDay(MyParentEvent);
                    int beginningWeekDayIndex = (int)beginningOfWeek_Day;
                    int dayDiff = ((beginningWeekDayIndex - dayOfWeekIndex) + daySpan) % daySpan;
                    dayDiff = dayDiff == 0 ? 7 : dayDiff;/// handles scenario if the current day is the same day as start of week
                    long endDayIndex = now.getDayIndexFromStartOfTime(EachRepeatCalendarStart) + dayDiff;
                    DayTimeLine endDayTimeLine = now.getDayTimeLineByDayIndex(endDayIndex);
                    IniRepeatEnd = EachRepeatCalendarEnd = endDayTimeLine.Start.AddMinutes(-1);
                    TimeSpan timeSpanForPossiblePartialFrequencyFrame = TimeSpan.FromDays(daySpan);
                    double ratio = (IniRepeatEnd - IniRepeatStart).TotalSeconds / timeSpanForPossiblePartialFrequencyFrame.TotalSeconds;
                    splitCount = (int)Math.Round(ratio * MyParentEvent.NumberOfSplit);


                    EachRepeatCalendarStart = IniRepeatEnd.AddMinutes(1).AddDays(-daySpan);
                    EachRepeatCalendarEnd = IniRepeatEnd;
                }
            }
            DateTimeOffset repeatEnd = IniRepeatEnd;
            if (repeatEnd > MyParentEvent.Repeat.Range.End)
            {
                repeatEnd = MyParentEvent.Repeat.Range.End;
                if ((repeatEnd - MyParentEvent.Start) < MyParentEvent.getActiveDuration)// if the duration of a subcalendar event cannot fit in the calendar event timeline then return.
                {
                    return;
                }
            }


            EventID MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
            TimeLineRestricted restrictionTimeLine = new TimeLineRestricted(EachRepeatCalendarStart, MyParentEvent.Repeat.Range.End, MyParentEvent.RestrictionProfile, now);
            TimeLine calendarTimeLine = new TimeLine(IniRepeatStart, repeatEnd);
            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();
            CalendarEventRestricted MyRepeatCalendarEvent = null;
            if (restrictionTimeLine.doesTimeLineInterfere(calendarTimeLine))
            {
                MyRepeatCalendarEvent = CalendarEventRestricted.InstantiateRepeatedCandidate(MyParentEvent.getName, calendarTimeLine.Start, calendarTimeLine.End, MyParentEvent.Calendar_EventID, MyParentEvent.RestrictionProfile, MyParentEvent.getActiveDuration, MyParentEvent.NumberOfSplit, MyParentEvent.LocationObj, MyParentEvent.getUIParam, MyParentEvent.isLocked, MyParentEvent.getPreparation, MyParentEvent.ThirdPartyID, MyParentEvent.Now, MyParentEvent.getCreator, MyParentEvent.Notes, MyParentEvent.getNowInfo, MyParentEvent.Procrastination_EventDB);

                int autoDisableCount = MyParentEvent.NumberOfSplit - splitCount;
                var subEvents = MyRepeatCalendarEvent.ActiveSubEvents.OrderBy(o => o.Start).Take(autoDisableCount).ToList();
                foreach (SubCalendarEvent subEvent in subEvents)
                {
                    subEvent.autoDisable(subEvent.ParentCalendarEvent, Reason.AutoDeletion.RepetitionSpanTooSmall);
                }
                MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                _DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.getId, MyRepeatCalendarEvent);
                MyRepeatCalendarEvent.setDayPreference(ParentEvent.DayPreference);
                foreach (SubCalendarEvent subEvent in MyRepeatCalendarEvent.AllSubEvents)
                {
                    subEvent.RepeatParentEvent = MyParentEvent;
                }
                MyRepeatCalendarEvent.IsRepeatsChildCalEvent = true;
                MyRepeatCalendarEvent.setDayPreference(ParentEvent.DayPreference);
                MyRepeatCalendarEvent.setTimeLineHistory(ParentEvent.TimeLineHistory);
                MyRepeatCalendarEvent.setRepeatParent(MyParentEvent);
                MyRepeatCalendarEvent.Location = MyParentEvent.LocationObj;
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, getFrequency);
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, getFrequency);
                calendarTimeLine = new TimeLine(EachRepeatCalendarStart, EachRepeatCalendarEnd);
            }


            this.extendTimeline(calendarTimeLine, MyParentEvent as CalendarEventRestricted);
            //for (; calendarTimeLine.End < MyParentEvent.Repeat.Range.End;)
            //{
            //    if (restrictionTimeLine.doesTimeLineInterfere(calendarTimeLine))
            //    {

            //        MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
            //        MyRepeatCalendarEvent = CalendarEventRestricted.InstantiateRepeatedCandidate(MyParentEvent.getName, calendarTimeLine.Start, calendarTimeLine.End, MyParentEvent.Calendar_EventID, MyParentEvent.RestrictionProfile, MyParentEvent.getActiveDuration, MyParentEvent.NumberOfSplit, MyParentEvent.LocationObj, MyParentEvent.getUIParam, MyParentEvent.isLocked, MyParentEvent.getPreparation, MyParentEvent.ThirdPartyID, MyParentEvent.Now, MyParentEvent.getCreator, MyParentEvent.Notes, MyParentEvent.getNowInfo, MyParentEvent.Procrastination_EventDB);
            //        MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
            //        _DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.getId, MyRepeatCalendarEvent);
            //        MyRepeatCalendarEvent.setDayPreference(ParentEvent.DayPreference);
            //        foreach (SubCalendarEvent subEvent in MyRepeatCalendarEvent.AllSubEvents)
            //        {
            //            subEvent.RepeatParentEvent = MyParentEvent;
            //        }
            //        MyRepeatCalendarEvent.IsRepeatsChildCalEvent = true;
            //        MyRepeatCalendarEvent.setDayPreference(ParentEvent.DayPreference);
            //        MyRepeatCalendarEvent.setTimeLineHistory(ParentEvent.TimeLineHistory);
            //        MyRepeatCalendarEvent.setRepeatParent(MyParentEvent);
            //        MyRepeatCalendarEvent.Location = MyParentEvent.LocationObj;
            //    }
            //    EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, getFrequency);
            //    EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, getFrequency);
            //    calendarTimeLine = new TimeLine(EachRepeatCalendarStart, EachRepeatCalendarEnd);
            //}
            //MyParentEvent.deleteAllSubCalendarEventsFromRepeatParentCalendarEvent();
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
        { // the bug is here. This does not enforce the day of the week for the week of the day sub events
            // trouble shoot line 171. See why the monday timeline isnt selected it keeps everyday
            _RepetitionRange = MyParentEvent.Repeat.Range;
            _RepetitionFrequency = MyParentEvent.Repeat.getFrequency;
            _EnableRepeat = true;
            DateTimeOffset EachRepeatCalendarStart = getStartTimeForAppropriateWeek(_RepetitionRange.Start, Weekdays[WeekDay]); //_RepetitionRange.Start;
            DateTimeOffset EachRepeatCalendarEnd = _RepetitionRange.End;
            DateTimeOffset StartTimeLineForActity = getStartTimeForAppropriateWeek(_initializingRange.Start, Weekdays[WeekDay]);
            DateTimeOffset EndTimeLineForActity = StartTimeLineForActity.Add(_initializingRange.TimelineSpan);
            if ((EachRepeatCalendarStart > _RepetitionRange.End) || (EachRepeatCalendarEnd - EachRepeatCalendarStart) < MyParentEvent.AverageTimeSpanPerSubEvent)
            {
                return;
            }

            TimeLine repetitionTimeline = new TimeLine(EachRepeatCalendarStart, EachRepeatCalendarEnd);
            TimeLine ActiveTimeline = new TimeLine(StartTimeLineForActity, EndTimeLineForActity);

            Repetition repetitionData = new Repetition(repetitionTimeline, this.getFrequency, ActiveTimeline, 7);
            repetitionData.ParentEvent = MyParentEvent;
            MyParentEvent.isRepeatLoaded_DB = true;

            this.ParentEvent = MyParentEvent;
            this._initializingRange = ActiveTimeline;
            this._RepetitionRange = repetitionTimeline;
            EventID MyEventCalendarID = EventID.GenerateRepeatDayCalendarEvent(MyParentEvent.getId, WeekDay);
            CalendarEvent MyRepeatCalendarEvent;
            if (MyParentEvent.isRigid)
            {
                MyRepeatCalendarEvent = new RigidCalendarEvent(MyParentEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.getActiveDuration, MyParentEvent.getPreparation, MyParentEvent.getPreDeadline, repetitionData, MyParentEvent.Location, MyParentEvent.getUIParam, MyParentEvent.Notes, MyParentEvent.isEnabled, MyParentEvent.getIsComplete, MyParentEvent.getCreator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID, MyParentEvent.getNowInfo, MyParentEvent.TimeLineHistory);
            }
            else
            {
                MyRepeatCalendarEvent = new CalendarEvent(MyParentEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.getActiveDuration, MyParentEvent.getPreparation, MyParentEvent.getPreDeadline, MyParentEvent.NumberOfSplit, repetitionData, MyParentEvent.Location, MyParentEvent.getUIParam, MyParentEvent.Notes, MyParentEvent.getProcrastinationInfo, MyParentEvent.getNowInfo, MyParentEvent.isEnabled, MyParentEvent.getIsComplete, MyParentEvent.getCreator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID, MyParentEvent.TimeLineHistory);
            }
            MyRepeatCalendarEvent.IsRepeatsChildCalEvent = true;
            MyRepeatCalendarEvent.setDayPreference(ParentEvent.DayPreference);
            MyRepeatCalendarEvent.setRepeatParent(MyParentEvent);
            foreach (SubCalendarEvent subEvent in MyRepeatCalendarEvent.AllSubEvents)
            {
                subEvent.RepeatParentEvent = MyParentEvent;
            }
            this.PopulateRepetitionParameters(MyRepeatCalendarEvent);
            MyRepeatCalendarEvent.deleteAllSubCalendarEventsFromRepeatParentCalendarEvent();
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
            this.ParentEvent = MyParentEvent;

            Repetition repetitionData = new Repetition(repetitionTimeline, this.getFrequency, ActiveTimeline, 7);
            repetitionData.ParentEvent = MyParentEvent;
            MyParentEvent.isRepeatLoaded_DB = true;

            this._initializingRange = ActiveTimeline;
            this._RepetitionRange = repetitionTimeline;
            EventID MyEventCalendarID = EventID.GenerateRepeatDayCalendarEvent(MyParentEvent.getId, WeekDay);
            CalendarEventRestricted MyRepeatCalendarEvent = new CalendarEventRestricted(MyParentEvent.getCreator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyParentEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.RestrictionProfile, MyParentEvent.getActiveDuration, repetitionData, MyParentEvent.getIsComplete, MyParentEvent.isEnabled, MyParentEvent.isLocked ? 1 : MyParentEvent.NumberOfSplit, MyParentEvent.isLocked, MyParentEvent.getNowInfo, MyParentEvent.LocationObj, MyParentEvent.getPreparation, MyParentEvent.getPreDeadline, MyEventCalendarID, MyParentEvent.Now, MyParentEvent.Procrastination_EventDB, MyParentEvent.TimeLineHistory, MyParentEvent.getUIParam, MyParentEvent.Notes);
            MyRepeatCalendarEvent.IsRepeatsChildCalEvent = true;
            MyRepeatCalendarEvent.setDayPreference(ParentEvent.DayPreference);
            MyRepeatCalendarEvent.Location = MyParentEvent.LocationObj;
            MyRepeatCalendarEvent.setRepeatParent(MyParentEvent);
            foreach (SubCalendarEvent subEvent in MyRepeatCalendarEvent.AllSubEvents)
            {
                subEvent.RepeatParentEvent = MyParentEvent;
            }
            this.PopulateRepetitionParameters(MyRepeatCalendarEvent);
            MyRepeatCalendarEvent.deleteAllSubCalendarEventsFromRepeatParentCalendarEvent();
        }
        
        private void extendTimeline(TimeLine timeLine, CalendarEvent MyParentEvent)
        {
            EventID MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();
            DateTimeOffset EachRepeatCalendarStart = timeLine.Start < MyParentEvent.Start ? timeLine.Start : MyParentEvent.Start;
            DateTimeOffset EachRepeatCalendarEnd = timeLine.End;
            TimeLine calendarTimeLine = timeLine.StartToEnd;
            CalendarEvent MyRepeatCalendarEvent = null;
            for (; calendarTimeLine.Start < MyParentEvent.Repeat.Range.End;)
            {
                if (MyRepeatCalendarEvent != null)
                {
                    if (MyRepeatCalendarEvent.StartToEnd.TimelineSpan >= MyRepeatCalendarEvent.AverageTimeSpanPerSubEvent)
                    {
                        MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                        _DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.getId, MyRepeatCalendarEvent);
                    }
                }

                TimeSpan repeatSpan = EachRepeatCalendarEnd - EachRepeatCalendarStart;
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, getFrequency); ;
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, getFrequency);
                
                TimeSpan nextCalTimeSpan = EachRepeatCalendarEnd - EachRepeatCalendarStart;
                //if (EachRepeatCalendarEnd > timeLine.End)
                //{
                //    EachRepeatCalendarEnd = timeLine.End;
                //}
                calendarTimeLine = new TimeLine(EachRepeatCalendarStart, EachRepeatCalendarEnd);
                
                if ( // if start time is past the repeat timeline
                    ((MyRepeatCalendarEvent != null) && ((EachRepeatCalendarEnd - EachRepeatCalendarStart) < MyRepeatCalendarEvent.AverageTimeSpanPerSubEvent)) || // if timespan is less than time span of sub events
                    nextCalTimeSpan < repeatSpan) // if next timeline span is less than span meant for each calendar event
                {
                    break;
                }

                MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
                if (MyParentEvent.isRigid)
                {
                    MyRepeatCalendarEvent = new RigidCalendarEvent(MyParentEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.getActiveDuration, MyParentEvent.getPreparation, MyParentEvent.getPreDeadline, MyParentEvent.Repeat, MyParentEvent.LocationObj, MyParentEvent.getUIParam, MyParentEvent.Notes, MyParentEvent.isEnabled, MyParentEvent.getIsComplete, MyParentEvent.getCreator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID, MyParentEvent.getNowInfo, MyParentEvent.TimeLineHistory);
                }
                else
                {
                    MyRepeatCalendarEvent = new CalendarEvent(MyParentEvent.getName, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.getActiveDuration, MyParentEvent.getPreparation, MyParentEvent.getPreDeadline, MyParentEvent.NumberOfSplit, MyParentEvent.Repeat, MyParentEvent.LocationObj, MyParentEvent.getUIParam, MyParentEvent.Notes, MyParentEvent.getProcrastinationInfo, MyParentEvent.getNowInfo, MyParentEvent.isEnabled, MyParentEvent.getIsComplete, MyParentEvent.getCreator, MyParentEvent.getAllUsers(), MyParentEvent.getTimeZone, MyEventCalendarID, MyParentEvent.TimeLineHistory);
                }
                MyRepeatCalendarEvent.setDayPreference(ParentEvent.DayPreference);
                MyRepeatCalendarEvent.Location = MyParentEvent.LocationObj;
                foreach (SubCalendarEvent subEvent in MyRepeatCalendarEvent.AllSubEvents)
                {
                    subEvent.RepeatParentEvent = MyParentEvent;
                }
                MyRepeatCalendarEvent.IsRepeatsChildCalEvent = true;
                MyRepeatCalendarEvent.setRepeatParent(MyParentEvent);
            }
            MyParentEvent.deleteAllSubCalendarEventsFromRepeatParentCalendarEvent();
            if (this.RepetitionRangeEnd < timeLine.End)
            {
                this.RepetitionRangeEnd = timeLine.End;
            }

            if (this.RepetitionRangeStart > timeLine.Start)
            {
                this.RepetitionRangeStart = timeLine.Start;
            }
        }

        private void extendTimeline(TimeLine timeLine, CalendarEventRestricted MyParentEvent)
        {
            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();
            DateTimeOffset EachRepeatCalendarStart = timeLine.Start < MyParentEvent.Start ? timeLine.Start : MyParentEvent.Start;
            DateTimeOffset EachRepeatCalendarEnd = timeLine.End;
            TimeLine calendarTimeLine = timeLine.StartToEnd;
            TimeLineRestricted restrictionTimeLine = new TimeLineRestricted(EachRepeatCalendarStart, MyParentEvent.Repeat.Range.End, MyParentEvent.RestrictionProfile, MyParentEvent.Now);
            for (; calendarTimeLine.End < MyParentEvent.Repeat.Range.End;)
            {
                if (restrictionTimeLine.doesTimeLineInterfere(calendarTimeLine))
                {

                    EventID MyEventCalendarID = EventID.GenerateRepeatCalendarEvent(MyParentEvent.getId);
                    CalendarEventRestricted MyRepeatCalendarEvent = CalendarEventRestricted.InstantiateRepeatedCandidate(MyParentEvent.getName, calendarTimeLine.Start, calendarTimeLine.End, MyParentEvent.Calendar_EventID, MyParentEvent.RestrictionProfile, MyParentEvent.getActiveDuration, MyParentEvent.NumberOfSplit, MyParentEvent.LocationObj, MyParentEvent.getUIParam, MyParentEvent.isLocked, MyParentEvent.getPreparation, MyParentEvent.ThirdPartyID, MyParentEvent.Now, MyParentEvent.getCreator, MyParentEvent.Notes, MyParentEvent.getNowInfo, MyParentEvent.Procrastination_EventDB);
                    MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                    _DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.getId, MyRepeatCalendarEvent);
                    MyRepeatCalendarEvent.setDayPreference(ParentEvent.DayPreference);
                    foreach (SubCalendarEvent subEvent in MyRepeatCalendarEvent.AllSubEvents)
                    {
                        subEvent.RepeatParentEvent = MyParentEvent;
                    }
                    MyRepeatCalendarEvent.IsRepeatsChildCalEvent = true;
                    MyRepeatCalendarEvent.setDayPreference(ParentEvent.DayPreference);
                    MyRepeatCalendarEvent.setTimeLineHistory(ParentEvent.TimeLineHistory);
                    MyRepeatCalendarEvent.setRepeatParent(MyParentEvent);
                    MyRepeatCalendarEvent.Location = MyParentEvent.LocationObj;
                }
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, getFrequency);
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, getFrequency);
                calendarTimeLine = new TimeLine(EachRepeatCalendarStart, EachRepeatCalendarEnd);
            }
            MyParentEvent.deleteAllSubCalendarEventsFromRepeatParentCalendarEvent();
            if (this.RepetitionRangeEnd < timeLine.End)
            {
                this.RepetitionRangeEnd = timeLine.End;
            }

            if (this.RepetitionRangeStart > timeLine.Start)
            {
                this.RepetitionRangeStart = timeLine.Start;
            }
        }

        /// <summary>
        /// Function enables the extension of the time line of the repetition object.
        /// NOTE this will not extend extend before the start time of the parent calendar event. 
        /// Also you need to pull in repetition from the DB
        /// </summary>
        /// <param name="updatedTimeLine"></param>
        /// <param name="MyParentEvent"></param>
        public void extendRepetitionTimeline(TimeLine updatedTimeLine, CalendarEvent MyParentEvent, ReferenceNow now)
        {
            if(updatedTimeLine.End > MyParentEvent.End && updatedTimeLine.TimelineSpan > MyParentEvent.StartToEnd.TimelineSpan)
            {
                MyParentEvent.updateTimeLine(updatedTimeLine, now);
                if (_DictionaryOfWeekDayToRepetition != null && _DictionaryOfWeekDayToRepetition.Count > 0)
                {
                    foreach (Repetition repetition in _DictionaryOfWeekDayToRepetition.Values)
                    {
                        CalendarEvent MyRepeatCalendarEvent = repetition._DictionaryOfIDAndCalendarEvents.Values.OrderBy(calEvent => calEvent.End).Last();
                        if (MyRepeatCalendarEvent.getIsEventRestricted && MyParentEvent.getIsEventRestricted)
                        {
                            repetition.extendTimeline(updatedTimeLine, MyParentEvent as CalendarEventRestricted);
                        }
                        else
                        {
                            repetition.extendTimeline(updatedTimeLine, MyParentEvent);
                        }
                    }
                }
                else
                {
                    if (MyParentEvent.getIsEventRestricted)
                    {
                        this.extendTimeline(updatedTimeLine, MyParentEvent as CalendarEventRestricted);
                    }
                    else
                    {
                        CalendarEvent MyRepeatCalendarEvent = _DictionaryOfIDAndCalendarEvents.Values.OrderBy(calEvent => calEvent.End).Last();
                        this.extendTimeline(updatedTimeLine, MyParentEvent);
                    }
                }
            } else
            {
                throw new CustomErrors(CustomErrors.Errors.Recurring_Tile_Extension_Has_To_Be_Greater_Than_Current_Timeline);
            }
            
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
            return _DictionaryOfWeekDayToRepetition != null && _DictionaryOfWeekDayToRepetition.Count > 0;
        }

        public List<Repetition> getDayRepetitions()
        {
            List<Repetition> retValue = _DictionaryOfWeekDayToRepetition.Values.ToList();
            return retValue;
        }

        #region properties

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

            set
            {
                RepetitionWeekDay = value;
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

        public Dictionary<string, Repetition> DayIndexToRepetition
        {
            get
            {
                Dictionary<string, Repetition> retValue = _DictionaryOfWeekDayToRepetition.Collection.ToDictionary(obj => obj.Key, obj => obj.Value);
                return retValue;
            }
        }

        #region undoProperties
        public DateTimeOffset UndoInitializingRangeStart
        {
            get
            {
                return _UndoInitializingRangeStart;
            }
            set
            {
                _UndoInitializingRangeStart = value;
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
        public int UndoRepetitionWeekDay
        {
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
        public string RepetitionFrequency
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
                if (_RepetitionRange == null && _RepetitionRangeEnd != null && _RepetitionRangeStart != null)
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
                return _EnableRepeat;
            }

            set
            {
                _EnableRepeat = value;
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

        [NotMapped]
        virtual public Location Location
        {
            set
            {
                _Location = value;
            }
            get
            {
                return _Location;
            }
        }

        [ForeignKey("LocationId")]
        virtual public Location Location_DB
        {
            set
            {
                _Location = value;
            }
            get
            {
                if (_Location != null)
                {
                    return _Location.isNull ? null : _Location;
                }
                return null;

            }
        }

        public string ParentEventId { get; set; }
        //[ForeignKey("ParentEventId")]
        [NotMapped]
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

        public ICollection<CalendarEvent> RepeatingEvents
        {
            get
            {
                return _DictionaryOfIDAndCalendarEvents ?? (_DictionaryOfIDAndCalendarEvents = new SubEventDictionary<string, CalendarEvent>());
            }
            set
            {
                if (value != null)
                {
                    this._DictionaryOfIDAndCalendarEvents = new SubEventDictionary<string, CalendarEvent>(value);
                }
                else
                {
                    this._DictionaryOfIDAndCalendarEvents = null;
                }
            }
        }

        public ICollection<Repetition> SubRepetitions
        {
            set
            {
                if (value != null)
                {
                    _DictionaryOfWeekDayToRepetition = new SubEventDictionary<int, Repetition>(value);
                }
                else
                {
                    _DictionaryOfWeekDayToRepetition = null;
                }

            }
            get
            {
                return _DictionaryOfWeekDayToRepetition ?? (_DictionaryOfWeekDayToRepetition = new SubEventDictionary<int, Repetition>());
            }
        }
        [NotMapped]
        public virtual bool isPersistable { get; set; } = true;

        public virtual bool FirstInstantiation { get; set; } = true;

        #endregion
        #endregion

        public CalendarEvent[] RecurringCalendarEvents()
        {
            CalendarEvent[] retValue = null;

            if (_DictionaryOfWeekDayToRepetition != null && _DictionaryOfWeekDayToRepetition.Count > 0)
            {
                retValue = _DictionaryOfWeekDayToRepetition.Values.SelectMany(obj => obj.RecurringCalendarEvents()).ToArray();
            }
            else
            {
                retValue = _DictionaryOfIDAndCalendarEvents?.Values.ToArray() ?? new CalendarEvent[0]; ;
            }
            return retValue;
        }

        public Repetition getSubRepeitionByWeekDay(DayOfWeek weekDay)
        {
            int dayOfWeek = (int)weekDay;
            var retValue = _DictionaryOfWeekDayToRepetition[dayOfWeek.ToString()];
            return retValue;
        }

        public Repetition CreateCopy()
        {
            Repetition repetition_cpy = new Repetition();
            if ((this._DictionaryOfIDAndCalendarEvents == null && this._DictionaryOfWeekDayToRepetition == null) || 
                ((this._DictionaryOfWeekDayToRepetition != null && this._DictionaryOfWeekDayToRepetition.Count < 1) &&
                (this._DictionaryOfIDAndCalendarEvents != null && this._DictionaryOfIDAndCalendarEvents.Count < 1))
               )
            {
                return repetition_cpy;
            }
            repetition_cpy._RepetitionFrequency = this._RepetitionFrequency;
            repetition_cpy._RepetitionRange = this._RepetitionRange?.CreateCopy();
            repetition_cpy._Location = _Location?.CreateCopy();
            repetition_cpy._EnableRepeat = _EnableRepeat;
            repetition_cpy.RepetitionWeekDay = RepetitionWeekDay;
            repetition_cpy._DictionaryOfIDAndCalendarEvents = new SubEventDictionary<string, CalendarEvent>(_DictionaryOfIDAndCalendarEvents?.Values.Select(obj => obj.createCopy()));
            repetition_cpy._DictionaryOfWeekDayToRepetition = new SubEventDictionary<int, Repetition>();

            repetition_cpy._DictionaryOfWeekDayToRepetition.Collection = _DictionaryOfWeekDayToRepetition?.Collection.AsParallel().ToDictionary(obj => obj.Key, obj1 => obj1.Value.CreateCopy());

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
            throw new NotImplementedException("Something doesn't smell right about the list generated by the linq stmt below. What to the elements with FirstInstantiation set to true");
            

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
                foreach (KeyValuePair<string, Repetition> kvp in _DictionaryOfWeekDayToRepetition)
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
                foreach (KeyValuePair<string, Repetition> kvp in _DictionaryOfWeekDayToRepetition)
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
