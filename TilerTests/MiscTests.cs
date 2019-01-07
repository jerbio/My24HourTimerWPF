using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerElements;
using System.Collections.Generic;
using TilerCore;
using TilerFront;

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
            Assert.AreEqual(RestrictionEnd.DayOfYear , interFerring.End.DayOfYear);
            interFerring = myRestrictionProfile.getLatestEndTimeWithinFrameBeforeRefTime(refTime);
            Assert.AreEqual(RestrictionEnd.DayOfYear, interFerring.End.DayOfYear);
        }
    }
}
