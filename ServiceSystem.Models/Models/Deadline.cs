using System.Collections.Generic;

namespace ServiceSystem.Models
{
    public class Deadline: Service
    {
        public List<TimeMeasure> TimeMeasures { get; set; }
        public List<PaymentMeasure> PaymentMeasures { get; set; }
    }
}