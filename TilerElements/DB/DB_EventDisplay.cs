using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Wpf;
using System.ComponentModel.DataAnnotations.Schema;

namespace TilerElements.DB
{
    public class DB_EventDisplay:EventDisplay
    {
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


        /// <summary>
        /// Function provides the COlor object selected. It is the same as UI coloro of event display, only this provides setter
        /// </summary>
        public TilerColor Color
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

        public static DB_EventDisplay ConvertToPersistable(EventDisplay eventDisplay)
        {
            DB_EventDisplay retValue = new DB_EventDisplay()
            {
                Id = eventDisplay.Id,
                Color = eventDisplay.UIColor,
                isCompleteUI = eventDisplay.isComplete,
                isDefault = eventDisplay.DefaultId,
                Visible = eventDisplay.VisibleFlag
            };
            return retValue;
        }
    }
}
