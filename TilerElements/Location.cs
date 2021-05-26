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
using GoogleMapsApi;
using GoogleMapsApi.Entities.Geocoding.Request;
using GoogleMapsApi.Entities.Geocoding.Response;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;
using GoogleMapsApi.Entities.Places.Request;
using GoogleMapsApi.Entities.Places.Response;
using GoogleMapsApi.Entities.PlacesFind.Request;
using GoogleMapsApi.Entities.PlacesFind.Response;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Places.Search.Find.Request;
using GoogleApi.Entities.Places.Search.Find.Request.Enums;
using GoogleApi.Exceptions;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using GoogleApi;
using GoogleApi.Entities.Places.Search.Find.Response;
using Newtonsoft.Json;
using GoogleMaps = GoogleMapsApi.GoogleMaps;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace TilerElements
{
    public class Location: IUndoable, IHasId
    {
        public static int LastLocationId = 1;
        public static double MaxLongitude = 181;
        public static double MaxLatitude = 91;
        public static string _ApiKey;
        public enum ThirdPartyMapSource
        {
            google,
            none
        }
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
        protected string _LookupString = "";
        protected bool _LocationIsVerified = false; // This is an address that has been confirmed from the user, either through the ui or some other confirmation
        protected string _UndoId = "";
        protected string _ThirdPartyId = "";
        protected ThirdPartyMapSource _ThirdPartySource;
        protected RestrictionProfile _ProfileOfRestriction = null;
        [NonSerialized]
        protected TilerUser _User;
        protected LocationValidation _LocationValidation;
        protected TilerEvent _Event;


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
        protected bool _UndoLocationIsVerified = false;
        protected bool _UndoNullLocation = true;
        protected bool _UndoDefaultFlag = false;
        protected string _UndoThirdPartyId = "";
        protected string _UndoThirdPartySource = "";
        protected string _UndoSearchedDescription = "";
        protected string _UndoLookupString = "";
        protected string _UndoLocationValidation = "";
        #endregion
        protected string _Id = Guid.NewGuid().ToString();
#region Constructor
        public Location()
        {
            _Latitude = defaultXValue;
            _Longitude = defaultYValue;
            _NullLocation = true;
        }

        public Location(double MyxValue, double MyyValue, string Id = "")
        {
            _Latitude = MyxValue;
            _Longitude = MyyValue;
            _NullLocation = false;
            _ThirdPartySource = ThirdPartyMapSource.none;
            if (!string.IsNullOrEmpty(Id))
            {
                _Id = Id;
            }
        }

        public Location(double latitutde, double longitude, string AddressEntry, string AddressDescription, bool isNull, bool isDefaultFlag, string ID = "")
        {
            _Latitude = latitutde;
            _Longitude = longitude;
            _TaggedAddress = AddressEntry;
            _TaggedDescription = AddressDescription;
            updateSearchedLocation();
            _NullLocation = isNull;
            if (string.IsNullOrEmpty(_TaggedDescription) || string.IsNullOrWhiteSpace(_TaggedDescription) && (!(string.IsNullOrEmpty(_TaggedAddress) || string.IsNullOrWhiteSpace(_TaggedAddress))))
            {
                _TaggedDescription = _TaggedAddress;
            }
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
            _NullLocation = true;
            if (string.IsNullOrEmpty(Address) || string.IsNullOrWhiteSpace(Address))
            {
                Address = "";
            }
            else
            {
                _NullLocation = false;
            }

            if (string.IsNullOrEmpty(tag) || string.IsNullOrWhiteSpace(tag))
            {
                tag = "";
            }
            else
            {
                _NullLocation = false;
            }



            Address = Address.Trim();
            _TaggedAddress = Address;
            _TaggedDescription = tag;
            if (string.IsNullOrEmpty(tag) || string.IsNullOrWhiteSpace(tag)&&(!(string.IsNullOrEmpty(_TaggedAddress) || string.IsNullOrWhiteSpace(_TaggedAddress))))
            {
                _TaggedDescription = _TaggedAddress;
            }

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
        /// this takes an ambihuous location name like "best buy" and tries to look up best buy relative to anchorLocation. It then returns the location it found
        /// </summary>
        /// <param name="anchorLocation"></param>
        /// <returns></returns>

        internal Location validate(Location anchorLocation, DateTimeOffset currentTime)
        {
            Location retValue = null;
            _TaggedAddress = _TaggedAddress.Trim();
            try
            {
                if (anchorLocation != null && !string.IsNullOrEmpty(_TaggedAddress) && !string.IsNullOrWhiteSpace(_TaggedAddress) && !anchorLocation.isDefault && !anchorLocation.isNull)
                {
                    if(_LocationValidation == null)
                    {
                        _LocationValidation = new LocationValidation();
                    }
                    _LocationValidation.instantiate();
                    Stopwatch googleRemoteWatch = new Stopwatch();
                    googleRemoteWatch.Start();
                    double distance = Location.calculateDistance(anchorLocation, _LocationValidation.getAverageLocation, -1);
                    double distaceVariance = Math.Abs(_LocationValidation.AverageDistanceFromAverageLocation - distance);
                    double varianceLocationValidation = _LocationValidation.AverageVariance;
                    double doubleVariance = varianceLocationValidation * 2;
                    double halfVariance = varianceLocationValidation / 2;
                    if (distaceVariance >= doubleVariance || distaceVariance <= halfVariance|| _LocationValidation.locations.Count <= 2)
                    {
                        var googleLocation = new GoogleApi.Entities.Common.Location(anchorLocation.Latitude, anchorLocation.Longitude);
                        var placesFindSearchRequest = new PlacesFindSearchRequest()
                        {
                            Type = GoogleApi.Entities.Places.Search.Find.Request.Enums.InputType.TextQuery,
                            Fields = FieldTypes.Basic,
                            Key = Location.ApiKey,
                            Input = _TaggedAddress,
                            Location = googleLocation,
                            Radius = 5000
                        };





                        var response = GooglePlaces.FindSearch.Query(placesFindSearchRequest);
                        GoogleApi.Entities.Places.Search.Find.Response.Candidate candidate = null;

                        Debug.WriteLine("Google remote request to -> " + _TaggedAddress);

                        if (response.Status == GoogleApi.Entities.Common.Enums.Status.Ok)
                        {
                            var jsonData = response.RawJson;
                            candidate = response.Candidates.FirstOrDefault();
                            RestrictionProfile restrictionProfile = null;

                            var placesDetailRequest = new GoogleApi.Entities.Places.Details.Request.PlacesDetailsRequest()
                            {
                                Fields = GoogleApi.Entities.Places.Details.Request.Enums.FieldTypes.Basic| GoogleApi.Entities.Places.Details.Request.Enums.FieldTypes.Utc_Offset|GoogleApi.Entities.Places.Details.Request.Enums.FieldTypes.Opening_Hours| GoogleApi.Entities.Places.Details.Request.Enums.FieldTypes.Formatted_Address,
                                Key = Location.ApiKey,
                                PlaceId = candidate.PlaceId,
                            };

                            var detailResponse = GooglePlaces.Details.Query(placesDetailRequest);
                            if (detailResponse.Status == GoogleApi.Entities.Common.Enums.Status.Ok)
                            {
                                //https://maps.googleapis.com/maps/api/place/details/json?place_id=ChIJsalwnJyMa4cRrt3GDP5iygM&fields=name,rating,formatted_phone_number,opening_hours,utc_offset&key=AIzaSyDXrtMxPbt6Dqlllpm77AQ47vcCFxZ4oUU
                                //TODO Make request as above directly to google maps api using rest request withou using the GooglePlaces.Details.Query. This is because we can't add muliple parameters
                                var detailJsonData = detailResponse.RawJson;
                                var detailJson = JObject.Parse(detailJsonData);
                                var resultProperty = detailJson.Property("result");
                                if(resultProperty != null)
                                {
                                    JObject result = resultProperty.Value as JObject;
                                    var opening_hoursProperties = result.Property("opening_hours");
                                    var offsetHoursProperties = result.Property("utc_offset");
                                    double offsetMinutes = 0;
                                    if (offsetHoursProperties != null)
                                    {
                                        offsetMinutes = Convert.ToDouble(offsetHoursProperties.Value);
                                    }
                                    if (opening_hoursProperties!=null)
                                    {
                                        JObject opening_hours = opening_hoursProperties.Value as JObject;
                                        var periodProperties = opening_hours.Property("periods");
                                        if(periodProperties!=null)
                                        {
                                            JArray periods = periodProperties.Value as JArray;
                                            if(periods.Count > 0)
                                            {
                                                Dictionary<int, List<JObject>> dayToTimeInfo = new Dictionary<int, List<JObject>>();
                                                foreach(var dayTimeInfo in periods)
                                                {
                                                    JObject dayData = dayTimeInfo as JObject;
                                                    var openProperty = dayData.Property("open");
                                                    if(openProperty!=null)
                                                    {
                                                        JObject openDayData = openProperty.Value as JObject;
                                                        if(openDayData != null)
                                                        {
                                                            var dayIndexProperty = openDayData.Property("day");
                                                            if(dayIndexProperty!=null)
                                                            {
                                                                int dayIndex = (int)dayIndexProperty.Value;
                                                                List<JObject> timeInfoList = new List<JObject>();
                                                                if(dayToTimeInfo.ContainsKey(dayIndex))
                                                                {
                                                                    timeInfoList = dayToTimeInfo[dayIndex];
                                                                } else
                                                                {
                                                                    dayToTimeInfo.Add(dayIndex, timeInfoList);
                                                                }

                                                                timeInfoList.Add(dayData);
                                                            }
                                                        }
                                                    }
                                                }
                                                List<DayOfWeek> daysOfWeek = new List<DayOfWeek>();
                                                List<RestrictionTimeLine> RestrictionTimeLines = new List<RestrictionTimeLine>();
                                                foreach (var kvp in dayToTimeInfo)
                                                {
                                                    DayOfWeek dayOfWeek = (DayOfWeek)kvp.Key;
                                                    JObject openFirst = kvp.Value.OrderBy(o => o["open"]["time"]).FirstOrDefault();
                                                    JObject closeLast= kvp.Value.OrderByDescending(o => o["close"]["time"]).FirstOrDefault();
                                                    string militaryOpenTimeString = (string)openFirst["open"]["time"];
                                                    string militaryCloseTimeString = (string)closeLast["close"]["time"];
                                                    double militaryOpenHours = Convert.ToDouble(militaryOpenTimeString);
                                                    double militaryCloseHours = Convert.ToDouble(militaryCloseTimeString);
                                                    var openTupleTimeData = RestrictionProfile.miltaryTimeHoursToDayOfWeek(dayOfWeek, militaryOpenHours, offsetMinutes);
                                                    var closeTupleTimeData = RestrictionProfile.miltaryTimeHoursToDayOfWeek(dayOfWeek, militaryCloseHours, offsetMinutes);
                                                    RestrictionTimeLine restrictionTimeLine = new RestrictionTimeLine(openTupleTimeData.Item2, closeTupleTimeData.Item2);
                                                    daysOfWeek.Add(openTupleTimeData.Item1);
                                                    RestrictionTimeLines.Add(restrictionTimeLine);
                                                }
                                                if(daysOfWeek.Count > 0 && daysOfWeek.Count == RestrictionTimeLines.Count)
                                                {
                                                    restrictionProfile = new RestrictionProfile(daysOfWeek, RestrictionTimeLines);
                                                }
                                            }
                                        }
                                    }
                                }
                                this.setRestrictionProfile(restrictionProfile);
                            }

                            googleRemoteWatch.Stop();
                            Debug.WriteLine("Google remote request took " + googleRemoteWatch.Elapsed.ToString());

                            retValue = new LocationJson();
                            if (candidate != null)
                            {
                                var result = candidate;
                                string address = result.FormattedAddress.Trim().ToLower();
                                bool useThis = IsLookUpAddressSameAsGoogleMapsAddress(_TaggedAddress, address);
                                retValue = useThis ? this : retValue;
                                retValue._TaggedAddress = address;
                                if (string.IsNullOrEmpty(retValue._TaggedDescription))
                                {
                                    retValue._TaggedDescription = retValue._TaggedAddress;
                                }
                                retValue._Latitude = Convert.ToDouble(result.Geometry.Location.Latitude);
                                retValue._Longitude = Convert.ToDouble(result.Geometry.Location.Longitude);
                                retValue._LookupString = this._LookupString;
                                retValue.SearchdDescription = this.SearchdDescription;
                                retValue.UserId = this.UserId??this.User?.Id;
                                retValue._NullLocation = false;
                                retValue._DefaultFlag = false;
                                retValue._ThirdPartyId = result.PlaceId;
                                retValue._LocationIsVerified = true;
                                retValue._ThirdPartySource = ThirdPartyMapSource.google;
                                
                                this._NullLocation = false;
                                this._DefaultFlag = false;
                                retValue.updateSearchedLocation();
                                LocationJson retValueJson = (retValue as LocationJson);
                                if (retValueJson != null)
                                {
                                    retValueJson.LastUsed = currentTime;
                                    retValueJson.setRestrictionProfile(restrictionProfile);
                                }
                                
                                if (!useThis)
                                {
                                    retValue._Id = result.PlaceId;
                                    retValue.setRestrictionProfile(restrictionProfile);
                                    _LocationValidation.addLocation(retValue as LocationJson, currentTime);
                                }
                                
                            }
                            else
                            {
                                retValue._NullLocation = true;
                            }
                        }
                        else
                        {
                            googleRemoteWatch.Stop();
                            Console.WriteLine(response.Status);
                            retValue = new Location();
                            retValue.initializeWithNull();
                        }
                    } else
                    {
                        retValue = _LocationValidation.getClosestLocation(anchorLocation, currentTime);
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

        public bool IsLookUpAddressSameAsGoogleMapsAddress(string inputAddress, string lookUpAddress)
        {
            return inputAddress.ToLower() == lookUpAddress.ToLower();
            string inputAddressNoPunctuation = new string(inputAddress.Where(c => !char.IsPunctuation(c) && !char.IsWhiteSpace(c)).ToArray());
            string lookUpAddressNoPunctuation = new string(lookUpAddress.Where(c => !char.IsPunctuation(c) && !char.IsWhiteSpace(c)).ToArray());

            string largerString = lookUpAddressNoPunctuation.Length > inputAddressNoPunctuation.Length ? lookUpAddressNoPunctuation : inputAddressNoPunctuation;
            int diffCount = Math.Abs(inputAddressNoPunctuation.Length - largerString.Length);
            if (diffCount == 0)
            {
                diffCount = Math.Abs(lookUpAddressNoPunctuation.Length - largerString.Length);
            }

            double percentageDiff = ((double)diffCount / (double)largerString.Length) *100;
            bool retValue = percentageDiff < 45;
            return retValue;
        }

        /// <summary>
        /// function tries to verify that the address provide exists in external service
        /// </summary>
        public virtual bool verify()
        {
            bool retValue = false;
            _TaggedAddress = _TaggedAddress.Trim();
            try
            {

                if (!String.IsNullOrEmpty(_TaggedAddress) && Location.ApiKey.isNot_NullEmptyOrWhiteSpace())
                {
                    GeocodingRequest request = new GeocodingRequest();
                    request.Address = _TaggedAddress;
                    request.ApiKey = Location.ApiKey;

                    var geocodingEngine = GoogleMaps.Geocode;
                    //Task<GeocodingResponse> geoCodeTask = geocodingEngine.QueryAsync(request);
                    //geoCodeTask.Wait();
                    //GeocodingResponse geocode = geoCodeTask.Result;
                    GeocodingResponse geocode = geocodingEngine.Query(request);
                    if (geocode.Status == GoogleMapsApi.Entities.Geocoding.Response.Status.OK)
                    {
                        if (string.IsNullOrEmpty(_TaggedDescription))
                        {
                            _TaggedDescription = _TaggedAddress;
                        }
                        var result = geocode.Results.First();
                        _TaggedAddress = result.FormattedAddress.ToLower();
                        _Latitude = Convert.ToDouble(result.Geometry.Location.Latitude);
                        _Longitude = Convert.ToDouble(result.Geometry.Location.Longitude);
                        _ThirdPartyId = result.PlaceId;
                        _DefaultFlag = false;
                        _NullLocation = false;
                        _LocationIsVerified = true;
                        _ThirdPartySource = ThirdPartyMapSource.google;
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

        public Location getLocationThroughValidation(string locationId, DateTimeOffset currentTime)
        {
            if(!string.IsNullOrEmpty(locationId) && !string.IsNullOrWhiteSpace(locationId) && _LocationValidation!=null)
            {
                return _LocationValidation.getLocation(locationId, currentTime);
            }
            return null;
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
            if(!first.isNull && !second.isNull && !first.isDefault && !second.isDefault)
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
                    DirectionsResponse directions = GoogleMapsApi.GoogleMaps.Directions.Query(directionsRequest);
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

        /// <summary>
        /// Function updates the current location with the new variable "location"
        /// </summary>
        /// <param name="location">New location to be copied from</param>
        /// <param name="resetLocationValidation">reset the Location validation, this is often used in the scenario where the location address is ambiguous this defaults to true</param>
        /// <param name="overrideLocationValidation">This copies over the location validation from the variable the passed location. This defaults to false</param>
        public void update(Location location, bool resetLocationValidation = true, bool overrideLocationValidation = false)
        {
            this._TaggedAddress = location._TaggedAddress;
            this._TaggedDescription = location._TaggedDescription;
            this._Latitude = location._Latitude;
            this._Longitude = location._Longitude;
            this._NullLocation = location._NullLocation;
            this._DefaultFlag = location._DefaultFlag;
            this._LocationIsVerified = location._LocationIsVerified;
            if (resetLocationValidation)
            {
                this._LocationValidation = new LocationValidation();
            }
            if(overrideLocationValidation)
            {
                this._LocationValidation = location._LocationValidation;
            }
            
            
            this._SearchdDescription = location._SearchdDescription;
            this._LookupString = location._LookupString;
            this._ThirdPartyId = location.ThirdPartyId;
            this._ThirdPartySource = location._ThirdPartySource;
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
            this_cpy._LocationIsVerified = this._LocationIsVerified;
            this_cpy._LocationValidation = this._LocationValidation;
            this_cpy._SearchdDescription = this._SearchdDescription;
            this_cpy._LookupString = this._LookupString;
            this_cpy._ThirdPartySource = this._ThirdPartySource;
            this_cpy._ThirdPartyId = this_cpy.ThirdPartyId;
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

                retValue = new Location(centralLatitude * 180 / Math.PI, centralLongitude * 180 / Math.PI);
                retValue.isDefault = false;
                retValue.isNull = false;

                return retValue;
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

        public RestrictionProfile RestrictionProfile
        {
            get
            {
                return _ProfileOfRestriction;
            }
        }

        public bool isRestricted
        {
            get 
            { 
                return this.RestrictionProfile != null;
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

        public void purgeLocationCache(DateTimeOffset currentTime)
        {
            if (_LocationValidation != null)
            {
                _LocationValidation.purge(currentTime);
            }
        }

        public void setRestrictionProfile(RestrictionProfile restrictionProfile)
        {
            this._ProfileOfRestriction = restrictionProfile;
        }

        public void undoUpdate(Undo undo)
        {
            _UndoLatitude = _Latitude;
            _UndoLongitude = _Longitude;
            _UndoTaggedDescription = _TaggedDescription;
            _UndoTaggedAddress = _TaggedAddress;
            _UndoNullLocation = _NullLocation;
            _UndoDefaultFlag = _DefaultFlag;
            _UndoThirdPartyId = _ThirdPartyId;
            _UndoThirdPartySource = _ThirdPartySource.ToString().ToLower();
            _UndoSearchedDescription = _SearchdDescription;
            _UndoLookupString = _LookupString;
            _UndoLocationValidation = LocationValidation_DB;

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
                Utility.Swap(ref _UndoLocationIsVerified, ref _LocationIsVerified);
                Utility.Swap(ref _UndoLookupString, ref _LookupString);
                Utility.Swap(ref _UndoThirdPartyId, ref _ThirdPartyId);
                Utility.Swap(ref _UndoSearchedDescription, ref _SearchdDescription);
                Utility.Swap(ref _UndoLookupString, ref _LookupString);
                string _LocationValidation = LocationValidation_DB;
                Utility.Swap(ref _UndoLocationValidation, ref _LocationValidation);
                LocationValidation_DB = _LocationValidation;
                string _thidParty = _ThirdPartySource.ToString().ToLower();
                Utility.Swap(ref _UndoThirdPartySource, ref _thidParty);
                _ThirdPartySource = Utility.ParseEnum<ThirdPartyMapSource>(_thidParty);
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
                Utility.Swap(ref _UndoLocationIsVerified, ref _LocationIsVerified);
                Utility.Swap(ref _UndoLookupString, ref _LookupString);
                Utility.Swap(ref _UndoLookupString, ref _LookupString);
                Utility.Swap(ref _UndoThirdPartyId, ref _ThirdPartyId);
                Utility.Swap(ref _UndoSearchedDescription, ref _SearchdDescription);
                Utility.Swap(ref _UndoLookupString, ref _LookupString);
                string _LocationValidation = LocationValidation_DB;
                Utility.Swap(ref _UndoLocationValidation, ref _LocationValidation);
                LocationValidation_DB = _LocationValidation;
                string _thidParty = _ThirdPartySource.ToString().ToLower();
                Utility.Swap(ref _UndoThirdPartySource, ref _thidParty);
                _ThirdPartySource = Utility.ParseEnum<ThirdPartyMapSource>(_thidParty);
            }
        }

        #endregion


        #region Properties

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

        public string ThirdPartyId
        {
            get
            {
                return _ThirdPartyId;
            }
        }

        public LocationValidation LocationValidation
        {
            get
            {
                return this._LocationValidation;
            }
        }


        public string LocationValidation_DB
        {
            get
            {
                var retValue = JsonConvert.SerializeObject(_LocationValidation);
                return retValue;
            }
            set
            {
                if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                {
                    _LocationValidation = new LocationValidation();
                } else
                {
                    _LocationValidation = JsonConvert.DeserializeObject<LocationValidation>(value);
                }
                
            }
        }

        public string ThirdPartySource
        {
            get
            {
                return _ThirdPartySource.ToString().ToLower();
            }
            set
            {
                if (string.IsNullOrEmpty( value)  || string.IsNullOrWhiteSpace(value))
                {
                    _ThirdPartySource = ThirdPartyMapSource.none;
                }
                else
                {
                    _ThirdPartySource = Utility.ParseEnum<ThirdPartyMapSource>(value);
                }

                
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

        public virtual string LookupString
        {
            get
            {
                return _LookupString;
            }
            set
            {
                _LookupString = value;
            }
        }

        public virtual bool IsVerified
        {
            get
            {
                return _LocationIsVerified;
            }
            set
            {
                _LocationIsVerified = value;
            }
        }

        public virtual bool IsAmbiguous
        {
            get
            {
                return !(IsVerified && (!isDefault || !isNull));
            }
        }

        public virtual string ThirdPartyId_DB
        {
            get
            {
                return _ThirdPartyId;
            }
            set
            {
                _ThirdPartyId = value;
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

        public bool isNotNullAndNotDefault
        {
            get
            {
                return !this.isNull && !this.isDefault;
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

        public string RestrictionProfile_DB
        {
            get
            {
                string retValue = "";
                if (_ProfileOfRestriction != null)
                {
                    JObject retJObject = new JObject();
                    retJObject.Add("DaySelection_DbString", _ProfileOfRestriction.DaySelection_DbString);
                    retJObject.Add("NoNull_DaySelections_DbString", _ProfileOfRestriction.NoNull_DaySelections_DbString);
                    retValue = retJObject.ToString();
                }
                return retValue;
            }

            set
            {
                if(value.isNot_NullEmptyOrWhiteSpace())
                {
                    RestrictionProfile serializedObject = JsonConvert.DeserializeObject<RestrictionProfile>(value);
                    serializedObject.InitializeOverLappingDictionary();
                    _ProfileOfRestriction = serializedObject;
                }
                else
                {
                    _ProfileOfRestriction = null;
                }
                
            }
        }
        [Index("UserIdAndDesciption", Order = 0, IsUnique = true)]
        public string UserId { get; set; }
        
        [Required, ForeignKey("UserId")]
        public virtual TilerUser User
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