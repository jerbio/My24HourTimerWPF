using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace My24HourTimerWPF
{

    class DBControl
    {
        /*
        static SqlConnection Wagtap = new SqlConnection("user id=wagtap;" +
                                   "password=Tagwapadmin001;server=OLUJEROME-PC\\WAGTAPSYS;" +
                                   "Trusted_Connection=yes;" +
                                   "database=WagtapUserAccounts; " +
                                   "connection timeout=30");
        */
        static SqlConnection Wagtap = new SqlConnection("Server=tcp:gjjadsh2tt.database.windows.net,1433;Database=DatabaseWaggy;User ID=wagtap@gjjadsh2tt;Password=Tagwapadminazure007#;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;");

        int ID;
        string UserName;
        string UserPassword;


        public DBControl()
        {

        }

        public DBControl(string UserName, string Password)
        {
            this.UserName = UserName;
            this.UserPassword = Password;
            ID = 0;
        }

        public DBControl(string UserName, int UserID)
        {
            this.UserName = UserName;
            ID = UserID;
        }

        public Tuple<bool, int,string> LogIn()//(string UserName, string Password)
        {
            Tuple<bool, int,string> retValue;
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
                if (this.ID == 0)//checks if ID has been initialized
                {
                    myCommand = new SqlCommand("select ID from DatabaseWaggy.dbo.UserLog where DatabaseWaggy.dbo.UserLog.UserName = '" + UserName + "' and DatabaseWaggy.dbo.UserLog.Password = '" + UserPassword + "'", Wagtap);
                    myReader = myCommand.ExecuteReader();
                    myCommand.CommandText = "";
                    while (myReader.Read())
                    {
                        ID = myReader["ID"].ToString();
                        this.ID = Convert.ToInt32(ID);
                    }
                    myReader.Close();
                    if (this.ID != 0)
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
                    this.ID = 0;
                    ID = "";
                    while (myReader.Read())
                    {
                        ID = myReader["ID"].ToString();
                        this.ID = Convert.ToInt32(ID);
                    }
                    myReader.Close();
                    if (this.ID != 0)
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
                retValue = new Tuple<bool, int,string>(false, 0,"");
            }
            else
            {
                retValue = new Tuple<bool, int, string>(true, this.ID, NamerOfUser);
            }

            Wagtap.Close();

            return retValue;
        }

        public Tuple<int, CustomErrors> RegisterUser(string FirstName, string LastName, string Email, string UserName, string Password)
        {
            Tuple<int, CustomErrors> retValue;
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
                //Insert into WagtapUserAccounts.dbo.UserLog (UserName,Password) values ('LiliUN','LiliPwd') Select ID from WagtapUserAccounts.dbo.UserLog where WagtapUserAccounts.dbo.UserLog.UserName='LiliUN';

                SqlCommand InserUName_UPwd = new SqlCommand("Insert into DatabaseWaggy.dbo.UserLog (UserName,Password,Active) values ('" + UserName + "','" + Password + "','" + 1 + "') select ID from DatabaseWaggy.dbo.UserLog where DatabaseWaggy.dbo.UserLog.UserName='" + UserName + "'", Wagtap);
                int ID_NUmb = 0;

                //'"select ID from DatabaseWaggy.dbo.UserLog where DatabaseWaggy.dbo.UserLog.UserName = '" + UserName + "' and DatabaseWaggy.dbo.UserLog.Password = '" + Password + "'", Wagtap);
                myReader = InserUName_UPwd.ExecuteReader();

                while (myReader.Read())
                {
                    ID = myReader["ID"].ToString();
                    ID_NUmb = Convert.ToInt32(ID);
                    this.ID = ID_NUmb;
                }
                myReader.Close();

                if (ID_NUmb != 0)
                {
                    SqlCommand InsertUserInfo = new SqlCommand("Insert into DatabaseWaggy.dbo.UserInfo (ID,FirstName,LastName,Email) values (" + ID_NUmb + ",'" + FirstName + "','" + LastName + "','" + Email + "');", Wagtap);
                    myReader = InsertUserInfo.ExecuteReader();
                }
                else
                {
                    retValue = new Tuple<int,CustomErrors>(0, new CustomErrors(true, "User Already Exist", 10001000));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            if (string.IsNullOrEmpty(ID))
            {
                retValue = new Tuple<int, CustomErrors>(0, new CustomErrors(true, "Issues registering new user", 30001000));
            }
            else
            {
                retValue = new Tuple<int, CustomErrors>(this.ID, new CustomErrors(false, "success"));
            }

            Wagtap.Close();

            return retValue;
        }


        public CustomErrors deleteUser()
        {
            CustomErrors retValue;

            Tuple<bool,int,string> LoginStatus=LogIn();
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
    }
}
