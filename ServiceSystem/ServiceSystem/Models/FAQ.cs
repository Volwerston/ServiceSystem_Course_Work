using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class FAQ
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string Question { get; set; }
        public string Answer
        {
            get; set;
        }
    }
}