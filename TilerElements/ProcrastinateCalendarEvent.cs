﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class ProcrastinateCalendarEvent : RigidCalendarEvent
    {
        protected ProcrastinateCalendarEvent(EventID eventId, 
            EventName NameEntry, DateTimeOffset StartData, DateTimeOffset EndData, TimeSpan EventDuration, TimeSpan eventPrepTime, TimeSpan PreDeadlineTimeSpan, Repetition EventRepetitionEntry, Location EventLocation, EventDisplay UiData, MiscData NoteData, bool EnabledEventFlag, bool CompletionFlag, TilerUser creator, TilerUserGroup users, string timeZone, int splitCount) : base(
                //eventId, 
                NameEntry, StartData, EndData, EventDuration, eventPrepTime, PreDeadlineTimeSpan, EventRepetitionEntry, EventLocation, UiData, NoteData, EnabledEventFlag, CompletionFlag, creator, users, timeZone, eventId, false)
        {
            isProcrastinateEvent = true;
            UniqueID = eventId;
            initializeSubEvents();
            _Splits = splitCount;
        }

        protected ProcrastinateCalendarEvent() : base()
        {
            isProcrastinateEvent = true;
            _Splits = 1;
            initializeSubEvents();
        }

        protected ProcrastinateCalendarEvent(CalendarEvent MyUpdated, SubCalendarEvent[] MySubEvents) : base(MyUpdated, MySubEvents)
        {

        }

        public override void initializeSubEvents()
        {
            SubEvents = new SubEventDictionary();
            for (int i = 0; i < _Splits; i++)
            {
                TimeLine procrastinationTimeLine = new TimeLine(StartDateTime, EndDateTime);
                SubCalendarEvent newSubCalEvent = new ProcrastinateAllSubCalendarEvent(getCreator, _Users, _TimeZone, procrastinationTimeLine, this.UniqueID, this._LocationInfo, this);
                newSubCalEvent.TimeCreated = this.TimeCreated;

                SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
            }
        }


        /// <summary>
        /// function updates the timeline of the Procrastinate calendarEvent.
        /// If the subEvent conflicts with any sub event then this sub event is to be stretched to fill the spans of the other subevent. The other sub events should also be deleted.
        /// If it doesnt then it should expand the time line of the Procrastinate calendarEvent appropriately.
        /// In the case of a procrastinate calendar event. the new timeline is unnecessary because it will be evaluated
        /// </summary>
        /// <param name="subEvent">The subvent thats triggerring the change in the calendar event</param>
        /// <param name="newTImeLine">new timeline</param>
        public override void updateTimeLine(SubCalendarEvent subEvent, TimeLine newTImeLine)
        {
            updateTimeLine(subEvent);
        }

        protected virtual void updateTimeLine(SubCalendarEvent subEvent)
        {
            HashSet<SubCalendarEvent> allSubevets = new HashSet<SubCalendarEvent>(this.AllSubEvents);
            allSubevets.Add(subEvent);
            BlobSubCalendarEvent blobOfAllSubEvents = new BlobSubCalendarEvent(allSubevets);

            TimeLine oldTimeLine = new TimeLine(blobOfAllSubEvents.Start, blobOfAllSubEvents.End);
            DateTimeOffset start = subEvent.Start < oldTimeLine.Start ? subEvent.Start : oldTimeLine.Start;
            DateTimeOffset end = subEvent.End > oldTimeLine.End ? subEvent.End : oldTimeLine.End;
            List<SubCalendarEvent> interferringWithSubEvents = ActiveSubEvents.Where(procrastinateEvent => procrastinateEvent.getId != subEvent.getId).Where(procrastinateEvent => procrastinateEvent.RangeTimeLine.doesTimeLineInterfere(subEvent.ActiveSlot)).ToList();
            if (interferringWithSubEvents.Count > 0)
            {
                BlobSubCalendarEvent blobSubEvent = new BlobSubCalendarEvent(interferringWithSubEvents);
                foreach (SubCalendarEvent interferringWithSubEvent in blobSubEvent.getSubCalendarEventsInBlob())
                {
                    interferringWithSubEvent.disable(this);
                }
                DateTimeOffset subEventStart = subEvent.Start < blobSubEvent.Start ? subEvent.Start : blobSubEvent.Start;
                DateTimeOffset subEventEnd = subEvent.End > blobSubEvent.End ? subEvent.End : blobSubEvent.End;
                subEvent.shiftEvent(subEventStart, true);
                TimeSpan delta = subEventEnd - subEvent.End;
                subEvent.addDurartion(delta);
                start = start<subEvent.Start? start : subEvent.Start;
                end = end > subEvent.End? end: subEvent.End;
            }
            TimeLine newTImeLine = new TimeLine(start, end);
            this.StartDateTime = newTImeLine.Start;
            this.EndDateTime = newTImeLine.End;
            AllSubEvents.AsParallel().ForAll(obj => obj.changeTimeLineRange(newTImeLine));
            updateEventSequence();
            this.UpdateTimePerSplit();
        }

public static CalendarEvent generateProcrastinateAll(DateTimeOffset referenceNow, TilerUser user, TimeSpan DelaySpan, string timeZone, ProcrastinateCalendarEvent procrastinateEvent = null, string NameOfEvent = "BLOCKED OUT")
        {
            EventName blockName = new EventName(user, null, NameOfEvent);
            EventID clearAllEventsId = new EventID(user.getClearAllEventsId());
            EventID suEventId = EventID.GenerateSubCalendarEvent(clearAllEventsId);
            DateTimeOffset eventStartTime = referenceNow;
            DateTimeOffset eventEndTime = eventStartTime.Add(DelaySpan);
            ProcrastinateCalendarEvent procrastinateAll;
            if (procrastinateEvent == null)
            {
                procrastinateAll = new ProcrastinateCalendarEvent(
                clearAllEventsId, 
                blockName, eventStartTime, eventEndTime, DelaySpan, new TimeSpan(0), new TimeSpan(0), new Repetition(), new Location(), new EventDisplay(), new MiscData(), true, false, user, new TilerUserGroup(), timeZone, 1);
            }
            else
            {
                procrastinateAll = procrastinateEvent;
                procrastinateAll.EndDateTime = referenceNow.Add(DelaySpan);
                TimeLine procrastinationTimeLine = new TimeLine(referenceNow, procrastinateAll.EndDateTime);
                ProcrastinateAllSubCalendarEvent subEvent = new ProcrastinateAllSubCalendarEvent(user, new TilerUserGroup(), user.TimeZone, procrastinationTimeLine, new EventID(suEventId.getCalendarEventID()), procrastinateAll._LocationInfo, procrastinateAll);
                //Combines multiple subcalendarevents that interfere into one single subcalendarEvent
                List<SubCalendarEvent> interferringSubEvents = procrastinateAll.ActiveSubEvents.Where(possibleInterferringSubEvent => possibleInterferringSubEvent.End >= referenceNow).OrderBy(possibleInterferringSubEvent => possibleInterferringSubEvent.End).ToList();
                if (interferringSubEvents.Count > 0)
                {
                    SubCalendarEvent interferringSubEvent = interferringSubEvents.OrderByDescending(obj=>obj.End).First();
                    interferringSubEvent.shiftEvent(subEvent.Start,true);
                    TimeSpan delta = subEvent.End - interferringSubEvent.End;
                    interferringSubEvent.addDurartion(delta);
                    procrastinateAll.updateTimeLine(interferringSubEvent);
                }
                else
                {
                    procrastinateAll.IncreaseSplitCount(1, new List<SubCalendarEvent>() { subEvent });
                }
            }
            blockName.Creator_EventDB = procrastinateAll.getCreator;
            blockName.AssociatedEvent = procrastinateAll;
            procrastinateAll.UpdateTimePerSplit();
            return procrastinateAll;
        }
    }
}
