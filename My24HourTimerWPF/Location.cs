using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
//using System.Windows.Forms;
using Google.Maps.Geocoding; 
using System.Drawing;
using TilerElements;


namespace TilerElements
{
    public class Location : Location_Elements
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

        /*double xValue;
        double yValue;
        string TaggedDescription;
        string TaggedAddress;
        bool NullLocation;
        int CheckDefault;*/

        public Location()
        {
            xValue = double.MaxValue;
            yValue = double.MaxValue;
        }

        public Location(double MyxValue, double MyyValue)
        {
            xValue = MyxValue;
            yValue = MyyValue;
        }

        public Location(double MyxValue, double MyyValue, string AddressEntry, string AddressDescription, bool isNull, int CheckDefault)
        {
            xValue = MyxValue;
            yValue = MyyValue;
            TaggedAddress = AddressEntry;
            TaggedDescription = AddressDescription;
            NullLocation = isNull;
            this.CheckDefault = CheckDefault;
        }
        
        public Location(string Address, string tag="")
        {
            
            NullLocation = true;
            if (string.IsNullOrEmpty(Address)||string.IsNullOrWhiteSpace(Address))
            {
                xValue = double.MaxValue;
                yValue = double.MaxValue;
                TaggedAddress = "";
                TaggedDescription = "";
            }
            else
            {
                Address = Address.Trim();
                try
                {
                    var request = new GeocodingRequest();
                    request.Address = Address;
                    request.Sensor = false;
                    var response = new GeocodingService().GetResponse(request);
                    var result = response.Results.First();
                    if (tag == "")
                    { TaggedDescription = Address; }
                    else 
                    {
                        TaggedDescription = tag;
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
                    if (tag == "")
                    { 
                        TaggedDescription = Address.ToLower(); 
                    }
                    else
                    {
                        TaggedDescription = tag.ToLower();
                    }
                    NullLocation = false;

                    TaggedAddress = Address;
                }
            }
        }


        #region Functions
        string getStringWebLocation24(float xLocation24, float yLocation24)
        {
            return "Hello";
        }
        float[,] getGPSWebLocation24(string Address)
        {
            
            return null;
        }

        static public double calculateDistance(Location Location24A, Location Location24B)
        {
            //note .... this function does not take into consideration the calendar event. So if there are two locations of the same calendarevent they will get scheduled right next to each other
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


        static public double calculateDistance(List<Location> ListOfLocations)
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

        /*
        private Geocode Deserialize( string json)
        {
            
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            
            
           
                Geocode geocode = javaScriptSerializer.Deserialize<Geocode>(json);
                return geocode;
            
        }
        */
        public Location CreateCopy()
        {
            Location this_cpy = new Location();
            this_cpy._token = this._token;
            this_cpy.TaggedAddress = this.TaggedAddress;
            this_cpy.TaggedDescription = this.TaggedDescription;
            this_cpy.xValue = this.xValue;
            this_cpy.yValue = this.yValue;
            this_cpy.NullLocation = this.NullLocation;
            return this_cpy;
        }

        public Location_struct toStruct()
        { 
            Location_struct retValue=new Location_struct();
            retValue.xValue=(float)xValue;
            retValue.yValue = (float)yValue;
            return retValue;
        }

        static public Location AverageGPSLocation(IEnumerable<Location> Locations)
        {
            Location retValue;
            if (Locations.Count() > 0)
            {
                double xCoord = Locations.Average(obj => obj.xValue);
                double yCoord = Locations.Average(obj => obj.yValue);
                retValue = new Location(xCoord, yCoord);
            }
            else 
            {
                retValue=new Location(0, 0);
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
        #endregion
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
}


