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
        IList<IList<double>> _Score;
        bool _isEvaluated = false;
        public SleepEvaluation(IEnumerable<DayTimeLine> dayTimeLines)
        {
            _DayTimeLines = dayTimeLines.OrderBy(dayTimeLine => dayTimeLine.End).ToImmutableList();   
        }

        public SleepEvaluation(ReferenceNow now, TimeLine calculationTimeline)
        {
            Now = now;
            Tuple<ulong, ulong> dayIndexes = Now.indexRange(calculationTimeline);
            List<DayTimeLine> dayTimeLines = new List<DayTimeLine>();
            ulong iniUniverslaIndex = Now.firstDay.UniversalIndex;
            for (ulong i = dayIndexes.Item1; i <= dayIndexes.Item2; i++ )
            {
                ulong universalIndex = iniUniverslaIndex + (ulong)i;
                DayTimeLine dayTimeLine = Now.getDayTimeLineByDayIndex(universalIndex);
                dayTimeLines.Add(dayTimeLine);
            }
            _DayTimeLines = dayTimeLines.ToImmutableList();
        }

        public void evaluate()
        {
            List<Tuple<TimeLine, ulong>> sleepTimeLines = new List<Tuple<TimeLine, ulong>>();
            List<List<double>> sleepScore = new List<List<double>>();
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
                    double deviationFromEndOfDay = Math.Abs((previousDayTimeLine.End - sleepStart).TotalHours);
                    double sleepSpan = Math.Abs(sleepTImeLine.TimelineSpan.TotalHours);
                    double sleepSpanFeature = sleepSpan < 0.00001 ? 24 : (1 / sleepSpan);
                    List<double> sleepCollection = new List<double>() { deviationFromEndOfDay, sleepSpanFeature };
                    sleepScore.Add(sleepCollection);
                }
            }
            _SleepTimeLines = sleepTimeLines.ToList();
            _Score = sleepScore.Select(obj => ((IList<double>)obj)).ToList();
            _isEvaluated = true;
        }

        /// <summary>
        /// function gets a score for each sleep timespan in the daytimeline
        /// </summary>
        /// <param name="forceEvaluate"></param>
        /// <returns></returns>
        public List<Tuple<TimeLine, ulong>> assessConsequence ()
        {
            if (!_isEvaluated)
            {
                this.evaluate();
            }
            TimeSpan sleepSpan = Utility.SleepSpan;
            List<Tuple<TimeLine, ulong>> undesirableSleepFrames = SleepTimeLines.Where(obj => obj.Item1.TimelineSpan <= sleepSpan).ToList();
            return undesirableSleepFrames;
        }

        public double ScoreTimeLine()
        {
            if (!_isEvaluated)
            {
                this.evaluate();
            }
            var scoreAvg = Utility.multiDimensionCalculationNormalize(_Score);
            double retValue = scoreAvg.Average();
            return retValue;
        }

        public Tuple<double, List<Tuple<TimeLine, ulong>>> scoreAndTimeLine()
        {
            this.evaluate();
            double timeLineScore = this.ScoreTimeLine();
            List<Tuple<TimeLine, ulong>> timeLines = this.SleepTimeLines;
            var retValue = new Tuple<double, List<Tuple<TimeLine, ulong>>>(timeLineScore, timeLines);
            return retValue;
        }

        public List<Tuple<TimeLine, ulong>> SleepTimeLines
        {
            get
            {
                if (!_isEvaluated)
                {
                    this.evaluate();
                }
                return _SleepTimeLines;
            }
        }

        public TimeSpan ExpectedSleepSpan
        {
            get
            {
                if (!_isEvaluated)
                {
                    this.evaluate();
                }
                return Utility.SleepSpan;
            }
        }
    }
}
