using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TilerElements.LocationCacheEntry;

namespace TilerElements
{
    [Serializable]
    public class TravelCache
    {
        SubEventDictionary<int, LocationCacheEntry> _LocationCombo;
        const int cacheEntryLimit = 50;
        [Key]
        public string Id { get; set; }
        [ForeignKey("Id")]
        TilerUser _TilerUser;
        public virtual TilerUser TilerUser_DB
        {
            get
            {
                return _TilerUser;
            }
            set
            {
                _TilerUser = value;
            }
        }

        // id
        // last look up per combination
        // Last complete look up as a matrix
        private void AddLocationCombination(Location firstLocation, Location secondLocation, TimeSpan timeSpanInMs, DateTimeOffset lastUpdate, TravelMedium medium = TravelMedium.driving, double distance = -1)
        {
            double calculatedDistance = distance >= 0 ? distance : Location.calculateDistance(firstLocation, secondLocation);
            LocationCacheEntry entry = new LocationCacheEntry()
            {
                Taiye = secondLocation,
                Kehinde = firstLocation,
                TimeSpanInMs = timeSpanInMs.TotalMilliseconds,
                Medium = medium,
                Distance = calculatedDistance,
                LastLookup = lastUpdate,
                LastUpdate = lastUpdate,
                TravelCache = this
            };
            if (_LocationCombo == null)
            {
                _LocationCombo = new SubEventDictionary<int, LocationCacheEntry>();
            }
            _LocationCombo.Add(entry.GetHashCode(), entry);
        }

        public void AddOrupdateLocationCache(Location firstLocation, Location secondLocation, TimeSpan timeSpanInMs, DateTimeOffset lastUpdate, TravelMedium medium = TravelMedium.driving, double distance = -1)
        {
            double calculatedDistance = distance >= 0 ? distance : Location.calculateDistance(firstLocation, secondLocation);
            LocationCacheEntry entry;
            if (_LocationCombo == null)
            {
                _LocationCombo = new SubEventDictionary<int, LocationCacheEntry>();
            }

            string key = LocationCacheEntry.getHashCode(firstLocation, secondLocation, medium.ToString(), this.Id).ToString();
            if (_LocationCombo.ContainsKey(key))
            {
                entry = _LocationCombo[key];
                entry.LastUpdate = lastUpdate;
                entry.TimeSpanInMs = timeSpanInMs.TotalMilliseconds;
            }
            else
            {
                AddLocationCombination(firstLocation, secondLocation, timeSpanInMs, lastUpdate, medium, distance);
            }
        }

        public LocationCacheEntry getLocation(Location first, Location second, DateTimeOffset TimeOfLookup, TravelMedium medium = TravelMedium.driving)
        {
            if (!TimeOfLookup.isBeginningOfTime())
            {
                LocationCacheEntry entry = new LocationCacheEntry()
                {
                    Taiye = second,
                    Kehinde = first,
                    Medium = medium,
                    TravelCache = this
                };
                string hashCode = entry.GetHashCode().ToString();
                if (_LocationCombo != null && _LocationCombo.ContainsKey(hashCode))
                {
                    var retValue = _LocationCombo[hashCode];
                    retValue.LastLookup = TimeOfLookup;
                    return retValue;
                }
                return null;
            } else
            {
                throw new CustomErrors(CustomErrors.Errors.BeginningOfTimeError);
            }

        }

        [NotMapped]
        public virtual ICollection<LocationCacheEntry> LocationCombo
        {
            get
            {
                return _LocationCombo;
            }
        }



        public virtual ICollection<LocationCacheEntry> LocationCombo_DB
        {
            get
            {
                var retValue = _LocationCombo ?? (_LocationCombo = new SubEventDictionary<int, LocationCacheEntry>());
                _LocationCombo = new SubEventDictionary<int, LocationCacheEntry>(retValue.Values.OrderByDescending(o => o.LastLookup).Take(cacheEntryLimit));


                return _LocationCombo;
            }

            set
            {
                _LocationCombo = new SubEventDictionary<int, LocationCacheEntry>();
                if (value != null)
                {
                    this._LocationCombo = new SubEventDictionary<int, LocationCacheEntry>(value);
                }
            }
        }

    }
}
