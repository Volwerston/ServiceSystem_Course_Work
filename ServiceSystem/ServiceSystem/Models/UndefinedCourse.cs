using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class UndefinedCourse: Service
    {
        public int NumOfDays { get; set; }
        public List<int> DurationsPerDay { get; set; }
        public double Price { get; set; }
    }
}