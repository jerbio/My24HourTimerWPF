using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements.Wpf
{
    
    public class EventDisplay
    {
        protected bool Visible;
        protected TilerColor eventColor;
        protected int Default = 0;//0->Default Calendar Colors,1->Set As Complete,2->subCalendar Event Specific colors,3->Calendar Event Specific colors
        protected bool CompleteUI;
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

        /// <summary>
        /// Function provides the COlor object selected. It is the same as UI coloro of event display, only this provides setter
        /// </summary>
        public virtual TilerColor Color
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
        virtual public TilerColor UIColor
        {
            get
            {
                return eventColor;
            }
        }

        virtual public bool isComplete
        {
            get
            {
                return CompleteUI;
            }
        }

        virtual public int DefaultId
        {
            get
            {
                return Default;
            }
        }

        virtual public bool VisibleFlag
        {
            get
            {
                return Visible;
            }
        }

        virtual public string Id
        {
            get
            {
                return ID;

            }
            set
            {
                Guid testValue;
                if (Guid.TryParse(value, out testValue))
                {
                    ID = value;
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
