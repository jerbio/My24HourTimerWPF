using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class ReferenceNow
    {
        static DateTimeOffset StartOfTime;
        DateTimeOffset CalculationNow;
        DateTimeOffset ImmutableNow;
        UInt64 ImmutableDayIndex;//'Cause tiler will exist 18,446,744,073,709,551,615 from 1970 
        TimeSpan ConstOfCalculation = new TimeSpan(90, 0, 0, 0, 0);
        DateTimeOffset tempNow = new DateTimeOffset(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month, DateTimeOffset.Now.Day, 0, 0, 0, new TimeSpan());
        TimeLine ComputationBound;// = new TimeLine(new DateTimeOffset(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month, DateTimeOffset.Now.Day, 0, 0, 0, new TimeSpan()), new DateTimeOffset(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month, DateTimeOffset.Now.Day, 0, 0, 0, new TimeSpan()).AddDays(90));
        DateTimeOffset StartOfDay;
        DayTimeLine refFirstDay;
        DayTimeLine[] AllDays;
        Dictionary<ulong, DayTimeLine> DayLookUp;
        uint DayCount;

        public ReferenceNow(DateTimeOffset Now, DateTimeOffset StartOfDay)
        {
            StartOfTime = new DateTimeOffset(1970, 1, 1, StartOfDay.Hour, StartOfDay.Minute, 0, new TimeSpan());
            Now = new DateTimeOffset(Now.Year, Now.Month, Now.Day, Now.Hour, Now.Minute, 0, new TimeSpan());
            CalculationNow = Now;
            this.StartOfDay = StartOfDay;
            InitializeParameters();
            
        }

        void InitializeParameters()
        {
            DateTimeOffset IndifferentStartOfDay = new DateTimeOffset(CalculationNow.Year, CalculationNow.Month, CalculationNow.Day, StartOfDay.Hour, StartOfDay.Minute, 0, new TimeSpan());
            DateTimeOffset refDayStart = CalculationNow;// < IndifferentStartOfDay ? CalculationNow : IndifferentStartOfDay;
            DateTimeOffset refDayEnd = CalculationNow < IndifferentStartOfDay ?refDayStart :refDayStart .AddDays(1);
            refDayEnd = new DateTimeOffset(refDayEnd.Year, refDayEnd.Month, refDayEnd.Day, StartOfDay.Hour, StartOfDay.Minute, 0, new TimeSpan());
            refFirstDay = new DayTimeLine(refDayStart, refDayEnd, (ulong)(refDayStart - StartOfTime).TotalDays,0);

            new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan()); ImmutableNow = CalculationNow;
            
            ImmutableDayIndex = (ulong)(refDayStart - StartOfTime).TotalDays;
            ComputationBound = new TimeLine(CalculationNow, CalculationNow.Add(ConstOfCalculation));
            InitializeAllDays();
        }


        void InitializeAllDays()
        {
            List<DayTimeLine> AllDayTImeLine = new List<DayTimeLine>();
            DayTimeLine FirstDay = (DayTimeLine)refFirstDay.CreateCopy();
            //AllDayTImeLine.Add(FirstDay);
            DateTimeOffset ComputationStart = refFirstDay.Start;
            DateTimeOffset ComputationEnd = refFirstDay.End;
            for (int i = 0; ComputationStart < ComputationBound.End; i++)
            {
                ulong myIndeUniversalIndex = getDayIndexFromStartOfTime(ComputationStart);
                AllDayTImeLine.Add( new DayTimeLine(ComputationStart, ComputationEnd, myIndeUniversalIndex, i));
                ComputationStart = ComputationEnd;
                ComputationEnd = ComputationEnd.AddDays(1);
            }
            AllDays = AllDayTImeLine.ToArray();
            DayLookUp = AllDays.ToDictionary(obj => obj.UniversalIndex, obj => obj);
            DayCount = (uint)AllDays.Length;
            ComputationBound = new TimeLine(AllDays[0].Start, AllDays[AllDays.Length - 1].End);
        }


        
        /// <summary>
        /// returns the day references using their universal indexes as the key. Note this is returning a reference for performance reasons.  If you nodify dictionary you'll be modifying the orginal dict.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<ulong, DayTimeLine>> getAllDaysLookup()
        {
            return DayLookUp;
        }

        public IEnumerable<DayTimeLine> getAllDaysForCalc()
        {
            return AllDays;
        }

        public IEnumerable<DayTimeLine> getAllDaysCount(uint NumberOfDays)
        {
            List<DayTimeLine> RetValue = getAllDaysForCalc().ToList().GetRange(0, (int)NumberOfDays);
            return RetValue;
        }

        public DateTimeOffset UpdateNow(DateTimeOffset UpdatedNow,bool ForceDayTimeReinitialization = true)
        {
            CalculationNow = UpdatedNow;
            if (ForceDayTimeReinitialization)
            {
                InitializeParameters();
            }
            return CalculationNow;
        }

        public int getDayIndexComputationBound(DateTimeOffset myDay)
        {
            int retValue = (int)(myDay - ComputationBound.Start).TotalDays;
            return retValue;
        }

        public Tuple<int, int> indexRange(TimeLine Range)
        {
            Tuple<int, int> retValue = new Tuple<int, int>((int)(Range.Start - ComputationBound.Start).TotalDays, (int)(Range.End - ComputationBound.Start).TotalDays);
            return retValue;
        }

        static public ulong getDayIndexFromStartOfTime(DateTimeOffset myDay)
        {
            ulong retValue = (ulong)((myDay - StartOfTime).TotalDays);
            return retValue;
        }

        public DateTimeOffset constNow
        {
            get
            {
                return ImmutableNow;
            }
        }

        public DateTimeOffset calculationNow
        {
            get
            {
                return CalculationNow;
            }
        }
        /// <summary>
        /// This generates the timeline for the first day. Not this does not necessarily mean a twenty four hour day. THe first day is described as the current time till the end of the day
        /// </summary>
        public DayTimeLine firstDay
        {
            get
            {
                return (DayTimeLine)refFirstDay.CreateCopy();
            }
        }



        public ulong consttDayIndex
        {
            get
            {
                return ImmutableDayIndex;
            }
        }

        public uint NumberOfDays
        {
            get
            {
                return DayCount;
            }
        }

        public TimeLine ComputationRange
        {
            get
            {
                return ComputationBound;
            }
        }


    }

}
