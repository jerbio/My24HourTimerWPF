using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class TimeOfDayPreferrence
    {
        public enum DaySection { Sleep, Morning, Afternoon, Evening, None, Disabled }
        protected DateTimeOffset tImeLineStart;
        protected TimeLine fullDayTImeLine;
        public static ImmutableList<DaySection> ActiveDaySections = new List<DaySection>() { DaySection.Sleep, DaySection.Morning, DaySection.Afternoon, DaySection.Evening }.ToImmutableList();
        public static ImmutableDictionary<DaySection, int> DaysectionToIndexDictionary = ActiveDaySections.Select((ret, i) => new Tuple<DaySection, int>(ret, i)).ToImmutableDictionary(obj => obj.Item1, obj => obj.Item2);
        List <Tuple<int, DaySection, bool, TimeLine>> _PreferenceOrder;
        List<Tuple<int, DaySection, bool, TimeLine>> _DefaultOrder = new List<Tuple<int, DaySection, bool, TimeLine>>(new[] {
                new Tuple<int, DaySection, bool, TimeLine>(1, DaySection.Sleep , false, new TimeLine()),
                new Tuple<int, DaySection, bool, TimeLine>(2, DaySection.Morning, false, new TimeLine()),
                new Tuple<int, DaySection, bool, TimeLine>(3, DaySection.Afternoon, false, new TimeLine()),
                new Tuple<int, DaySection, bool, TimeLine>(4, DaySection.Evening, false, new TimeLine()),
            new Tuple<int, DaySection, bool, TimeLine>(5, DaySection.None, false, new TimeLine())
            });
        IEnumerable<Tuple<int, DaySection, bool, TimeLine>> ActiveHours;
        bool _isDefaultOrdering = false;
        //TilerEvent ControlEvent;
        public TimeOfDayPreferrence(TimeLine timeLine, List<DaySection> preferredOrder = null,  DaySection userActivePreference = DaySection.Morning)
        {
            tImeLineStart = timeLine.Start;
            fullDayTImeLine = timeLine.StartToEnd;
            _PreferenceOrder = generateTimeFrames(timeLine, preferredOrder);
            _isDefaultOrdering = preferredOrder == null;
            _DefaultOrder = _PreferenceOrder.ToList();
            ActiveHours = _PreferenceOrder.Where(obj => obj.Item2 != DaySection.None && obj.Item2 != DaySection.Disabled).ToList();
        }

        protected List<Tuple<int, DaySection, bool, TimeLine>> generateTimeFrames(TimeLine timeLine, List<DaySection> preferredOrder = null)
        {
            tImeLineStart = timeLine.Start;
            List<DaySection> tempDaySectionOrder = preferredOrder ?? new List<DaySection>() { DaySection.Morning, DaySection.Afternoon, DaySection.Evening, DaySection.Sleep };
            List<Tuple<int, DaySection, bool, TimeLine>> retValue = new List<Tuple<int, DaySection, bool, TimeLine>>();
            var daySectionsToTimeline = splitIntoDaySections(timeLine);

            int indexCounter = -1;
            for (int i = 0; i < tempDaySectionOrder.Count; i++)
            {
                DaySection daySection = tempDaySectionOrder[i];
                if (daySectionsToTimeline.ContainsKey(daySection) && daySection!= DaySection.None   )
                {
                    ++indexCounter;
                    var subTimeLine = daySectionsToTimeline[daySection];
                    var preference = new Tuple<int, DaySection, bool, TimeLine>(i + 1, daySection, false, subTimeLine);
                    retValue.Add(preference);
                }
            }

            return retValue;
        }

        /// <summary>
        /// Giving a timeline this function will return the timeline for each day section. NOTE this gets the timeline based on the end of the timeline. So this means the first six hour timeline before the end is evening, and the next is afer morning then evening.
        /// So if you have a timeline of 10:00AM - 10:00PM this will result in Dictionary of only two entries
        /// 4:00PM - 10:00PM will be evening
        /// 10:AM - 4:00PM will be afternoon.
        /// There will be no morning or sleep day sections.
        /// </summary>
        /// <param name="timeLine"></param>
        /// <returns></returns>

        static public Dictionary<DaySection, TimeLine> splitIntoDaySections(TimeLine timeLine)
        {
            if(timeLine.TimelineSpan<=Utility.OneDayTimeSpan)
            {
                TimeLine refreshedTimeLine = timeLine.StartToEnd;
                Dictionary<DaySection, TimeLine> retValue = new Dictionary<DaySection, TimeLine>();
                TimeSpan spanPerSection = TimeSpan.FromTicks(Utility.OneDayTimeSpan.Ticks / 4);
                TimeSpan spanLeft = refreshedTimeLine.TimelineSpan;
                List<DaySection> daySections = ActiveDaySections.ToList();
                int i = daySections.Count - 1;
                for (; i >= 0 && refreshedTimeLine.TimelineSpan > spanPerSection; --i)
                {
                    DateTimeOffset start = refreshedTimeLine.End - spanPerSection;
                    DaySection daySection = daySections[i];
                    retValue.Add(daySection, new TimeLine(start, refreshedTimeLine.End));
                    refreshedTimeLine = new TimeLine(refreshedTimeLine.Start, refreshedTimeLine.End.Subtract(spanPerSection));
                }
                if(refreshedTimeLine.TimelineSpan.Ticks > 0)
                {
                    if(i>=0)
                    {
                        DaySection daySection = daySections[i];
                        retValue.Add(daySection, refreshedTimeLine);
                    } else
                    {
                        throw new Exception("THis is weird this is timeline more than 24 hours or there is something wrong with your for loop logic");
                    }
                }
                return retValue;
            }

            throw new Exception("Time line cannot be more than twentyfour hours");
        }

        internal void InitializeGrouping(TilerEvent ControlEvent)
        {
            if (ControlEvent.isLocked)
            {
                List<Tuple<int, DaySection, bool, TimeLine>> preferenceOrderCopy = _DefaultOrder.ToList();
                preferenceOrderCopy.RemoveAll(preferenceOrder => preferenceOrder.Item2 == DaySection.None);
                List<TimeLine> timeLines = preferenceOrderCopy.Select(timeLine => timeLine.Item4.InterferringTimeLine(ControlEvent.StartToEnd)).ToList();
                _PreferenceOrder = new List<Tuple<int, DaySection, bool, TimeLine>>();
                for (int i = 0; i < timeLines.Count; i++)
                {
                    TimeLine timeLine = timeLines[i];
                    if (timeLine != null)
                    {
                        _PreferenceOrder.Add(new Tuple<int, DaySection, bool, TimeLine>(i, preferenceOrderCopy[i].Item2, preferenceOrderCopy[i].Item3, preferenceOrderCopy[i].Item4.StartToEnd));
                    }
                }
            }
            return;
        }
        public DaySection getCurrentDayPreference()
        {
            if (_PreferenceOrder.Count > 0)
            {
                return _PreferenceOrder.First().Item2;
            }
            else
            {
                return DaySection.Disabled;
            }
        }

        public List<DaySection> getPreferenceOrder()
        {
            List<DaySection> retValue;
            if (_PreferenceOrder.Count > 0)
            {
                retValue = _PreferenceOrder.Select(prefOrder => prefOrder.Item2).ToList();
            }
            else
            {
                retValue = new List<DaySection>() { DaySection.Disabled };
            }
            return retValue;
        }

        public void setPreferenceOrder(IList<DaySection> orderedPreferences)
        {
            Dictionary<DaySection, Tuple<int, DaySection, bool, TimeLine>> dictOfPrefOrder = _PreferenceOrder.ToDictionary(prefOder => prefOder.Item2, prefOder => prefOder);
            List<Tuple<int, DaySection, bool, TimeLine>> newPreferenceOrder = orderedPreferences.Select(daySection => dictOfPrefOrder[daySection]).ToList();
            _PreferenceOrder = newPreferenceOrder;
        }

        public void setCurrentdayPreference(DaySection preferredSection)
        {
            Tuple<int, DaySection, bool, TimeLine> preferredOption = _PreferenceOrder.SingleOrDefault(preferenee => preferenee.Item2 == preferredSection);
            _PreferenceOrder.Remove(preferredOption);
            _PreferenceOrder.Insert(0, preferredOption);
        }

        public void assignSectorBasedOnTIme(DateTimeOffset time, TimeLine daytimeLine)
        {
            var daySector = getDaySectorByTime(time, daytimeLine);
            setCurrentdayPreference(daySector);
        }

        public DaySection getDaySectorByTime(DateTimeOffset time, TimeLine daytimeLine)
        {
            _PreferenceOrder = _DefaultOrder.ToList();
            daytimeLine = daytimeLine ?? fullDayTImeLine;
            if (daytimeLine.IsDateTimeWithin(time))
            {
                if (fullDayTImeLine != daytimeLine)
                {
                    _PreferenceOrder = generateTimeFrames(daytimeLine);
                }
                Tuple<int, DaySection, bool, TimeLine> daySection = _PreferenceOrder.Where(obj => obj.Item2 != DaySection.None).Where(obj => obj.Item4.IsDateTimeWithin(time)).First();
                if (daySection != null)
                {
                    return daySection.Item2;
                }
                else
                {
                    throw new Exception("Cannot add a day preference not within one of the precalculated day preferences");
                }
            }
            else
            {
                throw new Exception("Cannot work with a datetimeoffset that is outside the deay preference for this object");
            }
        }

        public void rejectCurrentPreference()
        {
            if (_PreferenceOrder.Count > 0)
            {
                _PreferenceOrder.RemoveAt(0);
            }
        }
        public void rejectCurrentPreference(DaySection daySection)
        {
            _PreferenceOrder.RemoveAll(daySector => daySector.Item2 == daySection);
        }
        public class SingleTimeOfDayPreference
        {
            TimeOfDayPreferrence.DaySection _Section;
            TimelineWithSubcalendarEvents _Timeline;
            public SingleTimeOfDayPreference(TimeOfDayPreferrence.DaySection section, TimelineWithSubcalendarEvents timeLine)
            {
                _Section = section;
                _Timeline = timeLine;
            }

            public TimelineWithSubcalendarEvents Timeline
            {
                get
                {
                    return _Timeline;
                }
            }

            public TimeOfDayPreferrence.DaySection DaySection
            {
                get
                {
                    return _Section;
                }
            }
        }

        public bool isDefaultOrdering
        {
            get
            {
                return _isDefaultOrdering;
            }
        }

        //public static List<DaySection> ActiveDaySections
        //{
        //    get
        //    {
        //        var values = Enum.GetValues(typeof(DaySection)).Cast<DaySection>();
        //        return values.Where(daysection => daysection != DaySection.None && daysection != DaySection.Disabled).ToList();
        //    }
            
        //}
    }
}
