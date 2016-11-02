using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using TilerElements;
using My24HourTimerWPF;

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
            TimeSpan jitterSpan = TimeSpan.FromMinutes(10);
            TimeSpan durationOfEvents = TimeSpan.FromMinutes(60);
            DateTimeOffset Start = DateTimeOffset.UtcNow;
            DateTimeOffset newStart = Start;
            UserAccount user = TestUtility.getTestUser();
            user.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Now;
            Schedule mySchedule = new Schedule(user, refNow);
            int count = 5;
            List<CalendarEvent> allEvents = new List<CalendarEvent>();
            for(int i=0; i<count; i++)
            {
                CalendarEvent CalendarEvent = TestUtility.generateCalendarEvent(durationOfEvents, new TilerElements.Repetition(), newStart, newStart.Add(durationOfEvents), 1, true );
                allEvents.Add(CalendarEvent);
                newStart = newStart.Add(jitterSpan);
                mySchedule.AddToSchedule(CalendarEvent);
            }

            List<BlobSubCalendarEvent> conflictingEvents = Utility.getConflictingEvents(allEvents.SelectMany(obj => obj.ActiveSubEvents));
            int conflictingEventCount = conflictingEvents.Sum(blob => blob.getSubCalendarEventsInBlob().Count());
            Assert.AreEqual(conflictingEventCount, count);
        }
        
        [ClassCleanup]
        public static void cleanUpTest()
        {
            UserAccount currentUser = TestUtility.getTestUser();
            currentUser.Login().Wait();
            DateTimeOffset refNow = DateTimeOffset.Now;
            Schedule Schedule = new Schedule(currentUser, refNow);
            currentUser.DeleteAllCalendarEvents();
        }
    }
}
