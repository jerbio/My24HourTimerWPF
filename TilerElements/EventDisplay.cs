using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace TilerElements
{
    
    public class EventDisplay
    {
        bool Visible;
        TilerColor eventColor;
        int _Default = 0;//0->Default Calendar Colors,1->Set As Complete,2->subCalendar Event Specific colors,3->Calendar Event Specific colors
        string _Id = Guid.NewGuid().ToString();
        string _colorId = Guid.NewGuid().ToString();
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
            _Default = 0;
        }
        public EventDisplay(bool VisibleFlag, TilerColor EventColor,int TypeOfDisplay=0,bool CompleteFlag=false)
        {
            Visible = VisibleFlag;
            eventColor = EventColor;
            _Default = TypeOfDisplay;
        }

        public EventDisplay createCopy()
        {
            EventDisplay retValue = new EventDisplay();
            retValue .Visible =Visible;
            retValue.eventColor = eventColor;
            retValue._Default = _Default;//0->Default Calendar Colors,1->Set As Complete,2->subCalendar Event Specific colors,3->Calendar Event Specific colors
            return retValue;
        }


        #region Properties
        public string ColorId
        {
            get
            {
                return _colorId;
            }
            set
            {
                _colorId = value;
            }
        }

        [ForeignKey("ColorId")]
        public TilerColor UIColor
        {
            set
            {
                eventColor = value;
            }
            get
            {
                return eventColor;
            }
        }

        public int Default
        {
            get
            {
                return _Default;
            }
        }

        public int isDefault
        {
            get 
            {
                return _Default;
            }
        }

        

        

        #endregion
    }

    
}
