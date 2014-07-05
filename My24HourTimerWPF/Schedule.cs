﻿#define StitcohRestrictedFromLeft
#define useLockedImplementation
#define useNonLockedImplementation
//#define EnableRestrictedLocationOptimization

//#define createCopyOfImplementation


//#define ForceSequentialSnugArray




//#define enableSequentialAcces


#if enableSequentialAcces
#undef enableMultithreading
#endif


#define enableDebugging



#define StitchRestrictedFromRight

#if StitchRestrictedFromLeft
#undef StitchRestrictedFromRight
#endif

using System.Threading;
using System.Threading.Tasks;
//#define EnableClashLog

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;


using System.IO;
namespace My24HourTimerWPF
{
    public class Schedule
    {


        Dictionary<string, CalendarEvent> AllEventDictionary;
        TimeLine CompleteSchedule;
        public TimeSpan ZeroTimeSpan = new TimeSpan(0);
        public TimeSpan TwentyFourHourTimeSpan = new TimeSpan(1,0,0,0);
        public TimeSpan HourTimeSpan = new TimeSpan(0, 1, 0, 0);
        ThirdPartyCalendarControl[] myCalendar;
        

        UserAccount myAccount;
        int LatesMainID;

        double PercentageOccupancy = 0;
        //public static DateTime Now = new DateTime(2014,4,6,0,0,0);//DateTime.Now;
        public static DateTime Now = DateTime.Now;
        //Schedule.Now = DateTime.Now;
        DateTime ReferenceDayTIime;
        static string stageOfProgram = "";




        /*
        public Schedule(string UserName, string Password, string LogDirectory = "")
        {
            myAccount = new UserAccount(UserName, Password, LogDirectory);
            Initialize();
        }


        public Schedule(string UserName, int UserID, string LogDirectory="")
        {
            myAccount = new UserAccount(UserName, UserID, LogDirectory);
            Initialize();
        }
        */

        public Schedule(UserAccount AccountEntry,DateTime referenceNow)
        {
            myAccount = AccountEntry;
            Now = referenceNow;
            Initialize();
        }

        void setAsComplete()
        {
            DateTime TempNow = ReferenceDayTIime;
            //ReferenceDayTIime = Now;
            //ReferenceDayTIime = new DateTime(ReferenceDayTIime.Year, ReferenceDayTIime.Month, ReferenceDayTIime.Day, 16, 0, 0);
            foreach (KeyValuePair<string, CalendarEvent> eachKeyValuePair in AllEventDictionary)
            {
                if (eachKeyValuePair.Value.RepetitionStatus)
                {
                    foreach (CalendarEvent eachCalendarEvent in eachKeyValuePair.Value.Repeat.RecurringCalendarEvents)
                    {
                        if (eachCalendarEvent.End <= TempNow)
                        {
                            eachKeyValuePair.Value.SetCompletionStatus(true);
                        }
                    }

                    foreach (SubCalendarEvent eachSubCalendarEvent in eachKeyValuePair.Value.AllSubEvents)
                    {
                        if (eachSubCalendarEvent.End <= TempNow)
                        {
                            eachSubCalendarEvent.SetCompletionStatus(true);
                        }
                    }
                }
                else
                {
                    if (eachKeyValuePair.Value.End <= TempNow)
                    {
                        eachKeyValuePair.Value.SetCompletionStatus(true);
                    }

                    foreach (SubCalendarEvent eachSubCalendarEvent in eachKeyValuePair.Value.AllSubEvents)
                    {
                        if (eachSubCalendarEvent.End <= TempNow)
                        {
                            eachSubCalendarEvent.SetCompletionStatus(true);
                        }
                    }
                }
            }
        }
        private void Initialize()
        {
            myAccount.Login();
            Tuple<Dictionary<string, CalendarEvent>, DateTime>profileData=myAccount.getProfileInfo();
            if (profileData!=null)
            { 
                ReferenceDayTIime = profileData.Item2;
                DateTime referenceDayTimeNow = DateTime.Parse(ReferenceDayTIime.ToShortTimeString());
                ReferenceDayTIime =Now <referenceDayTimeNow ?referenceDayTimeNow.AddDays(-1):referenceDayTimeNow ;
                //ReferenceDayTIime = DateTime.Parse("4:00PM");
                AllEventDictionary = profileData.Item1;
                if (AllEventDictionary != null)
                {
                    //setAsComplete();
                    myCalendar = new ThirdPartyCalendarControl[1];
                    myCalendar[0] = new ThirdPartyCalendarControl(ThirdPartyCalendarControl.CalendarTool.Outlook);
                    CompleteSchedule = getTimeLine();
                    EventIDGenerator.Initialize((uint)(this.LastScheduleIDNumber));
                }
            }
        }

        public CalendarEvent getCalendarEvent(string EventID)
        {
            EventID userEvent = new My24HourTimerWPF.EventID(EventID);
            return getCalendarEvent(userEvent);
            
            
        }

        public CalendarEvent getCalendarEvent(EventID myEventID)
        {

            CalendarEvent calEvent = AllEventDictionary[myEventID.ID[0]];

            CalendarEvent repeatEvent = calEvent.getRepeatedCalendarEvent(myEventID.ToString());


            if (repeatEvent == null)
            {
                return calEvent;
            }
            else
            {
                return repeatEvent;
            }
            
            //return AllEventDictionary[myEventID.ID[0]];
        }

        public SubCalendarEvent getSubCalendarEvent(string EventID)
        {
            CalendarEvent myCalendarEvent = getCalendarEvent(EventID);
            return myCalendarEvent.getSubEvent(new EventID(EventID));
        }


        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> UpdateDeadline(string EventID, DateTime NewDeadline)
        {
            CalendarEvent myCalendarEvent = getCalendarEvent(EventID);
            IEnumerable<SubCalendarEvent> AllSubEVents = myCalendarEvent.AllSubEvents.Select(obj=>obj.createCopy());
            IEnumerable<SubCalendarEvent> referenceSubEVents = AllSubEVents.Where(obj => obj.isActive);
            bool InitEnableStatus=myCalendarEvent.isEnabled;
            myCalendarEvent.Disable(false);

            //(string EventName, TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan Event_PreDeadline, bool EventRigidFlag, Repetition EventRepetitionEntry, int EventSplit, Location EventLocation, bool EnableFlag, EventDisplay UiData, MiscData NoteData, bool CompletionFlag)

            CalendarEvent ReadjustedCalendarEvent = new CalendarEvent(myCalendarEvent.Name, Utility.SumOfActiveDuration(referenceSubEVents), myCalendarEvent.Start, NewDeadline, ZeroTimeSpan, ZeroTimeSpan, myCalendarEvent.Rigid, new Repetition(), referenceSubEVents.Count(), myCalendarEvent.myLocation, true, myCalendarEvent.UIParam, myCalendarEvent.Notes, myCalendarEvent.isComplete);
            IEnumerable<SubCalendarEvent>  referenceSubEVents_Changed = referenceSubEVents.Select(obj => new SubCalendarEvent(obj.ActiveDuration, obj.Start, obj.End, obj.Preparation, ReadjustedCalendarEvent.ID, obj.Rigid, true, obj.UIParam, obj.Notes, obj.isComplete, obj.myLocation, ReadjustedCalendarEvent.RangeTimeLine));
            ReadjustedCalendarEvent = new CalendarEvent(ReadjustedCalendarEvent, referenceSubEVents_Changed.ToArray());

            HashSet<SubCalendarEvent> NoDoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            
            Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
            AllEventDictionary_Cpy = AllEventDictionary.ToDictionary(obj => obj.Key, obj => obj.Value.createCopy());
            
            ReadjustedCalendarEvent=EvaluateTotalTimeLineAndAssignValidTimeSpots(ReadjustedCalendarEvent,NoDoneYet);

            SubCalendarEvent[] UpdatedSubEvents= ReadjustedCalendarEvent.AllSubEvents;
            int i=0;

            ReadjustedCalendarEvent = new CalendarEvent(myCalendarEvent.Calendar_EventID, myCalendarEvent.Name, myCalendarEvent.ActiveDuration, myCalendarEvent.Start, ReadjustedCalendarEvent.End, myCalendarEvent.Preparation, myCalendarEvent.PreDeadline, myCalendarEvent.Rigid, myCalendarEvent.Repeat, myCalendarEvent.NumberOfSplit, myCalendarEvent.myLocation, myCalendarEvent.isEnabled, myCalendarEvent.UIParam, myCalendarEvent.Notes, myCalendarEvent.isComplete);

            

            foreach (SubCalendarEvent eachSubCalendarEvent in referenceSubEVents)
            {
                myCalendarEvent.updateSubEvent(eachSubCalendarEvent.SubEvent_ID, UpdatedSubEvents[i++]);
            }

            ReadjustedCalendarEvent = new CalendarEvent(ReadjustedCalendarEvent, myCalendarEvent.AllSubEvents.ToArray());

            myCalendarEvent.UpdateThis(ReadjustedCalendarEvent);

            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue =new Tuple<CustomErrors,Dictionary<string,CalendarEvent>>(ReadjustedCalendarEvent.Error,AllEventDictionary);
            myCalendarEvent.SetEventEnableStatus(InitEnableStatus);
            AllEventDictionary=AllEventDictionary_Cpy;
            return retValue ;
        }

        public Dictionary<string, CalendarEvent> getAllCalendarElements()
        {
            Tuple<Dictionary<string, CalendarEvent>, DateTime> profileData= myAccount.getProfileInfo();

            return profileData == null ? null : profileData.Item1;
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
                List<BusyTimeLine> MyTotalSubEvents = new List<BusyTimeLine>(0);
                foreach (KeyValuePair<string, CalendarEvent> MyCalendarEvents in AllEventDictionary)
                {
                    foreach (SubCalendarEvent MySubCalendarEvent in MyCalendarEvents.Value.ActiveSubEvents)
                    {
                        MyTotalSubEvents.Add(MySubCalendarEvent.ActiveSlot);
                    }
                }
                MyTotalSubEvents = Schedule.SortBusyTimeline(MyTotalSubEvents, true);
                DateTime MyNow = Now;//Moved Out of For loop for Speed boost
                for (int i = 0; i < MyTotalSubEvents.Count; i++)
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
            DateTime LastDeadline = Now.AddHours(1);
            List<BusyTimeLine> MyTotalBusySlots = new List<BusyTimeLine>(0);
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
            TimeLine MyTimeLine = new TimeLine(Now, Now.AddHours(1));
            if (MyTotalBusySlots.Count > 0)
            {
                MyTimeLine = new TimeLine(Now, MyTotalBusySlots[MyTotalBusySlots.Count - 1].End);
            }
            MyTimeLine.OccupiedSlots = MyTotalBusySlots.ToArray();
            return MyTimeLine;
        }



        public void RemoveAllCalendarEventFromLogAndCalendar()//MyTemp Function for deleting all calendar events
        {
            myAccount.DeleteAllCalendarEvents();
            removeAllFromOutlook();
        }

        public void removeAllFromOutlook()
        {
            myCalendar[0].removeAllEventsFromOutLook(AllEventDictionary.Values);
        }

        public void EmptyMemory()
        {
            AllEventDictionary = new Dictionary<string, CalendarEvent>();
        }

        BusyTimeLine[] GetBusySlotPerCalendarEvent(CalendarEvent MyEvent)
        {
            int i = 0;
            List<BusyTimeLine> MyTotalSubEventBusySlots = new List<BusyTimeLine>(0);
            BusyTimeLine[] ArrayOfBusySlotsInRepeat = new BusyTimeLine[0];
            DateTime LastDeadline = Now.AddHours(1);

            if (MyEvent.RepetitionStatus)
            {
                ArrayOfBusySlotsInRepeat = GetBusySlotsPerRepeat(MyEvent.Repeat);
            }

            /*for (;i<MyEvent.AllEvents.Length;i++)
            {
                {*/
            foreach (SubCalendarEvent MySubCalendarEvent in MyEvent.ActiveSubEvents)//Active Fix
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
            int j = 0;
            for (; i < MyListOfWithArrayOfBusySlots.Count; i++)
            {
                j = 0;
                for (; j < MyListOfWithArrayOfBusySlots[i].Length; j++)
                {
                    MyListOfBusySlots.Add(MyListOfWithArrayOfBusySlots[i][j]);
                }
            }

            return MyListOfBusySlots.ToArray();


        }


        string LogInfo = "";




        public void deleteCalendarEvent(string EventID)
        {
            CalendarEvent CalendarEventTOBeRemoved = getCalendarEvent(EventID);
            CalendarEventTOBeRemoved.Disable(true);
            CalendarEventTOBeRemoved.DisableSubEvents(CalendarEventTOBeRemoved.ActiveSubEvents);
            CalendarEventTOBeRemoved = new CalendarEvent(CalendarEventTOBeRemoved.Name,CalendarEventTOBeRemoved.ActiveDuration,CalendarEventTOBeRemoved.Start,CalendarEventTOBeRemoved.End,CalendarEventTOBeRemoved.Preparation,CalendarEventTOBeRemoved .PreDeadline,CalendarEventTOBeRemoved .Rigid,new Repetition(),1,CalendarEventTOBeRemoved .myLocation,false,new EventDisplay(),new MiscData(),false);
            CalendarEventTOBeRemoved.DisableSubEvents(CalendarEventTOBeRemoved.ActiveSubEvents);

            HashSet<SubCalendarEvent> NotDOneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            EvaluateTotalTimeLineAndAssignValidTimeSpots(CalendarEventTOBeRemoved, NotDOneYet);


            AllEventDictionary.Remove(CalendarEventTOBeRemoved.ID);//removes the false calendar event


            UpdateWithProcrastinateSchedule(AllEventDictionary);
        }


        
        public void markAsCompleteCalendarEventAndReadjust(string EventID)
        {
            CalendarEvent CalendarEventTOBeRemoved = getCalendarEvent(EventID);
            CalendarEventTOBeRemoved.SetCompletionStatus(true);
            
            CalendarEventTOBeRemoved = new CalendarEvent(CalendarEventTOBeRemoved.Name, CalendarEventTOBeRemoved.ActiveDuration, CalendarEventTOBeRemoved.Start, CalendarEventTOBeRemoved.End, CalendarEventTOBeRemoved.Preparation, CalendarEventTOBeRemoved.PreDeadline, CalendarEventTOBeRemoved.Rigid, new Repetition(), 1, CalendarEventTOBeRemoved.myLocation, false, new EventDisplay(), new MiscData(), false);
            CalendarEventTOBeRemoved.DisableSubEvents(CalendarEventTOBeRemoved.ActiveSubEvents);


            HashSet<SubCalendarEvent> NotDOneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            EvaluateTotalTimeLineAndAssignValidTimeSpots(CalendarEventTOBeRemoved, NotDOneYet);


            AllEventDictionary.Remove(CalendarEventTOBeRemoved.ID);//removes the false calendar event


            UpdateWithProcrastinateSchedule(AllEventDictionary);
        }

        

        public void markSubEventAsCompleteCalendarEventAndReadjust(string EventID)
        {

            CalendarEvent referenceCalendarEventWithSubEvent = getCalendarEvent(EventID);
            SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(EventID);

            EventID SubEventID = new EventID(EventID);

            

            bool InitialRigid = ReferenceSubEvent.Rigid;


            if (referenceCalendarEventWithSubEvent.RepetitionStatus)
            {
                referenceCalendarEventWithSubEvent = referenceCalendarEventWithSubEvent.getRepeatedCalendarEvent(SubEventID.getStringIDAtLevel(1));
            }
            Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
            AllEventDictionary_Cpy = AllEventDictionary.ToDictionary(obj => obj.Key, obj => obj.Value.createCopy());
            List<SubCalendarEvent> AllValidSubCalEvents = new List<SubCalendarEvent>() { ReferenceSubEvent };// ProcrastinateEvent.AllActiveSubEvents.Where(obj => obj.End > ReferenceSubEvent.Start).ToList();
            DateTime StartTime = Now;
            DateTime EndTime = StartTime.Add(ReferenceSubEvent.ActiveDuration); ;
            ReferenceSubEvent.SetCompletionStatus(true);


            TimeSpan TotalActiveDuration = Utility.SumOfActiveDuration(AllValidSubCalEvents);
            //CalendarEvent(string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag,Location EventLocation)
            CalendarEvent ScheduleUpdated = new CalendarEvent(referenceCalendarEventWithSubEvent.Name, referenceCalendarEventWithSubEvent.Start.ToString("hh:mm tt"), referenceCalendarEventWithSubEvent.Start, referenceCalendarEventWithSubEvent.End.ToString("hh:mm tt"), referenceCalendarEventWithSubEvent.End, 1.ToString(), referenceCalendarEventWithSubEvent.PreDeadline.ToString(), ReferenceSubEvent.ActiveDuration.ToString(), new Repetition(), true, ReferenceSubEvent.Rigid, referenceCalendarEventWithSubEvent.Preparation.ToString(), true, ReferenceSubEvent.myLocation, false, new EventDisplay(), new MiscData(), false);
            ScheduleUpdated.DisableSubEvents(ScheduleUpdated.AllSubEvents);//hackalert


            HashSet<SubCalendarEvent> NotDOneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            ScheduleUpdated = EvaluateTotalTimeLineAndAssignValidTimeSpots(ScheduleUpdated, NotDOneYet);

            AllEventDictionary.Remove(ScheduleUpdated.ID);//removes the false calendar event


            UpdateWithProcrastinateSchedule(AllEventDictionary);

        }

        public void deleteSubCalendarEvent(string EventID)
        {



            CalendarEvent referenceCalendarEventWithSubEvent = getCalendarEvent(EventID);
            SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(EventID);

            EventID SubEventID = new EventID(EventID);


            bool InitialRigid = ReferenceSubEvent.Rigid;

            
            if (referenceCalendarEventWithSubEvent.RepetitionStatus)
            {
                referenceCalendarEventWithSubEvent = referenceCalendarEventWithSubEvent.getRepeatedCalendarEvent(SubEventID.getStringIDAtLevel(1));
            }

            Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
            AllEventDictionary_Cpy = AllEventDictionary.ToDictionary(obj => obj.Key, obj => obj.Value.createCopy());

            List<SubCalendarEvent> AllValidSubCalEvents = new List<SubCalendarEvent>() { ReferenceSubEvent };// ProcrastinateEvent.AllActiveSubEvents.Where(obj => obj.End > ReferenceSubEvent.Start).ToList();
            DateTime StartTime = Now;
            DateTime EndTime = StartTime.Add(ReferenceSubEvent.ActiveDuration); ;



            referenceCalendarEventWithSubEvent.DisableSubEvents(AllValidSubCalEvents);

            TimeSpan TotalActiveDuration = Utility.SumOfActiveDuration(AllValidSubCalEvents);
            //CalendarEvent(string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag,Location EventLocation)
            CalendarEvent ScheduleUpdated = new CalendarEvent(referenceCalendarEventWithSubEvent.Name, referenceCalendarEventWithSubEvent.Start.ToString("hh:mm tt"), referenceCalendarEventWithSubEvent.Start, referenceCalendarEventWithSubEvent.End.ToString("hh:mm tt"), referenceCalendarEventWithSubEvent.End, 1.ToString(), referenceCalendarEventWithSubEvent.PreDeadline.ToString(), ReferenceSubEvent.ActiveDuration.ToString(), new Repetition(), true, ReferenceSubEvent.Rigid, referenceCalendarEventWithSubEvent.Preparation.ToString(), true, ReferenceSubEvent.myLocation, false,new EventDisplay(),new MiscData(),false);
            ScheduleUpdated.DisableSubEvents(ScheduleUpdated.AllSubEvents);


            HashSet<SubCalendarEvent> NotDOneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            ScheduleUpdated = EvaluateTotalTimeLineAndAssignValidTimeSpots(ScheduleUpdated, NotDOneYet);



            AllEventDictionary.Remove(ScheduleUpdated.ID);//removes the false calendar event

            UpdateWithProcrastinateSchedule(AllEventDictionary);
        }
        

        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> Procrastinate(CalendarEvent NewEvent)
        {
            HashSet<SubCalendarEvent> NotdoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            NewEvent = EvaluateTotalTimeLineAndAssignValidTimeSpots(NewEvent, NotdoneYet);
            Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
            AllEventDictionary_Cpy = AllEventDictionary.ToDictionary(obj => obj.Key, obj => obj.Value.createCopy());
            try
            {
                AllEventDictionary.Add(NewEvent.ID, NewEvent);
            }
            catch
            {
                AllEventDictionary[NewEvent.ID] = NewEvent;
            }

            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue = new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(NewEvent.Error, AllEventDictionary);
            AllEventDictionary = AllEventDictionary_Cpy;
            return retValue;
        }



        public void UpdateWithProcrastinateSchedule(Dictionary<string, CalendarEvent> UpdatedSchedule)
        {
            RemoveAllCalendarEventFromLogAndCalendar();
            AllEventDictionary = UpdatedSchedule;
            WriteFullScheduleToLogAndOutlook();
            CompleteSchedule = getTimeLine();
        }


        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> SetEventAsNow(string EventID,bool Force=false)
        {
            CalendarEvent ProcrastinateEvent = getCalendarEvent(EventID);
            SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(EventID);
            
            EventID SubEventID = new EventID(EventID);


            bool InitialRigid = ReferenceSubEvent.Rigid;

            if (!ReferenceSubEvent.shiftEvent(Now - ReferenceSubEvent.Start) && !Force)
            {
                return new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(new CustomErrors(true, "You will be going outside the limits of this event, Is that Ok?",5), AllEventDictionary);
            }


            if (ProcrastinateEvent.RepetitionStatus)
            {
                ProcrastinateEvent = ProcrastinateEvent.getRepeatedCalendarEvent(SubEventID.getStringIDAtLevel(1));
            }

            Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
            AllEventDictionary_Cpy = AllEventDictionary.ToDictionary(obj => obj.Key, obj => obj.Value.createCopy());

            List<SubCalendarEvent> AllValidSubCalEvents = new List<SubCalendarEvent>() { ReferenceSubEvent };// ProcrastinateEvent.AllActiveSubEvents.Where(obj => obj.End > ReferenceSubEvent.Start).ToList();

            DateTime StartTime = Now;
            DateTime EndTime = StartTime.Add(ReferenceSubEvent.ActiveDuration); ;


            
            ProcrastinateEvent.DisableSubEvents(AllValidSubCalEvents);
            
            TimeSpan TotalActiveDuration = Utility.SumOfActiveDuration(AllValidSubCalEvents);
            //CalendarEvent(string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag,Location EventLocation)
            CalendarEvent ScheduleUpdated = new CalendarEvent(ProcrastinateEvent.Name, StartTime.ToString("hh:mm tt"), StartTime, EndTime.ToString("hh:mm tt"), EndTime, AllValidSubCalEvents.Count.ToString(), ProcrastinateEvent.PreDeadline.ToString(), TotalActiveDuration.ToString(), new Repetition(), true, true, ProcrastinateEvent.Preparation.ToString(), true, ReferenceSubEvent.myLocation,true, new EventDisplay(),new MiscData(),false);
            SubCalendarEvent RigidizedEvent =ScheduleUpdated.ActiveSubEvents[0];
            RigidizedEvent.shiftEvent(Now - RigidizedEvent.Start, Force);//remember to fix shift force option


            SubCalendarEvent RigidSubCalendarEvent = new SubCalendarEvent(RigidizedEvent.ID, RigidizedEvent.Start, RigidizedEvent.End, RigidizedEvent.ActiveSlot, true, RigidizedEvent.isEnabled, ReferenceSubEvent.UIParam, ReferenceSubEvent.Notes, ReferenceSubEvent.isComplete, RigidizedEvent.myLocation, RigidizedEvent.getCalendarEventRange);
            RigidizedEvent.UpdateThis(RigidSubCalendarEvent);
            string IDOfRigidized = RigidizedEvent.ID;
            HashSet<SubCalendarEvent> NotDOneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();


            ScheduleUpdated = EvaluateTotalTimeLineAndAssignValidTimeSpots(ScheduleUpdated,NotDOneYet ,null,1);

            SubCalendarEvent[] UpdatedSubCalevents = ScheduleUpdated.ActiveSubEvents;

            for (int i = 0; i < AllValidSubCalEvents.Count; i++)//updates the subcalevents
            {
                bool Rigid = AllValidSubCalEvents[i].Rigid;
                if (IDOfRigidized == AllValidSubCalEvents[i].ID)
                { 
                    Rigid=InitialRigid;
                }

                SubCalendarEvent updatedSubCal = new SubCalendarEvent(AllValidSubCalEvents[i].ID, UpdatedSubCalevents[i].Start, UpdatedSubCalevents[i].End, UpdatedSubCalevents[i].ActiveSlot, Rigid, AllValidSubCalEvents[i].isEnabled, AllValidSubCalEvents[i].UIParam, AllValidSubCalEvents[i].Notes, AllValidSubCalEvents[i].isComplete, UpdatedSubCalevents[i].myLocation, ProcrastinateEvent.RangeTimeLine);
                ProcrastinateEvent.updateSubEvent(updatedSubCal.SubEvent_ID, updatedSubCal);
            }

            ProcrastinateEvent.EnableSubEvents(AllValidSubCalEvents);

            if (ScheduleUpdated.ErrorStatus)
            {
                LogStatus(ScheduleUpdated, "Set as now");
            }

            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue = new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(ScheduleUpdated.Error, AllEventDictionary);
            //AllEventDictionary = AllEventDictionary_Cpy;

            //UpdateWithProcrastinateSchedule(AllEventDictionary);
            return retValue;
        }

        public CustomErrors AddToSchedule(CalendarEvent NewEvent)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            HashSet<SubCalendarEvent> NotdoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            NewEvent = EvaluateTotalTimeLineAndAssignValidTimeSpots(NewEvent, NotdoneYet);

            sw.Stop();
            ///

            if (NewEvent == null)//checks if event was assigned and ID ehich means it was successfully able to find a spot
            {

                return new CustomErrors(NewEvent.ErrorStatus, NewEvent.ErrorMessage);
            }

            if (NewEvent.ID == "" || NewEvent == null)//checks if event was assigned and ID ehich means it was successfully able to find a spot
            {
                return new CustomErrors(NewEvent.ErrorStatus, NewEvent.ErrorMessage);
            }


            if (NewEvent.ErrorStatus)
            {
                LogStatus(NewEvent, "Adding New Event");
            }

            RemoveAllCalendarEventFromLogAndCalendar();
            try
            {
                AllEventDictionary.Add(NewEvent.ID, NewEvent);
            }
            catch
            {
                AllEventDictionary[NewEvent.ID] = NewEvent;
            }


            WriteFullScheduleToLogAndOutlook();

            CompleteSchedule = getTimeLine();




            return new CustomErrors(NewEvent.ErrorStatus, NewEvent.ErrorMessage);
        }


        public void WriteFullScheduleToOutlook()
        {
            foreach (CalendarEvent MyCalEvent in AllEventDictionary.Values)
            {
                myCalendar[0].WriteToOutlook(MyCalEvent);
            }
        }

        public void WriteFullScheduleToLogAndOutlook()
        {
            myAccount.UpdateReferenceDayTime(ReferenceDayTIime);
            foreach (CalendarEvent MyCalEvent in AllEventDictionary.Values)
            {
                myCalendar[0].WriteToOutlook(MyCalEvent);
            }

            myAccount.CommitEventToLog(AllEventDictionary.Values, EventIDGenerator.LatestID.ToString());

        }

        Dictionary<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> createCopyOfPossibleEvents(Dictionary<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> PossibleEntries)
        {
            Dictionary<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> retValue = new Dictionary<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>>();
            foreach (KeyValuePair<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> eachKeyValuePair in PossibleEntries)
            {
                Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> NewDict0 = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();
                foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair0 in eachKeyValuePair.Value)
                {
                    Dictionary<string, mTuple<bool, SubCalendarEvent>> NewDict1 = new Dictionary<string, mTuple<bool, SubCalendarEvent>>();

                    foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> eachKeyValuePair1 in eachKeyValuePair0.Value)
                    {
                        NewDict1.Add(eachKeyValuePair1.Key, new mTuple<bool, SubCalendarEvent>(eachKeyValuePair1.Value.Item1, eachKeyValuePair1.Value.Item2));
                    }
                    NewDict0.Add(eachKeyValuePair0.Key, NewDict1);
                }

                retValue.Add(eachKeyValuePair.Key, NewDict0);
            }

            return retValue;
        }

        List<List<SubCalendarEvent>> SpreadOutEvents(List<List<SubCalendarEvent>> AlreadyAlignedEvents, Double AverageOccupiedSchedule, List<TimeLine> AllFreeSpots, Dictionary<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> AllPossibleEvents, List<List<SubCalendarEvent>> RestrictedElements)
        {
            //Function takes a List Of TimeLines that have been compressed towards the left of a timeLine and Attempts to spread them out
            List<List<SubCalendarEvent>> retValue = new List<List<SubCalendarEvent>>();
            Dictionary<TimeLine, List<mTuple<double, SubCalendarEvent>>> MovableElements_Dict = new Dictionary<TimeLine, List<mTuple<double, SubCalendarEvent>>>();
            List<mTuple<double, SubCalendarEvent>> TotalMovableList = new List<mTuple<double, SubCalendarEvent>>();
            Dictionary<SubCalendarEvent, Tuple<int, TimeLine>> SubCalEventCurrentlyAssignedTImeLine = new Dictionary<SubCalendarEvent, Tuple<int, TimeLine>>();

            Dictionary<TimeLine, mTuple<TimeSpan, TimeSpan>> TImeLine_ToAverageTimeSpan = new Dictionary<TimeLine, mTuple<TimeSpan, TimeSpan>>();//holds the current timeline to mtuple of ideal average timespan and current total active timespan
            Dictionary<TimeLine, mTuple<int, double>> TimeLineOccupancy = new Dictionary<TimeLine, mTuple<int, double>>();
            int j = 0;
            List<mTuple<TimeLine, int>> LessThanAverage = new List<mTuple<TimeLine, int>>();
            List<mTuple<TimeLine, int>> AboveAverage = new List<mTuple<TimeLine, int>>();

            Dictionary<SubCalendarEvent, List<mTuple<TimeLine, int>>> TimeLineAndPossibleCalendarEvents = new Dictionary<SubCalendarEvent, List<mTuple<TimeLine, int>>>();

            foreach (TimeLine eachTimeLine in AllFreeSpots)
            {
                TimeSpan TotalActiveSpan = Utility.SumOfActiveDuration(AlreadyAlignedEvents[j]);
                TimeSpan AverageTimeSpan = new TimeSpan((long)(AverageOccupiedSchedule * (double)eachTimeLine.TimelineSpan.Ticks));
                TImeLine_ToAverageTimeSpan.Add(eachTimeLine, new mTuple<TimeSpan, TimeSpan>(AverageTimeSpan, TotalActiveSpan));
                double Occupancy = (double)TotalActiveSpan.Ticks / (double)eachTimeLine.TimelineSpan.Ticks;// percentage of active duration relative to the size of the TimeLine Timespan
                TimeLineOccupancy.Add(eachTimeLine, new mTuple<int, double>(j - 1, Occupancy));
                if (Occupancy > AverageOccupiedSchedule)
                {
                    AboveAverage.Add(new mTuple<TimeLine, int>(eachTimeLine, j));
                    List<SubCalendarEvent> AlreadyAssigned = AlreadyAlignedEvents[j];
                    List<SubCalendarEvent> RestrictedSubCalevent = RestrictedElements[j];
                    List<SubCalendarEvent> MovableElements = AlreadyAssigned.ToList();
                    MovableElements = MovableElements.Where(obj => (!RestrictedSubCalevent.Contains(obj))).ToList();



                    IEnumerable<KeyValuePair<SubCalendarEvent, Tuple<int, TimeLine>>> ListOFeachKeyValuePair = MovableElements.Select(obj => new KeyValuePair<SubCalendarEvent, Tuple<int, TimeLine>>(obj, new Tuple<int, TimeLine>(j, AllFreeSpots[j])));
                    foreach (KeyValuePair<SubCalendarEvent, Tuple<int, TimeLine>> eachKeyValuePair in ListOFeachKeyValuePair)
                    {
                        SubCalEventCurrentlyAssignedTImeLine.Add(eachKeyValuePair.Key, eachKeyValuePair.Value);
                    }


                    List<mTuple<double, SubCalendarEvent>> MovableForDict = MovableElements.Select(obj => new mTuple<double, SubCalendarEvent>(DistanceSolver.AverageToAllNodes(obj.myLocation, MovableElements.Where(obj1 => obj1 != obj).ToList().Select(obj2 => obj2.myLocation).ToList()), obj)).ToList();
                    MovableForDict.Sort(delegate(mTuple<double, SubCalendarEvent> A, mTuple<double, SubCalendarEvent> B)
                    {
                        return A.Item1.CompareTo(B.Item1);
                        /*if(A.Item1==B.Item1) return 0;
                        else{
                            A.Item1.CompareTo(A.Item1) 
                        }*/
                    });
                    TotalMovableList.AddRange(MovableForDict);
                    MovableElements_Dict.Add(AllFreeSpots[j], MovableForDict);
                }
                else
                {
                    if (Occupancy < AverageOccupiedSchedule)
                    {
                        double ExcessPercentageSpace = AverageOccupiedSchedule - Occupancy;
                        mTuple<TimeLine, int> LessThanAverageEntry = new mTuple<TimeLine, int>(eachTimeLine, j);
                        LessThanAverage.Add(LessThanAverageEntry);




                        IEnumerable<KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> DictOfPosSubCals0 = AllPossibleEvents[eachTimeLine].Select(obj => obj);
                        IEnumerable<Dictionary<string, mTuple<bool, SubCalendarEvent>>> DictOfPosSubCals1 = DictOfPosSubCals0.Select(obj => obj.Value);


                        //List<List<KeyValuePair<string, mTuple<bool, SubCalendarEvent>>>> DictOfPosSubCals0List = AllPossibleEvents[eachTimeLine].Select(obj => obj.Value).ToList();
                        //IEnumerable<KeyValuePair<string, mTuple<bool, SubCalendarEvent>>> DictOfPosSubCals =    AllPossibleEvents[eachTimeLine].SelectMany(obj=>obj.Value);
                        IEnumerable<SubCalendarEvent> DictOfPosSubCals = AllPossibleEvents[eachTimeLine].SelectMany(obj => obj.Value).Select(obj => obj.Value.Item2);

                        TimeSpan SpaceTogetAverage = new TimeSpan((long)(ExcessPercentageSpace * (double)eachTimeLine.TimelineSpan.Ticks));
                        List<mTuple<double, SubCalendarEvent>> CompatibleWithTimeLine = PopulateCompatibleList(TotalMovableList, DictOfPosSubCals.ToList(), eachTimeLine, SpaceTogetAverage);

                        
                        CompatibleWithTimeLine.AddRange(Utility.SubCalEventsTomTuple(AlreadyAlignedEvents[j], (double)100));
                        foreach (SubCalendarEvent eeachSubCalendarEvent in CompatibleWithTimeLine.Select(obj=>obj.Item2))
                        {
                            if (TimeLineAndPossibleCalendarEvents.ContainsKey(eeachSubCalendarEvent))
                            {
                                TimeLineAndPossibleCalendarEvents[eeachSubCalendarEvent].Add(LessThanAverageEntry);
                            }
                            else
                            {
                                TimeLineAndPossibleCalendarEvents.Add(eeachSubCalendarEvent, new List<mTuple<TimeLine, int>> { (LessThanAverageEntry) });
                            }
                        }


                    }
                }
                j++;
            }

            Dictionary<TimeLine, List<SubCalendarEvent>> OptimumAssignment = AllFreeSpots.ToDictionary(obj => obj, obj=>new List<SubCalendarEvent>());

            foreach (KeyValuePair<SubCalendarEvent, List<mTuple<TimeLine, int>>> eachKeyValuePair in TimeLineAndPossibleCalendarEvents)
            { //find best spots for
                int OptimumLocation = GetBestNodeToInsertSelf(AlreadyAlignedEvents, eachKeyValuePair.Key, eachKeyValuePair.Value, CalendarEvent.DistanceMatrix, OptimumAssignment);

                mTuple<TimeLine,int> OptimumIndex= eachKeyValuePair.Value[OptimumLocation];
                OptimumAssignment[OptimumIndex.Item1].Add(eachKeyValuePair.Key);
            }



            foreach (TimeLine eachTimeLine in LessThanAverage.Select(obj => obj.Item1))
            {
                j=AllFreeSpots.IndexOf(eachTimeLine);
                TimeSpan TotalActiveSpan = Utility.SumOfActiveDuration(AlreadyAlignedEvents[j]);
                TimeSpan AverageTimeSpan = new TimeSpan((long)(AverageOccupiedSchedule * (double)eachTimeLine.TimelineSpan.Ticks));
                //TImeLine_ToAverageTimeSpan.Add(eachTimeLine, new mTuple<TimeSpan, TimeSpan>(AverageTimeSpan, TotalActiveSpan));
                double Occupancy = (double)TotalActiveSpan.Ticks / (double)eachTimeLine.TimelineSpan.Ticks;// percentage of active duration relative to the size of the TimeLine Timespan


                Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithListForFunCall = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
                Dictionary<SubCalendarEvent, BusyTimeLine> SubCalendarEvent_OldTImeLine = new Dictionary<SubCalendarEvent, BusyTimeLine>();//Dictionary stores the Subcalendar event old TimeLine, just incase the do not get reassigned to the current timeline
                Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEventsForFuncCall = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();
                List<mTuple<bool, SubCalendarEvent>> restrictedForFuncCall = Utility.SubCalEventsTomTuple(AlreadyAlignedEvents[j], true);
                foreach (SubCalendarEvent eachmTuple in OptimumAssignment[eachTimeLine])
                {
                    TimeSpan ActiveTimeSpan = eachmTuple.ActiveDuration;
                    string subcalStringID = eachmTuple.ID;
                    SubCalendarEvent_OldTImeLine.Add(eachmTuple, eachmTuple.ActiveSlot.CreateCopy());

                    if (CompatibleWithListForFunCall.ContainsKey(ActiveTimeSpan))
                    {
                        ++CompatibleWithListForFunCall[ActiveTimeSpan].Item1;
                        ;
                    }
                    else
                    {
                        CompatibleWithListForFunCall.Add(ActiveTimeSpan, new mTuple<int, TimeSpanWithStringID>(1, new TimeSpanWithStringID(eachmTuple.ActiveDuration, ActiveTimeSpan.Ticks.ToString())));
                    }

                    if (PossibleEventsForFuncCall.ContainsKey(ActiveTimeSpan))
                    {
                        PossibleEventsForFuncCall[ActiveTimeSpan].Add(subcalStringID, new mTuple<bool, SubCalendarEvent>(true, eachmTuple));
                    }
                    else
                    {
                        PossibleEventsForFuncCall.Add(ActiveTimeSpan, new Dictionary<string, mTuple<bool, SubCalendarEvent>>());
                        PossibleEventsForFuncCall[ActiveTimeSpan].Add(subcalStringID, new mTuple<bool, SubCalendarEvent>(true, eachmTuple));
                    }
                }



                List<mTuple<bool, SubCalendarEvent>> UpdatedListForTimeLine = stitchUnRestrictedSubCalendarEvent(eachTimeLine, restrictedForFuncCall, PossibleEventsForFuncCall, CompatibleWithListForFunCall, Occupancy);//attempts to add new events into to the timelines with lesser occupancy than average

                TimeSpan OccupiedSpace = Utility.SumOfActiveDuration(UpdatedListForTimeLine.Select(obj => obj.Item2).ToList());//checks for how much space is used up
                TimeSpan ExcessSpace = OccupiedSpace - AverageTimeSpan;//checks how much excees
                if (ExcessSpace.Ticks > 0)//tries to trim the UpdatedListForTimeLine. This is done by removing an element in the updated list until its detected that the origin timeline is below or equal to its average
                {
                    IEnumerable<SubCalendarEvent> NewlyAddedElements = (UpdatedListForTimeLine.Where(obj => !AlreadyAlignedEvents[j].Contains(obj.Item2))).Select(obj => obj.Item2);//retrieves the newly added elements
                    List<mTuple<double, SubCalendarEvent>> NewlyAddedElementsWithCost = NewlyAddedElements.Select(obj => new mTuple<double, SubCalendarEvent>(DistanceSolver.AverageToAllNodes(obj.myLocation, UpdatedListForTimeLine.Select(obj3 => obj3.Item2).Where(obj1 => obj1 != obj).ToList().Select(obj2 => obj2.myLocation).ToList()), obj)).ToList();//creates mtuple of cost and subcal events
                    Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibilityListFOrTIghtestForExtraAverga = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
                    Dictionary<TimeSpan, List<mTuple<double, SubCalendarEvent>>> CompatibilityListForNewlyAddedElements = new Dictionary<TimeSpan, List<mTuple<double, SubCalendarEvent>>>();
                    foreach (mTuple<double, SubCalendarEvent> eachmTUple in NewlyAddedElementsWithCost)
                    {
                        if (CompatibilityListForNewlyAddedElements.ContainsKey(eachmTUple.Item2.ActiveDuration))
                        {
                            CompatibilityListForNewlyAddedElements[eachmTUple.Item2.ActiveDuration].Add(eachmTUple);
                            //CompatibilityListForNewlyAddedElements[eachmTUple.Item2.ActiveDuration] = Utility.RandomizeIEnumerable(CompatibilityListForNewlyAddedElements[eachmTUple.Item2.ActiveDuration]);
                            CompatibilityListForNewlyAddedElements[eachmTUple.Item2.ActiveDuration].Sort(delegate(mTuple<double, SubCalendarEvent> A, mTuple<double, SubCalendarEvent> B)
                            {
                                return A.Item1.CompareTo(B.Item1);
                            });
                        }
                        else
                        {
                            CompatibilityListForNewlyAddedElements.Add(eachmTUple.Item2.ActiveDuration, new List<mTuple<double, SubCalendarEvent>>() { eachmTUple });
                        }

                        if (CompatibilityListFOrTIghtestForExtraAverga.ContainsKey(eachmTUple.Item2.ActiveDuration))
                        {
                            ++CompatibilityListFOrTIghtestForExtraAverga[eachmTUple.Item2.ActiveDuration].Item1;
                        }
                        else
                        {
                            CompatibilityListFOrTIghtestForExtraAverga.Add(eachmTUple.Item2.ActiveDuration, new mTuple<int, TimeSpanWithStringID>(1, new TimeSpanWithStringID(eachmTUple.Item2.ActiveDuration, eachmTUple.Item2.ActiveDuration.Ticks.ToString())));
                        }
                    }

                    TimeSpan Space_NonAverage = TimeSpan.FromTicks((long)((1 - AverageOccupiedSchedule) * eachTimeLine.TimelineSpan.Ticks));//Space derive from subtracting the calculated expected average timespan for this time line from thie timeline
                    //ExcessSpace = Space_NonAverage;

                    SnugArray CompatibilityToBestAverageFit = new SnugArray(CompatibilityListFOrTIghtestForExtraAverga.Values.ToList(), ExcessSpace);
                    List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AllPossibleTIghtExcessFits = CompatibilityToBestAverageFit.MySnugPossibleEntries;
                    AllPossibleTIghtExcessFits = SnugArray.SortListSnugPossibilities_basedOnTimeSpan(AllPossibleTIghtExcessFits);
                    Dictionary<int, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> tightestElements = SnugArray.SortListSnugPossibilities_basedOnNumberOfDiffering(AllPossibleTIghtExcessFits);
                    if (tightestElements.Count > 0)
                    {
                        AllPossibleTIghtExcessFits = tightestElements.OrderBy(obj => obj.Key).Last().Value;
                    }

                    //SnugArray.SortListSnugPossibilities_basedOnTimeSpan(AllPossibleTIghtExcessFits);
                    List<mTuple<double, SubCalendarEvent>> removedElements = new List<mTuple<double, SubCalendarEvent>>();//stores element that dont get reassigned to this current timeLine
                    if (AllPossibleTIghtExcessFits.Count > 0)
                    {
                        Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> TIghtestFit = AllPossibleTIghtExcessFits[AllPossibleTIghtExcessFits.Count - 1];
                        foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in TIghtestFit)//Hack alert: Assumes tightest fit is most diverse
                        {

                            while (eachKeyValuePair.Value.Item1 > 0)
                            {
                                removedElements.Add(CompatibilityListForNewlyAddedElements[eachKeyValuePair.Value.Item2.timeSpan][CompatibilityListForNewlyAddedElements[eachKeyValuePair.Value.Item2.timeSpan].Count - 1]);
                                CompatibilityListForNewlyAddedElements[eachKeyValuePair.Value.Item2.timeSpan].RemoveAt(CompatibilityListForNewlyAddedElements[eachKeyValuePair.Value.Item2.timeSpan].Count - 1);
                                --eachKeyValuePair.Value.Item1;
                            }
                        }
                    }

                    NewlyAddedElements = CompatibilityListForNewlyAddedElements.SelectMany(obj => obj.Value).Select(obj => obj.Item2);
                    UpdatedListForTimeLine.RemoveAll(obj => removedElements.Select(obj1 => obj1.Item2).Contains(obj.Item2));//use LINQ to remove elements currently in "removedElements"
                    List<SubCalendarEvent> ListOfNewlyAddeedElements = NewlyAddedElements.ToList();

                    TimeSpan AllSumTImeSpan = Utility.SumOfActiveDuration(ListOfNewlyAddeedElements);
                    removedElements.ForEach(obj => obj.Item2.shiftEvent(SubCalendarEvent_OldTImeLine[obj.Item2].Start - obj.Item2.ActiveSlot.Start));


                    Dictionary<int, List<List<SubCalendarEvent>>> CurrentlyAssignedSubCalevents = new Dictionary<int, List<List<SubCalendarEvent>>>();//stores the index of each subcalendarevent timeline and its fellow Subcal events. Key= Index Of Current timeline. OuteList is grouping for each calendar event. Inner List is each Subcalevent within Calevent

                    //ListOfNewlyAddeedElements = Utility.RandomizeIEnumerable(ListOfNewlyAddeedElements);

                    for (int i = 0; i < ListOfNewlyAddeedElements.Count; i++)//removes Each reassigned element from its currently attached field
                    {
                        SubCalendarEvent eachSubCalendarEvent = ListOfNewlyAddeedElements[i];
                        Tuple<int, TimeLine> CurrentMatchingField = SubCalEventCurrentlyAssignedTImeLine[eachSubCalendarEvent];
                        mTuple<TimeSpan, TimeSpan> AverageTimeSpanAndTotalTimeSpan = TImeLine_ToAverageTimeSpan[CurrentMatchingField.Item2];
                        if (((AverageTimeSpanAndTotalTimeSpan.Item2 - eachSubCalendarEvent.ActiveDuration) >= AverageTimeSpanAndTotalTimeSpan.Item1))
                        {
                            AverageTimeSpanAndTotalTimeSpan.Item2 -= eachSubCalendarEvent.ActiveDuration;
                            AlreadyAlignedEvents[CurrentMatchingField.Item1].Remove(eachSubCalendarEvent);
                            SubCalEventCurrentlyAssignedTImeLine[eachSubCalendarEvent] = new Tuple<int, TimeLine>(j, AllFreeSpots[j]);

                            Dictionary<TimeLine, Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> AllPossibleEvents222222;
                            TotalMovableList.RemoveAll(obj => NewlyAddedElements.Contains(obj.Item2));//removes the newly added element from Total possible movable elements
                            AllPossibleEvents[AllFreeSpots[j]][eachSubCalendarEvent.ActiveDuration].Remove(eachSubCalendarEvent.ID);
                            if (AllPossibleEvents[AllFreeSpots[j]][eachSubCalendarEvent.ActiveDuration].Count < 1)
                            {
                                AllPossibleEvents[AllFreeSpots[j]].Remove(eachSubCalendarEvent.ActiveDuration);
                            }
                        }
                        else
                        {
                            ListOfNewlyAddeedElements.Remove(eachSubCalendarEvent);
                            eachSubCalendarEvent.shiftEvent(SubCalendarEvent_OldTImeLine[eachSubCalendarEvent].Start - eachSubCalendarEvent.ActiveSlot.Start);
                            --i;
                        }
                    }

                    NewlyAddedElements = ListOfNewlyAddeedElements;

                    AlreadyAlignedEvents[j].AddRange(NewlyAddedElements);

                    TotalActiveSpan = Utility.SumOfActiveDuration(AlreadyAlignedEvents[j]);
                    AverageTimeSpan = new TimeSpan((long)(AverageOccupiedSchedule * (double)eachTimeLine.TimelineSpan.Ticks));
                    Occupancy = (double)TotalActiveSpan.Ticks / (double)eachTimeLine.TimelineSpan.Ticks;// percentage of active duration relative to the size of the TimeLine Timespan

                }


            }


            Dictionary<TimeLine, List<SubCalendarEvent>> CompatibleList = new Dictionary<TimeLine, List<SubCalendarEvent>>();//this Dictionary stores keyValuepair of a TimeLine and Subcalevents that can work within said timeLine that are not part of the currently assigned set;
            retValue = AlreadyAlignedEvents.ToList();
            return retValue;
        }


        int GetBestNodeToInsertSelf(List<List<SubCalendarEvent>> AllSubEvents, SubCalendarEvent CurrentSubEvent, List<mTuple<TimeLine, int>> ValidLocations, Dictionary<string, List<Double>> DistanceMatrix,Dictionary<TimeLine, List<SubCalendarEvent>> OptimumSelected)
        {
            int retValue=-1;
            List<double> AverageDistances = (new double[(ValidLocations.Count)]).ToList();
            
            for(int i=0; i<ValidLocations.Count;i++)    
            {
                AverageDistances[i]=DistanceSolver.AverageToAllNodes(CurrentSubEvent, AllSubEvents[ValidLocations[i].Item2].Concat(OptimumSelected[ValidLocations[i].Item1]).ToList(), DistanceMatrix);
            }


            retValue = AverageDistances.IndexOf(AverageDistances.Min());

            return retValue;

        }


        List<mTuple<double, SubCalendarEvent>> PopulateCompatibleList(List<mTuple<double, SubCalendarEvent>> AllSubCalEvents, List<SubCalendarEvent> PossibleSubcalEvents, TimeLine PertinentTimeLine, TimeSpan TotalFreeSpaceInTImeLine)
        {
            /*
             * this function generates a list of List<mTuple<double, SubCalendarEvent>> that can work within the specified timeLine.
             * The PossibleSubcalevents parameter is a list of possible Calendar events that were calculated to be permissible within this timeLine. Note this includes the restricted valeues
             * The AllSubcalevents is a list of mTuples. Each mtuple has a subcalendar event and its average distance cost to the other nodes within its timeLine
             */

            IEnumerable<SubCalendarEvent> AllSubCalEvents_Unverified = AllSubCalEvents.Select(obj => obj.Item2);//generates an IEnumerableOf Subcalevents from the AllSubCalEvents which is an mTUple
            IEnumerable<SubCalendarEvent> UsableSubCalevents = AllSubCalEvents_Unverified.Where(obj => PossibleSubcalEvents.Contains(obj));//checks if the subcalevents are possible for the Timeline
            IEnumerable<mTuple<double, SubCalendarEvent>> UsableWithinTIimeLineAndFits = AllSubCalEvents.Where(obj1 => ((UsableSubCalevents.Contains(obj1.Item2)) && ((obj1.Item2.ActiveDuration <= TotalFreeSpaceInTImeLine))));//checks if the active duration will possibly fit the timespan requried to reach average
            List<mTuple<double, SubCalendarEvent>> retValue = UsableWithinTIimeLineAndFits.ToList();
            return retValue;
        }

        bool ShiftEvent(CalendarEvent CurrentEvent, TimeSpan DelayTime)
        {
            return true;
        }

        public CalendarEvent GenerateRigidSubEvents(CalendarEvent MyCalendarEvent)
        {
            int i = 0;
            List<SubCalendarEvent> MyArrayOfSubEvents = new List<SubCalendarEvent>();
            for (; i < MyCalendarEvent.ActiveSubEvents.Length; i++)
            {

            }

            if (MyCalendarEvent.RepetitionStatus)
            {

                SubCalendarEvent MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start, MyCalendarEvent.Repeat.Range.End, MyCalendarEvent.Preparation, MyCalendarEvent.ID, MyCalendarEvent.Rigid, MyCalendarEvent.isEnabled, MyCalendarEvent.UIParam, MyCalendarEvent.Notes, MyCalendarEvent.isComplete, MyCalendarEvent.myLocation, MyCalendarEvent.RangeTimeLine);


                //new SubCalendarEvent(MyCalendarEvent.End, MyCalendarEvent.Repeat.Range.Start, MyCalendarEvent.Repeat.Range.End, MyCalendarEvent.Preparation, MyCalendarEvent.ID);

                for (; MySubEvent.Start < MyCalendarEvent.Repeat.Range.End; )
                {
                    MyArrayOfSubEvents.Add(MySubEvent);
                    switch (MyCalendarEvent.Repeat.Frequency)
                    {
                        case "DAILY":
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start.AddDays(1), MyCalendarEvent.Repeat.Range.End.AddDays(1), MyCalendarEvent.Preparation, MyCalendarEvent.ID, MyCalendarEvent.Rigid, MyCalendarEvent.isEnabled, MyCalendarEvent.UIParam, MyCalendarEvent.Notes, MyCalendarEvent.isComplete, MyCalendarEvent.myLocation, MyCalendarEvent.RangeTimeLine);
                                break;
                            }
                        case "WEEKLY":
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start.AddDays(7), MyCalendarEvent.Repeat.Range.End.AddDays(7), MyCalendarEvent.Preparation, MyCalendarEvent.ID, MyCalendarEvent.Rigid, MyCalendarEvent.isEnabled, MyCalendarEvent.UIParam, MyCalendarEvent.Notes, MyCalendarEvent.isComplete, MyCalendarEvent.myLocation, MyCalendarEvent.RangeTimeLine);
                                break;
                            }
                        case "BI-WEEKLY":
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start.AddDays(14), MyCalendarEvent.Repeat.Range.End.AddDays(14), MyCalendarEvent.Preparation, MyCalendarEvent.ID, MyCalendarEvent.Rigid, MyCalendarEvent.isEnabled, MyCalendarEvent.UIParam, MyCalendarEvent.Notes, MyCalendarEvent.isComplete, MyCalendarEvent.myLocation, MyCalendarEvent.RangeTimeLine);
                                break;
                            }
                        case "MONTHLY":
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start.AddMonths(1), MyCalendarEvent.Repeat.Range.End.AddMonths(1), MyCalendarEvent.Preparation, MyCalendarEvent.ID, MyCalendarEvent.Rigid, MyCalendarEvent.isEnabled, MyCalendarEvent.UIParam, MyCalendarEvent.Notes, MyCalendarEvent.isComplete, MyCalendarEvent.myLocation, MyCalendarEvent.RangeTimeLine);
                                break;
                            }
                        case "YEARLY":
                            {
                                MySubEvent = new SubCalendarEvent(MyCalendarEvent.ActiveDuration, MyCalendarEvent.Repeat.Range.Start.AddYears(1), MyCalendarEvent.Repeat.Range.End.AddYears(1), MyCalendarEvent.Preparation, MyCalendarEvent.ID, MyCalendarEvent.Rigid, MyCalendarEvent.isEnabled, MyCalendarEvent.UIParam, MyCalendarEvent.Notes, MyCalendarEvent.isComplete, MyCalendarEvent.myLocation, MyCalendarEvent.RangeTimeLine);
                                break;
                            }
                    }


                }
            }

            return MyCalendarEvent;
            //
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



        bool[] CheckIfPotentialSubEventClashesWithAnyOtherSubEvents(CalendarEvent MyPotentialCalendarEvent, TimeLine MyTimeLineOfEvent)
        {
            BusyTimeLine[] ArrayOfBusySlots = MyTimeLineOfEvent.OccupiedSlots;
            bool[] StatusOfCollision = new bool[] { false, false };
            foreach (BusyTimeLine MyBusySlot in ArrayOfBusySlots)
            {
                if (MyBusySlot.doesTimeLineInterfere(MyPotentialCalendarEvent.RangeTimeLine))
                {
                    StatusOfCollision[0] = true;
                    if (AllEventDictionary[(new EventID(MyBusySlot.TimeLineID)).getLevelID(0)].Rigid)
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

        public CalendarEvent EvaluateTotalTimeLineAndAssignValidTimeSpots(CalendarEvent MyEvent,HashSet<SubCalendarEvent> UnDoneEvents, List<CalendarEvent> NoneCOmmitedCalendarEvent = null, int InterringWithNowEvent=0)
        {
            int i = 0;
            if (NoneCOmmitedCalendarEvent == null)
            {
                NoneCOmmitedCalendarEvent = new List<CalendarEvent>();
            }
            if (MyEvent.RepetitionStatus)
            {
                for (i = 0; i < MyEvent.Repeat.RecurringCalendarEvents.Length; i++)
                {
                    MyEvent.Repeat.RecurringCalendarEvents[i] = EvaluateTotalTimeLineAndAssignValidTimeSpots(MyEvent.Repeat.RecurringCalendarEvents[i], UnDoneEvents, NoneCOmmitedCalendarEvent);
                    NoneCOmmitedCalendarEvent.Add(MyEvent.Repeat.RecurringCalendarEvents[i]);
                    if (MyEvent.Repeat.RecurringCalendarEvents[i].ErrorStatus)
                    {
                        MyEvent.UpdateError(MyEvent.Repeat.RecurringCalendarEvents[i].Error);
                    }
                }
                return MyEvent;
            }

            BusyTimeLine[] AllOccupiedSlot = CompleteSchedule.OccupiedSlots;
            TimeSpan TotalActiveDuration = new TimeSpan();
            TimeLine[] TimeLineArrayWithSubEventsAssigned = new TimeLine[MyEvent.ActiveSubEvents.Length];
            SubCalendarEvent TempSubEvent = new SubCalendarEvent();
            BusyTimeLine MyTempBusyTimerLine = new BusyTimeLine();
            List<TimeLine> FreeSpotsAvailableWithinValidTimeline = getAllFreeSpots(new TimeLine(MyEvent.Start, MyEvent.End)).ToList();

            FreeSpotsAvailableWithinValidTimeline = CheckTimeLineListForEncompassingTimeLine(FreeSpotsAvailableWithinValidTimeline, MyEvent.RangeTimeLine);

            FreeSpotsAvailableWithinValidTimeline = getOnlyPertinentTimeFrame(FreeSpotsAvailableWithinValidTimeline.ToArray(), new TimeLine(MyEvent.Start, MyEvent.End));

            i = 0;
            TimeSpan TotalFreeTimeAvailable = new TimeSpan();
            if (MyEvent.Rigid)
            {
                TempSubEvent = new SubCalendarEvent(MyEvent.ActiveDuration, MyEvent.Start, MyEvent.End, MyEvent.Preparation, MyEvent.ID, MyEvent.Rigid, MyEvent.isEnabled, MyEvent.UIParam, MyEvent.Notes, MyEvent.isComplete, MyEvent.myLocation, MyEvent.RangeTimeLine);
                MyTempBusyTimerLine = new BusyTimeLine(TempSubEvent.ID, TempSubEvent.Start, TempSubEvent.End);
                TempSubEvent = new SubCalendarEvent(TempSubEvent.ID, TempSubEvent.Start, TempSubEvent.End, MyTempBusyTimerLine, MyEvent.Rigid, TempSubEvent.isEnabled, TempSubEvent.UIParam, TempSubEvent.Notes, TempSubEvent.isComplete, MyEvent.myLocation, MyEvent.RangeTimeLine);
                MyEvent.updateSubEvent(TempSubEvent.SubEvent_ID, TempSubEvent);


                KeyValuePair<CalendarEvent, TimeLine> TimeLineAndCalendarUpdated = ReArrangeClashingEventsofRigid(MyEvent, NoneCOmmitedCalendarEvent.ToList(), InterringWithNowEvent);

                CalendarEvent MyCalendarEventUpdated = TimeLineAndCalendarUpdated.Key;

                if (MyCalendarEventUpdated != null && !MyCalendarEventUpdated.ErrorStatus)
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
                                //bool Verified = AllEventDictionary[ParentID].updateSubEvent(new EventID(MyBusyTimeLine.TimeLineID), new SubCalendarEvent(MyBusyTimeLine.TimeLineID, MyBusyTimeLine.Start, MyBusyTimeLine.End, MyBusyTimeLine, AllEventDictionary[ParentID].Rigid, AllEventDictionary[ParentID].myLocation));
                                SubCalendarEvent referenceSubCalEvent = AllEventDictionary[ParentID].getSubEvent(new EventID(MyBusyTimeLine.TimeLineID));
                                referenceSubCalEvent.shiftEvent(MyBusyTimeLine.Start - referenceSubCalEvent.Start);
                            }
                            else
                            {
                                MyArrayOfSubCalendarEvents = AllEventDictionary[ParentID].AllSubEvents;
                                for (i = 0; i < MyArrayOfSubCalendarEvents.Length; i++)
                                {
                                    if (MyArrayOfSubCalendarEvents[i].ID == MyBusyTimeLine.TimeLineID)
                                    {
                                        //string ThirdPartyID = MyArrayOfSubCalendarEvents[i].ThirdPartyID;
                                        //SubCalendarEvent newSubCalevent = new SubCalendarEvent(MyBusyTimeLine.TimeLineID, MyBusyTimeLine.Start, MyBusyTimeLine.End, MyBusyTimeLine, MyArrayOfSubCalendarEvents[i].Rigid, MyArrayOfSubCalendarEvents[i].myLocation, AllEventDictionary[ParentID].RangeTimeLine);
                                        //newSubCalevent.ThirdPartyID = ThirdPartyID;
                                        //AllEventDictionary[ParentID].updateSubEvent(newSubCalevent.SubEvent_ID, newSubCalevent);
                                        SubCalendarEvent referenceSubCalEvent = MyArrayOfSubCalendarEvents[i];
                                        referenceSubCalEvent.shiftEvent(MyBusyTimeLine.Start - referenceSubCalEvent.Start);
                                        
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (MyCalendarEventUpdated == null)
                    {
                        MyCalendarEventUpdated = MyEvent;
                    }
                }

                return MyCalendarEventUpdated;
            }
            else
            {
                for (i = 0; i < FreeSpotsAvailableWithinValidTimeline.Count; i++)
                {
                    TotalFreeTimeAvailable += FreeSpotsAvailableWithinValidTimeline[i].TimelineSpan;
                }

                //if (TotalFreeTimeAvailable >= MyEvent.ActiveDuration)
                {
                    /*TimeLineArrayWithSubEventsAssigned = SplitFreeSpotsInToSubEventTimeSlots(FreeSpotsAvailableWithinValidTimeline.ToArray(), MyEvent.AllEvents.Length, MyEvent.ActiveDuration);
                    if (TimeLineArrayWithSubEventsAssigned == null)*/
                    {
                        BusyTimeLine[] CompleteScheduleOccupiedSlots = CompleteSchedule.OccupiedSlots;
                        KeyValuePair<CalendarEvent, TimeLine> TimeLineAndCalendarUpdated = ReArrangeTimeLineWithinWithinCalendaEventRange(MyEvent, NoneCOmmitedCalendarEvent.ToList(), InterringWithNowEvent, UnDoneEvents);
                        CalendarEvent MyCalendarEventUpdated = TimeLineAndCalendarUpdated.Key;
                        //CompleteSchedule.OccupiedSlots = TimeLineAndCalendarUpdated.Value.OccupiedSlots;//hack need to review architecture to avoid this assignment
                        if (MyCalendarEventUpdated != null)// && !MyCalendarEventUpdated.ErrorStatus)
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
                                        //bool Verified = AllEventDictionary[ParentID].updateSubEvent(new EventID(MyBusyTimeLine.TimeLineID), new SubCalendarEvent(MyBusyTimeLine.TimeLineID, MyBusyTimeLine.Start, MyBusyTimeLine.End, MyBusyTimeLine, AllEventDictionary[ParentID].Rigid, AllEventDictionary[ParentID].myLocation));
                                        SubCalendarEvent referenceSubCalEvent = AllEventDictionary[ParentID].getSubEvent(new EventID(MyBusyTimeLine.TimeLineID));
                                        referenceSubCalEvent.shiftEvent(MyBusyTimeLine.Start - referenceSubCalEvent.Start);
                                    }
                                    else
                                    {
                                        MyArrayOfSubCalendarEvents = AllEventDictionary[ParentID].ActiveSubEvents;
                                        for (i = 0; i < MyArrayOfSubCalendarEvents.Length; i++)
                                        {
                                            if (MyArrayOfSubCalendarEvents[i] != null)//for procrastinate scenario where subcalevents get removed
                                            {
                                                if (MyArrayOfSubCalendarEvents[i].ID == MyBusyTimeLine.TimeLineID)
                                                {
                                                    SubCalendarEvent referenceSubCalEvent = MyArrayOfSubCalendarEvents[i];
                                                    referenceSubCalEvent.shiftEvent(MyBusyTimeLine.Start - referenceSubCalEvent.Start);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            return MyCalendarEventUpdated;
                        }
                        else
                        {
                            throw new Exception("code generated a null calendar event. THis is weird");
                        }
                    }
                }
            }



            return MyEvent;
        }


        List<TimeLine> getOnlyCompatibleTimeLines(SubCalendarEvent SubEvent, List<TimeLine> TimeLines)
        {
            List<TimeLine> retValue = new System.Collections.Generic.List<TimeLine>();

            foreach (TimeLine eachTimeLine in TimeLines)
            {
                if (SubEvent.canExistWithinTimeLine(eachTimeLine))
                {
                    retValue.Add(eachTimeLine);
                }
            }


            return retValue;
        }



        Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> generateConstrainedList(List<TimeLine> AvailableTImeLines, List<mTuple<bool, SubCalendarEvent>> AllEvents)
        {
            Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> retValue = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();

            foreach (TimeLine eachTimeLine in AvailableTImeLines)
            {
                retValue.Add(eachTimeLine, new List<mTuple<bool, SubCalendarEvent>>());
            }

            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in AllEvents)
            {
                List<TimeLine> CompatibleTimeLines = getOnlyCompatibleTimeLines(eachmTuple.Item2, AvailableTImeLines);
                if (CompatibleTimeLines.Count == 1)
                {
                    retValue[CompatibleTimeLines[0]].Add(eachmTuple);
                }
            }


            return retValue;
        }

        CalendarEvent CheckUncommitedForSubCalevent(List<CalendarEvent> UncommitedCalendarEvents, SubCalendarEvent possibleSubCalendarevent)
        {
            List<CalendarEvent> PertinentCalendarEvent = UncommitedCalendarEvents.Where(obj => obj.ActiveSubEvents.Contains(possibleSubCalendarevent)).ToList();
            if (PertinentCalendarEvent.Count > 0)
            {
                return PertinentCalendarEvent[0];
            }
            else { return null; }
        }

        Dictionary<CalendarEvent, List<SubCalendarEvent>> generateDictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents(List<SubCalendarEvent> ListOfInterferringElements, List<CalendarEvent> UncommitedCalendarEvents)
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
                string ParentID = MyEventID.getLevelID(0);//This gets the parentID of the SubCalendarEventID
                CalendarEvent UncomittedCalendar = CheckUncommitedForSubCalevent(UncommitedCalendarEvents, ListOfInterferringElements[i]);
                if (UncomittedCalendar != null)
                {

                    if (DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.ContainsKey(UncomittedCalendar))
                    {
                        DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[UncomittedCalendar].Add(ListOfInterferringElements[i]);
                        j++;
                    }
                    else
                    {
                        DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Add(UncomittedCalendar, new List<SubCalendarEvent>());
                        DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[UncomittedCalendar].Add(ListOfInterferringElements[i]);
                        j++;
                    }
                }

                else
                {
                    if (AllEventDictionary[ParentID].RepetitionStatus)
                    {
                        CalendarEvent repeatCalEvent = AllEventDictionary[ParentID].getRepeatedCalendarEvent(MyEventID.getStringIDAtLevel(1));

                        if (DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.ContainsKey(repeatCalEvent))
                        {
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[repeatCalEvent].Add(ListOfInterferringElements[i]);
                            j++;
                        }
                        else
                        {
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Add(repeatCalEvent, new List<SubCalendarEvent>());
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[repeatCalEvent].Add(ListOfInterferringElements[i]);
                            j++;
                        }


                    }
                    else
                    {
                        CalendarEvent nonRepeatCalEvent = AllEventDictionary[ParentID];

                        if (DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.ContainsKey(nonRepeatCalEvent))
                        {
                            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents[nonRepeatCalEvent].Add(ListOfInterferringElements[i]);
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


        Tuple<IEnumerable<SubCalendarEvent>, DateTime,int> getStartTimeWhenCurrentTimeClashesWithSubcalevent(IEnumerable<SubCalendarEvent> CollectionOfSubCalEvent, DateTime ReferenceTime, int FlagType)
        {
            /*
             * function tries to derive the best start time and valid subcalendar events for the evaluation of schedule
             * It takes a collection of possible subcalendar events and an initializing reference time, the latter serves as the initializing position for the calculation.
             * It compares the referencetime with "Now". If Referencetime is earlier than Now it sets the reference time to now. Function checks if now clashes with any events. If it does, it checks if its past 10% of its duration. If yes, it drops it as an interferring element. It selects the end time of the Subcalendarevent (which is interferred by now) as the new reference time
             */
            ReferenceTime = ReferenceTime > Now ? ReferenceTime : Now;
            IEnumerable<SubCalendarEvent> retrievedData;
            IEnumerable<SubCalendarEvent> tenPercentdateTimeLine;
            Tuple<IEnumerable<SubCalendarEvent>, DateTime, int> retValue;
            int retFlagType = FlagType;
            
            CollectionOfSubCalEvent = CollectionOfSubCalEvent.Where(obj => obj.End > ReferenceTime);//ensures we are selecting the sub events that are actuve after the reference time
            if (ReferenceTime == Now)
            {
                IEnumerable<SubCalendarEvent> interfersWithNow = CollectionOfSubCalEvent.Where(obj => obj.IsDateTimeWithin(ReferenceTime));
                List<SubCalendarEvent> currentList = CollectionOfSubCalEvent.ToList();
                if (interfersWithNow.Count() > 0)
                {
                    switch (FlagType)
                    {
                        case 0://Do not include event interferring with now in calculations. Select the element with last deadline and set event endtime as now. THis ensures that any further calculations do not try to include now
                            {
                                interfersWithNow = interfersWithNow.OrderBy(obj => obj.End);
                                DateTime newNow = interfersWithNow.Last().End;
                                UpdateNow(newNow);
                                ReferenceTime = newNow;
                                CollectionOfSubCalEvent = currentList.Where(obj => !interfersWithNow.Contains(obj));
                                retFlagType = 1;
                            }
                            break;
                        case 1:
                            {
                                interfersWithNow = interfersWithNow.OrderBy(obj => obj.End);
                                retFlagType = 1;
                            }
                            break;
                        default:
                            { }
                            break;
                    }
                }
                retValue = new Tuple<IEnumerable<SubCalendarEvent>, DateTime, int>(CollectionOfSubCalEvent, ReferenceTime, retFlagType);
            }
            else
            {
                retrievedData = CollectionOfSubCalEvent.Where(obj => obj.RangeTimeLine.IsDateTimeWithin(ReferenceTime));//selects subcal event in which the Reference time intersect with the timeline. Just in case the reference timeline currently intersects it can reel in that new sub calendar event
                retrievedData = retrievedData.OrderBy(obj => obj.Start);
                List<SubCalendarEvent> allData = retrievedData.ToList();
                DateTime retValueDateTime = ReferenceTime;
                IEnumerable<SubCalendarEvent> retValueIenumerable = CollectionOfSubCalEvent;
                if (allData.Count > 0)
                {
                    retValueDateTime = allData[0].Start;
                    return getStartTimeWhenCurrentTimeClashesWithSubcalevent(retValueIenumerable, retValueDateTime, FlagType);
                }
                retValue = new Tuple<IEnumerable<SubCalendarEvent>, DateTime, int>(retValueIenumerable, retValueDateTime, FlagType);
            }

            return retValue;



            /*
            if (ReferenceTime == Now)
            {
                retrievedData = CollectionOfSubCalEvent.Where(obj => obj.RangeTimeLine.IsDateTimeWithin(ReferenceTime));//selects subcal event in which the Reference time intersect with the timeline
                tenPercentdateTimeLine = retrievedData.Where(obj => ((ReferenceTime >= obj.Start.Add(TimeSpan.FromTicks((long)(obj.RangeTimeLine.TimelineSpan.Ticks * .1)))) || obj.Rigid));//selects element that have 10% of events duration completed, also selects rigid elements
                tenPercentdateTimeLine.OrderBy(obj => obj.End);//sorts interferring events based on end time of events
                List<SubCalendarEvent> allData = tenPercentdateTimeLine.ToList();
                DateTime retValueDateTime = ReferenceTime;
                IEnumerable<SubCalendarEvent> retValueIenumerable = CollectionOfSubCalEvent;
                if (allData.Count > 0)
                {
                    retValueDateTime = allData[allData.Count - 1].End;//sets datetime to the latter of all deadlines
                    retrievedData = CollectionOfSubCalEvent.Where(obj => !tenPercentdateTimeLine.Contains(obj));//removes clashing rigids and elements that are completed by 10 mins
                    retValueIenumerable = retrievedData;
                    return getStartTimeWhenCurrentTimeClashesWithSubcalevent(retrievedData, retValueDateTime);
                }

                return new Tuple<IEnumerable<SubCalendarEvent>, DateTime>(retValueIenumerable, retValueDateTime);
            }
            else //reference time is ahead after now
             
            {
                retrievedData = CollectionOfSubCalEvent.Where(obj => obj.RangeTimeLine.IsDateTimeWithin(ReferenceTime));//selects subcal event in which the Reference time intersect with the timeline. Just in case the reference timeline currently intersects it can reel in that new sub calendar event
                retrievedData=retrievedData.OrderBy(obj => obj.Start);
                List<SubCalendarEvent> allData = retrievedData.ToList();
                DateTime retValueDateTime = ReferenceTime;
                IEnumerable<SubCalendarEvent> retValueIenumerable = CollectionOfSubCalEvent;
                if (allData.Count > 0)
                {
                    retValueDateTime = allData[0].Start;
                    return getStartTimeWhenCurrentTimeClashesWithSubcalevent(retValueIenumerable, retValueDateTime);
                }
                return new Tuple<IEnumerable<SubCalendarEvent>, DateTime>(retValueIenumerable, retValueDateTime);
            }
             * //*/
        }




        Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors> getAllInterferringEventsAndTimeLineInCurrentEvaluation(CalendarEvent initializingCalendarEvent, List<CalendarEvent> NoneCommitedCalendarEventsEvents,int FlagType, HashSet<SubCalendarEvent> NotDoneYet)
        {
            
            DateTime EarliestStartTime;
            DateTime LatestEndTime;
            List<SubCalendarEvent> collectionOfInterferringSubCalEvents;
            Tuple<IEnumerable<SubCalendarEvent>, DateTime, int> refinedStartTimeAndInterferringEvents;
            List<SubCalendarEvent> SubEventsholder;
            TimeLine RangeForScheduleUpdate = initializingCalendarEvent.RangeTimeLine;
            IEnumerable<SubCalendarEvent> PertinentNotDoneYet = NotDoneYet.Where(obj => obj.getCalendarEventRange.InterferringTimeLine(RangeForScheduleUpdate) != null);


            LatestEndTime = PertinentNotDoneYet != null ? (PertinentNotDoneYet.Count() > 0 ? PertinentNotDoneYet.Select(obj => obj.getCalendarEventRange.End).Max() > RangeForScheduleUpdate.End ? PertinentNotDoneYet.Select(obj => obj.getCalendarEventRange.End).Max() : RangeForScheduleUpdate.End : RangeForScheduleUpdate.End) : RangeForScheduleUpdate.End;

            LatestEndTime=LatestEndTime.AddDays(6);

            RangeForScheduleUpdate = new TimeLine(RangeForScheduleUpdate.Start, LatestEndTime);//updates the range for scheduling

            
            List<SubCalendarEvent> ArrayOfInterferringSubEvents = getInterferringSubEvents(RangeForScheduleUpdate, NoneCommitedCalendarEventsEvents).ToList();//It gets all the subevents within the time frame
            SubEventsholder = ArrayOfInterferringSubEvents.ToList();//holder List object for ArrayOfInterferringSubEvents
            SubEventsholder.AddRange(PertinentNotDoneYet.ToList());//Pins the Not done yet elements

            ArrayOfInterferringSubEvents=ArrayOfInterferringSubEvents.OrderBy(obj => obj.End).ToList();// sorts the elements by end date
            List<IDefinedRange>[] MyEdgeElements = getEdgeElements(RangeForScheduleUpdate, ArrayOfInterferringSubEvents);//gets the subevents crossing over the timeLine
            EarliestStartTime = MyEdgeElements[0].Count > 0 ? MyEdgeElements[0].OrderBy(obj => obj.Start).ToList()[0].Start : RangeForScheduleUpdate.Start;
            LatestEndTime = MyEdgeElements[1].Count > 0 ? MyEdgeElements[1].OrderBy(obj => obj.End).ToList()[MyEdgeElements[1].Count - 1].End : RangeForScheduleUpdate.End;
            EarliestStartTime = EarliestStartTime < Now ? Now : EarliestStartTime;
            RangeForScheduleUpdate = new TimeLine(EarliestStartTime, LatestEndTime);//updates the range of schedule

            /*
             * Continue from here Jerome: You need to move all the unnecessary code from rearrange ReArrangeClashingEventsofRigid and  in to this function. The first section will be getting the initializing interferring events. After which you will update the interferring list with the not done yets.
             * These not done yets will get pinned to end of the timeline. Remember we are already moving the excess functionality of the preceeding callers code into this section.
             */


            refinedStartTimeAndInterferringEvents = getStartTimeWhenCurrentTimeClashesWithSubcalevent(ArrayOfInterferringSubEvents, EarliestStartTime, FlagType);//gets the start time relatve to the ArrayOfInterferringSubEvents and flag type
            FlagType = refinedStartTimeAndInterferringEvents.Item3;//updates the flag type
            EarliestStartTime = refinedStartTimeAndInterferringEvents.Item2;//updates the earliest time from preceding function call
            ArrayOfInterferringSubEvents = refinedStartTimeAndInterferringEvents.Item1.ToList();//Updates the ArrayOfInterferringSubEvents just in case the Now element is dropped
            ArrayOfInterferringSubEvents.ToList();
            ArrayOfInterferringSubEvents.AddRange(PertinentNotDoneYet.ToList());//adds the PertinentNotDoneYet to the SubEventsholder list
            
            /*
            TimeLine pinningTimeLine = new TimeLine(ReferenceDayTIime, LatestEndTime);
            Tuple<IEnumerable<SubCalendarEvent>, IEnumerable<SubCalendarEvent>> PinningInformation_NotDoneYet = PintNotDoneYestSubEventToEndOfTimeLine(pinningTimeLine,SubEventsholder);
            pinningTimeLine = null;
            */

            RangeForScheduleUpdate = new TimeLine(EarliestStartTime, LatestEndTime);//updates the RangeForScheduleUpdate timeline
            CustomErrors errorStatus = new CustomErrors(false, "");
            TimeSpan SumOfAllEventsTimeSpan = Utility.SumOfActiveDuration(ArrayOfInterferringSubEvents.ToList());//sum all events

            while (SumOfAllEventsTimeSpan > RangeForScheduleUpdate.TimelineSpan-new TimeSpan(4,0,0))//loops untill the sum all the interferring events can possibly fit within the timeline. Essentially possibly fittable//hack alert to ensure usage of time space. THe extra addition has to be one pertaining to the occupancy
            {
                PertinentNotDoneYet = NotDoneYet.Where(obj => obj.getCalendarEventRange.InterferringTimeLine(RangeForScheduleUpdate) != null);
                ArrayOfInterferringSubEvents.AddRange(PertinentNotDoneYet.ToList());
                EarliestStartTime = ArrayOfInterferringSubEvents.OrderBy(obj => obj.getCalendarEventRange.Start).ToList()[0].getCalendarEventRange.Start;//attempts to get subcalevent with a calendarevent with earliest start time
                LatestEndTime = ArrayOfInterferringSubEvents.OrderBy(obj => obj.getCalendarEventRange.End).ToList()[ArrayOfInterferringSubEvents.Count() - 1].getCalendarEventRange.End;//attempts to get subcalevent with a calendarevent with latest Endtime
                EarliestStartTime = EarliestStartTime < Now ? Now : EarliestStartTime;
                
                
                refinedStartTimeAndInterferringEvents = getStartTimeWhenCurrentTimeClashesWithSubcalevent(ArrayOfInterferringSubEvents, EarliestStartTime, FlagType);
                FlagType = refinedStartTimeAndInterferringEvents.Item3;
                EarliestStartTime = refinedStartTimeAndInterferringEvents.Item2;
                ArrayOfInterferringSubEvents = refinedStartTimeAndInterferringEvents.Item1.ToList();
                ArrayOfInterferringSubEvents.AddRange(PertinentNotDoneYet.ToList());
                
                RangeForScheduleUpdate = new TimeLine(EarliestStartTime, LatestEndTime);//updates range of scan
                collectionOfInterferringSubCalEvents = getInterferringSubEvents(RangeForScheduleUpdate, NoneCommitedCalendarEventsEvents).ToList();//updates interferring events list


                ArrayOfInterferringSubEvents = collectionOfInterferringSubCalEvents.ToList();
                ArrayOfInterferringSubEvents=ArrayOfInterferringSubEvents.OrderBy(obj => obj.End).ToList();
                MyEdgeElements = getEdgeElements(RangeForScheduleUpdate, ArrayOfInterferringSubEvents);
                EarliestStartTime = MyEdgeElements[0].Count > 0 ? MyEdgeElements[0].OrderBy(obj => obj.Start).ToList()[0].Start : RangeForScheduleUpdate.Start;//if there is crossover with start time RangeForScheduleUpdate select the crossover subcalevent start time
                LatestEndTime = MyEdgeElements[1].Count > 0 ? MyEdgeElements[1].OrderBy(obj => obj.End).ToList()[MyEdgeElements[1].Count - 1].End : RangeForScheduleUpdate.End;
                EarliestStartTime = EarliestStartTime < Now ? Now : EarliestStartTime;

                
                refinedStartTimeAndInterferringEvents = getStartTimeWhenCurrentTimeClashesWithSubcalevent(ArrayOfInterferringSubEvents, EarliestStartTime, FlagType);
                FlagType = refinedStartTimeAndInterferringEvents.Item3;
                EarliestStartTime = refinedStartTimeAndInterferringEvents.Item2;
                
                ArrayOfInterferringSubEvents = refinedStartTimeAndInterferringEvents.Item1.ToList();
                ArrayOfInterferringSubEvents.AddRange(PertinentNotDoneYet.ToList());
                RangeForScheduleUpdate = new TimeLine(EarliestStartTime, LatestEndTime);
                TimeSpan newSumOfAllTimeSpans = Utility.SumOfActiveDuration(ArrayOfInterferringSubEvents);
                if (newSumOfAllTimeSpans == SumOfAllEventsTimeSpan)
                {
                    errorStatus = new CustomErrors(true, "Total sum of events exceeds available time span");
                    break;
                    //throw new Exception("You have events that cannot fit our time frame");
                }
                else
                {
                    SumOfAllEventsTimeSpan = newSumOfAllTimeSpans;
                }
            }

            
            return new Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors>(RangeForScheduleUpdate, ArrayOfInterferringSubEvents, errorStatus);
        }


        Tuple<IEnumerable<SubCalendarEvent>,IEnumerable<SubCalendarEvent>> PintNotDoneYestSubEventToEndOfTimeLine(TimeLine encasingTimeLine, IEnumerable<SubCalendarEvent> NotDoneYetEvents)
        {
            List<SubCalendarEvent> retValueSuccesfull = new List<SubCalendarEvent>();
            List<SubCalendarEvent> retValueFailure = new List<SubCalendarEvent>();

            Tuple<IEnumerable<SubCalendarEvent>, IEnumerable<SubCalendarEvent>> retValue = new Tuple<IEnumerable<SubCalendarEvent>, IEnumerable<SubCalendarEvent>>(retValueSuccesfull, retValueFailure);
            
            foreach (SubCalendarEvent eachSubCalendarEvent in NotDoneYetEvents)
            {
                if (eachSubCalendarEvent.PinToEnd(encasingTimeLine))
                {
                    ((List<SubCalendarEvent>)retValue.Item1).Add(eachSubCalendarEvent);
                }
                else 
                {
                    ((List<SubCalendarEvent>)retValue.Item2).Add(eachSubCalendarEvent);
                }
            }

            return retValue;
        }

        HashSet<SubCalendarEvent> getNoneDoneYetBetweenNowAndReerenceStartTIme()
        {/*
          * function gets the none done events within the current day frame.
          */


            TimeLine TimeLineBetweenNowAndReferenceStartTIme = new TimeLine(ReferenceDayTIime,Now);
            SubCalendarEvent[] NotDoneYet = getInterferringSubEvents(TimeLineBetweenNowAndReferenceStartTIme);
            IEnumerable<SubCalendarEvent> retValue = NotDoneYet.Where(obj => (!obj.Rigid)&&(!obj.RangeTimeLine.IsDateTimeWithin(Now)));
            HashSet<SubCalendarEvent> retValue_HashSet = new HashSet<SubCalendarEvent>();
            foreach (SubCalendarEvent eachSubCalendarEvent in retValue)
            {
                retValue_HashSet.Add(eachSubCalendarEvent);
            }




            return retValue_HashSet;
        }
        KeyValuePair<CalendarEvent, TimeLine> ReArrangeClashingEventsofRigid(CalendarEvent MyCalendarEvent, List<CalendarEvent> NoneCommitedCalendarEventsEvents, int CurrentEventStatus)// this looks at the timeline of the calendar event and then tries to rearrange all subevents within the range to suit final output. Such that there will be sufficient time space for each subevent
        {
            /*
                Name{: Jerome Biotidara
             * this function is responsible for making sure there is some dynamic allotment of time to the subeevents. It takes a calendarevent of a a rigid event. It attempts to rearrange elements around this event. It detects any clashing events and tries to rearrange any non rigid clashing events 
             */
            
            HashSet<SubCalendarEvent> NotDoneYetEvents = getNoneDoneYetBetweenNowAndReerenceStartTIme();

            //AllEventDictionary.Add(MyCalendarEvent.ID, MyCalendarEvent);
            NoneCommitedCalendarEventsEvents.Add(MyCalendarEvent);
            /*SubCalendarEvent[] ArrayOfInterferringSubEvents = getInterferringSubEvents(MyCalendarEvent, NoneCommitedCalendarEventsEvents);//It gets all the subevents within the time frame

            
            
            if (ArrayOfInterferringSubEvents.Length > 0)
            {



            }
            else
            {
                NoneCommitedCalendarEventsEvents.Remove(MyCalendarEvent);//removes my cal event
                return new KeyValuePair<CalendarEvent, TimeLine>(null, null);
            }
            */

            Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors> allInterferringSubCalEventsAndTimeLine = getAllInterferringEventsAndTimeLineInCurrentEvaluation(MyCalendarEvent, NoneCommitedCalendarEventsEvents,CurrentEventStatus,NotDoneYetEvents);
            SubCalendarEvent[] ArrayOfInterferringSubEvents = allInterferringSubCalEventsAndTimeLine.Item2.ToArray();
            TimeLine RangeForScheduleUpdate = allInterferringSubCalEventsAndTimeLine.Item1;
           


            TimeSpan SumOfAllEventsTimeSpan = Utility.SumOfActiveDuration(ArrayOfInterferringSubEvents);


            int i = 0;

            Dictionary<CalendarEvent, List<SubCalendarEvent>> DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents = new Dictionary<CalendarEvent, List<SubCalendarEvent>>();
            List<SubCalendarEvent> RigidSubCalendarEvents = new List<SubCalendarEvent>(0);

            RigidSubCalendarEvents = ArrayOfInterferringSubEvents.Where(obj => obj.Rigid).ToList();

            List<BusyTimeLine> RigidSubCalendarEventsBusyTimeLine = new List<BusyTimeLine>(0);
            RigidSubCalendarEventsBusyTimeLine = RigidSubCalendarEvents.Select(obj => obj.ActiveSlot).ToList();
            i = 0;
            double OccupancyOfTimeLineSPan = (double)SumOfAllEventsTimeSpan.Ticks / (double)RangeForScheduleUpdate.TimelineSpan.Ticks;

            ArrayOfInterferringSubEvents = Utility.NotInList(ArrayOfInterferringSubEvents.ToList(), RigidSubCalendarEvents).ToArray();//removes rigid elements

            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents = generateDictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents(ArrayOfInterferringSubEvents.ToList(), NoneCommitedCalendarEventsEvents);


            List<CalendarEvent> SortedInterFerringCalendarEvents_Deadline = DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Keys.ToList();
            SortedInterFerringCalendarEvents_Deadline = SortedInterFerringCalendarEvents_Deadline.OrderBy(obj => obj.End).ToList();







            TimeLine ReferenceTimeLine = RangeForScheduleUpdate.CreateCopy();
            ReferenceTimeLine.AddBusySlots(RigidSubCalendarEventsBusyTimeLine.ToArray());//Adds all the rigid elements

            TimeLine[] ArrayOfFreeSpots = getOnlyPertinentTimeFrame(getAllFreeSpots_NoCompleteSchedule(ReferenceTimeLine), ReferenceTimeLine).ToArray();
            ArrayOfFreeSpots = getOnlyPertinentTimeFrame(ArrayOfFreeSpots, ReferenceTimeLine).ToArray();

            Dictionary<TimeLine, List<CalendarEvent>> DictTimeLineAndListOfCalendarevent = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<CalendarEvent>>();


            if (ArrayOfInterferringSubEvents.Length == 0)
            {
                NoneCommitedCalendarEventsEvents.Remove(MyCalendarEvent);//removes my cal event
                return new KeyValuePair<CalendarEvent, TimeLine>(null, null);
            }


            List<List<List<SubCalendarEvent>>> SnugListOfPossibleSubCalendarEventsClumps = BuildAllPossibleSnugLists(SortedInterFerringCalendarEvents_Deadline, MyCalendarEvent, DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents, ReferenceTimeLine, OccupancyOfTimeLineSPan);
            //Remember Jerome, I need to implement a functionality that permutates through the various options of pin to start option. So take for example two different event timeline that are pertinent to a free spot however one has a dead line preceeding the other, there will be a pin to start for two scenarios, one for each calendar event in which either of them gets pinned first.



            if (!MyCalendarEvent.ErrorStatus && allInterferringSubCalEventsAndTimeLine.Item3.Status)
            {
                MyCalendarEvent.UpdateError(allInterferringSubCalEventsAndTimeLine.Item3);
            }


            NoneCommitedCalendarEventsEvents.Remove(MyCalendarEvent);

            return EvaluateEachSnugPossibiliyOfSnugPossibility(SnugListOfPossibleSubCalendarEventsClumps, ReferenceTimeLine, MyCalendarEvent);
        }




        KeyValuePair<CalendarEvent, TimeLine> ReArrangeTimeLineWithinWithinCalendaEventRange(CalendarEvent MyCalendarEvent, List<CalendarEvent> NoneCommitedCalendarEventsEvents,int InterferringWithNowFlag, HashSet<SubCalendarEvent> NotDoneYet)// this looks at the timeline of the calendar event and then tries to rearrange all subevents within the range to suit final output. Such that there will be sufficient time space for each subevent
        {
            /*
                Name{: Jerome Biotidara
             * this function is responsible for making sure there is some dynamic allotment of time to the subeevents. It takes a calendarevent, checks the alloted time frame and tries to move subevents within the time frame to satisfy the final goal.
             */
            int i = 0;

            if (MyCalendarEvent.RepetitionStatus != false)//Artificially generates random subevents for the calendar event
            {
                throw new Exception("invalid calendar event detected in ReArrangeTimeLineWithinWithinCalendaEventRange. Repeat not allowed");
            }

            /*
            TimeLine RangeForScheduleUpdate;
            DateTime EarliestStartTime;
            DateTime LatestEndTime;
            Tuple<IEnumerable<SubCalendarEvent>, DateTime,int> refinedStartTimeAndInterferringEvents;
            NoneCommitedCalendarEventsEvents.Add(MyCalendarEvent);
            SubCalendarEvent[] ArrayOfInterferringSubEvents = getInterferringSubEvents(MyCalendarEvent, NoneCommitedCalendarEventsEvents);//It gets all the subevents within the time frame

            List<SubCalendarEvent> collectionOfInterferringSubCalEvents;
            EarliestStartTime = ArrayOfInterferringSubEvents.OrderBy(obj => obj.getCalendarEventRange.Start).ToList()[0].getCalendarEventRange.Start;
            LatestEndTime = ArrayOfInterferringSubEvents.OrderBy(obj => obj.getCalendarEventRange.End).ToList()[ArrayOfInterferringSubEvents.Length - 1].getCalendarEventRange.End;
            EarliestStartTime = EarliestStartTime < Now ? Now : EarliestStartTime;
            refinedStartTimeAndInterferringEvents = getStartTimeWhenCurrentTimeClashesWithSubcalevent(ArrayOfInterferringSubEvents, EarliestStartTime, InterferringWithNowFlag);
            InterferringWithNowFlag = refinedStartTimeAndInterferringEvents.Item3;
            EarliestStartTime = refinedStartTimeAndInterferringEvents.Item2;
            ArrayOfInterferringSubEvents = refinedStartTimeAndInterferringEvents.Item1.ToArray();


            RangeForScheduleUpdate = new TimeLine(EarliestStartTime, LatestEndTime);

            collectionOfInterferringSubCalEvents = getInterferringSubEvents(RangeForScheduleUpdate, NoneCommitedCalendarEventsEvents).ToList();
            //collectionOfInterferringSubCalEvents.Add(MyCalendarEvent.AllEvents[0]);//artificially adds the new rigid event
            ArrayOfInterferringSubEvents = collectionOfInterferringSubCalEvents.ToArray();
            */

            NoneCommitedCalendarEventsEvents.Add(MyCalendarEvent);
            Tuple<TimeLine, IEnumerable<SubCalendarEvent>, CustomErrors> allInterferringSubCalEventsAndTimeLine = getAllInterferringEventsAndTimeLineInCurrentEvaluation(MyCalendarEvent, NoneCommitedCalendarEventsEvents, InterferringWithNowFlag, NotDoneYet);
            List<SubCalendarEvent> collectionOfInterferringSubCalEvents = allInterferringSubCalEventsAndTimeLine.Item2.ToList();

            IEnumerable<SubCalendarEvent> ArrayOfInterferringSubEvents = allInterferringSubCalEventsAndTimeLine.Item2;
            TimeLine RangeForScheduleUpdate = allInterferringSubCalEventsAndTimeLine.Item1;

            TimeSpan SumOfAllEventsTimeSpan = Utility.SumOfActiveDuration(ArrayOfInterferringSubEvents);

            Dictionary<CalendarEvent, List<SubCalendarEvent>> DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents = new Dictionary<CalendarEvent, List<SubCalendarEvent>>();
            List<SubCalendarEvent> RigidSubCalendarEvents = new List<SubCalendarEvent>(0);
            List<BusyTimeLine> RigidSubCalendarEventsBusyTimeLine = new List<BusyTimeLine>(0);


            RigidSubCalendarEvents = ArrayOfInterferringSubEvents.Where(obj => obj.Rigid).ToList();
            RigidSubCalendarEventsBusyTimeLine = RigidSubCalendarEvents.Select(obj => obj.ActiveSlot).ToList();

            double OccupancyOfTimeLineSPan = (double)SumOfAllEventsTimeSpan.Ticks / (double)RangeForScheduleUpdate.TimelineSpan.Ticks;

            ArrayOfInterferringSubEvents = Utility.NotInList(ArrayOfInterferringSubEvents.ToList(), RigidSubCalendarEvents).ToArray();//remove rigid elements


            //List<CalendarEvent>[]SubEventsTimeCategories= CategorizeSubEventsTimeLine
            /*
             * SubEventsTimeCategories has 4 list of containing lists.
             * 1st is a List with Elements Starting before The Mycalendaervent timeline and ends after the busytimeline
             * 2nd is a list with elements starting before the mycalendarvent timeline but ending before the myevent timeline
             * 3rd is a list with elements starting after the Mycalendar event start time but ending after the Myevent timeline
             * 4th is a list with elements starting after the MyCalendar event start time and ends before the Myevent timeline 
             * */
            DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents = generateDictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents(ArrayOfInterferringSubEvents.ToList(), NoneCommitedCalendarEventsEvents);//generates a dictionary of a Calendar Event and the interferring events in the respective Calendar event
            //DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Add(MyCalendarEvent, MyCalendarEvent.AllEvents.ToList());//artificially adds enew calendar event to dictionary


            List<CalendarEvent> SortedInterFerringCalendarEvents_Deadline = DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents.Keys.ToList();
            SortedInterFerringCalendarEvents_Deadline = SortedInterFerringCalendarEvents_Deadline.OrderBy(obj => obj.End).ToList();

            TimeLine ReferenceTimeLine = RangeForScheduleUpdate.CreateCopy();


            ReferenceTimeLine.AddBusySlots(RigidSubCalendarEventsBusyTimeLine.ToArray());//Adds all the rigid elements

            TimeLine[] ArrayOfFreeSpots = getOnlyPertinentTimeFrame(getAllFreeSpots_NoCompleteSchedule(ReferenceTimeLine), ReferenceTimeLine).ToArray();
            ArrayOfFreeSpots = getOnlyPertinentTimeFrame(ArrayOfFreeSpots, ReferenceTimeLine).ToArray();

            Dictionary<TimeLine, List<CalendarEvent>> DictTimeLineAndListOfCalendarevent = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<CalendarEvent>>();
            List<List<List<SubCalendarEvent>>> SnugListOfPossibleSubCalendarEventsClumps = BuildAllPossibleSnugLists(SortedInterFerringCalendarEvents_Deadline, MyCalendarEvent, DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents, ReferenceTimeLine, OccupancyOfTimeLineSPan);
            //Remember Jerome, I need to implement a functionality that permutates through the various options of pin to start option. So take for example two different event timeline that are pertinent to a free spot however one has a dead line preceeding the other, there will be a pin to start for two scenarios, one for each calendar event in which either of them gets pinned first.

            List<SubCalendarEvent> SerializedResult = SnugListOfPossibleSubCalendarEventsClumps[0].SelectMany(obj => obj).ToList();
            IEnumerable<SubCalendarEvent> InputSubEvents_Cpy=collectionOfInterferringSubCalEvents;

            int TotalUpdatedSchedule = SerializedResult.Count + RigidSubCalendarEvents.Count;

            if (TotalUpdatedSchedule != collectionOfInterferringSubCalEvents.Count)
            {
                
                
                MyCalendarEvent.UpdateError(new CustomErrors(true, "There is a clash in event"));
            }

            if (!MyCalendarEvent.ErrorStatus && allInterferringSubCalEventsAndTimeLine.Item3.Status)
            {
                MyCalendarEvent.UpdateError(allInterferringSubCalEventsAndTimeLine.Item3);
            }

            NoneCommitedCalendarEventsEvents.Remove(MyCalendarEvent);

            return EvaluateEachSnugPossibiliyOfSnugPossibility(SnugListOfPossibleSubCalendarEventsClumps, ReferenceTimeLine, MyCalendarEvent);
            ;//this will not be the final output. I'll need some class that stores the current output of both rearrange busytimelines and deleted timelines
        }

        KeyValuePair<CalendarEvent, TimeLine> EvaluateEachSnugPossibiliyOfSnugPossibility(List<List<List<SubCalendarEvent>>> SnugPossibilityPermutation, TimeLine ReferenceTimeLine, CalendarEvent ReferenceCalendarEvent)
        {
            TimeLine CopyOfReferenceTimeLine;
            List<TimeLine> SnugPossibilityTimeLine = new System.Collections.Generic.List<TimeLine>();
            Dictionary<BusyTimeLine, SubCalendarEvent> MyBusyTimeLineToSubCalendarEventDict = new System.Collections.Generic.Dictionary<BusyTimeLine, SubCalendarEvent>();

            Dictionary<CalendarEvent, TimeLine> CalendarEvent_EvaluationIndexDict = new System.Collections.Generic.Dictionary<CalendarEvent, TimeLine>();

            foreach (List<List<SubCalendarEvent>> SnugPermutation in SnugPossibilityPermutation)//goes each permutation of snug possibility generated
            {
                List<SubCalendarEvent> AllSubEvents = new System.Collections.Generic.List<SubCalendarEvent>();

                foreach (List<SubCalendarEvent> eachList in SnugPermutation)
                {
                    AllSubEvents.AddRange(eachList);
                    foreach (SubCalendarEvent eachSubCalendarEvent in eachList)
                    {
                        ReferenceCalendarEvent.updateSubEvent(eachSubCalendarEvent.SubEvent_ID, eachSubCalendarEvent);
                        /*if (SubEvent != null)
                        {
                            SubEvent.updateSubEvent(SubEvent.SubEvent_ID, eachSubCalendarEvent);
                        }*/
                    }

                }

                ReferenceTimeLine = Utility.AddSubCaleventsToTimeLine(ReferenceTimeLine, AllSubEvents);
                CalendarEvent_EvaluationIndexDict.Add(ReferenceCalendarEvent, ReferenceTimeLine);

                /*
                CopyOfReferenceTimeLine = ReferenceTimeLine.CreateCopy();
                //SnugPossibilityTimeLine.Add(CopyOfReferenceTimeLine);
                List<TimeLine> ListOfFreeSpots=getOnlyPertinentTimeFrame(getAllFreeSpots_NoCompleteSchedule(CopyOfReferenceTimeLine), CopyOfReferenceTimeLine);
                List<SubCalendarEvent> ReassignedSubEvents = new System.Collections.Generic.List<SubCalendarEvent>();
                for (int i=0; i<ListOfFreeSpots.Count;i++)
                {
                    DateTime RelativeStartTime = ListOfFreeSpots[i].Start;

                    TimeLine UpdatedTimeLine=Utility.AddSubCaleventsToTimeLine(ListOfFreeSpots[i], SnugPermutation[i]);
                    ListOfFreeSpots[i].AddBusySlots(UpdatedTimeLine.OccupiedSlots);
                    
                    foreach (SubCalendarEvent MySubCalendarEvent in SnugPermutation[i])
                    {//tries to reassign each element in a snug permutation into the referencetimeLine
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
                SnugPossibilityTimeLine.Add(CopyOfReferenceTimeLine);*/
            }

            Dictionary<string, double> DictionaryGraph = new System.Collections.Generic.Dictionary<string, double>();

            /*
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

                
            }*/

            double HighestValue = 0;

            KeyValuePair<CalendarEvent, TimeLine> FinalSuggestion = new System.Collections.Generic.KeyValuePair<CalendarEvent, TimeLine>(CalendarEvent_EvaluationIndexDict.Keys.ToList()[0], CalendarEvent_EvaluationIndexDict.Values.ToList()[0]);

            /*TimeLine TimeLineUpdated = null;
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
            */
            return FinalSuggestion;
        }

        




        string BuildStringIndexForMatch(BusyTimeLine PrecedingTimeLineEvent, BusyTimeLine NextTimeLineEvent)
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
            int i, j = 0;
            double CurrentSumOfLocationData = 0;
            for (i = 0; i < (ListOfBusySlots.Length - 1); i++)
            {
                j = i + 1;
                string generatedIndexMatch = BuildStringIndexForMatch(ListOfBusySlots[i], ListOfBusySlots[j]);

                CurrentSumOfLocationData += CurrentDictionary[generatedIndexMatch];
            }

            return CurrentSumOfLocationData;
        }

        Dictionary<string, double> BuildDictionaryDistanceEdge(TimeLine ReferenceTimeline, CalendarEvent ReferenceCalendarEvent, Dictionary<string, double> CurrentDictionary)
        {
            BusyTimeLine[] ListOfBusySlots = ReferenceTimeline.OccupiedSlots;
            int i = 0;
            //Dictionary<string, double> CurrentDictionaryFrom
            int j = 0;
            for (i = 0; i < (ListOfBusySlots.Length - 1); i++)
            {
                j = i + 1;
                string generatedIndexMatch = BuildStringIndexForMatch(ListOfBusySlots[i], ListOfBusySlots[j]);
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

        List<List<List<SubCalendarEvent>>> BuildAllPossibleSnugLists(List<CalendarEvent> SortedInterferringCalendarEvents, CalendarEvent ToBeFittedTimeLine, Dictionary<CalendarEvent, List<SubCalendarEvent>> DictionaryWithBothCalendarEventsAndListOfInterferringSubEvents, TimeLine ReferenceTimeLine, double Occupancy)
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
            List<SubCalendarEvent>[] MyListOfSubCalendarEvents = DictionaryWithBothCalendarEventsAndListOfInterferringSubEvents.Values.ToArray();
            //TimeLine[] FreeSpotsWithOnlyRigids= ToBeFittedTimeLine.EventTimeLine.getAllFreeSlots();
            TimeLine[] FreeSpotsWithOnlyRigids = getAllFreeSpots_NoCompleteSchedule(ReferenceTimeLine);
            List<SubCalendarEvent> ListOfAllInterferringSubCalendarEvents = new List<SubCalendarEvent>();
            List<TimeSpan> ListOfAllInterferringTimeSpans = new List<TimeSpan>();

            foreach (List<SubCalendarEvent> MyList in MyListOfSubCalendarEvents)//Loop creates a List of interferring SubCalendarEvens
            {
                foreach (SubCalendarEvent MySubEvents in MyList)
                {
                    ListOfAllInterferringSubCalendarEvents.Add(MySubEvents);
                    ListOfAllInterferringTimeSpans.Add(MySubEvents.ActiveSlot.BusyTimeSpan);
                }
            }


            List<SubCalendarEvent> ListOfAlreadyAssignedSubCalendarEvents = new System.Collections.Generic.List<SubCalendarEvent>();

            /*foreach (BusyTimeLine MyBusySlot in ReferenceTimeLine.OccupiedSlots)
            {
                SubCalendarEvent MySubCalendarEvent = getSubCalendarEvent(MyBusySlot.TimeLineID);
                if (MySubCalendarEvent != null)
                {
                    ListOfAlreadyAssignedSubCalendarEvents.Add(MySubCalendarEvent);
                }
            }

            ListOfAllInterferringSubCalendarEvents = Utility.NotInList(ListOfAllInterferringSubCalendarEvents, ListOfAlreadyAssignedSubCalendarEvents);*/

            Dictionary<TimeLine, List<CalendarEvent>> DictionaryOfFreeTimeLineAndPertinentCalendarEventList = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<CalendarEvent>>();

            Dictionary<TimeLine, Dictionary<CalendarEvent, List<SubCalendarEvent>>> DictionaryOfFreeTimeLineAndDictionaryOfCalendarEventAndListOfSubCalendarEvent = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.Dictionary<CalendarEvent, System.Collections.Generic.List<SubCalendarEvent>>>();



            foreach (TimeLine MyFreeTimeLine in JustFreeSpots)
            {
                CalendarEvent[] MyListOfPertinentCalendarEventsForMyTimeLine = getPertinentCalendarEvents(SortedInterferringCalendarEvents.ToArray(), MyFreeTimeLine);
                Dictionary<CalendarEvent, List<SubCalendarEvent>> MyDictionaryOfCalendarEventAndPertinentSubCalendarEvent = new System.Collections.Generic.Dictionary<CalendarEvent, System.Collections.Generic.List<SubCalendarEvent>>();
                foreach (CalendarEvent MyCalendarEvent in MyListOfPertinentCalendarEventsForMyTimeLine)
                {
                    List<SubCalendarEvent> MyListwe = DictionaryWithBothCalendarEventsAndListOfInterferringSubEvents[MyCalendarEvent];
                    MyDictionaryOfCalendarEventAndPertinentSubCalendarEvent.Add(MyCalendarEvent, MyListwe);
                }
                DictionaryOfFreeTimeLineAndDictionaryOfCalendarEventAndListOfSubCalendarEvent.Add(MyFreeTimeLine, MyDictionaryOfCalendarEventAndPertinentSubCalendarEvent);
                DictionaryOfFreeTimeLineAndPertinentCalendarEventList.Add(MyFreeTimeLine, MyListOfPertinentCalendarEventsForMyTimeLine.ToList());//Next step is to call the snug array. Note: you will need to ensure that when ever a subevent gets used in a free timeline. It will have to be removed from the List so that it cannot be used in another free timeline. Also you need to create every possible permutation. Take for example a calendar event thats pertinent to two different "free timelines". you need to ensure that you have different calls to the snuglist generator that has the calendar event enabled in one and disabled in the other.
            }


            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> EmptyIntialListOfSubCalendarEvemts = new List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();

            for (int i = 0; i < JustFreeSpots.Length; i++)
            {
                EmptyIntialListOfSubCalendarEvemts.Add(new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>());
            }



            Tuple<List<TimeSpanWithStringID>, List<mTuple<bool, SubCalendarEvent>>> Arg14 = ConvertSubCalendarEventToTimeSpanWitStringID(ListOfAllInterferringSubCalendarEvents);

            List<TimeSpanWithStringID> SubCalEventsAsTimeSpanWithStringID = Arg14.Item1;//ListOfAllInterferringSubCalendarEvents as TimeSpanWithStringID
            List<mTuple<bool, SubCalendarEvent>> Arg15 = Arg14.Item2;

            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> Dict_StringTickAndCount = new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> Dict_StringTickAndCount_Cpy = new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();

            foreach (TimeSpanWithStringID eachTimeSpanWithStringID in SubCalEventsAsTimeSpanWithStringID)
            {
                if (Dict_StringTickAndCount.ContainsKey(eachTimeSpanWithStringID.timeSpan))
                {
                    ++Dict_StringTickAndCount[eachTimeSpanWithStringID.timeSpan].Item1;
                    ++Dict_StringTickAndCount_Cpy[eachTimeSpanWithStringID.timeSpan].Item1;
                }
                else
                {
                    Dict_StringTickAndCount.Add(eachTimeSpanWithStringID.timeSpan, new mTuple<int, TimeSpanWithStringID>(1, eachTimeSpanWithStringID));
                    Dict_StringTickAndCount_Cpy.Add(eachTimeSpanWithStringID.timeSpan, new mTuple<int, TimeSpanWithStringID>(1, eachTimeSpanWithStringID));
                }
            }


            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> TotalSum = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();

            TotalSum = SnugArray.CreateCopyOFSnuPossibilities(Dict_StringTickAndCount_Cpy);

            InterferringTimeSpanWithStringID_Cpy = Dict_StringTickAndCount_Cpy;//hack to keep track of available events


            Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> Dict_TimeLine_ListOfSubCalendarEvent = BuildDicitionaryOfTimeLineAndSubcalendarEvents(Arg15, DictionaryOfFreeTimeLineAndDictionaryOfCalendarEventAndListOfSubCalendarEvent, ToBeFittedTimeLine);

            Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> Dict_ConstrainedList = generateConstrainedList(JustFreeSpots.ToList(), Arg15);

            Dictionary<TimeLine, List<mTuple<int, TimeSpanWithStringID>>> Dict_TimeLine_ListOfmTuple = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<mTuple<int, TimeSpanWithStringID>>>();

            Dictionary<TimeLine, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> Dict_TimeLine_Dict_string_mTple = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();

            Dictionary<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents = new Dictionary<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>>();

            Dictionary<TimeLine, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> Dict_TimeLine_Dict_string_mTple_Constrained = new Dictionary<TimeLine, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();

            foreach (TimeLine eachTimeLine in Dict_TimeLine_ListOfSubCalendarEvent.Keys)
            {
                List<mTuple<bool, SubCalendarEvent>> LisOfSubCalEvent = Dict_TimeLine_ListOfSubCalendarEvent[eachTimeLine];
                Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> myDict = new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
                Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> myDict0 = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();


                foreach (mTuple<bool, SubCalendarEvent> eachmTuple in LisOfSubCalEvent)//goes Through each Subcalevent in Each timeline and generates a dict for a TimeTick To List of TimeSpanID
                {
                    if (myDict.ContainsKey(eachmTuple.Item2.ActiveDuration))
                    {
                        ++myDict[eachmTuple.Item2.ActiveDuration].Item1;
                    }
                    else
                    {
                        myDict.Add(eachmTuple.Item2.ActiveDuration, new mTuple<int, TimeSpanWithStringID>(1, new TimeSpanWithStringID(eachmTuple.Item2.ActiveDuration, eachmTuple.Item2.ActiveDuration.Ticks.ToString())));

                    }



                    if (myDict0.ContainsKey(eachmTuple.Item2.ActiveDuration))
                    {


                        myDict0[eachmTuple.Item2.ActiveDuration].Add(eachmTuple.Item2.ID, eachmTuple);
                    }
                    else
                    {
                        Dictionary<string, mTuple<bool, SubCalendarEvent>> var17 = new System.Collections.Generic.Dictionary<string, mTuple<bool, SubCalendarEvent>>();
                        var17.Add(eachmTuple.Item2.ID, eachmTuple);
                        myDict0.Add(eachmTuple.Item2.ActiveDuration, var17);
                    }

                }
                Dict_TimeLine_Dict_string_mTple.Add(eachTimeLine, myDict);
                Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents.Add(eachTimeLine, myDict0);
            }


            foreach (TimeLine eachTimeLine in Dict_ConstrainedList.Keys)
            {
                List<mTuple<bool, SubCalendarEvent>> LisOfSubCalEvent = Dict_ConstrainedList[eachTimeLine];
                Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> myDict = new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
                foreach (mTuple<bool, SubCalendarEvent> eachmTuple in LisOfSubCalEvent)//goes Through each Subcalevent in Each timeline and generates a dict for a TimeTick To List of TimeSpanID
                {
                    if (myDict.ContainsKey(eachmTuple.Item2.ActiveDuration))
                    {
                        ++myDict[eachmTuple.Item2.ActiveDuration].Item1;
                    }
                    else
                    {
                        myDict.Add(eachmTuple.Item2.ActiveDuration, new mTuple<int, TimeSpanWithStringID>(1, new TimeSpanWithStringID(eachmTuple.Item2.ActiveDuration, eachmTuple.Item2.ActiveDuration.Ticks.ToString())));
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
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> var10 = new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>();
            List<KeyValuePair<TimeLine, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> var11 = Dict_TimeLine_Dict_string_mTple_Constrained.ToList(); //Same as Dict_TimeLine_Dict_string_mTple_Constrained only as List of KeyValuePair
            Dictionary<TimeLine, Dictionary<string, mTuple<int, TimeSpanWithStringID>>> var14 = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>();
            Dictionary<TimeLine, Tuple<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> var15 = new Dictionary<TimeLine, Tuple<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();

            foreach (KeyValuePair<TimeLine, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> eachKeyValuePair in Dict_TimeLine_Dict_string_mTple_Constrained)
            {
                i0 = var7.IndexOf(eachKeyValuePair.Key);
                Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> var8 = eachKeyValuePair.Value;
                Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> var9 = new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
                for (; i0 < var7.Count; i0++)
                {
                    Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> var12 = var11[i0].Value;
                    foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> var13 in var12)
                    {
                        if (var9.ContainsKey(var13.Key))
                        {
                            var9[var13.Key].Item1 += var13.Value.Item1;
                        }
                        else
                        {
                            var9.Add(var13.Key, new mTuple<int, TimeSpanWithStringID>(var13.Value.Item1, var13.Value.Item2));
                        }
                    }
                }
                var15.Add(eachKeyValuePair.Key, new Tuple<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>(eachKeyValuePair.Value, var9));
            }



            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> AllTImeLinesWithSnugPossibilities = generateTreeCallsToSnugArray(Dict_StringTickAndCount, JustFreeSpots.ToList(), 0, EmptyIntialListOfSubCalendarEvemts, Dict_TimeLine_Dict_string_mTple, var15);




            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> validMatches = getValidMatches(ListOfAllInterferringSubCalendarEvents, AllTImeLinesWithSnugPossibilities, Dict_TimeLine_Dict_string_mTple_Constrained);

            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AverageMatched = getAveragedOutTIimeLine(validMatches, 0);



            Dictionary<TimeLine, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AveragedMatchAsDictWithTimeLine = new Dictionary<TimeLine, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();

            i0 = 0;
            for (; i0 < JustFreeSpots.Length; i0++)
            {
                AveragedMatchAsDictWithTimeLine.Add(JustFreeSpots[i0], AverageMatched[i0]);
                //TotalSum[0] = SnugArray.AddToSnugPossibilityList(TotalSum[0], AverageMatched[i0]);
            }



            Dict_ConstrainedList = stitchRestrictedSubCalendarEvent(JustFreeSpots.ToList(), 0, Dict_ConstrainedList);

            Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> Dict_TimeLine_ListOfSubCalendarEvent_Cpy = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>(Dict_TimeLine_ListOfSubCalendarEvent);

            Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> DictWithTimeLine_ArrangedOptimizedSubCalEvents = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>()
;
            i0 = 0;
            List<mTuple<bool, SubCalendarEvent>> TotalArrangedElements = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            List<List<SubCalendarEvent>> TotalArrangedElements_NoMTuple = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
            List<TimeLine> ListOfTimeLines = JustFreeSpots.ToList();//This will be decremented so do not reused

            Dictionary<string, SubCalendarEvent> TestDict = new System.Collections.Generic.Dictionary<string, SubCalendarEvent>();

            List<List<SubCalendarEvent>> restrictedSubCaleventsAfterScheduleUpdate = new List<List<SubCalendarEvent>>();
            Dictionary<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> copyOfPossibleEvents = createCopyOfPossibleEvents(Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents);



            foreach (KeyValuePair<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> eachKeyValuePair in Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents)
            {
                List<mTuple<bool, SubCalendarEvent>> var16 = Dict_ConstrainedList[eachKeyValuePair.Key];
                List<BusyTimeLine> RestrictedBusySlots = new System.Collections.Generic.List<BusyTimeLine>();
                foreach (mTuple<bool, SubCalendarEvent> eachmTuple in var16)
                {
                    eachmTuple.Item1 = true;
                    RestrictedBusySlots.Add(eachmTuple.Item2.ActiveSlot);
                    TimeSpan ActiveTimespan1 = eachmTuple.Item2.ActiveDuration;
                    string SubEventID = eachmTuple.Item2.ID;
                    eachKeyValuePair.Value[ActiveTimespan1][SubEventID] = eachmTuple;
                }
                eachKeyValuePair.Key.AddBusySlots(RestrictedBusySlots.ToArray());
                if (i0 == 9 || i0 == 12)
                {
                    ;
                }


                stageOfProgram = "stitchUnRestrictedSubCalendarEvent index io is" + i0;
                List<mTuple<bool, SubCalendarEvent>> ArrangedElements = stitchUnRestrictedSubCalendarEvent(eachKeyValuePair.Key, var16, Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents[eachKeyValuePair.Key], AverageMatched[i0], Occupancy);
                foreach (TimeLine eachTimeLine in JustFreeSpots)
                {
                    foreach (mTuple<bool, SubCalendarEvent> eachmTuple in ArrangedElements)
                    {
                        TimeSpan ActiveTimeSpan0 = eachmTuple.Item2.ActiveDuration;
                        if (Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents[eachTimeLine].ContainsKey(ActiveTimeSpan0))
                        {
                            Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents[eachTimeLine][ActiveTimeSpan0].Remove(eachmTuple.Item2.ID);
                        }
                    }
                }
                /*
                if (i0 + 1 < Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents.Count)
                {
                    Dictionary<TimeLine, Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> test = Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents;
                    ArrangedElements = FurtherFillTimeLineWithSubCalEvents(ArrangedElements, JustFreeSpots[i0 + 1], AveragedMatchAsDictWithTimeLine, eachKeyValuePair.Key, test);
                    foreach (KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair0 in AveragedMatchAsDictWithTimeLine[eachKeyValuePair.Key])
                    {
                        if (AveragedMatchAsDictWithTimeLine[JustFreeSpots[i0 + 1]].ContainsKey(eachKeyValuePair0.Key))
                        {
                            AveragedMatchAsDictWithTimeLine[JustFreeSpots[i0 + 1]][eachKeyValuePair0.Key].Item1 += eachKeyValuePair0.Value.Item1;
                        }
                        else
                        {
                            AveragedMatchAsDictWithTimeLine[JustFreeSpots[i0 + 1]].Add(eachKeyValuePair0.Key, new mTuple<int, TimeSpanWithStringID>(eachKeyValuePair0.Value));
                        }
                    }

                }*/
                foreach (TimeLine eachTimeLine in JustFreeSpots)
                {
                    foreach (mTuple<bool, SubCalendarEvent> eachmTuple in ArrangedElements)
                    {


                        TimeSpan ActiveTimeSpan2 = eachmTuple.Item2.ActiveDuration;
                        if (Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents[eachTimeLine].ContainsKey(ActiveTimeSpan2))
                        {
                            Dict_TimeLine_Dict_StringID_Dict_SubEventStringID_mTuple_Bool_MatchinfSubCalevents[eachTimeLine][ActiveTimeSpan2].Remove(eachmTuple.Item2.ID);
                        }
                    }
                }



                TotalArrangedElements_NoMTuple.Add(Utility.mTupleToSubCalEvents(ArrangedElements));
                List<mTuple<bool, SubCalendarEvent>> previous = TotalArrangedElements.ToList();
                int BeforeErrorIndex = TotalArrangedElements.Count;
                TotalArrangedElements.AddRange(ArrangedElements);
                int AfterErrorIndex = TotalArrangedElements.Count;
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

                        foreach (mTuple<bool, SubCalendarEvent> eachmTuple in ArrangedElements)
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

               



                List<SubCalendarEvent> UnassignedSubevents = ListOfAllInterferringSubCalendarEvents.ToList();
                List<SubCalendarEvent> AlreadyAssigned = Utility.mTupleToSubCalEvents(TotalArrangedElements);


                foreach (SubCalendarEvent eachSubCalendarEvent in AlreadyAssigned)
                {
                    if (eachSubCalendarEvent.ID == "470_482")
                    {
                        ;
                    }
                    TestDict.Add(eachSubCalendarEvent.ID, eachSubCalendarEvent);//if it crashes at this point there is some possible error in your constrained list generating duplicate values. 
                }
                TestDict = new System.Collections.Generic.Dictionary<string, SubCalendarEvent>();
                /*
                UnassignedSubevents=Utility.NotInList(UnassignedSubevents, AlreadyAssigned);
                */
                List<string> MyAssignedIDs = AlreadyAssigned.Select(obj => obj.ID).ToList();

                UnassignedSubevents.RemoveAll(e => MyAssignedIDs.Contains(e.ID));
                int StartIndex = 0;
                if (Dict_ConstrainedList.Count > 1)
                {
                    restrictedSubCaleventsAfterScheduleUpdate.Add(Utility.mTupleToSubCalEvents(Dict_ConstrainedList[ListOfTimeLines[0]]));
                    StartIndex = i0 + 1;
                    ListOfTimeLines.RemoveAt(0);

                    Dict_ConstrainedList = generateConstrainedList(ListOfTimeLines, Utility.SubCalEventsTomTuple(UnassignedSubevents, false));

                    TimeLine PertinentTimeLine = Dict_TimeLine_ListOfSubCalendarEvent_Cpy.Keys.ToList()[0];
                    List<mTuple<bool, SubCalendarEvent>> beforeList = Dict_ConstrainedList[PertinentTimeLine];
                    int before = Dict_ConstrainedList[PertinentTimeLine].Count;

                    Dict_ConstrainedList[PertinentTimeLine] = stitchRestrictedSubCalendarEvent(Dict_ConstrainedList[PertinentTimeLine], PertinentTimeLine);
                    int after = Dict_ConstrainedList[PertinentTimeLine].Count;
                    List<mTuple<bool, SubCalendarEvent>> AfterList = Dict_ConstrainedList[PertinentTimeLine];
                    if (after != before)
                    {
                        ;
                    }
                    //Dict_ConstrainedList = stitchRestrictedSubCalendarEvent(Dict_TimeLine_ListOfSubCalendarEvent_Cpy.Keys.ToList(), 0, Dict_ConstrainedList);
                }
                i0++;
            }


            TimeSpan TotalBusyTimeFrame = Utility.SumOfActiveDuration(ListOfAllInterferringSubCalendarEvents);
            IEnumerable<SubCalendarEvent> JustAssignedSubCal = TotalArrangedElements.Select(obj => obj.Item2);
            IEnumerable<string> JustAssignedSubCalIDs = TotalArrangedElements.Select(obj => obj.Item2.ID);
            List<SubCalendarEvent> UnAssignedElements = ListOfAllInterferringSubCalendarEvents.Where(obj => !JustAssignedSubCal.Contains(obj)).ToList();
            ;

            foreach (BusyTimeLine OccupiedSlot in ReferenceTimeLine.OccupiedSlots)
            {
                TotalBusyTimeFrame += OccupiedSlot.BusyTimeSpan;
            }

            TimeSpan TotalTimeSpan = ReferenceTimeLine.TimelineSpan;


            double PercentageOfOccupiedSpace = (double)TotalBusyTimeFrame.Ticks / (double)TotalTimeSpan.Ticks;


            restrictedSubCaleventsAfterScheduleUpdate.Add(Utility.mTupleToSubCalEvents(Dict_ConstrainedList[ListOfTimeLines[0]]));//updates the constrained List one last time

            stageOfProgram = "before beginning spreadout";

#if EnableRestrictedLocationOptimization
            
#else
            TotalArrangedElements_NoMTuple=SpreadOutEvents(TotalArrangedElements_NoMTuple, PercentageOfOccupiedSpace, JustFreeSpots.ToList(), copyOfPossibleEvents, restrictedSubCaleventsAfterScheduleUpdate);
#endif
            //TotalArrangedElements_NoMTuple=OptimizeForLocation(TotalArrangedElements_NoMTuple, JustFreeSpots);
            ReadjustForDailySchedule(TotalArrangedElements_NoMTuple, JustFreeSpots.Select(obj => obj.CreateCopy()).ToList());


            List<List<List<SubCalendarEvent>>> ReValue = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();
            //List<List<List<SubCalendarEvent>>> ReValue = FixSubCalEventOrder(AllTImeLinesWithSnugPossibilities, JustFreeSpots);
            ReValue.Add(TotalArrangedElements_NoMTuple);

            return ReValue;
        }

        Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> InterferringTimeSpanWithStringID_Cpy = new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();



        List<List<SubCalendarEvent>> ReadjustForDailySchedule(List<List<SubCalendarEvent>> SpreadOutSchedule, List<TimeLine> AllFreeSpots)
        {
            List<mTuple<int, TimeLine>> LargerThan24Hours = new List<mTuple<int, TimeLine>>();
            List<List<SubCalendarEvent>> retValue = new List<List<SubCalendarEvent>>();
            TimeSpan TwentyFourTimeSpan = new TimeSpan(1, 0, 0, 0);
            for(int i=0; i<AllFreeSpots.Count;i++)
            {
                if (AllFreeSpots[i].TimelineSpan >= TwentyFourTimeSpan)
                {
                    LargerThan24Hours.Add(new mTuple<int, TimeLine>(i, AllFreeSpots[i]));
                }
            }


            foreach (mTuple<int, TimeLine> eachmTuple in LargerThan24Hours)
            {
                List <SubCalendarEvent> updatedList= resolveInTo24HourSlots(SpreadOutSchedule[eachmTuple.Item1].ToList(), eachmTuple.Item2);
                if (updatedList.Count != SpreadOutSchedule[eachmTuple.Item1].Count)
                {
                    throw new Exception("theres an error with resolveInTo24HourSlots");
                }
                SpreadOutSchedule[eachmTuple.Item1] = updatedList;
            }

            return retValue;
        }

        List<SubCalendarEvent> resolveInTo24HourSlots(List<SubCalendarEvent> currentListOfSubCalendarElements, TimeLine limitingTimeLine, mTuple<SubCalendarEvent,SubCalendarEvent>edgeElements=null)
        {
            //function takes a full freespot and tries to spread it out into 24 hour sections
            //this is done by intially sending every subcalevenet towards the end of the Timeline after which it takes 24hour chunks and attempts to 
            
            




            
            List<SubCalendarEvent> currentListOfSubCalendarElements_cpy = currentListOfSubCalendarElements.ToList();
            Utility.PinSubEventsToStart(currentListOfSubCalendarElements_cpy, limitingTimeLine);
            
            TimeLine limitingTimeLine_cpy = limitingTimeLine.CreateCopy();

            limitingTimeLine.AddBusySlots(currentListOfSubCalendarElements_cpy.Select(obj => obj.ActiveSlot));
            List<TimeLineWithEdgeElements> AllFreeSpots = limitingTimeLine_cpy.getAllFreeSlotsWithEdges().ToList();
            AllFreeSpots=AllFreeSpots.Where(obj => obj.TimelineSpan.Ticks > 0).OrderBy(obj=>obj.End).ToList();



            for (int i = AllFreeSpots.Count() - 1; AllFreeSpots.Count() > 0; )
            {
                TimeLine eachTimeLine = AllFreeSpots[i];
                List<SubCalendarEvent> reassignedElements = TossEndWards(currentListOfSubCalendarElements, eachTimeLine);//tries to toss any subcalendarevent towards the end
                reassignedElements=reassignedElements.OrderBy(obj => obj.End).ToList();
                Utility.PinSubEventsToEnd(reassignedElements, eachTimeLine);
                
                SubCalendarEvent lastElement;// = reassignedElements.Last();

                currentListOfSubCalendarElements.RemoveAll(obj => reassignedElements.Contains(obj));


                if (reassignedElements.Count > 0)
                {
                    lastElement = reassignedElements.First();
                    limitingTimeLine = new TimeLine(limitingTimeLine.Start, lastElement.Start);
                }
                else 
                {
                    currentListOfSubCalendarElements.OrderBy(obj => obj.End);
                    if (currentListOfSubCalendarElements.Count > 0)//hack alert you need to coscious of coliision scenario
                    {
                        lastElement = currentListOfSubCalendarElements.Last();
                        lastElement.PinToEnd(limitingTimeLine);
                        currentListOfSubCalendarElements.Remove(lastElement);
                        limitingTimeLine = new TimeLine(limitingTimeLine.Start, lastElement.Start);
                    }
                    else
                    {
                        break;
                    }
                    
                    
                }

                Utility.PinSubEventsToStart(currentListOfSubCalendarElements, limitingTimeLine);


                

                
                limitingTimeLine.AddBusySlots(currentListOfSubCalendarElements.Select(obj => obj.ActiveSlot));
                AllFreeSpots = limitingTimeLine.getAllFreeSlotsWithEdges().ToList();
                i = AllFreeSpots.Count() - 1;
                
            }

            TimeSpan TotalDuration = Utility.SumOfActiveDuration(currentListOfSubCalendarElements_cpy);
            double Occupancy = (double)TotalDuration.Ticks / (double)limitingTimeLine_cpy.TimelineSpan.Ticks;

            List<SubCalendarEvent> currentListOfSubCalendarElements_cpy_ref = currentListOfSubCalendarElements_cpy.ToList();
            Dictionary<string, mTuple<SubCalendarEvent, BusyTimeLine>> currentListOfSubCalendarElements_cpy_ref_Dict = currentListOfSubCalendarElements_cpy_ref.ToDictionary(obj => obj.ID, obj => new mTuple<SubCalendarEvent, BusyTimeLine>(obj, obj.ActiveSlot.CreateCopy()));
            TimeLine limitingTimeLine_cpy_cpy_ref = limitingTimeLine_cpy.CreateCopy();
            List<SubCalendarEvent> FullyUpdated = new List<SubCalendarEvent>();

            while(true)
            {
                Tuple<List<SubCalendarEvent>, TimeLine> CollectionUpdated = every24Interval(currentListOfSubCalendarElements_cpy_ref, limitingTimeLine_cpy_cpy_ref, Occupancy, currentListOfSubCalendarElements_cpy_ref_Dict);

                TimeSpan currTotalDuration = Utility.SumOfActiveDuration(CollectionUpdated.Item1);
                double currOccupancy =-8898;
                if(CollectionUpdated.Item2.TimelineSpan.Ticks>0)
                { currOccupancy = (double)currTotalDuration.Ticks / (double)CollectionUpdated.Item2.TimelineSpan.Ticks; }

                if (currOccupancy > Occupancy)
                {
                    ;
                }


                FullyUpdated.AddRange(CollectionUpdated.Item1);

                limitingTimeLine_cpy_cpy_ref = new TimeLine(CollectionUpdated.Item2.End, limitingTimeLine_cpy_cpy_ref.End);
                currentListOfSubCalendarElements_cpy_ref.RemoveAll(obj => FullyUpdated.Contains(obj));
                if ((currentListOfSubCalendarElements_cpy_ref.Count < 1) || (limitingTimeLine_cpy_cpy_ref.TimelineSpan.Ticks <= 0))
                {
                    break;
                }
            }


            currentListOfSubCalendarElements_cpy = currentListOfSubCalendarElements_cpy.OrderBy(obj => obj.End).ToList();
            List<SubCalendarEvent> retValue = currentListOfSubCalendarElements_cpy;

            return retValue;
        }


        Tuple<List<SubCalendarEvent>,TimeLine> every24Interval(List<SubCalendarEvent> currentListOfSubCalendarElements, TimeLine limitingTimeLine, double occupancy, Dictionary<string, mTuple<SubCalendarEvent, BusyTimeLine>> PreservedValues)
        {
            DateTime refEndTime=limitingTimeLine.Start.AddDays(1);
            refEndTime=refEndTime<=limitingTimeLine.End?refEndTime:limitingTimeLine.End;
            TimeLine CurrentDayReference = new TimeLine(limitingTimeLine.Start, refEndTime);
            List<SubCalendarEvent> currentOccupiers = new List<SubCalendarEvent>();

            currentOccupiers = currentListOfSubCalendarElements.Where(obj => obj.RangeTimeLine.InterferringTimeLine (CurrentDayReference)!=null).OrderBy(obj => obj.End).ToList();//hack alert be cautious of collision scenarios
            currentListOfSubCalendarElements.RemoveAll(obj=>currentOccupiers.Contains(obj));
            List<SubCalendarEvent> crossing24HourMark = currentOccupiers.Where(obj => obj.RangeTimeLine.IsDateTimeWithin(CurrentDayReference.End)).ToList();
            if (crossing24HourMark.Count > 0)
            {
                crossing24HourMark=crossing24HourMark.OrderBy(obj => obj.End).ToList();
                CurrentDayReference = new TimeLine(CurrentDayReference.Start, crossing24HourMark.Last().End);
            }

            List<SubCalendarEvent> canExistWithinTimeLine = currentListOfSubCalendarElements.Where(obj => obj.canExistWithinTimeLine(limitingTimeLine)).ToList();

            Dictionary<string, Tuple<Location, List<SubCalendarEvent>>> CalEventDictionaryMapping = new Dictionary<string, Tuple<Location, List<SubCalendarEvent>>>();

            foreach (SubCalendarEvent eachmTuple in canExistWithinTimeLine)
            { 
                string calLevelID=eachmTuple.SubEvent_ID.getLevelID(0);
                if (CalEventDictionaryMapping.ContainsKey(calLevelID))
                {
                    CalEventDictionaryMapping[calLevelID].Item2.Add(eachmTuple);
                }
                else
                {
                    CalEventDictionaryMapping.Add(calLevelID, new Tuple<Location, List<SubCalendarEvent>>(eachmTuple.myLocation, new List<SubCalendarEvent>() { eachmTuple }));
                }
            }

            

            Location AverageCurrentOccupiersGPSLocation= Location.AverageGPSLocation(currentOccupiers.Select(obj=>obj.myLocation));

            List<KeyValuePair<string, Tuple<Location, List<SubCalendarEvent>>>> SortedCalendarInfo = CalEventDictionaryMapping.OrderBy(obj => Location.calculateDistance(obj.Value.Item1, AverageCurrentOccupiersGPSLocation)).ToList();

            List<SubCalendarEvent> pertinentSubCalEvents = new List<SubCalendarEvent>();
            TimeSpan durationSofar= new TimeSpan();

            for (int q = 0; q < SortedCalendarInfo.Count; )
            {
                KeyValuePair<string, Tuple<Location, List<SubCalendarEvent>>> eachKeyValuePair = SortedCalendarInfo[q];
                if (eachKeyValuePair.Value.Item2.Count > 0)
                {
                    SubCalendarEvent mySubCal = eachKeyValuePair.Value.Item2[0];
                    pertinentSubCalEvents.Add(mySubCal);
                    durationSofar=durationSofar.Add(mySubCal.ActiveDuration);
                    eachKeyValuePair.Value.Item2.RemoveAt(0);
                }
                else 
                {
                    SortedCalendarInfo.RemoveAt(q);
                    q--;
                }

                if (durationSofar >= CurrentDayReference.TimelineSpan)
                {
                    break;
                }
                else
                {
                    if (++q >= SortedCalendarInfo.Count)
                    {
                        q = 0;
                    }
                }
            }

            pertinentSubCalEvents.AddRange(currentOccupiers);



            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries= new Dictionary<TimeSpan,Dictionary<string,mTuple<bool,SubCalendarEvent>>>();


            foreach (SubCalendarEvent eachmTuple in pertinentSubCalEvents)
            {
                TimeSpan ActiveTimeSpan = eachmTuple.ActiveDuration;
                string subcalStringID = eachmTuple.ID;
                    
                if (PossibleEntries.ContainsKey(ActiveTimeSpan))
                {
                    PossibleEntries[ActiveTimeSpan].Add(subcalStringID, new mTuple<bool, SubCalendarEvent>(true, eachmTuple));
                }
                else
                {
                    PossibleEntries.Add(ActiveTimeSpan, new Dictionary<string, mTuple<bool, SubCalendarEvent>>());
                    PossibleEntries[ActiveTimeSpan].Add(subcalStringID, new mTuple<bool, SubCalendarEvent>(true, eachmTuple));
                }
            }

            List<mTuple<bool, SubCalendarEvent>> AllEventsupdated = stitchUnRestrictedSubCalendarEvent(CurrentDayReference, currentOccupiers.Select(obj => new mTuple<bool, SubCalendarEvent>(false, obj)).ToList(), PossibleEntries, new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>(), 1);

            TimeSpan TotalOfCurrentOccupiers = Utility.SumOfActiveDuration(currentOccupiers);
            TimeSpan totalUsedUpSpace= Utility.SumOfActiveDuration(AllEventsupdated.Select(obj => obj.Item2));


            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibilityListFOrTIghtestForExtraAverga = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
            TimeSpan ExpectedOccupancyTimeSpan = TimeSpan.FromTicks((long)(CurrentDayReference.TimelineSpan.Ticks * (occupancy)));
            TimeSpan TotalOccupancyNotWanted = totalUsedUpSpace-TimeSpan.FromTicks((long)(CurrentDayReference.TimelineSpan.Ticks * (occupancy)));//checks the difference between the total used space and ammount used up by the expected occupancy
            List<SubCalendarEvent> AllTightUsableInfo = AllEventsupdated.Select(obj => obj.Item2).Where(obj => !currentOccupiers.Contains(obj)).ToList();
            Dictionary<SubCalendarEvent, double> OrderOfWorstFill = new Dictionary<SubCalendarEvent, double>();

            

            List<KeyValuePair<SubCalendarEvent, double>> OrderOfWorstFillAsList = OrderOfWorstFill.ToList();
            OrderOfWorstFillAsList=OrderOfWorstFillAsList.OrderBy(obj => obj.Value).Reverse().ToList();

            TimeSpan TotalTimeSofar= new TimeSpan(0);
            int i = -1;
            bool breakWasSelected = false;
            int NumberOfElement = 0;

            List<SubCalendarEvent> removeTheseElements = new List<SubCalendarEvent>();


            while (((Utility.SumOfActiveDuration(AllTightUsableInfo) + TotalOfCurrentOccupiers) > ExpectedOccupancyTimeSpan) && (AllTightUsableInfo.Count > 0))// && (TotalOccupancyNotWanted.Ticks > 0))//checks if 
            {
                OrderOfWorstFill = AllTightUsableInfo.ToDictionary(obj => obj, obj => DistanceSolver.AverageToAllNodes(obj, AllTightUsableInfo, CalendarEvent.DistanceMatrix));
                OrderOfWorstFillAsList = OrderOfWorstFill.ToList();
                OrderOfWorstFillAsList = OrderOfWorstFillAsList.OrderBy(obj => obj.Value).Reverse().ToList();
                KeyValuePair<SubCalendarEvent, double> eachKeyValuePair = OrderOfWorstFillAsList[0];
                removeTheseElements.Add(eachKeyValuePair.Key);
                AllTightUsableInfo.Remove(eachKeyValuePair.Key);
            }
            removeTheseElements=removeTheseElements.OrderBy(obj => obj.ActiveDuration).ToList();
            if (removeTheseElements.Count > 0)
            {
                AllTightUsableInfo.Add(removeTheseElements[0]);
                removeTheseElements.RemoveAt(0);
            }

            foreach (SubCalendarEvent eachSubCalendarEvent in removeTheseElements)
            {
                eachSubCalendarEvent.shiftEvent(PreservedValues[eachSubCalendarEvent.ID].Item2.Start - eachSubCalendarEvent.Start);
            }

            Tuple<List<SubCalendarEvent>, TimeLine> retValue;
            List<SubCalendarEvent> WhatsLeftAndViable = AllTightUsableInfo;//.Select(obj => obj.Key).ToList();
            WhatsLeftAndViable.AddRange(currentOccupiers);

            WhatsLeftAndViable = WhatsLeftAndViable.OrderBy(obj => obj.End).ToList();

            retValue = new Tuple<List<SubCalendarEvent>, TimeLine>(WhatsLeftAndViable, CurrentDayReference);


            /*
            for (i = 0; i < OrderOfWorstFillAsList.Count; i++)
            {
                KeyValuePair<SubCalendarEvent, double> eachKeyValuePair = OrderOfWorstFillAsList[i];   
                TotalTimeSofar += eachKeyValuePair.Key.ActiveDuration;
                ++NumberOfElement;
                if (TotalTimeSofar > TotalOccupancyNotWanted)
                {
                    breakWasSelected = true;
                    break;
                }
            }

            if (NumberOfElement > 0)
            {
                --i;
                for (; i >= 0; --i)
                {
                    OrderOfWorstFillAsList.RemoveAt(0);
                }
            }
            

            WhatsLeftAndViable = OrderOfWorstFillAsList.Select(obj => obj.Key).ToList();
            WhatsLeftAndViable.AddRange(currentOccupiers);
            WhatsLeftAndViable=WhatsLeftAndViable.OrderBy(obj => obj.End).ToList();
            */
            


            return retValue;
        }

        List<SubCalendarEvent> TossEndWards(IEnumerable<SubCalendarEvent> AllSubEvents, TimeLine FreeSpot)
        {
            List<SubCalendarEvent>retValue=new List<SubCalendarEvent>();
            AllSubEvents=AllSubEvents.Where(obj => obj.canExistWithinTimeLine(FreeSpot));
            if (AllSubEvents.Count() > 0)
            {
                AllSubEvents=AllSubEvents.OrderBy(obj => obj.getCalendarEventRange.End).Reverse();
                SubCalendarEvent FirstElement= AllSubEvents.First();
                FirstElement.PinToEnd(FreeSpot);
                retValue.Add(FirstElement);
                AllSubEvents = AllSubEvents.Where(obj => obj != FirstElement);//hack alert this can be made faster by passing AllSubevents as a collection and then removing the first element.
                FreeSpot = new TimeLine(FreeSpot.Start,FirstElement.Start );
                retValue.AddRange(TossEndWards(AllSubEvents, FreeSpot));
            }

            return retValue;
        }



        List<mTuple<bool, SubCalendarEvent>> FurtherFillTimeLineWithSubCalEvents(List<mTuple<bool, SubCalendarEvent>> AllReadyAssignedSubCalEvents, TimeLine ReferenceTimeLine, Dictionary<TimeLine, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AllCompatibleWithList, TimeLine PreceedingTimeLine, Dictionary<TimeLine, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> PossibleEntries)
        {
            /*
             * CompatibleWithList has whats left after stitchUnRestrictedSubCalendarEvent has removed all possible fittable Events
             * Hack Alert: The current implementation does not optimize for restricted values
             */

            List<SubCalendarEvent> AssignedSubCalendarEvents = new System.Collections.Generic.List<SubCalendarEvent>();
            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in AllReadyAssignedSubCalEvents)
            {
                AssignedSubCalendarEvents.Add(eachmTuple.Item2);
            }


            List<TimeSpanWithStringID> LeftOvers = new System.Collections.Generic.List<TimeSpanWithStringID>();

            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in AllCompatibleWithList[PreceedingTimeLine])
            {
                if (eachKeyValuePair.Value.Item1 > 0)
                {
                    LeftOvers.Add(eachKeyValuePair.Value.Item2);
                }
            }

            PreceedingTimeLine.Empty();
            TimeLine UpdatedTImeLine = Utility.AddSubCaleventsToTimeLine(PreceedingTimeLine, AssignedSubCalendarEvents);
            PreceedingTimeLine.AddBusySlots(UpdatedTImeLine.OccupiedSlots);

            List<TimeLine> AllFreeSpots = PreceedingTimeLine.getAllFreeSlots().ToList();
            Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> matchingValidSubcalendarEvents = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();//Dictionary contains a match up of the free within already assigned variables and possible fillers
            Dictionary<TimeLine, Dictionary<string, mTuple<int, TimeSpanWithStringID>>> ForSnugArray = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>();
            Dictionary<TimeLine, SnugArray> FreeSpotSnugArrays = new System.Collections.Generic.Dictionary<TimeLine, SnugArray>();
            Dictionary<TimeLine, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> FreeSpotSnugPossibiilities = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
            //Dictionary<TimeLine, List<TimeSpanWithStringID>> FreeSpotSnugPossibilities = new System.Collections.Generic.Dictionary<TimeLine, SnugArray>();
            Dictionary<string, SubCalendarEvent> AllMovableSubCalEvents = new System.Collections.Generic.Dictionary<string, SubCalendarEvent>();
            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AllMoveOvrSet = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();
            // Dictionary<string, Dictionary<SubCalendarEvent, List<TimeLine>>> SubEventToMatchingTimeLinePossible = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<SubCalendarEvent, System.Collections.Generic.List<TimeLine>>>();
            Dictionary<TimeLine, Dictionary<TimeSpan, List<mTuple<bool, SubCalendarEvent>>>> TimeLine_WithMathChingSubCalevents = new Dictionary<TimeLine, Dictionary<TimeSpan, List<mTuple<bool, SubCalendarEvent>>>>();// string in second dictionary is the String of the duration of the SubCalendarEvent
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> TotalPossibleTimeSpanWithStrings = new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();




            foreach (TimeLine eachTimeLine in AllFreeSpots)
            {
                List<mTuple<bool, SubCalendarEvent>> PossibleFillers = removeSubCalEventsThatCantWorkWithTimeLine(eachTimeLine, PossibleEntries[PreceedingTimeLine].ToList(), true);//hack we need to ensure cases of partial fit
                ForSnugArray.Add(eachTimeLine, new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>());
                TimeLine_WithMathChingSubCalevents.Add(eachTimeLine, new System.Collections.Generic.Dictionary<TimeSpan, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>());

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

                    if (!AllMovableSubCalEvents.ContainsKey(eachmTuple.Item2.ID))//populates all movable SubCalendarEVents
                    {
                        AllMovableSubCalEvents.Add(eachmTuple.Item2.ID, eachmTuple.Item2);

                        if (TotalPossibleTimeSpanWithStrings.ContainsKey(eachmTuple.Item2.ActiveDuration))
                        {
                            ++TotalPossibleTimeSpanWithStrings[eachmTuple.Item2.ActiveDuration].Item1;
                        }
                        else
                        {
                            TotalPossibleTimeSpanWithStrings.Add(eachmTuple.Item2.ActiveDuration, new mTuple<int, TimeSpanWithStringID>(1, new TimeSpanWithStringID(eachmTuple.Item2.ActiveDuration, eachmTuple.Item2.ActiveDuration.ToString())));
                        }
                    }
                    TimeSpan IdTimeSpan = eachmTuple.Item2.ActiveDuration;

                    if (TimeLine_WithMathChingSubCalevents[eachTimeLine].ContainsKey(IdTimeSpan))
                    {
                        TimeLine_WithMathChingSubCalevents[eachTimeLine][IdTimeSpan].Add(eachmTuple);
                    }
                    else
                    {
                        TimeLine_WithMathChingSubCalevents[eachTimeLine].Add(IdTimeSpan, new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>() { eachmTuple });
                    }


                    /*if (SubEventToMatchingTimeLinePossible.ContainsKey(eachmTuple.Item2.ActiveDuration.ToString()))// builds a dictionary of the TimeSpan String ID and a Dictionary of SubCalendar EVents with a List of feasible TimeLine
                    {
                        if (SubEventToMatchingTimeLinePossible[eachmTuple.Item2.ActiveDuration.ToString()].ContainsKey(eachmTuple.Item2))
                        {
                            SubEventToMatchingTimeLinePossible[eachmTuple.Item2.ActiveDuration.ToString()][eachmTuple.Item2].Add(eachTimeLine);
                        }
                        else 
                        {
                            SubEventToMatchingTimeLinePossible[eachmTuple.Item2.ActiveDuration.ToString()].Add(eachmTuple.Item2, new System.Collections.Generic.List<TimeLine>() { eachTimeLine });
                        }
                    }
                    else
                    {
                        SubEventToMatchingTimeLinePossible.Add(eachmTuple.Item2.ActiveDuration.ToString(), new System.Collections.Generic.Dictionary<SubCalendarEvent, System.Collections.Generic.List<TimeLine>>());
                        SubEventToMatchingTimeLinePossible[eachmTuple.Item2.ActiveDuration.ToString()].Add(eachmTuple.Item2, new System.Collections.Generic.List<TimeLine>() { eachTimeLine });
                    }*/
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

            foreach (List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> allFreeSpotSnugPossibiilities in FreeSpotSnugPossibiilities.Values)
            {
                AllMoveOvrSet.AddRange(allFreeSpotSnugPossibiilities);
            }

            TimeSpanWithStringID UnAssignedTimeSpanWithString = new TimeSpanWithStringID(ReferenceTimeLine.TimelineSpan, ReferenceTimeLine.TimelineSpan.ToString());
            /*if (LeftOvers.Count > 0)
            {
                UnAssignedTimeSpanWithString = LeftOvers[0];
            }*/

            TimeSpan TotalMovedSofar = new TimeSpan(0);

            Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> FreeSpots_MatchedEvents = new System.Collections.Generic.Dictionary<TimeLine, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();

            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleListForReferenceTimeLine = AllCompatibleWithList[ReferenceTimeLine];

            foreach (TimeLine eachTimeLine in AllFreeSpots)
            {
                FreeSpots_MatchedEvents.Add(eachTimeLine, new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>());
                Tuple<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> UpdatedCompatibleList_BestSnugPossibility = UpdateCompatibleListOfTimeLine(CompatibleListForReferenceTimeLine, FreeSpotSnugPossibiilities[eachTimeLine], ReferenceTimeLine, UnAssignedTimeSpanWithString, TotalPossibleTimeSpanWithStrings);
                Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> UpdatedCompatibleList = UpdatedCompatibleList_BestSnugPossibility.Item1;
                Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> BestSnugAvailability = UpdatedCompatibleList_BestSnugPossibility.Item2;
                TotalMovedSofar += SnugArray.TotalTimeSpanOfSnugPossibility(BestSnugAvailability);
                Dictionary<TimeSpan, List<mTuple<bool, SubCalendarEvent>>> TimeLineMatchingSubEvents = TimeLine_WithMathChingSubCalevents[eachTimeLine];

                foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in BestSnugAvailability)
                {
                    if (TimeLineMatchingSubEvents[eachKeyValuePair.Key].Count > 0)
                    {
                        FreeSpots_MatchedEvents[eachTimeLine].Add(TimeLineMatchingSubEvents[eachKeyValuePair.Key][0]);
                        TimeLineMatchingSubEvents[eachKeyValuePair.Key].RemoveAt(0);
                        --TotalPossibleTimeSpanWithStrings[eachKeyValuePair.Key].Item1;
                        if (TotalPossibleTimeSpanWithStrings[eachKeyValuePair.Key].Item1 == 0)
                        {
                            TotalPossibleTimeSpanWithStrings.Remove(eachKeyValuePair.Key);
                        }
                    }
                    else
                    {
                        ;
                    }
                }


                //KeyValuePair<string, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair
                List<TimeSpan> AllKeys = CompatibleListForReferenceTimeLine.Keys.ToList();
                foreach (TimeSpan eachTimeSpan in AllKeys)//Updates the  Compatible List of the Reference timelin, in order to reflect movement in data
                {
                    if (UpdatedCompatibleList.ContainsKey(eachTimeSpan))//checks if reference timeLine has updated  keys
                    {
                        CompatibleListForReferenceTimeLine[eachTimeSpan] = UpdatedCompatibleList[eachTimeSpan];
                    }
                    else
                    {
                        CompatibleListForReferenceTimeLine.Remove(eachTimeSpan);
                    }
                }
            }





            TimeSpan FreeSpaceAfterMovement = ReferenceTimeLine.TimelineSpan - SnugArray.TotalTimeSpanOfSnugPossibility(CompatibleListForReferenceTimeLine);

            if (LeftOvers.Count > 0)//checks if there are move overs from preceeding compatible TimeLine
            {
                SnugArray BestFitIntoCurrentTimeLine = new SnugArray(AllCompatibleWithList[PreceedingTimeLine].Values.ToList(), FreeSpaceAfterMovement);//Tries to construct tightest fit on newly created free space in reference timelin....hack alert, it does not prioritize restricted elements

                List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> SortedData = BestFitIntoCurrentTimeLine.MySnugPossibleEntries;//sort the snug possibility based on mst filling
                if (SortedData.Count > 0)
                {
                    Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> selectedDictionary = SortedData[SortedData.Count - 1];

                    foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in selectedDictionary)
                    {
                        if (CompatibleListForReferenceTimeLine.ContainsKey(eachKeyValuePair.Key))
                        {
                            CompatibleListForReferenceTimeLine[eachKeyValuePair.Key].Item1 += eachKeyValuePair.Value.Item1;
                            AllCompatibleWithList[PreceedingTimeLine][eachKeyValuePair.Key].Item1 -= eachKeyValuePair.Value.Item1;
                        }
                        else
                        {
                            CompatibleListForReferenceTimeLine.Add(eachKeyValuePair.Key, eachKeyValuePair.Value);
                            AllCompatibleWithList[PreceedingTimeLine][eachKeyValuePair.Key].Item1 -= eachKeyValuePair.Value.Item1;
                        }

                        if (AllCompatibleWithList[PreceedingTimeLine][eachKeyValuePair.Key].Item1 < 0)
                        {
                            ;
                        }
                    }
                }
            }

            List<mTuple<bool, SubCalendarEvent>> retValue = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>(AllReadyAssignedSubCalEvents);
            foreach (TimeLine eachTimeLine in AllFreeSpots)//Snaps each Subcalevent to the beginning of a free TimeLine
            {
                List<SubCalendarEvent> ListOfSubCalEvents = Utility.mTupleToSubCalEvents(FreeSpots_MatchedEvents[eachTimeLine]);
                if (Utility.PinSubEventsToStart(ListOfSubCalEvents, eachTimeLine))
                {
                    retValue.AddRange(FreeSpots_MatchedEvents[eachTimeLine]);
                }
                else
                {
                    ;
                }
            }

            return retValue;



        }



        Tuple<DateTime, List<SubCalendarEvent>> ObtainBetterEarlierReferenceTime(List<SubCalendarEvent> CurrentlyOptimizedList, Dictionary<string, Dictionary<string, SubCalendarEvent>> CalendarIDAndNonPartialSubCalEvents, TimeSpan LimitingTimeSpan, DateTime CurrentEarliestReferenceTIme, TimeLine PinToStartTimeLine, SubCalendarEvent LastSubCalEvent, bool Aggressive = true)
        {
            Tuple<DateTime, List<SubCalendarEvent>> retValue = null;
            CurrentlyOptimizedList = CurrentlyOptimizedList.ToList();
            Dictionary<string, double> AllValidNodes;
            AllValidNodes = CalendarEvent.DistanceToAllNodes("");
            if (LastSubCalEvent != null)
            {
                AllValidNodes = CalendarEvent.DistanceToAllNodes(LastSubCalEvent.SubEvent_ID.getLevelID(0));
            }


            //if (CurrentlyOptimizedList.Count > 0)
            {

                //LastSubCalEvent = CurrentlyOptimizedList[CurrentlyOptimizedList.Count - 1];




                DateTime EarliestReferenceTIme = new DateTime();
                SubCalendarEvent AppendableEVent;
                bool BreakOutsideForLoop = false;
                if (Aggressive)
                {
                    IEnumerable<string> plausibleStrings = new List<string>();
                    plausibleStrings = AllValidNodes.Keys.Where(obj => CalendarIDAndNonPartialSubCalEvents.ContainsKey(obj));
                    IEnumerable<Dictionary<string, SubCalendarEvent>> AllValidDicts;
                    if (plausibleStrings.Count() > 0)
                    {
                        AllValidDicts = plausibleStrings.Select(obj => CalendarIDAndNonPartialSubCalEvents[obj]);
                        if (AllValidDicts.Count() > 0)
                        {
                            SubCalendarEvent earliestSubCalEvent = null;
                            AllValidDicts = AllValidDicts.OrderBy(obj => obj.Values.ToArray()[0].getCalendarEventRange.End);


                            foreach (Dictionary<string, SubCalendarEvent> eachDict in AllValidDicts)
                            {
                                IEnumerable<SubCalendarEvent> AllSubCalevents = eachDict.Values.OrderBy(obj => obj.getCalendarEventRange.Start);
                                AllSubCalevents = AllSubCalevents.Where(obj => (obj.ActiveDuration <= (LimitingTimeSpan)) && (!CurrentlyOptimizedList.Contains(obj)));
                                if (AllSubCalevents.Count() > 0)
                                {
                                    if (earliestSubCalEvent == null)
                                    {
                                        earliestSubCalEvent = AllSubCalevents.ToList()[0];
                                    }
                                    else
                                    {
                                        SubCalendarEvent retrievedEarliestSubCal = AllSubCalevents.ToList()[0];
                                        if (retrievedEarliestSubCal.getCalendarEventRange.End < earliestSubCalEvent.getCalendarEventRange.End)
                                        {
                                            earliestSubCalEvent = retrievedEarliestSubCal;
                                        }
                                        else
                                        {
                                            if ((retrievedEarliestSubCal.getCalendarEventRange.Start == earliestSubCalEvent.getCalendarEventRange.Start) && (retrievedEarliestSubCal.ActiveDuration > earliestSubCalEvent.ActiveDuration))
                                            {
                                                earliestSubCalEvent = retrievedEarliestSubCal;
                                            }
                                        }
                                    }

                                }
                            }
                            if (earliestSubCalEvent != null)
                            {
                                CurrentlyOptimizedList.Add(earliestSubCalEvent);
                                bool error = Utility.PinSubEventsToStart(CurrentlyOptimizedList, PinToStartTimeLine);
                                if (error)
                                {
                                    EarliestReferenceTIme = earliestSubCalEvent.End;

                                    retValue = new Tuple<DateTime, List<SubCalendarEvent>>(EarliestReferenceTIme, CurrentlyOptimizedList);
                                    BreakOutsideForLoop = true;
                                }
                                else
                                {
                                    CurrentlyOptimizedList.Remove(earliestSubCalEvent);
                                }

                            }
                        }
                    }

                }
                else
                {
                    foreach (string eachstring in AllValidNodes.Keys)
                    {

                        if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(eachstring))
                        {
                            List<KeyValuePair<string, SubCalendarEvent>> AllSubCalEvent = CalendarIDAndNonPartialSubCalEvents[eachstring].ToList();
                            for (int i = 0; i < AllSubCalEvent.Count; i++)
                            {
                                AppendableEVent = CalendarIDAndNonPartialSubCalEvents[eachstring].ToList()[i].Value;//Assumes Theres Always an element
                                if ((AppendableEVent.ActiveDuration <= (LimitingTimeSpan)) && (!CurrentlyOptimizedList.Contains(AppendableEVent)))
                                {
                                    CurrentlyOptimizedList.Add(AppendableEVent);
                                    CalendarIDAndNonPartialSubCalEvents[eachstring].Remove(AppendableEVent.ID);
                                    if (CalendarIDAndNonPartialSubCalEvents[eachstring].Count < 1)//checks if List is empty. Deletes keyValuepair if list is empty
                                    {
                                        CalendarIDAndNonPartialSubCalEvents.Remove(eachstring);
                                    }
                                    //FreeSpotUpdated = new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End);
                                    Utility.PinSubEventsToStart(CurrentlyOptimizedList, PinToStartTimeLine);
                                    EarliestReferenceTIme = AppendableEVent.End;
                                    //retValue = new Tuple<DateTime, List<SubCalendarEvent>>(new DateTime(), new List<SubCalendarEvent>());
                                    retValue = new Tuple<DateTime, List<SubCalendarEvent>>(EarliestReferenceTIme, CurrentlyOptimizedList);
                                    BreakOutsideForLoop = true;
                                    break;
                                }
                            }

                        }
                        if (BreakOutsideForLoop)
                        {
                            break;
                        }
                    }
                }
            }

            return retValue;
        }


        Tuple<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> UpdateCompatibleListOfTimeLine(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CurrentCompatibleList, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> MovedOverSet, TimeLine ReferenceTimeLine, TimeSpanWithStringID LeftOuts, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> TotalOfMovedVariables)
        {
            //Hack alert: You need to create a situation that enforces a restricted event as being assigned first

            TimeSpan CurrentTotalOfSnugVariables = new TimeSpan(0);
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> retValue_MovedVariables = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> retValue_CurrentCompatibleList = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();

            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CurrentCompatibleList_Cpy = SnugArray.CreateCopyOFSnuPossibilities(CurrentCompatibleList);
            //TimeSpan SumOfLeftOuts= new TimeSpan(0);
            TimeSpan RemainderTimeSpan = new TimeSpan(0);
            Dictionary<string, mTuple<int, TimeSpanWithStringID>> MovedOverListUpdate = new System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>();

            if (LeftOuts.timeSpan > ReferenceTimeLine.TimelineSpan)
            {
                return null;
            }





            CurrentTotalOfSnugVariables = SnugArray.TotalTimeSpanOfSnugPossibility(CurrentCompatibleList_Cpy);

            RemainderTimeSpan = ReferenceTimeLine.TimelineSpan - CurrentTotalOfSnugVariables;
            TimeSpan RemainderOfLeftOverChunk = LeftOuts.timeSpan - RemainderTimeSpan;
            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> MovedOverSet_Cpy = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();

            foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in MovedOverSet)
            {
                MovedOverSet_Cpy.Add(compareMovedOverSetWithTotalPossibleEntries(eachDictionary, TotalOfMovedVariables));

            }


            while (RemainderOfLeftOverChunk.Ticks < 0)
            {
                ReferenceTimeLine = new TimeLine(ReferenceTimeLine.Start, ReferenceTimeLine.End - LeftOuts.timeSpan);
                RemainderOfLeftOverChunk = LeftOuts.timeSpan - RemainderTimeSpan;
            }


            SnugArray FitsInChunkOfRemainder_SnugArray = new SnugArray(CurrentCompatibleList_Cpy.Values.ToList(), RemainderOfLeftOverChunk);
            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> SnugPossibilities = FitsInChunkOfRemainder_SnugArray.MySnugPossibleEntries;
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> Viable_SnugPossibilities = getPlausibleEntriesFromMovedOverSet(MovedOverSet_Cpy, SnugPossibilities);
            Dictionary<Dictionary<string, mTuple<int, TimeSpanWithStringID>>, List<Dictionary<string, mTuple<int, TimeSpanWithStringID>>>> LeftAfterRemovalSnugPossibilities = new System.Collections.Generic.Dictionary<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>, System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, mTuple<int, TimeSpanWithStringID>>>>();
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> BestSnugPossibility = Viable_SnugPossibilities;
            retValue_MovedVariables = SnugArray.AddToSnugPossibilityList(retValue_MovedVariables, BestSnugPossibility);
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CurrentCompatibleList_Cpy_updated = SnugArray.RemoveSnugPossibilityFromAnother(CurrentCompatibleList_Cpy, BestSnugPossibility);


            retValue_CurrentCompatibleList = SnugArray.RemoveSnugPossibilityFromAnother(CurrentCompatibleList, retValue_MovedVariables);
            //retValue_CurrentCompatibleList = SnugArray.AddToSnugPossibilityList(CurrentCompatibleList, MovedOverListUpdate);


            Tuple<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> retValue = new Tuple<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>(retValue_CurrentCompatibleList, retValue_MovedVariables);
            //item1 is Updated CurrentCompatible List
            //item2 is Best Snug change to timeLine

            return retValue;
        }

        Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> compareMovedOverSetWithTotalPossibleEntries(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> MovedOverSet, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> TotalSet)
        {
            MovedOverSet = SnugArray.CreateCopyOFSnuPossibilities(MovedOverSet);
            List<KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>>> ListForDict = MovedOverSet.ToList();

            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in ListForDict)
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


        Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> getBestSnugPossiblity(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CurrentCompatibleList, Dictionary<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> LeftAfterRemovalSnugPossibilities)
        {
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CurrentCompatibleList_cpy = SnugArray.CreateCopyOFSnuPossibilities(CurrentCompatibleList);
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> retValue = new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
            foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in LeftAfterRemovalSnugPossibilities.Keys)//tries each snugPossibility as a potential i
            {
                Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> myCurrentCompatibleList = SnugArray.CreateCopyOFSnuPossibilities(CurrentCompatibleList_cpy);

                List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> ListOfLeftOfMovedOverSnugArray = LeftAfterRemovalSnugPossibilities[eachDictionary];
                foreach (TimeSpan eachTimeSpan in eachDictionary.Keys)
                {
                    myCurrentCompatibleList[eachTimeSpan].Item1 -= eachDictionary[eachTimeSpan].Item1;
                    if (myCurrentCompatibleList[eachTimeSpan].Item1 < 1)//removes mTuple where the TImeSpan spring is zero
                    {
                        myCurrentCompatibleList.Remove(eachTimeSpan);
                    }
                }

                List<KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>>> OtherValuesFromMyCurrenCompatibleList = myCurrentCompatibleList.ToList();
                OtherValuesFromMyCurrenCompatibleList = OtherValuesFromMyCurrenCompatibleList.OrderBy(obj => obj.Value.Item2.timeSpan).ToList();
                OtherValuesFromMyCurrenCompatibleList.Reverse();


                foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary0 in ListOfLeftOfMovedOverSnugArray)
                {
                    Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> Potential_retValue = SnugArray.CreateCopyOFSnuPossibilities(eachDictionary);
                    foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in OtherValuesFromMyCurrenCompatibleList)
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
                            if (Potential_retValue.ContainsKey(UpdatedmTuple.Item2.timeSpan))
                            {
                                Potential_retValue[UpdatedmTuple.Item2.timeSpan].Item1 += UpdatedmTuple.Item1;
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
                        if ((TotalTimeSpanSnug_Possible == TotalTimeSpanSnugRetValue) && (retValue.Count < Potential_retValue.Count))
                        {
                            retValue = Potential_retValue;
                        }
                    }
                }
            }

            return retValue;
        }

        Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> getPlausibleEntriesFromMovedOverSet(List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> MovedOverSet, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> SnugPossibilities)
        {
            /*
             * This function goes through the MovedOverSet and compares it with the snug possibilities.
             * It tries to find any set in the Movedoverset that has all the keys in the snug possibilities variable.
             * If it finds one with all these keys, it selects dict in which the int Count of the matching mTuple has a greater than or equal the current option of the current snug possibility mtuple
             */
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> retValue = new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();



            foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in MovedOverSet)
            {
                foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary0 in SnugPossibilities)
                {
                    IEnumerable<TimeSpan> Intersection = eachDictionary0.Keys.Intersect(eachDictionary.Keys);
                    Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> Possible_RetValue = new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
                    foreach (TimeSpan eachTimeSpan in Intersection)
                    {
                        Possible_RetValue.Add(eachTimeSpan, eachDictionary[eachTimeSpan].Item1 < eachDictionary0[eachTimeSpan].Item1 ? new mTuple<int, TimeSpanWithStringID>(eachDictionary[eachTimeSpan]) : new mTuple<int, TimeSpanWithStringID>(eachDictionary0[eachTimeSpan]));
                    }
                    if (SnugArray.TotalTimeSpanOfSnugPossibility(retValue) < SnugArray.TotalTimeSpanOfSnugPossibility(Possible_RetValue))
                    {
                        retValue = Possible_RetValue;
                    }
                }
            }
            return retValue;
        }

        List<mTuple<bool, SubCalendarEvent>> stitchUnRestrictedSubCalendarEvent(TimeLine FreeBoundary, List<mTuple<bool, SubCalendarEvent>> restrictedSnugFitAvailable, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithList, double Occupancy)
        {
            TimeLine[] AllFreeSpots = FreeBoundary.getAllFreeSlots();
            int TotalEventsForThisTImeLine = 0;

            if (FreeBoundary.End == new DateTime(2014, 04, 8, 17, 00, 0))
            {
                ;
            }

            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in CompatibleWithList)
            {
                TotalEventsForThisTImeLine += eachKeyValuePair.Value.Item1;
            }

            CompatibleWithList.Clear();


            DateTime EarliestReferenceTIme = FreeBoundary.Start;
            List<mTuple<bool, SubCalendarEvent>> FrontPartials = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            List<mTuple<bool, SubCalendarEvent>> EndPartials = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            Dictionary<DateTime, List<mTuple<bool, SubCalendarEvent>>> FrontPartials_Dict = new System.Collections.Generic.Dictionary<DateTime, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();
            Dictionary<DateTime, List<mTuple<bool, SubCalendarEvent>>> EndPartials_Dict = new System.Collections.Generic.Dictionary<DateTime, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();
            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries_Cpy = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();
            Dictionary<string, Dictionary<string, SubCalendarEvent>> CalendarIDAndNonPartialSubCalEvents = new Dictionary<string, Dictionary<string, SubCalendarEvent>>();//List of non partials for current Reference StartTime To End of FreeBoundary. Its gets updated with Partials once the earliest reference time passes the partial event start time

            foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleEntries)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                Dictionary<string, mTuple<bool, SubCalendarEvent>> NewDictEntry = new Dictionary<string, mTuple<bool, SubCalendarEvent>>();

                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> KeyValuePair0 in eachKeyValuePair.Value)
                {
                    mTuple<bool, SubCalendarEvent> MyEvent = KeyValuePair0.Value;

                    bool isInrestrictedSnugFitAvailable = false;
                    if (CompatibleWithList.ContainsKey(eachKeyValuePair.Key))
                    {
                        ++CompatibleWithList[eachKeyValuePair.Key].Item1;
                    }
                    else
                    {
                        CompatibleWithList.Add(eachKeyValuePair.Key, new mTuple<int, TimeSpanWithStringID>(1, new TimeSpanWithStringID(KeyValuePair0.Value.Item2.ActiveDuration, KeyValuePair0.Value.Item2.ActiveDuration.Ticks.ToString())));
                    }

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
                            string CalLevel0ID = KeyValuePair0.Value.Item2.SubEvent_ID.getLevelID(0);
                            if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(CalLevel0ID))
                            {
                                CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(KeyValuePair0.Value.Item2.ID, KeyValuePair0.Value.Item2);
                            }
                            else
                            {
                                //CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new List<SubCalendarEvent>() { KeyValuePair0.Value.Item2 });
                                CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new Dictionary<string, SubCalendarEvent>());
                                CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(KeyValuePair0.Value.Item2.ID, KeyValuePair0.Value.Item2);
                            }
                        }
                    }
                }
                if (NewDictEntry.Count > 0)
                { PossibleEntries_Cpy.Add(eachKeyValuePair.Key, NewDictEntry); }

            }

            FrontPartials = FrontPartials.OrderBy(obj => obj.Item2.getCalendarEventRange.Start).ToList();
            EndPartials = EndPartials.OrderBy(obj => obj.Item2.getCalendarEventRange.End).ToList();

            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in FrontPartials)//populates FrontPartials_Dict in ordered manner since FrontPartials is ordered
            {
                if (FrontPartials_Dict.ContainsKey(eachmTuple.Item2.getCalendarEventRange.Start))
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


            /*foreach (mTuple<bool, SubCalendarEvent> eachmTuple in restrictedSnugFitAvailable)//removes the restricted from CompatibleWithList
            {
                --CompatibleWithList[eachmTuple.Item2.ActiveDuration.Ticks.ToString()].Item1;
                //PossibleEntries_Cpy[eachmTuple.Item2.ActiveDuration.Ticks.ToString()].Remove(eachmTuple.Item2.ID);
            }*/

            List<DateTime> ListOfFrontPartialsStartTime = FrontPartials_Dict.Keys.ToList();

            int i = 0;
            int j = 0;
            int FrontPartialCounter = 0;

            if (restrictedSnugFitAvailable.Count < 1)
            {
                ;
            }

            Tuple<DateTime, List<SubCalendarEvent>> TimeLineUpdated = null;
            SubCalendarEvent BorderElementBeginning = null;
            SubCalendarEvent BorderElementEnd = null;
            SubCalendarEvent LastSubCalElementForEarlierReferenceTime = null;
            int a = restrictedSnugFitAvailable.Count;
            int previ = i;
            for (; i < restrictedSnugFitAvailable.Count; i++)
            {
                //bool isFreeSpotBeforeRigid = AllFreeSpots[i].End <= restrictedSnugFitAvailable[i].Item2.Start;
                TimeLineUpdated = null;


                if (a != restrictedSnugFitAvailable.Count)
                {
                    ;
                }
                if (i == 9)
                {
                    ;
                }
                previ = i;
                /*
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
                 
                 //eliminating excess comments
                */

                int DateTimeCounter = 0;
                List<SubCalendarEvent> LowestCostArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
                TimeLine PertinentFreeSpot = null;
                TimeLine FreeSpotUpdated = null;
                j = i + 1;
                if (ListOfFrontPartialsStartTime.Count > 0)//fits any sub calEvent in preceeding restricting free spot
                {
                    DateTime RestrictedStopper = restrictedSnugFitAvailable[i].Item2.Start;


                    bool breakForLoop = false;
                    bool PreserveRestrictedIndex = false;
                    bool ContinueTrestrictedSnugFitAvailableoForLoop = false;
                    for (; ((FrontPartialCounter < ListOfFrontPartialsStartTime.Count) && (i < restrictedSnugFitAvailable.Count)); FrontPartialCounter++)
                    {
                        TimeLineUpdated = null;
                        DateTime PertinentFreeSpotStart = EarliestReferenceTIme;
                        DateTime PertinentFreeSpotEnd;

                        if (restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.Start <= EarliestReferenceTIme)//this is to ensure the tightest configuration. If the restricted element calendarevent start range already preceedes the current start time then it can be appended immediately. because every other element is less restricted
                        {
                            CompleteArranegement.Add(restrictedSnugFitAvailable[i].Item2);
                            Utility.PinSubEventsToStart(CompleteArranegement, FreeBoundary);
                            EarliestReferenceTIme = CompleteArranegement[CompleteArranegement.Count - 1].End;
                            List<SubCalendarEvent> TempList = new List<SubCalendarEvent>() { restrictedSnugFitAvailable[i].Item2 };
                            ContinueTrestrictedSnugFitAvailableoForLoop = true;//forces the continuation of the for loop for (; i < restrictedSnugFitAvailable.Count; i++)
                            PreserveRestrictedIndex = false;
                            break;
                        }



                        if ((ListOfFrontPartialsStartTime[FrontPartialCounter] < RestrictedStopper))
                        {
                            PertinentFreeSpotEnd = ListOfFrontPartialsStartTime[FrontPartialCounter];
                            //FrontPartials_Dict.Remove(ListOfFrontPartialsStartTime[FrontPartialCounter]);
                            ListOfFrontPartialsStartTime.RemoveAt(FrontPartialCounter);
                            --FrontPartialCounter;
                            PreserveRestrictedIndex = true;
                        }
                        else
                        {
                            PertinentFreeSpotEnd = RestrictedStopper;

                            if (breakForLoop)
                            {//populates with final boundary for each restricted
                                /* PertinentFreeSpot = new TimeLine(PertinentFreeSpotStart, PertinentFreeSpotEnd);

                                 BorderElementBeginning = CompleteArranegement.Count>0?CompleteArranegement[CompleteArranegement.Count-1]:null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                                 BorderElementEnd = restrictedSnugFitAvailable[i].Item2;//uses restricted value as boundary element
                                 LowestCostArrangement = OptimizeArrangeOfSubCalEvent(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy,Occupancy);
                                 DateTime EarliestTimeForBetterEarlierReferenceTime = PertinentFreeSpot.Start;
                                 LastSubCalElementForEarlierReferenceTime = ((CompleteArranegement.Count < 1) || (CompleteArranegement == null) ? null : CompleteArranegement[CompleteArranegement.Count - 1]);
                                     FreeSpotUpdated = PertinentFreeSpot.CreateCopy();
                                 if (LowestCostArrangement.Count > 0)
                                 {
                                     if (!(LowestCostArrangement[0].getCalendarEventRange.Start == PertinentFreeSpot.Start))//Pin SubEvents To Start
                                     {//if the first element is not a partial Sub Cal Event element
                                         FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                                         Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                                     }
                                     else
                                     {
                                         FreeSpotUpdated = PertinentFreeSpot.CreateCopy();// new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                                         Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot);
                                     }
                                     EarliestReferenceTIme = PertinentFreeSpot.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;

                                     SubCalendarEvent LastSubCalEvent = LowestCostArrangement[LowestCostArrangement.Count - 1];
                                     EarliestTimeForBetterEarlierReferenceTime = LastSubCalEvent.End;
                                     LastSubCalElementForEarlierReferenceTime = LastSubCalEvent;
                                    
                                 }
                                 TimeLineUpdated = null;
                                 TimeLineUpdated = ObtainBetterEarlierReferenceTime(LowestCostArrangement, CalendarIDAndNonPartialSubCalEvents, RestrictedStopper - EarliestTimeForBetterEarlierReferenceTime, EarliestReferenceTIme, new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End), LastSubCalElementForEarlierReferenceTime);
                                 if (TimeLineUpdated != null)
                                 {
                                     LowestCostArrangement = TimeLineUpdated.Item2;
                                     EarliestReferenceTIme = TimeLineUpdated.Item1;
                                 }
                                

                                 foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                                 {
                                     --CompatibleWithList[eachSubCalendarEvent.ActiveDuration].Item1;
                                     PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Remove(eachSubCalendarEvent.ID);
                                     string SubCalString = eachSubCalendarEvent.SubEvent_ID.getLevelID(0);
                                     if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                                     {
                                         CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.ID);
                                         if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                                         {
                                             CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                                         }
                                     }
                                     if (PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Count < 1)
                                     {
                                         PossibleEntries_Cpy.Remove(eachSubCalendarEvent.ActiveDuration);
                                     }
                                 }


                                 LowestCostArrangement = CompleteArranegement.Concat(LowestCostArrangement).ToList();
                                 * *\//eliminating excess comments
                             */
                                LowestCostArrangement = PlaceSubCalEventInLowestCostPosition(FreeBoundary, restrictedSnugFitAvailable[i].Item2, CompleteArranegement);
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
                            PertinentFreeSpotEnd = RestrictedStopper;//as a result of the comment sections with the string "elemenating excess comments" 
                            breakForLoop = true;
                        }
                        PertinentFreeSpot = new TimeLine(PertinentFreeSpotStart, PertinentFreeSpotEnd);

                        BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                        BorderElementEnd = restrictedSnugFitAvailable[i].Item2;//uses restricted value as boundary element

                        LowestCostArrangement = OptimizeArrangeOfSubCalEvent(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, Occupancy);
                        DateTime LatestDaterforEarlierReferenceTime = PertinentFreeSpot.Start;
                        LastSubCalElementForEarlierReferenceTime = ((CompleteArranegement.Count < 1) || (CompleteArranegement == null) ? null : CompleteArranegement[CompleteArranegement.Count - 1]);//updates the last element as either null or the last element in the current Complete arrangement
                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();
                        if (LowestCostArrangement.Count > 0)
                        {
                            if (!(LowestCostArrangement[0].getCalendarEventRange.Start == PertinentFreeSpot.Start))//Pin SubEvents To Start
                            {//if the first element is not a partial Sub Cal Event element
                                FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);

                            }
                            else
                            {
                                //FreeSpotUpdated = new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                                FreeSpotUpdated = PertinentFreeSpot.CreateCopy();
                                Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot);
                            }
                            EarliestReferenceTIme = PertinentFreeSpot.End;

                            ///Comeback to this
                            ///
                            SubCalendarEvent LastSubCalEvent = LowestCostArrangement[LowestCostArrangement.Count - 1];
                            LatestDaterforEarlierReferenceTime = LastSubCalEvent.End;
                            LastSubCalElementForEarlierReferenceTime = LastSubCalEvent;
                        }


                        TimeLineUpdated = null;

                        if (restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.Start > LatestDaterforEarlierReferenceTime)
                        {
                            TimeLineUpdated = ObtainBetterEarlierReferenceTime(LowestCostArrangement, CalendarIDAndNonPartialSubCalEvents, RestrictedStopper - LatestDaterforEarlierReferenceTime, EarliestReferenceTIme, new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End), LastSubCalElementForEarlierReferenceTime);
                            //errorline

                            if (TimeLineUpdated != null)
                            {
                                LowestCostArrangement = TimeLineUpdated.Item2;
                                EarliestReferenceTIme = TimeLineUpdated.Item1;
                            }
                        }


                        foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                        {
                            --CompatibleWithList[eachSubCalendarEvent.ActiveDuration].Item1;
                            PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Remove(eachSubCalendarEvent.ID);
                            string SubCalString = eachSubCalendarEvent.SubEvent_ID.getLevelID(0);
                            if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                            {
                                CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.ID);
                                if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                                {
                                    CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                                }
                            }
                            if (PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Count < 1)
                            {
                                PossibleEntries_Cpy.Remove(eachSubCalendarEvent.ActiveDuration);
                            }
                        }
                        CompleteArranegement.AddRange(LowestCostArrangement);


                        DateTimeCounter = 0;
                        for (; DateTimeCounter < FrontPartials_Dict.Keys.Count; DateTimeCounter++)//updates CalendarIDAndNonPartialSubCalEvents if frontpartial Startime has been passed. Alls updates FrontPartials_Dict
                        {
                            DateTime eachDateTIme = FrontPartials_Dict.Keys.ToList()[DateTimeCounter];
                            if (EarliestReferenceTIme >= eachDateTIme)
                            {
                                List<mTuple<bool, SubCalendarEvent>> mTUpleSubCalEvents = FrontPartials_Dict[eachDateTIme];
                                foreach (mTuple<bool, SubCalendarEvent> eachmTUple in mTUpleSubCalEvents)
                                {

                                    string CalLevel0ID = eachmTUple.Item2.SubEvent_ID.getLevelID(0);
                                    if (!CompleteArranegement.Contains(eachmTUple.Item2))
                                    {
                                        if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(CalLevel0ID))
                                        {
                                            CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(eachmTUple.Item2.ID, eachmTUple.Item2);
                                        }
                                        else
                                        {
                                            //CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new List<SubCalendarEvent>() { KeyValuePair0.Value.Item2 });
                                            CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new Dictionary<string, SubCalendarEvent>());
                                            CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(eachmTUple.Item2.ID, eachmTUple.Item2);
                                        }
                                    }
                                }
                                FrontPartials_Dict.Remove(eachDateTIme);
                            }
                        }

                        //ListOfFrontPartialsStartTime = FrontPartials_Dict.Keys.ToList();


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

                    BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                    BorderElementEnd = restrictedSnugFitAvailable[i].Item2;//uses restricted value as boundary element

                    LowestCostArrangement = OptimizeArrangeOfSubCalEvent(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, Occupancy);

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
                        EarliestReferenceTIme = LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                    }

                    foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                    {
                        --CompatibleWithList[eachSubCalendarEvent.ActiveDuration].Item1;
                        PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Remove(eachSubCalendarEvent.ID);
                        string SubCalString = eachSubCalendarEvent.SubEvent_ID.getLevelID(0);
                        if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                        {
                            CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.ID);
                            if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                            {
                                CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                            }
                        }
                        if (PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Count < 1)
                        {
                            PossibleEntries_Cpy.Remove(eachSubCalendarEvent.ActiveDuration);
                        }
                    }


                    List<SubCalendarEvent> AdditionalCOstArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
                    DateTime RelativeEndTime;
                    if (j < restrictedSnugFitAvailable.Count)
                    {
                        //DateTime StartDateTimeAfterFitting = PertinentFreeSpot.End;
                        DateTime StartDateTimeAfterFitting = EarliestReferenceTIme;//this is the barring end time of the preceding boundary search. Earliest would have been updated if there was some event detected.


                        RelativeEndTime = restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End > restrictedSnugFitAvailable[j].Item2.Start ? restrictedSnugFitAvailable[j].Item2.Start : restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End;

                        RelativeEndTime -= restrictedSnugFitAvailable[i].Item2.ActiveDuration;
                        TimeLine CurrentlyFittedTimeLine = new TimeLine(StartDateTimeAfterFitting, RelativeEndTime);

                        BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                        BorderElementEnd = restrictedSnugFitAvailable[i].Item2;//uses restricted value as boundary element

                        AdditionalCOstArrangement = OptimizeArrangeOfSubCalEvent(CurrentlyFittedTimeLine, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, Occupancy);
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
                                --CompatibleWithList[eachSubCalendarEvent.ActiveDuration].Item1;
                                PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Remove(eachSubCalendarEvent.ID);
                                string SubCalString = eachSubCalendarEvent.SubEvent_ID.getLevelID(0);
                                if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                                {
                                    CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.ID);
                                    if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                                    {
                                        CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                                    }
                                }
                                if (PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Count < 1)
                                {
                                    PossibleEntries_Cpy.Remove(eachSubCalendarEvent.ActiveDuration);
                                }
                            }


                            RelativeEndTime = AdditionalCOstArrangement[AdditionalCOstArrangement.Count - 1].End;
                            RelativeEndTime += restrictedSnugFitAvailable[i].Item2.ActiveDuration; ;
                            CurrentlyFittedTimeLine = new TimeLine(FreeSpotUpdated.Start, RelativeEndTime);
                            //AdditionalCOstArrangement = PlaceSubCalEventInLowestCostPosition(CurrentlyFittedTimeLine, restrictedSnugFitAvailable[i].Item2, AdditionalCOstArrangement);
                        }
                        else
                        {//if there is no other Restricted in list
                            RelativeEndTime += restrictedSnugFitAvailable[i].Item2.ActiveDuration;
                            CurrentlyFittedTimeLine = new TimeLine(CurrentlyFittedTimeLine.Start, RelativeEndTime);
                            //AdditionalCOstArrangement = PlaceSubCalEventInLowestCostPosition(CurrentlyFittedTimeLine, restrictedSnugFitAvailable[i].Item2, AdditionalCOstArrangement);
                        }
                    }
                    else
                    {
                        RelativeEndTime = restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End > FreeBoundary.End ? FreeBoundary.End : restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End;
                        TimeLine CurrentlyFittedTimeLine = new TimeLine(EarliestReferenceTIme, RelativeEndTime);
                        //AdditionalCOstArrangement = PlaceSubCalEventInLowestCostPosition(CurrentlyFittedTimeLine, restrictedSnugFitAvailable[i].Item2, AdditionalCOstArrangement);
                    }

                    CompleteArranegement.AddRange(LowestCostArrangement);
                    CompleteArranegement.AddRange(AdditionalCOstArrangement);
                    TimeLine encasingTimeLine = new TimeLine(FreeBoundary.Start, RelativeEndTime);
                    CompleteArranegement = PlaceSubCalEventInLowestCostPosition(encasingTimeLine, restrictedSnugFitAvailable[i].Item2, CompleteArranegement);
                    Utility.PinSubEventsToStart(CompleteArranegement, FreeBoundary);
                    if (CompleteArranegement.Count > 0)
                    {
                        EarliestReferenceTIme = CompleteArranegement[CompleteArranegement.Count - 1].End;
                    }
                }
            }


            { //Handles THe Last Free Space outside of rigids
                TimeLine FreeSpotOutSideRigids = new TimeLine(EarliestReferenceTIme, FreeBoundary.End);
                TimeLine PertinentFreeSpot = new TimeLine(EarliestReferenceTIme, FreeBoundary.End); ;
                TimeLine FreeSpotUpdated;
                List<SubCalendarEvent> LowestCostArrangement;
                if (ListOfFrontPartialsStartTime.Count > 0)
                {
                    for (FrontPartialCounter = 0; FrontPartialCounter < ListOfFrontPartialsStartTime.Count; FrontPartialCounter++)
                    {
                        DateTime PertinentFreeSpotStart = EarliestReferenceTIme;
                        DateTime PertinentFreeSpotEnd;
                        PertinentFreeSpotEnd = ListOfFrontPartialsStartTime[FrontPartialCounter];
                        //FrontPartials_Dict.Remove(ListOfFrontPartialsStartTime[FrontPartialCounter]);
                        ListOfFrontPartialsStartTime.RemoveAt(FrontPartialCounter);
                        --FrontPartialCounter;
                        PertinentFreeSpot = new TimeLine(PertinentFreeSpotStart, PertinentFreeSpotEnd);
                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();

                        BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                        BorderElementEnd = null;

                        LowestCostArrangement = OptimizeArrangeOfSubCalEvent(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, Occupancy);
                        DateTime LatestTimeForBetterEarlierReferenceTime = PertinentFreeSpot.Start;
                        LastSubCalElementForEarlierReferenceTime = ((CompleteArranegement.Count < 1) || (CompleteArranegement == null) ? null : CompleteArranegement[CompleteArranegement.Count - 1]);
                        if (LowestCostArrangement.Count > 0)
                        {
                            if ((LowestCostArrangement[0].getCalendarEventRange.Start != PertinentFreeSpot.Start))//Pin SubEvents To Start
                            {//if the first element is not a partial Sub Cal Event element
                                FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);

                            }
                            else
                            {
                                FreeSpotUpdated = PertinentFreeSpot.CreateCopy();// new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                            }
                            EarliestReferenceTIme = PertinentFreeSpot.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                            SubCalendarEvent LastSubCalEvent = LowestCostArrangement[LowestCostArrangement.Count - 1];
                            LatestTimeForBetterEarlierReferenceTime = LastSubCalEvent.End;
                            LastSubCalElementForEarlierReferenceTime = LastSubCalEvent;
                            /*
                            Dictionary<string, double> AllValidNodes = CalendarEvent.DistanceToAllNodes(LastSubCalEvent.SubEvent_ID.getLevelID(0));
                            SubCalendarEvent AppendableEVent;
                            foreach (string eachstring in AllValidNodes.Keys)
                            {
                                if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(eachstring))
                                {
                                    AppendableEVent = CalendarIDAndNonPartialSubCalEvents[eachstring].ToList()[0].Value;//Assumes Theres Always an element
                                    

                                    if ((AppendableEVent.ActiveDuration <= (FreeBoundary.End - LastSubCalEvent.End)) && (!LowestCostArrangement.Contains(AppendableEVent)))
                                    {
                                        LowestCostArrangement.Add(AppendableEVent);
                                        CalendarIDAndNonPartialSubCalEvents[eachstring].Remove(AppendableEVent.ID);
                                        if (CalendarIDAndNonPartialSubCalEvents[eachstring].Count < 1)//checks if List is empty. Deletes keyValuepair if list is empty
                                        {
                                            CalendarIDAndNonPartialSubCalEvents.Remove(eachstring);
                                        }
                                        FreeSpotUpdated = new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End);
                                        Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                                        EarliestReferenceTIme = AppendableEVent.End;
                                        break;
                                    }
                                }
                            }*/
                        }


                        TimeLineUpdated = null;
                        TimeLineUpdated = ObtainBetterEarlierReferenceTime(LowestCostArrangement, CalendarIDAndNonPartialSubCalEvents, FreeBoundary.End - LatestTimeForBetterEarlierReferenceTime, EarliestReferenceTIme, new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End), LastSubCalElementForEarlierReferenceTime);
                        if (TimeLineUpdated != null)
                        {
                            LowestCostArrangement = TimeLineUpdated.Item2;
                            EarliestReferenceTIme = TimeLineUpdated.Item1;
                        }

                        foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                        {
                            --CompatibleWithList[eachSubCalendarEvent.ActiveDuration].Item1;
                            PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Remove(eachSubCalendarEvent.ID);
                            string SubCalString = eachSubCalendarEvent.SubEvent_ID.getLevelID(0);
                            if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                            {
                                CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.ID);
                                if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                                {
                                    CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                                }
                            }
                            if (PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Count < 1)
                            {
                                PossibleEntries_Cpy.Remove(eachSubCalendarEvent.ActiveDuration);
                            }
                        }
                        CompleteArranegement.AddRange(LowestCostArrangement);
                    }
                }


                DateTime ReferenceEndTime = FreeBoundary.End;
                PertinentFreeSpot = new TimeLine(EarliestReferenceTIme, ReferenceEndTime);
                /*LowestCostArrangement = OptimizeArrangeOfSubCalEvent(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(null, null), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy);

                if (LowestCostArrangement.Count > 0)
                {
                    if (!(LowestCostArrangement[0].getCalendarEventRange.Start == PertinentFreeSpot.Start))//Pin SubEvents To Start
                    {//if the first element is not a partial Sub Cal Event element
                        FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                        Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                    }
                    else
                    {
                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();// new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                        Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                    }
                    EarliestReferenceTIme = FreeSpotUpdated.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                }*/
                BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                BorderElementEnd = null;

                LowestCostArrangement = OptimizeArrangeOfSubCalEvent(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, Occupancy);
                LastSubCalElementForEarlierReferenceTime = ((CompleteArranegement.Count < 1) || (CompleteArranegement == null) ? null : CompleteArranegement[CompleteArranegement.Count - 1]);
                DateTime LimitForBetterEarlierReferencTime = EarliestReferenceTIme;
                FreeSpotUpdated = PertinentFreeSpot.CreateCopy();
                if (LowestCostArrangement.Count > 0)
                {
                    if ((LowestCostArrangement[0].getCalendarEventRange.Start != PertinentFreeSpot.Start))//Pin SubEvents To Start
                    {//if the first element is not a partial Sub Cal Event element
                        FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                        Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);

                    }
                    else
                    {
                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();// new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                        Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot);
                    }
                    EarliestReferenceTIme = PertinentFreeSpot.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                    SubCalendarEvent LastSubCalEvent = LowestCostArrangement[LowestCostArrangement.Count - 1];
                    LimitForBetterEarlierReferencTime = LastSubCalEvent.End;
                    LastSubCalElementForEarlierReferenceTime = LastSubCalEvent;



                    /*
                    
                    
                    Dictionary<string, double> AllValidNodes = CalendarEvent.DistanceToAllNodes(LastSubCalEvent.SubEvent_ID.getLevelID(0));
                    SubCalendarEvent AppendableEVent;
                    foreach (string eachstring in AllValidNodes.Keys)
                    {
                        if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(eachstring))
                        {
                            AppendableEVent = CalendarIDAndNonPartialSubCalEvents[eachstring].ToList()[0].Value;//Assumes Theres Always an element

                            if ((AppendableEVent.ActiveDuration <= (FreeBoundary.End - LastSubCalEvent.End)) && (!LowestCostArrangement.Contains(AppendableEVent)))
                            {
                                LowestCostArrangement.Add(AppendableEVent);
                                CalendarIDAndNonPartialSubCalEvents[eachstring].Remove(AppendableEVent.ID);
                                if (CalendarIDAndNonPartialSubCalEvents[eachstring].Count < 1)//checks if List is empty. Deletes keyValuepair if list is empty
                                {
                                    CalendarIDAndNonPartialSubCalEvents.Remove(eachstring);
                                }
                                FreeSpotUpdated = new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End);
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                                EarliestReferenceTIme = AppendableEVent.End;
                                break;
                            }
                        }
                    }*/

                }
                TimeLineUpdated = null;
                TimeLineUpdated = ObtainBetterEarlierReferenceTime(LowestCostArrangement, CalendarIDAndNonPartialSubCalEvents, FreeBoundary.End - LimitForBetterEarlierReferencTime, EarliestReferenceTIme, new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End), LastSubCalElementForEarlierReferenceTime);
                if (TimeLineUpdated != null)
                {
                    LowestCostArrangement = TimeLineUpdated.Item2;
                    EarliestReferenceTIme = TimeLineUpdated.Item1;
                }


                foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                {
                    --CompatibleWithList[eachSubCalendarEvent.ActiveDuration].Item1;
                    PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Remove(eachSubCalendarEvent.ID);
                    string SubCalString = eachSubCalendarEvent.SubEvent_ID.getLevelID(0);
                    if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                    {
                        CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.ID);
                        if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                        {
                            CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                        }
                    }

                    if (PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Count < 1)
                    {
                        PossibleEntries_Cpy.Remove(eachSubCalendarEvent.ActiveDuration);
                    }
                }
                CompleteArranegement.AddRange(LowestCostArrangement);

            }

            if(CompleteArranegement.Count>0)
            { 
                Utility.PinSubEventsToStart(CompleteArranegement,FreeBoundary);
                TimeLine newFreeBoundary= new TimeLine( CompleteArranegement.Last().End,FreeBoundary.End);
                List<mTuple<bool, SubCalendarEvent>> newRestricted=new List<mTuple<bool,SubCalendarEvent>>();
                
                Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> newCompatibleWithList =new Dictionary<TimeSpan,mTuple<int,TimeSpanWithStringID>>();

                IEnumerable<SubCalendarEvent> PossibleEntriesWhatsLeft= PossibleEntries_Cpy.Select(obj=>obj.Value).SelectMany(obj=>obj.Values).Select(obj=>obj.Item2).Where(obj=>obj.canExistWithinTimeLine(newFreeBoundary));
                if(PossibleEntriesWhatsLeft.Count()>0)
                {
                    CompleteArranegement.AddRange(stitchUnRestrictedSubCalendarEvent(newFreeBoundary, newRestricted, PossibleEntries_Cpy, newCompatibleWithList, Occupancy).Select(obj=>obj.Item2));
                }
            }

            List<mTuple<bool, SubCalendarEvent>> retValue = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            foreach (SubCalendarEvent eachSubCalendarEvent in CompleteArranegement)
            {
                PossibleEntries[eachSubCalendarEvent.ActiveDuration][eachSubCalendarEvent.ID].Item1 = true;
                retValue.Add(PossibleEntries[eachSubCalendarEvent.ActiveDuration][eachSubCalendarEvent.ID]);
            }

            //List<List<SubCalendarEvent>> unrestrictedValidCombinations = generateCombinationForDifferentEntries(CompatibleWithList, PossibleEntries);
            retValue = reAlignSubCalEvents(FreeBoundary, retValue);
            if (TotalEventsForThisTImeLine != retValue.Count)
            {
                ;
            }
            return retValue;
        }






        List<SubCalendarEvent> OptimizeArrangeOfSubCalEvent(TimeLine PertinentFreeSpot, Tuple<SubCalendarEvent, SubCalendarEvent> BoundaryCalendarEvent, List<mTuple<int, TimeSpanWithStringID>> CompatibleWithList, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries_Cpy, double occupancy = 0, bool Aggressive = true)
        {
            CompatibleWithList.Clear();
            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleSubCalEvents = removeSubCalEventsThatCantWorkWithTimeLine(PertinentFreeSpot, PossibleEntries_Cpy, true);
            Dictionary<DateTime, Dictionary<TimeSpan, int>> DeadLineTODuration = new Dictionary<DateTime, Dictionary<TimeSpan, int>>();
            foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleSubCalEvents)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                CompatibleWithList.Add(new mTuple<int, TimeSpanWithStringID>(eachKeyValuePair.Value.Count, new TimeSpanWithStringID(eachKeyValuePair.Value.ToList()[0].Value.Item2.ActiveDuration, eachKeyValuePair.Key.Ticks.ToString())));

                foreach (SubCalendarEvent eachSubcalevent in eachKeyValuePair.Value.Values.Select(obj => obj.Item2))
                {
                    DateTime endTime = eachSubcalevent.getCalendarEventRange.End;
                    if (DeadLineTODuration.ContainsKey(endTime))
                    {
                        if (DeadLineTODuration[endTime].ContainsKey(eachSubcalevent.ActiveDuration))
                        {
                            ++DeadLineTODuration[endTime][eachSubcalevent.ActiveDuration];
                        }
                        else
                        {
                            DeadLineTODuration[endTime].Add(eachSubcalevent.ActiveDuration, 1);
                        }
                    }
                    else
                    {
                        DeadLineTODuration.Add(endTime, new Dictionary<TimeSpan, int>());
                        DeadLineTODuration[endTime].Add(eachSubcalevent.ActiveDuration, 1);

                    }

                    DeadLineTODuration[endTime].OrderBy(obj => obj);
                }

            }


            SnugArray BestFit_beforeBreak = new SnugArray(CompatibleWithList, PertinentFreeSpot.TimelineSpan);
            TimeSpan AverageTimeSpan = new TimeSpan((long)(occupancy * (double)PertinentFreeSpot.TimelineSpan.Ticks));
            List<SubCalendarEvent> LowestCostArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AllPossibleBestFit_beforeBreak = BestFit_beforeBreak.MySnugPossibleEntries;
            AllPossibleBestFit_beforeBreak = SnugArray.SortListSnugPossibilities_basedOnTimeSpan(AllPossibleBestFit_beforeBreak, new TimeSpanWithStringID(AverageTimeSpan, AverageTimeSpan.Ticks.ToString()));
            AllPossibleBestFit_beforeBreak.Reverse();
            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> var3_beforeBreak = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
            if (AllPossibleBestFit_beforeBreak.Count > 0)
            {
                var3_beforeBreak.Add(AllPossibleBestFit_beforeBreak);
                if (AllPossibleBestFit_beforeBreak.Count > 1)
                {
                    ;
                }
                //List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AveragedBestFit = getAveragedOutTIimeLine(var3_beforeBreak, 0);
                List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AveragedBestFit = OptimizeForDeadLine(DeadLineTODuration, PertinentFreeSpot.TimelineSpan);
                Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> removedImpossibleValue = removeSubCalEventsThatCantWorkWithTimeLine(PertinentFreeSpot, PossibleEntries_Cpy);
                List<List<SubCalendarEvent>> PossibleSubCaleventsCobination = generateCombinationForDifferentEntries(AveragedBestFit[0], removedImpossibleValue);

                if (Aggressive)
                {
                    if (PossibleSubCaleventsCobination.Count > 1)
                    {
                        PossibleSubCaleventsCobination.OrderByDescending(obj => obj.Count);

                        PossibleSubCaleventsCobination = PossibleSubCaleventsCobination.GetRange(0, 1);
                    }
                }

                if (PossibleSubCaleventsCobination.Count>1)
                {
                    ;

                }
                PossibleSubCaleventsCobination = Utility.RandomizeIEnumerable(PossibleSubCaleventsCobination);
                LowestCostArrangement = getArrangementWithLowestDistanceCost(PossibleSubCaleventsCobination, BoundaryCalendarEvent);
                //TimeLine FreeSpotUpdated;
            }

            return LowestCostArrangement;

        }


        List<SubCalendarEvent> OptimizeArrangeOfSubCalEvent_NonAggressive(TimeLine PertinentFreeSpot, Tuple<SubCalendarEvent, SubCalendarEvent> BoundaryCalendarEvent, List<mTuple<int, TimeSpanWithStringID>> CompatibleWithList, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries_Cpy, List<mTuple<bool, SubCalendarEvent>> restOfRestrictedElements, double occupancy = 0)
        {
            CompatibleWithList.Clear();

            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in restOfRestrictedElements)
            {
                if (PossibleEntries_Cpy.ContainsKey(eachmTuple.Item2.RangeSpan))
                {
                    PossibleEntries_Cpy[eachmTuple.Item2.RangeSpan].Add(eachmTuple.Item2.ID, eachmTuple);
                }
                else
                {
                    PossibleEntries_Cpy.Add(eachmTuple.Item2.RangeSpan, new Dictionary<string, mTuple<bool, SubCalendarEvent>>());
                    PossibleEntries_Cpy[eachmTuple.Item2.RangeSpan].Add(eachmTuple.Item2.ID, eachmTuple);
                }
            }


            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleSubCalEvents = removeSubCalEventsThatCantWorkWithTimeLine(PertinentFreeSpot, PossibleEntries_Cpy, true);
            foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleSubCalEvents)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                CompatibleWithList.Add(new mTuple<int, TimeSpanWithStringID>(eachKeyValuePair.Value.Count, new TimeSpanWithStringID(eachKeyValuePair.Value.ToList()[0].Value.Item2.ActiveDuration, eachKeyValuePair.Key.Ticks.ToString())));
            }


            SnugArray BestFit_beforeBreak = new SnugArray(CompatibleWithList, PertinentFreeSpot.TimelineSpan);
            TimeSpan AverageTimeSpan = new TimeSpan((long)(occupancy * (double)PertinentFreeSpot.TimelineSpan.Ticks));
            List<SubCalendarEvent> LowestCostArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AllPossibleBestFit_beforeBreak = BestFit_beforeBreak.MySnugPossibleEntries;
            AllPossibleBestFit_beforeBreak = SnugArray.SortListSnugPossibilities_basedOnTimeSpan(AllPossibleBestFit_beforeBreak, new TimeSpanWithStringID(AverageTimeSpan, AverageTimeSpan.Ticks.ToString()));
            AllPossibleBestFit_beforeBreak.Reverse();
            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> var3_beforeBreak = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
            if (AllPossibleBestFit_beforeBreak.Count > 0)
            {
                var3_beforeBreak.Add(AllPossibleBestFit_beforeBreak);
                List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AveragedBestFit = getAveragedOutTIimeLine(var3_beforeBreak, 0);
                Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> removedImpossibleValue = removeSubCalEventsThatCantWorkWithTimeLine(PertinentFreeSpot, PossibleEntries_Cpy);
                List<List<SubCalendarEvent>> PossibleSubCaleventsCobination = generateCombinationForDifferentEntriesNonAggressive(AveragedBestFit[0], removedImpossibleValue);
                //PossibleSubCaleventsCobination = Utility.RandomizeIEnumerable(PossibleSubCaleventsCobination);
                LowestCostArrangement = getArrangementWithLowestDistanceCost(PossibleSubCaleventsCobination, BoundaryCalendarEvent);
                TimeLine FreeSpotUpdated;

            }


            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in restOfRestrictedElements)
            {
                if (PossibleEntries_Cpy.ContainsKey(eachmTuple.Item2.RangeSpan))
                {
                    PossibleEntries_Cpy[eachmTuple.Item2.RangeSpan].Remove(eachmTuple.Item2.ID);
                }
                /*else
                {
                    PossibleEntries_Cpy.Add(eachmTuple.Item2.RangeSpan, new Dictionary<string, mTuple<bool, SubCalendarEvent>>());
                    PossibleEntries_Cpy[eachmTuple.Item2.RangeSpan].Add(eachmTuple.Item2.ID, eachmTuple);
                }*/
            }



            return LowestCostArrangement;

        }



        int CountMostRestrictedElementsInDict(IEnumerable<TimeSpan> AllTImeSpan, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> myDict)
        {
            int CountSoFar = 0;
            foreach (TimeSpan eachTimeSpan in AllTImeSpan)
            {
                if (myDict.ContainsKey(eachTimeSpan))
                {
                    ++CountSoFar;
                }
            }
            return CountSoFar;
        }

        List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> OptimizeForDeadLine(IEnumerable<KeyValuePair<DateTime, Dictionary<TimeSpan, int>>> DeadLinePreference, TimeSpan CurrentFreeSpace, bool Aggressive = true)
        {
            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> retValue = new List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();
            HashSet<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> qualifiesForNextStage = new HashSet<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();
            HashSet<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AggressiveSet = new HashSet<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();
            IEnumerable<KeyValuePair<DateTime, Dictionary<TimeSpan, int>>> AllBestFitOptions_IEnu = DeadLinePreference;
            IEnumerable<KeyValuePair<DateTime, Dictionary<TimeSpan, int>>> DeadLinePreference_Qualified;
            HashSet<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> forNextLevel = new HashSet<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();

            AllBestFitOptions_IEnu = AllBestFitOptions_IEnu.OrderBy(obj => obj.Key);
            //AllBestFitOptions_IEnu.Reverse();
            bool iniAggressive = Aggressive;
            foreach (KeyValuePair<DateTime, Dictionary<TimeSpan, int>> eachKeyValuePair in AllBestFitOptions_IEnu)
            {
                //IEnumerable<KeyValuePair<TimeSpan, int>> AllTimeSpan = eachKeyValuePair.Value.OrderBy(obj => obj);
                IEnumerable<mTuple<int, TimeSpanWithStringID>> AllTimeSpan = eachKeyValuePair.Value.Select(obj => new mTuple<int, TimeSpanWithStringID>(obj.Value, new TimeSpanWithStringID(obj.Key, obj.Key.Ticks.ToString())));
                SnugArray BestFit_OfDeadline = new SnugArray(AllTimeSpan.ToList(), CurrentFreeSpace);
                List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AllPossibleBestFit_beforeBreak = SnugArray.SortListSnugPossibilities_basedOnTimeSpan(BestFit_OfDeadline.MySnugPossibleEntries);

                AllPossibleBestFit_beforeBreak.Reverse();

                if (AllPossibleBestFit_beforeBreak.Count > 0)
                {
                    Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> TightestConfiguration = AllPossibleBestFit_beforeBreak[0];
                    retValue.Add(TightestConfiguration);
                    TimeSpan UsedUpSpace = SnugArray.TotalTimeSpanOfSnugPossibility(TightestConfiguration);
                    CurrentFreeSpace -= UsedUpSpace;
                    DeadLinePreference_Qualified = AllBestFitOptions_IEnu.Where(obj => obj.Value != eachKeyValuePair.Value);
                    List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> furtherCalls = OptimizeForDeadLine(DeadLinePreference_Qualified, CurrentFreeSpace, Aggressive);
                    if (furtherCalls.Count > 0)
                    {
                        foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in furtherCalls)
                        {
                            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair0 in TightestConfiguration)
                            {
                                if (eachDictionary.ContainsKey(eachKeyValuePair0.Key))
                                {
                                    eachDictionary[eachKeyValuePair0.Key].Item1 += eachKeyValuePair0.Value.Item1;
                                }
                                else
                                {
                                    eachDictionary.Add(eachKeyValuePair0.Key, eachKeyValuePair0.Value);
                                }
                            }
                        }
                    }
                    else
                    {
                        furtherCalls.Add(TightestConfiguration);
                    }

                    return furtherCalls;
                }
            }

            return retValue;
        }




        //Dictionary<string, Dictionary<string, mTuple<bool, SubCalendarEvent>>> removedImpossibleValue = removeSubCalEventsThatCantWorkWithTimeLine(PertinentFreeSpot, PossibleEntries_Cpy);
        List<mTuple<bool, SubCalendarEvent>> removeSubCalEventsThatCantWorkWithTimeLine(TimeLine PertinentFreeSpot, List<KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>> PossibleEntries, bool MustFitAnyWhere = false)
        {
            /*
             * THis funcction checks if the active duration of each subcalevent in PossibleEntries can fit within the PertinentFreeSpot. Also the it checks if PertinentFreeSpot breaks the enclosing timeLine for the currently echeckcked subcalendar event. if both conditions are satisfied the subcalevent gets inserted into the retvalue
             * MustFitAnyWhere variable checks if the subcalevent can exist any where within the PertinentFreeSpot timeline, as well as verify the steps above. Its default value is false i.e it checks if the full subcalevent can exist in part of the pertinentfreespot
             
             */

            List<mTuple<bool, SubCalendarEvent>> retValue = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();



            foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleEntries)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                Dictionary<string, mTuple<bool, SubCalendarEvent>> UpdatedEntries = new System.Collections.Generic.Dictionary<string, mTuple<bool, SubCalendarEvent>>();

                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> eachKeyValuePair0 in eachKeyValuePair.Value)
                {
                    if (MustFitAnyWhere)
                    {
                        if (eachKeyValuePair0.Value.Item2.getCalendarEventRange.IsTimeLineWithin(PertinentFreeSpot) && (eachKeyValuePair0.Value.Item2.ActiveDuration <= PertinentFreeSpot.TimelineSpan))
                        {
                            retValue.Add(eachKeyValuePair0.Value);
                        }
                    }
                    else
                    {
                        if (eachKeyValuePair0.Value.Item2.canExistWithinTimeLine(PertinentFreeSpot))
                        {
                            retValue.Add(eachKeyValuePair0.Value);
                        }
                    }
                }
            }

            return retValue;


        }



        Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> removeSubCalEventsThatCantWorkWithTimeLine(TimeLine PertinentFreeSpot, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries, bool MustFitAnyWhere = false)
        {
            /*
             * THis funcction checks if the active duration of each subcalevent in PossibleEntries can fit within the PertinentFreeSpot. Also the it checks if PertinentFreeSpot breaks the enclosing timeLine for the currently echeckcked subcalendar event. if both conditions are satisfied the subcalevent gets inserted into the retvalue
             * MustFitAnyWhere variable checks if the subcalevent can exist any where within the PertinentFreeSpot timeline, as well as verify the steps above. Its default value is false i.e it checks if the full subcalevent can exist in part of the pertinentfreespot
             
             */

            List<mTuple<bool, SubCalendarEvent>> retValueList = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> retValue = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();


            foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleEntries)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                Dictionary<string, mTuple<bool, SubCalendarEvent>> UpdatedEntries = new System.Collections.Generic.Dictionary<string, mTuple<bool, SubCalendarEvent>>();

                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> eachKeyValuePair0 in eachKeyValuePair.Value)
                {
                    if (MustFitAnyWhere)
                    {
                        if (eachKeyValuePair0.Value.Item2.getCalendarEventRange.IsTimeLineWithin(PertinentFreeSpot) && (eachKeyValuePair0.Value.Item2.ActiveDuration <= PertinentFreeSpot.TimelineSpan))
                        {
                            retValueList.Add(eachKeyValuePair0.Value);
                        }
                    }
                    else
                    {
                        if (eachKeyValuePair0.Value.Item2.canExistWithinTimeLine(PertinentFreeSpot))
                        {
                            retValueList.Add(eachKeyValuePair0.Value);
                        }
                    }
                }
            }

            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in retValueList)
            {
                if (retValue.ContainsKey(eachmTuple.Item2.ActiveDuration))
                {
                    retValue[eachmTuple.Item2.ActiveDuration].Add(eachmTuple.Item2.ID, eachmTuple);
                }
                else
                {
                    retValue.Add(eachmTuple.Item2.ActiveDuration, new Dictionary<string, mTuple<bool, SubCalendarEvent>>());
                    retValue[eachmTuple.Item2.ActiveDuration].Add(eachmTuple.Item2.ID, eachmTuple);
                }
            }


            return retValue;


        }




        Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> removeSubCalEventsThatCantWorkWithTimeLine(TimeLine PertinentFreeSpot, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries)
        {
            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> retValue = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();


            foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleEntries)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                Dictionary<string, mTuple<bool, SubCalendarEvent>> UpdatedEntries = new System.Collections.Generic.Dictionary<string, mTuple<bool, SubCalendarEvent>>();

                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> eachKeyValuePair0 in eachKeyValuePair.Value)
                {
                    if (eachKeyValuePair0.Value.Item2.canExistWithinTimeLine(PertinentFreeSpot))
                    {
                        UpdatedEntries.Add(eachKeyValuePair0.Key, eachKeyValuePair0.Value);
                    }
                }


                if (UpdatedEntries.Count > 0)
                { retValue.Add(eachKeyValuePair.Key, UpdatedEntries); }
            }


            return retValue;
        }

        List<List<mTuple<bool, SubCalendarEvent>>> ThisWillClash = new System.Collections.Generic.List<System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();

        List<mTuple<bool, SubCalendarEvent>> reAlignSubCalEvents(TimeLine BoundaryTimeLine, List<mTuple<bool, SubCalendarEvent>> ListOfEvents)
        {
            DateTime ReferenceTime = BoundaryTimeLine.Start;
            TimeLine Boundary_Cpy = BoundaryTimeLine.CreateCopy();
            List<mTuple<bool, SubCalendarEvent>> myClashers = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();


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

        List<SubCalendarEvent> PlaceSubCalEventInLowestCostPosition(TimeLine MyLimitingTimeLine, SubCalendarEvent mySubcalevent, List<SubCalendarEvent> OptimizedArrangementOfEvent)
        {

            /**Hack Solution Start, this just assumes all events are right next to each other and appends mySubcalevetn to the end. It also shifts this sub cal event to represent this shift **/
            DateTime RelativeStartTime = MyLimitingTimeLine.Start + Utility.SumOfActiveDuration(OptimizedArrangementOfEvent);
            TimeLine encasingTimeLine = MyLimitingTimeLine.InterferringTimeLine(mySubcalevent.getCalendarEventRange);

            IEnumerable<SubCalendarEvent> Interferringevents = getInterferringSubEvents(encasingTimeLine, OptimizedArrangementOfEvent);
            List<SubCalendarEvent> ListSofar = Interferringevents.ToList();

            List<List<SubCalendarEvent>> AllPertinentList = new List<List<SubCalendarEvent>>();
            List<SubCalendarEvent> retValue = OptimizedArrangementOfEvent.ToList();
            retValue.Add(mySubcalevent);

#if EnableRestrictedLocationOptimization
            int NumberOfLists = ListSofar.Count;
            int LastInt = 0;
            List<SubCalendarEvent> currList;

            if (ListSofar.Count > 0)
            {
                ListSofar.RemoveAt(0);
                IEnumerable<int> AllValidIndexes = ListSofar.Select(obj => OptimizedArrangementOfEvent.IndexOf(obj));
                foreach (int eachInt in AllValidIndexes)
                {
                    currList = OptimizedArrangementOfEvent.ToList();
                    currList.Insert(eachInt, mySubcalevent);
                    AllPertinentList.Add(currList);
                    LastInt = eachInt;
                }

                currList = OptimizedArrangementOfEvent.ToList();
                currList.Insert(LastInt + 1, mySubcalevent);
                AllPertinentList.Add(currList);
                List<List<SubCalendarEvent>> ListThatWorks = new List<List<SubCalendarEvent>>();

                foreach (List<SubCalendarEvent> eachList in AllPertinentList)
                {
                    if (Utility.PinSubEventsToStart(eachList, MyLimitingTimeLine))
                    {
#if createCopyOfImplementation
                        List<SubCalendarEvent> eachListCpy = eachList.Select(obj => obj.createCopy()).ToList();
                        ListThatWorks.Add(eachListCpy);
#else
                        ListThatWorks.Add(eachList);
#endif
                    }
                }

                double lowestSofar = double.MaxValue;
            
                foreach (List<SubCalendarEvent> eachList in ListThatWorks)
                {
                    double currDistance = Utility.calculateDistance(eachList.ToList(), CalendarEvent.DistanceMatrix);
                    if (currDistance < lowestSofar)
                    { 
                        lowestSofar=currDistance;
#if createCopyOfImplementation    
                        retValue = eachList.SelectMany(obj => retValue.Where(obj0 => obj0.ID == obj.ID)).ToList(); ;
#else
                        retValue =eachList;
#endif

                    }
                }
            }

            Utility.PinSubEventsToStart(retValue, MyLimitingTimeLine);

#endif

            return retValue;
        }

        double calculateMyDistance(List<SubCalendarEvent> AllSubCalEvents)
        {
            double currDistance = Utility.calculateDistance(AllSubCalEvents.ToList(), CalendarEvent.DistanceMatrix);
            return currDistance;
        }

        List<SubCalendarEvent> getArrangementWithLowestDistanceCost(List<List<SubCalendarEvent>> viableCombinations, Tuple<SubCalendarEvent, SubCalendarEvent> BoundinngSubCaEvents)
        {
            double LowestCost = double.PositiveInfinity;
            List<SubCalendarEvent> retValue = new System.Collections.Generic.List<SubCalendarEvent>();
            Tuple<ICollection<SubCalendarEvent>, double> OptimizedArrangement;


            if (viableCombinations.Count > 0)
            {
                OptimizedArrangement = new Tuple<ICollection<SubCalendarEvent>, double>(viableCombinations[0], Utility.calculateDistance(viableCombinations[0], CalendarEvent.DistanceMatrix));
                retValue = OptimizedArrangement.Item1.ToList();
            }



            foreach (List<SubCalendarEvent> eachList in viableCombinations)
            {

                if ((eachList.Count < 1))
                {
                    List<Location> AllLocations = new System.Collections.Generic.List<Location>();
                    List<SubCalendarEvent> MyList = eachList.ToList();
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
                    List<SubCalendarEvent> MyList = eachList.ToList();
                    int beginning = 0;
                    int End = 0;
                    int BeginningAndEnd = 0;
                    if (BoundinngSubCaEvents.Item1 != null)
                    {
                        MyList.Insert(0, BoundinngSubCaEvents.Item1); beginning = 1;
                    }

                    if (BoundinngSubCaEvents.Item2 != null)
                    {
                        MyList.Add(BoundinngSubCaEvents.Item2);
                        End = 2;
                    }

                    BeginningAndEnd = beginning + End;
                    OptimizedArrangement = DistanceSolver.Run(MyList, BeginningAndEnd);
                    OptimizedArrangement.Item1.Remove(BoundinngSubCaEvents.Item2);
                    OptimizedArrangement.Item1.Remove(BoundinngSubCaEvents.Item1);
                    Dictionary<string, SubCalendarEvent> TestDict = new Dictionary<string, SubCalendarEvent>();
                    foreach (SubCalendarEvent eachSubCalendarEvent in OptimizedArrangement.Item1)
                    {
                        TestDict.Add(eachSubCalendarEvent.ID, eachSubCalendarEvent);
                    }

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

        Tuple<List<SubCalendarEvent>, double> rearrangeForOptimizedLocationOptimizedLocation(List<SubCalendarEvent> ListOfLocations)
        {
            Tuple<List<SubCalendarEvent>, double> retValue = new Tuple<System.Collections.Generic.List<SubCalendarEvent>, double>(ListOfLocations, calculateCostOSubCalArrangement(ListOfLocations));
            return retValue;
        }

        double calculateCostOSubCalArrangement(List<SubCalendarEvent> CurrentArrangementOfSubcalevent)
        {
            int i = 0;
            double retValue = 0;



            for (; i < CurrentArrangementOfSubcalevent.Count - 1; i++)
            {
                retValue += SubCalendarEvent.CalculateDistance(CurrentArrangementOfSubcalevent[i], CurrentArrangementOfSubcalevent[i + 1]);
            }

            return retValue;
        }

        Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> useAggressivePossibilitiesEntry(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithList, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries)
        {

            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> retValue = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();
            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in CompatibleWithList)
            {
                IEnumerable<KeyValuePair<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries_IEnu = PossibleEntries[eachKeyValuePair.Key];
                PossibleEntries_IEnu = PossibleEntries_IEnu.OrderBy(obj => obj.Value.Item2.getCalendarEventRange.End);
                //                //IEnumerable<int> AllIndexesWithValidEndtime = 

                PossibleEntries_IEnu = PossibleEntries_IEnu.Where((obj, index) => index < eachKeyValuePair.Value.Item1); //keeps looping as long as index is less than eachKeyValuePair.value.item1
                retValue.Add(eachKeyValuePair.Key, PossibleEntries_IEnu.ToDictionary(obj => obj.Key, obj => obj.Value));
            }

            return retValue;
        }


        int CountCall = 0;
        List<List<SubCalendarEvent>> generateCombinationForDifferentEntries(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithList, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries, bool Aggressive = true)
        {
            /*
             * Function attempts to generate multiple combinations of compatible sub calendar event for Snug fit entry
             * CompatibleWithList is an snug fit result
             * PossibleEntries are the possible sub calendar that can be used in the combinatorial result
             */
            ++CountCall;


            if (Aggressive)
            {
                PossibleEntries = useAggressivePossibilitiesEntry(CompatibleWithList, PossibleEntries);

            }



            List<List<List<string>>> MAtrixedSet = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<string>>>();
            Dictionary<string, mTuple<int, List<SubCalendarEvent>>> var4 = new System.Collections.Generic.Dictionary<string, mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>>();
            List<List<SubCalendarEvent>> retValue = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair0 in CompatibleWithList)//loops every timespan in Snug FIt possibility
            {
                TimeSpan eachTimeSpan = eachKeyValuePair0.Key;

                Dictionary<string, mTuple<bool, SubCalendarEvent>> var1 = PossibleEntries[eachTimeSpan];
                List<List<string>> receivedValue = new System.Collections.Generic.List<System.Collections.Generic.List<string>>();
                Dictionary<string, int> var2 = new System.Collections.Generic.Dictionary<string, int>();
                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> eachKeyValuePair in var1)
                {
                    string ParentID = eachKeyValuePair.Value.Item2.SubEvent_ID.getStringIDAtLevel(0);
                    if (var2.ContainsKey(ParentID))
                    {
                        ++var2[ParentID];
                        var4[ParentID].Item2.Add(eachKeyValuePair.Value.Item2);
                    }
                    else
                    {
                        var2.Add(ParentID, 1);
                        List<SubCalendarEvent> var5 = new System.Collections.Generic.List<SubCalendarEvent>();
                        var5.Add(eachKeyValuePair.Value.Item2);
                        var4.Add(ParentID, new mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>(0, var5));
                    }
                }
                List<mTuple<string, int>> PossibleCalEvents = new System.Collections.Generic.List<mTuple<string, int>>();
                foreach (KeyValuePair<string, int> eachKeyValuePair in var2)
                {
                    PossibleCalEvents.Add(new mTuple<string, int>(eachKeyValuePair.Key, eachKeyValuePair.Value));
                }

                List<List<string>> var3 = generateCombinationForSpecficTimeSpanStringID(eachKeyValuePair0.Value.Item1, PossibleCalEvents);
                MAtrixedSet.Add(var3);
            }

            List<List<string>> serializedList = Utility.SerializeList(MAtrixedSet);
            Dictionary<TimeSpan, long> TimeSpanOfEventToTotalDeadlineDateTime = new Dictionary<TimeSpan, long>();

            foreach (List<string> eachList in serializedList)//serializedList has a list of fittable ParentIDs, the loop replaces each List of strings with List of subCalendarEvents
            {
                List<SubCalendarEvent> var6 = new System.Collections.Generic.List<SubCalendarEvent>();
                mTuple<int, List<SubCalendarEvent>> var7 = new mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>(0, new System.Collections.Generic.List<SubCalendarEvent>());
                foreach (string eachString in eachList)
                {
                    var7 = var4[eachString];
                    var6.Add(var7.Item2[var7.Item1++]);
                }
                foreach (KeyValuePair<string, mTuple<int, List<SubCalendarEvent>>> eachKeyValuePair in var4)
                {
                    eachKeyValuePair.Value.Item1 = 0;
                }

                TimeSpan TotalActiveDuration = Utility.SumOfActiveDuration(var6);


                retValue.Add(var6);
            }

            return retValue;

        }

        List<List<string>> generateCombinationForSpecficTimeSpanStringID(int Count, List<mTuple<string, int>> PossibleCalEvents)
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

                List<mTuple<string, int>> PossibleCalEvents_Param = PossibleCalEvents.ToList();
                PossibleCalEvents_Param[i] = new mTuple<string, int>(PossibleCalEvents_Param[i].Item1, PossibleCalEvents_Param[i].Item2);
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


        List<List<SubCalendarEvent>> OptimizeForLocation(IEnumerable<IEnumerable<SubCalendarEvent>> AllSubCalEvents, IEnumerable<TimeLine> AllFreeSpots)
        {
            TimeLine[] AllFreeSpots_Array = AllFreeSpots.ToArray();
            List<List<SubCalendarEvent>> retValue = AllSubCalEvents.Select(obj => obj.ToList()).ToList();
#if enableMultithreading

            //int i = 0;



            Parallel.For(0, AllFreeSpots.Count(), i =>
            {
                IEnumerable<SubCalendarEvent> eachListOfSubCalEvents = retValue[i];

                List<SubCalendarEvent> beforeValue = eachListOfSubCalEvents.ToList();
                List<SubCalendarEvent> Aftervalue = OptimizeEachFreeSpotForLocation(AllFreeSpots_Array[i], beforeValue);
                if (beforeValue.Count != Aftervalue.Count)
                {
                    Utility.PinSubEventsToStart(beforeValue, AllFreeSpots_Array[i]);

                    Aftervalue = beforeValue;
                }
                retValue[i] = Aftervalue;


                i++;
            });
            return retValue;


#else
            int i = 0;


            foreach (IEnumerable<SubCalendarEvent> eachListOfSubCalEvents in AllSubCalEvents)
            {
                List<SubCalendarEvent> beforeValue = eachListOfSubCalEvents.ToList();

                if (i == 5)
                {
                    ;
                }


                List<SubCalendarEvent> Aftervalue = OptimizeEachFreeSpotForLocation(AllFreeSpots_Array[i], beforeValue);
                if (beforeValue.Count != Aftervalue.Count)
                {

                    Utility.PinSubEventsToStart(beforeValue, AllFreeSpots_Array[i]);

                    Aftervalue = beforeValue;
                }



                retValue[i] = Aftervalue;


                i++;
            }
            return retValue;
#endif

        }


        List<SubCalendarEvent> OptimizeEachFreeSpotForLocation(TimeLine referenceTimeline, List<SubCalendarEvent> AllInsertedElements)
        {
            Dictionary<DateTime, List<SubCalendarEvent>> frontPartialsDict = new Dictionary<DateTime, List<SubCalendarEvent>>();
            Dictionary<DateTime, List<SubCalendarEvent>> endPartialDict = new Dictionary<DateTime, List<SubCalendarEvent>>();
            IEnumerable<SubCalendarEvent> StartingBeforeReferenceTimeLine = AllInsertedElements.Where(obj => obj.getCalendarEventRange.Start <= referenceTimeline.Start);
            IEnumerable<SubCalendarEvent> startingAfterReferenceTimeLineStart = AllInsertedElements.Where(obj => obj.getCalendarEventRange.Start > referenceTimeline.Start);
            IEnumerable<SubCalendarEvent> endingBefore = AllInsertedElements.Where(obj => obj.getCalendarEventRange.End <= referenceTimeline.End);



            IEnumerable<SubCalendarEvent> endingBeforeAndStartBefore = AllInsertedElements.Where(obj => ((obj.getCalendarEventRange.End <= referenceTimeline.End) && (obj.getCalendarEventRange.Start <= referenceTimeline.Start)));

            IEnumerable<SubCalendarEvent> freeRoaming = AllInsertedElements.Where(obj => ((obj.getCalendarEventRange.Start <= referenceTimeline.Start) && (obj.getCalendarEventRange.End >= referenceTimeline.End)));

            IEnumerable<SubCalendarEvent> restrictedElements = startingAfterReferenceTimeLineStart;//AllInsertedElements.Where(obj => !freeRoaming.Contains(obj));
            IEnumerable<mTuple<bool, SubCalendarEvent>> CompatibleWithTimeLine = AllInsertedElements.Select(obj => new mTuple<bool, SubCalendarEvent>(false, obj));
            List<mTuple<bool, SubCalendarEvent>> restrictedSnugFitAvailable = stitchRestrictedSubCalendarEvent(CompatibleWithTimeLine.ToList(), referenceTimeline);

            //restrictedElements.Select(obj=> new mTuple<bool,SubCalendarEvent>(false,obj)).ToList();
            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();



            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in CompatibleWithTimeLine)
            {
                TimeSpan ActiveTimeSpan = eachmTuple.Item2.ActiveDuration;
                string subcalStringID = eachmTuple.Item2.ID;

                if (PossibleEntries.ContainsKey(ActiveTimeSpan))
                {
                    PossibleEntries[ActiveTimeSpan].Add(subcalStringID, new mTuple<bool, SubCalendarEvent>(true, eachmTuple.Item2));
                }
                else
                {
                    PossibleEntries.Add(ActiveTimeSpan, new Dictionary<string, mTuple<bool, SubCalendarEvent>>());
                    PossibleEntries[ActiveTimeSpan].Add(subcalStringID, new mTuple<bool, SubCalendarEvent>(true, eachmTuple.Item2));
                }
            }

            List<SubCalendarEvent> retvalue = stitchUnRestrictedSubCalendarEvent_NonAggressive(referenceTimeline, restrictedSnugFitAvailable, PossibleEntries, new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>(), 1).Select(obj => obj.Item2).ToList();

            return retvalue;
        }



        List<mTuple<bool, SubCalendarEvent>> stitchUnRestrictedSubCalendarEvent_NonAggressive(TimeLine FreeBoundary, List<mTuple<bool, SubCalendarEvent>> restrictedSnugFitAvailable, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithList, double Occupancy)
        {
            TimeLine[] AllFreeSpots = FreeBoundary.getAllFreeSlots();
            int TotalEventsForThisTImeLine = 0;

            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in CompatibleWithList)
            {
                TotalEventsForThisTImeLine += eachKeyValuePair.Value.Item1;
            }

            CompatibleWithList.Clear();


            DateTime EarliestReferenceTIme = FreeBoundary.Start;
            List<mTuple<bool, SubCalendarEvent>> FrontPartials = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            List<mTuple<bool, SubCalendarEvent>> EndPartials = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            Dictionary<DateTime, List<mTuple<bool, SubCalendarEvent>>> FrontPartials_Dict = new System.Collections.Generic.Dictionary<DateTime, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();
            Dictionary<DateTime, List<mTuple<bool, SubCalendarEvent>>> EndPartials_Dict = new System.Collections.Generic.Dictionary<DateTime, System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>>();
            Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries_Cpy = new Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>>();
            Dictionary<string, Dictionary<string, SubCalendarEvent>> CalendarIDAndNonPartialSubCalEvents = new Dictionary<string, Dictionary<string, SubCalendarEvent>>();//List of non partials for current Reference StartTime To End of FreeBoundary. Its gets updated with Partials once the earliest reference time passes the partial event start time

            foreach (KeyValuePair<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> eachKeyValuePair in PossibleEntries)//populates PossibleEntries_Cpy. I need a copy to maintain all references to PossibleEntries
            {
                Dictionary<string, mTuple<bool, SubCalendarEvent>> NewDictEntry = new Dictionary<string, mTuple<bool, SubCalendarEvent>>();

                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> KeyValuePair0 in eachKeyValuePair.Value)
                {
                    mTuple<bool, SubCalendarEvent> MyEvent = KeyValuePair0.Value;

                    if (MyEvent.Item2.ID == "469_471")
                    {
                        ;
                    }

                    bool isInrestrictedSnugFitAvailable = false;
                    if (CompatibleWithList.ContainsKey(eachKeyValuePair.Key))
                    {
                        ++CompatibleWithList[eachKeyValuePair.Key].Item1;
                    }
                    else
                    {
                        CompatibleWithList.Add(eachKeyValuePair.Key, new mTuple<int, TimeSpanWithStringID>(1, new TimeSpanWithStringID(KeyValuePair0.Value.Item2.ActiveDuration, KeyValuePair0.Value.Item2.ActiveDuration.Ticks.ToString())));
                    }

                    foreach (mTuple<bool, SubCalendarEvent> eachMtuple in restrictedSnugFitAvailable)//checks if event is in restricted list
                    {
                        if (eachMtuple.Item2.ID == MyEvent.Item2.ID)
                        {
                            isInrestrictedSnugFitAvailable = true;
                            break;
                        }

                    }


                    if (!isInrestrictedSnugFitAvailable)//stops restricted elements from being used in caslculation
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
                            else
                            {
                                string CalLevel0ID = KeyValuePair0.Value.Item2.SubEvent_ID.getLevelID(0);
                                if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(CalLevel0ID))
                                {
                                    CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(KeyValuePair0.Value.Item2.ID, KeyValuePair0.Value.Item2);
                                }
                                else
                                {
                                    //CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new List<SubCalendarEvent>() { KeyValuePair0.Value.Item2 });
                                    CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new Dictionary<string, SubCalendarEvent>());
                                    CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(KeyValuePair0.Value.Item2.ID, KeyValuePair0.Value.Item2);
                                }

                            }
                        }
                    }
                }
                if (NewDictEntry.Count > 0)
                { PossibleEntries_Cpy.Add(eachKeyValuePair.Key, NewDictEntry); }

            }

            FrontPartials = FrontPartials.OrderBy(obj => obj.Item2.getCalendarEventRange.Start).ToList();
            EndPartials = EndPartials.OrderBy(obj => obj.Item2.getCalendarEventRange.End).ToList();

            foreach (mTuple<bool, SubCalendarEvent> eachmTuple in FrontPartials)//populates FrontPartials_Dict in ordered manner since FrontPartials is ordered
            {
                if (FrontPartials_Dict.ContainsKey(eachmTuple.Item2.getCalendarEventRange.Start))
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

            List<mTuple<bool, SubCalendarEvent>> restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
            IEnumerable<SubCalendarEvent> selectedRestrictedElements;
            List<SubCalendarEvent> CompleteArranegement = new System.Collections.Generic.List<SubCalendarEvent>();
            int StartingReferneceIndex = 0;


            /*foreach (mTuple<bool, SubCalendarEvent> eachmTuple in restrictedSnugFitAvailable)//removes the restricted from CompatibleWithList
            {
                --CompatibleWithList[eachmTuple.Item2.ActiveDuration.Ticks.ToString()].Item1;
                //PossibleEntries_Cpy[eachmTuple.Item2.ActiveDuration.Ticks.ToString()].Remove(eachmTuple.Item2.ID);
            }*/

            List<DateTime> ListOfFrontPartialsStartTime = FrontPartials_Dict.Keys.ToList();

            int i = 0;
            int j = 0;
            int FrontPartialCounter = 0;

            Tuple<DateTime, List<SubCalendarEvent>> TimeLineUpdated = null;
            SubCalendarEvent BorderElementBeginning = null;
            SubCalendarEvent BorderElementEnd = null;
            SubCalendarEvent LastSubCalElementForEarlierReferenceTime = null;
            int a = restrictedSnugFitAvailable.Count;
            int previ = i;

            Utility.PinSubEventsToEnd(restrictedSnugFitAvailable.Select(obj => obj.Item2).ToList(), FreeBoundary);
            bool ignorePlaceRestrictedinBestPosition = false;

            for (; i < restrictedSnugFitAvailable.Count; i++)
            {
                //bool isFreeSpotBeforeRigid = AllFreeSpots[i].End <= restrictedSnugFitAvailable[i].Item2.Start;
                TimeLineUpdated = null;

                previ = i;

                //restrictedSnugFitAvailable[i].Item2.PinSubEventsToStart(new TimeLine(EarliestReferenceTIme, restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End));
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

                List<SubCalendarEvent> LowestCostArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
                TimeLine PertinentFreeSpot = null;
                TimeLine FreeSpotUpdated = null;
                j = i + 1;
                if (ListOfFrontPartialsStartTime.Count > 0)//fits any sub calEvent in preceeding restricting free spot
                {
                    DateTime RestrictedStopper = restrictedSnugFitAvailable[i].Item2.Start;
                    bool breakForLoop = false;
                    bool PreserveRestrictedIndex = false;
                    for (; FrontPartialCounter < ListOfFrontPartialsStartTime.Count; FrontPartialCounter++)//for loop tries to prioritize the front partial elements as the boundary of the calculation of fittable elements.
                    {
                        TimeLineUpdated = null;
                        DateTime PertinentFreeSpotStart = EarliestReferenceTIme;
                        DateTime PertinentFreeSpotEnd;

                        if ((ListOfFrontPartialsStartTime[FrontPartialCounter] < RestrictedStopper))
                        {
                            PertinentFreeSpotEnd = ListOfFrontPartialsStartTime[FrontPartialCounter];
                            ListOfFrontPartialsStartTime.RemoveAt(FrontPartialCounter);
                            --FrontPartialCounter;
                            PreserveRestrictedIndex = true;
                        }
                        else
                        {
                            PertinentFreeSpotEnd = RestrictedStopper;

                            if (breakForLoop)
                            {//this allows for the population or insertion of the restrictedSnugFitAvailable[i]. Within the enclosing for loop the restrictedSnugFitAvailable[i] never gets appended until this point. 
                                PertinentFreeSpot = new TimeLine(PertinentFreeSpotStart, PertinentFreeSpotEnd);

                                BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                                BorderElementEnd = restrictedSnugFitAvailable[i].Item2;//uses restricted value as boundary element


                                restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
                                for (int q = i; q < restrictedSnugFitAvailable.Count; q++)
                                {
                                    restOfrestrictedSnugFitAvailable.Add(restrictedSnugFitAvailable[q]);
                                }

                                selectedRestrictedElements = LowestCostArrangement.Intersect(restOfrestrictedSnugFitAvailable.Select(obj => obj.Item2));
                                if (selectedRestrictedElements.Count() > 0)
                                {
                                    if (selectedRestrictedElements.Contains(restrictedSnugFitAvailable[i].Item2))
                                    {
                                        ignorePlaceRestrictedinBestPosition = true;//forces the call to PlaceSubCalEventInLowestCostPosition to be ignored. THis is needed if one of the elements in selected restricted elements is the current restrictedSnugFitAvailable
                                    }
                                    restrictedSnugFitAvailable.RemoveAll(obj => selectedRestrictedElements.Contains(obj.Item2));
                                }

                                LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restOfrestrictedSnugFitAvailable, Occupancy);





                                //LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restrictedSnugFitAvailable,Occupancy);
                                DateTime EarliestTimeForBetterEarlierReferenceTime = PertinentFreeSpot.Start;
                                LastSubCalElementForEarlierReferenceTime = ((CompleteArranegement.Count < 1) || (CompleteArranegement == null) ? null : CompleteArranegement[CompleteArranegement.Count - 1]);
                                FreeSpotUpdated = PertinentFreeSpot.CreateCopy();
                                if (LowestCostArrangement.Count > 0)
                                {
                                    if (!(LowestCostArrangement[0].getCalendarEventRange.Start == PertinentFreeSpot.Start))//Pin SubEvents To Start
                                    {//if the first element is not a partial Sub Cal Event element
                                        FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                                        Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                                    }
                                    else
                                    {
                                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();// new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                                        Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot);
                                    }
                                    EarliestReferenceTIme = PertinentFreeSpot.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;

                                    SubCalendarEvent LastSubCalEvent = LowestCostArrangement[LowestCostArrangement.Count - 1];
                                    EarliestTimeForBetterEarlierReferenceTime = LastSubCalEvent.End;
                                    LastSubCalElementForEarlierReferenceTime = LastSubCalEvent;

                                }
                                TimeLineUpdated = null;
                                /*TimeLineUpdated = ObtainBetterEarlierReferenceTime(LowestCostArrangement, CalendarIDAndNonPartialSubCalEvents, RestrictedStopper - EarliestTimeForBetterEarlierReferenceTime, EarliestReferenceTIme, new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End), LastSubCalElementForEarlierReferenceTime);
                                if (TimeLineUpdated != null)
                                {
                                    LowestCostArrangement = TimeLineUpdated.Item2;
                                    EarliestReferenceTIme = TimeLineUpdated.Item1;
                                }
                                */

                                foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                                {
                                    if (!selectedRestrictedElements.Contains(eachSubCalendarEvent))
                                    {
                                        --CompatibleWithList[eachSubCalendarEvent.ActiveDuration].Item1;
                                        PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Remove(eachSubCalendarEvent.ID);
                                        string SubCalString = eachSubCalendarEvent.SubEvent_ID.getLevelID(0);
                                        if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                                        {
                                            CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.ID);
                                            if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                                            {
                                                CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                                            }
                                        }
                                        if (PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Count < 1)
                                        {
                                            PossibleEntries_Cpy.Remove(eachSubCalendarEvent.ActiveDuration);
                                        }
                                    }
                                }


                                LowestCostArrangement = CompleteArranegement.Concat(LowestCostArrangement).ToList();
                                if (!ignorePlaceRestrictedinBestPosition)
                                {
                                    LowestCostArrangement = PlaceSubCalEventInLowestCostPosition(FreeBoundary, restrictedSnugFitAvailable[i].Item2, LowestCostArrangement);
                                }

                                ignorePlaceRestrictedinBestPosition = false;
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeBoundary);

                                CompleteArranegement = LowestCostArrangement;
                                EarliestReferenceTIme = LowestCostArrangement[LowestCostArrangement.Count - 1].End;

                                PreserveRestrictedIndex = false;
                                break;
                            }


                            /*restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
                            for (int q = i+1; q < restrictedSnugFitAvailable.Count; q++)
                            {
                                restOfrestrictedSnugFitAvailable.Add(restrictedSnugFitAvailable[q]);
                            }*/


                            //if (restOfrestrictedSnugFitAvailable.Count > 0)
                            {
                                //Utility.PinSubEventsToEnd(restOfrestrictedSnugFitAvailable.Select(obj=>obj.Item2).ToList(), FreeBoundary);
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
                        }
                        PertinentFreeSpot = new TimeLine(PertinentFreeSpotStart, PertinentFreeSpotEnd);

                        BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                        BorderElementEnd = restrictedSnugFitAvailable[i].Item2;//uses restricted value as boundary element
                        restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
                        for (int q = i; q < restrictedSnugFitAvailable.Count; q++)
                        {
                            restOfrestrictedSnugFitAvailable.Add(restrictedSnugFitAvailable[q]);
                        }



                        LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restOfrestrictedSnugFitAvailable, Occupancy);

                        selectedRestrictedElements = LowestCostArrangement.Intersect(restOfrestrictedSnugFitAvailable.Select(obj => obj.Item2));
                        if (selectedRestrictedElements.Count() > 0)
                        {

                        }














                        LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restrictedSnugFitAvailable, Occupancy);
                        DateTime LatestDaterforEarlierReferenceTime = PertinentFreeSpot.Start;
                        LastSubCalElementForEarlierReferenceTime = ((CompleteArranegement.Count < 1) || (CompleteArranegement == null) ? null : CompleteArranegement[CompleteArranegement.Count - 1]);//updates the last element as either null or the last element in the current Complete arrangement
                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();
                        if (LowestCostArrangement.Count > 0)
                        {
                            if (!(LowestCostArrangement[0].getCalendarEventRange.Start == PertinentFreeSpot.Start))//Pin SubEvents To Start
                            {//if the first element is not a partial Sub Cal Event element
                                FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);

                            }
                            else
                            {
                                //FreeSpotUpdated = new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                                FreeSpotUpdated = PertinentFreeSpot.CreateCopy();
                                Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot);
                            }
                            EarliestReferenceTIme = PertinentFreeSpot.End;

                            ///Comeback to this
                            ///
                            SubCalendarEvent LastSubCalEvent = LowestCostArrangement[LowestCostArrangement.Count - 1];
                            LatestDaterforEarlierReferenceTime = LastSubCalEvent.End;
                            LastSubCalElementForEarlierReferenceTime = LastSubCalEvent;
                        }

                        /*TimeLineUpdated = ObtainBetterEarlierReferenceTime(LowestCostArrangement, CalendarIDAndNonPartialSubCalEvents, RestrictedStopper - LatestDaterforEarlierReferenceTime, EarliestReferenceTIme, new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End), LastSubCalElementForEarlierReferenceTime);
                        //errorline

                        if (TimeLineUpdated != null)
                        {
                            LowestCostArrangement = TimeLineUpdated.Item2;
                            EarliestReferenceTIme = TimeLineUpdated.Item1;
                        }

                        */
                        foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                        {
                            --CompatibleWithList[eachSubCalendarEvent.ActiveDuration].Item1;
                            PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Remove(eachSubCalendarEvent.ID);
                            string SubCalString = eachSubCalendarEvent.SubEvent_ID.getLevelID(0);
                            if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                            {
                                CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.ID);
                                if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                                {
                                    CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                                }
                            }
                            if (PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Count < 1)
                            {
                                PossibleEntries_Cpy.Remove(eachSubCalendarEvent.ActiveDuration);
                            }
                        }
                        CompleteArranegement.AddRange(LowestCostArrangement);


                        int DateTimeCounter = 0;
                        for (; DateTimeCounter < FrontPartials_Dict.Keys.Count; DateTimeCounter++)//updates CalendarIDAndNonPartialSubCalEvents if frontpartial Startime has been passed. Alls updates FrontPartials_Dict
                        {
                            DateTime eachDateTIme = FrontPartials_Dict.Keys.ToList()[DateTimeCounter];
                            if (EarliestReferenceTIme >= eachDateTIme)
                            {
                                List<mTuple<bool, SubCalendarEvent>> mTUpleSubCalEvents = FrontPartials_Dict[eachDateTIme];
                                foreach (mTuple<bool, SubCalendarEvent> eachmTUple in mTUpleSubCalEvents)
                                {

                                    string CalLevel0ID = eachmTUple.Item2.SubEvent_ID.getLevelID(0);
                                    if (!CompleteArranegement.Contains(eachmTUple.Item2))
                                    {
                                        if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(CalLevel0ID))
                                        {
                                            CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(eachmTUple.Item2.ID, eachmTUple.Item2);
                                        }
                                        else
                                        {
                                            //CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new List<SubCalendarEvent>() { KeyValuePair0.Value.Item2 });
                                            CalendarIDAndNonPartialSubCalEvents.Add(CalLevel0ID, new Dictionary<string, SubCalendarEvent>());
                                            CalendarIDAndNonPartialSubCalEvents[CalLevel0ID].Add(eachmTUple.Item2.ID, eachmTUple.Item2);
                                        }
                                    }
                                }
                                FrontPartials_Dict.Remove(eachDateTIme);
                            }
                        }


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

                    BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                    BorderElementEnd = restrictedSnugFitAvailable[i].Item2;//uses restricted value as boundary element

                    restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
                    for (int q = i; q < restrictedSnugFitAvailable.Count; q++)
                    {
                        restOfrestrictedSnugFitAvailable.Add(restrictedSnugFitAvailable[q]);
                    }



                    LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restOfrestrictedSnugFitAvailable, Occupancy);

                    selectedRestrictedElements = LowestCostArrangement.Intersect(restOfrestrictedSnugFitAvailable.Select(obj => obj.Item2));
                    if (selectedRestrictedElements.Count() > 0)
                    {
                        ;
                    }


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
                        --CompatibleWithList[eachSubCalendarEvent.ActiveDuration].Item1;
                        PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Remove(eachSubCalendarEvent.ID);
                        string SubCalString = eachSubCalendarEvent.SubEvent_ID.getLevelID(0);
                        if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                        {
                            CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.ID);
                            if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                            {
                                CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                            }
                        }
                        if (PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Count < 1)
                        {
                            PossibleEntries_Cpy.Remove(eachSubCalendarEvent.ActiveDuration);
                        }
                    }


                    List<SubCalendarEvent> AdditionalCOstArrangement = new System.Collections.Generic.List<SubCalendarEvent>();
                    DateTime RelativeEndTime;
                    if (j < restrictedSnugFitAvailable.Count)
                    {
                        //DateTime StartDateTimeAfterFitting = PertinentFreeSpot.End;
                        DateTime StartDateTimeAfterFitting = EarliestReferenceTIme;//this is the barring end time of the preceding boundary search. Earliest would have been updated if there was some event detected.


                        RelativeEndTime = restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End > restrictedSnugFitAvailable[j].Item2.Start ? restrictedSnugFitAvailable[j].Item2.Start : restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End;

                        RelativeEndTime -= restrictedSnugFitAvailable[i].Item2.ActiveDuration;
                        TimeLine CurrentlyFittedTimeLine = new TimeLine(StartDateTimeAfterFitting, RelativeEndTime);

                        BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                        BorderElementEnd = restrictedSnugFitAvailable[i].Item2;//uses restricted value as boundary element


                        restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
                        for (int q = i; q < restrictedSnugFitAvailable.Count; q++)
                        {
                            restOfrestrictedSnugFitAvailable.Add(restrictedSnugFitAvailable[q]);
                        }



                        AdditionalCOstArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(CurrentlyFittedTimeLine, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restOfrestrictedSnugFitAvailable, Occupancy);

                        selectedRestrictedElements = AdditionalCOstArrangement.Intersect(restOfrestrictedSnugFitAvailable.Select(obj => obj.Item2));
                        if (selectedRestrictedElements.Count() > 0)
                        {
                            ;
                        }



                        AdditionalCOstArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(CurrentlyFittedTimeLine, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restrictedSnugFitAvailable, Occupancy);
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
                                --CompatibleWithList[eachSubCalendarEvent.ActiveDuration].Item1;
                                PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Remove(eachSubCalendarEvent.ID);
                                string SubCalString = eachSubCalendarEvent.SubEvent_ID.getLevelID(0);
                                if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                                {
                                    CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.ID);
                                    if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                                    {
                                        CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                                    }
                                }
                                if (PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Count < 1)
                                {
                                    PossibleEntries_Cpy.Remove(eachSubCalendarEvent.ActiveDuration);
                                }
                            }


                            RelativeEndTime = AdditionalCOstArrangement[AdditionalCOstArrangement.Count - 1].End;
                            RelativeEndTime += restrictedSnugFitAvailable[i].Item2.ActiveDuration; ;
                            CurrentlyFittedTimeLine = new TimeLine(FreeSpotUpdated.Start, RelativeEndTime);
                            //AdditionalCOstArrangement = PlaceSubCalEventInLowestCostPosition(CurrentlyFittedTimeLine, restrictedSnugFitAvailable[i].Item2, AdditionalCOstArrangement);
                        }
                        else
                        {//if there is no other Restricted in list
                            RelativeEndTime += restrictedSnugFitAvailable[i].Item2.ActiveDuration;
                            CurrentlyFittedTimeLine = new TimeLine(CurrentlyFittedTimeLine.Start, RelativeEndTime);
                            //AdditionalCOstArrangement = PlaceSubCalEventInLowestCostPosition(CurrentlyFittedTimeLine, restrictedSnugFitAvailable[i].Item2, AdditionalCOstArrangement);
                        }
                    }
                    else
                    {
                        RelativeEndTime = restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End > FreeBoundary.End ? FreeBoundary.End : restrictedSnugFitAvailable[i].Item2.getCalendarEventRange.End;
                        TimeLine CurrentlyFittedTimeLine = new TimeLine(EarliestReferenceTIme, RelativeEndTime);
                        //AdditionalCOstArrangement = PlaceSubCalEventInLowestCostPosition(CurrentlyFittedTimeLine, restrictedSnugFitAvailable[i].Item2, AdditionalCOstArrangement);
                    }

                    CompleteArranegement.AddRange(LowestCostArrangement);
                    CompleteArranegement.AddRange(AdditionalCOstArrangement);
                    TimeLine encasingTimeLine = new TimeLine(FreeBoundary.Start, RelativeEndTime);
                    CompleteArranegement = PlaceSubCalEventInLowestCostPosition(encasingTimeLine, restrictedSnugFitAvailable[i].Item2, CompleteArranegement);
                    if (CompleteArranegement.Count > 0)
                    {
                        EarliestReferenceTIme = CompleteArranegement[CompleteArranegement.Count - 1].End;
                    }
                }
            }


            { //Handles THe Last Free Space outside of rigids
                TimeLine FreeSpotOutSideRigids = new TimeLine(EarliestReferenceTIme, FreeBoundary.End);
                TimeLine PertinentFreeSpot = new TimeLine(EarliestReferenceTIme, FreeBoundary.End); ;
                TimeLine FreeSpotUpdated;
                List<SubCalendarEvent> LowestCostArrangement;
                if (ListOfFrontPartialsStartTime.Count > 0)
                {
                    for (FrontPartialCounter = 0; FrontPartialCounter < ListOfFrontPartialsStartTime.Count; FrontPartialCounter++)
                    {
                        DateTime PertinentFreeSpotStart = EarliestReferenceTIme;
                        DateTime PertinentFreeSpotEnd;
                        PertinentFreeSpotEnd = ListOfFrontPartialsStartTime[FrontPartialCounter];
                        //FrontPartials_Dict.Remove(ListOfFrontPartialsStartTime[FrontPartialCounter]);
                        ListOfFrontPartialsStartTime.RemoveAt(FrontPartialCounter);
                        --FrontPartialCounter;
                        PertinentFreeSpot = new TimeLine(PertinentFreeSpotStart, PertinentFreeSpotEnd);
                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();

                        BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                        BorderElementEnd = null;


                        restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
                        for (int q = i; q < restrictedSnugFitAvailable.Count; q++)
                        {
                            restOfrestrictedSnugFitAvailable.Add(restrictedSnugFitAvailable[q]);
                        }



                        LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restOfrestrictedSnugFitAvailable, Occupancy);

                        selectedRestrictedElements = LowestCostArrangement.Intersect(restOfrestrictedSnugFitAvailable.Select(obj => obj.Item2));
                        if (selectedRestrictedElements.Count() > 0)
                        {
                            ;
                        }







                        LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restrictedSnugFitAvailable, Occupancy);
                        DateTime LatestTimeForBetterEarlierReferenceTime = PertinentFreeSpot.Start;
                        LastSubCalElementForEarlierReferenceTime = ((CompleteArranegement.Count < 1) || (CompleteArranegement == null) ? null : CompleteArranegement[CompleteArranegement.Count - 1]);
                        if (LowestCostArrangement.Count > 0)
                        {
                            if ((LowestCostArrangement[0].getCalendarEventRange.Start != PertinentFreeSpot.Start))//Pin SubEvents To Start
                            {//if the first element is not a partial Sub Cal Event element
                                FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);

                            }
                            else
                            {
                                FreeSpotUpdated = PertinentFreeSpot.CreateCopy();// new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                            }
                            EarliestReferenceTIme = PertinentFreeSpot.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                            SubCalendarEvent LastSubCalEvent = LowestCostArrangement[LowestCostArrangement.Count - 1];
                            LatestTimeForBetterEarlierReferenceTime = LastSubCalEvent.End;
                            LastSubCalElementForEarlierReferenceTime = LastSubCalEvent;
                            /*
                            Dictionary<string, double> AllValidNodes = CalendarEvent.DistanceToAllNodes(LastSubCalEvent.SubEvent_ID.getLevelID(0));
                            SubCalendarEvent AppendableEVent;
                            foreach (string eachstring in AllValidNodes.Keys)
                            {
                                if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(eachstring))
                                {
                                    AppendableEVent = CalendarIDAndNonPartialSubCalEvents[eachstring].ToList()[0].Value;//Assumes Theres Always an element
                                    

                                    if ((AppendableEVent.ActiveDuration <= (FreeBoundary.End - LastSubCalEvent.End)) && (!LowestCostArrangement.Contains(AppendableEVent)))
                                    {
                                        LowestCostArrangement.Add(AppendableEVent);
                                        CalendarIDAndNonPartialSubCalEvents[eachstring].Remove(AppendableEVent.ID);
                                        if (CalendarIDAndNonPartialSubCalEvents[eachstring].Count < 1)//checks if List is empty. Deletes keyValuepair if list is empty
                                        {
                                            CalendarIDAndNonPartialSubCalEvents.Remove(eachstring);
                                        }
                                        FreeSpotUpdated = new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End);
                                        Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                                        EarliestReferenceTIme = AppendableEVent.End;
                                        break;
                                    }
                                }
                            }*/
                        }


                        TimeLineUpdated = null;
                        /*TimeLineUpdated = ObtainBetterEarlierReferenceTime(LowestCostArrangement, CalendarIDAndNonPartialSubCalEvents, FreeBoundary.End - LatestTimeForBetterEarlierReferenceTime, EarliestReferenceTIme, new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End), LastSubCalElementForEarlierReferenceTime);
                        if (TimeLineUpdated != null)
                        {
                            LowestCostArrangement = TimeLineUpdated.Item2;
                            EarliestReferenceTIme = TimeLineUpdated.Item1;
                        }
                        */
                        foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                        {
                            --CompatibleWithList[eachSubCalendarEvent.ActiveDuration].Item1;
                            PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Remove(eachSubCalendarEvent.ID);
                            string SubCalString = eachSubCalendarEvent.SubEvent_ID.getLevelID(0);
                            if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                            {
                                CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.ID);
                                if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                                {
                                    CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                                }
                            }
                            if (PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Count < 1)
                            {
                                PossibleEntries_Cpy.Remove(eachSubCalendarEvent.ActiveDuration);
                            }
                        }
                        CompleteArranegement.AddRange(LowestCostArrangement);
                    }
                }


                DateTime ReferenceEndTime = FreeBoundary.End;
                PertinentFreeSpot = new TimeLine(EarliestReferenceTIme, ReferenceEndTime);
                /*LowestCostArrangement = OptimizeArrangeOfSubCalEvent(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(null, null), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy);

                if (LowestCostArrangement.Count > 0)
                {
                    if (!(LowestCostArrangement[0].getCalendarEventRange.Start == PertinentFreeSpot.Start))//Pin SubEvents To Start
                    {//if the first element is not a partial Sub Cal Event element
                        FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                        Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                    }
                    else
                    {
                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();// new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                        Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                    }
                    EarliestReferenceTIme = FreeSpotUpdated.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                }*/
                BorderElementBeginning = CompleteArranegement.Count > 0 ? CompleteArranegement[CompleteArranegement.Count - 1] : null;//Checks if Complete arrangement has partially being filled. Sets Last elements as boundary Element
                BorderElementEnd = null;




                restOfrestrictedSnugFitAvailable = new List<mTuple<bool, SubCalendarEvent>>();
                for (int q = i; q < restrictedSnugFitAvailable.Count; q++)
                {
                    restOfrestrictedSnugFitAvailable.Add(restrictedSnugFitAvailable[q]);
                }



                LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restOfrestrictedSnugFitAvailable, Occupancy);

                selectedRestrictedElements = LowestCostArrangement.Intersect(restOfrestrictedSnugFitAvailable.Select(obj => obj.Item2));
                if (selectedRestrictedElements.Count() > 0)
                {
                    ;
                }














                LowestCostArrangement = OptimizeArrangeOfSubCalEvent_NonAggressive(PertinentFreeSpot, new Tuple<SubCalendarEvent, SubCalendarEvent>(BorderElementBeginning, BorderElementEnd), CompatibleWithList.Values.ToList(), PossibleEntries_Cpy, restrictedSnugFitAvailable, Occupancy);
                LastSubCalElementForEarlierReferenceTime = ((CompleteArranegement.Count < 1) || (CompleteArranegement == null) ? null : CompleteArranegement[CompleteArranegement.Count - 1]);
                DateTime LimitForBetterEarlierReferencTime = EarliestReferenceTIme;
                FreeSpotUpdated = PertinentFreeSpot.CreateCopy();
                if (LowestCostArrangement.Count > 0)
                {
                    if ((LowestCostArrangement[0].getCalendarEventRange.Start != PertinentFreeSpot.Start))//Pin SubEvents To Start
                    {//if the first element is not a partial Sub Cal Event element
                        FreeSpotUpdated = new TimeLine(EarliestReferenceTIme, PertinentFreeSpot.End);
                        Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);

                    }
                    else
                    {
                        FreeSpotUpdated = PertinentFreeSpot.CreateCopy();// new TimeLine(LowestCostArrangement[0].getCalendarEventRange.Start, PertinentFreeSpot.End);
                        Utility.PinSubEventsToStart(LowestCostArrangement, PertinentFreeSpot);
                    }
                    EarliestReferenceTIme = PertinentFreeSpot.End;// LowestCostArrangement[LowestCostArrangement.Count - 1].End;
                    SubCalendarEvent LastSubCalEvent = LowestCostArrangement[LowestCostArrangement.Count - 1];
                    LimitForBetterEarlierReferencTime = LastSubCalEvent.End;
                    LastSubCalElementForEarlierReferenceTime = LastSubCalEvent;



                    /*
                    
                    
                    Dictionary<string, double> AllValidNodes = CalendarEvent.DistanceToAllNodes(LastSubCalEvent.SubEvent_ID.getLevelID(0));
                    SubCalendarEvent AppendableEVent;
                    foreach (string eachstring in AllValidNodes.Keys)
                    {
                        if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(eachstring))
                        {
                            AppendableEVent = CalendarIDAndNonPartialSubCalEvents[eachstring].ToList()[0].Value;//Assumes Theres Always an element

                            if ((AppendableEVent.ActiveDuration <= (FreeBoundary.End - LastSubCalEvent.End)) && (!LowestCostArrangement.Contains(AppendableEVent)))
                            {
                                LowestCostArrangement.Add(AppendableEVent);
                                CalendarIDAndNonPartialSubCalEvents[eachstring].Remove(AppendableEVent.ID);
                                if (CalendarIDAndNonPartialSubCalEvents[eachstring].Count < 1)//checks if List is empty. Deletes keyValuepair if list is empty
                                {
                                    CalendarIDAndNonPartialSubCalEvents.Remove(eachstring);
                                }
                                FreeSpotUpdated = new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End);
                                Utility.PinSubEventsToStart(LowestCostArrangement, FreeSpotUpdated);
                                EarliestReferenceTIme = AppendableEVent.End;
                                break;
                            }
                        }
                    }*/

                }
                TimeLineUpdated = null;
                /*TimeLineUpdated = ObtainBetterEarlierReferenceTime(LowestCostArrangement, CalendarIDAndNonPartialSubCalEvents, FreeBoundary.End - LimitForBetterEarlierReferencTime, EarliestReferenceTIme, new TimeLine(FreeSpotUpdated.Start, FreeBoundary.End), LastSubCalElementForEarlierReferenceTime);
                if (TimeLineUpdated != null)
                {
                    LowestCostArrangement = TimeLineUpdated.Item2;
                    EarliestReferenceTIme = TimeLineUpdated.Item1;
                }
                */

                foreach (SubCalendarEvent eachSubCalendarEvent in LowestCostArrangement)
                {
                    --CompatibleWithList[eachSubCalendarEvent.ActiveDuration].Item1;
                    PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Remove(eachSubCalendarEvent.ID);
                    string SubCalString = eachSubCalendarEvent.SubEvent_ID.getLevelID(0);
                    if (CalendarIDAndNonPartialSubCalEvents.ContainsKey(SubCalString))
                    {
                        CalendarIDAndNonPartialSubCalEvents[SubCalString].Remove(eachSubCalendarEvent.ID);
                        if (CalendarIDAndNonPartialSubCalEvents[SubCalString].Count < 1)
                        {
                            CalendarIDAndNonPartialSubCalEvents.Remove(SubCalString);
                        }
                    }

                    if (PossibleEntries_Cpy[eachSubCalendarEvent.ActiveDuration].Count < 1)
                    {
                        PossibleEntries_Cpy.Remove(eachSubCalendarEvent.ActiveDuration);
                    }
                }
                CompleteArranegement.AddRange(LowestCostArrangement);

            }






            List<mTuple<bool, SubCalendarEvent>> retValue = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();

            foreach (SubCalendarEvent eachSubCalendarEvent in CompleteArranegement)
            {
                PossibleEntries[eachSubCalendarEvent.ActiveDuration][eachSubCalendarEvent.ID].Item1 = true;
                retValue.Add(PossibleEntries[eachSubCalendarEvent.ActiveDuration][eachSubCalendarEvent.ID]);
            }

            //List<List<SubCalendarEvent>> unrestrictedValidCombinations = generateCombinationForDifferentEntries(CompatibleWithList, PossibleEntries);

            retValue = reAlignSubCalEvents(FreeBoundary, retValue);
            if (TotalEventsForThisTImeLine != retValue.Count)
            {
                ;
            }
            return retValue;

        }




        List<List<SubCalendarEvent>> generateCombinationForDifferentEntriesNonAggressive(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CompatibleWithList, Dictionary<TimeSpan, Dictionary<string, mTuple<bool, SubCalendarEvent>>> PossibleEntries)
        {
            /*
             * Function attempts to generate multiple combinations of compatible sub calendar event for Snug fit entry
             * CompatibleWithList is an snug fit result
             * PossibleEntries are the possible sub calendar that can be used in the combinatorial result
             */
            ++CountCall;
            if (CountCall == 4)
            {
                ;
            }
            List<List<List<string>>> MAtrixedSet = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<string>>>();
            Dictionary<string, mTuple<int, List<SubCalendarEvent>>> var4 = new System.Collections.Generic.Dictionary<string, mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>>();
            List<List<SubCalendarEvent>> retValue = new System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>();
            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair0 in CompatibleWithList)//loops every timespan in Snug FIt possibility
            {
                TimeSpan eachTimeSpan = eachKeyValuePair0.Key;

                Dictionary<string, mTuple<bool, SubCalendarEvent>> var1 = PossibleEntries[eachTimeSpan];
                List<List<string>> receivedValue = new System.Collections.Generic.List<System.Collections.Generic.List<string>>();
                Dictionary<string, int> var2 = new System.Collections.Generic.Dictionary<string, int>();
                foreach (KeyValuePair<string, mTuple<bool, SubCalendarEvent>> eachKeyValuePair in var1)
                {
                    string ParentID = eachKeyValuePair.Value.Item2.SubEvent_ID.getStringIDAtLevel(0);
                    if (var2.ContainsKey(ParentID))
                    {
                        ++var2[ParentID];
                        var4[ParentID].Item2.Add(eachKeyValuePair.Value.Item2);
                    }
                    else
                    {
                        var2.Add(ParentID, 1);
                        List<SubCalendarEvent> var5 = new System.Collections.Generic.List<SubCalendarEvent>();
                        var5.Add(eachKeyValuePair.Value.Item2);
                        var4.Add(ParentID, new mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>(0, var5));
                    }
                }
                List<mTuple<string, int>> PossibleCalEvents = new System.Collections.Generic.List<mTuple<string, int>>();
                foreach (KeyValuePair<string, int> eachKeyValuePair in var2)
                {
                    PossibleCalEvents.Add(new mTuple<string, int>(eachKeyValuePair.Key, eachKeyValuePair.Value));
                }

                List<List<string>> var3 = generateCombinationForSpecficTimeSpanStringID(eachKeyValuePair0.Value.Item1, PossibleCalEvents);

                if (var3.Count > 1)
                {
                    var3 = var3.Where(obj => obj.Count == eachKeyValuePair0.Value.Item1).ToList();
                }

                MAtrixedSet.Add(var3);
            }

            List<List<string>> serializedList = Utility.SerializeList(MAtrixedSet);
            foreach (List<string> eachList in serializedList)//serializedList has a list of fittable ParentIDs, the loop replaces each List of strings with List of subCalendarEvents
            {
                List<SubCalendarEvent> var6 = new System.Collections.Generic.List<SubCalendarEvent>();
                mTuple<int, List<SubCalendarEvent>> var7 = new mTuple<int, System.Collections.Generic.List<SubCalendarEvent>>(0, new System.Collections.Generic.List<SubCalendarEvent>());
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


        List<mTuple<bool, SubCalendarEvent>> stitchRestrictedSubCalendarEvent(List<mTuple<bool, SubCalendarEvent>> Arg1, TimeLine RestrictingTimeLine, SubCalendarEvent PrecedingPivot = null)
        {
            List<SubCalendarEvent> retValue = stitchRestrictedSubCalendarEvent(Arg1.Select(obj => new mTuple<double, mTuple<TimeLine, SubCalendarEvent>>(0, new mTuple<TimeLine, SubCalendarEvent>(new TimeLine(), obj.Item2))).ToList(), RestrictingTimeLine, PrecedingPivot);
            return retValue.Select(obj => new mTuple<bool, SubCalendarEvent>(true, obj)).ToList();
        }

        /*
        List< SubCalendarEvent> stitchRestrictedSubCalendarEvent(List< SubCalendarEvent> Arg1, TimeLine RestrictingTimeLine, SubCalendarEvent PrecedingPivot = null)
        {
            return stitchRestrictedSubCalendarEvent(Arg1.Select(obj => new mTuple<int, SubCalendarEvent>(0, obj)).ToList(), RestrictingTimeLine, null);
        }*/

        List<SubCalendarEvent> stitchRestrictedSubCalendarEvent(List<mTuple<double, mTuple<TimeLine, SubCalendarEvent>>> Arg1, TimeLine RestrictingTimeLine, SubCalendarEvent PrecedingPivot = null)
        {
            List<SubCalendarEvent> retValue = Arg1.Select(obj => obj.Item2.Item2).ToList();

            TimeSpan SumOfAllSubCalEvent = Utility.SumOfActiveDuration(Arg1.Select(obj => obj.Item2.Item2));
            List<SubCalendarEvent> CopyOfAllList = Arg1.Select(obj => obj.Item2.Item2).ToList();
            if (retValue.Count < 1)//if arg1 is empty return the list
            {
                return retValue;
            }
            List<SubCalendarEvent> AllSubCalEvents = Arg1.Select(obj => obj.Item2.Item2).ToList();
            List<mTuple<TimeLine, SubCalendarEvent>> AvaialableTimeSpan = new List<mTuple<TimeLine, SubCalendarEvent>>();
            int indexOfSmallest = -2222;
            int i = 0;


            TimeLine InterferringTimeLine = RestrictingTimeLine.CreateCopy();
            TimeSpan SmallestAssignedTimeSpan = new TimeSpan(3650, 0, 0, 0);//sets the smallest TimeSpan To 10 years
            DateTime SmallestDateTime = new DateTime(3000, 12, 31);


            Arg1 =Arg1.Select(obj => //using Linq to update timeline
            {
                obj.Item2.Item1 = obj.Item2.Item2.getCalendarEventRange.InterferringTimeLine(RestrictingTimeLine);//gets interferring TImeLine
                obj.Item1 += obj.Item2.Item1 != null ? obj.Item2.Item1.TimelineSpan.TotalSeconds / obj.Item2.Item2.ActiveDuration.TotalSeconds : 0;
                return obj; 
            }).ToList();
            Arg1=Arg1.OrderBy(obj => obj.Item1).ToList();
            List<mTuple<double, mTuple<TimeLine, SubCalendarEvent>>> WorkableList = Arg1.Where(obj => obj.Item2.Item1 != null).ToList();
            WorkableList = WorkableList.Where(obj => obj.Item2.Item1.TimelineSpan >= obj.Item2.Item2.ActiveDuration).ToList();

            //Build Strict Towards right of the tree
            if (WorkableList.Count > 0)
            {
                mTuple<double, mTuple<TimeLine, SubCalendarEvent>> PivotNodeData = WorkableList[0];


                bool PinningSuccess= PivotNodeData.Item2.Item2.PinSubEventsToStart(RestrictingTimeLine);
                DateTime StartTimeOfRightTree = RestrictingTimeLine.Start;
                DateTime EndTimeOfRightTree = RestrictingTimeLine.End;
                SubCalendarEvent includentSubCakendarEvent=null;
                if (PinningSuccess)//hack alert Subevent fittable a double less than 1 e,g 0.5
                {
                    StartTimeOfRightTree = PivotNodeData.Item2.Item2.End;
                    includentSubCakendarEvent = PivotNodeData.Item2.Item2;
                }
                WorkableList.RemoveAt(0);

                TimeLine RightTimeLine = new TimeLine(StartTimeOfRightTree, EndTimeOfRightTree);


                List<mTuple<double, mTuple<TimeLine, SubCalendarEvent>>> WorkableList_Cpy = WorkableList.ToList();
                List<SubCalendarEvent> rightTreeResult = stitchRestrictedSubCalendarEvent(WorkableList, RightTimeLine);
                if(includentSubCakendarEvent!=null)
                {
                    rightTreeResult.Insert(0, includentSubCakendarEvent);
                }

                if (!Utility.PinSubEventsToEnd(rightTreeResult, RestrictingTimeLine))
                {
                    ;
                };

                //Build Strict Towards Left of the tree
                DateTime StartTimeOfleftTree = RestrictingTimeLine.Start;
                DateTime EndTimeOfLeftTree = RestrictingTimeLine.End;
                if (rightTreeResult.Count > 0)
                {
                    EndTimeOfLeftTree = rightTreeResult[0].Start;
                }

                TimeLine LeftTimeLine = new TimeLine(StartTimeOfleftTree, EndTimeOfLeftTree);

                WorkableList_Cpy.RemoveAll(obj => rightTreeResult.Contains(obj.Item2.Item2));




                List<SubCalendarEvent> LeftTreeResult = stitchRestrictedSubCalendarEvent(WorkableList_Cpy, LeftTimeLine);
                Utility.PinSubEventsToEnd(LeftTreeResult, RestrictingTimeLine);

                retValue = LeftTreeResult.Concat(rightTreeResult).ToList();
            }
            else //if there are no feasible TimeLine that are withing RestrictingTimeLine that can also contain a subcalevent
            {
                return new List<SubCalendarEvent>();
            }
            return retValue;

        }


        /*
        List<SubCalendarEvent> stitchRestrictedSubCalendarEvent(List<mTuple<int, SubCalendarEvent>> Arg1, TimeLine RestrictingTimeLine, SubCalendarEvent PrecedingPivot = null)
        {
            //mTuple<int, SubCalendarEvent> 
            //return stitchRestrictedSubCalendarEvent(Arg1.Select(obj=>new mTuple<int , mTuple<TimeSpan, SubCalendarEvent>>( obj.Item1, new mTuple<TimeSpan,SubCalendarEvent>(TimeSpan.FromTicks(obj.Item1*obj.Item2.ActiveDuration.Ticks),obj.Item2)) ).ToList() , RestrictingTimeLine, PrecedingPivot);

        }*/
        
        
        /*
        SubCalendarEvent CalculateWorkSpaceUsage(TimeLine myTimeLine, List<SubCalendarEvent> AllSubCalEvent_SortedByDeadLine)
        {
            List<Tuple<SubCalendarEvent, double, TimeSpan>> SubcalEvents = new List<Tuple<SubCalendarEvent, double, TimeSpan>>();

        }
        */


        Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> stitchRestrictedSubCalendarEvent(List<TimeLine> AllTimeLines, int TimeLineIndex, Dictionary<TimeLine, List<mTuple<bool, SubCalendarEvent>>> arg1)
        {
            /*
             * arg1 is adictionary that has a timeline witha a list of restricted Sub calendar events             
             */

            Dictionary<string, bool> var1 = new System.Collections.Generic.Dictionary<string, bool>();

            foreach (TimeLine eachTimeLine in AllTimeLines)
            {

                arg1[eachTimeLine] = stitchRestrictedSubCalendarEvent(arg1[eachTimeLine], eachTimeLine);


                /*List<mTuple<bool, SubCalendarEvent>> arg2 = arg1[eachTimeLine];
                List<SubCalendarEvent> arg3 = new System.Collections.Generic.List<SubCalendarEvent>();
                foreach (mTuple<bool, SubCalendarEvent> eachmTuple in arg2)
                {
                    arg3.Add(eachmTuple.Item2);
                    var1.Add(eachmTuple.Item2.ID, eachmTuple.Item1);
                }
                arg3 = arg3.OrderBy(obj => obj.getCalendarEventRange.End).ToList();

                List<List<SubCalendarEvent>> arg4 = Pseudo_generateTreeCallsToSnugArray(arg3, eachTimeLine);

                List<SubCalendarEvent> OPtimizedList = getOPtimized(arg4);

                List<mTuple<bool, SubCalendarEvent>> var5 = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
                foreach (SubCalendarEvent eachSubCalendarEvent in OPtimizedList)
                {
                    var5.Add(new mTuple<bool, SubCalendarEvent>(var1[eachSubCalendarEvent.ID], eachSubCalendarEvent));
                }
                arg1[eachTimeLine] = var5;*/
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


        List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> getAveragedOutTIimeLine(List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> arg1, int PreecdintTimeSpanWithSringIDCoun)
        {
            /*
             * Function takes a list of valid possible matches. It uses this list of valid matches to calculate an average which will be used to calculate the best snug possibility
             * arg1= The List of possible snug time Lines
             */

            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> retValue = new List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();
            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> Total = new List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>();
            /*if (arg1.Count<1)
            {
                return retValue;
            }*/
            foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in arg1[0])//initializes Total with the first element in the list
            {
                Total.Add(new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>());
            }



            int i = 0;
            int j = 0;

            for (; j < arg1.Count; j++)
            {
                List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> eachList = arg1[j];
                if (j == 30)
                {
                    ;
                }
                i = 0;
                foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in eachList)
                {

                    foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in eachDictionary)
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

            Dictionary<TimeSpan, int> RoundUpRoundDown = new System.Collections.Generic.Dictionary<TimeSpan, int>();
            i = 0;
            foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in Total)//initializes Total with the first element in the list
            {

                retValue.Add(new System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>());
                foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in eachDictionary)
                {
                    double arg0 = eachKeyValuePair.Value.Item1 / j;
                    int PassedToRet = (int)Math.Round(arg0);
                    string[] Int_Decimal = arg0.ToString().Split('.');
                    if (Int_Decimal.Length == 2)
                    {
                        if (Int_Decimal[1] == "5")
                        {
                            if (RoundUpRoundDown.ContainsKey(eachKeyValuePair.Key))
                            {
                                if (RoundUpRoundDown[eachKeyValuePair.Key] == 0)
                                {
                                    RoundUpRoundDown[eachKeyValuePair.Key] = 1;
                                }
                                else
                                {
                                    PassedToRet = (int)Math.Floor(arg0);
                                    RoundUpRoundDown[eachKeyValuePair.Key] = 0;
                                }
                            }
                            else
                            {
                                PassedToRet = (int)Math.Floor(arg0);
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


            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> ListOfPossibleretValue = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();


            i = 0;
            foreach (List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> eachList in arg1)
            {
                i = 0;
                bool arg3 = false;
                foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in retValue)//initializes Total with the first element in the list
                {
                    Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> arg2 = eachList[i];
                    foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in eachDictionary)
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
            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> SpreadOutResult = getMostSpreadOut(ListOfPossibleretValue);
            Random myRandomizer = new Random();
            if (((SpreadOutResult.Count < 1) && (arg1.Count > 0)))//hack alert, this says if for some reason we have no generated possible ListOfPossibleretValue, we can pick one at random
            {
                return arg1[myRandomizer.Next(0, arg1.Count)];
            }
            ListOfPossibleretValue = SpreadOutResult;




            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> TempList = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
            TempList.Add(retValue);

            Tuple<int, List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>> arg4 = Utility.getHighestCountList(TempList, retValue);

            if (arg4.Item1 <= PreecdintTimeSpanWithSringIDCoun)
            {
                return retValue;
            }

            else
            {
                return getAveragedOutTIimeLine(ListOfPossibleretValue, arg4.Item1);
            }



        }


        List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> getMostSpreadOut(List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> arg1)
        {
            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> retValue = new List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
            int i = 0;

            int MaxCount = 0;
            foreach (List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> eachList in arg1)
            {
                i = 0;
                int CurrenCount = 0;

                foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in eachList)//initializes Total with the first element in the list
                {
                    CurrenCount += eachDictionary.Count;
                }

                if (CurrenCount >= MaxCount)
                {
                    if (CurrenCount > MaxCount)
                    {
                        retValue = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
                    }
                    retValue.Add(eachList);
                }

            }
            return retValue;
        }


        List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> getValidMatches(List<SubCalendarEvent> ListOfInterferringEvents, List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> PossibleMatches, Dictionary<TimeLine, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> ConstrainedList)
        {
            int MaxPossibleMatched = ListOfInterferringEvents.Count;
            int HighstSum = 0;
            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> retValue = new List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
            List<TimeLine> AllTimeLines = ConstrainedList.Keys.ToList();
            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> ListOfDict = ConstrainedList.Values.ToList();

            int j = 0;
            foreach (List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> eachList in PossibleMatches)
            {
                int CurrentSum = 0;
                int i = 0;

                foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in eachList)
                {
                    ICollection<KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AllData = eachDictionary;
                    foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in AllData)
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
                        retValue = new List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
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
            List<mTuple<bool, SubCalendarEvent>> retValue0 = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
            foreach (SubCalendarEvent eachSubCalendarEvent in Arg1)
            {
                retValue.Add(new TimeSpanWithStringID(eachSubCalendarEvent.ActiveDuration, eachSubCalendarEvent.ActiveDuration.Ticks.ToString()));
                retValue0.Add(new mTuple<bool, SubCalendarEvent>(false, eachSubCalendarEvent));
            }

            return new Tuple<List<TimeSpanWithStringID>, List<mTuple<bool, SubCalendarEvent>>>(retValue, retValue0);
        }


        int HighestSum = 0;
        List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> generateTreeCallsToSnugArray(Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> AvailableSubCalendarEvents, List<TimeLine> AllTimeLines, int TimeLineIndex, List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> SubCalendarEventsPopulatedIntoTimeLineIndex_SoFar, Dictionary<TimeLine, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> DictionaryOfTimelineAndSubcalendarEvents, Dictionary<TimeLine, Tuple<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>, Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> DictionaryOfTimelineAndConstrainedElements)//, List<SubCalendarEvent> usedSubCalendarEvensts)
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

            List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>> ListOfAllSnugPossibilitiesInRespectiveTImeLines = new List<List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>>>();
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
                    foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in AvailableSubCalendarEvents)
                    {
                        TOtalEvents += eachKeyValuePair.Value.Item1;
                    }
                }
                if (TOtalEvents == 1)
                {
                    ;
                }
            }

            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> MyPertinentSubcalendarEvents;
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> CantBeUsedInCurrentTimeLine = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> RestrictedToThisTimeLineElements = DictionaryOfTimelineAndConstrainedElements[AllTimeLines[TimeLineIndex]].Item1;



            TimeSpan TotalTimeUsedUpByConstrained = new TimeSpan(0);

            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in RestrictedToThisTimeLineElements)//HACK THIS does not take into account two clashing restricted events
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
            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in RestrictedToThisTimeLineElements)
            {

                MyPertinentSubcalendarEvents[eachKeyValuePair.Key].Item1 -= eachKeyValuePair.Value.Item1;
                TotalTimeUsedUpByConstrained += (TimeSpan.FromTicks(eachKeyValuePair.Value.Item2.timeSpan.Ticks * eachKeyValuePair.Value.Item1));
            }
            List<mTuple<int, TimeSpanWithStringID>> ListOfTimeSpanWithID_WithCounts = MyPertinentSubcalendarEvents.Values.ToList();


            SnugArray MySnugArray = new SnugArray(ListOfTimeSpanWithID_WithCounts, AllTimeLines[TimeLineIndex].TimelineSpan - TotalTimeUsedUpByConstrained);
            //SnugArray MySnugArray = new SnugArray(ConstrainedMySubcalendarEventTimespans, MySubcalendarEventTimespans, AllTimeLines[TimeLineIndex].TimelineSpan);



            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> AllSnugPossibilities = MySnugArray.MySnugPossibleEntries;

            AllSnugPossibilities = SnugArray.SortListSnugPossibilities_basedOnTimeSpan(AllSnugPossibilities);
            AllSnugPossibilities.Reverse();
            if (AllSnugPossibilities.Count > 1)
            {
                AllSnugPossibilities.RemoveRange(1, (AllSnugPossibilities.Count - 1));
            }

            foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachDictionary in AllSnugPossibilities)
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

                foreach (TimeSpan eachTimeSpan in RestrictedToThisTimeLineElements.Keys)
                {
                    if (eachDictionary.ContainsKey(eachTimeSpan))
                    {
                        eachDictionary[eachTimeSpan].Item1 += RestrictedToThisTimeLineElements[eachTimeSpan].Item1;
                    }
                    else
                    {
                        eachDictionary.Add(eachTimeSpan, new mTuple<int, TimeSpanWithStringID>(RestrictedToThisTimeLineElements[eachTimeSpan].Item1, RestrictedToThisTimeLineElements[eachTimeSpan].Item2));
                    }
                }
            }



            foreach (KeyValuePair<TimeSpan, mTuple<int, TimeSpanWithStringID>> eachKeyValuePair in CantBeUsedInCurrentTimeLine)//restores restricted for other timeLines into AvailableSubCalendarEvents
            {
                AvailableSubCalendarEvents[eachKeyValuePair.Key].Item1 += eachKeyValuePair.Value.Item1;
            }


            List<Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>> SerialIzedListOfSubCalendarEvents = AllSnugPossibilities;
            Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> AvailableSubCalendarEventsAfterRemovingAssingedSnugElements = new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>();






            if (SerialIzedListOfSubCalendarEvents.Count > 0)
            {

                foreach (Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>> AlreadyAssignedSubCalendarEvent in AllSnugPossibilities)
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
                AvailableSubCalendarEventsAfterRemovingAssingedSnugElements = Utility.NotInList_NoEffect(AvailableSubCalendarEvents, new Dictionary<TimeSpan, mTuple<int, TimeSpanWithStringID>>());//Hack this can be optimized... the whole "notinlist" call can be optimized as a call to AvailableSubCalendarEvents. Review to see if references to func are affected.
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

            for (int i = 0; i < DictOfSubCalEvent.Keys.Count; i++)//Goes through each dictionary index
            {
                List<List<List<SubCalendarEvent>>> NonTaintedListToWorkWith = retValue.ToList();
                List<List<List<SubCalendarEvent>>> MyTempList = new System.Collections.Generic.List<System.Collections.Generic.List<System.Collections.Generic.List<SubCalendarEvent>>>();
                foreach (List<SubCalendarEvent> ListOfClumpPermutation in DictOfSubCalEvent[i])
                {
                    List<List<List<SubCalendarEvent>>> TaintedListToWorkWith = NonTaintedListToWorkWith.ToList();
                    foreach (List<List<SubCalendarEvent>> ListSoFar in TaintedListToWorkWith)
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

        List<List<SubCalendarEvent>> Pseudo_generateTreeCallsToSnugArray(List<SubCalendarEvent> SortedAvailableSubCalEvents_Deadline, TimeLine BoundaryTimeLine)//, Dictionary<TimeLine, List<SubCalendarEvent>> DictionaryOfTimelineAndSubcalendarEvents)//, List<SubCalendarEvent> usedSubCalendarEvensts)
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
            foreach (List<SubCalendarEvent> AlreadyAssignedSubCalEvents in ClumpendListOfSubCalEvetns)
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






        List<CalendarEvent> getClashingCalendarEvent(List<CalendarEvent> MyListOfCalednarEvents, TimeLine MyTImeLine)
        {
            List<CalendarEvent> MyInterferringCaliendarEvents = getPertinentCalendarEvents(MyListOfCalednarEvents.ToArray(), MyTImeLine).ToList();
            List<CalendarEvent> ListOfCalendarEventsLargerThanTimeLine = new System.Collections.Generic.List<CalendarEvent>();

            foreach (CalendarEvent MyCalendarEvent in MyListOfCalednarEvents)
            {
                if (((MyCalendarEvent.End > MyTImeLine.Start) && (MyCalendarEvent.Start <= MyTImeLine.Start)) || ((MyTImeLine.Start > MyCalendarEvent.Start) && (MyTImeLine.Start <= MyCalendarEvent.Start)))
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
                int Arg2 = Arg1.IndexOf(eachmTuple.Item2);
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

                Dictionary<CalendarEvent, List<SubCalendarEvent>> MyListOfDictionaryOfCalendarEventAndSubCalendarEvent = DicitonaryTimeLineAndPertinentCalendarEventDictionary[MyTimeLine];
                List<CalendarEvent> MyListOfPertitnentCalendars = MyListOfDictionaryOfCalendarEventAndSubCalendarEvent.Keys.ToList();
                MyListOfPertitnentCalendars = MyListOfPertitnentCalendars.OrderBy(obj => obj.End).ToList();
                List<mTuple<bool, SubCalendarEvent>> MyListOfPertinentSubEvent = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
                foreach (CalendarEvent MyCalendarEvent in MyListOfPertitnentCalendars)
                {
                    List<SubCalendarEvent> InterFerringSubCalendarEventS = MyListOfDictionaryOfCalendarEventAndSubCalendarEvent[MyCalendarEvent];
                    if (MyCalendarEvent.Repeat.Enable)
                    {
                        List<CalendarEvent> MyListOfAffectedRepeatingCalendarEvent = getClashingCalendarEvent(MyCalendarEvent.Repeat.RecurringCalendarEvents.ToList(), MyTimeLine);



                        List<mTuple<bool, SubCalendarEvent>> ListOfAffectedSubcalendarEvents = new System.Collections.Generic.List<mTuple<bool, SubCalendarEvent>>();
                        foreach (CalendarEvent MyRepeatCalendarEvent in MyListOfAffectedRepeatingCalendarEvent)
                        {
                            SubCalendarEvent[] MyListOfSubCalendarEvents = MyRepeatCalendarEvent.ActiveSubEvents;
                            foreach (SubCalendarEvent PosibleClashingSubCalEvent in MyListOfSubCalendarEvents)
                            {
                                foreach (SubCalendarEvent eachInterFerringSubCalendarEvent in InterFerringSubCalendarEventS)
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
                        MyListOfPertinentSubEvent.AddRange(getmTupleSubCalendarEvent(MyListOfInterferringmTupleSubCalendarEvents, MyListOfDictionaryOfCalendarEventAndSubCalendarEvent[MyCalendarEvent]));

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
            int Sum = 0;
            foreach (List<SubCalendarEvent> EachTimeLineSubeventList in MySubCalEventInTimeLine)
            {
                Sum += EachTimeLineSubeventList.Count;
            }
            return Sum;
        }

        List<List<SubCalendarEvent>> SerializeListOfMatchingSubcalendarEvents(List<List<List<SubCalendarEvent>>> MyListofListOfListOfSubcalendar)
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
            List<SubCalendarEvent> MyNewList = new System.Collections.Generic.List<SubCalendarEvent>();
            foreach (SubCalendarEvent MySubCalendarEvent in MyCurrentList)
            {
                if (ListToCheck.IndexOf(MySubCalendarEvent) >= 0)
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

                if ((MyCalendarEvent.Start < VerifiableSpace.End) && (MyCalendarEvent.End > VerifiableSpace.Start))
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
            List<CalendarEvent> MyDeadlineSortedListOfCalendarEvents = QuickSortCalendarEventFunctionFromEnd(DictionaryCalendarEventAndJustInterferringSubCalendar.Keys.ToList(), 0, (DictionaryCalendarEventAndJustInterferringSubCalendar.Keys.Count - 1), (DictionaryCalendarEventAndJustInterferringSubCalendar.Keys.Count / 2));

            return new TimeLine();
        }










        List<List<SubCalendarEvent>> BuildListMatchingTimelineAndSubCalendarEvent(List<List<TimeSpanWithEventID>> ListOfSnugPossibilities, List<SubCalendarEvent> ListOfSubCalendarEvents, List<SubCalendarEvent> ConsrainedList)
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
                catch (Exception e)
                {

                    DictionaryOfTImespanandSubCalendarEvent.Add(MySubCalendar.ActiveSlot.BusyTimeSpan, new List<SubCalendarEvent>());
                    DictionaryOfTImespanandSubCalendarEvent[MySubCalendar.ActiveSlot.BusyTimeSpan].Add(MySubCalendar);
                    //DictionaryOfTImespanandSubCalendarEvent.Add(MyTimeSpan, new List<SubCalendarEvent>());
                }
            }




            foreach (List<TimeSpan> MySnugPossibility in ListOfSnugPossibilities)
            {
                List<List<SubCalendarEvent>> SnugPossibiltySubcalendarEvent = new List<List<SubCalendarEvent>>();
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
            foreach (SubCalendarEvent MySubCalendarEvent in MyListOfSubCalenedarEvents)
            {
                try
                {
                    MyDictionaryOfTimeSpanAndSubCalendarEvents.Add(MySubCalendarEvent.ActiveDuration, new List<SubCalendarEvent>());//Test to see if  the dictionary key is built with an object reference oir the value of the timespan. This is because the object is probably passed by referencxe
                    MyDictionaryOfTimeSpanAndSubCalendarEvents[MySubCalendarEvent.ActiveDuration].Add(MySubCalendarEvent);

                }
                catch (Exception ex)
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
                EventID MyEventID = new EventID(SortedByStartArrayOfBusyTimeLine[i].TimeLineID);
                string ParentCalendarEventID = MyEventID.getLevelID(0);

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
                EventID MyEventID = new EventID(SortedByStartArrayOfBusyTimeLine[i].TimeLineID);
                string ParentCalendarEventID = MyEventID.getLevelID(0);

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



        List<IDefinedRange>[] getEdgeElements(TimeLine MyRangeOfTimeLine, IEnumerable<IDefinedRange> ArrayOfTInterferringime)
        {
            /*
             * Function looks through interferring elements collection, it checks for the elements that crossover the beginning datetime and end datetime
             
             */

            List<IDefinedRange> ListOfEdgeElements = new List<IDefinedRange>();
            List<IDefinedRange> StartEdge = new List<IDefinedRange>();
            List<IDefinedRange> EndEdge = new List<IDefinedRange>();
            int i = 0;

            ListOfEdgeElements = ArrayOfTInterferringime.Where(obj => (obj.IsDateTimeWithin(MyRangeOfTimeLine.Start) || obj.IsDateTimeWithin(MyRangeOfTimeLine.End))).ToList();


            /*for (; i < ArrayOfTInterferringime.Length; i++)
            {
                if (!MyRangeOfTimeLine.IsTimeLineWithin(ArrayOfTInterferringime[i]))
                {
                    ListOfEdgeElements.Add(ArrayOfTInterferringime[i]);
                }
            }*/

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

            List<IDefinedRange>[] StartAndEndEdgeList = new List<IDefinedRange>[2];
            StartAndEndEdgeList[0] = StartEdge;
            StartAndEndEdgeList[1] = EndEdge;
            return StartAndEndEdgeList;
        }

        private SubCalendarEvent[] getInterferringSubEvents(CalendarEvent MyCalendarEvent, List<CalendarEvent> NonCOmmitedCalendarEvemts)
        {
            if (MyCalendarEvent.RepetitionStatus)
            {
                throw new Exception("Weird error, found repeating event repeaing evemt");
            }
            return getInterferringSubEvents(MyCalendarEvent.RangeTimeLine, NonCOmmitedCalendarEvemts);
        }

        public IEnumerable<SubCalendarEvent> getInterferringSubEvents(TimeLine EventRange, IEnumerable<SubCalendarEvent> PossibleSubCalEVents)//gets list of subcalendar event in which the busytimeline interfer with Event range
        {
            return PossibleSubCalEVents.Where(obj => (EventRange.InterferringTimeLine(obj.ActiveSlot) != null));
        }


        private SubCalendarEvent[] getInterferringSubEvents(TimeLine EventRange, List<CalendarEvent> NonCommitedCalendarEvemts = null)
        {

#if enableMultithreading
            ConcurrentBag<SubCalendarEvent> MyArrayOfInterferringSubCalendarEvents = new ConcurrentBag<SubCalendarEvent>();//List that stores the InterFerring List

            int lengthOfCalendarSubEvent = 0;


            Parallel.ForEach(AllEventDictionary, MyCalendarEventDictionaryEntry =>
            {
                int i = 0;
                if (MyCalendarEventDictionaryEntry.Value.RepetitionStatus)
                {
                    SubCalendarEvent[] ArrayOfSubcalendarEventsFromRepeatingEvents = MyCalendarEventDictionaryEntry.Value.AllActiveRepeatSubCalendarEvents.Where(obj => obj != null).ToArray();//hack alert you should be able to remove the LINQ test for null
                    lengthOfCalendarSubEvent = ArrayOfSubcalendarEventsFromRepeatingEvents.Length;
                    Parallel.ForEach(MyCalendarEventDictionaryEntry.Value.AllActiveRepeatSubCalendarEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.RangeTimeLine) != null), EacSubCal =>
                    {
                        MyArrayOfInterferringSubCalendarEvents.Add(EacSubCal);
                    }
                    );
                }
                else
                {
                    SubCalendarEvent[] ArrayOfSubcalendarEventsFromNonRepeatingEvents = MyCalendarEventDictionaryEntry.Value.AllActiveSubEvents.Where(obj => obj != null).ToArray();//hack alert you should be able to remove the LINQ test for null
                    lengthOfCalendarSubEvent = ArrayOfSubcalendarEventsFromNonRepeatingEvents.Length;


                    Parallel.ForEach(MyCalendarEventDictionaryEntry.Value.AllActiveSubEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.RangeTimeLine) != null), EacSubCal =>
                    {
                        MyArrayOfInterferringSubCalendarEvents.Add(EacSubCal);
                    }
                    );
                }
            }
            );

            if (NonCommitedCalendarEvemts != null)
            {
                foreach (CalendarEvent eachCalendarEvent in NonCommitedCalendarEvemts)
                {

                    int i = 0;
                    if (eachCalendarEvent.RepetitionStatus)
                    {
                        lengthOfCalendarSubEvent = eachCalendarEvent.AllActiveRepeatSubCalendarEvents.Length;
                        SubCalendarEvent[] ArrayOfSubcalendarEventsFromRepeatingEvents = eachCalendarEvent.AllActiveRepeatSubCalendarEvents;

                        Parallel.ForEach(eachCalendarEvent.AllActiveRepeatSubCalendarEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.RangeTimeLine) != null), EacSubCal =>
                        {
                            MyArrayOfInterferringSubCalendarEvents.Add(EacSubCal);
                        }
                        );



                        /*
                        for (i = 0; i < lengthOfCalendarSubEvent; i++)
                        {
                            //if ((EventRange.IsDateTimeWithin(ArrayOfSubcalendarEventsFromRepeatingEvents[i].Start)) || (EventRange.IsDateTimeWithin(ArrayOfSubcalendarEventsFromRepeatingEvents[i].End)))
                            if (EventRange.InterferringTimeLine(ArrayOfSubcalendarEventsFromRepeatingEvents[i].RangeTimeLine) != null)
                            {
                                MyArrayOfInterferringSubCalendarEvents.Add(ArrayOfSubcalendarEventsFromRepeatingEvents[i]);
                            }
                        }*/
                    }
                    else
                    {
                        SubCalendarEvent[] ArrayOfSubcalendarEventsFromNonRepeatingEvents = eachCalendarEvent.AllActiveSubEvents.Where(obj => obj != null).ToArray();//hack alert you should be able to remove the LINQ test for null
                        lengthOfCalendarSubEvent = ArrayOfSubcalendarEventsFromNonRepeatingEvents.Length;

                        Parallel.ForEach(eachCalendarEvent.AllActiveSubEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.RangeTimeLine) != null), EacSubCal =>
                        {
                            MyArrayOfInterferringSubCalendarEvents.Add(EacSubCal);
                        }
                        );



                        /*for (i = 0; i < lengthOfCalendarSubEvent; i++)
                        {

                            //if ((EventRange.IsDateTimeWithin(MyCalendarEventDictionaryEntry.Value.AllEvents[i].Start)) || (EventRange.IsDateTimeWithin(MyCalendarEventDictionaryEntry.Value.AllEvents[i].End)))
                            //if (MyCalendarEventDictionaryEntry.Value.AllEvents[i]!=null)
                            {
                                if (EventRange.InterferringTimeLine(ArrayOfSubcalendarEventsFromNonRepeatingEvents[i].RangeTimeLine) != null)
                                {
                                    MyArrayOfInterferringSubCalendarEvents.Add(ArrayOfSubcalendarEventsFromNonRepeatingEvents[i]);
                                }
                            }
                        }*/
                    }
                }
            }

            return MyArrayOfInterferringSubCalendarEvents.ToArray();

















            /////////////////



           
#else
            List<SubCalendarEvent> MyArrayOfInterferringSubCalendarEvents = new List<SubCalendarEvent>(0);//List that stores the InterFerring List
            int lengthOfCalendarSubEvent = 0;

            IEnumerable<KeyValuePair<string, CalendarEvent>> content = AllEventDictionary.Where(obj => obj.Value.isActive);

            foreach (KeyValuePair<string, CalendarEvent> MyCalendarEventDictionaryEntry in AllEventDictionary.Where(obj=>obj.Value.isActive))
            {
                int i = 0;
                if (MyCalendarEventDictionaryEntry.Value.ID == "373")
                {
                    ;
                }
                if (MyCalendarEventDictionaryEntry.Value.RepetitionStatus)
                {
                    if (MyCalendarEventDictionaryEntry.Value.ID == "373")
                    {
                        ;
                    }

                    SubCalendarEvent[] ArrayOfSubcalendarEventsFromRepeatingEvents = MyCalendarEventDictionaryEntry.Value.ActiveRepeatSubCalendarEvents.Where(obj => obj != null).ToArray();//hack alert you should be able to remove the LINQ test for null
                    lengthOfCalendarSubEvent = ArrayOfSubcalendarEventsFromRepeatingEvents.Length;
                    MyArrayOfInterferringSubCalendarEvents.AddRange(MyCalendarEventDictionaryEntry.Value.ActiveRepeatSubCalendarEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.RangeTimeLine) != null).ToList());
                }
                else
                {
                    SubCalendarEvent[] ArrayOfSubcalendarEventsFromNonRepeatingEvents = MyCalendarEventDictionaryEntry.Value.ActiveSubEvents.Where(obj => obj != null).ToArray();//hack alert you should be able to remove the LINQ test for null
                    lengthOfCalendarSubEvent = ArrayOfSubcalendarEventsFromNonRepeatingEvents.Length;
                    MyArrayOfInterferringSubCalendarEvents.AddRange(MyCalendarEventDictionaryEntry.Value.ActiveSubEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.RangeTimeLine) != null).ToList());
                }
            }

            if (NonCommitedCalendarEvemts != null)
            {
                foreach (CalendarEvent eachCalendarEvent in NonCommitedCalendarEvemts)
                {

                    int i = 0;
                    if (eachCalendarEvent.RepetitionStatus)
                    {
                        lengthOfCalendarSubEvent = eachCalendarEvent.ActiveRepeatSubCalendarEvents.Length;
                        SubCalendarEvent[] ArrayOfSubcalendarEventsFromRepeatingEvents = eachCalendarEvent.ActiveRepeatSubCalendarEvents;

                        MyArrayOfInterferringSubCalendarEvents.AddRange(eachCalendarEvent.ActiveRepeatSubCalendarEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.RangeTimeLine) != null).ToList());
                        /*for (i = 0; i < lengthOfCalendarSubEvent; i++)
                        {
                            //if ((EventRange.IsDateTimeWithin(ArrayOfSubcalendarEventsFromRepeatingEvents[i].Start)) || (EventRange.IsDateTimeWithin(ArrayOfSubcalendarEventsFromRepeatingEvents[i].End)))
                            if (EventRange.InterferringTimeLine(ArrayOfSubcalendarEventsFromRepeatingEvents[i].RangeTimeLine) != null)
                            {
                                MyArrayOfInterferringSubCalendarEvents.Add(ArrayOfSubcalendarEventsFromRepeatingEvents[i]);
                            }
                        }*/
                    }
                    else
                    {
                        SubCalendarEvent[] ArrayOfSubcalendarEventsFromNonRepeatingEvents = eachCalendarEvent.ActiveSubEvents.Where(obj => obj != null).ToArray();//hack alert you should be able to remove the LINQ test for null
                        lengthOfCalendarSubEvent = ArrayOfSubcalendarEventsFromNonRepeatingEvents.Length;


                        MyArrayOfInterferringSubCalendarEvents.AddRange(eachCalendarEvent.ActiveSubEvents.Where(obj => obj != null).Where(obj => EventRange.InterferringTimeLine(obj.RangeTimeLine) != null).ToList());
                        /*
                        for (i = 0; i < lengthOfCalendarSubEvent; i++)
                        {

                            //if ((EventRange.IsDateTimeWithin(MyCalendarEventDictionaryEntry.Value.AllEvents[i].Start)) || (EventRange.IsDateTimeWithin(MyCalendarEventDictionaryEntry.Value.AllEvents[i].End)))
                            //if (MyCalendarEventDictionaryEntry.Value.AllEvents[i]!=null)
                            {
                                if (EventRange.InterferringTimeLine(ArrayOfSubcalendarEventsFromNonRepeatingEvents[i].RangeTimeLine) != null)
                                {
                                    MyArrayOfInterferringSubCalendarEvents.Add(ArrayOfSubcalendarEventsFromNonRepeatingEvents[i]);
                                }
                            }
                        }*/
                    }
                }
            }

            return MyArrayOfInterferringSubCalendarEvents.ToArray();
#endif
        }



        public TimeLine[] SplitFreeSpotsInToSubEventTimeSlots(TimeLine[] AllAvailableFreeSpots, int NumberOfAllotments, TimeSpan TotalActiveDuration)//function is responsible for dividing busy timeline. Also sapcing them out
        {
            TimeLine[] MyArrayOfToBeAssignedTimeLine = new TimeLine[NumberOfAllotments];
            double IdealTimePerAllotment = TotalActiveDuration.TotalSeconds / NumberOfAllotments;
            Dictionary<TimeLine, int> TimeLineAndFitCount = new Dictionary<TimeLine, int>();
            int i = 0;
            int FitCount = 0;
            for (; i < AllAvailableFreeSpots.Length; i++)
            {
                FitCount = (int)(AllAvailableFreeSpots[i].TimelineSpan.TotalSeconds / IdealTimePerAllotment);
                TimeLineAndFitCount.Add(AllAvailableFreeSpots[i], FitCount);
            }
            i = 0;
            int[] AllFitCount = TimeLineAndFitCount.Values.ToArray();
            int myTotalFitCount = 0;
            for (; i < AllFitCount.Length; i++)
            {
                myTotalFitCount += AllFitCount[i];
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
            int j = 0;
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

            TimeLine[][] MyArrayOfTimeLines = TimeLineDictionary.Values.ToArray();

            k = 0;
            i = 0;
            //for (; i < MyArrayOfToBeAssignedTimeLine.Length; )
            {
                j = 0;
                for (; ((j < MyArrayOfTimeLines.Length) && (i < MyArrayOfToBeAssignedTimeLine.Length)); j++)
                {

                    foreach (TimeLine MyTimeLine in MyArrayOfTimeLines[j])
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
                //MessageBox.Show("ALERT!!! NOT ALL SUBCALENDAR EVENTS WILL BE ASSIGNED");
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
            TimeLine MyReferenceRange = new TimeLine();
            double TotalMilliseconds = i * (MySpanForEachSection.TotalMilliseconds);
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
            TimeLine CentralizedTimeline = new TimeLine();
            if (Difference.TotalMilliseconds < 0)
            {
                //MessageBox.Show("Cannot generate CentralizeYourSelfWithinRange TimeLine Because Difference is less than zero.\nWill Not Fit!!!");
                throw (new System.Exception("Cannot generate CentralizeYourSelfWithinRange TimeLine Because Difference is less than zero.\nWill Not Fit!!!"));
                //return CentralizedTimeline;
            }
            DateTime MyStart = Range.Start.AddSeconds(Difference.TotalSeconds / 2);
            CentralizedTimeline = new TimeLine(MyStart, MyStart.Add(Centralized));
            return CentralizedTimeline;
        }

        private SubCalendarEvent AssignSubEventTimeSlot(SubCalendarEvent MySubEvent)
        {
            BusyTimeLine[] ArrayOfInvadingEvents = CheckIfMyRangeAlreadyHasSchedule(CompleteSchedule.OccupiedSlots, MySubEvent);
            TimeLine[] AvailableFreeSpots = getAllFreeSpots(new TimeLine(MySubEvent.Start, MySubEvent.End));
            for (int i = 0; i < AvailableFreeSpots.Length; i++)
            {
                if (AvailableFreeSpots[i].TimelineSpan > (MySubEvent.ActiveDuration))
                {
                    DateTime DurationStartTime;
                    TimeSpan MyTimeSpan = new TimeSpan((((AvailableFreeSpots[i].TimelineSpan - MySubEvent.ActiveDuration).Milliseconds) / 2) * 10000);
                    DurationStartTime = AvailableFreeSpots[i].Start.Add(MyTimeSpan);
                    MySubEvent.ActiveSlot = new BusyTimeLine(MySubEvent.ID, DurationStartTime, DurationStartTime.Add(MySubEvent.ActiveDuration));
                }
            }
            return MySubEvent;
        }

        /*QUICK SORT SECTION START*/



        static public List<BusyTimeLine> QuickSortFunctionFromStart(List<BusyTimeLine> MyArray, int LeftIndexPassed, int RightIndexPassed, int PivotPassed)
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
                        if (MyArray[RightIndexPassed].Start < MyArray[NextPivot].Start)
                        {
                            Holder = MyArray[NextPivot];
                            MyArray[NextPivot] = MyArray[RightIndexPassed];
                            MyArray[RightIndexPassed] = Holder;
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
            //sorts elements using the start DATe time in ascending order i.e earliest start time first
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

            //sorts elements using the start DATe time in ascending order i.e earliest start time first
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
            List<BusyTimeLine> InvadingEvents = new List<BusyTimeLine>();
            int i = 0;
            for (; i < BusySlots.Length; i++)
            {
                if ((BusySlots[i].End > MySubEvent.Start) && (BusySlots[i].End < MySubEvent.End))
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

        static public List<BusyTimeLine> SortBusyTimeline(List<BusyTimeLine> MyUnsortedEvents, bool StartOrEnd)//True is from start. False is from end
        {
            int MiddleRoundedDown = ((MyUnsortedEvents.Count - 1) / 2);
            if (StartOrEnd)
            { return QuickSortFunctionFromStart(MyUnsortedEvents, 0, MyUnsortedEvents.Count - 1, MiddleRoundedDown); }
            else
            { return QuickSortFunctionFromEnd(MyUnsortedEvents, 0, MyUnsortedEvents.Count - 1, MiddleRoundedDown); }
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
            DateTime ReferenceTime = CompleteSchedule.Start;
            /*if (MyTimeLine.Start < CompleteSchedule.Start)
            {
                ReferenceTime = MyTimeLine.Start;
            }*/

            for (int i = 0; i < (AllBusySlots.Length); i++)//Free Spots Are only between two busy Slots. So Index Counter starts from 1 get start of second busy
            {

                AllFreeSlots[i] = new TimeLine(ReferenceTime, AllBusySlots[i].Start);
                ReferenceTime = AllBusySlots[i].End;
                //AllFreeSlots[i] = new TimeLine(AllBusySlots[i].End, AllBusySlots[i + 1].Start.AddMilliseconds(-1));
            }
            FinalCompleteScheduleDate = CompleteSchedule.End;
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



        DateTime UpdateNow(DateTime UpdatedNow)
        {
            Now = UpdatedNow;
            return Now;
        }


        public Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ProcrastinateJustAnEvent(string EventID, TimeSpan RangeOfPush)
        {

            CalendarEvent ProcrastinateEvent = getCalendarEvent(EventID);
            SubCalendarEvent ReferenceSubEvent = getSubCalendarEvent(EventID);
            EventID SubEventID = new EventID(EventID);
            DateTime ReferenceStart = Now > ReferenceSubEvent.Start ? Now : ReferenceSubEvent.Start;
            //BusyTimeLine[] allBusySlots = CompleteSchedule.OccupiedSlots;
            //IEnumerable<BusyTimeLine> AllLesssTnanRefTime = allBusySlots.Where(obj => obj.End < ReferenceStart);


            ReferenceStart = UpdateNow(ReferenceStart);
            DateTime StartTimeOfProcrastinate = ReferenceStart + RangeOfPush;

            if (StartTimeOfProcrastinate > ReferenceSubEvent.getCalendarEventRange.End)
            {
                return new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(new CustomErrors(true, "Procrastinated deadline event is before end of selected timeline space"), AllEventDictionary);
            }


            if (ProcrastinateEvent.RepetitionStatus)
            {
                ProcrastinateEvent = ProcrastinateEvent.getRepeatedCalendarEvent(SubEventID.getStringIDAtLevel(1));
            }

            Dictionary<string, CalendarEvent> AllEventDictionary_Cpy = new Dictionary<string, CalendarEvent>();
            AllEventDictionary_Cpy = AllEventDictionary.ToDictionary(obj => obj.Key, obj => obj.Value.createCopy());

            List<SubCalendarEvent> AllValidSubCalEvents = ProcrastinateEvent.ActiveSubEvents.Where(obj => obj.End > ReferenceSubEvent.Start).ToList();



            //ProcrastinateEvent.removeSubCalEvents(AllValidSubCalEvents);
            ProcrastinateEvent.DisableSubEvents(AllValidSubCalEvents);
            TimeSpan TotalActiveDuration = Utility.SumOfActiveDuration(AllValidSubCalEvents);
            //CalendarEvent(string NameEntry, string StartTime, DateTime StartDateEntry, string EndTime, DateTime EventEndDateEntry, string eventSplit, string PreDeadlineTime, string EventDuration, Repetition EventRepetitionEntry, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag,Location EventLocation)
            CalendarEvent ScheduleUpdated = new CalendarEvent(ProcrastinateEvent.Name, StartTimeOfProcrastinate.ToString("hh:mm tt"), StartTimeOfProcrastinate, ProcrastinateEvent.End.ToString("hh:mm tt"), ProcrastinateEvent.End, AllValidSubCalEvents.Count.ToString(), ProcrastinateEvent.PreDeadline.ToString(), TotalActiveDuration.ToString(), new Repetition(), true, ProcrastinateEvent.Rigid, ProcrastinateEvent.Preparation.ToString(), true, ProcrastinateEvent.myLocation, true, new EventDisplay(), new MiscData(), false);

            HashSet<SubCalendarEvent> NotDoneYet = getNoneDoneYetBetweenNowAndReerenceStartTIme();
            ScheduleUpdated = EvaluateTotalTimeLineAndAssignValidTimeSpots(ScheduleUpdated, NotDoneYet);

            SubCalendarEvent[] UpdatedSubCalevents = ScheduleUpdated.ActiveSubEvents;

            for (int i = 0; i < AllValidSubCalEvents.Count; i++)//updates the subcalevents
            {
                SubCalendarEvent updatedSubCal = new SubCalendarEvent(AllValidSubCalEvents[i].ID, UpdatedSubCalevents[i].Start, UpdatedSubCalevents[i].End, UpdatedSubCalevents[i].ActiveSlot, UpdatedSubCalevents[i].Rigid, AllValidSubCalEvents[i].isEnabled, AllValidSubCalEvents[i].UIParam, AllValidSubCalEvents[i].Notes, AllValidSubCalEvents[i].isComplete, UpdatedSubCalevents[i].myLocation, ProcrastinateEvent.RangeTimeLine);
                AllValidSubCalEvents[i].UpdateThis(updatedSubCal);
            }

            ProcrastinateEvent.EnableSubEvents(AllValidSubCalEvents);

            if (ScheduleUpdated.ErrorStatus)
            {
                LogStatus(ScheduleUpdated, "Procrastinate Single Event");
            }

            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue = new Tuple<CustomErrors, Dictionary<string, CalendarEvent>>(ScheduleUpdated.Error, AllEventDictionary);
            AllEventDictionary = AllEventDictionary_Cpy;
            return retValue;
        }






        //public XmlElement CreateEventScheduleNode(CalendarEvent MyEvent, XmlDocument xmldoc)




        static TimeLine ScheduleTimeline = new TimeLine();


        public void LogStatus(CalendarEvent triggerEvent, string Trigger)//writes to an XML Log file. Takes calendar event as an argument
        {
#if EnableClashLog
            
            XmlDocument xmldoc = new XmlDocument();
            string LogFile=Now.ToString();
            LogFile = "..\\..\\CustomErrorLogs\\" + LogFile.Replace('/', '_') + "_" + Trigger + ".xml";
            LogFile=LogFile.Replace(':','_');
            try
            {
                xmldoc.Load(LogFile);
            }
            catch
            {
                XmlDocument xmldoc1 = new XmlDocument();
                XmlElement MyEventScheduleNode = xmldoc1.CreateElement("LogReport");

                
                MyEventScheduleNode.PrependChild(xmldoc1.CreateElement("Error"));
                MyEventScheduleNode.ChildNodes[0].InnerText = triggerEvent.ErrorMessage;
                MyEventScheduleNode.PrependChild(xmldoc1.CreateElement("Action"));
                MyEventScheduleNode.ChildNodes[0].InnerText = Trigger;
                MyEventScheduleNode.PrependChild(xmldoc1.CreateElement("TriggerCalEvent"));
                XmlElement EventScheduleNode1 = CreateEventScheduleNode(triggerEvent);

                XmlNode MyImportedNode1 = xmldoc1.ImportNode(EventScheduleNode1 as XmlNode, true);
                MyEventScheduleNode.ChildNodes[0].PrependChild(MyImportedNode1);
                //xmldoc1.DocumentElement.SelectSingleNode("/LogReport/TriggerCalEvent").AppendChild(MyImportedNode1);
                MyEventScheduleNode.PrependChild(xmldoc1.CreateElement("CurrentCalEvents"));
                //xmldoc=xmldoc1;
                XmlNode initializedNode = xmldoc.ImportNode(MyEventScheduleNode as XmlNode, true);
                xmldoc.AppendChild(initializedNode);
                xmldoc.Save(LogFile);
            }


            XmlDocument doc = new XmlDocument();
            string NameOfFile = "MyEventLog.xml";
            
            doc.Load(NameOfFile);
            XmlNode RootNode = doc.DocumentElement.SelectSingleNode("/ScheduleLog");
            XmlNode MyImportedNode = xmldoc.ImportNode(RootNode as XmlNode, true);
            xmldoc.DocumentElement.SelectSingleNode("/LogReport/CurrentCalEvents").AppendChild(MyImportedNode);
            xmldoc.Save(LogFile);
#endif
        }




        //Properties

        public int LastScheduleIDNumber
        {
            get
            {
                return (myAccount.LastEventTopNodeID);
            }
        }

        public bool isScheduleLoadSuccessful
        {
            get 
            {
                return myAccount.Status;
            }
        }
    }
}
