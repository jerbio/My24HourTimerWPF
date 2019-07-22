using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TilerElements;



namespace TilerCore
{
    public class DayTempEvaluation
    {
        public ulong Left { get; set; } = 0;
        public ulong Right { get; set; } = 0;
        public ulong Diff { get; set; } = 0;
        public ulong DayIndex { get; set; } = 0;
        public double Score { get; set; } = double.NaN;
        public double TimeLineScore { get; set; } = double.NaN;

        public double [] toMultiArrayDict()
        {
            return new double[] { Diff, DayIndex, TimeLineScore
                , TimeLineScore };// artificially giving the score more importance
        }
    }
}
