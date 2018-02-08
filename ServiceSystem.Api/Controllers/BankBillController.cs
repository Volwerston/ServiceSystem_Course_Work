﻿using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceSystem.Controllers
{
    [Authorize]
    public class BankBillController : ApiController
    {
        [ActionName("ConfirmPayment")]
        public HttpResponseMessage Post([FromBody] int? application_id)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("spModifyBills", connection);

                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                try
                {
                    cmd.Parameters.AddWithValue("@Id", application_id);

                    connection.Open();
                    cmd.ExecuteNonQuery();

                    return Request.CreateResponse(HttpStatusCode.OK, "OK");
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }
    }
}
