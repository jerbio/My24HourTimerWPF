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
        int Default=0;

        public EventDisplay()
        {
            Visible = true;
            eventColor = new Color(127, 127, 127, 1);
            Default = 0;
        }
        public EventDisplay(bool VisibleFlag, Color EventColor,int TypeOfDisplay=1)
        {
            Visible = VisibleFlag;
            eventColor = EventColor;
            Default = TypeOfDisplay;
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

        #endregion
    }

    
}
