using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    static public class MultidimensionalCalculation
    {
        static public IList<Tuple<double, int>> getLowestCalculation(IList<IList<double>> DataSet, uint DimensionCount)
        {
            double[] Result = new double[DataSet.Count];
            List<Tuple<double,int>> RetValue = new List<Tuple<double,int>>();
            for (int i=0;i<DataSet.Count;i++)
            {
                Result[i] = calculate(DimensionCount, DataSet[i]);
            }
            for (int i=0 ; i<Result.Length;i++)
            {
                RetValue.Add(new Tuple<double, int>(Result[i], i));
            }
            return RetValue;
        }
        static double calculate(double DimensionCount,IList<double> Dimensions)
        {
            double Sum = 0;
            for (int i=0; i<DimensionCount;i++)
            {
                Sum += Math.Pow(Dimensions[i], 2);
            }
            double RetValue = Math.Sqrt(Sum);
            return RetValue;
        }
    }
}
