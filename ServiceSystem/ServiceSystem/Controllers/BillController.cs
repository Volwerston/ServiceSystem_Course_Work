using ServiceSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;

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
                        bill.PaymentDeadline = Convert.ToDateTime(set.Tables[0].Rows[0]["BillPaymentDeadline"].ToString());
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, bill);
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

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "INSERT INTO Bills VALUES(@AppId, @Status, @AdvancePercent, @Price, @StatusChangeTime, @Currency, @NextDeadline);";

                SqlCommand cmd = new SqlCommand(cmdString, connection);

                bill.StatusChangeTime = changeTime;

                cmd.Parameters.AddWithValue("@AppId", bill.ApplicationId);
                cmd.Parameters.AddWithValue("@Status", bill.Status);
                cmd.Parameters.AddWithValue("@AdvancePercent", bill.AdvancePercent);
                cmd.Parameters.AddWithValue("@Price", bill.Price);
                cmd.Parameters.AddWithValue("@StatusChangeTime", bill.StatusChangeTime);
                cmd.Parameters.AddWithValue("@Currency", bill.Currency);
                cmd.Parameters.AddWithValue("@NextDeadline", bill.PaymentDeadline);

                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
                catch(Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, "OK");
        }
    }
}
