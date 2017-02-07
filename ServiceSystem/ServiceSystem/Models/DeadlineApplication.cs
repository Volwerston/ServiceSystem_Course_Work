using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class DeadlineApplication : Application
    {
        public int DetailsId { get; set; }
        public bool HasLastDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [JsonConverter(typeof(TimeSpanObjectConverter))]
        public TimeSpan Duration { get; set; }
    }
}