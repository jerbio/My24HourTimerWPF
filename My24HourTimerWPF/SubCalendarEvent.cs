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

//            SubEventID = new EventID(new string[] { myParentID, EventIDGenerator.generate().ToString() });
            if (myParentID == "16")
            {
                ;
            }

            SubEventID = new EventID(myParentID+"_"+EventIDGenerator.generate().ToString());
            EventSequence = new EventTimeLine(SubEventID.ToString(), StartDateTime, EndDateTime);
        }

        public SubCalendarEvent(TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid)
        {
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

        public SubCalendarEvent(string MySubEventID, BusyTimeLine MyBusylot, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID)
        {
            //string eventName, TimeSpan EventDuration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = MyBusylot.End - MyBusylot.Start;
            BusyFrame = MyBusylot;
            PrepTime = EventPrepTime;
            SubEventID = new EventID(MySubEventID.Split('_'));
            EventSequence = new EventTimeLine(SubEventID.ToString(), StartDateTime, EndDateTime);
        }

        public SubCalendarEvent(string MySubEventID, DateTime EventStart, DateTime EventDeadline, BusyTimeLine SubEventBusy)
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

            if (myParentID == "16")
            {
                ;
            }

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
            EndDateTime = (EndTime);
            StartDateTime = StartTime;
            BusyFrame = new BusyTimeLine(SubEventID.ToString(), StartTime, EndTime);
        }

        public SubCalendarEvent createCopy()
        {
            SubCalendarEvent MySubCalendarEventCopy = new SubCalendarEvent(this.ID, new DateTime(Start.Ticks), new DateTime(End.Ticks), BusyFrame.CreateCopy(), this.RigidSchedule);
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
        #endregion

        #region Class Properties

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
