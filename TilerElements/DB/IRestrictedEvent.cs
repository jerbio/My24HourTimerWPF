using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Wpf;


namespace TilerElements.DB
{
    interface IRestrictedEvent
    {
        RestrictionProfile Restriction { get; set; }
    }
}
