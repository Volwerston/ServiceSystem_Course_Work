using System;
using System.Web.Mvc;

namespace ServiceSystem.Models
{

    public enum BillType
    {
        ADVANCE,
        MAIN
    }

    public static class BillGenerator
    {

        public static Bill GenerateBill(FormCollection collection)
        {
            Bill toReturn = null;

            switch(collection["payment_selector"])
            {
                case "wm":
                    toReturn = GenerateWebmoneyBill(collection);
                    break;
                case "privat":
                    toReturn = GeneratePrivatBankBill(collection);
                    break;
            }

            toReturn.AdvancePercent = Convert.ToDouble(collection["advance_percent"]);
            toReturn.AdvanceTimeLimit = Convert.ToDateTime(collection["date_limit_advance"]);
            toReturn.MainTimeLimit = Convert.ToDateTime(collection["date_limit_main"]);

            if (collection["has_time_limit"] == "on")
            {
                toReturn.AdvanceTimeLimit = toReturn.AdvanceTimeLimit.AddHours(
                    double.Parse(collection["time_limit_advance"].Split(':')[0])
                    );

                toReturn.AdvanceTimeLimit = toReturn.AdvanceTimeLimit.AddMinutes(
                    double.Parse(collection["time_limit_advance"].Split(':')[1])
                    );

                toReturn.MainTimeLimit = toReturn.MainTimeLimit.AddHours(
                    double.Parse(collection["time_limit_main"].Split(':')[0])
                    );

                toReturn.MainTimeLimit = toReturn.MainTimeLimit.AddMinutes(
                    double.Parse(collection["time_limit_main"].Split(':')[1])
                    );
            }

            toReturn.Currency = collection["price_measure"].Split('_')[0];
            toReturn.Price = Convert.ToDouble(collection["price"]);
            toReturn.StatusChangeTime = DateTime.Now;
            toReturn.ApplicationId = Convert.ToInt32(collection["application_id"]);

            return toReturn;
        }

        private static Bill GeneratePrivatBankBill(FormCollection collection)
        {
            BankBill bill = new BankBill();

            bill.Account = collection["account"];
            bill.EDRPOU = collection["edrpou"];
            bill.MFO = collection["mfo"];

            return bill;
        }

        private static Bill GenerateWebmoneyBill(FormCollection collection)
        {
            WebmoneyBill bill = new WebmoneyBill();

            bill.WM_Purse = collection["wm_purse"];

            return bill;
        }
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