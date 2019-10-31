using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    [Serializable]
    public class LocationCacheEntry
    {
        public enum TravelMedium { walk, driving, flying, timeTravel}
        protected Location _Taiye;
        protected Location _Kehinde;
        protected string _KehindeId;
        protected string _TaiyeId;
        protected TravelMedium _TravelMedium = TravelMedium.driving;
        public virtual DateTimeOffset LastLookup { get; set; }
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
                if(!string.IsNullOrEmpty( value) && !string.IsNullOrWhiteSpace(value))
                {
                    _TravelMedium = Utility.ParseEnum<TravelMedium>(value);
                } else
                {
                    _TravelMedium = TravelMedium.driving;
                }
                
            }
        }
        public Location Taiye {
            get
            {
                return _Taiye;
            } set {
                _Taiye = value;
                _TaiyeId = _Taiye.ThirdPartyId ?? _Taiye.Id;
            }
        }
        public Location Kehinde {
            get
            {
                return _Kehinde;
            }
            set
            {
                _Kehinde = value;
                _KehindeId= _Kehinde.ThirdPartyId ?? _Kehinde.Id;
            }
        }

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

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                string medium = Medium_DB;
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + TaiyeId.GetHashCode();
                hash = hash * 23 + KehindeId.GetHashCode();
                hash = hash * 23 + medium.GetHashCode();
                return hash;
            }
        }

    }
}
