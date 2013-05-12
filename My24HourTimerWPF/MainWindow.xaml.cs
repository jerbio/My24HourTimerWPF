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

namespace My24HourTimerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        Schedule MySchedule;
        public MainWindow()
        {
            InitializeComponent();
            calendar2.SelectedDate = DateTime.Now;
            calendar1.SelectedDate = DateTime.Now;
            MySchedule = new Schedule();
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
                textBlock2.Text = "Next Activity is : " + MySchedule.getMyCalendarEvent(NextActivity.TimeLineID).Name;
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
            
            DateTime eventStartDate = (DateTime)calendar2.SelectedDate.Value;
            string eventEndTime = textBox7.Text;
            DateTime eventEndDate = (DateTime)calendar1.SelectedDate.Value;
            bool EventRepetitionflag = checkBox2.IsChecked.Value;
            bool DefaultPrepTimeflag = checkBox3.IsChecked.Value;
            string eventPrepTime = textBox3.Text;
            bool RigidScheduleFlag = checkBox5.IsChecked.Value;
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
            if (eventEndTime == "")
            {
                MessageBox.Show("Please Type EndTime in The Format: HH:MM A/PM");
                return;
            }
            //CalendarEvent ScheduleUpdated = CreateSchedule(eventName, eventStartTime, eventStartDate, eventEndTime, eventEndDate, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionflag, DefaultPreDeadlineFlag, RigidScheduleFlag, eventPrepTime, DefaultPreDeadlineFlag);
            CalendarEvent ScheduleUpdated = new CalendarEvent(eventName, eventStartTime, eventStartDate, eventEndTime, eventEndDate, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionflag, DefaultPreDeadlineFlag, RigidScheduleFlag, eventPrepTime, DefaultPreDeadlineFlag);
            MySchedule.AddToSchedule(ScheduleUpdated);
            textBlock9.Text = "Schedule Updated";
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
            CalendarEvent MyCalendarEvent = new CalendarEvent(Name, Duration, StartDate, EventEndDate, PrepTime, PreDeadline, checkBox5.IsChecked.Value, checkBox2.IsChecked.Value, Split);
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

    }


    public class TimeLine
    {
        protected DateTime EndTime;
        protected DateTime StartTime;
        protected BusyTimeLine[] ActiveTimeSlots;
        

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

    public static class EventIDGenerator
    {
        static uint idcounter = 0;
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

        public override string ToString()
        {
            string IDCombination="";
            foreach (string MyString in LayerID)
            {
                IDCombination += MyString + "_";
            }
            return IDCombination.Substring(0, (IDCombination.Length - 1));
        }
    }

    public class CalendarEvent
    {
        // Fields
        protected DateTime DeadlineDateTime;
        protected TimeSpan EventDuration;
        string CalendarEventName;
        protected DateTime StartDateTime;
        protected DateTime EndDateTime;
        protected TimeSpan EventPreDeadline;
        protected TimeSpan PrepTime;
        protected int Priority;
        protected bool RepetitionFlag;
        //protected bool Completed = false;
        protected bool RigidSchedule;
        protected int Splits;
        protected int TimePerSplit;
        protected EventID CalendarEventID;
        protected static TimeLine EventSequence;
        protected SubCalendarEvent SubEvent;
        SubCalendarEvent[] ArrayOfSubEvents;
        protected bool SchedulStatus;

        public CalendarEvent()
        { 
            
        }

        //CalendarEvent MyCalendarEvent = new CalendarEvent(NameEntry, Duration, StartDate, EndDate, PrepTime, PreDeadline, Rigid, Repeat, Split);
        public CalendarEvent(string EventIDEntry, string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, bool EventRepetitionflag, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag): this(new ConstructorModified(EventIDEntry, NameEntry, StartTime, StartDateEntry, EndTime, EventEndDateEntry, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionflag, DefaultPrepTimeflag, RigidScheduleFlag, eventPrepTime, PreDeadlineFlag), new EventID(EventIDEntry.Split('_')))
        {}


        public CalendarEvent(string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, bool EventRepetitionflag, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag):this(new ConstructorModified(NameEntry, StartTime, StartDateEntry, EndTime, EventEndDateEntry, eventSplit, PreDeadlineTime, EventDuration, EventRepetitionflag, DefaultPrepTimeflag, RigidScheduleFlag, eventPrepTime, PreDeadlineFlag))
        {
        }

        public CalendarEvent(CalendarEvent MyUpdated, SubCalendarEvent[] MySubEvents)
        {
            CalendarEventName = MyUpdated.Name;
            StartDateTime = MyUpdated.StartDateTime;
            EndDateTime = MyUpdated.DeadlineDateTime;
            EventDuration = MyUpdated.ActiveDurationSpan;
            Splits = MyUpdated.Splits;
            PrepTime = MyUpdated.PrepTime;
            EventPreDeadline = new TimeSpan(Convert.ToInt32(MyUpdated.PreDeadline.ToString())*10000000);
            RigidSchedule = MyUpdated.Rigid;
            TimePerSplit = MyUpdated.TimePerSplit;
            ArrayOfSubEvents = new SubCalendarEvent[Splits];
            if (MyUpdated.ID != null)
            {
                CalendarEventID = new EventID(MyUpdated.ID.Split('_'));
            }
            //CalendarEventID = new EventID(new string[] { EventIDGenerator.generate().ToString() });
            //ArrayOfSubEvents = generateSubEvent(ArrayOfSubEvents, 4, EventDuration, CalendarEventID.ToString());
            ArrayOfSubEvents = MySubEvents;
            SchedulStatus = false;
        }
        private CalendarEvent(ConstructorModified UpdatedConstructor,EventID MyEventID): this(MyEventID, UpdatedConstructor.Name, UpdatedConstructor.Duration, UpdatedConstructor.StartDate, UpdatedConstructor.EndDate, UpdatedConstructor.PrepTime, UpdatedConstructor.PreDeadline, UpdatedConstructor.Rigid, UpdatedConstructor.Repeat, UpdatedConstructor.Split)
        {
        }

        private CalendarEvent(ConstructorModified UpdatedConstructor):this(UpdatedConstructor.Name,UpdatedConstructor.Duration,UpdatedConstructor.StartDate,UpdatedConstructor.EndDate,UpdatedConstructor.PrepTime,UpdatedConstructor.PreDeadline,UpdatedConstructor.Rigid,UpdatedConstructor.Repeat,UpdatedConstructor.Split)
        {
        }
        static public string convertTimeToMilitary(string TimeString)
        {
            TimeString = TimeString.Replace(" ", "").ToUpper();
            string[] TimeIsolated = TimeString.Split(':');
            if (TimeIsolated.Length == 2)//checks if time is in format HH:MMAM as opposed to HH:MM:SSAM 
            {
                char AorP = TimeIsolated[1][2];
                TimeIsolated[1] = TimeIsolated[1].Substring(0, 2) + ":00" + AorP + "M";
                return convertTimeToMilitary(TimeIsolated[0] + ":" + TimeIsolated[1]);

            }
            int HourInt = Convert.ToInt32(TimeIsolated[0]);
            if (TimeIsolated[2][2] == 'P')
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
            TimeIsolated[1] = TimeIsolated[1].Substring(0, 2);
            TimeString = TimeIsolated[0] + ":" + TimeIsolated[1];

            //TimeString=TimeString.Substring(0, 5);
            return TimeString;
        }
        
        private class ConstructorModified
        {
            public string Name;
            public TimeSpan Duration;
            public DateTime StartDate;
            public DateTime EndDate;
            public TimeSpan PrepTime;
            public TimeSpan PreDeadline;
            public bool Rigid;
            public bool Repeat;
            public int Split;//Make Sure this is UInt
            public EventID CalendarEventID;

            public ConstructorModified(string EventIDEntry, string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, bool EventRepetitionflag, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag)
            {
                CalendarEventID = new EventID(EventIDEntry.Split('_'));
                Name = NameEntry;
                EventDuration = EventDuration + ":00";
                string MiltaryStartTime = convertTimeToMilitary(StartTime);
                StartDate = new DateTime(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(MiltaryStartTime.Split(':')[0]), Convert.ToInt32(MiltaryStartTime.Split(':')[1]), 0);
                string MiltaryEndTime = convertTimeToMilitary(EndTime);
                EndDate = new DateTime(EventEndDateEntry.Year, EventEndDateEntry.Month, EventEndDateEntry.Day, Convert.ToInt32(MiltaryEndTime.Split(':')[0]), Convert.ToInt32(MiltaryEndTime.Split(':')[1]), 0);
                string[] TimeDuration = EventDuration.Split(':');
                uint AllMinutes = ConvertToMinutes(EventDuration);
                Duration = new TimeSpan((int)(AllMinutes / 60), (int)(AllMinutes % 60), 0);
                Split = Convert.ToInt32(eventSplit);
                if (PreDeadlineFlag)
                {
                    PreDeadline = new TimeSpan(((int)AllMinutes % 10) * 60);
                }
                else
                {
                    PreDeadline = new TimeSpan(Convert.ToInt64(PreDeadlineTime));
                }
                if (DefaultPrepTimeflag)
                {
                    PrepTime = new TimeSpan(15 * 60);
                }
                else
                {
                    //uint MyNumber = Convert.ToInt32(eventPrepTime);
                    PrepTime = new TimeSpan((long)ConvertToMinutes(eventPrepTime) * 60 * 10000000);
                }
                Rigid = RigidScheduleFlag;
                Repeat = EventRepetitionflag;
            }
            public ConstructorModified(string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, bool EventRepetitionflag, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag)
            {
                Name = NameEntry;
                EventDuration = EventDuration + ":00";
                string MiltaryStartTime = convertTimeToMilitary(StartTime);
                StartDate= new DateTime(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(MiltaryStartTime.Split(':')[0]), Convert.ToInt32(MiltaryStartTime.Split(':')[1]), 0);
                string MiltaryEndTime = convertTimeToMilitary(EndTime);
                EndDate = new DateTime(EventEndDateEntry.Year, EventEndDateEntry.Month, EventEndDateEntry.Day, Convert.ToInt32(MiltaryEndTime.Split(':')[0]), Convert.ToInt32(MiltaryEndTime.Split(':')[1]), 0);
                string[] TimeDuration = EventDuration.Split(':');
                uint AllMinutes = ConvertToMinutes(EventDuration);
                Duration = new TimeSpan((int)(AllMinutes / 60), (int)(AllMinutes % 60), 0);
                Split = Convert.ToInt32(eventSplit);
                if (PreDeadlineFlag)
                {
                    PreDeadline = new TimeSpan(((int)AllMinutes % 10) * 60);
                }
                else
                {
                    PreDeadline = new TimeSpan(Convert.ToInt64( PreDeadlineTime));
                }
                if (DefaultPrepTimeflag)
                {
                    PrepTime = new TimeSpan(15 * 60);
                }
                else
                {
                    //uint MyNumber = Convert.ToInt32(eventPrepTime);
                    PrepTime = new TimeSpan((long)ConvertToMinutes(eventPrepTime) * 60 * 10000000);
                }
                Rigid = RigidScheduleFlag;
                Repeat = EventRepetitionflag;
            }

            static public uint ConvertToMinutes(string  TimeEntry)
            {
                int MaxTimeIndexCounter = 5;
                string[] ArrayOfTimeComponent = TimeEntry.Split(':');
                Array.Reverse(ArrayOfTimeComponent);
                uint TotalMinutes=0;
                for (int x=0; x<ArrayOfTimeComponent.Length;x++)
                {
                    int Multiplier=0;
                    switch (x)
                    {
                        case 0:
                            Multiplier = 0;
                            break;
                        case 1:
                            Multiplier=1;
                            break;
                        case 2:
                            Multiplier=60;
                            break;
                        case 3:
                            Multiplier=36*24;
                            break;
                        case 4:
                            Multiplier=36*24*365;
                            break;
                    }
                    string JustHold = ArrayOfTimeComponent[x];
                    Int64 MyNumber = (Int64)Convert.ToDouble(JustHold);
                    TotalMinutes = (uint)(TotalMinutes + (Multiplier * MyNumber));
                    
                }

                return TotalMinutes;
                
            }

            string convertTimeToMilitary(string TimeString)
            {
                TimeString = TimeString.Replace(" ", "").ToUpper();
                string[] TimeIsolated = TimeString.Split(':');
                if (TimeIsolated.Length == 2)//checks if time is in format HH:MMAM as opposed to HH:MM:SSAM 
                {
                    char AorP = TimeIsolated[1][2];
                    TimeIsolated[1] = TimeIsolated[1].Substring(0, 2) + ":00" + AorP+"M";
                    return convertTimeToMilitary(TimeIsolated[0] + ":" + TimeIsolated[1]);
                    
                }
                int HourInt = Convert.ToInt32(TimeIsolated[0]);
                if (TimeIsolated[2][2] == 'P')
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
                TimeIsolated[1] = TimeIsolated[1].Substring(0, 2);
                TimeString = TimeIsolated[0] + ":" + TimeIsolated[1];

                //TimeString=TimeString.Substring(0, 5);
                return TimeString;
            }
        }
        protected DateTime [] getActiveSlots()
        {
            return new DateTime[0];
        }


        public CalendarEvent(EventID EventIDEntry, string EventName, TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan Event_PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit)
        {
            CalendarEventName = EventName;
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = Event_Duration;
            Splits = EventSplit;
            PrepTime = EventPrepTime;
            EventPreDeadline = Event_PreDeadline;
            RigidSchedule = EventRigidFlag;
            TimePerSplit = EventDuration.Minutes / Splits;
            ArrayOfSubEvents = new SubCalendarEvent[Splits];
            CalendarEventID = EventIDEntry;
            //ArrayOfSubEvents = generateSubEvent(ArrayOfSubEvents, 4, EventDuration, CalendarEventID.ToString());
        }

        public CalendarEvent(string EventName, TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan Event_PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit)
        {
//string eventName, TimeSpan EventDuration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit
            CalendarEventName = EventName;
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = Event_Duration;
            Splits = EventSplit;
            PrepTime = EventPrepTime;
            EventPreDeadline = Event_PreDeadline;
            RigidSchedule = EventRigidFlag;
            TimePerSplit = EventDuration.Minutes / Splits;
            ArrayOfSubEvents = new SubCalendarEvent[Splits];
            CalendarEventID = new EventID(new string[] { EventIDGenerator.generate().ToString() });
            //ArrayOfSubEvents = generateSubEvent(ArrayOfSubEvents, 4, EventDuration, CalendarEventID.ToString());
        }

        SubCalendarEvent[] generateSubEvent(SubCalendarEvent[] ArrayOfEvents, int NumberOfSplit, TimeSpan TotalActiveDurationSubEvents, string ParentID)
        {
            TimeSpan TimeSpanEvent = EndDateTime - StartDateTime;
         //       new TimeSpan((long)((().TotalSeconds/ ArrayOfEvents.Length)*100000000));
            TimeSpanEvent = new TimeSpan(((long)TimeSpanEvent.TotalMilliseconds * 10000) / ArrayOfEvents.Length);
            TimeSpan ActiveDurationPerSubEvents = new TimeSpan((long)(((TotalActiveDurationSubEvents.TotalSeconds)*10000000) / ArrayOfEvents.Length));
            DateTime SubStart;
            DateTime SubEnd;
            for (int i = 0; i < ArrayOfEvents.Length;i++ )
            {
                SubStart=StartDateTime.AddSeconds(TimeSpanEvent.TotalSeconds * i);
                SubEnd = StartDateTime.AddSeconds(TimeSpanEvent.TotalSeconds * (i + 1));
                ArrayOfEvents[i] = new SubCalendarEvent(ActiveDurationPerSubEvents, SubStart, SubEnd, PrepTime, ParentID);
            }

            return ArrayOfEvents;
        }

        public CalendarEvent GetAllScheduleEventsFromXML()
        {
            XmlTextReader reader = new XmlTextReader("MyEventLog.xml");
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        Console.Write("<" + reader.Name);
                        Console.WriteLine(">");
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        Console.WriteLine(reader.Value);
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        Console.Write("</" + reader.Name);
                        Console.WriteLine(">");
                        break;
                }
            }
            return new CalendarEvent();
        }

        //CalendarEvent Properties
        public string ID
        {
            get 
            {
                return CalendarEventID.ToString();
            }
        }

        public string Name
        {
            get 
            {
                return CalendarEventName;
            }
        }

        public TimeSpan TimeLeftBeforeDeadline
        {
            get
            {
                return EndDateTime - DateTime.Now;
            }
        }

        public DateTime Start
        {
            get
            {
                return StartDateTime;
            }
        }

        public DateTime End
        {
            get
            {
                return EndDateTime;
            }
        }

        public int NumberOfSplit
        {
            get
            {
                return Splits;
            }
        }

        public bool Rigid
        {
            get
            {
                return RigidSchedule;
            }
        }

        public bool Repetition
        {
            get
            {
                return RepetitionFlag;
            }
        }

        public bool Completed
        {
            get
            {
                if (DateTime.Now > EndDateTime)
                {
                    return false;
                }
                else 
                {
                    return true;
                }
            }
        }

        public TimeSpan Preparation
        {
            get
            {
                return PrepTime;
            }
        }

        public string PreDeadline
        {
            get
            {
                return EventPreDeadline.Seconds.ToString();
            }
        }

        public string ActiveDuration
        {
            get
            {
                return EventDuration.Seconds.ToString();
            }
        }

        public TimeSpan ActiveDurationSpan
       {
           get
           {
               return EventDuration;
           }
       }

        public SubCalendarEvent [] AllEvents
        {
            get
            {
                return ArrayOfSubEvents;
            }
        }
    }

    public class Repetition
    {
        public Repetition()
        { 
        
        }
    }

    public class Schedule
    {
        Dictionary<string, CalendarEvent> AllEventDictionary;
        TimeLine CompleteSchedule;

        public Schedule ()
        {
            AllEventDictionary = getAllCalendarFromXml();
            CompleteSchedule=getTimeLine();
        }


        public CalendarEvent getMyCalendarEvent(string EventID)
        {
            return AllEventDictionary[new EventID(EventID).ID[0]];
        }

        public CalendarEvent getMyCalendarEvent(EventID myEventID)
        {
            return AllEventDictionary[myEventID.ID[0]];
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
                MyTotalSubEvents = Schedule.SortMyEvents(MyTotalSubEvents);
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
            List<BusyTimeLine> MyTotalSubEvents=new List<BusyTimeLine>(0);
            foreach (KeyValuePair<string, CalendarEvent> MyCalendarEvent in AllEventDictionary)
            { 
                //LastDeadline
                foreach (SubCalendarEvent MySubCalendarEvent in MyCalendarEvent.Value.AllEvents)
                {
                    if (MySubCalendarEvent.End > LastDeadline)
                    {
                        LastDeadline = MySubCalendarEvent.End;
                    }
                    MyTotalSubEvents.Add(MySubCalendarEvent.ActiveSlot);
                }
            }
            
            TimeLine MyTimeLine= new TimeLine(DateTime.Now, LastDeadline);
            MyTimeLine.OccupiedSlots = MyTotalSubEvents.ToArray();
            return MyTimeLine;
        }
        
        Dictionary<string, CalendarEvent> getAllCalendarFromXml()
        {
            Dictionary<string, CalendarEvent> MyCalendarEventDictionary=new Dictionary<string, CalendarEvent>();
            XmlDocument doc = new XmlDocument();
            
            doc.Load("MyEventLog.xml");
            XmlNode node = doc.DocumentElement.SelectSingleNode("/ScheduleLog/LastIDCounter");
            string LastUsedIndex = node.InnerText;
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
                Name = EventScheduleNode.SelectSingleNode("Name").InnerText;
                ID=EventScheduleNode.SelectSingleNode("ID").InnerText;
                //EventScheduleNode.SelectSingleNode("ID").InnerXml = "<wetin></wetin>";
                Deadline = EventScheduleNode.SelectSingleNode("Deadline").InnerText;
                Rigid = EventScheduleNode.SelectSingleNode("RigidFlag").InnerText;
                Split= EventScheduleNode.SelectSingleNode("Split").InnerText;
                PreDeadline = EventScheduleNode.SelectSingleNode("PreDeadline").InnerText;
                //PreDeadlineFlag = EventScheduleNode.SelectSingleNode("PreDeadlineFlag").InnerText;
                CalendarEventDuration = EventScheduleNode.SelectSingleNode("Duration").InnerText;
                //EventRepetitionflag = EventScheduleNode.SelectSingleNode("RepetitionFlag").InnerText;
                //PrepTimeFlag = EventScheduleNode.SelectSingleNode("PrepTimeFlag").InnerText;
                PrepTime = EventScheduleNode.SelectSingleNode("PrepTime").InnerText;
                Completed = EventScheduleNode.SelectSingleNode("Completed").InnerText;
                StartDateTime=EventScheduleNode.SelectSingleNode("StartTime").InnerText.Split(' ');
                StartDate=StartDateTime[0];
                StartTime=StartDateTime[1]+StartDateTime[2];
                EndDateTime = EventScheduleNode.SelectSingleNode("Deadline").InnerText.Split(' ');
                EndDate = EndDateTime[0];
                EndTime = EndDateTime[1] + EndDateTime[2];
                //string Name, string StartTime, DateTime StartDate, string EndTime, DateTime EventEndDate, string eventSplit, string PreDeadlineTime, string EventDuration, bool EventRepetitionflag, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag
                DateTime StartTimeConverted = new DateTime(Convert.ToInt32(StartDate.Split('/')[2]), Convert.ToInt32(StartDate.Split('/')[0]), Convert.ToInt32(StartDate.Split('/')[1]));
                DateTime EndTimeConverted = new DateTime(Convert.ToInt32(EndDate.Split('/')[2]), Convert.ToInt32(EndDate.Split('/')[0]), Convert.ToInt32(EndDate.Split('/')[1]));
                //MainWindow.CreateSchedule("","",new DateTime(),"",new DateTime(),"","","",true,true,true,"",false);
                CalendarEvent RetrievedEvent = new CalendarEvent(ID, Name, StartTime, StartTimeConverted, EndTime, EndTimeConverted, Split, PreDeadline, CalendarEventDuration, false, false, Convert.ToBoolean(Rigid), PrepTime, false);
                RetrievedEvent = new CalendarEvent(RetrievedEvent, ReadSubSchedulesFromXMLNode(EventScheduleNode.SelectSingleNode("EventSubSchedules"), RetrievedEvent));
                //                MyCalendarEventDictionary.Add(
                MyCalendarEventDictionary.Add(RetrievedEvent.ID, RetrievedEvent);
            }

            return MyCalendarEventDictionary;
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
                MyArrayOfNodes[i] = new SubCalendarEvent(ID,BusySlot,Start,End,PrepTime,MyParent.ID);
            }

            return MyArrayOfNodes;
        }


        DateTime stringToDateTime(string MyDateTimeString)//String should be in format "MM/DD/YY HH:MM:SSA"
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
            NewEvent=UpdateTimeline(NewEvent);
            WriteToLog(NewEvent);
            AllEventDictionary.Add(NewEvent.ID, NewEvent);
            return true;
        }

        public CalendarEvent UpdateTimeline(CalendarEvent MyEvent)
        {
            BusyTimeLine [] AllOccupiedSlot= CompleteSchedule.OccupiedSlots;
            TimeSpan TotalActiveDuration=new TimeSpan();
            TimeLine[] TimeLineArrayWithSubEventsAssigned = new TimeLine[MyEvent.AllEvents.Length];
            
            TimeLine[] AllAvailableFreeSpots= getAllFreeSpots(new TimeLine(MyEvent.Start,MyEvent.End));
            int i = 0;
            TimeSpan TotalFreeTimeAvailable=new TimeSpan();
            for (i = 0; i < AllAvailableFreeSpots.Length; i++)
            {
                TotalFreeTimeAvailable+=AllAvailableFreeSpots[i].TimelineSpan;
            }
            if (TotalFreeTimeAvailable > MyEvent.ActiveDurationSpan)
            {
                //MyEvent.
                TimeLineArrayWithSubEventsAssigned = SplitFreeSpotsInToSubEventTimeSlots(AllAvailableFreeSpots, MyEvent.AllEvents.Length, MyEvent.ActiveDurationSpan);
            }
            else
            {
                MessageBox.Show("Sorry It Wont Fit, trying something else!!!");
            }
            SubCalendarEvent TempSubEvent;
            BusyTimeLine MyTempBusyTimerLine;
            for (; i < MyEvent.AllEvents.Length; i++)
            {
                //public SubCalendarEvent(TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID)
                TempSubEvent = new SubCalendarEvent(TimeLineArrayWithSubEventsAssigned[i].TimelineSpan, TimeLineArrayWithSubEventsAssigned[i].Start.Add(-MyEvent.Preparation), TimeLineArrayWithSubEventsAssigned[i].End, MyEvent.Preparation, MyEvent.ID);
                MyTempBusyTimerLine=new BusyTimeLine(TempSubEvent.ID, TimeLineArrayWithSubEventsAssigned[i].Start, TimeLineArrayWithSubEventsAssigned[i].End);
                TempSubEvent = new SubCalendarEvent(MyEvent.AllEvents[i].ID, TimeLineArrayWithSubEventsAssigned[i].Start.Add(-MyEvent.Preparation), TimeLineArrayWithSubEventsAssigned[i].End, MyTempBusyTimerLine);
                MyEvent.AllEvents[i] = TempSubEvent;
            }

            return MyEvent;
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
                MyArrayOfToBeAssignedTimeLine = IdealAllotAndInsert(TimeLineAndFitCount, MyArrayOfToBeAssignedTimeLine, new TimeSpan((long)IdealTimePerAllotment * 10000));
            }
            else 
            {
                MessageBox.Show("Sorry you will need to implement Dress funtionality to afford space.!!!");
            }

            return MyArrayOfToBeAssignedTimeLine;
        }

        public TimeLine[] IdealAllotAndInsert(Dictionary<TimeLine, int> AvailablFreeSpots, TimeLine[] MyArrayOfToBeAssignedTimeLine,TimeSpan IdealTimePerAllotment)
        {
            int i = 0;
            int j=0;
            int TopCounter = 0;
            TimeLine[] ArrayOfTimelineRanges = AvailablFreeSpots.Keys.ToArray();//array list of FreeTimeLineRanges
            Dictionary<TimeLine, TimeLine[]> TimeLineDictionary = new Dictionary<TimeLine, TimeLine[]>();
            int[] ArrayOfFitCount= AvailablFreeSpots.Values.ToArray();//array list of fit count
            for (; i < MyArrayOfToBeAssignedTimeLine.Length;i++)
            { 
                j=0;
                TopCounter++;
                for (; ((j < ArrayOfTimelineRanges.Length) && (i < MyArrayOfToBeAssignedTimeLine.Length)); j++)
                {
                    if (AvailablFreeSpots[ArrayOfTimelineRanges[j]] > 0)
                    {
                        
                        TimeLineDictionary.Add(ArrayOfTimelineRanges[j], SplitAndAssign(ArrayOfTimelineRanges[j], TopCounter, IdealTimePerAllotment));
                        --AvailablFreeSpots[ArrayOfTimelineRanges[j]];
                        i += TopCounter;
                    }
                }
            }

            TimeLine[][] MyArrayOfTimeLines=TimeLineDictionary.Values.ToArray();

            int k = 0;
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
                if (AvailableFreeSpots[i].TimelineSpan > (MySubEvent.ActiveDurationSpan))
                {
                    DateTime DurationStartTime;
                    TimeSpan MyTimeSpan = new TimeSpan((((AvailableFreeSpots[i].TimelineSpan - MySubEvent.ActiveDurationSpan).Milliseconds)/2)*10000);
                    DurationStartTime=AvailableFreeSpots[i].Start.Add(MyTimeSpan);
                    MySubEvent.ActiveSlot = new BusyTimeLine(MySubEvent.ID, DurationStartTime, DurationStartTime.Add(MySubEvent.ActiveDurationSpan));
                }
            } 
            return MySubEvent;
        }


        static public List<BusyTimeLine> QuickSortFunction(List<BusyTimeLine> MyArray,int LeftIndexPassed,int RightIndexPassed,int PivotPassed)
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
            RightIndex = GetRightThatsLess(MyArray, PivotIndex, RightIndexPassed);
            LeftIndex = GetLeftThatsGreater(MyArray, PivotIndex, LeftIndexPassed);
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
                        if (MyArray[LeftIndexPassed].Start > MyArray[NextPivot].Start)
                        {
                            Holder = MyArray[LeftIndexPassed];
                            MyArray[LeftIndexPassed] = MyArray[NextPivot];
                            MyArray[NextPivot]= Holder;
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

        static public int GetLeftThatsGreater(List<BusyTimeLine> MyArray, int MyPivot, int LeftStartinPosition)
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

        static public int GetRightThatsLess(List<BusyTimeLine> MyArray, int MyPivot, int RightStartinPosition)
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


            InvadingEvents = SortMyEvents(InvadingEvents);


            return InvadingEvents.ToArray();
        }

        static public List<BusyTimeLine> SortMyEvents(List<BusyTimeLine> MyUnsortedEvents)
        {
            int MiddleRoundedDown = ((MyUnsortedEvents.Count - 1) / 2);
            return QuickSortFunction(MyUnsortedEvents, 0, MyUnsortedEvents.Count - 1, MiddleRoundedDown);
        }


        TimeLine[] getAllFreeSpots(TimeLine MyTimeLine)//Gets an array of all the freespots between busy spots in given time line
        {
            BusyTimeLine[] AllBusySlots = CompleteSchedule.OccupiedSlots;
            DateTime FinalCompleteScheduleDate;
            AllBusySlots=SortMyEvents(AllBusySlots.ToList()).ToArray();
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
                    AllFreeSlots[0] = new TimeLine(DateTime.Now, AllBusySlots[0].Start.AddMilliseconds(-1));
                    AllFreeSlots[0] = new TimeLine(AllBusySlots[0].End.AddMilliseconds(1), AllBusySlots[0].End.AddYears(10));
                }
                else 
                {
                    AllFreeSlots = new TimeLine[1];
                    AllFreeSlots[0] = new TimeLine(MyTimeLine.Start, MyTimeLine.End);
                }
                return AllFreeSlots;
            }
            for (int i = 0; i < (AllBusySlots.Length-1); i++)//Free Spots Are only between two busy Slots. So Index Counter starts from 1 get start of second busy
            {
                AllFreeSlots[i] = new TimeLine(AllBusySlots[i].End.AddMilliseconds(1), AllBusySlots[i + 1].Start.AddMilliseconds(-1));
            }
            FinalCompleteScheduleDate = CompleteSchedule.End;
            if (FinalCompleteScheduleDate < MyTimeLine.End)
            {
                FinalCompleteScheduleDate = MyTimeLine.End;
            }
            AllFreeSlots[AllBusySlots.Length-1] = new TimeLine(DateTime.Now, AllBusySlots[0].Start);
            AllFreeSlots[AllBusySlots.Length] = new TimeLine(AllBusySlots[AllBusySlots.Length - 1].End, FinalCompleteScheduleDate);
            List<TimeLine> SpecificFreeSpots = new List<TimeLine>(0);
            for (int i = 0; i < AllFreeSlots.Length; i++)//Free Spots Are only between two busy Slots. So Index Counter starts from 1 get start of second busy
            {
//                AllFreeSlots[i] = new TimeLine(AllBusySlots[i].End.AddMilliseconds(1), AllBusySlots[i + 1].Start.AddMilliseconds(-1));
                if ((MyTimeLine.Start <= AllFreeSlots[i].End) && (AllFreeSlots[i].Start < MyTimeLine.End))
                {
                    SpecificFreeSpots.Add(AllFreeSlots[i]);
                }
            }

            return SpecificFreeSpots.ToArray();
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
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Repetition.ToString();
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
            XmlNode SubScheduleNodes = MyEventScheduleNode.SelectSingleNode("EventSubSchedules");
            foreach (SubCalendarEvent MySubEvent in MyEvent.AllEvents)
            {
                SubScheduleNodes.PrependChild(xmldoc.CreateElement("EventSubSchedule"));
                SubScheduleNodes.ChildNodes[0].InnerXml=CreateSubScheduleNode(MySubEvent).InnerXml;
            }
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.ID;

            
            return MyEventScheduleNode;
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
    }

    public class SubCalendarEvent : CalendarEvent
    {
        EventID SubEventID;
        BusyTimeLine BusyFrame;
        TimeSpan AvailablePreceedingFreeSpace;
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
