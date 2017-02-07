using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class Session: Service
    {
        public List<Day> Days { get; set; }
        public List<TimeMeasure> TimeMeasure { get; set;}
        public List<PaymentMeasure> PaymentMeasure { get; set; }
    }

    public class TimeMeasure
    {
        [JsonConverter(typeof(TimeSpanObjectConverter))]
        public TimeSpan MinDuration { get; set; }
        [JsonConverter(typeof(TimeSpanObjectConverter))]
        public TimeSpan MaxDuration { get; set; }
        public string Description { get; set; }
    }

    class TimeSpanObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jo = new JObject();
            jo["type"] = value.GetType().AssemblyQualifiedName;
            jo["value"] = JToken.FromObject(value, serializer);
            jo.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            Type type = Type.GetType(jo["type"].ToString(), throwOnError: true);
            return jo["value"].ToObject(type, serializer);
        }
    }


    public class PaymentMeasure
    {
        public string Currency { get; set; }
        public string ValueMeasure { get; set; }
        public double PricePerUnit { get; set; }
        public string Description { get; set; }
    }
}