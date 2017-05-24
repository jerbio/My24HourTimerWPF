using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GoogleMapsApi.Entities.Geocoding.Request;
using GoogleMapsApi;
using GoogleMapsApi.Entities.Geocoding.Response;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;

namespace TilerElements
{
    public class Location
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
        protected string _Id = Guid.NewGuid().ToString();

        public Location()
        {
            xValue = defaultXValue;
            yValue = defaultYValue;
            NullLocation = true;
        }


        public Location(double MyxValue, double MyyValue, string Id = "")
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
                    _Id = Id;
                }
            }
        }

        public Location(double MyxValue, double MyyValue, string AddressEntry, string AddressDescription, bool isNull, bool isDefaultFlag, string ID = "")
        {
            xValue = MyxValue;
            yValue = MyyValue;
            TaggedAddress = AddressEntry;
            TaggedDescription = AddressDescription;
            NullLocation = isNull;
            if (string.IsNullOrEmpty(ID))
            {
                _Id = Guid.NewGuid().ToString();
            }
            else
            {
                _Id = ID;
            }
            DefaultFlag = isDefaultFlag;
        }

        public Location(string Address, string tag = "", string ID = "")
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
                _Id = Guid.NewGuid().ToString();
            }
            else
            {
                _Id = ID;
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
                GeocodingRequest request = new GeocodingRequest();
                request.Address = TaggedAddress;
                request.Sensor = false;


                var geocodingEngine = GoogleMaps.Geocode;
                GeocodingResponse geocode = geocodingEngine.Query(request);
                Console.WriteLine(geocode);

                if (geocode.Status == Status.OK)
                {
                    if (string.IsNullOrEmpty(TaggedDescription))
                    {
                        TaggedDescription = TaggedAddress;
                    }
                    var result = geocode.Results.First();
                    TaggedAddress = result.FormattedAddress.ToLower();
                    xValue = Convert.ToDouble(result.Geometry.Location.Latitude);
                    yValue = Convert.ToDouble(result.Geometry.Location.Longitude);
                    NullLocation = false;
                }
                else
                {
                    initializeWithNull();
                }
            }
            catch
            {
                initializeWithNull();
            }

            return NullLocation;
        }

        void initializeWithNull()
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

        #region Functions
        public static void InitializeDefaultLongLat(double xLocation, double yLocation)
        {
            defaultXValue = xLocation;
            defaultYValue = yLocation;
        }

        public static Location getDefaultLocation()
        {
            Location RetValue = new Location(defaultXValue, defaultYValue);
            RetValue.DefaultFlag = true;
            RetValue.NullLocation = false;
            return RetValue;
        }

        /// <summary>
        /// FUnction gets the timespan needed to travel from location 'first' to 'second'. The travelMode provides the desired medium of traveling.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="travelMode"></param>
        /// <returns>The timespan it'll take to travel between both points. Note if the timespan ticks is less than 0 then the value could not be evaluated</returns>
        static public TimeSpan getDrivingTimeFromWeb(Location first, Location second, TravelMode travelMode = TravelMode.Driving)
        {
            TimeSpan retValue = new TimeSpan(-1);
            if(!first.isNull && !second.isNull)
            {
                
                DirectionsRequest directionsRequest = new DirectionsRequest()
                {
                    Origin = first.justLongLatString(),
                    Destination = second.justLongLatString(),
                    TravelMode = travelMode
                };
                try
                {
                    DirectionsResponse directions = GoogleMaps.Directions.Query(directionsRequest);
                    if (directions.Status == DirectionsStatusCodes.OK)
                    {
                        var route = directions.Routes.First();
                        retValue = route.Legs.First().Duration.Value;
                    }
                }
                catch(Exception e)
                {
                    retValue = new TimeSpan(-1);
                }
                
            }
            

            return retValue;
        }

        
        /// <summary>
        /// calculates distance of two locations. Result is in KM
        /// </summary>
        /// <param name="Location24A"></param>
        /// <param name="Location24B"></param>
        /// <param name="Worst"></param>
        /// <returns></returns>
        static public double calculateDistance(Location Location24A, Location Location24B, double Worst = double.MaxValue)
        {
            //note .... this function does not take into consideration the calendar event. So if there are two locations of the same calendarevent they will get scheduled right next to each other
            double maxDividedByTwo = MaxLongitude;
            if ((Location24A.xValue >= 180) || (Location24B.xValue > 180) || (Location24A.isNull) || (Location24B.isNull))// || (Location24A.isDefault) || (Location24B.isDefault))
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


        static public double calculateDistance(List<Location> ListOfLocations, double worstDistance = -1)
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



        public Location CreateCopy()
        {
            Location this_cpy = new Location();
            this_cpy.TaggedAddress = this.TaggedAddress;
            this_cpy.TaggedDescription = this.TaggedDescription;
            this_cpy.xValue = this.xValue;
            this_cpy.yValue = this.yValue;
            this_cpy.NullLocation = this.NullLocation;
            this_cpy._Id = this._Id;
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
        static public Location AverageGPSLocation(IEnumerable<Location> Locations, bool useDefaultLocation=true)
        {
            Locations = Locations.Where(obj => !obj.isNull).ToList();
            Location retValue;
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

                return new Location(centralLatitude * 180 / Math.PI, centralLongitude * 180 / Math.PI);
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
                    retValue = new Location();
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

        public static Location getClosestLocation(IEnumerable<Location> AllLocations, Location RefLocation)
        {
            Location RetValue = null;
            double shortestDistance = double.MaxValue;
            foreach (Location eachLocation in AllLocations)
            {
                double DistanceSoFar = Location.calculateDistance(eachLocation, RefLocation);
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
            set
            {
                TaggedDescription = value;
            }
            get
            {
                return TaggedDescription;
            }
        }

        public string Address
        {
            set
            {
                TaggedAddress = value;
            }
            get
            {
                return TaggedAddress;
            }
        }

        public double XCoordinate
        {
            set
            {
                xValue = value;
            }
            get
            { return xValue; }

        }


        public double YCoordinate
        {
            set
            {
                yValue = value;
            }
            get
            {
                return yValue;
            }
        }

        public bool isNull
        {
            set
            {
                isNull = value;
            }
            get
            {
                return NullLocation;
            }
        }

        public bool isDefault
        {
            set
            {
                DefaultFlag = value;
            }
            get
            {
                return DefaultFlag;
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