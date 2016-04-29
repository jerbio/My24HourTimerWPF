using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class Reason
    {
        public enum Options{ None,PreservedOrder,BestFit, HumidWeather, ColdWeather, WarmWeather, Weather, CompletionRate, CompletionLevel, DeletionRate, DeletionLevel, Occupancy, DeadlineApproaching, HighExhaustion, LowExhaustion, Exhaustion,  CloseToCluster, Far, ReduceTransitTime, IncreaseTransitTime, SimilarActivity,WillConflict, AvoidConflict, ProcrastinationIncrease, ProcrastinationDecrease }
        Options Option = Options.None;

        public Reason (Options option = Options.None)
        {
            this.Option = option;
        }

        public void updateOption(Options Option)
        {
            this.Option = Option;
        }

        public Options getOption()
        {
            return this.Option;
        }
    }
}
