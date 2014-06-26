//#define readfromBeforeInsertionFixingStiticRestricted
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;

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

        public CustomErrors Register(string FirstName, string LastName, string Email, string UserName, string PassWord)
        {
            CustomErrors retValue = new CustomErrors(false,"success");
            UserAccountDBAccess = new DBControl(UserName, PassWord);
            Tuple<int, CustomErrors> registrationStatus = UserAccountDBAccess.RegisterUser(FirstName, LastName, Email, UserName, PassWord);

            UserLog = new LogControl(UserName,PassWord);
            UserLog.Initialize();
            if (!registrationStatus.Item2.Status)
            {
                Username = UserName;
                Password = PassWord;
                retValue =UserLog.genereateNewLogFile(registrationStatus.Item1);

                if (retValue.Status && retValue.Code >= 20000000)//error 20000000 denotes log creation issue
                {
                    UserAccountDBAccess.deleteUser();
                }
            }

            return retValue;
        }

        public Tuple<Dictionary<string, CalendarEvent>, DateTime> getProfileInfo(string desiredDirectory="")
        {
            Tuple<Dictionary<string, CalendarEvent>, DateTime> retValue;
            if (UserLog.Status)
            {
                Dictionary<string, CalendarEvent> AllScheduleData = getAllCalendarElements(desiredDirectory);
                DateTime ReferenceTime = getDayReferenceTime(desiredDirectory);
                retValue = new Tuple<Dictionary<string, CalendarEvent>, DateTime>(AllScheduleData, ReferenceTime);
            }
            else 
            {
                retValue = null;
            }

            return retValue;

        }

        private Dictionary<string, CalendarEvent>  getAllCalendarElements(string desiredDirectory="")
        {
            Dictionary<string, CalendarEvent> retValue=new Dictionary<string,CalendarEvent>();
            retValue = UserLog.getAllCalendarFromXml(desiredDirectory);
            return retValue;
        }

        private DateTime getDayReferenceTime(string desiredDirectory = "")
        {
            DateTime retValue =  UserLog.getDayReferenceTime(desiredDirectory);
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

        public void UpdateReferenceDayTime(DateTime referenceTime)
        {
            UserLog.UpdateReferenceDay(referenceTime);
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

            public CustomErrors genereateNewLogFile(int UserID)//creates a new xml log file. Uses the passed UserID
            {
                CustomErrors retValue = new CustomErrors(false, "success");
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
                    EmptyCalendarXMLFile(CurrentLog);
                    //EmptyCalendarXMLFile();
                }
                catch (Exception e)
                {
                    retValue = new CustomErrors(true, "Error generating log\n" + e.ToString(), 20000000);
                }

                return retValue;

            }

            public CustomErrors DeleteLog()
            {
                CustomErrors retValue=new CustomErrors(false,"Success");
                try
                { 
                    File.Delete(CurrentLog);
                }
                catch(Exception e)
                {
                    retValue = new CustomErrors(true, e.ToString(), 20002000);
                }

                return retValue;
            }

            public void UpdateReferenceDay(DateTime referenceDay, string LogFile = "")
            { 
                if (LogFile == "")
                { LogFile = WagTapLogLocation + CurrentLog; }
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(LogFile);
                XmlElement refDayNode = xmldoc.CreateElement("referenceDay");
                refDayNode.InnerText = referenceDay.ToShortTimeString();
                XmlNode refNode=xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/referenceDay");
                if (refNode == null)
                {
                    XmlNode myNode = xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog");
                    myNode.AppendChild(refDayNode);
                }
                else
                {
                    refNode.InnerText = refDayNode.InnerText;
                }
                xmldoc.Save(LogFile);
                return;
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
                
                XmlElement EventScheduleNode = CreateEventScheduleNode(MyEvent);
                //EventSchedulesNodes[0].PrependChild(xmldoc.CreateElement("EventSchedule"));
                //EventSchedulesNodes[0].ChildNodes[0].InnerXml = CreateEventScheduleNode(MyEvent).InnerXml;
                XmlNode MyImportedNode = xmldoc.ImportNode(EventScheduleNode as XmlNode, true);
                //(EventScheduleNode, true);
                if (!UpdateInnerXml(ref EventScheduleNodes, "ID", MyEvent.ID.ToString(), EventScheduleNode))
                {
                    xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/EventSchedules").AppendChild(MyImportedNode);
                }
                while(true)
                {
                    try
                    {
                        xmldoc.Save(LogFile);
                        break;
                    }
                    catch (Exception e)
                    {
                        Thread.Sleep(160);
                    }
                }
            }

            

            public XmlElement CreateEventScheduleNode(CalendarEvent MyEvent)
            {
                XmlDocument xmldoc = new XmlDocument();


                XmlElement MyEventScheduleNode = xmldoc.CreateElement("EventSchedule");
                MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Completed"));
                MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.isComplete.ToString();
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
                MyEventScheduleNode.PrependChild(xmldoc.CreateElement("MiscData"));
                MyEventScheduleNode.ChildNodes[0].InnerXml = createMiscDataNode(MyEvent.Notes, "MiscData").InnerXml;
                


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
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Complete"));
                MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.isComplete.ToString();
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Location"));
                MyEventSubScheduleNode.ChildNodes[0].InnerXml = CreateLocationNode(MySubEvent.myLocation, "EventSubScheduleLocation").InnerXml;
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("UIParams"));
                MyEventSubScheduleNode.ChildNodes[0].InnerXml = createDisplayUINode(MySubEvent.UIParam, "UIParams").InnerXml;
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("MiscData"));
                MyEventSubScheduleNode.ChildNodes[0].InnerXml = createMiscDataNode(MySubEvent.Notes, "MiscData").InnerXml;


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
                var1.PrependChild(xmldoc.CreateElement("Type"));
                var1.ChildNodes[0].InnerText = Arg1.isDefault.ToString();
                return var1;
            }

            public XmlElement createColorNode(Color Arg1, string ElementIdentifier)
            {
                XmlDocument xmldoc = new XmlDocument();
                XmlElement var1 = xmldoc.CreateElement(ElementIdentifier);
                var1.PrependChild(xmldoc.CreateElement("Opacity"));
                var1.ChildNodes[0].InnerText = Arg1.O.ToString();
                var1.PrependChild(xmldoc.CreateElement("Red"));
                var1.ChildNodes[0].InnerText = Arg1.R.ToString();
                var1.PrependChild(xmldoc.CreateElement("Green"));
                var1.ChildNodes[0].InnerText = Arg1.G.ToString();
                var1.PrependChild(xmldoc.CreateElement("Blue"));
                var1.ChildNodes[0].InnerText = Arg1.B.ToString();

                return var1;
            }

            public XmlElement createMiscDataNode(MiscData Arg1, string ElementIdentifier)
            {
                XmlDocument xmldoc = new XmlDocument();
                XmlElement var1 = xmldoc.CreateElement(ElementIdentifier);
                var1.PrependChild(xmldoc.CreateElement("UserNote"));
                var1.ChildNodes[0].InnerText = Arg1.UserNote.ToString();
                var1.PrependChild(xmldoc.CreateElement("TypeSelection"));
                var1.ChildNodes[0].InnerText = Arg1.TypeSelection.ToString();
                return var1;
            }


            public void EmptyCalendarXMLFile(string dirString="")
            {
                if(string.IsNullOrEmpty(dirString))
                {
                    dirString = WagTapLogLocation + CurrentLog;
                }

                File.WriteAllText(dirString, "<?xml version=\"1.0\" encoding=\"utf-8\"?><ScheduleLog><LastIDCounter>0</LastIDCounter><referenceDay>12:00 AM</referenceDay><EventSchedules></EventSchedules></ScheduleLog>");
            }
            #endregion



            #region Read Data


            public string GetShortcutTarget(string file)
            {
                try
                {
                    if (System.IO.Path.GetExtension(file).ToLower() != ".lnk")
                    {
                        throw new Exception("Supplied file must be a .LNK file");
                    }

                    FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read);
                    using (System.IO.BinaryReader fileReader = new BinaryReader(fileStream))
                    {
                        fileStream.Seek(0x14, SeekOrigin.Begin);     // Seek to flags
                        uint flags = fileReader.ReadUInt32();        // Read flags
                        if ((flags & 1) == 1)
                        {                      // Bit 1 set means we have to
                            // skip the shell item ID list
                            fileStream.Seek(0x4c, SeekOrigin.Begin); // Seek to the end of the header
                            uint offset = fileReader.ReadUInt16();   // Read the length of the Shell item ID list
                            fileStream.Seek(offset, SeekOrigin.Current); // Seek past it (to the file locator info)
                        }

                        long fileInfoStartsAt = fileStream.Position; // Store the offset where the file info
                        // structure begins
                        uint totalStructLength = fileReader.ReadUInt32(); // read the length of the whole struct
                        fileStream.Seek(0xc, SeekOrigin.Current); // seek to offset to base pathname
                        uint fileOffset = fileReader.ReadUInt32(); // read offset to base pathname
                        // the offset is from the beginning of the file info struct (fileInfoStartsAt)
                        fileStream.Seek((fileInfoStartsAt + fileOffset), SeekOrigin.Begin); // Seek to beginning of
                        // base pathname (target)
                        long pathLength = (totalStructLength + fileInfoStartsAt) - fileStream.Position - 2; // read
                        // the base pathname. I don't need the 2 terminating nulls.
                        char[] linkTarget = fileReader.ReadChars((int)pathLength); // should be unicode safe
                        var link = new string(linkTarget);

                        int begin = link.IndexOf("\0\0");
                        if (begin > -1)
                        {
                            int end = link.IndexOf("\\\\", begin + 2) + 2;
                            end = link.IndexOf('\0', end) + 1;

                            string firstPart = link.Substring(0, begin);
                            string secondPart = link.Substring(end);

                            return firstPart + secondPart;
                        }
                        else
                        {
                            return link;
                        }
                    }
                }
                catch
                {
                    return "";
                }
            }

            public DateTime getDayReferenceTime(string NameOfFile)
            {
                XmlDocument doc = getLogDataStore(NameOfFile);
                XmlNode node = doc.DocumentElement.SelectSingleNode("/ScheduleLog/referenceDay");
                DateTime retValue= DateTime.Parse(node.InnerText);

                return retValue;
            }

            private XmlDocument getLogDataStore(string NameOfFile = "")
            {
                
                XmlDocument doc = new XmlDocument();

                if (string.IsNullOrEmpty(NameOfFile))
                {
                    //NameOfFile = "MyEventLog.xml";
                    NameOfFile = WagTapLogLocation + CurrentLog;
                }
#if readfromBeforeInsertionFixingStiticRestricted
                NameOfFile = WagTapLogLocation + "BeforeInsertionFixingStiticRestricted.xml.lnk";
                NameOfFile = GetShortcutTarget(NameOfFile);
#endif




                while (true) 
                {
                    if(!File.Exists(NameOfFile))
                    {
                        break;
                    }
                    try
                    {
                        doc.Load(NameOfFile);
                        break;
                    }
                    catch (Exception e)
                    {
                        Thread.Sleep(160);
                        ;
                        
                    }
                }

                return doc;
            }


            public Dictionary<string, CalendarEvent> getAllCalendarFromXml(string NameOfFile = "")
            {
                XmlDocument doc = getLogDataStore(NameOfFile);
                Dictionary<string, CalendarEvent> MyCalendarEventDictionary = new Dictionary<string, CalendarEvent>();


                XmlNode node = doc.DocumentElement.SelectSingleNode("/ScheduleLog/LastIDCounter");
                string LastUsedIndex = node.InnerText;
                LastIDNumber = LastUsedIndex;
                DateTime userReferenceDay;
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
                
                StartDateTime = EventScheduleNode.SelectSingleNode("StartTime").InnerText.Split(' ');

                StartDate = StartDateTime[0];
                StartTime = StartDateTime[1] + StartDateTime[2];
                EndDateTime = EventScheduleNode.SelectSingleNode("Deadline").InnerText.Split(' ');
                EndDate = EndDateTime[0];
                EndTime = EndDateTime[1] + EndDateTime[2];
                DateTime StartTimeConverted = DateTime.Parse(StartDate);// new DateTime(Convert.ToInt32(StartDate.Split('/')[2]), Convert.ToInt32(StartDate.Split('/')[0]), Convert.ToInt32(StartDate.Split('/')[1]));
                DateTime EndTimeConverted = DateTime.Parse(EndDate); //new DateTime(Convert.ToInt32(EndDate.Split('/')[2]), Convert.ToInt32(EndDate.Split('/')[0]), Convert.ToInt32(EndDate.Split('/')[1]));
                
                Repetition Recurrence;
                if (Convert.ToBoolean(EventRepetitionflag))
                {
                    RepeatStart = RecurrenceXmlNode.SelectSingleNode("RepeatStartDate").InnerText;
                    RepeatEnd = RecurrenceXmlNode.SelectSingleNode("RepeatEndDate").InnerText;
                    RepeatFrequency = RecurrenceXmlNode.SelectSingleNode("RepeatFrequency").InnerText;
                    XmlNode XmlNodeWithList = RecurrenceXmlNode.SelectSingleNode("RepeatCalendarEvents");
                    Recurrence = new Repetition(true, new TimeLine(DateTime.Parse(RepeatStart), DateTime.Parse(RepeatEnd)), RepeatFrequency, getAllRepeatCalendarEvents(XmlNodeWithList));


                    StartTimeConverted = DateTime.Parse(RepeatStart);
                    EndTimeConverted = DateTime.Parse(RepeatEnd);
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
                bool completedFlag = Convert.ToBoolean(Completed);
                
                

                //string Name, string StartTime, DateTime StartDate, string EndTime, DateTime EventEndDate, string eventSplit, string PreDeadlineTime, string EventDuration, bool EventRepetitionflag, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag
                
                //MainWindow.CreateSchedule("","",new DateTime(),"",new DateTime(),"","","",true,true,true,"",false);
                Recurrence = Recurrence;

                Location var3 = getLocation(EventScheduleNode);
                MiscData noteData = getMiscData(EventScheduleNode);
                EventDisplay UiData = getDisplayUINode(EventScheduleNode);


                CalendarEvent RetrievedEvent = new CalendarEvent(ID, Name, StartTime, StartTimeConverted, EndTime, EndTimeConverted, Split, PreDeadline, CalendarEventDuration, Recurrence, false, Convert.ToBoolean(Rigid), PrepTime, false, var3, EVentEnableFlag, UiData, noteData, completedFlag);
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
                    bool CompleteFlag = Convert.ToBoolean(MyXmlNode.ChildNodes[i].SelectSingleNode("Complete").InnerText);
                    Location var1 = getLocation(MyXmlNode.ChildNodes[i]);
                    MiscData noteData = getMiscData(MyXmlNode.ChildNodes[i]);
                    EventDisplay UiData = getDisplayUINode(MyXmlNode.ChildNodes[i]);


                    MyArrayOfNodes[i] = new SubCalendarEvent(ID, BusySlot, Start, End, PrepTime, MyParent.ID, MyParent.Rigid, Enabled, UiData, noteData, CompleteFlag, var1, MyParent.RangeTimeLine);
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

            MiscData getMiscData(XmlNode Arg1)
            {
                XmlNode var1 = Arg1.SelectSingleNode("MiscData");
                string stringData = (var1.SelectSingleNode("UserNote").InnerText);
                int NoteData = Convert.ToInt32(var1.SelectSingleNode("TypeSelection").InnerText);
                MiscData retValue = new MiscData(stringData, NoteData);
                return retValue;
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
                
                DateTime MyDateTime, MyNow;
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


            public string getFullLogDir
            { 
                get
                {
                    return WagTapLogLocation + CurrentLog;
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

        public string getFullLogDir
        {
            get 
            {
                return UserLog.getFullLogDir;
            }
        }
#endregion 

    }
}
