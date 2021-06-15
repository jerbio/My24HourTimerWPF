using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TilerElements;
using System.Threading.Tasks;
using System.Windows.Forms;
using NodaTime;


namespace TilerElements
{
    public static class Utility
    {
        public static DateTimeOffset StartOfTime = new DateTimeOffset();

        const uint fibonnaciLimit = 150;
        static uint[] fibonacciValues = new uint[fibonnaciLimit];
        public static DateTimeOffset JSStartTime = DateTimeOffset.FromUnixTimeMilliseconds(0);
        public static TimeSpan StartOfTimeTimeSpan = JSStartTime - new DateTimeOffset(0, new TimeSpan());
        public readonly static DateTimeOffset BeginningOfTime = new DateTimeOffset();
        public readonly static DateTimeOffset ProcrastinateStartTime = BeginningOfTime.AddDays(1);
        public readonly static Random rng = new Random();
        /// <summary>
        /// The default number of days before the currentTime to be use to include sub events for calculation
        /// </summary>
        public readonly static int defaultBeginDay = -15;
        /// <summary>
        /// The default number of days from the currentTime to be use to include sub events for calculation
        /// </summary>
        public readonly static int defaultEndDay = 90;
        public readonly static TimeSpan OneDayTimeSpan = TimeSpan.FromDays(1);
        public readonly static TimeSpan QuarterHourTimeSpan = TimeSpan.FromMinutes(15);
        public readonly static TimeSpan OneHourTimeSpan = TimeSpan.FromHours(1);
        public readonly static TimeSpan TwentyFourHoursAlmostTimeSpan = TimeSpan.FromDays(1).Subtract(TimeSpan.FromMinutes(1));
        public readonly static TimeSpan ZeroTimeSpan = TimeSpan.FromTicks(0);
        public readonly static TimeSpan OneMinuteTimeSpan = TimeSpan.FromMinutes(1);
        public readonly static TimeSpan NegativeTimeSpan = TimeSpan.FromTicks(-1);
        public readonly static TimeSpan SixHourTimeSpan = TimeSpan.FromHours(6);
        public readonly static TimeSpan OneWeekTimeSpan = TimeSpan.FromDays(7);
        public readonly static TimeSpan ArbitraryDayUseUpTimeSpan = TimeSpan.FromHours(1.5);// THis is to ensure things like showering, getting to bus station and random daily events that are not scheduled still get some aportioning
        public readonly static TimeSpan LeastAllDaySubeventDuration = Utility.OneDayTimeSpan.Add(-Utility.QuarterHourTimeSpan);
        public readonly static TimeSpan SleepSpan = Utility.SixHourTimeSpan;//careful about changing this from six hours because this is used to recalculate the post sleep timeline
        public static string timeZoneString = "";// "America/Denver";
        public static string debugString = "";// "America/Denver";

        static Utility()
        {
            initializeFibonacci();
        }

        public static bool isDebugSet()
        {
            return debugString.isNot_NullEmptyOrWhiteSpace();
        }
        static void initializeFibonacci()
        {
            uint a = 0;
            uint b = 1;
            fibonacciValues[0] = a;
            fibonacciValues[1] = b;
            // In N steps compute Fibonacci sequence iteratively.
            for (uint i = 2; i < fibonnaciLimit; i++)
            {
                uint temp = a;
                a = b;
                b = temp + b;
                fibonacciValues[i] = a;
            }
        }

        public static uint getFibonnacciNumber(uint index)
        {
            if (index >= fibonnaciLimit)
            {
                return fibonacciValues[fibonnaciLimit - 1];
            }
            else
            {
                return fibonacciValues[index];
            }
        }

        static public double getFibonacciSumToIndex(uint index)
        {
            IEnumerable<uint> values = fibonacciValues.Take((int)index);
            double retValue = values.Sum(value => value);
            return retValue;
        }
        public static List<SubCalendarEvent> sortSubCalEventByDeadline(List<SubCalendarEvent> SubCalEventRestricted, bool SecondSortByStartDate)
        {

            SubCalEventRestricted = SubCalEventRestricted.OrderBy(obj => obj.End).ToList();
            Dictionary<long, List<SubCalendarEvent>> Dict_DeadlineAndClashingDeadlineSubCalEvents = new Dictionary<long, List<SubCalendarEvent>>();
            foreach (SubCalendarEvent MySubCalEvent in SubCalEventRestricted)
            {

                if (Dict_DeadlineAndClashingDeadlineSubCalEvents.ContainsKey(MySubCalEvent.End.Ticks))
                {
                    Dict_DeadlineAndClashingDeadlineSubCalEvents[MySubCalEvent.End.Ticks].Add(MySubCalEvent);
                }
                else
                {
                    Dict_DeadlineAndClashingDeadlineSubCalEvents.Add(MySubCalEvent.End.Ticks, new List<SubCalendarEvent>());
                    Dict_DeadlineAndClashingDeadlineSubCalEvents[MySubCalEvent.End.Ticks].Add(MySubCalEvent);
                }
            }


            SubCalEventRestricted = new List<SubCalendarEvent>();
            foreach (long DeadLine in Dict_DeadlineAndClashingDeadlineSubCalEvents.Keys.ToArray())
            {
                Dict_DeadlineAndClashingDeadlineSubCalEvents[DeadLine] = Dict_DeadlineAndClashingDeadlineSubCalEvents[DeadLine].OrderBy(obj => obj.Start).ToList();
                if (!SecondSortByStartDate)
                { Dict_DeadlineAndClashingDeadlineSubCalEvents[DeadLine].Reverse(); }
                SubCalEventRestricted.AddRange(Dict_DeadlineAndClashingDeadlineSubCalEvents[DeadLine]);
            }



            return SubCalEventRestricted;
        }

        public static Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> NotInList_NoEffect(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> ListToCheck, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> MyCurrentList)
        {
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> retValue = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
            if (MyCurrentList.Keys.Count < 1)
            {
                foreach (TimeSpan eachTimespan in ListToCheck.Keys)
                {
                    retValue.Add(eachTimespan, new mTuple<int, TimeSpanWithStringID>(ListToCheck[eachTimespan].Item1, ListToCheck[eachTimespan].Item2));
                }
                return retValue;
            }

            foreach (TimeSpan eachTimeSpan in ListToCheck.Keys)
            {
                if (MyCurrentList.ContainsKey(eachTimeSpan))
                {
                    retValue.Add(eachTimeSpan, new mTuple<int, TimeSpanWithStringID>(ListToCheck[eachTimeSpan].Item1 - MyCurrentList[eachTimeSpan].Item1, ListToCheck[eachTimeSpan].Item2));
                }
                else
                {
                    retValue.Add(eachTimeSpan, new mTuple<int, TimeSpanWithStringID>(ListToCheck[eachTimeSpan].Item1, ListToCheck[eachTimeSpan].Item2));
                }
            }

            return retValue;
        }

        /// <summary>
        /// Function tires to find a central time slot of the span "Centralized" in the time slot. If Centralized span is larger than range it returns null
        /// </summary>
        /// <param name="Range"></param>
        /// <param name="Centralized"></param>
        /// <returns></returns>
        public static TimeLine CentralizeYourSelfWithinRange(TimeLine Range, TimeSpan Centralized)
        {
            TimeSpan Difference = Range.TimelineSpan - Centralized;
            TimeLine CentralizedTimeline = new TimeLine();
            if (Difference.TotalMilliseconds >= 0)
            {
                DateTimeOffset MyStart = Range.Start.Add(TimeSpan.FromMinutes(Math.Floor(Difference.TotalMinutes / 2)));
                CentralizedTimeline = new TimeLine(MyStart, MyStart.Add(Centralized));
                return CentralizedTimeline;
            }
            return null;
        }

        static public List<double> getOriginFromDimensions(IList<IList<double>> collection)
        {
            IList<double> firstDataSet = collection.First();
            int lengthOfEachDataset = firstDataSet.Count;
            List<double> summingArray = (new double [lengthOfEachDataset]).Select(obj=> 0.0).ToList();
            foreach(IList<double> eachDataSet in collection) 
            {
                for (int i = 0; i < lengthOfEachDataset; i++)
                {
                    summingArray[i] += eachDataSet[i];
                }
            }
            List<double> retValue = summingArray.Select(obj => obj / collection.Count).ToList();
            return retValue;
        }

        public static double CalcuateResultant(params double[] points)
        {
            double retValue = Math.Sqrt(points.AsParallel().Sum(point => point * point));
            return retValue;
        }

        static public List<double> multiDimensionCalculation(IList<IList<double>> collection, List<double> origin = null)
        {
            int counter = collection.Count;
            List<double> retValue = (new double[counter]).ToList();
            int length = collection.First().Count();
            if (origin == null)
            {
                origin = new List<double>();
                for (int i = 0; i < length; i++)
                {
                    origin.Add(0);
                }
            }

            for (int i = 0; i < counter; i++)
            {
                IList<double> calculationSet = collection[i];
                double sum = 0;
                if ((length != calculationSet.Count()) || (origin.Count != length))
                {
                    throw new Exception("Oops seems like you are trying to run pythagoras on sets of different sizes");
                }
                for (int j = 0; j < calculationSet.Count; j++)
                {
                    double delta = (calculationSet[j] - origin[j]);
                    sum += (delta * delta);
                }
                retValue[i] = Math.Sqrt(sum);
            }
            return retValue;
        }

        /// <summary>
        /// Function takes the provided collection and generates the resultant of the provided elements after running a self normalization or using the origin.
        /// Use this function over multiDimensionCalculation if you want to avoid a var of mulitvar calculation dominating the calculation
        /// NOTE: collection will get modified, just to reduce memory complexity
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="origin"></param>
        /// <param name="normalizedFields"></param>
        /// <returns></returns>
        static public List<double> multiDimensionCalculationNormalize(IList<IList<double>> collection, List<double> origin = null, IList<double> normalizedFields = null)
        {
            int counter = collection.Count;
            List<double> retValue = (new double[counter]).ToList();

            if (collection.Count > 0)
            {
                int length = collection.First().Count();
                if (origin == null)
                {
                    origin = new List<double>();
                    for (int i = 0; i < length; i++)
                    {
                        origin.Add(0);
                    }
                }

                if (normalizedFields == null)
                {
                    normalizedFields = new double[length];
                    for (int i = 0; i < normalizedFields.Count; i++)
                    {
                        normalizedFields[i] = collection.Select(obj => obj[i]).Max();
                        normalizedFields[i] = normalizedFields[i] == 0 ? 1 : normalizedFields[i];
                    }
                }

                //maxIndexes = normalizedFields.ToArray();
                for (int j = 0; j < collection.Count; j++)
                {
                    IList<double> row = collection[j];
                    for (int i = 0; i < normalizedFields.Count; i++)
                    {
                        row[i] = row[i] / normalizedFields[i];
                    }
                }

                for (int i = 0; i < counter; i++)
                {
                    IList<double> calculationSet = collection[i];
                    double sum = 0;
                    if ((length != calculationSet.Count()) || (origin.Count != length))
                    {
                        throw new Exception("Oops seems like you are trying to run pythagoras on sets of different sizes");
                    }
                    for (int j = 0; j < calculationSet.Count; j++)
                    {
                        double delta = (calculationSet[j] - origin[j]);
                        sum += (delta * delta);
                    }
                    retValue[i] = Math.Sqrt(sum);
                }
            }
            return retValue;
        }

        public static List<T> NotInList<T>(List<T> ListToCheck, List<T> MyCurrentList)
        {
            foreach (T MySubCalendarEvent in MyCurrentList)
            {
                ListToCheck.Remove(MySubCalendarEvent);
            }
            return ListToCheck;
        }


        public static List<T> NotInList_NoEffect<T>(List<T> ListToCheck, List<T> MyCurrentList)
        {
            List<T> ListToCheck_Cpy = ListToCheck.ToList();

            foreach (T MySubCalendarEvent in MyCurrentList)
            {
                ListToCheck_Cpy.Remove(MySubCalendarEvent);
            }
            return ListToCheck_Cpy;
        }

        public static Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> ListIntersection(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> DictToCheck, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> MyCurrentDict)
        {
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> InListElements = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();

            foreach (TimeSpan eachTimeSpan in DictToCheck.Keys)
            {
                if (MyCurrentDict.ContainsKey(eachTimeSpan))
                {
                    mTuple<int, TimeSpanWithStringID> MyCurrentDictTimeSpanWithStringID = new mTuple<int, TimeSpanWithStringID>(MyCurrentDict[eachTimeSpan].Item1, MyCurrentDict[eachTimeSpan].Item2);
                    mTuple<int, TimeSpanWithStringID> DictToCheckTimeSpanWithStringID = new mTuple<int, TimeSpanWithStringID>(DictToCheck[eachTimeSpan].Item1, DictToCheck[eachTimeSpan].Item2);
                    InListElements.Add(eachTimeSpan, (MyCurrentDictTimeSpanWithStringID.Item1 < DictToCheckTimeSpanWithStringID.Item1 ? MyCurrentDictTimeSpanWithStringID : DictToCheckTimeSpanWithStringID));
                }
            }
            return InListElements;
        }


        public static List<SubCalendarEvent> ListIntersection(List<SubCalendarEvent> ListToCheck, List<SubCalendarEvent> MyCurrentList)
        {
            List<SubCalendarEvent> InListElements = new List<SubCalendarEvent>();

            foreach (SubCalendarEvent MySubCalendarEvent0 in ListToCheck)
            {
                foreach (SubCalendarEvent MySubCalendarEvent1 in MyCurrentList)
                {
                    if (MySubCalendarEvent1.getId == MySubCalendarEvent0.getId)
                    {
                        InListElements.Add(MySubCalendarEvent1);
                    }
                }
            }
            return InListElements;
        }

        /// <summary>
        /// Function gets all the conflicting subbevents.
        /// It returns a tuple, 
        /// Item1 is a list of blobsubcalendarevents. Each blob is a group of subevents conflicting with each other
        /// Item2 is the none conflicting events in <paramref name="AllSubEvents"/>
        /// </summary>
        /// <param name="AllSubEvents"></param>
        /// <returns></returns>
        public static Tuple<List<BlobSubCalendarEvent>, HashSet<SubCalendarEvent>> getConflictingEvents(IEnumerable<SubCalendarEvent> AllSubEvents)
        {
            HashSet<SubCalendarEvent> dedupedSubEvents = new HashSet<SubCalendarEvent>(AllSubEvents);
            HashSet<SubCalendarEvent> nonConflict = new HashSet<SubCalendarEvent>(dedupedSubEvents);
            List<BlobSubCalendarEvent> conflictingBlob = new List<BlobSubCalendarEvent>();
            IEnumerable<SubCalendarEvent> orderedByStart = dedupedSubEvents.OrderBy(obj => obj.Start).ToList();
            List<SubCalendarEvent> AllSubEvents_List = orderedByStart.ToList();


            Dictionary<SubCalendarEvent, List<SubCalendarEvent>> subEventToConflicting = new Dictionary<SubCalendarEvent, List<SubCalendarEvent>>();


            for (int i = 0; i < AllSubEvents_List.Count && i >= 0; i++)
            {
                SubCalendarEvent refSubCalendarEvent = AllSubEvents_List[i];
                List<SubCalendarEvent> possibleInterferring = AllSubEvents_List.Where(obj => obj != refSubCalendarEvent).ToList();
                List<SubCalendarEvent> InterferringEvents = possibleInterferring.AsParallel().Where(obj => obj.StartToEnd.doesTimeLineInterfere(refSubCalendarEvent.StartToEnd)).ToList();
                if (InterferringEvents.Count() > 0)//this tries to select the rest of 
                {
                    bool conflictFound = false;
                    do
                    {
                        conflictFound = false;
                        AllSubEvents_List = AllSubEvents_List.Except(InterferringEvents).ToList();
                        DateTimeOffset LatestEndTime = InterferringEvents.Max(obj => obj.End);
                        TimeLine possibleInterferringTimeLine = new TimeLine(refSubCalendarEvent.Start, LatestEndTime);
                        foreach(SubCalendarEvent subEvent in AllSubEvents_List)
                        {
                            if(subEvent.StartToEnd.doesTimeLineInterfere(possibleInterferringTimeLine))
                            {
                                nonConflict.Remove(subEvent);
                                InterferringEvents.Add(subEvent);
                                conflictFound = true;
                            }
                        }
                    }
                    while (conflictFound);
                    --i;
                }
                if (InterferringEvents.Count > 0)
                {
                    conflictingBlob.Add(new BlobSubCalendarEvent(InterferringEvents));
                }
            }


            Tuple<List<BlobSubCalendarEvent>, HashSet<SubCalendarEvent>> retValue = new Tuple<List<BlobSubCalendarEvent>, HashSet<SubCalendarEvent>>(conflictingBlob, nonConflict);
            return retValue;
            //Continue from here Jerome you need to write the function for detecting conflicting events and then creating the interferring list.
        }


        public class ConflictEvaluation
        {
            IEnumerable<IDefinedRange> ConflctingDefinedRanges;
            IEnumerable<IDefinedRange> NonConflicting;
            public ConflictEvaluation(IEnumerable<IDefinedRange> elements)
            {
                Tuple<IEnumerable<IDefinedRange>, IEnumerable<IDefinedRange>> evaluation = getConflictingRangeElements(elements);
                NonConflicting = evaluation.Item1;
                ConflctingDefinedRanges = evaluation.Item2;
            }

            public ConflictEvaluation(IEnumerable<IDefinedRange> timeEvents, IDefinedRange timeLineFOrCheck): this(timeEvents.Where(timeline => timeLineFOrCheck.StartToEnd.doesTimeLineInterfere(timeline.StartToEnd)))
            {
                ;
            }

            /// <summary>
            /// Function computes all timelines that are conflicting and not conflicting. The firs
            /// </summary>
            /// <param name="elements"></param>
            /// <returns></returns>
            Tuple<IEnumerable<IDefinedRange>, IEnumerable<IDefinedRange>> getConflictingRangeElements(IEnumerable<IDefinedRange> timeLineElements)
            {
                List<IDefinedRange> zeroTimespan = new List<IDefinedRange>();
                List<IDefinedRange> elements = new List<IDefinedRange>();
                foreach(IDefinedRange eachRange in timeLineElements.OrderBy(obj => obj.Start))
                {
                    elements.Add(eachRange);
                }
                List<IDefinedRange> EventsWithTImeline = elements.ToList();
                List<TimeLine> retValue_ItemA = new List<TimeLine>();

                List<IDefinedRange> retValue_ItemB = elements.ToList();
                retValue_ItemB.Clear();//trying to make retValue_ItemB an empty collection with the same data type of AllSubEvents

                for (int i = 0; i < EventsWithTImeline.Count;)
                {
                    IDefinedRange refEvent = EventsWithTImeline[i];
                    EventsWithTImeline.Remove(refEvent);
                    IEnumerable<IDefinedRange> InterferringEvents = EventsWithTImeline.Where(obj => obj.StartToEnd.doesTimeLineInterfere(refEvent.StartToEnd));
                    bool AddrefTOretValue_ItemB = true;//flag will be set if refEvent is conflicitng
                    while (true && InterferringEvents.LongCount() > 0)
                    {
                        AddrefTOretValue_ItemB = false;
                        DateTimeOffset LowestInterferingStartTime = InterferringEvents.Select(obj => obj.Start).Min();
                        DateTimeOffset LatesInterferingEndTime = InterferringEvents.Select(obj => obj.End).Max();
                        DateTimeOffset refStartTIme = refEvent.Start <= LowestInterferingStartTime ? refEvent.Start : LowestInterferingStartTime;
                        DateTimeOffset refEndTIme = refEvent.End <= LatesInterferingEndTime ? LatesInterferingEndTime : refEvent.End;
                        TimeLine refTimeLineForInterferrers = new TimeLine(refStartTIme, refEndTIme);
                        EventsWithTImeline = EventsWithTImeline.Except(InterferringEvents).ToList();
                        IEnumerable<IDefinedRange> ExtraInterferringEvents = EventsWithTImeline.Where(obj => obj.StartToEnd.doesTimeLineInterfere (refTimeLineForInterferrers));
                        if (ExtraInterferringEvents.LongCount() < 1)
                        {
                            retValue_ItemA.Add(refTimeLineForInterferrers);
                            break;
                        }
                        else
                        {
                            InterferringEvents = InterferringEvents.Concat(ExtraInterferringEvents).ToList();
                        }
                    }
                    if (AddrefTOretValue_ItemB)
                    {
                        retValue_ItemB.Add(refEvent);
                    }
                }


                return new Tuple<IEnumerable<IDefinedRange>, IEnumerable<IDefinedRange>>(retValue_ItemA, retValue_ItemB);
            }

            public IEnumerable<IDefinedRange> ConflictingTimeRange {
                get
                {
                    return ConflctingDefinedRanges;
                }
            }

            public IEnumerable<IDefinedRange> NonConflictingTimeRange
            {
                get
                {
                    return NonConflicting;
                }

            }
        }


        private static long Factorial(int N)
        {
            long num = 1;
            for (int i = 1; i <= N; i++)
            {
                num *= i;
            }
            return num;
        }

        public static int[] generatePermutation(int[] OriginalPermutation, long CurrentIndex, int CurrentCycle, long NumberOfPermutation, int SizeOfArray, int boundSelect)
        {
            if (NumberOfPermutation == 1)
            {
                return OriginalPermutation;
            }

            long SizePerLevel = NumberOfPermutation / (SizeOfArray - CurrentCycle);
            if (boundSelect == 1)
            {
                CurrentCycle = boundSelect;
            }

            if (boundSelect == 2)
            {
                --SizeOfArray;
            }

            if (boundSelect == 3)
            {
                CurrentCycle = 1;
                --SizeOfArray;
            }

            for (; CurrentCycle < SizeOfArray; CurrentCycle++)
            {
                SizePerLevel = NumberOfPermutation / (SizeOfArray - CurrentCycle);
                int i = 0;
                for (; i * SizePerLevel <= CurrentIndex; i++)
                {
                    ;
                }

                --i;

                int myIndex = i + CurrentCycle;
                int tmp = OriginalPermutation[myIndex];
                int refIndex = CurrentCycle;// SizeOfArray - (CurrentCycle + 1);
                OriginalPermutation[myIndex] = OriginalPermutation[refIndex];
                OriginalPermutation[refIndex] = tmp;

                CurrentIndex = CurrentIndex - i * SizePerLevel;
                NumberOfPermutation = NumberOfPermutation / (SizeOfArray - CurrentCycle);
            }

            return OriginalPermutation;
        }

        public static SubCalendarEvent[] getBestPermutation(
            List<SubCalendarEvent> AllEvents,
            Tuple<Location, Location> BorderElements = null,
            double worstDistance = double.MinValue
            )
        {
            SubCalendarEvent[] retValue;

            if (AllEvents.Count <= 1)
            {
                return AllEvents.ToArray();
            }

            long numberOfpermutations = Factorial(AllEvents.Count);
            int permutationLimit = Int32.MaxValue / 16;

            System.Diagnostics.Stopwatch myWatch = new System.Diagnostics.Stopwatch();
            if (numberOfpermutations > permutationLimit)
            {
                if (worstDistance == double.MinValue)
                {
                    worstDistance = permutationLimit;
                }
                uint maxInterations = (uint)(numberOfpermutations / (permutationLimit));
                double[] distances = new double[maxInterations];

                SubCalendarEvent[][] retValues = new int[maxInterations].Select(obj => new SubCalendarEvent[0]).ToArray();

                Parallel.For(0, maxInterations, (i =>

                //for (int i = 0; i < maxInterations; i++ )
                {

                    SubCalendarEvent[] possibleOptimization = getBestPermutation(AllEvents, worstDistance, BorderElements);
                    MessageBox.Show("One set done");
                    double currentDistance = SubCalendarEvent.CalculateDistance(possibleOptimization);
                    retValues[i] = possibleOptimization;
                }
                ));

                for (int i = 0; i < maxInterations; i++)
                {
                    distances[i] = SubCalendarEvent.CalculateDistance(retValues[i]);
                }
                retValue = retValues[distances.MinIndex()];
            }

            else
            {
                if (worstDistance == double.MinValue)
                {
                    worstDistance = numberOfpermutations;
                }
                myWatch.Start();
                retValue = getBestPermutation(AllEvents, worstDistance, BorderElements);
            }
            myWatch.Stop();
            //MessageBox.Show("Ran for " + myWatch.ElapsedMilliseconds + "ms with " + numberOfpermutations + " permutations");

            return retValue;

        }
        public static SubCalendarEvent[] getBestPermutation(
            List<SubCalendarEvent> AllEvents,
            //double worstDistance,
            int permutationIndex,
            long permutationMax,
            Tuple<Location, Location> BorderElements = null
            )
        {
            int startingIndex = permutationIndex;
            int[] init_Array = new int[AllEvents.Count];

            for (int i = 0; i < init_Array.Length; i++)
            {
                init_Array[i] = i;
            }
            double worstDistance = double.MaxValue;
            double minValue = worstDistance;
            long minIndex = -1;

            double[] allFactorial = new double[permutationMax];

            //Parallel.For(0, numberOfpermutations, i =>

            for (; permutationIndex < permutationMax; permutationIndex++)
            {
                int[] myArray = generatePermutation(init_Array.ToArray(), permutationIndex, 0, permutationMax, init_Array.Length, 0);
                double totalDistance = worstDistance;
                List<SubCalendarEvent> myList = new List<SubCalendarEvent>();
                foreach (int eachInt in myArray)
                {
                    myList.Add(AllEvents[eachInt]);
                }
                //if (Utility.PinSubEventsToEnd(myList, restrictingTimeLine))
                {

                    totalDistance = SubCalendarEvent.CalculateDistance(myList, worstDistance);
                    if (BorderElements != null)
                    {
                        if (BorderElements.Item1 !=null && !BorderElements.Item1.isNull)
                        {
                            totalDistance += Location.calculateDistance(BorderElements.Item1, myList.First().Location);
                        }
                        if (BorderElements.Item2!=null && !BorderElements.Item2.isNull)
                        {
                            totalDistance += Location.calculateDistance(BorderElements.Item2, myList.Last().Location);
                        }
                    }
                    //Location_Elements.calculateDistance(myList.Select(obj => obj.myLocation).ToList());
                }
                allFactorial[permutationIndex - startingIndex] = totalDistance;
            }
            //);

            int lowestIndex = allFactorial.MinIndex() + startingIndex;
            int[] allIndex = generatePermutation(init_Array.ToArray(), lowestIndex, 0, permutationMax, init_Array.Length, 0);
            double lowestDist = worstDistance;
            List<SubCalendarEvent> bestOrder = new List<SubCalendarEvent>();
            foreach (int eachInt in allIndex)
            {
                bestOrder.Add(AllEvents[eachInt]);
            }

            return bestOrder.ToArray();

        }

        public static SubCalendarEvent[] getBestPermutation(List<SubCalendarEvent> AllEvents, double worstDistance, Tuple<Location, Location> BorderElements = null)
        {
            if (AllEvents.Count <= 1)
            {
                return AllEvents.ToArray();
            }
            const int maxFactorialCount = 11;
            int factorialLimiter = AllEvents.Count < maxFactorialCount ? AllEvents.Count : maxFactorialCount;
            long numberOfpermutations = Factorial(AllEvents.Count);
            int[] init_Array = new int[AllEvents.Count];
            double[] allFactorial = new double[Factorial(factorialLimiter)];
            double iniMinValue = double.MaxValue, minValue = double.MaxValue;

            long minIndex = -1;

            Parallel.For(0, allFactorial.Length, i =>
            {
                allFactorial[i] = worstDistance;
            });

            for (int i = 0; i < init_Array.Length; i++)
            {
                init_Array[i] = i;
            }
            long lengthOfLoop = allFactorial.LongLength;
            long LoopCounter = numberOfpermutations / lengthOfLoop;
            for (long j = 0; j < LoopCounter + 1; j++)
            {
                //Parallel.For(j * lengthOfLoop, numberOfpermutations, i =>
                for (long i = j * lengthOfLoop; i < numberOfpermutations; i++)
                {
                    int[] myArray = generatePermutation(init_Array.ToArray(), i, 0, numberOfpermutations, init_Array.Length, 0);
                    double totalDistance = worstDistance;
                    List<SubCalendarEvent> myList = new List<SubCalendarEvent>();
                    foreach (int eachInt in myArray)
                    {
                        myList.Add(AllEvents[eachInt]);
                    }
                    //if (Utility.PinSubEventsToEnd(myList, restrictingTimeLine))
                    {

                        totalDistance = SubCalendarEvent.CalculateDistance(myList, worstDistance);
                        if (BorderElements != null)
                        {
                            if (BorderElements.Item1!=null && !BorderElements.Item1.isNull)
                            {
                                totalDistance += Location.calculateDistance(BorderElements.Item1, myList.First().Location);
                            }
                            if (BorderElements.Item2 !=null && !BorderElements.Item2.isNull)
                            {
                                totalDistance += Location.calculateDistance(BorderElements.Item2, myList.Last().Location);
                            }
                        }
                        //Location_Elements.calculateDistance(myList.Select(obj => obj.myLocation).ToList());
                    }
                    long index = i % lengthOfLoop;
                    allFactorial[index] = totalDistance;

                }
                //);

                double currValue = allFactorial.Min();
                if (currValue < minValue)
                {
                    minValue = currValue;
                    minIndex = (j * lengthOfLoop) + allFactorial.MinIndex();
                }
                if (minValue == iniMinValue)
                {
                    minIndex = 0;
                }
            }

            int[] goodOrder = generatePermutation(init_Array.ToArray(), minIndex, 0, numberOfpermutations, init_Array.Length, 0);

            SubCalendarEvent[] ret_GoodOrder = new SubCalendarEvent[AllEvents.Count];
            for (int j = 0; j < AllEvents.Count; j++)
            {
                ret_GoodOrder[j] = AllEvents[goodOrder[j]];
            }


            return ret_GoodOrder;
        }

        public static Location[] getBestPermutation(List<Location> AllEvents, double worstDistance)
        {
            long numberOfpermutations = Factorial(AllEvents.Count);
            int[] init_Array = new int[AllEvents.Count];
            double[] allFactorial = new double[Factorial(11)];
            double minValue = worstDistance;
            long minIndex = -1;

            Parallel.For(0, allFactorial.Length, i =>
            {
                allFactorial[i] = worstDistance;
            });

            for (int i = 0; i < init_Array.Length; i++)
            {
                init_Array[i] = i;
            }
            long lengthOfLoop = allFactorial.LongLength;
            long LoopCounter = numberOfpermutations / lengthOfLoop;
            for (long j = 0; j < LoopCounter + 1; j++)
            {
                Parallel.For(j * lengthOfLoop, numberOfpermutations, i =>
                {
                    int[] myArray = generatePermutation(init_Array.ToArray(), i, 0, numberOfpermutations, init_Array.Length, 0);
                    double totalDistance = worstDistance;
                    List<Location> myList = new List<Location>();
                    foreach (int eachInt in myArray)
                    {
                        myList.Add(AllEvents[eachInt]);
                    }
                    //if (Utility.PinSubEventsToEnd(myList, restrictingTimeLine))
                    {
                        //totalDistance = SubCalendarEvent.CalculateDistance(myList, worstDistance);

                        totalDistance = Location.calculateDistance(myList);
                    }
                    allFactorial[i % lengthOfLoop] = totalDistance;

                }
                );

                double currValue = allFactorial.Min();
                if (currValue < minValue)
                {
                    minValue = currValue;
                    minIndex = (j * lengthOfLoop) + allFactorial.MinIndex();
                }
            }

            int[] goodOrder = generatePermutation(init_Array.ToArray(), minIndex, 0, numberOfpermutations, init_Array.Length, 0);

            Location[] ret_GoodOrder = new Location[AllEvents.Count];
            for (int j = 0; j < AllEvents.Count; j++)
            {
                ret_GoodOrder[j] = AllEvents[goodOrder[j]];
            }


            return ret_GoodOrder;
        }

        public static int MinIndex<T>(this IEnumerable<T> sequence) where T : IComparable<T>
        {
            int num = -1;
            T other = default(T);
            int num2 = 0;
            foreach (T local2 in sequence)
            {
                if ((local2.CompareTo(other) < 0) || (num == -1))
                {
                    num = num2;
                    other = local2;
                }
                num2++;
            }
            return num;
        }

        public static int MaxIndex<T>(this IEnumerable<T> sequence) where T : IComparable<T>
        {
            int num = -1;
            T other = default(T);
            int num2 = 0;
            foreach (T local2 in sequence)
            {
                if ((local2.CompareTo(other) > 0) || (num == -1))
                {
                    num = num2;
                    other = local2;
                }
                num2++;
            }
            return num;
        }

        public static int MinIndex(this IEnumerable<double> sequence)
        {
            int num = -1;
            double other = sequence.First();
            int num2 = 0;
            foreach (double local2 in sequence)
            {
                if (!double.IsNaN(local2) && ((local2 < other) || (num == -1)))
                {
                    num = num2;
                    other = local2;
                }
                num2++;
            }
            return num;
        }

        public static int MaxIndex(this IEnumerable<double> sequence)
        {
            int num = -1;
            double other = sequence.First();
            int num2 = 0;
            foreach (double local2 in sequence)
            {
                if (!double.IsNaN(local2) && ((local2 > other) || (num == -1)))
                {
                    num = num2;
                    other = local2;
                }
                num2++;
            }
            return num;
        }


        public static SubCalendarEvent getClosestSubCalendarEvent(IEnumerable<SubCalendarEvent> AllSubCalEvents, Location ReferenceSubEvent)
        {
            SubCalendarEvent RetValue = null;
            double shortestDistance = double.MaxValue;
            foreach (SubCalendarEvent eachSubCalendarEvent in AllSubCalEvents)
            {
                double DistanceSoFar = Location.calculateDistance(eachSubCalendarEvent.Location, ReferenceSubEvent);
                if (DistanceSoFar < shortestDistance)
                {
                    RetValue = eachSubCalendarEvent;
                    shortestDistance = DistanceSoFar;
                }
            }

            return RetValue;
        }

        public static List<T> InListAButNotInB<T>(List<T> ListA, List<T> ListB)
        {
            List<T> retValue = new List<T>();

            foreach (T eachT in ListA)
            {
                if (!(ListB.Contains(eachT)))
                {
                    retValue.Add(eachT);
                }
            }

            return retValue;
        }

        public static Tuple<int, List<List<Dictionary<T, mTuple<int, U>>>>> getHighestCountList<T, U>(List<List<Dictionary<T, mTuple<int, U>>>> PossibleMatches, List<Dictionary<T, mTuple<int, U>>> ConstrainedList)
        {
            int HighstSum = 0;
            List<List<Dictionary<T, mTuple<int, U>>>> retValue = new List<List<Dictionary<T, mTuple<int, U>>>>();
            //List<TimeLine> AllTimeLines = ConstrainedList.Keys.ToList();
            List<Dictionary<T, mTuple<int, U>>> ListOfDict = ConstrainedList;

            int j = 0;
            foreach (List<Dictionary<T, mTuple<int, U>>> eachList in PossibleMatches)
            {
                int CurrentSum = 0;
                int i = 0;

                foreach (Dictionary<T, mTuple<int, U>> eachDictionary in eachList)
                {
                    ICollection<KeyValuePair<T, mTuple<int, U>>> AllData = eachDictionary;
                    foreach (KeyValuePair<T, mTuple<int, U>> eachKeyValuePair in AllData)
                    {
                        int evpItem1 = 0;
                        if (ListOfDict[i].ContainsKey(eachKeyValuePair.Key))
                        {

                            int evpItem2 = ListOfDict[i][eachKeyValuePair.Key].Item1;
                            evpItem1 = eachKeyValuePair.Value.Item1 + evpItem2;
                        }
                        else
                        {
                            evpItem1 = eachKeyValuePair.Value.Item1;
                        }

                        evpItem1 = eachKeyValuePair.Value.Item1;
                        CurrentSum += evpItem1;
                    }
                    ++i;
                }

                if (CurrentSum >= HighstSum)
                {
                    retValue.Add(eachList);
                    if (CurrentSum > HighstSum)
                    {
                        retValue = new List<List<Dictionary<T, mTuple<int, U>>>>();
                        retValue.Add(eachList);
                    }
                    HighstSum = CurrentSum;
                }
                j++;
            }

            return new Tuple<int, List<List<Dictionary<T, mTuple<int, U>>>>>(HighstSum, retValue);
        }

        public static Dictionary<T, TimeLine> getTilerEventTimeline<T>(IEnumerable<T> tilerEvents) where T : TilerEvent
        {
            Dictionary<T, TimeLine> retValue = tilerEvents.ToDictionary(tilerEvent => tilerEvent, tilerEvent => tilerEvent.StartToEnd);
            return retValue;
        }

        static public bool tryPinSubEventsToStart(IEnumerable<SubCalendarEvent> arg1, TimeLine Arg2)
        {
            var dictOfSubEvents = getTilerEventTimeline(arg1);
            var retValue = PinSubEventsToStart(arg1.ToArray(), Arg2);

            if(!retValue)
            {
                foreach (var kvp in dictOfSubEvents)
                {
                    kvp.Key.shiftEvent(kvp.Value.Start);
                }
            }
            return retValue;
        }

        static public bool PinSubEventsToStart(IEnumerable<SubCalendarEvent> arg1, TimeLine Arg2)
        {
            return PinSubEventsToStart_NoEdit(arg1.ToArray(), Arg2);
        }

        static private bool PinSubEventsToStart_NoEdit(SubCalendarEvent[] arg1, TimeLine Arg2)
        {
            bool retValue = true;
            long length = arg1.LongLength;
            TimeLine refTimeline = Arg2.StartToEnd;
            for (int i = 0; i < length; i++)
            {
                if (arg1[i].PinToStart(refTimeline))
                {
                    ;
                }
                else
                {
                    retValue = false;
                    break;
                }
                refTimeline = new TimeLine(arg1[i].ActiveSlot.End, refTimeline.End);
            }

            return retValue;
        }

        /// <summary>
        /// This returns true or false if tile can be pin to end and will restore the time position of the tiles.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="Arg2"></param>
        /// <returns></returns>
        static public bool checkIfPinSubEventsToEnd(IEnumerable<SubCalendarEvent> arg1, TimeLine Arg2)
        {
            var dictOfSubEvents = arg1.ToDictionary(sub => sub, sub => sub.ActiveSlot.CreateCopy());
            var retValue = PinSubEventsToEnd(arg1.ToArray(), Arg2);

            foreach (var kvp in dictOfSubEvents)
            {
                kvp.Key.shiftEvent(kvp.Value.Start);
            }
            
            return retValue;
        }


        static public bool tryPinSubEventsToEnd(IEnumerable<SubCalendarEvent> arg1, TimeLine Arg2)
        {
            var dictOfSubEvents = arg1.ToDictionary(sub => sub, sub => sub.ActiveSlot.CreateCopy());
            var retValue = PinSubEventsToEnd(arg1.ToArray(), Arg2);

            if (!retValue)
            {
                foreach (var kvp in dictOfSubEvents)
                {
                    kvp.Key.shiftEvent(kvp.Value.Start);
                }
            }
            return retValue;
        }

        /// <summary>
        /// Pin Sub Events to the end of the TimeLine Each SubCalendarEvent Stays within the confines of its Calendar Event. The pinning starts from the last SubCalevent in the list
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="Arg2"></param>
        /// <returns></returns>
        static public bool PinSubEventsToEnd(IEnumerable<SubCalendarEvent> arg1, TimeLine Arg2)
        {
            return PinSubEventsToEnd_NoEdit(arg1.ToArray(), Arg2).Item1;
        }


        static internal Tuple<bool, int> PinSubEventsToEnd_NoEdit(SubCalendarEvent[] arg1, TimeLine Arg2)
        {
            bool successfulPin = true;
            int successFullPinCount = 0;
            long length = arg1.LongLength;
            TimeLine refTimeline = Arg2.StartToEnd;
            SubCalendarEvent refEvent;
            for (long i = length - 1; i >= 0; i--)
            {//hack notice you need to ensure that each subcalevent can fit within the timeline. YOu need a way to resolve this if not possible
                refEvent = arg1[i];
                if (refEvent.PinToEnd(refTimeline))
                {
                    successFullPinCount++;
                }
                else
                {
                    successfulPin = false;
                    break;
                }
                refTimeline = new TimeLine(refTimeline.Start, refEvent.ActiveSlot.Start);
            }

            Tuple<bool, int> retValue = new Tuple<bool, int>(successfulPin, successFullPinCount);
            return retValue;
        }


        static public List<List<T>> SerializeList<T>(List<List<List<T>>> Arg1)
        {
            List<List<T>> retValue = new List<List<T>>();
            int i = 0;
            for (; i < Arg1.Count; i++)
            {
                retValue = SerializeList(retValue, Arg1[i].ToList());
            }

            return retValue;
        }


        static private List<List<T>> SerializeList<T>(List<List<T>> Arg1, List<List<T>> Arg2)
        {
            /*
             * This Takes two List Arguements. Each List objection in Arg1 (the inner List object in Arg1) , Lets call it ListX, generates another List that appends each List in arg2 to ListX
             * e.g Arg1= {List0,List1,List2,List3}, Arg2= {ListA,ListB,ListC,ListD}
             * retValue Will Have {List0+ListA,List0+ListB,List0+ListC,List1+ListA,List1+ListB,List1+ListC,   List2+ListA,List2+ListB,List2+ListC}
             */


            List<List<T>> retValue = new List<List<T>>();

            if (Arg1.Count < 1)
            {
                //var Arg3 = retValue.Concat(Arg2);
                retValue = Arg2;//
                return retValue;
            }
            if (Arg2.Count < 1)
            {
                retValue = Arg1;//
                return retValue;
            }

            foreach (List<T> eachList in Arg1)
            {


                foreach (List<T> eachList0 in Arg2)
                {
                    List<T> eachList_cpy = eachList.ToList();
                    eachList_cpy.AddRange(eachList0);
                    retValue.Add(eachList_cpy);
                }



            }

            return retValue;
        }

        public static TimeSpan SumOfActiveDuration(IEnumerable<SubCalendarEvent> ListOfSubCalEvent)
        {
            TimeSpan retValue = new TimeSpan(0);

            //retValue=ListOfSubCalEvent.Sum(obj => obj.ActiveDuration);
            foreach (SubCalendarEvent eachSubCalendarEvent in ListOfSubCalEvent)
            {
                retValue += eachSubCalendarEvent.getActiveDuration;
            }
            return retValue;
        }


        /*public double calculateDistance(SubCalendarEvent SubCalendarEventA, SubCalendarEvent SubCalendarEventB, ref Dictionary<string, List<Double>> DistanceMatrix)
        { 
            
        }*/


        public static mTuple<bool, List<TimeSpanWithStringID>> isViolatingTimeSpanString(Dictionary<string, mTuple<int, TimeSpanWithStringID>> GuardList, List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> PotentialError_list)
        {
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> recompiledListToMatch_GuardList = new Dictionary<string, mTuple<int, TimeSpanWithStringID>>();

            mTuple<bool, List<TimeSpanWithStringID>> retValue = new mTuple<bool, List<TimeSpanWithStringID>>(false, new List<TimeSpanWithStringID>());
            foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in PotentialError_list)
            {
                foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in eachDictionary)
                {
                    if (recompiledListToMatch_GuardList.ContainsKey(eachKeyValuePair.Key))
                    {
                        recompiledListToMatch_GuardList[eachKeyValuePair.Key].Item1 += eachKeyValuePair.Value.Item1;
                    }
                    else
                    {
                        recompiledListToMatch_GuardList.Add(eachKeyValuePair.Key, new mTuple<int, TimeSpanWithStringID>(eachKeyValuePair.Value.Item1, eachKeyValuePair.Value.Item2));
                    }
                }
            }

            foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in recompiledListToMatch_GuardList)
            {
                if (GuardList.ContainsKey(eachKeyValuePair.Key))
                {
                    if (GuardList[eachKeyValuePair.Key].Item1 < eachKeyValuePair.Value.Item1)
                    {
                        retValue.Item1 = true;
                        retValue.Item2.Add(eachKeyValuePair.Value.Item2);
                    }
                }
                else
                {
                    retValue.Item1 = true;
                    retValue.Item2.Add(eachKeyValuePair.Value.Item2);
                }
            }

            return retValue;


        }


        static public IEnumerable<T> serialLizeMtuple_MTupleToVarT<T>(IEnumerable<mTuple<int, T>> EnunerableData)
        {
            List<T> retValue = new List<T>();
            foreach (mTuple<int, T> eachmTuple in EnunerableData)
            {
                int i = 0;
                for (; i < eachmTuple.Item1; i++)
                {
                    retValue.Add(eachmTuple.Item2);
                }
            }

            return retValue;
        }

        static public List<SubCalendarEvent> mTupleToSubCalEvents(List<mTuple<bool, SubCalendarEvent>> ListOfMTuples)
        {
            List<SubCalendarEvent> retValue = new System.Collections.Generic.List<SubCalendarEvent>();
            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in ListOfMTuples)
            {
                retValue.Add(eachmTuple.Item2);
            }

            return retValue;
        }

        static public List<mTuple<T, SubCalendarEvent>> SubCalEventsTomTuple<T>(IEnumerable<SubCalendarEvent> ListOfMTuples, T DefaultValue)
        {
            List<mTuple<T, SubCalendarEvent>> retValue = new System.Collections.Generic.List<mTuple<T, SubCalendarEvent>>();
            foreach (SubCalendarEvent eachmSubCalendarEvent in ListOfMTuples)
            {
                retValue.Add(new mTuple<T, SubCalendarEvent>(DefaultValue, eachmSubCalendarEvent));
            }

            return retValue;
        }


        static public List<mTuple<T, R>> mTupleTomTuple<T, U, R>(List<mTuple<U, R>> ListOfMTuples, T DefaultValue)
        {
            List<mTuple<T, R>> retValue = new System.Collections.Generic.List<mTuple<T, R>>();
            foreach (mTuple<U, R> eachmSubCalendarEvent in ListOfMTuples)
            {
                retValue.Add(new mTuple<T, R>(DefaultValue, eachmSubCalendarEvent.Item2));
            }

            return retValue;
        }

        public static List<object> dedupeAndPreserveListOrder(IEnumerable<object> entryList)
        {
            List<object> retValue = new List<object>();
            HashSet<object> dedupData = new HashSet<object>();
            foreach (object data in entryList)
            {
                if (!dedupData.Contains(data))
                {
                    retValue.Add(data);
                    dedupData.Add(data);
                }
            }
            return retValue;
        }

        /// <summary>
        /// function takes subEvents and generates the max timeline before and after each subevent. It tries to evaluate this max before and after by manipulating the pin to start and pin to end of each sub event preceding it.
        /// </summary>
        /// <param name="maxTImeLine"> The restricting timeline for which to evealutate the maximum timeline for pinning.</param>
        /// <param name="subEvents"></param>
        /// <returns></returns>
        public static Dictionary<SubCalendarEvent, nTuple<TimeLine, TimeLine, TimeLine>> subEventToMaxSpaceAvailable(TimeLine maxTImeLine, IEnumerable<SubCalendarEvent> subEvents)
        {
            Dictionary<EventID, SubCalendarEvent> eventIdToSubEvent = subEvents.ToDictionary(subEvent => subEvent.SubEvent_ID, subEvent => subEvent);
            Dictionary<EventID, DateTimeOffset> startTimes = new Dictionary<EventID, DateTimeOffset>();
            Dictionary<EventID, DateTimeOffset> endTimes = new Dictionary<EventID, DateTimeOffset>();
            List<SubCalendarEvent> ordedsubEvents = subEvents.Select(subEvent => subEvent.CreateCopy(subEvent.SubEvent_ID, subEvent.ParentCalendarEvent)).ToList();// the ordered passed in is preserved. We don't care about the time frame
            Dictionary<SubCalendarEvent, nTuple<TimeLine, TimeLine, TimeLine>> retValue = new Dictionary<SubCalendarEvent, nTuple<TimeLine, TimeLine, TimeLine>>();
            TimeLine timeLine = new TimeLine();
            DateTimeOffset start = maxTImeLine.Start;
            DateTimeOffset end = maxTImeLine.End;
            DateTimeOffset startBefore = maxTImeLine.Start;
            DateTimeOffset endBefore = maxTImeLine.End;
            DateTimeOffset startAfter = maxTImeLine.Start;
            DateTimeOffset endAfter = maxTImeLine.End;
            List<SubCalendarEvent> pinnedStartingFromLast = new List<SubCalendarEvent>();


            TimeLine iterationTImeLine = new TimeLine(start, end);
            TimeLine timeLineBefore = new TimeLine();
            TimeLine timeLineAfter = new TimeLine();
            TimeLine timeLineWithoutSubEvent = new TimeLine();
            if (Utility.tryPinSubEventsToStart(ordedsubEvents, maxTImeLine))
            {
                SubCalendarEvent subEvent;
                int i = ordedsubEvents.Count - 1;
                for (; i > 0; i--)
                {
                    int j = i - 1;
                    subEvent = ordedsubEvents[i];
                    pinnedStartingFromLast.Insert(0, subEvent);
                    startAfter = subEvent.End;
                    timeLineAfter = new TimeLine(startAfter, endAfter);
                    startBefore = ordedsubEvents[j].End;
                    Utility.PinSubEventsToEnd(pinnedStartingFromLast, maxTImeLine);
                    endAfter = subEvent.Start;
                    endBefore = ordedsubEvents[i].Start;
                    timeLineBefore = new TimeLine(startBefore, endBefore);
                    timeLineWithoutSubEvent = new TimeLine(timeLineBefore.Start, timeLineAfter.End);
                    retValue.Add(eventIdToSubEvent[subEvent.SubEvent_ID], new nTuple<TimeLine, TimeLine, TimeLine>(timeLineBefore, timeLineAfter, timeLineWithoutSubEvent));
                }

                subEvent = ordedsubEvents[i];
                pinnedStartingFromLast.Insert(0, subEvent);
                startAfter = subEvent.End;
                timeLineAfter = new TimeLine(startAfter, endAfter);
                startBefore = maxTImeLine.Start;
                Utility.PinSubEventsToEnd(pinnedStartingFromLast, maxTImeLine);
                endAfter = subEvent.Start;
                endBefore = ordedsubEvents[i].Start;
                timeLineBefore = new TimeLine(startBefore, endBefore);
                timeLineWithoutSubEvent = new TimeLine(timeLineBefore.Start, timeLineAfter.End);
                retValue.Add(eventIdToSubEvent[subEvent.SubEvent_ID], new nTuple<TimeLine, TimeLine, TimeLine>(timeLineBefore, timeLineAfter, timeLineWithoutSubEvent));
            }
            else
            {
                throw new Exception("There is a problem pinning the first initial bunch of elements in subEventToMaxSpaceAvailable");
            }

            return retValue;
        }
    
        
        /// <summary>
        /// Removes the seconds and milliseconds of the datetimeoffset account. It returns a new datetimeoffset struct object.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTimeOffset removeSecondsAndMilliseconds(this DateTimeOffset time)
        {
            DateTimeOffset retValue = new DateTimeOffset(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0, time.Offset);
            return retValue;
        }

        public static TimeSpan removeSecondsAndMilliseconds(this TimeSpan timeSpan)
        {
            TimeSpan retValue = new TimeSpan(timeSpan.Hours, timeSpan.Minutes, 0);
            return retValue;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        /// <summary>
        /// got from http://stackoverflow.com/questions/16100/how-do-i-convert-a-string-to-an-enum-in-c
        /// Used to parse string to enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
        public static ulong toJSMilliseconds(this DateTimeOffset time)
        {
            ulong retValue = (ulong)(time - ReferenceNow.StartOfTimeUTC).TotalMilliseconds;
            return retValue;
        }

        public static ulong toJSMilliseconds(this DateTime time)
        {
            ulong retValue = (ulong)(time - ReferenceNow.StartOfTimeUTC).TotalMilliseconds;
            return retValue;
        }


        public static bool isBeginningOfTime(this DateTimeOffset time)
        {
            return time == BeginningOfTime;
        }

        public static bool isBeginningOfJsTime(this DateTimeOffset time)
        {
            return time == JSStartTime;
        }

        //https://stackoverflow.com/questions/3141692/standard-deviation-of-generic-list
        public static double StandardDeviation(this IEnumerable<double> values)
        {
            double standardDeviation = 0;

            if (values.Any())
            {
                // Compute the average.     
                double avg = values.Average();

                // Perform the Sum of (value-avg)_2_2.      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));

                // Put it all together.      
                standardDeviation = Math.Sqrt((sum) / (values.Count() - 1));
            }

            return standardDeviation;
        }

        public static List<double> EvaluateTimeLines(IEnumerable<TimelineWithSubcalendarEvents> timeLines, TilerEvent tilerEvent)
        {
            List<IList<double>> multiDimensionalCalculation = new List<IList<double>>();
            List<TimelineWithSubcalendarEvents> validTimeLine = timeLines.Select(timeLine => {
                if(tilerEvent!=null)
                {
                    if (timeLine.doesTimeLineInterfere(tilerEvent.StartToEnd))
                    {
                        return timeLine;
                    }
                    else
                    {
                        return null;
                    }
                } else
                {
                    return timeLine;
                }
                
            }).ToList();
            TimeSpan totalAvailableSpan = TimeSpan.FromTicks(timeLines.Sum(timeLine => timeLine.TimelineSpan.Ticks));

            foreach (TimelineWithSubcalendarEvents timeline in validTimeLine)
            {
                if (timeline != null)
                {
                    double occupancy = (double)timeline.Occupancy;
                    
                    IList<double> dimensionsPerDay = new List<double>() { occupancy };
                    if(tilerEvent !=null)
                    {
                        List<TimeLine> interferringTImeLines = tilerEvent.getInterferringWithTimeLine(timeline);
                        TimeSpan totalInterferringSpan = TimeSpan.FromTicks(interferringTImeLines.Sum(objTimeLine => objTimeLine.TimelineSpan.Ticks));
                        double availableSpanRatio = (double)totalInterferringSpan.Ticks / totalAvailableSpan.Ticks;
                        double distance = Location.calculateDistance(timeline.averageLocation, tilerEvent.Location, 0);
                        double tickRatio = (double)tilerEvent.getActiveDuration.Ticks / totalInterferringSpan.Ticks;
                        dimensionsPerDay.Add(distance);
                        dimensionsPerDay.Add(tickRatio);
                        dimensionsPerDay.Add(availableSpanRatio);
                    }
                    multiDimensionalCalculation.Add(dimensionsPerDay);
                }
                else
                {
                    multiDimensionalCalculation.Add(null);
                }
            }
            var NotNullMultidimenstionValues = multiDimensionalCalculation.Where(obj => obj != null).ToList();
            List<double> foundIndexes = Utility.multiDimensionCalculationNormalize(NotNullMultidimenstionValues);
            List<double> retValue = new List<double>();
            int notNullCounter = 0;
            foreach (var coordinates in multiDimensionalCalculation)
            {
                if (coordinates != null)
                {
                    retValue.Add(foundIndexes[notNullCounter++]);
                }
                else
                {
                    retValue.Add(double.NaN);
                }
            }
            return retValue;
        }

        public static void Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }

        public static DateTimeOffset getClosestDayOfWeekAfterTime(this DateTimeOffset currentTime, DayOfWeek dayOfWeek)
        {
            int dayOfWeekIndex = (int)currentTime.DayOfWeek;
            int desiredDayOfWeek = (int)dayOfWeek;


            int dayDiff = ((desiredDayOfWeek - dayOfWeekIndex) + 7) % 7;

            DateTimeOffset retValue = currentTime.AddDays(dayDiff);

            return retValue;
        }

        static public DateTimeOffset MiddleTime(IDefinedRange timeLine)
        {
            DateTimeOffset retValue;
            TimeSpan timeSpan = timeLine.End - timeLine.Start;
            if(timeSpan.Ticks > 0)
            {
                retValue = timeLine.Start.Add(TimeSpan.FromSeconds(timeSpan.TotalSeconds / 2));
                retValue = retValue.removeSecondsAndMilliseconds();
            } else
            {
                retValue = timeLine.Start;
            }

            return retValue;
        }
        public static DateTimeOffset ParseTime(string timeString)
        {
            string preParsingString = timeString;
            if (!timeString.Contains("M +"))
            {
                preParsingString = preParsingString + " +00:00";
            }

            DateTimeOffset retValue = DateTimeOffset.Parse(preParsingString).ToLocalTime();
            return retValue;
        }

        public static DateTimeOffset toTimeZoneTime(this DateTimeOffset localDate)
        {
            if (!string.IsNullOrEmpty(Utility.timeZoneString) && !string.IsNullOrWhiteSpace(Utility.timeZoneString))
            {
                DateTimeZone userTimeZone = DateTimeZoneProviders.Tzdb[Utility.timeZoneString];
                //DateTimeOffset localDate = DateTimeOffset.Parse(this._EndfOfDayString).removeSecondsAndMilliseconds();
                LocalDateTime time = new LocalDateTime(localDate.Year, localDate.Month, localDate.Day, localDate.Hour, localDate.Minute);
                DateTimeOffset retValue = Instant.FromDateTimeOffset(localDate)
                      .InZone(userTimeZone)
                      .ToDateTimeUnspecified();
                return retValue;
            }
            else
            {
                return localDate;
            }
            
        }

        public static bool isGuid (this string input)
        {
            Guid guidOut;
            bool retValue = Guid.TryParse(input, out guidOut);
            return retValue;
        }

        public static bool isInt(this string input)
        {
            Int64 convertedOutput;
            bool retValue = Int64.TryParse(input, out convertedOutput);
            return retValue;
        }

        public static bool isNot_NullEmptyOrWhiteSpace(this string input)
        {
            bool retValue = !(string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input));
            return retValue;
        }


        public static int getBestPosition(TimeLine timeLine, SubCalendarEvent subEvent, IEnumerable<SubCalendarEvent> orderedAscendingByTimeCurrentSubEvents, HashSet<int> unusableIndexes = null, Tuple<Location, Location> BorderElements = null)
        {
            Dictionary<SubCalendarEvent, mTuple<SubCalendarEvent, int>> subeventToIndex = orderedAscendingByTimeCurrentSubEvents.Select((obj, index) => new mTuple<SubCalendarEvent, int>(obj, index)).ToDictionary(obj => obj.Item1, obj => obj);
            List<SubCalendarEvent> allSubEvents = orderedAscendingByTimeCurrentSubEvents.ToList();
            List<TimeLine> timeLineWorks = new List<TimeLine>();
            if (unusableIndexes == null)
            {
                unusableIndexes = new HashSet<int>();
            }

            HashSet<int> int_unusableIndex = new HashSet<int>(unusableIndexes);

            bool foundViableTimeLine = false;
            if (allSubEvents.Count > 0)
            {
                bool foundViableFromPreceding = false;
                var subEventToViableTimeLine = Utility.subEventToMaxSpaceAvailable(timeLine, allSubEvents);
                for (int i = 0; i < allSubEvents.Count; i++)
                {
                    var currentActiveSubEvent = allSubEvents[i];
                    var beforeAfterTimeLines = subEventToViableTimeLine[currentActiveSubEvent];
                    if (!foundViableFromPreceding)
                    {
                        var possibleBeforeTimeLine = subEvent.getTimeLinesInterferringWithCalculationRange(beforeAfterTimeLines.Item1) ?? new List<TimeLine>();
                        List<TimeLine> viableBeforeTImeLines = new List<TimeLine>();
                        foreach (var viableTimeLine in possibleBeforeTimeLine.Where(subTimeLine => subEvent.canExistWithinTimeLine(subTimeLine)))
                        {
                            foundViableFromPreceding = true;
                            foundViableTimeLine = true;
                            viableBeforeTImeLines.AddRange(subEvent.getTimeLinesInterferringWithCalculationRange(viableTimeLine));
                        }
                        if (viableBeforeTImeLines.Count < 1)
                        {
                            unusableIndexes.Add(i);
                        }
                    }


                    var possibleAfterTimeLine = subEvent.getTimeLinesInterferringWithCalculationRange(beforeAfterTimeLines.Item2) ?? new List<TimeLine>();
                    List<TimeLine> viableAterTImeLines = new List<TimeLine>();
                    foreach (var viableTimeLine in possibleAfterTimeLine.Where(subTimeLine => subEvent.canExistWithinTimeLine(subTimeLine)))
                    {
                        viableAterTImeLines.AddRange(subEvent.getTimeLinesInterferringWithCalculationRange(viableTimeLine));
                        foundViableFromPreceding = true;
                        foundViableTimeLine = true;
                    }
                    if (viableAterTImeLines.Count < 1)
                    {
                        unusableIndexes.Add(i + 1);
                        foundViableFromPreceding = false;
                    }
                }
                
                timeLineWorks = subEvent.getTimeLinesInterferringWithCalculationRange(timeLine);
            }
            else
            {
                if(subEvent.canExistWithinTimeLine(timeLine))
                {
                    foundViableTimeLine = true;
                    timeLineWorks = subEvent.getTimeLinesInterferringWithCalculationRange(timeLine);
                }
            }

            if (!foundViableTimeLine)
            {
                return -1;
            }
            List<SubCalendarEvent> fullSublist = allSubEvents.ToList();
            int retValue = -1;
            if (fullSublist.Count > 0)
            {
                retValue = getBestPosition(subEvent, fullSublist, unusableIndexes, BorderElements);
            }
            else if (fullSublist.Count == 0 && (unusableIndexes == null || !unusableIndexes.Contains(0)))
            {
                retValue = 0;
            }

            return retValue;
        }


        public static int getBestPosition(SubCalendarEvent SubEvent, IEnumerable<SubCalendarEvent> orderedAscendingCurrentSubEvents, HashSet<int> unusableIndexes = null, Tuple<Location, Location> BorderElements = null)
        {
            int i = 0;
            int RetValue = -1;
            if (unusableIndexes == null)
            {
                unusableIndexes = new HashSet<int>();
            }
            if (unusableIndexes.Count < orderedAscendingCurrentSubEvents.Count() + 1)
            {
                int currentCount = orderedAscendingCurrentSubEvents.Count();
                List<SubCalendarEvent> FullList = orderedAscendingCurrentSubEvents.ToList();
                double[] TotalDistances = new double[currentCount + 1];
                int countLimit = TotalDistances.Count();
                foreach (int index in unusableIndexes)
                {
                    if (index < countLimit && index >= 0)
                    {
                        TotalDistances[index] = double.MaxValue;
                    }

                }

                double worstValue = double.MaxValue / (orderedAscendingCurrentSubEvents.Count() + 2);
                for (; i <= currentCount; i++)
                {
                    if (!unusableIndexes.Contains(i))
                    {
                        List<SubCalendarEvent> FullList_Copy = FullList.ToList();
                        FullList_Copy.Insert(i, SubEvent);
                        Location firstBorderLocation = BorderElements?.Item1;
                        Location secondBorderLocation = BorderElements?.Item2;
                        double TotalDistance = SubCalendarEvent.CalculateDistance(FullList_Copy, 0, useFibonnacci: false);
                        if (firstBorderLocation != null)
                        {
                            double sumResult = Location.calculateDistance(FullList_Copy.First().Location, firstBorderLocation, worstValue);
                            if(sumResult != worstValue)
                            {
                                TotalDistance += sumResult;
                            }   
                        }

                        if (secondBorderLocation != null)
                        {
                            double sumResult = Location.calculateDistance(FullList_Copy.Last().Location, secondBorderLocation, worstValue);
                            if (sumResult != worstValue)
                            {
                                TotalDistance += sumResult;
                            }
                            
                        }
                        TotalDistances[i] = TotalDistance;
                    }

                }

                RetValue = TotalDistances.MinIndex();
                if (unusableIndexes.Contains(RetValue))
                {
                    RetValue = -1;
                }

            }
            return RetValue;
        }

        public static List<TilerEvent>order(IEnumerable<TilerEvent> tilerEvents)
        {
            return tilerEvents.OrderBy(o => o.Start).ToList();
        }

        public static List<EventName>names(IEnumerable<TilerEvent> tilerEvents)
        {
            return tilerEvents.Select(o => o.getName).ToList();
        }
    }
}
