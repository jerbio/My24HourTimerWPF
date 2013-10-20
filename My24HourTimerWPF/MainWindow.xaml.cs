using System;
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
using Outlook = Microsoft.Office.Interop.Outlook;
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
            datePicker1.SelectedDate = DateTime.Now.AddDays(1);
            datePicker2.SelectedDate = DateTime.Now.AddDays(1);
            MySchedule = new Schedule();
            LastID = (uint)(MySchedule.LastScheduleIDNumber);
            //EventIDGenerator();
            EventIDGenerator.Initialize();
        }

        DateTime FinalDate=new DateTime();
        private TimeSpan TimeLeft = new TimeSpan();
        private TimeSpan TimeTo24HourLeft = new TimeSpan();
        string SleepWakeString = "Sleep_Time_N";
        private void button5_Click(object sender, RoutedEventArgs e)
        {
        }

        static public uint ConvertToMinutes(string TimeEntry)
        {
            int MaxTimeIndexCounter = 5;
            string[] ArrayOfTimeComponent = TimeEntry.Split(':');
            Array.Reverse(ArrayOfTimeComponent);
            uint TotalMinutes = 0;
            for (int x = 0; x < ArrayOfTimeComponent.Length; x++)
            {
                int Multiplier = 0;
                switch (x)
                {
                    case 0:
                        Multiplier = 0;
                        break;
                    case 1:
                        Multiplier = 1;
                        break;
                    case 2:
                        Multiplier = 60;
                        break;
                    case 3:
                        Multiplier = 36 * 24;
                        break;
                    case 4:
                        Multiplier = 36 * 24 * 365;
                        break;
                }
                string JustHold = ArrayOfTimeComponent[x];
                Int64 MyNumber = (Int64)Convert.ToDouble(JustHold);
                TotalMinutes = (uint)(TotalMinutes + (Multiplier * MyNumber));

            }

            return TotalMinutes;

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
            MessageBox.Show(StringOfDateTime);
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
            string eventStartTime = textBox5.Text;

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
                eventStartTime = DateTime.Now.ToString();
                string[] TempString = eventStartTime.Split(' ');
                eventStartTime =TempString[1]+TempString[2];
            }
            //This attempts to detect invalid inputs for start time values 
            string[] TimeElements = CalendarEvent.convertTimeToMilitary(eventStartTime).Split(':');
            DateTime EnteredDateTime = new DateTime(eventStartDate.Year, eventStartDate.Month, eventStartDate.Day, Convert.ToInt32(TimeElements[0]), Convert.ToInt32(TimeElements[1]), 0);
            //if checks for StartDateTime
            if (EnteredDateTime < DateTime.Now)
            {
                DateTime Now=DateTime.Now;
                MessageBox.Show("Please Adjust Your Start Date:");
                return;
            }

            if (eventEndTime == "")
            {
                MessageBox.Show("Please Type EndTime in The Format: HH:MM A/PM");
                return;
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
                RepeatStart = (DateTime)calendar3.SelectedDate.Value;
                RepeatEnd = (DateTime)calendar4.SelectedDate.Value;
                RepetitionFlag = true;
                MyRepetition = new Repetition(RepetitionFlag, new TimeLine(RepeatStart, RepeatEnd), RepeatFrequency);
            }

            CustomErrors ErrorCheck = ValidateInputValues(EventDuration, eventStartTime, eventStartDate.ToString(), eventEndTime, eventEndDate.ToString(), RepeatStart.ToString(), RepeatEnd.ToString(), PreDeadlineTime, eventSplit, eventPrepTime, CurrentNow);

            if (!ErrorCheck.Status)
            { 
                MessageBox.Show(ErrorCheck.Message);
                return;
                    
            }

            //CalendarEvent ScheduleUpdated = CreateSchedule(eventName, eventStartTime, eventStartDate, eventEndTime, eventEndDate, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionflag, DefaultPreDeadlineFlag, RigidScheduleFlag, eventPrepTime, DefaultPreDeadlineFlag);
            CalendarEvent ScheduleUpdated = new CalendarEvent(eventName, eventStartTime, eventStartDate, eventEndTime, eventEndDate, eventSplit, PreDeadlineTime, EventDuration, MyRepetition, DefaultPreDeadlineFlag, RigidFlag, eventPrepTime, DefaultPreDeadlineFlag);
            ScheduleUpdated.Repeat.PopulateRepetitionParameters(ScheduleUpdated);
            MySchedule.AddToSchedule(ScheduleUpdated);
            textBlock9.Text = "Schedule Updated";
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
                    return new CustomErrors(false, "Invalid Duration Input");
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

        public CalendarEvent CreateSchedule(string Name, string StartTime, DateTime StartDate, string EndTime, DateTime EventEndDate, string eventSplit, string PreDeadlineTime, string EventDuration, bool EventRepetitionflag, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag)
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
                MessageBox.Show("Hey you clicked checkBox5");
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
            MySchedule.EmptyAllCalendarEvent();
        }



    }

    public class CustomErrors
    {
        bool Errorstatus;
        string ErrorMessage;
        public CustomErrors(bool StatusEntry, string MessagEntry)
        {
            Errorstatus = StatusEntry;
            ErrorMessage = MessagEntry;
        }

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
    }
    public class TimeLine
    {
        protected DateTime EndTime;
        protected DateTime StartTime;
        protected BusyTimeLine[] ActiveTimeSlots;
        
        #region constructor
        public TimeLine()
        { 
        
        }
        

        public TimeLine( DateTime MyStartTime, DateTime MyEndTime)
        {
            StartTime = MyStartTime;
            EndTime = MyEndTime;
            ActiveTimeSlots = new BusyTimeLine[0];
                //MessageBox.Show("Error In TimeLine Arguments End Time is less than Start Time");
            if (MyEndTime <= MyStartTime)
            {
                StartTime = MyStartTime;
                EndTime = MyStartTime;
            }
            //Debug.Assert(MyEndTime <= MyStartTime,"Error In TimeLine Arguments End Time is less than Start Time");
        }
        #endregion

        #region functions

        public bool IsDateTimeWithin(DateTime MyDateTime)
        {
            if ((MyDateTime >= StartTime) && (MyDateTime <= EndTime))
            {
                return true;
            }

            return false;
        }

        public TimeLine CreateCopy()
        {
            TimeLine CopyTimeLine= new TimeLine();
            CopyTimeLine.EndTime = EndTime;
            CopyTimeLine.StartTime = StartTime;
            CopyTimeLine.ActiveTimeSlots = ActiveTimeSlots;
            return CopyTimeLine;
        }

        public bool IsTimeLineWithin(TimeLine MyTimeLine)
        {
            if ((MyTimeLine.Start >= StartTime) && (MyTimeLine.End <= EndTime))
            {
                return true;
            }

            return false;
        }

        public void AddBusySlots(BusyTimeLine MyBusySlot)//further update will be to check if it interferes
        {
            List<BusyTimeLine> MyListOfActiveSlots = ActiveTimeSlots.ToList();//;
            MyListOfActiveSlots.Add(MyBusySlot);
            ActiveTimeSlots = MyListOfActiveSlots.ToArray();
        }

        public void AddBusySlots(BusyTimeLine[] MyBusySlot)//further update will be to check if it interferes
        {
            var MyNewArray = ActiveTimeSlots.Concat(MyBusySlot);
            ActiveTimeSlots = MyNewArray.ToArray();
        }

        public void MergeTimeLines(TimeLine OtherTimeLine)
        {
            AddBusySlots(OtherTimeLine.OccupiedSlots);
        }

        public List<BusyTimeLine> getBusyTimeLineWithinSlots(DateTime StartTime, DateTime EndTime) 
        { 
            TimeLine TempTimeLine = new TimeLine(StartTime,EndTime);
            List <BusyTimeLine> ActiveSlots = new List<BusyTimeLine>();
            foreach(BusyTimeLine MyBusyTimeline in ActiveTimeSlots)
            {
                if(TempTimeLine.IsTimeLineWithin(MyBusyTimeline))
                {
                    ActiveSlots.Add(MyBusyTimeline);
                }
            }
            return ActiveSlots;
        }

        public TimeLine[] getAllFreeSlots()
        {
            List<TimeLine> ListOfFreeSpots= new List<TimeLine>();
            if (ActiveTimeSlots.Length < 1)
            { 
                //List<TimeLine> SingleTimeline= new List<TimeLine>();
                ListOfFreeSpots.Add(this);

                return ListOfFreeSpots.ToArray();
            }

            DateTime PreceedingDateTime = StartTime;
            
            foreach (BusyTimeLine MyActiveSlot in ActiveTimeSlots)
            {
                ListOfFreeSpots.Add(new TimeLine(PreceedingDateTime, MyActiveSlot.Start));
                PreceedingDateTime = MyActiveSlot.End;
            }
            ListOfFreeSpots.Add(new TimeLine(ActiveTimeSlots[ActiveTimeSlots.Length-1].End, EndTime));

            return ListOfFreeSpots.ToArray();
        }

        public List<BusyTimeLine> getBusyTimeLineWithinSlots(TimeLine MyTimeLineRange) 
        {
            List<BusyTimeLine> ActiveSlots = new List<BusyTimeLine>();
            foreach (BusyTimeLine MyBusyTimeline in ActiveTimeSlots)
            {
                if (MyTimeLineRange.IsTimeLineWithin(MyBusyTimeline))
                {
                    ActiveSlots.Add(MyBusyTimeline);
                }
            }
            return ActiveSlots;
        }

        #endregion

        public DateTime Start
        {
            get
            {
                return StartTime;
            }
        }

        public DateTime End
        {
            get
            {
                return EndTime;
            }
        }

        public TimeSpan TimelineSpan
        {
            get
            {
                return EndTime - StartTime;
            }
        }

        public TimeSpan TimeTillEnd
        {
            get 
            {
                return EndTime- DateTime.Now;
            }
        }
        public TimeSpan TimeTillStart
        {
            get
            {
                return StartTime - DateTime.Now;
            }
        }

       

        virtual public BusyTimeLine [] OccupiedSlots
        {
            set
            {
                ActiveTimeSlots = value;
            }
            get 
            {
                return ActiveTimeSlots;
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

    public class BusyTimeLine : EventTimeLine
    {
        TimeSpan BusySpan;

        public BusyTimeLine()
        { 
        
        }
        public BusyTimeLine(TimeSpan MyBusySpan)
        {
            BusySpan = MyBusySpan;
        }
        public BusyTimeLine(string MyEventID, DateTime MyStartTime, DateTime MyEndTime)
        {
            StartTime = MyStartTime;
            EndTime = MyEndTime;
            BusySpan=EndTime - StartTime;
            TimeLineEventID = MyEventID;
        }
        #region Properties
        public TimeSpan BusyTimeSpan
        {
            get
            {
                return BusySpan;
            }
        }




        public DateTime Start
        {
            get 
            {
                return StartTime;
            }
        }
        public DateTime End
        {
            get
            {
                return EndTime;
            }
        }
        #endregion
    }

    public class EventTimeLine:TimeLine
    {
        protected string TimeLineEventID = "";
        
        public EventTimeLine()
        {
            
        }

        public EventTimeLine(string MyEventID, DateTime MyStartTime, DateTime MyEndTime)
        {
            StartTime = MyStartTime;
            EndTime = MyEndTime;
            TimeLineEventID=MyEventID;
        }

        public void PopulateBusyTimeSlot(string MyEventID, BusyTimeLine[] myActiveTimeSlots)
        {
            ActiveTimeSlots = myActiveTimeSlots;
            TimeLineEventID = MyEventID;
        }

        public string TimeLineID
        {
            get
            {
                return TimeLineEventID;
            }
        }

        override public BusyTimeLine[] OccupiedSlots
        {
            set
            {
                ActiveTimeSlots = value;
            }
            get
            {
                return ActiveTimeSlots;
            }
        }
    }

    class SnugArray
    {
        TimeSpan[] TopElements;
        SnugArray[] SubElements;


        public SnugArray()
        {

        }
        public SnugArray(List<TimeSpan> EntryArrayOfStuff, TimeSpan MaxValue)
        {
            List<TimeSpan> SmallerElements = new List<TimeSpan>();
            int i = 0;
            int MyArrayCount = EntryArrayOfStuff.Count;
            for (; i < MyArrayCount; i++)//loop gets element only smaller or equal to max size
            {
                if (EntryArrayOfStuff[i] <= MaxValue)//checks if smaller than max value
                {
                    SmallerElements.Add(EntryArrayOfStuff[i]);
                }
            }
            TopElements = SmallerElements.ToArray();
            SubElements = new SnugArray[TopElements.Length];

            i = 0;

            TimeSpan[] MyFittableElemtsHolder = new TimeSpan[SmallerElements.Count];//Array holds the current array of subelements for future reasons
            List<TimeSpan> MyFittableElementsHolderList = new List<TimeSpan>();//This is also holds a curent array of sub elements.
            SmallerElements.CopyTo(MyFittableElemtsHolder, 0);
            MyFittableElementsHolderList = MyFittableElemtsHolder.ToList();
            MyArrayCount = SmallerElements.Count;
            for (; i < MyArrayCount; i++)
            {
                MyFittableElementsHolderList.Remove(TopElements[i]);//removes a value that meets criteria of less than max value. This is used to form a node. The sub nodes will be have only values less than or equal to Max Value-other elements
                SubElements[i] = new SnugArray(MyFittableElementsHolderList, (MaxValue - TopElements[i]));
                SmallerElements.CopyTo(MyFittableElemtsHolder, 0);
                MyFittableElementsHolderList = MyFittableElemtsHolder.ToList();
            }
        }

        List<List<TimeSpan>> GenerateSnugPossibilities()
        {
            List<List<TimeSpan>> JustAllSubPossibilities;
            int i = 0;
            List<TimeSpan> MyCurrentList = new List<TimeSpan>();
            int j = 0;
            JustAllSubPossibilities = new List<List<TimeSpan>>();
            List<List<TimeSpan>> MyTotalSubPossibilities = new List<List<TimeSpan>>(); ;
            for (; i < TopElements.Length; i++)
            {
                MyCurrentList.Add(TopElements[i]);

                JustAllSubPossibilities = SubElements[i].MySnugPossibleEntries;
                //JustAllSubPossibilities.Add(MyCurrentList);
                j = 0;
                for (; j < JustAllSubPossibilities.Count; j++)
                {
                    JustAllSubPossibilities[j].Add(TopElements[i]);
                }
                if (JustAllSubPossibilities.Count == 0)
                {
                    JustAllSubPossibilities.Add(MyCurrentList);
                }
                MyCurrentList = new List<TimeSpan>();
                MyTotalSubPossibilities = MyTotalSubPossibilities.Concat(JustAllSubPossibilities).ToList();
            }
            return MyTotalSubPossibilities;
        }

        public List<List<TimeSpan>> MySnugPossibleEntries
        {
            get
            {
                List<List<TimeSpan>> JustAllSubPossibilities;
                int i = 0;
                List<TimeSpan> MyCurrentList = new List<TimeSpan>();
                int j = 0;
                JustAllSubPossibilities = new List<List<TimeSpan>>();
                List<List<TimeSpan>> MyTotalSubPossibilities = new List<List<TimeSpan>>(); ;
                for (; i < TopElements.Length; i++)
                {
                    MyCurrentList.Add(TopElements[i]);

                    JustAllSubPossibilities = SubElements[i].MySnugPossibleEntries;
                    //JustAllSubPossibilities.Add(MyCurrentList);
                    j = 0;
                    for (; j < JustAllSubPossibilities.Count; j++)
                    {
                        JustAllSubPossibilities[j].Add(TopElements[i]);
                    }
                    if (JustAllSubPossibilities.Count == 0)
                    {
                        JustAllSubPossibilities.Add(MyCurrentList);
                    }
                    MyCurrentList = new List<TimeSpan>();
                    MyTotalSubPossibilities = MyTotalSubPossibilities.Concat(JustAllSubPossibilities).ToList();
                }
                return MyTotalSubPossibilities;
            }
        }

        public TimeSpan[] MyTopElements
        {
            get
            {
                return TopElements;
            }
        }
    }

    public static class EventIDGenerator
    {
        static uint idcounter = 0;
        static bool AlreadyInitialized = false;
        public static void Initialize()
        {
            if (!AlreadyInitialized)
                idcounter = MainWindow.LastID;
            else
            {
                MessageBox.Show("Cannot Call ID initialize twice!!!");
            }
        }

        public static uint generate()
        {
            //update xml file with last counter
            return ++idcounter;
        }
    }

    public class EventID
    {
        string[] LayerID;
        public EventID()
        { 
        
        }
        public EventID(string myLayerID):this(myLayerID.Split('_'))
        { 
            
        }
        public EventID(string[] myLayerID)
        {
            LayerID = myLayerID;
        }

        public string[] ID
        {
            get 
            {
                return LayerID;
            }
        }

        public string getLevelID(int Level)
        {
            return LayerID[Level];
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
    }

    
    public class Repetition
    {
        
        string RepetitionFrequency;
        TimeLine RepetitionRange;
        bool EnableRepeat;
        CalendarEvent InitializingEvent;
        CalendarEvent[] RepeatingEvents;

        public Repetition()
        {
            RepetitionFrequency = "";
            RepetitionRange = new TimeLine();
            EnableRepeat = false;
        }
        public Repetition(bool EnableFlag ,TimeLine RepetitionRange_Entry, string Frequency)
        {
            RepetitionRange = RepetitionRange_Entry;
            RepetitionFrequency = Frequency.ToUpper();
            EnableRepeat = EnableFlag;
            InitializingEvent = new CalendarEvent();
        }

        //public Repetition(bool EnableFlag,CalendarEvent BaseCalendarEvent,  TimeLine RepetitionRange_Entry, string Frequency)
        public Repetition(bool ReadFromFileEnableFlag, TimeLine ReadFromFileRepetitionRange_Entry, string ReadFromFileFrequency, CalendarEvent[] ReadFromFileRecurringListOfCalendarEvents)
        {
            EnableRepeat = ReadFromFileEnableFlag;
            RepeatingEvents = ReadFromFileRecurringListOfCalendarEvents;
            RepetitionFrequency = ReadFromFileFrequency;
            RepetitionRange = ReadFromFileRepetitionRange_Entry;
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

            /*if (MyParentEvent.Rigid)//Rigid means if Start DateTime Will be Start Time and Date of CalendarEvent  and End Time and Date will be end DateTime of Calendar Event
            {
                EachRepeatCalendarStart = MyParentEvent.Start;
                EachRepeatCalendarEnd = MyParentEvent.End;
            }*/

            CalendarEvent MyRepeatCalendarEvent = new CalendarEvent(new EventID(MyParentEvent.ID), MyParentEvent.Name, MyParentEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.Rigid, new Repetition(), MyParentEvent.NumberOfSplit);
                //new CalendarEvent(MyParentEvent.Name, MyParentEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.Rigid, new Repetition(), MyParentEvent.NumberOfSplit);//first repeating calendar event
            
            //MyRepeatCalendarEvent = new CalendarEvent(new EventID(MyParentEvent.ID), MyRepeatCalendarEvent.Name, MyRepeatCalendarEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyRepeatCalendarEvent.Preparation, MyRepeatCalendarEvent.PreDeadline, MyRepeatCalendarEvent.Rigid, MyRepeatCalendarEvent.Repeat, MyRepeatCalendarEvent.NumberOfSplit);
            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();

            for (; MyRepeatCalendarEvent.Start < MyParentEvent.Repeat.Range.End; )
            {
                MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, Frequency); ;
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, Frequency);
                MyRepeatCalendarEvent = new CalendarEvent(new EventID(MyRepeatCalendarEvent.ID), MyRepeatCalendarEvent.Name, MyRepeatCalendarEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyRepeatCalendarEvent.Preparation, MyRepeatCalendarEvent.PreDeadline, MyRepeatCalendarEvent.Rigid, MyRepeatCalendarEvent.Repeat, MyRepeatCalendarEvent.NumberOfSplit);

            }
            RepeatingEvents = MyArrayOfRepeatingCalendarEvents.ToArray();
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
                RepeatingEvents=value;
            }
            get 
            {
                return RepeatingEvents;
            }
        }
    }
    public class Schedule
    {
        Dictionary<string, CalendarEvent> AllEventDictionary;
        TimeLine CompleteSchedule;
        string LastIDNumber;

        public Schedule ()
        {
            AllEventDictionary = getAllCalendarFromXml();
            CompleteSchedule=getTimeLine();
        }

        public CalendarEvent getCalendarEvent(string EventID)
        {
            return AllEventDictionary[new EventID(EventID).ID[0]];
        }

        public CalendarEvent getCalendarEvent(EventID myEventID)
        {
            return AllEventDictionary[myEventID.ID[0]];
        }

        public SubCalendarEvent getSubCalendarEvent(string EventID)
        {
            CalendarEvent myCalendarEvent=getCalendarEvent(EventID);
            return myCalendarEvent.getSubEvent(new EventID(EventID));
        }
        
        public void FindNewSlotForSubEvent(EventID MyEventID)
        {
            return;
        }

        public BusyTimeLine NextActivity
        {
            get 
            {
                //KeyValuePair<string, int> 
                List<BusyTimeLine> MyTotalSubEvents=new List<BusyTimeLine>(0);
                foreach (KeyValuePair<string, CalendarEvent> MyCalendarEvents in AllEventDictionary)
                {
                    foreach (SubCalendarEvent MySubCalendarEvent in MyCalendarEvents.Value.AllEvents)
                    {
                        MyTotalSubEvents.Add(MySubCalendarEvent.ActiveSlot);
                    }
                }
                MyTotalSubEvents = Schedule.SortBusyTimeline(MyTotalSubEvents, true);
                DateTime MyNow = DateTime.Now;//Moved Out of For loop for Speed boost
                for (int i=0; i<MyTotalSubEvents.Count;i++)
                {
                    if (MyTotalSubEvents[i].Start > MyNow)
                    {
                        return MyTotalSubEvents[i];
                    }
                }
                return null;
            }
        }
        
        TimeLine getTimeLine()
        {
            DateTime LastDeadline=DateTime.Now.AddHours(1);
            List<BusyTimeLine> MyTotalBusySlots=new List<BusyTimeLine>(0);
            //var Holder=new List();
            foreach (KeyValuePair<string, CalendarEvent> MyCalendarEvent in AllEventDictionary)
            {
                var Holder = MyTotalBusySlots.Concat(GetBusySlotPerCalendarEvent(MyCalendarEvent.Value));
                MyTotalBusySlots = Holder.ToList();
                /*foreach (SubCalendarEvent MySubCalendarEvent in MyCalendarEvent.Value.AllEvents)
                {
                    if (MySubCalendarEvent.End > LastDeadline)
                    {
                        LastDeadline = MySubCalendarEvent.End;
                    }
                    MyTotalBusySlots.Add(MySubCalendarEvent.ActiveSlot);
                }*/
            }
            MyTotalBusySlots = SortBusyTimeline(MyTotalBusySlots, true);
            TimeLine MyTimeLine = new TimeLine(DateTime.Now, DateTime.Now.AddHours(1));
            if (MyTotalBusySlots.Count > 0)
            {
                MyTimeLine = new TimeLine(DateTime.Now, MyTotalBusySlots[MyTotalBusySlots.Count - 1].End);
            }
            MyTimeLine.OccupiedSlots = MyTotalBusySlots.ToArray();
            return MyTimeLine;
        }

        public void EmptyAllCalendarEvent()//MyTemp Function for deleting all calendar events
        {
            int i = 0;
            CalendarEvent[] ArrayOfValues = AllEventDictionary.Values.ToArray();
            string[] ArrayOfKeys = AllEventDictionary.Keys.ToArray();


            for (; i < ArrayOfValues.Length;i++ )//this loops through the ArrayOfValues and ArrayOfIndex. Since each index loop corresponds to the same dictionary entry.
            {
                RemoveFromOutlook(ArrayOfValues[i]); //this removes the value from outlook
                AllEventDictionary.Remove(ArrayOfKeys[i]);//this removes the entry from The dictionary
            }

            File.WriteAllText("MyEventLog.xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?><ScheduleLog><LastIDCounter>0</LastIDCounter><EventSchedules></EventSchedules></ScheduleLog>");//This Explicitly Empties the MyEventLog File
        }

        BusyTimeLine[] GetBusySlotPerCalendarEvent(CalendarEvent MyEvent)
        { 
            int i=0;
            List<BusyTimeLine> MyTotalSubEventBusySlots = new List<BusyTimeLine>(0);
            BusyTimeLine[] ArrayOfBusySlotsInRepeat = new BusyTimeLine[0];
            DateTime LastDeadline = DateTime.Now.AddHours(1);

            if (MyEvent.RepetitionStatus)
            {
                ArrayOfBusySlotsInRepeat = GetBusySlotsPerRepeat(MyEvent.Repeat);
            }

            /*for (;i<MyEvent.AllEvents.Length;i++)
            {
                {*/
                    foreach (SubCalendarEvent MySubCalendarEvent in MyEvent.AllEvents)
                    {
                        MyTotalSubEventBusySlots.Add(MySubCalendarEvent.ActiveSlot);
                    }
                    
                    //MyTotalSubEventBusySlots.Add(MyEvent.AllEvents[i].ActiveSlot);
                /*}
            }*/

            //BusyTimeLine[] ConcatenatSumOfAllBusySlots = new BusyTimeLine[ArrayOfBusySlotsInRepeat.Length + MyTotalSubEventBusySlots.Count];
            /*
            i = 0;
            for (; i < ArrayOfBusySlotsInRepeat.Length; i++)
            {
                ConcatenatSumOfAllBusySlots[i] = ArrayOfBusySlotsInRepeat[i];
            }
            i = ArrayOfBusySlotsInRepeat.Length;
            int LengthOfConcatenatSumOfAllBusySlots = ConcatenatSumOfAllBusySlots.Length;
            int j = 0;
            j = i;
            for (; j < LengthOfConcatenatSumOfAllBusySlots;)
            {
                ConcatenatSumOfAllBusySlots[j] = MyTotalSubEventBusySlots[i];
                i++;
                j++;
            }*/
            var Holder = MyTotalSubEventBusySlots.Concat(ArrayOfBusySlotsInRepeat);
            BusyTimeLine[] ConcatenatSumOfAllBusySlots = Holder.ToArray();
            //ArrayOfBusySlotsInRepeat.CopyTo(ConcatenatSumOfAllBusySlots, 0);
            //MyTotalSubEventBusySlots.CopyTo(ConcatenatSumOfAllBusySlots, ConcatenatSumOfAllBusySlots.Length);
            //ArrayOfBusySlotsInRepeat.CopyTo(ConcatenatSumOfAllBusySlots, 0);
            return ConcatenatSumOfAllBusySlots;
        }

        BusyTimeLine[] GetBusySlotsPerRepeat(Repetition RecurringEvents)
        {
            int i = 0;
            BusyTimeLine[] ArrayOfBusySlotsPerRecurringEvents;
            List<BusyTimeLine[]> MyListOfWithArrayOfBusySlots = new List<BusyTimeLine[]>();//this can be used as a list since we know the limits of each repeating element. Only using list becaue it'll be faster to implement
            for (; i < RecurringEvents.RecurringCalendarEvents.Length; i++)
            {
                MyListOfWithArrayOfBusySlots.Add(GetBusySlotPerCalendarEvent(RecurringEvents.RecurringCalendarEvents[i]));
            }
            List<BusyTimeLine> MyListOfBusySlots = new List<BusyTimeLine>();
            i = 0;
            int j=0;
            for (; i < MyListOfWithArrayOfBusySlots.Count; i++)
            { 
                j=0;
                for (; j < MyListOfWithArrayOfBusySlots[i].Length; j++)
                {
                    MyListOfBusySlots.Add(MyListOfWithArrayOfBusySlots[i][j]);
                }
            }

            return MyListOfBusySlots.ToArray();


        }

        Dictionary<string, CalendarEvent> getAllCalendarFromXml()
        {
            Dictionary<string, CalendarEvent> MyCalendarEventDictionary=new Dictionary<string, CalendarEvent>();
            XmlDocument doc = new XmlDocument();
            
            doc.Load("MyEventLog.xml");
            XmlNode node = doc.DocumentElement.SelectSingleNode("/ScheduleLog/LastIDCounter");
            string LastUsedIndex = node.InnerText;
            LastIDNumber = LastUsedIndex;
            XmlNode EventSchedulesNodes = doc.DocumentElement.SelectSingleNode("/ScheduleLog/EventSchedules");
            string ID;
            string Deadline;
            string Split;
            string Completed;
            string Rigid;
            string Name;
            string[] StartDateTime;
            string StartDate;
            string StartTime;
            string[] EndDateTime;
            string EndDate;
            string EndTime;
            string PreDeadline;
            string CalendarEventDuration;
            string PreDeadlineFlag;
            string EventRepetitionflag;
            string PrepTimeFlag;
            string PrepTime;
            foreach (XmlNode EventScheduleNode in EventSchedulesNodes.ChildNodes)
            {
                CalendarEvent RetrievedEvent;
                RetrievedEvent=getCalendarEventObjFromNode(EventScheduleNode);
                MyCalendarEventDictionary.Add(RetrievedEvent.ID, RetrievedEvent);
            }

            return MyCalendarEventDictionary;
        }

        public static DateTime ConvertToDateTime(string StringOfDateTime)
        {
            string[] strArray = StringOfDateTime.Split(new char[] { '|' });
            string[] strArray2 = strArray[0].Split(new char[] { ' ' });
            string[] strArray3 = strArray[1].Split(new char[] { ' ' });
            return new DateTime(Convert.ToInt16(strArray2[0]), Convert.ToInt16(strArray2[1]), Convert.ToInt16(strArray2[2]), Convert.ToInt16(strArray3[0]), Convert.ToInt16(strArray3[1]), Convert.ToInt16(strArray3[2]));
        }

        CalendarEvent getCalendarEventObjFromNode(XmlNode EventScheduleNode)
        {
            string ID;
            string Deadline;
            string Split;
            string Completed;
            string Rigid;
            string Name;
            string[] StartDateTime;
            string StartDate;
            string StartTime;
            string[] EndDateTime;
            string EndDate;
            string EndTime;
            string PreDeadline;
            string CalendarEventDuration;
            string PreDeadlineFlag;
            string EventRepetitionflag;
            string PrepTimeFlag;
            string PrepTime;
            string RepeatStart;
            string RepeatEnd;
            string RepeatFrequency;

            Name = EventScheduleNode.SelectSingleNode("Name").InnerText;
            ID = EventScheduleNode.SelectSingleNode("ID").InnerText;
            //EventScheduleNode.SelectSingleNode("ID").InnerXml = "<wetin></wetin>";
            Deadline = EventScheduleNode.SelectSingleNode("Deadline").InnerText;
            Rigid = EventScheduleNode.SelectSingleNode("RigidFlag").InnerText;
            XmlNode RecurrenceXmlNode = EventScheduleNode.SelectSingleNode("Recurrence");
            EventRepetitionflag = EventScheduleNode.SelectSingleNode("RepetitionFlag").InnerText;
            Repetition Recurrence;
            if (Convert.ToBoolean(EventRepetitionflag))
            {
                RepeatStart = RecurrenceXmlNode.SelectSingleNode("RepeatStartDate").InnerText;
                RepeatEnd = RecurrenceXmlNode.SelectSingleNode("RepeatEndDate").InnerText;
                RepeatFrequency = RecurrenceXmlNode.SelectSingleNode("RepeatFrequency").InnerText;
                XmlNode XmlNodeWithList = RecurrenceXmlNode.SelectSingleNode("RepeatCalendarEvents");
                Recurrence = new Repetition(true, new TimeLine(stringToDateTime(RepeatStart), stringToDateTime(RepeatEnd)), RepeatFrequency, getAllRepeatCalendarEvents(XmlNodeWithList));
                //Recurrence.RecurringCalendarEvents = getAllRepeatCalendarEvents(XmlNodeWithList);
            }
            else 
            { 
                Recurrence=new Repetition();
            }
            Split = EventScheduleNode.SelectSingleNode("Split").InnerText;
            PreDeadline = EventScheduleNode.SelectSingleNode("PreDeadline").InnerText;
            //PreDeadlineFlag = EventScheduleNode.SelectSingleNode("PreDeadlineFlag").InnerText;
            CalendarEventDuration = EventScheduleNode.SelectSingleNode("Duration").InnerText;
            //EventRepetitionflag = EventScheduleNode.SelectSingleNode("RepetitionFlag").InnerText;
            //PrepTimeFlag = EventScheduleNode.SelectSingleNode("PrepTimeFlag").InnerText;
            PrepTime = EventScheduleNode.SelectSingleNode("PrepTime").InnerText;
            Completed = EventScheduleNode.SelectSingleNode("Completed").InnerText;
            StartDateTime = EventScheduleNode.SelectSingleNode("StartTime").InnerText.Split(' ');
            StartDate = StartDateTime[0];
            StartTime = StartDateTime[1] + StartDateTime[2];
            EndDateTime = EventScheduleNode.SelectSingleNode("Deadline").InnerText.Split(' ');
            EndDate = EndDateTime[0];
            EndTime = EndDateTime[1] + EndDateTime[2];
            //string Name, string StartTime, DateTime StartDate, string EndTime, DateTime EventEndDate, string eventSplit, string PreDeadlineTime, string EventDuration, bool EventRepetitionflag, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag
            DateTime StartTimeConverted = new DateTime(Convert.ToInt32(StartDate.Split('/')[2]), Convert.ToInt32(StartDate.Split('/')[0]), Convert.ToInt32(StartDate.Split('/')[1]));
            DateTime EndTimeConverted = new DateTime(Convert.ToInt32(EndDate.Split('/')[2]), Convert.ToInt32(EndDate.Split('/')[0]), Convert.ToInt32(EndDate.Split('/')[1]));
            //MainWindow.CreateSchedule("","",new DateTime(),"",new DateTime(),"","","",true,true,true,"",false);
            Recurrence = Recurrence;
            CalendarEvent RetrievedEvent = new CalendarEvent(ID, Name, StartTime, StartTimeConverted, EndTime, EndTimeConverted, Split, PreDeadline, CalendarEventDuration, Recurrence, false, Convert.ToBoolean(Rigid), PrepTime, false);
            RetrievedEvent = new CalendarEvent(RetrievedEvent, ReadSubSchedulesFromXMLNode(EventScheduleNode.SelectSingleNode("EventSubSchedules"), RetrievedEvent));
            return RetrievedEvent;
        }

        CalendarEvent[] getAllRepeatCalendarEvents(XmlNode RepeatEventSchedulesNode)
        {
            XmlNodeList ListOfRepeatEventScheduleNode = RepeatEventSchedulesNode.ChildNodes;
            List<CalendarEvent> ListOfRepeatCalendarNodes = new List<CalendarEvent>();
            foreach (XmlNode MyNode in ListOfRepeatEventScheduleNode)
            {
                ListOfRepeatCalendarNodes.Add(getCalendarEventObjFromNode(MyNode));
            }
            return ListOfRepeatCalendarNodes.ToArray();
        }

        SubCalendarEvent[] ReadSubSchedulesFromXMLNode(XmlNode MyXmlNode,CalendarEvent MyParent)
        {
            SubCalendarEvent[] MyArrayOfNodes = new SubCalendarEvent[MyXmlNode.ChildNodes.Count];
            string ID = "";
            DateTime Start = new DateTime();
            DateTime End = new DateTime();
            TimeSpan SubScheduleDuration=new TimeSpan();
            TimeSpan PrepTime=new TimeSpan();
            BusyTimeLine BusySlot=new BusyTimeLine();
            for (int i=0;i<MyXmlNode.ChildNodes.Count;i++)
            {
                BusyTimeLine SubEventActivePeriod = new BusyTimeLine(MyXmlNode.ChildNodes[i].SelectSingleNode("ID").InnerText,stringToDateTime(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveStartTime").InnerText),stringToDateTime(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveEndTime").InnerText));
                ID=MyXmlNode.ChildNodes[i].SelectSingleNode("ID").InnerText;
                Start=stringToDateTime(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveStartTime").InnerText);
                End=stringToDateTime(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveEndTime").InnerText);
                BusySlot=new BusyTimeLine(ID,Start,End);
                PrepTime=new TimeSpan(ConvertToMinutes(MyXmlNode.ChildNodes[i].SelectSingleNode("PrepTime").InnerText)*60*10000000);
                    //stringToDateTime();
                Start=stringToDateTime(MyXmlNode.ChildNodes[i].SelectSingleNode("StartTime").InnerText);
                End=stringToDateTime(MyXmlNode.ChildNodes[i].SelectSingleNode("EndTime").InnerText);
                MyArrayOfNodes[i] = new SubCalendarEvent(ID, BusySlot, Start, End, PrepTime, MyParent.ID, MyParent.Rigid);
                MyArrayOfNodes[i].ThirdPartyID = MyXmlNode.ChildNodes[i].SelectSingleNode("ThirdPartyID").InnerText;//this is a hack to just update the Third partyID
            }

            return MyArrayOfNodes;
        }

        public static DateTime stringToDateTime(string MyDateTimeString)//String should be in format "MM/DD/YY HH:MM:SSA"
        {
            //4/19/2013 11:34:40 AM
            string[] DateTimeComponents = MyDateTimeString.Split(' ');
            string[] DateComponents;
            string[] TimeComponents;
            int Year;
            int Month;
            int Day;
            int Hour;
            int Min;
            int sec;
            DateComponents = DateTimeComponents[0].Split('/');
            TimeComponents = (CalendarEvent.convertTimeToMilitary(DateTimeComponents[1] + DateTimeComponents[2])).Split(':');
            DateTime MyDateTime,MyNow;

            if (DateComponents.Length < 2)
            {
                MyNow = DateTime.Now;
                Hour = Convert.ToInt32(TimeComponents[0]);
                Min = Convert.ToInt32(TimeComponents[1]);
                sec = 0;
                MyDateTime = new DateTime(MyNow.Year, MyNow.Month, MyNow.Day, Hour, Min, sec);
                return MyDateTime;
            }
            if (TimeComponents.Length < 2)
            {
                Year = Convert.ToInt32(DateComponents[2]);
                Month = Convert.ToInt32(DateComponents[0]);
                Day = Convert.ToInt32(DateComponents[1]);
                MyDateTime = new DateTime(Year, Month, Day, 0, 0, 0);
                return MyDateTime;
            }
            
            Year=Convert.ToInt32(DateComponents[2]);
            Month=Convert.ToInt32(DateComponents[0]);
            Day= Convert.ToInt32(DateComponents[1]);
            Hour=Convert.ToInt32(TimeComponents[0]);
            Min=Convert.ToInt32(TimeComponents[1]);
            sec = 0;
            MyDateTime =new DateTime(Year,Month,Day,Hour,Min,sec);
            return MyDateTime;
        }

        static public uint ConvertToMinutes(string TimeEntry)
        {
            int MaxTimeIndexCounter = 5;
            string[] ArrayOfTimeComponent = TimeEntry.Split(':');
            Array.Reverse(ArrayOfTimeComponent);
            uint TotalMinutes = 0;
            for (int x = 0; x < ArrayOfTimeComponent.Length; x++)
            {
                int Multiplier = 0;
                switch (x)
                {
                    case 0:
                        Multiplier = 0;
                        break;
                    case 1:
                        Multiplier = 1;
                        break;
                    case 2:
                        Multiplier = 60;
                        break;
                    case 3:
                        Multiplier = 36 * 24;
                        break;
                    case 4:
                        Multiplier = 36 * 24 * 365;
                        break;
                }
                string JustHold = ArrayOfTimeComponent[x];
                Int64 MyNumber = (Int64)Convert.ToDouble(JustHold);
                TotalMinutes = (uint)(TotalMinutes + (Multiplier * MyNumber));

            }

            return TotalMinutes;

        }

        public bool AddToSchedule(CalendarEvent NewEvent)
        {
            NewEvent=EvaluateTotalTimeLineAndAssignValidTimeSpots(NewEvent);
            if (NewEvent.ID == "")//checks if event was assigned and ID ehich means it was successfully able to find a spot
            {
                return false;
            }
            //WriteToOutlook(NewEvent);
            WriteToLog(NewEvent);
            
            try
            {
                AllEventDictionary.Add(NewEvent.ID, NewEvent);
            }
            catch
            {
                AllEventDictionary[NewEvent.ID]= NewEvent;
            }
           CompleteSchedule= getTimeLine();

            return true;
        }

        public CalendarEvent GenerateRigidSubEvents(CalendarEvent MyCalendarEvent)
        {
            int i=0;
            List<SubCalendarEvent> MyArrayOfSubEvents=new List<SubCalendarEvent>();
            for (; i < MyCalendarEvent.AllEvents.Length; i++)
            { 
                
            }
            
            if (MyCalendarEvent.RepetitionStatus)
            {
                SubCalendarEvent MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start, MyCalendarEvent.Repeat.Range.End, MyCalendarEvent.Preparation, MyCalendarEvent.ID);
                     //new SubCalendarEvent(MyCalendarEvent.End, MyCalendarEvent.Repeat.Range.Start, MyCalendarEvent.Repeat.Range.End, MyCalendarEvent.Preparation, MyCalendarEvent.ID);
                
                for (;MySubEvent.Start<MyCalendarEvent.Repeat.Range.End;)
                {
                    MyArrayOfSubEvents.Add(MySubEvent);
                    switch (MyCalendarEvent.Repeat.Frequency)
                    {
                        case "DAILY":
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start.AddDays(1), MyCalendarEvent.Repeat.Range.End.AddDays(1), MyCalendarEvent.Preparation, MyCalendarEvent.ID);
                                break;
                            }
                        case "WEEKLY":
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start.AddDays(7), MyCalendarEvent.Repeat.Range.End.AddDays(7), MyCalendarEvent.Preparation, MyCalendarEvent.ID);
                                break;
                            }
                        case "BI-WEEKLY":
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start.AddDays(14), MyCalendarEvent.Repeat.Range.End.AddDays(14), MyCalendarEvent.Preparation, MyCalendarEvent.ID);
                                break;
                            }
                        case "MONTHLY":
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start.AddMonths(1), MyCalendarEvent.Repeat.Range.End.AddMonths(1), MyCalendarEvent.Preparation, MyCalendarEvent.ID);
                                break;
                            }
                        case "YEARLY":
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start.AddYears(1), MyCalendarEvent.Repeat.Range.End.AddYears(1), MyCalendarEvent.Preparation, MyCalendarEvent.ID);
                                break;
                            }
                    }
                    
                    
                }
            }

            return MyCalendarEvent;
        }

        public List<TimeLine> getOnlyPertinentTimeFrame(TimeLine[] ArraytOfFreeSpots, TimeLine myTimeLine)
        {
            /*
             * Name: Jerome Biotidara
             * Description: Function only takes a TImeLine and Array Of TimeLine FreeSpots. It returns a List Of TimeLine In whcih each elements  exist within the range of TimeLine
             */
            List<TimeLine> PertinentTimeLine = new List<TimeLine>();
            List<TimeLine> OutLiers = new List<TimeLine>();

            foreach (TimeLine MyFreeTimeLine in ArraytOfFreeSpots)
            {
                if (myTimeLine.IsTimeLineWithin(MyFreeTimeLine))
                {
                    PertinentTimeLine.Add(MyFreeTimeLine);
                }
                else 
                { 
                    OutLiers.Add(MyFreeTimeLine);
                }
            }

            foreach (TimeLine Outlier in OutLiers)//this can be embedded in the preceeding foreach loop above in the else branch
            {
                if (myTimeLine.IsDateTimeWithin(Outlier.Start))
                {
                    PertinentTimeLine.Add(new TimeLine(Outlier.Start, myTimeLine.End));
                }
                else 
                {
                    if (myTimeLine.IsDateTimeWithin(Outlier.End))
                    {
                        PertinentTimeLine.Add(new TimeLine(myTimeLine.Start, Outlier.End));
                    }
                }
            }

            return PertinentTimeLine;



            //return new List<TimeLine>();
        }

        public CalendarEvent EvaluateTotalTimeLineAndAssignValidTimeSpots(CalendarEvent MyEvent)
        {
            BusyTimeLine [] AllOccupiedSlot= CompleteSchedule.OccupiedSlots;
            TimeSpan TotalActiveDuration=new TimeSpan();
            TimeLine[] TimeLineArrayWithSubEventsAssigned = new TimeLine[MyEvent.AllEvents.Length];
            SubCalendarEvent TempSubEvent=new SubCalendarEvent();
            BusyTimeLine MyTempBusyTimerLine=new BusyTimeLine();
            TimeLine[] FreeSpotsAvailableWithinValidTimeline= getAllFreeSpots(new TimeLine(MyEvent.Start,MyEvent.End));
            FreeSpotsAvailableWithinValidTimeline = getOnlyPertinentTimeFrame(FreeSpotsAvailableWithinValidTimeline, new TimeLine(MyEvent.Start, MyEvent.End)).ToArray();
            
            int i = 0;
            TimeSpan TotalFreeTimeAvailable=new TimeSpan();
            if (MyEvent.Rigid)
            {
                TempSubEvent = new SubCalendarEvent(MyEvent.ActiveDuration, MyEvent.Start, MyEvent.End, MyEvent.Preparation, MyEvent.ID);
                MyTempBusyTimerLine = new BusyTimeLine(TempSubEvent.ID, TempSubEvent.Start, TempSubEvent.End);
                TempSubEvent = new SubCalendarEvent(TempSubEvent.ID, TempSubEvent.Start, TempSubEvent.End, MyTempBusyTimerLine);
                MyEvent.AllEvents[0] = TempSubEvent;
            }
            else 
            {
                for (i = 0; i < FreeSpotsAvailableWithinValidTimeline.Length; i++)
                {
                    TotalFreeTimeAvailable += FreeSpotsAvailableWithinValidTimeline[i].TimelineSpan;
                }
                
                if (TotalFreeTimeAvailable >= MyEvent.ActiveDuration)
                {
                    TimeLineArrayWithSubEventsAssigned = SplitFreeSpotsInToSubEventTimeSlots(FreeSpotsAvailableWithinValidTimeline, MyEvent.AllEvents.Length, MyEvent.ActiveDuration);
                    if (TimeLineArrayWithSubEventsAssigned == null)
                    {
                        BusyTimeLine[] CompleteScheduleOccupiedSlots = CompleteSchedule.OccupiedSlots;
                        CalendarEvent MyCalendarEventUpdated=ReArrangeTimeLineWithinWithinCalendaEventRange(MyEvent);
                        if (MyCalendarEventUpdated != null)
                        {
                            return MyCalendarEventUpdated;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Sorry, the total free time available during activiy limits is less than your active duration!!!");
                    return new CalendarEvent();
                }

                if (TimeLineArrayWithSubEventsAssigned.Length < MyEvent.AllEvents.Length)// This means the assigned time per subevent spots won't be sufficient for the subevents available to the calendar event
                {
                    return null;

                }

                i = 0;
                for (; i < MyEvent.AllEvents.Length; i++)
                {
                    TempSubEvent = new SubCalendarEvent(TimeLineArrayWithSubEventsAssigned[i].TimelineSpan, TimeLineArrayWithSubEventsAssigned[i].Start.Add(-MyEvent.Preparation), TimeLineArrayWithSubEventsAssigned[i].End, MyEvent.Preparation, MyEvent.ID,MyEvent.Rigid);
                    MyTempBusyTimerLine = new BusyTimeLine(TempSubEvent.ID, TimeLineArrayWithSubEventsAssigned[i].Start, TimeLineArrayWithSubEventsAssigned[i].End);
                    TempSubEvent = new SubCalendarEvent(TempSubEvent.ID, TimeLineArrayWithSubEventsAssigned[i].Start.Add(-MyEvent.Preparation), TimeLineArrayWithSubEventsAssigned[i].End, MyTempBusyTimerLine, MyEvent.Rigid);
                    MyEvent.AllEvents[i] = TempSubEvent;
                }
            }

            if (MyEvent.RepetitionStatus)
            {
                for (i = 0; i < MyEvent.Repeat.RecurringCalendarEvents.Length; i++)
                {
                    MyEvent.Repeat.RecurringCalendarEvents[i]=EvaluateTotalTimeLineAndAssignValidTimeSpots(MyEvent.Repeat.RecurringCalendarEvents[i]);
                }
            }

            return MyEvent;
        }
        public CalendarEvent EvaluateTotalTimeLineAndAssignValidTimeSpotsWithReferenceTimeLine(CalendarEvent MyEvent, TimeLine ReferenceTimeLine)
        {
            BusyTimeLine[] AllOccupiedSlot = ReferenceTimeLine.OccupiedSlots; //CompleteSchedule.OccupiedSlots;
            TimeSpan TotalActiveDuration = new TimeSpan();
            TimeLine[] TimeLineArrayWithSubEventsAssigned = new TimeLine[MyEvent.AllEvents.Length];
            SubCalendarEvent TempSubEvent = new SubCalendarEvent();
            BusyTimeLine MyTempBusyTimerLine = new BusyTimeLine();
            TimeLine[] FreeSpotsAvailableWithinValidTimeline = getAllFreeSpots(new TimeLine(MyEvent.Start, MyEvent.End));
            FreeSpotsAvailableWithinValidTimeline = getOnlyPertinentTimeFrame(FreeSpotsAvailableWithinValidTimeline, new TimeLine(MyEvent.Start, MyEvent.End)).ToArray();

            int i = 0;
            TimeSpan TotalFreeTimeAvailable = new TimeSpan();
            if (MyEvent.Rigid)
            {
                TempSubEvent = new SubCalendarEvent(MyEvent.ActiveDuration, MyEvent.Start, MyEvent.End, MyEvent.Preparation, MyEvent.ID);
                MyTempBusyTimerLine = new BusyTimeLine(TempSubEvent.ID, TempSubEvent.Start, TempSubEvent.End);
                TempSubEvent = new SubCalendarEvent(TempSubEvent.ID, TempSubEvent.Start, TempSubEvent.End, MyTempBusyTimerLine);
                MyEvent.AllEvents[0] = TempSubEvent;
            }
            else
            {
                for (i = 0; i < FreeSpotsAvailableWithinValidTimeline.Length; i++)
                {
                    TotalFreeTimeAvailable += FreeSpotsAvailableWithinValidTimeline[i].TimelineSpan;
                }

                if (TotalFreeTimeAvailable >= MyEvent.ActiveDuration)
                {
                    TimeLineArrayWithSubEventsAssigned = SplitFreeSpotsInToSubEventTimeSlots(FreeSpotsAvailableWithinValidTimeline, MyEvent.AllEvents.Length, MyEvent.ActiveDuration);
                    if (TimeLineArrayWithSubEventsAssigned == null)
                    {
                        BusyTimeLine[] CompleteScheduleOccupiedSlots = ReferenceTimeLine.OccupiedSlots;
                        CalendarEvent MyCalendarEventUpdated = ReArrangeTimeLineWithinWithinCalendaEventRange(MyEvent);
                        if (MyCalendarEventUpdated != null)
                        {
                            return MyCalendarEventUpdated;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Sorry, the total free time available during activiy limits is less than your active duration!!!");
                    return new CalendarEvent();
                }

                if (TimeLineArrayWithSubEventsAssigned.Length < MyEvent.AllEvents.Length)// This means the assigned time per subevent spots won't be sufficient for the subevents available to the calendar event
                {
                    return null;

                }

                i = 0;
                for (; i < MyEvent.AllEvents.Length; i++)
                {
                    TempSubEvent = new SubCalendarEvent(TimeLineArrayWithSubEventsAssigned[i].TimelineSpan, TimeLineArrayWithSubEventsAssigned[i].Start.Add(-MyEvent.Preparation), TimeLineArrayWithSubEventsAssigned[i].End, MyEvent.Preparation, MyEvent.ID);
                    MyTempBusyTimerLine = new BusyTimeLine(TempSubEvent.ID, TimeLineArrayWithSubEventsAssigned[i].Start, TimeLineArrayWithSubEventsAssigned[i].End);
                    TempSubEvent = new SubCalendarEvent(TempSubEvent.ID, TimeLineArrayWithSubEventsAssigned[i].Start.Add(-MyEvent.Preparation), TimeLineArrayWithSubEventsAssigned[i].End, MyTempBusyTimerLine);
                    MyEvent.AllEvents[i] = TempSubEvent;
                }
            }

            if (MyEvent.RepetitionStatus)
            {
                for (i = 0; i < MyEvent.Repeat.RecurringCalendarEvents.Length; i++)
                {
                    MyEvent.Repeat.RecurringCalendarEvents[i] = EvaluateTotalTimeLineAndAssignValidTimeSpots(MyEvent.Repeat.RecurringCalendarEvents[i]);
                }
            }

            return MyEvent;
        }

        Dictionary<CalendarEvent, List<SubCalendarEvent>> generateDictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents(List<SubCalendarEvent> ListOfInterferringElements)
        {
            /*
             Name:Function takes the list of interferring arrays and used to build a Calendar To "List of SubCalendarEvent" dictionary. 
             */
            
            int i = 0;
            Dictionary<CalendarEvent, List<SubCalendarEvent>> DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents = new Dictionary<CalendarEvent, List<SubCalendarEvent>>();
            for (; i < ListOfInterferringElements.Count; i++)
            {
                string ParentID=(new EventID(ListOfInterferringElements[i].ID)).getLevelID(0);//This gets the parentID of the SubCalendarEventID
                try//Try bock attempts to create new dictionary entry for Calendar event. Else it simply adds an element to the list created by the error
                {
                    DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Add(AllEventDictionary[ParentID],new List<SubCalendarEvent>(){ListOfInterferringElements[i]});
                }
                catch
                {
                    DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[AllEventDictionary[ParentID]].Add(ListOfInterferringElements[i]);
                }
                
                
            }

            return DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents;
        }

        CalendarEvent ReArrangeTimeLineWithinWithinCalendaEventRange(CalendarEvent MyCalendarEvent)// this looks at the timeline of the calendar event and then tries to rearrange all subevents within the range to suit final output. Such that there will be sufficient time space for each subevent
        {
            /*
                Name{: Jerome Biotidara
             * this function is responsible for making sure there is some dynamic allotment of time to the subeevents. It takes a calendarevent, checks the alloted time frame and tries to move subevents within the time frame to satisfy the final goal.
             */
            SubCalendarEvent[] ArrayOfInterferringSubEvents = getInterferringSubEvents(MyCalendarEvent);//It gets all the subevents within the time frame
            ArrayOfInterferringSubEvents = SortSubCalendarEvents(ArrayOfInterferringSubEvents.ToList(), false).ToArray();
            BusyTimeLine[] InterferringDateTime = new BusyTimeLine[ArrayOfInterferringSubEvents.Length];
            Dictionary<CalendarEvent, List<SubCalendarEvent>> DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents = new Dictionary<CalendarEvent, List<SubCalendarEvent>>();
            int i=0;
            for (; i < InterferringDateTime.Length; i++)//for loop populates InteferringDateTIme so that it can be populated
            {
                InterferringDateTime[i] = ArrayOfInterferringSubEvents[i].ActiveSlot;
            }

            InterferringDateTime = SortBusyTimeline(InterferringDateTime.ToList(), false).ToArray();
            List<BusyTimeLine>[] MyEdgeElements = getEdgeElements(MyCalendarEvent.EventTimeLine, InterferringDateTime);
            List<CalendarEvent>[] CalendarEventsTimeCategories = CategorizeCalendarEventTimeLine(MyCalendarEvent.EventTimeLine, InterferringDateTime);
            List<SubCalendarEvent>[]SubEventsTimeCategories= CategorizeSubEventsTimeLine(MyCalendarEvent.EventTimeLine, InterferringDateTime);
            
            List<CalendarEvent> EndingBeforeDeadlineList = CalendarEventsTimeCategories[1].Concat(CalendarEventsTimeCategories[3]).ToList();
            CalendarEvent[] EndingBeforeDeadlineArray = EndingBeforeDeadlineList.ToArray();

            Array.Sort(EndingBeforeDeadlineArray, CalendarEvent.CompareByEndDate);
            EndingBeforeDeadlineList = EndingBeforeDeadlineArray.ToList();
            
            //List<CalendarEvent>[]SubEventsTimeCategories= CategorizeSubEventsTimeLine
            /*
             * SubEventsTimeCategories has 4 list of containing lists.
             * 1st is a List with Elements Starting before The Mycalendaervent timeline and ends after the busytimeline
             * 2nd is a list with elements starting before the mycalendarvent timeline but ending before the myevent timeline
             * 3rd is a list with elements starting after the Mycalendar event start time but ending after the Myevent timeline
             * 4th is a list with elements starting after the MyCalendar event start time and ends before the Myevent timeline 
             * */
            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents = generateDictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents(ArrayOfInterferringSubEvents.ToList());
            List<CalendarEvent> SortedInterFerringCalendarEvents = SortCalendarEvent(DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Keys.ToList(),false);

            //SubEventsTimeCategories[]

            List<SubCalendarEvent> RigidSubCalendarEvents=new List<SubCalendarEvent>(0);
            List<BusyTimeLine> RigidSubCalendarEventsBusyTimeLine = new List<BusyTimeLine>(0);
            i = 0;
            int j = 0;

            for (; i < SubEventsTimeCategories.Length; i++)//loop detects the rigid subevents and populates RigidSubCalendarEvents list
            {
                j = 0;
                for (; j < SubEventsTimeCategories[i].Count; j++)
                {
                    if (SubEventsTimeCategories[i][j].Rigid)
                    {
                        RigidSubCalendarEvents.Add(SubEventsTimeCategories[i][j]);
                        RigidSubCalendarEventsBusyTimeLine.Add(SubEventsTimeCategories[i][j].ActiveSlot);
                    }
                }
            }


            
            

            TimeLine ReferenceTimeLine = MyCalendarEvent.EventTimeLine;


            ReferenceTimeLine.AddBusySlots(RigidSubCalendarEventsBusyTimeLine.ToArray());//Adds all the rigid elements

            TimeLine[] ArrayOfFreeSpots = getOnlyPertinentTimeFrame(getAllFreeSpots_NoCompleteSchedule(ReferenceTimeLine), ReferenceTimeLine).ToArray();
            ArrayOfFreeSpots = getOnlyPertinentTimeFrame(ArrayOfFreeSpots, ReferenceTimeLine).ToArray();
            //CompleteSchedule.MergeTimeLines(ReferenceTimeLine);//further check that mergre doesnt duplicate timeLines
            //NotInList(SortedInterFerringCalendarEve,

            Dictionary<TimeLine, List<CalendarEvent>> DictTimeLineAndListOfCalendarevent = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<CalendarEvent>>();
            /*foreach (CalendarEvent MyCalendarEvent0 in EndingBeforeDeadlineArray)//Foreach statement pins interferring subevents to start of timeline//Disabling pintostart to see if Buildsnugpossibility can fix this
            {
                foreach (TimeLine MyTimeLine in ArrayOfFreeSpots)
                {
                    
                    TimeLine TimeLineAfterPinToStart=MyCalendarEvent0.PinSubEventsToStart(MyTimeLine, DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[MyCalendarEvent0]);
                    if (TimeLineAfterPinToStart != null)
                    {
                        ReferenceTimeLine.MergeTimeLines(TimeLineAfterPinToStart);
                        break;
                    }
                    
                    //CalendarEvent[] PertinentCalendarEvents = getPertinentCalendarEvents(EndingBeforeDeadlineArray, MyTimeLine);
                    //Array.Sort(PertinentCalendarEvents, CalendarEvent.CompareByEndDate);
                    //DictTimeLineAndListOfCalendarevent.Add(MyTimeLine, PertinentCalendarEvents.ToList());
                }

                ArrayOfFreeSpots = getOnlyPertinentTimeFrame(getAllFreeSpots_NoCompleteSchedule(ReferenceTimeLine),ReferenceTimeLine).ToArray();
            }*/

            SubCalendarEvent[] TempSubCalendarEventsForMyCalendarEvent = new SubCalendarEvent[MyCalendarEvent.NumberOfSplit];


            for (i = 0; i < TempSubCalendarEventsForMyCalendarEvent.Length;i++)//populates the subevents for the calendar event
            {
                TimeSpan MyActiveTimeSpanPerSplit = new TimeSpan((long)((MyCalendarEvent.ActiveDuration.TotalSeconds / MyCalendarEvent.NumberOfSplit) * 10000000));
                    
                TempSubCalendarEventsForMyCalendarEvent[i] = new SubCalendarEvent(MyActiveTimeSpanPerSplit, MyCalendarEvent.Start, MyCalendarEvent.End, new TimeSpan(), MyCalendarEvent.ID);
            }

            /*foreach (TimeLine MyTimeLine in ArrayOfFreeSpots)
            {
                TimeLine TimeLineAfterPinToStart = MyCalendarEvent.PinSubEventsToStart(MyTimeLine, TempSubCalendarEventsForMyCalendarEvent.ToList());
                if (TimeLineAfterPinToStart != null)
                {
                    ReferenceTimeLine.MergeTimeLines(TimeLineAfterPinToStart);
                    break;
                }
            }*/


            /*BusyTimeLine[] BeforeSnugListFunctionalityImplemented=CompleteSchedule.OccupiedSlots;//****IMPORTANT DO NOT DELETE UNLESS YOU UNDERSTAND WHY YOU WANT TO DELETE COMMENT SECTIONN
            CompleteSchedule.OccupiedSlots=ReferenceTimeLine.OccupiedSlots;
            MyCalendarEvent=EvaluateTotalTimeLineAndAssignValidTimeSpots(MyCalendarEvent);
            if (MyCalendarEvent != null)
            {
                return MyCalendarEvent;
            }*/
            List<List<List<SubCalendarEvent>>> SnugListOfPossibleSubCalendarEventsClumps = BuildAllPossibleSnugLists(SortedInterFerringCalendarEvents, MyCalendarEvent, DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents, CalendarEventsTimeCategories, ReferenceTimeLine);
            //Remember Jerome, I need to implement a functionality that permutates through the various options of pin to start option. So take for example two different event timeline that are pertinent to a free spot however one has a dead line preceeding the other, there will be a pin to start for two scenarios, one for each calendar event in which either of them gets pinned first.



            return EvaluateEachSnugPossibiliyOfSnugPossibility(SnugListOfPossibleSubCalendarEventsClumps, ReferenceTimeLine);
            ;//this will not be the final output. I'll need some class that stores the current output of both rearrange busytimelines and deleted timelines
        }

        CalendarEvent EvaluateEachSnugPossibiliyOfSnugPossibility(List<List<List<SubCalendarEvent>>> SnugPossibilityPermutation ,TimeLine ReferenceTimeLine)
        {
            TimeLine CopyOfReferenceTimeLine;
            List<TimeLine> SnugPossibilityTimeLine = new System.Collections.Generic.List<TimeLine>();
            foreach (List<List<SubCalendarEvent>> SnugPermutation in SnugPossibilityPermutation)//goes each permutation of snug possibility generated
            {
                CopyOfReferenceTimeLine = ReferenceTimeLine.CreateCopy();
                SnugPossibilityTimeLine.Add(CopyOfReferenceTimeLine);
                List<TimeLine> ListOfFreeSpots=getOnlyPertinentTimeFrame(getAllFreeSpots_NoCompleteSchedule(CopyOfReferenceTimeLine), CopyOfReferenceTimeLine);
                List<SubCalendarEvent> ReassignedSubEvents = new System.Collections.Generic.List<SubCalendarEvent>();
                for (int i=0; i<ListOfFreeSpots.Count;i++)
                {
                    DateTime RelativeStartTime = ListOfFreeSpots[i].Start;
                    
                    foreach (SubCalendarEvent MySubCalendarEvent in SnugPermutation[i])
                    {
                        SubCalendarEvent CopyOfMySubCalendarEvent = MySubCalendarEvent.createCopy();
                        DateTime RelativeEndtime = RelativeStartTime + (CopyOfMySubCalendarEvent.End - CopyOfMySubCalendarEvent.Start);
                        CopyOfReferenceTimeLine.MergeTimeLines(CopyOfMySubCalendarEvent.EventTimeLine);
                        CopyOfMySubCalendarEvent.ReassignTime(RelativeStartTime, RelativeEndtime);
                        RelativeStartTime = CopyOfMySubCalendarEvent.End;
                    }
                }
            }
            
            return new CalendarEvent();
        }

        int EvaluateRandomnetIndex(TimeLine ReferenFilledReferenceTimeLine)
        {
            return 100;
        }

        int EvaluateClumpingIndex(TimeLine ReferenFilledReferenceTimeLine)
        {
            return 100;
        }

        List<List<List<SubCalendarEvent>>> BuildAllPossibleSnugLists(List<CalendarEvent> SortedInterferringCalendarEvents, CalendarEvent ToBeFittedTimeLine, Dictionary<CalendarEvent, List<SubCalendarEvent>> DictionaryWithBothCalendarEventsAndListOfInterferringSubEvents, List<CalendarEvent>[] CategoryOfSubEvents,TimeLine ReferenceTimeLine)
        {
            /*Name: Jerome Biotidara
             *Description: Function starts by Including all Rigid interferring schedules. Then goes on to setup tightest schedule.
             *Accomplished by:
             *1. stacking subevents of the same calendar event right next to each other.
             *2. Start Snugallotments based on deadline of Calendar Events
             *3. Try creating a snugness that has a snugness duration Greater than or Equal to start time and less than Or equal to the result generated by the difference between the CalendarEvent Deadline and Sum of Interferring subevent durations
             *4  Ensure that when you are assign subcalendar events, the sub calendar events that start within the timeline get noticed and are only allowed to start within the range
             */
            
            

            //TimeLine[] JustFreeSpots = ToBeFittedTimeLine.EventTimeLine.getAllFreeSlots();
            TimeLine[] JustFreeSpots = getAllFreeSpots_NoCompleteSchedule(ReferenceTimeLine);
            List<SubCalendarEvent>[] MyListOfSubCalendarEvents= DictionaryWithBothCalendarEventsAndListOfInterferringSubEvents.Values.ToArray();
            //TimeLine[] FreeSpotsWithOnlyRigids= ToBeFittedTimeLine.EventTimeLine.getAllFreeSlots();
            TimeLine[] FreeSpotsWithOnlyRigids= getAllFreeSpots_NoCompleteSchedule(ReferenceTimeLine);
            List<SubCalendarEvent> ListOfAllInterferringSubCalendarEvents= new List<SubCalendarEvent>();
            List<TimeSpan> ListOfAllInterferringTimeSpans= new List<TimeSpan>();

            foreach (List<SubCalendarEvent> MyList in MyListOfSubCalendarEvents)
            {
                foreach (SubCalendarEvent MySubEvents in MyList)
                {
                    ListOfAllInterferringSubCalendarEvents.Add(MySubEvents);
                    ListOfAllInterferringTimeSpans.Add(MySubEvents.ActiveSlot.BusyTimeSpan);
                }
            }


            List<SubCalendarEvent> ListOfAlreadyAssignedSubCalendarEvents = new System.Collections.Generic.List<SubCalendarEvent>();

            foreach (BusyTimeLine MyBusySlot in ReferenceTimeLine.OccupiedSlots)
            { 
                SubCalendarEvent MySubCalendarEvent = getSubCalendarEvent(MyBusySlot.TimeLineID);
                if (MySubCalendarEvent != null)
                {
                    ListOfAlreadyAssignedSubCalendarEvents.Add(MySubCalendarEvent);
                }
            }

            ListOfAllInterferringSubCalendarEvents=NotInList(ListOfAllInterferringSubCalendarEvents, ListOfAlreadyAssignedSubCalendarEvents);

            Dictionary<TimeLine, List<CalendarEvent>> DictionaryOfFreeTimeLineAndPertinentCalendarEventList = new System.Collections.Generic.Dictionary<TimeLine,System.Collections.Generic.List<CalendarEvent>>();

            Dictionary<TimeLine, Dictionary<CalendarEvent, List<SubCalendarEvent>>> DictionaryOfFreeTimeLineAndDictionaryOfCalendarEventAndListOfSubCalendarEvent = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.Dictionary<CalendarEvent, System.Collections.Generic.List<SubCalendarEvent>>>();

            foreach(TimeLine MyFreeTimeLine in JustFreeSpots)
            {
                CalendarEvent[] MyListOfPertinentCalendarEventsForMyTimeLine =getPertinentCalendarEvents(SortedInterferringCalendarEvents.ToArray(),MyFreeTimeLine);
                Dictionary<CalendarEvent, List<SubCalendarEvent>> MyDictionaryOfCalendarEventAndPertinentSubCalendarEvent = new System.Collections.Generic.Dictionary<CalendarEvent, System.Collections.Generic.List<SubCalendarEvent>>();
                foreach (CalendarEvent MyCalendarEvent in MyListOfPertinentCalendarEventsForMyTimeLine)
                {
                    List<SubCalendarEvent> MyListwe = DictionaryWithBothCalendarEventsAndListOfInterferringSubEvents[MyCalendarEvent];
                    MyDictionaryOfCalendarEventAndPertinentSubCalendarEvent.Add(MyCalendarEvent, MyListwe);
                }
                DictionaryOfFreeTimeLineAndDictionaryOfCalendarEventAndListOfSubCalendarEvent.Add(MyFreeTimeLine, MyDictionaryOfCalendarEventAndPertinentSubCalendarEvent);
                DictionaryOfFreeTimeLineAndPertinentCalendarEventList.Add(MyFreeTimeLine,MyListOfPertinentCalendarEventsForMyTimeLine.ToList());//Next step is to call the snug array. Note: you will need to ensure that when ever a subevent gets used in a free timeline. It will have to be removed from the List so that it cannot be used in another free timeline. Also you need to create every possible permutation. Take for example a calendar event thats pertinent to two different "free timelines". you need to ensure that you have different calls to the snuglist generator that has the calendar event enabled in one and disabled in the other.
            }
            

            List<List<SubCalendarEvent>> EmptyIntialListOfSubCalendarEvemts = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();

            for (int i = 0; i < JustFreeSpots.Length; i++)
            {
                EmptyIntialListOfSubCalendarEvemts.Add(new List<SubCalendarEvent>());
            }
            

            List<List<List<SubCalendarEvent>>> AllTImeLinesWithSnugPossibilities = generateTreeCallsToSnugArray(ListOfAllInterferringSubCalendarEvents, JustFreeSpots.ToList(), 0, EmptyIntialListOfSubCalendarEvemts, BuildDicitionaryOfTimeLineAndSubcalendarEvents(ListOfAllInterferringSubCalendarEvents, DictionaryOfFreeTimeLineAndDictionaryOfCalendarEventAndListOfSubCalendarEvent));
            
           

            return AllTImeLinesWithSnugPossibilities;

        }

        private Dictionary<TimeLine,List<SubCalendarEvent>> BuildDicitionaryOfTimeLineAndSubcalendarEvents(List<SubCalendarEvent> MyInterferringSubCalendarEvents, Dictionary<TimeLine, Dictionary<CalendarEvent,List<SubCalendarEvent>>> DicitonaryTimeLineAndPertinentCalendarEventDictionary)
        {
            List<TimeLine> MyListOfFreeTimelines = DicitonaryTimeLineAndPertinentCalendarEventDictionary.Keys.ToList();
            Dictionary<TimeLine,List<SubCalendarEvent>> DictionaryofTimeLineAndPertinentSubcalendar= new System.Collections.Generic.Dictionary<TimeLine,System.Collections.Generic.List<SubCalendarEvent>>();
            foreach (TimeLine MyTimeLine in MyListOfFreeTimelines)
            {
                Dictionary<CalendarEvent,List<SubCalendarEvent>> MyListOfDictionaryOfCalendarEventAndSubCalendarEvent=DicitonaryTimeLineAndPertinentCalendarEventDictionary[MyTimeLine];
                List<CalendarEvent> MyListOfPertitnentCalendars = MyListOfDictionaryOfCalendarEventAndSubCalendarEvent.Keys.ToList();
                List<SubCalendarEvent>  MyListOfPertinentSubEvent = new List<SubCalendarEvent>();
                foreach (CalendarEvent MyCalendarEvent in MyListOfPertitnentCalendars)
                {
                    var MyTempHolder = MyListOfPertinentSubEvent.Concat(MyListOfDictionaryOfCalendarEventAndSubCalendarEvent[MyCalendarEvent]);
                    MyListOfPertinentSubEvent=MyTempHolder.ToList();
                }

                DictionaryofTimeLineAndPertinentSubcalendar.Add(MyTimeLine, MyListOfPertinentSubEvent);
            }

            return DictionaryofTimeLineAndPertinentSubcalendar;

            /*foreach(TimeLine MyTimeLine in MyListOfFreeTimelines)
            {
                List<SubCalendarEvent> MyTimeLineListToWorkWith = getIntersectionList(MyInterferringSubCalendarEvents, DictionsryofTimeLineAndPertinentSubcalenda[MyTimeLine]);
                
            }
            */
            
            
        }

        List<List<List<SubCalendarEvent>>> generateTreeCallsToSnugArray(List<SubCalendarEvent> AvailableSubCalendarEvents, List<TimeLine> AllTimeLines, int TimeLineIndex, List<List<SubCalendarEvent>> SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar, Dictionary<TimeLine,List<SubCalendarEvent>> DictionaryOfTimelineAndSubcalendarEvents)//, List<SubCalendarEvent> usedSubCalendarEvensts)
        {
            /*
                * Name: jerome Biotidara
                * Description: This function is responsible for generating the snug possibilities for a list containing all timelines it takes 5 elements as its parameters. 
                    -AvailableSubCalendarEvents ->The remaining List of subcalendar events. 
                    -AllTimeLines->A list of the Timelines that needto be populated with the desired snug possibility
                    -TimeLineIndex->An Index keeping track of what timeline the funciton is current working on
                    -FullTimeLineWithSnugListOfSubcalendarEvents->is a list containing A list of subcalendarEvents for each TimeLine
             *      -PertainingSnugPossibilityForTimieline-> is a list of snugpossibilities that apply to a TimeLine in a TimeLineIndex
             *      -DictionaryOfTimelineAndSubcalendarEvents-> A dictionary of TimeLine and interferring subcalendarevents
             */
            List<List<List<SubCalendarEvent>>> ListOfAllSnugPossibilitiesInRespectiveTImeLines = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();
            if (AllTimeLines.Count < 1)
            {
                return null;
            }

            if ((TimeLineIndex >= AllTimeLines.Count)||(AvailableSubCalendarEvents.Count<1))
            {
                ListOfAllSnugPossibilitiesInRespectiveTImeLines.Add(SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar);
                return ListOfAllSnugPossibilitiesInRespectiveTImeLines;
            }

            List<TimeSpan> MySubcalendarEventTimespans = new System.Collections.Generic.List<TimeSpan>();
            List<SubCalendarEvent> MyPertinentSubcalendarEvents = ListPartOfList(AvailableSubCalendarEvents, DictionaryOfTimelineAndSubcalendarEvents[AllTimeLines[TimeLineIndex]]);
            foreach (SubCalendarEvent MySubcalendarevent in MyPertinentSubcalendarEvents)
            {
                MySubcalendarEventTimespans.Add(MySubcalendarevent.ActiveDuration);
            }
            SnugArray MySnugArray= new SnugArray(MySubcalendarEventTimespans,AllTimeLines[TimeLineIndex].TimelineSpan);
            List <List<TimeSpan>> AllSnugPossibilities = MySnugArray.MySnugPossibleEntries;
            /*
             AllSnugPossibilities looks like this 
             * {{43,23,54},{43,54,23},{23,43,54}...} where each digit is a time span and remember it generates every permutation.
             */
            List<List<List<SubCalendarEvent>>>SubcalendarMyList=BuildListMatchingTimelineAndSubCalendarEvent(AllSnugPossibilities, MyPertinentSubcalendarEvents);
            /*
             the function BuildListMatchingTimelineAndSubCalendarEvent builds a list of Subcalendar events for each timespan.
             * assuming subCal0 has TimeSpan =43
             * assuming subCal1 has TimeSpan =23
             * assuming subCal2 has TimeSpan =54
             * assuming subCal3 has TimeSpan =43
             * assuming subCal4 has TimeSpan =23
             * assuming subCal5 has TimeSpan =23
             * SubcalendarMyList will have the array {{{SubCal0,Subcal3},{subCal1,subCal4,subCal5},{subCal2}},{{SubCal0,Subcal3},{subCal2},{subCal1,subCal4,subCal5}},{{subCal1,subCal4,subCal5},{SubCal0,Subcal3},{subCal2}}...}
             */
            List<List<SubCalendarEvent>> SerialIzedListOfSubCalendarEvents = SerializeListOfMatchingSubcalendarEvents(SubcalendarMyList);
            List<SubCalendarEvent> AvailableSubCalendarEventsAfterRemovingAssingedSnugElements=new List<SubCalendarEvent>();
            

            
            foreach (List<SubCalendarEvent> AlreadyAssignedSubCalendarEvent in SerialIzedListOfSubCalendarEvents)
            {
                AvailableSubCalendarEventsAfterRemovingAssingedSnugElements = NotInList(AvailableSubCalendarEvents.ToList(), AlreadyAssignedSubCalendarEvent);
                SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar[TimeLineIndex] = AlreadyAssignedSubCalendarEvent;
                var MyHolder = ListOfAllSnugPossibilitiesInRespectiveTImeLines.Concat(generateTreeCallsToSnugArray(AvailableSubCalendarEventsAfterRemovingAssingedSnugElements, AllTimeLines, TimeLineIndex + 1, SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar.ToList(), DictionaryOfTimelineAndSubcalendarEvents));
                ListOfAllSnugPossibilitiesInRespectiveTImeLines = MyHolder.ToList();
            }

            return ListOfAllSnugPossibilitiesInRespectiveTImeLines;
        }

        List<List<SubCalendarEvent>> SerializeListOfMatchingSubcalendarEvents(List <List<List<SubCalendarEvent>>> MyListofListOfListOfSubcalendar)
        { 
            List<List<SubCalendarEvent>> MyTotalCompilation = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
            foreach (List<List<SubCalendarEvent>> MyListOfList in MyListofListOfListOfSubcalendar)
            {
                var MyHolder = MyTotalCompilation.Concat(SpreadOutList.GenerateListOfSubCalendarEvent(MyListOfList));
                MyTotalCompilation = MyHolder.ToList();
                //MyTotalCompilation.Add(SpreadOutList.GenerateListOfSubCalendarEvent(MyListOfList))
            }

            return MyTotalCompilation;
        }

        List<SubCalendarEvent> NotInList(List<SubCalendarEvent> ListToCheck, List<SubCalendarEvent> MyCurrentList)
        {
            foreach(SubCalendarEvent MySubCalendarEvent in MyCurrentList)
            {
                ListToCheck.Remove(MySubCalendarEvent);
            }
            return ListToCheck;
        }

        List<SubCalendarEvent> ListPartOfList(List<SubCalendarEvent> ListToCheck, List<SubCalendarEvent> MyCurrentList)
        {
            List<SubCalendarEvent> InListElements = new List<SubCalendarEvent>();

            foreach (SubCalendarEvent MySubCalendarEvent0 in ListToCheck)
            {
                foreach(SubCalendarEvent MySubCalendarEvent1 in MyCurrentList)
                {
                    if(MySubCalendarEvent1.ID==MySubCalendarEvent0.ID)
                    { 
                        InListElements.Add(MySubCalendarEvent1); 
                    }
                }
            }
            return InListElements;
        }


        bool isSubCalendarEventInList(SubCalendarEvent SubCalendarEventToCheck, List<SubCalendarEvent> MyCurrentList)
        {
            foreach (SubCalendarEvent MySubCalendarEvent in MyCurrentList)
            {
                if (SubCalendarEventToCheck.ID == MySubCalendarEvent.ID)
                {
                    return true;
                }
            }
            return false;
        }


        List<SubCalendarEvent> getIntersectionList(List<SubCalendarEvent> ListToCheck, List<SubCalendarEvent> MyCurrentList)
        {
            List<SubCalendarEvent> MyNewList= new System.Collections.Generic.List<SubCalendarEvent>();
            foreach(SubCalendarEvent MySubCalendarEvent in MyCurrentList)
            {
                if(ListToCheck.IndexOf(MySubCalendarEvent)>=0)
                {
                    MyNewList.Add(MySubCalendarEvent);
                }
            }

            return MyNewList;
        }
        
        
        
        private CalendarEvent[] getPertinentCalendarEvents(CalendarEvent[] PossibleCalendarEvents, TimeLine VerifiableSpace)
        {
            List<CalendarEvent> MyPertinentCalendarList = new List<CalendarEvent>();
             
            foreach (CalendarEvent MyCalendarEvent in PossibleCalendarEvents)
            {
                
                if((MyCalendarEvent.Start<VerifiableSpace.End)&&(MyCalendarEvent.End>VerifiableSpace.Start))
                {
                    MyPertinentCalendarList.Add(MyCalendarEvent);
                }
            }
            return MyPertinentCalendarList.ToArray();
        }

        TimeLine CreateTimeLineWithSubCalendarEventsStackedNextToEachOtherWithinRestrictiveCalendarEvent(Dictionary<CalendarEvent, List<SubCalendarEvent>> DictionaryCalendarEventAndJustInterferringSubCalendar)
        {
            /*
             * Name: Jerome Biotidara
             * Description: This function is called when a timeline in which the Calendar events have subcalendar events are set to their last possible assignable position.
             * e.g CalendarEvent1 with Time Line (12:00Am 1/1/2013 and 12:00AM 1/3/2013) and a busy time slot of 7 hours and 2 subcalendar events. 
             * The expected final result will be SubEvent[0]=>5:00pm 1/2/2013 SubEvent[0]=>8:30pm 1/2/2013
             * Note this is only called with a dicitionary this is because thois is supposed to be called in a case wehere we have to optimize the assignation of the subevents
             */
            TimeLine MyTimeLine = new TimeLine();
            List<CalendarEvent>MyDeadlineSortedListOfCalendarEvents =  QuickSortCalendarEventFunctionFromEnd(DictionaryCalendarEventAndJustInterferringSubCalendar.Keys.ToList(), 0, (DictionaryCalendarEventAndJustInterferringSubCalendar.Keys.Count - 1), (DictionaryCalendarEventAndJustInterferringSubCalendar.Keys.Count / 2));
            
            return new TimeLine();
        }

        List<List<List<SubCalendarEvent>>> BuildListMatchingTimelineAndSubCalendarEvent(List<List<TimeSpan>> ListOfSnugPossibilities, List<SubCalendarEvent> ListOfSubCalendarEvents)
        {
            /*
             *Name: Jerome Biotidara
             *Description: This function is to be called when all the snug possibilites are generated and we need to verify a match of the subcalendar events and Timespan. Essentially this function builds a permutation of the timeline and subcalendar match ups. Take for example two Subcalendar events with the same Timespan it means they can both possibly fit. So it builds two lists with bit possibilities.
             *It gets ListOfSnugPossibilities which is a List of Snug Timespan Permutation e.g for a time slot of 10 and entries 1,2,3,4 ListOfSnugPossibilities has ({1,2,3,4},{4,2,3,1,{1,3,2,4}}), ListOfSubCalendarEvents is just a list of SubCalendar Events
             *It ultimately returns a List similar to ListOfSnugPossibilities however each time Element is an array of Subcalendar Events that match the TimeSpan of time element
             *Date:07/02/2013
             */
            List<TimeSpan> AllTimesSpan = new List<TimeSpan>();
            Dictionary<TimeSpan,List<SubCalendarEvent>> DictionaryOfTImespanandSubCalendarEvent= new System.Collections.Generic.Dictionary<TimeSpan,System.Collections.Generic.List<SubCalendarEvent>>();
            List<List<List<SubCalendarEvent>>> MatchingListOfSnugPossibilitesWithSubcalendarEvents = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();
            /*foreach (List<TimeSpan> MySnugPossibility in ListOfSnugPossibilities)
            {
                var MyConcatList = AllTimesSpan.Concat(MySnugPossibility);
                AllTimesSpan=MyConcatList.ToList();
            }*/

            foreach (SubCalendarEvent MySubCalendar in ListOfSubCalendarEvents)
            {
                try
                {
                    DictionaryOfTImespanandSubCalendarEvent.Add(MySubCalendar.ActiveSlot.BusyTimeSpan,new List<SubCalendarEvent>());
                    DictionaryOfTImespanandSubCalendarEvent[MySubCalendar.ActiveSlot.BusyTimeSpan].Add(MySubCalendar);
                    //DictionaryOfTImespanandSubCalendarEvent.Add(MyTimeSpan, new List<SubCalendarEvent>());
                }
                catch ( Exception e)
                {
                    DictionaryOfTImespanandSubCalendarEvent[MySubCalendar.ActiveSlot.BusyTimeSpan].Add(MySubCalendar);
                }
            }

            


            foreach (List<TimeSpan> MySnugPossibility in ListOfSnugPossibilities)
            {
                List<List <SubCalendarEvent>> SnugPossibiltySubcalendarEvent= new List<List<SubCalendarEvent>>();
                foreach (TimeSpan MyTimeSpan in MySnugPossibility)
                {

                    SnugPossibiltySubcalendarEvent.Add(DictionaryOfTImespanandSubCalendarEvent[MyTimeSpan]);
                }
                MatchingListOfSnugPossibilitesWithSubcalendarEvents.Add(SnugPossibiltySubcalendarEvent);
            }

            return MatchingListOfSnugPossibilitesWithSubcalendarEvents;
            
        }

        Dictionary<TimeSpan, List<SubCalendarEvent>> BuildDictionaryOfSubCalendarEventsAndTimespan(List<TimeSpan> MyListOfTimeSpan, List<SubCalendarEvent> MyListOfSubCalenedarEvents)
        {
            Dictionary<TimeSpan, List<SubCalendarEvent>> MyDictionaryOfTimeSpanAndSubCalendarEvents = new Dictionary<TimeSpan, List<SubCalendarEvent>>();
            foreach (SubCalendarEvent MySubCalendarEvent in  MyListOfSubCalenedarEvents)
            {
                try
                {
                    MyDictionaryOfTimeSpanAndSubCalendarEvents.Add(MySubCalendarEvent.ActiveDuration, new List<SubCalendarEvent>());//Test to see if  the dictionary key is built with an object reference oir the value of the timespan. This is because the object is probably passed by referencxe
                    MyDictionaryOfTimeSpanAndSubCalendarEvents[MySubCalendarEvent.ActiveDuration].Add(MySubCalendarEvent);

                }
                catch(Exception ex)
                {
                    MyDictionaryOfTimeSpanAndSubCalendarEvents[MySubCalendarEvent.ActiveDuration].Add(MySubCalendarEvent);
                }
            }
            return MyDictionaryOfTimeSpanAndSubCalendarEvents;
        }


        List<CalendarEvent>[] CategorizeCalendarEventTimeLine(TimeLine MyRange, BusyTimeLine[] SortedByStartArrayOfBusyTimeLine)//This  returns An array of a List of SubCalendarEvents. The function goes through each of the BUsytimeline Events and verifies if its parent calendar Event has a start Time that is eiter earlier than range, within range or start and ends within the range
        { 
            List<SubCalendarEvent>[] ArrayOfDifferentVaryingSubEventsCategories = new List<SubCalendarEvent>[4];
            List<CalendarEvent>[] ArrayCalendarEventOfDifferentVaryingSubEventsCategories = new List<CalendarEvent>[4];
            
            List<SubCalendarEvent> MyArrayOfBeforeAndEndsAfterBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfBeforeAndEndsAfterBusyTimelines = new List<CalendarEvent>(0);
            
            List<SubCalendarEvent> MyArrayOfBeforeAndEndsBeforeBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfBeforeAndEndsBeforeBusyTimelines = new List<CalendarEvent>(0);

            List<SubCalendarEvent> MyArrayOfStartsAfterAndEndsAfterBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfStartsAfterAndEndsAfterBusyTimelines = new List<CalendarEvent>(0);
            
            List<SubCalendarEvent> MyArrayOfStartsAndEndsBeforeBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfStartsAndEndsBeforeBusyTimelines = new List<CalendarEvent>(0);
            int i = 0;
            for (; i < SortedByStartArrayOfBusyTimeLine.Length; i++)
            {
                EventID MyEventID= new EventID(SortedByStartArrayOfBusyTimeLine[i].TimeLineID);
                string ParentCalendarEventID=MyEventID.getLevelID(0);

                if ((AllEventDictionary[ParentCalendarEventID].Start < MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End > MyRange.End))//checks if Calendar Event Starts starts before range and ends after range
                { 
                    MyArrayOfBeforeAndEndsAfterBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfBeforeAndEndsAfterBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID]);
                }

                if ((AllEventDictionary[ParentCalendarEventID].Start < MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End <= MyRange.End))//checks if Calendar Event Starts starts before range and ends within range
                {
                    MyArrayOfBeforeAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfBeforeAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID]);
                }

                if ((AllEventDictionary[ParentCalendarEventID].Start >= MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End <= MyRange.End))//checks if Calendar Event Starts within range and ends within range
                {
                    MyArrayOfStartsAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfStartsAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID]);
                }

                if ((AllEventDictionary[ParentCalendarEventID].Start >= MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End > MyRange.End))//checks if Calendar Event Starts within range and ends after range
                {
                    MyArrayOfStartsAfterAndEndsAfterBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfStartsAfterAndEndsAfterBusyTimelines.Add((AllEventDictionary[ParentCalendarEventID]));
                }
            }
            ArrayOfDifferentVaryingSubEventsCategories[0] = MyArrayOfBeforeAndEndsAfterBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[1] = MyArrayOfBeforeAndEndsBeforeBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[2] = MyArrayOfStartsAfterAndEndsAfterBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[3] = MyArrayOfStartsAndEndsBeforeBusyTimelines;

            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[0] = MyArrayCalendarEventOfBeforeAndEndsAfterBusyTimelines;
            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[1] = MyArrayCalendarEventOfBeforeAndEndsBeforeBusyTimelines;
            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[2] = MyArrayCalendarEventOfStartsAfterAndEndsAfterBusyTimelines;
            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[3] = MyArrayCalendarEventOfStartsAndEndsBeforeBusyTimelines;
            return ArrayCalendarEventOfDifferentVaryingSubEventsCategories;
        }
        
        List<SubCalendarEvent>[] CategorizeSubEventsTimeLine(TimeLine MyRange, BusyTimeLine[] SortedByStartArrayOfBusyTimeLine)//This  returns An array of a List of SubCalendarEvents. The function goes through each of the BUsytimeline Events and verifies if its parent calendar Event has a start Time that is eiter earlier than range, within range or start and ends within the range
        {
            List<SubCalendarEvent>[] ArrayOfDifferentVaryingSubEventsCategories = new List<SubCalendarEvent>[4];
            List<CalendarEvent>[] ArrayCalendarEventOfDifferentVaryingSubEventsCategories = new List<CalendarEvent>[4];
            
            List<SubCalendarEvent> MyArrayOfBeforeAndEndsAfterBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfBeforeAndEndsAfterBusyTimelines = new List<CalendarEvent>(0);
            
            List<SubCalendarEvent> MyArrayOfBeforeAndEndsBeforeBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfBeforeAndEndsBeforeBusyTimelines = new List<CalendarEvent>(0);

            List<SubCalendarEvent> MyArrayOfStartsAfterAndEndsAfterBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfStartsAfterAndEndsAfterBusyTimelines = new List<CalendarEvent>(0);
            
            List<SubCalendarEvent> MyArrayOfStartsAndEndsBeforeBusyTimelines = new List<SubCalendarEvent>(0);
            List<CalendarEvent> MyArrayCalendarEventOfStartsAndEndsBeforeBusyTimelines = new List<CalendarEvent>(0);
            int i = 0;
            for (; i < SortedByStartArrayOfBusyTimeLine.Length; i++)
            {
                EventID MyEventID= new EventID(SortedByStartArrayOfBusyTimeLine[i].TimeLineID);
                string ParentCalendarEventID=MyEventID.getLevelID(0);

                if ((AllEventDictionary[ParentCalendarEventID].Start < MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End > MyRange.End))//checks if Calendar Event Starts starts before range and ends after range
                { 
                    MyArrayOfBeforeAndEndsAfterBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfBeforeAndEndsAfterBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID]);
                }

                if ((AllEventDictionary[ParentCalendarEventID].Start < MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End <= MyRange.End))//checks if Calendar Event Starts starts before range and ends within range
                {
                    MyArrayOfBeforeAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfBeforeAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID]);
                }

                if ((AllEventDictionary[ParentCalendarEventID].Start >= MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End <= MyRange.End))//checks if Calendar Event Starts within range and ends within range
                {
                    MyArrayOfStartsAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfStartsAndEndsBeforeBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID]);
                }

                if ((AllEventDictionary[ParentCalendarEventID].Start >= MyRange.Start) && (AllEventDictionary[ParentCalendarEventID].End > MyRange.End))//checks if Calendar Event Starts within range and ends after range
                {
                    MyArrayOfStartsAfterAndEndsAfterBusyTimelines.Add(AllEventDictionary[ParentCalendarEventID].getSubEvent(MyEventID));
                    MyArrayCalendarEventOfStartsAfterAndEndsAfterBusyTimelines.Add((AllEventDictionary[ParentCalendarEventID]));
                }
            }
            ArrayOfDifferentVaryingSubEventsCategories[0] = MyArrayOfBeforeAndEndsAfterBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[1] = MyArrayOfBeforeAndEndsBeforeBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[2] = MyArrayOfStartsAfterAndEndsAfterBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[3] = MyArrayOfStartsAndEndsBeforeBusyTimelines;

            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[0] = MyArrayCalendarEventOfBeforeAndEndsAfterBusyTimelines;
            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[1] = MyArrayCalendarEventOfBeforeAndEndsBeforeBusyTimelines;
            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[2] = MyArrayCalendarEventOfStartsAfterAndEndsAfterBusyTimelines;
            ArrayCalendarEventOfDifferentVaryingSubEventsCategories[3] = MyArrayCalendarEventOfStartsAndEndsBeforeBusyTimelines;
            return ArrayOfDifferentVaryingSubEventsCategories;
        }

        
        
        List<BusyTimeLine>[] getEdgeElements(TimeLine MyRangeOfTimeLine, BusyTimeLine[] ArrayOfTInterferringime)
        {
            List<BusyTimeLine> ListOfEdgeElements = new List<BusyTimeLine>();
            List<BusyTimeLine> StartEdge = new List<BusyTimeLine>();
            List<BusyTimeLine> EndEdge = new List<BusyTimeLine>();
            int i=0;
            for (; i < ArrayOfTInterferringime.Length; i++)
            {
                if (!MyRangeOfTimeLine.IsTimeLineWithin(ArrayOfTInterferringime[i]))
                {
                    ListOfEdgeElements.Add(ArrayOfTInterferringime[i]);
                }
            }

            i = 0;

            for (; i < ListOfEdgeElements.Count; i++)
            {
                if (ListOfEdgeElements[i].Start < MyRangeOfTimeLine.Start)
                {
                    StartEdge.Add(ListOfEdgeElements[i]);
                }
                else 
                {
                    EndEdge.Add(ListOfEdgeElements[i]);
                }
            }

            List<BusyTimeLine>[] StartAndEndEdgeList = new List<BusyTimeLine>[2];
            StartAndEndEdgeList[0] = StartEdge;
            StartAndEndEdgeList[1] = EndEdge;
            return StartAndEndEdgeList;
        }

        private SubCalendarEvent[] getInterferringSubEvents(CalendarEvent MyCalendarEvent)
        {
            List<SubCalendarEvent> MyArrayOfInterferringSubCalendarEvents = new List<SubCalendarEvent>(0);//List that stores the InterFerring List
            int i = 0;
            int lengthOfCalendarSubEvent = 0;
            foreach (KeyValuePair<string, CalendarEvent> MyCalendarEventDictionaryEntry in AllEventDictionary)
            {
                lengthOfCalendarSubEvent = MyCalendarEventDictionaryEntry.Value.AllEvents.Length;
                i = 0;
                for (; i < lengthOfCalendarSubEvent; i++)
                {
                    if ((MyCalendarEvent.EventTimeLine.IsDateTimeWithin(MyCalendarEventDictionaryEntry.Value.AllEvents[i].Start))||(MyCalendarEvent.EventTimeLine.IsDateTimeWithin(MyCalendarEventDictionaryEntry.Value.AllEvents[i].End)))
                    {
                        MyArrayOfInterferringSubCalendarEvents.Add(MyCalendarEventDictionaryEntry.Value.AllEvents[i]);
                    }
                }
            }

            return MyArrayOfInterferringSubCalendarEvents.ToArray();
        }

        public TimeLine[] SplitFreeSpotsInToSubEventTimeSlots(TimeLine[] AllAvailableFreeSpots,int NumberOfAllotments,TimeSpan TotalActiveDuration)//function is responsible for dividing busy timeline. Also sapcing them out
        {
            TimeLine[] MyArrayOfToBeAssignedTimeLine = new TimeLine[NumberOfAllotments];
            double IdealTimePerAllotment = TotalActiveDuration.TotalSeconds / NumberOfAllotments;
            Dictionary<TimeLine, int> TimeLineAndFitCount=new Dictionary<TimeLine, int>();
            int i=0;
            int FitCount=0;
            for (; i < AllAvailableFreeSpots.Length; i++)
            {
                FitCount = (int)(AllAvailableFreeSpots[i].TimelineSpan.TotalSeconds / IdealTimePerAllotment);
                TimeLineAndFitCount.Add(AllAvailableFreeSpots[i], FitCount);
            }
            i=0;
            int[] AllFitCount = TimeLineAndFitCount.Values.ToArray();
            int myTotalFitCount = 0;
            for (; i < AllFitCount.Length; i++)
            {
                myTotalFitCount+=AllFitCount[i];
            }

            if (myTotalFitCount >= NumberOfAllotments)
            {
                MyArrayOfToBeAssignedTimeLine = IdealAllotAndInsert(TimeLineAndFitCount, MyArrayOfToBeAssignedTimeLine, new TimeSpan((long)IdealTimePerAllotment * 10000000));
            }
            else 
            {
                MessageBox.Show("Sorry you will need to implement Dress funtionality to afford space.!!!");
                return null;
            }

            return MyArrayOfToBeAssignedTimeLine;
        }

        public TimeLine[] IdealAllotAndInsert(Dictionary<TimeLine, int> AvailablFreeSpots, TimeLine[] MyArrayOfToBeAssignedTimeLine,TimeSpan IdealTimePerAllotment)
        {
            int i = 0;
            int j=0;
            int k = 0;
            int TopCounter = 0;
            TimeLine[] ArrayOfTimelineRanges = AvailablFreeSpots.Keys.ToArray();//array of FreeTimeLineRanges
            Dictionary<TimeLine, TimeLine[]> TimeLineDictionary = new Dictionary<TimeLine, TimeLine[]>();
            int[] ArrayOfFitCount= AvailablFreeSpots.Values.ToArray();//array list of fit count
            
            /*foreach (TimeLine MyTimeLine in MyArrayOfToBeAssignedTimeLine)
            { 
                TimeLineDictionary.Add(MyTimeLine, new TimeLine[1]);
            }
            */
            for (; i < MyArrayOfToBeAssignedTimeLine.Length && k < MyArrayOfToBeAssignedTimeLine.Length; k++)//k counts to ensure we dont get infinite loop in scenario where all spaces are too small
            {
                j = 0;
                TopCounter++;
                for (; ((j < ArrayOfTimelineRanges.Length) && (i < MyArrayOfToBeAssignedTimeLine.Length)); j++)
                {
                    if (AvailablFreeSpots[ArrayOfTimelineRanges[j]] > 0)
                    {
                        try
                        {
                            TimeLineDictionary.Add(ArrayOfTimelineRanges[j], SplitAndAssign(ArrayOfTimelineRanges[j], TopCounter, IdealTimePerAllotment));
                        }
                        catch 
                        {
                            TimeLineDictionary[ArrayOfTimelineRanges[j]] = SplitAndAssign(ArrayOfTimelineRanges[j], TopCounter, IdealTimePerAllotment);
                        }
                        --AvailablFreeSpots[ArrayOfTimelineRanges[j]];
                        i += TopCounter;
                    }
                }
            }

            TimeLine[][] MyArrayOfTimeLines=TimeLineDictionary.Values.ToArray();

            k = 0;
            i=0;
            for (; i < MyArrayOfToBeAssignedTimeLine.Length; )
            { 
                j=0;
                for (; ((j < MyArrayOfTimeLines.Length) && (i < MyArrayOfToBeAssignedTimeLine.Length)); j++)
                {
                    
                    foreach (TimeLine MyTimeLine in MyArrayOfTimeLines[j] )
                    {
                        if (i < MyArrayOfToBeAssignedTimeLine.Length)
                        {
                            MyArrayOfToBeAssignedTimeLine[i] = MyTimeLine;
                            i++;
                        }
                    }
                }
            }

            return MyArrayOfToBeAssignedTimeLine;
        }

        TimeLine JustFillEachOneFirst(TimeLine MyRange, TimeSpan MyActiveDuration)
        {
            return CentralizeYourSelfWithinRange(MyRange, MyActiveDuration);
            
        }

        TimeLine[] SplitAndAssign(TimeLine RangeOfSplit, int NumberOfSplits, TimeSpan DurationLength)
        {

            TimeSpan MySpanForEachSection = new TimeSpan((long)((RangeOfSplit.TimelineSpan.TotalMilliseconds / NumberOfSplits) * 10000));
            TimeLine[] MyArrayOfElements = new TimeLine[NumberOfSplits];
            int i = 0;
            TimeLine MyReferenceRange=new TimeLine();
            double TotalMilliseconds=i*(MySpanForEachSection.TotalMilliseconds);
            DateTime RangeStart;
            DateTime RangeEnd;
            for (; i < NumberOfSplits; i++)
            {
                TotalMilliseconds = i * (MySpanForEachSection.TotalMilliseconds);
                RangeStart = RangeOfSplit.Start.AddMilliseconds(TotalMilliseconds);
                RangeEnd = RangeStart.Add(MySpanForEachSection);
                MyArrayOfElements[i] = CentralizeYourSelfWithinRange(new TimeLine(RangeStart, RangeEnd), DurationLength);
            }
            return MyArrayOfElements;

        }

        TimeLine CentralizeYourSelfWithinRange(TimeLine Range, TimeSpan Centralized)
        {
            TimeSpan Difference = Range.TimelineSpan - Centralized;
            TimeLine CentralizedTimeline=new TimeLine();
            if (Difference.TotalMilliseconds<0)
            {
                MessageBox.Show("Cannot generate TimeLine Because Difference is less than zero.\nWill Not Fit!!!");
                return CentralizedTimeline;
            }
            DateTime MyStart=Range.Start.AddSeconds(Difference.TotalSeconds / 2);
            CentralizedTimeline= new TimeLine(MyStart, MyStart.Add(Centralized));
            return CentralizedTimeline;
        }

        private SubCalendarEvent AssignSubEventTimeSlot(SubCalendarEvent MySubEvent)
        {
            BusyTimeLine [] ArrayOfInvadingEvents = CheckIfMyRangeAlreadyHasSchedule(CompleteSchedule.OccupiedSlots, MySubEvent);
            TimeLine[] AvailableFreeSpots = getAllFreeSpots(new TimeLine(MySubEvent.Start, MySubEvent.End));
            for (int i = 0; i < AvailableFreeSpots.Length; i++)
            {
                if (AvailableFreeSpots[i].TimelineSpan > (MySubEvent.ActiveDuration))
                {
                    DateTime DurationStartTime;
                    TimeSpan MyTimeSpan = new TimeSpan((((AvailableFreeSpots[i].TimelineSpan - MySubEvent.ActiveDuration).Milliseconds)/2)*10000);
                    DurationStartTime=AvailableFreeSpots[i].Start.Add(MyTimeSpan);
                    MySubEvent.ActiveSlot = new BusyTimeLine(MySubEvent.ID, DurationStartTime, DurationStartTime.Add(MySubEvent.ActiveDuration));
                }
            } 
            return MySubEvent;
        }

        /*QUICK SORT SECTION START*/

        

        static public List<BusyTimeLine> QuickSortFunctionFromStart(List<BusyTimeLine> MyArray,int LeftIndexPassed,int RightIndexPassed,int PivotPassed)
        {
            //Console.WriteLine("\n EntryLeft is " + LeftIndexPassed.ToString() + " EntryRight is " + RightIndexPassed.ToString() + " EntryPivot is " + PivotPassed.ToString() + "\n");
            int PivotIndex;
            BusyTimeLine PivotValue,Holder;
            if ((LeftIndexPassed == RightIndexPassed) || (MyArray.Count < 2))
            {
                return MyArray;
            }
            int LeftValue, LeftIndex=LeftIndexPassed, RightValue, RightIndex=RightIndexPassed;
            bool Detected = true;
            PivotIndex = PivotPassed;
            RightIndex = GetRightThatsLessFromStart(MyArray, PivotIndex, RightIndexPassed);
            LeftIndex = GetLeftThatsGreaterFromStart(MyArray, PivotIndex, LeftIndexPassed);
            //Console.WriteLine("\n ##Left is " + LeftIndex.ToString() + " Right is " + RightIndex.ToString() + "\n");
            PivotValue = MyArray[PivotIndex];
            Holder = MyArray[LeftIndex];
            MyArray[LeftIndex]= MyArray[RightIndex];
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
                        if (MyArray[RightIndexPassed].Start < MyArray[NextPivot].Start)
                        {
                            Holder = MyArray[NextPivot];
                            MyArray[NextPivot] = MyArray[RightIndexPassed];
                            MyArray[RightIndexPassed]= Holder;
                            return MyArray;
                        }
                    }
                    MyArray = QuickSortFunctionFromStart(MyArray, (PivotIndex + 1), RightIndexPassed, NextPivot); //right Partition sort
                }
                else
                {
                    //Console.Write("maybe RIGHT \t");
                    MyArray = QuickSortFunctionFromStart(MyArray, (PivotIndex), RightIndexPassed, (PivotIndex + ((RightIndexPassed - PivotIndex) / 2))); //right Partition sort
                }
                if ((PivotIndex - 1) > (LeftIndexPassed)-1)
                {
                    //Console.Write("maybe LEFT TOP\t");
                    middleDifference = (PivotIndex - 1) - LeftIndexPassed;
                    NextPivot = LeftIndexPassed + (middleDifference / 2);
                    if (middleDifference <= 1)
                    {
                        NextPivot = PivotIndex - 1;
                        if (MyArray[LeftIndexPassed].Start > MyArray[NextPivot].Start)
                        {
                            Holder = MyArray[LeftIndexPassed];
                            MyArray[LeftIndexPassed] = MyArray[NextPivot];
                            MyArray[NextPivot]= Holder;
                            return MyArray;
                        }
                    }
                    MyArray = QuickSortFunctionFromStart(MyArray, LeftIndexPassed, (PivotIndex - 1), NextPivot);//left Partition sort 
                }
                else 
                {
                    //Console.Write("maybe LEFT \t");
                    MyArray = QuickSortFunctionFromStart(MyArray, LeftIndexPassed, (PivotIndex), (LeftIndexPassed + ((PivotIndex) - LeftIndexPassed) / 2));//left Partition sort 
                }
            }
            else 
            {
                MyArray = QuickSortFunctionFromStart(MyArray, LeftIndexPassed, RightIndexPassed, PivotPassed);
            }
            return MyArray;
        }

        static public int GetLeftThatsGreaterFromStart(List<BusyTimeLine> MyArray, int MyPivot, int LeftStartinPosition)
        {
            for (int i = LeftStartinPosition; i <= MyPivot; i++)
            {
                if (MyArray[i].Start > MyArray[MyPivot].Start)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public int GetRightThatsLessFromStart(List<BusyTimeLine> MyArray, int MyPivot, int RightStartinPosition)
        {
            for (int i = RightStartinPosition; i >= MyPivot; i--)
            {
                if (MyArray[i].Start < MyArray[MyPivot].Start)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public List<BusyTimeLine> QuickSortFunctionFromEnd(List<BusyTimeLine> MyArray, int LeftIndexPassed, int RightIndexPassed, int PivotPassed)
        {
            //Console.WriteLine("\n EntryLeft is " + LeftIndexPassed.ToString() + " EntryRight is " + RightIndexPassed.ToString() + " EntryPivot is " + PivotPassed.ToString() + "\n");
            int PivotIndex;
            BusyTimeLine PivotValue, Holder;
            if ((LeftIndexPassed == RightIndexPassed) || (MyArray.Count < 2))
            {
                return MyArray;
            }
            int LeftValue, LeftIndex = LeftIndexPassed, RightValue, RightIndex = RightIndexPassed;
            bool Detected = true;
            PivotIndex = PivotPassed;
            RightIndex = GetRightThatsLessFromStart(MyArray, PivotIndex, RightIndexPassed);
            LeftIndex = GetLeftThatsGreaterFromStart(MyArray, PivotIndex, LeftIndexPassed);
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
                    middleDifference = RightIndexPassed - (PivotIndex + 1);
                    NextPivot = (PivotIndex + 1) + (middleDifference / 2);
                    if (middleDifference <= 1)
                    {
                        NextPivot = PivotIndex + 1;
                        if (MyArray[RightIndexPassed].End < MyArray[NextPivot].End)
                        {
                            Holder = MyArray[NextPivot];
                            MyArray[NextPivot] = MyArray[RightIndexPassed];
                            MyArray[RightIndexPassed] = Holder;
                            return MyArray;
                        }
                    }
                    MyArray = QuickSortFunctionFromEnd(MyArray, (PivotIndex + 1), RightIndexPassed, NextPivot); //right Partition sort
                }
                else
                {
                    //Console.Write("maybe RIGHT \t");
                    MyArray = QuickSortFunctionFromEnd(MyArray, (PivotIndex), RightIndexPassed, (PivotIndex + ((RightIndexPassed - PivotIndex) / 2))); //right Partition sort
                }
                if ((PivotIndex - 1) > (LeftIndexPassed) - 1)
                {
                    //Console.Write("maybe LEFT TOP\t");
                    middleDifference = (PivotIndex - 1) - LeftIndexPassed;
                    NextPivot = LeftIndexPassed + (middleDifference / 2);
                    if (middleDifference <= 1)
                    {
                        NextPivot = PivotIndex - 1;
                        if (MyArray[LeftIndexPassed].End > MyArray[NextPivot].End)
                        {
                            Holder = MyArray[LeftIndexPassed];
                            MyArray[LeftIndexPassed] = MyArray[NextPivot];
                            MyArray[NextPivot] = Holder;
                            return MyArray;
                        }
                    }
                    MyArray = QuickSortFunctionFromEnd(MyArray, LeftIndexPassed, (PivotIndex - 1), NextPivot);//left Partition sort 
                }
                else
                {
                    //Console.Write("maybe LEFT \t");
                    MyArray = QuickSortFunctionFromEnd(MyArray, LeftIndexPassed, (PivotIndex), (LeftIndexPassed + ((PivotIndex) - LeftIndexPassed) / 2));//left Partition sort 
                }
            }
            else
            {
                MyArray = QuickSortFunctionFromEnd(MyArray, LeftIndexPassed, RightIndexPassed, PivotPassed);
            }
            return MyArray;
        }

        static public int GetLeftThatsGreaterFromEnd(List<BusyTimeLine> MyArray, int MyPivot, int LeftStartinPosition)
        {
            for (int i = LeftStartinPosition; i <= MyPivot; i++)
            {
                if (MyArray[i].End > MyArray[MyPivot].End)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public int GetRightThatsLessFromEnd(List<BusyTimeLine> MyArray, int MyPivot, int RightStartinPosition)
        {
            for (int i = RightStartinPosition; i >= MyPivot; i--)
            {
                if (MyArray[i].End < MyArray[MyPivot].End)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public List<CalendarEvent> QuickSortCalendarEventFunctionFromEnd(List<CalendarEvent> MyArray, int LeftIndexPassed, int RightIndexPassed, int PivotPassed)
        {
            //Console.WriteLine("\n EntryLeft is " + LeftIndexPassed.ToString() + " EntryRight is " + RightIndexPassed.ToString() + " EntryPivot is " + PivotPassed.ToString() + "\n");
            int PivotIndex;
            CalendarEvent PivotValue, Holder;
            if ((LeftIndexPassed == RightIndexPassed) || (MyArray.Count < 2))
            {
                return MyArray;
            }
            int LeftValue, LeftIndex = LeftIndexPassed, RightValue, RightIndex = RightIndexPassed;
            bool Detected = true;
            PivotIndex = PivotPassed;
            RightIndex = GetRightCalendarEventThatsLessFromEnd(MyArray, PivotIndex, RightIndexPassed);
            LeftIndex = GetLeftCalendarEventThatsGreaterFromEnd(MyArray, PivotIndex, LeftIndexPassed);
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
                    middleDifference = RightIndexPassed - (PivotIndex + 1);
                    NextPivot = (PivotIndex + 1) + (middleDifference / 2);
                    if (middleDifference <= 1)
                    {
                        NextPivot = PivotIndex + 1;
                        if (MyArray[RightIndexPassed].End < MyArray[NextPivot].End)
                        {
                            Holder = MyArray[NextPivot];
                            MyArray[NextPivot] = MyArray[RightIndexPassed];
                            MyArray[RightIndexPassed] = Holder;
                            return MyArray;
                        }
                    }
                    MyArray = QuickSortCalendarEventFunctionFromEnd(MyArray, (PivotIndex + 1), RightIndexPassed, NextPivot); //right Partition sort
                }
                else
                {
                    //Console.Write("maybe RIGHT \t");
                    MyArray = QuickSortCalendarEventFunctionFromEnd(MyArray, (PivotIndex), RightIndexPassed, (PivotIndex + ((RightIndexPassed - PivotIndex) / 2))); //right Partition sort
                }
                if ((PivotIndex - 1) > (LeftIndexPassed) - 1)
                {
                    //Console.Write("maybe LEFT TOP\t");
                    middleDifference = (PivotIndex - 1) - LeftIndexPassed;
                    NextPivot = LeftIndexPassed + (middleDifference / 2);
                    if (middleDifference <= 1)
                    {
                        NextPivot = PivotIndex - 1;
                        if (MyArray[LeftIndexPassed].End > MyArray[NextPivot].End)
                        {
                            Holder = MyArray[LeftIndexPassed];
                            MyArray[LeftIndexPassed] = MyArray[NextPivot];
                            MyArray[NextPivot] = Holder;
                            return MyArray;
                        }
                    }
                    MyArray = QuickSortCalendarEventFunctionFromEnd(MyArray, LeftIndexPassed, (PivotIndex - 1), NextPivot);//left Partition sort 
                }
                else
                {
                    //Console.Write("maybe LEFT \t");
                    MyArray = QuickSortCalendarEventFunctionFromEnd(MyArray, LeftIndexPassed, (PivotIndex), (LeftIndexPassed + ((PivotIndex) - LeftIndexPassed) / 2));//left Partition sort 
                }
            }
            else
            {
                MyArray = QuickSortCalendarEventFunctionFromEnd(MyArray, LeftIndexPassed, RightIndexPassed, PivotPassed);
            }
            return MyArray;
        }

        static public int GetLeftCalendarEventThatsGreaterFromEnd(List<CalendarEvent> MyArray, int MyPivot, int LeftStartinPosition)
        {
            for (int i = LeftStartinPosition; i <= MyPivot; i++)
            {
                if (MyArray[i].End > MyArray[MyPivot].End)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public int GetRightCalendarEventThatsLessFromEnd(List<CalendarEvent> MyArray, int MyPivot, int RightStartinPosition)
        {
            for (int i = RightStartinPosition; i >= MyPivot; i--)
            {
                if (MyArray[i].End < MyArray[MyPivot].End)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public List<CalendarEvent> QuickSortCalendarEventFunctionFromStart(List<CalendarEvent> MyArray, int LeftIndexPassed, int RightIndexPassed, int PivotPassed)
        {
            //Console.WriteLine("\n EntryLeft is " + LeftIndexPassed.ToString() + " EntryRight is " + RightIndexPassed.ToString() + " EntryPivot is " + PivotPassed.ToString() + "\n");
            int PivotIndex;
            CalendarEvent PivotValue, Holder;
            if ((LeftIndexPassed == RightIndexPassed) || (MyArray.Count < 2))
            {
                return MyArray;
            }
            int LeftValue, LeftIndex = LeftIndexPassed, RightValue, RightIndex = RightIndexPassed;
            bool Detected = true;
            PivotIndex = PivotPassed;
            RightIndex = GetRightCalendarEventThatsLessFromEnd(MyArray, PivotIndex, RightIndexPassed);
            LeftIndex = GetLeftCalendarEventThatsGreaterFromEnd(MyArray, PivotIndex, LeftIndexPassed);
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
                    middleDifference = RightIndexPassed - (PivotIndex + 1);
                    NextPivot = (PivotIndex + 1) + (middleDifference / 2);
                    if (middleDifference <= 1)
                    {
                        NextPivot = PivotIndex + 1;
                        if (MyArray[RightIndexPassed].End < MyArray[NextPivot].End)
                        {
                            Holder = MyArray[NextPivot];
                            MyArray[NextPivot] = MyArray[RightIndexPassed];
                            MyArray[RightIndexPassed] = Holder;
                            return MyArray;
                        }
                    }
                    MyArray = QuickSortCalendarEventFunctionFromStart(MyArray, (PivotIndex + 1), RightIndexPassed, NextPivot); //right Partition sort
                }
                else
                {
                    //Console.Write("maybe RIGHT \t");
                    MyArray = QuickSortCalendarEventFunctionFromStart(MyArray, (PivotIndex), RightIndexPassed, (PivotIndex + ((RightIndexPassed - PivotIndex) / 2))); //right Partition sort
                }
                if ((PivotIndex - 1) > (LeftIndexPassed) - 1)
                {
                    //Console.Write("maybe LEFT TOP\t");
                    middleDifference = (PivotIndex - 1) - LeftIndexPassed;
                    NextPivot = LeftIndexPassed + (middleDifference / 2);
                    if (middleDifference <= 1)
                    {
                        NextPivot = PivotIndex - 1;
                        if (MyArray[LeftIndexPassed].Start > MyArray[NextPivot].Start)
                        {
                            Holder = MyArray[LeftIndexPassed];
                            MyArray[LeftIndexPassed] = MyArray[NextPivot];
                            MyArray[NextPivot] = Holder;
                            return MyArray;
                        }
                    }
                    MyArray = QuickSortCalendarEventFunctionFromStart(MyArray, LeftIndexPassed, (PivotIndex - 1), NextPivot);//left Partition sort 
                }
                else
                {
                    //Console.Write("maybe LEFT \t");
                    MyArray = QuickSortCalendarEventFunctionFromStart(MyArray, LeftIndexPassed, (PivotIndex), (LeftIndexPassed + ((PivotIndex) - LeftIndexPassed) / 2));//left Partition sort 
                }
            }
            else
            {
                MyArray = QuickSortCalendarEventFunctionFromStart(MyArray, LeftIndexPassed, RightIndexPassed, PivotPassed);
            }
            return MyArray;
        }

        static public int GetLeftCalendarEventThatsGreaterFromStart(List<CalendarEvent> MyArray, int MyPivot, int LeftStartinPosition)
        {
            for (int i = LeftStartinPosition; i <= MyPivot; i++)
            {
                if (MyArray[i].Start > MyArray[MyPivot].Start)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public int GetRightCalendarEventThatsLessFromStart(List<CalendarEvent> MyArray, int MyPivot, int RightStartinPosition)
        {
            for (int i = RightStartinPosition; i >= MyPivot; i--)
            {
                if (MyArray[i].Start < MyArray[MyPivot].Start)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public List<SubCalendarEvent> QuickSortSubCalendarEventFunctionFromEnd(List<SubCalendarEvent> MyArray, int LeftIndexPassed, int RightIndexPassed, int PivotPassed)
        {
            //Console.WriteLine("\n EntryLeft is " + LeftIndexPassed.ToString() + " EntryRight is " + RightIndexPassed.ToString() + " EntryPivot is " + PivotPassed.ToString() + "\n");
            int PivotIndex;
            SubCalendarEvent PivotValue, Holder;
            if ((LeftIndexPassed == RightIndexPassed) || (MyArray.Count < 2))
            {
                return MyArray;
            }
            int LeftValue, LeftIndex = LeftIndexPassed, RightValue, RightIndex = RightIndexPassed;
            bool Detected = true;
            PivotIndex = PivotPassed;
            RightIndex = GetRightSubCalendarEventThatsLessFromEnd(MyArray, PivotIndex, RightIndexPassed);
            LeftIndex = GetLeftSubCalendarEventThatsGreaterFromEnd(MyArray, PivotIndex, LeftIndexPassed);
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
                    middleDifference = RightIndexPassed - (PivotIndex + 1);
                    NextPivot = (PivotIndex + 1) + (middleDifference / 2);
                    if (middleDifference <= 1)
                    {
                        NextPivot = PivotIndex + 1;
                        if (MyArray[RightIndexPassed].End < MyArray[NextPivot].End)
                        {
                            Holder = MyArray[NextPivot];
                            MyArray[NextPivot] = MyArray[RightIndexPassed];
                            MyArray[RightIndexPassed] = Holder;
                            return MyArray;
                        }
                    }
                    MyArray = QuickSortSubCalendarEventFunctionFromEnd(MyArray, (PivotIndex + 1), RightIndexPassed, NextPivot); //right Partition sort
                }
                else
                {
                    //Console.Write("maybe RIGHT \t");
                    MyArray = QuickSortSubCalendarEventFunctionFromEnd(MyArray, (PivotIndex), RightIndexPassed, (PivotIndex + ((RightIndexPassed - PivotIndex) / 2))); //right Partition sort
                }
                if ((PivotIndex - 1) > (LeftIndexPassed) - 1)
                {
                    //Console.Write("maybe LEFT TOP\t");
                    middleDifference = (PivotIndex - 1) - LeftIndexPassed;
                    NextPivot = LeftIndexPassed + (middleDifference / 2);
                    if (middleDifference <= 1)
                    {
                        NextPivot = PivotIndex - 1;
                        if (MyArray[LeftIndexPassed].End > MyArray[NextPivot].End)
                        {
                            Holder = MyArray[LeftIndexPassed];
                            MyArray[LeftIndexPassed] = MyArray[NextPivot];
                            MyArray[NextPivot] = Holder;
                            return MyArray;
                        }
                    }
                    MyArray = QuickSortSubCalendarEventFunctionFromEnd(MyArray, LeftIndexPassed, (PivotIndex - 1), NextPivot);//left Partition sort 
                }
                else
                {
                    //Console.Write("maybe LEFT \t");
                    MyArray = QuickSortSubCalendarEventFunctionFromEnd(MyArray, LeftIndexPassed, (PivotIndex), (LeftIndexPassed + ((PivotIndex) - LeftIndexPassed) / 2));//left Partition sort 
                }
            }
            else
            {
                MyArray = QuickSortSubCalendarEventFunctionFromEnd(MyArray, LeftIndexPassed, RightIndexPassed, PivotPassed);
            }
            return MyArray;
        }

        static public int GetLeftSubCalendarEventThatsGreaterFromEnd(List<SubCalendarEvent> MyArray, int MyPivot, int LeftStartinPosition)
        {
            for (int i = LeftStartinPosition; i <= MyPivot; i++)
            {
                if (MyArray[i].End > MyArray[MyPivot].End)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public int GetRightSubCalendarEventThatsLessFromEnd(List<SubCalendarEvent> MyArray, int MyPivot, int RightStartinPosition)
        {
            for (int i = RightStartinPosition; i >= MyPivot; i--)
            {
                if (MyArray[i].End < MyArray[MyPivot].End)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public List<SubCalendarEvent> QuickSortSubCalendarEventFunctionFromStart(List<SubCalendarEvent> MyArray, int LeftIndexPassed, int RightIndexPassed, int PivotPassed)
        {
            //Console.WriteLine("\n EntryLeft is " + LeftIndexPassed.ToString() + " EntryRight is " + RightIndexPassed.ToString() + " EntryPivot is " + PivotPassed.ToString() + "\n");
            int PivotIndex;
            SubCalendarEvent PivotValue, Holder;
            if ((LeftIndexPassed == RightIndexPassed) || (MyArray.Count < 2))
            {
                return MyArray;
            }
            int LeftValue, LeftIndex = LeftIndexPassed, RightValue, RightIndex = RightIndexPassed;
            bool Detected = true;
            PivotIndex = PivotPassed;
            RightIndex = GetRightSubCalendarEventThatsLessFromEnd(MyArray, PivotIndex, RightIndexPassed);
            LeftIndex = GetLeftSubCalendarEventThatsGreaterFromEnd(MyArray, PivotIndex, LeftIndexPassed);
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
                    middleDifference = RightIndexPassed - (PivotIndex + 1);
                    NextPivot = (PivotIndex + 1) + (middleDifference / 2);
                    if (middleDifference <= 1)
                    {
                        NextPivot = PivotIndex + 1;
                        if (MyArray[RightIndexPassed].End < MyArray[NextPivot].End)
                        {
                            Holder = MyArray[NextPivot];
                            MyArray[NextPivot] = MyArray[RightIndexPassed];
                            MyArray[RightIndexPassed] = Holder;
                            return MyArray;
                        }
                    }
                    MyArray = QuickSortSubCalendarEventFunctionFromStart(MyArray, (PivotIndex + 1), RightIndexPassed, NextPivot); //right Partition sort
                }
                else
                {
                    //Console.Write("maybe RIGHT \t");
                    MyArray = QuickSortSubCalendarEventFunctionFromStart(MyArray, (PivotIndex), RightIndexPassed, (PivotIndex + ((RightIndexPassed - PivotIndex) / 2))); //right Partition sort
                }
                if ((PivotIndex - 1) > (LeftIndexPassed) - 1)
                {
                    //Console.Write("maybe LEFT TOP\t");
                    middleDifference = (PivotIndex - 1) - LeftIndexPassed;
                    NextPivot = LeftIndexPassed + (middleDifference / 2);
                    if (middleDifference <= 1)
                    {
                        NextPivot = PivotIndex - 1;
                        if (MyArray[LeftIndexPassed].Start > MyArray[NextPivot].Start)
                        {
                            Holder = MyArray[LeftIndexPassed];
                            MyArray[LeftIndexPassed] = MyArray[NextPivot];
                            MyArray[NextPivot] = Holder;
                            return MyArray;
                        }
                    }
                    MyArray = QuickSortSubCalendarEventFunctionFromStart(MyArray, LeftIndexPassed, (PivotIndex - 1), NextPivot);//left Partition sort 
                }
                else
                {
                    //Console.Write("maybe LEFT \t");
                    MyArray = QuickSortSubCalendarEventFunctionFromStart(MyArray, LeftIndexPassed, (PivotIndex), (LeftIndexPassed + ((PivotIndex) - LeftIndexPassed) / 2));//left Partition sort 
                }
            }
            else
            {
                MyArray = QuickSortSubCalendarEventFunctionFromStart(MyArray, LeftIndexPassed, RightIndexPassed, PivotPassed);
            }
            return MyArray;
        }

        static public int GetLeftSubCalendarEventThatsGreaterFromStart(List<SubCalendarEvent> MyArray, int MyPivot, int LeftStartinPosition)
        {
            for (int i = LeftStartinPosition; i <= MyPivot; i++)
            {
                if (MyArray[i].Start > MyArray[MyPivot].Start)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        static public int GetRightSubCalendarEventThatsLessFromStart(List<SubCalendarEvent> MyArray, int MyPivot, int RightStartinPosition)
        {
            for (int i = RightStartinPosition; i >= MyPivot; i--)
            {
                if (MyArray[i].Start < MyArray[MyPivot].Start)
                {
                    return i;
                }
            }
            return MyPivot;
        }

        /*QUICK SORT SECTION END*/

        public BusyTimeLine[] CheckIfMyRangeAlreadyHasSchedule(BusyTimeLine[] BusySlots, SubCalendarEvent MySubEvent)
        {
            List<BusyTimeLine> InvadingEvents=new List<BusyTimeLine>();
            int i = 0;
            for (; i < BusySlots.Length; i++)
            {
                if ((BusySlots[i].End>MySubEvent.Start)&&(BusySlots[i].End<MySubEvent.End))
                {
                    InvadingEvents.Add(BusySlots[i]);
                }
            }


            InvadingEvents = SortBusyTimeline(InvadingEvents, true);


            return InvadingEvents.ToArray();
        }


        static public List<CalendarEvent> SortEvents(List<CalendarEvent> MyUnSortedEvent, bool StartOrEnd)
        {
            int MiddleRoundedDown = ((MyUnSortedEvent.Count - 1) / 2);
            if (StartOrEnd)
            { return QuickSortCalendarEventFunctionFromStart(MyUnSortedEvent, 0, MyUnSortedEvent.Count - 1, MiddleRoundedDown); }
            else
            { return QuickSortCalendarEventFunctionFromEnd(MyUnSortedEvent, 0, MyUnSortedEvent.Count - 1, MiddleRoundedDown); }
        }

        static public List<SubCalendarEvent> SortSubCalendarEvents(List<SubCalendarEvent> MyUnSortedEvent, bool StartOrEnd)
        {
            int MiddleRoundedDown = ((MyUnSortedEvent.Count - 1) / 2);
            if (StartOrEnd)
            { return QuickSortSubCalendarEventFunctionFromStart(MyUnSortedEvent, 0, MyUnSortedEvent.Count - 1, MiddleRoundedDown); }
            else
            { return QuickSortSubCalendarEventFunctionFromEnd(MyUnSortedEvent, 0, MyUnSortedEvent.Count - 1, MiddleRoundedDown); }
        }

        static public List<CalendarEvent> SortCalendarEvent(List<CalendarEvent> MyUnSortedEvent, bool StartOrEnd)
        {
            int MiddleRoundedDown = ((MyUnSortedEvent.Count - 1) / 2);
            if (StartOrEnd)
            { return QuickSortCalendarEventFunctionFromStart(MyUnSortedEvent, 0, MyUnSortedEvent.Count - 1, MiddleRoundedDown); }
            else
            { return QuickSortCalendarEventFunctionFromEnd(MyUnSortedEvent, 0, MyUnSortedEvent.Count - 1, MiddleRoundedDown); }
        }
      
        static public List<BusyTimeLine> SortBusyTimeline(List<BusyTimeLine> MyUnsortedEvents,bool StartOrEnd)//True is from start. False is from end
        {
            int MiddleRoundedDown = ((MyUnsortedEvents.Count - 1) / 2);
            if (StartOrEnd)
            { return QuickSortFunctionFromStart(MyUnsortedEvents, 0, MyUnsortedEvents.Count - 1, MiddleRoundedDown); }
            else
            {return QuickSortFunctionFromEnd(MyUnsortedEvents, 0, MyUnsortedEvents.Count - 1, MiddleRoundedDown); }
        }

        public TimeLine[] getAllFreeSpots_NoCompleteSchedule(TimeLine MyTimeLine)//Gets an array of all the freespots between busy spots in given time line, note attribute completeschedule is not used in finding freespots
        {
            BusyTimeLine[] AllBusySlots = MyTimeLine.OccupiedSlots;
            DateTime FinalCompleteScheduleDate;
            AllBusySlots = SortBusyTimeline(AllBusySlots.ToList(), true).ToArray();
            TimeLine[] AllFreeSlots = new TimeLine[AllBusySlots.Length];

            if (AllBusySlots.Length > 1)
            {
                AllFreeSlots = new TimeLine[(AllBusySlots.Length) + 1];
            }
            else
            {
                if (AllBusySlots.Length == 1)
                {
                    AllFreeSlots = new TimeLine[2];
                    AllFreeSlots[0] = new TimeLine(MyTimeLine.Start, AllBusySlots[0].Start.AddMilliseconds(0));
                    AllFreeSlots[1] = new TimeLine(AllBusySlots[0].End.AddMilliseconds(0), MyTimeLine.End);
                    /*AllFreeSlots[0] = new TimeLine(DateTime.Now, AllBusySlots[0].Start.AddMilliseconds(-1));
                    AllFreeSlots[1] = new TimeLine(AllBusySlots[0].End.AddMilliseconds(1), AllBusySlots[0].End.AddYears(10));*/
                }
                else
                {
                    AllFreeSlots = new TimeLine[1];
                    AllFreeSlots[0] = new TimeLine(MyTimeLine.Start, MyTimeLine.End);
                }
                return AllFreeSlots;
            }
            DateTime ReferenceTime = MyTimeLine.Start;
            /*if (MyTimeLine.Start < MyTimeLine.Start)
            {
                ReferenceTime = MyTimeLine.Start;
            }*/

            for (int i = 0; i < (AllBusySlots.Length); i++)//Free Spots Are only between two busy Slots. So Index Counter starts from 1 get start of second busy
            {

                AllFreeSlots[i] = new TimeLine(ReferenceTime, AllBusySlots[i].Start);
                ReferenceTime = AllBusySlots[i].End;
                //AllFreeSlots[i] = new TimeLine(AllBusySlots[i].End, AllBusySlots[i + 1].Start.AddMilliseconds(-1));
            }
            FinalCompleteScheduleDate = MyTimeLine.End;
            if (FinalCompleteScheduleDate < MyTimeLine.End)
            {
                FinalCompleteScheduleDate = MyTimeLine.End;
            }
            //AllFreeSlots[AllBusySlots.Length-1] = new TimeLine(DateTime.Now, AllBusySlots[0].Start);
            AllFreeSlots[AllFreeSlots.Length - 1] = new TimeLine(AllBusySlots[AllBusySlots.Length - 1].End, FinalCompleteScheduleDate);
            List<TimeLine> SpecificFreeSpots = new List<TimeLine>(0);
            for (int i = 0; i < AllFreeSlots.Length; i++)//Free Spots Are only between two busy Slots. So Index Counter starts from 1 get start of second busy
            {
                //                AllFreeSlots[i] = new TimeLine(AllBusySlots[i].End.AddMilliseconds(1), AllBusySlots[i + 1].Start.AddMilliseconds(-1));
                if ((MyTimeLine.Start < AllFreeSlots[i].End) && (AllFreeSlots[i].Start < MyTimeLine.End))
                {
                    SpecificFreeSpots.Add(AllFreeSlots[i]);
                }
            }

            return SpecificFreeSpots.ToArray();
        }

        public TimeLine[] getAllFreeSpots(TimeLine MyTimeLine)//Gets an array of all the freespots between busy spots in given time line. Checks CompleteSchedule for the limits of free spots
        {
            BusyTimeLine[] AllBusySlots = CompleteSchedule.OccupiedSlots;
            DateTime FinalCompleteScheduleDate;
            AllBusySlots=SortBusyTimeline(AllBusySlots.ToList(),true).ToArray();
            TimeLine[] AllFreeSlots = new TimeLine[AllBusySlots.Length];

            if (AllBusySlots.Length > 1)
            {
                AllFreeSlots = new TimeLine[(AllBusySlots.Length)+1];
            }
            else 
            {
                if (AllBusySlots.Length == 1)
                {
                    AllFreeSlots = new TimeLine[2];
                    AllFreeSlots[0] = new TimeLine(MyTimeLine.Start, AllBusySlots[0].Start.AddMilliseconds(0));
                    AllFreeSlots[1] = new TimeLine(AllBusySlots[0].End.AddMilliseconds(0), MyTimeLine.End);
                    /*AllFreeSlots[0] = new TimeLine(DateTime.Now, AllBusySlots[0].Start.AddMilliseconds(-1));
                    AllFreeSlots[1] = new TimeLine(AllBusySlots[0].End.AddMilliseconds(1), AllBusySlots[0].End.AddYears(10));*/
                }
                else 
                {
                    AllFreeSlots = new TimeLine[1];
                    AllFreeSlots[0] = new TimeLine(MyTimeLine.Start, MyTimeLine.End);
                }
                return AllFreeSlots;
            }
            DateTime ReferenceTime = CompleteSchedule.Start;
            /*if (MyTimeLine.Start < CompleteSchedule.Start)
            {
                ReferenceTime = MyTimeLine.Start;
            }*/

            for (int i = 0; i < (AllBusySlots.Length); i++)//Free Spots Are only between two busy Slots. So Index Counter starts from 1 get start of second busy
            {

                AllFreeSlots[i] = new TimeLine(ReferenceTime, AllBusySlots[i].Start);
                ReferenceTime=AllBusySlots[i].End;
                //AllFreeSlots[i] = new TimeLine(AllBusySlots[i].End, AllBusySlots[i + 1].Start.AddMilliseconds(-1));
            }
            FinalCompleteScheduleDate = CompleteSchedule.End;
            if (FinalCompleteScheduleDate < MyTimeLine.End)
            {
                FinalCompleteScheduleDate = MyTimeLine.End;
            }
            //AllFreeSlots[AllBusySlots.Length-1] = new TimeLine(DateTime.Now, AllBusySlots[0].Start);
            AllFreeSlots[AllFreeSlots.Length-1] = new TimeLine(AllBusySlots[AllBusySlots.Length - 1].End, FinalCompleteScheduleDate);
            List<TimeLine> SpecificFreeSpots = new List<TimeLine>(0);
            for (int i = 0; i < AllFreeSlots.Length; i++)//Free Spots Are only between two busy Slots. So Index Counter starts from 1 get start of second busy
            {
//                AllFreeSlots[i] = new TimeLine(AllBusySlots[i].End.AddMilliseconds(1), AllBusySlots[i + 1].Start.AddMilliseconds(-1));
                if ((MyTimeLine.Start < AllFreeSlots[i].End) && (AllFreeSlots[i].Start < MyTimeLine.End))
                {
                    SpecificFreeSpots.Add(AllFreeSlots[i]);
                }
            }

            return SpecificFreeSpots.ToArray();
        }
        
        public void WriteToOutlook(CalendarEvent MyEvent)
        {
            int i = 0;
            for (; i < MyEvent.AllEvents.Length; i++)
            {
                MyEvent.AllEvents[i].ThirdPartyID = AddAppointment(MyEvent.AllEvents[i], MyEvent.Name);
            }
            if (MyEvent.RepetitionStatus)
            {
                LoopThroughAddRepeatEvents(MyEvent.Repeat);
            }

        }

        public void LoopThroughAddRepeatEvents(Repetition MyRepetition)
        {
            int i = 0;
            for(;i<MyRepetition.RecurringCalendarEvents.Length;i++)
            {
                //WriteToOutlook(MyRepetition.RecurringCalendarEvents[i]);
            }
        }

        public void RemoveFromOutlook(CalendarEvent MyEvent)
        {

                int i = 0;
                for (; i < MyEvent.AllEvents.Length; i++)
                {
                    DeleteAppointment(MyEvent.AllEvents[i], MyEvent.Name, MyEvent.AllEvents[i].ThirdPartyID);
                }
                if (MyEvent.RepetitionStatus)
                {
                    LoopThroughRemoveRepeatEvents(MyEvent.Repeat);
                }
            
        }

        public void LoopThroughRemoveRepeatEvents(Repetition MyRepetition)
        {
            int i = 0;
            for (; i < MyRepetition.RecurringCalendarEvents.Length; i++)
            {
                RemoveFromOutlook(MyRepetition.RecurringCalendarEvents[i]);
            }
        }

        private string AddAppointment(SubCalendarEvent ActiveSection, string NameOfParentCalendarEvent)
        {
            try
            {
                Outlook.Application app = new Microsoft.Office.Interop.Outlook.Application();
                Outlook.AppointmentItem newAppointment = (Outlook.AppointmentItem)app.CreateItem(Outlook.OlItemType.olAppointmentItem);
                    /*(Outlook.AppointmentItem)
                this.Application.CreateItem(Outlook.OlItemType.olAppointmentItem);*/
                newAppointment.Start = ActiveSection.Start;// DateTime.Now.AddHours(2);
                newAppointment.End = ActiveSection.End;// DateTime.Now.AddHours(3);
                newAppointment.Location = "TBD";
                newAppointment.Body ="JustTesting";
                newAppointment.AllDayEvent = false;
                newAppointment.Subject = ActiveSection.ID + "**" + NameOfParentCalendarEvent;
                /*newAppointment.Recipients.Add("Roger Harui");
                Outlook.Recipients sentTo = newAppointment.Recipients;
                Outlook.Recipient sentInvite = null;
                sentInvite = sentTo.Add("Holly Holt");
                sentInvite.Type = (int)Outlook.OlMeetingRecipientType
                    .olRequired;
                sentInvite = sentTo.Add("David Junca ");
                sentInvite.Type = (int)Outlook.OlMeetingRecipientType
                    .olOptional;
                sentTo.ResolveAll();*/
                newAppointment.Save();
                //newAppointment.EntryID;

                //newAppointment.Display(true);
                return newAppointment.EntryID;
            }
            catch (Exception ex)
            {
                MessageBox.Show("The following error occurred: " + ex.Message);
                return "";
            }
        }

        private void DeleteAppointment(SubCalendarEvent ActiveSection, string NameOfParentCalendarEvent, string entryID)
        {
            if (entryID == "")
            {
                return;
            }
            Outlook.Application outlookApp = new Microsoft.Office.Interop.Outlook.Application();
            Outlook.MAPIFolder calendar = outlookApp.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderCalendar);

            Outlook.Items calendarItems = calendar.Items;

            Outlook.AppointmentItem item = calendarItems[ActiveSection.ID + "**" + NameOfParentCalendarEvent] as Outlook.AppointmentItem;
            item.Delete();
            /*Outlook.RecurrencePattern pattern =
                item.GetRecurrencePattern();
            Outlook.AppointmentItem itemDelete = pattern.
                GetOccurrence(new DateTime(2006, 6, 28, 8, 0, 0));

            if (itemDelete != null)
            {
                itemDelete.Delete();
            }*/
        }
        
        public void WriteToLog(CalendarEvent MyEvent)//writes to an XML Log file. Takes calendar event as an argument
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load("MyEventLog.xml");
            xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/LastIDCounter").InnerText = MyEvent.ID;
            XmlNodeList EventSchedulesNodes = xmldoc.DocumentElement.SelectNodes("/ScheduleLog/EventSchedules");
            XmlNodeList EventScheduleNodes = xmldoc.DocumentElement.SelectNodes("/ScheduleLog/EventSchedules/EventSchedule");
            //EventScheduleNodes = new XmlNodeList();
            //XmlElement EventScheduleNode = CreateEventScheduleNode(MyEvent, xmldoc);
            XmlElement EventScheduleNode = CreateEventScheduleNode(MyEvent);
            //EventSchedulesNodes[0].PrependChild(xmldoc.CreateElement("EventSchedule"));
            //EventSchedulesNodes[0].ChildNodes[0].InnerXml = CreateEventScheduleNode(MyEvent).InnerXml;
            XmlNode MyImportedNode = xmldoc.ImportNode(EventScheduleNode as XmlNode, true);
                //(EventScheduleNode, true);
            if (!UpdateInnerXml(ref EventScheduleNodes, "ID", MyEvent.ID.ToString(), EventScheduleNode))
            {
                xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/EventSchedules").AppendChild(MyImportedNode);
            }
            xmldoc.Save("MyEventLog.xml");
        }

        //public XmlElement CreateEventScheduleNode(CalendarEvent MyEvent, XmlDocument xmldoc)
        public XmlElement CreateEventScheduleNode(CalendarEvent MyEvent)
        {
            XmlDocument xmldoc = new XmlDocument();
            
            
            XmlElement MyEventScheduleNode = xmldoc.CreateElement("EventSchedule");
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Completed"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Completed.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("RepetitionFlag"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.RepetitionStatus.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("EventSubSchedules"));
            //MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Repetition.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("RigidFlag"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Rigid.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Duration"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.ActiveDuration.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Split"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.NumberOfSplit.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Deadline"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.End.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("PrepTime"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Preparation.ToString();
            //MyEventScheduleNode.PrependChild(xmldoc.CreateElement("PreDeadlineFlag"));
            //MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Pre.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("PreDeadline"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.PreDeadline.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("StartTime"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Start.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Name"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Name.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("ID"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.ID.ToString();
            if (MyEvent.RepetitionStatus)
            {
                MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Recurrence"));
                MyEventScheduleNode.ChildNodes[0].InnerXml = CreateRepetitionNode(MyEvent.Repeat).InnerXml;
                //MyEventScheduleNode.PrependChild();
                //MyEventScheduleNode.PrependChild(CreateRepetitionNode(MyEvent.Repeat));
            }
            else 
            {
                MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Recurrence"));
                
            }
            XmlNode SubScheduleNodes = MyEventScheduleNode.SelectSingleNode("EventSubSchedules");
            foreach (SubCalendarEvent MySubEvent in MyEvent.AllEvents)
            {
                SubScheduleNodes.PrependChild(xmldoc.CreateElement("EventSubSchedule"));
                SubScheduleNodes.ChildNodes[0].InnerXml=CreateSubScheduleNode(MySubEvent).InnerXml;
            }
            //MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.ID;

            
            return MyEventScheduleNode;
        }

        public XmlElement CreateRepetitionNode(Repetition RepetitionObjEntry)//This takes a repetition object, and creates a Repetition XmlNode
        { 
            int i=0;
            XmlDocument xmldoc = new XmlDocument();
            XmlElement RepeatCalendarEventsNode = xmldoc.CreateElement("Recurrence");//Defines umbrella Repeat XmlNode 
            RepeatCalendarEventsNode.PrependChild(xmldoc.CreateElement("RepeatStartDate"));
            RepeatCalendarEventsNode.ChildNodes[0].InnerText = RepetitionObjEntry.Range.Start.ToString();
            RepeatCalendarEventsNode.PrependChild(xmldoc.CreateElement("RepeatEndDate"));
            RepeatCalendarEventsNode.ChildNodes[0].InnerText = RepetitionObjEntry.Range.End.ToString();
            RepeatCalendarEventsNode.PrependChild(xmldoc.CreateElement("RepeatFrequency"));
            RepeatCalendarEventsNode.ChildNodes[0].InnerText = RepetitionObjEntry.Frequency;
            XmlNode XmlNodeForRepeatListOfEvents = xmldoc.CreateElement("RepeatCalendarEvents");
            XmlElement RepeatCalendarEventNode;//Declares Repeat XmlNode 
            for (; i < RepetitionObjEntry.RecurringCalendarEvents.Length; i++)//For loop goes through each classEvent in repeat object and generates an xmlnode
            { 
                RepeatCalendarEventNode=xmldoc.CreateElement("RepeatCalendarEvent");
                RepeatCalendarEventNode.InnerXml = CreateEventScheduleNode(RepetitionObjEntry.RecurringCalendarEvents[i]).InnerXml;
                XmlNodeForRepeatListOfEvents.PrependChild(RepeatCalendarEventNode);
            }
            RepeatCalendarEventsNode.PrependChild(XmlNodeForRepeatListOfEvents);
            return RepeatCalendarEventsNode;
        }

        public XmlElement CreateSubScheduleNode(SubCalendarEvent MySubEvent)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement MyEventSubScheduleNode = xmldoc.CreateElement("EventSubSchedule");
            //MyEventSubScheduleNode.Name = "EventSubSchedule";
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("EndTime"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.End.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("StartTime"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.Start.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Duration"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.ActiveDuration.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ActiveEndTime"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.ActiveSlot.End.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ActiveStartTime"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.ActiveSlot.Start.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("PrepTime"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.Preparation.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ThirdPartyID"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.ThirdPartyID;
            //MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Name"));
            //MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.Name.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ID"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.ID.ToString();
            return MyEventSubScheduleNode;
        }

        public bool UpdateInnerXml(ref XmlNodeList MyLogList, string NodeName, string IdentifierData, XmlElement UpdatedData)
        {
            for (int i = 0; i < MyLogList.Count; i++)
            {
                //XmlNode XmlTempHolder = MyLogList[i];
                //string TempHolder = XmlTempHolder.SelectSingleNode("ID").InnerText;
                if (MyLogList[i].SelectSingleNode(NodeName).InnerText == IdentifierData)
                {
                    MyLogList[i].SelectSingleNode(NodeName).InnerXml = UpdatedData.InnerXml;
                    return true;
                }
            }
            return false;
        }

        public bool UpdateXMLInnerText(ref XmlNodeList MyLogList, string NodeName, string IdentifierData, string UpdatedData)
        {
            foreach (XmlNode MyNode in MyLogList)
            {
                if (MyNode.SelectSingleNode("/" + NodeName).InnerText == IdentifierData)
                {
                    MyNode.SelectSingleNode("/" + NodeName).InnerText = UpdatedData;
                    
                    return true;
                }
            }
            return false;
        }

        static TimeLine ScheduleTimeline = new TimeLine();

        //Properties

        public int LastScheduleIDNumber
        {
            get
            {
                return Convert.ToInt32(LastIDNumber);
            }
        }
    }
    public class SubCalendarEvent : CalendarEvent
    {
        EventID SubEventID;
        BusyTimeLine BusyFrame;
        TimeSpan AvailablePreceedingFreeSpace;

        #region Classs Constructor
        public SubCalendarEvent()
        { }
        public SubCalendarEvent(TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID)
        {
//string eventName, TimeSpan EventDuration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = Event_Duration;
            PrepTime = EventPrepTime;
            SubEventID = new EventID(new string[] { myParentID, EventIDGenerator.generate().ToString()});
            EventSequence = new EventTimeLine(SubEventID.ToString(),StartDateTime, EndDateTime);
        }

        public SubCalendarEvent(TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid)
        {
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = Event_Duration;
            PrepTime = EventPrepTime;
            SubEventID = new EventID(new string[] { myParentID, EventIDGenerator.generate().ToString() });
            EventSequence = new EventTimeLine(SubEventID.ToString(), StartDateTime, EndDateTime);
            RigidSchedule = Rigid;
        }

        public SubCalendarEvent(string MySubEventID, BusyTimeLine MyBusylot, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID)
        {
            //string eventName, TimeSpan EventDuration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = MyBusylot.End-MyBusylot.Start;
            BusyFrame = MyBusylot;
            PrepTime = EventPrepTime;
            SubEventID = new EventID(MySubEventID.Split('_'));
            EventSequence = new EventTimeLine(SubEventID.ToString(), StartDateTime, EndDateTime);
        }

        public SubCalendarEvent(string MySubEventID, DateTime EventStart, DateTime EventDeadline,BusyTimeLine SubEventBusy)
        {
            SubEventID = new EventID(MySubEventID.Split('_'));
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = SubEventBusy.TimelineSpan;
            BusyFrame = SubEventBusy;
        }

        public SubCalendarEvent(string MySubEventID, BusyTimeLine MyBusylot, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid)
        {
            //string eventName, TimeSpan EventDuration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = MyBusylot.End - MyBusylot.Start;
            BusyFrame = MyBusylot;
            PrepTime = EventPrepTime;
            SubEventID = new EventID(MySubEventID.Split('_'));
            EventSequence = new EventTimeLine(SubEventID.ToString(), StartDateTime, EndDateTime);
            RigidSchedule = Rigid;
        }

        public SubCalendarEvent(string MySubEventID, DateTime EventStart, DateTime EventDeadline, BusyTimeLine SubEventBusy, bool Rigid)
        {
            SubEventID = new EventID(MySubEventID.Split('_'));
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = SubEventBusy.TimelineSpan;
            BusyFrame = SubEventBusy;
            RigidSchedule = Rigid;
        }
        #endregion


        #region Class functions
        public static int CompareByEndDate(SubCalendarEvent SubCalendarEvent1, SubCalendarEvent SubCalendarEvent2)
        {
            return SubCalendarEvent1.End.CompareTo(SubCalendarEvent2.End);
        }

        public static int CompareByStartDate(SubCalendarEvent SubCalendarEvent1, SubCalendarEvent SubCalendarEvent2)
        {
            return SubCalendarEvent1.Start.CompareTo(SubCalendarEvent2.Start);
        }

        public override void ReassignTime(DateTime StartTime, DateTime EndTime)
        {
            EndDateTime=(EndTime);
            StartDateTime = StartTime;
            BusyFrame = new BusyTimeLine(SubEventID.ToString(), StartTime, EndTime);
        }

        public SubCalendarEvent createCopy()
        {
            SubCalendarEvent MySubCalendarEventCopy = new SubCalendarEvent();
            MySubCalendarEventCopy.SubEventID = SubEventID;
            MySubCalendarEventCopy.BusyFrame = BusyFrame;
            MySubCalendarEventCopy.StartDateTime = StartDateTime;
            MySubCalendarEventCopy.EndDateTime = EndDateTime;
            MySubCalendarEventCopy.EventDuration = EventDuration;
            MySubCalendarEventCopy.RigidSchedule= RigidSchedule;
            MySubCalendarEventCopy.SchedulStatus = SchedulStatus;
            MySubCalendarEventCopy.otherPartyID = otherPartyID;
            MySubCalendarEventCopy.EventPreDeadline = EventPreDeadline;
            MySubCalendarEventCopy.EventRepetition = EventRepetition;
            return MySubCalendarEventCopy;
        }
        #endregion
        
        #region Class Properties

        public override string ThirdPartyID
        {
            get 
            {
                return otherPartyID;
            }
            set 
            {
                otherPartyID = value;
            }
        }

        public BusyTimeLine ActiveSlot
        {
            set
            {
                BusyFrame = value;
            }
            get
            {
                return BusyFrame;
            }
        }

        public TimeSpan ActiveDuration
        {
            get 
            {
                return EventDuration;
            }
        }

        public string ID
        {
            get 
            {
                return SubEventID.ToString();
            }
        }

        override public bool Rigid
        {
            get
            {
                return RigidSchedule;
            }
        }

        public override TimeLine EventTimeLine
        {
            get
            {
                return BusyFrame;
            }
        }
        #endregion

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
