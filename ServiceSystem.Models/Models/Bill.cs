using System;

namespace ServiceSystem.Models
{

    public enum BillType
    {
        ADVANCE,
        MAIN
    }
    public class Bill
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public double AdvancePercent { get; set; }
        public double Price { get; set; }
        public DateTime StatusChangeTime { get; set; }
        public DateTime AdvanceTimeLimit { get; set; }
        public DateTime MainTimeLimit { get; set; }
        public string Currency { get; set; }

        public string ProviderFirstName { get; set; }
        public string ProviderLastName { get; set; }
        public string ProviderFatherName { get; set; }

        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerFatherName { get; set; }

        public string Type { get; set; }
    }
}