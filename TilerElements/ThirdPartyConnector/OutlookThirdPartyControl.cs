#define EnableOutlook

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TilerElements.Wpf;

#if EnableOutlook
using Outlook = Microsoft.Office.Interop.Outlook;
#endif

namespace TilerElements.ThirdPartyConnector
{
    public class OutlookThirdPartyControl : ThirdPartyCalendarControl
    {
        
        public OutlookThirdPartyControl()
        {
            SelectedCalendarTool = ThirdPartyControl.CalendarTool.Outlook;
        }


        public OutlookThirdPartyControl( Dictionary<string, CalendarEvent> CalendarData)
        {
            IDToCalendarEvent = CalendarData;

        }
        virtual public void removeAllEventsFromOutLook(ICollection<CalendarEvent> ArrayOfCalendarEvents)
        {
            int i = 0;
            CalendarEvent[] ArrayOfCalendarevents = ArrayOfCalendarEvents.ToArray();
            for (; i < ArrayOfCalendarevents.Length; i++)//this loops through the ArrayOfValues and ArrayOfIndex. Since each index loop corresponds to the same dictionary entry.
            {
                RemoveFromOutlook(ArrayOfCalendarevents[i]); //this removes the value from outlook
                // AllEventDictionary.Remove(ArrayOfKeys[i]);//this removes the entry from The dictionary
            }
        }

        override public void DeleteAppointment(SubCalendarEvent ActiveSection, string NameOfParentCalendarEvent = "", string entryID = "")
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
                string SubJectString = ActiveSection.ID + "**" + NameOfParentCalendarEvent;
                if (ActiveSection.isComplete)
                {
                    //SubJectString = ActiveSection.ID + "*\u221A*" + NameOfParentCalendarEvent;
                }
                Outlook.AppointmentItem item = calendarItems[SubJectString] as Outlook.AppointmentItem;
                item.Delete();
            }
            catch
            {
                return;
            }
            /*Outlook.RecurrencePattern pattern =
                item.GetRecurrencePattern();
            Outlook.AppointmentItem itemDelete = pattern.
                GetOccurrence(new DateTimeOffset(2006, 6, 28, 8, 0, 0));

            if (itemDelete != null)
            {
                itemDelete.Delete();
            }*/
#endif

        }
        override public string AddAppointment(SubCalendarEvent ActiveSection, string NameOfParentCalendarEvent = "")
        {
            if (!ActiveSection.isEnabled)
            {
                return "";
            }
#if EnableOutlook
            try
            {
                Outlook.Application app = new Microsoft.Office.Interop.Outlook.Application();
                Outlook.AppointmentItem newAppointment = (Outlook.AppointmentItem)app.CreateItem(Outlook.OlItemType.olAppointmentItem);
                /*(Outlook.AppointmentItem)
            this.Application.CreateItem(Outlook.OlItemType.olAppointmentItem);*/
                newAppointment.Start = ActiveSection.Start.DateTime;// DateTimeOffset.Now.AddHours(2);
                newAppointment.End = ActiveSection.End.DateTime;// DateTimeOffset.Now.AddHours(3);
                newAppointment.Location = "TBD";
                newAppointment.Body = "JustTesting";
                newAppointment.AllDayEvent = false;
                string SubJectString = ActiveSection.ID + "**" + NameOfParentCalendarEvent;
                if (ActiveSection.isComplete)
                {
                    //SubJectString = ActiveSection.ID + "*\u221A*" + NameOfParentCalendarEvent;
                }
                newAppointment.Subject = SubJectString;// ActiveSection.ID + "**" + NameOfParentCalendarEvent;

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

#endif
            return "";


        }




        virtual public void RemoveFromOutlook(CalendarEvent MyEvent)
        {

            int i = 0;
            if (MyEvent.RepetitionStatus)
            {
                LoopThroughRemoveRepeatEvents(MyEvent.Repeat);
            }
            else
            {
                for (i = 0; i < MyEvent.AllSubEvents.Length; i++)
                {
                    SubCalendarEvent pertinentSubCalEvent = MyEvent.AllSubEvents[i];
                    DeleteAppointment(pertinentSubCalEvent, MyEvent.Name, pertinentSubCalEvent.ThirdPartyID);
                }
            }
        }

        virtual public void WriteToOutlook(CalendarEvent MyEvent)
        {
#if (EnableOutlook)
            int i = 0;
            if (!MyEvent.isActive)
            {
                return;
            }
            if (MyEvent.RepetitionStatus)
            {
                LoopThroughAddRepeatEvents(MyEvent.Repeat);
            }
            else
            {
                SubCalendarEvent[] enableSubCalEVents = MyEvent.ActiveSubEvents;
                for (; i < enableSubCalEVents.Length; i++)
                {

                    SubCalendarEvent pertinentSubCalEvent = enableSubCalEVents[i];
                    pertinentSubCalEvent.ThirdPartyID = AddAppointment(pertinentSubCalEvent, MyEvent.Name);/////////////

                }
            }
#endif

        }

        virtual public void LoopThroughAddRepeatEvents(Repetition MyRepetition)
        {
            int i = 0;
            CalendarEvent[] recurringEvents = MyRepetition.RecurringCalendarEvents();
            for (; i < recurringEvents.Length; i++)
            {
                WriteToOutlook(recurringEvents[i]);
            }
        }



        virtual public void LoopThroughRemoveRepeatEvents(Repetition MyRepetition)
        {
            int i = 0;
            CalendarEvent[] recurringEvents = MyRepetition.RecurringCalendarEvents();
            for (; i < recurringEvents.Length; i++)
            {
                RemoveFromOutlook(recurringEvents[i]);
            }
        }


    }
}
