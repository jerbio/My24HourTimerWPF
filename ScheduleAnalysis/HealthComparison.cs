using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;

namespace ScheduleAnalysis
{
    public class HealthComparison
    {
        Health FirstHealth;
        Health SecondHealth;
        HealthEvaluation FirsstEvaluation;
        HealthEvaluation SecondEvaluation;

        public HealthComparison(Health firstHealth, Health secondHealth)
        {
            FirstHealth = firstHealth;
            SecondHealth = secondHealth;
            FirsstEvaluation = FirstHealth.getEvaluation();
            FirsstEvaluation.TravelTimeAnalysis.evaluate().Wait();
            SecondEvaluation = SecondHealth.getEvaluation();
            SecondEvaluation.TravelTimeAnalysis.evaluate().Wait();
        }

        /// <summary>
        /// This function compares two Health objects based on the various scores of a schedule. Note this does not use Health.getScore function. 
        /// This compares the different scores of both health and tries to evaluate the better of the two.
        /// It returns a 
        /// -1 if firstHealth is better
        /// 0 if both are equal
        /// 1 if secondHealth is better
        /// This function is to be used with a comparator function
        /// </summary>
        public int Compare
        {
            get
            {
                double retValue = 0;

                List<double> FirstParams = new List<double>() { FirstHealth.evaluateConflicts().Item1, FirstHealth.TotalDistance, FirstHealth.SleepEvaluation.ScoreTimeLine(), FirstHealth.evaluatePositioning() };
                List<double> SecondParams = new List<double>() { SecondHealth.evaluateConflicts().Item1, SecondHealth.TotalDistance, SecondHealth.SleepEvaluation.ScoreTimeLine(), SecondHealth.evaluatePositioning() };
                IList<IList<double>> multiParams = new List<IList<double>>() { FirstParams, SecondParams };
                var multiVarCalResult = Utility.multiDimensionCalculationNormalize(multiParams);
                

                retValue = multiVarCalResult[0] - multiVarCalResult[1];

                if (retValue > 0)
                {
                    retValue = 1;
                }
                else if (retValue < 0)
                {
                    retValue = -1;
                }

                return (int)retValue;
            }
        }
        
    }
}
