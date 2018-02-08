using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceSystem.Controllers
{
    public class WMData
    {
        public int LMI_PAYMENT_NO { get; set; }
        public ulong LMI_SYS_TRANS_NO { get; set; }
        public string LMI_SYS_TRANS_DATE { get; set; }
    }

    public class WebmoneyController : ApiController
    {
        [ActionName("PostTransactionData")]
        public HttpResponseMessage Post([FromBody] WMData wmData)
        {
            if (wmData != null && wmData.LMI_SYS_TRANS_NO != 0)
            {
                using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("spModifyBills", connection);

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    try
                    {
                        cmd.Parameters.AddWithValue("@Id", wmData.LMI_PAYMENT_NO);

                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch(Exception ex)
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }
            }

                return Request.CreateResponse(HttpStatusCode.OK, "OK");
        }
    }
}
