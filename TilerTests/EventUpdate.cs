﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TilerFront;
using My24HourTimerWPF;
using System.Linq;
using TilerElements;
using System.Collections.Generic;
using TilerCore;

namespace TilerTests
{
    [TestClass]
    public class EventUpdate
    {
        //[ClassInitialize]
        //public static void cleanUpTest(TestContext context)
        //{
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    Schedule Schedule = new TestSchedule(currentUser, refNow);
        //    currentUser.DeleteAllCalendarEvents();
        //}

        //[TestInitialize]
        //public void cleanUpLog()
        //{
        //    UserAccount currentUser = TestUtility.getTestUser();
        //    currentUser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    Schedule Schedule = new TestSchedule(currentUser, refNow);
        //    currentUser.DeleteAllCalendarEvents();
        //}

        //[TestCleanup]
        //public void eachTestCleanUp()
        //{
        //    cleanUpLog();
        //}

        //// Test group test the change of the name of sub event
        //[TestMethod]
        //public void NameOfRigidSubEventUpdate()
        //{
        //    string newName = Guid.NewGuid().ToString();
        //    UserAccount currentuser = TestUtility.getTestUser();
        //    currentuser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
        //    TestSchedule schedule = new TestSchedule(currentuser, refNow, startOfDay);
        //    TimeSpan duration = TimeSpan.FromHours(1);
        //    DateTimeOffset start = refNow;
        //    DateTimeOffset end = refNow.Add(duration);
        //    CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, true);
        //    schedule.AddToScheduleAndCommit(testEvent).Wait();
        //    TestSchedule scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    CalendarEvent retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, new EventName(currentuser.getTilerUser(), retrievedCalendarEvent, newName), retrievedCalendarEvent.Start, retrievedCalendarEvent.End, testEvent.NumberOfSplit, retrievedCalendarEvent.Notes.UserNote);
        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    Assert.AreEqual(retrievedCalendarEvent.getName.NameValue, newName);
        //    Assert.IsTrue(retrievedCalendarEvent.isTestEquivalent(testEvent));
        //}
        //[TestMethod]
        //public void NameOfRepeatRigidSubEventUpdate()
        //{
        //    /// todo write update implementation for repeating events
        //}

        //[TestMethod]
        //public void NameOfNonRigidSubEventUpdate()
        //{
        //    string newName = Guid.NewGuid().ToString();
        //    UserAccount currentuser = TestUtility.getTestUser();
        //    currentuser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
        //    TestSchedule schedule = new TestSchedule(currentuser, refNow, startOfDay);
        //    TimeSpan duration = TimeSpan.FromHours(1);
        //    DateTimeOffset start = refNow;
        //    DateTimeOffset end = refNow.Add(duration.Add(duration));
        //    CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, false);
        //    schedule.AddToScheduleAndCommit(testEvent).Wait();
        //    TestSchedule scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, new EventName(currentuser.getTilerUser(), testEvent, newName), testEvent.Start, testEvent.End, testEvent.NumberOfSplit, testEvent.Notes.UserNote);
        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    CalendarEvent retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    Assert.AreEqual(retrievedCalendarEvent.getName.NameValue, newName);
        //    Assert.IsTrue(retrievedCalendarEvent.isTestEquivalent(testEvent));
        //}

        //[TestMethod]
        //public void NameOfRepeatNonRigidSubEventUpdate()
        //{
        //    /// todo write update implementation for repeating events
        //}

        //[TestMethod]
        //public void NameOfRestrictedSubEventUpdate()
        //{
        //    string newName = Guid.NewGuid().ToString();
        //    UserAccount currentuser = TestUtility.getTestUser();
        //    currentuser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
        //    TestSchedule schedule = new TestSchedule(currentuser, refNow, startOfDay);
        //    TimeSpan duration = TimeSpan.FromHours(1);
        //    DateTimeOffset start = refNow;
        //    DateTimeOffset end = refNow.Add(duration.Add(duration).Add(duration).Add(duration));
        //    RestrictionProfile restrictionProfile = new RestrictionProfile(start.Add(duration), duration.Add(duration));
        //    CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, false, null, restrictionProfile, now: schedule.Now);
        //    schedule.AddToScheduleAndCommit(testEvent).Wait();
        //    TestSchedule scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, new EventName(currentuser.getTilerUser(), testEvent, newName), testEvent.Start, testEvent.End, testEvent.NumberOfSplit, testEvent.Notes.UserNote);
        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    CalendarEvent retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    Assert.AreEqual(retrievedCalendarEvent.getName.NameValue, newName);
        //    Assert.IsTrue(retrievedCalendarEvent.isTestEquivalent(testEvent));
        //}

        //[TestMethod]
        //public void NameOfRepeatRestrictedSubEventUpdate()
        //{
        //    /// todo write update implementation for repeating events
        //}

        //// Test group test the change of the split count of events
        //[TestMethod]
        //public void SplitCountOfRepeatRigidSubEventUpdate()
        //{
        //    /// todo write update implementation for repeating events
        //}

        //[TestMethod]
        //public void SplitCountOfNonRigidSubEventUpdate()
        //{
        //    /// Increasing the split count
        //    UserAccount currentuser = TestUtility.getTestUser();
        //    currentuser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
        //    TestSchedule schedule = new TestSchedule(currentuser, refNow, startOfDay);
        //    TimeSpan duration = TimeSpan.FromHours(1);
        //    DateTimeOffset start = refNow;
        //    DateTimeOffset end = refNow.Add(TimeSpan.FromTicks(duration.Ticks * 5));
        //    CalendarEvent increaseSplitCountTestEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, false);
        //    schedule.AddToScheduleAndCommit(increaseSplitCountTestEvent).Wait();
        //    TestSchedule scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    int newSplitCount = increaseSplitCountTestEvent.NumberOfSplit + 1;
        //    var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(increaseSplitCountTestEvent.getId, increaseSplitCountTestEvent.getName, increaseSplitCountTestEvent.Start, increaseSplitCountTestEvent.End, newSplitCount, increaseSplitCountTestEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    CalendarEvent retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(increaseSplitCountTestEvent.Calendar_EventID);
        //    Assert.AreEqual(retrievedCalendarEvent.NumberOfSplit, newSplitCount);
        //    Assert.AreEqual(retrievedCalendarEvent.AllSubEvents.Count(), newSplitCount);
        //    Assert.IsTrue(retrievedCalendarEvent.isTestEquivalent(increaseSplitCountTestEvent));

        //    /// Reducing the split count
        //    CalendarEvent decreaseSplitCountTestEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 3, false);
        //    schedule.AddToScheduleAndCommit(decreaseSplitCountTestEvent).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    newSplitCount = decreaseSplitCountTestEvent.NumberOfSplit - 1;
        //    scheduleUpdated = scheduleReloaded.BundleChangeUpdate(decreaseSplitCountTestEvent.getId, decreaseSplitCountTestEvent.getName, decreaseSplitCountTestEvent.Start, decreaseSplitCountTestEvent.End, newSplitCount, decreaseSplitCountTestEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(decreaseSplitCountTestEvent.Calendar_EventID);
        //    Assert.AreEqual(retrievedCalendarEvent.NumberOfSplit, newSplitCount);
        //    Assert.AreEqual(retrievedCalendarEvent.AllSubEvents.Count(), newSplitCount);
        //    Assert.IsTrue(retrievedCalendarEvent.isTestEquivalent(decreaseSplitCountTestEvent));
        //}

        //[TestMethod]
        //public void SplitCountOfRepeatNonRigidSubEventUpdate()
        //{
        //    /// todo write update implementation for repeating events
        //}

        //[TestMethod]
        //public void SplitCountOfRestrictedSubEventUpdate()
        //{
        //    /// Increasing the split count
        //    UserAccount currentuser = TestUtility.getTestUser();
        //    currentuser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
        //    TestSchedule schedule = new TestSchedule(currentuser, refNow, startOfDay);
        //    TimeSpan duration = TimeSpan.FromHours(1);
        //    DateTimeOffset start = refNow;
        //    DateTimeOffset end = refNow.Add(TimeSpan.FromTicks(duration.Ticks * 5));
        //    RestrictionProfile restrictionProfile = new RestrictionProfile(start.Add(duration), duration.Add(duration));
        //    CalendarEvent increaseSplitCountTestEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, false, null, restrictionProfile, now: schedule.Now);
        //    schedule.AddToScheduleAndCommit(increaseSplitCountTestEvent).Wait();
        //    TestSchedule scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    int newSplitCount = increaseSplitCountTestEvent.NumberOfSplit + 1;
        //    var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(increaseSplitCountTestEvent.getId, increaseSplitCountTestEvent.getName, increaseSplitCountTestEvent.Start, increaseSplitCountTestEvent.End, newSplitCount, increaseSplitCountTestEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    CalendarEvent retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(increaseSplitCountTestEvent.Calendar_EventID);
        //    Assert.AreEqual(retrievedCalendarEvent.NumberOfSplit, newSplitCount);
        //    Assert.AreEqual(retrievedCalendarEvent.AllSubEvents.Count(), newSplitCount);
        //    Assert.IsTrue(retrievedCalendarEvent.isTestEquivalent(increaseSplitCountTestEvent));

        //    /// Reducing the split count
        //    CalendarEvent decreaseSplitCountTestEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 3, false, null, restrictionProfile, now: schedule.Now);
        //    schedule.AddToScheduleAndCommit(decreaseSplitCountTestEvent).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    newSplitCount = decreaseSplitCountTestEvent.NumberOfSplit - 1;
        //    scheduleUpdated = scheduleReloaded.BundleChangeUpdate(decreaseSplitCountTestEvent.getId, decreaseSplitCountTestEvent.getName, decreaseSplitCountTestEvent.Start, decreaseSplitCountTestEvent.End, newSplitCount, decreaseSplitCountTestEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(decreaseSplitCountTestEvent.Calendar_EventID);
        //    Assert.AreEqual(retrievedCalendarEvent.NumberOfSplit, newSplitCount);
        //    Assert.AreEqual(retrievedCalendarEvent.AllSubEvents.Count(), newSplitCount);
        //    Assert.IsTrue(retrievedCalendarEvent.isTestEquivalent(decreaseSplitCountTestEvent));
        //}

        //[TestMethod]
        //public void SplitCountOfRepeatRestrictedSubEventUpdate()
        //{
        //    /// todo write update implementation for repeating events
        //}

        //// Test group test the change of the deadline
        //[TestMethod]
        //public void DeadlineOfRigidSubEventUpdate()
        //{
        //    // increases the deadline
        //    UserAccount currentuser = TestUtility.getTestUser();
        //    currentuser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
        //    TestSchedule schedule = new TestSchedule(currentuser, refNow, startOfDay);
        //    TimeSpan duration = TimeSpan.FromHours(1);
        //    DateTimeOffset start = refNow;
        //    DateTimeOffset end = refNow.Add(duration);
        //    CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, true);
        //    schedule.AddToScheduleAndCommit(testEvent).Wait();
        //    TestSchedule scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    DateTimeOffset newDeadline = testEvent.End.Add(duration);
        //    var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.ActiveSubEvents.First().getId, testEvent.getName, testEvent.Start, newDeadline, testEvent.Start, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    CalendarEvent retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    TimeSpan OneMinuteDiff = TimeSpan.FromMinutes(1);
        //    Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs
        //    Assert.IsFalse(retrievedCalendarEvent.isTestEquivalent(testEvent));// this should evaluate to false because we have modified the deadline of the original calendar event

        //    // decreases the deadline
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    newDeadline = testEvent.End.Add(TimeSpan.FromTicks((long)duration.Ticks/2));
        //    scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.ActiveSubEvents.First().getId, testEvent.getName, testEvent.Start, newDeadline, testEvent.Start, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs when storing events
        //    Assert.IsFalse(retrievedCalendarEvent.isTestEquivalent(testEvent));// this should evaluate to false because we have modified the deadline of the original calendar event

        //}

        //public void DeadlineOfRepeatRigidSubEventUpdate()
        //{
        //    /// todo write update implementation for repeating events
        //}

        //[TestMethod]
        //public void DeadlineOfNonRigidSubEventUpdate()
        //{
        //    // increases the deadline
        //    UserAccount currentuser = TestUtility.getTestUser();
        //    currentuser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
        //    TestSchedule schedule = new TestSchedule(currentuser, refNow, startOfDay);
        //    TimeSpan duration = TimeSpan.FromHours(1);
        //    DateTimeOffset start = refNow;
        //    DateTimeOffset end = refNow.Add(duration);
        //    CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, false);
        //    schedule.AddToScheduleAndCommit(testEvent).Wait();
        //    TestSchedule scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    DateTimeOffset newDeadline = testEvent.End.Add(duration);
        //    var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, testEvent.Start, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    CalendarEvent retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    TimeSpan OneMinuteDiff = TimeSpan.FromMinutes(1);
        //    Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs
        //    Assert.IsFalse(retrievedCalendarEvent.isTestEquivalent(testEvent));// this should evaluate to false because we have modified the deadline of the original calendar event

        //    // decreases the deadline
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    newDeadline = testEvent.End.Add(TimeSpan.FromTicks((long)duration.Ticks / 2));
        //    scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, testEvent.Start, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs when storing events
        //    Assert.IsFalse(retrievedCalendarEvent.isTestEquivalent(testEvent));// this should evaluate to false because we have modified the deadline of the original calendar event
        //}

        //[TestMethod]
        //public void DeadlineOfRepeatNonRigidSubEventUpdate()
        //{
        //    /// todo write update implementation for repeating events
        //}

        //[TestMethod]
        //public void DeadlineOfRestrictedSubEventUpdate()
        //{
        //    // increases the deadline
        //    UserAccount currentuser = TestUtility.getTestUser();
        //    currentuser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
        //    TestSchedule schedule = new TestSchedule(currentuser, refNow, startOfDay);
        //    TimeSpan duration = TimeSpan.FromHours(1);
        //    DateTimeOffset start = refNow;
        //    DateTimeOffset end = start.Add(TimeSpan.FromTicks(duration.Ticks * 5));
        //    RestrictionProfile restrictionProfile = new RestrictionProfile(start.Add(duration), duration.Add(duration));
        //    CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, false, null, restrictionProfile, now: schedule.Now);
        //    schedule.AddToScheduleAndCommit(testEvent).Wait();
        //    TestSchedule scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    DateTimeOffset newDeadline = end.Add(duration);
        //    var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, testEvent.Start, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    CalendarEvent retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    TimeSpan OneMinuteDiff = TimeSpan.FromMinutes(1);
        //    Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs
        //    Assert.IsFalse(retrievedCalendarEvent.isTestEquivalent(testEvent));// this should evaluate to false because we have modified the deadline of the original calendar event

        //    // decreases the deadline
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    newDeadline = end.Add(- TimeSpan.FromTicks(duration.Ticks * 3));
        //    scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, testEvent.Start, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs when storing events
        //    Assert.IsFalse(retrievedCalendarEvent.isTestEquivalent(testEvent));// this should evaluate to false because we have modified the deadline of the original calendar event
        //}

        //[TestMethod]
        //public void DeadlineOfRepeatRestrictedSubEventUpdate()
        //{
        //    /// todo write update implementation for repeating events
        //}

        //// Test group test the change of the Range time line of the calendar event
        //[TestMethod]
        //public void RangeTimeLineRigidSubEventUpdate()
        //{
        //    // increases the range
        //    UserAccount currentuser = TestUtility.getTestUser();
        //    currentuser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
        //    TestSchedule schedule = new TestSchedule(currentuser, refNow, startOfDay);
        //    TimeSpan duration = TimeSpan.FromHours(1);
        //    DateTimeOffset start = refNow;
        //    DateTimeOffset end = start.Add(duration);
        //    CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, true);
        //    schedule.AddToScheduleAndCommit(testEvent).Wait();
        //    TestSchedule scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    DateTimeOffset newStart = testEvent.Start.Add(-TimeSpan.FromTicks((long)duration.Ticks / 2));
        //    DateTimeOffset newDeadline = testEvent.End.Add(duration);
        //    var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.ActiveSubEvents.First().getId, testEvent.getName, newStart, newDeadline, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    CalendarEvent retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    TimeSpan OneMinuteDiff = TimeSpan.FromMinutes(1);
        //    Assert.AreEqual(retrievedCalendarEvent.getActiveDuration, newDeadline - newStart);
        //    Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs
        //    Assert.IsTrue((retrievedCalendarEvent.Start - newStart) < OneMinuteDiff);/// doing this because of the rounding that occurs
        //    Assert.IsFalse(retrievedCalendarEvent.isTestEquivalent(testEvent));// this should evaluate to false because we have modified the deadline of the original calendar event

        //    // decreases the range
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    newStart = newStart.Add(-duration);
        //    newDeadline = newDeadline.Add(-TimeSpan.FromTicks((long)(duration.Ticks * 2)));
        //    scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.ActiveSubEvents.First().getId, testEvent.getName, newStart, newDeadline, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    Assert.AreEqual(retrievedCalendarEvent.getActiveDuration, newDeadline - newStart);
        //    Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs when storing events
        //    Assert.IsTrue((retrievedCalendarEvent.Start - newStart) < OneMinuteDiff);/// doing this because of the rounding that occurs
        //    Assert.IsFalse(retrievedCalendarEvent.isTestEquivalent(testEvent));// this should evaluate to false because we have modified the deadline of the original calendar event
        //}
        //[TestMethod]
        //public void RangeTimeLineRepeatRigidSubEventUpdate()
        //{
        //    /// todo write update implementation for repeating events
        //}

        //[TestMethod]
        //public void RangeTimeLineNonRigidSubEventUpdate()
        //{
        //    // increases the range
        //    UserAccount currentuser = TestUtility.getTestUser();
        //    currentuser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
        //    TestSchedule schedule = new TestSchedule(currentuser, refNow, startOfDay);
        //    TimeSpan duration = TimeSpan.FromHours(1);
        //    DateTimeOffset start = refNow;
        //    DateTimeOffset end = start.Add(duration);
        //    CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, false);
        //    schedule.AddToScheduleAndCommit(testEvent).Wait();
        //    TestSchedule scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    DateTimeOffset newStart = start.Add(-TimeSpan.FromTicks((long)duration.Ticks / 2));
        //    DateTimeOffset newDeadline = end.Add(duration);
        //    var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    CalendarEvent retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    TimeSpan OneMinuteDiff = TimeSpan.FromMinutes(1);
        //    Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs
        //    Assert.IsTrue((retrievedCalendarEvent.Start - newStart) < OneMinuteDiff);/// doing this because of the rounding that occurs
        //    Assert.IsFalse(retrievedCalendarEvent.isTestEquivalent(testEvent));// this should evaluate to false because we have modified the deadline of the original calendar event

        //    // decreases the range
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    newStart = newStart.Add(-duration);
        //    newDeadline = newDeadline.Add(-TimeSpan.FromTicks((long)(duration.Ticks * 2)));
        //    scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs when storing events
        //    Assert.IsTrue((retrievedCalendarEvent.Start - newStart) < OneMinuteDiff);/// doing this because of the rounding that occurs
        //    Assert.IsFalse(retrievedCalendarEvent.isTestEquivalent(testEvent));// this should evaluate to false because we have modified the deadline of the original calendar event

        //    // too small a range
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    newStart = newStart.Add(duration);
        //    newDeadline = newStart.Add(TimeSpan.FromTicks((long)(duration.Ticks / 2)));
        //    try
        //    {
        //        scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

        //    } catch(CustomErrors tilerError)
        //    {
        //        if(tilerError.Code == 40000001)
        //        {
        //            return;
        //        }
        //    }
        //    Assert.Fail("Error should have been thrown for range timeline being too small");
        //}

        //[TestMethod]
        //public void RangeTimeLineRepeatNonRigidSubEventUpdate()
        //{
        //    /// todo write update implementation for repeating events
        //}

        //[TestMethod]
        //public void RangeTimeLineRestrictedSubEventUpdate()
        //{
        //    // increases the range
        //    UserAccount currentuser = TestUtility.getTestUser();
        //    currentuser.Login().Wait();
        //    DateTimeOffset refNow = DateTimeOffset.UtcNow;
        //    DateTimeOffset startOfDay = TestUtility.parseAsUTC("10:00 pm");
        //    TestSchedule schedule = new TestSchedule(currentuser, refNow, startOfDay);
        //    TimeSpan duration = TimeSpan.FromHours(1);
        //    DateTimeOffset start = refNow;
        //    DateTimeOffset end = start.Add(duration.Add(duration).Add(duration));
        //    RestrictionProfile restrictionProfile = new RestrictionProfile(start.Add(duration), duration.Add(duration));
        //    CalendarEvent testEvent = TestUtility.generateCalendarEvent(duration, new Repetition(), start, end, 1, false, null, restrictionProfile, now: schedule.Now);
        //    schedule.AddToScheduleAndCommit(testEvent).Wait();
        //    TestSchedule scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    DateTimeOffset newStart = start.Add(-TimeSpan.FromTicks((long)duration.Ticks / 2));
        //    DateTimeOffset newDeadline = end.Add(duration);
        //    var scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    CalendarEvent retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    TimeSpan OneMinuteDiff = TimeSpan.FromMinutes(1);
        //    Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs
        //    Assert.IsTrue((retrievedCalendarEvent.Start - newStart) < OneMinuteDiff);/// doing this because of the rounding that occurs
        //    Assert.IsFalse(retrievedCalendarEvent.isTestEquivalent(testEvent));// this should evaluate to false because we have modified the deadline of the original calendar event

        //    // decreases the range
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    newStart = newStart.Add(-duration);
        //    newDeadline = newDeadline.Add(-TimeSpan.FromTicks((long)(duration.Ticks * 2)));
        //    scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

        //    scheduleReloaded.UpdateWithDifferentSchedule(scheduleUpdated.Item2).Wait();
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    retrievedCalendarEvent = scheduleReloaded.getCalendarEvent(testEvent.Calendar_EventID);
        //    Assert.IsTrue((newDeadline - retrievedCalendarEvent.getDeadline) < OneMinuteDiff);/// doing this because of the rounding that occurs when storing events
        //    Assert.IsTrue((retrievedCalendarEvent.Start - newStart) < OneMinuteDiff);/// doing this because of the rounding that occurs
        //    Assert.IsFalse(retrievedCalendarEvent.isTestEquivalent(testEvent));// this should evaluate to false because we have modified the deadline of the original calendar event

        //    // too small a range
        //    scheduleReloaded = new TestSchedule(currentuser, refNow, startOfDay);
        //    newStart = newStart.Add(duration);
        //    newDeadline = newStart.Add(TimeSpan.FromTicks((long)(duration.Ticks / 2)));
        //    try
        //    {
        //        scheduleUpdated = scheduleReloaded.BundleChangeUpdate(testEvent.getId, testEvent.getName, newStart, newDeadline, testEvent.NumberOfSplit, testEvent.Notes.UserNote);

        //    }
        //    catch (CustomErrors tilerError)
        //    {
        //        if (tilerError.Code == 40000001)
        //        {
        //            return;
        //        }
        //    }
        //    Assert.Fail("Error should have been thrown for range timeline being too small");
        //}

        //[TestMethod]
        //public void RangeTimeLineRepeatRestrictedSubEventUpdate()
        //{
        //    /// todo write update implementation for repeating events
        //}

        //[TestMethod]
        //public void RigidSubEventUpdate()
        //{
        //}

        //[TestMethod]
        //public void RepeatRigidSubEventUpdate()
        //{
        //}

        //[TestMethod]
        //public void NonRigidSubEventUpdate()
        //{
        //}

        //[TestMethod]
        //public void RepeatNonRigidSubEventUpdate()
        //{
        //}

        //[TestMethod]
        //public void RestrictedSubEventUpdate()
        //{
        //}

        //[TestMethod]
        //public void RepeatRestrictedSubEventUpdate()
        //{
        //}





    }
}
