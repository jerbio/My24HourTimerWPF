#define EnableOutlook

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Outlook = Microsoft.Office.Interop.Outlook;


namespace My24HourTimerWPF
{
    class ThirdPartyCalendarControl
    {
        public enum CalendarTool { Outlook, Google, Facebook };

        public ThirdPartyCalendarControl(CalendarTool myCalendar)
        { 
            
        }

        public void DeleteAppointment(SubCalendarEvent ActiveSection, string NameOfParentCalendarEvent, string entryID)
        {
#if EnableOutlook
            if (entryID == "")
            {
                return;
            }
            Outlook.Application outlookApp = new Microsoft.Office.Interop.Outlook.Application();
            Outlook.MAPIFolder calendar = outlookApp.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderCalendar);

            Outlook.Items calendarItems = calendar.Items;
            try
            {
                Outlook.AppointmentItem item = calendarItems[ActiveSection.ID + "**" + NameOfParentCalendarEvent] as Outlook.AppointmentItem;
                item.Delete();
            }
            catch
            {
                return;
            }
            /*Outlook.RecurrencePattern pattern =
                item.GetRecurrencePattern();
            Outlook.AppointmentItem itemDelete = pattern.
                GetOccurrence(new DateTime(2006, 6, 28, 8, 0, 0));

            if (itemDelete != null)
            {
                itemDelete.Delete();
            }*/
#endif
        }

        private string AddAppointment(SubCalendarEvent ActiveSection, string NameOfParentCalendarEvent)
        {
            try
            {
                Outlook.Application app = new Microsoft.Office.Interop.Outlook.Application();
                Outlook.AppointmentItem newAppointment = (Outlook.AppointmentItem)app.CreateItem(Outlook.OlItemType.olAppointmentItem);
                /*(Outlook.AppointmentItem)
            this.Application.CreateItem(Outlook.OlItemType.olAppointmentItem);*/
                newAppointment.Start = ActiveSection.Start;// DateTime.Now.AddHours(2);
                newAppointment.End = ActiveSection.End;// DateTime.Now.AddHours(3);
                newAppointment.Location = "TBD";
                newAppointment.Body = "JustTesting";
                newAppointment.AllDayEvent = false;
                newAppointment.Subject = ActiveSection.ID + "**" + NameOfParentCalendarEvent;
                /*newAppointment.Recipients.Add("Roger Harui");
                Outlook.Recipients sentTo = newAppointment.Recipients;
                Outlook.Recipient sentInvite = null;
                sentInvite = sentTo.Add("Holly Holt");
                sentInvite.Type = (int)Outlook.OlMeetingRecipientType
                    .olRequired;
                sentInvite = sentTo.Add("David Junca ");
                sentInvite.Type = (int)Outlook.OlMeetingRecipientType
                    .olOptional;
                sentTo.ResolveAll();*/
                newAppointment.Save();
                //newAppointment.EntryID;

                //newAppointment.Display(true);
                return newAppointment.EntryID;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("The following error occurred: " + ex.Message);
                return "";
            }
        }

        public void removeAllEventsFromOutLook(ICollection<CalendarEvent> ArrayOfCalendarEvents)
        {
            int i = 0;
            CalendarEvent[] ArrayOfCalendarevents = ArrayOfCalendarEvents.ToArray();
            for (; i < ArrayOfCalendarevents.Length; i++)//this loops through the ArrayOfValues and ArrayOfIndex. Since each index loop corresponds to the same dictionary entry.
            {
                RemoveFromOutlook(ArrayOfCalendarevents[i]); //this removes the value from outlook
                // AllEventDictionary.Remove(ArrayOfKeys[i]);//this removes the entry from The dictionary
            }
        }
        
        public void RemoveFromOutlook(CalendarEvent MyEvent)
        {

            int i = 0;
            if (MyEvent.RepetitionStatus)
            {
                LoopThroughRemoveRepeatEvents(MyEvent.Repeat);
            }
            else
            {
                for (i = 0; i < MyEvent.AllEvents.Length; i++)
                {
                    DeleteAppointment(MyEvent.AllEvents[i], MyEvent.Name, MyEvent.AllEvents[i].ThirdPartyID);
                }
            }



        }

        public void WriteToOutlook(CalendarEvent MyEvent)
        {
            int i = 0;
            if (MyEvent.RepetitionStatus)
            {
                LoopThroughAddRepeatEvents(MyEvent.Repeat);
            }
            else
            {
                for (; i < MyEvent.AllEvents.Length; i++)
                {
#if (EnableOutlook)
                    MyEvent.AllEvents[i].ThirdPartyID = AddAppointment(MyEvent.AllEvents[i], MyEvent.Name);/////////////
#endif                    
                }
            }


        }

        public void LoopThroughAddRepeatEvents(Repetition MyRepetition)
        {
            int i = 0;
            for (; i < MyRepetition.RecurringCalendarEvents.Length; i++)
            {
                WriteToOutlook(MyRepetition.RecurringCalendarEvents[i]);
            }
        }



        public void LoopThroughRemoveRepeatEvents(Repetition MyRepetition)
        {
            int i = 0;
            for (; i < MyRepetition.RecurringCalendarEvents.Length; i++)
            {
                RemoveFromOutlook(MyRepetition.RecurringCalendarEvents[i]);
            }
        }

    }
}
