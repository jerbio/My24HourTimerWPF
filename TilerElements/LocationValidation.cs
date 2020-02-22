using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    [Serializable]
    public class LocationValidation
    {
        const int maxLocationCount = 5;
        public double AverageLongitude { get; set; } = 0;
        public double AverageLatitude { get; set; } = 0;
        public double AverageDistanceFromAverageLocation { get; set; } = 0;
        public double AverageVariance { get; set; } = 0;
        public static readonly TimeSpan CacheExpirationTimeSpan = TimeSpan.FromDays(5);
        [NonSerialized]
        bool isInstantiated = false;
        [NonSerialized]
        Dictionary<string, LocationJson> dictionary_location;

        public ICollection<LocationJson> locations { get; set; }
        
        public void instantiate()
        {
            if(!isInstantiated)
            {
                dictionary_location = new Dictionary<string, LocationJson>();
                if (locations == null)
                {
                    locations = new List<LocationJson>();
                }
                if (locations.Count > 0)
                {
                    updateAverageLocation();
                    dictionary_location = locations.ToDictionary(location => location.Id, location => location);
                }
                isInstantiated = true;
            }
        }

        internal void addLocation (LocationJson location, DateTimeOffset currentTime)
        {
            if(!dictionary_location.ContainsKey(location.Id))
            {
                dictionary_location.Add(location.Id, location);
                locations.Add(location);
                updateAverageLocation();
                updateAverageLocationDistance();
            }

            if(locations.Count > maxLocationCount)
            {
                while(locations.Count > maxLocationCount)
                {
                    removeHighestVaryingLocation(currentTime);
                }
            }
        }

        void removeHighestVaryingLocation(DateTimeOffset currentTime)
        {
            if(locations.Count > 0) {
                Location avgLocation = getAverageLocation;
                LocationJson leastLiekelyLocation = null;
                double maxDist = 0;
                List<IList<double>> multiArgs = new List<IList<double>>();
                var locationList = locations.ToList();
                foreach (LocationJson loc in locationList)
                {
                    double distance = Location.calculateDistance(avgLocation, loc, -1);
                    double timeDiff = (currentTime - loc.LastUsed).TotalMinutes;

                    multiArgs.Add(new List<double>() { distance, timeDiff });
                }

                var multiResult = Utility.multiDimensionCalculationNormalize(multiArgs);
                int leastLikelyIndex = multiResult.MaxIndex();
                leastLiekelyLocation = locationList[leastLikelyIndex];

                if (leastLiekelyLocation == null)
                {
                    leastLiekelyLocation = locations.OrderBy(o=>o.LastUsed).First();
                }

                locations.Remove(leastLiekelyLocation);
                dictionary_location.Remove(leastLiekelyLocation.Id);
            }
        }

        internal void updateAverageLocation()
        {
            Location avgLocation = Location.AverageGPSLocation(locations);
            AverageLatitude = avgLocation.Latitude;
            AverageLongitude = avgLocation.Longitude;
        }

        internal void updateAverageLocationDistance()
        {
            Location location = new Location(AverageLatitude, AverageLongitude);
            if(locations.Count > 0)
            {
                AverageDistanceFromAverageLocation = locations.Select(loc => Location.calculateDistance(location, loc, -1)).Where(dist => dist >= 0).Average();
                AverageVariance = locations.Select(loc => Location.calculateDistance(location, loc, -1)).Where(dist => dist >= 0).Select(dist => Math.Abs(dist - AverageDistanceFromAverageLocation)).Average();
            } else
            {
                AverageDistanceFromAverageLocation = 0;
                AverageVariance = 0;
            }
            
        }

        internal Location getClosestLocation(Location location, DateTimeOffset currentTime)
        {
            double lowestDistance = double.MaxValue;
            LocationJson retValue = null;
            if(locations.Count > 0)
            {
                retValue = locations.First();
                foreach (LocationJson loc in locations)
                {
                    double distance = Location.calculateDistance(location, loc, -1);
                    if (distance > 0)
                    {
                        if (distance < lowestDistance)
                        {
                            retValue = loc;
                            lowestDistance = distance;
                        }
                    }
                }
            }
            retValue.LastUsed = currentTime;
            return retValue;
        }

        internal Location getAverageLocation
        {
            get
            {
                return new Location(AverageLatitude, AverageLongitude);
            }
        }

        /// <summary>
        /// Validation has at least one location already added
        /// </summary>
        public bool isInitialized
        {
            get
            {
                return locations != null ? locations.Count > 0 : false;
            }
        }

        /// <summary>
        /// Gets the the location by Id, if the id isn't found then it returns null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal Location getLocation(string id, DateTimeOffset currentTime)
        {
            if (!this.isInstantiated)
            {
                this.instantiate();
            }
            LocationJson retValue;
            if (!string.IsNullOrEmpty(id))
            {
                if (dictionary_location.ContainsKey(id))
                {
                    retValue = dictionary_location[id];
                    retValue.LastUsed = currentTime;
                    return retValue;
                }
            }
            retValue = null;
            return retValue;
        }

        public void purge(DateTimeOffset currentTime)
        {
            if (dictionary_location != null)
            {
                bool reevaluateAverages = false;
                var locations = dictionary_location.Values.ToList();
                List<LocationJson> removedLocations = new List<LocationJson>();
                foreach (var location in locations)
                {
                    TimeSpan lasUsedSpan = currentTime - location.LastUsed;
                    if (lasUsedSpan >= CacheExpirationTimeSpan)
                    {
                        removedLocations.Add(location);
                        dictionary_location.Remove(location.Id);
                        reevaluateAverages = true;
                    }
                }

                if(reevaluateAverages)
                {
                    this.locations = dictionary_location.Values.ToList();
                    updateAverageLocation();
                    updateAverageLocationDistance();
                }
            }
        }
    }
}
