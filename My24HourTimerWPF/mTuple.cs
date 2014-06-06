using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    public class mTuple<T,U>
    {
        public T Item1;
        public U Item2;

        public  mTuple(T Arg1, U Arg2)
        {
            Item1 = Arg1;
            Item2 = Arg2;
        }

        public mTuple(mTuple<T,U> mTupleToBeCopied)
        {
            Item1 = mTupleToBeCopied.Item1;
            Item2 = mTupleToBeCopied.Item2;
        }

        public override string ToString()
        {
            return Item1.ToString() + ":||:" + Item2.ToString();
        }
    }
}
