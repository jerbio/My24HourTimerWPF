using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
namespace TilerElements.Wpf
{
    public class SubCalendarEventAbstract : TilerEvent
    {
        public override void updateRepetitionIndex(long RepetitionIndex)
        {
            throw new NotImplementedException("Need to inherit class and implement properties");
        }
    }
    /*
    public abstract class Person
    {
        public int ID { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(50)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Last Name")]
        public string LastName { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        [Column("FirstName")]
        [System.ComponentModel.DataAnnotations.Display(Name = "First Name")]
        public string FirstMidName { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Full Name")]
        public string FullName
        {
            get
            {
                return LastName + ", " + FirstMidName;
            }
        }
    }

    public class OfficeAssignment
    {
        public string Id { get; set; }
    }

    public class Course
    {
        public string Id { get; set; }
    }


    public class Instructor : Person
    {
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [System.ComponentModel.DataAnnotations.DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Hire Date")]
        public DateTime HireDate { get; set; }

        public virtual ICollection<Course> Courses { get; set; }
        public virtual OfficeAssignment OfficeAssignment { get; set; }
    }*/
    public class SubCalendarEvent : SubCalendarEventAbstract, IDefinedRange
    {
        public static DateTimeOffset InitialPauseTime  = new DateTimeOffset();
        protected BusyTimeLine BusyFrame = new BusyTimeLine();
        protected BusyTimeLine NonHumaneTimeLine= new BusyTimeLine();
        protected BusyTimeLine HumaneTimeLine = new BusyTimeLine();
        protected TimeLine CalendarEventRange;
        protected double EventScore;
        protected ConflictProfile ConflictingEvents = new ConflictProfile();
        protected ulong PreferredDayIndex=0;
        protected int MiscIntData;
        protected bool Vestige = false;
        protected ulong UnUsableIndex;
        protected ulong OldPreferredIndex;
        protected bool CalculationMode = false;
        protected DateTimeOffset _PauseTime = InitialPauseTime;
        
        protected bool MuddledEvent = false;
        
        #region Classs Constructor
        public SubCalendarEvent()
        { }


        public SubCalendarEvent(TimeSpan Event_Duration, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, DateTimeOffset OriginalStartData, string myParentID, bool Rigid, bool Enabled, EventDisplay UiParam,MiscData Notes,bool completeFlag,long RepetitionIndex, Location_Elements EventLocation =null, TimeLine RangeOfSubCalEvent = null, ConflictProfile conflicts=null, TilerUser Creator = null)
        {
            this._Creator = Creator;
            if (conflicts == null)
            {
                conflicts = new ConflictProfile();
            }

            RepetitionSequence = RepetitionIndex;
            ConflictingEvents = conflicts;
            CalendarEventRange = RangeOfSubCalEvent;
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = Event_Duration;
            OriginalStart = OriginalStartData;
            PrepTime = EventPrepTime;
            if (myParentID == "16")
            {
                ;
            }
            UiParams=UiParam;
            _DataBlob = Notes;
            Complete=completeFlag;
            UniqueID = EventID.GenerateSubCalendarEvent(myParentID, RepetitionSequence);
            BusyFrame = new BusyTimeLine(this.Id, StartDateTime, EndDateTime);//this is because in current implementation busy frame is the same as CalEvent frame
            this.LocationInfo = EventLocation;
//            EventSequence = new EventTimeLine(UniqueID.ToString(), StartDateTime, EndDateTime);
            RigidSchedule = Rigid;
            this.Enabled = Enabled;
        }


        public SubCalendarEvent(string MySubEventID, BusyTimeLine MyBusylot, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, DateTimeOffset OriginalStartData, string myParentID, bool Rigid, bool Enabled, EventDisplay UiParam, MiscData Notes, bool completeFlag,long RepetitionSequenceData,Location_Elements EventLocation = null, TimeLine RangeOfSubCalEvent = null, ConflictProfile conflicts = null,  TilerUser Creator = null)
        {
            this._Creator = Creator;
            if (conflicts == null)
            {
                conflicts = new ConflictProfile();
            }
            ConflictingEvents = conflicts;
            OriginalStart = OriginalStartData;
            CalendarEventRange = RangeOfSubCalEvent;
            //string eventName, TimeSpan EventDuration, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, TimeSpan PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = MyBusylot.End - MyBusylot.Start;
            BusyFrame = MyBusylot;
            PrepTime = EventPrepTime;
            UniqueID = new EventID(MySubEventID);
            this.LocationInfo = EventLocation;
            RepetitionSequence = RepetitionSequenceData;
            UiParams=UiParam;
            _DataBlob= Notes;
            Complete = completeFlag;

            this.Enabled = Enabled;
            //EventSequence = new EventTimeLine(UniqueID.ToString(), StartDateTime, EndDateTime);
            RigidSchedule = Rigid;
        }

        public SubCalendarEvent(string MySubEventID, DateTimeOffset EventStart, DateTimeOffset EventDeadline, BusyTimeLine SubEventBusy, DateTimeOffset OriginalStartData, bool Rigid, bool Enabled, EventDisplay UiParam, MiscData Notes, bool completeFlag,long RepetitionSequenceData, Location_Elements EventLocation = null, TimeLine RangeOfSubCalEvent = null, ConflictProfile conflicts = null, TilerUser Creator = null)
        {
            this._Creator = Creator;
            if (conflicts == null)
            {
                conflicts = new ConflictProfile();
            }
            OriginalStart = OriginalStartData;
            RepetitionSequence = RepetitionSequenceData;
            ConflictingEvents = conflicts;
            CalendarEventRange = RangeOfSubCalEvent;
            UniqueID = new EventID(MySubEventID);
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = SubEventBusy.TimelineSpan;
            BusyFrame = SubEventBusy;
            RigidSchedule = Rigid;
            this.Enabled = Enabled;
            this.LocationInfo = EventLocation;
            UiParams = UiParam;
            _DataBlob = Notes;
            Complete = completeFlag;
        }
        #endregion

        #region Class functions

        public virtual string ToString()
        {
            return this.Start.ToString() + " - " + this.End.ToString() + "::" + this.Id + "\t\t::" + this.ActiveDuration.ToString();
        }


        public void disable(CalendarEvent myCalEvent)
        {

            {
                setAsDeviated(0,myCalEvent);
            }
            this.Enabled = false;
            myCalEvent.incrementDeleteCount(this.RangeSpan);
        }


        public override void updateRepetitionIndex(long RepetitionIndex)
        {
            this.RepetitionSequence = RepetitionIndex;
        }
        internal void disableWithoutUpdatingCalEvent()
        {
            this.Enabled = false;
        }

        public void complete(CalendarEvent myCalEvent)
        {
            //if(Rigid)
            {
                setAsDeviated(CalendarEvent.DeviationType.completed, myCalEvent);
            }
            myCalEvent.incrementCompleteCount(this.RangeSpan);
            this.Complete = true;
        }

        public void setAsDeviated(CalendarEvent.DeviationType type,CalendarEvent ParentCalendarEvent)
        {
            ParentCalendarEvent.updateDeviationList(type,this);
        }


        public void nonComplete(CalendarEvent myCalEvent)
        {
            this.Complete = false;
            myCalEvent.decrementCompleteCount(this.RangeSpan);
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
            myCalEvent.decrementDeleteCount(this.RangeSpan);

        }

        internal void enableWithouUpdatingCalEvent()
        {
            this.Enabled = true;
        }

        public void resetPreferredDayIndex()
        {
            PreferredDayIndex = 0;
        }

        public void updateDayIndex(CalendarEvent myCalEvent)
        {
            PreferredDayIndex = ReferenceNow.getDayIndexFromStartOfTime(StartDateTime);
            myCalEvent.removeDayTimeFromFreeUpdays(PreferredDayIndex);
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
            
            double distance = Location_Elements.calculateDistance(refLocation,this.Location);
            TimeLine refTimeLine = new TimeLine(DayTimeLine.Start, CalendarEventRange.End);
            Tuple<TimeLine, double> retValue = new Tuple<TimeLine, double>(refTimeLine,distance);
            return retValue;
        }

        public static SubCalendarEvent getEmptySubCalendarEvent(EventID CalendarEventId)
        {
            SubCalendarEvent retValue = new SubCalendarEvent();
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventId.ToString(),1);
            retValue.StartDateTime = DateTimeOffset.Now;
            retValue.EndDateTime = DateTimeOffset.Now;
            retValue.EventDuration = new TimeSpan(0);
            
            retValue.RigidSchedule= true;
            retValue.Complete = true;
            retValue.Enabled = false;
            return retValue;
        }
        /// <summary>
        /// Function triggeres the deletion of the calendar event. Note deletion is different from disable. Deletion sends a trigger to the deviation
        /// </summary>
        virtual public void delete(CalendarEvent CalendarEventData )
        {
            setAsUserDeleted();
            disable(CalendarEventData);
        }


        

        virtual public SubCalendarEvent createCopy(EventID eventId)
        {
            string Id;
            if (eventId != null)
            {
                Id = eventId.ToString();
            }
            else
            {
                Id = this.Id;
            }
            
           
            SubCalendarEvent MySubCalendarEventCopy = new SubCalendarEvent(Id, Start, End, BusyFrame?.CreateCopy(), OriginalStart, this.RigidSchedule, this.isEnabled, this.UiParams?.createCopy(), this.DataBlob?.createCopy(), this.Complete, RepetitionSequence, this.LocationInfo, new TimeLine(CalendarEventRange.Start, CalendarEventRange.End), ConflictingEvents.CreateCopy());
            MySubCalendarEventCopy.ThirdPartyID = this.ThirdPartyID;
            MySubCalendarEventCopy.DeadlineElapsed = this.DeadlineElapsed;
            MySubCalendarEventCopy.UserDeleted = this.UserDeleted;
            MySubCalendarEventCopy.PreferredDayIndex = this.PreferredDayIndex;
            MySubCalendarEventCopy._Creator = this._Creator;
            MySubCalendarEventCopy.Semantics = this.Semantics != null ? this.Semantics.createCopy() : null;
            MySubCalendarEventCopy._UsedTime = this._UsedTime;
            MySubCalendarEventCopy._Creator = this._Creator;
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
            this.PreferredDayIndex = DayIndex;
        }

        public static void updateDayIndex(ulong DayIndex, IEnumerable<SubCalendarEvent> AllSUbevents)
        {
            foreach (SubCalendarEvent eachSubCalendarEvent in AllSUbevents)
            {
                eachSubCalendarEvent.PreferredDayIndex = DayIndex;
            }
        }

        public void setScore(double score)
        {
            EventScore = score;
        }

        public void incrementScore(double score)
        {
            EventScore += score;
        }

        public static void resetScores(IEnumerable<SubCalendarEvent> AllSUbevents)
        {
            AllSUbevents.AsParallel().ForAll(obj => obj.EventScore = 0);
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
        /// function updates the parameters of the current subcalevent using SubEventEntry. However it doesnt change some data memebers isrestricted. You 
        /// </summary>
        /// <param name="SubEventEntry"></param>
        /// <returns></returns>
        public bool UpdateThis(SubCalendarEvent SubEventEntry,bool force=false)
        {
            if ((this.Id == SubEventEntry.Id))
            {
                if (force)
                {
                    this.BusyFrame = SubEventEntry.ActiveSlot;
                    this.CalendarEventRange = SubEventEntry.getCalendarEventRange;
                    this.FromRepeatEvent = SubEventEntry.FromRepeat;
                    
                    this.EventDuration = SubEventEntry.ActiveDuration;
                    this.Complete = SubEventEntry.isComplete;
                    this.ConflictingEvents = SubEventEntry.Conflicts;
                    this._DataBlob = SubEventEntry.DataBlob;
                    this.DeadlineElapsed = SubEventEntry.isDeadlineElapsed;
                    this.Enabled = SubEventEntry.isEnabled;
                    this.EndDateTime = SubEventEntry.End;
                    this.EventPreDeadline = SubEventEntry.PreDeadline;
                    this.EventScore = SubEventEntry.EvaluatedScore;
                    //this.isRestricted = SubEventEntry.isEventRestricted;
                    this.LocationInfo = SubEventEntry.Location;
                    this.OldPreferredIndex = SubEventEntry.OldUniversalIndex;
                    this.otherPartyID = SubEventEntry.ThirdPartyID;
                    this.PreferredDayIndex = SubEventEntry.UniversalDayIndex;
                    this.PrepTime = SubEventEntry.Preparation;
                    this.Priority = SubEventEntry.EventPriority;
                    //this.ProfileOfNow = SubEventEntry.ProfileOfNow;
                    this.ProfileOfProcrastination = SubEventEntry.ProfileOfProcrastination;
                    //this.RepetitionFlag = SubEventEntry.FromRepeat;
                    this.RigidSchedule = SubEventEntry.Rigid;
                    this.StartDateTime = SubEventEntry.Start;
                    this.UiParams = SubEventEntry.UIParam;
                    this.UniqueID = SubEventEntry.SubEvent_ID;
                    this.NameOfEvent = new EventName(UniqueID, NameString);
                    this.UserDeleted = SubEventEntry.isUserDeleted;
                    this.UserIDs = SubEventEntry.getAllUsers();
                    this.Vestige = SubEventEntry.isVestige;
                    this.otherPartyID = SubEventEntry.otherPartyID;
                    return true;
                }
                else
                {
                    if (canExistWithinTimeLine(SubEventEntry.getCalendarEventRange))
                    {
                        this.BusyFrame = SubEventEntry.ActiveSlot;
                        this.CalendarEventRange = SubEventEntry.getCalendarEventRange;
                        this.FromRepeatEvent = SubEventEntry.FromRepeat;
                        this.EventDuration = SubEventEntry.ActiveDuration;
                        this.Complete = SubEventEntry.isComplete;
                        this.ConflictingEvents = SubEventEntry.Conflicts;
                        this._DataBlob = SubEventEntry.DataBlob;
                        this.DeadlineElapsed = SubEventEntry.isDeadlineElapsed;
                        this.Enabled = SubEventEntry.isEnabled;
                        this.EndDateTime = SubEventEntry.End;
                        this.EventPreDeadline = SubEventEntry.PreDeadline;
                        this.EventScore = SubEventEntry.EvaluatedScore;
                        //this.isRestricted = SubEventEntry.isEventRestricted;
                        this.LocationInfo = SubEventEntry.Location;
                        this.OldPreferredIndex = SubEventEntry.OldUniversalIndex;
                        this.otherPartyID = SubEventEntry.ThirdPartyID;
                        this.PreferredDayIndex = SubEventEntry.UniversalDayIndex;
                        this.PrepTime = SubEventEntry.Preparation;
                        this.Priority = SubEventEntry.EventPriority;
                        //this.ProfileOfNow = SubEventEntry.ProfileOfNow;
                        this.ProfileOfProcrastination = SubEventEntry.ProfileOfProcrastination;
                        //this.RepetitionFlag = SubEventEntry.FromRepeat;
                        this.RigidSchedule = SubEventEntry.Rigid;
                        this.StartDateTime = SubEventEntry.Start;
                        this.UiParams = SubEventEntry.UIParam;
                        this.UniqueID = SubEventEntry.SubEvent_ID;
                        this.NameOfEvent = new EventName(UniqueID, SubEventEntry.NameString);
                        this.UserDeleted = SubEventEntry.isUserDeleted;
                        this.UserIDs = SubEventEntry.getAllUsers();
                        this.Vestige = SubEventEntry.isVestige;
                        this.otherPartyID = SubEventEntry.otherPartyID;
                        return true;
                    }
                    throw new Exception("Error Detected: Trying to update SubCalendarEvent that cant fit in Calendar event range");
                }
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
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventData.Id,RepetitionSequence);
            retValue.shiftEvent(SpanShift,true);
            return retValue;
        }

        virtual public SubCalendarEvent getNowCopy(EventID CalendarEventID, NowProfile NowData)
        {
            SubCalendarEvent retValue = getCalulationCopy();
            retValue.RigidSchedule = true;
            TimeSpan SpanShift = NowData.PreferredTime - retValue.Start;
            retValue.UniqueID = EventID.GenerateSubCalendarEvent(CalendarEventID.ToString(), RepetitionSequence);
            retValue.shiftEvent(SpanShift, true);
            //this.ProfileOfNow = NowData;
            return retValue;
        }

        virtual protected SubCalendarEvent getCalulationCopy()
        {
            SubCalendarEvent retValue = new SubCalendarEvent();
            retValue.BusyFrame = this.ActiveSlot.CreateCopy();
            retValue.CalendarEventRange = this.getCalendarEventRange.CreateCopy();
            retValue.FromRepeatEvent = this.FromRepeat;
            retValue.EventDuration = this.ActiveDuration;
            retValue.Complete = this.isComplete;
            retValue.ConflictingEvents = this.Conflicts;
            retValue._DataBlob = this.DataBlob;
            retValue.DeadlineElapsed = this.isDeadlineElapsed;
            retValue.Enabled = this.isEnabled;
            retValue.EndDateTime = this.End;
            retValue.EventPreDeadline = this.PreDeadline;
            retValue.EventScore = this.EvaluatedScore;
            retValue.LocationInfo = this.Location.CreateCopy();
            retValue.OldPreferredIndex = this.OldUniversalIndex;
            retValue.otherPartyID = this.ThirdPartyID;
            retValue.PreferredDayIndex = this.UniversalDayIndex;
            retValue.PrepTime = this.Preparation;
            retValue.Priority = this.EventPriority;
            //retValue.ProfileOfNow = this.ProfileOfNow;
            retValue.ProfileOfProcrastination = this.ProfileOfProcrastination.CreateCopy();
            //retValue.RepetitionFlag = this.FromRepeat;
            retValue.RigidSchedule = this.Rigid;
            retValue.StartDateTime = this.Start;
            retValue.UiParams = this.UIParam;
            retValue.UniqueID = this.SubEvent_ID;
            retValue.NameOfEvent = new EventName(retValue.UniqueID, this.NameString);
            retValue.UserDeleted = this.isUserDeleted;
            retValue.UserIDs = this.getAllUsers();
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
            if (new EventID(RestrctingCalendarEvent.Id).getCalendarEventComponent() != UniqueID.getCalendarEventComponent())
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
                var locationA = Arg1.Location ?? new Location_Elements();
                var locationB = Arg2.Location ?? new Location_Elements();
                return Location_Elements.calculateDistance(locationA, locationB, worstDistance);
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
             SubCalendarEvent thisCopy = this.createCopy(this.UniqueID);
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

        virtual public DateTimeOffset getPauseTime()
        {
            return _PauseTime;
        }
        virtual internal TimeSpan Pause(DateTimeOffset currentTime)
        {
            _PauseTime = currentTime;
            DateTimeOffset Start = this.Start;
            DateTimeOffset End = this.End;
            TimeSpan NewUsedTime = _PauseTime - Start;

            _UsedTime = NewUsedTime;
            return NewUsedTime;
        }

        virtual internal bool Continue(DateTimeOffset currentTime)
        {
            _PauseTime = new DateTimeOffset();
            TimeSpan timeDiff = (currentTime- UsedTime) - (Start);
            bool RetValue = shiftEvent(timeDiff);
            return RetValue;
        }

        virtual public bool UnPause(DateTimeOffset currentTime)
        {
            _PauseTime = new DateTimeOffset();
            _UsedTime = new TimeSpan();
            TimeSpan timeDiff = new TimeSpan();
            bool RetValue = shiftEvent(timeDiff);
            return RetValue;
        }

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
                 return PreferredDayIndex;
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
                 BusyFrame.updateBusyTimeLine(new BusyTimeLine(Id, ActiveSlot.Start, ActiveSlot.Start.Add(EventDuration)));
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

        public virtual SubCalendarEventPersist ConvertToPersistable(Location_Elements location = null, NowProfile nowProfile=null, Procrastination procrastination = null, EventName eventName = null, MiscData dataBlob= null, Classification classification= null, EventDisplay uiData = null)
        {
            if((location != null)&& (this.Location != null))
            {
                if (location.Id != this.Location.Id)
                {
                    if (!this.Location.isNull)
                    {
                        location = DB.DB_LocationElements.ConvertToPersistable(this.LocationInfo, this.CreatorId);
                    }
                    else
                    {
                        location = null;
                    }
                }
                else/// if location id of passed arfuemwnt is the same as the location id of local data memeber then we can assume they equivalent, and we are assuming changes are universal
                {
                }
            }
            else
            {
                if (this.Location != null)
                {
                    if(!this.Location.isNull)
                    {
                        location = DB.DB_LocationElements.ConvertToPersistable(this.LocationInfo, this.CreatorId);
                    }
                    else
                    {
                        location = null;
                    }
                }
                else
                {
                    location = null;
                }
            }


            ///Now Profile
            if ((nowProfile != null)&& (this.NowInfo != null))
            {
                if (nowProfile.Id != this.NowInfo.Id)
                {
                    if (this.NowInfo.isInitialized)
                    {
                        nowProfile = DB.DB_NowProfile.ConvertToPersistable(this.NowInfo);
                    }
                    else
                    {
                        nowProfile = null;
                    }
                }
                else///if nowprofile id passed on is same as datamember's now profile id then we can assume they are the same. We are ignorign any changes
                {
                    
                }
            }
            else
            {
                if (this.NowInfo != null)
                {
                    if (this.NowInfo.isInitialized)
                    {
                        nowProfile = DB.DB_NowProfile.ConvertToPersistable(this.NowInfo);
                    }
                    else
                    {
                        nowProfile = null;
                    }
                }
                else
                {
                    nowProfile = null;
                }
            }

            //Procrastination switch
            if ((procrastination != null)&& (this.ProcrastinationInfo != null))
            {
                if (procrastination.Id != this.ProcrastinationInfo.Id)
                {
                    if (this.ProcrastinationInfo.IsInitialized())
                    {
                        procrastination = DB.DB_Procrastination.ConvertToPersistable(this.ProcrastinationInfo);
                    }
                    else
                    {
                        procrastination = null;
                    }
                }
                else/// if passed calendar event procrastination id equals datamember procrastination id then continue because they are both the same
                {
                    
                }
            }
            else
            {
                if (this.ProcrastinationInfo != null)
                {
                    if (this.ProcrastinationInfo.IsInitialized())
                    {
                        procrastination = DB.DB_Procrastination.ConvertToPersistable(this.ProcrastinationInfo);
                    }
                    else
                    {
                        procrastination = null;
                    }
                }
                else
                {
                    procrastination = null;
                }
            }


            //Event name switch
            if ((eventName != null)&&(this.NameOfEvent != null))
            {
                if (eventName.Id != this.NameOfEvent.Id)
                {
                    eventName = DB.DB_EventName.ConvertToPersistable(this.NameOfEvent);
                }
                else////if passed eventName(calenedar event name) has same id then continue
                {
                    //eventName = DB.DB_EventName.ConvertToPersistable(eventName);
                }
            }
            else
            {
                if (this.NameOfEvent != null)
                {
                    eventName = DB.DB_EventName.ConvertToPersistable(this.NameOfEvent);
                }
                else
                {
                    eventName = null;
                }
            }


            //datablob conversion
            if ((dataBlob != null)&&(this.DataBlob!=null))
            {
                if (dataBlob.Id != this.DataBlob.Id)
                {
                    dataBlob = DB.DB_MiscData.ConvertToPersistable(this.DataBlob);
                }
                else/// if calendarevent datablob id has same id as local dataelement datablob id, then continue
                {
                    //dataBlob = DB.DB_MiscData.ConvertToPersistable(dataBlob);
                }
            }
            else
            {
                if (this.DataBlob != null)
                {
                    dataBlob = DB.DB_MiscData.ConvertToPersistable(this.DataBlob);
                }
                else
                {
                    dataBlob = null;
                }
            }

            //classification conversion
            if ((classification != null)&&(this.Classification!=null))
            {
                if (classification.Id != this.Classification.Id)
                {
                    classification = DB.DB_Classification.ConvertToPersistable(this.Classification);
                }
                else/// if current classification id == calendareven  classification id then continue
                {
                    ///classification = DB.DB_Classification.ConvertToPersistable(classification);
                }
            }
            else
            {
                if (this.Classification != null)
                {
                    classification = DB.DB_Classification.ConvertToPersistable(this.Classification);
                }
                else
                {
                    classification = null;
                }
            }

            //uiData conversion
            if ((uiData != null) && (this.UIParam != null))
            {
                if (uiData.Id != this.UIParam.Id)
                {
                    uiData = DB.DB_EventDisplay.ConvertToPersistable(this.UIParam);
                }
                else/// if caleventData UIdata == current UIdata then continue
                {
                    //uiData = DB.DB_EventDisplay.ConvertToPersistable(uiData);
                }
            }
            else
            {
                if (this.UIParam != null)
                {
                    uiData = DB.DB_EventDisplay.ConvertToPersistable(this.UIParam);
                }
                else
                {
                    uiData = null;
                }
            }



            //procrastination = procrastination ?? DB.DB_Procrastination.ConvertToPersistable(this.ProcrastinationInfo);
            //eventName = eventName ?? DB.DB_EventName.ConvertToPersistable(this.NameOfEvent);
            //dataBlob = dataBlob ?? DB.DB_MiscData.ConvertToPersistable(this._DataBlob);
            DB.DB_SubCalendarEventFly RetValue = new DB.DB_SubCalendarEventFly()
            {
                BusyFrame = this.BusyFrame,
                CalculationMode = this.CalculationMode,
                CalendarEventRange = this.CalendarEventRange,
                ConflictingEvents = this.ConflictingEvents,
                EventScore = this.EventScore,
                HumaneTimeLine = this.HumaneTimeLine,
                MiscIntData = this.MiscIntData,
                MuddledEvent = this.MuddledEvent,
                NonHumaneTimeLine = this.NonHumaneTimeLine,
                OldPreferredIndex = this.OldPreferredIndex,
                PreferredDayIndex = this.PreferredDayIndex,
                UnUsableIndex = this.UnUsableIndex,
                Vestige = this.Vestige,
                _PauseTime = this._PauseTime,
                Complete = this.Complete,
                ConflictSetting = this.ConflictSetting,
                DeadlineElapsed = this.DeadlineElapsed,
                DeviationFlag = this.DeviationFlag,
                Enabled = this.Enabled,
                EndDateTime = this.EndDateTime,
                EventDuration = this.EventDuration,
                EventPreDeadline = this.EventPreDeadline,
                FromRepeatEvent = this.FromRepeatEvent,
                LocationInfo = location,
                NameOfEvent = this.NameOfEvent,
                OriginalEventID = this.OriginalEventID,
                OriginalStart = this.OriginalStart,
                otherPartyID = this.otherPartyID,
                PrepTime = this.PrepTime,
                Priority = this.Priority,
                ProfileOfNow = nowProfile,
                ProfileOfProcrastination = procrastination,
                RepetitionSequence = this.RepetitionSequence,
                RigidSchedule = this.RigidSchedule,
                Semantics = this.Semantics,
                StartDateTime = this.StartDateTime,
                ThirdPartyFlag = this.ThirdPartyFlag,
                ThirdPartyTypeInfo = this.ThirdPartyTypeInfo,
                ThirdPartyUserIDInfo = this.ThirdPartyUserIDInfo,
                UiParams = this.UiParams,
                UniqueID = this.UniqueID,
                UserDeleted = this.UserDeleted,
                UserIDs = this.UserIDs,
                Creator = this.EventCreator,
                CreatorId = this._CreatorId?? this._Creator.Id,
                _DataBlob = dataBlob,
                _UsedTime = this._UsedTime,
                IsEventModified = this.IsEventModified,
                Name = eventName,
                Classification = classification,
                UIData = uiData

            };
            return RetValue;
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

        public override DateTimeOffset Start
        {
            get
            {
                return BusyFrame.Start;
            }
            set
            {
                this.StartDateTime = value;
                BusyFrame = new BusyTimeLine(base.Id, value, BusyFrame.End);
            }
        }

        public override DateTimeOffset End
        {
            get
            {
                return BusyFrame.End;
            }
            set
            {
                this.EndDateTime = value;
                BusyFrame = new BusyTimeLine(base.Id, BusyFrame.Start, value);
            }
        }

        public bool isDesignated
        {
            get
            {
                bool retValue = PreferredDayIndex != 0;
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

        public double EvaluatedScore
        {
            get 
            {
                return EventScore;
            }/*
            set
            {
                EventScore = value;
            }*/
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
                return MuddledEvent;
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
                retValue.EventLocation = Location.toStruct();
                return retValue;
            }
        }

        override public MiscData DataBlob
        { 
            get
            {
                return _DataBlob;
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

        public bool isPaused
        {
            get
            {
                return getPauseTime() != InitialPauseTime;
            }
        }
        #endregion

    }
}


