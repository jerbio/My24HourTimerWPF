using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My24HourTimerWPF
{
    public struct Color
    {
        public int Red;
        public int Blue;
        public int Green;
        public double Opacity;
        
        public Color(int RedColor = 255, int BlueColor = 255, int GreenColor = 255, double Opacity=1)
        {
            Red = RedColor;
            Blue = BlueColor;
            Green = GreenColor;
            this.Opacity = Opacity;
        }
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
    }
}
