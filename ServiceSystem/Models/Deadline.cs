using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class Deadline: Service
    {
        public List<TimeMeasure> TimeMeasures { get; set; }
        public List<PaymentMeasure> PaymentMeasures { get; set; }
    }
}