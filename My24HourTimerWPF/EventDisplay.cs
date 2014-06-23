using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    
    public class EventDisplay
    {
        bool Visible;
        Color eventColor;
        int Default = 0;//0->Default Calendar Colors,1->Set As Complete,2->subCalendar Event Specific colors,3->Calendar Event Specific colors
        bool CompleteUI;
        public EventDisplay()
        {
            Visible = true;
            eventColor = new Color(127, 127, 127, 1);
            Default = 0;
        }
        public EventDisplay(bool VisibleFlag, Color EventColor,int TypeOfDisplay=0,bool CompleteFlag=false)
        {
            Visible = VisibleFlag;
            eventColor = EventColor;
            Default = TypeOfDisplay;
            CompleteUI = CompleteFlag;
        }

        public EventDisplay createCopy()
        {
            EventDisplay retValue = new EventDisplay();
            retValue .Visible =Visible;
            retValue.eventColor = eventColor;
            retValue.Default = Default;//0->Default Calendar Colors,1->Set As Complete,2->subCalendar Event Specific colors,3->Calendar Event Specific colors
            retValue.CompleteUI = CompleteUI;
            return retValue;
        }

        public void setCompleteUI(bool completeFlag)
        {
            CompleteUI = completeFlag;
        }



        #region Properties
        public bool isVisible
        {
            get
            {
                return Visible;
            }
        }

        public Color UIColor
        {
            get
            {
                return eventColor;
            }
        }


        public int isDefault
        {
            get 
            {
                return Default;
            }
        }

        public bool isCompleteUI
        {
            get
            {
                return CompleteUI;
            }
        }

        

        

        #endregion
    }

    
}
