using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using TilerElements;
using System.Collections.Immutable;

namespace ScheduleAnalysis
{
    public class SleepEvaluation
    {
        ImmutableList<DayTimeLine> _DayTimeLines;
        ReferenceNow Now;
        List<Tuple<TimeLine, ulong>> _SleepTimeLines;
        public SleepEvaluation(IEnumerable<DayTimeLine> dayTimeLines)
        {
            _DayTimeLines = dayTimeLines.OrderBy(dayTimeLine => dayTimeLine.End).ToImmutableList();   
        }

        public SleepEvaluation(ReferenceNow now, TimeLine calculationTimeline)
        {
            Now = now;
            Tuple<ulong, ulong> dayIndexes = Now.indexRange(calculationTimeline);
            List<DayTimeLine> dayTimeLines = new List<DayTimeLine>();
            for (ulong i = dayIndexes.Item1; i <= dayIndexes.Item2; i++ )
            {
                DayTimeLine dayTimeLine = Now.getDayTimeLineByDayIndex((ulong)i);
                dayTimeLines.Add(dayTimeLine);
            }
            _DayTimeLines = dayTimeLines.ToImmutableList();
        }

        public void evaluate()
        {
            List<Tuple<TimeLine, ulong>> sleepTimeLines = new List<Tuple<TimeLine, ulong>>();
            ulong iniUniverslaIndex = Now.firstDay.UniversalIndex;
            for (int i = 0; i < _DayTimeLines.Count; i++)
            {

                ulong universalIndex = iniUniverslaIndex + (ulong)i;
                DayTimeLine dayTimeLine = _DayTimeLines[i];
                if (universalIndex > iniUniverslaIndex)
                {
                    ulong previousDayUniversalIndex = universalIndex - 1;
                    DayTimeLine previousDayTimeLine = Now.getDayTimeLineByDayIndex(previousDayUniversalIndex);
                    DateTimeOffset sleepStart = dayTimeLine.PrecedingDaySleepSubEvent?.End ?? previousDayTimeLine.SleepSubEvent?.End ?? previousDayTimeLine.End;//
                    DateTimeOffset sleepEnd = dayTimeLine.WakeSubEvent?.Start ?? sleepStart.Add(Now.SleepSpan);
                    TimeLine sleepTImeLine = new TimeLine(sleepStart, sleepEnd);
                    sleepTimeLines.Add(new Tuple<TimeLine, ulong>(sleepTImeLine, dayTimeLine.UniversalIndex));
                }

            }
            _SleepTimeLines = sleepTimeLines.ToList();
        }

        public List<Tuple<TimeLine, ulong>> assessConsequence (bool forceEvaluate = true)
        {
            if(forceEvaluate)
            {
                this.evaluate();
            }
            TimeSpan sleepSpan = Utility.SleepSpan;
            List<Tuple<TimeLine, ulong>> undesirableSleepFrames = SleepTimeLines.Where(obj => obj.Item1.TimelineSpan <= sleepSpan).ToList();
            return undesirableSleepFrames;
        }

        public List<Tuple<TimeLine, ulong>> SleepTimeLines
        {
            get
            {
                return _SleepTimeLines;
            }
        }
    }
}
