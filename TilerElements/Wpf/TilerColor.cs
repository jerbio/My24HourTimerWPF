using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements.Wpf
{
    public class TilerColor
    {
        protected int Red;
        protected int Blue;
        protected int Green;
        protected double Opacity;
        protected int ColorSelection;
        protected string ID = Guid.NewGuid().ToString();

        public TilerColor(int RedColor = 255, int BlueColor = 255, int GreenColor = 255, double Opacity=1, int Selection=-1)
        {
            Red = RedColor;
            Blue = BlueColor;
            Green = GreenColor;
            this.Opacity = Opacity;
            ColorSelection = Selection;
        }
        
        #region Functions
        public TilerColor createCopy()
        {
            TilerColor retValue = new TilerColor();
            retValue.Red = Red;
            retValue.Blue = Blue;
            retValue.Green = Green;
            retValue.Opacity = Opacity;

            return retValue;
        }
        #endregion



        #region Properties
        virtual public int R
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
            }
            get
            {
                return Red;
            }
        }
        virtual public int G 
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
            } 
            get
            {
                return Green;
            }
                
        }
        virtual public int B
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
            }
            get
            {
                return Blue;
            }
        }
        virtual public double O 
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


        virtual public int UserColorSelection
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
