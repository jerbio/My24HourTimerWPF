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
using WinForms = System.Windows.Forms;
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
            string LocationString  = textBox8.Text.Trim();
            if (LocationString != "")
            {
                eventName += "," + LocationString;
            }

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
                MessageBox.Show(ErrorCheck.Message);
                return;

            }

            Location var0 = new Location(textBox8.Text);

            //CalendarEvent ScheduleUpdated = CreateSchedule(eventName, eventStartTime, eventStartDate, eventEndTime, eventEndDate, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionflag, DefaultPreDeadlineFlag, RigidScheduleFlag, eventPrepTime, DefaultPreDeadlineFlag);
            CalendarEvent ScheduleUpdated = new CalendarEvent(eventName, eventStartTime, eventStartDate, eventEndTime, eventEndDate, eventSplit, PreDeadlineTime, EventDuration, MyRepetition, DefaultPreDeadlineFlag, RigidFlag, eventPrepTime, DefaultPreDeadlineFlag, var0);
            ScheduleUpdated.Repeat.PopulateRepetitionParameters(ScheduleUpdated);
            textBlock9.Text = "...Loading";
            if(MySchedule.AddToSchedule(ScheduleUpdated))
            {textBlock9.Text = "Schedule Updated with " + ScheduleUpdated.Name;}
            else
            {
                textBlock9.Text = "Failed to update Schedule" + ScheduleUpdated.Name;
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

        private void textBox6_TextChanged(object sender, TextChangedEventArgs e)
        {

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
        private static int CalendarEvenntLimitIndex = 2;
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
    }

    
    public class Repetition
    {
        
        string RepetitionFrequency;
        TimeLine RepetitionRange;
        bool EnableRepeat;
        CalendarEvent InitializingEvent;
        CalendarEvent[] RepeatingEvents;
        Dictionary<string, CalendarEvent> DictionaryOfIDAndCalendarEvents;
        Location RepeatLocation;

        public Repetition()
        {
            RepetitionFrequency = "";
            RepetitionRange = new TimeLine();
            EnableRepeat = false;
            RepeatLocation = new Location();
            DictionaryOfIDAndCalendarEvents = new System.Collections.Generic.Dictionary<string, CalendarEvent>();
        }
        public Repetition(bool EnableFlag ,TimeLine RepetitionRange_Entry, string Frequency)
        {
            RepetitionRange = RepetitionRange_Entry;
            RepetitionFrequency = Frequency.ToUpper();
            EnableRepeat = EnableFlag;
            RepeatLocation = new Location();
            InitializingEvent = new CalendarEvent();
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

            /*if (MyParentEvent.Rigid)//Rigid means if Start DateTime Will be Start Time and Date of CalendarEvent  and End Time and Date will be end DateTime of Calendar Event
            {
                EachRepeatCalendarStart = MyParentEvent.Start;
                EachRepeatCalendarEnd = MyParentEvent.End;
            }*/
            EventID MyEventCalendarID = new EventID(MyParentEvent.ID + "_" + EventIDGenerator.generate().ToString());
            CalendarEvent MyRepeatCalendarEvent = new CalendarEvent(MyEventCalendarID, MyParentEvent.Name, MyParentEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.Rigid, new Repetition(), MyParentEvent.NumberOfSplit, MyParentEvent.myLocation);
            
            //new CalendarEvent(MyParentEvent.Name, MyParentEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyParentEvent.Preparation, MyParentEvent.PreDeadline, MyParentEvent.Rigid, new Repetition(), MyParentEvent.NumberOfSplit);//first repeating calendar event
            
            //MyRepeatCalendarEvent = new CalendarEvent(new EventID(MyParentEvent.ID), MyRepeatCalendarEvent.Name, MyRepeatCalendarEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyRepeatCalendarEvent.Preparation, MyRepeatCalendarEvent.PreDeadline, MyRepeatCalendarEvent.Rigid, MyRepeatCalendarEvent.Repeat, MyRepeatCalendarEvent.NumberOfSplit);
            List<CalendarEvent> MyArrayOfRepeatingCalendarEvents = new List<CalendarEvent>();

            for (; MyRepeatCalendarEvent.Start < MyParentEvent.Repeat.Range.End; )
            {
                MyArrayOfRepeatingCalendarEvents.Add(MyRepeatCalendarEvent);
                DictionaryOfIDAndCalendarEvents.Add(MyRepeatCalendarEvent.ID, MyRepeatCalendarEvent);
                EachRepeatCalendarStart = IncreaseByFrequency(EachRepeatCalendarStart, Frequency); ;
                EachRepeatCalendarEnd = IncreaseByFrequency(EachRepeatCalendarEnd, Frequency);
                MyEventCalendarID = new EventID(MyParentEvent.ID + "_" + EventIDGenerator.generate().ToString());
                MyRepeatCalendarEvent = new CalendarEvent(MyEventCalendarID, MyRepeatCalendarEvent.Name, MyRepeatCalendarEvent.ActiveDuration, EachRepeatCalendarStart, EachRepeatCalendarEnd, MyRepeatCalendarEvent.Preparation, MyRepeatCalendarEvent.PreDeadline, MyRepeatCalendarEvent.Rigid, MyRepeatCalendarEvent.Repeat, MyRepeatCalendarEvent.NumberOfSplit, MyParentEvent.myLocation);
                
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

        void EmptyCalendarXMLFile()
        {

            File.WriteAllText("MyEventLog.xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?><ScheduleLog><LastIDCounter>0</LastIDCounter><EventSchedules></EventSchedules></ScheduleLog>");
        }

        public void EmptyAllCalendarEvent()//MyTemp Function for deleting all calendar events
        {
            EmptyCalendarXMLFile();
            int i = 0;
            CalendarEvent[] ArrayOfValues = AllEventDictionary.Values.ToArray();
            string[] ArrayOfKeys = AllEventDictionary.Keys.ToArray();


            for (; i < ArrayOfValues.Length;i++ )//this loops through the ArrayOfValues and ArrayOfIndex. Since each index loop corresponds to the same dictionary entry.
            {
                RemoveFromOutlook(ArrayOfValues[i]); //this removes the value from outlook
               // AllEventDictionary.Remove(ArrayOfKeys[i]);//this removes the entry from The dictionary
            }
            
            //File.WriteAllText("MyEventLog.xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?><ScheduleLog><LastIDCounter>0</LastIDCounter><EventSchedules></EventSchedules></ScheduleLog>");//This Explicitly Empties the MyEventLog File
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
                        if (!MyEvent.RepetitionStatus)
                        { MyTotalSubEventBusySlots.Add(MySubCalendarEvent.ActiveSlot); }
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
        string LogInfo = "";
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
            string LocationData;


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

            Location var3 = getLocation(EventScheduleNode);



            CalendarEvent RetrievedEvent = new CalendarEvent(ID, Name, StartTime, StartTimeConverted, EndTime, EndTimeConverted, Split, PreDeadline, CalendarEventDuration, Recurrence, false, Convert.ToBoolean(Rigid), PrepTime, false, var3);
            RetrievedEvent = new CalendarEvent(RetrievedEvent, ReadSubSchedulesFromXMLNode(EventScheduleNode.SelectSingleNode("EventSubSchedules"), RetrievedEvent));
            return RetrievedEvent;
        }

        Location getLocation(XmlNode Arg1)
        {
            XmlNode var1=Arg1.SelectSingleNode("Location");
            if (var1 == null)
            {
                return new Location();
            }
            else
            {
                string XCoordinate_Str = var1.SelectSingleNode("XCoordinate").InnerText;
                string YCoordinate_Str = var1.SelectSingleNode("YCoordinate").InnerText;
                string Descripion = var1.SelectSingleNode("Description").InnerText;
                string Address = var1.SelectSingleNode("Address").InnerText;

                if (string.IsNullOrEmpty(XCoordinate_Str) || string.IsNullOrEmpty(YCoordinate_Str))
                {
                    return new Location(Address);
                }
                else 
                {
                    double xCoOrdinate = double.MaxValue;
                    double yCoOrdinate = double.MaxValue;

                    if (!(double.TryParse(XCoordinate_Str, out xCoOrdinate)))
                    {
                        xCoOrdinate = double.MaxValue;
                    }
                    
                    if(!(double.TryParse(YCoordinate_Str, out yCoOrdinate)))
                    {
                        yCoOrdinate = double.MaxValue;
                    }
                    
                    return new Location(xCoOrdinate, yCoOrdinate);
                }
            }


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
                Location var1 = getLocation(MyXmlNode.ChildNodes[i]);

                MyArrayOfNodes[i] = new SubCalendarEvent(ID, BusySlot, Start, End, PrepTime, MyParent.ID, MyParent.Rigid, var1, MyParent.EventTimeLine);
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
            if (NewEvent.ID == "" || NewEvent==null)//checks if event was assigned and ID ehich means it was successfully able to find a spot
            {
                return false;
            }
            EmptyAllCalendarEvent();
            try
            {
                AllEventDictionary.Add(NewEvent.ID, NewEvent);
            }
            catch
            {
                AllEventDictionary[NewEvent.ID]= NewEvent;
            }

            
            foreach(CalendarEvent MyCalEvent in AllEventDictionary.Values)
            {
                //WriteToOutlook(MyCalEvent);
                WriteToLog(MyCalEvent);
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
                SubCalendarEvent MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start, MyCalendarEvent.Repeat.Range.End, MyCalendarEvent.Preparation, MyCalendarEvent.ID, MyCalendarEvent.myLocation, MyCalendarEvent.EventTimeLine);
                     //new SubCalendarEvent(MyCalendarEvent.End, MyCalendarEvent.Repeat.Range.Start, MyCalendarEvent.Repeat.Range.End, MyCalendarEvent.Preparation, MyCalendarEvent.ID);
                
                for (;MySubEvent.Start<MyCalendarEvent.Repeat.Range.End;)
                {
                    MyArrayOfSubEvents.Add(MySubEvent);
                    switch (MyCalendarEvent.Repeat.Frequency)
                    {
                        case "DAILY":
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start.AddDays(1), MyCalendarEvent.Repeat.Range.End.AddDays(1), MyCalendarEvent.Preparation, MyCalendarEvent.ID, MyCalendarEvent.myLocation, MyCalendarEvent.EventTimeLine);
                                break;
                            }
                        case "WEEKLY":
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start.AddDays(7), MyCalendarEvent.Repeat.Range.End.AddDays(7), MyCalendarEvent.Preparation, MyCalendarEvent.ID, MyCalendarEvent.myLocation, MyCalendarEvent.EventTimeLine);
                                break;
                            }
                        case "BI-WEEKLY":
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start.AddDays(14), MyCalendarEvent.Repeat.Range.End.AddDays(14), MyCalendarEvent.Preparation, MyCalendarEvent.ID, MyCalendarEvent.myLocation, MyCalendarEvent.EventTimeLine);
                                break;
                            }
                        case "MONTHLY":
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start.AddMonths(1), MyCalendarEvent.Repeat.Range.End.AddMonths(1), MyCalendarEvent.Preparation, MyCalendarEvent.ID, MyCalendarEvent.myLocation, MyCalendarEvent.EventTimeLine);
                                break;
                            }
                        case "YEARLY":
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start.AddYears(1), MyCalendarEvent.Repeat.Range.End.AddYears(1), MyCalendarEvent.Preparation, MyCalendarEvent.ID, MyCalendarEvent.myLocation, MyCalendarEvent.EventTimeLine);
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
                    else 
                    {
                        if (Outlier.IsTimeLineWithin(myTimeLine))
                        {
                            PertinentTimeLine.Add(Outlier);
                        }
                    }
                }
            }

            return PertinentTimeLine;



            //return new List<TimeLine>();
        }

        

        bool []CheckIfPotentialSubEventClashesWithAnyOtherSubEvents(CalendarEvent MyPotentialCalendarEvent, TimeLine MyTimeLineOfEvent)
        {
            BusyTimeLine []ArrayOfBusySlots=MyTimeLineOfEvent.OccupiedSlots;
            bool[] StatusOfCollision=new bool[]{false,false};
            foreach (BusyTimeLine MyBusySlot in ArrayOfBusySlots)
            {
                if (MyBusySlot.doesTimeLineInterfere(MyPotentialCalendarEvent.EventTimeLine))
                {
                    StatusOfCollision[0]=true;
                    if (AllEventDictionary[(new EventID( MyBusySlot.TimeLineID)).getLevelID(0)].Rigid)
                    {
                        StatusOfCollision[1] = true;
                    }
                    
                }
            }

            return StatusOfCollision;
        }

        CalendarEvent ResolveWithDataOutsideCalendarEventSchedule(CalendarEvent MyEvent, List<CalendarEvent> ListOfOtherCalendarEvents)
        {
            
            return new CalendarEvent();
        }

        List<TimeLine> CheckTimeLineListForEncompassingTimeLine(List<TimeLine> ListOfTimeLine, TimeLine MyTimeLine)
        {
            /*
             * Function forces takes an array of timeLine that possibly Encompasses another timeline. If a timeline in the List the second arguement it is restricted to timeLimits of the TimeLine or else it is just added.
             */
            List<TimeLine> UpdatedTimeLine = new System.Collections.Generic.List<TimeLine>();
            foreach (TimeLine EncompassingTimeLine in ListOfTimeLine)
            {
                if (EncompassingTimeLine.IsTimeLineWithin(MyTimeLine))
                {
                    UpdatedTimeLine.Add(new TimeLine(MyTimeLine.Start, MyTimeLine.End));
                }
                else
                {
                    UpdatedTimeLine.Add(EncompassingTimeLine);
                }
            }
            return UpdatedTimeLine;
        }

        public CalendarEvent EvaluateTotalTimeLineAndAssignValidTimeSpots(CalendarEvent MyEvent)
        {
            int i = 0;
            if (MyEvent.RepetitionStatus)
            {
                for (i = 0; i < MyEvent.Repeat.RecurringCalendarEvents.Length; i++)
                {
                    if (i == 3)
                    {
                        //MessageBox.Show("Scroll through Jerome");
                        ;
                    }
                    MyEvent.Repeat.RecurringCalendarEvents[i] = EvaluateTotalTimeLineAndAssignValidTimeSpots(MyEvent.Repeat.RecurringCalendarEvents[i]);
                }
                return MyEvent;
            }
            
            BusyTimeLine [] AllOccupiedSlot= CompleteSchedule.OccupiedSlots;
            TimeSpan TotalActiveDuration=new TimeSpan();
            TimeLine[] TimeLineArrayWithSubEventsAssigned = new TimeLine[MyEvent.AllEvents.Length];
            SubCalendarEvent TempSubEvent=new SubCalendarEvent();
            BusyTimeLine MyTempBusyTimerLine=new BusyTimeLine();
            List<TimeLine> FreeSpotsAvailableWithinValidTimeline= getAllFreeSpots(new TimeLine(MyEvent.Start,MyEvent.End)).ToList();

            FreeSpotsAvailableWithinValidTimeline = CheckTimeLineListForEncompassingTimeLine(FreeSpotsAvailableWithinValidTimeline, MyEvent.EventTimeLine);
            
            FreeSpotsAvailableWithinValidTimeline = getOnlyPertinentTimeFrame(FreeSpotsAvailableWithinValidTimeline.ToArray(), new TimeLine(MyEvent.Start, MyEvent.End));

            i = 0;
            TimeSpan TotalFreeTimeAvailable=new TimeSpan();
            if (MyEvent.Rigid)
            {
                bool []StatusOfClash=CheckIfPotentialSubEventClashesWithAnyOtherSubEvents(MyEvent, CompleteSchedule);

                if (StatusOfClash[0] && StatusOfClash[1])
                {
                    //MessageBox.Show("Are you sure?","Confirm", MessageBoxButtons.YesNo, MessageBoxImage.Question)

                    if (!(((WinForms::DialogResult)MessageBox.Show("Are you sure", "Confirm", MessageBoxButton.YesNo)) == WinForms::DialogResult.Yes))
                    {
                        return new CalendarEvent();
                    }
                }

                TempSubEvent = new SubCalendarEvent(MyEvent.ActiveDuration, MyEvent.Start, MyEvent.End, MyEvent.Preparation, MyEvent.ID, MyEvent.myLocation,MyEvent.EventTimeLine);
                MyTempBusyTimerLine = new BusyTimeLine(TempSubEvent.ID, TempSubEvent.Start, TempSubEvent.End);
                TempSubEvent = new SubCalendarEvent(TempSubEvent.ID, TempSubEvent.Start, TempSubEvent.End, MyTempBusyTimerLine, MyEvent.Rigid, MyEvent.myLocation, MyEvent.EventTimeLine);
                MyEvent.AllEvents[0] = TempSubEvent;

            }
            else 
            {
                for (i = 0; i < FreeSpotsAvailableWithinValidTimeline.Count; i++)
                {
                    TotalFreeTimeAvailable += FreeSpotsAvailableWithinValidTimeline[i].TimelineSpan;
                }
                
                if (TotalFreeTimeAvailable >= MyEvent.ActiveDuration)
                {
                    TimeLineArrayWithSubEventsAssigned = SplitFreeSpotsInToSubEventTimeSlots(FreeSpotsAvailableWithinValidTimeline.ToArray(), MyEvent.AllEvents.Length, MyEvent.ActiveDuration);
                    if (TimeLineArrayWithSubEventsAssigned == null)
                    {
                        BusyTimeLine[] CompleteScheduleOccupiedSlots = CompleteSchedule.OccupiedSlots;
                        KeyValuePair<CalendarEvent, TimeLine> TimeLineAndCalendarUpdated = ReArrangeTimeLineWithinWithinCalendaEventRange(MyEvent);
                        CalendarEvent MyCalendarEventUpdated = TimeLineAndCalendarUpdated.Key;
                        //CompleteSchedule.OccupiedSlots = TimeLineAndCalendarUpdated.Value.OccupiedSlots;//hack need to review architecture to avoid this assignment
                        if (MyCalendarEventUpdated != null)
                        {
                            string MyEventParentID = (new EventID(MyEvent.ID)).getLevelID(0);
                            foreach (BusyTimeLine MyBusyTimeLine in TimeLineAndCalendarUpdated.Value.OccupiedSlots)
                            {
                                string ParentID = (new EventID(MyBusyTimeLine.TimeLineID)).getLevelID(0);
                                if (ParentID != MyEventParentID)
                                {
                                    SubCalendarEvent[] MyArrayOfSubCalendarEvents;
                                    if (AllEventDictionary[ParentID].RepetitionStatus)
                                    {
                                        bool Verified = AllEventDictionary[ParentID].updateSubEvent(new EventID(MyBusyTimeLine.TimeLineID), new SubCalendarEvent(MyBusyTimeLine.TimeLineID, MyBusyTimeLine.Start, MyBusyTimeLine.End, MyBusyTimeLine, null));
                                    }
                                    else 
                                    {
                                        MyArrayOfSubCalendarEvents = AllEventDictionary[ParentID].AllEvents;
                                        for (i = 0; i < MyArrayOfSubCalendarEvents.Length; i++)
                                        {
                                            if (MyArrayOfSubCalendarEvents[i].ID == MyBusyTimeLine.TimeLineID)
                                            {
                                                MyArrayOfSubCalendarEvents[i] = new SubCalendarEvent(MyBusyTimeLine.TimeLineID, MyBusyTimeLine.Start, MyBusyTimeLine.End, MyBusyTimeLine, MyArrayOfSubCalendarEvents[i].Rigid, MyArrayOfSubCalendarEvents[i].myLocation, AllEventDictionary[ParentID].EventTimeLine);
                                            }
                                        }
                                    }
                                    
                                    
                                }
                                
                            }
                            /*if (MyCalendarEventUpdated.RepetitionStatus)
                            {
                                for (i = 0; i < MyEvent.Repeat.RecurringCalendarEvents.Length; i++)
                                {
                                    MyCalendarEventUpdated.Repeat.RecurringCalendarEvents[i] = EvaluateTotalTimeLineAndAssignValidTimeSpots(MyEvent.Repeat.RecurringCalendarEvents[i]);
                                }
                            }*/


                            return MyCalendarEventUpdated;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Sorry, the total free time available during activiy limits is less than your active duration!!!");
                    List<CalendarEvent> ListOfCalendarEventsWithLatterDeadlines = new System.Collections.Generic.List<CalendarEvent>();
                    return new CalendarEvent();
                }

                if (TimeLineArrayWithSubEventsAssigned.Length < MyEvent.AllEvents.Length)// This means the assigned time per subevent spots won't be sufficient for the subevents available to the calendar event
                {
                    return null;

                }

                i = 0;
                for (; i < MyEvent.AllEvents.Length; i++)
                {
                    TempSubEvent = new SubCalendarEvent(TimeLineArrayWithSubEventsAssigned[i].TimelineSpan, TimeLineArrayWithSubEventsAssigned[i].Start, TimeLineArrayWithSubEventsAssigned[i].End, MyEvent.Preparation, MyEvent.ID, MyEvent.Rigid, MyEvent.myLocation, MyEvent.EventTimeLine);
                    MyTempBusyTimerLine = new BusyTimeLine(TempSubEvent.ID, TimeLineArrayWithSubEventsAssigned[i].Start, TimeLineArrayWithSubEventsAssigned[i].End);
                    TempSubEvent = new SubCalendarEvent(TempSubEvent.ID, TimeLineArrayWithSubEventsAssigned[i].Start.Add(-MyEvent.Preparation), TimeLineArrayWithSubEventsAssigned[i].End, MyTempBusyTimerLine, MyEvent.Rigid, MyEvent.myLocation, MyEvent.EventTimeLine);
                    MyEvent.AllEvents[i] = TempSubEvent;
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
            TimeLine[] FreeSpotsAvailableWithinValidTimeline = getAllFreeSpots_NoCompleteSchedule(ReferenceTimeLine);
            FreeSpotsAvailableWithinValidTimeline = getOnlyPertinentTimeFrame(FreeSpotsAvailableWithinValidTimeline, ReferenceTimeLine).ToArray();

            int i = 0;
            TimeSpan TotalFreeTimeAvailable = new TimeSpan();
            if (MyEvent.Rigid)
            {
                TempSubEvent = new SubCalendarEvent(MyEvent.ActiveDuration, MyEvent.Start, MyEvent.End, MyEvent.Preparation, MyEvent.ID,MyEvent.myLocation, MyEvent.EventTimeLine);
                MyTempBusyTimerLine = new BusyTimeLine(TempSubEvent.ID, TempSubEvent.Start, TempSubEvent.End);
                TempSubEvent = new SubCalendarEvent(TempSubEvent.ID, TempSubEvent.Start, TempSubEvent.End, MyTempBusyTimerLine, MyEvent.myLocation, MyEvent.EventTimeLine);
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
                        KeyValuePair<CalendarEvent, TimeLine> TimeLineAndCalendarUpdated = ReArrangeTimeLineWithinWithinCalendaEventRange(MyEvent);
                        CalendarEvent MyCalendarEventUpdated = TimeLineAndCalendarUpdated.Key;
                        CompleteSchedule.OccupiedSlots = TimeLineAndCalendarUpdated.Value.OccupiedSlots;//hack need to review architecture to avoid this assignment
                        if (MyCalendarEventUpdated != null)
                        {
                            foreach (BusyTimeLine MyBusyTimeLine in CompleteSchedule.OccupiedSlots)
                            {
                                string ParentID = (new EventID(MyBusyTimeLine.TimeLineID)).getLevelID(0);
                                SubCalendarEvent[] MyArrayOfSubCalendarEvents = AllEventDictionary[ParentID].AllEvents;
                                for (i = 0; i < MyArrayOfSubCalendarEvents.Length;i++ )
                                {
                                    if (MyArrayOfSubCalendarEvents[i].ID == MyBusyTimeLine.TimeLineID)
                                    {
                                        MyArrayOfSubCalendarEvents[i] = new SubCalendarEvent(MyBusyTimeLine.TimeLineID, MyBusyTimeLine.Start, MyBusyTimeLine.End, MyBusyTimeLine, MyArrayOfSubCalendarEvents[i].Rigid, MyArrayOfSubCalendarEvents[i].myLocation, MyArrayOfSubCalendarEvents[i].getCalendarEventRange);
                                    }
                                }
                            }

                            
                            
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
                    TempSubEvent = new SubCalendarEvent(TimeLineArrayWithSubEventsAssigned[i].TimelineSpan, TimeLineArrayWithSubEventsAssigned[i].Start.Add(-MyEvent.Preparation), TimeLineArrayWithSubEventsAssigned[i].End, MyEvent.Preparation, MyEvent.ID, MyEvent.AllEvents[i].myLocation, MyEvent.EventTimeLine);
                    MyTempBusyTimerLine = new BusyTimeLine(TempSubEvent.ID, TimeLineArrayWithSubEventsAssigned[i].Start, TimeLineArrayWithSubEventsAssigned[i].End);
                    TempSubEvent = new SubCalendarEvent(TempSubEvent.ID, TimeLineArrayWithSubEventsAssigned[i].Start.Add(-MyEvent.Preparation), TimeLineArrayWithSubEventsAssigned[i].End, MyTempBusyTimerLine, MyEvent.AllEvents[i].myLocation, MyEvent.EventTimeLine);
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
            int j = 0;
            for (; i < ListOfInterferringElements.Count; i++)
            {
                EventID MyEventID = new EventID(ListOfInterferringElements[i].ID);
                string ParentID=MyEventID.getLevelID(0);//This gets the parentID of the SubCalendarEventID
                
                //try//Try bock attempts to create new dictionary entry for Calendar event. Else it simply adds an element to the list created by the error
                {
                    if (AllEventDictionary[ParentID].RepetitionStatus)
                    {
                        CalendarEvent repeatCalEvent=AllEventDictionary[ParentID].getRepeatedCalendarEvent(MyEventID.getStringIDAtLevel(1));
                        
                        if (DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.ContainsKey(repeatCalEvent))
                        { 
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[repeatCalEvent].Add(ListOfInterferringElements[i]);
                            j++;
                        }
                        else 
                        {
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Add(repeatCalEvent,new List<SubCalendarEvent>());
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[repeatCalEvent].Add(ListOfInterferringElements[i]);
                            j++;
                        }

                        
                    }
                    else 
                    {
                        CalendarEvent nonRepeatCalEvent=AllEventDictionary[ParentID];

                        if (DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.ContainsKey(nonRepeatCalEvent))
                        {
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[nonRepeatCalEvent].Add( ListOfInterferringElements[i]);
                            j++;
                        }
                        else 
                        {
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Add(nonRepeatCalEvent, new List<SubCalendarEvent>());
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[nonRepeatCalEvent].Add(ListOfInterferringElements[i]);
                            j++;
                        }
                        
                    }
                }
                /*catch (Exception e)
                {
                    if (AllEventDictionary[ParentID].RepetitionStatus)
                    { 
                        
                    }
                    else
                    {
                        DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[AllEventDictionary[ParentID]].Add(ListOfInterferringElements[i]);
                    }
                }*/
                
                
            }

            return DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents;
        }

        KeyValuePair<CalendarEvent,TimeLine> ReArrangeTimeLineWithinWithinCalendaEventRange(CalendarEvent MyCalendarEvent)// this looks at the timeline of the calendar event and then tries to rearrange all subevents within the range to suit final output. Such that there will be sufficient time space for each subevent
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


            List<SubCalendarEvent> RigidSubCalendarEvents = new List<SubCalendarEvent>(0);
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

            ArrayOfInterferringSubEvents = Utility.NotInList(ArrayOfInterferringSubEvents.ToList(), RigidSubCalendarEvents).ToArray();

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


            if (MyCalendarEvent.RepetitionStatus == false)//generates random subevents for the calendar event
            {
                SubCalendarEvent[] TempSubCalendarEventsForMyCalendarEvent = new SubCalendarEvent[MyCalendarEvent.NumberOfSplit];

                //int i;
                for (i = 0; i < TempSubCalendarEventsForMyCalendarEvent.Length; i++)//populates the subevents for the calendar event
                {
                    TimeSpan MyActiveTimeSpanPerSplit = new TimeSpan((long)((MyCalendarEvent.ActiveDuration.TotalSeconds / MyCalendarEvent.NumberOfSplit) * 10000000));
                    TempSubCalendarEventsForMyCalendarEvent[i] = new SubCalendarEvent(MyActiveTimeSpanPerSplit, MyCalendarEvent.Start, (MyCalendarEvent.Start + MyActiveTimeSpanPerSplit), new TimeSpan(), MyCalendarEvent.ID, MyCalendarEvent.myLocation, MyCalendarEvent.EventTimeLine);
                    TempSubCalendarEventsForMyCalendarEvent[i] = new SubCalendarEvent(TempSubCalendarEventsForMyCalendarEvent[i].ID, TempSubCalendarEventsForMyCalendarEvent[i].Start, TempSubCalendarEventsForMyCalendarEvent[i].End, new BusyTimeLine(TempSubCalendarEventsForMyCalendarEvent[i].ID, TempSubCalendarEventsForMyCalendarEvent[i].Start, TempSubCalendarEventsForMyCalendarEvent[i].End), MyCalendarEvent.myLocation, MyCalendarEvent.EventTimeLine);
                    MyCalendarEvent.AllEvents[i] = TempSubCalendarEventsForMyCalendarEvent[i];
                }
            }

            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Add(MyCalendarEvent, MyCalendarEvent.AllEvents.ToList());


            List<CalendarEvent> SortedInterFerringCalendarEvents_Deadline = DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Keys.ToList();
            SortedInterFerringCalendarEvents_Deadline = SortedInterFerringCalendarEvents_Deadline.OrderBy(obj => obj.End).ToList();
                
                //SortCalendarEvent(DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Keys.ToList(),false);

            //SortedInterFerringCalendarEvents.Add(MyCalendarEvent);
            
            //SubEventsTimeCategories[]




            
            

            TimeLine ReferenceTimeLine = new TimeLine(MyCalendarEvent.Start,MyCalendarEvent.End);


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



            //DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Add(MyCalendarEvent, MyCalendarEvent.AllEvents.ToList());
            //RETURN HERE JEROME



            List<List<List<SubCalendarEvent>>> SnugListOfPossibleSubCalendarEventsClumps = BuildAllPossibleSnugLists(SortedInterFerringCalendarEvents_Deadline, MyCalendarEvent, DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents, CalendarEventsTimeCategories, ReferenceTimeLine);
            //Remember Jerome, I need to implement a functionality that permutates through the various options of pin to start option. So take for example two different event timeline that are pertinent to a free spot however one has a dead line preceeding the other, there will be a pin to start for two scenarios, one for each calendar event in which either of them gets pinned first.

            return EvaluateEachSnugPossibiliyOfSnugPossibility(SnugListOfPossibleSubCalendarEventsClumps, ReferenceTimeLine,MyCalendarEvent);
            ;//this will not be the final output. I'll need some class that stores the current output of both rearrange busytimelines and deleted timelines
        }

        KeyValuePair<CalendarEvent, TimeLine> EvaluateEachSnugPossibiliyOfSnugPossibility(List<List<List<SubCalendarEvent>>> SnugPossibilityPermutation, TimeLine ReferenceTimeLine, CalendarEvent ReferenceCalendarEvent)
        {
            TimeLine CopyOfReferenceTimeLine;
            List<TimeLine> SnugPossibilityTimeLine = new System.Collections.Generic.List<TimeLine>();
            Dictionary<BusyTimeLine, SubCalendarEvent> MyBusyTimeLineToSubCalendarEventDict = new System.Collections.Generic.Dictionary<BusyTimeLine, SubCalendarEvent>();

            foreach (List<List<SubCalendarEvent>> SnugPermutation in SnugPossibilityPermutation)//goes each permutation of snug possibility generated
            {
                
                
                CopyOfReferenceTimeLine = ReferenceTimeLine.CreateCopy();
                //SnugPossibilityTimeLine.Add(CopyOfReferenceTimeLine);
                List<TimeLine> ListOfFreeSpots=getOnlyPertinentTimeFrame(getAllFreeSpots_NoCompleteSchedule(CopyOfReferenceTimeLine), CopyOfReferenceTimeLine);
                List<SubCalendarEvent> ReassignedSubEvents = new System.Collections.Generic.List<SubCalendarEvent>();
                for (int i=0; i<ListOfFreeSpots.Count;i++)
                {
                    DateTime RelativeStartTime = ListOfFreeSpots[i].Start;
                    
                    foreach (SubCalendarEvent MySubCalendarEvent in SnugPermutation[i])
                    {
                        SubCalendarEvent CopyOfMySubCalendarEvent = MySubCalendarEvent.createCopy();
                        TimeSpan MySubCalendarDuration = (CopyOfMySubCalendarEvent.End - CopyOfMySubCalendarEvent.Start);
                        DateTime RelativeEndtime = RelativeStartTime + MySubCalendarDuration;
                        CopyOfMySubCalendarEvent.ReassignTime(RelativeStartTime, RelativeEndtime);
                        CopyOfMySubCalendarEvent.ActiveSlot = new BusyTimeLine(CopyOfMySubCalendarEvent.ID, RelativeStartTime, RelativeEndtime);//Note this is a hack to resolve the reassignment of time since we dont know currently know the distiction between BusyTimeLine and SubCalendarEvent(TimeLine)
                        TimeLine MyTimeLine=CopyOfMySubCalendarEvent.EventTimeLine;
                        CopyOfReferenceTimeLine.MergeTimeLines(MyTimeLine);
                        RelativeStartTime = CopyOfMySubCalendarEvent.End;
                        MyBusyTimeLineToSubCalendarEventDict.Add(CopyOfMySubCalendarEvent.ActiveSlot, CopyOfMySubCalendarEvent);
                    }
                }
                SnugPossibilityTimeLine.Add(CopyOfReferenceTimeLine);
            }
            Dictionary<CalendarEvent, TimeLine> CalendarEvent_EvaluationIndexDict = new System.Collections.Generic.Dictionary<CalendarEvent, TimeLine>();
            Dictionary<string, double> DictionaryGraph = new System.Collections.Generic.Dictionary<string, double>();
            

            foreach (TimeLine MyTimeLine in SnugPossibilityTimeLine)
            {
                CalendarEvent MyEventCopy=ReferenceCalendarEvent.createCopy();
                
                foreach (BusyTimeLine MyBusyPeriod in MyTimeLine.OccupiedSlots)
                {
                    EventID MyEventID = new EventID(MyBusyPeriod.TimeLineID);
                    string ParentCalendarEventID = MyEventID.getLevelID(0);
                    if (MyEventCopy.ID == ParentCalendarEventID)
                    {

                        SubCalendarEvent MySubCalendarEvent=MyBusyTimeLineToSubCalendarEventDict[MyBusyPeriod];
                        for (int i = 0; i < MyEventCopy.AllEvents.Length; i++)
                        {
                            if (MyEventCopy.AllEvents[i].ID == MySubCalendarEvent.ID)
                            {
                                MyEventCopy.AllEvents[i] = MySubCalendarEvent;
                                break;
                            }
                        }
                        
                    }
                }

                //MyEventCopy=EvaluateTotalTimeLineAndAssignValidTimeSpotsWithReferenceTimeLine(MyEventCopy, MyTimeLine);

                CalendarEvent_EvaluationIndexDict.Add(MyEventCopy, MyTimeLine);

            }

            double HighestValue=0;

            KeyValuePair<CalendarEvent, TimeLine> FinalSuggestion = new System.Collections.Generic.KeyValuePair<CalendarEvent,TimeLine>();
            TimeLine TimeLineUpdated = null;
            Dictionary<string, double> LocationVector = new System.Collections.Generic.Dictionary<string,double>();
            LocationVector.Add("sameElement", 10000000000);


            foreach (KeyValuePair<CalendarEvent, TimeLine> MyCalendarEvent_TimeLine in CalendarEvent_EvaluationIndexDict)
            {
                int RandomIndex = EvaluateRandomNetIndex(MyCalendarEvent_TimeLine.Value);
                RandomIndex = 0;
                LocationVector=BuildDictionaryDistanceEdge(MyCalendarEvent_TimeLine.Value, MyCalendarEvent_TimeLine.Key, LocationVector);
                double ClumpIndex = EvaluateClumpingIndex(MyCalendarEvent_TimeLine.Value, LocationVector);
                ClumpIndex = 1 / ClumpIndex;
                double EvaluationSum = ClumpIndex + RandomIndex;
                if (EvaluationSum < 0)
                {
                    EvaluationSum *= -1;
                }

                if ( EvaluationSum > HighestValue)
                {
                    HighestValue = EvaluationSum;
                    FinalSuggestion = MyCalendarEvent_TimeLine;
                }
            }

            if (FinalSuggestion.Equals(new KeyValuePair<CalendarEvent,TimeLine>()))
            {
                MessageBox.Show("Oh oh J, you'll need to look outside this range...Think of moving other events out of white box space");
            }

            return FinalSuggestion;
        }

        int EvaluateRandomNetIndex(TimeLine ReferenFilledReferenceTimeLine)
        {
            var r = new Random();
            return r.Next();
        }




        string BuildStringIndexForMatch(BusyTimeLine PrecedingTimeLineEvent,BusyTimeLine NextTimeLineEvent)
        {
            EventID MyEventID = new EventID(PrecedingTimeLineEvent.TimeLineID);
            int PrecedingCalendarEventID = Convert.ToInt16(MyEventID.getLevelID(0));
            int NextCalendarEventID = Convert.ToInt16(new EventID(NextTimeLineEvent.TimeLineID).getLevelID(0));

            if (PrecedingCalendarEventID == NextCalendarEventID)
            {
                return "sameElement";
            }
            else 
            {
                if (PrecedingCalendarEventID < NextCalendarEventID)
                {
                    return PrecedingCalendarEventID + "_" + NextCalendarEventID;
                }
                return NextCalendarEventID + "_" + PrecedingCalendarEventID;
            }
        }

        double EvaluateClumpingIndex(TimeLine ReferenFilledReferenceTimeLine, Dictionary<string, double> CurrentDictionary)
        {
            BusyTimeLine[] ListOfBusySlots = ReferenFilledReferenceTimeLine.OccupiedSlots;
            int i,j=0;
            double CurrentSumOfLocationData=0;
            for (i = 0; i < (ListOfBusySlots.Length - 1); i++)
            {
                j = i + 1;
                string generatedIndexMatch = BuildStringIndexForMatch(ListOfBusySlots[i], ListOfBusySlots[j]);
                
                CurrentSumOfLocationData += CurrentDictionary[generatedIndexMatch];
            }

            return CurrentSumOfLocationData;
        }

        Dictionary<string, double> BuildDictionaryDistanceEdge(TimeLine ReferenceTimeline,CalendarEvent ReferenceCalendarEvent,Dictionary<string, double> CurrentDictionary)
        { 
            BusyTimeLine []ListOfBusySlots=ReferenceTimeline.OccupiedSlots;
            int i=0;
            //Dictionary<string, double> CurrentDictionaryFrom
            int j=0;
            for (i=0;i<(ListOfBusySlots.Length-1);i++)
            {
                j=i+1;
                string generatedIndexMatch=BuildStringIndexForMatch(ListOfBusySlots[i],ListOfBusySlots[j]);
                CalendarEvent MyPrecedingCalendarEvent;
                CalendarEvent MyNextCalendarEvent;
                try
                {
                    MyPrecedingCalendarEvent = AllEventDictionary[generatedIndexMatch.Split('_')[0]];   
                }
                catch
                {
                    MyPrecedingCalendarEvent = ReferenceCalendarEvent;
                }

                try
                {
                    MyNextCalendarEvent = AllEventDictionary[generatedIndexMatch.Split('_')[1]];
                }

                catch
                {
                    MyNextCalendarEvent = ReferenceCalendarEvent;
                }
                if (!(CurrentDictionary.ContainsKey(generatedIndexMatch)))
                {
                    double Distance = Location.calculateDistance(MyPrecedingCalendarEvent.myLocation, MyNextCalendarEvent.myLocation);
                    CurrentDictionary.Add(generatedIndexMatch, Distance);
                }
            }

            return CurrentDictionary;
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

            foreach (List<SubCalendarEvent> MyList in MyListOfSubCalendarEvents)//Loop creates a List of interferring SubCalendarEvens
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

            ListOfAllInterferringSubCalendarEvents=Utility.NotInList(ListOfAllInterferringSubCalendarEvents, ListOfAlreadyAssignedSubCalendarEvents);

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


            List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> EmptyIntialListOfSubCalendarEvemts = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>();

            for (int i = 0; i < JustFreeSpots.Length; i++)
            {
                EmptyIntialListOfSubCalendarEvemts.Add(new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>());
            }



            Tuple<List<TimeSpanWithStringID>, List<mTuple<bool, SubCalendarEvent>>> Arg14= ConvertSubCalendarEventToTimeSpanWitStringID(ListOfAllInterferringSubCalendarEvents);

            List<TimeSpanWithStringID> SubCalEventsAsTimeSpanWithStringID = Arg14.Item1;//ListOfAllInterferringSubCalendarEvents as TimeSpanWithStringID
            List<mTuple<bool, SubCalendarEvent>>  Arg15 = Arg14.Item2;

            Dictionary<string, mTuple<int, TimeSpanWithStringID>> Dict_StringTickAndCount = new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>();
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> Dict_StringTickAndCount_Cpy = new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>();

            foreach (TimeSpanWithStringID eachTimeSpanWithStringID in SubCalEventsAsTimeSpanWithStringID)
            {                
                if (Dict_StringTickAndCount.ContainsKey(eachTimeSpanWithStringID.ID))
                {
                    ++Dict_StringTickAndCount[eachTimeSpanWithStringID.ID].Item1;
                    ++Dict_StringTickAndCount_Cpy[eachTimeSpanWithStringID.ID].Item1;
                }
                else
                {
                    Dict_StringTickAndCount.Add(eachTimeSpanWithStringID.ID, new mTuple<int, TimeSpanWithStringID>(1, eachTimeSpanWithStringID));
                    Dict_StringTickAndCount_Cpy.Add(eachTimeSpanWithStringID.ID, new mTuple<int, TimeSpanWithStringID>(1, eachTimeSpanWithStringID));
                }
            }

            InterferringTimeSpanWithStringID_Cpy = Dict_StringTickAndCount_Cpy;//hack to keep track of available events


            Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> Dict_TimeLine_ListOfSubCalendarEvent = BuildDicitionaryOfTimeLineAndSubcalendarEvents(Arg15, DictionaryOfFreeTimeLineAndDictionaryOfCalendarEventAndListOfSubCalendarEvent, ToBeFittedTimeLine);

            Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> Dict_ConstrainedList = generateConstrainedList(Dict_TimeLine_ListOfSubCalendarEvent);

            Dictionary<TimeLine, List<mTuple<int, TimeSpanWithStringID>>> Dict_TimeLine_ListOfmTuple = new System.Collections.Generic.Dictionary<TimeLine,System.Collections.Generic.List<mTuple<int,TimeSpanWithStringID>>>();

            Dictionary<TimeLine, Dictionary<string, mTuple<int, TimeSpanWithStringID>>> Dict_TimeLine_Dict_string_mTple = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>();

            Dictionary<TimeLine, Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, mTuple<bool, SubCalendarEvent>>>>();

            Dictionary<TimeLine, Dictionary<string, mTuple<int, TimeSpanWithStringID>>> Dict_TimeLine_Dict_string_mTple_Constrained = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>();

            foreach (TimeLine eachTimeLine in Dict_TimeLine_ListOfSubCalendarEvent.Keys)
            {
                List<mTuple<bool, SubCalendarEvent>> LisOfSubCalEvent = Dict_TimeLine_ListOfSubCalendarEvent[eachTimeLine];
                Dictionary<string, mTuple<int, TimeSpanWithStringID>> myDict = new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>();
                Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> myDict0 = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, mTuple<bool, SubCalendarEvent>>>();
                
                
                foreach (mTuple<bool, SubCalendarEvent> eachmTuple in LisOfSubCalEvent)//goes Through each Subcalevent in Each timeline and generates a dict for a TimeTick To List of TimeSpanID
                {
                    if (myDict.ContainsKey(eachmTuple.Item2.ActiveDuration.Ticks.ToString()))
                    {
                        ++myDict[eachmTuple.Item2.ActiveDuration.Ticks.ToString()].Item1;
                    }
                    else
                    {
                        myDict.Add(eachmTuple.Item2.ActiveDuration.Ticks.ToString(), new mTuple<int, TimeSpanWithStringID>(1, new TimeSpanWithStringID(eachmTuple.Item2.ActiveDuration, eachmTuple.Item2.ActiveDuration.Ticks.ToString())));

                    }


                    
                    if (myDict0.ContainsKey(eachmTuple.Item2.ActiveDuration.Ticks.ToString()))
                    {


                        myDict0[eachmTuple.Item2.ActiveDuration.Ticks.ToString()].Add(eachmTuple.Item2.ID, eachmTuple);
                    }
                    else
                    {
                        Dictionary<string, mTuple<bool, SubCalendarEvent>> var17 = new System.Collections.Generic.Dictionary<string, mTuple<bool, SubCalendarEvent>>();
                        var17.Add(eachmTuple.Item2.ID, eachmTuple);
                        myDict0.Add(eachmTuple.Item2.ActiveDuration.Ticks.ToString(), var17);
                    }

                }
                Dict_TimeLine_Dict_string_mTple.Add(eachTimeLine, myDict);
                Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents.Add(eachTimeLine,myDict0);
            }


            foreach (TimeLine eachTimeLine in Dict_ConstrainedList.Keys)
            {
                List<mTuple<bool, SubCalendarEvent>> LisOfSubCalEvent = Dict_ConstrainedList[eachTimeLine];
                Dictionary<string, mTuple<int, TimeSpanWithStringID>> myDict = new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>();
                foreach (mTuple<bool, SubCalendarEvent> eachmTuple in LisOfSubCalEvent)//goes Through each Subcalevent in Each timeline and generates a dict for a TimeTick To List of TimeSpanID
                {
                    if (myDict.ContainsKey(eachmTuple.Item2.ActiveDuration.Ticks.ToString()))
                    {
                        ++myDict[eachmTuple.Item2.ActiveDuration.Ticks.ToString()].Item1;
                    }
                    else
                    {
                        myDict.Add(eachmTuple.Item2.ActiveDuration.Ticks.ToString(), new mTuple<int, TimeSpanWithStringID>(1, new TimeSpanWithStringID(eachmTuple.Item2.ActiveDuration, eachmTuple.Item2.ActiveDuration.Ticks.ToString())));
                    }
                }

                Dict_TimeLine_Dict_string_mTple_Constrained.Add(eachTimeLine, myDict);
            }

            /*
            foreach (TimeLine eachTimeLine in Dict_TimeLine_Dict_string_mTple.Keys)
            {
                Dictionary<string, mTuple<int, TimeSpanWithStringID>> stringMtuple = Dict_TimeLine_Dict_string_mTple[eachTimeLine];
                List<mTuple<int, TimeSpanWithStringID>> List_mTuple = new System.Collections.Generic.List<mTuple<int, TimeSpanWithStringID>>();

                foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in stringMtuple)
                {
                    TimeSpanWithStringID myTimeSpanWithStringID = eachKeyValuePair.Value.Item2;
                    List_mTuple.Add(new mTuple<int, TimeSpanWithStringID>(eachKeyValuePair.Value.Item1, myTimeSpanWithStringID));
                }

                Dict_TimeLine_ListOfmTuple.Add(eachTimeLine, List_mTuple);
            }

            */


            Dictionary<TimeLine, Tuple<Dictionary<string, mTuple<int, TimeSpanWithStringID>>, Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> var6 = new System.Collections.Generic.Dictionary<TimeLine, Tuple<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>, System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();

            //Dictionary<TimeLine, Dictionary<string, mTuple<int, TimeSpanWithStringID>>>
            int i0 = 0;
            List<TimeLine> var7 = Dict_TimeLine_Dict_string_mTple_Constrained.Keys.ToList();//List Of TimeLines pertaining to COnstrained List i.e restricted elements
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> var10 = new System.Collections.Generic.Dictionary<string,mTuple<int,TimeSpanWithStringID>>();
            List<KeyValuePair<TimeLine, Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> var11 = Dict_TimeLine_Dict_string_mTple_Constrained.ToList(); //Same as Dict_TimeLine_Dict_string_mTple_Constrained only as List of KeyValuePair
            Dictionary<TimeLine, Dictionary<string, mTuple<int, TimeSpanWithStringID>>> var14 = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>();
            Dictionary<TimeLine, Tuple<Dictionary<string, mTuple<int, TimeSpanWithStringID>>, Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> var15 = new System.Collections.Generic.Dictionary<TimeLine,Tuple<System.Collections.Generic.Dictionary<string,mTuple<int,TimeSpanWithStringID>>,System.Collections.Generic.Dictionary<string,mTuple<int,TimeSpanWithStringID>>>>();

            foreach (KeyValuePair<TimeLine, Dictionary<string, mTuple<int, TimeSpanWithStringID>>> eachKeyValuePair in Dict_TimeLine_Dict_string_mTple_Constrained)
            {
                i0 = var7.IndexOf(eachKeyValuePair.Key);
                Dictionary<string, mTuple<int, TimeSpanWithStringID>> var8 =eachKeyValuePair.Value;
                Dictionary<string, mTuple<int, TimeSpanWithStringID>> var9 = new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>();
                for (; i0<var7.Count;i0++ )
                {
                    Dictionary<string, mTuple<int, TimeSpanWithStringID>> var12=var11[i0].Value;
                    foreach(KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> var13 in var12)
                    {
                        if(var9.ContainsKey(var13.Key))
                        {
                            var9[var13.Key].Item1+=var13.Value.Item1;
                        }
                        else
                        {
                            var9.Add(var13.Key, new mTuple<int,TimeSpanWithStringID>(var13.Value.Item1,var13.Value.Item2));
                        }
                    }
                }
                var15.Add(eachKeyValuePair.Key, new Tuple<Dictionary<string, mTuple<int, TimeSpanWithStringID>>, Dictionary<string, mTuple<int, TimeSpanWithStringID>>>(eachKeyValuePair.Value, var9));
            }


            Stopwatch sw = new Stopwatch();

            sw.Start();
            List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> AllTImeLinesWithSnugPossibilities = generateTreeCallsToSnugArray(Dict_StringTickAndCount, JustFreeSpots.ToList(), 0, EmptyIntialListOfSubCalendarEvemts, Dict_TimeLine_Dict_string_mTple, var15);

            sw.Stop();


            List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> validMatches = getValidMatches(ListOfAllInterferringSubCalendarEvents, AllTImeLinesWithSnugPossibilities, Dict_TimeLine_Dict_string_mTple_Constrained);

            List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> AverageMatched = getAveragedOutTIimeLine(validMatches,0);

            Dict_ConstrainedList=stitchRestrictedSubCalendarEvent(JustFreeSpots.ToList(), 0, Dict_ConstrainedList);

            Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> Dict_TimeLine_ListOfSubCalendarEvent_Cpy= new System.Collections.Generic.Dictionary<TimeLine,System.Collections.Generic.List<mTuple<bool,SubCalendarEvent>>>(Dict_TimeLine_ListOfSubCalendarEvent);

            Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> DictWithTimeLine_ArrangedOptimizedSubCalEvents = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>()
;
            i0=0;
            List<mTuple<bool, SubCalendarEvent>> TotalArrangedElements = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            List<List<SubCalendarEvent>> TotalArrangedElements_NoMTuple = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
            

            foreach (KeyValuePair<TimeLine, Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> eachKeyValuePair in Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents)
            {
                List<mTuple<bool, SubCalendarEvent>> var16 = Dict_ConstrainedList[eachKeyValuePair.Key];
                List<BusyTimeLine> RestrictedBusySlots= new System.Collections.Generic.List<BusyTimeLine>();
                foreach (mTuple<bool, SubCalendarEvent> eachmTuple in var16)
                {
                    eachmTuple.Item1 = true;
                    RestrictedBusySlots.Add(eachmTuple.Item2.ActiveSlot);
                    string timeSpanString=eachmTuple.Item2.ActiveDuration.Ticks.ToString();
                    string SubEventID=eachmTuple.Item2.ID;
                    eachKeyValuePair.Value[timeSpanString][SubEventID] = eachmTuple;
                }
                eachKeyValuePair.Key.AddBusySlots(RestrictedBusySlots.ToArray());
                if (i0 == 5)
                {
                    ;
                }
                List<mTuple<bool, SubCalendarEvent>> ArrangedElements = stitchUnRestrictedSubCalendarEvent(eachKeyValuePair.Key, var16, Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents[eachKeyValuePair.Key], AverageMatched[i0]);
                
                
                TotalArrangedElements_NoMTuple.Add(Utility.mTupleToSubCalEvents(ArrangedElements));
                TotalArrangedElements.AddRange(ArrangedElements);
                DictWithTimeLine_ArrangedOptimizedSubCalEvents.Add(eachKeyValuePair.Key, ArrangedElements);
                Dict_TimeLine_ListOfSubCalendarEvent_Cpy.Remove(eachKeyValuePair.Key);
                
                
               


                foreach (List<mTuple<bool, SubCalendarEvent>> eachList in Dict_TimeLine_ListOfSubCalendarEvent_Cpy.Values)
                {
                    int i = 0;
                    for (; i < eachList.Count; i++)
                    {
                        /*if (eachList[i].Item1)
                        {
                            eachList.RemoveAt(i);
                            --i;
                        }*/

                        foreach(mTuple<bool, SubCalendarEvent> eachmTuple in ArrangedElements)
                        {
                            if (eachList[i].Item2.ID == eachmTuple.Item2.ID)
                            {
                                eachList.RemoveAt(i);
                                --i;
                                break;
                            }


                        }
                        
                        
                    }
                }


                if (i0 < Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents.Count - 1)
                {
                   // Dict_ConstrainedList = generateConstrainedList(Dict_TimeLine_ListOfSubCalendarEvent_Cpy);
                    //Dict_ConstrainedList = stitchRestrictedSubCalendarEvent(Dict_TimeLine_ListOfSubCalendarEvent_Cpy.Keys.ToList(), 0, Dict_ConstrainedList);
                }
                i0++;
            }

            
            

            /*
            List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> tempValue = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string,mTuple<int,TimeSpanWithStringID>>>>();
            tempValue.Add(AverageMatched);
            validMatches = getValidMatches(ListOfAllInterferringSubCalendarEvents, tempValue, Dict_TimeLine_Dict_string_mTple_Constrained);
            */





            List<List<List<SubCalendarEvent>>> ReValue = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();
            //List<List<List<SubCalendarEvent>>> ReValue = FixSubCalEventOrder(AllTImeLinesWithSnugPossibilities, JustFreeSpots);
            ReValue.Add(TotalArrangedElements_NoMTuple);
            
            return ReValue;
        }

        Dictionary<string, mTuple<int, TimeSpanWithStringID>> InterferringTimeSpanWithStringID_Cpy = new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>();


        



        List<mTuple<bool, SubCalendarEvent>> FurtherFillTimeLineWithSubCalEvents(List<mTuple<bool, SubCalendarEvent>>AllReadyAssignedSubCalEvents, TimeLine ReferenceTimeLine,Dictionary<TimeLine, Dictionary<string, mTuple<int, TimeSpanWithStringID>>> AllCompatibleWithList,Dictionary<TimeLine,Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> PossibleEntries)
        {
            /*
             * CompatibleWithList has whats left after stitchUnRestrictedSubCalendarEvent has removed all possible fittable Events
             */

            List<SubCalendarEvent> AssignedSubCalendarEvents = new System.Collections.Generic.List<SubCalendarEvent>();
            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in AllReadyAssignedSubCalEvents)
            {
                AssignedSubCalendarEvents.Add(eachmTuple.Item2);
            }

            ReferenceTimeLine.Empty();
            ReferenceTimeLine=Utility.AddSubCaleventsToTimeLine(ReferenceTimeLine, AssignedSubCalendarEvents);
            List<TimeLine> AllFreeSpots = ReferenceTimeLine.getAllFreeSlots().ToList();
            Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> matchingValidSubcalendarEvents = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();//Dictionary contains a match up of the free within already assigned variables and possible fillers
            Dictionary<TimeLine, Dictionary<string, mTuple<int,TimeSpanWithStringID>>> ForSnugArray =  new System.Collections.Generic.Dictionary<TimeLine,System.Collections.Generic.Dictionary<string,mTuple<int,TimeSpanWithStringID>>>();
            Dictionary<TimeLine, SnugArray> FreeSpotSnugArrays = new System.Collections.Generic.Dictionary<TimeLine, SnugArray>();
            Dictionary<TimeLine, List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> FreeSpotSnugPossibiilities = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();
            //Dictionary<TimeLine, List<TimeSpanWithStringID>> FreeSpotSnugPossibilities = new System.Collections.Generic.Dictionary<TimeLine, SnugArray>();
            
            foreach (TimeLine eachTimeLine in AllFreeSpots)
            {
                List<mTuple<bool, SubCalendarEvent>> PossibleFillers = removeSubCalEventsThatCantWorkWithTimeLine(eachTimeLine, PossibleEntries[ReferenceTimeLine].ToList());
                ForSnugArray.Add(eachTimeLine, new System.Collections.Generic.Dictionary<string,mTuple<int,TimeSpanWithStringID>>());
                
                foreach (mTuple<bool, SubCalendarEvent> eachmTuple in PossibleFillers)
                {
                    if (ForSnugArray[eachTimeLine].ContainsKey(eachmTuple.Item2.ActiveDuration.ToString()))
                    {
                        ++ForSnugArray[eachTimeLine][eachmTuple.Item2.ActiveDuration.ToString()].Item1;
                    }
                    else
                    {
                        ForSnugArray[eachTimeLine].Add(eachmTuple.Item2.ActiveDuration.ToString(), new mTuple<int, TimeSpanWithStringID>(1, new TimeSpanWithStringID(eachmTuple.Item2.ActiveDuration, eachmTuple.Item2.ActiveDuration.ToString())));
                    }
                }
                matchingValidSubcalendarEvents.Add(eachTimeLine, PossibleFillers);
            }


            foreach (TimeLine eachTimeLine in AllFreeSpots)
            {
                FreeSpotSnugArrays.Add(eachTimeLine, new SnugArray(ForSnugArray[eachTimeLine].Values.ToList(), eachTimeLine.TimelineSpan));
            }

            foreach (TimeLine eachTimeLine in AllFreeSpots)
            {
                FreeSpotSnugPossibiilities.Add(eachTimeLine, FreeSpotSnugArrays[eachTimeLine].MySnugPossibleEntries);
            }
        
        }




        Tuple<Dictionary<string, mTuple<int, TimeSpanWithStringID>>,Dictionary<string, mTuple<int, TimeSpanWithStringID>>>  UpdateCompatibleListOfTimeLine(Dictionary<string, mTuple<int, TimeSpanWithStringID>> CurrentCompatibleList,List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> MovedOverSet, TimeLine ReferenceTimeLine, Dictionary<string, mTuple<int, TimeSpanWithStringID>> LeftOuts,Dictionary<string, mTuple<int, TimeSpanWithStringID>> TotalOfMovedVariables)
        {
            //You need to create a situation that enforces a restricted event as being assigned first
            
            TimeSpan CurrentTotalOfSnugVariables = new TimeSpan(0);
            
            List<TimeSpanWithStringID> listOfmTuples = Utility.serialLizeMtuple_MTupleToVarT(LeftOuts.Values.ToList().OrderBy(obj => obj.Item2.timeSpan)).ToList();
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> retValue_MovedVariables = new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>();
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> retValue_CurrentCompatibleList = new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>();
                
            Dictionary<string, mTuple<int, TimeSpanWithStringID>>  CurrentCompatibleList_Cpy = SnugArray.CreateCopyOFSnuPossibilities(CurrentCompatibleList);
             //TimeSpanWithStringID SmallestTableEntry = listOfmTuples[0] ;

            TimeSpan SumOfLeftOuts= new TimeSpan(0);
            TimeSpan RemainderTimeSpan = new TimeSpan(0);
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> MovedOverListUpdate    = new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>();
            


            while(listOfmTuples.Count>0)
            {
                CurrentTotalOfSnugVariables = SnugArray.TotalTimeSpanOfSnugPossibility(CurrentCompatibleList_Cpy);
                SumOfLeftOuts += listOfmTuples[0].timeSpan;//.Item2.timeSpan;
                RemainderTimeSpan = ReferenceTimeLine.TimelineSpan - CurrentTotalOfSnugVariables;
                TimeSpan RemainderOfLeftOverChunk = SumOfLeftOuts - RemainderTimeSpan;
                List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> MovedOverSet_Cpy = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>();

                foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in MovedOverSet)
                {
                    MovedOverSet_Cpy.Add(compareMovedOverSetWithTotalPossibleEntries(eachDictionary, TotalOfMovedVariables));
                }


                if (RemainderOfLeftOverChunk.Ticks > 0)
                {
                    SnugArray FitsInChunkOfRemainder_SnugArray = new SnugArray(CurrentCompatibleList_Cpy.Values.ToList(), RemainderOfLeftOverChunk);
                    List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> SnugPossibilities = FitsInChunkOfRemainder_SnugArray.MySnugPossibleEntries;
                    List<Tuple<Dictionary<string, mTuple<int, TimeSpanWithStringID>>, Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> Viable_SnugPossibilities = getPlausibleEntriesFromMovedOverSet(MovedOverSet_Cpy, SnugPossibilities);
                    Dictionary<Dictionary<string, mTuple<int, TimeSpanWithStringID>>, List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> LeftAfterRemovalSnugPossibilities = new System.Collections.Generic.Dictionary<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>, System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();
                    foreach (Tuple<Dictionary<string, mTuple<int, TimeSpanWithStringID>>, Dictionary<string, mTuple<int, TimeSpanWithStringID>>> eachTuple in Viable_SnugPossibilities)
                    {
                        Dictionary<string, mTuple<int, TimeSpanWithStringID>> fromMovedOvers = eachTuple.Item1;
                        Dictionary<string, mTuple<int, TimeSpanWithStringID>> fromMovedOvers_Cpy = new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>(fromMovedOvers);
                        Dictionary<string, mTuple<int, TimeSpanWithStringID>> fromSnugPossibilities = eachTuple.Item2;

                        foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in fromSnugPossibilities)
                        {
                            fromMovedOvers_Cpy[eachKeyValuePair.Key] = new mTuple<int, TimeSpanWithStringID>(fromMovedOvers_Cpy[eachKeyValuePair.Key].Item1, fromMovedOvers_Cpy[eachKeyValuePair.Key].Item2);
                            fromMovedOvers_Cpy[eachKeyValuePair.Key].Item1 -= eachKeyValuePair.Value.Item1;
                            if (fromMovedOvers_Cpy[eachKeyValuePair.Key].Item1 == 0)
                            {
                                fromMovedOvers_Cpy.Remove(eachKeyValuePair.Key);
                            }
                        }

                        if (LeftAfterRemovalSnugPossibilities.ContainsKey(fromSnugPossibilities))
                        {
                            LeftAfterRemovalSnugPossibilities[fromSnugPossibilities].Add(fromMovedOvers_Cpy);
                        }
                        else
                        {
                            LeftAfterRemovalSnugPossibilities.Add(fromSnugPossibilities, new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>() { fromMovedOvers_Cpy });
                        }
                    }

                    Dictionary<string, mTuple<int, TimeSpanWithStringID>> BestSnugPossibility = getBestSnugPossiblity(CurrentCompatibleList_Cpy, LeftAfterRemovalSnugPossibilities);
                    retValue_MovedVariables = SnugArray.AddToSnugPossibilityList(retValue_MovedVariables, BestSnugPossibility);
                    Dictionary<string, mTuple<int, TimeSpanWithStringID>> CurrentCompatibleList_Cpy_updated = SnugArray.RemoveSnugPossibilityFromAnother(CurrentCompatibleList_Cpy, BestSnugPossibility);
                    if (SnugArray.Equals(CurrentCompatibleList_Cpy_updated, CurrentCompatibleList_Cpy))
                    {
                        break;
                    }
                    else
                    {
                        TotalOfMovedVariables = SnugArray.RemoveSnugPossibilityFromAnother(TotalOfMovedVariables, BestSnugPossibility);
                        CurrentCompatibleList_Cpy = CurrentCompatibleList_Cpy_updated;
                        if (MovedOverListUpdate.ContainsKey(listOfmTuples[0].ID))
                        {
                            ++MovedOverListUpdate[listOfmTuples[0].ID].Item1;
                        }
                        else
                        {
                            MovedOverListUpdate.Add(listOfmTuples[0].ID, new mTuple<int, TimeSpanWithStringID>(1, listOfmTuples[0]));
                        }
                        listOfmTuples.RemoveAt(0);
                    }
                }
                else 
                {
                    if (MovedOverListUpdate.ContainsKey(listOfmTuples[0].ID))
                    {
                        ++MovedOverListUpdate[listOfmTuples[0].ID].Item1;
                    }
                    else
                    {
                        MovedOverListUpdate.Add(listOfmTuples[0].ID, new mTuple<int, TimeSpanWithStringID>(1, listOfmTuples[0]));
                    }

                    listOfmTuples.RemoveAt(0);
                }


                


            }

            retValue_CurrentCompatibleList = SnugArray.RemoveSnugPossibilityFromAnother(CurrentCompatibleList, retValue_MovedVariables);
            retValue_CurrentCompatibleList = SnugArray.AddToSnugPossibilityList(CurrentCompatibleList, MovedOverListUpdate);


            Tuple<Dictionary<string, mTuple<int, TimeSpanWithStringID>>,Dictionary<string, mTuple<int, TimeSpanWithStringID>>> retValue(retValue_CurrentCompatibleList,retValue_MovedVariables);
            return retValue;
        }

        Dictionary<string, mTuple<int, TimeSpanWithStringID>>  compareMovedOverSetWithTotalPossibleEntries(Dictionary<string, mTuple<int, TimeSpanWithStringID>> MovedOverSet, Dictionary<string, mTuple<int, TimeSpanWithStringID>> TotalSet)
        {
            MovedOverSet=SnugArray.CreateCopyOFSnuPossibilities(MovedOverSet);
            List<KeyValuePair<string, mTuple<int, TimeSpanWithStringID>>> ListForDict = MovedOverSet.ToList();

            foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in ListForDict)
            {
                if (TotalSet.ContainsKey(eachKeyValuePair.Key))
                {
                    if (TotalSet[eachKeyValuePair.Key].Item1 < eachKeyValuePair.Value.Item1)
                    {
                        MovedOverSet[eachKeyValuePair.Key].Item1 = TotalSet[eachKeyValuePair.Key].Item1;
                    }
                }
                else
                {
                    MovedOverSet.Remove(eachKeyValuePair.Key);
                }
            }

            return MovedOverSet;
        }


        Dictionary<string, mTuple<int, TimeSpanWithStringID>> getBestSnugPossiblity(Dictionary<string, mTuple<int, TimeSpanWithStringID>> CurrentCompatibleList, Dictionary<Dictionary<string, mTuple<int, TimeSpanWithStringID>>, List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> LeftAfterRemovalSnugPossibilities)
        {
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> CurrentCompatibleList_cpy = SnugArray.CreateCopyOFSnuPossibilities(CurrentCompatibleList);
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> retValue=new System.Collections.Generic.Dictionary<string,mTuple<int,TimeSpanWithStringID>>();
            foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in LeftAfterRemovalSnugPossibilities.Keys)//tries each snugPossibility as a potential i
            {
                Dictionary<string, mTuple<int, TimeSpanWithStringID>> myCurrentCompatibleList = SnugArray.CreateCopyOFSnuPossibilities(CurrentCompatibleList_cpy);
                
                List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> ListOfLeftOfMovedOverSnugArray = LeftAfterRemovalSnugPossibilities[eachDictionary];
                foreach (string eachString in eachDictionary.Keys)
                {
                    myCurrentCompatibleList[eachString].Item1 -= eachDictionary[eachString].Item1;
                    if (myCurrentCompatibleList[eachString].Item1 < 1)//removes mTuple where the TImeSpan spring is zero
                    { 
                        myCurrentCompatibleList.Remove(eachString);
                    }
                }

                List<KeyValuePair<string, mTuple<int, TimeSpanWithStringID>>> OtherValuesFromMyCurrenCompatibleList = myCurrentCompatibleList.ToList();
                OtherValuesFromMyCurrenCompatibleList=OtherValuesFromMyCurrenCompatibleList.OrderBy(obj => obj.Value.Item2.timeSpan).ToList();
                OtherValuesFromMyCurrenCompatibleList.Reverse();
                

                foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary0 in ListOfLeftOfMovedOverSnugArray)
                {
                    Dictionary<string, mTuple<int, TimeSpanWithStringID>> Potential_retValue = SnugArray.CreateCopyOFSnuPossibilities(eachDictionary);
                    foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in OtherValuesFromMyCurrenCompatibleList)
                    {
                        if (eachDictionary0.ContainsKey(eachKeyValuePair.Key))
                        {
                            mTuple<int, TimeSpanWithStringID> UpdatedmTuple;
                            if (eachDictionary0[eachKeyValuePair.Key].Item1 < eachKeyValuePair.Value.Item1)
                            {
                                UpdatedmTuple = eachDictionary0[eachKeyValuePair.Key];
                            }
                            else
                            {
                                UpdatedmTuple = eachKeyValuePair.Value;
                            }
                            if (Potential_retValue.ContainsKey(UpdatedmTuple.Item2.ID))
                            {
                                Potential_retValue[UpdatedmTuple.Item2.ID].Item1 += UpdatedmTuple.Item1;
                            }

                        }
                    }

                    TimeSpan TotalTimeSpanSnug_Possible = SnugArray.TotalTimeSpanOfSnugPossibility(Potential_retValue);
                    TimeSpan TotalTimeSpanSnugRetValue = SnugArray.TotalTimeSpanOfSnugPossibility(retValue);
                    if (TotalTimeSpanSnugRetValue < TotalTimeSpanSnug_Possible)
                    {
                        retValue = Potential_retValue;
                    }
                    else 
                    {
                        if ((TotalTimeSpanSnug_Possible == TotalTimeSpanSnugRetValue)&&(retValue.Count < Potential_retValue.Count))
                        {
                            retValue = Potential_retValue;
                        }
                    }
                }
            }

            return retValue;
        }

        List<Tuple<Dictionary<string, mTuple<int, TimeSpanWithStringID>>,Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> getPlausibleEntriesFromMovedOverSet(List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> MovedOverSet, List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> SnugPossibilities)
        {
            /*
             * This function goes through the MovedOverSet and compares it with the snug possibilities.
             * It tries to find any set in the Movedoverset that has all the keys in the snug possibilities variable.
             * If it finds one with all these keys, it selects dict in which the int Count of the matching mTuple has a greater than or equal the current option of the current snug possibility mtuple
             */
            List<Tuple<Dictionary<string, mTuple<int, TimeSpanWithStringID>>, Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> retValue = new System.Collections.Generic.List<Tuple<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>, System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();
            foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in SnugPossibilities)// goes through each dictionary in snug possibiltiy
            {
                foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary0 in MovedOverSet)//goes through each dictionary in Moved Over set.
                {
                    bool isCurrenteachDictionary0Ok = false;
                    foreach (string eachString in eachDictionary.Keys)
                    {
                        if (eachDictionary0.ContainsKey(eachString))
                        {
                            if (eachDictionary0[eachString].Item1 >= eachDictionary[eachString].Item1)
                            {
                                isCurrenteachDictionary0Ok = true;
                            }
                            else
                            {
                                isCurrenteachDictionary0Ok = false;
                                break;
                            }
                        }
                        else 
                        {
                            isCurrenteachDictionary0Ok=false;
                            break;
                        }
                    }
                    if(isCurrenteachDictionary0Ok)
                    {
                        retValue.Add(new Tuple<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>, System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>(eachDictionary0, eachDictionary));
                    }
                }

            }

            return retValue;


        }

        List<mTuple<bool, SubCalendarEvent>> stitchUnRestrictedSubCalendarEvent(TimeLine FreeBoundary, List<mTuple<bool, SubCalendarEvent>> restrictedSnugFitAvailable, Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries, Dictionary<string, mTuple<int, TimeSpanWithStringID>> CompatibleWithList)
        {
            TimeLine[] AllFreeSpots = FreeBoundary.getAllFreeSlots();
            int TotalEventsForThisTImeLine = 0;

            foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in CompatibleWithList)
            {
                TotalEventsForThisTImeLine += eachKeyValuePair.Value.Item1;
            }


            DateTime EarliestReferenceTIme = FreeBoundary.Start;
            List<mTuple<bool, SubCalendarEvent>> FrontPartials = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            List<mTuple<bool, SubCalendarEvent>> EndPartials = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            Dictionary<DateTime, List<mTuple<bool, SubCalendarEvent>>> FrontPartials_Dict = new System.Collections.Generic.Dictionary<DateTime, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();
            Dictionary<DateTime, List<mTuple<bool, SubCalendarEvent>>> EndPartials_Dict = new System.Collections.Generic.Dictionary<DateTime, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();
            Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries_Cpy = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, mTuple<bool, SubCalendarEvent>>>();
            foreach (KeyValuePair<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleEntries)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                 Dictionary<string, mTuple<bool, SubCalendarEvent>> NewDictEntry= new Dictionary<string, mTuple<bool, SubCalendarEvent>>();
                foreach(KeyValuePair<string, mTuple<bool, SubCalendarEvent>> KeyValuePair0 in eachKeyValuePair.Value)
                {
                    mTuple<bool, SubCalendarEvent> MyEvent = KeyValuePair0.Value;
                    bool isInrestrictedSnugFitAvailable = false;
                    foreach (mTuple<bool, SubCalendarEvent> eachMtuple in restrictedSnugFitAvailable)//checks if event is in restricted list
                    {
                        if (eachMtuple.Item2.ID == MyEvent.Item2.ID)
                        {
                            isInrestrictedSnugFitAvailable = true;
                            break;
                        }
                        
                    }


                    if (!isInrestrictedSnugFitAvailable)
                    {
                        NewDictEntry.Add(KeyValuePair0.Value.Item2.ID, KeyValuePair0.Value);
                        if (FreeBoundary.IsDateTimeWithin(KeyValuePair0.Value.Item2.getCalendarEventRange.Start))
                        {
                                FrontPartials.Add(KeyValuePair0.Value);
                        }
                        else
                        {
                            if (FreeBoundary.IsDateTimeWithin(KeyValuePair0.Value.Item2.getCalendarEventRange.End))
                            {
                                EndPartials.Add(KeyValuePair0.Value);
                            }
                        }
                    }
                }
                PossibleEntries_Cpy.Add(eachKeyValuePair.Key,NewDictEntry);

            }

            FrontPartials = FrontPartials.OrderBy(obj => obj.Item2.getCalendarEventRange.Start).ToList(); ;
            EndPartials = EndPartials.OrderBy(obj => obj.Item2.getCalendarEventRange.End).ToList();

            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in FrontPartials)//populates FrontPartials_Dict in ordered manner since FrontPartials is ordered
            { 
                if(FrontPartials_Dict.ContainsKey(eachmTuple.Item2.getCalendarEventRange.Start))
                {
                    FrontPartials_Dict[eachmTuple.Item2.getCalendarEventRange.Start].Add(eachmTuple);
                }
                else
                {
                    FrontPartials_Dict.Add(eachmTuple.Item2.getCalendarEventRange.Start, new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>() { eachmTuple });
                }
                
            }

            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in EndPartials)//populates EndPartials_Dict in ordered manner since EndPartials is ordered
            {
                if (EndPartials_Dict.ContainsKey(eachmTuple.Item2.getCalendarEventRange.Start))
                {
                    EndPartials_Dict[eachmTuple.Item2.getCalendarEventRange.Start].Add(eachmTuple);
                }
                else
                {
                    EndPartials_Dict.Add(eachmTuple.Item2.getCalendarEventRange.Start, new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>() { eachmTuple });
                }
            }


            List<SubCalendarEvent> CompleteArranegement = new System.Collections.Generic.List<SubCalendarEvent>();
            int StartingReferneceIndex = 0;
            
            
            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in restrictedSnugFitAvailable)//removes the restricted from CompatibleWithList
            {
                --CompatibleWithList[eachmTuple.Item2.ActiveDuration.Ticks.ToString()].Item1;
                //PossibleEntries_Cpy[eachmTuple.Item2.ActiveDuration.Ticks.ToString()].Remove(eachmTuple.Item2.ID);
            }

            List<DateTime> ListOfFrontPartialsStartTime = FrontPartials_Dict.Keys.ToList();

            int i = 0;
            int j = 0;
            int FrontPartialCounter = 0;
            for (; i < restrictedSnugFitAvailable.Count; i++)
            {
                //bool isFreeSpotBeforeRigid = AllFreeSpots[i].End <= restrictedSnugFitAvailable[i].Item2.Start;
                
                restrictedSnugFitAvailable[i].Item2.PinSubEventsToStart(new TimeLine(EarliestReferenceTIme, restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End));
                List<BusyTimeLine> RestrictedBusySlots = new System.Collections.Generic.List<BusyTimeLine>();
                FreeBoundary = new TimeLine(FreeBoundary.Start, FreeBoundary.End);
                foreach (mTuple<bool, SubCalendarEvent> eachmTuple in restrictedSnugFitAvailable)
                {
                    eachmTuple.Item1 = true;
                    RestrictedBusySlots.Add(eachmTuple.Item2.ActiveSlot);
                    string timeSpanString = eachmTuple.Item2.ActiveDuration.Ticks.ToString();
                    string SubEventID = eachmTuple.Item2.ID;

                }
                FreeBoundary.AddBusySlots(RestrictedBusySlots.ToArray());
                
                /*AllFreeSpots = FreeBoundary.getAllFreeSlots();
                
                
                

                foreach (TimeLine eachTimeLine in AllFreeSpots)
                {
                    if (restrictedSnugFitAvailable[i].Item2.Start == eachTimeLine.End)
                    {
                        PertinentFreeSpot = eachTimeLine;
                        break;
                    }
                }*/

                List<SubCalendarEvent> LowestCostArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
                TimeLine PertinentFreeSpot = null;
                TimeLine FreeSpotUpdated=null;
                j = i + 1;
                if (ListOfFrontPartialsStartTime.Count > 0)//fits any sub calEvent in preceeding restricting free spot
                {
                    DateTime RestrictedStopper = restrictedSnugFitAvailable[i].Item2.Start;

                    
                    bool breakForLoop = false;
                    bool PreserveRestrictedIndex = false;
                    for (; FrontPartialCounter < ListOfFrontPartialsStartTime.Count; FrontPartialCounter++)
                    {
                        DateTime PertinentFreeSpotStart = EarliestReferenceTIme;
                        DateTime PertinentFreeSpotEnd;
                        if ((ListOfFrontPartialsStartTime[FrontPartialCounter] < RestrictedStopper))
                        {
                            PertinentFreeSpotEnd = ListOfFrontPartialsStartTime[FrontPartialCounter];
                            FrontPartials_Dict.Remove(ListOfFrontPartialsStartTime[FrontPartialCounter]);
                            ListOfFrontPartialsStartTime.RemoveAt(FrontPartialCounter);
                            --FrontPartialCounter;
                            PreserveRestrictedIndex = true;
                        }
                        else
                        {
                            PertinentFreeSpotEnd = RestrictedStopper;

                            if (breakForLoop)
                            {//populates with final boundary for each restricted
                                PertinentFreeSpot = new TimeLine(PertinentFreeSpotStart, PertinentFreeSpotEnd);
                                LowestCostArrangement = OptimizeArrangeOfSubCalEvent(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(null, restrictedSnugFitAvailable[i].Item2), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy);

                                if (LowestCostArrangement.Count>0)
                                {   
                                    if (!(LowestCostArrangement[0].getCalendarEventRange.Start == PertinentFreeSpot.Start))//Pin SubEvents To Start
                                    {//if the first element is not a partial Sub Cal Event element
                                        FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                                        Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                                    }
                                    else
                                    {
                                        FreeSpotUpdated = new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                                        Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot);
                                    }
                                    EarliestReferenceTIme = PertinentFreeSpot.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                                }
                                foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                                {
                                    --CompatibleWithList[eachSubCalendarEvent.ActiveDuration.Ticks.ToString()].Item1;
                                    PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration.Ticks.ToString()].Remove(eachSubCalendarEvent.ID);
                                }


                                LowestCostArrangement = CompleteArranegement.Concat(LowestCostArrangement).ToList();
                                LowestCostArrangement = PlaceSubCalEventInLowestCostPosition(FreeBoundary, restrictedSnugFitAvailable[i].Item2, LowestCostArrangement);
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeBoundary);

                                CompleteArranegement = LowestCostArrangement;
                                EarliestReferenceTIme = LowestCostArrangement[LowestCostArrangement.Count - 1].End;

                                PreserveRestrictedIndex = false;
                                break;
                            }

                            --FrontPartialCounter;
                            if (j < restrictedSnugFitAvailable.Count)
                            {
                                RestrictedStopper = restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End > restrictedSnugFitAvailable[j].Item2.Start ? restrictedSnugFitAvailable[j].Item2.Start : restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End;
                            }
                            else 
                            {
                                RestrictedStopper = restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End > FreeBoundary.End ? FreeBoundary.End : restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End;
                            }
                                RestrictedStopper -= restrictedSnugFitAvailable[i].Item2.ActiveDuration;
                            breakForLoop = true;
                        }
                        PertinentFreeSpot = new TimeLine(PertinentFreeSpotStart, PertinentFreeSpotEnd);
                        LowestCostArrangement= OptimizeArrangeOfSubCalEvent(PertinentFreeSpot,new Tuple<SubCalendarEvent, SubCalendarEvent>(null, restrictedSnugFitAvailable[i].Item2),CompatibleWithList.Values.ToList(),PossibleEntries_Cpy);

                        if (LowestCostArrangement.Count > 0)
                        {
                            if (!(LowestCostArrangement[0].getCalendarEventRange.Start == PertinentFreeSpot.Start))//Pin SubEvents To Start
                            {//if the first element is not a partial Sub Cal Event element
                                FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);

                            }
                            else
                            {
                                FreeSpotUpdated = new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                                Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot);
                            }
                            EarliestReferenceTIme = PertinentFreeSpot.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                        }

                        foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                        {
                            --CompatibleWithList[eachSubCalendarEvent.ActiveDuration.Ticks.ToString()].Item1;
                            PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration.Ticks.ToString()].Remove(eachSubCalendarEvent.ID);
                        }
                        CompleteArranegement.AddRange(LowestCostArrangement);
                    }
                    if (PreserveRestrictedIndex)//verifies if we took the path of restricted or front partial element. The latter needs a preservation of the current restricted Subcalevent index index 
                    {
                        --i;
                    }
                }
                else 
                {//No FrontPartials
                    DateTime ReferenceEndTime = restrictedSnugFitAvailable[i].Item2.Start;
                    PertinentFreeSpot = new TimeLine(EarliestReferenceTIme, ReferenceEndTime);
                    LowestCostArrangement=OptimizeArrangeOfSubCalEvent(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(null, restrictedSnugFitAvailable[i].Item2), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy);

                    if (LowestCostArrangement.Count > 0)
                    {
                        if (!(LowestCostArrangement[0].getCalendarEventRange.Start == PertinentFreeSpot.Start))//Pin SubEvents To Start
                        {//if the first element is not a partial Sub Cal Event element
                            FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                            Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                        }
                        else
                        {
                            FreeSpotUpdated = new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                            Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot);
                        }
                        EarliestReferenceTIme = FreeSpotUpdated.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                    }

                    foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                    {
                        --CompatibleWithList[eachSubCalendarEvent.ActiveDuration.Ticks.ToString()].Item1;
                        PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration.Ticks.ToString()].Remove(eachSubCalendarEvent.ID);
                    }


                    List<SubCalendarEvent> AdditionalCOstArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
                    if (j < restrictedSnugFitAvailable.Count)
                    {
                        DateTime StartDateTimeAfterFitting = PertinentFreeSpot.End;//this is the barring end time of the preceding boundary search

                        DateTime RelativeEndTime = restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End > restrictedSnugFitAvailable[j].Item2.Start ? restrictedSnugFitAvailable[j].Item2.Start : restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End;

                        RelativeEndTime -= restrictedSnugFitAvailable[i].Item2.ActiveDuration;
                        TimeLine CurrentlyFittedTimeLine = new TimeLine(StartDateTimeAfterFitting, RelativeEndTime);
                        AdditionalCOstArrangement = OptimizeArrangeOfSubCalEvent(CurrentlyFittedTimeLine, new Tuple<SubCalendarEvent, SubCalendarEvent>(null, null), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy);
                        if (AdditionalCOstArrangement.Count > 0)
                        {//Additional get populated
                            if (!(AdditionalCOstArrangement[0].getCalendarEventRange.Start == CurrentlyFittedTimeLine.Start))//Pin SubEvents To Start
                            {//if the first element is not a partial Sub Cal Event element
                                FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, CurrentlyFittedTimeLine.End);
                                Utility.PinSubEventsToStart(AdditionalCOstArrangement, FreeSpotUpdated);
                            }
                            else
                            {
                                FreeSpotUpdated = new TimeLine(AdditionalCOstArrangement[0].getCalendarEventRange.Start, CurrentlyFittedTimeLine.End);
                                Utility.PinSubEventsToStart(AdditionalCOstArrangement, FreeSpotUpdated);
                            }

                            foreach (SubCalendarEvent eachSubCalendarEvent in AdditionalCOstArrangement)
                            {
                                --CompatibleWithList[eachSubCalendarEvent.ActiveDuration.Ticks.ToString()].Item1;
                                PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration.Ticks.ToString()].Remove(eachSubCalendarEvent.ID);
                            }


                            RelativeEndTime = AdditionalCOstArrangement[AdditionalCOstArrangement.Count - 1].End;
                            RelativeEndTime += restrictedSnugFitAvailable[i].Item2.ActiveDuration; ;
                            FreeSpotUpdated = new TimeLine(FreeSpotUpdated.Start, RelativeEndTime);
                            AdditionalCOstArrangement = PlaceSubCalEventInLowestCostPosition(FreeSpotUpdated, restrictedSnugFitAvailable[i].Item2, AdditionalCOstArrangement);
                        }
                        else
                        {//if there is no other Restricted in list
                            RelativeEndTime += restrictedSnugFitAvailable[i].Item2.ActiveDuration;

                            CurrentlyFittedTimeLine = new TimeLine(CurrentlyFittedTimeLine.Start, RelativeEndTime);

                            AdditionalCOstArrangement = PlaceSubCalEventInLowestCostPosition(CurrentlyFittedTimeLine, restrictedSnugFitAvailable[i].Item2, AdditionalCOstArrangement);
                        }
                    }
                    else 
                    {
                        DateTime RelativeEndTime = restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End > FreeBoundary.End ? FreeBoundary.End : restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End;
                        TimeLine CurrentlyFittedTimeLine = new TimeLine(EarliestReferenceTIme, RelativeEndTime);
                        AdditionalCOstArrangement = PlaceSubCalEventInLowestCostPosition(CurrentlyFittedTimeLine, restrictedSnugFitAvailable[i].Item2, AdditionalCOstArrangement);
                    }

                    List<SubCalendarEvent> var2 = new System.Collections.Generic.List<SubCalendarEvent>();//List is a addition of LowestCostArrangement and AdditionalCOstArrangement
                    var2 = LowestCostArrangement.Concat(AdditionalCOstArrangement).ToList();

                    CompleteArranegement.AddRange(LowestCostArrangement);
                    CompleteArranegement.AddRange(AdditionalCOstArrangement);
                    if (CompleteArranegement.Count>0)
                    {
                        EarliestReferenceTIme=CompleteArranegement[CompleteArranegement.Count-1].End;
                    }
                }



                /*j = i + 1;
                List<SubCalendarEvent> AdditionalCOstArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
                if (j < restrictedSnugFitAvailable.Count)
                {

                    DateTime StartDateTimeAfterFitting = restrictedSnugFitAvailable[i].Item2.Start;
                    if (PertinentFreeSpot!=null)
                    {
                        StartDateTimeAfterFitting= PertinentFreeSpot.Start;
                    }

                    DateTime RelativeEndTime = restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End > restrictedSnugFitAvailable[j].Item2.Start ? restrictedSnugFitAvailable[j].Item2.Start : restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End;
                        
                    RelativeEndTime-=restrictedSnugFitAvailable[i].Item2.ActiveDuration;
                    TimeLine CurrentlyFittedTimeLine = new TimeLine(StartDateTimeAfterFitting + Utility.SumOfActiveDuration(LowestCostArrangement), RelativeEndTime);
                    SnugArray BestFit = new SnugArray(CompatibleWithList.Values.ToList(), CurrentlyFittedTimeLine.TimelineSpan);
                    List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> AllPossibleBestFit = BestFit.MySnugPossibleEntries;
                    List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> var3 = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();
                    if (AllPossibleBestFit.Count > 0)
                    {
                        var3.Add(AllPossibleBestFit);
                        List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> AveragedBestFit = getAveragedOutTIimeLine(var3, 0);
                        Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> removedImpossibleValue = removeSubCalEventsThatCantWorkWithTimeLine(CurrentlyFittedTimeLine, PossibleEntries_Cpy);
                        List<List<SubCalendarEvent>> PossibleSubCaleventsCobination = generateCombinationForDifferentEntries(AveragedBestFit[0], removedImpossibleValue);
                        AdditionalCOstArrangement = getArrangementWithLowestDistanceCost(PossibleSubCaleventsCobination, new Tuple<SubCalendarEvent,SubCalendarEvent>(null,null));
                    }
                    else 
                    {
                        ;
                    }
                }

                TimeLine LimitingTimeFrame = new TimeLine(EarliestReferenceTIme, EarliestReferenceTIme + Utility.SumOfActiveDuration(LowestCostArrangement) + Utility.SumOfActiveDuration(AdditionalCOstArrangement) + restrictedSnugFitAvailable[i].Item2.ActiveDuration);

                foreach (SubCalendarEvent eachSubCalendarEvent in AdditionalCOstArrangement)
                {
                    --CompatibleWithList[eachSubCalendarEvent.ActiveDuration.Ticks.ToString()].Item1;
                    PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration.Ticks.ToString()].Remove(eachSubCalendarEvent.ID);
                }
                LowestCostArrangement.AddRange(AdditionalCOstArrangement);
                LowestCostArrangement = PlaceSubCalEventInLowestCostPosition(LimitingTimeFrame, restrictedSnugFitAvailable[i].Item2, LowestCostArrangement);
                CompleteArranegement.AddRange(LowestCostArrangement);



                EarliestReferenceTIme = (LimitingTimeFrame.End);*/
            }


            { //Handles THe Last Free Space for posisble fit

                TimeLine CurrentlyFittedTimeLine = new TimeLine(EarliestReferenceTIme,FreeBoundary.End);
                SnugArray BestFit = new SnugArray(CompatibleWithList.Values.ToList(), CurrentlyFittedTimeLine.TimelineSpan);
                List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> AllPossibleBestFit = BestFit.MySnugPossibleEntries;
                List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> var3 = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();
                List<SubCalendarEvent> AdditionalCOstArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
                if (AllPossibleBestFit.Count > 0)
                {
                    var3.Add(AllPossibleBestFit);
                    List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> AveragedBestFit = getAveragedOutTIimeLine(var3, 0);
                    Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> removedImpossibleValue = removeSubCalEventsThatCantWorkWithTimeLine(CurrentlyFittedTimeLine, PossibleEntries_Cpy);
                    List<List<SubCalendarEvent>> PossibleSubCaleventsCobination = generateCombinationForDifferentEntries(AveragedBestFit[0], removedImpossibleValue);
                    if (CompleteArranegement.Count > 0)
                    {
                        AdditionalCOstArrangement = getArrangementWithLowestDistanceCost(PossibleSubCaleventsCobination, new Tuple<SubCalendarEvent, SubCalendarEvent>(CompleteArranegement[CompleteArranegement.Count - 1], null));
                    }
                    else 
                    {
                        AdditionalCOstArrangement = getArrangementWithLowestDistanceCost(PossibleSubCaleventsCobination, new Tuple<SubCalendarEvent, SubCalendarEvent>(null, null));
                    }
                }
                //List<SubCalendarEvent> LowestCostArrangement=getArrangementWithLowestDistanceCost(PossibleSubCaleventsCobination);

                foreach (SubCalendarEvent eachSubCalendarEvent in AdditionalCOstArrangement)
                {
                    --CompatibleWithList[eachSubCalendarEvent.ActiveDuration.Ticks.ToString()].Item1;
                    PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration.Ticks.ToString()].Remove(eachSubCalendarEvent.ID);
                }
                CompleteArranegement.AddRange(AdditionalCOstArrangement);
                
            }


            



            List<mTuple<bool, SubCalendarEvent>> retValue= new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();

            foreach(SubCalendarEvent eachSubCalendarEvent in CompleteArranegement)
            {
                PossibleEntries[eachSubCalendarEvent.ActiveDuration.Ticks.ToString()][eachSubCalendarEvent.ID].Item1 = true;
                retValue.Add(PossibleEntries[eachSubCalendarEvent.ActiveDuration.Ticks.ToString()][eachSubCalendarEvent.ID]);
            }

            //List<List<SubCalendarEvent>> unrestrictedValidCombinations = generateCombinationForDifferentEntries(CompatibleWithList, PossibleEntries);

            retValue=reAlignSubCalEvents(FreeBoundary, retValue);
            if (TotalEventsForThisTImeLine != retValue.Count)
            {
                ;
            }
            return retValue;

        }


        List<SubCalendarEvent> OptimizeArrangeOfSubCalEvent(TimeLine PertinentFreeSpot, Tuple<SubCalendarEvent, SubCalendarEvent> BoundaryCalendarEvent, List<mTuple<int, TimeSpanWithStringID>> CompatibleWithList,Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries_Cpy )
        {
            SnugArray BestFit_beforeBreak = new SnugArray(CompatibleWithList, PertinentFreeSpot.TimelineSpan);
            List<SubCalendarEvent> LowestCostArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
            List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> AllPossibleBestFit_beforeBreak = BestFit_beforeBreak.MySnugPossibleEntries;
            List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> var3_beforeBreak = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();
            if (AllPossibleBestFit_beforeBreak.Count > 0)
            {
                var3_beforeBreak.Add(AllPossibleBestFit_beforeBreak);
                List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> AveragedBestFit = getAveragedOutTIimeLine(var3_beforeBreak, 0);
                Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> removedImpossibleValue = removeSubCalEventsThatCantWorkWithTimeLine(PertinentFreeSpot, PossibleEntries_Cpy);
                List<List<SubCalendarEvent>> PossibleSubCaleventsCobination = generateCombinationForDifferentEntries(AveragedBestFit[0], removedImpossibleValue);
                LowestCostArrangement = getArrangementWithLowestDistanceCost(PossibleSubCaleventsCobination, BoundaryCalendarEvent);
                TimeLine FreeSpotUpdated;
                
            }

            return LowestCostArrangement;
        
        }


        //Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> removedImpossibleValue = removeSubCalEventsThatCantWorkWithTimeLine(PertinentFreeSpot, PossibleEntries_Cpy);
        List<mTuple<bool, SubCalendarEvent>> removeSubCalEventsThatCantWorkWithTimeLine(TimeLine PertinentFreeSpot, List<KeyValuePair<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> PossibleEntries)
        {
            List<mTuple<bool, SubCalendarEvent>> retValue = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();

            foreach (KeyValuePair<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleEntries)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                Dictionary<string, mTuple<bool, SubCalendarEvent>> UpdatedEntries = new System.Collections.Generic.Dictionary<string, mTuple<bool, SubCalendarEvent>>();

                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> eachKeyValuePair0 in eachKeyValuePair.Value)
                {
                    if (eachKeyValuePair0.Value.Item2.canExistWithinTimeLine(PertinentFreeSpot))
                    {
                        retValue.Add(eachKeyValuePair0.Value);
                    }
                }
            }

            return retValue;
            
        
        }
        
        
        Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> removeSubCalEventsThatCantWorkWithTimeLine(TimeLine PertinentFreeSpot, Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries)
        {
            Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> retValue = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, mTuple<bool, SubCalendarEvent>>>();


            foreach (KeyValuePair<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleEntries)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                Dictionary<string, mTuple<bool, SubCalendarEvent>> UpdatedEntries = new System.Collections.Generic.Dictionary<string,mTuple<bool,SubCalendarEvent>>();
                
                foreach(KeyValuePair<string,mTuple<bool, SubCalendarEvent>> eachKeyValuePair0 in eachKeyValuePair.Value)
                {
                    if(eachKeyValuePair0.Value.Item2.canExistWithinTimeLine(PertinentFreeSpot))
                    {
                        UpdatedEntries.Add(eachKeyValuePair0.Key,eachKeyValuePair0.Value);
                    }
                }
 
                
                
                retValue.Add(eachKeyValuePair.Key, UpdatedEntries);
            }


            return retValue;
        }

        List<List<mTuple<bool, SubCalendarEvent>>> ThisWillClash = new System.Collections.Generic.List<System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();

        List<mTuple<bool, SubCalendarEvent>> reAlignSubCalEvents(TimeLine BoundaryTimeLine,List<mTuple<bool, SubCalendarEvent>> ListOfEvents)
        {
            DateTime ReferenceTime = BoundaryTimeLine.Start;
            TimeLine Boundary_Cpy= BoundaryTimeLine.CreateCopy();
            List<mTuple<bool, SubCalendarEvent>> myClashers= new System.Collections.Generic.List<mTuple<bool,SubCalendarEvent>>();


            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in ListOfEvents)
            {



                if (!eachmTuple.Item2.PinSubEventsToStart(Boundary_Cpy))
                {
                    myClashers.Add(eachmTuple);
                    
                    //throw new Exception("error in your shift algorithm");
                }
                else
                {
                    Boundary_Cpy = new TimeLine(eachmTuple.Item2.End, BoundaryTimeLine.End);
                };
                


            }


            ThisWillClash.Add(myClashers);
            

            return ListOfEvents;
        }

        List<SubCalendarEvent> PlaceSubCalEventInLowestCostPosition(TimeLine MyLimitingTimeLine, SubCalendarEvent mySubcalevetn, List<SubCalendarEvent> OptimizedArrangementOfEvent)
        {

            /**Hack Solution Start, this just assumes all events are right next to each other and appends mySubcalevetn to the end. It also shifts this sub cal event to represent this shift **/
            DateTime RelativeStartTime = MyLimitingTimeLine.Start + Utility.SumOfActiveDuration(OptimizedArrangementOfEvent);

            TimeSpan DifferenceInTimeSpan = RelativeStartTime - mySubcalevetn.ActiveSlot.Start;
            //mySubcalevetn.shiftEvent(DifferenceInTimeSpan);
            /**Hack Solution End**/
            OptimizedArrangementOfEvent.Add(mySubcalevetn);

            Utility.PinSubEventsToStart(OptimizedArrangementOfEvent, MyLimitingTimeLine);

            return OptimizedArrangementOfEvent.ToList();
        }

        List<SubCalendarEvent> getArrangementWithLowestDistanceCost(List<List<SubCalendarEvent>> viableCombinations,Tuple<SubCalendarEvent, SubCalendarEvent> BoundinngSubCaEvents)
        {
            double LowestCost= double.PositiveInfinity;
            List<SubCalendarEvent> retValue= new System.Collections.Generic.List<SubCalendarEvent>();

            
            
            foreach (List<SubCalendarEvent> eachList in viableCombinations)
            {
                Tuple<ICollection<SubCalendarEvent>, double> OptimizedArrangement;
                if (!(eachList.Count > 1))
                {
                    List<Location> AllLocations = new System.Collections.Generic.List<Location>();
                    List<SubCalendarEvent> MyList= eachList.ToList();
                    MyList.Insert(0, BoundinngSubCaEvents.Item1);
                    MyList.Add(BoundinngSubCaEvents.Item2);
                    foreach (SubCalendarEvent eachSubCalendarEvent in MyList)
                    {
                        if (eachSubCalendarEvent != null)
                        { AllLocations.Add(eachSubCalendarEvent.myLocation); }
                    }
                    OptimizedArrangement = new Tuple<System.Collections.Generic.ICollection<SubCalendarEvent>, double>(eachList, Location.calculateDistance(AllLocations));
                }
                else
                {
                    OptimizedArrangement = DistanceSolver.Run(eachList);
                    if (OptimizedArrangement.Item2 == double.PositiveInfinity)
                    {
                        OptimizedArrangement = new Tuple<System.Collections.Generic.ICollection<SubCalendarEvent>, double>(eachList, OptimizedArrangement.Item2);
                    }
                }
                 
                

                if (LowestCost >= OptimizedArrangement.Item2)
                {
                    LowestCost = OptimizedArrangement.Item2;
                    retValue = OptimizedArrangement.Item1.ToList();
                }
            }
            return retValue;
        }

        Tuple<List<SubCalendarEvent>,double>  rearrangeForOptimizedLocationOptimizedLocation(List<SubCalendarEvent> ListOfLocations)
        {
            Tuple<List<SubCalendarEvent>, double> retValue=new Tuple<System.Collections.Generic.List<SubCalendarEvent>,double>(ListOfLocations,calculateCostOSubCalArrangement(ListOfLocations));
            return retValue;
        }

        double calculateCostOSubCalArrangement(List<SubCalendarEvent> CurrentArrangementOfSubcalevent)
        { 
            int i =0;
            double retValue = 0;



            for (; i < CurrentArrangementOfSubcalevent.Count-1; i++)
            {
                retValue += SubCalendarEvent.CalculateDistance(CurrentArrangementOfSubcalevent[i], CurrentArrangementOfSubcalevent[i + 1]);
            }

            return retValue;
        }

        List<List<SubCalendarEvent>> generateCombinationForDifferentEntries(Dictionary<string, mTuple<int, TimeSpanWithStringID>> CompatibleWithList, Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries)
        {

            List<List<List<string>>> MAtrixedSet = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<string>>>();
            Dictionary<string, mTuple<int, List<SubCalendarEvent>>> var4 = new System.Collections.Generic.Dictionary<string, mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>>();
            List<List<SubCalendarEvent>> retValue = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
            foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair0 in CompatibleWithList)
            {
                string eachString = eachKeyValuePair0.Key;
                
                Dictionary<string, mTuple<bool, SubCalendarEvent>> var1 = PossibleEntries[eachString];
                List<List<string>> receivedValue = new System.Collections.Generic.List<System.Collections.Generic.List<string>>();
                Dictionary<string, int> var2 = new System.Collections.Generic.Dictionary<string, int>();
                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> eachKeyValuePair in var1)
                {
                    string ParentID=eachKeyValuePair.Value.Item2.SubEvent_ID.getStringIDAtLevel(0);
                    if (var2.ContainsKey(ParentID))
                    {
                        ++var2[ParentID];
                        var4[ParentID].Item2.Add(eachKeyValuePair.Value.Item2);
                    }
                    else
                    {
                        var2.Add(ParentID, 1);
                        List<SubCalendarEvent> var5= new System.Collections.Generic.List<SubCalendarEvent>();
                        var5.Add(eachKeyValuePair.Value.Item2);
                        var4.Add(ParentID, new mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>(0, var5));
                    }
                }
                List<mTuple<string, int>> PossibleCalEvents = new System.Collections.Generic.List<mTuple<string, int>>();
                foreach (KeyValuePair<string, int> eachKeyValuePair in var2)
                {
                    PossibleCalEvents.Add(new mTuple<string, int>(eachKeyValuePair.Key, eachKeyValuePair.Value));
                }

                List<List<string>> var3 = generateCombinationForSpecficTimeSpanStringID(eachKeyValuePair0.Value.Item1,PossibleCalEvents);
                MAtrixedSet.Add(var3);
            }

            List<List<string>> serializedList = Utility.SerializeList(MAtrixedSet);
            foreach (List<string> eachList in serializedList)//serializedList has a list of fittable ParentIDs, the loop replaces each List of strings with List of subCalendarEvents
            {
                List<SubCalendarEvent> var6 = new System.Collections.Generic.List<SubCalendarEvent>();
                mTuple<int, List<SubCalendarEvent>> var7 = new mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>(0,new System.Collections.Generic.List<SubCalendarEvent>());
                foreach (string eachString in eachList)
                {
                    var7 = var4[eachString];
                    var6.Add(var7.Item2[var7.Item1++]);
                }
                foreach (KeyValuePair<string, mTuple<int, List<SubCalendarEvent>>> eachKeyValuePair in var4)
                {
                    eachKeyValuePair.Value.Item1 = 0;
                }
                
                //var7.Item1 = 0;

                retValue.Add(var6);
            }

            return retValue;

        }

        List<List<string>> generateCombinationForSpecficTimeSpanStringID(int Count, List<mTuple<string,int>> PossibleCalEvents)
        {
            int CountCpy = Count;
            
            int i = 0;
            List<List<string>> retValue = new System.Collections.Generic.List<System.Collections.Generic.List<string>>();
            if (Count == 0)
            {
                return retValue;
            }
            for (; i < PossibleCalEvents.Count; i++)
            { 
                
                List<mTuple<string,int>>  PossibleCalEvents_Param=PossibleCalEvents.ToList();
                 PossibleCalEvents_Param[i]=new mTuple<string, int>(PossibleCalEvents_Param[i].Item1, PossibleCalEvents_Param[i].Item2);
                 mTuple<string, int> refSubCalEventUmbrella = PossibleCalEvents_Param[i];
                //refSubCalEventUmbrella = 
                
                if (refSubCalEventUmbrella.Item2 > 0)
                {
                    --refSubCalEventUmbrella.Item2;

                    List<List<string>> receivedCombination = generateCombinationForSpecficTimeSpanStringID(CountCpy - 1, PossibleCalEvents_Param);
                    foreach (List<string> eachList in receivedCombination)
                    {
                        eachList.Add(refSubCalEventUmbrella.Item1);
                    }
                    if (receivedCombination.Count < 1)
                    {
                        receivedCombination.Add(new System.Collections.Generic.List<string>() { refSubCalEventUmbrella.Item1 });
                    }
                    retValue.AddRange(receivedCombination);
                }
                PossibleCalEvents.RemoveAt(i);
                --i;
            }
            return retValue;


        }
        

        Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> stitchRestrictedSubCalendarEvent(List<TimeLine> AllTimeLines, int TimeLineIndex, Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> arg1)
        {
            /*
             * arg1 is adictionary that has a timeline witha a list of restricted Sub calendar events             
             */

            Dictionary<string, bool> var1 = new System.Collections.Generic.Dictionary<string, bool>();

            foreach (TimeLine eachTimeLine in AllTimeLines)
            {
                List<mTuple<bool, SubCalendarEvent>> arg2 = arg1[eachTimeLine];
                List<SubCalendarEvent> arg3 = new System.Collections.Generic.List<SubCalendarEvent>();
                foreach(mTuple<bool, SubCalendarEvent> eachmTuple in arg2)
                {
                    arg3.Add(eachmTuple.Item2);
                    var1.Add(eachmTuple.Item2.ID, eachmTuple.Item1);
                }
                

                List<List<SubCalendarEvent>> arg4= Pseudo_generateTreeCallsToSnugArray(arg3, eachTimeLine);

                List<SubCalendarEvent> OPtimizedList = getOPtimized(arg4);

                List<mTuple<bool, SubCalendarEvent>> var5 = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
                foreach (SubCalendarEvent eachSubCalendarEvent in OPtimizedList)
                { 
                    var5.Add(new mTuple<bool,SubCalendarEvent>(var1[eachSubCalendarEvent.ID],eachSubCalendarEvent));
                }
                arg1[eachTimeLine]=var5;
            }


            return arg1;
        }


        List<SubCalendarEvent> getOPtimized(List<List<SubCalendarEvent>> Arg1)
        {
            List<SubCalendarEvent> retValue = new System.Collections.Generic.List<SubCalendarEvent>();

            if (Arg1.Count > 0)
            {
                return Arg1[0];
            }
            return retValue;
        }
        

        List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> getAveragedOutTIimeLine(List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> arg1, int PreecdintTimeSpanWithSringIDCoun)
        {
            /*
             * Function takes a list of valid possible matches. It uses this list of valid matches to calculate an average which will be used to calculate the best snug possibility
             * arg1= The List of possible snug time Lines
             */

            List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> retValue = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>();
            List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> Total = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>();
            
            foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in arg1[0])//initializes Total with the first element in the list
            {
                Total.Add(new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>());
            }



            int i = 0;
            int j = 0;

            for (; j < arg1.Count; j++)
            {
                List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> eachList = arg1[j];
                if (j == 30)
                {
                    ;
                }
                i = 0;
                foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in eachList)
                {

                    foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in eachDictionary)
                    {
                        if (Total[i].ContainsKey(eachKeyValuePair.Key))
                        {
                            Total[i][eachKeyValuePair.Key].Item1 += eachKeyValuePair.Value.Item1;
                        }
                        else
                        {
                            Total[i].Add(eachKeyValuePair.Key, new mTuple<int, TimeSpanWithStringID>(eachKeyValuePair.Value.Item1, eachKeyValuePair.Value.Item2));
                        }
                    }
                    ++i;
                }
            }

            Dictionary<string, int> RoundUpRoundDown = new System.Collections.Generic.Dictionary<string, int>();
            i = 0;
            foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in Total)//initializes Total with the first element in the list
            {

                retValue.Add(new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>());
                foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in eachDictionary)
                {
                    double arg0 = eachKeyValuePair.Value.Item1 / j;
                    int PassedToRet =(int)Math.Round(arg0);
                    string[] Int_Decimal = arg0.ToString().Split('.');
                    if (Int_Decimal.Length == 2)
                    {
                        if (Int_Decimal[1] == "5")
                        {
                            if (RoundUpRoundDown.ContainsKey(eachKeyValuePair.Key))
                            {
                                if (RoundUpRoundDown[eachKeyValuePair.Key] == 0)
                                {
                                    RoundUpRoundDown[eachKeyValuePair.Key]=1;
                                }
                                else
                                {
                                    PassedToRet = (int)Math.Ceiling(arg0);
                                    RoundUpRoundDown[eachKeyValuePair.Key] = 0;
                                }
                            }
                            else
                            {
                                PassedToRet = (int)Math.Ceiling(arg0);
                                RoundUpRoundDown.Add(eachKeyValuePair.Key, 0);
                            }
                        }
                    }

                    if (PassedToRet > 0)
                    {
                        retValue[i].Add(eachKeyValuePair.Key, new mTuple<int, TimeSpanWithStringID>(PassedToRet, eachKeyValuePair.Value.Item2));
                    }
                }
                i++;
            }


            List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> ListOfPossibleretValue = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();


            i = 0;
            foreach(List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> eachList in arg1)
            {
                i = 0;
                bool arg3 = false;
                foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in retValue)//initializes Total with the first element in the list
                {
                    Dictionary<string, mTuple<int, TimeSpanWithStringID>> arg2 = eachList[i];
                    foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in eachDictionary)
                    {
                        if (arg2.ContainsKey(eachKeyValuePair.Key))
                        {
                            if (eachKeyValuePair.Value.Item1 <= arg2[eachKeyValuePair.Key].Item1)
                            {
                                arg3 = true;
                            }
                            else
                            {
                                arg3 = false;
                                break;
                            }
                        }
                    }

                    if (!arg3)
                    {
                        break;
                    }
                    i++;
                }

                if (arg3)
                {
                    ListOfPossibleretValue.Add(eachList);
                }


            }

            ListOfPossibleretValue = getMostSpreadOut(ListOfPossibleretValue);




            List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> TempList = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string,mTuple<int,TimeSpanWithStringID>>>>();
            TempList.Add(retValue);

            Tuple<int, List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>> arg4 = Utility.getHighestCountList(TempList, retValue);

            if (arg4.Item1 <= PreecdintTimeSpanWithSringIDCoun)
            {
                return retValue;
            }

            else 
            {
                return getAveragedOutTIimeLine(ListOfPossibleretValue, arg4.Item1);
            }

                        
            
        }


        List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> getMostSpreadOut(List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> arg1)
        {
            List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> retValue = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();
            int i = 0;

            int MaxCount = 0;
            foreach (List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> eachList in arg1)
            {
                i = 0;
                int CurrenCount = 0;

                foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in eachList)//initializes Total with the first element in the list
                {
                    CurrenCount += eachDictionary.Count;
                }

                if (CurrenCount >= MaxCount)
                {
                    if (CurrenCount > MaxCount)
                    {
                        retValue = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();
                    }
                    retValue.Add(eachList);
                }
               
            }
            return retValue;
        }


        List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> getValidMatches(List<SubCalendarEvent>ListOfInterferringEvents,List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> PossibleMatches, Dictionary<TimeLine, Dictionary<string, mTuple<int, TimeSpanWithStringID>>> ConstrainedList)
        {
            int MaxPossibleMatched=ListOfInterferringEvents.Count;
            int HighstSum = 0;
            List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> retValue = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();
            List<TimeLine> AllTimeLines = ConstrainedList.Keys.ToList();
            List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> ListOfDict = ConstrainedList.Values.ToList();

            int j = 0;
            foreach (List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> eachList in PossibleMatches)
            {
                int CurrentSum = 0;
                int i = 0;
                
                foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in eachList)
                {
                    ICollection<KeyValuePair<string, mTuple<int, TimeSpanWithStringID>>> AllData = eachDictionary;
                    foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in AllData)
                    {
                        int evpItem1 = 0;
                        if (ListOfDict[i].ContainsKey(eachKeyValuePair.Key))
                        {

                            int evpItem2 = ListOfDict[i][eachKeyValuePair.Key].Item1;
                            evpItem1 = eachKeyValuePair.Value.Item1 + evpItem2;
                        }
                        else 
                        {
                            evpItem1 = eachKeyValuePair.Value.Item1;
                        }

                        evpItem1 = eachKeyValuePair.Value.Item1;
                        CurrentSum += evpItem1;
                    }
                    ++i;
                }

                if (CurrentSum >= HighstSum)
                {
                    if (CurrentSum > HighstSum)
                    {
                        retValue = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();
                        //retValue.Add(eachList);
                    }
                    retValue.Add(eachList);
                    HighstSum = CurrentSum;
                }
                j++;
            }
            return retValue;
        }



        Tuple<List<TimeSpanWithStringID>, List<mTuple<bool, SubCalendarEvent>>> ConvertSubCalendarEventToTimeSpanWitStringID(List<SubCalendarEvent> Arg1)
        {
            List<TimeSpanWithStringID> retValue = new System.Collections.Generic.List<TimeSpanWithStringID>();
            List<mTuple<bool,SubCalendarEvent>> retValue0 = new System.Collections.Generic.List<mTuple<bool,SubCalendarEvent>>();
            foreach (SubCalendarEvent eachSubCalendarEvent in Arg1)
            { 
                retValue.Add(new TimeSpanWithStringID(eachSubCalendarEvent.ActiveDuration, eachSubCalendarEvent.ActiveDuration.Ticks.ToString()));
                retValue0.Add(new mTuple<bool, SubCalendarEvent>(false, eachSubCalendarEvent));
            }

            return new Tuple<List<TimeSpanWithStringID>, List<mTuple<bool, SubCalendarEvent>>>(retValue, retValue0);
        }


        int HighestSum = 0;
        List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> generateTreeCallsToSnugArray(Dictionary<string, mTuple<int, TimeSpanWithStringID>> AvailableSubCalendarEvents, List<TimeLine> AllTimeLines, int TimeLineIndex, List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar, Dictionary<TimeLine, Dictionary<string, mTuple<int, TimeSpanWithStringID>>> DictionaryOfTimelineAndSubcalendarEvents, Dictionary<TimeLine, Tuple<Dictionary<string, mTuple<int, TimeSpanWithStringID>>, Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> DictionaryOfTimelineAndConstrainedElements)//, List<SubCalendarEvent> usedSubCalendarEvensts)
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

            //if (ListOfAllSnugPossibilitiesInRespectiveTImeLines_hack.Count >= maxHackConstant)
            {
                //return ListOfAllSnugPossibilitiesInRespectiveTImeLines_hack;
            }





            if (AvailableSubCalendarEvents.Count > MaxNumberOfInterferringSubcalEvents)
            {
                MaxNumberOfInterferringSubcalEvents = AvailableSubCalendarEvents.Count;
            }
            if (TimeLineIndex > LargestTimeIndex)
            {
                LargestTimeIndex = TimeLineIndex;
            }

            int indexChecker = 14;
            /*if (TimeLineIndex == indexChecker)
            {
                mTuple<bool, List<TimeSpanWithStringID>> statusOfError=Utility.isViolatingTimeSpanString(InterferringTimeSpanWithStringID_Cpy, SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar);

                if (statusOfError.Item1 )
                {
                    ;
                }
            }*/

            List<List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> ListOfAllSnugPossibilitiesInRespectiveTImeLines = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();
            if (AllTimeLines.Count < 1)
            {
                return null;
            }

            if ((TimeLineIndex >= AllTimeLines.Count) && (AvailableSubCalendarEvents.Count > 0))
            {
                LogInfo += ("Couldnt Find For this timeLine timeLine\n");
            }

            if ((TimeLineIndex >= AllTimeLines.Count) || (AvailableSubCalendarEvents.Count < 1))
            {
                if (TimeLineIndex < AllTimeLines.Count)
                {
                    LogInfo += ("Weird Exit\n");
                }

                if (AvailableSubCalendarEvents.Count < 1)
                {
                    ;
                }





                ListOfAllSnugPossibilitiesInRespectiveTImeLines.Add(SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar);
                return ListOfAllSnugPossibilitiesInRespectiveTImeLines;
            }


            int TOtalEvents = 0;

            

            if ((TimeLineIndex == 13) || (TimeLineIndex == 13))
            {

                //foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar)
                {
                    foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in AvailableSubCalendarEvents)
                    {
                        TOtalEvents += eachKeyValuePair.Value.Item1;
                    }
                }
                if (TOtalEvents == 1)
                {
                    ;
                }
            }

            Dictionary<string, mTuple<int, TimeSpanWithStringID>> MyPertinentSubcalendarEvents;
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> CantBeUsedInCurrentTimeLine = new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>();
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> RestrictedToThisTimeLineElements = DictionaryOfTimelineAndConstrainedElements[AllTimeLines[TimeLineIndex]].Item1;
           
            

            TimeSpan TotalTimeUsedUpByConstrained = new TimeSpan(0);
            
            foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in RestrictedToThisTimeLineElements)//HACK THIS does not take into account two clashing restricted events
            {
                int CantBeUsedCount = DictionaryOfTimelineAndConstrainedElements[AllTimeLines[TimeLineIndex]].Item2[eachKeyValuePair.Key].Item1 - RestrictedToThisTimeLineElements[eachKeyValuePair.Key].Item1;

                CantBeUsedInCurrentTimeLine.Add(eachKeyValuePair.Key, new mTuple<int, TimeSpanWithStringID>(CantBeUsedCount, eachKeyValuePair.Value.Item2));
                AvailableSubCalendarEvents[eachKeyValuePair.Key].Item1 -= CantBeUsedCount;
                //AvailableSubCalendarEvents[eachKeyValuePair.Key].Item1 -= RestrictedToThisTimeLineElements[eachKeyValuePair.Key].Item1;
                //DictionaryOfTimelineAndSubcalendarEvents[AllTimeLines[TimeLineIndex]][eachKeyValuePair.Key].Item1 -= RestrictedToThisTimeLineElements[eachKeyValuePair.Key].Item1;
                
                
                //AvailableSubCalendarEvents[eachKeyValuePair.Key].Item1 -= AvailableSubCalendarEventsCountReduction;


                ;
                
            }

            MyPertinentSubcalendarEvents = Utility.ListIntersection(AvailableSubCalendarEvents, DictionaryOfTimelineAndSubcalendarEvents[AllTimeLines[TimeLineIndex]]);
            foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in RestrictedToThisTimeLineElements)
            {

                MyPertinentSubcalendarEvents[eachKeyValuePair.Key].Item1 -= eachKeyValuePair.Value.Item1;
                TotalTimeUsedUpByConstrained += (TimeSpan.FromTicks(eachKeyValuePair.Value.Item2.timeSpan.Ticks * eachKeyValuePair.Value.Item1));
            }
            List<mTuple<int, TimeSpanWithStringID>> ListOfTimeSpanWithID_WithCounts = MyPertinentSubcalendarEvents.Values.ToList();


            SnugArray MySnugArray = new SnugArray(ListOfTimeSpanWithID_WithCounts, AllTimeLines[TimeLineIndex].TimelineSpan - TotalTimeUsedUpByConstrained);
            //SnugArray MySnugArray = new SnugArray(ConstrainedMySubcalendarEventTimespans, MySubcalendarEventTimespans, AllTimeLines[TimeLineIndex].TimelineSpan);



            List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> AllSnugPossibilities = MySnugArray.MySnugPossibleEntries;

            foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> eachDictionary in AllSnugPossibilities)
            {
                /*if ((TimeLineIndex ==5)||(TimeLineIndex ==9))
                {
                    if (eachDictionary.ContainsKey("90000000000"))
                    {
                        if (eachDictionary["90000000000"].Item1 >= 2)
                        {
                            ;
                        }
                    }
                }*/
                
                foreach (string eachString in RestrictedToThisTimeLineElements.Keys)
                {
                    if (eachDictionary.ContainsKey(eachString))
                    {
                        eachDictionary[eachString].Item1 += RestrictedToThisTimeLineElements[eachString].Item1;
                    }
                    else 
                    {
                        eachDictionary.Add(eachString, new mTuple<int, TimeSpanWithStringID>(RestrictedToThisTimeLineElements[eachString].Item1, RestrictedToThisTimeLineElements[eachString].Item2));
                    }
                }
            }



            foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in CantBeUsedInCurrentTimeLine)//restores restricted for other timeLines into AvailableSubCalendarEvents
            {
                AvailableSubCalendarEvents[eachKeyValuePair.Key].Item1 += eachKeyValuePair.Value.Item1;
            }


            List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>> SerialIzedListOfSubCalendarEvents = AllSnugPossibilities;
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> AvailableSubCalendarEventsAfterRemovingAssingedSnugElements = new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>();

            



            
            if (SerialIzedListOfSubCalendarEvents.Count > 0)
            {

                foreach (Dictionary<string, mTuple<int, TimeSpanWithStringID>> AlreadyAssignedSubCalendarEvent in AllSnugPossibilities)
                {
                    AvailableSubCalendarEventsAfterRemovingAssingedSnugElements = Utility.NotInList_NoEffect(AvailableSubCalendarEvents, AlreadyAssignedSubCalendarEvent);

                    

                    SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar[TimeLineIndex] = AlreadyAssignedSubCalendarEvent;
                    /*if (checkSumOfTimeEvent(SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar) >= MaxNumberOfInterferringSubcalEvents)
                    {
                        ListOfAllSnugPossibilitiesInRespectiveTImeLines_hack.Add(SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar);
                        //if (ListOfAllSnugPossibilitiesInRespectiveTImeLines_hack.Count >= maxHackConstant)
                        {
                            //return ListOfAllSnugPossibilitiesInRespectiveTImeLines_hack;
                        }
                    }*/

                    ListOfAllSnugPossibilitiesInRespectiveTImeLines.AddRange(generateTreeCallsToSnugArray(AvailableSubCalendarEventsAfterRemovingAssingedSnugElements, AllTimeLines, TimeLineIndex + 1, SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar.ToList(), DictionaryOfTimelineAndSubcalendarEvents, DictionaryOfTimelineAndConstrainedElements));
                    //ListOfAllSnugPossibilitiesInRespectiveTImeLines = MyHolder.ToList();
                }
            }
            else
            {
                AvailableSubCalendarEventsAfterRemovingAssingedSnugElements = Utility.NotInList_NoEffect(AvailableSubCalendarEvents, new Dictionary<string, mTuple<int, TimeSpanWithStringID>>());//Hack this can be optimized... the whole "notinlist" call can be optimized as a call to AvailableSubCalendarEvents. Review to see if references to func are affected.
                ListOfAllSnugPossibilitiesInRespectiveTImeLines.AddRange(generateTreeCallsToSnugArray(AvailableSubCalendarEventsAfterRemovingAssingedSnugElements, AllTimeLines, TimeLineIndex + 1, SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar.ToList(), DictionaryOfTimelineAndSubcalendarEvents, DictionaryOfTimelineAndConstrainedElements));
            }


            

            return ListOfAllSnugPossibilitiesInRespectiveTImeLines;
        }


        List<List<List<SubCalendarEvent>>> FixSubCalEventOrder(List<List<List<SubCalendarEvent>>> AllPossibleFullTimeLines, TimeLine[] JustFreeSpots)
        {
            List<List<List<SubCalendarEvent>>> retValue = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();
            
            foreach (List<List<SubCalendarEvent>> PossibleTimeLine in AllPossibleFullTimeLines)
            {
                int index = 0;
                

                Dictionary<int, List<List<SubCalendarEvent>>> DictOfSubCalEvent = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();
                for (; index < JustFreeSpots.Length; index++)
                {
                    DictOfSubCalEvent.Add(index, new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>());
                    List<SubCalendarEvent> SubCalendarEventsInFreeTimeLine = PossibleTimeLine[index];
                    SubCalendarEventsInFreeTimeLine = SubCalendarEventsInFreeTimeLine.OrderBy(obj => obj.getCalendarEventRange.End).ToList();
                    List<List<SubCalendarEvent>> latestPermutations = Pseudo_generateTreeCallsToSnugArray(SubCalendarEventsInFreeTimeLine, JustFreeSpots[index]);
                    
                    DictOfSubCalEvent[index].AddRange(latestPermutations);
                }

                retValue.AddRange(SerializeDictionary(DictOfSubCalEvent));
                
            }

            retValue.RemoveRange(0, Convert.ToInt32(retValue.Count * 0.9999));

            return retValue;
        }



        List<List<List<SubCalendarEvent>>> SerializeDictionary(Dictionary<int, List<List<SubCalendarEvent>>> DictOfSubCalEvent)
        {
            List<List<List<SubCalendarEvent>>> retValue = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();
//            List<List<List<SubCalendarEvent>>> WorkWihList  = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();
            
            for(int i=0; i<DictOfSubCalEvent.Keys.Count;i++)//Goes through each dictionary index
            {
                List<List<List<SubCalendarEvent>>> NonTaintedListToWorkWith = retValue.ToList();
                List<List<List<SubCalendarEvent>>> MyTempList = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();
                foreach (List<SubCalendarEvent> ListOfClumpPermutation in DictOfSubCalEvent[i])
                {
                    List<List<List<SubCalendarEvent>>> TaintedListToWorkWith= NonTaintedListToWorkWith.ToList();
                    foreach(List<List<SubCalendarEvent>> ListSoFar in TaintedListToWorkWith)
                    {
                        List<List<SubCalendarEvent>> ListSoFar_cpy = ListSoFar.ToList();
                        ListSoFar_cpy.Add(ListOfClumpPermutation);
                        MyTempList.Add(ListSoFar_cpy);
                    }
                    if (TaintedListToWorkWith.Count < 1)
                    {
                        List<List<SubCalendarEvent>> ListSoFar = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
                        ListSoFar.Add(ListOfClumpPermutation);
                        MyTempList.Add(ListSoFar);

                    }
                }
                if (DictOfSubCalEvent[i].Count > 0)
                {
                    retValue = MyTempList;
                }
                else 
                {
                    foreach (List<List<SubCalendarEvent>> AvailList in retValue)
                    {
                        AvailList.Add(new List<SubCalendarEvent>());
                    }
                }
                
            }

            return retValue;
        }

        List<List<SubCalendarEvent>> Pseudo_generateTreeCallsToSnugArray(List<SubCalendarEvent> SortedAvailableSubCalEvents_Deadline,TimeLine BoundaryTimeLine)//, Dictionary<TimeLine, List<SubCalendarEvent>> DictionaryOfTimelineAndSubcalendarEvents)//, List<SubCalendarEvent> usedSubCalendarEvensts)
        {

            List<List<SubCalendarEvent>> RetValue = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
            if (SortedAvailableSubCalEvents_Deadline.Count < 1)
            {
                return RetValue;
               // throw new Exception("Check your Stack Calls to Pseudo_generateTreeCallsToSnugArray. Theres an error you are passing empty SortedAvailableSubCalEvents_Deadline");
            }
            
            
            ClumpSubCalendarEvent ClumpedSubEvents = new ClumpSubCalendarEvent(SortedAvailableSubCalEvents_Deadline.ToList(), BoundaryTimeLine);
            List<List<SubCalendarEvent>> ClumpendListOfSubCalEvetns = ClumpedSubEvents.GenerateList(0);
            ClumpSubCalendarEvent.Completed = 0;
            foreach (List<SubCalendarEvent>AlreadyAssignedSubCalEvents in ClumpendListOfSubCalEvetns)
            {
                Utility.PinSubEventsToEnd(AlreadyAssignedSubCalEvents.ToList(), BoundaryTimeLine);
                //Utility.PinSubEventsToStart(AlreadyAssignedSubCalEvents.ToList(), BoundaryTimeLine);
                
                /*TimeLine UpdatedBoundary = new TimeLine(AlreadyAssignedSubCalEvents[AlreadyAssignedSubCalEvents.Count - 1].End, BoundaryTimeLine.End);
                List<SubCalendarEvent> SubCalEventsLeft=Utility.NotInList_NoEffect(SortedAvailableSubCalEvents_Deadline, AlreadyAssignedSubCalEvents);
                List<List<SubCalendarEvent>> FurtherClumpedList = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
                if (SubCalEventsLeft.Count > 0)
                {
                    FurtherClumpedList = Pseudo_generateTreeCallsToSnugArray(SubCalEventsLeft, UpdatedBoundary);
                }
                if (FurtherClumpedList.Count > 0)
                {
                    foreach (List<SubCalendarEvent> UpdatedClumpList in FurtherClumpedList)
                    {
                        UpdatedClumpList.AddRange(AlreadyAssignedSubCalEvents);

                    }
                }
                else
                {
                    FurtherClumpedList = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
                    FurtherClumpedList.Add(new System.Collections.Generic.List<SubCalendarEvent>());
                    FurtherClumpedList[0].AddRange(AlreadyAssignedSubCalEvents);
                }

                
                RetValue.AddRange(FurtherClumpedList);*/
                RetValue.Add(AlreadyAssignedSubCalEvents);
            }


            return RetValue;
            
        }


        

        List<TimeLine> ReOrganizeTimeLine(List<TimeLine> InEfficientTimeLine, int Index, TimeSpan SpaceReadjuster,Dictionary<SubCalendarEvent,TimeLine> RelativeSubcalendarList)//Not yet Debugged
        {
            /*
             * This is called after the right index poistion has been determined. The only pertinent pseudo rigid timelines have been formatted with the right index
             
             * */            
            if (Index == (InEfficientTimeLine.Count - 1))
            {
                return InEfficientTimeLine;
            }



            List<SubCalendarEvent> RelativeSubCalendarEvent_list = RelativeSubcalendarList.Keys.ToList();//assumes that each Sub calendar Event is setup to stay in the lates possible scenario within the timeline in RelativeSubcalendarList dictionary. There is also only one Subcalendar event per dictionary.
            
            RelativeSubCalendarEvent_list = RelativeSubCalendarEvent_list.OrderBy(obj => obj.End).ToList();
            TimeSpan TotalUsedUpSpan = InEfficientTimeLine[Index].TotalActiveSpan;
            BusyTimeLine[] CurrenlyOccupied= InEfficientTimeLine[Index].OccupiedSlots;
            TimeLine[] AllFreeSpot = Schedule.getAllFreeSpots_NoCompleteSchedule(InEfficientTimeLine[Index]);

            TimeSpan Excess = new TimeSpan(0);
            if (AllFreeSpot.Length > 0)//this makes assumption that you are clumping up the subevents within the timeline so we only have one free timeLine Available
            {
                Excess = AllFreeSpot[AllFreeSpot.Length - 1].TimelineSpan;
            }

            SubCalendarEvent Pseudo_rigidSubcalEvent = RelativeSubCalendarEvent_list[Index];
            TimeLine PertinentTimeLine = RelativeSubcalendarList[Pseudo_rigidSubcalEvent];
            TimeSpan PossibleLeftMovement = Pseudo_rigidSubcalEvent.Start - PertinentTimeLine.Start;
            if (PossibleLeftMovement >= Excess)
            {
                Pseudo_rigidSubcalEvent = new SubCalendarEvent(Pseudo_rigidSubcalEvent.ID, Pseudo_rigidSubcalEvent.Start.Add(-Excess), Pseudo_rigidSubcalEvent.End.Add(-Excess), new BusyTimeLine(Pseudo_rigidSubcalEvent.ID, Pseudo_rigidSubcalEvent.Start.Add(-Excess), Pseudo_rigidSubcalEvent.End.Add(-Excess)), Pseudo_rigidSubcalEvent.myLocation, Pseudo_rigidSubcalEvent.getCalendarEventRange);// this can be optimized with a shift function
            }
            else
            {
                Pseudo_rigidSubcalEvent = new SubCalendarEvent(Pseudo_rigidSubcalEvent.ID, Pseudo_rigidSubcalEvent.Start.Add(-PossibleLeftMovement), Pseudo_rigidSubcalEvent.End.Add(-PossibleLeftMovement), new BusyTimeLine(Pseudo_rigidSubcalEvent.ID, Pseudo_rigidSubcalEvent.Start.Add(-PossibleLeftMovement), Pseudo_rigidSubcalEvent.End.Add(-PossibleLeftMovement)), Pseudo_rigidSubcalEvent.myLocation, Pseudo_rigidSubcalEvent.getCalendarEventRange);// this can be optimized with a shift function
            }
            RelativeSubcalendarList.Add(Pseudo_rigidSubcalEvent, PertinentTimeLine);
            DateTime LastDateTime= (InEfficientTimeLine[Index].OccupiedSlots.OrderBy(obj=>obj.End).ToList())[InEfficientTimeLine.Count-1].End;
            RelativeSubcalendarList.Remove(RelativeSubCalendarEvent_list[Index]);
            InEfficientTimeLine[Index] = new TimeLine(InEfficientTimeLine[Index].Start, LastDateTime);
            InEfficientTimeLine[Index].OccupiedSlots = CurrenlyOccupied;//check for bugs to ensure not out of timeLine Range;You also need to ensure that Pseudo_rigidSubcalEvent gets perpetuated to what ever func will be using it later
            CurrenlyOccupied = InEfficientTimeLine[Index + 1].OccupiedSlots;
            InEfficientTimeLine[Index + 1] = new TimeLine(Pseudo_rigidSubcalEvent.End, InEfficientTimeLine[Index + 1].End);
            InEfficientTimeLine[Index + 1].OccupiedSlots = CurrenlyOccupied;


            return InEfficientTimeLine;

        }


        List<CalendarEvent> getClashingCalendarEvent(List<CalendarEvent> MyListOfCalednarEvents, TimeLine MyTImeLine)
        {
            List<CalendarEvent> MyInterferringCaliendarEvents=getPertinentCalendarEvents(MyListOfCalednarEvents.ToArray(), MyTImeLine).ToList();
            List<CalendarEvent> ListOfCalendarEventsLargerThanTimeLine = new System.Collections.Generic.List<CalendarEvent>();
            
            foreach (CalendarEvent MyCalendarEvent in MyListOfCalednarEvents)
            {
                if (((MyCalendarEvent.End > MyTImeLine.Start) && (MyCalendarEvent.Start <= MyTImeLine.Start))||( (MyTImeLine.Start>MyCalendarEvent.Start)&&(MyTImeLine.Start<=MyCalendarEvent.Start)))
                {
                    MyInterferringCaliendarEvents.Add(MyCalendarEvent);
                }
            }

            return MyInterferringCaliendarEvents;
        }


         mTuple<bool, SubCalendarEvent> getmTupleSubCalendarEvent(List<mTuple<bool, SubCalendarEvent>> MyListOfInterferringmTupleSubCalendarEvents, SubCalendarEvent Arg1)
        {
            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in MyListOfInterferringmTupleSubCalendarEvents)
            {
                if (eachmTuple.Item2 == Arg1)
                {
                    return eachmTuple;
                }
            }

            return null;
        }


         List<mTuple<bool, SubCalendarEvent>> getmTupleSubCalendarEvent(List<mTuple<bool, SubCalendarEvent>> MyListOfInterferringmTupleSubCalendarEvents, List<SubCalendarEvent> Arg1)
         {
             List<mTuple<bool, SubCalendarEvent>> retValue = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
             int i = 0;
             foreach (mTuple<bool, SubCalendarEvent> eachmTuple in MyListOfInterferringmTupleSubCalendarEvents)
             {
                 int Arg2=Arg1.IndexOf(eachmTuple.Item2);
                 if (Arg2 > -1)
                 {
                     retValue.Add(eachmTuple);
                     if (++i == Arg1.Count)
                     {
                         return retValue;
                     }
                 }
             }

             return null;
         }

        private Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> BuildDicitionaryOfTimeLineAndSubcalendarEvents(List<mTuple<bool, SubCalendarEvent>> MyListOfInterferringmTupleSubCalendarEvents, Dictionary<TimeLine, Dictionary<CalendarEvent, List<SubCalendarEvent>>> DicitonaryTimeLineAndPertinentCalendarEventDictionary, CalendarEvent MyEvent)
        {
            
            
            List<TimeLine> MyListOfFreeTimelines = DicitonaryTimeLineAndPertinentCalendarEventDictionary.Keys.ToList();
            Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> DictionaryofTimeLineAndPertinentSubcalendar = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();
            

            foreach (TimeLine MyTimeLine in MyListOfFreeTimelines)
            {
                //Dictionary<TimeLine, Dictionary<CalendarEvent, List<SubCalendarEvent>>> DicitonaryTimeLineAndPertinentCalendarEventDictionary1;
                
                Dictionary<CalendarEvent,List<SubCalendarEvent>> MyListOfDictionaryOfCalendarEventAndSubCalendarEvent=DicitonaryTimeLineAndPertinentCalendarEventDictionary[MyTimeLine];
                List<CalendarEvent> MyListOfPertitnentCalendars = MyListOfDictionaryOfCalendarEventAndSubCalendarEvent.Keys.ToList();
                MyListOfPertitnentCalendars = MyListOfPertitnentCalendars.OrderBy(obj => obj.End).ToList();
                List<mTuple<bool, SubCalendarEvent>>  MyListOfPertinentSubEvent = new System.Collections.Generic.List<mTuple<bool,SubCalendarEvent>>();
                foreach (CalendarEvent MyCalendarEvent in MyListOfPertitnentCalendars)
                {
                    List<SubCalendarEvent> InterFerringSubCalendarEventS = MyListOfDictionaryOfCalendarEventAndSubCalendarEvent[MyCalendarEvent];
                    if (MyCalendarEvent.Repeat.Enable)
                    {
                        List<CalendarEvent> MyListOfAffectedRepeatingCalendarEvent = getClashingCalendarEvent(MyCalendarEvent.Repeat.RecurringCalendarEvents.ToList(), MyTimeLine);



                        List<mTuple<bool, SubCalendarEvent>> ListOfAffectedSubcalendarEvents = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
                        foreach (CalendarEvent MyRepeatCalendarEvent in MyListOfAffectedRepeatingCalendarEvent)
                        {
                            SubCalendarEvent[] MyListOfSubCalendarEvents = MyRepeatCalendarEvent.AllEvents;
                            foreach (SubCalendarEvent PosibleClashingSubCalEvent in MyListOfSubCalendarEvents )
                            {
                                foreach(SubCalendarEvent eachInterFerringSubCalendarEvent in InterFerringSubCalendarEventS)
                                {
                                    if (PosibleClashingSubCalEvent.ID == eachInterFerringSubCalendarEvent.ID)
                                    {
                                        ListOfAffectedSubcalendarEvents.Add(getmTupleSubCalendarEvent(MyListOfInterferringmTupleSubCalendarEvents, eachInterFerringSubCalendarEvent));
                                    }
                                }
                            }
                        }
                        MyListOfPertinentSubEvent.AddRange(ListOfAffectedSubcalendarEvents);
                    }
                    else
                    {
                        MyListOfPertinentSubEvent.AddRange(getmTupleSubCalendarEvent(MyListOfInterferringmTupleSubCalendarEvents,MyListOfDictionaryOfCalendarEventAndSubCalendarEvent[MyCalendarEvent]));
                        
                    }
                }
                //var ConcatVar = MyListOfPertinentSubEvent.Concat(TempSubCalendarEventsForMyCalendarEvent.ToList());
                //MyListOfPertinentSubEvent = ConcatVar.ToList();
                DictionaryofTimeLineAndPertinentSubcalendar.Add(MyTimeLine, MyListOfPertinentSubEvent);
            }
            
            return DictionaryofTimeLineAndPertinentSubcalendar;

            /*foreach(TimeLine MyTimeLine in MyListOfFreeTimelines)
            {
                List<SubCalendarEvent> MyTimeLineListToWorkWith = getIntersectionList(MyInterferringSubCalendarEvents, DictionsryofTimeLineAndPertinentSubcalenda[MyTimeLine]);
                
            }
            */
            
            
        }

        int MaxNumberOfInterferringSubcalEvents = 0;
        int maxHackConstant = 2;
        List<List<List<SubCalendarEvent>>> ListOfAllSnugPossibilitiesInRespectiveTImeLines_hack = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();

        int LargestTimeIndex = -1;
       
        int checkSumOfTimeEvent(List<List<SubCalendarEvent>> MySubCalEventInTimeLine)//hack for optimization
        {
            int Sum=0;
            foreach (List<SubCalendarEvent> EachTimeLineSubeventList in MySubCalEventInTimeLine)
            {
                Sum += EachTimeLineSubeventList.Count;
            }
            return Sum;
        }

        List<List<SubCalendarEvent>> SerializeListOfMatchingSubcalendarEvents(List <List<List<SubCalendarEvent>>> MyListofListOfListOfSubcalendar)
        {

            int MyCounterBreaker = 0;
            List<List<SubCalendarEvent>> MyTotalCompilation = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
            foreach (List<List<SubCalendarEvent>> MyListOfList in MyListofListOfListOfSubcalendar)
            {

                //++MyCounterBreaker;
               // if (MyCounterBreaker > 200)//estimate how long it'll take to lose memory then optimize for that
                {
                    //break;
                }
                var MyHolder = MyTotalCompilation.Concat(SpreadOutList.GenerateListOfSubCalendarEvent(MyListOfList));
                MyTotalCompilation = MyHolder.ToList();
                //MyTotalCompilation.Add(SpreadOutList.GenerateListOfSubCalendarEvent(MyListOfList))
            }

            

            return MyTotalCompilation;
        }


        Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> generateConstrainedList(Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> Dict_TimeLine_ListSubCalEvents)
        {
            Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> retValue = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();
            List<List<mTuple<bool, SubCalendarEvent>>> AllAvailForTimeLine = Dict_TimeLine_ListSubCalEvents.Values.ToList();
            List<TimeLine> ListOfTimeLineKeys=Dict_TimeLine_ListSubCalEvents.Keys.ToList();
            int current = 0;
            int Next = current + 1;

            List<mTuple<bool, SubCalendarEvent>> UnUsables = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            List<mTuple<bool, SubCalendarEvent>> CurrentConstrained;

            for (; current < Dict_TimeLine_ListSubCalEvents.Count - 1;current++ )
            {
                Next = current + 1;


                CurrentConstrained = Utility.InListAButNotInB<mTuple<bool, SubCalendarEvent>>(Utility.NotInList_NoEffect<mTuple<bool, SubCalendarEvent>>(AllAvailForTimeLine[current], UnUsables), AllAvailForTimeLine[Next]);

                UnUsables.AddRange(Utility.NotInList_NoEffect<mTuple<bool, SubCalendarEvent>>(AllAvailForTimeLine[current], CurrentConstrained));

                retValue.Add(ListOfTimeLineKeys[current], CurrentConstrained);
            }

            CurrentConstrained = Utility.InListAButNotInB<mTuple<bool, SubCalendarEvent>>(Utility.NotInList_NoEffect<mTuple<bool, SubCalendarEvent>>(AllAvailForTimeLine[current], UnUsables), new List<mTuple<bool, SubCalendarEvent>>());


            retValue.Add(ListOfTimeLineKeys[current], CurrentConstrained);
            return retValue;
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



        
        
        
        
        
        
        
        List<List<SubCalendarEvent>> BuildListMatchingTimelineAndSubCalendarEvent(List<List<TimeSpanWithEventID>> ListOfSnugPossibilities, List<SubCalendarEvent> ListOfSubCalendarEvents,List<SubCalendarEvent>ConsrainedList)
        {
            List<List<SubCalendarEvent>> retValue = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
            Dictionary<string, SubCalendarEventListCounter> Dict_ParentIDListOfSubCalEvents = new System.Collections.Generic.Dictionary<string, SubCalendarEventListCounter>();
            foreach (SubCalendarEvent eachSubCalendarEvent in ListOfSubCalendarEvents)
            {
                string ParentKey = eachSubCalendarEvent.SubEvent_ID.getCalendarEventID();
                if (Dict_ParentIDListOfSubCalEvents.ContainsKey(ParentKey))
                {
                    Dict_ParentIDListOfSubCalEvents[ParentKey].UpdateList = eachSubCalendarEvent;
                }
                else
                {
                    Dict_ParentIDListOfSubCalEvents.Add(ParentKey, new SubCalendarEventListCounter(eachSubCalendarEvent, ParentKey));
                }
            }

            foreach (List<TimeSpanWithEventID> eachListOfTimeSpanWithID in ListOfSnugPossibilities)
            {
                List<SubCalendarEvent> CurentLine = new System.Collections.Generic.List<SubCalendarEvent>();
                CurentLine.AddRange(ConsrainedList);
                foreach (TimeSpanWithEventID eachTimeSpanWithID in eachListOfTimeSpanWithID)
                {
                    CurentLine.Add(Dict_ParentIDListOfSubCalEvents[eachTimeSpanWithID.TimeSpanID.ToString()].getNextSubCalendarEvent);
                }
                
                retValue.Add(CurentLine);
                foreach (SubCalendarEventListCounter eachSubCalendarEventListCounter in Dict_ParentIDListOfSubCalEvents.Values)
                {
                    eachSubCalendarEventListCounter.reset();
                }

            }

            return retValue;
            
            
            
            /*List<TimeSpan> AllTimesSpan = new List<TimeSpan>();
            Dictionary<TimeSpanWithID, List<SubCalendarEvent>> DictionaryOfTimeSpanWithIDandSubCalendarEvent = new System.Collections.Generic.Dictionary<TimeSpanWithID, System.Collections.Generic.List<SubCalendarEvent>>();
            List<List<SubCalendarEvent>> MatchingListOfSnugPossibilitesWithSubcalendarEvents = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
            Dictionary<string, List<SubCalendarEvent>> ListOfCaleventIDAndListSubCalendarEvent = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<SubCalendarEvent>>();

            
            //foreach (List<TimeSpan> MySnugPossibility in ListOfSnugPossibilities)
            //{
                //var MyConcatList = AllTimesSpan.Concat(MySnugPossibility);
              //  AllTimesSpan=MyConcatList.ToList();
            //}
            Dictionary <string ,int> DictCalendarEventID_Ini = new System.Collections.Generic.Dictionary<string,int>();
            foreach (SubCalendarEvent MySubCalendarEvent in ListOfSubCalendarEvents)
            { 
                EventID MyEventID = new EventID( MySubCalendarEvent.ID);
                if (ListOfCaleventIDAndListSubCalendarEvent.ContainsKey(MyEventID.getLevelID(0)))
                {
                    ListOfCaleventIDAndListSubCalendarEvent[MyEventID.getLevelID(0)].Add(MySubCalendarEvent);
                }
                else
                {
                    ListOfCaleventIDAndListSubCalendarEvent.Add(MyEventID.getLevelID(0),new System.Collections.Generic.List<SubCalendarEvent>());
                    ListOfCaleventIDAndListSubCalendarEvent[MyEventID.getLevelID(0)].Add(MySubCalendarEvent);
                    DictCalendarEventID_Ini.Add(MyEventID.getLevelID(0),0);
                }
            }


            List<SubCalendarEvent> MyListOfSubCalendarEvent = new System.Collections.Generic.List<SubCalendarEvent>();
            int Index = 0;
            Dictionary <string ,int> DictCalendarEventID_Index = new System.Collections.Generic.Dictionary<string,int>(DictCalendarEventID_Ini);
            foreach (List<TimeSpanWithID> MyListOfTimeSpanWithID in ListOfSnugPossibilities)
            {
                DictCalendarEventID_Index = new System.Collections.Generic.Dictionary<string,int>(DictCalendarEventID_Ini);
                foreach (TimeSpanWithID MyTimeSpanWithID in MyListOfTimeSpanWithID)
                {
                    string ID=MyTimeSpanWithID.TimeSpanID.getLevelID(0);
                    Index = DictCalendarEventID_Index[ID];
                    MyListOfSubCalendarEvent.Add(ListOfCaleventIDAndListSubCalendarEvent[ID][Index]);
                    ++Index;
                    DictCalendarEventID_Index[ID] = Index;
                }
                MatchingListOfSnugPossibilitesWithSubcalendarEvents.Add(MyListOfSubCalendarEvent);
                MyListOfSubCalendarEvent = new System.Collections.Generic.List<SubCalendarEvent>();
            }
            

            return MatchingListOfSnugPossibilitesWithSubcalendarEvents;*/
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
            Dictionary<TimeSpan, List<SubCalendarEvent>> DictionaryOfTImespanandSubCalendarEvent = new System.Collections.Generic.Dictionary<TimeSpan, System.Collections.Generic.List<SubCalendarEvent>>();
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
                    DictionaryOfTImespanandSubCalendarEvent[MySubCalendar.ActiveSlot.BusyTimeSpan].Add(MySubCalendar);
                }
                catch ( Exception e)
                {
                    
                    DictionaryOfTImespanandSubCalendarEvent.Add(MySubCalendar.ActiveSlot.BusyTimeSpan,new List<SubCalendarEvent>());
                    DictionaryOfTImespanandSubCalendarEvent[MySubCalendar.ActiveSlot.BusyTimeSpan].Add(MySubCalendar);
                    //DictionaryOfTImespanandSubCalendarEvent.Add(MyTimeSpan, new List<SubCalendarEvent>());
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


        List<BusyTimeLine>[] CategorizeTimeLine_noEventSchedule(TimeLine MyRange, BusyTimeLine[] SortedByStartArrayOfBusyTimeLine)//This  returns An array of a List of SubCalendarEvents. The function goes through each of the BUsytimeline Events and verifies if its parent calendar Event has a start Time that is eiter earlier than range, within range or start and ends within the range
        {
            List<BusyTimeLine>[] ArrayOfDifferentVaryingSubEventsCategories = new List<BusyTimeLine>[4];


            List<BusyTimeLine> MyArrayOfBeforeAndEndsAfterBusyTimelines = new List<BusyTimeLine>(0);


            List<BusyTimeLine> MyArrayOfBeforeAndEndsBeforeBusyTimelines = new List<BusyTimeLine>(0);


            List<BusyTimeLine> MyArrayOfStartsAfterAndEndsAfterBusyTimelines = new List<BusyTimeLine>(0);


            List<BusyTimeLine> MyArrayOfStartsAndEndsBeforeBusyTimelines = new List<BusyTimeLine>(0);
            
            int i = 0;
            for (; i < SortedByStartArrayOfBusyTimeLine.Length; i++)
            {
                EventID MyEventID = new EventID(SortedByStartArrayOfBusyTimeLine[i].TimeLineID);
                string ParentCalendarEventID = MyEventID.getLevelID(0);

                if ((MyRange.Start < SortedByStartArrayOfBusyTimeLine[i].Start) && (MyRange.End > SortedByStartArrayOfBusyTimeLine[i].End))//checks if Calendar Event Starts starts before range and ends after range
                {
                    MyArrayOfStartsAndEndsBeforeBusyTimelines.Add(SortedByStartArrayOfBusyTimeLine[i]);
                    
                }

                if ((MyRange.Start < SortedByStartArrayOfBusyTimeLine[i].Start) && (MyRange.End <= SortedByStartArrayOfBusyTimeLine[i].End))//checks if Calendar Event Starts starts before range and ends within range
                {
                    MyArrayOfStartsAfterAndEndsAfterBusyTimelines.Add(SortedByStartArrayOfBusyTimeLine[i]);
                                   
                }

                if ((MyRange.Start >= SortedByStartArrayOfBusyTimeLine[i].Start) && (MyRange.End > SortedByStartArrayOfBusyTimeLine[i].End))//checks if Calendar Event Starts within range and ends after range
                {
                    MyArrayOfBeforeAndEndsBeforeBusyTimelines.Add(SortedByStartArrayOfBusyTimeLine[i]);
                    
                }
                
                if ((MyRange.Start >= SortedByStartArrayOfBusyTimeLine[i].Start) && (MyRange.End <= SortedByStartArrayOfBusyTimeLine[i].End))//checks if Calendar Event Starts within range and ends within range
                {
                    MyArrayOfBeforeAndEndsAfterBusyTimelines.Add(SortedByStartArrayOfBusyTimeLine[i]);
                }

                
            }
            ArrayOfDifferentVaryingSubEventsCategories[0] = MyArrayOfBeforeAndEndsAfterBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[1] = MyArrayOfBeforeAndEndsBeforeBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[2] = MyArrayOfStartsAfterAndEndsAfterBusyTimelines;
            ArrayOfDifferentVaryingSubEventsCategories[3] = MyArrayOfStartsAndEndsBeforeBusyTimelines;

           
            return ArrayOfDifferentVaryingSubEventsCategories;
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
                
                i = 0;
                if (MyCalendarEventDictionaryEntry.Value.RepetitionStatus)
                {
                    lengthOfCalendarSubEvent = MyCalendarEventDictionaryEntry.Value.AllRepeatSubCalendarEvents.Length;
                    SubCalendarEvent[] ArrayOfSubcalendarEventsFromRepeatingEvents = MyCalendarEventDictionaryEntry.Value.AllRepeatSubCalendarEvents;


                    for (i=0; i < lengthOfCalendarSubEvent; i++)
                    {
                        if ((MyCalendarEvent.EventTimeLine.IsDateTimeWithin(ArrayOfSubcalendarEventsFromRepeatingEvents[i].Start)) || (MyCalendarEvent.EventTimeLine.IsDateTimeWithin(ArrayOfSubcalendarEventsFromRepeatingEvents[i].End)))
                        {
                            MyArrayOfInterferringSubCalendarEvents.Add(ArrayOfSubcalendarEventsFromRepeatingEvents[i]);
                        }
                    }
                }

                else 
                {
                    lengthOfCalendarSubEvent = MyCalendarEventDictionaryEntry.Value.AllEvents.Length;
                    
                    for (i=0; i < lengthOfCalendarSubEvent; i++)
                    {
                        if (MyCalendarEvent.RepetitionStatus)
                        {
                            throw new Exception("Weird error, found repeating event repeaing evemt");
                        }

                        if ((MyCalendarEvent.EventTimeLine.IsDateTimeWithin(MyCalendarEventDictionaryEntry.Value.AllEvents[i].Start)) || (MyCalendarEvent.EventTimeLine.IsDateTimeWithin(MyCalendarEventDictionaryEntry.Value.AllEvents[i].End)))
                        {
                            MyArrayOfInterferringSubCalendarEvents.Add(MyCalendarEventDictionaryEntry.Value.AllEvents[i]);
                        }
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
                return null;
            }

            return MyArrayOfToBeAssignedTimeLine;
        }

        public TimeLine[] IdealAllotAndInsert(Dictionary<TimeLine, int> Dict_AvailablFreeTimeLineAndFitCount, TimeLine[] MyArrayOfToBeAssignedTimeLine, TimeSpan IdealTimePerAllotment)
        {
            int i = 0;
            int j=0;
            int k = 0;
            int TopCounter = 0;
            TimeLine[] ArrayOfFreeTimeline = Dict_AvailablFreeTimeLineAndFitCount.Keys.ToArray();//array of FreeTimeLineRanges
            Dictionary<TimeLine, TimeLine[]> TimeLineDictionary = new Dictionary<TimeLine, TimeLine[]>();
            int[] ArrayOfFitCount = Dict_AvailablFreeTimeLineAndFitCount.Values.ToArray();//array list of fit count
            
            /*foreach (TimeLine MyTimeLine in MyArrayOfToBeAssignedTimeLine)
            { 
                TimeLineDictionary.Add(MyTimeLine, new TimeLine[1]);
            }
            */
            int BlockSplitsPerTimeLine = 0;
            int TotalAssignedSoFar = 0;

            for (; (i < MyArrayOfToBeAssignedTimeLine.Length) && (TotalAssignedSoFar < MyArrayOfToBeAssignedTimeLine.Length); i++)
            {
                ++BlockSplitsPerTimeLine;
                TotalAssignedSoFar = 0;
                for (j = 0; (j < ArrayOfFreeTimeline.Length) && (TotalAssignedSoFar < MyArrayOfToBeAssignedTimeLine.Length); j++)
                {
                    if (Dict_AvailablFreeTimeLineAndFitCount[ArrayOfFreeTimeline[j]] > 0)
                    {
                        
                        if (Dict_AvailablFreeTimeLineAndFitCount[ArrayOfFreeTimeline[j]] >= BlockSplitsPerTimeLine)
                        {
                            try
                            { TimeLineDictionary.Add(ArrayOfFreeTimeline[j], SplitAndAssign(ArrayOfFreeTimeline[j], BlockSplitsPerTimeLine, IdealTimePerAllotment)); }
                            catch (Exception ex)
                            {
                                TimeLineDictionary[ArrayOfFreeTimeline[j]] = SplitAndAssign(ArrayOfFreeTimeline[j], BlockSplitsPerTimeLine, IdealTimePerAllotment);
                            }
                        }


                        TotalAssignedSoFar += TimeLineDictionary[ArrayOfFreeTimeline[j]].Length;
                    }
                    
                }
            }
            

            /*for (; i < MyArrayOfToBeAssignedTimeLine.Length && k < MyArrayOfToBeAssignedTimeLine.Length; k++)//k counts to ensure we dont get infinite loop in scenario where all spaces are too small
            {
                j = 0;
                TopCounter++;
                for (; ((j < ArrayOfFreeTimeline.Length) && (i < MyArrayOfToBeAssignedTimeLine.Length)); j++)
                {
                    if (Dict_AvailablFreeTimeLineAndFitCount[ArrayOfFreeTimeline[j]] > 0)
                    {
                        try
                        {
                            
                            TimeLineDictionary.Add(ArrayOfFreeTimeline[j], SplitAndAssign(ArrayOfFreeTimeline[j], TopCounter, IdealTimePerAllotment));
                        }
                        catch 
                        {
                            TimeLineDictionary[ArrayOfFreeTimeline[j]] = SplitAndAssign(ArrayOfFreeTimeline[j], TopCounter, IdealTimePerAllotment);
                        }
                        --Dict_AvailablFreeTimeLineAndFitCount[ArrayOfFreeTimeline[j]];
                        i = TopCounter;
                    }
                }
            }*/

            TimeLine[][] MyArrayOfTimeLines=TimeLineDictionary.Values.ToArray();

            k = 0;
            i=0;
            //for (; i < MyArrayOfToBeAssignedTimeLine.Length; )
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

            if (MyArrayOfToBeAssignedTimeLine.Length > i)
            {
                MessageBox.Show("ALERT!!! NOT ALL SUBCALENDAR EVENTS WILL BE ASSIGNED");
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
                //MessageBox.Show("Cannot generate CentralizeYourSelfWithinRange TimeLine Because Difference is less than zero.\nWill Not Fit!!!");
                throw (new System.Exception("Cannot generate CentralizeYourSelfWithinRange TimeLine Because Difference is less than zero.\nWill Not Fit!!!"));
                //return CentralizedTimeline;
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

        static public TimeLine[] getAllFreeSpots_NoCompleteSchedule(TimeLine MyTimeLine)//Gets an array of all the freespots between busy spots in given time line, note attribute completeschedule is not used in finding freespots
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
                    if ((AllFreeSlots[i].TimelineSpan.TotalSeconds > 1))
                    { SpecificFreeSpots.Add(AllFreeSlots[i]); }
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
                    if ((AllFreeSlots[i].TimelineSpan.TotalSeconds > 1))
                    { SpecificFreeSpots.Add(AllFreeSlots[i]); }
                }
            }

            return SpecificFreeSpots.ToArray();
        }
        
        public void WriteToOutlook(CalendarEvent MyEvent)
        {
            int i = 0;
            if (MyEvent.RepetitionStatus)
            {
                LoopThroughAddRepeatEvents(MyEvent.Repeat);
            }
            else
            {
                for (; i < MyEvent.AllEvents.Length; i++)
                {
                    MyEvent.AllEvents[i].ThirdPartyID = AddAppointment(MyEvent.AllEvents[i], MyEvent.Name);
                }
            }


        }

        public void LoopThroughAddRepeatEvents(Repetition MyRepetition)
        {
            int i = 0;
            for(;i<MyRepetition.RecurringCalendarEvents.Length;i++)
            {
                WriteToOutlook(MyRepetition.RecurringCalendarEvents[i]);
            }
        }

        public void RemoveFromOutlook(CalendarEvent MyEvent)
        {

                int i = 0;
                if (MyEvent.RepetitionStatus)
                {
                    LoopThroughRemoveRepeatEvents(MyEvent.Repeat);
                }
                else
                {
                    for (i=0; i < MyEvent.AllEvents.Length; i++)
                    {
                        DeleteAppointment(MyEvent.AllEvents[i], MyEvent.Name, MyEvent.AllEvents[i].ThirdPartyID);
                    }
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
            try
            {
                Outlook.AppointmentItem item = calendarItems[ActiveSection.ID + "**" + NameOfParentCalendarEvent] as Outlook.AppointmentItem;
                item.Delete();
            }
            catch
            {
                return;
            }
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
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Location"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = CreateLocationNode(MyEvent.myLocation, "EventScheduleLocation").InnerXml;
            if (MyEvent.RepetitionStatus)
            {
                MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Recurrence"));
                MyEventScheduleNode.ChildNodes[0].InnerXml = CreateRepetitionNode(MyEvent.Repeat).InnerXml;
                //MyEventScheduleNode.PrependChild();
                //MyEventScheduleNode.PrependChild(CreateRepetitionNode(MyEvent.Repeat));

                return MyEventScheduleNode;
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
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Location"));
            MyEventSubScheduleNode.ChildNodes[0].InnerXml = CreateLocationNode(MySubEvent.myLocation, "EventSubScheduleLocation").InnerXml;
            return MyEventSubScheduleNode;
        }

        public XmlElement CreateLocationNode(Location Arg1, string ElementIdentifier)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement var1 = xmldoc.CreateElement(ElementIdentifier);
            string XCoordinate = "";
            string YCoordinate = "";
            string Descripion = "";
            string MappedAddress = "";
            if (Arg1 != null)
            {
                XCoordinate = Arg1.XCoordinate.ToString();
                YCoordinate = Arg1.YCoordinate.ToString();
                Descripion =  Arg1.Description;
                MappedAddress = Arg1.Address;
            }
            var1.PrependChild(xmldoc.CreateElement("XCoordinate"));
            var1.ChildNodes[0].InnerText = XCoordinate;
            var1.PrependChild(xmldoc.CreateElement("YCoordinate"));
            var1.ChildNodes[0].InnerText = YCoordinate;
            var1.PrependChild(xmldoc.CreateElement("Address"));
            var1.ChildNodes[0].InnerText = MappedAddress;
            var1.PrependChild(xmldoc.CreateElement("Description"));
            var1.ChildNodes[0].InnerText = Descripion;
            return var1;
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
