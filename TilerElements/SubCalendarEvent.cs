using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace TilerElements
{
    public class SubCalendarEvent : TilerEvent,IDefinedRange
    {
        
        protected BusyTimeLine BusyFrame;
        protected BusyTimeLine NonHumaneTimeLine= new BusyTimeLine();
        protected BusyTimeLine HumaneTimeLine = new BusyTimeLine();
        TimeSpan AvailablePreceedingFreeSpace;
        protected TimeLine CalendarEventRange;
        protected double EventScore;
        protected ConflictProfile ConflictingEvents = new ConflictProfile();
        protected ulong preferredDayIndex=0;
        protected int MiscIntData;
        protected bool Vestige = false;
        protected ulong UnUsableIndex;
        protected ulong OldPreferredIndex;
        protected bool CalculationMode = false;
        protected bool BlobEvent = false;

        #region Classs Constructor
        public SubCalendarEvent()
        { }

        public SubCalendarEvent(TimeSpan Event_Duration, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, bool Enabled, EventDisplay UiParam,MiscData Notes,bool completeFlag, Location_Elements EventLocation =null, TimeLine RangeOfSubCalEvent = null, ConflictProfile conflicts=null, string Creator="")
        {
            CreatorIDInfo = Creator;
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
            this.LocationData = EventLocation;
//            EventSequence = new EventTimeLine(UniqueID.ToString(), StartDateTime, EndDateTime);
            RigidSchedule = Rigid;
            this.Enabled = Enabled;
        }


        public SubCalendarEvent(string MySubEventID, BusyTimeLine MyBusylot, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, bool Enabled, EventDisplay UiParam, MiscData Notes, bool completeFlag, Location_Elements EventLocation = null, TimeLine RangeOfSubCalEvent = null, ConflictProfile conflicts = null, string Creator = "")
        {
            CreatorIDInfo = Creator;
            if (conflicts == null)
            {
                conflicts = new ConflictProfile();
            }
            ConflictingEvents = conflicts;
            CalendarEventRange = RangeOfSubCalEvent;
            //string eventName, TimeSpan EventDuration, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, TimeSpan PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = MyBusylot.End - MyBusylot.Start;
            BusyFrame = MyBusylot;
            PrepTime = EventPrepTime;
            UniqueID = new EventID(MySubEventID);
            this.LocationData = EventLocation;
            
            UiParams=UiParam;
            DataBlob= Notes;
            Complete = completeFlag;

            this.Enabled = Enabled;
            //EventSequence = new EventTimeLine(UniqueID.ToString(), StartDateTime, EndDateTime);
            RigidSchedule = Rigid;
        }

        public SubCalendarEvent(string MySubEventID, DateTimeOffset EventStart, DateTimeOffset EventDeadline, BusyTimeLine SubEventBusy, bool Rigid, bool Enabled, EventDisplay UiParam, MiscData Notes, bool completeFlag, Location_Elements EventLocation = null, TimeLine RangeOfSubCalEvent = null, ConflictProfile conflicts = null, string Creator = "")
        {
            CreatorIDInfo = Creator;
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
            this.LocationData = EventLocation;
            UiParams = UiParam;
            DataBlob = Notes;
            Complete = completeFlag;
        }
        #endregion


        #region Class functions

        public virtual string ToString()
        {
            return this.Start.ToString() + " - " + this.End.ToString() + "::" + this.ID + "\t\t::" + this.ActiveDuration.ToString();
        }


        public void disable(CalendarEvent myCalEvent)
        {
            this.Enabled = false;
            myCalEvent.incrementDeleteCount();
        }

        internal void disableWithoutUpdatingCalEvent()
        {
            this.Enabled = false;
        }

        public void complete(CalendarEvent myCalEvent)
        {
            this.Complete= true;
            myCalEvent.incrementCompleteCount();
        }

        public void nonComplete(CalendarEvent myCalEvent)
        {
            this.Complete = false;
            myCalEvent.decrementCompleteCount();
        }

        internal void completeWithoutUpdatingCalEvent()
        {
            this.Complete = true;
        }

        internal void nonCompleteWithoutUpdatingCalEvent()
        {
            this.Complete = false;
        }

        public void Enable(CalendarEvent myCalEvent)
        {
            this.Enabled = true;
            myCalEvent.decrementDeleteCount();
        }

        internal void enableWithouUpdatingCalEvent()
        {
            this.Enabled = true;
        }

        public void resetPreferredDayIndex()
        {
            preferredDayIndex = 0;
        }

        public void updateDayIndex(CalendarEvent myCalEvent)
        {
            preferredDayIndex = ReferenceNow.getDayIndexFromStartOfTime(StartDateTime);
            myCalEvent.removeDayTimeFromFreeUpdays(preferredDayIndex);
        }

        public void SetCompletionStatus(bool CompletionStatus,CalendarEvent myCalendarEvent)
        {
            Complete = CompletionStatus;
            UiParams.setCompleteUI(Complete);
            if (CompletionStatus)
            {
                complete(myCalendarEvent);
            }
            else 
            {
                nonComplete(myCalendarEvent);
            }
        }


        public void SetAsRigid()
        {
            RigidSchedule = true;
        }

        public void SetAsNonRigid()
        {
            RigidSchedule = false;
        }

        /*
        virtual public void DisableIfDeadlineHasPassed(DateTimeOffset CurrNow)
        {
            if (CalendarEventRange.End < CurrNow)
            {
                Disable();
                DeadlineElapsed = true;
            }
        }
        */
        virtual public bool IsDateTimeWithin(DateTimeOffset DateTimeEntry)
        {
            return RangeTimeLine.IsDateTimeWithin(DateTimeEntry);
        }
        /// <summary>
        /// Function Subcalendarevent evaluates itself against the given parameters
        /// </summary>
        /// <param name="refLocation"></param>
        /// <param name="DayReference"></param>
        /// <returns></returns>
        virtual public Tuple<TimeLine,Double> evaluateAgainstOptimizationParameters(Location_Elements refLocation, TimeLine DayTimeLine)
        {
            
            double distance = Location_Elements.calculateDistance(refLocation,this.myLocation);
            TimeLine refTimeLine = new TimeLine(DayTimeLine.Start, CalendarEventRange.End);
            Tuple<TimeLine, double> retValue = new Tuple<TimeLine, double>(refTimeLine,distance);
            return retValue;
        }

        public static SubCalendarEvent getEmptyCalendarEvent()
        {
            SubCalendarEvent retValue = new SubCalendarEvent();
            retValue.UniqueID = new EventID("");
            retValue.StartDateTime = DateTimeOffset.Now;
            retValue.EndDateTime = DateTimeOffset.Now;
            retValue.EventDuration = new TimeSpan(0);
            
            retValue.RigidSchedule= true;
            retValue.Complete = true;
            retValue.Enabled = false;
            return retValue;
        }



        virtual public SubCalendarEvent createCopy()
        {
            SubCalendarEvent MySubCalendarEventCopy = new SubCalendarEvent(this.ID, Start, End, BusyFrame.CreateCopy(), this.RigidSchedule, this.isEnabled, this.UiParams.createCopy(), this.Notes.createCopy(), this.Complete, this.LocationData, new TimeLine(CalendarEventRange.Start, CalendarEventRange.End), ConflictingEvents.CreateCopy());
            MySubCalendarEventCopy.ThirdPartyID = this.ThirdPartyID;
            MySubCalendarEventCopy.DeadlineElapsed = this.DeadlineElapsed;
            MySubCalendarEventCopy.UserDeleted = this.UserDeleted;
            MySubCalendarEventCopy.isRestricted = this.isRestricted;
            MySubCalendarEventCopy.preferredDayIndex = this.preferredDayIndex;
            MySubCalendarEventCopy.CreatorIDInfo = this.CreatorIDInfo;
            return MySubCalendarEventCopy;
        }

        /*
        virtual public void updateEventSequence()
        {
            EventSequence = new TimeLine(this.Start, this.End);
            EventSequence.AddBusySlots(BusyFrame);
        }
        */

        public void updateDayIndex(ulong DayIndex)
        {
            this.preferredDayIndex = DayIndex;
        }

        public static void updateDayIndex(ulong DayIndex, IEnumerable<SubCalendarEvent> AllSUbevents)
        {
            foreach (SubCalendarEvent eachSubCalendarEvent in AllSUbevents)
            {
                eachSubCalendarEvent.preferredDayIndex = DayIndex;
            }
        }

        public static void resetScores(IEnumerable<SubCalendarEvent> AllSUbevents)
        {
            AllSUbevents.AsParallel().ForAll(obj => obj.Score = 0);
        }

        public static TimeSpan TotalActiveDuration(IEnumerable<SubCalendarEvent> ListOfSubCalendarEvent)
        {
            TimeSpan TotalTimeSpan = new TimeSpan(0);
            
            foreach (SubCalendarEvent mySubCalendarEvent in ListOfSubCalendarEvent)
            {
                TotalTimeSpan=TotalTimeSpan.Add(mySubCalendarEvent.ActiveDuration);
            }

            return TotalTimeSpan;
        }

        
        virtual public bool PinToStart(TimeLine MyTimeLine)
        {
            DateTimeOffset ReferenceStartTime = new DateTimeOffset();
            DateTimeOffset ReferenceEndTime = new DateTimeOffset();

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

        virtual public bool PinToPossibleLimit(TimeLine referenceTimeLine)
        { 
            TimeLine interferringTImeLine=CalendarEventRange.InterferringTimeLine( referenceTimeLine );
            if (interferringTImeLine == null)
            {
                return false;
            }
            DateTimeOffset EarliestEndTime = CalendarEventRange.Start + ActiveDuration;
            DateTimeOffset LatestEndTime = CalendarEventRange.End;

            DateTimeOffset DesiredEndtime = interferringTImeLine.End + (TimeSpan.FromTicks(((long)(ActiveDuration - interferringTImeLine.TimelineSpan).Ticks) / 2));

            if (DesiredEndtime < EarliestEndTime)
            {
                DesiredEndtime = EarliestEndTime;
            }

            if (DesiredEndtime > LatestEndTime)
            {
                DesiredEndtime = LatestEndTime;
            }
            TimeSpan shiftInEvent = DesiredEndtime-End;
            return shiftEvent(shiftInEvent);
        }


        /// <summary>
        /// function updates the parameters of the current sub calevent using SubEventEntry. However it doesnt change some datamemebres such as rigid, and isrestricted. You 
        /// </summary>
        /// <param name="SubEventEntry"></param>
        /// <returns></returns>
        virtual public bool UpdateThis(SubCalendarEvent SubEventEntry)
        {
            if ((this.ID == SubEventEntry.ID)&&canExistWithinTimeLine(SubEventEntry.getCalendarEventRange))
            {
                this.BusyFrame = SubEventEntry.ActiveSlot;
                this.CalendarEventRange = SubEventEntry.getCalendarEventRange;
                this.FromRepeatEvent = SubEventEntry.FromRepeat;
                this.EventName = SubEventEntry.Name;
                this.EventDuration = SubEventEntry.ActiveDuration;
                this.Complete = SubEventEntry.isComplete;
                this.ConflictingEvents = SubEventEntry.Conflicts;
                this.DataBlob = SubEventEntry.Notes;
                this.DeadlineElapsed = SubEventEntry.isDeadlineElapsed;
                this.Enabled = SubEventEntry.isEnabled;
                this.EndDateTime = SubEventEntry.End;
                this.EventPreDeadline = SubEventEntry.PreDeadline;
                this.EventScore = SubEventEntry.Score;
                this.isRestricted = SubEventEntry.isEventRestricted;
                this.LocationData = SubEventEntry.myLocation;
                this.OldPreferredIndex = SubEventEntry.OldUniversalIndex;
                this.otherPartyID = SubEventEntry.ThirdPartyID;
                this.preferredDayIndex = SubEventEntry.UniversalDayIndex;
                this.PrepTime = SubEventEntry.Preparation;
                this.Priority = SubEventEntry.EventPriority;
                this.ProfileOfNow = SubEventEntry.ProfileOfNow;
                this.ProfileOfProcrastination = SubEventEntry.ProfileOfProcrastination;
                this.RepetitionFlag = SubEventEntry.FromRepeat;
                //this.RigidSchedule = SubEventEntry.Rigid;
                this.StartDateTime = SubEventEntry.Start;
                this.UiParams = SubEventEntry.UIParam;
                this.UniqueID = SubEventEntry.SubEvent_ID;
                this.UserDeleted = SubEventEntry.isUserDeleted;
                this.UserIDs = SubEventEntry.getAllUserIDs();
                this.Vestige = SubEventEntry.isVestige;
                this.otherPartyID = SubEventEntry.otherPartyID;
                return true;
            }

            throw new Exception("Error Detected: Trying to update SubCalendar Event with non matching ID");
        }

        virtual public SubCalendarEvent getProcrastinationCopy(CalendarEvent CalendarEventData,Procrastination ProcrastinationData )
        {
            SubCalendarEvent retValue = getCalulationCopy();
            /*
            retValue.CalendarEventRange = CalendarEventData.RangeTimeLine;
            TimeSpan SpanShift = ProcrastinationData.PreferredStartTime - retValue.Start;
            */
            retValue.CalendarEventRange = new TimeLine(ProcrastinationData.PreferredStartTime, retValue.CalendarEventRange.End);
            TimeSpan SpanShift = (retValue.CalendarEventRange.End - retValue.RangeSpan) - retValue.Start;
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventData.ID);
            retValue.shiftEvent(SpanShift,true);
            return retValue;
        }

        virtual public SubCalendarEvent getNowCopy(EventID CalendarEventID, NowProfile NowData)
        {
            SubCalendarEvent retValue = getCalulationCopy();
            retValue.RigidSchedule = true;
            TimeSpan SpanShift = NowData.PreferredTime - retValue.Start;
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventID.ToString());
            retValue.shiftEvent(SpanShift, true);
            return retValue;
        }

        virtual protected SubCalendarEvent getCalulationCopy()
        {
            SubCalendarEvent retValue = new SubCalendarEvent();
            retValue.BusyFrame = this.ActiveSlot.CreateCopy();
            retValue.CalendarEventRange = this.getCalendarEventRange.CreateCopy();
            retValue.FromRepeatEvent = this.FromRepeat;
            retValue.EventName = this.Name;
            retValue.EventDuration = this.ActiveDuration;
            retValue.Complete = this.isComplete;
            retValue.ConflictingEvents = this.Conflicts;
            retValue.DataBlob = this.Notes;
            retValue.DeadlineElapsed = this.isDeadlineElapsed;
            retValue.Enabled = this.isEnabled;
            retValue.EndDateTime = this.End;
            retValue.EventPreDeadline = this.PreDeadline;
            retValue.EventScore = this.Score;
            retValue.isRestricted = this.isEventRestricted;
            retValue.LocationData = this.myLocation.CreateCopy();
            retValue.OldPreferredIndex = this.OldUniversalIndex;
            retValue.otherPartyID = this.ThirdPartyID;
            retValue.preferredDayIndex = this.UniversalDayIndex;
            retValue.PrepTime = this.Preparation;
            retValue.Priority = this.EventPriority;
            retValue.ProfileOfNow = this.ProfileOfNow;
            retValue.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            retValue.RepetitionFlag = this.FromRepeat;
            retValue.RigidSchedule = this.Rigid;
            retValue.StartDateTime = this.Start;
            retValue.UiParams = this.UIParam;
            retValue.UniqueID = this.SubEvent_ID;
            retValue.UserDeleted = this.isUserDeleted;
            retValue.UserIDs = this.getAllUserIDs();
            retValue.Vestige = this.isVestige;
            retValue.otherPartyID = this.otherPartyID;
            return retValue;
        }

        public static void updateMiscData(IList<SubCalendarEvent>AllSubCalendarEvents, IList<int> IntData)
        {
            if(AllSubCalendarEvents.Count!=IntData.Count)
            {
                throw new Exception("trying to update MiscData  while Subcalendar events with not matching count of intData");
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
            for (int i = 0; i < AllSubCalendarEvents.Count; i++)
            {
                AllSubCalendarEvents[i].MiscIntData = IntData;
            }
        }




        /*
        virtual public bool PinToEndAndIncludeInTimeLine(TimeLine LimitingTimeLine)
        {
            DateTimeOffset ReferenceTime = new DateTimeOffset();
            EndDateTime = this.getCalendarEventRange.End;
            ReferenceTime = EndDateTime;
            if (EndDateTime > LimitingTimeLine.End)
            {
                ReferenceTime = LimitingTimeLine.End;
            }
            DateTimeOffset MyStartTime = ReferenceTime - this.EventDuration;
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
        */


        virtual public  bool PinToEndAndIncludeInTimeLine(TimeLine LimitingTimeLine, CalendarEvent RestrctingCalendarEvent)
        {
            if (new EventID(RestrctingCalendarEvent.ID).getCalendarEventComponent() != UniqueID.getCalendarEventComponent())
            {
                throw new Exception("Oh oh Sub calendar event Trying to pin to end of invalid calendar event. Check that you have matchin IDs");
            }
            DateTimeOffset ReferenceTime = new DateTimeOffset();
            EndDateTime=RestrctingCalendarEvent.End;
            if (EndDateTime > LimitingTimeLine.End)
            {
                ReferenceTime = LimitingTimeLine.End;
            }
            /*else
            {
                ReferenceTime = End;
            }*/
            
            DateTimeOffset MyStartTime = ReferenceTime - this.EventDuration;

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

        virtual public bool PinToEnd(TimeLine LimitingTimeLine)
        {
            DateTimeOffset ReferenceTime = this.getCalendarEventRange.End;
            if (ReferenceTime > LimitingTimeLine.End)
            {
                ReferenceTime = LimitingTimeLine.End;
            }

            if (this.Rigid)
            {
                return (LimitingTimeLine.IsTimeLineWithin(this.RangeTimeLine));
            }


            DateTimeOffset MyStartTime = ReferenceTime - this.EventDuration;


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


        /*
        virtual public void PinToEnd(CalendarEvent RestrctingCalendarEvent)
        {
            if (new EventID(RestrctingCalendarEvent.ID).getCalendarEventComponent() != UniqueID.getCalendarEventComponent())
            {
                throw new Exception("Oh oh Sub calendar event Trying to pin to end of invalid calendar event. Check that you have matchin IDs");
            }
            DateTimeOffset ReferenceTime = new DateTimeOffset();
            EndDateTime = RestrctingCalendarEvent.End;
            ReferenceTime = EndDateTime;
            DateTimeOffset MyStartTime = ReferenceTime - this.EventDuration;
            StartDateTime = MyStartTime;

            
            //ActiveSlot = new BusyTimeLine(this.ID, (MyStartTime), ReferenceTime);
            TimeSpan BusyTimeLineShift = MyStartTime - ActiveSlot.Start;
            ActiveSlot.shiftTimeline(BusyTimeLineShift);
            EndDateTime = ReferenceTime;
        }
        */

        /// <summary>
        /// Shifts a subcalendar event by the specified "ChangeInTime". Function returns a false if the change in time will not fall within calendarevent range. It returns true if successful. The force variable makes the subcalendareventignore the check for fitting in the calendarevent range
        /// </summary>
        /// <param name="ChangeInTime"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        virtual public bool shiftEvent(TimeSpan ChangeInTime, bool force=false)
        {
            if (force)
            {
                StartDateTime += ChangeInTime;
                EndDateTime += ChangeInTime;
                ActiveSlot.shiftTimeline(ChangeInTime);
                return true;
            }
            TimeLine UpdatedTimeLine = new TimeLine(this.Start + ChangeInTime, this.End + ChangeInTime);
            if (!(this.getCalendarEventRange.IsTimeLineWithin(UpdatedTimeLine)))
            {
                return false;
            }
            else
            {
                StartDateTime += ChangeInTime;
                EndDateTime += ChangeInTime;
                ActiveSlot.shiftTimeline(ChangeInTime);
                return true;
            }
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

         virtual public bool canExistWithinTimeLine(TimeLine PossibleTimeLine)
         {
             SubCalendarEvent thisCopy = this.createCopy();
             bool retValue= (thisCopy.PinToStart(PossibleTimeLine) && thisCopy.PinToEnd(PossibleTimeLine));
             return retValue;
         }

         virtual public bool canExistTowardsEndWithoutSpace(TimeLine PossibleTimeLine)
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

         virtual public bool canExistTowardsStartWithoutSpace(TimeLine PossibleTimeLine)
         {
             TimeLine ParentCalRange = getCalendarEventRange;
             bool retValue = ((PossibleTimeLine.Start + ActiveDuration) <= ParentCalRange.End) && (ParentCalRange.Start <= PossibleTimeLine.Start) && (canExistWithinTimeLine(PossibleTimeLine));

             return retValue;
         }
         /// <summary>
         /// Function returns the largest Timeline interferes with its calendar event range. If restricted subcalevent you can use the orderbystart to make a preference for selection. Essentiall select the largest time line with earliest start time
         /// </summary>
         /// <param name="TimeLineData"></param>
         /// <returns></returns>
         virtual public List<TimeLine> getTimeLineInterferringWithCalEvent(TimeLine TimeLineData, bool orderByStart = true)
         {
             TimeLine retValuTimeLine= CalendarEventRange.InterferringTimeLine(TimeLineData);;
             List<TimeLine> retValue = null;
             if (retValuTimeLine!=null)
             {
                 retValue = new List<TimeLine>() { retValuTimeLine };
             }
             return retValue;
         }
        /* 
        public void SetEventEnableStatus(bool EnableDisableFlag)
         {
             this.Enabled = EnableDisableFlag;
         }
        */
         public void UpdateInHumaneTimeLine()
         {
             NonHumaneTimeLine = ActiveSlot.CreateCopy();
         }

         public void UpdateHumaneTimeLine()
         {
             HumaneTimeLine = ActiveSlot.CreateCopy();
         }

         public ulong UniversalDayIndex
         {
             get
             {
                 return preferredDayIndex;
             }
         }

        public void enableCalculationMode()
        {
            CalculationMode = true;
        }

        /// <summary>
        /// This changes the duration of the subevent. It requires the change in duration
        /// </summary>
        /// <param name="Delta"></param>
         public void changeDurartion(TimeSpan Delta)
         {
             TimeSpan NewEventDuration = EventDuration.Add(Delta);
             if (NewEventDuration > new TimeSpan(0))
             {
                 EventDuration = NewEventDuration;
                 EndDateTime = StartDateTime.Add(EventDuration);
                 BusyFrame.updateBusyTimeLine(new BusyTimeLine(ID, ActiveSlot.Start, ActiveSlot.Start.Add(EventDuration)));
                 return;
             }
             throw new Exception("You are trying to reduce the Duration length to Less than zero");

         }

         internal void changeTimeLineRange(TimeLine newTimeLine)
         {
             CalendarEventRange = newTimeLine.CreateCopy();
         }

         public void updateUnusables(ulong unwantedIndex)
         {
             UnUsableIndex = unwantedIndex;
         }

         public ulong getUnUsableIndex()
         {
             return UnUsableIndex;
         }

         public ulong resetAndgetUnUsableIndex()
         {
             ulong retValue = UnUsableIndex;
             UnUsableIndex = 0;
             return retValue;
         }
        

        public static void updateUnUsable(IEnumerable<SubCalendarEvent>SubEVents,ulong UnwantedIndex)
        {
            SubEVents.AsParallel().ForAll(obj=>{obj.UnUsableIndex=UnwantedIndex;});
        }
        #endregion

        #region Class Properties

        public ulong OldUniversalIndex
        {
            get
            {
                return OldPreferredIndex;
            }

        }

        public bool isDesignated
        {
            get
            {
                bool retValue = preferredDayIndex != 0;
                return retValue;
            }
        }
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


        public double fittability
        {
            get
            {
                double retValue = ((double)CalendarEventRange.TimelineSpan.Ticks )/ ((double)RangeSpan.Ticks);
                return retValue;
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

         public bool Rigid
        {
            get
            {
                return RigidSchedule;
            }
        }

        public  TimeLine RangeTimeLine
        {
            get
            {
                
                return ActiveSlot;
            }
        }


        public TimeSpan RangeSpan
        {
            get
            {
                return this.RangeTimeLine.TimelineSpan;
            }
        }

        virtual public bool isBlobEvent
        {
            get
            {
                return BlobEvent;
            }
    }
        


        public Event_Struct toEvent_Struct
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

         public MiscData Notes
        { 
            get
            {
                return DataBlob;
            }
        }

         public bool isVestige
         {
             get 
             {
                 return Vestige;
             }
         }
         
        public bool isInCalculationMode
        {
            get
            {
                return CalculationMode;
            }
        }
        
        #endregion

    }
}


