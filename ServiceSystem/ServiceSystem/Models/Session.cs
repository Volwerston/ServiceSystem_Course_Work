using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class Session: Service
    {
        public List<Day> Days { get; set; }
        public TimeSpan SessionDuration { get; set; }
        public double SessionPrice { get; set; }
    }
}