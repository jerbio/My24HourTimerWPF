﻿using System;
using System.Collections.Generic;
using System.Text;
using TilerElements;

namespace ScheduleAnalysis
{
    public class Suggestion
    {
        public Dictionary<CalendarEvent, TimeLine> DeadlineUpdates { get; set; } = new Dictionary<CalendarEvent, TimeLine>();
        public Dictionary<CalendarEvent, int> CountDelta { get; set; } = new Dictionary<CalendarEvent, int>();

        public void addCalendarEventAndTimeline(CalendarEvent calEvent, TimeLine timeline)
        {
            if(DeadlineUpdates.ContainsKey(calEvent))
            {
                DeadlineUpdates[calEvent] = timeline;
            }
            else
            {
                DeadlineUpdates.Add(calEvent, timeline);
            }
        }
    }
}
