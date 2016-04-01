using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TilerElements.Wpf;
//using TilerFront.Models;

namespace TilerElements.DB
{

    public class DBControlDirect:DBControl
    {
        /*
        static SqlConnection Wagtap = new SqlConnection("user id=wagtap;" +
                                   "password=Tagwapadmin001;server=OLUJEROME-PC\\WAGTAPSYS;" +
                                   "Trusted_Connection=yes;" +
                                   "database=WagtapUserAccounts; " +
                                   "connection timeout=30");
        */
        protected static SqlConnection Wagtap = new SqlConnection("Server=tcp:gjjadsh2tt.database.windows.net,1433;Database=DatabaseWaggy;User ID=wagtap@gjjadsh2tt;Password=Tagwapadminazure007#;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;");
        
        public DBControlDirect(string IDData, string UserNameData)
        {
            ID = IDData;
            UserName = UserNameData;
        }


    }
}
