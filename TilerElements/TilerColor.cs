using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class TilerColor: IUndoable
    {
        string _Id;
        int Red;
        int Blue;
        int Green;
        double Opacity;
        int ColorSelection;
        string HexColor;
        string _UndoId;
        public bool _FirstInstantiation;

        #region undoMembers
        public int UndoRed;
        public int UndoBlue;
        public int UndoGreen;
        public double UndoOpacity;
        public int UndoColorSelection;
        public string UndoHexColor;
        #endregion

        public TilerColor(int RedColor = 255, int BlueColor = 255, int GreenColor = 255, double Opacity=1, int Selection=-1)
        {
            UndoRed = 0;
            UndoBlue = 0;
            UndoGreen = 0;
            UndoOpacity = 0;
            UndoColorSelection = 0;
            UndoHexColor = "";
            _FirstInstantiation = false;
            _UndoId = "";
            Red = RedColor;
            Blue = BlueColor;
            Green = GreenColor;
            this.Opacity = Opacity;
            ColorSelection = Selection;
            _Id = Guid.NewGuid().ToString();
            string r = Red.ToString("X");
            string g = Green.ToString("X");
            string b = Blue.ToString("X");
            if (r.Length == 1)
            {
                r = "0" + r;
            }

            if (g.Length == 1)
            {
                g = "0" + g;
            }

            if (b.Length == 1)
            {
                b = "0" + b;
            }
            HexColor = '#' + r + g + b;
            updateHexColor();
        }
        
        #region Functions
        public TilerColor createCopy()
        {
            TilerColor retValue = new TilerColor();
            retValue.Red = Red;
            retValue.Blue = Blue;
            retValue.Green = Green;
            retValue.Opacity = Opacity;
            retValue.HexColor = HexColor;
            return retValue;
        }

        public void updateHexColor()
        {
            string r = Red.ToString("X");
            string g = Green.ToString("X");
            string b = Blue.ToString("X");
            if (r.Length == 1)
            {
                r = "0" + r;
            }

            if (g.Length == 1)
            {
                g = "0" + g;
            }

            if (b.Length == 1)
            {
                b = "0" + b;
            }
            HexColor = '#'+r + g + b;
        }

        public void undoUpdate(Undo undo)
        {
            UndoRed = Red;
            UndoBlue = Blue;
            UndoGreen = Green;
            UndoOpacity = Opacity;
            UndoColorSelection = ColorSelection;
            UndoHexColor = HexColor;
            this._UndoId = undo.id;
            _FirstInstantiation = true;
        }

        public void undo(string undoId)
        {
            if(this.UndoId == undoId)
            {
                Utility.Swap(ref UndoRed, ref Red);
                Utility.Swap(ref UndoBlue, ref Blue);
                Utility.Swap(ref UndoGreen, ref Green);
                Utility.Swap(ref UndoOpacity, ref Opacity);
                Utility.Swap(ref UndoColorSelection, ref ColorSelection);
                Utility.Swap(ref UndoHexColor, ref HexColor);
            }
        }

        public void redo(string undoId)
        {
            if (this.UndoId == undoId)
            {
                Utility.Swap(ref UndoRed, ref Red);
                Utility.Swap(ref UndoBlue, ref Blue);
                Utility.Swap(ref UndoGreen, ref Green);
                Utility.Swap(ref UndoOpacity, ref Opacity);
                Utility.Swap(ref UndoColorSelection, ref ColorSelection);
                Utility.Swap(ref UndoHexColor, ref HexColor);
            }
        }
        #endregion



        #region Properties
        public int R
        {
            set
            {
                if (value < 0)
                {
                    Red = 0;
                }
                if (value > 255)
                {
                    Red = 255;
                }
                updateHexColor();
            }
            get
            {
                return Red;
            }
        }
        public int G 
        { 
            set
            {
                if (value < 0)
                {
                    Green = 0;
                }
                if(value>255)
                {
                    Green=255;
                }
                updateHexColor();
            } 
            get
            {
                return Green;
            }
                
        }
        public int B
        {
            set
            {
                if (value < 0)
                {
                    Blue = 0;
                }
                if (value > 255)
                {
                    Blue = 255;
                }
                updateHexColor();
            }
            get
            {
                return Blue;
            }
        }
        public double O 
        {
            set
            {
                if (value < 0)
                {
                    Opacity = 0;
                }
                if (value > 1)
                {
                    Opacity = 1;
                }
            }
            get
            {
                return Opacity;
            }
        }

        public int User
        {
            set
            {
                ColorSelection=value;
            }
            get
            {
                return ColorSelection;
            }
        }

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

        public bool FirstInstantiation
        {
            set
            {
                _FirstInstantiation = value;
            }
            get
            {
                return _FirstInstantiation;
            }
        }
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
        #endregion
    }
}
