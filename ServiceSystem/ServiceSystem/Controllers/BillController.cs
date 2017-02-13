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
        public HttpResponseMessage Post([FromBody]Bill bill)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "INSERT INTO Bills VALUES(@AppId, @Status, @AdvancePercent, @Price, @StatusChangeTime, @Currency, @NextDeadline)";

                SqlCommand cmd = new SqlCommand(cmdString, connection);

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
