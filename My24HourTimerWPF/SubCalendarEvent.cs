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

        #region Classs Constructor
        public SubCalendarEvent()
        { }
        public SubCalendarEvent(TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID, TimeLine RangeOfSubCalEvent = null)
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
        }

        public SubCalendarEvent(TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, TimeLine RangeOfSubCalEvent = null)
        {
            CalendarEventRange = RangeOfSubCalEvent;
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = Event_Duration;
            PrepTime = EventPrepTime;
            //SubEventID = new EventID(new string[] { myParentID, EventIDGenerator.generate().ToString() });
            if (myParentID == "16")
            {
                ;
            }
            
            SubEventID = new EventID(myParentID + "_" + EventIDGenerator.generate().ToString());


            EventSequence = new EventTimeLine(SubEventID.ToString(), StartDateTime, EndDateTime);
            RigidSchedule = Rigid;
        }

        public SubCalendarEvent(string MySubEventID, BusyTimeLine MyBusylot, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID, TimeLine RangeOfSubCalEvent = null)
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
        }

        public SubCalendarEvent(string MySubEventID, DateTime EventStart, DateTime EventDeadline, BusyTimeLine SubEventBusy, TimeLine RangeOfSubCalEvent = null)
        {
            CalendarEventRange = RangeOfSubCalEvent;
            SubEventID = new EventID(MySubEventID.Split('_'));
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = SubEventBusy.TimelineSpan;
            BusyFrame = SubEventBusy;
        }

        public SubCalendarEvent(string MySubEventID, BusyTimeLine MyBusylot, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, TimeLine RangeOfSubCalEvent = null)
        {
            CalendarEventRange = RangeOfSubCalEvent;
            //string eventName, TimeSpan EventDuration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = MyBusylot.End - MyBusylot.Start;
            BusyFrame = MyBusylot;
            PrepTime = EventPrepTime;
            SubEventID = new EventID(MySubEventID.Split('_'));

            if (myParentID == "16")
            {
                ;
            }

            EventSequence = new EventTimeLine(SubEventID.ToString(), StartDateTime, EndDateTime);
            RigidSchedule = Rigid;
        }

        public SubCalendarEvent(string MySubEventID, DateTime EventStart, DateTime EventDeadline, BusyTimeLine SubEventBusy, bool Rigid, TimeLine RangeOfSubCalEvent = null)
        {
            CalendarEventRange = RangeOfSubCalEvent;
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
            EndDateTime = (EndTime);
            StartDateTime = StartTime;
            BusyFrame = new BusyTimeLine(SubEventID.ToString(), StartTime, EndTime);
        }



        public SubCalendarEvent createCopy()
        {
            SubCalendarEvent MySubCalendarEventCopy = new SubCalendarEvent(this.ID, new DateTime(Start.Ticks), new DateTime(End.Ticks), BusyFrame.CreateCopy(), this.RigidSchedule,new TimeLine(CalendarEventRange.Start,CalendarEventRange.End));
            MySubCalendarEventCopy.LocationString = LocationString;

            MySubCalendarEventCopy.CalendarEventLocation = CalendarEventLocation;//note check for possible reference issues for future versions
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

        public  TimeLine PinToEnd(TimeLine LimitingTimeLine, CalendarEvent RestrctingCalendarEvent)
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
            else
            {
                ReferenceTime = End;
            }
            DateTime MyStartTime = ReferenceTime - this.EventDuration;
            StartDateTime= MyStartTime;
            ActiveSlot = new BusyTimeLine(this.ID, (MyStartTime), ReferenceTime);
            EndDateTime = ReferenceTime;
            LimitingTimeLine.AddBusySlots(ActiveSlot);
            return LimitingTimeLine;
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
            ActiveSlot = new BusyTimeLine(this.ID, (MyStartTime), ReferenceTime);
            EndDateTime = ReferenceTime;
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

        public override TimeLine EventTimeLine
        {
            get
            {
                updateEventSequence();
                return EventSequence;
            }
        }
        #endregion

    }
}
