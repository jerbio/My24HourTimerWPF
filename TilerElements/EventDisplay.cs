using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    
    public class EventDisplay
    {
        bool Visible;
        TilerColor eventColor;
        int Default = 0;//0->Default Calendar Colors,1->Set As Complete,2->subCalendar Event Specific colors,3->Calendar Event Specific colors
        bool CompleteUI;
        string _Id = Guid.NewGuid().ToString();
        public string Id
        {
            get
            {
                return _Id;
            }
            set
            {
                _Id = value;
            }
        }
        public EventDisplay()
        {
            Visible = true;
            eventColor = new TilerColor(127, 127, 127, 1);
            Default = 0;
        }
        public EventDisplay(bool VisibleFlag, TilerColor EventColor,int TypeOfDisplay=0,bool CompleteFlag=false)
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

        public TilerColor UIColor
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
