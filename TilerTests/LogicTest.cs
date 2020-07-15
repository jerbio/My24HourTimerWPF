﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using TilerElements;
using TilerCore;

namespace TilerTests
{
    /// <summary>
    /// Test tries to verify the check for conflict algorithm works correctly in blob subcalendar event.
    /// </summary>
    [TestClass]
    public class LogicTest
    {
        [TestMethod]
        public void checkForConflict()
        {
            var packet = TestUtility.CreatePacket();
            UserAccount user = packet.Account;
            TilerUser tilerUser = packet.User;

            TimeSpan jitterSpan = TimeSpan.FromMinutes(10);
            TimeSpan durationOfEvents = TimeSpan.FromMinutes(60);
            DateTimeOffset Start = DateTimeOffset.UtcNow;
            DateTimeOffset newStart = Start;
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            Schedule mySchedule = new TestSchedule(user, refNow);
            int count = 5;
            List<CalendarEvent> allEvents = new List<CalendarEvent>();
            for (int i = 0; i < count; i++)
            {
                CalendarEvent CalendarEvent = TestUtility.generateCalendarEvent(tilerUser, durationOfEvents, new TilerElements.Repetition(), newStart, newStart.Add(durationOfEvents), 1, true);
                allEvents.Add(CalendarEvent);
                newStart = newStart.Add(jitterSpan);
                mySchedule.AddToSchedule(CalendarEvent);
            }

            List<BlobSubCalendarEvent> conflictingEvents = Utility.getConflictingEvents(allEvents.SelectMany(obj => obj.ActiveSubEvents)).Item1;
            int conflictingEventCount = conflictingEvents.Sum(blob => blob.getSubCalendarEventsInBlob().Count());
            Assert.AreEqual(conflictingEventCount, count);
        }

    }
}
