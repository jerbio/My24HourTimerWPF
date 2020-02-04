using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TilerElements
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

    public class nTuple<T, U, V>
    {
        public T Item1;
        public U Item2;
        public V Item3;

        public nTuple(T Arg1, U Arg2, V Arg3)
        {
            Item1 = Arg1;
            Item2 = Arg2;
            Item3 = Arg3;
        }

        public nTuple(nTuple<T, U, V> mTupleToBeCopied)
        {
            Item1 = mTupleToBeCopied.Item1;
            Item2 = mTupleToBeCopied.Item2;
            Item3 = mTupleToBeCopied.Item3;
        }

        public override string ToString()
        {
            return Item1.ToString() + ":||:" + Item2.ToString() + ":||:" + Item3.ToString();
        }
    }
}
