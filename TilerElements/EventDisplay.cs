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
        protected string ID { get; set; } = Guid.NewGuid().ToString();

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
        virtual public bool isVisible
        {
            get
            {
                return Visible;
            }
            set
            {
                Visible = value;
            }
        }

        public TilerColor UIColor
        {
            get
            {
                return eventColor;
            }
            set
            {
                eventColor = value;
            }
        }


        virtual public int isDefault
        {
            get 
            {
                return Default;
            }
            set
            {
                Default = value;
            }
        }

        virtual public bool isCompleteUI
        {
            get
            {
                return CompleteUI;
            }
            set
            {
                CompleteUI = value;
            }
        }

        virtual public string Id
        {
            get {
                return ID;

            }
            set
            {
                Guid testValue;
                if(Guid.TryParse(value,out testValue))
                {
                    Id = value;
                }
                else
                {
                    throw new Exception("Invalid id for event display");
                }
                
            }
        }
        

        

        

        #endregion
    }

    
}
