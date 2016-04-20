using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Google.Maps.Geocoding;


namespace TilerElements
{
    public class Location_Elements
    {
        string _token = string.Empty;
        enum requestType
        {
            authenticate,
            geocode,
            reverseGeocode,
            batchGeocode,
            route
        };

        static protected double defaultXValue = 105.2705;
        static protected double defaultYValue = 40.0150;

        protected double xValue;
        protected double yValue;
        protected string TaggedDescription;
        protected string TaggedAddress;
        protected bool NullLocation;
        protected int CheckDefault;
        protected int LocationID = 0;

        public Location_Elements()
        {
            xValue = defaultXValue;
            yValue = defaultYValue;
            LocationID = ID;
            NullLocation = true;
        }

        public Location_Elements(int ID)
        {
            LocationID = ID;
        }

        public Location_Elements(double MyxValue, double MyyValue, int ID = 0)
        {
            xValue = MyxValue;
            yValue = MyyValue;
        }

        public Location_Elements(double MyxValue, double MyyValue, string AddressEntry, string AddressDescription, bool isNull, int CheckDefault, int ID=0)
        {
            xValue = MyxValue;
            yValue = MyyValue;
            TaggedAddress = AddressEntry;
            TaggedDescription = AddressDescription;
            NullLocation = isNull;
            LocationID = ID;
            this.CheckDefault = CheckDefault;
        }

        public Location_Elements(string Address, string tag = "", int ID = 0)
        {
            Address=Address.Trim();
            NullLocation = true;
            TaggedAddress = Address;
            TaggedDescription = tag;
            LocationID = ID;
        }

        /// <summary>
        /// function tries to verify that the address provide exists in external service
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            TaggedAddress = TaggedAddress.Trim();
            try
            {
                var request = new GeocodingRequest();
                request.Address = TaggedAddress;
                request.Sensor = false;
                var response = new GeocodingService().GetResponse(request);
                var result = response.Results.First();
                if (string.IsNullOrEmpty(TaggedDescription))
                {
                    TaggedDescription = TaggedAddress;
                }

                TaggedAddress = result.FormattedAddress.ToLower();
                xValue = Convert.ToDouble(result.Geometry.Location.Latitude);
                yValue = Convert.ToDouble(result.Geometry.Location.Longitude);
                NullLocation = false;
                //MessageBox.Show("Found Location At: " + result.FormattedAddress + " Latitude: " + xValue + " Longitude: " + yValue); 
            }
            catch
            {
                xValue = double.MaxValue;
                yValue = double.MaxValue;
                if (string.IsNullOrEmpty(TaggedDescription)&& !string.IsNullOrEmpty(TaggedAddress))
                {
                    TaggedDescription = TaggedAddress.ToLower();
                }

                else
                {   
                    if(string.IsNullOrEmpty(TaggedAddress) && !string.IsNullOrEmpty(TaggedDescription))
                    {
                        TaggedAddress = TaggedDescription.ToLower();
                    }
                }
                NullLocation = true;
            }

            return NullLocation;
        }

        #region Functions
        public static void InitializeDefaultLongLat(double xLocation, double yLocation)
        {
            defaultXValue= xLocation;
            defaultYValue= yLocation;
        }

        public static Location_Elements getDefaultLocation()
        {
            Location_Elements RetValue = new Location_Elements(defaultXValue,defaultYValue);
            return RetValue;
        }

        string getStringWebLocation24(float xLocation24, float yLocation24)
        {
            return "Hello";
        }
        float[,] getGPSWebLocation24(string Address)
        {
            
            return null;
        }

        static public double calculateDistance(Location_Elements Location24A, Location_Elements Location24B, double Worst=double.MaxValue)
        {
            //note .... this function does not take into consideration the calendar event. So if there are two locations of the same calendarevent they will get scheduled right next to each other
            double maxDividedByTwo=double.MaxValue/2;
            if ((Location24A.xValue >= maxDividedByTwo)||(Location24B.xValue >maxDividedByTwo))
            {
                return Worst;
            }
            double R = 3958.7558657440545; // Radius of earth in Miles 
            double dLat = toRad(Location24A.xValue - Location24B.xValue);
            double dLon = toRad(Location24A.yValue - Location24B.yValue);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(toRad(Location24A.xValue)) * Math.Cos(toRad(Location24A.xValue)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = R * c;
            return d;
        }

        static double toRad(double value)
        {
            return value * Math.PI / 180;
        }


        static public double calculateDistance(List<Location_Elements> ListOfLocations)
        {
            int i = 0;
            int j = i + 1;
            double retValue = 0;
            while (j < ListOfLocations.Count)
            {
                retValue += calculateDistance(ListOfLocations[i], ListOfLocations[j]);
                i++; j++;
            }
            return retValue;
        }
        private string HttpGetWebRequest(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "GET";

            HttpWebResponse httpResponse = null;
            try
            {
                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch { return null; }

            string JSON = string.Empty;

            using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
                JSON = reader.ReadToEnd();

            return JSON;
        }

        

        public Location_Elements CreateCopy()
        {
            Location_Elements this_cpy = new Location_Elements();
            this_cpy._token = this._token;
            this_cpy.TaggedAddress = this.TaggedAddress;
            this_cpy.TaggedDescription = this.TaggedDescription;
            this_cpy.xValue = this.xValue;
            this_cpy.yValue = this.yValue;
            this_cpy.NullLocation = this.NullLocation;
            this_cpy.LocationID = this.LocationID;
            return this_cpy;
        }

        public Location_struct toStruct()
        { 
            Location_struct retValue=new Location_struct();
            retValue.xValue=(float)xValue;
            retValue.yValue = (float)yValue;
            return retValue;
        }

        static public Location_Elements AverageGPSLocation(IEnumerable<Location_Elements> Locations, bool useDefaultLocation=true)
        {
            Location_Elements retValue;
            if (Locations.Count() > 0)
            {
                double xCoord = Locations.Average(obj => obj.xValue);
                double yCoord = Locations.Average(obj => obj.yValue);
                retValue = new Location_Elements(xCoord, yCoord, -1);
            }
            else 
            {
                if (useDefaultLocation)
                {
                    retValue = new Location_Elements(defaultXValue, defaultYValue, -1);
                }
                else
                {
                    retValue = new Location_Elements(0, 0, -1);
                }
                
            }

            return retValue;
        }

        public override string ToString()
        {
            return Address + "||" + yValue + "\",\"" + xValue;
        }
        #endregion 


        #region Properties

        public string Description
        {
            get 
            {
                return TaggedDescription;
            }
        }

        public string Address
        {
            get
            {
                return TaggedAddress;
            }
        }

        public double XCoordinate
        {
            get
            { return xValue; }
            
        }


        public double YCoordinate
        {
            get
            {
                return yValue;
            }
        }

        public bool isNull
        {
            get
            {
                return NullLocation;
            }
        }

        public int DefaultCheck
        {
            get
            {
                return CheckDefault;
            }
        }

        public int ID
        {
            get
            {
                return LocationID;
            }
        }
        #endregion
    }
}


public struct Location_struct
{
    public float xValue;
    public float yValue;
    public int Number;
    /*
    public override string ToString()
    {
        return yValue + "," + xValue;
    }
    */
}