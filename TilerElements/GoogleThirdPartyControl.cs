using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class GoogleThirdPartyControl:ThirdPartyCalendarControl
    {
        public GoogleThirdPartyControl(Dictionary<string, CalendarEvent> CalendarData)
        {
            SelectedCalendarTool = ThirdPartyControl.CalendarTool.Google;
            IDToCalendarEvent = CalendarData;
        }

        public GoogleThirdPartyControl(IEnumerable< CalendarEvent> CalendarData)
        {
            SelectedCalendarTool = ThirdPartyControl.CalendarTool.Google;
            IDToCalendarEvent = CalendarData.ToDictionary(obj => obj.ID, obj => obj); 
        }

        public override string AddAppointment(SubCalendarEvent ActiveSection, string NameOfParentCalendarEvent = "")
        {
            throw new NotImplementedException();
        }

        public override void DeleteAppointment(SubCalendarEvent ActiveSection, string NameOfParentCalendarEvent = "", string entryID = "")
        {
            throw new NotImplementedException();
        }
    }
}
