using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace TilerElements
{
    
    public class EventDisplay: IUndoable
    {
        bool _Visible;
        TilerColor eventColor;
        int _Default = 0;//0->Default Calendar Colors,1->Set As Complete,2->subCalendar Event Specific colors,3->Calendar Event Specific colors
        string _Id = Guid.NewGuid().ToString();
        string _colorId = Guid.NewGuid().ToString();
        string _UndoId;

        #region undoMembers
        public bool UndoVisible;
        public TilerColor UndoEventColor;
        public int UndoDefault = 0;//0->Default Calendar Colors,1->Set As Complete,2->subCalendar Event Specific colors,3->Calendar Event Specific colors
        
        #endregion
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
        protected EventDisplay()
        {
            _Default = 0;
        }

        public EventDisplay(bool initialize = true)
        {
            if(initialize)
            {
                _Visible = true;
                eventColor = new TilerColor(127, 127, 127, 1);
                _Default = 0;
            }
            
        }
        public EventDisplay(bool VisibleFlag, TilerColor EventColor,int TypeOfDisplay=0,bool CompleteFlag=false)
        {
            _Visible = VisibleFlag;
            eventColor = EventColor;
            _Default = TypeOfDisplay;
        }

        public EventDisplay createCopy()
        {
            EventDisplay retValue = new EventDisplay(true);
            retValue ._Visible =_Visible;
            retValue.eventColor = eventColor;
            retValue._Default = _Default;//0->Default Calendar Colors,1->Set As Complete,2->subCalendar Event Specific colors,3->Calendar Event Specific colors
            return retValue;
        }

        public void undoUpdate(Undo undo)
        {
            UndoDefault = _Default;
            UndoVisible = _Visible;
            eventColor.undoUpdate(undo);
            FirstInstantiation = false;
            this._UndoId = undo.id;
        }

        public void undo(string undoId)
        {
            if(UndoId == undoId)
            {
                this.UndoVisible = _Visible;
                eventColor.undo(undoId);
            }
        }

        public void redo(string undoId)
        {
            if (UndoId == undoId)
            {
                this.UndoVisible = _Visible;
                eventColor.undo(undoId);
            }
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

        public bool isDefault
        {
            get 
            {
                return _Default == 0;
            }
        }

        public virtual bool FirstInstantiation { get; set; } = true;

        public string UndoId
        {
            set
            {
                _UndoId = value;
            }
            get
            {
                return _UndoId;
            }
        }

        public bool Visible
        {
            get
            {
                return _Visible;
            }
            set
            {
                _Visible = value;
            }
        }




        #endregion
    }

    
}
