﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        protected TilerUser _User;

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
        [Index("UserIdAndLocationCacheEntry", Order = 1, IsUnique = true)]
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
                    _TaiyeId = string.IsNullOrEmpty(_Taiye.ThirdPartyId) || string.IsNullOrWhiteSpace(_Taiye.ThirdPartyId) ? _Taiye.Id : _Taiye.ThirdPartyId;
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
                    _KehindeId = string.IsNullOrEmpty(_Kehinde.ThirdPartyId) || string.IsNullOrWhiteSpace(_Kehinde.ThirdPartyId) ? _Kehinde.Id : _Kehinde.ThirdPartyId;
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

        [Index("UserIdAndLocationCacheEntry", Order = 0, IsUnique = true)]
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

        public virtual TimeSpan TimeSpan
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
                var retValue = LocationCacheEntry.getHashCode(TaiyeId, KehindeId, Medium_DB, this.TravelCache.Id);
                if(retValue.Item1)
                {
                    return retValue.Item2;
                } else
                {
                    throw new Exception("invalid location");
                }
            }
        }

        private static Tuple<bool, int> getHashCode (string TaiyeId, string KehindeId, string medium, string travelCahceId)
        {
            unchecked
            {
                bool isValid = true;
                if(!string.IsNullOrEmpty(TaiyeId) && !string.IsNullOrWhiteSpace(KehindeId))
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

                    return new Tuple<bool, int>(isValid, hash);
                }
                else
                {
                    isValid = false;
                    return new Tuple<bool, int>(isValid, -1);
                }
            }
        }

        public static Tuple<bool, int> getHashCode(Location firstLocation, Location secondLocation, string medium, string travelCahceId) {
            string firstId = string.IsNullOrEmpty(firstLocation.ThirdPartyId)|| string.IsNullOrWhiteSpace(firstLocation.ThirdPartyId) ? firstLocation.Id : firstLocation.ThirdPartyId;
            string secondId = string.IsNullOrEmpty(secondLocation.ThirdPartyId) || string.IsNullOrWhiteSpace(secondLocation.ThirdPartyId) ? secondLocation.Id : secondLocation.ThirdPartyId;

            return LocationCacheEntry.getHashCode(firstId, secondId, medium, travelCahceId);
        }

    }
}
