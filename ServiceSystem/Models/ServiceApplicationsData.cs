using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class ServiceApplicationsData
    {
        public Dictionary<string, string> Consultants { get; set; }
        public int NoBillApplications { get; set; }
        public int AdvancePendingApplications { get; set; }
        public int MainPendingApplications { get; set; }
        public int MainPaidApplications { get; set; }
    }
}