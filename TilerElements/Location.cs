using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using GoogleMapsApi.Entities.Geocoding.Request;
using GoogleMapsApi;
using GoogleMapsApi.Entities.Geocoding.Response;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TilerElements
{
    public class Location: IUndoable, IHasId
    {
        public static int LastLocationId = 1;
        public static double MaxLongitude = 181;
        public static double MaxLatitude = 91;
        public static string _ApiKey;
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

        protected double _Latitude;
        protected double _Longitude;
        protected string _TaggedDescription = "";
        protected string _SearchdDescription = "";
        protected string _TaggedAddress = "";
        protected string _UndoId = "";
        protected TilerUser _User;

        /// <summary>
        /// was tiler able to pull location from google maps. If tiler fails to pull location from google maps then this location is null.
        /// </summary>
        protected bool _NullLocation = true;
        /// <summary>
        /// is the current object the default location, which will initially boulder co, before recalculation based on user locations
        /// </summary>
        protected bool _DefaultFlag = false;

        #region undoDatamembers
        protected double _UndoLatitude;
        protected double _UndoLongitude;
        protected string _UndoTaggedDescription = "";
        protected string _UndoTaggedAddress = "";
        protected bool _UndoNullLocation = true;
        protected bool _UndoDefaultFlag = false;
#endregion
        protected string _Id = Guid.NewGuid().ToString();
#region Constructor
        public Location()
        {
            _Latitude = defaultXValue;
            _Longitude = defaultYValue;
            _NullLocation = true;
        }

        protected TilerEvent _Event;
        [NotMapped]
        public TilerEvent AssociatedEvent
        {
            get
            {
                return _Event;
            }
            set
            {
                _Event = value;
            }
        }

        public Location(double MyxValue, double MyyValue, string Id = "")
        {
            _Latitude = MyxValue;
            _Longitude = MyyValue;
            _NullLocation = false;
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
            _Latitude = MyxValue;
            _Longitude = MyyValue;
            _TaggedAddress = AddressEntry;
            _TaggedDescription = AddressDescription;
            updateSearchedLocation();
            _NullLocation = isNull;
            if (string.IsNullOrEmpty(ID))
            {
                _Id = Guid.NewGuid().ToString();
            }
            else
            {
                _Id = ID;
            }
            _DefaultFlag = isDefaultFlag;
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
            _NullLocation = true;
            _TaggedAddress = Address;
            _TaggedDescription = tag;

            updateSearchedLocation();
            if (string.IsNullOrEmpty(ID))
            {
                _Id = Guid.NewGuid().ToString();
            }
            else
            {
                _Id = ID;
            }
        }
#endregion
        /// <summary>
        /// function tries to verify that the address provide exists in external service
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            bool retValue = false;
            _TaggedAddress = _TaggedAddress.Trim();
            try
            {
                
                if(!String.IsNullOrEmpty(_TaggedAddress))
                {
                    GeocodingRequest request = new GeocodingRequest();
                    request.Address = _TaggedAddress;
                    request.Sensor = true;
                    request.ApiKey = Location.ApiKey;

                    var geocodingEngine = GoogleMaps.Geocode;
                    GeocodingResponse geocode = geocodingEngine.Query(request);

                    if (geocode.Status == Status.OK)
                    {
                        if (string.IsNullOrEmpty(_TaggedDescription))
                        {
                            _TaggedDescription = _TaggedAddress;
                        }
                        var result = geocode.Results.First();
                        _TaggedAddress = result.FormattedAddress.ToLower();
                        _Latitude = Convert.ToDouble(result.Geometry.Location.Latitude);
                        _Longitude = Convert.ToDouble(result.Geometry.Location.Longitude);
                        _NullLocation = false;
                        retValue = true;
                    }
                    else
                    {
                        Console.WriteLine(geocode.Status);
                        initializeWithNull();
                    }
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
            updateSearchedLocation();
            return retValue;
        }

        protected void updateSearchedLocation()
        {
            _SearchdDescription = _TaggedDescription;

            if (!string.IsNullOrEmpty(_SearchdDescription) && !string.IsNullOrWhiteSpace(_SearchdDescription))
            {
                _SearchdDescription = _SearchdDescription.Trim().ToLower();
            }
        }

        void initializeWithNull()
        {
            _Latitude = MaxLatitude;
            _Longitude = MaxLongitude;
            if (string.IsNullOrEmpty(_TaggedDescription) && !string.IsNullOrEmpty(_TaggedAddress))
            {
                _TaggedDescription = _TaggedAddress.ToLower();
            }

            else
            {
                if (string.IsNullOrEmpty(_TaggedAddress) && !string.IsNullOrEmpty(_TaggedDescription))
                {
                    _TaggedAddress = _TaggedDescription.ToLower();
                }
            }
            _NullLocation = true;
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
            RetValue._DefaultFlag = true;
            RetValue._NullLocation = false;
            return RetValue;
        }

        public static Location getNullLocation()
        {
            Location RetValue = new Location();
            RetValue._DefaultFlag = true;
            RetValue._NullLocation = true;
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
                string apiKey = ConfigurationManager.AppSettings["googleMapsApiKey"];
                DirectionsRequest directionsRequest = new DirectionsRequest()
                {
                    Origin = first.justLongLatString(),
                    Destination = second.justLongLatString(),
                    TravelMode = travelMode,
                    ApiKey = apiKey
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
            if ((Location24A == null)|| (Location24B == null) || (Location24A._Latitude >= 180) || (Location24B._Latitude > 180) || (Location24A.isNull) || (Location24B.isNull) || (Location24A.isDefault) || (Location24B.isDefault))
            {
                return Worst;
            }
            double R = 6371; // Radius of earth in KM
            double dLat = toRad(Location24A._Latitude - Location24B._Latitude);
            double dLon = toRad(Location24A._Longitude - Location24B._Longitude);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(toRad(Location24A._Latitude)) * Math.Cos(toRad(Location24A._Latitude)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = R * c;
            return d;
        }

        /// <summary>
        /// calculates distance. Result is in KM
        /// </summary>
        /// <param name="Worst"></param>
        /// <param name="Locations"></param>
        /// <returns></returns>
        static public double sumDistance(double Worst = double.MaxValue, params Location [] Locations)
        {
            double retValue = 0;
            for (int i=0, j= 1; j < Locations.Length; i++,j++)
            {
                Location Location24A = Locations[i];
                Location Location24B = Locations[j];
                retValue += calculateDistance(Location24A, Location24B, Worst);
            }
            return retValue;
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

        public void update(Location location)
        {
            this._TaggedAddress = location._TaggedAddress;
            this._TaggedDescription = location._TaggedDescription;
            this._Latitude = location._Latitude;
            this._Longitude = location._Longitude;
            this._NullLocation = location._NullLocation;
            this._DefaultFlag = location._DefaultFlag;
        }

        public Location CreateCopy(string id=null)
        {
            Location this_cpy = new Location();
            this_cpy._TaggedAddress = this._TaggedAddress;
            this_cpy._TaggedDescription = this._TaggedDescription;
            this_cpy._Latitude = this._Latitude;
            this_cpy._Longitude = this._Longitude;
            this_cpy._NullLocation = this._NullLocation;
            this_cpy._Id = id?? this._Id;
            this_cpy.User = this.User;
            return this_cpy;
        }

        /// <summary>
        /// function returns the average location of several gps locations. Essentially, it'lll try to find the center of the various GPS locations. If there is a null or unfounded location, it is not included in the average calculation.
        /// </summary>
        /// <param name="Locations"></param>
        /// <param name="useDefaultLocation"></param>
        /// <returns></returns>
        static public Location AverageGPSLocation(IEnumerable<Location> Locations, bool useDefaultLocation=true)
        {
            Locations = Locations.Where(obj => obj!=null  && !obj.isNull).ToList();
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
                    var latitude = geoCoordinate.Latitude * Math.PI / 180;
                    var longitude = geoCoordinate.Longitude * Math.PI / 180;

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
                    retValue._DefaultFlag = true;
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
            return Address + "||" + _Latitude + "," + _Longitude;
        }

        public string justLongLatString()
        {
            return  _Latitude + "," + _Longitude+"\n";
        }

        public static void updateApiKey(string key)
        {
            _ApiKey = key;
        }

        public static string ApiKey
        {
            get
            {
                return _ApiKey;
            }
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

        public void undoUpdate(Undo undo)
        {
            _UndoLatitude = _Latitude;
            _UndoLongitude = _Longitude;
            _UndoTaggedDescription = _TaggedDescription;
            _UndoTaggedAddress = _TaggedAddress;
            _UndoNullLocation = _NullLocation;
            _UndoDefaultFlag = _DefaultFlag;
            FirstInstantiation = false;
            _UndoId = undo.id;
        }

        public void undo(string undoId)
        {
            if (undoId == this.UndoId)
            {
                Utility.Swap(ref _UndoLatitude, ref _Latitude);
                Utility.Swap(ref _UndoLongitude, ref _Longitude);
                Utility.Swap(ref _UndoTaggedDescription, ref _TaggedDescription);
                Utility.Swap(ref _UndoTaggedAddress, ref _TaggedAddress);
                Utility.Swap(ref _UndoNullLocation, ref _NullLocation);
                Utility.Swap(ref _UndoDefaultFlag, ref _DefaultFlag);
            }
        }

        public void redo(string undoId)
        {
            if (undoId == this.UndoId)
            {
                Utility.Swap(ref _UndoLatitude, ref _Latitude);
                Utility.Swap(ref _UndoLongitude, ref _Longitude);
                Utility.Swap(ref _UndoTaggedDescription, ref _TaggedDescription);
                Utility.Swap(ref _UndoTaggedAddress, ref _TaggedAddress);
                Utility.Swap(ref _UndoNullLocation, ref _NullLocation);
                Utility.Swap(ref _UndoDefaultFlag, ref _DefaultFlag);
            }
        }

        #endregion


        #region Properties
        /// <summary>
        /// NOTE DO NOT FORGET TO SAVE TO SearchdDescription. This is for performace reasons
        /// 
        /// </summary>
        public string Description
        {
            set
            {
                _TaggedDescription = value;
            }
            get
            {
                return _TaggedDescription;
            }
        }

        /// <summary>
        /// Holds description that is to be used for indexing and loacations loook up
        /// </summary>
        [MaxLength(256), Index("UserIdAndDesciption", Order = 1, IsUnique = true)]
        public string SearchdDescription
        {
            protected set
            {
                _SearchdDescription = value;
            }
            get
            {
                string retValue = _SearchdDescription;
                if(string.IsNullOrEmpty(retValue) || string.IsNullOrWhiteSpace(retValue))
                {
                    retValue = _TaggedDescription;
                }
                return retValue;
            }
        }

        public string Address
        {
            set
            {
                _TaggedAddress = value;
            }
            get
            {
                return _TaggedAddress;
            }
        }

        public double Latitude
        {
            set
            {
                _Latitude = value;
            }
            get
            { return _Latitude; }

        }


        public double Longitude
        {
            set
            {
                _Longitude = value;
            }
            get
            {
                return _Longitude;
            }
        }

        public bool isNull
        {
            set
            {
                _NullLocation = value;
            }
            get
            {
                return _NullLocation;
            }
        }

        public bool isDefault
        {
            set
            {
                _DefaultFlag = value;
            }
            get
            {
                return _DefaultFlag;
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

        public virtual bool FirstInstantiation { get; set; } = true;

        public string UndoId
        {
            get
            {
                return _UndoId;
            }
            set
            {
                _UndoId = value;
            }
        }

        public double UndoLatitude
        {
            get {
                return _UndoLatitude;
            }
            set {
                _UndoLatitude = value;
            }
        }
        public double UndoLongitude
        {
            get
            {
                return _UndoLatitude;
            }
            set
            {
                _UndoLatitude = value;
            }
        }
        public string UndoTaggedDescription
        {
            get
            {
                return _UndoTaggedDescription;
            }
            set
            {
                _UndoTaggedDescription = value;
            }
        }
        public string UndoTaggedAddress
        {
            get
            {
                return _UndoTaggedDescription;
            }
            set
            {
                _UndoTaggedDescription = value;
            }
        }
        public bool UndoNullLocation
        {
            get
            {
                return _UndoNullLocation;
            }
            set
            {
                _UndoNullLocation = value;
            }
        }
        public bool UndoDefaultFlag
        {
            get
            {
                return _UndoDefaultFlag;
            }
            set
            {
                _UndoDefaultFlag = value;
            }
        }
        [Index("UserIdAndDesciption", Order = 0, IsUnique = true)]
        public string UserId { get; set; }
        [Required, ForeignKey("UserId")]
        public TilerUser User
        {
            get
            {
                return _User;
            }
            set
            {
                _User = value;
            }
        }
        #endregion
    }
}