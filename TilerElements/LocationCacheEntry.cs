using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    [Serializable]
    public class LocationCacheEntry:IHasId
    {
        public enum TravelMedium { walk, driving, flying, timeTravel }
        [NonSerialized]
        protected Location _Taiye;
        [NonSerialized]
        protected Location _Kehinde;
        protected string _KehindeId;
        protected string _TaiyeId;
        protected string _id = "";
        protected TravelMedium _TravelMedium = TravelMedium.driving;
        public virtual DateTimeOffset LastLookup { get; set; }
        public virtual DateTimeOffset LastUpdate { get; set; }
        public virtual double TimeSpanInMs { get; set; }
        public virtual double Distance { get; set; }

        public virtual TravelMedium Medium
        {
            get
            {
                return _TravelMedium;
            }
            set
            {
                _TravelMedium = value;
            }
        }

        public string Medium_DB { get {
                return _TravelMedium.ToString().ToLower();
            }
            set {
                if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value))
                {
                    _TravelMedium = Utility.ParseEnum<TravelMedium>(value);
                } else
                {
                    _TravelMedium = TravelMedium.driving;
                }

            }
        }

        public virtual string Id {
            get
            {
                if(string.IsNullOrEmpty(_id) || string.IsNullOrWhiteSpace(_id))
                {
                    _id = this.GetHashCode().ToString();
                }

                return _id;
            }
            set
            {
                _id = value;
            }
        }

        [NotMapped]
        public Location Taiye {
            get
            {
                return _Taiye;
            } set {
                _Taiye = value;
                if (value != null)
                {
                    _TaiyeId = _Taiye?.ThirdPartyId ?? _Taiye.Id;
                }
            }
        }
        [NotMapped]
        public Location Kehinde {
            get
            {
                return _Kehinde;
            }
            set
            {
                _Kehinde = value;
                if(value!= null)
                {
                    _KehindeId = _Kehinde?.ThirdPartyId ?? _Kehinde.Id;
                }
                
            }
        }

        public TravelCache TravelCache { get;set; }

        public string TaiyeId
        {
            get
            {
                return _TaiyeId;
            }
            set
            {
                _TaiyeId = value;
            }
        }
        public string KehindeId
        {
            get
            {
                return _KehindeId;
            }
            set
            {
                _KehindeId = value;
            }
        }

        public virtual TimeSpan TimeSapn
        {
            get
            {
                TimeSpan retValue = TimeSpan.FromMilliseconds(this.TimeSpanInMs);
                return retValue;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return LocationCacheEntry.getHashCode(TaiyeId, KehindeId, Medium_DB, this.TravelCache.Id);
            }
        }

        private static int getHashCode (string TaiyeId, string KehindeId, string medium, string travelCahceId)
        {
            unchecked
            {
                int cacheId = travelCahceId.GetHashCode();
                int taiyeCode = TaiyeId.GetHashCode();
                int kehindeCode = KehindeId.GetHashCode();

                int smallerCode = 0, largerCode = 0;
                if (taiyeCode < kehindeCode)
                {
                    smallerCode = taiyeCode;
                    largerCode = kehindeCode;
                }
                else
                {
                    smallerCode = kehindeCode;
                    largerCode = taiyeCode;
                }

                medium = medium.ToLower();
                int hash = 17;
                hash = hash * 23 + smallerCode.GetHashCode();
                hash = hash * 23 + largerCode.GetHashCode();
                hash = hash * 23 + medium.GetHashCode();
                hash = hash * 23 + cacheId.GetHashCode();
                return hash;
            }
        }

        public static int getHashCode(Location firstLocation, Location secondLocation, string medium, string travelCahceId) {
            string firstId = firstLocation.ThirdPartyId ?? firstLocation.Id;
            string secondId = secondLocation.ThirdPartyId ?? secondLocation.Id;

            return LocationCacheEntry.getHashCode(firstId, secondId, medium, travelCahceId);
        }

    }
}
