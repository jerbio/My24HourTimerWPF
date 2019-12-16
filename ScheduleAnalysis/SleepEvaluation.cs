using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using TilerElements;
using System.Collections.Immutable;
using Newtonsoft.Json.Linq;

namespace ScheduleAnalysis
{
    public class SleepEvaluation:IJson
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
                    double deviationFromEndOfDayFeature = previousDayTimeLine.End > sleepStart ? 0 : Math.Abs((previousDayTimeLine.End - sleepStart).TotalHours); // if end of day is after the sleep then simply set to zero since its optimized
                    double sleepSpan = Math.Abs(sleepTImeLine.TimelineSpan.TotalHours);
                    double sleepSpanFeature = sleepSpan < 0.00001 ? 24 : (1 / (2*sleepSpan));// double(half) sleepSpanFeature to weight the span of the sleep span
                    List<double> sleepCollection = new List<double>() { deviationFromEndOfDayFeature, sleepSpanFeature };
                    sleepScore.Add(sleepCollection);
                }
            }
            _SleepTimeLines = sleepTimeLines.ToList();
            _Score = sleepScore.Select(obj => ((IList<double>)obj)).ToList();
            _isEvaluated = true;
        }

        /// <summary>
        /// function gets the undesirable sleep timelines from evaluation
        /// </summary>
        /// <param name="forceEvaluate"></param>
        /// <returns></returns>
        public List<Tuple<TimeLine, ulong>> undesirableSleepTimelines ()
        {
            if (!_isEvaluated)
            {
                this.evaluate();
            }
            TimeSpan sleepSpan = Utility.SleepSpan;
            List<Tuple<TimeLine, ulong>> undesirableSleepFrames = MaxSleepTimeLines.Where(obj => obj.Item1.TimelineSpan < sleepSpan).ToList();
            return undesirableSleepFrames;
        }

        public double ScoreTimeLine()
        {
            if (!_isEvaluated)
            {
                this.evaluate();
            }
            var scoreAvg = Utility.multiDimensionCalculation(_Score);
            double retValue = scoreAvg.Average();
            return retValue;
        }

        public Tuple<double, List<Tuple<TimeLine, TimeLine, ulong>>> scoreAndTimeLine()
        {
            this.evaluate();
            double timeLineScore = this.ScoreTimeLine();
            List<Tuple<TimeLine, TimeLine, ulong>> timeLines = this.SleepTimeLines;
            var retValue = new Tuple<double, List<Tuple<TimeLine, TimeLine, ulong>>>(timeLineScore, timeLines);
            return retValue;
        }

        public JObject ToJson()
        {
            JObject retValue = new JObject();
            JObject sleepTimeLinesJson = new JObject();
            var sleepTimeLines = SleepTimeLines.ToList();
            double score = this.ScoreTimeLine();
            for (int i = 0; i < sleepTimeLines.Count; i++)
            {
                var sleepTimeLine = sleepTimeLines[i];
                DateTimeOffset currentDay = Now.getClientBeginningOfDay(sleepTimeLine.Item3);
                JObject sleepJson = new JObject();
                sleepJson.Add("SleepTimeline", sleepTimeLine.Item1.ToJson());
                sleepJson.Add("MaximumSleepTimeLine", sleepTimeLine.Item2.ToJson());

                sleepTimeLinesJson.Add(currentDay.ToUnixTimeMilliseconds().ToString(), sleepJson);
            }

            JObject undesirableSleepTimeLines = new JObject();
            List<Tuple<TimeLine, ulong>> undesirableSleepTimelines = this.undesirableSleepTimelines();
            for (int i = 0; i < undesirableSleepTimelines.Count; i++)
            {
                Tuple<TimeLine, ulong> sleepTimeLine = undesirableSleepTimelines[i];
                DateTimeOffset currentDay = Now.getClientBeginningOfDay(sleepTimeLine.Item2);
                TimeSpan lostTimeSpan = ExpectedSleepSpan - sleepTimeLine.Item1.TimelineSpan;
                JObject undesiredDetails = new JObject();
                undesiredDetails.Add("LostSleep", lostTimeSpan.TotalMilliseconds);
                undesiredDetails.Add("SleepTimeline", sleepTimeLine.Item1.ToJson());
                undesirableSleepTimeLines.Add(currentDay.ToUnixTimeMilliseconds().ToString(), undesiredDetails);
            }


            retValue.Add("Score", score);
            retValue.Add("SleepTimeLines", sleepTimeLinesJson);
            retValue.Add("UndesiredTimeLines", undesirableSleepTimeLines);
            return retValue;
        }

        public List<Tuple<TimeLine, ulong>> MaxSleepTimeLines
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

        /// <summary>
        /// Returns the sleep timeline,
        /// Item1 is the timeframew within the expected timeline
        /// Item2 is the maximum sleep available
        /// Item3 is the day index
        /// </summary>
        public List<Tuple<TimeLine, TimeLine, ulong>> SleepTimeLines
        {
            get
            {
                var retValue = new List<Tuple<TimeLine, TimeLine, ulong>>();
                TimeSpan expectedSleepSpan = ExpectedSleepSpan;
                if (!_isEvaluated)
                {
                    this.evaluate();
                }

                for(int i=0; i<_SleepTimeLines.Count; i++)
                {
                    var sleepTImeLine = _SleepTimeLines[i];
                    Tuple<TimeLine, TimeLine, ulong> tupleInput;
                    if (sleepTImeLine.Item1.TimelineSpan > expectedSleepSpan)
                    {
                        tupleInput = new Tuple<TimeLine, TimeLine, ulong>(new TimeLine(sleepTImeLine.Item1.Start, sleepTImeLine.Item1.Start.Add(expectedSleepSpan)), sleepTImeLine.Item1.StartToEnd, sleepTImeLine.Item2);
                    }
                    else
                    {
                        tupleInput = new Tuple<TimeLine, TimeLine, ulong>(sleepTImeLine.Item1.StartToEnd, sleepTImeLine.Item1.StartToEnd, sleepTImeLine.Item2); ;
                    }
                    retValue.Add(tupleInput);
                }

                return retValue;
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
