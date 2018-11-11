using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public abstract class TilerEvent: IWhy
    {
        public static TimeSpan ZeroTimeSpan = new TimeSpan(0);
        protected EventName _Name;
        protected DateTimeOffset StartDateTime;
        protected DateTimeOffset EndDateTime;
        protected bool Complete = false;
        protected bool Enabled = true;
        protected bool DeadlineElapsed = false;
        protected bool UserDeleted = false;
        protected Location LocationInfo;
        protected EventDisplay UiParams = new EventDisplay();
        protected MiscData DataBlob = new MiscData();
        protected Repetition EventRepetition;
        //protected bool RepetitionFlag;
        protected bool RigidSchedule;
        protected TimeSpan EventDuration;
        protected string otherPartyID;
        protected TimeSpan EventPreDeadline;
        protected TimeSpan PrepTime;
        protected EventID UniqueID;
        protected int Priority;
        protected bool isRestricted = false;
        protected static DateTimeOffset EventNow = DateTimeOffset.UtcNow;
        protected static TimeSpan CalculationEndSpan = new TimeSpan(180, 0, 0, 0, 0);
        protected Procrastination ProfileOfProcrastination = new Procrastination(new DateTimeOffset(), new TimeSpan());
        protected NowProfile ProfileOfNow = new NowProfile();
        protected bool ThirdPartyFlag = false;
        protected string ThirdPartyUserIDInfo;
        protected ThirdPartyControl.CalendarTool ThirdPartyTypeInfo = ThirdPartyControl.CalendarTool.tiler;
        protected TilerUser _Creator;
        protected TimeSpan _UsedTime = new TimeSpan();
        protected Classification Semantics= new Classification();
        protected TimeOfDayPreferrence DaySectionPreference;
        protected TilerUserGroup _Users;
        protected string _TimeZone = "UTC";
        protected bool isProcrastinateEvent = false;
        public DateTimeOffset TimeCreated { get; set; } = new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan());
        internal TempTilerEventChanges TempChanges = new TempTilerEventChanges();
        protected bool _userLocked { get; set; } = false;

        #region IwhyImplementation
        abstract public IWhy Because();

        abstract public IWhy OtherWise();

        abstract public IWhy WhatIf(params Reason[] reasons);
        #endregion

        async public Task InitializeClassification()
        {
            //await Semantics.InitializeClassification(_Name.NameValue);
        }
        public TilerUserGroup getAllUsers()
        {
            return _Users;
        }

        /// <summary>
        /// This updates the name of an event
        /// </summary>
        /// <param name="NewName">The new name of the calendar event</param>
        virtual public void updateEventName(string NewName)
        {
            _Name.updateName(NewName);
        }

        /// <summary>
        /// This updates the notes of the event
        /// </summary>
        /// <param name="NewName">The new name of the calendar event</param>
        virtual public void updateMiscData(string Notes)
        {
            this.DataBlob.UserNote = Notes;
        }

        public bool getIsComplete
        {
            get
            {
                return Complete;
            }
        }

        //virtual protected void setEventId(EventID id)
        //{
        //    this.UniqueID = id;
        //}

        public EventDisplay getUIParam
        {
            get
            {
                return UiParams;
            }
        }

        public bool isActive
        {
            get
            {
                return ((!getIsComplete) && (isEnabled));
            }
        }

        virtual public bool isEnabled
        {
            get
            {
                return Enabled;
            }
        }

        virtual public bool getIsDeleted
        {
            get
            {
                return !isEnabled;
            }
        }

        virtual public  DateTimeOffset End
        {
            get
            {
                return EndDateTime;
            }
        }

        virtual public  DateTimeOffset Start
        {
            get
            {
                return StartDateTime;
            }
        }

        public ThirdPartyControl.CalendarTool ThirdpartyType
        {
            get 
            {
                return ThirdPartyTypeInfo;
            }
        }


        public bool getIsProcrastinateCalendarEvent
        {
            get
            {
                return isProcrastinateEvent;
            }
        }
        public bool isThirdParty
        {
            get
            {
                return ThirdpartyType != ThirdPartyControl.CalendarTool.tiler;
            }
        }

        public  string ThirdPartyID
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

        virtual public string getThirdPartyUserID
        {
            get
            {
                return ThirdPartyUserIDInfo;
            }
        }

        virtual public Location Location
        {
            set
            {
                LocationInfo = value;
            }
            get
            {
                return LocationInfo;
            }
        }

        public virtual bool getIsRepeat
        {
            get
            {
                return EventRepetition.Enable;
            }
        }

        virtual public TimeSpan getPreparation
         {
             get
             {
                 return PrepTime;
             }
         }
         public TimeSpan getPreDeadline
         {
             get
             {
                 return EventPreDeadline;
             }
         }

         public bool getIsEventRestricted
         {
             get
             {
                 return isRestricted;
             }
         }
        public bool getIsDeadlineElapsed
         {
             get 
             {
                 return DeadlineElapsed;
             }
         }
        public EventName getName
        {
            get
            {
                return _Name;
            }
        }

        public bool getIsUserDeleted
        {
            get
            {
                return UserDeleted;
            }

        }

        public int getEventPriority
        {
            get
            {
                return Priority;
            }
        }

        public Procrastination  getProcrastinationInfo
        {
            get
            {
                return ProfileOfProcrastination;
            }
        }

        public virtual NowProfile getNowInfo
        {
            get
            {
                return ProfileOfNow;
            }
        }

        public void InitializeDayPreference(TimeLine timeLine)
        {
            if (DaySectionPreference == null)
            {
                DaySectionPreference = new TimeOfDayPreferrence(timeLine);
            }
            DaySectionPreference.InitializeGrouping(this);// InitializeGrouping
        }

        public TimeOfDayPreferrence getDaySection()
        {
            return DaySectionPreference;
        }

        public List<OptimizedGrouping> evaluateDayPreference(IList<OptimizedGrouping> groupings)
        {
            Dictionary<TimelineWithSubcalendarEvents, OptimizedGrouping> TimelinesDict = groupings.ToDictionary(grouping => grouping.GroupAverage.TimeLine, grouping => grouping);
            Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping> TimeOfDayToGroup = groupings.ToDictionary(grouping => grouping.DaySector, grouping => grouping);
            Dictionary<TimelineWithSubcalendarEvents, OptimizedGrouping> timelineToGroups = groupings.ToDictionary(grouping => grouping.GroupAverage.TimeLine, grouping => grouping);
            List<TimelineWithSubcalendarEvents> Timelines = orderBasedOnProductivity(TimeOfDayToGroup);
            List<Tuple<Location, Location>> borderLocations = Timelines.Select(timeLine => 
                { var optimizedGrouping = timelineToGroups[timeLine];
                    return new Tuple<Location, Location>(optimizedGrouping.LeftBorder, optimizedGrouping.RightBorder);
                }).ToList();
            List<double> foundIndexes = EvaluateTimeLines(Timelines, borderLocations);
            List<Tuple<double, OptimizedGrouping>> indexToGrouping = foundIndexes.Select((score, index) => { return new Tuple<double, OptimizedGrouping>(score, TimelinesDict[Timelines[index]]);}).OrderBy(tuple => tuple.Item1).ToList();
            int bestIndex = foundIndexes.MinIndex();
            List<OptimizedGrouping> retValue = indexToGrouping.Select(tuple => tuple.Item2).ToList();
            return retValue;
        }

        public virtual List<double> EvaluateTimeLines (List<TimelineWithSubcalendarEvents> timeLines, List<Tuple<Location, Location>> borderLocations = null)
        {
            double worstDistanceInKM = 7;
            List<IList<double>> multiDimensionalClaculation = new List<IList<double>>();
            for (int i =0; i< timeLines.Count; i++)
            {
                TimelineWithSubcalendarEvents timeline = timeLines[i];
                double distance = Location.calculateDistance(timeline.averageLocation, this.Location, worstDistanceInKM);
                double tickRatio = (double)this.getActiveDuration.Ticks / timeline.TotalFreeSpotAvailable.Ticks;
                double occupancy = (double)timeline.Occupancy;
                IList<double> dimensionsPerDay = new List<double>() { distance, tickRatio, occupancy };
                if(borderLocations != null && borderLocations.Count == timeLines.Count)
                {
                    Tuple<Location, Location> borderLocation = borderLocations[i];
                    double borderLocationsDistance = Location.sumDistance(worstDistanceInKM, borderLocation.Item1, this.Location, borderLocation.Item2);
                    dimensionsPerDay.Add(borderLocationsDistance);
                }
                multiDimensionalClaculation.Add(dimensionsPerDay);
            }
            List<double> foundIndexes = Utility.multiDimensionCalculationNormalize(multiDimensionalClaculation);
            return foundIndexes;
        }

        /// <summary>
        /// Function tries to order the timelines in a manner that is most likely desired by a user for a user.
        /// </summary>
        /// <param name="timeLines"></param>
        /// <returns></returns>
        protected List<TimelineWithSubcalendarEvents> orderBasedOnProductivity(Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping> AllGroupings)
        {
            //TODO need to use machine learning to order the timelines right now the implemenation simple favors a morning schedule
            List<TimeOfDayPreferrence.DaySection> daySectionsPreferredOrder = (new List<TimeOfDayPreferrence.DaySection>() { TimeOfDayPreferrence.DaySection.Morning, TimeOfDayPreferrence.DaySection.Afternoon, TimeOfDayPreferrence.DaySection.Evening, TimeOfDayPreferrence.DaySection.Sleep }).Where(section  => AllGroupings.ContainsKey( section)).ToList();
            List<TimelineWithSubcalendarEvents> retValue = daySectionsPreferredOrder.Select(timeOfDay => AllGroupings[timeOfDay].GroupAverage.TimeLine).ToList();
            return retValue;
        }

        public void updateDayPreference(List<OptimizedGrouping> groupings)
        {
            Dictionary<TimeOfDayPreferrence.DaySection, OptimizedGrouping> sectionTOGrouping = groupings.ToDictionary(group => group.DaySector, group => group);
            List<TimeOfDayPreferrence.DaySection> daySections = DaySectionPreference.getPreferenceOrder();
            List<OptimizedGrouping> validGroupings = new List<OptimizedGrouping>();
            foreach(TimeOfDayPreferrence.DaySection section in daySections)
            {
                if (sectionTOGrouping.ContainsKey(section))
                {
                    validGroupings.Add(sectionTOGrouping[section]);
                }
            }
            if(validGroupings.Count > 0)
            {
                List<OptimizedGrouping> updatedGroupingOrder = evaluateDayPreference(validGroupings);
                DaySectionPreference.setPreferenceOrder(updatedGroupingOrder.Select(group => group.DaySector).ToList());
            }
        }

        public string getCreatorId
        {
            get
            {
                return _Creator.Id;
            }
        }

        public TilerUser getCreator
        {
            get
            {
                return _Creator;
            }
        }
        virtual public TimeSpan UsedTime
        {
            set
            {
                throw new NotImplementedException("You are trying to set the used up time in a tiler events. Invalid action.");
            }

            get
            {
                return _UsedTime;
            }
        }

        virtual public string getId
        {
            get
            {
                return UniqueID.ToString();
            }
        }

        virtual public DateTimeOffset getDeadline
        {
            get {
                return End;
            }
        }

        virtual public TimeSpan getActiveDuration
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        virtual public EventID getTilerID
        {
            get
            {
                return UniqueID;
            }
        }
        public override string ToString()
        {
            return this.Start.ToString() + " - " + this.End.ToString() + "::" + this.getId + "\t\t::" + this.getActiveDuration.ToString();
        }

        virtual public bool isLocked
        {
            get
            {
                return isRigid || _userLocked;
            }
        }

        virtual public bool isRigid
        {
            get
            {
                return RigidSchedule;
            }
        }

        virtual public bool userLocked
        {
            get
            {
                return _userLocked;
            }
            set
            {
                _userLocked = value;
            }
        }

        virtual public TimeLine RangeTimeLine
        {
            get
            {
                TimeLine retValue = new TimeLine(this.Start, this.End);
                return retValue;
            }
        }

        virtual public string getTimeZone
        {
            get
            {
                return _TimeZone;
            }
        }
    }
}
