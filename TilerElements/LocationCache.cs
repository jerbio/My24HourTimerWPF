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
    public class LocationCache
    {
        Dictionary<int, LocationCacheEntry> _LocationCombo;
        [Key]
        public string TilerUserId { get; set; }
        [ForeignKey("TilerUserId")]
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
        public void AddLocationCombination(Location firstLocation, Location secondLocation, double timeSpanInMs, TravelMedium medium = TravelMedium.driving, double distance = -1)
        {
            double calculatedDistance = distance >= 0 ? distance : Location.calculateDistance(firstLocation, secondLocation);
            LocationCacheEntry entry = new LocationCacheEntry()
            {
                Taiye = secondLocation,
                Kehinde = firstLocation,
                TimeSpanInMs = timeSpanInMs,
                Medium = medium,
                Distance = calculatedDistance
            };
            if (_LocationCombo == null)
            {
                _LocationCombo = new Dictionary<int, LocationCacheEntry>();
            }
            _LocationCombo.Add(entry.GetHashCode(), entry);
        }

        public LocationCacheEntry getLocation(Location first, Location second, DateTimeOffset TimeOfLookup, TravelMedium medium = TravelMedium.driving)
        {
            if (TimeOfLookup.isBeginningOfTime())
            {
                LocationCacheEntry entry = new LocationCacheEntry()
                {
                    Taiye = second,
                    Kehinde = first,
                    Medium = medium
                };
                int hashCode = entry.GetHashCode();
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

        public Dictionary<int, LocationCacheEntry> LocationCombo
        {
            get {
                return _LocationCombo;
            }

            set
            {
                _LocationCombo = value;
            }
        }


    }
}
