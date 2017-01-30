using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class DeadlineApplication : Application
    {
        public bool IsByLastDate { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}