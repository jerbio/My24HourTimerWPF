using System;
using System.Collections.Generic;
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
        List<Tuple<int, DaySection, bool, TimeLine>> PreferenceOrder;
        List<Tuple<int, DaySection, bool, TimeLine>> DefaultOrder = new List<Tuple<int, DaySection, bool, TimeLine>>(new[] {
                new Tuple<int, DaySection, bool, TimeLine>(1, DaySection.Sleep , false, new TimeLine()),
                new Tuple<int, DaySection, bool, TimeLine>(2, DaySection.Morning, false, new TimeLine()),
                new Tuple<int, DaySection, bool, TimeLine>(3, DaySection.Afternoon, false, new TimeLine()),
                new Tuple<int, DaySection, bool, TimeLine>(4, DaySection.Evening, false, new TimeLine()),
            new Tuple<int, DaySection, bool, TimeLine>(5, DaySection.None, false, new TimeLine())
            });

        //TilerEvent ControlEvent;
        public TimeOfDayPreferrence(TimeLine timeLine)
        {
            tImeLineStart = timeLine.Start;
            fullDayTImeLine = timeLine.CreateCopy();
            generateTimeFrames(timeLine);
            DefaultOrder = PreferenceOrder.ToList();
        }

        protected void generateTimeFrames(TimeLine timeLine)
        {
            tImeLineStart = timeLine.Start;
            fullDayTImeLine = timeLine.CreateCopy();
            TimeSpan spanPerSection = TimeSpan.FromTicks(timeLine.TimelineSpan.Ticks/4);
            PreferenceOrder = new List<Tuple<int, DaySection, bool, TimeLine>>(new[] {
                new Tuple<int, DaySection, bool, TimeLine>(1, DaySection.Sleep , false, new TimeLine(timeLine.Start, timeLine.Start.Add(spanPerSection).AddTicks(-1))),
                new Tuple<int, DaySection, bool, TimeLine>(2, DaySection.Morning, false, new TimeLine(timeLine.Start.Add(spanPerSection), timeLine.Start.AddTicks(2*spanPerSection.Ticks).AddTicks(-1))),
                new Tuple<int, DaySection, bool, TimeLine>(3, DaySection.Afternoon,  false, new TimeLine(timeLine.Start.AddTicks(2*spanPerSection.Ticks), timeLine.Start.AddTicks(3*spanPerSection.Ticks).AddTicks(-1))),
                new Tuple<int, DaySection, bool, TimeLine>(4, DaySection.Evening,false, new TimeLine(timeLine.Start.AddTicks(3*spanPerSection.Ticks), timeLine.Start.AddTicks(4*spanPerSection.Ticks).AddTicks(-1))),
                new Tuple<int, DaySection, bool, TimeLine>(5, DaySection.None, false, new TimeLine(timeLine.Start, timeLine.Start.AddDays(1).AddTicks(-1))),
            });
        }

        static public Dictionary<DaySection, TimeLine> splitIntoDaySections(TimeLine timeLine)
        {
            TimeSpan spanPerSection = TimeSpan.FromTicks(timeLine.TimelineSpan.Ticks / 4);
            List<Tuple<DaySection, TimeLine>>  tempHolder = new List<Tuple<DaySection, TimeLine>>(new[] {
                new Tuple<DaySection, TimeLine>(DaySection.Sleep , new TimeLine(timeLine.Start, timeLine.Start.Add(spanPerSection).AddTicks(-1))),
                new Tuple<DaySection, TimeLine>(DaySection.Morning, new TimeLine(timeLine.Start.Add(spanPerSection), timeLine.Start.AddTicks(2*spanPerSection.Ticks).AddTicks(-1))),
                new Tuple<DaySection, TimeLine>(DaySection.Afternoon, new TimeLine(timeLine.Start.AddTicks(2*spanPerSection.Ticks), timeLine.Start.AddTicks(3*spanPerSection.Ticks).AddTicks(-1))),
                new Tuple<DaySection, TimeLine>(DaySection.Evening , new TimeLine(timeLine.Start.AddTicks(3*spanPerSection.Ticks), timeLine.Start.AddTicks(4*spanPerSection.Ticks).AddTicks(-1))),
                new Tuple<DaySection, TimeLine>(DaySection.None, new TimeLine(timeLine.Start, timeLine.Start.AddDays(1).AddTicks(-1))),
            });
            Dictionary<DaySection, TimeLine> retValue = tempHolder.ToDictionary(obj => obj.Item1, obj => obj.Item2);
            return retValue;
        }
        internal void InitializeGrouping(TilerEvent ControlEvent)
        {
            if (ControlEvent.isLocked)
            {
                List<Tuple<int, DaySection, bool, TimeLine>> preferenceOrderCopy = DefaultOrder.ToList();
                preferenceOrderCopy.RemoveAll(preferenceOrder => preferenceOrder.Item2 == DaySection.None);
                List<TimeLine> timeLines = preferenceOrderCopy.Select(timeLine => timeLine.Item4.InterferringTimeLine(ControlEvent.RangeTimeLine)).ToList();
                PreferenceOrder = new List<Tuple<int, DaySection, bool, TimeLine>>();
                for (int i = 0; i < timeLines.Count; i++)
                {
                    TimeLine timeLine = timeLines[i];
                    if (timeLine != null)
                    {
                        PreferenceOrder.Add(new Tuple<int, DaySection, bool, TimeLine>(i, preferenceOrderCopy[i].Item2, preferenceOrderCopy[i].Item3, preferenceOrderCopy[i].Item4.CreateCopy()));
                    }
                }
            }
            return;
        }
        public DaySection getCurrentDayPreference()
        {
            if (PreferenceOrder.Count > 0)
            {
                return PreferenceOrder.First().Item2;
            }
            else
            {
                return DaySection.Disabled;
            }
        }

        public List<DaySection> getPreferenceOrder()
        {
            List<DaySection> retValue;
            if (PreferenceOrder.Count > 0)
            {
                retValue = PreferenceOrder.Select(prefOrder => prefOrder.Item2).ToList();
            }
            else
            {
                retValue = new List<DaySection>() { DaySection.Disabled };
            }
            return retValue;
        }

        public void setPreferenceOrder(IList<DaySection> orderedPreferences)
        {
            Dictionary<DaySection, Tuple<int, DaySection, bool, TimeLine>> dictOfPrefOrder = PreferenceOrder.ToDictionary(prefOder => prefOder.Item2, prefOder => prefOder);
            List<Tuple<int, DaySection, bool, TimeLine>> newPreferenceOrder = orderedPreferences.Select(daySection => dictOfPrefOrder[daySection]).ToList();
            PreferenceOrder = newPreferenceOrder;
        }

        public void setCurrentdayPreference(DaySection preferredSection)
        {
            Tuple<int, DaySection, bool, TimeLine> preferredOption = PreferenceOrder.SingleOrDefault(preferenee => preferenee.Item2 == preferredSection);
            PreferenceOrder.Remove(preferredOption);
            PreferenceOrder.Insert(0, preferredOption);
        }

        public void assignSectorBasedOnTIme(DateTimeOffset time, TimeLine daytimeLine)
        {
            PreferenceOrder = DefaultOrder.ToList();
            daytimeLine = daytimeLine ?? fullDayTImeLine;
            if (daytimeLine.IsDateTimeWithin(time))
            {
                if (fullDayTImeLine != daytimeLine)
                {
                    generateTimeFrames(daytimeLine);
                }
                Tuple<int, DaySection, bool, TimeLine> daySection = PreferenceOrder.Where(obj => obj.Item2 != DaySection.None).Where(obj => obj.Item4.IsDateTimeWithin(time)).First();
                if (daySection != null)
                {
                    setCurrentdayPreference(daySection.Item2);
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
            if (PreferenceOrder.Count > 0)
            {
                PreferenceOrder.RemoveAt(0);
            }
        }
        public void rejectCurrentPreference(DaySection daySection)
        {
            PreferenceOrder.RemoveAll(daySector => daySector.Item2 == daySection);
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
    }
}
