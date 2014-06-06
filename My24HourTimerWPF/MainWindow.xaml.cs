﻿using System;
using System.Diagnostics;
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
using System.Collections.Generic;
using Google.Maps.Geocoding;
using WinForms = System.Windows.Forms;




//using Microsoft.Office.Interop.Outlook;

namespace My24HourTimerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        Schedule MySchedule;
        static public uint LastID;
        public MainWindow()
        {
            InitializeComponent();

        }

        static class ProcrastinateComboBox
        {
            static public List<ComboData> Data;
            static ProcrastinateComboBox()
            {
                            
            }

            static public ComboBox PopulateComboBox(int Begin, int End, int Increment, ComboBox ComboObj)
            {
                Data = new System.Collections.Generic.List<ComboData>();
                while (Begin <= End)
                {
                    ComboData newItem = new ComboData() { Name = Begin, Value = Begin };
                    Data.Add(newItem);
                    Begin += Increment;
                }

                ComboObj.ItemsSource = Data;
                ComboObj.DisplayMemberPath = "Value";

                return ComboObj;
            }

            public class ComboData
            {
                public int Name { get; set; }
                public int Value { get; set; }
            }
        }

        DateTime FinalDate=new DateTime();
        private TimeSpan TimeLeft = new TimeSpan();
        private TimeSpan TimeTo24HourLeft = new TimeSpan();
        string SleepWakeString = "Sleep_Time_N";
        private void button5_Click(object sender, RoutedEventArgs e)
        {
        }



        public string PreceedingSplitString = 0.ToString();
        public string PreceedingDurationString= 0.ToString();

        private void button5_Click_1(object sender, RoutedEventArgs e)
        {
            string text = textBox1.Text;
            string str2 = textBox5.Text;
            datePicker1.SelectedDate = DateTime.Now;
            datePicker2.SelectedDate = DateTime.Now;
            DateTime time = datePicker1.SelectedDate.Value;
            bool flag = checkBox2.IsChecked.Value;
            bool flag2 = checkBox3.IsChecked.Value;
            string str3 = textBox3.Text;
            bool flag3 = checkBox5.IsChecked.Value;
            string str4 = textBox4.Text;
            DateTime time2 = datePicker2.SelectedDate.Value;
            string str5 = textBox2.Text;
            bool DefaultPreDeadlineFlag = checkBox4.IsChecked.Value;
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
            if (checkBox1.IsChecked.Value)
            {
                SleepWakeString="Sleep_Time_M";
            }
            else
            {
                SleepWakeString="Sleep_Time_N";
            }
        }

        private void checkBox2_Checked(object sender, RoutedEventArgs e)
        {
        }

        public static DateTime ConvertToDateTime(string StringOfDateTime)
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
            BusyTimeLine NextActivity = MySchedule.NextActivity;
            DateTime now = DateTime.Now;
            if (NextActivity != null)
            {
                textBlock2.Text = "Next Activity is : " + MySchedule.getCalendarEvent(NextActivity.TimeLineID).Name;
                FinalDate = NextActivity.Start;
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
                return;
            }
            textBlock2.Text = "Next Activity is : Time To Next 24 Hour Count Down";
            FinalDate = time3;
            now = new DateTime(now.Ticks - (now.Ticks % 0x989680L), now.Kind);
            FinalDate = new DateTime(FinalDate.Ticks - (FinalDate.Ticks % 0x989680L), FinalDate.Kind);
            TimeTo24HourLeft = (TimeSpan)(FinalDate - now);
            DispatcherTimer timer2 = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            timer2.Tick += new EventHandler(OnTimedEvent);
            timer2.Start();
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
            string LocationString  = textBox8.Text.Trim();
            /*if (LocationString != "")
            {
                eventName += "," + LocationString;
            }*/

            
            
            
            DateTime CurrentTimeOfExecution =DateTime.Now;
            string eventStartTime = textBox5.Text;
            string locationInformation = textBox8.Text;
            DateTime eventStartDate = (DateTime)datePicker1.SelectedDate.Value;
            string eventEndTime = textBox7.Text;
            DateTime eventEndDate = (DateTime)datePicker2.SelectedDate.Value;
            bool EventRepetitionflag = checkBox2.IsChecked.Value;
            bool DefaultPrepTimeflag = checkBox3.IsChecked.Value;
            string eventPrepTime = textBox3.Text;
            bool RigidScheduleFlag = checkBox5.IsChecked.Value;
            string RepeatFrequency = comboBox2.Text;
            string EventDuration = textBox4.Text;
            string eventSplit = textBox2.Text;
            bool DefaultPreDeadlineFlag = checkBox4.IsChecked.Value;
            string PreDeadlineTime = textBox6.Text;
            eventStartTime=eventStartTime.Trim();
            eventStartTime=eventStartTime.Replace(" ", string.Empty);
            if (eventStartTime == "")
            {
                eventStartTime = CurrentTimeOfExecution.ToString();
                string[] TempString = eventStartTime.Split(' ');
                eventStartTime =TempString[1]+TempString[2];
            }
            //This attempts to detect invalid inputs for start time values 
            string[] TimeElements = CalendarEvent.convertTimeToMilitary(eventStartTime).Split(':');
            DateTime EnteredDateTime = new DateTime(eventStartDate.Year, eventStartDate.Month, eventStartDate.Day, Convert.ToInt32(TimeElements[0]), Convert.ToInt32(TimeElements[1]), 0);
            //if checks for StartDateTime
            if (EnteredDateTime < DateTime.Now)
            {
                //DateTime Now=DateTime.Now;
                //MessageBox.Show("Please Adjust Your Start Date, Its less than the current time:");
                //return;
            }

            if (eventEndTime == "")
            {
                DateTime EventEndDateTime = new DateTime(eventEndDate.Year, eventEndDate.Month, eventEndDate.Day, EnteredDateTime.Hour, EnteredDateTime.Minute, EnteredDateTime.Second);

                eventEndTime = EventEndDateTime.ToString();
                //eventEndDate
                //MessageBox.Show("Please Type EndTime in The Format: HH:MM A/PM");
                //return;
            }
            TimeSpan TestTimeSpan = new TimeSpan();
            bool RigidFlag = false;
            bool RepetitionFlag = false;
            Repetition MyRepetition=new Repetition();
            if (checkBox5.IsChecked.Value)
            {
                RigidFlag = true;
            }
            DateTime CurrentNow = DateTime.Now;
            DateTime RepeatStart = CurrentNow;
            DateTime RepeatEnd=RepeatStart;

            if (checkBox2.IsChecked.Value)
            {
                //RepeatStart = (DateTime)calendar3.SelectedDate.Value;
                RepeatStart = DateTime.Parse(eventStartTime);
                RepeatEnd = (DateTime)calendar4.SelectedDate.Value;
                //RepeatEnd = (DateTime.Now).AddDays(7);
                RepetitionFlag = true;
                MyRepetition = new Repetition(RepetitionFlag, new TimeLine(RepeatStart, RepeatEnd), RepeatFrequency);
            }

            CustomErrors ErrorCheck = ValidateInputValues(EventDuration, eventStartTime, eventStartDate.ToString(), eventEndTime, eventEndDate.ToString(), RepeatStart.ToString(), RepeatEnd.ToString(), PreDeadlineTime, eventSplit, eventPrepTime, CurrentNow);

            if (!ErrorCheck.Status)
            { 
                //MessageBox.Show(ErrorCheck.Message);
                return;
                //
            }
            //C6RXEZ             
            Location var0 = new Location(textBox8.Text);

            //CalendarEvent ScheduleUpdated = CreateSchedule(eventName, eventStartTime, eventStartDate, eventEndTime, eventEndDate, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionflag, DefaultPreDeadlineFlag, RigidScheduleFlag, eventPrepTime, DefaultPreDeadlineFlag);
            CalendarEvent ScheduleUpdated = new CalendarEvent(eventName, eventStartTime, eventStartDate, eventEndTime, eventEndDate, eventSplit, PreDeadlineTime, EventDuration, MyRepetition, DefaultPreDeadlineFlag, RigidFlag, eventPrepTime, DefaultPreDeadlineFlag, var0,true);
            ScheduleUpdated.Repeat.PopulateRepetitionParameters(ScheduleUpdated);
            textBlock9.Text = "...Loading";
            Stopwatch snugarrayTester = new Stopwatch();
            snugarrayTester.Start();
            CustomErrors ScheduleUpdateMessage = MySchedule.AddToSchedule(ScheduleUpdated);
            snugarrayTester.Stop();
            //MessageBox.Show("It took " + snugarrayTester.ElapsedMilliseconds.ToString() + "ms max thread count is ");

            if (!ScheduleUpdateMessage.Status)
            {
                textBlock9.Text = "Schedule Updated with " + ScheduleUpdated.Name;
                if (ScheduleUpdateMessage.Status)
                {
                    textBlock9.Text = ScheduleUpdateMessage.Message;
                }
            }

            else
            {
                textBlock9.Text = "Failed to update Schedule" + ScheduleUpdated.Name;
                //MessageBox.Show(ScheduleUpdateMessage.Message);
            }
                
        }



        public static CustomErrors ValidateInputValues(string ActiveDuration, string StartTimeEntry, string StartDateEntry, string EndTimeEntry, string EndDateEntry, string RepeatStart, string RepeatEnd, string PredeadlineTime, string NumberOfSplits, string PrepTime, DateTime PassedNow)
        {
            TimeSpan ActiveDurationTimeSpan = TimeSpan.Parse(ActiveDuration);
            TimeSpan PrepTimeTimeSpan = TimeSpan.Parse(PrepTime);


            DateTime StartTimeDateTime = DateTime.Parse(StartTimeEntry);
            DateTime StartDateTime = DateTime.Parse(StartDateEntry);
            StartDateTime = new DateTime(StartDateTime.Year, StartDateTime.Month, StartDateTime.Day, StartTimeDateTime.Hour, StartTimeDateTime.Minute, StartTimeDateTime.Second);
            string[] StartDateArray = StartDateTime.ToString().Split(' ')[1].Split('/');



            DateTime EndTimeDateTime = DateTime.Parse(EndTimeEntry);
            DateTime EndDateTime = DateTime.Parse(EndDateEntry);
            EndDateTime = new DateTime(EndDateTime.Year, EndDateTime.Month, EndDateTime.Day, EndTimeDateTime.Hour, EndTimeDateTime.Minute, EndTimeDateTime.Second);
            DateTime RepeatStartDate = DateTime.Parse(RepeatStart);
            DateTime RepeatEndDate = DateTime.Parse(RepeatEnd);
            TimeSpan PreDeadlineTimeSpan=TimeSpan.Parse(PredeadlineTime);
            uint SplitCount = 1;

            //MessageBox.Show((EndDateTime - StartDateTime).Days.ToString());

            if (!uint.TryParse(NumberOfSplits.Trim(), out SplitCount))//checks to see if Number Of splits is integer
            {
                return new CustomErrors(false, "Please Check Number Split");
            }

            if (!uint.TryParse(NumberOfSplits.Trim(), out SplitCount))
            {
                return new CustomErrors(false, "Please Check Number Split");
            }

            if (ActiveDuration.Trim() != "00:00:00")//checks if Active Duration has valid input
            {
                if (ActiveDurationTimeSpan.ToString() == "00:00:00")
                {
                    return new CustomErrors(false, "Invalid Duration Input");
                }
            }

            if (ActiveDurationTimeSpan > (EndDateTime - StartDateTime))
            {
                return new CustomErrors(false, "Please Check your active duration, it is longer than the time span available between Start time and End Time");
            }

            if (PrepTime.Trim() != "00:00:00")//checks if Active Duration has valid input
            {
                if (PrepTimeTimeSpan.ToString() == "00:00:00")
                {
                    //return new CustomErrors(false, "Invalid PrepTimeTimeSpan Input");
                }
            }

            DateTime Hmm = DateTime.Now;
            


            /*if (StartTime == null)//checks if the value is valid
            {
                return new CustomErrors(false, "Please Check your Start Time Input");
            }
            if (EndTime == null)
            {
                return new CustomErrors(false, "Please Check your End Time Input");
            }
            */

            if (RepeatEndDate < RepeatStartDate)// checks if repeat start date is greater or later than Repeat End date
            {
                if ((RepeatEndDate == RepeatStartDate) && (RepeatStartDate == PassedNow))
                {
                    ;
                }
                else 
                {
                    return new CustomErrors(false, "Please Check your Repeat EndDate, it cannot be earlier than Repeat StartDate ");
                }
                
            }

            if (EndDateTime <= StartDateTime)// checks if repeat start date is greater or later than Repeat End date
            {
                return new CustomErrors(false, "Please Check your End Date, it cannot be earlier than Start Date ");
            }

            return new CustomErrors(true, "");
        }

        void UpdatePreDealineTime()
        {
            TimeSpan MyCurrentActiveDuration = TimeSpan.Parse(textBox4.Text);
            textBox6.Text=new TimeSpan(((MyCurrentActiveDuration.Seconds) * 1000000)).ToString();
        }

        static public TimeSpan StringToTimeSpan(string myTimeString)//takes String in the format dd.HH:MM:SS.MM
        {
            string[] TimeStringSplit = myTimeString.Split(':');
            TimeSpan FinalTimeSpan;

            if (TimeStringSplit.Length < 2)
            {
                return new TimeSpan();
            }

            if (TimeStringSplit.Length == 2)
            {
                return StringToTimeSpan(TimeStringSplit[0] + ":" + TimeStringSplit[1] + ":00");
            }

            double sec = 0;
            uint min = 0;
            double hour = 0;
            int Index = 0;
            for (; Index < TimeStringSplit.Length; Index++)
            {
                switch (Index)
                {
                    case 0:
                        {
                            if (double.TryParse(TimeStringSplit[Index], out sec))
                            {

                            }
                            else
                            {
                                return new TimeSpan();
                            }
                        }
                        break;
                    case 1:
                        {
                            if (uint.TryParse(TimeStringSplit[Index], out min))
                            {

                            }
                            else
                            {
                                return new TimeSpan();
                            }
                        }
                        break;
                    case 2:
                        {
                            if (double.TryParse(TimeStringSplit[Index], out hour))
                            {
                                int DecimalPart = Convert.ToInt32(TimeStringSplit[Index].Split('.')[1]);
                                int IntPart = Convert.ToInt32(TimeStringSplit[Index].Split('.')[0]);
                                IntPart *= 24;
                                hour = IntPart + DecimalPart;
                            }
                            else
                            {
                                return new TimeSpan();
                            }
                        }
                        break;
                }
            }
            FinalTimeSpan = new TimeSpan((int)hour, (int)min, (int)sec);
            return FinalTimeSpan;
        }

        /*public CalendarEvent CreateSchedule(string Name, string StartTime, DateTime StartDate, string EndTime, DateTime EventEndDate, string eventSplit, string PreDeadlineTime, string EventDuration, bool EventRepetitionflag, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag)
        {
            string MiltaryStartTime = convertTimeToMilitary(StartTime);
            StartDate = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, Convert.ToInt32(MiltaryStartTime.Split(':')[0]), Convert.ToInt32(MiltaryStartTime.Split(':')[1]), 0);
            string MiltaryEndTime = convertTimeToMilitary(EndTime);
            EventEndDate = new DateTime(EventEndDate.Year, EventEndDate.Month, EventEndDate.Day, Convert.ToInt32(MiltaryEndTime.Split(':')[0]), Convert.ToInt32(MiltaryEndTime.Split(':')[1]), 0);
            string []TimeDuration=textBox4.Text.Split(':');
            uint AllMinutes =(uint)((Convert.ToInt32(TimeDuration[0]) * 60) + (Convert.ToInt32(TimeDuration[1])));
            TimeSpan Duration = new TimeSpan((int)(AllMinutes / 60), (int)(AllMinutes % 60), 0);
            int Split =Convert.ToInt32(eventSplit);
            TimeSpan PreDeadline = new TimeSpan(((int)AllMinutes%10)*60);
            TimeSpan PrepTime;
            if (checkBox3.IsChecked.Value)
            {
                 PrepTime= new TimeSpan(15 * 60);
            }
            else
            {
                PrepTime = new TimeSpan(Convert.ToInt32(textBox3.Text));
            }
            CalendarEvent MyCalendarEvent = new CalendarEvent(Name, Duration, StartDate, EventEndDate, PrepTime, PreDeadline, checkBox5.IsChecked.Value, new Repetition(), Split);
            return MyCalendarEvent;
        }
        */

        string convertTimeToMilitary(string TimeString)
        {
            TimeString = TimeString.Replace(" ", "").ToUpper();
            string[] TimeIsolated = TimeString.Split(':');
            int HourInt = Convert.ToInt32(TimeIsolated[0]);
            if (TimeIsolated[1][2] == 'P')
            {
                HourInt = Convert.ToInt32(TimeIsolated[0]);
                HourInt += 12;
            }
            
/*                Replace("PM", "");
            TimeString = TimeString.Replace("AM", "");
            TimeIsolated = TimeString.Split(':');*/
            if ((HourInt % 12) == 0)
            {
                HourInt = HourInt - 12;
            }
            TimeIsolated[0] = HourInt.ToString();
            TimeIsolated[1]=TimeIsolated[1].Substring(0, 2);
            TimeString = TimeIsolated[0] + ":" + TimeIsolated[1];
            
            //TimeString=TimeString.Substring(0, 5);
            return TimeString;
        }

        private void HandleRigidCheckBoxClick(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb.Name == "checkBox5")
            {
                //MessageBox.Show("Hey you clicked checkBox5");
            }
        }

        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /*private void checkBox5_Checked(object sender, RoutedEventArgs e)
        {

        }*/

        private void checkBox5_Checked(object sender, RoutedEventArgs e)
        {
            string eventStartTime = textBox5.Text;
            string eventEndTime = textBox7.Text;
            string[] TimeElements = CalendarEvent.convertTimeToMilitary(eventStartTime).Split(':');
            //string[] TimeElements = CalendarEvent.convertTimeToMilitary(eventStartTime).Split(':');
            DateTime eventStartDate = (DateTime)datePicker1.SelectedDate.Value;
            DateTime eventEndDate = (DateTime)datePicker2.SelectedDate.Value;
            eventStartDate = new DateTime(eventStartDate.Year, eventStartDate.Month, eventStartDate.Day, Convert.ToInt32(TimeElements[0]), Convert.ToInt32(TimeElements[1]), 0);
            TimeElements = CalendarEvent.convertTimeToMilitary(eventEndTime).Split(':');
            eventEndDate = new DateTime(eventEndDate.Year, eventEndDate.Month, eventEndDate.Day, Convert.ToInt32(TimeElements[0]), Convert.ToInt32(TimeElements[1]), 0);
            
            
            if (checkBox5.IsChecked.Value)
            {
                PreceedingSplitString = textBox2.Text;
                PreceedingDurationString = textBox4.Text;
                //MessageBox.Show(PreceedingSplitString);
                textBox2.Text = 1.ToString();
                textBox4.Text = (eventEndDate - eventStartDate).ToString();
                textBox2.IsEnabled = false;
                textBox4.IsEnabled = false;
            }
            else 
            {
                textBox2.Text = PreceedingSplitString;
                textBox4.Text = PreceedingDurationString;
                textBox2.IsEnabled = true;
                textBox4.IsEnabled = true;
                
            }
            
            
        }

        private void checkBox2_Checked_1(object sender, RoutedEventArgs e)
        {
            if (checkBox2.IsChecked.Value)
            {
                grid4.Visibility = System.Windows.Visibility.Visible;
                comboBox2.IsEnabled = true;
            }
            else
            {
                grid4.Visibility = System.Windows.Visibility.Hidden;
                comboBox2.IsEnabled = false;
            }
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {           
            MySchedule.RemoveAllCalendarEventFromLogAndCalendar();
            MySchedule.EmptyMemory();
        }

        private void textBox6_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            MySchedule.removeAllFromOutlook();
            MySchedule.WriteFullScheduleToLogAndOutlook();
        }


        private void delete(object sender, RoutedEventArgs e)
        {
            string EventID = textBox9.Text;
            MySchedule.deleteCalendarEvent(EventID);
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            int ProcrastinateDays = Convert.ToInt16(comboBox4.Text);
            int ProcrastinateHours = Convert.ToInt16(comboBox5.Text);
            int ProcrastinateMins = Convert.ToInt16(comboBox6.Text);
            TimeSpan DelaySpan = new TimeSpan(ProcrastinateDays, ProcrastinateHours, ProcrastinateMins, 50);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage;

            string choicePath = "";
            if (string.IsNullOrEmpty(textBox9.Text))//check for specific id removal account
            {

                DateTime eventStartTime = DateTime.Now;
                
                DateTime eventEndTime = eventStartTime + DelaySpan;


                CalendarEvent ScheduleUpdated = new CalendarEvent("Procrastinate", DelaySpan, eventStartTime, eventEndTime, new TimeSpan(0), new TimeSpan(0), true, new Repetition(), 1, new Location());
                ScheduleUpdated.Repeat.PopulateRepetitionParameters(ScheduleUpdated);
                textBlock9.Text = "...Loading";
                choicePath = "ProcrastinateAll";
                 ScheduleUpdateMessage = MySchedule.Procrastinate(ScheduleUpdated);
                
            }
            else
            {
                choicePath = "ProcrastinateOneEvent";
                ScheduleUpdateMessage=MySchedule.ProcrastinateJustAnEvent(textBox9.Text, DelaySpan);
            }


            

            if (ScheduleUpdateMessage.Item1.Status)//checks for error
            {
                MessageBoxResult result = MessageBox.Show(ScheduleUpdateMessage.Item1.Message, "Schedule Collision, do you want to continue with this collision? ", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    MySchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2);
                }
            }
            else
            {

                MySchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2);

                textBlock9.Text = "Schedule updated no clash detected";
            }

        }

        private void NowButtonClick(object sender, RoutedEventArgs e)
        { 
            string EventID=textBox9.Text.Trim();
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage = MySchedule.SetEventAsNow(EventID);
            if (ScheduleUpdateMessage.Item1.Status)
            {
                switch (ScheduleUpdateMessage.Item1.Code)
                {
                    case 5:
                        {
                            MessageBoxResult result = MessageBox.Show(ScheduleUpdateMessage.Item1.Message, "Do you want to continue with this collision? ", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (result == MessageBoxResult.Yes)
                            {
                                MySchedule.SetEventAsNow(EventID, true); ;
//                                MySchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2);
                            }
                        }
                        break;
                    default://hack alert we need to figure out how to fix this error
                        MySchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2);
                        break;
                }
            }
            else
            {
                MySchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2);
            }

        }

        private void RunEvaluation(object sender, RoutedEventArgs e)
        {
            int NumberOfRetries = Convert.ToInt32(textBox10.Text);
            long[] AllData = new long[NumberOfRetries];
            
            while(--NumberOfRetries>=0)
            {
                UserAccount currentUser = new UserAccount(UserNameTextBox.Text, PasswordTextBox.Text);
                MySchedule = new Schedule(currentUser);
                
                string eventName = textBox1.Text;
                string LocationString = textBox8.Text.Trim();
                /*if (LocationString != "")
                {
                    eventName += "," + LocationString;
                }*/




                DateTime CurrentTimeOfExecution = DateTime.Now;
                string eventStartTime = textBox5.Text;
                string locationInformation = textBox8.Text;
                DateTime eventStartDate = (DateTime)datePicker1.SelectedDate.Value;
                string eventEndTime = textBox7.Text;
                DateTime eventEndDate = (DateTime)datePicker2.SelectedDate.Value;
                bool EventRepetitionflag = checkBox2.IsChecked.Value;
                bool DefaultPrepTimeflag = checkBox3.IsChecked.Value;
                string eventPrepTime = textBox3.Text;
                bool RigidScheduleFlag = checkBox5.IsChecked.Value;
                string RepeatFrequency = comboBox2.Text;
                string EventDuration = textBox4.Text;
                string eventSplit = textBox2.Text;
                bool DefaultPreDeadlineFlag = checkBox4.IsChecked.Value;
                string PreDeadlineTime = textBox6.Text;
                eventStartTime = eventStartTime.Trim();
                eventStartTime = eventStartTime.Replace(" ", string.Empty);
                if (eventStartTime == "")
                {
                    eventStartTime = CurrentTimeOfExecution.ToString();
                    string[] TempString = eventStartTime.Split(' ');
                    eventStartTime = TempString[1] + TempString[2];
                }
                //This attempts to detect invalid inputs for start time values 
                string[] TimeElements = CalendarEvent.convertTimeToMilitary(eventStartTime).Split(':');
                DateTime EnteredDateTime = new DateTime(eventStartDate.Year, eventStartDate.Month, eventStartDate.Day, Convert.ToInt32(TimeElements[0]), Convert.ToInt32(TimeElements[1]), 0);
                //if checks for StartDateTime
                if (EnteredDateTime < DateTime.Now)
                {
                    //DateTime Now=DateTime.Now;
                    //MessageBox.Show("Please Adjust Your Start Date, Its less than the current time:");
                    //return;
                }

                if (eventEndTime == "")
                {
                    DateTime EventEndDateTime = new DateTime(eventEndDate.Year, eventEndDate.Month, eventEndDate.Day, EnteredDateTime.Hour, EnteredDateTime.Minute, EnteredDateTime.Second);

                    eventEndTime = EventEndDateTime.ToString();
                    //eventEndDate
                    //MessageBox.Show("Please Type EndTime in The Format: HH:MM A/PM");
                    //return;
                }
                TimeSpan TestTimeSpan = new TimeSpan();
                bool RigidFlag = false;
                bool RepetitionFlag = false;
                Repetition MyRepetition = new Repetition();
                if (checkBox5.IsChecked.Value)
                {
                    RigidFlag = true;
                }
                DateTime CurrentNow = DateTime.Now;
                DateTime RepeatStart = CurrentNow;
                DateTime RepeatEnd = RepeatStart;

                if (checkBox2.IsChecked.Value)
                {
                    //RepeatStart = (DateTime)calendar3.SelectedDate.Value;
                    RepeatStart = DateTime.Parse(eventStartTime);
                    RepeatEnd = (DateTime)calendar4.SelectedDate.Value;
                    //RepeatEnd = (DateTime.Now).AddDays(7);
                    RepetitionFlag = true;
                    MyRepetition = new Repetition(RepetitionFlag, new TimeLine(RepeatStart, RepeatEnd), RepeatFrequency);
                }

                CustomErrors ErrorCheck = ValidateInputValues(EventDuration, eventStartTime, eventStartDate.ToString(), eventEndTime, eventEndDate.ToString(), RepeatStart.ToString(), RepeatEnd.ToString(), PreDeadlineTime, eventSplit, eventPrepTime, CurrentNow);

                if (!ErrorCheck.Status)
                {
                    //MessageBox.Show(ErrorCheck.Message);
                    return;
                    //
                }
                //C6RXEZ             
                Location var0 = new Location(textBox8.Text);

                //CalendarEvent ScheduleUpdated = CreateSchedule(eventName, eventStartTime, eventStartDate, eventEndTime, eventEndDate, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionflag, DefaultPreDeadlineFlag, RigidScheduleFlag, eventPrepTime, DefaultPreDeadlineFlag);
                CalendarEvent ScheduleUpdated = new CalendarEvent(eventName, eventStartTime, eventStartDate, eventEndTime, eventEndDate, eventSplit, PreDeadlineTime, EventDuration, MyRepetition, DefaultPreDeadlineFlag, RigidFlag, eventPrepTime, DefaultPreDeadlineFlag, var0,true);
                ScheduleUpdated.Repeat.PopulateRepetitionParameters(ScheduleUpdated);
                
                Stopwatch snugarrayTester = new Stopwatch();
                snugarrayTester.Start();
                CustomErrors ScheduleUpdateMessage = MySchedule.AddToSchedule(ScheduleUpdated);
                snugarrayTester.Stop();
                AllData[NumberOfRetries] = snugarrayTester.ElapsedMilliseconds;

            }


            int q= 0;
            double totalTIme = 0;
            for(q=0;q<AllData.Length;q++)
            {
                textBlock9.Text += AllData[q] + ",";
                totalTIme += AllData[q];
            }
            totalTIme /= q;


            textBlock9.Text += "Average is " + totalTIme;
        }

        private void LogInToWagtap()
        {
            UserAccount currentUser = new UserAccount(UserNameTextBox.Text, PasswordTextBox.Text);

            MySchedule = new Schedule(currentUser);
            if (MySchedule.isScheduleLoadSuccessful)
            {
                tabItem2.IsEnabled = true;
                datePicker1.SelectedDate = Schedule.Now.AddDays(7);// DateTime.Now.AddDays(0);
                //datePicker1.SelectedDate = DateTime.Now.AddDays(0);
                //datePicker1.SelectedDate = new DateTime(2013, 11, 20, 0, 0, 0);
                //datePicker2.SelectedDate = DateTime.Now.AddDays(2);
                datePicker2.SelectedDate = Schedule.Now.AddDays(12);//new DateTime(2014, 5, 15, 0, 0, 0);
                calendar4.SelectedDate = DateTime.Now.AddDays(0);
                Random myNumber = new Random();
                int RandomHour = myNumber.Next(0, 24);
                int RandomMinute = myNumber.Next(0, 60);
                textBox4.Text = RandomHour + ":" + RandomMinute;
                textBox4.Text = 16 + ":" + "00";//total time
                textBox2.Text = 7.ToString();//number of splits
                int ProcrastinateStartDay = 0;
                int ProcrastinateEndDay = 365;
                int ProcrastinateStartHour = 0;
                int ProcrastinateEndHour = 24;
                int ProcrastinateStartMin = 0;
                int ProcrastinateEndMin = 60;
                comboBox4 = ProcrastinateComboBox.PopulateComboBox(ProcrastinateStartDay, ProcrastinateEndDay, 1, comboBox4);
                comboBox5 = ProcrastinateComboBox.PopulateComboBox(ProcrastinateStartHour, ProcrastinateEndHour, 1, comboBox5);
                comboBox6 = ProcrastinateComboBox.PopulateComboBox(ProcrastinateStartMin, ProcrastinateEndMin, 1, comboBox6);
                comboBox4.Text = 0.ToString();
                comboBox5.Text = 0.ToString();
                comboBox6.Text = 0.ToString();

#if enableDebugging
#if enableMultithreading            
            MessageBox.Show("Multithreading Enabled");
#else
            MessageBox.Show("Sequential run enabled");
            
            var current = Process.GetCurrentProcess();
            var affinity = current.ProcessorAffinity.ToInt32();
            current.ProcessorAffinity = new IntPtr(affinity & 0x5555);
#endif
#endif

                EventIDGenerator.Initialize((uint)(MySchedule.LastScheduleIDNumber));
            }
            else
            {
                MessageBox.Show("Error loading Schedule please check user password");
            }
        }

        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            LogInToWagtap();
        }

        private void LogInButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            //Register(string FirstName, string LastName, string NickName, string UserName, string PassWord)
            UserAccount newUser = new UserAccount();
            if (newUser.Register(FirstNameRegisterTextBox.Text, LastNameRegisterTextBox.Text, NickNameRegisterTextBox.Text, UserNameRegisterTextBox.Text, PasswordRegisterTextBox.Text))
            {
                UserNameTextBox.Text = UserNameRegisterTextBox.Text;
                PasswordTextBox.Text = PasswordRegisterTextBox.Text;
                LogInToWagtap();
            }
            
        }

        



    }

    public class CustomErrors
    {
        bool Errorstatus;
        string ErrorMessage;
        int ErrorCode;
        public CustomErrors(bool StatusEntry, string MessagEntry,int ErrorCode=0)
        {
            Errorstatus = StatusEntry;
            ErrorMessage = MessagEntry;
            this.ErrorCode = ErrorCode;
        }

        //Error Code 0: No Error
        //Error Code 5: Set Sub event as Now was selected however, The sub event will exceed the bounds of the CalendarEvent

        public bool Status
        {
            get
            {
                return Errorstatus;
            }
        }

        public string Message
        {
            get
            {
                return ErrorMessage;
            }
        }

        public int Code
        {
            get
            {
                return ErrorCode;
            }
        }
    }
    

    public class QuickSort
    {
        private static List<int> ArrayOfElements;
        Dictionary<int, List<DateTime>> DictionaryOfIntAndDateTime;
        
        public QuickSort()
        {
            DictionaryOfIntAndDateTime = new Dictionary<int, List<DateTime>>();
            ArrayOfElements = new List<int>();
        }

        public QuickSort(List<DateTime> MyListOfElements)
        {
            foreach (DateTime MyDateTime in MyListOfElements)
            {
                int MyTicks = (int)(MyDateTime.Ticks);
                try
                {

                    DictionaryOfIntAndDateTime.Add(MyTicks, new List<DateTime>());
                    DictionaryOfIntAndDateTime[MyTicks].Add(MyDateTime);
                }
                catch (Exception e)
                {
                    DictionaryOfIntAndDateTime[MyTicks].Add(MyDateTime);
                }
            }
            ArrayOfElements = DictionaryOfIntAndDateTime.Keys.ToList();
        }

        public DateTime[] QuickSortFunction()
        {
            int[] MyArrayofTicks= QuickSortFunction(ArrayOfElements.ToArray(), 0, (ArrayOfElements.Count - 1), (ArrayOfElements.Count / 2));
            List<DateTime> ListOfSortedDateTime = new List<DateTime>();
            
            foreach (int MyInt in MyArrayofTicks)
            {
                var JustConcatenate = ListOfSortedDateTime.Concat(DictionaryOfIntAndDateTime[MyInt]);
                ListOfSortedDateTime = JustConcatenate.ToList();
            }

            return ListOfSortedDateTime.ToArray();
        }
        public int[] QuickSortFunction(int[] MyArray,int LeftIndexPassed,int RightIndexPassed,int PivotPassed)
        {
            //Console.WriteLine("\n EntryLeft is " + LeftIndexPassed.ToString() + " EntryRight is " + RightIndexPassed.ToString() + " EntryPivot is " + PivotPassed.ToString() + "\n");
            int PivotValue, PivotIndex;
            if ((LeftIndexPassed == RightIndexPassed)||(MyArray.Length<2))
            {
                return MyArray;
            }
            /*foreach (int myint in MyArray)
            {
                Console.Write(myint.ToString() + ",");
            }*/
            //Console.WriteLine("\n Music in me "+MyArray[PivotPassed].ToString()+"\n");
            int LeftValue, LeftIndex=LeftIndexPassed, RightValue, RightIndex=RightIndexPassed, Holder;
            bool Detected = true;
            PivotIndex = PivotPassed;
            RightIndex = GetRightThatsLess(MyArray, PivotIndex, RightIndexPassed);
            LeftIndex = GetLeftThatsGreater(MyArray, PivotIndex, LeftIndexPassed);
            //Console.WriteLine("\n ##Left is " + LeftIndex.ToString() + " Right is " + RightIndex.ToString() + "\n");
            PivotValue = MyArray[PivotIndex];
            Holder = MyArray[LeftIndex];
            MyArray[LeftIndex] = MyArray[RightIndex];
            MyArray[RightIndex] = Holder;
            int middleDifference;
            int NextPivot;
            if (RightIndex == LeftIndex)
            {
                if ((PivotIndex + 1) < (RightIndexPassed + 1))
                {
                    //Console.Write("maybe RIGHT TOP\t");
                    middleDifference=RightIndexPassed - (PivotIndex + 1);
                    NextPivot = (PivotIndex + 1)+(middleDifference / 2);
                    if (middleDifference<=1)
                    {
                        NextPivot = PivotIndex + 1;
                        if (MyArray[RightIndexPassed] < MyArray[NextPivot])
                        {
                            Holder = MyArray[NextPivot];
                            MyArray[NextPivot] = MyArray[RightIndexPassed];
                            MyArray[RightIndexPassed] = Holder;
                            return MyArray;
                        }
                    }
                    MyArray = QuickSortFunction(MyArray, (PivotIndex + 1), RightIndexPassed, NextPivot); //right Partition sort
                }
                else
                {
                    //Console.Write("maybe RIGHT \t");
                    MyArray = QuickSortFunction(MyArray, (PivotIndex), RightIndexPassed, (PivotIndex + ((RightIndexPassed - PivotIndex) / 2))); //right Partition sort
                }
                if ((PivotIndex - 1) > (LeftIndexPassed)-1)
                {
                    //Console.Write("maybe LEFT TOP\t");
                    middleDifference = (PivotIndex - 1) - LeftIndexPassed;
                    NextPivot = LeftIndexPassed + (middleDifference / 2);
                    if (middleDifference <= 1)
                    {
                        NextPivot = PivotIndex - 1;
                        if (MyArray[LeftIndexPassed] > MyArray[NextPivot])
                        {
                            Holder = MyArray[LeftIndexPassed];
                            MyArray[LeftIndexPassed] = MyArray[NextPivot];
                            MyArray[NextPivot] = Holder;
                            return MyArray;
                        }
                    }
                    MyArray = QuickSortFunction(MyArray, LeftIndexPassed, (PivotIndex - 1), NextPivot);//left Partition sort 
                }
                else 
                {
                    //Console.Write("maybe LEFT \t");
                    MyArray = QuickSortFunction(MyArray, LeftIndexPassed, (PivotIndex), (LeftIndexPassed + ((PivotIndex) - LeftIndexPassed) / 2));//left Partition sort 
                }
            }
            else 
            {
                MyArray = QuickSortFunction(MyArray, LeftIndexPassed, RightIndexPassed, PivotPassed);
            }
            return MyArray;
        }

        static int GetLeftThatsGreater(int[] MyArray, int MyPivot,int LeftStartinPosition)
        {
            for (int i = LeftStartinPosition; i <= MyPivot; i++)
            {
                if (MyArray[i] > MyArray[MyPivot])
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static int  GetRightThatsLess(int[] MyArray, int MyPivot, int RightStartinPosition)
        {
            for (int i = RightStartinPosition; i >= MyPivot; i--)
            {
                if (MyArray[i] < MyArray[MyPivot])
                {
                    return i;
                }
            }
            return MyPivot;
        }
    }

    

    
    
    

    public class EventID
    {
        private static int CalendarEvenntLimitIndex = 2;
        string[] LayerID;
        int FullID;
        public EventID(string myLayerID):this(myLayerID.Split('_'))
        { 
            
        }
        public EventID(string[] myLayerID)
        {
            LayerID = myLayerID;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (string eachString in LayerID)
            {
                sb.Append(eachString);
            }

            string currConcat=sb.ToString();
            if (string.IsNullOrEmpty(currConcat))
            {
                FullID = 0;
            }
            else
            {
                FullID = Convert.ToInt32(currConcat);
            }
            
        }

        public string[] ID
        {
            get 
            {
                return LayerID;
            }
        }

        public string getStringIDAtLevel(int LevelIndex)
        {
            int i = 0;
            string StringID = "";
            for (i = 0; (i <= LevelIndex - 1) && (LevelIndex < LayerID.Length); i++)
            {
                StringID += LayerID[i] + "_";
            }
            StringID += LayerID[LevelIndex];
            return StringID;

        }

        public string getLevelID(int Level)
        {
            return LayerID[Level];
        }

        public string getCalendarEventID()
        {
            if (LayerID.Length > CalendarEvenntLimitIndex)
            {
                return getStringIDAtLevel(1);
            }
            else 
            {
                return getStringIDAtLevel(0);
            }
        }

        public override string ToString()
        {
            string IDCombination="";
            if ((LayerID.Length == 1) && (LayerID[0] == ""))//checks if LayerID is empty
            {
                return "";
            }
            foreach (string MyString in LayerID)
            {
                IDCombination += MyString + "_";
            }
            return IDCombination.Substring(0, (IDCombination.Length - 1));
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }


            EventID p = obj as EventID;
            if ((System.Object)p == null)
            {
                return false;
            }

            return (this.FullID == p.FullID);
        }

        public override int GetHashCode()
        {
            return FullID;
        }
    }

    
    public class Repetition
    {
        
        string RepetitionFrequency;
        TimeLine RepetitionRange;
        bool EnableRepeat;
        CalendarEvent[] RepeatingEvents;
        Dictionary<string, CalendarEvent> DictionaryOfIDAndCalendarEvents;
        Location RepeatLocation;

        public Repetition()
        {
            RepetitionFrequency = "";
            RepetitionRange = new TimeLine();
            EnableRepeat = false;
            RepeatingEvents = new CalendarEvent[0];
            RepeatLocation = new Location();
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
        }
        public Repetition(bool EnableFlag ,TimeLine RepetitionRange_Entry, string Frequency)
        {
            RepetitionRange = RepetitionRange_Entry;
            RepetitionFrequency = Frequency.ToUpper();
            EnableRepeat = EnableFlag;
            RepeatLocation = new Location();
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
        }

        //public Repetition(bool EnableFlag,CalendarEvent BaseCalendarEvent,  TimeLine RepetitionRange_Entry, string Frequency)
        public Repetition(bool ReadFromFileEnableFlag, TimeLine ReadFromFileRepetitionRange_Entry, string ReadFromFileFrequency, CalendarEvent[] ReadFromFileRecurringListOfCalendarEvents)
        {
            EnableRepeat = ReadFromFileEnableFlag;
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();

            foreach (CalendarEvent MyRepeatCalendarEvent in ReadFromFileRecurringListOfCalendarEvents)
            {
                DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.ID, MyRepeatCalendarEvent);
            }

            RepeatingEvents = DictionaryOfIDAndCalendarEvents.Values.ToArray();
            RepetitionFrequency = ReadFromFileFrequency;
            RepetitionRange = ReadFromFileRepetitionRange_Entry;
            if (ReadFromFileRecurringListOfCalendarEvents.Length > 0)
            {
                RepeatLocation=ReadFromFileRecurringListOfCalendarEvents[0].myLocation;
            }
        }
        
        /*public Repetition(bool EnableFlag,TimeLine RepetitionRange_Entry, CalendarEvent ParentEvent)
        {
            RepetitionRange = RepetitionRange_Entry;
            RepetitionFrequency = Frequency.ToUpper();
            EnableRepeat = EnableFlag;
            DateTime RepeatCalendarStart = CalendarEventRepeatCalendarStart;
            DateTime RepeatCalendarEnd = CalendarEventRepeatCalendarEnd;
            //RepeatCalendarEnd =IncreaseByFrequency(RepeatCalendarStart, Frequency);
            CalendarEvent MyRepeatCalendarEvent = new CalendarEvent(CalendarEventName, CalendarEventActiveDuration, CalendarEventRepeatCalendarStart, CalendarEventRepeatCalendarEnd, CalendarEventPreparation, CalendarEventPreDeadline, CalendarEventRigid, new Repetition(), CalendarEventNumberOfSplit);//first repeating calendar event
            MyRepeatCalendarEvent = new CalendarEvent(new EventID(MyRepeatCalendarEvent.ID), MyRepeatCalendarEvent.Name, MyRepeatCalendarEvent.ActiveDuration, RepeatCalendarStart, RepeatCalendarEnd, MyRepeatCalendarEvent.Preparation, MyRepeatCalendarEvent.PreDeadline, MyRepeatCalendarEvent.Rigid, MyRepeatCalendarEvent.Repeat, MyRepeatCalendarEvent.NumberOfSplit);
            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();

            for (; MyRepeatCalendarEvent.Start < RepetitionRange_Entry.End; )
            {
                MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                RepeatCalendarStart = IncreaseByFrequency(RepeatCalendarStart, Frequency); ;
                RepeatCalendarEnd = IncreaseByFrequency(RepeatCalendarEnd, Frequency);
                MyRepeatCalendarEvent = new CalendarEvent(new EventID(MyRepeatCalendarEvent.ID), MyRepeatCalendarEvent.Name, MyRepeatCalendarEvent.ActiveDuration, RepeatCalendarStart, RepeatCalendarEnd, MyRepeatCalendarEvent.Preparation, MyRepeatCalendarEvent.PreDeadline, MyRepeatCalendarEvent.Rigid, MyRepeatCalendarEvent.Repeat, MyRepeatCalendarEvent.NumberOfSplit);
            }
            RepeatingEvents = MyArrayOfRepeatingCalendarEvents.ToArray();
        }*/

        public CalendarEvent getCalendarEvent(string RepeatingEventID)
        {
            try { return DictionaryOfIDAndCalendarEvents[RepeatingEventID]; }
            catch
            { 
                return null;
            }

        }

        public void PopulateRepetitionParameters(CalendarEvent MyParentEvent)//this function of repetition, is responsible for populating the repetition object in the passed CalendarEvent.
        {
            if (!MyParentEvent.Repeat.Enable)//Checks if Repetition object is enabled or disabled. If Disabled then just return else continue
            {
                return;
            }
            
            RepetitionRange = MyParentEvent.Repeat.Range;
            RepetitionFrequency = MyParentEvent.Repeat.Frequency;
            EnableRepeat = true;
            DateTime EachRepeatCalendarStart = MyParentEvent.Start;//Start DateTime Object for each recurring Calendar Event
            DateTime EachRepeatCalendarEnd = MyParentEvent.End;//End DateTime Object for each recurring Calendar Event

            EventID MyEventCalendarID = new EventID(MyParentEvent.ID + "_" + EventIDGenerator.generate().ToString());
            CalendarEvent MyRepeatCalendarEvent = new CalendarEvent(MyEventCalendarID, MyParentEvent.Name, MyParentEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.Rigid, new Repetition(), MyParentEvent.Rigid ? 1 : MyParentEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled);
            
            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();

            for (; MyRepeatCalendarEvent.Start < MyParentEvent.Repeat.Range.End; )
            {
                MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.ID, MyRepeatCalendarEvent);
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, Frequency); ;
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, Frequency);
                MyEventCalendarID = new EventID(MyParentEvent.ID + "_" + EventIDGenerator.generate().ToString());
                MyRepeatCalendarEvent = new CalendarEvent(MyEventCalendarID, MyRepeatCalendarEvent.Name, MyRepeatCalendarEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyRepeatCalendarEvent.Preparation, MyRepeatCalendarEvent.PreDeadline, MyRepeatCalendarEvent.Rigid, MyRepeatCalendarEvent.Repeat, MyRepeatCalendarEvent.NumberOfSplit, MyParentEvent.myLocation, MyParentEvent.isEnabled);
                
                if (MyParentEvent.myLocation == null)
                {
                    MessageBox.Show("weird error Jeromes");
                }
                MyRepeatCalendarEvent.myLocation = MyParentEvent.myLocation;
            }
            RepeatingEvents = DictionaryOfIDAndCalendarEvents.Values.ToArray();
        }

        public DateTime IncreaseByFrequency(DateTime MyTime, string Frequency)
        {
            Frequency = Frequency.ToUpper();
            switch (Frequency)
            {
                case "DAILY":
                    {
                        return MyTime.AddDays(1);
                    }
                case "WEEKLY":
                    {
                        return MyTime.AddDays(7);
                    }
                case "BI-WEEKLY":
                    {
                        return MyTime.AddDays(14);
                    }
                case "MONTHLY":
                    {
                        return MyTime.AddMonths(1);
                    }
                case "YEARLY":
                    {
                        return MyTime.AddYears(1);
                    }
                default:
                    {
                        return MyTime;
                    }
            }
        }

        
        /*public Repetition(bool EnableFlag, DateTime StartTime, TimeSpan Frequency)
        {
            Start = StartTime;
            RepetitionFrequency = Frequency;
            EnableRepeat = EnableFlag;
        }*/

        public bool Enable
        {
            get 
            {
                return EnableRepeat;
            }
        }

        public string Frequency
        {
            get
            {
                return RepetitionFrequency;
            }
        }

        public TimeLine Range
        {
            get
            {
                return RepetitionRange;
            }
        }

        public CalendarEvent[] RecurringCalendarEvents
        {
            set 
            {
                foreach (CalendarEvent MyCalEvent in value)
                {
                    DictionaryOfIDAndCalendarEvents[MyCalEvent.ID] = MyCalEvent;
                }

                RepeatingEvents = DictionaryOfIDAndCalendarEvents.Values.ToArray();//assign od diffe list can generate inconsistencies...watchout for bugs
            }
            get 
            {
                return DictionaryOfIDAndCalendarEvents.Values.ToArray();
            }
        }

        public Repetition CreateCopy()
        {
            Repetition repetition_cpy = new Repetition();
            if (this.RepeatingEvents.Length < 1)
            {
                return repetition_cpy;
            }
            repetition_cpy.RepetitionFrequency = this.RepetitionFrequency;
            repetition_cpy.RepetitionRange = this.RepetitionRange.CreateCopy();
            repetition_cpy.RepeatingEvents = RepeatingEvents.Select(obj => obj.createCopy()).ToArray();
            repetition_cpy.RepeatLocation = RepeatLocation.CreateCopy();
            repetition_cpy.EnableRepeat = EnableRepeat;
            repetition_cpy.DictionaryOfIDAndCalendarEvents = DictionaryOfIDAndCalendarEvents.ToDictionary(obj => obj.Key, obj1 => obj1.Value.createCopy());
            return repetition_cpy;
        }

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
