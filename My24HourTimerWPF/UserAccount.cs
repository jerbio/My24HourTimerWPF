//#define readfromBeforeInsertionFixingStiticRestricted
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace My24HourTimerWPF
{
    public class UserAccount
    {
        public static string WagTapLogLocation = "WagTapCalLogs\\";
        LogControl UserLog;
        int ID;
        string Username;
        string Password;
        DBControl UserAccountDBAccess;
        public UserAccount(string DirectoryEntry="")
        {
            if (!string.IsNullOrEmpty(DirectoryEntry))
            {
                WagTapLogLocation = DirectoryEntry;
            }
            Username = "";
            Password = "";
        }

        public UserAccount(string UserName, string PassWord, string DirectoryEntry = "")
        {
            if (!string.IsNullOrEmpty(DirectoryEntry))
            {
                WagTapLogLocation = DirectoryEntry;
            }
            this.Username = UserName;
            this.Password = PassWord;
        }

        public UserAccount(string UserName, int UserID, string DirectoryEntry = "")
        {
            if (!string.IsNullOrEmpty(DirectoryEntry))
            {
                WagTapLogLocation = DirectoryEntry;
            }
            this.Username = UserName;
            this.ID = UserID;
            this.Password = "";
        }

        public bool Login()
        {
            if(ID==0)
            { 
                UserLog = new LogControl(Username, Password);
            }
            else
            {
                UserLog = new LogControl(Username, ID);
            }
            UserLog.Initialize();
            ID = UserLog.LoggedUserID;
            return UserLog.Status;
        }

        public bool Register(string FirstName, string LastName, string Email, string UserName, string PassWord)
        {
            bool retValue = false;
            UserAccountDBAccess = new DBControl(UserName, PassWord);
            Tuple<bool, int> registrationStatus = UserAccountDBAccess.RegisterUser(FirstName, LastName, Email, UserName, PassWord);

            UserLog = new LogControl(UserName,PassWord);
            UserLog.Initialize();
            if (registrationStatus.Item1)
            {
                Username = UserName;
                Password = PassWord;
                retValue =UserLog.genereateNewLogFile(registrationStatus.Item2);
            }

            return retValue;
        }

        public Dictionary<string, CalendarEvent>  getAllCalendarElements()
        {
            Dictionary<string, CalendarEvent> retValue=new Dictionary<string,CalendarEvent>();

            if (UserLog.Status)
            {
                retValue = UserLog.getAllCalendarFromXml();
            }
            else
            {
                retValue = null;
            }
            return retValue;
        }

        public bool DeleteAllCalendarEvents()
        {
            bool retValue = false;
            
            if (UserLog.Status)
            {
                UserLog.EmptyCalendarXMLFile();
                retValue = true;
            }
            else
            {
                retValue = false;
            }
            return retValue;
        }

        public void CommitEventToLog(CalendarEvent MyCalEvent)
        {
            UserLog.WriteToLog(MyCalEvent);
        }
        
        #region LogControl Class
        class LogControl
        {
            int ID;
            string UserName;


            DBControl LogDBDataAccess;
            string LastIDNumber;
            string CurrentLog;

            bool LogStatus;


            public LogControl(string UserName, string Password)
            {
                LogDBDataAccess = new DBControl(UserName, Password);
                LogStatus = false;
            }

            public LogControl(string UserName, int UserID)
            {
                LogDBDataAccess = new DBControl(UserName, UserID);
                LogStatus = false;
            }

            public void Initialize()
            {
                Tuple<bool, int> VerifiedUser = LogDBDataAccess.LogIn();
                CurrentLog = "";
                if (VerifiedUser.Item1)
                {
                    ID = VerifiedUser.Item2;
                    CurrentLog = ID.ToString() + ".xml";
                    LogStatus = File.Exists(WagTapLogLocation + CurrentLog);
                }
            }


            #region Write Data

            public bool genereateNewLogFile(int UserID)//creates a new xml log file. Uses the passed UserID
            {
                bool retValue = false;
                try
                {
                    
                    string NameOfFile = WagTapLogLocation + UserID.ToString() + ".xml";
                    if (File.Exists(NameOfFile))
                    {
                        File.Delete(NameOfFile);
                    }

                    FileStream myFileStream= File.Create(NameOfFile);
                    myFileStream.Close();
                    
                    CurrentLog = NameOfFile;
                    EmptyCalendarXMLFile();
                    
                    retValue = true;

                }
                catch (Exception e)
                {
                    retValue = false;
                }

                return retValue;

            }

            public void WriteToLog(CalendarEvent MyEvent, string LogFile = "")//writes to an XML Log file. Takes calendar event as an argument
            {
                if (LogFile == "")
                { LogFile = WagTapLogLocation + CurrentLog; }
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(LogFile);
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
                xmldoc.Save(LogFile);
            }

            

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
                MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Enabled"));
                MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.isEnabled.ToString();
                MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Location"));
                MyEventScheduleNode.ChildNodes[0].InnerXml = CreateLocationNode(MyEvent.myLocation, "EventScheduleLocation").InnerXml;
                MyEventScheduleNode.PrependChild(xmldoc.CreateElement("UIParams"));
                MyEventScheduleNode.ChildNodes[0].InnerXml = createDisplayUINode(MyEvent.UIParam, "UIParams").InnerXml;

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
                foreach (SubCalendarEvent MySubEvent in MyEvent.AllSubEvents)
                {
                    SubScheduleNodes.PrependChild(xmldoc.CreateElement("EventSubSchedule"));
                    SubScheduleNodes.ChildNodes[0].InnerXml = CreateSubScheduleNode(MySubEvent).InnerXml;
                }
                //MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.ID;


                return MyEventScheduleNode;
            }

            public DateTime Truncate(DateTime dateTime, TimeSpan timeSpan)
            {
                if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
                return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
            }

            public XmlElement CreateRepetitionNode(Repetition RepetitionObjEntry)//This takes a repetition object, and creates a Repetition XmlNode
            {
                int i = 0;
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
                    RepeatCalendarEventNode = xmldoc.CreateElement("RepeatCalendarEvent");
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

                DateTime StartTime = MySubEvent.Start;
                StartTime = Truncate(StartTime, TimeSpan.FromSeconds(1));
                DateTime EndTime = MySubEvent.End;
                EndTime = Truncate(EndTime, TimeSpan.FromSeconds(1));
                TimeSpan EventTimeSpan = MySubEvent.ActiveDuration;
                long AllSecs = (long)EventTimeSpan.TotalSeconds;
                long AllTicks = (long)EventTimeSpan.TotalMilliseconds;
                long DiffSecs = (long)(EndTime - StartTime).TotalSeconds;
                long DiffTicks = (long)(EndTime - StartTime).TotalMilliseconds;
                EventTimeSpan = new TimeSpan(AllSecs * 10000000);
                if ((EndTime - StartTime) != EventTimeSpan)
                {
                    EndTime = StartTime.Add(EventTimeSpan);
                }





                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("EndTime"));
                MyEventSubScheduleNode.ChildNodes[0].InnerText = EndTime.ToString();
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("StartTime"));
                MyEventSubScheduleNode.ChildNodes[0].InnerText = StartTime.ToString();
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Duration"));
                MyEventSubScheduleNode.ChildNodes[0].InnerText = EventTimeSpan.ToString();
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ActiveEndTime"));
                MyEventSubScheduleNode.ChildNodes[0].InnerText = EndTime.ToString();
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ActiveStartTime"));
                MyEventSubScheduleNode.ChildNodes[0].InnerText = StartTime.ToString();
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("PrepTime"));
                MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.Preparation.ToString();
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ThirdPartyID"));
                MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.ThirdPartyID;
                //MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Name"));
                //MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.Name.ToString();
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ID"));
                MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.ID.ToString();
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Enabled"));
                MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.isEnabled.ToString();
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Location"));
                MyEventSubScheduleNode.ChildNodes[0].InnerXml = CreateLocationNode(MySubEvent.myLocation, "EventSubScheduleLocation").InnerXml;
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("UIParams"));
                MyEventSubScheduleNode.ChildNodes[0].InnerXml = createDisplayUINode(MySubEvent.UIParam, "UIParams").InnerXml;
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
                    Descripion = Arg1.Description;
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
                        MyLogList[i].InnerXml = UpdatedData.InnerXml;
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

            public XmlElement createDisplayUINode(EventDisplay Arg1, string ElementIdentifier)
            {
                XmlDocument xmldoc = new XmlDocument();
                XmlElement var1 = xmldoc.CreateElement(ElementIdentifier);
                var1.PrependChild(xmldoc.CreateElement("Visible"));
                var1.ChildNodes[0].InnerText = Arg1.isVisible.ToString();
                var1.PrependChild(xmldoc.CreateElement("Color"));
                var1.ChildNodes[0].InnerXml = createColorNode(Arg1.UIColor, "Color").InnerXml;
                var1.PrependChild(xmldoc.CreateElement("Default"));
                var1.ChildNodes[0].InnerText = Arg1.isDefault.ToString();
                return var1;
            }

            public XmlElement createColorNode(Color Arg1, string ElementIdentifier)
            {
                XmlDocument xmldoc = new XmlDocument();
                XmlElement var1 = xmldoc.CreateElement(ElementIdentifier);
                var1.PrependChild(xmldoc.CreateElement("Opacity"));
                var1.ChildNodes[0].InnerText = Arg1.Opacity.ToString();
                var1.PrependChild(xmldoc.CreateElement("Red"));
                var1.ChildNodes[0].InnerText = Arg1.R.ToString();
                var1.PrependChild(xmldoc.CreateElement("Green"));
                var1.ChildNodes[0].InnerText = Arg1.G.ToString();
                var1.PrependChild(xmldoc.CreateElement("Blue"));
                var1.ChildNodes[0].InnerText = Arg1.B.ToString();

                return var1;
            }




            public void EmptyCalendarXMLFile()
            {

                File.WriteAllText(WagTapLogLocation+ CurrentLog, "<?xml version=\"1.0\" encoding=\"utf-8\"?><ScheduleLog><LastIDCounter>0</LastIDCounter><EventSchedules></EventSchedules></ScheduleLog>");
            }
            #endregion



            #region Read Data

            public Dictionary<string, CalendarEvent> getAllCalendarFromXml(string NameOfFile = "")
            {
                Dictionary<string, CalendarEvent> MyCalendarEventDictionary = new Dictionary<string, CalendarEvent>();
                XmlDocument doc = new XmlDocument();

                if (string.IsNullOrEmpty(NameOfFile))
                {
                    //NameOfFile = "MyEventLog.xml";
                    NameOfFile = WagTapLogLocation + CurrentLog;
                }
#if readfromBeforeInsertionFixingStiticRestricted
            NameOfFile = WagTapLogLocation+"BeforeInsertionFixingStiticRestricted.xml";
#endif
                doc.Load(NameOfFile);

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
                    RetrievedEvent = getCalendarEventObjFromNode(EventScheduleNode);
                    MyCalendarEventDictionary.Add(RetrievedEvent.ID, RetrievedEvent);
                }

                return MyCalendarEventDictionary;
            }

            public CalendarEvent getCalendarEventObjFromNode(XmlNode EventScheduleNode)
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
                string EnableFlag;

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
                    Recurrence = new Repetition();
                }
                Split = EventScheduleNode.SelectSingleNode("Split").InnerText;
                PreDeadline = EventScheduleNode.SelectSingleNode("PreDeadline").InnerText;
                //PreDeadlineFlag = EventScheduleNode.SelectSingleNode("PreDeadlineFlag").InnerText;
                CalendarEventDuration = EventScheduleNode.SelectSingleNode("Duration").InnerText;
                //EventRepetitionflag = EventScheduleNode.SelectSingleNode("RepetitionFlag").InnerText;
                //PrepTimeFlag = EventScheduleNode.SelectSingleNode("PrepTimeFlag").InnerText;
                PrepTime = EventScheduleNode.SelectSingleNode("PrepTime").InnerText;
                Completed = EventScheduleNode.SelectSingleNode("Completed").InnerText;
                EnableFlag = EventScheduleNode.SelectSingleNode("Enabled").InnerText;
                bool EVentEnableFlag = Convert.ToBoolean(EnableFlag);
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

                if (ID == "11")
                {
                    ;
                }


                CalendarEvent RetrievedEvent = new CalendarEvent(ID, Name, StartTime, StartTimeConverted, EndTime, EndTimeConverted, Split, PreDeadline, CalendarEventDuration, Recurrence, false, Convert.ToBoolean(Rigid), PrepTime, false, var3, EVentEnableFlag);
                RetrievedEvent = new CalendarEvent(RetrievedEvent, ReadSubSchedulesFromXMLNode(EventScheduleNode.SelectSingleNode("EventSubSchedules"), RetrievedEvent));
                return RetrievedEvent;
            }


            SubCalendarEvent[] ReadSubSchedulesFromXMLNode(XmlNode MyXmlNode, CalendarEvent MyParent)
            {
                SubCalendarEvent[] MyArrayOfNodes = new SubCalendarEvent[MyXmlNode.ChildNodes.Count];
                string ID = "";
                DateTime Start = new DateTime();
                DateTime End = new DateTime();
                TimeSpan SubScheduleDuration = new TimeSpan();
                TimeSpan PrepTime = new TimeSpan();
                BusyTimeLine BusySlot = new BusyTimeLine();
                bool Enabled;
                for (int i = 0; i < MyXmlNode.ChildNodes.Count; i++)
                {
                    BusyTimeLine SubEventActivePeriod = new BusyTimeLine(MyXmlNode.ChildNodes[i].SelectSingleNode("ID").InnerText, stringToDateTime(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveStartTime").InnerText), stringToDateTime(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveEndTime").InnerText));
                    ID = MyXmlNode.ChildNodes[i].SelectSingleNode("ID").InnerText;
                    Start = DateTime.Parse(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveStartTime").InnerText);
                    End = DateTime.Parse(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveEndTime").InnerText);
                    BusySlot = new BusyTimeLine(ID, Start, End);
                    PrepTime = new TimeSpan(ConvertToMinutes(MyXmlNode.ChildNodes[i].SelectSingleNode("PrepTime").InnerText) * 60 * 10000000);
                    //stringToDateTime();
                    Start = DateTime.Parse(MyXmlNode.ChildNodes[i].SelectSingleNode("StartTime").InnerText);
                    End = DateTime.Parse(MyXmlNode.ChildNodes[i].SelectSingleNode("EndTime").InnerText);
                    Enabled=Convert.ToBoolean(MyXmlNode.ChildNodes[i].SelectSingleNode("Enabled").InnerText);
                    Location var1 = getLocation(MyXmlNode.ChildNodes[i]);

                    MyArrayOfNodes[i] = new SubCalendarEvent(ID, BusySlot, Start, End, PrepTime, MyParent.ID, MyParent.Rigid, var1, MyParent.RangeTimeLine, Enabled);
                    MyArrayOfNodes[i].ThirdPartyID = MyXmlNode.ChildNodes[i].SelectSingleNode("ThirdPartyID").InnerText;//this is a hack to just update the Third partyID
                }

                return MyArrayOfNodes;
            }


            CalendarEvent[] getAllRepeatCalendarEvents(XmlNode RepeatEventSchedulesNode)
            {
                XmlNodeList ListOfRepeatEventScheduleNode = RepeatEventSchedulesNode.ChildNodes;
                List<CalendarEvent> ListOfRepeatCalendarNodes = new List<CalendarEvent>();
                int i = 0;
                foreach (XmlNode MyNode in ListOfRepeatEventScheduleNode)
                {
                    if (i == 40)
                    {
                        ;
                    }

                    ListOfRepeatCalendarNodes.Add(getCalendarEventObjFromNode(MyNode));
                    i++;
                }
                return ListOfRepeatCalendarNodes.ToArray();
            }




            Location getLocation(XmlNode Arg1)
            {
                XmlNode var1 = Arg1.SelectSingleNode("Location");
                if (var1 == null)
                {
                    return new Location();
                }
                else
                {
                    string XCoordinate_Str = var1.SelectSingleNode("XCoordinate").InnerText;
                    string YCoordinate_Str = var1.SelectSingleNode("YCoordinate").InnerText;
                    string Descripion = var1.SelectSingleNode("Description").InnerText;
                    Descripion = string.IsNullOrEmpty(Descripion) ? "" : Descripion;
                    string Address = var1.SelectSingleNode("Address").InnerText;
                    Address = string.IsNullOrEmpty(Address) ? "" : Address;

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

                        if (!(double.TryParse(YCoordinate_Str, out yCoOrdinate)))
                        {
                            yCoOrdinate = double.MaxValue;
                        }

                        return new Location(xCoOrdinate, yCoOrdinate, Address, Descripion);
                    }
                }


            }


            public EventDisplay getDisplayUINode(XmlNode Arg1)
            {
                XmlNode var1 = Arg1.SelectSingleNode("UIParams");
                int DefaultFlag = Convert.ToInt32(var1.SelectSingleNode("Type").InnerText);
                bool DisplayFlag = Convert.ToBoolean(var1.SelectSingleNode("Visible").InnerText);
                EventDisplay retValue;

                if (DefaultFlag==0)
                {
                    retValue = new EventDisplay();
                }
                else 
                {
                    Color colorNode = getColorNode(var1);
                    retValue = new EventDisplay(DisplayFlag, colorNode, DefaultFlag);
                }
                return retValue;
            }

            public Color getColorNode(XmlNode Arg1)
            {
                XmlNode var1 = Arg1.SelectSingleNode("Color");
                int b = Convert.ToInt32(var1.SelectSingleNode("Blue").InnerText);
                int g = Convert.ToInt32(var1.SelectSingleNode("Green").InnerText);
                int r = Convert.ToInt32(var1.SelectSingleNode("Red").InnerText);
                double o = Convert.ToDouble(var1.SelectSingleNode("Opacity").InnerText);

                Color retValue = new Color(r, g, b, 0);
                return retValue;
            }


            #endregion

            private DateTime stringToDateTime(string MyDateTimeString)//String should be in format "MM/DD/YY HH:MM:SSA"
            {
                //4/19/2013 11:34:40 AM

                DateTime MyDateTime, MyNow;
                /*
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
            

                if (DateComponents.Length < 2)
                {
                    MyNow = Now;
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

                Year = Convert.ToInt32(DateComponents[2]);
                Month = Convert.ToInt32(DateComponents[0]);
                Day = Convert.ToInt32(DateComponents[1]);
                Hour = Convert.ToInt32(TimeComponents[0]);
                Min = Convert.ToInt32(TimeComponents[1]);
                sec = 0;
                DateTime MyDateTime0=  new DateTime(Year, Month, Day, Hour, Min, sec);
                */
                MyDateTime = DateTime.Parse(MyDateTimeString);


                return MyDateTime;
            }


            public static DateTime ConvertToDateTime(string StringOfDateTime)
            {
                string[] strArray = StringOfDateTime.Split(new char[] { '|' });
                string[] strArray2 = strArray[0].Split(new char[] { ' ' });
                string[] strArray3 = strArray[1].Split(new char[] { ' ' });
                return new DateTime(Convert.ToInt16(strArray2[0]), Convert.ToInt16(strArray2[1]), Convert.ToInt16(strArray2[2]), Convert.ToInt16(strArray3[0]), Convert.ToInt16(strArray3[1]), Convert.ToInt16(strArray3[2]));
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

            #region Properties

            public int LastUserID
            {
                get
                {
                    return Convert.ToInt32(LastIDNumber);
                }
            }

            public bool Status
            {
                get
                {
                    return LogStatus;
                }
            }

            public int LoggedUserID
            {
                get
                {
                    return ID;
                }
            }


            #endregion

        }
        #endregion

#region properties
        public int LastEventTopNodeID
        {
            get
            {
                if (UserLog.Status)
                {
                    return UserLog.LastUserID;
                }
                return 0;
            }
        }


        public bool Status
        {
            get
            {
                return UserLog.Status;
            }
        }

        public int UserID
        {
            get 
            {
                return ID;
            }
        }

        public string UserName
        {
            get
            {
                return Username;
            }
        }
#endregion 

    }
}
