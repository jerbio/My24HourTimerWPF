using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    public class SubCalendarEvent : CalendarEvent
    {
        EventID SubEventID;
        BusyTimeLine BusyFrame;
        TimeSpan AvailablePreceedingFreeSpace;
        TimeLine CalendarEventRange;
        Location EventLocation;
        protected EventDisplay UIParams = new EventDisplay();
        bool Enabled = true;

        #region Classs Constructor
        public SubCalendarEvent()
        { }
        /*
        public SubCalendarEvent(TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID, Location EventLocation =null, TimeLine RangeOfSubCalEvent = null)
        {
            CalendarEventRange = RangeOfSubCalEvent;
            //string eventName, TimeSpan EventDuration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = Event_Duration;
            PrepTime = EventPrepTime;
            


//            SubEventID = new EventID(new string[] { myParentID, EventIDGenerator.generate().ToString() });
            if (myParentID == "16")
            {
                ;
            }

            SubEventID = new EventID(myParentID+"_"+EventIDGenerator.generate().ToString());
            EventSequence = new EventTimeLine(SubEventID.ToString(), StartDateTime, EndDateTime);
        }*/

        public SubCalendarEvent(EventID ParentID)
        {
            SubEventID = new EventID(ParentID + "_" + EventIDGenerator.generate().ToString());
            BusyFrame= new BusyTimeLine();
            AvailablePreceedingFreeSpace=new TimeSpan();
            CalendarEventRange = new TimeLine();
            EventLocation= new Location();
        }

        public SubCalendarEvent(TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, bool Enabled,Location EventLocation =null, TimeLine RangeOfSubCalEvent = null)
        {
            CalendarEventRange = RangeOfSubCalEvent;
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = Event_Duration;
            PrepTime = EventPrepTime;
            if (myParentID == "16")
            {
                ;
            }
            
            SubEventID = new EventID(myParentID + "_" + EventIDGenerator.generate().ToString());
            BusyFrame = new BusyTimeLine(this.ID, StartDateTime, EndDateTime);//this is because in current implementation busy frame is the same as CalEvent frame
            this.EventLocation = EventLocation;
            EventSequence = new EventTimeLine(SubEventID.ToString(), StartDateTime, EndDateTime);
            RigidSchedule = Rigid;
            this.Enabled = Enabled;
        }

        public SubCalendarEvent(string MySubEventID, BusyTimeLine MyBusylot, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID, Location EventLocation =null, TimeLine RangeOfSubCalEvent = null)
        {
            CalendarEventRange = RangeOfSubCalEvent;
            //string eventName, TimeSpan EventDuration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = MyBusylot.End - MyBusylot.Start;
            BusyFrame = MyBusylot;
            PrepTime = EventPrepTime;
            SubEventID = new EventID(MySubEventID.Split('_'));
            EventSequence = new EventTimeLine(SubEventID.ToString(), StartDateTime, EndDateTime);
            this.EventLocation = EventLocation;
        }
        /*public SubCalendarEvent(string MySubEventID, DateTime EventStart, DateTime EventDeadline, BusyTimeLine SubEventBusy, Location EventLocation =null, TimeLine RangeOfSubCalEvent = null)
        {
            CalendarEventRange = RangeOfSubCalEvent;
            SubEventID = new EventID(MySubEventID.Split('_'));
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = SubEventBusy.TimelineSpan;
            BusyFrame = SubEventBusy;
        }*/

        public SubCalendarEvent(string MySubEventID, BusyTimeLine MyBusylot, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, Location EventLocation =null, TimeLine RangeOfSubCalEvent = null, bool Enabled =false)
        {
            CalendarEventRange = RangeOfSubCalEvent;
            //string eventName, TimeSpan EventDuration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = MyBusylot.End - MyBusylot.Start;
            BusyFrame = MyBusylot;
            PrepTime = EventPrepTime;
            SubEventID = new EventID(MySubEventID.Split('_'));
            this.EventLocation = EventLocation;
            if (myParentID == "16")
            {
                ;
            }
            this.Enabled = Enabled;
            EventSequence = new EventTimeLine(SubEventID.ToString(), StartDateTime, EndDateTime);
            RigidSchedule = Rigid;
        }

        public SubCalendarEvent(string MySubEventID, DateTime EventStart, DateTime EventDeadline, BusyTimeLine SubEventBusy, bool Rigid,bool Enabled, Location EventLocation =null, TimeLine RangeOfSubCalEvent = null)
        {
            CalendarEventRange = RangeOfSubCalEvent;
            SubEventID = new EventID(MySubEventID.Split('_'));
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = SubEventBusy.TimelineSpan;
            BusyFrame = SubEventBusy;
            RigidSchedule = Rigid;
            this.Enabled = Enabled;
            this.EventLocation = EventLocation;
        }
        #endregion


        #region Class functions

        public override string ToString()
        {
            return this.Start.ToString() + " - " + this.End.ToString() + "::" + this.ID + "\t\t::" + this.ActiveDuration.ToString();
        }

        public static int CompareByEndDate(SubCalendarEvent SubCalendarEvent1, SubCalendarEvent SubCalendarEvent2)
        {
            return SubCalendarEvent1.End.CompareTo(SubCalendarEvent2.End);
        }

        public static int CompareByStartDate(SubCalendarEvent SubCalendarEvent1, SubCalendarEvent SubCalendarEvent2)
        {
            return SubCalendarEvent1.Start.CompareTo(SubCalendarEvent2.Start);
        }
        /*
        public static bool operator ==(SubCalendarEvent arg1, SubCalendarEvent arg2)
        {
            return arg1.ID == arg2.ID;
        }

        public static bool operator !=(SubCalendarEvent arg1, SubCalendarEvent arg2)
        {
            return arg1.ID != arg2.ID;
        }
        */
        public override void ReassignTime(DateTime StartTime, DateTime EndTime)
        {
            EndDateTime = (EndTime);
            StartDateTime = StartTime;
            BusyFrame = new BusyTimeLine(SubEventID.ToString(), StartTime, EndTime);
        }


        public bool IsDateTimeWithin(DateTime DateTimeEntry)
        {
            return RangeTimeLine.IsDateTimeWithin(DateTimeEntry);
        }

        public SubCalendarEvent createCopy()
        {
            SubCalendarEvent MySubCalendarEventCopy = new SubCalendarEvent(this.ID, new DateTime(Start.Ticks), new DateTime(End.Ticks), BusyFrame.CreateCopy(), this.RigidSchedule, this.isEnabled, this.LocationData, new TimeLine(CalendarEventRange.Start, CalendarEventRange.End));
            //MySubCalendarEventCopy.LocationData = LocationData;//note check for possible reference issues for future versions
            /*MySubCalendarEventCopy.SubEventID = SubEventID;
            MySubCalendarEventCopy.BusyFrame = BusyFrame;
            MySubCalendarEventCopy.StartDateTime = StartDateTime;
            MySubCalendarEventCopy.EndDateTime = EndDateTime;
            MySubCalendarEventCopy.EventDuration = EventDuration;
            MySubCalendarEventCopy.RigidSchedule = RigidSchedule;
            MySubCalendarEventCopy.SchedulStatus = SchedulStatus;
            MySubCalendarEventCopy.otherPartyID = otherPartyID;
            MySubCalendarEventCopy.EventPreDeadline = EventPreDeadline;
            MySubCalendarEventCopy.EventRepetition = EventRepetition;*/
            MySubCalendarEventCopy.ThirdPartyID = this.ThirdPartyID;
            return MySubCalendarEventCopy;
        }

        override public void updateEventSequence()
        {
            EventSequence = new TimeLine(this.Start, this.End);
            EventSequence.AddBusySlots(BusyFrame);
        }

        public static TimeSpan TotalActiveDuration(ICollection<SubCalendarEvent> ListOfSubCalendarEvent)
        {
            TimeSpan TotalTimeSpan = new TimeSpan(0);
            
            foreach (SubCalendarEvent mySubCalendarEvent in ListOfSubCalendarEvent)
            {
                TotalTimeSpan=TotalTimeSpan.Add(mySubCalendarEvent.ActiveDuration);
            }

            return TotalTimeSpan;
        }

        public bool PinSubEventsToStart(TimeLine MyTimeLine)
        {
            TimeSpan SubCalendarTimeSpan = new TimeSpan();
            DateTime ReferenceStartTime = new DateTime();
            DateTime ReferenceEndTime = new DateTime();

            ReferenceStartTime = MyTimeLine.Start;
            if (this.getCalendarEventRange.Start > MyTimeLine.Start)
            {
                ReferenceStartTime = this.getCalendarEventRange.Start;
            }

            ReferenceEndTime = this.getCalendarEventRange.End;
            if (this.getCalendarEventRange.End > MyTimeLine.End)
            {
                ReferenceEndTime = MyTimeLine.End;
            }

            /*foreach (SubCalendarEvent MySubCalendarEvent in MySubCalendarEventList)
            {
                SubCalendarTimeSpan = SubCalendarTimeSpan.Add(MySubCalendarEvent.ActiveDuration);//  you might be able to combine the implementing for lopp with this in order to avoid several loops
            }*/
            TimeSpan TimeDifference = (ReferenceEndTime - ReferenceStartTime);

            if (this.Rigid)
            {
                return true;
            }

            if (this.EventDuration > TimeDifference)
            {
                return false;
                //throw new Exception("Oh oh check PinSubEventsToStart Subcalendar is longer than available timeline");
            }
            if ((ReferenceStartTime > this.getCalendarEventRange.End) || (ReferenceEndTime < this.getCalendarEventRange.Start))
            {
                return false;
                //throw new Exception("Oh oh Calendar event isn't Timeline range. Check PinSubEventsToEnd :(");
            }

            List<BusyTimeLine> MyActiveSlot = new List<BusyTimeLine>();
            //foreach (SubCalendarEvent MySubCalendarEvent in MySubCalendarEventList)
            
                this.StartDateTime= ReferenceStartTime;
                this.EndDateTime = this.StartDateTime + this.ActiveDuration;
                //this.ActiveSlot = new BusyTimeLine(this.ID, (this.StartDateTime), this.EndDateTime);
                TimeSpan BusyTimeLineShift = this.StartDateTime - ActiveSlot.Start;
                ActiveSlot.shiftTimeline(BusyTimeLineShift);
                return true;
        }

        public bool UpdateThis(SubCalendarEvent SubEventEntry)
        {
            if ((this.ID == SubEventEntry.ID)&&canExistWithinTimeLine(SubEventEntry.getCalendarEventRange))
            {
                StartDateTime= SubEventEntry.Start;
                EndDateTime= SubEventEntry.End;
                BusyFrame.updateBusyTimeLine(SubEventEntry.BusyFrame);
                AvailablePreceedingFreeSpace = SubEventEntry.AvailablePreceedingFreeSpace;
                RigidSchedule = SubEventEntry.Rigid;
                CalendarEventRange = SubEventEntry.CalendarEventRange;
                EventLocation = SubEventEntry.LocationData;
                Enabled = SubEventEntry.Enabled;
                ThirdPartyID = SubEventEntry.ThirdPartyID;
                return true;
            }

            return false;
        }







        public bool PinToEndAndIncludeInTimeLine(TimeLine LimitingTimeLine)
        {
            DateTime ReferenceTime = new DateTime();
            EndDateTime = this.getCalendarEventRange.End;
            ReferenceTime = EndDateTime;
            if (EndDateTime > LimitingTimeLine.End)
            {
                ReferenceTime = LimitingTimeLine.End;
            }
            DateTime MyStartTime = ReferenceTime - this.EventDuration;
            if(this.getCalendarEventRange.IsTimeLineWithin(new TimeLine(MyStartTime,ReferenceTime)))
            {

                StartDateTime = MyStartTime;
                //ActiveSlot = new BusyTimeLine(this.ID, (MyStartTime), ReferenceTime);
                TimeSpan BusyTimeLineShift = MyStartTime - ActiveSlot.Start;
                ActiveSlot.shiftTimeline(BusyTimeLineShift);
                EndDateTime = ReferenceTime;
                LimitingTimeLine.AddBusySlots(ActiveSlot);
                return true;
            }
            return false;
        }


        public  bool PinToEndAndIncludeInTimeLine(TimeLine LimitingTimeLine, CalendarEvent RestrctingCalendarEvent)
        {
            if (new EventID(RestrctingCalendarEvent.ID).getLevelID(0) != SubEventID.getLevelID(0))
            {
                throw new Exception("Oh oh Sub calendar event Trying to pin to end of invalid calendar event. Check that you have matchin IDs");
            }
            DateTime ReferenceTime = new DateTime();
            EndDateTime=RestrctingCalendarEvent.End;
            if (EndDateTime > LimitingTimeLine.End)
            {
                ReferenceTime = LimitingTimeLine.End;
            }
            /*else
            {
                ReferenceTime = End;
            }*/
            
            DateTime MyStartTime = ReferenceTime - this.EventDuration;

            if (this.getCalendarEventRange.IsTimeLineWithin(new TimeLine(MyStartTime, ReferenceTime)))
            {
                StartDateTime = MyStartTime;
                //ActiveSlot = new BusyTimeLine(this.ID, (MyStartTime), ReferenceTime);
                TimeSpan BusyTimeLineShift = MyStartTime - ActiveSlot.Start;
                ActiveSlot.shiftTimeline(BusyTimeLineShift);
                EndDateTime = ReferenceTime;
                LimitingTimeLine.AddBusySlots(ActiveSlot);
                return true;
            }

            return false;
        }

        public bool PinToEnd(TimeLine LimitingTimeLine)
        {
            DateTime ReferenceTime = new DateTime();
            EndDateTime = this.getCalendarEventRange.End;
            ReferenceTime = EndDateTime;
            if (EndDateTime > LimitingTimeLine.End)
            {
                ReferenceTime = LimitingTimeLine.End;
            }
            DateTime MyStartTime = ReferenceTime - this.EventDuration;
            if (this.getCalendarEventRange.IsTimeLineWithin(new TimeLine(MyStartTime, ReferenceTime)))
            {

                StartDateTime = MyStartTime;
                //ActiveSlot = new BusyTimeLine(this.ID, (MyStartTime), ReferenceTime);
                TimeSpan BusyTimeLineShift = MyStartTime - ActiveSlot.Start;
                ActiveSlot.shiftTimeline(BusyTimeLineShift);
                EndDateTime = ReferenceTime;
                return true;
            }
            return false;
        }



        public void PinToEnd(CalendarEvent RestrctingCalendarEvent)
        {
            if (new EventID(RestrctingCalendarEvent.ID).getLevelID(0) != SubEventID.getLevelID(0))
            {
                throw new Exception("Oh oh Sub calendar event Trying to pin to end of invalid calendar event. Check that you have matchin IDs");
            }
            DateTime ReferenceTime = new DateTime();
            EndDateTime = RestrctingCalendarEvent.End;
            ReferenceTime = EndDateTime;
            DateTime MyStartTime = ReferenceTime - this.EventDuration;
            StartDateTime = MyStartTime;

            
            //ActiveSlot = new BusyTimeLine(this.ID, (MyStartTime), ReferenceTime);
            TimeSpan BusyTimeLineShift = MyStartTime - ActiveSlot.Start;
            ActiveSlot.shiftTimeline(BusyTimeLineShift);
            EndDateTime = ReferenceTime;
        }

        public bool shiftEvent(TimeSpan ChangeInTime, bool force=false)
        {
            TimeLine UpdatedTimeLine = new TimeLine(this.Start + ChangeInTime, this.End + ChangeInTime);
            if (!(this.getCalendarEventRange.IsTimeLineWithin(UpdatedTimeLine))&&!force)
            {
                return false;
            }
            StartDateTime += ChangeInTime;
            EndDateTime += ChangeInTime;
            ActiveSlot.shiftTimeline(ChangeInTime);
            return true;
        }
        

         public static double CalculateDistance(SubCalendarEvent Arg1,SubCalendarEvent Arg2)
        {
            if(Arg1.SubEvent_ID.getStringIDAtLevel(0)==Arg1.SubEvent_ID.getStringIDAtLevel(0))
            {
                return double.MaxValue;
            }
            else
            {
                return Location.calculateDistance(Arg1.myLocation,Arg2.myLocation);
            }
        }


         public bool canExistWithinTimeLine(TimeLine PossibleTimeLine)
         {
             SubCalendarEvent thisCopy = this.createCopy();
             return (thisCopy.PinSubEventsToStart(PossibleTimeLine) && thisCopy.PinToEnd(PossibleTimeLine));
         }

         public bool canExistTowardsEndWithoutSpace(TimeLine PossibleTimeLine)
         {
             TimeLine ParentCalRange = getCalendarEventRange;
             bool retValue = (ParentCalRange.Start <= (PossibleTimeLine.End - ActiveDuration)) && (ParentCalRange.End>=PossibleTimeLine.End);

             return retValue;
         }

         public bool canExistTowardsStartWithoutSpace(TimeLine PossibleTimeLine)
         {
             TimeLine ParentCalRange = getCalendarEventRange;
             bool retValue = ((PossibleTimeLine.Start + ActiveDuration) <= ParentCalRange.End) && (ParentCalRange.Start <= PossibleTimeLine.Start);

             return retValue;
         }

         override public void SetEventEnableStatus(bool EnableDisableFlag)
         {
             /*Function enables or disables SubCalEvent*/
             
             this.Enabled = EnableDisableFlag;
         }
        #endregion

        #region Class Properties

        public TimeLine getCalendarEventRange
        {
            get 
            {
                return CalendarEventRange;
            }
        }

        public override DateTime End
        {
            get
            {
                return base.End;
            }
        }

        public override DateTime Start
        {
            get
            {
                return base.Start;
            }
        }
        
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
                if (BusyFrame.TimelineSpan != value.TimelineSpan)
                {
                    throw new Exception("New lhs Activeslot isnt the same duration as old active slot. Check for inconsistency in code");
                }
                else 
                {
                    TimeSpan ChangeInTimeSpan = value.Start - BusyFrame.Start;
                    shiftEvent(ChangeInTimeSpan);
                }
                
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


        public EventID SubEvent_ID
        {
            get
            {
                return SubEventID;//.ToString();
            }
        }

        override public bool Rigid
        {
            get
            {
                return RigidSchedule;
            }
        }

        public override TimeLine RangeTimeLine
        {
            get
            {
                updateEventSequence();
                return EventSequence;
            }
        }


        public TimeSpan RangeSpan
        {
            get
            {
                return this.RangeTimeLine.TimelineSpan;
            }
        }

        override public Location myLocation
        {
            set
            {
                EventLocation = value;
            }
            get
            {
                return EventLocation;
            }
        }

        override public bool isEnabled
        {
            get
            {
                return Enabled;
            }
        }


        override public Event_Struct toEvent_Struct
        {
            get
            {
                Event_Struct retValue = new Event_Struct();
                //retValue.StartTicks = Start.Ticks;
                //retValue.EndTicks = End.Ticks;
                //retValue.DurationTicks = ActiveDuration.Ticks;
                //retValue.EventID = ID;
                retValue.EventLocation = myLocation.toStruct();
                return retValue;
            }
        }


        
        #endregion

    }
}


