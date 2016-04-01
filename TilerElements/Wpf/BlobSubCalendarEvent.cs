using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements.Wpf
{
    public class BlobSubCalendarEvent:SubCalendarEvent
    {
        SubCalendarEvent[] EventClumps;

        public BlobSubCalendarEvent(IEnumerable<SubCalendarEvent> InterFerringEvents)
        {
            StartDateTime= InterFerringEvents.OrderBy(obj => obj.Start).First().Start;
            EndDateTime = InterFerringEvents.OrderByDescending(obj => obj.End).First().End;
            UniqueID = EventID.GenerateSubCalendarEvent(EventID.GenerateCalendarEvent().ToString(),0);
            BusyFrame = new BusyTimeLine(UniqueID.ToString(), StartDateTime, EndDateTime);
            CalendarEventRange = new TimeLine(StartDateTime,EndDateTime);
            CalendarEvent nullEvent = CalendarEvent.getEmptyCalendarEvent(UniqueID, StartDateTime, EndDateTime);
            RigidSchedule = true;
            double halfDouble=Double.MaxValue/2;
            LocationInfo = Location_Elements.AverageGPSLocation(InterFerringEvents.Where(Obj => Obj.Location.XCoordinate < halfDouble).Select(obj => obj.Location));
            EventScore = 0;
            EventClumps = InterFerringEvents.ToArray();
            EventDuration = TimeSpan.FromTicks( InterFerringEvents.Sum(obj => obj.ActiveDuration.Ticks));
            ConflictingEvents = new ConflictProfile();
            MuddledEvent = true;
        }

        public IEnumerable<SubCalendarEvent> getSubCalendarEventsInBlob()
        {
            return EventClumps;
        }

        #region Properties
        public string ID
        {
            get
            {
                return UniqueID.ToString();
            }
        }

        public ConflictProfile Conflicts
        {
            get
            {
                return ConflictingEvents;
            }
        }
        #endregion

    }
}
