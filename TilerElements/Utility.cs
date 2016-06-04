using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TilerElements;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TilerElements
{
    public static class Utility
    {
        public static DateTimeOffset StartOfTime = new DateTimeOffset();
        const uint fibonnaciLimit = 150;
        static uint[] fibonacciValues = new uint[fibonnaciLimit];
        public static DateTimeOffset JSStartTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan());
        public static DateTimeOffset BeginningOfTime = new DateTimeOffset();
        static Utility()
        {
            initializeFibonacci();
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
                return fibonacciValues[fibonnaciLimit];
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

        public static TimeLine CentralizeYourSelfWithinRange(TimeLine Range, TimeSpan Centralized)
        {
            TimeSpan Difference = Range.TimelineSpan - Centralized;
            TimeLine CentralizedTimeline = new TimeLine();
            if (Difference.TotalMilliseconds < 0)
            {
                throw (new System.Exception("Cannot generate CentralizeYourSelfWithinRange TimeLine Because Difference is less than zero.\nWill Not Fit!!!"));
            }
            DateTimeOffset MyStart = Range.Start.AddSeconds(Difference.TotalSeconds / 2);
            CentralizedTimeline = new TimeLine(MyStart, MyStart.Add(Centralized));
            return CentralizedTimeline;
        }

        static public List<double> getOriginFromDimensions(IList<IList<double>> collection)
        {
            IList<double> firstDataSet = collection.First();
            int lengthOfEachDataset = firstDataSet.Count;
            List<double> summingArray = (new double[lengthOfEachDataset]).Select(obj => 0.0).ToList();
            foreach (IList<double> eachDataSet in collection)
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


        static public List<double> multiDimensionCalculationNormalize(IList<IList<double>> collection, List<double> origin = null, IList<double> normalizedFields = null)
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
            //double[] maxIndexes;
            if (normalizedFields == null)
            {
                normalizedFields = new double[length];
                //maxIndexes = normalizedFields.ToArray();
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
                    if (MySubCalendarEvent1.ID == MySubCalendarEvent0.ID)
                    {
                        InListElements.Add(MySubCalendarEvent1);
                    }
                }
            }
            return InListElements;
        }


        public static List<BlobSubCalendarEvent> getConflictingEvents(IEnumerable<SubCalendarEvent> AllSubEvents)
        {
            List<BlobSubCalendarEvent> retValue = new List<BlobSubCalendarEvent>();
            IEnumerable<SubCalendarEvent> orderedByStart = AllSubEvents.OrderBy(obj => obj.Start).ToList();
            List<SubCalendarEvent> AllSubEvents_List = orderedByStart.ToList();


            Dictionary<SubCalendarEvent, List<SubCalendarEvent>> subEventToConflicting = new Dictionary<SubCalendarEvent, List<SubCalendarEvent>>();


            for (int i = 0; i < AllSubEvents_List.Count && i >= 0; i++)
            {
                SubCalendarEvent refSubCalendarEvent = AllSubEvents_List[i];
                List<SubCalendarEvent> possibleInterferring = AllSubEvents_List.Where(obj => obj != refSubCalendarEvent).ToList();
                List<SubCalendarEvent> InterferringEvents = possibleInterferring.AsParallel().Where(obj => obj.RangeTimeLine.InterferringTimeLine(refSubCalendarEvent.RangeTimeLine) != null).ToList();
                if (InterferringEvents.Count() > 0)//this tries to select the rest of 
                {
                    List<SubCalendarEvent> ExtraInterferringEVents = new List<SubCalendarEvent>();
                    do
                    {
                        AllSubEvents_List = AllSubEvents_List.Except(InterferringEvents).ToList();
                        DateTimeOffset LatestEndTime = InterferringEvents.Max(obj => obj.End);
                        TimeLine possibleInterferringTimeLine = new TimeLine(refSubCalendarEvent.Start, LatestEndTime);
                        ExtraInterferringEVents = AllSubEvents_List.AsParallel().Where(obj => obj.RangeTimeLine.InterferringTimeLine(possibleInterferringTimeLine) != null).ToList();
                        InterferringEvents = InterferringEvents.Concat(ExtraInterferringEVents).ToList();
                    }
                    while (ExtraInterferringEVents.Count > 0);
                    --i;
                }
                if (InterferringEvents.Count > 0)
                {
                    retValue.Add(new BlobSubCalendarEvent(InterferringEvents));
                }
            }

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

            /// <summary>
            /// Function computes all timelines that are conflicting and not conflicting. The firs
            /// </summary>
            /// <param name="elements"></param>
            /// <returns></returns>
            Tuple<IEnumerable<IDefinedRange>, IEnumerable<IDefinedRange>> getConflictingRangeElements(IEnumerable<IDefinedRange> elements)
            {
                elements = elements.OrderBy(obj => obj.Start);
                List<IDefinedRange> EventsWithTImeline = elements.ToList();
                List<TimeLine> retValue_ItemA = new List<TimeLine>();

                List<IDefinedRange> retValue_ItemB = elements.ToList();
                retValue_ItemB.Clear();//trying to make retValue_ItemB an empty collection with the same data type of AllSubEvents

                for (int i = 0; i < EventsWithTImeline.Count;)
                {
                    IDefinedRange refEvent = EventsWithTImeline[i];
                    EventsWithTImeline.Remove(refEvent);
                    IEnumerable<IDefinedRange> InterferringEvents = EventsWithTImeline.Where(obj => obj.RangeTimeLine.doesTimeLineInterfere(refEvent.RangeTimeLine));
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
                        IEnumerable<IDefinedRange> ExtraInterferringEvents = EventsWithTImeline.Where(obj => obj.RangeTimeLine.InterferringTimeLine(refTimeLineForInterferrers) != null);
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


        /*public static List<SubCalendarEvent> ListIntersection(List<SubCalendarEvent> ListToCheck, List<SubCalendarEvent> MyCurrentList)
        {
            List<SubCalendarEvent> InListElements = new List<SubCalendarEvent>();

            foreach (SubCalendarEvent MySubCalendarEvent0 in ListToCheck)
            {
                foreach (SubCalendarEvent MySubCalendarEvent1 in MyCurrentList)
                {
                    if (MySubCalendarEvent1.ID == MySubCalendarEvent0.ID)
                    {
                        InListElements.Add(MySubCalendarEvent1);
                    }
                }
            }
            return InListElements;
        }*/

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


        /*
        public static int[] generatePermutation(int[] OriginalPermutation, long CurrentIndex, long CurrentCycle, long NumberOfPermutation, int SizeOfArray, int boundSelect)
        {
            long num = NumberOfPermutation / (SizeOfArray - CurrentCycle);
            if (boundSelect == 1)
            {
                CurrentCycle = boundSelect;
            }
            if (boundSelect == 2)
            {
                SizeOfArray--;
            }
            if (boundSelect == 3)
            {
                CurrentCycle = 1;
                SizeOfArray--;
            }
            while (CurrentCycle < SizeOfArray)
            {
                num = NumberOfPermutation / (SizeOfArray - CurrentCycle);
                long num2 = 0;
                while ((num2 * num) <= CurrentIndex)
                {
                    num2++;
                }
                num2--;
                long index = num2 + CurrentCycle;
                int num4 = OriginalPermutation[index];
                long num5 = CurrentCycle;
                OriginalPermutation[index] = OriginalPermutation[num5];
                OriginalPermutation[num5] = num4;
                CurrentIndex -= num2 * num;
                NumberOfPermutation /= SizeOfArray - CurrentCycle;
                CurrentCycle++;
            }
            return OriginalPermutation;
        }*/

        public static SubCalendarEvent[] getBestPermutation(
            List<SubCalendarEvent> AllEvents,
            //double worstDistance,
            Tuple<Location_Elements, Location_Elements> BorderElements = null,
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
            Tuple<Location_Elements, Location_Elements> BorderElements = null
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
                        totalDistance += Location_Elements.calculateDistance(BorderElements.Item1, myList.First().myLocation);
                        totalDistance += Location_Elements.calculateDistance(BorderElements.Item2, myList.Last().myLocation);
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

        public static SubCalendarEvent[] getBestPermutation(List<SubCalendarEvent> AllEvents, double worstDistance, Tuple<Location_Elements, Location_Elements> BorderElements = null)
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
                            totalDistance += Location_Elements.calculateDistance(BorderElements.Item1, myList.First().myLocation);
                            totalDistance += Location_Elements.calculateDistance(BorderElements.Item2, myList.Last().myLocation);
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

        public static Location_Elements[] getBestPermutation(List<Location_Elements> AllEvents, double worstDistance)
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
                    List<Location_Elements> myList = new List<Location_Elements>();
                    foreach (int eachInt in myArray)
                    {
                        myList.Add(AllEvents[eachInt]);
                    }
                    //if (Utility.PinSubEventsToEnd(myList, restrictingTimeLine))
                    {
                        //totalDistance = SubCalendarEvent.CalculateDistance(myList, worstDistance);

                        totalDistance = Location_Elements.calculateDistance(myList);
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

            Location_Elements[] ret_GoodOrder = new Location_Elements[AllEvents.Count];
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


        public static SubCalendarEvent getClosestSubCalendarEvent(IEnumerable<SubCalendarEvent> AllSubCalEvents, Location_Elements ReferenceSubEvent)
        {
            SubCalendarEvent RetValue = null;
            double shortestDistance = double.MaxValue;
            foreach (SubCalendarEvent eachSubCalendarEvent in AllSubCalEvents)
            {
                double DistanceSoFar = Location_Elements.calculateDistance(eachSubCalendarEvent.myLocation, ReferenceSubEvent);
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

        static public bool PinSubEventsToStart(IEnumerable<SubCalendarEvent> arg1, TimeLine Arg2)
        {
            return PinSubEventsToStart_NoEdit(arg1.ToArray(), Arg2);
        }

        static private bool PinSubEventsToStart_NoEdit(SubCalendarEvent[] arg1, TimeLine Arg2)
        {
            bool retValue = true;
            long length = arg1.LongLength;
            TimeLine refTimeline = Arg2.CreateCopy();
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






        static private bool PinSubEventsToStart_NoEdit(List<SubCalendarEvent> arg1, TimeLine Arg2)
        {
            bool retValue = true;
            if (arg1.Count > 0)
            {
                retValue = arg1[0].PinToStart(Arg2);
                TimeLine var0 = new TimeLine(arg1[0].ActiveSlot.End, Arg2.End);
                arg1.RemoveAt(0);
                if (retValue && PinSubEventsToStart_NoEdit(arg1, var0))
                {
                    return true;
                }
                else
                {
                    return false;
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


            return PinSubEventsToEnd_NoEdit(arg1.ToArray(), Arg2);
        }


        static private bool PinSubEventsToEnd_NoEdit(SubCalendarEvent[] arg1, TimeLine Arg2)
        {
            bool retValue = true;
            long length = arg1.LongLength;
            TimeLine refTimeline = Arg2.CreateCopy();
            SubCalendarEvent refEvent;
            for (long i = length - 1; i >= 0; i--)
            {//hack notice you need to ensure that each subcalevent can fit within the timeline. YOu need a way to resolve this if not possible
                refEvent = arg1[i];
                if (refEvent.PinToEnd(refTimeline))
                {

                }
                else
                {
                    retValue = false;
                    break;
                }
                refTimeline = new TimeLine(refTimeline.Start, refEvent.ActiveSlot.Start);
            }
            return retValue;
        }


        static private bool PinSubEventsToEnd_NoEdit(List<SubCalendarEvent> arg1, TimeLine Arg2)
        {
            bool retValue = true;
            if (arg1.Count > 0)
            {//hack notice you need to ensure that each subcalevent can fit within the timeline. YOu need a way to resolve this if not possible
                retValue = arg1[arg1.Count - 1].PinToEnd(Arg2);
                TimeLine var0 = new TimeLine(Arg2.Start, arg1[arg1.Count - 1].ActiveSlot.Start);
                arg1.RemoveAt(arg1.Count - 1);
                if (retValue && PinSubEventsToEnd_NoEdit(arg1, var0))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

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
                retValue += eachSubCalendarEvent.ActiveDuration;
            }
            return retValue;
        }

        static public double calculateDistance(List<SubCalendarEvent> allSubCalEvents, Dictionary<string, List<Double>> DistanceMatrix)
        {
            List<string> AllIds = allSubCalEvents.Select(obj => obj.SubEvent_ID.getCalendarEventComponent()).ToList();
            List<string> Allkeys = DistanceMatrix.Keys.ToList();
            int i = 0;
            int j = i + 1;
            double retValue = 0;
            while (j < allSubCalEvents.Count)
            {
                string iIndex = AllIds[i];
                string jIndex = AllIds[j];
                int valueIndexofkeyValuePair = Allkeys.IndexOf(jIndex);
                double increment = DistanceMatrix[AllIds[i]][valueIndexofkeyValuePair];

                retValue += increment;
                i++; j++;
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

        static public TimeLine AddSubCaleventsToTimeLine(TimeLine EncasingTimeLine, IEnumerable<SubCalendarEvent> SubCalendarEvents)
        {
            EncasingTimeLine = EncasingTimeLine.CreateCopy();
            foreach (SubCalendarEvent eachSubCalendarEvent in SubCalendarEvents)
            {
                if (eachSubCalendarEvent.canExistWithinTimeLine(EncasingTimeLine))
                {
                    EncasingTimeLine.AddBusySlots(eachSubCalendarEvent.ActiveSlot);
                }
                else
                {
                    return null;
                }
            }

            return EncasingTimeLine;
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



        public static List<T> RandomizeIEnumerable<T>(List<T> EntryList)
        {
            //EntryList=EntryList.ToList();
            List<T> retValue = EntryList.ToList();
            Random myRand = new Random(1);
            for (int i = 0; i < EntryList.Count; i++)
            {
                int MyNumb = myRand.Next(0, EntryList.Count);
                T Temp = retValue[i];
                retValue[i] = retValue[MyNumb];
                retValue[MyNumb] = Temp;
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

    }
}
