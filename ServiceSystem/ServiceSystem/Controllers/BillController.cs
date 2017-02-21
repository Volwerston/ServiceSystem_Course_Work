using ServiceSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Text;

namespace ServiceSystem.Controllers
{
    public class BillController : ApiController
    {

        public HttpResponseMessage Get(int application_id)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("spGetBillBroadInfo", connection);

                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@Id", application_id);

                Dictionary<string, string> additionalParams = new Dictionary<string, string>();

                DataSet set = new DataSet();

                try
                {
                    da.Fill(set);

                    Bill bill = new Bill();

                    if (set.Tables[0].Rows.Count > 0)
                    {
                        bill.Id = Convert.ToInt32(set.Tables[0].Rows[0]["BillId"].ToString());
                        bill.Price = Convert.ToDouble(set.Tables[0].Rows[0]["BillPrice"].ToString());
                        bill.StatusChangeTime = Convert.ToDateTime(set.Tables[0].Rows[0]["BillStatusChangeTime"].ToString());
                        bill.AdvancePercent = Convert.ToDouble(set.Tables[0].Rows[0]["AdvancePercent"].ToString());
                        bill.CustomerFatherName = set.Tables[0].Rows[0]["CustomerFatherName"].ToString();
                        bill.CustomerLastName = set.Tables[0].Rows[0]["CustomerLastName"].ToString();
                        bill.CustomerFirstName = set.Tables[0].Rows[0]["CustomerFirstName"].ToString();
                        bill.ProviderFatherName = set.Tables[0].Rows[0]["ProviderFatherName"].ToString();
                        bill.ProviderLastName = set.Tables[0].Rows[0]["ProviderLastName"].ToString();
                        bill.ProviderFirstName = set.Tables[0].Rows[0]["ProviderFirstName"].ToString();
                        bill.Currency = set.Tables[0].Rows[0]["BillCurrency"].ToString();
                        bill.AdvanceTimeLimit = Convert.ToDateTime(set.Tables[0].Rows[0]["BillAdvanceDeadline"].ToString());
                        bill.MainTimeLimit = Convert.ToDateTime(set.Tables[0].Rows[0]["BillMainDeadline"].ToString());
                        bill.Type = set.Tables[0].Rows[0]["Type"].ToString();

                        if(set.Tables[0].Rows[0]["Type"].ToString() == "WEBMONEY")
                        {
                            additionalParams.Add("WMPurse", set.Tables[0].Rows[0]["WMPurse"].ToString());
                        }
                        else
                        {
                            additionalParams.Add("Account", set.Tables[0].Rows[0]["Account"].ToString());
                            additionalParams.Add("EDRPOU", set.Tables[0].Rows[0]["EDRPOU"].ToString());
                            additionalParams.Add("MFO", set.Tables[0].Rows[0]["MFO"].ToString());
                        }
                    }
                    else bill = null;

                    if (bill != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new Tuple<Bill, Dictionary<string, string>>(bill, additionalParams));
                    }
                    else
                    {
                        return Request.CreateResponse<Tuple<Bill, Dictionary<string, string>>>(HttpStatusCode.OK, null);
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        public HttpResponseMessage Post([FromBody]Bill bill)
        {
            DateTime changeTime = DateTime.Now;

            WebmoneyBill wmBill = bill as WebmoneyBill;
            BankBill pbBill = bill as BankBill;

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                StringBuilder cmdBuilder = new StringBuilder("");
                cmdBuilder.Append("INSERT INTO Bills VALUES(@AppId, @AdvancePercent, @Price, @StatusChangeTime, @Currency, @AdvanceDeadline, @MainDeadline,@Type, @DetailsId);");
                cmdBuilder.Append("UPDATE Applications SET STATUS='ADVANCE_PENDING' WHERE ID=@AppId;");

                Dictionary<string, object> values = new Dictionary<string, object>();

                bill.StatusChangeTime = changeTime;

                values.Add("@AppId", bill.ApplicationId);
                values.Add("@AdvancePercent", bill.AdvancePercent);
                values.Add("@Price", bill.Price);
                values.Add("@StatusChangeTime", bill.StatusChangeTime);
                values.Add("@Currency", bill.Currency);
                values.Add("@AdvanceDeadline", bill.AdvanceTimeLimit);
                values.Add("@MainDeadline", bill.MainTimeLimit);

                if(wmBill != null)
                {
                    cmdBuilder = cmdBuilder.Replace("@DetailsId", "IDENT_CURRENT('Webmoney_Details') + 1");
                    cmdBuilder.Append("INSERT INTO Webmoney_Details VALUES(@Purse);");

                    values.Add("@Type", "WEBMONEY");
                    values.Add("@Purse", wmBill.WM_Purse);
                }
                else if(pbBill != null)
                {
                    cmdBuilder = cmdBuilder.Replace("@DetailsId", "IDENT_CURRENT('Bank_Details') + 1");
                    cmdBuilder.Append("INSERT INTO Bank_Details VALUES(@Account, @EDRPOU, @MFO);");

                    values.Add("@Type", "BANK");
                    values.Add("@Account", pbBill.Account);
                    values.Add("@EDRPOU", pbBill.EDRPOU);
                    values.Add("@MFO", pbBill.MFO);
                }

                SqlCommand cmd = new SqlCommand(cmdBuilder.ToString(), connection);

                foreach(var kvp in values)
                {
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }

                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;

                try
                {
                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    transaction.Rollback();
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, "OK");
        }
    }
}
