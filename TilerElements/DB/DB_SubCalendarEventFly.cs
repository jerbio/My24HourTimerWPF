using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements.Wpf;
using System.ComponentModel.DataAnnotations.Schema;

namespace TilerElements.DB
{
    //[Table("SubCalendarEvents")]
    public class DB_SubCalendarEventFly : DB_SubCalendarEvent
    {
        public DB_SubCalendarEventFly()
        {
            CalendarEventRange = new TimeLine();
            BusyFrame = new BusyTimeLine();
            HumaneTimeLine = new BusyTimeLine();
        }

        #region functions
        internal DB_SubCalendarEventFly(EventID EventIDData, EventName Name, DateTimeOffset StartData, DateTimeOffset EndData, int PriorityData, Location_Elements LocationData, DateTimeOffset OriginalStartData, TimeSpan EventPrepTimeData, TimeSpan Event_PreDeadlineData, bool EventRigidFlagData, EventDisplay UiData, MiscData NoteData, bool CompletionFlagData, Procrastination ProcrastinationData, TimeLine CalendarEventRangeData, bool FromRepeatFlagData, bool ElapsedFlagData, bool EnabledFlagData, ICollection<TilerUser> UserIDData, long Sequence)
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
            this.BusyFrame = new BusyTimeLine(EventIDData.ToString(), StartData, EndData);
            this.CalendarEventRange = CalendarEventRangeData;
            this.FromRepeatEvent = FromRepeatFlagData;
            this.EventDuration = BusyFrame.BusyTimeSpan;
            this.Complete = CompletionFlagData;
            this.ConflictingEvents = new ConflictProfile();
            this._DataBlob = NoteData;
            this.DeadlineElapsed = ElapsedFlagData;
            this.Enabled = EnabledFlagData;
            this.StartDateTime = StartData;
            this.EndDateTime = EndData;
            this.EventPreDeadline = Event_PreDeadlineData;
            this.RepetitionSequence = Sequence;
            this.LocationInfo = LocationData;
            //this.OldPreferredIndex = mySubCalEvent.OldUniversalIndex;
            //this.otherPartyID = mySubCalEvent.ThirdPartyID;
            //this.preferredDayIndex = mySubCalEvent.UniversalDayIndex;
            this.PrepTime = EventPrepTimeData;
            this.Priority = PriorityData;
            //this.ProfileOfNow = NowProfileData;
            this.ProfileOfProcrastination = ProcrastinationData;
            //this.RepetitionFlag = mySubCalEvent.FromRepeat;
            this.RigidSchedule = EventRigidFlagData;

            this.UiParams = UiData;
            this.UniqueID = EventIDData;
            this.NameOfEvent = Name;
            this.UserIDs = Users.ToList();
            this.OriginalStart = OriginalStartData;
        }

        internal void InitializeDisabled(SubCalendarEvent SubCalendarEventData)
        {
            if (!SubCalendarEventData.isEnabled)
            {
                TimeSpan SPanShift = SubCalendarEventData.Start - this.Start;
                this.shiftEvent(SPanShift, true);
                this.Enabled = SubCalendarEventData.isEnabled;
                return;
            }
            throw new Exception("Trying to set undelete event as deleted, check DB_SubCalendarEventFly");
        }


        internal void InitializeCompleted(SubCalendarEvent SubCalendarEventData)
        {
            if (SubCalendarEventData.isComplete)
            {
                TimeSpan SPanShift = SubCalendarEventData.Start - this.Start;
                this.shiftEvent(SPanShift, true);
                this.Complete = SubCalendarEventData.isComplete;
                return;
            }
            throw new Exception("Trying to set uncomplete event as completed, check DB_SubCalendarEventFly");
        }

        public static DB_SubCalendarEventFly toDB_SubCalendarEvent(SubCalendarEvent Event)
        {
            DB_SubCalendarEventFly RetValue = new DB_SubCalendarEventFly();
            RetValue.UniqueID = Event.SubEvent_ID;
            RetValue.UpdateThis(Event);
            return RetValue;
        }
        #endregion
        #region properties



        override public DateTimeOffset CalendarEnd
        {
            get
            {
                return CalendarEventRange.End;
            }
            set
            {
                if (CalendarEventRange == null)
                {
                    CalendarEventRange = new TimeLine(new DateTimeOffset(), value);
                }
                else
                {
                    CalendarEventRange = new TimeLine(CalendarEventRange.Start, value);
                }
            }
        }

        override public DateTimeOffset CalendarStart
        {
            get
            {
                return CalendarEventRange.Start;
            }
            set
            {
                if (CalendarEventRange == null)
                {
                    CalendarEventRange = new TimeLine(value, value.AddSeconds(1));
                }
                else
                {
                    CalendarEventRange = new TimeLine(value, CalendarEventRange.End);
                }
            }
        }

        /// <summary>
        /// returns the humane end time i.e ideal start time for a subevent to start
        /// </summary>
        override public DateTimeOffset HumaneStart
        {
            get
            {
                return HumaneTimeLine.Start;
            }
            set
            {
                HumaneTimeLine = new BusyTimeLine(Id, value, HumaneTimeLine.End);
            }
        }
        /// <summary>
        /// returns the humane end time i.e ideal end time for a subevent to end
        /// </summary>
        override public DateTimeOffset HumaneEnd
        {
            get
            {
                return HumaneTimeLine.End;
            }
            set
            {
                HumaneTimeLine = new BusyTimeLine(Id, HumaneTimeLine.Start, value);
            }
        }

        /// <summary>
        /// returns the NonHumane end time i.e ideal start time for a subevent to start
        /// </summary>
        override public DateTimeOffset NonHumaneStart
        {
            get
            {
                return NonHumaneTimeLine.Start;
            }
            set
            {
                NonHumaneTimeLine = new BusyTimeLine(Id, value, NonHumaneTimeLine.End);
            }
        }
        /// <summary>
        /// returns the NonHumane end time i.e ideal end time for a subevent to end
        /// </summary>
        override public DateTimeOffset NonHumaneEnd
        {
            get
            {
                return NonHumaneTimeLine.End;
            }
            set
            {
                NonHumaneTimeLine = new BusyTimeLine(Id, NonHumaneTimeLine.Start, value);
            }
        }

        override public ulong OldDayIndex
        {
            get
            {
                return OldPreferredIndex;
            }
            set
            {
                OldPreferredIndex = value;
            }
        }
        override public ulong DesiredDayIndex
        {
            get
            {
                return PreferredDayIndex;
            }
            set
            {
                PreferredDayIndex = value;
            }
        }

        override public ulong InvalidDayIndex
        {
            get
            {
                return UnUsableIndex;
            }
            set
            {
                UnUsableIndex = value;
            }
        }


        override public DateTimeOffset InitializingStart
        {
            get
            {
                return OriginalStart;
            }
            set
            {
                OriginalStart = value;
            }
        }

        override public bool isDeletedByUser
        {
            get
            {
                return UserDeleted;
            }
            set
            {
                UserDeleted = value;
            }
        }

        override public bool isRepeat
        {
            get
            {
                return FromRepeatEvent;
            }
            set
            {
                FromRepeatEvent = value;
            }
        }


        override public bool CompleteFlag
        {
            get
            {
                return Complete;
            }
            set
            {
                Complete = value;
            }
        }

        /// <summary>
        /// Function gets and sets the priority of the current task.
        /// </summary>
        override public int Urgency
        {
            get
            {
                return Priority;
            }
            set
            {
                Priority = value;
            }
        }

        /// <summary>
        /// Function gets and sets the conflict setting for an event. It can be either averse, normal, Tolerant.
        /// </summary>
        override public Conflictability ConflictLevel
        {
            get
            {
                return ConflictSetting;
            }
            set
            {
                ConflictSetting = value;
            }
        }

        /// <summary>
        /// Holds the evaluated efficiency of the current subevent. Its based on a scale of 1
        /// </summary>
        override public double Score
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

        override public Procrastination ProcrastinationProfile
        {
            get
            {
                return ProfileOfProcrastination;
            }
            set
            {
                ProfileOfProcrastination = value;
            }
        }

        override public ConflictProfile conflict
        {
            get
            {
                return ConflictingEvents;
            }
            set
            {
                ConflictingEvents = value;
            }
        }



        override public DateTimeOffset Start
        {
            get
            {
                return StartDateTime;
            }

            set
            {
                StartDateTime = value;
                if (BusyFrame == null)
                {
                    BusyFrame = new BusyTimeLine(Id, value, value.AddSeconds(1));
                }
                else
                {
                    BusyFrame = new BusyTimeLine(Id, value, BusyFrame.End);
                }
            }
        }

        override public DateTimeOffset End
        {
            get
            {
                return EndDateTime;
            }

            set
            {
                EndDateTime = value;
                if (BusyFrame == null)
                {
                    BusyFrame = new BusyTimeLine(Id, new DateTimeOffset() , value);
                }
                else
                {
                    BusyFrame = new BusyTimeLine(Id, BusyFrame.Start, value);
                }
            }
        }

        override public bool isRigid
        {
            get
            {
                return RigidSchedule;
            }

            set
            {
                RigidSchedule = value;
            }
        }

        override public bool isDeleted
        {
            get
            {
                return Enabled;
            }

            set
            {
                Enabled = value;
            }
        }



        override public Classification Classification
        {
            get
            {
                return Semantics;
            }

            set
            {
                Semantics = value;
            }
        }


        override public MiscData Notes
        {
            get
            {
                return _DataBlob;
            }

            set
            {
                _DataBlob = value;
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

        override public EventDisplay UIData
        {
            get
            {
                return UiParams;
            }

            set
            {
                UiParams = value;
            }
        }


        override public EventName Name
        {
            get
            {
                return NameOfEvent;
            }

            set
            {
                NameOfEvent = value;
            }
        }

        override public bool isRestricted
        {
            get
            {
                return false;
            }
        }

        public override bool  isDeviated {
            get {
                return DeviationFlag;
            }
            set {
                DeviationFlag = value;
            }
        }

        #endregion
    }
}