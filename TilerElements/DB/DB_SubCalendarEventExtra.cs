using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront
{
    public class DB_SubCalendarEventExtra:SubCalendarEvent
    {
        internal DB_SubCalendarEventExtra(SubCalendarEvent mySubCalEvent, NowProfile NowProfileData, Procrastination ProcrastinationData)
        {
            this.BusyFrame = mySubCalEvent.ActiveSlot;
            this.CalendarEventRange = mySubCalEvent.getCalendarEventRange;
            this.FromRepeatEvent = mySubCalEvent.FromRepeat;
            this.EventName = mySubCalEvent.Name;
            this.EventDuration = mySubCalEvent.ActiveDuration;
            this.Complete = mySubCalEvent.isComplete;
            this.ConflictingEvents = mySubCalEvent.Conflicts;
            this.DataBlob = mySubCalEvent.Notes;
            this.DeadlineElapsed = mySubCalEvent.isDeadlineElapsed;
            this.Enabled = mySubCalEvent.isEnabled;
            this.EndDateTime = mySubCalEvent.End;
            this.EventPreDeadline = mySubCalEvent.PreDeadline;
            this.EventScore = mySubCalEvent.Score;
            this.isRestricted = mySubCalEvent.isEventRestricted;
            this.LocationInfo = mySubCalEvent.myLocation;
            this.OldPreferredIndex = mySubCalEvent.OldUniversalIndex;
            this.otherPartyID = mySubCalEvent.ThirdPartyID;
            this.preferredDayIndex = mySubCalEvent.UniversalDayIndex;
            this.PrepTime = mySubCalEvent.Preparation;
            this.Priority = mySubCalEvent.EventPriority;
            this.ProfileOfNow = NowProfileData;
            this.ProfileOfProcrastination = ProcrastinationData;
            //this.RepetitionFlag = mySubCalEvent.FromRepeat;
            this.RigidSchedule = mySubCalEvent.Rigid;
            this.StartDateTime = mySubCalEvent.Start;
            this.UiParams = mySubCalEvent.UIParam;
            this.UniqueID = mySubCalEvent.SubEvent_ID;
            this.UserDeleted = mySubCalEvent.isUserDeleted;
            this.UserIDs = mySubCalEvent.getAllUserIDs();
            this.Vestige = mySubCalEvent.isVestige;
            this.OriginalStart = mySubCalEvent.OrginalStartInfo;
        }
    }
}