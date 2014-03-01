#define EnableHive
#define LinearTSP
#if  EnableHive
#undef LinearTSP
#endif


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SolverFoundation.Common;
using Microsoft.SolverFoundation.Services;
using Microsoft.SolverFoundation.Solvers;



namespace My24HourTimerWPF
{
    static class DistanceSolver
    {
        public static Tuple<ICollection<SubCalendarEvent>, double> Run(ICollection<SubCalendarEvent> ListOfElements, int BeginningAndEnd=0)
        {
#if EnableHive            
            CitiesData citiesData = new CitiesData(ListOfElements.ToList());
            int totalNumberBees = 100;
            int numberInactive = 20;
            int numberActive = 50;
            int numberScout = 30;

            int maxNumberVisits = 50;
            int maxNumberCycles = 20;

            Hive hive = new Hive(totalNumberBees, numberInactive, numberActive, numberScout, maxNumberVisits, maxNumberCycles, citiesData, BeginningAndEnd);


            bool doProgressBar = false;
            hive.Solve(doProgressBar);
            return hive.getBestPath();
#endif
            



#if LinearTSP
            Coordinate[] data = new Coordinate[ListOfElements.Count];
            Dictionary<int, SubCalendarEvent> DictOFData = new Dictionary<int,SubCalendarEvent>();
            int NameIndex = 0;
            foreach (SubCalendarEvent eachSubCalendarEvent in ListOfElements)
            {
                data[NameIndex] = new Coordinate(NameIndex, eachSubCalendarEvent);
                DictOFData.Add(NameIndex++, eachSubCalendarEvent);
            }

            SolverContext context = SolverContext.GetContext();
            Model model = context.CreateModel();

            // ------------
            // Parameters
            Set city = new Set(Domain.IntegerNonnegative, "city");
            Parameter dist = new Parameter(Domain.Real, "dist", city, city);
            var arcs = from p1 in data
                       from p2 in data
                       select new Arc { City1 = p1.Name, City2 = p2.Name, Distance = p1.Distance(p2, data.Length) };
            dist.SetBinding(arcs, "Distance", "City1", "City2");
            model.AddParameters(dist);

            // ------------
            // Decisions
            Decision assign = new Decision(Domain.IntegerRange(0, 1), "assign", city, city);
            Decision rank = new Decision(Domain.RealNonnegative, "rank", city);
            model.AddDecisions(assign, rank);

            // ------------
            // Goal: minimize the length of the tour.
            Goal goal = model.AddGoal("TourLength", GoalKind.Minimize,
              Model.Sum(Model.ForEach(city, i => Model.ForEachWhere(city, j => dist[i, j] * assign[i, j], j => i != j))));

            // ------------
            // Enter and leave each city only once.
            int N = data.Length;
            model.AddConstraint("assign_1",
              Model.ForEach(city, i => Model.Sum(Model.ForEachWhere(city, j => assign[i, j],
                j => i != j)) == 1));
            model.AddConstraint("assign_2",
              Model.ForEach(city, j => Model.Sum(Model.ForEachWhere(city, i => assign[i, j], i => i != j)) == 1));

            // Forbid subtours (Miller, Tucker, Zemlin - 1960...)
            model.AddConstraint("no_subtours",
              Model.ForEach(city,
                i => Model.ForEachWhere(city,
                  j => rank[i] + 1 <= rank[j] + N * (1 - assign[i, j]),
                  j => Model.And(i != j, i >= 1, j >= 1)
                )
              )
            );

            Solution solution = context.Solve();
            double Cost = goal.ToDouble();
            List<SubCalendarEvent> OptimizedSubCalEvents = new List<SubCalendarEvent>();

            var tour = from p in assign.GetValues() where (double)p[0] > 0.9 select p;

            foreach (var i in tour.ToArray())
            {
                int MyIndex =Convert.ToInt32(i[2]);
                OptimizedSubCalEvents.Add(DictOFData[MyIndex]);
                //Console.WriteLine(i[1] + " -> " + );
            }

            context.ClearModel();

            return new Tuple<ICollection<SubCalendarEvent>, double>(OptimizedSubCalEvents, Cost);
#endif


        }

        // class CitiesData

        static public double AverageToAllNodes(Location MyLocation, List<Location> otherLocations)
        {
            double retValue = 0;
            foreach (Location eachLocation in otherLocations)
            {
                retValue += Location.calculateDistance(MyLocation, eachLocation);
            }
            retValue = retValue/(double)otherLocations.Count;
            return retValue;
        }

        }

    class Hive
    {
        public int BeginningAndEnd { set; get; }

        public class Bee
        {
            public int status; // 0 = inactive, 1 = active, 2 = scout
            public SubCalendarEvent[] memoryMatrix; // problem-specific. a path of cities.
            public double measureOfQuality; // smaller values are better. total distance of path.
            public int numberOfVisits;

            public Bee(int status, SubCalendarEvent[] memoryMatrix, double measureOfQuality, int numberOfVisits)
            {
                this.status = status;
                this.memoryMatrix = new SubCalendarEvent[memoryMatrix.Length];
                Array.Copy(memoryMatrix, this.memoryMatrix, memoryMatrix.Length);
                this.measureOfQuality = measureOfQuality;
                this.numberOfVisits = numberOfVisits;
            }

            public override string ToString()
            {
                string s = "";
                s += "Status = " + this.status + "\n";
                s += " Memory = " + "\n";
                for (int i = 0; i < this.memoryMatrix.Length - 1; ++i)
                    s += this.memoryMatrix[i] + "->";
                s += this.memoryMatrix[this.memoryMatrix.Length - 1] + "\n";
                s += " Quality = " + this.measureOfQuality.ToString("F4");
                s += " Number visits = " + this.numberOfVisits;
                return s;
            }
        } // Bee

        static Random random = null; // multipurpose

        public CitiesData citiesData; // this is the problem-specific data we want to optimize

        public int totalNumberBees; // mostly for readability in the object constructor call
        public int numberInactive;
        public int numberActive;
        public int numberScout;

        public int maxNumberCycles; // one cycle represents an action by all bees in the hive
        //public int maxCyclesWithNoImprovement; // deprecated

        public int maxNumberVisits; // max number of times bee will visit a given food source without finding a better neighbor
        public double probPersuasion = 1; // probability inactive bee is persuaded by better waggle solution
        public double probMistake = 0.00; // probability an active bee will reject a better neighbor food source OR accept worse neighbor food source

        public Bee[] bees;
        public SubCalendarEvent[] bestMemoryMatrix; // problem-specific
        public double bestMeasureOfQuality;
        public int[] indexesOfInactiveBees; // contains indexes into the bees array

        public Tuple<ICollection<SubCalendarEvent>, double> getBestPath()
        {
            return new Tuple<ICollection<SubCalendarEvent>, double>(this.bestMemoryMatrix.ToList(), bestMeasureOfQuality);
        }

        public override string ToString()
        {
            string s = "";
            s += "Best path found: ";
            for (int i = 0; i < this.bestMemoryMatrix.Length - 1; ++i)
                s += this.bestMemoryMatrix[i].SubEvent_ID.getLevelID(0) + "->";
            s += this.bestMemoryMatrix[this.bestMemoryMatrix.Length - 1].SubEvent_ID.getLevelID(0) + "\n";

            s += "Path quality:    ";
            if (bestMeasureOfQuality < 10000.0)
                s += bestMeasureOfQuality.ToString("F4") + "\n";
            else
                s += bestMeasureOfQuality.ToString("#.####e+00");
            s += "\n";
            return s;
        }

        public Hive(int totalNumberBees, int numberInactive, int numberActive, int numberScout, int maxNumberVisits,
          int maxNumberCycles, CitiesData citiesData, int BeginningAndEnd=0)
        {
            random = new Random(0);
            this.BeginningAndEnd = BeginningAndEnd;
            this.totalNumberBees = totalNumberBees;
            this.numberInactive = numberInactive;
            this.numberActive = numberActive;
            this.numberScout = numberScout;
            this.maxNumberVisits = maxNumberVisits;
            this.maxNumberCycles = maxNumberCycles;
            //this.maxCyclesWithNoImprovement = maxCyclesWithNoImprovement;

            //this.citiesData = new CitiesData(citiesData.cities.Length); // hive's copy of problem-specific data
            this.citiesData = citiesData; // reference to CityData

            // this.probPersuasion & this.probMistake are hard-coded in class definition

            this.bees = new Bee[totalNumberBees];
            this.bestMemoryMatrix = GenerateRandomMemoryMatrix(); // alternative initializations are possible
            this.bestMeasureOfQuality = MeasureOfQuality(this.bestMemoryMatrix);

            this.indexesOfInactiveBees = new int[numberInactive]; // indexes of bees which are currently inactive

            for (int i = 0; i < totalNumberBees; ++i) // initialize each bee, and best solution
            {
                int currStatus; // depends on i. need status before we can initialize Bee
                if (i < numberInactive)
                {
                    currStatus = 0; // inactive
                    indexesOfInactiveBees[i] = i; // curr bee is inactive
                }
                else if (i < numberInactive + numberScout)
                {
                    currStatus = 2; // scout
                }
                else
                {
                    currStatus = 1; // active
                }

                SubCalendarEvent[] randomMemoryMatrix = GenerateRandomMemoryMatrix();
                double mq = MeasureOfQuality(randomMemoryMatrix);
                int numberOfVisits = 0;

                bees[i] = new Bee(currStatus, randomMemoryMatrix, mq, numberOfVisits); // instantiate current bee

                // does this bee have best solution?
                if (bees[i].measureOfQuality < bestMeasureOfQuality) // curr bee is better (< because smaller is better)
                {
                    Array.Copy(bees[i].memoryMatrix, this.bestMemoryMatrix, bees[i].memoryMatrix.Length);
                    this.bestMeasureOfQuality = bees[i].measureOfQuality;
                }
            } // each bee

        } // TravelingSalesmanHive ctor


        public SubCalendarEvent[] GenerateRandomMemoryMatrix()
        {
            SubCalendarEvent[] result = new SubCalendarEvent[this.citiesData.cities.Length]; // // problem-specific
            
            
            //Dictionary<string, int> DictCount = new Dictionary<string, int>();
            Dictionary<string, List<SubCalendarEvent>> DictCities = new Dictionary<string, List<SubCalendarEvent>>();

            for (int j = 0; j < this.citiesData.cities.Length; j++)
            {
                if (DictCities.ContainsKey(this.citiesData.cities[j].SubEvent_ID.getLevelID(0)))
                {
                    //++DictCount[this.citiesData.cities[j].SubEvent_ID.getLevelID(0)];
                    DictCities[this.citiesData.cities[j].SubEvent_ID.getLevelID(0)].Add(this.citiesData.cities[j]);
                }
                else
                {
                    //DictCount.Add(this.citiesData.cities[j].SubEvent_ID.getLevelID(0), 1);
                    DictCities.Add(this.citiesData.cities[j].SubEvent_ID.getLevelID(0), new List<SubCalendarEvent>() { this.citiesData.cities[j] });
                }
            }

            
            int resultIndex = 0;
            int MaxResultIndex=this.citiesData.cities.Length;
            switch (BeginningAndEnd)
            {
                case 1://must Beginning with City
                    {
                        result[0] = this.citiesData.cities[0];
                        string ConcernedIndex = result[0].SubEvent_ID.getLevelID(0);
                        DictCities[ConcernedIndex].Remove(result[0]);
                        if (DictCities[ConcernedIndex].Count < 1)
                        {
                            DictCities.Remove(ConcernedIndex);
                        }
                        resultIndex=1;
                    }
                    break;
                case 2://must End with City
                    {
                        result[this.citiesData.cities.Length - 1] = this.citiesData.cities[this.citiesData.cities.Length - 1];
                        string ConcernedIndex = result[this.citiesData.cities.Length - 1].SubEvent_ID.getLevelID(0);
                        DictCities[ConcernedIndex].Remove(result[this.citiesData.cities.Length - 1]);
                        if (DictCities[ConcernedIndex].Count < 1)
                        {
                            DictCities.Remove(ConcernedIndex);
                        }
                        MaxResultIndex-=1;
                    }
                    break;
                case 3://must end and Begin with Cities
                    {
                        result[0] = this.citiesData.cities[0];
                        resultIndex=1;
                        result[this.citiesData.cities.Length - 1] = this.citiesData.cities[this.citiesData.cities.Length - 1];
                        MaxResultIndex-=1;

                        string ConcernedIndex=result[0].SubEvent_ID.getLevelID(0);
                        DictCities[ConcernedIndex].Remove(result[0]);
                        if (DictCities[ConcernedIndex].Count < 1)
                        {
                            DictCities.Remove(ConcernedIndex);
                        }
                        ConcernedIndex = result[this.citiesData.cities.Length - 1].SubEvent_ID.getLevelID(0);
                        DictCities[ConcernedIndex].Remove(result[this.citiesData.cities.Length - 1]);
                        if (DictCities[ConcernedIndex].Count < 1)
                        {
                            DictCities.Remove(ConcernedIndex);
                        }
                    }
                    break;
                default:
                    {
                        ;
                    }
                    break;
            }


            List<string> AllNodeNames = DictCities.Keys.ToList();
            List<string> ResultName = new List<string>();

            while (resultIndex < MaxResultIndex)
            {
                for (int i = 0; i < AllNodeNames.Count; i++) // Fisher-Yates (Knuth) shuffle
                {
                    int r = random.Next(i, AllNodeNames.Count);
                    SubCalendarEvent temp;

                    if (DictCities[AllNodeNames[r]].Count > 0)
                    {
                        string sTemp = AllNodeNames[r];
                        AllNodeNames[r] = AllNodeNames[i];
                        AllNodeNames[i] = sTemp;
                        temp = DictCities[sTemp][0];
                        DictCities[sTemp].RemoveAt(0);
                        result[resultIndex++] = temp;
                        //result[i] = temp;
                        if (DictCities[sTemp].Count < 1)
                        {
                            DictCities.Remove(sTemp);
                            AllNodeNames.Remove(sTemp);
                            --i;
                        }
                    }
                    else
                    {
                        ;
                    }
                    /*//SubCalendarEvent temp = result[r];
                    result[r] = result[i];
                    result[i] = temp;*/
                }



            }





            /*
            Array.Copy(this.citiesData.cities, result, this.citiesData.cities.Length);

            for (int i = 0; i < result.Length; i++) // Fisher-Yates (Knuth) shuffle
            {
                int r = random.Next(i, result.Length);
                SubCalendarEvent temp = result[r]; 
                result[r] = result[i]; 
                result[i] = temp;
            }*/
            return result;
        } // GenerateRandomMemoryMatrix()

        public SubCalendarEvent[] GenerateNeighborMemoryMatrix(SubCalendarEvent[] memoryMatrix)
        {
            SubCalendarEvent[] result = new SubCalendarEvent[memoryMatrix.Length];
            Array.Copy(memoryMatrix, result, memoryMatrix.Length);

            int Beginning = 0;
            int End = result.Length;

            switch (BeginningAndEnd)
            {
                case 1://must Beginning with City
                    {
                        Beginning = 1;
                        End = result.Length;
                    }
                    break;
                case 2://must End with City
                    {
                        Beginning = 0;
                        End = result.Length-1;
                    }
                    break;
                case 3://must end and Begin with Cities
                    {
                        Beginning = 1;
                        End = result.Length-1;
                    }
                    break;
                default:
                    { 
                        Beginning = 0;
                        End = result.Length;
                    }
                    break;
            }

            int ranIndex = random.Next(Beginning, End); // [0, Length-1] inclusive
            int adjIndex;
            if (ranIndex == End - 1)
                adjIndex = Beginning;
            else
                adjIndex = ranIndex + 1;

            SubCalendarEvent tmp = result[ranIndex];
            result[ranIndex] = result[adjIndex];
            result[adjIndex] = tmp;

            return result;
        } // GenerateNeighborMemoryMatrix()

        public double MeasureOfQuality(SubCalendarEvent[] memoryMatrix)
        {
            double answer = 0.0;
            for (int i = 0; i < memoryMatrix.Length - 1; ++i)
            {
                SubCalendarEvent c1 = memoryMatrix[i];
                SubCalendarEvent c2 = memoryMatrix[i + 1];
                double d = this.citiesData.Distance(c1, c2);
                answer += d;
            }
            return answer;
        } // MeasureOfQuality()

        public void Solve(bool doProgressBar) // find best Traveling Salesman Problem solution
        {
            bool pb = doProgressBar; // just want a shorter variable
            int numberOfSymbolsToPrint = 10; // 10 units so each symbol is 10.0% progress
            int increment = this.maxNumberCycles / numberOfSymbolsToPrint;
            if (pb) Console.WriteLine("\nEntering SBC Traveling Salesman Problem algorithm main processing loop\n");
            if (pb) Console.WriteLine("Progress: |==========|"); // 10 units so each symbol is 10% progress
            if (pb) Console.Write("           ");
            int cycle = 0;

            while (cycle < this.maxNumberCycles)
            {
                for (int i = 0; i < this.totalNumberBees; ++i) // each bee
                {
                    if (this.bees[i].status == 1) // active bee
                        ProcessActiveBee(i);
                    else if (this.bees[i].status == 2) // scout bee
                        ProcessScoutBee(i);
                    else if (this.bees[i].status == 0) // inactive bee
                        ProcessInactiveBee(i);
                } // for each bee
                ++cycle;

                // print a progress bar
                if (pb && cycle % increment == 0)
                    Console.Write("^");
            } // main while processing loop

            if (pb) Console.WriteLine(""); // end the progress bar
        } // Solve()

        private void ProcessInactiveBee(int i)
        {
            return; // not used in this implementation
        }

        private void ProcessActiveBee(int i)
        {
            SubCalendarEvent[] neighbor = GenerateNeighborMemoryMatrix(bees[i].memoryMatrix); // find a neighbor solution
            double neighborQuality = MeasureOfQuality(neighbor); // get its quality
            double prob = random.NextDouble(); // used to determine if bee makes a mistake; compare against probMistake which has some small value (~0.01)
            bool memoryWasUpdated = false; // used to determine if bee should perform a waggle dance when done
            bool numberOfVisitsOverLimit = false; // used to determine if bee will convert to inactive status

            if (neighborQuality < bees[i].measureOfQuality) // active bee found better neighbor (< because smaller values are better)
            {
                if (prob < probMistake) // bee makes mistake: rejects a better neighbor food source
                {
                    ++bees[i].numberOfVisits; // no change to memory but update number of visits
                    if (bees[i].numberOfVisits > maxNumberVisits) numberOfVisitsOverLimit = true;
                }
                else // bee does not make a mistake: accepts a better neighbor
                {
                    Array.Copy(neighbor, bees[i].memoryMatrix, neighbor.Length); // copy neighbor location into bee's memory
                    bees[i].measureOfQuality = neighborQuality; // update the quality
                    bees[i].numberOfVisits = 0; // reset counter
                    memoryWasUpdated = true; // so that this bee will do a waggle dance 
                }
            }
            else // active bee did not find a better neighbor
            {
                //Console.WriteLine("c");
                if (prob < probMistake) // bee makes mistake: accepts a worse neighbor food source
                {
                    Array.Copy(neighbor, bees[i].memoryMatrix, neighbor.Length); // copy neighbor location into bee's memory
                    bees[i].measureOfQuality = neighborQuality; // update the quality
                    bees[i].numberOfVisits = 0; // reset
                    memoryWasUpdated = true; // so that this bee will do a waggle dance 
                }
                else // no mistake: bee rejects worse food source
                {
                    ++bees[i].numberOfVisits;
                    if (bees[i].numberOfVisits > maxNumberVisits) numberOfVisitsOverLimit = true;
                }
            }

            // at this point we need to determine a.) if the number of visits has been exceeded in which case bee becomes inactive
            // or b.) memory was updated in which case check to see if new memory is a global best, and then bee does waggle dance
            // or c.) neither in which case nothing happens (bee just returns to hive).

            if (numberOfVisitsOverLimit == true)
            {
                bees[i].status = 0; // current active bee transitions to inactive
                bees[i].numberOfVisits = 0; // reset visits (and no change to this bees memory)
                int x = random.Next(numberInactive); // pick a random inactive bee. x is an index into a list, not a bee ID
                bees[indexesOfInactiveBees[x]].status = 1; // make it active
                indexesOfInactiveBees[x] = i; // record now-inactive bee 'i' in the inactive list
            }
            else if (memoryWasUpdated == true) // current bee returns and performs waggle dance
            {
                // first, determine if the new memory is a global best. note that if bee has accepted a worse food source this can't be true
                if (bees[i].measureOfQuality < this.bestMeasureOfQuality) // the modified bee's memory is a new global best (< because smaller is better)
                {
                    Array.Copy(bees[i].memoryMatrix, this.bestMemoryMatrix, bees[i].memoryMatrix.Length); // update global best memory
                    this.bestMeasureOfQuality = bees[i].measureOfQuality; // update global best quality
                }
                DoWaggleDance(i);
            }
            else // number visits is not over limit and memory was not updated so do nothing (return to hive but do not waggle)
            {
                return;
            }
        } // ProcessActiveBee()

        private void ProcessScoutBee(int i)
        {
            SubCalendarEvent[] randomFoodSource = GenerateRandomMemoryMatrix(); // scout bee finds a random food source. . . 
            double randomFoodSourceQuality = MeasureOfQuality(randomFoodSource); // and examines its quality
            if (randomFoodSourceQuality < bees[i].measureOfQuality) // scout bee has found a better solution than its current one (< because smaller measure is better)
            {
                Array.Copy(randomFoodSource, bees[i].memoryMatrix, randomFoodSource.Length); // unlike active bees, scout bees do not make mistakes
                bees[i].measureOfQuality = randomFoodSourceQuality;
                // no change to scout bee's numberOfVisits or status

                // did this scout bee find a better overall/global solution?
                if (bees[i].measureOfQuality < bestMeasureOfQuality) // yes, better overall solution (< because smaller is better)
                {
                    Array.Copy(bees[i].memoryMatrix, this.bestMemoryMatrix, bees[i].memoryMatrix.Length); // copy scout bee's memory to global best
                    this.bestMeasureOfQuality = bees[i].measureOfQuality;
                } // better overall solution

                DoWaggleDance(i); // scout returns to hive and does waggle dance

            } // if scout bee found better solution
        } // ProcessScoutBee()

        private void DoWaggleDance(int i)
        {
            for (int ii = 0; ii < numberInactive; ++ii) // each inactive/watcher bee
            {
                int b = indexesOfInactiveBees[ii]; // index of an inactive bee
                if (bees[b].status != 0) throw new Exception("Catastrophic logic error when scout bee waggles dances");
                if (bees[b].numberOfVisits != 0) throw new Exception("Found an inactive bee with numberOfVisits != 0 in Scout bee waggle dance routine");
                if (bees[i].measureOfQuality < bees[b].measureOfQuality) // scout bee has a better solution than current inactive/watcher bee (< because smaller is better)
                {
                    double p = random.NextDouble(); // will current inactive bee be persuaded by scout's waggle dance?
                    if (this.probPersuasion > p) // this inactive bee is persuaded by the scout (usually because probPersuasion is large, ~0.90)
                    {
                        Array.Copy(bees[i].memoryMatrix, bees[b].memoryMatrix, bees[i].memoryMatrix.Length);
                        bees[b].measureOfQuality = bees[i].measureOfQuality;
                    } // inactive bee has been persuaded
                } // scout bee has better solution than watcher/inactive bee
            } // each inactive bee
        } // DoWaggleDance()

    } // class ShortestPathHive


    class CitiesData
    {
        public SubCalendarEvent[] cities;
        public Dictionary<string, List<Double>> DistanceMatrix;
        public List<string> Horizontal;

        public CitiesData(List<SubCalendarEvent> MyLocations)
        {
            this.DistanceMatrix = new Dictionary<string, List<double>>();
            cities = MyLocations.ToArray();
            int i = 0;
            int j = 0;
            foreach (SubCalendarEvent eachSubCalendarEvent in MyLocations)
            {
                if (!this.DistanceMatrix.ContainsKey(eachSubCalendarEvent.SubEvent_ID.getLevelID(0)))
                {
                    this.DistanceMatrix.Add(eachSubCalendarEvent.SubEvent_ID.getLevelID(0), new List<double>());
                    //rizontal = DistanceMatrix.Keys.ToList();
                    foreach (SubCalendarEvent eachSubCalendarEvent0 in MyLocations)
                    {
                        double MyDistance = Location.calculateDistance(eachSubCalendarEvent.myLocation, eachSubCalendarEvent0.myLocation);
                        if (eachSubCalendarEvent0.SubEvent_ID.getLevelID(0) == eachSubCalendarEvent.SubEvent_ID.getLevelID(0))
                        {
                            MyDistance = double.MaxValue / MyLocations.Count;
                        }
                        this.DistanceMatrix[eachSubCalendarEvent.SubEvent_ID.getLevelID(0)].Add(MyDistance);
                    }
                }

            }

            Horizontal = DistanceMatrix.Keys.ToList();
        }
        public double Distance(SubCalendarEvent firstCity, SubCalendarEvent secondCity)
        {
            return DistanceMatrix[firstCity.SubEvent_ID.getLevelID(0)][Horizontal.IndexOf(secondCity.SubEvent_ID.getLevelID(0))];
        }
        public double ShortestPathLength()
        {
            return 1.0 * (this.cities.Length - 1);
        }
        public long NumberOfPossiblePaths()
        {
            long n = this.cities.Length;
            long answer = 1;
            for (int i = 1; i <= n; ++i)
            //checked 
            { answer *= i; }
            return answer;
        }
        public override string ToString()
        {
            string s = "";
            s += "Cities: ";
            for (int i = 0; i < this.cities.Length; ++i)
                s += this.cities[i].SubEvent_ID.getLevelID(0) + " ";
            return s;
        }
    } 

         class Coordinate
        {
            public int Name { get; set; }
            public SubCalendarEvent Event { get; set; }
            public string ID { get; set; }

            public Coordinate(int name, SubCalendarEvent MyEvent)
            {
                Name = name;
                Event = MyEvent;
                ID = MyEvent.SubEvent_ID.getStringIDAtLevel(0);
            }
            
            public double Distance(Coordinate p,int Divider)
            {
 
                // There may rounding difficulties her if the points are close together...just sayin'.
                double retValue;
                if (this.ID == p.ID)
                {
                    retValue = (double.MaxValue / Divider) -10000;
                    //retValue = 1;
                }
                else
                {
                    retValue = Location.calculateDistance(this.Event.myLocation, p.Event.myLocation);
                }
                return retValue;
            }
        }

         class Arc
        {
            public int City1 { get; set; }
            public int City2 { get; set; }
            public double Distance { get; set; }
        }


}
    
    


