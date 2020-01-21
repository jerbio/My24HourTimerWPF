using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace TilerElements
{
    public class BlobSubCalendarEvent:SubCalendarEvent
    {
        protected BlobSubCalendarEvent():base()
        {
            BlobEvent = true;
            _RigidSchedule = true;
        }
        HashSet<SubCalendarEvent> EventClumps;

        public BlobSubCalendarEvent(IEnumerable<SubCalendarEvent> InterFerringEvents)
        {
            updateStartTime( InterFerringEvents.OrderBy(obj => obj.Start).First().Start);
            updateEndTime( InterFerringEvents.OrderByDescending(obj => obj.End).First().End);
            UniqueID=EventID.GenerateSubCalendarEvent(EventID.GenerateCalendarEvent().ToString());
            BusyFrame = new BusyTimeLine(UniqueID.ToString(), Start, End);
            _CalendarEventRange = new TimeLine(Start,End);
            CalendarEvent nullEvent = CalendarEvent.getEmptyCalendarEvent(UniqueID, Start, End);
            _RigidSchedule = true;
            double halfDouble=Double.MaxValue/2;
            _LocationInfo = Location.AverageGPSLocation(InterFerringEvents.Where(Obj => Obj.Location!=null && Obj.Location.Latitude < halfDouble).Select(obj => obj.Location));
            EventScore = 0;
            EventClumps = new HashSet<SubCalendarEvent>(InterFerringEvents);
            _EventDuration = TimeSpan.FromTicks( InterFerringEvents.Sum(obj => obj.getActiveDuration.Ticks));
            
            _ConflictingEvents = new ConflictProfile();
            BlobEvent = true;
            _Name = new EventName(null, this);
        }

        public HashSet<SubCalendarEvent> getSubCalendarEventsInBlob()
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

        public override JObject Json
        {
            get
            {
                JObject retvalue = base.Json;
                JArray subEventsInBlob = new JArray(this.getSubCalendarEventsInBlob().Select(sub => sub.Json));

                retvalue.Add("subEventsInBlob", subEventsInBlob);
                return retvalue;
            }
        }
        #endregion

    }
}
