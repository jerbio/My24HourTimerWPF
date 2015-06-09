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
        public static int LastLocationId = 1;
        public static double MaxLongitude = 181;
        public static double MaxLatitude = 91;
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
        protected string TaggedDescription = "";
        protected string TaggedAddress = "";
        /// <summary>
        /// was tiler able to pull location from google maps. If tiler fails to pull location from google maps then this location is null.
        /// </summary>
        protected bool NullLocation = true;
        /// <summary>
        /// is the current object the default location, which will initially boulder co, before recalculation based on user locations
        /// </summary>
        protected bool DefaultFlag = false;
        protected string LocationID = Guid.NewGuid().ToString();

        public Location_Elements()
        {
            xValue = defaultXValue;
            yValue = defaultYValue;
            NullLocation = true;
        }


        public Location_Elements(double MyxValue, double MyyValue, string Id = "")
        {
            xValue = MyxValue;
            yValue = MyyValue;
            NullLocation = false;
            if (!string.IsNullOrEmpty(Id))
            {
                Guid validId;
                bool IdParseSuccess = Guid.TryParse(Id, out validId);
                if (IdParseSuccess)
                {
                    LocationID = Id;
                }
            }
        }

        public Location_Elements(double MyxValue, double MyyValue, string AddressEntry, string AddressDescription, bool isNull,bool iaDefaultFlag, string ID = "")
        {
            xValue = MyxValue;
            yValue = MyyValue;
            TaggedAddress = AddressEntry;
            TaggedDescription = AddressDescription;
            NullLocation = isNull;
            if (string.IsNullOrEmpty(ID))
            {
                LocationID = Guid.NewGuid().ToString();
            }
            else
            {
                LocationID = ID;
            }
            DefaultFlag = isDefault;
        }

        public Location_Elements(string Address, string tag = "", string ID = "")
        {
            if (string.IsNullOrEmpty(Address))
            {
                Address = "";
            }

            if (string.IsNullOrEmpty(tag))
            {
                tag = "";
            }



            Address = Address.Trim();
            NullLocation = true;
            TaggedAddress = Address;
            TaggedDescription = tag;
            if (string.IsNullOrEmpty(ID))
            {
                LocationID = Guid.NewGuid().ToString();
            }
            else
            {
                LocationID = ID;
            }
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
                xValue = MaxLatitude;
                yValue = MaxLongitude;
                if (string.IsNullOrEmpty(TaggedDescription) && !string.IsNullOrEmpty(TaggedAddress))
                {
                    TaggedDescription = TaggedAddress.ToLower();
                }

                else
                {
                    if (string.IsNullOrEmpty(TaggedAddress) && !string.IsNullOrEmpty(TaggedDescription))
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
            defaultXValue = xLocation;
            defaultYValue = yLocation;
        }

        public static Location_Elements getDefaultLocation()
        {
            Location_Elements RetValue = new Location_Elements(defaultXValue, defaultYValue);
            RetValue.DefaultFlag = true;
            RetValue.NullLocation = false;
            return RetValue;
        }

        
        /// <summary>
        /// calculates distance of two locations. Result is in KM
        /// </summary>
        /// <param name="Location24A"></param>
        /// <param name="Location24B"></param>
        /// <param name="Worst"></param>
        /// <returns></returns>
        static public double calculateDistance(Location_Elements Location24A, Location_Elements Location24B, double Worst = double.MaxValue)
        {
            //note .... this function does not take into consideration the calendar event. So if there are two locations of the same calendarevent they will get scheduled right next to each other
            double maxDividedByTwo = MaxLongitude;
            if ((Location24A.xValue >= 180) || (Location24B.xValue > 180) || (Location24A.isNull) || (Location24B.isNull) || (Location24A.isDefault) || (Location24B.isDefault))
            {
                return Worst;
            }
            double R = 6371; // Radius of earth in KM
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


        static public double calculateDistance(List<Location_Elements> ListOfLocations, double worstDistance = -1)
        {
            if(worstDistance == -1)
            {
                worstDistance = double.MaxValue / (ListOfLocations.Count + 1);
            }
            int i = 0;
            int j = i + 1;
            double retValue = 0;
            while (j < ListOfLocations.Count)
            {
                retValue += calculateDistance(ListOfLocations[i], ListOfLocations[j], worstDistance);
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
            Location_struct retValue = new Location_struct();
            retValue.xValue = (float)xValue;
            retValue.yValue = (float)yValue;
            return retValue;
        }

        /// <summary>
        /// function returns the average location of several gps locations. Essentially, it'lll try to find the center of the various GPS locations. If there is a null or unfounded location, it is not included in the average calculation.
        /// </summary>
        /// <param name="Locations"></param>
        /// <param name="useDefaultLocation"></param>
        /// <returns></returns>
        static public Location_Elements AverageGPSLocation(IEnumerable<Location_Elements> Locations, bool useDefaultLocation=true)
        {
            Locations = Locations.Where(obj => !obj.isNull).ToList();
            Location_Elements retValue;
            Locations = Locations.Where(location => !location.isDefault && !location.isNull).ToList();
            if (Locations.Count() > 0)
            {
                if (Locations.Count() == 1)
                {
                    return Locations.Single();
                }

                double x = 0;
                double y = 0;
                double z = 0;

                foreach (var geoCoordinate in Locations)
                {
                    var latitude = geoCoordinate.XCoordinate * Math.PI / 180;
                    var longitude = geoCoordinate.YCoordinate * Math.PI / 180;

                    x += Math.Cos(latitude) * Math.Cos(longitude);
                    y += Math.Cos(latitude) * Math.Sin(longitude);
                    z += Math.Sin(latitude);
                }

                var total = Locations.Count();

                x = x / total;
                y = y / total;
                z = z / total;

                var centralLongitude = Math.Atan2(y, x);
                var centralSquareRoot = Math.Sqrt(x * x + y * y);
                var centralLatitude = Math.Atan2(z, centralSquareRoot);

                return new Location_Elements(centralLatitude * 180 / Math.PI, centralLongitude * 180 / Math.PI);
            }
            else
            {
                if (useDefaultLocation)
                {
                    retValue = getDefaultLocation();
                    retValue.DefaultFlag = true;
                }
                else
                {
                    retValue = new Location_Elements();
                }

            }

            return retValue;
        }

        public override string ToString()
        {
            return Address + "||" + xValue + "," + yValue;
        }

        public string justLongLatString()
        {
            return  xValue + "," + yValue+"\n";
        }

        public static Location_Elements getClosestLocation(IEnumerable<Location_Elements> AllLocations, Location_Elements RefLocation)
        {
            Location_Elements RetValue = null;
            double shortestDistance = double.MaxValue;
            foreach (Location_Elements eachLocation in AllLocations)
            {
                double DistanceSoFar = Location_Elements.calculateDistance(eachLocation, RefLocation);
                if (DistanceSoFar < shortestDistance)
                {
                    RetValue = eachLocation;
                    shortestDistance = DistanceSoFar;
                }
            }

            return RetValue;
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

        public bool isDefault
        {
            get
            {
                return DefaultFlag;
            }
        }

        public string ID
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