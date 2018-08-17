using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;

namespace TilerTests.Models
{
    public class TestDBContext: TilerElements.TilerDbContext
    {
        public TestDBContext()
        {
        }
        public TestDBContext(string connectionName = "TestDBConnection")
            : base(connectionName)
        {
        }
    }
}
