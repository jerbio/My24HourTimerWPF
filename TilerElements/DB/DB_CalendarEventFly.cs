using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements.Wpf;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TilerElements.DB;

namespace TilerElements.DB
{

    public class DB_CalendarEventFly : DB_CalendarEvent
    {
        int DeletedSofar =0;
        int CompletedSofar = 0;

        protected List<SubCalendarEvent> ListBackedSubEVents = new List<SubCalendarEvent>();

        public DB_CalendarEventFly()//:base()
        {

        }

        public DB_CalendarEventFly(string EventIDData, EventName Name, DateTimeOffset StartData, DateTimeOffset EndData, int PriorityData, DB_Repetition RepetitionData, Location_Elements LocationData, TimeSpan TImePerSplitData, DateTimeOffset OriginalStartData, TimeSpan EventPrepTimeData, TimeSpan Event_PreDeadlineData, bool EventRigidFlagData, int SplitData, EventDisplay UiData, MiscData NoteData, bool CompletionFlagData, long RepeatIndexData, Procrastination ProcrastinationData, NowProfile NowProfileData, int CompleteCountData, int DeletionCountData,ICollection<TilerUser> AllUserIDs)
        {
            string message = "Halt JEROME !!!!!. This was a commit knowing this error will happen" +
    " You did this because you want to figure out your next steps.\n"
    + " You deleted all refereneces to the NowProfile in SubCalendarEVents and TilerEVents because they were inherited from TIler Events.\n"
    + "You did this because you think it was not needed and could easily be stored in the calendar event object. Since the calendar object can explicitly store a deviating subcalendar event and update the calculated rigid event with the deviation.\n"
    + "Beware Jerome of the case where a repeating rigid event gets created and then now is pressed. Tiler needs to know which rigid event was pressed to accomplish this now activity\n"
    + " You deleted the now profile because it was making the xml file too big and was hampering read performnace\n"
    + " You realized that non-rigid subevents still get persisted and are not calculated on the fly which is unlike their rigid counterparts(I havent tested the latter part because, but this branch is called newrigidimplementation aka on the fly rigid calculations).\n"
    + " You might want to resdesign the calls for the creation of non-rigid subevents to be calculated on the fly";
            //throw new Exception(message);
            
            StartDateTime = StartData;
            EndDateTime = EndData;
            EventRepeat = RepetitionData;
            LocationInfo = LocationData;
            TimePerSplit = TImePerSplitData;
            OriginalStart = OriginalStartData;
            PrepTime = EventPrepTimeData;
            EventPreDeadline = Event_PreDeadlineData;
            ProfileOfNow = NowProfileData;
            RigidSchedule = EventRigidFlagData;
            Complete = CompletionFlagData;
            CompletedCount = CompleteCountData;
            DeletedCount = DeletionCountData;
            ProfileOfProcrastination = ProcrastinationData;
            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            Priority = PriorityData;
            RepetitionSequence = 0;
            Splits = SplitData;
            UniqueID = new EventID(EventIDData);
            UserIDs = AllUserIDs.ToList();
            
            if (EventRepeat.Enable)
            {
                EventRepeat.PopulateRepetitionParameters(this);
            }
            else
            {
                
                DateTimeOffset SubEventEndData =  EndData;
                DateTimeOffset SubEventStartData =  SubEventEndData-TimePerSplit;
                int i = 0;
                int SubEventCount = Splits - (CompletedCount + DeletedCount);
                for (int j=0; j < SubEventCount; i++,j++)
                {
                    EventID SubEventID= EventID.GenerateSubCalendarEvent(UniqueID.ToString(),i+1);
                    SubCalendarEvent newSubCalEvent = new DB_SubCalendarEventFly(SubEventID, Name, SubEventStartData, SubEventEndData, PriorityData, LocationInfo.CreateCopy(), OriginalStart, EventPrepTimeData, Event_PreDeadlineData, EventRigidFlagData, UiData.createCopy(), NoteData.createCopy(), Complete, ProcrastinationData, this.RangeTimeLine, EventRepeat.Enable, false, true, AllUserIDs.ToList(),i);
                    SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
                }
            }
            NameOfEvent = Name;
            UpdateLocationMatrix(Location);
        }
        #region functions
        static public CalendarEvent InstantiateRepeatedCandidate(EventID EventIDEntry, EventName EventName, TimeSpan Event_Duration, DateTimeOffset EventStart, DateTimeOffset EventDeadline, DateTimeOffset OriginalStartData, TimeSpan EventPrepTime, TimeSpan Event_PreDeadline, bool EventRigidFlag, DB_Repetition EventRepetitionEntry, int EventSplit, Location_Elements EventLocation, bool enabledFlag, EventDisplay UiData, MiscData NoteData, bool CompletionFlag, long RepeatIndex, ConcurrentDictionary<DateTimeOffset, CalendarEvent> OrginalStartToCalendarEvent, DB_CalendarEvent RepeatRootData)
        {

            DB_CalendarEventFly RetValue = new DB_CalendarEventFly();
            //RetValue.EventName = EventName;
            RetValue.StartDateTime = EventStart;
            RetValue.EndDateTime = EventDeadline;
            RetValue.EventDuration = Event_Duration;
            RetValue.Enabled = enabledFlag;
            RetValue.EventRepeat = EventRepetitionEntry;
            RetValue.PrepTime = EventPrepTime;
            RetValue.EventPreDeadline = Event_PreDeadline;
            RetValue.RigidSchedule = EventRigidFlag;
            RetValue.LocationInfo = EventLocation;
            RetValue.UniqueID = EventIDEntry;
            RetValue.UiParams = UiData;
            RetValue._DataBlob = NoteData;
            RetValue.Complete = CompletionFlag;
            RetValue.RepetitionSequence = RepeatIndex;
            RetValue.OriginalStart = OriginalStartData;
            RetValue.Splits = EventSplit;
            RetValue.TimePerSplit = TimeSpan.FromTicks(((RetValue.EventDuration.Ticks / RetValue.Splits)));
            RetValue.FromRepeatEvent = true;
            RetValue.NameOfEvent = EventName;
            /*
            if (RetValue.EventRepetition.Enable)
            {
                RetValue.Splits = EventSplit;
                RetValue.TimePerSplit = new TimeSpan();
            }
            else
            {
                RetValue.Splits = EventSplit;
            }
            */
            RetValue.SubEvents = new Dictionary<EventID, SubCalendarEvent>();

            if (!RetValue.EventRepeat.Enable)
            { 
                for (int i = 0; i < RetValue.Splits; i++)
                {
                    //(TimeSpan Event_Duration, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, string myParentID, bool Rigid, Location EventLocation =null, TimeLine RangeOfSubCalEvent = null)
                    EventID SubEventID = EventID.GenerateSubCalendarEvent(RetValue.UniqueID.ToString(), i + 1);
                    SubCalendarEvent newSubCalEvent = new DB_SubCalendarEventFly(SubEventID, RetValue.NameOfEvent, (RetValue.EndDateTime - RetValue.TimePerSplit), RetValue.EndDateTime, RetValue.Priority, RetValue.Location, RetValue.OriginalStart, RetValue.Preparation, RetValue.PreDeadline, RetValue.Rigid, RetValue.UIParam.createCopy(), RetValue.Notes.createCopy(), false, RetValue.ProcrastinationInfo, RetValue.RangeTimeLine, true, false, true, RetValue.UserIDs,i);

                    //SubCalendarEvent newSubCalEvent = new SubCalendarEvent(RetValue.TimePerSplit, (RetValue.EndDateTime - RetValue.TimePerSplit), RetValue.End, new TimeSpan(), OriginalStartData, RetValue.UniqueID.ToString(), RetValue.RigidSchedule, RetValue.isEnabled, RetValue.UiParams, RetValue.Notes, RetValue.Complete, i+1, EventLocation, RetValue.RangeTimeLine);
                    RetValue.SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
                }
            }
            RetValue.EventSequence = new TimeLine(RetValue.StartDateTime, RetValue.EndDateTime);
            RetValue.RepeatRoot = RepeatRootData;
            RetValue.UpdateLocationMatrix(RetValue.LocationInfo);
            

            while(! OrginalStartToCalendarEvent.TryAdd(OriginalStartData,RetValue))
            {
                Thread.Sleep(10);
            }
            

            return RetValue;
        }

        public void UpdateModifiable(IEnumerable<SubCalendarEvent>Modifiables)
        {
            foreach(SubCalendarEvent eachSubCalendarEvent in Modifiables)
            {
                DB_CalendarEventFly RefCalEvent = (DB_CalendarEventFly)getCalEventByOrginalStart(eachSubCalendarEvent.OrginalStartInfo);
                
                
                if(eachSubCalendarEvent.isComplete)
                {
                    if ((RefCalEvent.CompletedSofar < (RefCalEvent.Splits - RefCalEvent.DeletedSofar)))
                    {
                        
                        DB_SubCalendarEventFly myDB_SubCalendarEventFly = (DB_SubCalendarEventFly)RefCalEvent.ActiveSubEvents.Last();
                        myDB_SubCalendarEventFly.InitializeCompleted(eachSubCalendarEvent);
                        try
                        {
                            RefCalEvent.updateDeviationList(CalendarEvent.DeviationType.completed, myDB_SubCalendarEventFly);
                            ++RefCalEvent.CompletedSofar;
                            RefCalEvent.CompletedCount = RefCalEvent.CompletedSofar;
                            continue;
                        }
                        catch(Exception e)
                        {

                        }
                    } 
                }

                if (!eachSubCalendarEvent.isEnabled)
                {
                    if ( (RefCalEvent.DeletedSofar < (RefCalEvent.Splits-RefCalEvent.CompletedSofar)))
                    {
                        DB_SubCalendarEventFly myDB_SubCalendarEventFly = (DB_SubCalendarEventFly)RefCalEvent.ActiveSubEvents.Last();
                        myDB_SubCalendarEventFly.InitializeDisabled(eachSubCalendarEvent);
                        try
                        {
                            RefCalEvent.updateDeviationList(0, myDB_SubCalendarEventFly);
                            ++RefCalEvent.DeletedSofar;
                            RefCalEvent.DeletedCount = RefCalEvent.DeletedSofar;
                            continue;
                        }
                        catch (Exception e)
                        {

                        }                        
                    }
                }

            }
        }


        


        CalendarEvent getCalEventByOrginalStart(DateTimeOffset OrginalStartData)
        {
            CalendarEvent RetValue = this;
            if(EventRepeat.Enable)
            {
                RetValue= EventRepeat.getCalendarEventByOriginalStart(OrginalStartData);
            }
            return RetValue;
        }
        #endregion


        #region properties
        public override DateTimeOffset CalculationEnd
        {
            get
            {
                return this.EndOfCalculation;
            }

            set
            {
                this.EndOfCalculation = value;
            }
        }

        public override int CompleteCount
        {
            get
            {
                return this.CompletedCount;
            }

            set
            {
                this.CompletedCount = value;
            }
        }


        public override int DeleteCount
        {
            get
            {
                return this.DeletedCount;
            }

            set
            {
                this.DeletedCount = value;
            }
        }

        public override DateTimeOffset InitializingStart
        {
            get
            {
                return this.OriginalStart;
            }

            set
            {
                this.OriginalStart = value;
            }
        }

        public override bool isDeleted
        {
            get
            {
                return this.Enabled;
            }

            set
            {
                this.Enabled = value;
            }
        }

        public override bool isDeletedByUser
        {
            get
            {
                return this.UserDeleted;
            }

            set
            {
                this.UserDeleted = value;
            }
        }

        public override bool isDeviated
        {
            get
            {
                return this.DeviationFlag;
            }

            set
            {
                this.DeviationFlag = value;
            }
        }

        public override bool isRepeat
        {
            get
            {
                return this.FromRepeatEvent;
            }

            set
            {
                this.FromRepeatEvent = value;
            }
        }

        public override bool isRigid
        {
            get
            {
                return this.RigidSchedule;
            }

            set
            {
                this.RigidSchedule = value;
            }
        }

        public override NowProfile LastNowProfile
        {
            get
            {
                return this.ProfileOfNow;
            }

            set
            {
                this.ProfileOfNow = value;
            }
        }

        public override TimeSpan OriginalTimeSpanPerSplit
        {
            get
            {
                return this.InitializingTimeSpanPerSplit;
            }

            set
            {
                this.InitializingTimeSpanPerSplit = value;
            }
        }

        public override Procrastination ProcrastinationProfile
        {
            get
            {
                return this.ProfileOfProcrastination;
            }

            set
            {
                this.ProfileOfProcrastination = value;
            }
        }

        public override int SplitCount
        {
            get
            {
                return this.Splits;
            }

            set
            {
                this.Splits = value;
            }
        }

        public override ICollection<SubCalendarEvent> SubCalendarEvents
        {
            get
            {
                if (calculationReadyFlag)
                {
                    return SubEvents.Values.ToList();
                }
                else
                {
                    return ListBackedSubEVents;
                }
            }
            set
            {
                if (calculationReadyFlag)
                {
                    SubEvents = value.ToDictionary(obj => obj.SubEvent_ID, obj => obj);
                }
                else
                {
                    ListBackedSubEVents = value.ToList();
                }

                
            }
        }

        public override void PrepareForCalculation()
        {
            SubEvents = ListBackedSubEVents.ToDictionary(obj => obj.SubEvent_ID, obj => obj);
            calculationReadyFlag = true;
        }

        public override TimeSpan TimeSpanPerSplit
        {
            get
            {
                return this.TimePerSplit;
            }

            set
            {
                TimePerSplit = value;
            }
        }

        public override EventDisplay UIData
        {
            get
            {
                return this.UIParam;
            }

            set
            {
                this.UiParams = value;
            }
        }


        public override int Urgency
        {
            get
            {
                return this.Priority;
            }

            set
            {
                this.Priority = value;
            }
        }

        public override ICollection<TilerUser> Users
        {
            get
            {
                return UserIDs;
            }

            set
            {
                UserIDs = value.ToList();
            }
        }

        public override DateTimeOffset EndTime
        {
            get
            {
                return this.EndDateTime;
            }

            set
            {
                this.EndDateTime = value;
                EventSequence = new TimeLine(this.Start, this.End);
            }
        }

        public override Repetition EventRepeat
        {
            get
            {
                if (RepetitionStatus)
                {
                    return Repeat;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value == null)
                {
                    this.EventRepeat = new Repetition();
                }
                else {
                    this.EventRepeat = value;
                }
            }
        }

        //public override TimeLine EventSequence
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        public override bool CompleteFlag
        {
            get
            {
                return this.Complete;
            }

            set
            {
                this.Complete = value;
            }
        }

        public override EventName Name
        {
            get
            {
                return this.NameOfEvent;
            }
            set
            {
                this.NameOfEvent = value;
            }
        }

        public override MiscData Notes
        {
            get
            {
                return this._DataBlob;
            }

            set
            {
                this._DataBlob = value;
            }
        }

        public override CalendarEvent RepeatRoot
        {
            get
            {
                return this.RootOfRepeat;
            }

            set
            {
                this.RootOfRepeat = value;
            }
        }

        public override DateTimeOffset StartTime
        {
            get
            {
                return StartDateTime;
            }
            set
            {
                this.StartDateTime = value;
                EventSequence = new TimeLine(this.Start, this.End);
            }
        }

        public virtual CalendarEventPersist ConvertToPersistableCalendarEvent()
        {
            return this;
        }
        #endregion
    }
}

