using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TilerElements;



namespace My24HourTimerWPF
{
    static class SpreadOutList
    {

        public static List<List<TimeSpan>> GenerateListOfTimeSpan(List<List<TimeSpan>> MyEncasingList)
        {
            //Trying To Spread out arrray
            List<List<TimeSpan>> ListOfList = new List<List<TimeSpan>>();
            List<List<TimeSpan>> ListOfListCopy = ListOfList.ToList();
            int i = 0;
            for (i = 0; i < MyEncasingList.Count; i++)
            {
                ListOfListCopy = ListOfList.ToList();
                List<List<TimeSpan>> UpdatedListOfList = new List<List<TimeSpan>>();
                foreach (TimeSpan MyTimeSpan in MyEncasingList[i])//ListOfList.CopyTo(MyOtherArray);
                {
                    //Console.WriteLine("other {0}\n", MyNumber);
                    if (ListOfListCopy.Count == 0)
                    {
                        UpdatedListOfList.Add(CreateNewListAndAppend(MyTimeSpan, new List<TimeSpan>()));
                    }
                    foreach (List<TimeSpan> MyUpdatedSingleList in ListOfListCopy)
                    {
                        UpdatedListOfList.Add(CreateNewListAndAppend(MyTimeSpan, MyUpdatedSingleList));
                    }
                }
                ListOfList = UpdatedListOfList;
            }
            return ListOfList;
        }

        public static List<List<SubCalendarEvent>> GenerateListOfSubCalendarEvent(List<List<SubCalendarEvent>> MyEncasingList)
        {
            //Trying To Spread out arrray
            List<List<SubCalendarEvent>> ListOfList = new List<List<SubCalendarEvent>>();
            List<List<SubCalendarEvent>> ListOfListCopy = ListOfList.ToList();
            int i = 0;
            for (i = 0; i < MyEncasingList.Count; i++)
            {
                ListOfListCopy = ListOfList.ToList();
                List<List<SubCalendarEvent>> UpdatedListOfList = new List<List<SubCalendarEvent>>();
                foreach (SubCalendarEvent MySubCalendarEvent in MyEncasingList[i])//ListOfList.CopyTo(MyOtherArray);
                {
                    //Console.WriteLine("other {0}\n", MyNumber);
                    if (ListOfListCopy.Count == 0)
                    {
                        UpdatedListOfList.Add(CreateNewListAndAppend(MySubCalendarEvent, new List<SubCalendarEvent>()));
                    }
                    foreach (List<SubCalendarEvent> MyUpdatedSingleList in ListOfListCopy)
                    {
                        if (!isSubCalendarEventAlreadyInList(MySubCalendarEvent, MyUpdatedSingleList))
                        {
                            UpdatedListOfList.Add(CreateNewListAndAppend(MySubCalendarEvent, MyUpdatedSingleList));
                        }
                        
                    }
                }
                ListOfList = UpdatedListOfList;
            }
            return ListOfList;
        }


        static List<TimeSpan> CreateNewListAndAppend(TimeSpan MyTimeSpan, List<TimeSpan> MyListCopy)
        {
            List<TimeSpan> JustAnotherCopy = MyListCopy.ToList();
            JustAnotherCopy.Add(MyTimeSpan);
            return JustAnotherCopy;
        }

        static List<SubCalendarEvent> CreateNewListAndAppend(SubCalendarEvent MySubCalendarEvent, List<SubCalendarEvent> MyListCopy)
        {
            List<SubCalendarEvent> JustAnotherCopy = MyListCopy.ToList();
            JustAnotherCopy.Add(MySubCalendarEvent);
            return JustAnotherCopy;
        }

        static bool isSubCalendarEventAlreadyInList(SubCalendarEvent MySubCalendarEvent, List<SubCalendarEvent> MyListCopy)
        {
            foreach (SubCalendarEvent SubCalendarElement in MyListCopy)
            {
                if (MySubCalendarEvent.getId == SubCalendarElement.getId)
                {
                    return true;
                }
            }

            return false;
        }
    
    }
}
