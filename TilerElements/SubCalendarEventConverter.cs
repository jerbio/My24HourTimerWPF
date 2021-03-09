using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class SubCalendarEventConverter : JsonConverter<SubCalendarEvent>
    {
        public override void WriteJson(JsonWriter writer, SubCalendarEvent value, JsonSerializer serializer)
        {

            JObject o = new JObject();
            o.Add("id", value.Id);
            o.Add("start", value.Start.ToUnixTimeMilliseconds());
            o.Add("end", value.End.ToUnixTimeMilliseconds());
            o.Add("name", value.getName?.NameValue);
            o.Add("travelTimeBefore", value.TravelTimeBefore.TotalMilliseconds);
            o.Add("travelTimeAfter", value.TravelTimeAfter.TotalMilliseconds);
            o.Add("address", value.Location?.Address);
            o.Add("addressDescription", value.Location?.Description);
            o.Add("rangeStart", value.CalendarEventRangeStart.ToUnixTimeMilliseconds());
            o.Add("rangeEnd", value.CalendarEventRangeStart.ToUnixTimeMilliseconds());
            o.Add("thirdpartyType", value.ThirdpartyType.ToString());
            o.Add("colorOpacity", value.getUIParam?.UIColor?.O);
            o.Add("colorRed", value.getUIParam?.UIColor?.R);
            o.Add("colorGreen", value.getUIParam?.UIColor?.G);
            o.Add("colorBlue", value.getUIParam?.UIColor?.B);
            o.Add("isPaused", value.isPauseLocked);
            o.Add("isComplete", value.getIsComplete);
            o.Add("isRecurring", value.IsFromRecurring);
            o.WriteTo(writer);
        }


        public override SubCalendarEvent ReadJson(JsonReader reader, Type objectType, SubCalendarEvent existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            SubCalendarEvent retValue = SubCalendarEvent.getEmptySubCalendarEvent(EventID.GenerateCalendarEvent());
            return retValue;
        }
    }
}
