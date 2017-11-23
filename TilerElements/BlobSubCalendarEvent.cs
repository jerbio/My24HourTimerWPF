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
            CalendarEvent nullEvent = CalendarEvent.getEmptyCalendarEvent(UniqueID, StartDateTime, EndDateTime);
            RigidSchedule = true;
            double halfDouble=Double.MaxValue/2;
            _LocationInfo = Location.AverageGPSLocation(InterFerringEvents.Where(Obj => Obj.Location.Latitude < halfDouble).Select(obj => obj.Location));
            EventScore = 0;
            EventClumps = InterFerringEvents.ToArray();
            _EventDuration = TimeSpan.FromTicks( InterFerringEvents.Sum(obj => obj.getActiveDuration.Ticks));
            ConflictingEvents = new ConflictProfile();
            BlobEvent = true;
            _Name = new EventName(null, this);
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
