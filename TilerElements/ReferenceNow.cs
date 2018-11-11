using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class ReferenceNow
    {
        public static DateTimeOffset StartOfTime;
        DateTimeOffset CalculationNow;
        DateTimeOffset ImmutableNow;
        const int numberOfDfDays = 90;
        UInt64 ImmutableDayIndex;//'Cause tiler will exist 18,446,744,073,709,551,615 from 1970 
        protected TimeSpan ConstOfCalculation = new TimeSpan(numberOfDfDays, 0, 0, 0, 0);
        DateTimeOffset tempNow = new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, DateTimeOffset.UtcNow.Day, 0, 0, 0, new TimeSpan());
        TimeLine ComputationBound;// = new TimeLine(new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, DateTimeOffset.UtcNow.Day, 0, 0, 0, new TimeSpan()), new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, DateTimeOffset.UtcNow.Day, 0, 0, 0, new TimeSpan()).AddDays(90));
        DateTimeOffset startOfDay;
        DayTimeLine refFirstDay;
        protected DayTimeLine[] AllDays;
        Dictionary<ulong, DayTimeLine> DayLookUp;
        public TimeSpan SleepSpan = new TimeSpan(0, 8, 0, 0, 0);
        ulong lastDayIndex = 0;
        uint DayCount;

        public ReferenceNow(DateTimeOffset Now, DateTimeOffset StartOfDay)
        {
            StartOfTime = new DateTimeOffset(1970, 1, 1, StartOfDay.Hour, StartOfDay.Minute, 0, new TimeSpan());
            Now = new DateTimeOffset(Now.Year, Now.Month, Now.Day, Now.Hour, Now.Minute, 0, new TimeSpan());
            CalculationNow = Now;
            ImmutableNow = CalculationNow;
            this.startOfDay = StartOfDay;
            InitializeParameters();
            
        }

        public virtual void InitializeParameters()
        {
            DateTimeOffset IndifferentStartOfDay = new DateTimeOffset(CalculationNow.Year, CalculationNow.Month, CalculationNow.Day, startOfDay.Hour, startOfDay.Minute, 0, new TimeSpan());
            DateTimeOffset refDayStart = CalculationNow;// < IndifferentStartOfDay ? CalculationNow : IndifferentStartOfDay;
            DateTimeOffset refDayEnd = CalculationNow < IndifferentStartOfDay ?refDayStart :refDayStart .AddDays(1);
            refDayEnd = new DateTimeOffset(refDayEnd.Year, refDayEnd.Month, refDayEnd.Day, startOfDay.Hour, startOfDay.Minute, 0, new TimeSpan());
            refFirstDay = new DayTimeLine(refDayStart, refDayEnd, (ulong)(refDayStart - StartOfTime).TotalDays,0);

            new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan()); 
            
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
                if(lastDayIndex < myIndeUniversalIndex)
                {
                    lastDayIndex = myIndeUniversalIndex;
                }
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


        /// <summary>
        /// Function return the corresponding daytimeline for a provided daytime. In the time calculations are alwways done in UTC. Note this dayIndex is the universal day index. If a dayindex cannot be found an exception is thrown. So the time being provided has to be pertinent to the initialization of this reference now object.
        /// e.g if this reference now is being initialized ON 1970,1,2 - 1970, 1, 4. This means this will be initialized with an index of just daytimelines within that timeline. Remmber universsal time starts from 1970,1,1. Meaning if you try to access the days of 1970,1,1 or 1970,1,5, this function will throw an exception.
        /// </summary>
        /// <param name="time">time to be looked up</param>
        /// <returns></returns>
        public DayTimeLine getDayTimeLineByTime(DateTimeOffset time)
        {
            
            ulong dayIndex = getDayIndexFromStartOfTime(time);
            DayTimeLine retValue = getDayTimeLineByDayIndex(dayIndex);
            return retValue;
        }


        /// <summary>
        /// Function return the corresponding daytimeline for a provided dayIndex. Note this dayIndex is the universal day index. If a dayindex cannot be found an exception is thrown. So the dayindex being provided has to be pertinent to the initialization of this reference now object.
        /// e.g if this reference now is being initialized ON 1970,1,2 - 1970, 1, 4. This means this will be initialized with an index of 1-3. Remmber universsal time starts from 1970,1,1. Meaning if you try to access the day universal indexes of 0 or 4, this function will throw an exception.
        /// </summary>
        /// <param name="dayIndex">the desired dxay index</param>
        /// <returns></returns>
        public DayTimeLine getDayTimeLineByDayIndex(ulong dayIndex)
        {
            if (DayLookUp.ContainsKey(dayIndex))
            {
                return DayLookUp[dayIndex];
            }
            ulong start = lastDayIndex - (DayCount-1);
            ulong end = lastDayIndex;
            string errorMessage = "You are trying to make a query for a day that isn't within the " + ConstOfCalculation.TotalDays + " For Reference now. Hint: the only valid indexes are" + start + " - " + end;
            throw new Exception(errorMessage);
        }

        public IEnumerable<DayTimeLine> getAllDaysCount(uint NumberOfDays)
        {
            List<DayTimeLine> RetValue = AllDays.Take((int)NumberOfDays).ToList();
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

        /// <summary>
        /// Function returns the index relative to the starting of the computation bound. Note, this does not return a universal index. If you ant a universal index then you should call getDayIndexFromStartOfTime
        /// Also this takes into account the start of the day
        /// </summary>
        /// <param name="myDay"></param>
        /// <returns></returns>
        public int getDayIndexComputationBound(DateTimeOffset myDay)
        {
            int beginIndex = (int)(myDay - ComputationBound.Start).TotalDays;
            //return beginIndex;
            int retValue = 0;
            int counter = 0;
            bool foundDay = false;
            for(counter =0; counter< 3;counter++)
            {
                beginIndex += counter;
                if (AllDays[beginIndex].IsDateTimeWithin(myDay))
                {
                    retValue = beginIndex;
                    foundDay = true;
                }
            }


            if (!foundDay)
            {
                throw new Exception("Something isn't right, could not find a day withing the three day limit of calculation");
            }

            return retValue;
        }

        virtual public Tuple<int, int> indexRange(TimeLine Range)
        {
            Tuple<int, int> retValue = new Tuple<int, int>((int)(Range.Start - ComputationBound.Start).TotalDays, (int)(Range.End - ComputationBound.Start).TotalDays);
            return retValue;
        }

        /// <summary>
        /// This returns the universal index relative to the start of time. which in this case is supposed to be 1970 ,1,1. Note all calculations are done using the utc timezone
        /// </summary>
        /// <param name="myDay">the time to be used as the reference day. This will be the beginning of the utc day</param>
        /// <returns></returns>
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

        public DateTimeOffset StartOfDay
        {
            get
            {
                return this.startOfDay;
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
