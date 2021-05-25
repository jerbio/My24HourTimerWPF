using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    class LocationConverter : JsonConverter<Location>
    {
        public override void WriteJson(JsonWriter writer, Location value, JsonSerializer serializer)
        {

            JObject o = new JObject();
            o.Add("id", value.Id);
            o.Add("description", value.Description);
            o.Add("address", value.Address);
            o.Add("longitude", value.Longitude);
            o.Add("latitude", value.Latitude);
            o.Add("isVerified", value.IsVerified);
            o.Add("thirdPartyId", value.ThirdPartyId);
            o.Add("userId", value.UserId);
            o.WriteTo(writer);
        }

        public override Location ReadJson(JsonReader reader, Type objectType, Location existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Location retValue = new Location();
            return retValue;
        }
    }
}
