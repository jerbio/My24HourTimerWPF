
        List<List<SubCalendarEvent>> FormatSnugTimeLine(List<List<SubCalendarEvent>> CompleteTimeLinesWithSnugPossibility)
        {
            foreach (List<SubCalendarEvent> SnugPermutationForTimeLine in CompleteTimeLinesWithSnugPossibility)
            { 
                
            }
        }


        List<List<SubCalendarEvent>> OptimizeForLocation(List<SubCalendarEvent> SnugPossibility, TimeLine RestrcitingTimeLine, CalendarEvent LookingForCalEvent)
         {
            List<BusyTimeLine> ListOfBusyTimeLines = new System.Collections.Generic.List<BusyTimeLine>();
            Dictionary<string, BusyTimeLine> Dict_SubcalIDAndRangeOfPossibleFit = new System.Collections.Generic.Dictionary<string, BusyTimeLine>();
            Dictionary<string, Tuple<SubCalendarEvent,TimeLine>> Dict_SubcalIDAndSubCal = new System.Collections.Generic.Dictionary<string,Tuple<SubCalendarEvent,TimeLine>>();
            RestrcitingTimeLine.OccupiedSlots=new BusyTimeLine[0];

            foreach (SubCalendarEvent InterFerringSubCalEvent in SnugPossibility)
            {
                string Zero_parentLevelID = InterFerringSubCalEvent.SubEvent_ID.getLevelID(0);
                
                TimeLine LatestTimeLine = new TimeLine();

                if (AllEventDictionary.ContainsKey(Zero_parentLevelID))
                {
                    if (AllEventDictionary[Zero_parentLevelID].RepetitionStatus)
                    {
                        LatestTimeLine = AllEventDictionary[Zero_parentLevelID].Repeat.getCalendarEvent(InterFerringSubCalEvent.SubEvent_ID.getStringIDAtLevel(1)).EventTimeLine.toBusyTimeLine(InterFerringSubCalEvent.ID);
                        Dict_SubcalIDAndRangeOfPossibleFit.Add(InterFerringSubCalEvent.ID, AllEventDictionary[Zero_parentLevelID].Repeat.getCalendarEvent(InterFerringSubCalEvent.SubEvent_ID.getStringIDAtLevel(1)).EventTimeLine.toBusyTimeLine(InterFerringSubCalEvent.ID));
                        Dict_SubcalIDAndSubCal.Add(InterFerringSubCalEvent.ID, new Tuple<SubCalendarEvent,TimeLine>(InterFerringSubCalEvent,AllEventDictionary[Zero_parentLevelID].Repeat.getCalendarEvent(InterFerringSubCalEvent.SubEvent_ID.getStringIDAtLevel(1)).EventTimeLine));
                        InterFerringSubCalEvent.PinToEnd(RestrcitingTimeLine,AllEventDictionary[Zero_parentLevelID].Repeat.getCalendarEvent(InterFerringSubCalEvent.SubEvent_ID.getStringIDAtLevel(1)));
                    }
                    else
                    {
                        Dict_SubcalIDAndRangeOfPossibleFit.Add(InterFerringSubCalEvent.ID, AllEventDictionary[Zero_parentLevelID].EventTimeLine.toBusyTimeLine(InterFerringSubCalEvent.ID));
                        
                        Dict_SubcalIDAndSubCal.Add(InterFerringSubCalEvent.ID, new Tuple<SubCalendarEvent,TimeLine>(InterFerringSubCalEvent,AllEventDictionary[Zero_parentLevelID].EventTimeLine));
                        
                        InterFerringSubCalEvent.PinToEnd(RestrcitingTimeLine,AllEventDictionary[Zero_parentLevelID]);
                    }
                }
                else 
                {
                    if (LookingForCalEvent.ID != Zero_parentLevelID)
                    { 
                        //this part of code should never be reached. This tests to see if an unadded event of AllEventDictionary is the same cal event that being fitted
                        throw new Exception("Error in code, invalid match of unadded calevent to AllEventDictionary value and in optimization");
                    }
                    else
                    {
                        if (LookingForCalEvent.RepetitionStatus)
                        {
                            Dict_SubcalIDAndRangeOfPossibleFit.Add(InterFerringSubCalEvent.ID, LookingForCalEvent.Repeat.getCalendarEvent(InterFerringSubCalEvent.SubEvent_ID.getStringIDAtLevel(1)).EventTimeLine.toBusyTimeLine(InterFerringSubCalEvent.ID));
                            Dict_SubcalIDAndSubCal.Add(InterFerringSubCalEvent.ID, new Tuple<SubCalendarEvent,TimeLine>(InterFerringSubCalEvent,LookingForCalEvent.Repeat.getCalendarEvent(InterFerringSubCalEvent.SubEvent_ID.getStringIDAtLevel(1)).EventTimeLine));
                            InterFerringSubCalEvent.PinToEnd(RestrcitingTimeLine,LookingForCalEvent.Repeat.getCalendarEvent(InterFerringSubCalEvent.SubEvent_ID.getStringIDAtLevel(1)));
                        }
                        else
                        {
                            Dict_SubcalIDAndRangeOfPossibleFit.Add(InterFerringSubCalEvent.ID, LookingForCalEvent.EventTimeLine.toBusyTimeLine(InterFerringSubCalEvent.ID));
                            Dict_SubcalIDAndSubCal.Add(InterFerringSubCalEvent.ID, new Tuple<SubCalendarEvent,TimeLine>(InterFerringSubCalEvent,AllEventDictionary[Zero_parentLevelID].EventTimeLine));
                            InterFerringSubCalEvent.PinToEnd(RestrcitingTimeLine,AllEventDictionary[Zero_parentLevelID]);
                        }
                    }

                }
            }
            ListOfBusyTimeLines = Dict_SubcalIDAndRangeOfPossibleFit.Values.ToList(); 
            ListOfBusyTimeLines=ListOfBusyTimeLines.OrderBy(obj=>obj.Start).ToList();


            

            List<BusyTimeLine>[] CategoriesOfCalEvents = CategorizeTimeLine_noEventSchedule(RestrcitingTimeLine, ListOfBusyTimeLines.ToArray());
        /*
             * SubEventsTimeCategories has 4 list of containing lists.
             * 1st is a List with Elements Starting before The Mycalendaervent timeline and ends after the busytimeline
             * 2nd is a list with elements starting before the mycalendarvent timeline but ending before the myevent timeline
             * 3rd is a list with elements starting after the Mycalendar event start time but ending after the Myevent timeline
             * 4th is a list with elements starting after the MyCalendar event start time and ends before the Myevent timeline 
             * */
            List<SubCalendarEvent> SubcCalEvent_ID = new System.Collections.Generic.List<SubCalendarEvent>();
            
            List<BusyTimeLine> EndingBeforeOrOnTimeEventEnd = CategoriesOfCalEvents[1];
            EndingBeforeOrOnTimeEventEnd.AddRange(CategoriesOfCalEvents[2]);
            EndingBeforeOrOnTimeEventEnd.AddRange(CategoriesOfCalEvents[3]);
            EndingBeforeOrOnTimeEventEnd = EndingBeforeOrOnTimeEventEnd.OrderBy(obj => obj.End).ToList();

            Dictionary<SubCalendarEvent,TimeLine> Dict_PseudoRigid_RestrictingTImeLine = new System.Collections.Generic.Dictionary<SubCalendarEvent,TimeLine>();


            foreach(BusyTimeLine mYbUSYTimeLine in EndingBeforeOrOnTimeEventEnd)
            {
                Dict_PseudoRigid_RestrictingTImeLine.Add(Dict_SubcalIDAndSubCal[mYbUSYTimeLine.ID].Item1,Dict_SubcalIDAndSubCal[mYbUSYTimeLine.ID].Item2);
            }


            List<SubCalendarEvent> SortedSubCalEvents = Utility.sortSubCalEventByDeadline(Dict_PseudoRigid_RestrictingTImeLine.Keys.ToList(), false);

            TimeLine[] FreeSpots=getAllFreeSpots_NoCompleteSchedule(RestrcitingTimeLine);

            EfficientTimeLine MyEfficientTimeLine = new EfficientTimeLine(RestrcitingTimeLine,Dict_PseudoRigid_RestrictingTImeLine);

            

            TimeLine ReferenceTimeLine = RestrcitingTimeLine.CreateCopy();
            ReferenceTimeLine.AddBusySlots(EndingBeforeOrOnTimeEventEnd.ToArray());

            TimeLine[] ArrayOfFreeSpots = getOnlyPertinentTimeFrame(getAllFreeSpots_NoCompleteSchedule(ReferenceTimeLine), ReferenceTimeLine).ToArray();


//BuildAllPossibleSnugLists(SortedInterFerringCalendarEvents, MyCalendarEvent, DictionaryWithBothCalendarEventIDAndListOfInterferringSubEvents, CalendarEventsTimeCategories, ReferenceTimeLine);
        }

        List<SubCalendarEvent> BuildPertinentList(List<SubCalendarEvent> AvailableSubCalendarEvents, TimeLine InvestigatingTimeLine)
        {
            List<SubCalendarEvent> PertinentList = new System.Collections.Generic.List<SubCalendarEvent>();
            foreach (SubCalendarEvent MySubCalEvent in AvailableSubCalendarEvents)
            { 
                
            }
            return PertinentList;
        }

        