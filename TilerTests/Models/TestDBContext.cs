using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TilerElements;

namespace TilerTests.Models
{
    public class TestDBContext: TilerElements.TilerDbContext
    {
        ///NOTE JEROME
        ///This should be uncommented out when running the entity framework commands.
        ///Also ensure tilertest project is the startup project for the solution so the right DB is used. The DB is selected based on the selected app.config file
        ///The "enable-migrations" uses the app.config of the startup project
        ///Also, you need to correctly initialize DB_RUNTIME_LOCATION to the location where the mdf file will be used during testing. Usually in the "bin/Debug" folder.
        ///After a successfull data base update, i.e. update-database COMMAND runs successfully, you need to copy the databse files (test-db.mdf and test-db_log.ldf) from the runtime folder back to project folder.
        ///This is because the database upgrade happens in the folder specified by DB_RUNTIME_LOCATION. And you need to ensure that git can store the new database at its initialized state.
        ///Warning, the package manager console might run things in some app_data temp location if DB_RUNTIME_LOCATION is not initialized.
        ///Also you might want to use this stackoverflow article https://stackoverflow.com/questions/25013790/code-first-migrations-updating-the-wrong-db-on-localdb
        ///
        public TestDBContext() : base("TestDBConnection")
        {
            string DB_RUNTIME_LOCATION = @"C:\Users\jerom\Documents\Visual Studio 2015\Projects\WagTap\My24HourTimerWPF\TilerTests\bin\Debug";
            AppDomain.CurrentDomain.SetData("DataDirectory", DB_RUNTIME_LOCATION);
            System.Data.Entity.Database.SetInitializer<TestDBContext>(null);
        }
        public TestDBContext(string connectionName = "TestDBConnection")
            : base(connectionName)
        {
            System.Data.Entity.Database.SetInitializer<TestDBContext>(null);// this is needed so you don't run into an error being thrown because its trying to create new database, or it fails to find an already created DB.
        }
    }
}