using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// This function is to be used with a comparator function
        /// </summary>
        public int Compare
        {
            get
            {
                double retValue = 0;
                double DistanceTravelTimeSpanDifferenceCriteria = 0;
                double DistanceTravelDifferenceCriteria = 0;
                double SleepDifferenceCriteria = 0;

                TravelTime firstTravelTime = FirsstEvaluation.TravelTimeAnalysis;
                firstTravelTime.evaluate().Wait();
                TimeSpan firstTravelTImeSpanDiff = TimeSpan.FromTicks(firstTravelTime.result().Sum(kvp => kvp.Value.Ticks));

                TravelTime secondTravelTime = SecondEvaluation.TravelTimeAnalysis;
                secondTravelTime.evaluate().Wait();
                TimeSpan secondTravelTimeSpanDiff = TimeSpan.FromTicks(secondTravelTime.result().Sum(kvp => kvp.Value.Ticks));

                if (firstTravelTImeSpanDiff > secondTravelTimeSpanDiff)
                {
                    DistanceTravelTimeSpanDifferenceCriteria = -1;
                }
                else if (firstTravelTImeSpanDiff < secondTravelTimeSpanDiff)
                {
                    DistanceTravelTimeSpanDifferenceCriteria = 1;
                }

                if (FirsstEvaluation.TotalDistance > SecondEvaluation.TotalDistance)
                {
                    DistanceTravelDifferenceCriteria = 1;
                }
                else if (FirsstEvaluation.TotalDistance > SecondEvaluation.TotalDistance)
                {
                    DistanceTravelDifferenceCriteria = -1;
                }


                TimeSpan firstSleep = TimeSpan.FromTicks(FirsstEvaluation.SleepSchedule.Sum(sleepSpans => sleepSpans.Ticks));
                TimeSpan secondSleep = TimeSpan.FromTicks(SecondEvaluation.SleepSchedule.Sum(sleepSpans => sleepSpans.Ticks));

                if (firstSleep > secondSleep)
                {
                    SleepDifferenceCriteria = -1;
                }
                else if (firstSleep < secondSleep)
                {
                    SleepDifferenceCriteria = 1;
                }




                retValue = DistanceTravelTimeSpanDifferenceCriteria + DistanceTravelDifferenceCriteria + SleepDifferenceCriteria;

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
