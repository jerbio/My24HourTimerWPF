using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    public struct Color
    {
        int Red;
        int Blue;
        int Green;
        double Opacity;
        
        public Color(int RedColor = 255, int BlueColor = 255, int GreenColor = 255, double Opacity=1)
        {
            Red = RedColor;
            Blue = BlueColor;
            Green = GreenColor;
            this.Opacity = Opacity;
        }
        
        #region Functions
        public Color createCopy()
        {
            Color retValue = new Color();
            retValue.Red = Red;
            retValue.Blue = Blue;
            retValue.Green = Green;
            retValue.Opacity = Opacity;

            return retValue;
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
        #endregion
    }
}
