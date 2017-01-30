using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class DefinedCourse: Service
    {
        public List<Day> Days { get; set; }
        public bool ParticipantsLimited { get; set; }
        public int ParticipantsNumber { get; set; }
        public double Price { get; set; }
    }
}