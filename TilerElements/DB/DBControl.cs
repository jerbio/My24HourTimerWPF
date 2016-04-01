using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TilerElements.Wpf;

namespace TilerElements.DB
{

    public class DBControl
    {
        /*
        static SqlConnection Wagtap = new SqlConnection("user id=wagtap;" +
                                   "password=Tagwapadmin001;server=OLUJEROME-PC\\WAGTAPSYS;" +
                                   "Trusted_Connection=yes;" +
                                   "database=WagtapUserAccounts; " +
                                   "connection timeout=30");
        */
        protected static string DatabaseName = "mytiler_db";
        protected static SqlConnection Wagtap = new SqlConnection("Server=tcp:gjjadsh2tt.database.windows.net,1433;Database=" + DatabaseName + ";User ID=wagtap@gjjadsh2tt;Password=Tagwapadminazure007#;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;");
        
        protected string ID;
        protected string UserName;
        string UserPassword;

        protected DBControl()
        {
            UserName = "";
            UserPassword = "";
            ID = "";
        }
        /*
        public DBControl(UserAccount verifiedUser)
        {

            if (verifiedUser.Status)
            { 
                
            }
        }*/
        /*
        public DBControl(string UserName, string Password)
        {
            this.UserName =  UserName.ToLower();
            this.UserPassword = Password;
        }
        */

        public DBControl(string UserName, string UserID)
        {
            this.UserName = UserName.ToLower();
            ID = UserID;
        }

        public Tuple<bool, string,string> LogIn()//(string UserName, string Password)
        {
            Tuple<bool, string, string> retValue;
            string NamerOfUser = "";
            try
            {
                Wagtap.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


            string ID = "";
            try
            {
                SqlDataReader myReader = null;
                SqlCommand myCommand;
                if ( string.IsNullOrEmpty(this.ID ))//checks if ID has been initialized
                {
                    myCommand = new SqlCommand("select ID from DatabaseWaggy.dbo.UserLog where DatabaseWaggy.dbo.UserLog.UserName = '" + UserName + "' and DatabaseWaggy.dbo.UserLog.Password = '" + UserPassword + "'", Wagtap);
                    myReader = myCommand.ExecuteReader();
                    myCommand.CommandText = "";
                    while (myReader.Read())
                    {
                        ID = myReader["ID"].ToString();
                        this.ID = (ID);
                    }
                    myReader.Close();
                    //if (this.ID != 0)
                    if (!string.IsNullOrEmpty(this.ID))
                    {
                        SqlCommand InsertUserInfo = new SqlCommand("select FirstName from DatabaseWaggy.dbo.UserInfo where DatabaseWaggy.dbo.UserInfo.ID =" + this.ID + "", Wagtap);
                        myReader = InsertUserInfo.ExecuteReader();


                        while (myReader.Read())
                        {
                            NamerOfUser = myReader["FirstName"].ToString();
                        }
                    }
                    myReader.Close();
                }
                else 
                {
                    myCommand = new SqlCommand("select ID from DatabaseWaggy.dbo.UserLog where DatabaseWaggy.dbo.UserLog.UserName = '" + UserName + "' and DatabaseWaggy.dbo.UserLog.ID = " + this.ID + "", Wagtap);
                    myReader = myCommand.ExecuteReader();
                    this.ID = "";
                    ID = "";
                    while (myReader.Read())
                    {
                        ID = myReader["ID"].ToString();
                        this.ID = ID;
                    }
                    myReader.Close();
                    if (!string.IsNullOrEmpty(this.ID))// != 0)
                    {
                        SqlCommand InsertUserInfo = new SqlCommand("select FirstName from DatabaseWaggy.dbo.UserInfo where DatabaseWaggy.dbo.UserInfo.ID =" + this.ID + "", Wagtap);
                        myReader = InsertUserInfo.ExecuteReader();


                        while (myReader.Read())
                        {
                            NamerOfUser = myReader["FirstName"].ToString();
                        }
                    }

                    myReader.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                ID = "";
            }

            if (string.IsNullOrEmpty(ID))
            {
                retValue = new Tuple<bool, string,string>(false, "","");
            }
            else
            {
                retValue = new Tuple<bool, string, string>(true, this.ID, NamerOfUser);
            }

            Wagtap.Close();

            return retValue;
        }

        async public Task<bool> CreateLatestChange(string userID, DateTimeOffset timeOfDay, long LastUserID=1)
        {
                //"Insert into DatabaseWaggy.dbo.UserLog (UserName,Password,Active) values ('" + this.UserName + "','" + this.UserPassword + "','" + 1 + "') select ID from DatabaseWaggy.dbo.UserLog where DatabaseWaggy.dbo.UserLog.UserName='" + this.UserName + "'"
            bool retValue = false;

            DateTimeOffset dateData = timeOfDay.UtcDateTime;//.DateTime.Add(WebApiConfig.StartOfTimeTimeSpan);
            string LatestHash = encryptString(DateTimeOffset.Now.ToString()).Substring(0,50);
            LatestHash=LatestHash.Replace("\'", "").Replace("\"", "");
            
            string GetExtraData = "select ChangeHash from "+DatabaseName+".dbo.UserLatestChange where "+DatabaseName+".dbo.UserLatestChange.UserID=\'" + userID+"\'";
            string QueryString = @"Insert into " + DatabaseName + ".dbo.UserLatestChange (UserID,ChangeHash,LastChange,LastUsedID) values (\'" + userID + "\','" + LatestHash + "', \'" + dateData.ToString() + "\' ," + LastUserID + ")";
            QueryString += GetExtraData;
            
            try
            {
                Wagtap.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


            string changeHash = "";
            try
            {
                SqlDataReader myReader = null;
                SqlCommand InserUName_UPwd = new SqlCommand(QueryString, Wagtap);

                //'"select ID from DatabaseWaggy.dbo.UserLog where DatabaseWaggy.dbo.UserLog.UserName = '" + UserName + "' and DatabaseWaggy.dbo.UserLog.Password = '" + Password + "'", Wagtap);
                myReader =await InserUName_UPwd.ExecuteReaderAsync(); //ExecuteReader();

                while (myReader.Read())
                {
                    changeHash = myReader["ChangeHash"].ToString();
                }
                myReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            if (!string.IsNullOrEmpty(changeHash))
            {
                //if (changeHash == LatestHash)
                {
                    retValue = true;
                }
            }
            Wagtap.Close();
            
            
            return retValue;

            

        }

        async public Task<Tuple<string, CustomErrors>> RegisterUser(string FirstName, string LastName, string Email)//, string UserName, string Password)
        {
            Tuple<string, CustomErrors> retValue;
            try
            {
                Wagtap.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


            string ID = "";
            try
            {
                SqlDataReader myReader = null;
                SqlCommand InserUName_UPwd = new SqlCommand("Insert into DatabaseWaggy.dbo.UserLog (UserName,Password,Active) values ('" + this.UserName + "','" + this.UserPassword + "','" + 1 + "') select ID from DatabaseWaggy.dbo.UserLog where DatabaseWaggy.dbo.UserLog.UserName='" + this.UserName + "'", Wagtap);
                //int ID_NUmb = 0;
                string ID_NUmb = "";

                //'"select ID from DatabaseWaggy.dbo.UserLog where DatabaseWaggy.dbo.UserLog.UserName = '" + UserName + "' and DatabaseWaggy.dbo.UserLog.Password = '" + Password + "'", Wagtap);
                myReader  = await InserUName_UPwd.ExecuteReaderAsync();
                //myReader = InserUName_UPwd.ExecuteReader();

                while (myReader.Read())
                {
                    ID = myReader["ID"].ToString();
                    //ID_NUmb = Convert.ToInt32(ID);
                    ID_NUmb = ID;
                    this.ID = ID_NUmb;
                }
                myReader.Close();

                //if (ID_NUmb != 0)
                if (!string.IsNullOrEmpty(ID_NUmb))
                {
                    SqlCommand InsertUserInfo = new SqlCommand("Insert into DatabaseWaggy.dbo.UserInfo (ID,FirstName,LastName,Email) values (" + ID_NUmb + ",'" + FirstName + "','" + LastName + "','" + Email + "');", Wagtap);
                    myReader = InsertUserInfo.ExecuteReader();
                }
                else
                {
                    retValue = new Tuple<string,CustomErrors>("", new CustomErrors(true, "User Already Exist", 10001000));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Wagtap.Close();

            DateTimeOffset myDateTimeOffset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan()); //WebApiConfig.JSStartTime;

            bool latestChange =await CreateLatestChange(this.ID,myDateTimeOffset );

            if (string.IsNullOrEmpty(ID)||!latestChange)
            {
                if (!latestChange)
                {
                    retValue = new Tuple<string, CustomErrors>("", new CustomErrors(true, "Issues registering new user, latestchange", 30001001));
                }
                else
                {
                    retValue = new Tuple<string, CustomErrors>("", new CustomErrors(true, "Issues registering new user", 30001000));
                }
            }
            else
            {
                retValue = new Tuple<string, CustomErrors>(this.ID, new CustomErrors(false, "success"));
            }

            

            return retValue;
        }

        async public Task<bool> WriteLatestData(DateTime referenceTimeOFDay, long LastEventID, string UserID)
        {
            bool retValue = false;
            string LatestHash = encryptString(DateTimeOffset.Now.ToString()).Substring(0,50);
            LatestHash = LatestHash.Replace("\'", "").Replace("\"", "");
            try
            {
                Wagtap.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


            string changeHash = "";
            string GetExtraData = " select ChangeHash from "+DatabaseName+".dbo.UserLatestChange where "+DatabaseName+".dbo.UserLatestChange.UserID=\'" + UserID+"\'";
            string QueryString = @"UPDATE " + DatabaseName + ".dbo.UserLatestChange SET ChangeHash='" + LatestHash + "', LastChange= \'" + referenceTimeOFDay + "\', LastUsedID= " + LastEventID + " WHERE UserID=\'" + UserID + "\'";
            try
            {
                SqlDataReader myReader = null;
                SqlCommand myCommand;

                QueryString += GetExtraData;
                {
                    myCommand = new SqlCommand(QueryString, Wagtap);
                    myReader = myCommand.ExecuteReader();
                    //this.ID = 0;
                    this.ID = "";
                    while (myReader.Read())
                    {
                        changeHash = myReader["ChangeHash"].ToString();
                        retValue = true;
                    }
                    myReader.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            

            Wagtap.Close();

            if (!retValue)
            {
                if (await CreateLatestChange(UserID, referenceTimeOFDay, LastEventID))
                {
                    retValue = await WriteLatestData(referenceTimeOFDay, LastEventID, UserID);
                }
            }
            

            return retValue;
        }


        async public Task<Tuple<bool, string, DateTimeOffset, long>> getLatestChanges(string userID)
        {
            Tuple<bool, string, DateTimeOffset, long> retValue;
            string LatestHash = "";
            DateTimeOffset TImeOfDay = new DateTimeOffset();
            long LastUsedID = 0;
            bool status = false;
            string QUerryString = "select * from " + DatabaseName + ".dbo.UserLatestChange where UserID = \'" + userID + "\'";
            try
            {
                Wagtap.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


            
            try
            {
                SqlDataReader myReader = null;
                SqlCommand myCommand;
                
                {
                    myCommand = new SqlCommand(QUerryString, Wagtap);

                    myReader = await myCommand.ExecuteReaderAsync().ConfigureAwait(false); ;
                    //myReader = myCommand.ExecuteReader();
                    myCommand.CommandText = "";
                    while (myReader.Read())
                    {
                        LatestHash = myReader["ChangeHash"].ToString();
                        TImeOfDay = DateTimeOffset.Parse(myReader["LastChange"].ToString());
                        LastUsedID = Convert.ToInt64(myReader["LastUsedID"].ToString());
                        status = true;
                    }
                    
                    myReader.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


            retValue = new Tuple<bool, string, DateTimeOffset, long>(status, LatestHash, TImeOfDay, LastUsedID);

            Wagtap.Close();
            return retValue;
        }

        static void EncryptPassword(int EntryID)
        {
            try
            {
                SqlDataReader myReader = null;
                try
                {
                    Wagtap.Open();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                
                SqlCommand myCommand;
                {
                    string UnEncryptedPword = "";
                    myCommand = new SqlCommand("select Password from DatabaseWaggy.dbo.UserLog where  DatabaseWaggy.dbo.UserLog.ID = " + EntryID + "", Wagtap);
                    myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        UnEncryptedPword = myReader["Password"].ToString();
                    }
                    myReader.Close();

                    string EncryptedString = encryptString(UnEncryptedPword);
                    EncryptedString = encryptString(EncryptedString);

                    myCommand = new SqlCommand("update DatabaseWaggy.dbo.UserLog set Password =\'" + EncryptedString + "\' where  DatabaseWaggy.dbo.UserLog.ID = " + EntryID + "", Wagtap);
                    myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        UnEncryptedPword = myReader["Password"].ToString();
                    }
                    myReader.Close();

                    Wagtap.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
               //ID = "";
            }

        }

        public static string encryptString(string UserString)
        {
            SHA512Managed ShaEncryption = new SHA512Managed();
            //Encoding.UTF8.GetBytes()
            byte[] result = ShaEncryption.ComputeHash(Encoding.UTF8.GetBytes(UserString));
            UserString = Encoding.UTF8.GetString(result);
            return UserString;
        }

        static void UpdateAllUserPassword()
        {
            try
            {
                Wagtap.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
            try
            {
                SqlDataReader myReader = null;
                SqlCommand myCommand;
                int CurrentID = -1;
                string readID = "";
                {
                    myCommand = new SqlCommand("select ID from DatabaseWaggy.dbo.UserLog", Wagtap);
                    myReader = myCommand.ExecuteReader();
                    myCommand.CommandText = "";
                    int i = 0;
                    List<int> AllIDs = new List<int>();
                    while (myReader.Read())
                    {
                        readID = myReader[0].ToString();
                        CurrentID = Convert.ToInt32(readID);
                        AllIDs.Add(CurrentID);
                    }
                    myReader.Close();
                    Wagtap.Close();
                    foreach (int eachInt in AllIDs)
                    {
                        EncryptPassword(eachInt);
                    }
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                //ID = "";
            }

        }
        
        public CustomErrors deleteUser()
        {
            CustomErrors retValue;

            Tuple<bool,string,string> LoginStatus=LogIn();
            if (!LoginStatus.Item1)
            {
                retValue = new CustomErrors(true, "invalid user",1);
            }

            try
            {
                Wagtap.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


            
            try
            {
                SqlDataReader myReader = null;

                SqlCommand deleteUserInfo = new SqlCommand("DELETE FROM DatabaseWaggy.dbo.UserInfo  where ID=" + ID, Wagtap);
                myReader = deleteUserInfo.ExecuteReader();
                myReader.Close();
                deleteUserInfo = new SqlCommand("DELETE FROM "+DatabaseName+".dbo.UserLatestChange where UserID=\'" + ID+"\'", Wagtap);
                myReader = deleteUserInfo.ExecuteReader();
                myReader.Close();
                deleteUserInfo.CommandText = "DELETE FROM DatabaseWaggy.dbo.UserLog where ID=" + ID + " AND UserName=\'" + UserName + "\'";
                myReader = deleteUserInfo.ExecuteReader();
                retValue = new CustomErrors(false, "success");
                myReader.Close();
            }
            catch (Exception e)
            {
                retValue = new CustomErrors(true, e.ToString(), 30000000);
            }

            Wagtap.Close();

            return retValue;

        }


        static public Tuple<bool,int> doesUserExistInOldDB(string UserName)
        {
            Tuple<bool, int> retValue;
            UserName = UserName.ToLower();
            try
            {
                Wagtap.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            bool found = false;
            int UserID=0;
            string ID = "";
            try
            {
                SqlDataReader myReader = null;
                SqlCommand myCommand;
                
                
                myCommand = new SqlCommand("select * from DatabaseWaggy.dbo.UserLog where DatabaseWaggy.dbo.UserLog.UserName = '" + UserName + "'", Wagtap);
                myReader = myCommand.ExecuteReader();
                myCommand.CommandText = "";
                while (myReader.Read())
                {
                    ID = myReader["ID"].ToString();
                    UserID = Convert.ToInt32(ID);
                }
                myReader.Close();
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                ID = "";
            }

            if (string.IsNullOrEmpty(ID))
            {
                found = false;
            }
            else
            {
                found = true;
            }
            retValue = new Tuple<bool, int>(found, UserID);

            Wagtap.Close();
            return retValue;
        }
    }
}
