using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public interface IWhy
    {
        IWhy Because();
        IWhy OtherWise();
        IWhy WhatIf();
    }
}
