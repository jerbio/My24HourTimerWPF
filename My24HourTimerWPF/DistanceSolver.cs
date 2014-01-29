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

        public static Tuple<ICollection<SubCalendarEvent>, double> Run(ICollection<SubCalendarEvent> ListOfElements)
        {
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

            

        }


        private class Coordinate
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

        private class Arc
        {
            public int City1 { get; set; }
            public int City2 { get; set; }
            public double Distance { get; set; }
        }
    }
    
    

}
