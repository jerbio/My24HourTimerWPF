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
using WinForms = System.Windows.Forms;
using TilerFront;
using System.Threading.Tasks;
using TilerElements;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Host;
//using System.Web.Mvc;



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
            //MessageBox.Show("Testing Branch creation");
            DateTimeOffset Start = new DateTimeOffset(2014, 2, 13, 0, 0, 0, new TimeSpan());
            DateTimeOffset Now = DateTimeOffset.Now;
            TimeSpan spent= Now - Start;
            LogInToWagtap();
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

        DateTimeOffset FinalDate=new DateTimeOffset();
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
            datePicker1.SelectedDate = new DateTime( DateTimeOffset.Now.ToLocalTime().Ticks);
            datePicker2.SelectedDate = new DateTime(DateTimeOffset.Now.ToLocalTime().Ticks);
            DateTimeOffset time = datePicker1.SelectedDate.Value;
            bool flag = checkBox2.IsChecked.Value;
            bool flag2 = checkBox3.IsChecked.Value;
            string str3 = textBox3.Text;
            bool flag3 = checkBox5.IsChecked.Value;
            string str4 = textBox4.Text;
            DateTimeOffset time2 = datePicker2.SelectedDate.Value;
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

        public static DateTimeOffset ConvertToDateTime(string StringOfDateTime)
        {
            
            string[] strArray = StringOfDateTime.Split(new char[] { '|' });
            string[] strArray2 = strArray[0].Split(new char[] { ' ' });
            string[] strArray3 = strArray[1].Split(new char[] { ' ' });
            return new DateTimeOffset(Convert.ToInt16(strArray2[0]), Convert.ToInt16(strArray2[1]), Convert.ToInt16(strArray2[2]), Convert.ToInt16(strArray3[0]), Convert.ToInt16(strArray3[1]), Convert.ToInt16(strArray3[2]), new TimeSpan());
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

        async private void UpdateDeadline(object sender, RoutedEventArgs e)
        {
            
            
            
            DateTimeOffset EndTime = DateTimeOffset.Parse(textBox7.Text);
            DateTimeOffset EndDate = DateTime.Parse( datePicker2.SelectedDate.Value.ToShortDateString ()+" " +textBox7.Text) ;
            string EventID = textBox9.Text;
            SubCalendarEvent MySubcal= MySchedule.getSubCalendarEvent(EventID);
            CalendarEvent myCalEvent = MySchedule.getCalendarEvent(MySubcal.SubEvent_ID.getCalendarEventID());

            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> result =MySchedule.BundleChangeUpdate(EventID,myCalEvent.Name,MySubcal.Start,MySubcal.End,MySubcal.getCalendarEventRange.Start,EndDate,myCalEvent.NumberOfSplit);

            //"BundleChangeUpdate"
            /*
            string CalId=MySubcal.SubEvent_ID.getCalendarEventID();
            CalendarEvent MyCal = MySchedule.getCalendarEvent(CalId);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> result = MySchedule.BundleChangeUpdate(EventID, MyCal.Name, MyCal.Start, MyCal.End.AddDays(1), MyCal.NumberOfSplit);*/
            //DateTimeOffset fullDate = new DateTimeOffset(EndDate.Year, EndDate.Month, EndDate.Day, EndTime.Hour, EndTime.Minute, EndTime.Second, new TimeSpan());
            //Tuple<CustomErrors, Dictionary<string, CalendarEvent>>result= MySchedule.UpdateDeadLine(EventID, fullDate);
            
            await MySchedule.UpdateWithProcrastinateSchedule(result.Item2).ConfigureAwait(false);

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

        public DateTimeOffset getCurrentTimeFromInternet()
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

        public DateTimeOffset GetLastTimeStamp(string MatchingStamp)
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
            return new DateTimeOffset();
        }

        public TimeSpan GetLatestSleepDifference()
        {
            string[] strArray = GetCurrentTextOfFile(@"..\WriteLines.txt").Split(new char[] { '\n' });
            bool flag = false;
            DateTimeOffset time = new DateTimeOffset();
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
                    DateTimeOffset time2 = ConvertToDateTime(strArray2[1] + "|" + strArray2[2]);
                    return (TimeSpan)(time - time2);
                }
            }
            return new TimeSpan(0, 0, 0, 0, 0);
        }

        public int getTimeDifference(DateTimeOffset Time1, DateTimeOffset Time2)
        {
            TimeSpan span = (TimeSpan)(Time2 - Time1);
            return span.Seconds;
        }

        public TimeSpan GetTotalSleepIn24Difference()
        {
            string[] strArray = GetCurrentTextOfFile(@"..\WriteLines.txt").Split(new char[] { '\n' });
            bool flag = false;
            TimeSpan span = new TimeSpan(0, 0, 0, 0, 0);
            DateTimeOffset time = new DateTimeOffset();
            for (int i = strArray.Length - 1; i >= 0; i--)
            {
                DateTimeOffset time2;
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

        public DateTimeOffset RetrieveDate(string[] ArrayOfDate, string ArrayOfTime)
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
            return new DateTimeOffset(Convert.ToInt16(ArrayOfDate[2]), month, Convert.ToInt16(ArrayOfDate[1]), hour, minute, 0, new TimeSpan());
        }

        public DateTimeOffset RetrieveStoredTime()
        {
            string[] strArray = File.ReadAllLines(@"..\WriteLines2.txt");
            return new DateTimeOffset(Convert.ToInt16(strArray[0]), Convert.ToInt16(strArray[1]), Convert.ToInt16(strArray[2]), Convert.ToInt16(strArray[3]), Convert.ToInt16(strArray[4]), Convert.ToInt16(strArray[5]), new TimeSpan());
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
            DateTimeOffset newSystemTime = getCurrentTimeFromInternet();
            newSystemTime.AddMilliseconds(0.0);
            textBlock1.Text = newSystemTime.ToString();
            SystemTimeUpdate update = new SystemTimeUpdate(newSystemTime);
            File.WriteAllText(@"..\WriteLines.txt", GetCurrentTextOfFile(@"..\WriteLines.txt") + string.Concat(new object[] { SleepWakeString, "|", newSystemTime.Year, " ", newSystemTime.Month, " ", newSystemTime.Day, "|", newSystemTime.Hour, " ", newSystemTime.Minute, " ", newSystemTime.Second, "\n" }));
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            DateTimeOffset newSystemTime = getCurrentTimeFromInternet();
            newSystemTime.AddMilliseconds(0.0);
            DateTimeOffset time3 = GetLastTimeStamp("Sleep_Time_M").AddDays(1.0);
            SystemTimeUpdate update = new SystemTimeUpdate(newSystemTime);
            BusyTimeLine NextActivity = MySchedule.NextActivity;
            DateTimeOffset now = DateTimeOffset.Now;
            if (NextActivity != null)
            {
                textBlock2.Text = "Next Activity is : " + MySchedule.getCalendarEvent(NextActivity.TimeLineID).Name;
                FinalDate = NextActivity.Start;
                now = new DateTimeOffset(now.Ticks - (now.Ticks % 0x989680L), new TimeSpan());
                FinalDate = new DateTimeOffset(FinalDate.Ticks - (FinalDate.Ticks % 0x989680L), new TimeSpan());
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
            now = new DateTimeOffset(now.Ticks - (now.Ticks % 0x989680L), new TimeSpan());
            FinalDate = new DateTimeOffset(FinalDate.Ticks - (FinalDate.Ticks % 0x989680L), new TimeSpan());
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
            DateTimeOffset newSystemTime = getCurrentTimeFromInternet();
            newSystemTime.AddMilliseconds(0.0);
            textBlock1.Text = newSystemTime.ToString();
            SystemTimeUpdate update = new SystemTimeUpdate(newSystemTime);
            File.WriteAllText(@"..\WriteLines.txt", GetCurrentTextOfFile(@"..\WriteLines.txt") + string.Concat(new object[] { "Wake_Time|", newSystemTime.Year, " ", newSystemTime.Month, " ", newSystemTime.Day, "|", newSystemTime.Hour, " ", newSystemTime.Minute, " ", newSystemTime.Second, "\n" }));
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            DateTimeOffset newSystemTime = getCurrentTimeFromInternet();
            newSystemTime.AddMilliseconds(0.0);
            TimeSpan span = GetTotalSleepIn24Difference();
            textBlock1.Text = string.Concat(new object[] { span.Days, ":", span.Hours, ":", span.Minutes, ":", span.Seconds });
            SystemTimeUpdate update = new SystemTimeUpdate(newSystemTime);
            string currentTextOfFile = GetCurrentTextOfFile(@"..\WriteLines.txt");
            string[] strArray = currentTextOfFile.Split(new char[] { '\n' });
            File.WriteAllText(@"..\WriteLines.txt", currentTextOfFile + string.Concat(new object[] { "Log_Check|", newSystemTime.Year, " ", newSystemTime.Month, " ", newSystemTime.Day, "|", newSystemTime.Hour, " ", newSystemTime.Minute, " ", newSystemTime.Second, "\n" }));
        }

        async private void button5_Click_2(object sender, RoutedEventArgs e)
        {
            
            string eventName = textBox1.Text;
            string LocationString  = textBox8.Text.Trim();
            /*if (LocationString != "")
            {
                eventName += "," + LocationString;
            }*/




            DateTimeOffset CurrentTimeOfExecution = Schedule.Now.calculationNow;
            string eventStartTime = textBox5.Text;
            string locationInformation = textBox8.Text;
            DateTimeOffset eventStartDate = (DateTimeOffset)datePicker1.SelectedDate.Value;
            string eventEndTime = textBox7.Text;
            DateTimeOffset eventEndDate = (DateTimeOffset)datePicker2.SelectedDate.Value;
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
            DateTimeOffset EnteredDateTime = new DateTimeOffset(eventStartDate.Year, eventStartDate.Month, eventStartDate.Day, Convert.ToInt32(TimeElements[0]), Convert.ToInt32(TimeElements[1]), 0, new TimeSpan());
            //if checks for StartDateTime
            if (EnteredDateTime < DateTimeOffset.Now)
            {
                //DateTimeOffset Now=DateTimeOffset.Now;
                //MessageBox.Show("Please Adjust Your Start Date, Its less than the current time:");
                //return;
            }

            if (eventEndTime == "")
            {
                string EventEndDateTime = CurrentTimeOfExecution.ToString();
                string[] TempString = EventEndDateTime.Split(' ');
                eventEndTime = TempString[1] + TempString[2];
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
            DateTimeOffset CurrentNow = DateTimeOffset.Now;
            DateTimeOffset RepeatStart = CurrentNow;
            DateTimeOffset RepeatEnd=RepeatStart;

            if (checkBox2.IsChecked.Value)
            {
                //RepeatStart = (DateTimeOffset)calendar3.SelectedDate.Value;
                DateTimeOffset FullStartTime = DateTimeOffset.Parse(eventStartDate.Date.ToShortDateString() + " " + eventStartTime);
                DateTimeOffset FullEndTime = DateTimeOffset.Parse(eventEndDate.Date.ToShortDateString() + " " + eventEndTime);

                List<int> selectedDaysOftheweek = getDaysOfWeek();


                RepeatStart = DateTimeOffset.Parse(eventStartTime);
                RepeatEnd = (DateTimeOffset)calendar4.SelectedDate.Value;
                RepetitionFlag = true;
                MyRepetition = new Repetition(RepetitionFlag, new TimeLine(RepeatStart, RepeatEnd), RepeatFrequency, new TimeLine((FullStartTime), (FullEndTime)), selectedDaysOftheweek.ToArray());
                //eventStartDate = RepeatStart;
                eventEndDate = RepeatEnd;
            }

            CustomErrors ErrorCheck = ValidateInputValues(EventDuration, eventStartTime, eventStartDate.ToString(), eventEndTime, eventEndDate.ToString(), RepeatStart.ToString(), RepeatEnd.ToString(), PreDeadlineTime, eventSplit, eventPrepTime, CurrentNow);

            if (!ErrorCheck.Status)
            { 
                MessageBox.Show(ErrorCheck.Message);
                return;
                //
            }
            //C6RXEZ             
            
            Location_Elements var0 = new Location_Elements(textBox8.Text);
            var0.Validate();
            EventDisplay UiData = new EventDisplay();
            MiscData NoteData = new MiscData();
            bool CompletedFlag = false;



            DateTimeOffset StartData = DateTimeOffset.Parse(eventStartDate.Date.ToShortDateString() + " " + eventStartTime);
            DateTimeOffset EndData = DateTimeOffset.Parse(eventEndDate.Date.ToShortDateString() + " " + eventEndTime);

            //CalendarEvent ScheduleUpdated = CreateSchedule(eventName, eventStartTime, eventStartDate, eventEndTime, eventEndDate, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionflag, DefaultPreDeadlineFlag, RigidScheduleFlag, eventPrepTime, DefaultPreDeadlineFlag);
            CalendarEvent ScheduleUpdated = new CalendarEvent(eventName, StartData, EndData, eventSplit, PreDeadlineTime, EventDuration, MyRepetition, DefaultPreDeadlineFlag, RigidFlag, eventPrepTime, DefaultPreDeadlineFlag, var0, true, UiData, NoteData, CompletedFlag);
            if (RestrictedCheckbox.IsChecked.Value)
            {
                string TimeString = eventStartDate.Date.ToShortDateString() + " " + eventStartTime+" +00:00";
                DateTimeOffset StartDateTime= DateTimeOffset.Parse(TimeString);
                TimeString = eventEndDate.Date.ToShortDateString() + " " + eventEndTime + " +00:00";
                DateTimeOffset EndDateTime= DateTimeOffset.Parse(TimeString);
                string restrictionStartString = TimeFrameStart.Text + " 1/1/1970 +00:00";
                string restrictionEndString = TimeFrameEnd.Text + " 1/1/1970 +00:00";

                DateTimeOffset RestrictionStart = DateTimeOffset.Parse(restrictionStartString);
                DateTimeOffset RestrictionEnd = DateTimeOffset.Parse(restrictionEndString);
                TimeSpan RestrinSpan = RestrictionEnd -RestrictionStart ;
                

                List<mTuple<bool,DayOfWeek>> allElements = (new mTuple<bool,System.DayOfWeek>[7]).ToList();
                
                

                allElements[(int)DayOfWeek.Sunday] = new mTuple<bool,System.DayOfWeek>(false,DayOfWeek.Sunday);
                allElements[(int)DayOfWeek.Monday] = new mTuple<bool,System.DayOfWeek>(false,DayOfWeek.Monday);
                allElements[(int)DayOfWeek.Tuesday] = new mTuple<bool,System.DayOfWeek>(false,DayOfWeek.Tuesday);
                allElements[(int)DayOfWeek.Wednesday] = new mTuple<bool,System.DayOfWeek>(false,DayOfWeek.Wednesday);
                allElements[(int)DayOfWeek.Thursday] = new mTuple<bool,System.DayOfWeek>(false,DayOfWeek.Thursday);
                allElements[(int)DayOfWeek.Friday] = new mTuple<bool,System.DayOfWeek>(false,DayOfWeek.Friday);
                allElements[(int)DayOfWeek.Saturday] = new mTuple<bool,System.DayOfWeek>(false,DayOfWeek.Saturday);

                allElements[0].Item1=SunCheckbox.IsChecked.Value;
                allElements[1].Item1=MonCheckbox.IsChecked.Value;
                allElements[2].Item1=TueCheckbox.IsChecked.Value;
                allElements[3].Item1=WedCheckbox.IsChecked.Value;
                allElements[4].Item1=ThuCheckbox.IsChecked.Value;
                allElements[5].Item1=FriCheckbox.IsChecked.Value;
                allElements[6].Item1=SatCheckbox.IsChecked.Value;
                RestrictionTimeLine restrictTimeLine = new RestrictionTimeLine(RestrictionStart,RestrictionEnd);
                //RestrictionProfile myRestrictionProfile= new RestrictionProfile(RestrictionStart,RestrinSpan);
                RestrictionProfile myRestrictionProfile = new RestrictionProfile(7,DayOfWeek.Monday, RestrictionStart, RestrictionEnd);
                foreach(mTuple<bool,DayOfWeek> eachMTuple in allElements)
                {
                    
                }

                //myRestrictionProfile=new RestrictionProfile(allElements.Where(obj=>obj.Item1).Select(obj=>obj.Item2),restrictTimeLine);
                
                //myRestrictionProfile = new RestrictionProfile()



                ScheduleUpdated = new CalendarEventRestricted(eventName, StartDateTime, EndDateTime, myRestrictionProfile, TimeSpan.Parse(EventDuration), MyRepetition, false, true, Convert.ToInt32(eventSplit), RigidFlag, new Location_Elements(), TimeSpan.Parse(eventPrepTime), TimeSpan.Parse(PreDeadlineTime), UiData, NoteData);
            }
            
            ScheduleUpdated.Repeat.PopulateRepetitionParameters(ScheduleUpdated);
            textBlock9.Text = "...Loading";
            Stopwatch snugarrayTester = new Stopwatch();
            snugarrayTester.Start();
            ///*
            Task<CustomErrors> addToScheduleTask = MySchedule.AddToScheduleAndCommit(ScheduleUpdated);
            CustomErrors ScheduleUpdateMessage = await addToScheduleTask.ConfigureAwait(false);
             //*/

            //CustomErrors ScheduleUpdateMessage = MySchedule.AddToSchedule(ScheduleUpdated);
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

        async private void PeekIntoFuture(object sender, RoutedEventArgs e)
        {

            string eventName = textBox1.Text;
            string LocationString = textBox8.Text.Trim();
            /*if (LocationString != "")
            {
                eventName += "," + LocationString;
            }*/




            DateTimeOffset CurrentTimeOfExecution = Schedule.Now.calculationNow;
            string eventStartTime = textBox5.Text;
            string locationInformation = textBox8.Text;
            DateTimeOffset eventStartDate = (DateTimeOffset)datePicker1.SelectedDate.Value;
            string eventEndTime = textBox7.Text;
            DateTimeOffset eventEndDate = (DateTimeOffset)datePicker2.SelectedDate.Value;
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
            DateTimeOffset EnteredDateTime = new DateTimeOffset(eventStartDate.Year, eventStartDate.Month, eventStartDate.Day, Convert.ToInt32(TimeElements[0]), Convert.ToInt32(TimeElements[1]), 0, new TimeSpan());
            //if checks for StartDateTime
            if (EnteredDateTime < DateTimeOffset.Now)
            {
                //DateTimeOffset Now=DateTimeOffset.Now;
                //MessageBox.Show("Please Adjust Your Start Date, Its less than the current time:");
                //return;
            }

            if (eventEndTime == "")
            {
                string EventEndDateTime = CurrentTimeOfExecution.ToString();
                string[] TempString = EventEndDateTime.Split(' ');
                eventEndTime = TempString[1] + TempString[2];
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
            DateTimeOffset CurrentNow = DateTimeOffset.Now;
            DateTimeOffset RepeatStart = CurrentNow;
            DateTimeOffset RepeatEnd = RepeatStart;

            if (checkBox2.IsChecked.Value)
            {
                //RepeatStart = (DateTimeOffset)calendar3.SelectedDate.Value;
                DateTimeOffset FullStartTime = DateTimeOffset.Parse(eventStartDate.Date.ToShortDateString() + " " + eventStartTime);
                DateTimeOffset FullEndTime = DateTimeOffset.Parse(eventEndDate.Date.ToShortDateString() + " " + eventEndTime);

                List<int> selectedDaysOftheweek = getDaysOfWeek();


                RepeatStart = DateTimeOffset.Parse(eventStartTime);
                RepeatEnd = (DateTimeOffset)calendar4.SelectedDate.Value;
                RepetitionFlag = true;
                MyRepetition = new Repetition(RepetitionFlag, new TimeLine(RepeatStart, RepeatEnd), RepeatFrequency, new TimeLine((FullStartTime), (FullEndTime)), selectedDaysOftheweek.ToArray());
                //eventStartDate = RepeatStart;
                eventEndDate = RepeatEnd;
            }

            CustomErrors ErrorCheck = ValidateInputValues(EventDuration, eventStartTime, eventStartDate.ToString(), eventEndTime, eventEndDate.ToString(), RepeatStart.ToString(), RepeatEnd.ToString(), PreDeadlineTime, eventSplit, eventPrepTime, CurrentNow);

            if (!ErrorCheck.Status)
            {
                MessageBox.Show(ErrorCheck.Message);
                return;
                //
            }
            //C6RXEZ             
            Location_Elements var0 = new Location_Elements(textBox8.Text);

            EventDisplay UiData = new EventDisplay();
            MiscData NoteData = new MiscData();
            bool CompletedFlag = false;



            DateTimeOffset StartData = DateTimeOffset.Parse(eventStartDate.Date.ToShortDateString() + " " + eventStartTime);
            DateTimeOffset EndData = DateTimeOffset.Parse(eventEndDate.Date.ToShortDateString() + " " + eventEndTime);

            //CalendarEvent ScheduleUpdated = CreateSchedule(eventName, eventStartTime, eventStartDate, eventEndTime, eventEndDate, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionflag, DefaultPreDeadlineFlag, RigidScheduleFlag, eventPrepTime, DefaultPreDeadlineFlag);
            CalendarEvent ScheduleUpdated = new CalendarEvent(eventName, StartData, EndData, eventSplit, PreDeadlineTime, EventDuration, MyRepetition, DefaultPreDeadlineFlag, RigidFlag, eventPrepTime, DefaultPreDeadlineFlag, var0, true, UiData, NoteData, CompletedFlag);
            if (RestrictedCheckbox.IsChecked.Value)
            {
                string TimeString = eventStartDate.Date.ToShortDateString() + " " + eventStartTime + " +00:00";
                DateTimeOffset StartDateTime = DateTimeOffset.Parse(TimeString);
                TimeString = eventEndDate.Date.ToShortDateString() + " " + eventEndTime + " +00:00";
                DateTimeOffset EndDateTime = DateTimeOffset.Parse(TimeString);
                string restrictionStartString = TimeFrameStart.Text + " 1/1/1970 +00:00";
                string restrictionEndString = TimeFrameEnd.Text + " 1/1/1970 +00:00";

                DateTimeOffset RestrictionStart = DateTimeOffset.Parse(restrictionStartString);
                DateTimeOffset RestrictionEnd = DateTimeOffset.Parse(restrictionEndString);
                TimeSpan RestrinSpan = RestrictionEnd - RestrictionStart;


                List<mTuple<bool, DayOfWeek>> allElements = (new mTuple<bool, System.DayOfWeek>[7]).ToList();



                allElements[(int)DayOfWeek.Sunday] = new mTuple<bool, System.DayOfWeek>(false, DayOfWeek.Sunday);
                allElements[(int)DayOfWeek.Monday] = new mTuple<bool, System.DayOfWeek>(false, DayOfWeek.Monday);
                allElements[(int)DayOfWeek.Tuesday] = new mTuple<bool, System.DayOfWeek>(false, DayOfWeek.Tuesday);
                allElements[(int)DayOfWeek.Wednesday] = new mTuple<bool, System.DayOfWeek>(false, DayOfWeek.Wednesday);
                allElements[(int)DayOfWeek.Thursday] = new mTuple<bool, System.DayOfWeek>(false, DayOfWeek.Thursday);
                allElements[(int)DayOfWeek.Friday] = new mTuple<bool, System.DayOfWeek>(false, DayOfWeek.Friday);
                allElements[(int)DayOfWeek.Saturday] = new mTuple<bool, System.DayOfWeek>(false, DayOfWeek.Saturday);

                allElements[0].Item1 = SunCheckbox.IsChecked.Value;
                allElements[1].Item1 = MonCheckbox.IsChecked.Value;
                allElements[2].Item1 = TueCheckbox.IsChecked.Value;
                allElements[3].Item1 = WedCheckbox.IsChecked.Value;
                allElements[4].Item1 = ThuCheckbox.IsChecked.Value;
                allElements[5].Item1 = FriCheckbox.IsChecked.Value;
                allElements[6].Item1 = SatCheckbox.IsChecked.Value;
                RestrictionTimeLine restrictTimeLine = new RestrictionTimeLine(RestrictionStart, RestrictionEnd);
                //RestrictionProfile myRestrictionProfile= new RestrictionProfile(RestrictionStart,RestrinSpan);
                RestrictionProfile myRestrictionProfile = new RestrictionProfile(7, DayOfWeek.Monday, RestrictionStart, RestrictionEnd);
                foreach (mTuple<bool, DayOfWeek> eachMTuple in allElements)
                {

                }

                //myRestrictionProfile=new RestrictionProfile(allElements.Where(obj=>obj.Item1).Select(obj=>obj.Item2),restrictTimeLine);

                //myRestrictionProfile = new RestrictionProfile()



                ScheduleUpdated = new CalendarEventRestricted(eventName, StartDateTime, EndDateTime, myRestrictionProfile, TimeSpan.Parse(EventDuration), MyRepetition, false, true, Convert.ToInt32(eventSplit), RigidFlag, new Location_Elements(), TimeSpan.Parse(eventPrepTime), TimeSpan.Parse(PreDeadlineTime), UiData, NoteData);
            }

            ScheduleUpdated.Repeat.PopulateRepetitionParameters(ScheduleUpdated);
            textBlock9.Text = "...Loading";
            Stopwatch snugarrayTester = new Stopwatch();
            snugarrayTester.Start();
            ///*
            Tuple<List<SubCalendarEvent>[], DayTimeLine[], List<SubCalendarEvent>> peekingResult = MySchedule.peekIntoSchedule(ScheduleUpdated);
            
            //*/

            //CustomErrors ScheduleUpdateMessage = MySchedule.AddToSchedule(ScheduleUpdated);
            snugarrayTester.Stop();
            
            //else
            {
                textBlock9.Text = "Peeking is complete" + ScheduleUpdated.Name;
                //MessageBox.Show(ScheduleUpdateMessage.Message);
            }

        }


        public static CustomErrors ValidateInputValues(string ActiveDuration, string StartTimeEntry, string StartDateEntry, string EndTimeEntry, string EndDateEntry, string RepeatStart, string RepeatEnd, string PredeadlineTime, string NumberOfSplits, string PrepTime, DateTimeOffset PassedNow)
        {
            TimeSpan ActiveDurationTimeSpan = TimeSpan.Parse(ActiveDuration);
            TimeSpan PrepTimeTimeSpan = TimeSpan.Parse(PrepTime);


            DateTimeOffset StartTimeDateTime = DateTimeOffset.Parse(StartTimeEntry);
            DateTimeOffset StartDateTime = DateTimeOffset.Parse(StartDateEntry);
            StartDateTime = new DateTimeOffset(StartDateTime.Year, StartDateTime.Month, StartDateTime.Day, StartTimeDateTime.Hour, StartTimeDateTime.Minute, StartTimeDateTime.Second, new TimeSpan());
            string[] StartDateArray = StartDateTime.ToString().Split(' ')[1].Split('/');



            DateTimeOffset EndTimeDateTime = DateTimeOffset.Parse(EndTimeEntry);
            DateTimeOffset EndDateTime = DateTimeOffset.Parse(EndDateEntry);
            EndDateTime = new DateTimeOffset(EndDateTime.Year, EndDateTime.Month, EndDateTime.Day, EndTimeDateTime.Hour, EndTimeDateTime.Minute, EndTimeDateTime.Second, new TimeSpan());
            DateTimeOffset RepeatStartDate = DateTimeOffset.Parse(RepeatStart);
            DateTimeOffset RepeatEndDate = DateTimeOffset.Parse(RepeatEnd);
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

            DateTimeOffset Hmm = DateTimeOffset.Now;
            


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

        /*public CalendarEvent CreateSchedule(string Name, string StartTime, DateTimeOffset StartDate, string EndTime, DateTimeOffset EventEndDate, string eventSplit, string PreDeadlineTime, string EventDuration, bool EventRepetitionflag, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag)
        {
            string MiltaryStartTime = convertTimeToMilitary(StartTime);
            StartDate = new DateTimeOffset(StartDate.Year, StartDate.Month, StartDate.Day, Convert.ToInt32(MiltaryStartTime.Split(':')[0]), Convert.ToInt32(MiltaryStartTime.Split(':')[1]), 0);
            string MiltaryEndTime = convertTimeToMilitary(EndTime);
            EventEndDate = new DateTimeOffset(EventEndDate.Year, EventEndDate.Month, EventEndDate.Day, Convert.ToInt32(MiltaryEndTime.Split(':')[0]), Convert.ToInt32(MiltaryEndTime.Split(':')[1]), 0);
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
            DateTimeOffset eventStartDate = (DateTimeOffset)datePicker1.SelectedDate.Value;
            DateTimeOffset eventEndDate = (DateTimeOffset)datePicker2.SelectedDate.Value;
            eventStartDate = new DateTimeOffset(eventStartDate.Year, eventStartDate.Month, eventStartDate.Day, Convert.ToInt32(TimeElements[0]), Convert.ToInt32(TimeElements[1]), 0, new TimeSpan());
            TimeElements = CalendarEvent.convertTimeToMilitary(eventEndTime).Split(':');
            eventEndDate = new DateTimeOffset(eventEndDate.Year, eventEndDate.Month, eventEndDate.Day, Convert.ToInt32(TimeElements[0]), Convert.ToInt32(TimeElements[1]), 0, new TimeSpan());
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
            MySchedule.deleteSubCalendarEventAndReadjust(EventID);
        }

        async private void button8_Click(object sender, RoutedEventArgs e)
        {
            int ProcrastinateDays = Convert.ToInt16(comboBox4.Text);
            int ProcrastinateHours = Convert.ToInt16(comboBox5.Text);
            int ProcrastinateMins = Convert.ToInt16(comboBox6.Text);
            TimeSpan DelaySpan = new TimeSpan(ProcrastinateDays, ProcrastinateHours, ProcrastinateMins, 0);
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage;

            string choicePath = "";
            if (string.IsNullOrEmpty(textBox9.Text))//check for specific id removal account
            {
                ScheduleUpdateMessage = MySchedule.ProcrastinateAll(DelaySpan);
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
                    await MySchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2).ConfigureAwait(false);
                }
            }
            else
            {

                await MySchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2).ConfigureAwait(false);

                textBlock9.Text = "Schedule updated no clash detected";
            }

        }


        private void MarkAsDoneAndReAdjust(object sender, RoutedEventArgs e)
        {
            string EventID = textBox9.Text.Trim();
            //MySchedule.markAsCompleteCalendarEventAndReadjust(EventID);
            MySchedule.markSubEventAsCompleteCalendarEventAndReadjust(EventID);
        }

        async private void NowButtonClick(object sender, RoutedEventArgs e)
        {
            string EventID = textBox9.Text.Trim();
            ///*
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage=MySchedule.SetCalendarEventAsNow(EventID);

             await MySchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2).ConfigureAwait(false);
             return;
            //*///
            /*
            
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage = MySchedule.SetEventAsNow(EventID);
            //*/
            if (ScheduleUpdateMessage.Item1.Status)
            {
                switch (ScheduleUpdateMessage.Item1.Code)
                {
                    case 5:
                        {
                            MessageBoxResult result = MessageBox.Show(ScheduleUpdateMessage.Item1.Message, "Do you want to continue with this collision? ", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                ScheduleUpdateMessage=MySchedule.SetEventAsNow(EventID, true); ;
                                await MySchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2).ConfigureAwait(false);
                            }
                        }
                        break;
                    default://hack alert we need to figure out how to fix this error
                        await MySchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2).ConfigureAwait(false);
                        break;
                }
            }
            else
            {
                await MySchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2).ConfigureAwait(false);
            }
            
        }

        async private void RunEvaluation(object sender, RoutedEventArgs e)
        {
            int NumberOfRetries = Convert.ToInt32(textBox10.Text);
            long[] AllData = new long[NumberOfRetries];
            
            while(--NumberOfRetries>=0)
            {
                TilerFront.Models.LoginViewModel myLogin = new TilerFront.Models.LoginViewModel() { Username = UserNameTextBox.Text, Password = PasswordTextBox.Text, RememberMe = true };

                TilerFront.Models.AuthorizedUser AuthorizeUser = new TilerFront.Models.AuthorizedUser() { UserID = "d350ba4d-fe0b-445c-bed6-b6411c2156b3", UserName = "jerbio" };

                UserAccount currentUser = await AuthorizeUser.getUserAccountDebug();
                //MySchedule = new Schedule(currentUser,DateTimeOffset.Now);
                
                string eventName = textBox1.Text;
                string LocationString = textBox8.Text.Trim();
                /*if (LocationString != "")
                {
                    eventName += "," + LocationString;
                }*/

                DateTimeOffset CurrentTimeOfExecution = DateTimeOffset.Now;
                string eventStartTime = textBox5.Text;
                string locationInformation = textBox8.Text;
                DateTimeOffset eventStartDate = (DateTimeOffset)datePicker1.SelectedDate.Value;
                string eventEndTime = textBox7.Text;
                DateTimeOffset eventEndDate = (DateTimeOffset)datePicker2.SelectedDate.Value;
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
                DateTimeOffset EnteredDateTime = new DateTimeOffset(eventStartDate.Year, eventStartDate.Month, eventStartDate.Day, Convert.ToInt32(TimeElements[0]), Convert.ToInt32(TimeElements[1]), 0, new TimeSpan());
                //if checks for StartDateTime
                if (EnteredDateTime < DateTimeOffset.Now)
                {
                    //DateTimeOffset Now=DateTimeOffset.Now;
                    //MessageBox.Show("Please Adjust Your Start Date, Its less than the current time:");
                    //return;
                }

                if (eventEndTime == "")
                {
                    DateTimeOffset EventEndDateTime = new DateTimeOffset(eventEndDate.Year, eventEndDate.Month, eventEndDate.Day, EnteredDateTime.Hour, EnteredDateTime.Minute, EnteredDateTime.Second, new TimeSpan());

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
                DateTimeOffset CurrentNow = DateTimeOffset.Now;
                DateTimeOffset RepeatStart = CurrentNow;
                DateTimeOffset RepeatEnd = RepeatStart;

                if (checkBox2.IsChecked.Value)
                {
                    

                    DateTimeOffset FullStartTime = DateTimeOffset.Parse(eventStartDate + " " + eventStartTime);
                    DateTimeOffset FullEndTime = DateTimeOffset.Parse(eventEndDate + " " + eventEndTime);

                    List<int> selectedDaysOftheweek = getDaysOfWeek();

                    //RepeatStart = (DateTimeOffset)calendar3.SelectedDate.Value;
                    RepeatStart = DateTimeOffset.Parse(eventStartTime);
                    RepeatEnd = (DateTimeOffset)calendar4.SelectedDate.Value;
                    //RepeatEnd = (DateTimeOffset.Now).AddDays(7);
                    RepetitionFlag = true;
                                                //(bool EnableFlag, TimeLine RepetitionRange_Entry, string Frequency, TimeLine EventActualRange, int[] WeekDayData)
                    MyRepetition = new Repetition(RepetitionFlag, new TimeLine(RepeatStart, RepeatEnd), RepeatFrequency, new TimeLine((FullStartTime), (FullEndTime)), selectedDaysOftheweek.ToArray());

                    //eventStartDate = RepeatStart;
                    eventEndDate = RepeatStart;
                }

                CustomErrors ErrorCheck = ValidateInputValues(EventDuration, eventStartTime, eventStartDate.ToString(), eventEndTime, eventEndDate.ToString(), RepeatStart.ToString(), RepeatEnd.ToString(), PreDeadlineTime, eventSplit, eventPrepTime, CurrentNow);

                if (!ErrorCheck.Status)
                {
                    //MessageBox.Show(ErrorCheck.Message);
                    return;
                    //
                }
                //C6RXEZ             
                Location_Elements var0 = new Location_Elements(textBox8.Text);

                EventDisplay UiData = new EventDisplay();
                MiscData NoteData = new MiscData();
                bool CompletedFlag = false;

                DateTimeOffset StartData = DateTimeOffset.Parse(eventStartTime + " " + eventStartDate.Date.ToShortDateString());
                DateTimeOffset EndData = DateTimeOffset.Parse(eventEndTime + " " + eventEndDate.Date.ToShortDateString());

                //CalendarEvent ScheduleUpdated = CreateSchedule(eventName, eventStartTime, eventStartDate, eventEndTime, eventEndDate, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionflag, DefaultPreDeadlineFlag, RigidScheduleFlag, eventPrepTime, DefaultPreDeadlineFlag);
                CalendarEvent ScheduleUpdated = new CalendarEvent(eventName, StartData, EndData, eventSplit, PreDeadlineTime, EventDuration, MyRepetition, DefaultPreDeadlineFlag, RigidFlag, eventPrepTime, DefaultPreDeadlineFlag, var0, true, UiData, NoteData, CompletedFlag);
                ScheduleUpdated.Repeat.PopulateRepetitionParameters(ScheduleUpdated);
                
                Stopwatch snugarrayTester = new Stopwatch();
                snugarrayTester.Start();
                //CustomErrors ScheduleUpdateMessage = await MySchedule.AddToScheduleAndCommit(ScheduleUpdated);
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

        private List<int> getDaysOfWeek()
        {
            List<int> retValue = new System.Collections.Generic.List<int>();
            
            
            if (TueCheckbox.IsChecked.Value)
            {
                retValue.Add((int)DayOfWeek.Tuesday);
            }

            if (WedCheckbox.IsChecked.Value)
            {
                retValue.Add((int)DayOfWeek.Wednesday);
            }

            if (ThuCheckbox.IsChecked.Value)
            {
                retValue.Add((int)DayOfWeek.Thursday);
            }
            if (FriCheckbox.IsChecked.Value)
            {
                retValue.Add((int)DayOfWeek.Friday);
            }
            if (SatCheckbox.IsChecked.Value)
            {
                retValue.Add((int)DayOfWeek.Saturday);
            }
            if (MonCheckbox.IsChecked.Value)
            {
                retValue.Add((int)DayOfWeek.Monday);
            }
            if (SunCheckbox.IsChecked.Value)
            {
                retValue.Add((int)DayOfWeek.Sunday);
            }

            
            return retValue;
        }

        private async void LogInToWagtap()
        {
            //string LogLocation = "";
            //LogLocation = @"C:\Users\OluJerome\Documents\Visual Studio 2010\Projects\LearnCuDAVS2010\LearnCUDAConsoleApplication\WagTapCalLogs\";
            //Tiler.LogControl.UpdateLogLocation(LogLocation);
            
            //WebApp.Start<Startup>("http://localhost:9000");

            TilerFront.Models.LoginViewModel myLogin = new TilerFront.Models.LoginViewModel() { Username = UserNameTextBox.Text, Password = PasswordTextBox.Text, RememberMe = true };

            TilerFront.Models.AuthorizedUser AuthorizeUser = new TilerFront.Models.AuthorizedUser(){UserID="d350ba4d-fe0b-445c-bed6-b6411c2156b3",UserName="jerbio"};

            UserAccount currentUser =await AuthorizeUser.getUserAccountDebug();// new UserAccountDebug("18");
            //await currentUser.batchMigrateXML();
            
            
            //UserAccountDirect currentUser =  new UserAccountDebug("18");
            await currentUser.Login();
            DateTimeOffset refNow=DateTimeOffset.Now;
            //refNow = DateTimeOffset.Parse("8:26 am 10/02/2016");
            //MySchedule = new Schedule(currentUser, refNow);


            Stopwatch timer = new Stopwatch();
            timer.Start();
            MySchedule = new Schedule(currentUser, refNow);
            
            if (MySchedule.isScheduleLoadSuccessful)
            {
                timer.Stop();
                //MessageBox.Show("Ellapsed is " + timer.ElapsedMilliseconds + "ms");
                
                tabItem2.IsEnabled = true;
                datePicker1.SelectedDate = new DateTime(Schedule.Now.calculationNow.AddDays(0).ToLocalTime().Ticks);// DateTimeOffset.Now.AddDays(0);
                //datePicker1.SelectedDate = DateTimeOffset.Now.AddDays(0);
                //datePicker1.SelectedDate = new DateTimeOffset(2013, 11, 20, 0, 0, 0);
                //datePicker2.SelectedDate = DateTimeOffset.Now.AddDays(2);
                datePicker2.SelectedDate = new DateTime(Schedule.Now.calculationNow.AddDays(1).ToLocalTime().Ticks);//new DateTimeOffset(2014, 5, 15, 0, 0, 0);
                calendar4.SelectedDate = new DateTime(DateTimeOffset.Now.AddDays(0).ToLocalTime().Ticks);
                Random myNumber = new Random();
                int RandomHour = myNumber.Next(0, 24);
                int RandomMinute = myNumber.Next(0, 60);
                textBox4.Text = RandomHour + ":" + RandomMinute;

                textBox4.Text = 1+ ":" + "45" + ":" + "00";//total time
                textBox2.Text = 1.ToString();//number of splits
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
                //EventIDGenerator.Initialize((uint)(MySchedule.LastScheduleIDNumber));
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

        private void EncryptClicked_Click(object sender, RoutedEventArgs e)
        {
            //DBControl.UpdateAllUserPassword();
            /*UserAccount currentUser = new UserAccount(UserNameTextBox.Text, PasswordTextBox.Text);
            currentUser.Login();
            currentUser.EncryptPassword();*/
        }

        private async void LogInButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            //Register(string FirstName, string LastName, string NickName, string UserName, string PassWord)
            MessageBox.Show("Uhm Jerome remember you havent implemented this for tilerfront");
            return;
            
            /*
            UserAccount newUser = new UserAccount();
            ;
            if (!(await newUser.Register(FirstNameRegisterTextBox.Text, LastNameRegisterTextBox.Text, NickNameRegisterTextBox.Text, UserNameRegisterTextBox.Text, PasswordRegisterTextBox.Text)).Item2.Status)
            {
                UserNameTextBox.Text = UserNameRegisterTextBox.Text;
                PasswordTextBox.Text = PasswordRegisterTextBox.Text;
                LogInToWagtap();
            }*/
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /*
            IList<CalendarEvent> AllCalendarEvents=MySchedule. getAllCalendarElements().Select(obj=>obj.Value).ToList();
            string NameOfEVent=NameOfEventSearch.Text;
            NameOfEVent = NameOfEVent.ToLower();

            IEnumerable<CalendarEvent> WITHnAME = AllCalendarEvents.Where(obj => obj.Name.ToLower().Contains(NameOfEVent));
            string FinalBox = string.Join("\n", WITHnAME.Select(OBJ => OBJ.Name));
            ResultOfSearch.Text = FinalBox;
            */
        }

        private void ForceCheckBoxDetection(object sender, RoutedEventArgs e)
        {

        }

        private void ForceClickDetection(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            Location_Elements locationdata = new Location_Elements();
            MySchedule.FindMeSomethingToDo(locationdata).Wait();
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
        public SystemTimeUpdate(DateTimeOffset NewSystemTime)
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
            /// Convert form System.DateTimeOffset
            /// </summary>
            /// <param name="time"></param>
            public void FromDateTime(DateTimeOffset time)
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
            /// Convert to System.DateTimeOffset
            /// </summary>
            /// <returns></returns>
            public DateTimeOffset ToDateTime()
            {
                return new DateTimeOffset(wYear, wMonth, wDay, wHour, wMinute, wSecond, wMilliseconds, new TimeSpan());
            }
            /// <summary>
            /// STATIC: Convert to System.DateTimeOffset
            /// </summary>
            /// <param name="time"></param>
            /// <returns></returns>
            public static DateTimeOffset ToDateTime(SYSTEMTIME time)
            {
                return time.ToDateTime();
            }
        }
        



        //SetLocalTime C# Signature
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool SetLocalTime(ref SYSTEMTIME Time);
        /*{ return false; }*/

        //Example
        public static void UpdateSystemTime(DateTimeOffset NewTime)
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
