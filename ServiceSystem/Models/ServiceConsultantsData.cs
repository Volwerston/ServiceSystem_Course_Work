﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class ServiceConsultantsData
    {
        public int NoBillApplications { get; set; }
        public int AdvancePendingApplications { get; set; }
        public int MainPendingApplications { get; set; }
        public int MainPaidApplications { get; set; }
        public double AverageMark { get; set; }
    }
}