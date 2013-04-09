using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using System.Windows.Markup;
using System.Xml;
using System.Globalization;
using System.Runtime.InteropServices;

namespace My24HourTimerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        DateTime FinalDate=new DateTime();
        private TimeSpan TimeLeft = new TimeSpan();
        private TimeSpan TimeTo24HourLeft = new TimeSpan();
        string SleepWakeString = "Sleep_Time_N";
        private void button5_Click(object sender, RoutedEventArgs e)
        {
        }

        private void button5_Click_1(object sender, RoutedEventArgs e)
        {
            string text = textBox1.Text;
            string str2 = textBox5.Text;
            DateTime time = calendar2.SelectedDate.Value;
            bool flag = checkBox2.IsChecked.Value;
            bool flag2 = checkBox3.IsChecked.Value;
            string str3 = textBox3.Text;
            bool flag3 = checkBox5.IsChecked.Value;
            string str4 = textBox4.Text;
            DateTime time2 = calendar1.SelectedDate.Value;
            string str5 = textBox2.Text;
            bool flag4 = checkBox4.IsChecked.Value;
            string str6 = textBox6.Text;
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBox1.IsChecked.Value)
            {
                SleepWakeString = "Sleep_Time_M";
            }
            else
            {
                SleepWakeString = "Sleep_Time_N";
            }
        }

        private void checkBox1_Checked_1(object sender, RoutedEventArgs e)
        {
        }

        private void checkBox2_Checked(object sender, RoutedEventArgs e)
        {
        }

        private DateTime ConvertToDateTime(string StringOfDateTime)
        {
            string[] strArray = StringOfDateTime.Split(new char[] { '|' });
            string[] strArray2 = strArray[0].Split(new char[] { ' ' });
            string[] strArray3 = strArray[1].Split(new char[] { ' ' });
            return new DateTime(Convert.ToInt16(strArray2[0]), Convert.ToInt16(strArray2[1]), Convert.ToInt16(strArray2[2]), Convert.ToInt16(strArray3[0]), Convert.ToInt16(strArray3[1]), Convert.ToInt16(strArray3[2]));
        }

        private void CountDownTimer()
        {
            TimeTo24HourLeft += new TimeSpan(0, 0, -1);
        }

        private CalendarEvent CreateSchedule()
        {
            return new CalendarEvent();
        }

        public void DetectAndCaptureTimeZone(string HtmlString)
        {
            string[] strArray = new string[4];
            long num = 0L;
            string[] strArray2 = new string[] { "US/Pacific Time", "US/Mountain Time", "US/Central Time", "US/Eastern Time" };
            while (num < HtmlString.Length)
            {
                num += 1L;
            }
        }

        public string GetCurrentTextOfFile(string FileDirectory)
        {
            string str2;
            try
            {
                using (StreamReader reader = new StreamReader(FileDirectory))
                {
                    str2 = reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                str2 = "The file could not be read";
            }
            return str2;
        }

        public DateTime getCurrentTimeFromInternet()
        {
            string address = "http://www.worldtimeserver.com/current_time_in_US-CO.aspx";
            WebClient client = new WebClient();
            string input = Regex.Replace(client.DownloadString(address).ToString(), @"\s", "");
            Match match = Regex.Match(input, "[0-9]+:[0-9]+[a|p]m", RegexOptions.IgnoreCase);
            Match match2 = Regex.Match(input, "[a-zA-Z]+,[a-zA-Z]+[0-9]+,[0-9]+", RegexOptions.IgnoreCase);
            string arrayOfTime = match.Groups[0].Value.ToUpper();
            string[] arrayOfDate = match2.Groups[0].Value.ToUpper().Split(new char[] { ',' });
            return RetrieveDate(arrayOfDate, arrayOfTime);
        }

        public DateTime GetLastTimeStamp(string MatchingStamp)
        {
            string[] strArray = GetCurrentTextOfFile(@"..\WriteLines.txt").Split(new char[] { '\n' });
            TimeSpan span = new TimeSpan(0, 0, 0, 0, 0);
            for (int i = strArray.Length - 1; i >= 0; i--)
            {
                string[] strArray2 = strArray[i].Split(new char[] { '|' });
                if (strArray2[0] == MatchingStamp)
                {
                    return ConvertToDateTime(strArray2[1] + "|" + strArray2[2]);
                }
            }
            return new DateTime();
        }

        public TimeSpan GetLatestSleepDifference()
        {
            string[] strArray = GetCurrentTextOfFile(@"..\WriteLines.txt").Split(new char[] { '\n' });
            bool flag = false;
            DateTime time = new DateTime();
            for (int i = strArray.Length - 1; i >= 0; i--)
            {
                string[] strArray2 = strArray[i].Split(new char[] { '|' });
                if (strArray2[0] == "Wake_Time")
                {
                    flag = true;
                    time = ConvertToDateTime(strArray2[1] + "|" + strArray2[2]);
                }
                if (strArray2[0] == "Sleep_Time")
                {
                    if (!flag)
                    {
                        return new TimeSpan(0, 0, 0, 0, 0);
                    }
                    DateTime time2 = ConvertToDateTime(strArray2[1] + "|" + strArray2[2]);
                    return (TimeSpan)(time - time2);
                }
            }
            return new TimeSpan(0, 0, 0, 0, 0);
        }

        public int getTimeDifference(DateTime Time1, DateTime Time2)
        {
            TimeSpan span = (TimeSpan)(Time2 - Time1);
            return span.Seconds;
        }

        public TimeSpan GetTotalSleepIn24Difference()
        {
            string[] strArray = GetCurrentTextOfFile(@"..\WriteLines.txt").Split(new char[] { '\n' });
            bool flag = false;
            TimeSpan span = new TimeSpan(0, 0, 0, 0, 0);
            DateTime time = new DateTime();
            for (int i = strArray.Length - 1; i >= 0; i--)
            {
                DateTime time2;
                TimeSpan span2;
                string[] strArray2 = strArray[i].Split(new char[] { '|' });
                if (strArray2[0] == "Wake_Time")
                {
                    flag = true;
                    time = ConvertToDateTime(strArray2[1] + "|" + strArray2[2]);
                }
                if (strArray2[0] == "Sleep_Time_M")
                {
                    if (!flag)
                    {
                        return new TimeSpan(0, 0, 0, 0, 0);
                    }
                    time2 = ConvertToDateTime(strArray2[1] + "|" + strArray2[2]);
                    span2 = (TimeSpan)(time - time2);
                    return (span + span2);
                }
                if (strArray2[0] == "Sleep_Time_N")
                {
                    if (!flag)
                    {
                        return new TimeSpan(0, 0, 0, 0, 0);
                    }
                    time2 = ConvertToDateTime(strArray2[1] + "|" + strArray2[2]);
                    span2 = (TimeSpan)(time - time2);
                    span += span2;
                }
            }
            return new TimeSpan(0, 0, 0, 0, 0);
        }

        /*public void InitializeComponent()
        {
            if (!_contentLoaded)
            {
                _contentLoaded = true;
                Uri resourceLocator = new Uri("/My24HourTimerWPF;component/mainwindow.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }*/

        private void OnTimedEvent(object sender, EventArgs e)
        {
            textBlock1.Text = TimeTo24HourLeft.ToString();
            CountDownTimer();
        }

        public DateTime RetrieveDate(string[] ArrayOfDate, string ArrayOfTime)
        {
            //Regex.Match(ArrayOfDate[1],"a-zA-Z",1)
            string oldValue = Regex.Match(ArrayOfDate[1], "[a-zA-Z]+", RegexOptions.IgnoreCase).Groups[0].Value.ToUpper();
            ArrayOfDate[1] = ArrayOfDate[1].Replace(oldValue, "");
            Match match2 = Regex.Match(ArrayOfTime, "[a-zA-Z]", RegexOptions.IgnoreCase);
            Match match3 = Regex.Match(ArrayOfTime, "[0-9]+", RegexOptions.IgnoreCase);
            Match match4 = Regex.Match(ArrayOfTime, ":[0-9]+", RegexOptions.IgnoreCase);
            string str2 = match3.Groups[0].Value.ToUpper();
            string str3 = match4.Groups[0].Value.ToUpper().Replace(":", "");
            int hour = Convert.ToInt16(str2);
            int minute = Convert.ToInt16(str3);
            if (match2.Groups[0].Value.ToUpper() == "P")
            {
                if (hour != 12)
                {
                    hour += 12;
                }
            }
            else if (hour == 12)
            {
                hour = 0;
            }
            int month = 1;
            switch (oldValue)
            {
                case "JANUARY":
                    break;

                case "FEBRUARY":
                    month++;
                    break;

                case "MARCH":
                    month += 2;
                    break;

                case "APRIL":
                    month += 3;
                    break;

                case "MAY":
                    month += 4;
                    break;

                case "JUNE":
                    month += 5;
                    break;

                case "JULY":
                    month += 6;
                    break;

                case "AUGUST":
                    month += 7;
                    break;

                case "SEPTEMBER":
                    month += 8;
                    break;

                case "OCTOBER":
                    month += 9;
                    break;

                case "NOVEMBER":
                    month += 10;
                    break;

                case "DECEMBER":
                    month += 11;
                    break;
            }
            return new DateTime(Convert.ToInt16(ArrayOfDate[2]), month, Convert.ToInt16(ArrayOfDate[1]), hour, minute, 0);
        }




        public DateTime RetrieveStoredTime()
        {
            string[] strArray = File.ReadAllLines(@"..\WriteLines2.txt");
            return new DateTime(Convert.ToInt16(strArray[0]), Convert.ToInt16(strArray[1]), Convert.ToInt16(strArray[2]), Convert.ToInt16(strArray[3]), Convert.ToInt16(strArray[4]), Convert.ToInt16(strArray[5]));
        }






        private void UpdateTimerDiv(TimeSpan TimeLeft, TextBlock TextBlockUpdate)
        {
            TextBlockUpdate.Text = TimeLeft.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        // Nested Types
        public enum Months
        {
            JANUARY,
            FEBRUARY,
            MARCH,
            APRIL,
            MAY,
            JUNE,
            JULY,
            AUGUST,
            SEPTEMBER,
            OCTOBER,
            NOVEMBER,
            DECEMBER
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DateTime newSystemTime = getCurrentTimeFromInternet();
            newSystemTime.AddMilliseconds(0.0);
            textBlock1.Text = newSystemTime.ToString();
            SystemTimeUpdate update = new SystemTimeUpdate(newSystemTime);
            File.WriteAllText(@"..\WriteLines.txt", GetCurrentTextOfFile(@"..\WriteLines.txt") + string.Concat(new object[] { SleepWakeString, "|", newSystemTime.Year, " ", newSystemTime.Month, " ", newSystemTime.Day, "|", newSystemTime.Hour, " ", newSystemTime.Minute, " ", newSystemTime.Second, "\n" }));
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            DateTime newSystemTime = getCurrentTimeFromInternet();
            newSystemTime.AddMilliseconds(0.0);
            DateTime time3 = GetLastTimeStamp("Sleep_Time_M").AddDays(1.0);
            SystemTimeUpdate update = new SystemTimeUpdate(newSystemTime);
            FinalDate = time3;
            DateTime now = DateTime.Now;
            now = new DateTime(now.Ticks - (now.Ticks % 0x989680L), now.Kind);
            FinalDate = new DateTime(FinalDate.Ticks - (FinalDate.Ticks % 0x989680L), FinalDate.Kind);
            TimeTo24HourLeft = (TimeSpan)(FinalDate - now);
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            timer.Tick += new EventHandler(OnTimedEvent);
            timer.Start();
            CountDownTimer();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            DateTime newSystemTime = getCurrentTimeFromInternet();
            newSystemTime.AddMilliseconds(0.0);
            textBlock1.Text = newSystemTime.ToString();
            SystemTimeUpdate update = new SystemTimeUpdate(newSystemTime);
            File.WriteAllText(@"..\WriteLines.txt", GetCurrentTextOfFile(@"..\WriteLines.txt") + string.Concat(new object[] { "Wake_Time|", newSystemTime.Year, " ", newSystemTime.Month, " ", newSystemTime.Day, "|", newSystemTime.Hour, " ", newSystemTime.Minute, " ", newSystemTime.Second, "\n" }));
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            DateTime newSystemTime = getCurrentTimeFromInternet();
            newSystemTime.AddMilliseconds(0.0);
            TimeSpan span = GetTotalSleepIn24Difference();
            textBlock1.Text = string.Concat(new object[] { span.Days, ":", span.Hours, ":", span.Minutes, ":", span.Seconds });
            SystemTimeUpdate update = new SystemTimeUpdate(newSystemTime);
            string currentTextOfFile = GetCurrentTextOfFile(@"..\WriteLines.txt");
            string[] strArray = currentTextOfFile.Split(new char[] { '\n' });
            File.WriteAllText(@"..\WriteLines.txt", currentTextOfFile + string.Concat(new object[] { "Log_Check|", newSystemTime.Year, " ", newSystemTime.Month, " ", newSystemTime.Day, "|", newSystemTime.Hour, " ", newSystemTime.Minute, " ", newSystemTime.Second, "\n" }));
        }

        private void button5_Click_2(object sender, RoutedEventArgs e)
        {
            string eventName = textBox1.Text;
            string eventStartTime = textBox5.Text;
            DateTime time = calendar2.SelectedDate.Value;
            bool flag = checkBox2.IsChecked.Value;
            bool flag2 = checkBox3.IsChecked.Value;
            string str3 = textBox3.Text;
            bool flag3 = checkBox5.IsChecked.Value;
            string str4 = textBox4.Text;
            DateTime time2 = calendar1.SelectedDate.Value;
            string str5 = textBox2.Text;
            bool flag4 = checkBox4.IsChecked.Value;
            string str6 = textBox6.Text;
        }


        private CalendarEvent CreateSchedule()
        {
            return new CalendarEvent();
        }

 


    }


    
    public class CalendarEvent
    {
        // Fields
        protected DateTime DeadlineDateTime;
        protected TimeSpan Duration;
        protected string eName;
        protected DateTime EndDateTime;
        protected TimeSpan eTimeSpan;
        protected TimeSpan PreDeadline;
        protected TimeSpan PrepTime;
        protected int Priority;
        protected bool Repetition;
        protected bool RigidSchedule;
        protected int Splits;
        protected DateTime StartDateTime;
        protected int TimePerSplit;
        public static int UniqueID;

    }



 


    public class SubSchedule : CalendarEvent
    {
        public static int SubUniqueID = 0;
    }
    public  class SystemTimeUpdate
    {
        /// <summary>
        /// SYSTEMTIME structure with some useful methods
        /// </summary>
        /// 
        public SystemTimeUpdate()
        { 
        
        }
        public SystemTimeUpdate(DateTime NewSystemTime)
        {
            UpdateSystemTime(NewSystemTime);
        }
        public struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;

            /// <summary>
            /// Convert form System.DateTime
            /// </summary>
            /// <param name="time"></param>
            public void FromDateTime(DateTime time)
            {
                wYear = (ushort)time.Year;
                wMonth = (ushort)time.Month;
                wDayOfWeek = (ushort)time.DayOfWeek;
                wDay = (ushort)time.Day;
                wHour = (ushort)time.Hour;
                wMinute = (ushort)time.Minute;
                wSecond = (ushort)time.Second;
                wMilliseconds = (ushort)time.Millisecond;
            }

            /// <summary>
            /// Convert to System.DateTime
            /// </summary>
            /// <returns></returns>
            public DateTime ToDateTime()
            {
                return new DateTime(wYear, wMonth, wDay, wHour, wMinute, wSecond, wMilliseconds);
            }
            /// <summary>
            /// STATIC: Convert to System.DateTime
            /// </summary>
            /// <param name="time"></param>
            /// <returns></returns>
            public static DateTime ToDateTime(SYSTEMTIME time)
            {
                return time.ToDateTime();
            }
        }
        



        //SetLocalTime C# Signature
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool SetLocalTime(ref SYSTEMTIME Time);
        /*{ return false; }*/

        //Example
        public static void UpdateSystemTime(DateTime NewTime)
        {

            SYSTEMTIME st = new SYSTEMTIME();
            st.FromDateTime(NewTime);
            //Call Win32 API to set time
            SetLocalTime(ref st);
        }
    }


    public class MyTime
    {
        int CurrentTimeInSeconds = 0;
        //int InitialTime = 
        public int Time
        {
            get 
            {
                return CurrentTimeInSeconds;
            }
            set
            {
                CurrentTimeInSeconds = value;
            }
        }
    }
}
