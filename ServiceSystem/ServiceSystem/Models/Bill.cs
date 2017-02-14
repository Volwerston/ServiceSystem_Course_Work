using System;
using System.Web.Mvc;

namespace ServiceSystem.Models
{
    public class Bill
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string Status { get; set; }
        public double AdvancePercent { get; set; }
        public double Price { get; set; }
        public DateTime StatusChangeTime { get; set; }
        public DateTime PaymentDeadline { get; set; }
        public string Currency { get; set; }

        public string ProviderFirstName { get; set; }
        public string ProviderLastName { get; set; }
        public string ProviderFatherName { get; set; }

        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerFatherName { get; set; }

        public static Bill GenerateBill(FormCollection collection)
        {
            Bill toReturn = new Bill();

            toReturn.AdvancePercent = Convert.ToDouble(collection["advance_percent"]);
            toReturn.PaymentDeadline = Convert.ToDateTime(collection["date_limit"]);

            if(collection["has_time_limit"] == "on")
            {
                toReturn.PaymentDeadline = toReturn.PaymentDeadline.AddHours(
                    double.Parse(collection["time_limit"].Split(':')[0])
                    );

                toReturn.PaymentDeadline = toReturn.PaymentDeadline.AddMinutes(
                    double.Parse(collection["time_limit"].Split(':')[1])
                    );
            }

            toReturn.Currency = collection["price_measure"].Split('_')[0];
            toReturn.Price = Convert.ToDouble(collection["price"]);
            toReturn.Status = "ADVANCE_PENGING";
            toReturn.StatusChangeTime = DateTime.Now;
            toReturn.ApplicationId = Convert.ToInt32(collection["application_id"]);

            return toReturn;
        }
    }
}