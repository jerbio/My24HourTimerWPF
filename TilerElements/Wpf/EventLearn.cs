using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements.Wpf
{
    public class EventLearn
    {
        const double constantOfSubstraction=0.5;
        public EventLearn()
        { 
            
        }

        public mTuple<TimeLineWithEdgeElements,double> SelectBestTimeline(IEnumerable<TimeLineWithEdgeElements> AllTimeLines, SubCalendarEvent referenceSubCalendarEvent)
        {
            Dictionary<TimeLineWithEdgeElements, double> myScore = AllTimeLines.ToDictionary(obj => obj, obj => 0.0);
            myScore=EvaluateBestFittability(AllTimeLines, referenceSubCalendarEvent, myScore);
            List<KeyValuePair<TimeLineWithEdgeElements, double>> retValuePrep = myScore.Where(obj => myScore.Max(obj1 => obj1.Value) == obj.Value).ToList();

            mTuple<TimeLineWithEdgeElements,double> retValue= new mTuple<TimeLineWithEdgeElements,double>(retValuePrep[0].Key,retValuePrep[0].Value);

            return retValue;
        }


        public mTuple<SubCalendarEvent, double> SelectBestSubCalendarEvent(TimeLineWithEdgeElements referenceTimeLines, IEnumerable <SubCalendarEvent >AllSubCalendarEvents)
        {
            Dictionary<SubCalendarEvent, double> myScore = AllSubCalendarEvents.ToDictionary(obj => obj, obj => 0.0);

            myScore = EvaluateBestFittability(AllSubCalendarEvents, referenceTimeLines, myScore);
            double max=myScore.Max(obj1 => obj1.Value);

            List<KeyValuePair<SubCalendarEvent, double>> retValuePrep = myScore.Where(obj => max == obj.Value).ToList();

            mTuple<SubCalendarEvent, double> retValue = new mTuple<SubCalendarEvent, double>(retValuePrep[0].Key, retValuePrep[0].Value);

            return retValue;

        }

        Dictionary<SubCalendarEvent, double> EvaluateBestFittability(IEnumerable<SubCalendarEvent> AllSubCalendarEvents, TimeLineWithEdgeElements referenceTimeLines, Dictionary<SubCalendarEvent, double> ScoreSofar)
        {
            TimeSpan maxTimeSpan = new TimeSpan(0);

            Dictionary<SubCalendarEvent, TimeLine> SubCalendarEvent_TimeSapn;

            SubCalendarEvent_TimeSapn = AllSubCalendarEvents.ToDictionary(obj => obj, obj => obj.getCalendarEventRange.InterferringTimeLine(referenceTimeLines));
            Dictionary<SubCalendarEvent, TimeLine> notNull = SubCalendarEvent_TimeSapn.Where(obj => obj.Value != null).ToDictionary(obj => obj.Key, obj => obj.Value);
            if (notNull.Count > 0)
            {
                maxTimeSpan = notNull.Max(obj => obj.Value.TimelineSpan);
            }

            List<SubCalendarEvent> allKeys = ScoreSofar.Keys.ToList();

            foreach (SubCalendarEvent eachKeyValuePair in allKeys)
            {
                if (notNull.ContainsKey(eachKeyValuePair))
                {
                    ScoreSofar[eachKeyValuePair] += notNull[eachKeyValuePair].TimelineSpan.Ticks / maxTimeSpan.Ticks;
                }
                else
                {
                    ScoreSofar[eachKeyValuePair] -= constantOfSubstraction;
                }
            }

            return ScoreSofar;
        }


        Dictionary<TimeLineWithEdgeElements,double> EvaluateBestFittability(IEnumerable<TimeLineWithEdgeElements> AllTimeLines, SubCalendarEvent referenceSubCalendarEvent, Dictionary<TimeLineWithEdgeElements,double> ScoreSofar)
        {
            TimeSpan maxTimeSpan = new TimeSpan(0);

            Dictionary<TimeLineWithEdgeElements, TimeLine> Timeline_TimeSapn;

            Timeline_TimeSapn = AllTimeLines.ToDictionary(obj => obj, obj => obj.InterferringTimeLine(referenceSubCalendarEvent.getCalendarEventRange));
            Dictionary<TimeLineWithEdgeElements, TimeLine>  notNull = Timeline_TimeSapn.Where(obj => obj.Value != null).ToDictionary(obj=>obj.Key, obj=>obj.Value);
            if (notNull.Count > 0)
            {
                maxTimeSpan = notNull.Max(obj => obj.Value.TimelineSpan);
            }

            foreach(KeyValuePair<TimeLineWithEdgeElements,double> eachKeyValuePair in ScoreSofar)
            {
                if(notNull.ContainsKey(eachKeyValuePair.Key))
                {
                    ScoreSofar[eachKeyValuePair.Key]+= notNull[eachKeyValuePair.Key].TimelineSpan.Ticks/maxTimeSpan.Ticks;
                }
                else
                {
                    ScoreSofar[eachKeyValuePair.Key] -= constantOfSubstraction;
                }
            }

            return ScoreSofar;

        }

        public float generateScore()
        {
            return 1;
        }
    }
}
