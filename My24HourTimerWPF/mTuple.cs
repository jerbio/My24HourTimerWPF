using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    class mTuple<T,U>
    {
        public T Item1;
        public U Item2;

        public  mTuple(T Arg1, U Arg2)
        {
            Item1 = Arg1;
            Item2 = Arg2;
        }
    }
}
