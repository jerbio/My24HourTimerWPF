using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerElements;
using System.Collections.Generic;
using TilerCore;
using TilerFront;
using System.Threading.Tasks;

namespace TilerTests
{
    [TestClass]
    public class MiscTests
    {
        [TestMethod]
        public void MiscMethod()
        {
            string restrictionStartString = "9:00 AM 1/1/1970 +00:00";
            string restrictionEndString = "9:00 PM 1/1/1970 +00:00";
            DateTimeOffset RestrictionStart = TestUtility.parseAsUTC(restrictionStartString);
            DateTimeOffset RestrictionEnd = TestUtility.parseAsUTC(restrictionEndString);
            string refTimeLineStart = "7:00 AM 03/02/2016 +00:00";
            string refTimeLineEnd = "10:00 AM 03/02/2016 +00:00";
            DateTimeOffset RefTimeLineStart = TestUtility.parseAsUTC(refTimeLineStart);
            DateTimeOffset RefTimeLineEnd = TestUtility.parseAsUTC(refTimeLineEnd);
            RestrictionProfile myRestrictionProfile = new RestrictionProfile(7, DayOfWeek.Monday, RestrictionStart, RestrictionEnd);
            var interFerring = myRestrictionProfile.getAllNonPartialTimeFrames(new TimeLine(RefTimeLineStart, RefTimeLineEnd));

        }

        [TestMethod]
        public void MiscMethod0()
        {
            string restrictionStartString = "4:00 PM 03/01/2014 +00:00"; //"9:00 AM 1/1/1970 +00:00";
            string restrictionEndString = "10:00 AM 03/02/2014 +00:00"; //"9:00 PM 1/1/1970 +00:00";
            DateTimeOffset RestrictionStart = TestUtility.parseAsUTC(restrictionStartString);
            DateTimeOffset RestrictionEnd = TestUtility.parseAsUTC(restrictionEndString);
            RestrictionTimeLine restrictedTimeLine = new RestrictionTimeLine(RestrictionStart, RestrictionEnd);// TimeSpan.FromHours(10));
            RestrictionProfile myRestrictionProfile = new RestrictionProfile(new List<DayOfWeek>() { DayOfWeek.Saturday}, new List<RestrictionTimeLine>() { restrictedTimeLine} );
            DateTimeOffset refTime = new DateTimeOffset(2014, 3, 2, 2, 0, 0, new TimeSpan());
            var interFerring = myRestrictionProfile.getEarliestStartTimeWithinAFrameAfterRefTime(refTime);
            Assert.AreEqual(RestrictionEnd.DayOfYear , interFerring.Item1.End.DayOfYear);
            interFerring = myRestrictionProfile.getLatestEndTimeWithinFrameBeforeRefTime(refTime);
            Assert.AreEqual(RestrictionEnd.DayOfYear, interFerring.Item1.End.DayOfYear);
        }

        /// <summary>
        /// Test tries catch a case where an event has restriction profile which has saturday a restriction day with a timeline that extends into a different day, which is Sunday.
        /// However, since it extends into a different day it means extends into a different week, since sunday is the beginning of a new week.
        /// This test tries to capture that.
        /// </summary>
        [TestMethod]
        public void RestrictedEventOnWeekend ()
        {
            DateTimeOffset refNow = TestUtility.parseAsUTC("3/30/2019 10:59:56 PM +00:00");
            List<DayOfWeek> daysOfTheWeek = new List<DayOfWeek>() { DayOfWeek.Sunday,
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday};
            List<RestrictionTimeLine> constrictionProfiles = new List<RestrictionTimeLine>()
            {
                new RestrictionTimeLine(TestUtility.parseAsUTC("1:00 PM +00:00"), TestUtility.parseAsUTC("2:00 AM +00:00")),
                new RestrictionTimeLine(TestUtility.parseAsUTC("12:00 PM +00:00"), TestUtility.parseAsUTC("2:30 AM +00:00")),
                new RestrictionTimeLine(TestUtility.parseAsUTC("12:00 PM +00:00"), TestUtility.parseAsUTC("2:30 AM +00:00")),
                new RestrictionTimeLine(TestUtility.parseAsUTC("12:00 PM +00:00"), TestUtility.parseAsUTC("2:30 AM +00:00")),
                new RestrictionTimeLine(TestUtility.parseAsUTC("12:00 PM +00:00"), TestUtility.parseAsUTC("2:30 AM +00:00")),
                new RestrictionTimeLine(TestUtility.parseAsUTC("12:00 PM +00:00"), TestUtility.parseAsUTC("2:30 AM +00:00")),
                new RestrictionTimeLine(TestUtility.parseAsUTC("12:00 PM +00:00"), TestUtility.parseAsUTC("2:30 AM +00:00")),
            };
            RestrictionProfile restrictionProfile = new RestrictionProfile(daysOfTheWeek, constrictionProfiles);
            TilerUser tilerUser = TestUtility.createUser();
            UserAccount user = TestUtility.getTestUser(userId: tilerUser.Id);
            tilerUser = user.getTilerUser();
            user.Login().Wait();
            TestSchedule Schedule = new TestSchedule(user, refNow);
            
            DateTimeOffset TimeCreation = DateTimeOffset.UtcNow;
            tilerUser = user.getTilerUser();
            DateTimeOffset start = refNow;
            DateTimeOffset end = refNow.AddDays(4);
            CalendarEvent testEventA = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(20), new Repetition(), start, end, 5, false, restrictionProfile: restrictionProfile, now: Schedule.Now);
            testEventA.TimeCreated = TimeCreation;
            Schedule.AddToScheduleAndCommit(testEventA).Wait();

            CalendarEvent testEventB = TestUtility.generateCalendarEvent(tilerUser, TimeSpan.FromHours(20), new Repetition(), start, end, 10, false, restrictionProfile: restrictionProfile, now: Schedule.Now);
            testEventB.TimeCreated = TimeCreation;
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(testEventB).Wait();

            TimeSpan duration = TimeSpan.FromHours(20);
            CalendarEventRestricted testEventResticted = TestUtility.generateCalendarEvent(tilerUser, duration, new Repetition(), start, end.AddDays(-2), 10, false, restrictionProfile: restrictionProfile, now: Schedule.Now) as CalendarEventRestricted;
            Schedule = new TestSchedule(user, refNow);
            Schedule.AddToScheduleAndCommit(testEventResticted).Wait();
        }
    }
}
