﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class BlobSubCalendarEvent:SubCalendarEvent
    {
        SubCalendarEvent[] EventClumps;

        public BlobSubCalendarEvent(IEnumerable<SubCalendarEvent> InterFerringEvents)
        {
            StartDateTime= InterFerringEvents.OrderBy(obj => obj.Start).First().Start;
            EndDateTime = InterFerringEvents.OrderByDescending(obj => obj.End).First().End;
            UniqueID=EventID.GenerateSubCalendarEvent(EventID.GenerateCalendarEvent().ToString());
            BusyFrame = new BusyTimeLine(UniqueID.ToString(), StartDateTime, EndDateTime);
            CalendarEventRange = new TimeLine(StartDateTime,EndDateTime);
            RigidSchedule = true;
            EventLocation = Location_Elements.AverageGPSLocation(InterFerringEvents.Select(obj => obj.myLocation));
            EventScore = 0;
            EventClumps = InterFerringEvents.ToArray();
            EventDuration = TimeSpan.FromTicks( InterFerringEvents.Sum(obj => obj.ActiveDuration.Ticks));
            ConflictingEvents = new ConflictProfile();
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
