using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public enum DataRetrivalOption {Name, DataBlob, Now, Evaluation, EvaluationPerformance, Ui, All, None, Prediction, Repetition, Pause, SubEvent, Location, TimeLineHistory};
    public static class DataRetrievalSet
    {
        public readonly static HashSet<DataRetrivalOption> scheduleManipulationPerformance = new HashSet<DataRetrivalOption>() {
            DataRetrivalOption.SubEvent,
            DataRetrivalOption.EvaluationPerformance,
            DataRetrivalOption.Now,
            DataRetrivalOption.Pause,
            DataRetrivalOption.Location
        };

        public readonly static HashSet<DataRetrivalOption> scheduleManipulation = new HashSet<DataRetrivalOption>() {
            DataRetrivalOption.SubEvent,
            DataRetrivalOption.Evaluation, 
            DataRetrivalOption.Now, 
            DataRetrivalOption.Pause,
            DataRetrivalOption.Location
        };

        public readonly static HashSet<DataRetrivalOption> scheduleManipulationWithRepeat = new HashSet<DataRetrivalOption>() {
            DataRetrivalOption.SubEvent,
            DataRetrivalOption.Evaluation,
            DataRetrivalOption.Now,
            DataRetrivalOption.Pause,
            DataRetrivalOption.Location,
            DataRetrivalOption.Repetition
        };
        public readonly static HashSet<DataRetrivalOption> scheduleManipulationWithUpdateHistory = new HashSet<DataRetrivalOption>() {
            DataRetrivalOption.SubEvent,
            DataRetrivalOption.Evaluation,
            DataRetrivalOption.Now,
            DataRetrivalOption.Pause,
            DataRetrivalOption.TimeLineHistory,
            DataRetrivalOption.Location
        };
        public readonly static HashSet<DataRetrivalOption> analysisManipulation = new HashSet<DataRetrivalOption>() {
            DataRetrivalOption.SubEvent,
            DataRetrivalOption.Evaluation,
            DataRetrivalOption.TimeLineHistory,
            DataRetrivalOption.Location,
            DataRetrivalOption.Now
        };
        public readonly static HashSet<DataRetrivalOption> UiSet = new HashSet<DataRetrivalOption>() { DataRetrivalOption.Ui, DataRetrivalOption.Location, DataRetrivalOption.Name, DataRetrivalOption.SubEvent, DataRetrivalOption.DataBlob };
        public readonly static HashSet<DataRetrivalOption> All = new HashSet<DataRetrivalOption>() { DataRetrivalOption.All };
    }
}
