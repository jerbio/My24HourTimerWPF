using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TilerElements;



namespace TilerCore
{
    public class DayTempEvaluation
    {
        public long Left { get; set; } = 0;
        public long Right { get; set; } = 0;
        public long Diff { get; set; } = 0;
        public long DayIndex { get; set; } = 0;
        public double Score { get; set; } = double.NaN;
        protected double idealSubEventPerDay; // The ideal daily count
        public double AssignedSubEventsInDay { get; set; } = 0;
        public double ElectedSubEventsInDayCount { get; set; } = 0;
        public double InitialTimeLineScore{ get; set; } = double.NaN;
        public double TimeLineScore { get; set; } = double.NaN;

        public DayTempEvaluation(double subEventPerDay)
        {
            this.idealSubEventPerDay = subEventPerDay;
        }

        public double [] toMultiArrayDict()
        {
            double subEventsPerDay_denominator = idealSubEventPerDay == 0 ? 1 : idealSubEventPerDay;
            double day_selection_or_election_score = (AssignedSubEventsInDay + ElectedSubEventsInDayCount) / subEventsPerDay_denominator;// this holds the score for when the day selection's being already assigned or elected compared to the ideal ratio.
            return new double[] {
                AssignedSubEventsInDay,
                AssignedSubEventsInDay,
                DayIndex,
                TimeLineScore,
                day_selection_or_election_score
            };
        }

        /// <summary>
        /// This needs to be called when the current day has been elected as a possible day to be filled
        /// </summary>
        public void incrementDayElectionCount()
        {
            ElectedSubEventsInDayCount += 1;
        }
    }
}
