using Newtonsoft.Json;
using ServiceSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceSystem.Controllers
{
    public class FAQController : ApiController
    {

        public HttpResponseMessage Delete([FromUri]int id)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "DELETE FROM FAQs WHERE ID=@Id";

                SqlCommand cmd = new SqlCommand(cmdString, connection);

                cmd.Parameters.AddWithValue("@Id", id);

                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();

                    return Request.CreateResponse(HttpStatusCode.OK, "OK");
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }
        public HttpResponseMessage Post([FromBody]FAQ faq)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "INSERT INTO FAQs VALUES(@Question, @Answer, @ServiceId); SELECT IDENT_CURRENT('FAQs');";

                SqlCommand cmd = new SqlCommand(cmdString, connection);

                cmd.Parameters.AddWithValue("@ServiceId", faq.ServiceId);
                cmd.Parameters.AddWithValue("@Question", faq.Question);
                cmd.Parameters.AddWithValue("@Answer", faq.Answer);

                try
                {
                    connection.Open();
                    var toReturn = cmd.ExecuteScalar();

                    return Request.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(toReturn));
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        public HttpResponseMessage Get([FromUri] int service_id)
        {
            List<FAQ> toReturn = new List<FAQ>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "SELECT ID, SERVICE_ID, ANSWER, QUESTION FROM FAQs WHERE SERVICE_ID=@Id";

                SqlDataAdapter da = new SqlDataAdapter(cmdString, connection);

                da.SelectCommand.Parameters.AddWithValue("@Id", service_id);

                DataSet set = new DataSet();

                try
                {
                    da.Fill(set);

                    if (set.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in set.Tables[0].Rows)
                        {
                            FAQ faq = new FAQ();

                            faq.Id = Convert.ToInt32(row["ID"].ToString());
                            faq.ServiceId = Convert.ToInt32(row["SERVICE_ID"].ToString());
                            faq.Answer = row["ANSWER"].ToString();
                            faq.Question = row["QUESTION"].ToString();

                            toReturn.Add(faq);
                        }
                    }
                    else
                    {
                        toReturn = null;
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, toReturn);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }
    }
}