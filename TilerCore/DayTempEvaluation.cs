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
        public double DayTimeLineTier { get; set; } = double.NaN;
        /// <summary>
        /// This holds the score of other tiles having an effect on the day of the tile. 
        /// So think if the dayIndex of this DayTempEvaluation is 125.  If there is a Tile assigned to dayIndex 122 and another tile assigned to 145. The tile on dayIndex 122 will create a higher heat score against This Day as opposed to day index 145.
        /// </summary>
        public double HeatScore { get; set; } = 0;

        public DayTempEvaluation(double subEventPerDay)
        {
            this.idealSubEventPerDay = subEventPerDay;
        }

        public double [] toMultiArrayDict()
        {
            double subEventsPerDay_denominator = idealSubEventPerDay == 0 ? 1 : idealSubEventPerDay;
            double day_selection_or_election_score = (AssignedSubEventsInDay + ElectedSubEventsInDayCount) / subEventsPerDay_denominator;// this holds the score for when the day selection's being already assigned or elected compared to the ideal ratio.
            double totalHeatScore = AssignedSubEventsInDay + HeatScore;
            return new double[] {
                AssignedSubEventsInDay,
                DayIndex,
                (DayTimeLineTier),
                //(DayTimeLineTier),
                totalHeatScore,
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

        
        public void incrementHeatScore(double deltaScore)
        {
            HeatScore += deltaScore;
        }
    }
}
