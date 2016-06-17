using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Wpf;

namespace TilerElements.DB
{
    public class TilerDbContext:LocalContext
    {
        public TilerDbContext(): base("DefaultConnection", throwIfV1Schema: false)
        {

        }

        public TilerDbContext(string Connection = "DefaultConnection", bool throwIfV1Schema = false)
            : base(Connection, throwIfV1Schema: false)
        {
        }

        public static TilerDbContext Create()
        {
            return new TilerDbContext();
        }
    }
}
