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
        public TimeSpan MinDuration { get; set; }
        public TimeSpan MaxDuration { get; set; }
        public string Description { get; set; }
    }

    public class PaymentMeasure
    {
        public string Currency { get; set; }
        public string ValueMeasure { get; set; }
        public double PricePerUnit { get; set; }
    }
}