using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class SubCalendarEvent : CalendarEvent
    {
        protected EventID UniqueID;
        protected BusyTimeLine BusyFrame;
        TimeSpan AvailablePreceedingFreeSpace;
        protected TimeLine CalendarEventRange;
        protected double EventScore;
        protected ConflictProfile ConflictingEvents;

        protected Location_Elements EventLocation;
        IList<EventID> InterferringEvents;

        int MiscIntData;

        #region Classs Constructor
        public SubCalendarEvent()
        { }

        public SubCalendarEvent(TimeSpan Event_Duration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, bool Enabled, EventDisplay UiParam,MiscData Notes,bool completeFlag, Location_Elements EventLocation =null, TimeLine RangeOfSubCalEvent = null, ConflictProfile conflicts=null)
        {
            if (conflicts == null)
            {
                conflicts = new ConflictProfile();
            }
            ConflictingEvents = conflicts;
            CalendarEventRange = RangeOfSubCalEvent;
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = Event_Duration;
            PrepTime = EventPrepTime;
            if (myParentID == "16")
            {
                ;
            }
            UiParams=UiParam;
            DataBlob = Notes;
            Complete=completeFlag;
            UniqueID = EventID.GenerateSubCalendarEvent(myParentID);
            BusyFrame = new BusyTimeLine(this.ID, StartDateTime, EndDateTime);//this is because in current implementation busy frame is the same as CalEvent frame
            this.EventLocation = EventLocation;
            EventSequence = new EventTimeLine(UniqueID.ToString(), StartDateTime, EndDateTime);
            RigidSchedule = Rigid;
            this.Enabled = Enabled;
        }


        public SubCalendarEvent(string MySubEventID, BusyTimeLine MyBusylot, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, bool Enabled, EventDisplay UiParam, MiscData Notes, bool completeFlag, Location_Elements EventLocation = null, TimeLine RangeOfSubCalEvent = null, ConflictProfile conflicts = null)
        {
            if (conflicts == null)
            {
                conflicts = new ConflictProfile();
            }
            ConflictingEvents = conflicts;
            CalendarEventRange = RangeOfSubCalEvent;
            //string eventName, TimeSpan EventDuration, DateTime EventStart, DateTime EventDeadline, TimeSpan EventPrepTime, TimeSpan PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = MyBusylot.End - MyBusylot.Start;
            BusyFrame = MyBusylot;
            PrepTime = EventPrepTime;
            UniqueID = new EventID(MySubEventID);
            this.EventLocation = EventLocation;
            
            UiParams=UiParam;
            DataBlob= Notes;
            Complete = completeFlag;

            this.Enabled = Enabled;
            EventSequence = new EventTimeLine(UniqueID.ToString(), StartDateTime, EndDateTime);
            RigidSchedule = Rigid;
        }

        public SubCalendarEvent(string MySubEventID, DateTime EventStart, DateTime EventDeadline, BusyTimeLine SubEventBusy, bool Rigid, bool Enabled, EventDisplay UiParam, MiscData Notes, bool completeFlag, Location_Elements EventLocation = null, TimeLine RangeOfSubCalEvent = null, ConflictProfile conflicts = null)
        {
            if (conflicts == null)
            {
                conflicts = new ConflictProfile();
            }
            ConflictingEvents = conflicts;
            CalendarEventRange = RangeOfSubCalEvent;
            UniqueID = new EventID(MySubEventID);
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = SubEventBusy.TimelineSpan;
            BusyFrame = SubEventBusy;
            RigidSchedule = Rigid;
            this.Enabled = Enabled;
            this.EventLocation = EventLocation;
            UiParams = UiParam;
            DataBlob = Notes;
            Complete = completeFlag;
        }
        #endregion


        #region Class functions

        public override string ToString()
        {
            return this.Start.ToString() + " - " + this.End.ToString() + "::" + this.ID + "\t\t::" + this.ActiveDuration.ToString();
        }

        public void Disable()
        {
            this.Enabled = false;
        }

        public void Enable()
        {
            this.Enabled = true;
        }

        public void SetCompletionStatus(bool CompletionStatus,CalendarEvent myCalendarEvent)
        {
            //Complete = CompletionStatus;
            myCalendarEvent.setSubEventCompletionStatus(CompletionStatus, this);
            
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
            BusyFrame = new BusyTimeLine(UniqueID.ToString(), StartTime, EndTime);
        }


        public void SetAsRigid()
        {
            Rigid = true;
        }

        public void SetAsNonRigid()
        {
            Rigid = false;
        }

        public void DisableIfPastDeadline(DateTime CurrNow)
        {
            if (CalendarEventRange.End < CurrNow)
            {
                Disable();
            }
        }

        public bool IsDateTimeWithin(DateTime DateTimeEntry)
        {
            return RangeTimeLine.IsDateTimeWithin(DateTimeEntry);
        }

        public static SubCalendarEvent getEmptyCalendarEvent()
        {
            SubCalendarEvent retValue = new SubCalendarEvent();
            retValue.UniqueID = new EventID("");
            retValue.StartDateTime = DateTime.Now;
            retValue.EndDateTime = DateTime.Now;
            retValue.EventDuration = new TimeSpan(0);
            retValue.Splits = 1;
            retValue.Rigid = true;
            retValue.Complete = true;
            retValue.Enabled = false;
            return retValue;
        }

        public SubCalendarEvent createCopy()
        {
            SubCalendarEvent MySubCalendarEventCopy = new SubCalendarEvent(this.ID, new DateTime(Start.Ticks), new DateTime(End.Ticks), BusyFrame.CreateCopy(), this.RigidSchedule, this.isEnabled, this.UiParams.createCopy(), this.Notes.createCopy(), this.Complete, this.EventLocation, new TimeLine(CalendarEventRange.Start, CalendarEventRange.End),ConflictingEvents.CreateCopy());
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

        public bool PinToStart(TimeLine MyTimeLine)
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
                return (MyTimeLine.IsTimeLineWithin( this.RangeTimeLine));
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

        public bool PinToPossibleLimit(TimeLine referenceTimeLine)
        { 
            TimeLine interferringTImeLine=CalendarEventRange.InterferringTimeLine( referenceTimeLine );
            if (interferringTImeLine == null)
            {
                return false;
            }
            DateTime EarliestEndTime = CalendarEventRange.Start + ActiveDuration;
            DateTime LatestEndTime = CalendarEventRange.End;

            DateTime DesiredEndtime = referenceTimeLine.End + (TimeSpan.FromTicks((long)(ActiveDuration - referenceTimeLine.TimelineSpan).Ticks / 2));

            if (DesiredEndtime < EarliestEndTime)
            {
                DesiredEndtime = EarliestEndTime;
            }

            if (DesiredEndtime > LatestEndTime)
            {
                DesiredEndtime = LatestEndTime;
            }
            TimeSpan shiftInEvent= End - DesiredEndtime;
            shiftEvent(shiftInEvent);
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
                EventLocation = SubEventEntry.EventLocation;
                Enabled = SubEventEntry.Enabled;
                ThirdPartyID = SubEventEntry.ThirdPartyID;
                return true;
            }

            throw new Exception("Error Detected: Trying to update SubCalendar Event with non matching ID");
        }

        public static void updateMiscData(IList<SubCalendarEvent>AllSubCalendarEvents, IList<int> IntData)
        {
            if(AllSubCalendarEvents.Count!=IntData.Count)
            {
                throw new Exception("trying to Subcalendar events with not matching count of intData");
            }
            else
            {
                for(int i=0;i<AllSubCalendarEvents.Count;i++)
                {
                    AllSubCalendarEvents[i].MiscIntData=IntData[i];
                }
            }
        }

        public static void incrementMiscdata(IList<SubCalendarEvent> AllSubCalendarEvents)
        {
            for (int i = 0; i < AllSubCalendarEvents.Count; i++)
            {
                ++AllSubCalendarEvents[i].MiscIntData;// = IntData[i];
            }
        }

        public static void decrementMiscdata(IList<SubCalendarEvent> AllSubCalendarEvents)
        {
            for (int i = 0; i < AllSubCalendarEvents.Count; i++)
            {
                --AllSubCalendarEvents[i].MiscIntData;// = IntData[i];
            }
        }



        public static void updateMiscData(IList<SubCalendarEvent> AllSubCalendarEvents, int IntData)
        {
            
            {
                for (int i = 0; i < AllSubCalendarEvents.Count; i++)
                {
                    AllSubCalendarEvents[i].MiscIntData = IntData;
                }
            }
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
            if (new EventID(RestrctingCalendarEvent.ID).getCalendarEventComponent() != UniqueID.getCalendarEventComponent())
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

            if (this.Rigid)
            {
                return (LimitingTimeLine.IsTimeLineWithin(this.RangeTimeLine));
            }


            DateTime MyStartTime = ReferenceTime - this.EventDuration;


            if ((MyStartTime>=LimitingTimeLine.Start )&&(MyStartTime>=getCalendarEventRange.Start))
            {

                StartDateTime = MyStartTime;
                //ActiveSlot = new BusyTimeLine(this.ID, (MyStartTime), ReferenceTime);
                TimeSpan BusyTimeLineShift = MyStartTime - ActiveSlot.Start;
                ActiveSlot.shiftTimeline(BusyTimeLineShift);
                EndDateTime = ReferenceTime;
                return true;
            }

            StartDateTime= ActiveSlot.Start;
            EndDateTime = ActiveSlot.End;
            return false;
        }



        public void PinToEnd(CalendarEvent RestrctingCalendarEvent)
        {
            if (new EventID(RestrctingCalendarEvent.ID).getCalendarEventComponent() != UniqueID.getCalendarEventComponent())
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
        

         public static double CalculateDistance(SubCalendarEvent Arg1,SubCalendarEvent Arg2, double worstDistance=double.MaxValue)
        {
            if (Arg1.SubEvent_ID.getIDUpToCalendarEvent() == Arg2.SubEvent_ID.getIDUpToCalendarEvent())
            {
                return worstDistance;
            }
            else
            {
                return Location_Elements.calculateDistance(Arg1.myLocation, Arg2.myLocation, worstDistance);
            }
        }


         public static double CalculateDistance(IList<SubCalendarEvent> Allevents, double worstDistance=double.MaxValue)
         {
             int j=0;
             double retValue = 0;
             for (int i = 0; i < Allevents.Count - 1; i++)
             { 
                 j=i+1;
                 retValue+=CalculateDistance(Allevents[i], Allevents[j], worstDistance);
             }
             return retValue;
         }

         public bool canExistWithinTimeLine(TimeLine PossibleTimeLine)
         {
             SubCalendarEvent thisCopy = this.createCopy();
             return (thisCopy.PinToStart(PossibleTimeLine) && thisCopy.PinToEnd(PossibleTimeLine));
         }

         public bool canExistTowardsEndWithoutSpace(TimeLine PossibleTimeLine)
         {
             TimeLine ParentCalRange = getCalendarEventRange;
             bool retValue = (ParentCalRange.Start <= (PossibleTimeLine.End - ActiveDuration)) && (ParentCalRange.End>=PossibleTimeLine.End)&&(canExistWithinTimeLine(PossibleTimeLine));

             return retValue;
         }

        static public bool isConflicting(SubCalendarEvent firstEvent, SubCalendarEvent secondEvent)
        {
            bool retValue = firstEvent.RangeTimeLine.InterferringTimeLine(secondEvent.RangeTimeLine) != null;
            return retValue;
        }

         public bool canExistTowardsStartWithoutSpace(TimeLine PossibleTimeLine)
         {
             TimeLine ParentCalRange = getCalendarEventRange;
             bool retValue = ((PossibleTimeLine.Start + ActiveDuration) <= ParentCalRange.End) && (ParentCalRange.Start <= PossibleTimeLine.Start) && (canExistWithinTimeLine(PossibleTimeLine));

             return retValue;
         }

         override public void SetEventEnableStatus(bool EnableDisableFlag)
         {
             /*Function enables or disables SubCalEvent*/
             
             this.Enabled = EnableDisableFlag;
         }
        #endregion

        #region Class Properties

         public ConflictProfile Conflicts
         {
             get
             {
                 return ConflictingEvents;
             }
         }

        public TimeLine getCalendarEventRange
        {
            get 
            {
                return CalendarEventRange;
            }
        }

        public double Score
        {
            get 
            {
                return EventScore;
            }
            set
            {
                EventScore = value;
            }
        }

        public int IntData
        {
            get
            {
                return MiscIntData;
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
                return UniqueID.ToString();
            }
        }

        

        public EventID SubEvent_ID
        {
            get
            {
                return UniqueID;//.ToString();
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

        override public Location_Elements myLocation
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

        override public MiscData Notes
        { 
            get
            {
                return DataBlob;
            }
        }
        
        #endregion

    }
}


