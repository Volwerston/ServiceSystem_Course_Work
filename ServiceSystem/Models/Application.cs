using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using ServiceSystem.Controllers;

namespace ServiceSystem.Models
{
    public class Application
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string Description { get; set; }
        public string ServiceType { get; set; }
        public string Status { get; set; }
        public DateTime StatusChangeDate { get; set; }
        public string ServiceName { get; set; }
        public string Username { get; set; }
        public string ConsultantName { get; set; }
        public int? Mark { get; set; }
        public string FinalEstimate { get; set; }

        public int DialogueId { get; set; }
    }
}