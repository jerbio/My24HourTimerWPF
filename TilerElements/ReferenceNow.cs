using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodaTime;
using static TilerElements.TimeOfDayPreferrence;

namespace TilerElements
{
    public class ReferenceNow
    {
        protected static DateTimeOffset _StartOfTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan());
        public static readonly long UndesignatedDayIndex = 0;
        public DateTimeOffset EndOfDayStartOfTime;// holds the End of day from 1970 January 1st. Note this will be the end of the zero universal index day. So if 10:00PM MST(-6:00) is provided this will be January 2 4:00AM because January 1, 1970 10:00PM MST is January 2 4:00A UTC
        private DateTimeOffset CalculationNow;
        private DateTimeOffset ImmutableNow;
        const int numberOfDfDays = 90;
        Int64 ImmutableDayIndex;//'Cause tiler will exist 18,446,744,073,709,551,615 days from 1970 ¯\_(ツ)_/¯
        protected TimeSpan ConstOfCalculation = new TimeSpan(numberOfDfDays, 0, 0, 0, 0);
        DateTimeOffset tempNow = new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, DateTimeOffset.UtcNow.Day, 0, 0, 0, new TimeSpan());
        TimeLine ComputationBound;// = new TimeLine(new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, DateTimeOffset.UtcNow.Day, 0, 0, 0, new TimeSpan()), new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, DateTimeOffset.UtcNow.Day, 0, 0, 0, new TimeSpan()).AddDays(90));
        DayTimeLine refFirstDay;
        DayOfWeek _ConstDayOfTheWeek;// this should be day of the week that constNow falls into. This belongs to the day of the week the of the timezone the request is coming from and not the time zone of the machine
        protected DayTimeLine[] AllDays;
        Dictionary<long, DayTimeLine> DayLookUp;
        public TimeSpan SleepSpan = Utility.SleepSpan;
        protected TimeSpan TimeZoneDiff;
        long lastDayIndex = 0;
        uint _DayCount;

        public ReferenceNow(DateTimeOffset now, DateTimeOffset endOfDay, TimeSpan timeDifference)
        {
            TimeZoneDiff = timeDifference;
            //DateTimeOffset endOfDayUniversal = endOfDay.ToUniversalTime();
            EndOfDayStartOfTime = new DateTimeOffset(1970, 1, 1, endOfDay.Hour, endOfDay.Minute, 0, endOfDay.Offset).ToUniversalTime();
            CalculationNow = now.removeSecondsAndMilliseconds().ToUniversalTime();
            ImmutableNow = CalculationNow;
            
            DateTimeOffset currentTime = new DateTimeOffset(ImmutableNow.Year, ImmutableNow.Month, ImmutableNow.Day, 0,0,0, new TimeSpan());
            DayOfWeek currentDayOfWeek = currentTime.DayOfWeek;
            DateTimeOffset startTimeForDayOfweek = currentTime.Add(timeDifference).removeSecondsAndMilliseconds();
            _ConstDayOfTheWeek = getDayOfTheWeek(ImmutableNow).Item1;
            ImmutableDayIndex = (long)(ImmutableNow - EndOfDayStartOfTime).TotalDays;
            InitializeParameters();
        }

        public virtual void InitializeParameters()
        {
            DateTimeOffset IndifferentEndOfDay = new DateTimeOffset(CalculationNow.Year, CalculationNow.Month, CalculationNow.Day, EndOfDayStartOfTime.Hour, EndOfDayStartOfTime.Minute, 0, new TimeSpan());
            DateTimeOffset refDayStart = CalculationNow;
            DateTimeOffset refDayEnd = CalculationNow < IndifferentEndOfDay ?refDayStart :refDayStart .AddDays(1);
            refDayEnd = new DateTimeOffset(refDayEnd.Year, refDayEnd.Month, refDayEnd.Day, EndOfDayStartOfTime.Hour, EndOfDayStartOfTime.Minute, 0, new TimeSpan());
            refFirstDay = new DayTimeLine(refDayStart, refDayEnd, (long)(refDayStart - EndOfDayStartOfTime).TotalDays,0);
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
            TimeSpan oneMinute = TimeSpan.FromMinutes(1);
            for (int i = 0; ComputationStart < ComputationBound.End; i++)
            {
                long myIndeUniversalIndex = getDayIndexFromStartOfTime(ComputationStart);
                AllDayTImeLine.Add( new DayTimeLine(ComputationStart, ComputationEnd.Subtract(oneMinute), myIndeUniversalIndex, i));
                ComputationStart = ComputationEnd;
                ComputationEnd = ComputationEnd.AddDays(1);
                if(lastDayIndex < myIndeUniversalIndex)
                {
                    lastDayIndex = myIndeUniversalIndex;
                }
            }
            AllDays = AllDayTImeLine.ToArray();
            DayLookUp = AllDays.ToDictionary(obj => obj.UniversalIndex, obj => obj);
            _DayCount = (uint)AllDays.Length;
            ComputationBound = new TimeLine(AllDays[0].Start, AllDays[AllDays.Length - 1].End);
        }


        
        /// <summary>
        /// returns the day references using their universal indexes as the key. Note this is returning a reference for performance reasons.  If you nodify dictionary you'll be modifying the orginal dict.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<long, DayTimeLine>> getAllDaysLookup()
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
            
            long dayIndex = getDayIndexFromStartOfTime(time);
            DayTimeLine retValue = getDayTimeLineByDayIndex(dayIndex);
            return retValue;
        }


        /// <summary>
        /// Function return the corresponding daytimeline for a provided dayIndex. Note this dayIndex is the universal day index. If a dayindex cannot be found an exception is thrown. So the dayindex being provided has to be pertinent to the initialization of this reference now object.
        /// e.g if this reference now is being initialized ON 1970,1,2 - 1970, 1, 4. This means this will be initialized with an index of 1-3. Remmber universsal time starts from 1970,1,1. Meaning if you try to access the day universal indexes of 0 or 4, this function will throw an exception.
        /// </summary>
        /// <param name="dayIndex">the desired dxay index</param>
        /// <returns></returns>
        public DayTimeLine getDayTimeLineByDayIndex(long dayIndex)
        {
            if (DayLookUp.ContainsKey(dayIndex))
            {
                return DayLookUp[dayIndex];
            }
            long start = lastDayIndex - (_DayCount-1);
            long end = lastDayIndex;
            string errorMessage = "You are trying to make a query for a day that isn't within the " + ConstOfCalculation.TotalDays + " For Reference now. Hint: the only valid indexes are" + start + " - " + end;
            throw new Exception(errorMessage);
        }

        /// <summary>
        /// This function tries to get the day of the for which "time" is provided. Note this is based on the timezone for which this got instantiated so for example if the data member "TimeZoneDiff" is -9:00 and current time is on Saturday 7/20/2019 1:00AM (UTC) this is saturday in UTC
        /// but still Friday 7/20/2019 3:00PM (-9:00UTC) this function will return friday and the full 24 hours of the friday.
        /// The timeline will be based on timezondiff, meaning shifted accordingly. SO if the data member "TimeZoneDiff" is -9:00 and current time is on Saturday 7/20/2019 1:00AM (UTC)
        /// The timeline will be 7/20/2019 9:00AM - 7/21/2019 9:00AM This is the UTC date shifted but represents the friday frame of the -9:00 time zone 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Tuple<DayOfWeek, TimeLine> getDayOfTheWeek (DateTimeOffset time)
        {
            Tuple<DayOfWeek, TimeLine> retValue;
            DateTimeOffset currentTime = new DateTimeOffset(time.Year, time.Month, time.Day, 0, 0, 0, new TimeSpan());
            DayOfWeek currentDayOfWeek = currentTime.DayOfWeek;
            DateTimeOffset startTimeForDayOfweek = new DateTimeOffset(time.Year, time.Month, time.Day, 0, 0, 0, this.TimeZoneDiff);
            TimeLine fullDayTime = new TimeLine(startTimeForDayOfweek, startTimeForDayOfweek.AddDays(1));
            if (fullDayTime.IsDateTimeWithin(time))
            {
                retValue = new Tuple<DayOfWeek, TimeLine>(currentDayOfWeek, fullDayTime) ;
            }
            else
            {
                if (time < fullDayTime.Start)
                {
                    DayOfWeek weekDay = (DayOfWeek)((((int)currentDayOfWeek - 1) + 7) % 7);

                    retValue = new Tuple<DayOfWeek, TimeLine>(weekDay, new TimeLine(fullDayTime.Start.AddDays(-1), fullDayTime.End.AddDays(-1)));
                }
                else
                {
                    DayOfWeek weekDay = (DayOfWeek)((((int)currentDayOfWeek + 1) + 7) % 7);
                    retValue = new Tuple<DayOfWeek, TimeLine>(weekDay, new TimeLine(fullDayTime.Start.AddDays(1), fullDayTime.End.AddDays(1)));
                }
            }

            return retValue;
        }
        /// <summary>
        /// This function gets the day of the week for which the timeline mostly conflicts with.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public TimeLine getDayOfTheWeekTimeLine(DateTimeOffset time)
        {
            return getDayOfTheWeek(time).Item2;
        }

        public TimeOfDayPreferrence.DaySection getDaySection(DateTimeOffset time)
        {
            var retTuple = getDaySectionAndTimeLine(time);
            return retTuple.Item1;
        }

        /// <summary>
        /// Funcion gets the day sector associated with the provided time. It returns the Day sector and the time line of the full day sector.
        /// Note the returned timeline is the full sector. So even it is part of a daytimeline thats less than 24 hours it will could return a timeline that is out side the daytimeline
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Tuple<DaySection, TimeLine> getDaySectionAndTimeLine(DateTimeOffset time)
        {
            DayTimeLine referenceDayTimeLine = getDayTimeLineByTime(time);

            if (referenceDayTimeLine != null)
            {
                Tuple<DaySection, TimeLine> retValue;

                TimeLine timeLine = AllDays[4];

                TimeSpan span = timeLine.Start - time;
                int dayCount = (int)Math.Floor(span.TotalDays);
                DateTimeOffset revisedTime = time.AddDays(dayCount);
                var daySections = TimeOfDayPreferrence.ActiveDaySections.ToList();
                DayTimeLine dayTimeLine = getDayTimeLineByTime(revisedTime);
                TimeSpan timeSpanPerSection = TimeSpan.FromMinutes(Math.Ceiling(dayTimeLine.TimelineSpan.TotalMinutes / daySections.Count));
                TimeLine sectionTimeLine = new TimeLine(dayTimeLine.Start, dayTimeLine.Start.Add(timeSpanPerSection).removeSecondsAndMilliseconds());
                foreach (var daySection in daySections)
                {
                    if (sectionTimeLine.IsDateTimeWithin(revisedTime))
                    {
                        long dayIidex = (long)this.getDayIndexFromStartOfTime(sectionTimeLine.Start);
                        long dayShift = (long)referenceDayTimeLine.UniversalIndex - dayIidex;

                        DateTimeOffset newStart = sectionTimeLine.Start;
                        DateTimeOffset newEnd = sectionTimeLine.End;

                        newStart = newStart.AddDays(dayShift);
                        newEnd = newEnd.AddDays(dayShift);
                        TimeLine retTimeline = new TimeLine(newStart, newEnd);
                        retValue = new Tuple<DaySection, TimeLine>(daySection, retTimeline);
                        return retValue;
                    }
                    else
                    {
                        sectionTimeLine = new TimeLine(sectionTimeLine.End, sectionTimeLine.End.Add(timeSpanPerSection));
                    }
                }
                throw new Exception("Something is wrong about this loop for day section");
            }
            throw new Exception("Time should be within the now window of timelines");
        }


        public DayOfWeek getDayOfTheWeek(TimeLine timeLine)
        {
            var startOfTimeLine = getDayOfTheWeek(timeLine.Start);
            var endTimeLine = getDayOfTheWeek(timeLine.End);
            DayOfWeek retValue;
            if (startOfTimeLine.Item1 == endTimeLine.Item1)
            {
                retValue = startOfTimeLine.Item1;
            } else {
                TimeLine startInterferring = startOfTimeLine.Item2.InterferringTimeLine(timeLine);
                TimeLine endInterferring = endTimeLine.Item2.InterferringTimeLine(timeLine);
                if(startInterferring !=null && endInterferring !=null)
                {
                    if (startInterferring.TimelineSpan >= endInterferring.TimelineSpan)
                    {
                        retValue = startOfTimeLine.Item1;
                    }
                    else
                    {
                        retValue = endTimeLine.Item1;
                    }
                } else
                {
                    retValue = startInterferring != null ? startOfTimeLine.Item1 : endTimeLine.Item1;
                }
            }

            return retValue;
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

        virtual public Tuple<long, long> indexRange(TimeLine Range)
        {
            Tuple<long, long> retValue = new Tuple<long, long>((long)(Range.Start - ComputationBound.Start).TotalDays, (long)(Range.End - ComputationBound.Start).TotalDays);
            return retValue;
        }

        /// <summary>
        /// This returns the universal index relative to the start of time. which in this case is supposed to be 1970 ,1,1. Note all calculations are done using the utc timezone
        /// </summary>
        /// <param name="myDay">the time to be used as the reference day. This will be the beginning of the utc day</param>
        /// <returns></returns
        public long getDayIndexFromStartOfTime(DateTimeOffset myDay)
        {
            double totalDays = (myDay - EndOfDayStartOfTime).TotalDays;
            long retValue = totalDays >= 0 ? (long)totalDays : ((long)totalDays - 1);
            return retValue;
        }

        /// <summary>
        /// Function returns all the day sectors within the timeline in the respective order of the timeline
        /// </summary>
        /// <param name="timeLine"></param>
        /// <returns></returns>
        public List<Tuple<TimeOfDayPreferrence.DaySection, TimeLine>>getDaySections(TimeLine timeLine)
        {
            List<Tuple<TimeOfDayPreferrence.DaySection, TimeLine>> retValue = new List<Tuple<TimeOfDayPreferrence.DaySection, TimeLine>>();

            var daySectorAndTimeline = getDaySectionAndTimeLine(timeLine.Start);
            TimeLine sectorInterferringTimeline = timeLine.InterferringTimeLine(daySectorAndTimeline.Item2);
            Tuple<DaySection, TimeLine> tuple = new Tuple<DaySection, TimeLine>(daySectorAndTimeline.Item1, sectorInterferringTimeline);
            retValue.Add(tuple);
            TimeLine sectionTimeLine = daySectorAndTimeline.Item2;
            DateTimeOffset nextStart = sectionTimeLine.End.removeSecondsAndMilliseconds();
            TimeLine nextSectionTimeLIne = new TimeLine(nextStart, nextStart.Add(sectionTimeLine.TimelineSpan));
            TimeLine interFerringTimeLine = timeLine.InterferringTimeLine(nextSectionTimeLIne);
            while (interFerringTimeLine!=null)
            {
                daySectorAndTimeline = getDaySectionAndTimeLine(interFerringTimeLine.Start);
                interFerringTimeLine = timeLine.InterferringTimeLine(daySectorAndTimeline.Item2);
                sectorInterferringTimeline = interFerringTimeLine;
                tuple = new Tuple<DaySection, TimeLine>(daySectorAndTimeline.Item1, sectorInterferringTimeline);
                retValue.Add(tuple);
                if (interFerringTimeLine.TimelineSpan != daySectorAndTimeline.Item2.TimelineSpan)
                {
                    break;
                } else
                {
                    sectionTimeLine = daySectorAndTimeline.Item2;
                    nextStart = sectionTimeLine.End.removeSecondsAndMilliseconds();
                    nextSectionTimeLIne = new TimeLine(nextStart, nextStart.Add(sectionTimeLine.TimelineSpan));
                    interFerringTimeLine = timeLine.InterferringTimeLine(nextSectionTimeLIne);
                }
            }
            
            return retValue;
        }

        /// <summary>
        /// Function gets you the beginning of the day based on the time zone difference. Note this is not based on end or start of day. 
        /// So if End of day is 10:00pm(MST) and the user is in denver time which is -6 from UTC. if you tried to get the value for <paramref name="dayIndex"/> being 0
        /// This will return December 31, 1969, 6:00AM because this return UTC time. Notice it is Dec 31 1969 because 
        /// </summary>
        /// <param name="dayIndex"></param>
        /// <returns></returns>
        public DateTimeOffset getClientBeginningOfDay(long dayIndex)
        {
            DateTimeOffset retValue = EndOfDayStartOfTime.AddDays(dayIndex).ToOffset(TimeZoneDiff);
            retValue = new DateTimeOffset(retValue.Year, retValue.Month, retValue.Day, 0, 0, 0, TimeZoneDiff).ToUniversalTime();
            return retValue;
        }

        public DayOfWeek ConstDayOfWeek
        {
            get {
                return _ConstDayOfTheWeek;
            }
        }

        public TimeSpan TimeZoneDifference
        {
            get
            {
                return TimeZoneDiff;
            }
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


        /// <summary>
        /// Day index of the constNow time. Note this isn't necessarily the same day as the firstDay.universalIndex. Const now can be on a next day which means constDay inxdex will be different
        /// </summary>
        public long consttDayIndex
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
                return _DayCount;
            }
        }

        public DateTimeOffset EndOfDay
        {
            get
            {
                return this.EndOfDayStartOfTime;
            }
        }

        public TimeLine ComputationRange
        {
            get
            {
                return ComputationBound;
            }
        }

        public static DateTimeOffset StartOfTimeUTC
        {
            get
            {
                return _StartOfTime;
            }
        }

    }

}
