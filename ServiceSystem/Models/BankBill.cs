using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class BankBill : Bill
    {
        public string Account { get; set; }
        public string EDRPOU { get; set; }
        public string MFO { get; set; }
    }
}