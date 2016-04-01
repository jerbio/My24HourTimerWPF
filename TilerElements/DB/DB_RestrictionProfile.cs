using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements.Wpf;

namespace TilerElements.DB
{
    public class DB_RestrictionProfile : RestrictionProfile
    {
        
        public DB_RestrictionProfile(List<Tuple<DayOfWeek, RestrictionTimeLine>> RestrictionTimeLineData)
        {
            this.daySelection = new Tuple<DayOfWeek, RestrictionTimeLine>[RestrictionTimeLineData.Count];
            foreach (Tuple<DayOfWeek, RestrictionTimeLine> eachTuple in RestrictionTimeLineData)
            {
                daySelection[(int)eachTuple.Item1] = eachTuple;
            }

            this.noNull_DaySelections = RestrictionTimeLineData.OrderBy(obj => obj.Item1).ToArray();
            InitializeOverLappingDictionary();
        }

        public DB_RestrictionProfile(RestrictionProfile RestrictionData)
        {
            this.dayOfWeekToOverlappingIndexes = RestrictionData.DayOfWeekToOverlappingIndexes;
            this.noNull_DaySelections = RestrictionData.NoNull_DaySelections;
            this.daySelection = RestrictionData.DaySelection;
            this.Id = RestrictionData.Id;
        }

        List<DB_RestrictionTimeLine> ConvertTuplesToDB_RestrictionTimeLine (ICollection<Tuple<DayOfWeek, RestrictionTimeLine>> Restrictions)
        {
            List<DB_RestrictionTimeLine> DB_RestrictionTimeLines = new List<DB_RestrictionTimeLine>();
            foreach (Tuple<DayOfWeek, RestrictionTimeLine> eachTuple in Restrictions)
            {
                DB_RestrictionTimeLine singleRestriction = new DB_RestrictionTimeLine(eachTuple.Item1, eachTuple.Item2.Start, eachTuple.Item2.End, eachTuple.Item2.Span, this);
                DB_RestrictionTimeLines.Add(singleRestriction);
            }

            return DB_RestrictionTimeLines;
        }

        Tuple<DayOfWeek, RestrictionTimeLine>[] ConvertDB_RestrictionTimeLineToTuples(ICollection<DB_RestrictionTimeLine> Restrictions)
        {
            List<Tuple<DayOfWeek, RestrictionTimeLine>> DB_RestrictionTimeLines = new List<Tuple<DayOfWeek, RestrictionTimeLine>>();
            foreach (DB_RestrictionTimeLine eachDB_RestrictionTimeLine in Restrictions)
            {
                Tuple<DayOfWeek, RestrictionTimeLine> tuple = new Tuple<DayOfWeek, RestrictionTimeLine>(eachDB_RestrictionTimeLine.WeekDay, eachDB_RestrictionTimeLine);
                DB_RestrictionTimeLines.Add(tuple);
            }
            return DB_RestrictionTimeLines.ToArray();
        }

        #region Properties

        public new ICollection<DB_RestrictionTimeLine> NoNull_DaySelections
        {
            get
            {
                List<DB_RestrictionTimeLine> RetValue = ConvertTuplesToDB_RestrictionTimeLine(this.daySelection);
                return RetValue;
            }
            set {
                this.daySelection = ConvertDB_RestrictionTimeLineToTuples(value);
            }
        }


        /*public int NoNullDaySelectionss
        {
            get
            {
                return noNull_DaySelections;
            }
            set
            {
                this.noNull_DaySelections = value;
            }
        }

        public DateTimeOffset Restriction*/


        #endregion region
    }
}