using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    /// <summary>
    /// Class is simply to provide a way to hold the result of an evaluated schedule. It is not meant to be used outside this Class. 
    /// </summary>
    public class HealthEvaluation
    {
        Health ScheduleHealth;
        List<BlobSubCalendarEvent> _ConflictingEvents { get; set; }
        double _TotalDistance { get; set; }
        double _PositioningScore { get; set; }
        List<TimeSpan> _SleepSchedule { get; set; }
        TravelTime _TravelTimeAnalysis { get; set; }
        DateTimeOffset _TimeOfAnalysis = DateTimeOffset.UtcNow;
        public HealthEvaluation(Health health)
        {
            this.ScheduleHealth = health;
        }

        public async Task evaluate(TimeLine timeLine)
        {
            this._ConflictingEvents = this.ScheduleHealth.evaluateConflicts();
            this._TotalDistance = this.ScheduleHealth.TotalDistance;
            this._PositioningScore = this.ScheduleHealth.evaluatePositioning();
            this._SleepSchedule = this.ScheduleHealth.SleepTimeLines.Select(sleepTimeLine => sleepTimeLine.TimelineSpan).ToList();
            this._TravelTimeAnalysis = new TravelTime(this.ScheduleHealth.orderedByStartThenEndSubEvents, this.ScheduleHealth.TravelMode);
        }


        public List<BlobSubCalendarEvent> ConflictingEvents
        {
            set
            {
                _ConflictingEvents = value;
            }
            get
            {
                return _ConflictingEvents;
            }
        }
        public double TotalDistance
        {
            set
            {
                _TotalDistance = value;
            }
            get
            {
                return _TotalDistance;
            }
        }
        public double PositioningScore
        {
            set
            {
                _PositioningScore = value;
            }
            get
            {
                return _PositioningScore;
            }
        }
        public List<TimeSpan> SleepSchedule
        {
            get
            {
                return _SleepSchedule;
            }
        }
        public TravelTime TravelTimeAnalysis
        {
            get
            {
                return _TravelTimeAnalysis;
            }
        }
    }

}
